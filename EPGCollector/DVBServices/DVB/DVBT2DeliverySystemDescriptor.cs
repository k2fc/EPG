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
    /// DVB T2 Terrestrial Delivery System descriptor class.
    /// </summary>
    internal class DVBT2DeliverySystemDescriptor : DVBDeliverySystemDescriptor
    {
        /// <summary>
        /// Get the tuner type for this descriptor.
        /// </summary>
        public override TunerType TunerType { get { return (TunerType.Terrestrial); } }

        /// <summary>
        /// Get the tag extension.
        /// </summary>
        public int TagExtension { get { return (tagExtension); } }

        /// <summary>
        /// Get the PLP ID.
        /// </summary>
        public int PlpId { get { return (plpId); } }

        /// <summary>
        /// Get the system ID.
        /// </summary>
        public int SystemId { get { return (systemId); } }

        /// <summary>
        /// Return true if there is extended data; false otherwise.
        /// </summary>
        public bool ExtendedDataPresent { get { return (extendedDataPresent); } }

        /// <summary>
        /// Get the siso/miso value.
        /// </summary>
        public int SisoMiso { get { return (sisoMiso); } }     
        
        /// <summary>
        /// Get the bandwidth.
        /// </summary>
        public int Bandwidth { get { return (bandWidth); } }        

        /// <summary>
        /// Get the guard interval.
        /// </summary>
        public int GuardInterval { get { return (guardInterval); } }

        /// <summary>
        /// Get the transmission mode.
        /// </summary>
        public int TransmissionMode { get { return (transmissionMode); } }

        /// <summary>
        /// Get the other frequency flag.
        /// </summary>
        public bool OtherFrequencyFlag { get { return (otherFrequencyFlag); } }

        /// <summary>
        /// Get the other TFS flag.
        /// </summary>
        public bool TfsFlag { get { return (tfsFlag); } }

        /// <summary>
        /// Get the collection of cells.
        /// </summary>
        public Collection<DVBT2Cell> Cells { get { return (cells); } }

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
                    throw (new InvalidOperationException("TerrestrialDeliverySystemDescriptor: Index requested before block processed"));
                return (lastIndex);
            }
        }

        private int tagExtension;
        private int plpId;        
        private int systemId;

        private bool extendedDataPresent;
        private int sisoMiso;
        private int bandWidth;
        private int guardInterval;
        private int transmissionMode;
        private bool otherFrequencyFlag;
        private bool tfsFlag;
        private Collection<DVBT2Cell> cells;

        private int lastIndex = -1;

        /// <summary>
        /// Initialize a new instance of the DVBT2DeliverySystemDescriptor class.
        /// </summary>
        internal DVBT2DeliverySystemDescriptor() { }

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
                tagExtension = (int)byteData[lastIndex];
                lastIndex++;

                plpId = (int)byteData[lastIndex];
                lastIndex++;

                systemId = Utils.Convert2BytesToInt(byteData, lastIndex);
                lastIndex += 2;

                if (Length > 4)
                {
                    extendedDataPresent = true;

                    sisoMiso = byteData[lastIndex] >> 6;
                    bandWidth = (byteData[lastIndex] >> 2) & 0x0f;
                    lastIndex++;

                    guardInterval = byteData[lastIndex] >> 5;
                    transmissionMode = (byteData[lastIndex] >> 2) & 0x07;
                    otherFrequencyFlag = (byteData[lastIndex] & 0x02) != 0;
                    tfsFlag = (byteData[lastIndex] & 0x01) != 0;                    
                    lastIndex++;

                    while (Length > lastIndex - index)
                    {
                        if (cells == null)
                            cells = new Collection<DVBT2Cell>();

                        DVBT2Cell cell = new DVBT2Cell();
                        cell.Process(byteData, lastIndex, tfsFlag);
                        cells.Add(cell);

                        lastIndex = cell.Index;
                    }
                }

                lastIndex = index + Length;

                Validate();
            }
            catch (IndexOutOfRangeException)
            {
                throw (new ArgumentOutOfRangeException("The DVB T2 Delivery Descriptor message is short"));
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

            Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "DVB T2 DELIVERY DESCRIPTOR: Tag ext: " + tagExtension +
                " plp ID: " + plpId +
                " system ID: " + systemId +
                " ext data: " + extendedDataPresent +
                " siso/miso: " + sisoMiso +
                " bandwidth: " + bandWidth +
                " guard: " + guardInterval +
                " trans mode: " + transmissionMode +
                " other freq: " + otherFrequencyFlag +
                " tfs flag: " + tfsFlag);

            if (Cells != null)
            {
                foreach (DVBT2Cell cell in Cells)
                {
                    Logger.IncrementProtocolIndent();
                    cell.LogMessage();
                    Logger.DecrementProtocolIndent();
                }
            }
        }
    }
}
