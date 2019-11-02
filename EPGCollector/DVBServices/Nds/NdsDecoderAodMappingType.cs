using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DVBServices
{
    internal class NdsDecoderAodMappingType
    {
        internal int SchemaId { get; private set; }
        internal int TypeIdentificationCode { get; private set; }

        internal NdsDecoderAodMappingType() { }

        internal void Process(byte[] byteData, ref int byteIndex, ref int bitIndex, int schemaCount)
        {
            SchemaId = NdsUtils.GetUimsbf(byteData, ref byteIndex, ref bitIndex, NdsUtils.GetCeilLog2(schemaCount));
            TypeIdentificationCode = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);
        }
    }
}
