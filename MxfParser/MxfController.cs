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

namespace MxfParser
{
/// <summary>
    /// The class that describes the MXF parser controller.
    /// </summary>
    public sealed class MxfController : ImportFileBase
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

        private static string currentScheduleService;

        /// <summary>
        /// Process an MXF file.
        /// </summary>
        /// <param name="fileName">The actual file path.</param>
        /// <param name="fileSpec">The file definition.</param>
        /// <returns>An error message or null if the file is processed successfully.</returns>
        public override string Process(string fileName, ImportFileSpec fileSpec)
        {
            MxfKeyword.Keywords = new Collection<MxfKeyword>();
            MxfGuideImage.GuideImages = new Collection<MxfGuideImage>();
            MxfSeriesInfo.SeriesInfos = new Collection<MxfSeriesInfo>();
            MxfPerson.People = new Collection<MxfPerson>();
            MxfService.Services = new Collection<MxfService>();
            MxfChannel.Channels = new Collection<MxfChannel>();
            MxfProgramme.Programmes = new Collection<MxfProgramme>();
            MxfScheduleEntry.ScheduleEntries = new Collection<MxfScheduleEntry>();

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
                return ("Failed to open " + fileName);
            }

            try
            {
                while (!xmlReader.EOF)
                {
                    xmlReader.Read();
                    if (xmlReader.IsStartElement())
                    {
                        switch (xmlReader.Name.ToLowerInvariant())
                        {
                            case "keyword":
                                MxfKeyword keyword = MxfKeyword.GetInstance(xmlReader);
                                MxfKeyword.Keywords.Add(keyword);
                                break;
                            case "guideimage":
                                MxfGuideImage guideImage = MxfGuideImage.GetInstance(xmlReader);
                                MxfGuideImage.GuideImages.Add(guideImage);
                                break;
                            case "seriesinfo":
                                MxfSeriesInfo seriesInfo = MxfSeriesInfo.GetInstance(xmlReader);
                                MxfSeriesInfo.SeriesInfos.Add(seriesInfo);
                                break;
                            case "person":
                                MxfPerson person = MxfPerson.GetInstance(xmlReader);
                                MxfPerson.People.Add(person);
                                break;
                            case "service":
                                MxfService service = MxfService.GetInstance(xmlReader);
                                MxfService.Services.Add(service);
                                break;
                            case "channel":
                                MxfChannel channel = MxfChannel.GetInstance(xmlReader);
                                MxfChannel.Channels.Add(channel);
                                break;
                            case "program":
                                MxfProgramme programme = MxfProgramme.GetInstance(xmlReader, xmlReader.ReadSubtree());
                                MxfProgramme.Programmes.Add(programme);
                                break;
                            case "scheduleentries":
                                currentScheduleService = xmlReader.GetAttribute("service");
                                break;
                            case "scheduleentry":
                                MxfScheduleEntry schedule = MxfScheduleEntry.GetInstance(xmlReader, currentScheduleService);
                                MxfScheduleEntry.ScheduleEntries.Add(schedule);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (XmlException e)
            {
                return ("Failed to load mxf file: " + e.Message);
            }
            catch (IOException e)
            {
                return ("Failed to load mxf file: " + e.Message);
            }

            if (xmlReader != null)
                xmlReader.Close();

            if (DebugEntry.IsDefined(DebugName.LogMxfSeries))
                produceXmfAnalysis();

            Collection<TVStation> mxfChannels = createEPGData(fileSpec.Precedence, fileSpec.NoLookup);
            if (mxfChannels != null)
                MergeChannels(mxfChannels, fileSpec.Precedence, fileSpec.AppendOnly);

            return (null);
        }

        private void produceXmfAnalysis()
        {
            Logger.Instance.WriteSeparator("MXF Analysis");

            foreach (MxfSeriesInfo series in MxfSeriesInfo.SeriesInfos)
            {
                Logger.Instance.Write("Series: " + series.Id + " " + series.Title);

                Collection<MxfProgramme> programmes = findProgrammes(series);

                foreach (MxfProgramme programme in programmes)
                {
                    Logger.Instance.Write("    Program: " + programme.Id + " " + programme.Uid + " " + programme.Title + ":" + programme.Description);

                    Collection<MxfScheduleEntry> schedules = findSchedules(programme);

                    foreach (MxfScheduleEntry schedule in schedules)
                        Logger.Instance.Write("        Schedule: " + schedule.StartTime + " " + findService(schedule.Service));                                           
                }
            }
        }

        private Collection<MxfProgramme> findProgrammes(MxfSeriesInfo series)
        {
            Collection<MxfProgramme> programmes = new Collection<MxfProgramme>();

            foreach (MxfProgramme programme in MxfProgramme.Programmes)
            {
                if (programme.Series != null && programme.Series == series.Id)
                    programmes.Add(programme);
            }

            return (programmes);
        }

        private Collection<MxfScheduleEntry> findSchedules(MxfProgramme programme)
        {
            Collection<MxfScheduleEntry> schedules = new Collection<MxfScheduleEntry>();

            foreach (MxfScheduleEntry schedule in MxfScheduleEntry.ScheduleEntries)
            {
                if (schedule.Program == programme.Id)
                    schedules.Add(schedule);
            }

            return (schedules);
        }

        private string findService(string serviceId)
        {
            foreach (MxfService service in MxfService.Services)
            {
                if (service.Id == serviceId)
                    return (service.Name);
            }

            return ("Unknown service: " + serviceId);
        }

        private Collection<TVStation> createEPGData(DataPrecedence precedence, bool noLookup)
        {
            Collection<TVStation> stations = processChannels();
            if (stations != null)
                processProgrammes(stations, noLookup);

            return (stations);
        }

        private Collection<TVStation> processChannels()
        {
            if (MxfChannel.Channels == null || MxfChannel.Channels.Count == 0)
                return (null);

            Collection<TVStation> stations = new Collection<TVStation>();

            foreach (MxfChannel channel in MxfChannel.Channels)
            {
                MxfService service = MxfService.FindService(channel.Service);

                if (service != null)
                {
                    TVStation station = new TVStation(service.Name);
                    int[] serviceIds = service.GetServiceIds();
                    if (serviceIds != null)
                    {
                        station.OriginalNetworkID = serviceIds[0];
                        station.TransportStreamID = serviceIds[1];
                        station.ServiceID = serviceIds[2];

                        if (channel.Number != null)
                            station.LogicalChannelNumber = ImportFileBase.GetNumber(null, channel.Number);

                        stations.Add(station);
                    }
                }
            }

            return (stations);
        }

        private void processProgrammes(Collection<TVStation> stations, bool noLookup)
        {
            if (MxfScheduleEntry.ScheduleEntries == null || MxfScheduleEntry.ScheduleEntries.Count == 0)
                return;

            int created = 0;
            int noStation = 0;

            foreach (MxfScheduleEntry scheduleEntry in MxfScheduleEntry.ScheduleEntries)
            {
                MxfService service = MxfService.FindService(scheduleEntry.Service);
                if (service != null)
                {
                    int[] ids = service.GetServiceIds();
                    if (ids != null)
                    {
                        TVStation station = findStation(stations, ids[0], ids[1], ids[2]);
                        if (station != null)
                        {
                            processProgramme(station, scheduleEntry, noLookup);
                            created++;
                        }
                        else
                            noStation++;
                    }
                    else
                        noStation++;
                }
                else
                    noStation++;
            }

            Logger.Instance.Write("EPG entries created = " + created + " MXF schedule entries ignored = " + noStation);
        }

        private TVStation findStation(Collection<TVStation> stations, int originalNetworkId, int transportStreamId, int serviceId)
        {
            foreach (TVStation station in stations)
            {
                if (station.OriginalNetworkID == originalNetworkId &&
                    station.TransportStreamID == transportStreamId &&
                    station.ServiceID == serviceId)
                    return (station);
            }

            return (null);
        }

        private void processProgramme(TVStation station, MxfScheduleEntry scheduleEntry, bool noLookup)
        {
            MxfProgramme programme = MxfProgramme.FindProgramme(scheduleEntry.Program);
            if (programme == null)
                return;

            if (station.EPGCollection == null)
                station.EPGCollection = new Collection<EPGEntry>();

            EPGEntry epgEntry = new EPGEntry();

            epgEntry.OriginalNetworkID = station.OriginalNetworkID;
            epgEntry.TransportStreamID = station.TransportStreamID;
            epgEntry.ServiceID = station.ServiceID;

            epgEntry.EventName = programme.Title;
            epgEntry.ShortDescription = programme.Description;            
            epgEntry.StartTime = DateTime.Parse(scheduleEntry.StartTime).ToLocalTime();

            TimeSpan? duration = scheduleEntry.GetDuration();
            if (duration == null)
                return;
            epgEntry.Duration = duration.Value;

            epgEntry.EventSubTitle = programme.EpisodeTitle;
            epgEntry.HasAdult = programme.HasAdult != null && programme.HasAdult == "1";
            epgEntry.HasGraphicLanguage = programme.HasGraphicLanguage != null && programme.HasGraphicLanguage == "1";
            epgEntry.HasGraphicViolence = programme.HasGraphicViolence != null && programme.HasGraphicViolence == "1";
            epgEntry.HasNudity = programme.HasNudity != null && programme.HasNudity == "1";
            epgEntry.HasStrongSexualContent = programme.HasStrongSexualContent != null && programme.HasStrongSexualContent == "1";

            if (programme.MpaaRating != null)
            {
                switch (programme.MpaaRating)
                {
                    case "1":
                        epgEntry.MpaaParentalRating = "G";
                        break;
                    case "2":
                        epgEntry.MpaaParentalRating = "PG";
                        break;
                    case "3":
                        epgEntry.MpaaParentalRating = "PG13";
                        break;
                    case "4":
                        epgEntry.MpaaParentalRating = "R";
                        break;
                    case "5":
                        epgEntry.MpaaParentalRating = "NC17";
                        break;
                    case "6":
                        epgEntry.MpaaParentalRating = "X";
                        break;
                    case "7":
                        epgEntry.MpaaParentalRating = "NR";
                        break;
                    case "8":
                        epgEntry.MpaaParentalRating = "AO";
                        break;
                    default:
                        break;
                }
            }

            if (programme.KeyWords != null)
            {
                string[] keywords = programme.KeyWords.Split(new char[] { ',' });
                if (keywords.Length != 0)
                {
                    StringBuilder keywordString = new StringBuilder();

                    foreach (string keyword in keywords)
                    {
                        MxfKeyword mxfKeyword = MxfKeyword.FindKeyword(keyword);
                        if (mxfKeyword != null)
                        {
                            if (keywordString.Length != 0)
                                keywordString.Append(",");
                            keywordString.Append(mxfKeyword.Word);
                        }
                    }

                    if (programme.IsMovie == "1")
                        keywordString.Append(",isMovie");
                    if (programme.IsSpecial == "1")
                        keywordString.Append(",isSpecial");
                    if (programme.IsSports == "1")
                        keywordString.Append(",isSports");
                    if (programme.IsNews == "1")
                        keywordString.Append(",isNews");
                    if (programme.IsKids == "1")
                        keywordString.Append(",isKids");

                    if (keywordString.Length != 0)
                        epgEntry.EventCategory = keywordString.ToString();
                }
            }

            epgEntry.Date = programme.Year;

            if (programme.SeasonNumber != null)
                epgEntry.SeasonNumber = ImportFileBase.GetNumber(null, programme.SeasonNumber);
            if (programme.EpisodeNumber != null)
                epgEntry.EpisodeNumber = ImportFileBase.GetNumber(null, programme.EpisodeNumber);

            if (programme.OriginalAirDate != null)
                epgEntry.PreviousPlayDate = DateTime.Parse(programme.OriginalAirDate);

            if (programme.HalfStars != null)
            {
                switch (programme.HalfStars)
                {
                    case "1":
                        epgEntry.StarRating = "+";
                        break;
                    case "2":
                        epgEntry.StarRating = "*";
                        break;
                    case "3":
                        epgEntry.StarRating = "*+";
                        break;
                    case "4":
                        epgEntry.StarRating = "**";
                        break;
                    case "5":
                        epgEntry.StarRating = "**+";
                        break;
                    case "6":
                        epgEntry.StarRating = "***";
                        break;
                    case "7":
                        epgEntry.StarRating = "***+";
                        break;
                    case "8":
                        epgEntry.StarRating = "****";
                        break;
                    default:
                        break;
                }
            }

            if (programme.GuideImage != null)
                epgEntry.Poster = new Guid(programme.GuideImage.Substring(2));

            if (programme.Actors != null && programme.Actors.Count != 0)
            {
                foreach (string actor in programme.Actors)
                {
                    MxfPerson person = MxfPerson.FindPerson(actor);
                    if (person != null)
                    {
                        if (epgEntry.Cast == null)
                            epgEntry.Cast = new Collection<string>();
                        epgEntry.Cast.Add(person.Name);
                    }
                }
            }

            if (programme.Directors != null && programme.Directors.Count != 0)
            {
                foreach (string director in programme.Directors)
                {
                    MxfPerson person = MxfPerson.FindPerson(director);
                    if (person != null)
                    {
                        if (epgEntry.Directors == null)
                            epgEntry.Directors = new Collection<string>();
                        epgEntry.Directors.Add(person.Name);
                    }
                }
            }

            if (programme.Producers != null && programme.Producers.Count != 0)
            { 
                foreach (string producer in programme.Producers)
                {
                    MxfPerson person = MxfPerson.FindPerson(producer);
                    if (person != null)
                    {
                        if (epgEntry.Producers == null)
                            epgEntry.Producers = new Collection<string>();
                        epgEntry.Producers.Add(person.Name);
                    }
                }
            }

            if (programme.Writers != null && programme.Writers.Count != 0)
            {
                foreach (string writer in programme.Writers)
                {
                    MxfPerson person = MxfPerson.FindPerson(writer);
                    if (person != null)
                    {
                        if (epgEntry.Writers == null)
                            epgEntry.Writers = new Collection<string>();
                        epgEntry.Writers.Add(person.Name);
                    }
                }
            }

            if (programme.Hosts != null && programme.Hosts.Count != 0)
            {
                foreach (string host in programme.Hosts)
                {
                    MxfPerson person = MxfPerson.FindPerson(host);
                    if (person != null)
                    {
                        if (epgEntry.Presenters == null)
                            epgEntry.Presenters = new Collection<string>();
                        epgEntry.Writers.Add(person.Name);
                    }
                }
            }

            if (programme.GuestStars != null && programme.GuestStars.Count != 0)
            {
                foreach (string guestStar in programme.GuestStars)
                {
                    MxfPerson person = MxfPerson.FindPerson(guestStar);
                    if (person != null)
                    {
                        if (epgEntry.GuestStars == null)
                            epgEntry.GuestStars = new Collection<string>();
                        epgEntry.GuestStars.Add(person.Name);
                    }
                }
            }

            epgEntry.NoLookup = noLookup;

            addEPGEntry(station, epgEntry);
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

        /// <summary>
        /// Process the channel information from an MXF file.
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
                return ("Failed to open " + fileName);
            }

            try
            {
                while (!xmlReader.EOF)
                {
                    xmlReader.Read();
                    if (xmlReader.IsStartElement())
                    {
                        switch (xmlReader.Name.ToLowerInvariant())
                        {
                            case "service":
                                MxfService service = MxfService.GetInstance(xmlReader);

                                if (MxfService.Services == null)
                                    MxfService.Services = new Collection<MxfService>();

                                MxfService.Services.Add(service);

                                break;
                            case "channel":
                                MxfChannel channel = MxfChannel.GetInstance(xmlReader);

                                if (MxfChannel.Channels == null)
                                    MxfChannel.Channels = new Collection<MxfChannel>();

                                MxfChannel.Channels.Add(channel);

                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (XmlException e)
            {
                return ("Failed to load mxf file: " + e.Message);
            }
            catch (IOException e)
            {
                return ("Failed to load mxf file: " + e.Message);
            }

            if (xmlReader != null)
                xmlReader.Close();

            return (null);
        }

        /// <summary>
        /// Clear the MXF data.
        /// </summary>
        public static void Clear()
        {
            if (MxfKeyword.Keywords != null)
                MxfKeyword.Keywords.Clear();

            if (MxfGuideImage.GuideImages != null)
                MxfGuideImage.GuideImages.Clear();

            if (MxfSeriesInfo.SeriesInfos != null)
                MxfSeriesInfo.SeriesInfos.Clear();

            if (MxfPerson.People != null)
                MxfPerson.People.Clear();

            if (MxfService.Services != null)
                MxfService.Services.Clear();

            if (MxfChannel.Channels != null)
                MxfChannel.Channels.Clear();

            if (MxfProgramme.Programmes != null)
                MxfProgramme.Programmes.Clear();

            if (MxfScheduleEntry.ScheduleEntries != null)
                MxfScheduleEntry.ScheduleEntries.Clear();
        }
    }
}
