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
    /// From ScanModulationTypes
    /// </summary>
    [Flags]
    public enum ScanModulationTypes
    {
        /// <summary>
        /// 16QAM.
        /// </summary>
        ScanMod16QAM = 0x00000001,
        /// <summary>
        /// 32QAM.
        /// </summary>
        ScanMod32QAM = 0x00000002,
        /// <summary>
        /// 64QAM.
        /// </summary>
        ScanMod64QAM = 0x00000004,
        /// <summary>
        /// 80QAM.
        /// </summary>
        ScanMod80QAM = 0x00000008,
        /// <summary>
        /// 96QAM.
        /// </summary>
        ScanMod96QAM = 0x00000010,
        /// <summary>
        /// 112QAM.
        /// </summary>
        ScanMod112QAM = 0x00000020,
        /// <summary>
        /// 128QAM.
        /// </summary>
        ScanMod128QAM = 0x00000040,
        /// <summary>
        /// 160QAM.
        /// </summary>
        ScanMod160QAM = 0x00000080,
        /// <summary>
        /// 192QAM.
        /// </summary>
        ScanMod192QAM = 0x00000100,
        /// <summary>
        /// 224QAM.
        /// </summary>
        ScanMod224QAM = 0x00000200,
        /// <summary>
        /// 256QAM.
        /// </summary>
        ScanMod256QAM = 0x00000400,
        /// <summary>
        /// 320QAM.
        /// </summary>
        ScanMod320QAM = 0x00000800,
        /// <summary>
        /// 384QAM.
        /// </summary>
        ScanMod384QAM = 0x00001000,
        /// <summary>
        /// 448QAM.
        /// </summary>
        ScanMod448QAM = 0x00002000,
        /// <summary>
        /// 512QAM.
        /// </summary>
        ScanMod512QAM = 0x00004000,
        /// <summary>
        /// 640QAM.
        /// </summary>
        ScanMod640QAM = 0x00008000,
        /// <summary>
        /// 768QAM.
        /// </summary>
        ScanMod768QAM = 0x00010000,
        /// <summary>
        /// 896QAM.
        /// </summary>
        ScanMod896QAM = 0x00020000,
        /// <summary>
        /// 1024QAM.
        /// </summary>
        ScanMod1024QAM = 0x00040000,
        /// <summary>
        /// QPSK.
        /// </summary>
        ScanModQPSK = 0x00080000,
        /// <summary>
        /// BPSK.
        /// </summary>
        ScanModBPSK = 0x00100000,
        /// <summary>
        /// OQPSK.
        /// </summary>
        ScanModOQPSK = 0x00200000,
        /// <summary>
        /// 8VSB.
        /// </summary>
        ScanMod8VSB = 0x00400000,
        /// <summary>
        /// 16VSB.
        /// </summary>
        ScanMod16VSB = 0x00800000,
        /// <summary>
        /// AM Radio.
        /// </summary>
        ScanModAM_RADIO = 0x01000000,
        /// <summary>
        /// FM Radio.
        /// </summary>
        ScanModFM_RADIO = 0x02000000,
        /// <summary>
        /// 8PSK.
        /// </summary>
        ScanMod8PSK = 0x04000000,
        /// <summary>
        /// RF.
        /// </summary>
        ScanModRF = 0x08000000,
        /// <summary>
        /// WMC digital cable.
        /// </summary>
        MCEDigitalCable = ModulationType.Mod640Qam | ModulationType.Mod256Qam,
        /// <summary>
        /// WMC terrestrial ATSC.
        /// </summary>
        MCETerrestrialATSC = ModulationType.Mod8Vsb,
        /// <summary>
        /// WMC analogue TV.
        /// </summary>
        MCEAnalogTv = ModulationType.ModRF,
        /// <summary>
        /// WMC TV.
        /// </summary>
        MCEAll_TV = unchecked((int)0xffffffff),
    }

    /// <summary>
    /// Rolloff values.
    /// </summary>
    public enum RollOff
    {
        /// <summary>
        /// Rolloff not set.
        /// </summary>
        NotSet = -1,
        /// <summary>
        /// Rolloff not defined.
        /// </summary>
        NotDefined = 0,
        /// <summary>
        /// Rolloff is 0.20.
        /// </summary>
        Twenty = 1,
        /// <summary>
        /// Rolloff is 0.25.
        /// </summary>
        TwentyFive,
        /// <summary>
        /// Rolloff is 0.35.
        /// </summary>
        ThirtyFive,
        /// <summary>
        /// Maximum value.
        /// </summary>
        Max
    }

    /// <summary>
    /// Pilot values.
    /// </summary>
    public enum Pilot
    {
        /// <summary>
        /// Pilot not set.
        /// </summary>
        NotSet = -1,
        /// <summary>
        /// Pilot not defined.
        /// </summary>
        NotDefined = 0,
        /// <summary>
        /// Pilot off.
        /// </summary>
        Off = 1,
        /// <summary>
        /// Pilot on.
        /// </summary>
        On,
        /// <summary>
        /// Maximum value.
        /// </summary>
        Max
    }

    /// <summary>
    /// From FECMethod
    /// </summary>
    public enum FECMethod
    {
        /// <summary>
        /// Not set
        /// </summary>
        MethodNotSet = -1,
        /// <summary>
        /// Not defined
        /// </summary>
        MethodNotDefined = 0,
        /// <summary>
        /// Viterbi
        /// </summary>
        Viterbi = 1, // FEC is a Viterbi Binary Convolution.
        /// <summary>
        /// RS204-188
        /// </summary>
        RS204_188, // The FEC is Reed-Solomon 204/188 (outer FEC)
        /// <summary>
        /// LDPC
        /// </summary>
        Ldpc,
        /// <summary>
        /// BCH
        /// </summary>
        Bch,
        /// <summary>
        /// RS 147-130
        /// </summary>
        RS147_130,
        /// Maximum entry.
        Max,
    }

    /// <summary>
    /// From BinaryConvolutionCodeRate
    /// </summary>
    public enum BinaryConvolutionCodeRate
    {
        /// <summary>
        /// Not set.
        /// </summary>
        RateNotSet = -1,
        /// <summary>
        /// Not defined.
        /// </summary>
        RateNotDefined = 0,
        /// <summary>
        /// 1/2
        /// </summary>
        Rate1_2 = 1, // 1/2
        /// <summary>
        /// 2/3
        /// </summary>
        Rate2_3, // 2/3
        /// <summary>
        /// 3/4
        /// </summary>
        Rate3_4, // 3/4
        /// <summary>
        /// 3/5
        /// </summary>
        Rate3_5,
        /// <summary>
        /// 4/5
        /// </summary>
        Rate4_5,
        /// <summary>
        /// 5/6
        /// </summary>
        Rate5_6, // 5/6
        /// <summary>
        /// 5/11
        /// </summary>
        Rate5_11,
        /// <summary>
        /// 7/8
        /// </summary>
        Rate7_8, // 7/8
        /// <summary>
        /// 1/4
        /// </summary>
        Rate1_4,
        /// <summary>
        /// 1/3
        /// </summary>
        Rate1_3,
        /// <summary>
        /// 2/5
        /// </summary>
        Rate2_5,
        /// <summary>
        /// 6/7
        /// </summary>
        Rate6_7,
        /// <summary>
        /// 8/9
        /// </summary>
        Rate8_9,
        /// <summary>
        /// 9/10
        /// </summary>
        Rate9_10,
        /// <summary>
        /// Maximum entry.
        /// </summary>
        RateMax
    }

    /// <summary>
    /// From Polarisation
    /// </summary>
    public enum Polarisation
    {
        /// <summary>
        /// Not set
        /// </summary>
        NotSet = -1,
        /// <summary>
        /// Not defined
        /// </summary>
        NotDefined = 0,
        /// <summary>
        /// Linear horizontal
        /// </summary>
        LinearH = 1, // Linear horizontal polarisation
        /// <summary>
        /// Linear vertical
        /// </summary>
        LinearV, // Linear vertical polarisation
        /// <summary>
        /// Circular left
        /// </summary>
        CircularL, // Circular left polarisation
        /// <summary>
        /// Circular right
        /// </summary>
        CircularR, // Circular right polarisation
        /// <summary>
        /// Maximum entry
        /// </summary>
        Max
    }

    /// <summary>
    /// From SpectralInversion
    /// </summary>
    public enum SpectralInversion
    {
        /// <summary>
        /// Not set
        /// </summary>
        NotSet = -1,
        /// <summary>
        /// Not defined
        /// </summary>
        NotDefined = 0,
        /// <summary>
        /// Automatic
        /// </summary>
        Automatic = 1,
        /// <summary>
        /// Normal
        /// </summary>
        Normal,
        /// <summary>
        /// Inverted
        /// </summary>
        Inverted,
        /// <summary>
        /// Maximum entry.
        /// </summary>
        Max
    }

    /// <summary>
    /// From ModulationType
    /// </summary>
    public enum ModulationType
    {
        /// <summary>
        /// Not set
        /// </summary>
        ModNotSet = -1,
        /// <summary>
        /// Not defined
        /// </summary>
        ModNotDefined = 0,
        /// <summary>
        /// 16 QAM
        /// </summary>
        Mod16Qam = 1,
        /// <summary>
        /// 32 QAM
        /// </summary>
        Mod32Qam,
        /// <summary>
        /// 64 QAM
        /// </summary>
        Mod64Qam,
        /// <summary>
        /// 80 QAM
        /// </summary>
        Mod80Qam,
        /// <summary>
        /// 96 QAM
        /// </summary>
        Mod96Qam,
        /// <summary>
        /// 112 QAM
        /// </summary>
        Mod112Qam,
        /// <summary>
        /// 128 QAM
        /// </summary>
        Mod128Qam,
        /// <summary>
        /// 160 QAM
        /// </summary>
        Mod160Qam,
        /// <summary>
        /// 192 QAM
        /// </summary>
        Mod192Qam,
        /// <summary>
        /// 224 QAM
        /// </summary>
        Mod224Qam,
        /// <summary>
        /// 256 QAM
        /// </summary>
        Mod256Qam,
        /// <summary>
        /// 320 QAM
        /// </summary>
        Mod320Qam,
        /// <summary>
        /// 384 QAM
        /// </summary>
        Mod384Qam,
        /// <summary>
        /// 448 QAM
        /// </summary>
        Mod448Qam,
        /// <summary>
        /// 512 QAM
        /// </summary>
        Mod512Qam,
        /// <summary>
        /// 640 QAM
        /// </summary>
        Mod640Qam,
        /// <summary>
        /// 768 QAM
        /// </summary>
        Mod768Qam,
        /// <summary>
        /// 896 QAM
        /// </summary>
        Mod896Qam,
        /// <summary>
        /// 1024 QAM
        /// </summary>
        Mod1024Qam,
        /// <summary>
        /// QPSK
        /// </summary>
        ModQpsk,
        /// <summary>
        /// BPSK
        /// </summary>
        ModBpsk,
        /// <summary>
        /// OQPSK
        /// </summary>
        ModOqpsk,
        /// <summary>
        /// 8VSB
        /// </summary>
        Mod8Vsb,
        /// <summary>
        /// 16VSB
        /// </summary>
        Mod16Vsb,
        /// <summary>
        /// Analogue
        /// </summary>
        ModAnalogAmplitude, // std am
        /// <summary>
        /// FM
        /// </summary>
        ModAnalogFrequency, // std fm
        /// <summary>
        /// 8PSK
        /// </summary>
        Mod8Psk,
        /// <summary>
        /// RF
        /// </summary>
        ModRF,
        /// <summary>
        /// 16 APSK
        /// </summary>
        Mod16Apsk,
        /// <summary>
        /// 32 APSK
        /// </summary>
        Mod32Apsk,
        /// <summary>
        /// NBC QPSK
        /// </summary>
        ModNbcQpsk,
        /// <summary>
        /// NBC 8PSK
        /// </summary>
        ModNbc8Psk,
        /// <summary>
        /// DirectTV
        /// </summary>
        ModDirectTv,
        /// <summary>
        /// Maximum entry.
        /// </summary>
        ModMax
    }

    /// <summary>
    /// From DVBSystemType
    /// </summary>
    public enum DVBSystemType
    {
        /// <summary>
        /// Cable
        /// </summary>
        Cable,
        /// <summary>
        /// Terrestrial
        /// </summary>
        Terrestrial,
        /// <summary>
        /// Satellite
        /// </summary>
        Satellite,
        /// <summary>
        /// ISDB terrestrial
        /// </summary>
        ISDBT,
        /// <summary>
        /// ISDB satellite
        /// </summary>
        ISDBS
    }

    /// <summary>
    /// From HierarchyAlpha
    /// </summary>
    public enum HierarchyAlpha
    {
        
        /// <summary>
        /// Not set
        /// </summary>
        HAlphaNotSet = -1,
        /// <summary>
        /// Not defined
        /// </summary>
        HAlphaNotDefined = 0,
        /// <summary>
        /// Alpha 1
        /// </summary>
        HAlpha1 = 1, // Hierarchy alpha is 1.
        /// <summary>
        /// Alpha 2
        /// </summary>
        HAlpha2, // Hierarchy alpha is 2.
        /// <summary>
        /// Alpha 4
        /// </summary>
        HAlpha4, // Hierarchy alpha is 4.
        /// <summary>
        /// Maximum entry.
        /// </summary>
        HAlphaMax,
    }

    /// <summary>
    /// From GuardInterval
    /// </summary>
    public enum GuardInterval
    {
        /// <summary>
        /// Not set
        /// </summary>
        GuardNotSet = -1,
        /// <summary>
        /// Not defined
        /// </summary>
        GuardNotDefined = 0,
        /// <summary>
        /// 1/32
        /// </summary>
        Guard1_32 = 1, // Guard interval is 1/32
        /// <summary>
        /// 1/16
        /// </summary>
        Guard1_16, // Guard interval is 1/16
        /// <summary>
        /// 1/8
        /// </summary>
        Guard1_8, // Guard interval is 1/8
        /// <summary>
        /// 1/4
        /// </summary>
        Guard1_4, // Guard interval is 1/4
        /// <summary>
        /// Maximum entry.
        /// </summary>
        GuardMax,
    }

    /// <summary>
    /// From TransmissionMode
    /// </summary>
    public enum TransmissionMode
    {
        /// <summary>
        /// Not set
        /// </summary>
        ModeNotSet = -1,
        /// <summary>
        /// Not defined
        /// </summary>
        ModeNotDefined = 0,
        /// <summary>
        /// 2k
        /// </summary>
        Mode2K = 1, // Transmission uses 1705 carriers (use a 2K FFT)
        /// <summary>
        /// 8k
        /// </summary>
        Mode8K, // Transmission uses 6817 carriers (use an 8K FFT)
        /// <summary>
        /// 4k
        /// </summary>
        Mode4K,
        /// <summary>
        /// 2k interleaved
        /// </summary>
        Mode2KInterleaved,
        /// <summary>
        /// 4k interleaved
        /// </summary>
        Mode4KInterleaved,
        /// <summary>
        /// Maximum entry.
        /// </summary>
        ModeMax,
    }

    /// <summary>
    /// From ComponentStatus
    /// </summary>
    public enum ComponentStatus
    {
        /// <summary>
        /// Active
        /// </summary>
        Active,
        /// <summary>
        /// Inactive
        /// </summary>
        Inactive,
        /// <summary>
        /// Unavailable
        /// </summary>
        Unavailable
    }

    /// <summary>
    /// From ComponentCategory
    /// </summary>
    public enum ComponentCategory
    {
        /// <summary>
        /// Not set
        /// </summary>
        NotSet = -1,
        /// <summary>
        /// Other
        /// </summary>
        Other = 0,
        /// <summary>
        /// Video
        /// </summary>
        Video,
        /// <summary>
        /// Audio
        /// </summary>
        Audio,
        /// <summary>
        /// Text
        /// </summary>
        Text,
        /// <summary>
        /// Data
        /// </summary>
        Data
    }

    /// <summary>
    /// From BDA_TEMPLATE_CONNECTION
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct BDATemplateConnection
    {
        /// <summary>
        /// From node type.
        /// </summary>
        public int FromNodeType;
        /// <summary>
        /// From node pin type.
        /// </summary>
        public int FromNodePinType;
        /// <summary>
        /// To node type.
        /// </summary>
        public int ToNodeType;
        /// <summary>
        /// To node pin type.
        /// </summary>
        public int ToNodePinType;
    }

    #endregion
}
