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

using DomainObjects;

namespace DVBServices
{
    /// <summary>
    /// The base descriptor class.
    /// </summary>
    internal class DescriptorBase
    {
        /// <summary>
        /// Get the table ID containing the descriptor data (Dish/Bell descriptors only).
        /// </summary>
        internal int Table { get { return (table); } }
        /// <summary>
        /// Get the tag of the record.
        /// </summary>
        internal int Tag { get { return (tag); } }
        /// <summary>
        /// Get the length of the descriptor data.
        /// </summary>
        internal int Length { get { return (length); } }
        /// <summary>
        /// Get the total length of the descriptor.
        /// </summary>
        internal int TotalLength { get { return (Length + 2); } }
        /// <summary>
        /// Get the record data.
        /// </summary>
        internal byte[] Data { get { return (data); } }

        /// <summary>
        /// Return true if the descriptor is undefined; false otherwise.
        /// </summary>
        internal bool IsUndefined { get { return (isUndefined); } }

        /// <summary>
        /// Return true if the descriptor is empty; false otherwise.
        /// </summary>
        internal bool IsEmpty { get { return (Length == 0); } }

        /// <summary>
        /// Return the minimum descriptor length in bytes.
        /// </summary>
        internal static int MinimumDescriptorLength { get { return (2); } }

        /// <summary>
        /// Get the index of the next byte in the MPEG2 section following this descriptor.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// The descriptor has not been processed.
        /// </exception> 
        public virtual int Index
        {
            get
            {
                if (lastIndex == -1)
                    throw (new InvalidOperationException("DescriptorBase: Index requested before block processed"));
                return (lastIndex);
            }
        }

        private int lastIndex = -1;

        // MHP descriptors

        internal const int ApplicationDescriptorTag = 0x00;
        internal const int ApplicationNameDescriptorTag = 0x01;
        internal const int TransportProtocolDescriptorTag = 0x02;
        internal const int JavaApplicationDescriptorTag = 0x03;
        internal const int JavaApplicationLocationDescriptorTag = 0x04;
        internal const int ExternalApplicationDescriptorTag = 0x05;
        internal const int ApplicationRecordingDescriptorTag = 0x06;
        internal const int HtmlApplicationDescriptorTag = 0x08;
        internal const int HtmlApplicationLocationDescriptorTag = 0x09; // Same as CA descriptor - scope used to create
        internal const int HtmlApplicationBoundaryDescriptorTag = 0x0a;
        internal const int ApplicationIconsDescriptorTag = 0x0b;
        internal const int PrefetchDescriptorTag = 0x0c;
        internal const int DIILocationDescriptorTag = 0x0d;
        internal const int ApplicationStorageDescriptorTag = 0x10;
        internal const int IPSignallingDescriptorTag = 0x11;
        internal const int GraphicsConstraintDescriptorTag = 0x14;
        internal const int SimpleApplicationLocationDescriptorTag = 0x15;
        internal const int ApplicationUsageDescriptorTag = 0x16;
        internal const int SimpleApplicationBoundaryDescriptorTag = 0x17;

        // Standard descriptors

        internal const int DescriptorTag_0x09 = 0x09;

        internal const int MetaDataPointerDescriptorTag = 0x25;
        internal const int MetaDataDescriptorTag = 0x26;

        internal const int NetworkNameDescriptorTag = 0x40;
        internal const int ServiceListDescriptorTag = 0x41;
        internal const int StuffingDescriptorTag = 0x42;
        internal const int SatelliteDeliverySystemDescriptorTag = 0x43;
        internal const int CableDeliverySystemDescriptorTag = 0x44;
        internal const int VbiDataDescriptorTag = 0x45;
        internal const int VbiTeletextDescriptorTag = 0x46;
        internal const int BouquetNameDescriptorTag = 0x47;
        internal const int ServiceDescriptorTag = 0x48;
        internal const int CountryAvailabilityDescriptorTag = 0x49;
        internal const int LinkageDescriptorTag = 0x4a;
        internal const int NvodReferenceDescriptorTag = 0x4b;
        internal const int TimeShiftedServiceDescriptorTag = 0x4c;
        internal const int ShortEventDescriptorTag = 0x4d;
        internal const int ExtendedEventDescriptorTag = 0x4e;
        internal const int TimeShiftedEventDescriptorTag = 0x4f;
        internal const int ComponentDescriptorTag = 0x50;
        internal const int MosaicDescriptorTag = 0x51;
        internal const int StreamIdentifierDescriptorTag = 0x52;
        internal const int CaIdentifierDescriptorTag = 0x53;
        internal const int ContentDescriptorTag = 0x54;
        internal const int ParentalRatingDescriptorTag = 0x55;
        internal const int TeletextDescriptorTag = 0x56;
        internal const int TelephoneDescriptorTag = 0x57;
        internal const int LocalTimeOffsetDescriptorTag = 0x58;
        internal const int SubtitlingDescriptorTag = 0x59;
        internal const int TerrestrialDeliverySystemDescriptorTag = 0x5a;
        internal const int MultilingualNetworkNameDescriptorTag = 0x5b;
        internal const int MultilingualBouquetNameDescriptorTag = 0x5c;
        internal const int MultilingualServiceNameDescriptorTag = 0x5d;
        internal const int MultilingualComponentDescriptorTag = 0x5e;
        internal const int PrivateDataSpecifierDescriptorTag = 0x5f;
        internal const int ServiceMoveDescriptorTag = 0x60;
        internal const int ShortSmoothingBufferDescriptorTag = 0x61;
        internal const int FrequenctListDescriptorTag = 0x62;
        internal const int PartialTransportStreamDescriptorTag = 0x63;
        internal const int DataBroadcastDescriptorTag = 0x64;
        internal const int ScramblingDescriptorTag = 0x65;
        internal const int DataBroadcastIDDescriptorTag = 0x66;
        internal const int TransportStreamDescriptorTag = 0x67;
        internal const int DsngDescriptorTag = 0x68;
        internal const int PdcDescriptorTag = 0x69;
        internal const int Ac3DescriptorTag = 0x6a;
        internal const int AncillaryDataDescriptorTag = 0x6b;
        internal const int CellListDescriptorTag = 0x6c;
        internal const int CellFrequencyLinkDescriptorTag = 0x6d;
        internal const int AnnouncementSupportDescriptorTag = 0x6e;
        internal const int ApplicationSignallingDescriptorTag = 0x6f;
        internal const int AdaptionFieldDataDescriptorTag = 0x70;
        internal const int ServiceIdentifierDescriptorTag = 0x71;
        internal const int ServiceAvailabilityDescriptorTag = 0x72;
        internal const int DefaultAuthorityDescriptorTag = 0x73;
        internal const int RelatedContentDescriptorTag = 0x74;
        internal const int TvaIDDescriptorTag = 0x75;
        internal const int ContentIdentifierDescriptorTag = 0x76;
        internal const int TimeSliceFECIdentifierDescriptorTag = 0x77;
        internal const int EcmRepetionRateDescriptorTag = 0x78;
        internal const int S2SatelliteDeliverySystemDescriptorTag = 0x79;
        internal const int EnhancedAC3DescriptorTag = 0x7a;
        internal const int DtsDescriptorTag = 0x7b;
        internal const int AacDescriptorTag = 0x7c;
        internal const int XaitLocationDescriptorTag = 0x7d;
        internal const int FtaContentManagementDescriptorTag = 0x7e;
        internal const int ExtensionDescriptorTag = 0x7f;
        internal const int UserDefinedDescriptorTag = 0x80;

        // Channel info descriptors - only valid if scope is Bouquet

        internal const int TurkeyChannelInfoDescriptorTag = 0x81;
        internal const int FreeviewChannelInfoDescriptorTag = 0x83;
        internal const int GenericChannelInfoDescriptorTag = 0x93;
        internal const int OpenTVChannelInfoDescriptorTag = 0xb1;        
        internal const int E2ChannelInfoDescriptorTag = 0xe2;

        /// <summary>
        /// OpenTV specific descriptors - only valid if collection type is OpenTV
        /// </summary>
        internal const int OpenTVChannelGroupDescriptorTag = 0xb2;  

        // FreeSat specific descriptors - only valid if collection type is FreeSat

        internal const int FreeSatChannelInfoDescriptorTag = 0xd3;
        internal const int FreeSatRegionDescriptorTag = 0xd4;
        internal const int FreeSatCategoryDescriptorTag = 0xd8;
        internal const int FreeSatImageDescriptorTag = 0xdf;

        // ATSC PSIP descriptors

        internal const int AtscAC3AudioDescriptorTag = 0x81;
        internal const int AtscCaptionServiceDescriptorTag = 0x86;
        internal const int AtscContentAdvisoryDescriptorTag = 0x87;
        internal const int AtscExtendedChannelNameDescriptorTag = 0xa0;
        internal const int AtscServiceLocationDescriptorTag = 0xa1;
        internal const int AtscGenreDescriptorTag = 0xab;

        // Dish Network descriptors 

        internal const int DishNetworkRatingDescriptorTag = 0x89;
        internal const int DishNetworkShortEventDescriptorTag = 0x91;
        internal const int DishNetworkExtendedEventDescriptorTag = 0x92;
        internal const int DishNetworkSupplementaryDescriptorTag = 0x94;
        internal const int DishNetworkVCHIPDescriptorTag = 0x95;
        internal const int DishNetworkSeriesDescriptorTag = 0x96;
        internal const int DishNetworkEPGLinkDescriptorTag = 0x9e;

        // Bell TV descriptors 

        internal const int BellTVRatingDescriptorTag = 0x89;        
        internal const int BellTVSeriesDescriptorTag = 0x96;

        // Extension descriptor values

        internal const int T2DeliverySystemDescriptorTag = 0x04;

        // UK cable channel descriptor

        internal const int UKCableChannelDescriptorTag = 0xca;

        private int table;
        private int tag;
        private int length;
        private byte[] data;

        private bool isUndefined;
        private static bool genericLoggingEnabled = true;

        /// <summary>
        /// Create an instance of the descriptor class.
        /// </summary>
        /// <param name="byteData">The MPEG2 section containing the descriptor.</param>
        /// <param name="index">The index of the tag byte of the descriptor.</param>
        /// <param name="scope">The current scope.</param>
        /// <returns>A descriptor instance.</returns>
        internal static DescriptorBase Instance(byte[] byteData, int index, Scope scope)
        {
            DescriptorBase descriptor = null;

            switch ((int)byteData[index])
            {
                case NetworkNameDescriptorTag:
                    if (inScope(scope, Scope.NIT))
                        descriptor = new DVBNetworkNameDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case SatelliteDeliverySystemDescriptorTag:
                    if (inScope(scope, Scope.NIT))
                        descriptor = new DVBSatelliteDeliverySystemDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case CableDeliverySystemDescriptorTag:
                    if (inScope(scope, Scope.NIT))
                        descriptor = new DVBCableDeliverySystemDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case ServiceDescriptorTag:
                    if (inScope(scope, Scope.SDT | Scope.SIT))
                        descriptor = new DVBServiceDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case LinkageDescriptorTag:
                    if (inScope(scope, Scope.NIT | Scope.BAT | Scope.SDT | Scope.EIT | Scope.SIT))
                        descriptor = new DVBLinkageDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case ServiceListDescriptorTag:
                    if (inScope(scope, Scope.NIT | Scope.BAT))
                        descriptor = new DVBServiceListDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case ShortEventDescriptorTag:
                    if (inScope(scope, Scope.EIT | Scope.SIT))
                        descriptor = new DVBShortEventDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case ExtendedEventDescriptorTag:
                    if (inScope(scope, Scope.EIT | Scope.SIT))
                        descriptor = new DVBExtendedEventDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case ComponentDescriptorTag:
                    if (inScope(scope, Scope.SDT | Scope.EIT | Scope.SIT))
                        descriptor = new DVBComponentDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case ContentDescriptorTag:
                    if (inScope(scope, Scope.EIT | Scope.SIT))
                        descriptor = new DVBContentDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case ParentalRatingDescriptorTag:
                    if (inScope(scope, Scope.EIT | Scope.SIT))
                        descriptor = new DVBParentalRatingDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;                
                case BouquetNameDescriptorTag:
                    if (inScope(scope, Scope.BAT | Scope.SDT | Scope.SIT))
                        descriptor = new DVBBouquetNameDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case LocalTimeOffsetDescriptorTag:
                    if (inScope(scope, Scope.TOT))
                        descriptor = new DVBLocalTimeOffsetDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case TerrestrialDeliverySystemDescriptorTag:
                    if (inScope(scope, Scope.NIT))
                        descriptor = new DVBTerrestrialDeliverySystemDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case ContentIdentifierDescriptorTag:
                    if (inScope(scope, Scope.EIT))
                        descriptor = new DVBContentIdentifierDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case ExtensionDescriptorTag:
                    if ((int)byteData[index + 1] == T2DeliverySystemDescriptorTag)
                    {
                        if (inScope(scope, Scope.NIT))
                            descriptor = new DVBT2DeliverySystemDescriptor();
                        else
                            logOutOfScope(byteData[index]);
                    }
                    break;
                case DescriptorTag_0x09:
                    if (inScope(scope, Scope.PMT))
                        descriptor = new DVBCADescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    /*{
                        if (inScope(scope, Scope.AIT))
                            descriptor = new DVBHtmlApplicationLocationDescriptor();
                    }*/
                    break;
                case DataBroadcastDescriptorTag:
                    if (inScope(scope, Scope.SDT | Scope.EIT | Scope.SIT))
                        descriptor = new DVBDataBroadcastDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case DataBroadcastIDDescriptorTag:
                    if (inScope(scope, Scope.PMT))
                        descriptor = new DVBDataBroadcastIdDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case PrivateDataSpecifierDescriptorTag:
                    if (inScope(scope, Scope.NIT | Scope.BAT | Scope.SDT | Scope.EIT | Scope.PMT | Scope.SIT))
                        descriptor = new DVBPrivateDataSpecifierDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case DefaultAuthorityDescriptorTag:
                    if (inScope(scope, Scope.NIT | Scope.BAT | Scope.SDT))
                        descriptor = new DVBDefaultAuthorityDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case ServiceAvailabilityDescriptorTag:
                    if (inScope(scope, Scope.SDT))
                        descriptor = new DVBServiceAvailabilityDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case StreamIdentifierDescriptorTag:
                    if (inScope(scope, Scope.PMT))
                        descriptor = new DVBStreamIdentifierDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case CountryAvailabilityDescriptorTag:
                    if (inScope(scope, Scope.BAT | Scope.SDT | Scope.SIT))
                        descriptor = new DVBCountryAvailabilityDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case ServiceIdentifierDescriptorTag:
                    if (inScope(scope, Scope.SDT))
                        descriptor = new DVBServiceIdentifierDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case OpenTVChannelInfoDescriptorTag:
                    if (EPGController.Instance.CurrentCollector != null &&
                        EPGController.Instance.CurrentCollector.CollectionType == CollectionType.OpenTV)
                        descriptor = new OpenTVChannelInfoDescriptor();
                    break;
                case OpenTVChannelGroupDescriptorTag:
                    if (EPGController.Instance.CurrentCollector != null &&
                        EPGController.Instance.CurrentCollector.CollectionType == CollectionType.OpenTV &&
                        inScope(scope, Scope.SDT))
                            descriptor = new OpenTVChannelGroupDescriptor();
                    break;
                case GenericChannelInfoDescriptorTag:
                    switch (scope)
                    {
                        case Scope.BAT:
                            descriptor = new FreeviewChannelInfoDescriptor();
                            break;
                        case Scope.SDT:
                            descriptor = new ServiceChannelDescriptor();
                            break;
                        default:
                            break;
                    }
                    break;
                case FreeviewChannelInfoDescriptorTag:
                    if (inScope(scope, Scope.BAT | Scope.NIT))
                        descriptor = new FreeviewChannelInfoDescriptor();
                    break;
                case TurkeyChannelInfoDescriptorTag:
                case E2ChannelInfoDescriptorTag:
                    if (inScope(scope, Scope.BAT))
                        descriptor = new FreeviewChannelInfoDescriptor();
                    break;
                case FreeSatChannelInfoDescriptorTag:
                    if (EPGController.Instance.CurrentCollector != null && EPGController.Instance.CurrentCollector.CollectionType == CollectionType.FreeSat)
                        descriptor = new FreeSatChannelInfoDescriptor();
                    break;
                case FreeSatRegionDescriptorTag:
                    if (EPGController.Instance.CurrentCollector != null && EPGController.Instance.CurrentCollector.CollectionType == CollectionType.FreeSat)
                        descriptor = new FreeSatRegionDescriptor();
                    break;
                case FreeSatCategoryDescriptorTag:
                    if (EPGController.Instance.CurrentCollector != null && EPGController.Instance.CurrentCollector.CollectionType == CollectionType.FreeSat)
                        descriptor = new FreeSatCategoryDescriptor();
                    break;
                case FreeSatImageDescriptorTag:
                    if (EPGController.Instance.CurrentCollector != null && EPGController.Instance.CurrentCollector.CollectionType == CollectionType.FreeSat)
                        descriptor = new FreeSatImageDescriptor();
                    break;
                case DishNetworkEPGLinkDescriptorTag:
                    if (EPGController.Instance.CurrentCollector != null && EPGController.Instance.CurrentCollector.CollectionType == CollectionType.DishNetwork)
                        descriptor = new DishNetworkEPGLinkDescriptor();
                    break;
                case UKCableChannelDescriptorTag:
                    if (EPGController.Instance.CurrentCollector != null && EPGController.Instance.CurrentCollector.CollectionType == CollectionType.EIT)
                    {
                        if (inScope(scope, Scope.SDT))
                            descriptor = new DVBCableChannelInfoDescriptor();
                        else
                            logOutOfScope(byteData[index]);
                    }
                    break;
                /*case ApplicationDescriptorTag:
                    if (inScope(scope, Scope.AIT))
                        descriptor = new DVBApplicationDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case ApplicationNameDescriptorTag:
                    if (inScope(scope, Scope.AIT))
                        descriptor = new DVBApplicationNameDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case TransportProtocolDescriptorTag:
                    if (inScope(scope, Scope.AIT))
                        descriptor = new DVBTransportProtocolDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case JavaApplicationDescriptorTag:
                    if (inScope(scope, Scope.AIT))
                        descriptor = new DVBJavaApplicationDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case JavaApplicationLocationDescriptorTag:
                    if (inScope(scope, Scope.AIT))
                        descriptor = new DVBJavaApplicationLocationDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case ExternalApplicationDescriptorTag:
                    if (inScope(scope, Scope.AIT))
                        descriptor = new DVBExternalApplicationDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case ApplicationRecordingDescriptorTag:
                    if (inScope(scope, Scope.AIT))
                        descriptor = new DVBApplicationRecordingDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case HtmlApplicationDescriptorTag:
                    if (inScope(scope, Scope.AIT))
                        descriptor = new DVBHtmlApplicationDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case HtmlApplicationLocationDescriptorTag:
                    descriptor = new DVBHtmlApplicationLocationDescriptor();
                    break;
                case HtmlApplicationBoundaryDescriptorTag:
                    if (inScope(scope, Scope.AIT))
                        descriptor = new DVBHtmlApplicationBoundaryDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case ApplicationIconsDescriptorTag:
                    if (inScope(scope, Scope.AIT))
                        descriptor = new DVBApplicationIconsDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case PrefetchDescriptorTag:
                    if (inScope(scope, Scope.AIT))
                        descriptor = new DVBPrefetchDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case DIILocationDescriptorTag:
                    if (inScope(scope, Scope.AIT))
                        descriptor = new DVBDIILocationDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case ApplicationStorageDescriptorTag:
                    if (inScope(scope, Scope.AIT))
                        descriptor = new DVBApplicationStorageDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case IPSignallingDescriptorTag:
                    if (inScope(scope, Scope.AIT))
                        descriptor = new DVBIPSignallingDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case ApplicationUsageDescriptorTag:
                    if (inScope(scope, Scope.AIT))
                        descriptor = new DVBApplicationUsageDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case SimpleApplicationLocationDescriptorTag:
                    if (inScope(scope, Scope.AIT))
                        descriptor = new DVBSimpleApplicationLocationDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case SimpleApplicationBoundaryDescriptorTag:
                    if (inScope(scope, Scope.AIT))
                        descriptor = new DVBSimpleApplicationBoundaryDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case GraphicsConstraintDescriptorTag:
                    if (inScope(scope, Scope.AIT))
                        descriptor = new DVBGraphicsConstraintDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;
                case ApplicationSignallingDescriptorTag:
                    if (inScope(scope, Scope.PMT))
                        descriptor = new DVBApplicationSignallingDescriptor();
                    else
                        logOutOfScope(byteData[index]);
                    break;*/
                default:
                    break;
            }

            if (descriptor == null)
                descriptor = new DescriptorBase();

            descriptor.tag = (int)byteData[index];
            index++;

            descriptor.length = (int)byteData[index];
            index++;

            if (descriptor.Length != 0)
                descriptor.Process(byteData, index);

            return (descriptor);
        }

        /// <summary>
        /// Create an instance of the descriptor class for ATSC descriptors.
        /// </summary>
        /// <param name="byteData">The MPEG2 section containing the descriptor.</param>
        /// <param name="index">The index of the tag byte of the descriptor.</param>
        /// <returns>A descriptor instance.</returns>
        internal static DescriptorBase AtscInstance(byte[] byteData, int index)
        {
            DescriptorBase descriptor;

            if (EPGController.Instance.CurrentCollector == null || EPGController.Instance.CurrentCollector.CollectionType == CollectionType.PSIP)
            {
                switch ((int)byteData[index])
                {
                    case AtscAC3AudioDescriptorTag:
                        descriptor = new AC3AudioDescriptor();
                        break;
                    case AtscCaptionServiceDescriptorTag:
                        descriptor = new CaptionServiceDescriptor();
                        break;
                    case AtscContentAdvisoryDescriptorTag:
                        descriptor = new ContentAdvisoryDescriptor();
                        break;
                    case AtscExtendedChannelNameDescriptorTag:
                        descriptor = new ExtendedChannelNameDescriptor();
                        break;
                    case AtscServiceLocationDescriptorTag:
                        descriptor = new ServiceLocationDescriptor();
                        break;
                    case AtscGenreDescriptorTag:
                        descriptor = new GenreDescriptor();
                        break;
                    default:
                        descriptor = new DescriptorBase();
                        break;
                }
            }
            else
                descriptor = new DescriptorBase();

            descriptor.tag = (int)byteData[index];
            index++;

            descriptor.length = (int)byteData[index];
            index++;

            descriptor.Process(byteData, index);

            return (descriptor);
        }

        /// <summary>
        /// Create an instance of the descriptor class for Dish Network descriptors.
        /// </summary>
        /// <param name="byteData">The MPEG2 section containing the descriptor.</param>
        /// <param name="index">The index of the tag byte of the descriptor.</param>
        /// <param name="table">The table ID containing this descriptor.</param>
        /// <returns>A descriptor instance.</returns>
        internal static DescriptorBase DishNetworkInstance(byte[] byteData, int index, int table)
        {
            DescriptorBase descriptor;

            if (EPGController.Instance.CurrentCollector.CollectionType == CollectionType.DishNetwork)
            {
                switch ((int)byteData[index])
                {
                    case DishNetworkRatingDescriptorTag:
                        descriptor = new DishNetworkRatingDescriptor();
                        break;
                    case DishNetworkShortEventDescriptorTag:
                        descriptor = new DishNetworkShortEventDescriptor();
                        break;
                    case DishNetworkExtendedEventDescriptorTag:
                        descriptor = new DishNetworkExtendedEventDescriptor();
                        break;
                    case DishNetworkSupplementaryDescriptorTag:
                        descriptor = new DishNetworkSupplementaryDescriptor();
                        break;
                    case DishNetworkVCHIPDescriptorTag:
                        descriptor = new DishNetworkVCHIPDescriptor();
                        break;
                    case DishNetworkSeriesDescriptorTag:
                        descriptor = new DishNetworkSeriesDescriptor();
                        break;
                    case ContentDescriptorTag:
                        descriptor = new DVBContentDescriptor();
                        break;
                    default:
                        descriptor = new DescriptorBase();
                        break;
                }
            }
            else
                descriptor = new DescriptorBase();

            descriptor.table = table;

            descriptor.tag = (int)byteData[index];
            index++;

            descriptor.length = (int)byteData[index];
            index++;

            descriptor.Process(byteData, index);

            return (descriptor);
        }

        /// <summary>
        /// Create an instance of the descriptor class for Bell TV descriptors.
        /// </summary>
        /// <param name="byteData">The MPEG2 section containing the descriptor.</param>
        /// <param name="index">The index of the tag byte of the descriptor.</param>
        /// <param name="table">The table ID containing this descriptor.</param>
        /// <returns>A descriptor instance.</returns>
        internal static DescriptorBase BellTVInstance(byte[] byteData, int index, int table)
        {
            DescriptorBase descriptor;

            if (EPGController.Instance.CurrentCollector.CollectionType == CollectionType.BellTV)
            {
                switch ((int)byteData[index])
                {
                    case ShortEventDescriptorTag:
                        descriptor = new BellShortEventDescriptor();
                        break;
                    case ExtendedEventDescriptorTag:
                        descriptor = new BellTVExtendedEventDescriptor();
                        break;
                    case BellTVRatingDescriptorTag:
                        descriptor = new BellTVRatingDescriptor();
                        break;
                    case BellTVSeriesDescriptorTag:
                        descriptor = new BellTVSeriesDescriptor();
                        break;
                    case ContentDescriptorTag:
                        descriptor = new DVBContentDescriptor();
                        break;
                    default:
                        descriptor = new DescriptorBase();
                        break;
                }
            }
            else
                descriptor = new DescriptorBase();

            descriptor.table = table;

            descriptor.tag = (int)byteData[index];
            index++;

            descriptor.length = (int)byteData[index];
            index++;

            descriptor.Process(byteData, index);

            return (descriptor);
        }

        /// <summary>
        /// Initialize a new instance of the DescriptorBase class.
        /// </summary>
        internal DescriptorBase() { }

        /// <summary>
        /// Parse the descriptor.
        /// </summary>
        /// <param name="byteData">The MPEG2 section containing the descriptor.</param>
        /// <param name="index">Index of the byte in the MPEG2 section following the descriptor length.</param>
        internal virtual void Process(byte[] byteData, int index)
        {
            lastIndex = index;

            if (Length != 0)
            {
                data = Utils.GetBytes(byteData, index, Length);
                lastIndex += Length;
            }

            isUndefined = true;
        }

        /// <summary>
        /// Validate the descriptor fields.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException">
        /// A descriptor field is not valid.
        /// </exception>
        internal virtual void Validate() { }

        private static bool inScope(Scope requestedScope, Scope validScope)
        {
            if (validScope == Scope.All)
                return (true);

            return ((requestedScope & validScope) != 0);
        }

        private static void logOutOfScope(byte tag)
        {
            if (!DebugEntry.IsDefined(DebugName.LogOutOfScope))
                return;

            Logger.Instance.Write("Descriptor tag 0x" + tag.ToString("x2") + " out of scope - generic descriptor used");
        }

        /// <summary>
        /// Enable or disable logging of generic descriptors.
        /// </summary>
        /// <param name="enabled">True to enable logging; false otherwise.</param>
        internal static void SetGenericLogging(bool enabled)
        {
            genericLoggingEnabled = enabled;            
        }

        /// <summary>
        /// Log the descriptor fields.
        /// </summary>
        internal virtual void LogMessage()
        {
            if (!genericLoggingEnabled)
                return;

            if (TraceEntry.IsDefined(TraceName.GenericDescriptorOnly))
                logMessage(Logger.Instance);
            else
            {
                if (Logger.ProtocolLogger == null)
                    return;

                if (!TraceEntry.IsDefined(TraceName.GenericDescriptor))
                    return;

                logMessage(Logger.ProtocolLogger);
            }            
        }

        private void logMessage(Logger logger)
        {
            logger.Write(Logger.ProtocolIndent + "DVB GENERIC DESCRIPTOR: Tag: " + Utils.ConvertToHex(tag) +
                " Length: " + length);

            if (length != 0)
                logger.Dump("Generic Descriptor Data", data, data.Length); 
        }
    }
}
