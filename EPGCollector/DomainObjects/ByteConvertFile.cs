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
using System.IO;
using System.Xml;

namespace DomainObjects
{
    /// <summary>
    /// The class that describes the byte conversion file.
    /// </summary>
    public class ByteConvertFile
    {
        /// <summary>
        /// Get the collection of conversion tables.
        /// </summary>
        public static Collection<ByteConvertTable> Tables
        {
            get
            {
                if (tables == null)
                    tables = new Collection<ByteConvertTable>();
                return (tables);
            }
        }

        /// <summary>
        /// Get the standard name of the conversion file.
        /// </summary>
        public static string FileName { get { return ("Byte Conversion Tables"); } }

        private static Collection<ByteConvertTable> tables;

        /// <summary>
        /// Initialize a new instance of the ByteConvertFile class.
        /// </summary>
        public ByteConvertFile() { }

        /// <summary>
        /// Find a conversion entry.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <param name="controlCode">The control code for the byte.</param>
        /// <param name="originalValue">The original value of the byte.</param>
        /// <returns>The converted byte value of 0x00 if the table or original value cannot be located.</returns>
        public static byte FindEntry(string tableName, byte controlCode, byte originalValue)
        {
            if (tables == null)
                return (0x00);

            foreach (ByteConvertTable table in tables)
            {
                if (table.TableName.ToLowerInvariant() == tableName.ToLowerInvariant())
                    return (table.FindEntry(controlCode, originalValue));
            }

            return (0x00);
        }

        /// <summary>
        /// Get a list of all the conversion table names.
        /// </summary>
        /// <returns>A list of table names.</returns>
        public static Collection<string> GetTableNameList()
        {
            if (tables == null)
                Load();

            if (tables == null)
                return (null);

            Collection<string> names = new Collection<string>();

            foreach (ByteConvertTable table in tables)
                addName(names, table.TableName);

            return (names);
        }

        private static void addName(Collection<string> names, string newName)
        {
            foreach (string oldName in names)
            {
                if (oldName == newName)
                    return;

                if (oldName.CompareTo(newName) > 0)
                {
                    names.Insert(names.IndexOf(oldName), newName);
                    return;
                }
            }

            names.Add(newName);
        }

        /// <summary>
        /// Load the definitions.
        /// </summary>
        /// <returns>True if the file has been loaded; false otherwise.</returns>
        public static bool Load()
        {
            string actualFileName = Path.Combine(RunParameters.DataDirectory, FileName + ".cfg");
            if (!File.Exists(actualFileName))
            {
                actualFileName = Path.Combine(RunParameters.ConfigDirectory, FileName + ".cfg");
                if (!File.Exists(actualFileName))
                    return (true);
            }

            return (Load(actualFileName));
        }

        /// <summary>
        /// Load the definitions.
        /// </summary>
        /// <returns>True if the file has been loaded; false otherwise.</returns>
        public static bool Load(string fileName)
        {
            if (tables != null)
                return (true);

            Logger.Instance.Write("Loading Byte Conversion tables from " + fileName);

            tables = new Collection<ByteConvertTable>();

            XmlReader reader = null;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;

            try
            {
                reader = XmlReader.Create(fileName, settings);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("<E> Failed to open " + fileName);
                Logger.Instance.Write("<E> " + e.Message);
                return (false);
            }

            try
            {
                while (!reader.EOF)
                {
                    reader.Read();
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name.ToLowerInvariant())
                        {
                            case "table":
                                string nameString = reader.GetAttribute("name");
                                if (nameString != null)
                                {
                                    ByteConvertTable table = new ByteConvertTable(nameString);
                                    table.Load(reader.ReadSubtree(), fileName);
                                    tables.Add(table);
                                }
                                else
                                    Logger.Instance.Write("Failed to parse byte conversion table - name attribute missing");
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (XmlException e)
            {
                Logger.Instance.Write("Failed to load file " + fileName);
                Logger.Instance.Write("Data exception: " + e.Message);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("Failed to load file " + fileName);
                Logger.Instance.Write("I/O exception: " + e.Message);
            }

            if (reader != null)
                reader.Close();

            if (tables != null && tables.Count > 0)
                Logger.Instance.Write("Loaded " + tables.Count + " byte conversion table(s)");
            else
                Logger.Instance.Write("No byte conversion tables loaded");

            return (true);
        }
    }
}
