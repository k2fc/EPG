using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DVBServices
{
    internal class NdsDecoderContextPath
    {
        internal byte[] ContextPath { get; private set; }
        internal byte[] ContextPathCode { get; private set; }

        internal NdsDecoderContextPath() { }

        internal void Process(byte[] byteData, ref int byteIndex, ref int bitIndex, bool completeTable, int codeLength)
        {
            int contextPathLength = NdsUtils.GetVluimsbf5(byteData, ref byteIndex, ref bitIndex);
            if (contextPathLength != 0)
                ContextPath = NdsUtils.GetBits(byteData, ref byteIndex, ref bitIndex, contextPathLength);

            if (!completeTable)
                ContextPathCode = NdsUtils.GetBits(byteData, ref byteIndex, ref bitIndex, codeLength);
        }
    }
}
