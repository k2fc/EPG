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
using System.Xml;
using System.IO;

using DomainObjects;

namespace ChannelUpdate
{
    internal abstract class DVBLinkBaseNode
    {
        internal Collection<DVBLinkElement> Elements { get; set; }

        internal DVBLinkBaseNode() { }

        internal string GetElementValue(string name)
        {
            DVBLinkElement element = FindElement(name);
            if (element != null)
                return (element.Value);
            else
                return (null);
        }

        internal bool SetElementValue(string name, string value)
        {
            DVBLinkElement element = FindElement(name);
            if (element != null)
            {
                element.Value = value;
                return (true);
            }
            else
                return (false);
        }

        internal DVBLinkElement FindElement(string name)
        {
            return (FindElement(Elements, name));
        }

        internal virtual bool Load(XmlReader xmlReader)
        {
            Elements = new Collection<DVBLinkElement>();

            DVBLinkElement currentElement = new DVBLinkElement(xmlReader.Name);
            Elements.Add(currentElement);

            bool reply = true;
            int depth = 0;

            try
            {
                while (!xmlReader.EOF)
                {
                    xmlReader.Read();

                    if (xmlReader.IsStartElement())
                    {
                        if (xmlReader.Depth == depth)
                        { 
                            currentElement = new DVBLinkElement(xmlReader.Name, xmlReader.ReadString());
                            
                            if (xmlReader.HasAttributes)
                            {
                                currentElement.Attributes = new Collection<DVBLinkAttribute>();

                                for (int index = 0; index < xmlReader.AttributeCount; index++)
                                {
                                    xmlReader.MoveToAttribute(index);
                                    currentElement.Attributes.Add(new DVBLinkAttribute(xmlReader.Name, xmlReader.Value));
                                }
                            }
                            Elements.Add(currentElement);
                        }
                        else
                            currentElement.Load(xmlReader);
                    }
                }
            }
            catch (XmlException e)
            {
                Logger.Instance.Write("Failed to load dvblink_configuration node");
                Logger.Instance.Write("Data exception: " + e.Message);
                reply = false;
            }
            catch (IOException e)
            {
                Logger.Instance.Write("Failed to load dvblink_configuration node");
                Logger.Instance.Write("I/O exception: " + e.Message);
                reply = false;
            }

            return (reply);
        }

        internal void Unload(XmlWriter xmlWriter)
        {
            bool first = true;

            foreach (DVBLinkElement element in Elements)
            {
                element.Unload(xmlWriter, first);
                first = false;
            }
        }

        internal string GetValue(string[] elementNames)
        {
            return (GetValue(this, elementNames));
        }

        internal static string GetValue(DVBLinkBaseNode baseNode, string[] elementNames)
        {
            DVBLinkElement element = FindElement(baseNode, elementNames);
            if (element != null)
                return (element.Value);
            else
                return (null);
        }

        internal static DVBLinkElement FindElement(DVBLinkBaseNode baseNode, string[] elementNames)
        {
            Collection<DVBLinkElement> currentNode = baseNode.Elements;
            DVBLinkElement element = null;

            foreach (string nodeName in elementNames)
            {
                element = FindElement(currentNode, nodeName);
                if (element == null)
                    return (null);

                if (element.Elements != null)
                    currentNode = element.Elements;
            }

            return (element);
        }

        internal static DVBLinkElement FindElement(Collection<DVBLinkElement> elements, string name)
        {
            foreach (DVBLinkElement element in elements)
            {
                try
                {
                    if (element.Name == name)
                        return (element);
                }
                catch (Exception)
                {
                    Logger.Instance.Write("Element name is null");
                }
            }

            return (null);
        }
    }
}
