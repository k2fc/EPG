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
    /// DVB linkage descriptor class.
    /// </summary>
    internal class DVBLinkageDescriptor : DescriptorBase
    {
        /// <summary>
        /// Get the original network ID.
        /// </summary>
        public int OriginalNetworkId { get; private set; } 

        /// <summary>
        /// Get the transport stream ID.
        /// </summary>
        public int TransportStreamId { get; private set; }

        /// <summary>
        /// Get the service ID.
        /// </summary>
        public int ServiceId { get; private set; }

        /// <summary>
        /// Get the linkage type.
        /// </summary>
        public int LinkageType { get; private set; }

        /// <summary>
        /// Get the handover type (linkage type mobile handover only).
        /// </summary>
        public int HandoverType { get; private set; }

        /// <summary>
        /// Get the origin type (linkage type mobile handover only).
        /// </summary>
        public bool OriginType { get; private set; }

        /// <summary>
        /// Get the network ID (linkage type mobile handover only).
        /// </summary>
        public int NetworkId { get; private set; }

        /// <summary>
        /// Get the initial service ID (linkage type mobile handover only).
        /// </summary>
        public int InitialServiceId { get; private set; }

        /// <summary>
        /// Get the target event ID (linkage type event only).
        /// </summary>
        public int TargetEventId { get; private set; }

        /// <summary>
        /// Get the target listed flag (linkage type event only).
        /// </summary>
        public bool TargetListed { get; private set; }

        /// <summary>
        /// Get the simulcast flag (linkage type event only).
        /// </summary>
        public bool Simulcast { get; private set; }

        /// <summary>
        /// Get the private data bytes (linkage type event only).
        /// </summary>
        public byte[] PrivateData { get; private set; }

        /// <summary>
        /// Return true if the linkage type is EPG; false otherwise.
        /// </summary>
        public bool IsEpgLinkage { get { return (LinkageType == 0x02); } }

        /// <summary>
        /// Return true if the linkage type is mobile handover; false otherwise.
        /// </summary>
        public bool IsMobileHandoverLinkage { get { return (LinkageType == 0x08); } }

        /// <summary>
        /// Return true if the linkage type is event; false otherwise.
        /// </summary>
        public bool IsEventLinkage { get { return (LinkageType == 0x0d); } }

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
                    throw (new InvalidOperationException("LinkageDescriptor: Index requested before block processed"));
                return (lastIndex);
            }
        }

        private int lastIndex = -1;

        /// <summary>
        /// Initialize a new instance of the DVBLinkageDescriptor class.
        /// </summary>
        internal DVBLinkageDescriptor() { }

        /// <summary>
        /// Parse the descriptor.
        /// </summary>
        /// <param name="byteData">The MPEG2 section containing the descriptor.</param>
        /// <param name="index">Index of the byte in the MPEG2 section following the descriptor length.</param>
        internal override void Process(byte[] byteData, int index)
        {
            lastIndex = index;

            try
            {
                TransportStreamId = Utils.Convert2BytesToInt(byteData, lastIndex);
                lastIndex += 2;

                OriginalNetworkId = Utils.Convert2BytesToInt(byteData, lastIndex);
                lastIndex += 2;

                ServiceId = Utils.Convert2BytesToInt(byteData, lastIndex);
                lastIndex += 2;

                LinkageType = (int)byteData[lastIndex];
                lastIndex++;

                if (IsMobileHandoverLinkage)
                {
                    HandoverType = byteData[lastIndex] >> 4;
                    OriginType = (byteData[lastIndex] & 0x01) == 1;
                    lastIndex++;

                    switch (HandoverType)
                    {
                        case 0x00:
                            InitialServiceId = Utils.Convert2BytesToInt(byteData, lastIndex);
                            lastIndex += 2;
                            break;
                        case 0x01:
                        case 0x02:
                        case 0x03:
                            NetworkId = Utils.Convert2BytesToInt(byteData, lastIndex);
                            lastIndex += 2;
                            break;
                        default:
                            break;
                    }
                }

                if (IsEventLinkage)
                {
                    TargetEventId = Utils.Convert2BytesToInt(byteData, lastIndex);
                    lastIndex += 2;

                    TargetListed = (byteData[lastIndex] & 0x80) == 1;
                    Simulcast = (byteData[lastIndex] & 0x40) == 1;
                    lastIndex++;
                }

                int remainingLength = Length - (lastIndex - index);
                if (remainingLength != 0)
                {
                    PrivateData = Utils.GetBytes(byteData, lastIndex, remainingLength);
                    lastIndex += remainingLength;
                }

                lastIndex = index + Length;

                Validate();
            }
            catch (IndexOutOfRangeException)
            {
                throw (new ArgumentOutOfRangeException("The DVB Linkage Descriptor message is short"));
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

            Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "DVB LINKAGE DESCRIPTOR:  tid: " + TransportStreamId +
                " onid: " + OriginalNetworkId +
                " sid: " + ServiceId +
                " linkage type: " + LinkageType +
                " handover type: " + HandoverType +
                " origin type: " + OriginType +
                " nid: " + NetworkId +
                " init sid: " + InitialServiceId +
                " target event: " + TargetEventId +
                " target listed: " + TargetListed +
                " simulcast: " + Simulcast +
                " private data: " + (PrivateData != null ? Utils.ConvertToHex(PrivateData) : "not present"));
        }
    }
}
