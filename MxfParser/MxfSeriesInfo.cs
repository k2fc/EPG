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

namespace MxfParser
{
    /// <summary>
    /// The class that describes an MXF series info entry.
    /// </summary>
    public class MxfSeriesInfo
    {
        /// <summary>
        /// Get or set the collection of series infos in an MXF file.
        /// </summary>
        public static Collection<MxfSeriesInfo> SeriesInfos { get; set; }

        /// <summary>
        /// Get the ID of the series info.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Get the title of the series info.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Get the short title of the series info.
        /// </summary>
        public string ShortTitle { get; private set; }

        /// <summary>
        /// Get the description of the series info.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Get the short description of the series info.
        /// </summary>
        public string ShortDescription { get; private set; }

        /// <summary>
        /// Get the start air date of the series info.
        /// </summary>
        public string StartAirDate { get; private set; }

        /// <summary>
        /// Get the end air date of the seriesw info.
        /// </summary>
        public string EndAirDate { get; private set; }

        private MxfSeriesInfo() { }

        private bool load(XmlReader xmlReader)
        {
            try
            {
                Id = xmlReader.GetAttribute("id");
                Title = xmlReader.GetAttribute("title");
                ShortTitle = xmlReader.GetAttribute("shortTitle");
                Description = xmlReader.GetAttribute("description");
                ShortDescription = xmlReader.GetAttribute("shortDescription");
                StartAirDate = xmlReader.GetAttribute("startAirdate");
                EndAirDate = xmlReader.GetAttribute("endAirdate");
            }
            catch (XmlException e)
            {
                Logger.Instance.Write("Failed to load mxf series info");
                Logger.Instance.Write("Data exception: " + e.Message);
                return (false);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("Failed to load mxf series info");
                Logger.Instance.Write("I/O exception: " + e.Message);
                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Get a new instance of the MxfSeriesInfo class with data loaded.
        /// </summary>
        /// <param name="xmlReader">An XmlReader instance for the channel tag.</param>
        /// <returns>An MxfSeriesInfo instance with data loaded.</returns>
        public static MxfSeriesInfo GetInstance(XmlReader xmlReader)
        {
            MxfSeriesInfo instance = new MxfSeriesInfo();
            instance.load(xmlReader);

            return (instance);
        }
    }
}
