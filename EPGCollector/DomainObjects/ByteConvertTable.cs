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
using System.Xml;
using System.Globalization;

namespace DomainObjects
{
    /// <summary>
    /// The class that describes a byte conversion table.
    /// </summary>
    public class ByteConvertTable
    {
        /// <summary>
        /// Get the name of the table.
        /// </summary>
        public string TableName { get; private set; }
        /// <summary>
        /// Get the collection of table entries.
        /// </summary>
        public Collection<ByteConvertEntry> Entries { get; private set; }

        private ByteConvertTable() { }

        /// <summary>
        /// Initialize a new instance of the ByteConvertTable class.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        public ByteConvertTable(string tableName)
        {
            TableName = tableName;

            Entries = new Collection<ByteConvertEntry>();
        }

        /// <summary>
        /// Find a conversion entry.
        /// </summary>
        /// <param name="controlCode">The control code for the byte.</param>
        /// <param name="originalValue">The original value of the byte.</param>
        /// <returns></returns>
        public byte FindEntry(byte controlCode, byte originalValue)
        {
            foreach (ByteConvertEntry entry in Entries)
            {
                if (entry.ControlCode == controlCode && entry.OriginalValue == originalValue)
                    return (entry.ConvertedValue);
            }

            return (0x00);
        }

        internal void Load(XmlReader reader, string fileName)
        {
            byte currentControlCode = 0x00;

            while (!reader.EOF)
            {
                reader.Read();
                if (reader.IsStartElement())
                {
                    switch (reader.Name.ToLowerInvariant())
                    {
                        case "controlcode":
                            string codeString = reader.GetAttribute("code");
                            if (codeString != null && codeString.Length ==4 && codeString.StartsWith("0x"))
                            {
                                try
                                {
                                    currentControlCode = byte.Parse(codeString.Substring(2), NumberStyles.HexNumber | NumberStyles.AllowHexSpecifier);
                                }
                                catch (FormatException e)
                                {
                                    Logger.Instance.Write("Failed to parse byte conversion control code - " + codeString);
                                    Logger.Instance.Write("Format exception: " + e.Message);
                                }
                                catch (OverflowException e)
                                {
                                    Logger.Instance.Write("Failed to parse byte conversion control code - " + codeString);
                                    Logger.Instance.Write("Overflow exception: " + e.Message);
                                }
                            }
                            else
                                Logger.Instance.Write("Failed to parse byte conversion control code - code attribute missing or in the wrong format");
                            break;
                        case "conversion":
                            byte originalValue;

                            string xmlOriginal = reader.GetAttribute("original");
                            if (xmlOriginal == null)
                            {
                                Logger.Instance.Write("Failed to parse byte conversion control code - original attribute missing");
                                break;
                            }

                            if (!xmlOriginal.StartsWith("0x"))
                            {
                                if (xmlOriginal.Length == 1)
                                    originalValue = (byte)xmlOriginal[0];
                                else
                                {
                                    Logger.Instance.Write("Failed to parse byte conversion original attribute - length invalid for character value");
                                    break;
                                }
                            }
                            else
                            {
                                if (xmlOriginal.Length == 4)
                                {
                                    try
                                    {
                                        originalValue = byte.Parse(xmlOriginal.Substring(2), NumberStyles.HexNumber | NumberStyles.AllowHexSpecifier);
                                    }
                                    catch (FormatException e)
                                    {
                                        Logger.Instance.Write("Failed to parse byte conversion original attribute - " + xmlOriginal);
                                        Logger.Instance.Write("Format exception: " + e.Message);
                                        break;
                                    }
                                    catch (OverflowException e)
                                    {
                                        Logger.Instance.Write("Failed to parse byte conversion original attribute - " + xmlOriginal);
                                        Logger.Instance.Write("Overflow exception: " + e.Message);
                                        break;
                                    }
                                }
                                else
                                {
                                    Logger.Instance.Write("Failed to parse byte conversion original attribute - length invalid for character value");
                                    break;
                                }
                            }

                            byte newValue;

                            string xmlNew = reader.GetAttribute("newValue");
                            if (xmlNew == null)
                            {
                                Logger.Instance.Write("Failed to parse byte conversion control code - newValue attribute missing");
                                break;
                            }

                            if (!xmlNew.StartsWith("0x"))
                            {
                                if (xmlNew.Length == 1)
                                    newValue = (byte)xmlNew[0];
                                else
                                {
                                    Logger.Instance.Write("Failed to parse byte conversion newValue attribute - length invalid for character value");
                                    break;
                                }
                            }
                            else
                            {
                                if (xmlNew.Length == 4)
                                {
                                    try
                                    {
                                        newValue = byte.Parse(xmlNew.Substring(2), NumberStyles.HexNumber | NumberStyles.AllowHexSpecifier);
                                    }
                                    catch (FormatException e)
                                    {
                                        Logger.Instance.Write("Failed to parse byte conversion newValue attribute - " + xmlOriginal);
                                        Logger.Instance.Write("Format exception: " + e.Message);
                                        break;
                                    }
                                    catch (OverflowException e)
                                    {
                                        Logger.Instance.Write("Failed to parse byte conversion newValue attribute - " + xmlOriginal);
                                        Logger.Instance.Write("Overflow exception: " + e.Message);
                                        break;
                                    }
                                }
                                else
                                {
                                    Logger.Instance.Write("Failed to parse byte conversion newValue attribute - length invalid for character value");
                                    break;
                                }
                            }

                            Entries.Add(new ByteConvertEntry(currentControlCode, originalValue, newValue));
                                    
                            break;
                        default:
                            break;
                    }
                }             
            }
            
            reader.Close();
        }
    }
}
