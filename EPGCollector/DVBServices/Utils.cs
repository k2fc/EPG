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
using System.Text;
using System.Reflection;

using DomainObjects;

namespace DVBServices
{
    /// <summary>
    /// General utility methods.
    /// </summary>
    public sealed class Utils
    {
        /// <summary>
        /// Get the full assembly version number.
        /// </summary>
        public static string AssemblyVersion
        {
            get
            {
                System.Version version = Assembly.GetExecutingAssembly().GetName().Version;
                return (version.Major + "." + version.Minor + "." + version.Build + "." + version.Revision);
            }
        }

        /// <summary>
        /// Get the control codes that were in the broadcast data.
        /// </summary>
        public static Collection<byte> FormattingBytes { get { return (formattingBytes); } }

        /// <summary>
        /// Get the number of escape sequences.
        /// </summary>
        public static int EscapeCount { get { return (MultiTreeDictionaryEntry.EscapeCount); } }

        private static Collection<byte> formattingBytes;        
        
        /// <summary>
        /// Convert 2 bytes to an integer with the most significant byte first.
        /// </summary>
        /// <param name="byteData">The byte array containing the byes to convert.</param>
        /// <param name="index">The index of the first byte in the array.</param>
        /// <returns>The converted value.</returns>
        public static int Convert2BytesToInt(byte[] byteData, int index)
        {
            return (Convert2BytesToInt(byteData, index, 0xff)); 
        }

        /// <summary>
        /// Convert 2 bytes to an integer with the most significant byte first and a mask.
        /// </summary>
        /// <param name="byteData">The byte array containing the byes to convert.</param>
        /// <param name="index">The index of the first byte in the array.</param>
        /// <param name="mask">The mask for the most significant byte.</param>
        /// <returns>The converted value.</returns>
        public static int Convert2BytesToInt(byte[] byteData, int index, byte mask)
        {
            return (((byteData[index] & mask) * 256) + (int)byteData[index + 1]);
        }

        /// <summary>
        /// Convert 4 bytes to an integer with the most significant byte first.
        /// </summary>
        /// <param name="byteData">The byte array containing the byes to convert.</param>
        /// <param name="index">The index of the first byte in the array.</param>
        /// <returns>The converted value.</returns>
        public static int Convert4BytesToInt(byte[] byteData, int index)
        {
            int temp = (int)byteData[index];
            temp = (temp * 256) + (int)byteData[index + 1];
            temp = (temp * 256) + (int)byteData[index + 2];
            temp = (temp * 256) + (int)byteData[index + 3];

            return (temp);            
        }

        /// <summary>
        /// Convert 8 bytes to a long with the most significant byte first.
        /// </summary>
        /// <param name="byteData">The byte array containing the byes to convert.</param>
        /// <param name="index">The index of the first byte in the array.</param>
        /// <returns>The converted value.</returns>
        public static long Convert8BytesToLong(byte[] byteData, int index)
        {
            long temp = (long)byteData[index];
            temp = (temp * 256) + (long)byteData[index + 1];
            temp = (temp * 256) + (long)byteData[index + 2];
            temp = (temp * 256) + (long)byteData[index + 3];
            temp = (temp * 256) + (long)byteData[index + 4];
            temp = (temp * 256) + (long)byteData[index + 5];
            temp = (temp * 256) + (long)byteData[index + 6];
            temp = (temp * 256) + (long)byteData[index + 7];

            return (temp);
        }

        /// <summary>
        /// Convert a BCD encoded string of bytes to an integer.
        /// </summary>
        /// <param name="byteData">The bytes to be converted.</param>
        /// <param name="index">Offset to the first byte.</param>
        /// <param name="count">The number of BCD nibbles to convert.</param>
        /// <returns>The converted value.</returns>
        public static int ConvertBCDToInt(byte[] byteData, int index, int count)
        {
            int result = 0;
            int shift = 4;

            for (int nibbleIndex = 0; nibbleIndex < count; nibbleIndex++)
            {
                result = (result * 10) + ((byteData[index] >> shift) & 0x0f);

                if (shift == 4)
                    shift = 0;
                else
                {
                    shift = 4;
                    index++;
                }
            }

            return (result);
        }

        /// <summary>
        /// Convert an integer value to a hex string.
        /// </summary>
        /// <param name="value">The value to be converted.</param>
        /// <returns>The converted string.</returns>
        public static string ConvertToHex(int value)
        {
            if (value == 0)
                return ("0x00");

            uint tempValue = (uint)value;

            char[] outputChars = new char[8];

            int outputIndex = 7;

            for (int index = 3; index > -1; index--)
            {
                uint hexByte = (tempValue << 24) >> 24;
                int hexByteLeft = (int)(hexByte >> 4);
                int hexByteRight =(int)(hexByte & 0x0f);

                outputChars[outputIndex] = getHex(hexByteRight);
                outputChars[outputIndex - 1] = getHex(hexByteLeft);

                outputIndex -= 2;
                tempValue = tempValue >> 8;
            }

            string replyString = new string(outputChars).TrimStart(new char[] { '0' });

            if (replyString.Length % 2 == 0)
                return("0x" + replyString);
            else
                return("0x0" + replyString);
        }

        /// <summary>
        /// Convert a string of bytes to a hex string.
        /// </summary>
        /// <param name="inputChars">The string to be converted.</param>
        /// <returns>The string of hex characters.</returns>
        public static string ConvertToHex(byte[] inputChars)
        {
            return (ConvertToHex(inputChars, inputChars.Length));
        }

        /// <summary>
        /// Convert a string of bytes to a hex string.
        /// </summary>
        /// <param name="inputChars">The array holding the bytes to be converted.</param>
        /// <param name="length">The number of byte to be converted.</param>
        /// <returns>The string of hex characters.</returns>
        public static string ConvertToHex(byte[] inputChars, int length)
        {
            return (ConvertToHex(inputChars, 0, length));
        }

        /// <summary>
        /// Convert a string of bytes to a hex string.
        /// </summary>
        /// <param name="inputChars">The array holding the bytes to be converted.</param>
        /// <param name="offset">The the offset to the first byte to be converted.</param> 
        /// <param name="length">The number of byte to be converted.</param>
        /// <returns>The string of hex characters.</returns>
        public static string ConvertToHex(byte[] inputChars, int offset, int length)
        {
            char[] outputChars = new char[length * 2];
            int outputIndex = 0;

            for (int inputIndex = 0; inputIndex < length; inputIndex++)
            {
                int hexByteLeft = inputChars[offset] >> 4;
                int hexByteRight = inputChars[offset] & 0x0f;

                outputChars[outputIndex] = getHex(hexByteLeft);
                outputChars[outputIndex + 1] = getHex(hexByteRight);

                outputIndex += 2;
                offset++;
            }

            return ("0x" + new string(outputChars));
        }

        /// <summary>
        /// Convert a byte array to a string of 1 and 0;
        /// </summary>
        /// <param name="inputChars">The byte array.</param>
        /// <returns>The byte array with 1 bit represented by each character.</returns>
        public static string ConvertToBits(byte[] inputChars)
        {
            StringBuilder bitString = new StringBuilder();            

            foreach (byte byteData in inputChars)
            {
                byte mask = 0x80;

                for (int shift = 0; shift < 8; shift++)
                {
                    if ((byteData & mask) == 0)
                        bitString.Append("0");
                    else
                        bitString.Append("1");

                    mask = (byte)(mask >> 1);
                }

            }

            return (bitString.ToString());
        }        

        private static char getHex(int value)
        {
            if (value < 10)
                return ((char)('0' + value));

            return ((char)('a' + (value - 10)));
        }

        /// <summary>
        /// Convert an array of bytes to an integer value.
        /// </summary>
        /// <param name="inputBytes">The array of input bytes.</param>
        /// <returns>The converted value.</returns>
        public static int ConvertCharByteToInt(byte[] inputBytes)
        {
            int result = 0;

            foreach (byte inputByte in inputBytes)
                result = (result * 10) + (inputByte - 0x30);

            return (result);
        }

        /// <summary>
        /// Convert an array of bytes to an integer up to a terminating byte.
        /// </summary>
        /// <param name="inputBytes">The array of input bytes.</param>
        /// <param name="terminator">The terminating byte.</param>
        /// <returns>The converted value.</returns>
        public static int ConvertCharByteToInt(byte[] inputBytes, byte terminator)
        {
            int result = 0;

            try
            {
                foreach (byte inputByte in inputBytes)
                {
                    if (inputByte == terminator)
                        return (result);
                    else
                        result = (result * 10) + (inputByte - 0x30);
                }
                return(result);
            }
            catch (FormatException)
            {
                throw (new ArgumentOutOfRangeException("ConvertCharByteToInt format wrong"));
            }
            catch (OverflowException)
            {
                throw (new ArgumentOutOfRangeException("ConvertCharByteToInt result too big"));
            }
        }

        /// <summary>
        /// Convert 2 bytes to an integer value with the least significant byte first. 
        /// </summary>
        /// <param name="byteData">The array of bytes containg the byte to be converted.</param>
        /// <param name="index">The index of the first byte.</param>
        /// <returns>The converted value.</returns>
        public static int Swap2BytesToInt(byte[] byteData, int index)
        {
            return ((byteData[index + 1] * 256) + (int)byteData[index]);            
        }

        /// <summary>
        /// Convert 4 bytes to an integer value with the least significant byte first. 
        /// </summary>
        /// <param name="byteData">The array of bytes containg the byte to be converted.</param>
        /// <param name="index">The index of the first byte.</param>
        /// <returns>The converted value.</returns>
        public static int Swap4BytesToInt(byte[] byteData, int index)
        {
            int temp = (int)byteData[index + 3];
            temp = (temp * 256) + (int)byteData[index + 2];
            temp = (temp * 256) + (int)byteData[index + 1];
            temp = (temp * 256) + (int)byteData[index];

            return (temp);            
        }

        /// <summary>
        /// Convert 8 bytes to a long value with the least significant byte first. 
        /// </summary>
        /// <param name="byteData">The array of bytes containg the byte to be converted.</param>
        /// <param name="index">The index of the first byte.</param>
        /// <returns>The converted value.</returns>
        public static long Swap8BytesToLong(byte[] byteData, int index)
        {
            long temp = (long)byteData[index + 7];
            temp = (temp * 256) + (long)byteData[index + 6];
            temp = (temp * 256) + (long)byteData[index + 5];
            temp = (temp * 256) + (long)byteData[index + 4];
            temp = (temp * 256) + (long)byteData[index + 3];
            temp = (temp * 256) + (long)byteData[index + 2];
            temp = (temp * 256) + (long)byteData[index + 1];
            temp = (temp * 256) + (long)byteData[index];

            return (temp);            
        }

        /// <summary>
        /// Convert an array of Unicode bytes to a string.
        /// </summary>
        /// <param name="byteData">The array of bytes.</param>
        /// <param name="offset">The index of the first byte to be converted.</param>
        /// <param name="length">The number of bytes to be converted.</param>
        /// <returns>The converted string.</returns>
        public static string GetUnicodeString(byte[] byteData, int offset, int length)
        {
            UnicodeEncoding encoding = new UnicodeEncoding(true, false);
            return (encoding.GetString(byteData, offset, length));
        }

        /// <summary>
        /// Convert an array of bytes to a string.
        /// </summary>
        /// <param name="byteData">The array of bytes.</param>
        /// <returns>The converted string.</returns>
        public static string GetAsciiString(byte[] byteData)
        {
            return (GetAsciiString(byteData, false));
        }

        /// <summary>
        /// Convert an array of bytes to a string conditionally replacing non-Ascii characters.
        /// </summary>
        /// <param name="byteData">The array of bytes.</param>
        /// <param name="replace">True to replace non-Ascii bytes with a space; false to ignore them.</param>
        /// <returns>The converted string.</returns>
        public static string GetAsciiString(byte[] byteData, bool replace)
        {
            StringBuilder stringData = new StringBuilder();

            for (int index = 0; index < byteData.Length; index++)
            {
                if (byteData[index] > 0x1f && byteData[index] < 0x7f)
                    stringData.Append((char)byteData[index]);
                else
                {
                    if (replace)
                        stringData.Append(' ');
                }
            }

            return (stringData.ToString());
        }

        /// <summary>
        /// Convert an array of bytes to a string.
        /// </summary>
        /// <param name="byteData">The array of bytes.</param>
        /// <param name="offset">Offset to first byte.</param>
        /// <param name="length">Number of bytes.</param>
        /// <returns>The converted string.</returns>
        public static string GetAsciiString(byte[] byteData, int offset, int length)
        {
            return Encoding.ASCII.GetString(byteData, offset, length);            
        }

        /// <summary>
        /// Convert a subset of an array of bytes to a string.
        /// </summary>
        /// <param name="byteData">The array of bytes.</param>
        /// <param name="offset">The index of the first byte to be converted.</param>
        /// <param name="length">The number of bytes to be converted.</param>
        /// <returns>The converted string.</returns>
        public static string GetString(byte[] byteData, int offset, int length)
        {
            return (GetString(byteData, offset, length, ReplaceMode.Ignore));
        }

        /// <summary>
        /// Convert a subset of an array of bytes to a string.
        /// </summary>
        /// <param name="byteData">The array of bytes.</param>
        /// <param name="offset">The index of the first byte to be converted.</param>
        /// <param name="length">The number of bytes to be converted.</param>
        /// <param name="processFormat">The number of bytes to be converted.</param>
        /// <returns>The converted string.</returns>
        public static string GetString(byte[] byteData, int offset, int length, bool processFormat)
        {
            if (!processFormat)
                return (GetString(byteData, offset, length, ReplaceMode.Ignore));

            if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options, OptionName.FormatRemove))
                return (GetString(byteData, offset, length, ReplaceMode.Ignore));

            if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options, OptionName.FormatReplace))
                return (GetString(byteData, offset, length, ReplaceMode.SetToSpace));

            if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options, OptionName.FormatConvert))
                return (GetString(byteData, offset, length, ReplaceMode.Convert));

            if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options, OptionName.FormatConvertTable))
                return (GetString(byteData, offset, length, ReplaceMode.ConvertUsingTable));

            return (GetString(byteData, offset, length, ReplaceMode.Ignore));                
        }

        /// <summary>
        /// Convert a subset of an array of text bytes to a string.
        /// </summary>
        /// <param name="byteData">The array of bytes.</param>
        /// <param name="offset">The index of the first byte to be converted.</param>
        /// <param name="length">The number of bytes to be converted.</param>
        /// <param name="replaceMode">Action to be taken for non-Ascii bytes.</param>
        /// <returns>The converted string.</returns>
        public static string GetString(byte[] byteData, int offset, int length, ReplaceMode replaceMode)
        {
            if (length == 0)
                return (string.Empty);

            string isoTable = null;
            int startByte = 0;
            bool cpPresent = false;
            CharacterSetUsage characterSetUsage;

            if (byteData[offset] >= 0x20)
            {
                if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CharacterSet != null)
                {
                    isoTable = RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CharacterSet;
                    characterSetUsage = CharacterSetUsage.User;                    
                }
                else
                {
                    isoTable = "iso-8859-1";
                    characterSetUsage = CharacterSetUsage.Default;                    
                }
            }
            else
            {
                if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CharacterSet != null && !OptionEntry.IsDefined(OptionName.UseBroadcastCp))
                {
                    isoTable = RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CharacterSet;
                    characterSetUsage = CharacterSetUsage.User;
                    startByte = 1;                    
                }
                else
                {
                    cpPresent = true;
                    characterSetUsage = CharacterSetUsage.Broadcast;

                    switch (byteData[offset])
                    {
                        case 0x01:
                        case 0x02:
                        case 0x03:
                        case 0x04:
                        case 0x05:
                        case 0x06:
                        case 0x07:
                        case 0x08:
                        case 0x09:
                        case 0x0a:
                        case 0x0b:
                            isoTable = "iso-8859-" + (byteData[offset] + 4).ToString();
                            startByte = 1;
                            break;
                        case 0x10:
                            if (byteData[offset + 1] == 0x00)
                            {
                                if (byteData[offset + 2] != 0x00 && byteData[offset + 2] != 0x0c)
                                {
                                    isoTable = "iso-8859-" + ((int)byteData[offset + 2]).ToString();
                                    startByte = 3;
                                    break;
                                }
                                else
                                    return ("Invalid DVB text string: byte 3 is not a valid value");
                            }
                            else
                                return ("Invalid DVB text string: byte 2 is not a valid value");
                        case 0x11:
                            isoTable = "utf-8";
                            startByte = 1;
                            break;
                        case 0x12:
                            isoTable = "ks_c_5601-1987";
                            startByte = 1;
                            break;
                        case 0x13:
                            isoTable = "hz-gb-2312";
                            startByte = 1;
                            break;
                        case 0x14:
                            isoTable = "big5";
                            startByte = 1;
                            break;
                        case 0x15:
                            isoTable = "utf-8";
                            startByte = 1;
                            break;                        
                        case 0x1f:
                            if (byteData[offset + 1] == 0x01 || byteData[offset + 1] == 0x02)
                            {
                                if (MultiTreeDictionaryEntry.Loaded)
                                {
                                    byte[] compressedBytes = Utils.GetBytes(byteData, offset, length + 1);
                                    string text = MultiTreeDictionaryEntry.DecodeData(compressedBytes, "utf-8");

                                    if (DebugEntry.IsDefined(DebugName.LogHuffman) && MultiTreeDictionaryEntry.EscapeCount != 0)
                                        Logger.Instance.Write("HD: Escape count: " + MultiTreeDictionaryEntry.EscapeCount + " result: " + text);
                                            
                                    return (text);
                                }
                                else
                                    return ("Huffman text: " + Utils.ConvertToHex(byteData, offset, length));
                            }
                            else
                                return ("Invalid DVB text string: Custom text specifier is not recognized: 0x" + byteData[offset + 1].ToString("x"));
                        default:
                            return ("Invalid DVB text string: byte 1 is not a valid value: 0x" + byteData[offset].ToString("x"));
                    }
                }
            }

            CharacterSet.MarkAsUsed(isoTable, characterSetUsage);

            byte[] editedBytes;
            int editedOffset = 0;
            int editedLength = 0;

            if (checkIfSingleByteTable(isoTable))
            {
                editedBytes = new byte[(length - startByte) * 2];

                for (int index = startByte; index < length; index++)
                {
                    if ((byteData[offset + index] > 0x1f && byteData[offset + index] < 0x80) || byteData[offset + index] > 0x9f)
                    {
                        if (!cpPresent && RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ByteConvertTable != null)
                        {
                            bool processed = getConvertedByte(byteData, offset + index, editedBytes, editedLength, offset + length);
                            if (processed)
                                index++;
                            editedLength++;
                        }
                        else
                        {
                            editedBytes[editedLength] = byteData[offset + index];
                            editedLength++;
                        }
                    }
                    else
                    {
                        if (isoTable != "utf-8")
                        {
                            if (formattingBytes == null)
                                formattingBytes = new Collection<byte>();
                            if (!formattingBytes.Contains(byteData[offset + index]))
                            {
                                formattingBytes.Add(byteData[offset + index]);
                                if (TraceEntry.IsDefined(TraceName.EitControlBytes))
                                {
                                    Logger.Instance.Write("Replace mode is " + replaceMode);
                                    Logger.Instance.Dump("Control Byte Example", byteData, offset, length);
                                }
                            }

                            switch (replaceMode)
                            {
                                case ReplaceMode.SetToSpace:
                                    editedBytes[editedLength] = 0x20;
                                    editedLength++;
                                    break;
                                case ReplaceMode.TransferUnchanged:
                                    editedBytes[editedLength] = byteData[offset + index];
                                    editedLength++;
                                    break;
                                case ReplaceMode.Convert:
                                    if (byteData[offset + index] == 0x8a)
                                    {
                                        editedBytes[editedLength] = 0x0d;
                                        editedBytes[editedLength + 1] = 0x0a;
                                        editedLength += 2;
                                    }
                                    break;
                                case ReplaceMode.Ignore:
                                    break;
                                case ReplaceMode.ConvertUsingTable:
                                    bool processed = getFormattingByte(byteData, offset + index, editedBytes, editedLength, offset + length);
                                    if (!processed)
                                    {
                                        if (byteData[offset + index] >= 0x20)
                                        {
                                            editedBytes[editedLength] = byteData[offset + index];
                                            editedLength++;
                                        }
                                    }
                                    else
                                        editedLength++;
                                    break;
                                default:
                                    break;
                            }
                        }
                        else
                        {
                            editedBytes[editedLength] = byteData[offset + index];
                            editedLength++;
                        }
                    }
                }

                if (editedLength == 0)
                    return (string.Empty);
            }
            else
            {
                editedBytes = byteData;
                editedOffset = offset + startByte;
                editedLength = length - startByte;
            }

            try
            {
                Encoding sourceEncoding = GetEncoding(isoTable);
                if (sourceEncoding == null)
                    sourceEncoding = Encoding.GetEncoding("iso-8859-1");

                string result = sourceEncoding.GetString(editedBytes, editedOffset, editedLength); 
                if (result.Length == 1 && result[0] == 0x00)
                    return (string.Empty);
                else
                {
                    if (!checkIfSingleByteTable(isoTable))
                        result = processMultiByteControls(result);
                    /*processControlChars(result, replaceMode);*/

                    return (result);
                    
                }
            }
            catch (ArgumentException e)
            {
                Logger.Instance.Write("<E> A text string could not be decoded");
                Logger.Instance.Write("<E> String: " + Utils.ConvertToHex(byteData, offset, length));
                Logger.Instance.Write("<E> Error: " + e.Message);
                return ("** ERROR DECODING STRING - SEE COLLECTION LOG **");
            }
        }

        private static bool checkIfSingleByteTable(string isoTable)
        {
            Encoding encoding = GetEncoding(isoTable);
            if (encoding == null)
                return true;
            else
                return encoding.IsSingleByte;
        }

        private static Encoding GetEncoding(string isoTable)
        {
            try
            {
                return Encoding.GetEncoding(isoTable);
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static string processMultiByteControls(string input)
        {
            string output = input;

            string[] controlCodes = new string[] { "\ue080", "\ue081", "\ue082", "\ue083", "\ue084", "\ue085","\ue086", "\ue087", "\ue088", "\ue089", "\ue08a", "\ue08b", "\ue08c", "\ue08d","\ue08e", "\ue08f",
                "\ue090", "\ue091", "\ue092", "\ue093", "\ue094", "\ue095","\ue096", "\ue097", "\ue098", "\ue099", "\ue09a", "\ue09b", "\ue09c", "\ue09d","\ue09e", "\ue09f" };

            foreach (string controlCode in controlCodes)
                output = output.Replace(controlCode, "");

            return output;            
        }

        private string processControlBytes(string text, ReplaceMode replaceMode)
        {
            Collection<char> controlChars = new Collection<char>();

            foreach (char character in text)
            {
                if (character < 0x1f || (character > 0x7f && character < 0xa0))
                {
                    if (!controlChars.Contains(character))
                        controlChars.Add(character);
                }
            }

            if (controlChars.Count == 0)
                return (text);

            string newText = text;

            foreach (char controlChar in controlChars)
            {
                switch (replaceMode)
                {
                    case ReplaceMode.SetToSpace:
                        newText = newText.Replace(controlChar, ' ');
                        break;
                    case ReplaceMode.TransferUnchanged:
                        break;
                    case ReplaceMode.Convert:
                        if (controlChar == 0x8a)
                            newText = newText.Replace(controlChar, (char)0x0a);                        
                        break;
                    case ReplaceMode.Ignore:
                        break;
                    case ReplaceMode.ConvertUsingTable:
                        byte newByte = ByteConvertFile.FindEntry(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ByteConvertTable, 0x00, (byte)controlChar);
                        if (newByte != 0x00)
                            newText = newText.Replace(controlChar, (char)newByte);
                        break;
                    default:
                        break;
                }
            }

            return (newText);
        }

        /// <summary>
        /// Calculate the number of possible control bytes at the start of a text string.
        /// </summary>
        /// <param name="byteData">The string to check.</param>
        /// <returns>The count of control bytes.</returns>
        public static int CountControlBytes(byte[] byteData)
        {
            if (byteData == null || byteData.Length == 0 || byteData[0] >= 0x20)
                return (0);

            switch (byteData[0])
            {
                case 0x01:
                case 0x02:
                case 0x03:
                case 0x04:
                case 0x05:
                case 0x06:
                case 0x07:
                case 0x08:
                case 0x09:
                case 0x0a:
                case 0x0b:
                    return (1);
                case 0x10:
                    return (3);
                case 0x11:
                case 0x15:
                    return (1);
                case 0x1f:
                    return (2);
                default:
                    return (0);
            }
        }

        private static bool getConvertedByte(byte[] byteData, int index, byte[] editedBytes, int editedLength, int maxLength)
        {
            if (byteData[index] < 0xa0)
            {
                editedBytes[editedLength] = byteData[index];
                return (false);
            }

            if (index + 1 == maxLength)
            {
                editedBytes[editedLength] = byteData[index];
                return (true);
            }

            byte convertedByte = ByteConvertFile.FindEntry(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ByteConvertTable, byteData[index], byteData[index + 1]);
            if (convertedByte == 0x00)
            {
                editedBytes[editedLength] = byteData[index];
                return (false);
            }

            editedBytes[editedLength] = convertedByte;
            
            return (true);
        }

        private static bool getFormattingByte(byte[] byteData, int index, byte[] editedBytes, int editedLength, int maxLength)
        {
            if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ByteConvertTable == null)
                return (false);

            byte convertedByte = ByteConvertFile.FindEntry(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ByteConvertTable, 0x00, byteData[index]);
            if (convertedByte == 0x00)
                return (false);

            editedBytes[editedLength] = convertedByte;
            
            return (true);
        }

        /// <summary>
        /// Convert an array of bytes up to a terminator to a string.
        /// </summary>
        /// <param name="byteData">The array of bytes.</param>
        /// <param name="offset">The index of the first byte to be converted.</param>
        /// <param name="terminator">The value of the terminator.</param>
        /// <returns>The converted string.</returns>
        public static string GetString(byte[] byteData, int offset, byte terminator)
        {
            StringBuilder stringData = new StringBuilder();

            while (byteData[offset] != terminator)
            {
                if (byteData[offset] > 0x1f && byteData[offset] < 0x7f)
                    stringData.Append((char)byteData[offset]);
                else
                    stringData.Append(' ');
                offset++;
            }

            return (stringData.ToString());
        }

        /// <summary>
        /// Convert an array of bytes to a string given an encoding.
        /// </summary>
        /// <param name="byteData">The array of bytes.</param>
        /// <param name="encoding">The type of encoding.</param>
        /// <returns>The converted string.</returns>
        public static string GetString(byte[] byteData, string encoding)
        {
            if (byteData.Length == 0)
                return (string.Empty);

            try
            {
                Encoding sourceEncoding = Encoding.GetEncoding(encoding);
                if (sourceEncoding == null)
                    sourceEncoding = Encoding.GetEncoding("iso-8859-1");

                return (sourceEncoding.GetString(byteData, 0, byteData.Length));
            }
            catch (ArgumentException e)
            {
                Logger.Instance.Write("<E> A text string could not be decoded");
                Logger.Instance.Write("<E> String: " + Utils.ConvertToHex(byteData, 0, byteData.Length));
                Logger.Instance.Write("<E> Error: " + e.Message);
                return ("** ERROR DECODING STRING - SEE COLLECTION LOG **");
            }
        }

        /// <summary>
        /// Get subset of bytes from an array.
        /// </summary>
        /// <param name="byteData">The array of bytes.</param>
        /// <param name="offset">The index of the first byte of the subset.</param>
        /// <param name="length">The length of the subset.</param>
        /// <returns>The subset of bytes.</returns>
        public static byte[] GetBytes(byte[] byteData, int offset, int length)
        {
            if (length < 1)
                throw (new ArgumentOutOfRangeException("GetBytes length wrong"));

            try
            {
                byte[] outputBytes = new byte[length];

                for (int index = 0; index < length; index++)
                    outputBytes[index] = byteData[offset + index];

                return (outputBytes);
            }
            catch (OutOfMemoryException)
            {
                throw (new ArgumentOutOfRangeException("GetBytes length wrong"));
            }
        }

        /// <summary>
        /// Get a subset of bytes from an array up to a terminator.
        /// </summary>
        /// <param name="byteData">The array of bytes.</param>
        /// <param name="offset">The index of the first byte of the subset.</param>
        /// <param name="terminator">The terminating value.</param>
        /// <returns>The subset of bytes.</returns>
        public static byte[] GetBytes(byte[] byteData, int offset, byte terminator)
        {
            int length = 0;

            while (offset + length < byteData.Length && byteData[offset + length] != terminator)
                length++;

            if (length == 0)
                return (new byte[0]);
            else
                return (GetBytes(byteData, offset, length));
        }

        /// <summary>
        /// Compare the bytes of 2 arrays for equality including length.
        /// </summary>
        /// <param name="array1">The first array.</param>
        /// <param name="array2">The second array.</param>
        /// <returns>Treu if the arrays are equal; false otherwise.</returns>
        public static bool CompareBytes(byte[] array1, byte[] array2)
        {
            if (array1.Length != array2.Length)
                return (false);

            for (int index = 0; index < array1.Length; index++)
            {
                if (array1[index] != array2[index])
                    return(false);
            }

            return(true);
        }

        /// <summary>
        /// Compare a subset of the bytes of 2 arrays for equality.
        /// </summary>
        /// <param name="array1">The first array.</param>
        /// <param name="array2">The second array.</param>
        /// <param name="length">The number of bytes to compare.</param>
        /// <returns>True if the arrays are equal; false otherwise.</returns>
        public static bool CompareBytes(byte[] array1, byte[] array2, int length)
        {
            if (array1.Length < length || array2.Length < length)
                return (false);

            for (int index = 0; index < length; index++)
            {
                if (array1[index] != array2[index])
                    return (false);
            }

            return (true);
        }

        /// <summary>
        /// Remove redundant spaces in a string.
        /// </summary>
        /// <param name="inputString">The string to be scanned.</param>
        /// <returns>The compaced string.</returns>
        public static string Compact(string inputString)
        {
            StringBuilder outputString = new StringBuilder();
            bool ignoreSpace = false;

            foreach (char inputChar in inputString)
            {
                if (inputChar == ' ')
                {
                    if (!ignoreSpace)
                    {
                        outputString.Append(inputChar);
                        ignoreSpace = true;
                    }
                }
                else
                {
                    outputString.Append(inputChar);
                    ignoreSpace = false;
                }
            }

            return (outputString.ToString().Trim());
        }

        /// <summary>
        /// Split an array of bytes into an array of strings based on a terminating character.
        /// </summary>
        /// <param name="inputBytes">The array of input bytes.</param>
        /// <param name="splitter">The terminating byte for each substring.</param>
        /// <returns>The array of strings.</returns>
        public static string[] SplitBytesToStrings(byte[] inputBytes, byte splitter)
        {
            int entries = 0;

            foreach (byte inputByte in inputBytes)
            {
                if (inputByte == splitter)
                    entries++;
            }

            if (entries == 0)
                entries = 1;

            string[] outputStrings = new string[entries];
            int outputCount = 0;

            StringBuilder currentString = new StringBuilder();

            foreach (byte inputByte in inputBytes)
            {
                if (inputByte != splitter)
                    currentString.Append(inputByte);
                else
                {
                    outputStrings[outputCount] = currentString.ToString();
                    outputCount++;
                    currentString.Length = 0;
                }

            }

            if (currentString.Length != 0)
                outputStrings[outputCount] = currentString.ToString();

            return (outputStrings);
        }

        /// <summary>
        /// Split an array of bytes into a collection of byte arrays based on a terminator.
        /// </summary>
        /// <param name="inputBytes">The array of bytes.</param>
        /// <param name="splitter">The terminating byte for each collection.</param>
        /// <returns>A collection of byte arrays.</returns>
        public static Collection<byte[]> SplitBytes(byte[] inputBytes, byte splitter)
        {
            Collection<byte[]> outputStrings = new Collection<byte[]>();

            int startIndex = 0;
            int currentIndex = 0;

            for (; currentIndex < inputBytes.Length; currentIndex++)
            {
                if (inputBytes[currentIndex] == splitter)
                {
                    outputStrings.Add(getByteEntry(inputBytes, startIndex, currentIndex));
                    startIndex = currentIndex + 1;
                }
            }

            if (currentIndex != startIndex)
                outputStrings.Add(getByteEntry(inputBytes, startIndex, currentIndex));

            return (outputStrings);
        }

        private static byte[] getByteEntry(byte[] inputBytes, int startIndex, int currentIndex)
        {
            byte[] bytes = new byte[currentIndex - startIndex];

            int outputIndex = 0;

            for (; startIndex < currentIndex; startIndex++)
            {
                bytes[outputIndex] = inputBytes[startIndex];
                outputIndex++;
            }

            return (bytes);
        }

        /// <summary>
        /// Optionally round a date and time to the nearest 5 minutes.
        /// </summary>
        /// <param name="oldDateTime">The date and time to be rounded.</param>
        /// <returns>The adjusted date and time.</returns>
        public static DateTime RoundTime(DateTime oldDateTime)
        {
            if (!OptionEntry.IsDefined(OptionName.RoundTime))
                return (oldDateTime);

            int partSeconds = (int)(oldDateTime.TimeOfDay.TotalSeconds % 300);
            
            if (partSeconds != 0)
            {
                if (partSeconds < 180)
                    return(oldDateTime.AddSeconds(partSeconds * -1));
                else
                    return(oldDateTime.AddSeconds(300 - partSeconds));                    
            }
            else
                return(oldDateTime);            
        }

        /// <summary>
        /// Optionally round a time to the nearest 5 minutes.
        /// </summary>
        /// <param name="oldTime">The time to be rounded.</param>
        /// <returns>The adjusted time.</returns>
        public static TimeSpan RoundTime(TimeSpan oldTime)
        {
            if (!OptionEntry.IsDefined(OptionName.RoundTime))
                return (oldTime);

            int partSeconds = (int)(oldTime.TotalSeconds % 300);

            if (partSeconds != 0)
            {
                if (partSeconds < 180)
                {
                    if (partSeconds != oldTime.TotalSeconds)
                        return (oldTime - getTimeSpan(partSeconds * -1));
                    else
                        return(new TimeSpan(0, 5, 0));
                }
                else
                    return (oldTime + getTimeSpan(300 - partSeconds));
            }
            else
                return (oldTime); 
        }

        private static TimeSpan getTimeSpan(int seconds)
        {
            return (new TimeSpan(0, seconds / 60, seconds % 60));
        }

        /// <summary>
        /// Get New Zealand format season and episode numbers.
        /// </summary>
        /// <param name="epgEntry">The EPG entry to be updated.</param>
        public static bool GetNZLSeasonEpisodeNumbers(EPGEntry epgEntry)
        {
            if (string.IsNullOrWhiteSpace(epgEntry.ShortDescription))
                return false;

            int index = epgEntry.ShortDescription.LastIndexOf(" ep.");
            if (index != -1)
                return getSeasonEpisodeNumbersFormat1(index, epgEntry);
            else
            {
                bool done = false;
                int startIndex = 0;

                while (!done)
                {
                    done = getSeasonEpisodeNumbersFormat3(epgEntry);
                    if (!done)
                    {
                        index = epgEntry.ShortDescription.IndexOf("Ep", startIndex);
                        done = index == -1;
                        if (!done)
                        {
                            done = getSeasonEpisodeNumbersFormat2(index, epgEntry);
                            if (!done)
                                startIndex = index + 1;
                        }
                    }
                }

                if (!done)
                    return false;

                return OptionEntry.IsDefined(OptionName.NoRemoveData);                
            }
        }

        private static bool getSeasonEpisodeNumbersFormat1(int index, EPGEntry epgEntry)
        {
            int index2 = index;

            for (; index2 > -1 && epgEntry.ShortDescription[index2] != 's'; index2--) ;

            if (index2 < 0)
                return false;
            if (epgEntry.ShortDescription[index2 + 1] != '.' || epgEntry.ShortDescription[index2 + 2] != ' ')
                return false;

            int startPoint = index2;

            index2 += 3;
            int seasonNumber = 0;

            for (; index2 < index && epgEntry.ShortDescription[index2] != '.'; index2++)
                seasonNumber = (seasonNumber * 10) + (epgEntry.ShortDescription[index2] - '0');

            if (index2 >= index)
                return false;

            int index3 = index + 5;
            if (index3 >= epgEntry.ShortDescription.Length)
                return false;

            int episodeNumber = 0;

            for (; index3 < epgEntry.ShortDescription.Length && epgEntry.ShortDescription[index3] != '.'; index3++)
                episodeNumber = (episodeNumber * 10) + (epgEntry.ShortDescription[index3] - '0');

            if (index3 >= epgEntry.ShortDescription.Length)
                return false;

            epgEntry.SeasonNumber = seasonNumber;
            epgEntry.EpisodeNumber = episodeNumber;

            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
                epgEntry.ShortDescription = epgEntry.ShortDescription.Remove(startPoint, (index3 - startPoint) + 1);

            return true;
        }

        private static bool getSeasonEpisodeNumbersFormat2(int index, EPGEntry epgEntry)
        {
            int index2 = index;

            for (; index2 > -1 && epgEntry.ShortDescription[index2] != 'S'; index2--) ;

            if (index2 < 0 || index2 < index - 5)
                return (false);

            int startPoint = index2;

            index2++;
            int seasonNumber = 0;

            for (; index2 < index && epgEntry.ShortDescription[index2] >= '0' && epgEntry.ShortDescription[index2] <= '9'; index2++)
                seasonNumber = (seasonNumber * 10) + (epgEntry.ShortDescription[index2] - '0');

            if (index2 >= index)
                return (false);

            int index3 = index + 2;
            if (index3 >= epgEntry.ShortDescription.Length)
                return (false);

            if (epgEntry.ShortDescription[index3] == ' ')
            {
                index3++;
                if (index3 >= epgEntry.ShortDescription.Length)
                    return (false);
            }

            int episodeNumber = 0;

            for (; index3 < epgEntry.ShortDescription.Length && epgEntry.ShortDescription[index3] >= '0' && epgEntry.ShortDescription[index3] <= '9'; index3++)
                episodeNumber = (episodeNumber * 10) + (epgEntry.ShortDescription[index3] - '0');

            epgEntry.SeasonNumber = seasonNumber;
            epgEntry.EpisodeNumber = episodeNumber;

            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
            {
                if (index3 - startPoint < epgEntry.ShortDescription.Length)
                    epgEntry.ShortDescription = epgEntry.ShortDescription.Remove(startPoint, index3 - startPoint).Trim();
                else
                    epgEntry.ShortDescription = null;
            }

            return (true);
        }

        private static bool getSeasonEpisodeNumbersFormat3(EPGEntry epgEntry)
        {
            if (string.IsNullOrWhiteSpace(epgEntry.ShortDescription) || epgEntry.ShortDescription.Length < 6)
                return (false);

            if (epgEntry.ShortDescription[0] != 'S')
                return (false);

            if (epgEntry.ShortDescription[1] < '0' || epgEntry.ShortDescription[1] > '9')
                return (false);

            int index = 1;
            int seasonNumber = 0;
            int episodeNumber = 0;

            for (; epgEntry.ShortDescription[index] >= '0' && epgEntry.ShortDescription[index] <= '9'; index++)
                seasonNumber = (seasonNumber * 10) + (epgEntry.ShortDescription[index] - '0');

            if (epgEntry.ShortDescription[index] != ' ' || epgEntry.ShortDescription[index + 1] != 'E')
                return (false);

            index += 2;

            for (; index < epgEntry.ShortDescription.Length && epgEntry.ShortDescription[index] >= '0' && epgEntry.ShortDescription[index] <= '9'; index++)
                episodeNumber = (episodeNumber * 10) + (epgEntry.ShortDescription[index] - '0');

            if (index < epgEntry.ShortDescription.Length && (epgEntry.ShortDescription[index] != ' ' && epgEntry.ShortDescription[index] != '.'))
                return (false);

            epgEntry.SeasonNumber = seasonNumber;
            epgEntry.EpisodeNumber = episodeNumber;

            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
            {
                if (index + 1 < epgEntry.ShortDescription.Length)
                    epgEntry.ShortDescription = epgEntry.ShortDescription.Remove(0, index + 1).Trim();
                else
                    epgEntry.ShortDescription = null;
            }

            return (true);
        }

        private Utils() { }
    }
}
