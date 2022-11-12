using System;
using System.IO;

namespace Hi3Helper.UABT.Binary
{
    public class EndianBinaryWriter : BinaryWriter
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

        public EndianBinaryWriter(Stream stream, EndianType endian = EndianType.BigEndian)
            : base(stream)
        {
            this.endian = endian;
        }

        public override void Write(short a)
        {
            if (endian == EndianType.BigEndian)
            {
                a16 = BitConverter.GetBytes(a);
                Array.Reverse(a16);
                Write(a16);
            }
            else
            {
                base.Write(a);
            }
        }

        public override void Write(int a)
        {
            if (endian == EndianType.BigEndian)
            {
                a32 = BitConverter.GetBytes(a);
                Array.Reverse(a32);
                Write(a32);
            }
            else
            {
                base.Write(a);
            }
        }

        public override void Write(long a)
        {
            if (endian == EndianType.BigEndian)
            {
                a64 = BitConverter.GetBytes(a);
                Array.Reverse(a64);
                Write(a64);
            }
            else
            {
                base.Write(a);
            }
        }

        public override void Write(ushort a)
        {
            if (endian == EndianType.BigEndian)
            {
                a16 = BitConverter.GetBytes(a);
                Array.Reverse(a16);
                Write(a16);
            }
            else
            {
                base.Write(a);
            }
        }

        public override void Write(uint a)
        {
            if (endian == EndianType.BigEndian)
            {
                a32 = BitConverter.GetBytes(a);
                Array.Reverse(a32);
                Write(a32);
            }
            else
            {
                base.Write(a);
            }
        }

        public override void Write(ulong a)
        {
            if (endian == EndianType.BigEndian)
            {
                a64 = BitConverter.GetBytes(a);
                Array.Reverse(a64);
                Write(a64);
            }
            else
            {
                base.Write(a);
            }
        }

        public override void Write(float a)
        {
            if (endian == EndianType.BigEndian)
            {
                a32 = BitConverter.GetBytes(a);
                Array.Reverse(a32);
                Write(a32);
            }
            else
            {
                base.Write(a);
            }
        }

        public override void Write(double a)
        {
            if (endian == EndianType.BigEndian)
            {
                a64 = BitConverter.GetBytes(a);
                Array.Reverse(a64);
                Write(a64);
            }
            else
            {
                base.Write(a);
            }
        }
    }
}
