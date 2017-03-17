//==========================================
// Matt Pietrek
// FILE: TypeRefTreeNode.cs
//==========================================
namespace TypeRefViewer
{
    using System;
    using System.Windows.Forms;

    public class TypeRefTreeNode : TreeNode
    { 
        // The System.Reflection.MemberInfo associated with this node
        public String m_strAssembly;
                    
        public TypeRefTreeNode(String caption, String strAssembly)
        {
            Text = caption;
            m_strAssembly = strAssembly;
        }
    }
}

