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

namespace DomainObjects
{
    /// <summary>
    /// The class that describes an EPG link descriptor.
    /// </summary>
    public class EPGLink
    {
        /// <summary>
        /// Get the network ID.
        /// </summary>
        public int OriginalNetworkID { get { return (originalNetworkID); } }
        /// <summary>
        /// Get the transport stream ID.
        /// </summary>
        public int TransportStreamID { get { return (transportStreamID); } }
        /// <summary>
        /// Get the service ID.
        /// </summary>
        public int ServiceID { get { return (serviceID); } }
        /// <summary>
        /// Get the program time offset.
        /// </summary>
        public int TimeOffset { get { return (timeOffset); } }

        private int originalNetworkID;
        private int transportStreamID;
        private int serviceID;
        private int timeOffset;

        private EPGLink() { }

        /// <summary>
        /// Initialize a new instance of the EPGLink class.
        /// </summary>
        /// <param name="originalNetworkID">The original network ID.</param>
        /// <param name="transportStreamID">The transport stream ID.</param>
        /// <param name="serviceID">The service ID.</param>
        /// <param name="timeOffset">The program time offset.</param>
        public EPGLink(int originalNetworkID, int transportStreamID, int serviceID, int timeOffset)
        {
            this.originalNetworkID = originalNetworkID;
            this.transportStreamID = transportStreamID;
            this.serviceID = serviceID;
            this.timeOffset = timeOffset;
        }
    }
}
