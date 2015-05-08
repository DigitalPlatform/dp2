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
using System.Threading;
using System.IO;
using System.Text;
using System.Xml;

using System.Reflection;
using System.Deployment.Application;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.rms.Client;
using DigitalPlatform.IO;
using DigitalPlatform.Range;
using DigitalPlatform.Marc;
using DigitalPlatform.Library;
using DigitalPlatform.Script;
using DigitalPlatform.MarcDom;
using DigitalPlatform.Text;

namespace dp2Batch
{

	/// <summary>
	/// Summary description for MainForm.
	/// </summary>
	public class MainForm : System.Windows.Forms.Form
	{
        public string DataDir = "";

		Assembly AssemblyMain = null;
		Assembly AssemblyFilter = null;
		MyFilterDocument MarcFilter = null;
		int		m_nAssemblyVersion = 0;
		Batch batchObj = null;
		int m_nRecordCount = 0;


		ScriptManager scriptManager = new ScriptManager();



		Hashtable m_tableMarcSyntax = new Hashtable();	// 数据库全路径和MARC格式的对照表
		string CurMarcSyntax = "";	// 当前MARC格式
		bool OutputCrLf = false;	// ISO2709文件记录尾部是否加入回车换行符号
        bool AddG01 = false;    // ISO2709文件中的记录内是否加入-01字段？(不加时就要去除原有的，以免误会)
        bool Remove998 = false; // 输出 ISO2709 文件的时候是否删除 998 字段?

		public CfgCache cfgCache = new CfgCache();


		public event CheckTargetDbEventHandler CheckTargetDb = null;

		public DigitalPlatform.StopManager	stopManager = new DigitalPlatform.StopManager();

		public ServerCollection Servers = null;

		//保存界面信息
		public ApplicationInfo	AppInfo = new ApplicationInfo("dp2batch.xml");


		//

		RmsChannel channel = null;	// 临时使用的channel对象

		public AutoResetEvent eventClose = new AutoResetEvent(false);

		RmsChannelCollection	Channels = new RmsChannelCollection();	// 拥有

		DigitalPlatform.Stop stop = null;

		string strLastOutputFileName = "";
		int nLastOutputFilterIndex = 1;

		// double ProgressRatio = 1.0;

		bool bNotAskTimestampMismatchWhenOverwrite = false;	// 当转入数据的时候,如果发生时间戳不匹配,是否不询问就强行覆盖

		private System.Windows.Forms.MainMenu mainMenu1;
		private System.Windows.Forms.MenuItem menuItem_exit;
		private System.Windows.Forms.TabControl tabControl_main;
		private System.Windows.Forms.TabPage tabPage_range;
		private System.Windows.Forms.Panel panel_range;
		private System.Windows.Forms.Panel panel_resdirtree;
		private System.Windows.Forms.Splitter splitter_range;
		private System.Windows.Forms.Panel panel_rangeParams;
		private System.Windows.Forms.CheckBox checkBox_verifyNumber;
		public System.Windows.Forms.TextBox textBox_dbPath;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.GroupBox groupBox1;
		public System.Windows.Forms.TextBox textBox_endNo;
		private System.Windows.Forms.Label label3;
		public System.Windows.Forms.TextBox textBox_startNo;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.RadioButton radioButton_startEnd;
		private System.Windows.Forms.RadioButton radioButton_all;
		private System.Windows.Forms.CheckBox checkBox_forceLoop;
		private System.Windows.Forms.TabPage tabPage_resultset;
		private System.Windows.Forms.MenuItem menuItem_file;
		private System.Windows.Forms.MenuItem menuItem_help;
		private System.Windows.Forms.MenuItem menuItem_copyright;
		private System.Windows.Forms.MenuItem menuItem_cfg;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.ToolBar toolBar_main;
		private System.Windows.Forms.ToolBarButton toolBarButton_stop;
        private System.Windows.Forms.ImageList imageList_toolbar;
		private System.Windows.Forms.ToolBarButton toolBarButton_begin;
		private System.Windows.Forms.TabPage tabPage_import;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Button button_import_findFileName;
		private System.Windows.Forms.TextBox textBox_import_fileName;
		private System.Windows.Forms.Label label5;
		private ResTree treeView_rangeRes;
		private System.Windows.Forms.CheckBox checkBox_export_delete;
		private System.Windows.Forms.Label label6;
		private System.Windows.Forms.TextBox textBox_import_range;
		private System.Windows.Forms.MenuItem menuItem_serversCfg;
		private System.Windows.Forms.MenuItem menuItem_projectManage;
		private System.Windows.Forms.MenuItem menuItem_run;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.TextBox textBox_import_dbMap;
		private System.Windows.Forms.Button button_import_dbMap;
        private MenuItem menuItem_openDataFolder;
        private MenuItem menuItem3;
        private MenuItem menuItem_rebuildKeys;
        private MenuItem menuItem_rebuildKeysByDbnames;
        private StatusStrip statusStrip_main;
        private ToolStripStatusLabel toolStripStatusLabel_main;
        private ToolStripProgressBar toolStripProgressBar_main;
        private CheckBox checkBox_export_fastMode;
        private CheckBox checkBox_import_fastMode;
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
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
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
            this.menuItem_file = new System.Windows.Forms.MenuItem();
            this.menuItem_run = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuItem_projectManage = new System.Windows.Forms.MenuItem();
            this.menuItem_cfg = new System.Windows.Forms.MenuItem();
            this.menuItem_serversCfg = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuItem_rebuildKeys = new System.Windows.Forms.MenuItem();
            this.menuItem_rebuildKeysByDbnames = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem_exit = new System.Windows.Forms.MenuItem();
            this.menuItem_help = new System.Windows.Forms.MenuItem();
            this.menuItem_copyright = new System.Windows.Forms.MenuItem();
            this.menuItem_openDataFolder = new System.Windows.Forms.MenuItem();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_range = new System.Windows.Forms.TabPage();
            this.panel_range = new System.Windows.Forms.Panel();
            this.panel_resdirtree = new System.Windows.Forms.Panel();
            this.splitter_range = new System.Windows.Forms.Splitter();
            this.panel_rangeParams = new System.Windows.Forms.Panel();
            this.checkBox_export_fastMode = new System.Windows.Forms.CheckBox();
            this.checkBox_export_delete = new System.Windows.Forms.CheckBox();
            this.checkBox_verifyNumber = new System.Windows.Forms.CheckBox();
            this.textBox_dbPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox_endNo = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_startNo = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.radioButton_startEnd = new System.Windows.Forms.RadioButton();
            this.radioButton_all = new System.Windows.Forms.RadioButton();
            this.checkBox_forceLoop = new System.Windows.Forms.CheckBox();
            this.tabPage_resultset = new System.Windows.Forms.TabPage();
            this.tabPage_import = new System.Windows.Forms.TabPage();
            this.checkBox_import_fastMode = new System.Windows.Forms.CheckBox();
            this.textBox_import_range = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.button_import_findFileName = new System.Windows.Forms.Button();
            this.textBox_import_fileName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button_import_dbMap = new System.Windows.Forms.Button();
            this.textBox_import_dbMap = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.toolBar_main = new System.Windows.Forms.ToolBar();
            this.toolBarButton_stop = new System.Windows.Forms.ToolBarButton();
            this.toolBarButton_begin = new System.Windows.Forms.ToolBarButton();
            this.imageList_toolbar = new System.Windows.Forms.ImageList(this.components);
            this.statusStrip_main = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_main = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar_main = new System.Windows.Forms.ToolStripProgressBar();
            this.treeView_rangeRes = new DigitalPlatform.rms.Client.ResTree();
            this.tabControl_main.SuspendLayout();
            this.tabPage_range.SuspendLayout();
            this.panel_range.SuspendLayout();
            this.panel_resdirtree.SuspendLayout();
            this.panel_rangeParams.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage_import.SuspendLayout();
            this.statusStrip_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem_file,
            this.menuItem_help});
            // 
            // menuItem_file
            // 
            this.menuItem_file.Index = 0;
            this.menuItem_file.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem_run,
            this.menuItem2,
            this.menuItem_projectManage,
            this.menuItem_cfg,
            this.menuItem_serversCfg,
            this.menuItem3,
            this.menuItem_rebuildKeys,
            this.menuItem_rebuildKeysByDbnames,
            this.menuItem1,
            this.menuItem_exit});
            this.menuItem_file.Text = "文件(&F)";
            // 
            // menuItem_run
            // 
            this.menuItem_run.Index = 0;
            this.menuItem_run.Text = "运行(&R)...";
            this.menuItem_run.Click += new System.EventHandler(this.menuItem_run_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 1;
            this.menuItem2.Text = "-";
            // 
            // menuItem_projectManage
            // 
            this.menuItem_projectManage.Index = 2;
            this.menuItem_projectManage.Text = "方案管理(&M)...";
            this.menuItem_projectManage.Click += new System.EventHandler(this.menuItem_projectManage_Click);
            // 
            // menuItem_cfg
            // 
            this.menuItem_cfg.Enabled = false;
            this.menuItem_cfg.Index = 3;
            this.menuItem_cfg.Text = "配置(&C)...";
            this.menuItem_cfg.Click += new System.EventHandler(this.menuItem_cfg_Click);
            // 
            // menuItem_serversCfg
            // 
            this.menuItem_serversCfg.Index = 4;
            this.menuItem_serversCfg.Text = "缺省帐户管理(&A)...";
            this.menuItem_serversCfg.Click += new System.EventHandler(this.menuItem_serversCfg_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 5;
            this.menuItem3.Text = "-";
            // 
            // menuItem_rebuildKeys
            // 
            this.menuItem_rebuildKeys.Index = 6;
            this.menuItem_rebuildKeys.Text = "重建检索点(&B)";
            this.menuItem_rebuildKeys.Click += new System.EventHandler(this.menuItem_rebuildKeys_Click);
            // 
            // menuItem_rebuildKeysByDbnames
            // 
            this.menuItem_rebuildKeysByDbnames.Index = 7;
            this.menuItem_rebuildKeysByDbnames.Text = "重建检索点[根据剪贴板内的数据库路径](&R)...";
            this.menuItem_rebuildKeysByDbnames.Click += new System.EventHandler(this.menuItem_rebuildKeysByDbnames_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 8;
            this.menuItem1.Text = "-";
            // 
            // menuItem_exit
            // 
            this.menuItem_exit.Index = 9;
            this.menuItem_exit.Text = "退出(&X)";
            this.menuItem_exit.Click += new System.EventHandler(this.menuItem_exit_Click);
            // 
            // menuItem_help
            // 
            this.menuItem_help.Index = 1;
            this.menuItem_help.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem_copyright,
            this.menuItem_openDataFolder});
            this.menuItem_help.Text = "帮助(&H)";
            // 
            // menuItem_copyright
            // 
            this.menuItem_copyright.Index = 0;
            this.menuItem_copyright.Text = "版权(&C)...";
            this.menuItem_copyright.Click += new System.EventHandler(this.menuItem_copyright_Click);
            // 
            // menuItem_openDataFolder
            // 
            this.menuItem_openDataFolder.Index = 1;
            this.menuItem_openDataFolder.Text = "打开数据文件夹(&D)...";
            this.menuItem_openDataFolder.Click += new System.EventHandler(this.menuItem_openDataFolder_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Controls.Add(this.tabPage_range);
            this.tabControl_main.Controls.Add(this.tabPage_resultset);
            this.tabControl_main.Controls.Add(this.tabPage_import);
            this.tabControl_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_main.Location = new System.Drawing.Point(0, 32);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(634, 273);
            this.tabControl_main.TabIndex = 1;
            this.tabControl_main.SelectedIndexChanged += new System.EventHandler(this.tabControl_main_SelectedIndexChanged);
            // 
            // tabPage_range
            // 
            this.tabPage_range.BackColor = System.Drawing.Color.Transparent;
            this.tabPage_range.Controls.Add(this.panel_range);
            this.tabPage_range.Location = new System.Drawing.Point(4, 23);
            this.tabPage_range.Name = "tabPage_range";
            this.tabPage_range.Padding = new System.Windows.Forms.Padding(6);
            this.tabPage_range.Size = new System.Drawing.Size(626, 246);
            this.tabPage_range.TabIndex = 0;
            this.tabPage_range.Text = "按记录ID导出";
            this.tabPage_range.UseVisualStyleBackColor = true;
            // 
            // panel_range
            // 
            this.panel_range.Controls.Add(this.panel_resdirtree);
            this.panel_range.Controls.Add(this.splitter_range);
            this.panel_range.Controls.Add(this.panel_rangeParams);
            this.panel_range.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_range.Location = new System.Drawing.Point(6, 6);
            this.panel_range.Name = "panel_range";
            this.panel_range.Size = new System.Drawing.Size(614, 234);
            this.panel_range.TabIndex = 8;
            // 
            // panel_resdirtree
            // 
            this.panel_resdirtree.Controls.Add(this.treeView_rangeRes);
            this.panel_resdirtree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_resdirtree.Location = new System.Drawing.Point(317, 0);
            this.panel_resdirtree.Name = "panel_resdirtree";
            this.panel_resdirtree.Padding = new System.Windows.Forms.Padding(0, 4, 4, 4);
            this.panel_resdirtree.Size = new System.Drawing.Size(297, 234);
            this.panel_resdirtree.TabIndex = 6;
            // 
            // splitter_range
            // 
            this.splitter_range.Location = new System.Drawing.Point(309, 0);
            this.splitter_range.Name = "splitter_range";
            this.splitter_range.Size = new System.Drawing.Size(8, 234);
            this.splitter_range.TabIndex = 8;
            this.splitter_range.TabStop = false;
            // 
            // panel_rangeParams
            // 
            this.panel_rangeParams.AutoScroll = true;
            this.panel_rangeParams.Controls.Add(this.checkBox_export_fastMode);
            this.panel_rangeParams.Controls.Add(this.checkBox_export_delete);
            this.panel_rangeParams.Controls.Add(this.checkBox_verifyNumber);
            this.panel_rangeParams.Controls.Add(this.textBox_dbPath);
            this.panel_rangeParams.Controls.Add(this.label1);
            this.panel_rangeParams.Controls.Add(this.groupBox1);
            this.panel_rangeParams.Controls.Add(this.checkBox_forceLoop);
            this.panel_rangeParams.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel_rangeParams.Location = new System.Drawing.Point(0, 0);
            this.panel_rangeParams.Name = "panel_rangeParams";
            this.panel_rangeParams.Size = new System.Drawing.Size(309, 234);
            this.panel_rangeParams.TabIndex = 7;
            // 
            // checkBox_export_fastMode
            // 
            this.checkBox_export_fastMode.AutoSize = true;
            this.checkBox_export_fastMode.Location = new System.Drawing.Point(144, 198);
            this.checkBox_export_fastMode.Name = "checkBox_export_fastMode";
            this.checkBox_export_fastMode.Size = new System.Drawing.Size(90, 18);
            this.checkBox_export_fastMode.TabIndex = 7;
            this.checkBox_export_fastMode.Text = "快速模式(&F)";
            this.checkBox_export_fastMode.UseVisualStyleBackColor = true;
            // 
            // checkBox_export_delete
            // 
            this.checkBox_export_delete.Location = new System.Drawing.Point(9, 195);
            this.checkBox_export_delete.Name = "checkBox_export_delete";
            this.checkBox_export_delete.Size = new System.Drawing.Size(117, 24);
            this.checkBox_export_delete.TabIndex = 6;
            this.checkBox_export_delete.Text = "删除记录(&D)";
            this.checkBox_export_delete.CheckedChanged += new System.EventHandler(this.checkBox_export_delete_CheckedChanged);
            // 
            // checkBox_verifyNumber
            // 
            this.checkBox_verifyNumber.Location = new System.Drawing.Point(9, 171);
            this.checkBox_verifyNumber.Name = "checkBox_verifyNumber";
            this.checkBox_verifyNumber.Size = new System.Drawing.Size(124, 18);
            this.checkBox_verifyNumber.TabIndex = 4;
            this.checkBox_verifyNumber.Text = "校准首尾ID(&V)";
            // 
            // textBox_dbPath
            // 
            this.textBox_dbPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dbPath.Location = new System.Drawing.Point(75, 6);
            this.textBox_dbPath.Name = "textBox_dbPath";
            this.textBox_dbPath.ReadOnly = true;
            this.textBox_dbPath.Size = new System.Drawing.Size(186, 22);
            this.textBox_dbPath.TabIndex = 2;
            this.textBox_dbPath.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(7, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 18);
            this.label1.TabIndex = 1;
            this.label1.Text = "数据库:";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.textBox_endNo);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textBox_startNo);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.radioButton_startEnd);
            this.groupBox1.Controls.Add(this.radioButton_all);
            this.groupBox1.Location = new System.Drawing.Point(7, 32);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(253, 134);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " 输出记录范围 ";
            // 
            // textBox_endNo
            // 
            this.textBox_endNo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_endNo.Location = new System.Drawing.Point(165, 95);
            this.textBox_endNo.Name = "textBox_endNo";
            this.textBox_endNo.Size = new System.Drawing.Size(74, 22);
            this.textBox_endNo.TabIndex = 5;
            this.textBox_endNo.TextChanged += new System.EventHandler(this.textBox_endNo_TextChanged);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(69, 97);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(85, 19);
            this.label3.TabIndex = 4;
            this.label3.Text = "结束记录ID:";
            // 
            // textBox_startNo
            // 
            this.textBox_startNo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_startNo.Location = new System.Drawing.Point(165, 63);
            this.textBox_startNo.Name = "textBox_startNo";
            this.textBox_startNo.Size = new System.Drawing.Size(74, 22);
            this.textBox_startNo.TabIndex = 3;
            this.textBox_startNo.TextChanged += new System.EventHandler(this.textBox_startNo_TextChanged);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(69, 65);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(85, 18);
            this.label2.TabIndex = 2;
            this.label2.Text = "起始记录ID:";
            // 
            // radioButton_startEnd
            // 
            this.radioButton_startEnd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.radioButton_startEnd.Checked = true;
            this.radioButton_startEnd.Location = new System.Drawing.Point(21, 38);
            this.radioButton_startEnd.Name = "radioButton_startEnd";
            this.radioButton_startEnd.Size = new System.Drawing.Size(143, 19);
            this.radioButton_startEnd.TabIndex = 1;
            this.radioButton_startEnd.TabStop = true;
            this.radioButton_startEnd.Text = "起止ID(&S) ";
            // 
            // radioButton_all
            // 
            this.radioButton_all.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.radioButton_all.Location = new System.Drawing.Point(21, 19);
            this.radioButton_all.Name = "radioButton_all";
            this.radioButton_all.Size = new System.Drawing.Size(56, 19);
            this.radioButton_all.TabIndex = 0;
            this.radioButton_all.Text = "全部(&A)";
            this.radioButton_all.CheckedChanged += new System.EventHandler(this.radioButton_all_CheckedChanged);
            // 
            // checkBox_forceLoop
            // 
            this.checkBox_forceLoop.Location = new System.Drawing.Point(144, 171);
            this.checkBox_forceLoop.Name = "checkBox_forceLoop";
            this.checkBox_forceLoop.Size = new System.Drawing.Size(158, 18);
            this.checkBox_forceLoop.TabIndex = 5;
            this.checkBox_forceLoop.Text = "未命中时继续循环(&C)";
            // 
            // tabPage_resultset
            // 
            this.tabPage_resultset.BackColor = System.Drawing.Color.Transparent;
            this.tabPage_resultset.Location = new System.Drawing.Point(4, 23);
            this.tabPage_resultset.Name = "tabPage_resultset";
            this.tabPage_resultset.Size = new System.Drawing.Size(626, 98);
            this.tabPage_resultset.TabIndex = 1;
            this.tabPage_resultset.Text = "按结果集导出";
            this.tabPage_resultset.UseVisualStyleBackColor = true;
            this.tabPage_resultset.Visible = false;
            // 
            // tabPage_import
            // 
            this.tabPage_import.AutoScroll = true;
            this.tabPage_import.BackColor = System.Drawing.Color.Transparent;
            this.tabPage_import.Controls.Add(this.checkBox_import_fastMode);
            this.tabPage_import.Controls.Add(this.textBox_import_range);
            this.tabPage_import.Controls.Add(this.label6);
            this.tabPage_import.Controls.Add(this.button_import_findFileName);
            this.tabPage_import.Controls.Add(this.textBox_import_fileName);
            this.tabPage_import.Controls.Add(this.label5);
            this.tabPage_import.Controls.Add(this.button_import_dbMap);
            this.tabPage_import.Controls.Add(this.textBox_import_dbMap);
            this.tabPage_import.Controls.Add(this.label4);
            this.tabPage_import.Location = new System.Drawing.Point(4, 23);
            this.tabPage_import.Name = "tabPage_import";
            this.tabPage_import.Size = new System.Drawing.Size(626, 98);
            this.tabPage_import.TabIndex = 2;
            this.tabPage_import.Text = "导入";
            this.tabPage_import.UseVisualStyleBackColor = true;
            // 
            // checkBox_import_fastMode
            // 
            this.checkBox_import_fastMode.AutoSize = true;
            this.checkBox_import_fastMode.Location = new System.Drawing.Point(119, 61);
            this.checkBox_import_fastMode.Name = "checkBox_import_fastMode";
            this.checkBox_import_fastMode.Size = new System.Drawing.Size(90, 18);
            this.checkBox_import_fastMode.TabIndex = 8;
            this.checkBox_import_fastMode.Text = "快速模式(&F)";
            this.checkBox_import_fastMode.UseVisualStyleBackColor = true;
            // 
            // textBox_import_range
            // 
            this.textBox_import_range.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_import_range.Location = new System.Drawing.Point(119, 33);
            this.textBox_import_range.Name = "textBox_import_range";
            this.textBox_import_range.Size = new System.Drawing.Size(379, 22);
            this.textBox_import_range.TabIndex = 7;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 36);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(76, 14);
            this.label6.TabIndex = 6;
            this.label6.Text = "导入范围(&R):";
            // 
            // button_import_findFileName
            // 
            this.button_import_findFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_import_findFileName.Location = new System.Drawing.Point(504, 8);
            this.button_import_findFileName.Name = "button_import_findFileName";
            this.button_import_findFileName.Size = new System.Drawing.Size(46, 22);
            this.button_import_findFileName.TabIndex = 5;
            this.button_import_findFileName.Text = "...";
            this.button_import_findFileName.Click += new System.EventHandler(this.button_import_findFileName_Click);
            // 
            // textBox_import_fileName
            // 
            this.textBox_import_fileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_import_fileName.Location = new System.Drawing.Point(119, 8);
            this.textBox_import_fileName.Name = "textBox_import_fileName";
            this.textBox_import_fileName.Size = new System.Drawing.Size(379, 22);
            this.textBox_import_fileName.TabIndex = 4;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(9, 10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(63, 14);
            this.label5.TabIndex = 3;
            this.label5.Text = "文件名(&F):";
            // 
            // button_import_dbMap
            // 
            this.button_import_dbMap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_import_dbMap.Location = new System.Drawing.Point(504, 102);
            this.button_import_dbMap.Name = "button_import_dbMap";
            this.button_import_dbMap.Size = new System.Drawing.Size(46, 23);
            this.button_import_dbMap.TabIndex = 2;
            this.button_import_dbMap.Text = "...";
            this.button_import_dbMap.Click += new System.EventHandler(this.button_import_dbMap_Click);
            // 
            // textBox_import_dbMap
            // 
            this.textBox_import_dbMap.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_import_dbMap.Location = new System.Drawing.Point(12, 104);
            this.textBox_import_dbMap.Multiline = true;
            this.textBox_import_dbMap.Name = "textBox_import_dbMap";
            this.textBox_import_dbMap.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_import_dbMap.Size = new System.Drawing.Size(489, 110);
            this.textBox_import_dbMap.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 87);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(101, 14);
            this.label4.TabIndex = 0;
            this.label4.Text = "库名映射规则(&T):";
            // 
            // toolBar_main
            // 
            this.toolBar_main.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
            this.toolBar_main.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.toolBarButton_stop,
            this.toolBarButton_begin});
            this.toolBar_main.Divider = false;
            this.toolBar_main.DropDownArrows = true;
            this.toolBar_main.ImageList = this.imageList_toolbar;
            this.toolBar_main.Location = new System.Drawing.Point(0, 0);
            this.toolBar_main.Name = "toolBar_main";
            this.toolBar_main.ShowToolTips = true;
            this.toolBar_main.Size = new System.Drawing.Size(634, 32);
            this.toolBar_main.TabIndex = 2;
            this.toolBar_main.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar1_ButtonClick);
            // 
            // toolBarButton_stop
            // 
            this.toolBarButton_stop.Enabled = false;
            this.toolBarButton_stop.ImageIndex = 0;
            this.toolBarButton_stop.Name = "toolBarButton_stop";
            this.toolBarButton_stop.ToolTipText = "停止";
            // 
            // toolBarButton_begin
            // 
            this.toolBarButton_begin.ImageIndex = 1;
            this.toolBarButton_begin.Name = "toolBarButton_begin";
            this.toolBarButton_begin.ToolTipText = "开始";
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
            this.statusStrip_main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_main,
            this.toolStripProgressBar_main});
            this.statusStrip_main.Location = new System.Drawing.Point(0, 305);
            this.statusStrip_main.Name = "statusStrip_main";
            this.statusStrip_main.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusStrip_main.Size = new System.Drawing.Size(634, 22);
            this.statusStrip_main.TabIndex = 4;
            this.statusStrip_main.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_main
            // 
            this.toolStripStatusLabel_main.Name = "toolStripStatusLabel_main";
            this.toolStripStatusLabel_main.Size = new System.Drawing.Size(445, 17);
            this.toolStripStatusLabel_main.Spring = true;
            // 
            // toolStripProgressBar_main
            // 
            this.toolStripProgressBar_main.Name = "toolStripProgressBar_main";
            this.toolStripProgressBar_main.Size = new System.Drawing.Size(172, 16);
            // 
            // treeView_rangeRes
            // 
            this.treeView_rangeRes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView_rangeRes.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.treeView_rangeRes.HideSelection = false;
            this.treeView_rangeRes.ImageIndex = 0;
            this.treeView_rangeRes.Location = new System.Drawing.Point(0, 4);
            this.treeView_rangeRes.Name = "treeView_rangeRes";
            this.treeView_rangeRes.SelectedImageIndex = 0;
            this.treeView_rangeRes.Size = new System.Drawing.Size(293, 226);
            this.treeView_rangeRes.TabIndex = 0;
            this.treeView_rangeRes.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeView_rangeRes_AfterCheck);
            this.treeView_rangeRes.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_rangeRes_AfterSelect);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 14F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 327);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.statusStrip_main);
            this.Controls.Add(this.toolBar_main);
            this.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Menu = this.mainMenu1;
            this.Name = "MainForm";
            this.Text = "dp2batch V2 -- 批处理";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.MainForm_Closing);
            this.Closed += new System.EventHandler(this.MainForm_Closed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_range.ResumeLayout(false);
            this.panel_range.ResumeLayout(false);
            this.panel_resdirtree.ResumeLayout(false);
            this.panel_rangeParams.ResumeLayout(false);
            this.panel_rangeParams.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage_import.ResumeLayout(false);
            this.tabPage_import.PerformLayout();
            this.statusStrip_main.ResumeLayout(false);
            this.statusStrip_main.PerformLayout();
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

		private void MainForm_Load(object sender, System.EventArgs e)
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
					+ "\\dp2batch_servers.bin",
					true);
				Servers.ownerForm = this;
			}
			catch (SerializationException ex)
			{
				MessageBox.Show(this, ex.Message);
				Servers = new ServerCollection();
				// 设置文件名，以便本次运行结束时覆盖旧文件
                Servers.FileName = this.DataDir
					+ "\\dp2batch_servers.bin";

			}

            this.Servers.ServerChanged += new ServerChangedEventHandle(Servers_ServerChanged);


			string strError = "";
            int nRet = cfgCache.Load(this.DataDir
				+ "\\cfgcache.xml",
				out strError);
			if (nRet == -1) 
			{
				MessageBox.Show(this, strError);
			}
            cfgCache.TempDir = this.DataDir
				+ "\\cfgcache";
			cfgCache.InstantSave = true;

		
			// 设置窗口尺寸状态
			if (AppInfo != null) 
			{
                /*
                // 首次运行，尽量利用“微软雅黑”字体
                if (this.IsFirstRun == true)
                {
                    SetFirstDefaultFont();
                }
                 * */

                SetFirstDefaultFont();

                MainForm.SetControlFont(this, this.DefaultFont);

				AppInfo.LoadFormStates(this,
					"mainformstate");
			}


			stopManager.Initial(this.toolBarButton_stop,
                this.toolStripStatusLabel_main,
                this.toolStripProgressBar_main);
			stopManager.LinkReverseButton(this.toolBarButton_begin);

			// ////////////////

			stop = new DigitalPlatform.Stop();
			stop.Register(this.stopManager, true);	// 和容器关联

            this.Channels.AskAccountInfo +=new AskAccountInfoEventHandle(this.Servers.OnAskAccountInfo);
            /*
			this.Channels.procAskAccountInfo = 
				new Delegate_AskAccountInfo(this.Servers.AskAccountInfo);
             */

			// 简单检索界面准备工作
			treeView_rangeRes.stopManager = this.stopManager;

			treeView_rangeRes.Servers = this.Servers;	// 引用

			treeView_rangeRes.Channels = this.Channels;	// 引用
            treeView_rangeRes.AppInfo = this.AppInfo;   // 2013/2/15
			treeView_rangeRes.Fill(null);

			this.textBox_import_fileName.Text = 
				AppInfo.GetString(
				"page_import",
				"source_file_name",
				"");

			this.textBox_import_range.Text = 
				AppInfo.GetString(
				"page_import",
				"range",
				"");

			this.textBox_import_dbMap.Text = 
				AppInfo.GetString(
				"page_import",
				"dbmap",
				"").Replace(";","\r\n");
            this.checkBox_import_fastMode.Checked = AppInfo.GetBoolean(
                "page_import",
                "fastmode",
                true);

			textBox_startNo.Text = 
				AppInfo.GetString(
				"rangePage",
				"startNumber",
				"");

			textBox_endNo.Text = 
				AppInfo.GetString(
				"rangePage",
				"endNumber",
				"");

			checkBox_verifyNumber.Checked = 
				Convert.ToBoolean(
				AppInfo.GetInt(
				"rangePage",
				"verifyrange",
				0)
				);

			checkBox_forceLoop.Checked = 
				Convert.ToBoolean(
				AppInfo.GetInt(
				"rangePage",
				"forceloop",
				0)
				);

			
			checkBox_export_delete.Checked = 
				Convert.ToBoolean(
				AppInfo.GetInt(
				"rangePage",
				"delete",
				0)
				);

            this.checkBox_export_fastMode.Checked = AppInfo.GetBoolean(
                "rangePage",
                "fastmode",
                true);

			this.radioButton_all.Checked = 
				Convert.ToBoolean(
				AppInfo.GetInt(
				"rangePage",
				"all",
				0)
				);

			strLastOutputFileName = 
				AppInfo.GetString(
				"rangePage",
				"lastoutputfilename",
				"");

			nLastOutputFilterIndex = 
				AppInfo.GetInt(
				"rangePage",
				"lastoutputfilterindex",
				1);

			scriptManager.applicationInfo = AppInfo;
			scriptManager.CfgFilePath =
                this.DataDir + "\\projects.xml";
            scriptManager.DataDir = this.DataDir;

			scriptManager.CreateDefaultContent -=new CreateDefaultContentEventHandler(scriptManager_CreateDefaultContent);
			scriptManager.CreateDefaultContent +=new CreateDefaultContentEventHandler(scriptManager_CreateDefaultContent);

			// 按照上次保存的路径展开resdircontrol树
			string strResDirPath = AppInfo.GetString(
				"rangePage",
				"resdirpath",
				"");
			if (strResDirPath != null)
			{
				object[] pList = { strResDirPath };

				this.BeginInvoke(new Delegate_ExpandResDir(ExpandResDir),
					pList);
			}

            checkBox_export_delete_CheckedChanged(null, null);
		}

        void Servers_ServerChanged(object sender, ServerChangedEventArgs e)
        {
            this.treeView_rangeRes.Refresh(ResTree.RefreshStyle.All);
        }

		public delegate void Delegate_ExpandResDir(string strResDirPath);

		void ExpandResDir(string strResDirPath)
		{
			this.toolStripStatusLabel_main.Text = "正在展开资源目录 " + strResDirPath + ", 请稍候...";
			this.Update();

			ResPath respath = new ResPath(strResDirPath);

			EnableControls(false);

			// 展开到指定的节点
			treeView_rangeRes.ExpandPath(respath);

			EnableControls(true);

			/*
			//Cursor.Current = Cursors.WaitCursor;
			dtlpResDirControl.ExpandPath(strResDirPath);
			//Cursor.Current = Cursors.Default;
			*/
            toolStripStatusLabel_main.Text = "";

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

		private void MainForm_Closed(object sender, System.EventArgs e)
		{

            this.Channels.AskAccountInfo -= new AskAccountInfoEventHandle(this.Servers.OnAskAccountInfo);

            this.Servers.ServerChanged -= new ServerChangedEventHandle(Servers_ServerChanged);

			// 保存到文件
			// parameters:
			//		strFileName	文件名。如果==null,表示使用装载时保存的那个文件名
			Servers.Save(null);
			Servers = null;

			string strError;
			int nRet = cfgCache.Save(null, out strError);
			if (nRet == -1)
				MessageBox.Show(this, strError);


			// 保存窗口尺寸状态
			if (AppInfo != null) 
			{
				AppInfo.SaveFormStates(this,
					"mainformstate");
			}

			AppInfo.SetString(
				"page_import",
				"source_file_name",
				this.textBox_import_fileName.Text);
			AppInfo.SetString(
				"page_import",
				"dbmap",
				this.textBox_import_dbMap.Text.Replace("\r\n",";"));
			AppInfo.SetString(
				"page_import",
				"range",
				this.textBox_import_range.Text);
            AppInfo.SetBoolean(
"page_import",
"fastmode",
this.checkBox_import_fastMode.Checked);


			AppInfo.SetString(
				"rangePage",
				"startNumber",
				textBox_startNo.Text);


			AppInfo.SetString(
				"rangePage",
				"endNumber",
				textBox_endNo.Text);


			AppInfo.SetInt(
				"rangePage",
				"verifyrange",
				Convert.ToInt32(checkBox_verifyNumber.Checked));

			AppInfo.SetInt(
				"rangePage",
				"forceloop",
				Convert.ToInt32(checkBox_forceLoop.Checked));

			AppInfo.SetInt(
				"rangePage",
				"delete",
				Convert.ToInt32(checkBox_export_delete.Checked));
            AppInfo.SetBoolean(
    "rangePage",
    "fastmode",
    this.checkBox_export_fastMode.Checked);

			AppInfo.SetInt(
				"rangePage",
				"all",
				Convert.ToInt32(this.radioButton_all.Checked));

			AppInfo.SetString(
				"rangePage",
				"lastoutputfilename",
				strLastOutputFileName);

			AppInfo.SetInt(
				"rangePage",
				"lastoutputfilterindex",
				nLastOutputFilterIndex);

			// 保存resdircontrol最后的选择

			ResPath respath = new ResPath(treeView_rangeRes.SelectedNode);
			AppInfo.SetString(
				"rangePage",
				"resdirpath",
				respath.FullPath);


			//记住save,保存信息XML文件
			AppInfo.Save();
			AppInfo = null;	// 避免后面再用这个对象		

		}


		#region 菜单命令

		private void menuItem_cfg_Click(object sender, System.EventArgs e)
		{
		
		}

		private void menuItem_exit_Click(object sender, System.EventArgs e)
		{
            this.Close();
		}

		#endregion

		private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			string strError = "";

			if (e.Button == toolBarButton_stop) 
			{
				stopManager.DoStopActive();
			}

			if (e.Button == toolBarButton_begin) 
			{
				// 出现对话框，询问Project名字
				GetProjectNameDlg dlg = new GetProjectNameDlg();
                MainForm.SetControlFont(dlg, this.DefaultFont);

				dlg.scriptManager = this.scriptManager;
				dlg.ProjectName = AppInfo.GetString(
					"main",
					"lastUsedProject",
					"");
				dlg.NoneProject = Convert.ToBoolean(AppInfo.GetInt(
					"main",
					"lastNoneProjectState",
					0));

				this.AppInfo.LinkFormState(dlg, "GetProjectNameDlg_state");
				dlg.ShowDialog(this);
				this.AppInfo.UnlinkFormState(dlg);


				if (dlg.DialogResult != DialogResult.OK)
					return;

				string strProjectName = "";
				string strLocate = "";	// 方案文件目录


				if (dlg.NoneProject == false)
				{
					// string strWarning = "";

					strProjectName = dlg.ProjectName;

					// 获得方案参数
					// strProjectNamePath	方案名，或者路径
					// return:
					//		-1	error
					//		0	not found project
					//		1	found
					int nRet = scriptManager.GetProjectData(
						strProjectName,
						out strLocate);

					if (nRet == 0) 
					{
						strError = "方案 " + strProjectName + " 没有找到...";
						goto ERROR1;
					}
					if (nRet == -1) 
					{
						strError = "scriptManager.GetProjectData() error ...";
						goto ERROR1;
					}
				}

				AppInfo.SetString(
					"main",
					"lastUsedProject",
					strProjectName);
				AppInfo.SetInt(
					"main",
					"lastNoneProjectState",
					Convert.ToInt32(dlg.NoneProject));



				if (tabControl_main.SelectedTab == this.tabPage_range)
				{
					this.DoExport(strProjectName, strLocate);
				}
				else if (tabControl_main.SelectedTab == this.tabPage_import)
				{
					this.DoImport(strProjectName, strLocate);
				}
				
			}

			return;

			ERROR1:
				MessageBox.Show(this, strError);
		}


        void DoImport(string strProjectName,
            string strProjectLocate)
        {
            string strError = "";
            int nRet = 0;

            Assembly assemblyMain = null;
            MyFilterDocument filter = null;
            this.MarcFilter = null;
            batchObj = null;
            m_nRecordCount = -1;

            // 准备脚本
            if (strProjectName != "" && strProjectName != null)
            {
                nRet = PrepareScript(strProjectName,
                    strProjectLocate,
                    out assemblyMain,
                    out filter,
                    out batchObj,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                this.AssemblyMain = assemblyMain;
                if (filter != null)
                    this.AssemblyFilter = filter.Assembly;
                else
                    this.AssemblyFilter = null;

                this.MarcFilter = filter;
            }

            // 执行脚本的OnInitial()

            // 触发Script中OnInitial()代码
            // OnInitial()和OnBegin的本质区别, 在于OnInitial()适合检查和设置面板参数
            if (batchObj != null)
            {
                BatchEventArgs args = new BatchEventArgs();
                batchObj.OnInitial(this, args);
                /*
                if (args.Continue == ContinueType.SkipBeginMiddle)
                    goto END1;
                if (args.Continue == ContinueType.SkipMiddle) 
                {
                    strError = "OnInitial()中args.Continue不能使用ContinueType.SkipMiddle.应使用ContinueType.SkipBeginMiddle";
                    goto ERROR1;
                }
                */
                if (args.Continue == ContinueType.SkipAll)
                    goto END1;
            }


            if (this.textBox_import_fileName.Text == "")
            {
                strError = "尚未指定输入文件名...";
                goto ERROR1;
            }
            FileInfo fi = new FileInfo(this.textBox_import_fileName.Text);
            if (fi.Exists == false)
            {
                strError = "文件" + this.textBox_import_fileName.Text + "不存在...";
                goto ERROR1;
            }

            OpenMarcFileDlg dlg = null;

            // ISO2709文件需要预先准备条件
            if (String.Compare(fi.Extension, ".iso", true) == 0
                || String.Compare(fi.Extension, ".mrc", true) == 0)
            {
                // 询问encoding和marcsyntax
                dlg = new OpenMarcFileDlg();
                MainForm.SetControlFont(dlg, this.DefaultFont);

                dlg.Text = "请指定要导入的 ISO2709 文件属性";
                dlg.FileName = this.textBox_import_fileName.Text;

                this.AppInfo.LinkFormState(dlg, "OpenMarcFileDlg_input_state");
                dlg.ShowDialog(this);
                this.AppInfo.UnlinkFormState(dlg);

                if (dlg.DialogResult != DialogResult.OK)
                    return;

                this.textBox_import_fileName.Text = dlg.FileName;
                this.CurMarcSyntax = dlg.MarcSyntax;
            }

            // 触发Script中OnBegin()代码
            // OnBegin()中仍然有修改MainForm面板的自由
            if (batchObj != null)
            {
                BatchEventArgs args = new BatchEventArgs();
                batchObj.OnBegin(this, args);
                /*
                if (args.Continue == ContinueType.SkipMiddle)
                    goto END1;
                if (args.Continue == ContinueType.SkipBeginMiddle)
                    goto END1;
                */
                if (args.Continue == ContinueType.SkipAll)
                    goto END1;
            }

            if (String.Compare(fi.Extension, ".dp2bak", true) == 0)
                nRet = this.DoImportBackup(this.textBox_import_fileName.Text,
                    out strError);

            else if (String.Compare(fi.Extension, ".xml", true) == 0)
                nRet = this.DoImportXml(this.textBox_import_fileName.Text,
                    out strError);

            else if (String.Compare(fi.Extension, ".iso", true) == 0
                || String.Compare(fi.Extension, ".mrc", true) == 0)
            {

                this.CheckTargetDb += new CheckTargetDbEventHandler(CheckTargetDbCallBack);

                try
                {
                    nRet = this.DoImportIso2709(dlg.FileName,
                        dlg.MarcSyntax,
                        dlg.Encoding,
                        out strError);
                }
                finally
                {
                    this.CheckTargetDb -= new CheckTargetDbEventHandler(CheckTargetDbCallBack);
                }

            }
            else
            {
                strError = "未知的文件类型...";
                goto ERROR1;
            }


        END1:
            // 触发Script的OnEnd()代码
            if (batchObj != null)
            {
                BatchEventArgs args = new BatchEventArgs();
                batchObj.OnEnd(this, args);
            }

            // END2:

            this.AssemblyMain = null;
            this.AssemblyFilter = null;
            if (filter != null)
                filter.Assembly = null;


            if (strError != "")
                MessageBox.Show(this, strError);

            this.MarcFilter = null;
            return;

        ERROR1:
            this.AssemblyMain = null;
            this.AssemblyFilter = null;
            if (filter != null)
                filter.Assembly = null;

            this.MarcFilter = null;

            MessageBox.Show(this, strError);
        }


		// 导入XML数据
		// parameter: 
		//		strFileName: 要导入的源XML文件
		// 说明: 导入数据是一个连续的过程,
		//		只要依据流的自然顺序依次上载每个记录就可以了。
		int DoImportXml(string strFileName,
			out string strError)
		{
			int nRet;
			strError = "";

            bool bFastMode = this.checkBox_import_fastMode.Checked;

			this.bNotAskTimestampMismatchWhenOverwrite = false;	// 要询问

			// 准备库名对照表
			DbNameMap map = DbNameMap.Build(this.textBox_import_dbMap.Text.Replace("\r\n", ";"),
                out strError);
            if (map == null)
                return -1;


			Stream file = File.Open(strFileName,
				FileMode.Open,
				FileAccess.Read);

			XmlTextReader reader = new XmlTextReader(file);

			//
			RangeList rl = null;
			long lMax = 0;
			long lMin = 0;
			long lSkipCount = 0;
			int nReadRet = 0;
			string strCount = "";

			//范围
			if (textBox_import_range.Text != "") 
			{
				rl = new RangeList(textBox_import_range.Text);
				rl.Sort();
				rl.Merge();
				lMin = rl.min();
				lMax = rl.max();
			}

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在导入");
            stop.BeginLoop();

            stop.SetProgressRange(0, file.Length);

            EnableControls(false);

            WriteLog("开始导入XML数据");

			try
			{
				bool bRet = false;
			
                // 移动到根元素
				while(true) 
				{
					bRet = reader.Read();
					if (bRet == false) 
					{
						strError = "没有根元素";
						goto ERROR1;
					}
					if (reader.NodeType == XmlNodeType.Element)
						break;
				}

                // 移动到其下级第一个element
                while (true)
                {
                    bRet = reader.Read();
                    if (bRet == false)
                    {
                        strError = "没有第一个记录元素";
                        goto END1;
                    }
                    if (reader.NodeType == XmlNodeType.Element)
                        break;
                }

				this.m_nRecordCount = 0;

				for(long lCount = 0;;lCount ++)
				{
					bool bSkip = false;
					nReadRet = 0;


					Application.DoEvents();	// 出让界面控制权

					if (stop.State != 0)
					{
						DialogResult result = MessageBox.Show(this,
							"确实要中断当前批处理操作?",
							"dp2batch",
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Question,
							MessageBoxDefaultButton.Button2);
						if (result == DialogResult.Yes)
						{
							strError = "用户中断";
							nReadRet = 100;
							goto ERROR1;
						}
						else 
						{
							stop.Continue();
						}
					}


					//检索当前记录是否在处理范围内
					if (rl != null) 
					{
						if (lMax != -1) // -1:不确定
						{
							if (lCount > lMax)
								nReadRet = 2;	// 后面看到这个状态将会break。为什么不在这里break，就是为了后面显示label信息
						}
						if (rl.IsInRange(lCount, true) == false) 
						{
							bSkip = true;
						}
					}


					// progressBar_main.Value = (int)((file.Position)/ProgressRatio);
                    stop.SetProgressValue(file.Position);

					// 显示信息
					if (bSkip == true) 
					{
						stop.SetMessage( ((bSkip == true) ? "正在跳过 " : "正在处理" )
							+ Convert.ToString(lCount+1) );
					}

					/*
					if (nReadRet == 2)
						goto CONTINUE;

					if (bSkip == true)
						goto CONTINUE;
					*/


					/*
					// 防止一条记录也没有的情况,所以把这个句写到前面
					if (file.Position >= file.Length)
						break;
					*/

					// 上载一个Item
					nRet = DoXmlItemUpload(
                        bFastMode,
                        reader,
						map,
						bSkip == true || nReadRet == 2,
						strCount,
						out strError);
					if (nRet == -1)
						goto ERROR1;
					if (nRet == 1)
						break;

					strCount = "处理数 "
						+ Convert.ToString(lCount - lSkipCount)
						+ "　/ 跳过数 " 
						+ Convert.ToString(lSkipCount);


					if (bSkip)
						lSkipCount ++;

					if (nReadRet == 1 || nReadRet == 2)  //判断大文件结束
						break;

				}

    		}
			finally
			{
				file.Close();

                WriteLog("结束导入XML数据");
			}

        END1:

            stop.EndLoop();
            stop.OnStop -= new StopEventHandler(this.DoStop);
            stop.Initial("");

            EnableControls(true);

            strError = "恢复数据文件 '" + strFileName + "' 完成。";
            return 0;
        ERROR1:
            stop.EndLoop();
            stop.OnStop -= new StopEventHandler(this.DoStop);
            stop.Initial("");
            EnableControls(true);

            if (nReadRet == 100)
                strError = "恢复数据文件 '" + strFileName + "' 被中断。原因： " + strError;
            else
                strError = "恢复数据文件 '" + strFileName + "' 失败。原因: " + strError;
            return -1;
		}


		// 导入ISO2709数据
		// parameter: 
		//		strFileName: 要导入的源ISO2709文件
		int DoImportIso2709(string strFileName,
			string strMarcSyntax,
			Encoding encoding,
			out string strError)
		{
			int nRet;
			strError = "";

            bool bFastMode = this.checkBox_import_fastMode.Checked;

			this.CurMarcSyntax = strMarcSyntax;	// 为C#脚本调用GetMarc()等函数提供条件

			this.bNotAskTimestampMismatchWhenOverwrite = false;	// 要询问

			// 准备库名对照表
			DbNameMap map = DbNameMap.Build(this.textBox_import_dbMap.Text.Replace("\r\n",";"),
                out strError);
            if (map == null)
            {
                strError = "根据库名映射规则创建库名对照表时出错: " + strError;
                return -1;
            }

			Stream file = File.Open(strFileName,
				FileMode.Open,
				FileAccess.Read);

			//
			RangeList rl = null;
			long lMax = 0;
			long lMin = 0;
			long lSkipCount = 0;
			int nReadRet = 0;
			string strCount = "";

			//范围
			if (textBox_import_range.Text != "") 
			{
				rl = new RangeList(textBox_import_range.Text);
				rl.Sort();
				rl.Merge();
				lMin = rl.min();
				lMax = rl.max();
			}

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在导入");
            stop.BeginLoop();

            stop.SetProgressRange(0, file.Length);

            EnableControls(false);

            WriteLog("开始导入ISO2709格式数据");

			try
			{



				// bool bRet = false;

				this.m_nRecordCount = 0;


				for(long lCount = 0;;lCount ++)
				{
					bool bSkip = false;
					nReadRet = 0;


					Application.DoEvents();	// 出让界面控制权


					if (stop.State != 0)
					{
						DialogResult result = MessageBox.Show(this,
							"确实要中断当前批处理操作?",
							"dp2batch",
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Question,
							MessageBoxDefaultButton.Button2);
						if (result == DialogResult.Yes)
						{
							strError = "用户中断";
							nReadRet = 100;
							goto ERROR1;
						}
						else 
						{
							stop.Continue();
						}
					}


					//检索当前记录是否在处理范围内
					if (rl != null) 
					{
						if (lMax != -1) // -1:不确定
						{
							if (lCount > lMax)
								nReadRet = 2;	// 后面看到这个状态将会break。为什么不在这里break，就是为了后面显示label信息
						}
						if (rl.IsInRange(lCount, true) == false) 
						{
							bSkip = true;
						}
					}


					// progressBar_main.Value = (int)((file.Position)/ProgressRatio);
                    stop.SetProgressValue(file.Position);

					// 显示信息
					if (bSkip == true) 
					{
						stop.SetMessage( ((bSkip == true) ? "正在跳过 " : "正在处理" )
							+ Convert.ToString(lCount+1) );
					}


					/*
					// 防止一条记录也没有的情况,所以把这个句写到前面
					if (file.Position >= file.Length)
						break;
					*/

					string strMARC = "";

					// 从ISO2709文件中读入一条MARC记录
					// return:
					//	-2	MARC格式错
					//	-1	出错
					//	0	正确
					//	1	结束(当前返回的记录有效)
					//	2	结束(当前返回的记录无效)
					nRet = MarcUtil.ReadMarcRecord(file, 
						encoding,
						true,	// bRemoveEndCrLf,
						true,	// bForce,
						out strMARC,
						out strError);
                    if (nRet == -2 || nRet == -1)
                    {
                        DialogResult result = MessageBox.Show(this,
                            "读入MARC记录(" + lCount .ToString()+ ")出错: " + strError + "\r\n\r\n确实要中断当前批处理操作?",
                            "dp2batch",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                        if (result == DialogResult.Yes)
                        {
                            break;
                        }
                        else
                            continue;
                    }

					if (nRet != 0 && nRet != 1)
						break;

					if (this.batchObj != null)
					{
						batchObj.MarcRecord = strMARC;
						batchObj.MarcRecordChanged = false;
						batchObj.MarcSyntax = strMarcSyntax;
					}

					string strXml = "";

					// 将MARC记录转换为xml格式
					nRet = MarcUtil.Marc2Xml(strMARC,
						strMarcSyntax,
						out strXml,
						out strError);
					if (nRet == -1)
						goto ERROR1;

                    // TODO: 能利用MARC记录中的-01字段，进行覆盖操作
                    // 难点有两个：1)原来有若干个-01，要根据条件(指定的目标库)筛选出一个 2) dt1000的-01中只能库名，无法容纳web service url名

					// 上载一个Item
					nRet = DoXmlItemUpload(
                        bFastMode,
                        strXml,
						map,
						bSkip == true || nReadRet == 2,
						strCount,
						out strError);
					if (nRet == -1)
						goto ERROR1;
					if (nRet == 1)
						break;

					strCount = "处理数 "
						+ Convert.ToString(lCount - lSkipCount)
						+ "　/ 跳过数 " 
						+ Convert.ToString(lSkipCount);


					if (bSkip)
						lSkipCount ++;

					if (nReadRet == 1 || nReadRet == 2)  //判断大文件结束
						break;

				}
			}
			finally
			{
				file.Close();

                WriteLog("结束导入ISO2709格式数据");
			}

            stop.EndLoop();
            stop.OnStop -= new StopEventHandler(this.DoStop);
            stop.Initial("");

            EnableControls(true);

            strError = "恢复数据文件 '" + strFileName + "' 完成。";
            return 0;
        ERROR1:
            stop.EndLoop();
            stop.OnStop -= new StopEventHandler(this.DoStop);
            stop.Initial("");
            EnableControls(true);

            if (nReadRet == 100)
                strError = "恢复数据文件 '" + strFileName + "' 被中断: " + strError;
            else
                strError = "恢复数据文件 '" + strFileName + "' 出错: " + strError;
            return -1;
		}

		// 上载一个item
		// parameter:
		//		strError: error info
		// return:
		//		-1	出错
		//		0	正常
		//		1	结束
		public int DoXmlItemUpload(
            bool bFastMode,
            XmlTextReader reader,
			DbNameMap map,
			bool bSkip,
			string strCount,
			out string strError)
		{
			strError = "";
			bool bRet = false;
			
			while(true) 
			{
                if (reader.NodeType == XmlNodeType.Element)
                    break;
                bRet = reader.Read();
				if (bRet == false)
					return 1;
			}

            /*
			if (bRet == false)
				return 1;	// 结束
             * */


			string strXml = reader.ReadOuterXml();

			return DoXmlItemUpload(
                bFastMode,
                strXml,
				map,
				bSkip,
				strCount,
				out strError);
		}

		public SearchPanel SearchPanel
		{
			get 
			{
				SearchPanel searchpanel = new SearchPanel();
				searchpanel.Initial(this.Servers,
					this.cfgCache);

                // 此时searchpanel.ServerUrl未定

				return searchpanel;
			}
		}

		// 检查目标库事件
		void CheckTargetDbCallBack(object sender,
			CheckTargetDbEventArgs e)
		{
			string strMarcSyntax = (string)m_tableMarcSyntax[e.DbFullPath];

			if (strMarcSyntax == null)
			{
				string strError = "";

				// 从marcdef配置文件中获得marc格式定义
				// return:
				//		-1	出错
				//		0	没有找到
				//		1	找到
				int nRet = this.SearchPanel.GetMarcSyntax(e.DbFullPath,
					out strMarcSyntax,
					out strError);
				if (nRet == 0 || nRet == -1)
				{
					e.Cancel = true;
					e.ErrorInfo = strError;
					return;
				}

				m_tableMarcSyntax[e.DbFullPath] = strMarcSyntax;
			}

            // if (String.Compare(this.CurMarcSyntax, strMarcSyntax, true) != 0)
            if (String.Compare(e.CurrentMarcSyntax, strMarcSyntax, true) != 0)
            {
                e.Cancel = true;
                // e.ErrorInfo = "您选择的 MARC 格式 '" + this.CurMarcSyntax + "' 和目标库 '" + e.DbFullPath + "' 中的 cfgs/marcdef 配置文件中定义的 MARC 格式 '" + strMarcSyntax + "' 不吻合, 操作被迫中断";
                e.ErrorInfo = "您选择的 MARC 格式 '" + e.CurrentMarcSyntax + "' 和目标库 '" + e.DbFullPath + "' 中的 cfgs/marcdef 配置文件中定义的 MARC 格式 '" + strMarcSyntax + "' 不吻合, 操作被迫中断";
                return;
            }
		}

		// 覆盖一条XML记录
		int DoOverwriteXmlRecord(
            bool bFastMode,
            string strRecFullPath,
			string strXmlBody,
			byte [] timestamp,
			out string strError)
		{
			strError = "";

			ResPath respath = new ResPath(strRecFullPath);

            RmsChannel channelSave = channel;

			channel = this.Channels.GetChannel(respath.Url);

			try 
			{

				string strWarning = "";
				byte [] output_timestamp = null;
				string strOutputPath = "";

			REDOSAVE:

				// 保存Xml记录
				long lRet = channel.DoSaveTextRes(respath.Path,
					strXmlBody,
					false,	// bIncludePreamble
					bFastMode == true ? "fastmode" : "",//strStyle,
					timestamp,
					out output_timestamp,
					out strOutputPath,
					out strError);

				if (lRet == -1) 
				{
					if (stop != null) 
						stop.Continue();

					if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
					{
                        string strDisplayRecPath = strOutputPath;
                        if (string.IsNullOrEmpty(strDisplayRecPath) == true)
                            strDisplayRecPath = respath.Path;

						if (this.bNotAskTimestampMismatchWhenOverwrite == true) 
						{
							timestamp = new byte[output_timestamp.Length];
							Array.Copy(output_timestamp, 0, timestamp, 0, output_timestamp.Length);
							strWarning = " (时间戳不匹配, 自动重试)";
							goto REDOSAVE;
						}


						DialogResult result = MessageDlg.Show(this,
                            "上载 '" + strDisplayRecPath  
							+" 时发现时间戳不匹配。详细情况如下：\r\n---\r\n"
							+ strError + "\r\n---\r\n\r\n是否以新时间戳强行上载?\r\n注：(是)强行上载 (否)忽略当前记录或资源上载，但继续后面的处理 (取消)中断整个批处理",
							"dp2batch",
							MessageBoxButtons.YesNoCancel,
							MessageBoxDefaultButton.Button1,
							ref this.bNotAskTimestampMismatchWhenOverwrite);
						if (result == DialogResult.Yes) 
						{
							timestamp = new byte[output_timestamp.Length];
							Array.Copy(output_timestamp, 0, timestamp, 0, output_timestamp.Length);
							strWarning = " (时间戳不匹配, 应用户要求重试)";
							goto REDOSAVE;
						}

						if (result == DialogResult.No) 
						{
							return 0;	// 继续作后面的资源
						}

						if (result == DialogResult.Cancel) 
						{
							strError = "用户中断";
							goto ERROR1;	// 中断整个处理
						}
					}

					// 询问是否重试
					DialogResult result1 = MessageBox.Show(this, 
						"上载 '" + respath.Path  
						+" 时发生错误。详细情况如下：\r\n---\r\n"
						+ strError + "\r\n---\r\n\r\n是否重试?\r\n注：(是)重试 (否)不重试，但继续后面的处理 (取消)中断整个批处理",
						"dp2batch",
						MessageBoxButtons.YesNoCancel,
						MessageBoxIcon.Question,
						MessageBoxDefaultButton.Button1);
					if (result1 == DialogResult.Yes) 
						goto REDOSAVE;
					if (result1 == DialogResult.No) 
						return 0;	// 继续作后面的资源


					goto ERROR1;
				}

				return 0;
			ERROR1:
				return -1;
			}
			finally 
			{
				channel = channelSave;
			}
		}

		// 上载一个item
		// parameter:
		//		strError: error info
		// return:
		//		-1	出错
		//		0	正常
		//		1	结束
		public int DoXmlItemUpload(
            bool bFastMode,
            string strXml,
			DbNameMap map,
			bool bSkip,
			string strCount,
			out string strError)
		{
			strError = "";
            int nRet = 0;
			// bool bRet = false;
			
			// MessageBox.Show(this, strXml);

			if (bSkip == true)
				return 0;

			XmlDocument dataDom = new XmlDocument();
			try
			{
				dataDom.LoadXml(strXml);
			}
			catch(Exception ex)
			{
				strError = "加载数据到dom出错!\r\n" + ex.Message;
				goto ERROR1;
			}

			XmlNode node = dataDom.DocumentElement;

			string strResPath = DomUtil.GetAttr(DpNs.dprms, node,"path");

			string strTargetPath = "";

            string strSourceDbPath = "";

            if (strResPath != "")
            {
                // 从map中查询覆盖还是追加？
                ResPath respath0 = new ResPath(strResPath);
                respath0.MakeDbName();
                strSourceDbPath = respath0.FullPath;
            }

        REDO:

            DbNameMapItem mapItem = null;


            mapItem = map.MatchItem(strSourceDbPath/*strResPath*/);
            if (mapItem != null)
                goto MAPITEMOK;

            if (mapItem == null)
            {

                if (strSourceDbPath/*strResPath*/ == "")
                {
                    string strText = "源数据文件中记录 " + Convert.ToString(this.m_nRecordCount) + " 没有来源数据库,对所有这样的数据,将作如何处理?";
                    WriteLog("打开对话框 '" + strText.Replace("\r\n", "\\n") + "'");
                    nRet = DbNameMapItemDlg.AskNullOriginBox(
                        this,
                        this.AppInfo,
                        strText,
                        this.SearchPanel,
                        map);
                    WriteLog("关闭对话框 '" + strText.Replace("\r\n", "\\n") + "'"); 

                    if (nRet == 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;	// 中断整个处理
                    }

                    goto REDO;

                }
                else
                {
                    string strText = "源数据文件中记录 " + Convert.ToString(this.m_nRecordCount) + " 的来源数据库 '" + strSourceDbPath/*strResPath*/ + "' 没有找到对应的目标库, 对所有这样的数据,将作如何处理?";
                    WriteLog("打开对话框 '" + strText.Replace("\r\n", "\\n") + "'");
                    nRet = DbNameMapItemDlg.AskNotMatchOriginBox(
                        this,
                        this.AppInfo,
                        strText,
                        this.SearchPanel,
                        strSourceDbPath/*strResPath*/,
                        map);
                    WriteLog("关闭对话框 '" + strText.Replace("\r\n", "\\n") + "'");
                    if (nRet == 0)
                    {
                        strError = "用户中断";
                        goto ERROR1;	// 中断整个处理
                    }

                    goto REDO;
                }
            }

        MAPITEMOK:

            if (mapItem.Style == "skip")
                return 0;

            // 构造目标路径

            // 1)从源路径中提取id。源路径来自备份文件数据
            ResPath respath = new ResPath(strResPath);
            string strID = respath.GetRecordId();

            if (strID == null || strID == ""
                || (mapItem.Style == "append")
                )
            {
                strID = "?";	// 将来加一个对话框
            }

			// 2)用目标库路径构造完整的记录路径
			string strTargetFullPath = "";
			if (mapItem.Target == "*") 
			{
				// 此时target为*, 需要从strResPath中获得库名

				if (strResPath == "")
				{
					Debug.Assert(false, "不可能出现的情况");
				}

				respath = new ResPath(strResPath);
				respath.MakeDbName();
				strTargetFullPath = respath.FullPath;
			}
			else 
			{
				strTargetFullPath = mapItem.Target;
			}

			respath = new ResPath(strTargetFullPath);


			// 需要检查目标库所允许的MARC格式
			if (CheckTargetDb != null)
			{
				CheckTargetDbEventArgs e = new CheckTargetDbEventArgs();
				e.DbFullPath = strTargetFullPath;
                e.CurrentMarcSyntax = this.CurMarcSyntax;
				this.CheckTargetDb(this, e);
				if (e.Cancel == true)
				{
					if (e.ErrorInfo == "")
						strError = "CheckTargetDb 事件导致中断";
					else
						strError = e.ErrorInfo;
					return -1;
				}

			}


			strTargetPath = respath.Path + "/" + strID;
			// strRecordPath = strTargetPath;

			channel = this.Channels.GetChannel(respath.Url);

			string strTimeStamp = DomUtil.GetAttr(DpNs.dprms, node,"timestamp");

			byte [] timestamp = ByteArray.GetTimeStampByteArray(strTimeStamp);

            // 2012/5/29
            string strOutMarcSyntax = "";
            string strMARC = "";
            // 将MARCXML格式的xml记录转换为marc机内格式字符串
            // parameters:
            //		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
            //		strMarcSyntax	指示marc语法,如果==""，则自动识别
            //		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
            nRet = MarcUtil.Xml2Marc(strXml,
                false,
                "",
                out strOutMarcSyntax,
                out strMARC,
                out strError);
            /*
            if (nRet == -1)
                return -1;
             * */

            // 2012/5/30
            if (batchObj != null)
            {
                batchObj.MarcSyntax = strOutMarcSyntax;
                batchObj.MarcRecord = strMARC;
                batchObj.MarcRecordChanged = false;	// 为本轮Script运行准备初始状态
            }


			if (this.MarcFilter != null)
			{
				// 触发filter中的Record相关动作
				nRet = MarcFilter.DoRecord(
					null,
					batchObj.MarcRecord,
					m_nRecordCount,
					out strError);
				if (nRet == -1) 
					goto ERROR1;
			}

			// C#脚本 -- Inputing
			if (this.AssemblyMain != null) 
			{
				// 这些变量要先初始化,因为filter代码可能用到这些Batch成员.
				batchObj.SkipInput = false;
				batchObj.XmlRecord = strXml;

				//batchObj.MarcSyntax = this.CurMarcSyntax;
				//batchObj.MarcRecord = strMarc;	// MARC记录体
				//batchObj.MarcRecordChanged = false;	// 为本轮Script运行准备初始状态


				batchObj.SearchPanel.ServerUrl = channel.Url;
				batchObj.ServerUrl = channel.Url;
				batchObj.RecPath = strTargetPath;	// 记录路径
				batchObj.RecIndex = m_nRecordCount;	// 当前记录在一批中的序号
				batchObj.TimeStamp = timestamp;


				BatchEventArgs args = new BatchEventArgs();

				batchObj.Inputing(this, args);
				if (args.Continue == ContinueType.SkipAll)
				{
					strError = "脚本中断SkipAll";
					goto END2;
				}

				if (batchObj.SkipInput == true)
					return 0;	// 继续处理后面的
			}


			string strWarning = "";
			byte [] output_timestamp = null;
			string strOutputPath = "";

			REDOSAVE:
				if (stop != null) 
				{
					if (strTargetPath.IndexOf("?") == -1)
					{
						stop.SetMessage("正在上载 " 
							+ strTargetPath + strWarning + " " + strCount);
					}
				}


			// 保存Xml记录
			long lRet = channel.DoSaveTextRes(strTargetPath,
				strXml,
				false,	// bIncludePreamble
                    bFastMode == true ? "fastmode" : "",//strStyle,
                timestamp,
				out output_timestamp,
				out strOutputPath,
				out strError);

			if (lRet == -1)
            {
                if (stop != null)
                    stop.Continue();

                if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
                {
                    string strDisplayRecPath = strOutputPath;
                    if (string.IsNullOrEmpty(strDisplayRecPath) == true)
                        strDisplayRecPath = strTargetPath;

                    if (this.bNotAskTimestampMismatchWhenOverwrite == true)
                    {
                        timestamp = new byte[output_timestamp.Length];
                        Array.Copy(output_timestamp, 0, timestamp, 0, output_timestamp.Length);
                        strWarning = " (时间戳不匹配, 自动重试)";
                        goto REDOSAVE;
                    }

                    string strText = "上载 '" + strDisplayRecPath
                        + " 时发现时间戳不匹配。详细情况如下：\r\n---\r\n"
                        + strError + "\r\n---\r\n\r\n是否以新时间戳强行上载?\r\n注：(是)强行上载 (否)忽略当前记录或资源上载，但继续后面的处理 (取消)中断整个批处理";
                    WriteLog("打开对话框 '" + strText.Replace("\r\n", "\\n") + "'");
                    DialogResult result = MessageDlg.Show(this,
                        strText,
                        "dp2batch",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxDefaultButton.Button1,
                        ref this.bNotAskTimestampMismatchWhenOverwrite);
                    WriteLog("关闭对话框 '" + strText.Replace("\r\n", "\\n") + "'");
                    if (result == DialogResult.Yes)
                    {
                        timestamp = new byte[output_timestamp.Length];
                        Array.Copy(output_timestamp, 0, timestamp, 0, output_timestamp.Length);
                        strWarning = " (时间戳不匹配, 应用户要求重试)";
                        goto REDOSAVE;
                    }

                    if (result == DialogResult.No)
                    {
                        return 0;	// 继续作后面的资源
                    }

                    if (result == DialogResult.Cancel)
                    {
                        strError = "用户中断";
                        goto ERROR1;	// 中断整个处理
                    }
                }

                // 询问是否重试
                {
                    string strText = "上载 '" + strTargetPath
                        + " 时发生错误。详细情况如下：\r\n---\r\n"
                        + strError + "\r\n---\r\n\r\n是否重试?\r\n注：(是)重试 (否)不重试，但继续后面的处理 (取消)中断整个批处理";
                    WriteLog("打开对话框 '" + strText.Replace("\r\n", "\\n") + "'");

                    DialogResult result1 = MessageBox.Show(this,
                        strText,
                        "dp2batch",
                        MessageBoxButtons.YesNoCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                    WriteLog("关闭对话框 '" + strText.Replace("\r\n", "\\n") + "'");
                    if (result1 == DialogResult.Yes)
                        goto REDOSAVE;
                    if (result1 == DialogResult.No)
                        return 0;	// 继续作后面的资源
                }

                goto ERROR1;
            }

			// C#脚本 -- Inputed()
			if (this.AssemblyMain != null) 
			{
				// 大部分变量保留刚才Inputing()时的原样，只修改部分

				batchObj.RecPath = strOutputPath;	// 记录路径
				batchObj.TimeStamp = output_timestamp;

				BatchEventArgs args = new BatchEventArgs();

				batchObj.Inputed(this, args);
                /*
                if (args.Continue == ContinueType.SkipMiddle)
                {
                    strError = "脚本中断SkipMiddle";
                    goto END1;
                }
                if (args.Continue == ContinueType.SkipBeginMiddle)
                {
                    strError = "脚本中断SkipBeginMiddle";
                    goto END1;
                }
                */
                if (args.Continue == ContinueType.SkipAll)
                {
                    strError = "脚本中断SkipAll";
                    goto END1;
                }
            }

            this.m_nRecordCount++;

            if (stop != null)
            {
                stop.SetMessage("已上载成功 '"
                    + strOutputPath + "' " + strCount);
            }


            // strRecordPath = strOutputPath;

            return 0;
        END1:
        END2:

        ERROR1:
            return -1;
        }

		// 导入数据
		// parameter: 
		//		strFileName: 要恢复的源备份文件
		// 说明: 导入数据是一个连续的过程,
		//		只要依据流的自然顺序依次上载每个记录就可以了。
		int DoImportBackup(string strFileName,
			out string strError)
		{
			int nRet;
			strError = "";

			this.bNotAskTimestampMismatchWhenOverwrite = false;	// 要询问

			// 准备库名对照表
			DbNameMap map = DbNameMap.Build(this.textBox_import_dbMap.Text.Replace("\r\n", ";"),
                out strError);
            if (map == null)
                return -1;


			Stream file = File.Open(strFileName,
				FileMode.Open,
				FileAccess.Read);

			//
			RangeList rl = null;
			long lMax = 0;
			long lMin = 0;
			long lSkipCount = 0;
			int nReadRet = 0;
			string strCount = "";

			//范围
			if (textBox_import_range.Text != "") 
			{
				rl = new RangeList(textBox_import_range.Text);
				rl.Sort();
				rl.Merge();
				lMin = rl.min();
				lMax = rl.max();
			}

            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在导入");
            stop.BeginLoop();

            stop.SetProgressRange(0, file.Length);

            EnableControls(false);

            WriteLog("开始导入.dp2bak格式数据");

			try
			{


				this.m_nRecordCount = 0;


				for(long lCount = 0;;lCount ++)
				{
					bool bSkip = false;
					nReadRet = 0;

					Application.DoEvents();	// 出让界面控制权

					if (stop.State != 0)
					{
						DialogResult result = MessageBox.Show(this,
							"确实要中断当前批处理操作?",
							"dp2batch",
							MessageBoxButtons.YesNo,
							MessageBoxIcon.Question,
							MessageBoxDefaultButton.Button2);
						if (result == DialogResult.Yes)
						{
							strError = "用户中断";
							nReadRet = 100;
							goto ERROR1;
						}
						else 
						{
							stop.Continue();
						}
					}


					//检索当前记录是否在处理范围内
					if (rl != null) 
					{
						if (lMax != -1) // -1:不确定
						{
							if (lCount > lMax)
								nReadRet = 2;	// 后面看到这个状态将会break。为什么不在这里break，就是为了后面显示label信息
						}
						if (rl.IsInRange(lCount, true) == false) 
						{
							bSkip = true;
						}
					}

					// progressBar_main.Value = (int)((file.Position)/ProgressRatio);
                    stop.SetProgressValue(file.Position);

					// 显示信息
					if (bSkip == true) 
					{
						stop.SetMessage( ((bSkip == true) ? "正在跳过 " : "正在处理" )
							+ Convert.ToString(lCount+1) );
					}

					// 防止一条记录也没有的情况,所以把这个句写到前面
					if (file.Position >= file.Length)
						break;

					// 上载一个Item
					nRet = DoBackupItemUpload(file,
						ref map,
						bSkip == true || nReadRet == 2,
						strCount,
						out strError);
					if (nRet == -1)
						goto ERROR1;

                    Debug.Assert(file.Position <= file.Length,
                        "经过DoBackupItemUpload()方法后, file的当前位置处在非法位置");


					if (bSkip)
						lSkipCount ++;

					strCount = "处理数 "
						+ Convert.ToString(lCount - lSkipCount)
						+ "　/ 跳过数 " 
						+ Convert.ToString(lSkipCount);

					if (nReadRet == 1 || nReadRet == 2)  //判断大文件结束
						break;

				}
			}
			finally
			{
				file.Close();

                WriteLog("结束导入.dp2bak格式数据");
			}

            stop.EndLoop();
            stop.OnStop -= new StopEventHandler(this.DoStop);
            stop.Initial("");

            EnableControls(true);

            strError = "恢复数据文件 '" + strFileName + "' 完成。";
            return 0;
        ERROR1:
            stop.EndLoop();
            stop.OnStop -= new StopEventHandler(this.DoStop);
            stop.Initial("");
            EnableControls(true);

            if (nReadRet == 100)
                strError = "恢复数据文件 '" + strFileName + "' 被中断: " + strError;
            else
                strError = "恢复数据文件 '" + strFileName + "' 出错: " + strError;
            return -1;



		}


		// 上载一个item
		// parameter:
		//		file:     源数据文件流
		//		strError: error info
		// return:
		//		-1: error
		//		0:  successed
		public int DoBackupItemUpload(Stream file,
			ref DbNameMap map,  // 2007/6/5 new add
			bool bSkip,
			string strCount,
			out string strError)
		{
			strError = "";

			long lStart = file.Position;

			byte [] data = new byte[8];
			int nRet = file.Read(data, 0 , 8);
			if (nRet == 0)
				return 1;	// 已经结束
			if (nRet < 8) 
			{
				strError = "read file error...";
				return -1;
			}

			// 毛长度
			long lLength = BitConverter.ToInt64(data, 0);   // +8可能是一个bug!!!

			if (bSkip == true)
			{
				file.Seek(lLength, SeekOrigin.Current);
				return 0;
			}

			this.channel = null;

			string strRecordPath = "";

			for(int i=0;;i++)
			{
				Application.DoEvents();	// 出让界面控制权

				if (stop.State != 0)
				{
					DialogResult result = MessageBox.Show(this,
						"确实要中断当前批处理操作?",
						"dp2batch",
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question,
						MessageBoxDefaultButton.Button2);
					if (result == DialogResult.Yes)
					{
						strError = "用户中断";
						return -1;
					}
					else 
					{
						stop.Continue();
					}
				}


				// progressBar_main.Value = (int)((file.Position)/ProgressRatio);
                stop.SetProgressValue(file.Position);


				if (file.Position - lStart >= lLength+8)    // 2006/8/29 changed
					break;

				// 上载对象资源
				nRet = this.DoResUpload(
					ref this.channel,
					ref strRecordPath,
					file,
					ref map,    // 2007/6/5 new add ref
					i==0? true : false,
					strCount,
					out strError);
				if (nRet == -1)
					return -1;
			}

			return 0;
		}


		// 上载一个res
		// parameter: 
		//		inputfile:   源流
		//		bIsFirstRes: 是否是第一个资源(xml)
		//		strError:    error info
		// return:
		//		-2	片断中发现时间戳不匹配。本函数调主可重上载整个资源
		//		-1	error
		//		0	successed
		public int DoResUpload(
            ref RmsChannel channel,
			ref string strRecordPath,
			Stream inputfile,
			ref DbNameMap map,
			bool bIsFirstRes,
			string strCount,
			out string strError)
		{
			strError = "";
			
			int nRet;
			long lBodyStart = 0;
			long lBodyLength = 0;

			// 1. 从输入流中得到strMetadata,与body(body放到一个临时文件里)
			string strMetaDataXml = "";

			nRet = GetResInfo(inputfile,
				bIsFirstRes,
				out strMetaDataXml,
				out lBodyStart,
				out lBodyLength,
				out strError);
			if (nRet == -1)
				goto ERROR1; 

			if (lBodyLength == 0)
				return 0;	// 空包不需上载
			

			// 2.为上载做准备
			XmlDocument metadataDom = new XmlDocument();
			try
			{
				metadataDom.LoadXml(strMetaDataXml);
			}
			catch(Exception ex)
			{
				strError = "加载元数据到dom出错!\r\n" + ex.Message;
				goto ERROR1;
			}

			XmlNode node = metadataDom.DocumentElement;

			string strResPath = DomUtil.GetAttr(node,"path");

			string strTargetPath = "";

			if (bIsFirstRes == true) // 第一个资源
			{
				// 从map中查询覆盖还是追加？
				ResPath respath = new ResPath(strResPath);
				respath.MakeDbName();

			REDO:
				DbNameMapItem mapItem = (DbNameMapItem)map["*"];
				if (mapItem != null)
				{
				}
				else 
				{
					mapItem = (DbNameMapItem)map[respath.FullPath.ToUpper()];
				}

				if (mapItem == null) 
				{
					OriginNotFoundDlg dlg = new OriginNotFoundDlg();
                    MainForm.SetControlFont(dlg, this.DefaultFont);

					dlg.Message = "数据中声明的数据库路径 '" +respath.FullPath+ "' 在覆盖关系对照表中没有找到, 请选择覆盖方式: " ;
					dlg.Origin = respath.FullPath.ToUpper();
					dlg.Servers = this.Servers;
					dlg.Channels = this.Channels;
					dlg.Map = map;

                    dlg.StartPosition = FormStartPosition.CenterScreen;
					dlg.ShowDialog(this);

					if (dlg.DialogResult != DialogResult.OK) 
					{
						strError = "用户中断...";
						goto ERROR1;
					}

					map = dlg.Map;
					goto REDO;
				}

				if (mapItem.Style == "skip")
					return 0;

				// 构造目标路径

				// 1)从源路径中提取id。源路径来自备份文件数据
				respath = new ResPath(strResPath);
				string strID = respath.GetRecordId();

				if (strID == null || strID == ""
					|| (mapItem.Style == "append")
					)
				{
					strID = "?";	// 将来加一个对话框
				}

				// 2)用目标库路径构造完整的记录路径
				string strTargetFullPath = "";
				if (mapItem.Target == "*") 
				{
					respath = new ResPath(strResPath);
					respath.MakeDbName();
					strTargetFullPath = respath.FullPath;
				}
				else 
				{
					strTargetFullPath = mapItem.Target;
				}

				respath = new ResPath(strTargetFullPath);
				strTargetPath = respath.Path + "/" + strID;
				strRecordPath = strTargetPath;

				channel = this.Channels.GetChannel(respath.Url);

			}
			else // 第二个以后的资源
			{
				if (channel == null)
				{
					strError = "当bIsFirstRes==false时，参数channel不应为null...";
					goto ERROR1;
				}


				ResPath respath = new ResPath(strResPath);
				string strObjectId = respath.GetObjectId();
				if (strObjectId == null || strObjectId == "") 
				{
					strError = "object id为空...";
					goto ERROR1;
				}
				strTargetPath = strRecordPath + "/object/" + strObjectId;
				if (strRecordPath == "")
				{
					strError = "strRecordPath参数值为空...";
					goto ERROR1;
				}
			}


			// string strLocalPath = DomUtil.GetAttr(node,"localpath");
			// string strMimeType = DomUtil.GetAttr(node,"mimetype");
			string strTimeStamp = DomUtil.GetAttr(node,"timestamp");
			// 注意,strLocalPath并不是要上载的body文件,它只用来作元数据\
			// body文件为strBodyTempFileName


			// 3.将body文件拆分成片断进行上载
			string[] ranges = null;

			if (lBodyLength == 0)	
			{ // 空文件
				ranges = new string[1];
				ranges[0] = "";
			}
			else 
			{
				string strRange = "";
				strRange = "0-" + Convert.ToString(lBodyLength-1);

				// 按照100K作为一个chunk
				ranges = RangeList.ChunkRange(strRange,
					100*1024);
			}



			byte [] timestamp = ByteArray.GetTimeStampByteArray(strTimeStamp);
			byte [] output_timestamp = null;

			REDOWHOLESAVE:
				string strOutputPath = "";
			string strWarning = "";

			for(int j=0;j<ranges.Length;j++) 
			{
			REDOSINGLESAVE:

				Application.DoEvents();	// 出让界面控制权

				if (stop.State != 0)
				{
					DialogResult result = MessageBox.Show(this,
						"确实要中断当前批处理操作?",
						"dp2batch",
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


				string strWaiting = "";
				if (j == ranges.Length - 1)
					strWaiting = " 请耐心等待...";

				string strPercent = "";
				RangeList rl = new RangeList(ranges[j]);
				if (rl.Count >= 1) 
				{
					double ratio = (double)((RangeItem)rl[0]).lStart / (double)lBodyLength;
					strPercent = String.Format("{0,3:N}",ratio * (double)100) + "%";
				}

				if (stop != null)
					stop.SetMessage("正在上载 " + ranges[j] + "/"
						+ Convert.ToString(lBodyLength)
						+ " " + strPercent + " " + strTargetPath + strWarning + strWaiting + " " + strCount);


				inputfile.Seek(lBodyStart, SeekOrigin.Begin);

				long lRet = channel.DoSaveResObject(strTargetPath,
					inputfile,
					lBodyLength,
					"",	// style
					strMetaDataXml,
					ranges[j],
					j == ranges.Length - 1 ? true : false,	// 最尾一次操作，提醒底层注意设置特殊的WebService API超时时间
					timestamp,
					out output_timestamp,
					out strOutputPath,
					out strError);

				// progressBar_main.Value = (int)((inputfile.Position)/ProgressRatio);
                stop.SetProgressValue(inputfile.Position);

				strWarning = "";

				if (lRet == -1) 
				{
					if (channel.ErrorCode == ChannelErrorCode.TimestampMismatch)
					{
                        string strDisplayRecPath = strOutputPath;
                        if (string.IsNullOrEmpty(strDisplayRecPath) == true)
                            strDisplayRecPath = strTargetPath;

						if (this.bNotAskTimestampMismatchWhenOverwrite == true) 
						{
							timestamp = new byte[output_timestamp.Length];
							Array.Copy(output_timestamp, 0, timestamp, 0, output_timestamp.Length);
							strWarning = " (时间戳不匹配, 自动重试)";
							if (ranges.Length == 1 || j==0) 
								goto REDOSINGLESAVE;
							goto REDOWHOLESAVE;
						}


						DialogResult result = MessageDlg.Show(this,
                            "上载 '" + strDisplayRecPath + "' (片断:" + ranges[j] + "/总尺寸:" + Convert.ToString(lBodyLength)
							+") 时发现时间戳不匹配。详细情况如下：\r\n---\r\n"
							+ strError + "\r\n---\r\n\r\n是否以新时间戳强行上载?\r\n注：(是)强行上载 (否)忽略当前记录或资源上载，但继续后面的处理 (取消)中断整个批处理",
							"dp2batch",
							MessageBoxButtons.YesNoCancel,
							MessageBoxDefaultButton.Button1,
							ref this.bNotAskTimestampMismatchWhenOverwrite);
						if (result == DialogResult.Yes) 
						{

							if (output_timestamp != null)
							{
								timestamp = new byte[output_timestamp.Length];
								Array.Copy(output_timestamp, 0, timestamp, 0, output_timestamp.Length);
							}
							else
							{
								timestamp = output_timestamp;
							}
							strWarning = " (时间戳不匹配, 应用户要求重试)";
							if (ranges.Length == 1 || j==0) 
								goto REDOSINGLESAVE;
							goto REDOWHOLESAVE;
						}

						if (result == DialogResult.No) 
						{
							return 0;	// 继续作后面的资源
						}

						if (result == DialogResult.Cancel) 
						{
							strError = "用户中断";
							goto ERROR1;	// 中断整个处理
						}
					}


					goto ERROR1;
				}

				timestamp = output_timestamp;
			}

			// 考虑到保存第一个资源的时候，id可能为“?”，因此需要得到实际的id值
			if (bIsFirstRes)
				strRecordPath = strOutputPath;

			return 0;
			
			ERROR1:
				return -1;
		}


		// 从输入流中得到一个res的metadata和body
		// parameter:
		//		inputfile:       源流
		//		bIsFirstRes:     是否是第一个资源
		//		strMetaDataXml:  返回metadata内容
		//		strError:        error info
		// return:
		//		-1: error
		//		0:  successed
		public static int GetResInfo(Stream inputfile,
			bool bIsFirstRes,
			out string strMetaDataXml,
			out long lBodyStart,
			out long lBodyLength,
			out string strError)
		{
			strMetaDataXml = "";
			strError = "";
			lBodyStart = 0;
			lBodyLength = 0;

			byte [] length = new byte[8];

			// 读入总长度
			int nRet = inputfile.Read(length, 0 , 8);
			if (nRet < 8) 
			{
				strError = "读取res总长度部分出错...";
				return -1;
			}

			long lTotalLength = BitConverter.ToInt64(length, 0);

			// 读入metadata长度
			nRet = inputfile.Read(length, 0 , 8);
			if (nRet < 8) 
			{
				strError = "读取metadata长度部分出错...";
				return -1;
			}

			long lMetaDataLength = BitConverter.ToInt64(length, 0);

			if (lMetaDataLength >= 100*1024)
			{
				strError = "metadata数据长度超过100K，似不是正确格式...";
				return -1;
			}

			byte[] metadata = new byte[(int)lMetaDataLength];
			int nReadLength = inputfile.Read(metadata,
				0,
				(int)lMetaDataLength);
			if (nReadLength < (int)lMetaDataLength)
			{
				strError = "metadata声明的长度超过文件末尾，格式错误";
				return -1;
			}

			strMetaDataXml = Encoding.UTF8.GetString(metadata);	// ? 是否可能抛出异常

			// 读body部分的长度
			nRet = inputfile.Read(length, 0 , 8);
			if (nRet < 8) 
			{
				strError = "读取body长度部分出错...";
				return -1;
			}

			lBodyStart = inputfile.Position;

			lBodyLength = BitConverter.ToInt64(length, 0);
			if (bIsFirstRes == true && lBodyLength >= 2000*1024)
			{
				strError = "第一个res中body的xml数据长度超过2000K，似不是正确格式...";
				return -1;
			}

			return 0;
		}

		// 管理库名对照表
		private void button_import_dbMap_Click(object sender, System.EventArgs e)
		{
			DbNameMapDlg dlg = new DbNameMapDlg();
            MainForm.SetControlFont(dlg, this.DefaultFont);

            string strError = "";

			dlg.SearchPanel = this.SearchPanel;
			dlg.DbNameMap = DbNameMap.Build(this.textBox_import_dbMap.Text.Replace("\r\n",";"),
                out strError);
            if (dlg.DbNameMap == null)
            {
                MessageBox.Show(this, strError);
                return;
            }

			this.AppInfo.LinkFormState(dlg, "DbNameMapDlg_state");
			dlg.ShowDialog(this);
			this.AppInfo.UnlinkFormState(dlg);

			if (dlg.DialogResult != DialogResult.OK)
				return;

			this.textBox_import_dbMap.Text = dlg.DbNameMap.ToString(true).Replace(";", "\r\n");
		}


		/*
		void oldfindTargetDB()
		{
			OpenResDlg dlg = new OpenResDlg();

			dlg.Text = "请选择目标数据库";
			dlg.EnabledIndices = new int[] { ResTree.RESTYPE_DB };
			dlg.ap = this.applicationInfo;
			dlg.ApCfgTitle = "pageimport_openresdlg";
			dlg.MultiSelect = true;
			dlg.Paths = textBox_import_targetDB.Text;
			dlg.Initial( this.Servers,
				this.Channels);	
			// dlg.StartPosition = FormStartPosition.CenterScreen;
			dlg.ShowDialog(this);

			if (dlg.DialogResult != DialogResult.OK)
				return;

			textBox_import_targetDB.Text = dlg.Paths;
		}
		*/


		void DoStop(object sender, StopEventArgs e)
		{
			if (this.channel != null)
				this.channel.Abort();
		}


		private void button_import_findFileName_Click(object sender, System.EventArgs e)
		{
			OpenFileDialog dlg = new OpenFileDialog();

			dlg.FileName = textBox_import_fileName.Text;
			dlg.Filter = "备份文件 (*.dp2bak)|*.dp2bak|XML文件 (*.xml)|*.xml|ISO2709文件 (*.iso;*.mrc)|*.iso;*.mrc|All files (*.*)|*.*" ;
			dlg.RestoreDirectory = true ;

			if(dlg.ShowDialog() != DialogResult.OK)
			{
				return;
			}

			textBox_import_fileName.Text = dlg.FileName;

		}

		// 准备脚本环境
		int PrepareScript(string strProjectName,
			string strProjectLocate,
			out Assembly assemblyMain,
			out MyFilterDocument filter,
			out Batch batchObj,
			out string strError)
		{
			assemblyMain = null;
			Assembly assemblyFilter = null;
			filter = null;
			batchObj = null;

			string strWarning = "";
			string strMainCsDllName = strProjectLocate + "\\~main_" + Convert.ToString(m_nAssemblyVersion++)+ ".dll";

            string strLibPaths = "\"" + this.DataDir + "\""
				+ "," 
				+ "\"" + strProjectLocate + "\"";

			string[] saAddRef = {
									Environment.CurrentDirectory + "\\digitalplatform.marcdom.dll",
									Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
									Environment.CurrentDirectory + "\\digitalplatform.rms.Client.dll",
									Environment.CurrentDirectory + "\\digitalplatform.library.dll",
									// Environment.CurrentDirectory + "\\digitalplatform.statis.dll",
									Environment.CurrentDirectory + "\\digitalplatform.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
									Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
									Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
									Environment.CurrentDirectory + "\\Interop.SHDocVw.dll",
									Environment.CurrentDirectory + "\\dp2batch.exe"};


			// 创建Project中Script main.cs的Assembly
			// return:
			//		-2	出错，但是已经提示过错误信息了。
			//		-1	出错
			int nRet = scriptManager.BuildAssembly(
                "MainForm",
				strProjectName,
				"main.cs",
				saAddRef,
				strLibPaths,
				strMainCsDllName,
				out strError,
				out strWarning);
			if (nRet == -2)
				goto ERROR1;
			if (nRet == -1) 
			{
				if (strWarning == "")
					goto ERROR1;
				MessageBox.Show(this, strWarning);
			}

			assemblyMain = Assembly.LoadFrom(strMainCsDllName);
			if (assemblyMain == null) 
			{
				strError = "LoadFrom " + strMainCsDllName + " fail";
				goto ERROR1;
			}


			// 得到Assembly中Batch派生类Type
			Type entryClassType = ScriptManager.GetDerivedClassType(
				assemblyMain,
				"dp2Batch.Batch");

			// new一个Batch派生对象
			batchObj = (Batch)entryClassType.InvokeMember(null, 
				BindingFlags.DeclaredOnly | 
				BindingFlags.Public | BindingFlags.NonPublic | 
				BindingFlags.Instance | BindingFlags.CreateInstance, null, null,
				null);

			// 为Batch派生类设置参数
			batchObj.MainForm = this;
			batchObj.ap = this.AppInfo;
			batchObj.ProjectDir = strProjectLocate;
			batchObj.DbPath = this.textBox_dbPath.Text;
			batchObj.SearchPanel = this.SearchPanel;
			/*
			batchObj.SearchPanel.InitialStopManager(this.toolBarButton_stop,
				this.statusBar_main);
			*/

			// batchObj.Channel = channel;
			//batchObj.GisIniFilePath = applicationInfo.GetString(
			//	"preference",
			//	"gisinifilepath",
			//	"");

			////////////////////////////
			// 装载marfilter.fltx
			string strFilterFileName = strProjectLocate + "\\marcfilter.fltx";

			if (FileUtil.FileExist(strFilterFileName) == true) 
			{

				filter = new MyFilterDocument();

				filter.Batch = batchObj;
				filter.strOtherDef = entryClassType.FullName + " Batch = null;";

			
				filter.strPreInitial = " MyFilterDocument doc = (MyFilterDocument)this.Document;\r\n";
				filter.strPreInitial += " Batch = ("
					+ entryClassType.FullName + ")doc.Batch;\r\n";

				filter.Load(strFilterFileName);

				nRet = filter.BuildScriptFile(strProjectLocate + "\\marcfilter.fltx.cs",
					out strError);
				if (nRet == -1)
					goto ERROR1;

				string[] saAddRef1 = {
										 Environment.CurrentDirectory + "\\digitalplatform.marcdom.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.marckernel.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.rms.client.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.library.dll",
										 // this.DataDir + "\\digitalplatform.statis.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.Text.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.IO.dll",
										 Environment.CurrentDirectory + "\\digitalplatform.Xml.dll",
										 Environment.CurrentDirectory + "\\Interop.SHDocVw.dll",
										 Environment.CurrentDirectory + "\\dp2batch.exe",
										 strMainCsDllName};

				string strfilterCsDllName = strProjectLocate + "\\~marcfilter_" + Convert.ToString(m_nAssemblyVersion++)+ ".dll";

				// 创建Project中Script的Assembly
				nRet = scriptManager.BuildAssembly(
                    "MainForm",
					strProjectName,
					"marcfilter.fltx.cs",
					saAddRef1,
					strLibPaths,
					strfilterCsDllName,
					out strError,
					out strWarning);
				if (nRet == -2)
					goto ERROR1;
				if (nRet == -1) 
				{
					if (strWarning == "") 
					{
						goto ERROR1;
					}
					MessageBox.Show(this, strWarning);
				}


				assemblyFilter = Assembly.LoadFrom(strfilterCsDllName);
				if (assemblyFilter == null) 
				{
					strError = "LoadFrom " + strfilterCsDllName + "fail";
					goto ERROR1;
				}


				filter.Assembly = assemblyFilter;

			}

			return 0;

			ERROR1:
				return -1;
		}

        public void WriteLog(string strText)
        {
            FileUtil.WriteErrorLog(
                this,
                this.DataDir,
                strText);
        }

        // 输出
		void DoExport(string strProjectName,
			string strProjectLocate)
		{
			string strError = "";
			int nRet = 0;

			Assembly assemblyMain = null;
			MyFilterDocument filter = null;
			batchObj = null;
			m_nRecordCount = -1;


			// 准备脚本
			if (strProjectName != "" && strProjectName != null)
			{
				nRet = PrepareScript(strProjectName,
					strProjectLocate,
					out assemblyMain,
					out filter,
					out batchObj,
					out strError);
				if (nRet == -1)
					goto ERROR1;

				this.AssemblyMain = assemblyMain;
				if (filter != null)
					this.AssemblyFilter = filter.Assembly;
				else
					this.AssemblyFilter = null;

			}


			// 执行脚本的OnInitial()

			// 触发Script中OnInitial()代码
			// OnInitial()和OnBegin的本质区别, 在于OnInitial()适合检查和设置面板参数
			if (batchObj != null)
			{
				BatchEventArgs args = new BatchEventArgs();
				batchObj.OnInitial(this, args);
				/*
				if (args.Continue == ContinueType.SkipBeginMiddle)
					goto END1;
				if (args.Continue == ContinueType.SkipMiddle) 
				{
					strError = "OnInitial()中args.Continue不能使用ContinueType.SkipMiddle.应使用ContinueType.SkipBeginMiddle";
					goto ERROR1;
				}
				*/
				if (args.Continue == ContinueType.SkipAll)
					goto END1;
			}

			string strOutputFileName = "";

			if (textBox_dbPath.Text == "")
			{
				MessageBox.Show(this, "尚未选择源库...");
				return;
			}

            string[] dbpaths = this.textBox_dbPath.Text.Split(new char[] {';'});

            Debug.Assert(dbpaths.Length != 0, "");

            // 如果为单库输出
            if (dbpaths.Length == 1)
            {
                // 否则移到DoExportFile()函数里面去校验
                ResPath respath = new ResPath(dbpaths[0]);

                channel = this.Channels.GetChannel(respath.Url);

                string strDbName = respath.Path;

                // 校验起止号
                if (checkBox_verifyNumber.Checked == true)
                {
                    nRet = VerifyRange(channel,
                        strDbName,
                        out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, strError);
                }
                else
                {
                    if (this.textBox_startNo.Text == "")
                    {
                        strError = "尚未指定起始号";
                        goto ERROR1;
                    }
                    if (this.textBox_endNo.Text == "")
                    {
                        strError = "尚未指定结束号";
                        goto ERROR1;
                    }
                }
            }
            else
            {
                // 多库输出。修改界面要素，表示针对每个库都是全库处理
                this.radioButton_all.Checked = true;
                this.textBox_startNo.Text = "1";
                this.textBox_endNo.Text = "9999999999";
            }
             

			SaveFileDialog dlg = null;

            if (checkBox_export_delete.Checked == true)
            {
                DialogResult result = MessageBox.Show(this,
                        "确实要(在输出的同时)删除数据库记录?\r\n\r\n---------\r\n(确定)删除 (放弃)放弃批处理",
                        "dp2batch",
                        MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                if (result != DialogResult.OK)
                {
                    strError = "放弃处理...";
                    goto ERROR1;
                }

                result = MessageBox.Show(this,
                    "在删除记录的同时, 是否将记录输出到文件?\r\n\r\n--------\r\n(是)一边删除一边输出 (否)只删除不输出",
                    "dp2batch",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (result != DialogResult.Yes)
                    goto SKIPASKFILENAME;
            }


			// 获得输出文件名
			dlg = new SaveFileDialog();

			dlg.Title = "请指定要保存的备份文件名";
			dlg.CreatePrompt = false;
			dlg.OverwritePrompt = false;
			dlg.FileName = strLastOutputFileName;
			dlg.FilterIndex = nLastOutputFilterIndex;

			dlg.Filter = "备份文件 (*.dp2bak)|*.dp2bak|XML文件 (*.xml)|*.xml|ISO2709文件 (*.iso;*.mrc)|*.iso;*.mrc|All files (*.*)|*.*" ;

			dlg.RestoreDirectory = true;

			if (dlg.ShowDialog(this) != DialogResult.OK) 
			{
				strError = "放弃处理...";
				goto ERROR1;
			}

			strLastOutputFileName = dlg.FileName;
			nLastOutputFilterIndex = dlg.FilterIndex;
			strOutputFileName = dlg.FileName;

			SKIPASKFILENAME:

				// 触发Script中OnBegin()代码
				// OnBegin()中仍然有修改MainForm面板的自由
			if (batchObj != null)
			{
				BatchEventArgs args = new BatchEventArgs();
				batchObj.OnBegin(this, args);
				/*
				if (args.Continue == ContinueType.SkipMiddle)
					goto END1;
				if (args.Continue == ContinueType.SkipBeginMiddle)
					goto END1;
				*/
				if (args.Continue == ContinueType.SkipAll)
					goto END1;
			}



            if (dlg == null || dlg.FilterIndex == 1)
                nRet = DoExportFile(
                    dbpaths,
                    strOutputFileName,
                    ExportFileType.BackupFile,
                    null,
                    out strError);
            else if (dlg.FilterIndex == 2)
                nRet = DoExportFile(
                    dbpaths,
                    strOutputFileName,
                    ExportFileType.XmlFile,
                    null,
                    out strError);
            else if (dlg.FilterIndex == 3)
            {
                ResPath respath = new ResPath(dbpaths[0]);

                string strMarcSyntax = "";
                // 从marcdef配置文件中获得marc格式定义
                // return:
                //		-1	出错
                //		0	没有找到
                //		1	找到
                nRet = this.SearchPanel.GetMarcSyntax(respath.FullPath,
                    out strMarcSyntax,
                    out strError);
                if (nRet == 0 || nRet == -1)
                {
                    strError = "获取数据库 '" + dbpaths[0] + "' 的MARC格式时发生错误: " + strError;
                    goto ERROR1;
                }

                // 如果多于一个数据库输出到一个文件，需要关心每个数据库的MARC格式是否相同，给与适当的警告
                if (dbpaths.Length > 1)
                {
                    string strWarning = "";
                    for (int i = 1; i < dbpaths.Length; i++)
                    {
                        ResPath current_respath = new ResPath(dbpaths[i]);

                        string strPerMarcSyntax = "";
                        // 从marcdef配置文件中获得marc格式定义
                        // return:
                        //		-1	出错
                        //		0	没有找到
                        //		1	找到
                        nRet = this.SearchPanel.GetMarcSyntax(current_respath.FullPath,
                            out strPerMarcSyntax,
                            out strError);
                        if (nRet == 0 || nRet == -1)
                        {
                            strError = "获取数据库 '" + dbpaths[i] + "' 的MARC格式时发生错误: " + strError;
                            goto ERROR1;
                        }

                        if (strPerMarcSyntax != strMarcSyntax)
                        {
                            if (String.IsNullOrEmpty(strWarning) == false)
                                strWarning += "\r\n";
                            strWarning += "数据库 '" + dbpaths[i] + "' (" + strPerMarcSyntax + ")";

                        }
                    }

                    if (String.IsNullOrEmpty(strWarning) == false)
                    {
                        strWarning = "所选择的数据库中，下列数据库的MARC格式和第一个数据库( '"+dbpaths[0]+"' ("+strMarcSyntax+"))的不同: \r\n---\r\n" + strWarning + "\r\n---\r\n\r\n如果把这些不同MARC格式的记录混合输出到一个文件中，可能会造成许多软件以后读取它时发生困难。\r\n\r\n确实要这样混合着转出到一个文件中?";
                        DialogResult result = MessageBox.Show(this,
                            strWarning,
                            "dp2batch",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                        if (result == DialogResult.No)
                        {
                            strError = "放弃处理...";
                            goto ERROR1;
                        }
                    }
                }

                OpenMarcFileDlg marcdlg = new OpenMarcFileDlg();
                MainForm.SetControlFont(marcdlg, this.DefaultFont);
                marcdlg.IsOutput = true;
                marcdlg.Text = "请指定要输出的 ISO2709 文件属性";
                marcdlg.FileName = strOutputFileName;
                marcdlg.MarcSyntax = strMarcSyntax;
                marcdlg.EnableMarcSyntax = false;   // 不允许用户选择marc syntax，因为这是数据库配置好了的属性 2007/8/18

                marcdlg.CrLf = this.OutputCrLf;
                marcdlg.AddG01 = this.AddG01;
                marcdlg.RemoveField998 = this.Remove998;


                this.AppInfo.LinkFormState(marcdlg, "OpenMarcFileDlg_output_state");
                marcdlg.ShowDialog(this);
                this.AppInfo.UnlinkFormState(marcdlg);


                if (marcdlg.DialogResult != DialogResult.OK)
                {
                    strError = "放弃处理...";
                    goto ERROR1;
                }

                if (marcdlg.AddG01 == true)
                {
                    MessageBox.Show(this, "您选择了在导出的ISO2709记录中加入-01字段。请注意dp2Batch在将来导入这样的ISO2709文件的时候，记录中-01字段***起不到***覆盖定位的作用。“加入-01字段”功能是为了将导出的ISO2709文件应用到dt1000系统而设计的。\r\n\r\n如果您这样做的目的是为了对dp2系统书目库中的数据进行备份，请改用.xml格式或.dp2bak格式。");
                }


                strOutputFileName = marcdlg.FileName;
                this.CurMarcSyntax = strMarcSyntax;
                this.OutputCrLf = marcdlg.CrLf;
                this.AddG01 = marcdlg.AddG01;
                this.Remove998 = marcdlg.RemoveField998;

                nRet = DoExportFile(
                    dbpaths,
                    marcdlg.FileName,
                    ExportFileType.ISO2709File,
                    marcdlg.Encoding,
                    out strError);
            }
            else
            {
                strError = "不支持的文件类型...";
                goto ERROR1;
            }

            /*
            if (nRet == 1)
                goto END2;
            */
            if (nRet == -1)
                goto ERROR1;
        END1:
            // 触发Script的OnEnd()代码
            if (batchObj != null)
            {
                BatchEventArgs args = new BatchEventArgs();
                batchObj.OnEnd(this, args);
            }

            // END2:

            this.AssemblyMain = null;
            this.AssemblyFilter = null;
            if (filter != null)
                filter.Assembly = null;
            this.MarcFilter = null;

            if (String.IsNullOrEmpty(strError) == false)
			    MessageBox.Show(this, strError);
			return;

        ERROR1:
            this.AssemblyMain = null;
            this.AssemblyFilter = null;
            if (filter != null)
                filter.Assembly = null;
            this.MarcFilter = null;


            MessageBox.Show(this, strError);

		}


#if NNNNN
		void DoExportXmlFile(string strOutputFileName)
		{
			string strError = "";

			FileStream outputfile = null;
			XmlTextWriter writer = null;   

			if (textBox_dbPath.Text == "")
			{
				MessageBox.Show(this, "尚未选择源库...");
				return;
			}

			ResPath respath = new ResPath(textBox_dbPath.Text);

			channel = this.Channels.GetChannel(respath.Url);

			string strDbName = respath.Path;

			if (strOutputFileName != null && strOutputFileName != "") 
			{
				// 探测文件是否存在
				FileInfo fi = new FileInfo(strOutputFileName);
				if (fi.Exists == true && fi.Length > 0)
				{
					DialogResult result = MessageBox.Show(this,
						"文件 '" + strOutputFileName + "' 已存在，是否覆盖?\r\n\r\n--------------------\r\n注：(是)覆盖  (否)中断处理",
						"dp2batch",
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question,
						MessageBoxDefaultButton.Button2);
					if (result != DialogResult.Yes) 
					{
						strError = "放弃处理...";
						goto ERROR1;
					}
				}

				// 打开文件
				outputfile = File.Create(
					strOutputFileName);

				writer = new XmlTextWriter(outputfile, Encoding.UTF8);
				writer.Formatting = Formatting.Indented;
				writer.Indentation = 4;

			}


			try 
			{
				
				Int64 nStart;
				Int64 nEnd;
				Int64 nCur;
				bool bAsc = GetDirection(out nStart,
					out nEnd);

				// 设置进度条范围
				Int64 nMax = nEnd - nStart;
				if (nMax < 0)
					nMax *= -1;
				nMax ++;

				ProgressRatio =  nMax / 10000;
				if (ProgressRatio < 1.0)
					ProgressRatio = 1.0;

				progressBar_main.Minimum = 0;
				progressBar_main.Maximum = (int)(nMax/ProgressRatio);
				progressBar_main.Value = 0;


				bool bFirst = true;	// 是否为第一次取记录

				string strID = this.textBox_startNo.Text;


				stop.Initial(new Delegate_doStop(this.DoStop),
					"正在导出数据");
				stop.BeginLoop();

				EnableControls(false);

				if (writer != null) 
				{
					writer.WriteStartDocument();
					writer.WriteStartElement("dprms","collection",DpNs.dprms);
					//writer.WriteStartElement("collection");
					//writer.WriteAttributeString("xmlns:marc",
					//	"http://www.loc.gov/MARC21/slim");

				}

				// 循环
				for(;;) 
				{
					Application.DoEvents();	// 出让界面控制权

					if (stop.State != 0)
					{
						strError = "用户中断";
						goto ERROR1;
					}

					string strStyle = "";
					if (outputfile != null)
						strStyle = "data,content,timestamp,outputpath";
					else
						strStyle = "timestamp,outputpath";	// 优化

					if (bFirst == true)
						strStyle += "";
					else 
					{
						if (bAsc == true)
							strStyle += ",next";
						else
							strStyle += ",prev";
					}


					string strPath = strDbName + "/" + strID;
					string strXmlBody = "";
					string strMetaData = "";
					byte[] baOutputTimeStamp = null;
					string strOutputPath = "";

					bool bFoundRecord = false;

					// 获得资源
					// return:
					//		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
					//		0	成功
					long lRet = channel.GetRes(strPath,
						strStyle,
						out strXmlBody,
						out strMetaData,
						out baOutputTimeStamp,
						out strOutputPath,
						out strError);


					if (lRet == -1) 
					{
						if (channel.ErrorCode == ChannelErrorCode.NotFound) 
						{
							if (checkBox_forceLoop.Checked == true && bFirst == true)
							{
								AutoCloseMessageBox.Show(this, "记录 " + strID + " 不存在。\r\n\r\n按 确认 继续。");

								bFirst = false;
								goto CONTINUE;
							}
							else 
							{
								if (bFirst == true)
								{
									strError = "记录 " + strID + " 不存在。处理结束。";
								}
								else 
								{
									if (bAsc == true)
										strError = "记录 " + strID + " 是最末一条记录。处理结束。";
									else
										strError = "记录 " + strID + " 是最前一条记录。处理结束。";
								}
							}

						}
						else if (channel.ErrorCode == ChannelErrorCode.EmptyRecord) 
						{
							bFirst = false;
							bFoundRecord = false;
							// 把id解析出来
							strID = ResPath.GetRecordId(strOutputPath);
							goto CONTINUE;

						}

						goto ERROR1;
					}

					bFirst = false;

					bFoundRecord = true;

					// 把id解析出来
					strID = ResPath.GetRecordId(strOutputPath);

				CONTINUE:
					stop.SetMessage(strID);

					// 是否超过循环范围
					try 
					{
						nCur = Convert.ToInt64(strID);
					}
					catch
					{
						// ???
						nCur = 0;
					}

					if (bAsc == true && nCur > nEnd)
						break;
					if (bAsc == false && nCur < nEnd)
						break;

					if (bFoundRecord == true 
						&& writer != null) 
					{
						// 写磁盘
						XmlDocument dom = new XmlDocument();

						try 
						{
							dom.LoadXml(strXmlBody);

							ResPath respathtemp = new ResPath();
							respathtemp.Url = channel.Url;
							respathtemp.Path = strOutputPath;



							// DomUtil.SetAttr(dom.DocumentElement, "xmlns:dprms", DpNs.dprms);
							// 给根元素设置几个参数
							DomUtil.SetAttr(dom.DocumentElement, "path", DpNs.dprms, respathtemp.FullPath);
							DomUtil.SetAttr(dom.DocumentElement, "timestamp", DpNs.dprms, ByteArray.GetHexTimeStampString(baOutputTimeStamp));

							// DomUtil.SetAttr(dom.DocumentElement, "xmlns:marc", null);
							dom.DocumentElement.WriteTo(writer);
						}
						catch (Exception ex)
						{
							strError = ex.Message;
							// 询问是否继续
							goto ERROR1;
						}


						/*
						if (nRet == -1) 
						{
							// 询问是否继续
							goto ERROR1;
						}
						*/
					}

					// 删除
					if (checkBox_export_delete.Checked == true)
					{

						byte [] baOutputTimeStamp1 = null;
						strPath = strOutputPath;	// 得到实际的路径

						lRet = channel.DoDeleteRecord(
							strPath,
							baOutputTimeStamp,
							out baOutputTimeStamp1,
							out strError);
						if (lRet == -1) 
						{
							// 询问是否继续
							goto ERROR1;
						}
					}


					if (bAsc == true) 
					{
						progressBar_main.Value = (int)((nCur-nStart + 1)/ProgressRatio);
					}
					else 
					{
						// ?
						progressBar_main.Value = (int)((nStart-nCur + 1)/ProgressRatio);
					}


					// 对已经作过的进行判断
					if (bAsc == true && nCur >= nEnd)
						break;
					if (bAsc == false && nCur <= nEnd)
						break;


				}


				stop.EndLoop();
				stop.Initial(null, "");

				EnableControls(true);

			}

			finally 
			{
				if (writer != null) 
				{
					writer.WriteEndElement();
					writer.WriteEndDocument();
					writer.Close();
					writer = null;
				}

				if (outputfile != null) 
				{
					outputfile.Close();
					outputfile = null;
				}

			}

			END1:
				channel = null;
			if (checkBox_export_delete.Checked == true)
				MessageBox.Show(this, "数据导出和删除完成。");
			else
				MessageBox.Show(this, "数据导出完成。");
			return;

			ERROR1:

				stop.EndLoop();
			stop.Initial(null, "");

			EnableControls(true);


			channel = null;
			MessageBox.Show(this, strError);
			return;
		
		}
#endif


		// return:
		//		-1	error
		//		0	正常结束
		//		1	希望跳过后来的OnEnd()
        int DoExportFile(
            string[] dbpaths,
            string strOutputFileName,
            ExportFileType exportType,
            Encoding targetEncoding,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            string strDeleteStyle = "";
            if (this.checkBox_export_fastMode.Checked == true)
                strDeleteStyle = "fastmode";

            string strInfo = "";    // 汇总信息，在完成后显示

            FileStream outputfile = null;	// Backup和Xml格式输出都需要这个
            XmlTextWriter writer = null;   // Xml格式输出时需要这个

            bool bAppend = true;

            Debug.Assert(dbpaths != null, "");

            if (dbpaths.Length == 0)
            {
                strError = "尚未指定源库...";
                goto ERROR1;
            }


            if (String.IsNullOrEmpty(strOutputFileName) == false)
            {
                // 探测输出文件是否已经存在
                FileInfo fi = new FileInfo(strOutputFileName);
                bAppend = true;
                if (fi.Exists == true && fi.Length > 0)
                {
                    if (exportType == ExportFileType.BackupFile
                        || exportType == ExportFileType.ISO2709File)
                    {
                        DialogResult result = MessageBox.Show(this,
                            "文件 '" + strOutputFileName + "' 已存在，是否追加?\r\n\r\n--------------------\r\n注：(是)追加  (否)覆盖  (取消)中断处理",
                            "dp2batch",
                            MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button1);
                        if (result == DialogResult.Yes)
                        {
                            bAppend = true;
                        }
                        if (result == DialogResult.No)
                        {
                            bAppend = false;
                        }
                        if (result == DialogResult.Cancel)
                        {
                            strError = "放弃处理...";
                            goto ERROR1;
                        }
                    }
                    else if (exportType == ExportFileType.XmlFile)
                    {
                        DialogResult result = MessageBox.Show(this,
                            "文件 '" + strOutputFileName + "' 已存在，是否覆盖?\r\n\r\n--------------------\r\n注：(是)覆盖  (否)中断处理",
                            "dp2batch",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                        if (result != DialogResult.Yes)
                        {
                            strError = "放弃处理...";
                            goto ERROR1;
                        }
                    }


                }

                // 打开文件
                if (exportType == ExportFileType.BackupFile
                    || exportType == ExportFileType.ISO2709File)
                {
                    outputfile = File.Open(
                        strOutputFileName,
                        FileMode.OpenOrCreate,	// 原来是Open，后来修改为OpenOrCreate。这样对临时文件被系统管理员手动意外删除(但是xml文件中仍然记载了任务)的情况能够适应。否则会抛出FileNotFoundException异常
                        FileAccess.Write,
                        FileShare.ReadWrite);
                }
                else if (exportType == ExportFileType.XmlFile)
                {
                    outputfile = File.Create(
                        strOutputFileName);

                    writer = new XmlTextWriter(outputfile, Encoding.UTF8);
                    writer.Formatting = Formatting.Indented;
                    writer.Indentation = 4;
                }

            }

            if ((exportType == ExportFileType.BackupFile
                || exportType == ExportFileType.ISO2709File)
                && outputfile != null)
            {
                if (bAppend == true)
                    outputfile.Seek(0, SeekOrigin.End);	// 具有追加的能力
                else
                    outputfile.SetLength(0);
            }

            WriteLog("开始输出");

            try
            {

                // string[] dbpaths = textBox_dbPath.Text.Split(new char[] { ';' });

                for (int f = 0; f < dbpaths.Length; f++)
                {
                    string strOneDbPath = dbpaths[f];

                    ResPath respath = new ResPath(strOneDbPath);

                    channel = this.Channels.GetChannel(respath.Url);

                    string strDbName = respath.Path;
                    if (String.IsNullOrEmpty(strInfo) == false)
                        strInfo += "\r\n";
                    strInfo += "" + strDbName;

                    // 实际处理的首尾号
                    string strRealStartNo = "";
                    string strRealEndNo = "";

                    /*
                    DialogResult result;
                    if (checkBox_export_delete.Checked == true)
                    {
                        result = MessageBox.Show(this,
                            "确实要删除 '" + respath.Path + "' 内指定范围的记录?\r\n\r\n---------\r\n(是)删除 (否)放弃批处理",
                            "dp2batch",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                        if (result != DialogResult.Yes)
                            continue;
                    }
                     * 
                     * */


                    //channel = this.Channels.GetChannel(respath.Url);

                    //string strDbName = respath.Path;

                    // 如果为多库输出
                    if (dbpaths.Length > 0)
                    {
                        // 如果为全选
                        if (this.radioButton_all.Checked == true)
                        {
                            // 恢复为最大范围
                            this.textBox_startNo.Text = "1";
                            this.textBox_endNo.Text = "9999999999";
                        }

                        // 校验起止号
                        if (checkBox_verifyNumber.Checked == true)
                        {
                            nRet = VerifyRange(channel,
                                strDbName,
                                out strError);
                            if (nRet == -1)
                                MessageBox.Show(this, strError);

                            if (nRet == 0)
                            {
                                // 库中无记录
                                AutoCloseMessageBox.Show(this, "数据库 " + strDbName + " 中无记录。");
                                strInfo += "(无记录)";
                                WriteLog("发现数据库 " + strDbName + " 中无记录");
                                continue;
                            }
                        }
                        else
                        {
                            if (this.textBox_startNo.Text == "")
                            {
                                strError = "尚未指定起始号";
                                goto ERROR1;
                            }
                            if (this.textBox_endNo.Text == "")
                            {
                                strError = "尚未指定结束号";
                                goto ERROR1;
                            }
                        }
                    }

                    string strOutputStartNo = "";
                    string strOutputEndNo = "";
                    // 虽然界面不让校验起止号，但是也要校验，为了设置好进度条
                    if (checkBox_verifyNumber.Checked == false)
                    {
                        // 校验起止号
                        // return:
                        //      0   不存在记录
                        //      1   存在记录
                        nRet = VerifyRange(channel,
                            strDbName,
                            this.textBox_startNo.Text,
                            this.textBox_endNo.Text,
                            out strOutputStartNo,
                            out strOutputEndNo,
                            out strError);
                    }

                    //try
                    //{

                    Int64 nStart = 0;
                    Int64 nEnd = 0;
                    Int64 nCur = 0;
                    bool bAsc = true;

                    bAsc = GetDirection(
                        this.textBox_startNo.Text,
                        this.textBox_endNo.Text,
                        out nStart,
                        out nEnd);

                    // 探测到的号码
                    long nOutputEnd = 0;
                    long nOutputStart = 0;
                    if (checkBox_verifyNumber.Checked == false)
                    {
                        GetDirection(
                            strOutputStartNo,
                            strOutputEndNo,
                            out nOutputStart,
                            out nOutputEnd);
                    }

                    // 设置进度条范围
                    if (checkBox_verifyNumber.Checked == true)
                    {

                        Int64 nMax = nEnd - nStart;
                        if (nMax < 0)
                            nMax *= -1;
                        nMax++;

                        /*
                        ProgressRatio = nMax / 10000;
                        if (ProgressRatio < 1.0)
                            ProgressRatio = 1.0;

                        progressBar_main.Minimum = 0;
                        progressBar_main.Maximum = (int)(nMax / ProgressRatio);
                        progressBar_main.Value = 0;
                         * */
                        stop.SetProgressRange(0, nMax);
                    }
                    else
                    {
                        Int64 nMax = nOutputEnd - nOutputStart;
                        if (nMax < 0)
                            nMax *= -1;
                        nMax++;
                        stop.SetProgressRange(0, nMax);
                    }


                    bool bFirst = true;	// 是否为第一次取记录

                    string strID = this.textBox_startNo.Text;

                    stop.OnStop += new StopEventHandler(this.DoStop);
                    stop.Initial("正在导出数据");
                    stop.BeginLoop();

                    EnableControls(false);

                    if (exportType == ExportFileType.XmlFile
                        && writer != null)
                    {
                        writer.WriteStartDocument();
                        writer.WriteStartElement("dprms", "collection", DpNs.dprms);
                        //writer.WriteStartElement("collection");
                        //writer.WriteAttributeString("xmlns:marc",
                        //	"http://www.loc.gov/MARC21/slim");

                    }

                    WriteLog("开始输出数据库 '"+strDbName+"' 内的数据记录");

                    m_nRecordCount = 0;
                    // 循环
                    for (; ; )
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop.State != 0)
                        {
                            WriteLog("打开对话框 '确实要中断当前批处理操作?'");
                            DialogResult result = MessageBox.Show(this,
                                "确实要中断当前批处理操作?",
                                "dp2batch",
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Question,
                                MessageBoxDefaultButton.Button2);
                            WriteLog("关闭对话框 '确实要中断当前批处理操作?'");
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

                        string strDirectionComment = "";
                        string strStyle = "";
                        if (outputfile != null)
                            strStyle = "data,content,timestamp,outputpath";
                        else
                            strStyle = "timestamp,outputpath";	// 优化

                        if (bFirst == true)
                        {
                            strStyle += "";
                        }
                        else
                        {
                            if (bAsc == true)
                            {
                                strStyle += ",next";
                                strDirectionComment = "的后一条记录";
                            }
                            else
                            {
                                strStyle += ",prev";
                                strDirectionComment = "的前一条记录";
                            }
                        }


                        string strPath = strDbName + "/" + strID;
                        string strXmlBody = "";
                        string strMetaData = "";
                        byte[] baOutputTimeStamp = null;
                        string strOutputPath = "";

                        bool bFoundRecord = false;

                        bool bNeedRetry = true;

                    REDO_GETRES:
                        // 获得资源
                        // return:
                        //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
                        //		0	成功
                        long lRet = channel.GetRes(strPath,
                            strStyle,
                            out strXmlBody,
                            out strMetaData,
                            out baOutputTimeStamp,
                            out strOutputPath,
                            out strError);


                        if (lRet == -1)
                        {
                            if (channel.ErrorCode == ChannelErrorCode.NotFound)
                            {
                                if (bFirst == true)
                                {
                                    if (checkBox_forceLoop.Checked == true)
                                    {
                                        string strText = "记录 " + strID + strDirectionComment + " 不存在。\r\n\r\n按 确认 继续。";
                                        WriteLog("打开对话框 '"+strText.Replace("\r\n", "\\n")+"'");
                                        AutoCloseMessageBox.Show(this, strText);
                                        WriteLog("关闭对话框 '" + strText.Replace("\r\n", "\\n") + "'");

                                        bFirst = false;
                                        goto CONTINUE;
                                    }
                                    else
                                    {
                                        // 如果不要强制循环，此时也不能结束，否则会让用户以为数据库里面根本没有数据
                                        string strText = "您为数据库 " + strDbName + " 指定的首记录 " + strID + strDirectionComment + " 不存在。\r\n\r\n(注：为避免出现此提示，可在操作前勾选“校准首尾ID”)\r\n\r\n按 确认 继续向后找...";
                                        WriteLog("打开对话框 '" + strText.Replace("\r\n", "\\n") + "'");
                                        AutoCloseMessageBox.Show(this, strText);
                                        WriteLog("关闭对话框 '" + strText.Replace("\r\n", "\\n") + "'");

                                        bFirst = false;
                                        goto CONTINUE;
                                    }
                                }
                                else
                                {
                                    Debug.Assert(bFirst == false, "");

                                    if (bFirst == true)
                                    {
                                        strError = "记录 " + strID + strDirectionComment + " 不存在。处理结束。";
                                    }
                                    else
                                    {
                                        if (bAsc == true)
                                            strError = "记录 " + strID + " 是最末一条记录。处理结束。";
                                        else
                                            strError = "记录 " + strID + " 是最前一条记录。处理结束。";
                                    }

                                    if (dbpaths.Length > 1)
                                        break;  // 多库情况，继续其它库循环
                                    else
                                    {
                                        bNeedRetry = false; // 单库情况，也没有必要出现重试对话框

                                        WriteLog("打开对话框 '" + strError.Replace("\r\n", "\\n") + "'");
                                        MessageBox.Show(this, strError);
                                        WriteLog("关闭对话框 '" + strError.Replace("\r\n", "\\n") + "'");
                                        break;
                                    }
                                }

                            }
                            else if (channel.ErrorCode == ChannelErrorCode.EmptyRecord)
                            {
                                bFirst = false;
                                bFoundRecord = false;
                                // 把id解析出来
                                strID = ResPath.GetRecordId(strOutputPath);
                                goto CONTINUE;

                            }

                            // 允许重试
                            if (bNeedRetry == true)
                            {
                                string strText = "获取记录 '" + strPath + "' (style='" + strStyle + "')时出现错误: " + strError + "\r\n\r\n重试，还是中断当前批处理操作?\r\n(Retry 重试；Cancel 中断批处理)";
                                WriteLog("打开对话框 '" + strText.Replace("\r\n", "\\n") + "'");
                                DialogResult redo_result = MessageBox.Show(this,
                                    strText,
                                    "dp2batch",
                                    MessageBoxButtons.RetryCancel,
                                    MessageBoxIcon.Question,
                                    MessageBoxDefaultButton.Button1);
                                WriteLog("关闭对话框 '" + strText.Replace("\r\n", "\\n") + "'");
                                if (redo_result == DialogResult.Cancel)
                                    goto ERROR1;
                                goto
                                    REDO_GETRES;
                            }
                            else
                            {
                                goto ERROR1;
                            }
                        }

                        // 2008/11/9 new add
                        if (String.IsNullOrEmpty(strXmlBody) == true)
                        {
                            bFirst = false;
                            bFoundRecord = false;
                            // 把id解析出来
                            strID = ResPath.GetRecordId(strOutputPath);
                            goto CONTINUE;
                        }

                        bFirst = false;

                        bFoundRecord = true;

                        // 把id解析出来
                        strID = ResPath.GetRecordId(strOutputPath);
                        stop.SetMessage("已导出记录 " + strOutputPath + "  " + m_nRecordCount.ToString());

                        if (String.IsNullOrEmpty(strRealStartNo) == true)
                        {
                            strRealStartNo = strID;
                        }

                        strRealEndNo = strID;

                    CONTINUE:

                        // 是否超过循环范围
                        try
                        {
                            nCur = Convert.ToInt64(strID);
                        }
                        catch
                        {
                            // ???
                            nCur = 0;
                        }

                        if (checkBox_verifyNumber.Checked == false)
                        {
                            // 如果当前记录号码突破预计的头部和尾部
                            if (nCur > nOutputEnd
                                || nCur < nOutputStart)
                            {
                                if (nCur > nOutputEnd)
                                    nOutputEnd = nCur;

                                if (nCur < nOutputStart)
                                    nOutputStart = nCur;

                                // 重新计算和设置进度条
                                long nMax = nOutputEnd - nOutputStart;
                                if (nMax < 0)
                                    nMax *= -1;
                                nMax++;

                                stop.SetProgressRange(0, nMax);
                            }
                        }

                        if (bAsc == true && nCur > nEnd)
                            break;
                        if (bAsc == false && nCur < nEnd)
                            break;

                        string strMarc = "";

                        // 将Xml转换为MARC
                        if (exportType == ExportFileType.ISO2709File
                            && bFoundRecord == true)    // 2008/11/13 new add
                        {
                            nRet = GetMarc(strXmlBody,
                                out strMarc,
                                out strError);
                            if (nRet == -1)
                            {
                                strError = "记录 " + strOutputPath + " 在将XML格式转换为MARC时出错: " + strError;
                                goto ERROR1;
                            }
                        }

                        if (this.MarcFilter != null)
                        {
                            // 触发filter中的Record相关动作
                            // TODO: 有可能strMarc为空哟，需要测试一下
                            nRet = MarcFilter.DoRecord(
                                null,
                                strMarc,
                                m_nRecordCount,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                        }

                        // 触发Script的Outputing()代码
                        if (bFoundRecord == true && this.AssemblyMain != null)
                        {
                            // 这些变量要先初始化,因为filter代码可能用到这些Batch成员.
                            batchObj.XmlRecord = strXmlBody;

                            batchObj.MarcSyntax = this.CurMarcSyntax;

                            batchObj.MarcRecord = strMarc;	// MARC记录体
                            batchObj.MarcRecordChanged = false;	// 为本轮Script运行准备初始状态

                            batchObj.SearchPanel.ServerUrl = channel.Url;
                            batchObj.ServerUrl = channel.Url;
                            batchObj.RecPath = strOutputPath;	// 记录路径
                            batchObj.RecIndex = m_nRecordCount;	// 当前记录在一批中的序号
                            batchObj.TimeStamp = baOutputTimeStamp;


                            BatchEventArgs args = new BatchEventArgs();

                            batchObj.Outputing(this, args);
                            /*
                            if (args.Continue == ContinueType.SkipMiddle)
                                goto CONTINUEDBS;
                            if (args.Continue == ContinueType.SkipBeginMiddle)
                                goto CONTINUEDBS;
                            */
                            if (args.Continue == ContinueType.SkipAll)
                                goto CONTINUEDBS;

                            // 观察用于输出的MARC记录是否被改变
                            if (batchObj.MarcRecordChanged == true)
                                strMarc = batchObj.MarcRecord;

                            // 观察XML记录是否被改变
                            if (batchObj.XmlRecordChanged == true)
                                strXmlBody = batchObj.XmlRecord;

                        }


                        if (bFoundRecord == true
                            && outputfile != null)
                        {
                            if (exportType == ExportFileType.BackupFile)
                            {
                                // 写磁盘
                                nRet = WriteRecordToBackupFile(
                                    outputfile,
                                    strDbName,
                                    strID,
                                    strMetaData,
                                    strXmlBody,
                                    baOutputTimeStamp,
                                    out strError);
                                if (nRet == -1)
                                {
                                    // 询问是否继续
                                    goto ERROR1;
                                }
                            }
                            else if (exportType == ExportFileType.ISO2709File)
                            {
                                // 写磁盘
                                nRet = WriteRecordToISO2709File(
                                    outputfile,
                                    strDbName,
                                    strID,
                                    strMarc,
                                    baOutputTimeStamp,
                                    targetEncoding,
                                    this.OutputCrLf,
                                    this.AddG01,
                                    this.Remove998,
                                    out strError);
                                if (nRet == -1)
                                {
                                    // 询问是否继续
                                    goto ERROR1;
                                }
                            }
                            else if (exportType == ExportFileType.XmlFile)
                            {
                                XmlDocument dom = new XmlDocument();

                                try
                                {
                                    dom.LoadXml(strXmlBody);

                                    ResPath respathtemp = new ResPath();
                                    respathtemp.Url = channel.Url;
                                    respathtemp.Path = strOutputPath;


                                    // DomUtil.SetAttr(dom.DocumentElement, "xmlns:dprms", DpNs.dprms);
                                    // 给根元素设置几个参数
                                    DomUtil.SetAttr(dom.DocumentElement, "path", DpNs.dprms, respathtemp.FullPath);
                                    DomUtil.SetAttr(dom.DocumentElement, "timestamp", DpNs.dprms, ByteArray.GetHexTimeStampString(baOutputTimeStamp));

                                    // DomUtil.SetAttr(dom.DocumentElement, "xmlns:marc", null);
                                    dom.DocumentElement.WriteTo(writer);
                                }
                                catch (Exception ex)
                                {
                                    strError = ex.Message;
                                    // 询问是否继续
                                    goto ERROR1;
                                }

                            }
                        }

                        // 删除
                        if (checkBox_export_delete.Checked == true)
                        {

                            byte[] baOutputTimeStamp1 = null;
                            strPath = strOutputPath;	// 得到实际的路径
                            lRet = channel.DoDeleteRes(
                                strPath,
                                baOutputTimeStamp,
                                strDeleteStyle,
                                out baOutputTimeStamp1,
                                out strError);
                            if (lRet == -1)
                            {
                                // 询问是否继续
                                goto ERROR1;
                            }
                            stop.SetMessage("已删除记录" + strPath + "  " + m_nRecordCount.ToString());
                        }

                        if (bFoundRecord == true)
                            m_nRecordCount++;


                        if (bAsc == true)
                        {
                            //progressBar_main.Value = (int)((nCur - nStart + 1) / ProgressRatio);
                            stop.SetProgressValue(nCur - nStart + 1);
                        }
                        else
                        {
                            // ?
                            // progressBar_main.Value = (int)((nStart - nCur + 1) / ProgressRatio);
                            stop.SetProgressValue(nStart - nCur + 1);
                        }


                        // 对已经作过的进行判断
                        if (bAsc == true && nCur >= nEnd)
                            break;
                        if (bAsc == false && nCur <= nEnd)
                            break;


                    } // end of for one database

                    stop.EndLoop();
                    stop.OnStop -= new StopEventHandler(this.DoStop);
                    stop.Initial("");

                    EnableControls(true);

                //}

            CONTINUEDBS:
                    strInfo += " : " + m_nRecordCount.ToString() + "条 (ID " + strRealStartNo + "-" + strRealEndNo + ")";

                }   // end of dbpaths loop


            }   // end of try
            finally
            {
                if (writer != null)
                {
                    writer.WriteEndElement();
                    writer.WriteEndDocument();
                    writer.Close();
                    writer = null;
                }

                if (outputfile != null)
                {
                    outputfile.Close();
                    outputfile = null;
                }

            }

            // END1:
            channel = null;

            if (checkBox_export_delete.Checked == true)
                strError = "数据导出和删除完成。\r\n---\r\n" + strInfo;
            else
                strError = "数据导出完成。\r\n---\r\n" + strInfo;

            WriteLog("结束输出");

            return 0;
        ERROR1:
            stop.EndLoop();
            stop.OnStop -= new StopEventHandler(this.DoStop);
            stop.Initial("");

            EnableControls(true);
            channel = null;
            return -1;
        }

		// 将Xml转换为MARC
		// 可供C#脚本调用
		public int GetMarc(string strXmlBody,
			out string strMarc,
			out string strError)
		{
			string strOutMarcSyntax = "";
			strMarc = "";

			// 将MARCXML格式的xml记录转换为marc机内格式字符串
			// parameters:
			//		bWarning	==true, 警告后继续转换,不严格对待错误; = false, 非常严格对待错误,遇到错误后不继续转换
			//		strMarcSyntax	指示marc语法,如果==""，则自动识别
			//		strOutMarcSyntax	out参数，返回marc，如果strMarcSyntax == ""，返回找到marc语法，否则返回与输入参数strMarcSyntax相同的值
			int nRet = MarcUtil.Xml2Marc(strXmlBody,
				true,  // true 比 false 要宽松 // false,
				this.CurMarcSyntax,
				out strOutMarcSyntax,
				out strMarc,
				out strError);
			if (nRet == -1)
				return -1;

			return 0;
		}

        // 在MARC记录中加入一个-01字段
        // 总是插入在第一个字段
        static int AddG01ToMarc(ref string strMARC,
            string strFieldContent,
            out string strError)
        {
            strError = "";

            if (strMARC.Length < 24)
            {
                strMARC = strMARC.PadRight(24, '*');
                strMARC += "-01" + strFieldContent + new string(MarcUtil.FLDEND, 1);
                return 1;
            }

            strMARC = strMARC.Insert(24, "-01" + strFieldContent + new string(MarcUtil.FLDEND, 1));

            /*
        // 如果原有记录中存在-01字段，则第一个-01字段将被覆盖
            // return:
            //		-1	出错
            //		0	没有找到指定的字段，因此将strField内容插入到适当位置了。
            //		1	找到了指定的字段，并且也成功用strField替换掉了。
            int nRet = MarcUtil.ReplaceField(
                ref strMARC,
                "-01",
                0,
                "-01" + strFieldContent);
            if (nRet == -1)
            {
                strError = "ReplaceField() error";
                return -1;
            }*/

            return 1;
        }

        // 去除MARC记录中的所有-01字段
        // return:
        //      -1  error
        //      0   not changed
        //      1   changed
        static int RemoveG01FromMarc(ref string strMARC,
            out string strError)
        {
            strError = "";

            if (strMARC.Length <=24)
                return 0;

            bool bChanged = false;

            for (; ; )
            {
                string strField = "";
                string strNextFieldName = "";
                // return:
                //		-1	出错
                //		0	所指定的字段没有找到
                //		1	找到。找到的字段返回在strField参数中
                int nRet = MarcUtil.GetField(strMARC,
                    "-01",
                    0,
                    out strField,
                    out strNextFieldName);
                if (nRet == -1)
                {
                    strError = "GetField() error";
                    return -1;
                }

                if (nRet == 0)
                    break;

                // return:
                //		-1	出错
                //		0	没有找到指定的字段，因此将strField内容插入到适当位置了。
                //		1	找到了指定的字段，并且也成功用strField替换掉了。
                nRet = MarcUtil.ReplaceField(
                    ref strMARC,
                    "-01",
                    0,
                    null);
                if (nRet == -1)
                {
                    strError = "ReplaceField() error";
                    return -1;
                }

                bChanged = true;
            }

            if (bChanged == true)
                return 1;

            return 0;
        }


		// 将记录写入ISO2709文件
		int WriteRecordToISO2709File(
			Stream outputfile,
			string strDbName,
			string strID,
			string strMarc,
			byte [] body_timestamp,
			Encoding targetEncoding,
			bool bOutputCrLf,
            bool bAddG01,
            bool bRemove998,
			out string strError)
		{

			int nRet = 0;

			string strPath = strDbName + "/" + strID;

			long lStart = outputfile.Position;	// 记忆起始位置


			ResPath respath = new ResPath();
			respath.Url = channel.Url;
			respath.Path = strPath;

            
                    // 去除MARC记录中的所有-01字段
        // return:
        //      -1  error
        //      0   not changed
        //      1   changed
            nRet = RemoveG01FromMarc(ref strMarc,
                out strError);
            if (nRet == -1)
                return -1;

            if (bAddG01 == true)
            {
                string strDt1000Path = "/" + strDbName + "/ctlno/" + strID.PadLeft(10, '0');
                string strTimestamp = ByteArray.GetHexTimeStampString(body_timestamp);

                nRet = AddG01ToMarc(ref strMarc,
                    strDt1000Path + "|" + strTimestamp,
                    out strError);
                if (nRet == -1)
                    return -1;
            }

            if (bRemove998 == true)
            {
                MarcRecord record = new MarcRecord(strMarc);
                record.select("field[@name='998']").detach();
                strMarc = record.Text;
            }

			byte [] baResult = null;
            // 将MARC机内格式转换为ISO2709格式
            // parameters:
            //		nMARCType	[in]MARC格式类型。0为UNIMARC 1为USMARC
            //		strSourceMARC		[in]机内格式MARC记录。
            //		targetEncoding	[in]输出ISO2709的编码方式为 UTF8 codepage-936等等
            //		baResult	[out]输出的ISO2709记录。字符集受nCharset参数控制。
            //					注意，缓冲区末尾不包含0字符。
            nRet = MarcUtil.CvtJineiToISO2709(
                strMarc,
                this.CurMarcSyntax,
                targetEncoding,
                out baResult,
                out strError);

			if (nRet == -1)
				return -1;

			outputfile.Write(baResult, 0, baResult.Length);

			if (bOutputCrLf == true)
			{
				baResult = new byte [2];
				baResult[0] = (byte)'\r';
				baResult[1] = (byte)'\n';
				outputfile.Write(baResult, 0, 2);
			}

			
			return 0;

			/*
			ERROR1:
				return -1;
			*/
		}

		// 将主记录和相关资源写入备份文件
		int WriteRecordToBackupFile(
			Stream outputfile,
			string strDbName,
			string strID,
			string strMetaData,
			string strXmlBody,
			byte [] body_timestamp,
			out string strError)
		{

            Debug.Assert(String.IsNullOrEmpty(strXmlBody) == false, "strXmlBody不能为空");

			string strPath = strDbName + "/" + strID;

			long lStart = outputfile.Position;	// 记忆起始位置

			byte [] length = new byte[8];

			outputfile.Write(length, 0, 8);	// 临时写点数据,占据记录总长度位置

			ResPath respath = new ResPath();
			respath.Url = channel.Url;
			respath.Path = strPath;

			// 加工元数据
            ExportUtil.ChangeMetaData(ref strMetaData,
				null,
				null,
				null,
				null,
				respath.FullPath,
				ByteArray.GetHexTimeStampString(body_timestamp));   // 2005/6/11

			// 向backup文件中保存第一个 res
			long lRet = Backup.WriteFirstResToBackupFile(
				outputfile,
				strMetaData,
				strXmlBody);

			// 其余

			string [] ids = null;

			// 得到Xml记录中所有<file>元素的id属性值
			int nRet = ExportUtil.GetFileIds(strXmlBody,
				out ids,
				out strError);
			if (nRet == -1) 
			{
				outputfile.SetLength(lStart);	// 把本次追加写入的全部去掉
				strError = "GetFileIds()出错，无法获得 XML 记录中的 <dprms:file>元素的 id 属性， 因此保存记录失败，原因: "+ strError;
				goto ERROR1;
			}


			nRet = WriteResToBackupFile(
				this,
				outputfile,
				respath.Path,
				ids,
				channel,
				stop,
				out strError);
			if (nRet == -1) 
			{
				outputfile.SetLength(lStart);	// 把本次追加写入的全部去掉
				strError = "WriteResToBackupFile()出错，因此保存记录失败，原因: "+ strError;
				goto ERROR1;
			}

			///


			// 写入总长度
			long lTotalLength = outputfile.Position - lStart - 8;
			byte[] data = BitConverter.GetBytes(lTotalLength);

			outputfile.Seek(lStart, SeekOrigin.Begin);
			outputfile.Write(data, 0, 8);
			outputfile.Seek(lTotalLength, SeekOrigin.Current);

			return 0;

			ERROR1:
				return -1;
		}

		// 下载资源，保存到备份文件
		public static int WriteResToBackupFile(
			IWin32Window owner,
			Stream outputfile,
			string strXmlRecPath,
			string [] res_ids,
            RmsChannel channel,
			DigitalPlatform.Stop stop,
			out string strError)
		{
			strError = "";


			long lRet;

			for(int i=0;i<res_ids.Length;i++)
			{
				Application.DoEvents();	// 出让界面控制权

				if (stop.State != 0)
				{
					DialogResult result = MessageBox.Show(owner,
						"确实要中断当前批处理操作?",
						"dp2batch",
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Question,
						MessageBoxDefaultButton.Button2);
					if (result == DialogResult.Yes)
					{
						strError = "用户中断";
						return -1;
					}
					else 
					{
						stop.Continue();
					}
				}

				string strID = res_ids[i].Trim();

				if (strID == "")
					continue;

				string strResPath = strXmlRecPath + "/object/" + strID;

				string strMetaData;

				if (stop != null)
					stop.SetMessage("正在下载 " + strResPath);

				long lResStart = 0;
				// 写res的头。
				// 如果不能预先确知整个res的长度，可以用随便一个lTotalLength值调用本函数，
				// 但是需要记忆下函数所返回的lStart，最后调用EndWriteResToBackupFile()。
				// 如果能预先确知整个res的长度，则最后不必调用EndWriteResToBackupFile()
				lRet = Backup.BeginWriteResToBackupFile(
					outputfile,
					0,	// 未知
					out lResStart);

				byte [] baOutputTimeStamp = null;
				string strOutputPath;

            REDO_GETRES:
				lRet = channel.GetRes(strResPath,
					(Stream)null,	// 故意不获取资源体
					stop,
					"metadata,timestamp,outputpath",
					null,
					out strMetaData,	// 但是要获得metadata
					out baOutputTimeStamp,
					out strOutputPath,
					out strError);
                if (lRet == -1)
                {
                    // TODO: 允许重试
                    DialogResult redo_result = MessageBox.Show(owner,
                        "获取记录 '" + strResPath + "' 时出现错误: " + strError + "\r\n\r\n重试，还是中断当前批处理操作?\r\n(Retry 重试；Cancel 中断批处理)",
                        "dp2batch",
                        MessageBoxButtons.RetryCancel,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button1);
                    if (redo_result == DialogResult.Cancel)
                        return -1;
                    goto
                        REDO_GETRES;
                }

				byte [] timestamp = baOutputTimeStamp;

				ResPath respath = new ResPath();
				respath.Url = channel.Url;
				respath.Path = strOutputPath;	// strResPath;

				// strMetaData还要加入资源id?
                ExportUtil.ChangeMetaData(ref strMetaData,
					strID,
					null,
					null,
					null,
					respath.FullPath,
					ByteArray.GetHexTimeStampString(baOutputTimeStamp));


				lRet = Backup.WriteResMetadataToBackupFile(outputfile,
					strMetaData);
				if (lRet == -1)
					return -1;

				long lBodyStart = 0;
				// 写res body的头。
				// 如果不能预先确知body的长度，可以用随便一个lBodyLength值调用本函数，
				// 但是需要记忆下函数所返回的lBodyStart，最后调用EndWriteResBodyToBackupFile()。
				// 如果能预先确知body的长度，则最后不必调用EndWriteResBodyToBackupFile()
				lRet = Backup.BeginWriteResBodyToBackupFile(
					outputfile,
					0, // 未知
					out lBodyStart);
				if (lRet == -1)
					return -1;

				if (stop != null)
					stop.SetMessage("正在下载 " + strResPath + " 的数据体");

            REDO_GETRES_1:
				lRet = channel.GetRes(strResPath,
					outputfile,
					stop,
					"content,data,timestamp", //"content,data,timestamp"
					timestamp,
					out strMetaData,
					out baOutputTimeStamp,
					out strOutputPath,
					out strError);
				if (lRet == -1) 
				{
					if (channel.ErrorCode == ChannelErrorCode.EmptyRecord)
					{
						// 空记录
					}
					else 
					{
                        // TODO: 允许重试
                        DialogResult redo_result = MessageBox.Show(owner,
                            "获取记录 '" + strResPath + "' 时出现错误: " + strError + "\r\n\r\n重试，还是中断当前批处理操作?\r\n(Retry 重试；Cancel 中断批处理)",
                            "dp2batch",
                            MessageBoxButtons.RetryCancel,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button1);
                        if (redo_result == DialogResult.Cancel)
                            return -1;
                        goto
                            REDO_GETRES_1;
					}
				}

				long lBodyLength = outputfile.Position - lBodyStart - 8;
				// res body收尾
				lRet = Backup.EndWriteResBodyToBackupFile(
					outputfile,
					lBodyLength,
					lBodyStart);
				if (lRet == -1)
					return -1;

				long lTotalLength = outputfile.Position - lResStart - 8;
				lRet = Backup.EndWriteResToBackupFile(
					outputfile,
					lTotalLength,
					lResStart);
				if (lRet == -1)
					return -1;

			}

			/*
			if (stop != null)
				stop.SetMessage("保存资源到备份文件全部完成");
			*/

			return 0;
		}


        // return:
        //		true	起始号为小号
        //		false	起始号为大号
        static bool GetDirection(
            string strStartNo,
            string strEndNo,
            out Int64 nStart,
            out Int64 nEnd)
        {
            bool bAsc = true;

            nStart = 0;
            nEnd = 9999999999;

            try
            {
                nStart = Convert.ToInt64(strStartNo);
            }
            catch
            {
            }

            try
            {
                nEnd = Convert.ToInt64(strEndNo);
            }
            catch
            {
            }


            if (nStart > nEnd)
                bAsc = false;
            else
                bAsc = true;

            return bAsc;
        }

#if NOOOOOOOOOOOO
		// return:
		//		true	起始号为小号
		//		false	起始号为大号
		bool GetDirection(out Int64 nStart,
			out Int64 nEnd)
		{
			bool bAsc = true;

			nStart = 0;
			nEnd = 9999999999;
			
			try 
			{
				nStart = Convert.ToInt64(textBox_startNo.Text);
			}
			catch 
			{
			}
				
			try 
			{
				nEnd = Convert.ToInt64(textBox_endNo.Text);
			}
			catch 
			{
			}


			if (nStart > nEnd)
				bAsc = false;
			else
				bAsc = true;

			return bAsc;
		}
#endif

        // 校验起止号
        // return:
        //      0   不存在记录
        //      1   存在记录
        int VerifyRange(RmsChannel channel,
            string strDbName,
            string strInputStartNo,
            string strInputEndNo,
            out string strOutputStartNo,
            out string strOutputEndNo,
            out string strError)
        {
            strError = "";
            strOutputStartNo = "";
            strOutputEndNo = "";

            bool bStartNotFound = false;
            bool bEndNotFound = false;

            // 如果输入参数中为空，则假定为“全部范围”
            if (strInputStartNo == "")
                strInputStartNo = "1";

            if (strInputEndNo == "")
                strInputEndNo = "9999999999";


            bool bAsc = true;

            Int64 nStart = 0;
            Int64 nEnd = 9999999999;


            try
            {
                nStart = Convert.ToInt64(strInputStartNo);
            }
            catch
            {
            }


            try
            {
                nEnd = Convert.ToInt64(strInputEndNo);
            }
            catch
            {
            }


            if (nStart > nEnd)
                bAsc = false;
            else
                bAsc = true;

            string strPath = strDbName + "/" + strInputStartNo;
            string strStyle = "outputpath";

            if (bAsc == true)
                strStyle += ",next,myself";
            else
                strStyle += ",prev,myself";

            string strResult;
            string strMetaData;
            byte[] baOutputTimeStamp;
            string strOutputPath;

            string strError0 = "";

            string strStartID = "";
            string strEndID = "";

            // 获得资源
            // return:
            //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
            //		0	成功
            long lRet = channel.GetRes(strPath,
                strStyle,
                out strResult,
                out strMetaData,
                out baOutputTimeStamp,
                out strOutputPath,
                out strError0);
            if (lRet == -1)
            {
                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                {
                    strStartID = strInputStartNo;
                    bStartNotFound = true;
                }
                else
                    strError += "校验startno时出错： " + strError0 + " ";

            }
            else
            {
                // 取得返回的id
                strStartID = ResPath.GetRecordId(strOutputPath);
            }

            if (strStartID == "")
            {
                strError = "strStartID为空..." + (string.IsNullOrEmpty(strError) == false? " : " + strError : "");
                return -1;
            }

            strPath = strDbName + "/" + strInputEndNo;

            strStyle = "outputpath";
            if (bAsc == true)
                strStyle += ",prev,myself";
            else
                strStyle += ",next,myself";

            // 获得资源
            // return:
            //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
            //		0	成功
            lRet = channel.GetRes(strPath,
                strStyle,
                out strResult,
                out strMetaData,
                out baOutputTimeStamp,
                out strOutputPath,
                out strError0);
            if (lRet == -1)
            {

                if (channel.ErrorCode == ChannelErrorCode.NotFound)
                {
                    strEndID = strInputEndNo;
                    bEndNotFound = true;
                }
                else
                {
                    strError += "校验endno时出错： " + strError0 + " ";
                }

            }
            else
            {
                // 取得返回的id
                strEndID = ResPath.GetRecordId(strOutputPath);
            }

            if (strEndID == "")
            {
                strError = "strEndID为空..." + (string.IsNullOrEmpty(strError) == false ? " : " + strError : ""); ;
                return -1;
            }

            ///
            bool bSkip = false;

            Int64 nTemp = 0;
            try
            {
                nTemp = Convert.ToInt64(strStartID);
            }
            catch
            {
                strError = "strStartID值 '" + strStartID + "' 不是数字...";
                return -1;
            }

            if (bAsc == true)
            {
                if (nTemp > nEnd)
                {
                    bSkip = true;
                }
            }
            else
            {
                if (nTemp < nEnd)
                {
                    bSkip = true;
                }
            }

            if (bSkip == false)
            {
                strOutputStartNo = strStartID;
            }


            ///

            bSkip = false;

            try
            {
                nTemp = Convert.ToInt64(strEndID);
            }
            catch
            {
                strError = "strEndID值 '" + strEndID + "' 不是数字...";
                return -1;
            }
            if (bAsc == true)
            {
                if (nTemp < nStart)
                {
                    bSkip = true;
                }
            }
            else
            {
                if (nTemp > nStart)
                {
                    bSkip = true;
                }
            }

            if (bSkip == false)
            {
                strOutputEndNo = strEndID;
            }

            if (bStartNotFound == true && bEndNotFound == true)
                return 0;

            return 1;
        }

        		// 校验起止号
        // return:
        //      0   不存在记录
        //      1   存在记录
        int VerifyRange(RmsChannel channel,
            string strDbName,
            out string strError)
        {
            strError = "";

            string strOutputStartNo = "";
            string strOutputEndNo = "";
            int nRet = VerifyRange(channel,
                strDbName,
                this.textBox_startNo.Text,
                this.textBox_endNo.Text,
                out strOutputStartNo,
                out strOutputEndNo,
                out strError);
            if (nRet == -1)
                return -1;

            this.textBox_startNo.Text = strOutputStartNo;
            this.textBox_endNo.Text = strOutputEndNo;

            return nRet;
        }

#if NOOOOOOOOOOOOOOOOOOOOOOOOOO
		// 校验起止号
        // return:
        //      0   不存在记录
        //      1   存在记录
        int VerifyRange(RmsChannel channel,
			string strDbName,
			out string strError)
		{
            bool bStartNotFound = false;
            bool bEndNotFound = false;

			strError = "";

			// 如果edit中为空，则假定为“全部范围”
			if (textBox_startNo.Text == "")
				textBox_startNo.Text = "1";

			if (textBox_endNo.Text == "")
				textBox_endNo.Text = "9999999999";


			bool bAsc = true;

			Int64 nStart = 0;
			Int64 nEnd = 9999999999;
			
			
			try 
			{
				nStart = Convert.ToInt64(textBox_startNo.Text);
			}
			catch 
			{
			}

				
			try 
			{
				nEnd = Convert.ToInt64(textBox_endNo.Text);
			}
			catch 
			{
			}


			if (nStart > nEnd)
				bAsc = false;
			else
				bAsc = true;

			string strPath = strDbName + "/" + textBox_startNo.Text;
			string strStyle = "outputpath";

			if (bAsc == true)
				strStyle += ",next,myself";
			else 
				strStyle += ",prev,myself";

			string strResult;
			string strMetaData;
			byte [] baOutputTimeStamp;
			string strOutputPath;

			string strError0  = "";

			string strStartID = "";
			string strEndID = "";

			// 获得资源
			// return:
			//		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
			//		0	成功
			long lRet = channel.GetRes(strPath,
				strStyle,
				out strResult,
				out strMetaData,
				out baOutputTimeStamp,
				out strOutputPath,
				out strError0);
			if (lRet == -1) 
			{
				if (channel.ErrorCode == ChannelErrorCode.NotFound)
				{
					strStartID = textBox_startNo.Text;
                    bStartNotFound = true;
				}
				else 
					strError += "校验startno时出错： " + strError0 + " ";
				
			}
			else 
			{
				// 取得返回的id
				strStartID = ResPath.GetRecordId(strOutputPath);


			}

			if (strStartID == "")
			{
				strError = "strStartID为空...";
				return -1;
			}

			strPath = strDbName + "/" + textBox_endNo.Text;

			strStyle = "outputpath";
			if (bAsc == true)
				strStyle += ",prev,myself";
			else 
				strStyle += ",next,myself";

			// 获得资源
			// return:
			//		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
			//		0	成功
			lRet = channel.GetRes(strPath,
				strStyle,
				out strResult,
				out strMetaData,
				out baOutputTimeStamp,
				out strOutputPath,
				out strError0);
			if (lRet == -1) 
			{

				if (channel.ErrorCode == ChannelErrorCode.NotFound)
				{
					strEndID = textBox_endNo.Text;
                    bEndNotFound = true;
				}
				else
					strError += "校验endno时出错： " + strError0 + " ";

			}
			else 
			{
				// 取得返回的id
				strEndID = ResPath.GetRecordId(strOutputPath);

			}

			if (strEndID == "")
			{
				strError = "strEndID为空...";
				return -1;
			}

			///
			bool bSkip = false;

			Int64 nTemp = 0;
			try 
			{
				nTemp = Convert.ToInt64(strStartID);
			}
			catch
			{
				strError = "strStartID值 '" + strStartID + "' 不是数字...";
				return -1;
			}

			if (bAsc == true) 
			{
				if (nTemp > nEnd)
				{
					bSkip = true;
				}
			}
			else
			{
				if (nTemp < nEnd)
				{
					bSkip = true;
				}
			}

			if (bSkip == false) 
			{
				textBox_startNo.Text = strStartID;
			}


			///

			bSkip = false;

			try 
			{
				nTemp = Convert.ToInt64(strEndID);
			}
			catch
			{
				strError = "strEndID值 '" + strEndID + "' 不是数字...";
				return -1;
			}
			if (bAsc == true) 
			{
				if (nTemp < nStart)
				{
					bSkip = true;
				}
			}
			else
			{
				if (nTemp > nStart)
				{
					bSkip = true;
				}
			}

			if (bSkip == false) 
			{
				textBox_endNo.Text = strEndID;
			}

            if (bStartNotFound == true && bEndNotFound == true)
			    return 0;

            return 1;
		}
#endif

		private void treeView_rangeRes_AfterSelect(object sender, System.Windows.Forms.TreeViewEventArgs e)
		{
            /*
			if (treeView_rangeRes.CheckBoxes == false
                && treeView_rangeRes.SelectedNode == null)
				return;
             */

            List<string> paths = treeView_rangeRes.GetCheckedDatabaseList();

            if (paths.Count == 0)
            {
                textBox_dbPath.Text = "";
                return;
            }

            // 多个数据库路径之间用';'隔开
            string strText = "";
            for (int i = 0; i < paths.Count; i++)
            {
                if (strText != "")
                    strText += ";";
                strText += paths[i];
            }

            textBox_dbPath.Text = strText;

            /*
			if (treeView_rangeRes.SelectedNode.ImageIndex != ResTree.RESTYPE_DB) 
			{
				textBox_dbPath.Text = "";
				return;
			}

			ResPath respath = new ResPath(treeView_rangeRes.SelectedNode);

			textBox_dbPath.Text = respath.FullPath;
             */

            // 当选择发生改变后，如果当前在“全部”状态，则要重设起止范围，以免误用了先前缩小过的其他库的范围
            if (this.radioButton_all.Checked == true)
            {
                this.textBox_startNo.Text = "1";
                this.textBox_endNo.Text = "9999999999";
            }

		}

        private void radioButton_all_CheckedChanged(object sender, System.EventArgs e)
		{
			if (radioButton_all.Checked == true
                && m_nPreventNest == 0)
			{
                m_nPreventNest++;
				this.textBox_startNo.Text = "1";
				this.textBox_endNo.Text = "9999999999";
                m_nPreventNest--;
			}
		
		}

		void EnableControls(bool bEnabled)
		{
			textBox_startNo.Enabled = bEnabled;

			textBox_endNo.Enabled = bEnabled;

			checkBox_verifyNumber.Enabled = bEnabled;

			checkBox_forceLoop.Enabled = bEnabled;

			treeView_rangeRes.Enabled = bEnabled;

			radioButton_startEnd.Enabled = bEnabled;

			radioButton_all.Enabled = bEnabled;

			checkBox_export_delete.Enabled = bEnabled;

			///

			this.textBox_import_dbMap.Enabled = bEnabled;

			textBox_import_fileName.Enabled = bEnabled;

			textBox_import_range.Enabled = bEnabled;

			this.button_import_dbMap.Enabled = bEnabled;
			button_import_findFileName.Enabled = bEnabled;

            this.checkBox_import_fastMode.Enabled = bEnabled;
            this.checkBox_export_fastMode.Enabled = bEnabled;
		}

		private void menuItem_serversCfg_Click(object sender, System.EventArgs e)
		{
			ServersDlg dlg = new ServersDlg();
            MainForm.SetControlFont(dlg, this.DefaultFont);

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

			ServerCollection newServers = Servers.Dup();

			dlg.Servers = newServers;

            //dlg.StartPosition = FormStartPosition.CenterScreen;
			//dlg.ShowDialog(this);
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

			// treeView_rangeRes.Servers = this.Servers;
			treeView_rangeRes.Fill(null);
		}

		private void menuItem_copyright_Click(object sender, System.EventArgs e)
		{
			CopyrightDlg dlg = new CopyrightDlg();
            MainForm.SetControlFont(dlg, this.DefaultFont);

			dlg.StartPosition = FormStartPosition.CenterScreen;
			dlg.ShowDialog(this);

		}

		private void menuItem_projectManage_Click(object sender, System.EventArgs e)
		{
			ProjectManageDlg dlg = new ProjectManageDlg();
            MainForm.SetControlFont(dlg, this.DefaultFont);

            dlg.DataDir = this.DataDir;
            dlg.ProjectsUrl = "http://dp2003.com/dp2batch/projects/projects.xml";
            dlg.HostName = "dp2Batch";
			dlg.scriptManager = this.scriptManager;
			dlg.AppInfo = AppInfo;	
			dlg.StartPosition = FormStartPosition.CenterScreen;
			dlg.ShowDialog(this);
		}

        // 启动运行一个方案
		private void menuItem_run_Click(object sender, System.EventArgs e)
		{
		
		}

		private void scriptManager_CreateDefaultContent(object sender, CreateDefaultContentEventArgs e)
		{
			string strPureFileName = Path.GetFileName(e.FileName);

			if (String.Compare(strPureFileName, "main.cs", true) == 0)
			{
				CreateDefaultMainCsFile(e.FileName);
				e.Created = true;
			}
			else if (String.Compare(strPureFileName, "marcfilter.fltx", true) == 0)
			{
				CreateDefaultMarcFilterFile(e.FileName);
				e.Created = true;
			}
			else 
			{
				e.Created = false;
			}

		}

		// 创建缺省的main.cs文件
		public static int CreateDefaultMainCsFile(string strFileName)
		{

			StreamWriter sw = new StreamWriter(strFileName, false, Encoding.UTF8);
			sw.WriteLine("using System;");
			sw.WriteLine("using System.Windows.Forms;");
			sw.WriteLine("using System.IO;");
			sw.WriteLine("using System.Text;");
			sw.WriteLine("");

			sw.WriteLine("using DigitalPlatform.MarcDom;");
			sw.WriteLine("using DigitalPlatform.Statis;");
			sw.WriteLine("using dp2Batch;");

			sw.WriteLine("public class MyBatch : Batch");

			sw.WriteLine("{");

			sw.WriteLine("	public override void OnBegin(object sender, BatchEventArgs e)");
			sw.WriteLine("	{");
			sw.WriteLine("	}");


			sw.WriteLine("}");
			sw.Close();

			return 0;
		}

		// 创建缺省的marcfilter.fltx文件
		public static int CreateDefaultMarcFilterFile(string strFileName)
		{

			StreamWriter sw = new StreamWriter(strFileName, false, Encoding.UTF8);

			sw.WriteLine("<?xml version='1.0' encoding='utf-8'?>");
			sw.WriteLine("<filter>");
			sw.WriteLine("<using>");
			sw.WriteLine("<![CDATA[");
			sw.WriteLine("using System;");
			sw.WriteLine("using System.IO;");
			sw.WriteLine("using System.Text;");
			sw.WriteLine("using System.Windows.Forms;");
			sw.WriteLine("using DigitalPlatform.MarcDom;");
			sw.WriteLine("using DigitalPlatform.Marc;");

			sw.WriteLine("using dp2Batch;");

			sw.WriteLine("]]>");
			sw.WriteLine("</using>");
			sw.WriteLine("	<record>");
			sw.WriteLine("		<def>");
			sw.WriteLine("		<![CDATA[");
			sw.WriteLine("			int i;");
			sw.WriteLine("			int j;");
			sw.WriteLine("		]]>");
			sw.WriteLine("		</def>");
			sw.WriteLine("		<begin>");
			sw.WriteLine("		<![CDATA[");
			sw.WriteLine("			MessageBox.Show(\"record data:\" + this.Data);");
			sw.WriteLine("		]]>");
			sw.WriteLine("		</begin>");
			sw.WriteLine("			 <field name=\"200\">");
			sw.WriteLine("");
			sw.WriteLine("			 </field>");
			sw.WriteLine("		<end>");
			sw.WriteLine("		<![CDATA[");
			sw.WriteLine("");
			sw.WriteLine("			j ++;");
			sw.WriteLine("		]]>");
			sw.WriteLine("		</end>");
			sw.WriteLine("	</record>");
			sw.WriteLine("</filter>");

			sw.Close();

			return 0;
		}

        private void treeView_rangeRes_AfterCheck(object sender, TreeViewEventArgs e)
        {
            treeView_rangeRes_AfterSelect(sender, e);
        }

        private void menuItem_openDataFolder_Click(object sender, EventArgs e)
        {
            try
            {
                System.Diagnostics.Process.Start(this.DataDir);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }

        }

        // 重建检索点
        private void menuItem_rebuildKeys_Click(object sender, EventArgs e)
        {
            DoRebuildKeys();
        }

        // 重建检索点
        // TODO: 需要改造为在不校准首位号的情况下进度条也要显示正确。可参考DoExportFile()
        // parameters:
        void DoRebuildKeys()
        {
            string strError = "";
            int nRet = 0;
            long lRet = 0;

            string strInfo = "";    // 汇总信息，在完成后显示

        //      bClearKeysAtBegin   批处理开始的时候清除了所有的keys表
        //      bDeleteOldKeysPerRecord 做每条记录的时候是否要先删除属于这条记录的旧的检索点。
            bool bClearKeysAtBegin = true;
            bool bDeleteOldKeysPerRecord = false;

            m_nRecordCount = -1;

            if (textBox_dbPath.Text == "")
            {
                MessageBox.Show(this, "尚未选择要重建检索点的数据库 ...");
                return;
            }

            DialogResult result = MessageBox.Show(this,
                "确实要对下列数据库\r\n---\r\n"+this.textBox_dbPath.Text.Replace(";","\r\n")+"\r\n---\r\n进行重建检索点的操作?",
                "dp2batch",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result == DialogResult.No)
                return;

            RebuildKeysDialog option_dlg = new RebuildKeysDialog();
            MainForm.SetControlFont(option_dlg, this.DefaultFont);
            option_dlg.StartPosition = FormStartPosition.CenterScreen;
            option_dlg.ShowDialog(this);
            if (option_dlg.DialogResult == DialogResult.Cancel)
                return;

            if (option_dlg.WholeMode == true)
            {
                bClearKeysAtBegin = true;
                bDeleteOldKeysPerRecord = false;
            }
            else
            {
                bClearKeysAtBegin = false;
                bDeleteOldKeysPerRecord = true;
            }

            string[] dbpaths = textBox_dbPath.Text.Split(new char[] { ';' });

            // 如果为单库输出
            if (dbpaths.Length == 1)
            {
                // 否则移到DoExportFile()函数里面去校验
                ResPath respath = new ResPath(dbpaths[0]);

                channel = this.Channels.GetChannel(respath.Url);

                string strDbName = respath.Path;

                // 校验起止号
                if (checkBox_verifyNumber.Checked == true)
                {
                    nRet = VerifyRange(channel,
                        strDbName,
                        out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, strError);
                }
                else
                {
                    if (this.textBox_startNo.Text == "")
                    {
                        strError = "尚未指定起始号";
                        goto ERROR1;
                    }
                    if (this.textBox_endNo.Text == "")
                    {
                        strError = "尚未指定结束号";
                        goto ERROR1;
                    }
                }
            }
            else
            {
                Debug.Assert(dbpaths.Length > 1, "");

                // 多库输出。修改界面要素，表示针对每个库都是全库处理
                this.radioButton_all.Checked = true;
                this.textBox_startNo.Text = "1";
                this.textBox_endNo.Text = "9999999999";
            }


            stop.OnStop += new StopEventHandler(this.DoStop);
            stop.Initial("正在重建检索点");
            stop.BeginLoop();


            EnableControls(false);
            try
            {

                // TODO: 如果是多库输出，是否要对非“全部”的起止号范围进行警告? 因为后面是强迫按照全部来进行的

                for (int f = 0; f < dbpaths.Length; f++)
                {
                    string strOneDbPath = dbpaths[f];

                    ResPath respath = new ResPath(strOneDbPath);

                    channel = this.Channels.GetChannel(respath.Url);

                    string strDbName = respath.Path;

                    if (String.IsNullOrEmpty(strInfo) == false)
                        strInfo += "\r\n";

                    strInfo += "" + strDbName;

                    // 实际处理的首尾号
                    string strRealStartNo = "";
                    string strRealEndNo = "";
                    /*
                    DialogResult result;
                    if (checkBox_export_delete.Checked == true)
                    {
                        result = MessageBox.Show(this,
                            "确实要删除" + respath.Path + "内指定范围的记录?\r\n\r\n---------\r\n(是)删除 (否)放弃批处理",
                            "dp2batch",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                        if (result != DialogResult.Yes)
                            continue;
                    }*/

                    // 

                    // 如果为多库重建
                    if (dbpaths.Length > 1)
                    {
                        // 如果为全选
                        if (this.radioButton_all.Checked == true
                            || f > 0)
                        {
                            // 恢复为最大范围
                            this.textBox_startNo.Text = "1";
                            this.textBox_endNo.Text = "9999999999";
                        }

                        // 校验起止号
                        if (checkBox_verifyNumber.Checked == true)
                        {
                            nRet = VerifyRange(channel,
                                strDbName,
                                out strError);
                            if (nRet == -1)
                                MessageBox.Show(this, strError);

                            if (nRet == 0)
                            {
                                // 库中无记录
                                AutoCloseMessageBox.Show(this, "数据库 " + strDbName + " 中无记录。");
                                strInfo += "(无记录)";

                                /*
                                if (bClearKeysAtBegin == true)
                                {
                                    // 结束Refresh数据库定义
                                    lRet = channel.DoRefreshDB(
                                        "end",
                                        strDbName,
                                        false,  // 此参数此时无用
                                        out strError);
                                    if (lRet == -1)
                                        goto ERROR1;
                                }
                                 * */

                                continue;
                            }
                        }
                        else
                        {
                            if (this.textBox_startNo.Text == "")
                            {
                                strError = "尚未指定起始号";
                                goto ERROR1;
                            }
                            if (this.textBox_endNo.Text == "")
                            {
                                strError = "尚未指定结束号";
                                goto ERROR1;
                            }

                        }
                    }


                    Int64 nStart;
                    Int64 nEnd;
                    Int64 nCur;
                    bool bAsc = GetDirection(
                        this.textBox_startNo.Text,
                        this.textBox_endNo.Text,
                        out nStart,
                        out nEnd);

                    // 设置进度条范围
                    Int64 nMax = nEnd - nStart;
                    if (nMax < 0)
                        nMax *= -1;
                    nMax++;

                    /*
                    ProgressRatio = nMax / 10000;
                    if (ProgressRatio < 1.0)
                        ProgressRatio = 1.0;

                    progressBar_main.Minimum = 0;
                    progressBar_main.Maximum = (int)(nMax / ProgressRatio);
                    progressBar_main.Value = 0;
                     * */
                    stop.SetProgressRange(0, nMax);

                    // Refresh数据库定义
                    lRet = channel.DoRefreshDB(
                        "begin",
                        strDbName,
                        bClearKeysAtBegin == true ? true : false,
                        out strError);
                    if (lRet == -1)
                        goto ERROR1;


                    bool bFirst = true;	// 是否为第一次取记录

                    string strID = this.textBox_startNo.Text;

                    m_nRecordCount = 0;
                    // 循环
                    for (; ; )
                    {
                        Application.DoEvents();	// 出让界面控制权

                        if (stop.State != 0)
                        {
                            result = MessageBox.Show(this,
                                "确实要中断当前批处理操作?",
                                "dp2batch",
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

                        string strDirectionComment = "";

                        string strStyle = "";

                        strStyle = "timestamp,outputpath";	// 优化

                        if (bDeleteOldKeysPerRecord == true)
                            strStyle += ",forcedeleteoldkeys";


                        if (bFirst == true)
                        {
                            // 注：如果不校验首号，只有强制循环的情况下，才能不需要next风格
                            strStyle += "";
                        }
                        else
                        {
                            if (bAsc == true)
                            {
                                strStyle += ",next";
                                strDirectionComment = "的后一条记录";
                            }
                            else
                            {
                                strStyle += ",prev";
                                strDirectionComment = "的前一条记录";
                            }
                        }

                        string strPath = strDbName + "/" + strID;
                        string strOutputPath = "";

                        bool bFoundRecord = false;

                        bool bNeedRetry = true;

                    REDO_REBUILD:
                        // 获得资源
                        // return:
                        //		-1	出错。具体出错原因在this.ErrorCode中。this.ErrorInfo中有出错信息。
                        //		0	成功
                        lRet = channel.DoRebuildResKeys(strPath,
                            strStyle,
                            out strOutputPath,
                            out strError);
                        if (lRet == -1)
                        {
                            if (channel.ErrorCode == ChannelErrorCode.NotFound)
                            {
                                if (bFirst == true)
                                {
                                    // 如果要强制循环
                                    if (checkBox_forceLoop.Checked == true)
                                    {
                                        AutoCloseMessageBox.Show(this, "您为数据库 "+strDbName+" 指定的首记录 " + strID + strDirectionComment + " 不存在。\r\n\r\n按 确认 继续向后找。");
                                        bFirst = false;
                                        goto CONTINUE;
                                    }
                                    else
                                    {
                                        // 如果不要强制循环，此时也不能结束，否则会让用户以为数据库里面根本没有数据
                                        AutoCloseMessageBox.Show(this, "您为数据库 " + strDbName + " 指定的首记录 " + strID + strDirectionComment + " 不存在。\r\n\r\n(注：为避免出现此提示，可在操作前勾选“校准首尾ID”)\r\n\r\n按 确认 继续向后找...");
                                        bFirst = false;
                                        goto CONTINUE;
                                    }
                                }
                                else
                                {
                                    Debug.Assert(bFirst == false, "");

                                    if (bFirst == true)
                                    {
                                        strError = "记录 " + strID + strDirectionComment + " 不存在。处理结束。";
                                    }
                                    else
                                    {
                                        if (bAsc == true)
                                            strError = "记录 " + strID + " 是最末一条记录。处理结束。";
                                        else
                                            strError = "记录 " + strID + " 是最前一条记录。处理结束。";
                                    }

                                    if (dbpaths.Length > 1)
                                        break;  // 多库情况，继续其它库循环
                                    else
                                    {
                                        bNeedRetry = false; // 单库情况，也没有必要出现重试对话框
                                        MessageBox.Show(this, strError);
                                        break;
                                    }
                                }

                            }
                            else if (channel.ErrorCode == ChannelErrorCode.EmptyRecord)
                            {
                                bFirst = false;
                                // bFoundRecord = false;
                                // 把id解析出来
                                strID = ResPath.GetRecordId(strOutputPath);
                                goto CONTINUE;

                            }

                            // 允许重试
                            if (bNeedRetry == true)
                            {
                                DialogResult redo_result = MessageBox.Show(this,
                                    "重建检索点 记录 '" + strPath + "' (style='" + strStyle + "')时出现错误: " + strError + "\r\n\r\n重试，还是中断当前批处理操作?\r\n(Retry 重试；Cancel 中断批处理)",
                                    "dp2batch",
                                    MessageBoxButtons.RetryCancel,
                                    MessageBoxIcon.Question,
                                    MessageBoxDefaultButton.Button1);
                                if (redo_result == DialogResult.Cancel)
                                    goto ERROR1;
                                goto
                                    REDO_REBUILD;
                            }
                            else
                            {
                                goto ERROR1;
                            }

                        } // end of nRet == -1

                        bFirst = false;

                        bFoundRecord = true;

                        // 把id解析出来
                        strID = ResPath.GetRecordId(strOutputPath);
                        stop.SetMessage("已重建检索点 记录 " + strOutputPath + "  " + m_nRecordCount.ToString());

                        if (String.IsNullOrEmpty(strRealStartNo) == true)
                        {
                            strRealStartNo = strID;
                        }

                        strRealEndNo = strID;

                    CONTINUE:

                        // 是否超过循环范围
                        try
                        {
                            nCur = Convert.ToInt64(strID);
                        }
                        catch
                        {
                            // ???
                            nCur = 0;
                        }

                        if (bAsc == true && nCur > nEnd)
                            break;
                        if (bAsc == false && nCur < nEnd)
                            break;

                        if (bFoundRecord == true)
                            m_nRecordCount++;

                        //
                        //

                        if (bAsc == true)
                        {
                            // progressBar_main.Value = (int)((nCur - nStart + 1) / ProgressRatio);
                            stop.SetProgressValue(nCur - nStart + 1);
                        }
                        else
                        {
                            // ?
                            // progressBar_main.Value = (int)((nStart - nCur + 1) / ProgressRatio);
                            stop.SetProgressValue(nStart - nCur + 1);
                        }


                        // 对已经作过的进行判断
                        if (bAsc == true && nCur >= nEnd)
                            break;
                        if (bAsc == false && nCur <= nEnd)
                            break;
                    }

                    if (bClearKeysAtBegin == true)
                    {
                        // 结束Refresh数据库定义
                        lRet = channel.DoRefreshDB(
                            "end",
                            strDbName,
                            false,  // 此参数此时无用
                            out strError);
                        if (lRet == -1)
                            goto ERROR1;
                    }

                    strInfo += " : " + m_nRecordCount.ToString() + "条 (ID " + strRealStartNo + "-" + strRealEndNo + ")";

                }   // end of dbpaths loop


            }   // end of try
            finally
            {
                EnableControls(true);

                stop.EndLoop();
                stop.OnStop -= new StopEventHandler(this.DoStop);
                stop.Initial("");
            }

            strError = "重建检索点完成。\r\n---\r\n" + strInfo;

        // END1:

            MessageBox.Show(this, strError);
            return;

        ERROR1:
            MessageBox.Show(this, strError);
        }


        private void tabControl_main_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabControl_main.SelectedTab == this.tabPage_range)
            {
                this.menuItem_rebuildKeys.Enabled = true;
            }
            else
            {
                this.menuItem_rebuildKeys.Enabled = false;
            }
        }

        private void menuItem_rebuildKeysByDbnames_Click(object sender, EventArgs e)
        {
            string strError = "";

            bool bHasClipboardObject = false;
            IDataObject iData = Clipboard.GetDataObject();
            if (iData == null
                || iData.GetDataPresent(typeof(string)) == false)
                bHasClipboardObject = false;
            else
                bHasClipboardObject = true;

            if (bHasClipboardObject == false)
            {
                strError = "当前Windows剪贴板中并没有包含数据库名信息";
                goto ERROR1;
            }

            string strDbnames = (string)iData.GetData(typeof(string));
            if (String.IsNullOrEmpty(strDbnames) == true)
            {
                strError = "当前Windows剪贴板中的数据库名信息为空";
                goto ERROR1;
            }

            int nRet = strDbnames.IndexOf("?"); // .asmx?
            if (nRet == -1)
            {
                string strText = strDbnames;
                if (strText.Length > 1000)
                    strText = strText.Substring(0, 1000) + "...";

                strError = "当前Windows剪贴板中所包含的字符串 '" + strText + "' 不是数据库名格式";
                goto ERROR1;
            }

            List<string> paths = new List<string>();
            string[] parts = strDbnames.Split(new char[] {';'});
            for (int i = 0; i < parts.Length; i++)
            {
                string strPart = parts[i].Trim();
                if (String.IsNullOrEmpty(strPart) == true)
                    continue;
                paths.Add(strPart);
            }

            Cursor oldCursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            Application.DoEvents(); // 让光标形状显示出来

            bool bRet = this.treeView_rangeRes.SelectDatabases(paths, out strError);

            this.Cursor = oldCursor;

            if (bRet == false)
            {
                strError = "下列数据库路径在资源树中不存在: \r\n---\r\n" + strError + "\r\n---\r\n\r\n请(用主菜单“文件/缺省帐户管理”命令)向资源树中添加新的服务器节点，或刷新资源树后，再重新进行重建检索点的操作";
                goto ERROR1;
            }

            DoRebuildKeys();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        int m_nPreventNest = 0;

        private void textBox_startNo_TextChanged(object sender, EventArgs e)
        {
            if (m_nPreventNest == 0)
            {
                m_nPreventNest++;   // 防止radioButton_all_CheckedChanged()随动
                this.radioButton_startEnd.Checked = true;
                m_nPreventNest--;
            }
        }

        private void textBox_endNo_TextChanged(object sender, EventArgs e)
        {
            if (m_nPreventNest == 0)
            {
                m_nPreventNest++;      // 防止radioButton_all_CheckedChanged()随动
                this.radioButton_startEnd.Checked = true;
                m_nPreventNest--;
            }
        }

        private void checkBox_export_delete_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_export_delete.Checked == true)
                this.checkBox_export_fastMode.Visible = true;
            else
                this.checkBox_export_fastMode.Visible = false;
        }

        public bool IsFirstRun
        {
            get
            {
                try
                {
                    if (ApplicationDeployment.CurrentDeployment.IsFirstRun == true)
                        return true;

                    return false;
                }
                catch
                {
                    return false;
                }

            }
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
                {
                    return GuiUtil.GetDefaultFont();    // 2015/5/8
                    // return null;
                }

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


	}

	public enum ExportFileType
	{
		BackupFile = 0,
		XmlFile = 1,
		ISO2709File = 2,
	}

	public class MyFilterDocument : FilterDocument
	{
		public Batch Batch = null;
	}

	// 浏览记录到达
	public delegate void CheckTargetDbEventHandler(object sender,
	CheckTargetDbEventArgs e);

	public class CheckTargetDbEventArgs: EventArgs
	{
		public string DbFullPath = "";	// 目标数据库全路径
        public string CurrentMarcSyntax = "";   // 当前记录的 MARC 格式 2014/5/28
		public bool Cancel = false;	// 是否需要中断
		public string ErrorInfo = "";	// 回调期间发生的错误信息
	}

}
