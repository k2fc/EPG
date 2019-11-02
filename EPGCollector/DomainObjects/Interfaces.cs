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

namespace DomainObjects
{
    /// <summary>
    /// The interface for obtaining samples from the input stream.
    /// </summary>
    public interface ITunerDataProvider
    {
        /// <summary>
        /// Return true if signal present; false otherwise.
        /// </summary>
        bool SignalPresent { get; }
        /// <summary>
        /// Return true if the signal is locked; false otherwise.
        /// </summary>
        bool SignalLocked { get; }
        /// <summary>
        /// Get the signal quality.
        /// </summary>
        int SignalQuality { get; }
        /// <summary>
        /// Get the signal strength.
        /// </summary>
        int SignalStrength { get; }
        /// <summary>
        /// Get the current tuner object.
        /// </summary>
        Tuner Tuner { get; }
        /// <summary>
        /// Dispose of the data provider.
        /// </summary>
        void Dispose();
        
    }

    /// <summary>
    /// The interface for obtaining samples from the input stream.
    /// </summary>
    public interface ISampleDataProvider
    {
        /// <summary>
        /// Get the amount of buffer space used in bytes
        /// </summary>
        int BufferSpaceUsed { get; }
        /// <summary>
        /// Get the address of the buffer.
        /// </summary>
        IntPtr BufferAddress { get; }
        /// <summary>
        /// Get the current frequency.
        /// </summary>
        TuningFrequency Frequency { get; }

        /// <summary>
        /// Change the PID mapping.
        /// </summary>
        /// <param name="pid">The PID to be mapped.</param>
        void ChangePidMapping(int pid);
        /// <summary>
        /// Change the PID mapping.
        /// </summary>
        /// <param name="pids">A list of PID's to be mapped.</param>
        void ChangePidMapping(int[] pids);

        /// <summary>
        /// Get the number of sync byte searches
        /// </summary>
        int SyncByteSearches { get; }
        /// <summary>
        /// Get the number of samples dropped
        /// </summary>
        int SamplesDropped { get; }
        /// <summary>
        /// Get the maximum sample size
        /// </summary>
        int MaximumSampleSize { get; }
        /// <summary>
        /// Get the dump file size
        /// </summary>
        int DumpFileSize { get; }
    }

    /// <summary>
    /// The interface for an event logger.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Write a log record.
        /// </summary>
        /// <param name="logData">The data to be written.</param>
        void Write(string logData);
    }
}
