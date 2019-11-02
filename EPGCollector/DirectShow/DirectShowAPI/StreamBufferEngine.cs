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
using System.Runtime.InteropServices;
using System.Text;

namespace DirectShowAPI
{
    /// <summary>
    /// From STREAMBUFFER_ATTR_DATATYPE
    /// </summary>
    public enum StreamBufferAttrDataType
    {
        /// <summary>
        /// Double word
        /// </summary>
        DWord = 0,
        /// <summary>
        /// String
        /// </summary>
        String = 1,
        /// <summary>
        /// Binary
        /// </summary>
        Binary = 2,
        /// <summary>
        /// Bool
        /// </summary>
        Bool = 3,
        /// <summary>
        /// Quad word
        /// </summary>
        QWord = 4,
        /// <summary>
        /// Word
        /// </summary>
        Word = 5,
        /// <summary>
        /// Guid
        /// </summary>
        Guid = 6
    }

    /// <summary>
    /// </summary>
    public sealed class StreamBufferEngine
    {
        private StreamBufferEngine() { }

        /// <summary>
        /// Duration attribute.
        /// </summary>
        public readonly string Duration = "Duration";
        /// <summary>
        /// Bitrate attribute.
        /// </summary>
        public readonly string Bitrate = "Bitrate";
        /// <summary>
        /// Seekable attribute.
        /// </summary>
        public readonly string Seekable = "Seekable";
        /// <summary>
        /// Stridable attribute.
        /// </summary>
        public readonly string Stridable = "Stridable";
        /// <summary>
        /// Broadcast attribute.
        /// </summary>
        public readonly string Broadcast = "Broadcast";
        /// <summary>
        /// Is protected attribute.
        /// </summary>
        public readonly string Protected = "Is_Protected";
        /// <summary>
        /// Is trusted attribute.
        /// </summary>
        public readonly string Trusted = "Is_Trusted";
        /// <summary>
        /// Signature name attribute.
        /// </summary>
        public readonly string Signature_Name = "Signature_Name";
        /// <summary>
        /// Has audio attribute.
        /// </summary>
        public readonly string HasAudio = "HasAudio";
        /// <summary>
        /// Has image attribute.
        /// </summary>
        public readonly string HasImage = "HasImage";
        /// <summary>
        /// Has script attribute.
        /// </summary>
        public readonly string HasScript = "HasScript";
        /// <summary>
        /// Has video attribute.
        /// </summary>
        public readonly string HasVideo = "HasVideo";
        /// <summary>
        /// Current bitrate attribute.
        /// </summary>
        public readonly string CurrentBitrate = "CurrentBitrate";
        /// <summary>
        /// Optimal bitrate attribute.
        /// </summary>
        public readonly string OptimalBitrate = "OptimalBitrate";
        /// <summary>
        /// Has attached images attribute.
        /// </summary>
        public readonly string HasAttachedImages = "HasAttachedImages";
        /// <summary>
        /// Can skip backward attribute.
        /// </summary>
        public readonly string SkipBackward = "Can_Skip_Backward";
        /// <summary>
        /// Can skip forward attribute.
        /// </summary>
        public readonly string SkipForward = "Can_Skip_Forward";
        /// <summary>
        /// Number of frames attribute.
        /// </summary>
        public readonly string NumberOfFrames = "NumberOfFrames";
        /// <summary>
        /// File size attribute.
        /// </summary>
        public readonly string FileSize = "FileSize";
        /// <summary>
        /// Has arbitrary data stream attribute.
        /// </summary>
        public readonly string HasArbitraryDataStream = "HasArbitraryDataStream";
        /// <summary>
        /// Has file transfer stream.
        /// </summary>
        public readonly string HasFileTransferStream = "HasFileTransferStream";

        /// <summary>
        /// Content title.
        /// </summary>
        public readonly string Title = "Title";
        /// <summary>
        /// Content author.
        /// </summary>
        public readonly string Author = "Author";
        /// <summary>
        /// Content description.
        /// </summary>
        public readonly string Description = "Description";
        /// <summary>
        /// Content rating.
        /// </summary>
        public readonly string Rating = "Rating";
        /// <summary>
        /// Content copyright.
        /// </summary>
        public readonly string Copyright = "Copyright";

        /// <summary>
        /// Album title attribute.
        /// </summary>
        public readonly string AlbumTitle = "WM/AlbumTitle";
        /// <summary>
        /// Track attribute.
        /// </summary>
        public readonly string Track = "WM/Track";
        /// <summary>
        /// Promotion URL attribute.
        /// </summary>
        public readonly string PromotionURL = "WM/PromotionURL";
        /// <summary>
        /// Album cover URL attribute.
        /// </summary>
        public readonly string AlbumCoverURL = "WM/AlbumCoverURL";
        /// <summary>
        /// Genre attribute.
        /// </summary>
        public readonly string Genre = "WM/Genre";
        /// <summary>
        /// Year of release attribute.
        /// </summary>
        public readonly string Year = "WM/Year";
        /// <summary>
        /// Genre ID attribute.
        /// </summary>
        public readonly string GenreID = "WM/GenreID";
        /// <summary>
        /// MCDI attribute.
        /// </summary>
        public readonly string MCDI = "WM/MCDI";
        /// <summary>
        /// Composer attribute.
        /// </summary>
        public readonly string Composer = "WM/Composer";
        /// <summary>
        /// Lyrics attribute.
        /// </summary>
        public readonly string Lyrics = "WM/Lyrics";
        /// <summary>
        /// Track number attribute.
        /// </summary>
        public readonly string TrackNumber = "WM/TrackNumber";
        /// <summary>
        /// Tool name attribute.
        /// </summary>
        public readonly string ToolName = "WM/ToolName";
        /// <summary>
        /// Tool version attribute.
        /// </summary>
        public readonly string ToolVersion = "WM/ToolVersion";
        /// <summary>
        /// Is VBR attribute.
        /// </summary>
        public readonly string IsVBR = "IsVBR";
        /// <summary>
        /// Album and artist attribute.
        /// </summary>
        public readonly string AlbumArtist = "WM/AlbumArtist";

        /// <summary>
        /// Banner image type attribute.
        /// </summary>
        public readonly string BannerImageType = "BannerImageType";
        /// <summary>
        /// Banner image data attribute.
        /// </summary>
        public readonly string BannerImageData = "BannerImageData";
        /// <summary>
        /// Banner image URL attribute.
        /// </summary>
        public readonly string BannerImageURL = "BannerImageURL";
        /// <summary>
        /// Copyright attribute.
        /// </summary>
        public readonly string CopyrightURL = "CopyrightURL";

        /// <summary>
        /// Video stream aspect ratio X.
        /// </summary>
        public readonly string AspectRatioX = "AspectRatioX";
        /// <summary>
        /// Video stream aspect ratio Y.
        /// </summary>
        public readonly string AspectRatioY = "AspectRatioY";

        /// <summary>
        /// NSC name attribute.
        /// </summary>
        public readonly string NSCName = "NSC_Name";
        /// <summary>
        /// NSC address attribute.
        /// </summary>
        public readonly string NSCAddress = "NSC_Address";
        /// <summary>
        /// NSC phone number attribute.
        /// </summary>
        public readonly string NSCPhone = "NSC_Phone";
        /// <summary>
        /// NSC email address attribute.
        /// </summary>
        public readonly string NSCEmail = "NSC_Email";
        /// <summary>
        /// NSC description attribute.
        /// </summary>
        public readonly string NSCDescription = "NSC_Description";
    }

    /// <summary>
    /// From STREAMBUFFER_ATTRIBUTE
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct StreamBufferAttribute
    {
        /// <summary>
        /// The name.
        /// </summary>
        [MarshalAs(UnmanagedType.LPWStr)] public string pszName;
        /// <summary>
        /// The attribute type.
        /// </summary>
        public StreamBufferAttrDataType StreamBufferAttributeType;
        /// <summary>
        /// The attribute.
        /// </summary>
        public IntPtr pbAttribute; // BYTE *
        /// <summary>
        /// The length.
        /// </summary>
        public short cbLength;
    }

    /// <summary>
    /// The StreamBufferRecordingAttribute interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("16CA4E03-FE69-4705-BD41-5B7DFC0C95F3"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IStreamBufferRecordingAttribute
    {
        /// <summary>
        /// Set an attribute.
        /// </summary>
        /// <param name="ulReserved">Reserved.</param>
        /// <param name="pszAttributeName">The attribute name.</param>
        /// <param name="StreamBufferAttributeType">The attribute type.</param>
        /// <param name="pbAttribute">The attribute.</param>
        /// <param name="cbAttributeLength">The attribute length.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int SetAttribute(
            [In] int ulReserved,
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszAttributeName,
            [In] StreamBufferAttrDataType StreamBufferAttributeType,
            [In] IntPtr pbAttribute, // BYTE *
            [In] short cbAttributeLength
            );

        /// <summary>
        /// Get the count of attributes.
        /// </summary>
        /// <param name="ulReserved">Reserved.</param>
        /// <param name="pcAttributes">The count.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int GetAttributeCount(
            [In] int ulReserved,
            [Out] out short pcAttributes
            );

        /// <summary>
        /// Get an attribute by name.
        /// </summary>
        /// <param name="pszAttributeName">The attribute name.</param>
        /// <param name="pulReserved">Reserved.</param>
        /// <param name="pStreamBufferAttributeType">The attribute type.</param>
        /// <param name="pbAttribute">The attribute.</param>
        /// <param name="pcbLength">The attribute length.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int GetAttributeByName(
            [In, MarshalAs(UnmanagedType.LPWStr)] string pszAttributeName,
            [In] int pulReserved,
            [Out] out StreamBufferAttrDataType pStreamBufferAttributeType,
            [In, Out] IntPtr pbAttribute, // BYTE *
            [In, Out] ref short pcbLength
            );

        /// <summary>
        /// Get an attribute by its position.
        /// </summary>
        /// <param name="wIndex">The position.</param>
        /// <param name="pulReserved">Reserved.</param>
        /// <param name="pszAttributeName">The attribute name.</param>
        /// <param name="pcchNameLength">The length of the name.</param>
        /// <param name="pStreamBufferAttributeType">The attribute type.</param>
        /// <param name="pbAttribute">The attribute.</param>
        /// <param name="pcbLength">The count of bytes returned.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int GetAttributeByIndex(
            [In] short wIndex,
            [In, Out] ref int pulReserved,
            [Out, MarshalAs(UnmanagedType.LPWStr)] StringBuilder pszAttributeName,
            [In, Out] ref short pcchNameLength,
            [Out] out StreamBufferAttrDataType pStreamBufferAttributeType,
            [Out, MarshalAs(UnmanagedType.LPArray)] byte[] pbAttribute,
            [In, Out] ref short pcbLength
            );

        /// <summary>
        /// Enumerate attributes.
        /// </summary>
        /// <param name="ppIEnumStreamBufferAttrib">The enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        int EnumAttributes([Out] out IEnumStreamBufferRecordingAttrib ppIEnumStreamBufferAttrib);
    }

    /// <summary>
    /// The EnumStreamBufferRecordingAttrib interface.
    /// </summary>
    [ComImport, System.Security.SuppressUnmanagedCodeSecurity,
    Guid("C18A9162-1E82-4142-8C73-5690FA62FE33"),
    InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IEnumStreamBufferRecordingAttrib
    {
        /// <summary>
        /// Get the next entry.
        /// </summary>
        /// <param name="cRequest">Number of entries requested.</param>
        /// <param name="pStreamBufferAttribute">Returned entries.</param>
        /// <param name="pcReceived">The count of entries returned.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Next(
            [In] int cRequest,
            [Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] StreamBufferAttribute[] pStreamBufferAttribute,
            [In] IntPtr pcReceived
            );

        /// <summary>
        /// Skip entries.
        /// </summary>
        /// <param name="cRecords">Count of entries to skip.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Skip([In] int cRecords);

        /// <summary>
        /// Reset the enmerator.
        /// </summary>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Reset();

        /// <summary>
        /// Clone the enumerator.
        /// </summary>
        /// <param name="ppIEnumStreamBufferAttrib">The cloned enumerator.</param>
        /// <returns>Zero if successful; error code otherwise.</returns>
        [PreserveSig]
        int Clone([Out] out IEnumStreamBufferRecordingAttrib ppIEnumStreamBufferAttrib);
    }
}
