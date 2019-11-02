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

namespace DomainObjects
{
    /// <summary>
    /// TV Station types.
    /// </summary>
    public enum TVStationType
    {
        /// <summary>
        /// The type is DVB.
        /// </summary>
        Dvb,
        /// <summary>
        /// The type is ATSC.
        /// </summary>
        Atsc
    }

    /// <summary>
    /// Tuner node types.
    /// </summary>
    public enum TunerNodeType
    {
        /// <summary>
        /// The tuner node type for satellite.
        /// </summary>
        Satellite,
        /// <summary>
        /// The tuner node type for terrestrial.
        /// </summary>
        Terrestrial,
        /// <summary>
        /// The tuner node type for cable.
        /// </summary>
        Cable,
        /// <summary>
        /// The tuner node type for ATSC.
        /// </summary>
        ATSC,
        /// <summary>
        /// The tuner node type for ISDB satellite.
        /// </summary>
        ISDBS,
        /// <summary>
        /// The tuner node type for ISDB terrestrial.
        /// </summary>
        ISDBT,
        /// <summary>
        /// The tuner node type for undefined types.
        /// </summary>
        Other
    }

    /// <summary>
    /// Tuner types.
    /// </summary>
    public enum TunerType
    {
        /// <summary>
        /// The tuner type for satellite.
        /// </summary>
        Satellite,
        /// <summary>
        /// The tuner type for terrestrial.
        /// </summary>
        Terrestrial,
        /// <summary>
        /// The tuner type for cable.
        /// </summary>
        Cable,
        /// <summary>
        /// The tuner type for ATSC terrestrial.
        /// </summary>
        ATSC,
        /// <summary>
        /// The tuner type for ATSC cable.
        /// </summary>
        ATSCCable,
        /// <summary>
        /// The tuner type for Clear QAM.
        /// </summary>
        ClearQAM,
        /// <summary>
        /// The tuner type for ISDB-S.
        /// </summary>
        ISDBS,
        /// <summary>
        /// The tuner type for ISDB-T.
        /// </summary>
        ISDBT,
        /// <summary>
        /// The tuner type for SAT>IP servers.
        /// </summary>
        SATIP,
        /// <summary>
        /// The tuner type for file types.
        /// </summary>
        File,
        /// <summary>
        /// The tuner type for stream types.
        /// </summary>
        Stream,
        /// <summary>
        /// The tuner type for undefined types.
        /// </summary>
        Other
    }

    /// <summary>
    /// EPG sources.
    /// </summary>
    public enum EPGSource
    {
        /// <summary>
        /// The EPG originated from the MHEG5 protocol.
        /// </summary>
        MHEG5,
        /// <summary>
        /// The EPG originated from the DVB EIT protocol.
        /// </summary>
        EIT,
        /// <summary>
        /// The EPG originated from the OpenTV protocol.
        /// </summary>
        OpenTV,
        /// <summary>
        /// The EPG originated from the FreeSat protocol.
        /// </summary>
        FreeSat,
        /// <summary>
        /// The EPG originated from the MediaHighway1 protocol.
        /// </summary>
        MediaHighway1,
        /// <summary>
        /// The EPG originated from the MediaHighway2 protocol.
        /// </summary>
        MediaHighway2,
        /// <summary>
        /// The EPG originated from the ATSC PSIP protocol.
        /// </summary>        
        PSIP,
        /// <summary>
        /// The EPG originated from the Dish Network EEPG protocol.
        /// </summary>
        DishNetwork,
        /// <summary>
        /// The EPG originated from the Bell TV EEPG protocol.
        /// </summary>
        BellTV,
        /// <summary>
        /// The EPG originated from the Siehfern Info protocol.
        /// </summary>
        SiehfernInfo
    }

    /// <summary>
    /// The types of EPG collection.
    /// </summary>
    public enum CollectionType
    {
        /// <summary>
        /// The collection is for the MHEG5 protocol.
        /// </summary>
        MHEG5,
        /// <summary>
        /// The collection is for the DVB EIT protocol.
        /// </summary>
        EIT,
        /// <summary>
        /// The collection is for the OpenTV protocol.
        /// </summary>
        OpenTV,
        /// <summary>
        /// The collection is for the FreeSat protocol.
        /// </summary>
        FreeSat,
        /// <summary>
        /// The collection is for the MediaHighway1 protocol.
        /// </summary>
        MediaHighway1,
        /// <summary>
        /// The collection is for the MediaHighway2 protocol.
        /// </summary>
        MediaHighway2,        
        /// <summary>
        /// The collection is for the ATSC PSIP protocol.
        /// </summary>
        PSIP,
        /// <summary>
        /// The collection is for the Dish Network EEPG protocol.
        /// </summary>
        DishNetwork,
        /// <summary>
        /// The collection is for the Bell TV EEPG protocol.
        /// </summary>
        BellTV,
        /// <summary>
        /// The collection is for the Siehfern Info protocol.
        /// </summary>
        SiehfernInfo,
        /// <summary>
        /// The collection is for the NDS protocol.
        /// </summary>
        NDS
    }

    /// <summary>
    /// The values that a diseqc switch can have.
    /// </summary>
    public enum DiseqcSettings
    {
        /// <summary>
        /// The switch is not used
        /// </summary>
        None,
        /// <summary>
        /// Simple A.
        /// </summary>
        A,
        /// <summary>
        /// Simple B.
        /// </summary>
        B,
        /// <summary>
        /// Use satellite A port A (Disqec 1.0 committed switch)
        /// </summary>
        AA,
        /// <summary>
        /// Use satellite A port B (Disqec 1.0 committed switch)
        /// </summary>
        AB,
        /// <summary>
        /// Use satellite B port A (Disqec 1.0 committed switch)
        /// </summary>
        BA,
        /// <summary>
        /// Use satellite B port B (Disqec 1.0 committed switch)
        /// </summary>
        BB,
        /// <summary>
        /// Use port 1 (Disqec 1.1 uncommitted switch)
        /// </summary>
        PORT1,
        /// <summary>
        /// Use port 2 (Disqec 1.1 uncommitted switch)
        /// </summary>
        PORT2,
        /// <summary>
        /// Use port 3 (Disqec 1.1 uncommitted switch)
        /// </summary>
        PORT3,
        /// <summary>
        /// Use port 4 (Disqec 1.1 uncommitted switch)
        /// </summary>
        PORT4,
        /// <summary>
        /// Use port 5 (Disqec 1.1 uncommitted switch)
        /// </summary>
        PORT5,
        /// <summary>
        /// Use port 6 (Disqec 1.1 uncommitted switch)
        /// </summary>
        PORT6,
        /// <summary>
        /// Use port 7 (Disqec 1.1 uncommitted switch)
        /// </summary>
        PORT7,
        /// <summary>
        /// Use port 8 (Disqec 1.1 uncommitted switch)
        /// </summary>
        PORT8,
        /// <summary>
        /// Use port 9 (Disqec 1.1 uncommitted switch)
        /// </summary>
        PORT9,
        /// <summary>
        /// Use port 10 (Disqec 1.1 uncommitted switch)
        /// </summary>
        PORT10,
        /// <summary>
        /// Use port 11 (Disqec 1.1 uncommitted switch)
        /// </summary>
        PORT11,
        /// <summary>
        /// Use port 12 (Disqec 1.1 uncommitted switch)
        /// </summary>
        PORT12,
        /// <summary>
        /// Use port 13 (Disqec 1.1 uncommitted switch)
        /// </summary>
        PORT13,
        /// <summary>
        /// Use port 14 (Disqec 1.1 uncommitted switch)
        /// </summary>
        PORT14,
        /// <summary>
        /// Use port 15 (Disqec 1.1 uncommitted switch)
        /// </summary>
        PORT15,
        /// <summary>
        /// Use port 16 (Disqec 1.1 uncommitted switch)
        /// </summary>
        PORT16,
        /// <summary>
        /// Use committed port AA uncommitted port 1 (Combination committed/uncommited switch)
        /// </summary>
        AAPORT1,
        /// <summary>
        /// Use committed port AB uncommitted port 1 (Combination committed/uncommited switch)
        /// </summary>
        ABPORT1,
        /// <summary>
        /// Use committed port BA uncommitted port 1 (Combination committed/uncommited switch)
        /// </summary>
        BAPORT1,
        /// <summary>
        /// Use committed port BB uncommitted port 1 (Combination committed/uncommited switch)
        /// </summary>
        BBPORT1,
        /// <summary>
        /// Use committed port AA uncommitted port 2 (Combination committed/uncommited switch)
        /// </summary>
        AAPORT2,
        /// <summary>
        /// Use committed port AB uncommitted port 2 (Combination committed/uncommited switch)
        /// </summary>
        ABPORT2,
        /// <summary>
        /// Use committed port BA uncommitted port 2 (Combination committed/uncommited switch)
        /// </summary>
        BAPORT2,
        /// <summary>
        /// Use committed port BB uncommitted port 2 (Combination committed/uncommited switch)
        /// </summary>
        BBPORT2,
        /// <summary>
        /// Use committed port AA uncommitted port 3 (Combination committed/uncommited switch)
        /// </summary>
        AAPORT3,
        /// <summary>
        /// Use committed port AB uncommitted port 3 (Combination committed/uncommited switch)
        /// </summary>
        ABPORT3,
        /// <summary>
        /// Use committed port BA uncommitted port 3 (Combination committed/uncommited switch)
        /// </summary>
        BAPORT3,
        /// <summary>
        /// Use committed port BB uncommitted port 3 (Combination committed/uncommited switch)
        /// </summary>
        BBPORT3,
        /// <summary>
        /// Use committed port AA uncommitted port 4 (Combination committed/uncommited switch)
        /// </summary>
        AAPORT4,
        /// <summary>
        /// Use committed port AB uncommitted port 4 (Combination committed/uncommited switch)
        /// </summary>
        ABPORT4,
        /// <summary>
        /// Use committed port BA uncommitted port 4 (Combination committed/uncommited switch)
        /// </summary>
        BAPORT4,
        /// <summary>
        /// Use committed port BB uncommitted port 4 (Combination committed/uncommited switch)
        /// </summary>
        BBPORT4

    }

    /// <summary>
    /// The state of a data update control.
    /// </summary>
    public enum DataState
    {
        /// <summary>
        /// There are unresolved errors.
        /// </summary>
        HasErrors,
        /// <summary>
        /// The data does not need saving.
        /// </summary>
        NotChanged,
        /// <summary>
        /// The data needs saving.
        /// </summary>
        Changed
    }

    /// <summary>
    /// The function of the parameters.
    /// </summary>
    public enum ParameterSet
    {
        /// <summary>
        /// The parameters are used by the Collector.
        /// </summary>
        Collector,
        /// <summary>
        /// The parameters are used by the DVBLogic plugin.
        /// </summary>
        Plugin
    }

    /// <summary>
    /// The type of run.
    /// </summary>
    public enum RunType
    {
        /// <summary>
        /// The parameters are being loaded to run a collection.
        /// </summary>
        Collection,
        /// <summary>
        /// The parameters are being loaded into EPG Centre.
        /// </summary>
        Centre
    }

    /// <summary>
    /// The matching method for lookups.
    /// </summary>
    public enum MatchMethod
    {
        /// <summary>
        /// The title must match exactly.
        /// </summary>
        Exact,
        /// <summary>
        /// The title must contain the string.
        /// </summary>
        Contains,
        /// <summary>
        /// The title must be the nearest match to the string.
        /// </summary>
        Nearest
    }

    /// <summary>
    /// The merge method for channel update.
    /// </summary>
    public enum ChannelMergeMethod
    {
        /// <summary>
        /// No matching.
        /// </summary>
        None,
        /// <summary>
        /// Match on station name.
        /// </summary>
        Name,
        /// <summary>
        /// Match on channel number.
        /// </summary>
        Number,
        /// <summary>
        /// Match on channel name and number.
        /// </summary>
        NameNumber
    }

    /// <summary>
    /// The EPG scan method for channel update.
    /// </summary>
    public enum ChannelEPGScanner
    {
        /// <summary>
        /// No scanning.
        /// </summary>
        None,
        /// <summary>
        /// Default scanning.
        /// </summary>
        Default,
        /// <summary>
        /// Scan using EPG Collector.
        /// </summary>
        EPGCollector,
        /// <summary>
        /// Scan using EIT scanner.
        /// </summary>
        EITScanner,
        /// <summary>
        /// XMLTV epg.
        /// </summary> 
        Xmltv
    }

    /// <summary>
    /// Program exit codes.
    /// </summary>
    public enum ExitCode
    {
        /// <summary>
        /// The run finished normally.
        /// </summary>
        OK,
        /// <summary>
        /// There are no suitable tuners installed.
        /// </summary>
        NoDVBTuners,
        /// <summary>
        /// The ini file cannot be opened.
        /// </summary>
        ParameterFileNotFound,
        /// <summary>
        /// There is an error in the ini file.
        /// </summary>
        ParameterError,
        /// <summary>
        /// The command line is incorrect.
        /// </summary>
        CommandLineWrong,
        /// <summary>
        /// A software exception has occurred.
        /// </summary>
        SoftwareException,
        /// <summary>
        /// Not all EPG data has been collected.
        /// </summary>
        EPGDataIncomplete,
        /// <summary>
        /// The user abandoned the run.
        /// </summary>
        AbandonedByUser,
        /// <summary>
        /// The ini file does not match the hardware configuration.
        /// </summary>
        ParameterTunerMismatch,
        /// <summary>
        /// The log file cannot be written.
        /// </summary>
        LogFileNotAvailable,
        /// <summary>
        /// Some frequencies could not be processed.
        /// </summary>
        SomeFrequenciesNotProcessed,
        /// <summary>
        /// The output file could not be created.
        /// </summary>
        OutputFileNotCreated,
        /// <summary>
        /// The simulation file could not be located or failed to load.
        /// </summary>
        SimulationFileError,
        /// <summary>
        /// No data was collected.
        /// </summary>
        NoDataCollected,
        /// <summary>
        /// The tuner filter could not be loaded.
        /// </summary>
        NoBDATunerFilter,
        /// <summary>
        /// The hardware filter chain could not be created.
        /// </summary>
        HardwareFilterChainNotBuilt,
        /// <summary>
        /// The DVBLogic plugin could not start.
        /// </summary>
        PluginNotStarted
    }

    /// <summary>
    /// The precedence of data for an import files.
    /// </summary>
    public enum DataPrecedence
    {
        /// <summary>
        /// The file data takes precedence over the broadcast data.
        /// </summary>
        File,
        /// <summary>
        /// The broadcast data takes precedence over the file data.
        /// </summary>
        Broadcast
    }

    /// <summary>
    /// The format of the ID attribute in an xmltv import file.
    /// </summary>
    public enum XmltvIdFormat
    {
        /// <summary>
        /// The id attribute has no specific format.
        /// </summary>
        Undefined,
        /// <summary>
        /// The id attribue is the service id.
        /// </summary>
        ServiceId,
        /// <summary>
        /// The id attribute is the user channel number.
        /// </summary>
        UserChannelNumber,
        /// <summary>
        /// The id attribute contains the full channel identification (ie nid:tid:sid:name).
        /// </summary>
        FullChannelId,
        /// <summary>
        /// The id attribute contains the name of the channel.
        /// </summary>
        Name
    }

    /// <summary>
    /// The amount of data a collection controller collects.
    /// </summary>
    public enum CollectionSpan
    {
        /// <summary>
        /// The controller collects all possible data.
        /// </summary>
        AllData,
        /// <summary>
        /// The controller collects only the station data.
        /// </summary>
        StationsOnly,
        /// <summary>
        /// The controller collects only the station and/or channel data.
        /// </summary>
        ChannelsOnly
    }

    /// <summary>
    /// The location of a text substring.
    /// </summary>
    public enum TextLocation
    {
        /// <summary>
        /// The substring is at the start of the text.
        /// </summary>
        Start,
        /// <summary>
        /// The substring is at the end of the text.
        /// </summary>
        End,
        /// <summary>
        /// The substring can occur anywhere.
        /// </summary>
        Anywhere
    }

    /// <summary>
    /// The method used to replace text.
    /// </summary>
    public enum TextReplacementMode
    {
        /// <summary>
        /// Replace only the specified text.
        /// </summary>
        TextOnly,
        /// <summary>
        /// Replace the specified text plus everything that follows.
        /// </summary>
        TextAndFollowing,
        /// <summary>
        /// Replace the specified text plus everything that preceeded it.
        /// </summary>
        TextAndPreceeding,
        /// <summary>
        /// Replace the specified text plus everything else.
        /// </summary>
        Everything
    }

    /// <summary>
    /// The metadata lookup image type.
    /// </summary>
    public enum LookupImageType
    {
        /// <summary>
        /// Type is thumbnail.
        /// </summary>
        Thumbnail,
        /// <summary>
        /// Type is poster.
        /// </summary>
        Poster,
        /// <summary>
        /// Type is banner.
        /// </summary>
        Banner,
        /// <summary>
        /// Type is fanart.
        /// </summary>
        Fanart,
        /// <summary>
        /// Type is small poster.
        /// </summary>
        SmallPoster,
        /// <summary>
        /// Type is small banner.
        /// </summary>
        SmallFanart,
        /// <summary>
        /// No image downloaded.
        /// </summary>
        None
    }

    /// <summary>
    /// The service type of a service.
    /// </summary>
    public enum ServiceType
    {
        /// <summary>
        /// Type is satellite.
        /// </summary>
        Satellite,
        /// <summary>
        /// Type is terrestrial.
        /// </summary>
        Terrestrial,
        /// <summary>
        /// Type is cable.
        /// </summary>
        Cable 
    }

    /// <summary>
    /// The protocols supported by stream collections.
    /// </summary>
    public enum StreamProtocol
    {
        /// <summary>
        /// The protocol is RTSP.
        /// </summary>
        Rtsp,
        /// <summary>
        /// The protocol is RTP/RTCP.
        /// </summary>
        Rtp,
        /// <summary>
        /// The protocol is UDP.
        /// </summary>
        Udp,
        /// <summary>
        /// The protocol is HTTP.
        /// </summary>
        Http
    }

    /// <summary>
    /// The methods used to select a character set
    /// </summary>
    public enum CharacterSetUsage
    {
        /// <summary>
        /// The character set has not been used.
        /// </summary>
        NotUsed,
        /// <summary>
        /// The default method was used.
        /// </summary>
        Default,
        /// <summary>
        /// The character set was determined by the data broadcast.
        /// </summary>
        Broadcast,
        /// <summary>
        /// The character set was determined by a user parameter.
        /// </summary>
        User
    }

    /// <summary>
    /// The identifiers used with the Option parameter.
    /// </summary>
    public enum OptionName
    {
        /// <summary>
        /// Don't log breaks in the data.
        /// </summary>
        AcceptBreaks,
        /// <summary>
        /// Add season/episode numbers to description.
        /// </summary>
        AddSeasonEpisodeToDesc,
        /// <summary>
        /// Autmatically map the data to the channels.
        /// </summary>
        AutoMapEpg,
        /// <summary>
        /// Output the full name as the identity.
        /// </summary>
        ChannelIdFullName,
        /// <summary>
        /// Output a sequential number as the identity.
        /// </summary>
        ChannelIdSeqNo,
        /// <summary>
        /// Output the service ID as the identity.
        /// </summary>
        ChannelIdSid,
        /// <summary>
        /// Check for programme repeats.
        /// </summary>
        CheckForRepeats,
        /// <summary>
        /// Create an audio description tag.
        /// </summary>
        CreateAdTag,
        /// <summary>
        /// Create rhe area/region cross reference file.
        /// </summary>
        CreateArChannels,
        /// <summary>
        /// Create the Bladerunner file.
        /// </summary>
        CreateBrChannels,
        /// <summary>
        /// Create channels that don't appear in the SDT.
        /// </summary>
        CreateMissingChannels,        
        /// <summary>
        /// Create the SageTV frequency file.
        /// </summary>        
        CreateSageTvFrq,
        /// <summary>
        /// Custom categories override the broadcasters categories.
        /// </summary>
        CustomCategoryOverride,
        /// <summary>
        /// Disable the drivers DiSEqc commands.
        /// </summary>
        DisableDriverDiseqc,
        /// <summary>
        /// Disable the WMC guide loader.
        /// </summary>
        DisableInbandLoader,
        /// <summary>
        /// Duplicate data for channels with the same name.
        /// </summary>
        DuplicateSameChannels,
        /// <summary>
        /// Clear the DVBViewer before loading.
        /// </summary>
        DvbViewerClear,
        /// <summary>
        /// Import the data to DVBViewer.
        /// </summary>
        DvbViewerImport,
        /// <summary>
        /// Import the data to the DVBViewer Recording Service.
        /// </summary>
        DvbViewerRecSvcImport,
        /// <summary>
        /// Make subtitles visible in the DVBViewer EPG.
        /// </summary>
        DvbViewerSubtitleVisible,
        /// <summary>
        /// EIT collections finish on record count.
        /// </summary>
        EitDoneOnCount,
        /// <summary>
        /// Ouput each element of a category in a separate xmltv tag.
        /// </summary>
        ElementPerTag,
        /// <summary>
        /// Convert EIT format bytes.
        /// </summary>
        FormatConvert,
        /// <summary>
        /// Convert EIT format bytes using a conversion table.
        /// </summary>
        FormatConvertTable,
        /// <summary>
        /// Remove EIT format bytes.
        /// </summary>
        FormatRemove,
        /// <summary>
        /// Replace EIT format bytes with a space.
        /// </summary>
        FormatReplace,
        /// <summary>
        /// Ignore WMC recordings when checking for repeats.
        /// </summary>
        IgnoreWmcRecordings,
        /// <summary>
        /// Don't create an episode tag.
        /// </summary>
        NoEpisodeTag,
        /// <summary>
        /// Don't use the keyboard during the collection process.
        /// </summary>
        NoLogExcluded,
        /// <summary>
        /// Don't remove data from descriptions.
        /// </summary>
        NoRemoveData,
        /// <summary>
        /// Dont check simulcast channels for repeats.
        /// </summary>
        NoSimulcastRepeats,        
        /// <summary>
        /// Import the data to DVBLogic.
        /// </summary>
        PluginImport,
        /// <summary>
        /// Process all channels irrespective of type.
        /// </summary>
        ProcessAllStations,
        /// <summary>
        /// Repeat DiSEqC commands if they fail.
        /// </summary>
        RepeatDiseqc,
        /// <summary>
        /// Round programme times.
        /// </summary>
        RoundTime,
        /// <summary>
        /// Run the collection process as a Windows service.
        /// </summary>
        RunFromService,
        /// <summary>
        /// Don't output to the SageTV file if a channel has no EPG data.
        /// </summary>
        SageTvOmitNoEpg,
        /// <summary>
        /// Use SID only to match EPG data with channel.
        /// </summary>
        SidMatchOnly,
        /// <summary>
        /// Save the channel information.
        /// </summary>        
        StoreStationInfo,
        /// <summary>
        /// Change the DiSEqC switch after starting the DirectShow graph.
        /// </summary>
        SwitchAfterPlay,
        /// <summary>
        /// Change the DiSEqC switch after tuning the frequency.
        /// </summary>
        SwitchAfterTune,
        /// <summary>
        /// Only terrestrial channels are relevant.
        /// </summary>
        TcRelevantOnly,
        /// <summary>
        /// Use the codepage from the broadcast.
        /// </summary>
        UseBroadcastCp,
        /// <summary>
        /// Create an XMLTV file suitable for BSEPG.
        /// </summary>
        UseBsepg,
        /// <summary>
        /// Use the channel ID as the idfentity.
        /// </summary>
        UseChannelId,
        /// <summary>
        /// Use the content subtype when processing content type.
        /// </summary>
        UseContentSubtype,
        /// <summary>
        /// Use the programme description as the category.
        /// </summary>
        UseDescAsCategory,
        /// <summary>
        /// Use the programme description as the subtitle.
        /// </summary>
        UseDescAsSubtitle,
        /// <summary>
        /// Use DiSEqC commands to change the switch.
        /// </summary>
        UseDiseqcCommand,
        /// <summary>
        /// Export to DVBViewer.
        /// </summary>
        UseDvbViewer,
        /// <summary>
        /// Use FreeSat tables to decode compressed EIT data.
        /// </summary>
        UseFreeSatTables,
        /// <summary>
        /// Download station images.
        /// </summary>
        UseImage,
        /// <summary>
        /// Use the logical channel number as the identity.
        /// </summary>
        UseLcn,
        /// <summary>
        /// Ignore the programme description.
        /// </summary>
        UseNoDesc,
        /// <summary>
        /// Use the numeric part of the CRID as the eisode identifier.
        /// </summary>
        UseNumericCrid,
        /// <summary>
        /// Use the whole CRID as the episode identifier.
        /// </summary>
        UseRawCrid,
        /// <summary>
        /// Only change the DiSEqC switch if the tuner is not in use.
        /// </summary>
        UseSafeDiseqc,        
        /// <summary>
        /// Use the stored channel information in place of the broadcast data.
        /// </summary>
        UseStoredStationInfo,
        /// <summary>
        /// Use the in-built WMC repeat checking with the programme titles.
        /// </summary>
        UseWmcRepeatCheck,
        /// <summary>
        /// Use the in-built WMC repeat checking with broadcaster references.
        /// </summary>
        UseWmcRepeatCheckBroadcast,
        /// <summary>
        /// Only create valid episode tags.
        /// </summary>
        ValidEpisodeTag,
        /// <summary>
        /// Only a VBox compatible episode tags.
        /// </summary>
        VBoxEpisodeTag,
        /// <summary>
        /// Import the data to WMC.
        /// </summary>
        WmcImport,
        /// <summary>
        /// Mark 4 star programmes as special for WMC.
        /// </summary>
        WmcStarSpecial,
    }

    /// <summary>
    /// The identifiers used with the TraceId parameter.
    /// </summary>
    public enum TraceName
    {
        /// <summary>
        /// Log new channels.
        /// </summary>
        AddChannel,
        /// <summary>
        /// Log BDA information.
        /// </summary>
        Bda,
        /// <summary>
        /// Log BDA signal statistics.
        /// </summary>
        BdaSigStats,
        /// <summary>
        /// Dump BellTV sections.
        /// </summary>
        BellTvSections,
        /// <summary>
        /// Log the bouquet data.
        /// </summary>
        BouquetSections,
        /// <summary>
        /// Log continuity errors.
        /// </summary>
        ContinuityErrors,
        /// <summary>
        /// Log the D3 descriptor.
        /// </summary>
        DescriptorD3,
        /// <summary>
        /// Log Dish Network data.
        /// </summary>
        DishNetworkSections,
        /// <summary>
        /// Log the completion status of DSMCC blocks.
        /// </summary>
        DsmccComplete,
        /// <summary>
        /// Log the DSMCC directory layout.
        /// </summary>
        DsmccDirLayout,
        /// <summary>
        /// Log the contents of DSMCC files.
        /// </summary>
        DsmccDumpFiles,
        /// <summary>
        /// Log the contents of the DSMCC EPG files.
        /// </summary>
        DsmccFile,
        /// <summary>
        /// Log the DSMCC modules.
        /// </summary>
        DsmccModules,
        /// <summary>
        /// Log the DSMCC EPG fields.
        /// </summary>
        DsmccRecLayout,
        /// <summary>
        /// Log the DSMCC records.
        /// </summary>
        DsmccRecord,
        /// <summary>
        /// Dump the MHW1/2 category data.
        /// </summary>
        DumpCategorySections,
        /// <summary>
        /// Dump the MHW1/2 channel data.
        /// </summary>
        DumpChannelSections,
        /// <summary>
        /// Dump the MHW1/2 summary sections.
        /// </summary>
        DumpSummarySections,
        /// <summary>
        /// Dump the MHW1/2 title sections.
        /// </summary>
        DumpTitleSections,
        /// <summary>
        /// Log programme repeats.
        /// </summary>
        DuplicatesFlagged,
        /// <summary>
        /// Log the EIT format control bytes.
        /// </summary>
        EitControlBytes,
        /// <summary>
        /// Dump the FreeSat data.
        /// </summary>
        FreeSatSections,
        /// <summary>
        /// Log generic descriptors.
        /// </summary>
        GenericDescriptor,
        /// <summary>
        /// Log only generic descriptors.
        /// </summary>
        GenericDescriptorOnly,
        /// <summary>
        /// Only log the protocol for OpenTV.
        /// </summary>
        GenericOpenTvRecord,
        /// <summary>
        /// Log metadata lookup results.
        /// </summary>
        LookupName,
        /// <summary>
        /// Log metadata lookup results for specified name.
        /// </summary>
        Lookups,
        /// <summary>
        /// Log metadata lookup details.
        /// </summary>
        LookupsDetail,
        /// <summary>
        /// Log metadata lookup errors.
        /// </summary>
        LookupsError,
        /// <summary>
        /// Dump the MPEG2 data packets.
        /// </summary>
        Mpeg2Packets,
        /// <summary>
        /// Log the MPEG2 sections stored and ignored.
        /// </summary>
        Mpeg2SectionsStored,
        /// <summary>
        /// Log the PID's used.
        /// </summary>
        PidHandler,
        /// <summary>
        /// Log the data blocks created by the PID handlers.
        /// </summary>
        PidHandlerBlocks,
        /// <summary>
        /// Log the PID activity for service information.
        /// </summary>
        PidHandlerSi,
        /// <summary>
        /// Log the PID numbers used.
        /// </summary>
        PidNumbers,
        /// <summary>
        /// Log the names of DSMCC PNG files.
        /// </summary>
        PngNames,
        /// <summary>
        /// Dump the protocol.
        /// </summary>
        Protocol,
        /// <summary>
        /// Set the PSIFilter up for logging.
        /// </summary>
        PsiFilter,
        /// <summary>
        /// Log the service entries in Australian MHEG5 data.
        /// </summary>
        ServiceEntries,
        /// <summary>
        /// Log the transport packets.
        /// </summary>
        TransportPackets,
        /// <summary>
        /// Log the packets from a transport stream file.
        /// </summary>
        TsFilePackets,
        /// <summary>
        /// Log the packets if they are relevant.
        /// </summary>
        TsPidPackets        
    }

    /// <summary>
    /// The identifiers used with the DebugId parameter.
    /// </summary>
    public enum DebugName
    {
        /// <summary>
        /// Adjust programme start times.
        /// </summary>
        AdjustStartTimes,
        /// <summary>
        /// Dump OpenTV data as a bit pattern.
        /// </summary>
        BitPattern,
        /// <summary>
        /// Extract the CanalSat zip files from the DSMCC carousel.
        /// </summary>
        CanalSatEpg,
        /// <summary>
        /// Process unknown carousels.
        /// </summary>
        Carousels,
        /// <summary>
        /// Generate a category cross-reference.
        /// </summary>
        CatXref,
        /// <summary>
        /// Create a satellite reference file.
        /// </summary>
        CreateSatIni,
        /// <summary>
        /// Create channels if they don't exist.
        /// </summary>        
        DontLogGaps,
        /// <summary>
        /// Don't log programme start time overlaps.
        /// </summary>
        DontLogOverlaps,
        /// <summary>
        /// Finish MHEG5 collections even if the data is incomplete.
        /// </summary>
        DsmccIgnoreIncomplete,
        /// <summary>
        /// Dump AIT sections.
        /// </summary>
        DumpAitSections,
        /// <summary>
        /// Dump EIT sections.
        /// </summary>
        DumpEitSections,
        /// <summary>
        /// Dump OpenTV summary sections.
        /// </summary>
        DumpOpenTvSummarySections,
        /// <summary>
        /// Dump the service description data blocks.
        /// </summary>
        DumpSdtBlock,
        /// <summary>
        /// Log the contents of an EIT Zip file.
        /// </summary>
        EitZipContents,
        /// <summary>
        /// Log the result of an episode metadata lookup.
        /// </summary>        
        EpisodeResult,
        /// <summary>
        /// Dump the ATSC event information table.
        /// </summary>
        EventInformationTable,
        /// <summary>
        /// Create an extended log file (up to 64mb).
        /// </summary>
        ExtendedLogFile,
        /// <summary>
        /// Dump the ATSC extended text table.
        /// </summary>
        ExtendedTextTable,
        /// <summary>
        /// Dump unspecified FeeSat sections.
        /// </summary>
        GetOtherSections,
        /// <summary>
        /// Switch off the xml character checking.
        /// </summary>
        IgnoreXmlChars,
        /// <summary>
        /// Log titles and descriptions that should be combined.
        /// </summary>
        LogBrokenTitles,
        /// <summary>
        /// Log the CA data blocks.
        /// </summary>
        LogCaData,
        /// <summary>
        /// Log an OpenTV category.
        /// </summary>
        LogCatEvent,
        /// <summary>
        /// Log all channels.
        /// </summary>
        LogChannels,
        /// <summary>
        /// Log the channel information in detail.
        /// </summary>
        LogChannelData,
        /// <summary>
        /// Log the channel groups (OpenTV only).
        /// </summary>
        LogChannelGroups,
        /// <summary>
        /// Log the codepages used.
        /// </summary>
        LogCodepages,
        /// <summary>
        /// Log the programme descriptions.
        /// </summary>
        LogDescriptions,
        /// <summary>
        /// Log descriptors.
        /// </summary>
        LogDescriptorData,
        /// <summary>
        /// Log the progress of the DVBViewer import process.
        /// </summary>
        LogDvbViewerImport,
        /// <summary>
        /// Log EPG linkage information.
        /// </summary>
        LogEpgLinkage,
        /// <summary>
        /// Log episode information.
        /// </summary>
        LogEpisodeInfo,
        /// <summary>
        /// Log OpenTV extended descriptions.
        /// </summary>
        LogExtendedDescriptions,
        /// <summary>
        /// Log escaped Huffman strings.
        /// </summary>
        LogHuffman,
        /// <summary>
        /// Set the Sat>IP log level.
        /// </summary>
        LogLevel,
        /// <summary>
        /// Log incomplete EIT entries.
        /// </summary>
        LogIncompleteEit,
        /// <summary>
        /// Log merged channels.
        /// </summary>
        LogMergedChannels,
        /// <summary>
        /// Log the series information from an MXF file.
        /// </summary>
        LogMxfSeries,
        /// <summary>
        /// Log the network data.
        /// </summary>
        LogNetwork,
        /// <summary>
        /// Log the network map.
        /// </summary>
        LogNetworkMap,
        /// <summary>
        /// Log the BellTV or Dish Network original description.
        /// </summary>
        LogOriginal,
        /// <summary>
        /// Log descriptors that are out of scope.
        /// </summary>
        LogOutOfScope,
        /// <summary>
        /// Log the program map table.
        /// </summary>
        LogPmt,
        /// <summary>
        /// Dump the ATSC extended text data.
        /// </summary>
        LogPsipExtendedText,
        /// <summary>
        /// Log the string used to create uids for programmes in an MXF file.
        /// </summary>
        LogPuids,
        /// <summary>
        /// Log the response keys from lookup requests.
        /// </summary>
        LogResponseKeys,
        /// <summary>
        /// Log the use of season/episode Id's.
        /// </summary>
        LogSeIds,
        /// <summary>
        /// Log the use of season/episode CRId's.
        /// </summary>
        LogSeCrids,
        /// <summary>
        /// Log the stream data.
        /// </summary>
        LogStreamInfo,
        /// <summary>
        /// Log the programme titles.
        /// </summary>
        LogTitles,
        /// <summary>
        /// Log undefined OpenTV records.
        /// </summary>
        LogUndefinedRecords,
        /// <summary>
        /// Log unknown OpenTV records.
        /// </summary>
        LogUnknownRecords,
        /// <summary>
        /// Dump the ATSC master guide table.
        /// </summary>
        MasterGuideTable,
        /// <summary>
        /// Set the MHEG5 PID.
        /// </summary>
        Mheg5Pid,
        /// <summary>
        /// Dump MHW1 category sections.
        /// </summary>
        Mhw1CategorySections,
        /// <summary>
        /// Log missing MHW2 summary sections.
        /// </summary>
        Mhw2SummaryMissing,
        /// <summary>
        /// Log unknown MHW2 sections.
        /// </summary>
        Mhw2Unknown,
        /// <summary>
        /// Dump NagraGuide data.
        /// </summary>
        NagraBlocks,
        /// <summary>
        /// Process NDS data.
        /// </summary>
        NDS,
        /// <summary>
        /// Log NDS binXml data.
        /// </summary>
        NDSBinXml,
        /// <summary>
        /// Log NDS binXml block data.
        /// </summary>
        NDSBinXmlBlocks,
        /// <summary>
        /// Log NDS SQL event data.
        /// </summary>
        NDSSqlEvents,
        /// <summary>
        /// Log NDS SQL group data.
        /// </summary>
        NDSSqlGroups,
        /// <summary>
        /// Log NDS SQL service data.
        /// </summary>
        NDSSqlServices,
        /// <summary>
        /// Log NDS SQL string data.
        /// </summary>
        NDSSqlStrings,
        /// <summary>
        /// Dump network information table sections.
        /// </summary>
        NitSections,
        /// <summary>
        /// Don't output log messages to the console window.
        /// </summary>
        NotQuiet,
        /// <summary>
        /// WMC is not present.
        /// </summary>
        NoWmc,
        /// <summary>
        /// Dump the data from other sections.
        /// </summary>
        OtherSections,
        /// <summary>
        /// Dump the ATSC rating region data.
        /// </summary>
        RatingRegionTable,
        /// <summary>
        /// Log replays.
        /// </summary>
        Replays,
        /// <summary>
        /// Don't delete the EIT carousel zip data.
        /// </summary>
        RetainZipData,
        /// <summary>
        /// Dump SiehFern data.
        /// </summary>
        ShowColons,
        /// <summary>
        /// Dump SiehFern data.
        /// </summary>
        SiehfernBlocks,
        /// <summary>
        /// Dump SiehFern channel data.
        /// </summary>
        SiehfernChannelBlocks,
        /// <summary>
        /// Dump SiehFern EPG blocks.
        /// </summary>
        SiehfernEpgBlocks,
        /// <summary>
        /// Dump SiehFern EPG details.
        /// </summary>
        SiehfernEpgDetail,
        /// <summary>
        /// Log the contents of the stack.
        /// </summary>
        StackTrace,
        /// <summary>
        /// Dump the OpenTV title data.
        /// </summary>
        TitleSection,        
        /// <summary>
        /// Log unknown descriptors.
        /// </summary>        
        UnknownDescriptors,
        /// <summary>
        /// Update the channel information.
        /// </summary>
        UpdateChannels,
        /// <summary>
        /// Update the station information.
        /// </summary>
        UpdateStation,
        /// <summary>
        /// Use DVBLink virtual tuners.
        /// </summary>
        UseDvbLink,
        /// <summary>
        /// Use the specific network provider.
        /// </summary>
        UseSpecificNp,
        /// <summary>
        /// Dump the ATSC virtual channel data.
        /// </summary>
        VirtualChannelTable,
        /// <summary>
        /// Create new WMC channels if they don't exist.
        /// </summary>
        WmcNewChannels
    }

    /// <summary>
    /// The level to be used in equality testing.
    /// </summary>
    public enum EqualityLevel
    {
        /// <summary>
        /// All properties must be checked.
        /// </summary>
        Entirely,
        /// <summary>
        /// Only identification fields are checked.
        /// </summary>
        Identity,
    }

    /// <summary>
    /// The streaming server type.
    /// </summary>
    public enum StreamServerType
    {
        /// <summary>
        /// Undefined.
        /// </summary>
        Any,
        /// <summary>
        /// Sat>IP.
        /// </summary>
        SatIP,
        /// <summary>
        /// VBox.
        /// </summary>
        VBox,
    }
}
