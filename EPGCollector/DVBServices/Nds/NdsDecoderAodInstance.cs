using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DVBServices
{
    internal class NdsDecoderAodInstance
    {
        internal byte[] Type { get; private set; }
        internal byte[] Parameters { get; private set; }
        
        internal NdsDecoderAodInstance() { }

        internal void Process(byte[] byteData, ref int byteIndex, ref int bitIndex, int decodersCount)
        {
            int instanceLength = NdsUtils.GetVluimsbf8(byteData, ref byteIndex, ref bitIndex);
            
            Type = NdsUtils.GetBits(byteData, ref byteIndex, ref bitIndex, NdsUtils.GetCeilLog2(decodersCount));
            Parameters = NdsUtils.GetBits(byteData, ref byteIndex, ref bitIndex, (instanceLength - NdsUtils.GetCeilLog2(decodersCount)));

            if (bitIndex != 0)
            {
                byteIndex++;
                bitIndex = 0;
            }
        }
    }
}
