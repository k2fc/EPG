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

    [ComImport, Guid("A2E30750-6C3D-11d3-B653-00C04F79498E")]
    internal class ATSCTuningSpace { }

    [ComImport, Guid("C6B14B32-76AA-4a86-A7AC-5C79AAF58DA7")]
    internal class DVBTuningSpace { }

    [ComImport, Guid("B64016F3-C9A2-4066-96F0-BD9563314726")]
    internal class DVBSTuningSpace { }

    [ComImport, Guid("D9BB4CEE-B87A-47F1-AC92-B08D9C7813FC")]
    internal class DigitalCableTuningSpace { }

    [ComImport, Guid("8872FF1B-98FA-4d7a-8D93-C9F1055F85BB")]
    internal class ATSCLocator { }

    [ComImport, Guid("C531D9FD-9685-4028-8B68-6E1232079F1E")]
    internal class DVBCLocator { }    

    [ComImport, Guid("9CD64701-BDF3-4d14-8E03-F12983D86664")]
    internal class DVBTLocator { }

    [ComImport, Guid("1DF7D126-4050-47f0-A7CF-4C4CA9241333")]
    internal class DVBSLocator { }    

    [ComImport, Guid("03C06416-D127-407A-AB4C-FDD279ABBE5D")]
    internal class DigitalCableLocator { }

    [ComImport, Guid("6504AFED-A629-455c-A7F1-04964DEA5CC4")]
    internal class ISDBSLocator { }

    [ComImport, Guid("9CD64701-BDF3-4D14-8E03-F12983D86664")]
    internal class ISDBTLocator { }    
    
    #endregion

    #region Interfaces

    /// <summary>
    /// Digital cable tuner request interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("BAD7753B-6B37-4810-AE57-3CE0C4A9E6CB"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IDigitalCableTuneRequest : IATSCChannelTuneRequest
    {
        #region ITuneRequest Methods

        /// <summary>
        /// Get the tuning space.
        /// </summary>
        /// <param name="TuningSpace">The tuning space returned.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_TuningSpace([Out] out ITuningSpace TuningSpace);

        /// <summary>
        /// Get the components interface.
        /// </summary>
        /// <param name="Components">The returned components.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_Components([Out] out IComponents Components);

        /// <summary>
        /// Clone the tune request.
        /// </summary>
        /// <param name="NewTuneRequest">The new tune request.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Clone([Out] out ITuneRequest NewTuneRequest);

        /// <summary>
        /// Get the locator.
        /// </summary>
        /// <param name="Locator">The locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_Locator([Out] out ILocator Locator);

        /// <summary>
        /// Set the locator.
        /// </summary>
        /// <param name="Locator">The locator to set.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_Locator([In] ILocator Locator);

        #endregion

        #region IChannelTuneRequest Methods

        /// <summary>
        /// Get the channel number.
        /// </summary>
        /// <param name="Channel">The returned channel number.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_Channel([Out] out int Channel);

        /// <summary>
        /// Set the channel number.
        /// </summary>
        /// <param name="Channel">The channel number.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_Channel([In] int Channel);

        #endregion

        #region IATSCChannelTuneRequest Methods

        /// <summary>
        /// Get the minor channel number.
        /// </summary>
        /// <param name="MinorChannel">The returned channel number.</param>
        /// <returns>Zero if successful; false otherwise.</returns>
        [PreserveSig]
        new int get_MinorChannel([Out] out int MinorChannel);

        /// <summary>
        /// Put the minor channel number.
        /// </summary>
        /// <param name="MinorChannel">The channel number.</param>
        /// <returns>Zero if successful; false otherwise.</returns>
        [PreserveSig]
        new int put_MinorChannel([In] int MinorChannel);

        #endregion

        /// <summary>
        /// Get the major channel number.
        /// </summary>
        /// <param name="pMajorChannel">The channel number.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_MajorChannel([Out] out int pMajorChannel);

        /// <summary>
        /// Set the major channel number;
        /// </summary>
        /// <param name="MajorChannel">The channel number.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_MajorChannel([In] int MajorChannel);

        /// <summary>
        /// Get the source ID.
        /// </summary>
        /// <param name="pSourceID">The source ID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_SourceID([Out] out int pSourceID);

        /// <summary>
        /// Set the source ID.
        /// </summary>
        /// <param name="SourceID">The source ID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_SourceID([In] int SourceID);
    }

    /// <summary>
    /// The ISDBSLocator interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("C9897087-E29C-473F-9E4B-7072123DEA14"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IISDBSLocator : IDVBSLocator
    {
        /// <summary>
        /// Get the carrier frequency.
        /// </summary>
        [DispId(1)]        
        int CarrierFrequency { 
            [PreserveSig, DispId(1)] get; 
            [param: In] [PreserveSig, DispId(1)] set; 
        }

        /// <summary>
        /// Get the inner FEC.
        /// </summary>
        [DispId(2)]
        FECMethod InnerFEC { 
            [PreserveSig, DispId(2)] get; 
            [param: In] [PreserveSig, DispId(2)] set; 
        }

        /// <summary>
        /// Get the inner FEC rate.
        /// </summary>
        [DispId(3)]
        BinaryConvolutionCodeRate InnerFECRate { 
            [PreserveSig, DispId(3)] get; 
            [param: In] [PreserveSig, DispId(3)] set; 
        }

        /// <summary>
        /// Get the outer FEC.
        /// </summary>
        [DispId(4)]
        FECMethod OuterFEC { 
            [PreserveSig, DispId(4)] get; 
            [param: In] [PreserveSig, DispId(4)] set; 
        }

        /// <summary>
        /// Get the outer FEC rate.
        /// </summary>
        [DispId(5)]
        BinaryConvolutionCodeRate OuterFECRate { 
            [PreserveSig, DispId(5)] get; 
            [param: In] [PreserveSig, DispId(5)] set; 
        }

        /// <summary>
        /// Get the modulation.
        /// </summary>
        [DispId(6)]
        ModulationType Modulation { 
            [PreserveSig, DispId(6)] get; 
            [param: In] [PreserveSig, DispId(6)] set;
        }

        /// <summary>
        /// Get the symbol rate.
        /// </summary>
        [DispId(7)]
        int SymbolRate { 
            [PreserveSig, DispId(7)] get; 
            [param: In] [PreserveSig, DispId(7)] set; 
        }

        /// <summary>
        /// Clone the locator.
        /// </summary>
        /// <returns></returns>
        [return: MarshalAs(UnmanagedType.Interface)]
        [PreserveSig, DispId(8)]
        ILocator Clone();

        /// <summary>
        /// Get the polarization.
        /// </summary>
        [DispId(0x191)]
        Polarisation SignalPolarisation { 
            [PreserveSig, DispId(0x191)] get; 
            [param: In] [PreserveSig, DispId(0x191)] set; 
        }

        /// <summary>
        /// Get the east/west flag.
        /// </summary>
        [DispId(0x192)]
        bool WestPosition { 
            [PreserveSig, DispId(0x192)] get; 
            [param: In] [PreserveSig, DispId(0x192)] set; 
        }

        /// <summary>
        /// Get the orbital position.
        /// </summary>
        [DispId(0x193)]
        int OrbitalPosition { 
            [PreserveSig, DispId(0x193)] get; 
            [param: In] [PreserveSig, DispId(0x193)] set; 
        }

        /// <summary>
        /// Get the azimuth.
        /// </summary>
        [DispId(0x194)]
        int Azimuth { 
            [PreserveSig, DispId(0x194)] get; 
            [param: In] [PreserveSig, DispId(0x194)] set; 
        }

        /// <summary>
        /// Get the elevation.
        /// </summary>
        [DispId(0x195)]
        int Elevation { 
            [PreserveSig, DispId(0x195)] get; 
            [param: In] [PreserveSig, DispId(0x195)] set; 
        }

    }

    /// <summary>
    /// The tuning space interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("061C6E30-E622-11d2-9493-00C04F72D980"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface ITuningSpace
    {
        /// <summary>
        /// Get the unique name.
        /// </summary>
        /// <param name="Name">The unique name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_UniqueName([Out, MarshalAs(UnmanagedType.BStr)] out string Name);

        /// <summary>
        /// Set the uniqie name.
        /// </summary>
        /// <param name="Name">The uniqie name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_UniqueName([In, MarshalAs(UnmanagedType.BStr)] string Name);

        /// <summary>
        /// Get the friendly name.
        /// </summary>
        /// <param name="Name">The friendly name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_FriendlyName([Out, MarshalAs(UnmanagedType.BStr)] out string Name);

        /// <summary>
        /// Set the friendly name.
        /// </summary>
        /// <param name="Name">The friendly name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_FriendlyName([In, MarshalAs(UnmanagedType.BStr)] string Name);

        /// <summary>
        /// Get the class ID.
        /// </summary>
        /// <param name="SpaceCLSID">The class ID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_CLSID([Out, MarshalAs(UnmanagedType.BStr)] out string SpaceCLSID);

        /// <summary>
        /// Get the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The network type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_NetworkType([Out, MarshalAs(UnmanagedType.BStr)] out string NetworkTypeGuid);

        /// <summary>
        /// Set the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The network type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_NetworkType([In, MarshalAs(UnmanagedType.BStr)] string NetworkTypeGuid);

        /// <summary>
        /// Set the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The network type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get__NetworkType([Out] out Guid NetworkTypeGuid);

        /// <summary>
        /// Set the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The network type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put__NetworkType([In, MarshalAs(UnmanagedType.LPStruct)] Guid NetworkTypeGuid);

        /// <summary>
        /// Create a tune request.
        /// </summary>
        /// <param name="TuneRequest">The tune request.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int CreateTuneRequest([Out] out ITuneRequest TuneRequest);

        /// <summary>
        /// Enumerate the category GUID's.
        /// </summary>
        /// <param name="ppEnum">The enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int EnumCategoryGUIDs([Out, MarshalAs(UnmanagedType.IUnknown)] out object ppEnum); // IEnumGUID**

        /// <summary>
        /// Enumerate the device monikers.
        /// </summary>
        /// <param name="ppEnum">The enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int EnumDeviceMonikers([Out] out IEnumMoniker ppEnum);

        /// <summary>
        /// Get hte default preferred component types.
        /// </summary>
        /// <param name="ComponentTypes">The preferred component types.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_DefaultPreferredComponentTypes([Out] out IComponentTypes ComponentTypes);

        /// <summary>
        /// Set the default preferred component types.
        /// </summary>
        /// <param name="NewComponentTypes">The component types.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_DefaultPreferredComponentTypes([In] IComponentTypes NewComponentTypes);

        /// <summary>
        /// Get the frequency mapping.
        /// </summary>
        /// <param name="pMapping">The frequency mapping.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_FrequencyMapping([Out, MarshalAs(UnmanagedType.BStr)] out string pMapping);

        /// <summary>
        /// Set the frequency mapping.
        /// </summary>
        /// <param name="Mapping">The frequency mapping.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_FrequencyMapping([In, MarshalAs(UnmanagedType.BStr)] string Mapping);

        /// <summary>
        /// Get the default locator.
        /// </summary>
        /// <param name="LocatorVal">The locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_DefaultLocator([Out] out ILocator LocatorVal);

        /// <summary>
        /// Set the default locator.
        /// </summary>
        /// <param name="LocatorVal">The locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_DefaultLocator([In] ILocator LocatorVal);

        /// <summary>
        ///  Clone the tuning space.
        /// </summary>
        /// <param name="NewTS">The cloned tuning space.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Clone([Out] out ITuningSpace NewTS);
    }

    /// <summary>
    /// The Tuner interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("28C52640-018A-11d3-9D8E-00C04F72D980"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface ITuner
    {
        /// <summary>
        /// Get the tuning space.
        /// </summary>
        /// <param name="TuningSpace">The tuning space.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_TuningSpace([Out] out ITuningSpace TuningSpace);

        /// <summary>
        /// Set the tuning space.
        /// </summary>
        /// <param name="TuningSpace">The tuning space.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_TuningSpace([In] ITuningSpace TuningSpace);

        /// <summary>
        /// Enumerate the tuning spaces.
        /// </summary>
        /// <param name="ppEnum">The enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int EnumTuningSpaces([Out] out IEnumTuningSpaces ppEnum);

        /// <summary>
        /// Get the tune request.
        /// </summary>
        /// <param name="TuneRequest">The tune request.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_TuneRequest([Out] out ITuneRequest TuneRequest);

        /// <summary>
        /// Set the tune request.
        /// </summary>
        /// <param name="TuneRequest">The tune request.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_TuneRequest([In] ITuneRequest TuneRequest);

        /// <summary>
        /// Validate a tune request.
        /// </summary>
        /// <param name="TuneRequest">The tune request.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Validate([In] ITuneRequest TuneRequest);

        /// <summary>
        /// Get the preferred component types.
        /// </summary>
        /// <param name="ComponentTypes">The component types.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_PreferredComponentTypes([Out] out IComponentTypes ComponentTypes);

        /// <summary>
        /// Set the preferred component types.
        /// </summary>
        /// <param name="ComponentTypes">The component types.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_PreferredComponentTypes([In] IComponentTypes ComponentTypes);

        /// <summary>
        /// Get the signal strength.
        /// </summary>
        /// <param name="Strength">The signal strength.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_SignalStrength([Out] out int Strength);

        /// <summary>
        /// Trigger signal events.
        /// </summary>
        /// <param name="Interval">Time interval.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int TriggerSignalEvents([In] int Interval);
    }

    /// <summary>
    /// The TuneRequest interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("07DDC146-FC3D-11d2-9D8C-00C04F72D980"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface ITuneRequest
    {
        /// <summary>
        /// Get the tuning space.
        /// </summary>
        /// <param name="TuningSpace">The tuning space.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_TuningSpace([Out] out ITuningSpace TuningSpace);

        /// <summary>
        /// Get the components.
        /// </summary>
        /// <param name="Components">The components.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_Components([Out] out IComponents Components);

        /// <summary>
        /// Clone the tune request.
        /// </summary>
        /// <param name="NewTuneRequest">The cloned tune request.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Clone([Out] out ITuneRequest NewTuneRequest);

        /// <summary>
        /// Get the locator.
        /// </summary>
        /// <param name="Locator">The locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_Locator([Out] out ILocator Locator);

        /// <summary>
        /// Set the locator.
        /// </summary>
        /// <param name="Locator">The locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_Locator([In] ILocator Locator);
    }

    /// <summary>
    /// The DVBS locator interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("3D7C353C-0D04-45f1-A742-F97CC1188DC8"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IDVBSLocator : IDigitalLocator
    {
        #region ILocator Methods

        /// <summary>
        /// Get the carrier frequency.
        /// </summary>
        /// <param name="Frequency">The frequency.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_CarrierFrequency([Out] out int Frequency);

        /// <summary>
        /// Set the carrier frquency.
        /// </summary>
        /// <param name="Frequency">The frequency.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_CarrierFrequency([In] int Frequency);

        /// <summary>
        /// Get the innr FEC.
        /// </summary>
        /// <param name="FEC">The FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_InnerFEC([Out] out FECMethod FEC);

        /// <summary>
        /// Set the inner FEC.
        /// </summary>
        /// <param name="FEC">The FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_InnerFEC([In] FECMethod FEC);

        /// <summary>
        /// Get the inner FEC rate.
        /// </summary>
        /// <param name="FEC">The rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_InnerFECRate([Out] out BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Set the inner FEC rate.
        /// </summary>
        /// <param name="FEC">The rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_InnerFECRate([In] BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Get the outer FEC.
        /// </summary>
        /// <param name="FEC">The FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_OuterFEC([Out] out FECMethod FEC);

        /// <summary>
        /// Set the outer FEC.
        /// </summary>
        /// <param name="FEC">The FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_OuterFEC([In] FECMethod FEC);

        /// <summary>
        /// Get the outer FEC rate.
        /// </summary>
        /// <param name="FEC">The rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_OuterFECRate([Out] out BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Set the outer FEC rate.
        /// </summary>
        /// <param name="FEC">The FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_OuterFECRate([In] BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Get the modulation.
        /// </summary>
        /// <param name="Modulation">The modulation.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_Modulation([Out] out ModulationType Modulation);

        /// <summary>
        /// Set the modulation.
        /// </summary>
        /// <param name="Modulation">The modulation.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_Modulation([In] ModulationType Modulation);

        /// <summary>
        /// Get the symbol rate.
        /// </summary>
        /// <param name="Rate">The symbol rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_SymbolRate([Out] out int Rate);

        /// <summary>
        /// Set the symbol rate.
        /// </summary>
        /// <param name="Rate">The symbol rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_SymbolRate([In] int Rate);

        /// <summary>
        /// Clone the locator.
        /// </summary>
        /// <param name="NewLocator">The cloned locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Clone([Out] out ILocator NewLocator);

        #endregion

        /// <summary>
        /// Get the polarization.
        /// </summary>
        /// <param name="PolarisationVal">The polarization.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_SignalPolarisation([Out] out Polarisation PolarisationVal);

        /// <summary>
        /// Set the polarization.
        /// </summary>
        /// <param name="PolarisationVal">The polarization.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_SignalPolarisation([In] Polarisation PolarisationVal);

        /// <summary>
        /// Get the east/west flag.
        /// </summary>
        /// <param name="WestLongitude">The flag.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_WestPosition([Out, MarshalAs(UnmanagedType.VariantBool)] out bool WestLongitude);

        /// <summary>
        /// Set the east/west flag.
        /// </summary>
        /// <param name="WestLongitude">The flag.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_WestPosition([In, MarshalAs(UnmanagedType.VariantBool)] bool WestLongitude);

        /// <summary>
        /// Get the longitude.
        /// </summary>
        /// <param name="longitude">The longitude.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_OrbitalPosition([Out] out int longitude);

        /// <summary>
        /// Set the longitude.
        /// </summary>
        /// <param name="longitude">The longitude.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_OrbitalPosition([In] int longitude);

        /// <summary>
        /// Get the azimuth.
        /// </summary>
        /// <param name="Azimuth">The azimuth.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_Azimuth([Out] out int Azimuth);

        /// <summary>
        /// Set the azimuth.
        /// </summary>
        /// <param name="Azimuth">The azimuth.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_Azimuth([In] int Azimuth);

        /// <summary>
        /// Get the elevation.
        /// </summary>
        /// <param name="Elevation">The elevation.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_Elevation([Out] out int Elevation);

        /// <summary>
        /// Set the elevation.
        /// </summary>
        /// <param name="Elevation">The elevation.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_Elevation([In] int Elevation);
    }

    /// <summary>
    /// The DVBS tuning space interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("CDF7BE60-D954-42fd-A972-78971958E470"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IDVBSTuningSpace : IDVBTuningSpace2
    {
        #region ITuningSpace Methods

        /// <summary>
        /// Get the unique name.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_UniqueName([Out, MarshalAs(UnmanagedType.BStr)] out string Name);

        /// <summary>
        /// Set the uniqie name.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_UniqueName([In, MarshalAs(UnmanagedType.BStr)] string Name);

        /// <summary>
        /// Get the friendly name.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_FriendlyName([Out, MarshalAs(UnmanagedType.BStr)] out string Name);

        /// <summary>
        /// Set the friendly name.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_FriendlyName([In, MarshalAs(UnmanagedType.BStr)] string Name);

        /// <summary>
        /// Get the class ID.
        /// </summary>
        /// <param name="SpaceCLSID">The class ID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_CLSID([Out, MarshalAs(UnmanagedType.BStr)] out string SpaceCLSID);

        /// <summary>
        /// Get the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The network type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_NetworkType([Out, MarshalAs(UnmanagedType.BStr)] out string NetworkTypeGuid);

        /// <summary>
        /// Set the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The network type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_NetworkType([In, MarshalAs(UnmanagedType.BStr)] string NetworkTypeGuid);

        /// <summary>
        /// Get the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The network type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get__NetworkType([Out] out Guid NetworkTypeGuid);

        /// <summary>
        /// Set the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The network type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put__NetworkType([In, MarshalAs(UnmanagedType.LPStruct)] Guid NetworkTypeGuid);

        /// <summary>
        /// Create a tune request.
        /// </summary>
        /// <param name="TuneRequest">The tune request.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int CreateTuneRequest([Out] out ITuneRequest TuneRequest);

        /// <summary>
        /// Enumerate the category GUIDs.
        /// </summary>
        /// <param name="ppEnum">The enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int EnumCategoryGUIDs([Out, MarshalAs(UnmanagedType.IUnknown)] out object ppEnum); // IEnumGUID**

        /// <summary>
        /// Enumerate device monikers.
        /// </summary>
        /// <param name="ppEnum">The enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
#if USING_NET11
        new int EnumDeviceMonikers([Out] out UCOMIEnumMoniker ppEnum);
#else        
        new int EnumDeviceMonikers([Out] out IEnumMoniker ppEnum);
#endif

        /// <summary>
        /// Get default preferred component types.
        /// </summary>
        /// <param name="ComponentTypes">The component types.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_DefaultPreferredComponentTypes([Out] out IComponentTypes ComponentTypes);

        /// <summary>
        /// Set the default preferred component types.
        /// </summary>
        /// <param name="NewComponentTypes">The component types.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_DefaultPreferredComponentTypes([In] IComponentTypes NewComponentTypes);

        /// <summary>
        /// Get the frequency mapping.
        /// </summary>
        /// <param name="pMapping">The frequency mapping.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_FrequencyMapping([Out, MarshalAs(UnmanagedType.BStr)] out string pMapping);

        /// <summary>
        /// Set the frequency mapping.
        /// </summary>
        /// <param name="Mapping">The frequency mapping.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_FrequencyMapping([In, MarshalAs(UnmanagedType.BStr)] string Mapping);

        /// <summary>
        /// Get the default locator.
        /// </summary>
        /// <param name="LocatorVal">The locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_DefaultLocator([Out] out ILocator LocatorVal);

        /// <summary>
        /// Set the default locator.
        /// </summary>
        /// <param name="LocatorVal">The locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_DefaultLocator([In] ILocator LocatorVal);

        /// <summary>
        /// Clone the tuning space.
        /// </summary>
        /// <param name="NewTS">The cloned tuning space.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Clone([Out] out ITuningSpace NewTS);

        #endregion

        #region IDVBTuningSpace Methods

        /// <summary>
        /// Get the system type.
        /// </summary>
        /// <param name="SysType">The system type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_SystemType([Out] out DVBSystemType SysType);

        /// <summary>
        /// Set the system type.
        /// </summary>
        /// <param name="SysType">The system type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_SystemType([In] DVBSystemType SysType);

        #endregion

        #region IDVBTuningSpace2 Methods

        /// <summary>
        /// Get the network ID.
        /// </summary>
        /// <param name="NetworkID">The network ID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_NetworkID([Out] out int NetworkID);

        /// <summary>
        /// Set the network ID.
        /// </summary>
        /// <param name="NetworkID">The network ID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_NetworkID([In] int NetworkID);

        #endregion

        /// <summary>
        /// Get the low oscillator.
        /// </summary>
        /// <param name="LowOscillator">The low oscillator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_LowOscillator([Out] out int LowOscillator);

        /// <summary>
        /// Set the low oscillator.
        /// </summary>
        /// <param name="LowOscillator">The low oscillator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_LowOscillator([In] int LowOscillator);

        /// <summary>
        /// Get the high oscillator.
        /// </summary>
        /// <param name="HighOscillator">The high oscillator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_HighOscillator([Out] out int HighOscillator);

        /// <summary>
        /// Set the high oscillator.
        /// </summary>
        /// <param name="HighOscillator">The high oscillator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_HighOscillator([In] int HighOscillator);

        /// <summary>
        /// Get the LNB switch.
        /// </summary>
        /// <param name="LNBSwitch">The LNB switch.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_LNBSwitch([Out] out int LNBSwitch);

        /// <summary>
        /// Set the LNB switch.
        /// </summary>
        /// <param name="LNBSwitch">The LNB switch.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_LNBSwitch([In] int LNBSwitch);

        /// <summary>
        /// Get the input range.
        /// </summary>
        /// <param name="InputRange">The input range.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_InputRange([Out, MarshalAs(UnmanagedType.BStr)] out string InputRange);

        /// <summary>
        /// Set the input range.
        /// </summary>
        /// <param name="InputRange">The input range.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_InputRange([Out, MarshalAs(UnmanagedType.BStr)] string InputRange);

        /// <summary>
        /// Get the spectral inversion.
        /// </summary>
        /// <param name="SpectralInversionVal">The spectral inversion.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_SpectralInversion([Out] out SpectralInversion SpectralInversionVal);

        /// <summary>
        /// Set the spectral inversion.
        /// </summary>
        /// <param name="SpectralInversionVal">The spectral inversion.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_SpectralInversion([In] SpectralInversion SpectralInversionVal);
    }

    /// <summary>
    /// The locator interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("286D7F89-760C-4F89-80C4-66841D2507AA"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface ILocator
    {
        /// <summary>
        /// Get the carrier frequency.
        /// </summary>
        /// <param name="Frequency">The frequency.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_CarrierFrequency([Out] out int Frequency);

        /// <summary>
        /// Set the carrier frequency.
        /// </summary>
        /// <param name="Frequency">The frequency.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_CarrierFrequency([In] int Frequency);

        /// <summary>
        /// Get the inner FEC.
        /// </summary>
        /// <param name="FEC">The inner FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_InnerFEC([Out] out FECMethod FEC);

        /// <summary>
        /// Set the inner FEC.
        /// </summary>
        /// <param name="FEC">The inner FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_InnerFEC([In] FECMethod FEC);

        /// <summary>
        /// Get the inner FEC rate.
        /// </summary>
        /// <param name="FEC">The rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_InnerFECRate([Out] out BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Set the inner FEC rate.
        /// </summary>
        /// <param name="FEC">The rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_InnerFECRate([In] BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Get the outer FEC.
        /// </summary>
        /// <param name="FEC">The outer FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_OuterFEC([Out] out FECMethod FEC);

        /// <summary>
        /// Set the outer FEC.
        /// </summary>
        /// <param name="FEC">The outer FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_OuterFEC([In] FECMethod FEC);

        /// <summary>
        /// Get the outer FEC rate.
        /// </summary>
        /// <param name="FEC">The rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_OuterFECRate([Out] out BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Set the outer FEC rate.
        /// </summary>
        /// <param name="FEC">The rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_OuterFECRate([In] BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Get the modulation.
        /// </summary>
        /// <param name="Modulation">The modulation.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_Modulation([Out] out ModulationType Modulation);

        /// <summary>
        /// Set the modulation.
        /// </summary>
        /// <param name="Modulation">The modulation.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_Modulation([In] ModulationType Modulation);

        /// <summary>
        /// Get the symbol rate.
        /// </summary>
        /// <param name="Rate">The symbol rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_SymbolRate([Out] out int Rate);

        /// <summary>
        /// Set the symbol rate.
        /// </summary>
        /// <param name="Rate">The symbol rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_SymbolRate([In] int Rate);

        /// <summary>
        /// Clone the locator.
        /// </summary>
        /// <param name="NewLocator">The cloned locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Clone([Out] out ILocator NewLocator);
    }

    /// <summary>
    /// The DVB tuning space interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("ADA0B268-3B19-4e5b-ACC4-49F852BE13BA"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IDVBTuningSpace : ITuningSpace
    {
        #region ITuningSpace Methods

        /// <summary>
        /// Get the unique name.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_UniqueName([Out, MarshalAs(UnmanagedType.BStr)] out string Name);

        /// <summary>
        /// Set the unique name.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_UniqueName([In, MarshalAs(UnmanagedType.BStr)] string Name);

        /// <summary>
        /// Get the friendly name.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_FriendlyName([Out, MarshalAs(UnmanagedType.BStr)] out string Name);

        /// <summary>
        /// Set the friendly name.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_FriendlyName([In, MarshalAs(UnmanagedType.BStr)] string Name);

        /// <summary>
        /// Get the class ID.
        /// </summary>
        /// <param name="SpaceCLSID">The class ID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_CLSID([Out, MarshalAs(UnmanagedType.BStr)] out string SpaceCLSID);

        /// <summary>
        /// Get the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The network type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_NetworkType([Out, MarshalAs(UnmanagedType.BStr)] out string NetworkTypeGuid);

        /// <summary>
        /// Set the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The network type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_NetworkType([In, MarshalAs(UnmanagedType.BStr)] string NetworkTypeGuid);

        /// <summary>
        /// Get the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The network type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get__NetworkType([Out] out Guid NetworkTypeGuid);

        /// <summary>
        /// Set the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The network type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put__NetworkType([In, MarshalAs(UnmanagedType.LPStruct)] Guid NetworkTypeGuid);

        /// <summary>
        /// Create a tune request.
        /// </summary>
        /// <param name="TuneRequest">The tune request.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int CreateTuneRequest([Out] out ITuneRequest TuneRequest);

        /// <summary>
        /// Enumerate category GUIDs.
        /// </summary>
        /// <param name="ppEnum">The enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int EnumCategoryGUIDs([Out, MarshalAs(UnmanagedType.IUnknown)] out object ppEnum); // IEnumGUID**

        /// <summary>
        /// Enumerate device monikers.
        /// </summary>
        /// <param name="ppEnum">The enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
#if USING_NET11
        new int EnumDeviceMonikers([Out] out UCOMIEnumMoniker ppEnum);
#else
        new int EnumDeviceMonikers([Out] out IEnumMoniker ppEnum);
#endif

        /// <summary>
        /// Get the default preferred component types.
        /// </summary>
        /// <param name="ComponentTypes">The component types.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_DefaultPreferredComponentTypes([Out] out IComponentTypes ComponentTypes);

        /// <summary>
        /// Set the default preferred component types.
        /// </summary>
        /// <param name="NewComponentTypes">The component types.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_DefaultPreferredComponentTypes([In] IComponentTypes NewComponentTypes);

        /// <summary>
        /// Get the frequency mapping.
        /// </summary>
        /// <param name="pMapping">The frequency mapping.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_FrequencyMapping([Out, MarshalAs(UnmanagedType.BStr)] out string pMapping);

        /// <summary>
        /// Set the frequency mapping.
        /// </summary>
        /// <param name="Mapping">The frequency mapping.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_FrequencyMapping([In, MarshalAs(UnmanagedType.BStr)] string Mapping);

        /// <summary>
        /// Get the default locator.
        /// </summary>
        /// <param name="LocatorVal">The locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_DefaultLocator([Out] out ILocator LocatorVal);

        /// <summary>
        /// Set the default locator.
        /// </summary>
        /// <param name="LocatorVal">The locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_DefaultLocator([In] ILocator LocatorVal);

        /// <summary>
        /// Clone the tuning space.
        /// </summary>
        /// <param name="NewTS">The cloned tuning space.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Clone([Out] out ITuningSpace NewTS);

        #endregion

        /// <summary>
        /// Get the system type.
        /// </summary>
        /// <param name="SysType">The system type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_SystemType([Out] out DVBSystemType SysType);

        /// <summary>
        /// Set the system type.
        /// </summary>
        /// <param name="SysType">The system type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_SystemType([In] DVBSystemType SysType);
    }

    /// <summary>
    /// The DVB tuning space 2 interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("843188B4-CE62-43db-966B-8145A094E040"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IDVBTuningSpace2 : IDVBTuningSpace
    {
        #region ITuningSpace Methods

        /// <summary>
        /// Get unique name.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_UniqueName([Out, MarshalAs(UnmanagedType.BStr)] out string Name);

        /// <summary>
        /// Set the unique name.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_UniqueName([In, MarshalAs(UnmanagedType.BStr)] string Name);

        /// <summary>
        /// Get the friendly name.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_FriendlyName([Out, MarshalAs(UnmanagedType.BStr)] out string Name);

        /// <summary>
        /// Set the friendly name.
        /// </summary>
        /// <param name="Name">The name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_FriendlyName([In, MarshalAs(UnmanagedType.BStr)] string Name);

        /// <summary>
        /// Get the class ID.
        /// </summary>
        /// <param name="SpaceCLSID">The class ID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_CLSID([Out, MarshalAs(UnmanagedType.BStr)] out string SpaceCLSID);

        /// <summary>
        /// Get the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The network type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_NetworkType([Out, MarshalAs(UnmanagedType.BStr)] out string NetworkTypeGuid);

        /// <summary>
        /// Set the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The network type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_NetworkType([In, MarshalAs(UnmanagedType.BStr)] string NetworkTypeGuid);

        /// <summary>
        /// Get the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The network type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get__NetworkType([Out] out Guid NetworkTypeGuid);

        /// <summary>
        /// Set the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The network type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put__NetworkType([In, MarshalAs(UnmanagedType.LPStruct)] Guid NetworkTypeGuid);

        /// <summary>
        /// Create a tune request.
        /// </summary>
        /// <param name="TuneRequest">The tune request.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int CreateTuneRequest([Out] out ITuneRequest TuneRequest);

        /// <summary>
        /// Enumerate the category GUIDs.
        /// </summary>
        /// <param name="ppEnum">The enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int EnumCategoryGUIDs([Out, MarshalAs(UnmanagedType.IUnknown)] out object ppEnum); // IEnumGUID**

        /// <summary>
        /// Enumerate the device monikers.
        /// </summary>
        /// <param name="ppEnum">The enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
#if USING_NET11
        new int EnumDeviceMonikers([Out] out UCOMIEnumMoniker ppEnum);
#else
        new int EnumDeviceMonikers([Out] out IEnumMoniker ppEnum);
#endif

        /// <summary>
        /// Get the default preferred component types.
        /// </summary>
        /// <param name="ComponentTypes">The component types.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_DefaultPreferredComponentTypes([Out] out IComponentTypes ComponentTypes);

        /// <summary>
        /// Set the default preferred component types.
        /// </summary>
        /// <param name="NewComponentTypes">The component types.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_DefaultPreferredComponentTypes([In] IComponentTypes NewComponentTypes);

        /// <summary>
        /// Get the frequency mapping.
        /// </summary>
        /// <param name="pMapping">The frequency mapping.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_FrequencyMapping([Out, MarshalAs(UnmanagedType.BStr)] out string pMapping);

        /// <summary>
        /// Set the frequency mapping.
        /// </summary>
        /// <param name="Mapping">The frequency mapping.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_FrequencyMapping([In, MarshalAs(UnmanagedType.BStr)] string Mapping);

        /// <summary>
        /// Get the default locator.
        /// </summary>
        /// <param name="LocatorVal">The locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_DefaultLocator([Out] out ILocator LocatorVal);

        /// <summary>
        /// Set the default locator.
        /// </summary>
        /// <param name="LocatorVal">The locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_DefaultLocator([In] ILocator LocatorVal);

        /// <summary>
        /// Clone the tuning space.
        /// </summary>
        /// <param name="NewTS">The cloned tuning space.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Clone([Out] out ITuningSpace NewTS);

        #endregion

        #region IDVBTuningSpace Methods

        /// <summary>
        /// Get the system type.
        /// </summary>
        /// <param name="SysType">The system type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_SystemType([Out] out DVBSystemType SysType);

        /// <summary>
        /// Set the system type.
        /// </summary>
        /// <param name="SysType">The system type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_SystemType([In] DVBSystemType SysType);

        #endregion

        /// <summary>
        /// Get the network ID.
        /// </summary>
        /// <param name="NetworkID">The network ID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_NetworkID([Out] out int NetworkID);

        /// <summary>
        /// Set the network ID.
        /// </summary>
        /// <param name="NetworkID">The network ID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_NetworkID([In] int NetworkID);
    }

    /// <summary>
    /// The EnumTuningSpaces interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("8B8EB248-FC2B-11d2-9D8C-00C04F72D980"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumTuningSpaces
    {
        /// <summary>
        /// Get the next entry.
        /// </summary>
        /// <param name="celt">?</param>
        /// <param name="rgelt">?</param>
        /// <param name="pceltFetched">The entry.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        int Next(
            [In] int celt,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] ITuningSpace[] rgelt,
            [In] IntPtr pceltFetched
            );

        /// <summary>
        /// Skip an entry.
        /// </summary>
        /// <param name="celt">?</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        int Skip([In] int celt);

        /// <summary>
        /// Reset the enumerator.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        int Reset();

        /// <summary>
        /// Clone the enumerator.
        /// </summary>
        /// <param name="ppEnum">The cloned enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        int Clone([Out] out IEnumTuningSpaces ppEnum);
    }

    /// <summary>
    /// The DVBTuneRequest interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("0D6F567E-A636-42bb-83BA-CE4C1704AFA2"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IDVBTuneRequest : ITuneRequest
    {
        #region ITuneRequest Methods

        /// <summary>
        /// Get the tuning space.
        /// </summary>
        /// <param name="TuningSpace">The tuning space.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_TuningSpace([Out] out ITuningSpace TuningSpace);

        /// <summary>
        /// Get the components.
        /// </summary>
        /// <param name="Components">The components.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_Components([Out] out IComponents Components);

        /// <summary>
        /// Clone the tune request.
        /// </summary>
        /// <param name="NewTuneRequest">The cloned tune request.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Clone([Out] out ITuneRequest NewTuneRequest);

        /// <summary>
        /// Get the locator.
        /// </summary>
        /// <param name="Locator">The locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_Locator([Out] out ILocator Locator);

        /// <summary>
        /// Set the locator.
        /// </summary>
        /// <param name="Locator">The locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_Locator([In] ILocator Locator);

        #endregion

        /// <summary>
        /// Get the ONID.
        /// </summary>
        /// <param name="ONID">The ONID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_ONID([Out] out int ONID);

        /// <summary>
        /// Set the ONID.
        /// </summary>
        /// <param name="ONID">The ONID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_ONID([In] int ONID);

        /// <summary>
        /// Get the TSID.
        /// </summary>
        /// <param name="TSID">The TSID</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_TSID([Out] out int TSID);

        /// <summary>
        /// Set the TSID.
        /// </summary>
        /// <param name="TSID">The TSID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_TSID([In] int TSID);

        /// <summary>
        /// Get the SID.
        /// </summary>
        /// <param name="SID">The SID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_SID([Out] out int SID);

        /// <summary>
        /// Set the SID.
        /// </summary>
        /// <param name="SID">The SID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_SID([In] int SID);
    }

    /// <summary>
    /// The DVBTLocator interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("8664DA16-DDA2-42ac-926A-C18F9127C302"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IDVBTLocator : IDigitalLocator
    {
        #region ILocator Methods

        /// <summary>
        /// Get the carrier frequency.
        /// </summary>
        /// <param name="Frequency">The frequency.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_CarrierFrequency([Out] out int Frequency);

        /// <summary>
        /// Set the carrier frequency.
        /// </summary>
        /// <param name="Frequency">The frequency.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_CarrierFrequency([In] int Frequency);

        /// <summary>
        /// Get the inner FEC.
        /// </summary>
        /// <param name="FEC">The FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_InnerFEC([Out] out FECMethod FEC);

        /// <summary>
        /// Set the inner FEC.
        /// </summary>
        /// <param name="FEC">The FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_InnerFEC([In] FECMethod FEC);

        /// <summary>
        /// Get the inner FEC rate.
        /// </summary>
        /// <param name="FEC">The FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_InnerFECRate([Out] out BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Set the inner FEC rate.
        /// </summary>
        /// <param name="FEC">The FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_InnerFECRate([In] BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Get the outer FEC.
        /// </summary>
        /// <param name="FEC">The FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_OuterFEC([Out] out FECMethod FEC);

        /// <summary>
        /// Set the outer FEC.
        /// </summary>
        /// <param name="FEC">The outer FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_OuterFEC([In] FECMethod FEC);

        /// <summary>
        /// Get the outer FEC rate.
        /// </summary>
        /// <param name="FEC">The FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_OuterFECRate([Out] out BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Set the outer FEC rate.
        /// </summary>
        /// <param name="FEC">The FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_OuterFECRate([In] BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Get the modulation.
        /// </summary>
        /// <param name="Modulation">The modulation.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_Modulation([Out] out ModulationType Modulation);

        /// <summary>
        /// Set the modulation.
        /// </summary>
        /// <param name="Modulation">The modulation.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_Modulation([In] ModulationType Modulation);

        /// <summary>
        /// Get the symbol rate.
        /// </summary>
        /// <param name="Rate">The symbol rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_SymbolRate([Out] out int Rate);

        /// <summary>
        /// Set the symbol rate.
        /// </summary>
        /// <param name="Rate">The symbol rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_SymbolRate([In] int Rate);

        /// <summary>
        /// Clone the locator.
        /// </summary>
        /// <param name="NewLocator">The cloned locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Clone([Out] out ILocator NewLocator);

        #endregion

        /// <summary>
        /// Get the bandwidth.
        /// </summary>
        /// <param name="BandwidthVal">The bandwidth.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_Bandwidth([Out] out int BandwidthVal);

        /// <summary>
        /// Set the bandwidth.
        /// </summary>
        /// <param name="BandwidthVal">The bandwidth.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_Bandwidth([In] int BandwidthVal);

        /// <summary>
        /// Get the LP inner FEC.
        /// </summary>
        /// <param name="FEC">The FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_LPInnerFEC([Out] out FECMethod FEC);

        /// <summary>
        /// Set the LP inner FEC.
        /// </summary>
        /// <param name="FEC">The FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_LPInnerFEC([In] FECMethod FEC);

        /// <summary>
        /// Get the LP inner FEC rate.
        /// </summary>
        /// <param name="FEC">The FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_LPInnerFECRate([Out] out BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Set the LP inner FEC rate.
        /// </summary>
        /// <param name="FEC">The FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_LPInnerFECRate([In] BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Get the HAlpha
        /// </summary>
        /// <param name="Alpha">The HAlpha.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_HAlpha([Out] out HierarchyAlpha Alpha);

        /// <summary>
        /// Set the HAlpha.
        /// </summary>
        /// <param name="Alpha">The HAlpha.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_HAlpha([In] HierarchyAlpha Alpha);

        /// <summary>
        /// Get the guard interval.
        /// </summary>
        /// <param name="GI">The guard interval.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_Guard([Out] out GuardInterval GI);

        /// <summary>
        /// Set the guard interval.
        /// </summary>
        /// <param name="GI">The guard interval.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_Guard([In] GuardInterval GI);

        /// <summary>
        /// Get the mode.
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_Mode([Out] out TransmissionMode mode);

        /// <summary>
        /// Set the mode.
        /// </summary>
        /// <param name="mode">The mode.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_Mode([In] TransmissionMode mode);

        /// <summary>
        /// Get the other frequencies in use.
        /// </summary>
        /// <param name="OtherFrequencyInUseVal">The other frequencies.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_OtherFrequencyInUse([Out, MarshalAs(UnmanagedType.VariantBool)] out bool OtherFrequencyInUseVal);

        /// <summary>
        /// Set the other frequencies in use.
        /// </summary>
        /// <param name="OtherFrequencyInUseVal">The other frequencies.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_OtherFrequencyInUse([In, MarshalAs(UnmanagedType.VariantBool)] bool OtherFrequencyInUseVal);
    }

    /// <summary>
    /// The component types enumerator interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("8A674B4A-1F63-11d3-B64C-00C04F79498E"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumComponentTypes
    {
        /// <summary>
        /// Get the next entry.
        /// </summary>
        /// <param name="celt">?</param>
        /// <param name="rgelt">?</param>
        /// <param name="pceltFetched">The next entry.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        int Next(
            [In] int celt,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IComponentType[] rgelt,
            [In] IntPtr pceltFetched
            );

        /// <summary>
        /// Skip the next entry.
        /// </summary>
        /// <param name="celt">?</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        int Skip([In] int celt);

        /// <summary>
        /// Reset the enumerator.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        int Reset();

        /// <summary>
        /// Clone the enumerator.
        /// </summary>
        /// <param name="ppEnum">The cloned enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        int Clone([Out] out IEnumComponentTypes ppEnum);
    }

    /// <summary>
    /// The component type interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("6A340DC0-0311-11d3-9D8E-00C04F72D980"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IComponentType
    {
        /// <summary>
        /// Get the category.
        /// </summary>
        /// <param name="Category">The category.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_Category(
            [Out] out ComponentCategory Category
            );

        /// <summary>
        /// Set the category.
        /// </summary>
        /// <param name="Category">The category.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_Category(
            [In] ComponentCategory Category
            );

        /// <summary>
        /// Get the major media type.
        /// </summary>
        /// <param name="MediaMajorType">The major media type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_MediaMajorType(
            [Out, MarshalAs(UnmanagedType.BStr)] out string MediaMajorType
            );

        /// <summary>
        /// Set the major media type.
        /// </summary>
        /// <param name="MediaMajorType">The major media type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_MediaMajorType(
            [In, MarshalAs(UnmanagedType.BStr)] string MediaMajorType
            );

        /// <summary>
        /// Get the media major type.
        /// </summary>
        /// <param name="MediaMajorType">The media major type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get__MediaMajorType(
            [Out] out Guid MediaMajorType
            );

        /// <summary>
        /// Set the media major type.
        /// </summary>
        /// <param name="MediaMajorType">The media major type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put__MediaMajorType(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid MediaMajorType
            );

        /// <summary>
        /// Get the media subtype.
        /// </summary>
        /// <param name="MediaSubType">The subtype.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_MediaSubType(
            [Out, MarshalAs(UnmanagedType.BStr)] out string MediaSubType
            );

        /// <summary>
        /// Set the media subtype.
        /// </summary>
        /// <param name="MediaSubType">The subtype.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_MediaSubType(
            [In, MarshalAs(UnmanagedType.BStr)] string MediaSubType
            );

        /// <summary>
        /// Get the media subtype.
        /// </summary>
        /// <param name="MediaSubType">The subtype.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get__MediaSubType(
            [Out] out Guid MediaSubType
            );

        /// <summary>
        /// Set the media subtype.
        /// </summary>
        /// <param name="MediaSubType">The subtype.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put__MediaSubType(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid MediaSubType
            );

        /// <summary>
        /// Get the media format type.
        /// </summary>
        /// <param name="MediaFormatType">The format type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_MediaFormatType(
            [Out, MarshalAs(UnmanagedType.BStr)] out string MediaFormatType
            );

        /// <summary>
        /// Set the media format type.
        /// </summary>
        /// <param name="MediaFormatType">The format type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_MediaFormatType(
            [In, MarshalAs(UnmanagedType.BStr)] string MediaFormatType
            );

        /// <summary>
        /// Get the media format type.
        /// </summary>
        /// <param name="MediaFormatType">The format type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get__MediaFormatType(
            [Out] out Guid MediaFormatType
            );

        /// <summary>
        /// Set the media format type.
        /// </summary>
        /// <param name="MediaFormatType">The format type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put__MediaFormatType(
            [In, MarshalAs(UnmanagedType.LPStruct)] Guid MediaFormatType
            );

        /// <summary>
        /// Get the media type.
        /// </summary>
        /// <param name="MediaType">The media type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_MediaType(
            [Out] AMMediaType MediaType
            );

        /// <summary>
        /// Set the media type.
        /// </summary>
        /// <param name="MediaType">The media type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_MediaType(
            [In] AMMediaType MediaType
            );

        /// <summary>
        /// Clone the component type.
        /// </summary>
        /// <param name="NewCT">The cloned component type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Clone(
            [Out] out IComponentType NewCT
            );
    }

    /// <summary>
    /// The component types interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("0DC13D4A-0313-11d3-9D8E-00C04F72D980"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IComponentTypes
    {
        /// <summary>
        /// Get the count.
        /// </summary>
        /// <param name="Count">The count.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_Count(
            [Out] out int Count
            );

        /// <summary>
        /// Get a new enumerator.
        /// </summary>
        /// <param name="ppNewEnum">The enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get__NewEnum(
        [Out] out IEnumVARIANT ppNewEnum
            );

        /// <summary>
        /// Enumerate the component types.
        /// </summary>
        /// <param name="ppNewEnum">The enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int EnumComponentTypes(
            [Out] out IEnumComponentTypes ppNewEnum
            );

        /// <summary>
        /// Get a component.
        /// </summary>
        /// <param name="varIndex">The index.</param>
        /// <param name="TuningSpace">The tuning space.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_Item(
            [In] object varIndex,
            [Out] out IComponentType TuningSpace
            );

        /// <summary>
        /// Put a component type.
        /// </summary>
        /// <param name="NewIndex">The index.</param>
        /// <param name="ComponentType">The component type.</param>
        /// <returns></returns>
        [PreserveSig]
        int put_Item(
            [In] object NewIndex,
            [In] IComponentType ComponentType
            );

        /// <summary>
        /// Add a component type.
        /// </summary>
        /// <param name="ComponentType">The component type.</param>
        /// <param name="NewIndex">The index.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Add(
            [In] IComponentType ComponentType,
            [Out] out object NewIndex
            );

        /// <summary>
        /// Remove a component type.
        /// </summary>
        /// <param name="Index">The index.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Remove(
            [In] object Index
            );

        /// <summary>
        /// Clone the component types.
        /// </summary>
        /// <param name="NewList">The cloned component types.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Clone(
            [Out] out IComponentTypes NewList
            );
    }

    /// <summary>
    /// The Enumerate component interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("2A6E2939-2595-11d3-B64C-00C04F79498E"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumComponents
    {
        /// <summary>
        /// Get the next entry.
        /// </summary>
        /// <param name="celt">?</param>
        /// <param name="rgelt">?</param>
        /// <param name="pceltFetched">The next entry.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        int Next(
            [In] int celt,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IComponent[] rgelt,
            [In] IntPtr pceltFetched
            );

        /// <summary>
        /// Skip an entry.
        /// </summary>
        /// <param name="celt">?</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        int Skip([In] int celt);

        /// <summary>
        /// Reset the enumerator.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        int Reset();

        /// <summary>
        /// Clone the enumerator.
        /// </summary>
        /// <param name="ppEnum">The cloned enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        int Clone([Out] out IEnumComponents ppEnum);
    }

    /// <summary>
    /// The component interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("1A5576FC-0E19-11d3-9D8E-00C04F72D980"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IComponent
    {
        /// <summary>
        /// Get the type.
        /// </summary>
        /// <param name="CT">The type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_Type([Out] out IComponentType CT);

        /// <summary>
        /// Set the type.
        /// </summary>
        /// <param name="CT">The type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_Type([In] IComponentType CT);

        /// <summary>
        /// Get the description lasnguage identity.
        /// </summary>
        /// <param name="LangID">The identity.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_DescLangID([Out] out int LangID);

        /// <summary>
        /// Set the description language identity.
        /// </summary>
        /// <param name="LangID">The identity.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_DescLangID([In] int LangID);

        /// <summary>
        /// Get the status.
        /// </summary>
        /// <param name="Status">The status.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_Status([Out] out ComponentStatus Status);

        /// <summary>
        /// Set the status.
        /// </summary>
        /// <param name="Status">The status.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_Status([In] ComponentStatus Status);

        /// <summary>
        /// Get the description.
        /// </summary>
        /// <param name="Description">The description.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_Description([Out, MarshalAs(UnmanagedType.BStr)] out string Description);

        /// <summary>
        /// Set the description.
        /// </summary>
        /// <param name="Description">The description.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_Description([In, MarshalAs(UnmanagedType.BStr)] string Description);

        /// <summary>
        /// Clone the component.
        /// </summary>
        /// <param name="NewComponent">The cloned component.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Clone([Out] out IComponent NewComponent);
    }

    /// <summary>
    /// The components interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("FCD01846-0E19-11d3-9D8E-00C04F72D980"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IComponents
    {
        /// <summary>
        /// Get the number of components.
        /// </summary>
        /// <param name="Count">The number of components.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_Count(
            [Out] out int Count
            );

        /// <summary>
        /// Get an enumerator.
        /// </summary>
        /// <param name="ppNewEnum">The enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get__NewEnum(
        [Out] out IEnumVARIANT ppNewEnum
            );

        /// <summary>
        /// Get an enumerator.
        /// </summary>
        /// <param name="ppNewEnum">The enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int EnumComponents(
            [Out] out IEnumComponents ppNewEnum
            );

        /// <summary>
        /// Get an entry.
        /// </summary>
        /// <param name="varIndex">The index.</param>
        /// <param name="TuningSpace">The tuning space.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_Item(
            [In] object varIndex,
            [Out] out IComponent TuningSpace
            );

        /// <summary>
        /// Add a component.
        /// </summary>
        /// <param name="Component">The component.</param>
        /// <param name="NewIndex">The index.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Add(
            [In] IComponent Component,
            [Out] out object NewIndex
            );

        /// <summary>
        /// Remove a component.
        /// </summary>
        /// <param name="Index">The components index.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Remove(
            [In] object Index
            );

        /// <summary>
        /// Clone the components.
        /// </summary>
        /// <param name="NewList">The cloned components.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Clone(
            [Out] out IComponents NewList
            );
    }

    /// <summary>
    /// The AnalogTVTuningSpace interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("2A6E293C-2595-11d3-B64C-00C04F79498E"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IAnalogTVTuningSpace : ITuningSpace
    {
        #region ITuningSpace Methods

        /// <summary>
        /// Get the uniqie name.
        /// </summary>
        /// <param name="Name">The uniqie name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_UniqueName([Out, MarshalAs(UnmanagedType.BStr)] out string Name);

        /// <summary>
        /// Set the uniqie name.
        /// </summary>
        /// <param name="Name">The uniqie name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_UniqueName([In, MarshalAs(UnmanagedType.BStr)] string Name);

        /// <summary>
        /// Get the friendly name.
        /// </summary>
        /// <param name="Name">The friendly name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_FriendlyName([Out, MarshalAs(UnmanagedType.BStr)] out string Name);

        /// <summary>
        /// Set the friendly name.
        /// </summary>
        /// <param name="Name">The friendly name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_FriendlyName([In, MarshalAs(UnmanagedType.BStr)] string Name);

        /// <summary>
        /// Get the class ID.
        /// </summary>
        /// <param name="SpaceCLSID">tHE CLASS id.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_CLSID([Out, MarshalAs(UnmanagedType.BStr)] out string SpaceCLSID);

        /// <summary>
        /// Get the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The network type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_NetworkType([Out, MarshalAs(UnmanagedType.BStr)] out string NetworkTypeGuid);

        /// <summary>
        /// Set the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The network type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_NetworkType([In, MarshalAs(UnmanagedType.BStr)] string NetworkTypeGuid);

        /// <summary>
        /// Get the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The network type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get__NetworkType([Out] out Guid NetworkTypeGuid);

        /// <summary>
        /// Set the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The network type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put__NetworkType([In, MarshalAs(UnmanagedType.LPStruct)] Guid NetworkTypeGuid);

        /// <summary>
        /// Create a tune request.
        /// </summary>
        /// <param name="TuneRequest">The tune request.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int CreateTuneRequest([Out] out ITuneRequest TuneRequest);

        /// <summary>
        /// Enumerate the category GUID's.
        /// </summary>
        /// <param name="ppEnum">The enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int EnumCategoryGUIDs([Out, MarshalAs(UnmanagedType.IUnknown)] out object ppEnum); // IEnumGUID**

        /// <summary>
        /// Enumerate the device monikers.
        /// </summary>
        /// <param name="ppEnum">The enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int EnumDeviceMonikers([Out] out IEnumMoniker ppEnum);

        /// <summary>
        /// Get the default preferred component types.
        /// </summary>
        /// <param name="ComponentTypes">The component types.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_DefaultPreferredComponentTypes([Out] out IComponentTypes ComponentTypes);

        /// <summary>
        /// Set the default preferred component types.
        /// </summary>
        /// <param name="NewComponentTypes">The component types.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_DefaultPreferredComponentTypes([In] IComponentTypes NewComponentTypes);

        /// <summary>
        /// Get the frequency mapping.
        /// </summary>
        /// <param name="pMapping">The frequency mapping.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_FrequencyMapping([Out, MarshalAs(UnmanagedType.BStr)] out string pMapping);

        /// <summary>
        /// Set the frequency mapping.
        /// </summary>
        /// <param name="Mapping">The frequency mapping.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_FrequencyMapping([In, MarshalAs(UnmanagedType.BStr)] string Mapping);

        /// <summary>
        /// Get the default locator.
        /// </summary>
        /// <param name="LocatorVal">The locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_DefaultLocator([Out] out ILocator LocatorVal);

        /// <summary>
        /// Set the default locator.
        /// </summary>
        /// <param name="LocatorVal">The locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_DefaultLocator([In] ILocator LocatorVal);

        /// <summary>
        /// Clone the tuning space.
        /// </summary>
        /// <param name="NewTS">The cloned tuning space.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Clone([Out] out ITuningSpace NewTS);

        #endregion

        /// <summary>
        /// Get the minimum channel number.
        /// </summary>
        /// <param name="MinChannelVal">The minimum channel number.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_MinChannel([Out] out int MinChannelVal);

        /// <summary>
        /// Set the minimum channel number.
        /// </summary>
        /// <param name="NewMinChannelVal">The minimum channel to set.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_MinChannel([In] int NewMinChannelVal);

        /// <summary>
        /// Get the maximum channel number.
        /// </summary>
        /// <param name="MaxChannelVal">The maximum channel number.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_MaxChannel([Out] out int MaxChannelVal);

        /// <summary>
        /// Set the maximum channel number.
        /// </summary>
        /// <param name="NewMaxChannelVal">The maximum channel number.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_MaxChannel([In] int NewMaxChannelVal);

        /// <summary>
        /// Get the input type.
        /// </summary>
        /// <param name="InputTypeVal">The input type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_InputType([Out] out TunerInputType InputTypeVal);

        /// <summary>
        /// Set the input type.
        /// </summary>
        /// <param name="NewInputTypeVal">The input type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_InputType([In] TunerInputType NewInputTypeVal);

        /// <summary>
        /// Get the country code.
        /// </summary>
        /// <param name="CountryCodeVal">The country code.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_CountryCode([Out] out int CountryCodeVal);

        /// <summary>
        /// Set the country code.
        /// </summary>
        /// <param name="NewCountryCodeVal">The country code.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_CountryCode([In] int NewCountryCodeVal);
    }

    /// <summary>
    /// The atsc tune request interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("0369B4E1-45B6-11d3-B650-00C04F79498E"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IATSCChannelTuneRequest : IChannelTuneRequest
    {
        #region ITuneRequest Methods

        /// <summary>
        /// Get a tuning space.
        /// </summary>
        /// <param name="TuningSpace">The tuning space.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_TuningSpace([Out] out ITuningSpace TuningSpace);

        /// <summary>
        /// Get the components.
        /// </summary>
        /// <param name="Components">The components.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_Components([Out] out IComponents Components);

        /// <summary>
        /// Clone the tune request.
        /// </summary>
        /// <param name="NewTuneRequest">The cloned request.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Clone([Out] out ITuneRequest NewTuneRequest);

        /// <summary>
        /// Get the locator.
        /// </summary>
        /// <param name="Locator">The locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_Locator([Out] out ILocator Locator);

        /// <summary>
        /// Set the locator.
        /// </summary>
        /// <param name="Locator">he locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_Locator([In] ILocator Locator);

        #endregion

        #region IChannelTuneRequest Methods

        /// <summary>
        /// Get thechannel.
        /// </summary>
        /// <param name="Channel">The channel.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_Channel([Out] out int Channel);

        /// <summary>
        /// Set the channel.
        /// </summary>
        /// <param name="Channel">The channel.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_Channel([In] int Channel);

        #endregion

        /// <summary>
        /// Get the minor channel.
        /// </summary>
        /// <param name="MinorChannel">The minor channel.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_MinorChannel([Out] out int MinorChannel);

        /// <summary>
        /// Set the minor channel.
        /// </summary>
        /// <param name="MinorChannel">The minor channel.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_MinorChannel([In] int MinorChannel);
    }

    /// <summary>
    /// The ATSC locator interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("BF8D986F-8C2B-4131-94D7-4D3D9FCC21EF"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IATSCLocator : IDigitalLocator
    {
        #region ILocator Methods
        
        /// <summary>
        /// Get the carrier frequency.
        /// </summary>
        /// <param name="Frequency">The carrier frequency.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_CarrierFrequency([Out] out int Frequency);

        /// <summary>
        /// Set the carrier frequency.
        /// </summary>
        /// <param name="Frequency">The carrier frequency.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_CarrierFrequency([In] int Frequency);

        /// <summary>
        /// Get the inner FEC.
        /// </summary>
        /// <param name="FEC">The inner FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_InnerFEC([Out] out FECMethod FEC);

        /// <summary>
        /// Set the inner FEC.
        /// </summary>
        /// <param name="FEC">The inner FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_InnerFEC([In] FECMethod FEC);

        /// <summary>
        /// Get the inner FEC rate.
        /// </summary>
        /// <param name="FEC">The inner FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_InnerFECRate([Out] out BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Set the inner FEC rate.
        /// </summary>
        /// <param name="FEC">The inner FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_InnerFECRate([In] BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Get the outer FEC.
        /// </summary>
        /// <param name="FEC">The outer FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_OuterFEC([Out] out FECMethod FEC);

        /// <summary>
        /// Set the outer FEC.
        /// </summary>
        /// <param name="FEC">The outer FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_OuterFEC([In] FECMethod FEC);

        /// <summary>
        /// Get the outer FEC rate.
        /// </summary>
        /// <param name="FEC">The outer FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_OuterFECRate([Out] out BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Set the outer FEC rate.
        /// </summary>
        /// <param name="FEC">The outer FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_OuterFECRate([In] BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Get the modulation.
        /// </summary>
        /// <param name="Modulation">The modulation.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_Modulation([Out] out ModulationType Modulation);

        /// <summary>
        /// Set the modulation.
        /// </summary>
        /// <param name="Modulation">The modulation.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_Modulation([In] ModulationType Modulation);

        /// <summary>
        /// Get the symbol rate.
        /// </summary>
        /// <param name="Rate">The symbol rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_SymbolRate([Out] out int Rate);

        /// <summary>
        /// Set the symbol rate.
        /// </summary>
        /// <param name="Rate">The symbol rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_SymbolRate([In] int Rate);

        /// <summary>
        /// Clone the locator.
        /// </summary>
        /// <param name="NewLocator">The cloned locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Clone([Out] out ILocator NewLocator);

        #endregion

        /// <summary>
        /// Get the physical channel.
        /// </summary>
        /// <param name="PhysicalChannel">The physical channel.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_PhysicalChannel([Out] out int PhysicalChannel);

        /// <summary>
        /// Set the physical channel.
        /// </summary>
        /// <param name="PhysicalChannel">The physical channel.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_PhysicalChannel([In] int PhysicalChannel);

        /// <summary>
        /// Get the TSID.
        /// </summary>
        /// <param name="TSID">The TSID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_TSID([Out] out int TSID);

        /// <summary>
        /// Set the TSID.
        /// </summary>
        /// <param name="TSID">The TSID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_TSID([In] int TSID);
    }

    /// <summary>
    /// The ATSC tuning space interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("0369B4E2-45B6-11d3-B650-00C04F79498E"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IATSCTuningSpace : IAnalogTVTuningSpace
    {
        #region ITuningSpace Methods

        /// <summary>
        /// Get the unique name.
        /// </summary>
        /// <param name="Name">The unique name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_UniqueName([Out, MarshalAs(UnmanagedType.BStr)] out string Name);

        /// <summary>
        /// Set the uniqie name.
        /// </summary>
        /// <param name="Name">The unique name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_UniqueName([In, MarshalAs(UnmanagedType.BStr)] string Name);

        /// <summary>
        /// Get the friendly name.
        /// </summary>
        /// <param name="Name">The friendly name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_FriendlyName([Out, MarshalAs(UnmanagedType.BStr)] out string Name);

        /// <summary>
        /// Set the friendly name.
        /// </summary>
        /// <param name="Name">The friendly name.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_FriendlyName([In, MarshalAs(UnmanagedType.BStr)] string Name);

        /// <summary>
        /// Get the class ID.
        /// </summary>
        /// <param name="SpaceCLSID">The class ID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_CLSID([Out, MarshalAs(UnmanagedType.BStr)] out string SpaceCLSID);

        /// <summary>
        /// Get the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The network tpye.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_NetworkType([Out, MarshalAs(UnmanagedType.BStr)] out string NetworkTypeGuid);

        /// <summary>
        /// Set the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The network type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_NetworkType([In, MarshalAs(UnmanagedType.BStr)] string NetworkTypeGuid);

        /// <summary>
        /// Get the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The network type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get__NetworkType([Out] out Guid NetworkTypeGuid);

        /// <summary>
        /// Set the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The network type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put__NetworkType([In, MarshalAs(UnmanagedType.LPStruct)] Guid NetworkTypeGuid);

        /// <summary>
        /// Create a tune request.
        /// </summary>
        /// <param name="TuneRequest">The tune request.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int CreateTuneRequest([Out] out ITuneRequest TuneRequest);

        /// <summary>
        /// Enumerate the category GUID's.
        /// </summary>
        /// <param name="ppEnum">The enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int EnumCategoryGUIDs([Out, MarshalAs(UnmanagedType.IUnknown)] out object ppEnum); // IEnumGUID**

        /// <summary>
        /// Enumerate the device monikers. 
        /// </summary>
        /// <param name="ppEnum">The enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int EnumDeviceMonikers([Out] out IEnumMoniker ppEnum);

        /// <summary>
        /// Get the default preferred components.
        /// </summary>
        /// <param name="ComponentTypes">The default components.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_DefaultPreferredComponentTypes([Out] out IComponentTypes ComponentTypes);

        /// <summary>
        /// Set the preferred default components.
        /// </summary>
        /// <param name="NewComponentTypes">The default components.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_DefaultPreferredComponentTypes([In] IComponentTypes NewComponentTypes);

        /// <summary>
        /// Get the frequency mapping.
        /// </summary>
        /// <param name="pMapping">The frequency mapping.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_FrequencyMapping([Out, MarshalAs(UnmanagedType.BStr)] out string pMapping);

        /// <summary>
        /// Set the frequency mapping.
        /// </summary>
        /// <param name="Mapping">The frequency mapping.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_FrequencyMapping([In, MarshalAs(UnmanagedType.BStr)] string Mapping);

        /// <summary>
        /// Get the default locator.
        /// </summary>
        /// <param name="LocatorVal">The default locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_DefaultLocator([Out] out ILocator LocatorVal);

        /// <summary>
        /// Set the default locator.
        /// </summary>
        /// <param name="LocatorVal">The default locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_DefaultLocator([In] ILocator LocatorVal);

        /// <summary>
        /// Clone the tuning space.
        /// </summary>
        /// <param name="NewTS">The cloned tuning space.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Clone([Out] out ITuningSpace NewTS);

        #endregion

        #region IAnalogTVTuningSpace Methods

        /// <summary>
        /// Get the minimum channel number.
        /// </summary>
        /// <param name="MinChannelVal">The minimum channel number.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_MinChannel([Out] out int MinChannelVal);

        /// <summary>
        /// Set the minimum channel number.
        /// </summary>
        /// <param name="NewMinChannelVal">The minimum channel number.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_MinChannel([In] int NewMinChannelVal);

        /// <summary>
        /// Get the maximum channel number.
        /// </summary>
        /// <param name="MaxChannelVal">The maximum channel number.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_MaxChannel([Out] out int MaxChannelVal);

        /// <summary>
        /// Set the maximum channel number.
        /// </summary>
        /// <param name="NewMaxChannelVal">The maximum channel number.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_MaxChannel([In] int NewMaxChannelVal);

        /// <summary>
        /// Get the input type.
        /// </summary>
        /// <param name="InputTypeVal">The input type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_InputType([Out] out TunerInputType InputTypeVal);

        /// <summary>
        /// Set the input type.
        /// </summary>
        /// <param name="NewInputTypeVal">The input type.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_InputType([In] TunerInputType NewInputTypeVal);

        /// <summary>
        /// Get the country code.
        /// </summary>
        /// <param name="CountryCodeVal">The country code.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_CountryCode([Out] out int CountryCodeVal);

        /// <summary>
        /// Set the country code.
        /// </summary>
        /// <param name="NewCountryCodeVal">The country code.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_CountryCode([In] int NewCountryCodeVal);

        #endregion

        /// <summary>
        /// Get the minimum minor channel.
        /// </summary>
        /// <param name="MinMinorChannelVal">The minimum minor channel.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_MinMinorChannel([Out] out int MinMinorChannelVal);

        /// <summary>
        /// Set the minimum minor channel.
        /// </summary>
        /// <param name="NewMinMinorChannelVal">The minimum minor channel.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_MinMinorChannel([In] int NewMinMinorChannelVal);

        /// <summary>
        /// Get the maximum minor channel. 
        /// </summary>
        /// <param name="MaxMinorChannelVal">The maximum minor channel.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_MaxMinorChannel([Out] out int MaxMinorChannelVal);

        /// <summary>
        /// Set the maximum minor channel.
        /// </summary>
        /// <param name="NewMaxMinorChannelVal">The maximum minor channel.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_MaxMinorChannel([In] int NewMaxMinorChannelVal);

        /// <summary>
        /// Get the minimum physical channel.
        /// </summary>
        /// <param name="MinPhysicalChannelVal">The minimum physical channel.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_MinPhysicalChannel([Out] out int MinPhysicalChannelVal);

        /// <summary>
        /// Set the minimum physical channel.
        /// </summary>
        /// <param name="NewMinPhysicalChannelVal">The minimum physical channel.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_MinPhysicalChannel([In] int NewMinPhysicalChannelVal);

        /// <summary>
        /// Get the maximum physical channel.
        /// </summary>
        /// <param name="MaxPhysicalChannelVal">The maximum physical channel.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_MaxPhysicalChannel([Out] out int MaxPhysicalChannelVal);

        /// <summary>
        /// Set the maximum physical channel.
        /// </summary>
        /// <param name="NewMaxPhysicalChannelVal">The maximum physical channel.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_MaxPhysicalChannel([In] int NewMaxPhysicalChannelVal);
    }

    /// <summary>
    /// The channel tune request interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("0369B4E0-45B6-11d3-B650-00C04F79498E"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IChannelTuneRequest : ITuneRequest
    {
        #region ITuneRequest Methods

        /// <summary>
        /// Get the tuning space.
        /// </summary>
        /// <param name="TuningSpace">The tuning space.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_TuningSpace([Out] out ITuningSpace TuningSpace);

        /// <summary>
        /// Get the components.
        /// </summary>
        /// <param name="Components">The components.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_Components([Out] out IComponents Components);

        /// <summary>
        /// Clone the tune request.
        /// </summary>
        /// <param name="NewTuneRequest">The tune request.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Clone([Out] out ITuneRequest NewTuneRequest);

        /// <summary>
        /// Get the locator.
        /// </summary>
        /// <param name="Locator">The locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_Locator([Out] out ILocator Locator);

        /// <summary>
        /// Set the locator.
        /// </summary>
        /// <param name="Locator">The locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_Locator([In] ILocator Locator);

        #endregion

       /// <summary>
       /// Get the channel.
       /// </summary>
       /// <param name="Channel">The channel.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_Channel([Out] out int Channel);

        /// <summary>
        /// Set the channel.
        /// </summary>
        /// <param name="Channel">The channel.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_Channel([In] int Channel);
    }

    /// <summary>
    /// The DVBC locator interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("6E42F36E-1DD2-43c4-9F78-69D25AE39034"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IDVBCLocator : IDigitalLocator
    {
        #region ILocator Methods

        /// <summary>
        /// Get the carrier frequency.
        /// </summary>
        /// <param name="Frequency">The carrier frequency.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_CarrierFrequency([Out] out int Frequency);

        /// <summary>
        /// Set the carrier frequency.
        /// </summary>
        /// <param name="Frequency">The carrier frequency.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_CarrierFrequency([In] int Frequency);

        /// <summary>
        /// Get the inner FEC.
        /// </summary>
        /// <param name="FEC">The inner FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_InnerFEC([Out] out FECMethod FEC);

        /// <summary>
        /// Set the inner FEC.
        /// </summary>
        /// <param name="FEC">The inner FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_InnerFEC([In] FECMethod FEC);

        /// <summary>
        /// Get the inner FEC rate.
        /// </summary>
        /// <param name="FEC">The inner FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_InnerFECRate([Out] out BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Set the inner FEC rate.
        /// </summary>
        /// <param name="FEC">The inner FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_InnerFECRate([In] BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Get the outer FEC.
        /// </summary>
        /// <param name="FEC">The outer FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_OuterFEC([Out] out FECMethod FEC);

        /// <summary>
        /// Set the outer FEC.
        /// </summary>
        /// <param name="FEC">The outer FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_OuterFEC([In] FECMethod FEC);

        /// <summary>
        /// Get the outer FEC rate.
        /// </summary>
        /// <param name="FEC">The outer FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_OuterFECRate([Out] out BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Set the outer FEC rate.
        /// </summary>
        /// <param name="FEC">The outer FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_OuterFECRate([In] BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Get the modulation.
        /// </summary>
        /// <param name="Modulation">The modulation.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_Modulation([Out] out ModulationType Modulation);

        /// <summary>
        /// Set the modulation.
        /// </summary>
        /// <param name="Modulation">The modulation.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_Modulation([In] ModulationType Modulation);

        /// <summary>
        /// Get the symbol rate.
        /// </summary>
        /// <param name="Rate">The symbol rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_SymbolRate([Out] out int Rate);

        /// <summary>
        /// Set the symbol rate.
        /// </summary>
        /// <param name="Rate">The symbol rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_SymbolRate([In] int Rate);

        /// <summary>
        /// Clone the locator.
        /// </summary>
        /// <param name="NewLocator">The cloned locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Clone([Out] out ILocator NewLocator);

        #endregion
    }

    /// <summary>
    /// The digital locator interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("19B595D8-839A-47F0-96DF-4F194F3C768C"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IDigitalLocator : ILocator
    {
        #region ILocator Methods

        /// <summary>
        /// Get the carrier frequency.
        /// </summary>
        /// <param name="Frequency">The carrier frequency.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_CarrierFrequency([Out] out int Frequency);

        /// <summary>
        /// Set the carrier frequency.
        /// </summary>
        /// <param name="Frequency">The carrier frequency.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_CarrierFrequency([In] int Frequency);

        /// <summary>
        /// Get the inner FEC.
        /// </summary>
        /// <param name="FEC">The inner FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_InnerFEC([Out] out FECMethod FEC);

        /// <summary>
        /// Set the inner FEC.
        /// </summary>
        /// <param name="FEC">The inner FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_InnerFEC([In] FECMethod FEC);

        /// <summary>
        /// Get the inner FEC rate.
        /// </summary>
        /// <param name="FEC">The inner FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_InnerFECRate([Out] out BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Set the inner FEC rate.
        /// </summary>
        /// <param name="FEC">The inner FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_InnerFECRate([In] BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Get the outer FEC.
        /// </summary>
        /// <param name="FEC">The outer FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_OuterFEC([Out] out FECMethod FEC);

        /// <summary>
        /// Set the outer FEC.
        /// </summary>
        /// <param name="FEC">The outer FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_OuterFEC([In] FECMethod FEC);

        /// <summary>
        /// Get the outer FEC rate.
        /// </summary>
        /// <param name="FEC">The outer FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_OuterFECRate([Out] out BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Set the outer FEC rate.
        /// </summary>
        /// <param name="FEC">The outer FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_OuterFECRate([In] BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Get the modulation.
        /// </summary>
        /// <param name="Modulation">The modulation.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_Modulation([Out] out ModulationType Modulation);

        /// <summary>
        /// Set the modulation.
        /// </summary>
        /// <param name="Modulation">The modulation.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_Modulation([In] ModulationType Modulation);

        /// <summary>
        /// Get the symbol rate.
        /// </summary>
        /// <param name="Rate">The symbol rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_SymbolRate([Out] out int Rate);

        /// <summary>
        /// Set the symbol rate.
        /// </summary>
        /// <param name="Rate">The symbol rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_SymbolRate([In] int Rate);

        /// <summary>
        /// Clone the locator.
        /// </summary>
        /// <param name="NewLocator">The cloned locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Clone([Out] out ILocator NewLocator);

        #endregion
    }

    /// <summary>
    /// The ATSC locator 2 interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("612AA885-66CF-4090-BA0A-566F5312E4CA"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IATSCLocator2 : IATSCLocator
    {
        #region ILocator Methods

        /// <summary>
        /// Get the carrier frequency.
        /// </summary>
        /// <param name="Frequency">The carrier frequency.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_CarrierFrequency([Out] out int Frequency);

        /// <summary>
        /// Set the carrier frequency.
        /// </summary>
        /// <param name="Frequency">The carrier frequency.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_CarrierFrequency([In] int Frequency);

        /// <summary>
        /// Get the inner FEC.
        /// </summary>
        /// <param name="FEC">The inner FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_InnerFEC([Out] out FECMethod FEC);

        /// <summary>
        /// Put the inner FEC.
        /// </summary>
        /// <param name="FEC">The inner FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_InnerFEC([In] FECMethod FEC);

        /// <summary>
        /// Get the inner FEC rate.
        /// </summary>
        /// <param name="FEC">The inner FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_InnerFECRate([Out] out BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Set the inner fec rate.
        /// </summary>
        /// <param name="FEC">The inner FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_InnerFECRate([In] BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Get the outer FEC.
        /// </summary>
        /// <param name="FEC">The outer FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_OuterFEC([Out] out FECMethod FEC);

        /// <summary>
        /// Put the outer FEC.
        /// </summary>
        /// <param name="FEC">The outer FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_OuterFEC([In] FECMethod FEC);

        /// <summary>
        /// Get the outer FEC rate.
        /// </summary>
        /// <param name="FEC">The outer FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_OuterFECRate([Out] out BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Put the outer FEC rate.
        /// </summary>
        /// <param name="FEC">The outer FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_OuterFECRate([In] BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Get the modulation.
        /// </summary>
        /// <param name="Modulation">The modulation.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_Modulation([Out] out ModulationType Modulation);

        /// <summary>
        /// Set the modulation.
        /// </summary>
        /// <param name="Modulation">The mosdulation.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_Modulation([In] ModulationType Modulation);

        /// <summary>
        /// Get the symbol rate.
        /// </summary>
        /// <param name="Rate">The symbol rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_SymbolRate([Out] out int Rate);

        /// <summary>
        /// Set the symbol rate.
        /// </summary>
        /// <param name="Rate">The symbol rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_SymbolRate([In] int Rate);

        /// <summary>
        /// Clone the locator.
        /// </summary>
        /// <param name="NewLocator">The cloned locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Clone([Out] out ILocator NewLocator);

        #endregion

        #region IATSCLocator Methods

        /// <summary>
        /// Get the physical channel.
        /// </summary>
        /// <param name="PhysicalChannel">The physical channel.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_PhysicalChannel([Out] out int PhysicalChannel);

        /// <summary>
        /// Set the physical channel.
        /// </summary>
        /// <param name="PhysicalChannel">The physical channel.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_PhysicalChannel([In] int PhysicalChannel);

        /// <summary>
        /// Get the TSID.
        /// </summary>
        /// <param name="TSID">The TSID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_TSID([Out] out int TSID);

        /// <summary>
        /// Set the TSID.
        /// </summary>
        /// <param name="TSID">The TSID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_TSID([In] int TSID);

        #endregion

        /// <summary>
        /// Get the program number.
        /// </summary>
        /// <param name="ProgramNumber">The returned value.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_ProgramNumber([Out] out int ProgramNumber);

        /// <summary>
        /// Set the program number.
        /// </summary>
        /// <param name="ProgramNumber">The value to be set.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_ProgramNumber([In] int ProgramNumber);
    }

    /// <summary>
    /// The digital cable locator interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("48F66A11-171A-419A-9525-BEEECD51584C"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IDigitalCableLocator : IATSCLocator2
    {
        #region ILocator Methods

        /// <summary>
        /// Get the carrier frequency.
        /// </summary>
        /// <param name="Frequency">The frequency.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_CarrierFrequency([Out] out int Frequency);

        /// <summary>
        /// Set the carrier frequency.
        /// </summary>
        /// <param name="Frequency">The frequency.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_CarrierFrequency([In] int Frequency);

        /// <summary>
        /// Get the inner FEC.
        /// </summary>
        /// <param name="FEC">The inner FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_InnerFEC([Out] out FECMethod FEC);

        /// <summary>
        /// Set the inner FEC.
        /// </summary>
        /// <param name="FEC">The inner FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_InnerFEC([In] FECMethod FEC);

        /// <summary>
        /// Get the inner FEC rate.
        /// </summary>
        /// <param name="FEC">The FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_InnerFECRate([Out] out BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Set the inner FEC rate.
        /// </summary>
        /// <param name="FEC">The FEC rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_InnerFECRate([In] BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Get the outer FEC.
        /// </summary>
        /// <param name="FEC">The outer FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_OuterFEC([Out] out FECMethod FEC);

        /// <summary>
        /// Set the outer FEC.
        /// </summary>
        /// <param name="FEC">The outer FEC.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_OuterFEC([In] FECMethod FEC);

        /// <summary>
        /// Get the outer FEC rate.
        /// </summary>
        /// <param name="FEC">The rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_OuterFECRate([Out] out BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Set the outer FEC rate.
        /// </summary>
        /// <param name="FEC">The rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_OuterFECRate([In] BinaryConvolutionCodeRate FEC);

        /// <summary>
        /// Get the modulation.
        /// </summary>
        /// <param name="Modulation">The modulation.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_Modulation([Out] out ModulationType Modulation);

        /// <summary>
        /// Set the modulation.
        /// </summary>
        /// <param name="Modulation">The modulation.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_Modulation([In] ModulationType Modulation);

        /// <summary>
        /// Get the symbol rate.
        /// </summary>
        /// <param name="Rate">The symbol rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_SymbolRate([Out] out int Rate);

        /// <summary>
        /// Set the symbol rate.
        /// </summary>
        /// <param name="Rate">The symbol rate.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_SymbolRate([In] int Rate);

        /// <summary>
        /// Clone the locator.
        /// </summary>
        /// <param name="NewLocator">The cloned locator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Clone([Out] out ILocator NewLocator);

        #endregion

        #region IATSCLocator Methods

        /// <summary>
        /// Get the physical channel.
        /// </summary>
        /// <param name="PhysicalChannel">The channel.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_PhysicalChannel([Out] out int PhysicalChannel);

        /// <summary>
        /// Set the physical channel.
        /// </summary>
        /// <param name="PhysicalChannel">The channel.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_PhysicalChannel([In] int PhysicalChannel);

        /// <summary>
        /// Get the TSID.
        /// </summary>
        /// <param name="TSID">The TSID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_TSID([Out] out int TSID);

        /// <summary>
        /// Set the TSID.
        /// </summary>
        /// <param name="TSID">The TSID.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_TSID([In] int TSID);

        #endregion

        #region IATSCLocator2 Methods

        /// <summary>
        /// Get the program number.
        /// </summary>
        /// <param name="ProgramNumber">The program number.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_ProgramNumber([Out] out int ProgramNumber);

        /// <summary>
        /// Set the program number.
        /// </summary>
        /// <param name="ProgramNumber">The program number.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_ProgramNumber([In] int ProgramNumber);

        #endregion
    }

    /// <summary>
    /// Digital cable tuning space.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("013F9F9C-B449-4ec7-A6D2-9D4F2FC70AE5"),
    InterfaceType(ComInterfaceType.InterfaceIsDual)]
    public interface IDigitalCableTuningSpace : IATSCTuningSpace
    {
        #region ITuningSpace Methods

        /// <summary>
        /// Get the unique name.
        /// </summary>
        /// <param name="Name">The returned value.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_UniqueName([Out, MarshalAs(UnmanagedType.BStr)] out string Name);

        /// <summary>
        /// Set the unique name.
        /// </summary>
        /// <param name="Name">The value to set.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_UniqueName([In, MarshalAs(UnmanagedType.BStr)] string Name);

        /// <summary>
        /// Get the friendly name.
        /// </summary>
        /// <param name="Name">The returned value.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_FriendlyName([Out, MarshalAs(UnmanagedType.BStr)] out string Name);

        /// <summary>
        /// Set the friendly name.
        /// </summary>
        /// <param name="Name">The value to set.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_FriendlyName([In, MarshalAs(UnmanagedType.BStr)] string Name);

        /// <summary>
        /// Get the CLSID.
        /// </summary>
        /// <param name="SpaceCLSID">The returned value.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_CLSID([Out, MarshalAs(UnmanagedType.BStr)] out string SpaceCLSID);

        /// <summary>
        /// Get the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The returned value.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_NetworkType([Out, MarshalAs(UnmanagedType.BStr)] out string NetworkTypeGuid);

        /// <summary>
        /// Set the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The value to set.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_NetworkType([In, MarshalAs(UnmanagedType.BStr)] string NetworkTypeGuid);

        /// <summary>
        /// Get the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The returned value.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get__NetworkType([Out] out Guid NetworkTypeGuid);

        /// <summary>
        /// Set the network type.
        /// </summary>
        /// <param name="NetworkTypeGuid">The value to set.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put__NetworkType([In, MarshalAs(UnmanagedType.LPStruct)] Guid NetworkTypeGuid);

        /// <summary>
        /// Create the tune request.
        /// </summary>
        /// <param name="TuneRequest">The returned tune request.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int CreateTuneRequest([Out] out ITuneRequest TuneRequest);

        /// <summary>
        /// Enumerate the category GUID's.
        /// </summary>
        /// <param name="ppEnum">The returned enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int EnumCategoryGUIDs([Out, MarshalAs(UnmanagedType.IUnknown)] out object ppEnum); // IEnumGUID**

        /// <summary>
        /// Enumerate the device monikers.
        /// </summary>
        /// <param name="ppEnum">The returned enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int EnumDeviceMonikers([Out] out IEnumMoniker ppEnum);

        /// <summary>
        /// Get the default preferred component types.
        /// </summary>
        /// <param name="ComponentTypes">The returned value.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_DefaultPreferredComponentTypes([Out] out IComponentTypes ComponentTypes);

        /// <summary>
        /// Set the default preferred component types.
        /// </summary>
        /// <param name="NewComponentTypes">The value to set.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_DefaultPreferredComponentTypes([In] IComponentTypes NewComponentTypes);

        /// <summary>
        /// Get the frequency mapping.
        /// </summary>
        /// <param name="pMapping">The returned value.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_FrequencyMapping([Out, MarshalAs(UnmanagedType.BStr)] out string pMapping);

        /// <summary>
        /// Set the frequency mapping.
        /// </summary>
        /// <param name="Mapping">The value to set.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_FrequencyMapping([In, MarshalAs(UnmanagedType.BStr)] string Mapping);

        /// <summary>
        /// Get the default locator.
        /// </summary>
        /// <param name="LocatorVal">The returned value.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_DefaultLocator([Out] out ILocator LocatorVal);

        /// <summary>
        /// Set the default locator.
        /// </summary>
        /// <param name="LocatorVal">The value to set.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_DefaultLocator([In] ILocator LocatorVal);

        /// <summary>
        /// Clone the tuning space.
        /// </summary>
        /// <param name="NewTS">The cloned tuning space.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int Clone([Out] out ITuningSpace NewTS);

        #endregion

        #region IAnalogTVTuningSpace Methods

        /// <summary>
        /// Get the minimum channel number.
        /// </summary>
        /// <param name="MinChannelVal">The returned value.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_MinChannel([Out] out int MinChannelVal);

        /// <summary>
        /// Set the minimum channel number.
        /// </summary>
        /// <param name="NewMinChannelVal">The value to set.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_MinChannel([In] int NewMinChannelVal);

        /// <summary>
        /// Get the maximum channel number.
        /// </summary>
        /// <param name="MaxChannelVal">The returned value.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_MaxChannel([Out] out int MaxChannelVal);

        /// <summary>
        /// Set the maximum channel number.
        /// </summary>
        /// <param name="NewMaxChannelVal">The value to set.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_MaxChannel([In] int NewMaxChannelVal);

        /// <summary>
        /// Get the input type.
        /// </summary>
        /// <param name="InputTypeVal">The returned value.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_InputType([Out] out TunerInputType InputTypeVal);

        /// <summary>
        /// Set the input type.
        /// </summary>
        /// <param name="NewInputTypeVal">The value to set.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_InputType([In] TunerInputType NewInputTypeVal);

        /// <summary>
        /// Get the country code.
        /// </summary>
        /// <param name="CountryCodeVal">The returned value.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_CountryCode([Out] out int CountryCodeVal);

        /// <summary>
        /// Set the country code.
        /// </summary>
        /// <param name="NewCountryCodeVal">The value to set.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_CountryCode([In] int NewCountryCodeVal);

        #endregion

        #region IATSCTuningSpace Methods

        /// <summary>
        /// Get the minor channel number.
        /// </summary>
        /// <param name="MinMinorChannelVal">The returned value.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_MinMinorChannel([Out] out int MinMinorChannelVal);

        /// <summary>
        /// Set the minor channel number.
        /// </summary>
        /// <param name="NewMinMinorChannelVal">The value to set.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_MinMinorChannel([In] int NewMinMinorChannelVal);

        /// <summary>
        /// Get the maximum minor channel number.
        /// </summary>
        /// <param name="MaxMinorChannelVal">The returned value.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_MaxMinorChannel([Out] out int MaxMinorChannelVal);

        /// <summary>
        /// Set the maximum minor channel number.
        /// </summary>
        /// <param name="NewMaxMinorChannelVal">The value to set.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_MaxMinorChannel([In] int NewMaxMinorChannelVal);

        /// <summary>
        /// Get the minimum physical channel number.
        /// </summary>
        /// <param name="MinPhysicalChannelVal">The returned value.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_MinPhysicalChannel([Out] out int MinPhysicalChannelVal);

        /// <summary>
        /// Set the minimum physical channel number.
        /// </summary>
        /// <param name="NewMinPhysicalChannelVal">The value to set.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_MinPhysicalChannel([In] int NewMinPhysicalChannelVal);

        /// <summary>
        /// Get the maximum physical channel number.
        /// </summary>
        /// <param name="MaxPhysicalChannelVal">The returned value.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int get_MaxPhysicalChannel([Out] out int MaxPhysicalChannelVal);

        /// <summary>
        /// Set the maximum physical channel number.
        /// </summary>
        /// <param name="NewMaxPhysicalChannelVal">The value to set.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        new int put_MaxPhysicalChannel([In] int NewMaxPhysicalChannelVal);

        #endregion

        /// <summary>
        /// Get the major and minor channel.
        /// </summary>
        /// <param name="MinMajorChannelVal">The returned value.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_MinMajorChannel([Out] out int MinMajorChannelVal);

        /// <summary>
        /// Set the major and minor channel.
        /// </summary>
        /// <param name="NewMinMajorChannelVal">The value to set.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_MinMajorChannel([In] int NewMinMajorChannelVal);

        /// <summary>
        /// Get the maximum major channel.
        /// </summary>
        /// <param name="MaxMajorChannelVal">The returned value.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_MaxMajorChannel([Out] out int MaxMajorChannelVal);

        /// <summary>
        /// Set the maximum major channel.
        /// </summary>
        /// <param name="NewMaxMajorChannelVal">The value to set.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_MaxMajorChannel([In] int NewMaxMajorChannelVal);

        /// <summary>
        /// Get the minimum source ID.
        /// </summary>
        /// <param name="MinSourceIDVal">The returned value.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int get_MinSourceID([Out] out int MinSourceIDVal);

        /// <summary>
        /// Set the minimum source ID.
        /// </summary>
        /// <param name="NewMinSourceIDVal">The value to set.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int put_MinSourceID([In] int NewMinSourceIDVal);

        /// <summary>
        /// Get the maximum source ID.
        /// </summary>
        /// <param name="MaxSourceIDVal">The returned value.</param>
        /// <returns>Zero if successful; non-zero otherwsie.</returns>
        [PreserveSig]
        int get_MaxSourceID([Out] out int MaxSourceIDVal);

        /// <summary>
        /// Set the maximum source ID.
        /// </summary>
        /// <param name="NewMaxSourceIDVal">The maximum value.</param>
        /// <returns>Zero if successful; non-zero otherwsie.</returns>
        [PreserveSig]
        int put_MaxSourceID([In] int NewMaxSourceIDVal);
    }    

    #endregion
}
