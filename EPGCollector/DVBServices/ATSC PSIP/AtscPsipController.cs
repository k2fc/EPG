﻿////////////////////////////////////////////////////////////////////////////////// 
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
using System.Text;

using DirectShow;
using DomainObjects;

namespace DVBServices
{
    /// <summary>
    /// The class that controls the acquisition and processing of ATSC PSIP data.
    /// </summary>
    public class AtscPsipController : ControllerBase
    {
        /// <summary>
        /// Get the collection type supported by this collector.
        /// </summary>
        public override CollectionType CollectionType { get { return (CollectionType.PSIP); } }
        /// <summary>
        /// Return true if the EIT data is complete; false otherwise.
        /// </summary>
        public override bool AllDataProcessed { get { return (guideDone); } }

        private MasterGuideTable masterGuideTable;

        private TSStreamReader guideReader;

        private bool guideDone = false;

        /// <summary>
        /// Initialize a new instance of the AtscPsipController class.
        /// </summary>
        public AtscPsipController() { }

        /// <summary>
        /// Stop acquiring and processing EIT data.
        /// </summary>
        public override void Stop()
        {
            Logger.Instance.Write("Stopping section readers");

            if (guideReader != null)
                guideReader.Stop();

            Logger.Instance.Write("Stopped section readers");
        }

        /// <summary>
        /// Acquire and process ATSC PSIP Info data.
        /// </summary>
        /// <param name="dataProvider">A sample data provider.</param>
        /// <param name="worker">The background worker that is running this collection.</param>
        /// <returns>A CollectorReply code.</returns>
        public override CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            return (Process(dataProvider, worker, CollectionSpan.AllData));
        }

        /// <summary>
        /// Acquire and process ATSC PSIP Info data.
        /// </summary>
        /// <param name="dataProvider">A sample data provider.</param>
        /// <param name="worker">The background worker that is running this collection.</param>
        /// <param name="collectionSpan">The amount of data to collect.</param>
        /// <returns>A CollectorReply code.</returns>
        public override CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker, CollectionSpan collectionSpan)
        {
            if (collectionSpan == CollectionSpan.AllData)
            {
                AtscPsipProgramCategory.Load();
                CustomProgramCategory.Load();

                bool referenceTablesLoaded = MultiTreeDictionaryEntry.Load(Path.Combine(RunParameters.ConfigDirectory, "Huffman Dictionary ATSC PSIP T1.cfg"), Path.Combine(RunParameters.ConfigDirectory, "Huffman Dictionary ATSC PSIP T2.cfg"));
                if (!referenceTablesLoaded)
                    return (CollectorReply.ReferenceDataError);
            }

            getMasterGuideData(dataProvider, worker);
            if (worker.CancellationPending)
                return (CollectorReply.Cancelled);

            getVirtualChannelData(dataProvider, worker);
            if (worker.CancellationPending)
                return (CollectorReply.Cancelled);

            creatStationsFromChannels();

            if (collectionSpan == CollectionSpan.ChannelsOnly)
                return (CollectorReply.OK);

            /*getRatingRegionData(dataProvider, worker);
            if (worker.CancellationPending)
                return (CollectorReply.Cancelled);*/

            getExtendedTextData(dataProvider, worker);
            if (worker.CancellationPending)
                return (CollectorReply.Cancelled);

            getEventInformationData(dataProvider, worker);
            if (worker.CancellationPending)
                return (CollectorReply.Cancelled);

            OutputFile.UseUnicodeEncoding = MultipleString.UseUnicodeEncoding;

            return (CollectorReply.OK);
        }

        private void getMasterGuideData(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            Logger.Instance.Write("Collecting Master Guide data", false, true);

            dataProvider.ChangePidMapping(new int[] { 0x1ffb });
            guideReader = new TSStreamReader(0xc7, 2000, dataProvider.BufferAddress);

            guideReader.Run();

            int repeats = 0;
            bool done = false;

            while (!done)
            {
                if (worker.CancellationPending)
                    return;

                Thread.Sleep(2000);
                Logger.Instance.Write(".", false, false);

                Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();

                guideReader.Lock("LoadMessages");
                if (guideReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in guideReader.Sections)
                        sections.Add(section);
                    guideReader.Sections.Clear();
                }
                guideReader.Release("LoadMessages");

                if (sections.Count != 0)
                {
                    processMasterGuideTable(sections);
                    done = true;
                }
                else
                    done = (repeats == RunParameters.Instance.Repeats);                
            }

            Logger.Instance.Write("", true, false);
            Logger.Instance.Write("Stopping reader");
            guideReader.Stop();

            Logger.Instance.Write("Master Guide Data: " + "buffer space used: " + dataProvider.BufferSpaceUsed +
                " discontinuities: " + guideReader.Discontinuities);
        }

        private void getVirtualChannelData(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            Logger.Instance.Write("Collecting Virtual Channel data", false, true);
            VirtualChannelTable.Clear();

            dataProvider.ChangePidMapping(new int[] { 0x1ffb });

            Collection<byte> tables = new Collection<byte>();
            tables.Add(0xc8);
            tables.Add(0xc9);
            guideReader = new TSStreamReader(tables, 2000, dataProvider.BufferAddress);
            guideReader.Run();

            int repeats = 0;
            bool done = false;

            while (!done)
            {
                if (worker.CancellationPending)
                    return;

                Thread.Sleep(2000);
                Logger.Instance.Write(".", false, false);

                Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();

                guideReader.Lock("LoadMessages");
                if (guideReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in guideReader.Sections)
                        sections.Add(section);
                    guideReader.Sections.Clear();
                }
                guideReader.Release("LoadMessages");

                if (sections.Count != 0)
                    processVirtualChannelTable(sections, dataProvider.Frequency.Frequency);

                done = VirtualChannelTable.Complete;
                if (!done)
                {
                    repeats++;
                    done = (repeats == RunParameters.Instance.Repeats);
                }
            }

            Logger.Instance.Write("", true, false);
            Logger.Instance.Write("Stopping reader");
            guideReader.Stop();

            Logger.Instance.Write("Virtual Channel Data: Channel count: " + VirtualChannelTable.Channels.Count +
                " buffer space used: " + dataProvider.BufferSpaceUsed +
                " discontinuities: " + guideReader.Discontinuities);
        }

        private void getRatingRegionData(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            Logger.Instance.Write("Collecting Rating Region data", false, true);
            RatingRegionTable.Clear();

            int[] rrtPids = masterGuideTable.GetRRTPids();
            if (rrtPids.Length == 0)
            {
                Logger.Instance.Write("", true, false);
                Logger.Instance.Write("No Rating Region PID's in the Master Guide Table");
                return;
            }

            dataProvider.ChangePidMapping(rrtPids);

            guideReader = new TSStreamReader(0xca, 2000, dataProvider.BufferAddress);
            guideReader.Run();

            int repeats = 0;
            bool done = false;

            while (!done)
            {
                if (worker.CancellationPending)
                    return;

                Thread.Sleep(2000);
                Logger.Instance.Write(".", false, false);

                Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();

                guideReader.Lock("LoadMessages");
                if (guideReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in guideReader.Sections)
                        sections.Add(section);
                    guideReader.Sections.Clear();
                }
                guideReader.Release("LoadMessages");

                if (sections.Count != 0)
                    processRatingRegionTable(sections);

                if (RatingRegionTable.CheckComplete(masterGuideTable.GetRRTRegions()))
                {
                    repeats++;
                    done = (repeats == RunParameters.Instance.Repeats);
                }
                else
                    repeats = 0;
            }

            Logger.Instance.Write("", true, false);
            Logger.Instance.Write("Stopping reader");
            guideReader.Stop();

            logPidsUsed(guideReader);

            Logger.Instance.Write("Rating Region Data: Regions: " + RatingRegionTable.Regions.Count +
                " buffer space used: " + dataProvider.BufferSpaceUsed +
                " discontinuities: " + guideReader.Discontinuities);
        }

        private void getExtendedTextData(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            Logger.Instance.Write("Collecting Extended Text data", false, true);
            ExtendedTextTable.Clear();

            int[] extendedTextPids = masterGuideTable.GetETTPids();
            if (extendedTextPids.Length == 0)
            {
                Logger.Instance.Write("", true, false);
                Logger.Instance.Write("No Extended Text PID's in Master Guide Table");
                return;
            }

            dataProvider.ChangePidMapping(extendedTextPids);

            guideReader = new TSStreamReader(0xcc, 2000, dataProvider.BufferAddress);
            guideReader.Run();

            int lastCount = 0;
            int repeats = 0;
            bool done = false;

            while (!done)
            {
                if (worker.CancellationPending)
                    return;

                Thread.Sleep(2000);
                Logger.Instance.Write(".", false, false);

                Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();

                guideReader.Lock("LoadMessages");
                if (guideReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in guideReader.Sections)
                        sections.Add(section);
                    guideReader.Sections.Clear();
                }
                guideReader.Release("LoadMessages");

                if (sections.Count != 0)
                    processExtendedTextTable(sections);

                if (ExtendedTextTable.TextEntries.Count == lastCount)
                {
                    repeats++;
                    done = (repeats == RunParameters.Instance.Repeats);
                }
                else
                    repeats = 0;

                lastCount = ExtendedTextTable.TextEntries.Count;
            }

            Logger.Instance.Write("", true, false);
            Logger.Instance.Write("Stopping reader");
            guideReader.Stop();

            logPidsUsed(guideReader);

            Logger.Instance.Write("Extended Text Data: Entry count: " + ExtendedTextTable.TextEntries.Count +
                " buffer space used: " + dataProvider.BufferSpaceUsed +
                " discontinuities: " + guideReader.Discontinuities);
        }

        private void getEventInformationData(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            Logger.Instance.Write("Collecting Event Information data", false, true);

            int[] eitPids = masterGuideTable.GetEITPids();
            if (eitPids.Length == 0)
            {
                Logger.Instance.Write("", true, false);
                Logger.Instance.Write("No Event Information PID's in Master Guide Table");
                return;
            }

            dataProvider.ChangePidMapping(eitPids);

            guideReader = new TSStreamReader(0xcb, 2000, dataProvider.BufferAddress);
            guideReader.Run();

            int lastCount = 0;
            int repeats = 0;
            bool done = false;

            while (!done)
            {
                if (worker.CancellationPending)
                    return;

                Thread.Sleep(2000);
                Logger.Instance.Write(".", false, false);

                Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();

                guideReader.Lock("LoadMessages");
                if (guideReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in guideReader.Sections)
                        sections.Add(section);
                    guideReader.Sections.Clear();
                }
                guideReader.Release("LoadMessages");

                if (sections.Count != 0)
                    processEventInformationTable(sections, dataProvider.Frequency.Frequency);

                if (VirtualChannelTable.EPGCount == lastCount)
                {
                    repeats++;
                    done = (repeats == RunParameters.Instance.Repeats);
                }
                else
                    repeats = 0;

                lastCount = VirtualChannelTable.EPGCount;
            }

            Logger.Instance.Write("", true, false);
            Logger.Instance.Write("Stopping reader"); 
            guideReader.Stop();

            logPidsUsed(guideReader);

            Logger.Instance.Write("Event Information Data: EPG count: " + VirtualChannelTable.EPGCount +
                " buffer space used: " + dataProvider.BufferSpaceUsed +
                " discontinuities: " + guideReader.Discontinuities);
        }

        private void logPidsUsed(TSStreamReader guideReader)
        {
            Collection<int> pidsUsed = guideReader.PidsUsed;
            if (pidsUsed.Count == 0)
                Logger.Instance.Write("No pids used");
            else
            {
                StringBuilder pidString = new StringBuilder();

                foreach (int pid in pidsUsed)
                {
                    if (pidString.Length != 0)
                        pidString.Append(", ");
                    pidString.Append("0x" + pid.ToString("x"));
                }

                Logger.Instance.Write("Pids used: " + pidString);
            }
        }

        private void processMasterGuideTable(Collection<Mpeg2Section> sections)
        {
            foreach (Mpeg2Section section in sections)
            {
                if (DebugEntry.IsDefined(DebugName.MasterGuideTable))
                    Logger.Instance.Dump("PSIP Master Guide Table", section.Data, section.Data.Length);

                try
                {
                    Mpeg2ExtendedHeader mpeg2Header = new Mpeg2ExtendedHeader();
                    mpeg2Header.Process(section.Data);
                    if (mpeg2Header.Current)
                    {
                        masterGuideTable = new MasterGuideTable();
                        masterGuideTable.Process(section.Data, mpeg2Header);
                        masterGuideTable.LogMessage();
                    }
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Logger.Instance.Write("<e> PSIP error: " + e.Message);
                }
            }
        }

        private string getPidString(int[] pids)
        {
            StringBuilder pidString = new StringBuilder();
            foreach (int pid in pids)
            {
                if (pidString.Length == 0)
                    pidString.Append("0x" + pid.ToString("x"));
                else
                    pidString.Append(", 0x" + pid.ToString("x"));
            }

            return pidString.ToString();
        }

        private void processVirtualChannelTable(Collection<Mpeg2Section> sections, int frequency)
        {
            foreach (Mpeg2Section section in sections)
            {
                if (DebugEntry.IsDefined(DebugName.VirtualChannelTable))
                    Logger.Instance.Dump("PSIP Virtual Channel Table", section.Data, section.Data.Length);

                try
                {
                    Mpeg2ExtendedHeader mpeg2Header = new Mpeg2ExtendedHeader();
                    mpeg2Header.Process(section.Data);
                    if (mpeg2Header.Current)
                    {
                        VirtualChannelTable virtualChannelTable = new VirtualChannelTable();
                        virtualChannelTable.Process(section.Data, mpeg2Header, (mpeg2Header.TableID == 0xc9), frequency);
                        VirtualChannelTable.AddSectionNumber(mpeg2Header.SectionNumber);
                        virtualChannelTable.LogMessage();
                    }
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Logger.Instance.Write("<e> PSIP error: " + e.Message);
                }
            }
        }

        private void processRatingRegionTable(Collection<Mpeg2Section> sections)
        {
            foreach (Mpeg2Section section in sections)
            {
                if (DebugEntry.IsDefined(DebugName.RatingRegionTable))
                    Logger.Instance.Dump("PSIP Rating Region Table", section.Data, section.Data.Length);

                try
                {
                    Mpeg2ExtendedHeader mpeg2Header = new Mpeg2ExtendedHeader();
                    mpeg2Header.Process(section.Data);
                    if (mpeg2Header.Current)
                    {
                        RatingRegionTable ratingRegionTable = new RatingRegionTable();
                        ratingRegionTable.Process(section.Data, mpeg2Header);
                        ratingRegionTable.LogMessage();

                        RatingRegionTable.AddRegion(ratingRegionTable.Region);
                    }
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Logger.Instance.Write("<e> PSIP error: " + e.Message);
                }
            }
        }

        private void processExtendedTextTable(Collection<Mpeg2Section> sections)
        {
            foreach (Mpeg2Section section in sections)
            {
                if (DebugEntry.IsDefined(DebugName.ExtendedTextTable))
                    Logger.Instance.Dump("PSIP Extended Text Table", section.Data, section.Data.Length);

                try
                {
                    Mpeg2ExtendedHeader mpeg2Header = new Mpeg2ExtendedHeader();
                    mpeg2Header.Process(section.Data);
                    if (mpeg2Header.Current)
                    {
                        ExtendedTextTable extendedTextTable = new ExtendedTextTable();
                        extendedTextTable.Process(section.Data, mpeg2Header);
                        extendedTextTable.LogMessage();
                    }
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Logger.Instance.Write("<e> PSIP error: " + e.Message);
                }
            }
        }

        private void processEventInformationTable(Collection<Mpeg2Section> sections, int frequency)
        {
            foreach (Mpeg2Section section in sections)
            {
                if (DebugEntry.IsDefined(DebugName.EventInformationTable))
                    Logger.Instance.Dump("PSIP Event Information Table", section.Data, section.Data.Length);

                try
                {
                    Mpeg2ExtendedHeader mpeg2Header = new Mpeg2ExtendedHeader();
                    mpeg2Header.Process(section.Data);
                    if (mpeg2Header.Current)
                    {
                        EventInformationTable eventInformationTable = new EventInformationTable();
                        eventInformationTable.Process(section.PID, section.Data, mpeg2Header);
                        eventInformationTable.LogMessage();

                        if (eventInformationTable.Events != null)
                        {
                            foreach (EventInformationTableEntry eventEntry in eventInformationTable.Events)
                                processEvent(frequency, eventInformationTable.SourceID, eventEntry);
                        }
                    }
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Logger.Instance.Write("<e> PSIP error: " + e.Message);
                }
            }
        }

        private void processEvent(int frequency, int sourceID, EventInformationTableEntry eventEntry)
        {
            VirtualChannel channel = VirtualChannelTable.FindChannel(frequency, sourceID);
            if (channel == null)
                return;

            EPGEntry epgEntry = new EPGEntry();

            epgEntry.EventID = eventEntry.EventID;

            if (eventEntry.EventName != null)
                epgEntry.EventName = EditSpec.ProcessTitle(eventEntry.EventName.ToString().Replace("\0", ""));
            else
                epgEntry.EventName = "No Event Name";

            if (eventEntry.ETMLocation == 1 || eventEntry.ETMLocation == 2)
            {
                ExtendedTextTableEntry textEntry = ExtendedTextTable.FindEntry(sourceID, eventEntry.EventID);
                if (textEntry != null)
                    epgEntry.ShortDescription = EditSpec.ProcessDescription(textEntry.Text.ToString().Replace("\0", ""));
            }

            epgEntry.StartTime = Utils.RoundTime(TimeOffsetEntry.GetAdjustedTime(eventEntry.StartTime));
            epgEntry.Duration = Utils.RoundTime(eventEntry.Duration);
            epgEntry.EventCategory = getEventCategory(epgEntry.EventName, epgEntry.ShortDescription, eventEntry);
            epgEntry.ParentalRating = eventEntry.ParentalRating;
            epgEntry.ParentalRatingSystem = "VCHIP";
            epgEntry.AudioQuality = eventEntry.AudioQuality;
            epgEntry.EPGSource = EPGSource.PSIP;

            channel.AddEPGEntry(epgEntry);
        }

        private string getEventCategory(string title, string description, EventInformationTableEntry eventEntry)
        {
            if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options,
                OptionName.CustomCategoryOverride))
            {
                string customCategory = getCustomCategory(title, description);
                if (customCategory != null)
                    return (customCategory);
            }

            string category = AtscPsipProgramCategory.GetCategories(eventEntry);
            if (category != null)
                return (category);

            if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options,
                OptionName.CustomCategoryOverride))
                return (null);

            return (getCustomCategory(title, description));
        }

        private string getCustomCategory(string title, string description)
        {
            string category = CustomProgramCategory.FindCategoryDescription(title);
            if (category != null)
                return (category);

            return (CustomProgramCategory.FindCategoryDescription(description));
        }

        /// <summary>
        /// Create the EPG entries.
        /// </summary>
        public override void FinishFrequency()
        {
            if (DebugEntry.IsDefined(DebugName.LogPsipExtendedText))
                ExtendedTextTable.LogEntries();

            AtscPsipProgramCategory.LogCategoryUsage();
            MultiTreeDictionaryEntry.LogUsage();

            if (VirtualChannelTable.Channels.Count == 0)
                return;

            Logger titleLogger = null;
            Logger descriptionLogger = null;

            if (DebugEntry.IsDefined(DebugName.LogTitles))
                titleLogger = new Logger("EPG Titles.log");
            if (DebugEntry.IsDefined(DebugName.LogDescriptions))
                descriptionLogger = new Logger("EPG Descriptions.log");

            foreach (VirtualChannel channel in VirtualChannelTable.Channels)
            {
                TVStation station = TVStation.FindStation(RunParameters.Instance.StationCollection,
                    channel.CollectionFrequency, channel.MajorChannelNumber, channel.MinorChannelNumber);
                if (station != null && station.EPGCollection.Count == 0)
                {
                    foreach (EPGEntry epgEntry in channel.EPGCollection)
                    {
                        bool include = CheckEPGDays(epgEntry.StartTime);
                        if (include)
                        {
                            station.EPGCollection.Add(epgEntry);

                            if (titleLogger != null)
                                titleLogger.Write(epgEntry.ServiceID + " " +
                                    epgEntry.StartTime.ToShortDateString() + " " +
                                    epgEntry.StartTime.ToString("HH:mm") + " - " +
                                    epgEntry.StartTime.Add(epgEntry.Duration).ToString("HH:mm") + " " +
                                    epgEntry.EventName);

                            if (descriptionLogger != null && epgEntry.ShortDescription != null)
                                descriptionLogger.Write(epgEntry.ServiceID + " " +
                                    " Evt ID " + epgEntry.EventID +
                                    epgEntry.StartTime.ToShortDateString() + " " +
                                    epgEntry.StartTime.ToString("HH:mm") + " - " +
                                    epgEntry.StartTime.Add(epgEntry.Duration).ToString("HH:mm") + " " +
                                    epgEntry.ShortDescription);
                        }
                    }
                }                    
            }            
        }

        private void creatStationsFromChannels()
        {
            foreach (VirtualChannel channel in VirtualChannelTable.Channels)
            {
                TVStation station = TVStation.FindStation(RunParameters.Instance.StationCollection,
                    channel.CollectionFrequency, channel.MajorChannelNumber, channel.MinorChannelNumber);
                if (station == null)
                {
                    station = new TVStation(channel.ShortName);
                    station.OriginalNetworkID = channel.CollectionFrequency;
                    station.TransportStreamID = channel.MajorChannelNumber;
                    station.ServiceID = channel.MinorChannelNumber;
                    station.ChannelID = channel.MajorChannelNumber + ":" + channel.MinorChannelNumber + ":" + channel.ShortName;
                    TVStation.AddStation(RunParameters.Instance.StationCollection, station);
                }

                station.Name = channel.ShortName;

                if (station.LogicalChannelNumber == -1)
                    station.LogicalChannelNumber = (channel.MajorChannelNumber * 100) + channel.MinorChannelNumber;

                station.MinorChannelNumber = channel.MinorChannelNumber;
            }
        }
    }
}
