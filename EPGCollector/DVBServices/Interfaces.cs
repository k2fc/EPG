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

using System.ComponentModel;

using DomainObjects;

namespace DVBServices
{
    /// <summary>
    /// The interface for an EPG collector.
    /// </summary>
    public interface IEPGCollector
    {
        /// <summary>
        /// Get the collection type for this collector.
        /// </summary>
        CollectionType CollectionType { get; }
        /// <summary>
        /// Return true if all data has been processed; false otherwise.
        /// </summary>
        bool AllDataProcessed { get; }

        /// <summary>
        /// Collect all the EPG data.
        /// </summary>
        /// <param name="dataProvider">The object that provides the data samples.</param>
        /// <param name="worker">The BackgroundWorker instance running the collection.</param>        
        /// <returns>A CollectorReply code.</returns>
        CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker);

        /// <summary>
        /// Collect part of the EPG data.
        /// </summary>
        /// <param name="dataProvider">The object that provides the data samples.</param>
        /// <param name="worker">The BackgroundWorker instance running the collection.</param>
        /// <param name="collectionSpan">The amount of data to collect.</param> 
        /// <returns>A CollectorReply code.</returns>
        CollectorReply Process(ISampleDataProvider dataProvider, BackgroundWorker worker, CollectionSpan collectionSpan);

        /// <summary>
        /// Stop the collection.
        /// </summary>
        void Stop();
        
        /// <summary>
        /// Carry out the processing necessary at the end of processing a frequency.
        /// </summary>
        void FinishFrequency();

        /// <summary>
        /// Carry out the processing necessary when all frequencies have been processed.
        /// </summary>
        void FinishRun();
    }

    /// <summary>
    /// The interface for a plugin controller
    /// </summary>
    public interface IPluginController
    {
        /// <summary>
        /// Set a PID to filter samples.
        /// </summary>
        /// <param name="pid">The required Pid.</param>
        void SetPid(int pid);

        /// <summary>
        /// Set a list of PID's to filter samples.
        /// </summary>
        /// <param name="pids">The list of required PID's.</param>
        void SetPids(int[] pids);
        
        /// <summary>
        /// Get the amount of buffer space used.
        /// </summary>
        /// <returns>The number of bytes used.</returns>
        int GetBufferSpaceUsed();

        /// <summary>
        /// Get the current tuning frequency.
        /// </summary>
        /// <returns>The tuning frequency.</returns>
        TuningFrequency GetFrequency();
    }
}
