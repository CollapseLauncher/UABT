using Hi3Helper.Data;
using System;
using System.Buffers;
using System.IO;
using System.Text;

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
            if (length > 0 && length <= reader.BaseStream.Length - reader.BaseStream.Position)
            {
                byte[] bytes = reader.ReadBytes(length);
                string @string = Encoding.UTF8.GetString(bytes);
                reader.AlignStream(4);
                return @string;
            }
            return "";
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

        public static Quaternion ReadQuaternion(this BinaryReader reader)
        {
            Quaternion result = default;
            result.X = reader.ReadSingle();
            result.Y = reader.ReadSingle();
            result.Z = reader.ReadSingle();
            result.W = reader.ReadSingle();
            return result;
        }

        public static Vector2 ReadVector2(this BinaryReader reader)
        {
            Vector2 result = default;
            result.X = reader.ReadSingle();
            result.Y = reader.ReadSingle();
            return result;
        }

        public static Vector3 ReadVector3(this BinaryReader reader)
        {
            Vector3 result = default;
            result.X = reader.ReadSingle();
            result.Y = reader.ReadSingle();
            result.Z = reader.ReadSingle();
            return result;
        }

        public static Vector4 ReadVector4(this BinaryReader reader)
        {
            Vector4 result = default;
            result.X = reader.ReadSingle();
            result.Y = reader.ReadSingle();
            result.Z = reader.ReadSingle();
            result.W = reader.ReadSingle();
            return result;
        }

        private static T[] ReadArray<T>(Func<T> del, int length)
        {
            T[] array = new T[length];
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = del();
            }
            return array;
        }

        public static int[] ReadInt32Array(this BinaryReader reader, int length)
        {
            return ReadArray(reader.ReadInt32, length);
        }

        public static uint[] ReadUInt32Array(this BinaryReader reader, int length)
        {
            return ReadArray(reader.ReadUInt32, length);
        }

        public static float[] ReadSingleArray(this BinaryReader reader, int length)
        {
            return ReadArray(reader.ReadSingle, length);
        }

        public static Vector2[] ReadVector2Array(this BinaryReader reader, int length)
        {
            return ReadArray(reader.ReadVector2, length);
        }

        public static Vector4[] ReadVector4Array(this BinaryReader reader, int length)
        {
            return ReadArray(reader.ReadVector4, length);
        }
    }
}
