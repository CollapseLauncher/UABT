using System;

namespace Hi3Helper.UABT.LZ4
{
    public static partial class LZ4CodecHelper
    {
        public static int Encode(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, int outputLength)
        {
            if (IntPtr.Size == 4)
            {
                return Encode32(input, inputOffset, inputLength, output, outputOffset, outputLength);
            }
            return Encode64(input, inputOffset, inputLength, output, outputOffset, outputLength);
        }

        public static byte[] EncodeHC(byte[] input, int inputOffset, int inputLength)
        {
            if (IntPtr.Size == 4)
            {
                return Encode32HC(input, inputOffset, inputLength);
            }
            return Encode64HC(input, inputOffset, inputLength);
        }

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
