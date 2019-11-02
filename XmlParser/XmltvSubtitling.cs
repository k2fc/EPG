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

using System.Xml;
using System.IO;

using DomainObjects;

namespace XmltvParser
{
    /// <summary>
    /// The class that describes an XMLTV subtitles tag.
    /// </summary>
    public class XmltvSubtitling
    {
        /// <summary>
        /// Get the type.
        /// </summary>
        public string Type { get; private set; }
        /// <summary>
        /// Get the language.
        /// </summary>
        public string Language { get; private set; }        

        private XmltvSubtitling() { }

        private bool load(XmlReader xmlReader)
        {
            try
            {
                while (!xmlReader.EOF)
                {
                    if (xmlReader.IsStartElement())
                    {
                        switch (xmlReader.Name.ToLowerInvariant())
                        {
                            case "subtitles":
                                Type = xmlReader.GetAttribute("type");
                                break;
                            case "language":
                                Language = xmlReader.ReadString();
                                break;
                            default:
                                break;
                        }
                    }

                    xmlReader.Read();
                }
            }
            catch (XmlException e)
            {
                Logger.Instance.Write("Failed to load xmltv subtitles");
                Logger.Instance.Write("Data exception: " + e.Message);
                return (false);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("Failed to load xmltv subtitles");
                Logger.Instance.Write("I/O exception: " + e.Message);
                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Get a loaded instance of the class.
        /// </summary>
        /// <param name="xmlReader">An XmlReader instance for the tag.</param>
        /// <returns>An instance of the class with the tag data loaded.</returns>
        public static XmltvSubtitling GetInstance(XmlReader xmlReader)
        {
            XmltvSubtitling instance = new XmltvSubtitling();
            instance.load(xmlReader);

            return (instance);
        }
    }
}
