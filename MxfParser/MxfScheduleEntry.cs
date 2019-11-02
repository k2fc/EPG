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
    /// The class that describes an MXF schedule entry.
    /// </summary>
    public class MxfScheduleEntry
    {
        /// <summary>
        /// Get or set the collection of schedule entries in an MXF file.
        /// </summary>
        public static Collection<MxfScheduleEntry> ScheduleEntries { get; set; }

        /// <summary>
        /// Get the service the schedule entry belongs to.
        /// </summary>
        public string Service { get; private set; }

        /// <summary>
        /// Get the program of the schedule entry.
        /// </summary>
        public string Program { get; private set; }

        /// <summary>
        /// Get the start time of the schedule entry.
        /// </summary>
        public string StartTime { get; private set; }

        /// <summary>
        /// Get the duration of the schedule entry.
        /// </summary>
        public string Duration { get; private set; }

        private static string lastStartTime;
        private static string lastDuration;

        private MxfScheduleEntry(string service) 
        {
            Service = service;
        }

        private bool load(XmlReader xmlReader)
        {
            try
            {
                Program = xmlReader.GetAttribute("program");

                StartTime = xmlReader.GetAttribute("startTime");
                Duration = xmlReader.GetAttribute("duration");

                if (StartTime == null)                    
                {
                    TimeSpan? duration = getDuration(lastDuration);
                    if (duration != null)
                    {
                        DateTime startTime = DateTime.Parse(lastStartTime) + duration.Value;
                        StartTime = startTime.Date.ToString("yyyy-MM-dd") + "T" +
                            startTime.Hour.ToString("00") + ":" +
                            startTime.Minute.ToString("00") + ":" +
                            startTime.Second.ToString("00");                        
                    }
                }

                lastStartTime = StartTime;
                lastDuration = Duration;
            }
            catch (XmlException e)
            {
                Logger.Instance.Write("Failed to load mxf schedule entry");
                Logger.Instance.Write("Data exception: " + e.Message);
                return (false);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("Failed to load mxf schedule entry");
                Logger.Instance.Write("I/O exception: " + e.Message);
                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Get a new instance of the MxfScheduleEntry class with data loaded.
        /// </summary>
        /// <param name="xmlReader">An XmlReader instance for the channel tag.</param>
        /// <returns>An MxfScheduleEntry instance with data loaded.</returns>
        public static MxfScheduleEntry GetInstance(XmlReader xmlReader, string service)
        {
            MxfScheduleEntry instance = new MxfScheduleEntry(service);
            instance.load(xmlReader);

            return (instance);
        }

        internal TimeSpan? GetDuration()
        {
            return (getDuration(Duration));
        }

        private TimeSpan? getDuration(string seconds)
        {
            try
            {
                int duration = Int32.Parse(seconds);
                return (TimeSpan.FromSeconds(duration));
            }
            catch (FormatException)
            {
                return (null);
            }
            catch (OverflowException)
            {
                return (null);
            }
            catch (ArgumentOutOfRangeException)
            {
                return (null);
            }
        }
    }
}
