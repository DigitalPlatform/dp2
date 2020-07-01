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
            this.components = new System.ComponentModel.Container();
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_led_cellHeight = new System.Windows.Forms.TextBox();
            this.textBox_led_cellWidth = new System.Windows.Forms.TextBox();
            this.textBox_led_xCount = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_led_serialPort = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_lamp = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_lock = new System.Windows.Forms.ComboBox();
            this.label8 = new System.Windows.Forms.Label();
            this.comboBox_deviceList = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.checkBox_speak = new System.Windows.Forms.CheckBox();
            this.checkBox_beep = new System.Windows.Forms.CheckBox();
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
            this.MenuItem_restart = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openSendKey = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_closeSendKey = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_simuLock = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openLock = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_closeLock = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_getLockState = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_getComPortInfo = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_turnOnLamp = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_turnOffLamp = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_sterilamp = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_sterilamp_turnOn = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_sterilamp_turnOff = new System.Windows.Forms.ToolStripMenuItem();
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
            this.MenuItem_testSetConfig = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_readConfig = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_writePassword = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_help = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_loadFactoryDefault = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_resetReaderToDigitalPlatformState = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_detectReader = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItem_deleteShortcut = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_openUserFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openDataFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openProgramFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_manual = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_about = new System.Windows.Forms.ToolStripMenuItem();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.MenuItem_ledDisplay = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.tabControl_main.SuspendLayout();
            this.tabPage_start.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_start)).BeginInit();
            this.splitContainer_start.Panel1.SuspendLayout();
            this.splitContainer_start.Panel2.SuspendLayout();
            this.splitContainer_start.SuspendLayout();
            this.tabPage_operHistory.SuspendLayout();
            this.tabPage_cfg.SuspendLayout();
            this.groupBox1.SuspendLayout();
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
            this.tabControl_main.Location = new System.Drawing.Point(0, 77);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(960, 427);
            this.tabControl_main.TabIndex = 2;
            // 
            // tabPage_start
            // 
            this.tabPage_start.Controls.Add(this.splitContainer_start);
            this.tabPage_start.Controls.Add(this.button_cancel);
            this.tabPage_start.Location = new System.Drawing.Point(4, 37);
            this.tabPage_start.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_start.Name = "tabPage_start";
            this.tabPage_start.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage_start.Size = new System.Drawing.Size(952, 386);
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
            this.splitContainer_start.Size = new System.Drawing.Size(939, 290);
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
            this.listView_chips.HideSelection = false;
            this.listView_chips.Location = new System.Drawing.Point(0, 0);
            this.listView_chips.Name = "listView_chips";
            this.listView_chips.Size = new System.Drawing.Size(252, 290);
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
            this.label_message.Size = new System.Drawing.Size(672, 290);
            this.label_message.TabIndex = 0;
            this.label_message.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.Location = new System.Drawing.Point(783, 307);
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
            this.tabPage_operHistory.Location = new System.Drawing.Point(4, 37);
            this.tabPage_operHistory.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_operHistory.Name = "tabPage_operHistory";
            this.tabPage_operHistory.Size = new System.Drawing.Size(952, 387);
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
            this.webBrowser1.Size = new System.Drawing.Size(952, 387);
            this.webBrowser1.TabIndex = 1;
            // 
            // tabPage_cfg
            // 
            this.tabPage_cfg.AutoScroll = true;
            this.tabPage_cfg.Controls.Add(this.groupBox1);
            this.tabPage_cfg.Controls.Add(this.comboBox_lamp);
            this.tabPage_cfg.Controls.Add(this.label1);
            this.tabPage_cfg.Controls.Add(this.comboBox_lock);
            this.tabPage_cfg.Controls.Add(this.label8);
            this.tabPage_cfg.Controls.Add(this.comboBox_deviceList);
            this.tabPage_cfg.Controls.Add(this.label6);
            this.tabPage_cfg.Controls.Add(this.checkBox_speak);
            this.tabPage_cfg.Controls.Add(this.checkBox_beep);
            this.tabPage_cfg.Location = new System.Drawing.Point(4, 37);
            this.tabPage_cfg.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_cfg.Name = "tabPage_cfg";
            this.tabPage_cfg.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage_cfg.Size = new System.Drawing.Size(952, 387);
            this.tabPage_cfg.TabIndex = 1;
            this.tabPage_cfg.Text = "配置参数";
            this.tabPage_cfg.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label9);
            this.groupBox1.Controls.Add(this.label7);
            this.groupBox1.Controls.Add(this.textBox_led_cellHeight);
            this.groupBox1.Controls.Add(this.textBox_led_cellWidth);
            this.groupBox1.Controls.Add(this.textBox_led_xCount);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.comboBox_led_serialPort);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(15, 225);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(669, 330);
            this.groupBox1.TabIndex = 12;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "LED 屏";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(345, 192);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(110, 28);
            this.label9.TabIndex = 11;
            this.label9.Text = "常用值: 32";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(345, 150);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(110, 28);
            this.label7.TabIndex = 10;
            this.label7.Text = "常用值: 64";
            // 
            // textBox_led_cellHeight
            // 
            this.textBox_led_cellHeight.Location = new System.Drawing.Point(176, 189);
            this.textBox_led_cellHeight.Name = "textBox_led_cellHeight";
            this.textBox_led_cellHeight.Size = new System.Drawing.Size(163, 35);
            this.textBox_led_cellHeight.TabIndex = 9;
            // 
            // textBox_led_cellWidth
            // 
            this.textBox_led_cellWidth.Location = new System.Drawing.Point(176, 150);
            this.textBox_led_cellWidth.Name = "textBox_led_cellWidth";
            this.textBox_led_cellWidth.Size = new System.Drawing.Size(163, 35);
            this.textBox_led_cellWidth.TabIndex = 8;
            // 
            // textBox_led_xCount
            // 
            this.textBox_led_xCount.Location = new System.Drawing.Point(176, 109);
            this.textBox_led_xCount.Name = "textBox_led_xCount";
            this.textBox_led_xCount.Size = new System.Drawing.Size(163, 35);
            this.textBox_led_xCount.TabIndex = 7;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(28, 192);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(131, 28);
            this.label5.TabIndex = 6;
            this.label5.Text = "单元高度(&H):";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(28, 153);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(136, 28);
            this.label4.TabIndex = 5;
            this.label4.Text = "单元宽度(&W):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(28, 112);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(150, 28);
            this.label3.TabIndex = 4;
            this.label3.Text = "水平单元数(&X):";
            // 
            // comboBox_led_serialPort
            // 
            this.comboBox_led_serialPort.FormattingEnabled = true;
            this.comboBox_led_serialPort.Items.AddRange(new object[] {
            "COM1",
            "COM2",
            "COM3",
            "COM4",
            "COM5",
            "COM6",
            "COM7",
            "COM8",
            "<不使用>"});
            this.comboBox_led_serialPort.Location = new System.Drawing.Point(176, 52);
            this.comboBox_led_serialPort.Name = "comboBox_led_serialPort";
            this.comboBox_led_serialPort.Size = new System.Drawing.Size(283, 36);
            this.comboBox_led_serialPort.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(28, 55);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 28);
            this.label2.TabIndex = 2;
            this.label2.Text = "串口(&P):";
            // 
            // comboBox_lamp
            // 
            this.comboBox_lamp.FormattingEnabled = true;
            this.comboBox_lamp.Items.AddRange(new object[] {
            "COM1",
            "COM2",
            "COM3",
            "COM4",
            "<不使用>"});
            this.comboBox_lamp.Location = new System.Drawing.Point(191, 77);
            this.comboBox_lamp.Name = "comboBox_lamp";
            this.comboBox_lamp.Size = new System.Drawing.Size(283, 36);
            this.comboBox_lamp.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 80);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(67, 28);
            this.label1.TabIndex = 2;
            this.label1.Text = "灯(&A):";
            // 
            // comboBox_lock
            // 
            this.comboBox_lock.FormattingEnabled = true;
            this.comboBox_lock.Items.AddRange(new object[] {
            "COM1",
            "COM2",
            "COM3",
            "COM4",
            "<自动>",
            "<不使用>"});
            this.comboBox_lock.Location = new System.Drawing.Point(191, 39);
            this.comboBox_lock.Name = "comboBox_lock";
            this.comboBox_lock.Size = new System.Drawing.Size(283, 36);
            this.comboBox_lock.TabIndex = 1;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(11, 42);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(84, 28);
            this.label8.TabIndex = 0;
            this.label8.Text = "门锁(&L):";
            // 
            // comboBox_deviceList
            // 
            this.comboBox_deviceList.FormattingEnabled = true;
            this.comboBox_deviceList.Location = new System.Drawing.Point(191, 392);
            this.comboBox_deviceList.Name = "comboBox_deviceList";
            this.comboBox_deviceList.Size = new System.Drawing.Size(283, 36);
            this.comboBox_deviceList.TabIndex = 11;
            this.comboBox_deviceList.Visible = false;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(11, 395);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(142, 28);
            this.label6.TabIndex = 10;
            this.label6.Text = "当前读卡器(&I):";
            this.label6.Visible = false;
            // 
            // checkBox_speak
            // 
            this.checkBox_speak.AutoSize = true;
            this.checkBox_speak.Checked = true;
            this.checkBox_speak.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_speak.Location = new System.Drawing.Point(140, 162);
            this.checkBox_speak.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.checkBox_speak.Name = "checkBox_speak";
            this.checkBox_speak.Size = new System.Drawing.Size(148, 32);
            this.checkBox_speak.TabIndex = 5;
            this.checkBox_speak.Text = "语音提示(&S)";
            this.checkBox_speak.UseVisualStyleBackColor = true;
            // 
            // checkBox_beep
            // 
            this.checkBox_beep.AutoSize = true;
            this.checkBox_beep.Location = new System.Drawing.Point(15, 162);
            this.checkBox_beep.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.checkBox_beep.Name = "checkBox_beep";
            this.checkBox_beep.Size = new System.Drawing.Size(107, 32);
            this.checkBox_beep.TabIndex = 4;
            this.checkBox_beep.Text = "蜂鸣(&B)";
            this.checkBox_beep.UseVisualStyleBackColor = true;
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 504);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 17, 0);
            this.statusStrip1.Size = new System.Drawing.Size(960, 41);
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
            this.toolStrip1.Location = new System.Drawing.Point(0, 39);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(960, 38);
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
            this.toolButton_stop.Size = new System.Drawing.Size(40, 32);
            this.toolButton_stop.Text = "停止";
            // 
            // toolStripDropDownButton_stopAll
            // 
            this.toolStripDropDownButton_stopAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButton_stopAll.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_stopAll});
            this.toolStripDropDownButton_stopAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_stopAll.Name = "toolStripDropDownButton_stopAll";
            this.toolStripDropDownButton_stopAll.Size = new System.Drawing.Size(21, 32);
            this.toolStripDropDownButton_stopAll.Text = "停止全部";
            // 
            // ToolStripMenuItem_stopAll
            // 
            this.ToolStripMenuItem_stopAll.Name = "ToolStripMenuItem_stopAll";
            this.ToolStripMenuItem_stopAll.Size = new System.Drawing.Size(242, 40);
            this.ToolStripMenuItem_stopAll.Text = "停止全部(&A)";
            this.ToolStripMenuItem_stopAll.ToolTipText = "停止全部正在处理的操作";
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton_autoInventory
            // 
            this.toolStripButton_autoInventory.CheckOnClick = true;
            this.toolStripButton_autoInventory.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_autoInventory.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_autoInventory.Image")));
            this.toolStripButton_autoInventory.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_autoInventory.Name = "toolStripButton_autoInventory";
            this.toolStripButton_autoInventory.Size = new System.Drawing.Size(100, 32);
            this.toolStripButton_autoInventory.Text = "自动感知";
            this.toolStripButton_autoInventory.CheckStateChanged += new System.EventHandler(this.toolStripButton_autoInventory_CheckStateChanged);
            // 
            // menuStrip1
            // 
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_file,
            this.MenuItem_testing,
            this.MenuItem_help});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 3, 0, 3);
            this.menuStrip1.Size = new System.Drawing.Size(960, 39);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // MenuItem_file
            // 
            this.MenuItem_file.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_restart,
            this.MenuItem_openSendKey,
            this.MenuItem_closeSendKey,
            this.toolStripSeparator1,
            this.MenuItem_simuLock,
            this.MenuItem_openLock,
            this.MenuItem_closeLock,
            this.MenuItem_getLockState,
            this.MenuItem_getComPortInfo,
            this.toolStripSeparator7,
            this.MenuItem_turnOnLamp,
            this.MenuItem_turnOffLamp,
            this.toolStripMenuItem_sterilamp,
            this.toolStripSeparator3,
            this.MenuItem_ledDisplay,
            this.toolStripSeparator8,
            this.ToolStripMenuItem_exit});
            this.MenuItem_file.Name = "MenuItem_file";
            this.MenuItem_file.Size = new System.Drawing.Size(72, 33);
            this.MenuItem_file.Text = "文件";
            // 
            // MenuItem_restart
            // 
            this.MenuItem_restart.Name = "MenuItem_restart";
            this.MenuItem_restart.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_restart.Text = "重新启动";
            this.MenuItem_restart.Click += new System.EventHandler(this.MenuItem_restart_Click);
            // 
            // MenuItem_openSendKey
            // 
            this.MenuItem_openSendKey.Name = "MenuItem_openSendKey";
            this.MenuItem_openSendKey.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_openSendKey.Text = "打开 发送";
            this.MenuItem_openSendKey.Click += new System.EventHandler(this.MenuItem_openSendKey_Click);
            // 
            // MenuItem_closeSendKey
            // 
            this.MenuItem_closeSendKey.Name = "MenuItem_closeSendKey";
            this.MenuItem_closeSendKey.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_closeSendKey.Text = "关闭 发送";
            this.MenuItem_closeSendKey.Click += new System.EventHandler(this.MenuItem_closeSendKey_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(312, 6);
            // 
            // MenuItem_simuLock
            // 
            this.MenuItem_simuLock.Name = "MenuItem_simuLock";
            this.MenuItem_simuLock.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_simuLock.Text = "模拟锁";
            this.MenuItem_simuLock.Click += new System.EventHandler(this.MenuItem_simuLock_Click);
            // 
            // MenuItem_openLock
            // 
            this.MenuItem_openLock.Name = "MenuItem_openLock";
            this.MenuItem_openLock.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_openLock.Text = "开锁";
            this.MenuItem_openLock.Click += new System.EventHandler(this.MenuItem_openLock_Click);
            // 
            // MenuItem_closeLock
            // 
            this.MenuItem_closeLock.Name = "MenuItem_closeLock";
            this.MenuItem_closeLock.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_closeLock.Text = "关门";
            this.MenuItem_closeLock.Click += new System.EventHandler(this.MenuItem_closeLock_Click);
            // 
            // MenuItem_getLockState
            // 
            this.MenuItem_getLockState.Name = "MenuItem_getLockState";
            this.MenuItem_getLockState.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_getLockState.Text = "探测锁状态";
            this.MenuItem_getLockState.Click += new System.EventHandler(this.MenuItem_getLockState_Click);
            // 
            // MenuItem_getComPortInfo
            // 
            this.MenuItem_getComPortInfo.Name = "MenuItem_getComPortInfo";
            this.MenuItem_getComPortInfo.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_getComPortInfo.Text = "探测 COM 口信息";
            this.MenuItem_getComPortInfo.Click += new System.EventHandler(this.MenuItem_getComPortInfo_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(312, 6);
            // 
            // MenuItem_turnOnLamp
            // 
            this.MenuItem_turnOnLamp.Name = "MenuItem_turnOnLamp";
            this.MenuItem_turnOnLamp.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_turnOnLamp.Text = "开灯";
            this.MenuItem_turnOnLamp.Click += new System.EventHandler(this.MenuItem_turnOnLamp_Click);
            // 
            // MenuItem_turnOffLamp
            // 
            this.MenuItem_turnOffLamp.Name = "MenuItem_turnOffLamp";
            this.MenuItem_turnOffLamp.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_turnOffLamp.Text = "关灯";
            this.MenuItem_turnOffLamp.Click += new System.EventHandler(this.MenuItem_turnOffLamp_Click);
            // 
            // toolStripMenuItem_sterilamp
            // 
            this.toolStripMenuItem_sterilamp.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_sterilamp_turnOn,
            this.ToolStripMenuItem_sterilamp_turnOff});
            this.toolStripMenuItem_sterilamp.Name = "toolStripMenuItem_sterilamp";
            this.toolStripMenuItem_sterilamp.Size = new System.Drawing.Size(315, 40);
            this.toolStripMenuItem_sterilamp.Text = "紫外灯";
            // 
            // ToolStripMenuItem_sterilamp_turnOn
            // 
            this.ToolStripMenuItem_sterilamp_turnOn.Name = "ToolStripMenuItem_sterilamp_turnOn";
            this.ToolStripMenuItem_sterilamp_turnOn.Size = new System.Drawing.Size(315, 40);
            this.ToolStripMenuItem_sterilamp_turnOn.Text = "开";
            this.ToolStripMenuItem_sterilamp_turnOn.Click += new System.EventHandler(this.ToolStripMenuItem_sterilamp_turnOn_Click);
            // 
            // ToolStripMenuItem_sterilamp_turnOff
            // 
            this.ToolStripMenuItem_sterilamp_turnOff.Name = "ToolStripMenuItem_sterilamp_turnOff";
            this.ToolStripMenuItem_sterilamp_turnOff.Size = new System.Drawing.Size(315, 40);
            this.ToolStripMenuItem_sterilamp_turnOff.Text = "关";
            this.ToolStripMenuItem_sterilamp_turnOff.Click += new System.EventHandler(this.ToolStripMenuItem_sterilamp_turnOff_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(312, 6);
            // 
            // ToolStripMenuItem_exit
            // 
            this.ToolStripMenuItem_exit.Name = "ToolStripMenuItem_exit";
            this.ToolStripMenuItem_exit.Size = new System.Drawing.Size(315, 40);
            this.ToolStripMenuItem_exit.Text = "退出(&X)";
            this.ToolStripMenuItem_exit.Click += new System.EventHandler(this.ToolStripMenuItem_exit_Click);
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
            this.ToolStripMenuItem_testRfidChannel,
            this.MenuItem_testSetConfig,
            this.MenuItem_readConfig,
            this.MenuItem_writePassword});
            this.MenuItem_testing.Name = "MenuItem_testing";
            this.MenuItem_testing.Size = new System.Drawing.Size(72, 33);
            this.MenuItem_testing.Text = "测试";
            // 
            // MenuItem_openReader
            // 
            this.MenuItem_openReader.Name = "MenuItem_openReader";
            this.MenuItem_openReader.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_openReader.Text = "Open Reader";
            this.MenuItem_openReader.Click += new System.EventHandler(this.MenuItem_openReader_Click);
            // 
            // MenuItem_closeReader
            // 
            this.MenuItem_closeReader.Name = "MenuItem_closeReader";
            this.MenuItem_closeReader.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_closeReader.Text = "Close Reader";
            this.MenuItem_closeReader.Click += new System.EventHandler(this.MenuItem_closeReader_Click);
            // 
            // MenuItem_inventory
            // 
            this.MenuItem_inventory.Name = "MenuItem_inventory";
            this.MenuItem_inventory.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_inventory.Text = "Inventory";
            this.MenuItem_inventory.Click += new System.EventHandler(this.MenuItem_inventory_Click);
            // 
            // MenuItem_getTagInfo
            // 
            this.MenuItem_getTagInfo.Name = "MenuItem_getTagInfo";
            this.MenuItem_getTagInfo.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_getTagInfo.Text = "Get Tag Info";
            this.MenuItem_getTagInfo.Click += new System.EventHandler(this.MenuItem_getTagInfo_Click);
            // 
            // MenuItem_readBlocks
            // 
            this.MenuItem_readBlocks.Name = "MenuItem_readBlocks";
            this.MenuItem_readBlocks.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_readBlocks.Text = "Read Blocks";
            this.MenuItem_readBlocks.Click += new System.EventHandler(this.MenuItem_readBlocks_Click);
            // 
            // MenuItem_throwException
            // 
            this.MenuItem_throwException.Name = "MenuItem_throwException";
            this.MenuItem_throwException.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_throwException.Text = "throw exception";
            // 
            // ToolStripMenuItem_testWriteContentToNewChip
            // 
            this.ToolStripMenuItem_testWriteContentToNewChip.Name = "ToolStripMenuItem_testWriteContentToNewChip";
            this.ToolStripMenuItem_testWriteContentToNewChip.Size = new System.Drawing.Size(360, 40);
            this.ToolStripMenuItem_testWriteContentToNewChip.Text = "测试给空白标签写入内容";
            this.ToolStripMenuItem_testWriteContentToNewChip.Click += new System.EventHandler(this.ToolStripMenuItem_testWriteContentToNewChip_Click);
            // 
            // ToolStripMenuItem_testLockBlocks
            // 
            this.ToolStripMenuItem_testLockBlocks.Name = "ToolStripMenuItem_testLockBlocks";
            this.ToolStripMenuItem_testLockBlocks.Size = new System.Drawing.Size(360, 40);
            this.ToolStripMenuItem_testLockBlocks.Text = "测试锁定某些块";
            this.ToolStripMenuItem_testLockBlocks.Click += new System.EventHandler(this.ToolStripMenuItem_testLockBlocks_Click);
            // 
            // ToolStripMenuItem_testRfidChannel
            // 
            this.ToolStripMenuItem_testRfidChannel.Name = "ToolStripMenuItem_testRfidChannel";
            this.ToolStripMenuItem_testRfidChannel.Size = new System.Drawing.Size(360, 40);
            this.ToolStripMenuItem_testRfidChannel.Text = "Test RfidChannel";
            this.ToolStripMenuItem_testRfidChannel.Click += new System.EventHandler(this.ToolStripMenuItem_testRfidChannel_Click);
            // 
            // MenuItem_testSetConfig
            // 
            this.MenuItem_testSetConfig.Name = "MenuItem_testSetConfig";
            this.MenuItem_testSetConfig.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_testSetConfig.Text = "SetConfig";
            this.MenuItem_testSetConfig.Click += new System.EventHandler(this.MenuItem_testSetConfig_Click);
            // 
            // MenuItem_readConfig
            // 
            this.MenuItem_readConfig.Name = "MenuItem_readConfig";
            this.MenuItem_readConfig.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_readConfig.Text = "ReadConfig";
            this.MenuItem_readConfig.Click += new System.EventHandler(this.MenuItem_readConfig_Click);
            // 
            // MenuItem_writePassword
            // 
            this.MenuItem_writePassword.Name = "MenuItem_writePassword";
            this.MenuItem_writePassword.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_writePassword.Text = "WritePassword";
            this.MenuItem_writePassword.Click += new System.EventHandler(this.MenuItem_writePassword_Click);
            // 
            // MenuItem_help
            // 
            this.MenuItem_help.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_loadFactoryDefault,
            this.MenuItem_resetReaderToDigitalPlatformState,
            this.MenuItem_detectReader,
            this.toolStripSeparator5,
            this.ToolStripMenuItem_deleteShortcut,
            this.toolStripSeparator2,
            this.MenuItem_openUserFolder,
            this.MenuItem_openDataFolder,
            this.MenuItem_openProgramFolder,
            this.toolStripSeparator6,
            this.MenuItem_manual,
            this.MenuItem_about});
            this.MenuItem_help.Name = "MenuItem_help";
            this.MenuItem_help.Size = new System.Drawing.Size(72, 33);
            this.MenuItem_help.Text = "帮助";
            // 
            // MenuItem_loadFactoryDefault
            // 
            this.MenuItem_loadFactoryDefault.Name = "MenuItem_loadFactoryDefault";
            this.MenuItem_loadFactoryDefault.Size = new System.Drawing.Size(382, 40);
            this.MenuItem_loadFactoryDefault.Text = "恢复读卡器出厂设置";
            this.MenuItem_loadFactoryDefault.Visible = false;
            this.MenuItem_loadFactoryDefault.Click += new System.EventHandler(this.MenuItem_loadFactoryDefault_Click);
            // 
            // MenuItem_resetReaderToDigitalPlatformState
            // 
            this.MenuItem_resetReaderToDigitalPlatformState.Name = "MenuItem_resetReaderToDigitalPlatformState";
            this.MenuItem_resetReaderToDigitalPlatformState.Size = new System.Drawing.Size(382, 40);
            this.MenuItem_resetReaderToDigitalPlatformState.Text = "恢复读卡器初始设置";
            this.MenuItem_resetReaderToDigitalPlatformState.Click += new System.EventHandler(this.MenuItem_resetReaderToDigitalPlatformState_Click);
            // 
            // MenuItem_detectReader
            // 
            this.MenuItem_detectReader.Name = "MenuItem_detectReader";
            this.MenuItem_detectReader.Size = new System.Drawing.Size(382, 40);
            this.MenuItem_detectReader.Text = "自动探测 COM 口读卡器 ...";
            this.MenuItem_detectReader.Click += new System.EventHandler(this.MenuItem_detectReader_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(379, 6);
            // 
            // ToolStripMenuItem_deleteShortcut
            // 
            this.ToolStripMenuItem_deleteShortcut.Name = "ToolStripMenuItem_deleteShortcut";
            this.ToolStripMenuItem_deleteShortcut.Size = new System.Drawing.Size(382, 40);
            this.ToolStripMenuItem_deleteShortcut.Text = "删除开机启动快捷方式";
            this.ToolStripMenuItem_deleteShortcut.Click += new System.EventHandler(this.ToolStripMenuItem_deleteShortcut_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(379, 6);
            // 
            // MenuItem_openUserFolder
            // 
            this.MenuItem_openUserFolder.Name = "MenuItem_openUserFolder";
            this.MenuItem_openUserFolder.Size = new System.Drawing.Size(382, 40);
            this.MenuItem_openUserFolder.Text = "打开用户文件夹(&U)";
            this.MenuItem_openUserFolder.Click += new System.EventHandler(this.MenuItem_openUserFolder_Click);
            // 
            // MenuItem_openDataFolder
            // 
            this.MenuItem_openDataFolder.Name = "MenuItem_openDataFolder";
            this.MenuItem_openDataFolder.Size = new System.Drawing.Size(382, 40);
            this.MenuItem_openDataFolder.Text = "打开数据文件夹(&D)";
            this.MenuItem_openDataFolder.Click += new System.EventHandler(this.MenuItem_openDataFolder_Click);
            // 
            // MenuItem_openProgramFolder
            // 
            this.MenuItem_openProgramFolder.Name = "MenuItem_openProgramFolder";
            this.MenuItem_openProgramFolder.Size = new System.Drawing.Size(382, 40);
            this.MenuItem_openProgramFolder.Text = "打开程序文件夹(&P)";
            this.MenuItem_openProgramFolder.Click += new System.EventHandler(this.MenuItem_openProgramFolder_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(379, 6);
            // 
            // MenuItem_manual
            // 
            this.MenuItem_manual.Name = "MenuItem_manual";
            this.MenuItem_manual.Size = new System.Drawing.Size(382, 40);
            this.MenuItem_manual.Text = "使用帮助 ...";
            // 
            // MenuItem_about
            // 
            this.MenuItem_about.Name = "MenuItem_about";
            this.MenuItem_about.Size = new System.Drawing.Size(382, 40);
            this.MenuItem_about.Text = "关于本软件 ...";
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.BalloonTipText = "dp2-RFID中心";
            this.notifyIcon1.BalloonTipTitle = "dp2-RFID中心";
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "dp2-RFID中心";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
            // 
            // MenuItem_ledDisplay
            // 
            this.MenuItem_ledDisplay.Name = "MenuItem_ledDisplay";
            this.MenuItem_ledDisplay.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_ledDisplay.Text = "LED 显示文字";
            this.MenuItem_ledDisplay.Click += new System.EventHandler(this.MenuItem_ledDisplay_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(312, 6);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 28F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(960, 545);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "dp2-RFID中心";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_start.ResumeLayout(false);
            this.splitContainer_start.Panel1.ResumeLayout(false);
            this.splitContainer_start.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_start)).EndInit();
            this.splitContainer_start.ResumeLayout(false);
            this.tabPage_operHistory.ResumeLayout(false);
            this.tabPage_cfg.ResumeLayout(false);
            this.tabPage_cfg.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
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
        private System.Windows.Forms.ComboBox comboBox_deviceList;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox checkBox_speak;
        private System.Windows.Forms.CheckBox checkBox_beep;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolButton_stop;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_stopAll;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_stopAll;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_file;
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
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openSendKey;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_closeSendKey;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_loadFactoryDefault;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_testSetConfig;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_resetReaderToDigitalPlatformState;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_readConfig;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_writePassword;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openUserFolder;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openDataFolder;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openProgramFolder;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_restart;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_detectReader;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_deleteShortcut;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ComboBox comboBox_lock;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openLock;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_getLockState;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_getComPortInfo;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_simuLock;
        private System.Windows.Forms.ComboBox comboBox_lamp;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_turnOnLamp;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_turnOffLamp;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_closeLock;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_sterilamp;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_sterilamp_turnOn;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_sterilamp_turnOff;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ComboBox comboBox_led_serialPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_led_cellHeight;
        private System.Windows.Forms.TextBox textBox_led_cellWidth;
        private System.Windows.Forms.TextBox textBox_led_xCount;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_ledDisplay;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
    }
}

