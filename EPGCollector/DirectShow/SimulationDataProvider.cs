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
using System.Runtime.InteropServices;
using System.IO;
using System.Threading;
using System.ComponentModel;

using DomainObjects;

namespace DirectShow
{
    /// <summary>
    /// The class that provides simulated DVB data.
    /// </summary>
    public class SimulationDataProvider : ISampleDataProvider
    {
        /// <summary>
        /// Get the amount of buffer space currently used by the provider.
        /// </summary>
        public int BufferSpaceUsed { get { return (size); } }

        /// <summary>
        /// Get the address of the buffer used by the provider.
        /// </summary>
        public IntPtr BufferAddress { get { return (memoryBlock); } }

        /// <summary>
        /// Get the frequency the provider is using.
        /// </summary>
        public TuningFrequency Frequency { get { return (tuningFrequency); } }

        /// <summary>
        /// Get the number of sync byte searches.
        /// </summary>
        public int SyncByteSearches { get { return (syncByteSearches); } }
        /// <summary>
        /// Get the number of sync byte searches.
        /// </summary>
        public int SamplesDropped { get { return (samplesDropped); } }
        /// <summary>
        /// Get the maximum sample size.
        /// </summary>
        public int MaximumSampleSize { get { return (0); } }
        /// <summary>
        /// Get the dump file size (not applicable - returns zero).
        /// </summary>
        public int DumpFileSize { get { return (dumpFileSize); } }

        private string fileName;
        private FileStream fileStream;
        
        private string dumpName;
        private FileStream dumpStream;
        private int dumpFileSize;

        private TuningFrequency tuningFrequency;

        private volatile BackgroundWorker backgroundWorker;

        private AutoResetEvent resetEvent = new AutoResetEvent(false);
        private bool running;

        private IntPtr memoryBlock;
        private IntPtr offset;
        private int size;
        private long memorySize = 50 * 1024 * 1024;        

        private const int packetSize = 188;
        private const byte syncByte = 0x47;

        /* DSS values
        private const int packetSize = 131;
        private const byte syncByte = 0x1d;*/

        private Collection<int> pids;
        private Mutex resourceMutex;

        private byte[] sampleBuffer = new byte[packetSize * 512];
        private int sampleBufferLength;
        private int sampleBufferIndex;

        private int syncByteSearches;
        private int samplesDropped;
        private int sampleCount;
        private int filteredSampleCount;
        
        private SimulationDataProvider() { }

        /// <summary>
        /// Initialise a new instance of the SimulationDataProvider class.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="tuningFrequency"></param>
        public SimulationDataProvider(string fileName, TuningFrequency tuningFrequency)
        {
            this.fileName = fileName;
            this.tuningFrequency = tuningFrequency;

            resourceMutex = new Mutex();
        }

        /// <summary>
        /// Initialise a new instance of the SimulationDataProvider class to dump the input stream.
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="tuningFrequency"></param>
        /// <param name="dumpName"></param>
        public SimulationDataProvider(string fileName, TuningFrequency tuningFrequency, string dumpName) : this(fileName, tuningFrequency)
        {
            this.dumpName = dumpName;
        }

        /// <summary>
        /// Start the provider.
        /// </summary>
        public string Run()
        {
            try
            {
                fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 2048, false);
            }
            catch (IOException e)
            {
                return (e.Message);
            }

            memorySize = (fileStream.Length / packetSize) * packetSize;
            if (memorySize > 1024 * 1024 * 1024)
                memorySize = (1024 * 1024 * 1024 / packetSize) * packetSize;
            fileStream.Close();

            memoryBlock = Marshal.AllocHGlobal((int)memorySize);
            Marshal.WriteInt32(memoryBlock, 0);

            offset = new IntPtr(memoryBlock.ToInt64() + 136);

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += new DoWorkEventHandler(workerDoWork);
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerCompleted);
            backgroundWorker.RunWorkerAsync(fileName);

            running = true;

            return (null);
        }

        private void runWorker(string fileName)
        {
            Logger.Instance.Write("Running background worker");

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerSupportsCancellation = true;
            backgroundWorker.DoWork += new DoWorkEventHandler(workerDoWork);
            backgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(workerCompleted);
            backgroundWorker.RunWorkerAsync(fileName);

            running = true;
        }

        private void workerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
                throw new InvalidOperationException("SimulationDataProvider background worker failed - see inner exception", e.Error);

            backgroundWorker = null;
        }

        /// <summary>
        /// Stop the reader.
        /// </summary>
        public void Stop()
        {
            if (running)
            {
                if (backgroundWorker != null)
                {
                    backgroundWorker.CancelAsync();
                    bool reply = resetEvent.WaitOne(new TimeSpan(0, 0, 40));
                }
                running = false;
            }
        }

        private void workerDoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;
            if (worker == null)
                throw (new ArgumentException("Worker thread has been started with an incorrecect sender"));

            string fileName = e.Argument as string;
            if (fileName == null)
                throw (new ArgumentException("Worker thread has been started with an incorrect parameter"));

            if (RunParameters.IsWindows)
                Thread.CurrentThread.Name = "Simulation Data Provider";
            Thread.CurrentThread.Priority = ThreadPriority.Normal;

            Logger.Instance.Write("Simulation Data Provider background worker running");

            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.Read, 2048, false);
            bool eof = false;

            if (dumpName != null)
            {
                try
                {
                    dumpStream = new FileStream(dumpName, FileMode.Create, FileAccess.Write, FileShare.None, 2048, false);
                }
                catch (IOException ex)
                {
                    Logger.Instance.Write("<e> Failed to create dump file");
                    Logger.Instance.Write("<e> " + ex.Message);
                }
            }

            while (!worker.CancellationPending & !eof)
                eof = fillBuffer(fileStream);

            fileStream.Close();

            if (dumpStream != null)
                dumpStream.Close();

            resetEvent.Set();

            Logger.Instance.Write("Background worker finished");
        }

        private bool fillBuffer(FileStream fileStream)
        {
            if (pids == null)
            {
                Thread.Sleep(100);
                return (false);
            }

            if (size + 136 + packetSize > memorySize)
            {
                return (false);
            }

            byte[] buffer = new byte[packetSize];
            int readCount = getPacket(fileStream, buffer);
            if (readCount < packetSize)
                return (readCount == 0);

            sampleCount++;

            TransportPacket transportPacket = new TransportPacket();
            try
            {
                transportPacket.Process(buffer);
            }
            catch (ArgumentOutOfRangeException)
            {
                Logger.Instance.Write("Sync byte error");
                return (false);
            }

            if (TraceEntry.IsDefined(TraceName.PidNumbers))
                Logger.Instance.Write("Simulation Packet for PID " + transportPacket.PID + " received");

            if (TraceEntry.IsDefined(TraceName.Mpeg2Packets))
            {
                if (pids[0] == -1 || pids.Contains(transportPacket.PID))
                    Logger.Instance.Dump("Simulation Packet for PID " + transportPacket.PID, buffer, buffer.Length);                
            }
            
            if (transportPacket.ErrorIndicator)
                return (false);
            if (transportPacket.IsNullPacket)
                return (false);

            bool reply = resourceMutex.WaitOne(5000, true);

            if (pids[0] == -1 || pids.Contains(transportPacket.PID))
            {
                Marshal.Copy(buffer, 0, offset, buffer.Length);

                if (dumpStream != null)
                {
                    dumpStream.Write(buffer, 0, buffer.Length);
                    dumpFileSize += buffer.Length;
                }

                offset+= packetSize;
                size += packetSize;
                Marshal.WriteInt32(memoryBlock, size);
                filteredSampleCount++;
            }

            if (reply)
                resourceMutex.ReleaseMutex();

            return (false);
        }

        private int getPacket(FileStream fileStream, byte[] buffer)
        {
            if (sampleBufferIndex >= sampleBufferLength)
            {
                int readOffset = 0;

                if (sampleBufferLength != 0)
                {
                    if (sampleBufferIndex != sampleBufferLength)
                    {
                        sampleBufferIndex -= packetSize;

                        for (readOffset = 0; sampleBufferIndex < sampleBufferLength; readOffset++)
                        {
                            sampleBuffer[readOffset] = sampleBuffer[sampleBufferIndex];
                            sampleBufferIndex++;
                        }
                    }              
                }

                sampleBufferLength = fileStream.Read(sampleBuffer, readOffset, (packetSize * 512) - readOffset) + readOffset;
                if (sampleBufferLength < packetSize)
                {
                    sampleBufferLength = 0;
                    return (0);
                }
                else
                    sampleBufferIndex = 0;
            }

            if (sampleBuffer[sampleBufferIndex] != syncByte)
            {
                Logger.Instance.Write("Searching for sync byte");
                syncByteSearches++;

                bool found = false;

                for (sampleBufferIndex++; sampleBufferIndex < packetSize && !found; sampleBufferIndex++)
                {
                    if (sampleBuffer[sampleBufferIndex] == syncByte)
                    {
                        found = true;

                        for (int restIndex = packetSize; restIndex + sampleBufferIndex < sampleBufferLength && found; restIndex += packetSize)
                        {
                            if (sampleBuffer[sampleBufferIndex + restIndex] != syncByte)
                                found = false;
                        }
                    }
                }

                if (!found)
                {
                    Logger.Instance.Write("No sync byte - sample dropped");
                    samplesDropped++;
                    sampleBufferLength = 0;
                    return (-1);
                }
                else
                    sampleBufferIndex--;
            }

            if (sampleBufferIndex + packetSize > sampleBuffer.Length)
            {
                sampleBufferIndex += packetSize;
                return (-1);
            }

            for (int index = 0; index < packetSize; index++)
                buffer[index] = sampleBuffer[sampleBufferIndex + index];

            sampleBufferIndex += packetSize;

            return (packetSize);
        }

        /// <summary>
        /// Change the PID mappings.
        /// </summary>
        /// <param name="newPid">The new PID to be set.</param>
        public void ChangePidMapping(int newPid)
        {
            ChangePidMapping(new int[] { newPid });
        }

        /// <summary>
        /// Change the PID mappings.
        /// </summary>
        /// <param name="newPids">A list of the new PID's to be set.</param>
        public void ChangePidMapping(int[] newPids)
        {
            Stop();

            while (backgroundWorker != null)
                Thread.Sleep(1000);

            bool reply = resourceMutex.WaitOne(5000, true);

            if (pids == null)
                pids = new Collection<int>();

            pids.Clear();

            foreach (int newPid in newPids)
                pids.Add(newPid);

            offset = new IntPtr(memoryBlock.ToInt64() + 136);
            Marshal.WriteInt32(memoryBlock, 0);
            size = 0;

            Logger.Instance.Write("Changed PID mapping");

            if (reply)
                resourceMutex.ReleaseMutex();

            runWorker(fileName);
        }
    }
}
