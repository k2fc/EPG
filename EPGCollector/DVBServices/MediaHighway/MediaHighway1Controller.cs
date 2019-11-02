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

using System.Collections.ObjectModel;
using System.Threading;
using System.ComponentModel;

using DirectShow;
using DomainObjects;

namespace DVBServices
{
    /// <summary>
    /// The class that controls the acquisition and processing of MediaHighway1 data.
    /// </summary>
    public class MediaHighway1Controller : ControllerBase
    {
        /// <summary>
        /// Get the collection type supported by this collector.
        /// </summary>
        public override CollectionType CollectionType { get { return (CollectionType.MediaHighway1); } }
        /// <summary>
        /// Return true if all data has been processed; false otherwise.
        /// </summary>
        public override bool AllDataProcessed { get { return (true); } }

        private TSStreamReader channelReader;
        private TSStreamReader categoryReader;
        private TSStreamReader titleReader;
        private TSStreamReader summaryReader;

        private int pid1 = 0xd2;
        private int pid2 = 0xd3;

        /// <summary>
        /// Initialize a new instance of the MediaHighway1Controller class.
        /// </summary>
        public MediaHighway1Controller() 
        {
            if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.MHW1Pids != null)
            {
                pid1 = RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.MHW1Pids[0];
                pid2 = RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.MHW1Pids[1];
            }
        }
        
        /// <summary>
        /// Stop acquiring and processing data.
        /// </summary>
        public override void Stop()
        {
            Logger.Instance.Write("Stopping section readers");

            if (channelReader != null)
                channelReader.Stop();

            if (categoryReader != null)
                categoryReader.Stop();

            if (titleReader != null)
                titleReader.Stop();

            if (summaryReader != null)
                summaryReader.Stop();

            Logger.Instance.Write("Stopped section readers");
        }

        /// <summary>
        /// Acquire and process MediaHighway1 data.
        /// </summary>
        /// <param name="dataProvider">A sample data provider.</param>
        /// <param name="worker">The background worker that is running this collection.</param>
        /// <returns>A CollectorReply code.</returns>
        public override CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            return (Process(dataProvider, worker, CollectionSpan.AllData));
        }

        /// <summary>
        /// Acquire and process MediaHighway1 data.
        /// </summary>
        /// <param name="dataProvider">A sample data provider.</param>
        /// <param name="worker">The background worker that is running this collection.</param>
        /// <param name="collectionSpan">The amount of data to collect.</param>
        /// <returns>A CollectorReply code.</returns>
        public override CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker, CollectionSpan collectionSpan)
        {
            if (collectionSpan == CollectionSpan.AllData)
            {
                MediaHighwayProgramCategory.LoadFromFrequency("1", dataProvider.Frequency.ToString());
                CustomProgramCategory.Load();
                ParentalRating.Load();
            }

            if (RunParameters.Instance.NetworkDataNeeded)
            {
                GetNetworkInformation(dataProvider, worker);
                if (worker.CancellationPending)
                    return (CollectorReply.Cancelled);
            }

            getChannelSections(dataProvider, worker);
            if (worker.CancellationPending)
                return (CollectorReply.Cancelled);

            if (Channel.Channels.Count == 0)
            {
                Logger.Instance.Write("<e> No channels located - data collection abandoned");
                return (CollectorReply.OK);
            }

            creatStationsFromChannels();

            if (collectionSpan == CollectionSpan.ChannelsOnly)
                return (CollectorReply.OK);

            getCategorySections(dataProvider, worker);
            if (worker.CancellationPending)
                return (CollectorReply.Cancelled);

            getTitleSections(dataProvider, worker);
            if (worker.CancellationPending)
                return (CollectorReply.Cancelled);

            getSummarySections(dataProvider, worker);

            return (CollectorReply.OK);
        }

        private void getChannelSections(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            Logger.Instance.Write("Collecting channel data", false, true);
            Channel.Channels.Clear();

            dataProvider.ChangePidMapping(new int[] { pid2 });

            channelReader = new TSStreamReader(0x91, 2000, dataProvider.BufferAddress);
            channelReader.Run();

            int lastCount = 0;
            int repeats = 0;

            bool channelSectionsDone = false;

            while (!channelSectionsDone)
            {
                if (worker.CancellationPending)
                    return;

                Thread.Sleep(2000);
                Logger.Instance.Write(".", false, false);

                Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();

                channelReader.Lock("ProcessMHW1Sections");
                if (channelReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in channelReader.Sections)
                        sections.Add(section);
                    channelReader.Sections.Clear();
                }
                channelReader.Release("ProcessMHW1Sections");

                if (sections.Count != 0)
                    processChannelSections(sections);

                if (Channel.Channels.Count == lastCount)
                {
                    repeats++;
                    channelSectionsDone = (repeats == RunParameters.Instance.Repeats);
                }
                else
                    repeats = 0;

                lastCount = Channel.Channels.Count;
            }

            Logger.Instance.Write("", true, false);
            Logger.Instance.Write("Stopping channel reader for PID 0x" + pid2.ToString("X").ToLowerInvariant());
            channelReader.Stop();

            Logger.Instance.Write("Channel count: " + Channel.Channels.Count + 
                " buffer space used: " + dataProvider.BufferSpaceUsed +
                " discontinuities: " + channelReader.Discontinuities);
        }

        private void getCategorySections(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            Logger.Instance.Write("Collecting category data", false, true);
            
            dataProvider.ChangePidMapping(new int[] { pid2 });

            categoryReader = new TSStreamReader(0x92, 2000, dataProvider.BufferAddress);
            categoryReader.Run();

            int lastCount = 0;
            int repeats = 0;

            bool categorySectionsDone = false;

            while (!categorySectionsDone)
            {
                if (worker.CancellationPending)
                    return;

                Thread.Sleep(2000);
                Logger.Instance.Write(".", false, false);

                Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();

                categoryReader.Lock("ProcessMHW1Sections");
                if (categoryReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in categoryReader.Sections)
                        sections.Add(section);
                    categoryReader.Sections.Clear();
                }
                categoryReader.Release("ProcessMHW1Sections");

                if (sections.Count != 0)
                    processCategorySections(sections);

                if (MediaHighwayProgramCategory.Categories.Count == lastCount)
                {
                    repeats++;
                    categorySectionsDone = (repeats == RunParameters.Instance.Repeats);
                }
                else
                    repeats = 0;

                lastCount = MediaHighwayProgramCategory.Categories.Count;
            }

            Logger.Instance.Write("", true, false);
            Logger.Instance.Write("Stopping category reader for PID 0x" + pid2.ToString("X").ToLowerInvariant());
            categoryReader.Stop();

            Logger.Instance.Write("Category count: " + MediaHighwayProgramCategory.Categories.Count +
                " buffer space used: " + dataProvider.BufferSpaceUsed +
                " discontinuities: " + categoryReader.Discontinuities); 
        }

        private void getTitleSections(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            Logger.Instance.Write("Collecting title data", false, true);

            dataProvider.ChangePidMapping(new int[] { pid1 });
            
            titleReader = new TSStreamReader(0x90, 2000, dataProvider.BufferAddress);
            titleReader.Run();

            int lastCount = 0;
            int repeats = 0;
            int titleDataCount = 0;

            bool titleSectionsDone = false;

            while (!titleSectionsDone)
            {
                if (worker.CancellationPending)
                    return;

                Thread.Sleep(1000);
                Logger.Instance.Write(".", false, false);

                Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();

                titleReader.Lock("ProcessMHW1Sections");

                if (titleReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in titleReader.Sections)
                        sections.Add(section);
                    titleReader.Sections.Clear();
                }

                titleReader.Release("ProcessMHW1Sections");

                if (sections.Count != 0)
                    processTitleSections(sections);

                titleDataCount = 0;
                foreach (MediaHighwayChannel channel in MediaHighwayChannel.Channels)
                    titleDataCount += channel.Titles.Count;

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
            Logger.Instance.Write("Stopping title reader for PID 0x" + pid1.ToString("X").ToLowerInvariant());
            titleReader.Stop();

            Logger.Instance.Write("Title count: " + titleDataCount +
                " buffer space used: " + dataProvider.BufferSpaceUsed +
                " discontinuities: " + titleReader.Discontinuities);
        }

        private void getSummarySections(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            Logger.Instance.Write("Collecting summary data", false, true);

            dataProvider.ChangePidMapping(new int[] { pid2 });
            
            summaryReader = new TSStreamReader(0x90, 2000, dataProvider.BufferAddress);
            summaryReader.Run();

            int lastCount = 0;
            int repeats = 0;

            bool summarySectionsDone = false;

            while (!summarySectionsDone)
            {
                if (worker.CancellationPending)
                    return;

                Thread.Sleep(1000);

                Logger.Instance.Write(".", false, false);

                Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();

                summaryReader.Lock("ProcessMHW1Sections");

                if (summaryReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in summaryReader.Sections)
                        sections.Add(section);
                    summaryReader.Sections.Clear();
                }

                summaryReader.Release("ProcessMHW1Sections");

                if (sections.Count != 0)
                    processSummarySections(sections);

                if (MediaHighwaySummary.Summaries.Count == lastCount)
                {
                    repeats++;
                    summarySectionsDone = (repeats == RunParameters.Instance.Repeats);
                }
                else
                    repeats = 0;

                lastCount = MediaHighwaySummary.Summaries.Count;
            }

            Logger.Instance.Write("", true, false);
            Logger.Instance.Write("Stopping summary reader for PID 0x" + pid2.ToString("X").ToLowerInvariant());
            summaryReader.Stop();

            Logger.Instance.Write("Summary count: " + MediaHighwaySummary.Summaries.Count +
                " buffer space used: " + dataProvider.BufferSpaceUsed +
                " discontinuities: " + summaryReader.Discontinuities);
        }

        private void processChannelSections(Collection<Mpeg2Section> sections)
        {
            foreach (Mpeg2Section section in sections)
            {
                if (TraceEntry.IsDefined(TraceName.DumpChannelSections))
                    Logger.Instance.Dump("Channel Section", section.Data, section.Length);

                MediaHighway1ChannelSection channelSection = MediaHighway1ChannelSection.ProcessMediaHighwayChannelTable(section.Data);
                if (channelSection != null)
                {
                    if (channelSection.Channels != null)
                    {
                        foreach (MediaHighwayChannelInfoEntry channelInfoEntry in channelSection.Channels)
                        {
                            MediaHighwayChannel channel = new MediaHighwayChannel();
                            channel.ChannelID = channelSection.Channels.IndexOf(channelInfoEntry) + 1;
                            channel.OriginalNetworkID = channelInfoEntry.OriginalNetworkID;
                            channel.TransportStreamID = channelInfoEntry.TransportStreamID;
                            channel.ServiceID = channelInfoEntry.ServiceID;
                            channel.ChannelName = channelInfoEntry.Name;
                            channel.UserChannel = Channel.Channels.Count + 1;
                            Channel.AddChannel(channel);
                        }
                    }                    
                }
            }
        }

        private void processCategorySections(Collection<Mpeg2Section> sections)
        {
            foreach (Mpeg2Section section in sections)
            {
                if (TraceEntry.IsDefined(TraceName.DumpCategorySections))
                    Logger.Instance.Dump("Category Section", section.Data, section.Length);

                MediaHighway1CategorySection categorySection = MediaHighway1CategorySection.ProcessMediaHighwayCategoryTable(section.Data);
                if (categorySection != null)
                {
                    if (categorySection.Categories != null)
                    {
                        foreach (MediaHighwayCategoryEntry categoryEntry in categorySection.Categories)
                            MediaHighwayProgramCategory.AddCategory(categoryEntry.Number, categoryEntry.Description);
                    }
                }
            }
        }

        private void processTitleSections(Collection<Mpeg2Section> sections)
        {
            foreach (Mpeg2Section section in sections)
            {
                if (TraceEntry.IsDefined(TraceName.DumpTitleSections))
                    Logger.Instance.Dump("Title Section", section.Data, section.Length);

                MediaHighway1TitleSection titleSection = MediaHighway1TitleSection.ProcessMediaHighwayTitleTable(section.Data);
                if (titleSection != null && titleSection.TitleData != null)
                {
                    MediaHighwayChannel channel = (MediaHighwayChannel)MediaHighwayChannel.FindChannel(titleSection.TitleData.ChannelID);
                    if (channel != null)
                    {
                        MediaHighwayTitle title = new MediaHighwayTitle();
                        title.CategoryID = titleSection.TitleData.CategoryID;
                        title.Duration = titleSection.TitleData.Duration;
                        title.EventID = titleSection.TitleData.EventID;
                        title.EventName = titleSection.TitleData.EventName;
                        title.StartTime = titleSection.TitleData.StartTime;
                        title.SummaryAvailable = titleSection.TitleData.SummaryAvailable;
                        title.Day = titleSection.TitleData.Day;
                        title.Hours = titleSection.TitleData.Hours;
                        title.Minutes = titleSection.TitleData.Minutes;
                        title.LogDay = titleSection.TitleData.LogDay;
                        title.LogHours = titleSection.TitleData.LogHours;
                        title.LogYesterday = titleSection.TitleData.LogYesterday;
                        channel.AddTitleData(title);
                    }
                }
            }
        }

        private void processSummarySections(Collection<Mpeg2Section> sections)
        {
            foreach (Mpeg2Section section in sections)
            {
                if (TraceEntry.IsDefined(TraceName.DumpSummarySections))
                    Logger.Instance.Dump("Summary Section", section.Data, section.Length);

                MediaHighway1SummarySection summarySection = MediaHighway1SummarySection.ProcessMediaHighwaySummaryTable(section.Data);
                if (summarySection != null && summarySection.SummaryData != null)
                {
                    MediaHighwaySummary summary = new MediaHighwaySummary();
                    summary.EventID = summarySection.SummaryData.EventID;
                    summary.ShortDescription = summarySection.SummaryData.ShortDescription;
                    summary.ReplayCount = summarySection.SummaryData.ReplayCount;
                    summary.Replays = summarySection.SummaryData.Replays;
                    MediaHighwaySummary.AddSummary(summary);
                }
            }
        }

        /// <summary>
        /// Create the EPG entries.
        /// </summary>
        public override void FinishFrequency()
        {
            if (MediaHighwayChannel.Channels.Count == 0)
                return;

            foreach (MediaHighwaySummary summary in MediaHighwaySummary.Summaries)
            {
                if (summary.Replays != null)
                {
                    MediaHighwayChannelTitle title = MediaHighwayChannel.FindChannelTitle(summary.EventID);
                    if (title != null)
                    {
                        foreach (MediaHighway1Replay replay in summary.Replays)
                        {
                            MediaHighwayTitle replayTitle = new MediaHighwayTitle();
                            replayTitle.EventID = title.Title.EventID;
                            replayTitle.EventName = title.Title.EventName;
                            replayTitle.CategoryID = title.Title.CategoryID;
                            replayTitle.StartTime = replay.ReplayTime;
                            replayTitle.Duration = title.Title.Duration;
                            replayTitle.SummaryAvailable = true;
                            replayTitle.PreviousPlayDate = title.Title.StartTime;
                            ((MediaHighwayChannel)Channel.FindChannel(replay.Channel)).AddTitleData(replayTitle);
                            
                            if (DebugEntry.IsDefined(DebugName.Replays))
                                Logger.Instance.Write("Replay: ch" + replay.Channel + " " +
                                    title.Title.EventName + " " +
                                    title.Title.StartTime + " " +
                                    title.Title.Duration + " " +
                                    replay.ReplayTime);
                        }
                    }
                }
            }

            Logger titleLogger = null;
            Logger descriptionLogger = null;

            if (DebugEntry.IsDefined(DebugName.LogTitles))
                titleLogger = new Logger("EPG Titles.log");
            if (DebugEntry.IsDefined(DebugName.LogDescriptions))
                descriptionLogger = new Logger("EPG Descriptions.log");

            foreach (MediaHighwayChannel channel in MediaHighwayChannel.Channels)
            {
                TVStation station = TVStation.FindStation(RunParameters.Instance.StationCollection,
                    channel.OriginalNetworkID, channel.TransportStreamID, channel.ServiceID);
                if (station != null && station.EPGCollection.Count == 0)
                    channel.ProcessChannelForEPG(station, titleLogger, descriptionLogger, CollectionType.MediaHighway1);
            }
        
            MediaHighwayProgramCategory.LogCategories();
            Channel.LogChannelsInChannelIDOrder();
        }

        private void creatStationsFromChannels()
        {
            foreach (MediaHighwayChannel channel in MediaHighwayChannel.Channels)
            {
                TVStation station = TVStation.FindStation(RunParameters.Instance.StationCollection,
                    channel.OriginalNetworkID, channel.TransportStreamID, channel.ServiceID);
                if (station == null)
                {
                    station = new TVStation(channel.ChannelName);
                    station.OriginalNetworkID = channel.OriginalNetworkID;
                    station.TransportStreamID = channel.TransportStreamID;
                    station.ServiceID = channel.ServiceID;
                    TVStation.AddStation(RunParameters.Instance.StationCollection, station);
                }

                station.Name = channel.ChannelName;

                if (station.LogicalChannelNumber == -1)
                    station.LogicalChannelNumber = channel.UserChannel;
            }
        }
    }
}
