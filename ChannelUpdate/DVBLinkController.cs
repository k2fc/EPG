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
using System.Reflection;
using System.ServiceProcess;
using System.Text;

using DomainObjects;

namespace ChannelUpdate
{
    /// <summary>
    /// The class that controls the DVBLink update process.
    /// </summary>
    public class DVBLinkController
    {
        /// <summary>
        /// Get the full assembly version number.
        /// </summary>
        public static string AssemblyVersion
        {
            get
            {
                System.Version version = Assembly.GetExecutingAssembly().GetName().Version;
                return (version.Major + "." + version.Minor + "." + version.Build + "." + version.Revision);
            }
        }

        /// <summary>
        /// Get or set the maximum frequency field.
        /// </summary>
        public static int MaxFrequency
        {
            get
            {
                if (configuration == null)
                    return (-1);

                return (configuration.MaxFrequency);                
            }
            set
            {
                if (configuration == null)
                    return;

                stopDVBLinkServer();

                configuration.MaxFrequency = value;
                configurationUpdated = true;
            }
        }

        private static DVBLinkSettings settings;
        private static DVBLinkConfiguration configuration;
        private static DVBLinkChannelStorage channelStorage;
        private static DVBLinkTVSourceSettings sourceSettings;

        private static ServiceController serviceController;
        private static bool configurationUpdated;

        private static Collection<TVStation> autoExcludedChannels;

        /// <summary>
        /// Load the DVBLink configuration files.
        /// </summary>
        /// <returns>True if the load succeeds; false otherwise.</returns>
        public static bool Load()
        {
            if (settings != null)
            {
                Logger.Instance.Write("DVBLink xml files already loaded");
                return (true);
            }

            Logger.Instance.Write("Loading DVBLink xml files");

            settings = new DVBLinkSettings();
            bool settingsLoaded = settings.Load();
            if (!settingsLoaded)
            {
                Logger.Instance.Write("DVBLogic settings not loaded - load abandoned");
                return (false);
            }

            configuration = new DVBLinkConfiguration();
            bool configurationLoaded = configuration.Load(settings.ConfigurationNode.InstallPath);
            if (!configurationLoaded)
            {
                Logger.Instance.Write("DVBLogic configuration not loaded - load abandoned");
                return (false);
            }

            channelStorage = new DVBLinkChannelStorage();
            bool channelStorageLoaded = channelStorage.Load(settings.ConfigurationNode.InstallPath);
            if (!channelStorageLoaded)
            {
                Logger.Instance.Write("DVBLogic channel storage not loaded - load abandoned");
                return (false);
            }

            sourceSettings = new DVBLinkTVSourceSettings();
            bool sourceSettingsLoaded = sourceSettings.Load(settings.ConfigurationNode.InstallPath);
            if (!sourceSettingsLoaded)
            {
                Logger.Instance.Write("DVBLogic TVSource settings not loaded - load abandoned");
                return (false);
            }

            if (sourceSettings.Settings == null)
            {
                Logger.Instance.Write("No TVSource settings files available - load abandoned");
                return (false);
            }

            Logger.Instance.Write("Creating source objects");
            configuration.LoadSources();
            Logger.Instance.Write("Created " + configuration.Sources.Count + " source objects");

            Logger.Instance.Write("Creating head end and physical channel objects from " + sourceSettings.Settings.Count + " TVSource settings files");
            foreach (DVBLinkTVSourceSetting setting in sourceSettings.Settings)
            {
                DVBLinkSource source = configuration.FindSource(setting.Source);
                if (source != null)
                {
                    source.LoadHeadEnds(setting.SettingsNode);
                    Logger.Instance.Write("Created " + source.HeadEnds.Count + " head end objects for source " + source.NormalizedName);

                    foreach (DVBLinkHeadEnd headEnd in source.HeadEnds)
                    {
                        headEnd.LoadChannels();
                        Logger.Instance.Write("Created " + headEnd.Channels.Count + " physical channel objects for head end " + headEnd.ChannelSourceName);
                    }
                }
            }
            
            Logger.Instance.Write("Creating logical channel objects");
            DVBLinkLogicalChannel.LoadChannels(channelStorage.ChannelInfoNode);
            Logger.Instance.Write("Created " + DVBLinkLogicalChannel.Channels.Count + " logical channel objects");

            Logger.Instance.Write("Creating epg map objects");
            DVBLinkEPGMapChannel.LoadChannels(channelStorage.ChannelInfoNode);
            Logger.Instance.Write("Created " + DVBLinkEPGMapChannel.Channels.Count + " epg map objects");

            if (DVBLinkSource.SourceVersion.CompareTo("45") < 0)
                Logger.Instance.Write("DVBLink file formats are pre v4.5");
            else
                Logger.Instance.Write("DVBLink file formats are v4.5 or later");

            return (true);
        }

        /// <summary>
        /// Unload the DVBLink configuration files.
        /// </summary>
        /// <returns>True if the unload succeeds; false otherwise.</returns>
        public static bool Unload()
        {
            bool scanIntervalUpdated = configuration.UpdateScanInterval();
            if (scanIntervalUpdated)
                configurationUpdated = true;

            if (autoExcludedChannels != null && autoExcludedChannels.Count != 0)
            {
                foreach (TVStation excludedChannel in autoExcludedChannels)
                    Logger.Instance.Write("Auto excluded channel: " + excludedChannel.Name);
                RunParameters.Instance.Save(CommandLine.IniFileName);
            }

            if (DVBLinkPhysicalChannel.ChannelsAdded == 0 &&
                DVBLinkPhysicalChannel.ChannelsChanged == 0 &&
                DVBLinkPhysicalChannel.ChannelsDeleted == 0 &&
                DVBLinkLogicalChannel.ChannelsAdded == 0 &&
                DVBLinkLogicalChannel.ChannelsChanged == 0 &&
                DVBLinkLogicalChannel.ChannelsDeleted == 0 &&
                !configurationUpdated)
            {
                Logger.Instance.Write("No updates detected - DVBLink xml files not updated");
                return (true);
            }

            Logger.Instance.Write("Unloading DVBLink xml files");

            if (serviceController == null)
                stopDVBLinkServer();

            if (configurationUpdated)
            {
                bool configurationUnloaded = configuration.Unload(settings.ConfigurationNode.InstallPath);
                if (!configurationUnloaded)
                {
                    Logger.Instance.Write("DVBLogic configuration not unloaded - channel update abandoned");
                    return (false);
                }
            }

            if (DVBLinkPhysicalChannel.ChannelsAdded == 0 &&
                DVBLinkPhysicalChannel.ChannelsChanged == 0 &&
                DVBLinkPhysicalChannel.ChannelsDeleted == 0 &&
                DVBLinkLogicalChannel.ChannelsAdded == 0 &&
                DVBLinkLogicalChannel.ChannelsChanged == 0 &&
                DVBLinkLogicalChannel.ChannelsDeleted == 0)
            {
                startDVBLinkServer();

                Logger.Instance.Write("No chanel updates detected - DVBLink channel xml files not updated");
                return (true);
            }

            bool channelStorageUnloaded = channelStorage.Unload(settings.ConfigurationNode.InstallPath);
            if (!channelStorageUnloaded)
            {
                Logger.Instance.Write("DVBLogic channel storage not unloaded - channel update abandoned");
                return (false);
            }

            bool sourceSettingsUnloaded = sourceSettings.Unload();
            if (!sourceSettingsUnloaded)
            {
                Logger.Instance.Write("DVBLogic TVSource settings not unloaded - channel update abandoned");
                return (false);
            }

            startDVBLinkServer();

            Logger.Instance.Write("Physical channel stats: Total = " + configuration.ChannelCount + " Added = " + DVBLinkPhysicalChannel.ChannelsAdded +
                " Changed = " + DVBLinkPhysicalChannel.ChannelsChanged + " Deleted = " + DVBLinkPhysicalChannel.ChannelsDeleted);
            Logger.Instance.Write("Logical channel stats: Total = " + DVBLinkLogicalChannel.Channels.Count + " Added = " + DVBLinkLogicalChannel.ChannelsAdded +
                " Changed = " + DVBLinkLogicalChannel.ChannelsChanged + " Deleted = " + DVBLinkLogicalChannel.ChannelsDeleted);

            return (true);
        }

        private static void stopDVBLinkServer()
        {
            if (serviceController != null)
                return;

            try
            {
                serviceController = new ServiceController("dvblink_server");
                Logger.Instance.Write("DVBLink server process status is " + serviceController.Status);

                if (serviceController.Status != ServiceControllerStatus.Stopped && serviceController.Status != ServiceControllerStatus.StopPending)
                {
                    Logger.Instance.Write("Stopping the DVBLink server process");
                    serviceController.Stop();

                    serviceController.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 30));
                }
            }
            catch (System.ServiceProcess.TimeoutException)
            {
                Logger.Instance.Write("<E> The DVBLink server process did not stop after 30 seconds");
            }
            catch (Exception e)
            {
                Logger.Instance.Write("<E> An exception of type " + e.GetType().Name + " has occurred");
                Logger.Instance.Write("<E> " + e.Message);
                Logger.Instance.Write("<E> Failed to stop the DVBLink server process");
            }
        }

        private static void startDVBLinkServer()
        {
            if (serviceController == null)
                return;

            try
            {
                Logger.Instance.Write("Restarting the DVBLink server process");
                serviceController.Start();
            }
            catch (Exception e)
            {
                Logger.Instance.Write("<E> An exception of type " + e.GetType().Name + " has occurred");
                Logger.Instance.Write("<E> Failed to restart the DVBLink server process");
            }
        }

        /// <summary>
        /// Process updates.
        /// </summary>
        /// <param name="frequency">The frequency being updated.</param>
        /// <returns>True if the update succeeds; false otherwise.</returns>
        public static bool Process(TuningFrequency frequency)
        {
            Logger.Instance.Write("Processing stations for frequency " + frequency);

            if (frequency.Stations == null || frequency.Stations.Count == 0)
            {
                Logger.Instance.Write("No stations to process");
                return (true);
            }

            if (autoExcludedChannels == null)
                autoExcludedChannels = new Collection<TVStation>();

            int processed = 0;

            foreach (TVStation station in frequency.Stations)
            {
                if (station.Included)
                {
                    if (DebugEntry.IsDefined(DebugName.UpdateStation))
                        Logger.Instance.Write("Station " + station.FullDescription + " being processed");
                    processPhysicalChannel(frequency, station, autoExcludedChannels);
                    processed++;
                }
                else
                {
                    if (DebugEntry.IsDefined(DebugName.UpdateStation))
                        Logger.Instance.Write("Station " + station.FullDescription + " not processed");
                }
            }

            Logger.Instance.Write("Processed " + processed + " stations");

            if (configuration.Sources != null)
            {
                foreach (DVBLinkSource source in configuration.Sources)
                {
                    if (source.HeadEnds != null)
                    {
                        foreach (DVBLinkHeadEnd headEnd in source.HeadEnds)
                        {
                            if (checkOrbitalPosition(frequency, headEnd))
                            {
                                if (headEnd.Channels != null)
                                {
                                    Collection<DVBLinkPhysicalChannel> deletedChannels = new Collection<DVBLinkPhysicalChannel>();
                                    
                                    foreach (DVBLinkPhysicalChannel physicalChannel in headEnd.Channels)
                                    {
                                        if (physicalChannel.Freq == frequency.Frequency)
                                        {
                                            foreach (TVStation station in frequency.Stations)
                                            {
                                                if (station.ServiceID == physicalChannel.Sid && !station.Included)
                                                {
                                                    deletedChannels.Add(physicalChannel);
                                                    break;
                                                }
                                            }
                                        }
                                    }

                                    Logger.Instance.Write(deletedChannels.Count + " physical channels to be deleted from " + headEnd.FullDescription);

                                    foreach (DVBLinkPhysicalChannel deletedChannel in deletedChannels)
                                    {
                                        deletedChannel.Delete();
                                        headEnd.Channels.Remove(deletedChannel);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return (true);
        }

        private static void processPhysicalChannel(TuningFrequency frequency, TVStation station, Collection<TVStation> autoExcludedChannels)
        {
            foreach (DVBLinkSource source in configuration.Sources)
            {
                if (source.HeadEnds != null)
                {
                    foreach (DVBLinkHeadEnd headEnd in source.HeadEnds)
                    {
                        if (checkOrbitalPosition(frequency, headEnd))
                        {
                            DVBLinkPhysicalChannel physicalChannel = headEnd.FindChannel(frequency.Frequency,
                                station.OriginalNetworkID, station.TransportStreamID, station.ServiceID);

                            if (physicalChannel != null)
                            {
                                if (processChange(headEnd, physicalChannel, frequency, station))
                                {
                                    DVBLinkLogicalChannel logicalChannel = DVBLinkLogicalChannel.FindChannel(physicalChannel);
                                    if (logicalChannel != null)
                                        logicalChannel.Change(physicalChannel);
                                }
                            }
                            else
                            {
                                bool notExcluded = true;

                                if (!station.CreatedFromIni && RunParameters.Instance.ChannelExcludeNew)
                                    notExcluded = searchSourcesForChannel(station.OriginalNetworkID, station.TransportStreamID, station.ServiceID);

                                if (notExcluded)
                                {
                                    DVBLinkPhysicalChannel newChannel = processAdd(headEnd, frequency, station);
                                    if (newChannel != null)
                                    {
                                        headEnd.Channels.Add(newChannel);
                                        DVBLinkLogicalChannel.Add(newChannel, configuration);
                                    }
                                }
                                else
                                {
                                    station.ExcludedByUser = true;
                                    autoExcludedChannels.Add(station);
                                }
                            }
                        }
                    }
                }
            }
        }

        private static bool checkOrbitalPosition(TuningFrequency tuningFrequency, DVBLinkHeadEnd headEnd)
        {
            SatelliteFrequency satelliteFrequency = tuningFrequency as SatelliteFrequency;
            if (satelliteFrequency == null)
                return (true);

            Satellite satellite = satelliteFrequency.Provider as Satellite;
            if (satellite == null)
                return(true);

            string sourceID = satellite.Longitude.ToString("0000") + ".ini"; 

            return (sourceID == headEnd.ChannelSourceID);
        }

        private static bool processChange(DVBLinkHeadEnd headEnd, DVBLinkPhysicalChannel physicalChannel, TuningFrequency frequency, TVStation station)
        {
            Logger.Instance.Write("Processing change for channel " + station.FullDescription + " " + headEnd.FullDescription + " on " + frequency.Frequency); 
            return (physicalChannel.Change(headEnd, frequency, station));            
        }

        private static DVBLinkPhysicalChannel processAdd(DVBLinkHeadEnd headEnd, TuningFrequency frequency, TVStation station)
        {
            Logger.Instance.Write("Processing addition for channel " + station.FullDescription + " " + headEnd.FullDescription + " on " + frequency.Frequency);            
            return(DVBLinkPhysicalChannel.AddChannel(headEnd, frequency, station));            
        }

        private static bool searchSourcesForChannel(int originalNetworkId, int transportStreamId, int serviceId)
        {
            foreach (DVBLinkSource source in configuration.Sources)
            {
                if (source.HeadEnds != null)
                {
                    foreach (DVBLinkHeadEnd headEnd in source.HeadEnds)
                    {
                        foreach (DVBLinkPhysicalChannel physicalChannel in headEnd.Channels)
                        {
                            if (physicalChannel.Nid == originalNetworkId &&
                                physicalChannel.Tid == transportStreamId &&
                                physicalChannel.Sid == serviceId)
                                return (true);
                        }
                    }
                }
            }

            return(false);
        }

        /// <summary>
        /// Clear existing data.
        /// </summary>
        public static void ClearData()
        {
            Logger.Instance.Write("Clearing existing data");

            configuration.Clear();
            channelStorage.Clear();
            sourceSettings.Clear();

            if (DVBLinkLogicalChannel.Channels != null)
                DVBLinkLogicalChannel.Channels.Clear();
            if (DVBLinkEPGMapChannel.Channels != null)
                DVBLinkEPGMapChannel.Channels.Clear();

            MaxFrequency = DVBLinkConfiguration.MaxFrequencyInitialValue;

            Logger.Instance.Write("Existing data cleared");
        }

        /// <summary>
        /// Log the channel map.
        /// </summary>
        public static void LogChannelMap(bool logCAData)
        {
            Logger.Instance.WriteSeparator("DVBLink Channel Map");

            if (DVBLinkLogicalChannel.Channels == null || DVBLinkLogicalChannel.Channels.Count == 0)
            {
                Logger.Instance.Write("No channels present");
                return;
            }

            foreach (DVBLinkLogicalChannel logicalChannel in DVBLinkLogicalChannel.GetChannelsNumberOrder())
            {
                string newComment = logicalChannel.New ? " ** NEW ** " : string.Empty;
                string changedComment = logicalChannel.Changed ? " ** CHANGED ** " : string.Empty;

                if (logicalChannel.SubNumber < 1)
                    Logger.Instance.Write(logicalChannel.Number + " " + logicalChannel.Name + newComment + changedComment);
                else
                    Logger.Instance.Write(logicalChannel.Number + ":" + logicalChannel.SubNumber + " " + logicalChannel.Name + newComment + changedComment);

                if (logicalChannel.PhysicalChannelLinks != null)
                {
                    foreach (DVBLinkPhysicalChannelLink link in logicalChannel.PhysicalChannelLinks)
                    {
                        DVBLinkPhysicalChannel physicalChannel = DVBLinkController.FindChannel(link);
                        if (physicalChannel == null)
                            Logger.Instance.Write("    Physical channel missing: " + link.Id);
                        else
                        {
                            newComment = physicalChannel.New ? " ** NEW ** " : string.Empty;
                            changedComment = physicalChannel.Changed ? " ** CHANGED ** " : string.Empty;
                            
                            StringBuilder caComment = new StringBuilder(); 
                            
                            if (logCAData)
                            {
                                if (physicalChannel.CA != null)
                                {
                                    caComment.Append(" CA data:");
                                    
                                    foreach (DVBLinkCAEntry caEntry in physicalChannel.CA)
                                    {
                                        caComment.Append(" ");
                                        caComment.Append(caEntry.Pid + ":" + caEntry.SystemID);
                                    }
                                }
                            }

                            Logger.Instance.Write("    " + physicalChannel.FullDescription + caComment + newComment + changedComment);
                        }
                    }
                }

            }
        }

        internal static DVBLinkPhysicalChannel FindChannel(DVBLinkPhysicalChannelLink physicalChannelLink)
        {
            if (configuration.Sources == null)
                return (null);

            foreach (DVBLinkSource source in configuration.Sources)
            {
                if (source.HeadEnds != null)
                {
                    foreach (DVBLinkHeadEnd headEnd in source.HeadEnds)
                    {
                        if (headEnd.Channels != null)
                        {
                            foreach (DVBLinkPhysicalChannel channel in headEnd.Channels)
                            {
                                if (channel.FullID == physicalChannelLink.Id)
                                    return (channel);
                            }
                        }
                    }
                }
            }

            return (null);
        }

        internal static int RoundFrequency(int frequency)
        {
            int divFrequency = frequency / 1000;
            
            /*int remFrequency = frequency % 1000;
            
            if (remFrequency > 499)
                divFrequency++;*/
            
            return(divFrequency * 1000);  
        }
    }
}
