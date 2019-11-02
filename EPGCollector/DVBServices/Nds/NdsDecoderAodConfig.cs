using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DVBServices
{
    internal class NdsDecoderAodConfig
    {
        internal Collection<NdsDecoderAodInstance> DecoderInstances { get; private set; }
        internal Collection<NdsDecoderAodMapping> DecoderMappings { get; private set; }

        internal NdsDecoderAodConfig() { }

        internal void Process(byte[] byteData, ref int byteIndex, ref int bitIndex, int schemaCount, int decodersCount)
        {
            int instanceCount = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);
            if (instanceCount != 0)
            {
                DecoderInstances = new Collection<NdsDecoderAodInstance>();

                while (DecoderInstances.Count != instanceCount)
                {
                    NdsDecoderAodInstance instance = new NdsDecoderAodInstance();
                    instance.Process(byteData, ref byteIndex, ref bitIndex, decodersCount);

                    DecoderInstances.Add(instance);
                }
            }

            int mappingCount = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);
            if (mappingCount != 0)
            {
                DecoderMappings = new Collection<NdsDecoderAodMapping>();

                while (DecoderMappings.Count != mappingCount)
                {
                    NdsDecoderAodMapping mapping = new NdsDecoderAodMapping();
                    mapping.Process(byteData, ref byteIndex, ref bitIndex, schemaCount, decodersCount);

                    DecoderMappings.Add(mapping);
                }
            }
        }
    }
}
