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
using System.Reflection;
using System.Collections.ObjectModel;

namespace DomainObjects
{
    /// <summary>
    /// The class that describes the xmltv file.
    /// </summary>
    public sealed class OutputFileXML
    {
        private OutputFileXML() { }

        internal static string Process(string fileName)
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

            Logger.Instance.Write("Creating output file: " + fileName);

            XmlWriterSettings settings = new XmlWriterSettings();

            settings.Indent = true;
            settings.NewLineOnAttributes = false;
            if (!OutputFile.UseUnicodeEncoding)
                settings.Encoding = new UTF8Encoding();
            else
                settings.Encoding = new UnicodeEncoding();
            settings.CloseOutput = true;
            
            if (DebugEntry.IsDefined(DebugName.IgnoreXmlChars))
                settings.CheckCharacters = false;

            try
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(fileName, settings))
                {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("tv");

                    xmlWriter.WriteAttributeString("generator-info-name", Assembly.GetCallingAssembly().GetName().Name
                        + "/" + Assembly.GetCallingAssembly().GetName().Version.ToString());

                    setChannelID();

                    Collection<TVStation> sortedStations = sortStations(RunParameters.Instance.StationCollection);

                    foreach (TVStation tvStation in sortedStations)
                        processStationHeader(xmlWriter, tvStation);                    

                    foreach (TVStation tvStation in sortedStations)
                        processStationEPG(xmlWriter, tvStation);

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
                if (OptionEntry.IsDefined(OptionName.ChannelIdFullName))
                {
                    foreach (TVStation station in RunParameters.Instance.StationCollection)
                    {
                        if (station.Name != null)
                            station.ChannelID = station.OriginalNetworkID + ":" +
                                station.TransportStreamID + ":" +
                                station.ServiceID + ":" +
                                station.Name;
                        else
                            station.ChannelID = station.OriginalNetworkID + ":" +
                                station.TransportStreamID + ":" +
                                station.ServiceID;
                    }
                }
            }
        }

        private static Collection<TVStation> sortStations(Collection<TVStation> stations)
        {
            Collection<TVStation> sortedStations = new Collection<TVStation>();

            foreach (TVStation station in stations)
            {
                if (station.Included && station.EPGCollection.Count != 0)
                {
                    if (OptionEntry.IsDefined(OptionName.ChannelIdSeqNo))
                        sortedStations.Add(station);
                    else
                    {
                        bool inserted = false;

                        foreach (TVStation sortedStation in sortedStations)
                        {
                            int sortResult = compareStations(sortedStation, station);

                            if (sortResult > 0)
                            {
                                sortedStations.Insert(sortedStations.IndexOf(sortedStation), station);
                                inserted = true;
                                break;
                            }
                        }

                        if (!inserted)
                            sortedStations.Add(station);
                    }
                }
            }

            return (sortedStations);
        }

        private static int compareStations(TVStation station1, TVStation station2)
        {
            if (station1.ChannelID == null)
            {
                if (!OptionEntry.IsDefined(OptionName.UseChannelId))
                    return(station1.ServiceID.CompareTo(station2.ServiceID));
                else
                {
                    if (station1.LogicalChannelNumber != -1)
                        return(station1.LogicalChannelNumber.CompareTo(station2.LogicalChannelNumber));
                    else
                        return (station1.ServiceID.CompareTo(station2.ServiceID));
                }
            }
            else
                return (station1.ChannelID.CompareTo(station2.ChannelID));
        }

        private static void processStationHeader(XmlWriter xmlWriter, TVStation tvStation)
        {
            xmlWriter.WriteStartElement("channel");

            if (tvStation.ChannelID == null)
            {
                if (!OptionEntry.IsDefined(OptionName.UseChannelId))
                    xmlWriter.WriteAttributeString("id", tvStation.ServiceID.ToString());
                else
                {
                    if (tvStation.LogicalChannelNumber != -1)
                        xmlWriter.WriteAttributeString("id", tvStation.LogicalChannelNumber.ToString());
                    else
                        xmlWriter.WriteAttributeString("id", tvStation.ServiceID.ToString());
                }
            }
            else
                xmlWriter.WriteAttributeString("id", tvStation.ChannelID);

            if (tvStation.NewName == null)
                xmlWriter.WriteElementString("display-name", tvStation.Name);
            else
                xmlWriter.WriteElementString("display-name", tvStation.NewName);

            if (OptionEntry.IsDefined(OptionName.UseLcn))
            {
                if (tvStation.ChannelID != null)
                    xmlWriter.WriteElementString("lcn", tvStation.ChannelID);
                else
                {
                    if (tvStation.LogicalChannelNumber != -1)
                        xmlWriter.WriteElementString("lcn", tvStation.LogicalChannelNumber.ToString());
                }
            }

            if (File.Exists(RunParameters.DataDirectory + "\\Images\\" + tvStation.ServiceID + ".png"))
            {
                xmlWriter.WriteStartElement("icon");
                xmlWriter.WriteAttributeString("src", "file://" + RunParameters.DataDirectory + "\\Images\\" + tvStation.ServiceID + ".png");
                xmlWriter.WriteEndElement();
            }

            xmlWriter.WriteEndElement();
        }

        private static void processStationEPG(XmlWriter xmlWriter, TVStation tvStation)
        {
            Regex whitespace = new Regex(@"\s+");

            string channelNumber;
            if (tvStation.ChannelID == null)
            {
                if (!OptionEntry.IsDefined(OptionName.UseChannelId))
                    channelNumber = tvStation.ServiceID.ToString();
                else
                {
                    if (tvStation.LogicalChannelNumber != -1)
                        channelNumber = tvStation.LogicalChannelNumber.ToString();
                    else
                        channelNumber = tvStation.ServiceID.ToString();
                }
            }
            else
                channelNumber = tvStation.ChannelID;

            for (int index = 0; index < tvStation.EPGCollection.Count; index++)
            {
                EPGEntry epgEntry = tvStation.EPGCollection[index];
                processEPGEntry(xmlWriter, channelNumber, epgEntry);
            }
        }

        private static void processEPGEntry(XmlWriter xmlWriter, string channelNumber, EPGEntry epgEntry)
        {
            Regex whitespace;

            if (!checkConvertPresent(RunParameters.Instance.FrequencyCollection))
                whitespace = new Regex(@"\s+");
            else
                whitespace = new Regex("[ ]{2,}");

            xmlWriter.WriteStartElement("programme");

            xmlWriter.WriteAttributeString("start", epgEntry.StartTime.ToString("yyyyMMddHHmmss zzz").Replace(":", ""));
            xmlWriter.WriteAttributeString("stop", (epgEntry.StartTime + epgEntry.Duration).ToString("yyyyMMddHHmmss zzz").Replace(":", ""));

            xmlWriter.WriteAttributeString("channel", channelNumber);

            if (epgEntry.EventName != null)
                xmlWriter.WriteElementString("title", whitespace.Replace(epgEntry.EventName, " "));                
            else
                xmlWriter.WriteElementString("title", "No Title");

            try
            {
                if (!OptionEntry.IsDefined(OptionName.UseDvbViewer))
                {
                    if (epgEntry.EventSubTitle != null)
                        xmlWriter.WriteElementString("sub-title", whitespace.Replace(epgEntry.EventSubTitle, " "));

                    if (epgEntry.ShortDescription != null)
                        xmlWriter.WriteElementString("desc", whitespace.Replace(epgEntry.ShortDescription, " "));
                    else
                    {
                        if (epgEntry.EventName != null)
                            xmlWriter.WriteElementString("desc", whitespace.Replace(epgEntry.EventName, " "));
                        else
                            xmlWriter.WriteElementString("desc", "No Description");
                    }
                }
                else
                {
                    if (epgEntry.ShortDescription != null)
                        xmlWriter.WriteElementString("sub-title", whitespace.Replace(epgEntry.ShortDescription, " "));
                    else
                    {
                        if (epgEntry.EventName != null)
                            xmlWriter.WriteElementString("sub-title", whitespace.Replace(epgEntry.EventName.Trim(), " "));
                        else
                            xmlWriter.WriteElementString("sub-title", "No Description");
                    }
                }
            }
            catch (ArgumentException)
            {
                Logger.Instance.Write("Invalid XML character");
                Logger.Instance.Write("Title: " + epgEntry.EventName);
                Logger.Instance.Write("Description: " + epgEntry.ShortDescription);
                throw;
            }

            if (epgEntry.EventCategory != null)
            {
                string[] categories = epgEntry.EventCategory.Split(new char[] { ',' });

                if (categories.Length > 1 && (categories[0] == "Movie" || categories[0] == "Series"))
                {
                    for (int index = 1; index < categories.Length; index++)
                    {
                        if (!categories[index].StartsWith("is"))
                        {
                            if (!OptionEntry.IsDefined(OptionName.ElementPerTag))
                                xmlWriter.WriteElementString("category", categories[0] + " - " + categories[index]);
                            else
                            {
                                if (index == 1)
                                    xmlWriter.WriteElementString("category", categories[0]);
                                xmlWriter.WriteElementString("category", categories[index]);
                            }
                        }
                    }
                }
                else
                {
                    foreach (string category in categories)
                    {
                        if (!category.ToLowerInvariant().StartsWith("is"))
                            xmlWriter.WriteElementString("category", category);
                    }
                }
            }

            if (epgEntry.ParentalRating != null)
            {
                xmlWriter.WriteStartElement("rating");
                if (epgEntry.ParentalRatingSystem != null)
                    xmlWriter.WriteAttributeString("system", epgEntry.ParentalRatingSystem);
                xmlWriter.WriteElementString("value", epgEntry.ParentalRating);
                xmlWriter.WriteEndElement();
            }

            if (epgEntry.StarRating != null)
            {
                xmlWriter.WriteStartElement("star-rating");
                
                switch (epgEntry.StarRating)
                {
                    case "+":
                        xmlWriter.WriteElementString("value", "1/8");
                        break;
                    case "*":
                        xmlWriter.WriteElementString("value", "2/8");
                        break;
                    case "*+":
                        xmlWriter.WriteElementString("value", "3/8");
                        break;
                    case "**":
                        xmlWriter.WriteElementString("value", "4/8");
                        break;
                    case "**+":
                        xmlWriter.WriteElementString("value", "5/8");
                        break;
                    case "***":
                        xmlWriter.WriteElementString("value", "6/8");
                        break;
                    case "***+":
                        xmlWriter.WriteElementString("value", "7/8");
                        break;
                    case "****":
                        xmlWriter.WriteElementString("value", "8/8");
                        break;
                    default:
                        xmlWriter.WriteElementString("value", "4/8");
                        break;
                }

                xmlWriter.WriteEndElement();
            }

            if (epgEntry.AspectRatio != null || epgEntry.VideoQuality != null)
            {
                xmlWriter.WriteStartElement("video");
                if (epgEntry.AspectRatio != null)
                    xmlWriter.WriteElementString("aspect", epgEntry.AspectRatio);
                if (epgEntry.VideoQuality != null)
                    xmlWriter.WriteElementString("quality", epgEntry.VideoQuality);
                xmlWriter.WriteEndElement();
            }

            if (epgEntry.AudioQuality != null)
            {
                xmlWriter.WriteStartElement("audio");
                xmlWriter.WriteElementString("stereo", epgEntry.AudioQuality);
                xmlWriter.WriteEndElement();
            }

            if (OptionEntry.IsDefined(OptionName.CreateAdTag))
            {
                xmlWriter.WriteStartElement("audio-description");
                xmlWriter.WriteAttributeString("present", epgEntry.HasAudioDescription ? "yes" : "no");
                xmlWriter.WriteEndElement();
            }

            if (epgEntry.SubTitles != null)
            {
                xmlWriter.WriteStartElement("subtitles");
                xmlWriter.WriteAttributeString("type", epgEntry.SubTitles);
                xmlWriter.WriteEndElement();
            }

            createEpisodeTag(xmlWriter, epgEntry);            

            if (epgEntry.PreviousPlayDate != DateTime.MinValue)
            {
                xmlWriter.WriteStartElement("previously-shown");
                xmlWriter.WriteAttributeString("start", epgEntry.PreviousPlayDate.ToString("yyyyMMddHHmmss zzz").Replace(":", ""));
                xmlWriter.WriteEndElement();
            }

            bool creditsWritten = writeCredits(xmlWriter, epgEntry.Directors, "director", false);
            creditsWritten = writeCredits(xmlWriter, epgEntry.Producers, "producer", creditsWritten);
            creditsWritten = writeCredits(xmlWriter, epgEntry.Writers, "writer", creditsWritten);
            creditsWritten = writeCredits(xmlWriter, epgEntry.Cast, "actor", creditsWritten);
            creditsWritten = writeCredits(xmlWriter, epgEntry.GuestStars, "guest", creditsWritten);
            creditsWritten = writeCredits(xmlWriter, epgEntry.Presenters, "presenter", creditsWritten);
            if (creditsWritten)
                xmlWriter.WriteEndElement();

            if (epgEntry.Date != null)
                xmlWriter.WriteElementString("date", epgEntry.Date);

            if (epgEntry.Country != null)
                xmlWriter.WriteElementString("country", epgEntry.Country);

            string group = null;
            string imagePath = null;

            if (epgEntry.Poster != null)
            {
                if (epgEntry.Poster.Value != new Guid())
                {
                    group = "Movies/";
                    imagePath = Path.Combine(RunParameters.ImagePath, "Movies", epgEntry.Poster + ".jpg");
                    if (!File.Exists(imagePath))
                    {
                        group = "Series/";
                        imagePath = Path.Combine(RunParameters.ImagePath, "TV Series", epgEntry.Poster + ".jpg");
                        if (!File.Exists(imagePath))
                        {
                            group = string.Empty;
                            imagePath = Path.Combine(RunParameters.ImagePath, epgEntry.Poster + ".jpg");
                            if (!File.Exists(imagePath))
                            {
                                string legalFileName = RunParameters.GetLegalFileName(epgEntry.EventName, ' ');

                                group = "Movies/";
                                imagePath = Path.Combine(RunParameters.ImagePath, "Movies", legalFileName + ".jpg");
                                if (!File.Exists(imagePath))
                                {
                                    group = "Series/";
                                    imagePath = Path.Combine(RunParameters.ImagePath, "TV Series", legalFileName + ".jpg");
                                    if (!File.Exists(imagePath))
                                    {
                                        group = string.Empty;
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
                {
                    string tagPath;

                    if (string.IsNullOrWhiteSpace(RunParameters.Instance.LookupXmltvImageTagPath))
                        tagPath = "file://" + imagePath;
                    else
                        tagPath = RunParameters.Instance.LookupXmltvImageTagPath +
                            (RunParameters.Instance.LookupXmltvImageTagPath.EndsWith("/") ? "" : "/") +
                        group + epgEntry.Poster + ".jpg";

                    xmlWriter.WriteStartElement("icon");
                    xmlWriter.WriteAttributeString("src", tagPath);
                    xmlWriter.WriteEndElement();
                }
            }
            else
            {
                if (epgEntry.PosterPath != null)
                {
                    xmlWriter.WriteStartElement("icon");
                    xmlWriter.WriteAttributeString("src", epgEntry.PosterPath);
                    if (epgEntry.PosterWidth != null)
                        xmlWriter.WriteAttributeString("width", epgEntry.PosterWidth);
                    if (epgEntry.PosterHeight != null)
                        xmlWriter.WriteAttributeString("height", epgEntry.PosterHeight);
                    xmlWriter.WriteEndElement();
                }
            }

            xmlWriter.WriteEndElement();
        }

        private static bool checkConvertPresent(Collection<TuningFrequency> frequencies)
        {
            bool present = false;

            foreach (TuningFrequency tuningFrequency in frequencies)
            {
                if (OptionEntry.IsDefined(tuningFrequency.AdvancedRunParamters.Options, OptionName.FormatConvert))
                    present = true;
            }

            return (present);
        }

        private static void createEpisodeTag(XmlWriter xmlWriter, EPGEntry epgEntry)
        {
            if (OptionEntry.IsDefined(OptionName.NoEpisodeTag))
                return;

            if (epgEntry.EpisodeTag != null)
            {
                xmlWriter.WriteStartElement("episode-num");
                xmlWriter.WriteAttributeString("system", epgEntry.EpisodeSystemType);
                xmlWriter.WriteString(epgEntry.EpisodeTag);
                xmlWriter.WriteEndElement();
                return;
            }

            if (OptionEntry.IsDefined(OptionName.ValidEpisodeTag))
            {
                createValidEpisodeTag(xmlWriter, epgEntry);
                return;
            }

            if (OptionEntry.IsDefined(OptionName.UseBsepg))
            {
                createBsepgEpisodeTag(xmlWriter, epgEntry);
                return;
            }

            if (OptionEntry.IsDefined(OptionName.UseNumericCrid))
            {
                createNumericCridEpisodeTag(xmlWriter, epgEntry);
                return;
            }

            if (OptionEntry.IsDefined(OptionName.UseRawCrid))
            {
                createRawCridEpisodeTag(xmlWriter, epgEntry);
                return;
            }

            if (OptionEntry.IsDefined(OptionName.VBoxEpisodeTag))
            {
                createVBoxEpisodeTag(xmlWriter, epgEntry);
                return;
            }
                
            createValidEpisodeTag(xmlWriter, epgEntry);
        }

        private static void createValidEpisodeTag(XmlWriter xmlWriter, EPGEntry epgEntry)
        {
            if (epgEntry.SeasonNumber == -1 && epgEntry.EpisodeNumber == -1)
                return;

            int xmltvSeasonNumber = epgEntry.SeasonNumber > 0 ? epgEntry.SeasonNumber - 1 : -1;
            int xmltvEpisodeNumber = epgEntry.EpisodeNumber > 0 ? epgEntry.EpisodeNumber - 1 : -1;

            string seasonString = xmltvSeasonNumber != -1 ? xmltvSeasonNumber.ToString() : string.Empty;
            string episodeString = xmltvEpisodeNumber != -1 ? xmltvEpisodeNumber.ToString() : string.Empty;

            xmlWriter.WriteStartElement("episode-num");
            xmlWriter.WriteAttributeString("system", "xmltv_ns");
            xmlWriter.WriteString(seasonString + " . " + episodeString + " . 0/1");
            xmlWriter.WriteEndElement();            
        }

        private static void createBsepgEpisodeTag(XmlWriter xmlWriter, EPGEntry epgEntry)
        {
            string series = epgEntry.SeriesId != null ? getNumber(epgEntry.SeriesId) : string.Empty;
            string episode = epgEntry.EpisodeId != null ? getNumber(epgEntry.EpisodeId) : string.Empty;

            if (series == string.Empty && episode == string.Empty)
                return;
                        
            xmlWriter.WriteStartElement("episode-num");
            xmlWriter.WriteAttributeString("system", "bsepg-epid");
            xmlWriter.WriteString("SE-" + series + " . " + "EP-" + episode);
            xmlWriter.WriteEndElement();
        }

        private static void createNumericCridEpisodeTag(XmlWriter xmlWriter, EPGEntry epgEntry)
        {
            string series = epgEntry.SeasonCrid != null ? getNumber(epgEntry.SeasonCrid) : string.Empty;            
            string episode = epgEntry.EpisodeCrid != null ? getNumber(epgEntry.EpisodeCrid) : string.Empty;

            if (series == string.Empty && episode == string.Empty)
                return;
            
            xmlWriter.WriteStartElement("episode-num");
            xmlWriter.WriteAttributeString("system", "crid_numeric");
            xmlWriter.WriteString(series + " . " + episode + " . ");
            xmlWriter.WriteEndElement();
        }

        private static string getNumber(string text)
        {
            if (text.Trim().Length == 0)
                return (string.Empty);

            StringBuilder numericString = new StringBuilder();

            foreach (char cridChar in text)
            {
                if (char.IsDigit(cridChar))
                    numericString.Append(cridChar);
            }

            if (numericString.Length != 0)
                return (numericString.ToString());
            else
                return (string.Empty);
        }

        private static void createRawCridEpisodeTag(XmlWriter xmlWriter, EPGEntry epgEntry)
        {
            string series = epgEntry.SeasonCrid != null ? epgEntry.SeasonCrid.Replace("/", "") : string.Empty;
            string episode = epgEntry.EpisodeCrid != null ? epgEntry.EpisodeCrid.Replace("/", "") : string.Empty;

            if (series == string.Empty && episode == string.Empty)
                return;
            
            xmlWriter.WriteStartElement("episode-num");
            xmlWriter.WriteAttributeString("system", "crid");
            xmlWriter.WriteString(series + " . " + episode + " . ");
            xmlWriter.WriteEndElement();
        }

        private static void createVBoxEpisodeTag(XmlWriter xmlWriter, EPGEntry epgEntry)
        {
            if (string.IsNullOrWhiteSpace(epgEntry.SeasonCrid) && string.IsNullOrWhiteSpace(epgEntry.EventName))
                return;

            string seedString = string.IsNullOrWhiteSpace(epgEntry.SeasonCrid) ? epgEntry.EventName : epgEntry.SeasonCrid;

            while (seedString.Length < 32)
                seedString = seedString + seedString;
            
            int xmltvSeasonNumber = epgEntry.SeasonNumber > 0 ? epgEntry.SeasonNumber - 1 : -1;
            int xmltvEpisodeNumber = epgEntry.EpisodeNumber > 0 ? epgEntry.EpisodeNumber - 1 : -1;

            string seasonString = xmltvSeasonNumber != -1 ? xmltvSeasonNumber.ToString() : string.Empty;
            string episodeString = xmltvEpisodeNumber != -1 ? xmltvEpisodeNumber.ToString() : string.Empty;

            xmlWriter.WriteStartElement("episode-num");
            xmlWriter.WriteAttributeString("system", "xmltv_ns");
            xmlWriter.WriteString((uint)seedString.GetHashCode() + " . " + seasonString + " . " + episodeString);
            xmlWriter.WriteEndElement();
        }

        private static bool writeCredits(XmlWriter xmlWriter, Collection<string> credits, string name, bool creditsWritten)
        {
            if (credits == null || credits.Count == 0)
                return (creditsWritten);

            bool written = creditsWritten;

            foreach (string credit in credits)
            {
                if (credit.Trim() != string.Empty)
                {
                    if (!written)
                    {
                        xmlWriter.WriteStartElement("credits");
                        written = true;
                    }

                    xmlWriter.WriteElementString(name, credit.Trim());
                }
            }

            return (written);
        }

        private static string removeInvalidXmlChars(string inputString)
        {
            Collection<char> invalidChars = new Collection<char>();

            foreach (char stringChar in inputString)
            {
                if (!XmlConvert.IsXmlChar(stringChar))
                    invalidChars.Add(stringChar);
            }

            string editedString = inputString;

            foreach (char invalidChar in invalidChars)
                editedString = editedString.Replace(invalidChar.ToString(), "");

            return editedString;
        }
    }
}
