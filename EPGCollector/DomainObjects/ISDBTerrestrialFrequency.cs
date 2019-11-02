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
    /// The class that describes a terrestrial frequency.
    /// </summary>
    public class ISDBTerrestrialFrequency : ChannelTuningFrequency, IComparable
    {
        /// <summary>
        /// Get or set the bandwidth.
        /// </summary>
        public int Bandwidth
        {
            get { return (bandwidth); }
            set { bandwidth = value; }
        }

        /// <summary>
        /// Get the tuner type for this type of frequency.
        /// </summary>
        public override TunerType TunerType { get { return (TunerType.ISDBT); } }

        private int bandwidth;

        /// <summary>
        /// Initialize a new instance of the ISDBTerrestrialFrequency class.
        /// </summary>
        public ISDBTerrestrialFrequency() { }

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
                            Frequency = Int32.Parse(reader.ReadString());
                            break;
                        case "BandWidth":
                            Bandwidth = Int32.Parse(reader.ReadString());
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
            ISDBTerrestrialFrequency isdbFrequency = frequency as ISDBTerrestrialFrequency;
            if (isdbFrequency == null)
                return (false);

            bool reply = base.EqualTo(isdbFrequency, level);
            if (!reply)
                return (false);

            if (level == EqualityLevel.Identity)
                return (true);

            if (Bandwidth != isdbFrequency.Bandwidth)
                return (false);

            return (true);
        }

        /// <summary>
        /// Get a description of this instance.
        /// </summary>
        /// <returns>A string describing this instance.</returns>
        public override string ToString()
        {
            return ("Channel " + ChannelNumber + " (" + Frequency / 1000 + " MHz)");
        }

        /// <summary>
        /// Generate a copy of this frequency.
        /// </summary>
        /// <returns>A new instance with the same properties as the old instance.</returns>
        public override TuningFrequency Clone()
        {
            ISDBTerrestrialFrequency newFrequency = new ISDBTerrestrialFrequency();
            base.Clone(newFrequency);

            newFrequency.Bandwidth = bandwidth;

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
                writer.WriteStartElement("Channel");

                writer.WriteElementString("CarrierFrequency", Frequency.ToString());
                writer.WriteElementString("BandWidth", Bandwidth.ToString());
                writer.WriteElementString("ChannelNumber", ChannelNumber.ToString());
                
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
