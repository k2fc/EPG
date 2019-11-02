using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace DVBServices
{
    internal class NdsDecoderExtSubstitutableElementTable
    {
        internal bool IsThereExternallySubstitutableType { get; private set; }
        internal bool AllTypeExternallySubstitutable { get; private set; }
        internal Collection<int> ExternallySubstitutableTypes { get; private set; }

        internal NdsDecoderExtSubstitutableElementTable() { }

        internal void Process(byte[] byteData, ref int byteIndex, ref int bitIndex, int globalElementsCount)
        {
            IsThereExternallySubstitutableType = NdsUtils.GetBool(byteData, ref byteIndex, ref bitIndex);

            if (IsThereExternallySubstitutableType)
            {
                AllTypeExternallySubstitutable = NdsUtils.GetBool(byteData, ref byteIndex, ref bitIndex);

                if (AllTypeExternallySubstitutable)
                {
                    int typeCount = NdsUtils.GetVluimsbf5(byteData, ref byteIndex, ref bitIndex);

                    if (typeCount != 0)
                    {
                        ExternallySubstitutableTypes = new Collection<int>();

                        while (ExternallySubstitutableTypes.Count != typeCount)
                            ExternallySubstitutableTypes.Add(NdsUtils.GetUimsbf(byteData, ref byteIndex, ref bitIndex, NdsUtils.GetCeilLog2(globalElementsCount)));
                    }
                }
            }
        }
    }
}
