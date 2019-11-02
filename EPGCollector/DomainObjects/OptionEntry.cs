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
    /// The class that describes an option parameter entry.
    /// </summary>
    public class OptionEntry
    {
        /// <summary>
        /// Get the entry name.
        /// </summary>
        public OptionName Name { get; private set; }
        
        /// <summary>
        /// Get or set the entry parameter.
        /// </summary>
        public int Parameter
        {
            get 
            {
                if (!parameterSet)
                    throw (new InvalidOperationException("OptionEntry parameter not set"));

                return (parameter); 
            }
            set
            {
                parameter = value;
                parameterSet = true;
            }
        }

        /// <summary>
        /// Return true if the entry parameter has been set; false otherwise.
        /// </summary>
        public bool ParameterSet { get { return (parameterSet); } }

        private int parameter;
        private bool parameterSet;

        private OptionEntry() { }

        /// <summary>
        /// Initialize a new instance of the OptionEntry class with a name.
        /// </summary>
        /// <param name="name">The name of the entry.</param>
        public OptionEntry(OptionName name)
        {
            Name = name;
        }

        /// <summary>
        /// Initialize a new entry of the OptionEntry class with a name and parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parameter"></param>
        public OptionEntry(OptionName name, int parameter) : this(name)
        {
            Parameter = parameter;
        }

        /// <summary>
        /// Get a string representation of the instance.
        /// </summary>
        /// <returns>A string representing the instance.</returns>
        public override string ToString()
        {
            return (parameterSet ? Name.ToString() + "-" + parameter : Name.ToString()); 
        }

        /// <summary>
        /// Copy the instance.
        /// </summary>
        /// <returns>A new instance of the OptionEntry class with the same values as this instance.</returns>
        public OptionEntry Clone()
        {
            OptionEntry newEntry = new OptionEntry(Name);

            if (parameterSet)
                newEntry.Parameter = Parameter;

            return (newEntry);
        }

        /// <summary>
        /// Get an instance of the OptionEntry from a parameter file entry.
        /// </summary>
        /// <param name="parameter">The parameter file entry.</param>
        /// <returns>A new instance of the class.</returns>
        public static OptionEntry GetInstance(string parameter)
        {
            string[] parameterParts = parameter.Split(new char[] { '-' });

            try
            {
                OptionEntry optionEntry = new OptionEntry((OptionName)Enum.Parse(typeof(OptionName), parameterParts[0].Trim(), true));

                if (parameterParts.Length == 2)
                {
                    try
                    {
                        optionEntry.Parameter = Int32.Parse(parameterParts[1]);
                    }
                    catch (FormatException)
                    {
                        return (null);
                    }
                    catch (OverflowException)
                    {
                        return (null);
                    }

                }

                return (optionEntry);
            }
            catch (ArgumentException)
            {
                return (null);
            }
        }

        /// <summary>
        /// Check if a option name is present.
        /// </summary>
        /// <param name="optionName">The name of the option entry.</param>
        /// <returns>True if the option name is present; false otherwise.</returns>
        public static bool IsDefined(OptionName optionName)
        {
            return (IsDefined(RunParameters.Instance.Options, optionName));
        }

        /// <summary>
        /// Check if a option name is present.
        /// </summary>
        /// <param name="options">The list of options to search.</param>
        /// <param name="optionName">The name of the option entry.</param>
        /// <returns>True if the option name is present; false otherwise.</returns>
        public static bool IsDefined(Collection<OptionEntry> options, OptionName optionName)
        {
            if (options == null)
                return (false);

            foreach (OptionEntry optionEntry in options)
            {
                if (optionEntry.Name == optionName)
                    return (true);
            }

            return (false);
        }

        /// <summary>
        /// Find an option entry.
        /// </summary>
        /// <param name="optionName">The name of the option entry.</param>
        /// <returns>The option entry if it is found; otherwise null</returns>
        public static OptionEntry FindEntry(OptionName optionName)
        {
            return (FindEntry(optionName, false));
        }

        /// <summary>
        /// Find an option entry.
        /// </summary>
        /// <param name="optionName">The name of the option entry.</param>
        /// <param name="withParameter">True if a parameter must be present; false otherwise.</param>
        /// <returns>The option entry if it is found; otherwise null</returns>
        public static OptionEntry FindEntry(OptionName optionName, bool withParameter)
        {
            if (RunParameters.Instance.Options == null)
                return (null);

            foreach (OptionEntry optionEntry in RunParameters.Instance.Options)
            {
                if (optionEntry.Name == optionName)
                {
                    if (withParameter)
                    {
                        if (optionEntry.ParameterSet)
                            return (optionEntry);
                    }
                    else
                        return (optionEntry);
                }
            }

            return (null);
        }

        /// <summary>
        /// Find an option entry.
        /// </summary>
        /// <param name="optionEntries">The list of option entries to search.</param>
        /// <param name="identifier">The string representation of the option entry.</param>
        /// <returns>The option entry if it is found; otherwise null</returns>
        public static OptionEntry FindEntry(Collection<OptionEntry> optionEntries, string identifier)
        {
            if (optionEntries == null)
                return (null);

            foreach (OptionEntry optionEntry in optionEntries)
            {
                if (optionEntry.ToString().ToUpperInvariant() == identifier.ToUpperInvariant())
                    return (optionEntry);
            }

            return (null);
        }

        /// <summary>
        /// Find an option entry.
        /// </summary>
        /// <param name="optionEntries">The list of option entries to search.</param>
        /// <param name="optionName">The name of the option entry.</param>
        /// <param name="withParameter">True if a parameter must be present; false otherwise.</param>
        /// <returns>The option entry if it is found; otherwise null</returns>
        public static OptionEntry FindEntry(Collection<OptionEntry> optionEntries, OptionName optionName, bool withParameter)
        {
            if (optionEntries == null)
                return (null);

            foreach (OptionEntry optionEntry in optionEntries)
            {
                if (optionEntry.Name == optionName)
                {
                    if (withParameter)
                    {
                        if (optionEntry.ParameterSet)
                            return (optionEntry);
                    }
                    else
                        return (optionEntry);
                }
            }

            return (null);
        }

        /// <summary>
        /// Remove an entry.
        /// </summary>
        /// <param name="optionEntries">The list of entries.</param>
        /// <param name="optionName">The name of the entry to remove.</param>
        public static void Remove(Collection<OptionEntry> optionEntries, OptionName optionName)
        {
            foreach (OptionEntry optionEntry in optionEntries)
            {
                if (optionEntry.Name == optionName)
                {
                    optionEntries.Remove(optionEntry);
                    return;
                }
            }
        }
    }
}
