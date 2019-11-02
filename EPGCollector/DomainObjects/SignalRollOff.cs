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
    /// The class that describes the DVB-S2 roll off.
    /// </summary>
    public sealed class SignalRollOff
    {
        /// <summary>
        /// The DVB-S2 roll-off values.
        /// </summary>
        public enum RollOff
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
            /// Roll off factor is 20%
            /// </summary>
            RollOff20,
            /// <summary>
            /// Roll off factor is 25%
            /// </summary>
            RollOff25,
            /// <summary>
            /// Roll off factor is 35%
            /// </summary>
            RollOff35
        }

        private SignalRollOff() { }

        /// <summary>
        /// Convert the DVB roll off to the internal roll off.
        /// </summary>
        /// <param name="rollOff">The roll off to be converted.</param>
        /// <returns>The converted value.</returns>
        public static RollOff ConvertDVBRollOff(int rollOff)
        {
            switch (rollOff)
            {
                case 0:
                    return (RollOff.RollOff35);
                case 1:
                    return (RollOff.RollOff25);
                case 2:
                    return (RollOff.RollOff20);
                default:
                    return (RollOff.RollOff35);
            }
        }

        /// <summary>
        /// Get a list of rolloff values.
        /// </summary>
        /// <returns>A list of rolloff values.</returns>
        public static Collection<string> GetRollOffs()
        {
            Collection<string> rolloffs = new Collection<string>();

            rolloffs.Add("Not Set");
            rolloffs.Add("0.20");
            rolloffs.Add("0.25");
            rolloffs.Add("0.35");

            return (rolloffs);
        }

        /// <summary>
        /// Get the index number of a rolloff value.
        /// </summary>
        /// <param name="rollOff">The rolloff value.</param>
        /// <returns>The index number.</returns>
        public static int GetIndex(SignalRollOff.RollOff rollOff)
        {
            switch (rollOff)
            {
                case RollOff.NotSet:
                    return (0);
                case RollOff.RollOff20:
                    return (1);
                case RollOff.RollOff25:
                    return (2);
                case RollOff.RollOff35:
                    return (3);
                default:
                    return (0);
            }
        }

        /// <summary>
        /// Get the rolloff from an index.
        /// </summary>
        /// <param name="index">The index value.</param>
        /// <returns>The rollOff.</returns>
        public static RollOff GetRollOff(int index)
        {
            switch (index)
            {
                case 0:
                    return (RollOff.NotSet);
                case 1:
                    return (RollOff.RollOff20);
                case 2:
                    return (RollOff.RollOff25);
                case 3:
                    return (RollOff.RollOff35);
                default:
                    return (RollOff.NotSet);
            }
        }

        /// <summary>
        /// Get the xml value for the rolloff.
        /// </summary>
        /// <returns>The rollOff.</returns>
        public static string GetXml(SignalRollOff.RollOff rollOff)
        {
            switch (rollOff)
            {
                case RollOff.NotSet:
                    return ("NotSet");
                case RollOff.NotDefined:
                    return ("NotDefined");
                case RollOff.RollOff20:
                    return ("Twenty");
                case RollOff.RollOff25:
                    return ("TwentyFive");
                case RollOff.RollOff35:
                    return ("ThirtyFive");
                default:
                    return ("NotSet");
            }
        }

        /// <summary>
        /// Get the description for the rolloff.
        /// </summary>
        /// <returns>The rollOff.</returns>
        public static string GetDescription(SignalRollOff.RollOff rollOff)
        {
            switch (rollOff)
            {
                case RollOff.NotSet:
                    return ("Not Set");
                case RollOff.NotDefined:
                    return ("Not Defined");
                case RollOff.RollOff20:
                    return ("0.20");
                case RollOff.RollOff25:
                    return ("0.25");
                case RollOff.RollOff35:
                    return ("0.35");
                default:
                    return ("Not Set");
            }
        }
    }
}
