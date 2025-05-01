using Hi3Helper.UABT.Binary;
using Hi3Helper.UABT.TypeTree;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

// ReSharper disable all

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
                header.MMetadataSize = reader.ReadUInt32();
                header.MFileSize = reader.ReadUInt32();
                header.MVersion = reader.ReadUInt32();
                header.MDataOffset = reader.ReadUInt32();
                if (header.MVersion >= 9)
                {
                    header.MEndianess = reader.ReadByte();
                    header.MReserved = reader.ReadBytes(3);
                    m_FileEndianess = (EndianType)header.MEndianess;
                }
                else
                {
                    reader.Position = header.MFileSize - header.MMetadataSize;
                    m_FileEndianess = (EndianType)reader.ReadByte();
                }
                if (m_FileEndianess == EndianType.LittleEndian)
                {
                    reader.Endian = EndianType.LittleEndian;
                }
                if (header.MVersion >= 7)
                {
                    unityVersion = reader.ReadStringToNull();
                    SetVersion(unityVersion);
                }
                if (header.MVersion >= 8)
                {
                    m_TargetPlatform = (BuildTarget)reader.ReadInt32();
                    if (!Enum.IsDefined(typeof(BuildTarget), m_TargetPlatform))
                    {
                        m_TargetPlatform = BuildTarget.UnknownPlatform;
                    }
                }
                if (header.MVersion >= 13)
                {
                    m_EnableTypeTree = reader.ReadBoolean();
                }
                int num = reader.ReadInt32();
                m_Types = new List<SerializedType>(num);
                for (int i = 0; i < num; i++)
                {
                    m_Types.Add(ReadSerializedType());
                }
                if (header.MVersion >= 7 && header.MVersion < 14)
                {
                    reader.ReadInt32();
                }
                int num2 = reader.ReadInt32();
                m_Objects = new List<ObjectInfo>(num2);
                for (int j = 0; j < num2; j++)
                {
                    ObjectInfo objectInfo = new ObjectInfo();
                    if (header.MVersion < 14)
                    {
                        objectInfo.MPathID = reader.ReadInt32();
                    }
                    else
                    {
                        reader.AlignStream(4);
                        objectInfo.MPathID = reader.ReadInt64();
                    }
                    objectInfo.ByteStart = reader.ReadUInt32();
                    objectInfo.ByteSize = reader.ReadUInt32();
                    objectInfo.TypeID = reader.ReadInt32();
                    if (header.MVersion < 16)
                    {
                        objectInfo.ClassID = reader.ReadUInt16();
                        objectInfo.SerializedType = m_Types.Find((SerializedType x) => x.ClassID == objectInfo.TypeID);
                        reader.ReadUInt16();
                    }
                    else
                    {
                        SerializedType serializedType = m_Types[objectInfo.TypeID];
                        objectInfo.SerializedType = serializedType;
                        objectInfo.ClassID = serializedType.ClassID;
                    }
                    if (header.MVersion == 15 || header.MVersion == 16)
                    {
                        reader.ReadByte();
                    }
                    m_Objects.Add(objectInfo);
                }
                if (header.MVersion >= 11)
                {
                    int num3 = reader.ReadInt32();
                    m_ScriptTypes = new List<LocalSerializedObjectIdentifier>(num3);
                    for (int k = 0; k < num3; k++)
                    {
                        LocalSerializedObjectIdentifier localSerializedObjectIdentifier = new LocalSerializedObjectIdentifier
                        {
                            LocalSerializedFileIndex = reader.ReadInt32()
                        };
                        if (header.MVersion < 14)
                        {
                            localSerializedObjectIdentifier.LocalIdentifierInFile = reader.ReadInt32();
                        }
                        else
                        {
                            reader.AlignStream(4);
                            localSerializedObjectIdentifier.LocalIdentifierInFile = reader.ReadInt64();
                        }
                        m_ScriptTypes.Add(localSerializedObjectIdentifier);
                    }
                }
                int num4 = reader.ReadInt32();
                m_Externals = new List<FileIdentifier>(num4);
                for (int l = 0; l < num4; l++)
                {
                    FileIdentifier fileIdentifier = new FileIdentifier();
                    if (header.MVersion >= 6)
                    {
                        reader.ReadStringToNull();
                    }
                    if (header.MVersion >= 5)
                    {
                        fileIdentifier.Guid = new Guid(reader.ReadBytes(16));
                        fileIdentifier.Type = reader.ReadInt32();
                    }
                    fileIdentifier.PathName = reader.ReadStringToNull();
                    fileIdentifier.FileName = Path.GetFileName(fileIdentifier.PathName);
                    m_Externals.Add(fileIdentifier);
                }
                _ = header.MVersion;
                _ = 5;
                for (int m = 0; m < m_Objects.Count; m++)
                {
                    reader.Position = m_Objects[m].ByteStart + header.MDataOffset;
                    m_Objects[m].Data = reader.ReadBytes((int)m_Objects[m].ByteSize);
                }
                ObjectInfo objectInfo2 = m_Objects.Find((ObjectInfo x) => x.ClassID == 142);
                if (objectInfo2.Data != null)
                {
                    assetinfolist = AssetBundle.GetFileList(objectInfo2.Data);
                }
                valid = true;
            }
            catch
            {
            }
        }

        public byte[] GetDataFirstOrDefaultByName(string name)
        {
            long fileID = assetinfolist.Where(x => Path.GetFileName(x.Path) == name).FirstOrDefault().PPtr.PathID;
            return m_Objects.Where(x => x.MPathID == fileID).FirstOrDefault().Data;
        }

        private void SetVersion(string stringVersion)
        {
            unityVersion = stringVersion;
            string[] array = Regex.Replace(stringVersion, "\\d", "",
                RegexOptions.NonBacktracking |
                RegexOptions.Compiled)
                .Split('.', StringSplitOptions.RemoveEmptyEntries);
            buildType = new BuildType(array[0]);
            string[] source = Regex.Replace(stringVersion, "\\D", ".",
                RegexOptions.NonBacktracking |
                RegexOptions.Compiled)
                .Split('.', StringSplitOptions.RemoveEmptyEntries);
            version = source.Select(int.Parse).ToArray();
        }

        private SerializedType ReadSerializedType()
        {
            SerializedType serializedType = new SerializedType();
            serializedType.ClassID = reader.ReadInt32();
            if (header.MVersion >= 16)
            {
                serializedType.MIsStrippedType = reader.ReadBoolean();
            }
            if (header.MVersion >= 17)
            {
                serializedType.MScriptTypeIndex = reader.ReadInt16();
            }
            if (header.MVersion >= 13)
            {
                if ((header.MVersion < 16 && serializedType.ClassID < 0) || (header.MVersion >= 16 && serializedType.ClassID == 114))
                {
                    serializedType.MScriptID = reader.ReadBytes(16);
                }
                serializedType.MOldTypeHash = reader.ReadBytes(16);
            }
            if (m_EnableTypeTree)
            {
                List<TypeTreeNode> list = new List<TypeTreeNode>();
                if (header.MVersion >= 12 || header.MVersion == 10)
                {
                    ReadTypeTree5(list);
                }
                else
                {
                    ReadTypeTree(list);
                }
                serializedType.MNodes = list;
            }
            return serializedType;
        }

        private void ReadTypeTree(List<TypeTreeNode> typeTree, int depth = 0)
        {
            TypeTreeNode typeTreeNode = new TypeTreeNode();
            typeTree.Add(typeTreeNode);
            typeTreeNode.MLevel = depth;
            typeTreeNode.MType = reader.ReadStringToNull();
            typeTreeNode.MName = reader.ReadStringToNull();
            typeTreeNode.MByteSize = reader.ReadInt32();
            if (header.MVersion == 2)
            {
                reader.ReadInt32();
            }
            if (header.MVersion != 3)
            {
                typeTreeNode.MIndex = reader.ReadInt32();
            }
            typeTreeNode.MIsArray = reader.ReadInt32();
            typeTreeNode.MVersion = reader.ReadInt32();
            if (header.MVersion != 3)
            {
                typeTreeNode.MMetaFlag = reader.ReadInt32();
            }
            typeTreeNode.MChildrenCount = reader.ReadInt32();
            for (int i = 0; i < typeTreeNode.MChildrenCount; i++)
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
                typeTreeNode.MVersion = reader.ReadUInt16();
                typeTreeNode.MLevel = reader.ReadByte();
                typeTreeNode.MIsArray = (reader.ReadBoolean() ? 1 : 0);
                ushort num3 = reader.ReadUInt16();
                if (reader.ReadUInt16() == 0)
                {
                    binaryReader.BaseStream.Position = num3;
                    typeTreeNode.MType = binaryReader.ReadStringToNull();
                }
                else
                {
                    typeTreeNode.MType = (CommonString.StringBuffer.ContainsKey(num3) ? CommonString.StringBuffer[num3] : num3.ToString());
                }
                ushort num4 = reader.ReadUInt16();
                if (reader.ReadUInt16() == 0)
                {
                    binaryReader.BaseStream.Position = num4;
                    typeTreeNode.MName = binaryReader.ReadStringToNull();
                }
                else
                {
                    typeTreeNode.MName = (CommonString.StringBuffer.ContainsKey(num4) ? CommonString.StringBuffer[num4] : num4.ToString());
                }
                typeTreeNode.MByteSize = reader.ReadInt32();
                typeTreeNode.MIndex = reader.ReadInt32();
                typeTreeNode.MMetaFlag = reader.ReadInt32();
            }
            reader.Position += num2;
        }
    }
}
