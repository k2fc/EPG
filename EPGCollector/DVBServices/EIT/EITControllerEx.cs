////////////////////////////////////////////////////////////////////////////////// 
//                                                                              //
//      Copyright (C) 2005-2016 nzsjb                                           //
//                                                                              //
//  This Program is free software; you can redistribute it and/or modify        //
//  it under the terms of the GNU General Public License as published by        //
//  the Free Software Foundation; either version 2, or (at your option)         //
//  any later version.                                                          //
//                                                                              //
//  This Program is distributed in the hope that it will be useful,             //
//  but WITHOUT ANY WARRANTY; without even the implied warranty of              //
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the                //
//  GNU General Public License for more details.                                //
//                                                                              //
//  You should have received a copy of the GNU General Public License           //
//  along with GNU Make; see the file COPYING.  If not, write to                //
//  the Free Software Foundation, 675 Mass Ave, Cambridge, MA 02139, USA.       //
//  http://www.gnu.org/copyleft/gpl.html                                        //
//                                                                              //  
//////////////////////////////////////////////////////////////////////////////////

using System;
using System.Threading;
using System.ComponentModel;
using System.IO;
using System.Text;

using DirectShow;
using DomainObjects;

namespace DVBServices
{
    /// <summary>
    /// The class that controls the acquisition and processing of EIT data using a blocking queue.
    /// </summary>
    public class EITControllerEx : ControllerBase
    {
        /// <summary>
        /// Get the collection type supported by this collector.
        /// </summary>
        public override CollectionType CollectionType { get { return (CollectionType.EIT); } }
        /// <summary>
        /// Return true if the EIT data is complete; false otherwise.
        /// </summary>
        public override bool AllDataProcessed { get { return (eitSectionsDone);  } }

        private TSStreamReaderEx eitReader;
        private bool eitSectionsDone = false;
        private int eitChannels;
        private int openTVChannels;
        
        /// <summary>
        /// Initialize a new instance of the EITController class.
        /// </summary>
        public EITControllerEx() { }

        /// <summary>
        /// Stop acquiring and processing EIT data.
        /// </summary>
        public override void Stop()
        {
            Logger.Instance.Write("Stopping section readers");

            if (NetworkReader != null)
                NetworkReader.Stop();

            if (BouquetReader != null)
                BouquetReader.Stop();

            if (eitReader != null)
                eitReader.Stop();

            Logger.Instance.Write("Stopped section readers");
        }

        /// <summary>
        /// Acquire and process EIT data.
        /// </summary>
        /// <param name="dataProvider">A sample data provider.</param>
        /// <param name="worker">The background worker that is running this collection.</param>
        /// <returns>A CollectorReply code.</returns>
        public override CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            return (Process(dataProvider, worker, CollectionSpan.AllData));
        }

        /// <summary>
        /// Acquire and process EIT data.
        /// </summary>
        /// <param name="dataProvider">A sample data provider.</param>
        /// <param name="worker">The background worker that is running this collection.</param>
        /// <param name="collectionSpan">The amount of data to be collected.</param>
        /// <returns>A CollectorReply code.</returns>
        public override CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker, CollectionSpan collectionSpan)
        {
            if (collectionSpan == CollectionSpan.AllData)
            {
                EITProgramContent.Load();
                CustomProgramCategory.Load();
                ParentalRating.Load();

                if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options,
                    OptionName.UseFreeSatTables))
                    MultiTreeDictionaryEntry.Load(Path.Combine(RunParameters.ConfigDirectory, "Huffman Dictionary Freesat T1.cfg"), Path.Combine(RunParameters.ConfigDirectory, "Huffman Dictionary Freesat T2.cfg"));

                if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ByteConvertTable != null)
                    ByteConvertFile.Load();
            }

            if (RunParameters.Instance.NetworkDataNeeded)
            {
                GetNetworkInformation(dataProvider, worker);
                if (worker.CancellationPending)
                    return (CollectorReply.Cancelled);
            }

            GetStationData(dataProvider, worker);
            if (worker.CancellationPending)
                return (CollectorReply.Cancelled);

            if (RunParameters.Instance.StationCollection.Count == 0 && 
                !OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options, OptionName.CreateMissingChannels))
            {
                Logger.Instance.Write("<e> No stations located - data collection abandoned");
                return (CollectorReply.OK);
            }

            if (RunParameters.Instance.ChannelDataNeeded)
            {
                GetBouquetSections(dataProvider, worker);

                string bouquetType;

                if (eitChannels > 0)
                    bouquetType = "Freeview";
                else
                    bouquetType = "OpenTV";

                Logger.Instance.Write("Used " + bouquetType + " channel descriptors");

                if (worker.CancellationPending)
                    return (CollectorReply.Cancelled);
            }

            if (collectionSpan == CollectionSpan.ChannelsOnly)
                return (CollectorReply.OK);

            getEITSections(dataProvider, worker);

            return (CollectorReply.OK);
        }

        /// <summary>
        /// Process the bouquet data.
        /// </summary>
        protected override void ProcessBouquetSections()
        {
            foreach (BouquetAssociationSection bouquetSection in BouquetAssociationSection.BouquetAssociationSections)
            {
                if (bouquetSection.TransportStreams != null)
                {
                    foreach (TransportStream transportStream in bouquetSection.TransportStreams)
                    {
                        if (transportStream.Descriptors != null)
                        {
                            foreach (DescriptorBase descriptor in transportStream.Descriptors)
                            {
                                FreeviewChannelInfoDescriptor freeviewInfoDescriptor = descriptor as FreeviewChannelInfoDescriptor;
                                if (freeviewInfoDescriptor != null)
                                    processFreeviewInfoDescriptor(freeviewInfoDescriptor, transportStream.OriginalNetworkID, transportStream.TransportStreamID, bouquetSection.BouquetID);
                                else
                                {
                                    OpenTVChannelInfoDescriptor openTVInfoDescriptor = descriptor as OpenTVChannelInfoDescriptor;
                                    if (openTVInfoDescriptor != null)
                                        processOpenTVInfoDescriptor(openTVInfoDescriptor, transportStream.OriginalNetworkID, transportStream.TransportStreamID, bouquetSection.BouquetID);                                                                                
                                }
                            }
                        }
                    }
                }
            }
        }

        private void processFreeviewInfoDescriptor(FreeviewChannelInfoDescriptor freeviewInfoDescriptor, int originalNetworkID, int transportStreamID, int bouquetID)
        {
            if (freeviewInfoDescriptor.ChannelInfoEntries == null)
                return;

            if (openTVChannels != 0)
                return;

            foreach (FreeviewChannelInfoEntry channelInfoEntry in freeviewInfoDescriptor.ChannelInfoEntries)
            {
                EITChannel channel = new EITChannel();
                channel.OriginalNetworkID = originalNetworkID;
                channel.TransportStreamID = transportStreamID;
                channel.ServiceID = channelInfoEntry.ServiceID;
                channel.UserChannel = channelInfoEntry.UserNumber;
                channel.Flags = channelInfoEntry.Flags;
                channel.BouquetID = bouquetID;
                EITChannel.AddChannel(channel);
                
                eitChannels++;

                Bouquet bouquet = Bouquet.FindBouquet(channel.BouquetID);
                if (bouquet == null)
                {
                    bouquet = new Bouquet(channel.BouquetID, BouquetAssociationSection.FindBouquetName(channel.BouquetID));
                    Bouquet.AddBouquet(bouquet);
                }
                
                Region region = bouquet.FindRegion(channel.Region);
                if (region == null)
                {
                    region = new Region(string.Empty, channel.Region);
                    bouquet.AddRegion(region);
                }

                region.AddChannel(channel);
            }
        }

        private void processOpenTVInfoDescriptor(OpenTVChannelInfoDescriptor openTVInfoDescriptor, int originalNetworkID, int transportStreamID, int bouquetID)
        {
            if (openTVInfoDescriptor.ChannelInfoEntries == null)
                return;

            if (eitChannels != 0)
            {
                OpenTVChannel.Channels.Clear();
                eitChannels = 0;
                return;
            }

            foreach (OpenTVChannelInfoEntry channelInfoEntry in openTVInfoDescriptor.ChannelInfoEntries)
            {
                OpenTVChannel channel = new OpenTVChannel();
                channel.OriginalNetworkID = originalNetworkID;
                channel.TransportStreamID = transportStreamID;
                channel.ServiceID = channelInfoEntry.ServiceID;
                channel.ChannelID = channelInfoEntry.ChannelID;
                channel.UserChannel = channelInfoEntry.UserNumber;
                channel.Type = channelInfoEntry.Type;
                channel.Flags = channelInfoEntry.Flags;
                channel.BouquetID = bouquetID;
                channel.Region = openTVInfoDescriptor.Region;
                OpenTVChannel.AddChannel(channel);

                openTVChannels++;

                Bouquet bouquet = Bouquet.FindBouquet(channel.BouquetID);
                if (bouquet == null)
                {
                    bouquet = new Bouquet(channel.BouquetID, BouquetAssociationSection.FindBouquetName(channel.BouquetID));
                    Bouquet.AddBouquet(bouquet);
                }

                Region region = bouquet.FindRegion(channel.Region);
                if (region == null)
                {
                    region = new Region(string.Empty, channel.Region);
                    bouquet.AddRegion(region);
                }

                region.AddChannel(channel); 
            }
        }

        private void getEITSections(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            Logger.Instance.Write("Collecting EIT data", false, true);

            int actualPid;
            if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.EITPid == -1)
                actualPid = BDAGraph.EitPid;
            else
                actualPid = RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.EITPid;

            Logger.Instance.Write("Collecting EIT data from pid 0x" + actualPid.ToString("x"));

            dataProvider.ChangePidMapping(new int[] { actualPid });            

            eitReader = new TSStreamReaderEx(2000, dataProvider.BufferAddress); 
            eitReader.Run();

            int lastCount = 0;
            int repeats = 0;
            int bufferFill = 1;
            int totalBufferUsed = 0;

            while (!eitSectionsDone)
            {
                if (worker.CancellationPending)
                    return;

                Mpeg2Section section = eitReader.Sections.Take();
                eitSectionsDone = section.PID == int.MaxValue;
                if (!eitSectionsDone)
                {
                    processSection(section);
                    LogProgress("EIT data", eitReader.SectionCount, 1000);

                    if (lastCount == TVStation.TotalEpgCount)
                    {
                        repeats++;
                        eitSectionsDone = (repeats == RunParameters.Instance.Repeats * 1000);

                        if (eitSectionsDone)
                        {
                            totalBufferUsed += dataProvider.BufferSpaceUsed;

                            if (RunParameters.Instance.BufferFills != 1)
                            {
                                Logger.Instance.Write("", true, false);
                                Logger.Instance.Write("Buffer scan " + bufferFill + " of " + RunParameters.Instance.BufferFills +
                                    " finished:" +
                                    " buffer space used " + dataProvider.BufferSpaceUsed);

                                eitSectionsDone = (bufferFill == RunParameters.Instance.BufferFills);

                                if (!eitSectionsDone)
                                {
                                    eitReader.Stop();

                                    dataProvider.ChangePidMapping(actualPid);

                                    eitReader = new TSStreamReaderEx(2000, dataProvider.BufferAddress);
                                    eitReader.Run();

                                    repeats = 0;
                                    bufferFill++;

                                    Logger.Instance.Write("Buffer scan " + bufferFill + " of " + RunParameters.Instance.BufferFills + " starting");

                                    Thread.Sleep(2000);
                                }
                            }
                        }
                    }
                    else
                        repeats = 0;

                    lastCount = TVStation.TotalEpgCount;
                }
            }

            Logger.Instance.Write("", true, false);
            Logger.Instance.Write("Stopping reader");
            eitReader.Stop();

            Logger.Instance.Write("EPG count: " + TVStation.TotalEpgCount + 
                " buffer space used: " + totalBufferUsed + 
                " discontinuities: " + eitReader.Discontinuities);            
        }

        private void processSection(Mpeg2Section section)
        {
            if (DebugEntry.IsDefined(DebugName.DumpEitSections))
                Logger.Instance.Dump("EIT Section", section.Data, section.Data.Length);

            if (section.Table >= 0x4e && section.Table <= 0x6f)
            {
                try
                {
                    Mpeg2ExtendedHeader mpeg2Header = new Mpeg2ExtendedHeader();
                    mpeg2Header.Process(section.Data);
                    if (mpeg2Header.Current)
                    {
                        EITSection eitSection = new EITSection();
                        eitSection.Process(section.Data, mpeg2Header);
                        eitSection.LogMessage();
                    }
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Logger.Instance.Write("<e> EIT error: " + e.Message);
                }
            }
        }

        /// <summary>
        /// Create the EPG entries.
        /// </summary>
        public override void FinishFrequency() 
        {
            if (!OptionEntry.IsDefined(OptionName.TcRelevantOnly))
            {
                if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ChannelBouquet != -1)
                {
                    foreach (TVStation tvStation in RunParameters.Instance.StationCollection)
                    {
                        bool process = checkChannelMapping(tvStation);
                        if (!process)
                            tvStation.EPGCollection.Clear();
                    }
                }
                else
                {
                    if (Bouquet.Bouquets.Count != 0)
                    {
                        foreach (Bouquet bouquet in Bouquet.Bouquets)
                        {
                            foreach (Region region in bouquet.Regions)
                            {
                                foreach (Channel channel in region.Channels)
                                {
                                    TVStation station = TVStation.FindStation(RunParameters.Instance.StationCollection,
                                        channel.OriginalNetworkID, channel.TransportStreamID, channel.ServiceID);
                                    if (station != null && station.LogicalChannelNumber == -1)
                                        station.LogicalChannelNumber = channel.UserChannel;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (TVStation tvStation in RunParameters.Instance.StationCollection)
                            checkNetworkMap(tvStation);
                    }
                }
            }
            else
            {
                foreach (TVStation tvStation in RunParameters.Instance.StationCollection)
                {
                    bool process = checkNetworkMap(tvStation);
                    if (!process)
                        tvStation.EPGCollection.Clear();
                }

            }

            if (Utils.FormattingBytes != null)
            {
                StringBuilder formatString = new StringBuilder();

                foreach (byte formatByte in Utils.FormattingBytes)
                {
                    if (formatString.Length == 0)
                        formatString.Append("Control codes used in text: ");
                    else
                        formatString.Append(", ");

                    string hexFormat = formatByte.ToString("X");
                    
                    if (hexFormat.Length != 1)
                        formatString.Append("0x" + hexFormat);
                    else
                        formatString.Append("0x0" + hexFormat);
                }

                Logger.Instance.Write(formatString.ToString());
            }

            EITProgramContent.LogContentUsage();
            LanguageCode.LogUsage();
            CharacterSet.LogUsage();
            logChannelInfo();
        }

        private bool checkChannelMapping(TVStation tvStation)
        {
            Bouquet bouquet = Bouquet.FindBouquet(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ChannelBouquet);
            if (bouquet == null)
                return (false);

            Region region = bouquet.FindRegion(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ChannelRegion != -1 ? RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ChannelRegion : 0);
            if (region == null)
                return (false);

            Channel channel = region.FindChannel(tvStation.OriginalNetworkID, tvStation.TransportStreamID, tvStation.ServiceID);
            if (channel == null)
                return (false);

            if (tvStation.LogicalChannelNumber != -1)
                return (true);

            tvStation.LogicalChannelNumber = channel.UserChannel;

            return (true);
        }

        private bool checkNetworkMap(TVStation tvStation)
        {
            TuningFrequency tuningFrequency = NetworkMap.FindFrequency(tvStation.OriginalNetworkID, tvStation.TransportStreamID);
            if (tuningFrequency == null)
                return (false);

            TerrestrialFrequency terrestrialFrequency = tuningFrequency as TerrestrialFrequency;
            if (terrestrialFrequency == null)
            {
                CableFrequency cableFrequency = tuningFrequency as CableFrequency;
                if (cableFrequency == null)
                    return (false);
            }

            if (tvStation.LogicalChannelNumber != -1)
                return (true);

            Channel channel = Channel.FindChannel(tvStation.OriginalNetworkID, tvStation.TransportStreamID, tvStation.ServiceID);
            if (channel != null)
                tvStation.LogicalChannelNumber = channel.UserChannel;

            return (true);
        }

        private void logChannelInfo()
        {
            if (!DebugEntry.IsDefined(DebugName.LogChannels))
                return;
            if (!RunParameters.Instance.ChannelDataNeeded)
                return;

            Logger.Instance.WriteSeparator("Bouquet Usage");

            if (Bouquet.Bouquets != null && Bouquet.Bouquets.Count != 0)
            {
                bool firstBouquet = true;

                foreach (Bouquet bouquet in Bouquet.GetBouquetsInNameOrder())
                {
                    if (!firstBouquet)
                        Logger.Instance.Write("");

                    firstBouquet = false;

                    if (!firstBouquet)
                        Logger.Instance.Write("");

                    firstBouquet = false;

                    foreach (Region region in bouquet.Regions)
                    {
                        Logger.Instance.Write("Bouquet: " + bouquet.BouquetID + " - " + bouquet.Name + " Region: " + region.Code + " (channels = " + region.Channels.Count + ")");

                        foreach (Channel channel in region.GetChannelsInNameOrder())
                            Logger.Instance.Write("    Channel: " + channel.ToString());
                    }
                }
            }
            else
            {
                Logger.Instance.Write("No bouquets available - channel data only");

                foreach (Channel channel in Channel.GetChannelsInUserNumberOrder())
                    Logger.Instance.Write("Channel: " + channel.ToString());
            }
        }
    }
}
