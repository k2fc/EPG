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
using System.Text;
using System.IO;

using DirectShow;
using DomainObjects;

namespace DVBServices
{
    /// <summary>
    /// The base class for the collector controllers.
    /// </summary>
    public abstract class ControllerBase : IEPGCollector
    {
        /// <summary>
        /// Get the network information reader.
        /// </summary>
        protected TSStreamReader NetworkReader { get { return (nitReader); } }
        /// <summary>
        /// Get the bouquet reader.
        /// </summary>
        protected TSStreamReader BouquetReader { get { return (bouquetReader); } }
        /// <summary>
        /// Get the time offset reader.
        /// </summary>
        protected TSStreamReader TimeOffsetReader { get { return (timeOffsetReader); } }

        /// <summary>
        /// Get the collection type supported by this collector.
        /// </summary>
        public abstract CollectionType CollectionType { get; }
        /// <summary>
        /// Return true if all data has been processed; false otherwise.
        /// </summary>
        public abstract bool AllDataProcessed { get; }

        private TSStreamReader nitReader;
        private TSStreamReader bouquetReader;
        private TSStreamReader timeOffsetReader;

        private static bool omitTimeZoneSections;

        private static bool stationCacheLoaded;

        private int lastReportGroup;        

        /// <summary>
        /// Initialise a new instance of the ControllerBase class.
        /// </summary>
        public ControllerBase() { }

        /// <summary>
        /// Collect EPG data.
        /// </summary>
        /// <param name="dataProvider">The provider for the data samples.</param>
        /// <param name="worker">The BackgroundWorker instance running the collection.</param>        
        /// <returns>A CollectorReply code.</returns>
        public abstract CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker);

        /// <summary>
        /// Collect partial EPG data.
        /// </summary>
        /// <param name="dataProvider">The provider for the data samples.</param>
        /// <param name="worker">The BackgroundWorker instance running the collection.</param>        
        /// <param name="collectionSpan">The amount of data to collect.</param>   
        /// <returns>A CollectorReply code.</returns>
        public abstract CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker, CollectionSpan collectionSpan);

        /// <summary>
        /// Get the stations using the standard SDT pid.
        /// </summary>
        /// <param name="dataProvider">The sample data provider.</param>
        /// <param name="worker">The BackgroundWorker instance running the collection.</param>
        protected void GetStationData(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            int actualPid;
            if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.SDTPid == -1)
                actualPid = BDAGraph.SdtPid;
            else
                actualPid = RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.SDTPid;

            GetStationData(dataProvider, worker, new int[] { actualPid });
        }

        /// <summary>
        /// Get the stations using specified pid's.
        /// </summary>
        /// <param name="dataProvider">The sample data provider.</param>
        /// <param name="worker">The BackgroundWorker instance running the collection.</param>
        /// <param name="pids">An array of pid's to search.</param>
        protected void GetStationData(ISampleDataProvider dataProvider, BackgroundWorker worker, int[] pids)
        {
            Collection<TVStation> stations;

            if (!OptionEntry.IsDefined(OptionName.UseStoredStationInfo))
            {
                FrequencyScanner frequencyScanner = new FrequencyScanner(dataProvider, pids, true, worker);
                stations = frequencyScanner.FindTVStations();
                if (worker.CancellationPending)
                    return;
            }
            else
            {
                if (!stationCacheLoaded)
                {
                    stations = TVStation.Load(Path.Combine(RunParameters.DataDirectory, "Station Cache.xml"));
                    if (stations == null)
                        return;

                    setMHEG5Pid(dataProvider, stations);
                    stationCacheLoaded = true;
                }
                else
                {
                    setMHEG5Pid(dataProvider, RunParameters.Instance.StationCollection);
                    return;
                }
            }

            foreach (TVStation station in RunParameters.Instance.StationCollection)
                station.CurrentScan = false;

            foreach (TVStation tvStation in TVStation.GetNameSortedStations(stations))
            {
                bool include = checkChannelFilters(tvStation);

                TVStation existingStation = TVStation.FindStation(RunParameters.Instance.StationCollection, 
                    tvStation.OriginalNetworkID, tvStation.TransportStreamID, tvStation.ServiceID);                    

                if (include)
                {
                    if (existingStation == null)
                    {
                        tvStation.CollectionType = dataProvider.Frequency.CollectionType;
                        tvStation.CurrentScan = true;
                        bool added = TVStation.AddStation(RunParameters.Instance.StationCollection, tvStation);
                        if (added)
                            Logger.Instance.Write("Station: " + getStationDescription(tvStation));
                    }
                    else
                    {
                        if (!existingStation.ExcludedByUser)
                        {
                            existingStation.Update(tvStation);
                            existingStation.CurrentScan = true;
                            Logger.Instance.Write("Station: " + getStationDescription(tvStation));
                        }
                        else
                        {
                            if (!OptionEntry.IsDefined(OptionName.NoLogExcluded))
                                Logger.Instance.Write("Station: " + getStationDescription(tvStation) + " ** Excluded **");
                        }
                    }
                }
                else
                {
                    if (existingStation != null)
                        existingStation.ExcludedByFilter = true;

                    if (!OptionEntry.IsDefined(OptionName.NoLogExcluded))
                        Logger.Instance.Write("Station: " + getStationDescription(tvStation) + " ** Filtered Out **");
                }
            }

            Logger.Instance.Write("Station count now: " + RunParameters.Instance.StationCollection.Count);
        }

        private bool checkChannelFilters(TVStation station)
        {
            if (RunParameters.Instance.MaxService != -1 && station.ServiceID > RunParameters.Instance.MaxService)
                return (false);

            if (RunParameters.Instance.ChannelFilters.Count == 0)
                return (true);

            if (ChannelFilterEntry.IsFrequencyPresent(RunParameters.Instance.ChannelFilters))
            {
                ChannelFilterEntry filterEntry = ChannelFilterEntry.FindFrequencyEntry(RunParameters.Instance.ChannelFilters, station.Frequency, station.OriginalNetworkID, station.TransportStreamID, station.ServiceID);
                if (filterEntry != null)
                    return (true);

                filterEntry = ChannelFilterEntry.FindFrequencyEntry(RunParameters.Instance.ChannelFilters, station.Frequency, station.OriginalNetworkID, station.TransportStreamID);
                if (filterEntry != null)
                    return (true);

                filterEntry = ChannelFilterEntry.FindFrequencyEntry(RunParameters.Instance.ChannelFilters, station.Frequency, station.OriginalNetworkID);
                if (filterEntry != null)
                    return (true);                
            }
            else
            {
                ChannelFilterEntry filterEntry = ChannelFilterEntry.FindEntry(RunParameters.Instance.ChannelFilters, station.OriginalNetworkID, station.TransportStreamID, station.ServiceID);
                if (filterEntry != null)
                    return (true);

                filterEntry = ChannelFilterEntry.FindEntry(RunParameters.Instance.ChannelFilters, station.OriginalNetworkID, station.TransportStreamID);
                if (filterEntry != null)
                    return (true);

                filterEntry = ChannelFilterEntry.FindEntry(RunParameters.Instance.ChannelFilters, station.OriginalNetworkID);
                if (filterEntry != null)
                    return (true);
            }

            return (false);
        }

        private void setMHEG5Pid(ISampleDataProvider dataProvider, Collection<TVStation> stations)
        {
            if (dataProvider.Frequency.CollectionType != CollectionType.MHEG5)
                return;

            foreach (TVStation station in stations)
            {
                if (dataProvider.Frequency.DSMCCPid == 0)
                    dataProvider.Frequency.DSMCCPid = station.DSMCCPID;

                if (!station.ExcludedByUser && station.DSMCCPID != 0 && station.Frequency == dataProvider.Frequency.Frequency)
                    dataProvider.Frequency.DSMCCPid = station.DSMCCPID;
            }
        }

        private string getStationDescription(TVStation station)
        {
            StringBuilder description = new StringBuilder(station.FixedLengthName);

            description.Append(" (");

            description.Append(station.FullID);
            description.Append(" Type: " + station.ServiceType);
            description.Append(" " + (station.Encrypted ? "Encrypted" : "Clear"));

            string epg;

            if (station.NextFollowingAvailable)
            {
                if (station.ScheduleAvailable)
                    epg = "NN&S";
                else
                    epg = "NN";
            }
            else
            {
                if (station.ScheduleAvailable)
                    epg = "S";
                else
                    epg = "None";
            }

            description.Append(" EPG: " + epg);
            description.Append(")");

            if (station.EPGLink != null)
                description.Append(" EPG Link: " + station.EPGLink.OriginalNetworkID + "," +
                    station.EPGLink.TransportStreamID + "," +
                    station.EPGLink.ServiceID + " Time offset " + station.EPGLink.TimeOffset);

            
            return (description.ToString());
        }

        /// <summary>
        /// Get the network information data from the standard PID.
        /// </summary>
        /// <param name="dataProvider">The sample data provider.</param>
        /// <param name="worker">The BackgroundWorker instance running the collection.</param>
        protected void GetNetworkInformation(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            GetNetworkInformation(dataProvider, worker, new int[] { BDAGraph.NitPid });
        }

        /// <summary>
        /// Get the network information data.
        /// </summary>
        /// <param name="dataProvider">The sample data provider.</param>
        /// <param name="worker">The BackgroundWorker instance running the collection.</param>
        /// <param name="pids">The PID's to scan.</param>
        protected void GetNetworkInformation(ISampleDataProvider dataProvider, BackgroundWorker worker, int[] pids)
        {
            if (!TraceEntry.IsDefined(TraceName.Bda))
                Logger.Instance.Write("Collecting network information data from PID(s) " + getPidString(pids), false, true);
            else
                Logger.Instance.Write("Collecting network information data from PID(s) " + getPidString(pids));

            dataProvider.ChangePidMapping(pids);

            Collection<byte> tables = new Collection<byte>();
            tables.Add(0x40);
            tables.Add(0x41);
            nitReader = new TSStreamReader(tables, 50000, dataProvider.BufferAddress);
            nitReader.Run();

            bool done = false;
            int lastCount = 0;
            int repeats = 0;

            while (!done)
            {
                if (worker != null && worker.CancellationPending)
                {
                    nitReader.Stop();
                    return;
                }

                Thread.Sleep(2000);

                if (!TraceEntry.IsDefined(TraceName.Bda))
                    Logger.Instance.Write(".", false, false);
                else
                    Logger.Instance.Write("Buffer space used " + dataProvider.BufferSpaceUsed);

                Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();

                nitReader.Lock("ProcessNITSections");
                if (nitReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in nitReader.Sections)
                        sections.Add(section);
                    nitReader.Sections.Clear();
                }
                nitReader.Release("ProcessNITSections");

                foreach (Mpeg2Section section in sections)
                {
                    NetworkInformationSection networkInformationSection = NetworkInformationSection.ProcessNetworkInformationTable(section.Data);
                    if (networkInformationSection != null)
                        NetworkInformationSection.AddSection(networkInformationSection);
                }

                done = NetworkInformationSection.CheckAllLoaded();
                if (!done)
                {
                    if (NetworkInformationSection.NetworkInformationSections.Count == lastCount)
                    {
                        repeats++;
                        done = (repeats == 10);
                    }
                    else
                        repeats = 0;

                    lastCount = NetworkInformationSection.NetworkInformationSections.Count;
                }
            }

            if (!TraceEntry.IsDefined(TraceName.Bda))
                Logger.Instance.Write("", true, false);

            Logger.Instance.Write("Stopping network information reader for frequency " + dataProvider.Frequency);
            nitReader.Stop();

            Logger.Instance.Write("Processing network information sections");
            ProcessNetworkSections();

            Logger.Instance.Write("NIT sections: " + NetworkInformationSection.NetworkInformationSections.Count + 
                " channel count: " + Channel.Channels.Count +
                " buffer space used: " + dataProvider.BufferSpaceUsed +
                " discontinuities: " + nitReader.Discontinuities);
        }

        /// <summary>
        /// Process the network data.
        /// </summary>
        protected virtual void ProcessNetworkSections() 
        {
            foreach (NetworkInformationSection networkSection in NetworkInformationSection.NetworkInformationSections)
            {
                if (networkSection.TransportStreams != null)
                {
                    foreach (TransportStream transportStream in networkSection.TransportStreams)
                    {
                        if (transportStream.Descriptors != null)
                        {
                            NetworkMap networkMap = NetworkMap.FindMap(transportStream.OriginalNetworkID);
                            NetworkMapEntry networkMapEntry = networkMap.FindMapEntry(transportStream.TransportStreamID);
                            if (networkMapEntry.TuningFrequency == null)
                                networkMapEntry.TuningFrequency = createTuningFrequency(transportStream);

                            Collection<ServiceListEntry> serviceListEntries = transportStream.ServiceList;
                            if (serviceListEntries != null)
                            {
                                foreach (ServiceListEntry serviceListEntry in serviceListEntries)
                                {
                                    if (!networkMapEntry.ServiceIds.Contains(serviceListEntry.ServiceID))
                                        networkMapEntry.ServiceIds.Add(serviceListEntry.ServiceID);
                                }
                            }

                            foreach (DescriptorBase descriptor in transportStream.Descriptors)
                                processNetworkDescriptor(descriptor, transportStream);
                        }
                    }
                }
            }
        }

        private TuningFrequency createTuningFrequency(TransportStream transportStream)
        {
            if (transportStream.IsSatellite)
            {
                SatelliteFrequency frequency = new SatelliteFrequency();
                frequency.Frequency = transportStream.Frequency * 10;
                frequency.FEC = FECRate.ConvertDVBFecRate(transportStream.Fec);
                frequency.DVBModulation = transportStream.Modulation;
                frequency.Pilot = SignalPilot.Pilot.NotSet;
                frequency.DVBPolarization = transportStream.Polarization;

                if (transportStream.IsS2)
                    frequency.RollOff = SignalRollOff.ConvertDVBRollOff(transportStream.RollOff);
                else
                    frequency.RollOff = SignalRollOff.RollOff.NotSet;

                frequency.SymbolRate = transportStream.SymbolRate * 100;
                frequency.ModulationSystem = transportStream.ModulationSystem;

                return (frequency);
            }

            if (transportStream.IsTerrestrial)
            {
                TerrestrialFrequency frequency = new TerrestrialFrequency();
                frequency.Frequency = transportStream.Frequency * 10;

                switch (transportStream.Bandwidth)
                {
                    case 0:
                        frequency.Bandwidth = 8;
                        break;
                    case 1:
                        frequency.Bandwidth = 7;
                        break;
                    case 2:
                        frequency.Bandwidth = 6;
                        break;
                    case 3:
                        frequency.Bandwidth = 5;
                        break;
                    default:
                        frequency.Bandwidth = 8;
                        break;
                }
                
                return (frequency);
            }

            if (transportStream.IsCable)
            {
                CableFrequency frequency = new CableFrequency();
                frequency.Frequency = transportStream.Frequency * 10;
                frequency.FEC = FECRate.ConvertDVBFecRate(transportStream.CableFec);
                frequency.DVBModulation = transportStream.CableModulation;
                frequency.Modulation = getCableModulation(transportStream.CableModulation); 
                frequency.SymbolRate = transportStream.CableSymbolRate * 100;

                return (frequency);
            }

            return (null);
        }

        private static SignalModulation.Modulation getCableModulation(int cableModulation)
        {
            switch (cableModulation)
            {
                case 1:
                    return SignalModulation.Modulation.QAM16;
                case 2:
                    return SignalModulation.Modulation.QAM32;
                case 3:
                    return SignalModulation.Modulation.QAM64;
                case 4:
                    return SignalModulation.Modulation.QAM128;
                case 5:
                    return SignalModulation.Modulation.QAM256;
                default:
                    return SignalModulation.Modulation.QAM64;
            }
        }

        private void processNetworkDescriptor(DescriptorBase descriptor, TransportStream transportStream)
        {
            FreeviewChannelInfoDescriptor freeviewDescriptor = descriptor as FreeviewChannelInfoDescriptor;
            if (freeviewDescriptor != null)
            {
                processFreeviewDescriptor(freeviewDescriptor, transportStream);
                return;
            }
        }

        private void processFreeviewDescriptor(FreeviewChannelInfoDescriptor freeviewDescriptor, TransportStream transportStream)
        {
            if (freeviewDescriptor.ChannelInfoEntries != null)
            {
                foreach (FreeviewChannelInfoEntry channelInfoEntry in freeviewDescriptor.ChannelInfoEntries)
                {
                    Channel channel = new Channel();
                    channel.OriginalNetworkID = transportStream.OriginalNetworkID;
                    channel.TransportStreamID = transportStream.TransportStreamID;
                    channel.ServiceID = channelInfoEntry.ServiceID;
                    channel.UserChannel = channelInfoEntry.UserNumber;
                    Channel.AddChannel(channel);
                }
            }  
        }

        /// <summary>
        /// Get the bouquet data from the standard PID.
        /// </summary>
        /// <param name="dataProvider">The sample data provider.</param>
        /// <param name="worker">The BackgroundWorker instance running the collection.</param>
        protected void GetBouquetSections(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            GetBouquetSections(dataProvider, worker, new int[] { 0x11 } );
        }

        /// <summary>
        /// Get the bouquet data.
        /// </summary>
        /// <param name="dataProvider">The sample data provider.</param>
        /// <param name="worker">The BackgroundWorker instance running the collection.</param>
        /// <param name="pids">The PID's to scan.</param>
        protected void GetBouquetSections(ISampleDataProvider dataProvider, BackgroundWorker worker, int[] pids)
        {
            if (!TraceEntry.IsDefined(TraceName.Bda))
                Logger.Instance.Write("Collecting bouquet data from PID(s) " + getPidString(pids), false, true);
            else
                Logger.Instance.Write("Collecting bouquet data from PID(s) " + getPidString(pids));

            BouquetAssociationSection.BouquetAssociationSections.Clear();
            
            dataProvider.ChangePidMapping(pids);

            bouquetReader = new TSStreamReader(0x4a, 2000, dataProvider.BufferAddress);
            bouquetReader.Run();

            int lastCount = 0;
            int repeats = 0;
            int sectionCount = 0;

            bool bouquetSectionsDone = false;

            while (!bouquetSectionsDone)
            {
                if (worker.CancellationPending)
                    return;

                Thread.Sleep(2000);
                Logger.Instance.Write(".", false, false);

                Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();

                bouquetReader.Lock("ProcessOpenTVSections");
                if (bouquetReader.Sections.Count != 0)
                {
                    sectionCount++;
                    foreach (Mpeg2Section section in bouquetReader.Sections)
                        sections.Add(section);
                    bouquetReader.Sections.Clear();
                }
                bouquetReader.Release("ProcessOpenTVSections");

                if (sections.Count != 0)
                    storeBouquetSections(sections);

                if (BouquetAssociationSection.BouquetAssociationSections.Count == lastCount)
                {
                    repeats++;
                    bouquetSectionsDone = (repeats == RunParameters.Instance.Repeats);
                }
                else
                    repeats = 0;

                lastCount = BouquetAssociationSection.BouquetAssociationSections.Count;
            }

            if (!TraceEntry.IsDefined(TraceName.Bda))
                Logger.Instance.Write("", true, false);

            Logger.Instance.Write("Stopping bouquet reader for frequency " + dataProvider.Frequency);
            bouquetReader.Stop();

            Logger.Instance.Write("Processing bouquet sections");
            ProcessBouquetSections();

            Logger.Instance.Write("Section count: " + sectionCount + 
                " channel count: " + Channel.Channels.Count + 
                " buffer space used: " + dataProvider.BufferSpaceUsed + 
                " discontinuities: " + bouquetReader.Discontinuities);
        }

        private void storeBouquetSections(Collection<Mpeg2Section> sections) 
        {
            foreach (Mpeg2Section section in sections)
            {
                if (TraceEntry.IsDefined(TraceName.BouquetSections))
                    Logger.Instance.Dump("Bouquet Section", section.Data, section.Length);

                BouquetAssociationSection bouquetSection = BouquetAssociationSection.ProcessBouquetAssociationTable(section.Data);
                if (bouquetSection != null)
                    BouquetAssociationSection.AddSection(bouquetSection);
            }
        }

        /// <summary>
        /// Process the bouquet data.
        /// </summary>
        protected virtual void ProcessBouquetSections() { }

        /// <summary>
        /// Get the time offset data.
        /// </summary>
        /// <param name="dataProvider">The sample data provider.</param>
        /// <param name="worker">The BackgroundWorker instance running the collection.</param>
        protected void GetTimeOffsetSections(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            if (omitTimeZoneSections)
                return;

            if (RunParameters.Instance.TimeZoneSet)
            {
                TimeOffsetEntry.CurrentTimeOffset = RunParameters.Instance.TimeZone;
                Logger.Instance.Write("Local time offset set from Timezone ini parameter");
                omitTimeZoneSections = true;
                return;
            }

            if (!TraceEntry.IsDefined(TraceName.Bda))
                Logger.Instance.Write("Collecting time zone data from PID(s) 0x14", false, true);
            else
                Logger.Instance.Write("Collecting time zone data from PID(s) 0x14");

            dataProvider.ChangePidMapping(new int[] { 0x14 });

            timeOffsetReader = new TSStreamReader(0x73, 50000, dataProvider.BufferAddress);
            timeOffsetReader.Run();

            bool timeOffsetSectionsDone = false;

            while (!timeOffsetSectionsDone)
            {
                if (worker.CancellationPending)
                    return;

                Thread.Sleep(2000);
                Logger.Instance.Write(".", false, false);

                Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();

                timeOffsetReader.Lock("ProcessOpenTVSections");
                if (timeOffsetReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in timeOffsetReader.Sections)
                        sections.Add(section);
                    timeOffsetReader.Sections.Clear();
                }
                timeOffsetReader.Release("ProcessOpenTVSections");

                if (sections.Count != 0)
                    processTimeOffsetSections(sections);

                timeOffsetSectionsDone = (TimeOffsetEntry.TimeOffsets.Count != 0);
            }

            Logger.Instance.Write("", true, false);

            foreach (TimeOffsetEntry timeOffsetEntry in TimeOffsetEntry.TimeOffsets)
                Logger.Instance.Write("Time offset: " + timeOffsetEntry.CountryCode + " region " + timeOffsetEntry.Region +
                    " offset " + timeOffsetEntry.TimeOffset + " next offset: " + timeOffsetEntry.NextTimeOffset +
                    " date: " + timeOffsetEntry.ChangeTime);

            Logger.Instance.Write("Stopping time offset reader");
            timeOffsetReader.Stop();

            setTimeOffset();

            Logger.Instance.Write("Time zone count: " + TimeOffsetEntry.TimeOffsets.Count + 
                " buffer space used: " + dataProvider.BufferSpaceUsed +
                " discontinuities: " + timeOffsetReader.Discontinuities);
        }

        private void processTimeOffsetSections(Collection<Mpeg2Section> sections)
        {
            foreach (Mpeg2Section section in sections)
            {
                TimeOffsetSection timeOffsetSection = TimeOffsetSection.ProcessTimeOffsetTable(section.Data);
                if (timeOffsetSection != null)
                {
                    if (timeOffsetSection.Descriptors != null)
                    {
                        foreach (DescriptorBase descriptor in timeOffsetSection.Descriptors)
                        {
                            DVBLocalTimeOffsetDescriptor timeOffsetDescriptor = descriptor as DVBLocalTimeOffsetDescriptor;
                            if (timeOffsetDescriptor != null)
                            {
                                foreach (DVBLocalTimeOffsetEntry entry in timeOffsetDescriptor.TimeOffsetEntries)
                                {
                                    TimeOffsetEntry offsetEntry = new TimeOffsetEntry();
                                    offsetEntry.CountryCode = entry.CountryCode;
                                    offsetEntry.Region = entry.Region;

                                    if (entry.OffsetPositive)
                                    {
                                        offsetEntry.TimeOffset = entry.TimeOffset;
                                        offsetEntry.NextTimeOffset = entry.NextTimeOffset;
                                    }
                                    else
                                    {
                                        offsetEntry.TimeOffset = new TimeSpan() - entry.TimeOffset;
                                        offsetEntry.NextTimeOffset = new TimeSpan() - entry.NextTimeOffset;
                                    }

                                    offsetEntry.ChangeTime = entry.ChangeTime;

                                    TimeOffsetEntry.AddEntry(offsetEntry);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void setTimeOffset()
        { 
            if (TimeOffsetEntry.TimeOffsets.Count == 0)
            {
                TimeOffsetEntry.CurrentTimeOffset = new TimeSpan();
                TimeOffsetEntry.FutureTimeOffset = new TimeSpan();
                TimeOffsetEntry.TimeOfFutureTimeOffset = new DateTime();
                Logger.Instance.Write("No local time offset in effect");
                return;
            }

            if (TimeOffsetEntry.TimeOffsets.Count == 1)
            {
                TimeOffsetEntry.CurrentTimeOffset = TimeOffsetEntry.TimeOffsets[0].TimeOffset;
                TimeOffsetEntry.FutureTimeOffset = TimeOffsetEntry.TimeOffsets[0].NextTimeOffset;
                TimeOffsetEntry.TimeOfFutureTimeOffset = TimeOffsetEntry.TimeOffsets[0].ChangeTime;
                Logger.Instance.Write("Local time offset set to " + TimeOffsetEntry.TimeOffsets[0].TimeOffset +
                    " for country " + TimeOffsetEntry.TimeOffsets[0].CountryCode +
                    " region " + TimeOffsetEntry.TimeOffsets[0].Region);
                Logger.Instance.Write("Time offset will change to " + TimeOffsetEntry.TimeOffsets[0].NextTimeOffset +
                    " at " + TimeOffsetEntry.TimeOffsets[0].ChangeTime);
                return;
            }

            if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode != null)
            {
                TimeOffsetEntry offsetEntry = TimeOffsetEntry.FindEntry(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode, 
                    RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Region);
                if (offsetEntry != null)
                {
                    TimeOffsetEntry.CurrentTimeOffset = offsetEntry.TimeOffset;
                    TimeOffsetEntry.FutureTimeOffset = offsetEntry.NextTimeOffset;
                    TimeOffsetEntry.TimeOfFutureTimeOffset = offsetEntry.ChangeTime;
                    Logger.Instance.Write("Local time offset set to " + offsetEntry.TimeOffset +
                        " for country " + offsetEntry.CountryCode +
                        " region " + offsetEntry.Region);
                    Logger.Instance.Write("Time offset will change to " + offsetEntry.NextTimeOffset +
                    " at " + offsetEntry.ChangeTime);
                }
                else
                {
                    TimeOffsetEntry.CurrentTimeOffset = new TimeSpan();
                    TimeOffsetEntry.FutureTimeOffset = new TimeSpan();
                    TimeOffsetEntry.TimeOfFutureTimeOffset = new DateTime();
                    Logger.Instance.Write("No local time offset in effect");
                }
            }
            else
            {
                TimeOffsetEntry.CurrentTimeOffset = new TimeSpan();
                TimeOffsetEntry.FutureTimeOffset = new TimeSpan();
                TimeOffsetEntry.TimeOfFutureTimeOffset = new DateTime();
                Logger.Instance.Write("No local time offset in effect");
            }
        }

        private string getPidString(int[] pids)
        {
            StringBuilder pidString = new StringBuilder();
            
            foreach (int pid in pids)
            {
                if (pidString.Length != 0)
                    pidString.Append(", ");
                pidString.Append("0x" + pid.ToString("X"));
            }

            return (pidString.ToString());            
        }

        internal static bool CheckEPGDays(DateTime startTime)
        {
            if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.EPGDays == -1)
                return (true);

            return (startTime <= DateTime.Now.AddDays(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.EPGDays));
        }

        /// <summary>
        /// Log the collection progress.
        /// </summary>
        protected void LogProgress(string description, int currentSectionCount, int reportCount)
        {
            int currentReportGroup = currentSectionCount / reportCount;

            if (currentReportGroup != lastReportGroup)
            {
                Logger.Instance.Write(description + ": MPEG2 sections processed =  " + currentSectionCount);
                lastReportGroup = currentReportGroup;
            }
        }

        /// <summary>
        /// Stop the collection.
        /// </summary>
        public abstract void Stop();

        /// <summary>
        /// Carry out the processing necessary at the end of processing a frequency.
        /// </summary>
        public virtual void FinishFrequency() { }

        /// <summary>
        /// Carry out the processing necessary when all frequencies have been processed.
        /// </summary>
        public virtual void FinishRun() { }
    }
}
