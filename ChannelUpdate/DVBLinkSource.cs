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
    internal class DVBLinkSource
    {
        internal Collection<DVBLinkHeadEnd> HeadEnds { get; private set; }

        internal string ElementName { get; private set; }

        /// <summary>
        /// Common properties
        /// </summary>
        internal string GUID { get; private set; }        
        internal string Template { get; private set; }
        internal string Type { get; private set; }
        internal string SourceControl { get; private set; }
        internal string EPGScanRepeatDelay { get; private set; }

        /// <summary>
        /// v4.1 properties
        /// </summary>
        internal string ColorID { get; private set; }        
        internal string StartAsService { get; private set; }

        /// <summary>
        /// v4.5 properties
        /// </summary>
        internal string ProductID { get; private set; }
        internal Collection<DVBLinkSourceInstance> Instances { get; private set; }

        internal string LinkID
        {
            get
            {
                if (ColorID != null)
                    return (GUID);
                else
                {
                    if (Instances != null)
                        return (Instances[0].TSControl);
                    else
                        return (null);
                }
            }
        }

        internal string EPGID
        {
            get
            {
                if (Instances != null)
                    return (Instances[0].EPGControl);
                else
                    return (null);
            }
        }

        internal static string SourceVersion { get; set; }

        internal string NormalizedName
        {
            get
            {
                if (ElementName == null)
                    return (null);
                else
                    return (NormalizeName(ElementName));
            }
        }

        internal bool IsTunable { get { return (Type == "3"); } }

        internal DVBLinkSource() { }

        internal bool Load(DVBLinkElement sourceElement)
        {
            if (sourceElement.Elements == null)
                return (false);

            ElementName = sourceElement.Name;

            try
            {
                ColorID = sourceElement.GetElementValue("color_id");
                GUID = sourceElement.GetElementValue("guid");
                SourceControl = sourceElement.GetElementValue("source_control");
                StartAsService = sourceElement.GetElementValue("start_as_service");
                Template = sourceElement.GetElementValue("template");
                Type = sourceElement.GetElementValue("type");
                EPGScanRepeatDelay = sourceElement.GetElementValue("EPGScanRepeatDelay");
                
                DVBLinkElement instancesElement = sourceElement.FindElement("instances");
                if (instancesElement == null || instancesElement.Elements == null)
                {
                    SourceVersion = "41";
                    return (true);
                }

                Instances = new Collection<DVBLinkSourceInstance>();

                foreach (DVBLinkElement instanceElement in instancesElement.Elements)
                {
                    DVBLinkSourceInstance sourceInstance = new DVBLinkSourceInstance();
                    sourceInstance.Load(instanceElement);
                    Instances.Add(sourceInstance);
                }

                SourceVersion = "45";
                return (true);
            }
            catch (Exception e)
            {
                Logger.Instance.Write("<E> An exception of type " + e.GetType().Name + " has occurred while parsing the xml for a source");
                Logger.Instance.Write("<E> " + e.Message);

                return (false);
            }
        }

        internal static string NormalizeName(string name)
        {
            if (name == null)
                return (null);
            else
                return (name.Replace("dl_xmltag_", string.Empty).Replace("_20", " ").Replace("_5f", "_"));
        }

        internal bool LoadHeadEnds(DVBLinkBaseNode baseNode)
        {
            if (HeadEnds == null)
                HeadEnds = new Collection<DVBLinkHeadEnd>();

            DVBLinkElement headEndListElement = DVBLinkBaseNode.FindElement(baseNode, new string[] { "TVSourceSettings", "HeadendList" });
            if (headEndListElement == null || headEndListElement.Elements == null)
                return (false);

            foreach (DVBLinkElement headEndElement in headEndListElement.Elements)
            {
                DVBLinkHeadEnd newHeadEnd = new DVBLinkHeadEnd();
                if (newHeadEnd.Load(this, baseNode, headEndElement))
                    HeadEnds.Add(newHeadEnd);
            }

            return (true);
        }

        internal DVBLinkHeadEnd FindHeadEnd(string headEndID)
        {
            if (HeadEnds == null)
                return (null);

            foreach (DVBLinkHeadEnd headEnd in HeadEnds)
            {
                if (headEnd.HeadEndID == headEndID)
                    return (headEnd);
            }

            return (null);
        }

        /// <summary>
        /// Get the name of the source.
        /// </summary>
        /// <returns>The normalized name.</returns>
        public override string ToString()
        {
            return (NormalizedName);
        }
    }
}
