namespace dp2Catalog
{
    partial class FirstRunDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FirstRunDialog));
            this.button_prev = new System.Windows.Forms.Button();
            this.button_next = new System.Windows.Forms.Button();
            this.button_finish = new System.Windows.Forms.Button();
            this.tabControl_main = new DigitalPlatform.WizardPages();
            this.tabPage_welcome = new System.Windows.Forms.TabPage();
            this.label_welcome = new System.Windows.Forms.Label();
            this.tabPage_license = new System.Windows.Forms.TabPage();
            this.checkBox_license_agree = new System.Windows.Forms.CheckBox();
            this.textBox_license = new System.Windows.Forms.TextBox();
            this.tabPage_licenseMode = new System.Windows.Forms.TabPage();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.radioButton_licenseMode_standard = new System.Windows.Forms.RadioButton();
            this.radioButton_licenseMode_community = new System.Windows.Forms.RadioButton();
            this.tabPage_serverInfo = new System.Windows.Forms.TabPage();
            this.comboBox_server_serverType = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_server_password = new System.Windows.Forms.TextBox();
            this.textBox_server_userName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_server_dp2LibraryServerUrl = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabControl_main.SuspendLayout();
            this.tabPage_welcome.SuspendLayout();
            this.tabPage_license.SuspendLayout();
            this.tabPage_licenseMode.SuspendLayout();
            this.tabPage_serverInfo.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_prev
            // 
            this.button_prev.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_prev.Location = new System.Drawing.Point(194, 251);
            this.button_prev.Name = "button_prev";
            this.button_prev.Size = new System.Drawing.Size(75, 23);
            this.button_prev.TabIndex = 1;
            this.button_prev.Text = "上一步";
            this.button_prev.UseVisualStyleBackColor = true;
            this.button_prev.Click += new System.EventHandler(this.button_prev_Click);
            // 
            // button_next
            // 
            this.button_next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_next.Location = new System.Drawing.Point(275, 251);
            this.button_next.Name = "button_next";
            this.button_next.Size = new System.Drawing.Size(75, 23);
            this.button_next.TabIndex = 2;
            this.button_next.Text = "下一步";
            this.button_next.UseVisualStyleBackColor = true;
            this.button_next.Click += new System.EventHandler(this.button_next_Click);
            // 
            // button_finish
            // 
            this.button_finish.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_finish.Location = new System.Drawing.Point(371, 251);
            this.button_finish.Name = "button_finish";
            this.button_finish.Size = new System.Drawing.Size(75, 23);
            this.button_finish.TabIndex = 3;
            this.button_finish.Text = "完成";
            this.button_finish.UseVisualStyleBackColor = true;
            this.button_finish.Click += new System.EventHandler(this.button_finish_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_welcome);
            this.tabControl_main.Controls.Add(this.tabPage_license);
            this.tabControl_main.Controls.Add(this.tabPage_licenseMode);
            this.tabControl_main.Controls.Add(this.tabPage_serverInfo);
            this.tabControl_main.Location = new System.Drawing.Point(1, 12);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(449, 233);
            this.tabControl_main.TabIndex = 0;
            this.tabControl_main.SelectedIndexChanged += new System.EventHandler(this.tabControl_main_SelectedIndexChanged);
            // 
            // tabPage_welcome
            // 
            this.tabPage_welcome.Controls.Add(this.label_welcome);
            this.tabPage_welcome.Location = new System.Drawing.Point(4, 22);
            this.tabPage_welcome.Name = "tabPage_welcome";
            this.tabPage_welcome.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_welcome.Size = new System.Drawing.Size(441, 207);
            this.tabPage_welcome.TabIndex = 0;
            this.tabPage_welcome.Text = "欢迎";
            this.tabPage_welcome.UseVisualStyleBackColor = true;
            // 
            // label_welcome
            // 
            this.label_welcome.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_welcome.Font = new System.Drawing.Font("宋体", 18F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label_welcome.Location = new System.Drawing.Point(3, 3);
            this.label_welcome.Name = "label_welcome";
            this.label_welcome.Size = new System.Drawing.Size(435, 201);
            this.label_welcome.TabIndex = 0;
            this.label_welcome.Text = "欢迎使用\r\ndp2 编目 (dp2Catalog)\r\n这是一个图书馆业务前端软件\r\n\r\n(C)2006-2015 版权所有 数字平台(北京)软件有限责任公司";
            this.label_welcome.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // tabPage_license
            // 
            this.tabPage_license.Controls.Add(this.checkBox_license_agree);
            this.tabPage_license.Controls.Add(this.textBox_license);
            this.tabPage_license.Location = new System.Drawing.Point(4, 22);
            this.tabPage_license.Name = "tabPage_license";
            this.tabPage_license.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_license.Size = new System.Drawing.Size(441, 207);
            this.tabPage_license.TabIndex = 1;
            this.tabPage_license.Text = "许可协议";
            this.tabPage_license.UseVisualStyleBackColor = true;
            // 
            // checkBox_license_agree
            // 
            this.checkBox_license_agree.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_license_agree.AutoSize = true;
            this.checkBox_license_agree.Location = new System.Drawing.Point(3, 190);
            this.checkBox_license_agree.Name = "checkBox_license_agree";
            this.checkBox_license_agree.Size = new System.Drawing.Size(240, 16);
            this.checkBox_license_agree.TabIndex = 1;
            this.checkBox_license_agree.Text = "我已经阅读并同意接受上述许可协议条款";
            this.checkBox_license_agree.UseVisualStyleBackColor = true;
            this.checkBox_license_agree.CheckedChanged += new System.EventHandler(this.checkBox_license_agree_CheckedChanged);
            // 
            // textBox_license
            // 
            this.textBox_license.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_license.BackColor = System.Drawing.SystemColors.Window;
            this.textBox_license.Location = new System.Drawing.Point(3, 3);
            this.textBox_license.Multiline = true;
            this.textBox_license.Name = "textBox_license";
            this.textBox_license.ReadOnly = true;
            this.textBox_license.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_license.Size = new System.Drawing.Size(435, 185);
            this.textBox_license.TabIndex = 0;
            // 
            // tabPage_licenseMode
            // 
            this.tabPage_licenseMode.Controls.Add(this.label6);
            this.tabPage_licenseMode.Controls.Add(this.label7);
            this.tabPage_licenseMode.Controls.Add(this.radioButton_licenseMode_standard);
            this.tabPage_licenseMode.Controls.Add(this.radioButton_licenseMode_community);
            this.tabPage_licenseMode.Location = new System.Drawing.Point(4, 22);
            this.tabPage_licenseMode.Name = "tabPage_licenseMode";
            this.tabPage_licenseMode.Size = new System.Drawing.Size(441, 207);
            this.tabPage_licenseMode.TabIndex = 3;
            this.tabPage_licenseMode.Text = "发行版";
            this.tabPage_licenseMode.UseVisualStyleBackColor = true;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.Location = new System.Drawing.Point(29, 123);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(368, 38);
            this.label6.TabIndex = 13;
            this.label6.Text = "付费版本。需要输入序列号。\r\n具有各种专业增强和深度定制功能。";
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.Location = new System.Drawing.Point(27, 37);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(370, 64);
            this.label7.TabIndex = 12;
            this.label7.Text = "免费版本。";
            // 
            // radioButton_licenseMode_standard
            // 
            this.radioButton_licenseMode_standard.AutoSize = true;
            this.radioButton_licenseMode_standard.Checked = true;
            this.radioButton_licenseMode_standard.Location = new System.Drawing.Point(8, 104);
            this.radioButton_licenseMode_standard.Name = "radioButton_licenseMode_standard";
            this.radioButton_licenseMode_standard.Size = new System.Drawing.Size(59, 16);
            this.radioButton_licenseMode_standard.TabIndex = 11;
            this.radioButton_licenseMode_standard.TabStop = true;
            this.radioButton_licenseMode_standard.Text = "专业版";
            this.radioButton_licenseMode_standard.UseVisualStyleBackColor = true;
            this.radioButton_licenseMode_standard.CheckedChanged += new System.EventHandler(this.radioButton_licenseMode_standard_CheckedChanged);
            // 
            // radioButton_licenseMode_community
            // 
            this.radioButton_licenseMode_community.AutoSize = true;
            this.radioButton_licenseMode_community.Location = new System.Drawing.Point(8, 18);
            this.radioButton_licenseMode_community.Name = "radioButton_licenseMode_community";
            this.radioButton_licenseMode_community.Size = new System.Drawing.Size(59, 16);
            this.radioButton_licenseMode_community.TabIndex = 10;
            this.radioButton_licenseMode_community.Text = "社区版";
            this.radioButton_licenseMode_community.UseVisualStyleBackColor = true;
            this.radioButton_licenseMode_community.CheckedChanged += new System.EventHandler(this.radioButton_licenseMode_testing_CheckedChanged);
            // 
            // tabPage_serverInfo
            // 
            this.tabPage_serverInfo.AutoScroll = true;
            this.tabPage_serverInfo.Controls.Add(this.comboBox_server_serverType);
            this.tabPage_serverInfo.Controls.Add(this.label4);
            this.tabPage_serverInfo.Controls.Add(this.textBox_server_password);
            this.tabPage_serverInfo.Controls.Add(this.textBox_server_userName);
            this.tabPage_serverInfo.Controls.Add(this.label3);
            this.tabPage_serverInfo.Controls.Add(this.label2);
            this.tabPage_serverInfo.Controls.Add(this.textBox_server_dp2LibraryServerUrl);
            this.tabPage_serverInfo.Controls.Add(this.label1);
            this.tabPage_serverInfo.Location = new System.Drawing.Point(4, 22);
            this.tabPage_serverInfo.Name = "tabPage_serverInfo";
            this.tabPage_serverInfo.Size = new System.Drawing.Size(441, 207);
            this.tabPage_serverInfo.TabIndex = 2;
            this.tabPage_serverInfo.Text = "选择服务器";
            this.tabPage_serverInfo.UseVisualStyleBackColor = true;
            // 
            // comboBox_server_serverType
            // 
            this.comboBox_server_serverType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_server_serverType.FormattingEnabled = true;
            this.comboBox_server_serverType.Items.AddRange(new object[] {
            "单机版 (dp2Library XE)",
            "红泥巴 · 数字平台服务器",
            "其它服务器",
            "[暂时不使用任何服务器]"});
            this.comboBox_server_serverType.Location = new System.Drawing.Point(108, 15);
            this.comboBox_server_serverType.Name = "comboBox_server_serverType";
            this.comboBox_server_serverType.Size = new System.Drawing.Size(156, 20);
            this.comboBox_server_serverType.TabIndex = 1;
            this.comboBox_server_serverType.SelectedIndexChanged += new System.EventHandler(this.comboBox_server_serverType_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(5, 18);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "服务器类型(&T):";
            // 
            // textBox_server_password
            // 
            this.textBox_server_password.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_server_password.Location = new System.Drawing.Point(108, 132);
            this.textBox_server_password.Name = "textBox_server_password";
            this.textBox_server_password.PasswordChar = '*';
            this.textBox_server_password.Size = new System.Drawing.Size(156, 21);
            this.textBox_server_password.TabIndex = 7;
            // 
            // textBox_server_userName
            // 
            this.textBox_server_userName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_server_userName.Location = new System.Drawing.Point(108, 105);
            this.textBox_server_userName.Name = "textBox_server_userName";
            this.textBox_server_userName.Size = new System.Drawing.Size(156, 21);
            this.textBox_server_userName.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(5, 135);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(75, 18);
            this.label3.TabIndex = 6;
            this.label3.Text = "密码(&P)：";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(5, 108);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(75, 18);
            this.label2.TabIndex = 4;
            this.label2.Text = "用户名(&U)：";
            // 
            // textBox_server_dp2LibraryServerUrl
            // 
            this.textBox_server_dp2LibraryServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_server_dp2LibraryServerUrl.Location = new System.Drawing.Point(7, 69);
            this.textBox_server_dp2LibraryServerUrl.Name = "textBox_server_dp2LibraryServerUrl";
            this.textBox_server_dp2LibraryServerUrl.Size = new System.Drawing.Size(423, 21);
            this.textBox_server_dp2LibraryServerUrl.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 53);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(137, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "dp2Library 服务器 URL:";
            // 
            // FirstRunDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(447, 286);
            this.Controls.Add(this.button_finish);
            this.Controls.Add(this.button_next);
            this.Controls.Add(this.button_prev);
            this.Controls.Add(this.tabControl_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FirstRunDialog";
            this.ShowInTaskbar = false;
            this.Text = "欢迎";
            this.Load += new System.EventHandler(this.FirstRunDialog_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_welcome.ResumeLayout(false);
            this.tabPage_license.ResumeLayout(false);
            this.tabPage_license.PerformLayout();
            this.tabPage_licenseMode.ResumeLayout(false);
            this.tabPage_licenseMode.PerformLayout();
            this.tabPage_serverInfo.ResumeLayout(false);
            this.tabPage_serverInfo.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DigitalPlatform.WizardPages tabControl_main;
        private System.Windows.Forms.TabPage tabPage_welcome;
        private System.Windows.Forms.TabPage tabPage_license;
        private System.Windows.Forms.TabPage tabPage_serverInfo;
        private System.Windows.Forms.Button button_prev;
        private System.Windows.Forms.Button button_next;
        private System.Windows.Forms.Button button_finish;
        private System.Windows.Forms.TextBox textBox_server_dp2LibraryServerUrl;
        private System.Windows.Forms.Label label1;
        public System.Windows.Forms.TextBox textBox_server_password;
        public System.Windows.Forms.TextBox textBox_server_userName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_server_serverType;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label_welcome;
        private System.Windows.Forms.TextBox textBox_license;
        private System.Windows.Forms.CheckBox checkBox_license_agree;
        private System.Windows.Forms.TabPage tabPage_licenseMode;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.RadioButton radioButton_licenseMode_standard;
        private System.Windows.Forms.RadioButton radioButton_licenseMode_community;
    }
}