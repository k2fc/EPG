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
using System.IO;
using System.Xml;

namespace DomainObjects
{
    /// <summary>
    /// The class that describes the carousels containg EIT data.
    /// </summary>
    public class EITCarouselFile
    {
        /// <summary>
        /// Get the collection of carousel definitions.
        /// </summary>
        public static Collection<EITCarousel> Carousels
        {
            get
            {
                if (carousels == null)
                    carousels = new Collection<EITCarousel>();
                return (carousels);
            }
        }

        /// <summary>
        /// Get the standard name of the carousel file.
        /// </summary>
        public static string FileName { get { return ("EIT Carousels"); } }

        private static Collection<EITCarousel> carousels;

        /// <summary>
        /// Initialize a new instance of the EITCarouselFile class.
        /// </summary>
        public EITCarouselFile() { }

        /// <summary>
        /// Find a carousel.
        /// </summary>
        /// <param name="name">The name of the carousel.</param>
        public static EITCarousel FindCarousel(string name)
        {
            if (carousels == null)
                return (null);

            foreach (EITCarousel carousel in Carousels)
            {
                if (carousel.Name.ToLowerInvariant() == name.ToLowerInvariant())
                    return (carousel);
            }

            return (null);
        }

        /// <summary>
        /// Get a list of all the carousel names.
        /// </summary>
        /// <returns>A list of table names.</returns>
        public static Collection<string> GetNameList()
        {
            if (carousels == null)
                Load();

            if (carousels == null)
                return (null);

            Collection<string> names = new Collection<string>();

            foreach (EITCarousel carousel in carousels)
                addName(names, carousel.Name);

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
        /// Load the carousel definitions.
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
            if (carousels != null)
                return (true);

            Logger.Instance.Write("Loading EIT Carousel tables from " + fileName);

            carousels = new Collection<EITCarousel>();

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
                            case "carousel":
                                string name = reader.GetAttribute("name");
                                string format = reader.GetAttribute("format");
                                string suffix = reader.GetAttribute("suffix");
                                
                                if (name != null)
                                {
                                    EITCarousel carousel = new EITCarousel(name, format, suffix);
                                    carousel.Load(reader.ReadSubtree(), fileName);
                                    carousels.Add(carousel);
                                }
                                else
                                    Logger.Instance.Write("Failed to parse EIT Carousel - name and/or pid attribute missing");
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

            if (carousels != null && carousels.Count > 0)
                Logger.Instance.Write("Loaded " + carousels.Count + " EIT carousels(s)");
            else
                Logger.Instance.Write("No EIT carousels loaded");

            return (true);
        }
    }
}
