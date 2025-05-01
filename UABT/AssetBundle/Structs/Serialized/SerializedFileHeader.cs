// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

namespace Hi3Helper.UABT
{
    public class SerializedFileHeader
    {
        public uint MMetadataSize;

        public uint MFileSize;

        public uint MVersion;

        public uint MDataOffset;

        public byte MEndianess;

        public byte[] MReserved;
    }
}
