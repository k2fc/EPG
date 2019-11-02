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
    /// <summary>
    /// The class that describes a content identifier entry.
    /// </summary>
    public class ContentIdentifier
    {
        /// <summary>
        /// Get the content type.
        /// </summary>
        public int Type { get; private set; }
        /// <summary>
        /// Get the content location.
        /// </summary>
        public int Location { get; private set; }
        /// <summary>
        /// Get the content identifier.
        /// </summary>
        public string Identifier { get; private set; }

        /// <summary>
        /// Return true if the link is an episode link; false otherwise.
        /// </summary>
        public bool IsEpisodeLink { get { return (Type == 1 || Type == 0x31); } }
        /// <summary>
        /// Return true if the link is a series link; false otherwise.
        /// </summary>
        public bool IsSeriesLink { get { return (Type == 2 || Type == 0x32); } }

        private ContentIdentifier() { }
        
        /// <summary>
        /// Initialize a new instance of the ContentIdentifier class.
        /// </summary>
        /// <param name="type">The type of the identifier.</param>
        /// <param name="location">The location of the identifier.</param>
        /// <param name="identifier">The identifier.</param>
        public ContentIdentifier(int type, int location, string identifier) 
        {
            Type = type;
            Location = location;
            Identifier = identifier;
        }
    }
}
