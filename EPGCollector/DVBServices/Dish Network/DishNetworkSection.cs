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
    /// The class that describes a Dish Network section.
    /// </summary>
    public class DishNetworkSection
    {
        /// <summary>
        /// Get the collection of EIT enteries in the section.
        /// </summary>
        public Collection<EITEntry> EITEntries { get { return (eitEntries); } }

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

        private int lastIndex;

        /// <summary>
        /// Initialize a new instance of the DishNetworkSection class.
        /// </summary>
        public DishNetworkSection()
        {
            eitEntries = new Collection<EITEntry>();
            Logger.ProtocolIndent = "";

            if (DebugEntry.IsDefined(DebugName.LogTitles) && titleLogger == null)
                titleLogger = new Logger("EPG Titles.log");
            if (DebugEntry.IsDefined(DebugName.LogDescriptions) && descriptionLogger == null)
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
                throw (new ArgumentOutOfRangeException("The Dish Network section is short"));
            }

            Collection<TVStation> linkedStations = null;
            int linkedIndex = 0;

            TVStation tvStation = TVStation.FindStation(RunParameters.Instance.StationCollection,
                originalNetworkID, transportStreamID, serviceID);
            if (tvStation == null)
            {
                if (!OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options, OptionName.CreateMissingChannels))
                {
                    linkedStations = TVStation.FindEPGLinks(RunParameters.Instance.StationCollection,
                        originalNetworkID, transportStreamID, serviceID);
                    if (linkedStations != null)
                    {
                        tvStation = linkedStations[0];
                        linkedIndex = 1;
                    }
                    else
                        return;
                }
                else
                {
                    tvStation = new TVStation("Auto Generated: " + originalNetworkID + ":" + transportStreamID + ":" + serviceID);
                    tvStation.OriginalNetworkID = originalNetworkID;
                    tvStation.TransportStreamID = transportStreamID;
                    tvStation.ServiceID = serviceID;

                    RunParameters.Instance.StationCollection.Add(tvStation);
                }
            }
            else
            {
                linkedStations = TVStation.FindEPGLinks(RunParameters.Instance.StationCollection,
                    originalNetworkID, transportStreamID, serviceID);
                if (linkedStations != null)
                    linkedIndex = 0;
            }

            bool newSection = tvStation.AddMapEntry(mpeg2Header.TableID, mpeg2Header.SectionNumber, lastTableID, mpeg2Header.LastSectionNumber, segmentLastSectionNumber);
            if (!newSection)
                return;

            if (!tvStation.Included)
                return;

            while (lastIndex < byteData.Length - 4)
            {
                DishNetworkEntry dishNetworkEntry = new DishNetworkEntry();
                dishNetworkEntry.Process(byteData, lastIndex, mpeg2Header.TableID);

                EPGEntry epgEntry = new EPGEntry();
                epgEntry.OriginalNetworkID = tvStation.OriginalNetworkID;
                epgEntry.TransportStreamID = tvStation.TransportStreamID;
                epgEntry.ServiceID = tvStation.ServiceID;
                epgEntry.EPGSource = EPGSource.DishNetwork;

                if (dishNetworkEntry.HighDefinition)
                    epgEntry.VideoQuality = "HDTV";
                if (dishNetworkEntry.ClosedCaptions)
                    epgEntry.SubTitles = "teletext";
                if (dishNetworkEntry.Stereo)
                    epgEntry.AudioQuality = "stereo";

                epgEntry.Duration = Utils.RoundTime(dishNetworkEntry.Duration);
                epgEntry.EventID = dishNetworkEntry.EventID;
                epgEntry.EventName = EditSpec.ProcessTitle(dishNetworkEntry.EventName);
                
                getParentalRating(epgEntry, dishNetworkEntry);
                
                epgEntry.RunningStatus = dishNetworkEntry.RunningStatus;
                epgEntry.Scrambled = dishNetworkEntry.Scrambled;
                epgEntry.ShortDescription = EditSpec.ProcessDescription(dishNetworkEntry.ShortDescription);
                if (dishNetworkEntry.SubTitle != dishNetworkEntry.EventName)
                    epgEntry.EventSubTitle = dishNetworkEntry.SubTitle;

                if (tvStation.EPGLink == null)
                    epgEntry.StartTime = Utils.RoundTime(TimeOffsetEntry.GetOffsetTime(dishNetworkEntry.StartTime));
                else
                    epgEntry.StartTime = Utils.RoundTime(TimeOffsetEntry.GetOffsetTime(dishNetworkEntry.StartTime.AddMinutes(tvStation.EPGLink.TimeOffset)));

                epgEntry.EventCategory = getEventCategory(epgEntry.EventName, epgEntry.ShortDescription, dishNetworkEntry.ContentType, dishNetworkEntry.ContentSubType);

                epgEntry.StarRating = getStarRating(dishNetworkEntry);
                epgEntry.Date = dishNetworkEntry.Date;
                epgEntry.Cast = dishNetworkEntry.Cast;

                getSeriesEpisode(epgEntry, dishNetworkEntry.Series, dishNetworkEntry.Episode);

                epgEntry.HasGraphicLanguage = dishNetworkEntry.HasStrongLanguage;
                epgEntry.HasStrongSexualContent = dishNetworkEntry.HasSexualContent;
                epgEntry.HasGraphicViolence = dishNetworkEntry.HasViolence;
                epgEntry.HasNudity = dishNetworkEntry.HasNudity;

                epgEntry.PreviousPlayDate = dishNetworkEntry.OriginalAirDate;

                bool include = ControllerBase.CheckEPGDays(epgEntry.StartTime);
                if (include)
                {
                    tvStation.AddEPGEntry(epgEntry);

                    if (linkedStations != null && linkedStations.Count > linkedIndex)
                    {
                        for (int index = linkedIndex; index < linkedStations.Count; index++)
                        {
                            epgEntry.StartTime = Utils.RoundTime(TimeOffsetEntry.GetOffsetTime(dishNetworkEntry.StartTime.AddMinutes(linkedStations[index].EPGLink.TimeOffset)));
                            linkedStations[index].AddEPGEntry(epgEntry);
                        }
                    }

                    if (titleLogger != null)
                        logTitle(dishNetworkEntry.EventName, epgEntry, titleLogger);
                    if (descriptionLogger != null)
                    {
                        if (!DebugEntry.IsDefined(DebugName.LogOriginal))
                            logDescription(dishNetworkEntry.ShortDescription, epgEntry, descriptionLogger);
                        else
                            logDescription(dishNetworkEntry.OriginalDescription, epgEntry, descriptionLogger);
                    }
                }

                lastIndex = dishNetworkEntry.Index;
            }
        }

        private string getEventCategory(string title, string description, int contentType, int contentSubType)
        {
            if (contentType == -1 || contentSubType == -1)
                return (getCustomCategory(title, description));

            if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options,
                OptionName.CustomCategoryOverride))
            {
                string customCategory = getCustomCategory(title, description);
                if (customCategory != null)
                    return (customCategory);
            }

            if (contentType == 0)
            {
                DishNetworkProgramCategory categoryEntry = DishNetworkProgramCategory.FindCategory(0, contentSubType);
                if (categoryEntry == null)
                {
                    DishNetworkProgramCategory.AddUndefinedCategory(contentType, contentSubType, "", title);
                    if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options,
                        OptionName.CustomCategoryOverride))
                        return (null);
                    return (getCustomCategory(title, description));
                }

                if (categoryEntry.SampleEvent == null)
                    categoryEntry.SampleEvent = title;
                categoryEntry.UsedCount++;

                return (categoryEntry.Description);
            }

            DishNetworkProgramCategory mainCategoryEntry = DishNetworkProgramCategory.FindCategory(contentType, 0);
            if (mainCategoryEntry == null)
            {
                DishNetworkProgramCategory.AddUndefinedCategory(contentType, contentSubType, "", title);

                if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options,
                    OptionName.CustomCategoryOverride))
                    return (null);

                return (getCustomCategory(title, description));
            }

            if (contentSubType == 0 || !OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options,
                OptionName.UseContentSubtype))
            {
                if (mainCategoryEntry.SampleEvent == null)
                    mainCategoryEntry.SampleEvent = title;
                mainCategoryEntry.UsedCount++;
                return (mainCategoryEntry.Description);
            }

            DishNetworkProgramCategory subCategoryEntry = DishNetworkProgramCategory.FindCategory(0, contentSubType);
            if (subCategoryEntry == null)
            {
                DishNetworkProgramCategory.AddUndefinedCategory(contentType, contentSubType, "", title);

                if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options,
                    OptionName.CustomCategoryOverride))
                    return (null);

                return (getCustomCategory(title, description));
            }

            if (subCategoryEntry.SampleEvent == null)
                subCategoryEntry.SampleEvent = title;
            subCategoryEntry.UsedCount++;

            string combinedDescription = mainCategoryEntry.DishNetworkDescription + " - " + subCategoryEntry.DishNetworkDescription + "=" +
                mainCategoryEntry.WMCDescription + "," + subCategoryEntry.WMCDescription + "=" +
                mainCategoryEntry.DVBLogicDescription + "=" +
                mainCategoryEntry.DVBViewerDescription;

            DishNetworkProgramCategory generatedCategory = new DishNetworkProgramCategory(contentType, contentSubType, combinedDescription);
            
            return (generatedCategory.Description);
        }

        private string getCustomCategory(string title, string description)
        {
            string category = CustomProgramCategory.FindCategoryDescription(title);
            if (category != null)
                return (category);

            return (CustomProgramCategory.FindCategoryDescription(description));
        }

        private void getParentalRating(EPGEntry epgEntry, DishNetworkEntry entry)
        {
            epgEntry.ParentalRating = ParentalRating.FindRating("USA", "DISHNETWORK", entry.ParentalRating.ToString());
            epgEntry.MpaaParentalRating = ParentalRating.FindMpaaRating("USA", "DISHNETWORK", entry.ParentalRating.ToString());
            epgEntry.ParentalRatingSystem = ParentalRating.FindSystem("USA", "DISHNETWORK", entry.ParentalRating.ToString());            
        }

        private string getStarRating(DishNetworkEntry entry)
        {
            switch (entry.StarRating)
            {
                case 0:
                    return (null);
                case 1:
                    return ("*");
                case 2:
                    return ("*+");
                case 3:
                    return ("**");
                case 4:
                    return ("**+");
                case 5:
                    return ("***");
                case 6:
                    return ("***+");
                case 7:
                    return ("****");
                default:
                    return (null);
            }            
        }

        private void getSeriesEpisode(EPGEntry epgEntry, int series, int episode)
        {
            if (series > 0)
            {
                epgEntry.SeriesId = series.ToString();
                epgEntry.SeasonCrid = series.ToString();
            }

            if (episode > 0)
            {
                epgEntry.EpisodeId = episode.ToString();
                epgEntry.EpisodeCrid = episode.ToString();
            }
        }

        private void logTitle(string title, EPGEntry epgEntry, Logger logger)
        {
            string episodeInfo;

            if (DebugEntry.IsDefined(DebugName.LogEpisodeInfo))
            {
                episodeInfo = (epgEntry.SeasonNumber == -1 ? "" : epgEntry.SeasonNumber.ToString()) + ":" +
                    (epgEntry.EpisodeNumber == -1 ? "" : epgEntry.EpisodeNumber.ToString()); 
            }
            else
                episodeInfo = string.Empty;

            logger.Write(epgEntry.OriginalNetworkID + ":" + epgEntry.TransportStreamID + ":" + epgEntry.ServiceID + " " +
                epgEntry.StartTime.ToShortDateString() + " " +
                epgEntry.StartTime.ToString("HH:mm") + " - " +
                epgEntry.StartTime.Add(epgEntry.Duration).ToString("HH:mm") + " " +
                title + " " +
                episodeInfo);
        }

        private void logDescription(string description, EPGEntry epgEntry, Logger logger)
        {
            logger.Write(epgEntry.OriginalNetworkID + ":" + epgEntry.TransportStreamID + ":" + epgEntry.ServiceID + " " +
                epgEntry.StartTime.ToShortDateString() + " " +
                epgEntry.StartTime.ToString("HH:mm") + " - " +
                epgEntry.StartTime.Add(epgEntry.Duration).ToString("HH:mm") + " " +
                description);
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

            Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "DISH NETWORK SECTION: ONID: " + originalNetworkID +
                " TSID: " + transportStreamID +
                " SID: " + serviceID);
        }
    }
}
