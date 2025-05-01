using Hi3Helper.UABT.Binary;
using System.Collections.Generic;
using System.IO;
// ReSharper disable IdentifierTypo

namespace Hi3Helper.UABT
{
    internal static class AssetBundle
    {
        public static List<AssetInfo> GetFileList(byte[] data)
        {
            EndianBinaryReader endianBinaryReader = new(new MemoryStream(data), EndianType.LittleEndian);
            endianBinaryReader.ReadAlignedString();
            int num = endianBinaryReader.ReadInt32();
            endianBinaryReader.Position += num * 12;
            int num2 = endianBinaryReader.ReadInt32();
            List<AssetInfo> list = new(num2);
            for (int i = 0; i < num2; i++)
            {
                string path = endianBinaryReader.ReadAlignedString();
                endianBinaryReader.Position += 8L;
                PPtr pPtr = default;
                pPtr.FileID = endianBinaryReader.ReadInt32();
                pPtr.PathID = endianBinaryReader.ReadInt64();
                PPtr pPtr2 = pPtr;
                AssetInfo item = default;
                item.Path = path;
                item.PPtr = pPtr2;
                list.Add(item);
            }
            return list;
        }
    }
}