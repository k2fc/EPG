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

using System.Data.SQLite;
using System.Data.SQLite.Generic;

using DirectShow;
using DomainObjects;

using zlib;

namespace DVBServices
{
    /// <summary>
    /// The class that controls the acquisition and processing of the NDS protocol.
    /// </summary>
    public class NdsController : ControllerBase
    {
        /// <summary>
        /// Get the collection type supported by this collector.
        /// </summary>
        public override CollectionType CollectionType { get { return (CollectionType.NDS); } }
        /// <summary>
        /// Return true if the data is complete; false otherwise.
        /// </summary>
        public override bool AllDataProcessed { get { return (checkAllDataLoaded()); } }

        private TSStreamReader ndsReader;

        private Collection<NdsType> ndsTypes;
        private Collection<FileSpec> fileSpecs;
        private byte[] unzippedData;
        private int unzipCount;

        private string workingPath;

        private int checkCount;

        /// <summary>
        /// Initialize a new instance of the TvaController class.
        /// </summary>
        public NdsController() { }

        /// <summary>
        /// Stop acquiring and processing TV Anytime data.
        /// </summary>
        public override void Stop()
        {
            Logger.Instance.Write("Stopping section readers");

            if (ndsReader != null)
                ndsReader.Stop();

            Logger.Instance.Write("Stopped section readers");
        }

        /// <summary>
        /// Acquire and process the data.
        /// </summary>
        /// <param name="dataProvider">A sample data provider.</param>
        /// <param name="worker">The background worker that is running this collection.</param>
        /// <returns>A CollectorReply code.</returns>
        public override CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            return (Process(dataProvider, worker, CollectionSpan.AllData));
        }

        /// <summary>
        /// Acquire and process the data.
        /// </summary>
        /// <param name="dataProvider">A sample data provider.</param>
        /// <param name="worker">The background worker that is running this collection.</param>
        /// <param name="collectionSpan">The amount of data to collect.</param>
        /// <returns>A CollectorReply code.</returns>
        public override CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker, CollectionSpan collectionSpan)
        {
            workingPath = Path.Combine(RunParameters.DataDirectory, "EPGC_Temporary") + Path.DirectorySeparatorChar;

            if (collectionSpan == CollectionSpan.AllData)
            {
                CustomProgramCategory.Load();
                ParentalRating.Load();
                /*TextEdit.Load();*/

                if (!string.IsNullOrWhiteSpace(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.EITCarousel))
                    EITCarouselFile.Load();
            }

            GetStationData(dataProvider, worker);
            if (worker.CancellationPending)
                return (CollectorReply.Cancelled);

            if (RunParameters.Instance.StationCollection.Count == 0)
            {
                Logger.Instance.Write("<e> No stations located - data collection abandoned");
                return (CollectorReply.OK);
            }

            if (collectionSpan == CollectionSpan.ChannelsOnly)
                return (CollectorReply.OK);

            EITCarousel carousel = EITCarouselFile.FindCarousel(RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.EITCarousel);
            if (carousel == null)
            {
                Logger.Instance.Write("Carousel '" + RunParameters.Instance.CurrentFrequency.AdvancedRunParamters.EITCarousel + "' not defined");
                return (CollectorReply.FatalFormatError);
            }

            foreach (EITCarouselPidSpec pidSpec in carousel.PidSpecs)
            {
                dataProvider.Frequency.DSMCCPid = pidSpec.Pid;

                GetNdsSections(dataProvider, worker);
                if (worker.CancellationPending)
                    return (CollectorReply.Cancelled);
            }

            return (CollectorReply.OK);
        }

        /// <summary>
        /// Build the NDS tables.
        /// </summary>
        /// <param name="dataProvider">The data provider.</param>
        /// <param name="worker">The background worker.</param>
        public void GetNdsSections(ISampleDataProvider dataProvider, BackgroundWorker worker)
        {
            int pid = dataProvider.Frequency.DSMCCPid;

            if (pid == 0)
            {
                Logger.Instance.Write("No NDS PID for frequency " + dataProvider.Frequency);
                return;
            }
            
            Logger.Instance.Write("Collecting NDS data from PID 0x" + pid.ToString("X").ToLowerInvariant(), false, true);

            dataProvider.ChangePidMapping(new int[] { pid });
            
            ndsReader = new TSStreamReader(500, dataProvider.BufferAddress);
            ndsReader.Run();

            while (!checkAllDataLoaded())
            {
                if (worker.CancellationPending)
                    return;

                Thread.Sleep(2000);
                Logger.Instance.Write(".", false, false);

                Collection<Mpeg2Section> sections = new Collection<Mpeg2Section>();

                ndsReader.Lock("LoadMessages");
                if (ndsReader.Sections.Count != 0)
                {
                    foreach (Mpeg2Section section in ndsReader.Sections)
                        sections.Add(section);
                    ndsReader.Sections.Clear();
                }
                ndsReader.Release("LoadMessages");

                foreach (Mpeg2Section section in sections)
                {
                    switch (section.Table)
                    {
                        case 0x9e:
                            processNdsSection(section);
                            break;
                        default:
                            break;
                    }
                }
            }

            Logger.Instance.Write("", true, false);
            processNdsData();

            Logger.Instance.Write("Stopping reader for frequency " + dataProvider.Frequency + " PID 0x" + pid.ToString("X").ToLowerInvariant());
            ndsReader.Stop();
            
            Logger.Instance.Write("Buffer space used: " + dataProvider.BufferSpaceUsed + 
                " discontinuities: " + ndsReader.Discontinuities); 
        }        

        private bool checkAllDataLoaded()
        {
            if (ndsTypes == null || fileSpecs == null)
                return false;

            bool loaded = true;

            foreach (FileSpec fileSpec in fileSpecs)
            {
                NdsType ndsType = findNdsType(fileSpec.FileNumber);
                if (ndsType != null)
                {
                    Logger.Instance.Write("File number 0x" + fileSpec.FileNumber.ToString("x4") + " filespec size is " + fileSpec.FileSize + " data size is " + ndsType.DataSize);

                    if (ndsType.DataSize != fileSpec.FileSize)
                        loaded = false;
                }
                else
                    loaded = false;
            }

            if (DebugEntry.IsDefined(DebugName.NDS) && !loaded)
            {
                checkCount++;
                loaded = checkCount == 10;
            }

            return loaded;
        }

        private void processNdsSection(Mpeg2Section section)
        {
            int type = Utils.Convert2BytesToInt(section.Data, 3);

            NdsType ndsType = findNdsType(Utils.Convert2BytesToInt(section.Data, 3));
            ndsType.AddBlock((int)((section.Data[11] * 256) + section.Data[6]), section.Data);

            if (fileSpecs == null && ndsType.Type == 0 && ndsType.Blocks.Count > 1)
            {
                fileSpecs = extractSectionNumbers(ndsType.Blocks[1].Data);
                addFileNames(fileSpecs, ndsType.Blocks[1].Data);
            }
        }

        private Collection<FileSpec> extractSectionNumbers(byte[] dataBlock)
        {
            Collection<FileSpec> fileSpecs = new Collection<FileSpec>();

            int index = 35;
            int lastFileNumber = 0;

            while (lastFileNumber != 1)
            {
                int fileNumber = Utils.Convert2BytesToInt(dataBlock, index);
                byte[] fileInfo = Utils.GetBytes(dataBlock, index + 2, 12);
                fileSpecs.Add(new FileSpec(fileNumber, fileInfo));

                lastFileNumber = fileNumber;
                index += 14;
            }

            return fileSpecs;
        }

        private void addFileNames(Collection<FileSpec> fileSpecs, byte[] dataBlock)
        {
            int index = Utils.Convert2BytesToInt(dataBlock, 24) + 12;
            int length = Utils.Convert2BytesToInt(dataBlock, 26);
            int entry = 0;

            while (length > 0)
            {
                Tuple<byte[], byte[]> fileSpec = getFileSpec(dataBlock, index);

                string fileName = Encoding.ASCII.GetString(fileSpec.Item1);
                string fileType = Encoding.ASCII.GetString(fileSpec.Item2);

                fileSpecs[entry].FileName = fileName;
                fileSpecs[entry].FileType = fileType;

                index += fileSpec.Item1.Length + fileSpec.Item2.Length + 2;
                length -= fileSpec.Item1.Length + fileSpec.Item2.Length + 2;
                entry++;
            }
        }

        private Tuple<byte[], byte[]> getFileSpec(byte[] dataBlock, int index)
        {
            int length = 0;

            while (dataBlock[index + length] != 0x00)
                length++;

            byte[] fileName = new byte[length];
            Array.Copy(dataBlock, index, fileName, 0, fileName.Length);

            index += fileName.Length + 1;

            length = 0;

            while (dataBlock[index + length] != 0x00)
                length++;

            byte[] fileType = new byte[length];
            Array.Copy(dataBlock, index, fileType, 0, fileType.Length);

            return Tuple.Create<byte[], byte[]>(fileName, fileType);
        }

        private NdsType findNdsType(int type)
        {
            if (ndsTypes == null)
                ndsTypes = new Collection<NdsType>();

            foreach (NdsType oldType in ndsTypes)
            {
                if (oldType.Type == type)
                    return oldType;

                if (oldType.Type > type)
                {
                    NdsType insertType = new NdsType(type);
                    ndsTypes.Insert(ndsTypes.IndexOf(oldType), insertType);
                    return insertType;
                }
            }

            NdsType newType = new NdsType(type);
            ndsTypes.Add(newType);
            
            return newType;
        }

        private void processNdsData()
        {
            if (ndsTypes == null)
            {
                Logger.Instance.Write("No NDS blocks loaded");
                return;
            }

            Logger databaseLogger = new Logger(Path.Combine(RunParameters.DataDirectory, "NDS Database.log"));

            if (Directory.Exists(workingPath))
                Directory.Delete(workingPath, true);
            Directory.CreateDirectory(workingPath);

            foreach (NdsType ndsType in ndsTypes)
            {
                Logger.Instance.Write("NDS type: 0x" + ndsType.Type.ToString("x4"));

                ndsType.CombineBlocks();

                if (ndsType.Type == 0x0002 || ndsType.Type > 0x1000)
                {
                    try
                    {
                        MemoryStream output = new MemoryStream();
                        ZOutputStream zstream = new ZOutputStream(output);

                        unzippedData = new byte[1024 * 1024 * 5];

                        zstream.Write(ndsType.Data, 0, ndsType.Data.Length);
                        zstream.Flush();

                        output.Seek(0, SeekOrigin.Begin);
                        unzipCount = output.Read(unzippedData, 0, unzippedData.Length);

                        Logger.Instance.Write("Type 0x" + ndsType.Type.ToString("x4") + " zipped bytes: " + ndsType.Data.Length + " unzipped bytes: " + unzipCount);
                        Logger.Instance.Dump("Unzipped Block", unzippedData, (unzipCount > 64 ? 64 : unzipCount));

                        string outFile = workingPath + getFileName(ndsType.Type, fileSpecs);
                        FileStream outFileStream = new FileStream(outFile, FileMode.Create);

                        outFileStream.Write(unzippedData, 0, unzipCount);
                        outFileStream.Close();

                        Logger.Instance.Write("Created file " + outFile);
                        logTables(outFile, databaseLogger);
                    }
                    catch (Exception e)
                    {
                        Logger.Instance.Write("Type 0x" + ndsType.Type.ToString("x4") + " not unzipped: " + e.Message);
                    }
                }
                else
                {
                    if (ndsType.Type == 0)
                    {
                        foreach (FileSpec fileSpec in fileSpecs)
                            Logger.Instance.Write("File spec: 0x" + fileSpec.FileNumber.ToString("x4") + " file name: " + fileSpec.FileName + " file info: " + Utils.ConvertToHex(fileSpec.FileInfo));
                    }
                    else
                    {
                        if (ndsType.Type == 0x0001)
                        {
                            string outFile = workingPath + getFileName(ndsType.Type, fileSpecs);
                            FileStream outFileStream = new FileStream(outFile, FileMode.Create);

                            outFileStream.Write(ndsType.Data, 0, ndsType.Data.Length);
                            outFileStream.Close();

                            Logger.Instance.Write("Created file " + outFile);
                        }
                        else
                            processBinXml(ndsType);
                    }
                }
            }

            logData(databaseLogger);
        }

        private string getFileName(int fileNumber, Collection<FileSpec> fileSpecs)
        {
            foreach (FileSpec fileSpec in fileSpecs)
            {
                if (fileSpec.FileNumber == fileNumber)
                    return fileSpec.FileName;
            }

            return null;
        }

        private void logTables (string outFile, Logger databaseLogger)
        {
            Logger.Instance.Write("Processing SQL data");

            SQLiteConnection connection = new SQLiteConnection("Data Source=" + outFile + ";Version=3;");
            connection.Open();

            if (outFile.Contains("strings-segment"))
                logStringTables(connection, databaseLogger);
            else
            {
                if (outFile.Contains("events-segment"))
                    logEventTables(connection, databaseLogger);
                else
                {
                    if (outFile.Contains("services.db"))
                        logServiceTable(connection, databaseLogger);
                    else
                    {
                        if (outFile.Contains("group.db"))
                            logGroupTable(connection, databaseLogger);
                    }
                }
            }

            connection.Close();
        }

        private void logStringTables(SQLiteConnection connection, Logger logger)
        {
            if (!DebugEntry.IsDefined(DebugName.NDSSqlStrings))
                return;

            SQLiteCommand command = new SQLiteCommand("select * from String", connection);
            SQLiteDataReader reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                long gsHandle = (long)reader["gs_handle"];
                byte[] stringBytes = reader["string"] as byte[];
                long stringType = (long)reader["string_type"];

                logger.Write("String: gs_handle: " + gsHandle + " string type: " + stringType + " string: " + Encoding.UTF8.GetString(stringBytes));
            }

            command = new SQLiteCommand("select * from String_Index", connection);
            reader = command.ExecuteReader();
            
            while (reader.Read())
            {
                long eHandle = (long)reader["e_handle"];
                long nameHandle = (long)reader["name_handle"];
                long shortDescHandle = (long)reader["short_descr_handle"];

                logger.Write("String_Index: e_handle: " + eHandle + " name handle: " + nameHandle + " short desc handle: " + shortDescHandle);
            }
        }

        private void logEventTables(SQLiteConnection connection, Logger logger)
        {
            if (!DebugEntry.IsDefined(DebugName.NDSSqlEvents))
                return;

            SQLiteCommand command = new SQLiteCommand("select * from Event", connection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                long eHandle = (long)reader["e_handle"];
                long eventId = (long)reader["event_id"];
                long duration = (long)reader["duration"];
                long cHandle = (long)reader["c_handle"];
                long genreRating = (long)reader["genre_rating"];
                string eventRef = reader["event_ref"] as string;
                long flags = (long)reader["flags"];
                byte[] binXml = reader["binxml_fragment"] as byte[];

                logger.Write("Event: e_handle: " + eHandle + " event_id: " + eventId + 
                    " duration: " + duration + " c_handle: " + cHandle + " genre_rating: 0x" + genreRating.ToString("x4") +
                    " event_ref: " + eventRef + " flags: 0x" + flags.ToString("x4") +
                    " binxml(" + binXml.Length + "): " + Utils.ConvertToHex(binXml, binXml.Length));                
            }

            command = new SQLiteCommand("select * from Content", connection);
            reader = command.ExecuteReader();

            while (reader.Read())
            {
                long cHandle = (long)reader["c_handle"];
                string contentRef = reader["content_ref"] as string;
                
                logger.Write("Content: c_handle: " + cHandle + " content_ref: " + contentRef);
            }
        }

        private void logServiceTable(SQLiteConnection connection, Logger logger)
        {
            if (!DebugEntry.IsDefined(DebugName.NDSSqlServices))
                return;

            SQLiteCommand command = new SQLiteCommand("select * from Service", connection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                long sHandle = (long)reader["s_handle"];
                long serviceId = (long)reader["service_id"];
                long tsId = (long)reader["ts_id"];
                long networkId = (long)reader["network_id"];
                byte[] binXml = reader["binxml_fragment"] as byte[];
                string key = reader["key"] as string;   

                logger.Write("Service: s_handle: " + sHandle + " service_id: " + serviceId + " ts_id: " + tsId +
                    " network_id: " + networkId + " key: " + key +
                    " binxml(" + binXml.Length + "): " + Utils.ConvertToHex(binXml, binXml.Length));

                for (int index = 1; index < 8; index++)
                {
                    int byteIndex = 4;
                    int bitIndex = index;

                    byte[] shifted = NdsUtils.GetBits(binXml, ref byteIndex, ref bitIndex, ((binXml.Length - 4) * 8) - index);
                    logger.Dump("Shift " + index, shifted, shifted.Length);
                }
            } 

            command = new SQLiteCommand("select * from ServiceGroup", connection);
            reader = command.ExecuteReader();

            while (reader.Read())
            {
                long sgHandle = (long)reader["sg_handle"];
                long serviceGroupId = (long)reader["service_group_id"];
                byte[] binXml = reader["binxml_fragment"] as byte[];

                logger.Write("Service Group: sg_handle: " + sgHandle + " service_group_id: " + serviceGroupId +
                    " binxml(" + binXml.Length + "): " + Utils.ConvertToHex(binXml, binXml.Length));  
            }

            command = new SQLiteCommand("select * from GroupServiceLink", connection);
            reader = command.ExecuteReader();

            while (reader.Read())
            {
                long sgHandle = (long)reader["sg_handle"];
                long sHandle = (long)reader["s_handle"];
                long lcn = (long)reader["lcn"];

                logger.Write("Group Service Link: sg_handle: " + sgHandle + " s_handle: " + sHandle + " lcn: " + lcn);
            }
        }

        private void logGroupTable(SQLiteConnection connection, Logger logger)
        {
            if (!DebugEntry.IsDefined(DebugName.NDSSqlGroups))
                return;

            SQLiteCommand command = new SQLiteCommand("select * from Groups", connection);
            SQLiteDataReader reader = command.ExecuteReader();

            while (reader.Read())
            {
                long gHandle = (long)reader["g_handle"];
                long groupType = (long)reader["group_type"];
                long groupExpiration = (long)reader["group_expiration"];
                byte[] binXml = reader["binxml_fragment"] as byte[];

                logger.Write("Group: g_handle: " + gHandle + " group_type: " + groupType +
                    " group_expiration: " + groupExpiration +
                    " binxml(" + binXml.Length + "): " + Utils.ConvertToHex(binXml, binXml.Length));

                for (int index = 1; index < 8; index++)
                {
                    int byteIndex = 0;
                    int bitIndex = index;

                    byte[] shifted = NdsUtils.GetBits(binXml, ref byteIndex, ref bitIndex, (binXml.Length * 8) - 8);
                    logger.Dump("Shift " + index, shifted, shifted.Length);
                }
            }
        }

        private void processBinXml(NdsType ndsType)
        {
            if (DebugEntry.IsDefined(DebugName.NDSBinXmlBlocks))
            {
                Logger.Instance.Dump("BinXml Block 0", ndsType.Blocks[0].Data, (ndsType.Blocks[0].Data.Length > 4096 ? 4096 : ndsType.Blocks[0].Data.Length));
                Logger.Instance.Dump("BinXml Data (" + ndsType.Data.Length + " bytes)", ndsType.Data, ndsType.Data.Length);
            }

            int nameOffset = 57;

            string name = Utils.GetAsciiString(ndsType.Data, nameOffset + 1, ndsType.Data[nameOffset] - 1);

            int pbdOffset;
            if (ndsType.Type == 4 || ndsType.Type == 7)
                pbdOffset = 109;
            else
            {
                if (ndsType.Type == 8)
                    pbdOffset = 113;
                else
                    pbdOffset = 105;
            }

            string pbd = Utils.GetAsciiString(ndsType.Data, pbdOffset + 1, ndsType.Data[pbdOffset] - 1);

            int urnOffset;
            if (ndsType.Type == 4 || ndsType.Type == 7)
                urnOffset = 144;
            else
            {
                if (ndsType.Type == 8)
                    urnOffset = 152;
                else
                    urnOffset = 136;
            }

            string urn = Utils.GetAsciiString(ndsType.Data, urnOffset + 1, ndsType.Data[urnOffset] - 1);

            int listOffset;
            if (ndsType.Type == 5 || ndsType.Type == 6)
                listOffset = 232;
            else
            {
                if (ndsType.Type == 4 || ndsType.Type == 7)
                    listOffset = 240;
                else
                    listOffset = 256;
            }

            Collection<BinXmlSchemaEntry> schemaEntries = new Collection<BinXmlSchemaEntry>();

            while (ndsType.Data[listOffset] < 10)
            {
                int entityType = ndsType.Data[listOffset];

                listOffset++;

                int length = 0;
                while (ndsType.Data[listOffset + length] != 0x00)
                    length++;

                string entityName = Utils.GetAsciiString(ndsType.Data, listOffset, length);

                schemaEntries.Add(new BinXmlSchemaEntry(entityType, entityName));

                listOffset += length + 1;
            }

            if (DebugEntry.IsDefined(DebugName.NDSBinXml))
            {
                Logger.Instance.Write("BinXml: name=" + name + " pbd: " + pbd + " urn: " + urn);

                foreach (BinXmlSchemaEntry schemaEntry in schemaEntries)
                    Logger.Instance.Write("    Schema entry: entity type: " + schemaEntry.EntityType + " entity name: " + schemaEntry.EntityName);
            }

            /*if (ndsType.Type == 4)
            {
                int byteIndex = 141;
                int bitIndex = 0;

                NdsDecoderInit decoderInit = new NdsDecoderInit();
                decoderInit.Process(ndsType.Data, ref byteIndex, ref bitIndex);
            }*/
        }

        private void logData(Logger logger)
        {
            /*Collection<SQLiteConnection> stringsConnections = new Collection<SQLiteConnection>();
            
            foreach (TvaType tvaType in tvaTypes)
            {
                string outFile = workingPath + getFileName(tvaType.Type, fileSpecs);

                if (outFile.Contains("strings-segment"))
                {
                    SQLiteConnection stringsConnection = new SQLiteConnection("Data Source=" + outFile + ";Version=3;");
                    stringsConnection.Open();

                    stringsConnections.Add(stringsConnection);
                }                                   
            }

            foreach (TvaType tvaType in tvaTypes)
            {
                string outFile = workingPath + getFileName(tvaType.Type, fileSpecs);

                if (outFile.Contains("events-segment"))
                {
                    SQLiteConnection eventsConnection = new SQLiteConnection("Data Source=" + outFile + ";Version=3;");
                    eventsConnection.Open();

                    SQLiteCommand eventsCommand = new SQLiteCommand("select * from Event", eventsConnection);
                    SQLiteDataReader eventsReader = eventsCommand.ExecuteReader();

                    while (eventsReader.Read())
                    {
                        long eHandle = (long)eventsReader["e_handle"];
                        long eventId = (long)eventsReader["event_id"];
                        long duration = (long)eventsReader["duration"];
                        long cHandle = (long)eventsReader["c_handle"];
                        long genreRating = (long)eventsReader["genre_rating"];
                        string eventRef = eventsReader["event_ref"] as string;
                        long flags = (long)eventsReader["flags"];
                        byte[] binXml = eventsReader["binxml_fragment"] as byte[];

                        SQLiteCommand contentsCommand = new SQLiteCommand("select * from Content where c_handle = " + cHandle, eventsConnection);
                        SQLiteDataReader contentsReader = contentsCommand.ExecuteReader();

                        string contentRef = null;
                        if (contentsReader.HasRows)
                        {
                            while (contentsReader.Read())
                                contentRef = contentsReader["content_ref"] as string;
                        }

                        Tuple<string, string> nameDescription = findNameDescription(eHandle, stringsConnections, logger);

                        logger.Write("Event: e_handle: " + eHandle + " event_id: " + eventId +
                            " duration: " + duration + " c_handle: " + cHandle + " genre_rating: 0x" + genreRating.ToString("x4") +
                            " event_ref: " + eventRef + " flags: 0x" + flags.ToString("x4") +
                            " binxml: " + Utils.ConvertToHex(binXml, binXml.Length > 32 ? 32 : binXml.Length));
                        logger.Write("    title: " + (nameDescription != null ? nameDescription.Item1 : "?"));
                        logger.Write("    description: " + (nameDescription != null ? nameDescription.Item2 : "?"));
                        logger.Write("    content ref: " + (contentRef != null ? contentRef : "?"));
                    }

                    eventsReader.Close();
                    eventsConnection.Close();
                }
            }*/
        }

        private Tuple<string, string> findNameDescription(long eHandle, Collection<SQLiteConnection> stringsConnections, Logger logger)
        {
            foreach (SQLiteConnection stringIndexConnection in stringsConnections)
            {
                SQLiteCommand stringIndexCommand = new SQLiteCommand("select * from String_Index where e_handle = " + eHandle, stringIndexConnection);
                SQLiteDataReader stringIndexReader = stringIndexCommand.ExecuteReader();

                if (stringIndexReader.HasRows)
                {
                    while (stringIndexReader.Read())
                    {
                        string name = null;
                        string description = null;

                        foreach (SQLiteConnection stringConnection in stringsConnections)
                        {
                            long nameHandle = (long)stringIndexReader["name_handle"];
                            SQLiteCommand stringCommand = new SQLiteCommand("select * from String where gs_handle = " + nameHandle, stringConnection);
                            SQLiteDataReader stringReader = stringCommand.ExecuteReader();

                            if (stringReader.HasRows)
                            {
                                while (stringReader.Read())
                                {
                                    byte[] stringBytes = stringReader["string"] as byte[];
                                    name = Encoding.UTF8.GetString(stringBytes);

                                    stringReader.Close();
                                    break;
                                }
                            }
                            else
                                stringReader.Close();
                        }      

                        foreach (SQLiteConnection stringConnection in stringsConnections)
                        {
                            long shortDescHandle = (long)stringIndexReader["short_descr_handle"];

                            SQLiteCommand stringCommand = new SQLiteCommand("select * from String where gs_handle = " + shortDescHandle, stringConnection);
                            SQLiteDataReader stringReader = stringCommand.ExecuteReader();

                            if (stringReader.HasRows)
                            {
                                while (stringReader.Read())
                                {
                                    byte[] stringBytes = stringReader["string"] as byte[];
                                    description = Encoding.UTF8.GetString(stringBytes);

                                    stringReader.Close();
                                    break;
                                }
                            }
                            else
                                stringReader.Close();
                        }

                        if (name != null || description != null)
                        {                            
                            stringIndexReader.Close();
                            return Tuple.Create<string, string>(name, description);
                        }
                    }
                }
                else
                    stringIndexReader.Close();
            }

            return null;
        }

        private class NdsType
        {
            internal int Type { get; private set; }
            internal Collection<NdsBlock> Blocks { get; private set; }

            internal byte[] Data { get; private set; }
            internal int DataSize { get; private set; }

            private NdsType() { }

            internal NdsType(int type)
            {
                Type = type;
            }

            internal void AddBlock(int sequenceNumber, byte[] sectionData)
            {
                if (Blocks == null)
                    Blocks = new Collection<NdsBlock>();

                foreach (NdsBlock oldBlock in Blocks)
                {
                    if (oldBlock.SequenceNumber == sequenceNumber)
                    {
                        if (compareBlocks(oldBlock.Data, sectionData))
                            return;
                    }

                    if (oldBlock.SequenceNumber > sequenceNumber)
                    {
                        NdsBlock insertBlock = new NdsBlock(sequenceNumber, sectionData);
                        Blocks.Insert(Blocks.IndexOf(oldBlock), insertBlock);
                        DataSize += sectionData.Length - 16;
                        return;
                    }
                }

                NdsBlock addBlock = new NdsBlock(sequenceNumber, sectionData);
                Blocks.Add(addBlock);
                DataSize += sectionData.Length - 16;

                return;
            }

            internal void CombineBlocks()
            {
                int totalLength = 0;

                foreach (NdsBlock ndsBlock in Blocks)
                    totalLength += ndsBlock.Data.Length - 16;

                Data = new byte[totalLength];
                int index = 0;

                foreach (NdsBlock ndsBlock in Blocks)
                {
                    Array.Copy(ndsBlock.Data, 12, Data, index, ndsBlock.Data.Length - 16);
                    index += ndsBlock.Data.Length - 16; 
                }
            }

            private bool compareBlocks(byte[] block1, byte[] block2)
            {
                if (block1.Length != block2.Length)
                    return false;

                for (int index = 0; index < block1.Length; index++)
                {
                    if (block1[index] != block2[index])
                        return false;
                }

                return true;
            }
        }

        private class NdsBlock
        {
            internal int SequenceNumber { get; private set; }
            internal byte[] Data { get; private set; }

            private NdsBlock() { }

            internal NdsBlock(int sequenceNumber, byte[] data)
            {
                SequenceNumber = sequenceNumber;
                Data = data;
            }
        }

        private class FileSpec
        {
            internal int FileNumber { get; private set; }
            internal byte[] FileInfo { get; private set; }

            internal string FileName { get; set; }
            internal string FileType { get; set; }

            internal int FileSize { get { return (((FileInfo[2] & 0x0f) * 256 * 256 * 256) + (FileInfo[3] * 256 * 256) + (FileInfo[4] * 256) + FileInfo[5]) >> 4; } } 

            private FileSpec() { }

            internal FileSpec(int fileNumber, byte[] fileInfo)
            {
                FileNumber = fileNumber;
                FileInfo = fileInfo;
            }
        }

        private class BinXmlSchemaEntry
        {
            internal int EntityType { get; private set; }
            internal string EntityName { get; private set; }
            
            private BinXmlSchemaEntry() { }

            internal BinXmlSchemaEntry(int entityType, string entityName)
            {
                EntityType = entityType;
                EntityName = entityName;
            }
        }
    }
}