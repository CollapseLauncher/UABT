using System;
using System.IO;
using System.Text;

namespace Hi3Helper.UABT.Binary
{
    public static class BinaryWriterExtensions
    {
        private static void WriteArray<T>(Action<T> del, T[] array)
        {
            foreach (T obj in array)
            {
                del(obj);
            }
        }

        public static void Write(this BinaryWriter writer, uint[] array)
        {
            WriteArray(writer.Write, array);
        }

        public static void AlignStream(this BinaryWriter writer, int alignment)
        {
            long num = writer.BaseStream.Position % alignment;
            if (num != 0L)
            {
                writer.Write(new byte[alignment - num]);
            }
        }

        public static void WriteAlignedString(this BinaryWriter writer, string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            writer.Write(bytes.Length);
            writer.Write(bytes);
            AlignStream(writer, 4);
        }

        public static void WriteStringToNull(this BinaryWriter writer, string str)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            writer.Write(bytes);
            writer.Write((byte)0);
        }
    }
}
