namespace SevenZip.Compression.RangeCoder
{
	internal readonly struct BitTreeDecoder(int numBitLevels)
	{
		private readonly BitDecoder[] _models = new BitDecoder[1 << numBitLevels];

		public void Init()
		{
			for (uint i = 1; i < 1 << numBitLevels; i++)
				_models[i].Init();
		}

		public uint Decode(Decoder rangeDecoder)
		{
			uint m = 1;
			for (int bitIndex = numBitLevels; bitIndex > 0; bitIndex--)
				m = (m << 1) + _models[m].Decode(rangeDecoder);
			return m - ((uint)1 << numBitLevels);
		}

		public uint ReverseDecode(Decoder rangeDecoder)
		{
			uint m = 1;
			uint symbol = 0;
			for (int bitIndex = 0; bitIndex < numBitLevels; bitIndex++)
			{
				uint bit = _models[m].Decode(rangeDecoder);
				m <<= 1;
				m += bit;
				symbol |= bit << bitIndex;
			}
			return symbol;
		}

		public static uint ReverseDecode(BitDecoder[] models, uint startIndex,
			Decoder rangeDecoder, int numBitLevels)
		{
			uint m = 1;
			uint symbol = 0;
			for (int bitIndex = 0; bitIndex < numBitLevels; bitIndex++)
			{
				uint bit = models[startIndex + m].Decode(rangeDecoder);
				m <<= 1;
				m += bit;
				symbol |= bit << bitIndex;
			}
			return symbol;
		}
	}
}
