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

namespace DirectShowAPI
{
    #region Declarations

    /// <summary>
    /// From BDA_CHANGE_STATE
    /// </summary>
    public enum BDAChangeState
    {
        /// <summary>
        /// Changes are complete.
        /// </summary>
        ChangesComplete = 0,
        /// <summary>
        /// Changes still pending.
        /// </summary>
        ChangesPending
    }

    /// <summary>
    /// From BDANODE_DESCRIPTOR
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BDANodeDescriptor
    {
        /// <summary>
        /// Node type.
        /// </summary>
        public int ulBdaNodeType;
        /// <summary>
        /// Node function.
        /// </summary>
        public Guid guidFunction;
        /// <summary>
        /// Node name.
        /// </summary>
        public Guid guidName;
    }

    #endregion

    #region Interfaces

    /// <summary>
    /// Media control interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("56a868b1-0ad4-11ce-b03a-0020af0ba770"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IMediaControl
    {
        /// <summary>
        /// Run the graph.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Run();

        /// <summary>
        /// Pause the graph.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Pause();

        /// <summary>
        /// Stop the graph.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Stop();

        /// <summary>
        /// Get the graph state.
        /// </summary>
        /// <param name="msTimeout">Timeout in milliseconds.</param>
        /// <param name="pfs">Pointer to FilterState structure for reply.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int GetState([In] int msTimeout, [Out] out FilterState pfs);

        /// <summary>
        /// Stop the graph when ready.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int StopWhenReady();
    }

    /// <summary>
    /// The persistance interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("0000010c-0000-0000-C000-000000000046"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPersist
    {
        /// <summary>
        /// Get the class ID.
        /// </summary>
        /// <param name="pClassID">The returned class ID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int GetClassID(
            [Out] out Guid pClassID);
    }

    /// <summary>
    /// The KsPin interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("b61178d1-a2d9-11cf-9e53-00aa00a216a1"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IKsPin
    {
        /// <summary>
        /// Query the pin mediums.
        /// </summary>
        /// <param name="ip">A pointer to the medium list.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int KsQueryMediums(
            out IntPtr ip);
    }

    /// <summary>
    /// The property bag interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("55272A00-42CB-11CE-8135-00AA004BB851"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IPropertyBag
    {
        /// <summary>
        /// Read the property.
        /// </summary>
        /// <param name="pszPropName">The propert name.</param>
        /// <param name="pVar">The returned property.</param>
        /// <param name="pErrorLog">The error log interface.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Read(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszPropName,
            [Out, MarshalAs(UnmanagedType.Struct)] out object pVar,
            [In] IErrorLog pErrorLog
            );

        /// <summary>
        /// Set the property.
        /// </summary>
        /// <param name="pszPropName">The property name.</param>
        /// <param name="pVar">The value.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Write(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszPropName,
            [In, MarshalAs(UnmanagedType.Struct)] ref object pVar
            );
    }

    /// <summary>
    /// The error log interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("3127CA40-446E-11CE-8135-00AA004BB851"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IErrorLog
    {
        /// <summary>
        /// Add an error.
        /// </summary>
        /// <param name="pszPropName">The property name.</param>
        /// <param name="pExcepInfo">Exception information.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int AddError(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszPropName,
            [In] System.Runtime.InteropServices.ComTypes.EXCEPINFO pExcepInfo);
    }

    /// <summary>
    /// The BDA DiSEqC command interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("F84E2AB0-3C6B-45E3-A0FC-8669D4B81F11"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IBDA_DiseqCommand
    {
        /// <summary>
        /// Enable or disable commands.
        /// </summary>
        /// <param name="bEnable">Enable or disable flag.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_EnableDiseqCommands(
            [In] byte bEnable
            );

        /// <summary>
        /// Set the LNB source.
        /// </summary>
        /// <param name="ulLNBSource">The LNB source.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_DiseqLNBSource(
            [In] int ulLNBSource
            );

       /// <summary>
       /// Enable or disable tone burst.
       /// </summary>
       /// <param name="bUseToneBurst">Enable or disable flag.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_DiseqUseToneBurst(
            [In] byte bUseToneBurst
            );

        /// <summary>
        /// Set the number of repeats.
        /// </summary>
        /// <param name="ulRepeats">The number of repeats.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_DiseqRepeats(
            [In] int ulRepeats
            );

        /// <summary>
        /// Send a command.
        /// </summary>
        /// <param name="ulRequestId">The request identifier.</param>
        /// <param name="ulcbCommandLen">The command length.</param>
        /// <param name="pbCommand">The command.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_DiseqSendCommand(
            [In] int ulRequestId,
            [In] int ulcbCommandLen,
            [In] ref byte pbCommand
            );

        /// <summary>
        /// Get a response.
        /// </summary>
        /// <param name="ulRequestId">The request identifier.</param>
        /// <param name="pulcbResponseLen">The maximum response length.</param>
        /// <param name="pbResponse">The reasponse.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_DiseqResponse(
            [In] int ulRequestId,
            [In, Out] ref int pulcbResponseLen,
            [In, Out] ref byte pbResponse
            );
    }

    /// <summary>
    /// The BDA digital demodulator interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("EF30F379-985B-4d10-B640-A79D5E04E1E0"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IBDA_DigitalDemodulator
    {
        /// <summary>
        /// Set the modulation type.
        /// </summary>
        /// <param name="pModulationType">The modulation type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_ModulationType([In] ref ModulationType pModulationType);

        /// <summary>
        /// Get the modulation type.
        /// </summary>
        /// <param name="pModulationType">The modulation type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_ModulationType([Out] out ModulationType pModulationType);

        /// <summary>
        /// Set the inner FEC method.
        /// </summary>
        /// <param name="pFECMethod">The method.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_InnerFECMethod([In] ref FECMethod pFECMethod);

        /// <summary>
        /// Get the inner FEC method.
        /// </summary>
        /// <param name="pFECMethod">The method.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_InnerFECMethod([Out] out FECMethod pFECMethod);

        /// <summary>
        /// Set the inner FEC rate.
        /// </summary>
        /// <param name="pFECRate">The rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_InnerFECRate([In] ref BinaryConvolutionCodeRate pFECRate);

        /// <summary>
        /// Get the inner FEC rate.
        /// </summary>
        /// <param name="pFECRate">The rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_InnerFECRate([Out] out BinaryConvolutionCodeRate pFECRate);

        /// <summary>
        /// Set the outer FEC method.
        /// </summary>
        /// <param name="pFECMethod">The method.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_OuterFECMethod([In] ref FECMethod pFECMethod);

        /// <summary>
        /// Get the outer FEC method.
        /// </summary>
        /// <param name="pFECMethod">The method.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_OuterFECMethod([Out] out FECMethod pFECMethod);

        /// <summary>
        /// Set the outer FEC rate.
        /// </summary>
        /// <param name="pFECRate">The rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_OuterFECRate([In] ref BinaryConvolutionCodeRate pFECRate);

        /// <summary>
        /// Get the outer FEC rate.
        /// </summary>
        /// <param name="pFECRate">The rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_OuterFECRate([Out] out BinaryConvolutionCodeRate pFECRate);

        /// <summary>
        /// Set the symbol rate.
        /// </summary>
        /// <param name="pSymbolRate">The symbol rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_SymbolRate([In] ref int pSymbolRate);

        /// <summary>
        /// Get the symbol rate.
        /// </summary>
        /// <param name="pSymbolRate">The symbol rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_SymbolRate([Out] out int pSymbolRate);

        /// <summary>
        /// Put the spectral inversion.
        /// </summary>
        /// <param name="pSpectralInversion">The spectral inversion.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_SpectralInversion([In] ref SpectralInversion pSpectralInversion);

        /// <summary>
        /// Get the spectral inverson.
        /// </summary>
        /// <param name="pSpectralInversion">The spectral inversion.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_SpectralInversion([Out] out SpectralInversion pSpectralInversion);
    }

    /// <summary>
    /// The bda DIGITAL DEMODULATOR 2 INTERFACE.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown),
    Guid("525ED3EE-5CF3-4E1E-9A06-5368A84F9A6E")]
    public interface IBDA_DigitalDemodulator2 : IBDA_DigitalDemodulator
    {
        #region IBDA_DigitalDemodulator Methods

        /// <summary>
        /// Set the modulation type.
        /// </summary>
        /// <param name="pModulationType">The modulation type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_ModulationType(
            [In] ref ModulationType pModulationType
            );

        /// <summary>
        ///  Get the modulation type.
        /// </summary>
        /// <param name="pModulationType">The modulation type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_ModulationType(
            [Out] out ModulationType pModulationType
            );

        /// <summary>
        /// Set the inner FEC method.
        /// </summary>
        /// <param name="pFECMethod">The method.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_InnerFECMethod(
            [In] ref FECMethod pFECMethod
            );

        /// <summary>
        /// Get the inner FEC method.
        /// </summary>
        /// <param name="pFECMethod">The method.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_InnerFECMethod(
            [Out] out FECMethod pFECMethod
            );

        /// <summary>
        /// Set the inner FEC rate.
        /// </summary>
        /// <param name="pFECRate">The rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_InnerFECRate(
            [In] ref BinaryConvolutionCodeRate pFECRate
            );

        /// <summary>
        /// Get the inner FEC rate.
        /// </summary>
        /// <param name="pFECRate">The rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_InnerFECRate(
            [Out] out BinaryConvolutionCodeRate pFECRate
            );

        /// <summary>
        /// Set the outer FEC method.
        /// </summary>
        /// <param name="pFECMethod">The method.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_OuterFECMethod(
            [In] ref FECMethod pFECMethod
            );

        /// <summary>
        /// Get the outer FEC method.
        /// </summary>
        /// <param name="pFECMethod">The method.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_OuterFECMethod(
            [Out] out FECMethod pFECMethod
            );

        /// <summary>
        /// Set the outer FEC rate.
        /// </summary>
        /// <param name="pFECRate">The rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_OuterFECRate(
            [In] ref BinaryConvolutionCodeRate pFECRate
            );

        /// <summary>
        /// Get the outer FEC rate.
        /// </summary>
        /// <param name="pFECRate">The rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_OuterFECRate(
            [Out] out BinaryConvolutionCodeRate pFECRate
            );

        /// <summary>
        /// Set the symbol rate.
        /// </summary>
        /// <param name="pSymbolRate">The rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_SymbolRate(
            [In] ref int pSymbolRate
            );

        /// <summary>
        /// Get the symbol rate.
        /// </summary>
        /// <param name="pSymbolRate">The rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_SymbolRate(
            [Out] out int pSymbolRate
            );

        /// <summary>
        /// Set the spectral inversion.
        /// </summary>
        /// <param name="pSpectralInversion">The spectral inversion.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_SpectralInversion(
            [In] ref SpectralInversion pSpectralInversion
            );

        /// <summary>
        /// Get the spectral inversion.
        /// </summary>
        /// <param name="pSpectralInversion">The spectral inversion.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_SpectralInversion(
            [Out] out SpectralInversion pSpectralInversion
            );

        #endregion

        
        /// <summary>
        /// Set the guard interval.
        /// </summary>
        /// <param name="pGuardInterval">The guard interval.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_GuardInterval(
            [In] ref GuardInterval pGuardInterval
            );

        /// <summary>
        /// Get the guard interval.
        /// </summary>
        /// <param name="pGuardInterval">The guard interval.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_GuardInterval(
            [In, Out] ref GuardInterval pGuardInterval
            );

        /// <summary>
        /// Set the transmission mode.
        /// </summary>
        /// <param name="pTransmissionMode">The mode.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_TransmissionMode(
            [In] ref TransmissionMode pTransmissionMode
            );

        /// <summary>
        /// Get the transmission mode.
        /// </summary>
        /// <param name="pTransmissionMode">The mode.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_TransmissionMode(
            [In, Out] ref TransmissionMode pTransmissionMode
            );

        /// <summary>
        /// Set the roll off.
        /// </summary>
        /// <param name="pRollOff">The roll off.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_RollOff(
            [In] ref RollOff pRollOff
            );

        /// <summary>
        /// Get the roll off.
        /// </summary>
        /// <param name="pRollOff">The roll off.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_RollOff(
            [In, Out] ref RollOff pRollOff
            );

        /// <summary>
        /// Set the pilot.
        /// </summary>
        /// <param name="pPilot">The pilot.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_Pilot(
            [In] ref Pilot pPilot
            );

        /// <summary>
        /// Get the pilot.
        /// </summary>
        /// <param name="pPilot">The pilot.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_Pilot(
            [In, Out] ref Pilot pPilot
            );
    }

    /// <summary>
    /// The BDA signal statistics.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("1347D106-CF3A-428a-A5CB-AC0D9A2A4338"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IBDA_SignalStatistics
    {
        /// <summary>
        /// Set the signal strength.
        /// </summary>
        /// <param name="lDbStrength">The strength.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_SignalStrength([In] int lDbStrength);

        /// <summary>
        /// Get the signal strength.
        /// </summary>
        /// <param name="plDbStrength">The signal strength.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_SignalStrength([Out] out int plDbStrength);

        /// <summary>
        /// Set the signal quality.
        /// </summary>
        /// <param name="lPercentQuality">The signal quality.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_SignalQuality([In] int lPercentQuality);

        /// <summary>
        /// Get the signal quality.
        /// </summary>
        /// <param name="plPercentQuality">The signal quality.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_SignalQuality([Out] out int plPercentQuality);

        /// <summary>
        /// Set the signal present flag.
        /// </summary>
        /// <param name="fPresent">The flag.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_SignalPresent([In, MarshalAs(UnmanagedType.U1)] bool fPresent);

        /// <summary>
        /// Get the signal present flag.
        /// </summary>
        /// <param name="pfPresent">The flag.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_SignalPresent([Out, MarshalAs(UnmanagedType.U1)] out bool pfPresent);

        /// <summary>
        /// Set the signal locked flag.
        /// </summary>
        /// <param name="fLocked">The flag.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_SignalLocked([In, MarshalAs(UnmanagedType.U1)] bool fLocked);

        /// <summary>
        /// Get the signal locked flag.
        /// </summary>
        /// <param name="pfLocked">The flag.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_SignalLocked([Out, MarshalAs(UnmanagedType.U1)] out bool pfLocked);

        /// <summary>
        /// Set the sample time.
        /// </summary>
        /// <param name="lmsSampleTime">The sample time.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_SampleTime([In] int lmsSampleTime);

        /// <summary>
        /// Get the sample time.
        /// </summary>
        /// <param name="plmsSampleTime">The sample time.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_SampleTime([Out] out int plmsSampleTime);
    }

    /// <summary>
    /// The BDA topology interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("79B56888-7FEA-4690-B45D-38FD3C7849BE"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IBDA_Topology
    {
        /// <summary>
        /// Get node types.
        /// </summary>
        /// <param name="pulcNodeTypes">The node types.</param>
        /// <param name="ulcNodeTypesMax">The maximum node types.</param>
        /// <param name="rgulNodeTypes">The node types.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int GetNodeTypes(
            [Out] out int pulcNodeTypes,
            [In] int ulcNodeTypesMax,
            [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4, SizeParamIndex = 1)] int[] rgulNodeTypes
            );

        /// <summary>
        /// Get the node descriptors.
        /// </summary>
        /// <param name="ulcNodeDescriptors">The node descriptors.</param>
        /// <param name="ulcNodeDescriptorsMax">The maximum node descriptors.</param>
        /// <param name="rgNodeDescriptors">The node descriptors.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int GetNodeDescriptors(
            [Out] out int ulcNodeDescriptors,
            [In] int ulcNodeDescriptorsMax,
            [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct, SizeParamIndex = 1)] BDANodeDescriptor[] rgNodeDescriptors
            );

        /// <summary>
        /// Get the node interfaces.
        /// </summary>
        /// <param name="ulNodeType">The node type.</param>
        /// <param name="pulcInterfaces">The interfaces.</param>
        /// <param name="ulcInterfacesMax">The maximum interfaces.</param>
        /// <param name="rgguidInterfaces">The interfaces.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int GetNodeInterfaces(
            [In] int ulNodeType,
            [Out] out int pulcInterfaces,
            [In] int ulcInterfacesMax,
            [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct, SizeParamIndex = 2)] Guid[] rgguidInterfaces
            );

        /// <summary>
        /// Get the pin types.
        /// </summary>
        /// <param name="pulcPinTypes">The pin types.</param>
        /// <param name="ulcPinTypesMax">The maximum pin types.</param>
        /// <param name="rgulPinTypes">The pin types.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int GetPinTypes(
            [Out] out int pulcPinTypes,
            [In] int ulcPinTypesMax,
            [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.I4, SizeParamIndex = 1)] int[] rgulPinTypes
            );

        /// <summary>
        /// Get the template connections.
        /// </summary>
        /// <param name="pulcConnections">The connections.</param>
        /// <param name="ulcConnectionsMax">The maximum connections.</param>
        /// <param name="rgConnections">The connections.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int GetTemplateConnections(
            [Out] out int pulcConnections,
            [In] int ulcConnectionsMax,
            [Out, MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStruct, SizeParamIndex = 1)] BDATemplateConnection[] rgConnections
            );

        /// <summary>
        /// Create a pin.
        /// </summary>
        /// <param name="ulPinType">The pin type.</param>
        /// <param name="pulPinId">The pin identity.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int CreatePin(
            [In] int ulPinType,
            [Out] out int pulPinId
            );

        /// <summary>
        /// Delete a pin.
        /// </summary>
        /// <param name="ulPinId">The pin identity.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int DeletePin([In] int ulPinId);

        /// <summary>
        /// Set the media type.
        /// </summary>
        /// <param name="ulPinId">The pin identity.</param>
        /// <param name="pMediaType">The media type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int SetMediaType(
            [In] int ulPinId,
            [In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pMediaType
            );

        /// <summary>
        /// Set the medium.
        /// </summary>
        /// <param name="ulPinId">The pin identity.</param>
        /// <param name="pMedium">The medium.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int SetMedium(
            [In] int ulPinId,
            [In] RegPinMedium pMedium
            );

        /// <summary>
        /// Create the topology.
        /// </summary>
        /// <param name="ulInputPinId">The input pin identity.</param>
        /// <param name="ulOutputPinId">The output pin identity.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int CreateTopology(
            [In] int ulInputPinId,
            [In] int ulOutputPinId
            );

        /// <summary>
        /// Get a control node.
        /// </summary>
        /// <param name="ulInputPinId">The input pin identity.</param>
        /// <param name="ulOutputPinId">The output pin identity.</param>
        /// <param name="ulNodeType">The node type.</param>
        /// <param name="ppControlNode">The control node.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int GetControlNode(
            [In] int ulInputPinId,
            [In] int ulOutputPinId,
            [In] int ulNodeType,
            [Out, MarshalAs(UnmanagedType.IUnknown)] out object ppControlNode // IUnknown
            );

    }

    /// <summary>
    /// The BDA device control interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("FD0A5AF3-B41D-11d2-9C95-00C04F7971E0"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IBDA_DeviceControl
    {
        /// <summary>
        /// Start changes.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int StartChanges();

        /// <summary>
        /// Check changes are valid.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int CheckChanges();

        /// <summary>
        /// Commit changes.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int CommitChanges();

        /// <summary>
        /// Get the change state.
        /// </summary>
        /// <param name="pState">The state.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int GetChangeState([Out] out BDAChangeState pState);
    }

    /// <summary>
    /// The BDA frequency filter interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("71985F47-1CA1-11d3-9CC8-00C04F7971E0"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IBDA_FrequencyFilter
    {
        /// <summary>
        /// Set auto tune.
        /// </summary>
        /// <param name="ulTransponder">The transponder.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_Autotune([In] int ulTransponder);

        /// <summary>
        /// Get the auto tune.
        /// </summary>
        /// <param name="pulTransponder">The transponder.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_Autotune([Out] out int pulTransponder);

        /// <summary>
        /// Set the frequency.
        /// </summary>
        /// <param name="ulFrequency">The frequency.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_Frequency([In] int ulFrequency);

        /// <summary>
        /// Get the frequency.
        /// </summary>
        /// <param name="pulFrequency">The frequency.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_Frequency([Out] out int pulFrequency);

        /// <summary>
        /// Set the polarity.
        /// </summary>
        /// <param name="Polarity">The polarity.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_Polarity([In] Polarisation Polarity);

        /// <summary>
        /// Get the polarity.
        /// </summary>
        /// <param name="pPolarity">The polarity.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_Polarity([Out] out Polarisation pPolarity);

        /// <summary>
        /// Set the range.
        /// </summary>
        /// <param name="ulRange">The range.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_Range([In] int ulRange);

        /// <summary>
        /// Get the range.
        /// </summary>
        /// <param name="pulRange">The range.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_Range([Out] out int pulRange);

        /// <summary>
        /// Set the bandwidth.
        /// </summary>
        /// <param name="ulBandwidth">The bandwidth.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_Bandwidth([In] int ulBandwidth);

        /// <summary>
        /// Get the bandwidth.
        /// </summary>
        /// <param name="pulBandwidth">The bandwidth.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_Bandwidth([Out] out int pulBandwidth);

        /// <summary>
        /// Set the frequency multiplier.
        /// </summary>
        /// <param name="ulMultiplier">The frequency multiplier.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_FrequencyMultiplier([In] int ulMultiplier);

        /// <summary>
        /// Get the frequency multiplier.
        /// </summary>
        /// <param name="pulMultiplier">The frequency multiplier.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_FrequencyMultiplier([Out] out int pulMultiplier);
    }

    #endregion
}
