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
using System.Threading;

using DirectShowAPI;

using DomainObjects;

namespace DirectShow
{
    internal class GenericDiseqcHandler : DiseqcHandlerBase
    {
        private enum bdaDiseqcProperty
        {
            Enable = 0,
            LnbSource,
            UseToneBurst,
            Repeats,
            Send,
            Response
        }

        private enum bdaDemodulatorProperty
        {
            ModulationType = 0,
            InnerFecType,
            InnerFecRate,
            OuterFecType,
            OuterFecRate,
            SymbolRate,
            SpectralInversion,
            TransmissionMode,
            RollOff,
            Pilot,
            SignalTimeouts,
            PlpNumber
        }

        private struct BdaDiseqcMessage
        {
            public UInt32 RequestId;
            public UInt32 PacketLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = maxDiseqcMessageLength)]
            public byte[] PacketData;
        }

        internal override string Description { get { return ("Generic"); } }
        internal override bool CardCapable { get { return (cardCapable); } }

        private IKsPropertySet diseqCommandPropertySet;
        private IBDA_FrequencyFilter frequencyFilter;
        private IBDA_DeviceControl deviceControl;

        private uint requestID = 1;
        private const int instanceSize = 32;
        private const int paramSize = 4;
        private const int diseqcMessageSize = 16;
        private const int maxDiseqcMessageLength = 8;
        
        private IBaseFilter tunerFilter;
        private bool cardCapable;

        private IntPtr instanceBuffer = IntPtr.Zero;
        private IntPtr parameterBuffer = IntPtr.Zero;        

        private int reply;

        /// <summary>
        /// Initializes a new instance of the GenericDiseqcHandler class.
        /// </summary>
        /// <param name="tunerFilter">The tuner filter.</param>
        internal GenericDiseqcHandler(IBaseFilter tunerFilter)
        {
            this.tunerFilter = tunerFilter;

            diseqCommandPropertySet = checkDiseqCommandSupported(tunerFilter);
            if (diseqCommandPropertySet == null)
                frequencyFilter = checkPutRangeSupported(tunerFilter);
            
            cardCapable = diseqCommandPropertySet != null || frequencyFilter != null;

            deviceControl = tunerFilter as IBDA_DeviceControl;
            parameterBuffer = Marshal.AllocCoTaskMem(instanceSize);
            instanceBuffer = Marshal.AllocCoTaskMem(instanceSize);
        }

        private IKsPropertySet checkDiseqCommandSupported(IBaseFilter filter)
        {
            IPin pin = DsFindPin.ByDirection(filter, PinDirection.Input, 0);
            if (pin == null)
                return (null);

            IKsPropertySet propertySet = pin as IKsPropertySet;
            if (propertySet == null)
            {
                Marshal.ReleaseComObject(pin);
                return (null);
            }

            KSPropertySupport support;
            reply = propertySet.QuerySupported(typeof(IBDA_DiseqCommand).GUID, (int)bdaDiseqcProperty.LnbSource, out support);
            if (reply != 0 || (support & KSPropertySupport.Set) == 0)
            {
                Marshal.ReleaseComObject(pin);
                return (null);
            }

            Logger.Instance.Write("Generic DiSEqC Handler: Using property set commands");

            return (propertySet);
        }

        private IBDA_FrequencyFilter checkPutRangeSupported(IBaseFilter filter)
        {
            IBDA_Topology topology = filter as IBDA_Topology;
            if (topology == null)
                return (null);

            object controlNode;
            reply = topology.GetControlNode(0, 1, 0, out controlNode);
            if (reply == 0 && controlNode is IBDA_FrequencyFilter)
            {
                Logger.Instance.Write("Generic DiSEqC Handler: Using Set Range method");
                return (controlNode as IBDA_FrequencyFilter);
            }
            
            if (controlNode != null)
                Marshal.ReleaseComObject(controlNode);

            return (null);
        }

        /// <summary>
        /// Sends the diseq command.
        /// </summary>
        /// <param name="tuningSpec">A tuning spec instance.</param>
        /// <param name="port">The Diseqc port ( eg AB).</param>        
        /// <param name="diseqcRunParameters">Additional parameters.</param>
        /// <returns>True if successful; false otherwise.</returns>
        internal override bool SendDiseqcCommand(TuningSpec tuningSpec, string port, DiseqcRunParameters diseqcRunParameters)
        {
            if (!cardCapable)
                return (false);

            int lnbNumber = GetLnbNumber(port);
            if (lnbNumber != -1)
                return (processPort(lnbNumber, tuningSpec, diseqcRunParameters));
            else
                return (processCommands(port, diseqcRunParameters));
        }

        private bool processPort(int lnbNumber, TuningSpec tuningSpec, DiseqcRunParameters diseqcRunParameters)
        {
            if (diseqCommandPropertySet == null && lnbNumber > 4)
            {
                Logger.Instance.Write("Generic DiSEqC Handler: Port number " + lnbNumber + " cannot be processed using Set Range method");
                return (false);
            }

            reply = deviceControl.StartChanges();
            if (reply != 0)
            {
                Logger.Instance.Write("Generic DiSEqC Handler: Start changes failed with reply 0x" + reply.ToString("X"));
                return (false);
            }
            else
                Logger.Instance.Write("Generic DiSEqC Handler: Start changes succeeded");

            if (diseqCommandPropertySet != null)
            {
                Logger.Instance.Write("Generic DiSEqC Handler: Processing using command property set for port number " + lnbNumber);

                string enableDisable;
                
                if (!diseqcRunParameters.DisableDriverDiseqc)
                {
                    Marshal.WriteInt32(parameterBuffer, 0, 1);
                    enableDisable = "enable";
                }
                else
                {
                    Marshal.WriteInt32(parameterBuffer, 0, 0);
                    enableDisable = "disable";
                }
                reply = diseqCommandPropertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)bdaDiseqcProperty.Enable, instanceBuffer, instanceSize, parameterBuffer, 4);
                if (reply != 0)
                    Logger.Instance.Write("Generic DiSEqC Handler: Command " + enableDisable + " failed with reply 0x" + reply.ToString("X"));
                else
                    Logger.Instance.Write("Generic DiSEqC Handler: Command " + enableDisable + " succeeded");

                Marshal.WriteInt32(parameterBuffer, 0, 0);
                reply = diseqCommandPropertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)bdaDiseqcProperty.Repeats, instanceBuffer, instanceSize, parameterBuffer, 4);
                if (reply != 0)
                    Logger.Instance.Write("Generic DiSEqC Handler: Set repeats failed with reply 0x" + reply.ToString("X"));
                else
                    Logger.Instance.Write("Generic DiSEqC Handler: Set repeats succeeded");
                    
                Marshal.WriteInt32(parameterBuffer, 0, 0);
                reply = diseqCommandPropertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)bdaDiseqcProperty.UseToneBurst, instanceBuffer, instanceSize, parameterBuffer, 4);
                if (reply != 0)
                    Logger.Instance.Write("Generic DiSEqC Handler: Disable tone burst failed with reply 0x" + reply.ToString("X"));
                else
                    Logger.Instance.Write("Generic DiSEqC Handler: Disable tone burst succeeded");
                    
                if (lnbNumber < 5 && !diseqcRunParameters.UseDiseqcCommand)
                {
                    Logger.Instance.Write("Generic DiSEqC Handler: Setting LNB source to " + lnbNumber); 

                    Marshal.WriteInt32(parameterBuffer, 0, lnbNumber);
                    reply = diseqCommandPropertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)bdaDiseqcProperty.LnbSource, instanceBuffer, instanceSize, parameterBuffer, 4);
                    if (reply != 0)
                    {
                        Logger.Instance.Write("Generic DiSEqC Handler: Set LNB source failed with reply 0x" + reply.ToString("X"));
                        return (false);
                    }
                    else
                        Logger.Instance.Write("Generic DiSEqC Handler: Set LNB source succeeded");
                }
                else
                {
                    byte[] command = GetCommand(lnbNumber, tuningSpec);

                    Logger.Instance.Write("Generic DiSEqC Handler: sending command " + ConvertToHex(command));

                    BdaDiseqcMessage message = new BdaDiseqcMessage();
                    message.RequestId = requestID;
                    message.PacketLength = (uint)command.Length;
                    message.PacketData = new byte[maxDiseqcMessageLength];
                    Buffer.BlockCopy(command, 0, message.PacketData, 0, command.Length);
                    Marshal.StructureToPtr(message, parameterBuffer, true);
                    
                    reply = diseqCommandPropertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)bdaDiseqcProperty.Send, instanceBuffer, instanceSize, parameterBuffer, diseqcMessageSize);
                    if (reply != 0)
                    {
                        Logger.Instance.Write("Generic DiSEqC Handler: Send command failed with reply 0x" + reply.ToString("X"));
                        return (false);
                    }
                    else
                        Logger.Instance.Write("Generic DiSEqC Handler: Send command succeeded");

                    command = GetSecondCommand(lnbNumber, tuningSpec);
                    if (command != null)
                    {
                        Thread.Sleep(150);

                        Logger.Instance.Write("Generic DiSEqC Handler: sending second command " + ConvertToHex(command));

                        message = new BdaDiseqcMessage();
                        message.RequestId = requestID + 1;
                        message.PacketLength = (uint)command.Length;
                        message.PacketData = new byte[maxDiseqcMessageLength];
                        Buffer.BlockCopy(command, 0, message.PacketData, 0, command.Length);
                        Marshal.StructureToPtr(message, parameterBuffer, true);

                        reply = diseqCommandPropertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)bdaDiseqcProperty.Send, instanceBuffer, instanceSize, parameterBuffer, diseqcMessageSize);
                        if (reply != 0)
                        {
                            Logger.Instance.Write("Generic DiSEqC Handler: Send second command failed with reply 0x" + reply.ToString("X"));
                            return (false);
                        }
                        else
                            Logger.Instance.Write("Generic DiSEqC Handler: Send second command succeeded");
                    }
                }
            }
            else
            {
                Logger.Instance.Write("Generic DiSEqC Handler: Processing using set range for port " + lnbNumber);

                lnbNumber -= 1;
                if (lnbNumber > 1)
                {
                    lnbNumber -= 2;
                    lnbNumber |= 0x100;
                }

                Logger.Instance.Write("Generic DiSEqC Handler: Setting range " + lnbNumber);
                reply = frequencyFilter.put_Range(lnbNumber);
                if (reply != 0)
                {
                    Logger.Instance.Write("Generic DiSEqC Handler: Put range failed with reply 0x" + reply.ToString("X"));
                    return (false);
                }
                else
                    Logger.Instance.Write("Generic DiSEqC Handler: Put range succeeded");

            }

            reply = deviceControl.CheckChanges();
            if (reply != 0)
            {
                Logger.Instance.Write("Generic DiSEqC Handler: Check changes failed with reply 0x" + reply.ToString("X"));
                return (false);
            }
            else
                Logger.Instance.Write("Generic DiSEqC Handler: Check changes succeeded");

            reply = deviceControl.CommitChanges();
            if (reply != 0)
            {
                Logger.Instance.Write("Generic DiSEqC Handler: Commit changes failed with reply 0x" + reply.ToString("X"));
                return (false);
            }
            else
                Logger.Instance.Write("Generic DiSEqC Handler: Commit changes succeeded");

            return (true);
        }

        private bool processCommands(string commands, DiseqcRunParameters diseqcRunParameters)
        {
            if (diseqCommandPropertySet == null)
                return (false);

            string[] commandStrings = commands.Split(new char[] { ':' });

            Logger.Instance.Write("Generic DiSEqC Handler: Processing " + commandStrings.Length + " command strings"); 

            reply = deviceControl.StartChanges();
            if (reply != 0)
            {
                Logger.Instance.Write("Generic DiSEqC Handler: Start changes failed with reply 0x" + reply.ToString("X"));
                return (false);
            }
            else
                Logger.Instance.Write("Generic DiSEqC Handler: Start changes succeeded");

            string enableDisable;

            if (!diseqcRunParameters.DisableDriverDiseqc)
            {
                Marshal.WriteInt32(parameterBuffer, 0, 1);
                enableDisable = "enable";
            }
            else
            {
                Marshal.WriteInt32(parameterBuffer, 0, 0);
                enableDisable = "disable";
            }
            reply = diseqCommandPropertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)bdaDiseqcProperty.Enable, instanceBuffer, instanceSize, parameterBuffer, 4);
            if (reply != 0)
                Logger.Instance.Write("Generic DiSEqC Handler: Command " + enableDisable + " failed with reply 0x" + reply.ToString("X"));                
            else
                Logger.Instance.Write("Generic DiSEqC Handler: Command " + enableDisable + " succeeded");

            Marshal.WriteInt32(parameterBuffer, 0, 0);
            reply = diseqCommandPropertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)bdaDiseqcProperty.Repeats, instanceBuffer, instanceSize, parameterBuffer, 4);
            if (reply != 0)
                Logger.Instance.Write("Generic DiSEqC Handler: Set repeats failed with reply 0x" + reply.ToString("X"));
            else
                Logger.Instance.Write("Generic DiSEqC Handler: Set repeats succeeded");

            Marshal.WriteInt32(parameterBuffer, 0, 0);
            reply = diseqCommandPropertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)bdaDiseqcProperty.UseToneBurst, instanceBuffer, instanceSize, parameterBuffer, 4);
            if (reply != 0)
                Logger.Instance.Write("Generic DiSEqC Handler: Disable tone burst failed with reply 0x" + reply.ToString("X"));
            else
                Logger.Instance.Write("Generic DiSEqC Handler: Disable tone burst succeeded");

            foreach (string commandString in commandStrings)
            {
                byte[] command = GetCommand(commandString);

                BdaDiseqcMessage message = new BdaDiseqcMessage();
                message.RequestId = requestID;
                message.PacketLength = (uint)command.Length;
                message.PacketData = new byte[maxDiseqcMessageLength];
                Buffer.BlockCopy(command, 0, message.PacketData, 0, command.Length);
                Marshal.StructureToPtr(message, parameterBuffer, true);

                Logger.Instance.Write("Generic DiSEqC Handler: Processing command " + ConvertToHex(command)); 

                reply = diseqCommandPropertySet.Set(typeof(IBDA_DiseqCommand).GUID, (int)bdaDiseqcProperty.Send, instanceBuffer, instanceSize, parameterBuffer, diseqcMessageSize);
                if (reply != 0)
                {
                    Logger.Instance.Write("Generic DiSEqC Handler: Send command failed with reply 0x" + reply.ToString("X"));
                    return (false);
                }
                else
                    Logger.Instance.Write("Generic DiSEqC Handler: Send command succeeded");

                Thread.Sleep(150);
            }

            reply = deviceControl.CheckChanges();
            if (reply != 0)
            {
                Logger.Instance.Write("Generic DiSEqC Handler: Check changes failed with reply 0x" + reply.ToString("X"));
                return (false);
            }
            else
                Logger.Instance.Write("Generic DiSEqC Handler: Check changes succeeded");

            reply = deviceControl.CommitChanges();
            if (reply != 0)
            {
                Logger.Instance.Write("Generic DiSEqC Handler: Commit changes failed with reply 0x" + reply.ToString("X"));
                return (false);
            }
            else
                Logger.Instance.Write("Generic DiSEqC Handler: Commit changes succeeded");

            return (true);
        }
    }
}
