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
    internal class DVBLinkHeadEnd
    {
        internal Collection<DVBLinkPhysicalChannel> Channels { get; private set; }

        internal DVBLinkSource Source { get; private set; }

        internal DVBLinkBaseNode BaseNode { get; private set; }
        internal DVBLinkElement BaseElement { get; private set; }
        
        internal string HeadEndID { get; private set; }
        internal string ChannelSourceID { get; private set; }
        internal string ChannelSourceName { get; private set; }
        internal string ChannelSourceType { get; private set; }
        internal string LNBTypeID{ get; private set; }
        internal string LNBTypeName { get; private set; }
        internal int LOF1 { get; private set; }
        internal int LOF2 { get; private set; }
        internal int LOFSW { get; private set; }
        internal string DiseqcTypeID { get; private set; }
        internal string DiseqcTypeName { get; private set; }

        internal int DiseqcNumber
        {
            get
            {
                if (DiseqcTypeName == null)
                    return (0);

                switch (DiseqcTypeName)
                {
                    case "None":
                        return (0);
                    case "Simple A":
                        return (1);
                    case "Simple B":
                        return (2);
                    case "Pos A/A":
                        return (3);
                    case "Pos A/B":
                        return (4);
                    case "Pos B/A":
                        return (5);
                    case "Pos B/B":
                        return (6);
                    case "LNB1 Diseqc1.1":
                        return (7);
                    case "LNB2 Diseqc1.1":
                        return (8);
                    case "LNB3 Diseqc1.1":
                        return (9);
                    case "LNB4 Diseqc1.1":
                        return (10);
                    case "LNB5 Diseqc1.1":
                        return (11);
                    case "LNB6 Diseqc1.1":
                        return (12);
                    case "LNB7 Diseqc1.1":
                        return (13);
                    case "LNB8 Diseqc1.1":
                        return (14);
                    case "LNB9 Diseqc1.1":
                        return (15);
                    case "LNB10 Diseqc1.1":
                        return (16);
                    case "LNB11 Diseqc1.1":
                        return (17);
                    case "LNB12 Diseqc1.1":
                        return (18);
                    case "LNB13 Diseqc1.1":
                        return (19);
                    case "LNB14 Diseqc1.1":
                        return (20);
                    case "LNB15 Diseqc1.1":
                        return (21);
                    case "LNB16 Diseqc1.1":
                        return (22);
                    default:
                        return (23);
                }
            }
        }

        internal bool IsCustomDiseqc { get { return (DiseqcNumber == 23); } }

        internal string FullDescription { get { return ("Source " + Source + " headend " + ChannelSourceName + " ID " + HeadEndID); } }

        internal DVBLinkHeadEnd() { }

        internal bool Load(DVBLinkSource source, DVBLinkBaseNode baseNode, DVBLinkElement channelElement)
        {
            if (channelElement.Elements == null)
                return (false);

            Source = source;
            BaseNode = baseNode;

            BaseNode = baseNode;
            
            try
            {
                HeadEndID = channelElement.GetElementValue("HeadendID");
                ChannelSourceID = channelElement.GetElementValue("ChannelSourceID");
                ChannelSourceName = channelElement.GetElementValue("ChannelSourceName");
                ChannelSourceType = channelElement.GetElementValue("ChannelSourceType");
                LNBTypeID = channelElement.GetElementValue("LNBTypeID");
                LNBTypeName = channelElement.GetElementValue("LNBTypeName");

                string lof1 = channelElement.GetElementValue("LOF1");
                if (lof1 != null)
                    LOF1 = Int32.Parse(lof1);

                string lof2 = channelElement.GetElementValue("LOF2");
                if (lof2 != null)
                    LOF2 = Int32.Parse(lof2);

                string lofsw = channelElement.GetElementValue("LOFSW");
                if (lofsw != null)
                    LOFSW = Int32.Parse(lofsw);
                
                DiseqcTypeID = channelElement.GetElementValue("DiseqcTypeID");
                DiseqcTypeName = channelElement.GetElementValue("DiseqcTypeName");                

                return (true);
            }
            catch (Exception e)
            {
                Logger.Instance.Write("<E> An exception of type " + e.GetType().Name + " has occurred while parsing the xml for a head end");
                Logger.Instance.Write("<E> " + e.Message);

                return (false);
            }
        }

        internal bool LoadChannels()
        {
            if (Channels == null)
                Channels = new Collection<DVBLinkPhysicalChannel>();

            DVBLinkElement startElement = DVBLinkBaseNode.FindElement(BaseNode, new string[] { "TVSourceSettings", "Channels" });
            if (startElement == null || startElement.Elements == null)
                return (false);

            foreach (DVBLinkElement headEndElement in startElement.Elements)
            {
                if (headEndElement.Name == "Headend")
                {
                    DVBLinkElement idElement = headEndElement.FindElement("HeadendID");
                    if (idElement != null && idElement.Value == HeadEndID)
                    {
                        DVBLinkElement channelListElement = headEndElement.FindElement("ChannelList");
                        if (channelListElement != null && channelListElement.Elements != null)
                        {
                            foreach (DVBLinkElement channelElement in channelListElement.Elements)
                            {
                                DVBLinkPhysicalChannel newChannel = new DVBLinkPhysicalChannel(Source, this);
                                bool loaded = newChannel.Load(channelListElement, channelElement);
                                if (loaded)
                                    Channels.Add(newChannel);
                            }
                        }

                        return (true);
                    }
                }
            }

            return (false);
        }

        internal DVBLinkPhysicalChannel FindChannel(int frequency, int originalNetworkID, int transportStreamID, int serviceID)
        {
            if (Channels == null)
                return (null);

            int actualFrequency = DVBLinkController.RoundFrequency(frequency);          

            foreach (DVBLinkPhysicalChannel channel in Channels)
            {
                if (channel.Freq == actualFrequency &&
                    channel.Nid == originalNetworkID && channel.Tid == transportStreamID && channel.Sid == serviceID)
                    return (channel);
            }

            return (null);
        }

        /// <summary>
        /// Get the description of this head end.
        /// </summary>
        /// <returns>The head end ID.</returns>
        public override string ToString()
        {
            return (HeadEndID);
        }
    }
}
