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
    internal class DVBT2SubCell
    {
        internal int ExtensionId { get { return (extensionId); } }
        internal int Frequency { get { return (frequency); } }

        /// <summary>
        /// Get the index of the next byte in the section following this descriptor.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The descriptor has not been processed.
        /// </exception> 
        public int Index
        {
            get
            {
                if (lastIndex == -1)
                    throw (new InvalidOperationException("DVBT2CellExtension: Index requested before block processed"));
                return (lastIndex);
            }
        }

        private int extensionId;
        private int frequency;

        private int lastIndex = -1;

        internal DVBT2SubCell() { }

        /// <summary>
        /// Parse the descriptor.
        /// </summary>
        /// <param name="byteData">The MPEG2 section containing the descriptor.</param>
        /// <param name="index">Index of the byte in the MPEG2 section following the descriptor length.</param>
        internal void Process(byte[] byteData, int index)
        {
            lastIndex = index;

            try
            {
                extensionId = (int)byteData[lastIndex];
                lastIndex++;

                frequency = Utils.Convert4BytesToInt(byteData, lastIndex);
                lastIndex += 4;
            }
            catch (IndexOutOfRangeException)
            {
                throw (new ArgumentOutOfRangeException("The DVB T2 Sub Cell message is short"));
            }
        }

        internal void LogMessage()
        {
            Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "DVB T2 SubCell: Extension ID: " + extensionId +
                " Frequency: " + frequency);
        }
    }
}
