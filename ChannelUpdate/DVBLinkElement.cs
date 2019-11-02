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
using System.IO;
using System.Xml;
using System.Reflection;

using DomainObjects;

namespace ChannelUpdate
{
    internal class DVBLinkElement
    {        
        internal string Name { get; private set; }
        internal string Value { get; set; }

        internal Collection<DVBLinkAttribute> Attributes { get; set; }

        internal Collection<DVBLinkElement> Elements { get; set; }

        internal DVBLinkElement() { }

        internal DVBLinkElement(string name)
        {
            Name = name;
        }

        internal DVBLinkElement(string name, string value)
        {
            Name = name;
            Value = value;
        }

        internal DVBLinkAttribute FindAttribute(string name)
        {
            if (Attributes == null)
                return (null);

            foreach (DVBLinkAttribute attribute in Attributes)
            {
                if (attribute.Name == name)
                    return (attribute);
            }

            return (null);
        }

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

        /// <summary>
        /// Load the node.
        /// </summary>
        /// <param name="xmlReader">An XmlReader instance for the node.</param>
        /// <returns>True if the load succeeds; false otherwise.</returns>
        internal virtual bool Load(XmlReader xmlReader)
        {
            Elements = new Collection<DVBLinkElement>();
            DVBLinkElement currentElement = new DVBLinkElement(xmlReader.Name);            

            bool reply = true;
            int depth = xmlReader.Depth;

            try
            {
                /*while (!xmlReader.EOF)
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
                        {
                            depth = xmlReader.Depth;
                            currentElement.Load(xmlReader);
                        }
                    }*/

                while (!xmlReader.EOF)
                {
                    switch (xmlReader.NodeType)
                    {
                        case XmlNodeType.Element:
                            if (xmlReader.Depth == depth)
                            {
                                currentElement = new DVBLinkElement(xmlReader.Name);
                                
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
                            break;
                        case XmlNodeType.Text:
                            currentElement.Value = xmlReader.Value;
                            break;
                        case XmlNodeType.EndElement:
                            if (xmlReader.Depth < depth)
                                return (true);
                            break;
                    }

                    xmlReader.Read();
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

        internal void Unload(XmlWriter xmlWriter, bool first)
        {
            bool currentFirst = first;

            if (Elements == null)
            {
                if (Attributes == null || Attributes.Count == 0)
                {
                    if (currentFirst)
                    {
                        xmlWriter.WriteStartElement(Name);

                        xmlWriter.WriteAttributeString("generator-info-name", Assembly.GetCallingAssembly().GetName().Name
                        + "/" + Assembly.GetCallingAssembly().GetName().Version.ToString());

                        xmlWriter.WriteValue(Value);
                        xmlWriter.WriteEndElement();

                        currentFirst = false;
                    }
                    else
                        xmlWriter.WriteElementString(Name, Value);
                }
                else
                {
                    xmlWriter.WriteStartElement(Name);

                    if (currentFirst)
                    {
                        xmlWriter.WriteAttributeString("generator-info-name", Assembly.GetCallingAssembly().GetName().Name
                        + "/" + Assembly.GetCallingAssembly().GetName().Version.ToString());

                        currentFirst = false;
                    }

                    foreach (DVBLinkAttribute attribute in Attributes)
                        xmlWriter.WriteAttributeString(attribute.Name, attribute.Value);
                    xmlWriter.WriteValue(Value);
                    xmlWriter.WriteEndElement();
                }
            }
            else
            {
                xmlWriter.WriteStartElement(Name);

                if (currentFirst)
                {
                    xmlWriter.WriteAttributeString("generator-info-name", Assembly.GetCallingAssembly().GetName().Name
                    + "/" + Assembly.GetCallingAssembly().GetName().Version.ToString());

                    currentFirst = false;
                }

                foreach (DVBLinkElement element in Elements)
                    element.Unload(xmlWriter, currentFirst);

                xmlWriter.WriteEndElement();
            }
        }

        internal string GetValue(string[] elementNames)
        {
            return (GetValue(this, elementNames));
        }

        internal static string GetValue(DVBLinkElement baseElement, string[] elementNames)
        {
            DVBLinkElement element = FindElement(baseElement, elementNames);
            if (element != null)
                return (element.Value);
            else
                return (null);
        }

        internal static DVBLinkElement FindElement(DVBLinkElement baseElement, string[] elementNames)
        {
            if (baseElement.Name != elementNames[0])
                return (null);

            DVBLinkElement currentElement = baseElement;
            DVBLinkElement element = null;

            for (int index = 1; index < elementNames.Length; index++)
            {
                element = FindElement(currentElement.Elements, elementNames[index]);
                if (element == null)
                    return (null);

                currentElement = element;
            }

            return (element);
        }

        internal static DVBLinkElement FindElement(Collection<DVBLinkElement> elements, string name)
        {
            if (elements == null)
                return (null);

            foreach (DVBLinkElement element in elements)
            {
                if (element.Name == name)
                    return (element);
            }

            return (null);
        }
    }
}
