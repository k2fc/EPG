﻿////////////////////////////////////////////////////////////////////////////////// 
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

using System.IO;
using System.Text;
using System.Xml;

namespace DomainObjects
{
    internal sealed class OutputFileAreaRegionChannels
    {
        private static string actualFileName;

        private OutputFileAreaRegionChannels() { }

        internal static void Process(string fileName)
        {
            if (RunParameters.Instance.AreaRegionFileName != null)
                actualFileName = RunParameters.Instance.AreaRegionFileName;
            else
                actualFileName = Path.Combine(Path.GetDirectoryName(fileName), "AreaRegionChannelInfo.xml");

            try
            {
                Logger.Instance.Write("Deleting any existing version of Area/Region channel file");
                File.SetAttributes(actualFileName, FileAttributes.Normal);
                File.Delete(actualFileName);
            }
            catch (IOException e)
            {
                Logger.Instance.Write("File delete exception: " + e.Message);
            }

            Logger.Instance.Write("Creating Area/Region channel file: " + actualFileName);

            XmlWriterSettings settings = new XmlWriterSettings();

            settings.Indent = true;
            settings.NewLineOnAttributes = true;
            settings.Encoding = new UTF8Encoding(false);
            settings.CloseOutput = true;

            using (XmlWriter xmlWriter = XmlWriter.Create(actualFileName, settings))
            {
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("areas");

                foreach (Bouquet bouquet in Bouquet.Bouquets)
                {
                    bool writeStart = true;

                    foreach (Region region in bouquet.Regions)
                    {
                        bool include = checkArea(bouquet.BouquetID, region.Code);
                        if (include)
                        {
                            if (writeStart)
                            {
                                xmlWriter.WriteStartElement("area");
                                xmlWriter.WriteAttributeString("id", bouquet.BouquetID.ToString());
                                xmlWriter.WriteAttributeString("name", string.IsNullOrWhiteSpace(bouquet.Name) ? "** Not Present **" : bouquet.Name);
                                writeStart = false;
                            }

                            xmlWriter.WriteStartElement("region");
                            xmlWriter.WriteAttributeString("id", region.Code.ToString());
                            xmlWriter.WriteAttributeString("name", string.IsNullOrWhiteSpace(region.Name) ? "** Not Present **" : region.Name);
                            
                            foreach (Channel channel in region.GetChannelsInChannelNumberOrder())
                            {
                                {
                                    TVStation station = TVStation.FindStation(RunParameters.Instance.StationCollection, 
                                        channel.OriginalNetworkID, channel.TransportStreamID, channel.ServiceID);
                                    if (station != null)
                                    {
                                        xmlWriter.WriteStartElement("channel");

                                        xmlWriter.WriteAttributeString("id", channel.UserChannel.ToString());
                                        xmlWriter.WriteAttributeString("nid", channel.OriginalNetworkID.ToString());
                                        xmlWriter.WriteAttributeString("tid", channel.TransportStreamID.ToString());
                                        xmlWriter.WriteAttributeString("sid", channel.ServiceID.ToString());

                                        if (station.NewName == null)
                                            xmlWriter.WriteAttributeString("name", station.Name);
                                        else
                                            xmlWriter.WriteAttributeString("name", station.NewName);

                                        xmlWriter.WriteEndElement();
                                    }
                                }
                            }

                            xmlWriter.WriteEndElement();
                        }
                    }

                    if (!writeStart)
                        xmlWriter.WriteEndElement();
                }

                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();

                xmlWriter.Flush();
                xmlWriter.Close();
            }
        }

        private static bool checkArea(int bouquet, int region)
        {
            foreach (TuningFrequency tuningFrequency in RunParameters.Instance.FrequencyCollection)
            {
                if (tuningFrequency.AdvancedRunParamters != null)
                {
                    if (tuningFrequency.AdvancedRunParamters.ChannelBouquet == -1)
                        return (true);
                    else
                    {
                        if (bouquet != tuningFrequency.AdvancedRunParamters.ChannelBouquet)
                            return (false);

                        if (tuningFrequency.AdvancedRunParamters.ChannelRegion == -1 || region == 65535)
                            return (true);
                            
                        return (region == tuningFrequency.AdvancedRunParamters.ChannelRegion);
                    }
                }
            }
            
            return (false);
        }
    }
}

