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
using System.Text;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Security.Principal;

using Microsoft.Win32;

namespace DomainObjects
{
    /// <summary>
    /// The class that processes the initialization file.
    /// </summary>
    public class RunParameters
    {
        /// <summary>
        /// Return true if running under any version of Windows.
        /// </summary>
        public static bool IsWindows
        {
            get
            {
                return (Environment.OSVersion.Platform == PlatformID.Win32NT ||
                    Environment.OSVersion.Platform == PlatformID.Win32S ||
                    Environment.OSVersion.Platform == PlatformID.Win32Windows ||
                    Environment.OSVersion.Platform == PlatformID.WinCE);

            }
        }

        /// <summary>
        /// Return true if running under 64-bit Windows
        /// </summary>
        public static bool Is64Bit { get { return (Environment.Is64BitOperatingSystem); } }

        /// <summary>
        /// Return true if running under Mono or the command line requests compatability.
        /// </summary>
        public static bool IsMono 
        { 
            get 
            {
                Type monoType = Type.GetType("Mono.Runtime");
                if (monoType != null)
                    return (true);

                return (CommandLine.MonoCompatible);
            } 
        }

        /// <summary>
        /// Get the Mono version number.
        /// </summary>
        public static string MonoVersion
        {
            get
            {
                Type type = Type.GetType("Mono.Runtime");
                if (type != null)
                {
                    MethodInfo displayName = type.GetMethod("GetDisplayName", BindingFlags.NonPublic | BindingFlags.Static);
                    if (displayName != null)
                        return ((string)displayName.Invoke(null, null));
                    else
                        return ("Version number not found");
                }
                else
                    return ("Not Mono environment");
            }
        }
        
        /// <summary>
        /// Return true if running under Wine.
        /// </summary>
        public static bool IsWine
        {
            get
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\Wine\Wine\Config", false);
                if (key != null)
                    return (true);

                return (CommandLine.WineCompatible);
            }
        }

        /// <summary>
        /// Get the system version number.
        /// </summary>
        public static string SystemVersion
        {
            get
            {
                System.Version version = Assembly.GetExecutingAssembly().GetName().Version;
                return (version.Major + "." + version.Minor + " FP " + Fixpack);
            }
        }

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
        /// Get the current fixpack number.
        /// </summary>
        public static string Fixpack { get { return ("11"); } }

        /// <summary>
        /// Get the privilege level.
        /// </summary>
        public static string Role
        {
            get
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                if (principal.IsInRole(WindowsBuiltInRole.Administrator))
                    return ("Administrator");
                if (principal.IsInRole(WindowsBuiltInRole.User))
                    return ("User");
                if (principal.IsInRole(WindowsBuiltInRole.Guest))
                    return ("Guest");
                return ("Other");
            }
        }

        /// <summary>
        /// Get or set the current run parameters instance.
        /// </summary>
        public static RunParameters Instance
        {
            get
            {
                if (instance == null)
                    instance = new RunParameters(ParameterSet.Collector, RunType.Collection);
                return (instance);
            }
            set { instance = value; }
        }

        /// <summary>
        /// Get or set the application base directory.
        /// </summary>
        public static string BaseDirectory
        {
            get { return (baseDirectory); }
            set { baseDirectory = value; }
        }

        /// <summary>
        /// Get the application data directory.
        /// </summary>
        public static string DataDirectory
        {
            get
            {
                if (applicationDirectory == null)
                {
                    applicationDirectory = Environment.GetEnvironmentVariable("EPGC_DATA_DIR", EnvironmentVariableTarget.Machine);
                    if (string.IsNullOrWhiteSpace(applicationDirectory))
                    {
                        applicationDirectory = Environment.GetEnvironmentVariable("EPGC_DATA_DIR", EnvironmentVariableTarget.Process);
                        if (string.IsNullOrWhiteSpace(applicationDirectory))
                            applicationDirectory = Environment.GetEnvironmentVariable("EPGC_DATA_DIR", EnvironmentVariableTarget.User);
                    }

                    if (string.IsNullOrWhiteSpace(applicationDirectory))
                    {
                        WindowsIdentity identity = WindowsIdentity.GetCurrent();
                        WindowsPrincipal principal = new WindowsPrincipal(identity);
                        if (principal.IsInRole(WindowsBuiltInRole.Administrator))
                            applicationDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), Path.Combine("Geekzone", "EPG Collector"));
                        else
                            applicationDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), Path.Combine("Geekzone", "EPG Collector"));
                    }

                    if (!Directory.Exists(applicationDirectory))
                        Directory.CreateDirectory(applicationDirectory);                    
                }
                return (applicationDirectory);
            }
        }

        /// <summary>
        /// Get the application configuration directory.
        /// </summary>
        public static string ConfigDirectory { get { return (Path.Combine(baseDirectory, "Configuration")); } }

        /// <summary>
        /// Get othe base path for lookup images.
        /// </summary>
        public static string ImagePath
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(Instance.LookupImagePath))
                    return (Instance.LookupImagePath.Trim());
                else
                    return (Path.Combine(RunParameters.DataDirectory, "Images"));
            }
        }

        /// <summary>
        /// Get the full name of the stream logging file. 
        /// </summary>
        public static string StreamLogFileName { get { return (Path.Combine(RunParameters.DataDirectory, "EPG Collector Stream.log")); } }

        /// <summary>
        /// Get the list of Sat>IP unique identifiers.
        /// </summary>
        public Collection<string> SatIpUniqueIdentifiers
        {
            get
            {
                if (satIpUniqueIdentifiers == null)
                    satIpUniqueIdentifiers = new Collection<string>();
                return (satIpUniqueIdentifiers);
            }
        }

        /// <summary>
        /// Get or set the output file name.
        /// </summary>
        /// <remarks>
        /// The default name will be returned if it is not overridden by the initialization file.
        /// </remarks>
        public string OutputFileName
        {
            get { return (outputFileName); }
            set { outputFileName = value; }
        }

        /// <summary>
        /// Get the INI file name.
        /// </summary>
        /// <remarks>
        /// The default name will be returned if it is not overridden by the initialization file.
        /// </remarks>
        public string IniFileName { get { return (iniFileName); } }

        /// <summary>
        /// Get or set the timeout for acquiring data for a frequency.
        /// </summary>
        /// <remarks>
        /// The default of 300 seconds will be returned if it is not overridden by the initialization file.
        /// </remarks>
        public TimeSpan FrequencyTimeout
        {
            get { return (frequencyTimeout); }
            set { frequencyTimeout = value; }
        }

        /// <summary>
        /// Get or set the timeout for acquiring a signal lock and receiving station information.
        /// </summary>
        /// <remarks>
        /// The default of 10 seconds will be returned if it is not overridden by the initialization file.
        /// </remarks>
        public TimeSpan LockTimeout
        {
            get { return (lockTimeout); }
            set { lockTimeout = value; }
        }

        /// <summary>
        /// Get or set the number of repeats for collections that cannot determine data complete.
        /// </summary>
        /// <remarks>
        /// The default of 5 will be returned if it is not overridden by the initialization file.
        /// </remarks>
        public int Repeats
        {
            get { return (repeats); }
            set { repeats = value; }
        }

        /// <summary>
        /// Get or set the size of the sample buffer in megabytes.
        /// </summary>
        /// <remarks>
        /// The default of 50 will be returned if it is not overridden by the initialization file.
        /// </remarks>
        public int BufferSize
        {
            get { return (bufferSize); }
            set { bufferSize = value; }
        }

        /// <summary>
        /// Get or set the number of times buffer is filled for collections that cannot determine data complete.
        /// </summary>
        /// <remarks>
        /// The default of 1 will be returned if it is not overridden by the initialization file.
        /// </remarks>
        public int BufferFills
        {
            get { return (bufferFills); }
            set { bufferFills = value; }
        }

        /// <summary>
        /// Get or set the timezone.
        /// </summary>
        public TimeSpan TimeZone
        {
            get { return (timeZone); }
            set { timeZone = value; }
        }

        /// <summary>
        /// Get or set the next timezone.
        /// </summary>
        public TimeSpan NextTimeZone
        {
            get { return (nextTimeZone); }
            set { nextTimeZone = value; }
        }

        /// <summary>
        /// Get or set the date/time of the next timezone change.
        /// </summary>
        public DateTime NextTimeZoneChange
        {
            get { return (nextTimeZoneChange); }
            set { nextTimeZoneChange = value; }
        }

        /// <summary>
        /// Get or set an indication of whether the timezone has been set.
        /// </summary>
        public bool TimeZoneSet
        {
            get { return (timeZoneSet); }
            set { timeZoneSet = value; }
        }

        /// <summary>
        /// Get the collection of options.
        /// </summary>
        public Collection<OptionEntry> Options { get { return (options); } }

        /// <summary>
        /// Get the collection of trace ID's.
        /// </summary>
        public Collection<TraceEntry> TraceIDs { get { return (traceIDs); } }

        /// <summary>
        /// Get the collection of debug ID's.
        /// </summary>
        public Collection<DebugEntry> DebugIDs { get { return (debugIDs); } }

        /// <summary>
        /// Get or set the maximum service ID.
        /// </summary>
        public int MaxService
        {
            get { return (maxService); }
            set { maxService = value; }
        }

        /// <summary>
        /// Get or set the WMC import name used in the MXF file.
        /// </summary>
        public string WMCImportName
        {
            get { return (wmcImportName); }
            set { wmcImportName = value; }
        }

        /// <summary>
        /// Get or set the flag for movie lookups.
        /// </summary>
        public bool MovieLookupEnabled
        {
            get { return (movieLookupEnabled); }
            set { movieLookupEnabled = value; }
        }

        /// <summary>
        /// Get or set the flag for downloading movie thumbnail.
        /// </summary>
        public LookupImageType DownloadMovieThumbnail
        {
            get { return (downloadMovieThumbnail); }
            set { downloadMovieThumbnail = value; }
        }

        /// <summary>
        /// Get or set the minimum length of time for a program to be considered a movie.
        /// </summary>
        public int MovieLowTime
        {
            get { return (movieLowTime); }
            set { movieLowTime = value; }
        }

        /// <summary>
        /// Get or set the maximum length of time for a program to be considered a movie.
        /// </summary>
        public int MovieHighTime
        {
            get { return (movieHighTime); }
            set { movieHighTime = value; }
        }

        /// <summary>
        /// Get or set the flag for TV lookups.
        /// </summary>
        public bool TVLookupEnabled
        {
            get { return (tvLookupEnabled); }
            set { tvLookupEnabled = value; }
        }

        /// <summary>
        /// Get or set the flag for downloading TV thumbnail.
        /// </summary>
        public LookupImageType DownloadTVThumbnail
        {
            get { return (downloadTVThumbnail); }
            set { downloadTVThumbnail = value; }
        }

        /// <summary>
        /// Get or set the time limit (minutes) for lookup processing.
        /// </summary>
        public int LookupTimeLimit
        {
            get { return (lookupTimeLimit); }
            set { lookupTimeLimit = value; }
        }

        /// <summary>
        /// Get or set the error limit) for lookup processing.
        /// </summary>
        public int LookupErrorLimit
        {
            get { return (lookupErrorLimit); }
            set { lookupErrorLimit = value; }
        }

        /// <summary>
        /// Get or set the type of matching for lookups.
        /// </summary>
        public MatchMethod LookupMatching
        {
            get { return (lookupMatching); }
            set { lookupMatching = value; }
        }

        /// <summary>
        /// Get or set the flag for lookup reload.
        /// </summary>
        public bool LookupReload
        {
            get { return (lookupReload); }
            set { lookupReload = value; }
        }

        /// <summary>
        /// Get or set the flag for always looking up not found entries.
        /// </summary>
        public bool LookupNotFound
        {
            get { return (lookupNotFound); }
            set { lookupNotFound = value; }
        }

        /// <summary>
        /// Get or set the flag for ignoring boradcast categories.
        /// </summary>
        public bool LookupIgnoreCategories
        {
            get { return (lookupIgnoreCategories); }
            set { lookupIgnoreCategories = value; }
        }

        /// <summary>
        /// Get or set the flag for processing as TV series if not movie.
        /// </summary>
        public bool LookupProcessAsTVSeries
        {
            get { return (lookupProcessAsTVSeries); }
            set { lookupProcessAsTVSeries = value; }
        }

        /// <summary>
        /// Get or set the user entered list of phrases to be ignored in the lookup title.
        /// </summary>
        public Collection<string> LookupIgnoredPhrases
        {
            get { return (lookupIgnoredPhrases); }
            set { lookupIgnoredPhrases = value; }
        }

        /// <summary>
        /// Get or set the separator  for ignored phrases in the lookup title.
        /// </summary>
        public string LookupIgnoredPhraseSeparator
        {
            get { return (lookupIgnoredPhraseSeparator); }
            set { lookupIgnoredPhraseSeparator = value; }
        }

        /// <summary>
        /// Get or set the user entered list of phrases to be used to identify a movie.
        /// </summary>
        public Collection<string> LookupMoviePhrases
        {
            get { return (lookupMoviePhrases); }
            set { lookupMoviePhrases = value; }
        }

        /// <summary>
        /// Get or set the separator  for movie phrases in the lookup title.
        /// </summary>
        public string MoviePhraseSeparator
        {
            get { return (moviePhraseSeparator); }
            set { moviePhraseSeparator = value; }
        }

        /// <summary>
        /// Get or set the base path for lookup images.
        /// </summary>
        public string LookupImagePath
        {
            get { return (lookupImagePath); }
            set { lookupImagePath = value; }
        }

        /// <summary>
        /// Get or set the path for XMLTV image tags.
        /// </summary>
        public string LookupXmltvImageTagPath
        {
            get { return (lookupXmltvImageTagPath); }
            set { lookupXmltvImageTagPath = value; }
        }

        /// <summary>
        /// Get the list of programme names that are not movies.
        /// </summary>
        public Collection<string> LookupNotMovie
        {
            get { return (lookupNotMovie); }
            set { lookupNotMovie = value; } 
        }

        /// <summary>
        /// Get or set the output language code for lookups.
        /// </summary>
        public string LookupOutputLanguageCode
        {
            get { return (lookupOutputLanguageCode); }
            set { lookupOutputLanguageCode = value; }
        }

        /// <summary>
        /// Get or set the flag for naming images with the programme title.
        /// </summary>
        public bool LookupImageNameTitle
        {
            get { return (lookupImageNameTitle); }
            set { lookupImageNameTitle = value; }
        }

        /// <summary>
        /// Get or set the flag for storing images in the base directory.
        /// </summary>
        public bool LookupImagesInBase
        {
            get { return (lookupImagesInBase); }
            set { lookupImagesInBase = value; }
        }

        /// <summary>
        /// Get or set the flag for channel updates.
        /// </summary>
        public bool ChannelUpdateEnabled
        {
            get { return (channelUpdateEnabled); }
            set { channelUpdateEnabled = value; }
        }

        /// <summary>
        /// Get or set the type of channel merging.
        /// </summary>
        public ChannelMergeMethod ChannelMergeMethod
        {
            get { return (channelMergeMethod); }
            set { channelMergeMethod = value; }
        }

        /// <summary>
        /// Get or set the type of channel EPG.
        /// </summary>
        public ChannelEPGScanner ChannelEPGScanner
        {
            get { return (channelEPGScanner); }
            set { channelEPGScanner = value; }
        }

        /// <summary>
        /// Get or set the channel update child lock for additions.
        /// </summary>
        public bool ChannelChildLock
        {
            get { return (channelChildLock); }
            set { channelChildLock = value; }
        }

        /// <summary>
        /// Get or set the flag for not logging the network map.
        /// </summary>
        public bool ChannelLogNetworkMap
        {
            get { return (channelLogNetworkMap); }
            set { channelLogNetworkMap = value; }
        }

        /// <summary>
        /// Get or set the EPG scan interval.
        /// </summary>
        public int ChannelEPGScanInterval
        {
            get { return (channelEPGScanInterval); }
            set { channelEPGScanInterval = value; }
        }

        /// <summary>
        /// Get or set the reload data flag.
        /// </summary>
        public bool ChannelReloadData
        {
            get { return (channelReloadData); }
            set { channelReloadData = value; }
        }

        /// <summary>
        /// Get or set the channel number update flag.
        /// </summary>
        public bool ChannelUpdateNumber
        {
            get { return (channelUpdateNumber); }
            set { channelUpdateNumber = value; }
        }

        /// <summary>
        /// Get or set the auto exclude channels flag.
        /// </summary>
        public bool ChannelExcludeNew
        {
            get { return (channelExcludeNew); }
            set { channelExcludeNew = value; }
        }

        /// <summary>
        /// Get or set the list of import files.
        /// </summary>
        public Collection<ImportFileSpec> ImportFiles
        {
            get { return (importFiles); }
            set { importFiles = value; }
        }

        /// <summary>
        /// Get or set the import file channel changes.
        /// </summary>
        public Collection<ImportChannelChange> ImportChannelChanges
        {
            get { return (importChannelChanges); }
            set { importChannelChanges = value; }
        }

        /// <summary>
        /// Get or set the text edit list.
        /// </summary>
        public Collection<EditSpec> EditSpecs
        {
            get { return (editSpecs); }
            set { editSpecs = value; }
        }

        /// <summary>
        /// Get or set the DVBViewer IP address list.
        /// </summary>
        public string DVBViewerIPAddress
        {
            get { return (dvbviewerIPAddress); }
            set { dvbviewerIPAddress = value; }
        }

        /// <summary>
        /// Get or set the BladeRunner file name.
        /// </summary>
        public string BladeRunnerFileName
        {
            get { return (bladeRunnerFileName); }
            set { bladeRunnerFileName = value; }
        }

        /// <summary>
        /// Get or set the Area/Region file name.
        /// </summary>
        public string AreaRegionFileName
        {
            get { return (areaRegionFileName); }
            set { areaRegionFileName = value; }
        }

        /// <summary>
        /// Get or set the SageTV file name.
        /// </summary>
        public string SageTVFileName
        {
            get { return (sageTVFileName); }
            set { sageTVFileName = value; }
        }

        /// <summary>
        /// Get or set the SageTV satellite number.
        /// </summary>
        public int SageTVSatelliteNumber
        {
            get { return (sageTVSatelliteNumber); }
            set { sageTVSatelliteNumber = value; }
        }

        /// <summary>
        /// Get an indication of whether the output file name has been set.
        /// </summary>
        public bool OutputFileSet { get { return (!string.IsNullOrWhiteSpace(outputFileName)); } }

        /// <summary>
        /// Get or set the flag for an abandon run request.
        /// </summary>
        public bool AbandonRequested
        {
            get { return (abandonRequested); }
            set { abandonRequested = value; }
        }

        /// <summary>
        /// Return true if the run parameters indicate channel data is needed; false otherwise.
        /// </summary>
        public bool ChannelDataNeeded
        {
            get
            {
                if (CurrentFrequency.AdvancedRunParamters.ChannelBouquet != -1 ||
                    OptionEntry.IsDefined(this.Options, OptionName.UseChannelId) ||
                    OptionEntry.IsDefined(this.Options, OptionName.UseLcn) ||
                    OptionEntry.IsDefined(this.Options, OptionName.CreateBrChannels) ||
                    OptionEntry.IsDefined(this.Options, OptionName.CreateSageTvFrq) ||
                    OptionEntry.IsDefined(this.Options, OptionName.TcRelevantOnly) ||
                    RunParameters.Instance.ChannelUpdateNumber)
                    return (true);
                else
                    return (false);
            }
        }

        /// <summary>
        /// Return true if the run parameters indicate network data is needed; false otherwise.
        /// </summary>
        public bool NetworkDataNeeded
        {
            get
            {
                if (DebugEntry.IsDefined(DebugName.CreateSatIni) ||
                    DebugEntry.IsDefined(DebugName.LogEpgLinkage) ||
                    this.ChannelUpdateEnabled ||
                    ChannelDataNeeded)
                    return (true);
                else
                    return (false);
            }
        }

        /// <summary>
        /// Return true if parameters specify import to DvbViewer; false otherwise.
        /// </summary>
        public bool ImportingToDvbViewer
        {
            get
            {
                return (OptionEntry.IsDefined(this.Options, OptionName.DvbViewerImport) || OptionEntry.IsDefined(this.Options, OptionName.DvbViewerRecSvcImport));
            }
        }

        /// <summary>
        /// Return true if the parameter set if for the plugin.
        /// </summary>
        public bool PluginParameters { get { return (pluginParameters); } }

        /// <summary>
        /// Get the collection of exclusions.
        /// </summary>
        public Collection<RepeatExclusion> Exclusions
        {
            get 
            {
                if (exclusions == null)
                    exclusions = new Collection<RepeatExclusion>();
                return (exclusions); 
            }
        }

        /// <summary>
        /// Get the phrases to ignore in program titles and descriptions.
        /// </summary>
        public Collection<string> PhrasesToIgnore
        {
            get 
            {
                if (phrasesToIgnore == null)
                    phrasesToIgnore = new Collection<string>();
                return (phrasesToIgnore); 
            }            
        }

        /// <summary>
        /// Get the collection of channel filters.
        /// </summary>
        public Collection<ChannelFilterEntry> ChannelFilters
        {
            get 
            {
                if (channelFilters == null)
                    channelFilters = new Collection<ChannelFilterEntry>();
                return (channelFilters); 
            } 
        }

        /// <summary>
        /// Get the collection of time offset channels.
        /// </summary>
        public Collection<TimeOffsetChannel> TimeOffsetChannels
        {
            get 
            {
                if (timeOffsetChannels == null)
                    timeOffsetChannels = new Collection<TimeOffsetChannel>();
                return (timeOffsetChannels); 
            }
            
        }

        /// <summary>
        /// Get or set the collection of frequencies defined for this run.
        /// </summary>       
        public Collection<TuningFrequency> FrequencyCollection
        {
            get 
            {
                if (frequencyCollection == null)
                    frequencyCollection = new Collection<TuningFrequency>();
                return (frequencyCollection); 
            }
        }

        /// <summary>
        /// Get the collection of stations processed in this run.
        /// </summary>
        public Collection<TVStation> StationCollection
        {
            get
            {
                if (stationCollection == null)
                    stationCollection = new Collection<TVStation>();
                return (stationCollection);
            }
        }

        /// <summary>
        /// Get or set the current tuning frequency.
        /// </summary>
        public TuningFrequency CurrentFrequency 
        {
            get { return (currentFrequency); }
            set { currentFrequency = value; } 
        }

        /// <summary>
        /// Get the last error message.
        /// </summary>
        public string LastError { get { return (lastError); } }

        private static RunParameters instance;
        private bool pluginParameters;

        private static string baseDirectory;
        private static string applicationDirectory;

        private ParameterSet parameterSet = ParameterSet.Collector;
        private RunType runType = RunType.Collection;

        private Collection<SelectedTuner> selectedTuners = new Collection<SelectedTuner>();
        private Collection<string> satIpUniqueIdentifiers;
        private string outputFileName;
        private string iniFileName = "";

        private string bladeRunnerFileName;
        private string areaRegionFileName;
        private string sageTVFileName;

        private TimeSpan frequencyTimeout = new TimeSpan(0, 5, 0);
        private TimeSpan lockTimeout = new TimeSpan(0, 0, 10);
        private int repeats = 5;
        private int bufferSize = 50;
        private int bufferFills = 1;

        private string byteConvertTable;
        private string eitCarousel;
        private int epgDays = -1;
        private int channelBouquet = -1;
        private int channelRegion = -1;        
        private string countryCode;
        private string characterSet;
        private string inputLanguage;
        private int region;
        private int sdtPid = -1;
        private int eitPid = -1;
        private int[] mhw1Pids;
        private int[] mhw2Pids;
        private int dishNetworkPid = -1;

        private TimeSpan timeZone;
        private TimeSpan nextTimeZone;
        private DateTime nextTimeZoneChange;
        private bool timeZoneSet;

        private string diseqcIdentity;
        private string tsFileName;

        private string wmcImportName;
        
        private bool movieLookupEnabled;
        private LookupImageType downloadMovieThumbnail;
        private int movieLowTime = 90;
        private int movieHighTime = 150;
        private string moviePhraseSeparator = ",";
        private bool tvLookupEnabled;
        private LookupImageType downloadTVThumbnail = LookupImageType.Poster;
        private int lookupTimeLimit = 60;
        private int lookupErrorLimit = 5;
        private MatchMethod lookupMatching = MatchMethod.Exact;
        private bool lookupNotFound = true;
        private Collection<string> lookupIgnoredPhrases = new Collection<string>();
        private Collection<string> lookupMoviePhrases = new Collection<string>();
        private bool lookupReload;
        private bool lookupIgnoreCategories;
        private bool lookupProcessAsTVSeries;
        private string lookupImagePath;
        private string lookupXmltvImageTagPath;
        private string lookupIgnoredPhraseSeparator = ",";
        private string lookupOutputLanguageCode = "eng";
        private Collection<string> lookupNotMovie;
        private bool lookupImageNameTitle;
        private bool lookupImagesInBase;

        private bool channelUpdateEnabled;
        private ChannelMergeMethod channelMergeMethod = ChannelMergeMethod.Name;
        private ChannelEPGScanner channelEPGScanner = ChannelEPGScanner.EPGCollector;
        private bool channelChildLock;
        private bool channelLogNetworkMap = true;
        private int channelEPGScanInterval = 720;
        private bool channelReloadData;
        private bool channelUpdateNumber;
        private bool channelExcludeNew;

        private int sageTVSatelliteNumber = -1;

        private Collection<ImportFileSpec> importFiles;
        private Collection<ImportChannelChange> importChannelChanges;

        private Collection<EditSpec> editSpecs;

        private string dvbviewerIPAddress;

        private int maxService = -1;

        private bool abandonRequested;

        private string lastError;
        private bool pluginHeader;

        private Collection<OptionEntry> options = new Collection<OptionEntry>();
        private Collection<TraceEntry> traceIDs = new Collection<TraceEntry>();
        private Collection<DebugEntry> debugIDs = new Collection<DebugEntry>();

        private Satellite currentSatellite;
        private SatelliteDish currentDish;
        private TuningFrequency currentFrequency;

        private Collection<RepeatExclusion> exclusions;
        private Collection<string> phrasesToIgnore;
        private Collection<ChannelFilterEntry> channelFilters;
        private Collection<TimeOffsetChannel> timeOffsetChannels;
        private Collection<TuningFrequency> frequencyCollection;
        private Collection<TVStation> stationCollection;

        private RunParameters() { }

        /// <summary>
        /// Initialise a new instance of the RunParameters class.
        /// </summary>
        /// <param name="parameterSet">The type of parameters this instance will hold.</param>
        /// <param name="runType">The type of run processing the parameters ie collection or EPG Centre.</param>
        public RunParameters(ParameterSet parameterSet, RunType runType)
        {
            this.parameterSet = parameterSet;
            this.runType = runType;
        }

        /// <summary>
        /// Process a parameter file.
        /// </summary>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>True if the parameters are valid; false otherwise.</returns>
        public ExitCode Process(string fileName)
        {
            Logger.Instance.Write("Loading collection parameter file from " + fileName);

            FileStream fileStream = null;

            try { fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read); }
            catch (IOException)
            {
                lastError = "Failed to open " + fileName;
                Logger.Instance.Write("Failed to open " + fileName);
                return (ExitCode.ParameterFileNotFound);
            }

            ExitCode reply = ExitCode.OK;
            pluginParameters = false;

            StreamReader streamReader = new StreamReader(fileStream);

            while (!streamReader.EndOfStream && reply == ExitCode.OK)
            {
                string line = streamReader.ReadLine();
                string processLine = line.Replace((char)0x09, ' ');
                processLine = processLine.Replace("\ufffd", "?");

                char splitter = ':';
                if (processLine.IndexOf('=') != -1)
                    splitter = '=';

                string[] parts = processLine.Split(new char[] { splitter });
                if (parts.Length > 0)
                {
                    switch (parts[0].Trim().ToUpperInvariant())
                    {
                        case "OUTPUT":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            if (parts.Length > 2)
                                reply = processOutput(parts[1] + ":" + parts[2]);
                            else
                                reply = processOutput(parts[1]);
                            break;
                        case "TUNER":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processTuner(parts[1]);
                            break;
                        case "SELECTEDTUNER":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processSelectedTuner(parts[1]);
                            break;
                        case "SATELLITE":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processSatellite(parts[1]);
                            break;
                        case "DISH":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processDish(parts[1]);
                            break;
                        case "FREQUENCY":
                            currentFrequency = null;
                            reply = processFrequency(parts[1]);
                            break;
                        case "SCANNINGFREQUENCY":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processFrequency(parts[1]);
                            break;
                        case "TIMEOUTS":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processTimeouts(parts[1]);
                            break;
                        case "STATION":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processStation(parts[1]);
                            break;
                        case "OPTION":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processOption(parts[1]);
                            break;
                        case "DISEQCOPTION":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processDiseqcOption(parts[1]);
                            break;
                        case "TRACE":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processTrace(parts[1]);
                            break;
                        case "DEBUG":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processDebug(parts[1]);
                            break;
                        case "LOCATION":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processLocation(parts[1]);
                            break;
                        case "LANGUAGE":
                        case "INPUTLANGUAGE":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processInputLanguage(parts[1]);
                            break;
                        case "TIMEZONE":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processTimeZone(parts[1]);
                            break;
                        case "TSFILE":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            if (parts.Length > 2)
                                reply = processTSFile(parts[1] + ":" + parts[2]);
                            else
                                reply = processTSFile(parts[1]);
                            break;
                        case "CHANNELS":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processChannels(parts[1]);
                            break;
                        case "DISEQC":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processDiseqc(parts[1]);
                            break;
                        case "DISEQCHANDLER":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processDiseqcHandler(parts[1]);
                            break;
                        case "CHARSET":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processCharSet(parts[1]);
                            break;
                        case "SDTPID":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processSDTPid(parts[1]);
                            break;
                        case "EITPID":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processEITPid(parts[1]);
                            break;
                        case "MHW1PIDS":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processMHW1Pids(parts[1]);
                            break;
                        case "MHW2PIDS":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processMHW2Pids(parts[1]);
                            break;
                        case "DISHNETWORKPID":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processDishNetworkPid(parts[1]);
                            break;
                        case "PLUGINFREQUENCY":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processPluginFrequency(parts[1]);
                            break;
                        case "[DVBS]":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            currentFrequency = new SatelliteFrequency();
                            break;
                        case "[DVBT]":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            currentFrequency = new TerrestrialFrequency();
                            break;
                        case "[DVBC]":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            currentFrequency = new CableFrequency();
                            break;
                        case "[ATSC]":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            currentFrequency = new AtscFrequency();
                            break;
                        case "[CLEARQAM]":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            currentFrequency = new ClearQamFrequency();
                            break;
                        case "[ISDBS]":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            currentFrequency = new ISDBSatelliteFrequency();
                            break;
                        case "[ISDBT]":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            currentFrequency = new ISDBTerrestrialFrequency();
                            break;
                        case "[FILE]":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            currentFrequency = new FileFrequency();
                            break;
                        case "[STREAM]":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            currentFrequency = new StreamFrequency();
                            break;
                        case "[PLUGIN]":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            pluginHeader = true;
                            break;
                        case "TUNINGFILE":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processTuningFile(parts[1]);
                            break;
                        case "SCANNED":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processScannedChannel(parts[1]);
                            break;
                        case "OFFSET":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processOffsetChannel(parts[1]);
                            break;
                        case "INCLUDESERVICE":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processIncludeService(parts[1]);
                            break;
                        case "MAXSERVICE":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processMaxService(parts[1]);
                            break;
                        case "REPEATEXCLUSION":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processRepeatExclusion(parts[1]);
                            break;
                        case "REPEATPHRASE":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processRepeatPhrase(parts[1]);
                            break;
                        case "WMCIMPORTNAME":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processWMCImportName(parts[1]);
                            break;
                        case "EPGDAYS":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processEPGDays(parts[1]);
                            break;
                        case "MOVIELOOKUPENABLED":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processMovieLookupEnabled(parts[1]);
                            break;
                        case "MOVIEIMAGE":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processMovieImage(parts[1]);
                            break;
                        case "MOVIELOWDURATION":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processMovieLowDuration(parts[1]);
                            break;
                        case "MOVIEHIGHDURATION":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processMovieHighDuration(parts[1]);
                            break;
                        case "TVLOOKUPENABLED":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processTVLookupEnabled(parts[1]);
                            break;
                        case "TVIMAGE":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processTVImage(parts[1]);
                            break;
                        case "LOOKUPMATCHING":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processLookupMatching(parts[1]);
                            break;
                        case "LOOKUPNOTFOUND":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processLookupNotFound(parts[1]);
                            break;
                        case "LOOKUPERRORS":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processLookupErrors(parts[1]);
                            break;
                        case "LOOKUPTIMELIMIT":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processLookupTimeLimit(parts[1]);
                            break;
                        case "LOOKUPIGNOREDPHRASES":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processLookupIgnoredPhrases(parts[1]);
                            break;
                        case "LOOKUPMOVIEPHRASES":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processLookupMoviePhrases(parts[1]);
                            break;
                        case "LOOKUPRELOAD":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processLookupReload(parts[1]);
                            break;
                        case "LOOKUPIGNORECATEGORIES":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processLookupIgnoreCategories(parts[1]);
                            break;
                        case "LOOKUPPROCESSASTVSERIES":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processLookupProcessAsTVSeries(parts[1]);
                            break;
                        case "LOOKUPIMAGEPATH":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processLookupImagePath(parts[1]);
                            break;
                        case "LOOKUPXMLTVIMAGETAGPATH":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processLookupXmltvImageTagPath(parts[1]);
                            break;
                        case "LOOKUPIGNOREDPHRASESEPARATOR":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processLookupIgnoredPhraseSeparator(parts[1]);
                            break;                        
                        case "MOVIEPHRASESEPARATOR":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processMoviePhraseSeparator(parts[1]);
                            break;
                        case "LOOKUPNOTMOVIE":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processLookupNotMovie(parts[1]);
                            break;
                        case "LOOKUPIMAGENAMETITLE":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processLookupImageNameTitle(parts[1]);
                            break;
                        case "LOOKUPIMAGESINBASE":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processLookupImagesInBase(parts[1]);
                            break;
                        case "CHANNELUPDATEENABLED":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processChannelUpdateEnabled(parts[1]);
                            break;
                        case "CHANNELMERGEMETHOD":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processChannelMergeMethod(parts[1]);
                            break;
                        case "CHANNELEPGSCANNER":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processChannelEPGScanner(parts[1]);
                            break;
                        case "CHANNELCHILDLOCK":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processChannelChildLock(parts[1]);
                            break;
                        case "CHANNELLOGNETWORKMAP":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processChannelLogNetworkMap(parts[1]);
                            break;
                        case "CHANNELEPGSCANINTERVAL":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processChannelEPGScanInterval(parts[1]);
                            break;
                        case "CHANNELRELOADDATA":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processChannelReloadData(parts[1]);
                            break;
                        case "CHANNELUPDATENUMBER":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processChannelUpdateNumber(parts[1]);
                            break;
                        case "CHANNELEXCLUDENEW":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processChannelExcludeNew(parts[1]);
                            break;
                        case "XMLTVIMPORTFILE":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processXmltvFile(parts[1]);
                            break;
                        case "XMLTVCHANNELCHANGE":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processXmltvChannelChange(parts[1]);
                            break;
                        case "EDITSPEC":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processEditSpec(parts[1]);
                            break;
                        case "DVBVIEWERIPADDRESS":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processDVBViewerIPAddress(parts[1]);
                            break;
                        case "BLADERUNNERFILENAME":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processBladeRunnerFileName(parts[1]);
                            break;
                        case "AREAREGIONFILENAME":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processAreaRegionFileName(parts[1]);
                            break;
                        case "SAGETVFILENAME":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processSageTVFileName(parts[1]);
                            break;
                        case "SAGETVSATNUM":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processSageTVSatNum(parts[1]);
                            break;
                        case "SATIPTUNER":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processSatIpTuner(parts[1]);
                            break;
                        case "SATIPFRONTEND":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processSatIpFrontend(parts[1]);
                            break;
                        case "BYTECONVERTTABLE":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processByteConvertTable(parts[1]);
                            break;
                        case "EITCAROUSEL":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            reply = processEITCarousel(parts[1]);
                            break;
                        case "[GENERAL]":
                        case "[DIAGNOSTICS]":
                        case "[SCANLIST]":
                        case "[STATIONS]":
                        case "[OFFSETS]":
                        case "[SERVICEFILTERS]":
                        case "[REPEATEXCLUSIONS]":
                        case "[LOOKUPS]":
                        case "[CHANNELUPDATE]":
                        case "[XMLTVIMPORT]":
                        case "[EDITSPECS]":
                            Logger.Instance.Write("Processing ini parameter: " + line);
                            break;
                        default:
                            if (parts[0].Trim().ToUpperInvariant().StartsWith("SCANNED"))
                            {
                                Logger.Instance.Write("Processing ini parameter: " + line);
                                reply = processScannedChannel(parts[1]);
                            }
                            else
                                if (!string.IsNullOrWhiteSpace(processLine))
                                    Logger.Instance.Write("Ignored ini parameter: " + processLine);
                            break;
                    }
                }
            }

            streamReader.Close();
            fileStream.Close();

            processOldParameters();

            return (reply);
        }

        private void processOldParameters()
        {
            if (tsFileName != null)
            {
                FileFrequency fileFrequency = new FileFrequency();
                fileFrequency.Path = tsFileName;
                FrequencyCollection.Add(fileFrequency);
            }

            if (parameterSet == ParameterSet.Plugin)
            {
                if (!pluginHeader)
                {
                    foreach (OptionEntry optionEntry in FrequencyCollection[0].AdvancedRunParamters.Options)
                        options.Add(optionEntry);

                    FrequencyCollection[0].AdvancedRunParamters.Options.Clear();
                }
            }

            bool openTVPresent = false;
            
            foreach (TuningFrequency tuningFrequency in FrequencyCollection)
            {
                if (tuningFrequency.CollectionType == CollectionType.OpenTV)
                    openTVPresent = true;
            }

            Collection<OptionName> deletedNames = new Collection<OptionName>();

            foreach (TuningFrequency tuningFrequency in FrequencyCollection)
            {
                if (byteConvertTable != null && tuningFrequency.CollectionType == CollectionType.EIT)
                    tuningFrequency.AdvancedRunParamters.ByteConvertTable = byteConvertTable;
                if (eitCarousel != null && tuningFrequency.CollectionType == CollectionType.EIT)
                    tuningFrequency.AdvancedRunParamters.EITCarousel = eitCarousel;
                if (epgDays != -1)
                    tuningFrequency.AdvancedRunParamters.EPGDays = epgDays;

                if (tuningFrequency.CollectionType != CollectionType.FreeSat ||
                    (tuningFrequency.CollectionType == CollectionType.FreeSat && !openTVPresent))
                {
                    if (countryCode != null)
                        tuningFrequency.AdvancedRunParamters.CountryCode = countryCode;
                    if (channelBouquet != -1)
                        tuningFrequency.AdvancedRunParamters.ChannelBouquet = channelBouquet;
                    if (channelRegion != -1)
                        tuningFrequency.AdvancedRunParamters.ChannelRegion = channelRegion;
                }

                if (characterSet != null && tuningFrequency.CollectionType == CollectionType.EIT)
                    tuningFrequency.AdvancedRunParamters.CharacterSet = characterSet;
                if (inputLanguage != null && tuningFrequency.CollectionType == CollectionType.EIT)
                    tuningFrequency.AdvancedRunParamters.InputLanguage = inputLanguage;

                if (sdtPid != -1)
                    tuningFrequency.AdvancedRunParamters.SDTPid = sdtPid;
                if (eitPid != -1 && tuningFrequency.CollectionType == CollectionType.EIT)
                    tuningFrequency.AdvancedRunParamters.EITPid = eitPid;
                if (mhw1Pids != null && tuningFrequency.CollectionType == CollectionType.MediaHighway1)
                    tuningFrequency.AdvancedRunParamters.MHW1Pids = mhw1Pids;
                if (mhw2Pids != null && tuningFrequency.CollectionType == CollectionType.MediaHighway2)
                    tuningFrequency.AdvancedRunParamters.MHW2Pids = mhw2Pids;
                if (dishNetworkPid != -1 && tuningFrequency.CollectionType == CollectionType.DishNetwork)
                    tuningFrequency.AdvancedRunParamters.DishNetworkPid = dishNetworkPid;
                if (region != 0)
                    tuningFrequency.AdvancedRunParamters.Region = region;

                if (OptionEntry.IsDefined(options, OptionName.UseContentSubtype))
                {
                    tuningFrequency.AdvancedRunParamters.Options.Add(new OptionEntry(OptionName.UseContentSubtype));
                    deletedNames.Add(OptionName.UseContentSubtype);
                }

                if (OptionEntry.IsDefined(options, OptionName.UseFreeSatTables))
                {
                    if (tuningFrequency.CollectionType == CollectionType.EIT)
                        tuningFrequency.AdvancedRunParamters.Options.Add(new OptionEntry(OptionName.UseFreeSatTables));
                    deletedNames.Add(OptionName.UseFreeSatTables);
                }

                if (OptionEntry.IsDefined(options, OptionName.CustomCategoryOverride))
                {
                    tuningFrequency.AdvancedRunParamters.Options.Add(new OptionEntry(OptionName.CustomCategoryOverride));
                    deletedNames.Add(OptionName.CustomCategoryOverride);
                }

                if (OptionEntry.IsDefined(options, OptionName.ProcessAllStations))
                {
                    tuningFrequency.AdvancedRunParamters.Options.Add(new OptionEntry(OptionName.ProcessAllStations));
                    deletedNames.Add(OptionName.ProcessAllStations);
                }

                if (OptionEntry.IsDefined(options, OptionName.FormatReplace))
                {
                    if (tuningFrequency.CollectionType == CollectionType.EIT)
                        tuningFrequency.AdvancedRunParamters.Options.Add(new OptionEntry(OptionName.FormatReplace));
                    deletedNames.Add(OptionName.FormatReplace);
                }

                if (OptionEntry.IsDefined(options, OptionName.FormatConvert))
                {
                    if (tuningFrequency.CollectionType == CollectionType.EIT)
                        tuningFrequency.AdvancedRunParamters.Options.Add(new OptionEntry(OptionName.FormatConvert));
                    deletedNames.Add(OptionName.FormatConvert);
                }

                if (OptionEntry.IsDefined(options, OptionName.FormatConvertTable))
                {
                    if (tuningFrequency.CollectionType == CollectionType.EIT)
                        tuningFrequency.AdvancedRunParamters.Options.Add(new OptionEntry(OptionName.FormatConvertTable));
                    deletedNames.Add(OptionName.FormatConvertTable);
                }

                if (OptionEntry.IsDefined(options, OptionName.UseImage))
                {
                    if (tuningFrequency.CollectionType == CollectionType.MHEG5)
                        tuningFrequency.AdvancedRunParamters.Options.Add(new OptionEntry(OptionName.UseImage));
                    deletedNames.Add(OptionName.UseImage);
                }

                if (OptionEntry.IsDefined(options, OptionName.UseDescAsCategory))
                {
                    if (tuningFrequency.CollectionType == CollectionType.EIT)
                        tuningFrequency.AdvancedRunParamters.Options.Add(new OptionEntry(OptionName.UseDescAsCategory));
                    deletedNames.Add(OptionName.UseDescAsCategory);
                }

                if (OptionEntry.IsDefined(options, OptionName.UseDescAsSubtitle))
                {
                    if (tuningFrequency.CollectionType == CollectionType.EIT)
                        tuningFrequency.AdvancedRunParamters.Options.Add(new OptionEntry(OptionName.UseDescAsSubtitle));
                    deletedNames.Add(OptionName.UseDescAsSubtitle);
                }

                if (OptionEntry.IsDefined(options, OptionName.UseNoDesc))
                {
                    if (tuningFrequency.CollectionType == CollectionType.EIT)
                        tuningFrequency.AdvancedRunParamters.Options.Add(new OptionEntry(OptionName.UseNoDesc));
                    deletedNames.Add(OptionName.UseNoDesc);
                }
            }

            if (selectedTuners.Count != 0)
            {
                foreach (TuningFrequency tuningFrequency in FrequencyCollection)
                {
                    foreach (SelectedTuner selectedTuner in selectedTuners)
                        tuningFrequency.SelectedTuners.Add(selectedTuner);
                }
            }

            foreach (TuningFrequency tuningFrequency in FrequencyCollection)
            {
                SatelliteFrequency satelliteFrequency = tuningFrequency as SatelliteFrequency;
                if (satelliteFrequency != null)
                {
                    if (diseqcIdentity != null)
                        satelliteFrequency.DiseqcRunParamters.DiseqcHandler = diseqcIdentity;

                    if (OptionEntry.IsDefined(options, OptionName.DisableDriverDiseqc))
                    {
                        satelliteFrequency.DiseqcRunParamters.Options.Add(new OptionEntry(OptionName.DisableDriverDiseqc));
                        deletedNames.Add(OptionName.DisableDriverDiseqc);
                    }

                    if (OptionEntry.IsDefined(options, OptionName.RepeatDiseqc))
                    {
                        satelliteFrequency.DiseqcRunParamters.Options.Add(new OptionEntry(OptionName.RepeatDiseqc));
                        deletedNames.Add(OptionName.RepeatDiseqc);
                    }

                    if (OptionEntry.IsDefined(options, OptionName.SwitchAfterPlay))
                    {
                        satelliteFrequency.DiseqcRunParamters.Options.Add(new OptionEntry(OptionName.SwitchAfterPlay));
                        deletedNames.Add(OptionName.SwitchAfterPlay);
                    }

                    if (OptionEntry.IsDefined(options, OptionName.SwitchAfterTune))
                    {
                        satelliteFrequency.DiseqcRunParamters.Options.Add(new OptionEntry(OptionName.SwitchAfterTune));
                        deletedNames.Add(OptionName.SwitchAfterTune);
                    }

                    if (OptionEntry.IsDefined(options, OptionName.UseDiseqcCommand))
                    {
                        satelliteFrequency.DiseqcRunParamters.Options.Add(new OptionEntry(OptionName.UseDiseqcCommand));
                        deletedNames.Add(OptionName.UseDiseqcCommand);
                    }

                    if (OptionEntry.IsDefined(options, OptionName.UseSafeDiseqc))
                    {
                        satelliteFrequency.DiseqcRunParamters.Options.Add(new OptionEntry(OptionName.UseSafeDiseqc));
                        deletedNames.Add(OptionName.UseSafeDiseqc);
                    }
                }
            }

            foreach (OptionName optionName in deletedNames)
                OptionEntry.Remove(options, optionName);
        }

        private ExitCode processOutput(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts))
            {
                lastError = "INI file format error: The Output line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            outputFileName = parts.Trim().Replace("<ApplicationData>", RunParameters.DataDirectory);
            
            return (ExitCode.OK);
        }

        private ExitCode processTuner(string parts)
        {
            if (parts.ToUpperInvariant() == "F")
                return (ExitCode.OK);

            try
            {
                string[] parameters = parts.Split(new char[] { ',' });

                foreach (string parameter in parameters)
                {
                    int selectedTuner = 0;

                    try
                    {
                        selectedTuner = Int32.Parse(parameter);
                    }
                    catch (FormatException)
                    {
                        lastError = "INI file format error: A Tuner number is in the wrong format.";
                        Logger.Instance.Write(lastError);
                    }
                    catch (OverflowException)
                    {
                        lastError = "INI file format error: A Tuner number is out of range.";
                        Logger.Instance.Write(lastError);
                    }

                    if (selectedTuner < 1)
                    {
                        lastError = "INI file format error: A Tuner number is out of range.";
                        Logger.Instance.Write(lastError);
                        return (ExitCode.ParameterError);
                    }
                    else
                        selectedTuners.Add(new SelectedTuner(selectedTuner));
                }
            }
            catch (FormatException)
            {
                lastError = "INI file format error: A Tuner line parameter is in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: A Tuner line is parameter is out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processSelectedTuner(string parts)
        {
            if (parts.Trim().Length < 3)
            {
                int selectedTuner = 0;

                try
                {
                    selectedTuner = Int32.Parse(parts.Trim());

                    if (selectedTuner < 1)
                    {
                        lastError = "INI file format error: A SelectedTuner number is out of range.";
                        Logger.Instance.Write(lastError);
                        return (ExitCode.ParameterError);
                    }
                    else
                    {
                        if (currentFrequency != null)
                            currentFrequency.SelectedTuners.Add(new SelectedTuner(selectedTuner));
                        else
                            selectedTuners.Add(new SelectedTuner(selectedTuner));
                    }
                }
                catch (FormatException)
                {
                    lastError = "INI file format error: A SelectedTuner line parameter is in the wrong format.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
                catch (OverflowException)
                {
                    lastError = "INI file format error: A SelectedTuner line parameter is out of range.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }
            else
            {
                if (currentFrequency != null)
                    currentFrequency.SelectedTuners.Add(new SelectedTuner(parts.Trim()));
                else
                    selectedTuners.Add(new SelectedTuner(parts.Trim()));
            }

            return (ExitCode.OK);
        }

        private ExitCode processSatellite(string parts)
        {
            string[] parameters = parts.Split(new char[] { ',' });
            if (parameters.Length != 1)
            {
                lastError = "INI file format error: A Satellite line has the wrong number of parameters (1).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                int longitude = Int32.Parse(parameters[0].Trim());

                currentSatellite = Satellite.FindSatellite(longitude);
                if (currentSatellite == null)
                    currentSatellite = new Satellite(longitude);

                if (currentFrequency != null && currentFrequency.Provider == null)
                    currentFrequency.Provider = currentSatellite;
            }
            catch (FormatException)
            {
                lastError = "INI file format error: A Satellite line parameter has the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: A Satellite line parameter is out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processDish(string parts)
        {
            string[] parameters = parts.Split(new char[] { ',' });
            if (parameters.Length < 3 || parameters.Length > 5)
            {
                lastError = "INI file format error: A Dish line has the wrong number of parameters (4 or 5).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            SatelliteDish satelliteDish = new SatelliteDish();
            string diseqcSwitch = null;

            try
            {
                satelliteDish.LNBLowBandFrequency = Int32.Parse(parameters[0].Trim());
                satelliteDish.LNBHighBandFrequency = Int32.Parse(parameters[1].Trim());
                satelliteDish.LNBSwitchFrequency = Int32.Parse(parameters[2].Trim());
                if (parameters.Length > 3 && parameters[3].Trim().Length != 0)
                    diseqcSwitch = parameters[3].ToUpperInvariant().Trim();

                if (parameters.Length == 5)
                    satelliteDish.LNBType = new LNBType(parameters[4].Trim());

                currentDish = satelliteDish;

                if (currentFrequency as SatelliteFrequency != null)
                {
                    SatelliteFrequency frequency = currentFrequency as SatelliteFrequency;
                    frequency.SatelliteDish = currentDish;
                    frequency.LNBConversion = parameters.Length == 5;
                    frequency.DiseqcRunParamters.DiseqcSwitch = diseqcSwitch;
                }
            }
            catch (FormatException)
            {
                lastError = "INI file format error: A Dish line parameter is in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: A Dish line parameter is out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processFrequency(string parts)
        {
            string[] parameters = parts.Split(new char[] { ',' });

            if (currentFrequency == null && parameters.Length == 2)
                return (processBasicFrequency(parameters));

            switch (currentFrequency.TunerType)
            {
                case TunerType.Satellite:
                    return (processSatelliteFrequency(parameters));
                case TunerType.Terrestrial:
                    return (processTerrestrialFrequency(parameters));
                case TunerType.Cable:
                    return (processCableFrequency(parameters));
                case TunerType.ATSC:
                case TunerType.ATSCCable:
                    return (processAtscFrequency(parameters));
                case TunerType.ClearQAM:
                    return (processClearQamFrequency(parameters));
                case TunerType.ISDBS:
                    return (processISDBSatelliteFrequency(parameters));
                case TunerType.ISDBT:
                    return (processISDBTerrestrialFrequency(parameters));
                case TunerType.File:
                    return (processFileFrequency(parts));
                case TunerType.Stream:
                    return (processStreamFrequency(parameters));
                default:
                    lastError = "Internal error: Current tuner type not recognised when processing a Frequency line.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
            }
        }

        private ExitCode processBasicFrequency(string[] parameters)
        {
            try
            {
                int frequency = Int32.Parse(parameters[0].Trim());

                if (currentFrequency == null)
                {
                    currentFrequency = new TerrestrialFrequency();
                    currentFrequency.Provider = new TerrestrialProvider("Undefined");
                }

                TerrestrialFrequency terrestrialFrequency = currentFrequency as TerrestrialFrequency;
                terrestrialFrequency.Frequency = frequency;
                ExitCode exitCode = getCollectionType(parameters[1].Trim().ToUpperInvariant(), terrestrialFrequency);
                if (exitCode != ExitCode.OK)
                    return (exitCode);

                FrequencyCollection.Add(terrestrialFrequency);
            }
            catch (FormatException)
            {
                lastError = "INI file format error: A Frequency line parameter has the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: A Frequency line parameter is out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processTerrestrialFrequency(string[] parameters)
        {
            try
            {
                int frequency = Int32.Parse(parameters[0].Trim());
                int bandWidth = Int32.Parse(parameters[1].Trim());

                int plpNumber = -1;
                int nextParameter = 2;

                if (parameters.Length > 3)
                {
                    plpNumber = Int32.Parse(parameters[2].Trim());
                    nextParameter = 3;
                }

                if (currentFrequency == null)
                {
                    currentFrequency = new TerrestrialFrequency();
                    currentFrequency.Provider = TerrestrialProvider.FindProvider(frequency, bandWidth);
                    if (currentFrequency.Provider == null)
                        currentFrequency.Provider = new TerrestrialProvider("Unknown");
                }

                TerrestrialFrequency terrestrialFrequency = currentFrequency as TerrestrialFrequency;
                terrestrialFrequency.Frequency = frequency;
                terrestrialFrequency.Bandwidth = bandWidth;
                terrestrialFrequency.PlpNumber = plpNumber;
                ExitCode exitCode = getCollectionType(parameters[nextParameter].Trim().ToUpperInvariant(), terrestrialFrequency);
                if (exitCode != ExitCode.OK)
                    return (exitCode);

                FrequencyCollection.Add(terrestrialFrequency);
            }
            catch (FormatException)
            {
                lastError = "INI file format error: A Frequency line parameter is in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: A Frequency line parameter is out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processSatelliteFrequency(string[] parameters)
        {
            if (parameters.Length != 5 && parameters.Length != 8)
            {
                lastError = "INI file format error: A DVB-S Frequency line has the wrong number of parameters (5 or 8).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                int frequency = Int32.Parse(parameters[0].Trim());
                int symbolRate = Int32.Parse(parameters[1].Trim());
                FECRate fecRate = new FECRate(parameters[2]);
                SignalPolarization polarization = new SignalPolarization(parameters[3].Trim()[0]);

                SignalPilot.Pilot pilot = SignalPilot.Pilot.NotSet;
                SignalRollOff.RollOff rollOff = SignalRollOff.RollOff.NotSet;
                SignalModulation.Modulation modulation = SignalModulation.Modulation.QPSK;

                int nextParameter = 4;

                if (parameters.Length == 8)
                {
                    pilot = (SignalPilot.Pilot)Enum.Parse(typeof(SignalPilot.Pilot), parameters[4]);
                    rollOff = (SignalRollOff.RollOff)Enum.Parse(typeof(SignalRollOff.RollOff), parameters[5]);
                    modulation = (SignalModulation.Modulation)Enum.Parse(typeof(SignalModulation.Modulation), parameters[6]);
                    nextParameter = 7;
                }

                if (currentFrequency == null)
                    currentFrequency = new SatelliteFrequency();

                SatelliteFrequency satelliteFrequency = currentFrequency as SatelliteFrequency;

                satelliteFrequency.Frequency = frequency;
                satelliteFrequency.SymbolRate = symbolRate;
                satelliteFrequency.FEC = fecRate;
                satelliteFrequency.Polarization = polarization;
                satelliteFrequency.Pilot = pilot;
                satelliteFrequency.RollOff = rollOff;
                satelliteFrequency.Modulation = modulation;
                ExitCode exitCode = getCollectionType(parameters[nextParameter].Trim().ToUpperInvariant(), satelliteFrequency);
                if (exitCode != ExitCode.OK)
                    return (exitCode);

                satelliteFrequency.SatelliteDish = currentDish;

                if (currentFrequency.Provider == null)
                    currentFrequency.Provider = currentSatellite;

                if (currentDish != null)
                    satelliteFrequency.LNBConversion = currentDish.LNBType != null;

                FrequencyCollection.Add(satelliteFrequency);
            }
            catch (FormatException)
            {
                lastError = "INI file format error: A DVB-S Frequency line parameter has the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: A DVB-S Frequency line parameter is out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processCableFrequency(string[] parameters)
        {
            if (parameters.Length != 5)
            {
                lastError = "INI file format error: A DVB-C Frequency line has the wrong number of parameters (5).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                int frequency = Int32.Parse(parameters[0].Trim());
                int symbolRate = Int32.Parse(parameters[1].Trim());
                FECRate fecRate = new FECRate(parameters[2]);
                SignalModulation.Modulation modulation = (SignalModulation.Modulation)Enum.Parse(typeof(SignalModulation.Modulation), parameters[3].ToUpperInvariant(), true);

                if (currentFrequency == null)
                {
                    currentFrequency = new CableFrequency();
                    currentFrequency.Provider = CableProvider.FindProvider(frequency, symbolRate, fecRate, modulation);
                    if (currentFrequency.Provider == null)
                        currentFrequency.Provider = new CableProvider("Unknown");
                }

                CableFrequency cableFrequency = currentFrequency as CableFrequency;
                cableFrequency.Frequency = frequency;
                cableFrequency.SymbolRate = symbolRate;
                cableFrequency.FEC = fecRate;
                cableFrequency.Modulation = modulation;
                ExitCode exitCode = getCollectionType(parameters[4].Trim().ToUpperInvariant(), cableFrequency);
                if (exitCode != ExitCode.OK)
                    return (exitCode);

                FrequencyCollection.Add(cableFrequency);
            }
            catch (FormatException)
            {
                lastError = "INI file format error: A DVB-C Frequency line parameter has the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: A DVB-C Frequency line parameter is out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processAtscFrequency(string[] parameters)
        {
            if (parameters.Length != 6)
            {
                lastError = "INI file format error: An ATSC Frequency line has the wrong number of parameters (6).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                int frequency = Int32.Parse(parameters[0].Trim());
                int channelNumber = Int32.Parse(parameters[1].Trim());
                int symbolRate = Int32.Parse(parameters[2].Trim());
                FECRate fecRate = new FECRate(parameters[3]);
                SignalModulation.Modulation modulation = (SignalModulation.Modulation)Enum.Parse(typeof(SignalModulation.Modulation), parameters[4].ToUpperInvariant(), true);

                AtscFrequency atscFrequency = currentFrequency as AtscFrequency;
                atscFrequency.Frequency = frequency;
                atscFrequency.ChannelNumber = channelNumber;
                atscFrequency.SymbolRate = symbolRate;
                atscFrequency.FEC = fecRate;
                atscFrequency.Modulation = modulation;
                ExitCode exitCode = getCollectionType(parameters[5].Trim().ToUpperInvariant(), atscFrequency);
                if (exitCode != ExitCode.OK)
                    return (exitCode);

                FrequencyCollection.Add(atscFrequency);
            }
            catch (FormatException)
            {
                lastError = "INI file format error: An ATSC Frequency line parameter is in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: An ATSC Frequency line parameter is out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processClearQamFrequency(string[] parameters)
        {
            if (parameters.Length != 6)
            {
                lastError = "INI file format error: A Clear QAM Frequency line has the wrong number of parameters (6).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                int frequency = Int32.Parse(parameters[0].Trim());
                int channelNumber = Int32.Parse(parameters[1].Trim());
                int symbolRate = Int32.Parse(parameters[2].Trim());
                FECRate fecRate = new FECRate(parameters[3]);
                SignalModulation.Modulation modulation = (SignalModulation.Modulation)Enum.Parse(typeof(SignalModulation.Modulation), parameters[4].ToUpperInvariant(), true);

                ClearQamFrequency clearQamFrequency = currentFrequency as ClearQamFrequency;
                clearQamFrequency.Frequency = frequency;
                clearQamFrequency.ChannelNumber = channelNumber;
                clearQamFrequency.SymbolRate = symbolRate;
                clearQamFrequency.FEC = fecRate;
                clearQamFrequency.Modulation = modulation;
                ExitCode exitCode = getCollectionType(parameters[5].Trim().ToUpperInvariant(), clearQamFrequency);
                if (exitCode != ExitCode.OK)
                    return (exitCode);

                FrequencyCollection.Add(clearQamFrequency);
            }
            catch (FormatException)
            {
                lastError = "INI file format error: A Clear QAM Frequency line parameter is in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: A Clear QAM Frequency line parameter is out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processISDBSatelliteFrequency(string[] parameters)
        {
            if (parameters.Length != 5)
            {
                lastError = "INI file format error: An ISDB Satellite Frequency line has the wrong number of parameters (5).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                int frequency = Int32.Parse(parameters[0].Trim());
                int symbolRate = Int32.Parse(parameters[1].Trim());
                FECRate fecRate = new FECRate(parameters[2]);
                SignalPolarization polarization = new SignalPolarization(parameters[3].Trim()[0]);

                if (currentFrequency == null)
                    currentFrequency = new ISDBSatelliteFrequency();

                ISDBSatelliteFrequency satelliteFrequency = currentFrequency as ISDBSatelliteFrequency;

                satelliteFrequency.Frequency = frequency;
                satelliteFrequency.SymbolRate = symbolRate;
                satelliteFrequency.FEC = fecRate;
                satelliteFrequency.Polarization = polarization;
                ExitCode exitCode = getCollectionType(parameters[4].Trim().ToUpperInvariant(), satelliteFrequency);
                if (exitCode != ExitCode.OK)
                    return (exitCode);

                satelliteFrequency.SatelliteDish = currentDish;

                if (currentFrequency.Provider == null)
                    currentFrequency.Provider = currentSatellite;

                FrequencyCollection.Add(satelliteFrequency);
            }
            catch (FormatException)
            {
                lastError = "INI file format error: An ISDB Satellite Frequency line parameter is in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: An ISDB Satellite Frequency line parameter is out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processISDBTerrestrialFrequency(string[] parameters)
        {
            if (parameters.Length != 4)
            {
                lastError = "INI file format error: An ISDB Terrestrial Frequency line has the wrong number of parameters (4).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                int channelNumber = Int32.Parse(parameters[0].Trim());
                int frequency = Int32.Parse(parameters[1].Trim());
                int bandWidth = Int32.Parse(parameters[2].Trim());

                if (currentFrequency == null)
                {
                    currentFrequency = new ISDBTerrestrialFrequency();
                    currentFrequency.Provider = ISDBTerrestrialProvider.FindProvider(frequency, bandWidth);
                    if (currentFrequency.Provider == null)
                        currentFrequency.Provider = new ISDBTerrestrialProvider("Unknown");
                }

                ISDBTerrestrialFrequency terrestrialFrequency = currentFrequency as ISDBTerrestrialFrequency;
                terrestrialFrequency.ChannelNumber = channelNumber;
                terrestrialFrequency.Frequency = frequency;
                terrestrialFrequency.Bandwidth = bandWidth;
                ExitCode exitCode = getCollectionType(parameters[3].Trim().ToUpperInvariant(), terrestrialFrequency);
                if (exitCode != ExitCode.OK)
                    return (exitCode);

                FrequencyCollection.Add(terrestrialFrequency);
            }
            catch (FormatException)
            {
                lastError = "INI file format error: An ISDB Terrestrial Frequency line parameter is in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: An ISDB Terrestrial Frequency line parameter is out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processFileFrequency(string parts)
        {
            int index = parts.LastIndexOf(",");
            if (index == -1)
            {
                lastError = "INI file format error: A File Frequency line has the wrong number of parameters (2).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }            

            if (currentFrequency == null)
                currentFrequency = new FileFrequency();

            FileFrequency fileFrequency = currentFrequency as FileFrequency;
            fileFrequency.Path = parts.Substring(0, index).Trim();
            ExitCode exitCode = getCollectionType(parts.Substring(index + 1).Trim().ToUpperInvariant(), fileFrequency);
            if (exitCode != ExitCode.OK)
                return (exitCode);

            FrequencyCollection.Add(fileFrequency);

            return (ExitCode.OK);
        }

        private ExitCode processStreamFrequency(string[] parameters)
        {
            if (parameters.Length != 5 && parameters.Length != 7)
            {
                lastError = "INI file format error: A Stream Frequency line has the wrong number of parameters (5 or 7).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                if (currentFrequency == null)
                    currentFrequency = new StreamFrequency();

                StreamFrequency streamFrequency = currentFrequency as StreamFrequency;
                streamFrequency.IPAddress = parameters[0].Trim();
                streamFrequency.PortNumber = Int32.Parse(parameters[1].Trim());

                int parameterIndex = 2;

                if (parameters.Length == 7)
                {
                    if (string.IsNullOrWhiteSpace(parameters[2]))
                    {
                        lastError = "INI file format error: A Stream Frequency line multisource address is missing.";
                        Logger.Instance.Write(lastError);
                        return (ExitCode.ParameterError);
                    }
                    
                    streamFrequency.MulticastSource = parameters[2].Trim();

                    if (!string.IsNullOrWhiteSpace(parameters[3]))
                    {
                        try
                        {
                            streamFrequency.MulticastPort = Int32.Parse(parameters[3].Trim());
                        }
                        catch (FormatException)
                        {
                            lastError = "INI file format error: A Stream Frequency line parameter is in the wrong format.";
                            Logger.Instance.Write(lastError);
                            return (ExitCode.ParameterError);
                        }
                        catch (OverflowException)
                        {
                            lastError = "INI file format error: A Stream Frequency line parameter is out of range.";
                            Logger.Instance.Write(lastError);
                            return (ExitCode.ParameterError);
                        }
                    }

                    parameterIndex += 2;
                }

                streamFrequency.Protocol = (StreamProtocol)Enum.Parse(typeof(StreamProtocol), parameters[parameterIndex].Trim());
                parameterIndex++;

                if (!string.IsNullOrWhiteSpace(parameters[parameterIndex]))
                    streamFrequency.Path = parameters[parameterIndex].Trim();
                parameterIndex++;

                ExitCode exitCode = getCollectionType(parameters[parameterIndex].Trim().ToUpperInvariant(), streamFrequency);
                if (exitCode != ExitCode.OK)
                    return (exitCode);

                FrequencyCollection.Add(streamFrequency);
            }
            catch (FormatException)
            {
                lastError = "INI file format error: A Stream Frequency line parameter is in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: A Stream Frequency line parameter is out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode getCollectionType(string parameter, TuningFrequency tuningFrequency)
        {
            switch (parameter)
            {
                case "N":
                case "EIT":
                    tuningFrequency.CollectionType = CollectionType.EIT;
                    break;
                case "Y":
                case "MHEG5":
                    tuningFrequency.CollectionType = CollectionType.MHEG5;
                    break;
                case "OPENTV":
                    tuningFrequency.CollectionType = CollectionType.OpenTV;
                    break;
                case "MHW1":
                case "MEDIAHIGHWAY1":
                    tuningFrequency.CollectionType = CollectionType.MediaHighway1;
                    break;
                case "MHW2":
                case "MEDIAHIGHWAY2":
                    tuningFrequency.CollectionType = CollectionType.MediaHighway2;
                    break;
                case "FREESAT":
                    tuningFrequency.CollectionType = CollectionType.FreeSat;
                    break;
                case "PSIP":
                    tuningFrequency.CollectionType = CollectionType.PSIP;
                    break;
                case "DISHNETWORK":
                    tuningFrequency.CollectionType = CollectionType.DishNetwork;
                    break;
                case "BELLTV":
                    tuningFrequency.CollectionType = CollectionType.BellTV;
                    break;
                case "SIEHFERNINFO":
                    tuningFrequency.CollectionType = CollectionType.SiehfernInfo;
                    break;
                case "NDS":
                    tuningFrequency.CollectionType = CollectionType.NDS;
                    break;
                default:
                    lastError = "INI file format error: The collection type on a Frequency line is not recognised.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processTimeouts(string parts)
        {
            string[] parameters = parts.Split(new char[] { ',' });
            if (parameters.Length < 2 || parameters.Length > 5)
            {
                lastError = "INI file format error: The Timeouts line has the wrong number of parameters (2-5).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                int timeoutSeconds = Int32.Parse(parameters[0].Trim());
                lockTimeout = new TimeSpan(0, timeoutSeconds / 60, timeoutSeconds % 60);

                timeoutSeconds = Int32.Parse(parameters[1].Trim());
                frequencyTimeout = new TimeSpan(0, timeoutSeconds / 60, timeoutSeconds % 60);

                if (parameters.Length > 2 && !string.IsNullOrWhiteSpace(parameters[2]))
                    repeats = Int32.Parse(parameters[2].Trim());

                if (parameters.Length > 3 && !string.IsNullOrWhiteSpace(parameters[3]))
                    bufferFills = Int32.Parse(parameters[3].Trim());

                if (parameters.Length > 4 && !string.IsNullOrWhiteSpace(parameters[4]))
                    bufferSize = Int32.Parse(parameters[4].Trim());

            }
            catch (FormatException)
            {
                lastError = "INI file format error: A Timeouts line parameter is in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: A Timeouts line parameter is out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processStation(string parts)
        {
            string[] parameters = parts.Split(new char[] { ',' });
            if (parameters.Length < 3 || parameters.Length > 5)
            {
                lastError = "INI file format error: A Station line has the wrong number of parameters (3-5).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            TVStation tvStation;
            if (parameters.Length == 3)
                tvStation = new TVStation("Excluded Station");
            else
                tvStation = new TVStation("Changed Station");

            tvStation.CreatedFromIni = true;

            try
            {
                tvStation.OriginalNetworkID = Int32.Parse(parameters[0].Trim());
                tvStation.TransportStreamID = Int32.Parse(parameters[1].Trim());
                tvStation.ServiceID = Int32.Parse(parameters[2].Trim());

                if (parameters.Length == 3)
                {
                    tvStation.ExcludedByUser = true;

                    TVStation oldStation = TVStation.FindStation(StationCollection, tvStation.OriginalNetworkID, tvStation.TransportStreamID, tvStation.ServiceID);
                    if (oldStation == null)
                        TVStation.AddStation(StationCollection, tvStation);
                    else
                        oldStation.ExcludedByUser = tvStation.ExcludedByUser;
                    return (ExitCode.OK);
                }

                try
                {
                    tvStation.LogicalChannelNumber = Int32.Parse(parameters[3].Trim());
                    if (parameters.Length == 5)
                        tvStation.NewName = parameters[4].Trim().Replace("%%", ",");

                    TVStation oldStation = TVStation.FindStation(StationCollection, tvStation.OriginalNetworkID, tvStation.TransportStreamID, tvStation.ServiceID);
                    if (oldStation == null)
                        TVStation.AddStation(StationCollection, tvStation);
                    else
                    {
                        oldStation.LogicalChannelNumber = tvStation.LogicalChannelNumber;
                        oldStation.NewName = tvStation.NewName;
                    }

                    return (ExitCode.OK);
                }
                catch (FormatException)
                {
                    if (parameters.Length == 5)
                    {
                        lastError = "INI file format error: A Station line parameter is in the wrong format.";
                        Logger.Instance.Write(lastError);
                        return (ExitCode.ParameterError);
                    }
                    else
                    {
                        tvStation.NewName = parameters[3].Trim().Replace("%%", ",");

                        TVStation oldStation = TVStation.FindStation(StationCollection, 
                            tvStation.OriginalNetworkID, tvStation.TransportStreamID, tvStation.ServiceID);
                        if (oldStation == null)
                            TVStation.AddStation(StationCollection, tvStation);
                        else
                            oldStation.NewName = tvStation.NewName;
                    }
                }
                catch (OverflowException)
                {
                    lastError = "INI file format error: A Station line parameter is out of range.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }
            catch (FormatException)
            {
                lastError = "INI file format error: A Station line parameter is in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: A Station line parameter is out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processOption(string parts)
        {
            string[] parameters = parts.Split(new char[] { ',' });

            foreach (string parameter in parameters)
            {
                string[] parameterParts = parameter.Split(new char[] { '-' });

                try
                {
                    OptionEntry optionEntry = new OptionEntry((OptionName)Enum.Parse(typeof(OptionName), parameterParts[0].Trim(), true));

                    if (parameterParts.Length == 2)
                    {
                        try
                        {
                            optionEntry.Parameter = Int32.Parse(parameterParts[1]);
                        }
                        catch (FormatException)
                        {
                            lastError = "INI file format error: The Option '" + parameterParts[0].Trim() + "' has a parameter in the wrong format.";
                            Logger.Instance.Write(lastError);
                            return (ExitCode.ParameterError);
                        }
                        catch (OverflowException)
                        {
                            lastError = "INI file format error: The Option '" + parameterParts[0].Trim() + "' has a parameter out of range.";
                            Logger.Instance.Write(lastError);
                            return (ExitCode.ParameterError);
                        }

                    }

                    if (currentFrequency != null)
                        currentFrequency.AdvancedRunParamters.Options.Add(optionEntry);                        
                    else
                        options.Add(optionEntry);
                }
                catch (ArgumentException)
                {
                    Logger.Instance.Write("INI file format error: The Option '" + parameter.Trim() + "' is undefined and will be ignored.");
                }
            }

            return (ExitCode.OK);
        }

        private ExitCode processDiseqcOption(string parts)
        {
            if (currentFrequency as SatelliteFrequency == null)
            {
                lastError = "INI file format error: The DiseqcOption parameter is not in a satellite section.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            string[] parameters = parts.Split(new char[] { ',' });

            foreach (string parameter in parameters)
            {
                string[] parameterParts = parameter.Split(new char[] { '-' });

                try
                {
                    OptionEntry optionEntry = new OptionEntry((OptionName)Enum.Parse(typeof(OptionName), parameterParts[0].Trim(), true));

                    if (parameterParts.Length == 2)
                    {
                        try
                        {
                            optionEntry.Parameter = Int32.Parse(parameterParts[1]);
                        }
                        catch (FormatException)
                        {
                            lastError = "INI file format error: The DiseqcOption '" + parameterParts[0].Trim() + "' has a parameter in the wrong format.";
                            Logger.Instance.Write(lastError);
                            return (ExitCode.ParameterError);
                        }
                        catch (OverflowException)
                        {
                            lastError = "INI file format error: The DiseqcOption '" + parameterParts[0].Trim() + "' has a parameter out of range.";
                            Logger.Instance.Write(lastError);
                            return (ExitCode.ParameterError);
                        }

                    }

                    ((SatelliteFrequency)currentFrequency).DiseqcRunParamters.Options.Add(optionEntry);
                }
                catch (ArgumentException)
                {
                    Logger.Instance.Write("INI file format error: The DiseqcOption '" + parameter.Trim() + "' is undefined and will be ignored.");
                }
            }

            return (ExitCode.OK);
        }

        private ExitCode processTrace(string parts)
        {
            string[] parameters = parts.Split(new char[] { ',' });

            foreach (string parameter in parameters)
            {
                TraceEntry traceEntry = TraceEntry.GetInstance(parameter);
                if (traceEntry == null)
                {
                    lastError = "INI file format error: " + TraceEntry.LastError;
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }

                traceIDs.Add(traceEntry);                
            }

            return (ExitCode.OK);
        }

        private ExitCode processDebug(string parts)
        {
            string[] parameters = parts.Split(new char[] { ',' });

            foreach (string parameter in parameters)
            {
                string[] parameterParts = parameter.Split(new char[] { '-' });

                try
                {
                    DebugEntry debugEntry = new DebugEntry((DebugName)Enum.Parse(typeof(DebugName), parameterParts[0].Trim(), true));

                    if (parameterParts.Length == 2)
                    {
                        try
                        {
                            debugEntry.Parameter = Int32.Parse(parameterParts[1]);
                        }
                        catch (FormatException)
                        {
                            lastError = "INI file format error: The Debug name '" + parameterParts[0].Trim() + "' has a parameter in the wrong format.";
                            Logger.Instance.Write(lastError);
                            return (ExitCode.ParameterError);
                        }
                        catch (OverflowException)
                        {
                            lastError = "INI file format error: The Debug name '" + parameterParts[0].Trim() + "' has a parameter out of range.";
                            Logger.Instance.Write(lastError);
                            return (ExitCode.ParameterError);
                        }

                    }

                    debugIDs.Add(debugEntry);
                }
                catch (ArgumentException)
                {
                    Logger.Instance.Write("INI file format error: The Debug ID '" + parameter.Trim() + "' is undefined and will be ignored.");
                }
            }

            return (ExitCode.OK);
        }

        private ExitCode processLocation(string parts)
        {
            string[] parameters = parts.Split(new char[] { ',' });
            if (parameters.Length > 2)
            {
                lastError = "INI file format error: A Location line has the wrong number of parameters (1 or 2).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                string countryCode = parameters[0].Trim().ToUpperInvariant();
                int region;
                if (parameters.Length == 2)
                    region = Int32.Parse(parameters[1].Trim());
                else
                    region = 0;

                if (currentFrequency != null)
                {
                    currentFrequency.AdvancedRunParamters.CountryCode = countryCode;
                    currentFrequency.AdvancedRunParamters.Region = region;
                }
                else
                {
                    this.countryCode = countryCode;
                    this.region = region;
                }
            }
            catch (FormatException)
            {
                lastError = "INI file format error: A Location line has a parameter in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: A Location line has a parameter out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processInputLanguage(string parts)
        {
            bool reply = LanguageCode.Validate(parts.Trim().ToLower(), LanguageCode.Usage.Input);
            if (!reply)
            {
                lastError = "INI file format error: The input language code is undefined.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (currentFrequency != null)
                currentFrequency.AdvancedRunParamters.InputLanguage = parts.Trim().ToLower();
            else
                inputLanguage = parts.Trim().ToLower();

            return (ExitCode.OK);
        }

        private ExitCode processTimeZone(string parts)
        {
            string[] parameters = parts.Split(new char[] { ',' });
            if (parameters.Length != 1 && parameters.Length != 4)
            {
                lastError = "INI file format error: The Timezone line has the wrong number of parameters (1 or 4).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                string[] offsetParts = parameters[0].Split(new char[] { '.' });
                if (offsetParts.Length != 2)
                {
                    lastError = "INI file format error: The Timezone line parameter format is incorrect.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
                int hours = Int32.Parse(offsetParts[0].Trim());
                int minutes = Int32.Parse(offsetParts[1].Trim());
                timeZone = new TimeSpan(hours, minutes, 0);

                if (parameters.Length == 4)
                {
                    string[] nextParts = parameters[1].Split(new char[] { '.' });
                    if (nextParts.Length != 2)
                    {
                        lastError = "INI file format error: The Timezone line parameter format is incorrect.";
                        Logger.Instance.Write(lastError);
                        return (ExitCode.ParameterError);
                    }

                    int nextHours = Int32.Parse(nextParts[0].Trim());
                    int nextMinutes = Int32.Parse(nextParts[1].Trim());
                    nextTimeZone = new TimeSpan(nextHours, nextMinutes, 0);

                    try
                    {
                        nextTimeZoneChange = DateTime.ParseExact(parameters[2].Trim() + " " + parameters[3].Trim() + ".00", "dd/MM/yy HH.mm.ss", null);
                    }
                    catch (FormatException)
                    {
                        lastError = "INI file format error: The Timezone line parameter format is incorrect.";
                        Logger.Instance.Write(lastError);
                        return (ExitCode.ParameterError);
                    }
                }
                else
                {
                    nextTimeZone = timeZone;
                    nextTimeZoneChange = DateTime.MaxValue;
                }

                timeZoneSet = true;
            }
            catch (FormatException)
            {
                lastError = "INI file format error: The Timezone line paramter format is incorrect.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (ArgumentOutOfRangeException)
            {
                lastError = "INI file format error: The Timezone line is incorrect.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processTSFile(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The TSFile line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            tsFileName = parts.Trim();

            return (ExitCode.OK);
        }

        private ExitCode processChannels(string parts)
        {
            string[] parameters = parts.Split(new char[] { ',' });
            if (parameters.Length > 2)
            {
                Logger.Instance.Write("INI file format error: A Channels line has the wrong number of parameters (1 or 2).");
                return (ExitCode.ParameterError);
            }

            try
            {
                int bouquet = Int32.Parse(parameters[0].Trim());
                int region = -1;
                if (parameters.Length > 1)
                    region = Int32.Parse(parameters[1].Trim());

                if (currentFrequency != null)
                {
                    currentFrequency.AdvancedRunParamters.ChannelBouquet = bouquet;
                    currentFrequency.AdvancedRunParamters.ChannelRegion = region;
                }
                else
                {
                    channelBouquet = bouquet;
                    channelRegion = region;
                }
            }
            catch (FormatException)
            {
                Logger.Instance.Write("INI file format error: A Channels line parameter is in the wrong format.");
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                Logger.Instance.Write("INI file format error: A Channels line parameter is out of range.");
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processDiseqc(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: A Diseqc line has the parameter missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            
            diseqcIdentity = parts.Trim().ToUpperInvariant();

            return (ExitCode.OK);
        }

        private ExitCode processDiseqcHandler(string parts)
        {
            if (currentFrequency as SatelliteFrequency == null)
            {
                lastError = "INI file format error: A DiseqcHandler line is not in a satellite section.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (parts == string.Empty)
            {
                lastError = "INI file format error: A DiseqcHandler line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            ((SatelliteFrequency)currentFrequency).DiseqcRunParamters.DiseqcHandler = parts.Trim().ToUpperInvariant();

            return (ExitCode.OK);
        }

        private ExitCode processCharSet(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: A CharSet line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            string characterSet;

            try
            {
                int characterSetSuffix = Int32.Parse(parts.Trim());
                characterSet = findCharacterSet(characterSetSuffix);
                if (characterSet == null)
                {
                    lastError = "INI file format error: A CharSet line has an unknown character set identifier.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }
            catch (FormatException)
            {
                characterSet = findCharacterSet(parts);
                if (characterSet == null)
                {
                    lastError = "INI file format error: A CharSet line has a parameter in the wrong format.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: A CharSet line has a parameter out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (currentFrequency != null)
                currentFrequency.AdvancedRunParamters.CharacterSet = characterSet;
            else
                this.characterSet = characterSet;

            return (ExitCode.OK);
        }

        private string findCharacterSet(int characterSetSuffix)
        {
            return (findCharacterSet("iso-8859-" + characterSetSuffix));
        }

        private string findCharacterSet(string characterSet)
        {
            foreach (EncodingInfo encoding in ASCIIEncoding.GetEncodings())
            {
                if (encoding.Name == characterSet)
                    return (characterSet);
            }

            return (null);
        }

        private ExitCode processSDTPid(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: An SDTPid line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                if (currentFrequency != null)
                    currentFrequency.AdvancedRunParamters.SDTPid = Int32.Parse(parts.Trim());
                else
                    sdtPid = Int32.Parse(parts.Trim());
            }
            catch (FormatException)
            {
                lastError = "INI file format error: An SDTPid line has the PID in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: An SDTPid line has the PID out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processEITPid(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: An EITPid line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                if (currentFrequency != null)
                    currentFrequency.AdvancedRunParamters.EITPid = Int32.Parse(parts.Trim());
                else
                    eitPid = Int32.Parse(parts.Trim());
            }
            catch (FormatException)
            {
                lastError = "INI file format error: An EITPid line has the PID in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: An EITPid line has the PID out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processMHW1Pids(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: A MHW1Pids line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            string[] parameters = parts.Split(new char[] { ',' });
            if (parameters.Length != 2)
            {
                lastError = "INI file format error: An MHW1Pids line has the wrong number of parameters (2).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                int pid1 = Int32.Parse(parameters[0].Trim());
                int pid2 = Int32.Parse(parameters[1].Trim());

                if (currentFrequency != null)
                    currentFrequency.AdvancedRunParamters.MHW1Pids = new int[] { pid1, pid2 };
                else
                    mhw1Pids = new int[] { pid1, pid2 };
            }
            catch (FormatException)
            {
                lastError = "INI file format error: An MHW1Pids line has a PID in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: An MHW1Pids line has a PID out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processMHW2Pids(string parts)
        {
            string[] parameters = parts.Split(new char[] { ',' });
            if (parameters.Length != 3)
            {
                lastError = "INI file format error: An MHW2Pids line has the wrong number of parameters (3).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                int pid1 = Int32.Parse(parameters[0].Trim());
                int pid2 = Int32.Parse(parameters[1].Trim());
                int pid3 = Int32.Parse(parameters[2].Trim());

                if (currentFrequency != null)
                    currentFrequency.AdvancedRunParamters.MHW2Pids = new int[] { pid1, pid2, pid3 };
                else
                    mhw2Pids = new int[] { pid1, pid2, pid3 };
            }
            catch (FormatException)
            {
                lastError = "INI file format error: An MHW2Pids line has a PID in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: An MHW2Pids line has a PID out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processDishNetworkPid(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: A DishNetworkPid line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                if (currentFrequency != null)
                    currentFrequency.AdvancedRunParamters.DishNetworkPid = Int32.Parse(parts.Trim());
                else
                    dishNetworkPid = Int32.Parse(parts.Trim());
            }
            catch (FormatException)
            {
                lastError = "INI file format error: A DishNetworkPid line has a PID in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: A DishNetworkPid line has a PID out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processPluginFrequency(string parts)
        {
            if (parameterSet != ParameterSet.Plugin)
            {
                lastError = "INI file format error: A PluginFrequency line has been encountered in a normal collection.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            string[] parameters = parts.Split(new char[] { ',' });
            if (parameters.Length != 5)
            {
                lastError = "INI file format error: A PluginFrequency line has the wrong number of parameters (5).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            TuningFrequency pluginFrequency;

            switch (parameters[0])
            {
                case "Satellite":
                    pluginFrequency = new SatelliteFrequency();
                    pluginFrequency.Provider = Satellite.FindProvider(parameters[1]);
                    break;
                case "Terrestrial":
                    pluginFrequency = new TerrestrialFrequency();
                    pluginFrequency.Provider = TerrestrialProvider.FindProvider(parameters[1]);
                    break;
                case "Cable":
                    pluginFrequency = new CableFrequency();
                    pluginFrequency.Provider = CableProvider.FindProvider(parameters[1]);
                    break;
                case "ATSC":
                    pluginFrequency = new AtscFrequency();
                    pluginFrequency.Provider = AtscProvider.FindProvider(parameters[1]);
                    break;
                case "ISDBS":
                    pluginFrequency = new ISDBSatelliteFrequency();
                    pluginFrequency.Provider = ISDBSatelliteProvider.FindProvider(parameters[1]);
                    break;
                case "ISDBT":
                    pluginFrequency = new ISDBTerrestrialFrequency();
                    pluginFrequency.Provider = ISDBTerrestrialProvider.FindProvider(parameters[1]);
                    break;
                default:
                    lastError = "INI file format error: A PluginFrequency line has an unknown delivery type - " + parameters[0] + ".";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
            }
            
            if (pluginFrequency.Provider == null)
                pluginFrequency.Provider = new Provider(parameters[1].Replace('+', ','));

            currentFrequency = pluginFrequency;
            FrequencyCollection.Add(pluginFrequency);
            pluginParameters = true;

            try
            {
                pluginFrequency.Frequency = Int32.Parse(parameters[2]);
                if (parameters[0] == "Satellite")
                    (pluginFrequency as SatelliteFrequency).Polarization = new SignalPolarization(parameters[3].Trim()[0]);
                return (getCollectionType(parameters[4].Trim().ToUpperInvariant(), pluginFrequency));
            }
            catch (FormatException)
            {
                lastError = "INI file format error: A PluginFrequency line has a frequency in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: A PluginFrequency line has a frequency out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
        }

        private ExitCode processTuningFile(string parts)
        {
            if (currentFrequency == null)
            {
                Logger.Instance.Write("INI file format error: A TuningFile parameter is out of sequence.");
                return (ExitCode.ParameterError);
            }

            string fileName = parts.Trim().Substring(0, parts.Length - 4);

            SatelliteFrequency satelliteFrequency = currentFrequency as SatelliteFrequency;
            if (satelliteFrequency != null)
            {
                satelliteFrequency.Provider = Satellite.FindSatellite(fileName);
                if (runType != RunType.Centre || satelliteFrequency.Provider != null)
                    return (ExitCode.OK);
                else
                {
                    lastError = "INI file format error: A TuningFile parameter references an unknown tuning file - " + fileName + ".";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }

            TerrestrialFrequency terrestrialFrequency = currentFrequency as TerrestrialFrequency;
            if (terrestrialFrequency != null)
            {
                terrestrialFrequency.Provider = TerrestrialProvider.FindProvider(fileName);
                if (runType != RunType.Centre || terrestrialFrequency.Provider != null)
                    return (ExitCode.OK);
                else
                {
                    lastError = "INI file format error: A TuningFile parameter references an unknown tuning file - " + fileName + ".";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }

            }

            CableFrequency cableFrequency = currentFrequency as CableFrequency;
            if (cableFrequency != null)
            {
                cableFrequency.Provider = CableProvider.FindProvider(fileName);
                if (runType != RunType.Centre || cableFrequency.Provider != null)
                    return (ExitCode.OK);
                else
                {
                    lastError = "INI file format error: A TuningFile parameter references an unknown tuning file - " + fileName + ".";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }

            AtscFrequency atscFrequency = currentFrequency as AtscFrequency;
            if (atscFrequency != null)
            {
                atscFrequency.Provider = AtscProvider.FindProvider(fileName);
                if (runType != RunType.Centre || atscFrequency.Provider != null)
                    return (ExitCode.OK);
                else
                {
                    lastError = "INI file format error: A TuningFile parameter references an unknown tuning file - " + fileName + ".";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }

            ClearQamFrequency clearQamFrequency = currentFrequency as ClearQamFrequency;
            if (clearQamFrequency != null)
            {
                clearQamFrequency.Provider = ClearQamProvider.FindProvider(fileName);
                if (runType != RunType.Centre || clearQamFrequency.Provider != null)
                    return (ExitCode.OK);
                else
                {
                    lastError = "INI file format error: A TuningFile parameter references an unknown tuning file - " + fileName + ".";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }

            ISDBSatelliteFrequency isdbSatelliteFrequency = currentFrequency as ISDBSatelliteFrequency;
            if (isdbSatelliteFrequency != null)
            {
                isdbSatelliteFrequency.Provider = ISDBSatelliteProvider.FindProvider(fileName);
                if (runType != RunType.Centre || isdbSatelliteFrequency.Provider != null)
                    return (ExitCode.OK);
                else
                {
                    lastError = "INI file format error: A TuningFile parameter references an unknown tuning file - " + fileName + ".";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }

            ISDBTerrestrialFrequency isdbTerrestrialFrequency = currentFrequency as ISDBTerrestrialFrequency;
            if (isdbTerrestrialFrequency != null)
            {
                isdbTerrestrialFrequency.Provider = ISDBTerrestrialProvider.FindProvider(fileName);
                if (runType != RunType.Centre || isdbTerrestrialFrequency.Provider != null)
                    return (ExitCode.OK);
                else
                {
                    lastError = "INI file format error: A TuningFile parameter references an unknown tuning file - " + fileName + ".";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }

            lastError = "INI file format error: Internal error - Frequency not recognised while processing tuning file " + fileName + ".";
            Logger.Instance.Write(lastError);
            return (ExitCode.ParameterError);
        }

        private ExitCode processScannedChannel(string parts)
        {
            string[] parameters = parts.Split(new char[] { ',' });
            if (parameters.Length != 5)
            {
                lastError = "INI file format error: A Scanned line has the wrong number of parameters (5).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                int originalNetworkID = Int32.Parse(parameters[0].Trim());
                int transportStreamID = Int32.Parse(parameters[1].Trim());
                int serviceID = Int32.Parse(parameters[2].Trim());

                TVStation station = TVStation.FindStation(StationCollection, originalNetworkID, transportStreamID, serviceID);
                if (station != null)
                    station.Name = parameters[4].Trim().Replace("%%", ",");
                else
                {
                    TVStation newStation = new TVStation(parameters[4].Trim().Replace("%%", ","));
                    newStation.OriginalNetworkID = originalNetworkID;
                    newStation.TransportStreamID = transportStreamID;
                    newStation.ServiceID = serviceID;
                    newStation.CreatedFromIni = true;
                    TVStation.AddStation(StationCollection, newStation);
                }
            }
            catch (FormatException)
            {
                lastError = "INI file format error: A Scanned line has a parameter in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: A Scanned line has a parameter out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processOffsetChannel(string parts)
        {
            string[] parameters = parts.Split(new char[] { ',' });
            if (parameters.Length != 9)
            {
                lastError = "INI file format error: An Offset line has the wrong number of parameters (9).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                TVStation sourceChannel = new TVStation(parameters[0].Trim().Replace("%%", ","));
                sourceChannel.OriginalNetworkID = Int32.Parse(parameters[1].Trim());
                sourceChannel.TransportStreamID = Int32.Parse(parameters[2].Trim());
                sourceChannel.ServiceID = Int32.Parse(parameters[3].Trim());

                TVStation destinationChannel = new TVStation(parameters[4].Trim().Replace("%%", ","));
                destinationChannel.OriginalNetworkID = Int32.Parse(parameters[5].Trim());
                destinationChannel.TransportStreamID = Int32.Parse(parameters[6].Trim());
                destinationChannel.ServiceID = Int32.Parse(parameters[7].Trim());

                TimeOffsetChannel channel = new TimeOffsetChannel(sourceChannel, destinationChannel, Int32.Parse(parameters[8].Trim()));
                TimeOffsetChannels.Add(channel);
            }
            catch (FormatException)
            {
                lastError = "INI file format error: An Offset line has a parameter in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: An Offset line has a parameter out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processIncludeService(string parts)
        {
            string[] parameters = parts.Split(new char[] { ',' });
            if (parameters.Length > 5)
            {
                lastError = "INI file format error: An ExcludeService line has the wrong number of parameters (5).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                int originalNetworkID = Int32.Parse(parameters[0]);
                int transportStreamID = -1;
                int startServiceID = -1;
                int endServiceID = -1;
                int frequency = -1;

                if (parameters.Length > 1)
                    transportStreamID = Int32.Parse(parameters[1]);
                if (parameters.Length > 2)
                    startServiceID = Int32.Parse(parameters[2]);
                if (parameters.Length > 3)
                    endServiceID = Int32.Parse(parameters[3]);
                if (parameters.Length > 4)
                    frequency = Int32.Parse(parameters[4]);

                ChannelFilters.Add(new ChannelFilterEntry(frequency, originalNetworkID, transportStreamID, startServiceID, endServiceID));
            }
            catch (FormatException)
            {
                lastError = "INI file format error: An ExcludeService line has a parameter in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: An ExcludeService line has a parameter out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processMaxService(string parts)
        {
            try
            {
                maxService = Int32.Parse(parts.Trim());
            }
            catch (FormatException)
            {
                lastError = "INI file format error: The MaxService line has a parameter in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: The MaxService line has a parameter out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processRepeatExclusion(string parts)
        {
            string[] parameters = parts.Split(new char[] { ',' });
            if (parameters.Length != 2)
            {
                lastError = "INI file format error: A RepeatExclusion line has the wrong number of parameters (2).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            Exclusions.Add(new RepeatExclusion(parameters[0], parameters[1]));
            return (ExitCode.OK);
        }

        private ExitCode processRepeatPhrase(string parts)
        {
            PhrasesToIgnore.Add(parts.Trim());
            return (ExitCode.OK);
        }

        private ExitCode processWMCImportName(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The WMCImportName line has the wrong number of parameters (1).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            wmcImportName = parts.Trim();
            return (ExitCode.OK);
        }

        private ExitCode processEPGDays(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The EPGDays line has the wrong number of parameters (1).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                int epgDays = Int32.Parse(parts);
                if (epgDays == 0)
                {
                    lastError = "INI file format error: The EPGDays line parameter cannot be zero.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
                else
                {
                    if (currentFrequency != null)
                        currentFrequency.AdvancedRunParamters.EPGDays = epgDays;
                    else
                        this.epgDays = epgDays;
                }
            }
            catch (FormatException)
            {
                lastError = "INI file format error: The EPGDays line has a parameter in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: The EPGDays line has a parameter out of range.";
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processMovieLookupEnabled(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The MovieLookupEnabled line parameter is missing).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (parts.Trim().ToLowerInvariant() == "yes")
                movieLookupEnabled = true;
            else
            {
                if (parts.Trim().ToLowerInvariant() == "no")
                    movieLookupEnabled = false;
                else
                {
                    lastError = "INI file format error: The MovieLookupEnabled line parameter is incorrect.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }

            return (ExitCode.OK);
        }

        private ExitCode processMovieImage(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The MovieImage line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (parts.Trim().ToLowerInvariant() == "thumbnail")
                downloadMovieThumbnail = LookupImageType.Thumbnail;
            else
            {
                if (parts.Trim().ToLowerInvariant() == "poster")
                    downloadMovieThumbnail = LookupImageType.Poster;
                else
                {
                    if (parts.Trim().ToLowerInvariant() == "none")
                        downloadMovieThumbnail = LookupImageType.None;
                    else
                    {
                        lastError = "INI file format error: The MovieImage line parameter is incorrect.";
                        return (ExitCode.ParameterError);
                    }
                }
            }

            return (ExitCode.OK);
        }

        private ExitCode processMovieLowDuration(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The MovieLowDuration line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                movieLowTime = Int32.Parse(parts.Trim());
            }
            catch (FormatException)
            {
                lastError = "INI file format error: The MovieLowDuration line parameter is incorrect.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: The MovieLowDuration line parameter is incorrect.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processMovieHighDuration(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The MovieHighDuration line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                movieHighTime = Int32.Parse(parts.Trim());
            }
            catch (FormatException)
            {
                lastError = "INI file format error: The MovieHighDuration line parameter is incorrect.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: The MovieHighDuration line parameter is incorrect.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processTVLookupEnabled(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The TVLookupEnabled line has the wrong number of parameters (1).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (parts.Trim().ToLowerInvariant() == "yes")
                tvLookupEnabled = true;
            else
            {
                if (parts.Trim().ToLowerInvariant() == "no")
                    tvLookupEnabled = false;
                else
                {
                    lastError = "INI file format error: The TVLookupEnabled line parameter is incorrect.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }

            return (ExitCode.OK);
        }

        private ExitCode processTVImage(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The TVImage line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            string imageType = parts.Trim().ToLowerInvariant();

            switch (imageType)
            {
                case "poster":
                    downloadTVThumbnail = LookupImageType.Poster;
                    break;
                case "banner":
                    downloadTVThumbnail = LookupImageType.Banner;
                    break;
                case "fanart":
                    downloadTVThumbnail = LookupImageType.Fanart;
                    break;
                case "smallposter":
                    downloadTVThumbnail = LookupImageType.SmallPoster;
                    break;
                case "smallfanart":
                    downloadTVThumbnail = LookupImageType.SmallFanart;
                    break;
                case "none":
                    downloadTVThumbnail = LookupImageType.None;
                    break;
                default:
                    lastError = "INI file format error: The TVImage line parameter is incorrect.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processLookupMatching(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The LookupMatching line parameter is missing."; 
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                LookupMatching = (MatchMethod)Enum.Parse(typeof(MatchMethod), parts);
            }
            catch (ArgumentException)
            {
                lastError = "INI file format error: The LookupMatching line parameter is incorrect.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: The LookupMatching line parameter is incorrect.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processLookupNotFound(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The LookupNotFound line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (parts.Trim().ToLowerInvariant() == "yes")
                LookupNotFound = true;
            else
            {
                if (parts.Trim().ToLowerInvariant() == "no")
                    LookupNotFound = false;
                else
                {
                    lastError = "INI file format error: The LookupNotFound line is incorrect.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }

            return (ExitCode.OK);
        }

        private ExitCode processLookupErrors(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The LookupErrors line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                lookupErrorLimit = Int32.Parse(parts.Trim());
            }
            catch (FormatException)
            {
                lastError = "INI file format error: The LookupErrors parameter is in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: The LookupErrors parameter is out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processLookupTimeLimit(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The LookupTimeLimit line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                lookupTimeLimit = Int32.Parse(parts.Trim());
            }
            catch (FormatException)
            {
                lastError = "INI file format error: The LookupTimeLimit line parameter is in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: The LookupTimeLimit line parameter is out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processLookupIgnoredPhrases(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The LookupIgnoredPhrases line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            string[] parameters = parts.Split(new string[] { lookupIgnoredPhraseSeparator }, StringSplitOptions.None);

            foreach (string parameter in parameters)
                lookupIgnoredPhrases.Add(parameter);

            return (ExitCode.OK);
        }

        private ExitCode processLookupMoviePhrases(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The LookupMoviePhrases line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            string[] parameters = parts.Split(new string[] { moviePhraseSeparator }, StringSplitOptions.None);

            foreach (string parameter in parameters)
                lookupMoviePhrases.Add(parameter);

            return (ExitCode.OK);
        }

        private ExitCode processLookupReload(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The LookupReload line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (parts.Trim().ToLowerInvariant() == "yes")
                LookupReload = true;
            else
            {
                if (parts.Trim().ToLowerInvariant() == "no")
                    LookupReload = false;
                else
                {
                    lastError = "INI file format error: The LookupReload line parameter is incorrect.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }

            return (ExitCode.OK);
        }

        private ExitCode processLookupIgnoreCategories(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The LookupIgnoreCategories line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (parts.Trim().ToLowerInvariant() == "yes")
                LookupIgnoreCategories = true;
            else
            {
                if (parts.Trim().ToLowerInvariant() == "no")
                    LookupIgnoreCategories = false;
                else
                {
                    lastError = "INI file format error: The LookupIgnoreCategories line parameter is incorrect.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }

            return (ExitCode.OK);
        }

        private ExitCode processLookupProcessAsTVSeries(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The LookupProcessAsTVSeries line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (parts.Trim().ToLowerInvariant() == "yes")
                LookupProcessAsTVSeries = true;
            else
            {
                if (parts.Trim().ToLowerInvariant() == "no")
                    LookupProcessAsTVSeries = false;
                else
                {
                    lastError = "INI file format error: The LookupProcessAsTVSeries line parameter is incorrect.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }

            return (ExitCode.OK);
        }

        private ExitCode processLookupImagePath(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The LookupImagePath line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            lookupImagePath = parts.Trim();

            return (ExitCode.OK);
        }

        private ExitCode processLookupXmltvImageTagPath(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The LookupXmltvImageTagPath line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            lookupXmltvImageTagPath = parts.Trim();

            return (ExitCode.OK);
        }

        private ExitCode processLookupIgnoredPhraseSeparator(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The LookupIgnoredPhraseSeparator line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            lookupIgnoredPhraseSeparator = parts.Trim();

            return (ExitCode.OK);
        }

        private ExitCode processMoviePhraseSeparator(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The MoviePhraseSeparator line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            moviePhraseSeparator = parts.Trim();

            return (ExitCode.OK);
        }

        private ExitCode processLookupNotMovie(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The LookupNotMovie line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (lookupNotMovie == null)
                lookupNotMovie = new Collection<string>();
            lookupNotMovie.Add(parts.Trim());

            return (ExitCode.OK);
        }

        private ExitCode processLookupImageNameTitle(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The LookupImageNameTitle line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (parts.Trim().ToLowerInvariant() == "yes")
                LookupImageNameTitle = true;
            else
            {
                if (parts.Trim().ToLowerInvariant() == "no")
                    LookupImageNameTitle = false;
                else
                {
                    lastError = "INI file format error: The LookupImageNameTitle line parameter is incorrect.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }

            return (ExitCode.OK);
        }

        private ExitCode processLookupImagesInBase(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The LookupImagesInBase line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (parts.Trim().ToLowerInvariant() == "yes")
                LookupImagesInBase = true;
            else
            {
                if (parts.Trim().ToLowerInvariant() == "no")
                    LookupImagesInBase = false;
                else
                {
                    lastError = "INI file format error: The LookupImagesInBase line parameter is incorrect.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }

            return (ExitCode.OK);
        }

        private ExitCode processChannelUpdateEnabled(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The ChannelUpdateEnabled line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (parts.Trim().ToLowerInvariant() == "yes")
                channelUpdateEnabled = true;
            else
            {
                if (parts.Trim().ToLowerInvariant() == "no")
                    channelUpdateEnabled = false;
                else
                {
                    lastError = "INI file format error: The ChannelUpdateEnabled line parameter is incorrect.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }

            return (ExitCode.OK);
        }

        private ExitCode processChannelMergeMethod(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The ChannelMergeMethod line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (parts.Trim().ToLowerInvariant() == "name")
                channelMergeMethod = ChannelMergeMethod.Name;
            else
            {
                if (parts.Trim().ToLowerInvariant() == "number")
                    channelMergeMethod = ChannelMergeMethod.Number;
                else
                {
                    if (parts.Trim().ToLowerInvariant() == "namenumber")
                        channelMergeMethod = ChannelMergeMethod.NameNumber;
                    else
                    {
                        if (parts.Trim().ToLowerInvariant() == "none")
                            channelMergeMethod = ChannelMergeMethod.None;
                        else
                        {
                            lastError = "INI file format error: The ChannelMergeMethod line parameter is incorrect.";
                            Logger.Instance.Write(lastError);
                            return (ExitCode.ParameterError);
                        }
                    }
                }
            }

            return (ExitCode.OK);
        }

        private ExitCode processChannelEPGScanner(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The ChannelEPGScanner line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (parts.Trim().ToLowerInvariant() == "epg collector")
                channelEPGScanner = ChannelEPGScanner.EPGCollector;
            else
            {
                if (parts.Trim().ToLowerInvariant() == "eit scanner")
                    channelEPGScanner = ChannelEPGScanner.EITScanner;
                else
                {
                    if (parts.Trim().ToLowerInvariant() == "none")
                        channelEPGScanner = ChannelEPGScanner.None;
                    else
                    {
                        if (parts.Trim().ToLowerInvariant() == "default")
                            channelEPGScanner = ChannelEPGScanner.Default;
                        else
                        {
                            if (parts.Trim().ToLowerInvariant() == "xmltv")
                                channelEPGScanner = ChannelEPGScanner.Xmltv;
                            else
                            {
                                lastError = "INI file format error: The ChannelEPGScanner line parameter is incorrect.";
                                Logger.Instance.Write(lastError);
                                return (ExitCode.ParameterError);
                            }
                        }
                    }
                }
            }

            return (ExitCode.OK);
        }

        private ExitCode processChannelChildLock(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The ChannelChildLock line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (parts.Trim().ToLowerInvariant() == "yes")
                channelChildLock = true;
            else
            {
                if (parts.Trim().ToLowerInvariant() == "no")
                    channelChildLock = false;
                else
                {
                    lastError = "INI file format error: The ChannelChildLock line parameter is incorrect.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }

            return (ExitCode.OK);
        }

        private ExitCode processChannelLogNetworkMap(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The ChannelLogNetworkMap line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (parts.Trim().ToLowerInvariant() == "yes")
                channelLogNetworkMap = true;
            else
            {
                if (parts.Trim().ToLowerInvariant() == "no")
                    channelLogNetworkMap = false;
                else
                {
                    lastError = "INI file format error: The ChannelLogNetworkMap line parameter is incorrect.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }

            return (ExitCode.OK);
        }

        private ExitCode processChannelEPGScanInterval(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The ChannelEPGScanInterval line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                channelEPGScanInterval = Int32.Parse(parts.Trim());
            }
            catch (FormatException)
            {
                lastError = "INI file format error: The ChannelEPGScanInterval line parameter format is incorrect.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: The ChannelEPGScanInterval line parameter is out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processChannelReloadData(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The ChannelReloadData line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (parts.Trim().ToLowerInvariant() == "yes")
                channelReloadData = true;
            else
            {
                if (parts.Trim().ToLowerInvariant() == "no")
                    channelReloadData = false;
                else
                {
                    lastError = "INI file format error: The ChannelReloadData line parameter is incorrect.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }

            return (ExitCode.OK);
        }

        private ExitCode processChannelUpdateNumber(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The ChannelUpdateNumber line parameter is missing."; 
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (parts.Trim().ToLowerInvariant() == "yes")
                channelUpdateNumber = true;
            else
            {
                if (parts.Trim().ToLowerInvariant() == "no")
                    channelUpdateNumber = false;
                else
                {
                    lastError = "INI file format error: The ChannelUpdateNumber line parameter is incorrect.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }

            return (ExitCode.OK);
        }

        private ExitCode processChannelExcludeNew(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The ChannelExcludeNew line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (parts.Trim().ToLowerInvariant() == "yes")
                channelExcludeNew = true;
            else
            {
                if (parts.Trim().ToLowerInvariant() == "no")
                    channelExcludeNew = false;
                else
                {
                    lastError = "INI file format error: The ChannelExcludeNew line parameter is incorrect.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }

            return (ExitCode.OK);
        }

        private ExitCode processXmltvFile(string parts)
        {
            string[] parameters = parts.Split(new char[] { ',' });
            if (parameters.Length < 2 || parameters.Length > 6)
            {
                lastError = "INI file format error: An XmltvFile line has the wrong number of parameters (2-4).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            ImportFileSpec importFileSpec = new ImportFileSpec(parameters[0].Trim().Replace("<ApplicationData>", RunParameters.DataDirectory));

            try
            {
                importFileSpec.Precedence = (DataPrecedence)Enum.Parse(typeof(DataPrecedence), parameters[1].Trim());
            }
            catch (ArgumentException)
            {
                lastError = "INI file format error: An XmltvFile line parameter is incorrect.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (parameters.Length > 2)
            {
                if (!string.IsNullOrWhiteSpace(parameters[2].Trim()))
                {
                    importFileSpec.Language = LanguageCode.FindLanguageCode(parameters[2].Trim());
                    if (importFileSpec.Language == null)
                    {
                        lastError = "INI file format error: An XmltvFile line language code not recognised).";
                        Logger.Instance.Write(lastError);
                        return (ExitCode.ParameterError);
                    }
                }
            }

            if (parameters.Length > 3)
            {
                try
                {
                    importFileSpec.NoLookup = bool.Parse(parameters[3].Trim());
                }
                catch (ArgumentException)
                {
                    lastError = "INI file format error: An XmltvFile line lookup parameter is incorrect.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }

            if (parameters.Length > 4)
            {
                try
                {
                    importFileSpec.IdFormat = (XmltvIdFormat)Enum.Parse(typeof(XmltvIdFormat), parameters[4].Trim(), true);
                }
                catch (ArgumentException)
                {
                    lastError = "INI file format error: An XmltvFile line ID format parameter is incorrect.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }

            if (parameters.Length > 5)
            {
                try
                {
                    importFileSpec.AppendOnly = bool.Parse(parameters[5].Trim());
                }
                catch (ArgumentException)
                {
                    lastError = "INI file format error: An XmltvFile line lookup parameter is incorrect.";
                    Logger.Instance.Write(lastError);
                    return (ExitCode.ParameterError);
                }
            }

            if (importFiles == null)
                importFiles = new Collection<ImportFileSpec>();

            importFiles.Add(importFileSpec);

            return (ExitCode.OK);

        }

        private ExitCode processXmltvChannelChange(string parts)
        {
            string[] parameters = parts.Split(new char[] { ',' });
            if (parameters.Length < 3 || parameters.Length > 4)
            {
                lastError = "INI file format error: An XmltvChannelChange line has the wrong number of parameters (3 or 4).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            ImportChannelChange xmltvChannelChange = new ImportChannelChange(parameters[0].Trim());

            try
            {
                xmltvChannelChange.ChannelNumber = Int32.Parse(parameters[1].Trim());
                xmltvChannelChange.Excluded = bool.Parse(parameters[2].Trim());

                if (parameters.Length == 4)
                    xmltvChannelChange.NewName = parameters[3].Trim();
            }
            catch (ArgumentException)
            {
                lastError = "INI file format error: An XmltvChannelChange line parameter is incorrect.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (importChannelChanges == null)
                importChannelChanges = new Collection<ImportChannelChange>();

            importChannelChanges.Add(xmltvChannelChange);

            return (ExitCode.OK);
        }

        private ExitCode processEditSpec(string parts)
        {
            string[] parameters = parts.Split(new char[] { ',' });
            if (parameters.Length < 5 || parameters.Length > 6)
            {
                lastError = "INI file format error: An EditSpec line has the wrong number of parameters (5 or 6).";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            EditSpec editSpec = null;

            try
            {
                editSpec = new EditSpec(parameters[0].Trim(), (TextLocation)Enum.Parse(typeof(TextLocation), parameters[4].Trim(), true), parameters[1].Trim());
                editSpec.ApplyToTitles = bool.Parse(parameters[2].Trim());
                editSpec.ApplyToDescriptions = bool.Parse(parameters[3].Trim());

                if (parameters.Length == 6)
                {
                    switch (parameters[5].Trim().ToLowerInvariant())
                    {
                        case "text":
                            editSpec.ReplacementMode = TextReplacementMode.TextOnly;
                            break;
                        case "following":
                            editSpec.ReplacementMode = TextReplacementMode.TextAndFollowing;
                            break;
                        case "preceeding":
                            editSpec.ReplacementMode = TextReplacementMode.TextAndPreceeding;
                            break;
                        case "everything":
                            editSpec.ReplacementMode = TextReplacementMode.Everything;
                            break;
                        default:
                            lastError = "INI file format error: An EditSpec line parameter is incorrect.";
                            Logger.Instance.Write(lastError);
                            return (ExitCode.ParameterError);
                    }
                }
            }
            catch (ArgumentException)
            {
                lastError = "INI file format error: An EditSpec line parameter is incorrect.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (editSpecs == null)
                editSpecs = new Collection<EditSpec>();

            editSpecs.Add(editSpec);

            return (ExitCode.OK);
        }

        private ExitCode processDVBViewerIPAddress(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The DVBViewerIPAddress line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            dvbviewerIPAddress = parts.Trim();
            return (ExitCode.OK);
        }

        private ExitCode processBladeRunnerFileName(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The BladeRunnerFileName line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            bladeRunnerFileName = parts.Trim();
            return (ExitCode.OK);
        }

        private ExitCode processAreaRegionFileName(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The AreaRegionFileName line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            areaRegionFileName = parts.Trim();
            return (ExitCode.OK);
        }

        private ExitCode processSageTVFileName(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The SageTVFileName line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            sageTVFileName = parts.Trim();
            return (ExitCode.OK);
        }

        private ExitCode processSageTVSatNum(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The SageTVSatNum line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                sageTVSatelliteNumber = Int32.Parse(parts.Trim());
            }
            catch (FormatException)
            {
                lastError = "INI file format error: The SageTVSatNum line parameter is in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: The SageTVSatNum line parameter is out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processSatIpTuner(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The SatIpTuner line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            SatIpUniqueIdentifiers.Add(parts.Trim());
            return (ExitCode.OK);
        }

        private ExitCode processSatIpFrontend(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The SatIpFrontend line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            try
            {
                currentFrequency.SatIpFrontend = Int32.Parse(parts.Trim());
            }
            catch (FormatException)
            {
                lastError = "INI file format error: The SatIPFrontend line parameter is in the wrong format.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }
            catch (OverflowException)
            {
                lastError = "INI file format error: The SatIPFrontend line parameter is out of range.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            return (ExitCode.OK);
        }

        private ExitCode processByteConvertTable(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The ByteConvertTable line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (currentFrequency != null)
                currentFrequency.AdvancedRunParamters.ByteConvertTable = parts.Trim();
            else
                byteConvertTable = parts.Trim();
            
            return (ExitCode.OK);
        }

        private ExitCode processEITCarousel(string parts)
        {
            if (string.IsNullOrWhiteSpace(parts.Trim()))
            {
                lastError = "INI file format error: The EITCarousel line parameter is missing.";
                Logger.Instance.Write(lastError);
                return (ExitCode.ParameterError);
            }

            if (currentFrequency != null)
                currentFrequency.AdvancedRunParamters.EITCarousel = parts.Trim();
            else
                eitCarousel = parts.Trim();

            return (ExitCode.OK);
        }

        /// <summary>
        /// Write the current parameter set to a file.
        /// </summary>
        /// <param name="fileName">The full name of the file.</param>
        /// <returns>Null if output was successful; a message identifying the error otherwise.</returns>
        public string Save(string fileName)
        {
            Logger.Instance.Write("Saving collection parameter file to " + fileName);

            try
            {
                if (File.Exists(fileName))
                {
                    if (File.Exists(fileName + ".bak"))
                    {
                        File.SetAttributes(fileName + ".bak", FileAttributes.Normal);
                        File.Delete(fileName + ".bak");
                    }

                    File.Copy(fileName, fileName + ".bak");
                    File.SetAttributes(fileName + ".bak", FileAttributes.ReadOnly);

                    File.SetAttributes(fileName, FileAttributes.Normal);
                }

                FileStream fileStream = new FileStream(fileName, FileMode.Create);
                StreamWriter streamWriter = new StreamWriter(fileStream);

                outputGeneralParameters(streamWriter);
                outputDiagnosticParameters(streamWriter);

                if (parameterSet == ParameterSet.Collector)
                {
                    foreach (TuningFrequency tuningFrequency in FrequencyCollection)
                        outputFrequencyParameters(streamWriter, tuningFrequency);
                }
                else
                {
                    if (parameterSet == ParameterSet.Plugin)
                    {
                        outputPluginFrequencyParameters(streamWriter, FrequencyCollection[0]);
                        FrequencyCollection[0].AdvancedRunParamters.OutputParameters(streamWriter);
                    }
                }

                outputStationParameters(streamWriter);
                outputScanListParameters(streamWriter);
                outputTimeOffsetParameters(streamWriter);
                outputServiceFilterParameters(streamWriter);
                outputRepeatExclusionParameters(streamWriter);
                outputLookupParameters(streamWriter);
                outputChannelUpdateParameters(streamWriter);
                outputXmltvFileParameters(streamWriter);
                outputEditParameters(streamWriter);

                streamWriter.Close();
                fileStream.Close();

                return (null);
            }
            catch (IOException e)
            {
                return (e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                return (e.Message);
            }
        }

        private void outputGeneralParameters(StreamWriter streamWriter)
        {
            streamWriter.WriteLine("[GENERAL]");

            if (OutputFileSet)
                streamWriter.WriteLine("Output=" + outputFileName);

            streamWriter.WriteLine(("Timeouts=" + LockTimeout.TotalSeconds + "," +
                FrequencyTimeout.TotalSeconds + "," +
                Repeats + "," +
                BufferFills + "," +
                BufferSize).ToString(CultureInfo.InvariantCulture));

            if (timeZoneSet)
            {
                streamWriter.WriteLine("Timezone=" + timeZone.Hours.ToString("00") + "." + timeZone.Minutes.ToString("00") + "," +
                    nextTimeZone.Hours.ToString("00") + "." + nextTimeZone.Minutes.ToString("00") + "," +
                    nextTimeZoneChange.ToString("dd/MM/yy") + "," +
                    nextTimeZoneChange.TimeOfDay.Hours.ToString("00") + "." + nextTimeZoneChange.TimeOfDay.Minutes.ToString("00"));
            }

            if (dvbviewerIPAddress != null)
                streamWriter.WriteLine("DVBViewerIPAddress=" + dvbviewerIPAddress);

            if (bladeRunnerFileName != null)
                streamWriter.WriteLine("BladeRunnerFileName=" + bladeRunnerFileName);
            if (areaRegionFileName != null)
                streamWriter.WriteLine("AreaRegionFileName=" + areaRegionFileName);
            if (sageTVFileName != null)
                streamWriter.WriteLine("SageTVFileName=" + sageTVFileName);
            if (sageTVSatelliteNumber != -1)
                streamWriter.WriteLine("SageTVSatNum=" + sageTVSatelliteNumber);

            if (options.Count != 0)
            {
                streamWriter.Write("Option=");

                bool first = true;

                foreach (OptionEntry optionEntry in options)
                {
                    if (!first)
                        streamWriter.Write(",");
                    streamWriter.Write(optionEntry.ToString());
                    first = false;
                }

                streamWriter.WriteLine();
            }

            if (wmcImportName != null)
                streamWriter.WriteLine("WMCImportName=" + wmcImportName);
        }

        private void outputDiagnosticParameters(StreamWriter streamWriter)
        {
            if (traceIDs.Count == 0 && debugIDs.Count == 0)
                return;

            streamWriter.WriteLine();
            streamWriter.WriteLine("[DIAGNOSTICS]");

            if (traceIDs.Count != 0)
            {
                streamWriter.Write("Trace=");

                foreach (TraceEntry traceEntry in traceIDs)
                {
                    if (traceIDs.IndexOf(traceEntry) == 0)
                        streamWriter.Write(traceEntry.ToString());
                    else
                        streamWriter.Write("," + traceEntry.ToString());
                }

                streamWriter.WriteLine();
            }

            if (debugIDs.Count != 0)
            {
                streamWriter.Write("Debug=");

                foreach (DebugEntry debugEntry in debugIDs)
                {
                    if (debugIDs.IndexOf(debugEntry) == 0)
                        streamWriter.Write(debugEntry.ToString());
                    else
                        streamWriter.Write("," + debugEntry.ToString());
                }

                streamWriter.WriteLine();
            }
        }

        private void outputFrequencyParameters(StreamWriter streamWriter, TuningFrequency tuningFrequency)
        {
            switch (tuningFrequency.TunerType)
            {
                case TunerType.Satellite:
                    outputSatelliteFrequency(streamWriter, tuningFrequency);
                    break;
                case TunerType.Terrestrial:
                    outputTerrestrialFrequency(streamWriter, tuningFrequency);
                    break;
                case TunerType.Cable:
                    outputCableFrequency(streamWriter, tuningFrequency);
                    break;
                case TunerType.ATSC:
                case TunerType.ATSCCable:
                    outputAtscFrequency(streamWriter, tuningFrequency);
                    break;
                case TunerType.ClearQAM:
                    outputClearQamFrequency(streamWriter, tuningFrequency);
                    break;
                case TunerType.ISDBS:
                    outputISDBSatelliteFrequency(streamWriter, tuningFrequency);
                    break;
                case TunerType.ISDBT:
                    outputISDBTerrestrialFrequency(streamWriter, tuningFrequency);
                    break;
                case TunerType.File:
                    outputFileFrequency(streamWriter, tuningFrequency);
                    break;
                case TunerType.Stream:
                    outputStreamFrequency(streamWriter, tuningFrequency);
                    break;
                default:
                    break;
            }

            tuningFrequency.AdvancedRunParamters.OutputParameters(streamWriter);
        }

        private void outputSatelliteFrequency(StreamWriter streamWriter, TuningFrequency tuningFrequency)
        {
            streamWriter.WriteLine();
            streamWriter.WriteLine("[DVBS]");

            streamWriter.WriteLine("Satellite=" + (tuningFrequency.Provider as Satellite).Longitude);

            SatelliteFrequency satelliteFrequency = tuningFrequency as SatelliteFrequency;

            if (satelliteFrequency.DiseqcRunParamters.DiseqcSwitch != null)
            {
                if (!satelliteFrequency.LNBConversion)
                    streamWriter.WriteLine("Dish=" + satelliteFrequency.SatelliteDish.LNBLowBandFrequency + "," +
                        satelliteFrequency.SatelliteDish.LNBHighBandFrequency + "," +
                        satelliteFrequency.SatelliteDish.LNBSwitchFrequency + "," +
                        satelliteFrequency.DiseqcRunParamters.DiseqcSwitch);
                else
                    streamWriter.WriteLine("Dish=" + satelliteFrequency.SatelliteDish.LNBLowBandFrequency + "," +
                        satelliteFrequency.SatelliteDish.LNBHighBandFrequency + "," +
                        satelliteFrequency.SatelliteDish.LNBSwitchFrequency + "," +
                        satelliteFrequency.DiseqcRunParamters.DiseqcSwitch + "," +
                        satelliteFrequency.SatelliteDish.LNBType.Type);
            }
            else
            {
                if (!satelliteFrequency.LNBConversion)
                    streamWriter.WriteLine("Dish=" + satelliteFrequency.SatelliteDish.LNBLowBandFrequency + "," +
                        satelliteFrequency.SatelliteDish.LNBHighBandFrequency + "," +
                        satelliteFrequency.SatelliteDish.LNBSwitchFrequency);
                else
                    streamWriter.WriteLine("Dish=" + satelliteFrequency.SatelliteDish.LNBLowBandFrequency + "," +
                    satelliteFrequency.SatelliteDish.LNBHighBandFrequency + "," +
                    satelliteFrequency.SatelliteDish.LNBSwitchFrequency + ",," +
                    satelliteFrequency.SatelliteDish.LNBType.Type);
            }

            streamWriter.WriteLine("TuningFile=" + satelliteFrequency.Provider + ".xml");

            if (satelliteFrequency.Pilot == SignalPilot.Pilot.NotSet && 
                satelliteFrequency.RollOff == SignalRollOff.RollOff.NotSet &&
                satelliteFrequency.Modulation == SignalModulation.Modulation.QPSK)
            {
                streamWriter.WriteLine("ScanningFrequency=" + satelliteFrequency.Frequency + "," +
                    satelliteFrequency.SymbolRate + "," +
                    satelliteFrequency.FEC + "," +
                    satelliteFrequency.Polarization.PolarizationAbbreviation + "," +
                    satelliteFrequency.CollectionType);
            }
            else
            {
                streamWriter.WriteLine("ScanningFrequency=" + satelliteFrequency.Frequency + "," +
                    satelliteFrequency.SymbolRate + "," +
                    satelliteFrequency.FEC + "," +
                    satelliteFrequency.Polarization.PolarizationAbbreviation + "," +
                    satelliteFrequency.Pilot + "," +
                    satelliteFrequency.RollOff + "," +
                    satelliteFrequency.Modulation + "," +
                    satelliteFrequency.CollectionType);
            }

            if (satelliteFrequency.SatIpFrontend != -1)
                streamWriter.WriteLine("SatIPFrontend=" + satelliteFrequency.SatIpFrontend);

            outputSelectedTuners(satelliteFrequency.SelectedTuners, streamWriter);            

            if (satelliteFrequency.DiseqcRunParamters.Options.Count != 0)
            {
                streamWriter.Write("DiseqcOption=");

                bool first = true;

                foreach (OptionEntry optionEntry in satelliteFrequency.DiseqcRunParamters.Options)
                {
                    if (!first)
                        streamWriter.Write(",");
                    streamWriter.Write(optionEntry.ToString());
                    first = false;
                }

                streamWriter.WriteLine();
            }

            if (satelliteFrequency.DiseqcRunParamters.DiseqcHandler != null && satelliteFrequency.DiseqcRunParamters.DiseqcHandler.ToUpperInvariant() != "DEFAULT")
                streamWriter.WriteLine("DiseqcHandler=" + satelliteFrequency.DiseqcRunParamters.DiseqcHandler);
        }

        private void outputTerrestrialFrequency(StreamWriter streamWriter, TuningFrequency tuningFrequency)
        {
            streamWriter.WriteLine();
            streamWriter.WriteLine("[DVBT]");

            TerrestrialFrequency terrestrialFrequency = tuningFrequency as TerrestrialFrequency;

            streamWriter.WriteLine("TuningFile=" + tuningFrequency.Provider + ".xml");

            if (terrestrialFrequency.PlpNumber == -1)
                streamWriter.WriteLine("ScanningFrequency=" + terrestrialFrequency.Frequency + "," +
                    terrestrialFrequency.Bandwidth + "," +
                    terrestrialFrequency.CollectionType);
            else
                streamWriter.WriteLine("ScanningFrequency=" + terrestrialFrequency.Frequency + "," +
                    terrestrialFrequency.Bandwidth + "," +
                    terrestrialFrequency.PlpNumber + "," +
                    terrestrialFrequency.CollectionType);

            if (terrestrialFrequency.SatIpFrontend != -1)
                streamWriter.WriteLine("SatIPFrontend=" + terrestrialFrequency.SatIpFrontend);

            outputSelectedTuners(terrestrialFrequency.SelectedTuners, streamWriter);  
        }

        private void outputCableFrequency(StreamWriter streamWriter, TuningFrequency tuningFrequency)
        {
            streamWriter.WriteLine();
            streamWriter.WriteLine("[DVBC]");

            CableFrequency cableFrequency = tuningFrequency as CableFrequency;

            streamWriter.WriteLine("TuningFile=" + tuningFrequency.Provider + ".xml");

            streamWriter.WriteLine("ScanningFrequency=" + cableFrequency.Frequency + "," +
                cableFrequency.SymbolRate + "," +
                cableFrequency.FEC + "," +
                cableFrequency.Modulation + "," +
                cableFrequency.CollectionType);

            if (cableFrequency.SatIpFrontend != -1)
                streamWriter.WriteLine("SatIPFrontend=" + cableFrequency.SatIpFrontend);

            outputSelectedTuners(cableFrequency.SelectedTuners, streamWriter);  
        }

        private void outputAtscFrequency(StreamWriter streamWriter, TuningFrequency tuningFrequency)
        {
            streamWriter.WriteLine();
            streamWriter.WriteLine("[ATSC]");

            AtscFrequency atscFrequency = tuningFrequency as AtscFrequency;

            streamWriter.WriteLine("TuningFile=" + tuningFrequency.Provider + ".xml");

            streamWriter.WriteLine("ScanningFrequency=" + atscFrequency.Frequency + "," +
                atscFrequency.ChannelNumber + "," +
                atscFrequency.SymbolRate + "," +
                atscFrequency.FEC + "," +
                atscFrequency.Modulation + "," +
                atscFrequency.CollectionType);

            outputSelectedTuners(atscFrequency.SelectedTuners, streamWriter);  
        }

        private void outputClearQamFrequency(StreamWriter streamWriter, TuningFrequency tuningFrequency)
        {
            streamWriter.WriteLine();
            streamWriter.WriteLine("[CLEARQAM]");

            ClearQamFrequency clearQamFrequency = tuningFrequency as ClearQamFrequency;

            streamWriter.WriteLine("TuningFile=" + tuningFrequency.Provider + ".xml");

            streamWriter.WriteLine("ScanningFrequency=" + clearQamFrequency.Frequency + "," +
                clearQamFrequency.ChannelNumber + "," +
                clearQamFrequency.SymbolRate + "," +
                clearQamFrequency.FEC + "," +
                clearQamFrequency.Modulation + "," +
                clearQamFrequency.CollectionType);

            outputSelectedTuners(clearQamFrequency.SelectedTuners, streamWriter);  
        }

        private void outputISDBSatelliteFrequency(StreamWriter streamWriter, TuningFrequency tuningFrequency)
        {
            streamWriter.WriteLine();
            streamWriter.WriteLine("[ISDBS]");

            streamWriter.WriteLine("Satellite=" + (tuningFrequency.Provider as ISDBSatelliteProvider).Longitude);

            ISDBSatelliteFrequency satelliteFrequency = tuningFrequency as ISDBSatelliteFrequency;

            if (satelliteFrequency.DiseqcRunParamters.DiseqcSwitch != null)
                streamWriter.WriteLine("Dish=" + satelliteFrequency.SatelliteDish.LNBLowBandFrequency + "," +
                    satelliteFrequency.SatelliteDish.LNBHighBandFrequency + "," +
                    satelliteFrequency.SatelliteDish.LNBSwitchFrequency + "," +
                    satelliteFrequency.DiseqcRunParamters.DiseqcSwitch);
            else
                streamWriter.WriteLine("Dish=" + satelliteFrequency.SatelliteDish.LNBLowBandFrequency + "," +
                    satelliteFrequency.SatelliteDish.LNBHighBandFrequency + "," +
                    satelliteFrequency.SatelliteDish.LNBSwitchFrequency);

            streamWriter.WriteLine("TuningFile=" + satelliteFrequency.Provider + ".xml");

            streamWriter.WriteLine("ScanningFrequency=" + satelliteFrequency.Frequency + "," +
                satelliteFrequency.SymbolRate + "," +
                satelliteFrequency.FEC + "," +
                satelliteFrequency.Polarization.PolarizationAbbreviation + "," +
                satelliteFrequency.CollectionType);

            outputSelectedTuners(satelliteFrequency.SelectedTuners, streamWriter);  
        }

        private void outputISDBTerrestrialFrequency(StreamWriter streamWriter, TuningFrequency tuningFrequency)
        {
            streamWriter.WriteLine();
            streamWriter.WriteLine("[ISDBT]");

            ISDBTerrestrialFrequency terrestrialFrequency = tuningFrequency as ISDBTerrestrialFrequency;

            streamWriter.WriteLine("TuningFile=" + tuningFrequency.Provider + ".xml");

            streamWriter.WriteLine("ScanningFrequency=" + terrestrialFrequency.ChannelNumber + "," +
                terrestrialFrequency.Frequency + "," +
                terrestrialFrequency.Bandwidth + "," +
                terrestrialFrequency.CollectionType);

            outputSelectedTuners(terrestrialFrequency.SelectedTuners, streamWriter);  
        }

        private void outputFileFrequency(StreamWriter streamWriter, TuningFrequency tuningFrequency)
        {
            streamWriter.WriteLine();
            streamWriter.WriteLine("[FILE]");

            FileFrequency fileFrequency = tuningFrequency as FileFrequency;

            streamWriter.WriteLine("ScanningFrequency=" + fileFrequency.Path + "," +
                fileFrequency.CollectionType);
        }

        private void outputStreamFrequency(StreamWriter streamWriter, TuningFrequency tuningFrequency)
        {
            streamWriter.WriteLine();
            streamWriter.WriteLine("[STREAM]");

            StreamFrequency streamFrequency = tuningFrequency as StreamFrequency;

            streamWriter.WriteLine("ScanningFrequency=" + streamFrequency.IPAddress + "," +
                streamFrequency.PortNumber + "," +
                (string.IsNullOrWhiteSpace(streamFrequency.MulticastSource) ? "" : streamFrequency.MulticastSource + ",") +
                (string.IsNullOrWhiteSpace(streamFrequency.MulticastSource) ? "" : (streamFrequency.MulticastPort == 0 ? "," : streamFrequency.MulticastPort + ",")) +
                streamFrequency.Protocol + "," +
                (streamFrequency.Path == null ? "," : streamFrequency.Path + ",") +
                streamFrequency.CollectionType);
        }

        private void outputSelectedTuners(Collection<SelectedTuner> selectedTuners, StreamWriter streamWriter)
        {
            if (selectedTuners.Count != 0)
            {
                foreach (SelectedTuner tuner in selectedTuners)
                {
                    if (!Tuner.TunerCollection[tuner.TunerNumber - 1].IsServerTuner)
                        streamWriter.WriteLine("SelectedTuner=" + tuner.TunerNumber);
                    else
                        streamWriter.WriteLine("SelectedTuner=" + tuner.UniqueIdentity);
                }
            }
        }

        private void outputPluginFrequencyParameters(StreamWriter streamWriter, TuningFrequency tuningFrequency)
        {
            streamWriter.WriteLine();
            streamWriter.WriteLine("[PLUGIN]");

            switch (tuningFrequency.TunerType)
            {
                case TunerType.Satellite:
                    streamWriter.WriteLine("PluginFrequency=Satellite," + tuningFrequency.Provider.ToString().Replace(',', '+') + "," +
                        tuningFrequency.Frequency + "," + (tuningFrequency as SatelliteFrequency).Polarization.PolarizationAbbreviation + "," +
                        tuningFrequency.CollectionType);
                    break;
                case TunerType.Terrestrial:
                    streamWriter.WriteLine("PluginFrequency=Terrestrial," + (tuningFrequency.Provider) + "," +
                        tuningFrequency.Frequency + ",," + tuningFrequency.CollectionType);
                    break;
                case TunerType.Cable:
                    streamWriter.WriteLine("PluginFrequency=Cable," + (tuningFrequency.Provider) + "," +
                        tuningFrequency.Frequency + ",," + tuningFrequency.CollectionType);
                    break;
                case TunerType.ATSC:
                case TunerType.ATSCCable:
                    streamWriter.WriteLine("PluginFrequency=ATSC," + (tuningFrequency.Provider) + "," +
                        tuningFrequency.Frequency + ",," + tuningFrequency.CollectionType);
                    break;
                case TunerType.ClearQAM:
                    streamWriter.WriteLine("PluginFrequency=ClearQAM," + (tuningFrequency.Provider) + "," +
                        tuningFrequency.Frequency + ",," + tuningFrequency.CollectionType);
                    break;
                case TunerType.ISDBS:
                    streamWriter.WriteLine("PluginFrequency=ISDBS," + (tuningFrequency.Provider) + "," +
                        tuningFrequency.Frequency + ",," + tuningFrequency.CollectionType);
                    break;
                case TunerType.ISDBT:
                    streamWriter.WriteLine("PluginFrequency=ISDBT," + (tuningFrequency.Provider) + "," +
                        tuningFrequency.Frequency + ",," + tuningFrequency.CollectionType);
                    break;
                default:
                    break;
            }
        }

        private void outputStationParameters(StreamWriter streamWriter)
        {
            if (StationCollection.Count == 0)
                return;

            bool first = true;

            foreach (TVStation station in StationCollection)
            {
                if (station.ExcludedByUser || (station.NewName != null && station.NewName.Trim() != string.Empty) || station.LogicalChannelNumber != -1)
                {
                    if (first)
                    {
                        streamWriter.WriteLine();
                        streamWriter.WriteLine("[STATIONS]");
                        first = false;
                    }

                    streamWriter.Write("Station=" + station.OriginalNetworkID + "," + station.TransportStreamID + "," + station.ServiceID);

                    if (!station.ExcludedByUser)
                    {
                        streamWriter.Write("," + station.LogicalChannelNumber);
                        if (station.NewName != null && station.NewName.Trim() != string.Empty)
                            streamWriter.Write("," + station.NewName.Replace(",", "%%"));
                    }

                    streamWriter.WriteLine();
                }
            }
        }

        private void outputScanListParameters(StreamWriter streamWriter)
        {
            if (StationCollection.Count == 0)
                return;

            streamWriter.WriteLine();
            streamWriter.WriteLine("[SCANLIST]");

            foreach (TVStation station in StationCollection)
                streamWriter.WriteLine("Scanned=" + station.OriginalNetworkID + "," +
                    station.TransportStreamID + "," +
                    station.ServiceID + "," +
                    station.LogicalChannelNumber + "," +
                    station.Name.Replace(",", "%%"));
        }

        private void outputTimeOffsetParameters(StreamWriter streamWriter)
        {
            if (TimeOffsetChannels.Count == 0)
                return;

            streamWriter.WriteLine();
            streamWriter.WriteLine("[OFFSETS]");

            foreach (TimeOffsetChannel timeOffsetChannel in TimeOffsetChannels)
                streamWriter.WriteLine("Offset=" + timeOffsetChannel.SourceChannel.Name.Replace(",", "%%") + "," +
                    timeOffsetChannel.SourceChannel.OriginalNetworkID + "," +
                    timeOffsetChannel.SourceChannel.TransportStreamID + "," +
                    timeOffsetChannel.SourceChannel.ServiceID + "," +
                    timeOffsetChannel.DestinationChannel.Name.Replace(",", "%%") + "," +
                    timeOffsetChannel.DestinationChannel.OriginalNetworkID + "," +
                    timeOffsetChannel.DestinationChannel.TransportStreamID + "," +
                    timeOffsetChannel.DestinationChannel.ServiceID + "," +
                    timeOffsetChannel.Offset
                    );
        }

        private void outputServiceFilterParameters(StreamWriter streamWriter)
        {
            if (ChannelFilters.Count == 0 && maxService == -1)
                return;

            streamWriter.WriteLine();
            streamWriter.WriteLine("[SERVICEFILTERS]");

            if (maxService != -1)
                streamWriter.WriteLine("MaxService=" + maxService);

            foreach (ChannelFilterEntry filterEntry in ChannelFilters)
            {
                streamWriter.WriteLine("IncludeService=" + filterEntry.OriginalNetworkID + "," +
                    filterEntry.TransportStreamID + "," +
                    filterEntry.StartServiceID + "," +
                    filterEntry.EndServiceID + "," +
                    filterEntry.Frequency);
            }
        }

        private void outputRepeatExclusionParameters(StreamWriter streamWriter)
        {
            if (Exclusions.Count == 0 && PhrasesToIgnore.Count == 0)
                return;

            streamWriter.WriteLine();
            streamWriter.WriteLine("[REPEATEXCLUSIONS]");

            foreach (RepeatExclusion exclusion in Exclusions)
                streamWriter.WriteLine("RepeatExclusion=" + exclusion.Title + "," + exclusion.Description);

            foreach (string phrase in PhrasesToIgnore)
                streamWriter.WriteLine("RepeatPhrase=" + phrase);
        }

        private void outputLookupParameters(StreamWriter streamWriter)
        {
            if (!MovieLookupEnabled && !TVLookupEnabled)
                return;

            streamWriter.WriteLine();
            streamWriter.WriteLine("[LOOKUPS]");

            if (MovieLookupEnabled)
            {
                streamWriter.WriteLine("MovieLookupEnabled=yes");
                if (DownloadMovieThumbnail == LookupImageType.Thumbnail)
                    streamWriter.WriteLine("MovieImage=thumbnail");
                else
                {
                    if (DownloadMovieThumbnail == LookupImageType.Poster)
                        streamWriter.WriteLine("MovieImage=poster");
                    else
                        streamWriter.WriteLine("MovieImage=none");
                }
                streamWriter.WriteLine("MovieLowDuration=" + MovieLowTime);
                streamWriter.WriteLine("MovieHighDuration=" + MovieHighTime);

                if (lookupMoviePhrases.Count != 0)
                {
                    if (moviePhraseSeparator != ",")
                        streamWriter.WriteLine("MoviePhraseSeparator=" + MoviePhraseSeparator);

                    streamWriter.Write("LookupMoviePhrases=");

                    foreach (string lookupMoviePhrase in lookupMoviePhrases)
                    {
                        if (lookupMoviePhrases.IndexOf(lookupMoviePhrase) == 0)
                            streamWriter.Write(lookupMoviePhrase);
                        else
                            streamWriter.Write(MoviePhraseSeparator + lookupMoviePhrase);
                    }

                    streamWriter.WriteLine();
                }

                if (lookupNotMovie != null)
                {
                    foreach (string notMovie in lookupNotMovie)
                        streamWriter.WriteLine("LookupNotMovie=" + notMovie);
                }
            }
            else
                streamWriter.WriteLine("MovieLookupEnabled=no");

            if (TVLookupEnabled)
            {
                streamWriter.WriteLine("TVLookupEnabled=yes");
                
                switch (DownloadTVThumbnail)
                {
                    case LookupImageType.Poster:
                        streamWriter.WriteLine("TVImage=poster");
                        break;
                    case LookupImageType.Banner:
                        streamWriter.WriteLine("TVImage=banner");
                        break;
                    case LookupImageType.Fanart:
                        streamWriter.WriteLine("TVImage=fanart");
                        break;
                    case LookupImageType.SmallPoster:
                        streamWriter.WriteLine("TVImage=smallposter");
                        break;
                    case LookupImageType.SmallFanart:
                        streamWriter.WriteLine("TVImage=smallfanart");
                        break;
                    case LookupImageType.None:
                        streamWriter.WriteLine("TVImage=none");
                        break;
                    default:                    
                        streamWriter.WriteLine("TVImage=none");
                        break;
                }
            }
            else
                streamWriter.WriteLine("TVLookupEnabled=no");

            streamWriter.WriteLine("LookupMatching=" + LookupMatching);

            if (LookupNotFound)
                streamWriter.WriteLine("LookupNotFound=yes");
            else
                streamWriter.WriteLine("LookupNotFound=no");

            if (LookupReload)
                streamWriter.WriteLine("LookupReload=yes");
            else
                streamWriter.WriteLine("LookupReload=no");

            if (LookupIgnoreCategories)
                streamWriter.WriteLine("LookupIgnoreCategories=yes");
            else
                streamWriter.WriteLine("LookupIgnoreCategories=no");

            if (LookupProcessAsTVSeries)
                streamWriter.WriteLine("LookupProcessAsTVSeries=yes");
            else
                streamWriter.WriteLine("LookupProcessAsTVSeries=no");

            streamWriter.WriteLine("LookupErrors=" + LookupErrorLimit);
            streamWriter.WriteLine("LookupTimeLimit=" + LookupTimeLimit);

            if (lookupIgnoredPhrases.Count != 0)
            {
                if (lookupIgnoredPhraseSeparator != ",")
                    streamWriter.WriteLine("LookupIgnoredPhraseSeparator=" + LookupIgnoredPhraseSeparator);

                streamWriter.Write("LookupIgnoredPhrases=");

                foreach (string lookupIgnoredPhrase in lookupIgnoredPhrases)
                {
                    if (lookupIgnoredPhrases.IndexOf(lookupIgnoredPhrase) == 0)
                        streamWriter.Write(lookupIgnoredPhrase);
                    else
                        streamWriter.Write(LookupIgnoredPhraseSeparator + lookupIgnoredPhrase);
                }

                streamWriter.WriteLine();
            }

            if (lookupImagePath != null)
                streamWriter.WriteLine("LookupImagePath=" + LookupImagePath);
            if (lookupXmltvImageTagPath != null)
                streamWriter.WriteLine("LookupXmltvImageTagPath=" + LookupXmltvImageTagPath);

            if (LookupImageNameTitle)
                streamWriter.WriteLine("LookupImageNameTitle=yes");
            else
                streamWriter.WriteLine("LookupImageNameTitle=no");

            if (LookupImagesInBase)
                streamWriter.WriteLine("LookupImagesInBase=yes");
            else
                streamWriter.WriteLine("LookupImagesInBase=no");
        }

        private void outputChannelUpdateParameters(StreamWriter streamWriter)
        {
            if (!ChannelUpdateEnabled)
                return;

            streamWriter.WriteLine();
            streamWriter.WriteLine("[CHANNELUPDATE]");

            streamWriter.WriteLine("ChannelUpdateEnabled=yes");

            switch (ChannelMergeMethod)
            {
                case ChannelMergeMethod.Name:
                    streamWriter.WriteLine("ChannelMergeMethod=name");
                    break;
                case ChannelMergeMethod.Number:
                    streamWriter.WriteLine("ChannelMergeMethod=number");
                    break;
                case ChannelMergeMethod.NameNumber:
                    streamWriter.WriteLine("ChannelMergeMethod=namenumber");
                    break;
                case ChannelMergeMethod.None:
                    streamWriter.WriteLine("ChannelMergeMethod=none");
                    break;
                default:
                    streamWriter.WriteLine("ChannelMergeMethod=none");
                    break;
            }

            switch (ChannelEPGScanner)
            {
                case ChannelEPGScanner.EPGCollector:
                    streamWriter.WriteLine("ChannelEPGScanner=EPG Collector");
                    break;
                case ChannelEPGScanner.EITScanner:
                    streamWriter.WriteLine("ChannelEPGScanner=EIT Scanner");
                    break;
                case ChannelEPGScanner.None:
                    streamWriter.WriteLine("ChannelEPGScanner=none");
                    break;
                case ChannelEPGScanner.Default:
                    streamWriter.WriteLine("ChannelEPGScanner=default");
                    break;
                case ChannelEPGScanner.Xmltv:
                    streamWriter.WriteLine("ChannelEPGScanner=xmltv");
                    break;
                default:
                    streamWriter.WriteLine("ChannelEPGScanner=none");
                    break;
            }

            if (ChannelChildLock)
                streamWriter.WriteLine("ChannelChildLock=yes");
            else
                streamWriter.WriteLine("ChannelChildLock=no");

            streamWriter.WriteLine("ChannelEPGScanInterval=" + ChannelEPGScanInterval.ToString());

            if (ChannelLogNetworkMap)
                streamWriter.WriteLine("ChannelLogNetworkMap=yes");
            else
                streamWriter.WriteLine("ChannelLogNetworkMap=no");

            if (ChannelReloadData)
                streamWriter.WriteLine("ChannelReloadData=yes");
            else
                streamWriter.WriteLine("ChannelReloadData=no");

            if (ChannelUpdateNumber)
                streamWriter.WriteLine("ChannelUpdateNumber=yes");
            else
                streamWriter.WriteLine("ChannelUpdateNumber=no");

            if (ChannelExcludeNew)
                streamWriter.WriteLine("ChannelExcludeNew=yes");
            else
                streamWriter.WriteLine("ChannelExcludeNew=no");
        }

        private void outputXmltvFileParameters(StreamWriter streamWriter)
        {
            if (importFiles == null || importFiles.Count == 0)
                return;

            streamWriter.WriteLine();
            streamWriter.WriteLine("[XMLTVIMPORT]");

            foreach (ImportFileSpec xmltvFileSpec in importFiles)
            {
                streamWriter.WriteLine("XmltvImportFile=" + xmltvFileSpec.FileName + "," +
                    xmltvFileSpec.Precedence.ToString() + "," +
                    (xmltvFileSpec.Language == null ? string.Empty : xmltvFileSpec.Language.Code) + "," +
                    xmltvFileSpec.NoLookup + "," +
                    xmltvFileSpec.IdFormat + "," +
                    xmltvFileSpec.AppendOnly);                
            }

            if (importChannelChanges != null)
            {
                foreach (ImportChannelChange channelChange in importChannelChanges)
                {
                    if (channelChange.NewName != null)
                    {
                        streamWriter.WriteLine("XmltvChannelChange=" + channelChange.DisplayName + "," +
                            channelChange.ChannelNumber + "," +
                            channelChange.Excluded.ToString() + "," +
                            channelChange.NewName);
                    }
                    else
                    {
                        streamWriter.WriteLine("XmltvChannelChange=" + channelChange.DisplayName + "," +
                            channelChange.ChannelNumber + "," +
                            channelChange.Excluded.ToString());
                    }
                }
            }
        }

        private void outputEditParameters(StreamWriter streamWriter)
        {
            if (editSpecs == null || editSpecs.Count == 0)
                return;

            streamWriter.WriteLine();
            streamWriter.WriteLine("[EDITSPECS]");

            foreach (EditSpec editSpec in editSpecs)
            {
                string replacementMode;

                switch (editSpec.ReplacementMode)
                {
                    case TextReplacementMode.TextOnly:
                        replacementMode = "Text";
                        break;
                    case TextReplacementMode.TextAndFollowing:
                        replacementMode = "Following";
                        break;
                    case TextReplacementMode.TextAndPreceeding:
                        replacementMode = "Preceeding";
                        break;
                    case TextReplacementMode.Everything:
                        replacementMode = "Everything";
                        break;
                    default:
                        replacementMode = "Text";
                        break;
                }

                streamWriter.WriteLine("EditSpec=" + editSpec.Text + "," + 
                    editSpec.ReplacementText + "," +
                    editSpec.ApplyToTitles + "," + 
                    editSpec.ApplyToDescriptions + "," + 
                    editSpec.Location + "," +
                    replacementMode);
            }
        }

        /// <summary>
        /// Create a copy of this instance.
        /// </summary>
        /// <returns>A new instance with the same properties as the current instance.</returns>
        public RunParameters Clone()
        {
            RunParameters newParameters = new RunParameters(parameterSet, runType);

            if (parameterSet == ParameterSet.Collector)
            {
                if (selectedTuners != null)
                {
                    newParameters.selectedTuners = new Collection<SelectedTuner>();
                    foreach (SelectedTuner tuner in selectedTuners)
                        newParameters.selectedTuners.Add(new SelectedTuner(tuner.TunerNumber, tuner.UniqueIdentity));
                }
            }

            newParameters.outputFileName = outputFileName;
            newParameters.wmcImportName = wmcImportName;
            newParameters.dvbviewerIPAddress = dvbviewerIPAddress;
            newParameters.frequencyTimeout = frequencyTimeout;
            newParameters.lockTimeout = lockTimeout;
            newParameters.repeats = repeats;
            newParameters.bufferSize = bufferSize;
            newParameters.bufferFills = bufferFills;
            newParameters.timeZone = timeZone;
            newParameters.nextTimeZone = nextTimeZone;
            newParameters.nextTimeZoneChange = nextTimeZoneChange;
            newParameters.timeZoneSet = timeZoneSet;
            
            newParameters.bladeRunnerFileName = bladeRunnerFileName;
            newParameters.areaRegionFileName = areaRegionFileName;
            newParameters.sageTVFileName = sageTVFileName;
            newParameters.sageTVSatelliteNumber = sageTVSatelliteNumber;

            if (options != null)
            {
                newParameters.options = new Collection<OptionEntry>();
                foreach (OptionEntry optionEntry in options)
                    newParameters.options.Add(optionEntry.Clone());
            }

            if (traceIDs != null)
            {
                newParameters.traceIDs = new Collection<TraceEntry>();
                foreach (TraceEntry traceEntry in traceIDs)
                    newParameters.traceIDs.Add(traceEntry.Clone());
            }

            if (debugIDs != null)
            {
                newParameters.debugIDs = new Collection<DebugEntry>();
                foreach (DebugEntry debugEntry in debugIDs)
                    newParameters.debugIDs.Add(debugEntry.Clone());
            }

            newParameters.diseqcIdentity = diseqcIdentity;
            
            foreach (TVStation station in StationCollection)
                newParameters.StationCollection.Add(station.Clone());

            foreach (TuningFrequency frequency in FrequencyCollection)
                newParameters.FrequencyCollection.Add(frequency.Clone());

            foreach (TimeOffsetChannel oldOffset in TimeOffsetChannels)
            {
                TimeOffsetChannel newOffset = new TimeOffsetChannel(oldOffset.SourceChannel, oldOffset.DestinationChannel, oldOffset.Offset);
                newParameters.TimeOffsetChannels.Add(newOffset);
            }

            foreach (ChannelFilterEntry oldFilterEntry in ChannelFilters)
            {
                ChannelFilterEntry newFilterEntry = new ChannelFilterEntry(oldFilterEntry.Frequency, oldFilterEntry.OriginalNetworkID,
                    oldFilterEntry.TransportStreamID, oldFilterEntry.StartServiceID, oldFilterEntry.EndServiceID);

                newParameters.ChannelFilters.Add(newFilterEntry);
            }

            foreach (RepeatExclusion oldRepeatExclusion in Exclusions)
            {
                RepeatExclusion newRepeatExclusion = new RepeatExclusion(oldRepeatExclusion.Title, oldRepeatExclusion.Description);
                newParameters.Exclusions.Add(newRepeatExclusion);
            }
            
            foreach (string oldPhrase in PhrasesToIgnore)
                newParameters.PhrasesToIgnore.Add(oldPhrase);

            newParameters.MaxService = maxService;

            newParameters.LookupErrorLimit = lookupErrorLimit;
            newParameters.LookupNotFound = lookupNotFound;
            newParameters.LookupReload = lookupReload;
            newParameters.LookupIgnoreCategories = lookupIgnoreCategories;
            newParameters.LookupProcessAsTVSeries = lookupProcessAsTVSeries;
            newParameters.LookupMatching = lookupMatching;
            newParameters.LookupTimeLimit = lookupTimeLimit;
            newParameters.MovieLookupEnabled = movieLookupEnabled;
            newParameters.DownloadMovieThumbnail = downloadMovieThumbnail;
            newParameters.MovieLowTime = movieLowTime;
            newParameters.MovieHighTime = movieHighTime;
            newParameters.TVLookupEnabled = tvLookupEnabled;
            newParameters.DownloadTVThumbnail = downloadTVThumbnail;
            newParameters.LookupIgnoredPhraseSeparator = lookupIgnoredPhraseSeparator;
            newParameters.MoviePhraseSeparator = moviePhraseSeparator;

            if (lookupNotMovie != null)
            {
                newParameters.lookupNotMovie = new Collection<string>();
                foreach (string notMovie in lookupNotMovie)
                    newParameters.LookupNotMovie.Add(notMovie);
            }

            if (lookupIgnoredPhrases != null)
            {
                newParameters.lookupIgnoredPhrases = new Collection<string>();
                foreach (string lookupIgnoredPhrase in lookupIgnoredPhrases)
                    newParameters.lookupIgnoredPhrases.Add(lookupIgnoredPhrase);
            }

            if (lookupMoviePhrases != null)
            {
                newParameters.lookupMoviePhrases = new Collection<string>();
                foreach (string lookupMoviePhrase in lookupMoviePhrases)
                    newParameters.lookupMoviePhrases.Add(lookupMoviePhrase);
            }

            newParameters.LookupImagePath = lookupImagePath;
            newParameters.LookupXmltvImageTagPath = lookupXmltvImageTagPath;
            
            newParameters.ChannelUpdateEnabled = channelUpdateEnabled;
            newParameters.ChannelMergeMethod = channelMergeMethod;
            newParameters.ChannelEPGScanner = channelEPGScanner;
            newParameters.ChannelChildLock = channelChildLock;
            newParameters.ChannelLogNetworkMap = channelLogNetworkMap;
            newParameters.ChannelEPGScanInterval = channelEPGScanInterval;
            newParameters.ChannelReloadData = channelReloadData;
            newParameters.ChannelUpdateNumber = channelUpdateNumber;
            newParameters.ChannelExcludeNew = channelExcludeNew;

            if (importFiles != null)
            {
                newParameters.ImportFiles = new Collection<ImportFileSpec>();

                foreach (ImportFileSpec oldSpec in importFiles)
                {
                    ImportFileSpec newSpec = new ImportFileSpec(oldSpec.FileName);
                    newSpec.Language = oldSpec.Language;
                    newSpec.Precedence = oldSpec.Precedence;
                    newSpec.NoLookup = oldSpec.NoLookup;
                    newSpec.AppendOnly = oldSpec.AppendOnly;
                    newSpec.IdFormat = oldSpec.IdFormat;

                    newParameters.ImportFiles.Add(newSpec);
                }
            }

            if (importChannelChanges != null)
            {
                newParameters.ImportChannelChanges = new Collection<ImportChannelChange>();

                foreach (ImportChannelChange oldChange in importChannelChanges)
                {
                    ImportChannelChange newChange = new ImportChannelChange(oldChange.DisplayName);
                    newChange.ChannelNumber = oldChange.ChannelNumber;
                    newChange.Excluded = oldChange.Excluded;
                    newChange.NewName = oldChange.NewName;

                    newParameters.ImportChannelChanges.Add(newChange);
                }
            }

            if (editSpecs != null)
            {
                newParameters.EditSpecs = new Collection<EditSpec>();

                foreach (EditSpec oldSpec in editSpecs)
                {
                    EditSpec newSpec = new EditSpec(oldSpec.Text, oldSpec.Location, oldSpec.ReplacementText);
                    newSpec.ApplyToTitles = oldSpec.ApplyToTitles;
                    newSpec.ApplyToDescriptions = oldSpec.ApplyToDescriptions;
                    newSpec.ReplacementMode = oldSpec.ReplacementMode;

                    newParameters.EditSpecs.Add(newSpec);
                }
            }

            return (newParameters);
        }

        /// <summary>
        /// Check if there have been and data changes.
        /// </summary>
        /// <param name="oldParameters">The original parameter values.</param>
        /// <returns>HasChanged if the data has changed; NotChanged otherwise.</returns>
        public DataState HasDataChanged(RunParameters oldParameters)
        {
            if (OutputFileSet != oldParameters.OutputFileSet)
                return (DataState.Changed);
            if (OutputFileSet && outputFileName != oldParameters.OutputFileName)
                return (DataState.Changed);
            if (wmcImportName != oldParameters.WMCImportName)
                return (DataState.Changed);

            if (dvbviewerIPAddress != oldParameters.dvbviewerIPAddress)
                return (DataState.Changed);

            if (bladeRunnerFileName != oldParameters.bladeRunnerFileName)
                return (DataState.Changed);
            if (areaRegionFileName != oldParameters.areaRegionFileName)
                return (DataState.Changed);
            if (sageTVFileName != oldParameters.sageTVFileName)
                return (DataState.Changed);
            if (sageTVSatelliteNumber != oldParameters.sageTVSatelliteNumber)
                return (DataState.Changed);

            if (maxService != oldParameters.MaxService)
                return (DataState.Changed);
            if (frequencyTimeout != oldParameters.FrequencyTimeout)
                return (DataState.Changed);
            if (lockTimeout != oldParameters.LockTimeout)
                return (DataState.Changed);
            if (repeats != oldParameters.Repeats)
                return (DataState.Changed);
            if (bufferSize != oldParameters.BufferSize)
                return (DataState.Changed);
            if (bufferFills != oldParameters.BufferFills)
                return (DataState.Changed);
            if (timeZoneSet != oldParameters.TimeZoneSet)
                return (DataState.Changed);
            if (timeZone != oldParameters.TimeZone)
                return (DataState.Changed);
            if (nextTimeZone != oldParameters.NextTimeZone)
                return (DataState.Changed);
            if (nextTimeZoneChange != oldParameters.NextTimeZoneChange)
                return (DataState.Changed);

            if ((options == null && oldParameters.Options != null) || (options != null && oldParameters.Options == null))
                return (DataState.Changed);

            if (options != null)
            {
                if (options.Count != oldParameters.Options.Count)
                    return (DataState.Changed);

                foreach (OptionEntry optionEntry in options)
                {
                    if (OptionEntry.FindEntry(oldParameters.Options, optionEntry.ToString()) == null)
                        return (DataState.Changed);
                }
            }

            if ((traceIDs == null && oldParameters.TraceIDs != null) || (traceIDs != null && oldParameters.TraceIDs == null))
                return (DataState.Changed);

            if (traceIDs != null)
            {
                if (traceIDs.Count != oldParameters.TraceIDs.Count)
                    return (DataState.Changed);

                foreach (TraceEntry traceEntry in traceIDs)
                {
                    if (TraceEntry.FindEntry(oldParameters.TraceIDs, traceEntry.ToString()) == null)
                        return (DataState.Changed);
                }
            }

            if ((debugIDs == null && oldParameters.DebugIDs != null) || (debugIDs != null && oldParameters.DebugIDs == null))
                return (DataState.Changed);

            if (debugIDs != null)
            {
                if (debugIDs.Count != oldParameters.DebugIDs.Count)
                    return (DataState.Changed);

                foreach (DebugEntry debugEntry in debugIDs)
                {
                    if (DebugEntry.FindEntry(oldParameters.DebugIDs, debugEntry.ToString()) == null)
                        return (DataState.Changed);
                }
            }

            if (FrequencyCollection.Count != oldParameters.FrequencyCollection.Count)
                return (DataState.Changed);

            for (int index = 0; index < FrequencyCollection.Count; index++)
            {
                if (!FrequencyCollection[index].EqualTo(oldParameters.FrequencyCollection[index], EqualityLevel.Entirely))
                    return (DataState.Changed);
            }            

            if (StationCollection.Count != oldParameters.StationCollection.Count)
                return (DataState.Changed);

            for (int index = 0; index < StationCollection.Count; index++)
            {
                TVStation newStation = StationCollection[index];
                TVStation oldStation = oldParameters.StationCollection[index];

                if (newStation.OriginalNetworkID != oldStation.OriginalNetworkID ||
                    newStation.TransportStreamID != oldStation.TransportStreamID ||
                    newStation.ServiceID != oldStation.ServiceID || 
                    !newStation.EqualTo(oldStation))
                        return(DataState.Changed);
            }

            if (TimeOffsetChannels.Count != oldParameters.TimeOffsetChannels.Count)
                return (DataState.Changed);

            for (int index = 0; index < TimeOffsetChannels.Count; index++)                    
            {
                TimeOffsetChannel newOffset = TimeOffsetChannels[index];
                TimeOffsetChannel oldOffset = oldParameters.TimeOffsetChannels[index];

                if (oldOffset.SourceChannel.Name != newOffset.SourceChannel.Name ||
                    oldOffset.DestinationChannel.Name != newOffset.DestinationChannel.Name ||
                    oldOffset.Offset != newOffset.Offset)
                    return (DataState.Changed);
            }            

            if (ChannelFilters.Count != oldParameters.ChannelFilters.Count)
                return (DataState.Changed);

            for (int index = 0; index < ChannelFilters.Count; index++)
            {
                ChannelFilterEntry newFilterEntry = ChannelFilters[index];
                ChannelFilterEntry oldFilterEntry = oldParameters.ChannelFilters[index];
                
                if (oldFilterEntry.OriginalNetworkID != newFilterEntry.OriginalNetworkID ||
                    oldFilterEntry.TransportStreamID != newFilterEntry.TransportStreamID ||
                    oldFilterEntry.StartServiceID != newFilterEntry.StartServiceID ||
                    oldFilterEntry.EndServiceID != newFilterEntry.EndServiceID ||
                    oldFilterEntry.Frequency != newFilterEntry.Frequency)
                    return (DataState.Changed);
            }
            
            if (Exclusions.Count != oldParameters.Exclusions.Count)
                return (DataState.Changed);

            for (int index = 0; index < Exclusions.Count; index++)
            {
                RepeatExclusion newExclusion = Exclusions[index];
                RepeatExclusion oldExclusion = oldParameters.Exclusions[index];
                    
                if (oldExclusion.Title != newExclusion.Title ||
                    oldExclusion.Description != newExclusion.Description)
                    return (DataState.Changed);
            }            

            if (PhrasesToIgnore.Count != oldParameters.PhrasesToIgnore.Count)
                return (DataState.Changed);

            for (int index = 0; index < PhrasesToIgnore.Count; index++ )
            {
                if (oldParameters.PhrasesToIgnore[index] != PhrasesToIgnore[index])
                    return (DataState.Changed);
            }

            if (lookupErrorLimit != oldParameters.LookupErrorLimit ||
                lookupNotFound != oldParameters.LookupNotFound ||
                lookupMatching != oldParameters.LookupMatching ||
                lookupTimeLimit != oldParameters.LookupTimeLimit ||
                movieLookupEnabled != oldParameters.MovieLookupEnabled ||
                downloadMovieThumbnail != oldParameters.DownloadMovieThumbnail ||
                movieLowTime != oldParameters.MovieLowTime ||
                movieHighTime != oldParameters.MovieHighTime ||
                tvLookupEnabled != oldParameters.TVLookupEnabled ||
                downloadTVThumbnail != oldParameters.DownloadTVThumbnail ||
                lookupReload != oldParameters.LookupReload ||
                lookupIgnoreCategories != oldParameters.LookupIgnoreCategories ||
                lookupProcessAsTVSeries != oldParameters.LookupProcessAsTVSeries)
                    return (DataState.Changed);

            if ((lookupIgnoredPhrases == null && oldParameters.LookupIgnoredPhrases != null) || (lookupIgnoredPhrases != null && oldParameters.LookupIgnoredPhrases == null))
                return (DataState.Changed);

            if (lookupIgnoredPhrases != null)
            {
                if (lookupIgnoredPhrases.Count != oldParameters.LookupIgnoredPhrases.Count)
                    return (DataState.Changed);

                foreach (string lookupIgnoredPhrase in lookupIgnoredPhrases)
                {
                    if (!oldParameters.LookupIgnoredPhrases.Contains(lookupIgnoredPhrase))
                        return (DataState.Changed);
                }
            }

            if ((lookupMoviePhrases == null && oldParameters.LookupMoviePhrases != null) || (lookupMoviePhrases != null && oldParameters.LookupMoviePhrases == null))
                return (DataState.Changed);

            if (lookupMoviePhrases != null)
            {
                if (lookupMoviePhrases.Count != oldParameters.LookupMoviePhrases.Count)
                    return (DataState.Changed);

                foreach (string lookupMoviePhrase in lookupMoviePhrases)
                {
                    if (!oldParameters.LookupMoviePhrases.Contains(lookupMoviePhrase))
                        return (DataState.Changed);
                }
            }

            if ((lookupNotMovie == null && oldParameters.LookupNotMovie != null) || (lookupNotMovie != null && oldParameters.LookupNotMovie == null))
                return (DataState.Changed);

            if (lookupNotMovie != null)
            {
                if (lookupNotMovie.Count != oldParameters.LookupNotMovie.Count)
                    return (DataState.Changed);

                foreach (string notMovie in lookupNotMovie)
                {
                    if (!oldParameters.LookupNotMovie.Contains(notMovie))
                        return (DataState.Changed);
                }
            }

            if (lookupIgnoredPhraseSeparator != oldParameters.LookupIgnoredPhraseSeparator)
                return (DataState.Changed);
            if (moviePhraseSeparator != oldParameters.MoviePhraseSeparator)
                return (DataState.Changed);

            if (lookupImagePath == null)
            {
                if (oldParameters.LookupImagePath != null)
                    return (DataState.Changed);
            }
            else
            {
                if (oldParameters.LookupImagePath == null)
                    return (DataState.Changed);
                else
                {
                    if (lookupImagePath != oldParameters.LookupImagePath)
                        return (DataState.Changed);
                }
            }

            if (lookupXmltvImageTagPath == null)
            {
                if (oldParameters.LookupXmltvImageTagPath != null)
                    return (DataState.Changed);
            }
            else
            {
                if (oldParameters.LookupXmltvImageTagPath == null)
                    return (DataState.Changed);
                else
                {
                    if (lookupXmltvImageTagPath != oldParameters.LookupXmltvImageTagPath)
                        return (DataState.Changed);
                }
            }

            if (oldParameters.ChannelUpdateEnabled != channelUpdateEnabled)
                return (DataState.Changed);
            if (oldParameters.ChannelMergeMethod != channelMergeMethod)
                return (DataState.Changed);
            if (oldParameters.ChannelEPGScanner != channelEPGScanner)
                return (DataState.Changed);
            if (oldParameters.ChannelChildLock != channelChildLock)
                return (DataState.Changed);
            if (oldParameters.ChannelLogNetworkMap != channelLogNetworkMap)
                return (DataState.Changed);
            if (oldParameters.ChannelEPGScanInterval != channelEPGScanInterval)
                return (DataState.Changed);
            if (oldParameters.ChannelReloadData != channelReloadData)
                return (DataState.Changed);
            if (oldParameters.ChannelUpdateNumber != channelUpdateNumber)
                return (DataState.Changed);
            if (oldParameters.ChannelExcludeNew != channelExcludeNew)
                return (DataState.Changed);

            if (oldParameters.ImportFiles == null && importFiles != null)
                return (DataState.Changed);
            if (oldParameters.ImportFiles != null && importFiles == null)
                return (DataState.Changed);
            if (oldParameters.ImportFiles != null)
            {
                if (oldParameters.ImportFiles.Count != importFiles.Count)
                    return (DataState.Changed);

                for (int index = 0; index < oldParameters.ImportFiles.Count; index++)
                {
                    ImportFileSpec oldSpec = oldParameters.ImportFiles[index];
                    ImportFileSpec newSpec = importFiles[index];

                    if (oldSpec.FileName != newSpec.FileName)
                        return (DataState.Changed);

                    if (oldSpec.Precedence != newSpec.Precedence)
                        return (DataState.Changed);

                    if (oldSpec.Language == null && newSpec.Language != null)
                        return (DataState.Changed);

                    if (oldSpec.Language != null && newSpec.Language == null)
                        return (DataState.Changed);

                    if (oldSpec.Language != null && oldSpec.Language.Code != newSpec.Language.Code)
                        return (DataState.Changed);

                    if (oldSpec.NoLookup != newSpec.NoLookup)
                        return (DataState.Changed);

                    if (oldSpec.AppendOnly != newSpec.AppendOnly)
                        return (DataState.Changed);

                    if (oldSpec.IdFormat != newSpec.IdFormat)
                        return (DataState.Changed);
                }
            }

            if (oldParameters.ImportChannelChanges == null && importChannelChanges != null)
                return (DataState.Changed);
            if (oldParameters.ImportChannelChanges != null && importChannelChanges == null)
                return (DataState.Changed);
            if (oldParameters.ImportChannelChanges != null)
            {
                if (oldParameters.ImportChannelChanges.Count != importChannelChanges.Count)
                    return (DataState.Changed);

                for (int index = 0; index < oldParameters.ImportChannelChanges.Count; index++)
                {
                    ImportChannelChange oldChange = oldParameters.ImportChannelChanges[index];
                    ImportChannelChange newChange = importChannelChanges[index];

                    if (oldChange.DisplayName != newChange.DisplayName)
                        return (DataState.Changed);

                    if (oldChange.NewName == null && newChange.NewName != null)
                        return (DataState.Changed);
                    if (oldChange.NewName != null && newChange.NewName == null)
                        return (DataState.Changed);
                    if (oldChange.NewName != null && oldChange.NewName != newChange.NewName)
                        return (DataState.Changed);

                    if (oldChange.ChannelNumber != newChange.ChannelNumber)
                        return (DataState.Changed);

                    if (oldChange.Excluded != newChange.Excluded)
                        return (DataState.Changed);
                }
            }

            if (oldParameters.EditSpecs == null && editSpecs != null)
                return (DataState.Changed);
            if (oldParameters.EditSpecs != null && editSpecs == null)
                return (DataState.Changed);
            if (oldParameters.EditSpecs != null)
            {
                if (oldParameters.EditSpecs.Count != editSpecs.Count)
                    return (DataState.Changed);

                for (int index = 0; index < oldParameters.EditSpecs.Count; index++)
                {
                    EditSpec oldSpec = oldParameters.EditSpecs[index];
                    EditSpec newSpec = editSpecs[index];

                    if (oldSpec.Text != newSpec.Text)
                        return (DataState.Changed);

                    if (oldSpec.ReplacementText != newSpec.ReplacementText)
                        return (DataState.Changed);

                    if (oldSpec.Location != newSpec.Location)
                        return (DataState.Changed);

                    if (oldSpec.ApplyToTitles != newSpec.ApplyToTitles)
                        return (DataState.Changed);

                    if (oldSpec.ApplyToDescriptions != newSpec.ApplyToDescriptions)
                        return (DataState.Changed);

                    if (oldSpec.ReplacementMode != newSpec.ReplacementMode)
                        return (DataState.Changed);
                }
            }

            return (DataState.NotChanged);
        }

        /// <summary>
        /// Get a legal filename string.
        /// </summary>
        /// <param name="fileName">The original file name.</param>
        /// <param name="replacer">The character to replace illegal characters.</param>
        /// <returns>The legal file name.</returns>
        public static string GetLegalFileName(string fileName, char replacer)
        {
            char[] illegalChars = Path.GetInvalidFileNameChars();

            string legalName = fileName;

            foreach (char illegalChar in illegalChars)
                legalName = legalName.Replace(illegalChar, replacer);

            return (legalName);
        }
    }
}
