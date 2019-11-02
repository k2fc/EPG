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

namespace DomainObjects
{
    /// <summary>
    /// The class that describes a service provider.
    /// </summary>
    public class Service
    {
        /// <summary>
        /// Get or set the areas in the service.
        /// </summary>
        public Collection<Area> Areas { get; set; }

        /// <summary>
        /// Get the name of the service.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Get the type of service.
        /// </summary>
        public ServiceType ServiceType { get; private set; }

        private Service() { }

        /// <summary>
        /// Initialize a new instance of the Service class.
        /// </summary>
        /// <param name="name">The name of the service.</param>
        /// <param name="serviceType">The type of service.</param>
        public Service(string name, ServiceType serviceType)
        {
            Name = name;
            ServiceType = serviceType;
        }
    }
}
