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
    /// The class that describes the GroupInfoIndicationEntry structure.
    /// </summary>
    public class GroupInfoIndicationEntry
    {
        /// <summary>
        /// Get the group ID.
        /// </summary>
        public int GroupID { get { return (groupID); } }
        /// <summary>
        /// Get the group size.
        /// </summary>
        public int GroupSize { get { return (groupSize); } }
        /// <summary>
        /// Get the compability descriptor.
        /// </summary>
        public DSMCCCompatibilityDescriptor CompatibilityDescriptor { get { return (compatibilityDescriptor); } }
        /// <summary>
        /// Get the length of the group info.
        /// </summary>
        public int GroupInfoLength { get { return (groupInfoLength); } }
        /// <summary>
        /// Get the group info.
        /// </summary>
        public byte[] GroupInfo { get { return (groupInfo); } }

        /// <summary>
        /// Get the index of the next byte in the MPEG2 section following the group information.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The information has not been processed.
        /// </exception> 
        public int Index
        {
            get
            {
                if (lastIndex == -1)
                    throw (new InvalidOperationException("GroupInfoIndicationEntry: Index requested before block processed"));
                return (lastIndex);
            }
        }

        private int groupID;
        private int groupSize;
        private DSMCCCompatibilityDescriptor compatibilityDescriptor;
        private int groupInfoLength;
        private byte[] groupInfo = new byte[1] { 0x00 };

        private int lastIndex = -1;

        /// <summary>
        /// Initialize a new instance of the GroupInfoIndication class.
        /// </summary>
        public GroupInfoIndicationEntry() { }

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
                groupID = Utils.Convert4BytesToInt(byteData, lastIndex);
                lastIndex += 4;

                groupSize = Utils.Convert4BytesToInt(byteData, lastIndex);
                lastIndex += 4;

                compatibilityDescriptor = new DSMCCCompatibilityDescriptor();
                compatibilityDescriptor.Process(byteData, lastIndex);
                lastIndex = compatibilityDescriptor.Index;

                groupInfoLength = Utils.Convert2BytesToInt(byteData, lastIndex);
                lastIndex += 2;

                if (groupInfoLength != 0)
                {
                    groupInfo = Utils.GetBytes(byteData, lastIndex, groupInfoLength);
                    lastIndex += groupInfoLength;
                }

                Validate();
            }
            catch (IndexOutOfRangeException)
            {
                throw (new ArgumentOutOfRangeException("The GroupInfoIndicationEntry message is short"));
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

            Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "GROUP INFO INDICATION ENTRY: Group ID: " + groupID +
                " Group size: " + groupSize +
                " Group info lth: " + groupInfoLength +
                " Group info: " + Utils.ConvertToHex(groupInfo));

            Logger.IncrementProtocolIndent();
            compatibilityDescriptor.LogMessage();
            Logger.DecrementProtocolIndent();
        }
    }
}
