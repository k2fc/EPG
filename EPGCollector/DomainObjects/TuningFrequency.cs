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
    /// The class that describes a tuning frequency.
    /// </summary>
    public abstract class TuningFrequency
    {
        /// <summary>
        /// Get or set the frequency.
        /// </summary>
        public int Frequency
        {
            get { return (frequency); }
            set { frequency = value; }
        }

        /// <summary>
        /// Get or set the type of data collection requested.
        /// </summary>
        public CollectionType CollectionType
        {
            get { return (collectionType); }
            set { collectionType = value; }
        }

        /// <summary>
        /// Get or set the DSMCC PID for this frequency.
        /// </summary>
        public int DSMCCPid
        {
            get { return (dsmccPid); }
            set { dsmccPid = value; }
        }

        /// <summary>
        /// Get or set the number of entries for this frequency.
        /// </summary>
        public int UsageCount
        {
            get { return (usageCount); }
            set { usageCount = value; }
        }

        /// <summary>
        /// Get or set the provider for this frequency.
        /// </summary>
        public Provider Provider
        {
            get { return (provider); }
            set { provider = value; }
        }

        /// <summary>
        /// Get or set the country code for this frequency.
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// Get or set the list of options for this frequency that were loaded from the tuning file.
        /// </summary>
        public Collection<OptionEntry> TuningFileOptions { get; set; }
        
        /// <summary>
        /// Get or set the list of channels for this frequency.
        /// </summary>
        public Collection<TVStation> Stations
        {
            get { return (stations); }
            set { stations = value; }
        }

        /// <summary>
        /// Get or set the Sat>IP front end to use.
        /// </summary>
        public int SatIpFrontend
        {
            get { return (satIpFrontend); }
            set { satIpFrontend = value; }
        }

        /// <summary>
        /// Get or set the advanced run parameters for this frequency;
        /// </summary>
        public AdvancedRunParameters AdvancedRunParamters 
        { 
            get 
            {
                if (advancedRunParameters == null)
                    advancedRunParameters = new AdvancedRunParameters();
                return (advancedRunParameters); 
            }
            set { advancedRunParameters = value; }
        }

        /// <summary>
        /// Get the selected tuners for this frequency.
        /// </summary>
        public Collection<SelectedTuner> SelectedTuners
        {
            get
            {
                if (selectedTuners == null)
                    selectedTuners = new Collection<SelectedTuner>();
                return (selectedTuners);
            }
        }

        /// <summary>
        /// Return the tuner type required by this frequency. This is overridden by derived classes to return a specific type.
        /// </summary>
        public virtual TunerType TunerType { get { return(TunerType.Other); } }

        private int frequency;
        private int dsmccPid;
        private CollectionType collectionType;
        private int usageCount;
        private Provider provider;
        private Collection<TVStation> stations;
        private int satIpFrontend = -1;
        private AdvancedRunParameters advancedRunParameters;
        private Collection<SelectedTuner> selectedTuners;
        
        /// <summary>
        ///  Initialize a new instance of the TuningFrequency class.
        /// </summary>
        public TuningFrequency() { }
        
        /// <summary>
        /// Initialize a new instance of the TuningFrequency class for a specified frequency and collection type.
        /// </summary>
        /// <param name="frequency">The frequency.</param>
        /// <param name="collectionType">The collection type.</param>
        public TuningFrequency(int frequency, CollectionType collectionType)
        {
            this.frequency = frequency;
            this.collectionType = collectionType;
        }

        /// <summary>
        /// Load the base properties.
        /// </summary>
        /// <param name="reader">The XmlReader to use.</param>
        protected void loadBase(XmlReader reader)
        {
            switch (reader.Name)
            {
                case "Frequency":
                    Frequency = Int32.Parse(reader.ReadString());
                    break;
                case "CollectionType":
                    switch (reader.ReadString().ToUpperInvariant())
                    {
                        case "EIT":
                            CollectionType = CollectionType.EIT;
                            break;
                        case "MHEG5":
                            CollectionType = CollectionType.MHEG5;
                            break;
                        case "OPENTV":
                            CollectionType = CollectionType.OpenTV;
                            break;
                        case "MHW1":
                            CollectionType = CollectionType.MediaHighway1;
                            break;
                        case "MHW2":
                            CollectionType = CollectionType.MediaHighway2;
                            break;
                        case "FREESAT":
                            CollectionType = CollectionType.FreeSat;
                            break;
                        case "PSIP":
                            CollectionType = CollectionType.PSIP;
                            break;
                        case "DISHNETWORK":
                            CollectionType = CollectionType.DishNetwork;
                            break;
                        case "BELLTV":
                            CollectionType = CollectionType.BellTV;
                            break;
                        case "SIEHFERNINFO":
                            CollectionType = CollectionType.SiehfernInfo;
                            break;
                        case "NDS":
                            CollectionType = CollectionType.NDS;
                            break;
                    }
                    break;
                case "CountryCode":
                    CountryCode = reader.ReadString();
                    break;
                case "Option":
                    try
                    {
                        OptionName optionName = (OptionName)Enum.Parse(typeof(OptionName), reader.ReadString().Trim(), true);
                        if (TuningFileOptions == null)
                            TuningFileOptions = new Collection<OptionEntry>();
                        TuningFileOptions.Add(new OptionEntry(optionName));
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
            writer.WriteElementString("CollectionType", CollectionType.ToString());

            if (CountryCode != null)
                writer.WriteElementString("CountryCode", CountryCode);

            if (TuningFileOptions != null && TuningFileOptions.Count != 0)
            {
                writer.WriteStartElement("Options");
                foreach (OptionEntry optionEntry in TuningFileOptions)
                    writer.WriteElementString("Option", optionEntry.Name.ToString());
                writer.WriteEndElement();
            }
        }

        /// <summary>
        /// Compare another tuning frequency with this one.
        /// </summary>
        /// <param name="compareFrequency">The tuning frequency to be compared to.</param>
        /// <returns>0 if the frequencies are equal, -1 if this instance is less, +1 otherwise.</returns>
        public virtual int CompareTo(object compareFrequency)
        {
            TuningFrequency tuningFrequency = compareFrequency as TuningFrequency;
            if (tuningFrequency != null)
                return(frequency.CompareTo(tuningFrequency.frequency));
            else
                throw (new ArgumentException("Object is not a TuningFrequency"));
        }

        /// <summary>
        /// Get a string representing this instance.
        /// </summary>
        /// <returns>The description of this instance.</returns>
        public override string ToString()
        {
            return (frequency.ToString());
        }

        /// <summary>
        /// Generate a copy of this frequency.
        /// </summary>
        /// <returns>A copy of this instance.</returns>
        public abstract TuningFrequency Clone();

        /// <summary>
        /// Generate a valid file name for this frequency.
        /// </summary>
        /// <returns>A valid file name.</returns>
        public virtual string GetValidFileName()
        {
            return (ToString());
        }

        /// <summary>
        /// Generate a copy of this frequency.
        /// </summary>
        /// <returns>A new instance with the same properties as the old instance.</returns>
        protected void Clone(TuningFrequency newFrequency)
        {
            newFrequency.Frequency = frequency;
            newFrequency.Provider = provider;
            newFrequency.CollectionType = collectionType;
            newFrequency.SatIpFrontend = satIpFrontend;
            newFrequency.CountryCode = CountryCode;            

            if (TuningFileOptions == null)
                newFrequency.TuningFileOptions = null;
            else
            {
                newFrequency.TuningFileOptions = new Collection<OptionEntry>();
                foreach (OptionEntry optionEntry in TuningFileOptions)
                    newFrequency.TuningFileOptions.Add(optionEntry.Clone());
            }

            newFrequency.AdvancedRunParamters = AdvancedRunParamters.Clone();

            foreach (SelectedTuner selectedTuner in SelectedTuners)
                newFrequency.SelectedTuners.Add(selectedTuner.Clone());
        }

        /// <summary>
        /// Check this frequency for equality with another.
        /// </summary>
        /// <param name="tuningFrequency">The other frequency.</param>
        /// <param name="level">The level of equality to be checked.</param>
        /// <returns>True if the instances are equal; false otherwise.</returns>
        public virtual bool EqualTo(TuningFrequency tuningFrequency, EqualityLevel level)
        {
            if (GetType().FullName != tuningFrequency.GetType().FullName)
                return (false);

            if (Frequency != tuningFrequency.Frequency)
                return (false);

            if (provider != null)
            {
                if (!provider.EqualTo(tuningFrequency.Provider, level))
                    return (false);
            }
            else
            {
                if (tuningFrequency.Provider != null)
                    return (false);
            }

            if (level == EqualityLevel.Identity)
                return (true);

            if (CollectionType != tuningFrequency.CollectionType)
                return (false);

            if (SatIpFrontend != tuningFrequency.SatIpFrontend)
                return (false);

            if (SelectedTuners.Count != tuningFrequency.SelectedTuners.Count)
                return (false);
            else
            {
                for (int index = 0; index < SelectedTuners.Count; index++)
                {

                    if (!SelectedTuners[index].EqualTo(tuningFrequency.SelectedTuners[index]))
                        return (false);
                }
            }

            return (this.AdvancedRunParamters.EqualTo(tuningFrequency.AdvancedRunParamters));
        }

        /// <summary>
        /// Compare another object with this one.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if the objects are equal; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            TuningFrequency otherFrequency = obj as TuningFrequency;
            if (otherFrequency == null)
                return (false);

            return (EqualTo(otherFrequency, EqualityLevel.Entirely));
        }

        /// <summary>
        /// Get a hash code for this instance.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return(ToString().GetHashCode());
        }

        /// <summary>
        /// Find a frequency.
        /// </summary>
        /// <param name="frequencies">The frequency list to search.</param>
        /// <param name="frequency">The frequency to be located.</param>
        /// <returns>The frequency instance or null if it cannot be located.</returns>
        public static TuningFrequency FindFrequency(Collection<TuningFrequency> frequencies, int frequency)
        {
            foreach (TuningFrequency tuningFrequency in frequencies)
            {
                if (tuningFrequency.Frequency == frequency)
                    return (tuningFrequency);
            }

            return (null);
        }

        /// <summary>
        /// Check if a collection type is present in a list of frequencies.
        /// </summary>
        /// <param name="frequencies">The list of frequencies.</param>
        /// <param name="collectionType">The collection type to check for.</param>
        /// <returns>True if the collection type is present; false otherwise.</returns>
        public static bool HasCollectionType(Collection<TuningFrequency> frequencies, CollectionType collectionType)
        {
            foreach (TuningFrequency tuningFrequency in frequencies)
            {
                if (tuningFrequency.CollectionType == collectionType)
                    return (true);
            }

            return (false);
        }

        /// <summary>
        /// Check if the MHEG5 collection type has been used in a list of frequencies and the PID is present.
        /// </summary>
        /// <param name="frequencies">The list of frequencies.</param>
        /// <returns>True if MHEG5 is present in the list and the PID is present; false otherwise.</returns>
        public static bool HasUsedMHEG5Frequency(Collection<TuningFrequency> frequencies)
        {
            foreach (TuningFrequency tuningFrequency in frequencies)
            {
                if (tuningFrequency.CollectionType == CollectionType.MHEG5 && tuningFrequency.DSMCCPid != 0)
                    return (true);
            }

            return (false);
        }

        /// <summary>
        /// Check if a satellite frequency is present in a list of frequencies.
        /// </summary>
        /// <param name="frequencies">The list of frequencies.</param>
        /// <returns>True if a satellite frequency is present; false otherwise.</returns>
        public static bool HasDVBSatelliteFrequency(Collection<TuningFrequency> frequencies)
        {
            foreach (TuningFrequency tuningFrequency in frequencies)
            {
                if (tuningFrequency.TunerType == TunerType.Satellite)
                    return (true);
            }

            return (false);
        }

        /// <summary>
        /// Check if a terrestrial frequency is present in a list of frequencies.
        /// </summary>
        /// <param name="frequencies">The list of frequencies.</param>
        /// <returns>True if a terrestrial frequency is present; false otherwise.</returns>
        public static bool HasDVBTerrestrialFrequency(Collection<TuningFrequency> frequencies)
        {
            foreach (TuningFrequency tuningFrequency in frequencies)
            {
                if (tuningFrequency.TunerType == TunerType.Terrestrial)
                    return (true);
            }

            return (false);
        }

        /// <summary>
        /// Check if a cable frequency is present in a list of frequencies.
        /// </summary>
        /// <param name="frequencies">The list of frequencies.</param>
        /// <returns>True if a cable frequency is present; false otherwise.</returns>
        public static bool HasDVBCableFrequency(Collection<TuningFrequency> frequencies)
        {
            foreach (TuningFrequency tuningFrequency in frequencies)
            {
                if (tuningFrequency.TunerType == TunerType.Cable)
                    return (true);
            }

            return (false);
        }

        /// <summary>n ATSC frequency is present in a list of frequencies.
        /// </summary>
        /// <param name="frequencies">The list of frequencies.</param>
        /// <returns>True if an ATSC frequency is present; false otherwise.</returns>
        public static bool HasAtscFrequency(Collection<TuningFrequency> frequencies)
        {
            foreach (TuningFrequency tuningFrequency in frequencies)
            {
                if (tuningFrequency.TunerType == TunerType.ATSC || tuningFrequency.TunerType == TunerType.ATSCCable)
                    return (true);
            }

            return (false);
        }

        /// <summary>
        /// Check if a clear QAM frequency is present in a list of frequencies.
        /// </summary>
        /// <param name="frequencies">The list of frequencies.</param>
        /// <returns>True if a clear QAM frequency is present; false otherwise.</returns>
        public static bool HasClearQamFrequency(Collection<TuningFrequency> frequencies)
        {
            foreach (TuningFrequency tuningFrequency in frequencies)
            {
                if (tuningFrequency.TunerType == TunerType.ClearQAM)
                    return (true);
            }

            return (false);
        }

        /// <summary>
        /// Check if tuners are needed for any frequency in a list of frequencies.
        /// </summary>
        /// <param name="frequencies">The list of frequencies.</param>
        /// <returns>True if a tuner is needed; false otherwise.</returns>
        public static bool TunersNeeded(Collection<TuningFrequency> frequencies)
        {
            foreach (TuningFrequency tuningFrequency in frequencies)
            {
                if (tuningFrequency.TunerType != TunerType.File && tuningFrequency.TunerType != TunerType.Stream)
                    return (true);
            }

            return (false);
        }

        /// <summary>
        /// Get the Sat>IP DiSEqC setting if relevant.
        /// </summary>
        /// <param name="frequency">The tuning frequency.</param>
        /// <returns>The Sat>IP setting.</returns>
        public static int GetDiseqcSetting(TuningFrequency frequency)
        {
            SatelliteFrequency satelliteFrequency = frequency as SatelliteFrequency;
            if (satelliteFrequency == null)
                return (0);

            if (satelliteFrequency.DiseqcRunParamters.DiseqcSwitch == null)
                return (0);

            switch (satelliteFrequency.DiseqcRunParamters.DiseqcSwitch)
            {
                case "A":
                    return (1);
                case "B":
                    return (2);
                case "AA":
                    return (1);
                case "AB":
                    return (2);
                case "BA":
                    return (3);
                case "BB":
                    return (4);
                default:
                    return (0);
            }
        }
    }
}
