///////////////////////////////////////////////////////////////////////////////// 
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

namespace XmltvParser
{
    /// <summary>
    /// The class that describes an XMLTV time tag.
    /// </summary>
    public class XmltvTime
    {
        /// <summary>
        /// Get the time.
        /// </summary>
        public DateTime? Time { get; private set; }
        /// <summary>
        /// Get the offset.
        /// </summary>
        public TimeSpan? Offset { get; private set; }

        private XmltvTime() { }

        private void load(string timeData)
        {
            string[] timeParts = timeData.Split(new char[] { ' ' });

            try
            {
                Time = new DateTime(Int32.Parse(timeParts[0].Substring(0, 4)),
                    Int32.Parse(timeParts[0].Substring(4, 2)),
                    Int32.Parse(timeParts[0].Substring(6, 2)),
                    Int32.Parse(timeParts[0].Substring(8, 2)),
                    Int32.Parse(timeParts[0].Substring(10, 2)),
                    Int32.Parse(timeParts[0].Substring(12, 2)));

                if (timeParts.Length == 1)
                    Offset = new TimeSpan(0);
                else
                {
                    Offset = new TimeSpan(Int32.Parse(timeParts[1].Substring(1, 2)), Int32.Parse(timeParts[1].Substring(3, 2)), 0);
                    if (timeParts[1][0] == '-')
                    {
                        Offset = new TimeSpan(0) - Offset;
                    }
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Write("<E> An exception of type " + e.GetType().Name + " has occurred while parsing a date or time field");
                Logger.Instance.Write("<E> " + e.Message);
                return;
            }
        }

        /// <summary>
        /// Get a loaded instance of the class.
        /// </summary>
        /// <param name="timeData">An XMLTV date/time string.</param>
        /// <returns>An instance of the class with the tag data loaded.</returns>
        public static XmltvTime GetInstance(string timeData)
        {
            XmltvTime instance = new XmltvTime();

            if (timeData != null)
                instance.load(timeData);

            return (instance);
        }
    }
}
