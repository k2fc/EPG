////////////////////////////////////////////////////////////////////////////////// 
//                                                                              //
//      Copyright (C) 2005-2016 nzsjb                                          //
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

using DirectShowAPI;

using DomainObjects;

namespace DirectShow
{
    /// <summary>
    /// The base class for Diseqc handlers.
    /// </summary>
    public abstract class DiseqcHandlerBase
    {
        /// <summary>
        /// Get the description of the handler.
        /// </summary>
        internal abstract string Description { get; }

        /// <summary>
        /// Return true if the card is Diseqc capable.
        /// </summary>
        internal abstract bool CardCapable { get; }

        /// <summary>
        /// Get a list of DiSEqC handlers.
        /// </summary>
        public static Collection<string> Handlers
        {
            get
            {
                Collection<string> handlers = new Collection<string>();

                handlers.Add("Default");
                handlers.Add("Generic");
                handlers.Add("Legacy");
                handlers.Add("Azurewave");
                handlers.Add("Conexant");
                handlers.Add("DigitalEverywhere");
                handlers.Add("Hauppauge");
                handlers.Add("TBS");
                handlers.Add("TechnoTrend");
                handlers.Add("Tevii");
                handlers.Add("Twinhan");

                return (handlers);
            }
        }

        /// <summary>
        /// Modulator settings.
        /// </summary>
        protected enum BdaDigitalModulator
        {
            /// <summary>
            /// Type.
            /// </summary>
            MODULATION_TYPE = 0,
            /// <summary>
            /// Inner FEC type..
            /// </summary>
            INNER_FEC_TYPE,
            /// <summary>
            /// Inner FEC rate.
            /// </summary>
            INNER_FEC_RATE,
            /// <summary>
            /// Outer FEC type.
            /// </summary>
            OUTER_FEC_TYPE,
            /// <summary>
            /// Outer FEC rate.
            /// </summary>
            OUTER_FEC_RATE,
            /// <summary>
            /// Symbol rate.
            /// </summary>
            SYMBOL_RATE,
            /// <summary>
            /// Spectral inversion.
            /// </summary>
            SPECTRAL_INVERSION,
            /// <summary>
            /// Guard interval.
            /// </summary>
            GUARD_INTERVAL,
            /// <summary>
            /// Transmission mode.
            /// </summary>
            TRANSMISSION_MODE
        }
        
        /// <summary>
        /// The tuner extension properties.
        /// </summary>
        protected enum BdaTunerExtension
        {
            /// <summary>
            /// DiSEqC property.
            /// </summary>
            KSPROPERTY_BDA_DISEQC = 0,
            /// <summary>
            /// Scan frequency property.
            /// </summary>
            KSPROPERTY_BDA_SCAN_FREQ,
            /// <summary>
            /// Channel change property.
            /// </summary>
            KSPROPERTY_BDA_CHANNEL_CHANGE,
            /// <summary>
            /// Effective frequency property.
            /// </summary>
            KSPROPERTY_BDA_EFFECTIVE_FREQ,
            /// <summary>
            /// Pilot property.
            /// </summary>
            KSPROPERTY_BDA_PILOT = 0x20,
            /// <summary>
            /// Rolloff property.
            /// </summary>
            KSPROPERTY_BDA_ROLL_OFF = 0x21
        }

        /// <summary>
        /// The DiSEqC version.
        /// </summary>
        protected enum DisEqcVersion
        {
            /// <summary>
            /// DiSEqC version 1X.
            /// </summary>
            DISEQC_VER_1X = 1,
            /// <summary>
            /// DiSEqC version 2.
            /// </summary>
            DISEQC_VER_2
        }

        /// <summary>
        /// Burst modulation type.
        /// </summary>
        protected enum BurstModulationType
        {
            /// <summary>
            /// Unmodulated.
            /// </summary>
            TONE_BURST_UNMODULATED = 0,
            /// <summary>
            /// Modulated.
            /// </summary>
            TONE_BURST_MODULATED
        }

        /// <summary>
        /// Receive mode.
        /// </summary>
        protected enum RxMode
        {
            /// <summary>
            /// Interrogation mode.
            /// </summary>
            RXMODE_INTERROGATION = 1,
            /// <summary>
            /// Quick reply mode.
            /// </summary>
            RXMODE_QUICKREPLY,
            /// <summary>
            /// No reply mode.
            /// </summary>
            RXMODE_NOREPLY,
            /// <summary>
            /// Default mode.
            /// </summary>
            RXMODE_DEFAULT = 0
        }

        internal static SwitchReply ProcessDisEqcSwitch(TuningSpec tuningSpec, Tuner tunerSpec, IBaseFilter tunerFilter, DiseqcRunParameters diseqcRunParameters)
        {
            DiseqcHandlerBase diseqcHandler = getDiseqcHandler(tunerSpec, tunerFilter, diseqcRunParameters);
            if (diseqcHandler == null)
            {
                Logger.Instance.Write("No DiSEqC handler available - switch request ignored");
                return (SwitchReply.NoHandler);
            }

            Logger.Instance.Write("Created " + diseqcHandler.Description + " DiSEqC handler");

            bool reply = diseqcHandler.SendDiseqcCommand(tuningSpec, ((SatelliteFrequency)tuningSpec.Frequency).DiseqcRunParamters.DiseqcSwitch, diseqcRunParameters);
            if (reply)
                return (SwitchReply.OK);
            else
                return (SwitchReply.Failed);
        }

        private static DiseqcHandlerBase getDiseqcHandler(Tuner tuner, IBaseFilter tunerFilter, DiseqcRunParameters diseqcRunParameters)
        {
            if (diseqcRunParameters.DiseqcHandler != null)
            {
                switch (diseqcRunParameters.DiseqcHandler.ToUpperInvariant())
                {
                    case "DEFAULT":
                        break;
                    case "HAUPPAUGE":
                        return (createHauppaugeHandler(tunerFilter, true));
                    case "TECHNOTREND":
                        return (createTechnoTrendHandler(tunerFilter, true));
                    case "CONEXANT":
                        return (createConexantHandler(tuner, tunerFilter, true));
                    case "TWINHAN":
                    case "AZUREWAVE":
                        return (createTwinhanHandler(tunerFilter, true));
                    case "TEVII":
                        return (createTeviiHandler(tuner, tunerFilter, true));
                    case "PROFRED":
                    case "TBS":
                        return (createProfRedHandler(tunerFilter, true));
                    case "DIGITALEVERYWHERE":
                        return (createDigitalEverywhereHandler(tunerFilter, true));
                    case "GENERIC":
                        return (createGenericHandler(tunerFilter, true));
                    case "LEGACY":
                        return (createLegacyHandler(tunerFilter, true));
                    default:
                        Logger.Instance.Write("DiSEqC handler '" + diseqcRunParameters.DiseqcHandler + "' not recognized");
                        return (null);
                }
            }

            DiseqcHandlerBase diseqcHandler = createHauppaugeHandler(tunerFilter, false);
            if (diseqcHandler != null)
            {
                Logger.Instance.Write("DiSEqC processing using Hauppauge method");
                return (diseqcHandler);
            }

            diseqcHandler = createTechnoTrendHandler(tunerFilter, false);
            if (diseqcHandler != null)
            {
                Logger.Instance.Write("DiSEqC processing using TechnoTrend method");
                return (diseqcHandler);
            }

            diseqcHandler = createTwinhanHandler(tunerFilter, false);
            if (diseqcHandler != null)
            {
                Logger.Instance.Write("DiSEqC processing using Twinhan/TechniSat method");
                return (diseqcHandler);
            }

            diseqcHandler = createConexantHandler(tuner, tunerFilter, false);
            if (diseqcHandler != null)
            {
                Logger.Instance.Write("DiSEqC processing using Conexant method");
                return (diseqcHandler);
            }

            diseqcHandler = createTeviiHandler(tuner, tunerFilter, false);
            if (diseqcHandler != null)
            {
                Logger.Instance.Write("DiSEqC processing using Tevii method");
                return (diseqcHandler);
            }

            diseqcHandler = createProfRedHandler(tunerFilter, false);
            if (diseqcHandler != null)
            {
                Logger.Instance.Write("DiSEqC processing using ProfRed/TBS method");
                return (diseqcHandler);
            }

            diseqcHandler = createDigitalEverywhereHandler(tunerFilter, false);
            if (diseqcHandler != null)
            {
                Logger.Instance.Write("DiSEqC processing using DigitalEverywhere method");
                return (diseqcHandler);
            }

            diseqcHandler = createGenericHandler(tunerFilter, false);
            if (diseqcHandler != null)
            {
                Logger.Instance.Write("DiSEqC processing using Generic method");
                return (diseqcHandler);
            }

            diseqcHandler = createLegacyHandler(tunerFilter, false);
            if (diseqcHandler != null)
            {
                Logger.Instance.Write("DiSEqC processing using Legacy method");
                return (diseqcHandler);
            }

            return (null);
        }

        private static DiseqcHandlerBase createHauppaugeHandler(IBaseFilter tunerFilter, bool logMessage)
        {
            HauppaugeDiseqcHandler hauppaugeHandler = new HauppaugeDiseqcHandler(tunerFilter);

            if (hauppaugeHandler.CardCapable)
                return (hauppaugeHandler);

            if (logMessage)
                Logger.Instance.Write("Hauppauge card is not DiSEqC capable");

            return (null);
        }

        private static DiseqcHandlerBase createTechnoTrendHandler(IBaseFilter tunerFilter, bool logMessage)
        {
            TechnoTrendDiseqcHandler technotrendHandler = new TechnoTrendDiseqcHandler(tunerFilter);

            if (technotrendHandler.CardCapable)
                return (technotrendHandler);

            if (logMessage)
                Logger.Instance.Write("TechnoTrend card is not DiSEqC capable");

            return (null);
        }

        private static DiseqcHandlerBase createTwinhanHandler(IBaseFilter tunerFilter, bool logMessage)
        {
            TwinhanDiseqcHandler twinhanHandler = new TwinhanDiseqcHandler(tunerFilter);

            if (twinhanHandler.CardCapable)
                return (twinhanHandler);

            if (logMessage)
                Logger.Instance.Write("Twinhan/Azurewave card is not DiSEqC capable");

            return (null);
        }

        private static DiseqcHandlerBase createConexantHandler(Tuner tuner, IBaseFilter tunerFilter, bool logMessage)
        {
            ConexantDiseqcHandler conexantHandler = new ConexantDiseqcHandler(tunerFilter, tuner);

            if (conexantHandler.CardCapable)
                return (conexantHandler);

            if (logMessage)
                Logger.Instance.Write("Conexant card is not DiSEqC capable");

            return (null);
        }

        private static DiseqcHandlerBase createTeviiHandler(Tuner tuner, IBaseFilter tunerFilter, bool logMessage)
        {
            TeviiDiseqcHandler teviiHandler = new TeviiDiseqcHandler(tunerFilter, tuner);

            if (teviiHandler.CardCapable)
                return (teviiHandler);

            if (logMessage)
                Logger.Instance.Write("Tevii card is not DiSEqC capable");

            return (null);
        }

        private static DiseqcHandlerBase createProfRedHandler(IBaseFilter tunerFilter, bool logMessage)
        {
            ProfRedDiseqcHandler profRedHandler = new ProfRedDiseqcHandler(tunerFilter);

            if (profRedHandler.CardCapable)
                return (profRedHandler);

            if (logMessage)
                Logger.Instance.Write("ProfRed card is not DiSEqC capable");

            return (null);
        }

        private static DiseqcHandlerBase createDigitalEverywhereHandler(IBaseFilter tunerFilter, bool logMessage)
        {
            DigitalEverywhereDiseqcHandler digitalEverywhereHandler = new DigitalEverywhereDiseqcHandler(tunerFilter);

            if (digitalEverywhereHandler.CardCapable)
                return (digitalEverywhereHandler);

            if (logMessage)
                Logger.Instance.Write("DigitalEverywhere card is not DiSEqC capable");

            return (null);
        }

        private static DiseqcHandlerBase createGenericHandler(IBaseFilter tunerFilter, bool logMessage)
        {
            GenericDiseqcHandler genericHandler = new GenericDiseqcHandler(tunerFilter);

            if (genericHandler.CardCapable)
                return (genericHandler);

            if (logMessage)
                Logger.Instance.Write("Generic card is not DiSEqC capable");

            return (null);
        }

        private static DiseqcHandlerBase createLegacyHandler(IBaseFilter tunerFilter, bool logMessage)
        {
            LegacyDiseqcHandler legacyHandler = new LegacyDiseqcHandler(tunerFilter);

            if (legacyHandler.CardCapable)
                return (legacyHandler);

            if (logMessage)
                Logger.Instance.Write("Legacy card is not DiSEqC capable");

            return (null);
        }

        /// <summary>
        /// Get the LNB number.
        /// </summary>
        /// <param name="port">The port identifier.</param>
        /// <returns>The LNB number.</returns>
        protected int GetLnbNumber(string port)
        {
            int lnbNumber = 0;

            switch (port)
            {
                case "A":
                    lnbNumber = 1;
                    break;
                case "B":
                    lnbNumber = 2;
                    break;
                case "AA":
                    lnbNumber = 1;
                    break;
                case "AB":
                    lnbNumber = 2;
                    break;
                case "BA":
                    lnbNumber = 3;
                    break;
                case "BB":
                    lnbNumber = 4;
                    break;
                case "PORT1":
                    lnbNumber = 5;
                    break;
                case "PORT2":
                    lnbNumber = 6;
                    break;
                case "PORT3":
                    lnbNumber = 7;
                    break;
                case "PORT4":
                    lnbNumber = 8;
                    break;
                case "PORT5":
                    lnbNumber = 9;
                    break;
                case "PORT6":
                    lnbNumber = 10;
                    break;
                case "PORT7":
                    lnbNumber = 11;
                    break;
                case "PORT8":
                    lnbNumber = 12;
                    break;
                case "PORT9":
                    lnbNumber = 13;
                    break;
                case "PORT10":
                    lnbNumber = 14;
                    break;
                case "PORT11":
                    lnbNumber = 15;
                    break;
                case "PORT12":
                    lnbNumber = 16;
                    break;
                case "PORT13":
                    lnbNumber = 17;
                    break;
                case "PORT14":
                    lnbNumber = 18;
                    break;
                case "PORT15":
                    lnbNumber = 19;
                    break;
                case "PORT16":
                    lnbNumber = 20;
                    break;
                case "AAPORT1":
                    lnbNumber = 21;
                    break;
                case "ABPORT1":
                    lnbNumber = 22;
                    break;
                case "BAPORT1":
                    lnbNumber = 23;
                    break;
                case "BBPORT1":
                    lnbNumber = 24;
                    break;
                case "AAPORT2":
                    lnbNumber = 25;
                    break;
                case "ABPORT2":
                    lnbNumber = 26;
                    break;
                case "BAPORT2":
                    lnbNumber = 27;
                    break;
                case "BBPORT2":
                    lnbNumber = 28;
                    break;
                case "AAPORT3":
                    lnbNumber = 29;
                    break;
                case "ABPORT3":
                    lnbNumber = 30;
                    break;
                case "BAPORT3":
                    lnbNumber = 31;
                    break;
                case "BBPORT3":
                    lnbNumber = 32;
                    break;
                case "AAPORT4":
                    lnbNumber = 33;
                    break;
                case "ABPORT4":
                    lnbNumber = 34;
                    break;
                case "BAPORT4":
                    lnbNumber = 35;
                    break;
                case "BBPORT4":
                    lnbNumber = 36;
                    break;
                default:
                    lnbNumber = -1;
                    break;
            }

            return (lnbNumber);
        }

        /// <summary>
        /// Convert an array of bytes to a hex string.
        /// </summary>
        /// <param name="byteData">The array of bytes.</param>
        /// <returns>The converted hex string.</returns>
        protected string ConvertToHex(byte[] byteData)
        {
            char[] outputChars = new char[byteData.Length * 2];
            int outputIndex = 0;

            for (int inputIndex = 0; inputIndex < byteData.Length; inputIndex++)
            {
                int hexByteLeft = byteData[inputIndex] >> 4;
                int hexByteRight = byteData[inputIndex] & 0x0f;

                outputChars[outputIndex] = getHex(hexByteLeft);
                outputChars[outputIndex + 1] = getHex(hexByteRight);

                outputIndex += 2;
            }

            return ("0x" + new string(outputChars));
        }

        private static char getHex(int value)
        {
            if (value < 10)
                return ((char)('0' + value));

            return ((char)('a' + (value - 10)));
        }

        /// <summary>
        /// Get the DiSEqC command from a string.
        /// </summary>
        /// <param name="hexCommand">The string.</param>
        /// <returns>The command bytes.</returns>
        protected byte[] GetCommand(string hexCommand)
        {
            string[] hexPairs = hexCommand.Split(new char[] { ' ' });

            byte[] hexBytes = new byte[hexPairs.Length];

            try
            {
                for (int index = 0; index < hexPairs.Length; index++)
                    hexBytes[index] = byte.Parse(hexPairs[index].Trim(), System.Globalization.NumberStyles.HexNumber);
            }
            catch (FormatException)
            {
                return(hexBytes);
            }
            catch (OverflowException)
            {
                return (hexBytes);
            }

            return (hexBytes);
        }

        /// <summary>
        /// Get the DiSEqC command string
        /// </summary>
        /// <param name="lnbNumber">The LNB number.</param>
        /// <param name="tuningSpec">A tuning spec instance.</param>
        /// <returns>The command bytes.</returns>
        protected byte[] GetCommand(int lnbNumber, TuningSpec tuningSpec)
        {
            byte[] commandBytes = new byte[4] { 0xe0, 0x10, 0x00, 0x00 };

            if (lnbNumber < 5)
            {
                commandBytes[2] = 0x38;
                commandBytes[3] = 0xf0;

                //bit 0	(1)	: 0=low band, 1 = hi band
                //bit 1 (2) : 0=vertical, 1 = horizontal
                //bit 3 (4) : 0=satellite position A, 1=satellite position B
                //bit 4 (8) : 0=switch option A, 1=switch option  B
                // LNB    option  position
                // 1        A         A
                // 2        A         B
                // 3        B         A
                // 4        B         B

                bool hiBand = (tuningSpec.Frequency.Frequency > ((SatelliteFrequency)tuningSpec.Frequency).SatelliteDish.LNBSwitchFrequency);
                bool isHorizontal = (tuningSpec.SignalPolarization.Polarization == SignalPolarization.LinearHorizontal) || (tuningSpec.SignalPolarization.Polarization == SignalPolarization.CircularLeft);

                commandBytes[3] |= (byte)(hiBand ? 1 : 0);
                commandBytes[3] |= (byte)((isHorizontal) ? 2 : 0);
                commandBytes[3] |= (byte)((lnbNumber - 1) << 2);
            }
            else
            {
                if (lnbNumber < 21)
                {
                    commandBytes[2] = 0x39;
                    commandBytes[3] = (byte)(lnbNumber - 5);
                }
                else
                {
                    commandBytes[2] = 0x38;
                    commandBytes[3] = 0xf0;

                    bool hiBand = (tuningSpec.Frequency.Frequency > ((SatelliteFrequency)tuningSpec.Frequency).SatelliteDish.LNBSwitchFrequency);
                    bool isHorizontal = (tuningSpec.SignalPolarization.Polarization == SignalPolarization.LinearHorizontal || tuningSpec.SignalPolarization.Polarization == SignalPolarization.CircularLeft);

                    commandBytes[3] |= (byte)(hiBand ? 1 : 0);
                    commandBytes[3] |= (byte)((isHorizontal) ? 2 : 0);

                    if (lnbNumber >= 21 && lnbNumber <= 24)
                        commandBytes[3] |= (byte)((lnbNumber - 21) << 2);
                    else
                    {
                        if (lnbNumber >= 25 && lnbNumber <= 28)
                            commandBytes[3] |= (byte)((lnbNumber - 25) << 2);
                        else
                        {
                            if (lnbNumber >= 29 && lnbNumber <= 32)
                                commandBytes[3] |= (byte)((lnbNumber - 29) << 2);
                            else
                                commandBytes[3] |= (byte)((lnbNumber - 33) << 2);
                        }
                    }
                }
            }

            return (commandBytes);
        }

        /// <summary>
        /// Get the second DiSEqC command.
        /// </summary>
        /// <param name="lnbNumber">The LNB number.</param>
        /// <param name="tuningSpec">A tuning spec instance.</param>
        /// <returns>The DiSEqC command bytes.</returns>
        protected byte[] GetSecondCommand(int lnbNumber, TuningSpec tuningSpec)
        {
            byte[] commandBytes = new byte[4] { 0xe0, 0x10, 0x39, 0x00 };

            if (lnbNumber < 21 || lnbNumber > 36)
                return(null);
            else
            {
                if (lnbNumber >= 21 && lnbNumber <= 24)
                    commandBytes[3] = 0;
                else
                {
                    if (lnbNumber >= 25 && lnbNumber <= 28)
                        commandBytes[3] = 1;
                    else
                    {
                        if (lnbNumber >= 29 && lnbNumber <= 32)
                            commandBytes[3] = 2;
                        else
                            commandBytes[3] = 3;
                    }
                }
            }

            return (commandBytes);
        }

        /// <summary>
        /// Check if a string represents the generic handler.
        /// </summary>
        /// <param name="name">The string to check.</param>
        /// <returns>True if it is generic; false otherwise.</returns>
        public static bool IsGeneric(string name)
        {
            return (name == "Generic");
        }

        internal abstract bool SendDiseqcCommand(TuningSpec tuningSpec, string port, DiseqcRunParameters diseqcRunParameters);        
    }
}
