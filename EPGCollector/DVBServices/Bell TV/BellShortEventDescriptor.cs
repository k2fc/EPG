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

namespace DVBServices
{
    internal class BellShortEventDescriptor : DVBShortEventDescriptor
    {
        /// <summary>
        /// Get the event name.
        /// </summary>
        public override string EventName { get { return (getEventName()); } }
        /// <summary>
        /// Get the short description.
        /// </summary>
        public override string ShortDescription { get { return (getShortDescription()); } }
        /// <summary>
        /// Get the short description.
        /// </summary>
        public bool HighDefinition { get { return (base.EventName.StartsWith("HD-") || base.EventName.StartsWith("HD - ")); } }

        internal string getEventName()
        {
            string eventName = base.EventName;

            if (eventName.StartsWith("HD-"))
                return (eventName.Substring(3));
            else
            {
                if (eventName.StartsWith("HD - "))
                    return(eventName.Substring(5));
                else
                    return (eventName);
            }
        }

        internal string getShortDescription()
        {
            return (base.ShortDescription);
        }
    }
}
