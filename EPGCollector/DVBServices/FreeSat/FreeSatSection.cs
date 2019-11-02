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
    /// The class that describes an FreeSat section.
    /// </summary>
    class FreeSatSection
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
        /// Initialize a new instance of the FreeSatSection class.
        /// </summary>
        public FreeSatSection()
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
                throw (new ArgumentOutOfRangeException("The FreeSat EIT section is short"));
            }

            TVStation tvStation = TVStation.FindStation(RunParameters.Instance.StationCollection,
                originalNetworkID, transportStreamID, serviceID);
            if (tvStation == null)
                return;

            bool newSection = tvStation.AddMapEntry(mpeg2Header.TableID, mpeg2Header.SectionNumber, lastTableID, mpeg2Header.LastSectionNumber, segmentLastSectionNumber);
            if (!newSection)
                return;

            while (lastIndex < byteData.Length - 4)
            {
                FreeSatEntry freeSatEntry = new FreeSatEntry();
                freeSatEntry.Process(byteData, lastIndex);

                EPGEntry epgEntry = new EPGEntry();
                epgEntry.OriginalNetworkID = tvStation.OriginalNetworkID;
                epgEntry.TransportStreamID = tvStation.TransportStreamID;
                epgEntry.ServiceID = tvStation.ServiceID;
                epgEntry.EPGSource = EPGSource.FreeSat;

                switch (freeSatEntry.ComponentTypeAudio)
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

                if (freeSatEntry.ComponentTypeVideo > 9)
                    epgEntry.VideoQuality = "HDTV";

                epgEntry.Duration = Utils.RoundTime(freeSatEntry.Duration);
                epgEntry.EventID = freeSatEntry.EventID;
                epgEntry.EventName = EditSpec.ProcessTitle(freeSatEntry.EventName);

                if (freeSatEntry.ParentalRating > 11)
                    epgEntry.ParentalRating = "AO";
                else
                {
                    if (freeSatEntry.ParentalRating > 8)
                        epgEntry.ParentalRating = "PGR";
                    else
                        epgEntry.ParentalRating = "G";
                }

                try
                {
                    getSeriesEpisodeIds(epgEntry, freeSatEntry);
                    getEpisodeNumber(epgEntry, freeSatEntry);
                }
                catch (Exception e)
                {
                    Logger.Instance.Write("<E> An exception of type " + e.GetType().Name + " has occurred while extracting sesonepisode numbers");
                    Logger.Instance.Write("<E> " + e.Message);
                }

                epgEntry.RunningStatus = freeSatEntry.RunningStatus;
                epgEntry.Scrambled = freeSatEntry.Scrambled;

                try
                {
                    string description = getCombinedTitleDescription(epgEntry, freeSatEntry.ShortDescription);
                    if (description != null)
                    {
                        epgEntry.ShortDescription = getShortDescription(EditSpec.ProcessDescription(description));
                        epgEntry.EventSubTitle = getSubTitle(epgEntry, EditSpec.ProcessDescription(description));
                    }
                }
                catch (Exception e)
                {
                    Logger.Instance.Write("<E> An exception of type " + e.GetType().Name + " has occurred while processing a programme description");
                    Logger.Instance.Write("<E> " + e.Message);
                }

                epgEntry.StartTime = Utils.RoundTime(TimeOffsetEntry.GetOffsetTime(freeSatEntry.StartTime));
                epgEntry.LanguageCode = freeSatEntry.LanguageCode;

                epgEntry.EventCategory = getEventCategory(epgEntry.EventName, epgEntry.ShortDescription, freeSatEntry.ContentType, freeSatEntry.ContentSubType);
                setDate(epgEntry, '(', ')');

                bool include = ControllerBase.CheckEPGDays(epgEntry.StartTime);
                if (include)
                {
                    tvStation.AddEPGEntry(epgEntry);

                    if (titleLogger != null)
                        logTitle(freeSatEntry.EventName, epgEntry, titleLogger);
                    if (descriptionLogger != null)
                        logDescription(freeSatEntry.ShortDescription, epgEntry, descriptionLogger);
                }

                lastIndex = freeSatEntry.Index;
            }
        }

        private string getCombinedTitleDescription(EPGEntry epgEntry, string description)
        {
            if (description == null || epgEntry.EventName == null)
                return (description);            

            if (DebugEntry.IsDefined(DebugName.LogBrokenTitles))
            {
                if (epgEntry.EventName.EndsWith("..") && description.StartsWith(".."))
                    Logger.Instance.Write("Broken title: " + epgEntry.EventName + "/" + description);
            }

            int titleDotCount = 0;
            int descriptionDotCount = 0;

            if (epgEntry.EventName.EndsWith("..."))
                titleDotCount = 3;
            else
            {
                if (epgEntry.EventName.EndsWith(".."))
                    titleDotCount = 2;
            }

            if (description.StartsWith("..."))
                descriptionDotCount = 3;
            else
            {
                if (description.StartsWith(".."))
                    descriptionDotCount = 2;
            }

            if (titleDotCount == 0 || descriptionDotCount == 0)
                return (description);
            if (epgEntry.EventName.Length == titleDotCount || description.Length == descriptionDotCount)
                return (description);

            int fullStopIndex = description.IndexOf('.', descriptionDotCount);
            int questionMarkIndex = description.IndexOf('?', descriptionDotCount);
            int exclamationMarkIndex = description.IndexOf('!', descriptionDotCount);
            int colonIndex = description.IndexOf(':', descriptionDotCount);

            int index = fullStopIndex;
            if (questionMarkIndex != -1 && questionMarkIndex < index)
                index = questionMarkIndex;
            else
            {
                if (exclamationMarkIndex != -1 && exclamationMarkIndex < index)
                    index = exclamationMarkIndex;
                else
                {
                    if (colonIndex != -1 && colonIndex < index)
                        index = colonIndex;
                }
            }

            if (index == -1)
                return(description);

            epgEntry.EventName = epgEntry.EventName.Substring(0, epgEntry.EventName.Length - titleDotCount) + " " +
                description.Substring(descriptionDotCount, (index + 1) - (descriptionDotCount + 1));

            return (description.Substring(index + 1).Trim());                
        }

        private string getShortDescription(string description)
        {
            if (description == null)
                return (null);

            int subTitleIndex = description.IndexOf(':');

            return (subTitleIndex == -1 ? description.Trim() : description.Substring(subTitleIndex + 1).Trim());
        }

        private string getSubTitle(EPGEntry epgEntry, string description)
        {
            if (description == null)
                return (null);

            int index = description.IndexOf(':');
            if (index != -1)
                return (description.Substring(0, index).Trim());

            return (null);
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

            EITProgramContent contentEntry;

            if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options,
                OptionName.UseContentSubtype))
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

            return (CustomProgramCategory.FindCategoryDescription(description));
        }

        private void getSeriesEpisodeIds(EPGEntry epgEntry, FreeSatEntry freeSatEntry)
        {
            if (freeSatEntry.SeriesId == null && freeSatEntry.EpisodeId == null)
                return;

            epgEntry.SeasonCrid = freeSatEntry.SeriesId;
            epgEntry.EpisodeCrid = freeSatEntry.EpisodeId;

            if (freeSatEntry.SeriesId != null)
            {
                if (freeSatEntry.EpisodeId != null)
                {
                    epgEntry.SeriesId = freeSatEntry.SeriesId;
                    epgEntry.EpisodeId = freeSatEntry.EpisodeId;
                }
                else
                {
                    epgEntry.SeriesId = freeSatEntry.SeriesId;
                    epgEntry.EpisodeId = null;
                }
            }
            else
            {
                epgEntry.SeriesId = null;
                epgEntry.EpisodeId = freeSatEntry.EpisodeId;
            }
        }

        private void getEpisodeNumber(EPGEntry epgEntry, FreeSatEntry freeSatEntry)
        {
            if (freeSatEntry.ShortDescription == null)
                return;

            int index1 = freeSatEntry.ShortDescription.IndexOf(" (S");
            if (index1 != -1)
            {
                if (getEpisodeNumberFormat2(epgEntry, freeSatEntry, index1))
                    return;
            }

            index1 = freeSatEntry.ShortDescription.IndexOf(" Ep ");
            if (index1 != -1)
            {
                if (getEpisodeNumberFormat3(epgEntry, freeSatEntry, index1))
                    return;
            }

            index1 = freeSatEntry.ShortDescription.LastIndexOf(" Series ");
            if (index1 != -1)
            {
                if (getEpisodeNumberFormat5(epgEntry, freeSatEntry, index1))
                    return;
            }

            index1 = freeSatEntry.ShortDescription.LastIndexOf(" S");
            if (index1 != -1)
            {
                if (getEpisodeNumberFormat4(epgEntry, freeSatEntry, index1))
                    return;
            }

            index1 = freeSatEntry.ShortDescription.IndexOf("/");
            if (index1 != -1)
            {
                if (getEpisodeNumberFormat1(epgEntry, freeSatEntry, index1))
                    return;
            }
        }

        /// <summary>
        /// Format is nn/nn.
        /// </summary>
        private bool getEpisodeNumberFormat1(EPGEntry epgEntry, FreeSatEntry freeSatEntry, int index1)
        {
            if (index1 == 0 || !char.IsDigit(freeSatEntry.ShortDescription[index1 - 1]))
                return (false);

            if (index1 + 1 == freeSatEntry.ShortDescription.Length || !char.IsDigit(freeSatEntry.ShortDescription[index1 + 1]))
                return (false);

            int episodeNumber = 0;
            int index2 = index1 - 1;
            int multiplier = 1;

            while (index2 > -1 && char.IsDigit(freeSatEntry.ShortDescription[index2]))
            {
                episodeNumber = episodeNumber + ((freeSatEntry.ShortDescription[index2] - '0') * multiplier);
                multiplier *= 10;
                index2--;
            }

            int episodeCount = 0;
            int index3 = index1 + 1;

            while (index3 < freeSatEntry.ShortDescription.Length && char.IsDigit(freeSatEntry.ShortDescription[index3]))
            {
                episodeCount = (episodeCount * 10) + (freeSatEntry.ShortDescription[index3] - '0');
                index3++;
            }

            if (episodeNumber <= episodeCount)
                epgEntry.EpisodeNumber = episodeNumber;

            return (true);
        }

        /// <summary>
        /// Format is Snn, Epnn (spaces optional, comma optional, 'p' optional, can also be all upper case)
        /// </summary>
        private bool getEpisodeNumberFormat2(EPGEntry epgEntry, FreeSatEntry freeSatEntry, int index1)
        {
            index1 += 3;

            if (index1 > freeSatEntry.ShortDescription.Length)
                return (false);

            if (freeSatEntry.ShortDescription[index1] == ' ')
                index1++;

            if (index1 > freeSatEntry.ShortDescription.Length)
                return (false);

            if (!char.IsDigit(freeSatEntry.ShortDescription[index1]))
                return (false);

            int seasonNumber = 0;

            while (char.IsDigit(freeSatEntry.ShortDescription[index1]))
            {
                seasonNumber = (seasonNumber * 10) + (freeSatEntry.ShortDescription[index1] - '0');
                index1++;
            }

            if (freeSatEntry.ShortDescription[index1] == ',')
                index1++;
            if (freeSatEntry.ShortDescription[index1] == ' ')
                index1++;

            if (index1 + 3 > freeSatEntry.ShortDescription.Length ||
                (freeSatEntry.ShortDescription[index1] != 'E' && freeSatEntry.ShortDescription[index1] != 'e') ||
                (freeSatEntry.ShortDescription[index1 + 1] != 'P' && freeSatEntry.ShortDescription[index1 + 1] != 'p'))
                return (false);

            index1+= 2;
            if (freeSatEntry.ShortDescription[index1] == ' ')
                index1++;

            int episodeNumber = 0;

            while (index1 < freeSatEntry.ShortDescription.Length && char.IsDigit(freeSatEntry.ShortDescription[index1]))
            {
                episodeNumber = (episodeNumber * 10) + (freeSatEntry.ShortDescription[index1] - '0');
                index1++;
            }

            epgEntry.SeasonNumber = seasonNumber;
            epgEntry.EpisodeNumber = episodeNumber;

            return (true);
        }

        /// <summary>
        /// Format is Snn, Epnn (comma optional)
        /// </summary>
        private bool getEpisodeNumberFormat3(EPGEntry epgEntry, FreeSatEntry freeSatEntry, int index1)
        {
            if (index1 + 4 > freeSatEntry.ShortDescription.Length)
                return (false);

            if (freeSatEntry.ShortDescription[index1 - 1] == ' ')
            {
                if (!char.IsDigit(freeSatEntry.ShortDescription[index1 - 1]))
                    return (false);
            }
            else
            {
                if (!char.IsDigit(freeSatEntry.ShortDescription[index1 - 2]))
                    return (false);                
            }

            int index2 = index1;

            while (index2 > 0 && freeSatEntry.ShortDescription[index2] != 'S')
                index2--;

            if (freeSatEntry.ShortDescription[index2] != 'S')
                return (false);

            index2++;

            int seasonNumber = 0;

            while (char.IsDigit(freeSatEntry.ShortDescription[index2]))
            {
                seasonNumber = (seasonNumber * 10) + (freeSatEntry.ShortDescription[index2] - '0');
                index2++;
            }

            index1+= 3;
            if (freeSatEntry.ShortDescription[index1] == ' ')
                index1++;

            int episodeNumber = 0;

            while (index1 < freeSatEntry.ShortDescription.Length && char.IsDigit(freeSatEntry.ShortDescription[index1]))
            {
                episodeNumber = (episodeNumber * 10) + (freeSatEntry.ShortDescription[index1] - '0');
                index1++;
            }

            epgEntry.SeasonNumber = seasonNumber;
            epgEntry.EpisodeNumber = episodeNumber;

            return (true);
        }

        /// <summary>
        /// Format is Snn EPnn at the end of the description (space optional, P optional).
        /// </summary>
        private bool getEpisodeNumberFormat4(EPGEntry epgEntry, FreeSatEntry freeSatEntry, int index1)
        {
            if (index1 + 4 > freeSatEntry.ShortDescription.Length)
                return (false);

            index1 += 2;

            if (!char.IsDigit(freeSatEntry.ShortDescription[index1]))
                return (false);

            int seasonNumber = 0;

            while (index1 < freeSatEntry.ShortDescription.Length && char.IsDigit(freeSatEntry.ShortDescription[index1]))
            {
                seasonNumber = (seasonNumber * 10) + (freeSatEntry.ShortDescription[index1] - '0');
                index1++;
            }

            if (index1 + 2 > freeSatEntry.ShortDescription.Length)
                return (false);

            if (freeSatEntry.ShortDescription[index1] == ' ')
                index1++;

            if (freeSatEntry.ShortDescription[index1] != 'E')
                return (false);

            index1++;
            if (freeSatEntry.ShortDescription[index1] == 'P' || freeSatEntry.ShortDescription[index1] == 'p')
                index1++;

            int episodeNumber = 0;

            while (index1 < freeSatEntry.ShortDescription.Length && char.IsDigit(freeSatEntry.ShortDescription[index1]))
            {
                episodeNumber = (episodeNumber * 10) + (freeSatEntry.ShortDescription[index1] - '0');
                index1++;
            }

            epgEntry.SeasonNumber = seasonNumber;
            epgEntry.EpisodeNumber = episodeNumber;

            return (true);
        }

        /// <summary>
        /// Format is Series nn, Episode nn.
        /// </summary>
        private bool getEpisodeNumberFormat5(EPGEntry epgEntry, FreeSatEntry freeSatEntry, int index1)
        {
            string identifier1 = " Series ";

            index1 += identifier1.Length;

            if (index1 >= freeSatEntry.ShortDescription.Length)
                return (false);
            if (!char.IsDigit(freeSatEntry.ShortDescription[index1]))
                return (false);

            int seasonNumber = 0;

            while (index1 < freeSatEntry.ShortDescription.Length && char.IsDigit(freeSatEntry.ShortDescription[index1]))
            {
                seasonNumber = (seasonNumber * 10) + (freeSatEntry.ShortDescription[index1] - '0');
                index1++;
            }

            if (index1 == freeSatEntry.ShortDescription.Length)
                return (false);
            if (freeSatEntry.ShortDescription[index1] != ',')
                return (false);

            string identifier2 = " Episode ";

            if (index1 + identifier2.Length >= freeSatEntry.ShortDescription.Length)
                return (false);

            index1 += identifier2.Length + 1;

            int episodeNumber = 0;

            while (index1 < freeSatEntry.ShortDescription.Length && char.IsDigit(freeSatEntry.ShortDescription[index1]))
            {
                episodeNumber = (episodeNumber * 10) + (freeSatEntry.ShortDescription[index1] - '0');
                index1++;
            }

            if (index1 == freeSatEntry.ShortDescription.Length)
                return (false);
            if (freeSatEntry.ShortDescription[index1] != '.')
                return (false);

            epgEntry.SeasonNumber = seasonNumber;
            epgEntry.EpisodeNumber = episodeNumber;

            return (true);
        }

        private void setDate(EPGEntry epgEntry, char startChar, char endChar)
        {
            if (epgEntry.ShortDescription == null || epgEntry.ShortDescription.Length < 6)
                return;

            int index1 = 0;

            while (index1 < epgEntry.ShortDescription.Length)
            {
                index1 = epgEntry.ShortDescription.IndexOf(startChar, index1);
                if (index1 == -1)
                    return;

                index1++;

                bool isDate = true;
                int index2 = 0;

                for (; index2 < 4; index2++)
                {
                    if (index2 + index1 == epgEntry.ShortDescription.Length)
                        return;

                    if (!char.IsDigit(epgEntry.ShortDescription[index2 + index1]))
                        isDate = false;
                }

                if (index2 + index1 == epgEntry.ShortDescription.Length)
                    return;

                if (isDate)
                {
                    if (epgEntry.ShortDescription[index2 + index1] == endChar)
                    {
                        if (epgEntry.ShortDescription[index1] == '1' || epgEntry.ShortDescription[index1] == '2')
                        {
                            try
                            {
                                epgEntry.Date = epgEntry.ShortDescription.Substring(index1, 4);
                                if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
                                    epgEntry.ShortDescription = epgEntry.ShortDescription.Remove(index1 - 1, 6).Trim();
                            }
                            catch (ArgumentOutOfRangeException)
                            {
                                return;
                            }
                        }
                    }
                }
            }
        }

        private void logTitle(string title, EPGEntry epgEntry, Logger logger)
        {
            string episodeInfo;

            if (DebugEntry.IsDefined(DebugName.LogEpisodeInfo))
            {
                episodeInfo = (epgEntry.SeriesId == null ? "" : epgEntry.SeriesId) + ":" + 
                    (epgEntry.EpisodeId == null ? "" : epgEntry.EpisodeId);                
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

            Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "FREESAT Section: ONID: " + originalNetworkID +
                " TSID: " + transportStreamID +
                " SID: " + serviceID);
        }
    }
}
