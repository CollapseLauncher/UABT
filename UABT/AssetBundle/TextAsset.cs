using Hi3Helper.UABT.Binary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Hi3Helper.UABT
{
    public class TextAsset
    {
        public string name;
        public string text;
        /// <summary>
        ///  TextAsset
        /// </summary>
        /// <param name="data"></param>
        public TextAsset(byte[] data)
        {
            EndianBinaryReader reader = new EndianBinaryReader(new MemoryStream(data), EndianType.LittleEndian);
            name = reader.ReadAlignedString();
            text = reader.ReadAlignedString();
        }

        /// <summary>
        /// TextAsset to byte[]
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            EndianBinaryWriter writer = new EndianBinaryWriter(new MemoryStream(), EndianType.LittleEndian);
            writer.WriteAlignedString(name);
            writer.WriteAlignedString(text);
            writer.Position = 0;
            byte[] data = new byte[writer.BaseStream.Length];
            writer.BaseStream.Read(data, 0, data.Length);
            return data;
        }

        /// <summary>
        /// TextAsset to string
        /// </summary>
        /// <returns></returns>
        public string GetString()
        {
            return text;
        }

        /// <summary>
        /// TextAsset to string
        /// </summary>
        /// <returns></returns>
        public List<string> GetStringList()
        {
            List<string> b = new List<string>();
            foreach (ReadOnlySpan<char> a in text.AsSpan().EnumerateLines())
            {
                b.Add(a.ToString());
            }
            return b;
        }

        /// <summary>
        /// TextAsset to string
        /// </summary>
        /// <returns></returns>
        public SpanLineEnumerator GetStringEnumeration()
        {
            return text.AsSpan().EnumerateLines();
        }
    }
}
