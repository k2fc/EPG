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
using System.Threading;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Reflection;
using System.Collections.ObjectModel;

using DirectShow;
using DomainObjects;
using DVBServices;
using Lookups;
using ChannelUpdate;
using XmltvParser;
using MxfParser;
using NetReceiver;
using SatIp;
using SatIpDomainObjects;
using NetworkProtocols;
using VBox;

namespace EPG
{
    class EPGCollector
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

        private static ITunerDataProvider graph;

        private static TimerCallback timerDelegate;
        private static System.Threading.Timer timer;

        private static BackgroundWorker graphWorker;
        private static BackgroundWorker keyboardWorker;

        private static AutoResetEvent endProgramEvent = new AutoResetEvent(false);
        private static AutoResetEvent endFrequencyEvent = new AutoResetEvent(false);

        private static bool cancelGraph;
        private static bool ignoreProcessComplete;

        private static int collectionsWorked;
        private static int tuneFailed;
        private static int timeOuts;

        private static DateTime startTime;
        private static int totalOverlaps;
        private static int totalGaps;

        private static bool pluginAbandon = false;

        public static void Run(bool reply)
        {
            RunParameters.BaseDirectory = Application.StartupPath;

            try
            {
                Logger.Instance.WriteSeparator("EPG Collector (Version " + RunParameters.SystemVersion + ")");
            }
            catch (Exception e)
            {
                Console.WriteLine("Cannot write log file");
                Console.WriteLine(e.Message);
                return;
            }

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(unhandledException);
            

            startTime = DateTime.Now;
            HistoryRecord.Current = new HistoryRecord(startTime);

            Logger.Instance.Write("");
            Logger.Instance.Write("OS version: " + Environment.OSVersion.Version + (RunParameters.Is64Bit ? " 64-bit" : " 32-bit"));
            Logger.Instance.Write("");
            Logger.Instance.Write("Executable build: " + AssemblyVersion);
            Logger.Instance.Write("DirectShow build: " + DirectShowGraph.AssemblyVersion);
            Logger.Instance.Write("DomainObjects build: " + RunParameters.AssemblyVersion);
            Logger.Instance.Write("DVBServices build: " + DVBServices.Utils.AssemblyVersion);
            Logger.Instance.Write("Lookups build: " + LookupController.AssemblyVersion);
            Logger.Instance.Write("ChannelUpdate build: " + DVBLinkController.AssemblyVersion);
            Logger.Instance.Write("XmltvParser build: " + XmltvController.AssemblyVersion);
            Logger.Instance.Write("MxfParser build: " + MxfController.AssemblyVersion);
            Logger.Instance.Write("NetReceiver build: " + ReceiverBase.AssemblyVersion);
            Logger.Instance.Write("NetworkProtocols build: " + NetworkConfiguration.AssemblyVersion);
            Logger.Instance.Write("SatIp build: " + SatIpController.AssemblyVersion);
            Logger.Instance.Write("VBox build: " + VBoxController.AssemblyVersion);
            Logger.Instance.Write("");
            Logger.Instance.Write("");
            Logger.Instance.Write("Privilege level: " + RunParameters.Role);
            Logger.Instance.Write("");
            Logger.Instance.Write("Base directory: " + RunParameters.BaseDirectory);
            Logger.Instance.Write("Data directory: " + RunParameters.DataDirectory);
            Logger.Instance.Write("");
            
            
            if (!reply)
            {
                Logger.Instance.Write("<e> Incorrect command line");
                Logger.Instance.Write("<e> Exiting with code = 4");
                return;
            }

            if (RunParameters.IsMono)
            {
                Logger.Instance.Write("Mono version: " + RunParameters.MonoVersion);
                Logger.Instance.Write("");
            }

            if (RunParameters.IsWine)
            {
                Logger.Instance.Write("Running in the Wine environment");
                Logger.Instance.Write("");
            }

            if (!CommandLine.PluginMode)
                runNormalCollection();
            else
                runPluginCollection();
        }

        private static void runNormalCollection()
        {
            if (CommandLine.TunerQueryOnly)
            {
                BDAGraph.LoadTuners();
                SatIpServer.LoadServers();
                VBoxTuner.LoadServers();
                if (!Tuner.TunerPresent)
                {
                    Logger.Instance.Write("<e> No tuners detected");
                    Logger.Instance.Write("<e> Exiting with code = " + (int)ExitCode.NoDVBTuners);
                    return;
                }
                else
                {
                    Logger.Instance.Write("Tuner query only");
                    Logger.Instance.Write("<C> Exiting with code = " + (int)ExitCode.OK);
                    return;
                }
            }

            ExitCode exitCode = ExitCode.OK;
            if (exitCode != ExitCode.OK)
            {
                HistoryRecord.Current.CollectionResult = CommandLine.GetCompletionCodeShortDescription(exitCode);
                Logger.Write(HistoryRecord.Current);

                Logger.Instance.Write("<e> Configuration incorrect");
                Logger.Instance.Write("<e> Exiting with code = " + (int)exitCode);
                return;
            }

            processTunerCollection();
        }

        private static void processTunerCollection()
        {
            bool reply;

            if (TuningFrequency.TunersNeeded(RunParameters.Instance.FrequencyCollection))
            {
                BDAGraph.LoadTuners();

                if (SatIpConfiguration.SatIpEnabled)
                    SatIpServer.LoadServers();

                if (VBoxConfiguration.VBoxEnabled)
                    VBoxTuner.LoadServers();

                if (!Tuner.TunerPresent)
                {
                    HistoryRecord.Current.CollectionResult = CommandLine.GetCompletionCodeShortDescription(ExitCode.NoDVBTuners);
                    Logger.Write(HistoryRecord.Current);

                    Logger.Instance.Write("<e> No tuners detected");
                    Logger.Instance.Write("<e> Exiting with code = " + ExitCode.NoDVBTuners);
                    return;
                }

                reply = checkConfiguration();
                if (!reply)
                {
                    HistoryRecord.Current.CollectionResult = CommandLine.GetCompletionCodeShortDescription(ExitCode.ParameterTunerMismatch);
                    Logger.Write(HistoryRecord.Current);

                    Logger.Instance.Write("<e> Configuration does not match ini parameters");
                    Logger.Instance.Write("<e> Exiting with code = " + (int)ExitCode.ParameterTunerMismatch);
                    return;
                }
            }

            EPGController.ProcessComplete += new EPGController.ProcessCompleteHandler(epgControllerProcessComplete);

            graphWorker = new BackgroundWorker();
            graphWorker.WorkerSupportsCancellation = true;
            graphWorker.DoWork += new DoWorkEventHandler(graphWorkerDoWork);
            graphWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(graphWorkerCompleted);
            graphWorker.RunWorkerAsync();

            if (RunParameters.IsWine)
                Logger.Instance.Write("Keyboard inhibited in Wine environment - cancellation not available");
            else
            {
                if (OptionEntry.IsDefined(OptionName.RunFromService))
                    Logger.Instance.Write("Keyboard inhibited by parameter - cancellation not available");
                else
                {
                    keyboardWorker = new BackgroundWorker();
                    keyboardWorker.WorkerSupportsCancellation = true;
                    keyboardWorker.DoWork += new DoWorkEventHandler(keyboardWorkerDoWork);
                    keyboardWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(keyboardWorkerCompleted);
                    keyboardWorker.RunWorkerAsync();
                }
            }

            reply = endProgramEvent.WaitOne();

            if (timer != null)
                timer.Dispose();

            if (cancelGraph)
            {
                HistoryRecord.Current.CollectionResult = CommandLine.GetCompletionCodeShortDescription(ExitCode.AbandonedByUser);
                Logger.Write(HistoryRecord.Current);

                Logger.Instance.Write("Cancelled by user - no file created");
                Logger.Instance.Write("<C> Exiting with code = " + (int)ExitCode.AbandonedByUser);
                return;
            }

            if (RunParameters.Instance.FrequencyCollection.Count == 0 || (RunParameters.Instance.FrequencyCollection.Count != 0 && collectionsWorked != 0))
            {
                HistoryRecord.Current.CollectionDuration = DateTime.Now - startTime;

                EPGController.Instance.FinishRun();
                if (RunParameters.Instance.AbandonRequested)
                    return;

                logDataCollected();

                string outputReply = OutputFile.Process();
                if (outputReply != null)
                {
                    HistoryRecord.Current.CollectionResult = CommandLine.GetCompletionCodeShortDescription(ExitCode.OutputFileNotCreated);
                    Logger.Write(HistoryRecord.Current);

                    Logger.Instance.Write("<e> The output file could not be created");
                    Logger.Instance.Write("<e> " + outputReply);
                    Logger.Instance.Write("<C> Exiting with code = " + (int)ExitCode.OutputFileNotCreated);
                    return;
                }
            }

            int recordCountOutput = TVStation.EPGCountIncluded(RunParameters.Instance.StationCollection);
            int recordCountTotal = TVStation.EPGCount(RunParameters.Instance.StationCollection);
            Logger.Instance.Write("<C> Finished - output " + recordCountOutput + " EPG entries" +
                (recordCountTotal == recordCountOutput ? "" : " out of " + recordCountTotal));
            HistoryRecord.Current.CollectionCount = recordCountOutput;

            if (collectionsWorked != RunParameters.Instance.FrequencyCollection.Count || timeOuts != 0)
            {
                HistoryRecord.Current.CollectionResult = CommandLine.GetCompletionCodeShortDescription(ExitCode.SomeFrequenciesNotProcessed);
                Logger.Write(HistoryRecord.Current);

                Logger.Instance.Write("<e> Exiting with code = " + (int)ExitCode.SomeFrequenciesNotProcessed);
                return;
            }

            if (TVStation.EPGCount(RunParameters.Instance.StationCollection) == 0)
            {
                HistoryRecord.Current.CollectionResult = CommandLine.GetCompletionCodeShortDescription(ExitCode.NoDataCollected);
                Logger.Write(HistoryRecord.Current);

                Logger.Instance.Write("<e> No data collected");
                Logger.Instance.Write("<e> Exiting with code = " + (int)ExitCode.NoDataCollected);
                return;
            }

            if (OptionEntry.IsDefined(OptionName.StoreStationInfo))
            {
                string unloadReply = TVStation.Unload(Path.Combine(RunParameters.DataDirectory, "Station Cache.xml"), RunParameters.Instance.StationCollection);
                if (unloadReply != null)
                {
                    Logger.Instance.Write("<C> Failed to output station cache file");
                    Logger.Instance.Write("<C> " + unloadReply);
                }
                else
                    Logger.Instance.Write("Station cache file output successfully");
            }

            if (RunParameters.Instance.ChannelUpdateEnabled)
                updateChannels();

            HistoryRecord.Current.CollectionResult = CommandLine.GetCompletionCodeShortDescription(ExitCode.OK);
            Logger.Write(HistoryRecord.Current);

            Logger.Instance.Write("<C> Exiting with code = " + (int)ExitCode.OK);
            return;
        }

        private static void graphWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                throw new InvalidOperationException("Graph background worker failed - see inner exception", e.Error);
        }

        private static void keyboardWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                throw new InvalidOperationException("Keyboard background worker failed - see inner exception", e.Error);
        }

        private static void unhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception exception = e.ExceptionObject as Exception;

            if (exception != null)
            {
                while (exception.InnerException != null)
                    exception = exception.InnerException;

                Logger.Instance.Write("<E> ** The program has failed with an exception of type " + exception.GetType().Name);
                Logger.Instance.Write("<E> ** Exception: " + exception.Message);
                Logger.Instance.Write("<E> ** Location: " + exception.StackTrace);
            }
            else
                Logger.Instance.Write("<E> An unhandled exception of type " + e.ExceptionObject + " has occurred");

            HistoryRecord.Current.CollectionResult = CommandLine.GetCompletionCodeShortDescription(ExitCode.SoftwareException);
            Logger.Write(HistoryRecord.Current);

            Logger.Instance.Write("<E> Exiting with code = " + (int)ExitCode.SoftwareException);
            return;
        }

        private static void epgControllerProcessComplete(object sender, EventArgs e)
        {
            if (ignoreProcessComplete)
                return;

            endFrequencyEvent.Set();
        }

        private static void timerCallback(object stateObject)
        {
            Logger.Instance.Write("");
            Logger.Instance.Write("<e> Collection timed out (" + RunParameters.Instance.FrequencyTimeout + ")");
            timeOuts++;
            endFrequencyEvent.Set();
        }

        private static void graphWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            int frequencyIndex = 0;

            while (frequencyIndex < RunParameters.Instance.FrequencyCollection.Count && !cancelGraph)
            {
                TuningFrequency frequency = RunParameters.Instance.FrequencyCollection[frequencyIndex];
                RunParameters.Instance.CurrentFrequency = frequency;

                switch (frequency.TunerType)
                {
                    case TunerType.File:
                        SimulationDataProvider dataProvider = new SimulationDataProvider(((FileFrequency)frequency).Path, frequency);
                        string providerReply = dataProvider.Run();
                        if (providerReply != null)
                        {
                            Logger.Instance.Write("<e> Simulation Data Provider failed");
                            Logger.Instance.Write("<e> " + providerReply);
                        }
                        else
                        {
                            getData(frequency, dataProvider);
                            dataProvider.Stop();
                        }
                        break;
                    case TunerType.Stream:
                        StreamFrequency streamFrequency = frequency as StreamFrequency;
                        StreamController streamController = new StreamController(streamFrequency.IPAddress, streamFrequency.PortNumber);
                        ErrorSpec errorSpec = streamController.Run(streamFrequency, null);
                        if (errorSpec != null)
                        {
                            Logger.Instance.Write("<e> Stream Data Provider failed");
                            Logger.Instance.Write("<e> " + errorSpec);
                        }
                        else
                        {
                            getData(frequency, streamController);
                            streamController.Stop();
                        }
                        break;
                    default:
                        bool tuned = tuneFrequency(frequency);
                        if (!tuned)
                            tuneFailed++;
                        else
                        {
                            getData(frequency, graph as ISampleDataProvider);

                            if (graph != null)
                                graph.Dispose();
                        }
                        break;
                }

                frequencyIndex++;
            }

            endProgramEvent.Set();
        }

        private static bool tuneFrequency(TuningFrequency frequency)
        {
            Logger.Instance.Write("Tuning to frequency " + frequency.Frequency + " on " + frequency.TunerType);

            TuningSpec tuningSpec;
            TunerNodeType tunerNodeType;

            switch (frequency.TunerType)
            {
                case TunerType.Satellite:
                    tuningSpec = new TuningSpec((Satellite)frequency.Provider, (SatelliteFrequency)frequency);
                    tunerNodeType = TunerNodeType.Satellite;
                    break;
                case TunerType.Terrestrial:
                    tuningSpec = new TuningSpec((TerrestrialFrequency)frequency);
                    tunerNodeType = TunerNodeType.Terrestrial;
                    break;
                case TunerType.Cable:
                    tuningSpec = new TuningSpec((CableFrequency)frequency);
                    tunerNodeType = TunerNodeType.Cable;
                    break;
                case TunerType.ATSC:
                case TunerType.ATSCCable:
                    tuningSpec = new TuningSpec((AtscFrequency)frequency);
                    tunerNodeType = TunerNodeType.ATSC;
                    break;
                case TunerType.ClearQAM:
                    tuningSpec = new TuningSpec((ClearQamFrequency)frequency);
                    tunerNodeType = TunerNodeType.Cable;
                    break;
                case TunerType.ISDBS:
                    tuningSpec = new TuningSpec((Satellite)frequency.Provider, (ISDBSatelliteFrequency)frequency);
                    tunerNodeType = TunerNodeType.ISDBS;
                    break;
                case TunerType.ISDBT:
                    tuningSpec = new TuningSpec((ISDBTerrestrialFrequency)frequency);
                    tunerNodeType = TunerNodeType.ISDBT;
                    break;
                default:
                    throw (new InvalidOperationException("Frequency tuner type not recognized"));
            }

            bool finished = false;
            int frequencyRetries = 0;

            Tuner currentTuner = null;

            while (!finished)
            {
                graph = BDAGraph.FindTuner(frequency.SelectedTuners, tunerNodeType, tuningSpec, currentTuner);
                if (graph == null)
                {
                    graph = SatIpController.FindReceiver(frequency.SelectedTuners, tunerNodeType, tuningSpec, currentTuner, getDiseqcSetting(tuningSpec.Frequency));
                    if (graph == null)
                    {
                        graph = VBoxController.FindReceiver(frequency.SelectedTuners, tunerNodeType, tuningSpec, currentTuner, getDiseqcSetting(tuningSpec.Frequency), false);
                        if (graph == null)
                        {
                            Logger.Instance.Write("<e> No tuner able to tune frequency " + frequency.ToString());
                            return (false);
                        }
                    }
                }

                TimeSpan timeout = new TimeSpan();
                bool done = false;
                bool locked = false;

                while (!done)
                {
                    if (cancelGraph)
                    {
                        graph.Dispose();
                        return (false);
                    }

                    locked = graph.SignalLocked;
                    if (!locked)
                    {
                        if (graph.SignalQuality > 0)
                        {
                            locked = true;
                            done = true;
                        }
                        else
                        {
                            if (graph.SignalPresent)
                            {
                                locked = true;
                                done = true;
                            }
                            else
                            {
                                Logger.Instance.Write("Signal not acquired: lock is " + graph.SignalLocked + " quality is " + graph.SignalQuality + " signal not present");
                                Thread.Sleep(1000);
                                timeout = timeout.Add(new TimeSpan(0, 0, 1));
                                done = (timeout.TotalSeconds == RunParameters.Instance.LockTimeout.TotalSeconds);
                            }
                        }
                    }
                    else
                        done = true;
                }

                if (!locked)
                {
                    Logger.Instance.Write("<e> Failed to acquire signal");
                    graph.Dispose();

                    if (frequencyRetries == 2)
                    {
                        currentTuner = graph.Tuner;
                        frequencyRetries = 0;
                    }
                    else
                    {
                        frequencyRetries++;
                        Logger.Instance.Write("Retrying frequency");
                    }
                }
                else
                {
                    finished = true;
                    Logger.Instance.Write("Signal acquired: lock is " + graph.SignalLocked + " quality is " + graph.SignalQuality + " strength is " + graph.SignalStrength);
                }
            }

            return (true);
        }

        private static int getDiseqcSetting(TuningFrequency frequency)
        {
            SatelliteFrequency satelliteFrequency = frequency as SatelliteFrequency;
            if (satelliteFrequency == null)
                return (0);

            if (satelliteFrequency.DiseqcRunParamters.DiseqcSwitch == null)
                return (0);

            switch (satelliteFrequency.DiseqcRunParamters.DiseqcSwitch)
            {
                case "A":
                    return (1);
                case "B":
                    return (2);
                case "AA":
                    return (1);
                case "AB":
                    return (2);
                case "BA":
                    return (3);
                case "BB":
                    return (4);
                default:
                    return (0);
            }
        }

        private static bool getData(TuningFrequency frequency, ISampleDataProvider dataProvider)
        {
            timerDelegate = new TimerCallback(timerCallback);
            timer = new System.Threading.Timer(timerDelegate, null, RunParameters.Instance.FrequencyTimeout, RunParameters.Instance.FrequencyTimeout);

            ignoreProcessComplete = false;

            EPGController.Instance.Run(dataProvider, frequency);
            bool reply = endFrequencyEvent.WaitOne();

            ignoreProcessComplete = true;

            EPGController.Instance.Stop();
            timer.Dispose();

            collectionsWorked++;

            return (true);
        }

        private static void keyboardWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            bool abandon = false;

            do
            {
                if (!CommandLine.BackgroundMode)
                {
                    ConsoleKeyInfo abandonKey = Console.ReadKey();
                    abandon = (abandonKey.Key == ConsoleKey.Q);
                }
                else
                {
                    Mutex cancelMutex = new Mutex(false, "EPG Collector Cancel Mutex " + CommandLine.RunReference);
                    cancelMutex.WaitOne();
                    cancelMutex.Close();
                    abandon = true;
                }
            }
            while (!abandon);

            cancelGraph = true;
            Logger.Instance.Write("Abandon request from user");

            RunParameters.Instance.AbandonRequested = true;

            endFrequencyEvent.Set();
        }

        private static bool checkConfiguration()
        {
            foreach (TuningFrequency tuningFrequency in RunParameters.Instance.FrequencyCollection)
            {
                foreach (SelectedTuner tuner in tuningFrequency.SelectedTuners)
                {
                    if (tuner.TunerNumber > Tuner.TunerCollection.Count)
                    {
                        Logger.Instance.Write("<e> INI file format error: A Tuner number is out of range.");
                        Logger.Instance.Write("<e> Exiting with code = " + (int)ExitCode.ParameterError);
                        return false;
                    }
                }
            }

            return (true);
        }

        private static void logDataCollected()
        {
            int expectedNoData = 0;
            bool first = true;

            Collection<TVStation> sortedStations = TVStation.GetNameSortedStations(RunParameters.Instance.StationCollection);

            foreach (TVStation tvStation in sortedStations)
            {
                if (tvStation.Included && tvStation.EPGCollection.Count == 0)
                {
                    if (first)
                    {
                        Logger.Instance.WriteSeparator("Stations With No Data");
                        first = false;
                    }

                    bool dataOK = logStationCollected(tvStation);
                    if (!dataOK)
                        expectedNoData++;
                }
            }

            if (expectedNoData != 0)
                Logger.Instance.Write("Stations that should have had data = " + expectedNoData);

            Logger.Instance.WriteSeparator("Output Data");

            int stations = 0;
            foreach (TVStation tvStation in sortedStations)
            {
                if (tvStation.Included && tvStation.EPGCollection.Count != 0)
                {
                    logStationCollected(tvStation);
                    stations++;
                }
            }

            Logger.Instance.Write("<S> Summary: Total Stations = " + stations +
                " Total Gaps = " + totalGaps +
                " Total Overlaps = " + totalOverlaps +
                " Total Time = " + (DateTime.Now - startTime));
        }

        private static bool logStationCollected(TVStation tvStation)
        {
            int records = 0;
            int gaps = 0;
            int overlaps = 0;
            DateTime startTime = DateTime.MinValue;
            DateTime endTime = DateTime.MinValue;

            foreach (EPGEntry epgEntry in tvStation.EPGCollection)
            {
                records++;

                if (startTime == DateTime.MinValue)
                {
                    startTime = epgEntry.StartTime;
                    endTime = epgEntry.StartTime + epgEntry.Duration;
                }
                else
                {
                    if (epgEntry.StartTime < endTime)
                    {
                        if (!DebugEntry.IsDefined(DebugName.DontLogOverlaps))
                            Logger.Instance.Write("Station: " + tvStation.FixedLengthName + " (" + tvStation.FullID + ") Overlap at " + epgEntry.StartTime + " " + epgEntry.EventName);
                        overlaps++;
                    }
                    else
                    {
                        if (OptionEntry.IsDefined(OptionName.AcceptBreaks))
                        {
                            if (epgEntry.StartTime > endTime + new TimeSpan(0, 5, 0))
                            {
                                if (!DebugEntry.IsDefined(DebugName.DontLogGaps))
                                    Logger.Instance.Write("Station: " + tvStation.FixedLengthName + " (" + tvStation.FullID + ") Gap " + endTime + " to " + epgEntry.StartTime + " " + epgEntry.EventName);
                                gaps++;
                            }
                        }
                        else
                        {
                            if (epgEntry.StartTime > endTime)
                            {
                                if (!DebugEntry.IsDefined(DebugName.DontLogGaps))
                                    Logger.Instance.Write("Station: " + tvStation.FixedLengthName + " (" + tvStation.FullID + ") Gap " + endTime + " to " + epgEntry.StartTime + " " + epgEntry.EventName);
                                gaps++;
                            }
                        }
                    }

                    endTime = epgEntry.StartTime + epgEntry.Duration;
                }
            }

            bool reply;

            if (startTime == DateTime.MinValue)
            {
                string epg;

                if (tvStation.NextFollowingAvailable)
                {
                    if (tvStation.ScheduleAvailable)
                        epg = "NN&S";
                    else
                        epg = "NN";

                    reply = false;
                }
                else
                {
                    if (tvStation.ScheduleAvailable)
                    {
                        epg = "S";
                        reply = false;
                    }
                    else
                    {
                        epg = "None";
                        reply = true;
                    }
                }

                string dataMissing = tvStation.NextFollowingAvailable | tvStation.ScheduleAvailable ? " ** No data received **" : "No data broadcast";

                string epgLink = string.Empty;

                if (tvStation.EPGLink != null)
                    epgLink = " EPG Link: " + tvStation.EPGLink.OriginalNetworkID + "," +
                        tvStation.EPGLink.TransportStreamID + "," +
                        tvStation.EPGLink.ServiceID + " Time offset " + tvStation.EPGLink.TimeOffset;

                Logger.Instance.Write("Station: " + tvStation.FixedLengthName + " (" + tvStation.FullID + " EPG: " + epg + epgLink + ") " + dataMissing);
            }
            else
            {
                Logger.Instance.Write("Station: " + tvStation.FixedLengthName + " (" + tvStation.FullID + ") Start: " +
                    startTime + " End: " + endTime +
                    " Records: " + records +
                    " Overlaps: " + overlaps + " Gaps: " + gaps);
                reply = true;
            }

            totalOverlaps += overlaps;
            totalGaps += gaps;

            return (reply);
        }

        private static void runPluginCollection()
        {
            RunParameters.Instance = new RunParameters(ParameterSet.Plugin, RunType.Collection);
            ExitCode exitCode = RunParameters.Instance.Process(CommandLine.IniFileName);
            if (exitCode != ExitCode.OK)
            {
                HistoryRecord.Current.CollectionResult = CommandLine.GetCompletionCodeShortDescription(ExitCode.ParameterError);
                Logger.Write(HistoryRecord.Current);

                Logger.Instance.Write("<e> Plugin parameters incorrect");
                Logger.Instance.Write("<e> Exiting with code = " + (int)exitCode);
                Environment.Exit((int)exitCode);
            }

            PluginDataProvider dataProvider = new PluginDataProvider(RunParameters.Instance.FrequencyCollection[0], CommandLine.RunReference);
            string response = dataProvider.Run(CommandLine.RunReference);
            if (response != null)
            {
                HistoryRecord.Current.CollectionResult = CommandLine.GetCompletionCodeShortDescription(ExitCode.PluginNotStarted);
                Logger.Write(HistoryRecord.Current);

                Logger.Instance.Write("<e> Plugin failed to start - " + response);
                Logger.Instance.Write("<e> Exiting with code = " + ExitCode.PluginNotStarted);
                Environment.Exit((int)ExitCode.PluginNotStarted);
            }

            EPGController.ProcessComplete += new EPGController.ProcessCompleteHandler(pluginProcessComplete);
            EPGController.Instance.Run(dataProvider, RunParameters.Instance.FrequencyCollection[0]);

            BackgroundWorker pluginAbandonWorker = new BackgroundWorker();
            pluginAbandonWorker.WorkerSupportsCancellation = true;
            pluginAbandonWorker.DoWork += new DoWorkEventHandler(pluginAbandonWorkerDoWork);
            pluginAbandonWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(pluginAbandonWorkerCompleted);
            pluginAbandonWorker.RunWorkerAsync();

            bool reply = endProgramEvent.WaitOne();

            dataProvider.Stop();

            if (pluginAbandon)
            {
                HistoryRecord.Current.CollectionResult = CommandLine.GetCompletionCodeShortDescription(ExitCode.AbandonedByUser);
                Logger.Write(HistoryRecord.Current);

                Logger.Instance.Write("Cancelled by user - no data created");
                Logger.Instance.Write("<C> Exiting with code = " + (int)ExitCode.AbandonedByUser);
                return;
            }

            HistoryRecord.Current.CollectionDuration = DateTime.Now - startTime;

            EPGController.Instance.FinishRun();
            if (RunParameters.Instance.AbandonRequested)
                return;

            logDataCollected();

            string outputReply = OutputFile.ProcessPlugin();
            if (outputReply != null)
            {
                HistoryRecord.Current.CollectionResult = CommandLine.GetCompletionCodeShortDescription(ExitCode.OutputFileNotCreated);
                Logger.Write(HistoryRecord.Current);

                Logger.Instance.Write("<e> The output data could not be created");
                Logger.Instance.Write("<e> " + outputReply);
                Logger.Instance.Write("<C> Exiting with code = " + (int)ExitCode.OutputFileNotCreated);
                return;
            }

            int recordCount = TVStation.EPGCount(RunParameters.Instance.StationCollection);
            Logger.Instance.Write("<C> Finished - created " + recordCount + " EPG entries");
            HistoryRecord.Current.CollectionCount = recordCount;

            if (TVStation.EPGCount(RunParameters.Instance.StationCollection) == 0)
            {
                HistoryRecord.Current.CollectionResult = CommandLine.GetCompletionCodeShortDescription(ExitCode.NoDataCollected);
                Logger.Write(HistoryRecord.Current);

                Logger.Instance.Write("<e> No data collected");
                Logger.Instance.Write("<C> Exiting with code = " + (int)ExitCode.NoDataCollected);
                return;
            }

            if (OptionEntry.IsDefined(OptionName.StoreStationInfo))
            {
                string unloadReply = TVStation.Unload(Path.Combine(RunParameters.DataDirectory, "Station Cache.xml"), RunParameters.Instance.StationCollection);
                if (unloadReply != null)
                {
                    Logger.Instance.Write("<C> Failed to output station cache file");
                    Logger.Instance.Write("<C> " + unloadReply);
                }
                else
                    Logger.Instance.Write("Station cache file output successfully");
            }

            if (RunParameters.Instance.ChannelUpdateEnabled)
                updateChannels();

            HistoryRecord.Current.CollectionResult = CommandLine.GetCompletionCodeShortDescription(ExitCode.OK);
            Logger.Write(HistoryRecord.Current);

            Logger.Instance.Write("<C> Exiting with code = " + (int)ExitCode.OK);
            return;
        }

        private static void pluginProcessComplete(object sender, EventArgs e)
        {
            endProgramEvent.Set();
        }

        private static void pluginAbandonWorkerDoWork(object sender, DoWorkEventArgs e)
        {
            string cancellationName = "EPG Collector Cancellation Mutex " + CommandLine.RunReference;
            Logger.Instance.Write("Cancellation mutex name is " + cancellationName);

            Mutex cancelMutex = new Mutex(false, cancellationName);

            try
            {
                cancelMutex.WaitOne();
                cancelMutex.Close();
            }
            catch (AbandonedMutexException)
            {
                Logger.Instance.Write("<E> TVSource has failed - the collection will be abandoned");
            }

            pluginAbandon = true;

            endProgramEvent.Set();
        }

        private static void pluginAbandonWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                throw new InvalidOperationException("Plugin abandon background worker failed - see inner exception", e.Error);
        }

        private static void updateChannels()
        {
            Logger.Instance.WriteSeparator("Channel Update Starting");

            DVBLinkController.Load();

            if (RunParameters.Instance.ChannelReloadData)
                DVBLinkController.ClearData();

            Collection<Provider> providers = createProviderList();

            foreach (Provider provider in providers)
                provider.LogNetworkInfo();

            Collection<TuningFrequency> unprocessedFrequencies = new Collection<TuningFrequency>();
            int processed = 0;

            foreach (Provider provider in providers)
            {
                foreach (TuningFrequency frequency in provider.Frequencies)
                {
                    if (RunParameters.Instance.FrequencyCollection[0].SelectedTuners != null)
                    {
                        foreach (SelectedTuner selectedTuner in RunParameters.Instance.FrequencyCollection[0].SelectedTuners)
                            frequency.SelectedTuners.Add(selectedTuner);
                    }

                    if (!processFrequency(frequency))
                        unprocessedFrequencies.Add(frequency);
                    else
                        processed++;
                }
            }

            DVBLinkController.Unload();
            DVBLinkController.LogChannelMap(DebugEntry.IsDefined(DebugName.LogCaData));

            Logger.Instance.Write(string.Empty);
            Logger.Instance.Write(processed + " frequencies processed");
            Logger.Instance.Write(unprocessedFrequencies.Count + " frequencies not processed");
            Logger.Instance.Write(string.Empty);

            if (unprocessedFrequencies.Count != 0)
            {
                Logger.Instance.Write(string.Empty);

                Logger.Instance.Write("The following frequencies have not been processed:");

                foreach (TuningFrequency tuningFrequency in unprocessedFrequencies)
                {
                    string s2Comment = string.Empty;
                    SatelliteFrequency satelliteFrequency = tuningFrequency as SatelliteFrequency;
                    if (satelliteFrequency != null)
                        s2Comment = satelliteFrequency.IsS2 ? "(S2)" : string.Empty;
                    Logger.Instance.Write("    " + tuningFrequency.ToString() + " " + s2Comment);
                }
            }

            Logger.Instance.WriteSeparator("Channel Update Finished");
        }

        private static bool processFrequency(TuningFrequency frequency)
        {
            Thread.Sleep(1000);

            Logger.Instance.Write("Tuning " + frequency.ToString());
            if (!DebugEntry.IsDefined(DebugName.NotQuiet))
                Logger.Instance.QuietMode = true;

            bool tuned = tuneFrequency(frequency);
            Logger.Instance.QuietMode = false;

            if (!tuned)
            {
                Logger.Instance.Write("Channel updates for frequency " + frequency.ToString() + " abandoned - failed to tune");
                return (false);
            }

            FrequencyScanner scanner = new FrequencyScanner((ISampleDataProvider)graph);
            scanner.SearchOtherStream = false;
            Collection<TVStation> scannedStations = scanner.FindTVStations();

            graph.Dispose();
            graph = null;

            if (frequency.Stations != null)
            {
                foreach (TVStation scannedStation in scannedStations)
                {
                    TVStation providerStation = findStation(frequency, scannedStation);
                    if (providerStation != null)
                        providerStation.Update(scannedStation);
                    else
                        Logger.Instance.Write("Scanned station " + scannedStation.FullDescription + " not in network");
                }
            }
            else
            {
                foreach (TVStation scannedStation in scannedStations)
                {
                    TVStation station = TVStation.FindStation(RunParameters.Instance.StationCollection,
                        scannedStation.OriginalNetworkID, scannedStation.TransportStreamID, scannedStation.ServiceID);
                    if (station != null)
                    {
                        if (frequency.Stations == null)
                            frequency.Stations = new Collection<TVStation>();
                        frequency.Stations.Add(station);
                    }
                    else
                        Logger.Instance.Write("Scanned station " + scannedStation.FullDescription + " not loaded");
                }
            }

            DVBLinkController.Process(frequency);

            return (true);
        }

        private static TVStation findStation(TuningFrequency frequency, TVStation scannedStation)
        {
            foreach (TVStation station in frequency.Stations)
            {
                if (station.OriginalNetworkID == scannedStation.OriginalNetworkID && station.TransportStreamID == scannedStation.TransportStreamID && station.ServiceID == scannedStation.ServiceID)
                    return (station);
            }

            return (null);
        }

        private static Collection<Provider> createProviderList()
        {
            Collection<TransportStream> transportStreams = new Collection<TransportStream>();

            foreach (NetworkInformationSection networkInformationSection in NetworkInformationSection.NetworkInformationSections)
            {
                if (networkInformationSection.TransportStreams != null)
                {
                    foreach (TransportStream newStream in networkInformationSection.TransportStreams)
                    {
                        bool inserted = false;

                        foreach (TransportStream oldStream in transportStreams)
                        {
                            if (oldStream.Frequency > newStream.Frequency)
                            {
                                transportStreams.Insert(transportStreams.IndexOf(oldStream), newStream);
                                inserted = true;
                                break;
                            }
                        }

                        if (!inserted)
                            transportStreams.Add(newStream);
                    }
                }
            }

            switch (RunParameters.Instance.FrequencyCollection[0].TunerType)
            {
                case TunerType.Satellite:
                    return (processSatelliteStream(transportStreams));
                case TunerType.Terrestrial:
                    return (processTerrestrialStream(transportStreams));
                case TunerType.Cable:
                    return (processCableStream(transportStreams));
                default:
                    return (null);
            }
        }

        private static Collection<Provider> processSatelliteStream(Collection<TransportStream> transportStreams)
        {
            Collection<Provider> satellites = new Collection<Provider>();

            foreach (TransportStream transportStream in transportStreams)
            {
                int orbitalPosition = (NetworkInformationSection.GetOrbitalPosition(transportStream.OriginalNetworkID, transportStream.TransportStreamID));
                bool eastFlag = (NetworkInformationSection.GetEastFlag(transportStream.OriginalNetworkID, transportStream.TransportStreamID));

                Satellite satellite = findSatellite(satellites, NetworkInformationSection.NetworkInformationSections[0].NetworkName,
                    orbitalPosition, eastFlag);

                SatelliteFrequency frequency = findFrequency(satellite, NetworkInformationSection.GetFrequency(transportStream.OriginalNetworkID, transportStream.TransportStreamID) * 10);
                frequency.CollectionType = CollectionType.MHEG5;
                frequency.FEC = FECRate.ConvertDVBFecRate(transportStream.Fec);
                frequency.Modulation = getSatelliteModulation(transportStream.Modulation);
                frequency.DVBModulation = transportStream.Modulation;
                frequency.Pilot = SignalPilot.Pilot.NotSet;
                frequency.DVBPolarization = transportStream.Polarization;

                if (transportStream.IsS2)
                    frequency.RollOff = SignalRollOff.ConvertDVBRollOff(transportStream.RollOff);
                else
                    frequency.RollOff = SignalRollOff.RollOff.NotSet;

                frequency.SatelliteDish = ((SatelliteFrequency)RunParameters.Instance.FrequencyCollection[0]).SatelliteDish;
                frequency.SymbolRate = transportStream.SymbolRate * 100;
                frequency.Provider = satellite;
                frequency.ModulationSystem = transportStream.ModulationSystem;

                Collection<ServiceListEntry> serviceListEntries = transportStream.ServiceList;

                if (serviceListEntries != null && serviceListEntries.Count != 0)
                {
                    foreach (ServiceListEntry serviceListEntry in serviceListEntries)
                    {
                        TVStation station = TVStation.FindStation(RunParameters.Instance.StationCollection,
                            transportStream.OriginalNetworkID, transportStream.TransportStreamID,
                            serviceListEntry.ServiceID);
                        if (station != null)
                        {
                            if (frequency.Stations == null)
                                frequency.Stations = new Collection<TVStation>();
                            frequency.Stations.Add(station);
                        }
                    }
                }

                addFrequency(satellite.Frequencies, frequency);
            }

            return (satellites);
        }

        private static Satellite findSatellite(Collection<Provider> satellites, string name, int orbitalPosition, bool eastFlag)
        {
            string eastWest = eastFlag ? "east" : "west";

            foreach (Satellite satellite in satellites)
            {
                if (satellite.Name == name && satellite.Longitude == orbitalPosition && satellite.EastWest == eastWest)
                    return (satellite);
            }

            Satellite newSatellite = new Satellite();
            newSatellite.Name = name;
            newSatellite.Longitude = orbitalPosition;
            newSatellite.EastWest = eastWest;

            satellites.Add(newSatellite);

            return (newSatellite);
        }

        private static SatelliteFrequency findFrequency(Satellite satellite, int frequency)
        {
            foreach (SatelliteFrequency oldFrequency in satellite.Frequencies)
            {
                if (oldFrequency.Frequency == frequency)
                    return (oldFrequency);
                else
                {
                    if (oldFrequency.Frequency > frequency)
                    {
                        SatelliteFrequency insertFrequency = new SatelliteFrequency();
                        insertFrequency.Frequency = frequency;
                        satellite.Frequencies.Add(insertFrequency);
                        return (insertFrequency);
                    }
                }

            }

            SatelliteFrequency addFrequency = new SatelliteFrequency();
            addFrequency.Frequency = frequency;

            satellite.Frequencies.Add(addFrequency);

            return (addFrequency);
        }

        private static SignalModulation.Modulation getSatelliteModulation(int satelliteModulation)
        {
            switch (satelliteModulation)
            {
                case 1:
                    return SignalModulation.Modulation.QPSK;
                case 2:
                    return SignalModulation.Modulation.PSK8;
                case 3:
                    return SignalModulation.Modulation.QAM16;
                default:
                    return SignalModulation.Modulation.QPSK;
            }
        }

        private static void addFrequency(Collection<TuningFrequency> frequencies, SatelliteFrequency newFrequency)
        {
            foreach (TuningFrequency oldFrequency in frequencies)
            {
                SatelliteFrequency satelliteFrequency = oldFrequency as SatelliteFrequency;

                if (satelliteFrequency.Frequency == newFrequency.Frequency && satelliteFrequency.Polarization == newFrequency.Polarization)
                    return;

                if (satelliteFrequency.Frequency == newFrequency.Frequency)
                {
                    frequencies.Insert(frequencies.IndexOf(satelliteFrequency), newFrequency);
                    return;
                }

                if (satelliteFrequency.Frequency > newFrequency.Frequency)
                {
                    frequencies.Insert(frequencies.IndexOf(satelliteFrequency), newFrequency);
                    return;
                }
            }

            frequencies.Add(newFrequency);
        }

        private static Collection<Provider> processTerrestrialStream(Collection<TransportStream> transportStreams)
        {
            Collection<Provider> providers = new Collection<Provider>();

            foreach (TransportStream transportStream in transportStreams)
            {
                TerrestrialProvider provider = findTerrestrialProvider(providers, NetworkInformationSection.NetworkInformationSections[0].NetworkName);
                TerrestrialFrequency frequency = findFrequency(provider, NetworkInformationSection.GetFrequency(transportStream.OriginalNetworkID, transportStream.TransportStreamID) * 10);
                frequency.CollectionType = CollectionType.MHEG5;
                frequency.Bandwidth = transportStream.Bandwidth;

                Collection<ServiceListEntry> serviceListEntries = transportStream.ServiceList;

                if (serviceListEntries != null && serviceListEntries.Count != 0)
                {
                    foreach (ServiceListEntry serviceListEntry in serviceListEntries)
                    {
                        TVStation station = TVStation.FindStation(RunParameters.Instance.StationCollection,
                            transportStream.OriginalNetworkID, transportStream.TransportStreamID,
                            serviceListEntry.ServiceID);
                        if (station != null)
                        {
                            if (frequency.Stations == null)
                                frequency.Stations = new Collection<TVStation>();
                            frequency.Stations.Add(station);
                        }
                    }
                }

                addFrequency(provider.Frequencies, frequency);
            }

            return (providers);
        }

        private static TerrestrialProvider findTerrestrialProvider(Collection<Provider> providers, string name)
        {
            foreach (TerrestrialProvider provider in providers)
            {
                if (provider.Name == name)
                    return (provider);
            }

            TerrestrialProvider newProvider = new TerrestrialProvider(name);
            providers.Add(newProvider);

            return (newProvider);
        }

        private static TerrestrialFrequency findFrequency(TerrestrialProvider provider, int frequency)
        {
            foreach (TerrestrialFrequency oldFrequency in provider.Frequencies)
            {
                if (oldFrequency.Frequency == frequency)
                    return (oldFrequency);
                else
                {
                    if (oldFrequency.Frequency > frequency)
                    {
                        TerrestrialFrequency insertFrequency = new TerrestrialFrequency();
                        insertFrequency.Frequency = frequency;
                        provider.Frequencies.Add(insertFrequency);
                        return (insertFrequency);
                    }
                }

            }

            TerrestrialFrequency addFrequency = new TerrestrialFrequency();
            addFrequency.Frequency = frequency;

            provider.Frequencies.Add(addFrequency);

            return (addFrequency);
        }

        private static Collection<Provider> processCableStream(Collection<TransportStream> transportStreams)
        {
            Collection<Provider> providers = new Collection<Provider>();

            foreach (TransportStream transportStream in transportStreams)
            {
                CableProvider provider = findCableProvider(providers, NetworkInformationSection.NetworkInformationSections[0].NetworkName);
                CableFrequency frequency = findFrequency(provider, NetworkInformationSection.GetFrequency(transportStream.OriginalNetworkID, transportStream.TransportStreamID) / 10);
                frequency.CollectionType = CollectionType.MHEG5;
                frequency.FEC = FECRate.ConvertDVBFecRate(transportStream.CableFec);
                frequency.DVBModulation = transportStream.CableModulation;
                frequency.Modulation = getCableModulation(transportStream.CableModulation);
                frequency.SymbolRate = transportStream.CableSymbolRate * 100;

                Collection<ServiceListEntry> serviceListEntries = transportStream.ServiceList;

                if (serviceListEntries != null && serviceListEntries.Count != 0)
                {
                    foreach (ServiceListEntry serviceListEntry in serviceListEntries)
                    {
                        TVStation station = TVStation.FindStation(RunParameters.Instance.StationCollection,
                            transportStream.OriginalNetworkID, transportStream.TransportStreamID,
                            serviceListEntry.ServiceID);
                        if (station != null)
                        {
                            if (frequency.Stations == null)
                                frequency.Stations = new Collection<TVStation>();
                            frequency.Stations.Add(station);
                        }
                    }
                }

                addFrequency(provider.Frequencies, frequency);
            }

            return (providers);
        }

        private static CableProvider findCableProvider(Collection<Provider> providers, string name)
        {
            foreach (CableProvider provider in providers)
            {
                if (provider.Name == name)
                    return (provider);
            }

            CableProvider newProvider = new CableProvider(name);
            providers.Add(newProvider);

            return (newProvider);
        }

        private static CableFrequency findFrequency(CableProvider provider, int frequency)
        {
            foreach (CableFrequency oldFrequency in provider.Frequencies)
            {
                if (oldFrequency.Frequency == frequency)
                    return (oldFrequency);
                else
                {
                    if (oldFrequency.Frequency > frequency)
                    {
                        CableFrequency insertFrequency = new CableFrequency();
                        insertFrequency.Frequency = frequency;
                        provider.Frequencies.Add(insertFrequency);
                        return (insertFrequency);
                    }
                }

            }

            CableFrequency addFrequency = new CableFrequency();
            addFrequency.Frequency = frequency;

            provider.Frequencies.Add(addFrequency);

            return (addFrequency);
        }

        private static SignalModulation.Modulation getCableModulation(int cableModulation)
        {
            switch (cableModulation)
            {
                case 1:
                    return SignalModulation.Modulation.QAM16;
                case 2:
                    return SignalModulation.Modulation.QAM32;
                case 3:
                    return SignalModulation.Modulation.QAM64;
                case 4:
                    return SignalModulation.Modulation.QAM128;
                case 5:
                    return SignalModulation.Modulation.QAM256;
                default:
                    return SignalModulation.Modulation.QAM64;
            }
        }

        private static void addFrequency(Collection<TuningFrequency> frequencies, TuningFrequency newFrequency)
        {
            foreach (TuningFrequency oldFrequency in frequencies)
            {
                if (oldFrequency.Frequency == newFrequency.Frequency)
                    return;

                if (oldFrequency.Frequency > newFrequency.Frequency)
                {
                    frequencies.Insert(frequencies.IndexOf(oldFrequency), newFrequency);
                    return;
                }
            }

            frequencies.Add(newFrequency);
        }
    }
}
