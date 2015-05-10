using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;

namespace dp2Manager
{

    public class QuickRights
    {
        XmlDocument dom = null;

        public event SetRightsEventHandle SetRights = null;

        public event GetNodeStyleEventHandle GetNodeStyle = null;

        /*
        public string ServerRights = "";
        public string DatabaseRights = "";
        public string DirectoryRights = "";
        public string FileRights = "";
         */

        public static bool MatchType(int nType,
              string strType)
        {
            if (nType == ResTree.RESTYPE_SERVER
                && strType == "server")
                return true;
            if (nType == ResTree.RESTYPE_DB
                && strType == "database")
                return true;
            if (nType == ResTree.RESTYPE_FOLDER
                && strType == "directory")
                return true;
            if (nType == ResTree.RESTYPE_FILE
                && strType == "file")
                return true;

            return false;
        }

        // 将xml文件中所使用的风格字符串翻译为ResTree.RESSTYLE_???类型的整数
        public static int GetStyleInt(string strStyle)
        {
            if (String.IsNullOrEmpty(strStyle) == true)
                return 0;

            if (String.Compare(strStyle, "userdatabase", true) == 0)
                return ResTree.RESSTYLE_USERDATABASE;

            throw (new Exception("未知的风格字符串 '" + strStyle + "'"));
        }

        public static int Build(XmlDocument CfgDom,
            string strProjectName,
            out QuickRights quickrights,
            out string strError)
        {
            quickrights = null;
            strError = "";

            string strXPath = "//style[@name='" + strProjectName + "']";
            XmlNode parent = CfgDom.DocumentElement.SelectSingleNode(strXPath);
            if (parent == null)
            {
                strError = "名字为 '" + strProjectName + "' 的方案没有找到";
                return -1;
            }

            quickrights = new QuickRights();

            quickrights.dom = new XmlDocument();

            quickrights.dom.LoadXml(parent.OuterXml);

            return 0;
        }

        // 外部调用
        // parameters:
        //      ois 要处理的顶层对象数组
        public int ModiRights(List<TreeNode> selectedTreeNodes,
            TreeNode root)
        {
            List<XmlNode> cfgNodes = new List<XmlNode>();

            if (root.ImageIndex == ResTree.RESTYPE_SERVER)
            {
                XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("object");
                for (int i = 0; i < nodes.Count; i++)
                {
                    cfgNodes.Add(nodes[i]);
                }
            }
            else if (root.ImageIndex == ResTree.RESTYPE_DB)
            {
                XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("object/object[@type='database' or @type='*']");
                for (int i = 0; i < nodes.Count; i++)
                {
                    cfgNodes.Add(nodes[i]);
                }
            }
            else
            {
                Debug.Assert(false, "不可能的情况");
            }

            ModiRights(0,
                selectedTreeNodes,
                root,
                cfgNodes);
            return 0;
        }

        /*
        // 在配置树上找到命中的全部配置节点
        void HitCfgNodes(string strPath,
            XmlNode parentXmlNode,
            ref List<XmlNode> cfgnodes)
        {
            if (strPath == "")
                return;

            string strCur = StringUtil.GetFirstPartPath(ref strPath);

            for (int i = 0; i < parentXmlNode.ChildNodes.Count; i++)
            {
                XmlNode xmlnode = parentXmlNode.ChildNodes[i];
                if (xmlnode.NodeType != XmlNodeType.Element)
                    continue;
                string strName = DomUtil.GetAttr(xmlnode, "name");
                if (strName == "" || strName == "*")
                {
                    goto DONEST;
                }

                if (strName == strCur)
                    goto DONEST;

                continue;

            DONEST:
                if (strPath == "")  // 末级
                    cfgnodes.Add(xmlnode);
                else
                {
                    HitCfgNodes(strPath,
                        xmlnode,
                        ref cfgnodes);
                }
            }
        }
         */

        // parameters:
        //      bLocateCfgNode  ==true treenode不是从和cfgParentNode对应的根开始的，而是需要根据事件来查找其路径并找到对应的cfgnode对象
        void ModiRights(
            int nFound,
            List<TreeNode> selectedTreeNodes,
            List<TreeNode> treenodes,
            List<XmlNode> cfgnodes)
        {

            for (int i = 0; i < treenodes.Count; i++)
            {
                TreeNode curtreenode = treenodes[i];

                ModiRights(nFound,
                    selectedTreeNodes,
                    curtreenode,
                    cfgnodes);
            }

        }

        // 按照预定模式修改一个节点以及以下的全部子节点的权限
        void ModiRights(int nFound,
            List<TreeNode> selectedTreeNodes,
            TreeNode curtreenode,
            List<XmlNode> cfgnodes)
        {
            for (int i = 0; i < cfgnodes.Count; i++)
            {
                // 当前xml节点信息
                XmlNode node = cfgnodes[i];

                string strType = DomUtil.GetAttr(node, "type");
                string strName = DomUtil.GetAttr(node, "name");
                string strRights = DomUtil.GetAttrDiff(node, "rights");
                int nStyle = QuickRights.GetStyleInt(DomUtil.GetAttr(node, "style"));

                // 匹配对象名
                if (strName != "" && strName != "*")
                {
                    // @
                    if (strName != curtreenode.Text)
                        continue;
                }

                // 匹配对象类型
                bool bRet = QuickRights.MatchType(curtreenode.ImageIndex,
                    strType);
                if (bRet == false)
                    continue;

                // 匹配对象风格
                if (nStyle != 0)
                {
                    if (this.GetNodeStyle == null)
                    { 
                        // 缺省行为
                        /*
                        if (nStyle != ResRightTree.GetNodeStyle(curtreenode))
                            continue;
                         */
                    }
                    else
                    {
                        GetNodeStyleEventArgs e = new GetNodeStyleEventArgs();
                        e.Node = curtreenode;
                        e.Style = 0;
                        this.GetNodeStyle(this, e);
                        if (nStyle != e.Style)
                            continue;
                    }
                }

                int nIndex = -1;
                if (nFound == 0)
                    nIndex = selectedTreeNodes.IndexOf(curtreenode);

                // 触发事件
                // 如果strRights == null，表示当前对象不需修改其rights值，但是递归仍然要进行
                if (strRights != null 
                    && (nIndex != -1 || nFound > 0))
                {
                    if (this.SetRights != null)
                    {
                        SetRightsEventArgs e = new SetRightsEventArgs();
                        e.Node = curtreenode;
                        if (strRights == "{clear}" || strRights == "{null}")
                            e.Rights = null;
                        else
                            e.Rights = strRights;
                        this.SetRights(this, e);
                    }

                }

            // DOCHILDREN:

                // 组织树子对象数组
                List<TreeNode> nodes = new List<TreeNode>();
                for (int j = 0; j < curtreenode.Nodes.Count; j++)
                {
                    nodes.Add(curtreenode.Nodes[j]);
                }

                // 递归
                if (nodes.Count != 0)
                {
                    List<XmlNode> chidrencfgnodes = new List<XmlNode>();
                    for (int j = 0; j < node.ChildNodes.Count; j++)
                    {
                        XmlNode cur = node.ChildNodes[j];

                        if (cur.NodeType != XmlNodeType.Element)
                            continue;

                        if (cur.Name != "object")
                            continue;

                        chidrencfgnodes.Add(cur);
                    }

                    if (nIndex != -1)
                        nFound++;
                    ModiRights(nFound, 
                        selectedTreeNodes,
                        nodes,
                        chidrencfgnodes);
                    if (nIndex != -1)
                        nFound--;
                }

            }

        }

        /*
        public static List<TreeNode> GetTreeNodeCollection(List<ObjectInfo> ois,
            GetTreeNodeByPathEventHandle callback)
        {
            List<TreeNode> treenodes = new List<TreeNode>();
            for (int i = 0; i < ois.Count; i++)
            {
                GetTreeNodeByPathEventArgs e = new GetTreeNodeByPathEventArgs();
                e.Path = ois[i].Path;
                e.Node = null;
                callback(null, e);
                treenodes.Add(e.Node);
            }

            return treenodes;
        }
         */
    }

    public class ObjectInfo
    {
        public int ImageIndex = -1;
        public string Path = "";
        public string Url = "";
    }

    public delegate void SetRightsEventHandle(object sender,
    SetRightsEventArgs e);

    public class SetRightsEventArgs : EventArgs
    {
        public TreeNode Node = null;
        public string Rights = "";
    }

    //

    // 得到treenode对象的style参数
    public delegate void GetNodeStyleEventHandle(object sender,
     GetNodeStyleEventArgs e);

    public class GetNodeStyleEventArgs : EventArgs
    {
        public TreeNode Node = null;
        public int Style = 0;
    }
}
