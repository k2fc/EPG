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

using DomainObjects;

namespace DVBServices
{
    /// <summary>
    /// Open TV Channel Group descriptor class.
    /// </summary>
    internal class OpenTVChannelGroupDescriptor : DescriptorBase
    {
        /// <summary>
        /// Get the type.
        /// </summary>
        public int Type { get { return (type); } }
        /// <summary>
        /// Get the flags.
        /// </summary>
        public byte Flags { get { return (flags); } }
        /// <summary>
        /// Get the group number.
        /// </summary>
        public int Group { get { return (group); } }
        /// <summary>
        /// Get the description.
        /// </summary>
        public byte[] Description{ get { return (description); } }

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
                    throw (new InvalidOperationException("OpenTVChannelGroupDescriptor: Index requested before block processed"));
                return (lastIndex);
            }
        }

        private int type;
        private byte flags;
        private int group;
        private byte[] description;        

        private int lastIndex = -1;

        /// <summary>
        /// Initialize a new instance of the OpenTVChannelGroupDescriptor class.
        /// </summary>
        internal OpenTVChannelGroupDescriptor() { }

        /// <summary>
        /// Parse the descriptor.
        /// </summary>
        /// <param name="byteData">The MPEG2 section containing the descriptor.</param>
        /// <param name="index">Index of the byte in the MPEG2 section following the descriptor length.</param>
        internal override void Process(byte[] byteData, int index)
        {
            lastIndex = index;

            type = byteData[lastIndex];
            lastIndex++;

            flags = byteData[lastIndex];
            lastIndex++;

            int length = byteData[lastIndex] & 0x0f;
            lastIndex++;

            if (length > 0)
            {
                group = byteData[lastIndex];
                lastIndex += length;
            }

            if (lastIndex - index != Length)
            {
                description = Utils.GetBytes(byteData, lastIndex, Length - (lastIndex - index));
                lastIndex += description.Length;
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

            Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "OPENTV CHANNEL GROUP DESCRIPTOR" +
                " Type: " + type +
                " Flags: " + Utils.ConvertToHex(flags) +
                " Group: " + group +
                " Description: " + (description != null ? Utils.ConvertToHex(description) : "n/a") +
                " Decode: " + SingleTreeDictionaryEntry.DecodeData(description));
        }
    }
}
