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
using System.Xml;
using System.IO;
using System.Text;

using DomainObjects;

namespace ChannelUpdate
{
    internal class DVBLinkTVSourceSetting
    {
        internal string DirectoryPath { get; private set; }
        internal DVBLinkTVSourceSettingsNode SettingsNode { get; private set; }

        internal string Source
        {
            get
            {
                DirectoryInfo directoryInfo = new DirectoryInfo(DirectoryPath);
                return (directoryInfo.Name);
            }
        }

        internal DVBLinkTVSourceSetting() { }

        internal bool Load(string directoryPath)
        {
            DirectoryPath = directoryPath;

            XmlReader xmlReader = null;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;

            string fileName = Path.Combine(DirectoryPath, "TVSourceSettings.xml");

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

            SettingsNode = new DVBLinkTVSourceSettingsNode();
            bool reply = SettingsNode.Load(xmlReader);

            if (xmlReader != null)
                xmlReader.Close();

            return (reply);
        }

        internal bool Unload()
        {
            string fileName = Path.Combine(DirectoryPath, "TVSourceSettings.xml");
            string backupName = Path.Combine(DirectoryPath, "TVSourceSettings.xml.bak");

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

            Logger.Instance.Write("Creating settings file: " + fileName);

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
                    SettingsNode.Unload(xmlWriter);
                    xmlWriter.WriteEndDocument();

                    xmlWriter.Flush();
                    xmlWriter.Close();
                }
            }
            catch (XmlException e)
            {
                Logger.Instance.Write("<E> XML Exception creating TV Source settings file");
                Logger.Instance.Write("E>" + e.Message);
                return (false);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("<E> IO Exception creating TV Source settings file");
                Logger.Instance.Write("E>" + e.Message);
                return (false);
            }

            return (true);
        }

        internal void Clear()
        {
            DVBLinkElement channelsElement = DVBLinkBaseNode.FindElement(SettingsNode, new string[] { "TVSourceSettings", "Channels" });
            if (channelsElement != null)
                channelsElement.Elements = null;         
        }
    }
}
