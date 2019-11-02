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
    /// The class that defines an OpenTV channel.
    /// </summary>
    public class OpenTVChannel : Channel
    {
        /// <summary>
        /// Get or set the channel type.
        /// </summary>
        public int Type
        {
            get { return (type); }
            set { type = value; }
        }

        /// <summary>
        /// Get the title data for the channel.
        /// </summary>
        public Collection<OpenTVTitleData> TitleData
        {
            get
            {
                if (titleData == null)
                    titleData = new Collection<OpenTVTitleData>();
                return (titleData);
            }
        }

        /// <summary>
        /// Get the summary data for the channel.
        /// </summary>
        public Collection<OpenTVSummaryData> SummaryData
        {
            get
            {
                if (summaryData == null)
                    summaryData = new Collection<OpenTVSummaryData>();
                return (summaryData);
            }
        }

        /// <summary>
        /// Get the suspect program times for the channel.
        /// </summary>
        public Collection<OpenTVTitleData> SuspectTimeTitleData
        {
            get
            {
                if (suspectTimeTitleData == null)
                    suspectTimeTitleData = new Collection<OpenTVTitleData>();
                return (suspectTimeTitleData);
            }
        }

        /// <summary>
        /// Get the collection of OpenTV category records.
        /// </summary>
        public static Collection<CategoryEntry> CategoryEntries { get { return (categoryEntries); } }

        private int type;

        private int castStartIndex;
        private int castStopIndex;
                
        private Collection<OpenTVTitleData> titleData;
        private Collection<OpenTVSummaryData> summaryData;

        private Collection<OpenTVTitleData> suspectTimeTitleData;

        private static Collection<CategoryEntry> categoryEntries;

        /// <summary>
        /// Initialize a new instance of the OpenTVChannel class.
        /// </summary>
        public OpenTVChannel() { }

        /// <summary>
        /// Add title data to the channel.
        /// </summary>
        /// <param name="newTitleData">The title data to be added.</param>
        public void AddTitleData(OpenTVTitleData newTitleData)
        {
            if (!OptionEntry.IsDefined(OptionName.AcceptBreaks))
            {
                if (newTitleData.StartTime.Second != 0)
                {
                    addSuspectTimeEntry(newTitleData);
                    return;
                }
            }

            foreach (OpenTVTitleData oldTitleData in TitleData)
            {
               if (oldTitleData.StartTime == newTitleData.StartTime)
                    return;

                if (oldTitleData.StartTime > newTitleData.StartTime)
                {
                    TitleData.Insert(TitleData.IndexOf(oldTitleData), newTitleData);
                    return;
                }
            }

            TitleData.Add(newTitleData);
        }

        private void addSuspectTimeEntry(OpenTVTitleData newTitleData)
        {
            foreach (OpenTVTitleData oldTitleData in SuspectTimeTitleData)
            {
                if (oldTitleData.StartTime == newTitleData.StartTime)
                    return;

                if (oldTitleData.StartTime > newTitleData.StartTime)
                {
                    suspectTimeTitleData.Insert(SuspectTimeTitleData.IndexOf(oldTitleData), newTitleData);
                    return;
                }
            }

            SuspectTimeTitleData.Add(newTitleData);
        }

        /// <summary>
        /// Add summary data to the channel.
        /// </summary>
        /// <param name="newSummaryData"></param>
        public void AddSummaryData(OpenTVSummaryData newSummaryData)
        {
            foreach (OpenTVSummaryData oldSummaryData in SummaryData)
            {
                if (oldSummaryData.EventID == newSummaryData.EventID)
                    return;

                if (oldSummaryData.EventID > newSummaryData.EventID)
                {
                    SummaryData.Insert(SummaryData.IndexOf(oldSummaryData), newSummaryData);
                    return;
                }
            }

            SummaryData.Add(newSummaryData);
        }

        /// <summary>
        /// Add a channel to the collection.
        /// </summary>
        /// <param name="newChannel">The channel to be added.</param>
        public static void AddChannel(OpenTVChannel newChannel)
        {
            if (newChannel.Type == 0)
                return;

            bool process = checkChannelBouquet(newChannel);
            if (!process)
                return;

            if (TraceEntry.IsDefined(TraceName.AddChannel))
            {
                string flagsString;
                if (newChannel.Flags != null)
                    flagsString = Utils.ConvertToHex(newChannel.Flags);
                else
                    flagsString = "???";

                string bouquetName = BouquetAssociationSection.FindBouquetName(newChannel.BouquetID);
                if (bouquetName == null)
                    bouquetName = "** No Name **";

                Logger.Instance.Write("Adding channel: ONID " + newChannel.OriginalNetworkID +
                    " TSID " + newChannel.TransportStreamID +
                    " SID " + newChannel.ServiceID +
                    " Channel ID: " + newChannel.ChannelID +
                    " User Channel: " + newChannel.UserChannel +
                    " Type: " + newChannel.Type +
                    " Flags: " + flagsString +
                    " Bqt ID: " + newChannel.BouquetID +
                    " Bqt name: " + bouquetName +
                    " Region: " + newChannel.Region);
            }

            foreach (Channel channel in Channels)
            {
                OpenTVChannel oldChannel = channel as OpenTVChannel;

                if (oldChannel != null)
                {
                    if (oldChannel.OriginalNetworkID == newChannel.OriginalNetworkID &&
                        oldChannel.TransportStreamID == newChannel.TransportStreamID &&
                        oldChannel.ServiceID == newChannel.ServiceID &&
                        oldChannel.ChannelID == newChannel.ChannelID)
                    {
                        if (TraceEntry.IsDefined(TraceName.AddChannel))
                        {
                            string bouquetName = BouquetAssociationSection.FindBouquetName(oldChannel.BouquetID);
                            if (bouquetName == null)
                                bouquetName = "** No Name **";
                            Logger.Instance.Write("Already exists in bouquet " + bouquetName);
                        }
                        return;
                    }

                    if (oldChannel.OriginalNetworkID == newChannel.OriginalNetworkID)
                    {
                        if (oldChannel.TransportStreamID == newChannel.TransportStreamID)
                        {
                            if (oldChannel.ServiceID == newChannel.ServiceID)
                            {
                                if (oldChannel.ChannelID > newChannel.ChannelID)
                                {
                                    Channels.Insert(Channels.IndexOf(oldChannel), newChannel);
                                    return;
                                }
                            }
                            else
                            {
                                if (oldChannel.ServiceID > newChannel.ServiceID)
                                {
                                    Channels.Insert(Channels.IndexOf(oldChannel), newChannel);
                                    return;
                                }
                            }
                        }
                        else
                        {
                            if (oldChannel.TransportStreamID > newChannel.TransportStreamID)
                            {
                                Channels.Insert(Channels.IndexOf(oldChannel), newChannel);
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (oldChannel.OriginalNetworkID > newChannel.OriginalNetworkID)
                        {
                            Channels.Insert(Channels.IndexOf(oldChannel), newChannel);
                            return;
                        }
                    }
                }
            }

            Channels.Add(newChannel);
        }

        private static bool checkChannelBouquet(OpenTVChannel channel)
        {
            if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ChannelBouquet == -1)
                return (true);

            if (channel.BouquetID != RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ChannelBouquet)
                return (false);

            if (channel.Region == 65535 || channel.Region == RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ChannelRegion)
                return (true);

            return (false);
        }

        /// <summary>
        /// Create the EPG entries from the stored title and summary data.
        /// </summary>
        /// <param name="station">The station that the EPG records are for.</param>
        /// <param name="titleLogger">A Logger instance for the program titles.</param>
        /// <param name="descriptionLogger">A Logger instance for the program descriptions.</param>
        /// <param name="extendedDescriptionLogger">A Logger instance for the extended program descriptions.</param>
        /// <param name="undefinedRecordLogger">A Logger instance for the undefined records.</param>
        public void ProcessChannelForEPG(TVStation station, Logger titleLogger, Logger descriptionLogger, Logger extendedDescriptionLogger, Logger undefinedRecordLogger)
        {
            bool first = true;
            DateTime expectedStartTime = new DateTime(); 

            foreach (OpenTVTitleData titleData in TitleData)
            {
                EPGEntry epgEntry = new EPGEntry();
                epgEntry.OriginalNetworkID = OriginalNetworkID;
                epgEntry.TransportStreamID = TransportStreamID;
                epgEntry.ServiceID = ServiceID;
                epgEntry.EventID = titleData.EventID;
                epgEntry.StartTime = Utils.RoundTime(TimeOffsetEntry.GetAdjustedTime(titleData.StartTime));
                epgEntry.Duration = Utils.RoundTime(titleData.Duration);

                getEventName(epgEntry, titleData);
                OpenTVSummaryData summary = getShortDescription(epgEntry, titleData);

                getParentalRating(titleData, epgEntry);                
                getAspectRatio(titleData, epgEntry);
                getVideoQuality(titleData, epgEntry);
                getAudioQuality(titleData, epgEntry);
                getAudioDescription(titleData, epgEntry);
                getSubTitles(titleData, epgEntry);
                getEventCategory(titleData, epgEntry);
                
                getSeasonEpisode(epgEntry);
                getSeriesLink(epgEntry, summary);
                
                getExtendedRatings(epgEntry);
                getDirector(epgEntry);
                getCast(epgEntry);
                getDate(epgEntry);

                getSubTitle(epgEntry);

                epgEntry.EPGSource = EPGSource.OpenTV;
                epgEntry.PID = titleData.PID;
                epgEntry.Table = titleData.Table;
                epgEntry.TimeStamp = titleData.TimeStamp;

                epgEntry.UnknownData = titleData.Flags;

                bool include = ControllerBase.CheckEPGDays(epgEntry.StartTime);
                if (include)
                {

                    station.AddEPGEntry(epgEntry);

                    if (first)
                    {
                        expectedStartTime = new DateTime();
                        first = false;
                    }
                    else
                    {
                        if (epgEntry.StartTime < expectedStartTime)
                        {
                            if (titleLogger != null)
                                titleLogger.Write(" ** Overlap In Schedule **");
                        }
                        else
                        {
                            if (OptionEntry.IsDefined(OptionName.AcceptBreaks))
                            {
                                if (epgEntry.StartTime > expectedStartTime + new TimeSpan(0, 5, 0))
                                {
                                    if (titleLogger != null)
                                        titleLogger.Write(" ** Gap In Schedule **");
                                }
                            }
                            else
                            {
                                if (epgEntry.StartTime > expectedStartTime)
                                {
                                    if (titleLogger != null)
                                        titleLogger.Write(" ** Gap In Schedule **");
                                }
                            }
                        }
                    }

                    expectedStartTime = epgEntry.StartTime + epgEntry.Duration;

                    if (titleLogger != null)
                    {
                        string seriesLink = "No ";
                        if (summary != null && summary.SeriesLink != -1)
                            seriesLink = "0x" + summary.SeriesLink.ToString("X");

                        titleLogger.Write(epgEntry.OriginalNetworkID + ":" + epgEntry.TransportStreamID + ":" + epgEntry.ServiceID + " " +
                            epgEntry.StartTime.ToShortDateString() + " " +
                            epgEntry.StartTime.ToString("HH:mm") + " - " +
                            epgEntry.StartTime.Add(epgEntry.Duration).ToString("HH:mm") + " " +
                            " Cat ID " + titleData.CategoryID.ToString("000 ") +
                            " Flags " + Utils.ConvertToHex(titleData.Flags) +
                            " SLink " + seriesLink + " " +                            
                            titleData.EventName);

                        if (DebugEntry.IsDefined(DebugName.BitPattern))
                            titleLogger.Write("Bit pattern: " + Utils.ConvertToBits(titleData.EventNameBytes));
                    }

                    if (descriptionLogger != null && summary != null)
                    {
                        descriptionLogger.Write(epgEntry.OriginalNetworkID + ":" + epgEntry.TransportStreamID + ":" + epgEntry.ServiceID + " " +
                            epgEntry.StartTime.ToShortDateString() + " " +
                            epgEntry.StartTime.ToString("HH:mm") + " - " +
                            epgEntry.StartTime.Add(epgEntry.Duration).ToString("HH:mm") + " " +
                            summary.ShortDescription);

                        if (DebugEntry.IsDefined(DebugName.BitPattern))
                            descriptionLogger.Write("Bit pattern: " + Utils.ConvertToBits(summary.ShortDescriptionBytes));
                    }

                    if (extendedDescriptionLogger != null && summary != null)
                    {
                        string extendedDescription = summary.ExtendedDescription;
                        if (extendedDescription != null)
                            extendedDescriptionLogger.Write(epgEntry.OriginalNetworkID + ":" + epgEntry.TransportStreamID + ":" + epgEntry.ServiceID + " " +
                                epgEntry.StartTime.ToShortDateString() + " " +
                                epgEntry.StartTime.ToString("HH:mm") + " - " +
                                epgEntry.StartTime.Add(epgEntry.Duration).ToString("HH:mm") + " " +
                                extendedDescription);
                    }

                    if (undefinedRecordLogger != null)
                    {
                        Collection<OpenTVRecordBase> undefinedTitleRecords = titleData.UndefinedRecords;

                        if (undefinedTitleRecords != null)
                        {
                            foreach (OpenTVRecordBase record in undefinedTitleRecords)
                            {
                                if (record.Data != null)
                                    undefinedRecordLogger.Write("Title records: " + epgEntry.OriginalNetworkID + ":" + epgEntry.TransportStreamID + ":" + epgEntry.ServiceID + " " +
                                        epgEntry.StartTime.ToShortDateString() + " " +
                                        epgEntry.StartTime.ToString("HH:mm") + " - " +
                                        epgEntry.StartTime.Add(epgEntry.Duration).ToString("HH:mm") + " " +
                                        titleData.EventName +
                                        " Tag: " + record.Tag.ToString("X") +
                                        " Data: " + Utils.ConvertToHex(record.Data));
                                else
                                    undefinedRecordLogger.Write("Title records: " + epgEntry.OriginalNetworkID + ":" + epgEntry.TransportStreamID + ":" + epgEntry.ServiceID + " " +
                                        epgEntry.StartTime.ToShortDateString() + " " +
                                        epgEntry.StartTime.ToString("HH:mm") + " - " +
                                        epgEntry.StartTime.Add(epgEntry.Duration).ToString("HH:mm") + " " +
                                        titleData.EventName +
                                        " Tag: 0x" + record.Tag.ToString("X") +
                                        " Data: No data");
                            }
                        }

                        if (summary != null)
                        {
                            Collection<OpenTVRecordBase> undefinedSummaryRecords = summary.UndefinedRecords;

                            if (undefinedSummaryRecords != null)
                            {
                                foreach (OpenTVRecordBase record in undefinedSummaryRecords)
                                {
                                    if (record.Data != null)
                                        undefinedRecordLogger.Write("Summary records: " + epgEntry.OriginalNetworkID + ":" + epgEntry.TransportStreamID + ":" + epgEntry.ServiceID + " " +
                                            epgEntry.StartTime.ToShortDateString() + " " +
                                            epgEntry.StartTime.ToString("HH:mm") + " - " +
                                            epgEntry.StartTime.Add(epgEntry.Duration).ToString("HH:mm") +
                                            " Tag: " + record.Tag.ToString("X") +
                                            " Data: " + Utils.ConvertToHex(record.Data));
                                    else
                                        undefinedRecordLogger.Write("Summary records: " + epgEntry.OriginalNetworkID + ":" + epgEntry.TransportStreamID + ":" + epgEntry.ServiceID + " " +
                                            epgEntry.StartTime.ToShortDateString() + " " +
                                            epgEntry.StartTime.ToString("HH:mm") + " - " +
                                            epgEntry.StartTime.Add(epgEntry.Duration).ToString("HH:mm") +
                                            " Tag: ox" + record.Tag.ToString("X") +
                                            " Data: No data");
                                }
                            }
                        }
                    }

                    if (DebugEntry.IsDefined(DebugName.CatXref))
                        updateCategoryEntries(OriginalNetworkID, TransportStreamID, ServiceID, epgEntry.StartTime, epgEntry.EventName, titleData.CategoryID);


                    if (!OptionEntry.IsDefined(OptionName.AcceptBreaks))
                    {
                        if (epgEntry.StartTime.Second != 0)
                        {
                            if (titleLogger != null)
                                titleLogger.Write("** Suspect Start Time **");
      
                        }
                    }
                }
            }

            foreach (OpenTVTitleData titleData in SuspectTimeTitleData)
            {
                if (titleLogger != null)
                    titleLogger.Write("** Suspect time: " + titleData.StartTime + " " + titleData.EventName);
            }
        }

        private OpenTVSummaryData getShortDescription(EPGEntry epgEntry, OpenTVTitleData titleData)
        {
            OpenTVSummaryData summary = findSummary(titleData.EventID);
            if (summary == null)
                return (null);

            epgEntry.ShortDescription = summary.ShortDescription;

            if (epgEntry.EventName == null)
                return (summary);

            if (DebugEntry.IsDefined(DebugName.LogBrokenTitles))
            {
                if (epgEntry.EventName.EndsWith("..") && epgEntry.ShortDescription.StartsWith(".."))
                    Logger.Instance.Write("Broken title: " + epgEntry.EventName + "/" + epgEntry.ShortDescription);
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

            if (epgEntry.ShortDescription.StartsWith("..."))
                descriptionDotCount = 3;
            else
            {
                if (epgEntry.ShortDescription.StartsWith(".."))
                    descriptionDotCount = 2;
            }

            if (titleDotCount == 0 || descriptionDotCount == 0)
                return (summary);
            if (epgEntry.EventName.Length == titleDotCount || epgEntry.ShortDescription.Length == descriptionDotCount)
                return (summary);

            int fullStopIndex = epgEntry.ShortDescription.IndexOf('.', descriptionDotCount);
            int questionMarkIndex = epgEntry.ShortDescription.IndexOf('?', descriptionDotCount);
            int exclamationMarkIndex = epgEntry.ShortDescription.IndexOf('!', descriptionDotCount);
            int colonIndex = epgEntry.ShortDescription.IndexOf(':', descriptionDotCount);

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
                return (summary);

            epgEntry.EventName = epgEntry.EventName.Substring(0, epgEntry.EventName.Length - titleDotCount) + " " +
                epgEntry.ShortDescription.Substring(descriptionDotCount, (index + 1) - (descriptionDotCount + 1));

            epgEntry.ShortDescription = EditSpec.ProcessDescription(epgEntry.ShortDescription.Substring(index + 1).Trim());
            
            return (summary);
        }

        private OpenTVSummaryData findSummary(int eventID)
        {
            foreach (OpenTVSummaryData summary in SummaryData)
            {
                if (summary.EventID == eventID)
                    return (summary);

                if (summary.EventID > eventID)
                    return (null);
            }

            return (null);
        }

        private void getEventName(EPGEntry epgEntry, OpenTVTitleData titleData)
        {
            switch (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode)
            {
                case Country.NewZealand:
                    getSkyNZEventName(epgEntry, titleData);
                    break;
                default:
                    epgEntry.EventName = EditSpec.ProcessTitle(titleData.EventName);
                    break;
            }
        }

        private void getSkyNZEventName(EPGEntry epgEntry, OpenTVTitleData titleData)
        {
            string eventName = titleData.EventName;

            string editedEventName;

            if (!titleData.EventName.StartsWith("[["))
                editedEventName = eventName;
            else
            {
                int startIndex = eventName.IndexOf("]");

                if (startIndex != -1 && eventName[startIndex + 1] == ']' && startIndex + 2 < eventName.Length)
                    editedEventName = eventName.Substring(startIndex + 2);
                else
                    editedEventName = eventName;
            }

            if (editedEventName.EndsWith(" HD"))
                epgEntry.EventName = EditSpec.ProcessTitle(editedEventName.Substring(0, editedEventName.Length - 3));
            else
                epgEntry.EventName = EditSpec.ProcessTitle(editedEventName);
        }

        private void getParentalRating(OpenTVTitleData titleData, EPGEntry epgEntry)
        {
            if (titleData.Flags == null || titleData.Flags.Length < 2)
                return;

            epgEntry.ParentalRating = ParentalRating.FindRating(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode, "OPENTV", (titleData.Flags[1] & 0x0f).ToString());
            epgEntry.MpaaParentalRating = ParentalRating.FindMpaaRating(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode, "OPENTV", (titleData.Flags[1] & 0x0f).ToString());
            epgEntry.ParentalRatingSystem = ParentalRating.FindSystem(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode, "OPENTV", (titleData.Flags[1] & 0x0f).ToString());
        }

        private void getAspectRatio(OpenTVTitleData titleData, EPGEntry epgEntry)
        {
            if (titleData.Flags == null || titleData.Flags.Length < 2)
                return;

            if ((titleData.Flags[0] & 0x08) != 0)
            {
                epgEntry.AspectRatio = "16:9";

                if (!OptionEntry.IsDefined(OptionName.NoRemoveData) && !string.IsNullOrWhiteSpace(epgEntry.ShortDescription))
                    epgEntry.ShortDescription = epgEntry.ShortDescription.Replace("(WS)", "").Trim();
            }
        }

        private void getVideoQuality(OpenTVTitleData titleData, EPGEntry epgEntry)
        {            
            if (titleData.Flags == null || titleData.Flags.Length < 2)
                return;

            if ((titleData.Flags[0] & 0x04) != 0)
            {
                epgEntry.VideoQuality = "HDTV";

                if (!OptionEntry.IsDefined(OptionName.NoRemoveData) && !string.IsNullOrWhiteSpace(epgEntry.ShortDescription))
                {
                    if (epgEntry.ShortDescription.EndsWith(" HD."))
                        epgEntry.ShortDescription = epgEntry.ShortDescription.Replace(" HD.", "").Trim();
                    if (epgEntry.ShortDescription.EndsWith(" HD"))
                        epgEntry.ShortDescription = epgEntry.ShortDescription.Replace(" HD", "").Trim();
                }
            }
        }

        private void getAudioQuality(OpenTVTitleData titleData, EPGEntry epgEntry)
        {
            if (titleData.Flags == null || titleData.Flags.Length < 2)
                return;

            switch (titleData.Flags[0] >> 6)
            {
                case 1:
                    epgEntry.AudioQuality = "stereo";
                    break;
                case 2:
                    epgEntry.AudioQuality = "surround";
                    break;
                case 3:
                    epgEntry.AudioQuality = "dolby digital";
                    break;
                default:
                    break;;
            }
        }

        private void getAudioDescription(OpenTVTitleData titleData, EPGEntry epgEntry)
        {
            if (titleData.Flags == null || titleData.Flags.Length < 2)
                return;

            epgEntry.HasAudioDescription = ((titleData.Flags[0] & 0x01) != 0);            
        }

        private void getSubTitles(OpenTVTitleData titleData, EPGEntry epgEntry)
        {
            if (titleData.Flags == null || titleData.Flags.Length < 2)
                return;

            if ((titleData.Flags[0] & 0x10) != 0 || (titleData.Flags[0] & 0x20) != 0)
                epgEntry.SubTitles = "teletext";            
        }

        private void getEventCategory(OpenTVTitleData titleData, EPGEntry epgEntry)
        {
            if (titleData.CategoryID == 0)
            {
                getCustomCategory(epgEntry.EventName, epgEntry.ShortDescription);
                return;
            }

            if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options,
                OptionName.CustomCategoryOverride))
            {
                epgEntry.EventCategory = getCustomCategory(epgEntry.EventName, epgEntry.ShortDescription);
                if (epgEntry.EventCategory != null)
                    return;
            }

            OpenTVProgramCategory category = OpenTVProgramCategory.FindCategory(titleData.CategoryID);
            if (category != null)
            {
                epgEntry.EventCategory = category.Description;                
                category.UsedCount++;
                category.SetTraceData(epgEntry.FullScheduleDescription);
                return;
            }
            else
                OpenTVProgramCategory.AddUndefinedCategory(titleData.CategoryID, epgEntry.FullScheduleDescription);

            if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options,
                OptionName.CustomCategoryOverride))
                return;

            epgEntry.EventCategory = getCustomCategory(epgEntry.EventName, epgEntry.ShortDescription);
        }

        private string getCustomCategory(string title, string description)
        {
            string category = CustomProgramCategory.FindCategoryDescription(title);
            if (category != null)
                return (category);

            return (CustomProgramCategory.FindCategoryDescription(description));
        }

        private void getSeasonEpisode(EPGEntry epgEntry)
        {
            if (string.IsNullOrWhiteSpace(epgEntry.ShortDescription))
                return;

            switch (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode)
            {
                case Country.Australia:
                    getSkyAUSSeasonEpisode(epgEntry);
                    break;
                case Country.UnitedKingdom:
                    getSkyGBRSeasonEpisode(epgEntry);
                    break;
                case Country.Italy:
                    getSkyITASeasonEpisode(epgEntry);
                    break;
                case Country.NewZealand:
                    getSkyNZLSeasonEpisode(epgEntry);
                    break;
                default:
                    break;
            }
        }

        private void getSkyAUSSeasonEpisode(EPGEntry epgEntry)
        {
            int index1 = epgEntry.ShortDescription.IndexOf(" Ep");
            if (index1 == -1)
                return;
            if (index1 + 3 == epgEntry.ShortDescription.Length)
                return;
            if (index1 < 2)
                return;
            if (!Char.IsDigit(epgEntry.ShortDescription[index1 + 3]))
                return;
            if (epgEntry.ShortDescription[index1 - 1] != ',')
                return;
            if (!Char.IsDigit(epgEntry.ShortDescription[index1 - 2]))
                return;

            int index2 = index1 - 2;

            while (epgEntry.ShortDescription[index2] != 'S')
                index2--;

            int index3 = index1 + 3;

            while (index3 < epgEntry.ShortDescription.Length && Char.IsDigit(epgEntry.ShortDescription[index3]))
                index3++;

            int series = 0;
            int index4 = index2 + 1;
            while (epgEntry.ShortDescription[index4] != ',')
            {
                series = (series * 10) + (epgEntry.ShortDescription[index4] - '0');
                index4++;
            }

            int episode = 0;
            int index5 = index1 + 3;
            while (index5 < index3)
            {
                episode = (episode * 10) + (epgEntry.ShortDescription[index5] - '0');
                index5++;
            }

            epgEntry.SeasonNumber = series;
            epgEntry.EpisodeNumber = episode;

            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
                epgEntry.ShortDescription = epgEntry.ShortDescription.Remove(index2, index5 - index2 + 1).Trim();
        }

        private void getSkyGBRSeasonEpisode(EPGEntry epgEntry)
        {
            try
            {
                int index1 = epgEntry.ShortDescription.IndexOf(", Ep. ");
                if (index1 != -1)
                {
                    getSkyGBREpisodeFormat1(epgEntry, index1);
                    return;
                }

                index1 = epgEntry.ShortDescription.IndexOf(", ep ");
                if (index1 != -1)
                {
                    getSkyGBREpisodeFormat2(epgEntry, index1);
                    return;
                }

                index1 = epgEntry.ShortDescription.IndexOf(", Episode ");
                if (index1 != -1)
                {
                    getSkyGBREpisodeFormat3(epgEntry, index1);
                    return;
                }

                index1 = epgEntry.ShortDescription.IndexOf(", Ep ");
                if (index1 != -1)
                {
                    getSkyGBREpisodeFormat4(epgEntry, index1);
                    return;
                }

                index1 = epgEntry.ShortDescription.IndexOf(", ep");
                if (index1 != -1)
                {
                    getSkyGBREpisodeFormat5(epgEntry, index1);
                    return;
                }

                index1 = epgEntry.ShortDescription.IndexOf(", Ep");
                if (index1 != -1)
                {
                    getSkyGBREpisodeFormat6(epgEntry, index1);
                    return;
                }

                getSkyGBREpisodeOnly(epgEntry);
            }
            catch (IndexOutOfRangeException) { }
        }

        private void getSkyGBREpisodeFormat1(EPGEntry epgEntry, int index1)
        {
            if (index1 < 3)
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 1: " + epgEntry.ShortDescription + "= failed");
                return;
            }

            int endOffset = 6;

            if (index1 + endOffset >= epgEntry.ShortDescription.Length)
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 1: " + epgEntry.ShortDescription + "= failed");
                return;
            }
            if (!Char.IsDigit(epgEntry.ShortDescription[index1 + endOffset]))
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 1: " + epgEntry.ShortDescription + "= failed");
                return;
            }
            if (!Char.IsDigit(epgEntry.ShortDescription[index1 - 1]))
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 1: " + epgEntry.ShortDescription + "= failed");
                return;
            }

            int index2 = index1 - 1;

            while (epgEntry.ShortDescription[index2] != 'S')
                index2--;

            char endChar = ')';

            if (epgEntry.ShortDescription[index2 - 1] != '(')
            {
                if (epgEntry.ShortDescription[index2 - 1] == ' ')
                    endChar = '.';
                else
                {
                    if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                        Logger.Instance.Write("Format 1: " + epgEntry.ShortDescription + "= failed");
                    return;
                }
            }

            int index3 = index1 + endOffset;

            while (Char.IsDigit(epgEntry.ShortDescription[index3]))
                index3++;

            if (epgEntry.ShortDescription[index3] != endChar)
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 1: " + epgEntry.ShortDescription + "= failed");
                return;
            }

            int series = 0;
            int index4 = index2 + 1;
            while (epgEntry.ShortDescription[index4] != ',')
            {
                series = (series * 10) + (epgEntry.ShortDescription[index4] - '0');
                index4++;
            }

            int episode = 0;
            int index5 = index1 + endOffset;
            while (index5 < index3)
            {
                episode = (episode * 10) + (epgEntry.ShortDescription[index5] - '0');
                index5++;
            }

            setSeriesEpisode(epgEntry, series, episode);

            if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                Logger.Instance.Write("Format 1: " + epgEntry.ShortDescription.Substring(index2 - 1, index5 - index2 + 2) + "= season " + series + " episode " + episode);

            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
                epgEntry.ShortDescription = epgEntry.ShortDescription.Remove(index2 - 1, index5 - index2 + 2).Trim();
        }

        private void getSkyGBREpisodeFormat2(EPGEntry epgEntry, int index1)
        {
            if (index1 < 3)
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 2: " + epgEntry.ShortDescription + "= failed");
                return;
            }

            int endOffset = 5;

            if (index1 + endOffset >= epgEntry.ShortDescription.Length)
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 2: " + epgEntry.ShortDescription + "= failed");
                return;
            }
            if (!Char.IsDigit(epgEntry.ShortDescription[index1 + endOffset]))
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 2: " + epgEntry.ShortDescription + "= failed");
                return;
            }
            if (!Char.IsDigit(epgEntry.ShortDescription[index1 - 1]))
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 2: " + epgEntry.ShortDescription + "= failed");
                return;
            }

            int index2 = index1 - 1;

            while (epgEntry.ShortDescription[index2] != 'S')
                index2--;

            if (epgEntry.ShortDescription[index2 - 1] != '(')
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 2: " + epgEntry.ShortDescription + "= failed");
                return;
            }

            int index3 = index1 + endOffset;

            while (Char.IsDigit(epgEntry.ShortDescription[index3]))
                index3++;

            if (epgEntry.ShortDescription[index3] != ')')
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 2: " + epgEntry.ShortDescription + "= failed");
                return;
            }

            int series = 0;
            int index4 = index2 + 1;
            while (epgEntry.ShortDescription[index4] != ',')
            {
                series = (series * 10) + (epgEntry.ShortDescription[index4] - '0');
                index4++;
            }

            int episode = 0;
            int index5 = index1 + endOffset;
            while (index5 < index3)
            {
                episode = (episode * 10) + (epgEntry.ShortDescription[index5] - '0');
                index5++;
            }

            setSeriesEpisode(epgEntry, series, episode);

            if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                Logger.Instance.Write("Format 2: " + epgEntry.ShortDescription.Substring(index2 - 1, index5 - index2 + 2) + "= season " + series + " episode " + episode);

            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
                epgEntry.ShortDescription = epgEntry.ShortDescription.Remove(index2 - 1, index5 - index2 + 2).Trim();
        }

        private void getSkyGBREpisodeFormat3(EPGEntry epgEntry, int index1)
        {
            if (index1 < 8)
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 3: " + epgEntry.ShortDescription + "= failed");
                return;
            }

            int endOffset = 10;

            if (index1 + endOffset >= epgEntry.ShortDescription.Length)
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 3: " + epgEntry.ShortDescription + "= failed 1");
                return;
            }
            if (!Char.IsDigit(epgEntry.ShortDescription[index1 + endOffset]))
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 3: " + epgEntry.ShortDescription + "= failed 2");
                return;
            }
            if (!Char.IsDigit(epgEntry.ShortDescription[index1 - 1]))
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 3: " + epgEntry.ShortDescription + "= failed 3");
                return;
            }

            int index2 = index1 - 1;

            while (epgEntry.ShortDescription[index2] != 'S')
                index2--;

            if (epgEntry.ShortDescription.Substring(index2, 7) != "Series " && epgEntry.ShortDescription.Substring(index2, 7) != "Season ")
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 3: " + epgEntry.ShortDescription + "= failed 4");
                return;
            }

            int index3 = index1 + endOffset;

            while (Char.IsDigit(epgEntry.ShortDescription[index3]))
                index3++;

            if (epgEntry.ShortDescription[index3] != '.' &&
                epgEntry.ShortDescription[index3] != ',' &&
                epgEntry.ShortDescription[index3] != ':' && 
                epgEntry.ShortDescription[index3] != ' ')
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 3: " + epgEntry.ShortDescription + "= failed 5");
                return;
            }

            int series = 0;
            int index4 = index2 + 7;
            while (epgEntry.ShortDescription[index4] != ',')
            {
                series = (series * 10) + (epgEntry.ShortDescription[index4] - '0');
                index4++;
            }

            int episode = 0;
            int index5 = index1 + endOffset;
            while (index5 < index3)
            {
                episode = (episode * 10) + (epgEntry.ShortDescription[index5] - '0');
                index5++;
            }

            setSeriesEpisode(epgEntry, series, episode);

            if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                Logger.Instance.Write("Format 3: " + epgEntry.ShortDescription.Substring(index2, index5 - index2 + 1) + "= season " + series + " episode " + episode);

            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
                epgEntry.ShortDescription = epgEntry.ShortDescription.Remove(index2, index5 - index2 + 1).Trim();
        }

        private void getSkyGBREpisodeFormat4(EPGEntry epgEntry, int index1)
        {
            if (index1 < 2)
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 4: " + epgEntry.ShortDescription + "= failed");
                return;
            }

            int endOffset = 5;

            if (index1 + endOffset >= epgEntry.ShortDescription.Length)
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 4: " + epgEntry.ShortDescription + "= failed");
                return;
            }
            if (!Char.IsDigit(epgEntry.ShortDescription[index1 + endOffset]))
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 4: " + epgEntry.ShortDescription + "= failed");
                return;
            }
            if (!Char.IsDigit(epgEntry.ShortDescription[index1 - 1]))
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 4: " + epgEntry.ShortDescription + "= failed");
                return;
            }

            int index2 = index1 - 1;

            while (epgEntry.ShortDescription[index2] != 'S')
                index2--;

            int index3 = index1 + endOffset;

            while (index3 < epgEntry.ShortDescription.Length && Char.IsDigit(epgEntry.ShortDescription[index3]))
                index3++;

            int series = 0;
            int index4 = index2 + 1;
            while (epgEntry.ShortDescription[index4] != ',')
            {
                series = (series * 10) + (epgEntry.ShortDescription[index4] - '0');
                index4++;
            }

            int episode = 0;
            int index5 = index1 + endOffset;
            while (index5 < index3)
            {
                episode = (episode * 10) + (epgEntry.ShortDescription[index5] - '0');
                index5++;
            }

            setSeriesEpisode(epgEntry, series, episode);

            if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                Logger.Instance.Write("Format 4: " + epgEntry.ShortDescription.Substring(index2, index5 - index2) + "= season " + series + " episode " + episode);

            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
            {
                int length = index5 - index2;
                if (index2 + length + 1 < epgEntry.ShortDescription.Length)
                    epgEntry.ShortDescription = epgEntry.ShortDescription.Remove(index2, length + 1).Trim();
                else
                    epgEntry.ShortDescription = epgEntry.ShortDescription.Remove(index2, length).Trim();
            }
        }

        private void getSkyGBREpisodeFormat5(EPGEntry epgEntry, int index1)
        {
            if (index1 < 2)
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 5: " + epgEntry.ShortDescription + "= failed");
                return;
            }

            int endOffset = 4;

            if (index1 + endOffset >= epgEntry.ShortDescription.Length)
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 5: " + epgEntry.ShortDescription + "= failed");
                return;
            }
            if (!Char.IsDigit(epgEntry.ShortDescription[index1 + endOffset]))
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 5: " + epgEntry.ShortDescription + "= failed");
                return;
            }
            if (!Char.IsDigit(epgEntry.ShortDescription[index1 - 1]))
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 5: " + epgEntry.ShortDescription + "= failed");
                return;
            }

            int index2 = index1 - 1;

            while (epgEntry.ShortDescription[index2] != 'S')
                index2--;

            int index3 = index1 + endOffset;

            while (Char.IsDigit(epgEntry.ShortDescription[index3]))
                index3++;

            int series = 0;
            int index4 = index2 + 1;
            while (epgEntry.ShortDescription[index4] != ',')
            {
                series = (series * 10) + (epgEntry.ShortDescription[index4] - '0');
                index4++;
            }

            int episode = 0;
            int index5 = index1 + endOffset;
            while (index5 < index3)
            {
                episode = (episode * 10) + (epgEntry.ShortDescription[index5] - '0');
                index5++;
            }

            setSeriesEpisode(epgEntry, series, episode);

            if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                Logger.Instance.Write("Format 5: " + epgEntry.ShortDescription.Substring(index2) + "= season " + series + " episode " + episode);

            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
                epgEntry.ShortDescription = epgEntry.ShortDescription.Remove(index2).Trim();
        }

        private void getSkyGBREpisodeFormat6(EPGEntry epgEntry, int index1)
        {
            if (index1 < 2)
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 6: " + epgEntry.ShortDescription + "= failed");
                return;
            }

            int endOffset = 4;

            if (index1 + endOffset >= epgEntry.ShortDescription.Length)
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 6: " + epgEntry.ShortDescription + "= failed");
                return;
            }
            if (!Char.IsDigit(epgEntry.ShortDescription[index1 + endOffset]))
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 6: " + epgEntry.ShortDescription + "= failed");
                return;
            }
            if (!Char.IsDigit(epgEntry.ShortDescription[index1 - 1]))
            {
                if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                    Logger.Instance.Write("Format 6: " + epgEntry.ShortDescription + "= failed");
                return;
            }

            int index2 = index1 - 1;

            while (epgEntry.ShortDescription[index2] != 'S')
                index2--;

            int index3 = index1 + endOffset;

            while (index3 < epgEntry.ShortDescription.Length && Char.IsDigit(epgEntry.ShortDescription[index3]))
                index3++;

            int series = 0;
            int index4 = index2 + 1;
            while (epgEntry.ShortDescription[index4] != ',')
            {
                series = (series * 10) + (epgEntry.ShortDescription[index4] - '0');
                index4++;
            }

            int episode = 0;
            int index5 = index1 + endOffset;
            while (index5 < index3)
            {
                episode = (episode * 10) + (epgEntry.ShortDescription[index5] - '0');
                index5++;
            }

            setSeriesEpisode(epgEntry, series, episode);

            if (DebugEntry.IsDefined(DebugName.EpisodeResult))
                Logger.Instance.Write("Format 6: " + epgEntry.ShortDescription.Substring(index2, index5 - index2) + "= season " + series + " episode " + episode);

            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
            {
                int length = index5 - index2;
                if (index2 + length + 1 < epgEntry.ShortDescription.Length)
                    epgEntry.ShortDescription = epgEntry.ShortDescription.Remove(index2, length + 1).Trim();
                else
                    epgEntry.ShortDescription = epgEntry.ShortDescription.Remove(index2, length).Trim();
            }
        }

        private void setSeriesEpisode(EPGEntry epgEntry, int series, int episode)
        {
            epgEntry.SeasonNumber = series;
            epgEntry.EpisodeNumber = episode;
        }

        private void getSkyGBREpisodeOnly(EPGEntry epgEntry)
        {
            int index1 = epgEntry.ShortDescription.IndexOf("/");
            if (index1 == -1)
                return;

            if (index1 == 0 || !Char.IsDigit(epgEntry.ShortDescription[index1 - 1]))
                return;

            if (index1 + 1 == epgEntry.ShortDescription.Length || !Char.IsDigit(epgEntry.ShortDescription[index1 + 1]))
                return;

            int episodeNumber = 0;
            int index2 = index1 - 1;
            int multiplier = 1;

            while (index2 > -1 && Char.IsDigit(epgEntry.ShortDescription[index2]))
            {
                episodeNumber = episodeNumber + ((epgEntry.ShortDescription[index2] - '0') * multiplier);
                multiplier *= 10;
                index2--;
            }

            int episodeCount = 0;
            int index3 = index1 + 1;

            while (index3 < epgEntry.ShortDescription.Length && Char.IsDigit(epgEntry.ShortDescription[index3]))
            {
                episodeCount = (episodeCount * 10) + (epgEntry.ShortDescription[index3] - '0');
                index3++;
            }

            if (episodeNumber <= episodeCount)
            {
                epgEntry.EpisodeId = episodeNumber.ToString(); 
                epgEntry.EpisodeNumber = episodeNumber;
            }
        }

        private void getSkyITASeasonEpisode(EPGEntry epgEntry)
        {
            epgEntry.SeasonNumber = getSkyITASeries(epgEntry);
            epgEntry.EpisodeNumber = getSkyITAEpisode(epgEntry);

            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
            {
                if (epgEntry.ShortDescription.StartsWith("- "))
                    epgEntry.ShortDescription = epgEntry.ShortDescription.Substring(2).Trim();
            }
        }

        private int getSkyITASeries(EPGEntry epgEntry)
        {
            string identity = "' Stagione ";

            int index = epgEntry.ShortDescription.IndexOf(identity);
            if (index == -1)
                return (-1);

            try
            {
                int series = Int32.Parse(epgEntry.ShortDescription.Substring(0, index));

                if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
                    epgEntry.ShortDescription = epgEntry.ShortDescription.Remove(0, index + identity.Length).Trim();

                return (series);
            }
            catch (FormatException) { return (-1); }
            catch (OverflowException) { return (-1); }
        }

        private int getSkyITAEpisode(EPGEntry epgEntry)
        {
            if (epgEntry.ShortDescription.StartsWith("- "))
                return (-1);

            int index1 = epgEntry.ShortDescription.IndexOf("Ep.");
            if (index1 == -1)
                return (-1);

            int index2 = index1 + 3;

            while (epgEntry.ShortDescription[index2] == ' ')
                index2++;

            int episode = 0;

            while (index2 < epgEntry.ShortDescription.Length && epgEntry.ShortDescription[index2] != ' ')
            {
                episode = (episode * 10) + (epgEntry.ShortDescription[index2] - '0');
                index2++;
            }

            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
                epgEntry.ShortDescription = epgEntry.ShortDescription.Remove(index1, index2 - index1).Trim();

            return (episode);
        }

        private void getSkyNZLSeasonEpisode(EPGEntry epgEntry)
        {
            if (!epgEntry.ShortDescription.StartsWith("S"))
                return;

            string[] parts = epgEntry.ShortDescription.Split(new char[] { ' ' });
            if (parts.Length < 2 || (!parts[1].StartsWith("E") && !parts[1].StartsWith("Ep")))
                return;

            if (parts[0].Length < 2 || parts[1].Length < 2)
                return;

            try
            {
                int series = Int32.Parse(parts[0].Substring(1));
                int episode;

                if (parts[1].StartsWith("Ep"))
                    episode = Int32.Parse(parts[1].Substring(2));
                else
                    episode = Int32.Parse(parts[1].Substring(1));

                epgEntry.SeasonNumber = series;
                epgEntry.EpisodeNumber = episode;

                if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
                    epgEntry.ShortDescription = epgEntry.ShortDescription.Substring(parts[0].Length + parts[1].Length + 2);
            }
            catch (FormatException) { }
            catch (OverflowException) { }
        }

        private void getSeriesLink(EPGEntry epgEntry, OpenTVSummaryData summaryData)
        {
            if (summaryData == null || summaryData.SeriesLink == -1)
                return;

            epgEntry.SeriesId = summaryData.SeriesLink.ToString();
        }

        private void 
            getExtendedRatings(EPGEntry epgEntry)
        {
            if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode != Country.Australia)
                return;

            if (string.IsNullOrWhiteSpace(epgEntry.ShortDescription))
                return;

            int startIndex = epgEntry.ShortDescription.IndexOf("(");

            while (startIndex != -1 && startIndex + 1 < epgEntry.ShortDescription.Length)
            {
                if (epgEntry.ShortDescription[startIndex + 1] >= 'a')
                {
                    int endIndex = epgEntry.ShortDescription.IndexOf(")", startIndex);
                    if (endIndex != -1)
                    {
                        string ratingString = epgEntry.ShortDescription.Substring(startIndex + 1, endIndex - (startIndex + 1));
                        Collection<string> ratings = new Collection<string>(ratingString.Split(new char[] { ',' }));

                        bool isRatingField = true;

                        foreach (string rating in ratings)
                        {
                            if (rating.Length != 1)
                                isRatingField = false;
                        }

                        if (isRatingField)
                        {
                            epgEntry.HasGraphicViolence = ratings.Contains("v");
                            epgEntry.HasGraphicLanguage = ratings.Contains("l");
                            epgEntry.HasStrongSexualContent = ratings.Contains("s");
                            epgEntry.HasAdult = ratings.Contains("a");
                            epgEntry.HasNudity = ratings.Contains("n");

                            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
                                epgEntry.ShortDescription = epgEntry.ShortDescription.Remove(startIndex, endIndex - startIndex + 1).Trim();

                            return;
                        }
                        else
                            startIndex = epgEntry.ShortDescription.IndexOf("(", startIndex + 1);
                    }
                    else
                        return;

                }
                else
                    startIndex = epgEntry.ShortDescription.IndexOf("(", startIndex + 1);
            }
        }

        private void getDirector(EPGEntry epgEntry)
        {
            switch (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode)
            {
                case Country.NewZealand:
                    getSkyNZLDirector(epgEntry);
                    break;
                default:
                    break;
            }
        }

        private void getSkyNZLDirector(EPGEntry epgEntry)
        {
            if (string.IsNullOrWhiteSpace(epgEntry.ShortDescription))
                return;

            string searchString = "Director:";
           
            int startIndex = epgEntry.ShortDescription.IndexOf(searchString);
            if (startIndex == -1)
            {
                searchString = "Dir:";
                startIndex = epgEntry.ShortDescription.IndexOf(searchString);
                if (startIndex == -1)
                    return;
            }

            int stopIndex = startIndex;

            while (stopIndex < epgEntry.ShortDescription.Length &&
                epgEntry.ShortDescription[stopIndex] != '.' &&
                epgEntry.ShortDescription[stopIndex] != '(' &&
                (epgEntry.ShortDescription[stopIndex] < '0' || epgEntry.ShortDescription[stopIndex] > '9'))
                stopIndex++;

            epgEntry.Directors = new Collection<string>();
            epgEntry.Directors.Add(epgEntry.ShortDescription.Substring(startIndex + searchString.Length, stopIndex - (startIndex + searchString.Length)));
            
            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
            {
                if (stopIndex == epgEntry.ShortDescription.Length)
                    epgEntry.ShortDescription = epgEntry.ShortDescription.Remove(startIndex, stopIndex - startIndex).Trim();
                else
                {
                    if (epgEntry.ShortDescription[stopIndex] != '.')
                        epgEntry.ShortDescription = epgEntry.ShortDescription.Remove(startIndex, stopIndex - startIndex).Trim();
                    else
                        epgEntry.ShortDescription = epgEntry.ShortDescription.Remove(startIndex, stopIndex - startIndex + 1).Trim();
                }
            }
        }

        private void getCast(EPGEntry epgEntry)
        {
            switch (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode)
            {
                case Country.NewZealand:
                    getSkyNZLCast(epgEntry);
                    break;
                default:
                    break;
            }
        }

        private void getSkyNZLCast(EPGEntry epgEntry)
        {
            if (string.IsNullOrWhiteSpace(epgEntry.ShortDescription))
                return;

            string searchText = "Starring:";

            castStartIndex = epgEntry.ShortDescription.IndexOf(searchText);
            if (castStartIndex == -1)
            {
                searchText = "Guest starring:";
                castStartIndex = epgEntry.ShortDescription.IndexOf(searchText);
                if (castStartIndex == -1)
                {
                    searchText = "Voice of:";
                    castStartIndex = epgEntry.ShortDescription.IndexOf(searchText);
                    if (castStartIndex == -1)
                    {
                        searchText = "Voices of:";
                        castStartIndex = epgEntry.ShortDescription.IndexOf(searchText);
                        if (castStartIndex == -1)
                            return;
                    }
                }                
            }

            epgEntry.Cast = new Collection<string>();

            int startIndex = castStartIndex + searchText.Length;

            while (startIndex != -1)
                startIndex = addCastName(epgEntry.Cast, epgEntry.ShortDescription, startIndex);

            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
            {
                epgEntry.ShortDescription = epgEntry.ShortDescription.Remove(castStartIndex, castStopIndex - castStartIndex).Trim();
                if (epgEntry.ShortDescription.EndsWith(" ."))
                    epgEntry.ShortDescription = epgEntry.ShortDescription.Remove(epgEntry.ShortDescription.Length - 2);
            }
        }

        private int addCastName(Collection<string> cast, string description, int startIndex)
        {
            castStopIndex = startIndex;

            while (castStopIndex < description.Length &&
                description[castStopIndex] != '.' &&
                description[castStopIndex] != ',' &&
                description[castStopIndex] != '(')
                castStopIndex++;

            bool done = (castStopIndex == description.Length || description[castStopIndex] == '.' || description[castStopIndex] == '(');

            cast.Add(description.Substring(startIndex, castStopIndex - startIndex).Trim());

            if (!done)
                return (castStopIndex + 1);
            else
                return (-1);
        }

        private void getDate(EPGEntry epgEntry)
        {
            if (string.IsNullOrWhiteSpace(epgEntry.ShortDescription))
                return;

            extractDate(epgEntry, '(', ')');
            if (epgEntry.Date != null)
                return;

            extractDate(epgEntry, '[', ']');
        }

        private void extractDate(EPGEntry epgEntry, char startChar, char endChar)
        {
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

                    if (epgEntry.ShortDescription[index2 + index1] < '0' || epgEntry.ShortDescription[index2 + index1] > '9')
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

        private void getSubTitle(EPGEntry epgEntry)
        {
            if (string.IsNullOrWhiteSpace(epgEntry.ShortDescription))
                return;

            switch (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode)
            {
                case Country.Australia:
                case Country.UnitedKingdom:
                    getSkyAUSGBRSubTitle(epgEntry);
                    break;
                case Country.Italy:
                    getSkyITASubTitle(epgEntry);
                    break;
                default:
                    break;
            }
        }

        private void getSkyAUSGBRSubTitle(EPGEntry epgEntry)
        {
            if (DebugEntry.IsDefined(DebugName.ShowColons))
            {
                int firstIndex = epgEntry.ShortDescription.IndexOf(":");
                int lastIndex = epgEntry.ShortDescription.LastIndexOf(":");                
                if (firstIndex != lastIndex)
                    Logger.Instance.Write("Multiple colons (" + lastIndex + ") " + epgEntry.ShortDescription);
            }

            int colonIndex = epgEntry.ShortDescription.IndexOf(":");
            if (colonIndex != -1)
            {
                epgEntry.EventSubTitle = epgEntry.ShortDescription.Substring(0, colonIndex).Trim();

                if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
                    epgEntry.ShortDescription = epgEntry.ShortDescription.Substring(colonIndex + 1).Trim();
            }
        }

        private void getSkyITASubTitle(EPGEntry epgEntry)
        {
            if (!epgEntry.ShortDescription.StartsWith("'"))
                return;

            int index = epgEntry.ShortDescription.IndexOf("' ", 1);
            if (index == -1)
                return;

            epgEntry.EventSubTitle = epgEntry.ShortDescription.Substring(1, index - 1).Trim();
            
            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
                epgEntry.ShortDescription = epgEntry.ShortDescription.Substring(index + 2);
        }

        private void updateCategoryEntries(int networkID, int transportStreamID, int serviceID,  DateTime startTime, string eventName, int category)
        {
            if (categoryEntries == null)
                categoryEntries = new Collection<CategoryEntry>();

            CategoryEntry newEntry = new CategoryEntry(networkID, transportStreamID, serviceID, startTime, eventName, category);

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
        /// Get the count of channels for a bouquet.
        /// </summary>
        /// <param name="bouquetID">The bouquet ID.</param>
        /// <returns>The number of channels defined for the bouquet.</returns>
        public static int ChannelCountByBouquet(int bouquetID)
        {
            int count = 0;

            foreach (OpenTVChannel channel in Channels)
            {
                if (channel.BouquetID == bouquetID)
                    count++;
            }

            return (count);

        }
    }
}
