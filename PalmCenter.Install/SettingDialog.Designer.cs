
namespace PalmCenter.Install
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
            this.tabPage_replication = new System.Windows.Forms.TabPage();
            this.textBox_replicationStart = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
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
            this.tabPage_palm = new System.Windows.Forms.TabPage();
            this.checkBox_allow_changeRecognitionQuality = new System.Windows.Forms.CheckBox();
            this.checkBox_allow_changeRegisterQuality = new System.Windows.Forms.CheckBox();
            this.checkBox_allow_changeThreshold = new System.Windows.Forms.CheckBox();
            this.button_setDefaultRecognitionQuality = new System.Windows.Forms.Button();
            this.textBox_cfg_recognitionQualityThreshold = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.button_setDefaultRegisterQuality = new System.Windows.Forms.Button();
            this.textBox_cfg_registerQualityThreshold = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.button_setDefaultThreshold = new System.Windows.Forms.Button();
            this.textBox_cfg_shreshold = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.comboBox_deviceList = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.checkBox_speak = new System.Windows.Forms.CheckBox();
            this.checkBox_beep = new System.Windows.Forms.CheckBox();
            this.tabControl1.SuspendLayout();
            this.tabPage_replication.SuspendLayout();
            this.toolStrip_server.SuspendLayout();
            this.tabPage_palm.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(799, 501);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(103, 40);
            this.button_Cancel.TabIndex = 5;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(688, 501);
            this.button_OK.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(103, 40);
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
            this.tabControl1.Controls.Add(this.tabPage_replication);
            this.tabControl1.Controls.Add(this.tabPage_palm);
            this.tabControl1.Location = new System.Drawing.Point(13, 13);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(890, 482);
            this.tabControl1.TabIndex = 6;
            // 
            // tabPage_replication
            // 
            this.tabPage_replication.AutoScroll = true;
            this.tabPage_replication.Controls.Add(this.textBox_replicationStart);
            this.tabPage_replication.Controls.Add(this.label5);
            this.tabPage_replication.Controls.Add(this.checkBox_cfg_savePasswordLong);
            this.tabPage_replication.Controls.Add(this.textBox_cfg_location);
            this.tabPage_replication.Controls.Add(this.label4);
            this.tabPage_replication.Controls.Add(this.textBox_cfg_password);
            this.tabPage_replication.Controls.Add(this.textBox_cfg_userName);
            this.tabPage_replication.Controls.Add(this.label3);
            this.tabPage_replication.Controls.Add(this.label2);
            this.tabPage_replication.Controls.Add(this.textBox_cfg_dp2LibraryServerUrl);
            this.tabPage_replication.Controls.Add(this.label1);
            this.tabPage_replication.Controls.Add(this.toolStrip_server);
            this.tabPage_replication.Location = new System.Drawing.Point(4, 31);
            this.tabPage_replication.Name = "tabPage_replication";
            this.tabPage_replication.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_replication.Size = new System.Drawing.Size(882, 447);
            this.tabPage_replication.TabIndex = 0;
            this.tabPage_replication.Text = "同步";
            this.tabPage_replication.UseVisualStyleBackColor = true;
            // 
            // textBox_replicationStart
            // 
            this.textBox_replicationStart.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_replicationStart.Location = new System.Drawing.Point(192, 378);
            this.textBox_replicationStart.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_replicationStart.Name = "textBox_replicationStart";
            this.textBox_replicationStart.Size = new System.Drawing.Size(283, 31);
            this.textBox_replicationStart.TabIndex = 46;
            this.textBox_replicationStart.Visible = false;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 381);
            this.label5.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(180, 21);
            this.label5.TabIndex = 45;
            this.label5.Text = "日志同步起点(&R):";
            this.label5.Visible = false;
            // 
            // checkBox_cfg_savePasswordLong
            // 
            this.checkBox_cfg_savePasswordLong.AutoSize = true;
            this.checkBox_cfg_savePasswordLong.Checked = true;
            this.checkBox_cfg_savePasswordLong.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_cfg_savePasswordLong.Enabled = false;
            this.checkBox_cfg_savePasswordLong.Location = new System.Drawing.Point(16, 309);
            this.checkBox_cfg_savePasswordLong.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_cfg_savePasswordLong.Name = "checkBox_cfg_savePasswordLong";
            this.checkBox_cfg_savePasswordLong.Size = new System.Drawing.Size(153, 25);
            this.checkBox_cfg_savePasswordLong.TabIndex = 37;
            this.checkBox_cfg_savePasswordLong.Text = "保存密码(&L)";
            // 
            // textBox_cfg_location
            // 
            this.textBox_cfg_location.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_cfg_location.Location = new System.Drawing.Point(192, 264);
            this.textBox_cfg_location.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_cfg_location.Name = "textBox_cfg_location";
            this.textBox_cfg_location.Size = new System.Drawing.Size(283, 31);
            this.textBox_cfg_location.TabIndex = 36;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 267);
            this.label4.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(148, 21);
            this.label4.TabIndex = 35;
            this.label4.Text = "工作台号(&W)：";
            // 
            // textBox_cfg_password
            // 
            this.textBox_cfg_password.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_cfg_password.Location = new System.Drawing.Point(192, 216);
            this.textBox_cfg_password.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_cfg_password.Name = "textBox_cfg_password";
            this.textBox_cfg_password.PasswordChar = '*';
            this.textBox_cfg_password.Size = new System.Drawing.Size(283, 31);
            this.textBox_cfg_password.TabIndex = 34;
            // 
            // textBox_cfg_userName
            // 
            this.textBox_cfg_userName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_cfg_userName.Location = new System.Drawing.Point(192, 168);
            this.textBox_cfg_userName.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_cfg_userName.Name = "textBox_cfg_userName";
            this.textBox_cfg_userName.Size = new System.Drawing.Size(283, 31);
            this.textBox_cfg_userName.TabIndex = 32;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 219);
            this.label3.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(106, 21);
            this.label3.TabIndex = 33;
            this.label3.Text = "密码(&P)：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 171);
            this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(127, 21);
            this.label2.TabIndex = 31;
            this.label2.Text = "用户名(&U)：";
            // 
            // textBox_cfg_dp2LibraryServerUrl
            // 
            this.textBox_cfg_dp2LibraryServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_cfg_dp2LibraryServerUrl.Location = new System.Drawing.Point(17, 48);
            this.textBox_cfg_dp2LibraryServerUrl.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_cfg_dp2LibraryServerUrl.Name = "textBox_cfg_dp2LibraryServerUrl";
            this.textBox_cfg_dp2LibraryServerUrl.Size = new System.Drawing.Size(841, 31);
            this.textBox_cfg_dp2LibraryServerUrl.TabIndex = 29;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(249, 21);
            this.label1.TabIndex = 28;
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
            this.toolStrip_server.Location = new System.Drawing.Point(17, 88);
            this.toolStrip_server.Name = "toolStrip_server";
            this.toolStrip_server.Size = new System.Drawing.Size(841, 51);
            this.toolStrip_server.TabIndex = 30;
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
            // tabPage_palm
            // 
            this.tabPage_palm.AutoScroll = true;
            this.tabPage_palm.Controls.Add(this.checkBox_allow_changeRecognitionQuality);
            this.tabPage_palm.Controls.Add(this.checkBox_allow_changeRegisterQuality);
            this.tabPage_palm.Controls.Add(this.checkBox_allow_changeThreshold);
            this.tabPage_palm.Controls.Add(this.button_setDefaultRecognitionQuality);
            this.tabPage_palm.Controls.Add(this.textBox_cfg_recognitionQualityThreshold);
            this.tabPage_palm.Controls.Add(this.label9);
            this.tabPage_palm.Controls.Add(this.button_setDefaultRegisterQuality);
            this.tabPage_palm.Controls.Add(this.textBox_cfg_registerQualityThreshold);
            this.tabPage_palm.Controls.Add(this.label8);
            this.tabPage_palm.Controls.Add(this.button_setDefaultThreshold);
            this.tabPage_palm.Controls.Add(this.textBox_cfg_shreshold);
            this.tabPage_palm.Controls.Add(this.label7);
            this.tabPage_palm.Controls.Add(this.comboBox_deviceList);
            this.tabPage_palm.Controls.Add(this.label6);
            this.tabPage_palm.Controls.Add(this.checkBox_speak);
            this.tabPage_palm.Controls.Add(this.checkBox_beep);
            this.tabPage_palm.Location = new System.Drawing.Point(4, 31);
            this.tabPage_palm.Name = "tabPage_palm";
            this.tabPage_palm.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_palm.Size = new System.Drawing.Size(882, 447);
            this.tabPage_palm.TabIndex = 1;
            this.tabPage_palm.Text = "掌纹";
            this.tabPage_palm.UseVisualStyleBackColor = true;
            // 
            // checkBox_allow_changeRecognitionQuality
            // 
            this.checkBox_allow_changeRecognitionQuality.AutoSize = true;
            this.checkBox_allow_changeRecognitionQuality.Location = new System.Drawing.Point(485, 174);
            this.checkBox_allow_changeRecognitionQuality.Name = "checkBox_allow_changeRecognitionQuality";
            this.checkBox_allow_changeRecognitionQuality.Size = new System.Drawing.Size(120, 25);
            this.checkBox_allow_changeRecognitionQuality.TabIndex = 71;
            this.checkBox_allow_changeRecognitionQuality.Text = "允许修改";
            this.checkBox_allow_changeRecognitionQuality.UseVisualStyleBackColor = true;
            // 
            // checkBox_allow_changeRegisterQuality
            // 
            this.checkBox_allow_changeRegisterQuality.AutoSize = true;
            this.checkBox_allow_changeRegisterQuality.Location = new System.Drawing.Point(485, 124);
            this.checkBox_allow_changeRegisterQuality.Name = "checkBox_allow_changeRegisterQuality";
            this.checkBox_allow_changeRegisterQuality.Size = new System.Drawing.Size(120, 25);
            this.checkBox_allow_changeRegisterQuality.TabIndex = 70;
            this.checkBox_allow_changeRegisterQuality.Text = "允许修改";
            this.checkBox_allow_changeRegisterQuality.UseVisualStyleBackColor = true;
            // 
            // checkBox_allow_changeThreshold
            // 
            this.checkBox_allow_changeThreshold.AutoSize = true;
            this.checkBox_allow_changeThreshold.Location = new System.Drawing.Point(485, 68);
            this.checkBox_allow_changeThreshold.Name = "checkBox_allow_changeThreshold";
            this.checkBox_allow_changeThreshold.Size = new System.Drawing.Size(120, 25);
            this.checkBox_allow_changeThreshold.TabIndex = 69;
            this.checkBox_allow_changeThreshold.Text = "允许修改";
            this.checkBox_allow_changeThreshold.UseVisualStyleBackColor = true;
            // 
            // button_setDefaultRecognitionQuality
            // 
            this.button_setDefaultRecognitionQuality.Location = new System.Drawing.Point(302, 166);
            this.button_setDefaultRecognitionQuality.Name = "button_setDefaultRecognitionQuality";
            this.button_setDefaultRecognitionQuality.Size = new System.Drawing.Size(177, 47);
            this.button_setDefaultRecognitionQuality.TabIndex = 68;
            this.button_setDefaultRecognitionQuality.Text = "恢复默认值";
            this.button_setDefaultRecognitionQuality.UseVisualStyleBackColor = true;
            // 
            // textBox_cfg_recognitionQualityThreshold
            // 
            this.textBox_cfg_recognitionQualityThreshold.Location = new System.Drawing.Point(196, 174);
            this.textBox_cfg_recognitionQualityThreshold.Name = "textBox_cfg_recognitionQualityThreshold";
            this.textBox_cfg_recognitionQualityThreshold.ReadOnly = true;
            this.textBox_cfg_recognitionQualityThreshold.Size = new System.Drawing.Size(100, 31);
            this.textBox_cfg_recognitionQualityThreshold.TabIndex = 67;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(17, 177);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(180, 21);
            this.label9.TabIndex = 66;
            this.label9.Text = "识别质量阈值(&R):";
            // 
            // button_setDefaultRegisterQuality
            // 
            this.button_setDefaultRegisterQuality.Location = new System.Drawing.Point(303, 113);
            this.button_setDefaultRegisterQuality.Name = "button_setDefaultRegisterQuality";
            this.button_setDefaultRegisterQuality.Size = new System.Drawing.Size(177, 47);
            this.button_setDefaultRegisterQuality.TabIndex = 65;
            this.button_setDefaultRegisterQuality.Text = "恢复默认值";
            this.button_setDefaultRegisterQuality.UseVisualStyleBackColor = true;
            // 
            // textBox_cfg_registerQualityThreshold
            // 
            this.textBox_cfg_registerQualityThreshold.Location = new System.Drawing.Point(197, 121);
            this.textBox_cfg_registerQualityThreshold.Name = "textBox_cfg_registerQualityThreshold";
            this.textBox_cfg_registerQualityThreshold.ReadOnly = true;
            this.textBox_cfg_registerQualityThreshold.Size = new System.Drawing.Size(100, 31);
            this.textBox_cfg_registerQualityThreshold.TabIndex = 64;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(18, 124);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(180, 21);
            this.label8.TabIndex = 63;
            this.label8.Text = "登记质量阈值(&R):";
            // 
            // button_setDefaultThreshold
            // 
            this.button_setDefaultThreshold.Location = new System.Drawing.Point(302, 57);
            this.button_setDefaultThreshold.Name = "button_setDefaultThreshold";
            this.button_setDefaultThreshold.Size = new System.Drawing.Size(177, 47);
            this.button_setDefaultThreshold.TabIndex = 60;
            this.button_setDefaultThreshold.Text = "恢复默认值";
            this.button_setDefaultThreshold.UseVisualStyleBackColor = true;
            // 
            // textBox_cfg_shreshold
            // 
            this.textBox_cfg_shreshold.Location = new System.Drawing.Point(196, 65);
            this.textBox_cfg_shreshold.Name = "textBox_cfg_shreshold";
            this.textBox_cfg_shreshold.ReadOnly = true;
            this.textBox_cfg_shreshold.Size = new System.Drawing.Size(100, 31);
            this.textBox_cfg_shreshold.TabIndex = 59;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(17, 68);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(180, 21);
            this.label7.TabIndex = 58;
            this.label7.Text = "掌纹比对阈值(&T):";
            // 
            // comboBox_deviceList
            // 
            this.comboBox_deviceList.FormattingEnabled = true;
            this.comboBox_deviceList.Location = new System.Drawing.Point(196, 16);
            this.comboBox_deviceList.Name = "comboBox_deviceList";
            this.comboBox_deviceList.Size = new System.Drawing.Size(283, 29);
            this.comboBox_deviceList.TabIndex = 57;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(16, 19);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(159, 21);
            this.label6.TabIndex = 56;
            this.label6.Text = "当前掌纹仪(&P):";
            // 
            // checkBox_speak
            // 
            this.checkBox_speak.AutoSize = true;
            this.checkBox_speak.Location = new System.Drawing.Point(141, 229);
            this.checkBox_speak.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.checkBox_speak.Name = "checkBox_speak";
            this.checkBox_speak.Size = new System.Drawing.Size(153, 25);
            this.checkBox_speak.TabIndex = 62;
            this.checkBox_speak.Text = "语音提示(&S)";
            this.checkBox_speak.UseVisualStyleBackColor = true;
            // 
            // checkBox_beep
            // 
            this.checkBox_beep.AutoSize = true;
            this.checkBox_beep.Location = new System.Drawing.Point(21, 229);
            this.checkBox_beep.Margin = new System.Windows.Forms.Padding(2, 4, 2, 4);
            this.checkBox_beep.Name = "checkBox_beep";
            this.checkBox_beep.Size = new System.Drawing.Size(111, 25);
            this.checkBox_beep.TabIndex = 61;
            this.checkBox_beep.Text = "蜂鸣(&B)";
            this.checkBox_beep.UseVisualStyleBackColor = true;
            // 
            // SettingDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(915, 553);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "SettingDialog";
            this.ShowIcon = false;
            this.Text = "设置";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SettingDialog_FormClosed);
            this.Load += new System.EventHandler(this.SettingDialog_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_replication.ResumeLayout(false);
            this.tabPage_replication.PerformLayout();
            this.toolStrip_server.ResumeLayout(false);
            this.toolStrip_server.PerformLayout();
            this.tabPage_palm.ResumeLayout(false);
            this.tabPage_palm.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_replication;
        private System.Windows.Forms.TabPage tabPage_palm;
        public System.Windows.Forms.TextBox textBox_replicationStart;
        private System.Windows.Forms.Label label5;
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
        private System.Windows.Forms.CheckBox checkBox_allow_changeRecognitionQuality;
        private System.Windows.Forms.CheckBox checkBox_allow_changeRegisterQuality;
        private System.Windows.Forms.CheckBox checkBox_allow_changeThreshold;
        private System.Windows.Forms.Button button_setDefaultRecognitionQuality;
        private System.Windows.Forms.TextBox textBox_cfg_recognitionQualityThreshold;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Button button_setDefaultRegisterQuality;
        private System.Windows.Forms.TextBox textBox_cfg_registerQualityThreshold;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button button_setDefaultThreshold;
        private System.Windows.Forms.TextBox textBox_cfg_shreshold;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ComboBox comboBox_deviceList;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.CheckBox checkBox_speak;
        private System.Windows.Forms.CheckBox checkBox_beep;
    }
}