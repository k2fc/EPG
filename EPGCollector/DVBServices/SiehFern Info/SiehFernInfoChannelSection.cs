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
    /// The class that describes an SiehFern Info Channel section.
    /// </summary>
    class SiehFernInfoChannelSection
    {
        /// <summary>
        /// Get the collection of Channel sections.
        /// </summary>
        public static Collection<SiehFernInfoChannelSection> Sections { get { return (sections); } }

        /// <summary>
        /// Get the block sequence number.
        /// </summary>
        public int SequenceNumber { get { return (sequenceNumber); } }

        private static Collection<SiehFernInfoChannelSection> sections;

        private int sequenceNumber;
        private byte[] data;

        private int lastIndex;

        /// <summary>
        /// Initialize a new instance of the SiehFernInfoChannelSection class.
        /// </summary>
        public SiehFernInfoChannelSection() { }

        /// <summary>
        /// Parse the section.
        /// </summary>
        /// <param name="byteData">The MPEG2 section containing the section.</param>
        /// <param name="mpeg2Header">The MPEG2 header that preceedes the section.</param>
        public void Process(byte[] byteData, Mpeg2ExtendedHeader mpeg2Header)
        {
            lastIndex = mpeg2Header.Index;
            
            try
            {
                lastIndex += 40;

                sequenceNumber = Utils.Convert2BytesToInt(byteData, lastIndex);
                lastIndex += 2;

                data = Utils.GetBytes(byteData, lastIndex, byteData.Length - lastIndex - 4);

                lastIndex += data.Length;                
            }
            catch (IndexOutOfRangeException)
            {
                throw (new ArgumentOutOfRangeException("The SiehFern Info Channel section is short"));
            }
        }

        /// <summary>
        /// Validate the entry fields.
        /// </summary>
        public void Validate() { }

        /// <summary>
        /// Log the entry fields.
        /// </summary>
        public void LogMessage()
        {
            if (Logger.ProtocolLogger == null)
                return;

            Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "SIEHFERN INFO CHANNEL SECTION");
        }

        internal static bool AddSection(SiehFernInfoChannelSection newSection)
        {
            if (sections == null)
                sections = new Collection<SiehFernInfoChannelSection>();

            foreach (SiehFernInfoChannelSection oldSection in sections)
            {
                if (oldSection.SequenceNumber == newSection.SequenceNumber)
                    return (false);

                if (oldSection.SequenceNumber > newSection.SequenceNumber)
                {
                    sections.Insert(sections.IndexOf(oldSection), newSection);
                    return (true);
                }
            }

            sections.Add(newSection);

            return (true);
        }
    }
}
