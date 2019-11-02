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
using System.IO;

using DomainObjects;

namespace MxfParser
{
    /// <summary>
    /// The class that describes an MXF service.
    /// </summary>
    public class MxfService
    {
        /// <summary>
        /// Get or set the collection of services in an MXF file.
        /// </summary>
        public static Collection<MxfService> Services { get; set; }

        /// <summary>
        /// Get the ID of the service.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Get the UID of the service.
        /// </summary>
        public string Uid { get; private set; }

        /// <summary>
        /// Get the name of the service.
        /// </summary>
        public string Name { get; private set; }

        private MxfService() { }

        private bool load(XmlReader xmlReader)
        {
            try
            {
                Id = xmlReader.GetAttribute("id");
                Uid = xmlReader.GetAttribute("uid");
                Name = xmlReader.GetAttribute("name");
            }
            catch (XmlException e)
            {
                Logger.Instance.Write("Failed to load mxf service");
                Logger.Instance.Write("Data exception: " + e.Message);
                return (false);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("Failed to load mxf service");
                Logger.Instance.Write("I/O exception: " + e.Message);
                return (false);
            }

            return (true);
        }

        internal static MxfService FindService(string id)
        {
            if (Services == null)
                return (null);

            foreach (MxfService service in Services)
            {
                if (service.Id == id)
                    return (service);
            }

            return (null);
        }

        internal int[] GetServiceIds()
        {
            string[] uidParts = Uid.Split(new char[] { '!' });
            if (uidParts.Length != 3 || uidParts[0].Length != 0 || uidParts[1] != "Service")
                return (null);

            string[] ids;
            string inet = "inet_";

            if (!uidParts[2].StartsWith(inet))
            {
                ids = uidParts[2].Split(new char[] { ':' });
                if (ids.Length != 3)
                    return (null);
            }
            else
            {
                ids = uidParts[2].Substring(inet.Length).Split(new char[] { ':' });
                if (ids.Length != 4)
                    return (null);
            }

            int[] serviceIds = new int[3];

            try
            {
                serviceIds[0] = Int32.Parse(ids[0]);
                serviceIds[1] = Int32.Parse(ids[1]);
                serviceIds[2] = Int32.Parse(ids[2]);
                return (serviceIds);
            }
            catch (FormatException)
            {
                return (null);
            }
            catch (OverflowException)
            {
                return (null);
            }
        }

        /// <summary>
        /// Get a new instance of the MxfService class with data loaded.
        /// </summary>
        /// <param name="xmlReader">An XmlReader instance for the channel tag.</param>
        /// <returns>An MxfService instance with data loaded.</returns>
        public static MxfService GetInstance(XmlReader xmlReader)
        {
            MxfService instance = new MxfService();
            instance.load(xmlReader);

            return (instance);
        }
    }
}
