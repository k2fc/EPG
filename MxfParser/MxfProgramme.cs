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

using System.Collections.ObjectModel;
using System.Xml;
using System.IO;

using DomainObjects;

namespace MxfParser
{
    /// <summary>
    /// The class that describes an MXF programme.
    /// </summary>
    public class MxfProgramme
    {
        /// <summary>
        /// Get or set the collection of programmes in an MXF file.
        /// </summary>
        public static Collection<MxfProgramme> Programmes { get; set; }

        /// <summary>
        /// Get the ID of the programme.
        /// </summary>
        public string Id { get; private set; }

        /// <summary>
        /// Get the UID of the programme.
        /// </summary>
        public string Uid { get; private set; }

        /// <summary>
        /// Get the title of the programme.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Get the description of the programme.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// Get the episode title of the programme.
        /// </summary>
        public string EpisodeTitle { get; private set; }

        /// <summary>
        /// Get the 'adult' flag of the programme.
        /// </summary>
        public string HasAdult { get; private set; }

        /// <summary>
        /// Get the 'graphic language' flag of the programme.
        /// </summary>
        public string HasGraphicLanguage { get; private set; }

        /// <summary>
        /// Get the 'graphic violence' flag of the programme.
        /// </summary>
        public string HasGraphicViolence { get; private set; }

        /// <summary>
        /// Get the 'nudity' flag of the programme.
        /// </summary>
        public string HasNudity { get; private set; }

        /// <summary>
        /// Get the 'strong sexual content' flag of the programme.
        /// </summary>
        public string HasStrongSexualContent { get; private set; }

        /// <summary>
        /// Get the MPAA rating of the programme.
        /// </summary>
        public string MpaaRating { get; private set; }

        /// <summary>
        /// Get the keywords for the programme.
        /// </summary>
        public string KeyWords { get; private set; }

        /// <summary>
        /// Get the series flag for the programme.
        /// </summary>
        public string IsSeries { get; private set; }

        /// <summary>
        /// Get the series for the programme.
        /// </summary>
        public string Series { get; private set; }

        /// <summary>
        /// Get the year of the programme.
        /// </summary>
        public string Year { get; private set; }

        /// <summary>
        /// Get the season number of the programme.
        /// </summary>
        public string SeasonNumber { get; private set; }

        /// <summary>
        /// Get the episode number of the programme.
        /// </summary>
        public string EpisodeNumber { get; private set; }

        /// <summary>
        /// Get the original air date of the programme.
        /// </summary>
        public string OriginalAirDate { get; private set; }

        /// <summary>
        /// Get the star rating of the programme.
        /// </summary>
        public string HalfStars { get; private set; }

        /// <summary>
        /// Get the special flag for the programme.
        /// </summary>
        public string IsSpecial { get; private set; }

        /// <summary>
        /// Get the movie flag for the programme.
        /// </summary>
        public string IsMovie { get; private set; }

        /// <summary>
        /// Get the sports flag for the programme.
        /// </summary>
        public string IsSports { get; private set; }

        /// <summary>
        /// Get the news flag for the programme.
        /// </summary>
        public string IsNews { get; private set; }

        /// <summary>
        /// Get the series flag for the programme.
        /// </summary>
        public string IsKids{ get; private set; }

        /// <summary>
        /// Get the guide image for the programme.
        /// </summary>
        public string GuideImage { get; private set; }

        /// <summary>
        /// Get the cast list for the programme.
        /// </summary>
        public Collection<string> Actors { get; private set; }

        /// <summary>
        /// Get the director list for the programme.
        /// </summary>
        public Collection<string> Directors { get; private set; }

        /// <summary>
        /// Get the producer list for the programme.
        /// </summary>
        public Collection<string> Producers { get; private set; }

        /// <summary>
        /// Get the writer list for the programme.
        /// </summary>
        public Collection<string> Writers { get; private set; }

        /// <summary>
        /// Get the host list for the programme.
        /// </summary>
        public Collection<string> Hosts { get; private set; }

        /// <summary>
        /// Get the guest star list for the programme.
        /// </summary>
        public Collection<string> GuestStars { get; private set; }

        private MxfProgramme() { }

        private bool load(XmlReader xmlReader, XmlReader personReader)
        {
            try
            {
                Id = xmlReader.GetAttribute("id");
                Uid = xmlReader.GetAttribute("uid");
                Title = xmlReader.GetAttribute("title");
                Description = xmlReader.GetAttribute("description");
                EpisodeTitle = xmlReader.GetAttribute("episodeTitle");
                HasAdult = xmlReader.GetAttribute("hasAdult");
                HasGraphicLanguage = xmlReader.GetAttribute("hasGraphicLanguage");
                HasGraphicViolence = xmlReader.GetAttribute("hasGraphicViolence");
                HasNudity = xmlReader.GetAttribute("hasNudity");
                HasStrongSexualContent = xmlReader.GetAttribute("hasStrongSexualContent");
                MpaaRating = xmlReader.GetAttribute("mpaaRating");
                KeyWords = xmlReader.GetAttribute("keywords");
                Year = xmlReader.GetAttribute("year");
                SeasonNumber = xmlReader.GetAttribute("seasonNumber");
                EpisodeNumber = xmlReader.GetAttribute("episodeNumber");
                OriginalAirDate = xmlReader.GetAttribute("originalAirdate");
                IsSeries = xmlReader.GetAttribute("isSeries");
                Series = xmlReader.GetAttribute("series");
                HalfStars = xmlReader.GetAttribute("halfStars");
                IsSpecial = xmlReader.GetAttribute("isSpecial");
                IsMovie = xmlReader.GetAttribute("isMovie");
                IsSports = xmlReader.GetAttribute("isSports");
                IsNews = xmlReader.GetAttribute("isNews");
                IsKids = xmlReader.GetAttribute("isKids");
                GuideImage = xmlReader.GetAttribute("guideImage");

                while (!personReader.EOF)
                {
                    if (personReader.IsStartElement())
                    {
                        switch (personReader.Name.ToLowerInvariant())
                        {
                            case "actorrole":
                                if (Actors == null)
                                    Actors = new Collection<string>();
                                Actors.Add(personReader.GetAttribute("person"));
                                break;
                            case "directorrole":
                                if (Directors == null)
                                    Directors = new Collection<string>();
                                Directors.Add(personReader.GetAttribute("person"));
                                break;
                            case "producerrole":
                                if (Producers == null)
                                    Producers = new Collection<string>();
                                Producers.Add(personReader.GetAttribute("person"));
                                break;
                            case "writerrole":
                                if (Writers == null)
                                    Writers = new Collection<string>();
                                Writers.Add(personReader.GetAttribute("person"));
                                break;
                            case "hostrole":
                                if (Hosts == null)
                                    Hosts = new Collection<string>();
                                Hosts.Add(personReader.GetAttribute("person"));
                                break;
                            case "guestrole":
                                if (GuestStars == null)
                                    GuestStars = new Collection<string>();
                                GuestStars.Add(personReader.GetAttribute("person"));
                                break;
                            default:
                                break;
                        }
                    }

                    personReader.Read();
                }
            }
            catch (XmlException e)
            {
                Logger.Instance.Write("Failed to load mxf channel");
                Logger.Instance.Write("Data exception: " + e.Message);
                return (false);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("Failed to load mxf channel");
                Logger.Instance.Write("I/O exception: " + e.Message);
                return (false);
            }

            personReader.Close();

            return (true);
        }

        internal static MxfProgramme FindProgramme(string id)
        {
            if (Programmes == null)
                return (null);

            foreach (MxfProgramme programme in Programmes)
            {
                if (programme.Id == id)
                    return (programme);
            }

            return (null);
        }

        /// <summary>
        /// Get a new instance of the MxfProgramme class with data loaded.
        /// </summary>
        /// <param name="xmlReader">An XmlReader instance for the program tag.</param>
        /// <param name="personReader">An XmlReader instance for the subtrees.</param>
        /// <returns>An MxfProgramme instance with data loaded.</returns>
        public static MxfProgramme GetInstance(XmlReader xmlReader, XmlReader personReader)
        {
            MxfProgramme instance = new MxfProgramme();
            instance.load(xmlReader, personReader);

            return (instance);
        }
    }
}
