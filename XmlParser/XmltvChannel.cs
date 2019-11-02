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
using System.Xml;
using System.IO;

using DomainObjects;

namespace XmltvParser
{
    /// <summary>
    /// The class that describes an XMLTV channel.
    /// </summary>
    public class XmltvChannel
    {
        /// <summary>
        /// Get or set the collection of channels in an XMLTV file.
        /// </summary>
        public static Collection<XmltvChannel> Channels { get; set; }
 
        /// <summary>
        /// Get the ID of the channel.
        /// </summary>
        public string Id { get; private set; }
        /// <summary>
        /// Get the display names of a channel.
        /// </summary>
        public Collection<XmltvText> DisplayNames { get; private set; }
        /// <summary>
        /// Get the icon that represents the channel.
        /// </summary>
        public XmltvIcon Icon { get; private set; }
        /// <summary>
        /// Get the URL's that are related to the channel.
        /// </summary>
        public Collection<string> Urls { get; private set; }

        /// <summary>
        /// Get or set the format of the ID attribute.
        /// </summary>
        public XmltvIdFormat IdFormat { get; set; }

        private XmltvChannel() { }

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
                            case "channel":
                                Id = xmlReader.GetAttribute("id");
                                break;
                            case "display-name":
                                if (DisplayNames == null)
                                    DisplayNames = new Collection<XmltvText>();
                                DisplayNames.Add(XmltvText.GetInstance(xmlReader));
                                break;
                            case "icon":
                                Icon = XmltvIcon.GetInstance(xmlReader);
                                break;
                            case "url":
                                if (Urls == null)
                                    Urls = new Collection<string>();
                                Urls.Add(xmlReader.ReadString());
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
                Logger.Instance.Write("Failed to load xmltv channel");
                Logger.Instance.Write("Data exception: " + e.Message);
                return (false);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("Failed to load xmltv channel");
                Logger.Instance.Write("I/O exception: " + e.Message);
                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Get a new instance of the XmltvChannel class with data loaded.
        /// </summary>
        /// <param name="xmlReader">An XmlReader instance for the channel tag.</param>
        /// <returns>An XmltvChannel instance with data loaded.</returns>
        public static XmltvChannel GetInstance(XmlReader xmlReader)
        {
            XmltvChannel instance = new XmltvChannel();
            instance.load(xmlReader);

            return (instance);
        }
    }
}
