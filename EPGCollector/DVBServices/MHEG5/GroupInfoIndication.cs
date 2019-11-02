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
    /// The class that describes the GroupInfoIndication structure.
    /// </summary>
    public class GroupInfoIndication
    {
        /// <summary>
        /// Get the group entries.
        /// </summary>
        public Collection<GroupInfoIndicationEntry> GroupEntries { get { return (groupEntries); } }
        /// <summary>
        /// Get the length of the private data.
        /// </summary>
        public int PrivateDataLength { get { return (privateDataLength); } }
        /// <summary>
        /// Get the private data.
        /// </summary>
        public byte[] PrivateData { get { return (privateData); } }        

        /// <summary>
        /// Get the index of the next byte in the MPEG2 section following the service gateway information.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The information has not been processed.
        /// </exception> 
        public int Index
        {
            get
            {
                if (lastIndex == -1)
                    throw (new InvalidOperationException("GroupInfoIndication: Index requested before block processed"));
                return (lastIndex);
            }
        }

        private Collection<GroupInfoIndicationEntry> groupEntries;
        private int privateDataLength;
        private byte[] privateData = new byte[1] { 0x00 };

        private int lastIndex = -1;

        /// <summary>
        /// Initialize a new instance of the GroupInfoIndication class.
        /// </summary>
        public GroupInfoIndication() { }

        /// <summary>
        /// Parse the group information.
        /// </summary>
        /// <param name="byteData">The MPEG2 section containing the group information.</param>
        /// <param name="index">Index of the byte in the MPEG2 section following the gateway information.</param>
        public void Process(byte[] byteData, int index)
        {
            lastIndex = index;

            try
            {
                int groupCount = Utils.Convert2BytesToInt(byteData, lastIndex);
                lastIndex += 2;

                if (groupCount != 0)
                {
                    groupEntries = new Collection<GroupInfoIndicationEntry>();

                    while (groupEntries.Count != groupCount)
                    {
                        GroupInfoIndicationEntry groupEntry = new GroupInfoIndicationEntry();
                        groupEntry.Process(byteData, lastIndex);
                        groupEntries.Add(groupEntry);

                        lastIndex = groupEntry.Index;
                    }
                }

                privateDataLength = Utils.Convert2BytesToInt(byteData, lastIndex);
                lastIndex += 2;

                if (privateDataLength != 0)
                {
                    privateData = Utils.GetBytes(byteData, lastIndex, privateDataLength);
                    lastIndex += privateDataLength;
                }                

                Validate();
            }
            catch (IndexOutOfRangeException)
            {
                throw (new ArgumentOutOfRangeException("The GroupInfoIndication message is short"));
            }
        }

        /// <summary>
        /// Validate the group information fields.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// An information field is not valid.
        /// </exception>
        public void Validate() { }

        /// <summary>
        /// Log the gateway information fields.
        /// </summary>
        public void LogMessage()
        {
            if (Logger.ProtocolLogger == null)
                return;

            int entryCount = 0;
            if (groupEntries != null)
                entryCount = groupEntries.Count;

            Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "GROUP INFO INDICATION: Group count: " + entryCount +
                " Private data lth: " + privateDataLength +
                " Private data: " + Utils.ConvertToHex(privateData));

            if (entryCount != 0)
            {
                foreach (GroupInfoIndicationEntry entry in groupEntries)
                {
                    Logger.IncrementProtocolIndent();
                    entry.LogMessage();
                    Logger.DecrementProtocolIndent();
                }
            }
        }
    }
}
