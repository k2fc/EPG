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
    /// The class that describes a channel group entry.
    /// </summary>
    public class ChannelGroupEntry
    {
        /// <summary>
        /// Get the channel name.
        /// </summary>
        public string ChannelName { get; private set; }
        /// <summary>
        /// Get the original network ID.
        /// </summary>
        public int OriginalNetworkId { get; set; }
        /// <summary>
        /// Get the transport stream ID.
        /// </summary>
        public int TransportStreamId { get; set; }
        /// <summary>
        /// Get the service ID.
        /// </summary>
        public int ServiceId { get; set; }

        private ChannelGroupEntry() { }

        /// <summary>
        /// Initialize a new instance of the ChannelGroupEntry class.
        /// </summary>
        /// <param name="channelName">The channel name.</param>
        public ChannelGroupEntry(string channelName)
        {
            ChannelName = channelName;
        }
    }
}
