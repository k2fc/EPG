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
using System.IO.Compression;
using System.Diagnostics;
using System.Xml;

using DirectShow;
using DomainObjects;

using zlib;

namespace DVBServices
{
    /// <summary>
    /// The class that controls the acquisition and processing of the DSMCC protocol.
    /// </summary>
    public class DSMCCController : ControllerBase
    {
        /// <summary>
        /// Get the collection type supported by this collector.
        /// </summary>
        public override CollectionType CollectionType { get { return (CollectionType.MHEG5); } }
        /// <summary>
        /// Return true if the DSMCC data is complete; false otherwise.
        /// </summary>
        public override bool AllDataProcessed { get { return (checkAllDataLoaded()); } }

        private TSStreamReader dsmccReader;

        private DSMCCDownloadServerInitiate dsiMessage;
        private Collection<DSMCCDownloadInfoIndication> diiMessages;
        private Collection<DSMCCModule> modules;

        private static Logger titleLogger;
        private static Logger descriptionLogger;

        private string prefix = "";

        private bool noMHEGPid;

        /// <summary>
        /// Initialize a new instance of the DSMCCController class.
        /// </summary>
        public DSMCCController()
        {
            if (diiMessages == null)
                diiMessages = new Collection<DSMCCDownloadInfoIndication>();
            if (modules == null)
                modules = new Collection<DSMCCModule>();
        }

        /// <summary>
        /// Stop acquiring and processing DSMCC data.
        /// </summary>
        public override void Stop()
        {
            Logger.Instance.Write("Stopping section readers");

            if (NetworkReader != null)
                NetworkReader.Stop();

            if (BouquetReader != null)
                BouquetReader.Stop();

            if (dsmccReader != null)
                dsmccReader.Stop();

            Logger.Instance.Write("Stopped section readers");
        }

        /// <summary>
        /// Acquire and process DSMCC data.
        /// </summary>
        /// <param name="dataProvider">A sample data provider.</param>
        /// <param name="worker">The background worker that is running this collection.</param>
        /// <returns>A CollectorReply code.</returns>
        public override CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            return (Process(dataProvider, worker, CollectionSpan.AllData));
        }

        /// <summary>
        /// Acquire and process DSMCC data.
        /// </summary>
        /// <param name="dataProvider">A sample data provider.</param>
        /// <param name="worker">The background worker that is running this collection.</param>
        /// <param name="collectionSpan">The amount of data to collect.</param>
        /// <returns>A CollectorReply code.</returns>
        public override CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker, CollectionSpan collectionSpan)
        {
            if (collectionSpan == CollectionSpan.AllData)
            {
                if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode == null)
                    MHEGParserParameters.Process("MHEG5 Parser Format NZL.cfg");
                else
                    MHEGParserParameters.Process("MHEG5 Parser Format " + RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode + ".cfg");

                CustomProgramCategory.Load();
                ParentalRating.Load();
                /*TextEdit.Load();*/
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

            if (RunParameters.Instance.StationCollection.Count == 0)
            {
                Logger.Instance.Write("<e> No stations located - data collection abandoned");
                return (CollectorReply.OK);
            }

            if (RunParameters.Instance.ChannelDataNeeded)
            {
                GetBouquetSections(dataProvider, worker);
                if (worker.CancellationPending)
                    return (CollectorReply.Cancelled);
            }

            if (collectionSpan == CollectionSpan.ChannelsOnly)
                return (CollectorReply.OK);

            GetDSMCCSections(dataProvider, worker);
            if (worker.CancellationPending)
                return (CollectorReply.Cancelled);

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
                                FreeviewChannelInfoDescriptor infoDescriptor = descriptor as FreeviewChannelInfoDescriptor;
                                if (infoDescriptor != null)
                                {
                                    if (infoDescriptor.ChannelInfoEntries != null)
                                    {
                                        foreach (FreeviewChannelInfoEntry channelInfoEntry in infoDescriptor.ChannelInfoEntries)
                                        {
                                            MHEG5Channel channel = new MHEG5Channel();
                                            channel.OriginalNetworkID = transportStream.OriginalNetworkID;
                                            channel.TransportStreamID = transportStream.TransportStreamID;
                                            channel.ServiceID = channelInfoEntry.ServiceID;
                                            channel.UserChannel = channelInfoEntry.UserNumber;
                                            channel.Flags = channelInfoEntry.Flags;
                                            channel.BouquetID = bouquetSection.BouquetID;
                                            MHEG5Channel.AddChannel(channel);

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
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Build the DSMCC tables.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="worker">The background worker.</param>
        public void GetDSMCCSections(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            int pid = dataProvider.Frequency.DSMCCPid;

            if (pid == 0)
            {
                noMHEGPid = true;
                Logger.Instance.Write("No MHEG5 PID's on frequency " + dataProvider.Frequency);
                return;
            }
            
            Logger.Instance.Write("Collecting MHEG5 data from PID 0x" + pid.ToString("X").ToLowerInvariant(), false, true);

            dataProvider.ChangePidMapping(new int[] { pid });
            
            dsmccReader = new TSStreamReader(500, dataProvider.BufferAddress);
            dsmccReader.Run();

            while (!checkAllDataLoaded())
            {
                if (worker.CancellationPending)
                    return;

                Thread.Sleep(2000);
                Logger.Instance.Write(".", false, false);

                Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();

                dsmccReader.Lock("LoadMessages");
                if (dsmccReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in dsmccReader.Sections)
                        sections.Add(section);
                    dsmccReader.Sections.Clear();
                }
                dsmccReader.Release("LoadMessages");

                foreach (Mpeg2Section section in sections)
                {
                    switch (section.Table)
                    {
                        case 0x3b:
                            processControlSection(section);
                            break;
                        case 0x3c:
                            processDataSection(section);
                            break;
                        default:
                            break;
                    }
                }
            }

            Logger.Instance.Write("", true, false);
            Logger.Instance.Write("Stopping reader for frequency " + dataProvider.Frequency + " PID 0x" + pid.ToString("X").ToLowerInvariant());
            dsmccReader.Stop();
            
            int totalBlocks = 0;
            foreach (DSMCCModule module in modules)
            {
                module.LogMessage();
                totalBlocks += module.Blocks.Count;
            }

            Logger.Instance.Write("Data blocks: " + totalBlocks + 
                " buffer space used: " + dataProvider.BufferSpaceUsed + 
                " discontinuities: " + dsmccReader.Discontinuities); 
        }        

        private bool checkAllDataLoaded()
        {
            if (dsiMessage == null || diiMessages == null || diiMessages.Count == 0)
            {
                if (TraceEntry.IsDefined(TraceName.DsmccComplete))
                {
                    int dsiCount = 0;
                    int diiCount = 0;

                    if (dsiMessage != null)
                        dsiCount++;
                    if (diiMessages != null)
                        diiCount = diiMessages.Count;

                    Logger.Instance.Write("DSMCC: DSI count: " + dsiCount + " DII count: " + diiCount);                    
                }

                return (false);
            }

            int foundCount = 0;
            int missingCount = 0;

            foreach (DSMCCDownloadInfoIndication diiMessage in diiMessages)
            {
                foreach (DSMCCDownloadInfoIndicationModule infoModule in diiMessage.ModuleList)
                {
                    bool found = false;

                    foreach (DSMCCModule module in modules)
                    {
                        if (module.ModuleID == infoModule.ModuleID)
                            found = true;
                    }

                    if (found)
                        foundCount++;
                    else
                        missingCount++;
                }
            }

            if (TraceEntry.IsDefined(TraceName.DsmccComplete))
                Logger.Instance.Write("DSMCC: Found: " + foundCount + " Missing: " + missingCount);

            int incompleteModules = 0;
            foreach (DSMCCModule module in modules)
            {
                if (!module.Complete)
                    incompleteModules++;
            }
            if (TraceEntry.IsDefined(TraceName.DsmccComplete))
                Logger.Instance.Write("DSMCC: Incomplete modules: " + incompleteModules);

            if (!DebugEntry.IsDefined(DebugName.DsmccIgnoreIncomplete))
                return (missingCount == 0 && incompleteModules == 0);
            else
                return (missingCount == 0);
        }

        private void processControlSection(Mpeg2Section section)
        {
            Mpeg2ExtendedHeader mpeg2Header = new Mpeg2ExtendedHeader();

            try
            {
                mpeg2Header.Process(section.Data);
                if (mpeg2Header.Current)
                {
                    DSMCCSection dsmccSection = new DSMCCSection();
                    dsmccSection.Process(section.Data, mpeg2Header);

                    if (dsmccSection.DSMCCMessage as DSMCCDownloadServerInitiate != null)
                        addDSIMessage(dsmccSection.DSMCCMessage as DSMCCDownloadServerInitiate);
                    else
                    {
                        if (dsmccSection.DSMCCMessage as DSMCCDownloadInfoIndication != null)
                            addDIIMessage(dsmccSection.DSMCCMessage as DSMCCDownloadInfoIndication);
                        else
                        {
                            if (dsmccSection.DSMCCMessage as DSMCCDownloadCancel != null)
                            {
                                Logger.Instance.Write("DSMCC Cancel message received: reloading all data");
                                dsiMessage = null;
                                diiMessages = null;
                                modules = null;
                            }
                        }
                    }
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                Logger.Instance.Write("<e> Error processing DSMCC control message: " + e.Message);
            }
        }

        private void processDataSection(Mpeg2Section section)
        {
            Mpeg2ExtendedHeader mpeg2Header = new Mpeg2ExtendedHeader();

            try
            {
                mpeg2Header.Process(section.Data);
                if (mpeg2Header.Current)
                {
                    DSMCCSection dsmccSection = new DSMCCSection();
                    dsmccSection.Process(section.Data, mpeg2Header);

                    if (dsmccSection.DSMCCMessage as DSMCCDownloadDataBlock != null)
                        addDDBMessage(dsmccSection.DSMCCMessage as DSMCCDownloadDataBlock);
                }
            }
            catch (ArgumentOutOfRangeException e)
            {
                Logger.Instance.Write("<e> Error processing DSMCC data message: " + e.Message);
            }
        }

        private bool addDSIMessage(DSMCCDownloadServerInitiate newMessage)
        {
            if (dsiMessage == null)
            {
                dsiMessage = newMessage;
                dsiMessage.LogMessage();
                return (true); ;
            }

            if (dsiMessage.DSMCCHeader.TransactionID.Identification == newMessage.DSMCCHeader.TransactionID.Identification)
            {
                if (dsiMessage.DSMCCHeader.TransactionID.Version == newMessage.DSMCCHeader.TransactionID.Version)
                    return (false);
                else
                {
                    dsiMessage = newMessage;
                    dsiMessage.LogMessage();
                }
            }
            else
            {
                dsiMessage = newMessage;
                dsiMessage.LogMessage();
            }

            return (true);
        }

        private bool addDIIMessage(DSMCCDownloadInfoIndication newMessage)
        {
            foreach (DSMCCDownloadInfoIndication oldMessage in diiMessages)
            {
                if (oldMessage.DownloadID == newMessage.DownloadID)
                {
                    if (oldMessage.DSMCCHeader.TransactionID.Identification == newMessage.DSMCCHeader.TransactionID.Identification)
                    {
                        if (oldMessage.DSMCCHeader.TransactionID.Version == newMessage.DSMCCHeader.TransactionID.Version)
                            return (false);
                        else
                        {
                            if (Logger.ProtocolLogger != null)
                                Logger.ProtocolLogger.Write("DII Message version change (" +
                                    oldMessage.DSMCCHeader.TransactionID.Version + " -> " +
                                    newMessage.DSMCCHeader.TransactionID.Version + ") - removing modules");
                            diiMessages.Remove(oldMessage);
                            removeModules(oldMessage);
                            diiMessages.Add(newMessage);
                            newMessage.LogMessage();
                            addModules(newMessage);
                            return (true);
                            
                        }
                    }
                    else
                    {
                        diiMessages.Add(newMessage);
                        newMessage.LogMessage();
                        addModules(newMessage);
                        return (true);
                    }
                }
            }

            diiMessages.Add(newMessage);
            newMessage.LogMessage();
            addModules(newMessage);

            return (true);
        }

        private void removeModules(DSMCCDownloadInfoIndication diiMessage)
        {
            foreach (DSMCCDownloadInfoIndicationModule module in diiMessage.ModuleList)
                checkRemoveModule(module);
        }

        private void checkRemoveModule(DSMCCDownloadInfoIndicationModule module)
        {
            foreach (DSMCCModule existingModule in modules)
            {
                if (existingModule.ModuleID == module.ModuleID)
                {
                    modules.Remove(existingModule);
                    return;
                }
            }
        }

        private void addModules(DSMCCDownloadInfoIndication diiMessage)
        {
            foreach (DSMCCDownloadInfoIndicationModule module in diiMessage.ModuleList)
                checkAddModule(module);
        }

        private void checkAddModule(DSMCCDownloadInfoIndicationModule module)
        {
            foreach (DSMCCModule oldModule in modules)
            {
                if (oldModule.ModuleID == module.ModuleID)
                {
                    if (oldModule.Version == module.ModuleVersion)
                        return;
                    else
                    {
                        if (oldModule.Version != module.ModuleVersion)
                        {
                            int replaceIndex = modules.IndexOf(oldModule);
                            modules.Remove(oldModule);
                            DSMCCModule replaceModule = new DSMCCModule(module.ModuleID, module.ModuleVersion, module.ModuleSize, module.OriginalSize);
                            if (replaceIndex != modules.Count)
                                modules.Insert(replaceIndex, replaceModule);
                            else
                                modules.Add(replaceModule);
                            return;
                        }
                    }
                }
                else
                {
                    if (oldModule.ModuleID > module.ModuleID)
                    {
                        DSMCCModule insertModule = new DSMCCModule(module.ModuleID, module.ModuleVersion, module.ModuleSize, module.OriginalSize);
                        modules.Insert(modules.IndexOf(oldModule), insertModule);
                        return;
                    }
                }
            }

            DSMCCModule newModule = new DSMCCModule(module.ModuleID, module.ModuleVersion, module.ModuleSize, module.OriginalSize);
            modules.Add(newModule);
        }

        private bool addDDBMessage(DSMCCDownloadDataBlock downloadDataBlock)
        {
            downloadDataBlock.LogMessage();

            foreach (DSMCCModule module in modules)
            {
                if (module.ModuleID == downloadDataBlock.ModuleID && module.Version == downloadDataBlock.ModuleVersion)
                    return (module.AddBlock(downloadDataBlock));
            }

            return (false);
        }

        /// <summary>
        /// Create the EPG entries.
        /// </summary>
        public override void FinishFrequency()
        {
            if (noMHEGPid)
                return;

            if (TraceEntry.IsDefined(TraceName.DsmccModules))
                logModules();

            if (DebugEntry.IsDefined(DebugName.LogTitles) && titleLogger == null)
                titleLogger = new Logger("EPG Titles.log");
            if (DebugEntry.IsDefined(DebugName.LogDescriptions) && descriptionLogger == null)
                descriptionLogger = new Logger("EPG Descriptions.log");

            foreach (TVStation tvStation in RunParameters.Instance.StationCollection)
            {
                bool process = checkChannelBouquet(tvStation);
                if (!process)
                    tvStation.ExcludedByChannel = true;                    
            }

            if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode == null)
            {
                processEPGforNZL();
                logChannelInfo();
                return;
            }

            switch (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode)
            {
                case Country.NewZealand:
                    processEPGforNZL();
                    break;
                case Country.Australia:
                    processEPGforAUS();
                    break;
                default:
                    break;
            }

            logChannelInfo();
        }

        private void processEPGforNZL()
        {
            if (dsiMessage == null)
                return;
            if (dsiMessage.ServiceGatewayInfo == null)
                return;
            if (dsiMessage.ServiceGatewayInfo.IOPIOR == null)
                return;
            if (dsiMessage.ServiceGatewayInfo.IOPIOR.TaggedProfiles == null)
                return;
            if (dsiMessage.ServiceGatewayInfo.IOPIOR.TaggedProfiles[0].ProfileBody == null)
                return;

            int serviceGatewayModuleID = dsiMessage.ServiceGatewayInfo.IOPIOR.TaggedProfiles[0].ProfileBody.ObjectLocation.ModuleID;
            byte[] serviceGatewayObjectKey = dsiMessage.ServiceGatewayInfo.IOPIOR.TaggedProfiles[0].ProfileBody.ObjectLocation.ObjectKeyData;

            BIOPServiceGatewayMessage serviceGateway = findObject(serviceGatewayModuleID, serviceGatewayObjectKey) as BIOPServiceGatewayMessage;
            if (serviceGateway == null)
                return;

            if (serviceGateway.Bindings == null)
                return;

            if (TraceEntry.IsDefined(TraceName.DsmccDirLayout))
                logDirectoryStructure(serviceGateway.Bindings);            
                
            BIOPDirectoryMessage epgDirectory = findObject(serviceGateway.Bindings, "epg", "dir") as BIOPDirectoryMessage;
            if (epgDirectory == null)
            {
                epgDirectory = findObject(serviceGateway.Bindings, "epgdtt", "dir") as BIOPDirectoryMessage;
                if (epgDirectory == null)
                    return;
            }

            BIOPDirectoryMessage dataDirectory = findObject(epgDirectory.Bindings, "data", "dir") as BIOPDirectoryMessage;
            if (dataDirectory == null)
                return;

            if (dataDirectory.Bindings == null)
                return;

            foreach (BIOPBinding dateBinding in dataDirectory.Bindings)
            {
                if (dateBinding.Names[0].Kind == "dir")
                {
                    if (dateBinding.IOPIOR != null)
                    {
                        int dateModuleID = dateBinding.IOPIOR.TaggedProfiles[0].ProfileBody.ObjectLocation.ModuleID;
                        byte[] dateObjectKey = dateBinding.IOPIOR.TaggedProfiles[0].ProfileBody.ObjectLocation.ObjectKeyData;
                        BIOPDirectoryMessage dateDirectory = findObject(dateModuleID, dateObjectKey) as BIOPDirectoryMessage;
                        if (dateDirectory != null)
                        {
                            if (dateDirectory.Bindings != null)
                            {
                                foreach (BIOPBinding fileBinding in dateDirectory.Bindings)
                                {
                                    if (fileBinding.Names[0].Kind == "fil")
                                    {
                                        if (fileBinding.IOPIOR != null)
                                        {
                                            int fileModuleID = fileBinding.IOPIOR.TaggedProfiles[0].ProfileBody.ObjectLocation.ModuleID;
                                            byte[] fileObjectKey = fileBinding.IOPIOR.TaggedProfiles[0].ProfileBody.ObjectLocation.ObjectKeyData;
                                            BIOPFileMessage epgFile = findObject(fileModuleID, fileObjectKey) as BIOPFileMessage;
                                            if (epgFile != null)
                                            {
                                                try
                                                {
                                                    processEPGFile(dateBinding.Names[0].Identity, fileBinding.Names[0].Identity, epgFile);
                                                }
                                                catch (ArgumentOutOfRangeException e)
                                                {
                                                    Logger.Instance.Write("Failed to process DSMCC file: " + e.Message);
                                                    modules.Clear();
                                                }
                                                catch (IndexOutOfRangeException e)
                                                {
                                                    Logger.Instance.Write("Failed to process DSMCC file: " + e.Message);
                                                    throw;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            combineMidnightPrograms();

            if (OptionEntry.IsDefined(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.Options, OptionName.UseImage))
            {
                BIOPDirectoryMessage pngsDirectory = findObject(epgDirectory.Bindings, "pngs", "dir") as BIOPDirectoryMessage;
                if (pngsDirectory == null)
                    return;

                if (pngsDirectory.Bindings == null)
                    return;

                foreach (BIOPBinding pngBinding in pngsDirectory.Bindings)
                {
                    if (pngBinding.Names[0].Kind == "fil")
                    {
                        if (pngBinding.IOPIOR != null)
                        {
                            int pngModuleID = pngBinding.IOPIOR.TaggedProfiles[0].ProfileBody.ObjectLocation.ModuleID;
                            byte[] pngObjectKey = pngBinding.IOPIOR.TaggedProfiles[0].ProfileBody.ObjectLocation.ObjectKeyData;
                            BIOPFileMessage pngFile = findObject(pngModuleID, pngObjectKey) as BIOPFileMessage;
                            if (pngFile != null)
                            {
                                if (pngBinding.Names[0].Identity[0] >= '0' && pngBinding.Names[0].Identity[0] <= '9')
                                {
                                    if (TraceEntry.IsDefined(TraceName.PngNames))
                                        Logger.Instance.Write("BIOP PNG File Name = " + pngBinding.Names[0].Identity);

                                    try
                                    {
                                        if (!Directory.Exists(Path.Combine(RunParameters.DataDirectory, "Images") + Path.DirectorySeparatorChar))
                                            Directory.CreateDirectory(Path.Combine(RunParameters.DataDirectory, "Images") + Path.DirectorySeparatorChar);
                                        string outFile = RunParameters.DataDirectory + Path.DirectorySeparatorChar + "Images" + Path.DirectorySeparatorChar + pngBinding.Names[0].Identity;
                                        FileStream outFileStream = new FileStream(outFile, FileMode.Create);
                                        outFileStream.Write(pngFile.ContentData, 0, pngFile.ContentData.Length);
                                        outFileStream.Close();
                                    }
                                    catch (IOException e)
                                    {
                                        Logger.Instance.Write("Failed to process PNG DSMCC file: " + e.Message);
                                        throw;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        private void processEPGforAUS()
        {
            if (dsiMessage == null)
                return;
            if (dsiMessage.ServiceGatewayInfo == null)
                return;
            if (dsiMessage.ServiceGatewayInfo.IOPIOR == null)
                return;
            if (dsiMessage.ServiceGatewayInfo.IOPIOR.TaggedProfiles == null)
                return;
            if (dsiMessage.ServiceGatewayInfo.IOPIOR.TaggedProfiles[0].ProfileBody == null)
                return;

            int serviceGatewayModuleID = dsiMessage.ServiceGatewayInfo.IOPIOR.TaggedProfiles[0].ProfileBody.ObjectLocation.ModuleID;
            byte[] serviceGatewayObjectKey = dsiMessage.ServiceGatewayInfo.IOPIOR.TaggedProfiles[0].ProfileBody.ObjectLocation.ObjectKeyData;

            BIOPServiceGatewayMessage serviceGateway = findObject(serviceGatewayModuleID, serviceGatewayObjectKey) as BIOPServiceGatewayMessage;
            if (serviceGateway == null)
                return;

            if (serviceGateway.Bindings == null)
                return;

            if (TraceEntry.IsDefined(TraceName.DsmccDirLayout))
                logDirectoryStructure(serviceGateway.Bindings);

            BIOPDirectoryMessage epgDirectory = findObject(serviceGateway.Bindings, "e1", "dir") as BIOPDirectoryMessage;
            if (epgDirectory == null)
            {
                Logger.Instance.Write("EPG base directory 'is missing - data cannot be processed");
                return;
            }

            Collection<DayEntry> dayEntries;

            BIOPFileMessage dayInfo = findObject(epgDirectory.Bindings, "day.txt", "fil") as BIOPFileMessage;
            if (dayInfo == null)
            {
                Logger.Instance.Write("Day Info file is missing - data assumed to be current");
                dayEntries = createDayEntries();
            }
            else
            {
                dayEntries = createDayEntries(dayInfo.ContentData);
                if (dayEntries == null || dayEntries.Count == 0)
                {
                    Logger.Instance.Write("Day Info file cannot be processed - data assumed to be current");
                    dayEntries = createDayEntries();
                }                    
            }

            Logger.Instance.Write("Day file date range is " + dayEntries[0].DateString + 
                " to " + dayEntries[dayEntries.Count - 1].DateString);

            BIOPFileMessage serviceInfo = findObject(epgDirectory.Bindings, "serviceinfo.txt", "fil") as BIOPFileMessage;
            if (serviceInfo == null)
            {
                Logger.Instance.Write("Service Info file is missing - data cannot be processed");
                return;
            }

            Collection<ServiceEntry> serviceEntries = createServiceEntries(serviceInfo.ContentData);
            if (serviceEntries == null || serviceEntries.Count == 0)
            {
                Logger.Instance.Write("Service Info file is empty or in the wrong format - data cannot be processed");
                return;
            }

            foreach (TVStation station in RunParameters.Instance.StationCollection)
            {
                if (station.Included)
                    processStation(station, serviceEntries, dayEntries, serviceGateway.Bindings);
            }

            combineMidnightPrograms();
        }

        private Collection<DayEntry> createDayEntries()
        {
            Collection<DayEntry> dayEntries = new Collection<DayEntry>();

            int dayNumber = 1;
            DateTime date = DateTime.Today;

            while (dayNumber < 7)
            {
                DayEntry dayEntry = new DayEntry(date.ToString("yyyyMMdd"),
                    dayNumber.ToString(),
                    date.ToShortDateString(),
                    "86400",
                    "90000",
                    "0");
                dayEntries.Add(dayEntry);

                dayNumber++;
                date = date.AddDays(1);
            }

            return (dayEntries);
        }

        private void processStation(TVStation station, Collection<ServiceEntry> serviceEntries, Collection<DayEntry> dayEntries, Collection<BIOPBinding> bindings)
        {
            ServiceEntry serviceEntry = findServiceEntry(station, serviceEntries);
            if (serviceEntry == null)
            {
                Logger.Instance.Write("Station " + station.Name + " (" + station.FullID + ") not in service info file - data cannot be processed");
                return;
            }

            BIOPDirectoryMessage epgDirectory = findObject(bindings, "e1", "dir") as BIOPDirectoryMessage;
            processDay(epgDirectory, dayEntries[0], serviceEntry);

            epgDirectory = findObject(bindings, "e2-8", "dir") as BIOPDirectoryMessage;
            if (epgDirectory == null)
                return;

            for (int index = 1; index < dayEntries.Count; index++ )
            {
                processDay(epgDirectory, dayEntries[index], serviceEntry);
                /*BIOPDirectoryMessage dayDirectory = findObject(epgDirectory.Bindings, day.ToString(), "dir") as BIOPDirectoryMessage;
                if (dayDirectory == null)
                    return;

                if (dayDirectory.Bindings == null)
                    return;

                foreach (BIOPBinding stationBinding in dayDirectory.Bindings)
                {
                    if (stationBinding.Names[0].Kind == "fil")
                    {
                        if (stationBinding.IOPIOR != null)
                        {
                            int fileModuleID = stationBinding.IOPIOR.TaggedProfiles[0].ProfileBody.ObjectLocation.ModuleID;
                            byte[] fileObjectKey = stationBinding.IOPIOR.TaggedProfiles[0].ProfileBody.ObjectLocation.ObjectKeyData;
                            BIOPFileMessage epgFile = findObject(fileModuleID, fileObjectKey) as BIOPFileMessage;
                            if (epgFile != null)
                            {
                                try
                                {
                                    string[] nameParts = stationBinding.Names[0].Identity.Split(new char[] { '_' });
                                    if (nameParts.Length == 3)
                                        processEPGFile(day, nameParts[2], epgFile);
                                    processEPGFile(day, nameParts[0], epgFile);
                                }
                                catch (ArgumentOutOfRangeException e)
                                {
                                    Logger.Instance.Write("Failed to process DSMCC file: " + e.Message);
                                    modules.Clear();
                                }
                                catch (IndexOutOfRangeException e)
                                {
                                    Logger.Instance.Write("Failed to process DSMCC file: " + e.Message);
                                    throw;
                                }
                            }
                        }
                    }
                }*/
            }
        }

        private Collection<DayEntry> createDayEntries(byte[] contentData)
        {
            Collection<byte[]> groupData = Utils.SplitBytes(contentData, 0x1e);

            Collection<DayEntry> dayEntries = null;

            for (int index = 1; index < groupData.Count; index++)
            {
                Collection<byte[]> dayData = Utils.SplitBytes(groupData[index], 0x1d);
                if (dayData.Count != 6)
                    return (null);

                if (dayEntries == null)
                    dayEntries = new Collection<DayEntry>();

                dayEntries.Add(new DayEntry(Utils.GetAsciiString(dayData[0]),
                    Utils.GetAsciiString(dayData[1]),
                    Utils.GetAsciiString(dayData[2]),
                    Utils.GetAsciiString(dayData[3]),
                    Utils.GetAsciiString(dayData[4]),
                    Utils.GetAsciiString(dayData[5])));
            }

            return (dayEntries);
        }

        private Collection<ServiceEntry> createServiceEntries(byte[] contentData)
        {
            Collection<byte[]> contentFields = Utils.SplitBytes(contentData, 0x1d);

            if (contentFields[0].Length != 0 || contentFields[1].Length != 0)
            {
                Logger.Instance.Write("Service Info data is in the wrong format (1) - cannot be processed");
                return(null);
            }

            int count = 0;

            try
            {
                count = Int32.Parse(Utils.GetAsciiString(contentFields[2]));
            }
            catch (FormatException)
            {
                Logger.Instance.Write("Service Info data is in the wrong format (2) - cannot be processed");
                return (null);
            }
            catch (OverflowException)
            {
                Logger.Instance.Write("Service Info data is in the wrong format (2) - cannot be processed");
                return (null);
            }

            int entryIndex = count + 3;
            
            Collection<ServiceEntry> serviceEntries = new Collection<ServiceEntry>();

            while (entryIndex < contentFields.Count)
            {
                ServiceEntry serviceEntry = new ServiceEntry(Utils.GetAsciiString(contentFields[entryIndex]),
                    Utils.GetAsciiString(contentFields[entryIndex + 1]),
                    Utils.GetAsciiString(contentFields[entryIndex + 2]),
                    Utils.GetAsciiString(contentFields[entryIndex + 3]));

                serviceEntries.Add(serviceEntry);

                entryIndex += 4;
            }

            if (TraceEntry.IsDefined(TraceName.ServiceEntries))
            {
                foreach (ServiceEntry serviceEntry in serviceEntries)
                    Logger.Instance.Write("Service entry:" +
                        " ONID=" + serviceEntry.OriginalNetworkID +
                        " TSID=" + serviceEntry.TransportStreamID +
                        " SID=" + serviceEntry.ServiceID +
                        " Ref=" + serviceEntry.ReferenceNumber +
                        " Crid=" + serviceEntry.BaseCRID);
            }

            return (serviceEntries);
        }

        private ServiceEntry findServiceEntry(TVStation station, Collection<ServiceEntry> serviceEntries)
        {
            foreach (ServiceEntry serviceEntry in serviceEntries)
            {
                if (serviceEntry.OriginalNetworkID == station.OriginalNetworkID &&
                    serviceEntry.TransportStreamID == station.TransportStreamID &&
                    serviceEntry.ServiceID == station.ServiceID)
                    return (serviceEntry);
            }

            return (null);
        }

        private void processDay(BIOPDirectoryMessage epgDirectory, DayEntry dayEntry, ServiceEntry serviceEntry)
        {
            BIOPDirectoryMessage dayDirectory = findObject(epgDirectory.Bindings, dayEntry.DayNumber.ToString(), "dir") as BIOPDirectoryMessage;
            if (dayDirectory == null)
                return;

            BIOPFileMessage epgFile = findObject(dayDirectory.Bindings, serviceEntry.ReferenceNumber, "fil") as BIOPFileMessage;
            if (epgFile == null)
                return;

            processEPGFile(dayEntry.OriginalDate, serviceEntry.ServiceID.ToString(), epgFile);                                
        }

        private void logModules()
        {
            Logger.Instance.WriteSeparator("Log DSMCC Modules");

            foreach (DSMCCModule module in modules)
            {
                Logger.Instance.Write("Module: 0x" + module.ModuleID.ToString("X"));

                if (module.Objects != null)
                {
                    foreach (BIOPMessage objectEntry in module.Objects)
                    {
                        Logger.Instance.Write("  Object: " + objectEntry.Kind + " key " + Utils.ConvertToHex(objectEntry.ObjectKeyData));

                        BIOPDirectoryMessage directoryMessage = objectEntry.MessageDetail as BIOPDirectoryMessage;
                        if (directoryMessage != null)
                            logDirectoryStructure(directoryMessage.Bindings);                        
                    }
                }
            }
        }

        private void logDirectoryStructure(Collection<BIOPBinding> bindings)
        {
            prefix += "    ";

            if (bindings == null)
            {
                Logger.Instance.Write(prefix + "No bindings");
                return;
            }

            foreach (BIOPBinding binding in bindings)
            {
                Logger.Instance.Write(prefix + "Binding: " + binding.Names[0].Identity + " Kind: " + binding.Names[0].Kind);
                if (binding.Names[0].Kind == "dir")
                {
                    if (binding.IOPIOR != null)
                    {
                        int moduleID = binding.IOPIOR.TaggedProfiles[0].ProfileBody.ObjectLocation.ModuleID;
                        byte[] objectKey = binding.IOPIOR.TaggedProfiles[0].ProfileBody.ObjectLocation.ObjectKeyData;
                        Logger.Instance.Write("Searching for module 0x" + moduleID.ToString("X") +
                            " object key " + Utils.ConvertToHex(objectKey));
                        BIOPDirectoryMessage directory = findObject(moduleID, objectKey) as BIOPDirectoryMessage;
                        if (directory != null)
                        {
                            if (directory.Bindings != null)
                                logDirectoryStructure(directory.Bindings);
                            else
                                Logger.Instance.Write(prefix + "Directory has no bindings");
                        }
                        else
                            Logger.Instance.Write(prefix + "Failed to find directory");
                    }
                }
                else
                {
                    if (binding.Names[0].Kind == "fil" && TraceEntry.IsDefined(TraceName.DsmccDumpFiles))
                    {
                        int fileModuleID = binding.IOPIOR.TaggedProfiles[0].ProfileBody.ObjectLocation.ModuleID;
                        byte[] fileObjectKey = binding.IOPIOR.TaggedProfiles[0].ProfileBody.ObjectLocation.ObjectKeyData;
                        BIOPFileMessage file = findObject(fileModuleID, fileObjectKey) as BIOPFileMessage;
                        if (file != null)
                            Logger.Instance.Dump("File Contents", file.ContentData, file.ContentLength > 64 ? 64 : file.ContentLength);
                        else
                            Logger.Instance.Write(prefix + "Failed to find file");                        
                    }
                }
            }

            prefix = prefix.Substring(4);
        }

        private BIOPMessageDetail findObject(Collection<BIOPBinding> bindings, string identity, string kind)
        {
            if (bindings == null)
                return (null);

            foreach (BIOPBinding binding in bindings)
            {
                if (binding.Names[0].Identity == identity && binding.Names[0].Kind == kind)
                {
                    if (binding.IOPIOR == null)
                        return (null);

                    int moduleID = binding.IOPIOR.TaggedProfiles[0].ProfileBody.ObjectLocation.ModuleID;
                    byte[] objectKey = binding.IOPIOR.TaggedProfiles[0].ProfileBody.ObjectLocation.ObjectKeyData;

                    return (findObject(moduleID, objectKey));
                }
            }

            return (null);
        }

        private BIOPMessageDetail findObject(int moduleID, byte[] objectKey)
        {
            foreach (DSMCCModule module in modules)
            {
                if (module.ModuleID == module.ModuleID && module.Complete)
                {
                    if (module.Objects != null)
                    {
                        foreach (BIOPMessage objectEntry in module.Objects)
                        {
                            if (Utils.CompareBytes(objectEntry.ObjectKeyData, objectKey))
                                return (objectEntry.MessageDetail);
                        }
                    }
                }
            }

            return (null);
        }

        private void processEPGFile(string dateString, string serviceID, BIOPFileMessage epgFile)
        {
            try
            {
                DateTime date = DateTime.ParseExact(dateString, "yyyyMMdd", null);
                processDSMCCEPGFile(date, Int32.Parse(serviceID), epgFile.ContentData, Logger.ProtocolLogger);
            }
            catch (FormatException)
            {
                throw (new ArgumentOutOfRangeException("The DSMCC file could not be processed"));
            }
        }

        private void processEPGFile(int dayNumber, string serviceID, BIOPFileMessage epgFile)
        {
            try
            {
                DateTime date = DateTime.Today + new TimeSpan(dayNumber - 1, 0, 0, 0);
                processDSMCCEPGFile(date, Int32.Parse(serviceID), epgFile.ContentData, Logger.ProtocolLogger);
            }
            catch (FormatException)
            {
                throw (new ArgumentOutOfRangeException("The DSMCC file could not be processed"));
            }
            catch (OverflowException)
            {
                throw (new ArgumentOutOfRangeException("The DSMCC file could not be processed"));
            }
        }

        private void processDSMCCEPGFile(DateTime date, int serviceID, byte[] fileData, Logger logger)
        {
            if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode == null)
            {
                processEPGFileForNZL(date, serviceID, fileData, logger);
                return;
            }

            switch (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode)
            {
                case Country.NewZealand:
                    processEPGFileForNZL(date, serviceID, fileData, logger);
                    break;
                case Country.Australia:
                    processEPGFileForAUS(date, serviceID, fileData, logger);
                    break;
                default:
                    break;
            }
        }

        private void processEPGFileForNZL(DateTime date, int serviceID, byte[] fileData, Logger logger)
        {
            TVStation tvStation = TVStation.FindCurrentStation(RunParameters.Instance.StationCollection, serviceID);
            if (tvStation == null)
                return;

            if (!tvStation.Included)
                return;

            if (logger != null && TraceEntry.IsDefined(TraceName.DsmccFile))
                logger.Dump("DSMCC Parser data - File Entry", fileData, fileData.Length);

            if (TraceEntry.IsDefined(TraceName.DsmccRecLayout))
            {
                Collection<byte[]> logRecords = Utils.SplitBytes(fileData, 0x1c);
                Logger.Instance.Write("Block contains " + logRecords.Count + " records");
                foreach (byte[] logRecord in logRecords)
                {
                    Collection<byte[]> logFields = Utils.SplitBytes(logRecord, 0x1d);
                    Logger.Instance.Write("Record contains " + logFields.Count + " fields");

                    StringBuilder recordString = new StringBuilder();

                    foreach (byte[] logField in logFields)
                    {
                        if (recordString.Length != 0)
                            recordString.Append(" : ");
                        recordString.Append(Utils.GetAsciiString(logField));
                    }

                    Logger.Instance.Write(recordString.ToString());
                }
            }

            int rootCRIDFieldNumber = MHEGParserParameters.GetField(MHEGParserParameters.FieldName.RootCRID);
            if (rootCRIDFieldNumber == -1)
                rootCRIDFieldNumber = 3;

            int programCountFieldNumber = MHEGParserParameters.GetField(MHEGParserParameters.FieldName.ProgramCount);
            if (programCountFieldNumber == -1)
                programCountFieldNumber = 4;

            int eventIDFieldNumber = MHEGParserParameters.GetField(MHEGParserParameters.FieldName.EventID);
            if (eventIDFieldNumber == -1)
                eventIDFieldNumber = 0;

            int startTimeFieldNumber = MHEGParserParameters.GetField(MHEGParserParameters.FieldName.StartTime);
            if (startTimeFieldNumber == -1)
                startTimeFieldNumber = 1;

            int endTimeFieldNumber = MHEGParserParameters.GetField(MHEGParserParameters.FieldName.EndTime);
            if (endTimeFieldNumber == -1)
                endTimeFieldNumber = 2;

            int eventCRIDFieldNumber = MHEGParserParameters.GetField(MHEGParserParameters.FieldName.ProgramCRID);
            if (eventCRIDFieldNumber == -1)
                eventCRIDFieldNumber = 6;

            int eventNameFieldNumber = MHEGParserParameters.GetField(MHEGParserParameters.FieldName.EventName);
            if (eventNameFieldNumber == -1)
                eventNameFieldNumber = 7;

            int shortDescriptionFieldNumber = MHEGParserParameters.GetField(MHEGParserParameters.FieldName.ShortDescription);
            if (shortDescriptionFieldNumber == -1)
                shortDescriptionFieldNumber = 8;

            int imageCountFieldNumber = MHEGParserParameters.GetField(MHEGParserParameters.FieldName.ImageCount);
            if (imageCountFieldNumber == -1)
                imageCountFieldNumber = 9;

            int seriesCRIDFieldNumber = MHEGParserParameters.GetField(MHEGParserParameters.FieldName.SeriesCRID);
            if (seriesCRIDFieldNumber == -1)
                seriesCRIDFieldNumber = 2;

            try
            {
                Collection<byte[]> records = Utils.SplitBytes(fileData, 0x1c);

                for (int index = 0; index < records.Count; index++)
                {
                    Collection<byte[]> headerFields = Utils.SplitBytes(records[index], 0x1d);

                    int expectedHeaderFieldCount = MHEGParserParameters.HeaderFields;
                    if (expectedHeaderFieldCount != -1 && expectedHeaderFieldCount != headerFields.Count)
                        throw (new IndexOutOfRangeException(
                            "MHEG format error - count of header fields is incorrect - expected " +
                            expectedHeaderFieldCount + " got " + headerFields.Count));

                    // These fields are not used
                    // byte[] programNumber = headerFields[0];
                    // string friendlyDate = Utils.GetString(headerFields[1]);
                    // string stationName = Utils.GetString(headerFields[2]);

                    string rootCRID = Utils.GetAsciiString(headerFields[rootCRIDFieldNumber]);
                    int programCount = Int32.Parse(Utils.GetAsciiString(headerFields[programCountFieldNumber]));

                    while (programCount > 0)
                    {
                        index++;

                        if (logger != null && TraceEntry.IsDefined(TraceName.DsmccRecord))
                            logger.Dump("DSMCC Parser data - Program Entry", records[index], records[index].Length);

                        Collection<byte[]> dataFields = Utils.SplitBytes(records[index], 0x1d);

                        try
                        {
                            if (dataFields[1].Length != 0)
                            {
                                EPGEntry epgEntry = new EPGEntry();
                                epgEntry.OriginalNetworkID = tvStation.OriginalNetworkID;
                                epgEntry.TransportStreamID = tvStation.TransportStreamID;
                                epgEntry.ServiceID = serviceID;

                                epgEntry.EventID = (date.DayOfYear * 1000) + Int32.Parse(Utils.GetAsciiString(dataFields[eventIDFieldNumber]));
                                epgEntry.StartTime = Utils.RoundTime(date.AddSeconds(((double)Int32.Parse(Utils.GetAsciiString(dataFields[startTimeFieldNumber])))));
                                epgEntry.Duration = Utils.RoundTime(date.AddSeconds(((double)Int32.Parse(Utils.GetAsciiString(dataFields[endTimeFieldNumber])))) - epgEntry.StartTime);

                                // These fields are not used
                                // byte[] titleLineCount = dataFields[3];
                                // byte[] friendlyTime = dataFields[4];
                                // byte[] entryType = dataFields[5];

                                string eventCRID = Utils.GetAsciiString(dataFields[eventCRIDFieldNumber]);

                                byte[] editedEventName = replaceByte(dataFields[eventNameFieldNumber], 0x0d, 0x20);
                                string eventName = Utils.GetString(editedEventName, "utf-8");
                                epgEntry.EventName = EditSpec.ProcessTitle(Utils.Compact(eventName));

                                byte[] editedDescription = replaceByte(dataFields[shortDescriptionFieldNumber], 0x0d, 0x20);
                                string eventDescription = Utils.GetString(editedDescription, "utf-8");
                                processNZLShortDescription(epgEntry, EditSpec.ProcessDescription(Utils.Compact(eventDescription)));

                                int iconCount = Int32.Parse(Utils.GetAsciiString(dataFields[imageCountFieldNumber]));

                                if (iconCount < 0 || iconCount > 10)
                                {
                                    Logger.Instance.Dump("DSMCC Parser error - Icon Count - File Entry", fileData, fileData.Length);
                                    if (logger != null)
                                        logger.Dump("DSMCC Parser error - Icon Count - File Entry", fileData, fileData.Length);
                                }
                                else
                                {
                                    int imageIndex;

                                    for (imageIndex = imageCountFieldNumber + 1; imageIndex < iconCount + imageCountFieldNumber + 1; imageIndex++)
                                    {
                                        switch (Utils.GetAsciiString(dataFields[imageIndex]))
                                        {
                                            case "/pngs/ao.png":
                                                epgEntry.ParentalRating = "AO";
                                                epgEntry.MpaaParentalRating = ParentalRating.FindMpaaRating("NZL", "MHEG5", "AO");
                                                break;
                                            case "/pngs/pgr.png":
                                                epgEntry.ParentalRating = "PGR";
                                                epgEntry.MpaaParentalRating = ParentalRating.FindMpaaRating("NZL", "MHEG5", "PGR");
                                                break;
                                            case "/pngs/g.png":
                                                epgEntry.ParentalRating = "G";
                                                epgEntry.MpaaParentalRating = ParentalRating.FindMpaaRating("NZL", "MHEG5", "G");
                                                break;
                                            case "/pngs/ear.png":
                                                epgEntry.SubTitles = "teletext";
                                                break;
                                            case "/pngs/hd.png":
                                                epgEntry.VideoQuality = "HDTV";
                                                break;
                                            case "/pngs/dolby.png":
                                                epgEntry.AudioQuality = "dolby digital";
                                                break;
                                            case "/pngs/ad.png":
                                                epgEntry.HasAudioDescription = true;
                                                break;
                                            default:
                                                break;
                                        }

                                    }

                                    int expectedDetailFieldCount = MHEGParserParameters.DetailFields;
                                    int addedDetailFieldCount = iconCount;

                                    string seriesCRID = string.Empty;

                                    if (Int32.Parse(Utils.GetAsciiString(dataFields[imageIndex])) != 0)
                                    {
                                        // This field is not used.
                                        // string entryType2 = Utils.GetString(dataFields[imageIndex + 1]);

                                        seriesCRID = Utils.GetAsciiString(dataFields[imageIndex + seriesCRIDFieldNumber]);

                                        // These fields are not used
                                        // string eventName2 = Utils.GetString(dataFields[imageIndex + 3]);                                        
                                        // string shortDescription2 = Utils.GetString(dataFields[imageIndex + 4]);
                                        // int otherIconCount = Int32.Parse(Utils.GetString(dataFields[imageIndex + 5]));

                                        addedDetailFieldCount += 6;
                                    }
                                    else
                                        addedDetailFieldCount += 2;

                                    if (expectedDetailFieldCount != -1 && expectedDetailFieldCount + addedDetailFieldCount != dataFields.Count)
                                        throw (new IndexOutOfRangeException(
                                            "MHEG format error - count of detail fields is incorrect - expected " +
                                            (expectedDetailFieldCount + addedDetailFieldCount) + " got " + dataFields.Count));

                                    processNZLCRID(epgEntry, seriesCRID, eventCRID);

                                    epgEntry.EventCategory = CustomProgramCategory.FindCategoryDescription(epgEntry.EventName);
                                    if (epgEntry.EventCategory == null)
                                        epgEntry.EventCategory = CustomProgramCategory.FindCategoryDescription(epgEntry.ShortDescription);

                                    bool include = ControllerBase.CheckEPGDays(epgEntry.StartTime);
                                    if (include)
                                    {
                                        tvStation.AddEPGEntry(epgEntry);

                                        if (titleLogger != null)
                                            logTitle(eventName, epgEntry, titleLogger);
                                        if (descriptionLogger != null)
                                            logDescription(eventDescription, epgEntry, descriptionLogger);
                                    }
                                }
                            }

                            programCount--;
                        }
                        catch (FormatException)
                        {
                            Logger.Instance.Dump("DSMCC Parser error - Format Exception - File Entry", fileData, fileData.Length);
                            if (logger != null)
                                logger.Dump("DSMCC Parser error - Format Exception - File Entry", fileData, fileData.Length);
                        }
                        catch (OverflowException)
                        {
                            Logger.Instance.Dump("DSMCC Parser error - Overflow Exception - File Entry", fileData, fileData.Length);
                            if (logger != null)
                                logger.Dump("DSMCC Parser error - Overflow Exception - File Entry", fileData, fileData.Length);
                        }
                        catch (Exception e)
                        {
                            Logger.Instance.Write("<E> DSMCC Parser exception of type " + e.GetType().Name + " has occurred");
                            Logger.Instance.Write("<E> " + e.Message);
                        }
                    }
                }
            }
            catch (FormatException e)
            {
                throw (new ArgumentOutOfRangeException("DSMCC file entry parsing failed: " + e.Message));
            }
            catch (OverflowException e)
            {
                throw (new ArgumentOutOfRangeException("DSMCC file entry parsing failed: " + e.Message));
            }
        }

        private byte[] replaceByte(byte[] inputBytes, byte oldValue, byte newValue)
        {
            byte[] outputBytes = new byte[inputBytes.Length];

            for (int index = 0; index < inputBytes.Length; index++)
            {
                if (inputBytes[index] == oldValue)
                    outputBytes[index] = newValue;
                else
                    outputBytes[index] = inputBytes[index];
            }

            return (outputBytes);
        }

        private void processNZLShortDescription(EPGEntry epgEntry, string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return;

            if (description.StartsWith("'") && description.EndsWith("'"))
                epgEntry.ShortDescription = description.Substring(1, description.Length - 2);
            else
                epgEntry.ShortDescription = description;

            Utils.GetNZLSeasonEpisodeNumbers(epgEntry);

            string editedDescription = epgEntry.ShortDescription;
            if (editedDescription == null)
                return;

            if (editedDescription.StartsWith("'. "))
            {
                epgEntry.ShortDescription = editedDescription.Substring(3);
                return;
            }

            if (!editedDescription.StartsWith("'"))
                return;

            int endIndex = editedDescription.IndexOf("'. ");
            if (endIndex == -1)
                return;

            if (endIndex == 0 || endIndex + 3 >= editedDescription.Length)
                return;

            epgEntry.ShortDescription = editedDescription.Substring(endIndex + 3);
            epgEntry.EventSubTitle = editedDescription.Substring(1, endIndex - 1);
        }

        private void processNZLCRID(EPGEntry epgEntry, string seriesCrid, string episodeCrid)
        {
            epgEntry.SeriesId = seriesCrid;
            epgEntry.EpisodeId = episodeCrid;

            epgEntry.SeasonCrid = seriesCrid;
            epgEntry.EpisodeCrid = episodeCrid;
        }

        private void processEPGFileForAUS(DateTime date, int serviceID, byte[] fileData, Logger logger)
        {
            TVStation tvStation = TVStation.FindStation(RunParameters.Instance.StationCollection, serviceID);
            if (tvStation == null)
                return;

            if (!tvStation.Included)
                return;

            if (logger != null && TraceEntry.IsDefined(TraceName.DsmccFile))
                logger.Dump("DSMCC Parser data - File Entry", fileData, fileData.Length);

            if (TraceEntry.IsDefined(TraceName.DsmccRecLayout))
            {
                Collection<byte[]> logRecords = Utils.SplitBytes(fileData, 0x1e);
                Logger.Instance.Write("Block contains " + logRecords.Count + " records");
                foreach (byte[] logRecord in logRecords)
                {
                    Collection<byte[]> logFields = Utils.SplitBytes(fileData, 0x1d);
                    Logger.Instance.Write("Record contains " + logFields.Count + " fields");
                    foreach (byte[] logField in logFields)
                        Logger.Instance.Write("    Field: " + Utils.GetAsciiString(logField));
                }
            }

            /*int rootCRIDFieldNumber = MHEGParserParameters.GetField(MHEGParserParameters.FieldName.RootCRID);
            if (rootCRIDFieldNumber == -1)
                rootCRIDFieldNumber = 0;*/

            int endTimeFieldNumber = MHEGParserParameters.GetField(MHEGParserParameters.FieldName.EndTime);
            if (endTimeFieldNumber == -1)
                endTimeFieldNumber = 0;

            int startTimeFieldNumber = MHEGParserParameters.GetField(MHEGParserParameters.FieldName.StartTime);
            if (startTimeFieldNumber == -1)
                startTimeFieldNumber = 1;

            int eventCRIDFieldNumber = MHEGParserParameters.GetField(MHEGParserParameters.FieldName.ProgramCRID);
            if (eventCRIDFieldNumber == -1)
                eventCRIDFieldNumber = 2;

            int seriesCRIDFieldNumber = MHEGParserParameters.GetField(MHEGParserParameters.FieldName.SeriesCRID);
            if (seriesCRIDFieldNumber == -1)
                seriesCRIDFieldNumber = 3;

            int eventNameFieldNumber = MHEGParserParameters.GetField(MHEGParserParameters.FieldName.EventName);
            if (eventNameFieldNumber == -1)
                eventNameFieldNumber = 4;

            int shortDescriptionFieldNumber = MHEGParserParameters.GetField(MHEGParserParameters.FieldName.ShortDescription);
            if (shortDescriptionFieldNumber == -1)
                shortDescriptionFieldNumber = 5;

            int highDefinitionFieldNumber = MHEGParserParameters.GetField(MHEGParserParameters.FieldName.HighDefinition);
            if (highDefinitionFieldNumber == -1)
                highDefinitionFieldNumber = 6;

            int closedCaptionsFieldNumber = MHEGParserParameters.GetField(MHEGParserParameters.FieldName.ClosedCaptions);
            if (closedCaptionsFieldNumber == -1)
                closedCaptionsFieldNumber = 7;

            int parentalRatingFieldNumber = MHEGParserParameters.GetField(MHEGParserParameters.FieldName.ParentalRating);
            if (parentalRatingFieldNumber == -1)
                parentalRatingFieldNumber = 8;

            try
            {
                Collection<byte[]> records = Utils.SplitBytes(fileData, 0x1e);

                for (int index = 1; index < records.Count; index++)
                {
                    if (logger != null && TraceEntry.IsDefined(TraceName.DsmccRecord))
                        logger.Dump("DSMCC Parser data - Program Entry", records[index], records[index].Length);

                    Collection<byte[]> dataFields = Utils.SplitBytes(records[index], 0x1d);

                    if (dataFields[0].Length != 0)
                    {
                        try
                        {
                            EPGEntry epgEntry = new EPGEntry();
                            epgEntry.OriginalNetworkID = tvStation.OriginalNetworkID;
                            epgEntry.TransportStreamID = tvStation.TransportStreamID;
                            epgEntry.ServiceID = serviceID;

                            epgEntry.StartTime = Utils.RoundTime(date.AddSeconds(((double)Int32.Parse(Utils.GetAsciiString(dataFields[startTimeFieldNumber])))));
                            epgEntry.Duration = Utils.RoundTime(date.AddSeconds(((double)Int32.Parse(Utils.GetAsciiString(dataFields[endTimeFieldNumber])))) - epgEntry.StartTime);
                            string eventCRID = Utils.GetAsciiString(dataFields[eventCRIDFieldNumber]);
                            string seriesCRID = Utils.GetAsciiString(dataFields[seriesCRIDFieldNumber]);

                            byte[] editedEventName = replaceByte(dataFields[eventNameFieldNumber], 0x0d, 0x20);
                            string eventName = Utils.GetString(editedEventName, "utf-8");
                            epgEntry.EventName = EditSpec.ProcessTitle(Utils.Compact(eventName));

                            byte[] editedDescription = replaceByte(dataFields[shortDescriptionFieldNumber], 0x0d, 0x20);
                            string eventDescription = Utils.GetString(editedDescription, "utf-8");
                            processAUSShortDescription(epgEntry, EditSpec.ProcessDescription(Utils.Compact(eventDescription)));
                                
                            if (Int32.Parse(Utils.GetAsciiString(dataFields[highDefinitionFieldNumber])) == 1)
                            {
                                epgEntry.AspectRatio = "16:9";
                                epgEntry.VideoQuality = "HDTV";
                                epgEntry.AudioQuality = "dolby digital";
                            }

                            if (Int32.Parse(Utils.GetAsciiString(dataFields[closedCaptionsFieldNumber])) == 1)
                                epgEntry.SubTitles = "teletext";

                            string parentalRating = Utils.GetAsciiString(dataFields[parentalRatingFieldNumber]).Replace("(", "").Replace(")", "");
                            if (parentalRating.Length != 0)
                            {
                                epgEntry.ParentalRating = parentalRating;
                                epgEntry.MpaaParentalRating = ParentalRating.FindMpaaRating("AUS", "MHEG5", parentalRating);
                                epgEntry.ParentalRatingSystem = ParentalRating.FindSystem("AUS", "MHEG5", parentalRating);
                            }

                            processAUSCRID(epgEntry, seriesCRID, eventCRID);

                            epgEntry.EventCategory = CustomProgramCategory.FindCategoryDescription(epgEntry.EventName);
                            if (epgEntry.EventCategory == null)
                                epgEntry.EventCategory = CustomProgramCategory.FindCategoryDescription(epgEntry.ShortDescription);

                            bool include = ControllerBase.CheckEPGDays(epgEntry.StartTime);
                            if (include)
                            {
                                tvStation.AddEPGEntry(epgEntry);

                                if (titleLogger != null)
                                    logTitle(eventName, epgEntry, titleLogger);
                                if (descriptionLogger != null)
                                    logDescription(eventDescription, epgEntry, descriptionLogger);
                            }
                        }
                        catch (FormatException)
                        {
                            Logger.Instance.Dump("DSMCC Parser error - Format Exception - File Entry", fileData, fileData.Length);
                            if (logger != null)
                                logger.Dump("DSMCC Parser error - Format Exception - File Entry", fileData, fileData.Length);
                        }
                        catch (OverflowException)
                        {
                            Logger.Instance.Dump("DSMCC Parser error - Overflow Exception - File Entry", fileData, fileData.Length);
                            if (logger != null)
                                logger.Dump("DSMCC Parser error - Overflow Exception - File Entry", fileData, fileData.Length);
                        }
                        catch (Exception e)
                        {
                            Logger.Instance.Write("<E> DSMCC Parser exception of type " + e.GetType().Name + " has occurred");
                            Logger.Instance.Write("<E> " + e.Message);
                        }
                    }
                }
            }
            catch (FormatException e)
            {
                throw (new ArgumentOutOfRangeException("DSMCC file entry parsing failed: " + e.Message));
            }
            catch (OverflowException e)
            {
                throw (new ArgumentOutOfRangeException("DSMCC file entry parsing failed: " + e.Message));
            }
        }

        private void processAUSShortDescription(EPGEntry epgEntry, string description)
        {
            string editedDescription;

            if (OptionEntry.IsDefined(OptionName.NoRemoveData))
            {
                epgEntry.ShortDescription = description;
                editedDescription = description;
            }
            else
            {
                editedDescription = removeParentalRating(description);
                editedDescription = removeHDFlag(editedDescription);
                editedDescription = removeClosedCaptionsFlag(editedDescription);

                if (editedDescription.StartsWith(epgEntry.EventName + " - "))
                    editedDescription = editedDescription.Substring(epgEntry.EventName.Length + 3);
            }

            int doubleIndex = editedDescription.IndexOf("))");
            if (doubleIndex != -1)
            {
                editedDescription = editedDescription.Replace("))", ">)");

                while (editedDescription[doubleIndex] != '(')
                    doubleIndex--;

                editedDescription = editedDescription.Remove(doubleIndex, 1);
                editedDescription = editedDescription.Insert(doubleIndex, "<");
            }

            string[] descriptionParts = editedDescription.Split(new char[] { '(' });

            if (doubleIndex != -1)
            {
                for (int index = 0; index < descriptionParts.Length; index++)
                {
                    descriptionParts[index] = descriptionParts[index].Replace("<", "(");
                    descriptionParts[index] = descriptionParts[index].Replace(">", ")");
                }
            }

            if (!OptionEntry.IsDefined(OptionName.NoRemoveData))
            {
                epgEntry.ShortDescription = descriptionParts[0];

                for (int index = 1; index < descriptionParts.Length; index++)            
                {
                    if (!descriptionParts[index].Trim().EndsWith(")"))
                        epgEntry.ShortDescription = epgEntry.ShortDescription + " (" + descriptionParts[index];
                }
            }

            if (descriptionParts.Length == 1)
            {
                if (epgEntry.EventSubTitle == null &&
                    epgEntry.ShortDescription.Length != 0 &&
                    epgEntry.EventName.Length > epgEntry.ShortDescription.Length)
                {
                    if (epgEntry.EventName.StartsWith(epgEntry.ShortDescription))
                    {
                        epgEntry.EventSubTitle = epgEntry.EventName.Substring(epgEntry.ShortDescription.Length + 1).Trim();
                        epgEntry.EventName = epgEntry.EventName.Substring(0, epgEntry.ShortDescription.Length).Trim();
                    }
                }

                return;
            }

            for (int index = 1; index < descriptionParts.Length; index++)            
            {
                if (descriptionParts[index].Trim() != "CC)" && descriptionParts[index].Trim().EndsWith(")"))
                {
                    string descriptionPart = descriptionParts[index];

                    bool processed = getDate(epgEntry, descriptionPart);
                    if (!processed)
                    {
                        if (descriptionPart.Contains(@"/") || epgEntry.EventSubTitle != null)
                            getCast(epgEntry, descriptionPart);
                        else
                        {
                            string subTitle = descriptionPart.Trim();
                            if (subTitle.Length != 0)
                            {
                                epgEntry.EventSubTitle = subTitle.Substring(0, subTitle.Length - 1);

                                int actualLength = epgEntry.EventName.Length - epgEntry.EventSubTitle.Length;
                                if (actualLength > 0)
                                {
                                    if (epgEntry.EventName.EndsWith(epgEntry.EventSubTitle))
                                        epgEntry.EventName = epgEntry.EventName.Substring(0, actualLength).Trim();
                                }

                                string checkString = epgEntry.EventName + " - ";

                                if (epgEntry.ShortDescription.StartsWith(checkString))
                                    epgEntry.ShortDescription = epgEntry.ShortDescription.Substring(checkString.Length).Trim();                            
                            }
                        }
                    }
                }
            }
        }

        private bool getDate(EPGEntry epgEntry, string description)
        {
            if (description.Trim().Length != 5 || description[4] != ')')
                return (false);

            try
            {
                int year = Int32.Parse(description.Substring(0, 4));
                epgEntry.Date = description.Substring(0, 4);
                return (true);
            }
            catch (FormatException)
            {
                return (false);
            }
            catch (OverflowException)
            {
                return (false);
            }
        }

        private bool getCast(EPGEntry epgEntry, string description)
        {
            if (!description.Trim().EndsWith(")"))
                return (false);

            string[] castMembers = description.Trim().Substring(0, description.Length - 1).Split(new char[] { '/' });

            epgEntry.Cast = new Collection<string>();

            foreach (string castMember in castMembers)
                epgEntry.Cast.Add(castMember);

            return (true);
        }

        private string removeParentalRating(string description)
        {
            if (OptionEntry.IsDefined(OptionName.NoRemoveData) || description[0] != '(')
                return (description);

            int endIndex = description.IndexOf(')');
            if (endIndex == -1)
                return (description);

            if (endIndex + 1 >= description.Length)
                return (description);

            return (description.Substring(endIndex + 1).Trim());
        }

        private string removeHDFlag(string description)
        {
            if (OptionEntry.IsDefined(OptionName.NoRemoveData))
                return (description);

            int startIndex = description.IndexOf("[HD]");
            if (startIndex == -1)
                return (description);

            if (startIndex + 4 >= description.Length)
                return (description);

            return (description.Substring(startIndex + 4).Trim());
        }

        private string removeClosedCaptionsFlag(string description)
        {
            if (OptionEntry.IsDefined(OptionName.NoRemoveData))
                return (description);

            int startIndex = description.IndexOf("[CC]");
            if (startIndex == -1)
                return (description);

            if (startIndex + 4 >= description.Length)
                return (description);

            return (description.Substring(startIndex + 4).Trim());
        }

        private void processAUSCRID(EPGEntry epgEntry, string seriesCRID, string episodeCRID)
        {
            epgEntry.SeriesId = seriesCRID;
            epgEntry.EpisodeId = episodeCRID;

            epgEntry.SeasonCrid = seriesCRID;
            epgEntry.EpisodeCrid = episodeCRID;
        }

        private void logTitle(string title, EPGEntry epgEntry, Logger logger)
        {
            logger.Write(epgEntry.OriginalNetworkID + ":" + epgEntry.TransportStreamID + ":" + epgEntry.ServiceID + " " +
                epgEntry.StartTime.ToShortDateString() + " " +
                epgEntry.StartTime.ToString("HH:mm") + " - " +
                epgEntry.StartTime.Add(epgEntry.Duration).ToString("HH:mm") + " " +
                title);
        }

        private void logDescription(string description, EPGEntry epgEntry, Logger logger)
        {
            logger.Write(epgEntry.OriginalNetworkID + ":" + epgEntry.TransportStreamID + ":" + epgEntry.ServiceID + " " +
                epgEntry.StartTime.ToShortDateString() + " " +
                epgEntry.StartTime.ToString("HH:mm") + " - " +
                epgEntry.StartTime.Add(epgEntry.Duration).ToString("HH:mm") + " " +
                description);
        }

        private bool checkChannelBouquet(TVStation tvStation)
        {
            if (!RunParameters.Instance.ChannelDataNeeded)
                return (true);

            if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.ChannelBouquet != -1)
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
                                if (channel.OriginalNetworkID == tvStation.OriginalNetworkID &&
                                    channel.TransportStreamID == tvStation.TransportStreamID &&
                                    channel.ServiceID == tvStation.ServiceID &&
                                    tvStation.LogicalChannelNumber == -1)
                                    tvStation.LogicalChannelNumber = channel.UserChannel;
                            }
                        }
                    }
                }
                else
                {
                    if (Channel.Channels.Count != 0)
                    {
                        foreach (Channel channel in Channel.Channels)
                        {
                            TVStation station = TVStation.FindStation(RunParameters.Instance.StationCollection,
                                channel.OriginalNetworkID, channel.TransportStreamID, channel.ServiceID);
                            if (station != null && station.LogicalChannelNumber == -1)
                                station.LogicalChannelNumber = channel.UserChannel;
                        }
                    }
                }
            }

            return (true);
        }

        private void logChannelInfo() 
        {
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

        private void combineMidnightPrograms()
        {
            foreach (TVStation tvStation in RunParameters.Instance.StationCollection)
            {
                for (int index = 0; index < tvStation.EPGCollection.Count; index++)
                {
                    EPGEntry epgEntry = tvStation.EPGCollection[index];
                    checkMidnightBreak(tvStation, epgEntry, index);
                }
            }
        }

        private void checkMidnightBreak(TVStation tvStation, EPGEntry currentEntry, int index)
        {
            if (index == tvStation.EPGCollection.Count - 1)
                return;

            EPGEntry nextEntry = tvStation.EPGCollection[index + 1];

            if (currentEntry.EventName != nextEntry.EventName)
                return;

            bool combined = false;

            if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode == null || RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode == Country.NewZealand)
                combined = checkNZLTimes(currentEntry, nextEntry);
            else
            {
                if (RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.CountryCode == Country.Australia)
                    combined = checkAUSTimes(currentEntry, nextEntry);
            }

            if (combined)
                tvStation.EPGCollection.RemoveAt(index + 1);
        }

        private bool checkNZLTimes(EPGEntry currentEntry, EPGEntry nextEntry)
        {
            if (!currentEntry.EndsAtMidnight)
                return (false);

            if (!nextEntry.StartsAtMidnight)
                return (false);

            if (currentEntry.StartTime + currentEntry.Duration != nextEntry.StartTime)
                return (false);

            if (nextEntry.Duration > new TimeSpan(3, 0, 0))
                return (false);

            Logger.Instance.Write("Combining " + currentEntry.ScheduleDescription + " with " + nextEntry.ScheduleDescription);
            currentEntry.Duration = currentEntry.Duration + nextEntry.Duration;

            return (true);
        }

        private bool checkAUSTimes(EPGEntry currentEntry, EPGEntry nextEntry)
        {
            if (!nextEntry.StartsAtMidnight)
                return (false);

            if (currentEntry.StartTime + currentEntry.Duration != nextEntry.StartTime + nextEntry.Duration)
                return (false);

            Logger.Instance.Write("Combining " + currentEntry.ScheduleDescription + " with " + nextEntry.ScheduleDescription);

            return (true);
        }

        /// <summary>
        /// Process an EIT carousel.
        /// </summary>
        /// <param name="carouselDirectories">The list of carousel directories.</param>
        /// <param name="fileSuffix">The filename suffix of the files to be included. Can be null.</param>
        /// <returns>A collection of tuples containing file name and file data or null if no data present.</returns>
        public Collection<Tuple<string, byte[]>> ProcessEITCarousel(Collection<string> carouselDirectories, string fileSuffix)
        {
            if (dsiMessage == null)
                return null;
            if (dsiMessage.ServiceGatewayInfo == null)
                return null;
            if (dsiMessage.ServiceGatewayInfo.IOPIOR == null)
                return null;
            if (dsiMessage.ServiceGatewayInfo.IOPIOR.TaggedProfiles == null)
                return null;
            if (dsiMessage.ServiceGatewayInfo.IOPIOR.TaggedProfiles[0].ProfileBody == null)
                return null;

            int serviceGatewayModuleID = dsiMessage.ServiceGatewayInfo.IOPIOR.TaggedProfiles[0].ProfileBody.ObjectLocation.ModuleID;
            byte[] serviceGatewayObjectKey = dsiMessage.ServiceGatewayInfo.IOPIOR.TaggedProfiles[0].ProfileBody.ObjectLocation.ObjectKeyData;

            BIOPServiceGatewayMessage serviceGateway = findObject(serviceGatewayModuleID, serviceGatewayObjectKey) as BIOPServiceGatewayMessage;
            if (serviceGateway == null)
                return null;

            if (serviceGateway.Bindings == null)
                return null;

            if (TraceEntry.IsDefined(TraceName.DsmccDirLayout))
                logDirectoryStructure(serviceGateway.Bindings);

            Collection<BIOPBinding> bindings = serviceGateway.Bindings;

            foreach (string directory in carouselDirectories)
            {
                BIOPDirectoryMessage directoryMessage = findObject(bindings, directory, "dir") as BIOPDirectoryMessage;
                if (directoryMessage == null)
                {
                    Logger.Instance.Write("Carousel directory not found: " + directory);
                    return null;
                }
                else
                    bindings = directoryMessage.Bindings;
            }

            Collection<Tuple<string, byte[]>> eitFiles = new Collection<Tuple<string, byte[]>>();

            foreach (BIOPBinding binding in bindings)
            {
                if (binding.Names[0].Kind == "fil")
                {
                    if (string.IsNullOrWhiteSpace(fileSuffix) || binding.Names[0].Identity.EndsWith(fileSuffix))
                    {
                        if (binding.IOPIOR != null)
                        {
                            int moduleID = binding.IOPIOR.TaggedProfiles[0].ProfileBody.ObjectLocation.ModuleID;
                            byte[] objectKey = binding.IOPIOR.TaggedProfiles[0].ProfileBody.ObjectLocation.ObjectKeyData;
                            BIOPFileMessage file = findObject(moduleID, objectKey) as BIOPFileMessage;
                            if (file != null)
                                eitFiles.Add(Tuple.Create(binding.Names[0].Identity, file.ContentData));
                        }                    
                    }
                }
            }

            return eitFiles;            
        }

        private class ServiceEntry
        {
            internal int OriginalNetworkID { get { return (originalNetworkID); } }
            internal int TransportStreamID { get { return (transportStreamID); } }
            internal int ServiceID { get { return (serviceID); } }
            internal string ReferenceNumber { get { return (referenceNumber); } }
            internal string BaseCRID { get { return (baseCRID); } }
            internal string Name { get { return (name); } }

            private int originalNetworkID;
            private int transportStreamID;
            private int serviceID;
            private string referenceNumber;
            private string baseCRID;
            private string name;

            private ServiceEntry() { }

            internal ServiceEntry(string identities, string referenceNumber, string baseCRID, string name)
            {
                string[] identityParts = identities.Split(new char[] { '_' });
                if (identityParts.Length != 3)
                    return;

                try
                {
                    originalNetworkID = Int32.Parse(identityParts[0].Trim());
                    transportStreamID = Int32.Parse(identityParts[1].Trim());
                    serviceID = Int32.Parse(identityParts[2].Trim());
                }
                catch (FormatException) { return; }
                catch (OverflowException) { return; }

                this.referenceNumber = referenceNumber.Trim();                
                this.baseCRID = baseCRID.Trim();
                this.name = name.Trim();
            }
        }

        private class DayEntry
        {
            internal string OriginalDate { get; private set; }
            internal DateTime Date { get; private set; }
            internal int DayNumber { get; private set; }
            internal string DateString { get; private set; }
            internal int Seconds { get; private set; }
            internal int Unknown1 { get; private set; }
            internal int Unknown2 { get; private set; }

            private DayEntry() { }

            internal DayEntry(string date, string dayNumber, string dateString, string seconds, string unknown1, string unknown2)
            {
                if (date.Length != 8)
                    return;

                try
                {
                    OriginalDate = date;
                    Date = DateTime.ParseExact(date, "yyyyMMdd", null);
                    DayNumber = Int32.Parse(dayNumber);
                    DateString = dateString;
                    Seconds = Int32.Parse(seconds);
                    Unknown1 = Int32.Parse(unknown1);
                    Unknown2 = Int32.Parse(unknown2);
                }
                catch (FormatException) { return; }
                catch (OverflowException) { return; }
                catch (ArgumentException) { return; }
            }
        }
    }
}