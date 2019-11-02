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
    /// The class that describes the stream information.
    /// </summary>
    public class StreamInfo
    {
        /// <summary>
        /// Get the stream type.
        /// </summary>
        public int StreamType { get { return (streamType); } }
        /// <summary>
        /// Get the elementary PID.
        /// </summary>
        public int ElementaryPid { get { return (elementaryPid); } }
        /// <summary>
        /// Get the collection of descriptors.
        /// </summary>
        internal Collection<DescriptorBase> Descriptors { get { return (descriptors); } }

        /// <summary>
        /// Get the index of the next byte in the MPEG2 section following the stream informationn.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The stream information has not been processed.
        /// </exception> 
        public int Index
        {
            get
            {
                if (lastIndex == -1)
                    throw (new InvalidOperationException("Stream Info: Index requested before block processed"));
                return (lastIndex);
            }
        }

        private int streamType;
        private int elementaryPid;
        private Collection<DescriptorBase> descriptors;

        private int lastIndex = -1;

        /// <summary>
        /// Initialize a new instance of the StreamInfo class.
        /// </summary>
        public StreamInfo() { }

        /// <summary>
        /// Parse the stream information.
        /// </summary>
        /// <param name="byteData">The MPEG2 section containing the stream information.</param>
        /// <param name="index">Index of the first byte of the stream information in the MPEG2 section.</param>
        internal void Process(byte[] byteData, int index)
        {
            lastIndex = index;

            try
            {
                streamType = (int)byteData[lastIndex];
                lastIndex ++;

                elementaryPid = Utils.Convert2BytesToInt(byteData, lastIndex, 0x1f);
                lastIndex += 2;

                int esInfoLength = Utils.Convert2BytesToInt(byteData, lastIndex, 0x0f);
                lastIndex += 2;

                if (esInfoLength != 0)
                {
                    descriptors = new Collection<DescriptorBase>();

                    while (esInfoLength > 0)
                    {
                        DescriptorBase descriptor = DescriptorBase.Instance(byteData, lastIndex, Scope.PMT);

                        if (!descriptor.IsEmpty)
                        {
                            descriptors.Add(descriptor);

                            lastIndex += descriptor.TotalLength;
                            esInfoLength -= descriptor.TotalLength;
                        }
                        else
                        {
                            lastIndex += DescriptorBase.MinimumDescriptorLength;
                            esInfoLength -= DescriptorBase.MinimumDescriptorLength;
                        }
                    }
                }

                Validate();
            }
            catch (IndexOutOfRangeException)
            {
                throw (new ArgumentOutOfRangeException("The DVB Stream Info message is short"));
            }
        }

        /// <summary>
        /// Validate the stream information fields.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// A stream information field is not valid.
        /// </exception>
        public void Validate() { }

        /// <summary>
        /// Log the stream Information fields.
        /// </summary>
        public void LogMessage() 
        {
            if (Logger.ProtocolLogger == null)
                return;

            Logger.ProtocolLogger.Write(Logger.ProtocolIndent + "DVB STREAM INFO:" +
                " Type: " + StreamType +
                " Elem PID: " + ElementaryPid);

            if (Descriptors != null)
            {
                foreach (DescriptorBase descriptor in Descriptors)
                {
                    Logger.IncrementProtocolIndent();
                    descriptor.LogMessage();
                    Logger.DecrementProtocolIndent();
                }
            }
        }
    }
}
