using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using System.Deployment.Application;
using System.IO;
using System.Web;
using System.Threading;

using DigitalPlatform;
using DigitalPlatform.Text;
using DigitalPlatform.rms.Client;
using DigitalPlatform.Xml;
using DigitalPlatform.GUI;
using DigitalPlatform.CommonDialog;

namespace dp2Manager
{
    /// <summary>
    /// Summary description for MainForm.
    /// </summary>
    public class MainForm : System.Windows.Forms.Form
    {
        public string DataDir = "";

        public DigitalPlatform.StopManager stopManager = new DigitalPlatform.StopManager();
        DigitalPlatform.Stop stop = null;

        public ServerCollection Servers = null;

        public LinkInfoCollection LinkInfos = null;

        public string Lang = "zh";

        //保存界面信息
        public ApplicationInfo AppInfo = new ApplicationInfo("dp2managers.xml");

        RmsChannel channel = null;	// 临时使用的channel对象

        public RmsChannelCollection Channels = new RmsChannelCollection();	// 拥有

        private ResTree treeView_res;

        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem_accountManagement;
        private System.Windows.Forms.MenuItem menuItem_databaseManagement;
        private System.Windows.Forms.MenuItem menuItem_newDatabase;
        private System.Windows.Forms.MenuItem menuItem_deleteDatabase;
        private System.Windows.Forms.MenuItem menuItem_refresh;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem menuItem_serversCfg;
        private System.Windows.Forms.MenuItem menuItem_exit;
        private System.Windows.Forms.ToolBar toolBar1;
        private System.Windows.Forms.ToolBarButton toolBarButton_stop;
        private System.Windows.Forms.ImageList imageList_toolbar;
        private System.Windows.Forms.MenuItem menuItem_cfgLinkInfo;
        private System.Windows.Forms.MenuItem menuItem3;
        private MenuItem menuItem_test;
        private StatusStrip statusStrip_main;
        private ToolStripStatusLabel toolStripStatusLabel_main;
        private ToolStripProgressBar toolStripProgressBar_main;
        private MenuItem menuItem_testAccessKey;
        private SplitContainer splitContainer1;
        private WebBrowser webBrowser1;
        private MenuItem menuItem_help;
        private MenuItem menuItem_openDataFolder;
        private MenuItem menuItem_openProgramFolder;
        private System.ComponentModel.IContainer components;

        public MainForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
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

                if (this.LinkInfos != null)
                    this.LinkInfos.Dispose();

                if (this.Channels != null)
                    this.Channels.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuItem_serversCfg = new System.Windows.Forms.MenuItem();
            this.menuItem_cfgLinkInfo = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuItem_exit = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem_accountManagement = new System.Windows.Forms.MenuItem();
            this.menuItem_databaseManagement = new System.Windows.Forms.MenuItem();
            this.menuItem_newDatabase = new System.Windows.Forms.MenuItem();
            this.menuItem_deleteDatabase = new System.Windows.Forms.MenuItem();
            this.menuItem_refresh = new System.Windows.Forms.MenuItem();
            this.menuItem_test = new System.Windows.Forms.MenuItem();
            this.menuItem_testAccessKey = new System.Windows.Forms.MenuItem();
            this.toolBar1 = new System.Windows.Forms.ToolBar();
            this.toolBarButton_stop = new System.Windows.Forms.ToolBarButton();
            this.imageList_toolbar = new System.Windows.Forms.ImageList(this.components);
            this.statusStrip_main = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_main = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar_main = new System.Windows.Forms.ToolStripProgressBar();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.treeView_res = new DigitalPlatform.rms.Client.ResTree();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.menuItem_help = new System.Windows.Forms.MenuItem();
            this.menuItem_openDataFolder = new System.Windows.Forms.MenuItem();
            this.menuItem_openProgramFolder = new System.Windows.Forms.MenuItem();
            this.statusStrip_main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem2,
            this.menuItem1,
            this.menuItem_help});
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 0;
            this.menuItem2.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem_serversCfg,
            this.menuItem_cfgLinkInfo,
            this.menuItem3,
            this.menuItem_exit});
            this.menuItem2.Text = "文件(&F)";
            // 
            // menuItem_serversCfg
            // 
            this.menuItem_serversCfg.Index = 0;
            this.menuItem_serversCfg.Text = "缺省帐户管理(&A)...";
            this.menuItem_serversCfg.Click += new System.EventHandler(this.menuItem_serversCfg_Click);
            // 
            // menuItem_cfgLinkInfo
            // 
            this.menuItem_cfgLinkInfo.Index = 1;
            this.menuItem_cfgLinkInfo.Text = "配置关联目录(&L)...";
            this.menuItem_cfgLinkInfo.Visible = false;
            this.menuItem_cfgLinkInfo.Click += new System.EventHandler(this.menuItem_cfgLinkInfo_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 2;
            this.menuItem3.Text = "-";
            // 
            // menuItem_exit
            // 
            this.menuItem_exit.Index = 3;
            this.menuItem_exit.Text = "退出(&X)";
            this.menuItem_exit.Click += new System.EventHandler(this.menuItem_exit_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 1;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem_accountManagement,
            this.menuItem_databaseManagement,
            this.menuItem_newDatabase,
            this.menuItem_deleteDatabase,
            this.menuItem_refresh,
            this.menuItem_test,
            this.menuItem_testAccessKey});
            this.menuItem1.Text = "功能(&U)";
            // 
            // menuItem_accountManagement
            // 
            this.menuItem_accountManagement.Index = 0;
            this.menuItem_accountManagement.Text = "帐户(&A)...";
            this.menuItem_accountManagement.Click += new System.EventHandler(this.menuItem_accountManagement_Click);
            // 
            // menuItem_databaseManagement
            // 
            this.menuItem_databaseManagement.Index = 1;
            this.menuItem_databaseManagement.Text = "数据库(&M)...";
            this.menuItem_databaseManagement.Click += new System.EventHandler(this.menuItem_databaseManagement_Click);
            // 
            // menuItem_newDatabase
            // 
            this.menuItem_newDatabase.Index = 2;
            this.menuItem_newDatabase.Text = "新建数据库(&N)...";
            this.menuItem_newDatabase.Click += new System.EventHandler(this.menuItem_newDatabase_Click);
            // 
            // menuItem_deleteDatabase
            // 
            this.menuItem_deleteDatabase.Index = 3;
            this.menuItem_deleteDatabase.Text = "删除数据库(&D)";
            this.menuItem_deleteDatabase.Click += new System.EventHandler(this.menuItem_deleteObject_Click);
            // 
            // menuItem_refresh
            // 
            this.menuItem_refresh.Index = 4;
            this.menuItem_refresh.Text = "刷新(&R)";
            this.menuItem_refresh.Click += new System.EventHandler(this.menuItem_refresh_Click);
            // 
            // menuItem_test
            // 
            this.menuItem_test.Index = 5;
            this.menuItem_test.Text = "test";
            this.menuItem_test.Visible = false;
            this.menuItem_test.Click += new System.EventHandler(this.menuItem_test_Click);
            // 
            // menuItem_testAccessKey
            // 
            this.menuItem_testAccessKey.Index = 6;
            this.menuItem_testAccessKey.Text = "测试检索点";
            this.menuItem_testAccessKey.Click += new System.EventHandler(this.menuItem_testAccessKey_Click);
            // 
            // toolBar1
            // 
            this.toolBar1.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
            this.toolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.toolBarButton_stop});
            this.toolBar1.DropDownArrows = true;
            this.toolBar1.ImageList = this.imageList_toolbar;
            this.toolBar1.Location = new System.Drawing.Point(0, 0);
            this.toolBar1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.toolBar1.Name = "toolBar1";
            this.toolBar1.ShowToolTips = true;
            this.toolBar1.Size = new System.Drawing.Size(882, 34);
            this.toolBar1.TabIndex = 2;
            this.toolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar1_ButtonClick);
            // 
            // toolBarButton_stop
            // 
            this.toolBarButton_stop.Enabled = false;
            this.toolBarButton_stop.ImageIndex = 0;
            this.toolBarButton_stop.Name = "toolBarButton_stop";
            this.toolBarButton_stop.ToolTipText = "停止";
            // 
            // imageList_toolbar
            // 
            this.imageList_toolbar.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_toolbar.ImageStream")));
            this.imageList_toolbar.TransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.imageList_toolbar.Images.SetKeyName(0, "");
            this.imageList_toolbar.Images.SetKeyName(1, "");
            // 
            // statusStrip_main
            // 
            this.statusStrip_main.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.statusStrip_main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_main,
            this.toolStripProgressBar_main});
            this.statusStrip_main.Location = new System.Drawing.Point(0, 550);
            this.statusStrip_main.Name = "statusStrip_main";
            this.statusStrip_main.Padding = new System.Windows.Forms.Padding(2, 0, 26, 0);
            this.statusStrip_main.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusStrip_main.Size = new System.Drawing.Size(882, 31);
            this.statusStrip_main.TabIndex = 5;
            this.statusStrip_main.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_main
            // 
            this.toolStripStatusLabel_main.Name = "toolStripStatusLabel_main";
            this.toolStripStatusLabel_main.Size = new System.Drawing.Size(535, 22);
            this.toolStripStatusLabel_main.Spring = true;
            // 
            // toolStripProgressBar_main
            // 
            this.toolStripProgressBar_main.Name = "toolStripProgressBar_main";
            this.toolStripProgressBar_main.Size = new System.Drawing.Size(315, 21);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 34);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeView_res);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.webBrowser1);
            this.splitContainer1.Size = new System.Drawing.Size(882, 516);
            this.splitContainer1.SplitterDistance = 511;
            this.splitContainer1.SplitterWidth = 15;
            this.splitContainer1.TabIndex = 6;
            // 
            // treeView_res
            // 
            this.treeView_res.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.treeView_res.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView_res.HideSelection = false;
            this.treeView_res.ImageIndex = 0;
            this.treeView_res.Location = new System.Drawing.Point(0, 0);
            this.treeView_res.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.treeView_res.Name = "treeView_res";
            this.treeView_res.SelectedImageIndex = 0;
            this.treeView_res.Size = new System.Drawing.Size(511, 516);
            this.treeView_res.TabIndex = 0;
            this.treeView_res.OnSetMenu += new DigitalPlatform.GUI.GuiAppendMenuEventHandle(this.treeView_res_OnSetMenu);
            this.treeView_res.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_res_AfterSelect);
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(37, 35);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(356, 516);
            this.webBrowser1.TabIndex = 0;
            // 
            // menuItem_help
            // 
            this.menuItem_help.Index = 2;
            this.menuItem_help.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem_openDataFolder,
            this.menuItem_openProgramFolder});
            this.menuItem_help.Text = "帮助(&H)";
            // 
            // menuItem_openDataFolder
            // 
            this.menuItem_openDataFolder.Index = 0;
            this.menuItem_openDataFolder.Text = "打开数据文件夹(&D)...";
            this.menuItem_openDataFolder.Click += new System.EventHandler(this.menuItem_openDataFolder_Click);
            // 
            // menuItem_openProgramFolder
            // 
            this.menuItem_openProgramFolder.Index = 1;
            this.menuItem_openProgramFolder.Text = "打开程序文件夹(&P)...";
            this.menuItem_openProgramFolder.Click += new System.EventHandler(this.menuItem_openProgramFolder_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(882, 581);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip_main);
            this.Controls.Add(this.toolBar1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.Menu = this.mainMenu1;
            this.Name = "MainForm";
            this.Text = "dp2manager V3 -- 内核管理";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.MainForm_Closing);
            this.Closed += new System.EventHandler(this.Form1_Closed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.statusStrip_main.ResumeLayout(false);
            this.statusStrip_main.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }

        private void Form1_Load(object sender, System.EventArgs e)
        {
            if (ApplicationDeployment.IsNetworkDeployed == true)
            {
                // MessageBox.Show(this, "network");
                DataDir = Application.LocalUserAppDataPath;
            }
            else
            {
                // MessageBox.Show(this, "no network");
                DataDir = Environment.CurrentDirectory;
            }

            // 从文件中装载创建一个ServerCollection对象
            // parameters:
            //		bIgnorFileNotFound	是否不抛出FileNotFoundException异常。
            //							如果==true，函数直接返回一个新的空ServerCollection对象
            // Exception:
            //			FileNotFoundException	文件没找到
            //			SerializationException	版本迁移时容易出现

            try
            {
                Servers = ServerCollection.Load(this.DataDir
                    + "\\manager_servers.bin",
                    true);
                Servers.ownerForm = this;
            }
            catch (SerializationException ex)
            {
                MessageBox.Show(this, ex.Message);
                Servers = new ServerCollection();
                // 设置文件名，以便本次运行结束时覆盖旧文件
                Servers.FileName = this.DataDir
                    + "\\manager_servers.bin";

            }

            this.Servers.ServerChanged += new ServerChangedEventHandle(Servers_ServerChanged);

            // 从文件中装载创建一个LinkInfoCollection对象
            // parameters:
            //		bIgnorFileNotFound	是否不抛出FileNotFoundException异常。
            //							如果==true，函数直接返回一个新的空ServerCollection对象
            // Exception:
            //			FileNotFoundException	文件没找到
            //			SerializationException	版本迁移时容易出现
            try
            {
                LinkInfos = LinkInfoCollection.Load(this.DataDir
                    + "\\manager_linkinfos.bin",
                    true);
            }
            catch (SerializationException ex)
            {
                MessageBox.Show(this, ex.Message);
                LinkInfos = new LinkInfoCollection();
                // 设置文件名，以便本次运行结束时覆盖旧文件
                LinkInfos.FileName = this.DataDir
                    + "\\manager_linkinfos.bin";

            }

            // 设置窗口尺寸状态
            if (AppInfo != null)
            {
                SetFirstDefaultFont();

                MainForm.SetControlFont(this, this.DefaultFont);

                AppInfo.LoadFormStates(this,
                    "mainformstate");
            }

            stopManager.Initial(
                this,
                toolBarButton_stop,
                this.toolStripStatusLabel_main,
                this.toolStripProgressBar_main);
            stop = new DigitalPlatform.Stop();
            stop.Register(this.stopManager, true);	// 和容器关联

            /*
			this.Channels.procAskAccountInfo = 
				new Delegate_AskAccountInfo(this.Servers.AskAccountInfo);
             */
            this.Channels.AskAccountInfo += new AskAccountInfoEventHandle(this.Servers.OnAskAccountInfo);



            // 简单检索界面准备工作
            treeView_res.AppInfo = this.AppInfo;	// 便于treeview中popup菜单修改配置文件时保存dialog尺寸位置

            treeView_res.stopManager = this.stopManager;

            treeView_res.Servers = this.Servers;	// 引用

            treeView_res.Channels = this.Channels;	// 引用

            treeView_res.Fill(null);

            //
            LinkInfos.Channels = this.Channels;

            int nRet = 0;
            string strError = "";
            nRet = this.LinkInfos.Link(out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

            this.ClearHtml();
        }

        void Servers_ServerChanged(object sender, ServerChangedEventArgs e)
        {
            this.treeView_res.Refresh(ResTree.RefreshStyle.All);   // 刷新第一级
        }


        private void Form1_Closed(object sender, System.EventArgs e)
        {
            this.Channels.AskAccountInfo -= new AskAccountInfoEventHandle(this.Servers.OnAskAccountInfo);

            // 如果缺了此句，则Servers.Save会出现问题
            this.Servers.ServerChanged -= new ServerChangedEventHandle(Servers_ServerChanged);

            // 保存到文件
            // parameters:
            //		strFileName	文件名。如果==null,表示使用装载时保存的那个文件名
            Servers.Save(null);
            Servers = null;

            LinkInfos.Save(null);
            LinkInfos = null;

            // 保存窗口尺寸状态
            if (AppInfo != null)
            {

                AppInfo.SaveFormStates(this,
                    "mainformstate");
            }

            //记住save,保存信息XML文件
            AppInfo.Save();
            AppInfo = null;	// 避免后面再用这个对象	
        }

        private void menuItem_accountManagement_Click(object sender, System.EventArgs e)
        {

            if (treeView_res.SelectedNode == null)
            {
                MessageBox.Show("请选择一个节点");
                return;
            }

            ResPath respath = new ResPath(treeView_res.SelectedNode);

            GetUserNameDlg namedlg = new GetUserNameDlg();
            MainForm.SetControlFont(namedlg, this.DefaultFont);

            string strError = "";
            this.Cursor = Cursors.WaitCursor;
            int nRet = namedlg.Initial(this.Servers,
                this.Channels,
                this.stopManager,
                respath.Url,
                out strError);
            this.Cursor = Cursors.Arrow;
            if (nRet == -1)
            {
                MessageBox.Show(strError);
                return;
            }

            namedlg.StartPosition = FormStartPosition.CenterScreen;
            namedlg.ShowDialog(this);
            if (namedlg.DialogResult != DialogResult.OK)
                return;

            UserRightsDlg dlg = new UserRightsDlg();
            MainForm.SetControlFont(dlg, this.DefaultFont);
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.UserName = namedlg.SelectedUserName;
            dlg.UserRecPath = namedlg.SelectedUserRecPath;
            dlg.ServerUrl = respath.Url;
            dlg.MainForm = this;

            this.AppInfo.LinkFormState(dlg, "userrightsdlg_state");
            dlg.ShowDialog(this);
            this.AppInfo.UnlinkFormState(dlg);
        }

        private void menuItem_databaseManagement_Click(object sender, System.EventArgs e)
        {
            if (treeView_res.SelectedNode == null)
            {
                MessageBox.Show("请选择一个数据库节点");
                return;
            }

            ResPath respath = new ResPath(treeView_res.SelectedNode);
            if (respath.Path == "")
            {
                MessageBox.Show("请选择一个数据库类型的节点");
                return;
            }
            string strPath = respath.Path;
            string strDbName = StringUtil.GetFirstPartPath(ref strPath);
            if (strDbName == "")
            {
                MessageBox.Show("错误: 数据库名为空");
                return;
            }

            DatabaseDlg dlg = new DatabaseDlg();
            MainForm.SetControlFont(dlg, this.DefaultFont);
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.MainForm = this;
            dlg.Initial(respath.Url,
                strDbName);

            this.AppInfo.LinkFormState(dlg, "databasedlg_state");
            dlg.ShowDialog(this);
            this.AppInfo.UnlinkFormState(dlg);
        }

        // 创建新数据库
        private void menuItem_newDatabase_Click(object sender, System.EventArgs e)
        {
            if (treeView_res.SelectedNode == null)
            {
                MessageBox.Show("请选择一个服务器或数据库节点");
                return;
            }

            ResPath respath = new ResPath(treeView_res.SelectedNode);

            string strRefDbName = "";
            if (treeView_res.SelectedNode != null
                && treeView_res.SelectedNode.ImageIndex == ResTree.RESTYPE_DB)
            {
                if (respath.Path != "")
                {
                    string strPath = respath.Path;
                    strRefDbName = StringUtil.GetFirstPartPath(ref strPath);
                }
            }


            DatabaseDlg dlg = new DatabaseDlg();
            MainForm.SetControlFont(dlg, this.DefaultFont);
            dlg.Text = "创建新数据库";
            dlg.IsCreate = true;
            dlg.RefDbName = strRefDbName;
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.MainForm = this;
            dlg.Initial(respath.Url,
                "");

            this.AppInfo.LinkFormState(dlg, "databasedlg_state");
            dlg.ShowDialog(this);
            this.AppInfo.UnlinkFormState(dlg);
        }

        // 获得用户记录
        // return:
        //      -1  error
        //      0   not found
        //      >=1   检索命中的条数
        public int GetUserRecord(
            string strServerUrl,
            string strUserName,
            out string strRecPath,
            out string strXml,
            out byte[] baTimeStamp,
            out string strError)
        {
            strError = "";

            strXml = "";
            strRecPath = "";
            baTimeStamp = null;

            if (strUserName == "")
            {
                strError = "用户名为空";
                return -1;
            }

            string strQueryXml = "<target list='" + Defs.DefaultUserDb.Name
                + ":" + Defs.DefaultUserDb.SearchPath.UserName + "'><item><word>"
                + strUserName + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>10</maxCount></item><lang>chi</lang></target>";

            RmsChannel channel = this.Channels.GetChannel(strServerUrl);
            if (channel == null)
            {
                strError = "Channels.GetChannel 异常";
                return -1;
            }

            long nRet = channel.DoSearch(strQueryXml,
                "default",
                out strError);
            if (nRet == -1)
            {
                strError = "检索帐户库时出错: " + strError;
                return -1;
            }

            if (nRet == 0)
                return 0;	// not found

            long nSearchCount = nRet;

            nRet = channel.DoGetSearchResult(
                "default",
                1,
                this.Lang,
                null,	// stop,
                out List<string> aPath,
                out strError);
            if (nRet == -1)
            {
                strError = "检索注册用户库获取检索结果时出错: " + strError;
                return -1;
            }
            if (aPath.Count == 0)
            {
                strError = "检索注册用户库获取的检索结果为空";
                return -1;
            }

            // strRecID = ResPath.GetRecordId((string)aPath[0]);
            strRecPath = (string)aPath[0];

            string strStyle = "content,data,timestamp,withresmetadata";
            string strMetaData;
            string strOutputPath;

            nRet = channel.GetRes((string)aPath[0],
                strStyle,
                out strXml,
                out strMetaData,
                out baTimeStamp,
                out strOutputPath,
                out strError);
            if (nRet == -1)
            {
                strError = "获取注册用户库记录体时出错: " + strError;
                return -1;
            }


            return (int)nSearchCount;
        }

        // 根据路径获得用户记录
        // return:
        //      -1  error
        //      0   not found
        //      >=1   检索命中的条数
        public int GetUserRecord(
            string strServerUrl,
            string strRecPath,
            out string strXml,
            out byte[] baTimeStamp,
            out string strError)
        {

            strError = "";

            strXml = "";
            baTimeStamp = null;

            if (strRecPath == "")
            {
                strError = "路径为空";
                return -1;
            }

            RmsChannel channel = this.Channels.GetChannel(strServerUrl);
            if (channel == null)
            {
                strError = "Channels.GetChannel 异常";
                return -1;
            }


            string strStyle = "content,data,timestamp,withresmetadata";
            string strMetaData;
            string strOutputPath;

            long nRet = channel.GetRes(strRecPath,
                strStyle,
                out strXml,
                out strMetaData,
                out baTimeStamp,
                out strOutputPath,
                out strError);
            if (nRet == -1)
            {
                strError = "获取注册用户库记录体时出错: " + strError;
                if (channel.IsNotFound())
                    return 0;
                return -1;
            }

            return 1;
        }



        private void menuItem_deleteObject_Click(object sender, System.EventArgs e)
        {
            try
            {
                string strError = "";

                if (treeView_res.SelectedNode == null)
                {
                    MessageBox.Show("请选择一个数据库、目录或文件节点");
                    return;
                }

                ResPath respath = new ResPath(treeView_res.SelectedNode);

                string strPath = "";
                if (respath.Path != "")
                {
                    strPath = respath.Path;
                    // strPath = StringUtil.GetFirstPartPath(ref strPath);
                }
                else
                {
                    // Debug.Assert(false, "");
                    MessageBox.Show("请选择一个数据库、目录或文件节点");
                    return;
                }

                string strText = "";

                if (treeView_res.SelectedNode.ImageIndex == ResTree.RESTYPE_DB)
                    strText = "确实要删除位于 " + respath.Url + "\r\n的数据库 '" + strPath + "' ?\r\n\r\n***警告：数据库一旦删除，就无法恢复。";
                else
                    strText = "确实要删除位于 " + respath.Url + "\r\n的对象 '" + strPath + "' ?\r\n\r\n***警告：对象一旦删除，就无法恢复。";

                //
                DialogResult result = MessageBox.Show(this,
                    strText,
                    "dp2manager",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result != DialogResult.Yes)
                    return;

                RmsChannel channel = Channels.GetChannel(respath.Url);
                if (channel == null)
                {
                    strError = "Channels.GetChannel 异常";
                    goto ERROR1;
                }

                long lRet = 0;

                if (treeView_res.SelectedNode.ImageIndex == ResTree.RESTYPE_DB)
                {
                    // 删除数据库
                    lRet = channel.DoDeleteDB(strPath, out strError);
                }
                else
                {
                    byte[] timestamp = null;
                    byte[] output_timestamp = null;

                REDODELETE:
                    // 删除其他资源
                    lRet = channel.DoDeleteRes(strPath,
                        timestamp,
                        out output_timestamp,
                        out strError);
                    if (lRet == -1 && channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                    {
                        timestamp = output_timestamp;
                        goto REDODELETE;
                    }
                }

                if (lRet == -1)
                    goto ERROR1;

                if (treeView_res.SelectedNode.ImageIndex == ResTree.RESTYPE_DB)
                    MessageBox.Show(this, "数据库 '" + strPath + "' 已被成功删除");
                else
                    MessageBox.Show(this, "对象 '" + strPath + "' 已被成功删除");



                this.treeView_res.Refresh(ResTree.RefreshStyle.All);

                return;
            ERROR1:
                MessageBox.Show(this, strError);
                return;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, "menuItem_deleteObject_Click（) 抛出异常: " + ExceptionUtil.GetDebugText(ex));
            }
        }

        public void menuItem_refresh_Click(object sender, System.EventArgs e)
        {
            treeView_res.menu_refresh(null, null);
        }

        private void treeView_res_OnSetMenu(object sender, DigitalPlatform.GUI.GuiAppendMenuEventArgs e)
        {
            Debug.Assert(e.ContextMenu != null, "e不能为null");

            int nNodeType = -1;
            TreeNode node = this.treeView_res.SelectedNode;
            if (node != null)
                nNodeType = node.ImageIndex;



            MenuItem menuItem = new MenuItem("-");
            e.ContextMenu.MenuItems.Add(menuItem);


            // 帐户管理
            menuItem = new MenuItem("帐户(&A)...");
            menuItem.Click += new System.EventHandler(this.menuItem_accountManagement_Click);
            e.ContextMenu.MenuItems.Add(menuItem);

            // 新建帐户
            menuItem = new MenuItem("新帐户(&N)...");
            menuItem.Click += new System.EventHandler(this.menuItem_newAccount_Click);
            e.ContextMenu.MenuItems.Add(menuItem);


            menuItem = new MenuItem("-");
            e.ContextMenu.MenuItems.Add(menuItem);


            // 配置数据库
            menuItem = new MenuItem("数据库(&M)...");
            menuItem.Click += new System.EventHandler(this.menuItem_databaseManagement_Click);
            if (nNodeType != ResTree.RESTYPE_DB)
                menuItem.Enabled = false;
            e.ContextMenu.MenuItems.Add(menuItem);



            // 新建数据库
            menuItem = new MenuItem("新建数据库(&N)...");
            menuItem.Click += new System.EventHandler(this.menuItem_newDatabase_Click);
            e.ContextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("-");
            e.ContextMenu.MenuItems.Add(menuItem);

            // 删除数据库
            menuItem = new MenuItem("删除数据库(&D)");
            menuItem.Click += new System.EventHandler(this.menuItem_deleteObject_Click);
            if (nNodeType != ResTree.RESTYPE_DB
                && nNodeType != ResTree.RESTYPE_FILE
                && nNodeType != ResTree.RESTYPE_FOLDER)
                menuItem.Enabled = false;
            if (nNodeType != ResTree.RESTYPE_DB)
                menuItem.Text = "删除对象(&D)";
            e.ContextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("-");
            e.ContextMenu.MenuItems.Add(menuItem);

#if NO
			// 关联本地目录
			menuItem = new MenuItem("关联本地目录(&L)...");
			menuItem.Click += new System.EventHandler(this.menuItem_linkLocalDir_Click);
			e.ContextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("-");
            e.ContextMenu.MenuItems.Add(menuItem);
#endif

            // 输出模板
            menuItem = new MenuItem("导出模板(&E)...");
            menuItem.Click += new System.EventHandler(this.menuItem_exportTemplate_Click);
            e.ContextMenu.MenuItems.Add(menuItem);

            // 导入模板
            menuItem = new MenuItem("导入模板(&I)...");
            menuItem.Click += new System.EventHandler(this.menuItem_importTemplate_Click);
            e.ContextMenu.MenuItems.Add(menuItem);


        }

        // 导出模板
        void menuItem_exportTemplate_Click(object sender, System.EventArgs e)
        {
            if (treeView_res.SelectedNode == null)
            {
                MessageBox.Show("请选择一个节点");
                return;
            }

            if (treeView_res.SelectedNode.ImageIndex != ResTree.RESTYPE_DB
                && treeView_res.SelectedNode.ImageIndex != ResTree.RESTYPE_SERVER)
            {
                MessageBox.Show("请选择一个服务器或数据库类型节点");
                return;
            }

            treeView_res.Refresh(ResTree.RefreshStyle.Selected);

            ExportTemplateDlg dlg = new ExportTemplateDlg();
            MainForm.SetControlFont(dlg, this.DefaultFont);

            dlg.Objects = new List<ObjectInfo>();

            if (treeView_res.SelectedNode.ImageIndex == ResTree.RESTYPE_SERVER)
            {
                for (int i = 0; i < treeView_res.SelectedNode.Nodes.Count; i++)
                {
                    ObjectInfo objectinfo = new ObjectInfo();

                    ResPath respath = new ResPath(treeView_res.SelectedNode.Nodes[i]);

                    objectinfo.Path = respath.Path;
                    objectinfo.Url = respath.Url;
                    objectinfo.ImageIndex = treeView_res.SelectedNode.Nodes[i].ImageIndex;
                    dlg.Objects.Add(objectinfo);
                }
            }
            else
            {
                ObjectInfo objectinfo = new ObjectInfo();

                ResPath respath = new ResPath(treeView_res.SelectedNode);

                objectinfo.Path = respath.Path;
                objectinfo.Url = respath.Url;
                objectinfo.ImageIndex = treeView_res.SelectedNode.ImageIndex;
                dlg.Objects.Add(objectinfo);
            }

            dlg.MainForm = this;
            dlg.ShowDialog(this);
        }

        // 导入模板
        void menuItem_importTemplate_Click(object sender, System.EventArgs e)
        {
            if (treeView_res.SelectedNode == null)
            {
                MessageBox.Show("请选择一个节点");
                return;
            }

            if (treeView_res.SelectedNode.ImageIndex != ResTree.RESTYPE_SERVER)
            {
                MessageBox.Show("请选择一个服务器类型节点");
                return;
            }

            ResPath respath = new ResPath(treeView_res.SelectedNode);

            /*
            string strRefDbName = "";
            if (treeView_res.SelectedNode != null)
            {
                if (respath.Path != "")
                {
                    string strPath = respath.Path;
                    strRefDbName = StringUtil.GetFirstPartPath(ref strPath);
                }
            }
             */


            OpenFileDialog filedlg = new OpenFileDialog();

            filedlg.FileName = "*.template";
            // filedlg.InitialDirectory = Environment.CurrentDirectory;
            filedlg.Filter = "模板文件 (*.template)|*.template|All files (*.*)|*.*";
            filedlg.RestoreDirectory = true;

            if (filedlg.ShowDialog() != DialogResult.OK)
            {
                return;
            }


            ImportTemplateDlg dlg = new ImportTemplateDlg();
            MainForm.SetControlFont(dlg, this.DefaultFont);

            dlg.Url = respath.Url;
            dlg.FileName = filedlg.FileName;
            dlg.MainForm = this;
            dlg.ShowDialog(this);
        }

        // 新建帐户
        void menuItem_newAccount_Click(object sender, System.EventArgs e)
        {

            if (treeView_res.SelectedNode == null)
            {
                MessageBox.Show("请选择一个节点");
                return;
            }

            ResPath respath = new ResPath(treeView_res.SelectedNode);

            UserRightsDlg dlg = new UserRightsDlg();
            MainForm.SetControlFont(dlg, this.DefaultFont);

            dlg.MainForm = this;
            dlg.ServerUrl = respath.Url;
            dlg.ShowDialog(this);
        }

        // 关联本地目录
        private void menuItem_linkLocalDir_Click(object sender, System.EventArgs e)
        {
            string strDefault = "";
            if (treeView_res.SelectedNode != null)
            {
                ResPath respath = new ResPath(treeView_res.SelectedNode);


                if (treeView_res.SelectedNode.ImageIndex == ResTree.RESTYPE_FOLDER)
                    strDefault = respath.FullPath;
                else
                    strDefault = respath.Url;
            }


            LinkInfoDlg dlg = new LinkInfoDlg();
            MainForm.SetControlFont(dlg, this.DefaultFont);

            dlg.LinkInfos = this.LinkInfos;
            dlg.CreateNewServerPath = strDefault;
            dlg.ShowDialog(this);
        }


        private void treeView_res_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
        {
            if (treeView_res.SelectedNode == null)
            {
                this.toolStripStatusLabel_main.Text = "尚未选择一个节点";
                return;
            }

            ResPath respath = new ResPath(treeView_res.SelectedNode);

            this.toolStripStatusLabel_main.Text = "当前节点: " + respath.FullPath;

        }

        private void menuItem_serversCfg_Click(object sender, System.EventArgs e)
        {
            ServersDlg dlg = new ServersDlg();
            MainForm.SetControlFont(dlg, this.DefaultFont);

            ServerCollection newServers = Servers.Dup();

            string strWidths = this.AppInfo.GetString(
"serversdlg",
"list_column_width",
"");
            if (String.IsNullOrEmpty(strWidths) == false)
            {
                ListViewUtil.SetColumnHeaderWidth(dlg.ListView,
                    strWidths,
                    true);
            }

            dlg.Servers = newServers;

            this.AppInfo.LinkFormState(dlg, "serversdlg_state");
            dlg.ShowDialog(this);
            this.AppInfo.UnlinkFormState(dlg);

            strWidths = ListViewUtil.GetColumnWidthListString(dlg.ListView);
            this.AppInfo.SetString(
                "serversdlg",
                "list_column_width",
                strWidths);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            // this.Servers = newServers;
            this.Servers.Import(newServers);

            // this.treeView_res.Servers = this.Servers;
            treeView_res.Fill(null);
        }

        private void menuItem_exit_Click(object sender, System.EventArgs e)
        {
            this.Close();
        }

        private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
        {
            if (e.Button == toolBarButton_stop)
            {
                stopManager.DoStopActive();
            }
        }

        private void MainForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (stop != null)
            {
                if (stop.State == 0 || stop.State == 1)
                {
                    this.channel.Abort();
                    e.Cancel = true;
                }
            }
        }

        void DoStop()
        {
            if (this.channel != null)
                this.channel.Abort();
        }

        private void menuItem_cfgLinkInfo_Click(object sender, System.EventArgs e)
        {
            LinkInfoDlg dlg = new LinkInfoDlg();
            MainForm.SetControlFont(dlg, this.DefaultFont);

            dlg.LinkInfos = this.LinkInfos;
            dlg.ShowDialog(this);
        }

        // 测试属性值对话框
        private void menuItem_test_Click(object sender, EventArgs e)
        {
            CategoryPropertyDlg dlg = new CategoryPropertyDlg();
            MainForm.SetControlFont(dlg, this.DefaultFont);

            dlg.CfgFileName = Environment.CurrentDirectory + "\\userrightsdef.xml";
            dlg.ShowDialog(this);
        }


        void SetFirstDefaultFont()
        {
            if (this.DefaultFont != null)
                return;

            try
            {
                FontFamily family = new FontFamily("微软雅黑");
            }
            catch
            {
                return;
            }
            this.DefaultFontString = "微软雅黑, 9pt";
        }

        public string DefaultFontString
        {
            get
            {
                return this.AppInfo.GetString(
                    "Global",
                    "default_font",
                    "");
            }
            set
            {
                this.AppInfo.SetString(
                    "Global",
                    "default_font",
                    value);
            }
        }

        new public Font DefaultFont
        {
            get
            {
                string strDefaultFontString = this.DefaultFontString;
                if (String.IsNullOrEmpty(strDefaultFontString) == true)
                    return null;

                // Create the FontConverter.
                System.ComponentModel.TypeConverter converter =
                    System.ComponentModel.TypeDescriptor.GetConverter(typeof(Font));

                return (Font)converter.ConvertFromString(strDefaultFontString);
            }
        }

        // parameters:
        //      bForce  是否强制设置。强制设置是指DefaultFont == null 的时候，也要按照Control.DefaultFont来设置
        public static void SetControlFont(Control control,
            Font font,
            bool bForce = false)
        {
            if (font == null)
            {
                if (bForce == false)
                    return;
                font = Control.DefaultFont;
            }
            if (font.Name == control.Font.Name
                && font.Style == control.Font.Style
                && font.SizeInPoints == control.Font.SizeInPoints)
            { }
            else
                control.Font = font;

            ChangeDifferentFaceFont(control, font);
        }

        static void ChangeDifferentFaceFont(Control parent,
            Font font)
        {
            // 修改所有下级控件的字体，如果字体名不一样的话
            foreach (Control sub in parent.Controls)
            {
                Font subfont = sub.Font;
                float ratio = subfont.SizeInPoints / font.SizeInPoints;
                if (subfont.Name != font.Name
                    || subfont.SizeInPoints != font.SizeInPoints)
                {
                    sub.Font = new Font(font.FontFamily, ratio * font.SizeInPoints, subfont.Style, GraphicsUnit.Point);


                    // sub.Font = new Font(font, subfont.Style);
                }

                if (sub is ToolStrip)
                {
                    ChangeDifferentFaceFont((ToolStrip)sub, font);
                }

                // 递归
                ChangeDifferentFaceFont(sub, font);
            }
        }

        static void ChangeDifferentFaceFont(ToolStrip tool,
    Font font)
        {
            // 修改所有事项的字体，如果字体名不一样的话
            for (int i = 0; i < tool.Items.Count; i++)
            {
                ToolStripItem item = tool.Items[i];

                Font subfont = item.Font;
                float ratio = subfont.SizeInPoints / font.SizeInPoints;
                if (subfont.Name != font.Name
                    || subfont.SizeInPoints != font.SizeInPoints)
                {
                    // item.Font = new Font(font, subfont.Style);
                    item.Font = new Font(font.FontFamily, ratio * font.SizeInPoints, subfont.Style, GraphicsUnit.Point);
                }
            }
        }

        // 测试一个数据库的全部检索点是否完好具备
        private void menuItem_testAccessKey_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (this.treeView_res.SelectedNode == null)
            {
                strError = "尚未选择要要导出数据的数据库节点";
                goto ERROR1;
            }

            List<string> paths = null;
            if (this.treeView_res.CheckBoxes == false)
            {
                if (this.treeView_res.SelectedNode.ImageIndex != ResTree.RESTYPE_DB)
                {
                    strError = "所选择的节点不是数据库类型。请选择要导出数据的数据库节点。";
                    goto ERROR1;
                }
                ResPath respath = new ResPath(this.treeView_res.SelectedNode);
                paths = new List<string>();
                paths.Add(respath.FullPath);   // respath.Path;
            }
            else
            {
                paths = this.treeView_res.GetCheckedDatabaseList();
                if (paths.Count == 0)
                {
                    strError = "请选择至少一个要导出数据的数据库节点。";
                    goto ERROR1;
                }
            }

            DigitalPlatform.Stop stop = this.treeView_res.PrepareStop("正在导出数据");

            this.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
+ " 开始检查检索点</div>");
            try
            {

                RecordLoader loader = new RecordLoader(this.Channels,
                    stop,
                    paths,
                    "default",
                    "id,xml");
                foreach (KernelRecord record in loader)
                {
                    string path = record.RecPath;
                    string url = record.Url;

                    // this.AppendHtml("<div class='debug recpath'>" + HttpUtility.HtmlEncode(path) + "</div>");

                    // return:
                    //      -1  出错
                    //      0   有检索点没有命中。出错情况在 strError 中返回
                    //      1   所有检索点均已命中
                    int nRet = VerifyAccessKey(
                        this.Channels,
                        stop,
                        url,
                        path,
                        record.Xml,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        strError = "验证检索点发现问题: " + strError;
                        this.AppendHtml("<div class='debug error'>" + HttpUtility.HtmlEncode(strError) + "</div>");
                        // goto ERROR1;
                    }
                }
            }
            finally
            {
                this.treeView_res.EndStop(stop);
            }

            this.AppendHtml("<div class='debug begin'>" + HttpUtility.HtmlEncode(DateTime.Now.ToLongTimeString())
+ " 结束检查检索点</div>");

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // return:
        //      -1  出错
        //      0   有检索点没有命中。出错情况在 strError 中返回
        //      1   所有检索点均已命中
        static int VerifyAccessKey(
            RmsChannelCollection channels,
            Stop stop,
            string url,
            string path,
            string strXml,
            out string strError)
        {
            strError = "";

            // 获得检索点

            // RmsChannel channel = channels.CreateTempChannel(url);
            RmsChannel channel = channels.GetChannel(url);
            try
            {
                if (stop != null)
                    stop.SetMessage("正在验证 " + path);

                List<AccessKeyInfo> keys = null;
                long lRet = channel.DoGetKeys(path,
                    strXml,
                    "zh",
                    stop,
                    out keys,
                    out strError);
                if (lRet == -1)
                    return -1;

                string strDbName = ResPath.GetDbName(path);

                // 对每个检索点都进行验证
                foreach (AccessKeyInfo info in keys)
                {
                    Application.DoEvents();

                    // 验证一个 key
                    // return:
                    //      -1  出错
                    //      0   没有命中
                    //      1   命中了
                    int nRet = VerifyOneKey(
                        channel,
                        strDbName,
                        info.FromName,
                        info.Key,
                        path,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 0)
                    {
                        string strError1 = "";
                        nRet = VerifyOneKey(
    channel,
    strDbName,
    info.FromName,
    info.KeyNoProcess,
    path,
    out strError1);
                        if (nRet == -1)
                            return -1;
                        if (nRet == 0)
                        {
                            strError += "; " + strError1;
                            return 0;
                        }
                    }

                }

                return 1;
            }
            finally
            {
                //channel.Close();
                //channel = null;
            }
        }

        // 验证一个 key
        // return:
        //      -1  出错
        //      0   没有命中
        //      1   命中了
        static int VerifyOneKey(
            RmsChannel channel,
            string strDbName,
            string strFromName,
            string strKey,
            string path,
            out string strError)
        {
            string strQueryXml = "<target list='" + StringUtil.GetXmlStringSimple(strDbName)
+ ":" + StringUtil.GetXmlStringSimple(strFromName) + "'><item><word>"
+ StringUtil.GetXmlStringSimple(strKey) + "</word><match>exact</match><relation>=</relation><dataType>string</dataType><maxCount>-1</maxCount></item><lang>chi</lang></target>";
            long lRet = channel.DoSearch(strQueryXml,
"test",
out strError);
            if (lRet == -1)
                return -1;

            if (lRet == 0)
            {
                strError = "检索点 '" + strKey + "' (" + strFromName + ") 没有命中记录 '" + path + "' (没有任何命中)";
                return 0;
            }

            // 验证命中记录中是否包含 path 这一条
            SearchResultLoader loader = new SearchResultLoader(channel,
                null,
                "test",
                "id");
            foreach (KernelRecord record in loader)
            {
                Application.DoEvents();

                if (record.RecPath == path)
                    return 1;
            }

            strError = "检索点 '" + strKey + "' (" + strFromName + ") 没有命中记录 '" + path + "'";
            return 0;
        }


        #region 操作历史显示区

        delegate void Delegate_AppendHtml(string strText);
        /// <summary>
        /// 向 IE 控件中追加一段 HTML 内容
        /// </summary>
        /// <param name="strText">HTML 内容</param>
        public void AppendHtml(string strText)
        {
            if (this.webBrowser1.InvokeRequired)
            {
                Delegate_AppendHtml d = new Delegate_AppendHtml(AppendHtml);
                this.webBrowser1.BeginInvoke(d, new object[] { strText });
                return;
            }

            WriteHtml(this.webBrowser1,
                strText);

            // 因为HTML元素总是没有收尾，其他有些方法可能不奏效
            this.webBrowser1.Document.Window.ScrollTo(0,
    this.webBrowser1.Document.Body.ScrollRectangle.Height);
        }

        public static void WriteHtml(WebBrowser webBrowser,
string strHtml)
        {
            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                // webBrowser.Navigate("about:blank");
                Navigate(webBrowser, "about:blank");  // 2015/7/28

                doc = webBrowser.Document;
            }

            doc.Write(strHtml);
        }

        internal static void Navigate(WebBrowser webBrowser, string urlString)
        {
            int nRedoCount = 0;
        REDO:
            try
            {
                webBrowser.Navigate(urlString);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                /*
System.Runtime.InteropServices.COMException (0x800700AA): 请求的资源在使用中。 (异常来自 HRESULT:0x800700AA)
   在 System.Windows.Forms.UnsafeNativeMethods.IWebBrowser2.Navigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.PerformNavigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.Navigate(String urlString)
   在 dp2Circulation.QuickChargingForm._setReaderRenderString(String strText) 位置 F:\cs4.0\dp2Circulation\Charging\QuickChargingForm.cs:行号 394
                 * */
                if ((uint)ex.ErrorCode == 0x800700AA)
                {
                    nRedoCount++;
                    if (nRedoCount < 5)
                    {
                        Application.DoEvents(); // 2015/8/13
                        Thread.Sleep(200);
                        goto REDO;
                    }
                }

                throw ex;
            }
        }

        public void ClearHtml()
        {
            string strCssUrl = Path.Combine(this.DataDir, "history.css");

            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";

            string strJs = "";

            {
                HtmlDocument doc = this.webBrowser1.Document;

                if (doc == null)
                {
                    webBrowser1.Navigate("about:blank");
                    doc = webBrowser1.Document;
                }
                doc = doc.OpenNew(true);
            }

            WriteHtml(this.webBrowser1,
                 "<html><head>" + strLink + strJs + "</head><body>");
        }

        #endregion

        private void menuItem_openDataFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(this.DataDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionUtil.GetAutoText(ex));
            }
        }

        private void menuItem_openProgramFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(Environment.CurrentDirectory);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ExceptionUtil.GetAutoText(ex));
            }
        }
    }
}
