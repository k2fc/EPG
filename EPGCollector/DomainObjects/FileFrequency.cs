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

namespace DomainObjects
{
    /// <summary>
    /// The class that describes a file used for a collection.
    /// </summary>
    public class FileFrequency : TuningFrequency
    {
        /// <summary>
        /// Get or set the path.
        /// </summary>
        public string Path
        {
            get { return (path); }
            set { path = value; }
        }

        /// <summary>
        /// Get the tuner type for this type of frequency.
        /// </summary>
        public override TunerType TunerType { get { return (TunerType.File); } }

        private string path;

        /// <summary>
        /// Initialize a new instance of the FileFrequency class.
        /// </summary>
        public FileFrequency() { }

        /// <summary>
        /// Check if this instance is equal to another.
        /// </summary>
        /// <param name="frequency">The other instance.</param>
        /// <param name="level">The level of equality to be checked.</param>
        /// <returns></returns>
        public override bool EqualTo(TuningFrequency frequency, EqualityLevel level)
        {
            bool reply = base.EqualTo(frequency, level);
            if (!reply)
                return (false);

            FileFrequency fileFrequency = frequency as FileFrequency;
            if (fileFrequency == null)
                return (false);

            if (Path != fileFrequency.Path)
                return (false);

            return (true);
        }

        /// <summary>
        /// Get a description of this instance.
        /// </summary>
        /// <returns>A string describing this instance.</returns>
        public override string ToString()
        {
            return (path);
        }

        /// <summary>
        /// Generate a copy of this frequency.
        /// </summary>
        /// <returns>A new instance with the same properties as the old instance.</returns>
        public override TuningFrequency Clone()
        {
            FileFrequency newFrequency = new FileFrequency();
            base.Clone(newFrequency);

            newFrequency.Path = path;
            
            return (newFrequency);
        }
    }
}
