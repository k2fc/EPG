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

namespace DVBServices
{
    /// <summary>
    /// The reply codes from the protocol collectors.
    /// </summary>
    public enum CollectorReply
    {
        /// <summary>
        /// The collection was successful.
        /// </summary>
        OK,
        /// <summary>
        /// The collection failed.
        /// </summary>
        GeneralFailure,
        /// <summary>
        /// There was a format error in the received data.
        /// </summary>
        FatalFormatError,
        /// <summary>
        /// The was an error loading the reference data.
        /// </summary>
        ReferenceDataError,
        /// <summary>
        /// There was an error in the broadcast data.
        /// </summary>
        BroadcastDataError,
        /// <summary>
        /// The collection was cancelled.
        /// </summary>
        Cancelled
    }

    /// <summary>
    /// The current scope
    /// </summary>
    [Flags]
    public enum Scope
    {
        /// <summary>
        /// No scope restrictions
        /// </summary>
        All = 0x7fffffff,
        /// <summary>
        /// Program Map table
        /// </summary>
        PMT = 0x0002,
        /// <summary>
        /// Program Association table
        /// </summary>
        PAT = 0x0004,
        /// <summary>
        /// Network Information table
        /// </summary>
        NIT = 0x0008,
        /// <summary>
        /// Bouquet Association table
        /// </summary>        
        BAT = 0x0010,
        /// <summary>
        /// Service Description table
        /// </summary>
        SDT = 0x0020,
        /// <summary>
        /// Event Information table
        /// </summary>
        EIT = 0x0040,
        /// <summary>
        /// Time and Date table
        /// </summary>
        TDT = 0x0080,
        /// <summary>
        /// Time offset table
        /// </summary>
        TOT = 0x0100,
        /// <summary>
        /// Running Status table
        /// </summary>
        RST = 0x0200,
        /// <summary>
        /// Stuffing table
        /// </summary>
        ST = 0x0400,
        /// <summary>
        /// Discontinuity Information table
        /// </summary>
        DIT = 0x0800,
        /// <summary>
        /// Selection Information table
        /// </summary>
        SIT = 0x1000,
        /// <summary>
        /// Application Information table
        /// </summary>
        AIT = 0x2000,
        /// <summary>
        /// DSMCC sections
        /// </summary>
        DSMCC = 0x4000,
    }

    /// <summary>
    /// The action to take for non-Ascii characters in text strings.
    /// </summary>
    public enum ReplaceMode
    {
        /// <summary>
        /// The non-ASCII character is removed.
        /// </summary>
        Ignore,
        /// <summary>
        /// The non-ASCII character is set to space.
        /// </summary>
        SetToSpace,
        /// <summary>
        /// The non-ASCII character is not changed in the output text.
        /// </summary>
        TransferUnchanged,
        /// <summary>
        /// The non-ASCII character is converted to its ASCII equivalent (EIT 0x8a only at present)
        /// </summary>
        Convert,
        /// <summary>
        /// The non-ASCII character is converted using the byte conversion table)
        /// </summary>
        ConvertUsingTable
    }

    /// <summary>
    /// The data broadcast ID values.
    /// </summary>
    public enum DataBroadcastId
    {
        /// <summary>
        /// DVB data pipe.
        /// </summary>
        DvbDataPipe = 0x0001,                   // EN 301 192 section 4.2.1
        /// <summary>
        /// Asynchronous data stream
        /// </summary>
        AsyncDataStream = 0x0002,               // EN 301 192 section 5.2.1
        /// <summary>
        /// Synchronous data stream
        /// </summary>
        SynchronousDataStream = 0x0003,         // EN 301 192 section 6.2.1
        /// <summary>
        /// Synchronous data streams
        /// </summary>
        SynchronousDataStreams = 0x0004,        // EN 301 192 section 6.2.1
        /// <summary>
        /// Multiprotocol encapsulation
        /// </summary>
        MultiProtocolEncapsulation = 0x0005,    // EN 301 192 section 4.2.1
        /// <summary>
        /// Data carousel
        /// </summary>
        DataCarousel = 0x0006,
        /// <summary>
        /// Object carousel
        /// </summary>
        ObjectCarousel = 0x0007,
        /// <summary>
        /// DVB ATM stream
        /// </summary>
        DvbAtmStream = 0x0008,
        /// <summary>
        /// Higher protocol asynchronous stream
        /// </summary>
        HigherProtcolAsyncStream = 0x0009,
        /// <summary>
        /// System software update service
        /// </summary>
        SsuService = 0x000a,
        /// <summary>
        /// IP/MAC notification service
        /// </summary>
        IpMacNotificationService = 0x000b,
        /// <summary>
        /// MHP object carousel
        /// </summary>
        MhpObjectCarousel = 0x00f0,             // TS 101 812
        /// <summary>
        /// MHP multiprotocol encapsulation
        /// </summary>
        MhpMultiProtoclEncapsulation = 0x000f1, // TS 101 812
        /// <summary>
        /// MHP application presence
        /// </summary>
        MhpApplicationPresence = 0x00f2,
        /// <summary>
        /// MHEG5 stream
        /// </summary>
        Mheg5 = 0x0106
    }
}
