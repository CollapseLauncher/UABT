using System;
using System.IO;
using System.Text;
// ReSharper disable IdentifierTypo

namespace Hi3Helper.UABT.Binary
{
    public class EndianBinaryWriter(Stream stream, EndianType endian = EndianType.BigEndian, bool leaveOpen = false)
        : BinaryWriter(stream, Encoding.UTF8, leaveOpen)
    {
        public EndianType Endian = endian;

        private byte[] _a16 = new byte[2];

        private byte[] _a32 = new byte[4];

        private byte[] _a64 = new byte[8];

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

        public override void Write(short a)
        {
            if (Endian == EndianType.BigEndian)
            {
                _a16 = BitConverter.GetBytes(a);
                Array.Reverse(_a16);
                Write(_a16);
            }
            else
            {
                base.Write(a);
            }
        }

        public override void Write(int a)
        {
            if (Endian == EndianType.BigEndian)
            {
                _a32 = BitConverter.GetBytes(a);
                Array.Reverse(_a32);
                Write(_a32);
            }
            else
            {
                base.Write(a);
            }
        }

        public override void Write(long a)
        {
            if (Endian == EndianType.BigEndian)
            {
                _a64 = BitConverter.GetBytes(a);
                Array.Reverse(_a64);
                Write(_a64);
            }
            else
            {
                base.Write(a);
            }
        }

        public override void Write(ushort a)
        {
            if (Endian == EndianType.BigEndian)
            {
                _a16 = BitConverter.GetBytes(a);
                Array.Reverse(_a16);
                Write(_a16);
            }
            else
            {
                base.Write(a);
            }
        }

        public override void Write(uint a)
        {
            if (Endian == EndianType.BigEndian)
            {
                _a32 = BitConverter.GetBytes(a);
                Array.Reverse(_a32);
                Write(_a32);
            }
            else
            {
                base.Write(a);
            }
        }

        public override void Write(ulong a)
        {
            if (Endian == EndianType.BigEndian)
            {
                _a64 = BitConverter.GetBytes(a);
                Array.Reverse(_a64);
                Write(_a64);
            }
            else
            {
                base.Write(a);
            }
        }

        public override void Write(float a)
        {
            if (Endian == EndianType.BigEndian)
            {
                _a32 = BitConverter.GetBytes(a);
                Array.Reverse(_a32);
                Write(_a32);
            }
            else
            {
                base.Write(a);
            }
        }

        public override void Write(double a)
        {
            if (Endian == EndianType.BigEndian)
            {
                _a64 = BitConverter.GetBytes(a);
                Array.Reverse(_a64);
                Write(_a64);
            }
            else
            {
                base.Write(a);
            }
        }
    }
}
