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
using System.Text;

namespace DomainObjects
{
    /// <summary>
    /// The class that describes a network map entry.
    /// </summary>
    public class NetworkMapEntry
    {
        /// <summary>
        /// Get the transport stream ID.
        /// </summary>
        public int TransportStreamId { get; private set; }
        /// <summary>
        /// Get or set the tuning frequency.
        /// </summary>
        public TuningFrequency TuningFrequency { get; set; }
        /// <summary>
        /// Get the collection of serviceID's.
        /// </summary>
        public Collection<int> ServiceIds { get; private set; }

        private NetworkMapEntry() { }

        /// <summary>
        /// Initialize a new instance of the NetworkMapEntry class.
        /// </summary>
        /// <param name="transportStreamId">The transport stream ID.</param>
        public NetworkMapEntry(int transportStreamId)
        {
            TransportStreamId = transportStreamId;
            ServiceIds = new Collection<int>();
        }

        /// <summary>
        /// Log map entry.
        /// </summary>
        public void LogMapEntry()
        {
            StringBuilder serviceList = new StringBuilder();

            if (ServiceIds == null || ServiceIds.Count == 0)
                serviceList.Append("** No service ID's **");
            else
            {
                foreach (int serviceId in ServiceIds)
                {
                    if (serviceList.Length != 0)
                        serviceList.Append(", ");
                    serviceList.Append(serviceId.ToString());
                }
            }

            Logger.Instance.Write("TSID: " + TransportStreamId + " Freq: " + TuningFrequency.ToString());
            Logger.Instance.Write("    SID's: " + serviceList.ToString()); 
        }
    }
}
