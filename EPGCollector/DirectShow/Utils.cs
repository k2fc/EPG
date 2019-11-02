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

using DomainObjects;
using DirectShowAPI;

namespace DirectShow
{
    /// <summary>
    /// Helper methods.
    /// </summary>
    public sealed class Utils
    {
        private Utils() { }

        /// <summary>
        /// Get the DirectShow FEC rate.
        /// </summary>
        public static BinaryConvolutionCodeRate GetNativeFECRate(FECRate fec)
        {
            switch (fec.Rate)
            {
                case FECRate.FECRate12:
                    return (BinaryConvolutionCodeRate.Rate1_2);
                case FECRate.FECRate13:
                    return (BinaryConvolutionCodeRate.Rate1_3);
                case FECRate.FECRate14:
                    return (BinaryConvolutionCodeRate.Rate1_4);
                case FECRate.FECRate23:
                    return (BinaryConvolutionCodeRate.Rate2_3);
                case FECRate.FECRate25:
                    return (BinaryConvolutionCodeRate.Rate2_5);
                case FECRate.FECRate34:
                    return (BinaryConvolutionCodeRate.Rate3_4);
                case FECRate.FECRate35:
                    return (BinaryConvolutionCodeRate.Rate3_5);
                case FECRate.FECRate45:
                    return (BinaryConvolutionCodeRate.Rate4_5);
                case FECRate.FECRate511:
                    return (BinaryConvolutionCodeRate.Rate5_11);
                case FECRate.FECRate56:
                    return (BinaryConvolutionCodeRate.Rate5_6);
                case FECRate.FECRate67:
                    return (BinaryConvolutionCodeRate.Rate6_7);
                case FECRate.FECRate78:
                    return (BinaryConvolutionCodeRate.Rate7_8);
                case FECRate.FECRate89:
                    return (BinaryConvolutionCodeRate.Rate8_9);
                case FECRate.FECRate910:
                    return (BinaryConvolutionCodeRate.Rate9_10);
                case FECRate.FECRateMax:
                    return (BinaryConvolutionCodeRate.RateMax);
                default:
                    return (BinaryConvolutionCodeRate.Rate3_4);
            }
        }

        /// <summary>
        /// Get the DirectShow modulation.
        /// </summary>
        public static ModulationType GetNativeModulation(SignalModulation.Modulation modulation)
        {
            switch (modulation)
            {
                case SignalModulation.Modulation.AMRadio:
                    return (ModulationType.ModAnalogAmplitude);
                case SignalModulation.Modulation.BPSK:
                    return (ModulationType.ModBpsk);
                case SignalModulation.Modulation.FMRadio:
                    return (ModulationType.ModAnalogFrequency);
                case SignalModulation.Modulation.OQPSK:
                    return (ModulationType.ModOqpsk);
                case SignalModulation.Modulation.PSK8:
                    return (ModulationType.Mod8Psk);
                case SignalModulation.Modulation.QAM1024:
                    return (ModulationType.Mod1024Qam);
                case SignalModulation.Modulation.QAM112:
                    return (ModulationType.Mod112Qam);
                case SignalModulation.Modulation.QAM128:
                    return (ModulationType.Mod128Qam);
                case SignalModulation.Modulation.QAM16:
                    return (ModulationType.Mod16Qam);
                case SignalModulation.Modulation.QAM160:
                    return (ModulationType.Mod160Qam);
                case SignalModulation.Modulation.QAM192:
                    return (ModulationType.Mod192Qam);
                case SignalModulation.Modulation.QAM224:
                    return (ModulationType.Mod224Qam);
                case SignalModulation.Modulation.QAM256:
                    return (ModulationType.Mod256Qam);
                case SignalModulation.Modulation.QAM32:
                    return (ModulationType.Mod32Qam);
                case SignalModulation.Modulation.QAM320:
                    return (ModulationType.Mod320Qam);
                case SignalModulation.Modulation.QAM384:
                    return (ModulationType.Mod384Qam);
                case SignalModulation.Modulation.QAM448:
                    return (ModulationType.Mod448Qam);
                case SignalModulation.Modulation.QAM512:
                    return (ModulationType.Mod512Qam);
                case SignalModulation.Modulation.QAM64:
                    return (ModulationType.Mod64Qam);
                case SignalModulation.Modulation.QAM768:
                    return (ModulationType.Mod768Qam);
                case SignalModulation.Modulation.QAM80:
                    return (ModulationType.Mod80Qam);
                case SignalModulation.Modulation.QAM896:
                    return (ModulationType.Mod896Qam);
                case SignalModulation.Modulation.QAM96:
                    return (ModulationType.Mod96Qam);
                case SignalModulation.Modulation.QPSK:
                    return (ModulationType.ModQpsk);
                case SignalModulation.Modulation.RF:
                    return (ModulationType.ModRF);
                case SignalModulation.Modulation.VSB16:
                case SignalModulation.Modulation.VSB16Cable:
                    return (ModulationType.Mod16Vsb);
                case SignalModulation.Modulation.VSB8:
                case SignalModulation.Modulation.VSB8Cable:
                    return (ModulationType.Mod8Vsb);
                default:
                    return (ModulationType.ModQpsk);
            }
        }

        /// <summary>
        /// Get the DirectShow polarization for a local polarization.
        /// </summary>
        /// <param name="polarization">The local polarization.</param>
        /// <returns>The native polarization.</returns>
        public static Polarisation GetNativePolarization(SignalPolarization polarization)
        {
            return (GetNativePolarization(polarization.Polarization));
        }

        /// <summary>
        /// Get the DirectShow polarization for a local polarization.
        /// </summary>
        /// <param name="polarization">The local polarization.</param>
        /// <returns>The native polarization.</returns>
        public static Polarisation GetNativePolarization(string polarization)
        {
            if (polarization == SignalPolarization.LinearHorizontal)
                return (Polarisation.LinearH);
            if (polarization == SignalPolarization.LinearVertical)
                return (Polarisation.LinearV);
            if (polarization == SignalPolarization.CircularLeft)
                return (Polarisation.CircularL);
            if (polarization == SignalPolarization.CircularRight)
                return (Polarisation.CircularR);

            return (Polarisation.LinearH);
        }
    }
}
