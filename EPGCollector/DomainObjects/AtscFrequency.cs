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
    /// The class that describes an ATSC frequency.
    /// </summary>
    public class AtscFrequency : ChannelTuningFrequency, IComparable
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
        /// Get or set the modulation.
        /// </summary>
        public SignalModulation.Modulation Modulation
        {
            get { return (modulation); }
            set { modulation = value; }
        }

        /// <summary>
        /// Get the tuner type for this type of frequency.
        /// </summary>
        public override TunerType TunerType 
        { 
            get 
            {
                if (modulation == SignalModulation.Modulation.VSB8)
                    return TunerType.ATSC;
                else
                    return TunerType.ATSCCable;                
            } 
        }             

        private int symbolRate = 6000;
        private FECRate fec = new FECRate();
        private SignalModulation.Modulation modulation = SignalModulation.Modulation.VSB8;
        
        /// <summary>
        /// Initialize a new instance of the AtscFrequency class.
        /// </summary>
        public AtscFrequency() { }

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
                        case "CarrierFrequency":
                            Frequency = Int32.Parse(reader.ReadString(), CultureInfo.InvariantCulture);
                            break;
                        case "SymbolRate":
                            symbolRate = Int32.Parse(reader.ReadString(), CultureInfo.InvariantCulture);
                            break;
                        case "Modulation":
                            switch (reader.ReadString())
                            {
                                case "BPSK":
                                    modulation = SignalModulation.Modulation.BPSK;
                                    break;
                                case "OQPSK":
                                    modulation = SignalModulation.Modulation.OQPSK;
                                    break;
                                case "PSK8":
                                    modulation = SignalModulation.Modulation.PSK8;
                                    break;
                                case "QAM1024":
                                    modulation = SignalModulation.Modulation.QAM1024;
                                    break;
                                case "QAM112":
                                    modulation = SignalModulation.Modulation.QAM112;
                                    break;
                                case "QAM128":
                                    modulation = SignalModulation.Modulation.QAM128;
                                    break;
                                case "QAM16":
                                    modulation = SignalModulation.Modulation.QAM16;
                                    break;
                                case "QAM160":
                                    modulation = SignalModulation.Modulation.QAM160;
                                    break;
                                case "QAM192":
                                    modulation = SignalModulation.Modulation.QAM192;
                                    break;
                                case "QAM224":
                                    modulation = SignalModulation.Modulation.QAM224;
                                    break;
                                case "QAM256":
                                    modulation = SignalModulation.Modulation.QAM256;
                                    break;
                                case "QAM32":
                                    modulation = SignalModulation.Modulation.QAM32;
                                    break;
                                case "QAM320":
                                    modulation = SignalModulation.Modulation.QAM320;
                                    break;
                                case "QAM384":
                                    modulation = SignalModulation.Modulation.QAM384;
                                    break;
                                case "QAM448":
                                    modulation = SignalModulation.Modulation.QAM448;
                                    break;
                                case "QAM512":
                                    modulation = SignalModulation.Modulation.QAM512;
                                    break;
                                case "QAM64":
                                    modulation = SignalModulation.Modulation.QAM64;
                                    break;
                                case "QAM640":
                                    modulation = SignalModulation.Modulation.QAM640;
                                    break;
                                case "QAM768":
                                    modulation = SignalModulation.Modulation.QAM768;
                                    break;
                                case "QAM80":
                                    modulation = SignalModulation.Modulation.QAM80;
                                    break;
                                case "QAM896":
                                    modulation = SignalModulation.Modulation.QAM896;
                                    break;
                                case "QAM96":
                                    modulation = SignalModulation.Modulation.QAM96;
                                    break;
                                case "QPSK":
                                    modulation = SignalModulation.Modulation.QPSK;
                                    break;
                                case "VSB16":
                                    modulation = SignalModulation.Modulation.VSB16;
                                    break;
                                case "VSB16Cable":
                                    modulation = SignalModulation.Modulation.VSB16Cable;
                                    break;
                                case "VSB8":
                                    modulation = SignalModulation.Modulation.VSB8;
                                    break;
                                case "VSB8Cable":
                                    modulation = SignalModulation.Modulation.VSB8Cable;
                                    break;
                            }
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
            AtscFrequency atscFrequency = frequency as AtscFrequency;
            if (atscFrequency == null)
                return (false);

            bool reply = base.EqualTo(atscFrequency, level);
            if (!reply)
                return (false);

            if (level == EqualityLevel.Identity)
                return (true);

            if (SymbolRate != atscFrequency.SymbolRate)
                return (false);

            if (FEC.Rate != atscFrequency.FEC.Rate)
                return (false);

            if (Modulation != atscFrequency.Modulation)
                return (false);

            return (true);
        }

        /// <summary>
        /// Get a description of this instance.
        /// </summary>
        /// <returns>A string describing this instance.</returns>
        public override string ToString()
        {
            return ("Channel " + ChannelNumber + " (" + Frequency / 1000  + " MHz)");
        }

        /// <summary>
        /// Generate a copy of this frequency.
        /// </summary>
        /// <returns>A new instance with the same properties as the old instance.</returns>
        public override TuningFrequency Clone()
        {
            AtscFrequency newFrequency = new AtscFrequency();
            base.Clone(newFrequency);
            
            newFrequency.SymbolRate = symbolRate;
            newFrequency.FEC = fec;
            newFrequency.SymbolRate = symbolRate;
            newFrequency.Modulation = modulation;

            return (newFrequency);
        }

        /// <summary>
        /// Get a hash code for this instance.
        /// </summary>
        /// <returns>The hash code.</returns>
        public override int GetHashCode()
        {
            return (base.GetHashCode());
        }

        /// <summary>
        /// Compare another object with this one.
        /// </summary>
        /// <param name="obj">The other object.</param>
        /// <returns>True if the objects are equal; false otherwise.</returns>
        public override bool Equals(object obj)
        {
            return (base.Equals(obj));
        }

        /// <summary>
        /// Compare two instances for equality.
        /// </summary>
        /// <param name="frequency1">The first frequency.</param>
        /// <param name="frequency2">The second frequency</param>
        /// <returns>True if the instances are equal; false otherwise.</returns>
        public static bool operator ==(AtscFrequency frequency1, AtscFrequency frequency2)
        {
            return (Equals(frequency1, frequency2));
        }

        /// <summary>
        /// Compare two instances for inequality.
        /// </summary>
        /// <param name="frequency1">The first frequency.</param>
        /// <param name="frequency2">The second frequency</param>
        /// <returns>True if the instances are not equal; false otherwise.</returns>
        public static bool operator !=(AtscFrequency frequency1, AtscFrequency frequency2)
        {
            return(!Equals(frequency1, frequency2));
        }

        /// <summary>
        /// Compare two instances for greater than.
        /// </summary>
        /// <param name="frequency1">The first frequency.</param>
        /// <param name="frequency2">The second frequency</param>
        /// <returns>True if the instance 1 is greater than instance 2; false otherwise.</returns>
        public static bool operator >(AtscFrequency frequency1, AtscFrequency frequency2)
        {
            if (frequency1 == null || frequency2 == null)
                return (false);

            return (frequency1.Frequency > frequency2.Frequency);
        }

        /// <summary>
        /// Compare two instances for less than.
        /// </summary>
        /// <param name="frequency1">The first frequency.</param>
        /// <param name="frequency2">The second frequency</param>
        /// <returns>True if the instance 1 is less than instance 2; false otherwise.</returns>
        public static bool operator <(AtscFrequency frequency1, AtscFrequency frequency2)
        {
            if (frequency1 == null || frequency2 == null)
                return (false);

            return (frequency1.Frequency < frequency2.Frequency);
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
                writer.WriteStartElement("Channel");

                writer.WriteElementString("ChannelNumber", ChannelNumber.ToString());
                writer.WriteElementString("CarrierFrequency", Frequency.ToString());
                writer.WriteElementString("SymbolRate", SymbolRate.ToString());
                writer.WriteElementString("Modulation", SignalModulation.GetAtscXml(Modulation));
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
