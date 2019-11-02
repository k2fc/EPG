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

using System.IO;

namespace DomainObjects
{
    /// <summary>
    /// The class that processes and stores command line parameters.
    /// </summary>
    public sealed class CommandLine
    {
        /// <summary>
        /// Returns true if the run is to query the tuners; false otherwise.
        /// </summary>
        public static bool TunerQueryOnly { get { return (tunerQueryOnly); } }
        /// <summary>
        /// Get the initialization file name.
        /// </summary>
        public static string IniFileName { get { return (iniFileName); } }
        /// <summary>
        /// Get the background mode.
        /// </summary>
        public static bool BackgroundMode { get { return (backgroundMode); } }
        /// <summary>
        /// Get the plugin mode.
        /// </summary>
        public static bool PluginMode { get { return (pluginMode); } }
        /// <summary>
        /// Get the run reference number.
        /// </summary>
        public static string RunReference { get { return (runReference); } }
        /// <summary>
        /// Returns true if compatability with the Mono environment requested.
        /// </summary>
        public static bool MonoCompatible { get { return (monoCompatible); } }
        /// <summary>
        /// Returns true if compatability with the Wine environment requested.
        /// </summary>
        public static bool WineCompatible { get { return (wineCompatible); } }
        /// <summary>
        /// Returns true if dummy test tuners are to be created.
        /// </summary>
        public static bool DummyTuners { get { return (dummyTuners); } }
        /// <summary>
        /// Returns true if WMC is assumed to be present.
        /// </summary>
        public static bool WmcPresent { get { return (wmcPresent); } }
        
        private static bool tunerQueryOnly;
        private static string iniFileName = Path.Combine(RunParameters.DataDirectory, "EPG Collector.ini");
        private static bool backgroundMode;
        private static bool pluginMode;
        private static string runReference;
        private static bool monoCompatible;
        private static bool wineCompatible;
        private static bool dummyTuners;
        private static bool wmcPresent;
        
        private CommandLine() { }

        /// <summary>
        /// Process the command line.
        /// </summary>
        /// <param name="args">The command line parameters.</param>
        /// <returns>True if the command line parameters are valid; false otherwise.</returns>
        public static bool Process(string[] args)
        {
            if (args.Length == 0)
                return (true);

            foreach (string arg in args)
            {
                Logger.Instance.Write("Processing command line parameter: " + arg);

                string[] parts = arg.Split(new char[] { '=' });

                switch (parts[0].ToUpperInvariant())
                {
                    case "/TUNERS":
                        if (args.Length != 1 || parts.Length != 1)
                        {
                            Logger.Instance.Write("Command line parameter wrong");
                            return (false);
                        }
                        else
                        {
                            tunerQueryOnly = true;
                            break;
                        }
                    case "/INI":
                        if (parts[1].Trim().Length == 0)
                        {
                            Logger.Instance.Write("Command line parameter wrong");
                            return (false);
                        }
                        else
                            iniFileName = parts[1];
                        break;
                    case "/BACKGROUND":
                        if (parts.Length != 2)
                        {
                            Logger.Instance.Write("Command line parameter wrong");
                            return (false);
                        }
                        else
                        {
                            backgroundMode = true;
                            runReference = parts[1];
                        }
                        break;
                    case "/PLUGIN":
                        if (args.Length != 2 || parts.Length != 2)
                        {
                            Logger.Instance.Write("Command line parameter wrong");
                            return (false);
                        }
                        else
                        {
                            pluginMode = true;
                            runReference = parts[1];
                        }
                        break;
                    case "/MONO":
                        if (parts.Length != 1)
                        {
                            Logger.Instance.Write("Command line parameter wrong");
                            return (false);
                        }
                        else
                        {
                            monoCompatible = true;
                            break;
                        }
                    case "/WINE":
                        if (parts.Length != 1)
                        {
                            Logger.Instance.Write("Command line parameter wrong");
                            return (false);
                        }
                        else
                        {
                            wineCompatible = true;
                            break;
                        }
                    case "/DUMMYTUNERS":
                        if (parts.Length != 1)
                        {
                            Logger.Instance.Write("Command line parameter wrong");
                            return (false);
                        }
                        else
                        {
                            dummyTuners = true;
                            break;
                        }
                    case "/WMCPRESENT":
                        if (parts.Length != 1)
                        {
                            Logger.Instance.Write("Command line parameter wrong");
                            return (false);
                        }
                        else
                        {
                            wmcPresent = true;
                            break;
                        }
                    default:
                        Logger.Instance.Write("Command line parameter not recognized: " + parts[0]);
                        return (false);
                }
            }

            return (true);
        }

        /// <summary>
        /// Get a description of the program exit code.
        /// </summary>
        /// <param name="exitCode">The exit code.</param>
        /// <returns>A description of the exit code.</returns>
        public static string GetCompletionCodeDescription(ExitCode exitCode)
        {
            switch (exitCode)
            {
                case ExitCode.OK:
                    return ("The run completed successfully.");
                case ExitCode.NoDVBTuners:
                    return ("No DVB tuners installed.");
                case ExitCode.ParameterFileNotFound:
                    return ("The initialization file cannot be located.");
                case ExitCode.ParameterError:
                    return ("The initialization file or parameter file has a parameter error.");
                case ExitCode.CommandLineWrong:
                    return ("The command line is incorrect.");
                case ExitCode.SoftwareException:
                    return ("A program exception occurred.");
                case ExitCode.EPGDataIncomplete:
                    return ("The EPG data is incomplete.");
                case ExitCode.AbandonedByUser:
                    return ("The collection was abandoned by the user.");
                case ExitCode.ParameterTunerMismatch:
                    return ("The initialization file parameters do not match the tuner configuration.");
                case ExitCode.LogFileNotAvailable:
                    return ("The log file cannot be written.");
                case ExitCode.SomeFrequenciesNotProcessed:
                    return ("Some frequencies could not be processed. See the log for details");
                case ExitCode.OutputFileNotCreated:
                    return ("The output file could not be created.");
                case ExitCode.SimulationFileError:
                    return ("The simulation file could not be located or failed to load.");
                case ExitCode.NoDataCollected:
                    return ("The collection finished normally but no data was collected.");
                case ExitCode.NoBDATunerFilter:
                    return ("No BDA tuner filter located.");
                case ExitCode.HardwareFilterChainNotBuilt:
                    return ("The hardware filter chain could not be built.");
                case ExitCode.PluginNotStarted:
                    return ("The DVBLogic plugin could not start.");
                default:
                    return ("The exit code is not recognized.");
            }
        }

        /// <summary>
        /// Get a description of the program exit code.
        /// </summary>
        /// <param name="exitCode">The exit code.</param>
        /// <returns>A description of the exit code.</returns>
        public static string GetCompletionCodeShortDescription(ExitCode exitCode)
        {
            switch (exitCode)
            {
                case ExitCode.OK:
                    return ("OK");
                case ExitCode.NoDVBTuners:
                    return ("No tuners installed");
                case ExitCode.ParameterFileNotFound:
                    return ("Ini file not found");
                case ExitCode.ParameterError:
                    return ("Ini file error");
                case ExitCode.CommandLineWrong:
                    return ("Command line error");
                case ExitCode.SoftwareException:
                    return ("Program exception");
                case ExitCode.EPGDataIncomplete:
                    return ("EPG data incomplete");
                case ExitCode.AbandonedByUser:
                    return ("Abandoned by user");
                case ExitCode.ParameterTunerMismatch:
                    return ("Tuner mismatch");
                case ExitCode.LogFileNotAvailable:
                    return ("Log file error");
                case ExitCode.SomeFrequenciesNotProcessed:
                    return ("Frequencies not processed");
                case ExitCode.OutputFileNotCreated:
                    return ("Output file not created");
                case ExitCode.SimulationFileError:
                    return ("Simulation file error");
                case ExitCode.NoDataCollected:
                    return ("No data collected");
                case ExitCode.NoBDATunerFilter:
                    return ("No BDA filter");
                case ExitCode.HardwareFilterChainNotBuilt:
                    return ("Hardware filter chain error");
                case ExitCode.PluginNotStarted:
                    return ("Plugin not started");
                default:
                    return ("Unknown exit code");
            }
        }
    }
}
