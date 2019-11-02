using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DVBServices
{
    internal sealed class NdsUtils
    {
        private static byte[] maskArray = new byte[] { 0x80, 0x40, 0x20, 0x10, 0x08, 0x04, 0x02, 0x01 };

        private NdsUtils() { }

        internal static string GetString(byte[] byteData, ref int index, ref int bitIndex, int byteCount)
        {
            byte[] extractedBytes = GetBits(byteData, ref index, ref bitIndex, byteCount * 8);
            return Encoding.UTF8.GetString(extractedBytes);
        }

        internal static bool GetBool(byte[] byteData, ref int index, ref int bitIndex)
        {
            bool reply = (byteData[index] & maskArray[bitIndex]) != 0;

            bitIndex++;

            if (bitIndex > 7)
            {
                index++;
                bitIndex = 0;
            }

            return reply;
        }

        internal static byte[] GetBslbf(byte[] byteData, ref int index, ref int bitIndex, int bitCount)
        {
            return GetBits(byteData, ref index, ref bitIndex, bitCount);
        }

        internal static int GetUimsbf(byte[] byteData, ref int index, ref int bitIndex, int bitCount)
        {
            int result = 0;

            while (bitCount > 0)
            {
                result = (result << 1) | ((byteData[index] & maskArray[bitIndex]) != 0 ? 1 : 0);
                
                bitIndex++;

                if (bitIndex > 7)
                {
                    index++;
                    bitIndex = 0;
                }

                bitCount--;
            }

            return result;
        }

        internal static byte[] GetVlclbf(byte[] byteData, ref int index, ref int bitIndex, int bitCount)
        {
            return GetBits(byteData, ref index, ref bitIndex, bitCount);
        }

        internal static int GetVluimsbf8(byte[] byteData, ref int index, ref int bitIndex)
        {
            int reply = 0;
            bool done = false;
            
            do
            {
                byte[] currentByte = GetBits(byteData, ref index, ref bitIndex, 8);
                reply = (reply * 128) + (currentByte[0] & 0x7f);

                done = (currentByte[0] & 0x80) == 0;
            }
            while (!done);
            
            return reply;
        }

        internal static int GetVluimsbf5(byte[] byteData, ref int index, ref int bitIndex)
        {
            int length = 1;
            bool lengthDone = false;

            do
            {
                if (GetBool(byteData, ref index, ref bitIndex))
                    length++;
                else
                    lengthDone = true;

            }
            while (!lengthDone);

            int reply = 0;            

            do
            {
                byte[] currentByte = GetBits(byteData, ref index, ref bitIndex, 4);
                reply = (reply * 16) + (currentByte[0] >> 4);

                length--;
            }
            while (length > 0);

            return reply;
        }

        internal static int GetCeilLog2(int value)
        {
            return (int)Math.Ceiling(Math.Log(value, 2));
        }

        internal static byte[] GetBits(byte[] byteData, ref int index, ref int bitIndex, int bitCount)
        {
            int byteCount = bitCount / 8;
            if (bitCount % 8 != 0)
                byteCount++;

            byte[] reply = new byte[byteCount];
            int currentByteIndex = 0;
            byte currentByte = 0;
            int currentBitIndex = 0;

            while (bitCount > 0)
            {
                if (currentBitIndex == bitIndex)
                    currentByte |= (byte)(byteData[index] & maskArray[bitIndex]);
                else
                {
                    if ((byteData[index] & maskArray[bitIndex]) != 0)
                        currentByte |= maskArray[currentBitIndex];
                }

                bitCount--;
                bitIndex++;

                if (bitIndex > 7)
                {
                    index++;
                    bitIndex = 0;
                }

                currentBitIndex++;

                if (currentBitIndex > 7)
                {
                    reply[currentByteIndex] = currentByte;
                    currentByteIndex++;

                    currentByte = 0;
                    currentBitIndex = 0;
                }
            }

            if (currentBitIndex != 0)
                reply[currentByteIndex] = currentByte;

            return reply;
        }
    }
}
