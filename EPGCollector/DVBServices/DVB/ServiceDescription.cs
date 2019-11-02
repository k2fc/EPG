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
    /// The class that describes the service description.
    /// </summary>
    public class ServiceDescription
    {
        /// <summary>
        /// Get the service identification (SID).
        /// </summary>
        public int ServiceID { get { return (serviceID); } }
        /// <summary>
        /// Returns true if the service provides EPG schedule information; false otherwise.
        /// </summary>
        public bool EITSchedule { get { return (eitSchedule); } }
        /// <summary>
        /// Returns true if the service provides next/following EPG information; false otherwise.
        /// </summary>
        public bool EITPresentFollowing { get { return (eitPresentFollowing); } }
        /// <summary>
        /// Get the running status of the service.
        /// </summary>
        public int RunningStatus { get { return (runningStatus); } }
        /// <summary>
        /// Return true if the service is encrypted; false otherwise.
        /// </summary>
        public bool Scrambled { get { return (scrambled); } }

        /// <summary>
        /// Get the service type.
        /// </summary>
        public int ServiceType 
        { 
            get 
            {
                if (serviceDescriptor == null)
                    return (0);
                else
                    return (serviceDescriptor.ServiceType); 
            } 
        }

        /// <summary>
        /// Get the provider name.
        /// </summary>
        public string ProviderName 
        { 
            get 
            {
                if (serviceDescriptor == null)
                    return (string.Empty);
                else
                    return (serviceDescriptor.ProviderName); 
            } 
        }

        /// <summary>
        /// Get the service name.
        /// </summary>
        public string ServiceName 
        {
            get
            {
                if (serviceDescriptor == null || serviceDescriptor.ServiceName == null)
                    return ("No Service Name");
                else
                    return (serviceDescriptor.ServiceName);
            } 
        }

        /// <summary>
        /// Get the channel number.
        /// </summary>
        public int ChannelNumber
        {
            get
            {
                if (serviceChannelDescriptor == null)
                    return (-1);
                else
                    return (serviceChannelDescriptor.ChannelNumber);
            }
        }

        /// <summary>
        /// Get the cable logical channel group number.
        /// </summary>
        public int CableChannelNumber
        {
            get
            {
                if (cableChannelInfoDescriptor == null)
                    return -1;
                else
                    return (cableChannelInfoDescriptor.ChannelNumber);
            }
        }

        /// <summary>
        /// Return true if the service is a digital television service; false otherwise.
        /// </summary>
        public bool IsDigitalTelevision 
        { 
            get 
            {
                if (serviceDescriptor == null)
                    return (false);
                else
                    return (serviceDescriptor.ServiceType == 1 || 
                        serviceDescriptor.ServiceType == 17 ||
                        serviceDescriptor.ServiceType == 22 || 
                        serviceDescriptor.ServiceType == 25); 
            } 
        }

        /// <summary>
        /// Return true if the service is a digital radio service; false otherwise.
        /// </summary>
        public bool IsDigitalRadio 
        { 
            get 
            {
                if (serviceDescriptor == null)
                    return (false);
                else
                    return (serviceDescriptor.ServiceType == 2 || serviceDescriptor.ServiceType == 10); 
            } 
        }

        /// <summary>
        /// Return true if the service is an HD television service; false otherwise.
        /// </summary>
        public bool IsHDTelevision 
        { 
            get 
            {
                if (serviceDescriptor == null)
                    return (false);
                else
                    return (serviceDescriptor.ServiceType == 17 || serviceDescriptor.ServiceType == 25); 
            } 
        }

        /// <summary>
        /// Return the optional EPG link.
        /// </summary>
        public EPGLink EPGLink 
        {
            get
            {
                if (dishNetworkEPGLinkDescriptor == null)
                    return (null);
                else
                    return (new EPGLink(dishNetworkEPGLinkDescriptor.OriginalNetworkID,
                        dishNetworkEPGLinkDescriptor.TransportStreamID,
                        dishNetworkEPGLinkDescriptor.ServiceID,
                        dishNetworkEPGLinkDescriptor.TimeOffset));
            }            
        }

        /// <summary>
        /// Return true if a linkage descriptor is present for EPG; false otherwise.
        /// </summary>
        public bool HasEpgLinkage { get { return (linkageDescriptor != null && linkageDescriptor.IsEpgLinkage); } }

        /// <summary>
        /// Get the epg linkage original network ID.
        /// </summary>
        public int EpgLinkageOnid
        {
            get
            {
                if (HasEpgLinkage)
                    return (linkageDescriptor.OriginalNetworkId);
                else
                    return (-1);
            }
        }

        /// <summary>
        /// Get the epg linkage transport stream ID.
        /// </summary>
        public int EpgLinkageTsid
        {
            get
            {
                if (HasEpgLinkage)
                    return (linkageDescriptor.TransportStreamId);
                else
                    return (-1);
            }
        }

        /// <summary>
        /// Get the epg linkage service ID.
        /// </summary>
        public int EpgLinkageSid
        {
            get
            {
                if (HasEpgLinkage)
                    return (linkageDescriptor.ServiceId);
                else
                    return (-1);
            }
        }

        /// <summary>
        /// Get the OpenTV channel group number.
        /// </summary>
        public int OpenTVChannelGroup
        {
            get
            {
                if (openTVChannelGroupDescriptor == null)
                    return -1;
                else
                    return (openTVChannelGroupDescriptor.Group);
            }
        }

        /// <summary>
        /// Get the index of the next byte in the MPEG2 section following the service description.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The service description has not been processed.
        /// </exception> 
        public int Index
        {
            get
            {
                if (lastIndex == -1)
                    throw (new InvalidOperationException("ServiceDescription: Index requested before block processed"));
                return (lastIndex);
            }
        }

        private int serviceID;
        private bool eitSchedule;
        private bool eitPresentFollowing;
        private int runningStatus;
        private bool scrambled;

        private DVBServiceDescriptor serviceDescriptor;
        private ServiceChannelDescriptor serviceChannelDescriptor;
        private DishNetworkEPGLinkDescriptor dishNetworkEPGLinkDescriptor;
        private DVBLinkageDescriptor linkageDescriptor;
        private OpenTVChannelGroupDescriptor openTVChannelGroupDescriptor;
        private DVBCableChannelInfoDescriptor cableChannelInfoDescriptor;
        private Collection<DescriptorBase> otherDescriptors;

        private int lastIndex = -1;

        /// <summary>
        /// Initialize a new instance of the ServiceDescription class.
        /// </summary>
        public ServiceDescription() { }

        /// <summary>
        /// Parse the service description.
        /// </summary>
        /// <param name="byteData">The MPEG2 section containing the service description.</param>
        /// <param name="index">Index of the first byte of the service description in the MPEG2 section.</param>
        internal void Process(byte[] byteData, int index)
        {
            lastIndex = index;

            try
            {
                serviceID = Utils.Convert2BytesToInt(byteData, lastIndex);
                lastIndex += 2;

                eitSchedule = ((int)byteData[lastIndex] & 0x02) != 0;
                eitPresentFollowing = ((int)byteData[lastIndex] & 0x01) != 0;
                lastIndex++;

                runningStatus = (int)(byteData[lastIndex] >> 5);
                scrambled = ((int)byteData[lastIndex] & 0x10) >> 4 == 1;

                int descriptorLoopLength = ((byteData[lastIndex] & 0x0f) * 256) + (int)byteData[lastIndex + 1];
                lastIndex += 2;

                while (descriptorLoopLength != 0)
                {
                    DescriptorBase descriptor = DescriptorBase.Instance(byteData, lastIndex, Scope.SDT);

                    if (!descriptor.IsEmpty)
                    {
                        if (descriptor as DVBServiceDescriptor != null)
                            serviceDescriptor = descriptor as DVBServiceDescriptor;
                        else
                        {
                            if (descriptor as ServiceChannelDescriptor != null)
                                serviceChannelDescriptor = descriptor as ServiceChannelDescriptor;
                            else
                            {
                                if (descriptor as DishNetworkEPGLinkDescriptor != null)
                                    dishNetworkEPGLinkDescriptor = descriptor as DishNetworkEPGLinkDescriptor;
                                else
                                {
                                    if (descriptor as DVBLinkageDescriptor != null)
                                        linkageDescriptor = descriptor as DVBLinkageDescriptor;
                                    else
                                    {
                                        if (descriptor as OpenTVChannelGroupDescriptor != null)
                                            openTVChannelGroupDescriptor = descriptor as OpenTVChannelGroupDescriptor;
                                        else
                                        {
                                            if (descriptor as DVBCableChannelInfoDescriptor != null)
                                                cableChannelInfoDescriptor = descriptor as DVBCableChannelInfoDescriptor;
                                            else
                                            {
                                                if (otherDescriptors == null)
                                                    otherDescriptors = new Collection<DescriptorBase>();
                                                otherDescriptors.Add(descriptor);
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        lastIndex = descriptor.Index;
                        descriptorLoopLength -= descriptor.TotalLength;
                    }
                    else
                    {
                        lastIndex += DescriptorBase.MinimumDescriptorLength;
                        descriptorLoopLength -= DescriptorBase.MinimumDescriptorLength;
                    }
                }

                Validate();
            }
            catch (IndexOutOfRangeException)
            {
                throw (new ArgumentOutOfRangeException("The DVB Service Description message is short"));
            }
        }

        /// <summary>
        /// Validate the service description fields.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// A service description field is not valid.
        /// </exception>
        public void Validate() { }

        /// <summary>
        /// Log the service description fields.
        /// </summary>
        public void LogMessage() 
        {
            if (Logger.ProtocolLogger == null)
                return;

            Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "SERVICE DESCRIPTION: Service ID: " + serviceID +
                " EIT Sched: " + eitSchedule +
                " EIT Now/next: " + eitPresentFollowing +
                " Running status: " + runningStatus +
                " Scrambled: " + scrambled);

            if (serviceDescriptor != null)
            {
                Logger.IncrementProtocolIndent();
                serviceDescriptor.LogMessage();
                Logger.DecrementProtocolIndent();
            }

            if (serviceChannelDescriptor != null)
            {
                Logger.IncrementProtocolIndent();
                serviceChannelDescriptor.LogMessage();
                Logger.DecrementProtocolIndent();
            }

            if (openTVChannelGroupDescriptor != null)
            {
                Logger.IncrementProtocolIndent();
                openTVChannelGroupDescriptor.LogMessage();
                Logger.DecrementProtocolIndent();
            }

            if (otherDescriptors != null)
            {
                Logger.IncrementProtocolIndent();
                foreach (DescriptorBase unknownDescriptor in otherDescriptors) 
                    unknownDescriptor.LogMessage();
                Logger.DecrementProtocolIndent();
            }
        }
    }
}
