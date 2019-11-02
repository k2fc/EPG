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
    /// The class that describes a FreeSat category entry.
    /// </summary>
    public class FreeSatCategoryEntry
    {
        /// <summary>
        /// Get the category ID.
        /// </summary>
        public int CategoryId { get { return (categoryId); } }
        /// <summary>
        /// Get the region number.
        /// </summary>
        public int CategoryNumber { get { return (categoryNumber); } }
        /// <summary>
        /// Get the language code.
        /// </summary>
        public string LanguageCode { get { return (languageCode); } }
        /// <summary>
        /// Get the region description.
        /// </summary>
        public string CategoryDescription { get { return (categoryDescription); } }

        /// <summary>
        /// Get the length of the entry.
        /// </summary>
        public int Length { get { return (3 + length); } }
        
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
                    throw (new InvalidOperationException("FreeSatCategoryEntry: Index requested before block processed"));
                return (lastIndex);
            }
        }

        private int categoryId;
        private int categoryNumber;
        private int length;
        private string languageCode;
        private string categoryDescription;
        
        private int lastIndex = -1;        

        /// <summary>
        /// Initialize a new instance of the FreeSatCategoryEntry class.
        /// </summary>
        public FreeSatCategoryEntry() { }

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
                categoryId = (int)byteData[lastIndex];
                lastIndex++;

                categoryNumber = (int)byteData[lastIndex];
                lastIndex++;

                length = (int)byteData[lastIndex];
                lastIndex++;

                languageCode = Utils.GetString(byteData, lastIndex, 3);
                lastIndex += 3;

                int descriptionLength = (int)byteData[lastIndex];
                lastIndex++;

                if (descriptionLength != 0)
                {
                    categoryDescription = Utils.GetString(byteData, lastIndex, descriptionLength);
                    lastIndex += descriptionLength;
                }

                Validate();
            }
            catch (IndexOutOfRangeException)
            {
                throw (new ArgumentOutOfRangeException("The FreeSat Category Entry message is short"));
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
            
            Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "FREESAT CATEGORY ENTRY: Category ID: " + categoryId +
                " Category no: " + categoryNumber +
                " Language code: " + languageCode +
                " Description: " + (categoryDescription != null ? categoryDescription : " not present"));
        }
    }
}
