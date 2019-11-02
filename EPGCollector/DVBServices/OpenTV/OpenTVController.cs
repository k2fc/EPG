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

using System.Collections.ObjectModel;
using System.Threading;
using System.ComponentModel;
using System.IO;

using DirectShow;
using DomainObjects;

namespace DVBServices
{
    /// <summary>
    /// The class that controls the acquisition and processing of Open TV data.
    /// </summary>
    public class OpenTVController : ControllerBase
    {
        /// <summary>
        /// Get the collection type supported by this collector.
        /// </summary>
        public override CollectionType CollectionType { get { return (CollectionType.OpenTV); } }
        /// <summary>
        /// Return true if all data has been processed; false otherwise.
        /// </summary>
        public override bool AllDataProcessed { get { return (true); } }

        private TSStreamReader titleReader;
        private TSStreamReader summaryReader;
        private TSStreamReader otherReader;

        private int initialCount;

        private int titleDataCount;
        private int summaryCount;

        /// <summary>
        /// Initialize a new instance of the OpenTVController class.
        /// </summary>
        public OpenTVController() { }

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

            if (titleReader != null)
                titleReader.Stop();

            if (summaryReader != null)
                summaryReader.Stop();

            if (otherReader != null)
                otherReader.Stop();

            Logger.Instance.Write("Stopped section readers");
        }

        /// <summary>
        /// Acquire and process OpenTV data.
        /// </summary>
        /// <param name="dataProvider">A sampe data provider.</param>
        /// <param name="worker">The background worker that is running this collection.</param>
        /// <returns>A CollectorReply code.</returns>
        public override CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            return (Process(dataProvider, worker, CollectionSpan.AllData));
        }

        /// <summary>
        /// Acquire and process OpenTV data.
        /// </summary>
        /// <param name="dataProvider">A sampe data provider.</param>
        /// <param name="worker">The background worker that is running this collection.</param>
        /// <param name="collectionSpan">The amount of data to collect.</param>
        /// <returns>A CollectorReply code.</returns>
        public override CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker, CollectionSpan collectionSpan)
        {
            if (collectionSpan == CollectionSpan.AllData)
            {
                OpenTVProgramCategory.LoadFromCode(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode.Trim());
                CustomProgramCategory.Load();
                ParentalRating.Load();

                bool referenceTablesLoaded = SingleTreeDictionaryEntry.Load(Path.Combine(RunParameters.ConfigDirectory, 
                    "Huffman Dictionary " +
                    RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode.Trim() + 
                    ".cfg"));
                if (!referenceTablesLoaded)
                    return (CollectorReply.ReferenceDataError);
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

            if (RunParameters.Instance.StationCollection.Count == 0)
            {
                Logger.Instance.Write("<e> No stations located - data collection abandoned");
                return (CollectorReply.OK);
            }

            GetBouquetSections(dataProvider, worker);
            if (worker.CancellationPending)
                return (CollectorReply.Cancelled);

            if (Channel.Channels.Count == 0)
            {
                Logger.Instance.Write("<e> No channels located - data collection abandoned");
                return (CollectorReply.OK);
            }

            if (collectionSpan == CollectionSpan.ChannelsOnly)
                return (CollectorReply.OK);

            getTitleSections(dataProvider, worker);
            if (worker.CancellationPending)
                return (CollectorReply.Cancelled);

            getSummarySections(dataProvider, worker);
            if (worker.CancellationPending)
                return (CollectorReply.Cancelled);

            if (DebugEntry.IsDefined(DebugName.OtherSections))
            {
                getOtherSections(dataProvider, worker);
                if (worker.CancellationPending)
                    return (CollectorReply.Cancelled);
            }

            return (CollectorReply.OK);
        }

        private void getTitleSections(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            foreach (Channel channel in Channel.Channels)
            {
                OpenTVChannel openTVChannel = channel as OpenTVChannel;
                if (openTVChannel != null)
                    initialCount += openTVChannel.TitleData.Count;
            }

            dataProvider.ChangePidMapping(new int[] { 0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37 });
            
            Logger.Instance.Write("Collecting title data", false, true);

            Collection<byte> tables = new Collection<byte>();
            tables.Add(0xa0);
            tables.Add(0xa1);
            tables.Add(0xa2);
            tables.Add(0xa3);
            titleReader = new TSStreamReader(tables, 2000, dataProvider.BufferAddress);            
            titleReader.Run();

            int lastCount = 0;
            int repeats = 0;
            titleDataCount = 0;

            bool titleSectionsDone = false;

            while (!titleSectionsDone)
            {
                if (worker.CancellationPending)
                    return;

                Thread.Sleep(1000);
                Logger.Instance.Write(".", false, false);

                Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();

                titleReader.Lock("ProcessOpenTVSections");

                if (titleReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in titleReader.Sections)
                        sections.Add(section);
                    titleReader.Sections.Clear();
                }

                titleReader.Release("ProcessOpenTVSections");

                if (sections.Count != 0)
                    processTitleSections(sections);

                titleDataCount = 0;
                foreach (Channel channel in Channel.Channels)
                {
                    OpenTVChannel openTVChannel = channel as OpenTVChannel;
                    if (openTVChannel != null)
                        titleDataCount += openTVChannel.TitleData.Count;
                }

                if (titleDataCount == lastCount)
                {
                    repeats++;
                    titleSectionsDone = (repeats == RunParameters.Instance.Repeats);
                }
                else
                    repeats = 0;

                lastCount = titleDataCount;
            }

            Logger.Instance.Write("", true, false);
            Logger.Instance.Write("Stopping title reader");
            titleReader.Stop();

            dataProvider.Frequency.UsageCount = titleDataCount - initialCount;
            Logger.Instance.Write("Title count: " + titleDataCount + 
                " buffer space used: " + dataProvider.BufferSpaceUsed + 
                " discontinuities: " + titleReader.Discontinuities);
        }

        private void getSummarySections(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            dataProvider.ChangePidMapping(new int[] { 0x40, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, 0x47 });
            
            Logger.Instance.Write("Collecting summary data", false, true);

            Collection<byte> tables = new Collection<byte>();
            tables.Add(0xa8);
            tables.Add(0xa9);
            tables.Add(0xaa);
            tables.Add(0xab);

            summaryReader = new TSStreamReader(tables, 2000, dataProvider.BufferAddress);
            summaryReader.Run();

            int lastCount = 0;
            int repeats = 0;
            summaryCount = 0;

            bool summarySectionsDone = false;

            while (!summarySectionsDone)
            {
                if (worker.CancellationPending)
                    return;

                Thread.Sleep(1000);

                Logger.Instance.Write(".", false, false);

                Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();
                OpenTVSummarySection.OpenTVSummarySections.Clear();

                summaryReader.Lock("ProcessOpenTVSections");

                if (summaryReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in summaryReader.Sections)
                        sections.Add(section);
                    summaryReader.Sections.Clear();
                }

                summaryReader.Release("ProcessOpenTVSections");

                if (sections.Count != 0)
                    processSummarySections(sections);

                summaryCount = 0;
                foreach (Channel channel in Channel.Channels)
                {
                    OpenTVChannel openTVChannel = channel as OpenTVChannel;
                    if (openTVChannel != null)
                        summaryCount += openTVChannel.SummaryData.Count;
                }

                if (summaryCount == lastCount)
                {
                    repeats++;
                    summarySectionsDone = (repeats == RunParameters.Instance.Repeats);
                }
                else
                    repeats = 0;

                lastCount = summaryCount;
            }

            Logger.Instance.Write("", true, false);
            Logger.Instance.Write("Stopping summary reader");
            summaryReader.Stop();

            Logger.Instance.Write("Summary count: " + summaryCount + 
                " buffer space used: " + dataProvider.BufferSpaceUsed +
                " discontinuities: " + summaryReader.Discontinuities);
        }

        private void getOtherSections(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            dataProvider.ChangePidMapping(new int[] { 0x55, 0x60 });

            Logger.Instance.Write("Collecting other data", false, true);

            otherReader = new TSStreamReader(2000, dataProvider.BufferAddress);
            otherReader.Run();

            int otherCount = 0;

            bool otherSectionsDone = false;

            while (!otherSectionsDone)
            {
                if (worker.CancellationPending)
                    return;

                Thread.Sleep(1000);

                Logger.Instance.Write(".", false, false);

                Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();
                OpenTVSummarySection.OpenTVSummarySections.Clear();

                otherReader.Lock("ProcessOpenTVSections");

                if (otherReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in otherReader.Sections)
                        sections.Add(section);
                    otherReader.Sections.Clear();
                }

                otherReader.Release("ProcessOpenTVSections");

                if (sections.Count != 0)
                {
                    foreach (Mpeg2Section section in sections)
                        Logger.Instance.Dump("PID " + section.PID, section.Data, section.Data.Length);
                }

                otherCount += sections.Count;
                otherSectionsDone = (otherCount * 5 == RunParameters.Instance.Repeats);
            }

            Logger.Instance.Write("", true, false);
            Logger.Instance.Write("Stopping other reader");
            summaryReader.Stop();

            Logger.Instance.Write("Other count: " + otherCount +
                " buffer space used: " + dataProvider.BufferSpaceUsed +
                " discontinuities: " + otherReader.Discontinuities);
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
                                OpenTVChannelInfoDescriptor infoDescriptor = descriptor as OpenTVChannelInfoDescriptor;
                                if (infoDescriptor != null)
                                {
                                    if (infoDescriptor.ChannelInfoEntries != null)
                                    {
                                        foreach (OpenTVChannelInfoEntry channelInfoEntry in infoDescriptor.ChannelInfoEntries)
                                        {
                                            OpenTVChannel channel = new OpenTVChannel();
                                            channel.OriginalNetworkID = transportStream.OriginalNetworkID;
                                            channel.TransportStreamID = transportStream.TransportStreamID;
                                            channel.ServiceID = channelInfoEntry.ServiceID;
                                            channel.ChannelID = channelInfoEntry.ChannelID;
                                            channel.UserChannel = channelInfoEntry.UserNumber;
                                            channel.Type = channelInfoEntry.Type;
                                            channel.Flags = channelInfoEntry.Flags;
                                            channel.BouquetID = bouquetSection.BouquetID;
                                            channel.Region = infoDescriptor.Region;                                            

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
                                            OpenTVChannel.AddChannel(channel);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void processTitleSections(Collection<Mpeg2Section> sections)
        {
            foreach (Mpeg2Section section in sections)
            {
                /*Logger.Instance.Dump("Title Section", section.Data, section.Length);*/

                OpenTVTitleSection titleSection = OpenTVTitleSection.ProcessOpenTVTitleTable(section.Data, section.PID, section.Table);
                if (titleSection != null)
                {
                    if (titleSection.TitleHeader.TitleData != null)
                    {
                        OpenTVChannel channel = (OpenTVChannel)Channel.FindChannel(titleSection.TitleHeader.ChannelID);
                        if (channel != null)
                        {
                            foreach (OpenTVTitleData titleData in titleSection.TitleHeader.TitleData)
                                channel.AddTitleData(titleData);
                        }
                    }
                    
                    titleSection.LogMessage();
                }
            }
        }

        private void processSummarySections(Collection<Mpeg2Section> sections)
        {
            OpenTVSummarySection.OpenTVSummarySections.Clear();

            foreach (Mpeg2Section section in sections)
            {
                if (DebugEntry.IsDefined(DebugName.DumpOpenTvSummarySections))
                    Logger.Instance.Dump("Summary Section", section.Data, section.Length);

                OpenTVSummarySection summarySection = OpenTVSummarySection.ProcessOpenTVSummaryTable(section.Data);
                if (summarySection != null)
                {
                    if (summarySection.SummaryHeader.SummaryData != null)
                    {
                        OpenTVChannel channel = (OpenTVChannel)Channel.FindChannel(summarySection.SummaryHeader.ChannelID);
                        if (channel != null)
                        {
                            foreach (OpenTVSummaryData summaryData in summarySection.SummaryHeader.SummaryData)
                                channel.AddSummaryData(summaryData);
                        }
                    }

                    /*OpenTVSummarySection.AddSection(summarySection);*/
                    /*summarySection.LogMessage();*/
                }
            }
        }

        /// <summary>
        /// Carry out the processing after all data has been collected for a frequency.
        /// </summary>
        public override void FinishFrequency()
        {
            if (OpenTVChannel.Channels.Count == 0)
                return;

            Logger titleLogger = null;
            Logger descriptionLogger = null;
            Logger extendedDescriptionLogger = null;
            Logger undefinedRecordLogger = null;

            if (DebugEntry.IsDefined(DebugName.LogTitles))
                titleLogger = new Logger("EPG Titles.log");
            if (DebugEntry.IsDefined(DebugName.LogDescriptions))
                descriptionLogger = new Logger("EPG Descriptions.log");
            if (DebugEntry.IsDefined(DebugName.LogExtendedDescriptions))
                extendedDescriptionLogger = new Logger("EPG Extended Descriptions.log");
            if (DebugEntry.IsDefined(DebugName.LogUndefinedRecords))
                undefinedRecordLogger = new Logger("EPG Undefined Records.log");

            /*foreach (OpenTVChannel channel in OpenTVChannel.Channels)
            {
                TVStation station = TVStation.FindStation(channel.OriginalNetworkID, channel.TransportStreamID, channel.ServiceID);
                if (station != null)
                {
                    if (station.EPGCollection.Count == 0)
                    {
                        channel.ProcessChannelForEPG(station, titleLogger, descriptionLogger, extendedDescriptionLogger, undefinedRecordLogger);
                        channel.CreateChannelMapping(station);
                    }
                }
            }*/

            foreach (TVStation station in RunParameters.Instance.StationCollection)
            {
                if (station.CurrentScan && station.Included)
                {
                    Channel channel = Channel.FindChannel(station.OriginalNetworkID, station.TransportStreamID, station.ServiceID);
                    if (channel != null)
                    {
                        OpenTVChannel openTVChannel = channel as OpenTVChannel;
                        if (openTVChannel != null)
                        {
                            openTVChannel.ProcessChannelForEPG(station, titleLogger, descriptionLogger, extendedDescriptionLogger, undefinedRecordLogger);
                            openTVChannel.CreateChannelMapping(station);
                        }
                        else
                            station.ExcludedByChannel = true;
                    }
                    else
                        station.ExcludedByChannel = true;
                }
            }
        
            if (DebugEntry.IsDefined(DebugName.LogChannels))
            {
                Logger.Instance.WriteSeparator("Bouquet Usage In Identity Order");

                bool firstBouquet = true;

                foreach (Bouquet bouquet in Bouquet.GetBouquetsInIdentityOrder())
                {
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

            int count = 0;

            foreach (TuningFrequency frequency in RunParameters.Instance.FrequencyCollection)
            {
                if (frequency.CollectionType == CollectionType.OpenTV)
                    count++;
            }

            if (count > 1)
            {
                bool frequencyFirst = true;

                foreach (TuningFrequency frequency in RunParameters.Instance.FrequencyCollection)
                {
                    if (frequency.CollectionType == CollectionType.OpenTV)
                     {
                        if (frequencyFirst)
                        {
                            Logger.Instance.WriteSeparator("Frequency Usage");
                            frequencyFirst = false;
                        }
                        Logger.Instance.Write("Frequency " + frequency.Frequency + " usage count " + frequency.UsageCount);
                    }
                }
            }

            OpenTVProgramCategory.LogCategoryUsage();

            if (DebugEntry.IsDefined(DebugName.LogChannelData))
            {
                if (OpenTVChannel.Channels.Count != 0)
                {
                    Logger.Instance.WriteSeparator("Channel Data - ID Sequence");

                    foreach (OpenTVChannel channel in OpenTVChannel.Channels)
                        channel.LogChannelMapping(Logger.Instance);

                    Collection<OpenTVChannel> sortedChannels = new Collection<OpenTVChannel>();

                    foreach (OpenTVChannel channel in OpenTVChannel.Channels)
                        addByUserChannel(sortedChannels, channel);

                    Logger.Instance.WriteSeparator("Channel Data - User Channel Sequence");

                    foreach (OpenTVChannel channel in sortedChannels)
                        channel.LogChannelMapping(Logger.Instance);

                    sortedChannels = new Collection<OpenTVChannel>();

                    foreach (OpenTVChannel channel in OpenTVChannel.Channels)
                        addByID(sortedChannels, channel);

                    Logger.Instance.WriteSeparator("Channel Data - NID/TID/SID Sequence");

                    foreach (OpenTVChannel channel in sortedChannels)
                        channel.LogChannelMapping(Logger.Instance);
                }
            }

            if (DebugEntry.IsDefined(DebugName.LogUnknownRecords))
            {
                if (OpenTVRecordBase.UnknownRecords != null)
                {
                    Logger.Instance.WriteSeparator("Unknown OpenTV Record");

                    foreach (OpenTVRecordBase record in OpenTVRecordBase.UnknownRecords)
                        record.LogMessage();
                }
            }

            if (DebugEntry.IsDefined(DebugName.LogChannelGroups))
                ChannelGroup.LogChannelGroups();
        }

        private void addByUserChannel(Collection<OpenTVChannel> sortedChannels, OpenTVChannel newChannel)
        {
            foreach (OpenTVChannel oldChannel in sortedChannels)
            {
                if (newChannel.UserChannel < oldChannel.UserChannel)
                {
                    sortedChannels.Insert(sortedChannels.IndexOf(oldChannel), newChannel);
                    return;
                }
            }

            sortedChannels.Add(newChannel);
        }

        private void addByID(Collection<OpenTVChannel> sortedChannels, OpenTVChannel newChannel)
        {
            foreach (OpenTVChannel oldChannel in sortedChannels)
            {
                if (oldChannel.OriginalNetworkID > newChannel.OriginalNetworkID ||
                    (oldChannel.OriginalNetworkID == newChannel.OriginalNetworkID && oldChannel.TransportStreamID > newChannel.TransportStreamID) ||
                    (oldChannel.OriginalNetworkID == newChannel.OriginalNetworkID && oldChannel.TransportStreamID == newChannel.TransportStreamID && oldChannel.ServiceID > newChannel.ServiceID))
                {
                    sortedChannels.Insert(sortedChannels.IndexOf(oldChannel), newChannel);
                    return;
                }
            }

            sortedChannels.Add(newChannel);
        }

        private int compareFlags(byte[] flags1, byte[] flags2)
        {
            for (int index = 0; index < flags1.Length; index++)
            {
                if (flags1[index] < flags2[index])
                    return (-1);
                else
                {
                    if (flags1[index] > flags2[index])
                        return (1);
                }
            }

            return (0);
        }
    }
}
