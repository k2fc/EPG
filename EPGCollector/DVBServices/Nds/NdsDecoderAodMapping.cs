using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DVBServices
{
    internal class NdsDecoderAodMapping
    {
        internal bool PreserveDefaultDecoderInMapping { get; private set; }
        internal Collection<int> AodInstanceIds { get; private set; }
        internal Collection<NdsDecoderAodMappingType> AodMappingTypes { get; private set; }

        internal NdsDecoderAodMapping() { }

        internal void Process(byte[] byteData, ref int byteIndex, ref int bitIndex, int schemaCount, int decoderInstances)
        {
            PreserveDefaultDecoderInMapping = NdsUtils.GetBool(byteData, ref byteIndex, ref bitIndex);
            byte[] reserved = NdsUtils.GetBits(byteData, ref byteIndex, ref bitIndex, 7);

            int idMappingCount = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);

            if (idMappingCount != 0)
            {
                AodInstanceIds = new Collection<int>();

                while (AodInstanceIds.Count != idMappingCount)
                    AodInstanceIds.Add(NdsUtils.GetUimsbf(byteData, ref byteIndex, ref bitIndex, NdsUtils.GetCeilLog2(decoderInstances)));
            }

            int typesInMappingCount = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);

            if (typesInMappingCount != 0)
            {
                AodMappingTypes = new Collection<NdsDecoderAodMappingType>();

                while (AodMappingTypes.Count != typesInMappingCount)
                {
                    NdsDecoderAodMappingType mappingType = new NdsDecoderAodMappingType();
                    mappingType.Process(byteData, ref byteIndex, ref bitIndex, schemaCount);

                    AodMappingTypes.Add(mappingType);
                }
            }
        }
    }
}
