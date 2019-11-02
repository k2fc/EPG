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
using System.Text;

using DomainObjects;

namespace DVBServices
{
    internal class DVBDataBroadcastIdDescriptor : DescriptorBase
    {
        /// <summary>
        /// Get the data broadcast identifier.
        /// </summary>
        public int DataBroadcastId { get ; private set; }
        /// <summary>
        /// Get the selector bytes.
        /// </summary>
        public byte[] SelectorBytes { get; private set; }

        /// <summary>
        /// Get the list of application types.
        /// </summary>
        public Collection<int> ApplicationTypes { get; private set; }

        /// <summary>
        /// Get the MHEG5 application type.
        /// </summary>
        public int Mheg5ApplicationType { get; private set; }
        /// <summary>
        /// Get the MHEG5 boot priority hint.
        /// </summary>
        public int Mheg5BootPriorityHint { get; private set; }
        /// <summary>
        /// Get the MHEG5 app specific data.
        /// </summary>
        public byte[] Mheg5AppSpecificData { get; private set; }

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
                    throw (new InvalidOperationException("DVBDataBroadcastIdDescriptor: Index requested before block processed"));
                return (lastIndex);
            }
        }

        private int lastIndex = -1;

        /// <summary>
        /// Initialize a new instance of the DVBDataBroadcastIdDescriptor class.
        /// </summary>
        internal DVBDataBroadcastIdDescriptor() { }

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
                    DataBroadcastId = Utils.Convert2BytesToInt(byteData, lastIndex);
                    lastIndex += 2;

                    int selectorLength = Length - (lastIndex - index);

                    if (selectorLength != 0)
                    {                        
                        SelectorBytes = Utils.GetBytes(byteData, lastIndex, selectorLength);
                        lastIndex += selectorLength;
                    }

                    if (SelectorBytes != null)
                    {
                        switch (DataBroadcastId)
                        {
                            case (int)DVBServices.DataBroadcastId.ObjectCarousel:
                            case (int)DVBServices.DataBroadcastId.MhpMultiProtoclEncapsulation:
                                processMhpSelectorBytes(SelectorBytes);
                                break;
                            case (int)DVBServices.DataBroadcastId.Mheg5:
                                processMheg5SelectorBytes(SelectorBytes);
                                break;
                        }
                    }
                }

                Validate();
            }
            catch (IndexOutOfRangeException)
            {
                throw (new ArgumentOutOfRangeException("The DVB Data Broadcast ID Descriptor message is short"));
            }
        }

        private void processMhpSelectorBytes(byte[] selectorBytes)
        {
            int currentIndex = 0;

            while (currentIndex < selectorBytes.Length)
            {
                if (ApplicationTypes == null)
                    ApplicationTypes = new Collection<int>();

                ApplicationTypes.Add(Utils.Convert2BytesToInt(selectorBytes, currentIndex, 0x7f));
                currentIndex += 2;
            }
        }

        private void processMheg5SelectorBytes(byte[] selectorBytes)
        {
            int currentIndex = 0;

            Mheg5ApplicationType = Utils.Convert2BytesToInt(selectorBytes, currentIndex);
            currentIndex += 2;

            Mheg5BootPriorityHint = (int)selectorBytes[currentIndex];
            currentIndex++;

            int appDataLength = (int)selectorBytes[currentIndex];
            currentIndex++;

            if (appDataLength != 0)
            {
                Mheg5AppSpecificData = Utils.GetBytes(selectorBytes, currentIndex, appDataLength);
                currentIndex += appDataLength;
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

            Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "DVB DATA BROADCAST ID DESCRIPTOR:" +
                " ID:" + DataBroadcastId +
                " Selector bytes: " + (SelectorBytes != null ? Utils.ConvertToHex(SelectorBytes) : " not present"));

            if (DataBroadcastId == (int)DVBServices.DataBroadcastId.Mheg5)
            {
                Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "    " +
                    " MHEG5 app type: " + Mheg5ApplicationType +
                    " MHEG5 boot hint: " + Mheg5BootPriorityHint +
                    " MHEG5 app data: " + (Mheg5AppSpecificData != null ? Utils.ConvertToHex(Mheg5AppSpecificData) : "Not present"));
            }

            if (ApplicationTypes != null)
            {
                StringBuilder typeString = new StringBuilder();

                foreach (int applicationType in ApplicationTypes)
                {
                    if (typeString.Length != 0)
                        typeString.Append(", ");

                    typeString.Append(applicationType);
                }

                Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "    Appl types: " + typeString);
            }
        }
    }
}
