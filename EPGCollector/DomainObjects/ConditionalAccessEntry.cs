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

namespace DomainObjects
{
    /// <summary>
    /// The class that describes conditional access data.
    /// </summary>
    public class ConditionalAccessEntry
    {
        /// <summary>
        /// Get or set the system ID.
        /// </summary>
        public int SystemID { get; set; }

        /// <summary>
        /// Get or set the PID.
        /// </summary>
        public int PID { get; set; }

        private ConditionalAccessEntry() { }

        /// <summary>
        /// Initialize a new instance of the ConditionalAccessEntry class.
        /// </summary>
        /// <param name="systemID">The system ID.</param>
        /// <param name="pid">The PID.</param>
        public ConditionalAccessEntry(int systemID, int pid)
        {
            SystemID = systemID;
            PID = pid;
        }
    }
}
