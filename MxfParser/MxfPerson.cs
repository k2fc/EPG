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
    /// The class that describes an MXF person.
    /// </summary>
    public class MxfPerson
    {
        /// <summary>
        /// Get or set the collection of people in an MXF file.
        /// </summary>
        public static Collection<MxfPerson> People { get; set; }

        /// <summary>
        /// Get the ID of the person.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Get the name of the person.
        /// </summary>
        public string Name { get; private set; }

        private MxfPerson() { }

        private bool load(XmlReader xmlReader)
        {
            try
            {
                Id = xmlReader.GetAttribute("id");
                Name = xmlReader.GetAttribute("name");                
            }
            catch (XmlException e)
            {
                Logger.Instance.Write("Failed to load mxf person");
                Logger.Instance.Write("Data exception: " + e.Message);
                return (false);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("Failed to load mxf person");
                Logger.Instance.Write("I/O exception: " + e.Message);
                return (false);
            }

            return (true);
        }

        internal static MxfPerson FindPerson(string id)
        {
            if (People == null)
                return (null);

            foreach (MxfPerson person in People)
            {
                if (person.Id == id)
                    return (person);
            }

            return (null);
        }

        /// <summary>
        /// Get a new instance of the MxfPerson class with data loaded.
        /// </summary>
        /// <param name="xmlReader">An XmlReader instance for the channel tag.</param>
        /// <returns>An MxfPerson instance with data loaded.</returns>
        public static MxfPerson GetInstance(XmlReader xmlReader)
        {
            MxfPerson instance = new MxfPerson();
            instance.load(xmlReader);

            return (instance);
        }
    }
}
