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
using System.Globalization;
using System.IO;

namespace DomainObjects
{
    /// <summary>
    /// The class that describes a cable frequency.
    /// </summary>
    public class CableFrequency : ChannelTuningFrequency, IComparable
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
        /// Get or set the FEC rate.
        /// </summary>
        public FECRate FEC
        {
            get { return (fec); }
            set { fec = value; }
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
        /// Returns a tuner type of cable.
        /// </summary>
        public override TunerType TunerType { get { return (TunerType.Cable); } }

        private int symbolRate = 22500;
        private FECRate fec = new FECRate();
        private SignalModulation.Modulation modulation = SignalModulation.Modulation.QPSK;
        private int dvbModulation;
        
        /// <summary>
        /// Initialize a new instance of the CableFrequency class.
        /// </summary>
        public CableFrequency() { }

        internal void load(XmlReader reader)
        {
            while (!reader.EOF)
            {
                reader.Read();
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "ChannelNumber":
                            ChannelNumber = Int32.Parse(reader.ReadString(), CultureInfo.InvariantCulture);
                            break;
                        case "ModulationType":
                            switch (reader.ReadString())
                            {
                                case "ModBPSK":
                                    modulation = SignalModulation.Modulation.BPSK;
                                    break;
                                case "ModOQPSK":
                                    modulation = SignalModulation.Modulation.OQPSK;
                                    break;
                                case "ModPSK8":
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
                        case "SymbolRate":
                            symbolRate = Int32.Parse(reader.ReadString(), CultureInfo.InvariantCulture);
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
            CableFrequency cableFrequency = frequency as CableFrequency;
            if (cableFrequency == null)
                return (false);

            bool reply = base.EqualTo(cableFrequency, level);
            if (!reply)
                return (false);

            if (level == EqualityLevel.Identity)
                return (true);

            if (SymbolRate != cableFrequency.SymbolRate)
                return (false);

            if (FEC.Rate != cableFrequency.FEC.Rate)
                return (false);

            if (Modulation != cableFrequency.Modulation)
                return (false);

            return (true);
        }

        /// <summary>
        /// Return a string describing the frequency.
        /// </summary>
        /// <returns>A string describing the frequency.</returns>
        public override string ToString()
        {
            if (ChannelNumber == 0)
                return (Frequency / 1000 + " MHz");
            else
                return ("Channel " + ChannelNumber + " (" + Frequency / 1000 + " MHz)");

        }

        /// <summary>
        /// Generate a copy of this frequency.
        /// </summary>
        /// <returns>A new instance with the same properties as the old instance.</returns>
        public override TuningFrequency Clone()
        {
            CableFrequency newFrequency = new CableFrequency();
            base.Clone(newFrequency);

            newFrequency.FEC = fec;
            newFrequency.SymbolRate = symbolRate;
            newFrequency.Modulation = modulation;            

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
                writer.WriteStartElement("DVBCTuning");

                writer.WriteElementString("Frequency", Frequency.ToString());
                writer.WriteElementString("SymbolRate", SymbolRate.ToString());
                writer.WriteElementString("ModulationType", SignalModulation.GetDvbcXml(Modulation));
                
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
