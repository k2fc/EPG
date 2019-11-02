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
    /// The class that describes the DVB pilot.
    /// </summary>
    public sealed class SignalPilot
    {
        /// <summary>
        /// The DVB-S2 pilot values.
        /// </summary>
        public enum Pilot
        {
            /// <summary>
            /// The value is not set.
            /// </summary>
            NotSet,
            /// <summary>
            /// The value is not defined.
            /// </summary>
            NotDefined,
            /// <summary>
            /// The pilot is off.
            /// </summary>
            Off,
            /// <summary>
            /// The pilot is on.
            /// </summary>
            On
        }

        private SignalPilot() { }

        /// <summary>
        /// Get a list of the pilot values.
        /// </summary>
        /// <returns>A list of the pilot values.</returns>
        public static Collection<string> GetPilots()
        {
            Collection<string> pilots = new Collection<string>();
            
            pilots.Add("Not Set");
            pilots.Add("Off");
            pilots.Add("On");

            return (pilots);
        }

        /// <summary>
        /// Get the index number of a pilot value.
        /// </summary>
        /// <param name="pilot">The pilot value.</param>
        /// <returns>The index number.</returns>
        public static int GetIndex(SignalPilot.Pilot pilot)
        {
            switch (pilot)
            {
                case Pilot.NotSet:
                    return (0);
                case Pilot.Off:
                    return (1);
                case Pilot.On:
                    return (2);
                default:
                    return (0);
            }
        }

        /// <summary>
        /// Get the descriptionr of a pilot value.
        /// </summary>
        /// <param name="pilot">The pilot value.</param>
        /// <returns>The description.</returns>
        public static string GetDescription(SignalPilot.Pilot pilot)
        {
            switch (pilot)
            {
                case Pilot.NotSet:
                    return ("Not Set");
                case Pilot.Off:
                    return ("Off");
                case Pilot.On:
                    return ("On");
                default:
                    return ("Not Set");
            }
        }
    }
}
