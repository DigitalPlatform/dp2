using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.IO;

using System.Diagnostics;

using DigitalPlatform.GUI;
using DigitalPlatform.rms.Client.rmsws_localhost;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;


namespace DigitalPlatform.rms.Client
{
    /*
    // 对象存储模式
    public enum StorageMode
    {
        None = 0,	// 尚未决定
        Real = 1,	// 真实对象
        Memory = 2,	// 内存对象
    }
    */

    /// <summary>
    /// 数据库下属对象管理 的 树 控件
    /// </summary>
    public class DatabaseObjectTree : System.Windows.Forms.TreeView
    {
        public bool EnableDefaultFileEditing = false;

        public int DbStyle = 0;   // 数据库Style
        public ApplicationInfo applicationInfo = null;

        public ObjEventCollection Log = new ObjEventCollection();

        // public StorageMode StorageMode = StorageMode.None;	// 存储模式暂时没有定

        public DigitalPlatform.StopManager stopManager = null;

        RmsChannel channel = null;

        public string Lang = "zh";

        public ServerCollection Servers = null;	// 引用
        public RmsChannelCollection Channels = null;

        public string ServerUrl = "";
        // public string DbName = "";
        string m_strDbName = "";

        public bool DisplayRoot = true;

        //
        public DatabaseObject Root = new DatabaseObject();	// 内存对象树的根对象。虚根。

        public event GuiAppendMenuEventHandle OnSetMenu;

        public event OnObjectDeletedEventHandle OnObjectDeleted;

        private System.Windows.Forms.ImageList imageList_resIcon;
        private System.ComponentModel.IContainer components;

        public DatabaseObjectTree()
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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(DatabaseObjectTree));
            this.imageList_resIcon = new System.Windows.Forms.ImageList(this.components);
            // 
            // imageList_resIcon
            // 
            this.imageList_resIcon.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList_resIcon.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_resIcon.ImageStream")));
            this.imageList_resIcon.TransparentColor = System.Drawing.Color.Fuchsia;
            // 
            // DatabaseObjectTree
            // 
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.DatabaseObjectTree_MouseUp);

        }
        #endregion

        public string DbName
        {
            get
            {
                return m_strDbName;
            }
            set
            {
                m_strDbName = value;
                if (this.DisplayRoot == true)
                {
                    if (this.Nodes.Count != 0)
                    {
                        this.Nodes[0].Text = value;
                        this.Root.Name = value; // 内存对象名字也要改变

                    }
                }
            }
        }

        public void Initial(ServerCollection servers,
            RmsChannelCollection channels,
            DigitalPlatform.StopManager stopManager,
            string serverUrl,
            string strDbName)
        {
            this.Servers = servers;
            this.Channels = channels;
            this.stopManager = stopManager;
            this.ServerUrl = serverUrl;
            this.DbName = strDbName;

            if (this.DbName != "")
            {

                // 获得数据库Style
                string strError = "";
                this.DbStyle = this.GetDbStyle(
                    this.DbName,
                    out strError);
                if (this.DbStyle == -1)
                    throw new Exception(strError);

                // 用服务器端获得的信息填充树
                Cursor save = this.Cursor;
                this.Cursor = Cursors.WaitCursor;
                FillAll(null);
                this.Cursor = save;
            }

        }

        public int CreateMemoryObject(out string strError)
        {
            strError = "";

            if (this.DbName == "" || this.DbName == "?")  // 2006/1/20
                return 0;

            DatabaseObject root = new DatabaseObject();
            root.Type = ResTree.RESTYPE_DB;
            root.Name = this.DbName;
            root.Style = this.DbStyle;
            int nRet = CreateObject(root,
                this.DbName,
                out strError);
            if (nRet == -1)
                return -1;

            this.Root = root;
            return 0;
        }

        // 填充全部节点
        public void FillAll(TreeNode node)
        {
            string strError = "";


            int nRet = CreateMemoryObject(out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

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

        string GetPath(TreeNode treenode)
        {
            if (this.DisplayRoot == false)
            {
                if (treenode == null)
                    return this.DbName;
                return this.DbName + "/" + TreeViewUtil.GetPath(treenode, '/');
            }
            else
            {
                Debug.Assert(treenode != null, "在显示根模式下, 不能用null调用本函数");
                return TreeViewUtil.GetPath(treenode, '/');
            }

        }

        // 根据TreeNode节点获得Style
        public int GetNodeStyle(TreeNode node)
        {
            if (this.Root == null)
                return 0;

            string strPath = "";
            if (node != null)
                strPath = TreeViewUtil.GetPath(node, '/');

            DatabaseObject obj = this.Root.LocateObject(strPath);
            if (obj == null)
                return 0;

            return obj.Style;
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

                if (this.DisplayRoot == true)
                {
                    if (true)
                    {
                        TreeNode nodeNew = new TreeNode(this.Root.Name,
                            this.Root.Type,
                            this.Root.Type);
                        ResTree.SetLoading(nodeNew);
                        children.Add(nodeNew);
                    }
                    return 0;
                }
            }


            string strPath = "";



            children.Clear();

            if (node != null)
                strPath = TreeViewUtil.GetPath(node, '/');
            else
            {
                strPath = "";
            }

            DatabaseObject parent = this.Root.LocateObject(strPath);
            if (parent == null)
            {
                Debug.Assert(false, "path not found");
                return -1;	// 路径没有找到
            }

            for (i = 0; i < parent.Children.Count; i++)
            {
                DatabaseObject child = (DatabaseObject)parent.Children[i];

                // 忽略from类型节点
                if (child.Type == ResTree.RESTYPE_FROM)
                    continue;

                TreeNode nodeNew = new TreeNode(child.Name, child.Type, child.Type);


                Debug.Assert(child.Type != -1, "类型值尚未初始化");

                if (child.Type == ResTree.RESTYPE_FOLDER)
                    ResTree.SetLoading(nodeNew);

                children.Add(nodeNew);
            }

            return 0;

        }

        // 将内存对象插入TreeView树
        // (暂不改变服务器端的真正对象)
        // parameters:
        //		nodeInsertPos	插入参考节点，在此前插入。如果==null，表示在parent下级末尾追加
        public int InsertObject(TreeNode nodeParent,
            TreeNode nodeInsertPos,
            DatabaseObject obj,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            TreeNodeCollection children = null;

            if (nodeParent == null)
            {
                children = this.Nodes;
            }
            else
            {

                children = nodeParent.Nodes;
            }

            ArrayList aObject = new ArrayList();
            if (obj.Type == -1 || obj.Type == ResTree.RESTYPE_DB)
            {
                aObject.AddRange(obj.Children);
            }
            else
            {
                aObject.Add(obj);
            }

            for (int i = 0; i < aObject.Count; i++)
            {
                DatabaseObject perObj = (DatabaseObject)aObject[i];

                TreeNode nodeNew = new TreeNode(perObj.Name,
                    perObj.Type, perObj.Type);

                Debug.Assert(perObj.Type != -1, "类型值尚未初始化");
                Debug.Assert(perObj.Type != ResTree.RESTYPE_DB, "插入类型不能为DB");

                if (nodeInsertPos == null)
                    children.Add(nodeNew);
                else
                {
                    int index = children.IndexOf(nodeInsertPos);
                    if (index == -1)
                        children.Add(nodeNew);
                    else
                        children.Insert(index, nodeNew);
                }

                string strPath = this.GetPath(nodeNew);

                // 把修改记录到日志
                ObjEvent objevent = new ObjEvent();
                objevent.Obj = perObj;
                objevent.Oper = ObjEventOper.New;
                objevent.Path = strPath;
                this.Log.Add(objevent);
                /*
                // 在服务器端兑现
                if (this.StorageMode == StorageMode.Real)
                {
                    MemoryStream stream = null;
					
                    if (perObj.Type == ResTree.RESTYPE_FILE)
                        stream = new MemoryStream(perObj.Content);

                    string strPath = this.GetPath(nodeNew);
                    nRet = NewServerSideObject(strPath,
                        nodeNew.ImageIndex,
                        stream,
                        perObj.TimeStamp,
                        out strError);
                    if (nRet == -1)
                        return -1;
                }
                */


                if (perObj.Type == ResTree.RESTYPE_FOLDER)
                {
                    // 递归
                    // ResTree.SetLoading(nodeNew);
                    for (int j = 0; j < perObj.Children.Count; j++)
                    {

                        nRet = InsertObject(nodeNew,
                            null,
                            (DatabaseObject)perObj.Children[j],
                            out strError);
                        if (nRet == -1)
                            return -1;
                    }

                }

                if (nodeNew.Parent != null)
                    nodeNew.Parent.Expand();

            }

            return 0;
        }

        // 回调函数
        void DoStop(object sender, StopEventArgs e)
        {
            if (this.channel != null)
                this.channel.Abort();
        }

        private void DatabaseObjectTree_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            int nImageIndex = -1;
            if (this.SelectedNode != null)
                nImageIndex = this.SelectedNode.ImageIndex;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;


            menuItem = new MenuItem("编辑配置文件(&E)");
            menuItem.Click += new System.EventHandler(this.menu_editCfgFile);
            if (nImageIndex != ResTree.RESTYPE_FILE)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("新对象 [同级](&S)");
            menuItem.Click += new System.EventHandler(this.menu_newObjectSibling_Click);
            if (nImageIndex == ResTree.RESTYPE_DB || this.Nodes.Count == 0)
                menuItem.Enabled = false;
            else
                menuItem.Enabled = true;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("新对象 [下级](&H)");
            menuItem.Click += new System.EventHandler(this.menu_newObjectChild_Click);
            if (nImageIndex == ResTree.RESTYPE_FOLDER || nImageIndex == ResTree.RESTYPE_DB)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            bool bHasClipboardObject = false;
            IDataObject iData = Clipboard.GetDataObject();
            if (iData == null
                || iData.GetDataPresent(typeof(DatabaseObject)) == false)
                bHasClipboardObject = false;
            else
                bHasClipboardObject = true;


            menuItem = new MenuItem("复制树(&T)");
            menuItem.Click += new System.EventHandler(this.menu_copyTreeToClipboard_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("粘贴树(&A)");
            menuItem.Click += new System.EventHandler(this.menu_pasteTreeFromClipboard_Click);
            if (bHasClipboardObject == false)
            {
                menuItem.Enabled = false;
            }
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("复制(&C)");
            menuItem.Click += new System.EventHandler(this.menu_copyObjectToClipboard_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("粘贴[前插](&P)");
            menuItem.Click += new System.EventHandler(this.menu_pasteObjectFromClipboard_InsertBefore_Click);
            if (bHasClipboardObject == false)
            {
                menuItem.Enabled = false;
            }
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("粘贴[追加到下级末尾](&S)");
            menuItem.Click += new System.EventHandler(this.menu_pasteObjectFromClipboard_AppendChild_Click);
            if (bHasClipboardObject == false)
            {
                menuItem.Enabled = false;
            }
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("删除对象(&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteObject_Click);
            if (this.SelectedNode == null || nImageIndex == ResTree.RESTYPE_DB)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("模式(&M)");
            menuItem.Click += new System.EventHandler(this.menu_displayMode_Click);
            contextMenu.MenuItems.Add(menuItem);

            ////

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


        // 创建新对象
        void DoNewObject(bool bInsertAsChild)
        {
            NewObjectDlg dlg = new NewObjectDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

            dlg.Type = ResTree.RESTYPE_FILE;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            string strPath = "";
            string strError = "";

            DatabaseObject obj = new DatabaseObject();
            obj.Name = dlg.textBox_objectName.Text;
            obj.Type = dlg.Type;
            obj.Changed = true;

            TreeNode node = new TreeNode(obj.Name, obj.Type, obj.Type);

            TreeNode nodeParent = null;

            if (bInsertAsChild == false)
            {
                // 插入同级


                // 查重
                TreeNode nodeDup = TreeViewUtil.FindNodeByText((TreeView)this,
                    this.SelectedNode != null ? this.SelectedNode.Parent : null,
                    dlg.textBox_objectName.Text);
                if (nodeDup != null)
                {
                    strError = "同名对象已经存在。放弃操作。";
                    goto ERROR1;
                }

                if (this.SelectedNode == null)
                {
                    strError = "尚未选择基准对象";
                    goto ERROR1;
                }

                nodeParent = this.SelectedNode.Parent;
                if (nodeParent == null)
                    nodeParent = this.Nodes[0];

            }
            else
            {
                // 插入下级

                // 查重
                TreeNode nodeDup = TreeViewUtil.FindNodeByText((TreeView)this,
                    this.SelectedNode,
                    dlg.textBox_objectName.Text);
                if (nodeDup != null)
                {
                    strError = "同名对象已经存在。放弃操作。";
                    goto ERROR1;
                }

                nodeParent = this.SelectedNode;
                if (nodeParent == null)
                    nodeParent = this.Nodes[0];

            }

            nodeParent.Nodes.Add(node);

            strPath = TreeViewUtil.GetPath(nodeParent, '/');
            DatabaseObject objParent = this.Root.LocateObject(strPath);
            if (objParent == null)
            {
                strError = "路径为 '" + strPath + "' 的内存对象没有找到...";
                goto ERROR1;
            }

            obj.Parent = objParent;
            objParent.Children.Add(obj);

            strPath = TreeViewUtil.GetPath(node, '/');

            // 把修改记录到日志
            ObjEvent objevent = new ObjEvent();
            objevent.Obj = obj;
            objevent.Oper = ObjEventOper.New;
            objevent.Path = strPath;
            this.Log.Add(objevent);

            /*
            int nRet = NewServerSideObject(strPath,
                dlg.Type,
                null,
                null,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // 刷新?
            FillAll(null);
            */

            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        // 在服务器端创建对象
        // return:
        //		-1	错误
        //		1	以及存在同名对象
        //		0	正常返回
        int NewServerSideObject(string strPath,
            int nType,
            Stream stream,
            byte[] baTimeStamp,
            out byte[] baOutputTimestamp,
            out string strError)
        {
            strError = "";
            baOutputTimestamp = null;

            Debug.Assert(this.Channels != null, "");

            this.channel = Channels.GetChannel(this.ServerUrl);

            Debug.Assert(channel != null, "Channels.GetChannel() 异常");

            DigitalPlatform.Stop stop = null;

            if (stopManager != null)
            {
                stop = new DigitalPlatform.Stop();

                stop.Register(this.stopManager, true);	// 和容器关联

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在创建新对象: " + this.ServerUrl + "?" + strPath);

                stop.BeginLoop();

            }

            string strOutputPath = "";
            string strStyle = "";

            if (nType == ResTree.RESTYPE_FOLDER)
                strStyle = "createdir";
            /*
            long lRet = channel.DoSaveTextRes(strPath,
                "",	// content 暂时为空
                true,	// bInlucdePreamble
                strStyle,	// style
                null,	// baTimeStamp,
                out baOutputTimestamp,
                out strOutputPath,
                out strError);
            */

            string strRange = "";
            if (stream != null && stream.Length != 0)
            {
                Debug.Assert(stream.Length != 0, "test");
                strRange = "0-" + Convert.ToString(stream.Length - 1);
            }
            long lRet = channel.DoSaveResObject(strPath,
                stream,
                (stream != null && stream.Length != 0) ? stream.Length : 0,
                strStyle,
                "",	// strMetadata,
                strRange,
                true,
                baTimeStamp,	// timestamp,
                out baOutputTimestamp,
                out strOutputPath,
                out strError);

            if (stopManager != null)
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                stop.Unregister();	// 和容器关联
            }

            if (lRet == -1)
            {
                if (this.channel.ErrorCode == ChannelErrorCode.AlreadyExist)
                {
                    this.channel = null;
                    return 1;	// 已经存在同名同类型对象
                }
                this.channel = null;
                strError = "写入 '" + strPath + "' 发生错误: " + strError;
                return -1;
            }

            this.channel = null;
            return 0;
        }

        // 编辑配置文件
        void menu_editCfgFile(object sender, System.EventArgs e)
        {
            if (this.SelectedNode == null)
            {
                MessageBox.Show(this, "尚未选择要编辑的配置文件节点");
                return;
            }

            if (this.SelectedNode.ImageIndex != ResTree.RESTYPE_FILE)
            {
                MessageBox.Show(this, "所选择的节点不是配置文件类型。请选择要编辑的配置文件节点。");
                return;
            }

            string strPath = this.GetPath(this.SelectedNode);

            if (DatabaseObject.IsDefaultFile(strPath) == true
                && EnableDefaultFileEditing == false)
            {
                MessageBox.Show(this, "数据库缺省的配置文件不能在此进行修改。");
                return;
            }

            DatabaseObject obj = this.Root.LocateObject(strPath);
            if (obj == null)
            {
                MessageBox.Show(this, "路径为 '" + strPath + "' 的内存对象没有找到...");
                return;
            }

            // 编辑配置文件
            CfgFileEditDlg dlg = new CfgFileEditDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

            dlg.Initial(obj, strPath);
            /*
            dlg.Initial(this.Servers,
                this.Channels,
                this.stopManager,
                this.ServerUrl,
                strPath);
            */

            if (this.applicationInfo != null)
                this.applicationInfo.LinkFormState(dlg, "CfgFileEditDlg_state");
            dlg.ShowDialog(this);
            if (this.applicationInfo != null)
                this.applicationInfo.UnlinkFormState(dlg);

            if (dlg.DialogResult == DialogResult.OK)
            {
                // 把修改记录到日志
                ObjEvent objevent = new ObjEvent();
                objevent.Obj = obj;
                objevent.Oper = ObjEventOper.Change;
                objevent.Path = strPath;
                this.Log.Add(objevent);
            }
        }

        // 显示模式
        private void menu_displayMode_Click(object sender, System.EventArgs e)
        {
            /*
            string strMode = "";
            if (this.StorageMode == StorageMode.Real)
            {
                strMode = "真实";
            }
            if (this.StorageMode == StorageMode.Memory)
            {
                strMode = "内存对象";
            }

            string strText = "当前对象存储模式为 '" + strMode + "' , 数据库名为 '" + this.DbName + "'。";
            MessageBox.Show(this, strText);
            */

        }


        // 复制当前选择的对象到剪贴板
        private void menu_copyObjectToClipboard_Click(object sender, System.EventArgs e)
        {
            DatabaseObject root = null;
            string strError = "";
            // int nRet = 0;
            /*
            if (this.StorageMode == StorageMode.Real)
            {
                nRet = BuildMemoryTree(
                    this.SelectedNode,
                    out root,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }
            }
            */
            if (true)
            {
                string strPath = "";
                if (this.SelectedNode != null)
                    strPath = TreeViewUtil.GetPath(this.SelectedNode, '/');

                DatabaseObject parent = this.Root.LocateObject(strPath);
                if (parent == null)
                {
                    strError = "路径 '" + strPath + "'没有找到对应的内存对象 ...";
                    MessageBox.Show(this, strError);
                    return;
                }

                root = parent.Clone();

            }

            Clipboard.SetDataObject(root);

            if (Control.ModifierKeys == Keys.Control)
                MessageBox.Show(this, root.Dump());
        }

        // 从剪贴板粘贴一个对象(前插)
        private void menu_pasteObjectFromClipboard_InsertBefore_Click(object sender, System.EventArgs e)
        {
            DoPasteObject(true);
        }

        // 从剪贴板粘贴一个对象(追加到下级末尾)
        private void menu_pasteObjectFromClipboard_AppendChild_Click(object sender, System.EventArgs e)
        {
            DoPasteObject(false);
        }

        // parameters:
        //		bInsert	是否前插 true:前插 false:追加到下级末尾
        void DoPasteObject(bool bInsert)
        {
            IDataObject iData = Clipboard.GetDataObject();
            if (iData == null
                || iData.GetDataPresent(typeof(DatabaseObject)) == false)
            {
                MessageBox.Show(this, "剪贴板中尚不存在DatabaseObject类型数据");
                return;
            }

            DatabaseObject root = (DatabaseObject)iData.GetData(typeof(DatabaseObject));

            if (root == null)
            {
                MessageBox.Show(this, "GetData error");
                return;
            }

            /*
            public void InsertObject(TreeNode nodeParent,
                TreeNode nodeInsertPos,
                DatabaseObject obj)
            */

            string strError = "";
            int nRet = 0;
            if (this.SelectedNode == null)
            {
                TreeNode nodeInsertPos = null;

                if (this.Nodes.Count != 0)
                    nodeInsertPos = this.Nodes[0];

                // 插入到第一级的最前面
                nRet = InsertObject(null,
                    nodeInsertPos,
                    root,
                    out strError);

            }
            else
            {
                if (bInsert == true)
                {
                    // 插入当前节点的前面
                    nRet = InsertObject(this.SelectedNode.Parent,
                        this.SelectedNode,
                        root,
                        out strError);
                }
                else
                {
                    // 插入当前节点的下级末尾
                    nRet = InsertObject(this.SelectedNode,
                        null,
                        root,
                        out strError);
                }

            }

            if (nRet == -1)
                MessageBox.Show(this, strError);
        }

        // 从剪贴板粘贴整个树
        private void menu_pasteTreeFromClipboard_Click(object sender, System.EventArgs e)
        {
            IDataObject iData = Clipboard.GetDataObject();
            if (iData == null
                || iData.GetDataPresent(typeof(DatabaseObject)) == false)
            {
                MessageBox.Show(this, "剪贴板中尚不存在DatabaseObject类型数据");
                return;
            }

            DatabaseObject root = (DatabaseObject)iData.GetData(typeof(DatabaseObject));

            if (root == null)
            {
                MessageBox.Show(this, "GetData error");
                return;
            }

            this.Nodes.Clear();

            // 把以前树上的全部节点归纳为删除操作记入日志

            PutAllObjToLog(ObjEventOper.Delete,
                this.Root);

            //StorageMode oldmode = this.StorageMode;
            //this.StorageMode = StorageMode.Memory;

            //DatabaseObject oldroot = this.Root;

            this.Root = root;
            this.FillAll(null);

            //this.Root = oldroot;
            //this.StorageMode = oldmode;

            // 把现在树上全部节点作为新增记入日志
            PutAllObjToLog(ObjEventOper.New,
                this.Root);
        }

        public void SetRootObject(DatabaseObject root)
        {
            this.Nodes.Clear();

            // 把以前树上的全部节点归纳为删除操作记入日志

            PutAllObjToLog(ObjEventOper.Delete,
                this.Root);


            this.Root = root;
            this.FillAll(null);


            // 把现在树上全部节点作为新增记入日志
            PutAllObjToLog(ObjEventOper.New,
                this.Root);
        }



        // 将树整体复制到clipboard
        private void menu_copyTreeToClipboard_Click(object sender, System.EventArgs e)
        {
            /*
            DatabaseObject root = null;
            string strError = "";
            int nRet = BuildMemoryTree(
                (TreeNode)null,
                out root,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }
            */
            DatabaseObject root = this.Root.Clone();

            Clipboard.SetDataObject(root);
        }

        /*
        // 切换为内存对象模式
        public int SwitchToMemoryMode(out string strError)
        {
            Debug.Assert(false, "废止");
            strError = "";

            if (this.DisplayRoot == true
                && this.Nodes.Count == 0)
            {
                strError = "树在显示根的情况下，第一级必须有而且只有一个节点存在...";
                return -1;
            }

            DatabaseObject root = null;
            int nRet = BuildMemoryTree(
                this.DisplayRoot == false ?
                (TreeNode)null : this.Nodes[0],
                out root,
                out strError);
            if (nRet == -1)
                return -1;

            this.Nodes.Clear();

            // this.StorageMode = StorageMode.Memory;

            this.Root = root;
            this.FillAll(null);

            return 0;
        }
        */


        // 新对象(插入在同级末尾)
        private void menu_newObjectSibling_Click(object sender, System.EventArgs e)
        {
            DoNewObject(false);
        }


        // 新对象(插入在下级末尾)
        private void menu_newObjectChild_Click(object sender, System.EventArgs e)
        {
            DoNewObject(true);
        }

        // 删除对象
        private void menu_deleteObject_Click(object sender, System.EventArgs e)
        {
            if (this.SelectedNode == null)
            {
                MessageBox.Show("尚未选择要删除的对象...");
                return;
            }

            if (this.SelectedNode.ImageIndex == ResTree.RESTYPE_DB)
            {
                MessageBox.Show("这里不能删除数据库对象...");
                return;
            }

            string strPath = this.GetPath(this.SelectedNode);
            DatabaseObject obj = this.Root.LocateObject(strPath);
            if (obj == null)
            {
                MessageBox.Show(this, "路径为 '" + strPath + "' 的内存对象没有找到...");
                return;
            }

            /*
            this.channel = Channels.GetChannel(this.ServerUrl);

            Debug.Assert(channel != null, "Channels.GetChannel() 异常");

            DigitalPlatform.GUI.Stop stop = null;

            if (stopManager != null) 
            {
                stop = new DigitalPlatform.GUI.Stop();
			
                stop.Register(this.stopManager);	// 和容器关联

                stop.Initial(new Delegate_doStop(this.DoStop),
                    "正在删除对象: " + this.ServerUrl + "?" + strPath);
                stop.BeginLoop();

            }

            byte [] baTimestamp = new byte [1];
            byte [] baOutputTimestamp = null;
            // string strOutputPath = "";
            string strError = "";


            REDO:
        // 删除数据库对象

            long lRet = channel.DoDeleteRecord(strPath,
                baTimestamp,
                out baOutputTimestamp,
                out strError);
            if (lRet == -1)
            {
                // 时间戳不匹配
                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    baTimestamp = baOutputTimestamp;
                    goto REDO;
                }
            }
			

            if (stopManager != null) 
            {
                stop.EndLoop();
                stop.Initial(null, "");

                stop.Unregister();	// 和容器关联
            }

            this.channel = null;

            if (lRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }
            */

            // 把修改记录到日志
            ObjEvent objevent = new ObjEvent();
            objevent.Obj = obj;
            objevent.Oper = ObjEventOper.Delete;
            objevent.Path = strPath;
            this.Log.Add(objevent);

            // 刷新?
            // FillAll(null);
            this.SelectedNode.Remove();

            if (OnObjectDeleted != null)
            {
                OnObjectDeletedEventArgs newargs = new OnObjectDeletedEventArgs();
                newargs.ObjectPath = strPath;
                OnObjectDeleted(this, newargs);
            }

        }


        /*
        // 把当前Real模式的对象全部构造为内存树
        public int BuildMemoryTree(
            TreeNode treenode,
            out DatabaseObject root,
            out string strError)
        {
            Debug.Assert(false, "废止");
            root = null;
            strError = "";

			
            //if (this.StorageMode != StorageMode.Real)
            //{
            //	strError = "必须在Real存储模式下调用函数BuildMemoryTree()";
            //	return -1;
            //}
			

            root = new DatabaseObject();

            return BuildMemoryTree(
                treenode,
                root,
                out strError);
        }
        */

        int BuildMemoryTree(
            TreeNode parentTreeNode,
            DatabaseObject parentDatabaseObject,
            out string strError)
        {
            strError = "";

            TreeNodeCollection children = null;

            if (parentTreeNode == null)
            {
                children = this.Nodes;
            }
            else
            {
                children = parentTreeNode.Nodes;
            }

            //DatabaseObject newObj = null;

            if (parentTreeNode != null)	// 实根
            {
                TreeNode treenode = parentTreeNode;

                parentDatabaseObject.Type = treenode.ImageIndex;
                parentDatabaseObject.Name = treenode.Text;

                if (treenode.ImageIndex == ResTree.RESTYPE_DB)
                {

                    //newObj = parentDatabaseObject;
                }
                else if (treenode.ImageIndex == ResTree.RESTYPE_FOLDER)
                {

                    /*
                    newObj = DatabaseObject.BuildDirObject(treenode.Text);
                    newObj.Type = treenode.ImageIndex;
                    newObj.Parent = parentDatabaseObject;
                    parentDatabaseObject.Children.Add(newObj);
                    */
                    //newObj = parentDatabaseObject;
                }
                else if (treenode.ImageIndex == ResTree.RESTYPE_FILE)
                {
                    this.channel = Channels.GetChannel(this.ServerUrl);

                    Debug.Assert(channel != null, "Channels.GetChannel() 异常");

                    string strPath = "";
                    byte[] baTimeStamp = null;
                    string strMetaData;
                    string strOutputPath;

                    strPath = this.GetPath(treenode);

                    // string strStyle = "attachment,data,timestamp,outputpath";
                    string strStyle = "content,data,timestamp,outputpath";
                    using (MemoryStream stream = new MemoryStream())
                    {
                        long lRet = channel.GetRes(strPath,
                            stream,
                            null,	// stop,
                            strStyle,
                            null,	// byte [] input_timestamp,
                            out strMetaData,
                            out baTimeStamp,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                            return -1;

                        parentDatabaseObject.SetData(stream);
                        parentDatabaseObject.TimeStamp = baTimeStamp;
                    }

                    return 0;
                }
                else
                {
                    Debug.Assert(false, "意外的节点类型");
                }

            }

            for (int i = 0; i < children.Count; i++)
            {
                TreeNode treenode = children[i];

                DatabaseObject child = new DatabaseObject();
                child.Parent = parentDatabaseObject;
                parentDatabaseObject.Children.Add(child);

                int nRet = BuildMemoryTree(
                    treenode,
                    child,
                    out strError);
                if (nRet == -1)
                    return -1;


            }

            return 0;
        }


        /*
        // 将内存对象创建为真正的服务器端对象
        public int BuildRealObjects(
            string strDbName,
            DatabaseObject root,
            out string strError)
        {
            strError = "";

            int nRet = 0;

            // 创建根
            if (root.Type != -1)
            {
                if (root.Type == ResTree.RESTYPE_DB)
                    goto DOCHILD;	// 忽略本节点，但是继续作下级节点


                // 缺省配置文件，忽略保存
                if (root.IsDefaultFile() == true)
                    return 0;

                MemoryStream stream = null;
					
                if (root.Type == ResTree.RESTYPE_FILE)
                    stream = new MemoryStream(root.Content);

                string strPath = root.MakePath(strDbName);
// 在服务器端创建对象
                nRet = NewServerSideObject(strPath,
                    root.Type,
                    stream,
                    root.TimeStamp,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            DOCHILD:
            // 递归
            for(int i=0;i<root.Children.Count;i++)
            {
                DatabaseObject obj = (DatabaseObject)root.Children[i];

                nRet = BuildRealObjects(
                    strDbName,
                    obj,
                    out strError);
                if (nRet == -1)
                    return -1;
            }


            return 0;
        }
        */


        // 根据路径创建内存对象
        public int CreateObject(DatabaseObject obj,
            string strPath,
            out string strError)
        {
            strError = "";
            obj.Children.Clear();


            if (obj.Type == ResTree.RESTYPE_FILE)
            {
                byte[] baTimeStamp = null;
                string strMetaData;
                string strOutputPath;


                this.channel = Channels.GetChannel(this.ServerUrl);

                Debug.Assert(channel != null, "Channels.GetChannel() 异常");


                // string strStyle = "attachment,data,timestamp,outputpath";
                string strStyle = "content,data,timestamp,outputpath";
                using (MemoryStream stream = new MemoryStream())
                {

                    long lRet = channel.GetRes(strPath,
                        stream,
                        null,	// stop,
                        strStyle,
                        null,	// byte [] input_timestamp,
                        out strMetaData,
                        out baTimeStamp,
                        out strOutputPath,
                        out strError);
                    if (lRet == -1)
                    {
                        // obj.SetData(null);
                        obj.TimeStamp = null;
                        return 0;	// 继续处理
                    }

                    obj.SetData(stream);
                    obj.TimeStamp = baTimeStamp;
                }
            }

            if (obj.Type == ResTree.RESTYPE_DB
                || obj.Type == ResTree.RESTYPE_FOLDER)
            {

                this.channel = Channels.GetChannel(this.ServerUrl);

                Debug.Assert(channel != null, "Channels.GetChannel() 异常");

                ResInfoItem[] items = null;

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
                    null,   // 不需要返回全部语言的名字
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
                    return -1;

                if (items == null)
                    return 0;

                for (int i = 0; i < items.Length; i++)
                {
                    // 忽略from类型节点
                    if (items[i].Type == ResTree.RESTYPE_FROM)
                        continue;

                    DatabaseObject child = new DatabaseObject();
                    child.Name = items[i].Name;
                    child.Type = items[i].Type;
                    child.Style = items[i].Style;

                    child.Parent = obj;
                    obj.Children.Add(child);

                    int nRet = CreateObject(child,
                        strPath + "/" + items[i].Name,
                        out strError);
                    if (nRet == -1)
                        return -1;

                }
            }




            return 0;
        }

        int GetDbStyle(string strDbName,
            out string strError)
        {
            strError = "";

            this.channel = Channels.GetChannel(this.ServerUrl);

            Debug.Assert(channel != null, "Channels.GetChannel() 异常");

            ResInfoItem[] items = null;

            DigitalPlatform.Stop stop = null;

            if (stopManager != null)
            {
                stop = new DigitalPlatform.Stop();
                stop.Register(this.stopManager, true);	// 和容器关联

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在获得数据库 " + strDbName + "的风格参数");

                stop.BeginLoop();
            }

            long lRet = channel.DoDir("",   // 列出全部数据库
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
                return -1;

            if (items == null)
                return 0;   // 数据库不存在

            for (int i = 0; i < items.Length; i++)
            {
                // 忽略非数据库类型节点
                if (items[i].Type != ResTree.RESTYPE_DB)
                    continue;

                if (items[i].Name == strDbName)
                    return items[i].Style;
            }

            return 0;   // 数据库不存在
        }

        // 将树上全部对象以特定类型加入日志中
        public void PutAllObjToLog(ObjEventOper oper,
            DatabaseObject root)
        {
            ObjEvent objevent = new ObjEvent();

            objevent.Obj = root;
            objevent.Oper = oper;
            objevent.Path = root.MakePath(this.DbName);

            this.Log.Add(objevent);

            if (oper == ObjEventOper.Delete
                && root.Type == ResTree.RESTYPE_FOLDER)
                return;	// 删除目录对象, 只要这个对象进入了日志, 其下级均不必再进入

            // 递归
            for (int i = 0; i < root.Children.Count; i++)
            {
                PutAllObjToLog(oper,
                    (DatabaseObject)root.Children[i]);
            }
        }

        // 2012/4/18
        // 刷新队列中制定位置以后的、针对某个对象的全部操作的时间戳
        void RefreshTimestamp(
            int nStartIndex,
            string strPath,
            byte[] baTimestamp)
        {
            ObjEventCollection log = this.Log;
            for (int i = nStartIndex; i < log.Count; i++)
            {
                ObjEvent objevent = (ObjEvent)log[i];

                if (objevent.Obj.Type == -1)    // 不完整的对象
                    continue;

                if (objevent.Obj.Type == ResTree.RESTYPE_DB)
                    continue;

                // 缺省配置文件，忽略操作
                if (objevent.Oper == ObjEventOper.New
                    || objevent.Oper == ObjEventOper.Change)
                {
                    if (DatabaseObject.IsDefaultFile(objevent.Path) == true)
                        continue;
                }

                if (objevent.Path != strPath)
                    continue;

                if (objevent.Obj != null)
                    objevent.Obj.TimeStamp = baTimestamp;
            }

        }

        public int SubmitLog(out string strErrorText)
        {
            strErrorText = "";
            int nRet;

            ObjEventCollection log = this.Log;
            string strError = "";

            for (int i = 0; i < log.Count; i++)
            {
                ObjEvent objevent = (ObjEvent)log[i];

                if (objevent.Obj.Type == -1)    // 不完整的对象
                    continue;

                if (objevent.Obj.Type == ResTree.RESTYPE_DB)
                    continue;

                // 缺省配置文件，忽略操作
                if (objevent.Oper == ObjEventOper.New
                    || objevent.Oper == ObjEventOper.Change)
                {
                    if (DatabaseObject.IsDefaultFile(objevent.Path) == true)
                        continue;
                }

                if (objevent.Oper == ObjEventOper.New)
                {
                    MemoryStream stream = null;
                    try
                    {
                        if (objevent.Obj.Type == ResTree.RESTYPE_FILE
                            && objevent.Obj.Content != null)
                            stream = new MemoryStream(objevent.Obj.Content);

                        string strPath = objevent.Path;
                        byte[] baOutputTimestamp = null;
                        nRet = NewServerSideObject(strPath,
                            objevent.Obj.Type,
                            stream,
                            objevent.Obj.TimeStamp,
                            out baOutputTimestamp,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "新建对象 '" + strPath + "' 时发生错误: " + strError;
                            MessageBox.Show(this, strError);
                            strErrorText += strError + "\r\n";
                            // return -1;
                        }
                        else
                        {
                            // 如果创建成功，需要把队列中后面的所有即将操作相同对象的动作修改时间戳
                            // 刷新队列中制定位置以后的、针对某个对象的全部操作的时间戳
                            RefreshTimestamp(
                                i + 1,
                                strPath,
                                baOutputTimestamp);
                        }
                    }
                    finally
                    {
                        if (stream != null)
                            stream.Close();
                    }
                }
                if (objevent.Oper == ObjEventOper.Change)
                {
                    MemoryStream stream = null;
                    try
                    {
                        if (objevent.Obj.Type == ResTree.RESTYPE_FILE)
                            stream = new MemoryStream(objevent.Obj.Content);

                        string strPath = objevent.Path;
                        byte[] baOutputTimestamp = null;
                        nRet = NewServerSideObject(strPath,
                            objevent.Obj.Type,
                            stream,
                            objevent.Obj.TimeStamp,
                            out baOutputTimestamp,
                            out strError);
                        if (nRet == -1)
                        {
                            strError = "修改对象 '" + strPath + "' 时发生错误: " + strError;
                            MessageBox.Show(this, strError);
                            strErrorText += strError + "\r\n";
                        }
                        else
                        {
                            // 如果创建成功，需要把队列中后面的所有即将操作相同对象的动作修改时间戳
                            // 刷新队列中制定位置以后的、针对某个对象的全部操作的时间戳
                            RefreshTimestamp(
                                i + 1,
                                strPath,
                                baOutputTimestamp);
                        }
                    }
                    finally
                    {
                        if (stream != null)
                            stream.Close();
                    }

                }
                else if (objevent.Oper == ObjEventOper.Delete)
                {
                    // TODO: 现在已经有时间戳了，可以不必重试

                    this.channel = Channels.GetChannel(this.ServerUrl);

                    Debug.Assert(channel != null, "Channels.GetChannel() 异常");

                    byte[] baTimestamp = new byte[1];
                    byte[] baOutputTimestamp = null;
                    string strPath = objevent.Path;
                // string strOutputPath = "";
                REDO:
                    // 删除数据库对象
                    long lRet = channel.DoDeleteRes(strPath,
                        baTimestamp,
                        "",
                        out baOutputTimestamp,
                        out strError);
                    if (lRet == -1)
                    {
                        // 时间戳不匹配
                        if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                        {
                            baTimestamp = baOutputTimestamp;
                            goto REDO;
                        }
                        strError = "删除对象 '" + strPath + "' 时发生错误: " + strError;
                        MessageBox.Show(this, strError);
                        strErrorText += strError + "\r\n";
                    }
                }

            }

            log.Clear();

            if (strErrorText == "")
                return 0;
            return -1;
        }

        public void ClearLog()
        {
            this.Log.Clear();
        }

    }


    // 对象被删除
    public delegate void OnObjectDeletedEventHandle(object sender,
    OnObjectDeletedEventArgs e);

    public class OnObjectDeletedEventArgs : EventArgs
    {
        public string ObjectPath = "";
    }

}
