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

using DomainObjects;

namespace ChannelUpdate
{
    internal class DVBLinkSettings
    {
        internal DVBLinkSettingsConfigurationNode ConfigurationNode { get; private set; }

        internal DVBLinkSettings() { }

        internal bool Load()
        {
            XmlReader xmlReader = null;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.IgnoreWhitespace = true;

            string fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), 
                Path.Combine(new string[] { "DVBLogic", "dvblink", "dvblink_settings.xml" } ));

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

            ConfigurationNode = new DVBLinkSettingsConfigurationNode();
            bool reply = ConfigurationNode.Load(xmlReader);

            if (xmlReader != null)
                xmlReader.Close();

            return (reply);
        }
    }
}
