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

using DomainObjects;

namespace ChannelUpdate
{
    internal class DVBLinkEPGScanOptionTransponder
    {
        internal DVBLinkBaseNode BaseNode { get; private set; }
        internal DVBLinkElement BaseElement { get; private set; }

        internal string HeadEndID { get; private set; }
        internal string TransponderID { get; private set; }
        
        internal DVBLinkEPGScanOptionTransponder() { }

        internal bool Load(DVBLinkBaseNode baseNode, DVBLinkElement baseElement)
        {
            if (baseElement.Elements == null)
                return (false);

            BaseNode = baseNode;
            BaseElement = baseElement;

            try
            {
                HeadEndID = baseElement.GetElementValue("HeadendID");
                TransponderID = baseElement.GetElementValue("TransponderID");

                return (true);
            }
            catch (Exception e)
            {
                Logger.Instance.Write("<E> An exception of type " + e.GetType().Name + " has occurred while parsing the xml for an epg scan option transponder");
                Logger.Instance.Write("<E> " + e.Message);

                return (false);
            }
        }
    }
}
