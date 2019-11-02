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

using DomainObjects;
using DirectShow;

namespace DVBServices
{
    /// <summary>
    /// The class the describes the frequency scanner that locates stations.
    /// </summary>
    public class FrequencyScanner
    {
        /// <summary>
        /// Get or set whether to include stations on the other transport stream.
        /// </summary>
        public bool SearchOtherStream
        {
            get { return (searchOtherTable); }
            set { searchOtherTable = value; }
        }

        private const int serviceTypeDigitalTV = 1;
        private const int serviceTypeDigitalRadio = 2;
        private const int serviceTypeHDTV = 17;

        private const int runningStatusRunning = 4;

        private const int streamTypeVideo = 2;
        private const int streamTypeAudio = 4;
        private const int streamTypePrivate = 5;
        private const int streamTypeDSMCCUserToNetwork = 11;

        private const int dataBroadcastIdFreeview = 0x106;

        private ISampleDataProvider dataProvider;        

        private int[] pids;
        private bool searchOtherTable = true;
        private BackgroundWorker worker;

        private string caComment;

        /// <summary>
        /// Initialize a new instance of the FrequencyScanner class.
        /// </summary>
        /// <param name="dataProvider">A sample data provider.</param>
        public FrequencyScanner(ISampleDataProvider dataProvider)
        {
            this.dataProvider = dataProvider;

            int actualPid;
            if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.SDTPid == -1)
                actualPid = BDAGraph.SdtPid;
            else
                actualPid = RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.SDTPid;

            pids = new int[] { actualPid } ;
        }

        /// <summary>
        /// Initialize a new instance of the FrequencyScanner class.
        /// </summary>
        /// <param name="dataProvider">A sample data provider.</param>
        /// <param name="worker">An optional background worker instance. Can be null.</param>
        public FrequencyScanner(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            this.dataProvider = dataProvider;
            this.worker = worker;

            int actualPid;
            if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.SDTPid == -1)
                actualPid = BDAGraph.SdtPid;
            else
                actualPid = RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.SDTPid;

            pids = new int[] { actualPid };
        }

        /// <summary>
        /// Initialize a new instance of the FrequencyScanner.
        /// </summary>
        /// <param name="dataProvider">A sample data provider.</param>
        /// <param name="pids">A collection of PID's to be searched.</param>
        public FrequencyScanner(ISampleDataProvider dataProvider, int[] pids) : this(dataProvider)
        {
            this.pids = pids;
        }

        /// <summary>
        /// Initialize a new instance of the FrequencyScanner.
        /// </summary>
        /// <param name="dataProvider">A sample data provider.</param>
        /// <param name="pids">A collection of PID's to be searched.</param>
        /// <param name="searchOtherTable">True to include the 'other' stations; false otherwise</param>
        public FrequencyScanner(ISampleDataProvider dataProvider, int[] pids, bool searchOtherTable) : this(dataProvider, pids)
        {
            this.searchOtherTable = searchOtherTable;
        }

        /// <summary>
        /// Initialize a new instance of the FrequencyScanner.
        /// </summary>
        /// <param name="dataProvider">A sample data provider.</param>
        /// <param name="worker">An optional background worker instance. Can be null.</param>
        /// <param name="searchOtherTable">True to include the 'other' stations; false otherwise</param>
        public FrequencyScanner(ISampleDataProvider dataProvider, BackgroundWorker worker, bool searchOtherTable) : this(dataProvider, worker)
        {
            this.searchOtherTable = searchOtherTable;
        }

        /// <summary>
        /// Initialize a new instance of the FrequencyScanner.
        /// </summary>
        /// <param name="dataProvider">A sample data provider.</param>
        /// <param name="pids">A collection of PID's to be searched.</param>
        /// <param name="searchOtherTable">True to include the 'other' stations; false otherwise</param>
        /// <param name="worker">An optional background worker instance. Can be null.</param>
        public FrequencyScanner(ISampleDataProvider dataProvider, int[] pids, bool searchOtherTable, BackgroundWorker worker) : this(dataProvider, pids, searchOtherTable)
        {
            this.worker = worker;
        }

        /// <summary>
        /// Find TV stations.
        /// </summary>
        /// <returns>A collection of TV stations.</returns>
        public Collection<TVStation> FindTVStations()
        {
            if (dataProvider.Frequency.TunerType != TunerType.ATSC && dataProvider.Frequency.TunerType != TunerType.ATSCCable)
                return (findDvbStations());
            else
                return (findAtscStations());
        }

        private Collection<TVStation> findDvbStations()
        {
            dataProvider.ChangePidMapping(pids);

            StringBuilder pidString = new StringBuilder();
            foreach (int pid in pids)
            {
                if (pidString.Length != 0)
                    pidString.Append(", ");
                pidString.Append("0x" + pid.ToString("X"));
            }

            if (!TraceEntry.IsDefined(TraceName.Bda))
                Logger.Instance.Write("Collecting station data from PID(s) " + pidString, false, true);
            else
                Logger.Instance.Write("Collecting station data from PID(s) " + pidString);

            Collection<TVStation> tvStations = new Collection<TVStation>();            

            Collection<byte> tables = new Collection<byte>();
            tables.Add(BDAGraph.SdtTable);
            if (searchOtherTable)
                tables.Add(BDAGraph.SdtOtherTable);

            TSStreamReader stationReader = stationReader = new TSStreamReader(tables, 2000, dataProvider.BufferAddress);
            stationReader.Run();

            Collection<Mpeg2Section> sections = null;

            int lastCount = 0;
            int repeats = 0;
            bool done = false;

            while (!done)
            {
                if (worker != null && worker.CancellationPending)
                {
                    stationReader.Stop();
                    return (null);
                }

                Thread.Sleep(2000);

                if (!TraceEntry.IsDefined(TraceName.Bda))
                    Logger.Instance.Write(".", false, false);
                else
                    Logger.Instance.Write("BDA Buffer space used " + dataProvider.BufferSpaceUsed);
                
                sections = new Collection<Mpeg2Section>();

                stationReader.Lock("ProcessSDTSections");
                if (stationReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in stationReader.Sections)
                        sections.Add(section);
                    stationReader.Sections.Clear();
                }
                stationReader.Release("ProcessSDTSections");

                foreach (Mpeg2Section section in sections)
                {
                    ServiceDescriptionSection serviceDescriptionSection = ServiceDescriptionSection.ProcessServiceDescriptionTable(section.Data);
                    if (serviceDescriptionSection != null)
                        processServiceDescriptionSection(serviceDescriptionSection, tvStations, dataProvider.Frequency,
                            section.Table == BDAGraph.SdtTable);
                }

                if (tvStations.Count == lastCount)
                {
                    repeats++;
                    done = (repeats == RunParameters.Instance.Repeats);
                }
                else
                    repeats = 0;

                lastCount = tvStations.Count;
            }

            if (!TraceEntry.IsDefined(TraceName.Bda))
                Logger.Instance.Write("", true, false);

            Logger.Instance.Write("Stopping station reader for frequency " + dataProvider.Frequency);
            stationReader.Stop();

            Logger.Instance.Write("Buffer space used: " + dataProvider.BufferSpaceUsed +
                " discontinuities: " + stationReader.Discontinuities);

            if ((dataProvider.Frequency.CollectionType == CollectionType.MHEG5 || (DebugEntry.IsDefined(DebugName.UpdateChannels)) && tvStations.Count != 0))
            {
                int debugPid = -1;

                DebugEntry debugEntry = DebugEntry.FindEntry(DebugName.Mheg5Pid, true);
                if (debugEntry != null)
                    debugPid = debugEntry.Parameter;

                if (debugPid == -1)
                {
                    processPATSections(tvStations);

                    foreach (TVStation station in tvStations)
                    {
                        if (!station.ExcludedByUser && station.DSMCCPID != 0)
                        {
                            if (dataProvider.Frequency.DSMCCPid != 0 && station.DSMCCPID != dataProvider.Frequency.DSMCCPid)
                                Logger.Instance.Write("Multiple MHEG5 pids - " + station.Name + " specifies " + station.DSMCCPID.ToString("X"));  

                            dataProvider.Frequency.DSMCCPid = station.DSMCCPID;
                        }
                    }
                }
                else
                    dataProvider.Frequency.DSMCCPid = debugPid;
            }

            Logger.Instance.Write("Station count: " + tvStations.Count);   

            return (tvStations);
        }

        private void processPATSections(Collection<TVStation> tvStations)
        {
            dataProvider.ChangePidMapping(new int[] { BDAGraph.PatPid } );

            if (!TraceEntry.IsDefined(TraceName.Bda))
                Logger.Instance.Write("Collecting PAT data from PID 0x" + BDAGraph.PatPid.ToString("x2"), false, true);
            else
                Logger.Instance.Write("Collecting PAT data from PID 0x" + BDAGraph.PatPid.ToString("x2"));

            TSReaderBase patReader = new TSStreamReader(BDAGraph.PatTable, 2000, dataProvider.BufferAddress);
            patReader.Run();

            Collection<ProgramAssociationSection> programAssociationSections = new Collection<ProgramAssociationSection>();
            Collection<int> sectionNumbers = new Collection<int>();

            Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();

            bool done = false;
            int lastCount = 0;
            int repeats = 0;
            int lastSectionNumber = Int16.MinValue;

            while (!done)
            {
                if (worker != null && worker.CancellationPending)
                {
                    patReader.Stop();
                    return;
                }

                Thread.Sleep(2000);

                if (!TraceEntry.IsDefined(TraceName.Bda))
                    Logger.Instance.Write(".", false, false);
                else
                    Logger.Instance.Write("BDA Buffer space used " + dataProvider.BufferSpaceUsed);

                sections.Clear();

                patReader.Lock("ProcessPATSections");
                if (patReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in patReader.Sections)
                        sections.Add(section);
                    patReader.Sections.Clear();
                }
                patReader.Release("ProcessPATSections");

                foreach (Mpeg2Section section in sections)
                {                    
                    ProgramAssociationSection programAssociationSection = ProgramAssociationSection.ProcessProgramAssociationTable(section.Data);
                    if (programAssociationSection != null)
                    {
                        lastSectionNumber = programAssociationSection.LastSectionNumber;

                        if (!sectionNumbers.Contains(programAssociationSection.SectionNumber))
                        {
                            programAssociationSections.Add(programAssociationSection);
                            sectionNumbers.Add(programAssociationSection.SectionNumber);
                        }
                    }
                }

                done = sectionNumbers.Count == lastSectionNumber + 1;

                if (!done)
                {
                    if (sectionNumbers.Count == lastCount)
                    {
                        repeats++;
                        done = (repeats == RunParameters.Instance.Repeats);
                    }
                    else
                        repeats = 0;

                    lastCount = sectionNumbers.Count;
                }
            }

            if (!TraceEntry.IsDefined(TraceName.Bda))
                Logger.Instance.Write("", true, false);

            Logger.Instance.Write("Stopping PAT reader for frequency " + dataProvider.Frequency);
            patReader.Stop();

            Logger.Instance.Write("PAT sections processed: " + sectionNumbers.Count +
                " buffer space used: " + dataProvider.BufferSpaceUsed +
                " discontinuities: " + patReader.Discontinuities);

            if (sectionNumbers.Count == 0)
                return;

            Collection<ProgramInfo> programInfos = new Collection<ProgramInfo>();

            foreach (ProgramAssociationSection programAssociationSection in programAssociationSections)
            {
                if (programAssociationSection.ProgramInfos != null)
                {
                    foreach (ProgramInfo programInfo in programAssociationSection.ProgramInfos)
                    {
                        if (programInfo.ProgramNumber != 0)
                            addProgramInfo(programInfo, programInfos);
                    }
                }
            }

            processPMTSections(programInfos, tvStations);
        }

        private void addProgramInfo(ProgramInfo newProgramInfo, Collection<ProgramInfo> programInfos)
        {
            foreach (ProgramInfo oldProgramInfo in programInfos)
            {
                if (oldProgramInfo.ProgramID == newProgramInfo.ProgramID)
                    return;
            }

            programInfos.Add(newProgramInfo);
        }

        private void processServiceDescriptionSection(ServiceDescriptionSection serviceDescriptionSection, Collection<TVStation> tvStations, TuningFrequency frequency, bool thisFrequency)
        {
            foreach (ServiceDescription serviceDescription in serviceDescriptionSection.ServiceDescriptions)
            {
                bool processStation = checkServiceInfo(serviceDescription);

                if (processStation)
                {
                    TVStation tvStation = new TVStation(serviceDescription.ServiceName);
                    tvStation.ProviderName = serviceDescription.ProviderName;
                    tvStation.Frequency = frequency.Frequency;
                    tvStation.OriginalNetworkID = serviceDescriptionSection.OriginalNetworkID;
                    tvStation.TransportStreamID = serviceDescriptionSection.TransportStreamID;
                    tvStation.ServiceID = serviceDescription.ServiceID;
                    tvStation.Encrypted = serviceDescription.Scrambled;
                    tvStation.ServiceType = serviceDescription.ServiceType;
                    tvStation.ScheduleAvailable = serviceDescription.EITSchedule;
                    tvStation.NextFollowingAvailable = serviceDescription.EITPresentFollowing;

                    tvStation.TunerType = frequency.TunerType;
                    if (frequency.TunerType == TunerType.Satellite)
                    {
                        Satellite satellite = ((SatelliteFrequency)frequency).Provider as Satellite;
                        if (satellite != null)
                            tvStation.Satellite = satellite;
                    }

                    if (OptionEntry.IsDefined(OptionName.UseChannelId))
                    {
                        if (serviceDescription.ChannelNumber != -1)
                            tvStation.OriginalChannelNumber = serviceDescription.ChannelNumber;
                    }

                    tvStation.LogicalChannelNumber = serviceDescription.CableChannelNumber;

                    if (thisFrequency)
                        tvStation.ActualFrequency = frequency.Frequency;

                    tvStation.EPGLink = serviceDescription.EPGLink;

                    if (serviceDescription.HasEpgLinkage)
                    {
                        tvStation.EpgLinkageOnid = serviceDescription.EpgLinkageOnid;
                        tvStation.EpgLinkageTsid = serviceDescription.EpgLinkageTsid;
                        tvStation.EpgLinkageSid = serviceDescription.EpgLinkageSid;
                    }

                    if (DebugEntry.IsDefined(DebugName.LogChannelGroups))
                    {
                        ChannelGroupEntry groupEntry = new ChannelGroupEntry(serviceDescription.ServiceName);
                        groupEntry.OriginalNetworkId = serviceDescriptionSection.OriginalNetworkID;
                        groupEntry.TransportStreamId = serviceDescriptionSection.TransportStreamID;
                        groupEntry.ServiceId = serviceDescription.ServiceID;
                        ChannelGroup.AddEntry(serviceDescription.OpenTVChannelGroup, groupEntry);
                    }

                    addStation(tvStations, tvStation);
                }
            }
        }

        private bool checkServiceInfo(ServiceDescription serviceDescription)
        {
            if (serviceDescription.ServiceType == 0)
                return (false);

            if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options, OptionName.ProcessAllStations))
                return (true);

            return (serviceDescription.ServiceType != 0x0c);
        }

        private void addStation(Collection<TVStation> tvStations, TVStation newStation)
        {
            foreach (TVStation oldStation in tvStations)
            {
                if (oldStation.OriginalNetworkID == newStation.OriginalNetworkID &&
                    oldStation.TransportStreamID == newStation.TransportStreamID &&
                    oldStation.ServiceID == newStation.ServiceID)
                {
                    oldStation.Frequency = newStation.Frequency;
                    if (oldStation.ActualFrequency == -1)
                        oldStation.ActualFrequency = newStation.ActualFrequency;
                    oldStation.Name = newStation.Name;
                    
                    return;
                }
            }

            tvStations.Add(newStation);
        }

        private void processPMTSections(Collection<ProgramInfo> programInfos, Collection<TVStation> tvStations)
        {
            int[] pids = new int[programInfos.Count];
            
            int index = 0;
            foreach (ProgramInfo programInfo in programInfos)
            {
                pids[index] = programInfo.ProgramID;
                index++;
            }

            dataProvider.ChangePidMapping(pids);

            if (!TraceEntry.IsDefined(TraceName.Bda))
                Logger.Instance.Write("Collecting PMT data from " + programInfos.Count + " pids", false, true);
            else
                Logger.Instance.Write("Collecting PMT data from " + programInfos.Count + " pids");

            TSReaderBase pmtReader = new TSStreamReader(BDAGraph.PmtTable, 2000, dataProvider.BufferAddress);
            pmtReader.Run();
            
            Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();

            int repeats = 0;
            bool done = false;
            Collection<int> pidsProcessed = new Collection<int>();

            while (!done)
            {
                Thread.Sleep(2000);

                if (!TraceEntry.IsDefined(TraceName.Bda))
                    Logger.Instance.Write(".", false, false);
                else
                    Logger.Instance.Write("BDA Buffer space used " + dataProvider.BufferSpaceUsed);
                                                                
                pmtReader.Lock("ProcessPMTSections");
                if (pmtReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in pmtReader.Sections)
                        sections.Add(section);                        
                    pmtReader.Sections.Clear();
                }
                pmtReader.Release("ProcessPMTSections");

                int processedCount = 0;

                foreach (Mpeg2Section section in sections)
                {
                    if (!pidsProcessed.Contains(section.PID))
                    {
                        foreach (ProgramInfo programInfo in programInfos)
                        {
                            if (programInfo.ProgramID == section.PID)
                            {
                                ProgramMapSection programMapSection = ProgramMapSection.ProcessProgramMapTable(section.Data);
                                processProgramMapSection(programMapSection, programInfo, tvStations);
                                processedCount++;
                            }
                        }

                        pidsProcessed.Add(section.PID);
                    }
                }

                done = pidsProcessed.Count == programInfos.Count;

                if (!done)
                {
                    if (processedCount == 0)
                    {
                        repeats++;
                        done = (repeats == RunParameters.Instance.Repeats);
                    }
                }
            }

            if (!TraceEntry.IsDefined(TraceName.Bda))
                Logger.Instance.Write("", true, false);

            Logger.Instance.Write("Stopping PMT reader for frequency " + dataProvider.Frequency);
            pmtReader.Stop();

            Logger.Instance.Write("PMT sections processed: " + pidsProcessed.Count +
                " buffer space used: " + dataProvider.BufferSpaceUsed +
                " discontinuities: " + pmtReader.Discontinuities);
        }

        private bool processProgramMapSection(ProgramMapSection programMapSection, ProgramInfo programInfo, Collection<TVStation> tvStations)
        {
            TVStation tvStation = findTVStation(programInfo.ProgramNumber, tvStations);
            if (tvStation == null)
                return (false);

            if (DebugEntry.IsDefined(DebugName.LogPmt))
                Logger.Instance.Write("PMT found for service ID " + tvStation.ServiceID + " " + (programMapSection.StreamInfos != null ? programMapSection.StreamInfos.Count + " stream info entries" : "No stream info"));

            caComment = string.Empty;

            foreach (StreamInfo streamInfo in programMapSection.StreamInfos)
            {
                if (DebugEntry.IsDefined(DebugName.LogStreamInfo))
                    Logger.Instance.Write("Stream info found for service ID " + tvStation.ServiceID + " stream type " + streamInfo.StreamType + " PID 0x" + streamInfo.ElementaryPid.ToString("X"));

                if (streamInfo.StreamType == streamTypeVideo)
                    tvStation.VideoPID = streamInfo.ElementaryPid;
                if (streamInfo.StreamType == streamTypeAudio)
                    tvStation.AudioPID = streamInfo.ElementaryPid;
                if (streamInfo.StreamType == streamTypeDSMCCUserToNetwork)
                {
                    bool use = checkDataBroadcastId(streamInfo);
                    if (use)
                    {
                        tvStation.DSMCCPID = streamInfo.ElementaryPid;

                        if (DebugEntry.IsDefined(DebugName.LogStreamInfo))
                            Logger.Instance.Write("MHEG5 PID set to 0x" + tvStation.DSMCCPID.ToString("X"));
                    }
                    else
                    {
                        if (DebugEntry.IsDefined(DebugName.LogStreamInfo))
                            Logger.Instance.Write("MHEG5 PID not set");
                    }

                }

                if (streamInfo.Descriptors != null)
                {
                    foreach (DescriptorBase descriptor in streamInfo.Descriptors)
                    {
                        DVBCADescriptor caDescriptor = descriptor as DVBCADescriptor;
                        if (caDescriptor != null)
                        {
                            if (tvStation.ConditionalAccessEntries == null)
                                tvStation.ConditionalAccessEntries = new Collection<ConditionalAccessEntry>();

                            bool duplicate = checkCADuplicates(caDescriptor, tvStation.ConditionalAccessEntries);
                            if (!duplicate)
                            {
                                ConditionalAccessEntry accessEntry = new ConditionalAccessEntry(caDescriptor.SystemID, caDescriptor.Pid);
                                tvStation.ConditionalAccessEntries.Add(accessEntry);

                                caComment = "CA entries loaded = " + tvStation.ConditionalAccessEntries.Count;
                            }
                        }
                    }
                }
            }

            if (programMapSection.Descriptors != null)
            {
                foreach (DescriptorBase descriptor in programMapSection.Descriptors)
                {
                    DVBCADescriptor caDescriptor = descriptor as DVBCADescriptor;
                    if (caDescriptor != null)
                    {
                        if (tvStation.ConditionalAccessEntries == null)
                            tvStation.ConditionalAccessEntries = new Collection<ConditionalAccessEntry>();

                        bool duplicate = checkCADuplicates(caDescriptor, tvStation.ConditionalAccessEntries);
                        if (!duplicate)
                        {
                            ConditionalAccessEntry accessEntry = new ConditionalAccessEntry(caDescriptor.SystemID, caDescriptor.Pid);
                            tvStation.ConditionalAccessEntries.Add(accessEntry);

                            caComment = "CA entries loaded = " + tvStation.ConditionalAccessEntries.Count;
                        }
                    }
                }
            }

            return (true);
        }

        private bool checkDataBroadcastId(StreamInfo streamInfo)
        {
            if (streamInfo.Descriptors == null)
                return (false);

            int idDescriptors = 0;

            foreach (DescriptorBase descriptor in streamInfo.Descriptors)
            {
                DVBDataBroadcastIdDescriptor dataBroadcastIdDescriptor = descriptor as DVBDataBroadcastIdDescriptor;
                if (dataBroadcastIdDescriptor != null)
                {
                    idDescriptors++;

                    bool reply = dataBroadcastIdDescriptor.DataBroadcastId == dataBroadcastIdFreeview;

                    if (DebugEntry.IsDefined(DebugName.LogStreamInfo))
                        Logger.Instance.Write("Data broadcast ID 0x" + dataBroadcastIdDescriptor.DataBroadcastId.ToString("X") + " " +
                            (reply ? "used" : "ignored"));

                    if (reply)
                        return (true);                    
                }
            }

            if (idDescriptors == 0)
            {
                if (DebugEntry.IsDefined(DebugName.LogStreamInfo))
                    Logger.Instance.Write("No data broadcast ID descriptors found");
            }

            return (false);
        }

        private bool checkCADuplicates(DVBCADescriptor caDescriptor, Collection<ConditionalAccessEntry> entries)
        {
            foreach (ConditionalAccessEntry entry in entries)
            {
                if (entry.PID == caDescriptor.Pid && entry.SystemID == caDescriptor.SystemID)
                    return (true);
            }

            return (false);
        }

        private TVStation findTVStation(int serviceID, Collection<TVStation> tvStations)
        {
            foreach (TVStation tvStation in tvStations)
            {
                if (tvStation.ServiceID == serviceID)
                    return (tvStation);
            }

            return (null);
        }

        private Collection<TVStation> findAtscStations()
        {
            Logger.Instance.Write("Collecting ATSC Channel data", false, true);

            Collection<TVStation> tvStations = new Collection<TVStation>();

            VirtualChannelTable.Clear();

            dataProvider.ChangePidMapping(new int[] { 0x1ffb });

            Collection<byte> tables = new Collection<byte>();
            tables.Add(0xc8);
            tables.Add(0xc9);
            TSStreamReader guideReader = new TSStreamReader(tables, 50000, dataProvider.BufferAddress);
            guideReader.Run();

            int repeats = 0;
            bool done = false;

            while (!done)
            {
                if (worker.CancellationPending)
                    return (tvStations);

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

            foreach (VirtualChannel channel in VirtualChannelTable.Channels)
            {
                TVStation station = new TVStation(channel.ShortName);
                station.StationType = TVStationType.Atsc;
                station.OriginalNetworkID = channel.CollectionFrequency;
                station.TransportStreamID = channel.MajorChannelNumber;
                station.ServiceID = channel.MinorChannelNumber;
                tvStations.Add(station);
            }

            return (tvStations);
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
                    }
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Logger.Instance.Write("PSIP error: " + e.Message);
                }
            }
        }
    }
}
