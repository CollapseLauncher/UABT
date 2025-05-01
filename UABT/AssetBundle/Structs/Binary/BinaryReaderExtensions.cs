using Hi3Helper.Data;
using System;
using System.Buffers;
using System.IO;
using System.Text;
// ReSharper disable IdentifierTypo

namespace Hi3Helper.UABT.Binary
{
    public static class BinaryReaderExtensions
    {
        public static void AlignStream(this BinaryReader reader, int alignment)
        {
            long num = reader.BaseStream.Position % alignment;
            if (num != 0L)
            {
                reader.BaseStream.Position += alignment - num;
            }
        }

        public static string ReadAlignedString(this BinaryReader reader)
        {
            return reader.ReadAlignedString(reader.ReadInt32());
        }

        public static string ReadAlignedString(this BinaryReader reader, int length)
        {
            if (length <= 0 || length > reader.BaseStream.Length - reader.BaseStream.Position)
            {
                return "";
            }

            byte[] bytes   = reader.ReadBytes(length);
            string @string = Encoding.UTF8.GetString(bytes);
            reader.AlignStream(4);
            return @string;
        }

        public static string ReadStringToNull(this BinaryReader reader, int bufferSize = 1 << 10)
        {
            byte[] buffer = ArrayPool<byte>.Shared.Rent(bufferSize);

            try
            {
                byte item;
                int count = 0;
                while ((item = reader.ReadByte()) != 0)
                {
                    if (count > bufferSize)
                        throw new IndexOutOfRangeException($"The string has the size that's more than allowed limit for the buffer: {ConverterTool.SummarizeSizeSimple(bufferSize)}");

                    buffer[count++] = item;
                }
                return Encoding.UTF8.GetString(buffer.AsSpan(0, count));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
