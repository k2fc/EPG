﻿////////////////////////////////////////////////////////////////////////////////// 
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

using DomainObjects;

namespace DVBServices
{
    /// <summary>
    /// The class then describes a FreeSat channel.
    /// </summary>
    public class FreeSatChannel : Channel
    {
        /// <summary>
        /// Get or set the first group of unknown bytes.
        /// </summary>
        public byte[] Unknown1
        {
            get { return (unknown1); }
            set { unknown1 = value; }
        }

        private byte[] unknown1;
        
        /// <summary>
        /// Initialize a new instance of the FreeSatChannel class.
        /// </summary>
        public FreeSatChannel() { }

        /// <summary>
        /// Get a description of this instance.
        /// </summary>
        /// <returns>A string describing this instance.</returns>
        public override string ToString()
        {
            return (base.ToString() + 
                " Unknown1: " + Utils.ConvertToHex(Unknown1));
        }
    }
}
