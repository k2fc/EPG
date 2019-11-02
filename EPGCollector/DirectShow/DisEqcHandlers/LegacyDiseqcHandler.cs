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

using System.Runtime.InteropServices;

using DirectShowAPI;

using DomainObjects;

namespace DirectShow
{
    internal class LegacyDiseqcHandler : DiseqcHandlerBase
    {
        internal override string Description { get { return ("Legacy"); } }
        internal override bool CardCapable { get { return (cardCapable); } }

        private IBDA_FrequencyFilter frequencyFilter;
        private IBDA_DeviceControl deviceControl;

        private IBaseFilter tunerFilter;
        private bool cardCapable;

        private int reply;

        /// <summary>
        /// Initializes a new instance of the GenericDiseqcHandler class.
        /// </summary>
        /// <param name="tunerFilter">The tuner filter.</param>
        internal LegacyDiseqcHandler(IBaseFilter tunerFilter)
        {
            this.tunerFilter = tunerFilter;

            frequencyFilter = checkPutRangeSupported(tunerFilter);
            cardCapable = frequencyFilter != null;

            deviceControl = tunerFilter as IBDA_DeviceControl;
            /*parameterBuffer = Marshal.AllocCoTaskMem(instanceSize);
            instanceBuffer = Marshal.AllocCoTaskMem(instanceSize);*/
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
                Logger.Instance.Write("Legacy DiSEqC Handler: Using Set Range method");
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
            if (lnbNumber > 4)
            {
                Logger.Instance.Write("Legacy DiSEqC Handler: Port number " + lnbNumber + " cannot be processed using Set Range method");
                return (false);
            }

            reply = deviceControl.StartChanges();
            if (reply != 0)
            {
                Logger.Instance.Write("Legacy DiSEqC Handler: Start changes failed with reply 0x" + reply.ToString("X"));
                return (false);
            }
            else
                Logger.Instance.Write("Legacy DiSEqC Handler: Start changes succeeded");

            Logger.Instance.Write("Legacy DiSEqC Handler: Processing using set range for port " + lnbNumber);

            lnbNumber -= 1;
            if (lnbNumber > 1)
            {
                lnbNumber -= 2;
                lnbNumber |= 0x100;
            }

            Logger.Instance.Write("Legacy DiSEqC Handler: Setting range " + lnbNumber);
            reply = frequencyFilter.put_Range(lnbNumber);
            if (reply != 0)
            {
                Logger.Instance.Write("Legacy DiSEqC Handler: Put range failed with reply 0x" + reply.ToString("X"));
                return (false);
            }
            else
                Logger.Instance.Write("Legacy DiSEqC Handler: Put range succeeded");

            reply = deviceControl.CheckChanges();
            if (reply != 0)
            {
                Logger.Instance.Write("Legacy DiSEqC Handler: Check changes failed with reply 0x" + reply.ToString("X"));
                return (false);
            }
            else
                Logger.Instance.Write("Legacy DiSEqC Handler: Check changes succeeded");

            reply = deviceControl.CommitChanges();
            if (reply != 0)
            {
                Logger.Instance.Write("Legacy DiSEqC Handler: Commit changes failed with reply 0x" + reply.ToString("X"));
                return (false);
            }
            else
                Logger.Instance.Write("Legacy DiSEqC Handler: Commit changes succeeded");

            return (true);
        }

        private bool processCommands(string commands, DiseqcRunParameters diseqcRunParameters)
        {
            Logger.Instance.Write("Legacy DiSEqC Handler: Commands cannot be processed using Set Range method");
            return (false);
        }
    }
}
