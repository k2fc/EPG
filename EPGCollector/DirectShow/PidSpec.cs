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

using System.Collections.ObjectModel;

namespace DirectShow
{
    /// <summary>
    /// The class that describes a PID and it's tables.
    /// </summary>
    public class PidSpec
    {
        /// <summary>
        /// Get the PID.
        /// </summary>
        public int Pid { get { return (pid); } }
        /// <summary>
        /// Get the tables associated with the PID.
        /// </summary>
        public Collection<int> Tables { get { return (tables); } }

        /// <summary>
        /// Get the number of continuity errors.
        /// </summary>
        public int ContinuityErrorCount { get { return (continuityErrors.Count); } }

        /// <summary>
        /// Get the number of error blocks
        /// </summary>
        public int ErrorBlockCount { get { return (errorBlocks.Count); } }

        private int pid;
        private Collection<int> tables = new Collection<int>();
        private int continuityCount = -1;
        private Collection<int> continuityErrors = new Collection<int>();
        private Collection<int> errorBlocks = new Collection<int>();

        /// <summary>
        /// Initialize a new instance of the PidSpec class.
        /// </summary>
        /// <param name="pid">The PID descibed by this instance.</param>
        public PidSpec(int pid)
        {
            this.pid = pid;
        }

        /// <summary>
        /// Process a transport packet.
        /// </summary>
        /// <param name="buffer">The buffer containing the transport packet.</param>
        /// <param name="packet">The transport packet.</param>
        /// <param name="packetNumber">The sequence number of the packet.</param>
        public void ProcessPacket(byte[] buffer, TransportPacket packet, int packetNumber)
        {
            if (packet.IsNullPacket)
                return;

            if (packet.ErrorIndicator)
            {
                errorBlocks.Add(packetNumber);
                return;
            }

            if (continuityCount == -1)
                continuityCount = packet.ContinuityCount;
            else
            {
                if (continuityCount == 15)
                    continuityCount = 0;
                else
                    continuityCount++;

                if (continuityCount != packet.ContinuityCount)
                {
                    continuityErrors.Add(packetNumber);
                    continuityCount = packet.ContinuityCount;
                }
            }

            if (!packet.StartIndicator)
                return;

            SIPacket siPacket = new SIPacket();
            siPacket.Process(buffer, packet);

            if (siPacket.DataIndex < siPacket.ByteData.Length)
                addTable((int)siPacket.ByteData[siPacket.DataIndex]);
        }

        private void addTable(int newTable)
        {
            foreach (int oldTable in tables)
            {
                if (oldTable == newTable)
                    return;

                if (oldTable > newTable)
                {
                    tables.Insert(tables.IndexOf(oldTable), newTable);
                    return;
                }
            }

            tables.Add(newTable);
        }
    }
}
