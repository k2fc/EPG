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
using System.Xml;

namespace XmltvParser
{
    /// <summary>
    /// The class that describes an XMLTV length tag.
    /// </summary>
    public class XmltvLength
    {
        /// <summary>
        /// Get the units.
        /// </summary>
        public string Units { get; private set; }
        /// <summary>
        /// Get the length.
        /// </summary>
        public string Length { get; private set; }

        /// <summary>
        /// Get the duration.
        /// </summary>
        public TimeSpan? Duration
        {
            get
            {
                if (Units != null)
                {
                    switch (Units)
                    {
                        case "seconds":
                            return (new TimeSpan(Int32.Parse(Length) * TimeSpan.TicksPerSecond));
                        case "minutes":
                            return (new TimeSpan(Int32.Parse(Length) * 60 * TimeSpan.TicksPerSecond));
                        case "hours":
                            return (new TimeSpan(Int32.Parse(Length) * 60 * 60 * TimeSpan.TicksPerSecond));
                        default:
                            return (new TimeSpan(Int32.Parse(Length) * 60 * TimeSpan.TicksPerSecond));
                    }
                }
                else
                    return(new TimeSpan(Int32.Parse(Length) * 60 * TimeSpan.TicksPerSecond));                     
            }
        }

        private XmltvLength() { }

        private void load(XmlReader xmlReader)
        {
            Units = xmlReader.GetAttribute("units");
            Length = xmlReader.ReadString();            
        }

        /// <summary>
        /// Get a loaded instance of the class.
        /// </summary>
        /// <param name="xmlReader">An XmlReader instance for the tag.</param>
        /// <returns>An instance of the class with the tag data loaded.</returns>
        public static XmltvLength GetInstance(XmlReader xmlReader)
        {
            XmltvLength instance = new XmltvLength();
            instance.load(xmlReader);

            return (instance);
        }
    }
}
