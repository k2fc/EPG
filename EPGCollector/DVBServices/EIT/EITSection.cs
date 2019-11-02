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

using DomainObjects;

namespace DVBServices
{
    /// <summary>
    /// The class that describes an EIT section.
    /// </summary>
    public class EITSection
    {
        /// <summary>
        /// Get the collection of EIT entries in the section.
        /// </summary>
        public Collection<EITEntry> EITEntries { get { return (eitEntries); } }

        /// <summary>
        /// Get the collection of EIT category records.
        /// </summary>
        public static Collection<CategoryEntry> CategoryEntries { get { return (categoryEntries); } }

        /// <summary>
        /// Get the original network identification (ONID).
        /// </summary>
        public int OriginalNetworkID { get { return (originalNetworkID); } }
        /// <summary>
        /// Get the transport stream identification (TSID).
        /// </summary>
        public int TransportStreamID { get { return (transportStreamID); } }
        /// <summary>
        /// Get the service identification (SID).
        /// </summary>
        public int ServiceID { get { return (serviceID); } }

        /// <summary>
        /// Get the identification of the last table for the EIT section.
        /// </summary>
        public int LastTableID { get { return (lastTableID); } }
        /// <summary>
        /// Get the segment last section number for the EIT section.
        /// </summary>
        public int SegmentLastSectionNumber { get { return (segmentLastSectionNumber); } }        

        private Collection<EITEntry> eitEntries;

        private int transportStreamID;
        private int originalNetworkID;
        private int serviceID;

        private int segmentLastSectionNumber;
        private int lastTableID;

        private static Logger titleLogger;
        private static Logger descriptionLogger;

        private static Collection<CategoryEntry> categoryEntries;

        private int lastIndex;

        /// <summary>
        /// Initialize a new instance of the EITSection class.
        /// </summary>
        public EITSection()
        {
            eitEntries = new Collection<EITEntry>();
            Logger.ProtocolIndent = "";

            if (DebugEntry.IsDefined(DebugName.LogTitles) && titleLogger == null)
                titleLogger = new Logger("EPG Titles.log");
            if (DebugEntry.IsDefined(DebugName.LogDescriptions)&& descriptionLogger == null)
                descriptionLogger = new Logger("EPG Descriptions.log");
        }

        /// <summary>
        /// Parse the section.
        /// </summary>
        /// <param name="byteData">The MPEG2 section containing the section.</param>
        /// <param name="mpeg2Header">The MPEG2 header that preceedes the section.</param>
        public void Process(byte[] byteData, Mpeg2ExtendedHeader mpeg2Header)
        {
            lastIndex = mpeg2Header.Index;
            serviceID = mpeg2Header.TableIDExtension;

            try
            {
                transportStreamID = Utils.Convert2BytesToInt(byteData, lastIndex);
                lastIndex += 2;

                originalNetworkID = Utils.Convert2BytesToInt(byteData, lastIndex);
                lastIndex += 2;

                segmentLastSectionNumber = (int)byteData[lastIndex];
                lastIndex++;

                lastTableID = (int)byteData[lastIndex];
                lastIndex++;
            }
            catch (IndexOutOfRangeException)
            {
                throw (new ArgumentOutOfRangeException("EIT section is short"));
            }

            bool addStationNeeded = false;

            TVStation tvStation;
            if (!OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options, OptionName.SidMatchOnly))
                tvStation = TVStation.FindStation(RunParameters.Instance.StationCollection, originalNetworkID, transportStreamID, serviceID);
            else
                tvStation = TVStation.FindStation(RunParameters.Instance.StationCollection, serviceID);
            if (tvStation == null)
            {
                if (!OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options, OptionName.CreateMissingChannels))
                    return;
                else
                {
                    tvStation = new TVStation("Auto Generated: " + originalNetworkID + ":" + transportStreamID + ":" + serviceID);
                    tvStation.OriginalNetworkID = originalNetworkID;
                    tvStation.TransportStreamID = transportStreamID;
                    tvStation.ServiceID = serviceID;

                    addStationNeeded = true;
                }
            }

            bool newSection = tvStation.AddMapEntry(mpeg2Header.TableID, mpeg2Header.SectionNumber, lastTableID, mpeg2Header.LastSectionNumber, segmentLastSectionNumber);
            if (!newSection)
                return;

            while (lastIndex < byteData.Length - 4)
            {
                EITEntry eitEntry = new EITEntry();
                eitEntry.Process(byteData, lastIndex);

                if (eitEntry.StartTime != DateTime.MinValue)
                {
                    EPGEntry epgEntry = new EPGEntry();
                    epgEntry.OriginalNetworkID = tvStation.OriginalNetworkID;
                    epgEntry.TransportStreamID = tvStation.TransportStreamID;
                    epgEntry.ServiceID = tvStation.ServiceID;
                    epgEntry.EPGSource = EPGSource.EIT;

                    switch (eitEntry.ComponentTypeAudio)
                    {
                        case 3:
                            epgEntry.AudioQuality = "stereo";
                            break;
                        case 5:
                            epgEntry.AudioQuality = "dolby digital";
                            break;
                        default:
                            break;
                    }

                    if (eitEntry.ComponentTypeVideo > 9)
                        epgEntry.VideoQuality = "HDTV";

                    if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options, OptionName.UseDescAsCategory))
                        epgEntry.EventCategory = EditSpec.ProcessDescription(eitEntry.ShortDescription);
                    else
                        epgEntry.EventCategory = getEventCategory(eitEntry.EventName, 
                            EditSpec.ProcessDescription(eitEntry.Description), 
                            eitEntry.ContentType, eitEntry.ContentSubType);

                    epgEntry.Duration = Utils.RoundTime(eitEntry.Duration);
                    epgEntry.EventID = eitEntry.EventID;
                    epgEntry.EventName = EditSpec.ProcessTitle(eitEntry.EventName);

                    if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode != null)
                    {
                        epgEntry.ParentalRating = ParentalRating.FindRating(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode, "EIT", (eitEntry.ParentalRating + 3).ToString());
                        epgEntry.MpaaParentalRating = ParentalRating.FindMpaaRating(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode, "EIT", (eitEntry.ParentalRating + 3).ToString());
                    }
                    else
                    {
                        if (eitEntry.ParentalRating > 11)
                        {
                            epgEntry.ParentalRating = "AO";
                            epgEntry.MpaaParentalRating = "AO";
                        }
                        else
                        {
                            if (eitEntry.ParentalRating > 8)
                            {
                                epgEntry.ParentalRating = "PGR";
                                epgEntry.MpaaParentalRating = "PG";
                            }
                            else
                            {
                                epgEntry.ParentalRating = "G";
                                epgEntry.MpaaParentalRating = "G";
                            }
                        }
                    }

                    epgEntry.RunningStatus = eitEntry.RunningStatus;
                    epgEntry.Scrambled = eitEntry.Scrambled;

                    try
                    {
                        if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options, OptionName.UseDescAsCategory))
                            epgEntry.ShortDescription = EditSpec.ProcessDescription(eitEntry.ExtendedDescription);
                        else
                        {
                            if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options, OptionName.UseDescAsSubtitle))
                            {
                                epgEntry.ShortDescription = EditSpec.ProcessDescription(eitEntry.ExtendedDescription);
                                epgEntry.EventSubTitle = EditSpec.ProcessDescription(eitEntry.ShortDescription);
                            }
                            else
                            {
                                if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options, OptionName.UseNoDesc))
                                {
                                    if (string.IsNullOrWhiteSpace(eitEntry.ExtendedDescription))
                                        epgEntry.ShortDescription = EditSpec.ProcessDescription(eitEntry.ShortDescription);
                                    else
                                        epgEntry.ShortDescription = EditSpec.ProcessDescription(eitEntry.ExtendedDescription);
                                }
                                else
                                {
                                    string description = getCombinedTitleDescription(epgEntry, eitEntry.Description);
                                    epgEntry.ShortDescription = getShortDescription(EditSpec.ProcessDescription(description));
                                    epgEntry.EventSubTitle = getSubTitle(EditSpec.ProcessDescription(description));
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.Write("<E> An exception of type " + e.GetType().Name + " has occurred while processing a programme description");
                        Logger.Instance.Write("<E> " + e.Message);
                    }

                    if (!string.IsNullOrWhiteSpace(epgEntry.ShortDescription))
                    {
                        if (epgEntry.ShortDescription.StartsWith("'") && epgEntry.ShortDescription.EndsWith("'"))
                            epgEntry.ShortDescription = epgEntry.ShortDescription.Substring(1, epgEntry.ShortDescription.Length - 2);
                    }

                    epgEntry.StartTime = Utils.RoundTime(TimeOffsetEntry.GetOffsetTime(eitEntry.StartTime));
                    epgEntry.LanguageCode = eitEntry.LanguageCode;

                    epgEntry.Cast = eitEntry.Cast;
                    epgEntry.Directors = eitEntry.Directors;
                    epgEntry.Writers = eitEntry.Writers;
                    epgEntry.Date = getDate(eitEntry, epgEntry);
                    if (eitEntry.TVRating != null)
                        epgEntry.ParentalRating = eitEntry.TVRating;
                    epgEntry.StarRating = eitEntry.StarRating;

                    if (eitEntry.TVRating != null)
                        epgEntry.ParentalRating = eitEntry.TVRating;

                    if (epgEntry.EventSubTitle == null)
                        epgEntry.EventSubTitle = eitEntry.Subtitle;

                    bool sePresent = false;

                    try
                    {
                        getSeriesEpisodeIds(epgEntry, eitEntry);
                        sePresent = getSeasonEpisodeNumbers(epgEntry, eitEntry);                            
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.Write("<E> An exception of type " + e.GetType().Name + " has occurred while extracting sesonepisode numbers");
                        Logger.Instance.Write("<E> " + e.Message);
                    }

                    if (!sePresent)
                        updateShortDescription(epgEntry);

                    epgEntry.Country = eitEntry.Country;

                    bool include = ControllerBase.CheckEPGDays(epgEntry.StartTime);
                    if (include)
                    {
                        tvStation.AddEPGEntry(epgEntry);

                        if (titleLogger != null)
                            logTitle(eitEntry.EventName, eitEntry, epgEntry, titleLogger);
                        if (descriptionLogger != null)
                            logTitle(eitEntry.Description, eitEntry, epgEntry, descriptionLogger);

                        if (DebugEntry.IsDefined(DebugName.CatXref))
                            updateCategoryEntries(tvStation, eitEntry);
                    }
                }

                lastIndex = eitEntry.Index;
            }

            if (addStationNeeded)
                RunParameters.Instance.StationCollection.Add(tvStation);
        }

        private string getCombinedTitleDescription(EPGEntry epgEntry, string description)
        {
            if (description == null || epgEntry.EventName == null)
                return (null);

            string editedDescription;

            if (epgEntry.EventName.EndsWith("...") && description.StartsWith("...") &&
                    epgEntry.EventName.Length > 3 && description.Length > 3)
            {
                int fullStopIndex = description.IndexOf('.', 3);
                int colonIndex = description.IndexOf(':', 3);

                int index = fullStopIndex;
                if (colonIndex != -1 && colonIndex < index)
                    index = colonIndex;

                if (index != -1)
                {
                    epgEntry.EventName = epgEntry.EventName.Substring(0, epgEntry.EventName.Length - 3) + " " +
                        description.Substring(3, index - 3);
                    editedDescription = description.Substring(index + 1).Trim();
                }
                else
                    editedDescription = description;
            }
            else
                editedDescription = description;

            return (editedDescription);
        }

        private string getShortDescription(string description)
        {
            if (description == null)
                return (null);

            if (description.Length > 0 && description[0] == '(')
                return (description);

            int index = description.IndexOf(':');
            if (index < 5 || index > 60)
                index = -1;

            return (index == -1 ? description : description.Substring(index + 1).Trim());
        }

        private string getSubTitle(string description)
        {
            if (description == null || (description.Length > 0 && description[0] == '('))
                return (null);

            int index = description.IndexOf(':');
            if (index < 5 ||index > 60)
                index = -1;

            return (index < 1 ? null : description.Substring(0, index).Trim());
        }

        private bool getSeasonEpisodeNumbers(EPGEntry epgEntry, EITEntry eitEntry)
        {
            bool sePresent = false;

            switch (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode)
            {
                case Country.NewZealand:
                    sePresent = Utils.GetNZLSeasonEpisodeNumbers(epgEntry);
                    break;
                case Country.Egypt:
                    sePresent = processEgyptSeasonEpisodeNumbers(epgEntry);
                    break;
                default:
                    sePresent = getOtherSeasonEpisodeNumbers(epgEntry);
                    break;
            }

            if (epgEntry.SeasonNumber == -1 && epgEntry.EpisodeNumber == -1)
            {
                epgEntry.SeasonNumber = eitEntry.SeasonNumber;
                epgEntry.EpisodeNumber = eitEntry.EpisodeNumber;                
            }

            return sePresent;
        }

        private bool getOtherSeasonEpisodeNumbers(EPGEntry epgEntry)
        {
            if (epgEntry.ShortDescription == null || !epgEntry.ShortDescription.StartsWith("("))
                return false;

            int series = 0;
            int episode = 0;
            int index = (epgEntry.ShortDescription.StartsWith("(Ep. ") ? 5 : 1);
            
            while (index < epgEntry.ShortDescription.Length && epgEntry.ShortDescription[index] >= '0' && epgEntry.ShortDescription[index] <= '9')
            {
                episode = (episode * 10) + (epgEntry.ShortDescription[index] - '0');
                index++;
            }

            if (index == epgEntry.ShortDescription.Length)
                return false;

            if (epgEntry.ShortDescription[index] != ')')
            {
                if (index + 1 == epgEntry.ShortDescription.Length)
                    return false;

                if (epgEntry.ShortDescription[index] == ':')
                {
                    index++;

                    int parts = 0;

                    while (index < epgEntry.ShortDescription.Length && epgEntry.ShortDescription[index] >= '0' && epgEntry.ShortDescription[index] <= '9')
                    {
                        parts = (parts * 10) + (epgEntry.ShortDescription[index] - '0');
                        index++;
                    }

                    if (index == epgEntry.ShortDescription.Length)
                        return false;

                    if (epgEntry.ShortDescription[index] != '/' || index + 1 == epgEntry.ShortDescription.Length || epgEntry.ShortDescription[index + 1] != 's')
                        return false;
                }
                else
                {
                    if (epgEntry.ShortDescription[index] != '/' || index + 1 == epgEntry.ShortDescription.Length || epgEntry.ShortDescription[index + 1] != 's')
                        return false;
                }

                index += 2;

                if (index >= epgEntry.ShortDescription.Length)
                    return false;

                while (index < epgEntry.ShortDescription.Length && epgEntry.ShortDescription[index] >= '0' && epgEntry.ShortDescription[index] <= '9')
                {
                    series = (series * 10) + (epgEntry.ShortDescription[index] - '0');
                    index++;
                }

                if (index == epgEntry.ShortDescription.Length)
                    return false;

                if (epgEntry.ShortDescription[index] != ')')
                    return false;
            }

            epgEntry.SeasonNumber = series;
            epgEntry.EpisodeNumber = episode;

            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
            {
                if (index + 1 < epgEntry.ShortDescription.Length)
                    epgEntry.ShortDescription = epgEntry.ShortDescription.Substring(index + 1).TrimStart();
                else
                    epgEntry.ShortDescription = null;

                return false;
            }

            return true;
        }

        private bool processEgyptSeasonEpisodeNumbers(EPGEntry epgEntry)
        {
            string seasonId = "Season ";
            string episodeId = "Episode ";

            string description = null;
            bool inDescription = true;

            if (epgEntry.ShortDescription != null && (epgEntry.ShortDescription.StartsWith(seasonId) || epgEntry.ShortDescription.StartsWith(episodeId)))
                description = epgEntry.ShortDescription;
            else
            {
                if (epgEntry.EventSubTitle != null && (epgEntry.EventSubTitle.StartsWith(seasonId) || epgEntry.EventSubTitle.StartsWith(episodeId)))
                {
                    description = epgEntry.EventSubTitle;
                    inDescription = false;
                }
                else
                    return false;
            }

            int season = -1;
            int episode = -1;
            int index;
            bool episodePresent = true;

            if (description.StartsWith(seasonId))
            {
                index = seasonId.Length;
                season = 0;

                while (index < description.Length && char.IsDigit(description[index]))
                {
                    season = (season * 10) + (description[index] - '0');
                    index++;
                }

                if (index < description.Length && description[index] == ',')
                    index++;
                if (index < description.Length && description[index] == ' ')
                    index++;

                if (index + episodeId.Length < description.Length)
                {
                    if (description.Substring(index, episodeId.Length) == episodeId)
                        index += episodeId.Length;
                    else
                        episodePresent = false;
                }
            }
            else
            {
                index = episodeId.Length;
                if (description[index] == ' ')
                    index++;
            }

            if (episodePresent)
            {
                episode = 0;

                while (index < description.Length && char.IsDigit(description[index]))
                {
                    episode = (episode * 10) + (description[index] - '0');
                    index++;
                }

                if (index < description.Length && description[index] == '.')
                    index++;
            }

            epgEntry.SeasonNumber = season == 0 ? -1 : season;
            epgEntry.EpisodeNumber = episode == 0 ? -1 : episode;

            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
            {
                if (index + 2 < description.Length)
                {
                    if (inDescription)
                        epgEntry.ShortDescription = description.Substring(index).Trim();
                    else
                        epgEntry.EventSubTitle = description.Substring(index).Trim();
                }
                else
                {
                    if (inDescription)
                        epgEntry.ShortDescription = null;
                    else
                        epgEntry.EventSubTitle = null;

                    return false;
                }
            }

            return true;
        }

        private void getSeriesEpisodeIds(EPGEntry epgEntry, EITEntry eitEntry)
        {
            if (eitEntry.SeriesId == null && eitEntry.EpisodeId == null)
                return;

            epgEntry.SeasonCrid = eitEntry.SeriesId;
            epgEntry.EpisodeCrid = eitEntry.EpisodeId;

            if (eitEntry.SeriesId != null)
            {
                if (eitEntry.EpisodeId != null)
                {
                    epgEntry.SeriesId = eitEntry.SeriesId;
                    epgEntry.EpisodeId = eitEntry.EpisodeId;
                }
                else
                {
                    epgEntry.SeriesId = eitEntry.SeriesId;
                    epgEntry.EpisodeId = null;
                }
            }
            else
            {
                epgEntry.SeriesId = null;
                epgEntry.EpisodeId = eitEntry.EpisodeId;
            }
        }

        private string getEventCategory(string title, string description, int contentType, int contentSubType)
        {     
            if (contentType == -1 || contentSubType == -1)
                return (getCustomCategory(title, description));

            if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options, OptionName.CustomCategoryOverride))
            {
                string customCategory = getCustomCategory(title, description);
                if (customCategory != null)
                    return (customCategory);
            }

            EITProgramContent contentEntry;

            if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options, OptionName.UseContentSubtype))
            {
                contentEntry = EITProgramContent.FindContent(contentType, contentSubType);
                if (contentEntry != null)
                {
                    if (contentEntry.SampleEvent == null)
                        contentEntry.SampleEvent = title;
                    contentEntry.UsedCount++;
                    return (contentEntry.Description);
                }
            }

            contentEntry = EITProgramContent.FindContent(contentType, 0);
            if (contentEntry != null)
            {
                if (contentEntry.SampleEvent == null)
                    contentEntry.SampleEvent = title;
                contentEntry.UsedCount++;
                return (contentEntry.Description);
            }

            EITProgramContent.AddUndefinedContent(contentType, contentSubType, "", title);

            if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options,
                OptionName.CustomCategoryOverride))
                return (null);

            return (getCustomCategory(title, description));
        }

        private string getCustomCategory(string title, string description)
        {
            string category = CustomProgramCategory.FindCategoryDescription(title);
            if (category != null)
                return (category);

            return(CustomProgramCategory.FindCategoryDescription(description));            
        }

        private string getDate(EITEntry eitEntry, EPGEntry epgEntry)
        {
            if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode != Country.Egypt)
                return (eitEntry.Year);

            if (epgEntry.ShortDescription == null || epgEntry.ShortDescription.Length < 4)
                return (null);

            if (!char.IsDigit(epgEntry.ShortDescription[0]) ||
                !char.IsDigit(epgEntry.ShortDescription[1]) ||
                !char.IsDigit(epgEntry.ShortDescription[2]) ||
                !char.IsDigit(epgEntry.ShortDescription[3]))
                return (null);

            if (epgEntry.ShortDescription.Length > 4 && epgEntry.ShortDescription[4] != ':')
                return (null);

            string date = epgEntry.ShortDescription.Substring(0, 4);

            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
            {
                if (epgEntry.ShortDescription.Length > 5)
                    epgEntry.ShortDescription = epgEntry.ShortDescription.Substring(5).Trim();
                else
                    epgEntry.ShortDescription = null;
            }

            return (date);
        }

        private void updateShortDescription(EPGEntry epgEntry)
        {
            if (!OptionEntry.IsDefined(RunParameters.Instance.Options, OptionName.AddSeasonEpisodeToDesc))
                return;

            if (epgEntry.SeasonNumber == -1 && epgEntry.EpisodeNumber == -1)
                return;

            string seasonSuffix = epgEntry.SeasonNumber != -1 ? "S" + epgEntry.SeasonNumber : null;
            string episodeSuffix = epgEntry.EpisodeNumber != -1 ? "Ep" + epgEntry.EpisodeNumber : null;

            string fullSuffix;

            if (seasonSuffix != null)
            {
                fullSuffix = seasonSuffix;

                if (episodeSuffix != null)
                    fullSuffix += " " + episodeSuffix;
            }
            else
                fullSuffix = episodeSuffix;

            if (epgEntry.ShortDescription != null)
                epgEntry.ShortDescription += " (" + fullSuffix + ")";
            else
                epgEntry.ShortDescription = " (" + fullSuffix + ")";
        }

        private void logTitle(string title, EITEntry eitEntry, EPGEntry epgEntry, Logger logger)
        {
            logger.Write(epgEntry.OriginalNetworkID + ":" + epgEntry.TransportStreamID + ":" + epgEntry.ServiceID + " " +            
                epgEntry.StartTime.ToShortDateString() + " " +
                epgEntry.StartTime.ToString("HH:mm") + " - " +
                epgEntry.StartTime.Add(epgEntry.Duration).ToString("HH:mm") + " " +
                "Content: " + eitEntry.ContentType + "/" + eitEntry.ContentSubType + " " +
                title);
        }

        private void logDescription(string description, EITEntry eitEntry, EPGEntry epgEntry, Logger logger)
        {
            logger.Write(epgEntry.OriginalNetworkID + ":" + epgEntry.TransportStreamID + ":" + epgEntry.ServiceID + " " +
                epgEntry.StartTime.ToShortDateString() + " " +
                epgEntry.StartTime.ToString("HH:mm") + " - " +
                epgEntry.StartTime.Add(epgEntry.Duration).ToString("HH:mm") + " " +
                "Content: " + eitEntry.ContentType + "/" + eitEntry.ContentSubType + " " +
                description);
        }

        private void updateCategoryEntries(TVStation tvStation, EITEntry eitEntry)
        {
            if (categoryEntries == null)
                categoryEntries = new Collection<CategoryEntry>();

            CategoryEntry newEntry = new CategoryEntry(tvStation.OriginalNetworkID, tvStation.TransportStreamID, tvStation.ServiceID, eitEntry.StartTime, eitEntry.EventName, eitEntry.ContentType, eitEntry.ContentSubType);

            foreach (CategoryEntry oldEntry in categoryEntries)
            {
                if (oldEntry.NetworkID == newEntry.NetworkID &&
                    oldEntry.TransportStreamID == newEntry.TransportStreamID &&
                    oldEntry.ServiceID == newEntry.ServiceID &&
                    oldEntry.StartTime == newEntry.StartTime)
                    return;

                if (oldEntry.NetworkID > newEntry.NetworkID)
                {
                    categoryEntries.Insert(categoryEntries.IndexOf(oldEntry), newEntry);
                    return;
                }

                if (oldEntry.NetworkID == newEntry.NetworkID &&
                    oldEntry.TransportStreamID > newEntry.TransportStreamID)
                {
                    categoryEntries.Insert(categoryEntries.IndexOf(oldEntry), newEntry);
                    return;
                }

                if (oldEntry.NetworkID == newEntry.NetworkID &&
                    oldEntry.TransportStreamID == newEntry.TransportStreamID &&
                    oldEntry.ServiceID > newEntry.ServiceID)
                {
                    categoryEntries.Insert(categoryEntries.IndexOf(oldEntry), newEntry);
                    return;
                }

                if (oldEntry.NetworkID == newEntry.NetworkID &&
                    oldEntry.TransportStreamID == newEntry.TransportStreamID &&
                    oldEntry.ServiceID == newEntry.ServiceID &&
                    oldEntry.StartTime > newEntry.StartTime)
                {
                    categoryEntries.Insert(categoryEntries.IndexOf(oldEntry), newEntry);
                    return;
                }
            }

            categoryEntries.Add(newEntry);
        }

        /// <summary>
        /// Validate the entry fields.
        /// </summary>
        public void Validate() { }

        /// <summary>
        /// Log the entry fields.
        /// </summary>
        public void LogMessage()
        {
            if (Logger.ProtocolLogger == null)
                return;

            Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "EIT Section: ONID: " + originalNetworkID +
                " TSID: " + transportStreamID +
                " SID: " + serviceID);
        }
    }
}
