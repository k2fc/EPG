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

using DomainObjects;

namespace DVBServices
{
    /// <summary>
    /// Freeview Channel Info descriptor class.
    /// </summary>
    internal class FreeviewChannelInfoDescriptor : DescriptorBase
    {
        /// <summary>
        /// Get the collection of channel entries.
        /// </summary>
        public Collection<FreeviewChannelInfoEntry> ChannelInfoEntries { get { return (channelInfoEntries); } }

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
                    throw (new InvalidOperationException("FreeviewChannelInfoDescriptor: Index requested before block processed"));
                return (lastIndex);
            }
        }

        private Collection<FreeviewChannelInfoEntry> channelInfoEntries;

        private int lastIndex = -1;

        /// <summary>
        /// Initialize a new instance of the FreeviewChannelInfoDescriptor class.
        /// </summary>
        internal FreeviewChannelInfoDescriptor() { }

        /// <summary>
        /// Parse the descriptor.
        /// </summary>
        /// <param name="byteData">The MPEG2 section containing the descriptor.</param>
        /// <param name="index">Index of the byte in the MPEG2 section following the descriptor length.</param>
        internal override void Process(byte[] byteData, int index)
        {
            lastIndex = index;

            if (Length % 4 != 0)
            {
                lastIndex = index + Length;
                return;
            }

            try
            {
                if (Length != 2)
                {
                    channelInfoEntries = new Collection<FreeviewChannelInfoEntry>();

                    int length = Length - 2;                    

                    while (length > 0)
                    {
                        FreeviewChannelInfoEntry channelInfoEntry = new FreeviewChannelInfoEntry();
                        channelInfoEntry.Process(byteData, lastIndex);
                        channelInfoEntries.Add(channelInfoEntry);

                        lastIndex += channelInfoEntry.Length;
                        length -= channelInfoEntry.Length;
                    }
                }

                lastIndex = index + Length;

                Validate();
            }
            catch (IndexOutOfRangeException)
            {
                throw (new ArgumentOutOfRangeException("The Freeview Channel Info Descriptor message is short"));
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

            Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "FREEVIEW CHANNEL INFO DESCRIPTOR");

            if (channelInfoEntries != null)
            {
                Logger.IncrementProtocolIndent();

                foreach (FreeviewChannelInfoEntry channelInfoEntry in channelInfoEntries)
                    channelInfoEntry.LogMessage();

                Logger.DecrementProtocolIndent();
            }
        }
    }
}
