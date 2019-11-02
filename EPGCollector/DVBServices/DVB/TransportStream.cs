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
using System.Collections.ObjectModel;

using DomainObjects;

namespace DVBServices
{
    /// <summary>
    /// The class that describes a transport stream.
    /// </summary>
    public class TransportStream
    {
        /// <summary>
        /// Get the transport strema identification (TSID).
        /// </summary>
        public int TransportStreamID { get { return (transportStreamID); } }
        /// <summary>
        /// Get the original network identification (ONID).
        /// </summary>
        public int OriginalNetworkID { get { return (originalNetworkID); } }
        /// <summary>
        /// Get the collection of descriptors describing this transport stream.
        /// </summary>
        internal Collection<DescriptorBase> Descriptors { get { return (descriptors); } }
        /// <summary>
        /// Get the total length of the transport stream data.
        /// </summary>
        public int TotalLength { get { return (totalLength); } }

        /// <summary>
        /// Get the frequency.
        /// </summary>
        public int Frequency
        {
            get
            {
                if (Descriptors == null)
                    return (-1);

                foreach (DescriptorBase descriptor in Descriptors)
                {
                    DVBDeliverySystemDescriptor deliveryDescriptor = descriptor as DVBDeliverySystemDescriptor;
                    if (deliveryDescriptor != null)
                        return (deliveryDescriptor.Frequency);                    
                }

                return (-1);
            }
        }

        /// <summary>
        /// Get the inner FEC rate.
        /// </summary>
        public int Fec
        {
            get
            {
                if (Descriptors == null)
                    return (-1);

                foreach (DescriptorBase descriptor in Descriptors)
                {
                    DVBSatelliteDeliverySystemDescriptor satelliteDescriptor = descriptor as DVBSatelliteDeliverySystemDescriptor;
                    if (satelliteDescriptor != null)
                        return (satelliteDescriptor.InnerFEC);                    
                }

                return (-1);
            }
        }

        /// <summary>
        /// Get the modulation.
        /// </summary>
        public int Modulation
        {
            get
            {
                if (Descriptors == null)
                    return (-1);

                foreach (DescriptorBase descriptor in Descriptors)
                {
                    DVBSatelliteDeliverySystemDescriptor satelliteDescriptor = descriptor as DVBSatelliteDeliverySystemDescriptor;
                    if (satelliteDescriptor != null)
                        return (satelliteDescriptor.ModulationType);                    
                }

                return (-1);
            }
        }

        /// <summary>
        /// Get the modulation system.
        /// </summary>
        public int ModulationSystem
        {
            get
            {
                if (Descriptors == null)
                    return (0);

                foreach (DescriptorBase descriptor in Descriptors)
                {
                    DVBSatelliteDeliverySystemDescriptor satelliteDescriptor = descriptor as DVBSatelliteDeliverySystemDescriptor;
                    if (satelliteDescriptor != null)
                        return (satelliteDescriptor.ModulationSystem);
                }

                return (0);
            }
        }

        /// <summary>
        /// Return true if the modulation system is S2; false otherwise.
        /// </summary>
        public bool IsS2 { get { return (ModulationSystem == 1); } }

        /// <summary>
        /// Get the polarization.
        /// </summary>
        public int Polarization
        {
            get
            {
                if (Descriptors == null)
                    return (-1);

                foreach (DescriptorBase descriptor in Descriptors)
                {
                    DVBSatelliteDeliverySystemDescriptor satelliteDescriptor = descriptor as DVBSatelliteDeliverySystemDescriptor;
                    if (satelliteDescriptor != null)
                        return (satelliteDescriptor.Polarization);                    
                }

                return (-1);
            }
        }

        /// <summary>
        /// Get the S2 roll off.
        /// </summary>
        public int RollOff
        {
            get
            {
                if (Descriptors == null)
                    return (-1);

                foreach (DescriptorBase descriptor in Descriptors)
                {
                    DVBSatelliteDeliverySystemDescriptor satelliteDescriptor = descriptor as DVBSatelliteDeliverySystemDescriptor;
                    if (satelliteDescriptor != null)
                        return (satelliteDescriptor.RollOff);                    
                }

                return (-1);
            }
        }

        /// <summary>
        /// Get the symbol rate.
        /// </summary>
        public int SymbolRate
        {
            get
            {
                if (Descriptors == null)
                    return (-1);

                foreach (DescriptorBase descriptor in Descriptors)
                {
                    DVBSatelliteDeliverySystemDescriptor satelliteDescriptor = descriptor as DVBSatelliteDeliverySystemDescriptor;
                    if (satelliteDescriptor != null)
                        return (satelliteDescriptor.SymbolRate);                    
                }

                return (-1);
            }
        }

        /// <summary>
        /// Get the bandwidth.
        /// </summary>
        public int Bandwidth
        {
            get
            {
                if (Descriptors == null)
                    return (-1);

                foreach (DescriptorBase descriptor in Descriptors)
                {
                    DVBTerrestrialDeliverySystemDescriptor terrestrialDescriptor = descriptor as DVBTerrestrialDeliverySystemDescriptor;
                    if (terrestrialDescriptor != null)
                        return (terrestrialDescriptor.Bandwidth);

                    DVBT2DeliverySystemDescriptor t2Descriptor = descriptor as DVBT2DeliverySystemDescriptor;
                    if (t2Descriptor != null)
                        return (t2Descriptor.Bandwidth);
                }

                return (-1);
            }
        }

        /// <summary>
        /// Get the symbol rate for a cable transport stream.
        /// </summary>
        public int CableSymbolRate
        {
            get
            {
                if (Descriptors == null)
                    return (-1);

                foreach (DescriptorBase descriptor in Descriptors)
                {
                    DVBCableDeliverySystemDescriptor cableDescriptor = descriptor as DVBCableDeliverySystemDescriptor;
                    if (cableDescriptor != null)
                        return (cableDescriptor.SymbolRate);
                }

                return (-1);
            }
        }

        /// <summary>
        /// Get the cable inner FEC rate.
        /// </summary>
        public int CableFec
        {
            get
            {
                if (Descriptors == null)
                    return (-1);

                foreach (DescriptorBase descriptor in Descriptors)
                {
                    DVBCableDeliverySystemDescriptor cableDescriptor = descriptor as DVBCableDeliverySystemDescriptor;
                    if (cableDescriptor != null)
                        return (cableDescriptor.InnerFEC);
                }

                return (-1);
            }
        }

        /// <summary>
        /// Get the cable modulation.
        /// </summary>
        public int CableModulation
        {
            get
            {
                if (Descriptors == null)
                    return (-1);

                foreach (DescriptorBase descriptor in Descriptors)
                {
                    DVBCableDeliverySystemDescriptor cableDescriptor = descriptor as DVBCableDeliverySystemDescriptor;
                    if (cableDescriptor != null)
                        return (cableDescriptor.Modulation);
                }

                return (-1);
            }
        }

        /// <summary>
        /// Get the service list for this transport stream.
        /// </summary>
        public Collection<ServiceListEntry> ServiceList
        {
            get
            {
                if (Descriptors == null)
                    return (null);

                Collection<ServiceListEntry> serviceList = new Collection<ServiceListEntry>();

                foreach (DescriptorBase descriptor in Descriptors)
                {
                    DVBServiceListDescriptor serviceListDescriptor = descriptor as DVBServiceListDescriptor;
                    if (serviceListDescriptor != null)
                    {
                        foreach (ServiceListEntry newEntry in serviceListDescriptor.ServiceList)
                        {
                            bool inserted = false;

                            foreach (ServiceListEntry oldEntry in serviceList)
                            {
                                if (oldEntry.ServiceID > newEntry.ServiceID)
                                {
                                    serviceList.Insert(serviceList.IndexOf(oldEntry), newEntry);
                                    inserted = true;
                                    break;
                                }
                            }

                            if (!inserted)
                                serviceList.Add(newEntry);
                        }                       
                    }
                }

                return (serviceList);
            }
        }

        /// <summary>
        /// Returns true if the transport stream is satellite; false otherwise.
        /// </summary>
        public bool IsSatellite
        {
            get
            {
                foreach (DescriptorBase descriptor in Descriptors)
                {
                    DVBSatelliteDeliverySystemDescriptor satelliteDescriptor = descriptor as DVBSatelliteDeliverySystemDescriptor;
                    if (satelliteDescriptor != null)
                        return (true);
                }
                return (false);
            }
        }

        /// <summary>
        /// Returns true if the transport stream is terrestrial; false otherwise.
        /// </summary>
        public bool IsTerrestrial
        {
            get
            {
                foreach (DescriptorBase descriptor in Descriptors)
                {
                    DVBTerrestrialDeliverySystemDescriptor terrestrialDescriptor = descriptor as DVBTerrestrialDeliverySystemDescriptor;
                    if (terrestrialDescriptor != null)
                        return (true);

                    DVBT2DeliverySystemDescriptor t2Descriptor = descriptor as DVBT2DeliverySystemDescriptor;
                    if (t2Descriptor != null)
                        return (true);
                }
                return (false);
            }
        }

        /// <summary>
        /// Returns true if the transport stream is cable; false otherwise.
        /// </summary>
        public bool IsCable
        {
            get
            {
                foreach (DescriptorBase descriptor in Descriptors)
                {
                    DVBCableDeliverySystemDescriptor cableDescriptor = descriptor as DVBCableDeliverySystemDescriptor;
                    if (cableDescriptor != null)
                        return (true);
                }
                return (false);
            }
        }

        /// <summary>
        /// Get the index of the next byte in the MPEG2 section following the transport stream.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The transport stream has not been processed.
        /// </exception> 
        public int Index
        {
            get
            {
                if (lastIndex == -1)
                    throw (new InvalidOperationException("TransportStream: Index requested before block processed"));
                return (lastIndex);
            }
        }

        private int transportStreamID;
        private int originalNetworkID;
        private Collection<DescriptorBase> descriptors;
        private int totalLength;

        private int lastIndex = -1;

        /// <summary>
        /// Initialize a new instance of the TransportStream class.
        /// </summary>
        public TransportStream() { }

        /// <summary>
        /// Parse the entry.
        /// </summary>
        /// <param name="byteData">The MPEG2 section containing the transport stream.</param>
        /// <param name="index">Index of the first byte of the transport stream in the MPEG2 section.</param>
        /// <param name="scope">The scope of the processing..</param>
        internal void Process(byte[] byteData, int index, Scope scope)
        {
            lastIndex = index;

            try
            {
                transportStreamID = Utils.Convert2BytesToInt(byteData, lastIndex);
                lastIndex += 2;

                originalNetworkID = Utils.Convert2BytesToInt(byteData, lastIndex);
                lastIndex += 2;

                int descriptorLoopLength = ((byteData[lastIndex] & 0x0f) * 256) + (int)byteData[lastIndex + 1];
                lastIndex += 2;

                totalLength = descriptorLoopLength + 6;

                if (descriptorLoopLength != 0)
                {
                    descriptors = new Collection<DescriptorBase>();

                    while (descriptorLoopLength != 0)
                    {
                        DescriptorBase descriptor = DescriptorBase.Instance(byteData, lastIndex, scope);

                        if (!descriptor.IsEmpty)
                        {
                            descriptors.Add(descriptor);

                            lastIndex = descriptor.Index;
                            descriptorLoopLength -= descriptor.TotalLength;
                        }
                        else
                        {
                            lastIndex += DescriptorBase.MinimumDescriptorLength;
                            descriptorLoopLength -= DescriptorBase.MinimumDescriptorLength;
                        }
                    }
                }

                Validate();
            }
            catch (IndexOutOfRangeException)
            {
                throw (new ArgumentOutOfRangeException("The DVB Transport Stream message is short"));
            }
        }

        /// <summary>
        /// Validate the transport stream fields.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// A transport stream field is not valid.
        /// </exception>
        public void Validate() { }

        /// <summary>
        /// Log the transport stream fields.
        /// </summary>
        public void LogMessage() 
        {
            if (Logger.ProtocolLogger == null)
                return;

            Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "TRANSPORT STREAM: TSID: " + transportStreamID +
                " ONID: " + originalNetworkID);

            if (descriptors != null)
            {
                Logger.IncrementProtocolIndent();

                foreach (DescriptorBase descriptor in descriptors)
                    descriptor.LogMessage();

                Logger.DecrementProtocolIndent();
            }
        }

        /// <summary>
        /// Check if service exists.
        /// </summary>
        /// <param name="serviceId">The service to search for.</param>
        /// <returns>True if the service is present; false otherwise.</returns>
        public bool CheckForService(int serviceId)
        {
            Collection<ServiceListEntry> serviceList = ServiceList;
            if (serviceList == null)
                return false;

            foreach (ServiceListEntry serviceListEntry in serviceList)
            {
                if (serviceListEntry.ServiceID == serviceId)
                    return true;
            }

            return false;
        }
    }
}
