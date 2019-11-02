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

namespace ChannelUpdate
{
    internal class DVBLinkEPGMapChannel
    {
        internal static Collection<DVBLinkEPGMapChannel> Channels { get; set; }

        internal int ChannelFrequency { get; set; }        
        internal string EPGChannel { get; set; }

        /// <summary>
        /// v4.1 properties
        /// </summary>
        internal string Source { get; set; }
        internal string Name { get; set; }

        /// <summary>
        /// v4.5 properties
        /// </summary>
        internal string ControlID { get; set; }

        private DVBLinkElement baseElement;

        internal DVBLinkEPGMapChannel() { }

        internal bool Load(DVBLinkElement channelElement)
        {
            if (channelElement.Elements == null)
                return (false);
            
            baseElement = channelElement;

            try
            {
                ChannelFrequency = Int32.Parse(channelElement.GetElementValue("channel_frequency"));
                Source = channelElement.GetElementValue("source");
                EPGChannel = channelElement.GetElementValue("epg_channel");
                Name = channelElement.GetElementValue("name");
                ControlID = channelElement.GetElementValue("control_id");
                
                return (true);
            }
            catch (Exception e)
            {
                Logger.Instance.Write("<E> An exception of type " + e.GetType().Name + " has occurred while parsing the xml for an EPG map channel");
                Logger.Instance.Write("<E> " + e.Message);

                return (false);
            }
        }

        internal static bool LoadChannels(DVBLinkBaseNode baseNode)
        {
            if (Channels == null)
                Channels = new Collection<DVBLinkEPGMapChannel>();

            DVBLinkElement epgMapElement = DVBLinkBaseNode.FindElement(baseNode, new string[] { "channel_info", "epg_map" });
            if (epgMapElement == null || epgMapElement.Elements == null)
                return (false);

            foreach (DVBLinkElement channelElement in epgMapElement.Elements)
            {
                if (channelElement.Name == "channel" && channelElement.Elements != null)
                {
                    DVBLinkEPGMapChannel newChannel = new DVBLinkEPGMapChannel();
                    bool loaded = newChannel.Load(channelElement);
                    if (loaded)
                        Channels.Add(newChannel);
                }
            }

            return (true);
        }

        internal static DVBLinkEPGMapChannel FindChannel(int frequency)
        {
            foreach (DVBLinkEPGMapChannel mapChannel in Channels)
            {
                if (mapChannel.ChannelFrequency == frequency)
                    return (mapChannel);
            }

            return (null);
        }

        internal bool Delete()
        {
            Channels.Remove(this);

            DVBLinkElement epgMapElement = DVBLinkBaseNode.FindElement(DVBLinkLogicalChannel.BaseNode, new string[] { "channel_info", "epg_map" });
            if (epgMapElement != null && epgMapElement.Elements != null)
            {
                epgMapElement.Elements.Remove(baseElement);
                Logger.Instance.Write("Deleted EPG map channel for frequency " + ChannelFrequency);
            }

            return (true);
        }
    }
}
