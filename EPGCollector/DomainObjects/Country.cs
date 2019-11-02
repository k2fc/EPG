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
using System.IO;
using System.Xml;
using System.Collections.ObjectModel;

namespace DomainObjects
{
    /// <summary>
    /// The class that describes a country.
    /// </summary>
    public class Country
    {
        /// <summary>
        /// Get the name of the country.
        /// </summary>
        public string Name { get { return(name); } }

        /// <summary>
        /// Get the code of the country.
        /// </summary>
        public string Code { get { return (code); } }

        /// <summary>
        /// Get the collection of countries.
        /// </summary>
        public static Collection<Country> Countries
        {
            get
            {
                if (countries == null)
                    countries = Load();
                return (countries);
            }
        }

        /// <summary>
        /// Get the collection of services in the country.
        /// </summary>
        public Collection<Service> Services
        {
            get
            {
                if (services == null)
                    services = new Collection<Service>();
                return (services);
            }
        }
        
        /// <summary>
        /// Get the collection of areas in the country.
        /// </summary>
        public Collection<Area> Areas
        {
            get
            {
                if (areas == null)
                    areas = new Collection<Area>();
                return(areas);
            }
        }

        /// <summary>
        /// Get the country code for Egypt.
        /// </summary>
        public const string Egypt = "EGY";
        /// <summary>
        /// Get the country code for New Zealand.
        /// </summary>
        public const string NewZealand = "NZL";
        /// <summary>
        /// Get the country code for Australia.
        /// </summary>
        public const string Australia = "AUS";
        /// <summary>
        /// Get the country code for France.
        /// </summary>
        public const string France = "FRA";
        /// <summary>
        /// Get the country code for Spain.
        /// </summary>
        public const string Spain = "ESP";
        /// <summary>
        /// Get the country code for the United Kingdom.
        /// </summary>
        public const string UnitedKingdom = "GBR";
        /// <summary>
        /// Get the country code for Italy.
        /// </summary>
        public const string Italy = "ITA";

        private string name;
        private string code;

        private static Collection<Country> countries;
        
        private Collection<Service> services;
        private Collection<Area> areas;
        
        private Country() { }

        /// <summary>
        /// Initialize a new instance of the Country class.
        /// </summary>
        /// <param name="name">The name of the country.</param>
        /// <param name="code">The code of the country.</param>
        public Country(string name, string code)
        {
            this.name = name;
            this.code = code;
        }

        internal void load(XmlReader reader)
        {
            while (!reader.EOF)
            {
                reader.Read();
                if (reader.IsStartElement())
                {
                    switch (reader.Name)
                    {
                        case "Service":
                            Service service = new Service(reader.GetAttribute("name"), (ServiceType)Enum.Parse(typeof(ServiceType), reader.GetAttribute("type"), true));
                            if (services == null)
                                services = new Collection<Service>();
                            services.Add(service);
                            break;
                        case "Area":
                            Area area = new Area(reader.GetAttribute("name"), Int32.Parse(reader.GetAttribute("code")));
                            area.Load(reader.ReadSubtree());
                            AddArea(area, true);

                            if (services != null)
                            {
                                Service lastService = services[services.Count - 1];
                                if (lastService.Areas == null)
                                    lastService.Areas = new Collection<Area>();
                                lastService.Areas.Add(area);
                            }

                            break;
                        default:
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Add an area to the country.
        /// </summary>
        /// <param name="newArea">The area to be added.</param>
        /// <param name="addUndefined">True if an undefined entry is to be added to the start of the list; false otherwise.</param>
        public void AddArea(Area newArea, bool addUndefined)
        {
            if (addUndefined && Areas.Count == 0)
            {
                Area undefinedArea = new Area("-- Undefined --", 0);
                undefinedArea.AddRegion(new Region("-- Undefined --", 0));
                Areas.Add(undefinedArea);
            }

            foreach (Area oldArea in Areas)
            {
                if (oldArea.Name == newArea.Name)
                    return;

                if (oldArea.Name.CompareTo(newArea.Name) > 0)
                {
                    areas.Insert(areas.IndexOf(oldArea), newArea);
                    return;
                }
            }

            areas.Add(newArea);
        }

        /// <summary>
        /// Return a description of this instance.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return (name);
        }

        /// <summary>
        /// Load the country collection from the configuration file.
        /// </summary>
        public static Collection<Country> Load()
        {
            if (countries != null)
                return (countries);

            countries = new Collection<Country>();

            XmlReader reader = null;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;

            try
            {
                reader = XmlReader.Create(Path.Combine(RunParameters.ConfigDirectory, "Countries.cfg"), settings);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("<E> Failed to open " + Path.Combine(RunParameters.ConfigDirectory, "Countries.cfg"));
                Logger.Instance.Write("<E> " + e.Message);
                return(countries);
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
                            case "Country":
                                Country country = new Country(reader.GetAttribute("name"), reader.GetAttribute("code"));
                                country.load(reader.ReadSubtree());                                
                                addCountry(country, countries);
                                break;
                            default:
                               break;
                        }
                    }
                }
            }
            catch (XmlException e)
            {
                Logger.Instance.Write("Failed to load file " + Path.Combine(RunParameters.ConfigDirectory, "Countries.cfg"));
                Logger.Instance.Write("Data exception: " + e.Message);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("Failed to load file " + Path.Combine(RunParameters.ConfigDirectory, "Countries.cfg"));
                Logger.Instance.Write("I/O exception: " + e.Message);
            }

            if (reader != null)
                reader.Close();

            return (countries);
        }

        /// <summary>
        /// Add a country to a collection.
        /// </summary>
        /// <param name="newCountry">The country to be added.</param>
        /// <param name="countries">The collection of countries to be added to.</param>
        public static void addCountry(Country newCountry, Collection<Country> countries)
        {
            if (countries.Count == 0)
            {
                Country undefinedCountry = new Country("-- Undefined --", "");
                Area undefinedArea = new Area("-- Undefined --", 0);
                undefinedArea.Regions.Add(new Region("-- Undefined --", 0));
                undefinedCountry.Areas.Add(undefinedArea);
                countries.Add(undefinedCountry);
            }

            foreach (Country oldCountry in countries)
            {
                if (oldCountry.Code == newCountry.Code)
                    return;

                if (oldCountry.Code.CompareTo(newCountry.Code) > 0)
                {
                    countries.Insert(countries.IndexOf(oldCountry), newCountry);
                    return;
                }
            }

            countries.Add(newCountry);
        }

        /// <summary>
        /// Find a country given the country code.
        /// </summary>
        /// <param name="countryCode">The country code.</param>
        /// <param name="countries">The collection of countries to search.</param>
        /// <returns>The country or null if it cannot be located.</returns>
        public static Country FindCountryCode(string countryCode, Collection<Country> countries)
        {
            foreach (Country country in countries)
            {
                if (country.Code == countryCode)
                    return (country);
            }

            return (null);
        }

        /// <summary>
        /// Find a service defined for the country.
        /// </summary>
        /// <param name="serviceName">The name of the service.</param>
        /// <returns>The service or null if it cannot be located.</returns>
        public Service FindService(string serviceName)
        {
            foreach (Service service in Services)
            {
                if (service.Name.Trim().ToLowerInvariant() == serviceName.Trim().ToLowerInvariant())
                    return (service);
            }

            return (null);
        }

        /// <summary>
        /// Find a service defined for the country.
        /// </summary>
        /// <param name="serviceName">The name of the service.</param>
        /// <param name="type">The type of the service.</param>
        /// <returns>The service or null if it cannot be located.</returns>
        public Service FindService(string serviceName, string type)
        {
            foreach (Service service in Services)
            {
                if (service.Name.Trim().ToLowerInvariant() == serviceName.Trim().ToLowerInvariant() &&
                    service.ServiceType.ToString().ToLowerInvariant() == type.ToLowerInvariant())
                    return (service);
            }

            return (null);
        }

        /// <summary>
        /// Find an area defined for the country.
        /// </summary>
        /// <param name="areaCode">The area code.</param>
        /// <returns>The area or null if it cannot be located.</returns>
        public Area FindArea(int areaCode)
        {
            foreach (Area area in Areas)
            {
                if (area.Code == areaCode)
                    return (area);
            }

            return (null);
        }

        /// <summary>
        /// Find an area defined for the country that contains a specific region.
        /// </summary>
        /// <param name="areaCode">The area code.</param>
        /// <param name="regionCode">The region code.</param>
        /// <returns>The area or null if it cannot be located.</returns>
        public Area FindArea(int areaCode, int regionCode)
        {
            foreach (Area area in Areas)
            {
                if (area.Code == areaCode)
                {
                    foreach (Region region in area.Regions)
                    {
                        if (region.Code == regionCode)
                            return (area);
                    }
                }
            }

            return (null);
        }

        /// <summary>
        /// Find a region defined for the country.
        /// </summary>
        /// <param name="areaCode">The area code.</param>
        /// <param name="regionCode">The region code.</param>
        /// <returns>The region or null if it cannot be located.</returns>
        public Region FindRegion(int areaCode, int regionCode)
        {
            foreach (Area area in Areas)
            {
                if (area.Code == areaCode)
                {
                    foreach (Region region in area.Regions)
                    {
                        if (region.Code == regionCode)
                            return (region);
                    }
                }
            }

            return (null);
        }
    }
}
