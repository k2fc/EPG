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

using DirectShow;
using DomainObjects;

namespace DVBServices
{
    /// <summary>
    /// The class that controls the acquisition and processing of NagraGuide data.
    /// </summary>
    public class NagraGuideController : ControllerBase
    {
        /// <summary>
        /// Get the collection type supported by this collector.
        /// </summary>
        public override CollectionType CollectionType { get { return (CollectionType.EIT); } }
        /// <summary>
        /// Return true if the EIT data is complete; false otherwise.
        /// </summary>
        public override bool AllDataProcessed { get { return (guideDone); } }

        private TSStreamReader guideReader;

        private bool guideDone = false;

        /// <summary>
        /// Initialize a new instance of the NagraGuideController class.
        /// </summary>
        public NagraGuideController() { }

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
        /// Acquire and process NagraGuide data.
        /// </summary>
        /// <param name="dataProvider">A sample data provider.</param>
        /// <param name="worker">The background worker that is running this collection.</param>
        /// <returns>A CollectorReply code.</returns>
        public override CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            return (Process(dataProvider, worker, CollectionSpan.AllData));
        }

        /// <summary>
        /// Acquire and process NagraGuide data.
        /// </summary>
        /// <param name="dataProvider">A sample data provider.</param>
        /// <param name="worker">The background worker that is running this collection.</param>
        /// <param name="collectionSpan">The amount of data to collect.</param>
        /// <returns>A CollectorReply code.</returns>
        public override CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker, CollectionSpan collectionSpan)
        {
            getGuideSections(dataProvider, worker);

            return (CollectorReply.OK);
        }

        private void getGuideSections(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            Logger.Instance.Write("Collecting NagraGuide data", false, true);

            dataProvider.ChangePidMapping(new int[] { 0xc8 });            

            guideReader = new TSStreamReader(0xb0, 50000, dataProvider.BufferAddress);
            guideReader.Run();

            int lastCount = 0;
            int repeats = 0;
            int epgCount = 0;

            while (!guideDone)
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
                    processSections(sections);

                epgCount = TVStation.EPGCount(RunParameters.Instance.StationCollection);

                if (epgCount == lastCount)
                {
                    repeats++;
                    guideDone = (repeats == RunParameters.Instance.Repeats);
                }
                else
                    repeats = 0;

                lastCount = epgCount;
            }

            Logger.Instance.Write("", true, false);
            Logger.Instance.Write("Stopping reader");
            guideReader.Stop();

            Logger.Instance.Write("EPG count: " + epgCount + " buffer space used: " + dataProvider.BufferSpaceUsed);
        }

        private void processSections(Collection<Mpeg2Section> sections)
        {
            foreach (Mpeg2Section section in sections)
            {
                if (DebugEntry.IsDefined(DebugName.NagraBlocks))
                    Logger.Instance.Dump("Nagra Block", section.Data, section.Data.Length);

                try
                {
                    Mpeg2ExtendedHeader mpeg2Header = new Mpeg2ExtendedHeader();
                    mpeg2Header.Process(section.Data);
                    Logger.Instance.Write("Table ID ext: 0x" + mpeg2Header.TableIDExtension.ToString("X"));
                    /*if (mpeg2Header.Current)
                    {
                        EITSection eitSection = new EITSection();
                        eitSection.Process(section.Data, mpeg2Header);
                        eitSection.LogMessage();
                    }*/
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Logger.Instance.Write("NagraGuide error: " + e.Message);
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
                    bool process = checkChannelMapping(tvStation);
                    if (!process)
                        tvStation.EPGCollection.Clear();
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

            logData();
        }

        private bool checkChannelMapping(TVStation tvStation)
        {
            Bouquet bouquet = Bouquet.FindBouquet(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ChannelBouquet);
            if (bouquet == null)
                return (false);

            Region region = bouquet.FindRegion(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ChannelRegion);
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

        private void logData()
        {
            if (!OptionEntry.IsDefined(OptionName.EitDoneOnCount))
            {
                if (TVStation.EITNotComplete(RunParameters.Instance.StationCollection) != 0)
                    TVStation.LogIncompleteEITMapEntries(RunParameters.Instance.StationCollection);
            }

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
                    Logger.Instance.Write("Bouquet: " + bouquet.BouquetID + " - " + bouquet.Name + " Region: " + region.Code + " (channels = " + region.Channels.Count + ")");

                    foreach (Channel channel in region.GetChannelsInNameOrder())
                        Logger.Instance.Write("    Channel: " + channel.ToString());
                }
            }
        }
    }
}
