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

using DomainObjects;

namespace DVBServices
{
    /// <summary>
    /// Dish Network Short Event descriptor class.
    /// </summary>
    internal class DishNetworkShortEventDescriptor : DishNetworkTextDescriptor
    {
        /// <summary>
        /// Get the event name.
        /// </summary>
        public string EventName { get { return (eventName); } }

        /// <summary>
        /// Get the index of the next byte in the section following this descriptor.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The descriptor has not been processed.
        /// </exception>        
        public override int Index
        {
            get
            {
                if (lastIndex == -1)
                    throw (new InvalidOperationException("DishNetworkShortEventDescriptor: Index requested before block processed"));
                return (lastIndex);
            }
        }

        private string eventName;

        private int lastIndex = -1;

        /// <summary>
        /// Initialize a new instance of the DishNetworkShortEventDescriptor class.
        /// </summary>
        internal DishNetworkShortEventDescriptor() { }

        /// <summary>
        /// Parse the descriptor.
        /// </summary>
        /// <param name="byteData">The MPEG2 section containing the descriptor.</param>
        /// <param name="index">Index of the byte in the MPEG2 section following the descriptor length.</param>
        internal override void Process(byte[] byteData, int index)
        {
            try
            {
                eventName = Decompress(byteData, index);
            }
            catch (InvalidOperationException e)
            {
                throw (new ArgumentOutOfRangeException(e.Message));
            }

            lastIndex = index + Length;

            Validate();            
        }

        /// <summary>
        /// Validate the descriptor fields.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// A descriptor field is not valid.
        /// </exception>
        internal override void Validate() { }

        /// <summary>
        /// Log the descriptor fields.
        /// </summary>
        internal override void LogMessage()
        {
            if (Logger.ProtocolLogger == null)
                return;

            Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "DISH SHORT EVENT DESCRIPTOR:" +
                " Name: " + (eventName == null ? "Not present" : eventName));
        }
    }
}
