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

namespace DomainObjects
{
    /// <summary>
    /// The class that describes a selected tuner.
    /// </summary>
    public class SelectedTuner
    {
        /// <summary>
        /// Get the selected tuner number.
        /// </summary>
        public int TunerNumber 
        { 
            get
            {
                if (tunerNumber != 0)
                    return (tunerNumber);

                if (UniqueIdentity == null)
                    throw (new ArgumentNullException("The unique identity of a selected tuner is null when the tuner number is zero"));

                foreach (Tuner tuner in Tuner.TunerCollection)
                {
                    if (tuner.IsServerTuner)
                    {
                        if (tuner.UniqueIdentity == UniqueIdentity)
                        {
                            tunerNumber = Tuner.TunerCollection.IndexOf(tuner) + 1;
                            return(tunerNumber);
                        }
                    }
                }

                return (0);
            }
                
            private set { tunerNumber = value; } 
        }
        
        /// <summary>
        /// Get the unique identity.
        /// </summary>
        public string UniqueIdentity { get; private set; }

        private int tunerNumber;

        private SelectedTuner() { }

        /// <summary>
        /// Initialize a new instance of the SelectedTuner class with a tuner number.
        /// </summary>
        /// <param name="tunerNumber">The tuner number.</param>
        public SelectedTuner(int tunerNumber)
        {
            TunerNumber = tunerNumber;
        }

        /// <summary>
        /// Initialize a new instance of the SelectedTuner class with a tuner identity.
        /// </summary>
        /// <param name="uniqueIdentity">The unique identity.</param>
        public SelectedTuner(string uniqueIdentity)
        {
            UniqueIdentity = uniqueIdentity;
        }

        /// <summary>
        /// Initialize a new instance of the SelectedTuner class with a tuner number and unique identity.
        /// </summary>
        /// <param name="tunerNumber">The tuner number.</param>
        /// <param name="uniqueIdentity">The unique identity.</param>
        public SelectedTuner(int tunerNumber, string uniqueIdentity)
        {
            TunerNumber = tunerNumber;
            UniqueIdentity = uniqueIdentity;
        }

        /// <summary>
        /// Check if a tuner has been selected.
        /// </summary>
        /// <param name="tuners">The list of selected tuners.</param>
        /// <param name="tunerNumber">The tuner number to check.</param>
        /// <returns>True if the tuner has been selected; false otherwise.</returns>
        public static bool Selected(Collection<SelectedTuner> tuners, int tunerNumber)
        {
            if (tuners == null || tuners.Count == 0)
                return(false);

            foreach (SelectedTuner selectedTuner in tuners)
            {
                if (selectedTuner.TunerNumber == tunerNumber)
                    return(true);
            }

            return(false);
        }

        /// <summary>
        /// Clone the current instance.
        /// </summary>
        /// <returns>A copy of the current instance.</returns>
        public SelectedTuner Clone()
        {
            SelectedTuner newTuner = new SelectedTuner();
            newTuner.TunerNumber = tunerNumber;
            newTuner.UniqueIdentity = UniqueIdentity;

            return (newTuner);
        }

        /// <summary>
        /// Test if this instance is equal to another instance.
        /// </summary>
        /// <param name="otherTuner">The other instance.</param>
        /// <returns>True if the instances are equal; false otherwise.</returns>
        public bool EqualTo(SelectedTuner otherTuner)
        {
            if (TunerNumber != otherTuner.TunerNumber)
                return (false);

            if (UniqueIdentity != null)
            {
                if (otherTuner.UniqueIdentity == null)
                    return (false);
                else
                {
                    if (UniqueIdentity != otherTuner.UniqueIdentity)
                        return (false);
                }
            }
            else
            {
                if (otherTuner.UniqueIdentity != null)
                    return (false);
            }

            return (true);
        }
    }
}
