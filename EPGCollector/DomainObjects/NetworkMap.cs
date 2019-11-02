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

namespace DomainObjects
{
    /// <summary>
    /// The class that represents a network map.
    /// </summary>
    public class NetworkMap
    {
        /// <summary>
        /// Get the collection of network maps.
        /// </summary>
        public static Collection<NetworkMap> NetworkMaps { get; private set; }

        /// <summary>
        /// Get the original network ID.
        /// </summary>
        public int OriginalNetworkId { get; private set; }
        /// <summary>
        /// Get the collection of network map entries.
        /// </summary>
        public Collection<NetworkMapEntry> MapEntries { get; private set; }

        private NetworkMap() { }

        /// <summary>
        /// Initialize a new instance of the NetworkMap class.
        /// </summary>
        /// <param name="originalNetworkId">The original network ID.</param>
        public NetworkMap(int originalNetworkId)
        {
            OriginalNetworkId = originalNetworkId;
            MapEntries = new Collection<NetworkMapEntry>();
        }

        /// <summary>
        /// Find a network map.
        /// </summary>
        /// <param name="originalNetworkId">The original network ID.</param>
        /// <returns>The network map or null if it cannot be found.</returns>
        public static NetworkMap FindMap(int originalNetworkId)
        {
            if (NetworkMaps == null)
                NetworkMaps = new Collection<NetworkMap>();

            foreach (NetworkMap networkMap in NetworkMaps)
            {
                if (networkMap.OriginalNetworkId == originalNetworkId)
                    return (networkMap);
            }

            NetworkMap newMap = new NetworkMap(originalNetworkId);
            NetworkMaps.Add(newMap);

            return (newMap);
        }

        /// <summary>
        /// Check if a service is present.
        /// </summary>
        /// <param name="originalNetworkId">The original network ID.</param>
        /// <param name="transportStreamId">The transport stream ID</param>
        /// <param name="serviceId">The service ID</param>
        /// <returns>True if the service is present; false otherwsir.</returns>
        public static bool CheckForService(int originalNetworkId, int transportStreamId, int serviceId)
        {
            NetworkMap networkMap = FindMap(originalNetworkId);
            if (networkMap == null)
                return false;

            NetworkMapEntry networkMapEntry = networkMap.FindMapEntry(transportStreamId);
            if (networkMapEntry == null || networkMapEntry.ServiceIds == null)
                return false;

            return (networkMapEntry.ServiceIds.Contains(serviceId));
        }

        /// <summary>
        /// Find a frequency.
        /// </summary>
        /// <param name="originalNetworkId">The original network ID.</param>
        /// <param name="transportStreamId">The transport stream ID</param>
        /// <returns>The tuning frequency or null if it cannot be found.</returns>
        public static TuningFrequency FindFrequency(int originalNetworkId, int transportStreamId)
        {
            NetworkMap networkMap = FindMap(originalNetworkId);
            if (networkMap == null)
                return (null);

            NetworkMapEntry networkMapEntry = networkMap.FindMapEntry(transportStreamId);
            if (networkMapEntry == null)
                return (null);

            return (networkMapEntry.TuningFrequency);
        }

        /// <summary>
        /// Find a map entry.
        /// </summary>
        /// <param name="transportStreamId">The transport stream ID.</param>
        /// <returns>The network map entry or null if it cannot be found.</returns>
        public NetworkMapEntry FindMapEntry(int transportStreamId)
        {
            if (MapEntries == null)
                MapEntries = new Collection<NetworkMapEntry>();

            foreach (NetworkMapEntry networkMapEntry in MapEntries)
            {
                if (networkMapEntry.TransportStreamId == transportStreamId)
                    return (networkMapEntry);
            }

            NetworkMapEntry newMapEntry = new NetworkMapEntry(transportStreamId);
            MapEntries.Add(newMapEntry);

            return (newMapEntry);
        }

        /// <summary>
        /// Log map entries.
        /// </summary>
        public void LogMapEntry()
        {
            Logger.Instance.Write("ONID: " + OriginalNetworkId);

            if (MapEntries == null || MapEntries.Count == 0)
            {
                Logger.Instance.Write("No map data");
                return;
            }

            foreach (NetworkMapEntry mapEntry in MapEntries)
                mapEntry.LogMapEntry();
        }

        /// <summary>
        /// Log the collection of map entries.
        /// </summary>
        public static void LogMapEntries()
        {
            Logger.Instance.WriteSeparator("Network Map Entries");

            if (NetworkMaps == null || NetworkMaps.Count == 0)
            {
                Logger.Instance.Write("No network map data loaded");
                return;
            }

            foreach (NetworkMap networkMap in NetworkMaps)
                networkMap.LogMapEntry();
        }
    }
}
