
namespace dp2Inventory
{
    partial class SettingDialog
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_rfid = new System.Windows.Forms.TabPage();
            this.textBox_rfid_uploadInterfaceUrl = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.checkBox_rfid_rpanTypeSwitch = new System.Windows.Forms.CheckBox();
            this.numericUpDown_seconds = new System.Windows.Forms.NumericUpDown();
            this.label10 = new System.Windows.Forms.Label();
            this.groupBox11 = new System.Windows.Forms.GroupBox();
            this.button_rfid_setRfidUrlDefaultValue = new System.Windows.Forms.Button();
            this.textBox_rfid_rfidCenterUrl = new System.Windows.Forms.TextBox();
            this.tabPage_dp2library = new System.Windows.Forms.TabPage();
            this.textBox_dp2library_location = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_dp2library_password = new System.Windows.Forms.TextBox();
            this.textBox_dp2library_userName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_dp2library_serverUrl = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.toolStrip_server = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_cfg_setXeServer = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_cfg_setHongnibaServer = new System.Windows.Forms.ToolStripButton();
            this.tabPage_sip2 = new System.Windows.Forms.TabPage();
            this.checkBox_sip_localStore = new System.Windows.Forms.CheckBox();
            this.textBox_sip_locationList = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.textBox_sip_institution = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.textBox_sip_port = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.textBox_sip_encoding = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_sip_password = new System.Windows.Forms.TextBox();
            this.textBox_sip_userName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_sip_serverAddr = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.tabPage_other = new System.Windows.Forms.TabPage();
            this.checkBox_enableTagCache = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox_verifyRule = new System.Windows.Forms.TextBox();
            this.tabControl1.SuspendLayout();
            this.tabPage_rfid.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_seconds)).BeginInit();
            this.groupBox11.SuspendLayout();
            this.tabPage_dp2library.SuspendLayout();
            this.toolStrip_server.SuspendLayout();
            this.tabPage_sip2.SuspendLayout();
            this.tabPage_other.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(698, 646);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(111, 40);
            this.button_Cancel.TabIndex = 5;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(581, 646);
            this.button_OK.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(111, 40);
            this.button_OK.TabIndex = 4;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage_rfid);
            this.tabControl1.Controls.Add(this.tabPage_dp2library);
            this.tabControl1.Controls.Add(this.tabPage_sip2);
            this.tabControl1.Controls.Add(this.tabPage_other);
            this.tabControl1.Location = new System.Drawing.Point(12, 11);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(797, 631);
            this.tabControl1.TabIndex = 3;
            // 
            // tabPage_rfid
            // 
            this.tabPage_rfid.AutoScroll = true;
            this.tabPage_rfid.Controls.Add(this.textBox_rfid_uploadInterfaceUrl);
            this.tabPage_rfid.Controls.Add(this.label13);
            this.tabPage_rfid.Controls.Add(this.checkBox_rfid_rpanTypeSwitch);
            this.tabPage_rfid.Controls.Add(this.numericUpDown_seconds);
            this.tabPage_rfid.Controls.Add(this.label10);
            this.tabPage_rfid.Controls.Add(this.groupBox11);
            this.tabPage_rfid.Location = new System.Drawing.Point(4, 31);
            this.tabPage_rfid.Name = "tabPage_rfid";
            this.tabPage_rfid.Size = new System.Drawing.Size(789, 596);
            this.tabPage_rfid.TabIndex = 0;
            this.tabPage_rfid.Text = "RFID";
            this.tabPage_rfid.UseVisualStyleBackColor = true;
            // 
            // textBox_rfid_uploadInterfaceUrl
            // 
            this.textBox_rfid_uploadInterfaceUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_rfid_uploadInterfaceUrl.Location = new System.Drawing.Point(28, 389);
            this.textBox_rfid_uploadInterfaceUrl.Name = "textBox_rfid_uploadInterfaceUrl";
            this.textBox_rfid_uploadInterfaceUrl.Size = new System.Drawing.Size(723, 31);
            this.textBox_rfid_uploadInterfaceUrl.TabIndex = 7;
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(24, 365);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(182, 21);
            this.label13.TabIndex = 6;
            this.label13.Text = "上传接口 URL(&U):";
            // 
            // checkBox_rfid_rpanTypeSwitch
            // 
            this.checkBox_rfid_rpanTypeSwitch.AutoSize = true;
            this.checkBox_rfid_rpanTypeSwitch.Location = new System.Drawing.Point(28, 297);
            this.checkBox_rfid_rpanTypeSwitch.Name = "checkBox_rfid_rpanTypeSwitch";
            this.checkBox_rfid_rpanTypeSwitch.Size = new System.Drawing.Size(314, 25);
            this.checkBox_rfid_rpanTypeSwitch.TabIndex = 5;
            this.checkBox_rfid_rpanTypeSwitch.Text = "启用 R-PAN 标签类型切换(&R)";
            this.checkBox_rfid_rpanTypeSwitch.UseVisualStyleBackColor = true;
            // 
            // numericUpDown_seconds
            // 
            this.numericUpDown_seconds.Location = new System.Drawing.Point(267, 226);
            this.numericUpDown_seconds.Maximum = new decimal(new int[] {
            10,
            0,
            0,
            0});
            this.numericUpDown_seconds.Name = "numericUpDown_seconds";
            this.numericUpDown_seconds.Size = new System.Drawing.Size(120, 31);
            this.numericUpDown_seconds.TabIndex = 4;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(24, 228);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(222, 21);
            this.label10.TabIndex = 3;
            this.label10.Text = "扫描前倒计时秒数(&S):";
            // 
            // groupBox11
            // 
            this.groupBox11.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox11.Controls.Add(this.button_rfid_setRfidUrlDefaultValue);
            this.groupBox11.Controls.Add(this.textBox_rfid_rfidCenterUrl);
            this.groupBox11.Location = new System.Drawing.Point(17, 19);
            this.groupBox11.Margin = new System.Windows.Forms.Padding(5);
            this.groupBox11.Name = "groupBox11";
            this.groupBox11.Padding = new System.Windows.Forms.Padding(5);
            this.groupBox11.Size = new System.Drawing.Size(748, 163);
            this.groupBox11.TabIndex = 2;
            this.groupBox11.TabStop = false;
            this.groupBox11.Text = " RFID 读卡器接口 URL ";
            // 
            // button_rfid_setRfidUrlDefaultValue
            // 
            this.button_rfid_setRfidUrlDefaultValue.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_rfid_setRfidUrlDefaultValue.Location = new System.Drawing.Point(544, 82);
            this.button_rfid_setRfidUrlDefaultValue.Margin = new System.Windows.Forms.Padding(5);
            this.button_rfid_setRfidUrlDefaultValue.Name = "button_rfid_setRfidUrlDefaultValue";
            this.button_rfid_setRfidUrlDefaultValue.Size = new System.Drawing.Size(193, 40);
            this.button_rfid_setRfidUrlDefaultValue.TabIndex = 1;
            this.button_rfid_setRfidUrlDefaultValue.Text = "设为常用值";
            this.button_rfid_setRfidUrlDefaultValue.UseVisualStyleBackColor = true;
            this.button_rfid_setRfidUrlDefaultValue.Click += new System.EventHandler(this.button_rfid_setRfidUrlDefaultValue_Click);
            // 
            // textBox_rfid_rfidCenterUrl
            // 
            this.textBox_rfid_rfidCenterUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_rfid_rfidCenterUrl.Location = new System.Drawing.Point(11, 35);
            this.textBox_rfid_rfidCenterUrl.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_rfid_rfidCenterUrl.Name = "textBox_rfid_rfidCenterUrl";
            this.textBox_rfid_rfidCenterUrl.Size = new System.Drawing.Size(723, 31);
            this.textBox_rfid_rfidCenterUrl.TabIndex = 0;
            // 
            // tabPage_dp2library
            // 
            this.tabPage_dp2library.AutoScroll = true;
            this.tabPage_dp2library.Controls.Add(this.textBox_dp2library_location);
            this.tabPage_dp2library.Controls.Add(this.label4);
            this.tabPage_dp2library.Controls.Add(this.textBox_dp2library_password);
            this.tabPage_dp2library.Controls.Add(this.textBox_dp2library_userName);
            this.tabPage_dp2library.Controls.Add(this.label3);
            this.tabPage_dp2library.Controls.Add(this.label2);
            this.tabPage_dp2library.Controls.Add(this.textBox_dp2library_serverUrl);
            this.tabPage_dp2library.Controls.Add(this.label1);
            this.tabPage_dp2library.Controls.Add(this.toolStrip_server);
            this.tabPage_dp2library.Location = new System.Drawing.Point(4, 31);
            this.tabPage_dp2library.Name = "tabPage_dp2library";
            this.tabPage_dp2library.Size = new System.Drawing.Size(789, 596);
            this.tabPage_dp2library.TabIndex = 1;
            this.tabPage_dp2library.Text = "dp2library";
            this.tabPage_dp2library.UseVisualStyleBackColor = true;
            // 
            // textBox_dp2library_location
            // 
            this.textBox_dp2library_location.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_dp2library_location.Location = new System.Drawing.Point(191, 268);
            this.textBox_dp2library_location.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_dp2library_location.Name = "textBox_dp2library_location";
            this.textBox_dp2library_location.Size = new System.Drawing.Size(266, 31);
            this.textBox_dp2library_location.TabIndex = 18;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 271);
            this.label4.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(148, 21);
            this.label4.TabIndex = 17;
            this.label4.Text = "工作台号(&W)：";
            // 
            // textBox_dp2library_password
            // 
            this.textBox_dp2library_password.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_dp2library_password.Location = new System.Drawing.Point(191, 220);
            this.textBox_dp2library_password.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_dp2library_password.Name = "textBox_dp2library_password";
            this.textBox_dp2library_password.PasswordChar = '*';
            this.textBox_dp2library_password.Size = new System.Drawing.Size(266, 31);
            this.textBox_dp2library_password.TabIndex = 16;
            // 
            // textBox_dp2library_userName
            // 
            this.textBox_dp2library_userName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_dp2library_userName.Location = new System.Drawing.Point(191, 172);
            this.textBox_dp2library_userName.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_dp2library_userName.Name = "textBox_dp2library_userName";
            this.textBox_dp2library_userName.Size = new System.Drawing.Size(266, 31);
            this.textBox_dp2library_userName.TabIndex = 14;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 223);
            this.label3.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(106, 21);
            this.label3.TabIndex = 15;
            this.label3.Text = "密码(&P)：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 175);
            this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(127, 21);
            this.label2.TabIndex = 13;
            this.label2.Text = "用户名(&U)：";
            // 
            // textBox_dp2library_serverUrl
            // 
            this.textBox_dp2library_serverUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dp2library_serverUrl.Location = new System.Drawing.Point(16, 52);
            this.textBox_dp2library_serverUrl.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_dp2library_serverUrl.Name = "textBox_dp2library_serverUrl";
            this.textBox_dp2library_serverUrl.Size = new System.Drawing.Size(760, 31);
            this.textBox_dp2library_serverUrl.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 20);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(249, 21);
            this.label1.TabIndex = 10;
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
            this.toolStrip_server.Location = new System.Drawing.Point(16, 92);
            this.toolStrip_server.Name = "toolStrip_server";
            this.toolStrip_server.Size = new System.Drawing.Size(760, 51);
            this.toolStrip_server.TabIndex = 12;
            this.toolStrip_server.Text = "toolStrip1";
            // 
            // toolStripButton_cfg_setXeServer
            // 
            this.toolStripButton_cfg_setXeServer.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_cfg_setXeServer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_cfg_setXeServer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_cfg_setXeServer.Name = "toolStripButton_cfg_setXeServer";
            this.toolStripButton_cfg_setXeServer.Size = new System.Drawing.Size(142, 45);
            this.toolStripButton_cfg_setXeServer.Text = "单机版服务器";
            this.toolStripButton_cfg_setXeServer.ToolTipText = "设为单机版服务器";
            this.toolStripButton_cfg_setXeServer.Click += new System.EventHandler(this.toolStripButton_cfg_setXeServer_Click);
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
            this.toolStripButton_cfg_setHongnibaServer.Size = new System.Drawing.Size(231, 45);
            this.toolStripButton_cfg_setHongnibaServer.Text = "红泥巴.数字平台服务器";
            this.toolStripButton_cfg_setHongnibaServer.ToolTipText = "设为红泥巴.数字平台服务器";
            this.toolStripButton_cfg_setHongnibaServer.Click += new System.EventHandler(this.toolStripButton_cfg_setHongnibaServer_Click);
            // 
            // tabPage_sip2
            // 
            this.tabPage_sip2.AutoScroll = true;
            this.tabPage_sip2.Controls.Add(this.checkBox_sip_localStore);
            this.tabPage_sip2.Controls.Add(this.textBox_sip_locationList);
            this.tabPage_sip2.Controls.Add(this.label12);
            this.tabPage_sip2.Controls.Add(this.textBox_sip_institution);
            this.tabPage_sip2.Controls.Add(this.label11);
            this.tabPage_sip2.Controls.Add(this.textBox_sip_port);
            this.tabPage_sip2.Controls.Add(this.label9);
            this.tabPage_sip2.Controls.Add(this.textBox_sip_encoding);
            this.tabPage_sip2.Controls.Add(this.label5);
            this.tabPage_sip2.Controls.Add(this.textBox_sip_password);
            this.tabPage_sip2.Controls.Add(this.textBox_sip_userName);
            this.tabPage_sip2.Controls.Add(this.label6);
            this.tabPage_sip2.Controls.Add(this.label7);
            this.tabPage_sip2.Controls.Add(this.textBox_sip_serverAddr);
            this.tabPage_sip2.Controls.Add(this.label8);
            this.tabPage_sip2.Location = new System.Drawing.Point(4, 31);
            this.tabPage_sip2.Name = "tabPage_sip2";
            this.tabPage_sip2.Size = new System.Drawing.Size(789, 596);
            this.tabPage_sip2.TabIndex = 2;
            this.tabPage_sip2.Text = "SIP2";
            this.tabPage_sip2.UseVisualStyleBackColor = true;
            // 
            // checkBox_sip_localStore
            // 
            this.checkBox_sip_localStore.AutoSize = true;
            this.checkBox_sip_localStore.Location = new System.Drawing.Point(19, 440);
            this.checkBox_sip_localStore.Name = "checkBox_sip_localStore";
            this.checkBox_sip_localStore.Size = new System.Drawing.Size(195, 25);
            this.checkBox_sip_localStore.TabIndex = 33;
            this.checkBox_sip_localStore.Text = "启用本地存储(&S)";
            this.checkBox_sip_localStore.UseVisualStyleBackColor = true;
            // 
            // textBox_sip_locationList
            // 
            this.textBox_sip_locationList.AcceptsReturn = true;
            this.textBox_sip_locationList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_sip_locationList.Location = new System.Drawing.Point(218, 321);
            this.textBox_sip_locationList.Multiline = true;
            this.textBox_sip_locationList.Name = "textBox_sip_locationList";
            this.textBox_sip_locationList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_sip_locationList.Size = new System.Drawing.Size(561, 101);
            this.textBox_sip_locationList.TabIndex = 32;
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Location = new System.Drawing.Point(15, 321);
            this.label12.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(117, 42);
            this.label12.TabIndex = 31;
            this.label12.Text = "馆藏地(&L):\r\n[每行一个]";
            // 
            // textBox_sip_institution
            // 
            this.textBox_sip_institution.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_sip_institution.Location = new System.Drawing.Point(218, 263);
            this.textBox_sip_institution.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_sip_institution.Name = "textBox_sip_institution";
            this.textBox_sip_institution.Size = new System.Drawing.Size(242, 31);
            this.textBox_sip_institution.TabIndex = 30;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(15, 266);
            this.label11.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(148, 21);
            this.label11.TabIndex = 29;
            this.label11.Text = "机构代码(&I)：";
            // 
            // textBox_sip_port
            // 
            this.textBox_sip_port.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_sip_port.Location = new System.Drawing.Point(218, 89);
            this.textBox_sip_port.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_sip_port.Name = "textBox_sip_port";
            this.textBox_sip_port.Size = new System.Drawing.Size(139, 31);
            this.textBox_sip_port.TabIndex = 28;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(15, 92);
            this.label9.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(127, 21);
            this.label9.TabIndex = 27;
            this.label9.Text = "端口号(&P)：";
            // 
            // textBox_sip_encoding
            // 
            this.textBox_sip_encoding.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_sip_encoding.Location = new System.Drawing.Point(218, 222);
            this.textBox_sip_encoding.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_sip_encoding.Name = "textBox_sip_encoding";
            this.textBox_sip_encoding.Size = new System.Drawing.Size(242, 31);
            this.textBox_sip_encoding.TabIndex = 26;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(15, 225);
            this.label5.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(148, 21);
            this.label5.TabIndex = 25;
            this.label5.Text = "编码方式(&E)：";
            // 
            // textBox_sip_password
            // 
            this.textBox_sip_password.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_sip_password.Location = new System.Drawing.Point(218, 181);
            this.textBox_sip_password.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_sip_password.Name = "textBox_sip_password";
            this.textBox_sip_password.PasswordChar = '*';
            this.textBox_sip_password.Size = new System.Drawing.Size(242, 31);
            this.textBox_sip_password.TabIndex = 24;
            // 
            // textBox_sip_userName
            // 
            this.textBox_sip_userName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_sip_userName.Location = new System.Drawing.Point(218, 140);
            this.textBox_sip_userName.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_sip_userName.Name = "textBox_sip_userName";
            this.textBox_sip_userName.Size = new System.Drawing.Size(242, 31);
            this.textBox_sip_userName.TabIndex = 22;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(15, 184);
            this.label6.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(106, 21);
            this.label6.TabIndex = 23;
            this.label6.Text = "密码(&P)：";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(15, 143);
            this.label7.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(127, 21);
            this.label7.TabIndex = 21;
            this.label7.Text = "用户名(&U)：";
            // 
            // textBox_sip_serverAddr
            // 
            this.textBox_sip_serverAddr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_sip_serverAddr.Location = new System.Drawing.Point(218, 48);
            this.textBox_sip_serverAddr.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_sip_serverAddr.Name = "textBox_sip_serverAddr";
            this.textBox_sip_serverAddr.Size = new System.Drawing.Size(561, 31);
            this.textBox_sip_serverAddr.TabIndex = 20;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(15, 51);
            this.label8.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(181, 21);
            this.label8.TabIndex = 19;
            this.label8.Text = "SIP2 服务器地址:";
            // 
            // tabPage_other
            // 
            this.tabPage_other.Controls.Add(this.checkBox_enableTagCache);
            this.tabPage_other.Controls.Add(this.groupBox1);
            this.tabPage_other.Location = new System.Drawing.Point(4, 31);
            this.tabPage_other.Name = "tabPage_other";
            this.tabPage_other.Size = new System.Drawing.Size(789, 596);
            this.tabPage_other.TabIndex = 3;
            this.tabPage_other.Text = "其它";
            this.tabPage_other.UseVisualStyleBackColor = true;
            // 
            // checkBox_enableTagCache
            // 
            this.checkBox_enableTagCache.AutoSize = true;
            this.checkBox_enableTagCache.Location = new System.Drawing.Point(23, 30);
            this.checkBox_enableTagCache.Name = "checkBox_enableTagCache";
            this.checkBox_enableTagCache.Size = new System.Drawing.Size(237, 25);
            this.checkBox_enableTagCache.TabIndex = 6;
            this.checkBox_enableTagCache.Text = "启用标签信息缓存(&C)";
            this.checkBox_enableTagCache.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.textBox_verifyRule);
            this.groupBox1.Location = new System.Drawing.Point(3, 89);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(12);
            this.groupBox1.Size = new System.Drawing.Size(783, 504);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " 条码号校验规则 ";
            // 
            // textBox_verifyRule
            // 
            this.textBox_verifyRule.AcceptsReturn = true;
            this.textBox_verifyRule.AcceptsTab = true;
            this.textBox_verifyRule.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_verifyRule.Location = new System.Drawing.Point(12, 36);
            this.textBox_verifyRule.Multiline = true;
            this.textBox_verifyRule.Name = "textBox_verifyRule";
            this.textBox_verifyRule.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_verifyRule.Size = new System.Drawing.Size(759, 456);
            this.textBox_verifyRule.TabIndex = 2;
            // 
            // SettingDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(821, 697);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "SettingDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "设置";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SettingDialog_FormClosed);
            this.Load += new System.EventHandler(this.SettingDialog_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_rfid.ResumeLayout(false);
            this.tabPage_rfid.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_seconds)).EndInit();
            this.groupBox11.ResumeLayout(false);
            this.groupBox11.PerformLayout();
            this.tabPage_dp2library.ResumeLayout(false);
            this.tabPage_dp2library.PerformLayout();
            this.toolStrip_server.ResumeLayout(false);
            this.toolStrip_server.PerformLayout();
            this.tabPage_sip2.ResumeLayout(false);
            this.tabPage_sip2.PerformLayout();
            this.tabPage_other.ResumeLayout(false);
            this.tabPage_other.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_rfid;
        private System.Windows.Forms.TabPage tabPage_dp2library;
        private System.Windows.Forms.TabPage tabPage_sip2;
        private System.Windows.Forms.GroupBox groupBox11;
        private System.Windows.Forms.Button button_rfid_setRfidUrlDefaultValue;
        private System.Windows.Forms.TextBox textBox_rfid_rfidCenterUrl;
        public System.Windows.Forms.TextBox textBox_dp2library_location;
        private System.Windows.Forms.Label label4;
        public System.Windows.Forms.TextBox textBox_dp2library_password;
        public System.Windows.Forms.TextBox textBox_dp2library_userName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_dp2library_serverUrl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStrip toolStrip_server;
        private System.Windows.Forms.ToolStripButton toolStripButton_cfg_setXeServer;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_cfg_setHongnibaServer;
        public System.Windows.Forms.TextBox textBox_sip_port;
        private System.Windows.Forms.Label label9;
        public System.Windows.Forms.TextBox textBox_sip_encoding;
        private System.Windows.Forms.Label label5;
        public System.Windows.Forms.TextBox textBox_sip_password;
        public System.Windows.Forms.TextBox textBox_sip_userName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox_sip_serverAddr;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.NumericUpDown numericUpDown_seconds;
        private System.Windows.Forms.Label label10;
        public System.Windows.Forms.TextBox textBox_sip_institution;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox textBox_sip_locationList;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.CheckBox checkBox_sip_localStore;
        private System.Windows.Forms.CheckBox checkBox_rfid_rpanTypeSwitch;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TextBox textBox_rfid_uploadInterfaceUrl;
        private System.Windows.Forms.TabPage tabPage_other;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox_verifyRule;
        private System.Windows.Forms.CheckBox checkBox_enableTagCache;
    }
}