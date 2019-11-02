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
    /// The class that converts the DVB east flag.
    /// </summary>
    public sealed class EastWestFlag
    {
        private EastWestFlag() { }

        /// <summary>
        /// Convert the DVB east flag to a single character string.
        /// </summary>
        /// <param name="eastFlag">The DVB east flag.</param>
        /// <returns>The converted character.</returns>
        public static string ConvertDVBEastFlagToChar(bool eastFlag)
        {
            return (eastFlag ? "E" : "W");
        }

        /// <summary>
        /// Convert the DVB east flag to a text string.
        /// </summary>
        /// <param name="eastFlag">The DVB east flag.</param>
        /// <returns>The converted string.</returns>
        public static string ConvertDVBEastFlagTostring(bool eastFlag)
        {
            return (eastFlag ? "East" : "West");
        }
    }
}
