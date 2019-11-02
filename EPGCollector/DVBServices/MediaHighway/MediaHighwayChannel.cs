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
using System.Text;

using DomainObjects;

namespace DVBServices
{
    /// <summary>
    /// The class that defines a MediaHighway channel.
    /// </summary>
    public class MediaHighwayChannel : Channel
    {
        /// <summary>
        /// Get or set the channel name.
        /// </summary>
        public string ChannelName
        {
            get { return (channelName); }
            set { channelName = value; }
        }

        /// <summary>
        /// Get or set the unknown data.
        /// </summary>
        public byte[] Unknown
        {
            get { return (unknown); }
            set { unknown = value; }
        }

        /// <summary>
        /// Get the title data for the channel.
        /// </summary>
        public Collection<MediaHighwayTitle> Titles
        {
            get
            {
                if (titles == null)
                    titles = new Collection<MediaHighwayTitle>();
                return (titles);
            }
        }

        private Collection<MediaHighwayTitle> titles;
        
        private string channelName;
        private byte[] unknown;

        /// <summary>
        /// Initialize a new instance of the MediaHighwayChannel class.
        /// </summary>
        public MediaHighwayChannel() { }

        /// <summary>
        /// Add title data to the channel.
        /// </summary>
        /// <param name="newTitle">The title to be added.</param>
        public void AddTitleData(MediaHighwayTitle newTitle)
        {
            foreach (MediaHighwayTitle oldTitle in Titles)
            {
                if (oldTitle.StartTime == newTitle.StartTime)
                    return;

                if (oldTitle.StartTime > newTitle.StartTime)
                {
                    Titles.Insert(Titles.IndexOf(oldTitle), newTitle);
                    return;
                }
            }

            Titles.Add(newTitle);
        }

        /// <summary>
        /// Create the EPG entries from the stored title and summary data.
        /// </summary>
        /// <param name="station">The station that the EPG records are for.</param>
        /// <param name="titleLogger">A Logger instance for the program titles.</param>
        /// <param name="descriptionLogger">A Logger instance for the program descriptions.</param>
        /// <param name="collectionType">The type of collection, MHW1 or MHW2.</param>
        public void ProcessChannelForEPG(TVStation station, Logger titleLogger, Logger descriptionLogger, CollectionType collectionType)
        {
            bool first = true;
            DateTime expectedStartTime = new DateTime();

            foreach (MediaHighwayTitle title in Titles)
            {
                EPGEntry epgEntry = new EPGEntry();
                epgEntry.OriginalNetworkID = OriginalNetworkID;
                epgEntry.TransportStreamID = TransportStreamID;
                epgEntry.ServiceID = ServiceID;
                epgEntry.EventID = title.EventID;

                processEventName(epgEntry, title.EventName);

                MediaHighwaySummary summary = null;

                if (title.SummaryAvailable)
                {
                    summary = findSummary(title.EventID);
                    if (summary != null)
                        processShortDescription(epgEntry, title.EventName, summary.ShortDescription);
                    else
                    {
                        if (DebugEntry.IsDefined(DebugName.Mhw2SummaryMissing))
                            Logger.Instance.Write("Summary missing for event ID " + title.EventID);
                    }
                }                
                if (summary == null)
                    epgEntry.ShortDescription = "No Synopsis Available";

                if (collectionType == CollectionType.MediaHighway1)
                    epgEntry.StartTime = Utils.RoundTime(TimeOffsetEntry.GetOffsetTime(title.StartTime));
                else
                    epgEntry.StartTime = Utils.RoundTime(TimeOffsetEntry.GetAdjustedTime(title.StartTime));
                epgEntry.Duration = Utils.RoundTime(title.Duration);

                epgEntry.EventCategory = getEventCategory(epgEntry.EventName, epgEntry.ShortDescription, title.CategoryID);                

                if (collectionType == CollectionType.MediaHighway1)
                    epgEntry.EPGSource = EPGSource.MediaHighway1;
                else
                    epgEntry.EPGSource = EPGSource.MediaHighway2;

                epgEntry.VideoQuality = getVideoQuality(title.EventName);
                epgEntry.PreviousPlayDate = title.PreviousPlayDate;
                epgEntry.LanguageCode = RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.InputLanguage;

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
                        if (collectionType == CollectionType.MediaHighway1)
                            titleLogger.Write(epgEntry.OriginalNetworkID + ":" + epgEntry.TransportStreamID + ":" + epgEntry.ServiceID + " " +
                                " Evt ID " + title.EventID +
                                " Cat ID " + title.CategoryID.ToString("00") +
                                " Summary " + title.SummaryAvailable + ":" + (summary != null) + " " +
                                " Orig Day " + title.LogDay +
                                " Orig Hours " + title.LogHours +
                                " YDay " + title.LogYesterday +
                                " Day " + title.Day +
                                " Hours " + title.Hours +
                                " Mins " + title.Minutes + " " +
                                epgEntry.StartTime.ToShortDateString() + " " +
                                epgEntry.StartTime.ToString("HH:mm") + " - " +
                                epgEntry.StartTime.Add(epgEntry.Duration).ToString("HH:mm") + " " +
                                title.EventName);
                        else
                            titleLogger.Write(epgEntry.OriginalNetworkID + ":" + epgEntry.TransportStreamID + ":" + epgEntry.ServiceID + " " +
                                " Evt ID " + title.EventID +
                                " Cat ID " + title.CategoryID.ToString("000") +
                                " Main cat " + title.MainCategory +
                                " Sub cat " + title.SubCategory +
                                " Summary " + title.SummaryAvailable + ":" + (summary != null) +
                                " Unknown " + Utils.ConvertToHex(title.Unknown) + " " +
                                epgEntry.StartTime.ToShortDateString() + " " +
                                epgEntry.StartTime.ToString("HH:mm") + " - " +
                                epgEntry.StartTime.Add(epgEntry.Duration).ToString("HH:mm") + " " +
                                title.EventName);

                    }

                    if (descriptionLogger != null && summary != null)
                    {
                        if (collectionType == CollectionType.MediaHighway1)
                            descriptionLogger.Write(epgEntry.OriginalNetworkID + ":" + epgEntry.TransportStreamID + ":" + epgEntry.ServiceID + " " +
                                " Evt ID " + title.EventID +
                                " Rpts: " + summary.ReplayCount + " " +
                                epgEntry.StartTime.ToShortDateString() + " " +
                                epgEntry.StartTime.ToString("HH:mm") + " - " +
                                epgEntry.StartTime.Add(epgEntry.Duration).ToString("HH:mm") + " " +
                                summary.ShortDescription);
                        else
                            descriptionLogger.Write(epgEntry.OriginalNetworkID + ":" + epgEntry.TransportStreamID + ":" + epgEntry.ServiceID + " " +
                                " Evt ID " + title.EventID + " " +
                                " Unknown " + Utils.ConvertToHex(summary.Unknown) + " " +
                                epgEntry.StartTime.ToShortDateString() + " " +
                                epgEntry.StartTime.ToString("HH:mm") + " - " +
                                epgEntry.StartTime.Add(epgEntry.Duration).ToString("HH:mm") + " " +
                                summary.ShortDescription);
                    }

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
        }

        internal static MediaHighwayChannelTitle FindChannelTitle(int eventID)
        {
            foreach (MediaHighwayChannel channel in MediaHighwayChannel.Channels)
            {
                MediaHighwayTitle title = channel.findTitle(eventID);
                if (title != null)
                    return (new MediaHighwayChannelTitle(channel, title));
            }

            return (null);
        }

        private MediaHighwayTitle findTitle(int eventID)
        {
            foreach (MediaHighwayTitle title in Titles)
            {
                if (title.EventID == eventID)
                    return (title);

                if (title.EventID > eventID)
                    return (null);
            }

            return (null);
        }

        private MediaHighwaySummary findSummary(int eventID)
        {
            foreach (MediaHighwaySummary summary in MediaHighwaySummary.Summaries)
            {
                if (summary.EventID == eventID)
                    return (summary);

                if (summary.EventID > eventID)
                    return (null);
            }

            return (null);
        }

        private string getEventCategory(string title, string description, int categoryID)
        {
            if (categoryID == 0)
                return (getCustomCategory(title, description));

            if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options,
                OptionName.CustomCategoryOverride))
            {
                string customCategory = getCustomCategory(title, description);
                if (customCategory != null)
                    return (customCategory);
            }
            
            MediaHighwayProgramCategory category = MediaHighwayProgramCategory.FindCategory(categoryID);
            if (category != null)
                return (getCasedString(category.Description));

            if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options,
                OptionName.CustomCategoryOverride))
                return (null);

            return(getCustomCategory(title, description));
        }

        private string getCustomCategory(string title, string description)
        {
            string category = CustomProgramCategory.FindCategoryDescription(title);
            if (category != null)
                return (category);

            return (CustomProgramCategory.FindCategoryDescription(description));
        }

        private string getVideoQuality(string title)
        {
            if (title.ToLowerInvariant().Contains("(hd)"))
                return ("HDTV");
            else
                return (null);
        }

        private void processEventName(EPGEntry epgEntry, string eventName)
        {
            switch (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode)
            {
                case Country.France:
                    processFrenchTitle(epgEntry, eventName);
                    break;
                case Country.Spain:
                    processSpanishTitle(epgEntry, eventName);
                    break;
                default:
                    epgEntry.EventName = EditSpec.ProcessTitle(eventName);
                    break;
            }
        }

        private void processFrenchTitle(EPGEntry epgEntry, string eventName)
        {
            string title = EditSpec.ProcessTitle(getCasedString(eventName));
            
            if (title.Contains("(HD)"))
                title = title.Replace("(HD)", "").Trim();
            if (title.Contains("(hd)"))
                title = title.Replace("(hd)", "").Trim();

            if (!title.EndsWith(")"))
            {
                epgEntry.EventName = title;
                return;
            }

            int index = title.Length - 1;

            while (index > -1 && title[index] != '(')
                index--;

            if (index < 1)
            {
                epgEntry.EventName = title;
                return;
            }

            epgEntry.EventName = title.Substring(0, index).Trim();
        }

        private void processSpanishTitle(EPGEntry epgEntry, string eventName)
        {
            string title = EditSpec.ProcessTitle(eventName);

            if (title.StartsWith("("))
            {
                epgEntry.EventName = title;
                return;
            }

            if (title.Contains("(HD)"))
                title = title.Replace("(HD)", "").Trim();
            if (title.Contains("(hd)"))
                title = title.Replace("(hd)", "").Trim();

            if (title.Contains("(VOS)"))
                title = title.Replace("(VOS)", "").Trim();

            int bracketIndex = title.IndexOf("(");            

            int otherIndex = title.IndexOf(":");
            if (otherIndex == -1)
            {
                otherIndex = title.IndexOf(".");
                if (otherIndex < 5)
                    otherIndex = -1;
            }

            if (otherIndex != -1 && (bracketIndex == -1 || otherIndex < bracketIndex))
            {
                if (otherIndex > 0)
                {
                    if (isNumeric(title[otherIndex - 1]))
                    {
                        otherIndex--;

                        while (otherIndex > 0 && isNumeric(title[otherIndex]))
                            otherIndex--;

                        if (otherIndex < 1)
                        {
                            epgEntry.EventName = title;
                            return;
                        }

                        if (title[otherIndex] != 'T')
                        {
                            epgEntry.EventName = title;
                            return;
                        }

                        epgEntry.EventName = title.Substring(0, otherIndex).Trim();
                        return;
                    }
                    else
                    {
                        epgEntry.EventName = title.Substring(0, otherIndex);
                        return;
                    }

                }
            }
            else
            {
                if (bracketIndex != -1)
                {
                    epgEntry.EventName = title.Substring(0, bracketIndex).Trim();
                    return;
                }
            }

            epgEntry.EventName = title;
        }

        private void processShortDescription(EPGEntry epgEntry, string originalTitle, string description)
        {
            epgEntry.ShortDescription = EditSpec.ProcessDescription(description);

            switch (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode)
            {
                case Country.France:
                    epgEntry.ShortDescription = processFrenchDescription(epgEntry, description);
                    break;
                case Country.Spain:
                    processSpanishDescription(epgEntry, originalTitle);
                    break;
                default:
                    break;
            }
        }

        private string processFrenchDescription(EPGEntry epgEntry, string inputDescription)
        {
            if (inputDescription == null || inputDescription == string.Empty)
                return (inputDescription);

            inputDescription = inputDescription.Replace("\n", " ");

            string editedDescription = getFrenchParentalRating(epgEntry, inputDescription);
            editedDescription = getFrenchSubtitle(epgEntry, editedDescription);
            editedDescription = getFrenchPresenters(epgEntry, editedDescription);
            editedDescription = getFrenchDirectors(epgEntry, editedDescription);
            editedDescription = getFrenchCast(epgEntry, editedDescription);
            editedDescription = getFrenchGuestStars(epgEntry, editedDescription);
            editedDescription = getFrenchSeasonEpisode(epgEntry, editedDescription);

            return (editedDescription.Replace("  ", " "));
        }

        private string getFrenchParentalRating(EPGEntry epgEntry, string description)
        {
            if (description.Substring(0, 2) != "(-")
                return (description);

            int index1 = description.IndexOf(')');
            if (index1 == -1)
                return (description);

            try
            {
                int parentalRating = Int32.Parse(description.Substring(2, index1 - 2));

                epgEntry.ParentalRating = ParentalRating.FindRating(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode, "MHW1", parentalRating.ToString());
                epgEntry.MpaaParentalRating = ParentalRating.FindMpaaRating(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode, "MHW1", parentalRating.ToString());

                if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
                    return (description.Substring(index1 + 1).Trim());
                else
                    return (description);
            }
            catch (FormatException)
            {
                return (description);
            }
            catch (OverflowException)
            {
                return (description);
            }
        }

        private string getFrenchSubtitle(EPGEntry epgEntry, string description)
        {
            int index1 = description.IndexOf('"');
            if (index1 == -1)
                return (description);

            int index2 = description.IndexOf('"', index1 + 1);
            if (index2 == -1 || index2 >= description.Length - 1)
                return (description);

            if (description[index2 + 1] != '.')
                return (description);

            epgEntry.EventSubTitle = description.Substring(index1 + 1, index2 - (index1 + 1));

            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
                return (description.Remove(index1, (index2 + 2) - index1).Trim());
            else
                return (description);
        }

        private string getFrenchPresenters(EPGEntry epgEntry, string description)
        {
            string presenterString = "présenté par ";

            int index1 = description.ToLower().IndexOf(presenterString);
            if (index1 == -1)
                return (description);

            int index2 = description.IndexOf(".", index1);
            if (index2 == -1)
                return (description);

            string[] presenters = description.Substring(index1 + presenterString.Length, index2 - (index1 + presenterString.Length)).Split(new char[] { ',' } );

            epgEntry.Presenters = new Collection<string>();

            foreach (string presenter in presenters)
                epgEntry.Presenters.Add(presenter.Trim());

            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
                return (description.Remove(index1, (index2 + 1) - index1).Trim());
            else
                return (description);
        }

        private string getFrenchDirectors(EPGEntry epgEntry, string description)
        {
            string directorString = "réalisé par ";
            string endString = " en ";

            bool datePresent = false;

            int index1 = description.ToLower().IndexOf(directorString);
            if (index1 == -1)
                return (description);

            int index2 = description.IndexOf(endString, index1);
            if (index2 == -1)
            {
                endString = ".";

                index2 = description.IndexOf(endString, index1);
                if (index2 == -1)
                    return (description);
            }
            else
                datePresent = true;

            string[] directors = description.Substring(index1 + directorString.Length, index2 - (index1 + directorString.Length)).Split(new char[] { ',' });

            epgEntry.Directors = new Collection<string>();

            foreach (string director in directors)
                epgEntry.Directors.Add(director.Trim());

            if (datePresent)
            {
                epgEntry.Date = description.Substring(index2 + endString.Length, 4);

                if (description[index2 + endString.Length + 4] != '.')
                    index2 += endString.Length + 4;
                else
                {
                    if (index1 != 0)
                        index2 += endString.Length + 3;
                    else
                        index2 += endString.Length + 4;
                }

                if (index1 > 0)
                    index1--;
            }

            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
                return (description.Remove(index1, (index2 + 1) - index1).Trim());
            else
                return (description);
        }

        private string getFrenchCast(EPGEntry epgEntry, string description)
        {
            string castString = "Avec ";

            int index1 = description.IndexOf(castString);
            if (index1 == -1)
            {
                castString = "avec la participation de ";

                index1 = description.ToLower().IndexOf(castString);
                if (index1 == -1)
                {
                    castString = "avec ";

                    index1 = description.ToLower().IndexOf(castString);
                    if (index1 == -1 || index1 > 60)
                        return (description);
                }
            }

            int index2 = description.IndexOf(".", index1);
            if (index2 == -1)
                return (description);

            string[] castMembers = description.Substring(index1 + castString.Length, index2 - (index1 + castString.Length)).Split(new char[] { ',' });

            foreach (string castMember in castMembers)
            {
                string[] nameParts = castMember.Trim().Split(new char[] { ' ' });
                if (nameParts.Length > 3)
                    return (description);
            }

            epgEntry.Cast = new Collection<string>();

            foreach (string castMember in castMembers)
                epgEntry.Cast.Add(castMember.Trim());

            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
                return (description.Remove(index1, (index2 + 1) - index1).Trim());
            else
                return (description);
        }

        private string getFrenchGuestStars(EPGEntry epgEntry, string description)
        {
            string guestString = "invité : ";

            int index1 = description.ToLower().IndexOf(guestString);
            if (index1 == -1)
                return (description);

            int index2 = description.IndexOf(".", index1);
            if (index2 == -1)
                return (description);

            string[] guests = description.Substring(index1 + guestString.Length, index2 - (index1 + guestString.Length)).Split(new char[] { ',' });

            epgEntry.GuestStars = new Collection<string>();

            foreach (string guest in guests)
                epgEntry.GuestStars.Add(guest.Trim());

            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
                return (description.Remove(index1, (index2 + 1) - index1).Trim());
            else
                return (description);
        }

        private string getFrenchSeasonEpisode(EPGEntry epgEntry, string description)
        {
            string seasonString = "saison ";

            int index = description.ToLower().IndexOf(seasonString);
            if (index != -1)
                return (getFrenchFullSeasonEpisode(epgEntry, description, seasonString, index));

            index = description.ToLower().IndexOf("/");
            if (index != -1)
                return (getFrenchEpisodeOnly(epgEntry, description, index));

            return (description);
        }

        private string getFrenchFullSeasonEpisode(EPGEntry epgEntry, string description, string seasonString, int index1)
        {
            int index2 = description.IndexOf(".", index1);
            if (index2 == -1)
                return (description);

            int seasonNumber = -1;
            int episodeNumber = -1;
            int episodeCount = -1;

            int index3 = -1;
            int index4 = -1;
            int index5 = -1;

            try
            {
                seasonNumber = Int32.Parse(description.Substring(index1 + seasonString.Length, index2 - (index1 + seasonString.Length)));

                if (index2 + 3 < description.Length && description.Substring(index2 + 1, 2) == " (")
                {
                    if (description.Substring(index2 + 3, 1) != "n")
                    {
                        index3 = index2 + 3;
                        index4 = description.IndexOf('/');
                        if (index4 == -1)
                            return (description);
                        index5 = description.IndexOf(')', index4);
                        if (index5 == -1)
                            return (description);

                        episodeNumber = Int32.Parse(description.Substring(index3, index4 - index3));
                        episodeCount = Int32.Parse(description.Substring(index4 + 1, index5 - (index4 + 1)));
                    }
                    else
                    {
                        index3 = index2 + 5;
                        index4 = description.IndexOf(')', index3);
                        if (index4 == -1)
                            return (description);

                        episodeNumber = Int32.Parse(description.Substring(index3, index4 - index3));
                        index5 = index4;
                    }
                }
                else
                    index5 = index2 - 1;

                epgEntry.SeasonNumber = seasonNumber;
                epgEntry.EpisodeNumber = episodeNumber;

                if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
                    return (description.Remove(index1, (index5 + 2) - index1).Trim());
                else
                    return (description);
            }
            catch (FormatException)
            {
                return (description);
            }
            catch (OverflowException)
            {
                return (description);
            }
        }

        private string getFrenchEpisodeOnly(EPGEntry epgEntry, string description, int index1)
        {
            int startIndex = index1;

            while (startIndex > -1 && description[startIndex] != '(')
                startIndex--;

            if (startIndex < 0)
                return (description);

            int endIndex = startIndex;

            while (endIndex < description.Length && description[endIndex] != ')')
                endIndex++;

            int episodeNumber = -1;
            int episodeCount = -1;

            try
            {
                episodeNumber = Int32.Parse(description.Substring(startIndex + 1, index1 - startIndex - 1));
                episodeCount = Int32.Parse(description.Substring(index1 + 1, endIndex - index1 - 1));

                epgEntry.SeasonNumber = -1;
                epgEntry.EpisodeNumber = episodeNumber;

                if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
                {
                    if (startIndex > 0 && description[startIndex - 1] == ' ')
                        startIndex--;
                    else
                    {
                        if (startIndex == 0)
                            endIndex++;
                    }

                    return (description.Remove(startIndex, endIndex - startIndex + 1).Trim());
                }
                else
                    return (description);
            }
            catch (FormatException)
            {
                return (description);
            }
            catch (OverflowException)
            {
                return (description);
            }
        }

        private void processSpanishDescription(EPGEntry epgEntry, string originalTitle)
        {
            int descriptionStart = -1;

            int lastIndex = getSpanishOtherData(epgEntry);
            if (lastIndex > descriptionStart)
                descriptionStart = lastIndex;
            
            lastIndex = getSpanishSeasonEpisode(epgEntry);
            if (lastIndex > descriptionStart)
                descriptionStart = lastIndex;

            lastIndex = getSpanishSubTitle(epgEntry);
            if (lastIndex > descriptionStart)
                descriptionStart = lastIndex;

            lastIndex = getSpanishYear(epgEntry);
            if (lastIndex > descriptionStart)
                descriptionStart = lastIndex;

            lastIndex = getSpanishParentalRating(epgEntry);
            if (lastIndex > descriptionStart)
                descriptionStart = lastIndex;

            lastIndex = getSpanishDirectors(epgEntry);
            if (lastIndex != -1)
                descriptionStart = lastIndex;

            lastIndex = getSpanishProducers(epgEntry);
            if (lastIndex != -1)
                descriptionStart = lastIndex;

            lastIndex = getSpanishCast(epgEntry);
            if (lastIndex != -1)
                descriptionStart = lastIndex;

            if (descriptionStart == -1)
            {
                editFields(epgEntry);
                return;
            }

            if (OptionEntry.IsDefined(OptionName.NoRemoveData))
            {
                editFields(epgEntry);
                return;
            }

            if (descriptionStart < epgEntry.ShortDescription.Length)
                epgEntry.ShortDescription = epgEntry.ShortDescription.Substring(descriptionStart).Trim();
            else
                epgEntry.ShortDescription = null;

            editFields(epgEntry);
        }

        private void editFields(EPGEntry epgEntry)
        {
            if (!string.IsNullOrWhiteSpace(epgEntry.EventName))
                epgEntry.EventName = MediaHighway2Controller.FixString(epgEntry.EventName, 0);
            
            if (!string.IsNullOrWhiteSpace(epgEntry.EventSubTitle))
                epgEntry.EventSubTitle = MediaHighway2Controller.FixString(epgEntry.EventSubTitle, 0);
            
            if (!string.IsNullOrWhiteSpace(epgEntry.ShortDescription))
                epgEntry.ShortDescription = MediaHighway2Controller.FixString(epgEntry.ShortDescription, 0);
        }

        private int getSpanishSeasonEpisode(EPGEntry epgEntry)
        {
            int index = 0;
            bool found = false;

            while (!found && index != -1)
            {
                index = epgEntry.ShortDescription.IndexOf(" T", index);
                if (index != -1)
                {
                    if (index < epgEntry.ShortDescription.Length - 2)
                        found = epgEntry.ShortDescription[index + 2] >= '0' && epgEntry.ShortDescription[index + 2] <= '9';

                    if (!found)
                        index++;
                }
            }

            if (!found)
                return (-1);

            int startIndex = index;
            index += 2;

            int seasonNumber = 0;
            int episodeNumber = -1;

            while (index < epgEntry.ShortDescription.Length && (epgEntry.ShortDescription[index] != ' ' && epgEntry.ShortDescription[index] != '.'))
            {
                seasonNumber = (seasonNumber * 10) + (epgEntry.ShortDescription[index] - '0');
                index++;
            }

            if (index > epgEntry.ShortDescription.Length - 4)
            {
                setSpanishSeasonEpisode(epgEntry, seasonNumber, episodeNumber);
                return (index);
            }

            index++;

            if (epgEntry.ShortDescription[index] != 'E' || 
                epgEntry.ShortDescription[index + 1] != 'p' ||
                epgEntry.ShortDescription[index + 2] != '.')
            {
                setSpanishSeasonEpisode(epgEntry, seasonNumber, episodeNumber);
                return (index - 1);
            }

            index += 3;
            episodeNumber = 0;

            while (index < epgEntry.ShortDescription.Length && epgEntry.ShortDescription[index] != ' ')
            {
                episodeNumber = (episodeNumber * 10) + (epgEntry.ShortDescription[index] - '0');
                index++;
            }

            setSpanishSeasonEpisode(epgEntry, seasonNumber, episodeNumber);

            return (index);
        }

        private void setSpanishSeasonEpisode(EPGEntry epgEntry, int seasonNumber, int episodeNumber)
        {
            epgEntry.SeasonNumber = seasonNumber;
            epgEntry.EpisodeNumber = episodeNumber;
        }    

        private int getSpanishSubTitle(EPGEntry epgEntry)
        {
            int index1 = epgEntry.ShortDescription.IndexOf('"');
            if (index1 == -1)
                return (-1);

            int index2 = epgEntry.ShortDescription.IndexOf('"', index1 + 1);
            if (index2 == -1)
                return (-1);

            epgEntry.EventSubTitle = epgEntry.ShortDescription.Substring(index1 + 1, index2 - index1 - 1);

            return (index2 + 1);
        }

        private int getSpanishDirectors(EPGEntry epgEntry)
        {
            string identifier = "Dir: ";

            int index1 = epgEntry.ShortDescription.IndexOf(identifier);
            if (index1 == -1)
            {
                identifier = "DIRECTOR MUSICAL: ";

                index1 = epgEntry.ShortDescription.IndexOf(identifier);
                if (index1 == -1)
                    return (-1);
            }

            int index2 = epgEntry.ShortDescription.IndexOf('.', index1);
            if (index2 == -1)
                return (-1);

            bool done = false;

            while (!done)
            {
                if (epgEntry.ShortDescription[index2 - 2] == ' ')
                {
                    int lastIndex2 = index2;
                    index2 = epgEntry.ShortDescription.IndexOf('.', index2 + 1);
                    if (index2 > lastIndex2 + 60)
                    {
                        index2 = lastIndex2;
                        done = true;
                    }
                }
                else
                    done = true;
            }

            string directorsString = epgEntry.ShortDescription.Substring(index1 + identifier.Length, index2 - (index1 + identifier.Length));
            string[] directorParts = directorsString.Split(new char[] { ',' });

            epgEntry.Directors = new Collection<string>();

            foreach (string director in directorParts)
                epgEntry.Directors.Add(director.Trim());

            return (index2 + 1);
        }

        private int getSpanishProducers(EPGEntry epgEntry)
        {
            string identifier = "Prod: ";

            int index1 = epgEntry.ShortDescription.IndexOf(identifier);
            if (index1 == -1)
            {
                identifier = "REALIZADOR: ";

                index1 = epgEntry.ShortDescription.IndexOf(identifier);
                if (index1 == -1)
                    return (-1);
            }

            int index2 = epgEntry.ShortDescription.IndexOf('.', index1);
            if (index2 == -1)
                return (-1);

            bool done = false;

            while (!done)
            {
                if (epgEntry.ShortDescription[index2 - 2] == ' ')
                {
                    int lastIndex2 = index2;
                    index2 = epgEntry.ShortDescription.IndexOf('.', index2 + 1);
                    if (index2 > lastIndex2 + 60)
                    {
                        index2 = lastIndex2;
                        done = true;
                    }
                }
                else
                    done = true;
            }

            string producersString = epgEntry.ShortDescription.Substring(index1 + identifier.Length, index2 - (index1 + identifier.Length));
            string[] producerParts = producersString.Split(new char[] { ',' });

            epgEntry.Producers = new Collection<string>();

            foreach (string producer in producerParts)
                epgEntry.Producers.Add(producer.Trim());

            return (index2 + 1);
        }

        private int getSpanishCast(EPGEntry epgEntry)
        {
            string identifier = "Int: ";

            int index1 = epgEntry.ShortDescription.IndexOf(identifier);
            if (index1 == -1)
            {
                identifier = "Pres: ";
                
                index1 = epgEntry.ShortDescription.IndexOf(identifier);
                if (index1 == -1)
                {
                    identifier = "CANTANTE: ";

                    index1 = epgEntry.ShortDescription.IndexOf(identifier);
                    if (index1 == -1)
                        return (-1);
                }
            }

            int index2 = epgEntry.ShortDescription.IndexOf('.', index1);
            if (index2 == -1)
                return (-1);

            bool done = false;

            while (!done)
            {
                if (epgEntry.ShortDescription[index2 - 2] == ' ')
                {
                    int lastIndex2 = index2;
                    index2 = epgEntry.ShortDescription.IndexOf('.', index2 + 1);
                    if (index2 == -1 || index2 > lastIndex2 + 60)
                    {
                        index2 = lastIndex2;
                        done = true;
                    }
                }
                else
                    done = true;
            }

            string castString = epgEntry.ShortDescription.Substring(index1 + identifier.Length, index2 - (index1 + identifier.Length));
            string[] castParts = castString.Split(new char[] { ',' });

            epgEntry.Cast = new Collection<string>();

            foreach (string castMember in castParts)
                epgEntry.Cast.Add(castMember.Trim());

            return (index2 + 1);
        }

        private int getSpanishYear(EPGEntry epgEntry)
        {
            string[] descriptionParts = epgEntry.ShortDescription.Split(new char[] { ' ' });
            
            for (int index = 0; index < descriptionParts.Length; index++)
            {
                string descriptionPart = descriptionParts[index];

                if (descriptionPart.Length == 5 && 
                    isNumeric(descriptionPart[0]) &&
                    isNumeric(descriptionPart[1]) &&
                    isNumeric(descriptionPart[2]) &&
                    isNumeric(descriptionPart[3]) &&
                    descriptionPart[4] == '.' &&
                    (descriptionPart[0] == '1' || descriptionPart[0] == '2'))
                {
                    if (index < descriptionParts.Length * 0.75)
                    {
                        epgEntry.Date = descriptionPart.Substring(0, 4);
                        return (epgEntry.ShortDescription.IndexOf(epgEntry.Date) + 5);
                    }
                }
            }

            return (-1);
        }

        private int getSpanishParentalRating(EPGEntry epgEntry)
        {
            int startIndex = epgEntry.ShortDescription.IndexOf("(+");
            if (startIndex == -1)
                return (-1);

            int endIndex = startIndex + 2;

            while (epgEntry.ShortDescription[endIndex] != ')')
                endIndex++;

            string parentalRating = epgEntry.ShortDescription.Substring(startIndex + 2, endIndex - startIndex - 2);

            epgEntry.ParentalRating = ParentalRating.FindRating(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode, "MHW2", parentalRating);
            epgEntry.MpaaParentalRating = ParentalRating.FindMpaaRating(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode, "MHW2", parentalRating);

            return (endIndex + 1);
        }

        private int getSpanishOtherData(EPGEntry epgEntry)
        {
            int lastIndex = -1;

            int currentIndex = epgEntry.ShortDescription.IndexOf("EE.UU.");
            if (currentIndex != -1 && currentIndex + 6 > lastIndex)
                lastIndex = currentIndex + 6;

            currentIndex = epgEntry.ShortDescription.IndexOf("EEUU.");
            if (currentIndex != -1 && currentIndex + 5 > lastIndex)
                lastIndex = currentIndex + 5;

            currentIndex = epgEntry.ShortDescription.IndexOf("(TP)");
            if (currentIndex != -1 && currentIndex + 4 > lastIndex)
                lastIndex = currentIndex + 4;

            currentIndex = epgEntry.ShortDescription.IndexOf("(SC)");
            if (currentIndex != -1 && currentIndex + 4 > lastIndex)
                lastIndex = currentIndex + 4;

            currentIndex = epgEntry.ShortDescription.IndexOf("R.U.");
            if (currentIndex != -1 && currentIndex + 4 > lastIndex)
                lastIndex = currentIndex + 4;

            currentIndex = epgEntry.ShortDescription.IndexOf("(INF)");
            if (currentIndex != -1 && currentIndex + 5 > lastIndex)
                lastIndex = currentIndex + 5;

            return (lastIndex);
        }

        private bool isNumeric(char testChar)
        {
            return (testChar >= '0' && testChar <= '9');
        }

        private string getCasedString(string inputString)
        {
            if (inputString == null || inputString.Length == 0)
                return (inputString);

            string[] inputParts = inputString.Trim().Split(' ');

            StringBuilder casedString = new StringBuilder();

            foreach (string inputPart in inputParts)
            {
                if (inputPart.Length != 0)
                {
                    if (casedString.Length != 0)
                        casedString.Append(' ');

                    casedString.Append(inputPart.Substring(0, 1).ToUpper());

                    if (inputPart.Length > 1)
                    {
                        if (inputPart[1] != '.')
                            casedString.Append(inputPart.Substring(1).ToLower());
                        else
                            casedString.Append(inputPart.Substring(1));
                    }
                }
            }

            return (casedString.ToString());           
        }

        /// <summary>
        /// Get a description of this instance.
        /// </summary>
        /// <returns>A string describing this instance.</returns>
        public override string ToString()
        {
            TVStation station = TVStation.FindStation(RunParameters.Instance.StationCollection, 
                OriginalNetworkID, TransportStreamID, ServiceID);
            string stationName;
            if (station != null)
                stationName = station.Name;
            else
                stationName = "** No Station **";

            string unknownString;
            if (unknown == null)
                unknownString = "n/a";
            else
                unknownString = Utils.ConvertToHex(unknown);

            return ("ONID " + OriginalNetworkID +
                " TSID " + TransportStreamID +
                " SID " + ServiceID +
                " Channel ID: " + ChannelID +
                " Unknown: " + unknownString +
                " Name: " + channelName +
                " Station: " + stationName);
        }
    }
}
