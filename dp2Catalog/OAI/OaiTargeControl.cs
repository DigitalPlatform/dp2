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

namespace dp2Catalog
{
    public partial class OaiTargeControl : TreeView
    {
        XmlDocument dom = null;

        bool m_bChanged = false;

        string m_strFileName = "";

        public const int TYPE_DIR = 0;
        public const int TYPE_SERVER_OFFLINE = 1;
        public const int TYPE_DATABASE = 2; // 暂不用
        public const int TYPE_SERVER_ONLINE = 3;

        public event GuiAppendMenuEventHandle OnSetMenu;
        public event ServerChangedEventHandle OnServerChanged;


        public OaiTargeControl()
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
                strError = "装载文件 '" + strFileName + "' 到XMLDOM时出错: " + ex.Message;
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

        private void OaiTargeControl_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem menuItem = null;
            // ToolStripMenuItem subMenuItem = null;

            TreeNode node = this.SelectedNode;

            // 属性
            menuItem = new ToolStripMenuItem("属性(&P)");
            if (node == null
                || (node != null && node.ImageIndex == TYPE_DATABASE))
                menuItem.Enabled = false;
            menuItem.Click += new EventHandler(menuItem_property_Click);
            contextMenu.Items.Add(menuItem);


            /*
            // 新增下级
            menuItem = new ToolStripMenuItem("新增下级(&C)");
            if (node != null && node.ImageIndex == TYPE_DATABASE)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // 子菜单
            subMenuItem = new ToolStripMenuItem();
            subMenuItem.Text = "目录(&D)";
            subMenuItem.Tag = "dir";
            if (IsServer(node) == true)
                subMenuItem.Enabled = false;
            subMenuItem.Click += new EventHandler(MenuItem_newChild_Click);
            menuItem.DropDown.Items.Add(subMenuItem);

            subMenuItem = new ToolStripMenuItem();
            subMenuItem.Text = "服务器(&S)";
            subMenuItem.Tag = "server";
            if (IsServer(node) == true)
                subMenuItem.Enabled = false;
            subMenuItem.Click += new EventHandler(MenuItem_newChild_Click);
            menuItem.DropDown.Items.Add(subMenuItem);


            // 新增同级
            menuItem = new ToolStripMenuItem("新增同级(&S)");
            if (node == null
                || (node != null && node.ImageIndex == TYPE_DATABASE))
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
             * */


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

        public static bool IsServer(int nType)
        {
            if (nType == TYPE_SERVER_OFFLINE
                || nType == TYPE_SERVER_ONLINE)
                return true;

            return false;
        }

        public static bool IsServer(TreeNode node)
        {
            if (node == null)
                return false;
            int nType = node.ImageIndex;
            return IsServer(nType);
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

            if (IsServer(node) == true)
            {
                OaiServerPropertyForm dlg = new OaiServerPropertyForm();
                GuiUtil.SetControlFont(dlg, this.Font);
                dlg.XmlNode = TreeNodeInfo.GetXmlNode(node);
                dlg.StartPosition = FormStartPosition.CenterScreen;
                dlg.ShowDialog(this);
                if (dlg.DialogResult == DialogResult.OK)
                {
                    this.Changed = true;
                    node.Text = dlg.ServerName;

                    TreeNodeInfo info = (TreeNodeInfo)node.Tag;
                    info.Name = dlg.ServerName;

                    /*
                    // 显示下面的数据库节点
                    RefreshDatabaseNames(node);
                    node.Expand();
                     * */

                    if (this.OnServerChanged != null)
                    {
                        ServerChangedEventArgs e1 = new ServerChangedEventArgs();
                        e1.TreeNode = node;
                        this.OnServerChanged(this, e1);
                    }

                    if (node.ImageIndex == TYPE_SERVER_ONLINE)
                        MessageBox.Show(this, "注意：(当前OAI服务器处于已联机状态。) 对OAI服务器属性参数的修改，要在下一次连接中才能生效。\r\n\r\n为使参数立即生效，可断开连接，然后重新进行检索操作。");

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

    }
}
