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
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Security;
using System.IO;

using DirectShowAPI;

using DomainObjects;

namespace DirectShow
{
    /// <summary>
    /// The class the supports a BDA DirectShow graph.
    /// </summary>
    public class BDAGraph : DirectShowGraph, ITunerDataProvider, ISampleDataProvider
    {
        /// <summary>
        /// Create the custom PSI memory filter (32-bit).
        /// </summary>
        /// <param name="graphBuilder">The graph builder instance.</param>
        /// <param name="logging">True to enable logging.</param>
        /// <param name="logFileName">The name of the log file or null.</param>
        /// <param name="dumping">True to enable data dumping.</param>
        /// <param name="dumpFileName">The name of the dump file or null.</param>
        /// <param name="bufferSize">The size of the data buffer.</param>
        /// <returns>Zero if successful; a COM error code otherwise.</returns>
        [DllImport("PSIMemoryShare.dll", ExactSpelling = true, CallingConvention = CallingConvention.Cdecl),
            SuppressUnmanagedCodeSecurity]
        public static extern int CreatePSIMemoryFilter(IGraphBuilder graphBuilder,
            bool logging,
            [MarshalAs(UnmanagedType.LPStr)]string logFileName,
            bool dumping,
            [MarshalAs(UnmanagedType.LPStr)]string dumpFileName,
            int bufferSize);

        /// <summary>
        /// The PID of the Program Association table.
        /// </summary>
        public const short PatPid = 0x00;
        /// <summary>
        /// The PID of the Network Information table.
        /// </summary>
        public const short NitPid = 0x10;
        /// <summary>
        /// The PID of the Service Description table.
        /// </summary>
        public const short SdtPid = 0x11;
        /// <summary>
        /// The PID of the Event Information table.
        /// </summary>
        public const short EitPid = 0x12;

        /// <summary>
        /// The table number of the Program Association table.
        /// </summary>
        public const byte PatTable = 0x00;
        /// <summary>
        /// The table number of the Program Map table.
        /// </summary>
        public const byte PmtTable = 0x02;
        /// <summary>
        /// The table number of the Service Description table.
        /// </summary>
        public const byte SdtTable = 0x42;
        /// <summary>
        /// The table number of the Service Description table for the 'other' transport stream.
        /// </summary>
        public const byte SdtOtherTable = 0x46;
        /// <summary>
        /// The table number of the now/next table for the Event Information table for the 'current' transport stream.
        /// </summary>
        public const byte EitTable = 0x4e;

        /// <summary>
        /// Get the frequency the graph is currently tuned to.
        /// </summary>
        public TuningFrequency Frequency { get { return (tuningSpec.Frequency); } }

        /// <summary>
        /// Get the current signal strength.
        /// </summary>
        public int SignalStrength
        {
            get
            {
                if (networkProviderFilter == null)
                    return (0);

                int signalStrength;

                int reply = ((ITuner)networkProviderFilter).get_SignalStrength(out signalStrength);
                DsError.ThrowExceptionForHR(reply);

                return (signalStrength);
            }
        }

        /// <summary>
        /// Get the current signal quality.
        /// </summary>
        public int SignalQuality
        {
            get
            {
                if (tunerFilter == null)
                    return (0);

                Collection<IBDA_SignalStatistics> signalStatisticsCollection = getSignalStatisticsInterfaces("signal quality");
                if (signalStatisticsCollection != null)
                {
                    foreach (IBDA_SignalStatistics signalStatistics in signalStatisticsCollection)
                    {
                        int signalQuality;
                        reply = signalStatistics.get_SignalQuality(out signalQuality);
                        if (reply >= 0)
                        {
                            releaseSignalStatisticsInterfaces(signalStatisticsCollection);
                            if (TraceEntry.IsDefined(TraceName.BdaSigStats))
                                Logger.Instance.Write("BDA Signal quality returned from signal stats interface " + (signalStatisticsCollection.IndexOf(signalStatistics) + 1));
                            return (signalQuality);
                        }
                    }

                    releaseSignalStatisticsInterfaces(signalStatisticsCollection);
                }

                if (TraceEntry.IsDefined(TraceName.BdaSigStats))
                    Logger.Instance.Write("BDA Signal quality not found on any signal stats interface - returning -1");

                return (-1);
            }
        }

        /// <summary>
        /// Return true if there is currently a signal present; false otherwise.
        /// </summary>
        public bool SignalPresent
        {
            get
            {
                if (tunerFilter == null)
                    return (false);

                Collection<IBDA_SignalStatistics> signalStatisticsCollection = getSignalStatisticsInterfaces("signal present");
                if (signalStatisticsCollection != null)
                {
                    foreach (IBDA_SignalStatistics signalStatistics in signalStatisticsCollection)
                    {
                        bool signalPresent;
                        reply = signalStatistics.get_SignalPresent(out signalPresent);
                        Marshal.ReleaseComObject(signalStatistics);
                        if (reply >= 0 && signalPresent)
                        {
                            if (TraceEntry.IsDefined(TraceName.BdaSigStats))
                                Logger.Instance.Write("BDA Signal present returned from signal stats interface " + (signalStatisticsCollection.IndexOf(signalStatistics) + 1));
                            releaseSignalStatisticsInterfaces(signalStatisticsCollection);
                            return (true);
                        }
                    }

                    if (TraceEntry.IsDefined(TraceName.BdaSigStats))
                        Logger.Instance.Write("BDA Signal not present on any signal stats interface - returning false");
                    releaseSignalStatisticsInterfaces(signalStatisticsCollection);
                    return (false);
                }

                if (TraceEntry.IsDefined(TraceName.BdaSigStats))
                    Logger.Instance.Write("BDA No signal stats interface available to determine signal present - returning false");

                return (false);
            }
        }

        /// <summary>
        /// Return true if the signal is currently locked; false otherwise.
        /// </summary>
        public bool SignalLocked
        {
            get
            {
                if (tunerFilter == null)
                    return (false);

                Collection<IBDA_SignalStatistics> signalStatisticsCollection = getSignalStatisticsInterfaces("signal locked");
                if (signalStatisticsCollection != null)
                {
                    foreach (IBDA_SignalStatistics signalStatistics in signalStatisticsCollection)
                    {
                        bool signalLocked;
                        reply = signalStatistics.get_SignalLocked(out signalLocked);
                        if (reply >= 0 && signalLocked)
                        {
                            if (TraceEntry.IsDefined(TraceName.BdaSigStats))
                                Logger.Instance.Write("BDA Signal locked returned from signal stats interface " + (signalStatisticsCollection.IndexOf(signalStatistics) + 1));
                            releaseSignalStatisticsInterfaces(signalStatisticsCollection);
                            return (true);
                        }
                    }

                    if (TraceEntry.IsDefined(TraceName.BdaSigStats))
                        Logger.Instance.Write("BDA Signal not locked on any signal stats interface - returning false");
                    releaseSignalStatisticsInterfaces(signalStatisticsCollection);
                    return (false);
                }

                if (TraceEntry.IsDefined(TraceName.BdaSigStats))
                    Logger.Instance.Write("BDA No signal stats interface available to determine signal lock - returning false");

                return (false);
            }
        }

        /// <summary>
        /// Return true if data is currently flowing through the graph; false otherwise.
        /// </summary>
        public bool DataFlowing
        {
            get
            {
                if (psiMemoryFilter == null)
                    return (false);

                bool isDataFlowing;
                ((IMemSinkSettings)psiMemoryFilter).get_IsDataFlowing(out isDataFlowing);
                return (isDataFlowing);
            }
        }

        /// <summary>
        /// Get the amount of buffer space currently used by the custom PSI memory filter.
        /// </summary>
        public int BufferSpaceUsed
        {
            get
            {
                if (psiMemoryFilter == null)
                    return (0);

                int bufferUsed;
                ((IMemSinkSettings)psiMemoryFilter).get_BufferUsed(out bufferUsed);
                return (bufferUsed);
            }
        }

        /// <summary>
        /// Get the no of sync byte searches used by the custom PSI memory filter.
        /// </summary>
        public int SyncByteSearches
        {
            get
            {
                if (psiMemoryFilter == null)
                    return (0);

                int syncByteSearches;
                ((IMemSinkSettings)psiMemoryFilter).get_SyncByteSearchCount(out syncByteSearches);
                return (syncByteSearches);
            }
        }

        /// <summary>
        /// Get the no of samples dropped by the custom PSI memory filter.
        /// </summary>
        public int SamplesDropped
        {
            get
            {
                if (psiMemoryFilter == null)
                    return (0);

                int samplesDropped;
                ((IMemSinkSettings)psiMemoryFilter).get_SamplesDropped(out samplesDropped);
                return (samplesDropped);
            }
        }

        /// <summary>
        /// Get the maximum sample size received by the custom PSI memory filter.
        /// </summary>
        public int MaximumSampleSize
        {
            get
            {
                if (psiMemoryFilter == null)
                    return (0);

                int maximumSampleSize;
                ((IMemSinkSettings)psiMemoryFilter).get_MaximumSampleSize(out maximumSampleSize);
                return (maximumSampleSize);
            }
        }

        /// <summary>
        /// Get the file size of the dump file created by the custom PSI memory filter.
        /// </summary>
        public int DumpFileSize
        {
            get
            {
                if (psiMemoryFilter == null)
                    return (0);

                int dumpFileSize;
                ((IMemSinkSettings)psiMemoryFilter).get_DumpFileSize(out dumpFileSize);
                return (dumpFileSize);
            }
        }

        /// <summary>
        /// Get the address of the buffer used by the custom PSI memory filter.
        /// </summary>
        public IntPtr BufferAddress
        {
            get
            {
                if (psiMemoryFilter == null)
                    return (new IntPtr(0));

                int bufferAddress;
                ((IMemSinkSettings)psiMemoryFilter).get_BufferAddress(out bufferAddress);
                return (new IntPtr(bufferAddress));
            }
        }

        /// <summary>
        /// Get the current graph instance.
        /// </summary>
        public static BDAGraph CurrentGraph { get { return (currentGraph); } }

        /// <summary>
        /// Get the current tuner.
        /// </summary>
        public Tuner Tuner { get { return (tunerSpec); } }

        private ITuningSpace tuningSpace = null;
        private ITuneRequest tuneRequest = null;

        private IBaseFilter networkProviderFilter = null;
        private IBaseFilter tunerFilter = null;
        private IBaseFilter mpeg2DemuxFilter = null;
        private IBaseFilter tifFilter = null;
        private IBaseFilter secTabFilter = null;
        private IBaseFilter infiniteTeeFilter = null;
        private IBaseFilter psiMemoryFilter = null;

        private Collection<IBaseFilter> receiverFilters;

        private ILocator currentLocator;

        private DVBS2HandlerBase dvbs2Handler;

        private TuningSpec tuningSpec;
        private Tuner tunerSpec;

        private Collection<int> psiPids = new Collection<int>();

        private string dumpFileName;

        private int reply;

        private static BDAGraph currentGraph;

        /// <summary>
        /// Initialize a new instance of the BDAGraph1 class.
        /// </summary>
        /// <param name="componentName">The name of the current component.</param>
        /// <param name="tuningSpec">A tuning spec instance with details of the tuning requirements.</param>
        /// <param name="tunerSpec">A tuner spec instance specifying the tuner to be used.</param>
        public BDAGraph(string componentName, TuningSpec tuningSpec, Tuner tunerSpec) : base(componentName)
        {
            currentGraph = this;

            this.tuningSpec = tuningSpec;
            this.tunerSpec = tunerSpec;

            createTuningSpace(tuningSpec);
            createTuneRequest(tuningSpace, tuningSpec);

            buildGraph(tuningSpace, tunerSpec, tuningSpec);
        }

        /// <summary>
        /// Initialize a new instance of the BDAGraph1 class for dumping a transport stream.
        /// </summary>
        /// <param name="componentName">The name of the current component.</param>
        /// <param name="tuningSpec">A tuning spec instance with details of the tuning requirements.</param>
        /// <param name="tunerSpec">A tuner spec instance specifying the tuner to be used.</param>
        /// <param name="dumpFileName">The name of the dump file.</param>
        public BDAGraph(string componentName, TuningSpec tuningSpec, Tuner tunerSpec, string dumpFileName) : base(componentName)
        {
            currentGraph = this;

            this.tuningSpec = tuningSpec;
            this.tunerSpec = tunerSpec;

            createTuningSpace(tuningSpec);
            createTuneRequest(tuningSpace, tuningSpec);

            this.dumpFileName = dumpFileName;

            buildGraph(tuningSpace, tunerSpec, tuningSpec);
        }

        private void createTuningSpace(TuningSpec tuningSpec)
        {
            switch (tuningSpec.Frequency.TunerType)
            {
                case TunerType.Satellite:
                    {
                        LogMessage("Creating DVB Satellite tuning space");

                        tuningSpace = (ITuningSpace)new DVBSTuningSpace();

                        reply = tuningSpace.put_UniqueName("EPG Collector DVB-S Tuning Space");
                        DsError.ThrowExceptionForHR(reply);

                        reply = tuningSpace.put_FriendlyName("EPG Collector DVB-S Tuning Space");
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDVBSTuningSpace)tuningSpace).put_HighOscillator(((SatelliteFrequency)tuningSpec.Frequency).SatelliteDish.LNBHighBandFrequency);
                        DsError.ThrowExceptionForHR(reply);

                        if (((SatelliteFrequency)tuningSpec.Frequency).SatelliteDish.LNBSwitchFrequency != 0)
                            reply = ((IDVBSTuningSpace)tuningSpace).put_LNBSwitch(((SatelliteFrequency)tuningSpec.Frequency).SatelliteDish.LNBSwitchFrequency);
                        else
                            reply = ((IDVBSTuningSpace)tuningSpace).put_LNBSwitch(20000000);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDVBSTuningSpace)tuningSpace).put_LowOscillator(((SatelliteFrequency)tuningSpec.Frequency).SatelliteDish.LNBLowBandFrequency);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDVBSTuningSpace)tuningSpace).put_NetworkID(-1);
                        DsError.ThrowExceptionForHR(reply);

                        reply = tuningSpace.put__NetworkType(new Guid("fa4b375a-45b4-4d45-8440-263957b11623"));
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDVBSTuningSpace)tuningSpace).put_SpectralInversion(SpectralInversion.NotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDVBTuningSpace)tuningSpace).put_SystemType(DVBSystemType.Satellite);
                        DsError.ThrowExceptionForHR(reply);

                        LogMessage("Tuning Space LNB Low: " + ((SatelliteFrequency)tuningSpec.Frequency).SatelliteDish.LNBLowBandFrequency.ToString());
                        LogMessage("Tuning Space LNB High: " + ((SatelliteFrequency)tuningSpec.Frequency).SatelliteDish.LNBHighBandFrequency.ToString());
                        LogMessage("Tuning Space LNB Switch: " + ((SatelliteFrequency)tuningSpec.Frequency).SatelliteDish.LNBSwitchFrequency.ToString());

                        reply = ((IDVBSTuningSpace)tuningSpace).put_DefaultLocator(getLocator(tuningSpec, false));
                        DsError.ThrowExceptionForHR(reply);

                        break;
                    }
                case TunerType.Terrestrial:
                    {
                        LogMessage("Creating DVB Terrestrial tuning space");

                        tuningSpace = (IDVBTuningSpace)new DVBTuningSpace();

                        reply = tuningSpace.put_UniqueName("EPG Collector DVB-T Tuning Space");
                        DsError.ThrowExceptionForHR(reply);

                        reply = tuningSpace.put_FriendlyName("EPG Collector DVB-T Tuning Space");
                        DsError.ThrowExceptionForHR(reply);

                        reply = tuningSpace.put__NetworkType(new Guid("216c62df-6d7f-4e9a-8571-05f14edb766a"));
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDVBTuningSpace)tuningSpace).put_SystemType(DVBSystemType.Terrestrial);
                        DsError.ThrowExceptionForHR(reply);

                        reply = tuningSpace.put_DefaultLocator(getLocator(tuningSpec, false));
                        DsError.ThrowExceptionForHR(reply);

                        break;
                    }
                case TunerType.Cable:
                    {
                        LogMessage("Creating DVB Cable tuning space");

                        tuningSpace = (IDVBTuningSpace)new DVBTuningSpace();

                        reply = tuningSpace.put_UniqueName("EPG Collector DVB-C Tuning Space");
                        DsError.ThrowExceptionForHR(reply);

                        reply = tuningSpace.put_FriendlyName("EPG Collector DVB-C Tuning Space");
                        DsError.ThrowExceptionForHR(reply);

                        reply = tuningSpace.put__NetworkType(typeof(DVBCNetworkProvider).GUID);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDVBTuningSpace)tuningSpace).put_SystemType(DVBSystemType.Cable);
                        DsError.ThrowExceptionForHR(reply);

                        reply = tuningSpace.put_DefaultLocator(getLocator(tuningSpec, false));
                        DsError.ThrowExceptionForHR(reply);

                        break;
                    }
                case TunerType.ATSC:
                    {
                        LogMessage("Creating ATSC Terrestrial tuning space");

                        tuningSpace = (IATSCTuningSpace)new ATSCTuningSpace();

                        reply = tuningSpace.put_UniqueName("EPG Collector ATSC Terrestrial Tuning Space");
                        DsError.ThrowExceptionForHR(reply);

                        reply = tuningSpace.put_FriendlyName("EPG Collector ATSC Terrestrial Tuning Space");
                        DsError.ThrowExceptionForHR(reply);

                        reply = tuningSpace.put__NetworkType(typeof(ATSCNetworkProvider).GUID);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IATSCTuningSpace)tuningSpace).put_InputType(TunerInputType.Antenna);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IATSCTuningSpace)tuningSpace).put_MaxMinorChannel(999);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IATSCTuningSpace)tuningSpace).put_MaxPhysicalChannel(69);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IATSCTuningSpace)tuningSpace).put_MaxChannel(99);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IATSCTuningSpace)tuningSpace).put_MinMinorChannel(-1);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IATSCTuningSpace)tuningSpace).put_MinPhysicalChannel(-1);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IATSCTuningSpace)tuningSpace).put_MinChannel(-1);
                        DsError.ThrowExceptionForHR(reply);

                        reply = tuningSpace.put_DefaultLocator(getLocator(tuningSpec, false));
                        DsError.ThrowExceptionForHR(reply);

                        LogMessage("Tuning Space Max Physical Channel: 69");


                        break;
                    }
                case TunerType.ATSCCable:
                    {
                        LogMessage("Creating ATSC Cable tuning space");

                        tuningSpace = (IATSCTuningSpace)new ATSCTuningSpace();

                        reply = tuningSpace.put_UniqueName("EPG Collector ATSC Cable Tuning Space");
                        DsError.ThrowExceptionForHR(reply);

                        reply = tuningSpace.put_FriendlyName("EPG Collector ATSC Cable Tuning Space");
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IATSCTuningSpace)tuningSpace).put_InputType(TunerInputType.Cable);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IATSCTuningSpace)tuningSpace).put_MaxMinorChannel(999);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IATSCTuningSpace)tuningSpace).put_MaxPhysicalChannel(158);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IATSCTuningSpace)tuningSpace).put_MaxChannel(9999);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IATSCTuningSpace)tuningSpace).put_MinMinorChannel(-1);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IATSCTuningSpace)tuningSpace).put_MinPhysicalChannel(-1);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IATSCTuningSpace)tuningSpace).put_MinChannel(-1);
                        DsError.ThrowExceptionForHR(reply);

                        reply = tuningSpace.put__NetworkType(typeof(ATSCNetworkProvider).GUID);
                        DsError.ThrowExceptionForHR(reply);

                        reply = tuningSpace.put_DefaultLocator(getLocator(tuningSpec, false));
                        DsError.ThrowExceptionForHR(reply);

                        LogMessage("Tuning Space Max Physical Channel: 158");

                        break;
                    }
                case TunerType.ClearQAM:
                    {
                        LogMessage("Creating Clear QAM tuning space");

                        tuningSpace = (IDigitalCableTuningSpace)new DigitalCableTuningSpace();

                        reply = tuningSpace.put_UniqueName("EPG Collector Clear QAM Tuning Space");
                        DsError.ThrowExceptionForHR(reply);

                        reply = tuningSpace.put_FriendlyName("EPG Collector Clear QAM Tuning Space");
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDigitalCableTuningSpace)tuningSpace).put_InputType(TunerInputType.Cable);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDigitalCableTuningSpace)tuningSpace).put_MaxChannel(9999);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDigitalCableTuningSpace)tuningSpace).put_MaxMajorChannel(99);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDigitalCableTuningSpace)tuningSpace).put_MaxMinorChannel(999);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDigitalCableTuningSpace)tuningSpace).put_MaxPhysicalChannel(158);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDigitalCableTuningSpace)tuningSpace).put_MaxSourceID(0x7fffffff);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDigitalCableTuningSpace)tuningSpace).put_MinChannel(-1);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDigitalCableTuningSpace)tuningSpace).put_MinMajorChannel(-1);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDigitalCableTuningSpace)tuningSpace).put_MinMinorChannel(-1);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDigitalCableTuningSpace)tuningSpace).put_MinPhysicalChannel(2);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDigitalCableTuningSpace)tuningSpace).put_MinSourceID(0);
                        DsError.ThrowExceptionForHR(reply);

                        reply = tuningSpace.put__NetworkType(new Guid("143827ab-f77b-498d-81ca-5a007aec28bf"));
                        DsError.ThrowExceptionForHR(reply);

                        reply = tuningSpace.put_DefaultLocator(getLocator(tuningSpec, false));
                        DsError.ThrowExceptionForHR(reply);

                        LogMessage("Tuning Space Max Physical Channel: 158");

                        break;
                    }
                case TunerType.ISDBS:
                    {
                        LogMessage("Creating ISDB Satellite tuning space");

                        tuningSpace = (ITuningSpace)new DVBSTuningSpace();

                        reply = tuningSpace.put_UniqueName("EPG Collector ISDB-S Tuning Space");
                        DsError.ThrowExceptionForHR(reply);

                        reply = tuningSpace.put_FriendlyName("EPG Collector ISDB-S Tuning Space");
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDVBSTuningSpace)tuningSpace).put_HighOscillator(((ISDBSatelliteFrequency)tuningSpec.Frequency).SatelliteDish.LNBHighBandFrequency);
                        DsError.ThrowExceptionForHR(reply);

                        if (((ISDBSatelliteFrequency)tuningSpec.Frequency).SatelliteDish.LNBSwitchFrequency != 0)
                            reply = ((IDVBSTuningSpace)tuningSpace).put_LNBSwitch(((SatelliteFrequency)tuningSpec.Frequency).SatelliteDish.LNBSwitchFrequency);
                        else
                            reply = ((IDVBSTuningSpace)tuningSpace).put_LNBSwitch(20000000);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDVBSTuningSpace)tuningSpace).put_LowOscillator(((ISDBSatelliteFrequency)tuningSpec.Frequency).SatelliteDish.LNBLowBandFrequency);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDVBSTuningSpace)tuningSpace).put_NetworkID(-1);
                        DsError.ThrowExceptionForHR(reply);

                        reply = tuningSpace.put__NetworkType(new Guid("b0a4e6a0-6a1a-4b83-bb5b-903e1d90e6b6"));
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDVBSTuningSpace)tuningSpace).put_SpectralInversion(SpectralInversion.NotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDVBTuningSpace)tuningSpace).put_SystemType(DVBSystemType.ISDBS);
                        DsError.ThrowExceptionForHR(reply);

                        LogMessage("Tuning Space LNB Low: " + ((ISDBSatelliteFrequency)tuningSpec.Frequency).SatelliteDish.LNBLowBandFrequency.ToString());
                        LogMessage("Tuning Space LNB High: " + ((ISDBSatelliteFrequency)tuningSpec.Frequency).SatelliteDish.LNBHighBandFrequency.ToString());
                        LogMessage("Tuning Space LNB Switch: " + ((ISDBSatelliteFrequency)tuningSpec.Frequency).SatelliteDish.LNBSwitchFrequency.ToString());

                        reply = ((IDVBSTuningSpace)tuningSpace).put_DefaultLocator(getLocator(tuningSpec, false));
                        DsError.ThrowExceptionForHR(reply);

                        break;
                    }
                case TunerType.ISDBT:
                    {
                        LogMessage("Creating ISDB Terrestrial tuning space");

                        tuningSpace = (IDVBTuningSpace)new DVBTuningSpace();

                        reply = tuningSpace.put_UniqueName("EPG Collector ISDB-T Tuning Space");
                        DsError.ThrowExceptionForHR(reply);

                        reply = tuningSpace.put_FriendlyName("EPG Collector ISDB-T Tuning Space");
                        DsError.ThrowExceptionForHR(reply);

                        reply = tuningSpace.put__NetworkType(new Guid("95037f6f-3ac7-4452-b6c4-45a9ce9292a2"));
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDVBTuningSpace)tuningSpace).put_SystemType(DVBSystemType.ISDBT);
                        DsError.ThrowExceptionForHR(reply);

                        reply = tuningSpace.put_DefaultLocator(getLocator(tuningSpec, false));
                        DsError.ThrowExceptionForHR(reply);

                        break;
                    }
            }
        }

        private void createTuneRequest(ITuningSpace tuningSpace, TuningSpec tuningSpec)
        {
            reply = tuningSpace.CreateTuneRequest(out tuneRequest);
            DsError.ThrowExceptionForHR(reply);

            if (tuningSpec.Frequency.TunerType == TunerType.Satellite ||
                tuningSpec.Frequency.TunerType == TunerType.Terrestrial ||
                tuningSpec.Frequency.TunerType == TunerType.Cable)
            {
                LogMessage("Creating DVB tune request");

                reply = ((IDVBTuneRequest)tuneRequest).put_ONID(-1);
                DsError.ThrowExceptionForHR(reply);

                reply = ((IDVBTuneRequest)tuneRequest).put_TSID(-1);
                DsError.ThrowExceptionForHR(reply);

                reply = ((IDVBTuneRequest)tuneRequest).put_SID(-1);
                DsError.ThrowExceptionForHR(reply);
            }
            else
            {
                if (tuningSpec.Frequency.TunerType == TunerType.ATSC || tuningSpec.Frequency.TunerType == TunerType.ATSCCable)
                {
                    LogMessage("Creating ATSC tune request");

                    reply = ((IATSCChannelTuneRequest)tuneRequest).put_Channel(-1);
                    DsError.ThrowExceptionForHR(reply);

                    reply = ((IATSCChannelTuneRequest)tuneRequest).put_MinorChannel(-1);
                    DsError.ThrowExceptionForHR(reply);
                }
                else
                {
                    if (tuningSpec.Frequency.TunerType == TunerType.ClearQAM)
                    {
                        LogMessage("Creating Clear QAM tune request");

                        reply = ((IDigitalCableTuneRequest)tuneRequest).put_Channel(-1);
                        DsError.ThrowExceptionForHR(reply);

                        reply = ((IDigitalCableTuneRequest)tuneRequest).put_MinorChannel(-1);
                        DsError.ThrowExceptionForHR(reply);
                    }
                    else
                    {
                        if (tuningSpec.Frequency.TunerType == TunerType.ISDBS ||
                            tuningSpec.Frequency.TunerType == TunerType.ISDBT)
                        {
                            LogMessage("Creating ISDB tune request");

                            reply = ((IDVBTuneRequest)tuneRequest).put_ONID(-1);
                            DsError.ThrowExceptionForHR(reply);

                            reply = ((IDVBTuneRequest)tuneRequest).put_TSID(-1);
                            DsError.ThrowExceptionForHR(reply);

                            reply = ((IDVBTuneRequest)tuneRequest).put_SID(-1);
                            DsError.ThrowExceptionForHR(reply);
                        }
                    }
                }
            }

            reply = tuneRequest.put_Locator(getLocator(tuningSpec, true));
            DsError.ThrowExceptionForHR(reply);
        }

        private ILocator getLocator(TuningSpec tuningSpec, bool logSettings)
        {
            switch (tuningSpec.Frequency.TunerType)
            {
                case TunerType.Satellite:
                    {
                        LogMessage("Creating DVB Satellite locator");

                        IDVBSLocator locator = (IDVBSLocator)new DVBSLocator();
                        SatelliteFrequency satelliteFrequency = tuningSpec.Frequency as SatelliteFrequency;

                        bool conversionNeeded = satelliteFrequency.LNBConversion;
                        if (conversionNeeded)
                        {
                            conversionNeeded = satelliteFrequency.SatelliteDish.LNBType.Type != LNBType.Legacy;

                            if (conversionNeeded)
                            {
                                conversionNeeded = (tuningSpec.SignalPolarization.Polarization == SignalPolarization.CircularLeft) ||
                                    (tuningSpec.SignalPolarization.Polarization == SignalPolarization.LinearHorizontal);
                            }
                        }

                        string frequencyConversionString = string.Empty;
                        string polarizationConversionString = string.Empty;                        

                        reply = locator.put_Azimuth(-1);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_Elevation(-1);
                        DsError.ThrowExceptionForHR(reply);

                        if (!conversionNeeded)
                            reply = locator.put_CarrierFrequency(tuningSpec.Frequency.Frequency);
                        else
                        {
                            int adjustedFrequency;

                            switch (satelliteFrequency.SatelliteDish.LNBType.Type)
                            {
                                case LNBType.DishProDigitalService:
                                    adjustedFrequency = 25600000 - tuningSpec.Frequency.Frequency;
                                    reply = locator.put_CarrierFrequency(adjustedFrequency);
                                    frequencyConversionString = "/" + tuningSpec.Frequency.Frequency + " (converted for DSS)";
                                    break;
                                case LNBType.DishProFixedService:
                                    adjustedFrequency = 24600000 - tuningSpec.Frequency.Frequency;
                                    reply = locator.put_CarrierFrequency(adjustedFrequency);
                                    frequencyConversionString = "/" + tuningSpec.Frequency.Frequency + " (converted for FSS)";
                                    break;
                                default:
                                    reply = locator.put_CarrierFrequency(tuningSpec.Frequency.Frequency);
                                    break;
                            }
                        }
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_InnerFEC(FECMethod.Viterbi);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_InnerFECRate(Utils.GetNativeFECRate(tuningSpec.FECRate));
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_Modulation(Utils.GetNativeModulation(tuningSpec.Modulation));
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_OrbitalPosition(tuningSpec.Satellite.Longitude);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_OuterFEC(FECMethod.MethodNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        if (!conversionNeeded)
                            reply = locator.put_SignalPolarisation(Utils.GetNativePolarization(tuningSpec.SignalPolarization));
                        else
                        {
                            switch (satelliteFrequency.SatelliteDish.LNBType.Type)
                            {
                                case LNBType.DishProDigitalService:
                                    if (tuningSpec.SignalPolarization.Polarization == SignalPolarization.CircularLeft ||
                                        tuningSpec.SignalPolarization.Polarization == SignalPolarization.LinearHorizontal)
                                    {
                                        reply = locator.put_SignalPolarisation(Utils.GetNativePolarization(SignalPolarization.LinearVertical));
                                        polarizationConversionString = "/" + tuningSpec.SignalPolarization.Polarization + " (converted for DSS)";
                                    }
                                    else
                                        reply = locator.put_SignalPolarisation(Utils.GetNativePolarization(tuningSpec.SignalPolarization));
                                    break;
                                case LNBType.DishProFixedService:
                                    if (tuningSpec.SignalPolarization.Polarization == SignalPolarization.CircularLeft ||
                                        tuningSpec.SignalPolarization.Polarization == SignalPolarization.LinearHorizontal)
                                    {
                                        reply = locator.put_SignalPolarisation(Utils.GetNativePolarization(SignalPolarization.LinearVertical));
                                        polarizationConversionString = "/" + tuningSpec.SignalPolarization.Polarization + " (converted for FSS)";
                                    }
                                    else
                                        reply = locator.put_SignalPolarisation(Utils.GetNativePolarization(tuningSpec.SignalPolarization));
                                    break;
                                default:
                                    reply = locator.put_SignalPolarisation(Utils.GetNativePolarization(tuningSpec.SignalPolarization));
                                    break;
                            }
                        }
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_SymbolRate(tuningSpec.SymbolRate);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_WestPosition(tuningSpec.Satellite.EastWest == "west");
                        DsError.ThrowExceptionForHR(reply);

                        currentLocator = locator;

                        int carrierFrequency;
                        reply = locator.get_CarrierFrequency(out carrierFrequency);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator frequency: " + carrierFrequency + frequencyConversionString);
                        
                        int symbolRate;
                        reply = locator.get_SymbolRate(out symbolRate);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator symbol rate: " + symbolRate);

                        BinaryConvolutionCodeRate innerFec;
                        reply = locator.get_InnerFECRate(out innerFec);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator inner FEC: " + innerFec);
                        
                        Polarisation polarisation;
                        reply = locator.get_SignalPolarisation(out polarisation);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator polarization: " + polarisation + polarizationConversionString);

                        ModulationType modulation;
                        reply = locator.get_Modulation(out modulation);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator modulation: " + modulation);

                        if (logSettings)
                        {
                            Logger.Instance.Write("Locator settings: frequency: " + carrierFrequency +
                                " symbol rate: " + symbolRate +
                                " fec: " + innerFec +
                                " polarization: " + polarisation +
                                " modulation: " + modulation);
                        }

                        return (locator);
                    }
                case TunerType.Terrestrial:
                    {
                        LogMessage("Creating DVB Terrestrial locator");

                        IDVBTLocator locator = (IDVBTLocator)new DVBTLocator();

                        reply = locator.put_Bandwidth(tuningSpec.Bandwidth);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_CarrierFrequency(tuningSpec.Frequency.Frequency);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_Guard(GuardInterval.GuardNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_HAlpha(HierarchyAlpha.HAlphaNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_InnerFEC(FECMethod.MethodNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_LPInnerFEC(FECMethod.MethodNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_LPInnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_Modulation(ModulationType.ModNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_OtherFrequencyInUse(false);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_OuterFEC(FECMethod.MethodNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_SymbolRate(-1);
                        DsError.ThrowExceptionForHR(reply);

                        currentLocator = locator;

                        int carrierFrequency;
                        reply = locator.get_CarrierFrequency(out carrierFrequency);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator frequency: " + carrierFrequency);

                        int bandwidth;
                        reply = locator.get_Bandwidth(out bandwidth);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator bandwidth: " + bandwidth);

                        if (logSettings)
                        {
                            Logger.Instance.Write("Locator settings: frequency: " + carrierFrequency +
                                " bandwidth: " + bandwidth);
                        }

                        return (locator);
                    }
                case TunerType.Cable:
                    {
                        LogMessage("Creating DVB Cable locator");

                        IDVBCLocator locator = (IDVBCLocator)new DVBCLocator();

                        reply = locator.put_CarrierFrequency(tuningSpec.Frequency.Frequency);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_SymbolRate(tuningSpec.SymbolRate);
                        DsError.ThrowExceptionForHR(reply);

                        /*reply = locator.put_InnerFEC(FECMethod.Viterbi);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_InnerFECRate(Utils.GetNativeFECRate(tuningSpec.FECRate));
                        DsError.ThrowExceptionForHR(reply);*/

                        reply = locator.put_Modulation(Utils.GetNativeModulation(tuningSpec.Modulation));
                        DsError.ThrowExceptionForHR(reply);

                        currentLocator = locator;

                        int carrierFrequency;
                        reply = locator.get_CarrierFrequency(out carrierFrequency);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator frequency: " + carrierFrequency);

                        int symbolRate;
                        reply = locator.get_SymbolRate(out symbolRate);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator symbol rate: " + symbolRate);

                        /*BinaryConvolutionCodeRate innerFec;
                        reply = locator.get_InnerFECRate(out innerFec);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator inner FEC: " + innerFec);*/

                        ModulationType modulation;
                        reply = locator.get_Modulation(out modulation);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator modulation: " + modulation);                        

                        if (logSettings)
                        {
                            Logger.Instance.Write("Locator settings: frequency: " + carrierFrequency +
                                " symbol rate: " + symbolRate +
                                " modulation: " + modulation);
                        }

                        return (locator);
                    }
                case TunerType.ATSC:
                    {
                        LogMessage("Creating ATSC Terrestrial locator");

                        IATSCLocator locator = (IATSCLocator)new ATSCLocator();

                        reply = locator.put_CarrierFrequency(-1);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_InnerFEC(FECMethod.MethodNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_Modulation(ModulationType.ModNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_OuterFEC(FECMethod.MethodNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_PhysicalChannel(tuningSpec.ChannelNumber);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_SymbolRate(-1);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_TSID(-1);
                        DsError.ThrowExceptionForHR(reply);

                        currentLocator = locator;

                        int physicalChannel;
                        reply = locator.get_PhysicalChannel(out physicalChannel);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator physical channel: " + physicalChannel);

                        if (logSettings)
                            Logger.Instance.Write("Physical channel: " + physicalChannel);

                        return (locator);
                    }
                case TunerType.ATSCCable:
                    {
                        LogMessage("Creating ATSC Cable locator");

                        IATSCLocator locator = (IATSCLocator)new ATSCLocator();

                        reply = locator.put_CarrierFrequency(tuningSpec.Frequency.Frequency);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_InnerFEC(FECMethod.MethodNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_Modulation(Utils.GetNativeModulation(tuningSpec.Modulation));
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_OuterFEC(FECMethod.MethodNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_PhysicalChannel(tuningSpec.ChannelNumber);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_SymbolRate(-1);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_TSID(-1);
                        DsError.ThrowExceptionForHR(reply);

                        currentLocator = locator;

                        int carrierFrequency;
                        reply = locator.get_CarrierFrequency(out carrierFrequency);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator frequency: " + carrierFrequency);

                        ModulationType modulation;
                        reply = locator.get_Modulation(out modulation);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator modulation: " + modulation);

                        int physicalChannel;
                        reply = locator.get_PhysicalChannel(out physicalChannel);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator physical channel: " + physicalChannel);

                        if (logSettings)
                        {
                            Logger.Instance.Write("Locator settings: frequency: " + carrierFrequency +
                                " modulation: " + modulation +
                                " physical channel: " + physicalChannel);
                        }

                        return (locator);
                    }
                case TunerType.ClearQAM:
                    {
                        LogMessage("Creating Clear QAM locator");

                        IDigitalCableLocator locator = (IDigitalCableLocator)new DigitalCableLocator();

                        reply = locator.put_CarrierFrequency(tuningSpec.Frequency.Frequency);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_InnerFEC(FECMethod.MethodNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_Modulation(Utils.GetNativeModulation(tuningSpec.Modulation));
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_OuterFEC(FECMethod.MethodNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_PhysicalChannel(tuningSpec.ChannelNumber);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_ProgramNumber(-1);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_SymbolRate(-1);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_TSID(-1);
                        DsError.ThrowExceptionForHR(reply);

                        currentLocator = locator;

                        int carrierFrequency;
                        reply = locator.get_CarrierFrequency(out carrierFrequency);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator frequency: " + carrierFrequency);

                        ModulationType modulation;
                        reply = locator.get_Modulation(out modulation);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator modulation: " + modulation);

                        int physicalChannel;
                        reply = locator.get_PhysicalChannel(out physicalChannel);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator physical channel: " + physicalChannel);

                        if (logSettings)
                        {
                            Logger.Instance.Write("Locator settings: frequency: " + carrierFrequency +
                                " modulation: " + modulation +
                                " physical chanel: " + physicalChannel);
                        }

                        return (locator);
                    }
                case TunerType.ISDBS:
                    {
                        LogMessage("Creating ISDB Satellite locator");

                        IISDBSLocator locator = (IISDBSLocator)new ISDBSLocator();

                        reply = locator.put_Azimuth(-1);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_Elevation(-1);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_CarrierFrequency(tuningSpec.Frequency.Frequency);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_InnerFEC(FECMethod.Viterbi);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_InnerFECRate(Utils.GetNativeFECRate(tuningSpec.FECRate));
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_Modulation(ModulationType.ModNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_OrbitalPosition(tuningSpec.Satellite.Longitude);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_OuterFEC(FECMethod.MethodNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_SignalPolarisation(Utils.GetNativePolarization(tuningSpec.SignalPolarization));
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_SymbolRate(tuningSpec.SymbolRate);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_WestPosition(tuningSpec.Satellite.EastWest == "west");
                        DsError.ThrowExceptionForHR(reply);

                        currentLocator = locator;

                        int carrierFrequency;
                        reply = locator.get_CarrierFrequency(out carrierFrequency);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator frequency: " + carrierFrequency);

                        int symbolRate;
                        reply = locator.get_SymbolRate(out symbolRate);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator symbol rate: " + symbolRate);

                        BinaryConvolutionCodeRate innerFec;
                        reply = locator.get_InnerFECRate(out innerFec);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator inner FEC: " + innerFec);

                        Polarisation polarisation;
                        reply = locator.get_SignalPolarisation(out polarisation);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator polarization: " + polarisation);

                        ModulationType modulation;
                        reply = locator.get_Modulation(out modulation);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator modulation: " + modulation);

                        if (logSettings)
                        {
                            Logger.Instance.Write("Locator settings: frequency: " + carrierFrequency +
                                " symbol rate: " + symbolRate +
                                " fec: " + innerFec +
                                " polarization: " + polarisation +
                                " modulation: " + modulation);
                        }

                        return (locator);
                    }
                case TunerType.ISDBT:
                    {
                        LogMessage("Creating ISDB Terrestrial locator");

                        IDVBTLocator locator = (IDVBTLocator)new DVBTLocator();

                        reply = locator.put_Bandwidth(tuningSpec.Bandwidth);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_CarrierFrequency(tuningSpec.Frequency.Frequency);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_Guard(GuardInterval.GuardNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_HAlpha(HierarchyAlpha.HAlphaNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_InnerFEC(FECMethod.MethodNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_InnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_LPInnerFEC(FECMethod.MethodNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_LPInnerFECRate(BinaryConvolutionCodeRate.RateNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_Modulation(ModulationType.ModNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_OtherFrequencyInUse(false);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_OuterFEC(FECMethod.MethodNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_OuterFECRate(BinaryConvolutionCodeRate.RateNotSet);
                        DsError.ThrowExceptionForHR(reply);

                        reply = locator.put_SymbolRate(-1);
                        DsError.ThrowExceptionForHR(reply);

                        currentLocator = locator;

                        int carrierFrequency;
                        reply = locator.get_CarrierFrequency(out carrierFrequency);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator frequency: " + carrierFrequency);

                        int bandwidth;
                        reply = locator.get_Bandwidth(out bandwidth);
                        DsError.ThrowExceptionForHR(reply);
                        LogMessage("Locator bandwidth: " + bandwidth);

                        if (logSettings)
                        {
                            Logger.Instance.Write("Locator settings: frequency: " + carrierFrequency +
                                " bandwidth: " + bandwidth);
                        }

                        return (locator);
                    }
                default:
                    return (null);
            }
        }

        /// <summary>
        /// Find a tuner.
        /// </summary>
        /// <param name="tuners">The list of tuners to try.</param>
        /// <param name="tunerNodeType">The node type the tuner must have.</param>
        /// <param name="tuningSpec">A tuning spec instance with tuning details.</param>
        /// <param name="lastTuner">The last tuner used or null if all are to be considered.</param>
        /// <returns>A graph instance.</returns>
        public static BDAGraph FindTuner(Collection<SelectedTuner> tuners, TunerNodeType tunerNodeType, TuningSpec tuningSpec, Tuner lastTuner)
        {
            return (FindTuner(tuners, tunerNodeType, tuningSpec, lastTuner, null));

        }

        /// <summary>
        /// Find a tuner.
        /// </summary>
        /// <param name="tuners">The list of tuners to try.</param>
        /// <param name="tunerNodeType">The node type the tuner must have.</param>
        /// <param name="tuningSpec">A tuning spec instance with tuning details.</param>
        /// <param name="lastTuner">The last tuner used or null if all are to be considered.</param>
        /// <param name="dumpFileName">The name of the dump file.</param>
        /// <returns>A graph instance.</returns>
        public static BDAGraph FindTuner(Collection<SelectedTuner> tuners, TunerNodeType tunerNodeType, TuningSpec tuningSpec, Tuner lastTuner, string dumpFileName)
        {
            bool process = (lastTuner == null);

            if (tuners.Count != 0)
            {
                for (int index = 0; index < tuners.Count; index++)
                {
                    int tunerNumber = tuners[index].TunerNumber - 1;

                    if (tunerNumber < Tuner.TunerCollection.Count && Tuner.TunerCollection[tunerNumber].Supports(tunerNodeType))
                    {
                        Tuner tuner = Tuner.TunerCollection[tunerNumber];

                        if (!tuner.IsServerTuner && process)
                        {
                            BDAGraph graph = checkTunerAvailability(tuner, tuningSpec, dumpFileName);
                            if (graph != null)
                            {
                                Logger.Instance.Write("Using tuner " + (tunerNumber + 1) + ": " + tuner.Name);
                                return (graph);
                            }
                        }

                        if (!process)
                            process = (tuner == lastTuner);
                    }
                }
                return (null);
            }

            for (int index = 0; index < Tuner.TunerCollection.Count; index++)
            {
                Tuner tuner = Tuner.TunerCollection[index];

                if (!tuner.IsServerTuner && process)
                {
                    if (tuner.Supports(tunerNodeType))
                    {
                        BDAGraph graph = checkTunerAvailability(tuner, tuningSpec, dumpFileName);
                        if (graph != null)
                        {
                            Logger.Instance.Write("Using tuner " + (Tuner.TunerCollection.IndexOf(tuner) + 1) + ": " + tuner.Name);
                            return (graph);
                        }
                    }
                }

                if (!process)
                    process = (tuner == lastTuner);
            }

            return (null);
        }

        private static BDAGraph checkTunerAvailability(Tuner tuner, TuningSpec tuningSpec, string dumpFileName)
        {
            if (!DebugEntry.IsDefined(DebugName.UseDvbLink))
            {
                if (tuner.Name.ToUpperInvariant().StartsWith("DVBLINK"))
                    return (null);
            }

            BDAGraph graph = new BDAGraph("BDA", tuningSpec, tuner, dumpFileName);
            graph.LogFilters();

            if (graph.Play())
                return (graph);
            else
            {
                Logger.Instance.Write("Tuner in use - ignored: " + tuner.Name);
                graph.Dispose();
                return (null);
            }
        }

        private void buildGraph(ITuningSpace tuningSpace, Tuner tuner, TuningSpec tuningSpec)
        {
            base.BuildGraph();

            addNetworkProviderFilter(tuningSpace);
            infiniteTeeFilter = addInfiniteTeeFilter();
            mpeg2DemuxFilter = addMPEG2DemuxFilter();

            addHardwareFilters(tuner);
            insertInfiniteTee();
            addTransportStreamFilters();
            addPSIMemoryFilter();

            if (tuningSpec.Frequency.TunerType == TunerType.ATSC ||
                tuningSpec.Frequency.TunerType == TunerType.ATSCCable ||
                tuningSpec.Frequency.TunerType == TunerType.ClearQAM)
                connectDownStreamFilters(mpeg2DemuxFilter, MediaSubType.AtscSI);
            else
                connectDownStreamFilters(mpeg2DemuxFilter, MediaSubType.DvbSI);
        }

        private void addNetworkProviderFilter(ITuningSpace dvbTuningSpace)
        {
            Guid genProviderClsId = new Guid("{B2F3A67C-29DA-4C78-8831-091ED509A475}");
            Guid networkProviderClsId;
            reply = dvbTuningSpace.get__NetworkType(out networkProviderClsId);

            LogMessage("Adding Network Provider");

            if (!DebugEntry.IsDefined(DebugName.UseSpecificNp))
            {
                LogMessage("Trying Generic Network Provider");

                if (FilterGraphTools.IsThisComObjectInstalled(genProviderClsId))
                {
                    LogMessage("Adding Generic Network Provider");
                    this.networkProviderFilter = FilterGraphTools.AddFilterFromClsid(GraphBuilder, genProviderClsId, "Generic Network Provider");
                    LogMessage("Generic Provider added - setting tuning space");
                    reply = (this.networkProviderFilter as ITuner).put_TuningSpace(dvbTuningSpace);
                    LogMessage("Tuning space set");
                    return;
                }
            }

            LogMessage("Adding Network Specific Provider");

            if (networkProviderClsId == typeof(DVBTNetworkProvider).GUID)
            {
                this.networkProviderFilter = FilterGraphTools.AddFilterFromClsid(GraphBuilder, networkProviderClsId, "DVBT Network Provider");
            }
            else if (networkProviderClsId == typeof(DVBSNetworkProvider).GUID)
            {
                this.networkProviderFilter = FilterGraphTools.AddFilterFromClsid(GraphBuilder, networkProviderClsId, "DVBS Network Provider");
            }
            else if (networkProviderClsId == typeof(ATSCNetworkProvider).GUID)
            {
                this.networkProviderFilter = FilterGraphTools.AddFilterFromClsid(GraphBuilder, networkProviderClsId, "ATSC Network Provider");
            }
            else if (networkProviderClsId == typeof(DVBCNetworkProvider).GUID)
            {
                this.networkProviderFilter = FilterGraphTools.AddFilterFromClsid(GraphBuilder, networkProviderClsId, "DVBC Network Provider");
            }
            else
                throw new ArgumentException("Tuning Space not supported");

            LogMessage("Network Specific Provider added - setting tuning space");
            reply = (this.networkProviderFilter as ITuner).put_TuningSpace(dvbTuningSpace);
            LogMessage("Tuning space set");
        }

        private void addHardwareFilters(Tuner selectedTuner)
        {
            LogMessage("Adding hardware filters");

            if (!addTunerFilter(selectedTuner))
            {
                Logger.Instance.Write("<E> No valid BDA tuner filter found");
                Environment.Exit((int)ExitCode.NoBDATunerFilter);
            }

            reply = CaptureGraphBuilder.RenderStream(null, null, tunerFilter, null, mpeg2DemuxFilter);
            if (reply >= 0)
            {
                LogMessage("Tuner filter only needed for hardware");
                return;
            }

            DsDevice[] receiverComponents = DsDevice.GetDevicesOfCat(FilterCategory.BDAReceiverComponentsCategory);
            LogMessage("Receiver filter count: " + receiverComponents.Length);
            for (int logIndex = 0; logIndex < receiverComponents.Length; logIndex++)
                LogMessage("Receiver filter " + receiverComponents[logIndex].Name);

            receiverFilters = new Collection<IBaseFilter>();

            bool done = addReceiverFilters(tunerFilter, mpeg2DemuxFilter, receiverComponents, receiverFilters);
            if (done)
                LogMessage("Hardware filters added and connected");
            else
            {
                Logger.Instance.Write("<E> Failed to load and connect hardware filters.");
                Environment.Exit((int)ExitCode.HardwareFilterChainNotBuilt);
            }
        }

        private bool addTunerFilter(Tuner selectedTuner)
        {
            LogMessage("Adding tuner filter");

            DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);

            for (int tunerIndex = 0; tunerIndex < devices.Length; tunerIndex++)
            {
                bool correctTuner = (selectedTuner.Path == (devices[tunerIndex].DevicePath));

                if (correctTuner)
                {
                    IBaseFilter tempTunerFilter;

                    LogMessage("Adding tuner filter " + devices[tunerIndex].Name);
                    reply = GraphBuilder.AddSourceFilterForMoniker(devices[tunerIndex].Moniker, null, devices[tunerIndex].Name, out tempTunerFilter);
                    if (reply >= 0)
                    {
                        reply = CaptureGraphBuilder.RenderStream(null, null, networkProviderFilter, null, tempTunerFilter);
                        if (reply >= 0)
                        {
                            LogMessage("Added tuner filter " + devices[tunerIndex].Name);
                            tunerFilter = tempTunerFilter;
                            return (true);
                        }
                        else
                        {
                            LogMessage("Failed to connect network provider to tuner filter " + devices[tunerIndex].Name + " reply 0x" + reply.ToString("X"));
                            reply = GraphBuilder.RemoveFilter(tempTunerFilter);
                            Marshal.ReleaseComObject(tempTunerFilter);
                        }
                    }
                    else
                        LogMessage("<E> Failed to add tuner filter " + devices[tunerIndex].Name + " reply 0x" + reply.ToString("X"));
                }
            }

            return (false);
        }

        private bool addReceiverFilters(IBaseFilter preceedingFilter, IBaseFilter mpeg2Demux, DsDevice[] receiverComponents, Collection<IBaseFilter> receiverFilters)
        {
            IBaseFilter tempFilter;

            for (int index = 0; index < receiverComponents.Length; index++)
            {
                DsDevice currentReceiver = receiverComponents[index];

                LogMessage("Adding receiver filter " + currentReceiver.Name);
                reply = GraphBuilder.AddSourceFilterForMoniker(currentReceiver.Moniker, null, currentReceiver.Name, out tempFilter);
                if (reply < 0)
                    LogMessage("<E> Failed to load filter " + currentReceiver.Name);
                else
                {
                    reply = CaptureGraphBuilder.RenderStream(null, null, preceedingFilter, null, tempFilter);
                    if (reply < 0)
                    {
                        LogMessage("Could not connect to preceeding filter reply 0x" + reply.ToString("X"));
                        reply = GraphBuilder.RemoveFilter(tempFilter);
                        DsError.ThrowExceptionForHR(reply);
                        Marshal.ReleaseComObject(tempFilter);
                    }
                    else
                    {
                        LogMessage("Connected to preceeding filter");
                        receiverFilters.Add(tempFilter);

                        reply = CaptureGraphBuilder.RenderStream(null, null, tempFilter, null, mpeg2Demux);
                        if (reply < 0)
                        {
                            LogMessage("Could not connect to MPEG2 Demux filter reply 0x" + reply.ToString("X"));
                            return (addReceiverFilters(tempFilter, mpeg2Demux, receiverComponents, receiverFilters));
                        }
                        else
                        {
                            LogMessage("Connected to MPEG2 demux filter - hardware chain complete");
                            return (true);
                        }
                    }
                }
            }

            return (false);
        }

        private void addTransportStreamFilters()
        {
            DsDevice[] devices;

            devices = DsDevice.GetDevicesOfCat(FilterCategory.BDATransportInformationRenderersCategory);
            for (int i = 0; i < devices.Length; i++)
            {
                if (devices[i].Name.Equals("BDA MPEG2 Transport Information Filter"))
                {
                    LogMessage("Adding TIF Filter");
                    reply = GraphBuilder.AddSourceFilterForMoniker(devices[i].Moniker, null, devices[i].Name, out tifFilter);
                    DsError.ThrowExceptionForHR(reply);
                    LogMessage("Added TIF Filter");
                    continue;
                }

                if (devices[i].Name.Equals("MPEG-2 Sections and Tables"))
                {
                    LogMessage("Adding Sections And Tables Filter");
                    reply = GraphBuilder.AddSourceFilterForMoniker(devices[i].Moniker, null, devices[i].Name, out secTabFilter);
                    DsError.ThrowExceptionForHR(reply);
                    LogMessage("Added Sections And Tables Filter");
                    continue;
                }
            }
        }

        private IBaseFilter addInfiniteTeeFilter()
        {
            LogMessage("Adding Infinite Tee Filter");

            IBaseFilter infiniteTeeFilter = (IBaseFilter)new InfTee();
            reply = GraphBuilder.AddFilter(infiniteTeeFilter, "Infinite Tee Filter");
            DsError.ThrowExceptionForHR(reply);

            LogMessage("Added Infinite Tee Filter");

            return (infiniteTeeFilter);
        }

        private IBaseFilter addMPEG2DemuxFilter()
        {
            LogMessage("Adding MPEG2 Demux");

            IBaseFilter mpeg2DemuxFilter = (IBaseFilter)new MPEG2Demultiplexer();
            reply = GraphBuilder.AddFilter(mpeg2DemuxFilter, "MPEG2 Demultiplexer");
            DsError.ThrowExceptionForHR(reply);

            LogMessage("Added MPEG2 Demux");

            return (mpeg2DemuxFilter);
        }

        private void addPSIMemoryFilter()
        {
            LogMessage("Adding PSI Memory Filter");

            if (dumpFileName == null)
            {
                reply = CreatePSIMemoryFilter(GraphBuilder,
                    TraceEntry.IsDefined(TraceName.PsiFilter),
                    Path.Combine(RunParameters.DataDirectory, "PSI Memory Filter.log"),
                    false, string.Empty,
                    RunParameters.Instance.BufferSize);
            }
            else
            {
                reply = CreatePSIMemoryFilter(GraphBuilder,
                    TraceEntry.IsDefined(TraceName.PsiFilter),
                    Path.Combine(RunParameters.DataDirectory, "PSI Memory Filter.log"),
                    true, dumpFileName,
                    RunParameters.Instance.BufferSize);
            }

            DsError.ThrowExceptionForHR(reply);

            reply = GraphBuilder.FindFilterByName("PSI Memory Filter", out psiMemoryFilter);
            DsError.ThrowExceptionForHR(reply);

            LogMessage("Added PSI Memory Filter");
        }

        private void connectDownStreamFilters(IBaseFilter mpeg2DemuxFilter, Guid mediaSubType)
        {
            LogMessage("Connecting MPEG2 demux to TIF filter using media subtype " + TranslateMediaSubType(mediaSubType));

            IPin muxTIFPin = FindPin(mpeg2DemuxFilter, MediaType.Mpeg2Sections, mediaSubType, PinDirection.Output);
            if (muxTIFPin == null)
            {
                LogFilters();
                Logger.Instance.Write("<E> The MPEG2 demux pin for media subtype " + TranslateMediaSubType(mediaSubType) + " does not exist");
            }
            IPin tifPin = FindPin(tifFilter, PinDirection.Input);
            reply = GraphBuilder.Connect(muxTIFPin, tifPin);
            if (reply != 0)
                LogFilters();
            DsError.ThrowExceptionForHR(reply);
            LogMessage("Connected MPEG2 demux to TIF filter");

            Marshal.ReleaseComObject(muxTIFPin);
            Marshal.ReleaseComObject(tifPin);

            LogMessage("Connecting MPEG2 Demux to Sections And Tables filter");
            IPin muxPinSecTab = FindPin(mpeg2DemuxFilter, MediaType.Mpeg2Sections, MediaSubType.Mpeg2Data, PinDirection.Output);
            if (muxPinSecTab == null)
                Logger.Instance.Write("<E> The MPEG2 demux pin for the Sections and Tables filter does not exist");
            IPin secTabPin = FindPin(secTabFilter, PinDirection.Input);
            reply = GraphBuilder.Connect(muxPinSecTab, secTabPin);
            DsError.ThrowExceptionForHR(reply);
            LogMessage("Connected MPEG2 demux to Sections And Tables filter");

            Marshal.ReleaseComObject(muxPinSecTab);
            Marshal.ReleaseComObject(secTabPin);

            LogMessage("Connecting Infinite Tee to PSI Memory filter");

            IPin infiniteTeeOutputPin = FindPin(infiniteTeeFilter, "Output2");
            if (infiniteTeeOutputPin == null)
                Logger.Instance.Write("<E> InfiniteTee pin 'Output2' does not exist");
            IPin psiInputPin = FindPin(psiMemoryFilter, PinDirection.Input);
            reply = GraphBuilder.Connect(infiniteTeeOutputPin, psiInputPin);
            DsError.ThrowExceptionForHR(reply);
            LogMessage("Connected Infinite Tee to PSI Memory filter");

            Marshal.ReleaseComObject(infiniteTeeOutputPin);
            Marshal.ReleaseComObject(psiInputPin);
        }

        private void insertInfiniteTee()
        {
            LogMessage("Inserting Infinite Tee in graph");

            IBaseFilter sourceFilter;

            if (receiverFilters == null)
                sourceFilter = tunerFilter;
            else
                sourceFilter = receiverFilters[receiverFilters.Count - 1];

            LogMessage("Disconnecting source filter output pin");
            IPin captureOutputPin = FindPin(sourceFilter, PinDirection.Output);
            GraphBuilder.Disconnect(captureOutputPin);
            DsError.ThrowExceptionForHR(reply);

            LogMessage("Disconnecting MPEG2 Demux filter input pin");
            IPin mpeg2DemuxInputPin = FindPin(mpeg2DemuxFilter, PinDirection.Input);
            GraphBuilder.Disconnect(mpeg2DemuxInputPin);
            DsError.ThrowExceptionForHR(reply);

            LogMessage("Connecting source filter output to infinite tee input");
            IPin infiniteTeeInputPin = FindPin(infiniteTeeFilter, PinDirection.Input);
            GraphBuilder.Connect(captureOutputPin, infiniteTeeInputPin);
            DsError.ThrowExceptionForHR(reply);

            LogMessage("Connecting infinite tee output to MPEG2 demux input");
            mpeg2DemuxInputPin = FindPin(mpeg2DemuxFilter, PinDirection.Input);
            IPin infiniteTeeOutputPin = FindPin(infiniteTeeFilter, PinDirection.Output);
            GraphBuilder.Connect(infiniteTeeOutputPin, mpeg2DemuxInputPin);
            DsError.ThrowExceptionForHR(reply);

            Marshal.ReleaseComObject(captureOutputPin);
            Marshal.ReleaseComObject(infiniteTeeInputPin);
            Marshal.ReleaseComObject(mpeg2DemuxInputPin);
            Marshal.ReleaseComObject(infiniteTeeOutputPin);
        }

        /// <summary>
        /// Change the PSI filter PID mappings.
        /// </summary>
        /// <param name="newPid">The new PID to be set.</param>
        public void ChangePidMapping(int newPid)
        {
            ChangePidMapping(new int[] { newPid });
        }

        /// <summary>
        /// Change the PSI filter PID mappings.
        /// </summary>
        /// <param name="newPids">A list of the new PID's to be set.</param>
        public void ChangePidMapping(int[] newPids)
        {
            logPidsUnMapped(psiPids);

            ((IMemSinkSettings)psiMemoryFilter).clearPIDs();
            psiPids.Clear();

            logPidsMapped(newPids);

            foreach (int newPid in newPids)
            {
                ((IMemSinkSettings)psiMemoryFilter).mapPID(newPid);
                psiPids.Add(newPid);
            }

            ((IMemSinkSettings)psiMemoryFilter).clear();
        }

        private void logPidsUnMapped(Collection<int> oldPids)
        {
            if (oldPids.Count == 0)
                return;

            StringBuilder pidString = new StringBuilder();
            foreach (int oldPid in oldPids)
            {
                if (pidString.Length == 0)
                    pidString.Append("Unmapping pid(s) 0x" + oldPid.ToString("X"));
                else
                    pidString.Append(", 0x" + oldPid.ToString("X"));
            }
            LogMessage(pidString.ToString());
        }

        private void logPidsMapped(int[] newPids)
        {
            if (newPids.Length == 0)
                return;

            StringBuilder pidString = new StringBuilder();
            foreach (int newPid in newPids)
            {
                if (pidString.Length == 0)
                    pidString.Append("Mapping pid(s) 0x" + newPid.ToString("X"));
                else
                    pidString.Append(", 0x" + newPid.ToString("X"));
            }
            LogMessage(pidString.ToString());
        }

        /// <summary>
        /// Start the graph.
        /// </summary>
        /// <returns>True if the graph can be started; false otherwise.</returns>
        public override bool Play()
        {
            TerrestrialFrequency terrestrialFrequency = tuningSpec.Frequency as TerrestrialFrequency;
            if (terrestrialFrequency != null && terrestrialFrequency.PlpNumber != -1)
                GenericDVBT2Handler.SetPlp(tunerFilter, terrestrialFrequency.PlpNumber);

            SatelliteFrequency satelliteFrequency = tuningSpec.Frequency as SatelliteFrequency;
            if (satelliteFrequency == null)
            {
                Logger.Instance.Write("Setting tune request");
                reply = (networkProviderFilter as ITuner).put_TuneRequest(tuneRequest);
                DsError.ThrowExceptionForHR(reply);

                Logger.Instance.Write("Running graph");
                return (base.Play());
            }

            if (!satelliteFrequency.DiseqcRunParamters.SwitchAfterPlay)
            {
                if (tuningSpec.Frequency.TunerType == TunerType.Satellite && ((SatelliteFrequency)tuningSpec.Frequency).DiseqcRunParamters.DiseqcSwitch != null)
                {
                    if (satelliteFrequency.DiseqcRunParamters.UseSafeDiseqc)
                    {
                        Logger.Instance.Write("Checking tuner free for safe DiSEqC switching");
                        
                        bool tunerFree = base.Play();
                        base.Stop();
                        if (!tunerFree)
                            return (false);

                        Logger.Instance.Write("Tuner not in use - OK to change DiSEqC switch");
                    }
                }

                if (!satelliteFrequency.DiseqcRunParamters.SwitchAfterTune)
                    changeDiseqcSwitch(satelliteFrequency.DiseqcRunParamters);

                Logger.Instance.Write("Setting tune request");
                reply = (networkProviderFilter as ITuner).put_TuneRequest(tuneRequest);
                DsError.ThrowExceptionForHR(reply);

                if (satelliteFrequency.DiseqcRunParamters.SwitchAfterTune)
                    changeDiseqcSwitch(satelliteFrequency.DiseqcRunParamters);

                setDVBS2Parameters();

                Logger.Instance.Write("Running graph");
                return (base.Play());
            }
            else
            {
                Logger.Instance.Write("Running graph");
                bool playReply = base.Play();
                if (playReply)
                {
                    if (!satelliteFrequency.DiseqcRunParamters.SwitchAfterTune)
                        changeDiseqcSwitch(satelliteFrequency.DiseqcRunParamters);                    

                    Logger.Instance.Write("Setting tune request");
                    reply = (networkProviderFilter as ITuner).put_TuneRequest(tuneRequest);
                    DsError.ThrowExceptionForHR(reply);

                    if (satelliteFrequency.DiseqcRunParamters.SwitchAfterTune)
                        changeDiseqcSwitch(satelliteFrequency.DiseqcRunParamters); 

                    setDVBS2Parameters();
                }

                return (playReply);
            }

        }

        private void changeDiseqcSwitch(DiseqcRunParameters diseqcRunParameters)
        {
            if (tuningSpec.Frequency.TunerType != TunerType.Satellite || ((SatelliteFrequency)tuningSpec.Frequency).DiseqcRunParamters.DiseqcSwitch == null)
                return;
            
            SwitchReply switchReply = SwitchReply.NotSet;

            switchReply = DiseqcHandlerBase.ProcessDisEqcSwitch(tuningSpec, tunerSpec, tunerFilter, diseqcRunParameters);
            Thread.Sleep(500);
            
            if (switchReply == SwitchReply.Failed)
            {
                Logger.Instance.Write("Repeating DiSEqC command due to initial failure");
                switchReply = DiseqcHandlerBase.ProcessDisEqcSwitch(tuningSpec, tunerSpec, tunerFilter, diseqcRunParameters);
            }            
        }

        private void setDVBS2Parameters()
        {
            SatelliteFrequency satelliteFrequency = tuningSpec.Frequency as SatelliteFrequency;
            if (satelliteFrequency == null)
                return;

            dvbs2Handler = DVBS2HandlerBase.GetDVBS2Handler(tuningSpec, tunerFilter, tuneRequest, tunerSpec);
            if (dvbs2Handler == null)
                LogMessage("DVB-S2: No handler available");
            else
            {
                if (satelliteFrequency.Pilot == SignalPilot.Pilot.NotSet && satelliteFrequency.RollOff == SignalRollOff.RollOff.NotSet)
                {
                    bool parametersCleared = dvbs2Handler.ClearDVBS2Parameters(tuningSpec, tunerFilter, tuneRequest);
                    if (parametersCleared)
                        LogMessage("DVB-S2: Parameters cleared");
                    else
                        LogMessage("DVB-S2: Handler failed to clear DVB-S2 parameters");
                }
                else
                {
                    bool parametersSet = dvbs2Handler.SetDVBS2Parameters(tuningSpec, tunerFilter, tuneRequest);
                    if (!parametersSet)
                        LogMessage("DVB-S2: Handler failed to set parameters");
                }
            }
        }

        /// <summary>
        /// Dispose of the filters created by this graph.
        /// </summary>
        public override void Dispose()
        {
            if (CaptureGraphBuilder == null)
            {
                LogMessage("No graph components exist for disposal");
                return;
            }

            tunerFilter.Stop();

            base.Dispose();

            LogMessage("Releasing Network Provider");
            Marshal.ReleaseComObject(networkProviderFilter);
            networkProviderFilter = null;

            LogMessage("Releasing MPEG2 Demux");
            Marshal.ReleaseComObject(mpeg2DemuxFilter);
            mpeg2DemuxFilter = null;

            LogMessage("Releasing Tuner");
            Marshal.ReleaseComObject(tunerFilter);
            tunerFilter = null;

            if (receiverFilters != null)
            {
                foreach (IBaseFilter receiverFilter in receiverFilters)
                {
                    LogMessage("Releasing Receiver filter");
                    Marshal.ReleaseComObject(receiverFilter);
                }
            }

            if (tifFilter != null)
            {
                LogMessage("Releasing TIF filter");
                Marshal.ReleaseComObject(tifFilter);
                tifFilter = null;
            }

            if (secTabFilter != null)
            {
                LogMessage("Releasing Sections And Tables filter");
                Marshal.ReleaseComObject(secTabFilter);
                secTabFilter = null;
            }

            if (infiniteTeeFilter != null)
            {
                LogMessage("Releasing Infinite Tee filter");
                Marshal.ReleaseComObject(infiniteTeeFilter);
                infiniteTeeFilter = null;
            }

            if (psiMemoryFilter != null)
            {
                LogMessage("Releasing PSI Memory filter");
                Marshal.ReleaseComObject(psiMemoryFilter);
                psiMemoryFilter = null;
            }

            if (tuningSpace != null)
            {
                LogMessage("Releasing Tuning Space");
                Marshal.ReleaseComObject(tuningSpace);
                tuningSpace = null;
            }

            if (tuneRequest != null)
            {
                LogMessage("Releasing Tune Request");
                Marshal.ReleaseComObject(tuneRequest);
                tuneRequest = null;
            }

            if (currentLocator != null)
            {
                LogMessage("Releasing Locator");
                Marshal.ReleaseComObject(currentLocator);
                currentLocator = null;
            }
        }

        /// <summary>
        /// Load the installed tuners.
        /// </summary>
        public static void LoadTuners()
        {
            ObservableCollection<Tuner> tuners = new ObservableCollection<Tuner>();
            
            if (RunParameters.IsMono || RunParameters.IsWine)
            {
                Tuner.TunerCollection = tuners;
                return;
            }

            if (CommandLine.DummyTuners)
            {
                Tuner tuner1 = new Tuner("");
                tuner1.Name = "Dummy Terrestrial Tuner";
                tuner1.TunerNodes = new Collection<TunerNode>();
                tuner1.TunerNodes.Add(new TunerNode(0, TunerNodeType.Terrestrial));
                tuners.Add(tuner1);

                Tuner tuner2 = new Tuner("");
                tuner2.Name = "Dummy Cable Tuner";
                tuner2.TunerNodes = new Collection<TunerNode>();
                tuner2.TunerNodes.Add(new TunerNode(0, TunerNodeType.Cable));
                tuners.Add(tuner2);

                Tuner tuner3 = new Tuner("");
                tuner3.Name = "Dummy ATSC Tuner";
                tuner3.TunerNodes = new Collection<TunerNode>();
                tuner3.TunerNodes.Add(new TunerNode(0, TunerNodeType.ATSC));
                tuner3.TunerNodes.Add(new TunerNode(0, TunerNodeType.Cable));
                tuners.Add(tuner3);

                Tuner tuner4 = new Tuner("");
                tuner4.Name = "Dummy ISDB-S Tuner";
                tuner4.TunerNodes = new Collection<TunerNode>();
                tuner4.TunerNodes.Add(new TunerNode(0, TunerNodeType.ISDBS));
                tuners.Add(tuner4);

                Tuner tuner5 = new Tuner("");
                tuner5.Name = "Dummy ISDB-T Tuner";
                tuner5.TunerNodes = new Collection<TunerNode>();
                tuner5.TunerNodes.Add(new TunerNode(0, TunerNodeType.ISDBT));
                tuners.Add(tuner5);
            }

            DsDevice[] devices = DsDevice.GetDevicesOfCat(FilterCategory.BDASourceFiltersCategory);

            Logger.Instance.Write("Number of devices: " + devices.Length);
            int deviceNumber = 1;
            
            foreach (DsDevice device in devices)
            {
                Tuner tuner = new Tuner(device.DevicePath);
                tuner.Name = device.Name;
                tuner.TunerNodes = getTunerNodes(device, deviceNumber);
                tuners.Add(tuner);
                deviceNumber++;
            }

            Logger.Instance.Write(" ");
            int tunerNumber = 1;
            foreach (Tuner tuner in tuners)
            {
                Logger.Instance.Write("Found tuner " + tunerNumber + ": " + tuner);
                tunerNumber++;
            }
            Logger.Instance.Write(" ");

            Tuner.TunerCollection = tuners;
        }

        private static Collection<TunerNode> getTunerNodes(DsDevice device, int deviceNumber)
        {
            Logger.Instance.Write("Tuner info: Processing device " + deviceNumber + " - " + (string.IsNullOrWhiteSpace(device.DevicePath) ? "Unknown Device" : device.DevicePath));

            IFilterGraph2 graphBuilder;
            IBaseFilter tunerFilter;            
            int reply;

            Collection<TunerNode> tunerNodes = new Collection<TunerNode>();

            if (device.Moniker != null)
            {
                string displayName = null;
                device.Moniker.GetDisplayName(null, null, out displayName);
                Logger.Instance.Write("Tuner info: Device moniker display name is " +
                    (string.IsNullOrWhiteSpace(displayName) ? "null" : displayName));
            }
            else
            {
                Logger.Instance.Write("Device " +
                    (!string.IsNullOrWhiteSpace(device.Name) ? device.Name : device.DevicePath) +
                    " ignored - moniker is null");
                return (tunerNodes);
            }

            Logger.Instance.Write("Tuner info: Device name is " +
                (string.IsNullOrWhiteSpace(device.Name) ? "null" : device.Name));

            graphBuilder = (IFilterGraph2)new FilterGraph();

            try
            {
                reply = graphBuilder.AddSourceFilterForMoniker(device.Moniker, null, device.Name, out tunerFilter);
                if (reply != 0)
                {
                    Logger.Instance.Write("<e> Device " +
                        (!string.IsNullOrWhiteSpace(device.Name) ? device.Name : device.DevicePath) +
                        " ignored - tuner filter could not be added to graph due to error");
                    string errorMessage = DsError.GetErrorText(reply);
                    Logger.Instance.Write("<e> " + (errorMessage != null ? errorMessage : "Error code 0x" + reply.ToString("x")));
                    Marshal.ReleaseComObject(graphBuilder);
                    return (tunerNodes);
                }
            }
            catch (Exception e)
            {
                Logger.Instance.Write("<E> Device " +
                    (!string.IsNullOrWhiteSpace(device.Name) ? device.Name : device.DevicePath) +
                    " ignored - tuner filter could not be added to graph due to exception");
                Logger.Instance.Write("<E> " + e.Message);
                Marshal.ReleaseComObject(graphBuilder);
                return (tunerNodes);
            }

            IBDA_Topology topology = tunerFilter as IBDA_Topology;
            if (topology == null)
            {
                FilterGraphTools.RemoveAllFilters(graphBuilder);
                Marshal.ReleaseComObject(graphBuilder);
                Marshal.ReleaseComObject(tunerFilter);
                Logger.Instance.Write("Unable to get topology from tuner");
                return (tunerNodes);
            }

            int nodeCount;
            int[] nodeTypes = new int[256];
            reply = topology.GetNodeTypes(out nodeCount, nodeTypes.Length, nodeTypes);
            DsError.ThrowExceptionForHR(reply);

            if (nodeCount != 0)
            {
                for (int index = 0; index < nodeCount; index++)
                    Logger.Instance.Write("Tuner info: Node type " + nodeTypes[index]);
            }
            else
                Logger.Instance.Write("Tuner info: GetNodeTypes returned zero entries");

            int descriptorCount;
            BDANodeDescriptor[] descriptors = new BDANodeDescriptor[256];
            reply = topology.GetNodeDescriptors(out descriptorCount, descriptors.Length, descriptors);
            DsError.ThrowExceptionForHR(reply);

            if (descriptorCount != 0)
            {
                for (int index = 0; index < descriptorCount; index++)
                    Logger.Instance.Write("Tuner info: Descriptor " + descriptors[index].guidName.ToString() + " " + descriptors[index].guidFunction.ToString() + " " + descriptors[index].ulBdaNodeType);
            }
            else
                Logger.Instance.Write("Tuner info: GetNodeDescriptors returned zero entries");

            for (int nodeIndex = 0; nodeIndex < nodeCount; nodeIndex++)
            {
                int nodeType = nodeTypes[nodeIndex];
                Logger.Instance.Write("Tuner info: Processing node type " + nodeType);

                bool found = false;

                for (int descriptorIndex = 0; descriptorIndex < descriptorCount; descriptorIndex++)
                {
                    BDANodeDescriptor descriptor = descriptors[descriptorIndex];

                    if (descriptor.ulBdaNodeType == nodeType)
                    {
                        found = true;

                        if (descriptor.guidFunction == BDANodeCategory.QPSKDemodulator)
                        {
                            Logger.Instance.Write("Tuner info: Found satellite descriptor");
                            tunerNodes.Add(new TunerNode(nodeType, TunerNodeType.Satellite));
                        }
                        else
                        {
                            if (descriptor.guidFunction == BDANodeCategory.COFDMDemodulator)
                            {
                                Logger.Instance.Write("Tuner info: Found terrestrial descriptor");
                                tunerNodes.Add(new TunerNode(nodeType, TunerNodeType.Terrestrial));
                            }
                            else
                            {
                                if (descriptor.guidFunction == BDANodeCategory.QAMDemodulator)
                                {
                                    Logger.Instance.Write("Tuner info: Found cable descriptor");
                                    tunerNodes.Add(new TunerNode(nodeType, TunerNodeType.Cable));
                                }
                                else
                                {
                                    if (descriptor.guidFunction == BDANodeCategory.EightVSBDemodulator)
                                    {
                                        Logger.Instance.Write("Tuner info: Found ATSC descriptor");
                                        tunerNodes.Add(new TunerNode(nodeType, TunerNodeType.ATSC));
                                    }
                                    else
                                    {
                                        if (descriptor.guidFunction == BDANodeCategory.ISDBSDemodulator)
                                        {
                                            Logger.Instance.Write("Tuner info: Found ISDB-S descriptor");
                                            tunerNodes.Add(new TunerNode(nodeType, TunerNodeType.ISDBS));
                                        }
                                        else
                                        {
                                            if (descriptor.guidFunction == BDANodeCategory.ISDBTDemodulator)
                                            {
                                                Logger.Instance.Write("Tuner info: Found ISDB-T descriptor");
                                                tunerNodes.Add(new TunerNode(nodeType, TunerNodeType.ISDBT));
                                            }
                                            else
                                            {
                                                Logger.Instance.Write("Tuner info: Undefined descriptor found " + descriptor.guidFunction);
                                                tunerNodes.Add(new TunerNode(nodeType, TunerNodeType.Other));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (!found)
                    Logger.Instance.Write("Tuner info: Node type " + nodeType + " ignored - no descriptor found");
            }

            Logger.Instance.Write("Tuner info: All nodes processed");

            FilterGraphTools.RemoveAllFilters(graphBuilder);
            Marshal.ReleaseComObject(graphBuilder);
            Marshal.ReleaseComObject(tunerFilter);

            Logger.Instance.Write("Tuner info: Processing completed for tuner " + deviceNumber);

            return (tunerNodes);
        }

        private Collection<IBDA_SignalStatistics> getSignalStatisticsInterfaces(string action)
        {
            IBDA_Topology topology = tunerFilter as IBDA_Topology;
            if (topology == null)
            {
                Logger.Instance.Write("BDA Can't get " + action + ": no topology");
                return (null);
            }

            Collection<IBDA_SignalStatistics> signalStatsCollection = new Collection<IBDA_SignalStatistics>();

            if (TraceEntry.IsDefined(TraceName.BdaSigStats))
                Logger.Instance.Write("BDA Topology available");

            int nodeTypeCount = 0;
            int[] nodeTypes = new int[10];

            int reply = topology.GetNodeTypes(out nodeTypeCount, 10, nodeTypes);
            DsError.ThrowExceptionForHR(reply);

            if (nodeTypeCount != 0)
            {
                StringBuilder displayNodes = new StringBuilder();

                for (int index = 0; index < nodeTypeCount; index++)
                {
                    if (displayNodes.Length != 0)
                        displayNodes.Append(", ");
                    displayNodes.Append(nodeTypes[index].ToString());
                }

                if (TraceEntry.IsDefined(TraceName.BdaSigStats))
                    Logger.Instance.Write("BDA Node types: " + displayNodes);
            }
            else
                Logger.Instance.Write("BDA GetNodeTypes returned zero entries");

            for (int nodeTypeIndex = 0; nodeTypeIndex < nodeTypeCount; nodeTypeIndex++)
            {
                int interfaceCount;
                Guid[] interfaces = new Guid[32];
                reply = topology.GetNodeInterfaces(nodeTypes[nodeTypeIndex], out interfaceCount, 32, interfaces);
                DsError.ThrowExceptionForHR(reply);

                if (TraceEntry.IsDefined(TraceName.BdaSigStats))
                {
                    Logger.Instance.Write("BDA node type " + nodeTypes[nodeTypeIndex] + " has " + interfaceCount + " interfaces");

                    for (int interfaceIndex = 0; interfaceIndex < interfaceCount; interfaceIndex++)
                        Logger.Instance.Write("BDA Interface: " + interfaces[interfaceIndex].ToString());
                }
            }

            for (int nodeTypeIndex = 0; nodeTypeIndex < nodeTypeCount; nodeTypeIndex++)
            {
                int interfaceCount;
                Guid[] interfaces = new Guid[32];
                reply = topology.GetNodeInterfaces(nodeTypes[nodeTypeIndex], out interfaceCount, 32, interfaces);
                DsError.ThrowExceptionForHR(reply);

                if (TraceEntry.IsDefined(TraceName.BdaSigStats))
                    Logger.Instance.Write("BDA Signal stats interface is " + typeof(IBDA_SignalStatistics).GUID.ToString());

                for (int searchIndex = 0; searchIndex < interfaceCount; searchIndex++)
                {
                    if (interfaces[searchIndex] == typeof(IBDA_SignalStatistics).GUID)
                    {
                        if (TraceEntry.IsDefined(TraceName.BdaSigStats))
                            Logger.Instance.Write("BDA Signal stats interface located for node type " + nodeTypes[nodeTypeIndex]);

                        object controlNode;
                        reply = topology.GetControlNode(0, 1, nodeTypes[nodeTypeIndex], out controlNode);
                        DsError.ThrowExceptionForHR(reply);

                        IBDA_SignalStatistics signalStats = controlNode as IBDA_SignalStatistics;
                        if (signalStats == null)
                            Logger.Instance.Write("BDA Can't get " + action + ": cast of control node failed");
                        else
                        {
                            if (TraceEntry.IsDefined(TraceName.BdaSigStats))
                                Logger.Instance.Write("BDA Adding signal stats interface to collection");
                            signalStatsCollection.Add(signalStats);
                        }
                    }
                }
            }

            if (TraceEntry.IsDefined(TraceName.BdaSigStats))
                Logger.Instance.Write("BDA Returning " + signalStatsCollection.Count + " signal stats interfaces");

            return (signalStatsCollection);
        }

        private void releaseSignalStatisticsInterfaces(Collection<IBDA_SignalStatistics> signalStatisticsCollection)
        {
            if (TraceEntry.IsDefined(TraceName.BdaSigStats))
                Logger.Instance.Write("BDA Releasing signal stats interfaces");

            foreach (IBDA_SignalStatistics signalStatistics in signalStatisticsCollection)
                Marshal.ReleaseComObject(signalStatistics);
        }
    }
}