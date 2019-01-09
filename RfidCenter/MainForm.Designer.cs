namespace RfidCenter
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_start = new System.Windows.Forms.TabPage();
            this.splitContainer_start = new System.Windows.Forms.SplitContainer();
            this.listView_chips = new System.Windows.Forms.ListView();
            this.columnHeader_uid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_pii = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label_message = new System.Windows.Forms.Label();
            this.button_cancel = new System.Windows.Forms.Button();
            this.tabPage_operHistory = new System.Windows.Forms.TabPage();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.tabPage_cfg = new System.Windows.Forms.TabPage();
            this.button_setDefaultThreshold = new System.Windows.Forms.Button();
            this.textBox_cfg_shreshold = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.comboBox_deviceList = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_replicationStart = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBox_speak = new System.Windows.Forms.CheckBox();
            this.checkBox_beep = new System.Windows.Forms.CheckBox();
            this.checkBox_cfg_savePasswordLong = new System.Windows.Forms.CheckBox();
            this.textBox_cfg_location = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_cfg_password = new System.Windows.Forms.TextBox();
            this.textBox_cfg_userName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_cfg_dp2LibraryServerUrl = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.toolStrip_server = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_cfg_setXeServer = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_cfg_setHongnibaServer = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolButton_stop = new System.Windows.Forms.ToolStripButton();
            this.toolStripDropDownButton_stopAll = new System.Windows.Forms.ToolStripDropDownButton();
            this.ToolStripMenuItem_stopAll = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_autoInventory = new System.Windows.Forms.ToolStripButton();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.MenuItem_file = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_start = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_reopen = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_refresh = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_clearFingerprintCacheFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItem_exit = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_testing = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openReader = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_closeReader = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_inventory = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_getTagInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_readBlocks = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_throwException = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_testWriteContentToNewChip = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_testLockBlocks = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_testRfidChannel = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_help = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_setupDriver = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_manual = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_about = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_startCapture = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_stopCapture = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl_main.SuspendLayout();
            this.tabPage_start.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_start)).BeginInit();
            this.splitContainer_start.Panel1.SuspendLayout();
            this.splitContainer_start.Panel2.SuspendLayout();
            this.splitContainer_start.SuspendLayout();
            this.tabPage_operHistory.SuspendLayout();
            this.tabPage_cfg.SuspendLayout();
            this.toolStrip_server.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_main
            // 
            this.tabControl_main.Controls.Add(this.tabPage_start);
            this.tabControl_main.Controls.Add(this.tabPage_operHistory);
            this.tabControl_main.Controls.Add(this.tabPage_cfg);
            this.tabControl_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_main.Location = new System.Drawing.Point(0, 65);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(960, 443);
            this.tabControl_main.TabIndex = 7;
            // 
            // tabPage_start
            // 
            this.tabPage_start.Controls.Add(this.splitContainer_start);
            this.tabPage_start.Controls.Add(this.button_cancel);
            this.tabPage_start.Location = new System.Drawing.Point(4, 33);
            this.tabPage_start.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_start.Name = "tabPage_start";
            this.tabPage_start.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage_start.Size = new System.Drawing.Size(952, 406);
            this.tabPage_start.TabIndex = 0;
            this.tabPage_start.Text = "开始";
            this.tabPage_start.UseVisualStyleBackColor = true;
            // 
            // splitContainer_start
            // 
            this.splitContainer_start.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_start.BackColor = System.Drawing.Color.Transparent;
            this.splitContainer_start.Location = new System.Drawing.Point(7, 8);
            this.splitContainer_start.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer_start.Name = "splitContainer_start";
            // 
            // splitContainer_start.Panel1
            // 
            this.splitContainer_start.Panel1.Controls.Add(this.listView_chips);
            // 
            // splitContainer_start.Panel2
            // 
            this.splitContainer_start.Panel2.Controls.Add(this.label_message);
            this.splitContainer_start.Size = new System.Drawing.Size(939, 306);
            this.splitContainer_start.SplitterDistance = 252;
            this.splitContainer_start.SplitterWidth = 15;
            this.splitContainer_start.TabIndex = 5;
            // 
            // listView_chips
            // 
            this.listView_chips.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_uid,
            this.columnHeader_pii});
            this.listView_chips.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_chips.FullRowSelect = true;
            this.listView_chips.Location = new System.Drawing.Point(0, 0);
            this.listView_chips.Name = "listView_chips";
            this.listView_chips.Size = new System.Drawing.Size(252, 306);
            this.listView_chips.TabIndex = 0;
            this.listView_chips.UseCompatibleStateImageBehavior = false;
            this.listView_chips.View = System.Windows.Forms.View.Details;
            this.listView_chips.SelectedIndexChanged += new System.EventHandler(this.listView_chips_SelectedIndexChanged);
            this.listView_chips.DoubleClick += new System.EventHandler(this.listView_chips_DoubleClick);
            // 
            // columnHeader_uid
            // 
            this.columnHeader_uid.Text = "UID";
            this.columnHeader_uid.Width = 120;
            // 
            // columnHeader_pii
            // 
            this.columnHeader_pii.Text = "PII";
            this.columnHeader_pii.Width = 300;
            // 
            // label_message
            // 
            this.label_message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_message.Font = new System.Drawing.Font("微软雅黑", 22F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_message.Location = new System.Drawing.Point(0, 0);
            this.label_message.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(672, 306);
            this.label_message.TabIndex = 0;
            this.label_message.Text = "label6";
            this.label_message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.Location = new System.Drawing.Point(783, 323);
            this.button_cancel.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(160, 63);
            this.button_cancel.TabIndex = 4;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Visible = false;
            // 
            // tabPage_operHistory
            // 
            this.tabPage_operHistory.Controls.Add(this.webBrowser1);
            this.tabPage_operHistory.Location = new System.Drawing.Point(4, 33);
            this.tabPage_operHistory.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_operHistory.Name = "tabPage_operHistory";
            this.tabPage_operHistory.Size = new System.Drawing.Size(952, 406);
            this.tabPage_operHistory.TabIndex = 2;
            this.tabPage_operHistory.Text = "操作历史";
            this.tabPage_operHistory.UseVisualStyleBackColor = true;
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.Margin = new System.Windows.Forms.Padding(4);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(24, 27);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(952, 406);
            this.webBrowser1.TabIndex = 1;
            // 
            // tabPage_cfg
            // 
            this.tabPage_cfg.AutoScroll = true;
            this.tabPage_cfg.Controls.Add(this.button_setDefaultThreshold);
            this.tabPage_cfg.Controls.Add(this.textBox_cfg_shreshold);
            this.tabPage_cfg.Controls.Add(this.label7);
            this.tabPage_cfg.Controls.Add(this.comboBox_deviceList);
            this.tabPage_cfg.Controls.Add(this.label6);
            this.tabPage_cfg.Controls.Add(this.textBox_replicationStart);
            this.tabPage_cfg.Controls.Add(this.label5);
            this.tabPage_cfg.Controls.Add(this.checkBox_speak);
            this.tabPage_cfg.Controls.Add(this.checkBox_beep);
            this.tabPage_cfg.Controls.Add(this.checkBox_cfg_savePasswordLong);
            this.tabPage_cfg.Controls.Add(this.textBox_cfg_location);
            this.tabPage_cfg.Controls.Add(this.label4);
            this.tabPage_cfg.Controls.Add(this.textBox_cfg_password);
            this.tabPage_cfg.Controls.Add(this.textBox_cfg_userName);
            this.tabPage_cfg.Controls.Add(this.label3);
            this.tabPage_cfg.Controls.Add(this.label2);
            this.tabPage_cfg.Controls.Add(this.textBox_cfg_dp2LibraryServerUrl);
            this.tabPage_cfg.Controls.Add(this.label1);
            this.tabPage_cfg.Controls.Add(this.toolStrip_server);
            this.tabPage_cfg.Location = new System.Drawing.Point(4, 33);
            this.tabPage_cfg.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_cfg.Name = "tabPage_cfg";
            this.tabPage_cfg.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage_cfg.Size = new System.Drawing.Size(952, 406);
            this.tabPage_cfg.TabIndex = 1;
            this.tabPage_cfg.Text = "配置参数";
            this.tabPage_cfg.UseVisualStyleBackColor = true;
            // 
            // button_setDefaultThreshold
            // 
            this.button_setDefaultThreshold.Location = new System.Drawing.Point(297, 433);
            this.button_setDefaultThreshold.Name = "button_setDefaultThreshold";
            this.button_setDefaultThreshold.Size = new System.Drawing.Size(177, 47);
            this.button_setDefaultThreshold.TabIndex = 14;
            this.button_setDefaultThreshold.Text = "恢复默认值";
            this.button_setDefaultThreshold.UseVisualStyleBackColor = true;
            // 
            // textBox_cfg_shreshold
            // 
            this.textBox_cfg_shreshold.Location = new System.Drawing.Point(191, 441);
            this.textBox_cfg_shreshold.Name = "textBox_cfg_shreshold";
            this.textBox_cfg_shreshold.Size = new System.Drawing.Size(100, 31);
            this.textBox_cfg_shreshold.TabIndex = 13;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 444);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(144, 24);
            this.label7.TabIndex = 12;
            this.label7.Text = "指纹识别阈值(&T):";
            // 
            // comboBox_deviceList
            // 
            this.comboBox_deviceList.FormattingEnabled = true;
            this.comboBox_deviceList.Location = new System.Drawing.Point(191, 392);
            this.comboBox_deviceList.Name = "comboBox_deviceList";
            this.comboBox_deviceList.Size = new System.Drawing.Size(283, 32);
            this.comboBox_deviceList.TabIndex = 11;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 395);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(121, 24);
            this.label6.TabIndex = 10;
            this.label6.Text = "当前读卡器(&I):";
            // 
            // textBox_replicationStart
            // 
            this.textBox_replicationStart.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_replicationStart.Location = new System.Drawing.Point(191, 544);
            this.textBox_replicationStart.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_replicationStart.Name = "textBox_replicationStart";
            this.textBox_replicationStart.Size = new System.Drawing.Size(283, 31);
            this.textBox_replicationStart.TabIndex = 18;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 547);
            this.label5.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(146, 24);
            this.label5.TabIndex = 17;
            this.label5.Text = "日志同步起点(&R):";
            // 
            // checkBox_speak
            // 
            this.checkBox_speak.AutoSize = true;
            this.checkBox_speak.Location = new System.Drawing.Point(135, 491);
            this.checkBox_speak.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.checkBox_speak.Name = "checkBox_speak";
            this.checkBox_speak.Size = new System.Drawing.Size(130, 28);
            this.checkBox_speak.TabIndex = 16;
            this.checkBox_speak.Text = "语音提示(&S)";
            this.checkBox_speak.UseVisualStyleBackColor = true;
            // 
            // checkBox_beep
            // 
            this.checkBox_beep.AutoSize = true;
            this.checkBox_beep.Location = new System.Drawing.Point(15, 491);
            this.checkBox_beep.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.checkBox_beep.Name = "checkBox_beep";
            this.checkBox_beep.Size = new System.Drawing.Size(95, 28);
            this.checkBox_beep.TabIndex = 15;
            this.checkBox_beep.Text = "蜂鸣(&B)";
            this.checkBox_beep.UseVisualStyleBackColor = true;
            // 
            // checkBox_cfg_savePasswordLong
            // 
            this.checkBox_cfg_savePasswordLong.AutoSize = true;
            this.checkBox_cfg_savePasswordLong.Location = new System.Drawing.Point(15, 317);
            this.checkBox_cfg_savePasswordLong.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_cfg_savePasswordLong.Name = "checkBox_cfg_savePasswordLong";
            this.checkBox_cfg_savePasswordLong.Size = new System.Drawing.Size(129, 28);
            this.checkBox_cfg_savePasswordLong.TabIndex = 9;
            this.checkBox_cfg_savePasswordLong.Text = "保存密码(&L)";
            // 
            // textBox_cfg_location
            // 
            this.textBox_cfg_location.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_cfg_location.Location = new System.Drawing.Point(191, 272);
            this.textBox_cfg_location.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_cfg_location.Name = "textBox_cfg_location";
            this.textBox_cfg_location.Size = new System.Drawing.Size(283, 31);
            this.textBox_cfg_location.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 275);
            this.label4.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(130, 24);
            this.label4.TabIndex = 7;
            this.label4.Text = "工作台号(&W)：";
            // 
            // textBox_cfg_password
            // 
            this.textBox_cfg_password.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_cfg_password.Location = new System.Drawing.Point(191, 224);
            this.textBox_cfg_password.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_cfg_password.Name = "textBox_cfg_password";
            this.textBox_cfg_password.PasswordChar = '*';
            this.textBox_cfg_password.Size = new System.Drawing.Size(283, 31);
            this.textBox_cfg_password.TabIndex = 6;
            // 
            // textBox_cfg_userName
            // 
            this.textBox_cfg_userName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_cfg_userName.Location = new System.Drawing.Point(191, 176);
            this.textBox_cfg_userName.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_cfg_userName.Name = "textBox_cfg_userName";
            this.textBox_cfg_userName.Size = new System.Drawing.Size(283, 31);
            this.textBox_cfg_userName.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 227);
            this.label3.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(87, 24);
            this.label3.TabIndex = 5;
            this.label3.Text = "密码(&P)：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 179);
            this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 24);
            this.label2.TabIndex = 3;
            this.label2.Text = "用户名(&U)：";
            // 
            // textBox_cfg_dp2LibraryServerUrl
            // 
            this.textBox_cfg_dp2LibraryServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_cfg_dp2LibraryServerUrl.Location = new System.Drawing.Point(16, 56);
            this.textBox_cfg_dp2LibraryServerUrl.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_cfg_dp2LibraryServerUrl.Name = "textBox_cfg_dp2LibraryServerUrl";
            this.textBox_cfg_dp2LibraryServerUrl.Size = new System.Drawing.Size(713, 31);
            this.textBox_cfg_dp2LibraryServerUrl.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 24);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(207, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "dp2Library 服务器 URL:";
            // 
            // toolStrip_server
            // 
            this.toolStrip_server.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip_server.AutoSize = false;
            this.toolStrip_server.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip_server.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_server.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip_server.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_cfg_setXeServer,
            this.toolStripSeparator1,
            this.toolStripButton_cfg_setHongnibaServer});
            this.toolStrip_server.Location = new System.Drawing.Point(16, 96);
            this.toolStrip_server.Name = "toolStrip_server";
            this.toolStrip_server.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStrip_server.Size = new System.Drawing.Size(716, 51);
            this.toolStrip_server.TabIndex = 2;
            this.toolStrip_server.Text = "toolStrip1";
            // 
            // toolStripButton_cfg_setXeServer
            // 
            this.toolStripButton_cfg_setXeServer.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_cfg_setXeServer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_cfg_setXeServer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_cfg_setXeServer.Name = "toolStripButton_cfg_setXeServer";
            this.toolStripButton_cfg_setXeServer.Size = new System.Drawing.Size(122, 48);
            this.toolStripButton_cfg_setXeServer.Text = "单机版服务器";
            this.toolStripButton_cfg_setXeServer.ToolTipText = "设为单机版服务器";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 51);
            // 
            // toolStripButton_cfg_setHongnibaServer
            // 
            this.toolStripButton_cfg_setHongnibaServer.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_cfg_setHongnibaServer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_cfg_setHongnibaServer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_cfg_setHongnibaServer.Name = "toolStripButton_cfg_setHongnibaServer";
            this.toolStripButton_cfg_setHongnibaServer.Size = new System.Drawing.Size(198, 48);
            this.toolStripButton_cfg_setHongnibaServer.Text = "红泥巴.数字平台服务器";
            this.toolStripButton_cfg_setHongnibaServer.ToolTipText = "设为红泥巴.数字平台服务器";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 508);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 17, 0);
            this.statusStrip1.Size = new System.Drawing.Size(960, 37);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(122, 31);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 32);
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolButton_stop,
            this.toolStripDropDownButton_stopAll,
            this.toolStripSeparator4,
            this.toolStripButton_autoInventory});
            this.toolStrip1.Location = new System.Drawing.Point(0, 34);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(960, 31);
            this.toolStrip1.TabIndex = 5;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolButton_stop
            // 
            this.toolButton_stop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolButton_stop.Enabled = false;
            this.toolButton_stop.Image = ((System.Drawing.Image)(resources.GetObject("toolButton_stop.Image")));
            this.toolButton_stop.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolButton_stop.Name = "toolButton_stop";
            this.toolButton_stop.Size = new System.Drawing.Size(28, 28);
            this.toolButton_stop.Text = "停止";
            // 
            // toolStripDropDownButton_stopAll
            // 
            this.toolStripDropDownButton_stopAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButton_stopAll.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_stopAll});
            this.toolStripDropDownButton_stopAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_stopAll.Name = "toolStripDropDownButton_stopAll";
            this.toolStripDropDownButton_stopAll.Size = new System.Drawing.Size(18, 28);
            this.toolStripDropDownButton_stopAll.Text = "停止全部";
            // 
            // ToolStripMenuItem_stopAll
            // 
            this.ToolStripMenuItem_stopAll.Name = "ToolStripMenuItem_stopAll";
            this.ToolStripMenuItem_stopAll.Size = new System.Drawing.Size(189, 30);
            this.ToolStripMenuItem_stopAll.Text = "停止全部(&A)";
            this.ToolStripMenuItem_stopAll.ToolTipText = "停止全部正在处理的操作";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 31);
            // 
            // toolStripButton_autoInventory
            // 
            this.toolStripButton_autoInventory.CheckOnClick = true;
            this.toolStripButton_autoInventory.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_autoInventory.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_autoInventory.Image")));
            this.toolStripButton_autoInventory.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_autoInventory.Name = "toolStripButton_autoInventory";
            this.toolStripButton_autoInventory.Size = new System.Drawing.Size(86, 28);
            this.toolStripButton_autoInventory.Text = "自动感知";
            this.toolStripButton_autoInventory.CheckStateChanged += new System.EventHandler(this.toolStripButton_autoInventory_CheckStateChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_file,
            this.MenuItem_testing,
            this.MenuItem_help});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 3, 0, 3);
            this.menuStrip1.Size = new System.Drawing.Size(960, 34);
            this.menuStrip1.TabIndex = 4;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // MenuItem_file
            // 
            this.MenuItem_file.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_start,
            this.ToolStripMenuItem_reopen,
            this.MenuItem_startCapture,
            this.MenuItem_stopCapture,
            this.MenuItem_refresh,
            this.MenuItem_clearFingerprintCacheFile,
            this.toolStripSeparator3,
            this.ToolStripMenuItem_exit});
            this.MenuItem_file.Name = "MenuItem_file";
            this.MenuItem_file.Size = new System.Drawing.Size(58, 28);
            this.MenuItem_file.Text = "文件";
            // 
            // ToolStripMenuItem_start
            // 
            this.ToolStripMenuItem_start.Name = "ToolStripMenuItem_start";
            this.ToolStripMenuItem_start.Size = new System.Drawing.Size(252, 30);
            this.ToolStripMenuItem_start.Text = "启动(&S)";
            // 
            // ToolStripMenuItem_reopen
            // 
            this.ToolStripMenuItem_reopen.Name = "ToolStripMenuItem_reopen";
            this.ToolStripMenuItem_reopen.Size = new System.Drawing.Size(252, 30);
            this.ToolStripMenuItem_reopen.Text = "重新启动(&R)";
            // 
            // MenuItem_refresh
            // 
            this.MenuItem_refresh.Name = "MenuItem_refresh";
            this.MenuItem_refresh.Size = new System.Drawing.Size(252, 30);
            this.MenuItem_refresh.Text = "刷新指纹信息";
            // 
            // MenuItem_clearFingerprintCacheFile
            // 
            this.MenuItem_clearFingerprintCacheFile.Name = "MenuItem_clearFingerprintCacheFile";
            this.MenuItem_clearFingerprintCacheFile.Size = new System.Drawing.Size(252, 30);
            this.MenuItem_clearFingerprintCacheFile.Text = "删除本地缓存文件";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(249, 6);
            // 
            // ToolStripMenuItem_exit
            // 
            this.ToolStripMenuItem_exit.Name = "ToolStripMenuItem_exit";
            this.ToolStripMenuItem_exit.Size = new System.Drawing.Size(252, 30);
            this.ToolStripMenuItem_exit.Text = "退出(&X)";
            // 
            // MenuItem_testing
            // 
            this.MenuItem_testing.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_openReader,
            this.MenuItem_closeReader,
            this.MenuItem_inventory,
            this.MenuItem_getTagInfo,
            this.MenuItem_readBlocks,
            this.MenuItem_throwException,
            this.ToolStripMenuItem_testWriteContentToNewChip,
            this.ToolStripMenuItem_testLockBlocks,
            this.ToolStripMenuItem_testRfidChannel});
            this.MenuItem_testing.Name = "MenuItem_testing";
            this.MenuItem_testing.Size = new System.Drawing.Size(58, 28);
            this.MenuItem_testing.Text = "测试";
            // 
            // MenuItem_openReader
            // 
            this.MenuItem_openReader.Name = "MenuItem_openReader";
            this.MenuItem_openReader.Size = new System.Drawing.Size(290, 30);
            this.MenuItem_openReader.Text = "Open Reader";
            this.MenuItem_openReader.Click += new System.EventHandler(this.MenuItem_openReader_Click);
            // 
            // MenuItem_closeReader
            // 
            this.MenuItem_closeReader.Name = "MenuItem_closeReader";
            this.MenuItem_closeReader.Size = new System.Drawing.Size(290, 30);
            this.MenuItem_closeReader.Text = "Close Reader";
            this.MenuItem_closeReader.Click += new System.EventHandler(this.MenuItem_closeReader_Click);
            // 
            // MenuItem_inventory
            // 
            this.MenuItem_inventory.Name = "MenuItem_inventory";
            this.MenuItem_inventory.Size = new System.Drawing.Size(290, 30);
            this.MenuItem_inventory.Text = "Inventory";
            this.MenuItem_inventory.Click += new System.EventHandler(this.MenuItem_inventory_Click);
            // 
            // MenuItem_getTagInfo
            // 
            this.MenuItem_getTagInfo.Name = "MenuItem_getTagInfo";
            this.MenuItem_getTagInfo.Size = new System.Drawing.Size(290, 30);
            this.MenuItem_getTagInfo.Text = "Get Tag Info";
            this.MenuItem_getTagInfo.Click += new System.EventHandler(this.MenuItem_getTagInfo_Click);
            // 
            // MenuItem_readBlocks
            // 
            this.MenuItem_readBlocks.Name = "MenuItem_readBlocks";
            this.MenuItem_readBlocks.Size = new System.Drawing.Size(290, 30);
            this.MenuItem_readBlocks.Text = "Read Blocks";
            this.MenuItem_readBlocks.Click += new System.EventHandler(this.MenuItem_readBlocks_Click);
            // 
            // MenuItem_throwException
            // 
            this.MenuItem_throwException.Name = "MenuItem_throwException";
            this.MenuItem_throwException.Size = new System.Drawing.Size(290, 30);
            this.MenuItem_throwException.Text = "throw exception";
            // 
            // ToolStripMenuItem_testWriteContentToNewChip
            // 
            this.ToolStripMenuItem_testWriteContentToNewChip.Name = "ToolStripMenuItem_testWriteContentToNewChip";
            this.ToolStripMenuItem_testWriteContentToNewChip.Size = new System.Drawing.Size(290, 30);
            this.ToolStripMenuItem_testWriteContentToNewChip.Text = "测试给空白标签写入内容";
            this.ToolStripMenuItem_testWriteContentToNewChip.Click += new System.EventHandler(this.ToolStripMenuItem_testWriteContentToNewChip_Click);
            // 
            // ToolStripMenuItem_testLockBlocks
            // 
            this.ToolStripMenuItem_testLockBlocks.Name = "ToolStripMenuItem_testLockBlocks";
            this.ToolStripMenuItem_testLockBlocks.Size = new System.Drawing.Size(290, 30);
            this.ToolStripMenuItem_testLockBlocks.Text = "测试锁定某些块";
            this.ToolStripMenuItem_testLockBlocks.Click += new System.EventHandler(this.ToolStripMenuItem_testLockBlocks_Click);
            // 
            // ToolStripMenuItem_testRfidChannel
            // 
            this.ToolStripMenuItem_testRfidChannel.Name = "ToolStripMenuItem_testRfidChannel";
            this.ToolStripMenuItem_testRfidChannel.Size = new System.Drawing.Size(290, 30);
            this.ToolStripMenuItem_testRfidChannel.Text = "Test RfidChannel";
            this.ToolStripMenuItem_testRfidChannel.Click += new System.EventHandler(this.ToolStripMenuItem_testRfidChannel_Click);
            // 
            // MenuItem_help
            // 
            this.MenuItem_help.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_setupDriver,
            this.toolStripSeparator2,
            this.MenuItem_manual,
            this.MenuItem_about});
            this.MenuItem_help.Name = "MenuItem_help";
            this.MenuItem_help.Size = new System.Drawing.Size(58, 28);
            this.MenuItem_help.Text = "帮助";
            // 
            // MenuItem_setupDriver
            // 
            this.MenuItem_setupDriver.Name = "MenuItem_setupDriver";
            this.MenuItem_setupDriver.Size = new System.Drawing.Size(353, 30);
            this.MenuItem_setupDriver.Text = "下载安装\'中控\'指纹仪厂家驱动 ...";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(350, 6);
            // 
            // MenuItem_manual
            // 
            this.MenuItem_manual.Name = "MenuItem_manual";
            this.MenuItem_manual.Size = new System.Drawing.Size(353, 30);
            this.MenuItem_manual.Text = "使用帮助 ...";
            // 
            // MenuItem_about
            // 
            this.MenuItem_about.Name = "MenuItem_about";
            this.MenuItem_about.Size = new System.Drawing.Size(353, 30);
            this.MenuItem_about.Text = "关于本软件 ...";
            // 
            // MenuItem_startCapture
            // 
            this.MenuItem_startCapture.Name = "MenuItem_startCapture";
            this.MenuItem_startCapture.Size = new System.Drawing.Size(252, 30);
            this.MenuItem_startCapture.Text = "开始捕获";
            this.MenuItem_startCapture.Click += new System.EventHandler(this.MenuItem_startCapture_Click);
            // 
            // MenuItem_stopCapture
            // 
            this.MenuItem_stopCapture.Name = "MenuItem_stopCapture";
            this.MenuItem_stopCapture.Size = new System.Drawing.Size(252, 30);
            this.MenuItem_stopCapture.Text = "停止捕获";
            this.MenuItem_stopCapture.Click += new System.EventHandler(this.MenuItem_stopCapture_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(960, 545);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "dp2-RFID中心";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_start.ResumeLayout(false);
            this.splitContainer_start.Panel1.ResumeLayout(false);
            this.splitContainer_start.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_start)).EndInit();
            this.splitContainer_start.ResumeLayout(false);
            this.tabPage_operHistory.ResumeLayout(false);
            this.tabPage_cfg.ResumeLayout(false);
            this.tabPage_cfg.PerformLayout();
            this.toolStrip_server.ResumeLayout(false);
            this.toolStrip_server.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_start;
        private System.Windows.Forms.SplitContainer splitContainer_start;
        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.TabPage tabPage_operHistory;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.TabPage tabPage_cfg;
        private System.Windows.Forms.Button button_setDefaultThreshold;
        private System.Windows.Forms.TextBox textBox_cfg_shreshold;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboBox_deviceList;
        private System.Windows.Forms.Label label6;
        public System.Windows.Forms.TextBox textBox_replicationStart;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox checkBox_speak;
        private System.Windows.Forms.CheckBox checkBox_beep;
        public System.Windows.Forms.CheckBox checkBox_cfg_savePasswordLong;
        public System.Windows.Forms.TextBox textBox_cfg_location;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.TextBox textBox_cfg_password;
        public System.Windows.Forms.TextBox textBox_cfg_userName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_cfg_dp2LibraryServerUrl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStrip toolStrip_server;
        private System.Windows.Forms.ToolStripButton toolStripButton_cfg_setXeServer;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_cfg_setHongnibaServer;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolButton_stop;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_stopAll;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_stopAll;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_file;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_start;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_reopen;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_refresh;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_clearFingerprintCacheFile;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_exit;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_testing;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openReader;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_closeReader;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_inventory;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_getTagInfo;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_readBlocks;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_throwException;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_help;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_setupDriver;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_manual;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_about;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_testWriteContentToNewChip;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_testLockBlocks;
        private System.Windows.Forms.ListView listView_chips;
        private System.Windows.Forms.ColumnHeader columnHeader_uid;
        private System.Windows.Forms.ColumnHeader columnHeader_pii;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton toolStripButton_autoInventory;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_testRfidChannel;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_startCapture;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_stopCapture;
    }
}

