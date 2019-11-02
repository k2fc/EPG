using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DVBServices
{
    internal class NdsDecoderContextPathTable
    {
        internal Collection<NdsDecoderContextPath> ContextPaths { get; private set; }
        internal bool CompleteContextPathTable { get; private set; }

        internal NdsDecoderContextPathTable() { }

        internal void Process(byte[] byteData, ref int byteIndex, ref int bitIndex)
        {
            int tableLength = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);
            if (tableLength != 0)
            {
                int codeLength = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);

                int contextPathCount = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);
                CompleteContextPathTable = NdsUtils.GetBool(byteData, ref byteIndex, ref bitIndex);

                ContextPaths = new Collection<NdsDecoderContextPath>();

                while (ContextPaths.Count != contextPathCount)
                {
                    NdsDecoderContextPath contextPath = new NdsDecoderContextPath();
                    contextPath.Process(byteData, ref byteIndex, ref bitIndex, CompleteContextPathTable, codeLength);

                    ContextPaths.Add(contextPath);
                }
            }

            if (bitIndex != 0)
            {
                byteIndex++;
                bitIndex = 0;
            }
        }
    }
}
