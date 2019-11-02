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
    internal class DVBDataBroadcastDescriptor : DescriptorBase
    {
        /// <summary>
        /// Get the data broadcast identifier.
        /// </summary>
        public int DataBroadcastId { get { return (dataBroadcastId); } }

        /// <summary>
        /// Get the component tag.
        /// </summary>
        public int ComponentTag { get { return (componentTag); } }

        /// <summary>
        /// Get the selector bytes.
        /// </summary>
        public byte[] SelectorBytes { get { return (selectorBytes); } }

        /// <summary>
        /// Get the language code.
        /// </summary>
        public string LanguageCode { get { return (languageCode); } }

        /// <summary>
        /// Get the text description.
        /// </summary>
        public string TextDescription { get { return (textDescription); } }

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
                    throw (new InvalidOperationException("DataBroadcastDescriptor: Index requested before block processed"));
                return (lastIndex);
            }
        }

        private int dataBroadcastId;
        private int componentTag;
        private byte[] selectorBytes;
        private string languageCode;
        private string textDescription;

        private int lastIndex = -1;

        /// <summary>
        /// Initialize a new instance of the DVBDataBroadcastIdDescriptor class.
        /// </summary>
        internal DVBDataBroadcastDescriptor() { }

        /// <summary>
        /// Parse the descriptor.
        /// </summary>
        /// <param name="byteData">The mpeg2 section containing the descriptor.</param>
        /// <param name="index">Index of the byte in the mpeg2 section following the descriptor length.</param>
        internal override void Process(byte[] byteData, int index)
        {
            lastIndex = index;

            try
            {
                if (Length != 0)
                {
                    dataBroadcastId = Utils.Convert2BytesToInt(byteData, lastIndex);
                    lastIndex += 2;

                    componentTag = (int)byteData[lastIndex];
                    lastIndex++;

                    int selectorLength = (int)byteData[lastIndex];
                    lastIndex++;

                    if (selectorLength != 0)
                    {                        
                        selectorBytes = Utils.GetBytes(byteData, lastIndex, selectorLength);
                        lastIndex += selectorLength;
                    }

                    languageCode = Utils.GetString(byteData, lastIndex, 3);
                    lastIndex += languageCode.Length;
                }

                int textLength = (int)byteData[lastIndex];
                lastIndex++;

                if (textLength != 0)
                {
                    textDescription = Utils.GetString(byteData, lastIndex, textLength);
                    lastIndex += textLength;
                }

                Validate();
            }
            catch (IndexOutOfRangeException)
            {
                throw (new ArgumentOutOfRangeException("The DVB Data Broadcast Descriptor message is short"));
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

            Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "DVB DATA BROADCAST DESCRIPTOR: ID: " + dataBroadcastId +
                " Selector bytes: " + selectorBytes != null ? Utils.ConvertToHex(selectorBytes) : " not present" +
                " Language code: " + languageCode +
                " Text description: " + textDescription != null ? textDescription : " not present");            
        }
    }
}
