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
using System.Threading;
using System.ComponentModel;
using System.Text;

using DirectShow;
using DomainObjects;

namespace DVBServices
{
    /// <summary>
    /// The class that controls the acquisition and processing of SiehFern Info data.
    /// </summary>
    public class SiehFernInfoController : ControllerBase
    {
        /// <summary>
        /// Get the collection type supported by this collector.
        /// </summary>
        public override CollectionType CollectionType { get { return (CollectionType.SiehfernInfo); } }
        /// <summary>
        /// Return true if the EIT data is complete; false otherwise.
        /// </summary>
        public override bool AllDataProcessed { get { return (guideDone); } }

        private TSStreamReader guideReader;

        private bool guideDone = false;

        private TVStation currentStation;
        private DateTime startDate;
        private EPGEntry epgEntry;
        private DateTime lastStartTime;

        /// <summary>
        /// Initialize a new instance of the SiehFernInfoController class.
        /// </summary>
        public SiehFernInfoController() { }

        /// <summary>
        /// Stop acquiring and processing EIT data.
        /// </summary>
        public override void Stop()
        {
            Logger.Instance.Write("Stopping section readers");

            if (guideReader != null)
                guideReader.Stop();

            Logger.Instance.Write("Stopped section readers");
        }

        /// <summary>
        /// Acquire and process Siehfern Info data.
        /// </summary>
        /// <param name="dataProvider">A sample data provider.</param>
        /// <param name="worker">The background worker that is running this collection.</param>
        /// <returns>A CollectorReply code.</returns>
        public override CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            return (Process(dataProvider, worker, CollectionSpan.AllData));
        }

        /// <summary>
        /// Acquire and process Siehfern Info data.
        /// </summary>
        /// <param name="dataProvider">A sample data provider.</param>
        /// <param name="worker">The background worker that is running this collection.</param>
        /// <param name="collectionSpan">The amount of data to collect.</param>
        /// <returns>A CollectorReply code.</returns>
        public override CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker, CollectionSpan collectionSpan)
        {
            /*getChannelSections(dataProvider, worker);*/
            getEPGSections(dataProvider, worker);

            return (CollectorReply.OK);
        }

        private void getChannelSections(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            Logger.Instance.Write("Collecting Channel data", false, true);

            dataProvider.ChangePidMapping(new int[] { 0x711 });

            guideReader = new TSStreamReader(0x3e, 50000, dataProvider.BufferAddress);
            guideReader.Run();

            int lastCount = 0;
            int repeats = 0;

            while (!guideDone)
            {
                if (worker.CancellationPending)
                    return;

                Thread.Sleep(2000);
                Logger.Instance.Write(".", false, false);

                Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();

                guideReader.Lock("LoadMessages");
                if (guideReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in guideReader.Sections)
                        sections.Add(section);

                    guideReader.Sections.Clear();
                }
                guideReader.Release("LoadMessages");

                if (sections.Count != 0)
                    processChannelSections(sections);

                if (SiehFernInfoEPGSection.Sections == null || SiehFernInfoChannelSection.Sections.Count == lastCount)
                {
                    repeats++;
                    guideDone = (repeats == RunParameters.Instance.Repeats);
                }
                else
                    repeats = 0;

                if (SiehFernInfoEPGSection.Sections != null)
                    lastCount = SiehFernInfoEPGSection.Sections.Count;
            }

            Logger.Instance.Write("", true, false);
            Logger.Instance.Write("Stopping reader");
            guideReader.Stop();

            Logger.Instance.Write("Section count: " + lastCount + 
                " buffer space used: " + dataProvider.BufferSpaceUsed +
                " discontinuities: " + guideReader.Discontinuities);
        }

        private void getEPGSections(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            Logger.Instance.Write("Collecting EPG data", false, true);

            dataProvider.ChangePidMapping(new int[] { 0x711 });

            guideReader = new TSStreamReader(0x3e, 50000, dataProvider.BufferAddress);
            guideReader.Run();

            int lastCount = 0;
            int repeats = 0;
            guideDone = false;

            while (!guideDone)
            {
                if (worker.CancellationPending)
                    return;

                Thread.Sleep(2000);
                Logger.Instance.Write(".", false, false);

                Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();

                guideReader.Lock("LoadMessages");
                if (guideReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in guideReader.Sections)
                        sections.Add(section);
                    guideReader.Sections.Clear();
                }
                guideReader.Release("LoadMessages");

                if (sections.Count != 0)
                    processEPGSections(sections);

                if (SiehFernInfoEPGSection.Sections == null || SiehFernInfoEPGSection.Sections.Count == lastCount)
                {
                    repeats++;
                    guideDone = (repeats == RunParameters.Instance.Repeats);
                }
                else
                    repeats = 0;

                if (SiehFernInfoEPGSection.Sections != null)
                    lastCount = SiehFernInfoEPGSection.Sections.Count;
            }

            Logger.Instance.Write("", true, false);
            Logger.Instance.Write("Stopping reader");
            guideReader.Stop();

            Logger.Instance.Write("Section count: " + lastCount + 
                " buffer space used: " + dataProvider.BufferSpaceUsed +
                " discontinuities: " + guideReader.Discontinuities);
        }

        private void processChannelSections(Collection<Mpeg2Section> sections)
        {
            foreach (Mpeg2Section section in sections)
            {
                if (DebugEntry.IsDefined(DebugName.SiehfernBlocks))
                    Logger.Instance.Dump("Siehfern Block", section.Data, section.Data.Length);

                try
                {
                    Mpeg2ExtendedHeader mpeg2Header = new Mpeg2ExtendedHeader();                    
                    mpeg2Header.Process(section.Data);
                    if (mpeg2Header.Current)
                    {
                        if (mpeg2Header.TableIDExtension == 0x1502)
                        {
                            SiehFernInfoChannelSection channelSection = new SiehFernInfoChannelSection();
                            channelSection.Process(section.Data, mpeg2Header);
                            channelSection.LogMessage();

                            bool added = SiehFernInfoChannelSection.AddSection(channelSection);
                            if (added)
                            {
                                if (DebugEntry.IsDefined(DebugName.SiehfernChannelBlocks))
                                    Logger.Instance.Dump("Siehfern Info Block Type 0x" + mpeg2Header.TableIDExtension.ToString("x"), section.Data, section.Data.Length);
                            }
                        }                        
                    }
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Logger.Instance.Write("<e> Error processing SiehFern Info Channel section: " + e.Message);
                }
            }
        }

        private void processEPGSections(Collection<Mpeg2Section> sections)
        {
            foreach (Mpeg2Section section in sections)
            {
                if (DebugEntry.IsDefined(DebugName.SiehfernBlocks))
                    Logger.Instance.Dump("Siehfern Block", section.Data, section.Data.Length);

                try
                {
                    Mpeg2ExtendedHeader mpeg2Header = new Mpeg2ExtendedHeader();
                    mpeg2Header.Process(section.Data);
                    if (mpeg2Header.Current)
                    {
                        if (mpeg2Header.TableIDExtension >= 0x100 && mpeg2Header.TableIDExtension <= 0x900)
                        {
                            if (DebugEntry.IsDefined(DebugName.SiehfernEpgBlocks))
                                Logger.Instance.Dump("Siehfern Info Block Type 0x" + mpeg2Header.TableIDExtension.ToString("x"), section.Data, section.Data.Length);

                            SiehFernInfoEPGSection epgSection = new SiehFernInfoEPGSection();
                            epgSection.Process(section.Data, mpeg2Header);
                            epgSection.LogMessage();

                            SiehFernInfoEPGSection.AddSection(epgSection);
                        }
                    }
                }
                catch (ArgumentOutOfRangeException e)
                {
                    Logger.Instance.Write("<e> Error processing SiehFern Info EPG section: " + e.Message);
                }
            }
        }

        /// <summary>
        /// Create the EPG entries.
        /// </summary>
        public override void FinishFrequency()
        {
            if (SiehFernInfoEPGSection.Sections == null)
                return;

            Logger titleLogger = null;
            Logger descriptionLogger = null;

            if (DebugEntry.IsDefined(DebugName.LogTitles))
                titleLogger = new Logger("EPG Titles.log");
            if (DebugEntry.IsDefined(DebugName.LogDescriptions))
                descriptionLogger = new Logger("EPG Descriptions.log");

            int extensionId = 0x102;
            int dayNumber = 0;

            while (extensionId < 0x802)
            {
                int byteArraySize = 0;

                foreach (SiehFernInfoEPGSection epgSection in SiehFernInfoEPGSection.Sections)
                {
                    if (epgSection.TableIdExtension == extensionId)
                        byteArraySize += epgSection.Data.Length;
                }

                if (byteArraySize != 0)
                {
                    byte[] epgBuffer = new byte[byteArraySize];

                    int putIndex = 0;

                    foreach (SiehFernInfoEPGSection epgSection in SiehFernInfoEPGSection.Sections)
                    {
                        if (epgSection.TableIdExtension == extensionId)
                        {
                            epgSection.Data.CopyTo(epgBuffer, putIndex);
                            putIndex += epgSection.Data.Length;
                        }
                    }

                    /*int leaderP = countLeaders(epgBuffer, "@P");
                    int leaderE = countLeaders(epgBuffer, "@E");
                    int leaderS = countLeaders(epgBuffer, "@S");
                    int leaderQ = countLeaders(epgBuffer, "@Q");
                    int leaderL = countLeaders(epgBuffer, "@L");
                    int leaderH = countLeaders(epgBuffer, "@H");

                    Logger.Instance.Write("Ext: 0x" + extensionId.ToString("x") +
                    "P: " + leaderP + "E: " + leaderE + "S: " + leaderS + "Q: " + leaderQ + "Q: " + leaderQ + "L: " + leaderL);*/

                    int getIndex = 0;
                    string titleText = string.Empty;

                    while (epgBuffer[getIndex] != '@')
                        getIndex++;

                    while (getIndex < epgBuffer.Length)
                    {
                        byte[] epgLine = getLine(epgBuffer, getIndex);
                        getIndex += epgLine.Length;

                        for (int scan = 0; scan < epgLine.Length; scan++)
                        {
                            if (epgLine[scan] == 0x0d || epgLine[scan] == 0x0a)
                                epgLine[scan] = (byte)'|';
                            else
                            {
                                if (epgLine[scan] == 0x8a)
                                    epgLine[scan] = (byte)' ';
                            }
                        }

                        string epgText = Utils.GetString(epgLine, 0, epgLine.Length);

                        switch (epgText.Substring(0, 3))
                        {
                            case "@P:":
                                processStation(epgText, dayNumber);
                                break;
                            case "@E:":
                                if (epgEntry != null)
                                    logEntry(titleText, titleLogger, descriptionLogger);
                                titleText = epgText;
                                processProgramTitle(epgText);
                                break;
                            case "@S:":
                                processSeries(epgText);
                                break;
                            case "@L:":
                                processDescription(epgText, descriptionLogger);
                                break;
                            default:
                                break;
                        }
                    }

                    if (epgEntry != null)
                        logEntry(titleText, titleLogger, descriptionLogger);
                }

                extensionId += 0x100;
                dayNumber++;
            }
        }

        private int countLeaders(byte[] buffer, string leader)
        {
            string text = Encoding.ASCII.GetString(buffer);

            int index = 0;
            int count = 0;

            while (index != -1)
            {
                index = text.IndexOf(leader, index);
                if (index == -1)
                    return (count);

                count++;
                index++;
            }

            return (count);
        }

        private byte[] getLine(byte[] epgBuffer, int index)
        {
            int length = 0;

            for (int lengthIndex = index + 1; lengthIndex < epgBuffer.Length && epgBuffer[lengthIndex] != '@'; lengthIndex++)
                length++;            

            byte[] outputBytes = new byte[length + 1];

            outputBytes[0] = (byte)'@';
            int outputIndex = 1;

            for (int getIndex = index + 1; getIndex < epgBuffer.Length && epgBuffer[getIndex] != '@'; getIndex++)
            {
                outputBytes[outputIndex] = epgBuffer[getIndex];
                outputIndex++;
            }
            
            return (outputBytes);
        }

        private void processStation(string epgText, int dateOffset)
        {
            string[] parts = epgText.Substring(3).Split(new char[] { '(' } );
            string[] stationDefinition = parts[1].Split(new char[] { ')' } );
            string[] stationParts = stationDefinition[0].Split(new char[] { ',' } );

            currentStation = new TVStation(parts[0]);
            currentStation.Frequency = Int32.Parse(stationParts[1].Trim());
            currentStation.ServiceID = Int32.Parse(stationParts[2].Trim());

            bool addStation = true;
            
            foreach (TVStation existingStation in RunParameters.Instance.StationCollection)
            {
                if (existingStation.OriginalNetworkID == currentStation.OriginalNetworkID &&
                    existingStation.TransportStreamID == currentStation.TransportStreamID &&
                    existingStation.ServiceID == currentStation.ServiceID)
                {
                    currentStation = existingStation;
                    addStation = false;
                    break;
                }
            }

            if (addStation)
                RunParameters.Instance.StationCollection.Add(currentStation);

            string[] dateParts = stationDefinition[1].Split(new char[] { ' ' } );
            string[] dayMonthYear = dateParts[1].Trim().Split(new char[] { '.' } );
            int day = Int32.Parse(dayMonthYear[0]);
            int month = Int32.Parse(dayMonthYear[1]);
            int year = Int32.Parse(dayMonthYear[2]);

            startDate = new DateTime(year, month, day);
            startDate.Date.AddDays(dateOffset);
            lastStartTime = startDate;
        }

        private void processProgramTitle(string epgText)
        {
            if (currentStation == null)
                return;

            epgEntry = new EPGEntry();
            epgEntry.OriginalNetworkID = currentStation.OriginalNetworkID;
            epgEntry.TransportStreamID = currentStation.TransportStreamID;
            epgEntry.ServiceID = currentStation.ServiceID;
            epgEntry.EPGSource = EPGSource.SiehfernInfo;

            string time = epgText.Substring(3, 5);
            int hours = Int32.Parse(time.Substring(0, 2));
            int minutes = Int32.Parse(time.Substring(3, 2));

            TimeSpan startTime = new TimeSpan(hours, minutes, 0);
            if (startDate + startTime < lastStartTime)
                startTime = startTime.Add(new TimeSpan(24, 0, 0));

            epgEntry.StartTime = Utils.RoundTime(startDate + startTime);

            int separatorIndex = epgText.IndexOf('|');
            if (separatorIndex == -1)
                epgEntry.EventName = EditSpec.ProcessTitle(epgText.Substring(9));
            else
                epgEntry.EventName = EditSpec.ProcessTitle(epgText.Substring(9, separatorIndex - 9));

            bool include = ControllerBase.CheckEPGDays(epgEntry.StartTime);
            if (include)
            {
                currentStation.EPGCollection.Add(epgEntry);
                lastStartTime = epgEntry.StartTime;

                if (currentStation.EPGCollection.Count > 1)
                {
                    int count = currentStation.EPGCollection.Count;
                    if (currentStation.EPGCollection[count - 2].Duration.TotalSeconds == 0)
                        currentStation.EPGCollection[count - 2].Duration = Utils.RoundTime(currentStation.EPGCollection[count - 1].StartTime - currentStation.EPGCollection[count - 2].StartTime);
                }
            }
        }

        private void processSeries(string epgText)
        {
            if (epgEntry == null)
                return;

            getShortDescription(epgText);
            getDuration(epgText);
            getYear(epgText);
            getEpisode(epgText);
        }

        private void getShortDescription(string epgText)
        {
            int count = 0;

            foreach (char epgChar in epgText)
            {
                if (epgChar == ',')
                    count++;
            }

            if (count < 2)
                return;

            int startIndex = 3;

            string identifier = "Folge ";
            int prefixIndex = epgText.IndexOf(identifier);
            if (prefixIndex != -1)
            {
                prefixIndex = epgText.IndexOf(":", prefixIndex);
                if (prefixIndex != -1)
                    startIndex = prefixIndex + 2;
            }

            int endIndex = epgText.IndexOf(",");
            if (startIndex >= endIndex)
                return;

            string description = epgText.Substring(startIndex, endIndex - startIndex).Trim();
            description = description.Replace(@"\", "");
            description = description.Replace("||", "");
            epgEntry.ShortDescription = description;
        }

        private void getDuration(string epgText)
        {
            int timeIndex = epgText.LastIndexOf(',');
            if (timeIndex == -1)
                return;

            string[] timeParts = epgText.Substring(timeIndex + 1).Replace(@"\", "").Split(new char[] { ' ' } );
            if (timeParts.Length >= 2 && timeParts[1].Length > 0)
            {
                try
                {
                    int totalMinutes = Int32.Parse(timeParts[1].Trim());
                    int hours = totalMinutes / 60;
                    int minutes = totalMinutes % 60;
                    epgEntry.Duration = new TimeSpan(hours, minutes, 0);
                }
                catch (FormatException) { }
            }
        }

        private void getYear(string epgText)
        {
            int yearIndex = epgText.LastIndexOf(',');
            if (yearIndex < 4)
                return;

            if (!char.IsDigit(epgText[yearIndex - 1]) ||
                !char.IsDigit(epgText[yearIndex - 2]) ||
                !char.IsDigit(epgText[yearIndex - 3]) ||
                !char.IsDigit(epgText[yearIndex - 4]))
                return;

            epgEntry.Date = epgText.Substring(yearIndex - 4, 4);
        }

        private void getEpisode(string epgText)
        {
            string identifier = "Folge ";

            int episodeIndex = epgText.LastIndexOf(identifier);
            if (episodeIndex == -1)
                return;

            int episodeNumber = 0;

            while (char.IsDigit(epgText[episodeIndex + identifier.Length]))
            {
                episodeNumber = (episodeNumber * 10) + (epgText[episodeIndex + identifier.Length] - '0');
                episodeIndex++;
            }

            epgEntry.EpisodeNumber = episodeNumber;            
        }

        private void processDescription(string epgText, Logger descriptionLogger)
        {
            if (epgEntry == null)
                return;

            string description = epgText.Substring(3).Trim();
            description = description.Replace(@"\", "");
            description = description.Replace("||", "");
            epgEntry.ShortDescription = description;
        }

        private void logEntry(string titleText, Logger titleLogger, Logger descriptionLogger)
        {
            if (titleLogger != null)
            {
                titleLogger.Write(epgEntry.OriginalNetworkID + ":" + epgEntry.TransportStreamID + ":" + epgEntry.ServiceID + " " +
                    epgEntry.StartTime.ToShortDateString() + " " +
                    epgEntry.StartTime.ToString("HH:mm") + " - " +
                    epgEntry.StartTime.Add(epgEntry.Duration).ToString("HH:mm") + " " +
                    epgEntry.EventName + " " +
                    titleText);
            }

            if (descriptionLogger != null)
            {
                descriptionLogger.Write(epgEntry.OriginalNetworkID + ":" + epgEntry.TransportStreamID + ":" + epgEntry.ServiceID + " " +
                    epgEntry.StartTime.ToShortDateString() + " " +
                    epgEntry.StartTime.ToString("HH:mm") + " - " +
                    epgEntry.StartTime.Add(epgEntry.Duration).ToString("HH:mm") + " " +
                    epgEntry.ShortDescription);
            }
        }
    }
}
