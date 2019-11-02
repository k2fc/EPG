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
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Collections.ObjectModel;

namespace DomainObjects
{
    /// <summary>
    /// The class that describes the xml DVBLogic output file.
    /// </summary>
    public sealed class OutputFilePlugin
    {
        private OutputFilePlugin() { }

        /// <summary>
        /// Create the file.
        /// </summary>
        /// <param name="fileName">The name of the file to be created.</param>
        /// <returns></returns>
        public static string Process(string fileName)
        {
            try
            {
                Logger.Instance.Write("Deleting any existing version of output file");
                File.SetAttributes(fileName, FileAttributes.Normal);
                File.Delete(fileName);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("File delete exception: " + e.Message);
            }

            Logger.Instance.Write("Creating plugin output file: " + fileName);

            XmlWriterSettings settings = new XmlWriterSettings();

            settings.Indent = true;
            settings.NewLineOnAttributes = false;
            settings.Encoding = new UTF8Encoding(false);
            settings.CloseOutput = true;

            setChannelID();

            try
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(fileName, settings))
                {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("EPGInfo");

                    foreach (TVStation tvStation in RunParameters.Instance.StationCollection)
                    {
                        if (tvStation.Included && tvStation.EPGCollection.Count != 0)
                        {
                            xmlWriter.WriteStartElement("Channel");

                            processStationHeader(xmlWriter, tvStation);
                            processStationEPG(xmlWriter, tvStation);

                            xmlWriter.WriteEndElement();
                        }
                    }

                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndDocument();

                    xmlWriter.Flush();
                    xmlWriter.Close();
                }
            }
            catch (XmlException ex1)
            {
                return (ex1.Message);
            }
            catch (IOException ex2)
            {
                return (ex2.Message);
            }

            if (OptionEntry.IsDefined(OptionName.CreateBrChannels))
                OutputFileBladeRunner.Process(fileName);
            if (OptionEntry.IsDefined(OptionName.CreateArChannels))
                OutputFileAreaRegionChannels.Process(fileName);
            if (OptionEntry.IsDefined(OptionName.CreateSageTvFrq))
                OutputFileSageTVFrq.Process(fileName);

            return (null);
        }

        private static void setChannelID()
        {
            if (OptionEntry.IsDefined(OptionName.ChannelIdSeqNo))
            {
                int seqNo = 1;

                foreach (TVStation station in RunParameters.Instance.StationCollection)
                {
                    if (station.Included && station.EPGCollection.Count != 0)
                    {
                        station.ChannelID = seqNo.ToString();
                        seqNo++;
                    }
                }
            }
            else
            {
                if (OptionEntry.IsDefined(OptionName.ChannelIdSid))
                {
                    foreach (TVStation station in RunParameters.Instance.StationCollection)
                        station.ChannelID = station.ServiceID.ToString();
                }
                else
                {
                    if (OptionEntry.IsDefined(OptionName.UseChannelId))
                    {
                        foreach (TVStation station in RunParameters.Instance.StationCollection)
                            station.ChannelID = station.LogicalChannelNumber.ToString();
                    }
                    else
                    {
                        foreach (TVStation station in RunParameters.Instance.StationCollection)
                            station.ChannelID = station.OriginalNetworkID + ":" + station.TransportStreamID + ":" + station.ServiceID;
                    }
                }
            }
        }

        private static void processStationHeader(XmlWriter xmlWriter, TVStation tvStation)
        {
            xmlWriter.WriteAttributeString("Name", tvStation.Name.Trim());
            xmlWriter.WriteAttributeString("ID", tvStation.ChannelID);
            xmlWriter.WriteAttributeString("NID", tvStation.OriginalNetworkID.ToString());
            xmlWriter.WriteAttributeString("TID", tvStation.TransportStreamID.ToString());
            xmlWriter.WriteAttributeString("SID", tvStation.ServiceID.ToString());
            xmlWriter.WriteAttributeString("Count", tvStation.EPGCollection.Count.ToString());
            xmlWriter.WriteAttributeString("FirstStart", getStartTime(tvStation.EPGCollection[0]));
            xmlWriter.WriteAttributeString("LastStart", getStartTime(tvStation.EPGCollection[tvStation.EPGCollection.Count - 1]));
        }

        private static void processStationEPG(XmlWriter xmlWriter, TVStation tvStation)
        {
            xmlWriter.WriteStartElement("dvblink_epg");

            for (int index = 0; index < tvStation.EPGCollection.Count; index++)
            {
                EPGEntry epgEntry = tvStation.EPGCollection[index];

                if (TuningFrequency.HasUsedMHEG5Frequency(RunParameters.Instance.FrequencyCollection))
                    checkMidnightBreak(tvStation, epgEntry, index);

                processEPGEntry(xmlWriter, epgEntry);
            }

            xmlWriter.WriteEndElement();
        }

        private static void checkMidnightBreak(TVStation tvStation, EPGEntry currentEntry, int index)
        {
            if (index == tvStation.EPGCollection.Count - 1)
                return;

            EPGEntry nextEntry = tvStation.EPGCollection[index + 1];

            if (currentEntry.EventName != nextEntry.EventName)
                return;

            bool combined = false;
            if (RunParameters.Instance.FrequencyCollection[0].AdvancedRunParamters.CountryCode == null)
                combined = checkNZLTimes(currentEntry, nextEntry);
            else
            {
                switch (RunParameters.Instance.FrequencyCollection[0].AdvancedRunParamters.CountryCode)
                {
                    case Country.NewZealand:
                        combined = checkNZLTimes(currentEntry, nextEntry);
                        break;
                    case Country.Australia:
                        combined = checkAUSTimes(currentEntry, nextEntry);
                        break;
                    default:
                        break;
                }
            }

            if (combined)
                tvStation.EPGCollection.RemoveAt(index + 1);
        }

        private static bool checkNZLTimes(EPGEntry currentEntry, EPGEntry nextEntry)
        {
            if (!currentEntry.EndsAtMidnight)
                return (false);

            if (!nextEntry.StartsAtMidnight)
                return (false);

            if (currentEntry.StartTime + currentEntry.Duration != nextEntry.StartTime)
                return (false);

            if (nextEntry.Duration > new TimeSpan(3, 0, 0))
                return (false);

            Logger.Instance.Write("Combining " + currentEntry.ScheduleDescription + " with " + nextEntry.ScheduleDescription);
            currentEntry.Duration = currentEntry.Duration + nextEntry.Duration;

            return (true);
        }

        private static bool checkAUSTimes(EPGEntry currentEntry, EPGEntry nextEntry)
        {
            if (!nextEntry.StartsAtMidnight)
                return (false);

            if (currentEntry.StartTime + currentEntry.Duration != nextEntry.StartTime + nextEntry.Duration)
                return (false);

            Logger.Instance.Write("Combining " + currentEntry.ScheduleDescription + " with " + nextEntry.ScheduleDescription);

            return (true);
        }

        private static void processEPGEntry(XmlWriter xmlWriter, EPGEntry epgEntry)
        {
            Regex whitespace;

            if (!OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options, OptionName.FormatConvert))
                whitespace = new Regex(@"\s+");
            else
                whitespace = new Regex("[ ]{2,}");

            xmlWriter.WriteStartElement("program");

            if (epgEntry.EventName != null)
                xmlWriter.WriteAttributeString("name", whitespace.Replace(epgEntry.EventName.Trim(), " "));
            else
                xmlWriter.WriteAttributeString("name", "No Title");

            xmlWriter.WriteElementString("start_time", getStartTime(epgEntry));
            xmlWriter.WriteElementString("duration", epgEntry.Duration.TotalSeconds.ToString());

            if (epgEntry.EventSubTitle != null)
                xmlWriter.WriteElementString("subname", whitespace.Replace(epgEntry.EventSubTitle.Trim(), " "));

            if (epgEntry.ShortDescription != null)
                xmlWriter.WriteElementString("short_desc", whitespace.Replace(epgEntry.ShortDescription.Trim(), " "));
            else
            {
                if (epgEntry.EventName != null)
                    xmlWriter.WriteElementString("short_desc", whitespace.Replace(epgEntry.EventName.Trim(), " "));
                else
                    xmlWriter.WriteElementString("short_desc", "No Description");
            }

            if (epgEntry.EventCategory != null)
            {
                xmlWriter.WriteElementString("categories", "");

                string[] categoryParts = epgEntry.EventCategory.Split(new char[] { ',' });

                foreach (string categoryPart in categoryParts)
                {
                    if (!categoryPart.StartsWith("is"))
                        xmlWriter.WriteElementString("cat_" + categoryPart.Trim().Replace("/", " and ").Replace(' ', '_'), "");
                }
            }

            /* if (epgEntry.ParentalRating != null)
             {
                 xmlWriter.WriteStartElement("rating");
                 if (epgEntry.ParentalRatingSystem != null)
                     xmlWriter.WriteAttributeString("system", epgEntry.ParentalRatingSystem);
                 xmlWriter.WriteElementString("value", epgEntry.ParentalRating);
                 xmlWriter.WriteEndElement();
             }*/

            if (epgEntry.VideoQuality != null && epgEntry.VideoQuality.ToLowerInvariant() == "hdtv")
                xmlWriter.WriteElementString("hdtv", string.Empty);

            if (epgEntry.SeasonNumber > 0)
                xmlWriter.WriteElementString("season_num", epgEntry.SeasonNumber.ToString());
            if (epgEntry.EpisodeNumber > 0)
                xmlWriter.WriteElementString("episode_num", epgEntry.EpisodeNumber.ToString());

            writeCredit(xmlWriter, epgEntry.Cast, "actors");
            writeCredit(xmlWriter, epgEntry.Directors, "directors");
            writeCredit(xmlWriter, epgEntry.Writers, "writers");
            writeCredit(xmlWriter, epgEntry.Producers, "producers");

            if (epgEntry.StarRating != null)
            {
                switch (epgEntry.StarRating)
                {
                    case "+":
                        xmlWriter.WriteElementString("stars_num", "1");
                        break;
                    case "*":
                        xmlWriter.WriteElementString("stars_num", "2");
                        break;
                    case "*+":
                        xmlWriter.WriteElementString("stars_num", "3");
                        break;
                    case "**":
                        xmlWriter.WriteElementString("stars_num", "4");
                        break;
                    case "**+":
                        xmlWriter.WriteElementString("stars_num", "5");
                        break;
                    case "***":
                        xmlWriter.WriteElementString("stars_num", "6");
                        break;
                    case "***+":
                        xmlWriter.WriteElementString("stars_num", "7");
                        break;
                    case "****":
                        xmlWriter.WriteElementString("stars_num", "8");                        
                        break;
                    default:
                        xmlWriter.WriteElementString("stars_num", "4");
                        break;
                }

                xmlWriter.WriteElementString("starsmax_num", "8"); 
            }

            if (epgEntry.Date != null)
                xmlWriter.WriteElementString("year", epgEntry.Date);

            if (epgEntry.PreviousPlayDate != DateTime.MinValue)
                xmlWriter.WriteElementString("repeat", string.Empty);

            if (epgEntry.Poster != null)
            {
                string imagePath = null;

                if (epgEntry.Poster.Value != new Guid())
                {
                    imagePath = Path.Combine(RunParameters.ImagePath, "Movies", epgEntry.Poster + ".jpg");
                    if (!File.Exists(imagePath))
                    {
                        imagePath = Path.Combine(RunParameters.ImagePath, "TV Series", epgEntry.Poster + ".jpg");
                        if (!File.Exists(imagePath))
                        {
                            imagePath = Path.Combine(RunParameters.ImagePath, epgEntry.Poster + ".jpg");
                            if (!File.Exists(imagePath))
                            {
                                string legalFileName = RunParameters.GetLegalFileName(epgEntry.EventName, ' ');

                                imagePath = Path.Combine(RunParameters.ImagePath, "Movies", legalFileName + ".jpg");
                                if (!File.Exists(imagePath))
                                {
                                    imagePath = Path.Combine(RunParameters.ImagePath, "TV Series", legalFileName + ".jpg");
                                    if (!File.Exists(imagePath))
                                    {
                                        imagePath = Path.Combine(RunParameters.ImagePath, legalFileName + ".jpg");
                                        if (!File.Exists(imagePath))
                                            imagePath = null;
                                    }
                                }
                            }
                        }
                    }
                }

                if (imagePath != null)
                    xmlWriter.WriteElementString("image", "file://" + imagePath);
            }

            xmlWriter.WriteEndElement();
        }

        private static void writeCredit(XmlWriter xmlWriter, Collection<string> credits, string name)
        {
            if (credits == null || credits.Count == 0)
                return;

            StringBuilder creditString = new StringBuilder();

            foreach (string credit in credits)
            {
                if (credit.Trim() != string.Empty)
                {
                    if (creditString.Length != 0)
                        creditString.Append("/");

                    creditString.Append(credit.Trim());
                }
            }

            if (creditString.Length != 0)
                xmlWriter.WriteElementString(name, creditString.ToString());
        }

        private static string getStartTime(EPGEntry epgEntry)
        {
            DateTime gmtStartTime;

            if (!TimeZoneInfo.Local.IsInvalidTime(epgEntry.StartTime))
                gmtStartTime = TimeZoneInfo.ConvertTimeToUtc(epgEntry.StartTime);
            else
                gmtStartTime = TimeZoneInfo.ConvertTimeToUtc(epgEntry.StartTime.AddHours(1));

            TimeSpan timeSpan = gmtStartTime - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            UInt32 seconds = Convert.ToUInt32(Math.Abs(timeSpan.TotalSeconds));

            return (seconds.ToString());
        }
    }
}
