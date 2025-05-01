// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

using Hi3Helper.UABT.Binary;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Hi3Helper.UABT
{
    public class TextAsset
    {
        public string Name;
        public string Text;
        /// <summary>
        ///  TextAsset
        /// </summary>
        /// <param name="data"></param>
        public TextAsset(byte[] data)
        {
            EndianBinaryReader reader = new(new MemoryStream(data), EndianType.LittleEndian);
            Name = reader.ReadAlignedString();
            Text = reader.ReadAlignedString();
        }

        /// <summary>
        /// TextAsset to byte[]
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            EndianBinaryWriter writer = new(new MemoryStream(), EndianType.LittleEndian);
            writer.WriteAlignedString(Name);
            writer.WriteAlignedString(Text);
            writer.Position = 0;
            byte[] data = new byte[writer.BaseStream.Length];
            _ = writer.BaseStream.Read(data, 0, data.Length);
            return data;
        }

        /// <summary>
        /// TextAsset to string
        /// </summary>
        /// <returns></returns>
        public string GetString()
        {
            return Text;
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// TextAsset to string
        /// </summary>
        /// <returns></returns>
        public List<string> GetStringList()
        {
            List<string> b = [];
            foreach (ReadOnlySpan<char> a in Text.AsSpan().EnumerateLines())
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
            return Text.AsSpan().EnumerateLines();
        }
#endif
    }
}
