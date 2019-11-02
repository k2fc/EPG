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
    /// DVB Content descriptor class.
    /// </summary>
    internal class DVBContentDescriptor : DescriptorBase
    {
        /// <summary>
        /// Get the list of content types.
        /// </summary>
        public Collection<ContentType> ContentTypes { get { return (contentTypes); } }

        /// <summary>
        /// Get the index of the next byte in the EIT section following this descriptor.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The descriptor has not been processed.
        /// </exception> 
        public override int Index
        {
            get
            {
                if (lastIndex == -1)
                    throw (new InvalidOperationException("ContentDescriptor: Index requested before block processed"));
                return (lastIndex);
            }
        }

        private Collection<ContentType> contentTypes;

        private int lastIndex = -1;

        /// <summary>
        /// Initialize a new instance of the DVBContentDescriptor class.
        /// </summary>
        internal DVBContentDescriptor() { }

        /// <summary>
        /// Parse the descriptor.
        /// </summary>
        /// <param name="byteData">The mpeg2 section containing the descriptor.</param>
        /// <param name="index">Index of the byte in the mpeg2 section following the descriptor length.</param>
        internal override void Process(byte[] byteData, int index)
        {
            lastIndex = index;

            contentTypes = new Collection<ContentType>();
            int dataLength = Length;

            while (dataLength > 0)
            {

                try
                {
                    int contentType = (int)(byteData[lastIndex] >> 4);
                    int contentSubType = (int)(byteData[lastIndex] & 0x0f);
                    lastIndex++;

                    int userType = (int)byteData[lastIndex];
                    lastIndex++;

                    contentTypes.Add(new ContentType(contentType, contentSubType, userType));
                    dataLength -= 2;
                }
                catch (IndexOutOfRangeException)
                {
                    throw (new ArgumentOutOfRangeException("The DVB Content Descriptor message is short"));
                }
            }

            Validate();
        }

        /// <summary>
        /// Validate the descriptor fields.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// A descriptor field is not valid.
        /// </exception>
        internal override void Validate() 
        {
            if (contentTypes == null || contentTypes.Count == 0)
                throw (new ArgumentOutOfRangeException("There are no content types in the Content descriptor"));
        }

        /// <summary>
        /// Log the descriptor fields.
        /// </summary>
        internal override void LogMessage() 
        {
            if (Logger.ProtocolLogger == null)
                return;

            if (contentTypes == null || contentTypes.Count == 0)
            {
                Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "DVB CONTENT DESCRIPTOR: No content types present");
                return;
            }

            string leadIn = "DVB CONTENT DESCRIPTOR: Type: ";

            foreach (ContentType contentType in contentTypes)
            {
                Logger.ProtocolLogger.Write(Logger.ProtocolIndent + leadIn + contentType.Type +
                    " Sub type: " + contentType.SubType +
                    " User type: " + contentType.UserType);

                leadIn = "    Type: ";
            }
        }
    }
}
