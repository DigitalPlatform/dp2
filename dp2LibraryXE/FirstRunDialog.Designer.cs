namespace dp2LibraryXE
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
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.radioButton_licenseMode_standard = new System.Windows.Forms.RadioButton();
            this.radioButton_licenseMode_testing = new System.Windows.Forms.RadioButton();
            this.label3 = new System.Windows.Forms.Label();
            this.radioButton_licenseMode_miniServer = new System.Windows.Forms.RadioButton();
            this.tabControl_main.SuspendLayout();
            this.tabPage_welcome.SuspendLayout();
            this.tabPage_license.SuspendLayout();
            this.tabPage_licenseMode.SuspendLayout();
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
            this.label_welcome.Text = "欢迎使用\r\ndp2 单机版/小型版图书馆服务器 (dp2Library XE)\r\n这是一个图书馆业务服务器软件\r\n\r\n(C)2006-2014 版权所有 数字平台(北京)" +
    "软件有限责任公司";
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
            this.tabPage_licenseMode.AutoScroll = true;
            this.tabPage_licenseMode.Controls.Add(this.label3);
            this.tabPage_licenseMode.Controls.Add(this.radioButton_licenseMode_miniServer);
            this.tabPage_licenseMode.Controls.Add(this.label2);
            this.tabPage_licenseMode.Controls.Add(this.label1);
            this.tabPage_licenseMode.Controls.Add(this.radioButton_licenseMode_standard);
            this.tabPage_licenseMode.Controls.Add(this.radioButton_licenseMode_testing);
            this.tabPage_licenseMode.Location = new System.Drawing.Point(4, 22);
            this.tabPage_licenseMode.Name = "tabPage_licenseMode";
            this.tabPage_licenseMode.Size = new System.Drawing.Size(441, 207);
            this.tabPage_licenseMode.TabIndex = 2;
            this.tabPage_licenseMode.Text = "授权模式";
            this.tabPage_licenseMode.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.Location = new System.Drawing.Point(28, 123);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(385, 38);
            this.label2.TabIndex = 3;
            this.label2.Text = "需要注册后才能启用。\r\n数据库的数量和记录容量无任何限制。";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(26, 37);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(387, 64);
            this.label1.TabIndex = 2;
            this.label1.Text = "评估模式不需要注册即可使用。\r\n在此许可您在非生产环境下免费评估使用本软件 30 天，详见“最终用户许可协议”。\r\n评估模式下，数据库的数量和记录容量有一定限制。" +
    "";
            // 
            // radioButton_licenseMode_standard
            // 
            this.radioButton_licenseMode_standard.AutoSize = true;
            this.radioButton_licenseMode_standard.Checked = true;
            this.radioButton_licenseMode_standard.Location = new System.Drawing.Point(7, 104);
            this.radioButton_licenseMode_standard.Name = "radioButton_licenseMode_standard";
            this.radioButton_licenseMode_standard.Size = new System.Drawing.Size(89, 16);
            this.radioButton_licenseMode_standard.TabIndex = 1;
            this.radioButton_licenseMode_standard.Text = "正式 - 单机";
            this.radioButton_licenseMode_standard.UseVisualStyleBackColor = true;
            this.radioButton_licenseMode_standard.CheckedChanged += new System.EventHandler(this.radioButton_licenseMode_standard_CheckedChanged);
            // 
            // radioButton_licenseMode_testing
            // 
            this.radioButton_licenseMode_testing.AutoSize = true;
            this.radioButton_licenseMode_testing.Location = new System.Drawing.Point(7, 18);
            this.radioButton_licenseMode_testing.Name = "radioButton_licenseMode_testing";
            this.radioButton_licenseMode_testing.Size = new System.Drawing.Size(89, 16);
            this.radioButton_licenseMode_testing.TabIndex = 0;
            this.radioButton_licenseMode_testing.Text = "评估 - 单机";
            this.radioButton_licenseMode_testing.UseVisualStyleBackColor = true;
            this.radioButton_licenseMode_testing.CheckedChanged += new System.EventHandler(this.radioButton_licenseMode_testing_CheckedChanged);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.Location = new System.Drawing.Point(28, 183);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(385, 50);
            this.label3.TabIndex = 5;
            this.label3.Text = "需要注册后才能启用。\r\n数据库的数量和记录容量无任何限制。";
            // 
            // radioButton_licenseMode_miniServer
            // 
            this.radioButton_licenseMode_miniServer.AutoSize = true;
            this.radioButton_licenseMode_miniServer.Location = new System.Drawing.Point(7, 164);
            this.radioButton_licenseMode_miniServer.Name = "radioButton_licenseMode_miniServer";
            this.radioButton_licenseMode_miniServer.Size = new System.Drawing.Size(125, 16);
            this.radioButton_licenseMode_miniServer.TabIndex = 4;
            this.radioButton_licenseMode_miniServer.Text = "正式 - 小型服务器";
            this.radioButton_licenseMode_miniServer.UseVisualStyleBackColor = true;
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
            this.ResumeLayout(false);

        }

        #endregion

        private DigitalPlatform.WizardPages tabControl_main;
        private System.Windows.Forms.TabPage tabPage_welcome;
        private System.Windows.Forms.TabPage tabPage_license;
        private System.Windows.Forms.TabPage tabPage_licenseMode;
        private System.Windows.Forms.Button button_prev;
        private System.Windows.Forms.Button button_next;
        private System.Windows.Forms.Button button_finish;
        private System.Windows.Forms.Label label_welcome;
        private System.Windows.Forms.TextBox textBox_license;
        private System.Windows.Forms.CheckBox checkBox_license_agree;
        private System.Windows.Forms.RadioButton radioButton_licenseMode_standard;
        private System.Windows.Forms.RadioButton radioButton_licenseMode_testing;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.RadioButton radioButton_licenseMode_miniServer;
    }
}