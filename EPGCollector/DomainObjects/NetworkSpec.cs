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

using System.Net;

namespace DomainObjects
{
    /// <summary>
    /// The class that defines a network.
    /// </summary>
    public class NetworkSpec
    {
        /// <summary>
        /// Get the name of the network.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Get the IP address of the network.
        /// </summary>
        public IPAddress Address { get; private set; }

        private NetworkSpec() { }

        /// <summary>
        /// Initialize a new instance of the NetworkSpec class.
        /// </summary>
        /// <param name="name">The name of the network.</param>
        /// <param name="address">The IP address of the network.</param>
        public NetworkSpec(string name, IPAddress address)
        {
            Name = name;
            Address = address;
        }
    }
}
