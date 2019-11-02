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
using System.Collections;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Globalization;
using System.Text;

using DomainObjects;

using TheTvDB;

namespace Lookups
{
    internal class TVLookup
    {
        internal int WebLookups { get; private set; }
        internal int InStoreLookups { get; private set; }
        internal int CacheLookups { get; private set; }

        internal Collection<string> UnusedPosters { get; private set; }

        internal bool Initialized { get; private set; }

        private Collection<TVSeriesEntry> tvSeries;
        private Collection<CacheEntry> cacheEntries;
        private Collection<PosterEntry> posterEntries;

        private int webExceptionCount;
        private bool webException;

        private DateTime startTime;

        private TvdbAPI apiInstance;

        private int noData;
        private int outstanding;
        private int subtitleLookups;
        private int likeSearches;
        private int likeSearchesWorked;
        private int webErrors;
        private int duplicatesDeleted;

        private string[] punctuation = new string[] { "...", ". . .", "?", "!", ",", "'", ";", ":", " - ", "the ", " and ", " an ", " " };
        private double threshold = 0.60;

        private string languageCode;

        private int referencedPosters = 0;
        private int unmatchedDeleted = 0;
        private int unmatchedNotDeleted = 0;

        private string regionCode;
        private string regionName;

        internal TVLookup()
        {
            try
            {
                apiInstance = new TvdbAPI("8F5A6310BB09360E");
                Initialized = true;
            }
            catch (Exception e)
            {
                Logger.Instance.Write("<e> An exception of type " + e.GetType().Name + " has occured while connecting to TVDB");
                Logger.Instance.Write("<e> " + e.Message);
                Logger.Instance.Write("<e> No TV series metadata available");
                return;
            }

            loadTVSeriesDatabase();
            startTime = DateTime.Now;

            LanguageCode.Load();

            try
            {
                RegionInfo regionInfo = new RegionInfo(CultureInfo.CurrentCulture.LCID);
                regionCode = regionInfo.TwoLetterISORegionName;
                regionName = regionInfo.EnglishName;

                Logger.Instance.Write("Using region code '" + regionCode + "' and region name '" + regionName + "' when matching");
            }
            catch (ArgumentException)
            {
                Logger.Instance.Write("No region information available");
            }

            noData = 0;
            outstanding = 0;
            subtitleLookups = 0;
            likeSearches = 0;
            likeSearchesWorked = 0;
            webErrors = 0;
        }

        private void loadTVSeriesDatabase()
        {
            tvSeries = new Collection<TVSeriesEntry>();
            posterEntries = new Collection<PosterEntry>();

            if (RunParameters.Instance.LookupReload)
            {
                clearExistingData();
                return;
            }

            TVSeriesEntry tvSeriesEntry = null;
            XmlReader reader = null;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;

            string path = Path.Combine(RunParameters.DataDirectory, "TV Series Database.xml");

            Logger.Instance.Write("Loading TV series database from " + path);
            int notFoundCount = 0;

            try
            {
                reader = XmlReader.Create(path, settings);
            }
            catch (IOException)
            {
                Logger.Instance.Write("TV series database cannot be opened");
                return;
            }

            try
            {
                while (!reader.EOF)
                {
                    reader.Read();
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name.ToLowerInvariant())
                        {
                            case "series":
                                tvSeriesEntry = new TVSeriesEntry(reader.GetAttribute("title"), reader.GetAttribute("metaDataTitle"));
                                tvSeriesEntry.Status = reader.GetAttribute("status");

                                tvSeriesEntry.Load(reader.ReadSubtree());
                                if (tvSeriesEntry.Poster != null)
                                {
                                    PosterEntry posterEntry = PosterEntry.FindPosterEntry(posterEntries, tvSeriesEntry.Poster.Value);
                                    posterEntry.Count++;
                                }

                                addTVSeries(tvSeriesEntry);
                                if (tvSeriesEntry.Status == "notfound")
                                    notFoundCount++;

                                break;
                            default:
                                break;
                        }
                    }
                }
            }
            catch (XmlException e)
            {
                Logger.Instance.Write("Failed to load file " + path);
                Logger.Instance.Write("Data exception: " + e.Message);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("Failed to load file " + path);
                Logger.Instance.Write("I/O exception: " + e.Message);
            }

            if (reader != null)
                reader.Close();

            Logger.Instance.Write("Loaded " + tvSeries.Count + " TV series (not found = " + notFoundCount + ")");
        }

        private void addTVSeries(TVSeriesEntry newEntry)
        {
            if (newEntry.Status == "notfound" && string.IsNullOrWhiteSpace(newEntry.Overview))
                return;

            foreach (TVSeriesEntry oldEntry in tvSeries)
            {
                if (newEntry.Title == oldEntry.Title &&
                    newEntry.EpisodeName == oldEntry.EpisodeName &&
                    newEntry.SeasonNumber == oldEntry.SeasonNumber &&
                    newEntry.EpisodeNumber == oldEntry.EpisodeNumber &&
                    newEntry.Overview == oldEntry.Overview)
                {
                    if (TraceEntry.IsDefined(TraceName.Lookups))
                        Logger.Instance.Write("Duplicate TV database entry ignored for " + newEntry.ToString());

                    duplicatesDeleted++;
                    return;
                }
            }

            tvSeries.Add(newEntry);
        }

        private void clearExistingData()
        {
            Logger.Instance.Write("Clearing existing TV series data");

            string path = Path.Combine(RunParameters.DataDirectory, "TV Series Database.xml");

            try
            {
                File.Delete(path);
                Logger.Instance.Write("TV series database deleted");
            }
            catch (IOException e)
            {
                Logger.Instance.Write("Failed to delete " + path);
                Logger.Instance.Write("I/O exception: " + e.Message);
            }
            catch (UnauthorizedAccessException e)
            {
                Logger.Instance.Write("Failed to delete " + path);
                Logger.Instance.Write("I/O exception: " + e.Message);
            }

            LookupController.ClearPosterDirectory(RunParameters.ImagePath);
            LookupController.ClearPosterDirectory(Path.Combine(RunParameters.ImagePath, "TV Series"));            

            Logger.Instance.Write("Existing TV series data cleared");
        }

        private void clearPosterDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                return;

            DirectoryInfo directory = new DirectoryInfo(directoryPath);
            FileInfo[] files = directory.GetFiles("*.jpg", SearchOption.TopDirectoryOnly);

            foreach (FileInfo file in files)
            {
                try
                {
                    File.Delete(file.FullName);
                }
                catch (IOException e)
                {
                    Logger.Instance.Write("Failed to delete " + file.FullName);
                    Logger.Instance.Write("I/O exception: " + e.Message);
                }
                catch (UnauthorizedAccessException e)
                {
                    Logger.Instance.Write("Failed to delete " + file.FullName);
                    Logger.Instance.Write("I/O exception: " + e.Message);
                }
            }
        }

        internal LookupController.LookupReply Process(EPGEntry epgEntry)
        {
            bool isTVSeries = checkForTVSeries(epgEntry);
            if (!isTVSeries)
                return (LookupController.LookupReply.NotTVSeries);

            if (epgEntry.EventName == null || epgEntry.ShortDescription == null)
                return (LookupController.LookupReply.NoData);

            if (TraceEntry.IsDefined(TraceName.LookupName))
            {
                TraceEntry traceEntry = TraceEntry.FindEntry(TraceName.LookupName, true);

                if (traceEntry != null && traceEntry.StringParameterSet)
                {
                    if (epgEntry.EventName != traceEntry.StringParameter)
                        return (LookupController.LookupReply.NoData);
                }
            }

            if (TraceEntry.IsDefined(TraceName.Lookups))
            {
                string languageCode = string.Empty;
                if (epgEntry.LanguageCode != null)
                    languageCode = " language code '" + LanguageCode.FindLanguageCode(epgEntry.LanguageCode).LookupCode + "'";

                Logger.Instance.Write("Processing TV series " + epgEntry.EventName + languageCode +
                    " subtitle " + (string.IsNullOrWhiteSpace(epgEntry.EventSubTitle) ? "n/a" : epgEntry.EventSubTitle) +
                    " season number " + epgEntry.SeasonNumber +
                    " episode number " + epgEntry.EpisodeNumber);
            }

            string searchTitle = getSearchName(epgEntry.EventName);

            TVSeriesEntry existingTVSeries = findTVSeriesInLocalDB(searchTitle, epgEntry);
            if (existingTVSeries != null)
            {
                if (TraceEntry.IsDefined(TraceName.Lookups))
                    Logger.Instance.Write("Entry found in local database for " + epgEntry.EventName + " - status is " + existingTVSeries.Status);

                if (existingTVSeries.Found)
                {
                    processTVSeries(epgEntry, existingTVSeries);

                    if (TraceEntry.IsDefined(TraceName.Lookups))
                        Logger.Instance.Write("Season set to " + epgEntry.SeasonNumber + " Episode set to " + epgEntry.EpisodeNumber +
                            " Episode name set to " + (string.IsNullOrWhiteSpace(epgEntry.EventSubTitle) ? "n/a" : "'" + epgEntry.EventSubTitle + "'"));

                    InStoreLookups++;
                    return (LookupController.LookupReply.InStore);
                }
                else
                {
                    if (existingTVSeries.UsedThisTime || !RunParameters.Instance.LookupNotFound)
                    {
                        existingTVSeries.UsedThisTime = true;
                        existingTVSeries.DateLastUsed = DateTime.Now;
                        noData++;
                        return (LookupController.LookupReply.NoData);
                    }
                }
            }

            if (DateTime.Now >= startTime.AddMinutes(RunParameters.Instance.LookupTimeLimit))
            {
                if (TraceEntry.IsDefined(TraceName.Lookups))
                    Logger.Instance.Write("Lookups timed out - entry ignored");

                outstanding++;
                return (LookupController.LookupReply.NotProcessed);
            }

            if (webException)
            {
                if (TraceEntry.IsDefined(TraceName.Lookups))
                    Logger.Instance.Write("Web exception limit reached (" + RunParameters.Instance.LookupErrorLimit + ") - entry ignored");

                outstanding++;
                return (LookupController.LookupReply.NotProcessed);
            }

            try
            {
                TvdbSeries tvSeriesEntry = null;
                bool fromCache = false;

                if (cacheEntries != null)
                    tvSeriesEntry = findCacheEntry(searchTitle, epgEntry.EventSubTitle);

                if (tvSeriesEntry == null)
                {
                    TvdbSeriesSearchResult results;

                    if (epgEntry.LanguageCode == null)
                        languageCode = null;
                    else
                        languageCode = LanguageCode.FindLanguageCode(epgEntry.LanguageCode).LookupCode;

                    results = TvdbSeries.Search(apiInstance, searchTitle, languageCode);

                    webExceptionCount = 0;

                    if (results == null || results.Series == null || results.Series.Count == 0)
                    {
                        if (languageCode != null && languageCode != apiInstance.DefaultLanguageCode)
                        {
                            languageCode = null;
                            results = TvdbSeries.Search(apiInstance, searchTitle, languageCode);
                        }
                    }

                    if (results == null || results.Series == null || results.Series.Count == 0)
                    {
                        if (existingTVSeries == null)
                            tvSeries.Add(TVSeriesEntry.CreateNotFoundEntry(searchTitle, epgEntry.EventSubTitle, epgEntry.ShortDescription, epgEntry.SeasonNumber, epgEntry.EpisodeNumber));
                        else
                        {
                            existingTVSeries.UsedThisTime = true;
                            existingTVSeries.DateLastUsed = DateTime.Now;
                        }

                        if (TraceEntry.IsDefined(TraceName.Lookups))
                            Logger.Instance.Write("No results for " + epgEntry.EventName);

                        noData++;
                        return (LookupController.LookupReply.NoData);
                    }

                    if (TraceEntry.IsDefined(TraceName.Lookups))
                        Logger.Instance.Write("Retrieved " + results.Series.Count + " matches for " + searchTitle);

                    tvSeriesEntry = findTVSeriesEntry(results.Series, searchTitle, epgEntry.EventSubTitle);
                }
                else
                {
                    fromCache = true;
                    if (TraceEntry.IsDefined(TraceName.Lookups))
                        Logger.Instance.Write("Entry found in cache for " + epgEntry.EventName);
                }

                if (tvSeriesEntry != null)
                {
                    if (TraceEntry.IsDefined(TraceName.Lookups))
                        Logger.Instance.Write("Found series " + tvSeriesEntry.SeriesName);

                    if (!fromCache)
                        addToCacheEntries(searchTitle, tvSeriesEntry);

                    TVSeriesEntry newTVSeries = createTVSeriesEntry(searchTitle, tvSeriesEntry, epgEntry, fromCache);
                    if (newTVSeries != null)
                    {
                        processTVSeries(epgEntry, newTVSeries);
                        if (TraceEntry.IsDefined(TraceName.Lookups))
                            Logger.Instance.Write("Season set to " + epgEntry.SeasonNumber + " Episode set to " + epgEntry.EpisodeNumber +
                                " Episode name set to " + (string.IsNullOrWhiteSpace(epgEntry.EventSubTitle) ? "n/a" : "'" + epgEntry.EventSubTitle + "'"));

                        if (fromCache)
                            CacheLookups++;
                        else
                            WebLookups++;

                        return (LookupController.LookupReply.WebLookup);
                    }
                    else
                    {
                        noData++;
                        return (LookupController.LookupReply.NoData);
                    }
                }

                tvSeries.Add(TVSeriesEntry.CreateNotFoundEntry(searchTitle, epgEntry.EventSubTitle, epgEntry.ShortDescription, epgEntry.SeasonNumber, epgEntry.EpisodeNumber));

                if (TraceEntry.IsDefined(TraceName.Lookups))
                    Logger.Instance.Write("No matching result for " + epgEntry.EventName);

                noData++;
                return (LookupController.LookupReply.NoData);
            }
            catch (Exception e)
            {
                webErrors++;
                webExceptionCount++;

                if (webExceptionCount < RunParameters.Instance.LookupErrorLimit)
                {
                    Logger.Instance.Write("<e> TV series lookup has encountered an exception of type " + e.GetType().Name
                        + " when searching for '" + searchTitle + "'");
                    Logger.Instance.Write("<e> " + e.Message);
                    
                    if (DebugEntry.IsDefined(DebugName.LogResponseKeys) && apiInstance.ResponseKeys != null)
                    {
                        foreach (DictionaryEntry dictionaryEntry in apiInstance.ResponseKeys)
                            Logger.Instance.Write("Response key: " + dictionaryEntry.Key + " value: " + dictionaryEntry.Value);
                    }
                }
                else
                    webException = true;

                noData++;
                return (LookupController.LookupReply.NoData);
            }
        }

        private bool checkForTVSeries(EPGEntry epgEntry)
        {
            if (RunParameters.Instance.LookupProcessAsTVSeries)
                return (true);

            if (epgEntry.SeriesId != null || epgEntry.SeasonNumber != -1)
                return (true);

            if (epgEntry.EventCategory != null && epgEntry.EventCategory.ToLowerInvariant().Contains("istvseries"))
                return (true);

            if (!string.IsNullOrWhiteSpace(epgEntry.EventSubTitle))
                return (true);

            return (false);
        }

        private string getSearchName(string eventName)
        {
            if (eventName == null || RunParameters.Instance.LookupIgnoredPhrases == null)
                return (eventName);

            string searchName = eventName;

            if (RunParameters.Instance.LookupIgnoredPhrases != null)
            {
                foreach (string phrase in RunParameters.Instance.LookupIgnoredPhrases)
                    searchName = searchName.Replace(phrase.Trim(), "");
            }

            return (searchName.Trim());
        }

        private TVSeriesEntry findTVSeriesInLocalDB(string title, EPGEntry epgEntry)
        {
            if (title == null)
                return (null);

            if (TraceEntry.IsDefined(TraceName.LookupsDetail))
                Logger.Instance.Write("Searching local database for title: " + title + " season: " + epgEntry.SeasonNumber +
                    " episode Number: " + epgEntry.EpisodeNumber + " description: " +
                    (string.IsNullOrWhiteSpace(epgEntry.ShortDescription) ? "n/a" : epgEntry.ShortDescription));

            foreach (TVSeriesEntry tvSeriesEntry in tvSeries)
            {
                bool matched = matchLocalDBEntries(tvSeriesEntry, title, epgEntry.EventSubTitle, epgEntry.ShortDescription,
                    epgEntry.SeasonNumber, epgEntry.EpisodeNumber);
                if (matched)
                    return (tvSeriesEntry);
            }

            return (null);
        }

        private bool matchLocalDBEntries(TVSeriesEntry tvSeriesEntry, string title, string subTitle, string description, int seasonNumber, int episodeNumber)
        {
            if (tvSeriesEntry.Title == null || tvSeriesEntry.Title.ToLowerInvariant() != title.ToLowerInvariant())
                return (false);

            if (!string.IsNullOrWhiteSpace(tvSeriesEntry.EpisodeName) && !string.IsNullOrWhiteSpace(subTitle))
                return (tvSeriesEntry.EpisodeName == subTitle);

            if (seasonNumber != -1 && episodeNumber != -1)
            {
                if (tvSeriesEntry.SeasonNumber == seasonNumber && tvSeriesEntry.EpisodeNumber == episodeNumber)
                    return (true);
            }

            if (!string.IsNullOrWhiteSpace(subTitle))
                return (false);

            if (!string.IsNullOrWhiteSpace(tvSeriesEntry.Overview) && !string.IsNullOrWhiteSpace(description))
            {
                if (tvSeriesEntry.Overview.ToLowerInvariant() == description.ToLowerInvariant())
                    return (true);
            }

            return (false);
        }

        private TvdbSeries findTVSeriesEntry(Collection<TvdbSeries> series, string title, string subTitle)
        {
            if (!string.IsNullOrWhiteSpace(subTitle))
            {
                TvdbSeries episodeSeries = findTVSeriesEntryUsingSubTitle(series, title, subTitle);
                if (episodeSeries != null)
                    return (episodeSeries);
            }

            string noCaseTitle = title.ToLowerInvariant();
            string noCaseRegionCode = regionCode.ToLowerInvariant();
            string noCaseRegionName = regionName.ToLowerInvariant();

            foreach (TvdbSeries tvSeriesEntry in series)
            {
                if (!string.IsNullOrWhiteSpace(tvSeriesEntry.SeriesName))
                {
                    if (tvSeriesEntry.SeriesName.ToLowerInvariant() == noCaseTitle + " (" + noCaseRegionCode + ")" ||
                        tvSeriesEntry.SeriesName.ToLowerInvariant() == noCaseTitle + " " + noCaseRegionCode ||
                        tvSeriesEntry.SeriesName.ToLowerInvariant() == noCaseTitle + " " + noCaseRegionName)
                        return (tvSeriesEntry);
                }
            }

            foreach (TvdbSeries tvSeriesEntry in series)
            {
                if (!string.IsNullOrWhiteSpace(tvSeriesEntry.SeriesName))
                {
                    if (tvSeriesEntry.SeriesName.ToLowerInvariant() == noCaseTitle)
                        return (tvSeriesEntry);
                }
            }

            switch (RunParameters.Instance.LookupMatching)
            {
                case MatchMethod.Exact:
                    return (null);

                case MatchMethod.Contains:
                    TvdbSeries matchedEntry = null;

                    foreach (TvdbSeries tvSeriesEntry in series)
                    {
                        if (!string.IsNullOrWhiteSpace(tvSeriesEntry.SeriesName))
                        {
                            if (tvSeriesEntry.SeriesName.ToLowerInvariant().Contains(noCaseTitle))
                            {
                                if (matchedEntry == null)
                                    matchedEntry = tvSeriesEntry;
                                else
                                {
                                    int lengthDiff1 = title.Length - matchedEntry.SeriesName.Length;
                                    if (lengthDiff1 < 0)
                                        lengthDiff1 *= -1;

                                    int lengthDiff2 = title.Length - tvSeriesEntry.SeriesName.Length;
                                    if (lengthDiff2 < 0)
                                        lengthDiff2 *= -1;

                                    if (lengthDiff2 < lengthDiff1)
                                        matchedEntry = tvSeriesEntry;
                                }
                            }
                        }
                    }

                    return (matchedEntry);

                case MatchMethod.Nearest:
                    if (series.Count == 1)
                        return (series[0]);

                    Collection<string> titleList = new Collection<string>();

                    foreach (TvdbSeries tvSeriesEntry in series)
                    {
                        if (!string.IsNullOrWhiteSpace(tvSeriesEntry.SeriesName))
                            titleList.Add(tvSeriesEntry.SeriesName);
                    }

                    int index = LookupController.FindBestMatch(title, titleList).Index;
                    return (series[index]);
                default:
                    return (null);
            }
        }

        private TvdbSeries findTVSeriesEntryUsingSubTitle(Collection<TvdbSeries> series, string title, string subTitle)
        {
            if (string.IsNullOrWhiteSpace(subTitle))
                return (null);

            foreach (TvdbSeries tvSeriesEntry in series)
            {
                tvSeriesEntry.LoadEpisodes(apiInstance, languageCode);
                if (!string.IsNullOrWhiteSpace(tvSeriesEntry.LastError))
                {
                    if (TraceEntry.IsDefined(TraceName.LookupsError))
                    {
                        Logger.Instance.Write("<e> Series " + title + " has encountered an error when loading episodes");
                        Logger.Instance.Write("<e> " + tvSeriesEntry.LastError);
                    }

                }

                if (tvSeriesEntry.Episodes != null)
                {
                    foreach (TvdbEpisode episode in tvSeriesEntry.Episodes)
                    {
                        bool found = matchEpisodeNames(episode.EpisodeName, subTitle);
                        if (found)
                        {
                            if (TraceEntry.IsDefined(TraceName.Lookups))
                                Logger.Instance.Write("Series " + tvSeriesEntry.SeriesName + " located with episode " + episode.EpisodeName);
                            return (tvSeriesEntry);
                        }
                    }
                }
            }

            double bestResult = -1;
            TvdbSeries bestSeries = null;
            TvdbEpisode bestEpisode = null;

            string matchSubTitle = removePunctuation(processEpisodePartNumber(subTitle.Trim().ToLowerInvariant()), punctuation);

            foreach (TvdbSeries tvSeriesEntry in series)
            {
                if (tvSeriesEntry.Episodes != null)
                {
                    foreach (TvdbEpisode episode in tvSeriesEntry.Episodes)
                    {
                        if (!string.IsNullOrWhiteSpace(episode.EpisodeName))
                        {
                            string matchEpisodeName = removePunctuation(processNamePartNumber(episode.EpisodeName.Trim().ToLowerInvariant()), punctuation);
                            double result = LookupController.CompareLikeStrings(matchEpisodeName, matchSubTitle);

                            if (result > bestResult)
                            {
                                bestResult = result;
                                bestSeries = tvSeriesEntry;
                                bestEpisode = episode;
                            }
                        }
                        else
                        {
                            if (TraceEntry.IsDefined(TraceName.LookupsError))
                                Logger.Instance.Write("Series " + tvSeriesEntry.SeriesName + " episode " + episode.EpisodeNumber + " has no episode name");
                        }
                    }
                }
            }

            if (bestResult >= threshold)
            {
                if (TraceEntry.IsDefined(TraceName.Lookups))
                {
                    Logger.Instance.Write("Episode " + subTitle + " matched (" + bestResult.ToString("0.000") + ") with episode " + bestEpisode.EpisodeName +
                        " in series " + bestSeries.SeriesName);
                }

                return (bestSeries);
            }

            if (TraceEntry.IsDefined(TraceName.Lookups))
                Logger.Instance.Write("Best match was " + bestResult.ToString("0.000") + " episode " +
                    bestEpisode.EpisodeName + " in series " + bestSeries.SeriesName);

            foreach (TvdbSeries tvSeriesEntry in series)
            {
                if (tvSeriesEntry.Episodes != null)
                {
                    foreach (TvdbEpisode episode in tvSeriesEntry.Episodes)
                    {
                        if (!string.IsNullOrWhiteSpace(episode.EpisodeName))
                        {
                            string matchEpisodeName = removePunctuation(processNamePartNumber(episode.EpisodeName.Trim().ToLowerInvariant()), punctuation);
                            if (matchEpisodeName.StartsWith(matchSubTitle))
                            {
                                if (TraceEntry.IsDefined(TraceName.Lookups))
                                    Logger.Instance.Write("Series " + tvSeriesEntry.SeriesName + " partial match with episode " + episode.EpisodeName);

                                return (tvSeriesEntry);
                            }
                        }
                        else
                        {
                            if (TraceEntry.IsDefined(TraceName.LookupsError))
                                Logger.Instance.Write("Series " + tvSeriesEntry.SeriesName + " episode " + episode.EpisodeNumber + " has no episode name");
                        }
                    }
                }
            }

            return (null);
        }

        private void addToCacheEntries(string title, TvdbSeries series)
        {
            if (cacheEntries == null)
                cacheEntries = new Collection<CacheEntry>();

            foreach (CacheEntry existingEntry in cacheEntries)
            {
                if (existingEntry.Title == title)
                    return;
            }

            cacheEntries.Add(new CacheEntry(title, series));
        }

        private TvdbSeries findCacheEntry(string title, string subTitle)
        {
            foreach (CacheEntry cacheEntry in cacheEntries)
            {
                if (cacheEntry.Title == title)
                {
                    if (string.IsNullOrWhiteSpace(subTitle))
                        return (cacheEntry.Series);

                    if (cacheEntry.Series.Episodes != null)
                    {
                        foreach (TvdbEpisode episode in cacheEntry.Series.Episodes)
                        {
                            bool matched = matchEpisodeNames(episode.EpisodeName, subTitle);
                            if (matched)
                                return (cacheEntry.Series);
                        }
                    }
                }
            }

            return (null);
        }

        private TVSeriesEntry createTVSeriesEntry(string searchTitle, TvdbSeries tvSeriesEntry, EPGEntry epgEntry, bool fromCache)
        {
            TVSeriesEntry seriesEntry = null;
            string posterPath = null;

            try
            {
                bool loadAllData = needAllData(epgEntry.SeasonNumber, epgEntry.EpisodeNumber, epgEntry.EventSubTitle);

                if (!fromCache)
                {
                    tvSeriesEntry.LoadDetails(apiInstance, languageCode);
                    tvSeriesEntry.LoadActors(apiInstance);

                    if (loadAllData)
                    {
                        tvSeriesEntry.LoadEpisodes(apiInstance, languageCode);
                        if (!string.IsNullOrWhiteSpace(tvSeriesEntry.LastError))
                        {
                            if (TraceEntry.IsDefined(TraceName.LookupsError))
                            {
                                Logger.Instance.Write("<e> Series " + searchTitle + " has encountered an error when loading episodes");
                                Logger.Instance.Write("<e> " + tvSeriesEntry.LastError);
                            }

                        }
                    }
                }

                seriesEntry = new TVSeriesEntry(searchTitle, tvSeriesEntry.SeriesName);
                seriesEntry.OriginalTitle = epgEntry.EventName;
                seriesEntry.SeriesOverview = LookupController.ReplaceHtmlChars(tvSeriesEntry.Overview);
                seriesEntry.SeriesStartDate = tvSeriesEntry.FirstAiredDate;

                if (epgEntry.ShortDescription != null)
                    seriesEntry.Overview = epgEntry.ShortDescription;
                else
                {
                    if (tvSeriesEntry.Overview != null)
                        seriesEntry.Overview = LookupController.ReplaceHtmlChars(tvSeriesEntry.Overview);
                }

                seriesEntry.Cast = tvSeriesEntry.ActorsNames;
                seriesEntry.StarRating = LookupController.GetStarRating(tvSeriesEntry.Rating);

                if (tvSeriesEntry.Genres != null && tvSeriesEntry.Genres.Count > 0)
                    seriesEntry.Genre = tvSeriesEntry.Genres[0];

                seriesEntry.SeasonNumber = epgEntry.SeasonNumber;
                seriesEntry.EpisodeNumber = epgEntry.EpisodeNumber;
                seriesEntry.EpisodeName = epgEntry.EventSubTitle;

                if (loadAllData)
                {
                    TvdbEpisode episode = findEpisode(tvSeriesEntry, epgEntry.SeasonNumber, epgEntry.EpisodeNumber, epgEntry.EventName, epgEntry.EventSubTitle);

                    string subTitleComment = " Subtitle " + (!string.IsNullOrWhiteSpace(epgEntry.EventSubTitle) ? "'" + epgEntry.EventSubTitle + "'" : "n/a");

                    if (episode != null)
                    {
                        if (TraceEntry.IsDefined(TraceName.Lookups))
                            Logger.Instance.Write("Season " + epgEntry.SeasonNumber + " Episode " + epgEntry.EpisodeNumber + subTitleComment + " located in episodes");

                        seriesEntry.SeasonNumber = episode.SeasonNumber;
                        seriesEntry.EpisodeNumber = episode.EpisodeNumber;
                        seriesEntry.EpisodeName = epgEntry.EventSubTitle;
                        seriesEntry.MetaDataEpisodeName = episode.EpisodeName;

                        if (string.IsNullOrWhiteSpace(seriesEntry.Overview) && !string.IsNullOrWhiteSpace(episode.Overview))
                            seriesEntry.Overview = LookupController.ReplaceHtmlChars(episode.Overview);

                        seriesEntry.Directors = episode.DirectorsNames;
                        seriesEntry.Writers = episode.Writers;
                        seriesEntry.GuestStars = episode.GuestStars;
                    }
                    else
                    {
                        if (TraceEntry.IsDefined(TraceName.Lookups))
                            Logger.Instance.Write("Season " + epgEntry.SeasonNumber + " Episode " + epgEntry.EpisodeNumber + subTitleComment + " not located");
                    }
                }
                else
                {
                    if (TraceEntry.IsDefined(TraceName.Lookups))
                        Logger.Instance.Write("Season/episode numbers or subtitle not available");
                }

                loadImage(epgEntry.EventName, searchTitle, seriesEntry, tvSeriesEntry);
                webExceptionCount = 0;

                tvSeries.Add(seriesEntry);

                return (seriesEntry);
            }
            catch (Exception e)
            {
                Logger.Instance.Write("<e> TV series lookup has encountered an exception of type " + e.GetType().Name +
                    " when loading series data for '" + searchTitle + "'");
                Logger.Instance.Write("<e> " + e.Message);

                if (DebugEntry.IsDefined(DebugName.LogResponseKeys) && apiInstance.ResponseKeys != null)
                {
                    foreach (DictionaryEntry dictionaryEntry in apiInstance.ResponseKeys)
                        Logger.Instance.Write("Response key: " + dictionaryEntry.Key + " value: " + dictionaryEntry.Value);
                }

                if (DebugEntry.IsDefined(DebugName.StackTrace))
                    Logger.Instance.Write("<e> " + e.StackTrace);

                webErrors++;
                webExceptionCount++;

                if (webExceptionCount >= RunParameters.Instance.LookupErrorLimit)
                    webException = true;

                if (posterPath != null && File.Exists(posterPath))
                    File.Delete(posterPath);

                return (null);
            }
        }

        private void loadImage(string title, string searchTitle, TVSeriesEntry seriesEntry, TvdbSeries tvSeriesEntry)
        {
            if (RunParameters.Instance.DownloadTVThumbnail == LookupImageType.None)
                return;

            string imageDirectory = (RunParameters.Instance.LookupImagesInBase ?
                RunParameters.ImagePath :
                Path.Combine(RunParameters.ImagePath, "TV Series"));

            if (!Directory.Exists(imageDirectory))
                Directory.CreateDirectory(imageDirectory);

            Guid imageGuid = Guid.NewGuid();
                        
            string posterPath = (RunParameters.Instance.LookupImageNameTitle ?
                Path.Combine(imageDirectory, RunParameters.GetLegalFileName(title, ' ') + ".jpg") :
                Path.Combine(imageDirectory, imageGuid + ".jpg"));

            bool imageLoaded = false;

            TVSeriesEntry existingEntry = findPosterEntry(searchTitle);
            if (existingEntry == null || existingEntry.Poster == null)
            {
                try
                {
                    switch (RunParameters.Instance.DownloadTVThumbnail)
                    {
                        case LookupImageType.Poster:
                            imageLoaded = tvSeriesEntry.GetPosterImage(apiInstance, posterPath);
                            break;
                        case LookupImageType.Banner:
                            imageLoaded = tvSeriesEntry.GetBannerImage(apiInstance, 0, posterPath);
                            break;
                        case LookupImageType.Fanart:
                            imageLoaded = tvSeriesEntry.GetFanArtImage(apiInstance, posterPath);
                            break;
                        case LookupImageType.SmallPoster:
                            imageLoaded = tvSeriesEntry.GetSmallPosterImage(apiInstance, posterPath);
                            break;
                        case LookupImageType.SmallFanart:
                            imageLoaded = tvSeriesEntry.GetSmallFanArtImage(apiInstance, posterPath);
                            break;
                    }
                }
                catch (Exception e)
                {
                    Logger.Instance.Write("<e> TV series lookup has encountered an exception of type " + e.GetType().Name + " when loading a series poster");
                    Logger.Instance.Write("<e> " + e.Message);
                }
            }
            else
            {
                imageGuid = existingEntry.Poster.Value;
                imageLoaded = true;
            }

            if (imageLoaded)
            {
                seriesEntry.Poster = imageGuid;
                
                PosterEntry posterEntry = PosterEntry.FindPosterEntry(posterEntries, imageGuid);
                posterEntry.Count++;

                if (TraceEntry.IsDefined(TraceName.Lookups))
                {
                    if (posterEntry.Count == 1)
                        Logger.Instance.Write("Image downloaded to " + imageGuid);
                    else
                        Logger.Instance.Write("Image reused from " + imageGuid);
                }
            }
            else
            {
                if (TraceEntry.IsDefined(TraceName.Lookups))
                    Logger.Instance.Write("No image loaded");
            }
        }

        private bool needAllData(int seasonNumber, int episodeNumber, string subTitle)
        {
            if (seasonNumber != -1 || episodeNumber != -1)
                return (true);

            return (!string.IsNullOrWhiteSpace(subTitle));
        }

        private TvdbEpisode findEpisode(TvdbSeries series, int seasonNumber, int episodeNumber, string title, string subTitle)
        {
            if (series.Episodes == null)
                return (null);

            if (TraceEntry.IsDefined(TraceName.Lookups))
                Logger.Instance.Write("Looking for episode: season " + seasonNumber + " episode " + episodeNumber +
                    " subtitle " + (string.IsNullOrWhiteSpace(subTitle) ? "n/a" : subTitle) +
                    " in series " + series.SeriesName);

            foreach (TvdbEpisode episode in series.Episodes)
            {
                if (episode.SeasonNumber == seasonNumber && episode.EpisodeNumber == episodeNumber)
                    return (episode);
            }

            if (string.IsNullOrWhiteSpace(subTitle))
                return (null);

            subtitleLookups++;

            foreach (TvdbEpisode episode in series.Episodes)
            {
                if (matchEpisodeNames(episode.EpisodeName, subTitle))
                    return (episode);
            }

            double bestResult = -1;
            TvdbEpisode bestEpisode = null;

            likeSearches++;
            string matchSubTitle = removePunctuation(processEpisodePartNumber(subTitle.Trim().ToLowerInvariant()), punctuation);

            foreach (TvdbEpisode episode in series.Episodes)
            {
                if (!string.IsNullOrWhiteSpace(episode.EpisodeName))
                {
                    string matchEpisodeName = removePunctuation(processNamePartNumber(episode.EpisodeName.Trim().ToLowerInvariant()), punctuation);
                    double result = LookupController.CompareLikeStrings(matchEpisodeName, matchSubTitle);

                    if (result > bestResult)
                    {
                        bestResult = result;
                        bestEpisode = episode;
                    }
                }
                else
                {
                    if (TraceEntry.IsDefined(TraceName.LookupsError))
                        Logger.Instance.Write("Series " + series.SeriesName + " episode " + episode.EpisodeNumber + " has no episode name");
                }
            }

            if (bestResult >= threshold)
            {
                likeSearchesWorked++;

                if (TraceEntry.IsDefined(TraceName.Lookups))
                    Logger.Instance.Write("Episode " + subTitle + " matched (" + bestResult.ToString("0.000") + ") with episode " + bestEpisode.EpisodeName);

                return (bestEpisode);
            }

            if (TraceEntry.IsDefined(TraceName.Lookups))
                Logger.Instance.Write("Best match was " + bestResult.ToString("0.000") + " '" + bestEpisode.EpisodeName + "'");

            foreach (TvdbEpisode episode in series.Episodes)
            {
                if (!string.IsNullOrWhiteSpace(episode.EpisodeName))
                {
                    string matchEpisodeName = removePunctuation(processNamePartNumber(episode.EpisodeName.Trim().ToLowerInvariant()), punctuation);
                    if (matchEpisodeName.StartsWith(matchSubTitle))
                    {
                        if (TraceEntry.IsDefined(TraceName.Lookups))
                            Logger.Instance.Write("Episode " + subTitle + " partial match match with episode " + episode.EpisodeName);

                        return (episode);
                    }
                }
                else
                {
                    if (TraceEntry.IsDefined(TraceName.LookupsError))
                        Logger.Instance.Write("Series " + series.SeriesName + " episode " + episode.EpisodeNumber + " has no episode name");
                }
            }

            return (null);
        }

        private bool matchEpisodeNames(string episodeName, string subTitle)
        {
            if (string.IsNullOrWhiteSpace(episodeName) || string.IsNullOrWhiteSpace(subTitle))
                return (false);

            string trimmedEpisodeName = episodeName.Trim().ToLowerInvariant();
            string trimmedSubTitle = subTitle.Trim().ToLowerInvariant();

            if (trimmedEpisodeName == trimmedSubTitle)
                return (true);

            string matchEpisodeName = removePunctuation(processNamePartNumber(trimmedEpisodeName), punctuation);
            string matchSubTitle = removePunctuation(processEpisodePartNumber(trimmedSubTitle), punctuation);

            if (matchEpisodeName == matchSubTitle)
                return (true);

            string[] parts = matchEpisodeName.Split(new char[] { '(' });
            if (parts.Length == 2 && !string.IsNullOrWhiteSpace(parts[1]) && char.IsDigit(parts[1][0]))
            {
                if (matchSubTitle == parts[0].Trim())
                    return (true);
            }

            return (false);
        }

        private string removePunctuation(string text, string[] punctuation)
        {
            foreach (string element in punctuation)
            {
                if (text.Contains(element))
                    text = text.Replace(element, "");
            }

            return (text.Trim());
        }

        private string processNamePartNumber(string episodeName)
        {
            string identifier1 = "(part ";

            int index = episodeName.IndexOf(identifier1);
            if (index != -1)
                return (episodeName.Substring(0, index) + " (" + episodeName.Substring(index + identifier1.Length, 1) + ")");

            string identifier2 = ": part ";
            index = episodeName.IndexOf(identifier2);
            if (index != -1)
                return (episodeName.Substring(0, index) + " (" + episodeName.Substring(index + identifier2.Length, 1) + ")");

            return (episodeName);
        }

        private string processEpisodePartNumber(string episodeName)
        {
            string identifier1 = ", part ";

            int index = episodeName.IndexOf(identifier1);
            if (index != -1)
                return (episodeName.Substring(0, index) + " (" + episodeName.Substring(index + identifier1.Length, 1) + ")");

            string identifier2 = " - part ";
            index = episodeName.IndexOf(identifier2);
            if (index != -1)
                return (parseEpisodePart(episodeName, index, identifier2));

            string[] nameParts = episodeName.Split(new char[] { ' ' });
            if (nameParts[nameParts.Length - 1].Length > 2)
                return (episodeName);

            string digit = getDigit(nameParts[nameParts.Length - 1]);
            if (digit == null)
                return (episodeName);

            StringBuilder editedName = new StringBuilder();
            foreach (string part in nameParts)
                editedName.Append(part + " ");

            return (editedName + " (" + digit + ")");
        }

        private string parseEpisodePart(string episodeName, int index, string identifier)
        {
            int digitIndex = index + identifier.Length;

            if (episodeName[digitIndex] >= '0' && episodeName[digitIndex] <= '9')
                return (episodeName.Substring(0, index) + " (" + episodeName.Substring(digitIndex) + ")");

            StringBuilder number = new StringBuilder();
            int numberIndex = index + identifier.Length;

            while (numberIndex < episodeName.Length && episodeName[numberIndex] != ' ')
            {
                number.Append(episodeName[numberIndex]);
                numberIndex++;
            }

            string digit = getDigit(number.ToString());
            if (digit == null)
                return (episodeName);

            return (episodeName.Substring(0, index) + " (" + digit + ")");
        }

        private string getDigit(string number)
        {
            string digit = null;

            switch (number)
            {
                case "one":
                case "i":
                    digit = "1";
                    break;
                case "two":
                case "ii":
                    digit = "2";
                    break;
                case "three":
                case "iii":
                    digit = "3";
                    break;
                case "four":
                case "iv":
                    digit = "4";
                    break;
                case "five":
                case "v":
                    digit = "5";
                    break;
                case "six":
                case "vi":
                    digit = "6";
                    break;
                case "seven":
                case "vii":
                    digit = "7";
                    break;
                case "eight":
                case "viii":
                    digit = "8";
                    break;
                case "nine":
                case "ix":
                    digit = "9";
                    break;
                default:
                    break;
            }

            return (digit);
        }

        private TVSeriesEntry findPosterEntry(string title)
        {
            if (title == null)
                return (null);

            string noCaseTitle = title.ToLowerInvariant();

            foreach (TVSeriesEntry tvSeriesEntry in tvSeries)
            {
                if (tvSeriesEntry.Title != null &&
                    tvSeriesEntry.Title.ToLowerInvariant() == noCaseTitle &&
                    tvSeriesEntry.Status == "notfound")
                    return (null);

                if (tvSeriesEntry.Title != null &&
                    tvSeriesEntry.Title.ToLowerInvariant() == noCaseTitle)
                    return (tvSeriesEntry);
            }

            return (null);
        }

        private void processTVSeries(EPGEntry epgEntry, TVSeriesEntry seriesEntry)
        {
            epgEntry.MetaDataTitle = seriesEntry.Title;

            if (epgEntry.EventSubTitle == null)
                epgEntry.EventSubTitle = seriesEntry.MetaDataEpisodeName;

            if (epgEntry.ShortDescription == null)
                epgEntry.ShortDescription = seriesEntry.Overview;

            if (seriesEntry.Cast != null && seriesEntry.Cast.Count != 0)
                epgEntry.Cast = seriesEntry.Cast;

            if (seriesEntry.Directors != null && seriesEntry.Directors.Count != 0)
                epgEntry.Directors = seriesEntry.Directors;

            if (seriesEntry.Writers != null && seriesEntry.Writers.Count != 0)
                epgEntry.Writers = seriesEntry.Writers;

            if (seriesEntry.GuestStars != null && seriesEntry.GuestStars.Count != 0)
                epgEntry.GuestStars = seriesEntry.GuestStars;

            if (seriesEntry.Poster != null)
                epgEntry.Poster = seriesEntry.Poster;

            if (seriesEntry.StarRating != null)
                epgEntry.StarRating = seriesEntry.StarRating;

            /*if (epgEntry.EventCategory == null && seriesEntry.Genre != null)
                epgEntry.EventCategory = "Series," + seriesEntry.Genre;*/
            createEventCategory(epgEntry, seriesEntry);
            
            if (epgEntry.SeasonNumber == -1)
            {
                epgEntry.SeasonNumber = seriesEntry.SeasonNumber;
                epgEntry.EpisodeNumber = seriesEntry.EpisodeNumber;
            }

            epgEntry.SeriesDescription = seriesEntry.SeriesOverview;
            epgEntry.SeriesStartDate = seriesEntry.SeriesStartDate;

            seriesEntry.UsedThisTime = true;
            seriesEntry.DateLastUsed = DateTime.Now;
        }

        private void createEventCategory(EPGEntry epgEntry, TVSeriesEntry seriesEntry)
        {
            if (string.IsNullOrWhiteSpace(seriesEntry.Genre))
                return;
            if (!RunParameters.Instance.LookupIgnoreCategories && epgEntry.EventCategory != null)
                return;

            if (RunParameters.Instance.ImportingToDvbViewer || (CommandLine.PluginMode && !RunParameters.Instance.OutputFileSet))
                return;

            epgEntry.EventCategory = "Series," + seriesEntry.Genre;            
        }

        internal void CreateTVDatabase()
        {
            string databasePath = Path.Combine(RunParameters.DataDirectory, "TV Series Database.xml");
            Logger.Instance.Write("Creating TV series database " + databasePath);

            FileStream fileStream = new FileStream(databasePath, FileMode.Create);

            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Indent = true;
            settings.NewLineOnAttributes = false;
            settings.CloseOutput = true;

            XmlWriter writer = null;

            int outputCount = 0;
            int outputNotFoundCount = 0;
            int entriesDeleted = 0;
            int postersDeleted = 0;
            Collection<TVSeriesEntry> outputEntries = new Collection<TVSeriesEntry>();

            try
            {
                writer = XmlWriter.Create(fileStream, settings);

                writer.WriteStartElement("TVSeries");

                foreach (TVSeriesEntry seriesEntry in tvSeries)
                {
                    if (seriesEntry.UsedThisTime || (seriesEntry.DateLastUsed != null && seriesEntry.DateLastUsed.Value.AddDays(10) > DateTime.Now))
                    {
                        outputEntries.Add(seriesEntry);
                        seriesEntry.Unload(writer);

                        outputCount++;
                        if (seriesEntry.Status == "notfound")
                            outputNotFoundCount++;
                    }
                    else
                    {
                        entriesDeleted++;

                        if (seriesEntry.Poster.HasValue)
                        {
                            PosterEntry posterEntry = PosterEntry.FindPosterEntry(posterEntries, seriesEntry.Poster.Value);
                            posterEntry.Count--;

                            if (posterEntry.Count == 0)
                            {
                                bool deleted = LookupController.DeletePoster("TV Series", seriesEntry.Poster, seriesEntry.OriginalTitle);
                                if (deleted)
                                    postersDeleted++;
                            }
                        }
                    }
                }

                writer.WriteEndElement();

                writer.Close();
            }
            catch (XmlException e)
            {
                Logger.Instance.Write("Failed to unload TV series database");
                Logger.Instance.Write("Data exception: " + e.Message);
                throw;
            }
            catch (IOException e)
            {
                Logger.Instance.Write("Failed to unload TV series database");
                Logger.Instance.Write("I/O exception: " + e.Message);
                throw;
            }

            Logger.Instance.Write("Output " + outputCount + " TV series database entries (not found = " + outputNotFoundCount + ")");
            Logger.Instance.Write("Deleted " + duplicatesDeleted + " duplicate TV series entries");
            Logger.Instance.Write("Deleted " + entriesDeleted + " TV series entries no longer in use");
            Logger.Instance.Write("Deleted " + postersDeleted + " TV series posters no longer in use");

            if (!RunParameters.Instance.LookupImagesInBase)
                cleanPosterDirectory(Path.Combine(RunParameters.ImagePath, "TV Series"), outputEntries);
            else
                UnusedPosters = collectUnusedPosters(RunParameters.ImagePath, outputEntries);
                            
            Logger.Instance.Write("Referenced TV Series posters = " + referencedPosters);
            Logger.Instance.Write("Unreferenced TV Series posters deleted = " + unmatchedDeleted);
            Logger.Instance.Write("Unreferenced TV Series posters not deleted = " + unmatchedNotDeleted);   
        }

        private void cleanPosterDirectory(string posterDirectory, Collection<TVSeriesEntry> outputEntries)
        {
            if (!Directory.Exists(posterDirectory))
                return;

            string[] posterFiles = Directory.GetFiles(posterDirectory, "*.jpg", SearchOption.TopDirectoryOnly);
                
            foreach (string posterName in posterFiles)
            {
                bool found = false;

                FileInfo fileInfo = new FileInfo(posterName);

                foreach (TVSeriesEntry seriesEntry in outputEntries)
                {
                    if (seriesEntry.Poster != null && seriesEntry.Poster.HasValue)
                    {
                        if (seriesEntry.Poster.Value + ".jpg" == fileInfo.Name)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(seriesEntry.OriginalTitle))
                    {
                        if (RunParameters.GetLegalFileName(seriesEntry.OriginalTitle, ' ') + ".jpg" == fileInfo.Name)
                        {
                            found = true;
                            break;
                        }
                    }
                }

                if (!found)
                {
                    try
                    {
                        File.Delete(fileInfo.FullName);
                        Logger.Instance.Write("Unreferenced TV series poster deleted - " + posterName);
                        unmatchedDeleted++;
                    }
                    catch (IOException e)
                    {
                        Logger.Instance.Write("<e> Failed to delete unreferenced TV series poster - " + posterName);
                        Logger.Instance.Write("<e> " + e.Message);
                        unmatchedNotDeleted++;
                    }
                    catch (UnauthorizedAccessException e)
                    {
                        Logger.Instance.Write("<e> Failed to delete unreferenced TV series poster - " + posterName);
                        Logger.Instance.Write("<e> " + e.Message);
                        unmatchedNotDeleted++;
                    }
                }
                else
                    referencedPosters++;
            }
        }

        private Collection<string> collectUnusedPosters(string posterDirectory, Collection<TVSeriesEntry> outputEntries)
        {
            Collection<string> unusedPosters = new Collection<string>();
 
            if (!Directory.Exists(posterDirectory))
                return (unusedPosters);

            string[] posterFiles = Directory.GetFiles(posterDirectory, "*.jpg", SearchOption.TopDirectoryOnly);

            foreach (string posterName in posterFiles)
            {
                bool found = false;

                FileInfo fileInfo = new FileInfo(posterName);

                foreach (TVSeriesEntry seriesEntry in outputEntries)
                {
                    if (seriesEntry.Poster != null && seriesEntry.Poster.HasValue)
                    {
                        if (seriesEntry.Poster.Value + ".jpg" == fileInfo.Name)
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!string.IsNullOrWhiteSpace(seriesEntry.OriginalTitle))
                    {
                        if (RunParameters.GetLegalFileName(seriesEntry.OriginalTitle, ' ') + ".jpg" == fileInfo.Name)
                        {
                            found = true;
                            break;
                        }
                    }
                }

                if (!found)
                    unusedPosters.Add(fileInfo.Name);
                else
                    referencedPosters++;
            }

            return (unusedPosters);
        }

        internal void LogStats()
        {
            Logger.Instance.Write("TV statistics: Web Lookups = " + WebLookups +
                    " Cache lookups = " + CacheLookups +
                    " In-store lookups = " + InStoreLookups +
                    " No data = " + noData +
                    " Outstanding = " + outstanding);

            if (likeSearches != 0)
                Logger.Instance.Write("TV statistics: Subtitle lookups = " + subtitleLookups +
                    " Like searches = " + likeSearches +
                    " Like searches above threshold = " + likeSearchesWorked +
                    " (" + (((double)likeSearchesWorked * 100) / likeSearches).ToString("0.000") + "%)");
            else
                Logger.Instance.Write("TV statistics: Subtitle lookups = " + subtitleLookups +
                    " Like searches = 0" +
                    " Like searches above threshold = 0 (0.000%)");

            if (apiInstance.TotalRequestCount != 0)
            {
                Logger.Instance.Write("TV statistics: Total web requests = " + apiInstance.TotalRequestCount +
                    " Total web request time = " + string.Format("{0:hh\\:mm\\:ss}", apiInstance.TotalRequestTime) +
                    " Average web request time = " + (apiInstance.TotalRequestTime.Value.TotalSeconds / apiInstance.TotalRequestCount).ToString("0.000") + " secs");

                Logger.Instance.Write("TV statistics: Average time between requests = " + (apiInstance.TotalTimeBetweenRequests.Value.TotalSeconds / apiInstance.TotalRequestCount).ToString("0.000") + " secs" +
                    " Minimum time between requests = " + (apiInstance.MinimumTimeBetweenRequests != null ? (apiInstance.MinimumTimeBetweenRequests.Value.TotalMilliseconds / 1000).ToString("0.000") + " secs" : "n/a") +
                    " Maximum time between requests = " + (apiInstance.MaximumTimeBetweenRequests != null ? (apiInstance.MaximumTimeBetweenRequests.Value.TotalMilliseconds / 1000).ToString("0.000") + " secs" : "n/a"));

                if (apiInstance.TotalDelays != 0)
                    Logger.Instance.Write("TV statistics: Total request delays = " + apiInstance.TotalDelays +
                        " Total request delay time = " + ((decimal)apiInstance.TotalDelayTime / 1000).ToString("0.000") + " secs" +
                        " Average request delay time = " + ((((decimal)apiInstance.TotalDelayTime) / apiInstance.TotalDelays) / 1000).ToString("0.000") + " secs");
                else
                    Logger.Instance.Write("TV statistics: Total request delays = 0" +
                    " Total request delay time = 0.000 secs" +
                    " Average request delay time = 0.000 secs");
            }

            Logger.Instance.Write("TV statistics: Web errors = " + webErrors + 
                " default timeout = " + apiInstance.DefaultTimeout +
                " actual timeout = " + apiInstance.ActualTimeout);
        }

        private class TVSeriesEntry
        {
            internal string Title { get; set; }
            internal string MetaDataTitle { get; set; }
            internal int SeasonNumber { get; set; }
            internal int EpisodeNumber { get; set; }
            internal string EpisodeName { get; set; }
            internal string MetaDataEpisodeName { get; set; }
            internal string Overview { get; set; }
            internal Collection<string> Cast { get; set; }
            internal Collection<string> Directors { get; set; }
            internal Collection<string> Writers { get; set; }
            internal Collection<string> GuestStars { get; set; }
            internal string StarRating { get; set; }
            internal string Genre { get; set; }
            internal Guid? Poster { get; set; }
            internal bool UsedThisTime { get; set; }
            internal string Status { get; set; }
            internal DateTime? DateLastUsed { get; set; }

            internal string SeriesOverview { get; set; }
            internal DateTime? SeriesStartDate { get; set; }

            internal string OriginalTitle { get; set; }

            internal bool Found { get { return (Status != "notfound"); } }

            private TVSeriesEntry()
            {
                Status = "found";

                SeasonNumber = -1;
                EpisodeNumber = -1;

                DateLastUsed = DateTime.Now.AddDays(-1);
            }

            internal TVSeriesEntry(string title, string metaDataTitle) : this()
            {
                Title = title;
                OriginalTitle = title;
                MetaDataTitle = metaDataTitle;
            }

            internal void Load(XmlReader reader)
            {
                while (!reader.EOF)
                {
                    reader.Read();
                    if (reader.IsStartElement())
                    {
                        switch (reader.Name.ToLowerInvariant())
                        {
                            case "seasonnumber":
                                try
                                {
                                    SeasonNumber = Int32.Parse(reader.ReadString());
                                }
                                catch (FormatException) { }
                                catch (OverflowException) { }
                                break;
                            case "episodenumber":
                                try
                                {
                                    EpisodeNumber = Int32.Parse(reader.ReadString());
                                }
                                catch (FormatException) { }
                                catch (OverflowException) { }
                                break;
                            case "episodename":
                                EpisodeName = reader.ReadString();
                                break;
                            case "metadataepisodename":
                                MetaDataEpisodeName = reader.ReadString();
                                break;
                            case "overview":
                                Overview = reader.ReadString();
                                break;
                            case "actor":
                                if (Cast == null)
                                    Cast = new Collection<string>();
                                Cast.Add(reader.ReadString());
                                break;
                            case "director":
                                if (Directors == null)
                                    Directors = new Collection<string>();
                                Directors.Add(reader.ReadString());
                                break;
                            case "writer":
                                if (Writers == null)
                                    Writers = new Collection<string>();
                                Writers.Add(reader.ReadString());
                                break;
                            case "gueststar":
                                if (GuestStars == null)
                                    GuestStars = new Collection<string>();
                                GuestStars.Add(reader.ReadString());
                                break;
                            case "starrating":
                                StarRating = reader.ReadString();
                                break;
                            case "genre":
                                Genre = reader.ReadString();
                                break;
                            case "poster":
                                Poster = Guid.Parse(reader.ReadString());
                                break;
                            case "datelastused":
                                DateLastUsed = DateTime.Parse(reader.ReadString(), CultureInfo.InvariantCulture);
                                break;
                            case "seriesoverview":
                                SeriesOverview = reader.ReadString();
                                break;
                            case "seriesstartdate":
                                SeriesStartDate = DateTime.Parse(reader.ReadString(), CultureInfo.InvariantCulture);
                                break;
                            case "originaltitle":
                                OriginalTitle = reader.ReadString();
                                break;
                            default:
                                break;
                        }
                    }
                }
            }

            internal void Unload(XmlWriter xmlWriter)
            {
                xmlWriter.WriteStartElement("series");

                xmlWriter.WriteAttributeString("title", Title);
                xmlWriter.WriteAttributeString("metaDataTitle", MetaDataTitle);
                xmlWriter.WriteAttributeString("status", Status);

                if (SeasonNumber != -1)
                    xmlWriter.WriteElementString("seasonNumber", SeasonNumber.ToString());
                if (EpisodeNumber != -1)
                    xmlWriter.WriteElementString("episodeNumber", EpisodeNumber.ToString());

                if (EpisodeName != null)
                    xmlWriter.WriteElementString("episodeName", EpisodeName);
                if (MetaDataEpisodeName != null)
                    xmlWriter.WriteElementString("metaDataEpisodeName", MetaDataEpisodeName);
                if (Overview != null)
                    xmlWriter.WriteElementString("overview", Overview);

                if (Cast != null && Cast.Count != 0)
                {
                    xmlWriter.WriteStartElement("cast");
                    foreach (string actor in Cast)
                        xmlWriter.WriteElementString("actor", actor);
                    xmlWriter.WriteEndElement();
                }

                if (Directors != null && Directors.Count != 0)
                {
                    xmlWriter.WriteStartElement("directors");
                    foreach (string director in Directors)
                        xmlWriter.WriteElementString("director", director);
                    xmlWriter.WriteEndElement();
                }

                if (Writers != null && Writers.Count != 0)
                {
                    xmlWriter.WriteStartElement("writers");
                    foreach (string writer in Writers)
                        xmlWriter.WriteElementString("writer", writer);
                    xmlWriter.WriteEndElement();
                }

                if (GuestStars != null && GuestStars.Count != 0)
                {
                    xmlWriter.WriteStartElement("guestStars");
                    foreach (string guestStar in GuestStars)
                        xmlWriter.WriteElementString("guestStar", guestStar);
                    xmlWriter.WriteEndElement();
                }

                if (StarRating != null)
                    xmlWriter.WriteElementString("starRating", StarRating);

                if (Genre != null)
                    xmlWriter.WriteElementString("genre", Genre);

                if (Poster.HasValue)
                    xmlWriter.WriteElementString("poster", Poster.ToString());

                if (DateLastUsed != null)
                    xmlWriter.WriteElementString("dateLastUsed", DateLastUsed.Value.ToString(CultureInfo.InvariantCulture));

                if (!string.IsNullOrWhiteSpace(OriginalTitle) && OriginalTitle != Title)
                    xmlWriter.WriteElementString("originalTitle", OriginalTitle);

                xmlWriter.WriteEndElement();
            }

            internal static TVSeriesEntry CreateNotFoundEntry(string title, string subTitle, string description, int seasonNumber, int episodeNumber)
            {
                TVSeriesEntry tvSeries = new TVSeriesEntry(title, string.Empty);
                tvSeries.EpisodeName = subTitle;
                tvSeries.Overview = description;
                tvSeries.SeasonNumber = seasonNumber;
                tvSeries.EpisodeNumber = episodeNumber;

                tvSeries.Status = "notfound";
                tvSeries.UsedThisTime = true;
                tvSeries.DateLastUsed = DateTime.Now;

                return (tvSeries);
            }

            public override string ToString()
            {
                return ("Title: " + Title +
                    " Subtitle: " + (EpisodeName != null ? EpisodeName : "n/a") +
                    " Season: " + SeasonNumber +
                    " Episode: " + EpisodeNumber +
                    " Overview: " + Overview);
            }
        }

        private class CacheEntry
        {
            internal string Title { get; private set; }
            internal TvdbSeries Series { get; private set; }

            private CacheEntry() { }

            internal CacheEntry(string title, TvdbSeries series)
            {
                Title = title;
                Series = series;
            }
        }
    }
}
