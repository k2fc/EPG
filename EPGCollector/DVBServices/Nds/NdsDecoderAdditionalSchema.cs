using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DVBServices
{
    internal class NdsDecoderAdditionalSchema
    {
        internal int SchemaId { get; private set; }
        internal string SchemaUri { get; private set; }
        internal string BinaryLocationHint { get; private set; }
        internal Collection<NdsDecoderTypeCodec> TypeCodecs { get; private set; }
        internal NdsDecoderExtCastableTypeTable CastableTypeTable { get; private set; }
        internal NdsDecoderExtSubstitutableElementTable SubstitutableElementTable { get; private set; }

        internal NdsDecoderAdditionalSchema() { }

        internal void Process(byte[] byteData, ref int byteIndex, ref int bitIndex)
        {
            SchemaId = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);

            int uriLength = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);
            if (uriLength != 0)
                SchemaUri = NdsUtils.GetString(byteData, ref byteIndex, ref bitIndex, uriLength);

            int hintLength = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);
            if (hintLength != 0)
                BinaryLocationHint = NdsUtils.GetString(byteData, ref byteIndex, ref bitIndex, uriLength);

            int typeCodecCount = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);
            if (typeCodecCount != 0)
            {
                TypeCodecs = new Collection<NdsDecoderTypeCodec>();

                while (TypeCodecs.Count != typeCodecCount)
                {
                    NdsDecoderTypeCodec typeCodec = new NdsDecoderTypeCodec();
                    typeCodec.Process(byteData, ref byteIndex, ref bitIndex);

                    TypeCodecs.Add(typeCodec);
                }
            }

            CastableTypeTable = new NdsDecoderExtCastableTypeTable();
            CastableTypeTable.Process(byteData, ref byteIndex, ref bitIndex, 1);

            SubstitutableElementTable = new NdsDecoderExtSubstitutableElementTable();
            SubstitutableElementTable.Process(byteData, ref byteIndex, ref bitIndex, 1);
        }
    }
}
