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
    /// The class that controls creation of the output file.
    /// </summary>
    public sealed class OutputFile
    {
        /// <summary>
        /// Get or set the flag that forces the output to use Unicode encoding. The default is UTF-8.
        /// </summary>
        public static bool UseUnicodeEncoding
        {
            get { return (useUnicodeEncoding); }
            set { useUnicodeEncoding = value; }
        }

        private static bool useUnicodeEncoding;
        
        private OutputFile() { }

        /// <summary>
        /// Generate the output for normal collections.
        /// </summary>
        /// <returns>Null if successful; an error message otherwise.</returns>
        public static string Process()
        {
            string reply = null;

            if (RunParameters.Instance.OutputFileSet)
            {
                reply = OutputFileXML.Process(RunParameters.Instance.OutputFileName);
                if (reply != null)
                    return (reply);
            }

            if (OptionEntry.IsDefined(OptionName.WmcImport))
            {
                reply = OutputFileMXF.Process();
                if (reply != null)
                    return (reply);
            }

            if (RunParameters.Instance.ImportingToDvbViewer)
            {
                reply = OutputFileDVBViewer.Process();
                if (reply != null)
                    return (reply);
            }

            return (null);                       
        }

        /// <summary>
        /// Generate the output for plugin collections.
        /// </summary>
        /// <returns>Null if successful; an error message otherwise.</returns>
        public static string ProcessPlugin()
        {
            string reply = null;

            if (RunParameters.Instance.OutputFileSet)
            {
                reply = OutputFile.Process();
                if (reply != null)
                    return (reply);
            }

            if (OptionEntry.IsDefined(OptionName.WmcImport))
            {
                reply = OutputFileMXF.Process();
                if (reply != null)
                    return (reply);
            }

            if (OptionEntry.IsDefined(OptionName.PluginImport)  || (!RunParameters.Instance.OutputFileSet && !OptionEntry.IsDefined(OptionName.WmcImport)))
            {
                FileInfo iniFileInfo = new FileInfo(CommandLine.IniFileName);
                string outputFileName = Path.Combine(iniFileInfo.DirectoryName, "EPG Collector Plugin.xml");
                reply = OutputFilePlugin.Process(outputFileName);
                if (reply != null)
                    return (reply);                    
            }

            return (null);
        }
    }
}
