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
using System.Collections.ObjectModel;
using System.Threading;
using System.ComponentModel;
using System.IO;

using DirectShow;
using DomainObjects;

namespace DVBServices
{
    /// <summary>
    /// The class that controls the acquisition and processing of FreeSat data.
    /// </summary>   
    public class FreeSatController : ControllerBase
    {
        /// <summary>
        /// Get the collection type supported by this collector.
        /// </summary>
        public override CollectionType CollectionType { get { return (CollectionType.FreeSat); } }
        /// <summary>
        /// Return true if all data has been processed; false otherwise.
        /// </summary>
        public override bool AllDataProcessed { get { return (true); } }

        private TSStreamReader freeSatReader;
        private bool freeSatSectionsDone = false;        

        /// <summary>
        /// Initialize a new instance of the FreeSatController class.
        /// </summary>
        public FreeSatController() { }

        /// <summary>
        /// Stop acquiring and processing data.
        /// </summary>
        public override void Stop()
        {
            Logger.Instance.Write("Stopping section readers");

            if (NetworkReader != null)
                NetworkReader.Stop();

            if (BouquetReader != null)
                BouquetReader.Stop();

            if (freeSatReader != null)
                freeSatReader.Stop();

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
        /// <param name="collectionSpan">The amount of data to collect.</param>
        /// <returns>A CollectorReply code.</returns>
        public override CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker, CollectionSpan collectionSpan)
        {
            if (collectionSpan == CollectionSpan.AllData)
            {
                EITProgramContent.Load();
                CustomProgramCategory.Load();

                MultiTreeDictionaryEntry.Load(Path.Combine(RunParameters.ConfigDirectory, "Huffman Dictionary Freesat T1.cfg"), Path.Combine(RunParameters.ConfigDirectory, "Huffman Dictionary Freesat T2.cfg"));
            }

            if (RunParameters.Instance.NetworkDataNeeded)
            {
                GetNetworkInformation(dataProvider, worker, new int[] { 0xbb9 });
                if (worker.CancellationPending)
                    return (CollectorReply.Cancelled);
            }

            GetStationData(dataProvider, worker, new int[] { 0xbba });
            if (worker.CancellationPending)
                return (CollectorReply.Cancelled);

            if (RunParameters.Instance.StationCollection.Count == 0)
            {
                Logger.Instance.Write("<e> No stations located - data collection abandoned");
                return (CollectorReply.OK);
            }

            if (RunParameters.Instance.ChannelDataNeeded)
            {
                GetBouquetSections(dataProvider, worker, new int[] { 0xbba });
                if (worker.CancellationPending)
                    return (CollectorReply.Cancelled);                
            }

            if (collectionSpan == CollectionSpan.ChannelsOnly)
                return (CollectorReply.OK);

            getFreeSatSections(dataProvider, worker);
            if (worker.CancellationPending)
                return (CollectorReply.Cancelled);

            if (DebugEntry.IsDefined(DebugName.GetOtherSections))
                getOtherSections(dataProvider, worker);

            return (CollectorReply.OK);
        }

        /// <summary>
        /// Process the bouquet data.
        /// </summary>
        protected override void ProcessBouquetSections()
        {
            foreach (BouquetAssociationSection bouquetSection in BouquetAssociationSection.BouquetAssociationSections)
            {
                Bouquet bouquet = Bouquet.FindBouquet(bouquetSection.BouquetID);
                if (bouquet == null)
                {
                    bouquet = new Bouquet(bouquetSection.BouquetID, BouquetAssociationSection.FindBouquetName(bouquetSection.BouquetID));
                    Bouquet.AddBouquet(bouquet);
                }

                if (bouquetSection.TransportStreams != null)
                {
                    foreach (TransportStream transportStream in bouquetSection.TransportStreams)
                    {
                        if (transportStream.Descriptors != null)
                        {
                            foreach (DescriptorBase descriptor in transportStream.Descriptors)
                            {
                                FreeSatChannelInfoDescriptor freeSatInfoDescriptor = descriptor as FreeSatChannelInfoDescriptor;
                                if (freeSatInfoDescriptor != null)
                                {                                    
                                    if (freeSatInfoDescriptor.ChannelInfoEntries != null)
                                    {
                                        foreach (FreeSatChannelInfoEntry channelInfoEntry in freeSatInfoDescriptor.ChannelInfoEntries)
                                        {
                                            foreach (FreeSatChannelInfoRegionEntry regionEntry in channelInfoEntry.RegionEntries)
                                            {                                                    
                                                Region region = bouquet.FindRegion(regionEntry.RegionNumber);
                                                if (region == null)
                                                {
                                                    region = new Region(findRegionName(bouquetSection.BouquetID, regionEntry.RegionNumber), regionEntry.RegionNumber);
                                                    bouquet.AddRegion(region);
                                                }

                                                FreeSatChannel channel = new FreeSatChannel();
                                                channel.OriginalNetworkID = transportStream.OriginalNetworkID;
                                                channel.TransportStreamID = transportStream.TransportStreamID;
                                                channel.ServiceID = channelInfoEntry.ServiceID;
                                                
                                                channel.ChannelID = regionEntry.ChannelNumber;
                                                channel.UserChannel = regionEntry.ChannelNumber;
                                                
                                                channel.Unknown1 = channelInfoEntry.Unknown1;
                                                channel.BouquetID = bouquetSection.BouquetID;                                                  

                                                region.AddChannel(channel);
                                                FreeSatChannel.AddChannel(channel); 
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }                
            }
        }

        private string findRegionName(int bouquetID, int regionNumber)
        {
            foreach (BouquetAssociationSection section in BouquetAssociationSection.BouquetAssociationSections)
            {
                if (section.BouquetID == bouquetID)
                {
                    if (section.BouquetDescriptors != null)
                    {
                        foreach (DescriptorBase descriptor in section.BouquetDescriptors)
                        {
                            FreeSatRegionDescriptor regionDescriptor = descriptor as FreeSatRegionDescriptor;
                            if (regionDescriptor != null && regionDescriptor.RegionEntries != null)
                            {
                                foreach (FreeSatRegionEntry regionEntry in regionDescriptor.RegionEntries)
                                {
                                    if (regionEntry.RegionNumber == regionNumber)
                                        return (regionEntry.RegionDescription);
                                }
                            }
                        }
                    }
                }
            }

            return (string.Empty);
        }

        private void getFreeSatSections(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            Logger.Instance.Write("Collecting FreeSat data", false, true);

            dataProvider.ChangePidMapping(new int[] { 0xbbb, 0xc1f, 0xf02 });

            freeSatReader = new TSStreamReader(2000, dataProvider.BufferAddress);
            freeSatReader.Run();

            int lastCount = 0;
            int repeats = 0;
            int epgCount = 0;

            while (!freeSatSectionsDone)
            {
                if (worker.CancellationPending)
                    return;

                Thread.Sleep(2000);
                Logger.Instance.Write(".", false, false);

                Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();

                freeSatReader.Lock("LoadMessages");
                if (freeSatReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in freeSatReader.Sections)
                        sections.Add(section);
                    freeSatReader.Sections.Clear();
                }
                freeSatReader.Release("LoadMessages");

                if (sections.Count != 0)
                    processSections(sections);

                epgCount = TVStation.EPGCount(RunParameters.Instance.StationCollection);

                if (epgCount == lastCount)
                {
                    repeats++;
                    freeSatSectionsDone = (repeats == RunParameters.Instance.Repeats);
                }
                else
                    repeats = 0;

                lastCount = epgCount;
            }

            Logger.Instance.Write("", true, false);
            Logger.Instance.Write("Stopping reader");
            freeSatReader.Stop();

            Logger.Instance.Write("EPG count: " + epgCount + 
                " buffer space used: " + dataProvider.BufferSpaceUsed +
                " discontinuities: " + freeSatReader.Discontinuities);
        }

        private void getOtherSections(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            dataProvider.ChangePidMapping(new int[] { 0xbbe });

            Logger.Instance.Write("Collecting other data", false, true);

            TSStreamReader otherReader = new TSStreamReader(2000, dataProvider.BufferAddress);
            otherReader.Run();

            int repeats = 0;

            while (repeats < 1)
            {
                if (worker.CancellationPending)
                    return;

                Thread.Sleep(1000);

                Logger.Instance.Write(".", false, false);

                Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();

                otherReader.Lock("ProcessOtherSections");

                if (otherReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in otherReader.Sections)
                        sections.Add(section);
                    otherReader.Sections.Clear();
                }

                otherReader.Release("ProcessOtherSections");

                if (sections.Count != 0)
                {
                    foreach (Mpeg2Section section in sections)
                        Logger.Instance.Dump("Other Section", section.Data, section.Data.Length);
                }

                repeats++;
            }

            Logger.Instance.Write("", true, false);
            Logger.Instance.Write("Stopping other reader");
            otherReader.Stop();
        }

        private void processSections(Collection<Mpeg2Section> sections)
        {
            foreach (Mpeg2Section section in sections)
            {
                if (TraceEntry.IsDefined(TraceName.FreeSatSections))
                    Logger.Instance.Dump("FreeSat Section", section.Data, section.Length);

                try
                {
                    Mpeg2ExtendedHeader mpeg2Header = new Mpeg2ExtendedHeader();
                    mpeg2Header.Process(section.Data);
                    if (mpeg2Header.Current)
                    {
                        FreeSatSection freeSatSection = new FreeSatSection();
                        freeSatSection.Process(section.Data, mpeg2Header);
                    }
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Logger.Instance.Write("<e> FreeSat error: " + e.Message);
                }
            }
        }

        /// <summary>
        /// Create the EPG entries.
        /// </summary>
        public override void FinishFrequency()
        {
            if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ChannelBouquet != -1)
            {
                foreach (TVStation tvStation in RunParameters.Instance.StationCollection)
                {
                    if (tvStation.CurrentScan && tvStation.Included)
                    {
                        bool process = checkChannelMapping(tvStation);
                        if (!process)
                        {
                            tvStation.ExcludedByChannel = true;
                            tvStation.EPGCollection.Clear();
                        }
                    }
                }
            }
            else
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

            EITProgramContent.LogContentUsage();
            LanguageCode.LogUsage();
            MultiTreeDictionaryEntry.LogUsage();
            logChannelInfo();
        }

        private bool checkChannelMapping(TVStation tvStation)
        {
            Bouquet bouquet = Bouquet.FindBouquet(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ChannelBouquet);
            if (bouquet == null)
                return (false);

            Channel channel = null;

            if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ChannelRegion != -1)
                channel = findChannel(bouquet, RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ChannelRegion, tvStation.OriginalNetworkID, tvStation.TransportStreamID, tvStation.ServiceID);
            
            if (channel == null)
            {
                /*channel = findChannel(bouquet, 0, tvStation.OriginalNetworkID, tvStation.TransportStreamID, tvStation.ServiceID);*/
                {
                    /*if (channel == null)*/
                        channel = findChannel(bouquet, 100, tvStation.OriginalNetworkID, tvStation.TransportStreamID, tvStation.ServiceID);
                    {
                        if (channel == null)
                            channel = findChannel(bouquet, 65535, tvStation.OriginalNetworkID, tvStation.TransportStreamID, tvStation.ServiceID);
                    }
                }
            }

            if (channel == null)
                return (false);

            if (tvStation.LogicalChannelNumber == -1)
                tvStation.LogicalChannelNumber = channel.UserChannel;

            return (true);
        }

        private Channel findChannel(Bouquet bouquet, int regionNumber, int originalNetworkID, int transportStreamID, int serviceID)
        {
            Region region = bouquet.FindRegion(regionNumber);
            if (region == null)
                return (null);

            return(region.FindChannel(originalNetworkID, transportStreamID, serviceID));            
        }

        private void logChannelInfo()
        {
            if (!DebugEntry.IsDefined(DebugName.LogChannels))
                return;

            if (!RunParameters.Instance.ChannelDataNeeded)
                return;

            Logger.Instance.WriteSeparator("Bouquet Usage");

            bool firstBouquet = true;

            foreach (Bouquet bouquet in Bouquet.GetBouquetsInNameOrder())
            {
                if (!firstBouquet)
                    Logger.Instance.Write("");

                firstBouquet = false;

                foreach (Region region in bouquet.Regions)
                {
                    Logger.Instance.Write("Bouquet: " + bouquet.BouquetID + " - " + bouquet.Name + 
                        " Region: " + region.Code + " (channels = " + region.Channels.Count + ")");

                    foreach (Channel channel in region.GetChannelsInNameOrder())
                        Logger.Instance.Write("    Channel: " + channel.ToString());
                }
            }
        }
    }
}
