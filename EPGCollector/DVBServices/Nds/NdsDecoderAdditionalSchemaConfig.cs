using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DVBServices
{
    internal class NdsDecoderAdditionalSchemaConfig
    {
        internal int NumberOfAdditionalSchemas { get; private set; }
        internal Collection<NdsDecoderAdditionalSchema> KnownAdditionalSchemas { get; private set; }
        internal int SchemaEncodingMethod { get; private set; }
        internal NdsDecoderExtCastableTypeTable CastableTypeTable { get; private set; }
        internal NdsDecoderExtSubstitutableElementTable SubstitutableElementTable { get; private set; }

        internal NdsDecoderAdditionalSchemaConfig() { }

        internal void Process(byte[] byteData, ref int byteIndex, ref int bitIndex)
        {
            NumberOfAdditionalSchemas = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);

            int knownSchemaCount = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);

            if (knownSchemaCount != 0)
            {
                KnownAdditionalSchemas = new Collection<NdsDecoderAdditionalSchema>();

                while (KnownAdditionalSchemas.Count != knownSchemaCount)
                {
                    NdsDecoderAdditionalSchema additionalSchema = new NdsDecoderAdditionalSchema();
                    additionalSchema.Process(byteData, ref byteIndex, ref bitIndex);

                    KnownAdditionalSchemas.Add(additionalSchema);
                }
            }

            SchemaEncodingMethod = NdsUtils.GetUimsbf(byteData, ref byteIndex, ref bitIndex, 8);

            CastableTypeTable = new NdsDecoderExtCastableTypeTable();
            CastableTypeTable.Process(byteData, ref byteIndex, ref bitIndex, 1);

            SubstitutableElementTable = new NdsDecoderExtSubstitutableElementTable();
            SubstitutableElementTable.Process(byteData, ref byteIndex, ref bitIndex, 1);

            byte[] reservedBits = NdsUtils.GetBits(byteData, ref byteIndex, ref bitIndex, 7); 
        }
    }
}
