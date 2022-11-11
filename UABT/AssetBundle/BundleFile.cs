using System.Collections.Generic;
using System.IO;
using System.Linq;
using UABT.Binary;
using UABT.LZ4;

namespace UABT
{
    public class BundleFile
    {
        public string versionPlayer;

        public string versionEngine;

        public List<StreamFile> fileList = new List<StreamFile>();

        public BundleFile()
        {
        }

        public BundleFile(byte[] data)
        {
            EndianBinaryReader bundleReader = new EndianBinaryReader(new MemoryStream(data));
            Load(bundleReader);
        }

        public BundleFile(Stream stream)
        {
            EndianBinaryReader bundleReader = new EndianBinaryReader(stream);
            Load(bundleReader);
        }

        public BundleFile(EndianBinaryReader reader)
        {
            Load(reader);
        }

        private void Load(EndianBinaryReader bundleReader)
        {
            string a;
            if ((a = bundleReader.ReadStringToNull()) == "UnityFS")
            {
                int num = bundleReader.ReadInt32();
                versionPlayer = bundleReader.ReadStringToNull();
                versionEngine = bundleReader.ReadStringToNull();
                if (num == 6)
                {
                    ReadFormat6(bundleReader);
                }
            }
        }

        private void ReadFormat6(EndianBinaryReader bundleReader, bool padding = false)
        {
            bundleReader.ReadInt64();
            int num = bundleReader.ReadInt32();
            int num2 = bundleReader.ReadInt32();
            int num3 = bundleReader.ReadInt32();
            if (padding)
            {
                bundleReader.ReadByte();
            }
            byte[] array;
            if (((uint)num3 & 0x80u) != 0)
            {
                long position = bundleReader.Position;
                bundleReader.Position = bundleReader.BaseStream.Length - num;
                array = bundleReader.ReadBytes(num);
                bundleReader.Position = position;
            }
            else
            {
                array = bundleReader.ReadBytes(num);
            }
            MemoryStream stream;
            switch (num3 & 0x3F)
            {
                default:
                    stream = new MemoryStream(array);
                    break;
                case 1:
                    stream = SevenZipHelper.StreamDecompress(new MemoryStream(array));
                    break;
                case 2:
                case 3:
                    {
                        byte[] array2 = new byte[num2];
                        LZ4CodecHelper.Decode(array, 0, array.Length, array2, 0, num2);
                        stream = new MemoryStream(array2);
                        break;
                    }
            }
            using EndianBinaryReader endianBinaryReader = new EndianBinaryReader(stream);
            endianBinaryReader.Position = 16L;
            int num4 = endianBinaryReader.ReadInt32();
            BlockInfo[] array3 = new BlockInfo[num4];
            for (int i = 0; i < num4; i++)
            {
                array3[i] = new BlockInfo
                {
                    uncompressedSize = endianBinaryReader.ReadUInt32(),
                    compressedSize = endianBinaryReader.ReadUInt32(),
                    flag = endianBinaryReader.ReadInt16()
                };
            }
            array3.Sum((BlockInfo x) => x.uncompressedSize);
            Stream stream2 = new MemoryStream();
            BlockInfo[] array4 = array3;
            foreach (BlockInfo blockInfo in array4)
            {
                switch (blockInfo.flag & 0x3F)
                {
                    default:
                        {
                            byte[] array7 = bundleReader.ReadBytes((int)blockInfo.compressedSize);
                            stream2.Write(array7, 0, array7.Length);
                            break;
                        }
                    case 1:
                        SevenZipHelper.StreamDecompress(bundleReader.BaseStream, stream2, blockInfo.compressedSize, blockInfo.uncompressedSize);
                        break;
                    case 2:
                    case 3:
                        {
                            byte[] array5 = bundleReader.ReadBytes((int)blockInfo.compressedSize);
                            byte[] array6 = new byte[blockInfo.uncompressedSize];
                            int count = LZ4CodecHelper.Decode(array5, 0, array5.Length, array6, 0, (int)blockInfo.uncompressedSize);
                            stream2.Write(array6, 0, count);
                            break;
                        }
                }
            }
            stream2.Position = 0L;
            using (stream2)
            {
                int num5 = endianBinaryReader.ReadInt32();
                for (int k = 0; k < num5; k++)
                {
                    StreamFile streamFile = new StreamFile();
                    long position2 = endianBinaryReader.ReadInt64();
                    long num6 = endianBinaryReader.ReadInt64();
                    endianBinaryReader.ReadInt32();
                    streamFile.fileName = Path.GetFileName(endianBinaryReader.ReadStringToNull());
                    streamFile.stream = new MemoryStream();
                    stream2.Position = position2;
                    stream2.CopyTo(streamFile.stream, (int)num6);
                    streamFile.stream.Position = 0L;
                    fileList.Add(streamFile);
                }
            }
        }
    }
}
