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
using System.Text;

using DomainObjects;

namespace ChannelUpdate
{
    internal class DVBLinkConfiguration
    {
        internal DVBLinkConfigurationNode ConfigurationNode { get; private set; }

        internal Collection<DVBLinkSource> Sources { get; private set; }

        internal int MaxFrequency
        {
            get
            {
                DVBLinkElement maxFrequencyElement = DVBLinkBaseNode.FindElement(ConfigurationNode, new string[] { "dvblink_configuration", "max_frequency" });
                if (maxFrequencyElement != null)
                    return (Int32.Parse(maxFrequencyElement.Value));
                else
                    return (-1);
            }
            set
            {
                DVBLinkElement maxFrequencyElement = DVBLinkBaseNode.FindElement(ConfigurationNode, new string[] { "dvblink_configuration", "max_frequency" });
                if (maxFrequencyElement != null)
                    maxFrequencyElement.Value = value.ToString();
            }
        }

        internal int ChannelCount
        {
            get
            {
                if (Sources == null)
                    return (0);

                int total = 0;

                foreach (DVBLinkSource source in Sources)
                {
                    if (source.HeadEnds != null)
                    {
                        foreach (DVBLinkHeadEnd headEnd in source.HeadEnds)
                        {
                            if (headEnd.Channels != null)
                                total += headEnd.Channels.Count;
                        }
                    }                        
                }

                return (total);
            }
        }

        internal static int MaxFrequencyInitialValue = 10000000;

        private static string epgScanIntervalName = "EPGScanRepeatDelay";
        private static int epgScanIntervalDefault = 720;

        internal DVBLinkConfiguration() { }

        internal bool Load(string installPath)
        {
            XmlReader xmlReader = null;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;

            string fileName = Path.Combine(installPath, "dvblink_configuration.xml");

            try
            {
                xmlReader = XmlReader.Create(fileName, settings);
            }
            catch (IOException)
            {
                Logger.Instance.Write("Failed to open " + fileName);
                return (false);
            }

            Logger.Instance.Write("Processing " + fileName);

            xmlReader.Read();
            if (xmlReader.Name != "xml")
                throw (new FormatException("Expected xml element - got " + xmlReader.Name));

            ConfigurationNode = new DVBLinkConfigurationNode();
            bool reply = ConfigurationNode.Load(xmlReader);

            if (xmlReader != null)
                xmlReader.Close();

            return (reply);
        }

        internal bool Unload(string installPath)
        {
            string fileName = Path.Combine(installPath, "dvblink_configuration.xml");
            string backupName = Path.Combine(installPath, "dvblink_configuration.xml.bak");

            try
            {
                Logger.Instance.Write("Deleting backup file " + backupName);
                File.SetAttributes(backupName, FileAttributes.Normal);
                File.Delete(backupName);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("File delete exception: " + e.Message);
            }

            try
            {
                Logger.Instance.Write("Renaming " + fileName + " for backup");
                File.Move(fileName, backupName);
                File.SetAttributes(backupName, FileAttributes.ReadOnly);                
            }
            catch (IOException e)
            {
                Logger.Instance.Write("File rename exception: " + e.Message);
                return (false);
            }

            Logger.Instance.Write("Creating configuration file: " + fileName);

            XmlWriterSettings settings = new XmlWriterSettings();

            settings.Indent = true;
            settings.NewLineOnAttributes = false;
            settings.Encoding = new UTF8Encoding();
            settings.CloseOutput = true;

            try
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(fileName, settings))
                {
                    xmlWriter.WriteStartDocument();
                    ConfigurationNode.Unload(xmlWriter);
                    xmlWriter.WriteEndDocument();

                    xmlWriter.Flush();
                    xmlWriter.Close();
                }
            }
            catch (XmlException e)
            {
                Logger.Instance.Write("<E> XML Exception creating channel storage file");
                Logger.Instance.Write("E>" + e.Message);
                return (false);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("<E> IO Exception creating channel storage file");
                Logger.Instance.Write("E>" + e.Message);
                return (false);
            }

            return (true);
        }

        internal bool UpdateScanInterval()
        {
            bool updated = false;

            foreach (DVBLinkSource source in Sources)
            {
                if (source.IsTunable)
                {
                    if (source.EPGScanRepeatDelay == null)
                    {
                        if (RunParameters.Instance.ChannelEPGScanInterval != epgScanIntervalDefault)
                        {
                            if (addScanInterval(source, RunParameters.Instance.ChannelEPGScanInterval))
                                updated = true;
                        }
                    }
                    else
                    {
                        if (source.EPGScanRepeatDelay != (RunParameters.Instance.ChannelEPGScanInterval * 60).ToString())
                        {
                            if (RunParameters.Instance.ChannelEPGScanInterval != epgScanIntervalDefault)
                            {
                                if (changeScanInterval(source, RunParameters.Instance.ChannelEPGScanInterval))
                                    updated = true;
                            }
                            else
                            {
                                if (removeScanInterval(source))
                                    updated = true;
                            }
                        }
                    }
                }
            }

            return (updated);
        }

        private bool addScanInterval(DVBLinkSource source, int scanInterval)
        {
            Logger.Instance.Write("Add "+ epgScanIntervalName + " of " + (scanInterval * 60) + " to source " + source.NormalizedName);

            DVBLinkElement sourceElement = DVBLinkBaseNode.FindElement(ConfigurationNode, new string[] { "dvblink_configuration", "sources", source.ElementName });
            if (sourceElement == null)
            {
                Logger.Instance.Write("Failed to locate source " + source.NormalizedName + " - " + epgScanIntervalName + " not set");
                return (false); ;
            }

            if (sourceElement.Elements == null || sourceElement.Elements.Count == 0)
            {
                Logger.Instance.Write("Found source " + source.NormalizedName + " but elements missing - " + epgScanIntervalName + " not set");
                return (false); ;
            }

            sourceElement.Elements.Add(new DVBLinkElement(epgScanIntervalName, (scanInterval * 60).ToString()));
            Logger.Instance.Write("Source " + source.NormalizedName + " EPGScanRepeatDelay set to " + (scanInterval * 60));

            return (true);
        }

        private bool changeScanInterval(DVBLinkSource source, int scanInterval)
        {
            Logger.Instance.Write("Changing " + epgScanIntervalName + " to " + (scanInterval * 60) + " for source " + source.NormalizedName);

            DVBLinkElement element = DVBLinkBaseNode.FindElement(ConfigurationNode, new string[] { "dvblink_configuration", "sources", source.ElementName, epgScanIntervalName });
            if (element == null)
            {
                Logger.Instance.Write("Failed to locate element " + epgScanIntervalName + " - value not changed");
                return (false); ;
            }

            element.Value = (scanInterval * 60).ToString();
            Logger.Instance.Write("Source " + source.NormalizedName + " EPGScanRepeatDelay set to " + (scanInterval * 60));

            return (true);
        }

        private bool removeScanInterval(DVBLinkSource source)
        {
            Logger.Instance.Write("Removing " + epgScanIntervalName + " from source " + source.NormalizedName);

            DVBLinkElement sourceElement = DVBLinkBaseNode.FindElement(ConfigurationNode, new string[] { "dvblink_configuration", "sources", source.ElementName });
            if (sourceElement == null)
            {
                Logger.Instance.Write("Failed to locate source " + source.NormalizedName + " - " + epgScanIntervalName + " not removed");
                return (false); ;
            }

            if (sourceElement.Elements == null || sourceElement.Elements.Count == 0)
            {
                Logger.Instance.Write("Found source " + source.NormalizedName + " but elements missing - " + epgScanIntervalName + " not removed");
                return (false); ;
            }

            foreach (DVBLinkElement element in sourceElement.Elements)
            {
                if (element.Name == epgScanIntervalName)
                {
                    sourceElement.Elements.Remove(element);
                    Logger.Instance.Write("Source " + source.NormalizedName + " " + epgScanIntervalName + " removed");
                    return (true); ;
                }
            }

            Logger.Instance.Write("Found source " + source.NormalizedName + " but element missing - " + epgScanIntervalName + " not removed");
            return (false);
        }

        internal bool LoadSources()
        {
            if (Sources == null)
                Sources = new Collection<DVBLinkSource>();

            DVBLinkElement sourcesElement = DVBLinkBaseNode.FindElement(ConfigurationNode, new string[] { "dvblink_configuration", "sources" });
            if (sourcesElement == null || sourcesElement.Elements == null)
                return (false);

            foreach (DVBLinkElement sourceElement in sourcesElement.Elements)
            {
                DVBLinkSource newSource = new DVBLinkSource();
                bool loaded = newSource.Load(sourceElement);
                if (loaded)
                    Sources.Add(newSource);
            }

            return (true);
        }

        internal DVBLinkSource FindSource(string name)
        {
            if (Sources == null)
                return (null);

            string normalizedName = DVBLinkSource.NormalizeName(name);

            foreach (DVBLinkSource source in Sources)
            {
                if (source.ColorID != null)
                {
                    if (source.NormalizedName.ToLowerInvariant() == normalizedName.ToLowerInvariant())
                        return (source);
                }
                else
                {
                    if (source.Instances != null)
                    {
                        foreach (DVBLinkSourceInstance instance in source.Instances)
                        {
                            if (instance.NormalizedName.ToLowerInvariant() == normalizedName.ToLowerInvariant())
                                return (source);
                        }
                    }
                }
            }

            return (null);
        }

        internal void Clear() 
        {
            if (Sources == null)
                return;

            foreach (DVBLinkSource source in Sources)
            {
                if (source.HeadEnds != null)
                {
                    foreach (DVBLinkHeadEnd headEnd in source.HeadEnds)
                    {
                        if (headEnd.Channels != null)
                            headEnd.Channels.Clear();
                    }
                }
            }
        }
    }
}
