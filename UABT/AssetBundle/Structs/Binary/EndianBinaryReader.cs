using System;
using System.Buffers.Binary;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
// ReSharper disable IdentifierTypo
// ReSharper disable UnusedMember.Global

namespace Hi3Helper.UABT.Binary
{
    public class EndianBinaryReader(Stream stream, EndianType endian = EndianType.BigEndian, bool leaveOpen = false)
        : BinaryReader(stream, Encoding.UTF8, leaveOpen)
    {
        private const int        StackallocMax = 4 << 10;
        public        EndianType Endian        = endian;

        public long Position
        {
            get
            {
                return BaseStream.Position;
            }
            set
            {
                BaseStream.Position = value;
            }
        }

        public override short ReadInt16()
        {
            Span<byte> buffer = stackalloc byte[2];
            base.BaseStream.ReadAtLeast(buffer, 2);

            return Endian == EndianType.BigEndian ? BinaryPrimitives.ReadInt16BigEndian(buffer)
                : BinaryPrimitives.ReadInt16LittleEndian(buffer);
        }

        public override int ReadInt32()
        {
            Span<byte> buffer = stackalloc byte[4];
            base.BaseStream.ReadAtLeast(buffer, 4);

            return Endian == EndianType.BigEndian ? BinaryPrimitives.ReadInt32BigEndian(buffer)
                : BinaryPrimitives.ReadInt32LittleEndian(buffer);
        }

        public override long ReadInt64()
        {
            Span<byte> buffer = stackalloc byte[8];
            base.BaseStream.ReadAtLeast(buffer, 8);

            return Endian == EndianType.BigEndian ? BinaryPrimitives.ReadInt64BigEndian(buffer)
                : BinaryPrimitives.ReadInt64LittleEndian(buffer);
        }

        public Int128 ReadInt128()
        {
            Span<byte> buffer = stackalloc byte[16];
            base.BaseStream.ReadAtLeast(buffer, 16);

            return Endian == EndianType.BigEndian ? BinaryPrimitives.ReadInt128BigEndian(buffer)
                : BinaryPrimitives.ReadInt128LittleEndian(buffer);
        }

        public override ushort ReadUInt16()
        {
            Span<byte> buffer = stackalloc byte[2];
            base.BaseStream.ReadAtLeast(buffer, 2);

            return Endian == EndianType.BigEndian ? BinaryPrimitives.ReadUInt16BigEndian(buffer)
                : BinaryPrimitives.ReadUInt16LittleEndian(buffer);
        }

        public override uint ReadUInt32()
        {
            Span<byte> buffer = stackalloc byte[4];
            base.BaseStream.ReadAtLeast(buffer, 4);

            return Endian == EndianType.BigEndian ? BinaryPrimitives.ReadUInt32BigEndian(buffer)
                : BinaryPrimitives.ReadUInt32LittleEndian(buffer);
        }

        public override ulong ReadUInt64()
        {
            Span<byte> buffer = stackalloc byte[8];
            base.BaseStream.ReadAtLeast(buffer, 8);

            return Endian == EndianType.BigEndian ? BinaryPrimitives.ReadUInt64BigEndian(buffer)
                : BinaryPrimitives.ReadUInt64LittleEndian(buffer);
        }

        public UInt128 ReadUInt128()
        {
            Span<byte> buffer = stackalloc byte[16];
            base.BaseStream.ReadAtLeast(buffer, 16);

            return Endian == EndianType.BigEndian ? BinaryPrimitives.ReadUInt128BigEndian(buffer)
                : BinaryPrimitives.ReadUInt128LittleEndian(buffer);
        }

        public override float ReadSingle()
        {
            Span<byte> buffer = stackalloc byte[4];
            base.BaseStream.ReadAtLeast(buffer, 4);

            return Endian == EndianType.BigEndian ? BinaryPrimitives.ReadSingleBigEndian(buffer)
                : BinaryPrimitives.ReadSingleLittleEndian(buffer);
        }

        public override double ReadDouble()
        {
            Span<byte> buffer = stackalloc byte[8];
            base.BaseStream.ReadAtLeast(buffer, 8);

            return Endian == EndianType.BigEndian ? BinaryPrimitives.ReadDoubleBigEndian(buffer)
                : BinaryPrimitives.ReadDoubleLittleEndian(buffer);
        }

        public string ReadString8BitLength()
        {
            sbyte len = (sbyte)base.BaseStream.ReadByte();
            return ReadStringFromLen(len);
        }

        public override string ReadString()
        {
            ushort len = ReadUInt16();
            return ReadStringFromLen(len);
        }

        private unsafe string ReadStringFromLen(int len)
        {
            Span<byte> buffer = len > StackallocMax ? new byte[len] : stackalloc byte[len];
            string returnStr;

            base.BaseStream.ReadAtLeast(buffer, len);
            fixed (sbyte* bufferByte = MemoryMarshal.Cast<byte, sbyte>(buffer))
                returnStr = new string(bufferByte, 0, len);

            return returnStr;
        }
    }
}
