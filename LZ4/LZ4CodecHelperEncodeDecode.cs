// ReSharper disable IdentifierTypo
namespace Hi3Helper.UABT.LZ4
{
    public static partial class Lz4CodecHelper
    {
        public static int Decode(byte[] input, int inputOffset, int inputLength, byte[] output, int outputOffset, int outputLength)
        {
            return nint.Size == 4 ? Decode32(input, inputOffset, inputLength, output, outputOffset, outputLength, true) : Decode64(input, inputOffset, inputLength, output, outputOffset, outputLength, true);
        }
    }
}
