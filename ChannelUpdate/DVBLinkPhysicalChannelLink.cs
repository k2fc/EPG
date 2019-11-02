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
    /// <summary>
    /// The class that describes a DVBLink physical channel.
    /// </summary>
    public class DVBLinkPhysicalChannelLink
    {
        internal int Number { get; set; }
        internal int SubNumber { get; set; }
        internal string Type { get; set; }
        /// <summary>
        /// Get or set the physical channel ID.
        /// </summary>
        public string Id { get; internal set; }        
        internal string Name { get; set; }        
        internal string AltID { get; set; }
        internal int Fta { get; set; }        

        /// <summary>
        /// 4.1 properties
        /// </summary>
        internal string SourceID { get; set; }
        internal string SourceName { get; set; }
        internal string Category { get; set; }
        internal int ChildLock { get; set; }

        /// <summary>
        /// 4.5 properties
        /// </summary>
        internal string ControlID { get; set; }
        internal string Sync { get; set; }

        /// <summary>
        /// Only used to aid in creating configuration EPG map entry
        /// </summary>
        internal string EPGID { get; set; }

        internal DVBLinkElement BaseElement { get; private set; }

        internal DVBLinkPhysicalChannelLink() { }

        internal bool Load(DVBLinkElement channelElement)
        {
            if (channelElement.Elements == null)
                return (false);

            BaseElement = channelElement;

            try
            {
                Number = Int32.Parse(channelElement.GetElementValue("number"));
                SubNumber = Int32.Parse(channelElement.GetElementValue("subnumber"));
                Type = channelElement.GetElementValue("type");
                Id = channelElement.GetElementValue("id");                
                Name = channelElement.GetElementValue("name");                
                AltID = channelElement.GetElementValue("altid");
                Fta = Int32.Parse(channelElement.GetElementValue("fta"));
                
                // 4.1 properties
                SourceID = channelElement.GetElementValue("source_id");
                SourceName = channelElement.GetElementValue("source_name");
                Category = channelElement.GetElementValue("category");

                string childLock = channelElement.GetElementValue("childlock");
                if (childLock != null)
                    ChildLock = Int32.Parse(childLock);

                // 4.5 properties
                ControlID = channelElement.GetElementValue("control_id");
                Sync = channelElement.GetElementValue("sync");

                return (true);
            }
            catch (Exception e)
            {
                Logger.Instance.Write("<E> An exception of type " + e.GetType().Name + " has occurred while parsing the xml for a physical channel link");
                Logger.Instance.Write("<E> " + e.Message);

                return (false);
            }
        }

        internal Collection<string> Change(DVBLinkPhysicalChannel physicalChannel)
        {
            Collection<string> fieldNames = new Collection<string>();

            if (physicalChannel.ChNum != -1 && Number != physicalChannel.ChNum)
            {
                Number = physicalChannel.ChNum;
                BaseElement.SetElementValue("number", Number.ToString());
                fieldNames.Add("number");
            }

            if (physicalChannel.ChSubNum != -1 && SubNumber != physicalChannel.ChSubNum)
            {
                SubNumber = physicalChannel.ChSubNum;
                BaseElement.SetElementValue("subnumber", Number.ToString());
                fieldNames.Add("subnumber");
            }

            if (physicalChannel.Name != Name)
            {
                Name = physicalChannel.Name;
                BaseElement.SetElementValue("name", Name);
                fieldNames.Add("name");
            }

            return (fieldNames);
        }
    }
}
