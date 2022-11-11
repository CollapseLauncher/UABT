using System.Collections.Generic;
using UABT.TypeTree;

namespace UABT
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
