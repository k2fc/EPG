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

namespace XmltvParser
{
    /// <summary>
    /// The class that describes an XMLTV programme.
    /// </summary>
    public class XmltvProgramme
    {
        /// <summary>
        /// Get or set the list of programmes.
        /// </summary>
        public static Collection<XmltvProgramme> Programmes { get; set; }

        /// <summary>
        /// Get the start time.
        /// </summary>
        public XmltvTime StartTime { get; private set; }
        /// <summary>
        /// Get the stop time.
        /// </summary>
        public XmltvTime StopTime { get; private set; }
        /// <summary>
        /// Get the PDC start time.
        /// </summary>
        public string PdcStart { get; private set; }
        /// <summary>
        /// Get the VPS start time.
        /// </summary>
        public string VpsStart { get; private set; }
        /// <summary>
        /// Get the show view value.
        /// </summary>
        public string ShowView { get; private set; }
        /// <summary>
        /// Get the video plus code.
        /// </summary>
        public string VideoPlus { get; private set; }
        /// <summary>
        /// Get the channel.
        /// </summary>
        public string Channel { get; private set; }
        /// <summary>
        /// Get the climp IDX value.
        /// </summary>
        public string ClumpIdx { get; private set; }

        /// <summary>
        /// Get the titles.
        /// </summary>
        public Collection<XmltvText> Titles { get; private set; }
        /// <summary>
        /// Get the sub-titles.
        /// </summary>
        public Collection<XmltvText> SubTitles { get; private set; }
        /// <summary>
        /// Get the descriptions.
        /// </summary>
        public Collection<XmltvText> Descriptions { get; private set; }

        /// <summary>
        /// Get the directors.
        /// </summary>
        public Collection<XmltvPerson> Directors { get; private set; }
        /// <summary>
        /// Get the actors.
        /// </summary>
        public Collection<XmltvPerson> Actors { get; private set; }
        /// <summary>
        /// Get the writers.
        /// </summary>
        public Collection<XmltvPerson> Writers { get; private set; }
        /// <summary>
        /// Get the adapters.
        /// </summary>
        public Collection<XmltvPerson> Adapters { get; private set; }
        /// <summary>
        /// Get the producers.
        /// </summary>
        public Collection<XmltvPerson> Producers { get; private set; }
        /// <summary>
        /// Get the presenters.
        /// </summary>
        public Collection<XmltvPerson> Presenters { get; private set; }
        /// <summary>
        /// Get the commentators.
        /// </summary>
        public Collection<XmltvPerson> Commentators { get; private set; }
        /// <summary>
        /// Get the guests.
        /// </summary>
        public Collection<XmltvPerson> Guests { get; private set; }

        /// <summary>
        /// Get the date.
        /// </summary>
        public string Date { get; private set; }

        /// <summary>
        /// Get the programme categories.
        /// </summary>
        public Collection<XmltvText> Categories { get; private set; }
        /// <summary>
        /// Get the language.
        /// </summary>
        public XmltvText Language { get; private set; }
        /// <summary>
        /// Get the original language.
        /// </summary>
        public XmltvText OriginalLanguage { get; private set; }
        /// <summary>
        /// Get the duration.
        /// </summary>
        public XmltvLength Length { get; private set; }
        /// <summary>
        /// Get the icon.
        /// </summary>
        public XmltvIcon Icon { get; private set; }
        /// <summary>
        /// Get the country code.
        /// </summary>
        public XmltvText Country { get; private set; }
        /// <summary>
        /// Get the episode number.
        /// </summary>
        public Collection<XmltvEpisodeNumber> EpisodeNumbers { get; private set; }
        /// <summary>
        /// Get the video quality.
        /// </summary>
        public XmltvVideo Video { get; private set; }
        /// <summary>
        /// Get the audio quality.
        /// </summary>
        public XmltvAudio Audio { get; private set; }
        /// <summary>
        /// Get the previously shown tag.
        /// </summary>
        public XmltvPreviouslyShown PreviouslyShown { get; private set; }
        /// <summary>
        /// Get the premiere tag.
        /// </summary>
        public XmltvText Premiere { get; private set; }
        /// <summary>
        /// Get the last chance tag.
        /// </summary>
        public XmltvText LastChance { get; private set; }
        /// <summary>
        /// Get the new tag.
        /// </summary>
        public bool New { get; private set; }
        /// <summary>
        /// Get the subtitling tags.
        /// </summary>
        public Collection<XmltvSubtitling> Subtitlings { get; private set; }
        /// <summary>
        /// Get the rating.
        /// </summary>
        public XmltvRating Rating { get; private set; }
        /// <summary>
        /// Get the star rating.
        /// </summary>
        public XmltvStarRating StarRating { get; private set; }
        /// <summary>
        /// Get the review.
        /// </summary>
        public XmltvReview Review { get; private set; }

        private XmltvProgramme() { }

        private bool load(XmlReader xmlReader)
        {
            try
            {
                while (!xmlReader.EOF)
                {
                    if (xmlReader.IsStartElement())
                    {
                        switch (xmlReader.Name.ToLowerInvariant())
                        {
                            case "programme":
                                StartTime = XmltvTime.GetInstance(xmlReader.GetAttribute("start"));
                                StopTime = XmltvTime.GetInstance(xmlReader.GetAttribute("stop"));
                                PdcStart = xmlReader.GetAttribute("pdc-start");
                                VpsStart = xmlReader.GetAttribute("vps-start");
                                ShowView = xmlReader.GetAttribute("showview");
                                VideoPlus = xmlReader.GetAttribute("videoplus");
                                Channel = xmlReader.GetAttribute("channel");
                                ClumpIdx = xmlReader.GetAttribute("clumpidx");
                                break;
                            case "title":
                                if (Titles == null)
                                    Titles = new Collection<XmltvText>();
                                Titles.Add(XmltvText.GetInstance(xmlReader));
                                break;
                            case "sub-title":
                                if (SubTitles == null)
                                    SubTitles = new Collection<XmltvText>();
                                SubTitles.Add(XmltvText.GetInstance(xmlReader));
                                break;
                            case "desc":
                                if (Descriptions == null)
                                    Descriptions = new Collection<XmltvText>();
                                Descriptions.Add(XmltvText.GetInstance(xmlReader));
                                break;
                            case "director":
                                if (Directors == null)
                                    Directors = new Collection<XmltvPerson>();
                                Directors.Add(XmltvPerson.GetInstance(xmlReader));
                                break;
                            case "actor":
                                if (Actors == null)
                                    Actors = new Collection<XmltvPerson>();
                                Actors.Add(XmltvPerson.GetInstance(xmlReader));
                                break;
                            case "writer":
                                if (Writers == null)
                                    Writers = new Collection<XmltvPerson>();
                                Writers.Add(XmltvPerson.GetInstance(xmlReader));
                                break;
                            case "adapter":
                                if (Adapters == null)
                                    Adapters = new Collection<XmltvPerson>();
                                Adapters.Add(XmltvPerson.GetInstance(xmlReader));
                                break;
                            case "producer":
                                if (Producers == null)
                                    Producers = new Collection<XmltvPerson>();
                                Producers.Add(XmltvPerson.GetInstance(xmlReader));
                                break;
                            case "presenter":
                                if (Presenters == null)
                                    Presenters = new Collection<XmltvPerson>();
                                Presenters.Add(XmltvPerson.GetInstance(xmlReader));
                                break;
                            case "commentator":
                                if (Commentators == null)
                                    Commentators = new Collection<XmltvPerson>();
                                Commentators.Add(XmltvPerson.GetInstance(xmlReader));
                                break;
                            case "guest":
                                if (Guests == null)
                                    Guests = new Collection<XmltvPerson>();
                                Guests.Add(XmltvPerson.GetInstance(xmlReader));
                                break;
                            case "date":
                                Date = xmlReader.ReadString();
                                break;
                            case "category":
                                if (Categories == null)
                                    Categories = new Collection<XmltvText>();
                                Categories.Add(XmltvText.GetInstance(xmlReader));
                                break;
                            case "language":
                                Language = XmltvText.GetInstance(xmlReader);
                                break;
                            case "orig-language":
                                OriginalLanguage = XmltvText.GetInstance(xmlReader);
                                break;
                            case "length":
                                Length = XmltvLength.GetInstance(xmlReader);
                                break;
                            case "icon":
                                Icon = XmltvIcon.GetInstance(xmlReader);
                                break;
                            case "country":
                                Country = XmltvText.GetInstance(xmlReader);
                                break;
                            case "episode-num":
                                if (EpisodeNumbers == null)
                                    EpisodeNumbers = new Collection<XmltvEpisodeNumber>();
                                EpisodeNumbers.Add(XmltvEpisodeNumber.GetInstance(xmlReader));
                                break;
                            case "video":
                                Video = XmltvVideo.GetInstance(xmlReader.ReadSubtree());
                                break;
                            case "audio":
                                Audio = XmltvAudio.GetInstance(xmlReader.ReadSubtree());
                                break;
                            case "previously-shown":
                                PreviouslyShown = XmltvPreviouslyShown.GetInstance(xmlReader);
                                break;
                            case "premiere":
                                Premiere = XmltvText.GetInstance(xmlReader);
                                break;
                            case "last-chance":
                                LastChance = XmltvText.GetInstance(xmlReader);
                                break;
                            case "new":
                                New = true;
                                break;
                            case "subtitles":
                                if (Subtitlings == null)
                                    Subtitlings = new Collection<XmltvSubtitling>();
                                Subtitlings.Add(XmltvSubtitling.GetInstance(xmlReader.ReadSubtree()));
                                break;
                            case "rating":
                                Rating = XmltvRating.GetInstance(xmlReader.ReadSubtree());
                                break;
                            case "star-rating":
                                StarRating = XmltvStarRating.GetInstance(xmlReader.ReadSubtree());
                                break;
                            case "review":
                                Review = XmltvReview.GetInstance(xmlReader);
                                break;
                            default:
                                break;
                        }                        
                    }

                    xmlReader.Read();
                }
            }
            catch (XmlException e)
            {
                Logger.Instance.Write("Failed to load xmltv channel");
                Logger.Instance.Write("Data exception: " + e.Message);
                return (false);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("Failed to load xmltv channel");
                Logger.Instance.Write("I/O exception: " + e.Message);
                return (false);
            }

            return (true);
        }

        /// <summary>
        /// Locate an episode number with a specific system type.
        /// </summary>
        /// <param name="episodeType">The system type to search for.</param>
        /// <returns>The episode number if found; null otherwise.</returns>
        public XmltvEpisodeNumber FindEpisodeType(string episodeType)
        {
            if (EpisodeNumbers == null)
                return (null);

            foreach (XmltvEpisodeNumber episodeNumber in EpisodeNumbers)
            {
                if (episodeNumber.System == episodeType)
                    return (episodeNumber);
            }

            return (null);
        }

        /// <summary>
        /// Get a loaded instance of the class.
        /// </summary>
        /// <param name="xmlReader">An XmlReader instance for the tag.</param>
        /// <returns>An instance of the class with the tag data loaded.</returns>
        public static XmltvProgramme GetInstance(XmlReader xmlReader)
        {
            XmltvProgramme instance = new XmltvProgramme();
            instance.load(xmlReader);

            return (instance);
        }
    }
}
