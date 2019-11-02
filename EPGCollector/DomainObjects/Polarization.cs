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
    /// The class that provides polarization conversions.
    /// </summary>
    public sealed class Polarization
    {
        private Polarization() { }

        /// <summary>
        /// Convert the DVB polarization to a single character string.
        /// </summary>
        /// <param name="polarization">The DVB polarization.</param>
        /// <returns>The converted value.</returns>
        public static string ConvertDVBPolarizationToChar(int polarization)
        {
            switch (polarization)
            {
                case 0:
                    return ("H");
                case 1:
                    return ("V");
                case 2:
                    return ("L");
                case 3:
                    return ("R");
                default:
                    return ("?");
            }
        }

        /// <summary>
        /// Convert the DVB polarization to a text string.
        /// </summary>
        /// <param name="polarization">The DVB polarization.</param>
        /// <returns>The converted string.</returns>
        public static string ConvertDVBPolarizationToString(int polarization)
        {
            switch (polarization)
            {
                case 0:
                    return ("Linear Horizontal");
                case 1:
                    return ("Linear Vertical");
                case 2:
                    return ("Circular Left");
                case 3:
                    return ("Circular Right");
                default:
                    return ("?");
            }
        }

        /// <summary>
        /// Convert the DVB polarization to the DVBLink polarization.
        /// </summary>
        /// <param name="polarization">The DVB polarization.</param>
        /// <returns>The DVBLink polarization.</returns>
        public static int ConvertDVBPolarizationDVBLogic(int polarization)
        {
            switch (polarization)
            {
                case 0:
                    return (0);
                case 1:
                    return (1);
                case 2:
                    return (0);
                case 3:
                    return (1);
                default:
                    return (0);
            }
        }
    }
}
