using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Xml;
using System.Text;

using DigitalPlatform.GUI;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;

using DigitalPlatform.rms.Client.rmsws_localhost;
using System.Threading.Tasks;
using System.Linq;
using System.Data.SqlClient;
using System.Security.Policy;

namespace DigitalPlatform.rms.Client
{
    /// <summary>
    /// Summary description for ResTree.
    /// </summary>
    public class ResTree : System.Windows.Forms.TreeView
    {
        public ApplicationInfo AppInfo = null;

        public DigitalPlatform.StopManager stopManager = null;

        RmsChannel channel = null;

        #region	资源类型。可作Icon下标用

        public const int RESTYPE_SERVER = 2;
        public const int RESTYPE_DB = 0;
        public const int RESTYPE_FROM = 1;
        public const int RESTYPE_LOADING = 3;
        public const int RESTYPE_FOLDER = 4;
        public const int RESTYPE_FILE = 5;

        #endregion

        #region 资源风格
        public const int RESSTYLE_USERDATABASE = 0x01;
        #endregion


        public string Lang = "zh";

        public int[] EnabledIndices = null; // null表示全部发黑。如果对象存在，但是元素个数为0，表示全部发灰

        public ServerCollection Servers = null; // 引用
        public RmsChannelCollection Channels = null;

        public event GuiAppendMenuEventHandle OnSetMenu;

        private System.Windows.Forms.ImageList imageList_resIcon;
        private System.ComponentModel.IContainer components;

        public ResTree()
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

                DoStop(null, null);

                if (components != null)
                {
                    components.Dispose();
                }
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ResTree));
            this.imageList_resIcon = new System.Windows.Forms.ImageList(this.components);
            this.SuspendLayout();
            // 
            // imageList_resIcon
            // 
            this.imageList_resIcon.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_resIcon.ImageStream")));
            this.imageList_resIcon.TransparentColor = System.Drawing.Color.Fuchsia;
            this.imageList_resIcon.Images.SetKeyName(0, "database.bmp");
            this.imageList_resIcon.Images.SetKeyName(1, "searchfrom.bmp");
            this.imageList_resIcon.Images.SetKeyName(2, "");
            this.imageList_resIcon.Images.SetKeyName(3, "");
            this.imageList_resIcon.Images.SetKeyName(4, "");
            this.imageList_resIcon.Images.SetKeyName(5, "");
            // 
            // ResTree
            // 
            this.LineColor = System.Drawing.Color.Black;
            this.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.ResTree_AfterCheck);
            this.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ResTree_MouseUp);
            this.AfterExpand += new System.Windows.Forms.TreeViewEventHandler(this.ResTree_AfterExpand);
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ResTree_MouseDown);
            this.ResumeLayout(false);

        }
        #endregion

        public int Fill(TreeNode node)
        {
            string strError = "";
            int nRet = Fill(node, out strError);
            if (nRet == -1)
            {
                try
                {
                    this.MessageBoxShow(strError);
                }
                catch
                {
                    // this可能已经不存在
                }
                return -1;
            }

            return nRet;
        }

        // 递归
        public int Fill(TreeNode node,
            out string strError)
        {
            strError = "";
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

                for (i = 0; i < Servers.Count; i++)
                {
                    Server server = (Server)Servers[i];
                    TreeNode nodeNew = new TreeNode(server.Url, RESTYPE_SERVER, RESTYPE_SERVER);
                    SetLoading(nodeNew);

                    if (EnabledIndices != null
                        && StringUtil.IsInList(nodeNew.ImageIndex, EnabledIndices) == false)
                        nodeNew.ForeColor = ControlPaint.LightLight(nodeNew.ForeColor);

                    children.Add(nodeNew);
                }

                return 0;
            }


            // 根以下的节点类型
            ResPath respath = new ResPath(node);

            this.channel = Channels.GetChannel(respath.Url);

            Debug.Assert(channel != null, "Channels.GetChannel() 异常");

            /*
			int nStart = 0;
			int nPerCount = -1;
			int nCount = 0;
			*/

            ResInfoItem[] items = null;

#if NO
			DigitalPlatform.Stop stop = null;

			if (stopManager != null) 
			{
				stop = new DigitalPlatform.Stop();

                stop.Register(this.stopManager, true);	// 和容器关联

                stop.OnStop += new StopEventHandler(this.DoStop);
				stop.Initial("正在列目录: " + respath.FullPath);
				stop.BeginLoop();
			}
#endif
            DigitalPlatform.Stop stop = PrepareStop("正在列目录: " + respath.FullPath);

            long lRet = 0;
            try
            {
                lRet = channel.DoDir(respath.Path,
                    this.Lang,
                    null,   // 不需要列出全部语言的名字
                    out items,
                    out strError);
            }
            finally
            {
                EndStop(stop);
            }

#if NO
			if (stopManager != null) 
			{
				stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
				stop.Initial("");

				stop.Unregister();	// 和容器关联
			}
#endif

            this.channel = null;

            if (lRet == -1)
            {
#if NO
				try 
				{
					MessageBox.Show(this, "Channel::DoDir() Error: " + strError);
				}
				catch
				{
					// this可能已经不存在
					return -1;
				}
#endif
                if (node != null)
                {
                    SetLoading(node);   // 出错的善后处理，重新出现+号
                    node.Collapse();
                }
                return -1;
            }


            if (items != null)
            {
                children.Clear();

                //for(i=0;i<items.Length;i++) 
                foreach (ResInfoItem res_item in items)
                {
                    // ResInfoItem res_item = items[i];

                    TreeNode nodeNew = new TreeNode(res_item.Name, res_item.Type, res_item.Type);

                    if (res_item.Type == RESTYPE_DB)
                    {
                        DbProperty prop = new DbProperty();
                        prop.TypeString = res_item.TypeString;  // 类型字符串
                        nodeNew.Tag = prop;
                        List<string> column_names = null;
                        int nRet = GetBrowseColumns(
                            node.Text,
                            res_item.Name,
                            out column_names,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        prop.ColumnNames = column_names;
                    }
                    else
                    {
                        ItemProperty prop = new ItemProperty();
                        prop.TypeString = res_item.TypeString;  // 类型字符串
                        nodeNew.Tag = prop;
                    }

                    if (res_item.HasChildren)
                        SetLoading(nodeNew);

                    if (EnabledIndices != null
                        && StringUtil.IsInList(nodeNew.ImageIndex, EnabledIndices) == false)
                        nodeNew.ForeColor = ControlPaint.LightLight(nodeNew.ForeColor);

                    children.Add(nodeNew);
                }
            }

            return 0;
        }

        // return:
        //      -1  出错，不希望继续以后的操作
        //      0   成功
        public int GetBrowseColumns(
            string strServerUrl,
            string strDbName,
            out List<string> column_names,
            out string strError)
        {
            strError = "";
            column_names = new List<string>();

            this.channel = Channels.GetChannel(strServerUrl);
            Debug.Assert(channel != null, "Channels.GetChannel() 异常");

            try
            {
                string strCfgFilePath = strDbName + "/cfgs/browse";
                string strStyle = "content,data";   // ,metadata,timestamp,outputpath";

                string strResult = "";
                string strMetaData = "";
                byte[] baOutputTimeStamp = null;
                string strOutputResPath = "";
                long lRet = this.channel.GetRes(strCfgFilePath,
                    strStyle,
                    out strResult,
                    out strMetaData,
                    out baOutputTimeStamp,
                    out strOutputResPath,
                    out strError);
                if (lRet == -1)
                    return -1;

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strResult);
                }
                catch (Exception ex)
                {
                    strError = "配置文件 " + strCfgFilePath + " 内容装入XMLDOM时出错: " + ex.Message;
                    return -1;
                }

                XmlNodeList nodes = dom.DocumentElement.SelectNodes("//col");
                for (int j = 0; j < nodes.Count; j++)
                {
                    string strColumnTitle = DomUtil.GetAttr(nodes[j], "title");
                    column_names.Add(strColumnTitle);
                }

                return 0;
            }
            finally
            {
                this.channel = null;
            }
        }

        // 回调函数
        void DoStop(object sender, StopEventArgs e)
        {
            if (this.channel != null)
                this.channel.Abort();
        }

        // 在一个节点下级插入"loading..."，以便出现+号
        public static void SetLoading(TreeNode node)
        {
            if (node == null)
                return;

            // 新node
            TreeNode nodeNew = new TreeNode("loading...", RESTYPE_LOADING, RESTYPE_LOADING);

            node.Nodes.Clear();
            node.Nodes.Add(nodeNew);
        }

        // 下级是否包含loading...?
        public static bool IsLoading(TreeNode node)
        {
            if (node.Nodes.Count == 0)
                return false;

            if (node.Nodes[0].Text == "loading...")
                return true;

            return false;
        }

        private void ResTree_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            // TreeNode node = this.SelectedNode;

            //
            menuItem = new MenuItem("新增服务器(&A)");
            menuItem.Click += new System.EventHandler(this.menu_newServer);
            //	menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            // 
            menuItem = new MenuItem("登录(&L)");
            menuItem.Click += new System.EventHandler(this.menu_login);
            if (this.SelectedNode == null)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("登出(&O)");
            menuItem.Click += new System.EventHandler(this.menu_logout);
            if (this.SelectedNode == null)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("刷新(&R)");
            menuItem.Click += new System.EventHandler(this.menu_refresh);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("初始化数据库(&I)");
            menuItem.Click += new System.EventHandler(this.menu_initialDB);
            if (this.SelectedNode == null || this.SelectedNode.ImageIndex != RESTYPE_DB)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("刷新数据库定义(&R)");
            menuItem.Click += new System.EventHandler(this.menu_refreshDB);
            if (this.SelectedNode == null || this.SelectedNode.ImageIndex != RESTYPE_DB)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("编辑配置文件(&E)");
            menuItem.Click += new System.EventHandler(this.menu_editCfgFile);
            if (this.SelectedNode == null || this.SelectedNode.ImageIndex != RESTYPE_FILE)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("修改密码(&P)");
            menuItem.Click += new System.EventHandler(this.menu_changePassword);
            if (this.SelectedNode == null)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("后台任务(&T)");
            menuItem.Click += new System.EventHandler(this.menu_batchTask);
            if (this.SelectedNode == null)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("导出数据(&E)");
            menuItem.Click += new System.EventHandler(this.menu_export);
            if (this.SelectedNode == null)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("导入数据(&I)");
            menuItem.Click += new System.EventHandler(this.menu_import);
            if (this.SelectedNode == null)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("快速导入数据(&S)");
            menuItem.Click += new System.EventHandler(this.menu_quickImport);
            if (this.SelectedNode == null)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("管理操作");
            contextMenu.MenuItems.Add(menuItem);

            MenuItem subMenuItem = new MenuItem("delete keys index");
            subMenuItem.Click += new System.EventHandler(this.menu_deleteKeysIndex);
            if (this.SelectedNode == null)
                subMenuItem.Enabled = false;
            menuItem.MenuItems.Add(subMenuItem);


            subMenuItem = new MenuItem("create keys index");
            subMenuItem.Click += new System.EventHandler(this.menu_createKeysIndex);
            if (this.SelectedNode == null)
                subMenuItem.Enabled = false;
            menuItem.MenuItems.Add(subMenuItem);

            subMenuItem = new MenuItem("disable keys index");
            subMenuItem.Click += new System.EventHandler(this.menu_disableKeysIndex);
            if (this.SelectedNode == null)
                subMenuItem.Enabled = false;
            menuItem.MenuItems.Add(subMenuItem);

            subMenuItem = new MenuItem("rebuild keys index");
            subMenuItem.Click += new System.EventHandler(this.menu_rebuildKeysIndex);
            if (this.SelectedNode == null)
                subMenuItem.Enabled = false;
            menuItem.MenuItems.Add(subMenuItem);

            subMenuItem = new MenuItem("flush pending keys");
            subMenuItem.Click += new System.EventHandler(this.menu_flushKeysCache);
            if (this.SelectedNode == null)
                subMenuItem.Enabled = false;
            menuItem.MenuItems.Add(subMenuItem);

            subMenuItem = new MenuItem("endfastappend");
            subMenuItem.Click += new System.EventHandler(this.menu_endFastAppend);
            if (this.SelectedNode == null)
                subMenuItem.Enabled = false;
            menuItem.MenuItems.Add(subMenuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 
            menuItem = new MenuItem("允许复选(&M)");
            menuItem.Click += new System.EventHandler(this.menu_toggleCheckBoxes);
            if (this.CheckBoxes == true)
                menuItem.Checked = true;
            else
                menuItem.Checked = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("清除全部复选(&C)");
            menuItem.Click += new System.EventHandler(this.menu_clearCheckBoxes);
            if (this.CheckBoxes == true)
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("全选下级节点(&A)");
            menuItem.Click += new System.EventHandler(this.menu_checkAllSubNodes);
            menuItem.Enabled = this.CheckBoxes;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 
            menuItem = new MenuItem("节点信息(&N)");
            menuItem.Click += new System.EventHandler(this.menu_nodeInfo);
            contextMenu.MenuItems.Add(menuItem);


            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 
            menuItem = new MenuItem("在下级创建新目录或文件(&N)");
            menuItem.Click += new System.EventHandler(this.menu_newDirectoryOrFile);
            if (this.SelectedNode != null
                && (this.SelectedNode.ImageIndex == ResTree.RESTYPE_SERVER
                || this.SelectedNode.ImageIndex == ResTree.RESTYPE_DB
                || this.SelectedNode.ImageIndex == ResTree.RESTYPE_FOLDER))
                menuItem.Enabled = true;
            else
                menuItem.Enabled = false;

            contextMenu.MenuItems.Add(menuItem);

            // 测试
            {
                menuItem = new MenuItem("测试超时");
                menuItem.Click += menuItem_testTimeout_Click;
                contextMenu.MenuItems.Add(menuItem);
            }


            ////



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

        // 全选下级节点
        void menu_checkAllSubNodes(object sender, EventArgs e)
        {
            TreeNode node = this.SelectedNode;
            if (node == null)
                return;

            foreach (TreeNode current in node.Nodes)
            {
                current.Checked = true;
            }
        }

        // 测试超时
        void menuItem_testTimeout_Click(object sender, EventArgs e)
        {
            ResPath respath = new ResPath(this.SelectedNode);

            this.channel = Channels.GetChannel(respath.Url);

            Debug.Assert(channel != null, "Channels.GetChannel() 异常");

            this.channel.DoTest("test");

            this.channel = null;
        }

        // 新创建一个目录或文件
        void menu_newDirectoryOrFile(object sender, System.EventArgs e)
        {
            string strError = "";

            int nRet = NewServerSideObject(ResTree.RESTYPE_FOLDER, out strError);
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 1)
            {
                strError = "对象已经存在 : " + strError;
                goto ERROR1;
            }

            menu_refresh(null, null);
            if (this.SelectedNode != null)
                this.SelectedNode.Expand();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            return;
        }

        int NewServerSideObject(int nType,
            out string strError)
        {
            strError = "";
            if (this.SelectedNode == null)
            {
                strError = "尚未选择基准节点";
                goto ERROR1;
            }

            if (this.SelectedNode.ImageIndex != ResTree.RESTYPE_SERVER
                && this.SelectedNode.ImageIndex != ResTree.RESTYPE_DB
                && this.SelectedNode.ImageIndex != ResTree.RESTYPE_FOLDER)
            {
                strError = "只能在服务器、数据库、目录对象之下创建新目录";
                goto ERROR1;
            }

            NewObjectDlg dlg = new NewObjectDlg();
            dlg.Font = GuiUtil.GetDefaultFont();
            dlg.Type = nType;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return 0;

            ResPath respath = new ResPath(this.SelectedNode);

            string strPath = "";
            if (respath.Path == "")
                strPath = dlg.textBox_objectName.Text;
            else
                strPath = respath.Path + "/" + dlg.textBox_objectName.Text;

            int nRet = NewServerSideObject(
                respath.Url,
                strPath,
                dlg.Type,
                null,
                null,   // byte[] baTimeStamp,
                out strError);

            return nRet;
        ERROR1:
            return -1;
        }

        // 在服务器端创建对象
        // return:
        //		-1	错误
        //		1	以及存在同名对象
        //		0	正常返回
        int NewServerSideObject(
            string strServerUrl,
            string strPath,
            int nType,
            Stream stream,
            byte[] baTimeStamp,
            out string strError)
        {
            strError = "";

            this.channel = Channels.GetChannel(strServerUrl);
            Debug.Assert(channel != null, "Channels.GetChannel() 异常");

        REDO:

#if NO
            DigitalPlatform.Stop stop = null;

            if (stopManager != null)
            {
                stop = new DigitalPlatform.Stop();
                stop.Register(this.stopManager, true);	// 和容器关联

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在创建新对象: " + strServerUrl + "?" + strPath);
                stop.BeginLoop();

            }
#endif
            DigitalPlatform.Stop stop = PrepareStop("正在创建新对象: " + strServerUrl + "?" + strPath);

            byte[] baOutputTimestamp = null;
            string strOutputPath = "";
            string strStyle = "";

            if (nType == ResTree.RESTYPE_FOLDER)
                strStyle = "createdir";

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

            EndStop(stop);
#if NO
            if (stopManager != null)
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
                stop.Unregister();	// 和容器关联
            }
#endif

            if (lRet == -1)
            {
                if (this.channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    baTimeStamp = baOutputTimestamp;
                    goto REDO;
                }

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

        // 刷新
        public void menu_refresh(object sender, System.EventArgs e)
        {
            /*
			if (this.SelectedNode == null) 
			{
				this.Fill(null);
				return;
			}

			ResPath respath = new ResPath(this.SelectedNode);

			// 刷新
			ResPath OldPath = new ResPath(this.SelectedNode);

			respath.Path = "";
			ExpandPath(respath);	// 选中服务器，以下节点清除
			SetLoading(this.SelectedNode);

			ExpandPath(OldPath);
             */
            this.Refresh(RefreshStyle.All);
        }

        // 刷新风格
        public enum RefreshStyle
        {
            All = 0xffff,
            Servers = 0x01,
            Selected = 0x02,
        }

        public void Refresh(RefreshStyle style)
        {
            ResPath OldPath = null;
            bool bExpanded = false;

            // 保存
            if (this.SelectedNode != null)
            {
                OldPath = new ResPath(this.SelectedNode);
                bExpanded = this.SelectedNode.IsExpanded;
            }

            // 刷新服务器级
            if ((style & RefreshStyle.Servers) == RefreshStyle.Servers)
            {
                this.Fill(null);
            }


            // 刷新当前选择的节点
            if (OldPath != null
                && (style & RefreshStyle.Selected) == RefreshStyle.Selected)
            {
                ResPath respath = OldPath.Clone();

                // 刷新

                respath.Path = "";
                ExpandPath(respath);	// 选中服务器，以下节点清除
                SetLoading(this.SelectedNode);

                ExpandPath(OldPath);
                if (bExpanded == true && this.SelectedNode != null)
                    this.SelectedNode.Expand();
            }
        }

        // 新增一个服务器节点
        void menu_newServer(object sender, System.EventArgs e)
        {
            ServerNameDlg dlg = new ServerNameDlg();
            dlg.Font = GuiUtil.GetDefaultFont();
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            int nRet = Servers.NewServer(dlg.textBox_url.Text, -1);
            if (nRet == 1)
            {
                MessageBox.Show(this, "服务器 " + dlg.textBox_url.Text + " 已经存在...");
                return;
            }

            // 刷新
            this.Fill(null);

            // 刷新后恢复原来选择的node
        }

        // 登录
        void menu_login(object sender, System.EventArgs e)
        {
            if (this.SelectedNode == null)
            {
                MessageBox.Show(this, "尚未选择节点");
                return;
            }

            ResPath respath = new ResPath(this.SelectedNode);

            this.channel = Channels.GetChannel(respath.Url);

            Debug.Assert(channel != null, "Channels.GetChannel() 异常");

            string strError;
            // return:
            //		-1	error
            //		0	login failed
            //		1	login succeed
            int nRet = channel.UiLogin(
                null,
                respath.Path,
                LoginStyle.FillDefaultInfo,
                out strError);


            this.channel = null;

            if (nRet == -1 || nRet == 0)
            {
                MessageBox.Show(this, strError);
                return;
            }

            // 刷新
            ResPath OldPath = new ResPath(this.SelectedNode);

            respath.Path = "";
            ExpandPath(respath);    // 选中服务器，以下节点清除
            SetLoading(this.SelectedNode);

            ExpandPath(OldPath);

        }

        // 登出
        void menu_logout(object sender, System.EventArgs e)
        {
            if (this.SelectedNode == null)
            {
                MessageBox.Show(this, "尚未选择节点");
                return;
            }

            ResPath respath = new ResPath(this.SelectedNode);

            this.channel = Channels.GetChannel(respath.Url);

            Debug.Assert(channel != null, "Channels.GetChannel() 异常");

#if NO
			DigitalPlatform.Stop stop = null;

			if (stopManager != null) 
			{
				stop = new DigitalPlatform.Stop();

                stop.Register(this.stopManager, true);	// 和容器关联

                stop.OnStop += new StopEventHandler(this.DoStop);
				stop.Initial("正在登出: " + respath.FullPath);
				stop.BeginLoop();

			}
#endif
            DigitalPlatform.Stop stop = PrepareStop("正在登出: " + respath.FullPath);

            string strError;
            // return:
            //		-1	error
            //		0	login failed
            //		1	login succeed
            long nRet = channel.DoLogout(
                out strError);

            EndStop(stop);
#if NO
			if (stopManager != null) 
			{
				stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
				stop.Initial("");

				stop.Unregister();	// 和容器关联
			}
#endif

            this.channel = null;

            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }

            // 刷新
            //ResPath OldPath = new ResPath(this.SelectedNode);

            respath.Path = "";
            ExpandPath(respath);    // 选中服务器，以下节点清除
            SetLoading(this.SelectedNode);
            if (this.SelectedNode != null)
                this.SelectedNode.Collapse();

            //ExpandPath(OldPath);

        }

        void menu_deleteKeysIndex(object sender, System.EventArgs e)
        {
            ManageKeysIndex("deletekeysindex");
        }

        void menu_createKeysIndex(object sender, System.EventArgs e)
        {
            ManageKeysIndex("createkeysindex");
        }

        void menu_disableKeysIndex(object sender, System.EventArgs e)
        {
            ManageKeysIndex("disablekeysindex");
        }

        void menu_rebuildKeysIndex(object sender, System.EventArgs e)
        {
            ManageKeysIndex("rebuildkeysindex");
        }

        void menu_flushKeysCache(object sender, System.EventArgs e)
        {
            ManageKeysIndex("flushpendingkeys");
        }

        // 2024/6/4
        void menu_endFastAppend(object sender, EventArgs e)
        {
            string strError = "";
            TreeNode selectedNode = (TreeNode)this.Invoke(new Func<TreeNode>(() =>
            {
                return this.SelectedNode;
            }));

            if (selectedNode == null)
            {
                strError = "尚未选择要要导入数据的数据库节点";
                goto ERROR1;
            }

            if (selectedNode.ImageIndex != RESTYPE_DB)
            {
                strError = "所选择的节点不是数据库类型。请选择要导入数据的数据库节点。";
                goto ERROR1;
            }

            ResPath default_target_respath = new ResPath(selectedNode);
            var url = GetDbUrl(default_target_respath.FullPath);
            EndFastAppend(null, new List<string> { url });
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void ManageKeysIndex(string strAction)
        {
            string strError = "";
            int nRet = ManageKeysIndex(null, strAction, null, out strError);
            if (nRet == -1)
                goto ERROR1;
            MessageBox.Show(this, "操作成功");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        int ManageKeysIndex(
            string strDbUrl,
            string strAction,
            string strMessage,
            out string strError)
        {
            strError = "";

            ResPath respath = null;
            if (strDbUrl == null)
            {
                if (this.SelectedNode == null)
                {
                    strError = "尚未选择要要操作的数据库节点";
                    goto ERROR1;
                }

                if (this.SelectedNode.ImageIndex != RESTYPE_DB)
                {
                    strError = "所选择的节点不是数据库类型。请选择要操作的数据库节点。";
                    goto ERROR1;
                }
                respath = new ResPath(this.SelectedNode);
            }
            else
                respath = new ResPath(strDbUrl);

            this.channel = Channels.GetChannel(respath.Url);
            Debug.Assert(channel != null, "Channels.GetChannel() 异常");

#if NO
            DigitalPlatform.Stop stop = null;

            if (stopManager != null)
            {
                stop = new DigitalPlatform.Stop();

                stop.Register(this.stopManager, true);	// 和容器关联

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在导出数据 " + respath.FullPath);
                stop.BeginLoop();
            }
#endif
            DigitalPlatform.Stop stop = PrepareStop(
                strMessage == null ?
                "正在对 " + respath.FullPath + " 进行管理操作 " + strAction + " ..."
                : strMessage);

            TimeSpan old_timeout = channel.Timeout;
            if (strAction == "endfastappend")
            {
                // 收尾阶段可能要耗费很长的时间
                channel.Timeout = new TimeSpan(3, 0, 0);
            }

            try
            {
                // TODO: 改造为新的查询任务是否完成的用法
                long lRet = channel.DoRefreshDB(
                    strAction,
                    respath.Path,
                    false,
                    out strError);
                if (lRet == -1)
                {
                    strError = "管理数据库 '" + respath.Path + "' 时出错: " + strError;
                    goto ERROR1;
                }

                // 2019/5/13
                return (int)lRet;
            }
            finally
            {
                EndStop(stop);
#if NO
                if (stopManager != null)
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");

                    stop.Unregister();	// 和容器脱离关联
                }
#endif
                if (strAction == "endfastappend")
                {
                    channel.Timeout = old_timeout;
                }

                this.channel = null;
            }
        ERROR1:
            return -1;
        }

        // 快速导入
        void menu_quickImport(object sender, System.EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                ImportData(true);
            });
        }

        // 慢速导入数据
        void menu_import(object sender, System.EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                ImportData(false);
            });
        }

        // TODO: 改造为可以在非界面线程运行
        // 导入数据
        int ImportData(bool bFastMode = false)
        {
            string strError = "";
            int CHUNK_SIZE = 150 * 1024;    // 70

            TreeNode selectedNode = (TreeNode)this.Invoke(new Func<TreeNode>(() =>
            {
                return this.SelectedNode;
            }));

            if (selectedNode == null)
            {
                strError = "尚未选择要要导入数据的数据库节点";
                goto ERROR0;
            }

            if (selectedNode.ImageIndex != RESTYPE_DB)
            {
                strError = "所选择的节点不是数据库类型。请选择要导入数据的数据库节点。";
                goto ERROR0;
            }

            if (bFastMode == true)
            {
                DialogResult result = (DialogResult)this.Invoke(new Func<DialogResult>(() =>
                {
                    return MessageBox.Show(this,
        "警告：\r\n在快速导入期间，相关数据库会进入一种锁定状态，对数据库的其他检索和修改操作暂时会被禁止，直到处理完成。\r\n\r\n请问确实要进行快速导入么?",
        "导入数据",
        MessageBoxButtons.YesNo,
        MessageBoxIcon.Question,
        MessageBoxDefaultButton.Button2);
                }));
                if (result == DialogResult.No)
                    return 0;
            }

#if NO
            OpenFileDialog dlg = new OpenFileDialog();

            dlg.Title = "请指定要导入的数据文件";
            dlg.FileName = "";
            dlg.Filter = "备份文件 (*.dp2bak)|*.dp2bak|XML文件 (*.xml)|*.xml|ISO2709文件 (*.iso;*.mrc)|*.iso;*.mrc|All files (*.*)|*.*";
            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog() != DialogResult.OK)
            {
                return 0;
            }
#endif
            // string fileName = "";
            bool importDataRecord = false;
            bool insertMissing = false;
            bool importObject = false;
            List<string> filenames = new List<string>();

            int nRet = (int)this.Invoke(new Func<int>(() =>
            {
                using (ImportDataDialog dlg = new ImportDataDialog())
                {
                    dlg.Font = GuiUtil.GetDefaultFont();
                    // dlg.FileName = "";
                    dlg.FileNames = null;

                    if (this.AppInfo != null)
                    {
                        this.AppInfo.LinkFormState(dlg, "ImportDataDialog_state");

                        dlg.UiState = this.AppInfo.GetString(
                            "ResTree",
                            "ImportDataDialog_uiState",
                            "");
                    }

                    dlg.ShowDialog(this);

                    if (this.AppInfo != null)
                    {
                        this.AppInfo.SetString(
                            "ResTree",
                            "ImportDataDialog_uiState",
                            dlg.UiState);
                    }

                    if (dlg.DialogResult != DialogResult.OK)
                        return 0;

                    // fileName = dlg.FileName;
                    filenames = dlg.FileNames.ToList();
                    importDataRecord = dlg.ImportDataRecord;
                    insertMissing = dlg.InsertMissing;
                    importObject = dlg.ImportObject;
                    return 1;
                }
            }));
            if (nRet == 0)
                return 0;

            List<string> target_dburls = new List<string>();
            List<string> times = new List<string>();    // 记载每个文件导入耗费的时间
            long lTotalCount = 0;

            string syntax = "";
            Encoding encoding = Encoding.UTF8;

            bool bDontPromptTimestampMismatchWhenOverwrite = false;
            DbNameMap map = new DbNameMap();

            ResPath default_target_respath = new ResPath(selectedNode);
            RmsChannel cur_channel = Channels.CreateTempChannel(default_target_respath.Url);
            Debug.Assert(cur_channel != null, "Channels.GetChannel() 异常");

            var stop = PrepareStop($"正在导入数据 ...");  // + default_target_respath.FullPath);
            stop.OnStop -= new StopEventHandler(this.DoStop);   // 去掉缺省的回调函数
            stop.OnStop += (sender1, e1) =>
            {
                if (cur_channel != null)
                    cur_channel.Abort();
            };
            stop.Style = StopStyle.EnableHalfStop;  // API的间隙才让中断。避免获取结果集的中途，因为中断而导致 Session 失效，结果集丢失，进而无法 Retry 获取

            var total_length = GetTotalLength(filenames);
            ProgressEstimate estimate = new ProgressEstimate();
            estimate.SetRange(0, total_length);
            estimate.StartEstimate();

            stop.SetProgressRange(0, total_length);

            // 前面文件已经处理过的 bytes 数
            long processed_length = 0;
            try
            {
                for (int i = 0; i < filenames.Count; i++)
                {
                    string fileName = filenames[i];


                    ImportUtil import_util = new ImportUtil();

                    ImportUtil.delegate_ask func = null;
                    if (i > 0)
                        func = (filename) =>
                        {
                            return new ImportUtil.AskMarcFileInfoResult
                            {
                                Value = 0,
                                Syntax = syntax,
                                Encoding = encoding,
                            };
                        };

                    // 2024/6/4
                    // 导入时自动添加 997 字段
                    import_util.AddUniformKey = true;
                    nRet = import_util.Begin(this,
                        this.AppInfo,
                        fileName,   // dlg.FileName,
                        func,
                        out strError);
                    if (nRet == -1 || nRet == 1)
                        goto ERROR0;

                    // 记忆下来
                    syntax = import_util.MarcSyntax;
                    encoding = import_util.Encoding;

#if NO
            ResPath respath = new ResPath(this.SelectedNode);
            this.channel = Channels.GetChannel(respath.Url);
            Debug.Assert(channel != null, "Channels.GetChannel() 异常");
#endif
                    // 缺省的目标数据库路径

                    // 本文件总共处理记录数
                    long current_count = 0;

                    try // open import util
                    {

                        long lSaveOffs = -1;


                        stop.SetMessage($"正在导入文件 {fileName} ({i + 1}/{filenames.Count}) 中的数据 ...");

                        List<UploadRecord> records = new List<UploadRecord>();
                        int nBatchSize = 0;
                        for (int index = 0; ; index++)
                        {
                            // Application.DoEvents();	// 出让界面控制权

                            if (stop.State != 0)
                            {
                                DialogResult result = (DialogResult)this.Invoke(new Func<DialogResult>(() =>
                                {
                                    return MessageBox.Show(this,
                                    "确实要中断当前批处理操作?",
                                    "导入数据",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question,
                                    MessageBoxDefaultButton.Button2);
                                }));
                                if (result == DialogResult.Yes)
                                {
                                    strError = "用户中断";
                                    goto ERROR1;
                                }
                                else
                                {
                                    stop.Continue();
                                }
                            }

                            if (import_util.FileType == ExportFileType.BackupFile)
                            {
                                if (lSaveOffs != -1 && import_util.Stream.Position != lSaveOffs)
                                {
                                    // import_util.Stream.Seek(lSaveOffs, SeekOrigin.Begin);
                                    StreamUtil.FastSeek(import_util.Stream, lSaveOffs);
                                }
                            }

                            nRet = import_util.ReadOneRecord(out UploadRecord record,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                            if (nRet == 1)
                                break;

                            current_count++;

                            if (import_util.FileType == ExportFileType.BackupFile)
                            {
                                // 保存每次读取后的文件指针位置
                                lSaveOffs = import_util.Stream.Position;
                            }

                            Debug.Assert(record != null, "");

                            // 准备目标路径
                            {
                                string strLongPath = record.Url + "?" + record.RecordBody.Path;

                                // 根据原始路径准备即将写入的路径
                                // return:
                                //      -1  出错
                                //      0   用户放弃
                                //      1   成功
                                //      2   要跳过本条
                                nRet = ImportUtil.PrepareOverwritePath(
                                    this,
                                    this.Servers,
                                    this.Channels,
                                    this.AppInfo,
                                    index,
                                    default_target_respath.FullPath,
                                    ref map,
                                    ref strLongPath,
                                    out strError);
                                if (nRet == 0 || nRet == -1)
                                    goto ERROR1;
                                if (nRet == 2)
                                    continue;

                                ResPath respath = new ResPath(strLongPath);
                                record.Url = respath.Url;
                                record.RecordBody.Path = respath.Path;

                                // 记载每个数据库的 URL
                                string strDbUrl = GetDbUrl(strLongPath);
                                if (target_dburls.IndexOf(strDbUrl) == -1)
                                {
                                    // 每个数据库要进行一次快速模式的准备操作
                                    if (bFastMode == true)
                                    {
                                        nRet = ManageKeysIndex(strDbUrl,
                                            "beginfastappend",
                                            "正在对数据库 " + strDbUrl + " 进行快速导入模式的准备工作 ...",
                                            out strError);
                                        if (nRet == -1)
                                            goto ERROR1;
                                    }
                                    target_dburls.Add(strDbUrl);
                                }
                            }

                            bool bNeedPush = false;
                            // 是否要把积累的记录推送出去进行写入?
                            // 要进行以下检查：
                            // 1) 当前记录和前一条记录之间，更换了服务器
                            // 2) 累积的记录尺寸超过要求
                            // 3) 当前记录是一条超大的记录 (这是因为要保持从文件中读出的顺序来写入(例如追加时候的号码增量顺序)，就必须在单条写入本条前，先写入积累的那些记录)
                            if (records.Count > 0)
                            {
                                if (record.TooLarge() == true)
                                    bNeedPush = true;
                                else if (nBatchSize + record.RecordBody.Xml.Length > CHUNK_SIZE)
                                    bNeedPush = true;
                                else
                                {
                                    if (LastUrl(records) != record.Url)
                                        bNeedPush = true;
                                }
                            }

                            if (bNeedPush == true)
                            {
                                // 准备 Channel
                                Debug.Assert(records.Count > 0, "");
                                cur_channel = ImportUtil.GetChannel(this.Channels,
                                    stop,
                                    LastUrl(records),
                                    cur_channel);

                                List<UploadRecord> save_records = new List<UploadRecord>();
                                save_records.AddRange(records);

                                if (importDataRecord)
                                {
                                    while (records.Count > 0)
                                    {
                                        // 将 XML 记录成批写入数据库
                                        // return:
                                        //      -1  出错
                                        //      >=0 本次已经写入的记录个数。本函数返回时 records 集合的元素数没有变化(但元素的Path和Timestamp会有变化)，如果必要调主可截取records集合中后面未处理的部分再次调用本函数
                                        nRet = ImportUtil.WriteRecords(
                                            this,
                                            stop,
                                            cur_channel,
                                            bFastMode,
                                            insertMissing,
                                            records,
                                            ref bDontPromptTimestampMismatchWhenOverwrite,
                                            out strError);
                                        if (nRet == -1)
                                            goto ERROR1;
                                        if (nRet == 0)
                                        {
                                            // TODO: 或可以改为单条写入
                                            strError = "WriteRecords() error :" + strError;
                                            goto ERROR1;
                                        }
                                        Debug.Assert(nRet <= records.Count, "");
                                        records.RemoveRange(0, nRet);
                                        lTotalCount += nRet;
                                    }
                                }
                                else
                                {
                                    lTotalCount += records.Count;
                                    records.Clear();
                                }

                                if (importObject)
                                {
                                    // 上载对象
                                    // return:
                                    //      -1  出错
                                    //      0   成功
                                    nRet = import_util.UploadObjects(
                                        stop,
                                        cur_channel,
                                        save_records,
                                        true,
                                        ref bDontPromptTimestampMismatchWhenOverwrite,
                                        out strError);
                                    if (nRet == -1)
                                        goto ERROR1;
                                }

                                nBatchSize = 0;
                                stop.SetProgressValue(import_util.Stream.Position + processed_length);

                                stop.SetMessage($"{fileName} ({i + 1}/{filenames.Count}) 已经写入记录 "
                                    + lTotalCount.ToString() + " 条。"
                                    + "剩余时间 " + ProgressEstimate.Format(estimate.Estimate(import_util.Stream.Position + processed_length)) + " 已经过时间 " + ProgressEstimate.Format(estimate.delta_passed));
                            }

                            // 如果 记录的 XML 尺寸太大不便于成批上载，需要在单独直接上载
                            if (record.TooLarge() == true)
                            {
                                // 准备 Channel
                                // ResPath respath = new ResPath(record.RecordBody.Path);
                                cur_channel = ImportUtil.GetChannel(this.Channels,
                                    stop,
                                    record.Url,
                                    cur_channel);

                                if (importDataRecord)
                                {
                                    // 写入一条 XML 记录
                                    // return:
                                    //      -1  出错
                                    //      0   邀请中断整个处理
                                    //      1   成功
                                    //      2   跳过本条，继续处理后面的
                                    nRet = ImportUtil.WriteOneXmlRecord(
                                        this,
                                        stop,
                                        cur_channel,
                                        record,
                                        ref bDontPromptTimestampMismatchWhenOverwrite,
                                        out strError);
                                    if (nRet == -1)
                                        goto ERROR1;
                                    if (nRet == 0)
                                        goto ERROR1;
                                }

                                if (importObject)
                                {
                                    List<UploadRecord> temp = new List<UploadRecord>();
                                    temp.Add(record);
                                    // 上载对象
                                    // return:
                                    //      -1  出错
                                    //      0   成功
                                    nRet = import_util.UploadObjects(
                                        stop,
                                        cur_channel,
                                        temp,
                                        true,
                                        ref bDontPromptTimestampMismatchWhenOverwrite,
                                        out strError);
                                    if (nRet == -1)
                                        goto ERROR1;
                                }

                                lTotalCount += 1;
                                continue;
                            }

                            records.Add(record);
                            if (record.RecordBody != null && record.RecordBody.Xml != null)
                                nBatchSize += record.RecordBody.Xml.Length;
                        }

                        // 最后提交一次
                        if (records.Count > 0)
                        {
                            // 准备 Channel
                            Debug.Assert(records.Count > 0, "");
                            cur_channel = ImportUtil.GetChannel(this.Channels,
                                stop,
                                LastUrl(records),
                                cur_channel);

                            List<UploadRecord> save_records = new List<UploadRecord>();
                            save_records.AddRange(records);

                            if (importDataRecord)
                            {
                                while (records.Count > 0)
                                {
                                    // 将 XML 记录成批写入数据库
                                    // return:
                                    //      -1  出错
                                    //      >=0 本次已经写入的记录个数。本函数返回时 records 集合的元素数没有变化(但元素的Path和Timestamp会有变化)，如果必要调主可截取records集合中后面未处理的部分再次调用本函数
                                    nRet = ImportUtil.WriteRecords(
                                        this,
                                        stop,
                                        cur_channel,
                                        bFastMode,
                                        insertMissing,
                                        records,
                                        ref bDontPromptTimestampMismatchWhenOverwrite,
                                        out strError);
                                    if (nRet == -1)
                                        goto ERROR1;
                                    if (nRet == 0)
                                    {
                                        strError = "WriteRecords() error :" + strError;
                                        goto ERROR1;
                                    }
                                    Debug.Assert(nRet <= records.Count, "");
                                    records.RemoveRange(0, nRet);
                                    lTotalCount += nRet;
                                }
                            }
                            else
                            {
                                lTotalCount += records.Count;
                                records.Clear();
                            }

                            if (importObject)
                            {
                                // 上载对象
                                // return:
                                //      -1  出错
                                //      0   成功
                                nRet = import_util.UploadObjects(
                                    stop,
                                    cur_channel,
                                    save_records,
                                    true,
                                    ref bDontPromptTimestampMismatchWhenOverwrite,
                                    out strError);
                                if (nRet == -1)
                                    goto ERROR1;
                            }

                            nBatchSize = 0;
                            stop.SetProgressValue(import_util.Stream.Position + processed_length);

                            stop.SetMessage($"{fileName} ({i + 1}/{filenames.Count}) 已经写入记录 "
                                + lTotalCount.ToString() + " 条。"
                                + "剩余时间 " + ProgressEstimate.Format(estimate.Estimate(import_util.Stream.Position + processed_length)) + " 已经过时间 " + ProgressEstimate.Format(estimate.delta_passed));

                            records.Clear();
                            nBatchSize = 0;
                        }

                        processed_length += import_util.Stream.Length;
                        times.Add($"{fileName}: 记录数 {current_count}");

                    }// close import util
                    catch (Exception ex)
                    {
                        strError = $"导入数据过程出现异常: {ExceptionUtil.GetExceptionText(ex)}";
                        goto ERROR1;
                    }
                    finally
                    {


#if NO
                if (stopManager != null)
                {
                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");

                    stop.Unregister();	// 和容器脱离关联
                }
#endif

                        import_util.End();
                    }

                }
            }
            finally
            {
                if (bFastMode == true)
                {
                    EndFastAppend(null, target_dburls);
#if NO
                    foreach (string url in target_dburls)
                    {
                        nRet = ManageKeysIndex(url,
                            "endfastappend",
                            "正在对数据库 " + url + " 进行快速导入模式的收尾工作，请耐心等待 ...",
                            out string strQuickModeError);
                        if (nRet == -1)
                            MessageBoxShow(strQuickModeError);
                    }
#endif
                }

                EndStop(stop);
                cur_channel.Close();
                cur_channel = null;
            }

            // TODO: 对话框可能超高
            this.TryInvoke(() =>
            {
                string strTimeMessage = "总共耗费时间: " + estimate.GetTotalTime().ToString();
                MessageDlg.Show(this,
                    "文件 " + StringUtil.MakePathList(filenames) + " 内的数据已经成功导入下列数据库:\r\n\r\n" + StringUtil.MakePathList(target_dburls, "\r\n") + "\r\n\r\n共导入记录 " + lTotalCount.ToString() + " 条。\r\n\r\n" + strTimeMessage,
                    "导入成功");
            });
            return 0;
        ERROR0:
            this.TryInvoke(() =>
            {
                // TODO: 内容太高时候要改进对话框显示方式
                MessageDlg.Show(this, strError, "错误");
            });
            return -1;
        ERROR1:
            this.TryInvoke(() =>
            {
                MessageDlg.Show(this, strError, "错误");
                // 使用了 lTotalCount 和 estimate 以后的报错
                if (lTotalCount > 0)
                {
                    string strTimeMessage = "耗费时间: " + estimate.GetTotalTime().ToString();
                    MessageDlg.Show(this,
                        "文件 " + StringUtil.MakePathList(filenames) + " 内的部分数据已经成功导入下列数据库:\r\n\r\n" + StringUtil.MakePathList(target_dburls, "\r\n") + "\r\n\r\n共导入记录 " + lTotalCount.ToString() + " 条。\r\n\r\n" + strTimeMessage,
                        "信息");
                }
            });

            return -1;
        }

        // TODO: 根据 dp2kernel 版本，决定是否用 start_endfastappend 方法
        void EndFastAppend(Stop stop,
    List<string> target_dburls)
        {
            var bar = this.TryGet(() => {
                var result = MessageBar.Create(this, "快速导入收尾");
                result.Font = this.Font;
                return result;
            });
            try
            {
                int nRet = 0;
                foreach (string url in target_dburls)
                {
                    if (stop != null
                        && stop.State != 0)
                        throw new Exception("快速导入收尾阶段被强行中断，恢复没有完成");

                    if (stop != null)
                        stop.SetMessage("正在对数据库 " + url + " 进行快速导入模式的最后收尾工作，请耐心等待 ...");

                    this.TryInvoke(() =>
                    {
                        bar.SetMessageText($"正在对数据库 {url} 进行收尾工作，请耐心等待 ...");
                    });

                    //LibraryChannelManager.Log?.Debug($"开始对数据库{url}进行快速导入模式的最后收尾工作");
                    try
                    {
                        nRet = ManageKeysIndex(url,
                            "start_endfastappend",
                            "正在对数据库 " + url + " 启动快速导入模式的收尾工作，请耐心等待 ...",
        out string strQuickModeError);
                        //if (nRet == -1)
                        //    MessageBoxShow(strQuickModeError);
                        if (nRet == -1)
                            throw new Exception(strQuickModeError);
                        else if (nRet == 1)
                        {
                            while (true)
                            {
                                if (stop != null
                                    && stop.State != 0)
                                    throw new Exception("快速导入收尾阶段被强行中断，恢复没有完成");

                                //                  detect_endfastappend 探寻任务的状态。返回 0 表示任务尚未结束; 1 表示任务已经结束
                                nRet = ManageKeysIndex(
            url,
            "detect_endfastappend",
            "正在对数据库 " + url + " 进行快速导入模式的收尾工作，请耐心等待 ...",
            out strQuickModeError);
                                if (nRet == -1)
                                    throw new Exception(strQuickModeError);
                                if (nRet == 1)
                                    break;
                            }
                        }
                        else if (nRet == 2)
                        {
                            //      2   本次已经减少计数 1，但依然不够，还剩下一定的计数当重新请求直到计数为零才会自动启动“收尾快速导入”任务
                            throw new Exception(strQuickModeError);
                            // TODO: 也可以尝试再次调用 ManageKeysIndex(url, "start_endfastappend",
                        }
                    }
                    catch (Exception ex)
                    {
                        //LibraryChannelManager.Log?.Debug($"对数据库{url}进行快速导入模式的最后收尾工作阶段出现异常: {ExceptionUtil.GetExceptionText(ex)}\r\n(其后的 URL 没有被收尾)全部数据库 URL:{StringUtil.MakePathList(target_dburls, "; ")}");
                        // throw new Exception($"对数据库 {url} 进行收尾时候出现异常。\r\n(其后的 URL 没有被收尾)全部数据库 URL:{StringUtil.MakePathList(target_dburls, "; ")}", ex);
                        MessageBoxShow($"对数据库 {url} 进行收尾时候出现异常。\r\n(其后的 URL 没有被收尾)全部数据库 URL:{StringUtil.MakePathList(target_dburls, "; ")}。\r\n异常信息{ExceptionUtil.GetExceptionText(ex)}");
                    }
                    finally
                    {
                        //LibraryChannelManager.Log?.Debug($"结束对数据库{url}进行快速导入模式的最后收尾工作");
                    }
                }
                if (stop != null)
                    stop.SetMessage("");
            }
            finally
            {
                this.TryInvoke(() => {
                    bar.Close();
                });
            }
        }

        static long GetTotalLength(List<string> filenames)
        {
            long length = 0;
            foreach (var filename in filenames)
            {
                if (File.Exists(filename) == false)
                    continue;
                FileInfo fi = new FileInfo(filename);
                length += fi.Length;
            }

            return length;
        }


        public void MessageBoxShow(string strText)
        {
            if (this.IsHandleCreated)
                this.Invoke((Action)(() =>
                {
                    try
                    {
                        MessageBox.Show(this, strText);
                    }
                    catch (ObjectDisposedException)
                    {

                    }
                }));
        }

#if NO
        // 集合中最后一个元素的 LongPath
        static string LastLongPath(List<UploadRecord> records)
        {
            Debug.Assert(records.Count > 0, "");
            UploadRecord last_record = records[records.Count - 1];
            if (last_record.RecordBody == null)
                return null;
            return last_record.RecordBody.Path;
        }
#endif
        // 集合中最后一个元素的 Url
        static string LastUrl(List<UploadRecord> records)
        {
            Debug.Assert(records.Count > 0, "");
            UploadRecord last_record = records[records.Count - 1];
            return last_record.Url;
        }

        static string GetDbUrl(string strLongPath)
        {
            ResPath respath = new ResPath(strLongPath);
            respath.MakeDbName();
            return respath.FullPath;
        }

        // 后台任务管理
        void menu_batchTask(object sender, System.EventArgs e)
        {
            string strError = "";

            if (this.SelectedNode == null)
            {
                strError = "尚未选择要要观察其后台任务的服务器节点";
                goto ERROR1;
            }
            /*
            if (this.SelectedNode.ImageIndex != RESTYPE_DB)
            {
                strError = "所选择的节点不是数据库类型。请选择要导出数据的数据库节点。";
                goto ERROR1;
            }
             * */

            ResPath respath = new ResPath(this.SelectedNode);
            RmsChannel cur_channel = Channels.CreateTempChannel(respath.Url);
            Debug.Assert(cur_channel != null, "Channels.GetChannel() 异常");
            DigitalPlatform.Stop stop = PrepareStop("正在导出数据 " + respath.FullPath);

            stop.OnStop -= new StopEventHandler(this.DoStop);   // 去掉缺省的回调函数
            stop.OnStop += (sender1, e1) =>
            {
                if (cur_channel != null)
                    cur_channel.Abort();
            };

            try
            {
                BatchTaskForm dlg = new BatchTaskForm();

                dlg.Channel = cur_channel;
                dlg.Stop = stop;
                dlg.ShowDialog(this);

            }
            finally
            {
                EndStop(stop);

                cur_channel.Close();
                cur_channel = null;
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 导出数据
        void menu_export(object sender, System.EventArgs e)
        {
            string strError = "";

            string strTimeMessage = "";
            long lTotalCount = 0;	// 总命中数
            long lExportCount = 0;	// 已经传出的数量

            if (this.SelectedNode == null)
            {
                strError = "尚未选择要要导出数据的数据库节点";
                goto ERROR1;
            }

            List<string> paths = null;
            if (this.CheckBoxes == false)
            {
                if (this.SelectedNode.ImageIndex != RESTYPE_DB)
                {
                    strError = "所选择的节点不是数据库类型。请选择要导出数据的数据库节点。";
                    goto ERROR1;
                }
                ResPath respath = new ResPath(this.SelectedNode);
                paths = new List<string>
                {
                    respath.FullPath   // respath.Path;
                };
            }
            else
            {
                paths = GetCheckedDatabaseList();
                if (paths.Count == 0)
                {
                    strError = "请选择至少一个要导出数据的数据库节点。";
                    goto ERROR1;
                }
            }

            // 询问导出数据的范围
            ExportDataDialog data_range_dlg = new ExportDataDialog();
            data_range_dlg.DbPath = StringUtil.MakePathList(paths); ;
            data_range_dlg.AllRecords = true;
            if (paths.Count > 1)
                data_range_dlg.StartEndEnabled = false;
            data_range_dlg.StartPosition = FormStartPosition.CenterScreen;
            data_range_dlg.ShowDialog(this);

            if (data_range_dlg.DialogResult != DialogResult.OK)
                return;

            string strRange = "0-9999999999";
            if (data_range_dlg.AllRecords == false)
                strRange = data_range_dlg.StartID + "-" + data_range_dlg.EndID;

            // 获得输出文件名
            SaveFileDialog dlg = new SaveFileDialog();

            dlg.Title = "请指定要保存的数据文件名";
            dlg.CreatePrompt = false;
            dlg.OverwritePrompt = false;
            dlg.FileName = "";
            dlg.FilterIndex = 1;

            dlg.Filter = "备份文件 (*.dp2bak)|*.dp2bak|XML文件 (*.xml)|*.xml|ISO2709文件 (*.iso;*.mrc)|*.iso;*.mrc|All files (*.*)|*.*";

            dlg.RestoreDirectory = true;

            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;

            ExportUtil export_util = new ExportUtil();

            int nRet = export_util.Begin(this,
dlg.FileName,
data_range_dlg.OutputEncoding,
out strError);
            if (nRet == -1)
                goto ERROR1;

            RmsChannel cur_channel = null;
            DigitalPlatform.Stop stop = PrepareStop("正在导出数据");

            stop.OnStop -= new StopEventHandler(this.DoStop);   // 去掉缺省的回调函数
            stop.OnStop += (sender1, e1) =>
            {
                if (cur_channel != null)
                    cur_channel.Abort();
            };
            ProgressEstimate estimate = new ProgressEstimate();

            try
            {
                int i_path = 0;
                foreach (string path in paths)
                {
                    ResPath respath = new ResPath(path);

                    string strQueryXml = "<target list='" + respath.Path
            + ":" + "__id'><item><word>" + strRange + "</word><match>exact</match><relation>range</relation><dataType>number</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";

                    cur_channel = Channels.CreateTempChannel(respath.Url);
                    Debug.Assert(cur_channel != null, "Channels.GetChannel() 异常");

                    try
                    {
                        long lRet = cur_channel.DoSearch(strQueryXml,
            "default",
            out strError);
                        if (lRet == -1)
                        {
                            strError = "检索数据库 '" + respath.Path + "' 时出错: " + strError;
                            goto ERROR1;
                        }

                        if (lRet == 0)
                        {
                            strError = "数据库 '" + respath.Path + "' 中没有任何数据记录";
                            // goto ERROR1;	// not found
                            continue;
                        }

                        stop.Style = StopStyle.EnableHalfStop;  // API的间隙才让中断。避免获取结果集的中途，因为中断而导致 Session 失效，结果集丢失，进而无法 Retry 获取

                        lTotalCount += lRet;	// 总命中数
                        long lRestCount = lRet; // 余下的数量
                        long lResultSetCount = lRet;    // 本次结果集内的总数
                        long lStart = 0;

                        estimate.SetRange(0, lTotalCount);
                        if (i_path == 0)
                            estimate.StartEstimate();

                        stop.SetProgressRange(0, lTotalCount);

                        DialogResult last_one_result = DialogResult.Yes;    // 前一次对话框选择的方式
                        bool bDontAskOne = false;
                        int nRedoOneCount = 0;
                        DialogResult last_get_result = DialogResult.Retry;    // 前一次对话框选择的方式
                        bool bDontAskGet = false;
                        int nRedoGetCount = 0;

                        for (; ; )
                        {
                            Application.DoEvents();	// 出让界面控制权

                            if (stop.State != 0)
                            {
                                DialogResult result = MessageBox.Show(this,
                                    "确实要中断当前批处理操作?",
                                    "导出数据",
                                    MessageBoxButtons.YesNo,
                                    MessageBoxIcon.Question,
                                    MessageBoxDefaultButton.Button2);
                                if (result == DialogResult.Yes)
                                {
                                    strError = "用户中断";
                                    goto ERROR1;
                                }
                                else
                                {
                                    stop.Continue();
                                }
                            }

                            string strStyle = "id,xml,timestamp";
                            if (export_util.FileType == ExportFileType.BackupFile)
                                strStyle = "id,xml,timestamp,metadata";

                            nRedoGetCount = 0;
                        REDO_GET:
                            Record[] searchresults = null;
                            lRet = cur_channel.DoGetSearchResult(
                                "default",
                                lStart,
                                lRestCount,
                                strStyle,
                                this.Lang,
                                stop,
                                out searchresults,
                                out strError);
                            if (lRet == -1)
                            {
                                if (stop.State != 0)    // 已经中断
                                    goto ERROR1;

                                // 自动重试有次数限制，避免进入死循环
                                if (bDontAskGet == true && last_get_result == DialogResult.Retry
                                    && nRedoGetCount < 3)
                                {
                                    nRedoGetCount++;
                                    goto REDO_GET;
                                }

                                DialogResult result = MessageDlg.Show(this,
            "获取检索结果时 (偏移量 " + lStart + ") 出错：\r\n---\r\n"
            + strError + "\r\n---\r\n\r\n是否重试获取操作?\r\n\r\n注：\r\n[重试] 重新获取\r\n[中断] 中断整个批处理",
            "导出数据",
            MessageBoxButtons.RetryCancel,
            MessageBoxDefaultButton.Button1,
            ref bDontAskOne,
            new string[] { "重试", "中断" });
                                last_get_result = result;

                                if (result == DialogResult.Retry)
                                {
                                    nRedoGetCount = 0;
                                    goto REDO_GET;
                                }

                                Debug.Assert(result == DialogResult.Cancel, "");

                                strError = "获取检索结果时出错: " + strError;
                                goto ERROR1;
                            }

                            // TODO: 要判断 searchresults.Length == 0 跳出循环。否则容易进入死循环

                            for (int i = 0; i < searchresults.Length; i++)
                            {
                                Application.DoEvents();

                                Record record = searchresults[i];

                                if (i == 0)
                                {
                                    stop.SetMessage("正在输出记录 " + record.Path + "，已输出 " + lExportCount.ToString() + " 条。"
                + "剩余时间 " + ProgressEstimate.Format(estimate.Estimate(lExportCount)) + " 已经过时间 " + ProgressEstimate.Format(estimate.delta_passed));
                                }
                                nRedoOneCount = 0;

                                // 2017/5/18
                                if (record.RecordBody == null
                                    || string.IsNullOrEmpty(record.RecordBody.Xml))
                                    continue;

                                REDO_ONE:
                                nRet = export_util.ExportOneRecord(
                                    cur_channel,
                                    stop,
                                    respath.Url,
                                    record.Path,
                                    record.RecordBody.Xml,
                                    record.RecordBody.Metadata,
                                    record.RecordBody.Timestamp,
                                    out strError);
                                if (nRet == -1)
                                {
                                    if (stop.State != 0)    // 已经中断
                                        goto ERROR1;

                                    // 重试、跳过、中断?
                                    // 重试的时候，注意保持文件最后位置，不要留下残余的尾部
                                    // MessageBoxButtons.AbortRetryIgnore  YesNoCancel
                                    if (bDontAskOne == true && last_one_result == DialogResult.No)
                                        continue;   // TODO: 最好在日志文件中记载跳过的记录。或者批处理结束后显示出来

                                    // 自动重试有次数限制，避免进入死循环
                                    if (bDontAskOne == true && last_one_result == DialogResult.Yes
                                        && nRedoOneCount < 3)
                                    {
                                        nRedoOneCount++;
                                        goto REDO_ONE;
                                    }

                                    DialogResult result = MessageDlg.Show(this,
                                        "导出记录 '" + record.Path + "' 时出错：\r\n---\r\n"
                                        + strError + "\r\n---\r\n\r\n是否重试导出操作?\r\n\r\n注：\r\n[重试] 重新导出这条记录\r\n[跳过] 忽略导出这条记录，但继续后面的处理\r\n[中断] 中断整个批处理",
                                        "导出数据",
                                        MessageBoxButtons.YesNoCancel,
                                        MessageBoxDefaultButton.Button1,
                                        ref bDontAskOne,
                                        new string[] { "重试", "跳过", "中断" });
                                    last_one_result = result;

                                    if (result == DialogResult.Yes)
                                    {
                                        nRedoOneCount = 0;
                                        goto REDO_ONE;
                                    }

                                    if (result == DialogResult.No)
                                        continue;

                                    Debug.Assert(result == DialogResult.Cancel, "");

                                    goto ERROR1;
                                }

                                stop.SetProgressValue(lExportCount + 1);
                                lExportCount++;
                            }

                            if (lStart + searchresults.Length >= lResultSetCount)
                                break;

                            lStart += searchresults.Length;
                            lRestCount -= searchresults.Length;
                        }

                        strTimeMessage = "总共耗费时间: " + estimate.GetTotalTime().ToString();
                    }
                    finally
                    {
                        cur_channel.Close();
                        cur_channel = null;
                    }
                    // MessageBox.Show(this, "位于服务器 '" + respath.Url + "' 上的数据库 '" + respath.Path + "' 内共有记录 " + lTotalCount.ToString() + " 条，本次导出 " + lExportCount.ToString() + " 条。" + strTimeMessage);

                    i_path++;
                }
            }
            finally
            {
                export_util.End();

                EndStop(stop);
            }
            MessageBox.Show(this, "数据库 '" + StringUtil.MakePathList(paths) + "' 内共有记录 " + lTotalCount.ToString() + " 条，本次导出 " + lExportCount.ToString() + " 条。" + strTimeMessage);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
            if (lExportCount > 0)
                MessageBox.Show(this, "数据库内共有记录 " + lTotalCount.ToString() + " 条，本次导出 " + lExportCount.ToString() + " 条");
        }

        public DigitalPlatform.Stop PrepareStop(string strText)
        {
            if (stopManager == null)
                return null;

            DigitalPlatform.Stop stop = new DigitalPlatform.Stop();

            stop.Register(this.stopManager, true);	// 和容器关联

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial(strText);
            stop.BeginLoop();

            return stop;
        }

        public void EndStop(DigitalPlatform.Stop stop)
        {
            if (stopManager == null || stop == null)
                return;

            stop.EndLoop();
            stop.OnStop -= new StopEventHandler(this.DoStop);
            stop.Initial("");

            stop.Unregister();	// 和容器关联
        }


        void menu_changePassword(object sender, System.EventArgs e)
        {
            if (this.SelectedNode == null)
            {
                MessageBox.Show(this, "尚未选择服务器 ...");
                return;
            }
            ChangePasswordDlg dlg = new ChangePasswordDlg();
            dlg.Font = GuiUtil.GetDefaultFont();
            ResPath respath = new ResPath(this.SelectedNode);

            dlg.Channels = this.Channels;
            dlg.Url = respath.Url;
            if (Servers != null)
            {
                Server server = Servers[respath.Url];
                if (server != null)
                    dlg.UserName = server.DefaultUserName;
            }
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);
        }

        // 编辑配置文件
        void menu_editCfgFile(object sender, System.EventArgs e)
        {
            if (this.SelectedNode == null)
            {
                MessageBox.Show(this, "尚未选择要编辑的配置文件节点");
                return;
            }

            if (this.SelectedNode.ImageIndex != RESTYPE_FILE)
            {
                MessageBox.Show(this, "所选择的节点不是配置文件类型。请选择要编辑的配置文件节点。");
                return;
            }

            ResPath respath = new ResPath(this.SelectedNode);

            // 编辑配置文件
            CfgFileEditDlg dlg = new CfgFileEditDlg();
            dlg.Font = GuiUtil.GetDefaultFont();

            dlg.Initial(this.Servers,
                this.Channels,
                this.stopManager,
                respath.Url,
                respath.Path);

            if (this.AppInfo != null)
                this.AppInfo.LinkFormState(dlg, "CfgFileEditDlg_state");
            dlg.ShowDialog(this);
#if NO
			if (this.AppInfo != null)
				this.AppInfo.UnlinkFormState(dlg);
#endif

            /*
			if (dlg.DialogResult != DialogResult.OK)
				goto FINISH;
			*/




        }

        // 初始化数据库
        void menu_initialDB(object sender, System.EventArgs e)
        {
            if (this.SelectedNode == null)
            {
                MessageBox.Show(this, "尚未选择要初始化的数据库节点");
                return;
            }

            if (this.SelectedNode.ImageIndex != RESTYPE_DB)
            {
                MessageBox.Show(this, "所选择的节点不是数据库类型。请选择要初始化的数据库节点。");
                return;
            }

            ResPath respath = new ResPath(this.SelectedNode);

            string strText = "你确实要初始化位于服务器 '" + respath.Url + "' 上的数据库 '" + respath.Path + "' 吗?\r\n\r\n警告：数据库一旦被初始化，其中包含的原有数据将全部被摧毁，并且无法恢复！";

            DialogResult msgResult = MessageBox.Show(this,
                strText,
                "初始化数据库",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (msgResult != DialogResult.OK)
            {
                MessageBox.Show(this, "初始化数据库操作被放弃...");
                return;
            }


            this.channel = Channels.GetChannel(respath.Url);

            Debug.Assert(channel != null, "Channels.GetChannel() 异常");


            string strError = "";

#if NO
			DigitalPlatform.Stop stop = null;

			if (stopManager != null) 
			{
				stop = new DigitalPlatform.Stop();

                stop.Register(this.stopManager, true);	// 和容器关联

                stop.OnStop += new StopEventHandler(this.DoStop);
				stop.Initial("正在初始化数据库: " + respath.FullPath);
				stop.BeginLoop();

			}
#endif
            DigitalPlatform.Stop stop = PrepareStop("正在初始化数据库: " + respath.FullPath);

            long lRet = channel.DoInitialDB(respath.Path, out strError);

            EndStop(stop);
#if NO
			if (stopManager != null) 
			{
				stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
				stop.Initial("");

				stop.Unregister();	// 和容器脱离关联
			}
#endif

            this.channel = null;

            if (lRet == -1)
            {
                MessageBox.Show(this, strError);
            }
            else
            {
                MessageBox.Show(this, "位于服务器'" + respath.Url + "'上的数据库 '" + respath.Path + "' 被成功初始化。");
            }
        }

        // 刷新数据库定义(不清除原有keys)
        void menu_refreshDB(object sender, System.EventArgs e)
        {
            RefreshDB(false);
        }

        void RefreshDB(bool bClearAllKeyTables)
        {
            if (this.SelectedNode == null)
            {
                MessageBox.Show(this, "尚未选择要刷新定义的数据库节点");
                return;
            }

            if (this.SelectedNode.ImageIndex != RESTYPE_DB)
            {
                MessageBox.Show(this, "所选择的节点不是数据库类型。请选择要刷新定义的数据库节点。");
                return;
            }

            ResPath respath = new ResPath(this.SelectedNode);

            string strText = "确实要刷新位于服务器 '" + respath.Url + "' 上的数据库 '" + respath.Path + "' 的定义吗?\r\n\r\n注：刷新数据库定义，会为数据库增补在keys配置文件中新增的SQL表，不会损坏数据库中已有的数据。";

            DialogResult msgResult = MessageBox.Show(this,
                strText,
                "刷新数据库定义",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);

            if (msgResult != DialogResult.OK)
            {
                MessageBox.Show(this, "刷新数据库定义的操作被放弃...");
                return;
            }


            this.channel = Channels.GetChannel(respath.Url);

            Debug.Assert(channel != null, "Channels.GetChannel() 异常");


            string strError = "";

#if NO
            DigitalPlatform.Stop stop = null;

            if (stopManager != null)
            {
                stop = new DigitalPlatform.Stop();

                stop.Register(this.stopManager, true);	// 和容器关联

                stop.OnStop += new StopEventHandler(this.DoStop);
                stop.Initial("正在刷新数据库定义: " + respath.FullPath );
                stop.BeginLoop();

            }
#endif
            DigitalPlatform.Stop stop = PrepareStop("正在刷新数据库定义: " + respath.FullPath);


            long lRet = channel.DoRefreshDB(
                "begin",
                respath.Path,
                bClearAllKeyTables,
                out strError);

            EndStop(stop);
#if NO
            if (stopManager != null)
            {
                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");

                stop.Unregister();	// 和容器脱离关联
            }
#endif

            this.channel = null;

            if (lRet == -1)
            {
                MessageBox.Show(this, strError);
            }
            else
            {
                MessageBox.Show(this, "位于服务器'" + respath.Url + "'上的数据库 '" + respath.Path + "' 被成功刷新了定义。");
            }
        }


        void menu_nodeInfo(object sender, System.EventArgs e)
        {
            if (this.SelectedNode == null)
            {
                MessageBox.Show(this, "尚未选择节点");
                return;
            }

            string strText = "";

            ItemProperty prop = (ItemProperty)this.SelectedNode.Tag;
            if (prop != null)
            {
                string strTypeString = prop.TypeString;
                strText = "节点名: " + this.SelectedNode.Text + "\r\n";
                if (String.IsNullOrEmpty(strTypeString) == false)
                    strText += "类型: " + strTypeString;
                else
                    strText += "类型: " + "(无)";
            }
            else
                strText = "ItemProperty == null";

            MessageBox.Show(this, strText);
        }

        void menu_clearCheckBoxes(object sender, System.EventArgs e)
        {
            this.ClearChildrenCheck(null);
        }

        void menu_toggleCheckBoxes(object sender, System.EventArgs e)
        {
            if (this.CheckBoxes == true)
                this.CheckBoxes = false;
            else
                this.CheckBoxes = true;
        }

        delegate int Delegate_Fill(TreeNode node);

        private void ResTree_AfterExpand(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {

            TreeNode node = e.Node;

            if (node == null)
                return;

            // 需要展开
            if (IsLoading(node) == true)
            {
                //Fill(node);

                object[] pList = new object[] { node };

                this.BeginInvoke(new Delegate_Fill(this.Fill), pList);

            }

        }




        // 根据路径,设置一个node的checked状态
        public bool CheckNode(ResPath respath,
            bool bChecked)
        {
            TreeNode node = this.GetTreeNode(respath);
            if (node == null)
                return false;

            node.Checked = bChecked;
            return true;
        }

        // 根据路径得到node节点对象
        public TreeNode GetTreeNode(ResPath respath)
        {

            string[] aName = respath.Path.Split(new Char[] { '/' });

            TreeNode node = null;
            TreeNode nodeThis = null;


            string[] temp = new string[aName.Length + 1];
            Array.Copy(aName, 0, temp, 1, aName.Length);
            temp[0] = respath.Url;

            aName = temp;

            for (int i = 0; i < aName.Length; i++)
            {
                TreeNodeCollection nodes = null;

                if (node == null)
                    nodes = this.Nodes;
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

        // 正规化绝对URI (指不包括query部分的左边部分)
        static string CanonicalizeAbsoluteUri(string s)
        {
            if (string.IsNullOrEmpty(s) == true)
                return s;

            s = s.ToLower();

            // 确保最后有一个'/'字符
            if (s[s.Length - 1] != '/')
                s += "/";

            return s;
        }

        // 2012/3/31
        // 判断两个URL是否等同
        static bool IsUrlEqual(string s1, string s2)
        {
            try
            {
                Uri uri1 = new Uri(s1);
                Uri uri2 = new Uri(s2);

                if (CanonicalizeAbsoluteUri(uri1.AbsoluteUri) != CanonicalizeAbsoluteUri(uri2.AbsoluteUri))
                    return false;
                if (uri1.Query != uri2.Query)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        // 根据路径逐步展开
        // return:
        //      true    最末一级找到
        //      false   没有找到(至少有一级没有找到)
        public bool ExpandPath(ResPath respath)
        {
            string[] aName = respath.Path.Split(new Char[] { '/' });

            TreeNode node = null;
            TreeNode nodeThis = null;

            string[] temp = new string[aName.Length + 1];
            Array.Copy(aName, 0, temp, 1, aName.Length);
            temp[0] = respath.Url;

            aName = temp;

            bool bBreak = false;    // 是否因某级没有找到在中间中断了?
            for (int i = 0; i < aName.Length; i++)
            {
                TreeNodeCollection nodes = null;

                if (node == null)
                    nodes = this.Nodes;
                else
                    nodes = node.Nodes;

                bool bFound = false;
                for (int j = 0; j < nodes.Count; j++)
                {
                    if (aName[i] == nodes[j].Text
                        || (i == 0 && IsUrlEqual(aName[i], nodes[j].Text) == true)) // URL部分需要特殊比较方法 2012/3/31
                    {
                        bFound = true;
                        nodeThis = nodes[j];
                        break;
                    }
                }
                if (bFound == false)
                {
                    bBreak = true;
                    break;
                }

                node = nodeThis;

                // 需要展开
                if (IsLoading(node) == true)
                {
                    Fill(node);
                }
                node.Expand();  // 2006/1/20     即便最终层次没有找到，也要展开中间层次
            }

            if (nodeThis != null && nodeThis.Parent != null)
                nodeThis.Parent.Expand();

            this.SelectedNode = nodeThis;

            // 2009/3/3
            if (bBreak == true)
                return false;

            return true;
        }


        // 根据路径逐步check下去。check不是检查的意思，而是勾选的意思
        // return:
        //      true    找到
        //      false   没有找到(至少有一级没有找到)
        public bool CheckPath(ResPath respath)
        {
            Debug.Assert(this.CheckBoxes == true, "只能在CheckBoxes==true的情况下调用");

            string[] aName = respath.Path.Split(new Char[] { '/' });
            TreeNode node = null;
            TreeNode nodeThis = null;

            string[] temp = new string[aName.Length + 1];
            Array.Copy(aName, 0, temp, 1, aName.Length);
            // 第一级是Url
            temp[0] = respath.Url;

            aName = temp;

            bool bBreak = false;    // 是否因某级没有找到在中间中断了?
            for (int i = 0; i < aName.Length; i++)
            {
                TreeNodeCollection nodes = null;

                if (node == null)
                    nodes = this.Nodes;
                else
                    nodes = node.Nodes;

                bool bFound = false;
                for (int j = 0; j < nodes.Count; j++)
                {
                    if (aName[i] == nodes[j].Text
                        || (i == 0 && IsUrlEqual(aName[i], nodes[j].Text) == true)) // URL部分需要特殊比较方法 2012/4/1
                    {
                        bFound = true;
                        nodeThis = nodes[j];
                        break;
                    }
                }
                if (bFound == false)
                {
                    bBreak = true;
                    break;
                }

                node = nodeThis;

                // 需要展开
                if (IsLoading(node) == true)
                {
                    Fill(node);
                }
                node.Expand();  // 2006/1/20     即便最终层次没有找到，也要展开中间层次
                node.Checked = true;
            }

            if (nodeThis != null && nodeThis.Parent != null)
                nodeThis.Parent.Expand();

            if (bBreak == true)
                return false;

            return true;
        }

        // 第一阶段。负责产生TargetItem的Url和Target内容
        // 根据树上的选择状态生成检索目标字符串
        // 不同的服务器中的字符串分开放
        public TargetItemCollection GetSearchTarget()
        {
            string strDb = "";

            TargetItemCollection aText = new TargetItemCollection();

            if (this.CheckBoxes == false)
            {
                ArrayList aNode = new ArrayList();
                TreeNode node = this.SelectedNode;
                if (node == null)
                    return aText;

                for (; node != null;)
                {
                    aNode.Insert(0, node);
                    node = node.Parent;
                }

                if (aNode.Count == 0)
                    goto END1;


                TargetItem item = new TargetItem();
                item.Lang = this.Lang;

                aText.Add(item);

                item.Url = ((TreeNode)aNode[0]).Text;

                if (aNode.Count == 1)
                    goto END1;

                item.Target = ((TreeNode)aNode[1]).Text;

                if (aNode.Count == 2)
                    goto END1;

                item.Target += ":" + ((TreeNode)aNode[2]).Text;

            END1:
                return aText;
            }

            // 找选中的服务器
            foreach (TreeNode nodeServer in this.Nodes)
            {
                if (nodeServer.Checked == false)
                    continue;

                // 找选中的数据库
                foreach (TreeNode nodeDb in nodeServer.Nodes)
                {
                    if (nodeDb.Checked == false)
                        continue;

                    if (nodeDb.ImageIndex != RESTYPE_DB)
                        continue;   // 2006/6/16 因为可能有配置文件目录或者文件对象需要跳过

                    if (strDb != "")
                        strDb += ";";
                    strDb += nodeDb.Text + ":";

                    //用一个strFrom新变量，可以很好地处理逗号
                    string strFrom = "";
                    //找选中的from
                    foreach (TreeNode nodeFrom in nodeDb.Nodes)
                    {
                        if (nodeFrom.Checked == true)
                        {
                            if (strFrom != "")
                                strFrom += ",";
                            strFrom += nodeFrom.Text;
                        }
                    }
                    strDb += strFrom;
                }

                TargetItem item = new TargetItem();
                item.Url = nodeServer.Text;
                item.Target = strDb;
                item.Lang = this.Lang;

                aText.Add(item);

                strDb = "";
            }

            return aText;
        }

        // 获得当前选中的一个或若干个数据库对象的全路径
        public List<string> GetCheckedDatabaseList()
        {
            List<string> result = new List<string>();

            if (this.CheckBoxes == false)
            {
                TreeNode node = this.SelectedNode;
                if (node == null)
                    return result;

                if (node.ImageIndex != RESTYPE_DB)
                    return result;

                ResPath respath = new ResPath(node);

                result.Add(respath.FullPath);
                return result;
            }

            // 找选中的服务器
            foreach (TreeNode nodeServer in this.Nodes)
            {
                if (nodeServer.Checked == false)
                    continue;

                // 找选中的数据库
                foreach (TreeNode nodeDb in nodeServer.Nodes)
                {
                    if (nodeDb.Checked == false)
                        continue;

                    if (nodeDb.ImageIndex != RESTYPE_DB)
                        continue;

                    ResPath respath = new ResPath(nodeDb);

                    result.Add(respath.FullPath);
                }
            }

            return result;
        }

        // 2008/11/17
        // 根据路径列表，勾选若干数据库
        // parameters:
        //      paths   路径的数组。每个路径的形态如: http://localhost/dp2kernel?数据库名
        // return:
        //      false   要选择的目标不存在
        //      true    已经选定
        public bool SelectDatabases(List<string> paths,
            out string strError)
        {
            strError = "";

            if (paths.Count == 0)
            {
                if (this.CheckBoxes == true)
                    ClearChildrenCheck(null);
                this.SelectedNode = null;
                strError = "paths数组中没有任何元素";
                return false;
            }

            if (paths.Count == 1)
            {
                this.CheckBoxes = false;
                this.SelectedNode = null;
                ResPath respath = new ResPath(paths[0]);

                // return:
                //      true    最末一级找到
                //      false   没有找到(至少有一级没有找到)
                bool bRet = ExpandPath(respath);
                if (this.SelectedNode == null)
                {
                    strError = "'" + paths[0] + "' 的服务器节点在资源树中没有找到";
                    return false;
                }

                if (bRet == false)
                {
                    strError = "'" + respath.FullPath + "' 的数据库节点在资源树中没有找到";
                    return false;
                }

                return true;
            }

            this.CheckBoxes = true;
            ClearChildrenCheck(null);
            this.SelectedNode = null;

            for (int i = 0; i < paths.Count; i++)
            {
                ResPath respath = new ResPath(paths[i]);

                bool bFound = CheckPath(respath);
                if (bFound == false)
                {
                    if (String.IsNullOrEmpty(strError) == false)
                        strError += "\r\n";
                    strError += "'" + respath.FullPath + "' 的某级在资源树中没有找到";
                }
            }

            if (String.IsNullOrEmpty(strError) == true)
                return true;

            return false;
        }

        private void ResTree_AfterCheck(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            TreeNode node = e.Node;
            if (node == null)
                return;

            // 2008/11/17
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

            if (node.Checked == false)
            {
                ClearOneLevelChildrenCheck(node);
            }
            else
            {
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

        // 2008/11/17
        // 清除下级所有的选中的项(不包括自己)
        // parameters:
        //      nodeStart   起点node。如果为null, 表示从根层开始，清除全部
        public void ClearChildrenCheck(TreeNode nodeStart)
        {
            TreeNodeCollection nodes = null;
            if (nodeStart == null)
            {
                nodes = this.Nodes;
            }
            else
                nodes = nodeStart.Nodes;

            foreach (TreeNode node in nodes)
            {
                node.Checked = false;
                ClearChildrenCheck(node);	// 递归
            }
        }

        private void ResTree_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {

            TreeNode curSelectedNode = this.GetNodeAt(e.X, e.Y);

            if (this.SelectedNode != curSelectedNode)
                this.SelectedNode = curSelectedNode;

        }

        /*
		// 循环，依次把父亲节点选中(不包括自己)
		public void CheckParent(TreeNode node)
		{
			node = node.Parent;
			while(true)
			{
				if (node == null)
					return;
				node.Checked = true;  //check事件会调这个函数，所以必须用父亲
				break;
				// node = node.Parent;
			}
		}
		*/

        public List<string> GetBrowseColumnNames(string strServerUrlOrName,
    string strDbName)
        {
            // 找到服务器节点
            TreeNode node_server = this.FindServer(strServerUrlOrName);
            if (node_server == null)
                return null;    // not found server

            string strError = "";
            // 需要展开
            if (IsLoading(node_server) == true)
            {
                int nRet = Fill(node_server, out strError);
                if (nRet == -1)
                {
                    throw new Exception(strError);
                }
            }

            // 找到数据库节点
            TreeNode node_db = FindDb(node_server, strDbName);
            if (node_db == null)
                return null;    // not found db

            DbProperty prop = (DbProperty)node_db.Tag;

            return prop.ColumnNames;
        }

        // 找到服务器节点
        TreeNode FindServer(string strServerUrlOrName)
        {
            for (int i = 0; i < this.Nodes.Count; i++)
            {
                TreeNode currrent_node = this.Nodes[i];

#if NO
                Debug.Assert(currrent_node.Tag is dp2ServerNodeInfo, "");
                dp2ServerNodeInfo info = (dp2ServerNodeInfo)currrent_node.Tag;

                if (info.Url == strServerUrlOrName
                    || info.Name == strServerUrlOrName)
                    return currrent_node;
#endif
                if (currrent_node.Text == strServerUrlOrName)
                    return currrent_node;
            }

            return null;
        }

        static TreeNode FindDb(TreeNode server_node, string strDbName)
        {
            Debug.Assert(server_node != null, "");
            for (int i = 0; i < server_node.Nodes.Count; i++)
            {
                TreeNode node = server_node.Nodes[i];
                if (node.Text == strDbName)
                    return node;
            }

            return null;
        }
    }

    public class ItemProperty
    {
        public string TypeString = "";
    }

    public class DbProperty : ItemProperty
    {
        public string DbName = "";
        public List<string> ColumnNames = new List<string>();
    }

    // 检索目标事项
    public class TargetItem
    {
        public string Lang = "";
        public string Url = "";
        public string Target = "";  // 检索目标字符串，例如"库1:from1,from2;库2:from1,from2"

        public string Words = "";   // 原始态的检索词,尚未切割
        public string[] aWord = null;   // MakeWordPhrases()加工后的字符串
        public string Xml = "";
        public int MaxCount = -1;   // 检索的最大条数
    }

    // 检索目标容器
    public class TargetItemCollection : ArrayList
    {

        // 第二阶段: 根据每个TargetItem中Words中原始形态的检索词，切割为string[] aWord
        // 调用本函数前，应当为每个TargetItem对象设置好Words成员值
        // 第二阶段和第一阶段先后顺序不重要。
        public int MakeWordPhrases(
            bool bSplitWords,
            bool bAutoDetectRange,
            bool bAutoDetectRelation)
        {
            for (int i = 0; i < this.Count; i++)
            {
                TargetItem item = (TargetItem)this[i];
                item.aWord = MakeWordPhrases(item.Words,
                    bSplitWords,
                    bAutoDetectRange,
                    bAutoDetectRelation);
            }

            return 0;
        }


        // 第三阶段：根据每个TargetItem中的Target和aWord，构造出Xml内容
        public int MakeXml()
        {
            string strText = "";
            for (int i = 0; i < this.Count; i++)
            {
                TargetItem item = (TargetItem)this[i];

                strText = "";

                string strCount = "";

                if (item.MaxCount != -1)
                    strCount = "<maxCount>" + Convert.ToString(item.MaxCount) + "</maxCount>";

                for (int j = 0; j < item.aWord.Length; j++)
                {
                    if (j != 0)
                    {
                        strText += "<operator value='OR' />";
                    }

                    strText += "<item>" + item.aWord[j] + strCount + "</item>";
                }

                strText = "<target list='"
                    + StringUtil.GetXmlStringSimple(item.Target)       // 2007/9/14
                    + "'>" + strText
                    + "<lang>" + item.Lang + "</lang></target>";

                item.Xml = strText;
            }

            return 0;
        }

        // 匹配左右括号
        static bool MatchTailQuote(char left, char right)
        {
            if (left == '“' && right == '”')
                return true;
            if (left == '‘' && right == '’')
                return true;

            if (left == '\'' && right == '\'')
                return true;

            if (left == '"' && right == '"')
                return true;

            return false;
        }

        // 按照空格切割出检索词
        static List<string> SplitWords(string strWords)
        {
            List<string> results = new List<string>();
            string strWord = "";
            bool bInQuote = false;
            char chQuote = '\'';
            for (int i = 0; i < strWords.Length; i++)
            {
                if ("\'\"“”‘’".IndexOf(strWords[i]) != -1)
                {
                    if (bInQuote == true
                        && MatchTailQuote(chQuote, strWords[i]) == true)
                    {
                        bInQuote = false;
                        continue;   // 在结果中忽略这个符号
                    }
                    else if (bInQuote == false)
                    {
                        bInQuote = true;
                        chQuote = strWords[i];
                        continue;   // 在结果中忽略这个符号
                    }
                }

                if ((strWords[i] == ' ' || strWords[i] == '　')
                    && bInQuote == false
                    && String.IsNullOrEmpty(strWord) == false)
                {
                    results.Add(strWord);
                    strWord = "";
                }
                else
                {
                    strWord += strWords[i];
                }
            }

            if (String.IsNullOrEmpty(strWord) == false)
            {
                results.Add(strWord);
                strWord = "";
            }


            return results;
        }

        // 根据一个检索词字符串，按照空白切割成单个检索词，
        // 并且根据检索词是否为数字等等猜测出其它检索参数，构造成
        // 含<item>内部分元素的字符串。调用者然后可增加<target>等元素，
        // 最终构成完整的<item>字符串
        public static string[] MakeWordPhrases(string strWords,
            bool bSplitWords,
            bool bAutoDetectRange,
            bool bAutoDetectRelation)
        {
            /*
			string[] aWord;
			aWord = strWords.Split(new Char [] {' '});
             */
            List<string> aWord = null;

            if (bSplitWords == true)
                aWord = SplitWords(strWords);

            if (aWord == null || aWord.Count == 0)
            {
                aWord = new List<string>();
                aWord.Add(strWords);
            }

            string strXml = "";
            string strWord = "";
            string strMatch = "";
            string strRelation = "";
            string strDataType = "";

            ArrayList aResult = new ArrayList();

            foreach (string strOneWord in aWord)
            {
                /*
				strRelation = "";
				strDataType = "";	
				strWord = "";
				strMatch = "";
				*/


                if (bAutoDetectRange == true)
                {
                    string strID1;
                    string strID2;

                    QueryClient.SplitRangeID(strOneWord, out strID1, out strID2);
                    if (StringUtil.IsNum(strID1) == true
                        && StringUtil.IsNum(strID2) && strOneWord != "")
                    {
                        strWord = strOneWord;
                        strMatch = "exact";
                        strRelation = "range";  // 2012/3/29
                        strDataType = "number";
                        goto CONTINUE;
                    }
                }


                if (bAutoDetectRelation == true)
                {
                    string strOperatorTemp;
                    string strRealText;

                    int ret;
                    ret = QueryClient.GetPartCondition(strOneWord,
                        out strOperatorTemp,
                        out strRealText);

                    if (ret == 0 && strOneWord != "")
                    {
                        strWord = strRealText;
                        strMatch = "exact";
                        strRelation = strOperatorTemp;
                        if (StringUtil.IsNum(strRealText) == true)
                            strDataType = "number";
                        else
                            strDataType = "string";
                        goto CONTINUE;
                    }
                }

                strWord = strOneWord;
                strMatch = "left";
                strRelation = "=";
                strDataType = "string";



            CONTINUE:

                // 2007/4/5 改造 加上了 GetXmlStringSimple()
                strXml += "<word>"
                    + StringUtil.GetXmlStringSimple(strWord)
                    + "</word>"
                    + "<match>" + strMatch + "</match>"
                    + "<relation>" + strRelation + "</relation>"
                    + "<dataType>" + strDataType + "</dataType>";

                aResult.Add(strXml);

                strXml = "";
            }

            return ConvertUtil.GetStringArray(0, aResult);
        }


    }

}
