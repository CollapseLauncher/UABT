using System;
using System.IO;
using System.Text;

namespace Hi3Helper.UABT.Binary
{
    public class EndianBinaryReader : BinaryReader
    {
        public EndianType endian;

        private byte[] a16 = new byte[2];

        private byte[] a32 = new byte[4];

        private byte[] a64 = new byte[8];

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

        public EndianBinaryReader(Stream stream, EndianType endian = EndianType.BigEndian)
            : base(stream)
        {
            this.endian = endian;
        }

        public override short ReadInt16()
        {
            if (endian == EndianType.BigEndian)
            {
                a16 = ReadBytes(2);
                Array.Reverse(a16);
                return BitConverter.ToInt16(a16, 0);
            }
            return base.ReadInt16();
        }

        public override int ReadInt32()
        {
            if (endian == EndianType.BigEndian)
            {
                a32 = ReadBytes(4);
                Array.Reverse(a32);
                return BitConverter.ToInt32(a32, 0);
            }
            return base.ReadInt32();
        }

        public override long ReadInt64()
        {
            if (endian == EndianType.BigEndian)
            {
                a64 = ReadBytes(8);
                Array.Reverse(a64);
                return BitConverter.ToInt64(a64, 0);
            }
            return base.ReadInt64();
        }

        public override ushort ReadUInt16()
        {
            if (endian == EndianType.BigEndian)
            {
                a16 = ReadBytes(2);
                Array.Reverse(a16);
                return BitConverter.ToUInt16(a16, 0);
            }
            return base.ReadUInt16();
        }

        public override uint ReadUInt32()
        {
            if (endian == EndianType.BigEndian)
            {
                a32 = ReadBytes(4);
                Array.Reverse(a32);
                return BitConverter.ToUInt32(a32, 0);
            }
            return base.ReadUInt32();
        }

        public override ulong ReadUInt64()
        {
            if (endian == EndianType.BigEndian)
            {
                a64 = ReadBytes(8);
                Array.Reverse(a64);
                return BitConverter.ToUInt64(a64, 0);
            }
            return base.ReadUInt64();
        }

        public override float ReadSingle()
        {
            if (endian == EndianType.BigEndian)
            {
                a32 = ReadBytes(4);
                Array.Reverse(a32);
                return BitConverter.ToSingle(a32, 0);
            }
            return base.ReadSingle();
        }

        public override double ReadDouble()
        {
            if (endian == EndianType.BigEndian)
            {
                a64 = ReadBytes(8);
                Array.Reverse(a64);
                return BitConverter.ToUInt64(a64, 0);
            }
            return base.ReadDouble();
        }

        public override string ReadString()
        {
            a16 = ReadBytes(2);
            if (endian == EndianType.BigEndian)
            {
                Array.Reverse(a16);
            }
            ushort count = BitConverter.ToUInt16(a16);
            ReadOnlySpan<byte> strSpan = ReadBytes(count);
            return Encoding.UTF8.GetString(strSpan);
        }
    }
}
