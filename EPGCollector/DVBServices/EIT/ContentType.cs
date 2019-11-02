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
    /// The class that describes a content type entry.
    /// </summary>
    public class ContentType
    {
        /// <summary>
        /// Get the content type.
        /// </summary>
        public int Type { get; private set; }
        /// <summary>
        /// Get the content subtype.
        /// </summary>
        public int SubType { get; private set; }
        /// <summary>
        /// Get the content user type.
        /// </summary>
        public int UserType { get; private set; }

        private ContentType() { }
        
        /// <summary>
        /// Initialize a new instance of the ContentType class.
        /// </summary>
        /// <param name="type">The content type</param>
        /// <param name="subType">The content subtype.</param>
        /// <param name="userType">The user type.</param>
        public ContentType(int type, int subType, int userType) 
        {
            Type = type;
            SubType = subType;
            UserType = userType;
        }
    }
}
