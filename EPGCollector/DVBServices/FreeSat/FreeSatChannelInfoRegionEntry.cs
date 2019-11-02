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
    /// The class that describes a FreeSat channel region entry.
    /// </summary>
    public class FreeSatChannelInfoRegionEntry
    {
        /// <summary>
        /// Get the flags.
        /// </summary>
        public byte Flags { get { return (flags); } }
        /// <summary>
        /// Get the region number.
        /// </summary>
        public int RegionNumber { get { return (regionNumber); } }
        /// <summary>
        /// Get the channel number.
        /// </summary>
        public int ChannelNumber { get { return (channelNumber); } }
        
        /// <summary>
        /// Get the length of the entry.
        /// </summary>
        public int Length { get { return (4); } }
        
        /// <summary>
        /// Get the index of the next byte in the section following this entry.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The entry has not been processed.
        /// </exception> 
        public int Index
        {
            get
            {
                if (lastIndex == -1)
                    throw (new InvalidOperationException("FreeSatChannelInfoRegionEntry: Index requested before block processed"));
                return (lastIndex);
            }
        }

        private byte flags;
        private int regionNumber;
        private int channelNumber;
                
        private int lastIndex = -1;        

        /// <summary>
        /// Initialize a new instance of the FreeSatChannelInfoRegionEntry class.
        /// </summary>
        public FreeSatChannelInfoRegionEntry() { }

        /// <summary>
        /// Parse the descriptor.
        /// </summary>
        /// <param name="byteData">The MPEG2 section containing the entry.</param>
        /// <param name="index">Index of the first byte in the MPEG2 section of the entry.</param>
        internal void Process(byte[] byteData, int index)
        {
            lastIndex = index;

            try
            {
                flags = (byte)(byteData[lastIndex] & 0xf0);
                channelNumber = ((byteData[lastIndex] & 0x0f) * 256) + byteData[lastIndex + 1];
                lastIndex += 2;

                regionNumber = Utils.Convert2BytesToInt(byteData, lastIndex);
                lastIndex += 2;

                Validate();
            }
            catch (IndexOutOfRangeException)
            {
                throw (new ArgumentOutOfRangeException("The FreeSat Channel Info Region Entry message is short"));
            }
        }

        /// <summary>
        /// Validate the entry fields.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// A descriptor field is not valid.
        /// </exception>
        internal void Validate() { }

        /// <summary>
        /// Log the entry fields.
        /// </summary>
        internal void LogMessage()
        {
            if (Logger.ProtocolLogger == null)
                return;
            
            Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "FREESAT CHANNEL INFO REGION ENTRY: Flags: 0x" + flags.ToString("X") + 
                " Channel no: " + channelNumber +
                " Region no: " + regionNumber);
        }
    }
}
