using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;

using DigitalPlatform.GUI;
using DigitalPlatform.rms.Client.rmsws_localhost;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;


namespace DigitalPlatform.rms.Client
{
    /// <summary>
    /// Summary description for ResRightTree.
    /// </summary>
    public class ResRightTree : System.Windows.Forms.TreeView
    {
        public ServerCollection Servers = null;	// 引用
        public RmsChannelCollection Channels = null;

        bool m_bChanged = false;

        public DigitalPlatform.StopManager stopManager = null;

        RmsChannel channel = null;

        public string ServerUrl = "";

        public string Lang = "zh";

        public XmlDocument UserRightsDom = null;	// 用户帐户记录

        public string PropertyCfgFileName = "";

        TreeNode m_oldHoverNode = null;

        public int[] EnabledIndices = null;

        public event GuiAppendMenuEventHandle OnSetMenu;

        public event NodeRightsChangedEventHandle OnNodeRightsChanged;

        private System.Windows.Forms.ImageList imageList_resIcon;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.ComponentModel.IContainer components;

        public ResRightTree()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();

            this.ImageList = imageList_resIcon;

        }

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }

                if (this.Channels != null)
                    this.Channels.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ResRightTree));
            this.imageList_resIcon = new System.Windows.Forms.ImageList(this.components);
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            // 
            // imageList_resIcon
            // 
            this.imageList_resIcon.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList_resIcon.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_resIcon.ImageStream")));
            this.imageList_resIcon.TransparentColor = System.Drawing.Color.Fuchsia;
            // 
            // ResRightTree
            // 
            this.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.ResRightTree_AfterExpand);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ResRightTree_MouseUp);
            this.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.ResRightTree_AfterSelect);
            this.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ResRightTree_MouseMove);

        }
        #endregion

        public bool Changed
        {
            get
            {
                return m_bChanged;
            }
            set
            {
                m_bChanged = value;
            }
        }

        // 初始化
        // parameters:
        //      userRightsDom   用户记录的dom对象。将直接引用这个对象
        public void Initial(ServerCollection servers,
            RmsChannelCollection channels,
            DigitalPlatform.StopManager stopManager,
            string serverUrl,
            XmlDocument UserRightsDom)
        {
            this.Servers = servers;
            this.Channels = channels;
            this.stopManager = stopManager;
            this.ServerUrl = serverUrl;

            this.UserRightsDom = UserRightsDom; // 直接引用外界的dom对象

            // 用服务器端获得的信息填充树
            Cursor save = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            FillAll(null);
            InitialRightsParam();
            this.Cursor = save;

            this.m_bChanged = false;
        }

        string GetDefElementString(int nType)
        {
            if (nType == ResTree.RESTYPE_DB)
                return "database";
            if (nType == ResTree.RESTYPE_FILE)
                return "file";
            if (nType == ResTree.RESTYPE_FOLDER)
                return "dir";
            if (nType == ResTree.RESTYPE_FROM)
                return "from";
            if (nType == ResTree.RESTYPE_SERVER)
                return "server";

            return "object";
        }


        // 递归
        public int Fill(TreeNode node)
        {
            TreeNodeCollection children = null;

            if (node == null)
            {
                children = this.Nodes;
            }
            else
            {
                children = node.Nodes;
            }

            int i;


            // 填充根
            if (node == null)
            {
                children.Clear();

                TreeNode nodeNew = new TreeNode(this.ServerUrl, ResTree.RESTYPE_SERVER, ResTree.RESTYPE_SERVER);
                ResTree.SetLoading(nodeNew);

                NodeInfo nodeinfo = new NodeInfo();
                nodeinfo.TreeNode = nodeNew;
                nodeinfo.Expandable = true;
                nodeinfo.DefElement = GetDefElementString(nodeNew.ImageIndex);
                nodeinfo.NodeState |= NodeState.Object;

                nodeNew.Tag = nodeinfo;


                if (EnabledIndices != null
                    && StringUtil.IsInList(nodeNew.ImageIndex, EnabledIndices) == false)
                    nodeNew.ForeColor = ControlPaint.LightLight(nodeNew.ForeColor);

                children.Add(nodeNew);
                return 0;
            }


            // 根以下的节点类型
            ResPath respath = new ResPath(node);

            string strPath = respath.Path;

            //if (node != null)
            //	strPath = TreeViewUtil.GetPath(node);

            this.channel = Channels.GetChannel(this.ServerUrl);

            Debug.Assert(channel != null, "Channels.GetChannel() 异常");

            ResInfoItem[] items = null;

            string strError = "";

            DigitalPlatform.Stop stop = null;

            if (stopManager != null)
            {
                stop = new DigitalPlatform.Stop();

                stop.Register(this.stopManager, true);	// 和容器关联

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在列目录: " + this.ServerUrl + "?" + strPath);
                stop.BeginLoop();

            }

            long lRet = channel.DoDir(strPath,
                this.Lang,
                null,   // 不需要列出全部语言的名字
                out items,
                out strError);

            if (stopManager != null)
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                stop.Unregister();	// 和容器关联
            }

            this.channel = null;

            if (lRet == -1)
            {
                try
                {
                    MessageBox.Show(this, "Channel::DoDir() Error: " + strError);
                }
                catch
                {
                    // this可能已经不存在
                    return -1;
                }

                if (node != null)
                {
                    ResTree.SetLoading(node);	// 出错的善后处理，重新出现+号
                    node.Collapse();
                }
                return -1;
            }


            if (items != null)
            {
                children.Clear();

                for (i = 0; i < items.Length; i++)
                {
                    // 忽略from类型节点
                    if (items[i].Type == ResTree.RESTYPE_FROM)
                        continue;

                    TreeNode nodeNew = new TreeNode(items[i].Name, items[i].Type, items[i].Type);


                    NodeInfo nodeinfo = new NodeInfo();
                    nodeinfo.TreeNode = nodeNew;
                    nodeinfo.Expandable = items[i].HasChildren;
                    nodeinfo.DefElement = GetDefElementString(nodeNew.ImageIndex);
                    nodeinfo.NodeState |= NodeState.Object;
                    nodeinfo.Style = items[i].Style;
                    nodeNew.Tag = nodeinfo;

                    if (items[i].HasChildren)
                        ResTree.SetLoading(nodeNew);

                    if (EnabledIndices != null
                        && StringUtil.IsInList(nodeNew.ImageIndex, EnabledIndices) == false)
                        nodeNew.ForeColor = ControlPaint.LightLight(nodeNew.ForeColor);

                    children.Add(nodeNew);
                }
            }

            return 0;
        }

        /*
        // 在一个节点下级插入"loading..."，以便出现+号
        static void SetLoading(TreeNode node)
        {
            // 新node
            TreeNode nodeNew = new TreeNode("loading...", ResTree.RESTYPE_LOADING, ResTree.RESTYPE_LOADING);

            node.Nodes.Clear();
            node.Nodes.Add(nodeNew);
        }

        // 下级是否包含loading...?
        static bool IsLoading(TreeNode node)
        {
            if (node.Nodes.Count == 0)
                return false;

            if (node.Nodes[0].Text == "loading...")
                return true;

            return false;
        }
        */

        // 回调函数
        void DoStop(object sender, StopEventArgs e)
        {
            if (this.channel != null)
                this.channel.Abort();
        }

        private void ResRightTree_AfterExpand(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            TreeNode node = e.Node;

            if (node == null)
                return;

            // 需要展开
            if (ResTree.IsLoading(node) == true)
            {
                Fill(node);
            }
        }

        private void ResRightTree_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            TreeNode node = e.Node;

            if (node == null)
                return;

            // 需要展开
            if (ResTree.IsLoading(node) == true)
            {
                Fill(node);
            }

        }

        void FillAll(TreeNode node)
        {
            Fill(node);

            TreeNodeCollection children = null;

            if (node == null)
            {
                children = this.Nodes;
            }
            else
            {
                node.Expand();
                children = node.Nodes;
            }

            for (int i = 0; i < children.Count; i++)
            {
                TreeNode child = children[i];
                // 需要展开
                if (ResTree.IsLoading(child) == true)
                {
                    FillAll(child);
                }
            }
        }

        // 根据帐户记录中的信息, 填充全部对象的权限信息
        int InitialRightsParam()
        {
            // 得到<server>节点

            XmlNode nodeRoot = this.UserRightsDom.SelectSingleNode("//server");   // rightsItem

            if (nodeRoot == null)
                return 0;	// 容器节点没有找到

            // 按照自然结构进行初始化
            InitialRights(nodeRoot,
                this.Nodes[0]);


            return 0;
        }


        // 初始化: 把xml中的信息填入treeview
        int InitialRights(XmlNode parentXmlNode,
            TreeNode parentTreeNode)
        {
            // 服务器节点特殊
            if (parentTreeNode.ImageIndex == ResTree.RESTYPE_SERVER)
            {
                string strName = DomUtil.GetAttr(parentXmlNode, "name");

                NodeInfo nodeinfo = null;
                nodeinfo = (NodeInfo)parentTreeNode.Tag;
                if (nodeinfo == null)
                {
                    nodeinfo = new NodeInfo();
                    nodeinfo.TreeNode = parentTreeNode;
                    parentTreeNode.Tag = nodeinfo;
                }
                nodeinfo.NodeState |= NodeState.Account | NodeState.Object;

                nodeinfo.DefElement = parentXmlNode.Name;
                nodeinfo.DefName = strName;
                nodeinfo.Rights = DomUtil.GetAttrDiff(parentXmlNode, "rights");

            }



            for (int i = 0; i < parentXmlNode.ChildNodes.Count; i++)
            {
                XmlNode childXmlNode = parentXmlNode.ChildNodes[i];
                if (childXmlNode.NodeType != XmlNodeType.Element)
                    continue;

                string strName = DomUtil.GetAttr(childXmlNode, "name");

                int nType = 0;
                bool bExpandable = false;
                // 数据库
                if (childXmlNode.Name == "database")
                {
                    nType = ResTree.RESTYPE_DB;
                    bExpandable = true;
                }
                // 目录
                if (childXmlNode.Name == "dir")
                {
                    nType = ResTree.RESTYPE_FOLDER;
                    bExpandable = true;
                }
                // 文件
                if (childXmlNode.Name == "file")
                {
                    nType = ResTree.RESTYPE_FILE;
                    bExpandable = true;
                }

                TreeNode childTreeNode = FindTreeNode(parentTreeNode, strName);

                NodeInfo nodeinfo = null;

                // 没有找到
                if (childTreeNode == null)
                {
                    // 新创建一个,但是标注为未使用的
                    childTreeNode = new TreeNode(strName, nType, nType);

                    nodeinfo = new NodeInfo();
                    nodeinfo.TreeNode = childTreeNode;
                    nodeinfo.Expandable = bExpandable;
                    nodeinfo.NodeState |= NodeState.Account;
                    childTreeNode.Tag = nodeinfo;

                    childTreeNode.ForeColor = ControlPaint.LightLight(childTreeNode.ForeColor);	// 灰色

                    parentTreeNode.Nodes.Add(childTreeNode);
                }
                else // 找到
                {
                    nodeinfo = (NodeInfo)childTreeNode.Tag;
                    if (nodeinfo == null)
                    {
                        nodeinfo = new NodeInfo();
                        nodeinfo.TreeNode = childTreeNode;
                        childTreeNode.Tag = nodeinfo;
                    }
                    nodeinfo.NodeState |= NodeState.Account | NodeState.Object;
                }


                nodeinfo.DefElement = childXmlNode.Name;
                nodeinfo.DefName = strName;
                nodeinfo.Rights = DomUtil.GetAttrDiff(childXmlNode, "rights");

                if (nodeinfo.Rights == "" || nodeinfo.Rights == null)
                    childTreeNode.ForeColor = SystemColors.GrayText;	// ControlPaint.LightLight(nodeNew.ForeColor);
                else
                    childTreeNode.ForeColor = SystemColors.WindowText;


                // 递归
                InitialRights(childXmlNode,
                    childTreeNode);

            }

            return 0;
        }

        TreeNode FindTreeNode(TreeNode parent,
            string strName)
        {
            for (int i = 0; i < parent.Nodes.Count; i++)
            {
                TreeNode node = parent.Nodes[i];
                if (node.Text == strName)
                    return node;
            }

            return null;
        }

        // 找到儿子节点中包含指定name属性值的
        XmlNode FindXmlNode(XmlNode parent,
            string strName)
        {
            for (int i = 0; i < parent.ChildNodes.Count; i++)
            {
                XmlNode node = parent.ChildNodes[i];
                if (node.NodeType != XmlNodeType.Element)
                    continue;
                if (DomUtil.GetAttr(node, "name") == strName)
                    return node;
            }

            return null;
        }


        // 把treeview中的信息收集增补到xml中
        public int FinishRightsParam()
        {
            // 得到<server>节点

            XmlNode nodeRoot = this.UserRightsDom.SelectSingleNode("//server");   // rightsItem

            if (nodeRoot == null)
            {
                // 容器节点没有找到
                DomUtil.SetElementText(this.UserRightsDom.DocumentElement, "server", "");   // rightsItem
                nodeRoot = this.UserRightsDom.SelectSingleNode("//server"); // rightsItems
                Debug.Assert(nodeRoot != null, "新增了为何找不到呢?");

            }

            // 按照自然结构进行初始化
            FinishRights(nodeRoot,
                this.Nodes[0]);


            return 0;
        }

        // 保存: 把treeview中的信息收集增补到xml中
        int FinishRights(XmlNode parentXmlNode,
            TreeNode parentTreeNode)
        {
            ArrayList aFound = new ArrayList();

            // 根要特殊处理
            if (parentTreeNode.ImageIndex == ResTree.RESTYPE_SERVER)
            {
                NodeInfo nodeinfo = (NodeInfo)parentTreeNode.Tag;

                // 设置权限属性
                DomUtil.SetAttr(parentXmlNode, "rights", nodeinfo.Rights);
            }

            for (int i = 0; i < parentTreeNode.Nodes.Count; i++)
            {
                TreeNode childTreeNode = parentTreeNode.Nodes[i];

                string strName = childTreeNode.Text;

                NodeInfo nodeinfo = (NodeInfo)childTreeNode.Tag;

                // 找到儿子节点中包含指定name属性值的
                XmlNode childXmlNode = FindXmlNode(parentXmlNode,
                    strName);

                if (childXmlNode == null)
                {

                    Debug.Assert(nodeinfo.DefElement != "", "nodeinfo中DefElement尚未设置");

                    childXmlNode = parentXmlNode.OwnerDocument.CreateElement(nodeinfo.DefElement);
                    childXmlNode = parentXmlNode.AppendChild(childXmlNode);
                    DomUtil.SetAttr(childXmlNode, "name", strName);
                }
                else
                {
                    // 找到
                }

                aFound.Add(childXmlNode);


                // 设置权限属性
                DomUtil.SetAttr(childXmlNode, "rights", nodeinfo.Rights);

                // 递归
                FinishRights(childXmlNode,
                    childTreeNode);

            }

            // 比treenode多出来的,要删除
            for (int i = 0; i < parentXmlNode.ChildNodes.Count; i++)
            {

                XmlNode node = parentXmlNode.ChildNodes[i];
                if (node.NodeType != XmlNodeType.Element)
                    continue;

                if (aFound.IndexOf(node) == -1)
                {
                    node.ParentNode.RemoveChild(node);
                    i--;
                }
            }

            return 0;
        }

        private void ResRightTree_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            menuItem = new MenuItem("权限(&R)");
            menuItem.Click += new System.EventHandler(this.menu_editRights_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("删除(&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteNode_Click);
            if (this.SelectedNode != null && this.SelectedNode.ImageIndex == ResTree.RESTYPE_SERVER)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            if (OnSetMenu != null)
            {
                GuiAppendMenuEventArgs newargs = new GuiAppendMenuEventArgs();
                newargs.ContextMenu = contextMenu;
                OnSetMenu(this, newargs);
                if (newargs.ContextMenu != contextMenu)
                    contextMenu = newargs.ContextMenu;
            }

            if (contextMenu != null)
                contextMenu.Show(this, new Point(e.X, e.Y));

        }

        // 编辑权限
        // return:
        //      false   没有发生修改
        //      true    发生了修改
        public DialogResult NodeRightsDlg(TreeNode node,
            out string strRights)
        {
            strRights = "";

            DigitalPlatform.CommonDialog.CategoryPropertyDlg dlg = new DigitalPlatform.CommonDialog.CategoryPropertyDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

            if (node == null)
                node = this.SelectedNode;

            /*
			NodeInfo nodeinfo = (NodeInfo)this.SelectedNode.Tag;

			dlg.StartPosition = FormStartPosition.CenterScreen;
			dlg.Text = "对象 '"+ this.SelectedNode.Text +"' 的权限";
			dlg.PropertyString = nodeinfo.Rights;
			dlg.CfgFileName = this.PropertyCfgFileName;
			dlg.ShowDialog(this);

			if (dlg.DialogResult != DialogResult.OK)
				return;


			nodeinfo.Rights = dlg.PropertyString;

			if (nodeinfo.Rights == "")
				this.SelectedNode.ForeColor = SystemColors.GrayText;	// ControlPaint.LightLight(nodeNew.ForeColor);
			else
				this.SelectedNode.ForeColor = SystemColors.WindowText;

			this.m_bChanged = true;
             */

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.Text = "对象 '" + node.Text + "' 的权限";
            dlg.PropertyString = GetNodeRights(node);
            dlg.CfgFileName = this.PropertyCfgFileName;
            dlg.ShowDialog(this);

            strRights = dlg.PropertyString;

            return dlg.DialogResult;
        }

        // 编辑权限
        private void menu_editRights_Click(object sender, System.EventArgs e)
        {
            if (this.SelectedNode == null)
            {
                MessageBox.Show("尚未选择要编辑的事项...");
                return;
            }

            string strRights = "";

            if (NodeRightsDlg(this.SelectedNode,
                out strRights) != DialogResult.OK)
                return;

            SetNodeRights(this.SelectedNode, strRights);
        }

        // 获得一个节点所包含的权限字符串
        public static string GetNodeRights(TreeNode node)
        {
            NodeInfo nodeinfo = (NodeInfo)node.Tag;

            if (nodeinfo == null)
                return null;

            return nodeinfo.Rights;
        }

        public static int GetNodeStyle(TreeNode node)
        {
            NodeInfo nodeinfo = (NodeInfo)node.Tag;

            if (nodeinfo == null)
                return 0;

            return nodeinfo.Style;
        }

        // 获得一个节点是否可以展开的状态
        public static bool GetNodeExpandable(TreeNode node)
        {
            NodeInfo nodeinfo = (NodeInfo)node.Tag;

            if (nodeinfo == null)
                return false;

            return nodeinfo.Expandable;
        }

        // 设置一个节点所包含的权限字符串
        public void SetNodeRights(TreeNode node,
            string strRights)
        {
            NodeInfo nodeinfo = (NodeInfo)node.Tag;

            if (nodeinfo.Rights == strRights)
                return;

            nodeinfo.Rights = strRights;

            if (nodeinfo.Rights == "" || nodeinfo.Rights == null)
                node.ForeColor = SystemColors.GrayText;	// ControlPaint.LightLight(nodeNew.ForeColor);
            else
                node.ForeColor = SystemColors.WindowText;

            this.m_bChanged = true;

            if (OnNodeRightsChanged != null)
            {
                NodeRightsChangedEventArgs e = new NodeRightsChangedEventArgs();
                e.Node = node;
                e.Rights = strRights;
                OnNodeRightsChanged(this, e);
            }
        }

        // 删除节点
        private void menu_deleteNode_Click(object sender, System.EventArgs e)
        {
            if (this.SelectedNode == null)
            {
                MessageBox.Show("尚未选择要删除的节点...");
                return;
            }

            if (this.SelectedNode.ImageIndex == ResTree.RESTYPE_SERVER)
            {
                MessageBox.Show("不能删除服务器节点...");
                return;
            }



            DialogResult result = MessageBox.Show(this,
                "确实要删除节点 " + this.SelectedNode.Text + "?",
                "ResRightTree",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
                return;

            this.SelectedNode.Remove();

            this.m_bChanged = true;
        }

        private void ResRightTree_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            string strText = "";

            // Point p = this.PointToClient(new Point(e.X, e.Y));
            //TreeNode selection = this.GetNodeAt(p);
            TreeNode selection = this.GetNodeAt(e.X, e.Y);

            if (m_oldHoverNode == selection)
                return;


            if (selection != null)
            {
                selection.BackColor = SystemColors.Info;
                NodeInfo nodeinfo = (NodeInfo)selection.Tag;
                if (nodeinfo != null)
                {
                    string strState = "";
                    strState = NodeInfo.GetNodeStateString(nodeinfo);

                    if (nodeinfo.Rights == null)
                        strText = "对象 '" + selection.Text + "' 权限 -- (未定义);  状态--" + strState;
                    else if (nodeinfo.Rights == "")
                        strText = "对象 '" + selection.Text + "' 权限 -- (空);  状态--" + strState;
                    else
                        strText = "对象 '" + selection.Text + "' 权限 -- " + nodeinfo.Rights + ";  状态 -- " + strState;
                }

            }
            toolTip1.SetToolTip(this, strText);

            if (m_oldHoverNode != selection)
            {
                if (m_oldHoverNode != null)
                    m_oldHoverNode.BackColor = SystemColors.Window;

                m_oldHoverNode = selection;
            }
        }

    }

    public enum NodeState
    {
        None = 0,
        Object = 0x01,	// 来自实际对象
        Account = 0x02,	// 来自帐户记录定义

    }

    // 节点详细信息
    public class NodeInfo
    {
        public bool Expandable = false;	// 是否可以展开下级对象
        public string Rights = "";	// 权限字符串

        public string DefElement = "";	// 定义用的元素名
        public string DefName = "";	// 定义元素中的name属性值

        public TreeNode TreeNode = null;

        public NodeState NodeState = NodeState.None;

        public int Style = 0;

        public static string GetNodeStateString(NodeInfo nodeinfo)
        {
            string strState = "";
            if ((nodeinfo.NodeState & NodeState.Account) == NodeState.Account)
                strState = "帐户定义";
            if ((nodeinfo.NodeState & NodeState.Object) == NodeState.Object)
            {
                if (strState != "")
                    strState += ",";
                strState = "对象";
            }

            return strState;
        }

    }

    public delegate void NodeRightsChangedEventHandle(object sender,
    NodeRightsChangedEventArgs e);

    public class NodeRightsChangedEventArgs : EventArgs
    {
        public TreeNode Node = null;
        public string Rights = "";
    }

}
