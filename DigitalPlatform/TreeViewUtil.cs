using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

/*
using System.Threading;
using System.Drawing;
using System.Runtime.InteropServices;
 */

namespace DigitalPlatform.GUI
{
    // TreeView实用函数
    public class TreeViewUtil
    {
        // 获得所有被checkbox选中的节点
        public static TreeNode[] GetCheckedNodes(TreeView tree)
        {
            ArrayList aNode = new ArrayList();

            int i = 0;
            for (i = 0; i < tree.Nodes.Count; i++)
            {
                GetCheckedNodes(tree.Nodes[i], ref aNode);
            }

            TreeNode[] result = new TreeNode[aNode.Count];
            for (i = 0; i < aNode.Count; i++)
            {
                result[i] = (TreeNode)aNode[i];
            }

            return result;
        }
        // 获得一个节点或其下被checkbox选中的节点
        static void GetCheckedNodes(TreeNode node,
            ref ArrayList aNode)
        {
            if (node.Checked == true)
                aNode.Add(node);

            for (int i = 0; i < node.Nodes.Count; i++)
            {
                GetCheckedNodes(node.Nodes[i], ref aNode);
            }
        }

        // 根据路径,设置一个node的checked状态
        // TODO: !=null 可以理解为true
        public static TreeNode CheckNode(TreeView tree,
            string strPath,
            bool bChecked)
        {
            TreeNode node = TreeViewUtil.GetTreeNode(tree, strPath);
            if (node == null)
                return null;
            node.Checked = bChecked;
            return node;
        }

        // 从树的根开始，根据路径定位节点。路径中用'/'分隔
        public static TreeNode GetTreeNode(TreeView treeView,
            string strPath)
        {
            string[] aName = strPath.Split(new Char[] { '/','\\' });    // 2007/8/2 changed 兼容原来，用/；现在推荐用\

            TreeNode node = null;
            TreeNode nodeThis = null;
            for (int i = 0; i < aName.Length; i++)
            {
                TreeNodeCollection nodes = null;

                if (node == null)
                    nodes = treeView.Nodes;
                else
                    nodes = node.Nodes;

                bool bFound = false;
                for (int j = 0; j < nodes.Count; j++)
                {
                    if (aName[i] == nodes[j].Text)
                    {
                        bFound = true;
                        nodeThis = nodes[j];
                        break;
                    }
                }
                if (bFound == false)
                    return null;

                node = nodeThis;

            }

            return nodeThis;
        }

        // 从树的某个节点开始，根据路径定位节点。路径中用'/'分隔
        // 2006/1/12
        public static TreeNode GetTreeNode(TreeNode start,
            string strPath)
        {
            if (start == null)
                throw new Exception("start参数不能为null");

            string[] aName = strPath.Split(new Char[] { '/', '\\' });   // 2007/8/2 changed 兼容原来，用/；现在推荐用\

            TreeNode node = start;
            TreeNode nodeThis = null;
            for (int i = 0; i < aName.Length; i++)
            {
                TreeNodeCollection nodes = null;
                nodes = node.Nodes;

                bool bFound = false;
                for (int j = 0; j < nodes.Count; j++)
                {
                    if (aName[i] == nodes[j].Text)
                    {
                        bFound = true;
                        nodeThis = nodes[j];
                        break;
                    }
                }
                if (bFound == false)
                    return null;

                node = nodeThis;

            }

            return nodeThis;
        }

        // 根据路径选定节点。路径中用'/'分隔
        public static void SelectTreeNode(TreeView treeView,
            string strPath,
            char delimeter)
        {
            string[] aName = null;

#if DEBUG
            if (delimeter != (char)0)
            {
                Debug.Assert(treeView.PathSeparator == new string(delimeter, 1), "delimeter最好和treeview的PathSeparator一致");
            }
#endif

            if (delimeter == (char)0)
                aName = strPath.Split(new Char[] { '/', '\\' });   // 2007/8/2 changed 兼容原来，用/；现在推荐用\
            else
                aName = strPath.Split(new Char[] { delimeter });   // 2007/8/2


            TreeNode node = null;
            TreeNode nodeThis = null;
            for (int i = 0; i < aName.Length; i++)
            {
                TreeNodeCollection nodes = null;

                if (node == null)
                    nodes = treeView.Nodes;
                else
                    nodes = node.Nodes;

                bool bFound = false;
                for (int j = 0; j < nodes.Count; j++)
                {
                    if (aName[i] == nodes[j].Text)
                    {
                        bFound = true;
                        nodeThis = nodes[j];
                        break;
                    }
                }
                if (bFound == false)
                    break;

                node = nodeThis;

            }

            if (nodeThis != null && nodeThis.Parent != null)
                nodeThis.Parent.Expand();

            treeView.SelectedNode = nodeThis;
        }

        /*
        // 兼容原来的版本
        public static string GetPath(TreeNode node)
        {
            // 为了兼容原来的版本
            return GetPath(node, '/');
        }*/


        // 得到一个节点的路径
        // 为了兼容原来的版本，用'/'；现在推荐用'\'
        public static string GetPath(TreeNode node,
            char delimeter)
        {
            TreeNode nodeCur = node;
            string strPath = "";

            while (true)
            {
                if (nodeCur == null)
                    break;
                if (strPath != "")
                    strPath = nodeCur.Text + new string(delimeter, 1) + strPath;
                else
                    strPath = nodeCur.Text;

                nodeCur = nodeCur.Parent;
            }

            return strPath;
        }

        /* 为促使修改代码，而故意隐藏掉
        // 获得当前已选中节点的路径
        public static string GetSelectedTreeNodePath(TreeView treeView)
        {
            return GetPath(treeView.SelectedNode);
        }*/

        // 获得当前已选中节点的路径
        public static string GetSelectedTreeNodePath(TreeView treeView,
            char delimeter)
        {
            return GetPath(treeView.SelectedNode, delimeter);
        }


        // 在节点儿子中搜寻具有特定名字的对象
        public static TreeNode FindNodeByText(TreeNode parent,
            string strText)
        {
            for (int i = 0; i < parent.Nodes.Count; i++)
            {
                TreeNode node = parent.Nodes[i];
                if (node.Text == strText)
                    return node;
            }

            return null;
        }

        // 在树顶层对象中搜寻具有特定名字的对象
        public static TreeNode FindTopNodeByText(TreeView tree,
            string strText)
        {
            for (int i = 0; i < tree.Nodes.Count; i++)
            {
                TreeNode node = tree.Nodes[i];
                if (node.Text == strText)
                    return node;
            }

            return null;
        }

        // 在节点儿子中搜寻具有特定名字的对象
        public static TreeNode FindNodeByText(TreeView tree,
            TreeNode parent,
            string strText)
        {
            if (parent != null)
            {
                for (int i = 0; i < parent.Nodes.Count; i++)
                {
                    TreeNode node = parent.Nodes[i];
                    if (node.Text == strText)
                        return node;
                }

                return null;
            }
            else
            {
                if (tree == null)
                {
                    throw (new Exception("当parent参数为空的时候，tree参数不能为空"));
                }

                for (int i = 0; i < tree.Nodes.Count; i++)
                {
                    TreeNode node = tree.Nodes[i];
                    if (node.Text == strText)
                        return node;
                }

                return null;
            }

        }
    }
}
