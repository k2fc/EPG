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
using System.IO;
using System.Text;
using System.Globalization;
using System.Collections.ObjectModel;

using DomainObjects;

namespace DVBServices
{
    /// <summary>
    /// The class that describes a dictionary entry for a multi-tree Hufmman scenario.
    /// </summary>
    public class MultiTreeDictionaryEntry
    {
        /// <summary>
        /// Return true if the translation tables have been loaded; false otherwise.
        /// </summary>
        public static bool Loaded { get { return (loaded); } }

        /// <summary>
        /// Get the count of escape sequences for a text string.
        /// </summary>
        public static int EscapeCount { get; private set; }

        private const int stop = 0x02;
        private const int start = 0x00;
        private const int escape = 0x01;

        private static HuffmanEntry[] table1Roots = new HuffmanEntry[256];
        private static HuffmanEntry[] table2Roots = new HuffmanEntry[256];        

        /// <summary>
        /// Get the decode string.
        /// </summary>
        public string Decode { get { return (decode); } }

        private string decode;
        private string pattern;
        private static bool loaded;

        private static Collection<string> encodings;
        private static int singleByteEscapes;
        private static int multiByteEscapes;
        
        /// <summary>
        /// Initialize a new instance of the MultiTreeDictionaryEntry class.
        /// </summary>
        /// <param name="pattern">The Huffman bit pattern.</param>
        /// <param name="decode">The decode for the bit pattern.</param>
        public MultiTreeDictionaryEntry(string pattern, string decode)
        {
            this.pattern = pattern;
            this.decode = decode;
        }

        /// <summary>
        /// Load the reference tables.
        /// </summary>
        /// <param name="fileName1">The full name of the T1 file.</param>
        /// <param name="fileName2">The full name of the T2 file.</param>
        /// <returns>True if the file is loaded successfully;false otherwise.</returns>
        public static bool Load(string fileName1, string fileName2)
        {
            if (loaded)
                return (true);

            Logger.Instance.Write("Loading Huffman Dictionary 1 from " + fileName1);
            try
            {
                loadFile(table1Roots, fileName1);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("Huffman Dictionary file " + fileName1 + " not available");
                Logger.Instance.Write(e.Message);
                return (false);
            }

            Logger.Instance.Write("Loading Huffman Dictionary 2 from " + fileName2);
            try
            {
                loadFile(table2Roots, fileName2);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("Huffman Dictionary file " + fileName2 + " not available");
                Logger.Instance.Write(e.Message);
                return (false);
            }

            Logger.Instance.Write("Dictionaries loaded");

            loaded = true;
            return (true);
        }

        private static void loadFile(HuffmanEntry[] roots, string filename)
        {
            FileStream fileStream = new FileStream(filename, FileMode.Open, FileAccess.Read);
            StreamReader streamReader = new StreamReader(fileStream);

            while (!streamReader.EndOfStream)
            {
                string line = streamReader.ReadLine();

                if (line != string.Empty && !line.StartsWith("####"))
                {
                    string[] parts = line.Split(new char[] { ':' });
                    if (parts.Length == 4)
                    {
                        int rootOffSet = (int)(resolveChar(parts[0]));

                        if (roots[rootOffSet] == null)
                            roots[rootOffSet] = new HuffmanEntry();

                        HuffmanEntry currentEntry = roots[rootOffSet];
                        string pattern = parts[1];    

                        for (int index = 0; index < parts[1].Length; index++)
                        {
                            char patternChar = pattern[index];
                            
                            switch (patternChar)
                            {
                                case '0':
                                    if (currentEntry.P0 == null)
                                    {
                                        currentEntry.P0 = new HuffmanEntry();
                                        currentEntry = currentEntry.P0;
                                        if (index == pattern.Length - 1)
                                            currentEntry.Value = resolveChar(parts[2]).ToString();
                                    }
                                    else
                                    {
                                        currentEntry = currentEntry.P0;
                                        if (currentEntry.HoldsValue && index == pattern.Length - 1)
                                            Logger.Instance.Write("Dictionary entry already set");
                                    }
                                    break;
                                case '1':
                                    if (currentEntry.P1 == null)
                                    {
                                        currentEntry.P1 = new HuffmanEntry();
                                        currentEntry = currentEntry.P1;
                                        if (index == pattern.Length - 1)
                                            currentEntry.Value = resolveChar(parts[2]).ToString();
                                    }
                                    else
                                    {
                                        currentEntry = currentEntry.P1;
                                        if (currentEntry.HoldsValue && index == pattern.Length - 1)
                                            Logger.Instance.Write("Dictionary entry already set: " + line);
                                    }
                                    break;
                                default:
                                    break;
                            }
                        }                        
                    }
                }
            }

            streamReader.Close();
            fileStream.Close();
        }

        /// <summary>
        /// Decode a Multi-tree text string which includes the text prefix (ie 0x1f?? where ?? indicates the table number 1 or 2).
        /// </summary>
        /// <param name="byteData">The encoded string.</param>
        /// <param name="encoding">The encoding used for the decompressed bytes.</param>
        /// <returns>The decoded string.</returns>
        public static string DecodeData(byte[] byteData, string encoding)
        {
            if (byteData[1] == 1)
                return(decodeData(byteData, table1Roots, 2, encoding));
            else
                return(decodeData(byteData, table2Roots, 2, encoding));
        }

        /// <summary>
        /// Decode a Multi-tree text string with no prefix.
        /// </summary>
        /// <param name="table">The decode table to use (1 or 2).</param>
        /// <param name="byteData">The encoded string.</param>
        /// <param name="encoding">The encoding used for the decompressed bytes.</param>
        /// <returns>The decoded string.</returns>
        public static string DecodeData(int table, byte[] byteData, string encoding)
        {
            if (table == 1)
                return (decodeData(byteData, table1Roots, 0, encoding));
            else
                return (decodeData(byteData, table2Roots, 0, encoding));
        }

        private static string decodeData(byte[] byteData, HuffmanEntry[] roots, int startIndex, string encoding)
        {
            byte[] decompressedBytes = getByteBuffer(null);
            int decompressedIndex = 0;

            Encoding sourceEncoding = sourceEncoding = Encoding.GetEncoding(encoding);            

            if (encodings == null)
                encodings = new Collection<string>();
            if (!encodings.Contains(encoding))
                encodings.Add(encoding);

            HuffmanEntry currentEntry = roots[0];
            byte mask = 0x80;
            bool finished = false;
            EscapeCount = 0;
            
            for (int index = startIndex; index < byteData.Length && !finished; index++)
            {
                byte dataByte = byteData[index];

                while (mask > 0 && !finished)
                {
                    if (currentEntry.HoldsValue)
                    {
                        switch ((int)currentEntry.Value[0])
                        {
                            case stop:
                                finished = true;
                                break;
                            case escape:                                
                                bool escapeDone = false;

                                while (!escapeDone)
                                {
                                    byte encodedValue = 0x00;

                                    for (int bitCount = 0; bitCount < 8; bitCount++)
                                    {
                                        encodedValue = (byte)(encodedValue << 1);

                                        if ((dataByte & mask) != 0)
                                            encodedValue |= 0x01;

                                        mask = (byte)(mask >> 1);
                                        if (mask == 0)
                                        {
                                            index++;
                                            dataByte = byteData[index];
                                            mask = 0x80;
                                        }
                                    }

                                    if (encodedValue > 0x1f)
                                        decompressedBytes = storeDecompressedByte(encodedValue, decompressedBytes, ref decompressedIndex);

                                    int length;

                                    if ((encodedValue & 0xe0) == 0xc0) // UTF-8 2 bytes 
                                        length = 2;
                                    else
                                    {
                                        if ((encodedValue & 0xf0) == 0xe0) // UTF-8 3 bytes 
                                            length = 3;
                                        else
                                        {
                                            if ((encodedValue & 0xf8) == 0xf0) // UTF-8 4 bytes 
                                                length = 4;
                                            else
                                                length = 1; // ASCII byte
                                        }
                                    }

                                    if (DebugEntry.IsDefined(DebugName.LogHuffman))
                                        Logger.Instance.Write("HD: Escaped length is " + length + " byte 0 encoded value was 0x" + encodedValue.ToString("x"));

                                    if (length == 1)
                                        singleByteEscapes++;
                                    else
                                    {
                                        multiByteEscapes++;
                                        EscapeCount++;
                                    }
                                    
                                    while (length > 1)
                                    {
                                        encodedValue = 0x00;

                                        for (int bitCount = 0; bitCount < 8; bitCount++)
                                        {
                                            encodedValue = (byte)(encodedValue << 1);

                                            if ((dataByte & mask) != 0)
                                                encodedValue |= 0x01;

                                            mask = (byte)(mask >> 1);
                                            if (mask == 0)
                                            {
                                                index++;
                                                dataByte = byteData[index];
                                                mask = 0x80;
                                            }
                                        }

                                        decompressedBytes = storeDecompressedByte(encodedValue, decompressedBytes, ref decompressedIndex);
                                        if (DebugEntry.IsDefined(DebugName.LogHuffman) && length > 1)
                                            Logger.Instance.Write("HD: byte encoded value was 0x" + encodedValue.ToString("x"));

                                        length--;
                                    }

                                    if (encodedValue < 0x20)
                                    {
                                        /*finished = true;
                                        escapeDone = true;*/
                                        currentEntry = roots[encodedValue];
                                        escapeDone = true;
                                    }
                                    else
                                    {
                                        if (encodedValue < 0x80)
                                        {
                                            currentEntry = roots[encodedValue];
                                            escapeDone = true;
                                        }
                                    }
                                }
                                                                
                                break;                                
                            default:
                                decompressedBytes = storeDecompressedByte((byte)currentEntry.Value[0], decompressedBytes, ref decompressedIndex);                                                                
                                currentEntry = roots[(int)currentEntry.Value[0]];
                                break;
                        }
                    }

                    if (!finished)
                    {
                        if ((dataByte & mask) == 0)
                        {
                            if (currentEntry != null && currentEntry.P0 != null)
                                currentEntry = currentEntry.P0;
                            else
                            {
                                string outputString = sourceEncoding.GetString(decompressedBytes, 0, decompressedIndex);

                                Logger.Instance.Write(" ** DECOMPRESSION FAILED **");
                                Logger.Instance.Write("Original data: " + Utils.ConvertToHex(byteData));
                                Logger.Instance.Write("Decoded data: " + outputString.ToString());
                                return (outputString.ToString() + " ** DECOMPRESSION FAILED **");
                            }
                        }
                        else
                        {
                            if (currentEntry != null && currentEntry.P1 != null)
                                currentEntry = currentEntry.P1;
                            else
                            {
                                string outputString = sourceEncoding.GetString(decompressedBytes, 0, decompressedIndex);
                                
                                Logger.Instance.Write(" ** DECOMPRESSION FAILED **");
                                Logger.Instance.Write("Original data: " + Utils.ConvertToHex(byteData));
                                Logger.Instance.Write("Decoded data: " + outputString.ToString());
                                return (outputString.ToString() + " ** DECOMPRESSION FAILED **");
                            }
                        }

                        mask = (byte)(mask >> 1);
                    }
                }

                mask = 0x80;
            }

            if (decompressedIndex != 0)
            {
                string response = sourceEncoding.GetString(decompressedBytes, 0, decompressedIndex); 
                /*if (WasEscaped)
                    Logger.Instance.Write("HD: " + response);*/

                return (response);
            }
            else
                return (string.Empty);
        }

        private static byte[] storeDecompressedByte(byte decompressedByte, byte[] decompressedBytes, ref int decompressedIndex)
        {
            if (decompressedByte == 0x00)
                return (decompressedBytes);

            byte[] outputBuffer;

            if (decompressedIndex > decompressedBytes.Length)
                outputBuffer = getByteBuffer(decompressedBytes);
            else
                outputBuffer = decompressedBytes;

            outputBuffer[decompressedIndex] = decompressedByte;
            decompressedIndex++;

            return (outputBuffer);
        }

        private static byte[] getByteBuffer(byte[] existingBuffer)
        {
            int size = 1024;

            if (existingBuffer != null)
                size += existingBuffer.Length;

            byte[] newBuffer = new byte[size];

            if (existingBuffer != null)
                Array.Copy(existingBuffer, newBuffer, existingBuffer.Length);

            return (newBuffer);
        }

        private static char resolveChar(string input)
        {
            int val = new int();
            char myChar = input[0]; //default value

            switch (input.ToUpper())
            {
                case "START":
                    myChar = (char)0x00;
                    break;
                case "STOP":
                    myChar = (char)0x02;
                    break;
                case "ESCAPE":
                    myChar = (char)0x01;
                    break;
                default:
                    try
                    {
                        if (input.Substring(0, 2) == "0x")
                        {
                            val = int.Parse(input.Substring(2, input.Length - 2), NumberStyles.AllowHexSpecifier); //ASCII for the input character
                        }
                        myChar = (char)val;
                    }
                    catch
                    {

                    }
                    break;
            }

            return (myChar);
        }

        /// <summary>
        /// Log the decoding usage.
        /// </summary>
        public static void LogUsage()
        {
            if (encodings == null)
                return;

            Logger.Instance.WriteSeparator("Huffman Usage");

            StringBuilder text = new StringBuilder();

            foreach (string encoding in encodings)
            {
                if (text.Length != 0)
                    text.Append(", ");
                text.Append(encoding);                
            }

            Logger.Instance.Write("Huffman encodings used: " + text);
            Logger.Instance.Write("Huffman single byte escape sequences: " + singleByteEscapes);
            Logger.Instance.Write("Huffman multi byte escape sequences: " + multiByteEscapes);

            Logger.Instance.WriteSeparator("End Of Huffman Usage");

            encodings = null;
            singleByteEscapes = 0;
            multiByteEscapes = 0;
        }
    }
}
