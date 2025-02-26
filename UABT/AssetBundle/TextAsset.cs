﻿using Hi3Helper.UABT.Binary;
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
            EndianBinaryReader reader = new(new MemoryStream(data), EndianType.LittleEndian);
            name = reader.ReadAlignedString();
            text = reader.ReadAlignedString();
        }

        /// <summary>
        /// TextAsset to byte[]
        /// </summary>
        /// <returns></returns>
        public byte[] GetBytes()
        {
            EndianBinaryWriter writer = new(new MemoryStream(), EndianType.LittleEndian);
            writer.WriteAlignedString(name);
            writer.WriteAlignedString(text);
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
            return text;
        }

#if NET6_0_OR_GREATER
        /// <summary>
        /// TextAsset to string
        /// </summary>
        /// <returns></returns>
        public List<string> GetStringList()
        {
            List<string> b = new();
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
#endif
    }
}
