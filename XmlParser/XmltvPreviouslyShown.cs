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

namespace XmltvParser
{
    /// <summary>
    /// The class that describes an XMLTV previously shown tag.
    /// </summary>
    public class XmltvPreviouslyShown
    {
        /// <summary>
        /// Get the start time.
        /// </summary>
        public XmltvTime Start { get; private set; }
        /// <summary>
        /// Get the channel.
        /// </summary>
        public string Channel { get; private set; }
        
        private XmltvPreviouslyShown() { }

        private void load(XmlReader xmlReader)
        {
            Start = XmltvTime.GetInstance(xmlReader.GetAttribute("start"));
            Channel = xmlReader.GetAttribute("channel");            
        }

        /// <summary>
        /// Get a loaded instance of the class.
        /// </summary>
        /// <param name="xmlReader">An XmlReader instance for the tag.</param>
        /// <returns>An instance of the class with the tag data loaded.</returns>
        public static XmltvPreviouslyShown GetInstance(XmlReader xmlReader)
        {
            XmltvPreviouslyShown instance = new XmltvPreviouslyShown();
            instance.load(xmlReader);

            return (instance);
        }
    }
}
