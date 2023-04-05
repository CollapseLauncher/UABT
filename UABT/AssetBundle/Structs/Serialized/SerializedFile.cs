using Hi3Helper.UABT.Binary;
using Hi3Helper.UABT.TypeTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Hi3Helper.UABT
{
    public class SerializedFile
    {
        public EndianBinaryReader reader = new EndianBinaryReader(new MemoryStream());

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

        public SerializedFile(Stream readeri)
        {
            DoReadSerializedFiles(new EndianBinaryReader(readeri));
        }

        public SerializedFile(string fullName, Stream readeri)
        {
            this.fullName = fullName;
            fileName = Path.GetFileName(fullName);
            upperFileName = fileName.ToUpper();
            DoReadSerializedFiles(new EndianBinaryReader(readeri));
        }

        public SerializedFile(EndianBinaryReader readeri)
        {
            DoReadSerializedFiles(readeri);
        }

        public SerializedFile(string fullName, EndianBinaryReader readeri)
        {
            this.fullName = fullName;
            fileName = Path.GetFileName(fullName);
            upperFileName = fileName.ToUpper();
            DoReadSerializedFiles(readeri);
        }

        private void DoReadSerializedFiles(EndianBinaryReader readeri)
        {
            readeri.BaseStream.CopyTo(reader.BaseStream);
            reader.Position = 0L;
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

        public byte[] GetDataFirstOrDefaultByName(string name)
        {
            long fileID = assetinfolist.Where(x => Path.GetFileName(x.path) == name).FirstOrDefault().pPtr.pathID;
            return m_Objects.Where(x => x.m_PathID == fileID).FirstOrDefault().data;
        }

        private void SetVersion(string stringVersion)
        {
            unityVersion = stringVersion;
            string[] array = Regex.Replace(stringVersion, "\\d", "",
#if NET7_0_OR_GREATER
                RegexOptions.NonBacktracking |
#endif
                RegexOptions.Compiled)
                .Split('.', StringSplitOptions.RemoveEmptyEntries);
            buildType = new BuildType(array[0]);
            string[] source = Regex.Replace(stringVersion, "\\D", ".",
#if NET7_0_OR_GREATER
                RegexOptions.NonBacktracking |
#endif
                RegexOptions.Compiled)
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
    }
}
