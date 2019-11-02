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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Web;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;

using DomainObjects;

namespace Lookups
{
    /// <summary>
    /// The class that controls metadata lookup.
    /// </summary>
    public sealed class LookupController
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
        /// Get the full assembly version number of the TMDB library.
        /// </summary>
        public static string TmdbAssemblyVersion { get { return (TheMovieDB.TmdbAPI.AssemblyVersion); } }

        /// <summary>
        /// Get the full assembly version number of the TVDB library.
        /// </summary>
        public static string TvdbAssemblyVersion { get { return (TheTvDB.TvdbAPI.AssemblyVersion); } }

        /// <summary>
        /// The possible replies from metadata lookup.
        /// </summary>
        public enum LookupReply
        {
            /// <summary>
            /// Metadata was added from existing information.
            /// </summary>
            InStore,
            /// <summary>
            /// Metadata was added from the web databases.
            /// </summary>
            WebLookup,
            /// <summary>
            /// No data located.
            /// </summary>
            NoData,
            /// <summary>
            /// The EPG entry is not considered to be a movie.
            /// </summary>
            NotMovie,
            /// <summary>
            /// The EPG entry is not considered a TV series.
            /// </summary>
            NotTVSeries,
            /// <summary>
            /// The type of lookup has not been enabled by the user.
            /// </summary>
            NotEnabled,
            /// <summary>
            /// The EPG entry has not been processed.
            /// </summary>
            NotProcessed
        }
        
        private static MovieLookup movieLookup;
        private static TVLookup tvLookup;

        private LookupController() { }

        /// <summary>
        /// Run the metadata lookup process.
        /// </summary>
        /// <param name="stations">The channels to process.</param>
        public static void Process(Collection<TVStation> stations)
        {
            Logger.Instance.WriteSeparator("Metadata Lookup Processing");

            if (!RunParameters.Instance.MovieLookupEnabled && !RunParameters.Instance.TVLookupEnabled)
            {
                Logger.Instance.Write("Movie and TV lookups not enabled");
                return;
            }
            else
            {
                if (RunParameters.Instance.MovieLookupEnabled && RunParameters.Instance.TVLookupEnabled)
                    Logger.Instance.Write("Movie and TV lookups processing starting");
                else
                {
                    if (RunParameters.Instance.MovieLookupEnabled)
                        Logger.Instance.Write("Movie lookup processing starting");
                    else
                        Logger.Instance.Write("TV lookup processing starting");
                }
            }

            Logger.Instance.Write("Lookup processing time limit is " + RunParameters.Instance.LookupTimeLimit + " minutes");
            DateTime startTime = DateTime.Now;

            movieLookup = new MovieLookup();
            tvLookup = new TVLookup();

            int totalEntries = 0;
            
            foreach (TVStation station in stations)
            {
                if (checkChannelIncluded(station))
                {
                    Logger.Instance.Write("Processing " + station.Name);

                    int startLookups = movieLookup.InStoreLookups + movieLookup.WebLookups + 
                        tvLookup.InStoreLookups + tvLookup.WebLookups + tvLookup.CacheLookups;

                    foreach (EPGEntry epgEntry in station.EPGCollection)
                    {
                        if (!RunParameters.Instance.AbandonRequested)
                        {
                            if (!string.IsNullOrWhiteSpace(epgEntry.EventName) && !epgEntry.NoLookup)
                            {
                                totalEntries++;

                                switch (processEPGEntry(epgEntry, movieLookup))
                                {
                                    case LookupReply.InStore:
                                        break;
                                    case LookupReply.NoData:
                                        switch (processEPGEntry(epgEntry, tvLookup))
                                        {
                                            case LookupReply.InStore:
                                                break;
                                            case LookupReply.NoData:
                                                break;
                                            case LookupReply.WebLookup:
                                                break;
                                            case LookupReply.NotProcessed:
                                                break;
                                            default:
                                                break;
                                        }
                                        break;
                                    case LookupReply.WebLookup:
                                        break;
                                    case LookupReply.NotProcessed:
                                        break;
                                    case LookupReply.NotMovie:
                                    case LookupReply.NotEnabled:
                                        switch (processEPGEntry(epgEntry, tvLookup))
                                        {
                                            case LookupReply.InStore:
                                                break;
                                            case LookupReply.NoData:
                                                break;
                                            case LookupReply.WebLookup:
                                                break;
                                            case LookupReply.NotProcessed:
                                                break;
                                            default:
                                                break;
                                        }
                                        break;
                                    default:
                                        break;
                                }
                            }                            
                        }
                        else
                            break;                        
                    }

                    if (station.EPGCollection.Count != 0)
                    {
                        int endLookups = movieLookup.InStoreLookups + movieLookup.WebLookups + 
                            tvLookup.InStoreLookups + tvLookup.WebLookups + tvLookup.CacheLookups;

                        int lookupsCount = endLookups - startLookups;

                        Logger.Instance.Write("Processed " + station.Name +
                            " EPG entries = " + station.EPGCollection.Count +
                            " EPG entries with metadata added = " + lookupsCount + 
                            " (" + ((lookupsCount * 100) / station.EPGCollection.Count) + "%)");
                    }
                    else
                        Logger.Instance.Write("Processed " + station.Name + " No EPG entries");

                }
            }            

            if (RunParameters.Instance.AbandonRequested)
            {
                Logger.Instance.Write("Lookup processing abandoned");
                return;
            }

            string finished = null;
            
            if (DateTime.Now < startTime.AddMinutes(RunParameters.Instance.LookupTimeLimit))
                finished = "completed";
            else
                finished = "timed out";

            if (totalEntries != 0)
            {
                if (movieLookup.Initialized)
                    movieLookup.CreateMovieDatabase();
                if (tvLookup.Initialized)
                    tvLookup.CreateTVDatabase();
                Logger.Instance.Write("Lookup processing " + finished);

                if (RunParameters.Instance.LookupImagesInBase && movieLookup.Initialized && tvLookup.Initialized)
                    cleanPosterDirectory(movieLookup.UnusedPosters, tvLookup.UnusedPosters);

                if (movieLookup.Initialized)
                    movieLookup.LogStats();
                if (tvLookup.Initialized)
                    tvLookup.LogStats();
            }

            int totalMatched = movieLookup.InStoreLookups + movieLookup.WebLookups + 
                tvLookup.InStoreLookups + tvLookup.WebLookups + tvLookup.CacheLookups;

            int percent = totalEntries != 0 ? (totalMatched * 100) / totalEntries : 0;
            Logger.Instance.Write("Total EPG entries = " + totalEntries +
                " Total EPG entries with metadata added = " + totalMatched + " (" + percent + "%)");
            
            HistoryRecord.Current.LookupResult = finished.Substring(0, 1).ToUpperInvariant() + finished.Substring(1);
            HistoryRecord.Current.LookupDuration = DateTime.Now - startTime;
            HistoryRecord.Current.LookupRate = percent;
        }

        private static bool checkChannelIncluded(TVStation station)
        {
            if (!station.Included)
                return (false);

            if (station.IsRadio)
                return (false);

            return (true);
        }

        private static LookupReply processEPGEntry(EPGEntry epgEntry, MovieLookup lookup)
        {
            if (!RunParameters.Instance.MovieLookupEnabled || !movieLookup.Initialized)
                return (LookupReply.NotEnabled);

            if (!string.IsNullOrWhiteSpace(epgEntry.EventSubTitle))
                return (LookupReply.NotMovie);

            return (lookup.Process(epgEntry));
        }

        private static LookupReply processEPGEntry(EPGEntry epgEntry, TVLookup lookup)
        {
            if (!RunParameters.Instance.TVLookupEnabled || !tvLookup.Initialized)
                return (LookupReply.NotEnabled);

            return (lookup.Process(epgEntry));
        }

        private static void cleanPosterDirectory(Collection<string> unusedMoviePosters, Collection<string> unusedTVSeriesPosters)
        {
            int unmatchedDeleted = 0;
            int unmatchedNotDeleted = 0;

            string[] posterFiles = Directory.GetFiles(RunParameters.ImagePath, "*.jpg", SearchOption.TopDirectoryOnly);

            foreach (string posterName in posterFiles)
            {
                FileInfo fileInfo = new FileInfo(posterName);

                if (unusedMoviePosters.Contains(fileInfo.Name) && unusedTVSeriesPosters.Contains(fileInfo.Name))
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
            }

            Logger.Instance.Write("Unreferenced posters deleted = " + unmatchedDeleted);
            Logger.Instance.Write("Unreferenced posters not deleted = " + unmatchedNotDeleted);  
        }

        internal static string GetStarRating(decimal programRating)
        {
            decimal rating = programRating / 2;

            if (rating < (decimal)0.5)
                return (null);
            if (rating < 1)
                return ("+");
            if (rating < (decimal)1.5)
                return ("*");
            if (rating < 2)
                return ("*+");
            if (rating < (decimal)2.5)
                return ("**");
            if (rating < 3)
                return ("**+");
            if (rating < (decimal)3.5)
                return ("***");
            if (rating < 4)
                return ("***+");
            return ("****");
        }

        internal static string ReplaceHtmlChars(string inputString)
        {
            return (HttpUtility.HtmlDecode(inputString));
        }

        internal static MatchResult FindBestMatch(string title, Collection<string> titleList)
        {
            if (titleList.Count == 1)
                return (new MatchResult(0, 0));

            Collection<int> ratings = new Collection<int>();

            string lowerCaseTitle = title.ToLowerInvariant();

            foreach (string titleEntry in titleList)
                ratings.Add(calculateDistance(lowerCaseTitle, titleEntry.ToLowerInvariant()));

            int minimumIndex = 0;
            int minimumRating = int.MaxValue;

            foreach (int rating in ratings)
            {
                if (rating < minimumRating)
                {
                    minimumIndex = ratings.IndexOf(rating);
                    minimumRating = rating;
                }
            }

            return (new MatchResult(minimumIndex, minimumRating));
        }

        private static int calculateDistance(string referenceString, string currentString)
        {
            int[,] matrix = new int[referenceString.Length + 1, currentString.Length + 1];
            int index1;
            int index2;
            int cost;

            char[] referenceCharArray = referenceString.ToCharArray();
            char[] currentCharArray = currentString.ToCharArray();

            for (index1 = 0; index1 <= referenceCharArray.Length; index1++)
                matrix[index1, 0] = index1;

            for (index2 = 0; index2 <= currentCharArray.Length; index2++)
                matrix[0, index2] = index2;

            for (index1 = 1; index1 <= referenceCharArray.Length; index1++)
            {
                for (index2 = 1; index2 <= currentCharArray.Length; index2++)
                {

                    if (referenceCharArray[index1 - 1] == currentCharArray[index2 - 1])
                        cost = 0;
                    else
                        cost = 1;

                    matrix[index1, index2] = Math.Min(matrix[index1 - 1, index2] + 1,
                            Math.Min(matrix[index1, index2 - 1] + 1, matrix[index1 - 1, index2 - 1] + cost));

                    if ((index1 > 1) && (index2 > 1) && (referenceCharArray[index1 - 1] ==
                        currentCharArray[index2 - 2]) && (referenceCharArray[index1 - 2] == currentCharArray[index2 - 1]))
                    {
                        matrix[index1, index2] = Math.Min(matrix[index1, index2], matrix[index1 - 2, index2 - 2] + cost);
                    }
                }
            }

            return (matrix[referenceCharArray.Length, currentCharArray.Length]);
        }

        
        /// <summary>
        /// Compares the two strings based on letter pair matches
        /// </summary>
        /// <param name="str1"></param>
        /// <param name="str2"></param>
        /// <returns>The percentage match from 0.0 to 1.0 where 1.0 is 100%</returns>
        internal static double CompareLikeStrings(string str1, string str2)
        {
            List<string> pairs1 = wordLetterPairs(str1.ToUpper());
            List<string> pairs2 = wordLetterPairs(str2.ToUpper());

            int intersection = 0;
            int union = pairs1.Count + pairs2.Count;

            for (int i = 0; i < pairs1.Count; i++)
            {
                for (int j = 0; j < pairs2.Count; j++)
                {
                    if (pairs1[i] == pairs2[j])
                    {
                        intersection++;
                        pairs2.RemoveAt(j);     //Must remove the match to prevent "GGGG" from appearing to match "GG" with 100% success
                        break;
                    }
                }
            }

            return ((2.0 * intersection) / union);
        }

        /// <summary>
        /// Gets all letter pairs for each
        /// individual word in the string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static List<string> wordLetterPairs(string str)
        {
            List<string> allPairs = new List<string>();

            // Tokenize the string and put the tokens/words into an array
            string[] words = Regex.Split(str, @"\s");

            // For each word

            for (int w = 0; w < words.Length; w++)
            {
                if (!string.IsNullOrEmpty(words[w]))
                {
                    // Find the pairs of characters
                    String[] pairsInWord = letterPairs(words[w]);

                    for (int p = 0; p < pairsInWord.Length; p++)
                        allPairs.Add(pairsInWord[p]);
                }
            }
            
            return (allPairs);
        }
        
        /// <summary>
        /// Generates an array containing every
        /// two consecutive letters in the input string
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private static string[] letterPairs(string str)
        {
            int numPairs = str.Length - 1;
            string[] pairs = new string[numPairs];
            
            for (int i = 0; i < numPairs; i++)
                pairs[i] = str.Substring(i, 2);

            return (pairs);
        }

        internal static void ClearPosterDirectory(string directoryPath)
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

        internal static bool DeletePoster(string type, Guid? posterGuid, string originalTitle)
        {
            string posterPath = Path.Combine(RunParameters.ImagePath, type, posterGuid + ".jpg");
            bool deleted = deletePoster(posterPath);
            if (deleted)
                return (true);

            posterPath = Path.Combine(RunParameters.ImagePath, posterGuid + ".jpg");
            deleted = deletePoster(posterPath);
            if (deleted)
                return (true);

            if (string.IsNullOrWhiteSpace(originalTitle))
                return (false);

            string legalFileName = RunParameters.GetLegalFileName(originalTitle, ' ');

            posterPath = Path.Combine(RunParameters.ImagePath, type, legalFileName + ".jpg");
            deleted = deletePoster(posterPath);
            if (deleted)
                return (true);

            posterPath = Path.Combine(RunParameters.ImagePath, legalFileName + ".jpg");
            deleted = deletePoster(posterPath);

            return (deleted);
        }

        private static bool deletePoster(string posterPath)
        {
            try
            {
                File.Delete(posterPath);
                return (true);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("<e>Failed to delete TV poster image " + posterPath);
                Logger.Instance.Write("<e> " + e.Message);
                return (false);
            }
        }
    }
}
