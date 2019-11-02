﻿////////////////////////////////////////////////////////////////////////////////// 
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
using System.IO;
using System.Text;

namespace DomainObjects
{
    /// <summary>
    /// The class that describes the system logger.
    /// </summary>
    public class Logger : ILogger
    {
        /// <summary>
        /// Get an instance of a logger.
        /// </summary>
        public static Logger Instance
        {
            get
            {
                if (instance == null)
                    instance = new Logger();
                return (instance);
            }
        }

        /// <summary>
        /// Get the protocol logger instance.
        /// </summary>
        public static Logger ProtocolLogger
        {
            get
            {
                if (TraceEntry.IsDefined(TraceName.Protocol))
                {
                    if (protocolLogger == null)
                        protocolLogger = new Logger("Protocol.log");
                    return (protocolLogger);
                }
                else
                    return (null);
            }
        }

        /// <summary>
        /// Get the identation for protocol logging.
        /// </summary>
        public static string ProtocolIndent
        {
            get { return (protocolIndent); }
            set { protocolIndent = value; }
        }
        
        /// <summary>
        /// Get or set quiet mode.
        /// </summary>
        public bool QuietMode
        {
            get { return (quietMode); }
            set { quietMode = value; }
        }

        /// <summary>
        /// Get the full path to the history file.
        /// </summary>
        public static string HistoryFilePath { get { return (Path.Combine(RunParameters.DataDirectory, historyFileName)); } }

        /// <summary>
        /// Get the full path to the Network file.
        /// </summary>
        public static string NetworkFilePath { get { return (Path.Combine(RunParameters.DataDirectory, networkFileName)); } }
        
        
        private static Logger instance;
        private static string defaultFileName = "EPG Collector.log";
        private string logFileName;

        private long maxFileSize = normalMb * 1024 * 1024;
        private const long normalMb = 8;
        private const long extendedMb = 256;

        private bool consoleOutput;

        private static string protocolIndent = "";
        private static Logger protocolLogger;

        private static string historyFileName = "EPG Collector.hst";
        private static string networkFileName = "EPG Collector Network.log";
        
        private StreamReader inputStreamReader;
        private bool noCreateInput;

        private bool quietMode;

        private object lockObject = new Object();

        private Logger() 
        {
            consoleOutput = true;
            logFileName = defaultFileName;

            string fileSize;

            if (DebugEntry.IsDefined(DebugName.ExtendedLogFile))
            {
                maxFileSize = extendedMb * 1024 * 1024;
                fileSize = extendedMb.ToString();
            }
            else
            {
                maxFileSize = normalMb * 1024 * 1024;
                fileSize = normalMb.ToString();
            }

            Write("Started on " + DateTime.Now.ToShortDateString() + " max file size " + fileSize + "Mb");
        }

        /// <summary>
        /// Initialize a new instance of the Logger class with a given log file name.
        /// </summary>
        /// <param name="fileName">The name of the log file.</param>
        public Logger(string fileName) : this(fileName, true)
        {            
        }

        /// <summary>
        /// Initialize a new instance of the Logger class with a given log file name and an option to clear.
        /// </summary>
        /// <param name="fileName">The name of the log file.</param>
        /// <param name="clear">True to clear the log; false otherwise.</param>
        public Logger(string fileName, bool clear)
        {
            if (clear)
                Clear(fileName);
            logFileName = fileName;

            string fileSize;

            if (DebugEntry.IsDefined(DebugName.ExtendedLogFile))
            {
                maxFileSize = extendedMb * 1024 * 1024;
                fileSize = extendedMb.ToString();
            }
            else
            {
                maxFileSize = normalMb * 1024 * 1024;
                fileSize = normalMb.ToString();
            }

            Write("Started on " + DateTime.Now.ToShortDateString() + " max file size " + fileSize + "Mb");
        }

        /// <summary>
        /// Initialize a new instance of the Logger class for reading.
        /// </summary>
        /// <param name="noCreateInput">True if file not to be created; false otherwise.</param>
        public Logger(bool noCreateInput) 
        {
            this.noCreateInput = noCreateInput; 
        }

        /// <summary>
        /// Clear the default log file.
        /// </summary>
        public static string Clear()
        {
            return(Clear(defaultFileName));
        }

        /// <summary>
        /// Clear a log file.
        /// </summary>
        /// <param name="logFileName">The name of the log file.</param>
        /// <returns>Null if the log has been cleared; an error message otherwise.</returns>
        public static string Clear(string logFileName)
        {
            FileStream fileStream;

            try
            {
                fileStream = new FileStream(Path.Combine(RunParameters.DataDirectory, logFileName), FileMode.Open);
                fileStream.SetLength(0);
                fileStream.Close();
                return (null);
            }
            catch (IOException e)
            {
                return (e.Message);
            }
        }

        /// <summary>
        /// Clear the log instance.
        /// </summary>
        public static void Reset()
        {
            instance = null;
        }

        /// <summary>
        /// Set a file name as the default name.
        /// </summary>
        /// <param name="name">The name of the file.</param>
        public static void SetFileName(string name)
        {
            defaultFileName = name;
        } 

        /// <summary>
        /// Write a log record.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        public void Write(string message)
        {
            lock (lockObject)
            {
                Write(message, true, true);
            }
        }

        /// <summary>
        /// Write a log record.
        /// </summary>
        /// <param name="message">The message to be logged.</param>
        /// <param name="addNewLine">True if the record requires a newline; false otherwise.</param>
        /// <param name="addHeader">True if the record should have the standard header; false otherwise.</param>
        public void Write(string message, bool addNewLine, bool addHeader)
        {
            if (quietMode)
                return;

            if (consoleOutput)
            {
                Console.Write(message);
                if (addNewLine)
                    Console.WriteLine(string.Empty);
            }

            FileStream fileStream;

            try { fileStream = new FileStream(Path.Combine(RunParameters.DataDirectory, logFileName), FileMode.OpenOrCreate, FileAccess.Write); }
            catch (IOException) { return; }

            string resetMessage = null;

            if (fileStream.Length + message.Length + 13 > maxFileSize)
            {
                fileStream.SetLength(0);
                resetMessage = "Log file reset";
            }
            fileStream.Seek(0, SeekOrigin.End);

            StreamWriter streamWriter = new StreamWriter(fileStream);

            if (resetMessage != null)
            {
                if (addHeader)
                    logHeader(streamWriter, DateTime.Now);

                streamWriter.Write(resetMessage);
                if (addNewLine)
                    streamWriter.WriteLine(string.Empty);
            }

            if (addHeader)
                logHeader(streamWriter, DateTime.Now);

            streamWriter.Write(message);
            if (addNewLine)
                streamWriter.WriteLine(string.Empty);

            streamWriter.Flush();
            streamWriter.Close();
            fileStream.Close();
        }

        /// <summary>
        /// Write a log separator line.
        /// </summary>
        /// <param name="identity">The text of the separator.</param>
        public void WriteSeparator(string identity)
        {
            Write("");
            Write("============================ " + identity + " ==============================");
            Write("");
        }

        private void logHeader(StreamWriter streamWriter, DateTime now)
        {
            streamWriter.Write(now.ToString("HH:mm:ss") + ":" + now.Millisecond.ToString("000") + " ");
        }

        /// <summary>
        /// Log a block of non-ascii data.
        /// </summary>
        /// <param name="identity">The heading for the data.</param>
        /// <param name="buffer">The data to be logged.</param>
        /// <param name="length">The length of the data to be logged.</param>
        public void Dump(string identity, byte[] buffer, int length)
        {
            Dump(identity, buffer, 0, length);
        }

        /// <summary>
        /// Log a block of non-ascii data.
        /// </summary>
        /// <param name="identity">The heading for the data.</param>
        /// <param name="buffer">The data to be logged.</param>
        /// <param name="offset">The index of the byte within the data to begin logging.</param>
        /// <param name="length">The length of the data to be logged.</param>
        public void Dump(string identity, byte[] buffer, int offset, int length)
        {
            Write("============================ " + identity + " starting ==============================");

            StringBuilder loggerLineHex = new StringBuilder("0000 ");
            StringBuilder loggerLineChar = new StringBuilder();
            int lineIndex = 0;

            for (int index = 0; index < length; index++)
            {
                if (lineIndex == 16)
                {
                    Write(loggerLineHex.ToString() + "  " + loggerLineChar.ToString());
                    loggerLineHex.Remove(0, loggerLineHex.Length);
                    loggerLineChar.Remove(0, loggerLineChar.Length);
                    loggerLineHex.Append(index.ToString("0000 "));
                    lineIndex = 0;
                }

                loggerLineHex.Append(getHex(buffer[index + offset] >> 4));
                loggerLineHex.Append(getHex(buffer[index + offset] & 0x0f));
                loggerLineHex.Append(' ');

                if (buffer[index + offset] > ' ' - 1 && buffer[index + offset] < 0x7f)
                    loggerLineChar.Append((char)buffer[index + offset]);
                else
                    loggerLineChar.Append('.');

                lineIndex++;
            }

            if (loggerLineHex.Length != 0)
                Write(loggerLineHex.ToString() + "  " + loggerLineChar.ToString());

            Write("============================ " + identity + " ending ==============================");
        }

        private char getHex(int dataChar)
        {
            if (dataChar < 10)
                return ((char)('0' + dataChar));

            return ((char)('a' + dataChar - 10));
        }

        /// <summary>
        /// Write a history record.
        /// </summary>
        /// <param name="historyRecord">The history record to be logged.</param>
        public static void Write(HistoryRecord historyRecord)
        {
            FileStream fileStream;

            try { fileStream = new FileStream(HistoryFilePath, FileMode.OpenOrCreate, FileAccess.Write); }
            catch (IOException) { return; }

            fileStream.Seek(0, SeekOrigin.End);

            StreamWriter streamWriter = new StreamWriter(fileStream);

            streamWriter.WriteLine(historyRecord.Contents);
            
            streamWriter.Flush();
            streamWriter.Close();
            fileStream.Close();
        }

        /// <summary>
        /// Increase the protocol logging identation.
        /// </summary>
        public static void IncrementProtocolIndent()
        {
            if (protocolLogger == null)
                return;

            protocolIndent = protocolIndent + "    ";
        }

        /// <summary>
        /// Reduce the protocol logging indentation.
        /// </summary>
        public static void DecrementProtocolIndent()
        {
            if (protocolLogger == null)
                return;

            if (protocolIndent.Length > 3)
                protocolIndent = protocolIndent.Remove(protocolIndent.Length - 4);
        }

        /// <summary>
        /// Open the default log file for reading.
        /// </summary>
        /// <returns>True if the file can be opened; false otherwise.</returns>
        public bool Open()
        {
            return (Open(Path.Combine(RunParameters.DataDirectory, defaultFileName)));
        }

        /// <summary>
        /// Open a log file for reading.
        /// </summary>
        /// <param name="fileName">The full name of the file.</param>
        /// <returns>True if the file can be opened; false otherwise.</returns>
        public bool Open(string fileName)
        {
            try
            {
                if (noCreateInput)
                    inputStreamReader = new StreamReader(new FileStream(fileName, FileMode.Open, FileAccess.Read), Encoding.UTF8);
                else
                    inputStreamReader = new StreamReader(new FileStream(fileName, FileMode.OpenOrCreate, FileAccess.Read), Encoding.UTF8);
                return (true);
            }
            catch (IOException)
            {
                return (false);
            }
        }

        /// <summary>
        /// Read a log record.
        /// </summary>
        /// <returns>The log record or null if there are no more records.</returns>
        public string Read()
        {
            return (inputStreamReader.ReadLine());
        }

        /// <summary>
        /// Close the log file.
        /// </summary>
        public void Close()
        {
            inputStreamReader.BaseStream.Close();
            inputStreamReader.Close();            
        }

        /// <summary>
        /// Log an array of Ascii bytes.
        /// </summary>
        /// <param name="title">The title of the array.</param>
        /// <param name="buffer">The block of bytes to log.</param>
        /// <param name="count">The number of bytes to log.</param>
        public static void LogReply(string title, byte[] buffer, int count)
        {
            LogReply(title, new StreamReader(new MemoryStream(buffer, 0, count)));
        }

        /// <summary>
        /// Log a stream of Ascii bytes.
        /// </summary>
        /// <param name="title">The title of the array.</param>
        /// <param name="streamReader">A reader for the stream.</param>
        public static void LogReply(string title, StreamReader streamReader)
        {
            while (!streamReader.EndOfStream)
                Logger.Instance.Write(title + streamReader.ReadLine());

            streamReader.Close();
        }
    }
}
