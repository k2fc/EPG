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
    /// The class that describes an MXF guide image.
    /// </summary>
    public class MxfGuideImage
    {
        /// <summary>
        /// Get or set the collection of guide images in an MXF file.
        /// </summary>
        public static Collection<MxfGuideImage> GuideImages { get; set; }

        /// <summary>
        /// Get the ID of the image.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Get the URL of the image.
        /// </summary>
        public string ImageUrl { get; private set; }

        private MxfGuideImage() { }

        private bool load(XmlReader xmlReader)
        {
            try
            {
                Id = xmlReader.GetAttribute("id");
                ImageUrl = xmlReader.GetAttribute("imageUrl");
            }
            catch (XmlException e)
            {
                Logger.Instance.Write("Failed to load mxf guide image");
                Logger.Instance.Write("Data exception: " + e.Message);
                return (false);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("Failed to load mxf guide image");
                Logger.Instance.Write("I/O exception: " + e.Message);
                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Get a new instance of the MxfGuideImage class with data loaded.
        /// </summary>
        /// <param name="xmlReader">An XmlReader instance for the channel tag.</param>
        /// <returns>An MxfGuideImage instance with data loaded.</returns>
        public static MxfGuideImage GetInstance(XmlReader xmlReader)
        {
            MxfGuideImage instance = new MxfGuideImage();
            instance.load(xmlReader);

            return (instance);
        }
    }
}
