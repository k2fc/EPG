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

using System.Globalization;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;

namespace DomainObjects
{
    /// <summary>
    /// Initialize a new instance of the Satellite class.
    /// </summary>
    public class Satellite : Provider
    {
        /// <summary>
        /// Get the collection of providers.
        /// </summary>
        public static Collection<Satellite> Providers
        {
            get
            {
                if (providers == null)
                    providers = new Collection<Satellite>();
                return (providers);
            }
        }

        /// <summary>
        /// Get or set the azimuth of the satellite.
        /// </summary>
        public int Azimuth
        {
            get { return (azimuth); }
            set { azimuth = value; }
        }

        /// <summary>
        /// Get or set the elevation of the satellite.
        /// </summary>
        public int Elevation
        {
            get { return (elevation); }
            set { elevation = value; }
        }

        /// <summary>
        /// Get or set the longitude of the satellite in tenths of a degree.
        /// </summary>
        public int Longitude
        {
            get { return (longitude); }
            set { longitude = value; }
        }

        /// <summary>
        /// Get or set the east/west setting.
        /// </summary>
        public string EastWest
        {
            get { return (eastWest); }
            set { eastWest = value; }
        }

        /// <summary>
        /// Get the sort key for the satellite.
        /// </summary>
        public decimal SortKey
        {
            get
            {
                if (Name == null)
                    return (0);

                string[] parts = Name.Split(new char[] { ' ' });

                decimal eastWest = 0;
                if (parts[0].EndsWith("W"))
                    eastWest = 10000;

                decimal longitude = decimal.Parse(parts[0].Substring(0, parts[0].Length - 2), CultureInfo.InvariantCulture);
                return ((longitude * 10) + eastWest);                
            }
        }

        private int azimuth;
        private int elevation;
        private int longitude;
        private string eastWest;        

        private bool lnbConversion;
        
        private static Collection<Satellite> providers;

        /// <summary>
        /// Initialize a new instance of the Satellite class.
        /// </summary>
        public Satellite() : base() { }

        /// <summary>
        /// Initialize a new instance of the Satellite class.
        /// </summary>
        /// <param name="name">The name of the satellite.</param>
        public Satellite(string name) : base(name) 
        {
            string[] parts = Name.Split(new char[] { ' ' });
            if (parts.Length == 1)
                return;

            if (parts[0].EndsWith("E"))
                eastWest = "east";
            else
                eastWest = "west";

            longitude = (int)(decimal.Parse(parts[0].Substring(0, parts[0].Length - 2), CultureInfo.InvariantCulture) * 10);       
        }

        /// <summary>
        /// Initialize a new instance of the Satellite class.
        /// </summary>
        /// <param name="longitude">The longitude in 1/10th's of a degree. Negative for west.</param>
        public Satellite(int longitude)
        {
            string namePart1 = ((decimal)(longitude / 10)).ToString();

            string namePart2;
            if (longitude < 0)
                namePart2 = "W";
            else
                namePart2 = "E";

            Satellite satellite = new Satellite(namePart1 + "\u00b0" + namePart2 + " Satellite");

            if (longitude < 0)
            {
                eastWest = "west";
                this.longitude = longitude * -1;
            }
            else
            {
                eastWest = "east";
                this.longitude = longitude;
            }
        }

        /// <summary>
        /// Find a satellite.
        /// </summary>
        /// <param name="name">The name of the Satellite.</param>
        /// <returns>The satellite or null if the name cannot be located.</returns>
        public static Satellite FindSatellite(string name)
        {
            foreach (Satellite satellite in Providers)
            {
                if (satellite.Name == name)
                    return (satellite);
            }

            return (null);
        }

        /// <summary>
        /// Find a satellite.
        /// </summary>
        /// <param name="longitude">The longitude of the Satellite.</param>
        /// <returns>The satellite or null if the name cannot be located.</returns>
        public static Satellite FindSatellite(int longitude)
        {
            foreach (Satellite satellite in Providers)
            {
                if (satellite.Longitude == longitude)
                    return (satellite);
            }

            return (null);
        }

        internal void load(FileInfo fileInfo)
        {
            XmlReader reader = null;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;

            try
            {
                reader = XmlReader.Create(fileInfo.FullName, settings);
            }
            catch (IOException)
            {
                Logger.Instance.Write("Failed to open " + fileInfo.Name);
                return;
            }
            
            try
            {
                while (!reader.EOF)
                {
                    reader.Read();
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name)
                        {  
                            case "Transponder":
                                SatelliteFrequency satelliteFrequency = new SatelliteFrequency();
                                satelliteFrequency.Provider = this;
                                satelliteFrequency.LNBConversion = lnbConversion;
                                satelliteFrequency.load(reader.ReadSubtree());
                                AddFrequency(satelliteFrequency);
                                break;
                            case "LNBConversion":
                                if (reader.ReadString() == "yes")
                                    lnbConversion = true;
                                else
                                    lnbConversion = false;
                                break;
                            default:
                                loadBase(reader);
                                break;
                        }
                    }
                }
            }
            catch (XmlException e)
            {
                Logger.Instance.Write("Failed to load file " + fileInfo.Name);
                Logger.Instance.Write("Data exception: " + e.Message);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("Failed to load file " + fileInfo.Name);
                Logger.Instance.Write("I/O exception: " + e.Message);
            }

            if (reader != null)
                reader.Close();
        }

        /// <summary>
        /// Load the satellite collection from the tuning files.
        /// </summary>
        public static void Load()
        {
            if (Providers.Count != 0)
                return;

            Providers.Clear();

            string directoryName = Path.Combine(RunParameters.DataDirectory, "TuningParameters", "dvbs");
            DirectoryInfo directoryInfo;
            
            if (Directory.Exists(directoryName))
            {
                Logger.Instance.Write("Loading DVB-S tuning files from " + directoryName);

                directoryInfo = new DirectoryInfo(directoryName);

                foreach (FileInfo fileInfo in directoryInfo.GetFiles("*.xml"))
                {
                    Satellite satellite = new Satellite(fileInfo.Name.Substring(0, fileInfo.Name.Length - 4));
                    /*satellite.Name = fileInfo.Name.Substring(0, fileInfo.Name.Length - 4);*/
                    satellite.load(fileInfo);
                    AddProvider(satellite);
                }
            }

            directoryName = Path.Combine(RunParameters.ConfigDirectory, "TuningParameters", "dvbs");

            if (Directory.Exists(directoryName))
            {
                Logger.Instance.Write("Loading DVB-S tuning files from " + directoryName);

                directoryInfo = new DirectoryInfo(directoryName);

                foreach (FileInfo fileInfo in directoryInfo.GetFiles("*.xml"))
                {
                    Satellite satellite = new Satellite(fileInfo.Name.Substring(0, fileInfo.Name.Length - 4));
                    satellite.load(fileInfo);
                    AddProvider(satellite);
                }
            }
            
        }

        /// <summary>
        /// Add a new frequency.
        /// </summary>
        /// <param name="newFrequency">The frequency to be added.</param>
        public void AddFrequency(SatelliteFrequency newFrequency)
        {
            foreach (SatelliteFrequency oldFrequency in Frequencies)
            {
                if (oldFrequency.Frequency == newFrequency.Frequency)
                {
                    if (oldFrequency.Polarization == newFrequency.Polarization)
                        return;
                    else
                    {
                        if (oldFrequency.Polarization.PolarizationAbbreviation.CompareTo(newFrequency.Polarization.PolarizationAbbreviation) > 0)
                        {
                            Frequencies.Insert(Frequencies.IndexOf(oldFrequency), newFrequency);
                            return;
                        }
                    }
                }

                if (oldFrequency.Frequency > newFrequency.Frequency)
                {
                    Frequencies.Insert(Frequencies.IndexOf(oldFrequency), newFrequency);
                    return;
                }
            }

            Frequencies.Add(newFrequency);
        }

        /// <summary>
        /// Add a provider to the list.
        /// </summary>
        /// <param name="newProvider">The provider to be added.</param>
        public static void AddProvider(Satellite newProvider)
        {
            foreach (Satellite oldProvider in Providers)
            {
                if (oldProvider.SortKey == newProvider.SortKey)
                    return;

                if (oldProvider.SortKey > newProvider.SortKey)
                {
                    Providers.Insert(Providers.IndexOf(oldProvider), newProvider);
                    return;
                }
            }

            Providers.Add(newProvider);
        }

        /// <summary>
        /// Find a provider.
        /// </summary>
        /// <param name="name">The name of the provider.</param>
        /// <returns>The provider or null if the name cannot be located.</returns>
        public static Satellite FindProvider(string name)
        {
            foreach (Satellite provider in Providers)
            {
                if (provider.Name == name)
                    return (provider);
            }

            return (null);
        }

        /// <summary>
        /// Find a provider given the broadcast parameters.
        /// </summary>
        /// <param name="frequency">The frequency of the provider.</param>
        /// <param name="symbolRate">The symbol rate of the provider.</param>
        /// <param name="fecRate">The FEC rate of the provider.</param>
        /// <param name="polarization">The polarization of the provider.</param>
        /// <returns>The provider or null if it cannot be located.</returns>
        public static Satellite FindProvider(int frequency, int symbolRate, FECRate fecRate, SignalPolarization polarization)
        {
            foreach (Satellite provider in Providers)
            {
                foreach (SatelliteFrequency satelliteFrequency in provider.Frequencies)
                {
                    if (satelliteFrequency.Frequency == frequency &&
                        satelliteFrequency.SymbolRate == symbolRate &&
                        satelliteFrequency.FEC.Rate == fecRate.Rate &&
                        satelliteFrequency.Polarization.Polarization == polarization.Polarization)
                        return (provider);
                }
            }

            return (null);
        }

        /// <summary>
        /// Find a satelllite frequency.
        /// </summary>
        /// <param name="frequency">The frequency to be searched for.</param>
        /// <param name="polarization">The polariz\ation of the frequency to be searched for.</param>
        /// <returns>The tuning frequency or null if it cannot be located.</returns>
        public SatelliteFrequency FindFrequency(int frequency, SignalPolarization polarization)
        {
            foreach (TuningFrequency tuningFrequency in Frequencies)
            {
                if (tuningFrequency.Frequency == frequency)
                {
                    SatelliteFrequency satelliteFrequency = tuningFrequency as SatelliteFrequency;
                    if (satelliteFrequency != null && satelliteFrequency.Polarization.Polarization == polarization.Polarization)
                        return (satelliteFrequency);
                }
            }

            return (null);
        }

        /// <summary>
        /// Check if this instance is equal to another.
        /// </summary>
        /// <param name="provider">The other instance.</param>
        /// <param name="level">The level of equality to be checked.</param>
        /// <returns>True if the instances are equal; false otherwise.</returns>
        public override bool EqualTo(Provider provider, EqualityLevel level)
        {
            Satellite satellite = provider as Satellite;
            if (satellite == null)
                return (false);

            bool reply = base.EqualTo(satellite, level);
            if (!reply)
                return (false);

            if (Longitude != satellite.Longitude)
                return (false);

            if (EastWest != satellite.EastWest)
                return (false);

            if (level == EqualityLevel.Identity)
                return (true);

            if (Azimuth != satellite.Azimuth)
                return (false);

            if (Elevation != satellite.Elevation)
                return (false);

            return (true);
        }

        /// <summary>
        /// Log the network information
        /// </summary>
        public override void LogNetworkInfo()
        {
            if (!RunParameters.Instance.ChannelLogNetworkMap)
            {
                Logger.Instance.Write("Network name is " + Name);
                return;
            }

            Logger.Instance.Write("Network map for network " + Name);

            foreach (SatelliteFrequency frequency in Frequencies)
            {
                double orbitalPosition = ((double)Longitude / 10);

                Logger.Instance.Write(orbitalPosition.ToString("0.0") +
                    " " + EastWest +
                    " Frequency " + frequency.Frequency + " " + frequency.Polarization.PolarizationAbbreviation +
                    " " + (frequency.IsS2 ? " S2 " : string.Empty) +
                    " Symbol rate " + frequency.SymbolRate +
                    " Modulation " + frequency.Modulation +
                    " Fec " + frequency.FEC);

                if (frequency.Stations != null && frequency.Stations.Count != 0)
                {
                    foreach (TVStation station in frequency.Stations)
                    {
                        if (station.Included)
                            Logger.Instance.Write("    " + station.FullFixedLengthDescription);
                        else
                            Logger.Instance.Write("    " + station.FullFixedLengthDescription + " excluded");
                    }
                }
            }
        }

        /// <summary>
        /// Create an xml file containing the frequency definitions.
        /// </summary>
        /// <returns>Null if the file was created successfully; an error message otherwise.</returns>
        public string Unload()
        {
            string path = Path.Combine(RunParameters.DataDirectory, "TuningParameters", "dvbs");

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string fullPath = Path.Combine(path, Name + ".xml");

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.CloseOutput = true;
            settings.Indent = true;
            settings.NewLineOnAttributes = false;

            XmlWriter writer = null;

            try
            {
                writer = XmlWriter.Create(new FileStream(fullPath, FileMode.Create), settings);

                writer.WriteStartElement("Transponders");
                writer.WriteAttributeString("generator-info-name", "DomainObjects " + RunParameters.AssemblyVersion);

                unloadBase(writer);

                bool first = true;

                foreach (SatelliteFrequency frequency in Frequencies)
                {
                    if (first)
                    {
                        if (frequency.LNBConversion)
                            writer.WriteElementString("LNBConversion", "yes");                        
                        first = false;
                    }

                    string reply = frequency.Unload(writer, fullPath);
                    if (reply != null)
                        return (reply);
                }

                writer.WriteEndElement();
                writer.Close();
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
