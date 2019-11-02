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
using System.Xml;
using System.IO;

namespace DomainObjects
{
    /// <summary>
    /// The class that describes a satellite frequency.
    /// </summary>
    public class ISDBSatelliteFrequency : TuningFrequency, IComparable
    {
        /// <summary>
        /// Get or set the symbol rate.
        /// </summary>
        public int SymbolRate
        {
            get { return (symbolRate); }
            set { symbolRate = value; }
        }

        /// <summary>
        /// Get or set the forward error correction system.
        /// </summary>
        public FECRate FEC
        {
            get { return (fec); }
            set { fec = value; }
        }

        /// <summary>
        /// Get or set the signal polarization.
        /// </summary>
        public SignalPolarization Polarization
        {
            get { return (polarization); }
            set { polarization = value; }
        }

        /// <summary>
        /// Get or set the satellite dish for this frequency.
        /// </summary>
        public SatelliteDish SatelliteDish
        {
            get { return (satelliteDish); }
            set { satelliteDish = value; }
        }

        /// <summary>
        /// Get or set the DiSEqC run parameters for this frequency;
        /// </summary>
        public DiseqcRunParameters DiseqcRunParamters
        {
            get
            {
                if (diseqcRunParameters == null)
                    diseqcRunParameters = new DiseqcRunParameters();
                return (diseqcRunParameters);
            }
            set { diseqcRunParameters = value; }
        }

        /// <summary>
        /// Get the tuner type needed for this type of frequency.
        /// </summary>
        public override TunerType TunerType { get { return (TunerType.ISDBS); } }

        private int symbolRate = 30000;
        private FECRate fec = new FECRate();
        private SignalPolarization polarization = new SignalPolarization("Linear Horizontal");
        private SatelliteDish satelliteDish;

        private DiseqcRunParameters diseqcRunParameters;

        /// <summary>
        /// Initialize a new instance of the ISDBSatelliteFrequency class.
        /// </summary>
        public ISDBSatelliteFrequency() { }

        internal void load(XmlReader reader)
        {
            while (!reader.EOF)
            {
                reader.Read();
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "CarrierFrequency":
                            Frequency = Int32.Parse(reader.ReadString());
                            break;
                        case "Polarisation":
                            switch (reader.ReadString())
                            {
                                case "CircularL":
                                    polarization = new SignalPolarization(SignalPolarization.CircularLeft);
                                    break;
                                case "CircularR":
                                    polarization = new SignalPolarization(SignalPolarization.CircularRight);
                                    break;
                                case "LinearH":
                                    polarization = new SignalPolarization(SignalPolarization.LinearHorizontal);
                                    break;
                                case "LinearV":
                                    polarization = new SignalPolarization(SignalPolarization.LinearVertical);
                                    break;
                            }
                            break;
                        case "SymbolRate":
                            symbolRate = Int32.Parse(reader.ReadString());
                            break;
                        case "InnerFecRate":
                            switch (reader.ReadString())
                            {
                                case "Rate1_2":
                                    fec = new FECRate(FECRate.FECRate12);
                                    break;
                                case "Rate1_3":
                                    fec = new FECRate(FECRate.FECRate13);
                                    break;
                                case "Rate1_4":
                                    fec = new FECRate(FECRate.FECRate14);
                                    break;
                                case "Rate2_3":
                                    fec = new FECRate(FECRate.FECRate23);
                                    break;
                                case "Rate2_5":
                                    fec = new FECRate(FECRate.FECRate25);
                                    break;
                                case "Rate3_4":
                                    fec = new FECRate(FECRate.FECRate34);
                                    break;
                                case "Rate3_5":
                                    fec = new FECRate(FECRate.FECRate35);
                                    break;
                                case "Rate4_5":
                                    fec = new FECRate(FECRate.FECRate45);
                                    break;
                                case "Rate5_11":
                                    fec = new FECRate(FECRate.FECRate511);
                                    break;
                                case "Rate5_6":
                                    fec = new FECRate(FECRate.FECRate56);
                                    break;
                                case "Rate6_7":
                                    fec = new FECRate(FECRate.FECRate67);
                                    break;
                                case "Rate7_8":
                                    fec = new FECRate(FECRate.FECRate78);
                                    break;
                                case "Rate8_9":
                                    fec = new FECRate(FECRate.FECRate89);
                                    break;
                                case "Rate9_10":
                                    fec = new FECRate(FECRate.FECRate910);
                                    break;
                            }
                            break;
                        default:
                            loadBase(reader);
                            break;
                    }
                }
            }

            reader.Close();
        }

        /// <summary>
        /// Check if this instance is equal to another.
        /// </summary>
        /// <param name="frequency">The other instance.</param>
        /// <param name="level">The level of equality to be checked.</param>
        /// <returns></returns>
        public override bool EqualTo(TuningFrequency frequency, EqualityLevel level)
        {
            ISDBSatelliteFrequency isdbFrequency = frequency as ISDBSatelliteFrequency;
            if (isdbFrequency == null)
                return (false);

            bool reply = base.EqualTo(isdbFrequency, level);
            if (!reply)
                return (false);

            if (Polarization.Polarization != isdbFrequency.Polarization.Polarization)
                return (false);

            if (level == EqualityLevel.Identity)
                return (true);

            if (!((ISDBSatelliteProvider)Provider).EqualTo(((ISDBSatelliteProvider)(frequency.Provider)), level))
                return (false);

            if (SatelliteDish != null)
            {
                if (isdbFrequency.SatelliteDish != null)
                {
                    if (!SatelliteDish.EqualTo(isdbFrequency.SatelliteDish))
                        return (false);
                }
                else
                    return (false);
            }
            else
            {
                if (isdbFrequency.SatelliteDish != null)
                    return (false);
            }

            if (SymbolRate != isdbFrequency.SymbolRate)
                return (false);

            if (FEC.Rate != isdbFrequency.FEC.Rate)
                return (false);

            return (DiseqcRunParamters.EqualTo(isdbFrequency.DiseqcRunParamters));
        }

        /// <summary>
        /// Compare another satellite frequency with this one.
        /// </summary>
        /// <param name="compareFrequency">The tuning frequency to be compared to.</param>
        /// <returns>0 if the frequencies are equal, -1 if this instance is less, +1 otherwise.</returns>
        public override int CompareTo(object compareFrequency)
        {
            ISDBSatelliteFrequency satelliteFrequency = compareFrequency as ISDBSatelliteFrequency;
            if (satelliteFrequency == null)
                throw (new ArgumentException("Object is not a ISDBSatelliteFrequency"));

            if (satelliteFrequency.Frequency == Frequency)
                return (polarization.ToString().CompareTo(satelliteFrequency.Polarization.ToString()));

            return (Frequency.CompareTo(satelliteFrequency.Frequency));
        }

        /// <summary>
        /// Get a description of this satellite frequency.
        /// </summary>
        /// <returns>A string describing this frequency.</returns>
        public override string ToString()
        {
            string polarity = string.Empty;

            switch (polarization.Polarization)
            {
                case SignalPolarization.CircularLeft:
                    polarity = "L";
                    break;
                case SignalPolarization.CircularRight:
                    polarity = "R";
                    break;
                case SignalPolarization.LinearHorizontal:
                    polarity = "H";
                    break;
                case SignalPolarization.LinearVertical:
                    polarity = "V";
                    break;
                default:
                    polarity = "?";
                    break;
            }

            return (Frequency.ToString() + " - " + polarity);
        }

        /// <summary>
        /// Generate a copy of this frequency.
        /// </summary>
        /// <returns>A new instance with the same properties as the old instance.</returns>
        public override TuningFrequency Clone()
        {
            ISDBSatelliteFrequency newFrequency = new ISDBSatelliteFrequency();
            base.Clone(newFrequency);

            newFrequency.FEC = fec;
            newFrequency.Polarization = polarization;
            newFrequency.SymbolRate = symbolRate;

            if (satelliteDish != null)
                newFrequency.SatelliteDish = (SatelliteDish)satelliteDish.Clone();

            newFrequency.DiseqcRunParamters = DiseqcRunParamters.Clone();

            return (newFrequency);
        }

        /// <summary>
        /// Create the xml definition for the frequency.
        /// </summary>
        /// <param name="writer">An xml writer instance.</param>
        /// <param name="fullPath">The full path of the file being created.</param>
        /// <returns>Null if the entry was created successfully; an error message otherwise.</returns>
        public string Unload(XmlWriter writer, string fullPath)
        {
            try
            {
                writer.WriteStartElement("Transponder");

                writer.WriteElementString("CarrierFrequency", Frequency.ToString());
                writer.WriteElementString("Polarisation", Polarization.GetXml());
                writer.WriteElementString("SymbolRate", SymbolRate.ToString());
                writer.WriteElementString("InnerFecRate", FEC.ToString());
                
                unloadBase(writer);

                writer.WriteEndElement();
            }
            catch (XmlException e)
            {
                Logger.Instance.Write("Failed to unload " + fullPath);
                Logger.Instance.Write("Data exception: " + e.Message);
                return ("Failed to unload " + fullPath);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("Failed to unload " + fullPath);
                Logger.Instance.Write("I/O exception: " + e.Message);
                return ("Failed to unload " + fullPath);
            }

            return (null);
        }
    }
}
