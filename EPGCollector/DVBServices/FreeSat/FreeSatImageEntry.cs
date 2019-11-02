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
    /// The class that describes a FreeSat image entry.
    /// </summary>
    public class FreeSatImageEntry
    {
        /// <summary>
        /// Get the image type.
        /// </summary>
        public int ImageType { get { return (imageType); } }
        /// <summary>
        /// Get the image address.
        /// </summary>
        public string ImageAddress { get { return (imageAddress); } }
        
        /// <summary>
        /// Get the length of the entry.
        /// </summary>
        public int Length { get { return (2 + (imageAddress != null ? imageAddress.Length : 0)); } }
        
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
                    throw (new InvalidOperationException("FreeImageEntry: Index requested before block processed"));
                return (lastIndex);
            }
        }

        private int imageType;
        private string imageAddress;
                
        private int lastIndex = -1;        

        /// <summary>
        /// Initialize a new instance of the FreeImageEntry class.
        /// </summary>
        public FreeSatImageEntry() { }

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
                imageType = (int)byteData[lastIndex];
                lastIndex++;

                int addressLength = (int)byteData[lastIndex];
                lastIndex++;

                if (addressLength != 0)
                {
                    imageAddress = Utils.GetString(byteData, lastIndex, addressLength);
                    lastIndex += addressLength;
                }

                Validate();
            }
            catch (IndexOutOfRangeException)
            {
                throw (new ArgumentOutOfRangeException("The FreeSat Image Entry message is short"));
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

            Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "FREESAT IMAGE ENTRY: Category ID: " + imageType +
                " Address: " + (imageAddress != null ? imageAddress : "not present"));
        }
    }
}
