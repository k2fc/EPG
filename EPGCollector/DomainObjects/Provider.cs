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

namespace DomainObjects
{
    /// <summary>
    /// The class that describes a provider.
    /// </summary>
    public class Provider
    {
        /// <summary>
        /// Get or set the name of the provider.
        /// </summary>
        public string Name 
        { 
            get { return(name); }
            set { name = value; }
        }

        /// <summary>
        /// Get or set the country code for the provider.
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Get or set the list of options for the provider.
        /// </summary>
        public Collection<OptionEntry> Options { get; set; }

        /// <summary>
        /// Get the collection of frequencies from the provider.
        /// </summary>
        public Collection<TuningFrequency> Frequencies
        {
            get
            {
                if (frequencies == null)
                    frequencies = new Collection<TuningFrequency>();
                return (frequencies);
            }
        }

        private string name;
        private Collection<TuningFrequency> frequencies;

        /// <summary>
        /// Initialize a new instance of the Provider class.
        /// </summary>
        public Provider() { }

        /// <summary>
        /// Initialize a new instance of the Provider class. 
        /// </summary>
        /// <param name="name">The name of the provider.</param>
        public Provider(string name)
        {
            this.name = name;
        }

        /// <summary>
        /// Load the base properties.
        /// </summary>
        /// <param name="reader">The XmlReader to use.</param>
        protected void loadBase(XmlReader reader)
        {
            switch (reader.Name)
            {
                case "CountryCode":
                    CountryCode = reader.ReadString();
                    break;
                case "Option":
                    try
                    {
                        OptionName optionName = (OptionName)Enum.Parse(typeof(OptionName), reader.ReadString().Trim(), true);
                        if (Options == null)
                            Options = new Collection<OptionEntry>();
                        Options.Add(new OptionEntry(optionName));
                    }
                    catch (ArgumentException) { }
                    catch (OverflowException) { }
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Unload the base properties.
        /// </summary>
        /// <param name="writer">The XmlWriter to use.</param>
        protected void unloadBase(XmlWriter writer)
        {
            if (CountryCode != null)
                writer.WriteElementString("CountryCode", CountryCode);

            if (Options != null && Options.Count != 0)
            {
                writer.WriteStartElement("Options");
                foreach (OptionEntry optionEntry in Options)
                    writer.WriteElementString("Option", optionEntry.Name.ToString());
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Add a new frequency.
        /// </summary>
        /// <param name="newFrequency">The frequency to be added.</param>
        public void AddFrequency(TuningFrequency newFrequency)
        {
            foreach (TuningFrequency oldFrequency in Frequencies)
            {
                if (oldFrequency.Frequency == newFrequency.Frequency)
                    return;

                if (oldFrequency.Frequency > newFrequency.Frequency)
                {
                    Frequencies.Insert(Frequencies.IndexOf(oldFrequency), newFrequency);
                    return;
                }
            }

            Frequencies.Add(newFrequency);
        }

        /// <summary>
        /// Find a tuning frequency.
        /// </summary>
        /// <param name="frequency">The frequency to be searched for.</param>
        /// <returns>The tuning frequency or null if it cannot be located.</returns>
        public virtual TuningFrequency FindFrequency(int frequency)
        {
            foreach (TuningFrequency tuningFrequency in Frequencies)
            {
                if (tuningFrequency.Frequency == frequency)
                    return (tuningFrequency);
            }

            return (null);
        }        

        /// <summary>
        /// Get a string representing this instance.
        /// </summary>
        /// <returns>The description of this instance.</returns>
        public override string ToString()
        {
            return (name);
        }

        /// <summary>
        /// Check this provider for equality with another.
        /// </summary>
        /// <param name="provider">The other frequency.</param>
        /// <param name="level">The level of equality to be checked.</param>
        /// <returns>True if the instances are equal; false otherwise.</returns>
        public virtual bool EqualTo(Provider provider, EqualityLevel level)
        {
            if (name != provider.Name)
                return (false);

            return (true);
        }

        /// <summary>
        /// Log the network information.
        /// </summary>
        public virtual void LogNetworkInfo() { }
    }
}
