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

using DomainObjects;
using DirectShowAPI;

namespace DirectShow
{
    internal sealed class GenericDVBT2Handler
    {
        private static readonly Guid KSPROPSETID_BdaDigitalDemodulator = new Guid("EF30F379-985B-4d10-B640-A79D5E04E1E0");    

        enum KSPROPERTY_BDA_DIGITAL_DEMODULATOR
        {
            KSPROPERTY_BDA_MODULATION_TYPE = 0,
            KSPROPERTY_BDA_INNER_FEC_TYPE,
            KSPROPERTY_BDA_INNER_FEC_RATE,
            KSPROPERTY_BDA_OUTER_FEC_TYPE,
            KSPROPERTY_BDA_OUTER_FEC_RATE,
            KSPROPERTY_BDA_SYMBOL_RATE,
            KSPROPERTY_BDA_SPECTRAL_INVERSION,
            KSPROPERTY_BDA_GUARD_INTERVAL,
            KSPROPERTY_BDA_TRANSMISSION_MODE,
            KSPROPERTY_BDA_ROLL_OFF,
            KSPROPERTY_BDA_PILOT,
            KSPROPERTY_BDA_SIGNALTIMEOUTS,
            KSPROPERTY_BDA_PLP_NUMBER
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KSPROPERTY
        {
            Guid Set;
            [MarshalAs(UnmanagedType.U4)]
            int Id;
            [MarshalAs(UnmanagedType.U4)]
            int Flags;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct KSPNODE
        {
            KSPROPERTY Property;
            [MarshalAs(UnmanagedType.U4)]
            public int NodeId;
            [MarshalAs(UnmanagedType.U4)]
            public int Resevred;
        }

        private GenericDVBT2Handler() { }

        internal static object KSGetNode(IKsPropertySet ksTarget, Guid ksGuid, int ksParam, Type ksType)
        {
            object obj;

            int dataPtrSize = Marshal.SizeOf(ksType);
            IntPtr dataPtr = Marshal.AllocCoTaskMem(dataPtrSize);
            int instancePtrSize = Marshal.SizeOf(typeof(KSPNODE));
            IntPtr instancePtr = Marshal.AllocCoTaskMem(instancePtrSize);

            try
            {
                int cbBytes;
                int reply = ksTarget.Get(ksGuid, ksParam, instancePtr, instancePtrSize, dataPtr, dataPtrSize, out cbBytes);
                if (reply != 0)
                {
                    Logger.Instance.Write("Generic DVB-T2 handler: KSPropertySet KSP_NODE GET method failed - " + reply.ToString("X"));
                    return (null);
                }

                obj = Marshal.PtrToStructure(dataPtr, ksType);
            }
            finally
            {
                if (dataPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(dataPtr);
                if (instancePtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(instancePtr);
            }

            return (obj);
        }

        public static bool KSSetNode(IKsPropertySet ksTarget, Guid ksGuid, int ksParam, object ksStructure)
        {
            int dataPtrSize = Marshal.SizeOf(ksStructure);
            IntPtr dataPtr = Marshal.AllocCoTaskMem(dataPtrSize);
            int instancePtrSize = Marshal.SizeOf(typeof(KSPNODE));
            IntPtr instancePtr = Marshal.AllocCoTaskMem(instancePtrSize);

            Marshal.StructureToPtr(ksStructure, dataPtr, true);

            try
            {
                int reply = ksTarget.Set(ksGuid, ksParam, instancePtr, instancePtrSize, dataPtr, dataPtrSize);
                if (reply != 0)
                {
                    Logger.Instance.Write("Generic DVB-T2 handler: KSPropertySet KSP_NODE SET method failed - " + reply.ToString("X"));
                    return (false);
                }
            }
            finally
            {
                if (dataPtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(dataPtr);
                if (instancePtr != IntPtr.Zero)
                    Marshal.FreeCoTaskMem(instancePtr);
            }

            return (true);
        }

        public static bool KSSupported(IKsPropertySet propSet, Guid ksGuid, int ksParam, KSPropertySupport supportedFlag)
        {
            KSPropertySupport supported;
            
            int reply = propSet.QuerySupported(ksGuid, ksParam, out supported);
            if (reply != 0)
                return (false);
            else
                return (supported.HasFlag(supportedFlag));
        }

        internal static void SetPlp(IBaseFilter tunerFilter, int plp)
        {
            IPin pin = DsFindPin.ByDirection(tunerFilter, PinDirection.Output, 0);
            if (pin == null)
            {
                Logger.Instance.Write("Generic DVB-T2 handler: Tuner output pin not found - PLP not set");
                return;
            }

            bool plpSupported = KSSupported((IKsPropertySet)pin, KSPROPSETID_BdaDigitalDemodulator,
                (int)KSPROPERTY_BDA_DIGITAL_DEMODULATOR.KSPROPERTY_BDA_PLP_NUMBER, KSPropertySupport.Set);

            if (plpSupported)
            {
                bool reply = KSSetNode((IKsPropertySet)pin, KSPROPSETID_BdaDigitalDemodulator,
                    (int)KSPROPERTY_BDA_DIGITAL_DEMODULATOR.KSPROPERTY_BDA_PLP_NUMBER, plp);
                if (reply)
                    Logger.Instance.Write("Generic DVB-T2 handler: PLP set to plp");
                else
                    Logger.Instance.Write("Generic DVB-T2 handler: Property set failed - PLP not set");
            }
            else
                Logger.Instance.Write("Generic DVB-T2 handler: Set property not supported - PLP not set");

        }
    }
}
