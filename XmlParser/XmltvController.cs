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
using System.Xml;
using System.IO;
using System.Text;
using System.Reflection;

using DomainObjects;

namespace XmltvParser
{
    /// <summary>
    /// The class that describes the Xmltv parser controller.
    /// </summary>
    public sealed class XmltvController : ImportFileBase
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
        /// Process an XMLTV file.
        /// </summary>
        /// <param name="fileName">The actual file path.</param>
        /// <param name="fileSpec">The file definition.</param>
        /// <returns>An error message or null if the file is processed successfully.</returns>
        public override string Process(string fileName, ImportFileSpec fileSpec)
        {
            XmltvProgramCategory.Load();
            CustomProgramCategory.Load();

            XmltvChannel.Channels = new Collection<XmltvChannel>();
            XmltvProgramme.Programmes = new Collection<XmltvProgramme>();

            XmlReader xmlReader = null;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            settings.DtdProcessing = DtdProcessing.Ignore;
            settings.CheckCharacters = false;
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;

            try
            {
                xmlReader = XmlReader.Create(fileName, settings);
            }
            catch (IOException)
            {
                return("Failed to open " + fileName);
            }

            try
            {
                while (!xmlReader.EOF)
                {
                    xmlReader.Read();
                    if (xmlReader.IsStartElement())
                    {
                        switch (xmlReader.Name)
                        {
                            case "channel":
                                XmltvChannel channel = XmltvChannel.GetInstance(xmlReader.ReadSubtree());
                                channel.IdFormat = fileSpec.IdFormat;
                                XmltvChannel.Channels.Add(channel);                                
                                break;
                            case "programme":
                                XmltvProgramme programme = XmltvProgramme.GetInstance(xmlReader.ReadSubtree());
                                XmltvProgramme.Programmes.Add(programme);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (XmlException e)
            {
                return ("Failed to load xmltv file: " + e.Message);                
            }
            catch (IOException e)
            {
                return ("Failed to load xmltv file: " + e.Message);                
            }

            if (xmlReader != null)
                xmlReader.Close();

            Collection<TVStation> xmltvChannels = createEPGData(fileSpec.Language, fileSpec.Precedence, fileSpec.NoLookup);
            if (xmltvChannels != null)
                MergeChannels(xmltvChannels, fileSpec.Precedence, fileSpec.AppendOnly);

            return (null);
        }

        private Collection<TVStation> createEPGData(LanguageCode languageCode, DataPrecedence precedence, bool noLookup)
        {
            Collection<TVStation> stations = processChannels(languageCode);
            if (stations != null)
                processProgrammes(stations, languageCode, noLookup);

            return (stations);
        }

        private Collection<TVStation> processChannels(LanguageCode languageCode)
        {
            if (XmltvChannel.Channels == null || XmltvChannel.Channels.Count == 0)
                return (null);

            Collection<TVStation> stations = new Collection<TVStation>();

            foreach (XmltvChannel channel in XmltvChannel.Channels)
            {
                TVStation station;

                if (channel.DisplayNames == null || channel.DisplayNames.Count == 0)
                    station = new TVStation("No Name");
                else
                    station = new TVStation(findLanguageString(channel.DisplayNames, languageCode).Trim());

                if (channel.IdFormat == XmltvIdFormat.FullChannelId)
                    setStationId(station, channel);
                else
                {
                    if (channel.IdFormat == XmltvIdFormat.ServiceId)
                        setStationServiceId(station, channel);
                    else
                    {
                        station.ServiceID = stations.Count + 1;
                        station.UseNameForMerge = channel.IdFormat == XmltvIdFormat.Name;
                    }
                }

                station.ProviderName = channel.Id;
                
                if (channel.IdFormat == XmltvIdFormat.UserChannelNumber)
                    setStationLogicalChannelNumber(station, channel);

                if (RunParameters.Instance.ImportChannelChanges != null)
                {
                    foreach (ImportChannelChange channelChange in RunParameters.Instance.ImportChannelChanges)
                    {
                        if (channelChange.DisplayName == station.Name)
                        {
                            if (!String.IsNullOrWhiteSpace(channelChange.NewName)) 
                                station.NewName = channelChange.NewName;
                            if (channelChange.ChannelNumber != -1)
                                station.LogicalChannelNumber = channelChange.ChannelNumber;
                            station.ExcludedByUser = channelChange.Excluded;
                        }
                    }
                }

                stations.Add(station);

                Logger.Instance.Write("Created channel '" + station.Name + "' identity '" + channel.Id + "'");
            }

            Logger.Instance.Write("Created " + stations.Count + " channels from the xmltv data");

            return (stations);
        }

        private void setStationId(TVStation station, XmltvChannel channel)
        {
            string[] parts = channel.Id.Split(new char[] { ':' });
            if (parts.Length != 4)
                return;

            try
            {
                int originalNetworkId = Int32.Parse(parts[0].Trim());
                int transportStreamId = Int32.Parse(parts[1].Trim());
                int serviceId = Int32.Parse(parts[2].Trim());

                station.OriginalNetworkID = originalNetworkId;
                station.TransportStreamID = transportStreamId;
                station.ServiceID = serviceId;
            }
            catch (FormatException) 
            {
                Logger.Instance.Write("Full channel ID format wrong for '" + channel.Id + "'");
            }
            catch (OverflowException) 
            {
                Logger.Instance.Write("Full channel ID too large for '" + channel.Id + "'");
            }
        }

        private void setStationServiceId(TVStation station, XmltvChannel channel)
        {
            try
            {
                station.ServiceID = Int32.Parse(channel.Id.Trim());
            }
            catch (FormatException)
            {
                Logger.Instance.Write("Channel ID format wrong for '" + channel.Id + "'");
            }
            catch (OverflowException)
            {
                Logger.Instance.Write("Channel ID too large for '" + channel.Id + "'");
            }
        }

        private void setStationLogicalChannelNumber(TVStation station, XmltvChannel channel)
        {
            try
            {
                station.LogicalChannelNumber = Int32.Parse(channel.Id.Trim());
            }
            catch (FormatException)
            {
                Logger.Instance.Write("Channel ID format wrong for '" + channel.Id + "'");
            }
            catch (OverflowException)
            {
                Logger.Instance.Write("Channel ID too large for '" + channel.Id + "'");
            }
        }

        private void processProgrammes(Collection<TVStation> stations, LanguageCode languageCode, bool noLookup)
        {
            if (XmltvProgramme.Programmes == null || XmltvProgramme.Programmes.Count == 0)
                return;

            int created = 0;
            int noStation = 0;

            foreach (XmltvProgramme programme in XmltvProgramme.Programmes)
            {
                TVStation station = findStation(stations, programme.Channel);
                if (station != null)
                {
                    processProgramme(station, programme, languageCode, noLookup);
                    created++;
                }
                else
                    noStation++;
            }

            Logger.Instance.Write("EPG entries created = " + created + " Xmltv programmes ignored = " + noStation);
        }

        private TVStation findStation(Collection<TVStation> stations, string channelID)
        {
            foreach (TVStation station in stations)
            {
                if (station.ProviderName == channelID)
                    return (station);
            }

            return (null);
        }

        private void processProgramme(TVStation station, XmltvProgramme programme, LanguageCode languageCode, bool noLookup)
        {
            if (programme.StartTime == null || programme.StopTime == null)
                return;

            if (station.EPGCollection == null)
                station.EPGCollection = new Collection<EPGEntry>();

            EPGEntry epgEntry = new EPGEntry();

            epgEntry.OriginalNetworkID = station.OriginalNetworkID;
            epgEntry.TransportStreamID = station.TransportStreamID;
            epgEntry.ServiceID = station.ServiceID;

            epgEntry.EventName = findLanguageString(programme.Titles, languageCode);

            if (programme.Descriptions != null && programme.Descriptions.Count != 0)
                epgEntry.ShortDescription = findLanguageString(programme.Descriptions, languageCode);
            else
                epgEntry.ShortDescription = "No Synopsis Available";

            if (programme.SubTitles != null && programme.SubTitles.Count != 0)
                epgEntry.EventSubTitle = findLanguageString(programme.SubTitles, languageCode);

            epgEntry.StartTime = (DateTime)(programme.StartTime.Time.Value - programme.StartTime.Offset);
            epgEntry.Duration = (DateTime)(programme.StopTime.Time.Value - programme.StartTime.Offset) - epgEntry.StartTime;

            epgEntry.Date = programme.Date;

            if (programme.Categories != null && programme.Categories.Count != 0)
            {
                StringBuilder categoryString = new StringBuilder();

                foreach (XmltvText category in programme.Categories)
                {
                    string languageText = checkCategoryLanguage(category, languageCode);
                    if (languageText != null)
                    {
                        if (categoryString.Length != 0)
                            categoryString.Append(',');

                        string decodedCategory = getEventCategory(languageText, epgEntry);
                        if (decodedCategory != null)
                            categoryString.Append(decodedCategory);
                        else
                            categoryString.Append(languageText);
                    }
                }

                epgEntry.EventCategory = categoryString.ToString();
            }

            if (programme.Rating != null)
            {
                epgEntry.ParentalRating = programme.Rating.Value;
                epgEntry.ParentalRatingSystem = programme.Rating.System;
            }

            if (programme.Video != null)
            {
                epgEntry.VideoQuality = programme.Video.Quality;
                epgEntry.AspectRatio = programme.Video.Aspect;
            }

            if (programme.Audio != null)
                epgEntry.AudioQuality = programme.Audio.Stereo;

            if (programme.StarRating != null)
                epgEntry.StarRating = getStarRating(programme.StarRating.Value);

            if (programme.Subtitlings != null && programme.Subtitlings.Count != 0)
                epgEntry.SubTitles = findSubtitlingsLanguage(programme.Subtitlings, languageCode);

            if (programme.PreviouslyShown != null && programme.PreviouslyShown.Start != null && programme.PreviouslyShown.Start.Time != null)
                epgEntry.PreviousPlayDate = programme.PreviouslyShown.Start.Time.Value;

            if (programme.EpisodeNumbers != null)
            {
                XmltvEpisodeNumber episodeNumber = programme.FindEpisodeType("xmltv_ns");
                if (episodeNumber != null)
                {
                    epgEntry.EpisodeSystemType = episodeNumber.System;

                    string[] parts = episodeNumber.Episode.Split(new char[] { '.' });
                    epgEntry.SeasonNumber = getEpisodeData(parts[0]);
                    if (parts.Length > 1)
                    {
                        epgEntry.EpisodeNumber = getEpisodeData(parts[1]);
                        if (parts.Length > 2)
                            epgEntry.PartNumber = parts[2].Trim();                            
                    }
                }
                else
                {
                    episodeNumber = programme.FindEpisodeType("bsepg-epid");
                    if (episodeNumber != null)
                    {
                        epgEntry.EpisodeSystemType = episodeNumber.System;

                        string[] parts = episodeNumber.Episode.Split(new char[] { '.' });
                        epgEntry.SeriesId = parts[0].Trim();
                        epgEntry.EpisodeId = parts[1].Trim();
                    }
                    else
                    {
                        episodeNumber = programme.FindEpisodeType("crid");
                        if (episodeNumber == null)
                            episodeNumber = programme.FindEpisodeType("crid_numeric");

                        if (episodeNumber != null)
                        {
                            epgEntry.EpisodeSystemType = episodeNumber.System;

                            string[] parts = episodeNumber.Episode.Split(new char[] { '.' });
                            epgEntry.SeasonCrid = parts[0].Trim();
                            epgEntry.EpisodeCrid = parts[1].Trim();
                        }
                        else
                            epgEntry.EpisodeTag = programme.EpisodeNumbers[0].Episode;
                    }                        
                }   
            }

            if (programme.Directors != null && programme.Directors.Count != 0)
            {
                epgEntry.Directors = new Collection<string>();
                foreach (XmltvPerson director in programme.Directors)
                    epgEntry.Directors.Add(director.Name);
            }

            if (programme.Actors != null && programme.Actors.Count != 0)
            {
                epgEntry.Cast = new Collection<string>();
                foreach (XmltvPerson actor in programme.Actors)
                    epgEntry.Cast.Add(actor.Name);
            }

            if (programme.Guests != null && programme.Guests.Count != 0)
            {
                epgEntry.GuestStars = new Collection<string>();
                foreach (XmltvPerson guest in programme.Guests)
                    epgEntry.GuestStars.Add(guest.Name);
            }

            if (programme.Presenters != null && programme.Presenters.Count != 0)
            {
                epgEntry.Presenters = new Collection<string>();
                foreach (XmltvPerson presenter in programme.Presenters)
                    epgEntry.Presenters.Add(presenter.Name);
            }

            if (programme.Producers != null && programme.Producers.Count != 0)
            {
                epgEntry.Producers = new Collection<string>();
                foreach (XmltvPerson producer in programme.Producers)
                    epgEntry.Producers.Add(producer.Name);
            }

            if (programme.Writers != null && programme.Writers.Count != 0)
            {
                epgEntry.Writers = new Collection<string>();
                foreach (XmltvPerson writer in programme.Writers)
                    epgEntry.Writers.Add(writer.Name);
            }

            if (programme.Icon != null)
            {
                epgEntry.PosterPath = programme.Icon.Source;
                epgEntry.PosterHeight = programme.Icon.Height;
                epgEntry.PosterWidth = programme.Icon.Width;
            }

            if (languageCode != null)
                epgEntry.LanguageCode = languageCode.Code;

            epgEntry.NoLookup = noLookup;

            addEPGEntry(station, epgEntry);            
        }

        private string getStarRating(string rating)
        {
            string[] parts = rating.Trim().Split(new char[] { '/' });
            if (parts.Length != 2)
                return (null);

            int ratingValue = 0;
            int ratingMax = 0;

            try
            {
                ratingValue = Int32.Parse(parts[0].Trim());
                ratingMax = Int32.Parse(parts[1].Trim());
            }
            catch (FormatException)
            {
                return (null);
            }
            catch (OverflowException)
            {
                return (null);
            }

            int adjustedRating = ratingValue * (8 / ratingMax);

            switch (adjustedRating)
            {
                case 0:
                    return (null);
                case 1:
                    return ("+");
                case 2:
                    return ("*");
                case 3:
                    return ("*+");
                case 4:
                    return ("**");
                case 5:
                    return ("**+");
                case 6:
                    return ("***");
                case 7:
                    return ("***+");
                case 8:
                    return ("****");
                default:
                    return ("****");
            }
        }

        private string getEventCategory(string xmltvCategory, EPGEntry epgEntry)
        {
            /*if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options,
                OptionName.CustomCategoryOverride))
            {
                string eventCategory = getCustomCategory(epgEntry.EventName, epgEntry.ShortDescription);
                if (eventCategory != null)
                    return (eventCategory);
            }*/

            XmltvProgramCategory category = XmltvProgramCategory.FindCategory(xmltvCategory);
            if (category != null)
                return (category.Description);

            /*if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options,
                OptionName.CustomCategoryOverride))
                return (null);*/

            return (getCustomCategory(epgEntry.EventName, epgEntry.ShortDescription));
        }

        private string getCustomCategory(string title, string description)
        {
            string category = CustomProgramCategory.FindCategoryDescription(title);
            if (category != null)
                return (category);

            return (CustomProgramCategory.FindCategoryDescription(description));
        }

        private int getEpisodeData(string part)
        {
            string[] parts = part.Split(new char[] { '/' });

            try
            {
                return (Int32.Parse(parts[0]) + 1);
            }
            catch (FormatException)
            {
                return (-1);
            }
            catch (OverflowException)
            {
                return (-1);
            }
        }

        private void addEPGEntry(TVStation station, EPGEntry newEntry)
        {
            foreach (EPGEntry oldEntry in station.EPGCollection)
            {
                if (oldEntry.StartTime == newEntry.StartTime)
                    return;

                if (oldEntry.StartTime.CompareTo(newEntry.StartTime) > 0)
                {
                    station.EPGCollection.Insert(station.EPGCollection.IndexOf(oldEntry), newEntry);
                    return;
                }
            }

            station.EPGCollection.Add(newEntry);
        }

        private string findLanguageString(Collection<XmltvText> languageStrings, LanguageCode languageCode)
        {
            if (languageCode == null)
            {
                foreach (XmltvText textString in languageStrings)
                {
                    if (textString.Language == null)
                        return (textString.Text);
                }

                return (languageStrings[0].Text);
            }

            foreach (XmltvText textString in languageStrings)
            {
                if (textString.Language != null || textString.Language == languageCode.TranslationCode)
                    return (textString.Text);
            }

            foreach (XmltvText textString in languageStrings)
            {
                if (textString.Language == null)
                    return (textString.Text);
            }

            return (languageStrings[0].Text);
        }

        private string checkCategoryLanguage(XmltvText category, LanguageCode languageCode)
        {
            if (category.Language == null || languageCode == null)
                return (category.Text);

            if (category.Language == languageCode.TranslationCode)
                return (category.Text);

            return (null);
        }

        private string findSubtitlingsLanguage(Collection<XmltvSubtitling> subtitlings, LanguageCode languageCode)
        {
            if (languageCode == null)
            {
                foreach (XmltvSubtitling subtitling in subtitlings)
                {
                    if (subtitling.Language == null)
                        return (subtitling.Type);
                }

                return (subtitlings[0].Type);
            }

            foreach (XmltvSubtitling subtitling in subtitlings)
            {
                if (subtitling.Language != null && subtitling.Language == languageCode.TranslationCode)
                    return (subtitling.Type);
            }

            foreach (XmltvSubtitling subtitling in subtitlings)
            {
                if (subtitling.Language == null)
                    return (subtitling.Type);
            }

            return (null);
        }

        /// <summary>
        /// Process the channel information from an XMLTV file.
        /// </summary>
        /// <param name="fileName">The actual file path.</param>
        /// <param name="fileSpec">The file definition.</param>
        /// <returns>An error message or null if the file was processed successfully.</returns>
        public override string ProcessChannels(string fileName, ImportFileSpec fileSpec)
        {
            XmlReader xmlReader = null;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;
            settings.DtdProcessing = DtdProcessing.Ignore;
            settings.CheckCharacters = false;
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            
            try
            {
                xmlReader = XmlReader.Create(fileName, settings);
            }
            catch (IOException)
            {
                return("Failed to open " + fileName);
            }

            try
            {
                while (!xmlReader.EOF)
                {
                    xmlReader.Read();
                    if (xmlReader.IsStartElement())
                    {
                        switch (xmlReader.Name)
                        {
                            case "channel":
                                XmltvChannel channel = XmltvChannel.GetInstance(xmlReader.ReadSubtree());

                                if (XmltvChannel.Channels == null)
                                    XmltvChannel.Channels = new Collection<XmltvChannel>();
 
                                XmltvChannel.Channels.Add(channel);
                                
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (XmlException e)
            {
                return ("Failed to load xmltv file: " + e.Message);                
            }
            catch (IOException e)
            {
                return ("Failed to load xmltv file: " + e.Message);                
            }

            if (xmlReader != null)
                xmlReader.Close();

            return (null);
        }

        /// <summary>
        /// Clear the XMLTV data.
        /// </summary>
        public static void Clear()
        {
            if (XmltvChannel.Channels != null)
                XmltvChannel.Channels.Clear();

            if (XmltvProgramme.Programmes != null)
                XmltvProgramme.Programmes.Clear();
        }
    }
}
