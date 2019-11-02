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
using System.Text;

using DomainObjects;

namespace ChannelUpdate
{
    /// <summary>
    /// The class that describes a DVBLink logical channel.
    /// </summary>
    public class DVBLinkLogicalChannel
    {
        internal static Collection<DVBLinkLogicalChannel> Channels { get; private set; }

        internal int ChildLock { get; private set; }
        internal string Type { get; private set; }
        internal int Number { get; private set; }
        internal int SubNumber { get; private set; }
        internal string Name { get; private set; }
        internal string Category { get; private set; }
        internal int Frequency { get; private set; }

        /// <summary>
        /// Get the collection of physical channels linked to this logical channels.
        /// </summary>
        public Collection<DVBLinkPhysicalChannelLink> PhysicalChannelLinks { get; private set; }

        internal bool New { get; private set; }
        internal bool Changed { get; private set; }

        internal static int ChannelsAdded { get; private set; }
        internal static int ChannelsChanged { get; private set; }
        internal static int ChannelsDeleted { get; private set; }

        internal static DVBLinkBaseNode BaseNode { get; set; }
        
        private DVBLinkElement baseElement;

        private enum addResult
        {
            addedlogicalChannel,
            addedPhysicalChannel,
            noAddition
        }

        internal DVBLinkLogicalChannel() { }

        internal bool Load(DVBLinkElement channelElement)
        {
            if (channelElement.Elements == null)
                return (false);

            baseElement = channelElement;

            try
            {
                ChildLock = Int32.Parse(channelElement.GetElementValue("childlock"));
                Type = channelElement.GetElementValue("type");
                Number = Int32.Parse(channelElement.GetElementValue("number"));
                SubNumber = Int32.Parse(channelElement.GetElementValue("subnumber"));
                Name = channelElement.GetElementValue("name");
                Category = channelElement.GetElementValue("category");
                Frequency = Int32.Parse(channelElement.GetElementValue("frequency"));

                foreach (DVBLinkElement element in channelElement.Elements)
                {
                    if (element.Name == "physical_channel" && element.Elements != null)
                    {
                        DVBLinkPhysicalChannelLink newLink = new DVBLinkPhysicalChannelLink();
                        bool loaded = newLink.Load(element);
                        if (loaded)
                        {
                            if (PhysicalChannelLinks == null)
                                PhysicalChannelLinks = new Collection<DVBLinkPhysicalChannelLink>();
                            PhysicalChannelLinks.Add(newLink);
                        }
                    }
                }
                
                return (true);
            }
            catch (Exception e)
            {
                Logger.Instance.Write("<E> An exception of type " + e.GetType().Name + " has occurred while parsing the xml for a logical channel");
                Logger.Instance.Write("<E> " + e.Message);

                return (false);
            }
        }

        internal static bool LoadChannels(DVBLinkBaseNode baseNode)
        {
            BaseNode = baseNode;

            if (Channels == null)
                Channels = new Collection<DVBLinkLogicalChannel>();

            DVBLinkElement channelMapElement = DVBLinkBaseNode.FindElement(baseNode, new string[] { "channel_info", "channel_map" });
            if (channelMapElement == null || channelMapElement.Elements == null)
                return (false);

            foreach (DVBLinkElement logicalChannelElement in channelMapElement.Elements)
            {
                if (logicalChannelElement.Name == "logical_channel" && logicalChannelElement.Elements != null)
                {
                    DVBLinkLogicalChannel newChannel = new DVBLinkLogicalChannel();
                    bool loaded = newChannel.Load(logicalChannelElement);
                    if (loaded)
                        Channels.Add(newChannel);
                }    
            }

            return (true);
        }

        /// <summary>
        /// Locate a channel by frequency.
        /// </summary>
        /// <param name="frequency">The frequency of the channel.</param>
        /// <returns></returns>
        public static DVBLinkLogicalChannel FindChannel(int frequency)
        {
            if (Channels == null)
                return (null);

            foreach (DVBLinkLogicalChannel channel in Channels)
            {
                if (channel.Frequency == frequency)
                    return (channel);
            }

            return (null);
        }

        /// <summary>
        /// Locate a channel by physical channel.
        /// </summary>
        /// <param name="physicalChannel">The physical channel.</param>
        /// <returns></returns>
        internal static DVBLinkLogicalChannel FindChannel(DVBLinkPhysicalChannel physicalChannel)
        {
            if (Channels == null)
                return (null);

            foreach (DVBLinkLogicalChannel channel in Channels)
            {
                if (channel.PhysicalChannelLinks != null)
                {
                    foreach (DVBLinkPhysicalChannelLink physicalChannelLink in channel.PhysicalChannelLinks)
                    {
                        if (physicalChannelLink.Id == physicalChannel.FullID)
                            return (channel);
                    }
                }
            }

            return (null);
        }

        internal bool Delete()
        {
            DVBLinkElement channelMapElement = DVBLinkBaseNode.FindElement(BaseNode, new string[] { "channel_info", "channel_map" });
            if (channelMapElement == null || channelMapElement.Elements == null)
                return (false);

            foreach (DVBLinkElement logicalChannelElement in channelMapElement.Elements)
            {
                if (logicalChannelElement == baseElement)
                {
                    channelMapElement.Elements.Remove(logicalChannelElement);
                    Logger.Instance.Write("Deleted logical channel " + Name + " frequency " + Frequency);
                    ChannelsDeleted++;

                    DVBLinkLogicalChannel.Channels.Remove(this);

                    DVBLinkEPGMapChannel mapChannel = DVBLinkEPGMapChannel.FindChannel(Frequency);
                    if (mapChannel != null)
                        mapChannel.Delete();

                    return (true);
                }
            }

            return (false);
        }

        internal bool Delete(DVBLinkPhysicalChannelLink channelLink)
        {
            PhysicalChannelLinks.Remove(channelLink);
            baseElement.Elements.Remove(channelLink.BaseElement);

            if (PhysicalChannelLinks.Count != 0)
            {
                Logger.Instance.Write("Deleted physical channel link " + channelLink.Id + " from logical channel " + Name);
                Changed = true;
                ChannelsChanged++;
            }

            return (true);
        }

        internal bool Change(DVBLinkPhysicalChannel physicalChannel)
        {
            Collection<string> fieldNames = new Collection<string>();

            if (RunParameters.Instance.ChannelUpdateNumber)
            {
                if (physicalChannel.ChNum != -1 && Number != physicalChannel.ChNum)
                {
                    Number = physicalChannel.ChNum;
                    baseElement.SetElementValue("number", Number.ToString());
                    fieldNames.Add("number");
                }

                if (physicalChannel.ChSubNum != -1 && SubNumber != physicalChannel.ChSubNum)
                {
                    SubNumber = physicalChannel.ChSubNum;
                    baseElement.SetElementValue("subnumber", Number.ToString());
                    fieldNames.Add("subnumber");
                }
            }

            if (physicalChannel.Name != Name)
            {
                Name = physicalChannel.Name;
                baseElement.SetElementValue("name", Name);
                fieldNames.Add("name");
            }

            Collection<string> linkFieldsChanged = null;

            DVBLinkPhysicalChannelLink channelLink = FindPhysicalChannelLink(physicalChannel);
            if (channelLink == null)
                Logger.Instance.Write("Physical channel link lost for logical channel " + physicalChannel.FullDescription);
            else
                linkFieldsChanged = channelLink.Change(physicalChannel);

            if (linkFieldsChanged != null)
            {
                foreach (string linkFieldChanged in linkFieldsChanged)
                    fieldNames.Add(linkFieldChanged);
            }

            if (fieldNames.Count == 0)
                return (false);

            StringBuilder changedFields = new StringBuilder();
            foreach (string fieldName in fieldNames)
            {
                if (changedFields.Length != 0)
                    changedFields.Append(", ");
                changedFields.Append(fieldName);
            }

            Logger.Instance.Write("Changed logical channel " + Name);
            Logger.Instance.Write("Fields changed: " + changedFields);

            Changed = true;
            ChannelsChanged++;

            return (true);
        }

        internal DVBLinkPhysicalChannelLink FindPhysicalChannelLink(DVBLinkPhysicalChannel physicalChannel)
        {
            if (PhysicalChannelLinks == null)
                return (null);

            foreach (DVBLinkPhysicalChannelLink physicalChannelLink in PhysicalChannelLinks)
            {
                if (physicalChannelLink.Id == physicalChannel.FullID)
                    return (physicalChannelLink);
            }

            return (null);
        }

        internal static bool Add(DVBLinkPhysicalChannel physicalChannel, DVBLinkConfiguration configuration)
        {
            switch (RunParameters.Instance.ChannelMergeMethod)
            {
                case ChannelMergeMethod.None:
                    bool added = addNoMerge(physicalChannel, configuration);
                    if (added)
                    {
                        ChannelsAdded++;
                        Logger.Instance.Write("Added logical channel " + physicalChannel.Name + " (no merging)");
                    }
                    else
                        Logger.Instance.Write("Failed to add logical channel " + physicalChannel.Name + " (no merging)");
                    return (added);
                case ChannelMergeMethod.Name:
                    addResult addedName = addByName(physicalChannel, configuration);
                    switch (addedName)
                    {
                        case addResult.addedlogicalChannel:
                            ChannelsAdded++;
                            Logger.Instance.Write("Added logical channel " + physicalChannel.Name + " (merging by name)");
                            return (true);
                        case addResult.addedPhysicalChannel:
                            ChannelsChanged++;
                            Logger.Instance.Write("Added to logical channel " + physicalChannel.Name + " (merged by name)");
                            return (true);
                        case addResult.noAddition:
                            added = addNoMerge(physicalChannel, configuration);
                            if (added)
                            {
                                ChannelsAdded++;
                                Logger.Instance.Write("Added logical channel " + physicalChannel.Name + " (no merging)");
                            }
                            else
                                Logger.Instance.Write("Failed to add logical channel " + physicalChannel.Name + " (merging by name)");
                            return (added);
                        default:
                            return (false);
                    }
                case ChannelMergeMethod.Number:
                    addResult addedNumber = addByNumber(physicalChannel, configuration);
                    switch (addedNumber)
                    {
                        case addResult.addedlogicalChannel:
                            ChannelsAdded++;
                            Logger.Instance.Write("Added logical channel " + physicalChannel.Name + " (merging by number)");
                            return (true);
                        case addResult.addedPhysicalChannel:
                            ChannelsChanged++;
                            Logger.Instance.Write("Added to logical channel " + physicalChannel.Name + " (merged by number)");
                            return (true);
                        case addResult.noAddition:
                            added = addNoMerge(physicalChannel, configuration);
                            if (added)
                            {
                                ChannelsAdded++;
                                Logger.Instance.Write("Added logical channel " + physicalChannel.Name + " (no merging)");
                            }
                            else
                                Logger.Instance.Write("Failed to add logical channel " + physicalChannel.Name + " (merging by number)");
                            return (added);
                        default:
                            return (false);
                    }
                case ChannelMergeMethod.NameNumber:
                    addResult addedNameNumber = addByNameNumber(physicalChannel, configuration);
                    switch (addedNameNumber)
                    {
                        case addResult.addedlogicalChannel:
                            ChannelsAdded++;
                            Logger.Instance.Write("Added logical channel " + physicalChannel.Name + " (merging by name and number)");
                            return (true);
                        case addResult.addedPhysicalChannel:
                            ChannelsChanged++;
                            Logger.Instance.Write("Added to logical channel " + physicalChannel.Name + " (merged by name and number)");
                            return (true);
                        case addResult.noAddition:
                            added = addNoMerge(physicalChannel, configuration);
                            if (added)
                            {
                                ChannelsAdded++;
                                Logger.Instance.Write("Added logical channel " + physicalChannel.Name + " (no merging)");
                            }
                            else
                                Logger.Instance.Write("Failed to add logical channel " + physicalChannel.Name + " (merging by name and number)");
                            return (added);
                        default:
                            return (false);
                    }
                default:
                    return(false);
            }
        }

        private static bool addNoMerge(DVBLinkPhysicalChannel physicalChannel, DVBLinkConfiguration configuration)
        {
            DVBLinkLogicalChannel newChannel = new DVBLinkLogicalChannel();
            newChannel.New = true;
            newChannel.ChildLock = RunParameters.Instance.ChannelChildLock ? 1 : 0;

            if (physicalChannel.Type == 1)
                newChannel.Type = "TV";
            else
                newChannel.Type = "Radio";

            newChannel.Frequency = DVBLinkController.MaxFrequency + 10000;
            newChannel.Name = physicalChannel.Name;
            newChannel.Number = physicalChannel.ChNum;
            newChannel.SubNumber = physicalChannel.ChSubNum;

            if (Channels == null)
                Channels = new Collection<DVBLinkLogicalChannel>(); 
            Channels.Add(newChannel);

            newChannel.PhysicalChannelLinks = new Collection<DVBLinkPhysicalChannelLink>();

            DVBLinkPhysicalChannelLink newLink = createPhysicalChannelLink(physicalChannel);
            newChannel.PhysicalChannelLinks.Add(newLink);

            DVBLinkElement channelInfoElement = DVBLinkBaseNode.FindElement(BaseNode, new string[] { "channel_info" });
            if (channelInfoElement == null)
            {
                channelInfoElement = new DVBLinkElement("channel_info");
                BaseNode.Elements.Add(channelInfoElement);
            }

            if (channelInfoElement.Elements == null)
                channelInfoElement.Elements = new Collection<DVBLinkElement>();

            DVBLinkElement channelMapElement = DVBLinkBaseNode.FindElement(BaseNode, new string[] { "channel_info", "channel_map" });
            if (channelMapElement == null)
            {
                channelInfoElement.Elements = new Collection<DVBLinkElement>();
                channelMapElement = new DVBLinkElement("channel_map");
                channelInfoElement.Elements.Add(channelMapElement);
            }
            
            if (channelMapElement.Elements == null)
                channelMapElement.Elements = new Collection<DVBLinkElement>();

            DVBLinkElement logicalChannelElement = new DVBLinkElement("logical_channel");            
            logicalChannelElement.Elements = new Collection<DVBLinkElement>();

            channelMapElement.Elements.Add(logicalChannelElement);
            newChannel.baseElement = logicalChannelElement;

            logicalChannelElement.Elements.Add(new DVBLinkElement("childlock", newChannel.ChildLock.ToString()));
            logicalChannelElement.Elements.Add(new DVBLinkElement("type", newChannel.Type));
            logicalChannelElement.Elements.Add(new DVBLinkElement("number", newChannel.Number.ToString()));
            logicalChannelElement.Elements.Add(new DVBLinkElement("subnumber", newChannel.SubNumber.ToString()));
            logicalChannelElement.Elements.Add(new DVBLinkElement("name", newChannel.Name));
            logicalChannelElement.Elements.Add(new DVBLinkElement("category"));
            logicalChannelElement.Elements.Add(new DVBLinkElement("frequency", newChannel.Frequency.ToString()));

            DVBLinkElement physicalChannelElement = createPhysicalChannelElement(newLink);
            logicalChannelElement.Elements.Add(physicalChannelElement);

            DVBLinkEPGMapChannel mapChannel = new DVBLinkEPGMapChannel();
            mapChannel.ChannelFrequency = newChannel.Frequency;

            switch (RunParameters.Instance.ChannelEPGScanner)
            {
                case ChannelEPGScanner.EPGCollector:
                    mapChannel.EPGChannel = "dvblogiccppplugin" + ":" + physicalChannel.Nid + ":" + physicalChannel.Tid + ":" + physicalChannel.Sid;
                    mapChannel.ControlID = newLink.EPGID;
                    break;
                case ChannelEPGScanner.EITScanner:
                    mapChannel.EPGChannel = "eitscanner"+ ":" + physicalChannel.Freq + ":" + physicalChannel.Nid + ":" + physicalChannel.Tid + ":" + physicalChannel.Sid;
                    mapChannel.ControlID = newLink.EPGID;
                    break;
                case ChannelEPGScanner.None:
                    mapChannel.ControlID = "5f199a2a-4092-4c99-8033-7ea35d816835";
                    break;
                case ChannelEPGScanner.Default:
                    mapChannel.ControlID = "710bcb73-0cf2-4d65-bd92-daab57d1c85c";
                    break;
                case ChannelEPGScanner.Xmltv:
                    DVBLinkSource source = configuration.FindSource("XMLTV-1");
                    if (source != null)
                    {
                        mapChannel.EPGChannel = physicalChannel.Sid.ToString();
                        mapChannel.ControlID = source.EPGID;
                    }
                    break;
                default:
                    break;
            }

            if (DVBLinkSource.SourceVersion == "41")
            {
                mapChannel.Name = newLink.SourceName;
                mapChannel.Source = newLink.SourceID;
            }

            if (DVBLinkEPGMapChannel.Channels == null)
                DVBLinkEPGMapChannel.Channels = new Collection<DVBLinkEPGMapChannel>();
            DVBLinkEPGMapChannel.Channels.Add(mapChannel);

            DVBLinkElement epgMapElement = DVBLinkBaseNode.FindElement(BaseNode, new string[] { "channel_info", "epg_map" });
            if (epgMapElement == null)
            {
                epgMapElement = new DVBLinkElement("epg_map");
                channelInfoElement.Elements.Add(epgMapElement);
            }

            if (epgMapElement.Elements == null)
                epgMapElement.Elements = new Collection<DVBLinkElement>();

            DVBLinkElement epgChannelElement = new DVBLinkElement("channel");
            epgChannelElement.Elements = new Collection<DVBLinkElement>();

            if (DVBLinkSource.SourceVersion == "41")
            {
                epgChannelElement.Elements.Add(new DVBLinkElement("channel_frequency", mapChannel.ChannelFrequency.ToString()));
                epgChannelElement.Elements.Add(new DVBLinkElement("source", mapChannel.Source));
                epgChannelElement.Elements.Add(new DVBLinkElement("epg_channel", mapChannel.EPGChannel));
                epgChannelElement.Elements.Add(new DVBLinkElement("name", mapChannel.Name));
            }
            else
            {
                epgChannelElement.Elements.Add(new DVBLinkElement("channel_frequency", mapChannel.ChannelFrequency.ToString()));
                epgChannelElement.Elements.Add(new DVBLinkElement("control_id", mapChannel.ControlID)); 
                epgChannelElement.Elements.Add(new DVBLinkElement("epg_channel", mapChannel.EPGChannel));                
            }

            epgMapElement.Elements.Add(epgChannelElement);

            DVBLinkController.MaxFrequency = newChannel.Frequency;

            return (true);
        }

        private static addResult addByName(DVBLinkPhysicalChannel physicalChannel, DVBLinkConfiguration configuration)
        {
            foreach (DVBLinkLogicalChannel logicalChannel in DVBLinkLogicalChannel.Channels)
            {
                if (logicalChannel.Name == physicalChannel.Name)
                {
                    DVBLinkPhysicalChannelLink newLink = createPhysicalChannelLink(physicalChannel); 
                    logicalChannel.PhysicalChannelLinks.Add(newLink);

                    logicalChannel.baseElement.Elements.Add(createPhysicalChannelElement(newLink));

                    ChannelsChanged++;

                    return (addResult.addedPhysicalChannel);
                }
            }

            return (addNoMerge(physicalChannel, configuration) ? addResult.addedlogicalChannel : addResult.noAddition);
        }

        private static addResult addByNumber(DVBLinkPhysicalChannel physicalChannel, DVBLinkConfiguration configuration)
        {
            foreach (DVBLinkLogicalChannel logicalChannel in DVBLinkLogicalChannel.Channels)
            {
                if (logicalChannel.Number == physicalChannel.ChNum && logicalChannel.SubNumber == physicalChannel.ChSubNum)
                {
                    DVBLinkPhysicalChannelLink newLink = createPhysicalChannelLink(physicalChannel);
                    logicalChannel.PhysicalChannelLinks.Add(newLink);

                    logicalChannel.baseElement.Elements.Add(createPhysicalChannelElement(newLink));

                    ChannelsChanged++;
                    return (addResult.addedlogicalChannel);
                }
            }

            return (addNoMerge(physicalChannel, configuration) ? addResult.addedlogicalChannel : addResult.noAddition);
        }

        private static addResult addByNameNumber(DVBLinkPhysicalChannel physicalChannel, DVBLinkConfiguration configuration)
        {
            foreach (DVBLinkLogicalChannel logicalChannel in DVBLinkLogicalChannel.Channels)
            {
                if (logicalChannel.Name == physicalChannel.Name && logicalChannel.Number == physicalChannel.ChNum && logicalChannel.SubNumber == physicalChannel.ChSubNum)
                {
                    DVBLinkPhysicalChannelLink newLink = createPhysicalChannelLink(physicalChannel);
                    logicalChannel.PhysicalChannelLinks.Add(newLink);

                    logicalChannel.baseElement.Elements.Add(createPhysicalChannelElement(newLink));

                    ChannelsChanged++;
                    return (addResult.addedlogicalChannel);
                }
            }

            return (addNoMerge(physicalChannel, configuration) ? addResult.addedlogicalChannel : addResult.noAddition);
        }

        private static DVBLinkPhysicalChannelLink createPhysicalChannelLink(DVBLinkPhysicalChannel physicalChannel)
        {
            DVBLinkPhysicalChannelLink newLink = new DVBLinkPhysicalChannelLink();

            newLink.Number = physicalChannel.ChNum;
            newLink.SubNumber = physicalChannel.ChSubNum;

            if (physicalChannel.Type == 1)
                newLink.Type = "TV";
            else
                newLink.Type = "Radio";            

            string id = physicalChannel.HeadEnd.HeadEndID + ":" + physicalChannel.Freq + ":" + physicalChannel.Nid + ":" + physicalChannel.Tid + ":" + physicalChannel.Sid;
            newLink.Id = id;

            newLink.Name = physicalChannel.Name;

            string altID = physicalChannel.HeadEnd.ChannelSourceID + ":" + (physicalChannel.Freq / 1000) + ":" + physicalChannel.Nid + ":" + physicalChannel.Tid + ":" + physicalChannel.Sid;
            newLink.AltID = altID;

            newLink.Fta = physicalChannel.Encrypt == 0 ? 1 : 0;

            if (DVBLinkSource.SourceVersion == "41")
            {
                newLink.SourceID = physicalChannel.Source.LinkID;
                newLink.SourceName = physicalChannel.Source.NormalizedName;
                newLink.Category = @"\" + physicalChannel.HeadEnd.ChannelSourceName + @"\" + newLink.Type + @"\ByName";
                newLink.ChildLock = RunParameters.Instance.ChannelChildLock ? 1 : 0;                
            }
            else
            {
                newLink.ControlID = physicalChannel.Source.LinkID;
                newLink.Sync = "0";
            }

            newLink.EPGID = physicalChannel.Source.EPGID;

            return (newLink);
        }

        private static DVBLinkElement createPhysicalChannelElement(DVBLinkPhysicalChannelLink newLink)
        {
            DVBLinkElement physicalChannelElement = new DVBLinkElement("physical_channel");
            physicalChannelElement.Elements = new Collection<DVBLinkElement>();

            physicalChannelElement.Elements.Add(new DVBLinkElement("number", newLink.Number.ToString()));
            physicalChannelElement.Elements.Add(new DVBLinkElement("subnumber", newLink.SubNumber.ToString()));
            physicalChannelElement.Elements.Add(new DVBLinkElement("type", newLink.Type));
            physicalChannelElement.Elements.Add(new DVBLinkElement("id", newLink.Id));

            if (DVBLinkSource.SourceVersion == "41")
            {
                physicalChannelElement.Elements.Add(new DVBLinkElement("source_id", newLink.SourceID));
                physicalChannelElement.Elements.Add(new DVBLinkElement("source_name", newLink.SourceName));
                physicalChannelElement.Elements.Add(new DVBLinkElement("category", newLink.Category));
                physicalChannelElement.Elements.Add(new DVBLinkElement("name", newLink.Name));
                physicalChannelElement.Elements.Add(new DVBLinkElement("altid", newLink.AltID));
                physicalChannelElement.Elements.Add(new DVBLinkElement("fta", newLink.Fta.ToString()));
                physicalChannelElement.Elements.Add(new DVBLinkElement("childlock", newLink.ChildLock.ToString()));
            }
            else
            {
                physicalChannelElement.Elements.Add(new DVBLinkElement("control_id", newLink.ControlID));
                physicalChannelElement.Elements.Add(new DVBLinkElement("name", newLink.Name));
                physicalChannelElement.Elements.Add(new DVBLinkElement("altid", newLink.AltID));
                physicalChannelElement.Elements.Add(new DVBLinkElement("fta", newLink.Fta.ToString()));
                physicalChannelElement.Elements.Add(new DVBLinkElement("sync", newLink.Sync));
            }

            return (physicalChannelElement);
        }

        internal static Collection<DVBLinkLogicalChannel> GetChannelsNumberOrder()
        {
            if (Channels == null)
                return (null);

            Collection<DVBLinkLogicalChannel> channels = new Collection<DVBLinkLogicalChannel>();
            
            foreach (DVBLinkLogicalChannel newChannel in Channels)
                addChannelByNumber(channels, newChannel);

            return (channels);
        }

        private static void addChannelByNumber(Collection<DVBLinkLogicalChannel> channels, DVBLinkLogicalChannel newChannel)
        {
            foreach (DVBLinkLogicalChannel oldChannel in channels)
            {
                if (oldChannel.Number > newChannel.Number)
                {
                    channels.Insert(channels.IndexOf(oldChannel), newChannel);
                    return;
                }

                if (oldChannel.Number == newChannel.Number)
                {
                    if (oldChannel.SubNumber >= newChannel.SubNumber)
                    {
                        channels.Insert(channels.IndexOf(oldChannel), newChannel);
                        return;
                    }
                }
            }

            channels.Add(newChannel);
        }
    }
}
