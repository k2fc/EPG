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
    /// The class that describes a FreeSat region entry.
    /// </summary>
    public class FreeSatRegionEntry
    {
        /// <summary>
        /// Get the region number.
        /// </summary>
        public int RegionNumber { get { return (regionNumber); } }
        /// <summary>
        /// Get the language code.
        /// </summary>
        public string LanguageCode { get { return (languageCode); } }
        /// <summary>
        /// Get the region description.
        /// </summary>
        public string RegionDescription { get { return (regionDescription); } }

        /// <summary>
        /// Get the length of the entry.
        /// </summary>
        public int Length { get { return (6 + (regionDescription != null ? regionDescription.Length : 0)); } }
        
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
                    throw (new InvalidOperationException("FreeSatRegionEntry: Index requested before block processed"));
                return (lastIndex);
            }
        }

        private int regionNumber;
        private string languageCode;
        private string regionDescription;
        
        private int lastIndex = -1;        

        /// <summary>
        /// Initialize a new instance of the FreeSatRegionEntry class.
        /// </summary>
        public FreeSatRegionEntry() { }

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
                regionNumber = Utils.Convert2BytesToInt(byteData, lastIndex);
                lastIndex += 2;

                languageCode = Utils.GetString(byteData, lastIndex, 3);
                lastIndex += 3;

                int descriptionLength = (int)byteData[lastIndex];
                lastIndex++;

                if (descriptionLength != 0)
                {
                    regionDescription = Utils.GetString(byteData, lastIndex, descriptionLength);
                    lastIndex += descriptionLength;
                }

                Validate();
            }
            catch (IndexOutOfRangeException)
            {
                throw (new ArgumentOutOfRangeException("The FreeSat Region Entry message is short"));
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
            
            Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "FREESAT REGION ENTRY: Region no: " + regionNumber +
                " Language code: " + languageCode +
                " Description: " + (regionDescription != null ? regionDescription : " not present"));
        }
    }
}
