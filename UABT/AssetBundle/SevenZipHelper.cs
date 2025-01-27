using SevenZip.Compression.LZMA;
using System;
using System.IO;

namespace Hi3Helper.UABT
{
    public static class SevenZipHelper
    {
        public static MemoryStream StreamDecompress(MemoryStream inStream)
        {
            Decoder decoder = new();
            inStream.Seek(0L, SeekOrigin.Begin);
            MemoryStream memoryStream = new();
            byte[]       array        = new byte[5];
            if (inStream.Read(array, 0, 5) != 5)
            {
                throw new Exception("input .lzma is too short");
            }
            long num = 0L;
            for (int i = 0; i < 8; i++)
            {
                int num2 = inStream.ReadByte();
                if (num2 < 0)
                {
                    throw new Exception("Can't Read 1");
                }
                num |= (long)((ulong)(byte)num2 << 8 * i);
            }
            decoder.SetDecoderProperties(array);
            long inSize = inStream.Length - inStream.Position;
            decoder.Code(inStream, memoryStream, inSize, num, null);
            memoryStream.Position = 0L;
            return memoryStream;
        }

        public static void StreamDecompress(Stream inStream, Stream outStream, long inSize, long outSize)
        {
            Decoder decoder = new();
            byte[]  array   = new byte[5];
            if (inStream.Read(array, 0, 5) != 5)
            {
                throw new Exception("input .lzma is too short");
            }
            decoder.SetDecoderProperties(array);
            inSize -= 5;
            decoder.Code(inStream, outStream, inSize, outSize, null);
        }
    }
}
