// ReSharper disable UnusedMember.Global
// ReSharper disable InconsistentNaming
// ReSharper disable IdentifierTypo

using Hi3Helper.UABT.TypeTree;
using System.Collections.Generic;

namespace Hi3Helper.UABT
{
    public class SerializedType
    {
        public int ClassID;

        public bool MIsStrippedType;

        public short MScriptTypeIndex = -1;

        public List<TypeTreeNode> MNodes;

        public byte[] MScriptID;

        public byte[] MOldTypeHash;
    }
}
