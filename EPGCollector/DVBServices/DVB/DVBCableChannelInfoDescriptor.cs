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

using DomainObjects;

namespace DVBServices
{
    /// <summary>
    /// DVB Cable channel (UK Virgin) descriptor class.
    /// </summary>
    internal class DVBCableChannelInfoDescriptor : DescriptorBase
    {
        /// <summary>
        /// Get the channel number.
        /// </summary>
        public int ChannelNumber { get { return (channelNumber); } }

        /// <summary>
        /// Get the channel name.
        /// </summary>
        public string Name { get { return (channelName); } }

        /// <summary>
        /// Get the index of the next byte in the section following this descriptor.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The descriptor has not been processed.
        /// </exception> 
        public override int Index
        {
            get
            {
                if (lastIndex == -1)
                    throw (new InvalidOperationException("CableChannelInfoDescriptor: Index requested before block processed"));
                return (lastIndex);
            }
        }

        private int channelNumber;
        private string channelName;
        private byte[] unknown;

        private int lastIndex = -1;

        /// <summary>
        /// Initialize a new instance of the DVBCableChannelInfoDescriptor class.
        /// </summary>
        internal DVBCableChannelInfoDescriptor() { }

        /// <summary>
        /// Parse the descriptor.
        /// </summary>
        /// <param name="byteData">The mpeg2 section containing the descriptor.</param>
        /// <param name="index">Index of the byte in the mpeg2 section following the descriptor length.</param>
        internal override void Process(byte[] byteData, int index)
        {
            lastIndex = index;

            try
            {
                channelNumber = Utils.Convert2BytesToInt(byteData, lastIndex);
                lastIndex += 2;

                int nameLength = byteData[lastIndex];
                lastIndex++;

                if (nameLength != 0)
                {
                    channelName = Utils.GetString(byteData, lastIndex, nameLength);
                    lastIndex += nameLength;
                }

                unknown = Utils.GetBytes(byteData, lastIndex, 6);
                lastIndex += 6;

                Validate();
            }
            catch (IndexOutOfRangeException)
            {
                throw (new ArgumentOutOfRangeException("The DVB Cable Channel Info Descriptor message is short"));
            }
        }

        /// <summary>
        /// Validate the descriptor fields.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// A descriptor field is not valid.
        /// </exception>
        internal override void Validate() { }

        /// <summary>
        /// Log the descriptor fields.
        /// </summary>
        internal override void LogMessage()
        {
            if (Logger.ProtocolLogger == null)
                return;

            Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "DVB CABLE CHANNEL INFO DESCRIPTOR:" +
                " No.: " + channelNumber +
                " Name: " + channelName +
                " Unknown: " + Utils.ConvertToHex(unknown));
        }
    }
}
