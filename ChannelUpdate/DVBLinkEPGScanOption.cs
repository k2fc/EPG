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
    internal class DVBLinkEPGScanOption
    {
        internal string Source { get; private set; }

        internal static Collection<DVBLinkEPGScanOption> EPGScanOptions { get; private set; }

        internal DVBLinkBaseNode BaseNode { get; private set; }
        internal DVBLinkElement BaseElement { get; private set; }

        internal string Name { get; private set; }
        internal string Enabled { get; private set; }
        internal string ShowNoNameChannels { get; private set; }
        internal string ScanMode { get; private set; }
        internal string Lang { get; private set; }

        internal Collection<DVBLinkEPGScanOptionTransponder> Transponders { get; private set; }

        internal DVBLinkEPGScanOption() { }

        internal bool Load(string source, DVBLinkBaseNode baseNode, DVBLinkElement baseElement)
        {
            if (baseElement.Elements == null)
                return (false);

            Source = source;

            BaseNode = baseNode;
            BaseElement = baseElement;

            try
            {
                Name = baseElement.Name;
                Enabled = baseElement.GetElementValue("Enabled");
                ShowNoNameChannels = baseElement.GetElementValue("ShowNoNameChannels");
                ScanMode = baseElement.GetElementValue("ScanMode");
                Lang = baseElement.GetElementValue("Lang");

                DVBLinkElement transpondersElement = DVBLinkElement.FindElement(baseElement, new string[] { "Transponders" });
                if (transpondersElement != null && transpondersElement.Elements != null && transpondersElement.Elements.Count != 0)
                {
                    Transponders = new Collection<DVBLinkEPGScanOptionTransponder>();

                    foreach (DVBLinkElement transponderElement in transpondersElement.Elements)
                    {
                        DVBLinkEPGScanOptionTransponder newTransponder = new DVBLinkEPGScanOptionTransponder();
                        newTransponder.Load(baseNode, transponderElement);
                        Transponders.Add(newTransponder);
                    }

                }

                return (true);
            }
            catch (Exception e)
            {
                Logger.Instance.Write("<E> An exception of type " + e.GetType().Name + " has occurred while parsing the xml for an epg scan option");
                Logger.Instance.Write("<E> " + e.Message);

                return (false);
            }
        }

        internal static bool LoadEPGScanOptions(string source, DVBLinkBaseNode baseNode)
        {
            if (EPGScanOptions == null)
                EPGScanOptions = new Collection<DVBLinkEPGScanOption>();

            DVBLinkElement epgScanOptionsElement = DVBLinkBaseNode.FindElement(baseNode, new string[] { "TVSourceSettings", "EPGScanOptions" });
            if (epgScanOptionsElement == null || epgScanOptionsElement.Elements == null)
                return (false);

            foreach (DVBLinkElement epgScanOptionElement in epgScanOptionsElement.Elements)
            {
                DVBLinkEPGScanOption newScanOption = new DVBLinkEPGScanOption();
                if (newScanOption.Load(source, baseNode, epgScanOptionElement))
                    EPGScanOptions.Add(newScanOption);
            }

            return (true);
        }

        internal static DVBLinkEPGScanOption FindScanOption(string name)
        {
            if (EPGScanOptions == null)
                return (null);

            foreach (DVBLinkEPGScanOption scanOption in EPGScanOptions)
            {
                if (scanOption.Name == name)
                    return (scanOption);
            }

            return (null);
        }

        internal DVBLinkEPGScanOptionTransponder FindTransponder(string headEndID, string transponderID)
        {
            if (Transponders == null)
                return (null);

            foreach (DVBLinkEPGScanOptionTransponder transponder in Transponders)
            {
                if (transponder.HeadEndID == headEndID && transponder.TransponderID == transponderID)
                    return (transponder);
            }

            return (null);
        }
    }
}
