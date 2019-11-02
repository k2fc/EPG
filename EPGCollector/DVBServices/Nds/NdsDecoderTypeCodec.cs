using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DVBServices
{
    internal class NdsDecoderTypeCodec
    {
        internal string TypeCodecUri { get; private set; }
        internal Collection<int> TypeIdentificationCodes { get; private set; }

        internal NdsDecoderTypeCodec() { }

        internal void Process(byte[] byteData, ref int byteIndex, ref int bitIndex)
        {
            int uriLength = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);
            if (uriLength != 0)
                TypeCodecUri = NdsUtils.GetString(byteData, ref byteIndex, ref bitIndex, uriLength);

            int typeCount = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);

            if (typeCount != 0)
            {
                TypeIdentificationCodes = new Collection<int>();

                while (TypeIdentificationCodes.Count != typeCount)
                    TypeIdentificationCodes.Add(NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex));                
            }
        }
    }
}
