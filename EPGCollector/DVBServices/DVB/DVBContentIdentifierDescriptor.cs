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

namespace DVBServices
{
    /// <summary>
    /// DVB Content Identifier descriptor class.
    /// </summary>
    internal class DVBContentIdentifierDescriptor : DescriptorBase
    {
        /// <summary>
        /// Get the list of identifiers.
        /// </summary>
        public Collection<ContentIdentifier> ContentIdentifiers { get { return (contentIdentifiers); } }

        /// <summary>
        /// Return true if there is a series link content identifier present; false otherwise.
        /// </summary>
        public bool HasSeriesLink
        {
            get
            {
                if (contentIdentifiers == null)
                    return (false);

                foreach (ContentIdentifier contentIdentifier in contentIdentifiers)
                {
                    if (contentIdentifier.IsSeriesLink)
                        return (true);
                }

                return (false);
            }
        }

        /// <summary>
        /// Get the series link identifier; returns null if not present;
        /// </summary>
        public string SeriesLink
        {
            get
            {
                if (contentIdentifiers == null)
                    return (null);

                foreach (ContentIdentifier contentIdentifier in contentIdentifiers)
                {
                    if (contentIdentifier.IsSeriesLink)
                        return (contentIdentifier.Identifier);
                }

                return (null);
            }
        }

        /// <summary>
        /// Return true if there is an episode link content identifier present; false otherwise.
        /// </summary>
        public bool HasEpisodeLink
        {
            get
            {
                if (contentIdentifiers == null)
                    return (false);

                foreach (ContentIdentifier contentIdentifier in contentIdentifiers)
                {
                    if (contentIdentifier.IsEpisodeLink)
                        return (true);
                }

                return (false);
            }
        }

        /// <summary>
        /// Get the episode link identifier; returns null if not present;
        /// </summary>
        public string EpisodeLink
        {
            get
            {
                if (contentIdentifiers == null)
                    return (null);

                foreach (ContentIdentifier contentIdentifier in contentIdentifiers)
                {
                    if (contentIdentifier.IsEpisodeLink)
                        return (contentIdentifier.Identifier);
                }

                return (null);
            }
        }


        /// <summary>
        /// Get the index of the next byte in the section following this descriptor.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The descriptor has not been processed.
        /// </exception> 
        public override int Index
        {
            get
            {
                if (lastIndex == -1)
                    throw (new InvalidOperationException("DVBContentIdentifierDescriptor: Index requested before block processed"));
                return (lastIndex);
            }
        }

        private Collection<ContentIdentifier> contentIdentifiers; 

        private int lastIndex = -1;

        /// <summary>
        /// Initialize a new instance of the DVBContentIdentifierDescriptor class.
        /// </summary>
        internal DVBContentIdentifierDescriptor() { }

        /// <summary>
        /// Parse the descriptor.
        /// </summary>
        /// <param name="byteData">The MPEG2 section containing the descriptor.</param>
        /// <param name="index">Index of the byte in the MPEG2 section following the descriptor length.</param>
        internal override void Process(byte[] byteData, int index)
        {
            lastIndex = index;

            contentIdentifiers = new Collection<ContentIdentifier>();
            int dataLength = Length;

            while (dataLength > 0)
            {
                try
                {
                    int contentType = (int)byteData[lastIndex] >> 2;
                    int contentLocation = (int)byteData[lastIndex] & 0x03;
                    lastIndex++;

                    string contentReference = null;

                    if (contentLocation == 0)
                    {
                        int contentReferenceLength = (int)byteData[lastIndex];
                        lastIndex++;

                        if (contentReferenceLength != 0)
                        {
                            contentReference = Utils.GetString(byteData, lastIndex, contentReferenceLength);
                            lastIndex += contentReferenceLength;
                        }

                        contentIdentifiers.Add(new ContentIdentifier(contentType, contentLocation, contentReference));
                        dataLength -= contentReferenceLength + 2;
                    }
                    else
                    {
                        int referenceNumber = Utils.Convert2BytesToInt(byteData, lastIndex);
                        lastIndex += 2;

                        contentIdentifiers.Add(new ContentIdentifier(contentType, contentLocation, referenceNumber.ToString()));
                        dataLength -= 3;
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    throw (new ArgumentOutOfRangeException("The DVB Content Identifier Descriptor message is short"));
                }
            }

            Validate();
        }

        /// <summary>
        /// Validate the descriptor fields.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// A descriptor field is not valid.
        /// </exception>
        internal override void Validate() { }

        /// <summary>
        /// Log the descriptor fields.
        /// </summary>
        internal override void LogMessage()
        {
            if (Logger.ProtocolLogger == null)
                return;

            if (contentIdentifiers == null || contentIdentifiers.Count == 0)
            {
                Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "DVB CONTENT IDENTIFIER DESCRIPTOR: No identifiers present");
                return;
            }

            string leadIn = "DVB CONTENT IDENTIFIER DESCRIPTOR: Type: ";

            foreach (ContentIdentifier contentIdentifier in contentIdentifiers)
            {
                string referenceString;

                if (contentIdentifier.Identifier != null)
                    referenceString = contentIdentifier.Identifier;
                else
                    referenceString = "** Not Available **";

                Logger.ProtocolLogger.Write(Logger.ProtocolIndent + leadIn + contentIdentifier.Type +
                    " Location: " + contentIdentifier.Location +
                    " Reference: " + referenceString);

                leadIn = "    Type: ";
            }
        }
    }
}
