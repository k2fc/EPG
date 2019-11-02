////////////////////////////////////////////////////////////////////////////////// 
//                                                                              //
//      Copyright (C) 2005-2012 nzsjb                                           //
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
    public class DVBLinkSubTreeNode : DVBLinkBaseNode
    {
        public DVBLinkSubTreeNode() { }

        /// <summary>
        /// Load the node.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns>True if the load succeeds; false otherwise.</returns>
        public bool Load(XmlReader xmlReader, string name)
        {
            bool reply = true;
            int depth = xmlReader.Depth;
            DVBLinkElement currentElement = null;

            try
            {
                /*while (!xmlReader.EOF)
                {
                    if (xmlReader.IsStartElement())
                    {
                        if (xmlReader.Depth == depth)
                        {
                            currentElement = new DVBLinkElement(xmlReader.Name, xmlReader.ReadString());

                            if (Elements == null)
                                Elements = new Collection<DVBLinkElement>();

                            Elements.Add(currentElement);
                        }
                        else
                        {
                            currentElement.SubTreeNode = new DVBLinkSubTreeNode();
                            currentElement.SubTreeNode.Load(xmlReader, currentElement.Name);
                        }
                    }

                    xmlReader.Read();
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

                                if (Elements == null)
                                    Elements = new Collection<DVBLinkElement>();

                                Elements.Add(currentElement);
                            }
                            else
                            {
                                currentElement.SubTreeNode = new DVBLinkSubTreeNode();
                                currentElement.SubTreeNode.Load(xmlReader, currentElement.Name);
                            }
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
                Logger.Instance.Write("Failed to sub-tree node");
                Logger.Instance.Write("Data exception: " + e.Message);
                reply = false;
            }
            catch (IOException e)
            {
                Logger.Instance.Write("Failed to load physical_channel node");
                Logger.Instance.Write("I/O exception: " + e.Message);
                reply = false;
            }

            return (reply);
        }

        public void Unload(XmlWriter xmlWriter, string name)
        {
            xmlWriter.WriteStartElement(name);

            if (Elements != null)
            {
                foreach (DVBLinkElement element in Elements)
                {
                    if (element.SubTreeNode == null)
                    {
                        if (element.Attributes == null || element.Attributes.Count == 0)
                            xmlWriter.WriteElementString(element.Name, element.Value);
                        else
                        {
                            xmlWriter.WriteStartElement(element.Name);
                            foreach (DVBLinkAttribute attribute in element.Attributes)
                                xmlWriter.WriteAttributeString(attribute.Name, attribute.Value);
                            xmlWriter.WriteValue(element.Value);
                            xmlWriter.WriteEndElement();
                        }
                    }
                    else
                        element.SubTreeNode.Unload(xmlWriter, element.Name);
                }
            }

            xmlWriter.WriteEndElement();
        }
    }
}
