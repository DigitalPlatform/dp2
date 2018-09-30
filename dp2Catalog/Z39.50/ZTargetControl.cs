using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.GUI;  // for event SetMenuEventHandle
using DigitalPlatform.Xml;
using DigitalPlatform.Z3950;

namespace dp2Catalog
{
    public partial class ZTargetControl : TreeView
    {
        public bool AllowCheckbox = true;
        public MainForm MainForm = null; // 2007/12/16

        public Marc8Encoding Marc8Encoding = null;

        const int TYPE_DIR = 0;
        internal const int TYPE_SERVER_OFFLINE = 1;
        const int TYPE_DATABASE = 2;
        internal const int TYPE_SERVER_ONLINE = 3;
        const int TYPE_DATABASE_NOTINALL = 4;

        XmlDocument dom = null;

        bool m_bChanged = false;

        string m_strFileName = "";

        public event GuiAppendMenuEventHandle OnSetMenu;
        public event ServerChangedEventHandle OnServerChanged;

        public delegate void Delegate_SetNodeImageIndex(TreeNode node,
            int nImageIndex);

        public static void SetNodeImageIndex(TreeNode node,
            int nImageIndex)
        {
            node.ImageIndex = nImageIndex;
            node.SelectedImageIndex = nImageIndex;
        }

        /*
        public static bool IsServer(int nType)
        {
            if (nType == TYPE_SERVER_OFFLINE
                || nType == TYPE_SERVER_ONLINE)
                return true;

            return false;
        }
         * */

        public static bool IsServerType(TreeNode node)
        {
            if (node == null)
                return false;

            if (node.ImageIndex == TYPE_SERVER_OFFLINE
                || node.ImageIndex == TYPE_SERVER_ONLINE)
                return true;

            return false;
        }

        public static bool IsServerOnlineType(TreeNode node)
        {
            if (node == null)
                return false;
            if (node.ImageIndex == TYPE_SERVER_ONLINE)
                return true;

            return false;
        }

        public static bool IsServerOfflineType(TreeNode node)
        {
            if (node == null)
                return false;
            if (node.ImageIndex == TYPE_SERVER_OFFLINE)
                return true;

            return false;
        }

        public static bool IsDirType(TreeNode node)
        {
            if (node == null)
                return false;
            if (node.ImageIndex == TYPE_DIR)
                return true;

            return false;
        }

        // 树节点是否为Database类型?
        public static bool IsDatabaseType(TreeNode node)
        {
            if (node == null)
                return false;
            if (node.ImageIndex == TYPE_DATABASE
                || node.ImageIndex == TYPE_DATABASE_NOTINALL)
                return true;
            return false;
        }

        /*
        public static bool IsServer(TreeNode node)
        {
            if (node == null)
                return false;
            int nType = node.ImageIndex;
            return IsServer(nType);
        }
         * */

        public bool Changed
        {
            get
            {
                return this.m_bChanged;
            }
            set
            {
                this.m_bChanged = value;
            }
        }

        public ZTargetControl()
        {
            InitializeComponent();

            this.ImageList = this.imageList_resIcon;
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            // TODO: Add custom paint code here

            // Calling the base class OnPaint
            base.OnPaint(pe);
        }

        // 从XML文件中装载树结构
        public int Load(string strFileName,
            out string strError)
        {
            strError = "";

            this.m_strFileName = strFileName;

            dom = new XmlDocument();

            try
            {
                dom.Load(strFileName);
            }
            catch (Exception ex)
            {
                strError = "装载文件 '"+strFileName+"' 到XMLDOM时出错: " + ex.Message;
                return -1;
            }

            int nRet = NewOneNodeAndChildren(dom.DocumentElement,
                null,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        public void Save()
        {
            if (this.Changed == true 
                && this.m_strFileName != ""
                && this.dom != null)
            {
                dom.Save(this.m_strFileName);
            }
        }



        // 根据XML节点信息创建TreeView的一个节点和下级节点
        // 用于从XML文件中装载树结构
        // 本函数要递归
        int NewOneNodeAndChildren(XmlNode node,
            TreeNode parent,
            out string strError)
        {
            strError = "";

            TreeNodeCollection treeNodes = null;

            if (parent == null)
                treeNodes = this.Nodes;
            else
                treeNodes = parent.Nodes;

            TreeNode curTreeNode = null;

            if (node.Name != "root")
            {
                string strName = DomUtil.GetAttr(node, "name");

                if (node.Name == "dir")
                {
                    curTreeNode = new TreeNode(strName, TYPE_DIR, TYPE_DIR);
                }
                if (node.Name == "server")
                {
                    curTreeNode = new TreeNode(strName, TYPE_SERVER_OFFLINE, TYPE_SERVER_OFFLINE);
                }
                if (node.Name == "database")
                {
                    if (DomUtil.GetBooleanParam(node,
                        "notInAll",
                        false) == true)
                        curTreeNode = new TreeNode(strName, TYPE_DATABASE_NOTINALL, TYPE_DATABASE_NOTINALL);
                    else
                        curTreeNode = new TreeNode(strName, TYPE_DATABASE, TYPE_DATABASE);
                }

                treeNodes.Add(curTreeNode);

                TreeNodeInfo info = new TreeNodeInfo(node);
                info.Name = strName;
                curTreeNode.Tag = info; // 记忆
            }

            // 递归
            for (int i = 0; i < node.ChildNodes.Count; i++)
            {
                XmlNode child_node = node.ChildNodes[i];
                if (child_node.NodeType != XmlNodeType.Element)
                    continue;

                int nRet = NewOneNodeAndChildren(child_node,
                    curTreeNode,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            return 0;
        }


        // 2007/7/28
        // 将服务器节点的图标变成online/offline状态
        public static void OnlineServerNodeIcon(TreeNode treenode,
            bool bOnline)
        {
            if (treenode == null)
                throw new Exception("treenode为空");

            if (IsServerType(treenode) == false)
                throw new Exception("不是server类型节点");

            if (bOnline == true)
            {
                treenode.ImageIndex = ZTargetControl.TYPE_SERVER_ONLINE;
                treenode.SelectedImageIndex = ZTargetControl.TYPE_SERVER_ONLINE;
            }
            else
            {
                treenode.ImageIndex = ZTargetControl.TYPE_SERVER_OFFLINE;
                treenode.SelectedImageIndex = ZTargetControl.TYPE_SERVER_OFFLINE;
            }
        }

        // 2011/10/12
        public List<string> GetCheckedNodeFullPaths(
            char delimeter = '\\')
        {
            List<string> results = new List<string>();

            TreeNode[] nodes = TreeViewUtil.GetCheckedNodes(this);

            foreach (TreeNode node in nodes)
            {
                results.Add(GetNodeFullPath(node, delimeter));
            }

            return results;
        }

        public void CheckNodes(List<string> paths)
        {
            m_nInInitial++;
            try
            {
                foreach (string path in paths)
                {
                    TreeNode node = TreeViewUtil.CheckNode(this,
                        path,
                        true);
                    if (node != null)
                    {
                        SetNodeColor(node);

                        if (IsDirType(node) == true)
                            node.Expand();
                    }
                }
            }
            finally
            {
                m_nInInitial--;
            }
        }

        // 2007/8/3
        // 得到一个节点的路径。这个函数的特点，是得到纯净的节点名，而不包含命中数部分文字
        public static string GetNodeFullPath(TreeNode node,
            char delimeter)
        {
            TreeNode nodeCur = node;
            string strPath = "";

            while (true)
            {
                if (nodeCur == null)
                    break;

                TreeNodeInfo info = (TreeNodeInfo)nodeCur.Tag;

                if (strPath != "")
                    strPath = info.Name + new string(delimeter, 1) + strPath;
                else
                    strPath = info.Name;

                nodeCur = nodeCur.Parent;
            }

            return strPath;
        }


        // 2007/7/28
        public static TreeNode GetServerNode(TreeNode treenode)
        {
            if (treenode == null)
                return null;

            if (IsServerType(treenode) == true)
            {
                return treenode;
            }

            if (IsDatabaseType(treenode) == true)
            {
                if (treenode.Parent == null)
                {
                    throw new Exception("数据库类型树节点 '" + treenode.Name + "' 居然没有父节点");
                }
                else
                {
                    Debug.Assert(IsServerType(treenode.Parent) == true, "");
                }

                return treenode.Parent;
            }

            return null;    // 不是服务器类型节点
        }

        // 2007/7/28
        // 设置当前树上已经选择的节点的扩展信息
        // parameters:
        //      treenode   服务器或者数据库类型节点
        public static int SetCurrentTargetExtraInfo(
            TreeNode treenode,
            string strExtraInfo,
            out string strError)
        {
            strError = "";


            /*
            TreeNode treenode = null;
            if (startnode != null)
                treenode = startnode;
            else
            {
                treenode = this.SelectedNode;
                if (treenode == null)
                {
                    strError = "尚未选择节点";
                    return 0;
                }
            }
             * */

            if (treenode.ImageIndex == TYPE_DIR)
            {
                strError = "目录类型的树节点，无法进行SetCurrentTargetExtraInfo()";
                return -1;
            }

            // 定位server节点

            if (IsServerType(treenode) == true)
            {
                TreeNodeInfo.SetExtraInfo(treenode, strExtraInfo);
            }

            if (IsDatabaseType(treenode) == true)
            {
                if (treenode.Parent == null)
                {
                    strError = "树节点 '" + treenode.Name + "' 没有父节点";
                    return -1;
                }

                TreeNodeInfo.SetExtraInfo(treenode.Parent, strExtraInfo);
            }
            return 0;
        }

        /*
        // 2007/7/28
        // 设置当前树上已经选择的节点的扩展信息
        public int SetCurrentTargetExtraInfo(
            string strExtraInfo,
            out string strError)
        {
            strError = "";

            TreeNode treenode = this.SelectedNode;
            if (treenode == null)
            {
                strError = "尚未选择节点";
                return 0;
            }

            if (treenode.ImageIndex == TYPE_DIR)
            {
                strError = "暂不支持目标目录检索";
                return -1;
            }

            // 定位server节点的XML节点
            XmlNode xmlServerNode = null;

            if (IsServer(treenode) == true)
            {
                xmlServerNode = TreeNodeInfo.GetXmlNode(treenode);
                if (xmlServerNode == null)
                {
                    strError = "树节点 '" + treenode.Name + "' Tag或TreeNodeInfo.XmlNode为空";
                    return -1;
                }
            }

            if (IsDatabaseType(treenode) == true)
            {
                if (treenode.Parent == null)
                {
                    strError = "树节点 '" + treenode.Name + "' 没有父节点";
                    return -1;
                }

                xmlServerNode = TreeNodeInfo.GetXmlNode(treenode.Parent);
                if (xmlServerNode == null)
                {
                    strError = "树节点 '" + treenode.Parent.Name + "' Tag或TreeNodeInfo.XmlNode为空";
                    return -1;
                }

            }

            Debug.Assert(xmlServerNode != null, "");

            DomUtil.SetAttr(xmlServerNode, "extraInfo", strExtraInfo);

            return 0;
        }*/

        static List<string> GetCheckedDbNames(TreeNode node_server)
        {
            if (IsServerType(node_server) == false)
            {
                Debug.Assert(false, "");
                throw new Exception("必须是 Server 类型的节点");
            }

            Debug.Assert(node_server != null, "");

            List<string> dbname_list = new List<string>();
            for (int i = 0; i < node_server.Nodes.Count; i++)
            {
                TreeNode child = node_server.Nodes[i];
                if (child.Checked == true)
                {
                    TreeNodeInfo info = (TreeNodeInfo)child.Tag;
                    Debug.Assert(info != null, "");
                    dbname_list.Add(info.Name);
                }
            }
            return dbname_list;
        }

        public int GetTarget(
            TreeNode treenode,
            out TargetInfo targetinfo,
            out string strError)
        {
            strError = "";
            targetinfo = null;

            if (treenode == null)
            {
                treenode = this.SelectedNode;
                if (treenode == null)
                {
                    strError = "treenode参数为空时，当前目标树又尚未选择节点";
                    return 0;
                }
            }

            if (treenode.ImageIndex == TYPE_DIR)
            {
                strError = "暂不支持目标目录检索";
                return -1;
            }
            
            targetinfo = new TargetInfo();

            string[] dbnames = null;

            XmlNode xmlServerNode = null;

            // 如果是server类型节点，则选择下面的全部数据库名
            if (IsServerType(treenode) == true)
            {
                targetinfo.ServerNode = treenode;
                targetinfo.StartNode = treenode;    // 这是发起的节点

                xmlServerNode = TreeNodeInfo.GetXmlNode(treenode);
                if (xmlServerNode == null)
                {
                    strError = "树节点 '" + treenode.Text + "' Tag或TreeNodeInfo.XmlNode为空";
                    return -1;
                }

                /*
                if (treenode.Nodes.Count == 0)
                {
                    strError = "服务器节点 '" + treenode.Text + "' 下不包含任何数据库节点，无法进行检索";
                    return -1;
                }
                 * */

                List<string> dbname_list = null;
                if (this.CheckBoxes == true)
                    dbname_list = GetCheckedDbNames(treenode);

                // 如果checkboxes情况下没有勾选的数据库节点，则当作全部数据库节点都要包括进来的情况
                if (dbname_list == null
                    || dbname_list.Count == 0)
                {
                    dbname_list = new List<string>();
                    for (int i = 0; i < treenode.Nodes.Count; i++)
                    {
                        TreeNode child = treenode.Nodes[i];

                        if (child.ImageIndex == TYPE_DATABASE_NOTINALL)
                            continue;    // 跳过notInAll数据库节点 

                        TreeNodeInfo info = (TreeNodeInfo)child.Tag;
                        Debug.Assert(info != null, "");
                        // dbnames[i] = info.Name; //  child.Text;
                        dbname_list.Add(info.Name);
                    }
                    /*
                    if (dbname_list.Count == 0)
                    {
                        strError = "服务器节点 '" + treenode.Text + "' 下的 " + treenode.Nodes.Count.ToString() + "  个数据库节点全部为 '在全选时不参与检索' 属性，所以通过选定该服务器节点无法直接进行检索，只能通过选定其下的某个数据库节点进行检索";
                        return -1;
                    }
                     * */
                }

                dbnames = new string[dbname_list.Count];
                dbname_list.CopyTo(dbnames);
            }

            // 如果是database类型节点，则只选择一个数据库名
            if (IsDatabaseType(treenode) == true)
            {
                if (treenode.Parent == null)
                {
                    strError = "树节点 '" + treenode.Name + "' 没有父节点";
                    return -1;
                }

                // ZTargetInfo.ServerNode中存放的其实是server node
                targetinfo.ServerNode = treenode.Parent;
                targetinfo.StartNode = treenode;    // 这是发起的节点

                xmlServerNode = TreeNodeInfo.GetXmlNode(treenode.Parent);
                if (xmlServerNode == null)
                {
                    strError = "树节点 '" + treenode.Parent.Name + "' Tag或TreeNodeInfo.XmlNode为空";
                    return -1;
                }

                dbnames = new string[1];
                dbnames[0] = treenode.Text;
            }


            targetinfo.HostName = DomUtil.GetAttr(xmlServerNode,
                "addr");
            string strPort = DomUtil.GetAttr(xmlServerNode,
                "port");
            if (String.IsNullOrEmpty(strPort) == false)
                targetinfo.Port = Convert.ToInt32(strPort);

            targetinfo.DbNames = dbnames;
            targetinfo.UserName = DomUtil.GetAttr(xmlServerNode,
                "username");

            // password
            string strPassword = DomUtil.GetAttr(xmlServerNode,
            "password");
            targetinfo.Password = ZServerPropertyForm.GetPassword(
                    strPassword);

            targetinfo.GroupID = DomUtil.GetAttr(xmlServerNode,
                "groupid");
            string strAuthenticationMethod = DomUtil.GetAttr(xmlServerNode,
                "authmethod");
            if (String.IsNullOrEmpty(strAuthenticationMethod) == false)
                targetinfo.AuthenticationMethod = Convert.ToInt32(strAuthenticationMethod);

            targetinfo.ConvertEACC = ZServerPropertyForm.GetBool(
                DomUtil.GetAttr(xmlServerNode,
                "converteacc"));
            targetinfo.FirstFull = ZServerPropertyForm.GetBool(
                DomUtil.GetAttr(xmlServerNode,
                "firstfull"));
            targetinfo.DetectMarcSyntax = ZServerPropertyForm.GetBool(
                DomUtil.GetAttr(xmlServerNode,
                "detectmarcsyntax"));

            targetinfo.IgnoreReferenceID = ZServerPropertyForm.GetBool(
    DomUtil.GetAttr(xmlServerNode,
    "ignorereferenceid"));

            // 对ISBN的预处理
            targetinfo.IsbnForce13 = ZServerPropertyForm.GetBool(
DomUtil.GetAttr(xmlServerNode,
"isbn_force13"));
            targetinfo.IsbnForce10 = ZServerPropertyForm.GetBool(
DomUtil.GetAttr(xmlServerNode,
"isbn_force10"));
            targetinfo.IsbnAddHyphen = ZServerPropertyForm.GetBool(
DomUtil.GetAttr(xmlServerNode,
"isbn_addhyphen"));
            targetinfo.IsbnRemoveHyphen = ZServerPropertyForm.GetBool(
DomUtil.GetAttr(xmlServerNode,
"isbn_removehyphen"));
            targetinfo.IsbnWild = ZServerPropertyForm.GetBool(
DomUtil.GetAttr(xmlServerNode,
"isbn_wild"));

            targetinfo.IssnForce8 = ZServerPropertyForm.GetBool(
DomUtil.GetAttr(xmlServerNode,
"issn_force8"));

            string strPresentPerBatchCount = DomUtil.GetAttr(xmlServerNode,
                "recsperbatch");

            if (String.IsNullOrEmpty(strPresentPerBatchCount) == false)
                targetinfo.PresentPerBatchCount = Convert.ToInt32(strPresentPerBatchCount);

            // 缺省编码方式
            string strDefaultEncodingName = DomUtil.GetAttr(xmlServerNode,
                "defaultEncoding");

            if (String.IsNullOrEmpty(strDefaultEncodingName) == false)
            {
                try
                {
                    // 单独处理MARC-8 Encoding
                    if (strDefaultEncodingName.ToLower() == "eacc"
                        || strDefaultEncodingName.ToLower() == "marc-8")
                    {
                        if (this.Marc8Encoding == null)
                        {
                            strError = "尚未初始化this.EaccEncoding成员";
                            return -1;
                        }
                        targetinfo.DefaultRecordsEncoding = this.Marc8Encoding;
                    }
                    else 
                        targetinfo.DefaultRecordsEncoding = Encoding.GetEncoding(strDefaultEncodingName);
                }
                catch
                {
                    targetinfo.DefaultRecordsEncoding = Encoding.GetEncoding(936);
                }
            }

            // 检索词编码方式
            string strQueryTermEncodingName = DomUtil.GetAttr(xmlServerNode,
                "queryTermEncoding");

            if (String.IsNullOrEmpty(strQueryTermEncodingName) == false)
            {
                try
                {
                    targetinfo.DefaultQueryTermEncoding = Encoding.GetEncoding(strQueryTermEncodingName);
                }
                catch
                {
                    targetinfo.DefaultQueryTermEncoding = Encoding.GetEncoding(936);
                }
            }

            string strDefaultMarcSyntax = DomUtil.GetAttr(xmlServerNode,
                "defaultMarcSyntaxOID");
            // strDefaultMarcSyntax = strDefaultMarcSyntax;    // 可以有--部分

            if (String.IsNullOrEmpty(strDefaultMarcSyntax) == false)
                targetinfo.PreferredRecordSyntax = strDefaultMarcSyntax;

            //
            string strDefaultElementSetName = DomUtil.GetAttr(xmlServerNode,
                "defaultElementSetName");
            // strDefaultElementSetName = strDefaultElementSetName;    // 可以有--部分

            if (String.IsNullOrEmpty(strDefaultElementSetName) == false)
                targetinfo.DefaultElementSetName = strDefaultElementSetName;

            // 格式和编码之间的绑定信息
            string strBindingDef = DomUtil.GetAttr(xmlServerNode,
                "recordSyntaxAndEncodingBinding");
            targetinfo.Bindings = new RecordSyntaxAndEncodingBindingCollection();
            if (String.IsNullOrEmpty(strBindingDef) == false)
                targetinfo.Bindings.Load(strBindingDef);

            // charset nego
            targetinfo.CharNegoUTF8 = ZServerPropertyForm.GetBool(
                DomUtil.GetAttr(xmlServerNode,
                "charNegoUtf8"));
            targetinfo.CharNegoRecordsUTF8 = ZServerPropertyForm.GetBool(
                DomUtil.GetAttr(xmlServerNode,
                "charNego_recordsInSeletedCharsets"));

            targetinfo.UnionCatalogBindingDp2ServerName =
                DomUtil.GetAttr(xmlServerNode,
                "unionCatalog_bindingDp2ServerName");

            targetinfo.UnionCatalogBindingUcServerUrl =
    DomUtil.GetAttr(xmlServerNode,
    "unionCatalog_bindingUcServerUrl");


            return 0;
        }

        // 解析出 '-' 左边的值
        public static string GetLeftValue(string strText)
        {
            int nRet = strText.IndexOf("-");
            if (nRet != -1)
                return strText.Substring(0, nRet).Trim();
            else
                return strText.Trim();
        }

        private void ZTargetControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem menuItem = null;
            ToolStripMenuItem subMenuItem = null;
            // ToolStripSeparator menuSepItem = null;

            TreeNode node = this.SelectedNode;

            // 属性
            menuItem = new ToolStripMenuItem("属性(&P)");
            if (node == null
                || (node != null && IsDatabaseType(node) == true ))
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_property_Click);
            contextMenu.Items.Add(menuItem);


            /*
            // --
            menuItem = new ToolStripMenuItem("-");
            contextMenu.Items.Add(menuItem);

            // 切断连接
            menuItem = new ToolStripMenuItem("断开连接(&C)");
            if (node == null
                || (node != null && node.ImageIndex != TYPE_SERVER))
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_closeZAssociation_Click);
            contextMenu.Items.Add(menuItem);
             * */


            // 新增下级
            menuItem = new ToolStripMenuItem("新增下级(&C)");
            if (node != null && IsDatabaseType(node) == true)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // 子菜单
            subMenuItem = new ToolStripMenuItem();
            subMenuItem.Text = "目录(&D)";
            subMenuItem.Tag = "dir";
            if (IsServerType(node) == true)
                subMenuItem.Enabled = false;
            subMenuItem.Click += new EventHandler(MenuItem_newChild_Click);
            menuItem.DropDown.Items.Add(subMenuItem);

            subMenuItem = new ToolStripMenuItem();
            subMenuItem.Text = "服务器(&S)";
            subMenuItem.Tag = "server";
            if (IsServerType(node) == true)
                subMenuItem.Enabled = false;
            subMenuItem.Click += new EventHandler(MenuItem_newChild_Click);
            menuItem.DropDown.Items.Add(subMenuItem);


            // 新增同级
            menuItem = new ToolStripMenuItem("新增同级(&S)");
            if (node == null
                || (node != null && IsDatabaseType(node) == true))
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // 子菜单
            subMenuItem = new ToolStripMenuItem();
            subMenuItem.Text = "目录(&D)";
            subMenuItem.Tag = "dir";
            subMenuItem.Click += new EventHandler(MenuItem_newSibling_Click);
            menuItem.DropDown.Items.Add(subMenuItem);

            subMenuItem = new ToolStripMenuItem();
            subMenuItem.Text = "服务器(&S)";
            subMenuItem.Tag = "server";
            subMenuItem.Click += new EventHandler(MenuItem_newSibling_Click);
            menuItem.DropDown.Items.Add(subMenuItem);


            // 删除
            menuItem = new ToolStripMenuItem("删除(&R)");
            if (node == null)
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_delete_Click);
            contextMenu.Items.Add(menuItem);

            ToolStripSeparator sep = null;

            if (this.AllowCheckbox == true)
            {
                // ---
                sep = new ToolStripSeparator();
                contextMenu.Items.Add(sep);
                // 复选
                menuItem = new ToolStripMenuItem("复选(&H)");
                if (this.CheckBoxes == true)
                    menuItem.Checked = true;
                menuItem.Click += new EventHandler(menuItem_toggleCheckBoxes_Click);
                contextMenu.Items.Add(menuItem);
            }

            if (OnSetMenu != null)
            {
                GuiAppendMenuEventArgs newargs = new GuiAppendMenuEventArgs();
                newargs.ContextMenuStrip = contextMenu;
                OnSetMenu(this, newargs);
                if (newargs.ContextMenuStrip != contextMenu)
                    contextMenu = newargs.ContextMenuStrip;
            }		

            contextMenu.Show(this, e.Location);
        }

        void menuItem_toggleCheckBoxes_Click(object sender, EventArgs e)
        {
            if (this.CheckBoxes == true)
                this.CheckBoxes = false;
            else
                this.CheckBoxes = true;
        }
        

        private void ZTargetControl_MouseDown(object sender, MouseEventArgs e)
        {
            TreeNode curSelectedNode = this.GetNodeAt(e.X, e.Y);

            if (this.SelectedNode != curSelectedNode)
                this.SelectedNode = curSelectedNode;
        }

        void menuItem_delete_Click(object sender,
            EventArgs e)
        {
            TreeNode node = this.SelectedNode;
            if (node == null)
            {
                MessageBox.Show(this, "尚未选择节点");
                return;
            }

            DialogResult result = MessageBox.Show(this,
"确实要删除节点 '"+node.Text+"'? ",
"dp2Catalog",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            XmlNode xmlnode = TreeNodeInfo.GetXmlNode(node);

            TreeNodeCollection nodes = null;
            TreeNode parent = node.Parent;
            if (parent == null)
                nodes = this.Nodes;
            else
                nodes = parent.Nodes;

            nodes.Remove(node);

            xmlnode.ParentNode.RemoveChild(xmlnode);

            this.Changed = true;

            if (parent != null)
                this.SelectedNode = parent;
        }

        // 根据XML数据，刷新服务器对象下的数据库名
        void RefreshDatabaseNames(TreeNode server_node)
        {
            server_node.Nodes.Clear();

            XmlNode xmlnode = TreeNodeInfo.GetXmlNode(server_node);
            XmlNodeList nodelist = xmlnode.SelectNodes("database");
            for (int i = 0; i < nodelist.Count; i++)
            {
                XmlNode cur_node = nodelist[i];
                string strDatabaseName = DomUtil.GetAttr(cur_node, "name");

                int nImageIndex = TYPE_DATABASE;
                if (DomUtil.GetBooleanParam(cur_node,
                    "notInAll",
                    false) == true)
                    nImageIndex = TYPE_DATABASE_NOTINALL;

                TreeNode newtreenode = new TreeNode(strDatabaseName,
                    nImageIndex, nImageIndex);
                server_node.Nodes.Add(newtreenode);

                TreeNodeInfo info = new TreeNodeInfo(nodelist[i]);
                info.Name = newtreenode.Text;
                newtreenode.Tag = info;
            }
        }

        void MenuItem_newChild_Click(object sender,
            EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            string strType = (string)menu.Tag;

            TreeNodeCollection container = null;

            TreeNode parent_tree_node = this.SelectedNode;
            if (parent_tree_node == null)
            {
                // MessageBox.Show(this, "尚未选择节点");
                // return;
                // first level
                container = this.Nodes;
            }
            else
            {
                container = parent_tree_node.Nodes;
            }

            XmlNode parent_xml_node = null;

            if (parent_tree_node == null)
                parent_xml_node = this.dom.DocumentElement;
            else
                parent_xml_node = TreeNodeInfo.GetXmlNode(parent_tree_node);


            if (strType == "server")
            {
                if (IsServerType(parent_tree_node) == true)
                {
                    MessageBox.Show(this, "服务器下不能再增加服务器");
                    return;
                }


                TreeNode newnode = new TreeNode("",
                    TYPE_SERVER_OFFLINE, TYPE_SERVER_OFFLINE);

                container.Add(newnode);

                XmlNode newxmlnode = parent_xml_node.OwnerDocument.CreateElement("server");
                parent_xml_node.AppendChild(newxmlnode);
                DomUtil.SetAttr(newxmlnode, "recsperbatch", "10");

                TreeNodeInfo info = new TreeNodeInfo(newxmlnode);
                newnode.Tag = info; // 记忆

                ZServerPropertyForm dlg = new ZServerPropertyForm();
                GuiUtil.SetControlFont(dlg, this.Font);
                dlg.MainForm = this.MainForm;
                dlg.XmlNode = newxmlnode;
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);
                if (dlg.DialogResult == DialogResult.OK)
                {
                    this.Changed = true;
                    newnode.Text = dlg.ServerName;
                    info.Name = dlg.ServerName;
                    this.SelectedNode = newnode;

                    // 显示下面的数据库节点
                    RefreshDatabaseNames(newnode);
                    newnode.Expand();

                    if (this.OnServerChanged != null)
                    {
                        ServerChangedEventArgs e1 = new ServerChangedEventArgs();
                        e1.TreeNode = newnode;
                        this.OnServerChanged(this, e1);
                    }
                }
                else
                {
                    container.Remove(newnode);
                    parent_xml_node.RemoveChild(newxmlnode);
                }

            }

            if (strType == "dir")
            {
                if (IsServerType(parent_tree_node) == true)
                {
                    MessageBox.Show(this, "服务器下不能再增加目录");
                    return;
                }

                TreeNode newnode = new TreeNode("",
                    TYPE_DIR, TYPE_DIR);
                container.Add(newnode);

                XmlNode newxmlnode = parent_xml_node.OwnerDocument.CreateElement(
                    "dir");
                parent_xml_node.AppendChild(newxmlnode);

                TreeNodeInfo info = new TreeNodeInfo(newxmlnode);
                newnode.Tag = info; // 记忆

                ZDirPopertyForm dlg = new ZDirPopertyForm();
                GuiUtil.SetControlFont(dlg, this.Font);
                dlg.XmlNode = newxmlnode;
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);
                if (dlg.DialogResult == DialogResult.OK)
                {
                    this.Changed = true;
                    newnode.Text = dlg.DirName;
                    info.Name = dlg.DirName;
                    this.SelectedNode = newnode;
                }
                else
                {
                    container.Remove(newnode);
                    parent_xml_node.RemoveChild(newxmlnode);
                }

            }
        }

        public static void SetNodeResultCount(TreeNode node,
            int nResultCount)
        {
            TreeNodeInfo info = (TreeNodeInfo)node.Tag;

            info.ResultCount = nResultCount;

            // 刷新名字显示
            RefreshNodeNameDisplay(node);
        }

        // 刷新名字显示
        public static void RefreshNodeNameDisplay(TreeNode node)
        {
            TreeNodeInfo info = (TreeNodeInfo)node.Tag;
            if (info.ResultCount == -3)
                node.Text = info.Name;
            else if (info.ResultCount == -2)
                node.Text = info.Name + "(?)";
            else if (info.ResultCount == -1)
                node.Text = info.Name + "(出错...)";
            else
                node.Text = info.Name + " (" + info.ResultCount.ToString() + ")";

            node.TreeView.Update();
        }

        // 新增同级
        void MenuItem_newSibling_Click(object sender,
            EventArgs e)
        {
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;
            string strType = (string)menu.Tag;

            TreeNode node = this.SelectedNode;
            if (node == null)
            {
                MessageBox.Show(this, "尚未选择节点");
                return;
            }

            TreeNodeCollection nodes = null;

            TreeNode parent = node.Parent;
            if (parent == null)
                nodes = this.Nodes;
            else
                nodes = parent.Nodes;

            node = null;    // 防止后面继续用

            if (strType == "server")
            {
                //
                XmlNode xmlnode = null;
                if (parent != null)
                    xmlnode = TreeNodeInfo.GetXmlNode(parent);
                else
                    xmlnode = this.dom.DocumentElement;

                TreeNode newnode = new TreeNode("", 
                    TYPE_SERVER_OFFLINE, TYPE_SERVER_OFFLINE);
                //
                nodes.Add(newnode);

                XmlNode newxmlnode = xmlnode.OwnerDocument.CreateElement("server");
                xmlnode.AppendChild(newxmlnode);
                DomUtil.SetAttr(newxmlnode, "recsperbatch", "10");

                TreeNodeInfo info = new TreeNodeInfo(newxmlnode);
                newnode.Tag = info;

                ZServerPropertyForm dlg = new ZServerPropertyForm();
                GuiUtil.SetControlFont(dlg, this.Font);
                dlg.MainForm = this.MainForm;
                dlg.XmlNode = newxmlnode;
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);
                if (dlg.DialogResult == DialogResult.OK)
                {
                    this.Changed = true;
                    newnode.Text = dlg.ServerName;
                    info.Name = dlg.ServerName;
                    this.SelectedNode = newnode;

                    // 显示下面的数据库节点
                    RefreshDatabaseNames(newnode);
                    newnode.Expand();

                    if (this.OnServerChanged != null)
                    {
                        ServerChangedEventArgs e1 = new ServerChangedEventArgs();
                        e1.TreeNode = newnode;
                        this.OnServerChanged(this, e1);
                    }
                }
                else
                {
                    //
                    nodes.Remove(newnode);
                    xmlnode.RemoveChild(newxmlnode);
                }

            }

            if (strType == "dir")
            {
                //
                XmlNode xmlnode = null;
                if (parent != null)
                    xmlnode = TreeNodeInfo.GetXmlNode(parent);
                else
                    xmlnode = this.dom.DocumentElement;

                TreeNode newnode = new TreeNode("",
                    TYPE_DIR, TYPE_DIR);

                //
                nodes.Add(newnode);

                XmlNode newxmlnode = xmlnode.OwnerDocument.CreateElement(
                    "dir");
                xmlnode.AppendChild(newxmlnode);

                TreeNodeInfo info = new TreeNodeInfo(newxmlnode);
                newnode.Tag = info;

                ZDirPopertyForm dlg = new ZDirPopertyForm();
                GuiUtil.SetControlFont(dlg, this.Font);
                dlg.XmlNode = newxmlnode;
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);
                if (dlg.DialogResult == DialogResult.OK)
                {
                    this.Changed = true;
                    newnode.Text = dlg.DirName;
                    info.Name = dlg.DirName;
                    this.SelectedNode = newnode;
                }
                else
                {
                    nodes.Remove(newnode);
                    xmlnode.RemoveChild(newxmlnode);
                }

            }
        }


        void menuItem_property_Click(object sender,
            EventArgs e)
        {
            TreeNode node = this.SelectedNode;
            if (node == null)
            {
                MessageBox.Show(this, "尚未选择节点");
                return;
            }

            if (IsServerType(node) == true)
            {
                ZServerPropertyForm dlg = new ZServerPropertyForm();
                GuiUtil.SetControlFont(dlg, this.Font);
                dlg.MainForm = this.MainForm;
                dlg.XmlNode = TreeNodeInfo.GetXmlNode(node);
                dlg.InitialResultInfo = TreeNodeInfo.GetExtraInfo(node);    // 2007/7/28
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);
                if (dlg.DialogResult == DialogResult.OK)
                {
                    this.Changed = true;
                    node.Text = dlg.ServerName;

                    TreeNodeInfo info = (TreeNodeInfo)node.Tag;
                    info.Name = dlg.ServerName;

                    // 显示下面的数据库节点
                    RefreshDatabaseNames(node);
                    node.Expand();

                    if (this.OnServerChanged != null)
                    {
                        ServerChangedEventArgs e1 = new ServerChangedEventArgs();
                        e1.TreeNode = node;
                        this.OnServerChanged(this, e1);
                    }

                    if (node.ImageIndex == TYPE_SERVER_ONLINE)
                        MessageBox.Show(this, "注意：(当前Z39.50服务器处于已联机状态。) 对Z39.50服务器属性参数的修改，要在下一次连接中才能生效。\r\n\r\n为使参数立即生效，可断开连接，然后重新进行检索操作。");

                }
            }

            if (node.ImageIndex == TYPE_DIR)
            {
                ZDirPopertyForm dlg = new ZDirPopertyForm();
                GuiUtil.SetControlFont(dlg, this.Font);
                dlg.XmlNode = TreeNodeInfo.GetXmlNode(node);
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);
                if (dlg.DialogResult == DialogResult.OK)
                {
                    this.Changed = true;
                    node.Text = dlg.DirName;

                    TreeNodeInfo info = (TreeNodeInfo)node.Tag;
                    info.Name = dlg.DirName;

                }
                
            }
        }

        static void SetNodeColor(TreeNode node)
        {
            if (node.Checked == true)
            {
                node.ForeColor = SystemColors.InfoText;
                node.BackColor = SystemColors.Info;
            }
            else
            {
                node.ForeColor = SystemColors.WindowText;
                node.BackColor = SystemColors.Window;
            }
        }

        // 计算checked的服务器节点数量
        public int GetCheckedServerCount(TreeNode start_node = null)
        {
            Debug.Assert(this.CheckBoxes == true, "只有在 CheckBoxes == true 情况下调用本函数才有意义");

            TreeNodeCollection nodes = null;

            if (start_node == null)
                nodes = this.Nodes;
            else
                nodes = start_node.Nodes;

            int nCount = 0;
            foreach (TreeNode node in nodes)
            {
                if (node.Checked == true
                    && IsServerType(node) == true)
                    nCount++;

                // 递归
                if (IsDirType(node) == true)
                    nCount += GetCheckedServerCount(node);
            }

            return nCount;
        }

        int m_nInInitial = 0;   // checked是否在初始化状态

        private void ZTargetControl_AfterCheck(object sender, TreeViewEventArgs e)
        {
            // 初始化状态下不要进行连带选定
            if (this.m_nInInitial > 0)
                return;

            TreeNode node = e.Node;
            if (node == null)
                return;

            SetNodeColor(node);

            if (node.Checked == false)
            {
                ClearOneLevelChildrenCheck(node);

                // 如果兄弟都为Unckecked，则Uncheck parent node
                IfClearParentNodeCheck(node);
            }
            else
            {
                // 如果下级为一个都没有Checked，则全部Check它们，并展开本node
                IfCheckChildNodes(node);

                // 只要本级有一个checked，则check parent
                if (node.Parent != null)
                    node.Parent.Checked = true;
            }

            // 注：事件自己会递归
        }

        // 清除下级所有的选中的项(不包括自己)
        public void ClearOneLevelChildrenCheck(TreeNode nodeStart)
        {
            if (nodeStart == null)
                return;
            foreach (TreeNode node in nodeStart.Nodes)
            {
                node.Checked = false;
                // ClearChildrenCheck(node);	// 暂时不递归
            }
        }

        // 如果全部兄弟节点都是Unchecked，则Unchecked parent node
        public void IfClearParentNodeCheck(TreeNode node)
        {
            if (node.Parent == null)
                return;

            foreach (TreeNode current in node.Parent.Nodes)
            {
                if (current.Checked == true)
                    return;
            }

            if (node.Parent.Checked == true)
                node.Parent.Checked = false;
        }

        // 如果下级为一个都没有Checked，则全部Check它们，并展开本node
        public void IfCheckChildNodes(TreeNode node)
        {
            if (node.Nodes.Count == 0)
                return;

            foreach (TreeNode current in node.Nodes)
            {
                if (current.Checked == true)
                    return;
            }

            node.Expand();

            foreach (TreeNode current in node.Nodes)
            {
                if (IsDatabaseType(current) == true
                    && current.ImageIndex == TYPE_DATABASE_NOTINALL)
                    continue;
                current.Checked = true;
            }
        }

    }

    // 2007/7/28
    // TreeNode的Tag里面存储的信息结构
    public class TreeNodeInfo
    {
        public XmlNode XmlNode = null;
        public string ExtraInfo = "";
        public string Name = "";    // 显示出来的服务器名部分
        public int ResultCount = -3;    // 显示出来的检索命中数部分 -3表示未曾检索过(不显示数字); -2表示正在检索; -1表示检索出错; >=0表示命中数量

        public TreeNodeInfo(XmlNode xmlnode,
            string strExtraInfo)
        {
            this.XmlNode = xmlnode;
            this.ExtraInfo = strExtraInfo;
        }

        public TreeNodeInfo(XmlNode xmlnode)
        {
            this.XmlNode = xmlnode;
            this.ExtraInfo = "";
        }

        public static XmlNode GetXmlNode(TreeNode treenode)
        {
            TreeNodeInfo info = (TreeNodeInfo)treenode.Tag;
            if (info == null)
                return null;
            return info.XmlNode;
        }

        public static string GetExtraInfo(TreeNode treenode)
        {
            TreeNodeInfo info = (TreeNodeInfo)treenode.Tag;
            if (info == null)
                return null;
            return info.ExtraInfo;
        }

        public static void SetExtraInfo(TreeNode treenode,
            string strExtraInfo)
        {
            TreeNodeInfo info = (TreeNodeInfo)treenode.Tag;
            if (info == null)
                treenode.Tag = new TreeNodeInfo(null, strExtraInfo);
            else
                info.ExtraInfo = strExtraInfo;
        }

    }

    // 检索目标信息结构
    public class TargetInfo
    {
        public TreeNode ServerNode = null;  // 相关的server类型节点
        public TreeNode StartNode = null;   // 发起检索的节点。可以不是server类型节点

        public string HostName = "";
        public int Port = 210;

        public string[] DbNames = null;


        public string UserName = "";
		public string Password = "";
		public string GroupID = "";
		public int AuthenticationMethod = 0;

        public string PreferredRecordSyntax = BerTree.MARC_SYNTAX;  // 可以有--部分。使用时候小心，用GetLeftValue()获得干净的值
        public string DefaultResultSetName = "default";

        public string DefaultElementSetName = "F -- Full"; // 可以有--部分。使用时候小心，用GetLeftValue()获得干净的值

        public int PresentPerBatchCount = 10;   // 每批数量

        public bool ConvertEACC = true;
        public bool FirstFull = true;
        public bool DetectMarcSyntax = true;
        public bool IgnoreReferenceID = false;
        public bool IsbnForce13 = false;
        public bool IsbnForce10 = false;
        public bool IsbnAddHyphen = false;
        public bool IsbnRemoveHyphen = false;
        public bool IsbnWild = false;

        public bool IssnForce8 { get; set; }

        public Encoding DefaultRecordsEncoding = Encoding.GetEncoding(936);
        public Encoding DefaultQueryTermEncoding = Encoding.GetEncoding(936);

        public RecordSyntaxAndEncodingBindingCollection Bindings = null;

        public bool CharNegoUTF8 = true;
        public bool CharNegoRecordsUTF8 = true;

        public string UnionCatalogBindingDp2ServerName = "";
        public string UnionCatalogBindingUcServerUrl = "";

        bool m_bChanged = false;

        // 树上显示的名字
        public string Name
        {
            get
            {
                // 2007/8/3
                if (this.StartNode != null)
                {
                    // Debug.Assert(this.ServerNode == this.StartNode.Parent, ""); // 2007/11/2 BUG

                    if (ZTargetControl.IsDatabaseType(this.StartNode) == true)
                        return this.StartNode.Text + "." + this.StartNode.Parent.Text;
                }

                if (this.ServerNode == null)
                    return "";

                return this.ServerNode.Text;
            }
        }

        // 服务器名。树节点上显示的名字，不包含括号部分
        public string ServerName
        {
            get
            {
                if (this.ServerNode == null)
                    return "";
                TreeNodeInfo info = (TreeNodeInfo)this.ServerNode.Tag;
                if (info == null)
                    return "";
                return info.Name;
            }
        }

        public string HostNameAndPort
        {
            get
            {
                return this.HostName + ":" + this.Port.ToString();
            }
        }

        public void OnlineServerIcon(bool bOnline)
        {
            int nImageIndex = ZTargetControl.TYPE_SERVER_ONLINE;

            if (bOnline == false)
                nImageIndex = ZTargetControl.TYPE_SERVER_OFFLINE;

            if (this.ServerNode != null)
            {
                if (this.ServerNode.TreeView.InvokeRequired == true)
                {
                    ZTargetControl.Delegate_SetNodeImageIndex d = new ZTargetControl.Delegate_SetNodeImageIndex(ZTargetControl.SetNodeImageIndex);
                    this.ServerNode.TreeView.Invoke(d, new object[] { this.ServerNode, nImageIndex });
                }
                else
                {
                    this.ServerNode.ImageIndex = nImageIndex;
                    this.ServerNode.SelectedImageIndex = nImageIndex;
                }
            }
        }

#if NOOOOOOOOOOOO
        public void OfflineServerIcon()
        {
            if (this.ServerNode != null)
            {
                /*
                this.ServerNode.ImageIndex = ZTargetControl.TYPE_SERVER_OFFLINE;
                this.ServerNode.SelectedImageIndex = ZTargetControl.TYPE_SERVER_OFFLINE;
                 * */
                ZTargetControl.OnlineServerNodeIcon(this.ServerNode, false);
            }
        }
#endif 

        // 内容是否发生过修改
        public bool Changed
        {
            get
            {
                return this.m_bChanged;
            }
            set
            {
                this.m_bChanged = value;
            }
        }

        // 用于判断对象唯一性的名字
        public string QualifiedName
        {
            get
            {
                string strDbNameList = "";
                if (this.DbNames != null)
                {
                    strDbNameList = string.Join(",", this.DbNames);
                }
 
                string strTreePath = "";

                if (this.StartNode != null)
                    strTreePath = this.StartNode.FullPath;
                else if (this.ServerNode != null)
                    strTreePath = this.ServerNode.FullPath;

                return this.HostName + ":" + this.Port.ToString() + ";treepath=" + strTreePath + ";dbnames=" + strDbNameList;
            }
        }
    }

    // 绑定信息元素
    public class RecordSyntaxAndEncodingBindingItem
    {
        public string RecordSyntaxOID = "";
        public string RecordSyntaxComment = "";
        public string EncodingName = "";
        public string EncodingNameComment = "";

        // 将 "value -- comment" 形态的字符串拆分为"value"和"comment"两个部分
        public static void ParseValueAndComment(string strText,
            out string strValue,
            out string strComment)
        {
            int nRet = strText.IndexOf("--");
            if (nRet == -1)
            {
                strValue = strText.Trim();
                strComment = "";
                return;
            }

            strValue = strText.Substring(0, nRet).Trim();
            strComment = strText.Substring(nRet + 2).Trim();
        }

        public string RecordSyntax
        {
            get
            {
                if (String.IsNullOrEmpty(this.RecordSyntaxComment) == true)
                    return this.RecordSyntaxOID;

                return this.RecordSyntaxOID + " -- " + this.RecordSyntaxComment;
            }
            set
            {
                string strValue = "";
                string strComment = "";

                ParseValueAndComment(value, out strValue, out strComment);
                this.RecordSyntaxOID = strValue;
                this.RecordSyntaxComment = strComment;
            }
        }

        public string Encoding
        {
            get
            {
                if (String.IsNullOrEmpty(this.EncodingNameComment) == true)
                    return this.EncodingName;
                return this.EncodingName + " -- " + this.EncodingNameComment;
            }
            set
            {
                string strValue = "";
                string strComment = "";

                ParseValueAndComment(value, out strValue, out strComment);
                this.EncodingName = strValue;
                this.EncodingNameComment = strComment;
            }
        }
    }

    // 绑定信息数组
    public class RecordSyntaxAndEncodingBindingCollection : List<RecordSyntaxAndEncodingBindingItem>
    {
        // parameters:
        //      strBindingString    格式为"syntaxoid1 -- syntaxcomment1|encodingname1 -- encodingcomment1||syntaxoid2 -- syntaxcomment2|encodingname2 -- encodingcomment2"，末尾可能有多余的“||”
        public void Load(string strBindingString)
        {
            this.Clear();

            string[] lines = strBindingString.Split(new string[] { "||" },
                StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < lines.Length; i++)
            {
                string strSyntax = "";
                string strEncoding = "";
                string strLine = lines[i].Trim();
                if (String.IsNullOrEmpty(strLine) == true)
                    continue;
                int nRet = strLine.IndexOf('|');
                if (nRet != -1)
                {
                    strSyntax = strLine.Substring(0, nRet).Trim();
                    strEncoding = strLine.Substring(nRet + 1).Trim();
                }
                else
                {
                    strSyntax = strLine;
                    strEncoding = "";
                }

                RecordSyntaxAndEncodingBindingItem item = new RecordSyntaxAndEncodingBindingItem();
                item.RecordSyntax = strSyntax;
                item.Encoding = strEncoding;

                this.Add(item);
            }
        }

        // 返还为字符串形态
        public string GetString()
        {
            string strResult = "";
            for (int i = 0; i < this.Count; i++)
            {
                RecordSyntaxAndEncodingBindingItem item = this[i];
                strResult += item.RecordSyntax + "|" + item.Encoding + "||";
            }

            return strResult;
        }

        public string GetEncodingName(string strRecordSyntaxOID)
        {
            for (int i = 0; i < this.Count; i++)
            {
                RecordSyntaxAndEncodingBindingItem item = this[i];

                if (item.RecordSyntaxOID == strRecordSyntaxOID)
                    return item.EncodingName;
            }

            return null;    // not found
        }

    }

    // 服务器发生改变
    public delegate void ServerChangedEventHandle(object sender,
        ServerChangedEventArgs e);

    public class ServerChangedEventArgs : EventArgs
    {
        public TreeNode TreeNode = null;
    }
}
