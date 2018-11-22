namespace FingerprintCenter
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.MenuItem_file = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_start = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_reopen = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_refresh = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_clearFingerprintCacheFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItem_exit = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_testing = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_lightWhite = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_lightRed = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_lightGreen = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_replication = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_testInitCache = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_throwException = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolButton_stop = new System.Windows.Forms.ToolStripButton();
            this.toolStripDropDownButton_stopAll = new System.Windows.Forms.ToolStripDropDownButton();
            this.ToolStripMenuItem_stopAll = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_start = new System.Windows.Forms.TabPage();
            this.splitContainer_start = new System.Windows.Forms.SplitContainer();
            this.pictureBox_fingerprint = new System.Windows.Forms.PictureBox();
            this.label_message = new System.Windows.Forms.Label();
            this.button_cancel = new System.Windows.Forms.Button();
            this.tabPage_operHistory = new System.Windows.Forms.TabPage();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.tabPage_cfg = new System.Windows.Forms.TabPage();
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
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tabControl_main.SuspendLayout();
            this.tabPage_start.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_start)).BeginInit();
            this.splitContainer_start.Panel1.SuspendLayout();
            this.splitContainer_start.Panel2.SuspendLayout();
            this.splitContainer_start.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_fingerprint)).BeginInit();
            this.tabPage_operHistory.SuspendLayout();
            this.tabPage_cfg.SuspendLayout();
            this.toolStrip_server.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_file,
            this.MenuItem_testing});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(851, 32);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // MenuItem_file
            // 
            this.MenuItem_file.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_start,
            this.ToolStripMenuItem_reopen,
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
            this.ToolStripMenuItem_start.Size = new System.Drawing.Size(236, 30);
            this.ToolStripMenuItem_start.Text = "启动(&S)";
            this.ToolStripMenuItem_start.Click += new System.EventHandler(this.ToolStripMenuItem_start_Click);
            // 
            // ToolStripMenuItem_reopen
            // 
            this.ToolStripMenuItem_reopen.Name = "ToolStripMenuItem_reopen";
            this.ToolStripMenuItem_reopen.Size = new System.Drawing.Size(236, 30);
            this.ToolStripMenuItem_reopen.Text = "重新启动(&R)";
            this.ToolStripMenuItem_reopen.Click += new System.EventHandler(this.ToolStripMenuItem_reopen_Click);
            // 
            // MenuItem_refresh
            // 
            this.MenuItem_refresh.Name = "MenuItem_refresh";
            this.MenuItem_refresh.Size = new System.Drawing.Size(236, 30);
            this.MenuItem_refresh.Text = "刷新指纹信息";
            this.MenuItem_refresh.Click += new System.EventHandler(this.MenuItem_refresh_Click);
            // 
            // MenuItem_clearFingerprintCacheFile
            // 
            this.MenuItem_clearFingerprintCacheFile.Name = "MenuItem_clearFingerprintCacheFile";
            this.MenuItem_clearFingerprintCacheFile.Size = new System.Drawing.Size(236, 30);
            this.MenuItem_clearFingerprintCacheFile.Text = "删除本地缓存文件";
            this.MenuItem_clearFingerprintCacheFile.Click += new System.EventHandler(this.MenuItem_clearFingerprintCacheFile_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(233, 6);
            // 
            // ToolStripMenuItem_exit
            // 
            this.ToolStripMenuItem_exit.Name = "ToolStripMenuItem_exit";
            this.ToolStripMenuItem_exit.Size = new System.Drawing.Size(236, 30);
            this.ToolStripMenuItem_exit.Text = "退出(&X)";
            this.ToolStripMenuItem_exit.Click += new System.EventHandler(this.ToolStripMenuItem_exit_Click);
            // 
            // MenuItem_testing
            // 
            this.MenuItem_testing.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_lightWhite,
            this.MenuItem_lightRed,
            this.MenuItem_lightGreen,
            this.MenuItem_replication,
            this.MenuItem_testInitCache,
            this.MenuItem_throwException});
            this.MenuItem_testing.Name = "MenuItem_testing";
            this.MenuItem_testing.Size = new System.Drawing.Size(58, 28);
            this.MenuItem_testing.Text = "测试";
            // 
            // MenuItem_lightWhite
            // 
            this.MenuItem_lightWhite.Name = "MenuItem_lightWhite";
            this.MenuItem_lightWhite.Size = new System.Drawing.Size(231, 30);
            this.MenuItem_lightWhite.Text = "白灯";
            this.MenuItem_lightWhite.Click += new System.EventHandler(this.MenuItem_lightWhite_Click);
            // 
            // MenuItem_lightRed
            // 
            this.MenuItem_lightRed.Name = "MenuItem_lightRed";
            this.MenuItem_lightRed.Size = new System.Drawing.Size(231, 30);
            this.MenuItem_lightRed.Text = "红灯";
            this.MenuItem_lightRed.Click += new System.EventHandler(this.MenuItem_lightRed_Click);
            // 
            // MenuItem_lightGreen
            // 
            this.MenuItem_lightGreen.Name = "MenuItem_lightGreen";
            this.MenuItem_lightGreen.Size = new System.Drawing.Size(231, 30);
            this.MenuItem_lightGreen.Text = "绿灯";
            this.MenuItem_lightGreen.Click += new System.EventHandler(this.MenuItem_lightGreen_Click);
            // 
            // MenuItem_replication
            // 
            this.MenuItem_replication.Name = "MenuItem_replication";
            this.MenuItem_replication.Size = new System.Drawing.Size(231, 30);
            this.MenuItem_replication.Text = "Replication";
            this.MenuItem_replication.Click += new System.EventHandler(this.MenuItem_replication_Click);
            // 
            // MenuItem_testInitCache
            // 
            this.MenuItem_testInitCache.Name = "MenuItem_testInitCache";
            this.MenuItem_testInitCache.Size = new System.Drawing.Size(231, 30);
            this.MenuItem_testInitCache.Text = "test InitCache";
            this.MenuItem_testInitCache.Click += new System.EventHandler(this.MenuItem_testInitCache_Click);
            // 
            // MenuItem_throwException
            // 
            this.MenuItem_throwException.Name = "MenuItem_throwException";
            this.MenuItem_throwException.Size = new System.Drawing.Size(231, 30);
            this.MenuItem_throwException.Text = "throw exception";
            this.MenuItem_throwException.Click += new System.EventHandler(this.MenuItem_throwException_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolButton_stop,
            this.toolStripDropDownButton_stopAll});
            this.toolStrip1.Location = new System.Drawing.Point(0, 32);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(851, 31);
            this.toolStrip1.TabIndex = 1;
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
            this.toolButton_stop.Click += new System.EventHandler(this.toolButton_stop_Click);
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
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 536);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(851, 29);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 23);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 24);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Controls.Add(this.tabPage_start);
            this.tabControl_main.Controls.Add(this.tabPage_operHistory);
            this.tabControl_main.Controls.Add(this.tabPage_cfg);
            this.tabControl_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_main.Location = new System.Drawing.Point(0, 63);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(851, 473);
            this.tabControl_main.TabIndex = 3;
            // 
            // tabPage_start
            // 
            this.tabPage_start.Controls.Add(this.splitContainer_start);
            this.tabPage_start.Controls.Add(this.button_cancel);
            this.tabPage_start.Location = new System.Drawing.Point(4, 28);
            this.tabPage_start.Name = "tabPage_start";
            this.tabPage_start.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_start.Size = new System.Drawing.Size(843, 441);
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
            this.splitContainer_start.Location = new System.Drawing.Point(6, 6);
            this.splitContainer_start.Name = "splitContainer_start";
            // 
            // splitContainer_start.Panel1
            // 
            this.splitContainer_start.Panel1.Controls.Add(this.pictureBox_fingerprint);
            // 
            // splitContainer_start.Panel2
            // 
            this.splitContainer_start.Panel2.Controls.Add(this.label_message);
            this.splitContainer_start.Size = new System.Drawing.Size(834, 374);
            this.splitContainer_start.SplitterDistance = 225;
            this.splitContainer_start.SplitterWidth = 12;
            this.splitContainer_start.TabIndex = 5;
            // 
            // pictureBox_fingerprint
            // 
            this.pictureBox_fingerprint.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBox_fingerprint.Location = new System.Drawing.Point(0, 0);
            this.pictureBox_fingerprint.Name = "pictureBox_fingerprint";
            this.pictureBox_fingerprint.Size = new System.Drawing.Size(225, 374);
            this.pictureBox_fingerprint.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox_fingerprint.TabIndex = 0;
            this.pictureBox_fingerprint.TabStop = false;
            // 
            // label_message
            // 
            this.label_message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_message.Font = new System.Drawing.Font("微软雅黑", 22F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_message.Location = new System.Drawing.Point(0, 0);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(597, 374);
            this.label_message.TabIndex = 0;
            this.label_message.Text = "label6";
            this.label_message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.Location = new System.Drawing.Point(706, 387);
            this.button_cancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(131, 47);
            this.button_cancel.TabIndex = 4;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Visible = false;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // tabPage_operHistory
            // 
            this.tabPage_operHistory.Controls.Add(this.webBrowser1);
            this.tabPage_operHistory.Location = new System.Drawing.Point(4, 28);
            this.tabPage_operHistory.Name = "tabPage_operHistory";
            this.tabPage_operHistory.Size = new System.Drawing.Size(843, 441);
            this.tabPage_operHistory.TabIndex = 2;
            this.tabPage_operHistory.Text = "操作历史";
            this.tabPage_operHistory.UseVisualStyleBackColor = true;
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(843, 441);
            this.webBrowser1.TabIndex = 1;
            // 
            // tabPage_cfg
            // 
            this.tabPage_cfg.AutoScroll = true;
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
            this.tabPage_cfg.Location = new System.Drawing.Point(4, 28);
            this.tabPage_cfg.Name = "tabPage_cfg";
            this.tabPage_cfg.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_cfg.Size = new System.Drawing.Size(843, 441);
            this.tabPage_cfg.TabIndex = 1;
            this.tabPage_cfg.Text = "配置参数";
            this.tabPage_cfg.UseVisualStyleBackColor = true;
            // 
            // textBox_replicationStart
            // 
            this.textBox_replicationStart.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_replicationStart.Location = new System.Drawing.Point(156, 380);
            this.textBox_replicationStart.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_replicationStart.Name = "textBox_replicationStart";
            this.textBox_replicationStart.Size = new System.Drawing.Size(232, 28);
            this.textBox_replicationStart.TabIndex = 23;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(10, 385);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(112, 27);
            this.label5.TabIndex = 22;
            this.label5.Text = "同步点(&R):";
            // 
            // checkBox_speak
            // 
            this.checkBox_speak.AutoSize = true;
            this.checkBox_speak.Location = new System.Drawing.Point(13, 340);
            this.checkBox_speak.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_speak.Name = "checkBox_speak";
            this.checkBox_speak.Size = new System.Drawing.Size(133, 22);
            this.checkBox_speak.TabIndex = 21;
            this.checkBox_speak.Text = "语音提示(&S)";
            this.checkBox_speak.UseVisualStyleBackColor = true;
            // 
            // checkBox_beep
            // 
            this.checkBox_beep.AutoSize = true;
            this.checkBox_beep.Location = new System.Drawing.Point(13, 315);
            this.checkBox_beep.Margin = new System.Windows.Forms.Padding(2, 3, 2, 3);
            this.checkBox_beep.Name = "checkBox_beep";
            this.checkBox_beep.Size = new System.Drawing.Size(97, 22);
            this.checkBox_beep.TabIndex = 20;
            this.checkBox_beep.Text = "蜂鸣(&B)";
            this.checkBox_beep.UseVisualStyleBackColor = true;
            // 
            // checkBox_cfg_savePasswordLong
            // 
            this.checkBox_cfg_savePasswordLong.Location = new System.Drawing.Point(13, 257);
            this.checkBox_cfg_savePasswordLong.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_cfg_savePasswordLong.Name = "checkBox_cfg_savePasswordLong";
            this.checkBox_cfg_savePasswordLong.Size = new System.Drawing.Size(234, 28);
            this.checkBox_cfg_savePasswordLong.TabIndex = 19;
            this.checkBox_cfg_savePasswordLong.Text = "保存密码(&L)";
            // 
            // textBox_cfg_location
            // 
            this.textBox_cfg_location.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_cfg_location.Location = new System.Drawing.Point(156, 204);
            this.textBox_cfg_location.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_cfg_location.Name = "textBox_cfg_location";
            this.textBox_cfg_location.Size = new System.Drawing.Size(232, 28);
            this.textBox_cfg_location.TabIndex = 17;
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(9, 207);
            this.label4.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(136, 27);
            this.label4.TabIndex = 16;
            this.label4.Text = "工作台号(&W)：";
            // 
            // textBox_cfg_password
            // 
            this.textBox_cfg_password.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_cfg_password.Location = new System.Drawing.Point(156, 168);
            this.textBox_cfg_password.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_cfg_password.Name = "textBox_cfg_password";
            this.textBox_cfg_password.PasswordChar = '*';
            this.textBox_cfg_password.Size = new System.Drawing.Size(232, 28);
            this.textBox_cfg_password.TabIndex = 14;
            this.textBox_cfg_password.TextChanged += new System.EventHandler(this.textBox_cfg_userName_TextChanged);
            // 
            // textBox_cfg_userName
            // 
            this.textBox_cfg_userName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_cfg_userName.Location = new System.Drawing.Point(156, 132);
            this.textBox_cfg_userName.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_cfg_userName.Name = "textBox_cfg_userName";
            this.textBox_cfg_userName.Size = new System.Drawing.Size(232, 28);
            this.textBox_cfg_userName.TabIndex = 11;
            this.textBox_cfg_userName.TextChanged += new System.EventHandler(this.textBox_cfg_userName_TextChanged);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(10, 171);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(112, 27);
            this.label3.TabIndex = 13;
            this.label3.Text = "密码(&P)：";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(10, 137);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 27);
            this.label2.TabIndex = 10;
            this.label2.Text = "用户名(&U)：";
            // 
            // textBox_cfg_dp2LibraryServerUrl
            // 
            this.textBox_cfg_dp2LibraryServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_cfg_dp2LibraryServerUrl.Location = new System.Drawing.Point(13, 42);
            this.textBox_cfg_dp2LibraryServerUrl.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_cfg_dp2LibraryServerUrl.Name = "textBox_cfg_dp2LibraryServerUrl";
            this.textBox_cfg_dp2LibraryServerUrl.Size = new System.Drawing.Size(754, 28);
            this.textBox_cfg_dp2LibraryServerUrl.TabIndex = 8;
            this.textBox_cfg_dp2LibraryServerUrl.TextChanged += new System.EventHandler(this.textBox_cfg_userName_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 18);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(206, 18);
            this.label1.TabIndex = 7;
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
            this.toolStrip_server.Location = new System.Drawing.Point(13, 72);
            this.toolStrip_server.Name = "toolStrip_server";
            this.toolStrip_server.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStrip_server.Size = new System.Drawing.Size(756, 38);
            this.toolStrip_server.TabIndex = 9;
            this.toolStrip_server.Text = "toolStrip1";
            // 
            // toolStripButton_cfg_setXeServer
            // 
            this.toolStripButton_cfg_setXeServer.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_cfg_setXeServer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_cfg_setXeServer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_cfg_setXeServer.Name = "toolStripButton_cfg_setXeServer";
            this.toolStripButton_cfg_setXeServer.Size = new System.Drawing.Size(122, 35);
            this.toolStripButton_cfg_setXeServer.Text = "单机版服务器";
            this.toolStripButton_cfg_setXeServer.ToolTipText = "设为单机版服务器";
            this.toolStripButton_cfg_setXeServer.Click += new System.EventHandler(this.toolStripButton_cfg_setXeServer_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton_cfg_setHongnibaServer
            // 
            this.toolStripButton_cfg_setHongnibaServer.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_cfg_setHongnibaServer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_cfg_setHongnibaServer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_cfg_setHongnibaServer.Name = "toolStripButton_cfg_setHongnibaServer";
            this.toolStripButton_cfg_setHongnibaServer.Size = new System.Drawing.Size(198, 35);
            this.toolStripButton_cfg_setHongnibaServer.Text = "红泥巴.数字平台服务器";
            this.toolStripButton_cfg_setHongnibaServer.ToolTipText = "设为红泥巴.数字平台服务器";
            this.toolStripButton_cfg_setHongnibaServer.Click += new System.EventHandler(this.toolStripButton_cfg_setHongnibaServer_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(851, 565);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.Text = "dp2-指纹中心";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_start.ResumeLayout(false);
            this.splitContainer_start.Panel1.ResumeLayout(false);
            this.splitContainer_start.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_start)).EndInit();
            this.splitContainer_start.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_fingerprint)).EndInit();
            this.tabPage_operHistory.ResumeLayout(false);
            this.tabPage_cfg.ResumeLayout(false);
            this.tabPage_cfg.PerformLayout();
            this.toolStrip_server.ResumeLayout(false);
            this.toolStrip_server.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_start;
        private System.Windows.Forms.TabPage tabPage_cfg;
        private System.Windows.Forms.TextBox textBox_cfg_dp2LibraryServerUrl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStrip toolStrip_server;
        private System.Windows.Forms.ToolStripButton toolStripButton_cfg_setXeServer;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_cfg_setHongnibaServer;
        public System.Windows.Forms.CheckBox checkBox_cfg_savePasswordLong;
        public System.Windows.Forms.TextBox textBox_cfg_location;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.TextBox textBox_cfg_password;
        public System.Windows.Forms.TextBox textBox_cfg_userName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_file;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_clearFingerprintCacheFile;
        private System.Windows.Forms.ToolStripButton toolButton_stop;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_stopAll;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_stopAll;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.CheckBox checkBox_speak;
        private System.Windows.Forms.CheckBox checkBox_beep;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.SplitContainer splitContainer_start;
        private System.Windows.Forms.PictureBox pictureBox_fingerprint;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_exit;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_start;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_reopen;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_testing;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_lightWhite;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_lightRed;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_lightGreen;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_replication;
        public System.Windows.Forms.TextBox textBox_replicationStart;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_testInitCache;
        private System.Windows.Forms.TabPage tabPage_operHistory;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_refresh;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_throwException;
    }
}

