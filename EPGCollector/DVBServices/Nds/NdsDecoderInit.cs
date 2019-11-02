using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DVBServices
{
    internal class NdsDecoderInit
    {
        internal int SystemsProfileLevelIndication { get; private set; }
        internal int UnitSizeCode { get; private set; }
        internal bool NoAdvancedFeatures { get; private set; }
        
        internal bool InsertFlag { get; private set; }
        internal bool AdvancedOptimisedDecodersFlag { get; private set; }
        internal bool AdditionalSchemaFlag { get; private set; }
        internal bool AdditionalSchemaUpdatesOnlyFlag { get; private set; }
        internal bool FragmentReferenceFlag { get; private set; }
        internal bool MPCOnlyFlag { get; private set; }
        internal bool HierarchyBasedSubstitutionCodingFlag { get; private set; }
        internal bool ContextPathTableFlag { get; private set; }

        internal Collection<NdsDecoderSchema> Schemas { get; private set; }
        internal NdsDecoderContextPathTable ContextPathTable { get; private set; }

        internal Collection<string> AodUrls { get; private set; }
        internal NdsDecoderAodConfig AodConfig { get; private set; }

        internal Collection<int> SupportedFragmentReferenceFormats { get; private set; }
        internal NdsDecoderAdditionalSchemaConfig AdditionalSchemaConfig;

        internal byte[] InitialDocument { get; private set; }

        private int reservedBits;
        private int advancedFeatureFlagsLength;

        internal NdsDecoderInit() { }

        internal void Process(byte[] byteData, ref int byteIndex, ref int bitIndex)
        {
            SystemsProfileLevelIndication = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);
            UnitSizeCode = NdsUtils.GetUimsbf(byteData, ref byteIndex, ref bitIndex, 3);
            
            NoAdvancedFeatures = NdsUtils.GetBool(byteData, ref byteIndex, ref bitIndex);
            reservedBits = NdsUtils.GetUimsbf(byteData, ref byteIndex, ref bitIndex, 4);

            if (!NoAdvancedFeatures)
            {
                advancedFeatureFlagsLength = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);

                InsertFlag = NdsUtils.GetBool(byteData, ref byteIndex, ref bitIndex);
                AdvancedOptimisedDecodersFlag = NdsUtils.GetBool(byteData, ref byteIndex, ref bitIndex);
                AdditionalSchemaFlag = NdsUtils.GetBool(byteData, ref byteIndex, ref bitIndex);
                AdditionalSchemaUpdatesOnlyFlag = NdsUtils.GetBool(byteData, ref byteIndex, ref bitIndex);
                FragmentReferenceFlag = NdsUtils.GetBool(byteData, ref byteIndex, ref bitIndex);
                MPCOnlyFlag = NdsUtils.GetBool(byteData, ref byteIndex, ref bitIndex);
                HierarchyBasedSubstitutionCodingFlag = NdsUtils.GetBool(byteData, ref byteIndex, ref bitIndex);
                ContextPathTableFlag = NdsUtils.GetBool(byteData, ref byteIndex, ref bitIndex);

                byte[] zeroBits = NdsUtils.GetBits(byteData, ref byteIndex, ref bitIndex, (advancedFeatureFlagsLength * 8) - 8);
            }

            int schemaCount = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);

            if (schemaCount != 0)
            {
                Schemas = new Collection<NdsDecoderSchema>();

                while (Schemas.Count != schemaCount)
                {
                    NdsDecoderSchema decoderSchema = new NdsDecoderSchema();
                    decoderSchema.Process(byteData, ref byteIndex, ref bitIndex);

                    Schemas.Add(decoderSchema);
                }

            }

            if (ContextPathTableFlag)
            {
                ContextPathTable = new NdsDecoderContextPathTable();
                ContextPathTable.Process(byteData, ref byteIndex, ref bitIndex);
            }

            if (AdvancedOptimisedDecodersFlag)
            {
                int aodCount = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);

                if (aodCount != 0)
                {
                    AodUrls = new Collection<string>();

                    while (AodUrls.Count != aodCount)
                    {
                        int urlLength = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);
                        AodUrls.Add(NdsUtils.GetString(byteData, ref byteIndex, ref bitIndex, urlLength));
                    }
                }

                AodConfig = new NdsDecoderAodConfig();
                AodConfig.Process(byteData, ref byteIndex, ref bitIndex, Schemas.Count, aodCount);
            }

            if (FragmentReferenceFlag)
            {
                int formatCount = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);

                if (formatCount != 0)
                {
                    SupportedFragmentReferenceFormats = new Collection<int>();

                    while (SupportedFragmentReferenceFormats.Count != formatCount)
                        SupportedFragmentReferenceFormats.Add(NdsUtils.GetUimsbf(byteData, ref byteIndex, ref bitIndex, 8));
                }

            }

            if (AdditionalSchemaFlag)
            {
                AdditionalSchemaConfig = new NdsDecoderAdditionalSchemaConfig();
                AdditionalSchemaConfig.Process(byteData, ref byteIndex, ref bitIndex);
            }

            if (!AdditionalSchemaUpdatesOnlyFlag)
            {
                int initialDocumentLength = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);
                InitialDocument = NdsUtils.GetBits(byteData, ref byteIndex, ref bitIndex, initialDocumentLength * 8);
            }
        }
    }
}
