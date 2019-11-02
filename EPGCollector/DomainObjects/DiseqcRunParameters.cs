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
    /// The class that describes the per frequency DiSEqC run parameters.
    /// </summary>
    public class DiseqcRunParameters
    {
        /// <summary>
        /// Get or set the switch setting.
        /// </summary>
        public string DiseqcSwitch { get; set; }
        /// <summary>
        /// Get or set the switch handler.
        /// </summary>
        public string DiseqcHandler { get; set; }
        /// <summary>
        /// Get or set the list of options.
        /// </summary>
        public Collection<OptionEntry> Options { get; set; }

        /// <summary>
        /// Get or set the SwitchAfterPlay option.
        /// </summary>
        public bool SwitchAfterPlay 
        { 
            get { return (OptionEntry.IsDefined(Options, OptionName.SwitchAfterPlay)); }
            set
            {
                if (value)
                    Options.Add(new OptionEntry(OptionName.SwitchAfterPlay));
                else
                    OptionEntry.Remove(Options, OptionName.SwitchAfterPlay);
            }

        }

        /// <summary>
        /// Get or set the DisableDriverDiseqc option.
        /// </summary>
        public bool DisableDriverDiseqc 
        { 
            get { return (OptionEntry.IsDefined(Options, OptionName.DisableDriverDiseqc)); }
            set
            {
                if (value)
                    Options.Add(new OptionEntry(OptionName.DisableDriverDiseqc));
                else
                    OptionEntry.Remove(Options, OptionName.DisableDriverDiseqc);
            }
        }
        
        /// <summary>
        /// Get or set the RepeatDiseqc option.
        /// </summary>        
        public bool RepeatDiseqc 
        { 
            get { return (OptionEntry.IsDefined(Options, OptionName.RepeatDiseqc)); }
            set
            {
                if (value)
                    Options.Add(new OptionEntry(OptionName.RepeatDiseqc));
                else
                    OptionEntry.Remove(Options, OptionName.RepeatDiseqc);
            }
        }
        
        /// <summary>
        /// Get or set the SwitchAfterTune option.
        /// </summary>        
        public bool SwitchAfterTune 
        { 
            get { return (OptionEntry.IsDefined(Options, OptionName.SwitchAfterTune)); }
            set
            {
                if (value)
                    Options.Add(new OptionEntry(OptionName.SwitchAfterTune));
                else
                    OptionEntry.Remove(Options, OptionName.SwitchAfterTune);
            }
        }
        
        /// <summary>
        /// Get or set the UseDiseqcCommand option.
        /// </summary>        
        public bool UseDiseqcCommand 
        { 
            get { return (OptionEntry.IsDefined(Options, OptionName.UseDiseqcCommand)); }
            set
            {
                if (value)
                    Options.Add(new OptionEntry(OptionName.UseDiseqcCommand));
                else
                    OptionEntry.Remove(Options, OptionName.UseDiseqcCommand);
            }
        }
        
        /// <summary>
        /// Get or set the UseDiseqcCommand option.
        /// </summary>
        public bool UseSafeDiseqc 
        { 
            get { return (OptionEntry.IsDefined(Options, OptionName.UseSafeDiseqc)); }
            set
            {
                if (value)
                    Options.Add(new OptionEntry(OptionName.UseSafeDiseqc));
                else
                    OptionEntry.Remove(Options, OptionName.UseSafeDiseqc);
            }
        }

        /// <summary>
        /// Initialize a new instance of the DiseqcRunParameters class.
        /// </summary>
        public DiseqcRunParameters() 
        {
            Options = new Collection<OptionEntry>();
        }

        /// <summary>
        /// Clone the current instance.
        /// </summary>
        /// <returns>The new instance.</returns>
        public DiseqcRunParameters Clone()
        {
            DiseqcRunParameters newParameters = new DiseqcRunParameters();
            newParameters.DiseqcSwitch = DiseqcSwitch;
            newParameters.DiseqcHandler = DiseqcHandler;

            if (Options == null)
                newParameters.Options = null;
            else
            {
                newParameters.Options = new Collection<OptionEntry>();

                foreach (OptionEntry entry in Options)
                    newParameters.Options.Add(entry.Clone());
            }

            return (newParameters);
        }

        /// <summary>
        /// Compare this instance with another.
        /// </summary>
        /// <param name="otherParameters">The other instance.</param>
        /// <returns>Truer if they are equal; false otherwise.</returns>
        public bool EqualTo(DiseqcRunParameters otherParameters)
        {
            if (DiseqcSwitch != null)
            {
                if (otherParameters.DiseqcSwitch == null)
                    return (false);
                else
                {
                    if (DiseqcSwitch != otherParameters.DiseqcSwitch)
                        return (false);
                }
            }
            else
            {
                if (otherParameters.DiseqcSwitch != null)
                    return (false);
            }

            if (DiseqcHandler != null)
            {
                if (otherParameters.DiseqcHandler == null)
                    return (false);
                else
                {
                    if (DiseqcHandler.ToLowerInvariant() != otherParameters.DiseqcHandler.ToLowerInvariant())
                        return (false);
                }
            }
            else
            {
                if (otherParameters.DiseqcHandler != null)
                    return (false);
            }

            if (Options != null)
            {
                if (otherParameters.Options == null || Options.Count != otherParameters.Options.Count)
                    return (false);

                foreach (OptionEntry optionEntry in Options)
                {
                    if (OptionEntry.FindEntry(otherParameters.Options, optionEntry.ToString()) == null)
                        return (false);
                }
            }
            else
            {
                if (otherParameters.Options != null)
                    return (false);
            }

            return (true);
        }
    }
}
