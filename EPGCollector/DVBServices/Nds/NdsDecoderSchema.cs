using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DVBServices
{
    internal class NdsDecoderSchema
    {
        internal string SchemaUri { get; private set; }
        internal string LocationHint { get; private set; }

        internal Collection<NdsDecoderTypeCodec> TypeCodecs { get; private set; }

        internal NdsDecoderSchema() { }

        internal void Process(byte[] byteData, ref int byteIndex, ref int bitIndex)
        {
            int urlLength = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);
            if (urlLength != 0)
                SchemaUri = NdsUtils.GetString(byteData, ref byteIndex, ref bitIndex, urlLength);

            int locationHintLength = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);
            if (locationHintLength != 0)
                LocationHint = NdsUtils.GetString(byteData, ref byteIndex, ref bitIndex, locationHintLength);

            int typeCodecsCount = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);

            if (typeCodecsCount != 0)
            {
                TypeCodecs = new Collection<NdsDecoderTypeCodec>();

                while (TypeCodecs.Count != typeCodecsCount)
                {
                    NdsDecoderTypeCodec typeCodec = new NdsDecoderTypeCodec();
                    typeCodec.Process(byteData, ref byteIndex, ref bitIndex);

                    TypeCodecs.Add(typeCodec);
                }
            }
        }
    }
}
