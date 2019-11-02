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
    public class SatelliteFrequency : TuningFrequency, IComparable
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
        /// Get or set the DVB signal polarization.
        /// </summary>
        public int DVBPolarization
        {
            get { return (dvbPolarization); }
            set 
            { 
                dvbPolarization = value;
                Polarization = SignalPolarization.ConvertDVBPolarization(value);
            }
        }

        /// <summary>
        /// Get or set the DVB-S2 pilot value.
        /// </summary>
        public SignalPilot.Pilot Pilot
        {
            get { return (pilot); }
            set { pilot = value; }
        }

        /// <summary>
        /// Get or set the DVB-S2 roll-off value.
        /// </summary>
        public SignalRollOff.RollOff RollOff
        {
            get { return (rollOff); }
            set { rollOff = value; }
        }

        /// <summary>
        /// Get or set the modulation.
        /// </summary>
        public SignalModulation.Modulation Modulation
        {
            get { return (modulation); }
            set { modulation = value; }
        }

        /// <summary>
        /// Get or set the DVB modulation.
        /// </summary>
        public int DVBModulation
        {
            get { return (dvbModulation); }
            set 
            { 
                dvbModulation = value;
                Modulation = SignalModulation.ConvertDVBModulation(value);
            }
        }

        /// <summary>
        /// Get or set the LNB conversion flag.
        /// </summary>
        public bool LNBConversion
        {
            get { return (lnbConversion); }
            set { lnbConversion = value; }
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
        /// Get or set the modulation system.
        /// </summary>
        public int ModulationSystem
        {
            get { return (modulationSystem); }
            set { modulationSystem = value; }
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
        /// Return true if the modulation system is S2; false otherwise.
        /// </summary>
        public bool IsS2 
        { 
            get 
            {
                if (modulationSystem == 1)
                    return (true);

                if (pilot != SignalPilot.Pilot.NotSet && rollOff != SignalRollOff.RollOff.NotSet)
                    return (true);

                return (false);
            } 
        }

        /// <summary>
        /// Return true if the frequency is Dish Network
        /// </summary>
        public bool IsDishNetwork { get { return (CollectionType == CollectionType.DishNetwork); } }

        /// <summary>
        /// Get the tuner type needed for this type of frequency.
        /// </summary>
        public override TunerType TunerType { get { return (TunerType.Satellite); } }

        private int symbolRate = 22500;
        private FECRate fec = new FECRate();
        private SignalPolarization polarization = new SignalPolarization("Linear Horizontal");
        private int dvbPolarization;
        private SignalPilot.Pilot pilot = SignalPilot.Pilot.NotSet;
        private SignalRollOff.RollOff rollOff = SignalRollOff.RollOff.NotSet;
        private SignalModulation.Modulation modulation = SignalModulation.Modulation.QPSK;
        private int dvbModulation;
        private SatelliteDish satelliteDish;
        private bool lnbConversion;

        private int modulationSystem;

        private DiseqcRunParameters diseqcRunParameters;
        
        /// <summary>
        /// Initialize a new instance of the SatelliteFrequency class.
        /// </summary>
        public SatelliteFrequency() { }

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
                        case "Pilot":
                            switch (reader.ReadString())
                            {
                                case "NotSet":
                                    pilot = SignalPilot.Pilot.NotSet;
                                    break;
                                case "NotDefined":
                                    pilot = SignalPilot.Pilot.NotDefined;
                                    break;
                                case "On":
                                    pilot = SignalPilot.Pilot.On;
                                    break;
                                case "Off":
                                    pilot = SignalPilot.Pilot.Off;
                                    break;
                                default:
                                    pilot = SignalPilot.Pilot.NotSet;
                                    break;
                            }
                            break;
                        case "Rolloff":
                            switch (reader.ReadString())
                            {
                                case "NotSet":
                                    rollOff = SignalRollOff.RollOff.NotSet;
                                    break;
                                case "NotDefined":
                                    rollOff = SignalRollOff.RollOff.NotDefined;
                                    break;
                                case "Twenty":
                                    rollOff = SignalRollOff.RollOff.RollOff20;
                                    break;
                                case "TwentyFive":
                                    rollOff = SignalRollOff.RollOff.RollOff25;
                                    break;
                                case "ThirtyFive":
                                    rollOff = SignalRollOff.RollOff.RollOff35;
                                    break;
                                default:
                                    rollOff = SignalRollOff.RollOff.NotSet;
                                    break;
                            }
                            break;
                        case "Modulation":
                            switch (reader.ReadString())
                            {
                                case "ModBPSK":
                                    modulation = SignalModulation.Modulation.BPSK;
                                    break;
                                case "ModOQPSK":
                                    modulation = SignalModulation.Modulation.OQPSK;
                                    break;
                                case "Mod8Psk":
                                    modulation = SignalModulation.Modulation.PSK8;
                                    break;
                                case "Mod1024Qam":
                                    modulation = SignalModulation.Modulation.QAM1024;
                                    break;
                                case "Mod112Qam":
                                    modulation = SignalModulation.Modulation.QAM112;
                                    break;
                                case "Mod128Qam":
                                    modulation = SignalModulation.Modulation.QAM128;
                                    break;
                                case "Mod16Qam":
                                    modulation = SignalModulation.Modulation.QAM16;
                                    break;
                                case "Mod160Qam":
                                    modulation = SignalModulation.Modulation.QAM160;
                                    break;
                                case "Mod192Qam":
                                    modulation = SignalModulation.Modulation.QAM192;
                                    break;
                                case "Mod224Qam":
                                    modulation = SignalModulation.Modulation.QAM224;
                                    break;
                                case "Mod256Qam":
                                    modulation = SignalModulation.Modulation.QAM256;
                                    break;
                                case "Mod32Qam":
                                    modulation = SignalModulation.Modulation.QAM32;
                                    break;
                                case "Mod320Qam":
                                    modulation = SignalModulation.Modulation.QAM320;
                                    break;
                                case "Mod384Qam":
                                    modulation = SignalModulation.Modulation.QAM384;
                                    break;
                                case "Mod448Qam":
                                    modulation = SignalModulation.Modulation.QAM448;
                                    break;
                                case "Mod512Qam":
                                    modulation = SignalModulation.Modulation.QAM512;
                                    break;
                                case "Mod64Qam":
                                    modulation = SignalModulation.Modulation.QAM64;
                                    break;
                                case "Mod640Qam":
                                    modulation = SignalModulation.Modulation.QAM640;
                                    break;
                                case "Mod768Qam":
                                    modulation = SignalModulation.Modulation.QAM768;
                                    break;
                                case "Mod80Qam":
                                    modulation = SignalModulation.Modulation.QAM80;
                                    break;
                                case "Mod896Qam":
                                    modulation = SignalModulation.Modulation.QAM896;
                                    break;
                                case "Mod96Qam":
                                    modulation = SignalModulation.Modulation.QAM96;
                                    break;
                                case "ModQPSK":
                                    modulation = SignalModulation.Modulation.QPSK;
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
        /// Compare another satellite frequency with this one.
        /// </summary>
        /// <param name="compareFrequency">The tuning frequency to be compared to.</param>
        /// <returns>0 if the frequencies are equal, -1 if this instance is less, +1 otherwise.</returns>
        public override int CompareTo(object compareFrequency)
        {
            SatelliteFrequency satelliteFrequency = compareFrequency as SatelliteFrequency;
            if (satelliteFrequency == null)
                throw (new ArgumentException("Object is not a SatelliteFrequency"));

            if (satelliteFrequency.Frequency == Frequency)
                return(polarization.ToString().CompareTo(satelliteFrequency.Polarization.ToString()));

            return(Frequency.CompareTo(satelliteFrequency.Frequency));          
        }
        
        /// <summary>
        /// Get a description of this satellite frequency.
        /// </summary>
        /// <returns>A string describing this frequency.</returns>
        public override string ToString()
        {
            if (Frequency == 0)
                return (" -- New --");

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
            SatelliteFrequency newFrequency = new SatelliteFrequency();
            base.Clone(newFrequency);

            newFrequency.FEC = fec;
            newFrequency.Polarization = polarization;
            newFrequency.SymbolRate = symbolRate;
            newFrequency.Pilot = pilot;
            newFrequency.RollOff = rollOff;
            newFrequency.Modulation = modulation;

            if (satelliteDish != null)
                newFrequency.SatelliteDish = (SatelliteDish)satelliteDish.Clone();

            newFrequency.LNBConversion = lnbConversion;

            newFrequency.DiseqcRunParamters = DiseqcRunParamters.Clone();

            return (newFrequency);
        }

        /// <summary>
        /// Check if this instance is equal to another.
        /// </summary>
        /// <param name="frequency">The other instance.</param>
        /// <param name="level">The level of equality to be checked.</param>
        /// <returns></returns>
        public override bool EqualTo(TuningFrequency frequency, EqualityLevel level)
        {
            SatelliteFrequency satelliteFrequency = frequency as SatelliteFrequency;
            if (satelliteFrequency == null)
                return (false);

            bool reply = base.EqualTo(satelliteFrequency, level);
            if (!reply)
                return (false);

            if (Polarization.Polarization != satelliteFrequency.Polarization.Polarization)
                return (false);

            if (level == EqualityLevel.Identity)
                return(true);

            if (Provider as Satellite != null)
            {
                if (!((Satellite)Provider).EqualTo(((Satellite)(frequency.Provider)), level))
                    return (false);
            }

            if (SatelliteDish != null)
            {
                if (satelliteFrequency.SatelliteDish != null)
                {
                    if (!SatelliteDish.EqualTo(satelliteFrequency.SatelliteDish))
                        return (false);
                }
                else
                    return (false);
            }
            else
            {
                if (satelliteFrequency.SatelliteDish != null)
                    return (false);
            }

            if (SymbolRate != satelliteFrequency.SymbolRate)
                return (false);

            if (FEC.Rate != satelliteFrequency.FEC.Rate)
                return (false);

            if (Modulation != satelliteFrequency.Modulation)
                return (false);

            if (Pilot != satelliteFrequency.Pilot)
                return (false);

            if (RollOff != satelliteFrequency.RollOff)
                return (false);

            if (LNBConversion != satelliteFrequency.LNBConversion)
                return (false);

            return (DiseqcRunParamters.EqualTo(satelliteFrequency.DiseqcRunParamters));
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
                writer.WriteElementString("Modulation", SignalModulation.GetDvbsXml(Modulation));
                writer.WriteElementString("InnerFecRate", FEC.ToString());

                if (Pilot != SignalPilot.Pilot.NotDefined && Pilot != SignalPilot.Pilot.NotSet)
                {
                    writer.WriteElementString("Pilot", Pilot.ToString());
                    writer.WriteElementString("Rolloff", SignalRollOff.GetXml(RollOff));
                }

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
