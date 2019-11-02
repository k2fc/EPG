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
using System.IO;
using System.Xml;
using System.Text;
using System.Collections.ObjectModel;

using DomainObjects;

namespace ChannelUpdate
{
    internal class DVBLinkChannelStorage
    {
        internal DVBLinkChannelInfoNode ChannelInfoNode { get; private set; }

        internal DVBLinkChannelStorage() { }

        internal bool Load(string installPath)
        {
            XmlReader xmlReader = null;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;

            string fileName = Path.Combine(installPath, "dvblink_channel_storage.xml");

            try
            {
                xmlReader = XmlReader.Create(fileName, settings);
            }
            catch (IOException)
            {
                Logger.Instance.Write("Failed to open " + fileName);
                return (false);
            }

            Logger.Instance.Write("Processing " + fileName);

            xmlReader.Read();
            if (xmlReader.Name != "xml")
                throw (new FormatException("Expected xml element - got " + xmlReader.Name));

            ChannelInfoNode = new DVBLinkChannelInfoNode();
            bool reply = ChannelInfoNode.Load(xmlReader);

            if (xmlReader != null)
                xmlReader.Close();

            return (reply);
        }

        internal bool Unload(string installPath)
        {
            string fileName = Path.Combine(installPath, "dvblink_channel_storage.xml");
            string backupName = Path.Combine(installPath, "dvblink_channel_storage.xml.bak");

            try
            {
                Logger.Instance.Write("Deleting backup file " + backupName);
                File.SetAttributes(backupName, FileAttributes.Normal);
                File.Delete(backupName);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("File delete exception: " + e.Message);
            }

            try
            {
                Logger.Instance.Write("Renaming " + fileName + " for backup");
                File.Move(fileName, backupName);
                File.SetAttributes(backupName, FileAttributes.ReadOnly);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("File rename exception: " + e.Message);
                return (false);
            }

            Logger.Instance.Write("Creating channel storage file: " + fileName);

            XmlWriterSettings settings = new XmlWriterSettings();

            settings.Indent = true;
            settings.NewLineOnAttributes = false;
            settings.Encoding = new UTF8Encoding();
            settings.CloseOutput = true;

            try
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(fileName, settings))
                {
                    xmlWriter.WriteStartDocument();
                    ChannelInfoNode.Unload(xmlWriter);
                    xmlWriter.WriteEndDocument();

                    xmlWriter.Flush();
                    xmlWriter.Close();
                }
            }
            catch (XmlException e)
            {
                Logger.Instance.Write("<E> XML Exception creating channel storage file");
                Logger.Instance.Write("E>" + e.Message);
                return (false);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("<E> IO Exception creating channel storage file");
                Logger.Instance.Write("E>" + e.Message);
                return (false);
            }

            return (true);
        }

        internal void Clear()
        {
            ChannelInfoNode = new DVBLinkChannelInfoNode();
            DVBLinkLogicalChannel.BaseNode = ChannelInfoNode;

            ChannelInfoNode.Elements = new Collection<DVBLinkElement>();
            ChannelInfoNode.Elements.Add(new DVBLinkElement("channel_info"));

            ChannelInfoNode.Elements[0].Elements = new Collection<DVBLinkElement>();
            ChannelInfoNode.Elements[0].Elements.Add(new DVBLinkElement("channel_map"));
            ChannelInfoNode.Elements[0].Elements.Add(new DVBLinkElement("epg_map"));
            ChannelInfoNode.Elements[0].Elements.Add(new DVBLinkElement("record_configuration"));

            Logger.Instance.Write("Channel storage cleared");
        }
    }
}
