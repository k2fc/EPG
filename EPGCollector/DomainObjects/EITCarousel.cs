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
    /// The class that describes an EIT carousel.
    /// </summary>
    public class EITCarousel
    {
        /// <summary>
        /// Get the name of the carousel.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Get the format of the files in the carousel.
        /// </summary>
        public string Format { get; private set; }
        /// <summary>
        /// Get the suffix of the files in the carousel.
        /// </summary>
        public string Suffix { get; private set; }
        /// <summary>
        /// Get the list of pid specs.
        /// </summary>
        public Collection<EITCarouselPidSpec> PidSpecs { get; private set; }
        /// <summary>
        /// Get the path to the Zip executable.
        /// </summary>
        public string ZipExePath { get; private set; } 

        private EITCarousel() { }

        /// <summary>
        /// Initialize a new instance of the EITCarousel class.
        /// </summary>
        /// <param name="name">The name of the carousel.</param>
        /// <param name="format">The format of the files in the carousel.</param>
        /// <param name="suffix">The suffix of the files in the carousel.</param>
        public EITCarousel(string name, string format, string suffix)
        {
            Name = name;
            Format = format;
            Suffix = suffix;
        }

        internal void Load(XmlReader reader, string fileName)
        {
            while (!reader.EOF)
            {
                reader.Read();
                if (reader.IsStartElement())
                {
                    switch (reader.Name.ToLowerInvariant())
                    {
                        case "pidspec":
                            if (PidSpecs == null)
                                PidSpecs = new Collection<EITCarouselPidSpec>();

                            EITCarouselPidSpec pidSpec = new EITCarouselPidSpec();
                            pidSpec.Load(Int32.Parse(reader.GetAttribute("pid")), reader.ReadSubtree());
                            PidSpecs.Add(pidSpec);
                            break;
                        case "zipexepath":
                            ZipExePath = reader.ReadString().Trim();
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
