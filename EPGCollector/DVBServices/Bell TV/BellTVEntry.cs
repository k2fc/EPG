﻿////////////////////////////////////////////////////////////////////////////////// 
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
    /// The class that describes a Bell TV entry.
    /// </summary>
    public class BellTVEntry
    {
        /// <summary>
        /// Get the event identification.
        /// </summary>
        public int EventID { get { return (eventID); } }
        /// <summary>
        /// Get the event start time.
        /// </summary>
        public DateTime StartTime { get { return (startTime); } }
        /// <summary>
        /// Get the event duration.
        /// </summary>
        public TimeSpan Duration { get { return (duration); } }
        /// <summary>
        /// Get the running status of the event.
        /// </summary>
        public int RunningStatus { get { return (runningStatus); } }
        /// <summary>
        /// Return true if the event is scrambled; false otherwise.
        /// </summary>
        public bool Scrambled { get { return (scrambled); } }
        /// <summary>
        /// Get the event name.
        /// </summary>
        public string EventName
        {
            get { return (eventName); }
            set { eventName = value; }
        }
        /// <summary>
        /// Get the short description for the event.
        /// </summary>
        public string ShortDescription { get { return (shortDescription); } }
        /// <summary>
        /// Get the original description for the event.
        /// </summary>
        public string OriginalDescription { get { return (originalDescription); } }
        /// <summary>
        /// Get the sub title for the event.
        /// </summary>
        public string SubTitle { get { return (subTitle); } }
        /// <summary>
        /// Get the closed captions flag.
        /// </summary>
        public bool ClosedCaptions { get { return (closedCaptions); } }
        /// <summary>
        /// Get the high definition flag.
        /// </summary>
        public bool HighDefinition { get { return (highDefinition); } }
        /// <summary>
        /// Get the stereo flag.
        /// </summary>
        public bool Stereo { get { return (stereo); } }
        /// <summary>
        /// Get the DVB standard (EN 300 468) content type.
        /// </summary>
        public int ContentType { get { return (contentType); } }
        /// <summary>
        /// Get the DVB standard (EN 300 468) content subype.
        /// </summary>
        public int ContentSubType { get { return (contentSubType); } }
        /// <summary>
        /// Get the parental rating.
        /// </summary>
        public int ParentalRating { get { return (parentalRating); } }
        /// <summary>
        /// Get the star rating.
        /// </summary>
        public int StarRating { get { return (starRating); } }
        /// <summary>
        /// Get the date.
        /// </summary>
        public string Date { get { return (date); } }
        /// <summary>
        /// Get the series.
        /// </summary>
        public int Series { get { return (series); } }
        /// <summary>
        /// Get the episode.
        /// </summary>
        public int Episode { get { return (episode); } }
        /// <summary>
        /// Get the cast.
        /// </summary>
        public Collection<string> Cast { get { return (cast); } }
        /// <summary>
        /// Get the sexual content advisory setting.
        /// </summary>
        public bool HasSexualContent { get { return (hasSexualContent); } }
        /// <summary>
        /// Get the strong language advisory setting.
        /// </summary>
        public bool HasStrongLanguage { get { return (hasStrongLanguage); } }
        /// <summary>
        /// Get the violence advisory setting.
        /// </summary>
        public bool HasViolence { get { return (hasViolence); } }
        /// <summary>
        /// Get the nudity advisory setting.
        /// </summary>
        public bool HasNudity { get { return (hasNudity); } }
        /// <summary>
        /// Get the original air date.
        /// </summary>
        public DateTime OriginalAirDate { get { return (originalAirDate); } }

        /// <summary>
        /// Get the index of the next byte in the MPEG2 section following the EIT entry.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The entry has not been processed.
        /// </exception>
        public int Index
        {
            get
            {
                if (lastIndex == -1)
                    throw (new InvalidOperationException("BellTVEntry: Index requested before block processed"));
                return (lastIndex);
            }
        }

        private int eventID;
        private DateTime startTime;
        private TimeSpan duration;
        private int runningStatus;
        private bool scrambled;

        private string eventName;
        private string originalDescription;
        private string shortDescription;
        private string subTitle;
        private bool closedCaptions;
        private bool highDefinition;
        private bool stereo;
        private int contentType = -1;
        private int contentSubType = -1;
        private int parentalRating = -1;
        private int starRating = -1;
        private string date;
        private int series = -1;
        private int episode = -1;
        private Collection<string> cast;
        private bool hasSexualContent;
        private bool hasStrongLanguage;
        private bool hasViolence;
        private bool hasNudity;
        private DateTime originalAirDate;

        private int lastIndex = -1;

        /// <summary>
        /// Initialize a new instance of the BellTVEntry class.
        /// </summary>
        public BellTVEntry() { }

        /// <summary>
        /// Parse the entry.
        /// </summary>
        /// <param name="byteData">The MPEG2 section containing the entry.</param>
        /// <param name="index">Index of the event identification byte in the MPEG2 section.</param>
        /// <param name="table">The table ID containing this section.</param>
        public void Process(byte[] byteData, int index, int table)
        {
            lastIndex = index;

            try
            {
                eventID = Utils.Convert2BytesToInt(byteData, lastIndex);
                lastIndex += 2;

                getStartTime(byteData, lastIndex);
                lastIndex += 5;

                getDuration(byteData, lastIndex);
                lastIndex += 3;

                runningStatus = (int)(byteData[lastIndex] >> 5);
                scrambled = ((int)byteData[lastIndex] & 0x10) >> 4 == 1;

                int descriptorLoopLength = ((byteData[lastIndex] & 0x0f) * 256) + (int)byteData[lastIndex + 1];
                lastIndex += 2;

                while (descriptorLoopLength != 0)
                {
                    DescriptorBase descriptor = DescriptorBase.BellTVInstance(byteData, lastIndex, table);

                    if (!descriptor.IsEmpty)
                    {
                        processDescriptor(descriptor);
                        descriptor.LogMessage();

                        lastIndex = descriptor.Index;
                        descriptorLoopLength -= descriptor.TotalLength;
                    }
                    else
                    {
                        lastIndex += DescriptorBase.MinimumDescriptorLength;
                        descriptorLoopLength -= DescriptorBase.MinimumDescriptorLength;
                    }
                }

                Validate();
            }
            catch (IndexOutOfRangeException)
            {
                throw (new ArgumentOutOfRangeException("The Bell TV message is short"));
            }
        }

        private void getStartTime(byte[] byteData, int index)
        {
            int startDate = Utils.Convert2BytesToInt(byteData, index);

            int year = (int)((startDate - 15078.2) / 365.25);
            int month = (int)(((startDate - 14956.1) - (int)(year * 365.25)) / 30.6001);
            int day = (startDate - 14956) - (int)(year * 365.25) - (int)(month * 30.6001);

            int adjust;

            if (month == 14 || month == 15)
                adjust = 1;
            else
                adjust = 0;

            year = year + 1900 + adjust;
            month = month - 1 - (adjust * 12);

            int hour1 = (int)byteData[index + 2] >> 4;
            int hour2 = (int)byteData[index + 2] & 0x0f;
            int hour = (hour1 * 10) + hour2;

            int minute1 = (int)byteData[index + 3] >> 4;
            int minute2 = (int)byteData[index + 3] & 0x0f;
            int minute = (minute1 * 10) + minute2;

            int second1 = (int)byteData[index + 4] >> 4;
            int second2 = (int)byteData[index + 4] & 0x0f;
            int second = (second1 * 10) + second2;

            try
            {
                DateTime utcStartTime = new DateTime(year, month, day, hour, minute, second);
                startTime = utcStartTime.ToLocalTime();
            }
            catch (ArgumentOutOfRangeException)
            {
                throw (new ArgumentOutOfRangeException("The start time element(s) are out of range"));
            }
            catch (ArgumentException)
            {
                throw (new ArgumentOutOfRangeException("The start time element(s) result in a start time that is out of range"));
            }
        }

        private void getDuration(byte[] byteData, int index)
        {
            int durationHours1 = (int)byteData[index] >> 4;
            int durationHours2 = (int)byteData[index] & 0x0f;
            int durationHours = (durationHours1 * 10) + durationHours2;

            int durationMinutes1 = (int)byteData[index + 1] >> 4;
            int durationMinutes2 = (int)byteData[index + 1] & 0x0f;
            int durationMinutes = (durationMinutes1 * 10) + durationMinutes2;

            int durationSeconds1 = (int)byteData[index + 2] >> 4;
            int durationSeconds2 = (int)byteData[index + 2] & 0x0f;
            int durationSeconds = (durationSeconds1 * 10) + durationSeconds2;

            try
            {
                duration = new TimeSpan(durationHours, durationMinutes, durationSeconds);
            }
            catch (ArgumentOutOfRangeException)
            {
                throw (new ArgumentOutOfRangeException("The duration time span element(s) are out of range"));
            }
        }

        private void processDescriptor(DescriptorBase descriptor)
        {
            switch (descriptor.Tag)
            {
                case DescriptorBase.ShortEventDescriptorTag:
                    BellShortEventDescriptor shortEventDescriptor = descriptor as BellShortEventDescriptor;
                    if (eventName == null)
                    {
                        eventName = shortEventDescriptor.EventName;
                        highDefinition = shortEventDescriptor.HighDefinition;
                    }
                    else
                    {
                        if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.InputLanguage == null)
                        {
                            eventName = shortEventDescriptor.EventName;
                            highDefinition = shortEventDescriptor.HighDefinition;
                        }
                        else
                        {
                            if (shortEventDescriptor.LanguageCode == RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.InputLanguage)
                            {
                                eventName = shortEventDescriptor.EventName;
                                highDefinition = shortEventDescriptor.HighDefinition;
                            }
                            else
                            {
                                if (shortEventDescriptor.LanguageCode == "eng")
                                {
                                    eventName = shortEventDescriptor.EventName;
                                    highDefinition = shortEventDescriptor.HighDefinition;
                                }
                            }
                        }
                    }
                    LanguageCode.RegisterUsage(shortEventDescriptor.LanguageCode);
                    break;
                case DescriptorBase.ExtendedEventDescriptorTag:
                    BellTVExtendedEventDescriptor extendedEventDescriptor = descriptor as BellTVExtendedEventDescriptor;

                    if (shortDescription == null)
                    {
                        originalDescription = extendedEventDescriptor.OriginalDescription;
                        shortDescription = extendedEventDescriptor.EventDescription;
                        subTitle = extendedEventDescriptor.SubTitle;
                    }
                    else
                    {
                        if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.InputLanguage == null)
                        {
                            originalDescription = extendedEventDescriptor.OriginalDescription;
                            shortDescription = extendedEventDescriptor.EventDescription;
                            subTitle = extendedEventDescriptor.SubTitle;
                        }
                        else
                        {
                            if (extendedEventDescriptor.LanguageCode == RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.InputLanguage)
                            {
                                originalDescription = extendedEventDescriptor.OriginalDescription;
                                shortDescription = extendedEventDescriptor.EventDescription;
                                subTitle = extendedEventDescriptor.SubTitle;
                            }
                            else
                            {
                                if (extendedEventDescriptor.LanguageCode == "eng")
                                {
                                    originalDescription = extendedEventDescriptor.OriginalDescription;
                                    shortDescription = extendedEventDescriptor.EventDescription;
                                    subTitle = extendedEventDescriptor.SubTitle;
                                }
                            }
                        }
                    }
                    LanguageCode.RegisterUsage(extendedEventDescriptor.LanguageCode);
                    
                    closedCaptions = extendedEventDescriptor.ClosedCaptions;                    
                    stereo = extendedEventDescriptor.Stereo;
                    date = extendedEventDescriptor.Date;
                    cast = extendedEventDescriptor.Cast;
                    break;
                case DescriptorBase.BellTVRatingDescriptorTag:
                    BellTVRatingDescriptor ratingDescriptor = descriptor as BellTVRatingDescriptor;
                    parentalRating = ratingDescriptor.ParentalRating;
                    starRating = ratingDescriptor.StarRating;
                    hasSexualContent = ratingDescriptor.HasSexualContent;
                    hasStrongLanguage = ratingDescriptor.HasStrongLanguage;
                    hasViolence = ratingDescriptor.HasViolence;
                    hasNudity = ratingDescriptor.HasNudity;
                    break;
                case DescriptorBase.BellTVSeriesDescriptorTag:
                    BellTVSeriesDescriptor seriesDescriptor = descriptor as BellTVSeriesDescriptor;
                    series = seriesDescriptor.Series;
                    episode = seriesDescriptor.Episode;
                    originalAirDate = seriesDescriptor.OriginalAirDate;
                    break;
                case DescriptorBase.ContentDescriptorTag:
                    contentType = (descriptor as DVBContentDescriptor).ContentTypes[0].SubType;
                    contentSubType = (descriptor as DVBContentDescriptor).ContentTypes[0].UserType;
                    break;
                default:
                    if (DebugEntry.IsDefined(DebugName.UnknownDescriptors))
                    {
                        Logger.Instance.Write("Unprocessed Bell TV descriptor: 0x" + descriptor.Tag.ToString("X"));
                        if (DebugEntry.IsDefined(DebugName.LogDescriptorData))
                            Logger.Instance.Dump("Descriptor Data", descriptor.Data, descriptor.Data.Length);
                    }
                    break;
            }
        }

        /// <summary>
        /// Validate the entry fields.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// An entry field is not valid.
        /// </exception>
        public void Validate() { }

        /// <summary>
        /// Log the entry fields.
        /// </summary>
        public void LogMessage() { }
    }
}
