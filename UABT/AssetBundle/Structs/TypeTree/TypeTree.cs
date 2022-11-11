using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UABT.Binary;

namespace UABT.TypeTree
{
    public static class TypeTreeHelper
    {
        public static void ReadTypeString(StringBuilder sb, List<TypeTreeNode> members, BinaryReader reader)
        {
            for (int i = 0; i < members.Count; i++)
            {
                ReadStringValue(sb, members, reader, ref i);
            }
        }

        private static void ReadStringValue(StringBuilder sb, List<TypeTreeNode> members, BinaryReader reader, ref int i)
        {
            TypeTreeNode typeTreeNode = members[i];
            int level = typeTreeNode.m_Level;
            string type = typeTreeNode.m_Type;
            string name = typeTreeNode.m_Name;
            object obj = null;
            bool flag = true;
            bool flag2 = (typeTreeNode.m_MetaFlag & 0x4000) != 0;
            switch (type)
            {
                case "SInt8":
                    obj = reader.ReadSByte();
                    break;
                case "UInt8":
                    obj = reader.ReadByte();
                    break;
                case "short":
                case "SInt16":
                    obj = reader.ReadInt16();
                    break;
                case "UInt16":
                case "unsigned short":
                    obj = reader.ReadUInt16();
                    break;
                case "int":
                case "SInt32":
                    obj = reader.ReadInt32();
                    break;
                case "UInt32":
                case "unsigned int":
                case "Type*":
                    obj = reader.ReadUInt32();
                    break;
                case "long long":
                case "SInt64":
                    obj = reader.ReadInt64();
                    break;
                case "UInt64":
                case "unsigned long long":
                    obj = reader.ReadUInt64();
                    break;
                case "float":
                    obj = reader.ReadSingle();
                    break;
                case "double":
                    obj = reader.ReadDouble();
                    break;
                case "bool":
                    obj = reader.ReadBoolean();
                    break;
                case "string":
                    {
                        flag = false;
                        string text = reader.ReadAlignedString();
                        sb.AppendFormat("{0}{1} {2} = \"{3}\"\r\n", new string('\t', level), type, name, text);
                        i += 3;
                        break;
                    }
                case "vector":
                    {
                        if (((uint)members[i + 1].m_MetaFlag & 0x4000u) != 0)
                        {
                            flag2 = true;
                        }
                        flag = false;
                        sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level), type, name);
                        sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level + 1), "Array", "Array");
                        int num3 = reader.ReadInt32();
                        sb.AppendFormat("{0}{1} {2} = {3}\r\n", new string('\t', level + 1), "int", "size", num3);
                        List<TypeTreeNode> members6 = GetMembers(members, level, i);
                        i += members6.Count - 1;
                        members6.RemoveRange(0, 3);
                        for (int l = 0; l < num3; l++)
                        {
                            sb.AppendFormat("{0}[{1}]\r\n", new string('\t', level + 2), l);
                            int i4 = 0;
                            ReadStringValue(sb, members6, reader, ref i4);
                        }
                        break;
                    }
                case "map":
                    {
                        if (((uint)members[i + 1].m_MetaFlag & 0x4000u) != 0)
                        {
                            flag2 = true;
                        }
                        flag = false;
                        sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level), type, name);
                        sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level + 1), "Array", "Array");
                        int num2 = reader.ReadInt32();
                        sb.AppendFormat("{0}{1} {2} = {3}\r\n", new string('\t', level + 1), "int", "size", num2);
                        List<TypeTreeNode> members3 = GetMembers(members, level, i);
                        i += members3.Count - 1;
                        members3.RemoveRange(0, 4);
                        List<TypeTreeNode> members4 = GetMembers(members3, members3[0].m_Level, 0);
                        members3.RemoveRange(0, members4.Count);
                        List<TypeTreeNode> members5 = members3;
                        for (int k = 0; k < num2; k++)
                        {
                            sb.AppendFormat("{0}[{1}]\r\n", new string('\t', level + 2), k);
                            sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level + 2), "pair", "data");
                            int i2 = 0;
                            int i3 = 0;
                            ReadStringValue(sb, members4, reader, ref i2);
                            ReadStringValue(sb, members5, reader, ref i3);
                        }
                        break;
                    }
                case "TypelessData":
                    {
                        flag = false;
                        int num = reader.ReadInt32();
                        reader.ReadBytes(num);
                        i += 2;
                        sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level), type, name);
                        sb.AppendFormat("{0}{1} {2} = {3}\r\n", new string('\t', level), "int", "size", num);
                        break;
                    }
                default:
                    if (i == members.Count || !(members[i + 1].m_Type == "Array"))
                    {
                        flag = false;
                        sb.AppendFormat("{0}{1} {2}\r\n", new string('\t', level), type, name);
                        List<TypeTreeNode> members2 = GetMembers(members, level, i);
                        members2.RemoveAt(0);
                        i += members2.Count;
                        for (int j = 0; j < members2.Count; j++)
                        {
                            ReadStringValue(sb, members2, reader, ref j);
                        }
                        break;
                    }
                    goto case "vector";
            }
            if (flag)
            {
                sb.AppendFormat("{0}{1} {2} = {3}\r\n", new string('\t', level), type, name, obj);
            }
            if (flag2)
            {
                reader.AlignStream(4);
            }
        }

        public static Dictionary<string, object> ReadBoxingType(List<TypeTreeNode> members, BinaryReader reader)
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>();
            for (int i = 0; i < members.Count; i++)
            {
                string name = members[i].m_Name;
                dictionary[name] = ReadValue(members, reader, ref i);
            }
            return dictionary;
        }

        private static object ReadValue(List<TypeTreeNode> members, BinaryReader reader, ref int i)
        {
            TypeTreeNode typeTreeNode = members[i];
            int level = typeTreeNode.m_Level;
            string type = typeTreeNode.m_Type;
            bool flag = (typeTreeNode.m_MetaFlag & 0x4000) != 0;
            object result;
            switch (type)
            {
                case "SInt8":
                    result = reader.ReadSByte();
                    break;
                case "UInt8":
                    result = reader.ReadByte();
                    break;
                case "short":
                case "SInt16":
                    result = reader.ReadInt16();
                    break;
                case "UInt16":
                case "unsigned short":
                    result = reader.ReadUInt16();
                    break;
                case "int":
                case "SInt32":
                    result = reader.ReadInt32();
                    break;
                case "UInt32":
                case "unsigned int":
                case "Type*":
                    result = reader.ReadUInt32();
                    break;
                case "long long":
                case "SInt64":
                    result = reader.ReadInt64();
                    break;
                case "UInt64":
                case "unsigned long long":
                    result = reader.ReadUInt64();
                    break;
                case "float":
                    result = reader.ReadSingle();
                    break;
                case "double":
                    result = reader.ReadDouble();
                    break;
                case "bool":
                    result = reader.ReadBoolean();
                    break;
                case "string":
                    result = reader.ReadAlignedString();
                    i += 3;
                    break;
                case "vector":
                    {
                        if (((uint)members[i + 1].m_MetaFlag & 0x4000u) != 0)
                        {
                            flag = true;
                        }
                        int num2 = reader.ReadInt32();
                        List<object> list2 = new List<object>(num2);
                        List<TypeTreeNode> members6 = GetMembers(members, level, i);
                        i += members6.Count - 1;
                        members6.RemoveRange(0, 3);
                        for (int l = 0; l < num2; l++)
                        {
                            int i4 = 0;
                            list2.Add(ReadValue(members6, reader, ref i4));
                        }
                        result = list2;
                        break;
                    }
                case "map":
                    {
                        if (((uint)members[i + 1].m_MetaFlag & 0x4000u) != 0)
                        {
                            flag = true;
                        }
                        int num = reader.ReadInt32();
                        List<KeyValuePair<object, object>> list = new List<KeyValuePair<object, object>>(num);
                        List<TypeTreeNode> members3 = GetMembers(members, level, i);
                        i += members3.Count - 1;
                        members3.RemoveRange(0, 4);
                        List<TypeTreeNode> members4 = GetMembers(members3, members3[0].m_Level, 0);
                        members3.RemoveRange(0, members4.Count);
                        List<TypeTreeNode> members5 = members3;
                        for (int k = 0; k < num; k++)
                        {
                            int i2 = 0;
                            int i3 = 0;
                            list.Add(new KeyValuePair<object, object>(ReadValue(members4, reader, ref i2), ReadValue(members5, reader, ref i3)));
                        }
                        result = list;
                        break;
                    }
                case "TypelessData":
                    {
                        int count = reader.ReadInt32();
                        result = reader.ReadBytes(count);
                        i += 2;
                        break;
                    }
                default:
                    if (i == members.Count || !(members[i + 1].m_Type == "Array"))
                    {
                        List<TypeTreeNode> members2 = GetMembers(members, level, i);
                        members2.RemoveAt(0);
                        i += members2.Count;
                        Dictionary<string, object> dictionary = new Dictionary<string, object>();
                        for (int j = 0; j < members2.Count; j++)
                        {
                            string name = members2[j].m_Name;
                            dictionary[name] = ReadValue(members2, reader, ref j);
                        }
                        result = dictionary;
                        break;
                    }
                    goto case "vector";
            }
            if (flag)
            {
                reader.AlignStream(4);
            }
            return result;
        }

        private static List<TypeTreeNode> GetMembers(List<TypeTreeNode> members, int level, int index)
        {
            List<TypeTreeNode> list = new List<TypeTreeNode>();
            list.Add(members[0]);
            for (int i = index + 1; i < members.Count; i++)
            {
                TypeTreeNode typeTreeNode = members[i];
                if (typeTreeNode.m_Level <= level)
                {
                    return list;
                }
                list.Add(typeTreeNode);
            }
            return list;
        }

        public static byte[] WriteBoxingType(Dictionary<string, object> obj, List<TypeTreeNode> members)
        {
            MemoryStream memoryStream = new MemoryStream();
            BinaryWriter write = new BinaryWriter(memoryStream);
            for (int i = 0; i < members.Count; i++)
            {
                string name = members[i].m_Name;
                WriteValue(obj[name], members, write, ref i);
            }
            return memoryStream.ToArray();
        }

        private static void WriteValue(object value, List<TypeTreeNode> members, BinaryWriter write, ref int i)
        {
            TypeTreeNode typeTreeNode = members[i];
            int level = typeTreeNode.m_Level;
            string type = typeTreeNode.m_Type;
            bool flag = (typeTreeNode.m_MetaFlag & 0x4000) != 0;
            switch (type)
            {
                case "SInt8":
                    write.Write((sbyte)value);
                    break;
                case "UInt8":
                    write.Write((byte)value);
                    break;
                case "short":
                case "SInt16":
                    write.Write((short)value);
                    break;
                case "UInt16":
                case "unsigned short":
                    write.Write((ushort)value);
                    break;
                case "int":
                case "SInt32":
                    write.Write((int)value);
                    break;
                case "UInt32":
                case "unsigned int":
                case "Type*":
                    write.Write((uint)value);
                    break;
                case "long long":
                case "SInt64":
                    write.Write((long)value);
                    break;
                case "UInt64":
                case "unsigned long long":
                    write.Write((ulong)value);
                    break;
                case "float":
                    write.Write((float)value);
                    break;
                case "double":
                    write.Write((double)value);
                    break;
                case "bool":
                    write.Write((bool)value);
                    break;
                case "string":
                    write.WriteAlignedString((string)value);
                    i += 3;
                    break;
                case "vector":
                    {
                        if (((uint)members[i + 1].m_MetaFlag & 0x4000u) != 0)
                        {
                            flag = true;
                        }
                        List<object> list2 = (List<object>)value;
                        int count2 = list2.Count;
                        write.Write(count2);
                        List<TypeTreeNode> members6 = GetMembers(members, level, i);
                        i += members6.Count - 1;
                        members6.RemoveRange(0, 3);
                        for (int l = 0; l < count2; l++)
                        {
                            int i4 = 0;
                            WriteValue(list2[l], members6, write, ref i4);
                        }
                        break;
                    }
                case "map":
                    {
                        if (((uint)members[i + 1].m_MetaFlag & 0x4000u) != 0)
                        {
                            flag = true;
                        }
                        List<KeyValuePair<object, object>> list = (List<KeyValuePair<object, object>>)value;
                        int count = list.Count;
                        write.Write(count);
                        List<TypeTreeNode> members3 = GetMembers(members, level, i);
                        i += members3.Count - 1;
                        members3.RemoveRange(0, 4);
                        List<TypeTreeNode> members4 = GetMembers(members3, members3[0].m_Level, 0);
                        members3.RemoveRange(0, members4.Count);
                        List<TypeTreeNode> members5 = members3;
                        for (int k = 0; k < count; k++)
                        {
                            int i2 = 0;
                            int i3 = 0;
                            WriteValue(list[k].Key, members4, write, ref i2);
                            WriteValue(list[k].Value, members5, write, ref i3);
                        }
                        break;
                    }
                case "TypelessData":
                    {
                        byte[] array = ((object[])value).Cast<byte>().ToArray();
                        int value2 = array.Length;
                        write.Write(value2);
                        write.Write(array);
                        i += 2;
                        break;
                    }
                default:
                    if (i == members.Count || !(members[i + 1].m_Type == "Array"))
                    {
                        List<TypeTreeNode> members2 = GetMembers(members, level, i);
                        members2.RemoveAt(0);
                        i += members2.Count;
                        Dictionary<string, object> dictionary = (Dictionary<string, object>)value;
                        for (int j = 0; j < members2.Count; j++)
                        {
                            string name = members2[j].m_Name;
                            WriteValue(dictionary[name], members2, write, ref j);
                        }
                        break;
                    }
                    goto case "vector";
            }
            if (flag)
            {
                write.AlignStream(4);
            }
        }
    }
}
