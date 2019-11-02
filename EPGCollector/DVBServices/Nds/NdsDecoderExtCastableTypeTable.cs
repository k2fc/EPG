using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DVBServices
{
    internal class NdsDecoderExtCastableTypeTable
    {
        internal bool IsThereExternallyCastableType { get; private set; }
        internal bool AllTypeExternallyCastable { get; private set; }
        internal Collection<int> ExternallyCastableTypes { get; private set; }

        internal NdsDecoderExtCastableTypeTable() { }

        internal void Process(byte[] byteData, ref int byteIndex, ref int bitIndex, int globalTypesCount)
        {
            IsThereExternallyCastableType = NdsUtils.GetBool(byteData, ref byteIndex, ref bitIndex);

            if (IsThereExternallyCastableType)
            {
                AllTypeExternallyCastable = NdsUtils.GetBool(byteData, ref byteIndex, ref bitIndex);
                
                if (AllTypeExternallyCastable)
                {
                    int typeCount = NdsUtils.GetVluimsbf5(byteData, ref byteIndex, ref bitIndex);

                    if (typeCount != 0)
                    {
                        ExternallyCastableTypes = new Collection<int>();

                        while (ExternallyCastableTypes.Count != typeCount)
                            ExternallyCastableTypes.Add(NdsUtils.GetUimsbf(byteData, ref byteIndex, ref bitIndex, NdsUtils.GetCeilLog2(globalTypesCount)));
                    }
                }
            }
        }
    }
}
