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
using System.Diagnostics;
using System.Threading;
using System.IO;

namespace DomainObjects
{
    /// <summary>
    /// The class that describes the WMC utility function.
    /// </summary>
    public sealed class WMCUtility
    {
        private static Process process;
        private static bool exited;

        /// <summary>
        /// Run the utility functions.
        /// </summary>
        /// <param name="description">The description of the function.</param>
        /// <param name="arguments">The parameters to the function.</param>
        /// <returns></returns>
        public static string Run(string description, string arguments)
        {
            Logger.Instance.Write("Running Windows Media Centre Utility to " + description);

            process = new Process();

            if (Environment.OSVersion.Version.Major < 6)
                return ("Windows Media Centre Utility cannot run on this version of Windows (" + Environment.OSVersion + ")");
            
            /*switch (Environment.OSVersion.Version.Minor)
            {
                case 0:
                    process.StartInfo.FileName = "WMCUtilityVista.exe";
                    break;
                case 1:
                    process.StartInfo.FileName = "WMCUtility.exe";
                    break;
                case 2:
                    process.StartInfo.FileName = "WMCUtilityWin8.exe";
                    break;
                case 3:
                    process.StartInfo.FileName = "WMCUtilityWin81.exe";
                    break;
                default:
                    process.StartInfo.FileName = "WMCUtilityWin81.exe";
                    break;
            }*/

            FileVersionInfo fileVersionInfo;

            try
            {
                fileVersionInfo =  FileVersionInfo.GetVersionInfo(Path.Combine(Environment.GetEnvironmentVariable("windir"), Path.Combine("ehome", "mcepg.dll")));
                if (fileVersionInfo == null)
                    return ("Windows Media Centre Utility cannot run because the file mcepg.dll has no file version number");
                
                Logger.Instance.Write("The file version number for mcepg.dll is " + fileVersionInfo.FileVersion);
            }
            catch (FileNotFoundException)
            {
                return ("Windows Media Centre Utility cannot run - can't find Windows Media Centre file mcepg.dll");
            }

            /*switch (fileVersionInfo.FileMinorPart)
            {
                case 1:
                    if (fileVersionInfo.FileBuildPart < 7000)
                        process.StartInfo.FileName = "WMCUtilityVista.exe";
                    else
                        process.StartInfo.FileName = "WMCUtility.exe";
                    break;
                case 2:
                    process.StartInfo.FileName = "WMCUtilityWin8.exe";
                    break;
                case 3:
                    process.StartInfo.FileName = "WMCUtilityWin81.exe";
                    break;
                default:
                    process.StartInfo.FileName = "WMCUtilityWin81.exe";
                    break;
            }*/

            process.StartInfo.FileName = "WMCUtility.exe";
            /*Logger.Instance.Write("Windows Media Centre Utility run from " + process.StartInfo.FileName);*/

            process.StartInfo.WorkingDirectory = Environment.CurrentDirectory;

            if (arguments != null)
                process.StartInfo.Arguments = arguments;

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.EnableRaisingEvents = true;
            process.Exited += new EventHandler(processExited);
            process.OutputDataReceived += new DataReceivedEventHandler(processOutputDataReceived);
            process.ErrorDataReceived += new DataReceivedEventHandler(processErrorDataReceived);

            exited = false;

            try
            {
                process.Start();

                process.BeginOutputReadLine();
                process.BeginErrorReadLine();

                while (!exited)
                    Thread.Sleep(500);

                Logger.Instance.Write("Windows Media Centre Utility has completed: exit code " + process.ExitCode);
                if (process.ExitCode == 0)
                    return (null);
                else
                    return ("Windows Media Centre failed: reply code " + process.ExitCode);
            }
            catch (Exception e)
            {
                Logger.Instance.Write("<e> Failed to run the Windows Media Centre Utility");
                Logger.Instance.Write("<e> " + e.Message);
                return ("Failed to run Windows Media Centre Utility due to an exception");
            }
        }

        private static void processOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            Logger.Instance.Write(e.Data);
        }

        private static void processErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            Logger.Instance.Write(e.Data);
        }

        private static void processExited(object sender, EventArgs e)
        {
            exited = true;
        }
    }
}
