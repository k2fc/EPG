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
    /// The class that describes a channel filter entry.
    /// </summary>
    public class ChannelFilterEntry
    {
        /// <summary>
        /// Get the original network ID.
        /// </summary>
        public int OriginalNetworkID { get { return (originalNetworkID); } }
        /// <summary>
        /// Get the transport stream ID.
        /// </summary>
        public int TransportStreamID { get { return (transportStreamID); } }
        /// <summary>
        /// Get the start of the excluded service ID range.
        /// </summary>
        public int StartServiceID { get { return (startServiceID); } }
        /// <summary>
        /// Get the end of the excluded service ID range.
        /// </summary>
        public int EndServiceID { get { return (endServiceID); } }
        /// <summary>
        /// Get the frequency.
        /// </summary>
        public int Frequency { get { return (frequency); } }

        private int originalNetworkID = -1;
        private int transportStreamID = -1;
        private int startServiceID = -1;
        private int endServiceID = -1;
        private int frequency = -1;

        private ChannelFilterEntry() { }

        /// <summary>
        /// Create a new instance of the ChannelFilterEntry class for a station.
        /// </summary>
        /// <param name="originalNetworkID">The original network ID.</param>
        /// <param name="transportStreamID">The transport stream ID.</param>
        /// <param name="startServiceID">The start service ID.</param>
        /// <param name="endServiceID">The end service ID.</param>
        public ChannelFilterEntry(int originalNetworkID, int transportStreamID, int startServiceID, int endServiceID)
        {
            this.originalNetworkID = originalNetworkID;
            this.transportStreamID = transportStreamID;
            this.startServiceID = startServiceID;
            this.endServiceID = endServiceID;
        }

        /// <summary>
        /// Create a new instance of the ChannelFilterEntry class for a station.
        /// </summary>
        /// <param name="frequency">The frequency.</param>
        /// <param name="originalNetworkID">The original network ID.</param>
        /// <param name="transportStreamID">The transport stream ID.</param>
        /// <param name="startServiceID">The start service ID.</param>
        /// <param name="endServiceID">The end service ID.</param>        
        public ChannelFilterEntry(int frequency, int originalNetworkID, int transportStreamID, int startServiceID, int endServiceID) 
            : this(originalNetworkID, transportStreamID, startServiceID, endServiceID)
        {
            this.frequency = frequency;            
        }

        /// <summary>
        /// Check if a frequency filter is present.
        /// </summary>
        /// <param name="channelFilters">The list of filters to check.</param>
        /// <returns>Treu if a frequency filter is present; false otherwise.</returns>
        public static bool IsFrequencyPresent(Collection<ChannelFilterEntry> channelFilters)
        {
            if (channelFilters == null || channelFilters.Count == 0)
                return (false);

            foreach (ChannelFilterEntry entry in channelFilters)
            {
                if (entry.Frequency != -1)
                    return (true);
            }

            return (false);
        }

        /// <summary>
        /// Find a filter entry for an original network ID.
        /// </summary>
        /// <param name="channelFilters">The list of filters.</param>
        /// <param name="originalNetworkID">The original network ID.</param>
        /// <returns>The filter entry or null if it cannot be located.</returns>
        public static ChannelFilterEntry FindEntry(Collection<ChannelFilterEntry> channelFilters, int originalNetworkID)
        {
            if (channelFilters == null)
                return (null);

            foreach (ChannelFilterEntry filterEntry in channelFilters)
            {
                if (filterEntry.OriginalNetworkID == originalNetworkID &&
                    filterEntry.TransportStreamID == -1 &&
                    filterEntry.StartServiceID == -1 &&
                    filterEntry.EndServiceID == -1)
                    return (filterEntry);
            }

            return (null);
        }

        /// <summary>
        /// Find a filter entry for an original network ID and transport stream ID.
        /// </summary>
        /// <param name="channelFilters">The list of filters.</param>
        /// <param name="originalNetworkID">The original network ID.</param>
        /// <param name="transportStreamID">The transport stream ID.</param>
        /// <returns>The filter entry or null if it cannot be located.</returns>
        public static ChannelFilterEntry FindEntry(Collection<ChannelFilterEntry> channelFilters, int originalNetworkID, int transportStreamID)
        {
            if (channelFilters == null)
                return (null);

            foreach (ChannelFilterEntry filterEntry in channelFilters)
            {
                if (filterEntry.OriginalNetworkID == originalNetworkID && 
                    filterEntry.TransportStreamID == transportStreamID &&
                    filterEntry.StartServiceID == -1 &&
                    filterEntry.EndServiceID == -1)
                    return (filterEntry);
            }

            return (null);
        }

        /// <summary>
        /// Find a filter entry for an original network ID, transport stream ID and serviceID within range.
        /// </summary>
        /// <param name="channelFilters">The list of filters.</param>
        /// <param name="originalNetworkID">The original network ID.</param>
        /// <param name="transportStreamID">The transport stream ID.</param>
        /// <param name="serviceID">The service ID.</param>
        /// <returns>The filter entry or null if it cannot be located.</returns>
        public static ChannelFilterEntry FindEntry(Collection<ChannelFilterEntry> channelFilters, int originalNetworkID, int transportStreamID, int serviceID)
        {
            if (channelFilters == null)
                return (null);

            foreach (ChannelFilterEntry filterEntry in channelFilters)
            {
                if (filterEntry.OriginalNetworkID == originalNetworkID && 
                    filterEntry.TransportStreamID == transportStreamID  &&
                    filterEntry.StartServiceID <= serviceID)
                {
                    if (filterEntry.EndServiceID != -1)
                    {
                        if (filterEntry.EndServiceID >= serviceID)
                            return (filterEntry);
                    }
                    else
                        return (filterEntry);
                }
            }

            foreach (ChannelFilterEntry filterEntry in channelFilters)
            {
                if (filterEntry.OriginalNetworkID == originalNetworkID &&
                    filterEntry.TransportStreamID == -1 &&
                    filterEntry.StartServiceID <= serviceID)
                {
                    if (filterEntry.EndServiceID != -1)
                    {
                        if (filterEntry.EndServiceID >= serviceID)
                            return (filterEntry);
                    }
                    else
                        return (filterEntry);
                }
            }

            foreach (ChannelFilterEntry filterEntry in channelFilters)
            {
                if (filterEntry.OriginalNetworkID == -1 &&
                    filterEntry.TransportStreamID == transportStreamID &&
                    filterEntry.StartServiceID <= serviceID)
                {
                    if (filterEntry.EndServiceID != -1)
                    {
                        if (filterEntry.EndServiceID >= serviceID)
                            return (filterEntry);
                    }
                    else
                        return (filterEntry);
                }
            }

            foreach (ChannelFilterEntry filterEntry in channelFilters)
            {
                if (filterEntry.OriginalNetworkID == -1 &&
                    filterEntry.TransportStreamID == -1 &&
                    filterEntry.StartServiceID <= serviceID)
                {
                    if (filterEntry.EndServiceID != -1)
                    {
                        if (filterEntry.EndServiceID >= serviceID)
                            return (filterEntry);
                    }
                    else
                        return (filterEntry);
                }
            }

            return (null);
        }

        /// <summary>
        /// Find a filter entry for an original network ID on a specified frequency.
        /// </summary>
        /// <param name="channelFilters">The list of filters.</param>
        /// <param name="frequency">The frequency.</param>
        /// <param name="originalNetworkID">The original network ID.</param>
        /// <returns>The filter entry or null if it cannot be located.</returns>
        public static ChannelFilterEntry FindFrequencyEntry(Collection<ChannelFilterEntry> channelFilters, int frequency, int originalNetworkID)
        {
            if (channelFilters == null)
                return (null);

            foreach (ChannelFilterEntry filterEntry in channelFilters)
            {
                if (filterEntry.Frequency == frequency &&
                    filterEntry.OriginalNetworkID == originalNetworkID &&
                    filterEntry.TransportStreamID == -1 &&
                    filterEntry.StartServiceID == -1 &&
                    filterEntry.EndServiceID == -1)
                    return (filterEntry);
            }

            return (null);
        }

        /// <summary>
        /// Find a filter entry for an original network ID and transport stream ID on a specified frequency.
        /// </summary>
        /// <param name="channelFilters">The list of filters.</param>
        /// <param name="frequency">The frequency.</param>
        /// <param name="originalNetworkID">The original network ID.</param>
        /// <param name="transportStreamID">The transport stream ID.</param>
        /// <returns>The filter entry or null if it cannot be located.</returns>
        public static ChannelFilterEntry FindFrequencyEntry(Collection<ChannelFilterEntry> channelFilters, int frequency, int originalNetworkID, int transportStreamID)
        {
            if (channelFilters == null)
                return (null);

            foreach (ChannelFilterEntry filterEntry in channelFilters)
            {
                if (filterEntry.Frequency == frequency &&
                    filterEntry.OriginalNetworkID == originalNetworkID &&
                    filterEntry.TransportStreamID == transportStreamID &&
                    filterEntry.StartServiceID == -1 &&
                    filterEntry.EndServiceID == -1)
                    return (filterEntry);
            }

            return (null);
        }

        /// <summary>
        /// Find a filter entry for an original network ID, transport stream ID and serviceID within range on a specified frequency.
        /// </summary>
        /// <param name="channelFilters">The list of filters.</param>
        /// <param name="frequency">The frequency.</param>
        /// <param name="originalNetworkID">The original network ID.</param>
        /// <param name="transportStreamID">The transport stream ID.</param>
        /// <param name="serviceID">The service ID.</param>        
        /// <returns>The filter entry or null if it cannot be located.</returns>
        public static ChannelFilterEntry FindFrequencyEntry(Collection<ChannelFilterEntry> channelFilters, int frequency, int originalNetworkID, int transportStreamID, int serviceID)
        {
            if (channelFilters == null)
                return (null);

            foreach (ChannelFilterEntry filterEntry in channelFilters)
            {
                if (filterEntry.Frequency == frequency &&
                    filterEntry.OriginalNetworkID == originalNetworkID &&
                    filterEntry.TransportStreamID == transportStreamID &&
                    filterEntry.StartServiceID <= serviceID)
                {
                    if (filterEntry.EndServiceID != -1)
                    {
                        if (filterEntry.EndServiceID >= serviceID)
                            return (filterEntry);
                    }
                    else
                        return (filterEntry);
                }
            }

            foreach (ChannelFilterEntry filterEntry in channelFilters)
            {
                if (filterEntry.Frequency == frequency &&
                    filterEntry.OriginalNetworkID == originalNetworkID &&
                    filterEntry.TransportStreamID == -1 &&
                    filterEntry.StartServiceID <= serviceID)
                {
                    if (filterEntry.EndServiceID != -1)
                    {
                        if (filterEntry.EndServiceID >= serviceID)
                            return (filterEntry);
                    }
                    else
                        return (filterEntry);
                }
            }

            foreach (ChannelFilterEntry filterEntry in channelFilters)
            {
                if (filterEntry.Frequency == frequency &&
                    filterEntry.OriginalNetworkID == -1 &&
                    filterEntry.TransportStreamID == transportStreamID &&
                    filterEntry.StartServiceID <= serviceID)
                {
                    if (filterEntry.EndServiceID != -1)
                    {
                        if (filterEntry.EndServiceID >= serviceID)
                            return (filterEntry);
                    }
                    else
                        return (filterEntry);
                }
            }

            foreach (ChannelFilterEntry filterEntry in channelFilters)
            {
                if (filterEntry.Frequency == frequency &&
                    filterEntry.OriginalNetworkID == -1 &&
                    filterEntry.TransportStreamID == -1 &&
                    filterEntry.StartServiceID <= serviceID)
                {
                    if (filterEntry.EndServiceID != -1)
                    {
                        if (filterEntry.EndServiceID >= serviceID)
                            return (filterEntry);
                    }
                    else
                        return (filterEntry);
                }
            }

            return (null);
        }
    }
}
