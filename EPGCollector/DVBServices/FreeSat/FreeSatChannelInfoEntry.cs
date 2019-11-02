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
    /// The class that describes a FreeSat channel info entry.
    /// </summary>
    public class FreeSatChannelInfoEntry
    {
        /// <summary>
        /// Get the service ID.
        /// </summary>
        public int ServiceID { get { return (serviceID); } }
        /// <summary>
        /// Get the unknown bytes(1).
        /// </summary>
        public byte[] Unknown1 { get { return (unknown1); } }
        /// <summary>
        /// Get the list of region entries.
        /// </summary>
        public Collection<FreeSatChannelInfoRegionEntry> RegionEntries { get { return (regionEntries); } }
        
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
                    throw (new InvalidOperationException("FreeSatChannelInfoEntry: Index requested before block processed"));
                return (lastIndex);
            }
        }

        /// <summary>
        /// Get the length of the entry.
        /// </summary>
        public int Length { get { return (length); } }

        private int serviceID;
        private byte[] unknown1;        
        private Collection<FreeSatChannelInfoRegionEntry> regionEntries;

        private int length;

        private int lastIndex = -1;
        
        /// <summary>
        /// Initialize a new instance of the FreeSatChannelInfoEntry class.
        /// </summary>
        public FreeSatChannelInfoEntry() { }

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
                serviceID = Utils.Convert2BytesToInt(byteData, lastIndex);
                lastIndex += 2;

                unknown1 = Utils.GetBytes(byteData, lastIndex, 2);
                lastIndex += 2;

                int detailLength = (int)byteData[lastIndex];
                lastIndex++;

                length = 5;

                if (detailLength != 0)
                {
                    regionEntries = new Collection<FreeSatChannelInfoRegionEntry>();

                    while (detailLength != 0)
                    {
                        FreeSatChannelInfoRegionEntry regionEntry = new FreeSatChannelInfoRegionEntry();
                        regionEntry.Process(byteData, lastIndex);
                        regionEntries.Add(regionEntry);

                        lastIndex+= regionEntry.Length;

                        detailLength -= regionEntry.Length;                        
                        length += regionEntry.Length;
                    }
                }

                Validate();
            }
            catch (IndexOutOfRangeException)
            {
                throw (new ArgumentOutOfRangeException("The FreeSat Channel Info Entry message is short"));
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
            if (TraceEntry.IsDefined(TraceName.DescriptorD3))
                logMessage(Logger.Instance);
            else
            {
                if (Logger.ProtocolLogger != null)
                    logMessage(Logger.ProtocolLogger);
            }            
        }

        private void logMessage(Logger logger)
        {
            string unknown1String;
            if (unknown1 != null)
                unknown1String = Utils.ConvertToHex(unknown1);
            else
                unknown1String = "not present";            

            logger.Write(Logger.ProtocolIndent + "FREESAT CHANNEL INFO ENTRY: Service ID: " + serviceID +
                " Unknown1: " + unknown1String);

            Logger.IncrementProtocolIndent();

            if (regionEntries != null && regionEntries.Count != 0)
            {
                foreach (FreeSatChannelInfoRegionEntry regionEntry in regionEntries)
                    regionEntry.LogMessage();
            }
            else
                logger.Write(Logger.ProtocolIndent + "No region entries present");

            Logger.DecrementProtocolIndent();
        }
    }
}
