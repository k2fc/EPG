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
using System.Runtime.InteropServices;
using System.Text;
using System.Net;
using System.IO;
using System.Xml;

using DVBViewerServer;

namespace DomainObjects
{
    /// <summary>
    /// The DVBViewer output class.
    /// </summary>
    public class OutputFileDVBViewer
    {
        private static WebRequest webRequest;

        /// <summary>
        /// Create the DVBViewer EPG entries.
        /// </summary>
        /// <returns>Null if the process succeeeded; an error message otherwise.</returns>
        public static string Process()
        {
            if (OptionEntry.IsDefined(OptionName.DvbViewerImport))
                return (processDVBViewer());
            else
                return (processRecordingService());
        }

        private static string processDVBViewer()
        {
            Logger.Instance.Write("Importing data to DVBViewer");

            DVBViewer dvbViewer;

            try
            {
                dvbViewer = (DVBViewer)Marshal.GetActiveObject("DVBViewerServer.DVBViewer");
            }
            catch (COMException)
            {
                return ("DVBViewer COM server not available");
            }

            IEPGManager epgManager = dvbViewer.EPGManager;
            if (epgManager == null)
                return ("Cannot get IEPGManager interface");

            IEPGAddBuffer epgAddBuffer = epgManager.AddEPG();
            int epgCount = 0;

            foreach (TVStation tvStation in RunParameters.Instance.StationCollection)
            {
                if (tvStation.Included && tvStation.EPGCollection.Count != 0)
                    epgCount+= processStationEPG(epgAddBuffer, tvStation);
            }

            Logger.Instance.Write("DVBViewer import finished - entries imported = " + epgCount);

            return (null);
        }

        private static int processStationEPG(IEPGAddBuffer epgAddBuffer, TVStation tvStation)
        {
            int epgCount = 0;

            foreach (EPGEntry epgEntry in tvStation.EPGCollection)
            {
                IEPGItem newItem = epgAddBuffer.NewItem();
                newItem.SetEPGEventID(epgEntry.ServiceID, epgEntry.TransportStreamID);
                newItem.EventID = epgEntry.EventID;
                
                if (epgEntry.EventName != null)
                    newItem.Title = epgEntry.EventName;
                else
                    newItem.Title = "No Title";

                if (epgEntry.ShortDescription != null)
                    newItem.Event = epgEntry.ShortDescription;
                else
                {
                    if (epgEntry.EventName != null)
                        newItem.Event = epgEntry.EventName;
                    else
                        newItem.Event = "No Description";
                }
                
                newItem.Description = string.Empty;
                newItem.Time = epgEntry.StartTime;
                newItem.Duration = new DateTime(1899, 12, 30) + epgEntry.Duration; 

                if (epgEntry.EventCategory != null)
                {
                    try
                    {
                        newItem.Content = Int32.Parse(epgEntry.EventCategory.Trim());
                    }
                    catch (FormatException)
                    {
                        newItem.Content = 0;
                    }
                    catch (OverflowException)
                    {
                        newItem.Content = 0;
                    }
                }
                else
                    newItem.Content = 0;                
                                          
                epgAddBuffer.Add(newItem);

                epgCount++;
            }

            epgAddBuffer.Commit();

            return (epgCount);
        }

        private static string processRecordingService()
        {
            Logger.Instance.Write("Importing data to DVBViewer Recording Service");

            string port = "8089";

            OptionEntry optionEntry = OptionEntry.FindEntry(OptionName.DvbViewerRecSvcImport, true);
            if (optionEntry != null)
                port = optionEntry.Parameter.ToString();

            if (RunParameters.Instance.DVBViewerIPAddress == null)
                return(processIPAddress("127.0.0.1", port, null, null));
            else
            {
                string currentIPAddress;
                string currentUserName;
                string currentPassword;

                string[] ipAddresses = RunParameters.Instance.DVBViewerIPAddress.Split(new char[] { ';' });

                foreach (string ipAddress in ipAddresses)
                {
                    currentUserName = null;
                    currentPassword = null;

                    int index1 = ipAddress.IndexOf('(');
                    if (index1 == -1)
                        currentIPAddress = ipAddress.Trim();
                    else
                    {
                        int index2 = ipAddress.IndexOf(')', index1);
                        if (index2 == -1)
                            currentIPAddress = ipAddress.Trim();
                        else
                        {
                            currentIPAddress = ipAddress.Substring(0, index1);

                            string namePasswordSpec = ipAddress.Substring(index1 + 1, index2 - index1 - 1);

                            string[] namePassword = namePasswordSpec.Split(new char[] { '/' });
                            if (namePassword.Length == 2)
                            {
                                currentUserName = namePassword[0];
                                currentPassword = namePassword[1];
                            }
                        }
                    }

                    string reply = processIPAddress(currentIPAddress, port, currentUserName, currentPassword);
                    if (reply != null)
                        return (reply);
                }
            }


            return (null);
        }

        private static string processIPAddress(string ipAddress, string port, string userName, string password)
        {
            string destination = @"http://" + ipAddress + ":" + port;
            
            if (userName == null)
                Logger.Instance.Write("Recording Service address is " + destination);
            else
                Logger.Instance.Write("Recording Service address is " + destination + " (username: " + userName + " password: " + password + ")");

            if (OptionEntry.IsDefined(OptionName.DvbViewerClear))
            {
                Logger.Instance.Write("Clearing DVBViewer EPG data");
                string clearResponse = sendGetRequest(destination + @"/index.html?epg_clear=true", userName, password);
                if (clearResponse != null)
                {
                    Logger.Instance.Write("<e> Failed to clear recording service data - output abandoned");
                    Logger.Instance.Write(clearResponse);
                    return (clearResponse);
                }
            }

            Logger.Instance.Write("DVBViewer Recording Service import starting");

            XmlWriterSettings settings = new XmlWriterSettings();

            settings.Indent = true;
            settings.NewLineOnAttributes = false;
            settings.Encoding = new UTF8Encoding(false);
            settings.CloseOutput = false;

            MemoryStream memoryStream = new MemoryStream();
            int epgCount = 0;

            try
            {
                using (XmlWriter xmlWriter = XmlWriter.Create(memoryStream, settings))
                {
                    xmlWriter.WriteStartDocument();
                    xmlWriter.WriteStartElement("epg");
                    xmlWriter.WriteAttributeString("Ver", "1");

                    foreach (TVStation tvStation in RunParameters.Instance.StationCollection)
                    {
                        if (tvStation.Included && tvStation.EPGCollection.Count != 0)
                            epgCount+= processStationEPG(xmlWriter, tvStation);
                    }

                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndDocument();

                    xmlWriter.Flush();
                    xmlWriter.Close();
                }
            }
            catch (XmlException ex1)
            {
                return (ex1.Message);
            }
            catch (IOException ex2)
            {
                return (ex2.Message);
            }

            string sendResponse = sendPostRequest(memoryStream, destination + "/cgi-bin/EPGimport", userName, password);
            if (sendResponse != null)
                return (sendResponse);

            Logger.Instance.Write("DVBViewer Recording Service import finished - entries imported = " + epgCount);
            return (null);
        }

        private static int processStationEPG(XmlWriter xmlWriter, TVStation tvStation)
        {
            int epgCount = 0;

            foreach (EPGEntry epgEntry in tvStation.EPGCollection)
            {
                xmlWriter.WriteStartElement("programme");

                xmlWriter.WriteAttributeString("start", epgEntry.StartTime.ToString("yyyyMMddHHmmss").Replace(":", ""));
                xmlWriter.WriteAttributeString("stop", (epgEntry.StartTime + epgEntry.Duration).ToString("yyyyMMddHHmmss").Replace(":", ""));
                xmlWriter.WriteAttributeString("channel", ((tvStation.TransportStreamID << 16) + tvStation.ServiceID).ToString());

                xmlWriter.WriteElementString("eventid", epgEntry.EventID.ToString());

                if (epgEntry.EventCategory != null)
                {
                    try
                    {
                        string[] parts = epgEntry.EventCategory.Split(new char[] { ',' });

                        string category;

                        if (parts.Length == 1)
                        {
                            Int32 digit1 = Int32.Parse(parts[0]);
                            category = (digit1 * 16).ToString();
                        }
                        else
                        {
                            if (parts.Length == 2)
                            {
                                Int32 digit1 = Int32.Parse(parts[0]);
                                Int32 digit2 = Int32.Parse(parts[1]);
                                category = ((digit1 * 16) + digit2).ToString();
                            }
                            else
                                category = "48";
                        }
                        
                        xmlWriter.WriteElementString("content", category);
                    }
                    catch (FormatException) 
                    {
                        xmlWriter.WriteElementString("content", "48");
                    }
                    catch (OverflowException) 
                    {
                        xmlWriter.WriteElementString("content", "48");
                    }
                }
                else
                    xmlWriter.WriteElementString("content", "48");
                
                xmlWriter.WriteElementString("charset", "255");

                xmlWriter.WriteStartElement("titles");
                xmlWriter.WriteStartElement("title");
                if (epgEntry.EventName != null)
                    xmlWriter.WriteValue(epgEntry.EventName);
                else
                    xmlWriter.WriteValue("No Title");
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();

                if (!OptionEntry.IsDefined(OptionName.DvbViewerSubtitleVisible))
                {
                    xmlWriter.WriteStartElement("events");
                    xmlWriter.WriteStartElement("event");

                    if (epgEntry.ShortDescription != null)
                        xmlWriter.WriteValue(epgEntry.ShortDescription);
                    else
                    {
                        if (epgEntry.EventName != null)
                            xmlWriter.WriteValue(epgEntry.EventName);
                        else
                            xmlWriter.WriteValue("No Description");
                    }

                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                }
                else
                {
                    if (epgEntry.EventSubTitle != null)
                    {
                        xmlWriter.WriteStartElement("events");
                        xmlWriter.WriteStartElement("event");
                        xmlWriter.WriteValue(epgEntry.EventSubTitle);
                        xmlWriter.WriteEndElement();
                        xmlWriter.WriteEndElement();
                    }

                    xmlWriter.WriteStartElement("descriptions");
                    xmlWriter.WriteStartElement("description");

                    if (epgEntry.ShortDescription != null)
                        xmlWriter.WriteValue(epgEntry.ShortDescription);
                    else
                    {
                        if (epgEntry.EventName != null)
                            xmlWriter.WriteValue(epgEntry.EventName);
                        else
                            xmlWriter.WriteValue("No Description");
                    }

                    xmlWriter.WriteEndElement();
                    xmlWriter.WriteEndElement();
                }

                bool creditsWritten = writeCredits(xmlWriter, epgEntry.Directors, "director", false);
                creditsWritten = writeCredits(xmlWriter, epgEntry.Producers, "producer", creditsWritten);
                creditsWritten = writeCredits(xmlWriter, epgEntry.Writers, "writer", creditsWritten);
                creditsWritten = writeCredits(xmlWriter, epgEntry.Cast, "actor", creditsWritten);
                creditsWritten = writeCredits(xmlWriter, epgEntry.GuestStars, "guest", creditsWritten);
                if (creditsWritten)
                    xmlWriter.WriteEndElement();

                xmlWriter.WriteEndElement();

                epgCount++;

                if (DebugEntry.IsDefined(DebugName.LogDvbViewerImport))
                {
                    Logger.Instance.Write("Station=" + tvStation.Name +
                        " ID=" + epgEntry.EventID.ToString() +
                        " Title=" + epgEntry.EventName +
                        " Start=" + epgEntry.StartTime +
                        " Desc=" + epgEntry.ShortDescription);
                }
            }

            return (epgCount);
        }

        private static bool writeCredits(XmlWriter xmlWriter, Collection<string> credits, string name, bool creditsWritten)
        {
            if (credits == null || credits.Count == 0)
                return (creditsWritten);

            bool written = creditsWritten;

            foreach (string credit in credits)
            {
                if (credit.Trim() != string.Empty)
                {
                    if (!written)
                    {
                        xmlWriter.WriteStartElement("credits");
                        written = true;
                    }

                    xmlWriter.WriteElementString(name, credit.Trim());
                }
            }

            return (written);
        }

        private static string sendGetRequest(string httpRequest, string userName, string password)
        {
            HttpWebResponse webResponse = null;

            try
            {
                webRequest = WebRequest.Create(httpRequest);
                webRequest.ContentType = "text/html";
                webRequest.Timeout = 180000;
                ((HttpWebRequest)webRequest).UserAgent = "HBS";

                ((HttpWebRequest)webRequest).Credentials = getCredentials(userName, password);

                webResponse = (HttpWebResponse)webRequest.GetResponse();

                Stream receiveStream = webResponse.GetResponseStream();
                Encoding encode = System.Text.Encoding.GetEncoding("utf-8");

                StreamReader readStream = new StreamReader(receiveStream, encode);
                string buffer = readStream.ReadToEnd();

                readStream.Close();
                webResponse.Close();

                return (null);
            }
            catch (Exception e)
            {
                Logger.Instance.Write("An exception of type " + e.GetType().Name + " has occurred while communicating with the Recording Service");
                Logger.Instance.Write("<e> " + e.Message);

                if (webResponse != null)
                    webResponse.Close();

                return (e.Message);
            }

        }

        private static string sendPostRequest(MemoryStream memoryStream, string httpRequest, string username, string password)
        {
            HttpWebResponse webResponse = null;

            try
            {
                WebRequest webRequest = WebRequest.Create(httpRequest);
                webRequest.Method = "POST";
                webRequest.ContentType = "text/html";
                webRequest.ContentLength = memoryStream.Length;
                webRequest.Timeout = 180000;
                ((HttpWebRequest)webRequest).UserAgent = "HBS";

                ((HttpWebRequest)webRequest).Credentials = getCredentials(username, password);

                byte[] dataBuffer = new byte[memoryStream.Length];
                memoryStream.Seek(0, SeekOrigin.Begin);
                memoryStream.Read(dataBuffer, 0, (int)memoryStream.Length);

                Stream stream = webRequest.GetRequestStream();
                stream.Write(dataBuffer, 0, dataBuffer.Length);
                stream.Close();
                memoryStream.Close();

                webResponse = (HttpWebResponse)webRequest.GetResponse();

                Stream receiveStream = webResponse.GetResponseStream();
                Encoding encode = System.Text.Encoding.GetEncoding("utf-8");

                StreamReader readStream = new StreamReader(receiveStream, encode);
                string buffer = readStream.ReadToEnd();

                readStream.Close();
                webResponse.Close();

                return (null);
            }
            catch (Exception e)
            {
                Logger.Instance.Write("<e> An exception of type " + e.GetType().Name + " has occurred while communicating with the Recording Service");
                
                if (webResponse != null)
                    webResponse.Close();

                return (e.Message);
            }

        }

        private static NetworkCredential getCredentials(string userName, string password)
        {
            if (userName != null && password != null)
                return (new NetworkCredential(userName, password));
            else
                return (null);
        }
    }
}
