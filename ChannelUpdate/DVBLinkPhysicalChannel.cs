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
    internal class DVBLinkPhysicalChannel
    {
        internal DVBLinkSource Source { get; private set; }        
        internal DVBLinkHeadEnd HeadEnd { get; private set; }
        
        internal int Diseqc { get; private set; }
        internal string DiseqcRawData { get; private set; }
        internal string Fec { get; private set; }
        internal int Freq { get; private set; }
        internal string LNBSel { get; private set; }
        internal int LOF { get; private set; }
        internal int Mod { get; private set; }
        internal int SR { get; private set; }
        internal int Pol { get; private set; }
        internal int Nid { get; private set; }
        internal int Tid { get; private set; }
        internal int Sid { get; private set; }
        internal int Encrypt { get; private set; }
        internal int Type { get; private set; }
        internal int ChNum { get; private set; }
        internal int ChSubNum { get; private set; }
        internal int Ecm_Pid { get; private set; }
        internal int LOF1 { get; private set; }
        internal int LOF2 { get; private set; }
        internal int LOFSW { get; private set; }
        internal Collection<DVBLinkCAEntry> CA { get; private set; }
        internal string Name { get; private set; }
        internal string Provider { get; private set; }
        internal string Id { get; private set; }

        internal bool New { get; private set; }
        internal bool Changed { get; private set; }

        internal string FullDescription { get { return (Name + " (" + Nid + "," + Tid + "," + Sid + ")"); } }
        internal string FullID { get { return (HeadEnd + ":" + Freq + ":" + Nid + ":" + Tid + ":" + Sid); } }

        internal static int ChannelsAdded { get; private set; }
        internal static int ChannelsChanged { get; private set; }
        internal static int ChannelsDeleted { get; private set; }

        private DVBLinkElement channelListElement;
        private DVBLinkElement channelElement;

        private DVBLinkPhysicalChannel() { }

        internal DVBLinkPhysicalChannel(DVBLinkSource source, DVBLinkHeadEnd headEnd)
        {
            Source = source;
            HeadEnd = headEnd;
        }

        internal bool Load(DVBLinkElement channelListElement, DVBLinkElement channelElement)
        {
            if (channelElement.Elements == null)
                return (false);

            this.channelListElement = channelListElement;
            this.channelElement = channelElement;

            try
            {
                Diseqc = Int32.Parse(channelElement.GetElementValue("Diseqc"));
                DiseqcRawData = channelElement.GetElementValue("DiseqcRawData");
                Fec = channelElement.GetElementValue("Fec");
                Freq = Int32.Parse(channelElement.GetElementValue("Freq"));
                LNBSel = channelElement.GetElementValue("LNBSel");
                LOF = Int32.Parse(channelElement.GetElementValue("LOF"));
                Mod = Int32.Parse(channelElement.GetElementValue("Mod"));
                SR = Int32.Parse(channelElement.GetElementValue("SR"));
                Pol = Int32.Parse(channelElement.GetElementValue("Pol"));
                Nid = Int32.Parse(channelElement.GetElementValue("nid"));
                Tid = Int32.Parse(channelElement.GetElementValue("tid"));
                Sid = Int32.Parse(channelElement.GetElementValue("sid"));
                Encrypt = Int32.Parse(channelElement.GetElementValue("Encrypt"));
                Type = Int32.Parse(channelElement.GetElementValue("Type"));
                ChNum = Int32.Parse(channelElement.GetElementValue("ChNum"));
                ChSubNum = Int32.Parse(channelElement.GetElementValue("ChSubNum"));
                Ecm_Pid = Int32.Parse(channelElement.GetElementValue("ecm_pid"));
                LOF1 = Int32.Parse(channelElement.GetElementValue("LOF1"));
                LOF2 = Int32.Parse(channelElement.GetElementValue("LOF2"));
                LOFSW = Int32.Parse(channelElement.GetElementValue("LOFSW"));

                DVBLinkElement caElement = channelElement.FindElement("CA");
                if (caElement != null && caElement.Elements != null)
                {
                    CA = new Collection<DVBLinkCAEntry>();

                    foreach (DVBLinkElement descriptorElement in caElement.Elements)
                    {
                        if (descriptorElement.Name == "descriptor")
                        {
                            DVBLinkAttribute pidAttribute = descriptorElement.FindAttribute("pid"); 
                            DVBLinkAttribute sysIDAttribute = descriptorElement.FindAttribute("SysId");
                             
                            if (pidAttribute != null && sysIDAttribute != null) 
                                CA.Add(new DVBLinkCAEntry(Int32.Parse(pidAttribute.Value), Int32.Parse(sysIDAttribute.Value)));
                        }
                    }
                }

                Name = channelElement.GetElementValue("Name");
                Provider = channelElement.GetElementValue("Provider");
                Id = channelElement.GetElementValue("id");

                return (true);
            }
            catch (Exception e)
            {
                Logger.Instance.Write("<E> An exception of type " + e.GetType().Name + " has occurred while parsing the xml for a physical channel");
                Logger.Instance.Write("<E> " + e.Message);

                return (false);
            }
        }

        internal bool Change(DVBLinkHeadEnd headEnd, TuningFrequency tuningFrequency, TVStation station)
        {
            SatelliteFrequency satelliteFrequency = tuningFrequency as SatelliteFrequency;
            if (satelliteFrequency != null)
                return changeSatelliteFrequency(satelliteFrequency, headEnd, station);

            TerrestrialFrequency terrestrialFrequency = tuningFrequency as TerrestrialFrequency;
            if (terrestrialFrequency != null)
                return changeTerrestrialFrequency(terrestrialFrequency, headEnd, station);

            CableFrequency cableFrequency = tuningFrequency as CableFrequency;
            if (cableFrequency != null)
                return changeCableFrequency(cableFrequency, headEnd, station);

            return false;
        }

        internal bool changeSatelliteFrequency(SatelliteFrequency frequency, DVBLinkHeadEnd headEnd, TVStation station)
        {
            bool logicalChangeNeeded = false;

            Collection<string> fieldNames = new Collection<string>();

            if (Diseqc != headEnd.DiseqcNumber)
            {
                fieldNames.Add("Diseqc: was " + Diseqc + " now " + headEnd.DiseqcNumber);
                Diseqc = headEnd.DiseqcNumber;
                channelElement.SetElementValue("Diseqc", Diseqc.ToString());                
            }

            string rawData;
            
            if (headEnd.IsCustomDiseqc)
                rawData = headEnd.DiseqcTypeID;
            else
                rawData = null;

            if (DiseqcRawData != rawData)
            {
                fieldNames.Add("DiseqcRawData: was " + DiseqcRawData + " now " + rawData);
                DiseqcRawData = rawData;
                channelElement.SetElementValue("DiseqcRawData", DiseqcRawData);                
            }

            string newFec = getFec(frequency.FEC.Rate);

            if (Fec != newFec)
            {
                fieldNames.Add("Fec: was " + Fec + " now " + newFec);
                Fec = newFec;
                channelElement.SetElementValue("Fec", Fec);
            }

            if (LOF != headEnd.LOF1)
            {
                fieldNames.Add("LOF: was " + LOF + " now " + headEnd.LOF1);
                LOF = headEnd.LOF1;
                channelElement.SetElementValue("LOF", LOF.ToString());                
            }

            int newMod = getModulation(frequency.DVBModulation, frequency.IsS2, frequency.IsDishNetwork);
            if (Mod != newMod)
            {
                fieldNames.Add("Mod: was " + Mod + " now " + newMod);
                Mod = newMod;
                channelElement.SetElementValue("Mod", Mod.ToString());
            }

            int newSR = frequency.SymbolRate;

            if (SR != newSR)
            {
                fieldNames.Add("SR: was " + SR + " now " + newSR);
                SR = newSR;
                channelElement.SetElementValue("SR", SR.ToString());                
            }

            int newPol = frequency.DVBPolarization;

            if (Pol != newPol)
            {
                fieldNames.Add("Pol: was " + Pol + " now " + newPol);
                Pol = newPol;
                channelElement.SetElementValue("Pol", Pol.ToString());                
            }

            int newEncrypt;

            if (!station.Encrypted)
                newEncrypt = 0;
            else
                newEncrypt = 1;

            if (Encrypt != newEncrypt)
            {
                fieldNames.Add("Encrypt: was " + Encrypt + " now " + newEncrypt);
                Encrypt = newEncrypt;
                channelElement.SetElementValue("Encrypt", Encrypt.ToString());                
            }

            int newType;

            if (!station.IsRadio)
                newType = 1;
            else
                newType = 2;

            if (Type != newType)
            {
                fieldNames.Add("Type: was " + Type + " now " + newType);
                Type = newType;
                channelElement.SetElementValue("Type", Type.ToString());                
            }

            if (RunParameters.Instance.ChannelUpdateNumber)
            {
                if (station.LogicalChannelNumber == -1)
                {
                    if (ChNum != station.OriginalChannelNumber)
                    {
                        fieldNames.Add("ChNum: was " + ChNum + " now " + station.OriginalChannelNumber);
                        ChNum = station.OriginalChannelNumber;
                        channelElement.SetElementValue("ChNum", ChNum.ToString());
                        logicalChangeNeeded = true;
                    }
                }
                else
                {
                    if (ChNum != station.LogicalChannelNumber)
                    {
                        fieldNames.Add("ChNum: was " + ChNum + " now " + station.LogicalChannelNumber);
                        ChNum = station.LogicalChannelNumber;
                        channelElement.SetElementValue("ChNum", ChNum.ToString());
                        logicalChangeNeeded = true;
                    }
                }

                if (ChSubNum != station.MinorChannelNumber)
                {
                    if (ChSubNum != 0)
                    {
                        int oldChSubNum = ChSubNum;

                        if (station.MinorChannelNumber != -1)
                        {
                            ChSubNum = station.MinorChannelNumber;
                            channelElement.SetElementValue("ChSubNum", station.MinorChannelNumber.ToString());
                        }
                        else
                        {
                            ChSubNum = 0;
                            channelElement.SetElementValue("ChSubNum", "0");
                        }

                        fieldNames.Add("ChSubNum: was " + oldChSubNum + " now " + ChSubNum);
                        logicalChangeNeeded = true;
                    }
                    else
                    {
                        if (station.MinorChannelNumber != -1)
                        {
                            fieldNames.Add("ChSubNum: was " + ChSubNum + " now " + station.MinorChannelNumber);
                            ChSubNum = station.MinorChannelNumber;
                            channelElement.SetElementValue("ChSubNum", station.MinorChannelNumber.ToString());
                            logicalChangeNeeded = true;
                        }
                    }

                }
            }

            if (LOF1 != headEnd.LOF1)
            {
                fieldNames.Add("LOF1: was " + LOF1 + " now " + headEnd.LOF1);
                LOF1 = headEnd.LOF1;
                channelElement.SetElementValue("LOF1", LOF1.ToString());                
            }

            if (LOF2 != headEnd.LOF2)
            {
                fieldNames.Add("LOF2: was " + LOF2 + " now " + headEnd.LOF2);
                LOF2 = headEnd.LOF2;
                channelElement.SetElementValue("LOF2", LOF2.ToString());                
            }

            if (LOFSW != headEnd.LOFSW)
            {
                fieldNames.Add("LOFSW: was " + LOFSW + " now " + headEnd.LOFSW);
                LOFSW = headEnd.LOFSW;
                channelElement.SetElementValue("LOFSW", LOF1.ToString());                
            }

            if (checkCAChanges(station.ConditionalAccessEntries, station.Encrypted))
            {
                if (station.ConditionalAccessEntries != null)
                {
                    StringBuilder caString = new StringBuilder();
                    
                    if (CA == null)
                        caString.Append("CA old=null");
                    else
                    {
                        caString.Append("CA old=");
                        foreach (DVBLinkCAEntry oldCAEntry in CA)
                        {
                            if (caString.Length != 7)
                                caString.Append(" ");
                            caString.Append(oldCAEntry.Pid + ":" + oldCAEntry.SystemID);
                        }
                    }

                    caString.Append(" new=");
                    bool first = true;
                    foreach (ConditionalAccessEntry newCAEntry in station.ConditionalAccessEntries)
                    {
                        if (!first)
                            caString.Append(" ");
                        caString.Append(newCAEntry.PID + ":" + newCAEntry.SystemID);
                        first = false;
                    }

                    CA = new Collection<DVBLinkCAEntry>();
                    foreach (ConditionalAccessEntry accessEntry in station.ConditionalAccessEntries)
                        CA.Add(new DVBLinkCAEntry(accessEntry.PID, accessEntry.SystemID));

                    DVBLinkElement caElement = channelElement.FindElement("CA");
                    if (caElement != null)
                        caElement.Attributes = createCAElement(CA).Attributes;
                    else
                    {
                        DVBLinkElement nameElement = channelElement.FindElement("Name");
                        channelElement.Elements.Insert(channelElement.Elements.IndexOf(nameElement), createCAElement(CA));
                    }

                    fieldNames.Add(caString.ToString());
                }
                else
                {
                    CA = null;

                    DVBLinkElement caElement = channelElement.FindElement("CA");
                    if (caElement != null)
                        channelElement.Elements.Remove(caElement);

                    fieldNames.Add("CA removed");
                }                
            }

            if (station.Encrypted && (CA == null || CA.Count == 0))
                Logger.Instance.Write("Physical channel " + FullDescription + " is encrypted but no CA entries available to output");

            if (station.NewName == null)
            {
                if (Name != station.Name)
                {
                    fieldNames.Add("Name: was " + Name + " now " + station.Name);
                    Name = station.Name;
                    channelElement.SetElementValue("Name", Name);
                    logicalChangeNeeded = true;
                }
            }
            else
            {
                if (Name != station.NewName)
                {
                    fieldNames.Add("Name: was " + Name + " now " + station.NewName);
                    Name = station.NewName;
                    channelElement.SetElementValue("Name", Name);
                    logicalChangeNeeded = true;
                }
            }

            if (Provider != station.ProviderName)
            {
                fieldNames.Add("Provider: was " + Provider + " now " + station.ProviderName);
                Provider = station.ProviderName;
                channelElement.SetElementValue("Provider", Provider);                
            }

            if (fieldNames.Count == 0)
                return (false);

            Logger.Instance.Write("Changed physical satellite channel " + FullDescription + " " + headEnd.FullDescription);
            foreach (string fieldName in fieldNames)
                Logger.Instance.Write("Field changed: " + fieldName);

            Changed = true;
            ChannelsChanged++;

            return (logicalChangeNeeded);
        }

        internal bool changeTerrestrialFrequency(TerrestrialFrequency frequency, DVBLinkHeadEnd headEnd, TVStation station)
        {
            bool logicalChangeNeeded = false;

            Collection<string> fieldNames = new Collection<string>();

            int newEncrypt;

            if (!station.Encrypted)
                newEncrypt = 0;
            else
                newEncrypt = 1;

            if (Encrypt != newEncrypt)
            {
                fieldNames.Add("Encrypt: was " + Encrypt + " now " + newEncrypt);
                Encrypt = newEncrypt;
                channelElement.SetElementValue("Encrypt", Encrypt.ToString());
            }

            int newType;

            if (!station.IsRadio)
                newType = 1;
            else
                newType = 2;

            if (Type != newType)
            {
                fieldNames.Add("Type: was " + Type + " now " + newType);
                Type = newType;
                channelElement.SetElementValue("Type", Type.ToString());
            }

            if (RunParameters.Instance.ChannelUpdateNumber)
            {
                if (station.LogicalChannelNumber == -1)
                {
                    if (ChNum != station.OriginalChannelNumber)
                    {
                        fieldNames.Add("ChNum: was " + ChNum + " now " + station.OriginalChannelNumber);
                        ChNum = station.OriginalChannelNumber;
                        channelElement.SetElementValue("ChNum", ChNum.ToString());
                        logicalChangeNeeded = true;
                    }
                }
                else
                {
                    if (ChNum != station.LogicalChannelNumber)
                    {
                        fieldNames.Add("ChNum: was " + ChNum + " now " + station.LogicalChannelNumber);
                        ChNum = station.LogicalChannelNumber;
                        channelElement.SetElementValue("ChNum", ChNum.ToString());
                        logicalChangeNeeded = true;
                    }
                }

                if (ChSubNum != station.MinorChannelNumber)
                {
                    if (ChSubNum != 0)
                    {
                        int oldChSubNum = ChSubNum;

                        if (station.MinorChannelNumber != -1)
                        {
                            ChSubNum = station.MinorChannelNumber;
                            channelElement.SetElementValue("ChSubNum", station.MinorChannelNumber.ToString());
                        }
                        else
                        {
                            ChSubNum = 0;
                            channelElement.SetElementValue("ChSubNum", "0");
                        }

                        fieldNames.Add("ChSubNum: was " + oldChSubNum + " now " + ChSubNum);
                        logicalChangeNeeded = true;
                    }
                    else
                    {
                        if (station.MinorChannelNumber != -1)
                        {
                            fieldNames.Add("ChSubNum: was " + ChSubNum + " now " + station.MinorChannelNumber);
                            ChSubNum = station.MinorChannelNumber;
                            channelElement.SetElementValue("ChSubNum", station.MinorChannelNumber.ToString());
                            logicalChangeNeeded = true;
                        }
                    }

                }
            }

            if (checkCAChanges(station.ConditionalAccessEntries, station.Encrypted))
            {
                if (station.ConditionalAccessEntries != null)
                {
                    StringBuilder caString = new StringBuilder();

                    if (CA == null)
                        caString.Append("CA old=null");
                    else
                    {
                        caString.Append("CA old=");
                        foreach (DVBLinkCAEntry oldCAEntry in CA)
                        {
                            if (caString.Length != 7)
                                caString.Append(" ");
                            caString.Append(oldCAEntry.Pid + ":" + oldCAEntry.SystemID);
                        }
                    }

                    caString.Append(" new=");
                    bool first = true;
                    foreach (ConditionalAccessEntry newCAEntry in station.ConditionalAccessEntries)
                    {
                        if (!first)
                            caString.Append(" ");
                        caString.Append(newCAEntry.PID + ":" + newCAEntry.SystemID);
                        first = false;
                    }

                    CA = new Collection<DVBLinkCAEntry>();
                    foreach (ConditionalAccessEntry accessEntry in station.ConditionalAccessEntries)
                        CA.Add(new DVBLinkCAEntry(accessEntry.PID, accessEntry.SystemID));

                    DVBLinkElement caElement = channelElement.FindElement("CA");
                    if (caElement != null)
                        caElement.Attributes = createCAElement(CA).Attributes;
                    else
                    {
                        DVBLinkElement nameElement = channelElement.FindElement("Name");
                        channelElement.Elements.Insert(channelElement.Elements.IndexOf(nameElement), createCAElement(CA));
                    }

                    fieldNames.Add(caString.ToString());
                }
                else
                {
                    CA = null;

                    DVBLinkElement caElement = channelElement.FindElement("CA");
                    if (caElement != null)
                        channelElement.Elements.Remove(caElement);

                    fieldNames.Add("CA removed");
                }
            }

            if (station.Encrypted && (CA == null || CA.Count == 0))
                Logger.Instance.Write("Physical channel " + FullDescription + " is encrypted but no CA entries available to output");

            if (station.NewName == null)
            {
                if (Name != station.Name)
                {
                    fieldNames.Add("Name: was " + Name + " now " + station.Name);
                    Name = station.Name;
                    channelElement.SetElementValue("Name", Name);
                    logicalChangeNeeded = true;
                }
            }
            else
            {
                if (Name != station.NewName)
                {
                    fieldNames.Add("Name: was " + Name + " now " + station.NewName);
                    Name = station.NewName;
                    channelElement.SetElementValue("Name", Name);
                    logicalChangeNeeded = true;
                }
            }

            if (Provider != station.ProviderName)
            {
                fieldNames.Add("Provider: was " + Provider + " now " + station.ProviderName);
                Provider = station.ProviderName;
                channelElement.SetElementValue("Provider", Provider);
            }

            if (fieldNames.Count == 0)
                return (false);

            Logger.Instance.Write("Changed physical terrestrial channel " + FullDescription + " " + headEnd.FullDescription);
            foreach (string fieldName in fieldNames)
                Logger.Instance.Write("Field changed: " + fieldName);

            Changed = true;
            ChannelsChanged++;

            return (logicalChangeNeeded);
        }

        internal bool changeCableFrequency(CableFrequency frequency, DVBLinkHeadEnd headEnd, TVStation station)
        {
            bool logicalChangeNeeded = false;

            Collection<string> fieldNames = new Collection<string>();

            int newSR = frequency.SymbolRate;
            if (SR != newSR)
            {
                fieldNames.Add("SR: was " + SR + " now " + newSR);
                SR = newSR;
                channelElement.SetElementValue("SR", SR.ToString());
            }

            int newLOF = getCableLOF(frequency);
            if (LOF != newLOF)
            {
                fieldNames.Add("LOF: was " + LOF + " now " + newLOF);
                LOF = newLOF;
                channelElement.SetElementValue("LOF", LOF.ToString());
            }            

            int newEncrypt;

            if (!station.Encrypted)
                newEncrypt = 0;
            else
                newEncrypt = 1;

            if (Encrypt != newEncrypt)
            {
                fieldNames.Add("Encrypt: was " + Encrypt + " now " + newEncrypt);
                Encrypt = newEncrypt;
                channelElement.SetElementValue("Encrypt", Encrypt.ToString());
            }

            int newType;

            if (!station.IsRadio)
                newType = 1;
            else
                newType = 2;

            if (Type != newType)
            {
                fieldNames.Add("Type: was " + Type + " now " + newType);
                Type = newType;
                channelElement.SetElementValue("Type", Type.ToString());
            }

            if (RunParameters.Instance.ChannelUpdateNumber)
            {
                if (station.LogicalChannelNumber == -1)
                {
                    if (ChNum != station.OriginalChannelNumber)
                    {
                        fieldNames.Add("ChNum: was " + ChNum + " now " + station.OriginalChannelNumber);
                        ChNum = station.OriginalChannelNumber;
                        channelElement.SetElementValue("ChNum", ChNum.ToString());
                        logicalChangeNeeded = true;
                    }
                }
                else
                {
                    if (ChNum != station.LogicalChannelNumber)
                    {
                        fieldNames.Add("ChNum: was " + ChNum + " now " + station.LogicalChannelNumber);
                        ChNum = station.LogicalChannelNumber;
                        channelElement.SetElementValue("ChNum", ChNum.ToString());
                        logicalChangeNeeded = true;
                    }
                }

                if (ChSubNum != station.MinorChannelNumber)
                {
                    if (ChSubNum != 0)
                    {
                        int oldChSubNum = ChSubNum;

                        if (station.MinorChannelNumber != -1)
                        {
                            ChSubNum = station.MinorChannelNumber;
                            channelElement.SetElementValue("ChSubNum", station.MinorChannelNumber.ToString());
                        }
                        else
                        {
                            ChSubNum = 0;
                            channelElement.SetElementValue("ChSubNum", "0");
                        }

                        fieldNames.Add("ChSubNum: was " + oldChSubNum + " now " + ChSubNum);
                        logicalChangeNeeded = true;
                    }
                    else
                    {
                        if (station.MinorChannelNumber != -1)
                        {
                            fieldNames.Add("ChSubNum: was " + ChSubNum + " now " + station.MinorChannelNumber);
                            ChSubNum = station.MinorChannelNumber;
                            channelElement.SetElementValue("ChSubNum", station.MinorChannelNumber.ToString());
                            logicalChangeNeeded = true;
                        }
                    }

                }
            }

            if (checkCAChanges(station.ConditionalAccessEntries, station.Encrypted))
            {
                if (station.ConditionalAccessEntries != null)
                {
                    StringBuilder caString = new StringBuilder();

                    if (CA == null)
                        caString.Append("CA old=null");
                    else
                    {
                        caString.Append("CA old=");
                        foreach (DVBLinkCAEntry oldCAEntry in CA)
                        {
                            if (caString.Length != 7)
                                caString.Append(" ");
                            caString.Append(oldCAEntry.Pid + ":" + oldCAEntry.SystemID);
                        }
                    }

                    caString.Append(" new=");
                    bool first = true;
                    foreach (ConditionalAccessEntry newCAEntry in station.ConditionalAccessEntries)
                    {
                        if (!first)
                            caString.Append(" ");
                        caString.Append(newCAEntry.PID + ":" + newCAEntry.SystemID);
                        first = false;
                    }

                    CA = new Collection<DVBLinkCAEntry>();
                    foreach (ConditionalAccessEntry accessEntry in station.ConditionalAccessEntries)
                        CA.Add(new DVBLinkCAEntry(accessEntry.PID, accessEntry.SystemID));

                    DVBLinkElement caElement = channelElement.FindElement("CA");
                    if (caElement != null)
                        caElement.Attributes = createCAElement(CA).Attributes;
                    else
                    {
                        DVBLinkElement nameElement = channelElement.FindElement("Name");
                        channelElement.Elements.Insert(channelElement.Elements.IndexOf(nameElement), createCAElement(CA));
                    }

                    fieldNames.Add(caString.ToString());
                }
                else
                {
                    CA = null;

                    DVBLinkElement caElement = channelElement.FindElement("CA");
                    if (caElement != null)
                        channelElement.Elements.Remove(caElement);

                    fieldNames.Add("CA removed");
                }
            }

            if (station.Encrypted && (CA == null || CA.Count == 0))
                Logger.Instance.Write("Physical channel " + FullDescription + " is encrypted but no CA entries available to output");

            if (station.NewName == null)
            {
                if (Name != station.Name)
                {
                    fieldNames.Add("Name: was " + Name + " now " + station.Name);
                    Name = station.Name;
                    channelElement.SetElementValue("Name", Name);
                    logicalChangeNeeded = true;
                }
            }
            else
            {
                if (Name != station.NewName)
                {
                    fieldNames.Add("Name: was " + Name + " now " + station.NewName);
                    Name = station.NewName;
                    channelElement.SetElementValue("Name", Name);
                    logicalChangeNeeded = true;
                }
            }

            if (Provider != station.ProviderName)
            {
                fieldNames.Add("Provider: was " + Provider + " now " + station.ProviderName);
                Provider = station.ProviderName;
                channelElement.SetElementValue("Provider", Provider);
            }

            if (fieldNames.Count == 0)
                return (false);

            Logger.Instance.Write("Changed physical cable channel " + FullDescription + " " + headEnd.FullDescription);
            foreach (string fieldName in fieldNames)
                Logger.Instance.Write("Field changed: " + fieldName);

            Changed = true;
            ChannelsChanged++;

            return (logicalChangeNeeded);
        }

        private bool checkCAChanges(Collection<ConditionalAccessEntry> conditionalAccessEntries, bool encrypted)
        {
            if (encrypted)
            {
                if (conditionalAccessEntries == null || conditionalAccessEntries.Count == 0)
                {
                    Logger.Instance.Write("Physical channel " + FullDescription + " is encrypted but no CA entries present in broadcast");
                    return (false);
                }
            }

            if (CA == null && conditionalAccessEntries != null)
                return (true);

            if (CA != null && conditionalAccessEntries == null)
                return (true);

            if (CA == null)
                return (false);

            if (CA.Count != conditionalAccessEntries.Count)
                return (true);

            foreach (DVBLinkCAEntry caEntry in CA)
            {
                bool found = false;

                foreach (ConditionalAccessEntry accessEntry in conditionalAccessEntries)
                {
                    if (caEntry.Pid == accessEntry.PID && caEntry.SystemID == accessEntry.SystemID)
                        found = true;
                }

                if (!found)
                    return (true);
            }

            return (false);
        }

        internal void Delete()
        {
            channelListElement.Elements.Remove(channelElement);
            ChannelsDeleted++;   

            Logger.Instance.Write("Deleted physical channel " + FullDescription + " " + HeadEnd.FullDescription);

            DVBLinkLogicalChannel logicalChannel = DVBLinkLogicalChannel.FindChannel(this);
            if (logicalChannel != null)
            {
                DVBLinkPhysicalChannelLink physicalChannelLink = logicalChannel.FindPhysicalChannelLink(this);
                if (physicalChannelLink != null)
                {
                    logicalChannel.Delete(physicalChannelLink);
                    if (logicalChannel.PhysicalChannelLinks.Count == 0)
                        logicalChannel.Delete();
                }                
            }       
        }

        internal static DVBLinkPhysicalChannel AddChannel(DVBLinkHeadEnd headEnd, TuningFrequency tuningFrequency, TVStation station)
        {
            SatelliteFrequency satelliteFrequency = tuningFrequency as SatelliteFrequency;
            if (satelliteFrequency != null)
                return addSatelliteChannel(satelliteFrequency, headEnd, station);

            TerrestrialFrequency terrestrialFrequency = tuningFrequency as TerrestrialFrequency;
            if (terrestrialFrequency != null)
                return addTerrestrialChannel(terrestrialFrequency, headEnd, station);

            CableFrequency cableFrequency = tuningFrequency as CableFrequency;
            if (cableFrequency != null)
                return addCableChannel(cableFrequency, headEnd, station);

            return null;
        }

        private static DVBLinkPhysicalChannel addSatelliteChannel(SatelliteFrequency frequency, DVBLinkHeadEnd headEnd, TVStation station)
        {
            DVBLinkPhysicalChannel newChannel = new DVBLinkPhysicalChannel(headEnd.Source, headEnd);
            
            newChannel.Diseqc = headEnd.DiseqcNumber;

            if (headEnd.IsCustomDiseqc)
                newChannel.DiseqcRawData = headEnd.DiseqcTypeID;
            else
                newChannel.DiseqcRawData = string.Empty;

            newChannel.Fec = getFec(frequency.FEC.Rate);
            newChannel.Freq = DVBLinkController.RoundFrequency(frequency.Frequency);
            newChannel.LOF = headEnd.LOF1;
            newChannel.Mod = getModulation(frequency.DVBModulation, frequency.IsS2, frequency.IsDishNetwork);
            newChannel.SR = frequency.SymbolRate;
            newChannel.Pol = frequency.DVBPolarization;
            newChannel.Nid = station.OriginalNetworkID;
            newChannel.Tid = station.TransportStreamID;
            newChannel.Sid = station.ServiceID;

            if (!station.Encrypted)
                newChannel.Encrypt = 0;
            else
                newChannel.Encrypt = 1;

            if (!station.IsRadio)
                newChannel.Type = 1;
            else
                newChannel.Type = 2;

            if (station.LogicalChannelNumber == -1)
                newChannel.ChNum = station.OriginalChannelNumber;
            else
                newChannel.ChNum = station.LogicalChannelNumber;
            if (station.MinorChannelNumber != -1)
                newChannel.ChSubNum = station.MinorChannelNumber;
            else
                newChannel.ChSubNum = 0;

            newChannel.LOF1 = headEnd.LOF1;
            newChannel.LOF2 = headEnd.LOF2;
            newChannel.LOFSW = headEnd.LOFSW;

            if (station.ConditionalAccessEntries != null)
            {
                newChannel.CA = new Collection<DVBLinkCAEntry>();
                foreach (ConditionalAccessEntry accessEntry in station.ConditionalAccessEntries)
                    newChannel.CA.Add(new DVBLinkCAEntry(accessEntry.PID, accessEntry.SystemID));
            }

            if (station.Encrypted && (newChannel.CA == null || newChannel.CA.Count == 0))
                Logger.Instance.Write("Physical channel " + newChannel.FullDescription + " is encrypted but no CA entries available to output");

            if (station.NewName == null)
                newChannel.Name = station.Name;
            else
                newChannel.Name = station.NewName;

            newChannel.Provider = station.ProviderName;
            newChannel.Id = newChannel.Freq + ":" + station.OriginalNetworkID + ":" + station.TransportStreamID + ":" + station.ServiceID;

            DVBLinkElement channelsElement = DVBLinkElement.FindElement(headEnd.BaseNode.Elements[0], new string[] { "TVSourceSettings", "Channels" });

            DVBLinkElement headEndElement = null;

            if (channelsElement.Elements == null)
            {
                channelsElement.Elements = new Collection<DVBLinkElement>();
                headEndElement = addHeadEnd(channelsElement, headEnd);
            }
            else
            {
                headEndElement = findHeadEnd(channelsElement, headEnd);
                if (headEndElement == null)
                    headEndElement = addHeadEnd(channelsElement, headEnd);
            }

            DVBLinkElement channelListElement = headEndElement.FindElement("ChannelList");

            if (channelListElement.Elements == null)
                channelListElement.Elements = new Collection<DVBLinkElement>();

            DVBLinkElement channelElement = new DVBLinkElement(("Channel"));
            channelListElement.Elements.Add(channelElement);
            
            channelElement.Elements = new Collection<DVBLinkElement>();

            channelElement.Elements.Add(new DVBLinkElement("Diseqc", newChannel.Diseqc.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("DiseqcRawData", newChannel.DiseqcRawData));
            channelElement.Elements.Add(new DVBLinkElement("Fec", newChannel.Fec));
            channelElement.Elements.Add(new DVBLinkElement("Freq", newChannel.Freq.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("LNBSel", "1"));
            channelElement.Elements.Add(new DVBLinkElement("LOF", newChannel.LOF.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("Mod", newChannel.Mod.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("SR", newChannel.SR.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("Pol", newChannel.Pol.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("nid", newChannel.Nid.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("tid", newChannel.Tid.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("sid", newChannel.Sid.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("Encrypt", newChannel.Encrypt.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("Type", newChannel.Type.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("ChNum", newChannel.ChNum.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("ChSubNum", newChannel.ChSubNum.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("ecm_pid", newChannel.Ecm_Pid.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("LOF1", newChannel.LOF1.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("LOF2", newChannel.LOF2.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("LOFSW", newChannel.LOFSW.ToString()));

            if (newChannel.CA != null)
                channelElement.Elements.Add(createCAElement(newChannel.CA));

            channelElement.Elements.Add(new DVBLinkElement("Name", newChannel.Name));
            channelElement.Elements.Add(new DVBLinkElement("Provider", newChannel.Provider));
            channelElement.Elements.Add(new DVBLinkElement("id", newChannel.Id));

            ChannelsAdded++;

            Logger.Instance.Write("Added physical satellite channel " + newChannel.FullDescription + " " + headEnd.FullDescription);            

            return (newChannel);
        }

        private static DVBLinkPhysicalChannel addTerrestrialChannel(TerrestrialFrequency frequency, DVBLinkHeadEnd headEnd, TVStation station)
        {
            DVBLinkPhysicalChannel newChannel = new DVBLinkPhysicalChannel(headEnd.Source, headEnd);

            newChannel.Diseqc = 0;
            newChannel.DiseqcRawData = string.Empty;
            newChannel.Fec = string.Empty;
            newChannel.Freq = DVBLinkController.RoundFrequency(frequency.Frequency);
            newChannel.LOF = 0;
            newChannel.Mod = 0;
            newChannel.SR = 0;
            newChannel.Pol = 0;
            newChannel.Nid = station.OriginalNetworkID;
            newChannel.Tid = station.TransportStreamID;
            newChannel.Sid = station.ServiceID;

            if (!station.Encrypted)
                newChannel.Encrypt = 0;
            else
                newChannel.Encrypt = 1;

            if (!station.IsRadio)
                newChannel.Type = 1;
            else
                newChannel.Type = 2;

            if (station.LogicalChannelNumber == -1)
                newChannel.ChNum = station.OriginalChannelNumber;
            else
                newChannel.ChNum = station.LogicalChannelNumber;
            if (station.MinorChannelNumber != -1)
                newChannel.ChSubNum = station.MinorChannelNumber;
            else
                newChannel.ChSubNum = 0;

            newChannel.LOF1 = 0;
            newChannel.LOF2 = 0;
            newChannel.LOFSW = 0;

            if (station.ConditionalAccessEntries != null)
            {
                newChannel.CA = new Collection<DVBLinkCAEntry>();
                foreach (ConditionalAccessEntry accessEntry in station.ConditionalAccessEntries)
                    newChannel.CA.Add(new DVBLinkCAEntry(accessEntry.PID, accessEntry.SystemID));
            }

            if (station.Encrypted && (newChannel.CA == null || newChannel.CA.Count == 0))
                Logger.Instance.Write("Physical channel " + newChannel.FullDescription + " is encrypted but no CA entries available to output");

            if (station.NewName == null)
                newChannel.Name = station.Name;
            else
                newChannel.Name = station.NewName;

            newChannel.Provider = station.ProviderName;
            newChannel.Id = newChannel.Freq + ":" + station.OriginalNetworkID + ":" + station.TransportStreamID + ":" + station.ServiceID;

            DVBLinkElement channelsElement = DVBLinkElement.FindElement(headEnd.BaseNode.Elements[0], new string[] { "TVSourceSettings", "Channels" });

            DVBLinkElement headEndElement = null;

            if (channelsElement.Elements == null)
            {
                channelsElement.Elements = new Collection<DVBLinkElement>();
                headEndElement = addHeadEnd(channelsElement, headEnd);
            }
            else
            {
                headEndElement = findHeadEnd(channelsElement, headEnd);
                if (headEndElement == null)
                    headEndElement = addHeadEnd(channelsElement, headEnd);
            }

            DVBLinkElement channelListElement = headEndElement.FindElement("ChannelList");

            if (channelListElement.Elements == null)
                channelListElement.Elements = new Collection<DVBLinkElement>();

            DVBLinkElement channelElement = new DVBLinkElement(("Channel"));
            channelListElement.Elements.Add(channelElement);

            channelElement.Elements = new Collection<DVBLinkElement>();

            channelElement.Elements.Add(new DVBLinkElement("Diseqc", newChannel.Diseqc.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("DiseqcRawData", newChannel.DiseqcRawData));
            channelElement.Elements.Add(new DVBLinkElement("Fec", newChannel.Fec));
            channelElement.Elements.Add(new DVBLinkElement("Freq", newChannel.Freq.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("LNBSel", "0"));
            channelElement.Elements.Add(new DVBLinkElement("LOF", newChannel.LOF.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("Mod", newChannel.Mod.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("SR", newChannel.SR.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("Pol", newChannel.Pol.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("nid", newChannel.Nid.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("tid", newChannel.Tid.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("sid", newChannel.Sid.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("Encrypt", newChannel.Encrypt.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("Type", newChannel.Type.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("ChNum", newChannel.ChNum.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("ChSubNum", newChannel.ChSubNum.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("ecm_pid", newChannel.Ecm_Pid.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("LOF1", newChannel.LOF1.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("LOF2", newChannel.LOF2.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("LOFSW", newChannel.LOFSW.ToString()));

            if (newChannel.CA != null)
                channelElement.Elements.Add(createCAElement(newChannel.CA));

            channelElement.Elements.Add(new DVBLinkElement("Name", newChannel.Name));
            channelElement.Elements.Add(new DVBLinkElement("Provider", newChannel.Provider));
            channelElement.Elements.Add(new DVBLinkElement("id", newChannel.Id));

            ChannelsAdded++;

            Logger.Instance.Write("Added physical terrestrial channel " + newChannel.FullDescription + " " + headEnd.FullDescription);

            return (newChannel);
        }

        private static DVBLinkPhysicalChannel addCableChannel(CableFrequency frequency, DVBLinkHeadEnd headEnd, TVStation station)
        {
            DVBLinkPhysicalChannel newChannel = new DVBLinkPhysicalChannel(headEnd.Source, headEnd);

            newChannel.Diseqc = 0;
            newChannel.DiseqcRawData = string.Empty;

            newChannel.Fec = "0";
            newChannel.Freq = DVBLinkController.RoundFrequency(frequency.Frequency);
            newChannel.LOF = getCableLOF(frequency);
            newChannel.Mod = 0;
            newChannel.SR = frequency.SymbolRate;
            newChannel.Pol = 0;
            newChannel.Nid = station.OriginalNetworkID;
            newChannel.Tid = station.TransportStreamID;
            newChannel.Sid = station.ServiceID;

            if (!station.Encrypted)
                newChannel.Encrypt = 0;
            else
                newChannel.Encrypt = 1;

            if (!station.IsRadio)
                newChannel.Type = 1;
            else
                newChannel.Type = 2;

            if (station.LogicalChannelNumber == -1)
                newChannel.ChNum = station.OriginalChannelNumber;
            else
                newChannel.ChNum = station.LogicalChannelNumber;
            if (station.MinorChannelNumber != -1)
                newChannel.ChSubNum = station.MinorChannelNumber;
            else
                newChannel.ChSubNum = 0;

            newChannel.LOF1 = 0;
            newChannel.LOF2 = 0;
            newChannel.LOFSW = 0;

            if (station.ConditionalAccessEntries != null)
            {
                newChannel.CA = new Collection<DVBLinkCAEntry>();
                foreach (ConditionalAccessEntry accessEntry in station.ConditionalAccessEntries)
                    newChannel.CA.Add(new DVBLinkCAEntry(accessEntry.PID, accessEntry.SystemID));
            }

            if (station.Encrypted && (newChannel.CA == null || newChannel.CA.Count == 0))
                Logger.Instance.Write("Physical channel " + newChannel.FullDescription + " is encrypted but no CA entries available to output");

            if (station.NewName == null)
                newChannel.Name = station.Name;
            else
                newChannel.Name = station.NewName;

            newChannel.Provider = station.ProviderName;
            newChannel.Id = newChannel.Freq + ":" + station.OriginalNetworkID + ":" + station.TransportStreamID + ":" + station.ServiceID;

            DVBLinkElement channelsElement = DVBLinkElement.FindElement(headEnd.BaseNode.Elements[0], new string[] { "TVSourceSettings", "Channels" });

            DVBLinkElement headEndElement = null;

            if (channelsElement.Elements == null)
            {
                channelsElement.Elements = new Collection<DVBLinkElement>();
                headEndElement = addHeadEnd(channelsElement, headEnd);
            }
            else
            {
                headEndElement = findHeadEnd(channelsElement, headEnd);
                if (headEndElement == null)
                    headEndElement = addHeadEnd(channelsElement, headEnd);
            }

            DVBLinkElement channelListElement = headEndElement.FindElement("ChannelList");

            if (channelListElement.Elements == null)
                channelListElement.Elements = new Collection<DVBLinkElement>();

            DVBLinkElement channelElement = new DVBLinkElement(("Channel"));
            channelListElement.Elements.Add(channelElement);

            channelElement.Elements = new Collection<DVBLinkElement>();

            channelElement.Elements.Add(new DVBLinkElement("Diseqc", newChannel.Diseqc.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("DiseqcRawData", newChannel.DiseqcRawData));
            channelElement.Elements.Add(new DVBLinkElement("Fec", newChannel.Fec));
            channelElement.Elements.Add(new DVBLinkElement("Freq", newChannel.Freq.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("LNBSel", "0"));
            channelElement.Elements.Add(new DVBLinkElement("LOF", newChannel.LOF.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("Mod", newChannel.Mod.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("SR", newChannel.SR.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("Pol", newChannel.Pol.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("nid", newChannel.Nid.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("tid", newChannel.Tid.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("sid", newChannel.Sid.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("Encrypt", newChannel.Encrypt.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("Type", newChannel.Type.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("ChNum", newChannel.ChNum.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("ChSubNum", newChannel.ChSubNum.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("ecm_pid", newChannel.Ecm_Pid.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("LOF1", newChannel.LOF1.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("LOF2", newChannel.LOF2.ToString()));
            channelElement.Elements.Add(new DVBLinkElement("LOFSW", newChannel.LOFSW.ToString()));

            if (newChannel.CA != null)
                channelElement.Elements.Add(createCAElement(newChannel.CA));

            channelElement.Elements.Add(new DVBLinkElement("Name", newChannel.Name));
            channelElement.Elements.Add(new DVBLinkElement("Provider", newChannel.Provider));
            channelElement.Elements.Add(new DVBLinkElement("id", newChannel.Id));

            ChannelsAdded++;

            Logger.Instance.Write("Added physical channel " + newChannel.FullDescription + " " + headEnd.FullDescription);

            return (newChannel);
        }

        private static DVBLinkElement createCAElement(Collection<DVBLinkCAEntry> accessEntries)
        {
            DVBLinkElement caElement = new DVBLinkElement("CA");
            caElement.Elements =  new Collection<DVBLinkElement>();

            foreach (DVBLinkCAEntry caEntry in accessEntries)
            {
                DVBLinkElement descriptorElement = new DVBLinkElement("descriptor");
                descriptorElement.Attributes = new Collection<DVBLinkAttribute>();                
                descriptorElement.Attributes.Add(new DVBLinkAttribute("SysId", caEntry.SystemID.ToString()));
                descriptorElement.Attributes.Add(new DVBLinkAttribute("pid", caEntry.Pid.ToString()));

                caElement.Elements.Add(descriptorElement);
            }

            return (caElement);
        }

        private static int getCableLOF(CableFrequency frequency)
        {
            switch (frequency.Modulation)
            {
                case SignalModulation.Modulation.QAM16:
                    return 3;
                case SignalModulation.Modulation.QAM32:
                    return 4;
                case SignalModulation.Modulation.QAM64:
                    return 5;
                case SignalModulation.Modulation.QAM128:
                    return 6;
                case SignalModulation.Modulation.QAM256:
                    return 7;
                default:
                    return 0;
            }
        }

        private static DVBLinkElement addHeadEnd(DVBLinkElement channelsElement, DVBLinkHeadEnd headEnd)
        {
            DVBLinkElement headEndElement = new DVBLinkElement("Headend");
            channelsElement.Elements.Add(headEndElement);

            headEndElement.Elements = new Collection<DVBLinkElement>();
            headEndElement.Elements.Add(new DVBLinkElement("HeadendID", headEnd.HeadEndID));
            headEndElement.Elements.Add(new DVBLinkElement("ChannelList"));

            headEndElement.Elements[1].Elements = new Collection<DVBLinkElement>();

            return (headEndElement);
        }

        internal static DVBLinkElement findHeadEnd(DVBLinkElement channelsElement, DVBLinkHeadEnd headEnd)
        {
            if (channelsElement.Elements == null)
                return (null);

            foreach (DVBLinkElement headEndElement in channelsElement.Elements)
            {
                if (headEndElement.Name == "Headend")
                {
                    if (headEndElement.Elements != null)
                    {
                        DVBLinkElement headEndIDElement = headEndElement.FindElement("HeadendID");
                        if (headEndIDElement != null)
                        {
                            if (headEndIDElement.Value == headEnd.HeadEndID)
                                return (headEndElement);
                        }
                    }
                }
            }

            return (null);
        }

        internal static string getFec(string fec)
        {
            string[] parts = fec.Split(new char[] { '/' }  );            
            return (((Int32.Parse(parts[0]) * 16) + (Int32.Parse(parts[1]))).ToString());
        }

        internal static int getModulation(int modulation, bool isS2, bool isDishNetwork)
        {
            switch (modulation)
            {
                case 0:
                    return (0);
                case 1:
                    if (!isS2)
                    {
                        if (!isDishNetwork)
                            return (0);
                        else
                            return (4);
                    }
                    else
                        return (6);
                case 2:
                    if (!isDishNetwork)
                        return (1);
                    else
                        return (5);
                case 3:
                    return (2);
                default:
                    return (0);
            }
        }
    }
}
