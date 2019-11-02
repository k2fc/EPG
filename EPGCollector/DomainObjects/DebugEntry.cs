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
    /// The class that describes a debug parameter entry.
    /// </summary>
    public class DebugEntry
    {
        /// <summary>
        /// Get the entry name.
        /// </summary>
        public DebugName Name { get; private set; }
        
        /// <summary>
        /// Get or set the entry parameter.
        /// </summary>
        public int Parameter
        {
            get 
            {
                if (!parameterSet)
                    throw (new InvalidOperationException("DebugEntry parameter not set"));

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

        private DebugEntry() { }

        /// <summary>
        /// Initialize a new instance of the DebugEntry class with a name.
        /// </summary>
        /// <param name="name">The name of the entry.</param>
        public DebugEntry(DebugName name)
        {
            Name = name;
        }

        /// <summary>
        /// Initialize a new entry of the DebugEntry class with a name and parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parameter"></param>
        public DebugEntry(DebugName name, int parameter) : this(name)
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
        /// <returns>A new instance of the DebugEntry class with the same values as this instance.</returns>
        public DebugEntry Clone()
        {
            DebugEntry newEntry = new DebugEntry(Name);

            if (parameterSet)
                newEntry.Parameter = Parameter;

            return (newEntry);
        }

        /// <summary>
        /// Get an instance of the DebugEntry from a parameter file entry.
        /// </summary>
        /// <param name="parameter">The parameter file entry.</param>
        /// <returns>A new instance of the class.</returns>
        public static DebugEntry GetInstance(string parameter)
        {
            string[] parameterParts = parameter.Split(new char[] { '-' });

            try
            {
                DebugEntry debugEntry = new DebugEntry((DebugName)Enum.Parse(typeof(DebugName), parameterParts[0].Trim(), true));

                if (parameterParts.Length == 2)
                {
                    try
                    {
                        debugEntry.Parameter = Int32.Parse(parameterParts[1]);
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

                return (debugEntry);
            }
            catch (ArgumentException)
            {
                return (null);
            }
        }

        /// <summary>
        /// Check if a debug name is present.
        /// </summary>
        /// <param name="debugName">The name of the debug entry.</param>
        /// <returns>True if the debug name is present; false otherwise.</returns>
        public static bool IsDefined(DebugName debugName)
        {
            if (RunParameters.Instance.DebugIDs == null)
                return (false);

            foreach (DebugEntry debugEntry in RunParameters.Instance.DebugIDs)
            {
                if (debugEntry.Name == debugName)
                    return (true);
            }

            return (false);
        }

        /// <summary>
        /// Find a debug entry.
        /// </summary>
        /// <param name="debugName">The name of the debug entry.</param>
        /// <returns>The debug entry if it is found; otherwise null</returns>
        public static DebugEntry FindEntry(DebugName debugName)
        {
            return (FindEntry(debugName, false));
        }

        /// <summary>
        /// Find a debug entry.
        /// </summary>
        /// <param name="debugName">The name of the debug entry.</param>
        /// <param name="withParameter">True if a parameter must be present; false otherwise.</param>
        /// <returns>The debug entry if it is found; otherwise null</returns>
        public static DebugEntry FindEntry(DebugName debugName, bool withParameter)
        {
            if (RunParameters.Instance.DebugIDs == null)
                return (null);

            foreach (DebugEntry debugEntry in RunParameters.Instance.DebugIDs)
            {
                if (debugEntry.Name == debugName)
                {
                    if (withParameter)
                    {
                        if (debugEntry.ParameterSet)
                            return (debugEntry);
                    }
                    else
                        return (debugEntry);
                }
            }

            return (null);
        }

        /// <summary>
        /// Find a debug entry.
        /// </summary>
        /// <param name="debugEntries">The list of debug entries to search.</param>
        /// <param name="identifier">The string representation of the debug entry.</param>
        /// <returns>The debug entry if it is found; otherwise null</returns>
        public static DebugEntry FindEntry(Collection<DebugEntry> debugEntries, string identifier)
        {
            if (debugEntries == null)
                return (null);

            foreach (DebugEntry debugEntry in debugEntries)
            {
                if (debugEntry.ToString().ToUpperInvariant() == identifier.ToUpperInvariant())
                    return (debugEntry);
            }

            return (null);
        }
    }
}
