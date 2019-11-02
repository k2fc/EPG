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

namespace DVBServices
{
    internal abstract class DishNetworkTextDescriptor : DescriptorBase
    {
        internal int CompressedLength { get; private set; }
        internal int DecompressedLength { get; private set; }
        internal int StartIndex { get; private set; }
        internal int HuffmanTable { get; private set; }

        internal string Decompress(byte[] byteData, int lastIndex)
        {
            if ((byteData[lastIndex] & 0x80) == 0x80)
            {
                CompressedLength = Length - 1;
                StartIndex = lastIndex + 1;
                DecompressedLength = byteData[lastIndex] & 0x7f;
            }
            else
            {
                if ((byteData[lastIndex + 1] & 0x80) == 0x80)
                {
                    CompressedLength = Length - 2;
                    StartIndex = lastIndex + 2;                    

                    if ((byteData[lastIndex] & 0x40) != 0)
                        DecompressedLength = (byteData[lastIndex] & 0x3f) | ((byteData[lastIndex + 1] << 6) & 0xff);
                    else
                        DecompressedLength = byteData[lastIndex] & 0x3f;
                }
                else
                    throw (new InvalidOperationException("Dish Network decompress error: " + Utils.ConvertToHex(byteData, lastIndex, 4)));
            }

            if (CompressedLength <= 0)
                return (null);

            byte[] eventDescriptionBytes = Utils.GetBytes(byteData, StartIndex, CompressedLength);

            if (Table <= 0x80)
                HuffmanTable = 1;
            else
                HuffmanTable = 2;

            string decodedDescription = SingleTreeDictionaryEntry.DecodeData(HuffmanTable, eventDescriptionBytes);

            return (decodedDescription.Substring(0, DecompressedLength));
        }
    }
}
