using System;
using System.IO;
namespace SevenZip
{
	using CommandLineParser;
	
	public class CDoubleStream: Stream
	{
		public System.IO.Stream s1;
		public System.IO.Stream s2;
		public int fileIndex;
		public long skipSize;
		
		public override bool CanRead { get { return true; }}
		public override bool CanWrite { get { return false; }}
		public override bool CanSeek { get { return false; }}
		public override long Length { get { return s1.Length + s2.Length - skipSize; } }
		public override long Position
		{
			get { return 0;	}
			set { }
		}
		public override void Flush() { }
		public override int Read(byte[] buffer, int offset, int count) 
		{
			int numTotal = 0;
			while (count > 0)
			{
				if (fileIndex == 0)
				{
					int num = s1.Read(buffer, offset, count);
					offset += num;
					count -= num;
					numTotal += num;
					if (num == 0)
						fileIndex++;
				}
				if (fileIndex == 1)
				{
					numTotal += s2.Read(buffer, offset, count);
					return numTotal;
				}
			}
			return numTotal;
		}
		public override void Write(byte[] buffer, int offset, int count)
		{
			throw (new Exception("can't Write"));
		}
		public override long Seek(long offset, System.IO.SeekOrigin origin)
		{
			throw (new Exception("can't Seek"));
		}
		public override void SetLength(long value)
		{
			throw (new Exception("can't SetLength"));
		}
	}
}
