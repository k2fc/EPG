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
    internal class DVBT2Cell
    {
        internal int CellId { get { return (cellId); } }
        internal Collection<int> Frequencies { get { return (frequencies); } }
        internal Collection<DVBT2SubCell> SubCells { get { return (subCells); } }

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
                    throw (new InvalidOperationException("DVBT2Cell: Index requested before block processed"));
                return (lastIndex);
            }
        }

        private int cellId;
        private Collection<int> frequencies;
        private Collection<DVBT2SubCell> subCells;

        private int lastIndex = -1;

        internal DVBT2Cell() { }

        /// <summary>
        /// Parse the descriptor.
        /// </summary>
        /// <param name="byteData">The MPEG2 section containing the descriptor.</param>
        /// <param name="index">Index of the byte in the MPEG2 section following the descriptor length.</param>
        /// <param name="tfsFlag">Other frequency flag.</param>
        internal void Process(byte[] byteData, int index, bool tfsFlag)
        {
            lastIndex = index;

            try
            {
                cellId = Utils.Convert2BytesToInt(byteData, lastIndex);
                lastIndex += 2;

                if (tfsFlag)
                {
                    int frequencyLoopLength = (int)byteData[lastIndex];
                    lastIndex++;

                    while (frequencyLoopLength > 0)
                    {
                        if (frequencies == null)
                            frequencies = new Collection<int>();

                        frequencies.Add(Utils.Convert4BytesToInt(byteData, lastIndex));
                        lastIndex += 4;

                        frequencyLoopLength -= 4;
                    }
                }
                else
                {
                    frequencies = new Collection<int>();
                    frequencies.Add(Utils.Convert4BytesToInt(byteData, lastIndex));
                    lastIndex += 4;
                }

                int subCellLoopLength = (int)byteData[lastIndex];
                lastIndex++;

                while (subCellLoopLength > 0)
                {
                    if (subCells == null)
                        subCells = new Collection<DVBT2SubCell>();

                    DVBT2SubCell subCell = new DVBT2SubCell();
                    subCell.Process(byteData, lastIndex);                    
                    subCells.Add(subCell);                    

                    subCellLoopLength -= (subCell.Index - lastIndex);
                    lastIndex = subCell.Index;
                }
            }
            catch (IndexOutOfRangeException)
            {
                throw (new ArgumentOutOfRangeException("The DVB T2 Cell message is short"));
            }
        }

        internal void LogMessage()
        {
            Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "DVB T2 Cell: CellID: " + cellId);
            
            if (SubCells != null)
            {
                foreach (DVBT2SubCell subCell in SubCells)
                {
                    Logger.IncrementProtocolIndent();
                    subCell.LogMessage();
                    Logger.DecrementProtocolIndent();
                }
            }
        }
    }
}
