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
using System.Globalization;

namespace DomainObjects
{
    /// <summary>
    /// The class that describes a Windows Media Center recording.
    /// </summary>
    public class WMCRecording
    {
        /// <summary>
        /// Get or set the collection of recordings.
        /// </summary>
        public static Collection<WMCRecording> Recordings 
        { 
            get { return (recordings); }
            set { recordings = value; }            
        }

        /// <summary>
        /// Get the title.
        /// </summary>
        public string Title { get; private set; }
        /// <summary>
        /// Get the description.
        /// </summary>
        public string Description { get; private set; }
        /// <summary>
        /// Get the start time.
        /// </summary>
        public DateTime StartTime { get; private set; }
        /// <summary>
        /// Get the season number.
        /// </summary>
        public int SeasonNumber { get; private set; }
        /// <summary>
        /// Get the episode number.
        /// </summary>
        public int EpisodeNumber { get; private set; }

        private static Collection<WMCRecording> recordings;

        /// <summary>
        /// Initialize a new instance of the WMCRecording class.
        /// </summary>
        public WMCRecording() 
        {
            SeasonNumber = -1;
            EpisodeNumber = -1;
        }

        /// <summary>
        /// Load the xml data.
        /// </summary>
        /// <param name="reader">The xml reader for the file.</param>
        public void Load(XmlReader reader)
        {
            Title = reader.GetAttribute("title");
            StartTime = DateTime.Parse(reader.GetAttribute("startTime"), CultureInfo.InvariantCulture);
            Description = reader.GetAttribute("description");

            try
            {
                SeasonNumber = Int32.Parse(reader.GetAttribute("seasonNumber"));
                EpisodeNumber = Int32.Parse(reader.GetAttribute("episodeNumber"));
            }
            catch (FormatException) { }
            catch (OverflowException) { }
            
            if (recordings == null)
                recordings = new Collection<WMCRecording>();

            recordings.Add(this);  
        }
    }
}
