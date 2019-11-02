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
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Collections.ObjectModel;

namespace DomainObjects
{
    /// <summary>
    /// The base class for XMLTV or MXF imports.
    /// </summary>
    public abstract class ImportFileBase
    {
        private static bool temporaryFile;
        private static string temporaryFileName;

        /// <summary>
        /// Process an MXF file.
        /// </summary>
        /// <param name="fileName">The actual file path.</param>
        /// <param name="fileSpec">The file definition.</param>
        /// <returns>An error message or null if the file is processed successfully.</returns>
        public abstract string Process(string fileName, ImportFileSpec fileSpec);

        /// <summary>
        /// Process the channel information from an MXF file.
        /// </summary>
        /// <param name="fileName">The actual file path.</param>
        /// <param name="fileSpec">The file definition.</param>
        /// <returns>An error message or null if the file was processed successfully.</returns>
        public abstract string ProcessChannels(string fileName, ImportFileSpec fileSpec);

        /// <summary>
        /// Get the real name of the import file.
        /// </summary>
        /// <param name="fileName">The top level name of the file.</param>
        /// <returns>The target name of the file.</returns>
        public static string GetActualFileName(string fileName)
        {
            temporaryFile = false;
            temporaryFileName = null;

            try
            {
                Uri checkPath = new Uri(fileName);
                if (checkPath.IsFile)
                {
                    temporaryFileName = decompressFile(fileName, fileName);
                    temporaryFile = temporaryFileName != fileName;

                    return (temporaryFileName);
                }
            }
            catch (UriFormatException e)
            {
                Logger.Instance.Write("<e> Format error in " + fileName);
                Logger.Instance.Write("<e> " + e.Message);
                return (fileName);
            }

            try
            {
                WebClient webClient = new WebClient();
                temporaryFileName = Path.GetTempFileName();
                temporaryFile = true;

                Logger.Instance.Write("Downloading " + fileName + " to temporary file " + temporaryFileName);
                webClient.DownloadFile(fileName, temporaryFileName);
                Logger.Instance.Write("File downloaded successfully");

                string actualName = decompressFile(fileName, temporaryFileName);

                if (actualName != temporaryFileName)
                {
                    try
                    {
                        File.Delete(temporaryFileName);
                        Logger.Instance.Write("Deleted temporary download file " + temporaryFileName);
                    }
                    catch (IOException e)
                    {
                        Logger.Instance.Write("<e> Failed to delete temporary download file " + temporaryFileName);
                        Logger.Instance.Write("<e> " + e.Message);
                    }
                }

                temporaryFileName = actualName;
                return (temporaryFileName);
            }
            catch (WebException e)
            {
                Logger.Instance.Write("<e> Failed to download " + fileName);
                Logger.Instance.Write("<e> " + e.Message);
                return (Path.GetTempFileName());
            }
        }

        private static string decompressFile(string originalFileName, string inputFileName)
        {
            if (originalFileName.ToLowerInvariant().EndsWith(".gz"))
                return (decompressGZFile(inputFileName));

            return (inputFileName);
        }

        private static string decompressGZFile(string inputFileName)
        {
            Logger.Instance.Write("Decompressing GZ file " + inputFileName);

            string outputFileName = Path.GetTempFileName();
            Logger.Instance.Write("GZ file will be decompressed to " + outputFileName);

            FileStream inputStream = null;
            FileStream outputStream = null;
            GZipStream gzipStream = null;

            try
            {
                inputStream = new FileStream(inputFileName, FileMode.Open);
                outputStream = new FileStream(outputFileName, FileMode.Create);
                gzipStream = new GZipStream(inputStream, CompressionMode.Decompress);

                byte[] buffer = new byte[4096];
                int count = -1;

                while (count != 0)
                {
                    count = gzipStream.Read(buffer, 0, 4096);
                    if (count != 0)
                        outputStream.Write(buffer, 0, count);
                }

                Logger.Instance.Write("Decompress successful");
            }
            catch (IOException e)
            {
                Logger.Instance.Write("<e> Failed to decompress GZ file " + inputFileName);
                Logger.Instance.Write("<e> " + e.Message);
            }

            if (gzipStream != null)
                gzipStream.Close();
            if (inputStream != null)
                inputStream.Close();
            if (outputStream != null)
                outputStream.Close();

            return (outputFileName);
        }

        /// <summary>
        /// Delete any temporary file.
        /// </summary>
        public static void DeleteTemporaryFile()
        {
            if (!temporaryFile)
                return;

            try
            {
                File.Delete(temporaryFileName);
                Logger.Instance.Write("Deleted temporary file " + temporaryFileName);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("<e> Failed to delete temporary file " + temporaryFileName);
                Logger.Instance.Write("<e> " + e.Message);
            }
        }

        /// <summary>
        /// Extract a number from a string.
        /// </summary>
        /// <param name="prefix">The prefix to be ignored. Can be null.</param>
        /// <param name="parameter">The string to be processed.</param>
        /// <returns>The extracted number.</returns>
        public static int GetNumber(string prefix, string parameter)
        {
            int startIndex = 0;

            if (prefix != null)
            {
                if (!parameter.StartsWith(prefix))
                    throw (new ArgumentException("the mxf string prefix is not " + prefix));

                startIndex = prefix.Length;
            }

            return (Int32.Parse(parameter.Substring(startIndex)));
        }

        /// <summary>
        /// Merge the XMLTV data with the broadcast data.
        /// </summary>
        /// <param name="newChannels">The collection of XMLTV channels.</param>
        /// <param name="precedence">The data precedence.</param>
        /// <param name="appendOnly">True if the data without precedence is to be appended to the data with precedence; false for a normal merge.</param>
        public void MergeChannels(Collection<TVStation> newChannels, DataPrecedence precedence, bool appendOnly)
        {
            Logger.Instance.Write("Merging imported channels with broadcast data");

            foreach (TVStation newStation in newChannels)
            {
                bool merged = false;

                foreach (TVStation existingStation in RunParameters.Instance.StationCollection)
                {
                    if (existingStation.Included)
                    {
                        merged = matchChannels(existingStation, newStation);
                        if (merged)
                        {
                            mergeChannel(existingStation, newStation, precedence, appendOnly);
                            Logger.Instance.Write("Merged data for channel '" + existingStation.FullDescription + "' EPG entries = " + existingStation.EPGCollection.Count);
                            merged = true;
                            break;
                        }
                    }
                }

                if (!merged)
                {
                    RunParameters.Instance.StationCollection.Add(newStation);
                    Logger.Instance.Write("Added data for channel '" + newStation.FullDescription + "' EPG entries = " + newStation.EPGCollection.Count);
                }
            }
        }

        private bool matchChannels(TVStation existingStation, TVStation newStation)
        {
            if (newStation.UseNameForMerge)
            {
                if (string.IsNullOrWhiteSpace(existingStation.NewName))
                    return existingStation.Name.Trim().ToLowerInvariant() == newStation.Name.Trim().ToLowerInvariant();
                else
                    return existingStation.NewName.Trim().ToLowerInvariant() == newStation.Name.Trim().ToLowerInvariant();
            }

            if (existingStation.ServiceID == -1 || newStation.ServiceID == -1)
            {
                string existingName = existingStation.NewName != null ? existingStation.NewName : existingStation.Name;
                string newName = newStation.NewName != null ? newStation.NewName : newStation.Name;

                return (existingStation.Included && existingName.Trim().ToLowerInvariant() == newName.Trim().ToLowerInvariant());
            }
            else
                return (existingStation.OriginalNetworkID == newStation.OriginalNetworkID &&
                    existingStation.TransportStreamID == newStation.TransportStreamID &&
                    existingStation.ServiceID == newStation.ServiceID);
        }

        private void mergeChannel(TVStation oldStation, TVStation newStation, DataPrecedence precedence, bool appendOnly)
        {
            if (newStation.EPGCollection == null || newStation.EPGCollection.Count == 0)
                return;

            if (oldStation.EPGCollection == null || oldStation.EPGCollection.Count == 0)
            {
                oldStation.EPGCollection = new Collection<EPGEntry>();

                foreach (EPGEntry epgEntry in newStation.EPGCollection)
                    oldStation.EPGCollection.Add(epgEntry);

                return;
            }

            if (appendOnly)
            {
                if (precedence == DataPrecedence.Broadcast)
                {
                    DateTime lastBroadcastTime = oldStation.EPGCollection[oldStation.EPGCollection.Count - 1].StartTime + oldStation.EPGCollection[oldStation.EPGCollection.Count - 1].Duration;

                    foreach (EPGEntry epgEntry in newStation.EPGCollection)
                    {
                        if (epgEntry.StartTime >= lastBroadcastTime)
                            oldStation.EPGCollection.Add(epgEntry);
                    }
                }
                else
                {
                    DateTime lastBroadcastTime = newStation.EPGCollection[newStation.EPGCollection.Count - 1].StartTime + newStation.EPGCollection[newStation.EPGCollection.Count - 1].Duration;

                    foreach (EPGEntry epgEntry in oldStation.EPGCollection)
                    {
                        if (epgEntry.StartTime >= lastBroadcastTime)
                            newStation.EPGCollection.Add(epgEntry);
                    }

                    oldStation.EPGCollection = newStation.EPGCollection;
                }

                return;
            }

            bool done = false;

            int oldIndex = 0;
            int newIndex = 0;

            while (!done)
            {
                DateTime oldStartTime = oldStation.EPGCollection[oldIndex].StartTime;
                DateTime newStartTime = newStation.EPGCollection[newIndex].StartTime;

                if (oldStartTime == newStartTime)
                {
                    mergeProgramme(oldStation.EPGCollection[oldIndex], newStation.EPGCollection[newIndex], precedence);

                    if (oldIndex + 1 < oldStation.EPGCollection.Count)
                        oldIndex++;
                    newIndex++;
                }
                else
                {
                    if (oldStartTime.CompareTo(newStartTime) < 0)
                    {
                        if (oldIndex + 1 < oldStation.EPGCollection.Count)
                            oldIndex++;
                        else
                        {
                            oldStation.EPGCollection.Add(newStation.EPGCollection[newIndex]);
                            newIndex++;
                        }
                    }
                    else
                    {
                        oldStation.EPGCollection.Insert(oldIndex, newStation.EPGCollection[newIndex]);
                        newIndex++;
                    }
                }

                done = newIndex == newStation.EPGCollection.Count;
            }
        }

        private void mergeProgramme(EPGEntry oldProgramme, EPGEntry newProgramme, DataPrecedence precedence)
        {
            if (newProgramme.EventName != null && precedence == DataPrecedence.File)
                oldProgramme.EventName = newProgramme.EventName;

            if (newProgramme.ShortDescription != null && precedence == DataPrecedence.File)
                oldProgramme.ShortDescription = newProgramme.ShortDescription;

            if (newProgramme.EventSubTitle != null && precedence == DataPrecedence.File)
                oldProgramme.EventSubTitle = newProgramme.EventSubTitle;

            if (newProgramme.Date != null && precedence == DataPrecedence.File)
                oldProgramme.Date = newProgramme.Date;

            if (newProgramme.EventCategory != null && precedence == DataPrecedence.File)
                oldProgramme.EventCategory = newProgramme.EventCategory;

            if (newProgramme.ParentalRating != null && precedence == DataPrecedence.File)
            {
                oldProgramme.ParentalRating = newProgramme.ParentalRating;
                oldProgramme.ParentalRatingSystem = newProgramme.ParentalRatingSystem;
            }

            if (newProgramme.VideoQuality != null && precedence == DataPrecedence.File)
            {
                oldProgramme.VideoQuality = newProgramme.VideoQuality;
                oldProgramme.AspectRatio = newProgramme.AspectRatio;
            }

            if (newProgramme.AudioQuality != null && precedence == DataPrecedence.File)
                oldProgramme.AudioQuality = newProgramme.AudioQuality;

            if (newProgramme.StarRating != null && precedence == DataPrecedence.File)
                oldProgramme.StarRating = newProgramme.StarRating;

            if (newProgramme.SubTitles != null && precedence == DataPrecedence.File)
                oldProgramme.SubTitles = newProgramme.SubTitles;

            if (newProgramme.PreviousPlayDate != DateTime.MinValue && precedence == DataPrecedence.File)
                oldProgramme.PreviousPlayDate = newProgramme.PreviousPlayDate;

            if (newProgramme.SeriesId != null && precedence == DataPrecedence.File)
                oldProgramme.SeriesId = newProgramme.SeriesId;
            if (newProgramme.EpisodeId != null && precedence == DataPrecedence.File)
                oldProgramme.EpisodeId = newProgramme.EpisodeId;
            if (newProgramme.PartNumber != null && precedence == DataPrecedence.File)
                oldProgramme.PartNumber = newProgramme.PartNumber;

            if (newProgramme.SeasonNumber != -1 && precedence == DataPrecedence.File)
                oldProgramme.SeasonNumber = newProgramme.SeasonNumber;
            if (newProgramme.EpisodeNumber != -1 && precedence == DataPrecedence.File)
                oldProgramme.EpisodeNumber = newProgramme.EpisodeNumber;

            if (newProgramme.Directors != null && precedence == DataPrecedence.File)
                oldProgramme.Directors = newProgramme.Directors;

            if (newProgramme.Cast != null && precedence == DataPrecedence.File)
                oldProgramme.Cast = newProgramme.Cast;

            if (newProgramme.GuestStars != null && precedence == DataPrecedence.File)
                oldProgramme.GuestStars = newProgramme.GuestStars;

            if (newProgramme.Presenters != null && precedence == DataPrecedence.File)
                oldProgramme.Presenters = newProgramme.Presenters;

            if (newProgramme.Producers != null && precedence == DataPrecedence.File)
                oldProgramme.Producers = newProgramme.Producers;

            if (newProgramme.Writers != null && precedence == DataPrecedence.File)
                oldProgramme.Writers = newProgramme.Writers;

            if (newProgramme.LanguageCode != null && precedence == DataPrecedence.File)
                oldProgramme.LanguageCode = newProgramme.LanguageCode;

            if (precedence == DataPrecedence.Broadcast)
                oldProgramme.NoLookup = false;
            else
                oldProgramme.NoLookup = newProgramme.NoLookup;
        }
    }
}
