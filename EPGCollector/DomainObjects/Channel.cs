﻿////////////////////////////////////////////////////////////////////////////////// 
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
using System.Text;

namespace DomainObjects
{
    /// <summary>
    /// The class that describes a program channel.
    /// </summary>
    public class Channel
    {
        /// <summary>
        /// Get the collection of channels.
        /// </summary>
        public static Collection<Channel> Channels
        {
            get
            {
                if (channels == null)
                    channels = new Collection<Channel>();
                return (channels);
            }
        }

        /// <summary>
        /// Get or set the ONID.
        /// </summary>
        public int OriginalNetworkID
        {
            get { return (originalNetworkID); }
            set { originalNetworkID = value; }
        }

        /// <summary>
        /// Get or set the TSID.
        /// </summary>
        public int TransportStreamID
        {
            get { return (transportStreamID); }
            set { transportStreamID = value; }
        }

        /// <summary>
        /// Get or set the SID.
        /// </summary>
        public int ServiceID
        {
            get { return (serviceID); }
            set { serviceID = value; }
        }

        /// <summary>
        /// Get or set the channel ID.
        /// </summary>
        public int ChannelID
        {
            get { return (channelID); }
            set { channelID = value; }
        }

        /// <summary>
        /// Get or set the user channel number.
        /// </summary>
        public int UserChannel
        {
            get { return (userChannel); }
            set { userChannel = value; }
        }

        /// <summary>
        /// Get or set the bouquet ID of the bouquet containing the channel.
        /// </summary>
        public int BouquetID
        {
            get { return (bouquetID); }
            set { bouquetID = value; }
        }

        /// <summary>
        /// Get or set the region containing the channel.
        /// </summary>
        public int Region
        {
            get { return (region); }
            set { region = value; }
        }

        /// <summary>
        /// Get or set the channel flag bytes.
        /// </summary>
        public byte[] Flags
        {
            get { return (flags); }
            set { flags = value; }
        }

        private byte[] flags;

        private int originalNetworkID;
        private int transportStreamID;
        private int serviceID;
        private int channelID;
        private int userChannel;

        private int bouquetID;
        private int region;

        private static Collection<Channel> channels;

        /// <summary>
        /// Initialize a new instance of the Channel class.
        /// </summary>
        public Channel() { }

        /// <summary>
        /// Add a channel to the collection.
        /// </summary>
        /// <param name="newChannel">The channel to be added.</param>
        public static void AddChannel(Channel newChannel)
        {
            if (TraceEntry.IsDefined(TraceName.AddChannel))
            {
                Logger.Instance.Write("Adding channel: ONID " + newChannel.OriginalNetworkID +
                    " TSID " + newChannel.TransportStreamID +
                    " SID " + newChannel.ServiceID +
                    " Channel ID: " + newChannel.ChannelID +
                    " User Channel: " + newChannel.UserChannel +
                    " Bqt ID: " + newChannel.BouquetID +
                    " Region: " + newChannel.Region);
            }

            foreach (Channel oldChannel in Channels)
            {
                if (oldChannel.OriginalNetworkID == newChannel.OriginalNetworkID &&
                    oldChannel.TransportStreamID == newChannel.TransportStreamID &&
                    oldChannel.ServiceID == newChannel.ServiceID &&
                    oldChannel.ChannelID == newChannel.ChannelID)
                {
                    if (TraceEntry.IsDefined(TraceName.AddChannel))
                        Logger.Instance.Write("Already exists");
                    return;
                }

                if (oldChannel.OriginalNetworkID == newChannel.OriginalNetworkID)
                {
                    if (oldChannel.TransportStreamID == newChannel.TransportStreamID)
                    {
                        if (oldChannel.ServiceID == newChannel.ServiceID)
                        {
                            if (oldChannel.ChannelID > newChannel.ChannelID)
                            {
                                Channels.Insert(Channels.IndexOf(oldChannel), newChannel);
                                return;
                            }
                        }
                        else
                        {
                            if (oldChannel.ServiceID > newChannel.ServiceID)
                            {
                                Channels.Insert(Channels.IndexOf(oldChannel), newChannel);
                                return;
                            }
                        }
                    }
                    else
                    {
                        if (oldChannel.TransportStreamID > newChannel.TransportStreamID)
                        {
                            Channels.Insert(Channels.IndexOf(oldChannel), newChannel);
                            return;
                        }
                    }
                }
                else
                {
                    if (oldChannel.OriginalNetworkID > newChannel.OriginalNetworkID)
                    {
                        Channels.Insert(Channels.IndexOf(oldChannel), newChannel);
                        return;
                    }
                }
            }

            Channels.Add(newChannel);
        }

        /// <summary>
        /// Map the channel to a TV station.
        /// </summary>
        /// <param name="station">The TV station to be mapped.</param>
        public void CreateChannelMapping(TVStation station)
        {
            if (station.LogicalChannelNumber != -1)
                return;
            
            station.LogicalChannelNumber = UserChannel;
        }

        /// <summary>
        /// Log all the channel mappings.
        /// </summary>
        /// <param name="logger">The logger instance to be used.</param>
        public void LogChannelMapping(Logger logger)
        {
            TVStation station = TVStation.FindStation(RunParameters.Instance.StationCollection, 
                OriginalNetworkID, TransportStreamID, ServiceID);

            string stationName;
            if (station != null)
                stationName = station.Name;
            else
                stationName = "** No Station **";

            StringBuilder flagString = new StringBuilder();
            if (Flags != null)
            {
                flagString.Append("0x");

                foreach (byte flagByte in Flags)
                    flagString.Append(flagByte.ToString("X").PadLeft(2, '0'));
            }
            else
                flagString.Append("N/A");

            logger.Write("Channel: ONID " + OriginalNetworkID +
                " TSID " + TransportStreamID +
                " SID " + ServiceID +
                " Channel ID: " + ChannelID +
                " User Channel: " + UserChannel +
                " Bouquet: " +  BouquetID +
                " Region: " + Region +
                " Flags: " +  flagString +
                /*" Flags Decoded: " + ((Flags[0] * 256 + Flags[1]) >> 4).ToString() + " 0x" + (Flags[1] & 0x0f).ToString("x") +*/
                " Name: " + stationName);
        }

        /// <summary>
        /// Find a channel.
        /// </summary>
        /// <param name="channelID">The channel ID.</param>
        /// <returns>A channel instance or null if the channel cannot be located.</returns>
        public static Channel FindChannel(int channelID)
        {
            foreach (Channel channel in Channel.Channels)
            {
                if (channel.ChannelID == channelID)
                    return (channel);
            }

            return (null);
        }

        /// <summary>
        /// Find a channel.
        /// </summary>
        /// <param name="originalNetworkID">The original network ID.</param>
        /// <param name="transportStreamID">The transport stream ID.</param>
        /// <param name="serviceID">The service ID.</param>
        /// <returns>A channel instance or null if the channel cannot be located.</returns>
        public static Channel FindChannel(int originalNetworkID, int transportStreamID, int serviceID)
        {
            foreach (Channel channel in Channel.Channels)
            {
                if (channel.OriginalNetworkID == originalNetworkID && channel.TransportStreamID == transportStreamID && channel.ServiceID == serviceID)
                    return (channel);
            }

            return (null);
        }

        /// <summary>
        /// Get a description of this instance.
        /// </summary>
        /// <returns>A string describing this instance.</returns>
        public override string ToString()
        {
            TVStation station = TVStation.FindStation(RunParameters.Instance.StationCollection, 
                originalNetworkID, transportStreamID, serviceID);

            string stationName;
            if (station != null)
                stationName = station.Name;
            else
                stationName = "** No Station **";

            return ("ONID " + OriginalNetworkID +
                " TSID " + TransportStreamID +
                " SID " + ServiceID +
                " Channel ID: " + ChannelID +
                " User Channel: " + UserChannel +
                " Bouquet: " + BouquetID +
                " Region: " + Region +
                " Station: " + stationName);
        }

        /// <summary>
        /// Get the channels in user channel number order.
        /// </summary>
        /// <returns>The channels sorted in channel number order.</returns>
        public static Collection<Channel> GetChannelsInUserNumberOrder()
        {
            Collection<Channel> sortedChannels = new Collection<Channel>();

            foreach (Channel channel in Channels)
                addInUserNumberOrder(channel, sortedChannels);

            return (sortedChannels);
        }

        private static void addInUserNumberOrder(Channel newChannel, Collection<Channel> sortedChannels)
        {
            foreach (Channel oldChannel in sortedChannels)
            {
                if (oldChannel.UserChannel >= newChannel.UserChannel)
                {
                    sortedChannels.Insert(sortedChannels.IndexOf(oldChannel), newChannel);
                    return;
                }
            }

            sortedChannels.Add(newChannel);
        }

        /// <summary>
        /// Log all the channels stored.
        /// </summary>
        public static void LogChannels()
        {
            Logger.Instance.WriteSeparator("Channel List");

            foreach (Channel channel in Channels)
                Logger.Instance.Write(channel.ToString());
        }

        /// <summary>
        /// Log all the channels stored in channel number order.
        /// </summary>
        public static void LogChannelsInChannelIDOrder()
        {
            Collection<Channel> sortedChannels = new Collection<Channel>();

            foreach (Channel channel in Channels)
                addInChannelIDOrder(channel, sortedChannels);

            Logger.Instance.WriteSeparator("Channel List");

            foreach (Channel channel in sortedChannels)
                Logger.Instance.Write(channel.ToString());
        }

        private static void addInChannelIDOrder(Channel newChannel, Collection<Channel> sortedChannels)
        {
            foreach (Channel oldChannel in sortedChannels)
            {
                if (oldChannel.ChannelID >= newChannel.ChannelID)
                {
                    sortedChannels.Insert(sortedChannels.IndexOf(oldChannel), newChannel);
                    return;
                }
            }

            sortedChannels.Add(newChannel);
        }
    }
}
