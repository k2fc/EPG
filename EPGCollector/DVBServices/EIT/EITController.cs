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
using System.IO;
using System.Text;
using System.Diagnostics;

using DirectShow;
using DomainObjects;

namespace DVBServices
{
    /// <summary>
    /// The class that controls the acquisition and processing of EIT data.
    /// </summary>
    public class EITController : ControllerBase
    {
        /// <summary>
        /// Get the collection type supported by this collector.
        /// </summary>
        public override CollectionType CollectionType { get { return (CollectionType.EIT); } }
        /// <summary>
        /// Return true if the EIT data is complete; false otherwise.
        /// </summary>
        public override bool AllDataProcessed { get { return (eitSectionsDone);  } }

        private TSStreamReader eitReader;
        private bool eitSectionsDone = false;
        private int eitChannels;
        private int openTVChannels;

        private bool unzipExited;

        /// <summary>
        /// Initialize a new instance of the EITController class.
        /// </summary>
        public EITController() { }

        /// <summary>
        /// Stop acquiring and processing EIT data.
        /// </summary>
        public override void Stop()
        {
            Logger.Instance.Write("Stopping section readers");

            if (NetworkReader != null)
                NetworkReader.Stop();

            if (BouquetReader != null)
                BouquetReader.Stop();

            if (eitReader != null)
                eitReader.Stop();

            Logger.Instance.Write("Stopped section readers");
        }

        /// <summary>
        /// Acquire and process EIT data.
        /// </summary>
        /// <param name="dataProvider">A sample data provider.</param>
        /// <param name="worker">The background worker that is running this collection.</param>
        /// <returns>A CollectorReply code.</returns>
        public override CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            return (Process(dataProvider, worker, CollectionSpan.AllData));
        }

        /// <summary>
        /// Acquire and process EIT data.
        /// </summary>
        /// <param name="dataProvider">A sample data provider.</param>
        /// <param name="worker">The background worker that is running this collection.</param>
        /// <param name="collectionSpan">The amount of data to be collected.</param>
        /// <returns>A CollectorReply code.</returns>
        public override CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker, CollectionSpan collectionSpan)
        {
            if (collectionSpan == CollectionSpan.AllData)
            {
                EITProgramContent.Load();
                CustomProgramCategory.Load();
                ParentalRating.Load();

                if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options,
                    OptionName.UseFreeSatTables))
                    MultiTreeDictionaryEntry.Load(Path.Combine(RunParameters.ConfigDirectory, "Huffman Dictionary Freesat T1.cfg"), Path.Combine(RunParameters.ConfigDirectory, "Huffman Dictionary Freesat T2.cfg"));

                if (!string.IsNullOrWhiteSpace(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ByteConvertTable))
                    ByteConvertFile.Load();

                if (!string.IsNullOrWhiteSpace(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.EITCarousel))
                    EITCarouselFile.Load();
            }

            if (RunParameters.Instance.NetworkDataNeeded)
            {
                GetNetworkInformation(dataProvider, worker);
                if (worker.CancellationPending)
                    return (CollectorReply.Cancelled);
            }

            GetStationData(dataProvider, worker);
            if (worker.CancellationPending)
                return (CollectorReply.Cancelled);

            if (RunParameters.Instance.StationCollection.Count == 0 && 
                !OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options, OptionName.CreateMissingChannels))
            {
                Logger.Instance.Write("<e> No stations located - data collection abandoned");
                return (CollectorReply.OK);
            }

            if (RunParameters.Instance.ChannelDataNeeded)
            {
                GetBouquetSections(dataProvider, worker);

                string bouquetType;

                if (eitChannels > 0)
                    bouquetType = "Freeview";
                else
                    bouquetType = "OpenTV";

                Logger.Instance.Write("Used " + bouquetType + " channel descriptors");

                if (worker.CancellationPending)
                    return (CollectorReply.Cancelled);
            }

            if (collectionSpan == CollectionSpan.ChannelsOnly)
                return (CollectorReply.OK);

            if (string.IsNullOrWhiteSpace(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.EITCarousel))
                getEITSections(dataProvider, worker);
            else
                getCarouselSections(dataProvider, worker);

            return (CollectorReply.OK);
        }

        /// <summary>
        /// Process the bouquet data.
        /// </summary>
        protected override void ProcessBouquetSections()
        {
            foreach (BouquetAssociationSection bouquetSection in BouquetAssociationSection.BouquetAssociationSections)
            {
                if (bouquetSection.TransportStreams != null)
                {
                    foreach (TransportStream transportStream in bouquetSection.TransportStreams)
                    {
                        if (transportStream.Descriptors != null)
                        {
                            foreach (DescriptorBase descriptor in transportStream.Descriptors)
                            {
                                FreeviewChannelInfoDescriptor freeviewInfoDescriptor = descriptor as FreeviewChannelInfoDescriptor;
                                if (freeviewInfoDescriptor != null)
                                    processFreeviewInfoDescriptor(freeviewInfoDescriptor, transportStream.OriginalNetworkID, transportStream.TransportStreamID, bouquetSection.BouquetID);
                                else
                                {
                                    OpenTVChannelInfoDescriptor openTVInfoDescriptor = descriptor as OpenTVChannelInfoDescriptor;
                                    if (openTVInfoDescriptor != null)
                                        processOpenTVInfoDescriptor(openTVInfoDescriptor, transportStream.OriginalNetworkID, transportStream.TransportStreamID, bouquetSection.BouquetID);                                                                                
                                }
                            }
                        }
                    }
                }
            }
        }

        private void processFreeviewInfoDescriptor(FreeviewChannelInfoDescriptor freeviewInfoDescriptor, int originalNetworkID, int transportStreamID, int bouquetID)
        {
            if (freeviewInfoDescriptor.ChannelInfoEntries == null)
                return;

            if (openTVChannels != 0)
                return;

            foreach (FreeviewChannelInfoEntry channelInfoEntry in freeviewInfoDescriptor.ChannelInfoEntries)
            {
                EITChannel channel = new EITChannel();
                channel.OriginalNetworkID = originalNetworkID;
                channel.TransportStreamID = transportStreamID;
                channel.ServiceID = channelInfoEntry.ServiceID;
                channel.UserChannel = channelInfoEntry.UserNumber;
                channel.Flags = channelInfoEntry.Flags;
                channel.BouquetID = bouquetID;
                EITChannel.AddChannel(channel);
                
                eitChannels++;

                Bouquet bouquet = Bouquet.FindBouquet(channel.BouquetID);
                if (bouquet == null)
                {
                    bouquet = new Bouquet(channel.BouquetID, BouquetAssociationSection.FindBouquetName(channel.BouquetID));
                    Bouquet.AddBouquet(bouquet);
                }
                
                Region region = bouquet.FindRegion(channel.Region);
                if (region == null)
                {
                    region = new Region(string.Empty, channel.Region);
                    bouquet.AddRegion(region);
                }

                region.AddChannel(channel);
            }
        }

        private void processOpenTVInfoDescriptor(OpenTVChannelInfoDescriptor openTVInfoDescriptor, int originalNetworkID, int transportStreamID, int bouquetID)
        {
            if (openTVInfoDescriptor.ChannelInfoEntries == null)
                return;

            if (eitChannels != 0)
            {
                OpenTVChannel.Channels.Clear();
                eitChannels = 0;
                return;
            }

            foreach (OpenTVChannelInfoEntry channelInfoEntry in openTVInfoDescriptor.ChannelInfoEntries)
            {
                OpenTVChannel channel = new OpenTVChannel();
                channel.OriginalNetworkID = originalNetworkID;
                channel.TransportStreamID = transportStreamID;
                channel.ServiceID = channelInfoEntry.ServiceID;
                channel.ChannelID = channelInfoEntry.ChannelID;
                channel.UserChannel = channelInfoEntry.UserNumber;
                channel.Type = channelInfoEntry.Type;
                channel.Flags = channelInfoEntry.Flags;
                channel.BouquetID = bouquetID;
                channel.Region = openTVInfoDescriptor.Region;
                OpenTVChannel.AddChannel(channel);

                openTVChannels++;

                Bouquet bouquet = Bouquet.FindBouquet(channel.BouquetID);
                if (bouquet == null)
                {
                    bouquet = new Bouquet(channel.BouquetID, BouquetAssociationSection.FindBouquetName(channel.BouquetID));
                    Bouquet.AddBouquet(bouquet);
                }

                Region region = bouquet.FindRegion(channel.Region);
                if (region == null)
                {
                    region = new Region(string.Empty, channel.Region);
                    bouquet.AddRegion(region);
                }

                region.AddChannel(channel); 
            }
        }

        private void getEITSections(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            Logger.Instance.Write("Collecting EIT data", false, true);

            int actualPid;
            if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.EITPid == -1)
                actualPid = BDAGraph.EitPid;
            else
                actualPid = RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.EITPid;

            dataProvider.ChangePidMapping(new int[] { actualPid });            

            eitReader = new TSStreamReader(2000, dataProvider.BufferAddress); 
            eitReader.Run();

            int lastCount = 0;
            int repeats = 0;
            int bufferFill = 1;
            int totalBufferUsed = 0;

            while (!eitSectionsDone)
            {
                if (worker.CancellationPending)
                {
                    Logger.Instance.Write("", true, false);
                    Logger.Instance.Write("Stopping reader");
                    eitReader.Stop();

                    Logger.Instance.Write("EPG count: " + TVStation.TotalEpgCount +
                        " buffer space used: " + totalBufferUsed +
                        " discontinuities: " + eitReader.Discontinuities);

                    return;
                }

                Thread.Sleep(2000);
                Logger.Instance.Write(".", false, false);

                Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();

                eitReader.Lock("LoadMessages");
                if (eitReader.Sections.Count != 0)
                {                    
                    foreach (Mpeg2Section section in eitReader.Sections)
                        sections.Add(section);
                    eitReader.Sections.Clear();
                }
                eitReader.Release("LoadMessages");

                if (sections.Count != 0)
                    processSections(sections);

                if (lastCount == TVStation.TotalEpgCount)
                {
                    repeats++;
                    eitSectionsDone = (repeats == RunParameters.Instance.Repeats);

                    if (eitSectionsDone)
                    {
                        totalBufferUsed += dataProvider.BufferSpaceUsed;

                        if (RunParameters.Instance.BufferFills != 1)
                        {
                            Logger.Instance.Write("", true, false);
                            Logger.Instance.Write("Buffer scan " + bufferFill + " of " + RunParameters.Instance.BufferFills +
                                " finished:" +
                                " buffer space used " + dataProvider.BufferSpaceUsed);

                            eitSectionsDone = (bufferFill == RunParameters.Instance.BufferFills);

                            if (!eitSectionsDone)
                            {
                                eitReader.Stop();

                                dataProvider.ChangePidMapping(actualPid);

                                eitReader = new TSStreamReader(2000, dataProvider.BufferAddress);
                                eitReader.Run();

                                repeats = 0;
                                bufferFill++;

                                Logger.Instance.Write("Buffer scan " + bufferFill + " of " + RunParameters.Instance.BufferFills + " starting");
                            }
                        }
                    }
                }
                else
                    repeats = 0;

                lastCount = TVStation.TotalEpgCount;
            }

            if (!TraceEntry.IsDefined(TraceName.Bda))
                Logger.Instance.Write("", true, false);
            Logger.Instance.Write("Stopping reader");
            eitReader.Stop();

            Logger.Instance.Write("EPG count: " + TVStation.TotalEpgCount + 
                " buffer space used: " + totalBufferUsed + 
                " discontinuities: " + eitReader.Discontinuities);            
        }

        private void getCarouselSections(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            EITCarousel carousel = EITCarouselFile.FindCarousel(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.EITCarousel);
            if (carousel == null)
            {
                Logger.Instance.Write("Carousel '" + RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.EITCarousel + "' not defined");
                return;
            }

            foreach (EITCarouselPidSpec pidSpec in carousel.PidSpecs)
                processCarouselPid(carousel, pidSpec, dataProvider, worker);            
        }

        private void processCarouselPid(EITCarousel carousel, EITCarouselPidSpec pidSpec, ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            dataProvider.Frequency.DSMCCPid = pidSpec.Pid;
            DSMCCController carouselController = new DSMCCController();
            carouselController.GetDSMCCSections(dataProvider, worker);

            if (worker.CancellationPending)
                return;

            Collection<Tuple<string, byte[]>> files = carouselController.ProcessEITCarousel(pidSpec.CarouselDirectories, carousel.Suffix);
            if (files == null)
            {
                Logger.Instance.Write("No EIT files found in carousel");
                return;
            }

            byte[] eitData = getFileData(files, carousel.Format, pidSpec.ZipDirectories, carousel.ZipExePath);
            if (eitData == null)
            {
                Logger.Instance.Write("No EIT carousel data located");
                return;
            }

            Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();
            int dataIndex = 0;

            while (dataIndex < eitData.Length)
            {
                int sectionLength = ((eitData[dataIndex + 1] & 0x0f) * 256) + (int)eitData[dataIndex + 2];

                if (dataIndex + sectionLength + 3 <= eitData.Length)
                {
                    Mpeg2Section section = new Mpeg2Section();
                    section.PID = pidSpec.Pid;
                    section.Data = new byte[sectionLength + 3];
                    Array.Copy(eitData, dataIndex, section.Data, 0, sectionLength + 3);
                    section.Length = section.Data.Length;

                    sections.Add(section);
                }                

                dataIndex += sectionLength + 3;
            }

            processSections(sections);
        }

        private byte[] getFileData(Collection<Tuple<string, byte[]>> files, string zipFormat, Collection<string> zipDirectories, string zipExePath)
        {
            Collection<byte[]> fileBlocks = new Collection<byte[]>();
            
            string workingPath = Path.Combine(RunParameters.DataDirectory, "EPGC_Temporary") + Path.DirectorySeparatorChar;

            foreach (Tuple<string, byte[]> file in files)
            {
                try
                {
                    if (string.IsNullOrWhiteSpace(zipFormat))
                        fileBlocks.Add(file.Item2);
                    else
                    {
                        if (Directory.Exists(workingPath))
                            Directory.Delete(workingPath, true);
                        Directory.CreateDirectory(workingPath);

                        string outFile = workingPath + file.Item1;
                        FileStream outFileStream = new FileStream(outFile, FileMode.Create);

                        outFileStream.Write(file.Item2, 0, file.Item2.Length);
                        outFileStream.Close();

                        if (DebugEntry.IsDefined(DebugName.RetainZipData))
                            saveZipData(outFile);

                        string zipReply = unzipFile(zipFormat, workingPath, outFile, zipExePath);
                        if (zipReply != null)
                        {
                            Logger.Instance.Write("Failed to unzip file: " + zipReply);
                            return null;
                        }

                        string unzipPath = workingPath;

                        if (zipDirectories != null)
                        {
                            foreach (string zipDirectory in zipDirectories)
                                unzipPath = Path.Combine(unzipPath, zipDirectory);
                        }

                        DirectoryInfo directoryInfo = new DirectoryInfo(unzipPath);

                        foreach (FileInfo fileInfo in directoryInfo.GetFiles())
                        {
                            if (fileInfo.FullName != outFile)
                            {
                                if (DebugEntry.IsDefined(DebugName.RetainZipData))
                                    saveZipData(fileInfo.FullName);

                                if (DebugEntry.IsDefined(DebugName.EitZipContents))
                                    Logger.Instance.Write("Processing unzipped EIT file " + fileInfo.Name);

                                FileStream unzippedFile = new FileStream(fileInfo.FullName, FileMode.Open);
                                byte[] unzippedBytes = new byte[fileInfo.Length];
                                unzippedFile.Read(unzippedBytes, 0, unzippedBytes.Length);
                                unzippedFile.Close();
                                fileBlocks.Add(unzippedBytes);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Instance.Write("Failed to process zipped EIT file: An exception of type " + e.GetType().Name + " has occurred");
                    Logger.Instance.Write("Failed to process zipped EIT file: " + e.Message);
                    return null;
                }
            }

            if (Directory.Exists(workingPath))
                Directory.Delete(workingPath, true);

            return combineFileBlocks(fileBlocks);
        }

        private string unzipFile(string zipFormat, string directory, string fileName, string zipExePath)
        {
            switch (zipFormat.ToLowerInvariant())
            {
                case "default":
                    string zipReply = run7Zip(directory, fileName, zipExePath);
                    if (zipReply != null)
                    {
                        Logger.Instance.Write("Unzipping with 7-Zip not available: " + zipReply);
                        zipReply = runWinZip(directory, fileName, zipExePath);
                        if (zipReply != null)
                        {
                            Logger.Instance.Write("Unzipping with WinZip not available: " + zipReply);
                            zipReply = runDotNet(directory, fileName);
                            if (zipReply != null)
                            {
                                Logger.Instance.Write("Unzipping with .Net API not available: " + zipReply);
                                return "No method of unzipping files available";
                            }
                        }

                    }
                    return null;
                case "7zip":
                    return run7Zip(directory, fileName, zipExePath);
                case "winzip":
                    return runWinZip(directory, fileName, zipExePath);
                case "dotnet":
                case ".net":
                    return runDotNet(directory, fileName);
                default:
                    return "Zip format not recognized: " + zipFormat;
            }
        }

        private void saveZipData(string filePath)
        {
            try
            {
                string savePath = Path.Combine(RunParameters.DataDirectory, "EPGC_Zip_Output");
                if (!Directory.Exists(savePath))
                    Directory.CreateDirectory(savePath);

                FileInfo sourceInfo = new FileInfo(filePath);

                File.Copy(filePath, Path.Combine(savePath, sourceInfo.Name), true);
            }
            catch (Exception e)
            {
                Logger.Instance.Write("Failed to save zip output: exception type is " + e.GetType().Name);
                Logger.Instance.Write(e.Message);
            }
        }

        private string run7Zip(string directory, string fileName, string zipExePath)
        {
            Logger.Instance.Write("Running 7-Zip to unzip " + fileName);

            Process unzipProcess = new Process();

            string exePath = getZipDirectory(zipExePath, "7-Zip");
            if (exePath == null)
                return "Failed to locate 7-Zip executable";

            Logger.Instance.Write("Running 7-Zip from " + exePath);

            unzipProcess.StartInfo.FileName = exePath + "\\7z.exe";
            unzipProcess.StartInfo.WorkingDirectory = exePath;
            unzipProcess.StartInfo.Arguments = "e -y -spf -o" + "\"" + directory + "\"" + " \"" + fileName + "\"";
            unzipProcess.StartInfo.UseShellExecute = false;
            unzipProcess.StartInfo.CreateNoWindow = true;
            unzipProcess.StartInfo.RedirectStandardOutput = true;
            unzipProcess.StartInfo.RedirectStandardError = true;
            unzipProcess.EnableRaisingEvents = true;
            unzipProcess.Exited += new EventHandler(unzipProcessExited);

            Logger.Instance.Write("7-Zip command line: " + unzipProcess.StartInfo.Arguments);

            unzipExited = false;

            try
            {
                unzipProcess.Start();

                while (!unzipExited)
                    Thread.Sleep(100);

                Logger.Instance.Write("7-Zip has completed: exit code " + unzipProcess.ExitCode);
                if (unzipProcess.ExitCode == 0)
                    return (null);
                else
                    return ("Failed to unzip data: reply code " + unzipProcess.ExitCode);
            }
            catch (Exception e)
            {
                Logger.Instance.Write("<e> Failed to run 7-Zip");
                Logger.Instance.Write("<e> " + e.Message);
                return ("Failed to run 7-Zip due to an exception");
            }
        }

        private string runWinZip(string directory, string fileName, string zipExePath)
        {
            Logger.Instance.Write("Running WinZip to unzip " + fileName);

            Process unzipProcess = new Process();

            string exePath = getZipDirectory(zipExePath, "WinZip");
            if (exePath == null)
                return "Failed to locate WinZip executable";

            Logger.Instance.Write("Running WinZip from " + exePath);

            unzipProcess.StartInfo.FileName = exePath + "\\wzunzip.exe";
            unzipProcess.StartInfo.WorkingDirectory = exePath;
            unzipProcess.StartInfo.Arguments = "-d -o -ybc " + "\"" + fileName + "\"" + " \"" + directory + "\"";
            unzipProcess.StartInfo.UseShellExecute = false;
            unzipProcess.StartInfo.CreateNoWindow = true;
            unzipProcess.StartInfo.RedirectStandardOutput = true;
            unzipProcess.StartInfo.RedirectStandardError = true;
            unzipProcess.EnableRaisingEvents = true;
            unzipProcess.Exited += new EventHandler(unzipProcessExited);

            Logger.Instance.Write("WinZip command line: " + unzipProcess.StartInfo.Arguments);

            unzipExited = false;

            try
            {
                unzipProcess.Start();

                while (!unzipExited)
                    Thread.Sleep(100);

                Logger.Instance.Write("WinZip has completed: exit code " + unzipProcess.ExitCode);
                if (unzipProcess.ExitCode == 0)
                    return (null);
                else
                    return ("Failed to unzip data: reply code " + unzipProcess.ExitCode);
            }
            catch (Exception e)
            {
                Logger.Instance.Write("<e> Failed to run WinZip");
                Logger.Instance.Write("<e> " + e.Message);
                return ("Failed to run WinZip due to an exception");
            }
        }

        private string getZipDirectory(string userSpecifiedPath, string zipName)
        {
            if (!string.IsNullOrWhiteSpace(userSpecifiedPath))
                return userSpecifiedPath;

            string path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), zipName);
            Logger.Instance.Write("Looking for " + zipName + " installation directory at " + path);

            if (Directory.Exists(path))
                return path;
            else
                return null;
        }

        private void unzipProcessExited(object sender, EventArgs e)
        {
            unzipExited = true;
        }

        private string runDotNet(string directory, string fileName)
        {
            Logger.Instance.Write("Using .Net API to unzip " + fileName);

            return "Not implemented";
        }

        private byte[] combineFileBlocks(Collection<byte[]> fileBlocks)
        {
            int totalLength = 0;

            foreach (byte[] fileBlock in fileBlocks)
                totalLength += fileBlock.Length;

            byte[] combinedData = new byte[totalLength];
            int currentLength = 0;

            foreach (byte[] fileBlock in fileBlocks)
            {
                Array.Copy(fileBlock, 0, combinedData, currentLength, fileBlock.Length);
                currentLength += fileBlock.Length;
            }

            return combinedData;
        }

        private void processSections(Collection<Mpeg2Section> sections)
        {
            foreach (Mpeg2Section section in sections)
            {
                if (DebugEntry.IsDefined(DebugName.DumpEitSections))
                    Logger.Instance.Dump("EIT Section", section.Data, section.Data.Length);

                if (section.Table >= 0x4e && section.Table <= 0x6f)
                {
                    try
                    {
                        Mpeg2ExtendedHeader mpeg2Header = new Mpeg2ExtendedHeader();
                        mpeg2Header.Process(section.Data);
                        if (mpeg2Header.Current)
                        {
                            EITSection eitSection = new EITSection();
                            eitSection.Process(section.Data, mpeg2Header);
                            eitSection.LogMessage();
                        }
                    }
                    catch (ArgumentOutOfRangeException e)
                    {
                        Logger.Instance.Write("<e> EIT error: " + e.Message);
                    }
                }
            }
        }

        /*private void getAITSections(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            Logger.Instance.Write("Collecting AIT data", false, true);

            int aitPid = 0;

            foreach (TVStation station in RunParameters.Instance.StationCollection)
            {
                if (station.AitPid != 0)
                    aitPid = station.AitPid; 
            }

            if (aitPid == 0)
            {
                Logger.Instance.Write("", true, false);
                Logger.Instance.Write("No AIT pid available");
                return;
            }

            Logger.Instance.Write("Collecting AIT data from PID 0x" + aitPid.ToString("X"), false, true);

            dataProvider.ChangePidMapping(new int[] { aitPid });

            aitReader = new TSStreamReader(0X74, 2000, dataProvider.BufferAddress);
            aitReader.Run();

            int repeats = 0;

            while (!aitSectionsDone)
            {
                if (worker.CancellationPending)
                    return;

                Thread.Sleep(2000);
                Logger.Instance.Write(".", false, false);

                Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();

                aitReader.Lock("LoadMessages");
                if (aitReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in aitReader.Sections)
                        sections.Add(section);
                    aitReader.Sections.Clear();
                }
                aitReader.Release("LoadMessages");

                if (sections.Count != 0)
                    processAITSections(sections);

                repeats++;
                aitSectionsDone = repeats > 10;
            }

            Logger.Instance.Write("", true, false);
            Logger.Instance.Write("Stopping reader");
            aitReader.Stop();
        }*/

        /// <summary>
        /// Create the EPG entries.
        /// </summary>
        public override void FinishFrequency() 
        {
            if (!OptionEntry.IsDefined(OptionName.TcRelevantOnly))
            {
                if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ChannelBouquet != -1)
                {
                    foreach (TVStation tvStation in RunParameters.Instance.StationCollection)
                    {
                        bool process = checkChannelMapping(tvStation);
                        if (!process)
                            tvStation.EPGCollection.Clear();
                    }
                }
                else
                {
                    if (Bouquet.Bouquets.Count != 0)
                    {
                        foreach (Bouquet bouquet in Bouquet.Bouquets)
                        {
                            foreach (Region region in bouquet.Regions)
                            {
                                foreach (Channel channel in region.Channels)
                                {
                                    TVStation station = TVStation.FindStation(RunParameters.Instance.StationCollection,
                                        channel.OriginalNetworkID, channel.TransportStreamID, channel.ServiceID);
                                    if (station != null && station.LogicalChannelNumber == -1)
                                        station.LogicalChannelNumber = channel.UserChannel;
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (TVStation tvStation in RunParameters.Instance.StationCollection)
                            checkNetworkMap(tvStation);
                    }
                }
            }
            else
            {
                foreach (TVStation tvStation in RunParameters.Instance.StationCollection)
                {
                    bool process = checkNetworkMap(tvStation);
                    if (!process)
                        tvStation.EPGCollection.Clear();
                }

            }

            if (Utils.FormattingBytes != null)
            {
                StringBuilder formatString = new StringBuilder();

                foreach (byte formatByte in Utils.FormattingBytes)
                {
                    if (formatString.Length == 0)
                        formatString.Append("Control codes used in text: ");
                    else
                        formatString.Append(", ");

                    string hexFormat = formatByte.ToString("X");
                    
                    if (hexFormat.Length != 1)
                        formatString.Append("0x" + hexFormat);
                    else
                        formatString.Append("0x0" + hexFormat);
                }

                Logger.Instance.Write(formatString.ToString());
            }

            EITProgramContent.LogContentUsage();
            LanguageCode.LogUsage();
            CharacterSet.LogUsage();
            MultiTreeDictionaryEntry.LogUsage();
            logChannelInfo();
        }

        private bool checkChannelMapping(TVStation tvStation)
        {
            Bouquet bouquet = Bouquet.FindBouquet(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ChannelBouquet);
            if (bouquet == null)
                return (false);

            Region region = bouquet.FindRegion(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ChannelRegion != -1 ? RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ChannelRegion : 0);
            if (region == null)
                return (false);

            Channel channel = region.FindChannel(tvStation.OriginalNetworkID, tvStation.TransportStreamID, tvStation.ServiceID);
            if (channel == null)
                return (false);

            if (tvStation.LogicalChannelNumber != -1)
                return (true);

            tvStation.LogicalChannelNumber = channel.UserChannel;

            return (true);
        }

        private bool checkNetworkMap(TVStation tvStation)
        {
            TuningFrequency tuningFrequency = NetworkMap.FindFrequency(tvStation.OriginalNetworkID, tvStation.TransportStreamID);
            if (tuningFrequency == null)
                return (false);

            TerrestrialFrequency terrestrialFrequency = tuningFrequency as TerrestrialFrequency;
            if (terrestrialFrequency == null)
            {
                CableFrequency cableFrequency = tuningFrequency as CableFrequency;
                if (cableFrequency == null)
                    return (false);
            }

            if (tvStation.LogicalChannelNumber != -1)
                return (true);

            Channel channel = Channel.FindChannel(tvStation.OriginalNetworkID, tvStation.TransportStreamID, tvStation.ServiceID);
            if (channel != null)
                tvStation.LogicalChannelNumber = channel.UserChannel;

            return (true);
        }

        private void logChannelInfo()
        {
            if (DebugEntry.IsDefined(DebugName.LogIncompleteEit))
                TVStation.LogIncompleteEITMapEntries(RunParameters.Instance.StationCollection);

            if (!DebugEntry.IsDefined(DebugName.LogChannels))
                return;
            if (!RunParameters.Instance.ChannelDataNeeded)
                return;

            Logger.Instance.WriteSeparator("Bouquet Usage");

            if (Bouquet.Bouquets != null && Bouquet.Bouquets.Count != 0)
            {
                bool firstBouquet = true;

                foreach (Bouquet bouquet in Bouquet.GetBouquetsInNameOrder())
                {
                    if (!firstBouquet)
                        Logger.Instance.Write("");

                    firstBouquet = false;

                    if (!firstBouquet)
                        Logger.Instance.Write("");

                    firstBouquet = false;

                    foreach (Region region in bouquet.Regions)
                    {
                        Logger.Instance.Write("Bouquet: " + bouquet.BouquetID + " - " + bouquet.Name + " Region: " + region.Code + " (channels = " + region.Channels.Count + ")");

                        foreach (Channel channel in region.GetChannelsInNameOrder())
                            Logger.Instance.Write("    Channel: " + channel.ToString());
                    }
                }
            }
            else
            {
                Logger.Instance.Write("No bouquets available - channel data only");

                foreach (Channel channel in Channel.GetChannelsInUserNumberOrder())
                    Logger.Instance.Write("Channel: " + channel.ToString());
            }
        }
    }
}
