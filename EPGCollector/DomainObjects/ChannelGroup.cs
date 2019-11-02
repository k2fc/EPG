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
    /// The class that describes an OpenTV channel group.
    /// </summary>
    public class ChannelGroup
    {
        /// <summary>
        /// Get the list of channel groups.
        /// </summary>
        public static Collection<ChannelGroup> Groups { get; private set; }
 
        /// <summary>
        /// Get the group number.
        /// </summary>
        public int GroupNumber { get; private set; }
        /// <summary>
        /// Get the list of channels for the group.
        /// </summary>
        public Collection<ChannelGroupEntry> Channels { get; private set; }        

        private ChannelGroup() { }

        /// <summary>
        /// Initialize a new instance of the ChannelGroup class.
        /// </summary>
        /// <param name="groupNumber">The group number.</param>
        public ChannelGroup(int groupNumber)
        {
            GroupNumber = groupNumber;            
        }

        /// <summary>
        /// Add a new channel group entry.
        /// </summary>
        /// <param name="newGroupNumber">The channel group.</param>
        /// <param name="newEntry">The new entry.</param>
        public static void AddEntry(int newGroupNumber, ChannelGroupEntry newEntry)
        {
            if (Groups == null)
                Groups = new Collection<ChannelGroup>();

            ChannelGroup group = findGroup(newGroupNumber);
            group.AddEntry(group, newEntry);
        }

        private static ChannelGroup findGroup(int groupNumber)
        {
            foreach (ChannelGroup oldGroup in Groups)
            {
                if (oldGroup.GroupNumber == groupNumber)                
                    return (oldGroup);

                if (oldGroup.GroupNumber > groupNumber)
                {
                    ChannelGroup newGroup = new ChannelGroup(groupNumber);
                    Groups.Insert(Groups.IndexOf(oldGroup), newGroup);
                    return (newGroup);
                }
            }

            ChannelGroup addGroup = new ChannelGroup(groupNumber);
            Groups.Add(addGroup);
            return (addGroup);
        }

        private void AddEntry(ChannelGroup group, ChannelGroupEntry newEntry)
        {
            if (group.Channels == null)
                group.Channels = new Collection<ChannelGroupEntry>();

            foreach (ChannelGroupEntry oldEntry in group.Channels)
            {
                if (oldEntry.ChannelName == newEntry.ChannelName &&
                    oldEntry.OriginalNetworkId == newEntry.OriginalNetworkId &&
                    oldEntry.TransportStreamId == newEntry.TransportStreamId &&
                    oldEntry.ServiceId == newEntry.ServiceId)
                    return;

                if (oldEntry.ChannelName.CompareTo(newEntry.ChannelName) > 0)
                {
                    group.Channels.Insert(group.Channels.IndexOf(oldEntry), newEntry);
                    return;
                }
            }

            group.Channels.Add(newEntry);
        }

        /// <summary>
        /// Log the channel groups.
        /// </summary>
        public static void LogChannelGroups()
        {
            if (!DebugEntry.IsDefined(DebugName.LogChannelGroups))
                return;

            Logger.Instance.WriteSeparator("Channel Groups");

            if (Groups == null || Groups.Count == 0)
            {
                Logger.Instance.Write("No groups present");
                return;
            }

            foreach (ChannelGroup group in Groups)
            {
                Logger.Instance.Write("Group: " + group.GroupNumber + "(0x" + group.GroupNumber.ToString("x2") + ")");

                foreach (ChannelGroupEntry groupEntry in group.Channels)
                    Logger.Instance.Write("    Channel: " + groupEntry.ChannelName + "(" + 
                        groupEntry.OriginalNetworkId + ":" + groupEntry.TransportStreamId + ":" + groupEntry.ServiceId + ")");
            }
        }
    }
}
