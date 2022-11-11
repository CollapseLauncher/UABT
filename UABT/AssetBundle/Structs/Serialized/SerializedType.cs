using Hi3Helper.UABT.TypeTree;
using System.Collections.Generic;

namespace Hi3Helper.UABT
{
    public class SerializedType
    {
        public int classID;

        public bool m_IsStrippedType;

        public short m_ScriptTypeIndex = -1;

        public List<TypeTreeNode> m_Nodes;

        public byte[] m_ScriptID;

        public byte[] m_OldTypeHash;
    }
}
