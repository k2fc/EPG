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

using System.Collections.ObjectModel;

namespace DomainObjects
{
    /// <summary>
    /// The class that describes the DVB signal modulation.
    /// </summary>
    public sealed class SignalModulation
    {
        /// <summary>
        /// Signal modulation values.
        /// </summary>
        public enum Modulation
        {
            /// <summary>
            /// The modulation value for QAM16.
            /// </summary>
            QAM16,
            /// <summary>
            /// The modulation value for QAM32.
            /// </summary>
            QAM32,
            /// <summary>
            /// The modulation value for QAM64.
            /// </summary>
            QAM64,
            /// <summary>
            /// The modulation value for QAM80.
            /// </summary>
            QAM80,
            /// <summary>
            /// The modulation value for QAM96.
            /// </summary>
            QAM96,
            /// <summary>
            /// The modulation value for QAM112.
            /// </summary>
            QAM112,
            /// <summary>
            /// The modulation value for QAM128.
            /// </summary>
            QAM128,
            /// <summary>
            /// The modulation value for QAM160.
            /// </summary>
            QAM160,
            /// <summary>
            /// The modulation value for QAM192.
            /// </summary>
            QAM192,
            /// <summary>
            /// The modulation value for QAM224.
            /// </summary>
            QAM224,
            /// <summary>
            /// The modulation value for QAM256.
            /// </summary>
            QAM256,
            /// <summary>
            /// The modulation value for QAM320.
            /// </summary>
            QAM320,
            /// <summary>
            /// The modulation value for QAM384.
            /// </summary>
            QAM384,
            /// <summary>
            /// The modulation value for QAM448.
            /// </summary>
            QAM448,
            /// <summary>
            /// The modulation value for QAM512.
            /// </summary>
            QAM512,
            /// <summary>
            /// The modulation value for QAM640.
            /// </summary>
            QAM640,
            /// <summary>
            /// The modulation value for QAM768.
            /// </summary>
            QAM768,
            /// <summary>
            /// The modulation value for QAM896.
            /// </summary>
            QAM896,
            /// <summary>
            /// The modulation value for QAM1024.
            /// </summary>
            QAM1024,
            /// <summary>
            /// The modulation value for QPSK.
            /// </summary>
            QPSK,
            /// <summary>
            /// The modulation value for BPSK.
            /// </summary>
            BPSK,
            /// <summary>
            /// The modulation value for OQPSK.
            /// </summary>
            OQPSK,
            /// <summary>
            /// The modulation value for VSB8.
            /// </summary>
            VSB8,
            /// <summary>
            /// The modulation value for VSB16.
            /// </summary>
            VSB16,
            /// <summary>
            /// The modulation value for AM radio.
            /// </summary>
            AMRadio,
            /// <summary>
            /// The modulation value for FM radio.
            /// </summary>
            FMRadio,
            /// <summary>
            /// The modulation value for PSK8.
            /// </summary>
            PSK8,
            /// <summary>
            /// The modulation value for RF.
            /// </summary>
            RF,
            /// <summary>
            /// The modulation value for VSB8 when used for ATSC over cable.
            /// </summary>
            VSB8Cable,
            /// <summary>
            /// The modulation value for VSB16 when used for ATSC over cable.
            /// </summary>
            VSB16Cable
        }

        private SignalModulation() { }

        /// <summary>
        /// Convert the DVB modulation to the internal modulation.
        /// </summary>
        /// <param name="modulation">The modulation to be converted.</param>
        /// <returns>The converted value.</returns>
        public static Modulation ConvertDVBModulation(int modulation)
        {
            switch (modulation)
            {
                case 0:
                    return (Modulation.QPSK);
                case 1:
                    return (Modulation.QPSK);
                case 2:
                    return (Modulation.PSK8);
                case 3:
                    return (Modulation.QAM16);
                default:
                    return (Modulation.QPSK);
            }
        }

        /// <summary>
        /// Get a list of the DVB-S modulations.
        /// </summary>
        /// <returns>A list of the DVB-S modulations.</returns>
        public static Collection<string> GetDvbsModulations()
        {
            Collection<string> modulations = new Collection<string>();
            
            modulations.Add(Modulation.QPSK.ToString());
            modulations.Add(Modulation.PSK8.ToString());
            modulations.Add(Modulation.QAM16.ToString());

            return (modulations);
        }

        /// <summary>
        /// Get the index number of a DVB-S modulation value.
        /// </summary>
        /// <param name="modulation">The modulation value.</param>
        /// <returns>The index number.</returns>
        public static int GetDvbsIndex(SignalModulation.Modulation modulation)
        {
            switch (modulation)
            {
                case Modulation.QPSK:
                    return (0);
                case Modulation.PSK8:
                    return (1);
                case Modulation.QAM16:
                    return (2);
                default:
                    return (0);
            }
        }

        /// <summary>
        /// Get the xml value of a DVB-S modulation value.
        /// </summary>
        /// <param name="modulation">The modulation value.</param>
        /// <returns>The xml value.</returns>
        public static string GetDvbsXml(SignalModulation.Modulation modulation)
        {
            switch (modulation)
            {
                case Modulation.QPSK:
                    return ("ModQPSK");
                case Modulation.PSK8:
                    return ("Mod8Psk");
                case Modulation.QAM16:
                    return ("Mod16Qam");
                default:
                    return ("ModQPSK");
            }
        }

        /// <summary>
        /// Get a list of the DVB-C modulations.
        /// </summary>
        /// <returns>A list of the DVB-C modulations.</returns>
        public static Collection<string> GetDvbcModulations()
        {
            Collection<string> modulations = new Collection<string>();

            modulations.Add(Modulation.QAM16.ToString());
            modulations.Add(Modulation.QAM32.ToString());
            modulations.Add(Modulation.QAM64.ToString());
            modulations.Add(Modulation.QAM128.ToString());
            modulations.Add(Modulation.QAM256.ToString());

            return (modulations);
        }

        /// <summary>
        /// Get the index number of a DVB-C modulation value.
        /// </summary>
        /// <param name="modulation">The modulation value.</param>
        /// <returns>The index number.</returns>
        public static int GetDvbcIndex(SignalModulation.Modulation modulation)
        {
            switch (modulation)
            {
                case Modulation.QAM16:
                    return (0);
                case Modulation.QAM32:
                    return (1);
                case Modulation.QAM64:
                    return (2);
                case Modulation.QAM128:
                    return (3);
                case Modulation.QAM256:
                    return (4);
                default:
                    return (0);
            }
        }

        /// <summary>
        /// Get the xml value of a DVB-C modulation value.
        /// </summary>
        /// <param name="modulation">The modulation value.</param>
        /// <returns>The xml value.</returns>
        public static string GetDvbcXml(SignalModulation.Modulation modulation)
        {
            switch (modulation)
            {
                case Modulation.QAM16:
                    return ("Mod16Qam");
                case Modulation.QAM32:
                    return ("Mod32Qam");
                case Modulation.QAM64:
                    return ("Mod64Qam");
                case Modulation.QAM128:
                    return ("Mod128Qam");
                case Modulation.QAM256:
                    return ("Mod256Qam");
                default:
                    return ("Mod16Qam");
            }
        }

        /// <summary>
        /// Get a list of the ATSC modulations.
        /// </summary>
        /// <returns>A list of the ATSC modulations.</returns>
        public static Collection<string> GetAtscModulations()
        {
            Collection<string> modulations = new Collection<string>();

            modulations.Add(Modulation.VSB8.ToString());
            modulations.Add(Modulation.VSB8Cable.ToString());
            modulations.Add(Modulation.VSB16.ToString());
            modulations.Add(Modulation.VSB16Cable.ToString());
            modulations.Add(Modulation.QAM16.ToString());
            modulations.Add(Modulation.QAM32.ToString());
            modulations.Add(Modulation.QAM64.ToString());
            modulations.Add(Modulation.QAM80.ToString());
            modulations.Add(Modulation.QAM96.ToString());
            modulations.Add(Modulation.QAM112.ToString());
            modulations.Add(Modulation.QAM128.ToString());
            modulations.Add(Modulation.QAM160.ToString());
            modulations.Add(Modulation.QAM192.ToString());
            modulations.Add(Modulation.QAM224.ToString());            
            modulations.Add(Modulation.QAM256.ToString());
            modulations.Add(Modulation.QAM320.ToString());
            modulations.Add(Modulation.QAM384.ToString());
            modulations.Add(Modulation.QAM448.ToString());
            modulations.Add(Modulation.QAM512.ToString());
            modulations.Add(Modulation.QAM640.ToString());
            modulations.Add(Modulation.QAM768.ToString());
            modulations.Add(Modulation.QAM896.ToString());
            modulations.Add(Modulation.QAM1024.ToString());

            return (modulations);
        }

        /// <summary>
        /// Get the index number of an ATSC modulation value.
        /// </summary>
        /// <param name="modulation">The modulation value.</param>
        /// <returns>The index number.</returns>
        public static int GetAtscIndex(SignalModulation.Modulation modulation)
        {
            switch (modulation)
            {
                case Modulation.VSB8:
                    return (0);
                case Modulation.VSB8Cable:
                    return (1);
                case Modulation.VSB16:
                    return (2);
                case Modulation.VSB16Cable:
                    return (3);
                case Modulation.QAM16:
                    return (4);
                case Modulation.QAM32:
                    return (5);
                case Modulation.QAM64:
                    return (6);
                case Modulation.QAM80:
                    return (7);
                case Modulation.QAM96:
                    return (8);
                case Modulation.QAM112:
                    return (9);
                case Modulation.QAM128:
                    return (10);
                case Modulation.QAM160:
                    return (11);
                case Modulation.QAM192:
                    return (12);
                case Modulation.QAM224:
                    return (13);
                case Modulation.QAM256:
                    return (14);
                case Modulation.QAM320:
                    return (15);
                case Modulation.QAM384:
                    return (16);
                case Modulation.QAM448:
                    return (17);
                case Modulation.QAM512:
                    return (18);
                case Modulation.QAM640:
                    return (19);
                case Modulation.QAM768:
                    return (20);
                case Modulation.QAM896:
                    return (21);
                case Modulation.QAM1024:
                    return (22);
                default:
                    return (0);
            }
        }

        /// <summary>
        /// Get the xml value of an ATSC modulation value.
        /// </summary>
        /// <param name="modulation">The modulation value.</param>
        /// <returns>The xml value.</returns>
        public static string GetAtscXml(SignalModulation.Modulation modulation)
        {
            switch (modulation)
            {
                case Modulation.VSB8:
                    return ("VSB8");
                case Modulation.VSB16:
                    return ("VSB16");
                case Modulation.QAM16:
                    return ("QAM16");
                case Modulation.QAM32:
                    return ("QAM32");
                case Modulation.QAM64:
                    return ("QAM64");
                case Modulation.QAM80:
                    return ("QAM80");
                case Modulation.QAM96:
                    return ("QAM96");
                case Modulation.QAM112:
                    return ("QAM112");
                case Modulation.QAM128:
                    return ("QAM128");
                case Modulation.QAM160:
                    return ("QAM160");
                case Modulation.QAM192:
                    return ("QAM192");
                case Modulation.QAM224:
                    return ("QAM224");
                case Modulation.QAM256:
                    return ("QAM256");
                case Modulation.QAM320:
                    return ("QAM320");
                case Modulation.QAM384:
                    return ("QAM384");
                case Modulation.QAM448:
                    return ("QAM448");
                case Modulation.QAM512:
                    return ("QAM512");
                case Modulation.QAM640:
                    return ("QAM640");
                case Modulation.QAM768:
                    return ("QAM768");
                case Modulation.QAM896:
                    return ("QAM896");
                case Modulation.QAM1024:
                    return ("QAM1024");
                default:
                    return ("VSB8");
            }
        }

        /// <summary>
        /// Get a list of the ClearQAM modulations.
        /// </summary>
        /// <returns>A list of the ClearQAM modulations.</returns>
        public static Collection<string> GetClearQamModulations()
        {
            Collection<string> modulations = new Collection<string>();

            modulations.Add(Modulation.VSB8.ToString());
            modulations.Add(Modulation.VSB16.ToString());
            modulations.Add(Modulation.QAM16.ToString());
            modulations.Add(Modulation.QAM32.ToString());
            modulations.Add(Modulation.QAM64.ToString());
            modulations.Add(Modulation.QAM80.ToString());
            modulations.Add(Modulation.QAM96.ToString());
            modulations.Add(Modulation.QAM112.ToString());
            modulations.Add(Modulation.QAM128.ToString());
            modulations.Add(Modulation.QAM160.ToString());
            modulations.Add(Modulation.QAM192.ToString());
            modulations.Add(Modulation.QAM224.ToString());
            modulations.Add(Modulation.QAM256.ToString());
            modulations.Add(Modulation.QAM320.ToString());
            modulations.Add(Modulation.QAM384.ToString());
            modulations.Add(Modulation.QAM448.ToString());
            modulations.Add(Modulation.QAM512.ToString());
            modulations.Add(Modulation.QAM640.ToString());
            modulations.Add(Modulation.QAM768.ToString());
            modulations.Add(Modulation.QAM896.ToString());
            modulations.Add(Modulation.QAM1024.ToString());

            return (modulations);
        }

        /// <summary>
        /// Get the index number of a ClearQAM modulation value.
        /// </summary>
        /// <param name="modulation">The modulation value.</param>
        /// <returns>The index number.</returns>
        public static int GetClearQamIndex(SignalModulation.Modulation modulation)
        {
            switch (modulation)
            {
                case Modulation.VSB8:
                    return (0);
                case Modulation.VSB16:
                    return (1);
                case Modulation.QAM16:
                    return (2);
                case Modulation.QAM32:
                    return (3);
                case Modulation.QAM64:
                    return (4);
                case Modulation.QAM80:
                    return (5);
                case Modulation.QAM96:
                    return (6);
                case Modulation.QAM112:
                    return (7);
                case Modulation.QAM128:
                    return (8);
                case Modulation.QAM160:
                    return (9);
                case Modulation.QAM192:
                    return (10);
                case Modulation.QAM224:
                    return (11);
                case Modulation.QAM256:
                    return (12);
                case Modulation.QAM320:
                    return (13);
                case Modulation.QAM384:
                    return (14);
                case Modulation.QAM448:
                    return (15);
                case Modulation.QAM512:
                    return (16);
                case Modulation.QAM640:
                    return (17);
                case Modulation.QAM768:
                    return (18);
                case Modulation.QAM896:
                    return (19);
                case Modulation.QAM1024:
                    return (20);
                default:
                    return (0);
            }
        }

        /// <summary>
        /// Get the xml value of a ClearQAM modulation value.
        /// </summary>
        /// <param name="modulation">The modulation value.</param>
        /// <returns>The xml value.</returns>
        public static string GetClearQamXml(SignalModulation.Modulation modulation)
        {
            switch (modulation)
            {
                case Modulation.VSB8:
                    return ("VSB8");
                case Modulation.VSB16:
                    return ("VSB16");
                case Modulation.QAM16:
                    return ("QAM16");
                case Modulation.QAM32:
                    return ("QAM32");
                case Modulation.QAM64:
                    return ("QAM64");
                case Modulation.QAM80:
                    return ("QAM80");
                case Modulation.QAM96:
                    return ("QAM96");
                case Modulation.QAM112:
                    return ("QAM112");
                case Modulation.QAM128:
                    return ("QAM128");
                case Modulation.QAM160:
                    return ("QAM160");
                case Modulation.QAM192:
                    return ("QAM192");
                case Modulation.QAM224:
                    return ("QAM224");
                case Modulation.QAM256:
                    return ("QAM256");
                case Modulation.QAM320:
                    return ("QAM320");
                case Modulation.QAM384:
                    return ("QAM384");
                case Modulation.QAM448:
                    return ("QAM448");
                case Modulation.QAM512:
                    return ("QAM512");
                case Modulation.QAM640:
                    return ("QAM640");
                case Modulation.QAM768:
                    return ("QAM768");
                case Modulation.QAM896:
                    return ("QAM896");
                case Modulation.QAM1024:
                    return ("QAM1024");
                default:
                    return ("VSB8");
            }
        }
    }
}
