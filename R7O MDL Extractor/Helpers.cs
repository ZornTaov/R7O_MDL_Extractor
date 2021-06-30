using System;

namespace R7O_MDL_Extractor
{
    public class Helpers
    {
        public static int Search(byte[] src, byte[] pattern, int offset)
        {
            int maxFirstCharSlot = src.Length - pattern.Length + 1;
            int j;
            for (int i = offset; i < maxFirstCharSlot; i++)
            {
                if (src[i] != pattern[0]) continue;//comp only first byte

                // found a match on first byte, it tries to match rest of the pattern
                for (j = pattern.Length - 1; j >= 1 && src[i + j] == pattern[j]; j--) ;
                if (j == 0) 
                    return i;
            }
            return -1;
        }
        public static float toTwoByteFloat(byte LO, byte HO)
        {
            var intVal = BitConverter.ToInt32(new byte[] { HO, LO, 0, 0 }, 0);

            int mant = intVal & 0x03ff;
            int exp = intVal & 0x7c00;
            if (exp == 0x7c00) exp = 0x3fc00;
            else if (exp != 0)
            {
                exp += 0x1c000;
                if (mant == 0 && exp > 0x1c400)
                    return BitConverter.ToSingle(BitConverter.GetBytes((intVal & 0x8000) << 16 | exp << 13 | 0x3ff), 0);
            }
            else if (mant != 0)
            {
                exp = 0x1c400;
                do
                {
                    mant <<= 1;
                    exp -= 0x400;
                } while ((mant & 0x400) == 0);
                mant &= 0x3ff;
            }
            return BitConverter.ToSingle(BitConverter.GetBytes((intVal & 0x8000) << 16 | (exp | mant) << 13), 0);
        }
    }
}
