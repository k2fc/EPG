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
using System.IO;
using System.Text;
using System.Xml;
using System.Diagnostics;
using System.Threading;
using System.Reflection;

namespace DomainObjects
{
    /// <summary>
    /// The class that creates an MXF file for import to 7MC.
    /// </summary>
    public sealed class OutputFileMXF
    {
        private static string actualFileName;

        private static Collection<KeywordGroup> groups;
        private static Collection<string> people;
        private static Collection<string> series;
        private static Collection<int> stationImages;
        private static Collection<Guid> programImages;
        private static Collection<string> duplicateStationNames;
        
        private static bool isSpecial;
        private static bool isMovie;
        private static bool isSports;
        private static bool isKids;
        private static bool isNews;

        private static string importName;
        private static string importReference;

        private static Process importProcess;
        private static bool importExited;

        private static Collection<string> programIdentifiers;
        private static Collection<string> programIdentifierTitles;

        private static string mcstoreVersion;
        private static string mcstorePublicKey;

        private static string mcepgVersion;
        private static string mcepgPublicKey;

        private OutputFileMXF() { }

        /// <summary>
        /// Create the MXF file.
        /// </summary>
        /// <returns>An error message if the process fails; null otherwise.</returns>
        public static string Process()
        {
            mcepgVersion = getAssemblyVersion("mcepg.dll");
            if (string.IsNullOrWhiteSpace(mcepgVersion))
                return ("Failed to get the assembly version for mcpeg.dll");
 
            mcepgPublicKey = getAssemblyPublicKey("mcepg.dll");
            if (string.IsNullOrWhiteSpace(mcepgPublicKey))
                return ("Failed to get the public key for mcpeg.dll");
            
            mcstoreVersion = getAssemblyVersion("mcstore.dll");
            if (string.IsNullOrWhiteSpace(mcstoreVersion))
                return ("Failed to get the assembly version for mcstore.dll");

            mcstorePublicKey = getAssemblyPublicKey("mcstore.dll");
            if (string.IsNullOrWhiteSpace(mcstorePublicKey))
                return ("Failed to get the public key for mcstore.dll");

            actualFileName = Path.Combine(RunParameters.DataDirectory, "TVGuide.mxf");

            try
            {
                Logger.Instance.Write("Deleting any existing version of output file");
                File.SetAttributes(actualFileName, FileAttributes.Normal);
                File.Delete(actualFileName);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("File delete exception: " + e.Message);
            }

            if (RunParameters.Instance.WMCImportName != null)
                importName = RunParameters.Instance.WMCImportName;

            if (importName == null)
                importName = "EPG Collector";
            importReference = importName.Replace(" ", string.Empty);
            Logger.Instance.Write("Import name set to '" + importName + "'");

            duplicateStationNames = new Collection<string>();
            foreach (TVStation station in RunParameters.Instance.StationCollection)
            {
                if (station.Included)
                {
                    int occurrences = countStationName(station);
                    if (occurrences > 1)
                    {
                        if (!duplicateStationNames.Contains(station.Name))
                            duplicateStationNames.Add(station.Name);
                    }
                }
            }

            Logger.Instance.Write("Creating output file: " + actualFileName);

            XmlWriterSettings settings = new XmlWriterSettings();

            settings.Indent = true;
            settings.NewLineOnAttributes = false;
            if (!OutputFile.UseUnicodeEncoding)
                settings.Encoding = new UTF8Encoding();
            else
                settings.Encoding = new UnicodeEncoding();
            settings.CloseOutput = true;

            try
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(actualFileName, settings))
                {
                    xmlWriter.WriteStartDocument();

                    xmlWriter.WriteStartElement("MXF");
                    xmlWriter.WriteAttributeString("xmlns", "sql", null, "urn:schemas-microsoft-com:XML-sql");
                    xmlWriter.WriteAttributeString("xmlns", "xsi", null, @"http://www.w3.org/2001/XMLSchema-instance");

                    xmlWriter.WriteStartElement("Assembly");
                    xmlWriter.WriteAttributeString("name", "mcepg");
                    xmlWriter.WriteAttributeString("version", mcepgVersion);

                    xmlWriter.WriteAttributeString("cultureInfo", "");
                    xmlWriter.WriteAttributeString("publicKey", mcepgPublicKey);
                    xmlWriter.WriteStartElement("NameSpace");
                    xmlWriter.WriteAttributeString("name", "Microsoft.MediaCenter.Guide");

                    xmlWriter.WriteStartElement("Type");
                    xmlWriter.WriteAttributeString("name", "Lineup");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Type");
                    xmlWriter.WriteAttributeString("name", "Channel");
                    xmlWriter.WriteAttributeString("parentFieldName", "lineup");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Type");
                    xmlWriter.WriteAttributeString("name", "Service");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Type");
                    xmlWriter.WriteAttributeString("name", "ScheduleEntry");
                    xmlWriter.WriteAttributeString("groupName", "ScheduleEntries");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Type");
                    xmlWriter.WriteAttributeString("name", "Program");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Type");
                    xmlWriter.WriteAttributeString("name", "Keyword");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Type");
                    xmlWriter.WriteAttributeString("name", "KeywordGroup");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Type");
                    xmlWriter.WriteAttributeString("name", "Person");
                    xmlWriter.WriteAttributeString("groupName", "People");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Type");
                    xmlWriter.WriteAttributeString("name", "ActorRole");
                    xmlWriter.WriteAttributeString("parentFieldName", "program");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Type");
                    xmlWriter.WriteAttributeString("name", "DirectorRole");
                    xmlWriter.WriteAttributeString("parentFieldName", "program");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Type");
                    xmlWriter.WriteAttributeString("name", "WriterRole");
                    xmlWriter.WriteAttributeString("parentFieldName", "program");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Type");
                    xmlWriter.WriteAttributeString("name", "HostRole");
                    xmlWriter.WriteAttributeString("parentFieldName", "program");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Type");
                    xmlWriter.WriteAttributeString("name", "GuestActorRole");
                    xmlWriter.WriteAttributeString("parentFieldName", "program");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Type");
                    xmlWriter.WriteAttributeString("name", "ProducerRole");
                    xmlWriter.WriteAttributeString("parentFieldName", "program");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Type");
                    xmlWriter.WriteAttributeString("name", "GuideImage");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Type");
                    xmlWriter.WriteAttributeString("name", "Affiliate");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Type");
                    xmlWriter.WriteAttributeString("name", "SeriesInfo");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Type");
                    xmlWriter.WriteAttributeString("name", "Season");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Assembly");
                    xmlWriter.WriteAttributeString("name", "mcstore");
                    xmlWriter.WriteAttributeString("version", mcstoreVersion);

                    xmlWriter.WriteAttributeString("cultureInfo", "");
                    xmlWriter.WriteAttributeString("publicKey", mcstorePublicKey);
                    xmlWriter.WriteStartElement("NameSpace");
                    xmlWriter.WriteAttributeString("name", "Microsoft.MediaCenter.Store");

                    xmlWriter.WriteStartElement("Type");
                    xmlWriter.WriteAttributeString("name", "Provider");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Type");
                    xmlWriter.WriteAttributeString("name", "UId");
                    xmlWriter.WriteAttributeString("parentFieldName", "target");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Providers");
                    xmlWriter.WriteStartElement("Provider");
                    xmlWriter.WriteAttributeString("id", "provider1");
                    xmlWriter.WriteAttributeString("name", importReference);
                    xmlWriter.WriteAttributeString("displayName", importName);
                    xmlWriter.WriteAttributeString("copyright", "");
                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("With");
                    xmlWriter.WriteAttributeString("provider", "provider1");

                    xmlWriter.WriteStartElement("Keywords");
                    processKeywords(xmlWriter);
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("KeywordGroups");
                    processKeywordGroups(xmlWriter);
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("GuideImages");
                    processGuideImages(xmlWriter);
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("People");
                    processPeople(xmlWriter);
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("SeriesInfos");
                    processSeries(xmlWriter);
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Seasons");
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Programs");
                    processPrograms(xmlWriter);
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Affiliates");
                    processAffiliates(xmlWriter);
                    xmlWriter.WriteEndElement();

                    xmlWriter.WriteStartElement("Services");
                    processServices(xmlWriter);
                    xmlWriter.WriteEndElement();

                    processSchedules(xmlWriter);

                    xmlWriter.WriteStartElement("Lineups");
                    processLineUps(xmlWriter);
                    xmlWriter.WriteEndElement();

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

            string reply = runImportUtility(actualFileName);
            if (reply != null)
                return (reply);

            if (OptionEntry.IsDefined(OptionName.CreateBrChannels))
                OutputFileBladeRunner.Process(actualFileName);
            if (OptionEntry.IsDefined(OptionName.CreateArChannels))
                OutputFileAreaRegionChannels.Process(actualFileName);
            if (OptionEntry.IsDefined(OptionName.CreateSageTvFrq))
                OutputFileSageTVFrq.Process(actualFileName);

            return (null);
        }

        private static int countStationName(TVStation station)
        {
            int count = 0;

            foreach (TVStation existingStation in RunParameters.Instance.StationCollection)
            {
                if (existingStation.Included && station.Name == existingStation.Name)
                    count++;
            }

            return (count);
        }

        private static void processKeywords(XmlWriter xmlWriter)
        {
            groups = new Collection<KeywordGroup>();
            groups.Add(new KeywordGroup("General"));

            foreach (TVStation station in RunParameters.Instance.StationCollection)
            {
                if (station.Included)
                {
                    foreach (EPGEntry epgEntry in station.EPGCollection)
                    {
                        if (epgEntry.EventCategory != null)
                            processCategory(xmlWriter, groups, epgEntry.EventCategory); 
                    }
                }
            }

            foreach (KeywordGroup group in groups)
            {
                xmlWriter.WriteStartElement("Keyword");
                xmlWriter.WriteAttributeString("id", "k" + ((groups.IndexOf(group) + 1)));
                xmlWriter.WriteAttributeString("word", group.Name.Trim());
                xmlWriter.WriteEndElement();

                foreach (string keyword in group.Keywords)
                {
                    xmlWriter.WriteStartElement("Keyword");
                    xmlWriter.WriteAttributeString("id", "k" + (((groups.IndexOf(group) + 1) * 100) + group.Keywords.IndexOf(keyword)));
                    xmlWriter.WriteAttributeString("word", keyword.Trim());
                    xmlWriter.WriteEndElement();
                }
            }
        }

        private static void processCategory(XmlWriter xmlWriter, Collection<KeywordGroup> groups, string category)
        {
            string[] parts = removeSpecialCategories(category);
            if (parts == null)
                return;

            if (parts.Length == 1)
            {                
                foreach (KeywordGroup keywordGroup in groups)
                {
                    if (keywordGroup.Name == parts[0])
                        return;
                }

                KeywordGroup singleGroup = new KeywordGroup(parts[0]);
                singleGroup.Keywords.Add("All");
                singleGroup.Keywords.Add("General");
                groups.Add(singleGroup);
                return;
            }

            foreach (KeywordGroup group in groups)
            {
                if (group.Name == parts[0])
                {
                    for (int index = 1; index < parts.Length; index++)
                    {
                        bool keywordFound = false;

                        foreach (string keyword in group.Keywords)
                        {
                            if (keyword == parts[index])
                                keywordFound = true;
                        }

                        if (!keywordFound)
                        {
                            if (group.Keywords.Count == 0)
                                group.Keywords.Add("All");
                            group.Keywords.Add(parts[index]);
                        }
                    }

                    return;
                }
            }

            KeywordGroup newGroup = new KeywordGroup(parts[0]);
            newGroup.Keywords.Add("All");

            for (int partAddIndex = 1; partAddIndex < parts.Length; partAddIndex++)
                newGroup.Keywords.Add(parts[partAddIndex]);

            groups.Add(newGroup);            
        }

        private static string[] removeSpecialCategories(string category)
        {
            string[] parts = category.Split(new string[] { "," }, StringSplitOptions.None);

            int specialCategoryCount = 0;

            foreach (string part in parts)
            {
                string specialCategory = getSpecialCategory(part);
                if (specialCategory != null)
                    specialCategoryCount++;
            }

            if (specialCategoryCount == parts.Length)
                return (null);

            string[] editedParts = new string[parts.Length - specialCategoryCount];
            int index = 0;

            foreach (string part in parts)
            {
                string specialCategory = getSpecialCategory(part);
                if (specialCategory == null)
                {
                    editedParts[index] = part;
                    index++;
                }

            }

            return (editedParts);
        }

        private static void processKeywordGroups(XmlWriter xmlWriter)
        {
            int groupNumber = 1;

            foreach (KeywordGroup group in groups)
            {
                xmlWriter.WriteStartElement("KeywordGroup");
                xmlWriter.WriteAttributeString("uid", "!KeywordGroup!k-" + group.Name.ToLowerInvariant().Replace(' ', '-'));
                xmlWriter.WriteAttributeString("groupName", "k" + groupNumber);

                StringBuilder keywordString = new StringBuilder();
                int keywordNumber = 0;

                foreach (string keyword in group.Keywords)
                {
                    if (keywordString.Length != 0)
                        keywordString.Append(",");
                    keywordString.Append("k" + ((groupNumber * 100) + keywordNumber));

                    keywordNumber++;
                }

                xmlWriter.WriteAttributeString("keywords", keywordString.ToString());
                xmlWriter.WriteEndElement();

                groupNumber++;
            }
        }

        private static void processGuideImages(XmlWriter xmlWriter)
        {
            string stationDirectory = Path.Combine(RunParameters.DataDirectory, "Images") + Path.DirectorySeparatorChar;
            
            if (Directory.Exists(stationDirectory))
            {
                stationImages = new Collection<int>();

                DirectoryInfo directoryInfo = new DirectoryInfo(stationDirectory);

                foreach (FileInfo fileInfo in directoryInfo.GetFiles())
                {
                    if (fileInfo.Extension.ToLowerInvariant() == ".png")
                    {
                        string serviceID = fileInfo.Name.Remove(fileInfo.Name.Length - 4);

                        try
                        {
                            stationImages.Add(Int32.Parse(serviceID));

                            xmlWriter.WriteStartElement("GuideImage");
                            xmlWriter.WriteAttributeString("id", "i" + stationImages.Count);
                            xmlWriter.WriteAttributeString("uid", "!Image!SID" + serviceID);
                            xmlWriter.WriteAttributeString("imageUrl", "file://" + fileInfo.FullName);
                            xmlWriter.WriteEndElement();
                        }
                        catch (FormatException) { }
                        catch (OverflowException) { }                        
                    }
                }
            }

            if (!RunParameters.Instance.LookupImagesInBase)
            {
                addLookupImages(xmlWriter, Path.Combine(RunParameters.ImagePath, "Movies"));
                addLookupImages(xmlWriter, Path.Combine(RunParameters.ImagePath, "TV Series"));
            }
            else
                addLookupImages(xmlWriter, Path.Combine(RunParameters.ImagePath));
        }

        private static void addLookupImages(XmlWriter xmlWriter, string directory)
        {
            if (Directory.Exists(directory))
            {
                if (programImages == null)
                    programImages = new Collection<Guid>();

                DirectoryInfo directoryInfo = new DirectoryInfo(directory);

                foreach (FileInfo fileInfo in directoryInfo.GetFiles())
                {
                    if (fileInfo.Extension.ToLowerInvariant() == ".jpg")
                    {
                        Guid guid = Guid.Parse(fileInfo.Name.Substring(0, fileInfo.Name.Length - 4));

                        try
                        {
                            programImages.Add(guid);

                            xmlWriter.WriteStartElement("GuideImage");
                            xmlWriter.WriteAttributeString("id", "i-" + guid);
                            xmlWriter.WriteAttributeString("uid", "!Image!" + guid);
                            xmlWriter.WriteAttributeString("imageUrl", "file://" + fileInfo.FullName);
                            xmlWriter.WriteEndElement();
                        }
                        catch (FormatException) { }
                        catch (OverflowException) { }
                    }
                }
            }
        }

        private static void processPeople(XmlWriter xmlWriter)
        {
            people = new Collection<string>();

            foreach (TVStation station in RunParameters.Instance.StationCollection)
            {
                if (station.Included)
                {
                    foreach (EPGEntry epgEntry in station.EPGCollection)
                    {
                        if (epgEntry.Cast != null)
                        {
                            foreach (string person in epgEntry.Cast)
                                processPerson(xmlWriter, people, person);
                        }

                        if (epgEntry.Directors != null)
                        {
                            foreach (string person in epgEntry.Directors)
                                processPerson(xmlWriter, people, person);
                        }

                        if (epgEntry.Producers != null)
                        {
                            foreach (string person in epgEntry.Producers)
                                processPerson(xmlWriter, people, person);
                        }

                        if (epgEntry.Writers != null)
                        {
                            foreach (string person in epgEntry.Writers)
                                processPerson(xmlWriter, people, person);
                        }

                        if (epgEntry.Presenters != null)
                        {
                            foreach (string person in epgEntry.Presenters)
                                processPerson(xmlWriter, people, person);
                        }

                        if (epgEntry.GuestStars != null)
                        {
                            foreach (string person in epgEntry.GuestStars)
                                processPerson(xmlWriter, people, person);
                        }
                    }
                }
            }
        }

        private static void processPerson(XmlWriter xmlWriter, Collection<string> people, string newPerson)
        {
            string trimPerson = newPerson.Trim();

            foreach (string existingPerson in people)
            {
                if (existingPerson == trimPerson)
                    return;
            }

            people.Add(trimPerson);

            xmlWriter.WriteStartElement("Person");
            xmlWriter.WriteAttributeString("id", "prs" + people.Count);
            xmlWriter.WriteAttributeString("name", trimPerson);
            xmlWriter.WriteAttributeString("uid", "!Person!" + trimPerson);
            xmlWriter.WriteEndElement();
        }

        private static void processSeries(XmlWriter xmlWriter)
        {
            series = new Collection<string>();

            foreach (TVStation station in RunParameters.Instance.StationCollection)
            {
                if (station.Included)
                {
                    foreach (EPGEntry epgEntry in station.EPGCollection)
                    {
                        string seriesLink = processEpisode(xmlWriter, series, epgEntry);
                        if (seriesLink != null)
                        {
                            xmlWriter.WriteStartElement("SeriesInfo");
                            xmlWriter.WriteAttributeString("id", "si" + series.Count);
                            xmlWriter.WriteAttributeString("uid", "!Series!" + seriesLink);
                            xmlWriter.WriteAttributeString("title", epgEntry.EventName);
                            xmlWriter.WriteAttributeString("shortTitle", epgEntry.EventName);

                            if (epgEntry.SeriesDescription != null)
                            {
                                xmlWriter.WriteAttributeString("description", epgEntry.SeriesDescription);
                                xmlWriter.WriteAttributeString("shortDescription", epgEntry.SeriesDescription);
                            }
                            else
                            {
                                xmlWriter.WriteAttributeString("description", epgEntry.EventName);
                                xmlWriter.WriteAttributeString("shortDescription", epgEntry.EventName);
                            }

                            if (epgEntry.SeriesStartDate != null)
                                xmlWriter.WriteAttributeString("startAirdate", convertDateTimeToString(epgEntry.SeriesStartDate.Value));
                            else
                                xmlWriter.WriteAttributeString("startAirdate", convertDateTimeToString(DateTime.MinValue));

                            if (epgEntry.SeriesEndDate != null)
                                xmlWriter.WriteAttributeString("endAirdate", convertDateTimeToString(epgEntry.SeriesEndDate.Value));
                            else
                                xmlWriter.WriteAttributeString("endAirdate", convertDateTimeToString(DateTime.MinValue));
                            
                            setGuideImage(xmlWriter, epgEntry);
       
                            xmlWriter.WriteEndElement();
                        }
                    }
                }
            }
        }

        private static string processEpisode(XmlWriter xmlWriter, Collection<string> series, EPGEntry epgEntry)
        {
            string newSeriesLink = getSeriesLink(epgEntry);
            if (newSeriesLink == null)
                return (null);

            foreach (string oldSeriesLink in series)
            {
                if (oldSeriesLink == newSeriesLink)
                    return (null);
            }

            series.Add(newSeriesLink);

            return (newSeriesLink);
        }

        private static void processPrograms(XmlWriter xmlWriter)
        {
            if (OptionEntry.IsDefined(OptionName.UseWmcRepeatCheck) || OptionEntry.IsDefined(OptionName.UseWmcRepeatCheckBroadcast))
            {
                programIdentifiers = new Collection<string>();
                programIdentifierTitles = new Collection<string>();
            }

            int programNumber = 1;

            foreach (TVStation station in RunParameters.Instance.StationCollection)
            {
                if (station.Included)
                {
                    foreach (EPGEntry epgEntry in station.EPGCollection)
                    {                        
                        if (processProgram(xmlWriter, programNumber, epgEntry))
                            programNumber++;
                    }
                }
            }
        }

        private static bool processProgram(XmlWriter xmlWriter, int programNumber, EPGEntry epgEntry)
        {
            string uniqueID = getProgramIdentifier(epgEntry);
            if (uniqueID == null)
                return (false);

            isSpecial = false;
            isMovie = false;
            isSports = false;
            isKids = false;
            isNews = false;

            xmlWriter.WriteStartElement("Program");
            xmlWriter.WriteAttributeString("id", "prg" + programNumber);
            xmlWriter.WriteAttributeString("uid", "!Program!" + uniqueID);

            if (epgEntry.EventName != null)
                xmlWriter.WriteAttributeString("title", epgEntry.EventName);
            else
                xmlWriter.WriteAttributeString("title", "No Title");

            if (epgEntry.ShortDescription != null)
                xmlWriter.WriteAttributeString("description", epgEntry.ShortDescription);
            else
            {
                if (epgEntry.EventName != null)
                    xmlWriter.WriteAttributeString("description", epgEntry.EventName);
                else
                    xmlWriter.WriteAttributeString("description", "No Description");
            }
            
            if (epgEntry.EventSubTitle != null)
                xmlWriter.WriteAttributeString("episodeTitle", epgEntry.EventSubTitle);

            if (epgEntry.HasAdult)
                xmlWriter.WriteAttributeString("hasAdult", "1");
            if (epgEntry.HasGraphicLanguage)
                xmlWriter.WriteAttributeString("hasGraphicLanguage", "1");
            if (epgEntry.HasGraphicViolence)
                xmlWriter.WriteAttributeString("hasGraphicViolence", "1");
            if (epgEntry.HasNudity)
                xmlWriter.WriteAttributeString("hasNudity", "1");
            if (epgEntry.HasStrongSexualContent)
                xmlWriter.WriteAttributeString("hasStrongSexualContent", "1");

            if (epgEntry.MpaaParentalRating != null)
            {
                switch (epgEntry.MpaaParentalRating)
                {
                    case "G":
                        xmlWriter.WriteAttributeString("mpaaRating", "1");
                        break;
                    case "PG":
                        xmlWriter.WriteAttributeString("mpaaRating", "2");
                        break;
                    case "PG13":
                        xmlWriter.WriteAttributeString("mpaaRating", "3");
                        break;
                    case "R":
                        xmlWriter.WriteAttributeString("mpaaRating", "4");
                        break;
                    case "NC17":
                        xmlWriter.WriteAttributeString("mpaaRating", "5");
                        break;
                    case "X":
                        xmlWriter.WriteAttributeString("mpaaRating", "6");
                        break;
                    case "NR":
                        xmlWriter.WriteAttributeString("mpaaRating", "7");
                        break;
                    case "AO":
                        xmlWriter.WriteAttributeString("mpaaRating", "8");
                        break;
                    default:
                        break;
                }
            }

            processCategoryKeywords(xmlWriter, epgEntry.EventCategory);

            if (epgEntry.Date != null)
                xmlWriter.WriteAttributeString("year", epgEntry.Date);

            if (epgEntry.SeasonNumber != -1)
                xmlWriter.WriteAttributeString("seasonNumber", epgEntry.SeasonNumber.ToString());
            if (epgEntry.EpisodeNumber != -1)
                xmlWriter.WriteAttributeString("episodeNumber", epgEntry.EpisodeNumber.ToString());

            if (!OptionEntry.IsDefined(OptionName.UseWmcRepeatCheck) && !OptionEntry.IsDefined(OptionName.UseWmcRepeatCheckBroadcast))
                xmlWriter.WriteAttributeString("originalAirdate", convertDateTimeToString(epgEntry.PreviousPlayDate));                
            else
                xmlWriter.WriteAttributeString("originalAirdate", convertDateTimeToString(epgEntry.StartTime));

            processSeries(xmlWriter, epgEntry);

            if (epgEntry.StarRating != null)
            {
                switch (epgEntry.StarRating)
                {
                    case "+":
                        xmlWriter.WriteAttributeString("halfStars", "1");
                        break;
                    case "*":
                        xmlWriter.WriteAttributeString("halfStars", "2");
                        break;
                    case "*+":
                        xmlWriter.WriteAttributeString("halfStars", "3");
                        break;
                    case "**":
                        xmlWriter.WriteAttributeString("halfStars", "4");
                        break;
                    case "**+":
                        xmlWriter.WriteAttributeString("halfStars", "5");
                        break;
                    case "***":
                        xmlWriter.WriteAttributeString("halfStars", "6");
                        break;
                    case "***+":
                        xmlWriter.WriteAttributeString("halfStars", "7");
                        break;
                    case "****":
                        xmlWriter.WriteAttributeString("halfStars", "8");
                        if (OptionEntry.IsDefined(OptionName.WmcStarSpecial))
                            isSpecial = true;
                        break;
                    default:
                        break;
                }
            }

            processCategoryAttributes(xmlWriter);
            setGuideImage(xmlWriter, epgEntry);

            if (epgEntry.Cast != null && epgEntry.Cast.Count != 0)
                processCast(xmlWriter, epgEntry.Cast);

            if (epgEntry.Directors != null && epgEntry.Directors.Count != 0)
                processDirectors(xmlWriter, epgEntry.Directors);

            if (epgEntry.Producers != null && epgEntry.Producers.Count != 0)
                processProducers(xmlWriter, epgEntry.Producers);

            if (epgEntry.Writers != null && epgEntry.Writers.Count != 0)
                processWriters(xmlWriter, epgEntry.Writers);

            if (epgEntry.Presenters != null && epgEntry.Presenters.Count != 0)
                processPresenters(xmlWriter, epgEntry.Presenters);

            if (epgEntry.GuestStars != null && epgEntry.GuestStars.Count != 0)
                processGuestStars(xmlWriter, epgEntry.GuestStars);

            xmlWriter.WriteEndElement();

            return (true);
        }

        private static string getProgramIdentifier(EPGEntry epgEntry)
        {
            if (!OptionEntry.IsDefined(OptionName.UseWmcRepeatCheck) && !OptionEntry.IsDefined(OptionName.UseWmcRepeatCheckBroadcast))
                return (epgEntry.OriginalNetworkID + ":" +
                        epgEntry.TransportStreamID + ":" +
                        epgEntry.ServiceID +
                        getUtcTime(epgEntry.StartTime).ToString()).Replace(" ", "").Replace(":", "").Replace("/", "").Replace(".", "");                

            string crcString;
            string mode;

            if (OptionEntry.IsDefined(OptionName.UseWmcRepeatCheck))
            {
                crcString = getWmcProgramIdentifier(epgEntry);
                mode = "basic";
            }
            else
            {
                crcString = getWmcProgramIdentifierBroadcast(epgEntry);
                mode = "crids";
            }
            
            epgEntry.UniqueIdentifier = Crc.CalculateCRC(crcString).ToString();

            if (programIdentifiers.Contains(epgEntry.UniqueIdentifier))
            {
                string storedTitle = programIdentifierTitles[programIdentifiers.IndexOf(epgEntry.UniqueIdentifier)];
                if (storedTitle != epgEntry.EventName)
                {
                    Logger.Instance.Write("<e> Duplicate UID generated for '" + storedTitle + "' and '" +
                        epgEntry.EventName + "'");
                }
                return (null);
            }

            if (DebugEntry.IsDefined(DebugName.LogPuids))
                Logger.Instance.Write("Program ID: mode: " + mode + 
                    " uid: " + epgEntry.UniqueIdentifier + 
                    " crc string: " + crcString +
                    " title: " + (string.IsNullOrWhiteSpace(epgEntry.EventName) ? "No title" : epgEntry.EventName));
            
            programIdentifiers.Add(epgEntry.UniqueIdentifier);
            programIdentifierTitles.Add(epgEntry.EventName);
            
            return (epgEntry.UniqueIdentifier);
        }

        private static string getWmcProgramIdentifier(EPGEntry epgEntry)
        {
            if (epgEntry.SeasonNumber != -1 && epgEntry.EpisodeNumber != -1)
                return (epgEntry.EventName + " Season " + epgEntry.SeasonNumber + " Episode " + epgEntry.EpisodeNumber);
            else
                return (epgEntry.EventName + " + " + epgEntry.ShortDescription);            
        }

        private static string getWmcProgramIdentifierBroadcast(EPGEntry epgEntry)
        {
            string crcString;

            if (!string.IsNullOrEmpty(epgEntry.SeasonCrid) || !string.IsNullOrEmpty(epgEntry.EpisodeCrid))
            {
                string seasonCrid = !string.IsNullOrEmpty(epgEntry.SeasonCrid) ? epgEntry.SeasonCrid : "n/a";
                string episodeCrid = !string.IsNullOrEmpty(epgEntry.EpisodeCrid) ? epgEntry.EpisodeCrid : "n/a";

                crcString = "Season CRID " + seasonCrid + " Episode CRID " + episodeCrid;
            }
            else
            {
                if (!string.IsNullOrEmpty(epgEntry.SeriesId) || !string.IsNullOrEmpty(epgEntry.EpisodeId))
                {
                    string seriesId = !string.IsNullOrEmpty(epgEntry.SeriesId) ? epgEntry.SeriesId : "n/a";
                    string episodeId = !string.IsNullOrEmpty(epgEntry.EpisodeId) ? epgEntry.EpisodeId : "n/a";

                    crcString = "Series ID " + seriesId + " Episode ID " + episodeId + " " +
                        (!string.IsNullOrEmpty(epgEntry.ShortDescription) ? epgEntry.ShortDescription : "No Description");
                }
                else
                {
                    if (epgEntry.SeasonNumber != -1 || epgEntry.EpisodeNumber != -1)
                        crcString = epgEntry.EventName + " Season No. " + epgEntry.SeasonNumber + " Episode No. " + epgEntry.EpisodeNumber;
                    else
                        crcString = (!string.IsNullOrEmpty(epgEntry.EventName) ? epgEntry.EventName : "No Name") + " + " +
                            (!string.IsNullOrEmpty(epgEntry.ShortDescription) ? epgEntry.ShortDescription : "No Description") +
                            getUtcTime(epgEntry.StartTime).Date.ToString().Replace(" ", "").Replace(":", "").Replace("/", "").Replace(".", "");
                }
            }

            return (crcString);            
        }

        private static void processCategoryKeywords(XmlWriter xmlWriter, string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                xmlWriter.WriteAttributeString("keywords", "");
                return;
            }

            string[] parts = processSpecialCategories(xmlWriter, category);
            if (parts == null)
            {
                xmlWriter.WriteAttributeString("keywords", "");
                return;
            }

            /*if (parts.Length < 2)
                return;*/

            StringBuilder keywordString = new StringBuilder();    

            int groupNumber = 1;            

            foreach (KeywordGroup group in groups)
            {
                if (group.Name == parts[0])
                {                    
                    keywordString.Append("k" + groupNumber);                    

                    int keywordNumber = groupNumber * 100;

                    if (parts.Length < 2)
                        keywordString.Append(",k" + (keywordNumber + 1));
                    else
                    {
                        for (int keywordIndex = 1; keywordIndex < group.Keywords.Count; keywordIndex++)
                        {
                            keywordNumber++;

                            for (int partsIndex = 1; partsIndex < parts.Length; partsIndex++)
                            {
                                if (group.Keywords[keywordIndex] == parts[partsIndex])
                                    keywordString.Append(",k" + keywordNumber);
                            }
                        }
                    }

                    xmlWriter.WriteAttributeString("keywords", keywordString.ToString());
                    return;
                }
                groupNumber++;
            }

            xmlWriter.WriteAttributeString("keywords", "");
        }

        private static string[] processSpecialCategories(XmlWriter xmlWriter, string category)
        {
            Collection<string> specialCategories = new Collection<string>();

            string[] parts = category.Split(new string[] { "," }, StringSplitOptions.None);

            foreach (string part in parts)
            {
                string specialCategory = getSpecialCategory(part);
                if (specialCategory != null)
                    specialCategories.Add(specialCategory);
            }

            if (specialCategories.Count == parts.Length)
                return (null);

            string[] editedParts = new string[parts.Length - specialCategories.Count];
            int index = 0;

            foreach (string part in parts)
            {
                string specialCategory = getSpecialCategory(part);
                if (specialCategory == null)
                {
                    editedParts[index] = part;
                    index++;
                }

            }

            return (editedParts);
        }

        private static string getSpecialCategory(string category)
        {
            switch (category.ToUpperInvariant())
            {
                case "ISMOVIE":
                    isMovie = true;
                    return ("isMovie");
                case "ISSPECIAL":
                    isSpecial = true;
                    return ("isSpecial");
                case "ISSPORTS":
                    isSports = true;
                    return ("isSports");
                case "ISNEWS":
                    isNews = true;
                    return ("isNews");
                case "ISKIDS":
                    isKids = true;
                    return ("isKids");
                default:
                    return (null);
            }
        }

        private static void addSpecialCategory(Collection<string> specialCategories, string newCategory)
        {
            foreach (string oldCategory in specialCategories)
            {
                if (oldCategory == newCategory)
                    return;
            }

            specialCategories.Add(newCategory);
        }

        private static void processCast(XmlWriter xmlWriter, Collection<string> cast)
        {
            if (people == null)
                return;

            int rank = 1;

            foreach (string actor in cast)
            {
                xmlWriter.WriteStartElement("ActorRole");
                xmlWriter.WriteAttributeString("person", "prs" + (people.IndexOf(actor.Trim()) + 1));
                xmlWriter.WriteAttributeString("rank", rank.ToString());
                xmlWriter.WriteEndElement();

                rank++;
            }
        }

        private static void processDirectors(XmlWriter xmlWriter, Collection<string> directors)
        {
            if (people == null)
                return;

            int rank = 1;

            foreach (string director in directors)
            {
                xmlWriter.WriteStartElement("DirectorRole");
                xmlWriter.WriteAttributeString("person", "prs" + (people.IndexOf(director.Trim()) + 1));
                xmlWriter.WriteAttributeString("rank", rank.ToString());
                xmlWriter.WriteEndElement();

                rank++;
            }
        }

        private static void processProducers(XmlWriter xmlWriter, Collection<string> producers)
        {
            if (people == null)
                return;

            int rank = 1;

            foreach (string producer in producers)
            {
                xmlWriter.WriteStartElement("ProducerRole");
                xmlWriter.WriteAttributeString("person", "prs" + (people.IndexOf(producer.Trim()) + 1));
                xmlWriter.WriteAttributeString("rank", rank.ToString());
                xmlWriter.WriteEndElement();

                rank++;
            }
        }

        private static void processWriters(XmlWriter xmlWriter, Collection<string> writers)
        {
            if (people == null)
                return;

            int rank = 1;

            foreach (string writer in writers)
            {
                xmlWriter.WriteStartElement("WriterRole");
                xmlWriter.WriteAttributeString("person", "prs" + (people.IndexOf(writer.Trim()) + 1));
                xmlWriter.WriteAttributeString("rank", rank.ToString());
                xmlWriter.WriteEndElement();

                rank++;
            }
        }

        private static void processPresenters(XmlWriter xmlWriter, Collection<string> presenters)
        {
            if (people == null)
                return;

            int rank = 1;

            foreach (string presenter in presenters)
            {
                xmlWriter.WriteStartElement("HostRole");
                xmlWriter.WriteAttributeString("person", "prs" + (people.IndexOf(presenter.Trim()) + 1));
                xmlWriter.WriteAttributeString("rank", rank.ToString());
                xmlWriter.WriteEndElement();

                rank++;
            }
        }

        private static void processGuestStars(XmlWriter xmlWriter, Collection<string> guestStars)
        {
            if (people == null)
                return;

            int rank = 1;

            foreach (string guestStar in guestStars)
            {
                xmlWriter.WriteStartElement("GuestActorRole");
                xmlWriter.WriteAttributeString("person", "prs" + (people.IndexOf(guestStar.Trim()) + 1));
                xmlWriter.WriteAttributeString("rank", rank.ToString());
                xmlWriter.WriteEndElement();

                rank++;
            }
        }

        private static void processSeries(XmlWriter xmlWriter, EPGEntry epgEntry)
        {
            string seriesLink = getSeriesLink(epgEntry);
            if (seriesLink == null)
                return;

            foreach (string oldSeriesLink in series)
            {
                if (oldSeriesLink == seriesLink)
                {
                    xmlWriter.WriteAttributeString("isSeries", "1");
                    xmlWriter.WriteAttributeString("series", "si" + (series.IndexOf(oldSeriesLink) + 1).ToString());

                    return;
                }
            }

            xmlWriter.WriteAttributeString("isSeries", "0");
        }

        private static void processCategoryAttributes(XmlWriter xmlWriter)
        {
            if (isSpecial)
                xmlWriter.WriteAttributeString("isSpecial", "1");
            else
                xmlWriter.WriteAttributeString("isSpecial", "0");

            if (isMovie)
                xmlWriter.WriteAttributeString("isMovie", "1");
            else
                xmlWriter.WriteAttributeString("isMovie", "0");

            if (isSports)
                xmlWriter.WriteAttributeString("isSports", "1");
            else
                xmlWriter.WriteAttributeString("isSports", "0");

            if (isNews)
                xmlWriter.WriteAttributeString("isNews", "1");
            else
                xmlWriter.WriteAttributeString("isNews", "0");

            if (isKids)
                xmlWriter.WriteAttributeString("isKids", "1");
            else
                xmlWriter.WriteAttributeString("isKids", "0");
        }

        private static void setGuideImage(XmlWriter xmlWriter, EPGEntry epgEntry)
        {
            if (epgEntry.Poster == null || programImages == null)
                return;

            foreach (Guid guid in programImages)
            {
                if (guid == epgEntry.Poster)
                {
                    xmlWriter.WriteAttributeString("guideImage", "i-" + guid);
                    break;
                }
            }
        }

        private static void processAffiliates(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("Affiliate");
            xmlWriter.WriteAttributeString("name", importName);
            xmlWriter.WriteAttributeString("uid", "!Affiliate!" + importReference);
            xmlWriter.WriteEndElement();

            foreach (TVStation station in RunParameters.Instance.StationCollection)
            {
                if (station.Included && duplicateStationNames.Contains(station.Name))
                {
                    xmlWriter.WriteStartElement("Affiliate");
                    xmlWriter.WriteAttributeString("name", importName + "-" + station.ServiceID);
                    xmlWriter.WriteAttributeString("uid", "!Affiliate!" + importReference + "-" + station.ServiceID);
                    xmlWriter.WriteEndElement();
                }
            }
        }

        private static void processServices(XmlWriter xmlWriter)
        {
            foreach (TVStation station in RunParameters.Instance.StationCollection)
            {
                if (station.Included)
                {
                    xmlWriter.WriteStartElement("Service");
                    xmlWriter.WriteAttributeString("id", "s" + (RunParameters.Instance.StationCollection.IndexOf(station) + 1));
                    xmlWriter.WriteAttributeString("uid", "!Service!" +
                        station.OriginalNetworkID + ":" + 
                        station.TransportStreamID + ":" + 
                        station.ServiceID);
                    xmlWriter.WriteAttributeString("name", string.IsNullOrWhiteSpace(station.NewName) ? station.Name : station.NewName);
                    xmlWriter.WriteAttributeString("callSign", string.IsNullOrWhiteSpace(station.NewName) ? station.Name : station.NewName);

                    if (!duplicateStationNames.Contains(station.Name))
                        xmlWriter.WriteAttributeString("affiliate", "!Affiliate!" + importReference);
                    else
                        xmlWriter.WriteAttributeString("affiliate", "!Affiliate!" + importReference + "-" + station.ServiceID);

                    if (stationImages != null)
                    {
                        int imageIndex = 1;

                        foreach (int imageServiceID in stationImages)
                        {
                            if (imageServiceID == station.ServiceID)
                            {
                                xmlWriter.WriteAttributeString("logoImage", "i" + imageIndex.ToString());
                                break;
                            }

                            imageIndex++;
                        }
                    }

                    xmlWriter.WriteEndElement();
                }
            }
        }

        private static void processSchedules(XmlWriter xmlWriter)
        {
            int programNumber = 1;

            adjustOldStartTimes();

            foreach (TVStation station in RunParameters.Instance.StationCollection)
            {
                if (station.Included)
                {
                    xmlWriter.WriteStartElement("ScheduleEntries");
                    xmlWriter.WriteAttributeString("service", "s" + (RunParameters.Instance.StationCollection.IndexOf(station) + 1));
                    
                    foreach (EPGEntry epgEntry in station.EPGCollection)
                    {
                        xmlWriter.WriteStartElement("ScheduleEntry");

                        if (!OptionEntry.IsDefined(OptionName.UseWmcRepeatCheck) && !OptionEntry.IsDefined(OptionName.UseWmcRepeatCheckBroadcast))
                            xmlWriter.WriteAttributeString("program", "prg" + programNumber);
                        else
                            xmlWriter.WriteAttributeString("program", "prg" + (programIdentifiers.IndexOf(epgEntry.UniqueIdentifier) + 1));

                        xmlWriter.WriteAttributeString("startTime", convertDateTimeToString(epgEntry.StartTime));
                        xmlWriter.WriteAttributeString("duration", epgEntry.Duration.TotalSeconds.ToString());

                        if (epgEntry.VideoQuality != null && epgEntry.VideoQuality.ToLowerInvariant() == "hdtv")
                            xmlWriter.WriteAttributeString("isHdtv", "true");

                        if (epgEntry.AudioQuality != null)
                        {
                            switch (epgEntry.AudioQuality.ToLowerInvariant())
                            {
                                case "mono":
                                    xmlWriter.WriteAttributeString("audioFormat", "1");
                                    break;
                                case "stereo":
                                    xmlWriter.WriteAttributeString("audioFormat", "2");
                                    break;
                                case "dolby":
                                case "surround":
                                    xmlWriter.WriteAttributeString("audioFormat", "3");
                                    break;
                                case "dolby digital":
                                    xmlWriter.WriteAttributeString("audioFormat", "4");
                                    break;
                                default:
                                    break;
                            }
                        }

                        xmlWriter.WriteEndElement();

                        programNumber++;
                    }

                    xmlWriter.WriteEndElement();
                }
            }
        }

        private static void processLineUps(XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("Lineup");
            xmlWriter.WriteAttributeString("id", "l1");
            xmlWriter.WriteAttributeString("uid", "!Lineup!" + importName);
            xmlWriter.WriteAttributeString("name", importName);
            xmlWriter.WriteAttributeString("primaryProvider", "!MCLineup!MainLineup");

            xmlWriter.WriteStartElement("channels");

            foreach (TVStation station in RunParameters.Instance.StationCollection)
            {
                if (station.Included)
                {
                    xmlWriter.WriteStartElement("Channel");
                    if (!DebugEntry.IsDefined(DebugName.WmcNewChannels) && station.WMCUniqueID != null)
                        xmlWriter.WriteAttributeString("uid", station.WMCUniqueID);
                    else
                        xmlWriter.WriteAttributeString("uid", "!Channel!EPGCollector!" + station.OriginalNetworkID + ":" +
                            station.TransportStreamID + ":" +
                            station.ServiceID);
                    xmlWriter.WriteAttributeString("lineup", "l1");
                    xmlWriter.WriteAttributeString("service", "s" + (RunParameters.Instance.StationCollection.IndexOf(station) + 1));
                    
                    if (OptionEntry.IsDefined(OptionName.AutoMapEpg))
                    {
                        if (station.WMCMatchName != null)
                            xmlWriter.WriteAttributeString("matchName", station.WMCMatchName);
                    }

                    if (station.LogicalChannelNumber != -1)
                        xmlWriter.WriteAttributeString("number", station.LogicalChannelNumber.ToString());

                    xmlWriter.WriteEndElement();
                }
            }

            xmlWriter.WriteEndElement();
            xmlWriter.WriteEndElement();
        }

        private static string runImportUtility(string fileName)
        {
            string runDirectory = Path.Combine(Environment.GetEnvironmentVariable("SystemRoot"), "ehome");
            Logger.Instance.Write("Running Windows Media Centre import utility LoadMXF from " + runDirectory);
 
            importProcess = new Process();

            importProcess.StartInfo.FileName = Path.Combine(runDirectory, "LoadMXF.exe");
            importProcess.StartInfo.WorkingDirectory = runDirectory + Path.DirectorySeparatorChar;
            importProcess.StartInfo.Arguments = @"-v -i " + '"' + fileName + '"';
            importProcess.StartInfo.UseShellExecute = false;
            importProcess.StartInfo.CreateNoWindow = true;
            importProcess.StartInfo.RedirectStandardOutput = true;
            importProcess.StartInfo.RedirectStandardError = true;
            importProcess.EnableRaisingEvents = true;
            importProcess.OutputDataReceived += new DataReceivedEventHandler(importProcessOutputDataReceived);
            importProcess.ErrorDataReceived += new DataReceivedEventHandler(importProcessErrorDataReceived);
            importProcess.Exited += new EventHandler(importProcessExited);

            try
            {
                importProcess.Start();

                importProcess.BeginOutputReadLine();
                importProcess.BeginErrorReadLine();

                while (!importExited)
                    Thread.Sleep(500);

                Logger.Instance.Write("Windows Media Centre import utility LoadMXF has completed: exit code " + importProcess.ExitCode);
                if (importProcess.ExitCode == 0)
                    return (null);
                else
                    return ("Failed to load Windows Media Centre data: reply code " + importProcess.ExitCode);
            }
            catch (Exception e)
            {
                Logger.Instance.Write("<e> Failed to run the Windows Media Centre import utility LoadMXF");
                Logger.Instance.Write("<e> " + e.Message);
                return ("Failed to load Windows Media Centre data due to an exception");
            }
        }

        private static void importProcessOutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            Logger.Instance.Write("LoadMXF message: " + e.Data);
        }

        private static void importProcessErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null)
                return;

            Logger.Instance.Write("<e> LoadMXF error: " + e.Data);
        }

        private static void importProcessExited(object sender, EventArgs e)
        {
            importExited = true;
        }

        private static string convertDateTimeToString(DateTime dateTime)
        {
            DateTime utcTime = getUtcTime(dateTime);

            return (utcTime.Date.ToString("yyyy-MM-dd") + "T" +
                utcTime.Hour.ToString("00") + ":" +
                utcTime.Minute.ToString("00") + ":" +
                utcTime.Second.ToString("00"));            
        }

        private static DateTime getUtcTime(DateTime dateTime)
        {
            try
            {
                return(TimeZoneInfo.ConvertTimeToUtc(dateTime));
            }
            catch (ArgumentException e)
            {
                Logger.Instance.Write("<e> Local start date/time is invalid: " + dateTime);
                Logger.Instance.Write("<e> " + e.Message);
                Logger.Instance.Write("<e> Start time will be advanced by 1 hour");

                return(TimeZoneInfo.ConvertTimeToUtc(dateTime.AddHours(1)));
            }
        }

        private static void adjustOldStartTimes()
        {
            if (!DebugEntry.IsDefined(DebugName.AdjustStartTimes))
                return;

            foreach (TVStation station in RunParameters.Instance.StationCollection)
            {
                if (station.Included && station.EPGCollection.Count > 0)
                {
                    TimeSpan offset = DateTime.Now - station.EPGCollection[0].StartTime;

                    foreach (EPGEntry epgEntry in station.EPGCollection)
                        epgEntry.StartTime = epgEntry.StartTime + offset;
                }
            }
        }

        private static string getSeriesLink(EPGEntry epgEntry)
        {
            if (!OptionEntry.IsDefined(OptionName.UseWmcRepeatCheckBroadcast))
                return (epgEntry.EventName);

            string result = null;

            if (!string.IsNullOrEmpty(epgEntry.SeasonCrid))
                result = getNumber(epgEntry.SeasonCrid);
            
            if (string.IsNullOrEmpty(result))
            {
                if (!string.IsNullOrEmpty(epgEntry.SeriesId))
                    result = getNumber(epgEntry.SeriesId);                
            }

            if (string.IsNullOrEmpty(result))
                result = epgEntry.EventName;

            return (result);
        }

        private static string getNumber(string text)
        {
            if (text.Trim().Length == 0)
                return (string.Empty);

            StringBuilder numericString = new StringBuilder();

            foreach (char cridChar in text)
            {
                if (cridChar >= '0' && cridChar <= '9')
                    numericString.Append(cridChar);
            }

            if (numericString.Length != 0)
                return (numericString.ToString());
            else
                return (string.Empty);
        }

        private static string getAssemblyVersion(string fileName)
        {
            string path = Path.Combine(Environment.GetEnvironmentVariable("windir"), Path.Combine("ehome", fileName));

            try
            {
                AssemblyName assemblyName = AssemblyName.GetAssemblyName(path);

                return (assemblyName.Version.Major + "." +
                    assemblyName.Version.Minor + "." +
                    assemblyName.Version.MajorRevision + "." +
                    assemblyName.Version.MinorRevision);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("Failed to get assembly version for " + fileName);
                Logger.Instance.Write(e.Message);
                return (string.Empty);
            }
        }

        private static string getAssemblyPublicKey(string fileName)
        {
            string path = Path.Combine(Environment.GetEnvironmentVariable("windir"), Path.Combine("ehome", fileName));

            try
            {
                AssemblyName assemblyName = AssemblyName.GetAssemblyName(path);

                byte[] publicKey = assemblyName.GetPublicKey();

                StringBuilder builder = new StringBuilder();
                foreach (byte keyByte in publicKey)
                    builder.Append(keyByte.ToString("x2"));

                return (builder.ToString());
            }
            catch (IOException e)
            {
                Logger.Instance.Write("Failed to get assembly public key for " + fileName);
                Logger.Instance.Write(e.Message);
                return (string.Empty);
            }
        }

        internal class KeywordGroup
        {
            internal string Name { get { return(name); } }
            internal Collection<string> Keywords { get { return (keywords); } }

            private string name;
            private Collection<string> keywords;

            internal KeywordGroup(string name)
            {
                this.name = name;
                keywords = new Collection<string>();
            }
        }
    }
}
