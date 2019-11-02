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
    /// The class that controls the acquisition and processing of Dish Network data.
    /// </summary>   
    public class DishNetworkController : ControllerBase
    {
        /// <summary>
        /// Get the collection type supported by this collector.
        /// </summary>
        public override CollectionType CollectionType { get { return (CollectionType.DishNetwork); } }
        /// <summary>
        /// Return true if all data has been processed; false otherwise.
        /// </summary>
        public override bool AllDataProcessed { get { return (true); } }

        private TSStreamReader dishNetworkReader;
        private bool dishNetworkSectionsDone = false;

        /// <summary>
        /// Initialize a new instance of the DishNetworkController class.
        /// </summary>
        public DishNetworkController() { }

        /// <summary>
        /// Stop acquiring and processing data.
        /// </summary>
        public override void Stop()
        {
            Logger.Instance.Write("Stopping section readers");

            if (dishNetworkReader != null)
                dishNetworkReader.Stop();

            Logger.Instance.Write("Stopped section readers");
        }

        /// <summary>
        /// Acquire and process Dish Network data.
        /// </summary>
        /// <param name="dataProvider">A sample data provider.</param>
        /// <param name="worker">The background worker that is running this collection.</param>
        /// <returns>A CollectorReply code.</returns>
        public override CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            return (Process(dataProvider, worker, CollectionSpan.AllData));
        }

        /// <summary>
        /// Acquire and process Dish Network data.
        /// </summary>
        /// <param name="dataProvider">A sample data provider.</param>
        /// <param name="worker">The background worker that is running this collection.</param>
        /// <param name="collectionSpan">The amount of data to collect.</param>
        /// <returns>A CollectorReply code.</returns>
        public override CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker, CollectionSpan collectionSpan)
        {
            if (collectionSpan == CollectionSpan.AllData)
            {
                DishNetworkProgramCategory.Load();
                CustomProgramCategory.Load();
                ParentalRating.Load();

                SingleTreeDictionaryEntry.Load(Path.Combine(RunParameters.ConfigDirectory, "Huffman Dictionary Dish Network 128.cfg"), 1);
                SingleTreeDictionaryEntry.Load(Path.Combine(RunParameters.ConfigDirectory, "Huffman Dictionary Dish Network 255.cfg"), 2);
                SingleTreeDictionaryEntry.OffsetStart = false;
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

            if (collectionSpan == CollectionSpan.ChannelsOnly)
                return (CollectorReply.OK);

            getDishNetworkData(dataProvider, worker);

            return (CollectorReply.OK);
        }

        private void getDishNetworkData(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            Logger.Instance.Write("Collecting Dish Network data");

            int actualPid;
            if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.DishNetworkPid == -1)
                actualPid = 0x300;
            else
                actualPid = RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.DishNetworkPid;

            dataProvider.ChangePidMapping(actualPid);
            
            dishNetworkReader = new TSStreamReader(2000, dataProvider.BufferAddress);
            dishNetworkReader.Run();

            int repeats = 0;
            int bufferFill = 1;

            int sectionCount = 0;
            int totalSectionsProcessed = 0;            

            Logger.Instance.Write("Buffer scan " + bufferFill + " of " + RunParameters.Instance.BufferFills + " starting", false, true);                        

            while (!dishNetworkSectionsDone)
            {
                if (worker.CancellationPending)
                    return;

                Thread.Sleep(5000);
                Logger.Instance.Write(".", false, false);

                Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();

                dishNetworkReader.Lock("LoadMessages");
                if (dishNetworkReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in dishNetworkReader.Sections)
                        sections.Add(section);
                    dishNetworkReader.Sections.Clear();
                }
                dishNetworkReader.Release("LoadMessages");

                if (sections.Count != 0)
                {
                    processSections(sections);
                    sectionCount += sections.Count;
                }
                else
                {
                    repeats++;
                    dishNetworkSectionsDone = (repeats == RunParameters.Instance.Repeats);

                    if (dishNetworkSectionsDone)
                    {
                        Logger.Instance.Write("", true, false);
                        Logger.Instance.Write("Buffer scan " + bufferFill + " of " + RunParameters.Instance.BufferFills +
                            " finished: " + 
                            " sections processed: " + sectionCount + 
                            " buffer space used: " + dataProvider.BufferSpaceUsed);

                        totalSectionsProcessed += sectionCount;

                        dishNetworkSectionsDone = (bufferFill == RunParameters.Instance.BufferFills);

                        if (!dishNetworkSectionsDone)
                        {
                            dishNetworkReader.Stop();

                            dataProvider.ChangePidMapping(actualPid);

                            dishNetworkReader = new TSStreamReader(2000, dataProvider.BufferAddress);
                            dishNetworkReader.Run();

                            repeats = 0;
                            sectionCount = 0;
                            bufferFill++;                            

                            Logger.Instance.Write("Buffer scan " + bufferFill + " of " + RunParameters.Instance.BufferFills + " starting");
                        }
                    }
                }
            }
            
            Logger.Instance.Write("Stopping reader");
            dishNetworkReader.Stop();

            Logger.Instance.Write("Buffer processing complete - EPG count: " + TVStation.EPGCount(RunParameters.Instance.StationCollection) + 
                " total sections processed: " + totalSectionsProcessed + 
                " buffer space used: " + dataProvider.BufferSpaceUsed +
                " discontinuities: " + dishNetworkReader.Discontinuities);
        }

        private void processSections(Collection<Mpeg2Section> sections)
        {
            foreach (Mpeg2Section section in sections)
            {
                if (TraceEntry.IsDefined(TraceName.DescriptorD3))
                    Logger.Instance.Dump("Dish Network Section", section.Data, section.Length);

                try
                {
                    Mpeg2ExtendedHeader mpeg2Header = new Mpeg2ExtendedHeader();
                    mpeg2Header.Process(section.Data);
                    if (mpeg2Header.Current)
                    {
                        if (checkTableIncluded(mpeg2Header.TableID) || RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.DishNetworkPid != -1)
                        {
                            DishNetworkSection dishNetworkSection = new DishNetworkSection();
                            dishNetworkSection.Process(section.Data, mpeg2Header);
                            dishNetworkSection.LogMessage();
                        }
                    }
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Logger.Instance.Write("<e> Dish Network error: " + e.Message);
                }
            }
        }

        private bool checkTableIncluded(int tableID)
        {
            if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.DishNetworkPid != -1)
                return (true);

            return (tableID > 0x80 && tableID < 0xa5);
        }

        /// <summary>
        /// Create the EPG entries.
        /// </summary>
        public override void FinishFrequency()
        {
            DishNetworkProgramCategory.LogCategoryUsage();

            foreach (TVStation station in RunParameters.Instance.StationCollection)
            {
                if (station.OriginalChannelNumber != -1 || station.LogicalChannelNumber != -1)
                {
                    Channel channel = new Channel();
                    channel.OriginalNetworkID = station.OriginalNetworkID;
                    channel.TransportStreamID = station.TransportStreamID;
                    channel.ServiceID = station.ServiceID;

                    if (station.OriginalChannelNumber != -1)
                        channel.UserChannel = station.OriginalChannelNumber;
                    else
                        channel.UserChannel = station.LogicalChannelNumber;

                    Channel.AddChannel(channel);
                }
            }
        }
    }
}
