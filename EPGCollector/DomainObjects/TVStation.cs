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
using System.ComponentModel;
using System.Xml;
using System.IO;
using System.Text;
using System.Globalization;

namespace DomainObjects
{
    /// <summary>
    /// The class that describes a TV Station.
    /// </summary>
    public class TVStation : INotifyPropertyChanged
    {
        /// <summary>
        /// The property changed event.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Get the total number of EPG entries.
        /// </summary>
        public static int TotalEpgCount { get; private set; }

        /// <summary>
        /// Get or set the collection of EPG entries for the station.
        /// </summary>
        public Collection<EPGEntry> EPGCollection
        {
            get
            {
                if (epgCollection == null)
                    epgCollection = new Collection<EPGEntry>();
                return (epgCollection);
            }
            set { epgCollection = value; }
        }

        /// <summary>
        /// Get or set the name of the station. 
        /// <remarks>
        /// Stations in the excluded list have a dummy name as they are identified by service ID.
        /// </remarks>
        /// </summary>
        public string Name
        {
            get { return (name); }
            set { name = value; }
        }

        /// <summary>
        /// Get or set the fixed length name of the station. 
        /// <remarks>
        /// Stations in the excluded list have a dummy name as they are identified by the full ID.
        /// </remarks>
        /// </summary>
        public string FixedLengthName
        {
            get 
            { 
                if (name.Length > 16)
                    return(name.Substring(0, 16));
                else
                    return (name.PadRight(16, ' ')); }
            
        }

        /// <summary>
        /// Get or set the frequency the station transmits on.
        /// </summary>
        public int Frequency 
        {
            get { return (frequency); }
            set { frequency = value; }
        }

        /// <summary>
        /// Get or set the actual frequency the station transmits on.
        /// </summary>
        public int ActualFrequency
        {
            get { return (actualFrequency); }
            set { actualFrequency = value; }
        }

        /// <summary>
        /// Get or set the transport stream ID of the station.
        /// </summary>
        public int TransportStreamID
        {
            get { return (transportStreamID); }
            set { transportStreamID = value; }
        }

        /// <summary>
        /// Get or set the original network ID of the station.
        /// </summary>
        public int OriginalNetworkID
        {
            get { return (originalNetworkID); }
            set { originalNetworkID = value; }
        }

        /// <summary>
        /// Get or set the Service ID of the station.
        /// </summary>
        public int ServiceID
        {
            get { return (serviceID); }
            set { serviceID = value; }
        }

        /// <summary>
        /// Get or set the video stream PID of the station.
        /// </summary>
        public int VideoPID
        {
            get { return (videoPID); }
            set { videoPID = value; }
        }

        /// <summary>
        /// Get ot set the audio PID of the station.
        /// </summary>
        public int AudioPID
        {
            get { return (audioPID); }
            set { audioPID = value; }
        }

        /// <summary>
        /// Get or set the MHEG PID of the station.
        /// <remarks>
        /// The PID will be zero if the station does not transmit MHEG information.
        /// </remarks>
        /// </summary>
        public int DSMCCPID
        {
            get { return (dsmccPID); }
            set { dsmccPID = value; }
        }

        /// <summary>
        /// Get or set whether the broadcast is encrypted.
        /// </summary>
        public bool Encrypted
        {
            get { return (encrypted); }
            set { encrypted = value; }
        }

        /// <summary>
        /// Get or set the service type of the station (eg digital television or radio).
        /// </summary>
        public int ServiceType
        {
            get { return (serviceType); }
            set { serviceType = value; }
        }

        /// <summary>
        /// Get or set whether the station broadcasts the EPG schedule.
        /// </summary>
        public bool ScheduleAvailable
        {
            get { return (scheduleAvailable); }
            set { scheduleAvailable = value; }
        }

        /// <summary>
        /// Get or set whether the station broadcasts the next/following EPG information.
        /// </summary>
        public bool NextFollowingAvailable
        {
            get { return (nextFollowingAvailable); }
            set { nextFollowingAvailable = value; }
        }

        /// <summary>
        /// Get the map of EIT section received.
        /// </summary>
        public Collection<SectionMapEntry> SectionMap
        {
            get
            {
                if (sectionMap == null)
                    sectionMap = new Collection<SectionMapEntry>();
                return (sectionMap);
            }
        }

        /// <summary>
        /// Get whether all the EIT EPG data for the station has been received.
        /// </summary>
        public bool EITCompleted 
        { 
            get 
            {
                if (!eitCompleted)
                    eitCompleted = checkStationComplete();
                return (eitCompleted);
            } 
        }

        /// <summary>
        /// Get or set the type of collection for this station.
        /// </summary>
        public CollectionType CollectionType
        {
            get { return (collectionType); }
            set { collectionType = value; }
        }

        /// <summary>
        /// Get or set the provider name of the station. 
        /// </summary>
        public string ProviderName
        {
            get { return (providerName); }
            set { providerName = value; }
        }

        /// <summary>
        /// Get or set the tuner type the station is located on. 
        /// </summary>
        public TunerType TunerType
        {
            get { return (tunerType); }
            set { tunerType = value; }
        }

        /// <summary>
        /// Get or set the satellite the station is located on. 
        /// </summary>
        public Satellite Satellite
        {
            get { return (satellite); }
            set { satellite = value; }
        }

        /// <summary>
        /// Get or set the new name for the station. 
        /// </summary>
        public string NewName
        {
            get { return (newName); }
            set { newName = value; }
        }

        /// <summary>
        /// Get or set the logical channel number of the station. 
        /// </summary>
        public int LogicalChannelNumber
        {
            get { return (logicalChannelNumber); }
            set { logicalChannelNumber = value; }
        }

        /// <summary>
        /// Get or set the displayed logical channel number of the station. 
        /// </summary>
        public string DisplayedLogicalChannelNumber
        {
            get 
            {
                if (LogicalChannelNumber != -1)
                    return (logicalChannelNumber.ToString(CultureInfo.InvariantCulture));
                else
                    return(string.Empty);
            }
            set 
            {
                if (String.IsNullOrWhiteSpace(value))
                    logicalChannelNumber = -1;
                else
                    logicalChannelNumber = Int32.Parse(value, CultureInfo.InvariantCulture); 
            }
        }

        /// <summary>
        /// Get or set the minor channel number of the station. 
        /// </summary>
        public int MinorChannelNumber
        {
            get { return (minorChannelNumber); }
            set { minorChannelNumber = value; }
        }

        /// <summary>
        /// Get or set the original broadcast channel number. 
        /// </summary>
        public int OriginalChannelNumber 
        {
            get { return (originalChannelNumber); }
            set { originalChannelNumber = value; }
        }

        /// <summary>
        /// Get or set the alphanumeric channel ID. 
        /// </summary>
        public string ChannelID
        {
            get { return (channelID); }
            set { channelID = value; }
        }

        /// <summary>
        /// Get or set the station type. 
        /// </summary>
        public TVStationType StationType
        {
            get { return (stationType); }
            set { stationType = value; }
        }

        /// <summary>
        /// Get or set the Windows Media Centre match name. 
        /// </summary>
        public string WMCMatchName
        {
            get { return (wmcMatchName); }
            set { wmcMatchName = value; }
        }

        /// <summary>
        /// Get or set the Windows Media Centre unique ID. 
        /// </summary>
        public string WMCUniqueID
        {
            get { return (wmcUniqueID); }
            set { wmcUniqueID = value; }
        }

        /// <summary>
        /// Get or set the list of conditional access entries for this station.
        /// </summary>
        public Collection<ConditionalAccessEntry> ConditionalAccessEntries
        {
            get { return (conditionalAccessEntries); }
            set { conditionalAccessEntries = value; }
        }

        /// <summary>
        /// Get the full identification string for the station. 
        /// </summary>
        public string FullID { get { return (originalNetworkID + "," + transportStreamID + "," + serviceID); } }

        /// <summary>
        /// Get the full description of a station.
        /// </summary>
        public string FullDescription { get { return (Name + "(" + FullID + ")"); } }

        /// <summary>
        /// Get the full fixed length description of a station.
        /// </summary>
        public string FullFixedLengthDescription { get { return (FixedLengthName + "(" + FullID + ")"); } }

        /// <summary>
        /// Return true if the station broadcasts digital radio; false otherwise.
        /// </summary>
        public bool IsRadio { get { return (ServiceType == 2 || ServiceType == 10); } }

        /// <summary>
        /// Get or set the redirected EPG link;
        /// </summary>
        public EPGLink EPGLink
        {
            get { return (epgLink); }
            set { epgLink = value; }
        }

        /// <summary>
        /// Get or set the EPG linkage original network ID.
        /// </summary>
        public int EpgLinkageOnid { get; set; }
        /// <summary>
        /// Get or set the EPG linkage transport stream ID.
        /// </summary>
        public int EpgLinkageTsid { get; set; }
        /// <summary>
        /// Get or set the EPG linkage service ID.
        /// </summary>
        public int EpgLinkageSid { get; set; }

        /*/// <summary>
        /// Get or set the AIT PID for this service.
        /// </summary>
        public int AitPid
        {
            get { return (aitPid); }
            set { aitPid = value; }
        }*/

        /// <summary>
        /// Get or set whether the station has been excluded by the user.
        /// </summary>
        public bool ExcludedByUser
        {
            get { return (excludedByUser); }
            set { excludedByUser = value; }
        }

        /// <summary>
        /// Get or set whether the station has been excluded by a filter.
        /// </summary>
        public bool ExcludedByFilter
        {
            get { return (excludedByFilter); }
            set { excludedByFilter = value; }
        }

        /// <summary>
        /// Get or set whether the station has been excluded by bouquet.
        /// </summary>
        public bool ExcludedByChannel
        {
            get { return (excludedByChannel); }
            set { excludedByChannel = value; }
        }

        /// <summary>
        /// Return true if the station is not excluded and is filtered in if filters are used; false otherwise.
        /// </summary>
        public bool Included
        {
            get
            {
                if (ExcludedByUser || ExcludedByFilter || ExcludedByChannel)
                    return (false);

                return (true);
            }
        }

        /// <summary>
        /// Get or set whether the station was created from the ini file.
        /// </summary>
        public bool CreatedFromIni
        {
            get { return (createdFromIni); }
            set { createdFromIni = value; }
        }

        /// <summary>
        /// Get or set whether the station was present in the current channel scan.
        /// </summary>
        public bool CurrentScan
        {
            get { return (currentScan); }
            set { currentScan = value; }
        }

        /// <summary>
        /// Get or set whether the channel name is to be used when merging external XMLTV data.
        /// </summary>
        public bool UseNameForMerge
        {
            get { return (currentScan); }
            set { currentScan = value; }
        }

        private string name;
        private int frequency = -1;
        private int actualFrequency = -1;
        private int serviceID = -1;
        private int originalNetworkID = -1;
        private int transportStreamID = -1;
        private int videoPID;
        private int audioPID;
        private int dsmccPID;
        private bool encrypted;
        private int serviceType;
        private bool scheduleAvailable;
        private bool nextFollowingAvailable;
        private CollectionType collectionType;
        private string providerName;
        
        private bool excludedByUser;
        private bool excludedByFilter;
        private bool excludedByChannel;

        private bool createdFromIni;
        private bool currentScan;
                
        private string newName;        
        private int logicalChannelNumber = -1;
        private int minorChannelNumber = -1;
        private TunerType tunerType;
        private Satellite satellite;
        private int originalChannelNumber = -1;
        private string channelID;
        private TVStationType stationType = TVStationType.Dvb;
        private string wmcMatchName;
        private string wmcUniqueID;
        private Collection<ConditionalAccessEntry> conditionalAccessEntries;
        
        private bool eitCompleted;

        private EPGLink epgLink;

        private Collection<EPGEntry> epgCollection;

        private Collection<SectionMapEntry> sectionMap;
        private int highestTable;

        /// <summary>
        /// Initialize a new instance of the TVStation class with the station name.
        /// </summary>
        /// <param name="name">The name of the station.</param>
        public TVStation(string name) 
        {
            this.name = name;
        }

        /// <summary>
        /// Get a string representing this instance.
        /// </summary>
        /// <returns>A description of this instance.</returns>
        public override string ToString()
        {
            return (name);
        }

        /// <summary>
        /// Add an entry to the EIT map table.
        /// </summary>
        /// <remarks>
        /// All parameters are extracted from the DVB protocol that delivers the EIT section.
        /// </remarks>
        /// <param name="tableID">The table ID of the new entry.</param>
        /// <param name="sectionNumber">The section number of the new entry.</param>
        /// <param name="lastTableID">The last table ID of the new entry.</param>
        /// <param name="lastSectionNumber">The last section number of the new entry.</param>
        /// <param name="segmentLastSectionNumber">The last section number of the segment for the new entry.</param>
        /// <returns></returns>
        public bool AddMapEntry(int tableID, int sectionNumber, int lastTableID, int lastSectionNumber, int segmentLastSectionNumber)
        {
            SectionMapEntry newEntry = new SectionMapEntry(tableID, sectionNumber, lastTableID, lastSectionNumber, segmentLastSectionNumber);

            foreach (SectionMapEntry oldEntry in SectionMap)
            {
                if (oldEntry.TableID == newEntry.TableID && oldEntry.SectionNumber == newEntry.SectionNumber)
                    return (false);

                if (oldEntry.TableID == newEntry.TableID)
                {
                    if (oldEntry.SectionNumber > newEntry.SectionNumber)
                    {
                        sectionMap.Insert(sectionMap.IndexOf(oldEntry), newEntry);
                        return (true);
                    }
                }
                else
                {
                    if (oldEntry.TableID > newEntry.TableID)
                    {
                        sectionMap.Insert(sectionMap.IndexOf(oldEntry), newEntry);
                        return(true);
                    }
                }                
            }

            sectionMap.Add(newEntry);

            return (true);
        }

        /// <summary>
        /// Check that all the stations for a specified frequency have completed acquiring data (EIT only).
        /// </summary>
        /// <param name="stations">The list of stations to process.</param>
        /// <param name="frequency">The frequency to be checked.</param>
        /// <returns>True if data acquisition is complete; false otherwise.</returns>
        public static bool CheckStationsComplete(Collection<TVStation> stations, int frequency)
        {
            foreach (TVStation station in stations)
            {
                if (station.Frequency == frequency)
                {
                    bool reply = station.checkStationComplete();
                    if (!reply)
                        return (false);
                }
            }

            return (true);
        }

        private bool checkStationComplete()
        {
            if (collectionType != CollectionType.EIT)
                return (true);

            if (SectionMap.Count == 0)
                return (false);

            if (SectionMap[0].TableID == 0x4e)
                return(checkTransportStream(0x4e, 0x50));
            else
                return(checkTransportStream(0x4f, 0x60));
        }

        private bool checkTransportStream(byte presentFollowingTable, byte scheduleStartTable)
        {
            bool reply;

            if (nextFollowingAvailable)
            {
                reply = checkTable(presentFollowingTable, 1);
                if (!reply)
                    return (false);
            }

            if (scheduleAvailable)
            {
                reply = checkTable(scheduleStartTable, 8);
                if (!reply)
                    return (false);

                for (int table = scheduleStartTable + 1; table < highestTable + 1; table++)
                {
                    reply = checkTable(table, 8);
                    if (!reply)
                        return (false);
                }
            }
            
            return (true);
        }

        private bool checkTable(int table, int increment)
        {
            int checkSection = 0;
            int actualIncrement = increment;

            foreach (SectionMapEntry mapEntry in SectionMap)
            {
                if (mapEntry.TableID == table)
                {
                    if (mapEntry.SectionNumber != checkSection)
                        return (false);

                    highestTable = mapEntry.LastTableID;

                    if (mapEntry.SectionNumber == mapEntry.LastSectionNumber)
                        return (true);
                    else
                    {
                        if (mapEntry.SectionNumber != mapEntry.SegmentLastSectionNumber)
                        {
                            checkSection++;
                            actualIncrement--;
                        }
                        else
                        {
                            checkSection += actualIncrement;
                            actualIncrement = increment;
                        }
                    }
                }
            }

            return (false);
        }

        /// <summary>
        /// Find a station.
        /// </summary>
        /// <param name="stations">The list of stations to process.</param>
        /// <param name="serviceID">The service ID of the station.</param>
        /// <returns>The station object or null if it does not exist.</returns>
        public static TVStation FindStation(Collection<TVStation> stations, int serviceID)
        {
            foreach (TVStation tvStation in stations)
            {
                if (tvStation.ServiceID == serviceID)
                    return (tvStation);
            }

            return (null);
        }

        /// <summary>
        /// Find a station from the current scan.
        /// </summary>
        /// <param name="stations">The list of stations to process.</param>
        /// <param name="serviceID">The service ID of the station.</param>
        /// <returns>The station object or null if it does not exist.</returns>
        public static TVStation FindCurrentStation(Collection<TVStation> stations, int serviceID)
        {
            foreach (TVStation tvStation in stations)
            {
                if (tvStation.CurrentScan && tvStation.ServiceID == serviceID)
                    return (tvStation);
            }

            return (null);
        }

        /// <summary>
        /// Find a station.
        /// </summary>
        /// <param name="stations">The list of stations to process.</param>
        /// <param name="frequency">The frequency of the station.</param>
        /// <param name="serviceID">The service ID of the station.</param>
        /// <returns>The station object or null if it does not exist.</returns>
        public static TVStation FindStation(Collection<TVStation> stations, int frequency, int serviceID)
        {
            foreach (TVStation tvStation in stations)
            {
                if (tvStation.Frequency == frequency && tvStation.ServiceID == serviceID)
                    return (tvStation);
            }

            return (null);
        }

        /// <summary>
        /// Find a station.
        /// </summary>
        /// <param name="stations">The list of stations to process.</param>
        /// <param name="originalNetworkID">The original network ID of the station.</param>
        /// <param name="transportStreamID">The transport stream ID of the station.</param>
        /// <param name="serviceID">The service ID of the station.</param>
        /// <returns>The station object or null.</returns>
        public static TVStation FindStation(Collection<TVStation> stations, int originalNetworkID, int transportStreamID, int serviceID)
        {
            foreach (TVStation tvStation in stations)
            {
                if (tvStation.OriginalNetworkID == originalNetworkID && tvStation.TransportStreamID == transportStreamID && tvStation.ServiceID == serviceID)
                    return (tvStation);
            }

            return (null);
        }

        /// <summary>
        /// Find the stations that have EPG links that point to a specified NID/TID/SID.
        /// </summary>
        /// <param name="stations">The list of stations to process.</param>
        /// <param name="originalNetworkID">The original network ID.</param>
        /// <param name="transportStreamID">The transport stream ID.</param>
        /// <param name="serviceID">The service ID.</param>
        /// <returns>The collection of stations</returns>
        public static Collection<TVStation> FindEPGLinks(Collection<TVStation> stations, int originalNetworkID, int transportStreamID, int serviceID)
        {
            Collection<TVStation> stationList = null;

            foreach (TVStation tvStation in stations)
            {
                if (!tvStation.ExcludedByUser && tvStation.EPGLink != null)
                {
                    if (tvStation.EPGLink.OriginalNetworkID == originalNetworkID &&
                        tvStation.EPGLink.TransportStreamID == transportStreamID &&
                        tvStation.EPGLink.ServiceID == serviceID)
                    {
                        if (stationList == null)
                            stationList = new Collection<TVStation>();

                        stationList.Add(tvStation);
                    }
                }
            }

            return (stationList);
        }

        /// <summary>
        /// Log stations with incomplete EIT EPG data.
        /// </summary>
        /// <param name="stations">The list of stations to process.</param>
        public static void LogIncompleteEITMapEntries(Collection<TVStation> stations)
        {
            bool first = true;

            foreach (TVStation station in stations)
            {
                if (station.Included && !station.EITCompleted)
                {
                    if (first)
                    {
                        Logger.Instance.Write("Start of incomplete EIT table data");
                        first = false;
                    }

                    foreach (SectionMapEntry mapEntry in station.SectionMap)
                    {
                        Logger.Instance.Write("ID: " + station.FullID +
                            " TableID: " + mapEntry.TableID +
                            " Sect No: " + mapEntry.SectionNumber +
                            " Last Table ID: " + mapEntry.LastTableID +
                            " Last Sect No: " + mapEntry.LastSectionNumber +
                            " Seg Last Sect No: " + mapEntry.SegmentLastSectionNumber);
                    }
                }
            }

            if (!first)
                Logger.Instance.Write("End of incomplete EIT table data");        
        }

        /// <summary>
        /// Add a station to the collection of stations to be processed rejecting duplicates.
        /// </summary>
        /// <param name="stations">The list of stations to process.</param>
        /// <param name="newStation">The station to be added.</param>
        public static bool AddStation(Collection<TVStation> stations, TVStation newStation)
        {
            if (newStation == null)
                throw (new ArgumentException("The new station cannot be null", "newStation"));

            foreach (TVStation oldStation in stations)
            {
                if (oldStation.OriginalNetworkID != -1)
                {
                    if (oldStation.OriginalNetworkID == newStation.OriginalNetworkID
                        && oldStation.TransportStreamID == newStation.TransportStreamID
                        && oldStation.ServiceID == newStation.ServiceID)
                    {
                        oldStation.Frequency = newStation.Frequency;
                        if (oldStation.ActualFrequency == -1)
                            oldStation.ActualFrequency = newStation.ActualFrequency;
                        oldStation.Name = newStation.Name;
                        return (false);
                    }
                }
                else
                {
                    if (oldStation.Frequency != -1)
                    {
                        if (oldStation.Frequency == newStation.Frequency
                            && oldStation.ServiceID == newStation.ServiceID)
                        {
                            oldStation.Name = newStation.Name;
                            oldStation.OriginalNetworkID = newStation.OriginalNetworkID;
                            oldStation.TransportStreamID = newStation.TransportStreamID;
                            if (oldStation.ActualFrequency == -1)
                                oldStation.ActualFrequency = newStation.ActualFrequency;
                            return (false);
                        }
                    }
                    else
                    {
                        if (oldStation.ServiceID == newStation.ServiceID)
                        {
                            oldStation.Name = newStation.Name;
                            oldStation.OriginalNetworkID = newStation.OriginalNetworkID;
                            oldStation.TransportStreamID = newStation.TransportStreamID;
                            oldStation.Frequency = newStation.Frequency;
                            if (oldStation.ActualFrequency == -1)
                                oldStation.ActualFrequency = newStation.ActualFrequency;
                            return (false);
                        }
                    }
                }
            }

            stations.Add(newStation);

            return (true);
        }

        /// <summary>
        /// Create a station instance.
        /// </summary>
        /// <param name="stations">The list of stations to process.</param>
        /// <param name="serviceID">The service ID of the station.</param>
        /// <returns>A TVStation instance.</returns>
        public static TVStation CreateStation(Collection<TVStation> stations, int serviceID)
        {
            TVStation newStation = new TVStation(serviceID.ToString(CultureInfo.InvariantCulture));
            newStation.ServiceID = serviceID;
            
            AddStation(stations, newStation);

            return (newStation);
        }

        /// <summary>
        /// Create a station instance.
        /// </summary>
        /// <param name="stations">The list of stations to process.</param>
        /// <param name="originalNetworkID">The ONID of the station.</param>
        /// <param name="transportStreamID">The TSID of the station.</param>
        /// <param name="serviceID">The SID of the station.</param>
        /// <returns>A TVStation instance.</returns>
        public static TVStation CreateStation(Collection<TVStation> stations, int originalNetworkID, int transportStreamID, int serviceID)
        {
            TVStation newStation = new TVStation(originalNetworkID + ":" + transportStreamID + ":" + serviceID);
            newStation.OriginalNetworkID = originalNetworkID;
            newStation.TransportStreamID = transportStreamID;
            newStation.ServiceID = serviceID;

            AddStation(stations, newStation);

            return (newStation);
        }

        /// <summary>
        /// Add an EPG entry for the station.
        /// </summary>
        /// <param name="newEntry">The EPG entry.</param>
        /// <returns>True if it was added; false if it replaced an existing entry.</returns>
        public bool AddEPGEntry(EPGEntry newEntry)
        {
            if (newEntry == null)
                throw (new ArgumentException("The new entry cannot be null", "newEntry"));

            foreach (EPGEntry oldEntry in EPGCollection)
            {
                if (newEntry.StartTime == oldEntry.StartTime)
                {
                    /*if (newEntry.EventName != null)
                    {
                        EPGCollection.Insert(EPGCollection.IndexOf(oldEntry), newEntry);
                        EPGCollection.Remove(oldEntry);
                    }*/
                    return (false);
                }
                
                /*if (newEntry.StartTime > oldEntry.StartTime && (newEntry.StartTime + newEntry.Duration) <= (oldEntry.StartTime + oldEntry.Duration))
                    return (false);*/

                if (newEntry.StartTime < oldEntry.StartTime)
                {
                    EPGCollection.Insert(EPGCollection.IndexOf(oldEntry), newEntry);
                    TotalEpgCount++;
                    return (true);
                }
            }

            EPGCollection.Add(newEntry);
            TotalEpgCount++;

            return (true);
        }

        /// <summary>
        /// Update this instance from another.
        /// </summary>
        /// <param name="station">The instance containing the update data.</param>
        public void Update(TVStation station)
        {
            if (station == null)
                throw (new ArgumentException("The station cannot be null", "Station"));

            audioPID = station.AudioPID;
            dsmccPID = station.DSMCCPID;
            encrypted = station.Encrypted;
            frequency = station.Frequency;

            if (actualFrequency == -1)
                actualFrequency = station.ActualFrequency;

            name = station.Name;
            nextFollowingAvailable = station.NextFollowingAvailable;
            providerName = station.ProviderName;
            scheduleAvailable = station.ScheduleAvailable;
            serviceType = station.ServiceType;
            videoPID = station.VideoPID;

            if (station.ConditionalAccessEntries != null)
            {
                conditionalAccessEntries = new Collection<ConditionalAccessEntry>();

                foreach (ConditionalAccessEntry accessEntry in station.ConditionalAccessEntries)
                    conditionalAccessEntries.Add(new ConditionalAccessEntry(accessEntry.SystemID, accessEntry.PID));
            }
        }

        private void notifyPropertyChanged(string stationName)
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(stationName));
        }

        /// <summary>
        /// Compare this instance with another for sorting purposes.
        /// </summary>
        /// <param name="station">The other instance.</param>
        /// <param name="keyName">The name of the key to compare on.</param>
        /// <returns>Zero if the instances are equal, Greater than 0 if this instance is greater; less than zero otherwise.</returns>
        public int CompareForSorting(TVStation station, string keyName)
        {
            if (station == null)
                throw (new ArgumentException("The station cannot be null", "station"));
            if (keyName == null)
                throw (new ArgumentException("The key name cannot be null", "keyName"));

            switch (keyName)
            {
                case "Name":
                    if (name == station.Name)
                    {
                        if (originalNetworkID == station.originalNetworkID)
                        {
                            if (transportStreamID == station.TransportStreamID)
                                return (serviceID.CompareTo(station.ServiceID));
                            else
                                return (transportStreamID.CompareTo(station.TransportStreamID));
                        }
                        else
                            return (originalNetworkID.CompareTo(station.OriginalNetworkID));
                    }
                    else
                        return (name.CompareTo(station.Name));
                case "ONID":
                    if (originalNetworkID == station.originalNetworkID)
                    {
                        if (transportStreamID == station.TransportStreamID)
                        {
                            if (serviceID == station.ServiceID)
                                return(name.CompareTo(station.Name));
                            else
                                return (serviceID.CompareTo(station.ServiceID));
                        }
                        else
                            return (transportStreamID.CompareTo(station.TransportStreamID));
                    }
                    else
                        return (originalNetworkID.CompareTo(station.OriginalNetworkID));
                case "TSID":
                    if (transportStreamID == station.TransportStreamID)
                    {
                        if (originalNetworkID == station.OriginalNetworkID)
                        {
                            if (serviceID == station.ServiceID)
                                return (name.CompareTo(station.Name));
                            else
                                return (serviceID.CompareTo(station.ServiceID));
                        }
                        else
                            return (originalNetworkID.CompareTo(station.OriginalNetworkID));
                    }
                    else
                        return (transportStreamID.CompareTo(station.TransportStreamID));
                case "SID":
                    if (serviceID == station.ServiceID)
                    {
                        /*if (originalNetworkID == station.OriginalNetworkID)
                        {
                            if (transportStreamID == station.TransportStreamID)
                                return (name.CompareTo(station.Name));
                            else
                                return (transportStreamID.CompareTo(station.ServiceID));
                        }
                        else
                            return (originalNetworkID.CompareTo(station.OriginalNetworkID));*/
                        return (0);
                    }
                    else
                        return (serviceID.CompareTo(station.ServiceID));
                case "ExcludedByUser":
                    if (excludedByUser == station.ExcludedByUser)
                    {
                        if (originalNetworkID == station.originalNetworkID)
                        {
                            if (transportStreamID == station.TransportStreamID)
                            {
                                if (serviceID == station.ServiceID)
                                    return (name.CompareTo(station.Name));
                                else
                                    return (serviceID.CompareTo(station.ServiceID));
                            }
                            else
                                return (transportStreamID.CompareTo(station.TransportStreamID));
                        }
                        else
                            return (originalNetworkID.CompareTo(station.OriginalNetworkID));
                    }
                    else
                        return (excludedByUser.CompareTo(station.ExcludedByUser));
                case "NewName":
                    string newNameString;
                    string otherNewNameString;

                    if (newName == null)
                        newNameString = string.Empty;
                    else
                        newNameString = newName;

                    if (station.NewName == null)
                        otherNewNameString = string.Empty;
                    else
                        otherNewNameString = station.NewName;

                    if (newNameString == otherNewNameString)
                    {
                        if (originalNetworkID == station.originalNetworkID)
                        {
                            if (transportStreamID == station.TransportStreamID)
                                return (serviceID.CompareTo(station.ServiceID));
                            else
                                return (transportStreamID.CompareTo(station.TransportStreamID));
                        }
                        else
                            return (originalNetworkID.CompareTo(station.OriginalNetworkID));
                    }
                    else
                        return (newNameString.CompareTo(otherNewNameString));
                case "ChannelNumber":
                    if (logicalChannelNumber == station.LogicalChannelNumber)
                    {
                        if (originalNetworkID == station.originalNetworkID)
                        {
                            if (transportStreamID == station.TransportStreamID)
                            {
                                if (serviceID == station.ServiceID)
                                    return (name.CompareTo(station.Name));
                                else
                                    return (serviceID.CompareTo(station.ServiceID));
                            }
                            else
                                return (transportStreamID.CompareTo(station.TransportStreamID));
                        }
                        else
                            return (originalNetworkID.CompareTo(station.OriginalNetworkID));
                    }
                    else
                        return (logicalChannelNumber.CompareTo(station.LogicalChannelNumber));
                default:
                    return (0);
            }
        }

        /// <summary>
        /// Return a copy of this instance.
        /// </summary>
        /// <returns>A new instance with the same properties as this instance.</returns>
        public TVStation Clone()
        {
            TVStation newStation = new TVStation(name);

            newStation.Frequency = frequency;
            newStation.OriginalNetworkID = originalNetworkID;
            newStation.TransportStreamID = transportStreamID;
            newStation.ServiceID = serviceID;
            newStation.AudioPID = audioPID;
            newStation.VideoPID = videoPID;
            newStation.DSMCCPID = dsmccPID;
            newStation.Encrypted = encrypted;
            newStation.ServiceType = serviceType;
            newStation.NextFollowingAvailable = nextFollowingAvailable;
            newStation.ScheduleAvailable = scheduleAvailable;
            newStation.CollectionType = collectionType;
            newStation.ProviderName = providerName;
            newStation.ExcludedByUser = ExcludedByUser;
            newStation.NewName = newName;
            newStation.LogicalChannelNumber = logicalChannelNumber;
            newStation.MinorChannelNumber = minorChannelNumber;
            
            return (newStation);
        }

        /// <summary>
        /// Checks this instance for equality with another.
        /// </summary>
        /// <param name="station">The other instance.</param>
        /// <returns>True if the other instance is equal; false otherwise.</returns>
        public bool EqualTo(TVStation station)
        {
            if (excludedByUser != station.ExcludedByUser)
                return (false);

            if (newName != station.NewName)
                return (false);

            if (logicalChannelNumber != station.LogicalChannelNumber)
                return (false);

            return (true);
        }

        private void load(XmlReader reader)
        {

            switch (reader.Name)
            {
                case "Name":
                    Name = reader.ReadString();
                    break;
                case "Provider":
                    ProviderName = reader.ReadString();
                    break;
                case "Frequency":
                    Frequency = Int32.Parse(reader.ReadString(), CultureInfo.InvariantCulture);
                    break;
                case "ONID":
                    OriginalNetworkID = Int32.Parse(reader.ReadString(), CultureInfo.InvariantCulture);
                    break;
                case "TSID":
                    TransportStreamID = Int32.Parse(reader.ReadString(), CultureInfo.InvariantCulture);
                    break;
                case "SID":
                    ServiceID = Int32.Parse(reader.ReadString(), CultureInfo.InvariantCulture);
                    break;
                case "Encrypted":
                    Encrypted = Boolean.Parse(reader.ReadString());
                    break;
                case "ServiceType":
                    ServiceType = Int32.Parse(reader.ReadString(), CultureInfo.InvariantCulture);
                    break;
                case "ScheduleAvailable":
                    ScheduleAvailable = Boolean.Parse(reader.ReadString());
                    break;
                case "NextFollowingAvailable":
                    NextFollowingAvailable = Boolean.Parse(reader.ReadString());
                    break;
                case "DSMCCPID":
                    DSMCCPID = Int32.Parse(reader.ReadString(), CultureInfo.InvariantCulture);
                    break;
                default:
                    break;
            }
        }

        private void unload(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("Station");

            xmlWriter.WriteElementString("Name", name);

            if (providerName != null)
                xmlWriter.WriteElementString("Provider", providerName);

            xmlWriter.WriteElementString("Frequency", frequency.ToString(CultureInfo.InvariantCulture));
            xmlWriter.WriteElementString("ONID", originalNetworkID.ToString(CultureInfo.InvariantCulture));
            xmlWriter.WriteElementString("TSID", transportStreamID.ToString(CultureInfo.InvariantCulture));
            xmlWriter.WriteElementString("SID", serviceID.ToString(CultureInfo.InvariantCulture));
            xmlWriter.WriteElementString("Encrypted", encrypted.ToString(CultureInfo.InvariantCulture));
            xmlWriter.WriteElementString("ServiceType", serviceType.ToString(CultureInfo.InvariantCulture));
            xmlWriter.WriteElementString("ScheduleAvailable", scheduleAvailable.ToString(CultureInfo.InvariantCulture));
            xmlWriter.WriteElementString("NextFollowingAvailable", nextFollowingAvailable.ToString(CultureInfo.InvariantCulture));
            xmlWriter.WriteElementString("DSMCCPID", dsmccPID.ToString(CultureInfo.InvariantCulture));

            xmlWriter.WriteEndElement();
        }

        /// <summary>
        /// Load the station collection from an XML file.
        /// </summary>
        /// <param name="fileName">The full name of the xml file.</param>
        /// <returns>A collection of stations or null if the file cannot be opened.</returns>
        public static Collection<TVStation> Load(string fileName)
        {
            TVStation station = null;
            XmlReader reader = null;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;

            try
            {
                reader = XmlReader.Create(fileName, settings);
            }
            catch (IOException)
            {
                Logger.Instance.Write("Failed to open station store from " + fileName);
                return (null);
            }

            Collection<TVStation> stations = new Collection<TVStation>();

            try
            {
                while (!reader.EOF)
                {
                    reader.Read();
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {
                            case "Station":
                                if (station != null)
                                    addStation(stations, station);

                                station = new TVStation("");

                                break;
                            default:
                                if (station != null)
                                    station.load(reader);
                                break;
                        }
                    }
                }

                if (station != null)
                    addStation(stations, station);
            }
            catch (XmlException e)
            {
                Logger.Instance.Write("Failed to load file " + fileName);
                Logger.Instance.Write("Data exception: " + e.Message);
                stations = null;
            }
            catch (IOException e)
            {
                Logger.Instance.Write("Failed to load file " + fileName);
                Logger.Instance.Write("I/O exception: " + e.Message);
                stations = null;
            }

            if (reader != null)
                reader.Close();

            return (stations);
        }

        private static void addStation(Collection<TVStation> stations, TVStation newStation)
        {
            foreach (TVStation oldStation in stations)
            {
                if (oldStation.OriginalNetworkID == newStation.OriginalNetworkID &&
                    oldStation.TransportStreamID == newStation.TransportStreamID &&
                    oldStation.ServiceID == newStation.ServiceID)
                    return;
            }

            stations.Add(newStation);
        }

        /// <summary>
        /// Unload a station collection to an XML file.
        /// </summary>
        /// <param name="fileName">The full name of the file.</param>
        /// <param name="stations">The station collection.</param>
        /// <returns></returns>
        public static string Unload(string fileName, Collection<TVStation> stations)
        {
            if (fileName == null)
                throw (new ArgumentException("The filename cannot be null", "fileName"));
            if (stations == null)
                throw (new ArgumentException("The stations cannot be null", "stations"));

            try
            {
                Logger.Instance.Write("Deleting any existing version of station file");
                File.SetAttributes(fileName, FileAttributes.Normal);
                File.Delete(fileName);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("File delete exception: " + e.Message);
            }

            Logger.Instance.Write("Creating station file: " + fileName);

            XmlWriterSettings settings = new XmlWriterSettings();

            settings.Indent = true;
            settings.NewLineOnAttributes = false;
            settings.Encoding = new UTF8Encoding(false);
            settings.CloseOutput = true;

            try
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(fileName, settings))
                {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("Stations");

                    foreach (TVStation station in stations)
                        station.unload(xmlWriter);

                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndDocument();

                    xmlWriter.Flush();                    
                }
            }
            catch (XmlException ex1)
            {
                return (ex1.Message);
            }
            catch (IOException ex2)
            {
                return (ex2.Message);
            }

            return (null);
        }

        /// <summary>
        /// Sort a list of stations by name or new name.
        /// </summary>
        /// <param name="stations">The list of stations.</param>
        /// <returns>The sorted list.</returns>
        public static Collection<TVStation> GetNameSortedStations(Collection<TVStation> stations)
        {
            Collection<TVStation> sortedStations = new Collection<TVStation>();

            foreach (TVStation newStation in stations)
                updateNameSortedCollection(sortedStations, newStation);

            return (sortedStations);
        }

        private static void updateNameSortedCollection(Collection<TVStation> sortedStations, TVStation newStation)
        {
            string newName = newStation.NewName != null ? newStation.NewName : newStation.Name;

            foreach (TVStation oldStation in sortedStations)
            {
                string oldName = oldStation.NewName != null ? oldStation.NewName : oldStation.Name;

                if (oldName.CompareTo(newName) > 0)
                {
                    sortedStations.Insert(sortedStations.IndexOf(oldStation), newStation);
                    return;
                }
            }

            sortedStations.Add(newStation);
        }

        /// <summary>
        /// Sort a list of stations by user channel number.
        /// </summary>
        /// <param name="stations">The list of stations.</param>
        /// <returns>The sorted list.</returns>
        public static Collection<TVStation> GetUserChannelSortedStations(Collection<TVStation> stations)
        {
            Collection<TVStation> sortedStations = new Collection<TVStation>();

            foreach (TVStation newStation in stations)
                updateUserChannelSortedCollection(sortedStations, newStation);

            return (sortedStations);
        }

        private static void updateUserChannelSortedCollection(Collection<TVStation> sortedStations, TVStation newStation)
        {
            foreach (TVStation oldStation in sortedStations)
            {
                if (oldStation.LogicalChannelNumber > newStation.LogicalChannelNumber)
                {
                    sortedStations.Insert(sortedStations.IndexOf(oldStation), newStation);
                    return;
                }
            }

            sortedStations.Add(newStation);
        }

        /// <summary>
        /// Get the count of stations where the EIT data is complete.
        /// </summary>
        /// <param name="stations">The list of stations.</param>
        /// <returns>The number of stations with completed data.</returns>
        public static int EITComplete(Collection<TVStation> stations)
        {
            int count = 0;

            foreach (TVStation station in stations)
            {
                if (station.CollectionType == CollectionType.EIT && station.EITCompleted)
                    count++;
            }

            return (count);
        }

        /// <summary>
        /// Get the count of stations where the EIT data is not complete.
        /// </summary>
        /// <param name="stations">The list of stations.</param>
        /// <returns>The number of stations with incomplete data.</returns>
        public static int EITNotComplete(Collection<TVStation> stations)
        {
            int count = 0;

            foreach (TVStation station in stations)
            {
                if (station.CollectionType == CollectionType.EIT && !station.EITCompleted)
                    count++;
            }

            return (count);
        }

        /// <summary>
        /// Get the total number of EPG entries for all stations.
        /// </summary>
        /// <param name="stations">The list of stations.</param>
        /// <returns>The number of EPG entries.</returns>
        public static int EPGCount(Collection<TVStation> stations)
        {
            int count = 0;

            foreach (TVStation tvStation in stations)
                count += tvStation.EPGCollection.Count;

            return (count);
        }

        /// <summary>
        /// Get the total number of EPG entries for included stations.
        /// </summary>
        /// <param name="stations">The list of stations.</param>
        /// <returns>The number of EPG entries.</returns>
        public static int EPGCountIncluded(Collection<TVStation> stations)
        {
            int count = 0;

            foreach (TVStation tvStation in stations)
            {
                if (tvStation.Included)
                    count += tvStation.EPGCollection.Count;
            }

            return (count);
        }
    }
}
