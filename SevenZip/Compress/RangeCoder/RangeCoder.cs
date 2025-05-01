using System.IO;

namespace SevenZip.Compression.RangeCoder
{
    internal class Decoder
    {
        public const uint KTopValue = 1 << 24;
        public       uint Range;
        public       uint Code;
        // public Buffer.InBuffer Stream = new Buffer.InBuffer(1 << 16);
        public Stream Stream;

        public void Init(Stream stream)
        {
            // Stream.Init(stream);
            Stream = stream;

            Code = 0;
            Range = 0xFFFFFFFF;
            for (int i = 0; i < 5; i++)
                Code = (Code << 8) | (byte)Stream.ReadByte();
        }

        public void ReleaseStream()
        {
            // Stream.ReleaseStream();
            Stream = null;
        }

        public void Normalize()
        {
            while (Range < KTopValue)
            {
                Code = (Code << 8) | (byte)Stream.ReadByte();
                Range <<= 8;
            }
        }

        public void Decode(uint start, uint size, uint total)
        {
            Code -= start * Range;
            Range *= size;
            Normalize();
        }

        public uint DecodeDirectBits(int numTotalBits)
        {
            uint range = Range;
            uint code = Code;
            uint result = 0;
            for (int i = numTotalBits; i > 0; i--)
            {
                range >>= 1;
                /*
				result <<= 1;
				if (code >= range)
				{
					code -= range;
					result |= 1;
				}
				*/
                uint t = (code - range) >> 31;
                code -= range & (t - 1);
                result = (result << 1) | (1 - t);

                if (range >= KTopValue)
                {
                    continue;
                }

                code  =   (code << 8) | (byte)Stream.ReadByte();
                range <<= 8;
            }
            Range = range;
            Code = code;
            return result;
        }

        // ulong GetProcessedSize() {return Stream.GetProcessedSize(); }
    }
}
