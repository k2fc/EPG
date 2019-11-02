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
using System.Globalization;

namespace DomainObjects
{
    /// <summary>
    /// The class that describes XMLTV user channel changes.
    /// </summary>
    public class ImportChannelChange
    {
        /// <summary>
        /// Get the display name.
        /// </summary>
        public string DisplayName { get; private set; }
        /// <summary>
        /// Get or set the new name.
        /// </summary>
        public string NewName { get; set; }
        /// <summary>
        /// Get or set the channel number.
        /// </summary>
        public int ChannelNumber { get; set; }
        /// <summary>
        /// Get or set the excluded flag.
        /// </summary>
        public bool Excluded { get; set; }

        /// <summary>
        /// Get or set the displayed channel number. 
        /// </summary>
        public string DisplayedChannelNumber
        {
            get
            {
                if (ChannelNumber != -1)
                    return (ChannelNumber.ToString(CultureInfo.InvariantCulture));
                else
                    return (string.Empty);
            }
            set
            {
                if (String.IsNullOrWhiteSpace(value))
                    ChannelNumber = -1;
                else
                    ChannelNumber = Int32.Parse(value, CultureInfo.InvariantCulture);
            }
        }

        private ImportChannelChange() { }

        /// <summary>
        /// Initialize a new instance of the XmltvChannelChange class.
        /// </summary>
        /// <param name="displayName">The channels display name.</param>
        public ImportChannelChange(string displayName)
        {
            DisplayName = displayName;
            ChannelNumber = -1;
        }

        /// <summary>
        /// Compare this instance with another for sorting purposes.
        /// </summary>
        /// <param name="channelChange">The other instance.</param>
        /// <param name="keyName">The name of the key to compare on.</param>
        /// <returns>Zero if the instances are equal, Greater than 0 if this instance is greater; less than zero otherwise.</returns>
        public int CompareForSorting(ImportChannelChange channelChange, string keyName)
        {
            if (channelChange == null)
                throw (new ArgumentException("The channel change cannot be null", "channelChange"));
            if (keyName == null)
                throw (new ArgumentException("The key name cannot be null", "keyName"));

            switch (keyName)
            {
                case "Name":
                    return (DisplayName.CompareTo(channelChange.DisplayName));
                case "Excluded":
                    return (Excluded.CompareTo(channelChange.Excluded));
                case "NewName":
                    string newNameString;
                    string otherNewNameString;

                    if (NewName == null)
                        newNameString = string.Empty;
                    else
                        newNameString = NewName;

                    if (channelChange.NewName == null)
                        otherNewNameString = string.Empty;
                    else
                        otherNewNameString = channelChange.NewName;

                    return (newNameString.CompareTo(otherNewNameString));
                case "ChannelNumber":
                    return (ChannelNumber.CompareTo(channelChange.ChannelNumber));
                default:
                    return (0);
            }
        }
    }
}
