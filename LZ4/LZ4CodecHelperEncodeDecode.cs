using System;

namespace Hi3Helper.UABT.LZ4
{
    public static partial class LZ4CodecHelper
    {
        public static int Decode(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, int outputLength)
        {
            if (IntPtr.Size == 4)
            {
                return Decode32(input, inputOffset, inputLength, output, outputOffset, outputLength, true);
            }
            return Decode64(input, inputOffset, inputLength, output, outputOffset, outputLength, true);
        }
    }
}
