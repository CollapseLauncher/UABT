using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UABT.Binary;
using UABT.TypeTree;
using UABT.UABT.AssetBundle.Structs;

namespace UABT
{
    public class SerializedFile
    {
        public EndianBinaryReader reader = new EndianBinaryReader(new MemoryStream());

        public EndianBinaryWriter writer;

        public string fullName;

        public string originalPath;

        public string fileName;

        public string upperFileName;

        public int[] version = new int[4];

        public BuildType buildType;

        public bool valid;

        public SerializedFileHeader header;

        private EndianType m_FileEndianess;

        public string unityVersion = "2.5.0f5";

        public BuildTarget m_TargetPlatform = BuildTarget.UnknownPlatform;

        private bool m_EnableTypeTree = true;

        public List<SerializedType> m_Types;

        public List<ObjectInfo> m_Objects;

        private List<LocalSerializedObjectIdentifier> m_ScriptTypes;

        public List<FileIdentifier> m_Externals;

        public List<AssetInfo> assetinfolist;

        public SerializedFile(string fullName, EndianBinaryReader readeri)
        {
            readeri.BaseStream.CopyTo(reader.BaseStream);
            reader.Position = 0L;
            this.fullName = fullName;
            fileName = Path.GetFileName(fullName);
            upperFileName = fileName.ToUpper();
            try
            {
                header = new SerializedFileHeader();
                header.m_MetadataSize = reader.ReadUInt32();
                header.m_FileSize = reader.ReadUInt32();
                header.m_Version = reader.ReadUInt32();
                header.m_DataOffset = reader.ReadUInt32();
                if (header.m_Version >= 9)
                {
                    header.m_Endianess = reader.ReadByte();
                    header.m_Reserved = reader.ReadBytes(3);
                    m_FileEndianess = (EndianType)header.m_Endianess;
                }
                else
                {
                    reader.Position = header.m_FileSize - header.m_MetadataSize;
                    m_FileEndianess = (EndianType)reader.ReadByte();
                }
                if (m_FileEndianess == EndianType.LittleEndian)
                {
                    reader.endian = EndianType.LittleEndian;
                }
                if (header.m_Version >= 7)
                {
                    unityVersion = reader.ReadStringToNull();
                    SetVersion(unityVersion);
                }
                if (header.m_Version >= 8)
                {
                    m_TargetPlatform = (BuildTarget)reader.ReadInt32();
                    if (!Enum.IsDefined(typeof(BuildTarget), m_TargetPlatform))
                    {
                        m_TargetPlatform = BuildTarget.UnknownPlatform;
                    }
                }
                if (header.m_Version >= 13)
                {
                    m_EnableTypeTree = reader.ReadBoolean();
                }
                int num = reader.ReadInt32();
                m_Types = new List<SerializedType>(num);
                for (int i = 0; i < num; i++)
                {
                    m_Types.Add(ReadSerializedType());
                }
                if (header.m_Version >= 7 && header.m_Version < 14)
                {
                    reader.ReadInt32();
                }
                int num2 = reader.ReadInt32();
                m_Objects = new List<ObjectInfo>(num2);
                for (int j = 0; j < num2; j++)
                {
                    ObjectInfo objectInfo = new ObjectInfo();
                    if (header.m_Version < 14)
                    {
                        objectInfo.m_PathID = reader.ReadInt32();
                    }
                    else
                    {
                        reader.AlignStream(4);
                        objectInfo.m_PathID = reader.ReadInt64();
                    }
                    objectInfo.byteStart = reader.ReadUInt32();
                    objectInfo.byteSize = reader.ReadUInt32();
                    objectInfo.typeID = reader.ReadInt32();
                    if (header.m_Version < 16)
                    {
                        objectInfo.classID = reader.ReadUInt16();
                        objectInfo.serializedType = m_Types.Find((SerializedType x) => x.classID == objectInfo.typeID);
                        reader.ReadUInt16();
                    }
                    else
                    {
                        SerializedType serializedType = m_Types[objectInfo.typeID];
                        objectInfo.serializedType = serializedType;
                        objectInfo.classID = serializedType.classID;
                    }
                    if (header.m_Version == 15 || header.m_Version == 16)
                    {
                        reader.ReadByte();
                    }
                    m_Objects.Add(objectInfo);
                }
                if (header.m_Version >= 11)
                {
                    int num3 = reader.ReadInt32();
                    m_ScriptTypes = new List<LocalSerializedObjectIdentifier>(num3);
                    for (int k = 0; k < num3; k++)
                    {
                        LocalSerializedObjectIdentifier localSerializedObjectIdentifier = new LocalSerializedObjectIdentifier
                        {
                            localSerializedFileIndex = reader.ReadInt32()
                        };
                        if (header.m_Version < 14)
                        {
                            localSerializedObjectIdentifier.localIdentifierInFile = reader.ReadInt32();
                        }
                        else
                        {
                            reader.AlignStream(4);
                            localSerializedObjectIdentifier.localIdentifierInFile = reader.ReadInt64();
                        }
                        m_ScriptTypes.Add(localSerializedObjectIdentifier);
                    }
                }
                int num4 = reader.ReadInt32();
                m_Externals = new List<FileIdentifier>(num4);
                for (int l = 0; l < num4; l++)
                {
                    FileIdentifier fileIdentifier = new FileIdentifier();
                    if (header.m_Version >= 6)
                    {
                        reader.ReadStringToNull();
                    }
                    if (header.m_Version >= 5)
                    {
                        fileIdentifier.guid = new Guid(reader.ReadBytes(16));
                        fileIdentifier.type = reader.ReadInt32();
                    }
                    fileIdentifier.pathName = reader.ReadStringToNull();
                    fileIdentifier.fileName = Path.GetFileName(fileIdentifier.pathName);
                    m_Externals.Add(fileIdentifier);
                }
                _ = header.m_Version;
                _ = 5;
                for (int m = 0; m < m_Objects.Count; m++)
                {
                    reader.Position = m_Objects[m].byteStart + header.m_DataOffset;
                    m_Objects[m].data = reader.ReadBytes((int)m_Objects[m].byteSize);
                }
                ObjectInfo objectInfo2 = m_Objects.Find((ObjectInfo x) => x.classID == 142);
                if (objectInfo2.data != null)
                {
                    assetinfolist = AssetBundle.GetFileList(objectInfo2.data);
                }
                valid = true;
            }
            catch
            {
            }
        }

        public void addobject(long pathID, ClassIDType classID, int typeID, byte[] data)
        {
            ObjectInfo item = new ObjectInfo
            {
                m_PathID = pathID,
                classID = (int)classID,
                typeID = typeID,
                data = data
            };
            m_Objects.Add(item);
        }

        public void removeObject(long pathID)
        {
            int index = m_Objects.FindIndex((ObjectInfo x) => x.m_PathID == pathID);
            m_Objects.RemoveAt(index);
        }

        public byte[] Pack()
        {
            writer = new EndianBinaryWriter(new MemoryStream());
            writer.Write(header.m_MetadataSize);
            writer.Write(header.m_FileSize);
            writer.Write(header.m_Version);
            writer.Write(header.m_DataOffset);
            if (header.m_Version >= 9)
            {
                writer.Write(header.m_Endianess);
                writer.Write(header.m_Reserved);
            }
            else
            {
                writer.Position = header.m_FileSize - header.m_MetadataSize;
                writer.Write((byte)m_FileEndianess);
            }
            if (m_FileEndianess == EndianType.LittleEndian)
            {
                writer.endian = EndianType.LittleEndian;
            }
            if (header.m_Version >= 7)
            {
                writer.WriteStringToNull(unityVersion);
            }
            if (header.m_Version >= 8)
            {
                writer.Write((int)m_TargetPlatform);
            }
            if (header.m_Version >= 13)
            {
                writer.Write(m_EnableTypeTree);
            }
            writer.Write(m_Types.Count);
            for (int i = 0; i < m_Types.Count; i++)
            {
                WriteSerializedType(m_Types[i]);
            }
            if (header.m_Version >= 7 && header.m_Version < 14)
            {
                writer.Write(1);
            }
            MemoryStream memoryStream = new MemoryStream();
            for (int j = 0; j < m_Objects.Count; j++)
            {
                m_Objects[j].byteSize = (uint)m_Objects[j].data.Length;
                m_Objects[j].byteStart = (uint)memoryStream.Position;
                memoryStream.Write(m_Objects[j].data, 0, (int)m_Objects[j].byteSize);
                long num = memoryStream.Position % 8;
                if (num != 0L)
                {
                    memoryStream.Position += 8 - num;
                }
            }
            writer.Write(m_Objects.Count);
            for (int k = 0; k < m_Objects.Count; k++)
            {
                ObjectInfo objectInfo = m_Objects[k];
                if (header.m_Version < 14)
                {
                    writer.Write((int)objectInfo.m_PathID);
                }
                else
                {
                    writer.AlignStream(4);
                    writer.Write(objectInfo.m_PathID);
                }
                writer.Write(objectInfo.byteStart);
                writer.Write(objectInfo.byteSize);
                writer.Write(objectInfo.typeID);
                if (header.m_Version < 16)
                {
                    writer.Write(objectInfo.classID);
                    writer.Write((ushort)0);
                }
                if (header.m_Version == 15 || header.m_Version == 16)
                {
                    writer.Write((byte)0);
                }
            }
            if (header.m_Version >= 11)
            {
                writer.Write(m_ScriptTypes.Count);
                for (int l = 0; l < m_ScriptTypes.Count; l++)
                {
                    writer.Write(m_ScriptTypes[l].localSerializedFileIndex);
                    if (header.m_Version < 14)
                    {
                        writer.Write(m_ScriptTypes[l].localIdentifierInFile);
                        continue;
                    }
                    writer.AlignStream(4);
                    writer.Write(m_ScriptTypes[l].localIdentifierInFile);
                }
            }
            writer.Write(m_Externals.Count);
            for (int m = 0; m < m_Externals.Count; m++)
            {
                FileIdentifier fileIdentifier = m_Externals[m];
                if (header.m_Version >= 6)
                {
                    writer.WriteStringToNull("");
                }
                if (header.m_Version >= 5)
                {
                    writer.Write(fileIdentifier.guid.ToByteArray());
                    writer.Write(fileIdentifier.type);
                }
                writer.WriteStringToNull(fileIdentifier.pathName);
            }
            writer.WriteStringToNull("");
            long num2 = writer.Position - 20;
            writer.AlignStream(32);
            uint value;
            if (writer.Position > 4096)
            {
                value = (uint)writer.Position;
            }
            else
            {
                value = 4096u;
                writer.Position = 4096L;
            }
            memoryStream.Position = 0L;
            memoryStream.CopyTo(writer.BaseStream);
            long length = writer.BaseStream.Length;
            writer.Position = 0L;
            writer.endian = EndianType.BigEndian;
            writer.Write((uint)num2);
            writer.Write((uint)length);
            writer.Position += 4L;
            writer.Write(value);
            writer.Position = 0L;
            byte[] array = new byte[writer.BaseStream.Length];
            writer.BaseStream.Read(array, 0, array.Length);
            return array;
        }

        public long GetPathIDByName(string name)
        {
            foreach (AssetInfo item in assetinfolist)
            {
                if (Path.GetFileName(item.path) == name)
                {
                    return item.pPtr.pathID;
                }
            }
            return -1L;
        }

        private void SetVersion(string stringVersion)
        {
            unityVersion = stringVersion;
            string[] array = Regex.Replace(stringVersion, "\\d", "", RegexOptions.NonBacktracking | RegexOptions.Compiled)
                .Split('.', StringSplitOptions.RemoveEmptyEntries);
            buildType = new BuildType(array[0]);
            string[] source = Regex.Replace(stringVersion, "\\D", ".", RegexOptions.NonBacktracking | RegexOptions.Compiled)
                .Split('.', StringSplitOptions.RemoveEmptyEntries);
            version = source.Select(int.Parse).ToArray();
        }

        private SerializedType ReadSerializedType()
        {
            SerializedType serializedType = new SerializedType();
            serializedType.classID = reader.ReadInt32();
            if (header.m_Version >= 16)
            {
                serializedType.m_IsStrippedType = reader.ReadBoolean();
            }
            if (header.m_Version >= 17)
            {
                serializedType.m_ScriptTypeIndex = reader.ReadInt16();
            }
            if (header.m_Version >= 13)
            {
                if ((header.m_Version < 16 && serializedType.classID < 0) || (header.m_Version >= 16 && serializedType.classID == 114))
                {
                    serializedType.m_ScriptID = reader.ReadBytes(16);
                }
                serializedType.m_OldTypeHash = reader.ReadBytes(16);
            }
            if (m_EnableTypeTree)
            {
                List<TypeTreeNode> list = new List<TypeTreeNode>();
                if (header.m_Version >= 12 || header.m_Version == 10)
                {
                    ReadTypeTree5(list);
                }
                else
                {
                    ReadTypeTree(list);
                }
                serializedType.m_Nodes = list;
            }
            return serializedType;
        }

        private void WriteSerializedType(SerializedType type)
        {
            writer.Write(type.classID);
            if (header.m_Version >= 16)
            {
                writer.Write(type.m_IsStrippedType);
            }
            if (header.m_Version >= 17)
            {
                writer.Write(type.m_ScriptTypeIndex);
            }
            if (header.m_Version >= 13)
            {
                if ((header.m_Version < 16 && type.classID < 0) || (header.m_Version >= 16 && type.classID == 114))
                {
                    writer.Write(type.m_ScriptID);
                }
                writer.Write(type.m_OldTypeHash);
            }
            if (m_EnableTypeTree)
            {
                if (header.m_Version >= 12 || header.m_Version == 10)
                {
                    WriteTypeTree5(type.m_Nodes);
                }
                else
                {
                    WriteTypeTree(type.m_Nodes);
                }
            }
        }

        private void WriteTypeTree(List<TypeTreeNode> typeTree)
        {
            foreach (TypeTreeNode item in typeTree)
            {
                writer.WriteStringToNull(item.m_Type);
                writer.WriteStringToNull(item.m_Name);
                writer.Write(item.m_ByteSize);
                if (header.m_Version == 2)
                {
                    writer.Write(0);
                }
                if (header.m_Version != 3)
                {
                    writer.Write(item.m_Index);
                }
                writer.Write(item.m_IsArray);
                writer.Write(item.m_Version);
                if (header.m_Version != 3)
                {
                    writer.Write(item.m_MetaFlag);
                }
                writer.Write(item.m_childrenCount);
            }
        }

        private void ReadTypeTree(List<TypeTreeNode> typeTree, int depth = 0)
        {
            TypeTreeNode typeTreeNode = new TypeTreeNode();
            typeTree.Add(typeTreeNode);
            typeTreeNode.m_Level = depth;
            typeTreeNode.m_Type = reader.ReadStringToNull();
            typeTreeNode.m_Name = reader.ReadStringToNull();
            typeTreeNode.m_ByteSize = reader.ReadInt32();
            if (header.m_Version == 2)
            {
                reader.ReadInt32();
            }
            if (header.m_Version != 3)
            {
                typeTreeNode.m_Index = reader.ReadInt32();
            }
            typeTreeNode.m_IsArray = reader.ReadInt32();
            typeTreeNode.m_Version = reader.ReadInt32();
            if (header.m_Version != 3)
            {
                typeTreeNode.m_MetaFlag = reader.ReadInt32();
            }
            typeTreeNode.m_childrenCount = reader.ReadInt32();
            for (int i = 0; i < typeTreeNode.m_childrenCount; i++)
            {
                ReadTypeTree(typeTree, depth + 1);
            }
        }

        private void WriteTypeTree5(List<TypeTreeNode> typeTree)
        {
            writer.Write(typeTree.Count);
            long position = writer.Position;
            writer.Position += 4L;
            using BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream());
            Dictionary<string, int> dictionary = new Dictionary<string, int>();
            for (int i = 0; i < typeTree.Count; i++)
            {
                TypeTreeNode typeTreeNode = typeTree[i];
                writer.Write((ushort)typeTreeNode.m_Version);
                writer.Write((byte)typeTreeNode.m_Level);
                writer.Write(typeTreeNode.m_IsArray != 0);
                if (CommonString.StringBuffer.ContainsValue(typeTreeNode.m_Type))
                {
                    writer.Write((ushort)GetKey(typeTreeNode.m_Type));
                    writer.Write((ushort)32768);
                }
                else
                {
                    if (dictionary.ContainsKey(typeTreeNode.m_Type))
                    {
                        writer.Write((ushort)dictionary[typeTreeNode.m_Type]);
                    }
                    else
                    {
                        dictionary.Add(typeTreeNode.m_Type, (int)binaryWriter.BaseStream.Position);
                        writer.Write((ushort)binaryWriter.BaseStream.Position);
                        binaryWriter.WriteStringToNull(typeTreeNode.m_Type);
                    }
                    writer.Write((ushort)0);
                }
                if (CommonString.StringBuffer.ContainsValue(typeTreeNode.m_Name))
                {
                    writer.Write((ushort)GetKey(typeTreeNode.m_Name));
                    writer.Write((ushort)32768);
                }
                else
                {
                    if (dictionary.ContainsKey(typeTreeNode.m_Name))
                    {
                        writer.Write((ushort)dictionary[typeTreeNode.m_Name]);
                    }
                    else
                    {
                        dictionary.Add(typeTreeNode.m_Name, (int)binaryWriter.BaseStream.Position);
                        writer.Write((ushort)binaryWriter.BaseStream.Position);
                        binaryWriter.WriteStringToNull(typeTreeNode.m_Name);
                    }
                    writer.Write((ushort)0);
                }
                writer.Write(typeTreeNode.m_ByteSize);
                writer.Write(typeTreeNode.m_Index);
                writer.Write(typeTreeNode.m_MetaFlag);
            }
            binaryWriter.BaseStream.Position = 0L;
            binaryWriter.BaseStream.CopyTo(writer.BaseStream);
            writer.Position = position;
            writer.Write((int)binaryWriter.BaseStream.Length);
            writer.Seek(0, SeekOrigin.End);
        }

        private void ReadTypeTree5(List<TypeTreeNode> typeTree)
        {
            int num = reader.ReadInt32();
            int num2 = reader.ReadInt32();
            reader.Position += num * 24;
            using BinaryReader binaryReader = new BinaryReader(new MemoryStream(reader.ReadBytes(num2)));
            reader.Position -= num * 24 + num2;
            for (int i = 0; i < num; i++)
            {
                TypeTreeNode typeTreeNode = new TypeTreeNode();
                typeTree.Add(typeTreeNode);
                typeTreeNode.m_Version = reader.ReadUInt16();
                typeTreeNode.m_Level = reader.ReadByte();
                typeTreeNode.m_IsArray = (reader.ReadBoolean() ? 1 : 0);
                ushort num3 = reader.ReadUInt16();
                if (reader.ReadUInt16() == 0)
                {
                    binaryReader.BaseStream.Position = num3;
                    typeTreeNode.m_Type = binaryReader.ReadStringToNull();
                }
                else
                {
                    typeTreeNode.m_Type = (CommonString.StringBuffer.ContainsKey(num3) ? CommonString.StringBuffer[num3] : num3.ToString());
                }
                ushort num4 = reader.ReadUInt16();
                if (reader.ReadUInt16() == 0)
                {
                    binaryReader.BaseStream.Position = num4;
                    typeTreeNode.m_Name = binaryReader.ReadStringToNull();
                }
                else
                {
                    typeTreeNode.m_Name = (CommonString.StringBuffer.ContainsKey(num4) ? CommonString.StringBuffer[num4] : num4.ToString());
                }
                typeTreeNode.m_ByteSize = reader.ReadInt32();
                typeTreeNode.m_Index = reader.ReadInt32();
                typeTreeNode.m_MetaFlag = reader.ReadInt32();
            }
            reader.Position += num2;
        }

        private int GetKey(string str)
        {
            foreach (KeyValuePair<int, string> item in CommonString.StringBuffer)
            {
                if (item.Value == str)
                {
                    return item.Key;
                }
            }
            return 0;
        }
    }
}
