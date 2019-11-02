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
    /// The class that describes a byte conversion entry.
    /// </summary>
    public class ByteConvertEntry
    {
        /// <summary>
        /// Get the control code.
        /// </summary>
        public byte ControlCode { get; private set; }
        /// <summary>
        /// Get the original value.
        /// </summary>
        public byte OriginalValue { get; private set; }
        /// <summary>
        /// Get the converted value;
        /// </summary>
        public byte ConvertedValue { get; private set; }

        private ByteConvertEntry() { }

        /// <summary>
        /// Initialize a new instance of the ByteConvertEntry class.
        /// </summary>
        /// <param name="controlCode">The control code for the byte.</param>
        /// <param name="originalValue">The original value of the byte.</param>
        /// <param name="convertedValue">The converted value of the byte.</param>
        public ByteConvertEntry(byte controlCode, byte originalValue, byte convertedValue)
        {
            ControlCode = controlCode;
            OriginalValue = originalValue;
            ConvertedValue = convertedValue;
        }
    }
}
