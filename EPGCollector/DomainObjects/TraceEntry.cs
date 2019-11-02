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
    /// The class that describes a trace parameter entry.
    /// </summary>
    public class TraceEntry
    {
        /// <summary>
        /// Get the last error message.
        /// </summary>
        public static string LastError { get { return (lastError); } }

        /// <summary>
        /// Get the entry name.
        /// </summary>
        public TraceName Name { get; private set; }
        
        /// <summary>
        /// Get or set the number entry parameter.
        /// </summary>
        public int NumberParameter
        {
            get 
            {
                if (!numberParameterSet)
                    throw (new InvalidOperationException("TraceEntry number parameter not set"));

                return (numberParameter); 
            }
            set
            {
                numberParameter = value;
                numberParameterSet = true;
            }
        }

        /// <summary>
        /// Get or set the string entry parameter.
        /// </summary>
        public string StringParameter
        {
            get
            {
                if (!stringParameterSet)
                    throw (new InvalidOperationException("TraceEntry string parameter not set"));

                return (stringParameter);
            }
            set
            {
                stringParameter = value;
                stringParameterSet = true;
            }
        }

        /// <summary>
        /// Return true if the entry number parameter has been set; false otherwise.
        /// </summary>
        public bool NumberParameterSet { get { return (numberParameterSet); } }
        /// <summary>
        /// Return true if the entry string parameter has been set; false otherwise.
        /// </summary>
        public bool StringParameterSet { get { return (stringParameterSet); } }

        private int numberParameter;
        private bool numberParameterSet;
        private string stringParameter;
        private bool stringParameterSet;

        private static string lastError;

        private TraceEntry() { }

        /// <summary>
        /// Initialize a new instance of the TraceEntry class with a name.
        /// </summary>
        /// <param name="name">The name of the entry.</param>
        public TraceEntry(TraceName name)
        {
            Name = name;
        }

        /// <summary>
        /// Initialize a new entry of the TraceEntry class with a name and number parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parameter"></param>
        public TraceEntry(TraceName name, int parameter) : this(name)
        {
            NumberParameter = parameter;
        }

        /// <summary>
        /// Initialize a new entry of the TraceEntry class with a name and string parameter.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="parameter"></param>
        public TraceEntry(TraceName name, string parameter) : this(name)
        {
            StringParameter = parameter;
        }

        /// <summary>
        /// Get a string representation of the instance.
        /// </summary>
        /// <returns>A string representing the instance.</returns>
        public override string ToString()
        {
            if (numberParameterSet)
                return (Name.ToString() + "-" + NumberParameter);
            else
            {
                if (stringParameterSet)
                    return (Name.ToString() + "-" + '"' + StringParameter + '"');
                else
                    return (Name.ToString());
            }
        }

        /// <summary>
        /// Copy the instance.
        /// </summary>
        /// <returns>A new instance of the TraceEntry class with the same values as this instance.</returns>
        public TraceEntry Clone()
        {
            TraceEntry newEntry = new TraceEntry(Name);

            if (numberParameterSet)
                newEntry.NumberParameter = NumberParameter;
            if (stringParameterSet)
                newEntry.StringParameter = StringParameter;

            return (newEntry);
        }

        /// <summary>
        /// Get an instance of the TraceEntry from a parameter file entry.
        /// </summary>
        /// <param name="parameter">The parameter file entry.</param>
        /// <returns>A new instance of the class.</returns>
        public static TraceEntry GetInstance(string parameter)
        {
            string[] parameterParts = parameter.Split(new char[] { '-' });

            if (parameterParts.Length == 2 && string.IsNullOrWhiteSpace(parameterParts[1]))
                return(null);

            try
            {
                TraceEntry traceEntry = new TraceEntry((TraceName)Enum.Parse(typeof(TraceName), parameterParts[0].Trim(), true));

                if (parameterParts.Length == 2)
                {
                    if (parameterParts[1].Trim()[0] != '"')
                    {
                        try
                        {
                            traceEntry.NumberParameter = Int32.Parse(parameterParts[1]);
                        }
                        catch (FormatException)
                        {
                            lastError = "The Trace name '" + parameterParts[0].Trim() + "' has a parameter in the wrong format.";
                            return (null);
                        }
                        catch (OverflowException)
                        {
                            lastError = "The Trace name '" + parameterParts[0].Trim() + "' has a parameter out of range.";
                            return (null);
                        }
                    }
                    else
                    {
                        if (parameterParts[1].Trim().Length < 3 || parameterParts[1].Trim()[parameterParts[1].Length - 1] != '"')
                            return (null);

                        traceEntry.StringParameter = parameterParts[1].Trim().Substring(1, parameterParts[1].Length - 2);
                    }
                }

                return (traceEntry);
            }
            catch (ArgumentException)
            {
                lastError = "The Trace ID '" + parameter.Trim() + "' is undefined and will be ignored.";
                return (null);
            }
        }

        /// <summary>
        /// Check if a trace name is present.
        /// </summary>
        /// <param name="traceName">The name of the trace entry.</param>
        /// <returns>True if the trace name is present; false otherwise.</returns>
        public static bool IsDefined(TraceName traceName)
        {
            if (RunParameters.Instance.TraceIDs == null)
                return (false);

            foreach (TraceEntry traceEntry in RunParameters.Instance.TraceIDs)
            {
                if (traceEntry.Name == traceName)
                    return (true);
            }

            return (false);
        }

        /// <summary>
        /// Find a trace entry.
        /// </summary>
        /// <param name="traceName">The name of the trace entry.</param>
        /// <returns>The trace entry if it is found; otherwise null</returns>
        public static TraceEntry FindEntry(TraceName traceName)
        {
            return (FindEntry(traceName, false));
        }

        /// <summary>
        /// Find a trace entry.
        /// </summary>
        /// <param name="traceName">The name of the trace entry.</param>
        /// <param name="withParameter">True if a parameter must be present; false otherwise.</param>
        /// <returns>The trace entry if it is found; otherwise null</returns>
        public static TraceEntry FindEntry(TraceName traceName, bool withParameter)
        {
            if (RunParameters.Instance.TraceIDs == null)
                return (null);

            foreach (TraceEntry traceEntry in RunParameters.Instance.TraceIDs)
            {
                if (traceEntry.Name == traceName)
                {
                    if (withParameter)
                    {
                        if (traceEntry.NumberParameterSet || traceEntry.StringParameterSet)
                            return (traceEntry);
                    }
                    else
                        return (traceEntry);
                }
            }

            return (null);
        }

        /// <summary>
        /// Find a trace entry.
        /// </summary>
        /// <param name="traceEntries">The list of trace entries to search.</param>
        /// <param name="identifier">The string representation of the debug entry.</param>
        /// <returns>The trace entry if it is found; otherwise null</returns>
        public static TraceEntry FindEntry(Collection<TraceEntry> traceEntries, string identifier)
        {
            if (traceEntries == null)
                return (null);

            foreach (TraceEntry traceEntry in traceEntries)
            {
                if (traceEntry.ToString().ToUpperInvariant() == identifier.ToUpperInvariant())
                    return (traceEntry);
            }

            return (null);
        }
    }
}
