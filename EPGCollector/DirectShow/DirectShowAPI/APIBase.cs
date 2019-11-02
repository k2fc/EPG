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
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace DirectShowAPI
{
    #region Declarations

    /// <summary>
    /// From PIN_INFO.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    public struct PinInfo
    {
        /// <summary>
        /// The pin filter.
        /// </summary>
        [MarshalAs(UnmanagedType.Interface)] public IBaseFilter filter;
        /// <summary>
        /// The pin direction.
        /// </summary>
        public PinDirection dir;
        /// <summary>
        /// The name of the pin.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)] 
        public string name;
    }

    /// <summary>
    /// From AM_MEDIA_TYPE.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class AMMediaType
    {
        /// <summary>
        /// The major type.
        /// </summary>
        public Guid majorType;
        /// <summary>
        /// The subtype.
        /// </summary>
        public Guid subType;
        /// <summary>
        /// True if fixed size samples; false otherwise.
        /// </summary>
        [MarshalAs(UnmanagedType.Bool)] 
        public bool fixedSizeSamples;
        /// <summary>
        /// True if temporal compression; false otherwise.
        /// </summary>
        [MarshalAs(UnmanagedType.Bool)] 
        public bool temporalCompression;
        /// <summary>
        /// Sample size.
        /// </summary>
        public int sampleSize;
        /// <summary>
        /// Format type.
        /// </summary>
        public Guid formatType;
        /// <summary>
        /// Unknown pointer.
        /// </summary>
        public IntPtr unkPtr;
        /// <summary>
        /// Format size.
        /// </summary>
        public int formatSize;
        /// <summary>
        /// Format pointer.
        /// </summary>
        public IntPtr formatPtr;
    }

    /// <summary>
    /// From PIN_DIRECTION
    /// </summary>
    public enum PinDirection
    {
        /// <summary>
        /// Input pin.
        /// </summary>
        Input,
        /// <summary>
        /// Output pin.
        /// </summary>
        Output
    }

    /// <summary>
    /// From FILTER_STATE
    /// </summary>
    public enum FilterState
    {
        /// <summary>
        /// Filter is stopped.
        /// </summary>
        Stopped,
        /// <summary>
        /// Filter is paused.
        /// </summary>
        Paused,
        /// <summary>
        /// Filter is running.
        /// </summary>
        Running
    }

    /// <summary>
    /// From FILTER_INFO
    /// </summary>
    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode)]
    public struct FilterInfo
    {
        /// <summary>
        /// Filter name.
        /// </summary>
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst=128)] 
        public string achName;
        /// <summary>
        /// The filter graph.
        /// </summary>
        [MarshalAs(UnmanagedType.Interface)] 
        public IFilterGraph pGraph;
    }

    /// <summary>
    /// From REGFILTER
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct RegFilter
    {
        /// <summary>
        /// The filters class ID.
        /// </summary>
        public Guid Clsid;
        /// <summary>
        /// The filters name.
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)]
        public string Name;
    }

    /// <summary>
    /// From REGPINMEDIUM
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public class RegPinMedium
    {
        /// <summary>
        /// The pin medium.
        /// </summary>
        public Guid clsMedium;
        /// <summary>
        /// Parameter 1.
        /// </summary>
        public int dw1;
        /// <summary>
        /// Parameter 2.
        /// </summary>
        public int dw2;
    }

    /// <summary>
    /// From _AM_RENDEREXFLAGS
    /// </summary>
    [Flags]
    public enum AMRenderExFlags
    {
        /// <summary>
        /// No flags.
        /// </summary>
        None = 0,
        /// <summary>
        /// Existing renderer flag.
        /// </summary>
        RenderToExistingRenderers = 1
    }

    /// <summary>
    /// From KSPROPERTY_SUPPORT_* defines
    /// </summary>
    public enum KSPropertySupport
    {
        /// <summary>
        /// Get property.
        /// </summary>
        Get = 1,
        /// <summary>
        /// Set property.
        /// </summary>
        Set = 2
    }

    /// <summary>
    /// From AMTunerModeType
    /// </summary>
    [Flags]
    public enum AMTunerModeType
    {
        /// <summary>
        /// Default mode.
        /// </summary>
        Default = 0x0000,
        /// <summary>
        /// TV mode.
        /// </summary>
        TV = 0x0001,
        /// <summary>
        /// FM radio.
        /// </summary>
        FMRadio = 0x0002,
        /// <summary>
        /// AM radio.
        /// </summary>
        AMRadio = 0x0004,
        /// <summary>
        /// DSS mODE.
        /// </summary>
        Dss = 0x0008,
        /// <summary>
        /// DTV mODE.
        /// </summary>
        DTV = 0x0010
    }

    /// <summary>
    /// From TunerInputType
    /// </summary>
    public enum TunerInputType
    {
        /// <summary>
        /// Tuner uses cable.
        /// </summary>
        Cable,
        /// <summary>
        /// Tuner uses antenna.
        /// </summary>
        Antenna
    }

    #endregion

    #region Interfaces

    /// <summary>
    /// The Pin interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("56a86891-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPin
    {
        /// <summary>
        /// Connect the pin.
        /// </summary>
        /// <param name="pReceivePin">The other pin.</param>
        /// <param name="pmt">The media type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Connect(
            [In] IPin pReceivePin,
            [In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt
            );

        /// <summary>
        /// Receive a connection.
        /// </summary>
        /// <param name="pReceivePin">The pin.</param>
        /// <param name="pmt">The media type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int ReceiveConnection(
            [In] IPin pReceivePin,
            [In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt
            );

        /// <summary>
        /// Disconnect the pin.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Disconnect();

        /// <summary>
        /// Query pin connected to.
        /// </summary>
        /// <param name="ppPin">The other pin.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int ConnectedTo(
            [Out] out IPin ppPin);

        /// <summary>
        /// Get the connection media type.
        /// </summary>
        /// <param name="pmt">The media type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int ConnectionMediaType(
            [Out, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt);

        /// <summary>
        /// Get the pin information.
        /// </summary>
        /// <param name="pInfo">The pin information.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int QueryPinInfo([Out] out PinInfo pInfo);

        /// <summary>
        /// Get the pin direction.
        /// </summary>
        /// <param name="pPinDir">The pin direction.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int QueryDirection(out PinDirection pPinDir);

        /// <summary>
        /// Get the pin ID.
        /// </summary>
        /// <param name="Id">The pin ID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int QueryId([Out, MarshalAs(UnmanagedType.LPWStr)] out string Id);

        /// <summary>
        /// Query the acceptable media type.
        /// </summary>
        /// <param name="pmt">The media type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int QueryAccept([In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt);

        /// <summary>
        /// Enumerate the media types.
        /// </summary>
        /// <param name="ppEnum">The enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int EnumMediaTypes([Out] out IEnumMediaTypes ppEnum);

        /// <summary>
        /// Get the internal connection.s
        /// </summary>
        /// <param name="ppPins">The pins.</param>
        /// <param name="nPin">The pin.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int QueryInternalConnections(
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=1)] IPin[] ppPins,
            [In, Out] ref int nPin
            );

        /// <summary>
        /// Signal end of stream.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int EndOfStream();

        /// <summary>
        /// Signal beginning of flush.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int BeginFlush();

        /// <summary>
        /// Signal end of flush.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int EndFlush();

        /// <summary>
        /// Define a new segment.
        /// </summary>
        /// <param name="tStart">The start time.</param>
        /// <param name="tStop">The stop time.</param>
        /// <param name="dRate">The data rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int NewSegment(
            [In] long tStart,
            [In] long tStop,
            [In] double dRate
            );
    }

    /// <summary>
    /// The MediaFilter interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("56a86899-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IMediaFilter : IPersist
    {
        #region IPersist Methods

        /// <summary>
        /// Get the class ID.
        /// </summary>
        /// <param name="pClassID">The class ID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int GetClassID(
            [Out] out Guid pClassID);

        #endregion

        /// <summary>
        /// Stop.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Stop();

        /// <summary>
        /// Pause.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Pause();

        /// <summary>
        /// Run.
        /// </summary>
        /// <param name="tStart">Time to start.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Run([In] long tStart);

        /// <summary>
        /// Get the filter state.
        /// </summary>
        /// <param name="dwMilliSecsTimeout">The timeout.</param>
        /// <param name="filtState">The filter state.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int GetState(
            [In] int dwMilliSecsTimeout,
            [Out] out FilterState filtState
            );

        /// <summary>
        /// Set the sync source.
        /// </summary>
        /// <param name="pClock">The sync source.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int SetSyncSource([In] IReferenceClock pClock);

        /// <summary>
        /// Get the sync source.
        /// </summary>
        /// <param name="pClock">The sync source.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int GetSyncSource([Out] out IReferenceClock pClock);
    }

    /// <summary>
    /// The base filter interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("56a86895-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IBaseFilter : IMediaFilter
    {
        #region IPersist Methods

        /// <summary>
        /// Get the class ID.
        /// </summary>
        /// <param name="pClassID">The class ID.</param>
        /// <returns></returns>
        [PreserveSig]
        new int GetClassID(
            [Out] out Guid pClassID);

        #endregion

        #region IMediaFilter Methods

        /// <summary>
        /// Stop the graph.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Stop();

        /// <summary>
        /// Pause the graph.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Pause();

        /// <summary>
        /// Run the graph.
        /// </summary>
        /// <param name="tStart">The start point.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Run(long tStart);

        /// <summary>
        /// Get the filter state.
        /// </summary>
        /// <param name="dwMilliSecsTimeout">The timeout.</param>
        /// <param name="filtState">The filter state.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int GetState([In] int dwMilliSecsTimeout, [Out] out FilterState filtState);

        /// <summary>
        /// Set the sync source.
        /// </summary>
        /// <param name="pClock">The clock to use.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int SetSyncSource([In] IReferenceClock pClock);

        /// <summary>
        /// Get the sync source.
        /// </summary>
        /// <param name="pClock">The clock.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int GetSyncSource([Out] out IReferenceClock pClock);

        #endregion

        /// <summary>
        /// Enumerate the pins.
        /// </summary>
        /// <param name="ppEnum">The enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int EnumPins([Out] out IEnumPins ppEnum);

        /// <summary>
        /// Find a pin.
        /// </summary>
        /// <param name="Id">The identity.</param>
        /// <param name="ppPin">The pin.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int FindPin(
            [In, MarshalAs(UnmanagedType.LPWStr)] string Id,
            [Out] out IPin ppPin
            );

        /// <summary>
        /// Get the filter info.
        /// </summary>
        /// <param name="pInfo">The filter info.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int QueryFilterInfo([Out] out FilterInfo pInfo);

        /// <summary>
        /// Join the filter graph.
        /// </summary>
        /// <param name="pGraph">The graph.</param>
        /// <param name="pName">The name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int JoinFilterGraph(
            [In] IFilterGraph pGraph,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pName
            );

        /// <summary>
        /// Get the vendor info.
        /// </summary>
        /// <param name="pVendorInfo">The vendor info.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int QueryVendorInfo([Out, MarshalAs(UnmanagedType.LPWStr)] out string pVendorInfo);
    }

    /// <summary>
    /// The filter graph interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("56a8689f-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IFilterGraph
    {
        /// <summary>
        /// Add a filter.
        /// </summary>
        /// <param name="pFilter">The filter.</param>
        /// <param name="pName">The filter name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int AddFilter(
            [In] IBaseFilter pFilter,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pName
            );

        /// <summary>
        /// Remove a filter.
        /// </summary>
        /// <param name="pFilter">The filter.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int RemoveFilter([In] IBaseFilter pFilter);

        /// <summary>
        /// Enumerate the filters.
        /// </summary>
        /// <param name="ppEnum">The enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int EnumFilters([Out] out IEnumFilters ppEnum);

        /// <summary>
        /// Find a filter by name.
        /// </summary>
        /// <param name="pName">The name.</param>
        /// <param name="ppFilter">The filter.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int FindFilterByName(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pName,
            [Out] out IBaseFilter ppFilter
            );

        /// <summary>
        /// Directly connect pins.
        /// </summary>
        /// <param name="ppinOut">The output pin.</param>
        /// <param name="ppinIn">The input pin.</param>
        /// <param name="pmt">The media type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int ConnectDirect(
            [In] IPin ppinOut,
            [In] IPin ppinIn,
            [In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt
            );

        /// <summary>
        /// Reconnect a pin.
        /// </summary>
        /// <param name="ppin">The pin.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        [Obsolete("This method is obsolete; use the IFilterGraph2.ReconnectEx method instead.")]
        int Reconnect([In] IPin ppin);

        /// <summary>
        /// Disconnect a pin.
        /// </summary>
        /// <param name="ppin">The pin.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Disconnect([In] IPin ppin);

        /// <summary>
        /// Set the default sync source.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int SetDefaultSyncSource();
    }

    /// <summary>
    /// The EnumFilters interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("56a86893-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumFilters
    {
        /// <summary>
        /// Get the next entry.
        /// </summary>
        /// <param name="cFilters">The count of possible filters.</param>
        /// <param name="ppFilter">The next filter.</param>
        /// <param name="pcFetched">The filter count.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Next(
            [In] int cFilters,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] IBaseFilter[] ppFilter,
            [In] IntPtr pcFetched
            );

        /// <summary>
        /// Skip an entry.
        /// </summary>
        /// <param name="cFilters">The count of filters to skip.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Skip([In] int cFilters);

        /// <summary>
        /// Reset the enumerator.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Reset();

        /// <summary>
        /// Clone the enumerator.
        /// </summary>
        /// <param name="ppEnum">The cloned enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Clone([Out] out IEnumFilters ppEnum);
    }

    /// <summary>
    /// The EnumPins interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("56a86892-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumPins
    {
        /// <summary>
        /// Get the next entry.
        /// </summary>
        /// <param name="cPins">The maximum count to return.</param>
        /// <param name="ppPins">The entyries returned.</param>
        /// <param name="pcFetched">The count of entries returned.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Next(
            [In] int cPins,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex=0)] IPin[] ppPins,
            [In] IntPtr pcFetched
            );

        /// <summary>
        /// Skip entries.
        /// </summary>
        /// <param name="cPins">The number of entries to skip.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Skip([In] int cPins);

        /// <summary>
        /// Reset the enumerator.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Reset();

        /// <summary>
        /// Clone the enumerator.
        /// </summary>
        /// <param name="ppEnum">The cloned enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Clone([Out] out IEnumPins ppEnum);
    }

    /// <summary>
    /// The reference clock interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("56a86897-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IReferenceClock
    {
        /// <summary>
        /// Get the time.
        /// </summary>
        /// <param name="pTime">The time.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>     
        [PreserveSig]
        int GetTime([Out] out long pTime);

        /// <summary>
        /// Advise the time.
        /// </summary>
        /// <param name="baseTime">The base time.</param>
        /// <param name="streamTime">The stream time.</param>
        /// <param name="hEvent">The event.</param>
        /// <param name="pdwAdviseCookie">The cookie.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int AdviseTime(
            [In] long baseTime,
            [In] long streamTime,
            [In] IntPtr hEvent, // System.Threading.WaitHandle?
            [Out] out int pdwAdviseCookie
            );

        /// <summary>
        /// Set up a periodic advise.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="periodTime">The period time.</param>
        /// <param name="hSemaphore">The semaphore handle.</param>
        /// <param name="pdwAdviseCookie">The cookie.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int AdvisePeriodic(
            [In] long startTime,
            [In] long periodTime,
            [In] IntPtr hSemaphore, // System.Threading.WaitHandle?
            [Out] out int pdwAdviseCookie
            );

        /// <summary>
        /// Remove an advise.
        /// </summary>
        /// <param name="dwAdviseCookie">The cookie.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Unadvise([In] int dwAdviseCookie);
    }

    /// <summary>
    /// The EnumMediaTypes interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("89c31040-846b-11ce-97d3-00aa0055595a"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumMediaTypes
    {
        /// <summary>
        /// Get the next entry.
        /// </summary>
        /// <param name="cMediaTypes">The maximum count to return.</param>
        /// <param name="ppMediaTypes">The media types returned.</param>
        /// <param name="pcFetched">The count returned.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Next(
            [In] int cMediaTypes,
            [In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef=typeof(EMTMarshaler), SizeParamIndex = 0)] AMMediaType[] ppMediaTypes,
            [In] IntPtr pcFetched
            );

        /// <summary>
        /// Skip entries.
        /// </summary>
        /// <param name="cMediaTypes">The number to skip.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Skip([In] int cMediaTypes);

        /// <summary>
        /// Reset the enumerator.
        /// </summary>
        /// <returns></returns>
        [PreserveSig]
        int Reset();

        /// <summary>
        /// Clone the enumerator.
        /// </summary>
        /// <param name="ppEnum">The cloned enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Clone([Out] out IEnumMediaTypes ppEnum);
    }

    /// <summary>
    /// The EnumRegFilters interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("56a868a4-0ad4-11ce-b03a-0020af0ba770"),
    Obsolete("This interface has been deprecated.  Use IFilterMapper2::EnumMatchingFilters", false),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumRegFilters
    {
        /// <summary>
        /// Get the next entry.
        /// </summary>
        /// <param name="cFilters">The maximum count to return.</param>
        /// <param name="apRegFilter">The entries returned.</param>
        /// <param name="pcFetched">The count returned.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Next(
            [In] int cFilters,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] RegFilter[] apRegFilter,
            [In] IntPtr pcFetched
            );

        /// <summary>
        /// Skip entries.
        /// </summary>
        /// <param name="cFilters">The count to skip.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Skip([In] int cFilters);

        /// <summary>
        /// Reset the enumerator.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Reset();

        /// <summary>
        /// Clone the enumerator.
        /// </summary>
        /// <param name="ppEnum">The cloned enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Clone([Out] out IEnumRegFilters ppEnum);
    }

    /// <summary>
    /// The GraphBuilder interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("56a868a9-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IGraphBuilder : IFilterGraph
    {
        #region IFilterGraph Methods

        /// <summary>
        /// Add a filter.
        /// </summary>
        /// <param name="pFilter">The filter.</param>
        /// <param name="pName">The filter name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int AddFilter(
            [In] IBaseFilter pFilter,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pName
            );

        /// <summary>
        /// Remove a filter.
        /// </summary>
        /// <param name="pFilter">The filter.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int RemoveFilter([In] IBaseFilter pFilter);

        /// <summary>
        /// Enumerate the filters.
        /// </summary>
        /// <param name="ppEnum">The enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int EnumFilters([Out] out IEnumFilters ppEnum);

        /// <summary>
        /// Find a filter by name.
        /// </summary>
        /// <param name="pName">The name.</param>
        /// <param name="ppFilter">The filter.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int FindFilterByName(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pName,
            [Out] out IBaseFilter ppFilter
            );

        /// <summary>
        /// Connect filters directly.
        /// </summary>
        /// <param name="ppinOut">The output pin.</param>
        /// <param name="ppinIn">The input pin.</param>
        /// <param name="pmt">The media type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int ConnectDirect(
            [In] IPin ppinOut,
            [In] IPin ppinIn,
            [In, MarshalAs(UnmanagedType.LPStruct)]
            AMMediaType pmt
            );

        /// <summary>
        /// Reconnect pins.
        /// </summary>
        /// <param name="ppin">The pin.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Reconnect([In] IPin ppin);

        /// <summary>
        /// Disconnect a pin.
        /// </summary>
        /// <param name="ppin">The pin.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Disconnect([In] IPin ppin);

        /// <summary>
        /// Set the default sync source.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int SetDefaultSyncSource();

        #endregion

        /// <summary>
        /// Connect pins.
        /// </summary>
        /// <param name="ppinOut">The output pin.</param>
        /// <param name="ppinIn">The input pin.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Connect(
            [In] IPin ppinOut,
            [In] IPin ppinIn
            );

        /// <summary>
        /// Render a pin.
        /// </summary>
        /// <param name="ppinOut">The pin.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Render([In] IPin ppinOut);

        /// <summary>
        /// Render a file.
        /// </summary>
        /// <param name="lpcwstrFile">The file.</param>
        /// <param name="lpcwstrPlayList">The playlist.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int RenderFile(
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFile,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrPlayList
            );

        /// <summary>
        /// Add a source filter.
        /// </summary>
        /// <param name="lpcwstrFileName">The filename.</param>
        /// <param name="lpcwstrFilterName">The filter name.</param>
        /// <param name="ppFilter">The filter.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int AddSourceFilter(
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFileName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFilterName,
            [Out] out IBaseFilter ppFilter
            );

        /// <summary>
        /// Set the log file.
        /// </summary>
        /// <param name="hFile">The file.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int SetLogFile(IntPtr hFile); // DWORD_PTR

        /// <summary>
        /// Abort.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Abort();

        /// <summary>
        /// Check if operation should continue.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int ShouldOperationContinue();
    }

    /// <summary>
    /// The FilterGraph2 interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("36b73882-c2c8-11cf-8b46-00805f6cef60"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IFilterGraph2 : IGraphBuilder
    {
        #region IFilterGraph Methods

        /// <summary>
        /// Add a filter.
        /// </summary>
        /// <param name="pFilter">The filter.</param>
        /// <param name="pName">The filter name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int AddFilter(
            [In] IBaseFilter pFilter,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pName
            );

        /// <summary>
        /// Remove a filter.
        /// </summary>
        /// <param name="pFilter">The filter.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int RemoveFilter([In] IBaseFilter pFilter);

        /// <summary>
        /// Enumerate the filters.
        /// </summary>
        /// <param name="ppEnum">The enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int EnumFilters([Out] out IEnumFilters ppEnum);

        /// <summary>
        /// Find a filter by name.
        /// </summary>
        /// <param name="pName">The filter name.</param>
        /// <param name="ppFilter">The filter.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int FindFilterByName(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pName,
            [Out] out IBaseFilter ppFilter
            );

        /// <summary>
        /// Connect pins directly.
        /// </summary>
        /// <param name="ppinOut">The output pin.</param>
        /// <param name="ppinIn">The input pin.</param>
        /// <param name="pmt">The media type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int ConnectDirect(
            [In] IPin ppinOut,
            [In] IPin ppinIn,
            [In, MarshalAs(UnmanagedType.LPStruct)]
            AMMediaType pmt
            );

        /// <summary>
        /// Reconnect a pin.
        /// </summary>
        /// <param name="ppin">The pin.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Reconnect([In] IPin ppin);

        /// <summary>
        /// Disconnect a pin.
        /// </summary>
        /// <param name="ppin">The pin.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Disconnect([In] IPin ppin);

        /// <summary>
        /// Set the default sync source.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int SetDefaultSyncSource();

        #endregion

        #region IGraphBuilder Method

        /// <summary>
        /// Connect pins.
        /// </summary>
        /// <param name="ppinOut">The output pin.</param>
        /// <param name="ppinIn">The input pins.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Connect(
            [In] IPin ppinOut,
            [In] IPin ppinIn
            );

        /// <summary>
        /// Render a pin.
        /// </summary>
        /// <param name="ppinOut">The pin.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Render([In] IPin ppinOut);

        /// <summary>
        /// Render a file.
        /// </summary>
        /// <param name="lpcwstrFile">The file.</param>
        /// <param name="lpcwstrPlayList">The playlist.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int RenderFile(
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFile,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrPlayList
            );

        /// <summary>
        /// Add source filter.
        /// </summary>
        /// <param name="lpcwstrFileName">The filename.</param>
        /// <param name="lpcwstrFilterName">The filter name.</param>
        /// <param name="ppFilter">The filter.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int AddSourceFilter(
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFileName,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFilterName,
            [Out] out IBaseFilter ppFilter
            );

        /// <summary>
        /// Set the log file.
        /// </summary>
        /// <param name="hFile">The file.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int SetLogFile(IntPtr hFile); // DWORD_PTR

        /// <summary>
        /// Abort.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Abort();

        /// <summary>
        /// Check if operation should continue.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int ShouldOperationContinue();

        #endregion

        /// <summary>
        /// Add a source filter from a moniker.
        /// </summary>
        /// <param name="pMoniker">The moniker.</param>
        /// <param name="pCtx">The context.</param>
        /// <param name="lpcwstrFilterName">The filter name.</param>
        /// <param name="ppFilter">The filter.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int AddSourceFilterForMoniker(
            [In] IMoniker pMoniker,
            [In] IBindCtx pCtx,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFilterName,
            [Out] out IBaseFilter ppFilter
            );

        /// <summary>
        /// Reconnect a pin.
        /// </summary>
        /// <param name="ppin">The pin.</param>
        /// <param name="pmt">The media type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int ReconnectEx(
            [In] IPin ppin,
            [In] AMMediaType pmt
            );

        /// <summary>
        /// Render a pin.
        /// </summary>
        /// <param name="pPinOut">The pin.</param>
        /// <param name="dwFlags">Flags.</param>
        /// <param name="pvContext">The context.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int RenderEx(
            [In] IPin pPinOut,
            [In] AMRenderExFlags dwFlags,
            [In] IntPtr pvContext // DWORD *
            );
    }

    /// <summary>
    /// The FileSinkFilter interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("a2104830-7c70-11cf-8bce-00aa00a3f1a6"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IFileSinkFilter
    {
        /// <summary>
        /// Set the filename.
        /// </summary>
        /// <param name="pszFileName">The filename.</param>
        /// <param name="pmt">The media type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int SetFileName(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
            [In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt
            );

        /// <summary>
        /// Get the current file.
        /// </summary>
        /// <param name="pszFileName">The filename.</param>
        /// <param name="pmt">The media type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int GetCurFile(
            [Out, MarshalAs(UnmanagedType.LPWStr)] out string pszFileName,
            [Out, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt
            );
    }

    /// <summary>
    /// The FileSourceFilter interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("56a868a6-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IFileSourceFilter
    {
        /// <summary>
        /// Load the file.
        /// </summary>
        /// <param name="pszFileName">The file.</param>
        /// <param name="pmt">The media type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Load(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
            [In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt
            );

        /// <summary>
        /// Get the current file.
        /// </summary>
        /// <param name="pszFileName">The filename.</param>
        /// <param name="pmt">The media type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int GetCurFile(
            [Out, MarshalAs(UnmanagedType.LPWStr)] out string pszFileName,
            [Out, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt
            );
    }

    /// <summary>
    /// The KsPropertySet interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("31EFAC30-515C-11d0-A9AA-00AA0061BE93"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IKsPropertySet
    {
        /// <summary>
        /// Set a property.
        /// </summary>
        /// <param name="guidPropSet">The property set.</param>
        /// <param name="dwPropID">The property set ID.</param>
        /// <param name="pInstanceData">Instance data.</param>
        /// <param name="cbInstanceData">The length of the instancec data.</param>
        /// <param name="pPropData">Property data.</param>
        /// <param name="cbPropData">The length of the property data.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Set(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidPropSet,
            [In] int dwPropID,
            [In] IntPtr pInstanceData,
            [In] int cbInstanceData,
            [In] IntPtr pPropData,
            [In] int cbPropData
            );

        /// <summary>
        /// Get a property.
        /// </summary>
        /// <param name="guidPropSet">The property set.</param>
        /// <param name="dwPropID">The property ID.</param>
        /// <param name="pInstanceData">Instance data.</param>
        /// <param name="cbInstanceData">The length of the instance data.</param>
        /// <param name="pPropData">Property data.</param>
        /// <param name="cbPropData">The length of the property data.</param>
        /// <param name="pcbReturned">Count of bytes returned.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Get(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidPropSet,
            [In] int dwPropID,
            [In] IntPtr pInstanceData,
            [In] int cbInstanceData,
            [In, Out] IntPtr pPropData,
            [In] int cbPropData,
            [Out] out int pcbReturned
            );

        /// <summary>
        /// Query if property supported.
        /// </summary>
        /// <param name="guidPropSet">The property set.</param>
        /// <param name="dwPropID">The property ID.</param>
        /// <param name="pTypeSupport">The response.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int QuerySupported(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid guidPropSet,
            [In] int dwPropID,
            [Out] out KSPropertySupport pTypeSupport
            );
    }

    /// <summary>
    /// Capture file progress interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("670d1d20-a068-11d0-b3f0-00aa003761c5"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IAMCopyCaptureFileProgress
    {
        /// <summary>
        /// Progress.
        /// </summary>
        /// <param name="iProgress">Progress percentage.</param>
        /// <returns>Update progress.</returns>
        [PreserveSig]
        int Progress(int iProgress);
    }

    /// <summary>
    /// Capture graph builder 2 interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("93E5A4E0-2D50-11d2-ABFA-00A0C9C6E38D"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ICaptureGraphBuilder2
    {
        /// <summary>
        /// Set the filter graph.
        /// </summary>
        /// <param name="pfg">The filter graph.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int SetFiltergraph([In] IGraphBuilder pfg);

        /// <summary>
        /// Get the filter graph.
        /// </summary>
        /// <param name="ppfg">The filter graph.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int GetFiltergraph([Out] out IGraphBuilder ppfg);

        /// <summary>
        /// Set the output filename.
        /// </summary>
        /// <param name="pType">The type.</param>
        /// <param name="lpstrFile">The file.</param>
        /// <param name="ppbf">?</param>
        /// <param name="ppSink">The sink.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int SetOutputFileName(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid pType,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpstrFile,
            [Out] out IBaseFilter ppbf,
            [Out] out IFileSinkFilter ppSink
            );

        /// <summary>
        /// Find an interface.
        /// </summary>
        /// <param name="pCategory">The category.</param>
        /// <param name="pType">The type.</param>
        /// <param name="pbf">?</param>
        /// <param name="riid">The identifier.</param>
        /// <param name="ppint">The interface.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int FindInterface(
            [In, MarshalAs(UnmanagedType.LPStruct)] DsGuid pCategory,
            [In, MarshalAs(UnmanagedType.LPStruct)] DsGuid pType,
            [In] IBaseFilter pbf,
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
            [Out, MarshalAs(UnmanagedType.IUnknown)] out object ppint
            );

        /// <summary>
        /// Render a stream.
        /// </summary>
        /// <param name="PinCategory">The pin category.</param>
        /// <param name="MediaType">The media type.</param>
        /// <param name="pSource">The source.</param>
        /// <param name="pfCompressor">The stream compressor.</param>
        /// <param name="pfRenderer">The renderer.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int RenderStream(
            [In, MarshalAs(UnmanagedType.LPStruct)] DsGuid PinCategory,
            [In, MarshalAs(UnmanagedType.LPStruct)] DsGuid MediaType,
            [In, MarshalAs(UnmanagedType.IUnknown)] object pSource,
            [In] IBaseFilter pfCompressor,
            [In] IBaseFilter pfRenderer
            );

        /// <summary>
        /// Control a stream.
        /// </summary>
        /// <param name="PinCategory">The pin category.</param>
        /// <param name="MediaType">The media type.</param>
        /// <param name="pFilter">The filter.</param>
        /// <param name="pstart">The start point.</param>
        /// <param name="pstop">The stop point.</param>
        /// <param name="wStartCookie">The start cookie.</param>
        /// <param name="wStopCookie">The stop cookie.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int ControlStream(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid PinCategory,
            [In, MarshalAs(UnmanagedType.LPStruct)] DsGuid MediaType,
            [In, MarshalAs(UnmanagedType.Interface)] IBaseFilter pFilter,
            [In] DsLong pstart,
            [In] DsLong pstop,
            [In] short wStartCookie,
            [In] short wStopCookie
            );

        /// <summary>
        /// Allocate capture file.
        /// </summary>
        /// <param name="lpstrFile">The file name.</param>
        /// <param name="dwlSize">The size.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int AllocCapFile(
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpstrFile,
            [In] long dwlSize
            );

        /// <summary>
        /// Copy capture file.
        /// </summary>
        /// <param name="lpwstrOld">The old file.</param>
        /// <param name="lpwstrNew">The new file.</param>
        /// <param name="fAllowEscAbort">Flag to allow Esc to abort.</param>
        /// <param name="pFilter">The capture filter.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int CopyCaptureFile(
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpwstrOld,
            [In, MarshalAs(UnmanagedType.LPWStr)] string lpwstrNew,
            [In, MarshalAs(UnmanagedType.Bool)] bool fAllowEscAbort,
            [In] IAMCopyCaptureFileProgress pFilter
            );

        /// <summary>
        /// Find a pin.
        /// </summary>
        /// <param name="pSource">The source filter.</param>
        /// <param name="pindir">The pin direction.</param>
        /// <param name="PinCategory">The pin category.</param>
        /// <param name="MediaType">The media type.</param>
        /// <param name="fUnconnected">Flag for unconnected.</param>
        /// <param name="ZeroBasedIndex">The index.</param>
        /// <param name="ppPin">The pin.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int FindPin(
            [In, MarshalAs(UnmanagedType.IUnknown)] object pSource,
            [In] PinDirection pindir,
            [In, MarshalAs(UnmanagedType.LPStruct)] DsGuid PinCategory,
            [In, MarshalAs(UnmanagedType.LPStruct)] DsGuid MediaType,
            [In, MarshalAs(UnmanagedType.Bool)] bool fUnconnected,
            [In] int ZeroBasedIndex,
            [Out, MarshalAs(UnmanagedType.Interface)] out IPin ppPin
            );
    }

    #endregion
}
