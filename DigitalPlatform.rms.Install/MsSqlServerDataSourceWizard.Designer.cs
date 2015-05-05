namespace DigitalPlatform.rms
{
    partial class MsSqlServerDataSourceWizard
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
            this.button_finish = new System.Windows.Forms.Button();
            this.button_next = new System.Windows.Forms.Button();
            this.button_prev = new System.Windows.Forms.Button();
            this.tabControl_main = new DigitalPlatform.WizardPages();
            this.tabPage_welcome = new System.Windows.Forms.TabPage();
            this.textBox_message = new System.Windows.Forms.TextBox();
            this.tabPage_sqlServerName = new System.Windows.Forms.TabPage();
            this.textBox_instanceName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button_getSqlServerName = new System.Windows.Forms.Button();
            this.textBox_sqlServerName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage_createLogin = new System.Windows.Forms.TabPage();
            this.button_copySqlServerInfo = new System.Windows.Forms.Button();
            this.groupBox_login = new System.Windows.Forms.GroupBox();
            this.textBox_confirmLoginPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_loginPassword = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_loginName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tabControl_main.SuspendLayout();
            this.tabPage_welcome.SuspendLayout();
            this.tabPage_sqlServerName.SuspendLayout();
            this.tabPage_createLogin.SuspendLayout();
            this.groupBox_login.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_finish
            // 
            this.button_finish.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_finish.Location = new System.Drawing.Point(333, 234);
            this.button_finish.Name = "button_finish";
            this.button_finish.Size = new System.Drawing.Size(75, 23);
            this.button_finish.TabIndex = 6;
            this.button_finish.Text = "完成";
            this.button_finish.UseVisualStyleBackColor = true;
            this.button_finish.Click += new System.EventHandler(this.button_finish_Click);
            // 
            // button_next
            // 
            this.button_next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_next.Location = new System.Drawing.Point(227, 234);
            this.button_next.Name = "button_next";
            this.button_next.Size = new System.Drawing.Size(75, 23);
            this.button_next.TabIndex = 5;
            this.button_next.Text = "下一步";
            this.button_next.UseVisualStyleBackColor = true;
            this.button_next.Click += new System.EventHandler(this.button_next_Click);
            // 
            // button_prev
            // 
            this.button_prev.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_prev.Location = new System.Drawing.Point(146, 234);
            this.button_prev.Name = "button_prev";
            this.button_prev.Size = new System.Drawing.Size(75, 23);
            this.button_prev.TabIndex = 4;
            this.button_prev.Text = "上一步";
            this.button_prev.UseVisualStyleBackColor = true;
            this.button_prev.Click += new System.EventHandler(this.button_prev_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_welcome);
            this.tabControl_main.Controls.Add(this.tabPage_sqlServerName);
            this.tabControl_main.Controls.Add(this.tabPage_createLogin);
            this.tabControl_main.Location = new System.Drawing.Point(1, 13);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(407, 215);
            this.tabControl_main.TabIndex = 0;
            this.tabControl_main.SelectedIndexChanged += new System.EventHandler(this.tabControl_main_SelectedIndexChanged);
            // 
            // tabPage_welcome
            // 
            this.tabPage_welcome.Controls.Add(this.textBox_message);
            this.tabPage_welcome.Location = new System.Drawing.Point(4, 22);
            this.tabPage_welcome.Name = "tabPage_welcome";
            this.tabPage_welcome.Size = new System.Drawing.Size(399, 189);
            this.tabPage_welcome.TabIndex = 2;
            this.tabPage_welcome.Text = "说明";
            this.tabPage_welcome.UseVisualStyleBackColor = true;
            // 
            // textBox_message
            // 
            this.textBox_message.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_message.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_message.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_message.Location = new System.Drawing.Point(0, 0);
            this.textBox_message.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_message.Multiline = true;
            this.textBox_message.Name = "textBox_message";
            this.textBox_message.ReadOnly = true;
            this.textBox_message.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_message.Size = new System.Drawing.Size(399, 189);
            this.textBox_message.TabIndex = 10;
            // 
            // tabPage_sqlServerName
            // 
            this.tabPage_sqlServerName.Controls.Add(this.textBox_instanceName);
            this.tabPage_sqlServerName.Controls.Add(this.label4);
            this.tabPage_sqlServerName.Controls.Add(this.button_getSqlServerName);
            this.tabPage_sqlServerName.Controls.Add(this.textBox_sqlServerName);
            this.tabPage_sqlServerName.Controls.Add(this.label1);
            this.tabPage_sqlServerName.Location = new System.Drawing.Point(4, 22);
            this.tabPage_sqlServerName.Name = "tabPage_sqlServerName";
            this.tabPage_sqlServerName.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_sqlServerName.Size = new System.Drawing.Size(399, 189);
            this.tabPage_sqlServerName.TabIndex = 0;
            this.tabPage_sqlServerName.Text = "SQL 服务器名";
            this.tabPage_sqlServerName.UseVisualStyleBackColor = true;
            this.tabPage_sqlServerName.Validating += new System.ComponentModel.CancelEventHandler(this.tabPage_sqlServerName_Validating);
            // 
            // textBox_instanceName
            // 
            this.textBox_instanceName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_instanceName.Location = new System.Drawing.Point(125, 102);
            this.textBox_instanceName.Name = "textBox_instanceName";
            this.textBox_instanceName.Size = new System.Drawing.Size(163, 21);
            this.textBox_instanceName.TabIndex = 8;
            this.textBox_instanceName.Text = "dp2kernel";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 105);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "内核实例名(&I):";
            // 
            // button_getSqlServerName
            // 
            this.button_getSqlServerName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getSqlServerName.Location = new System.Drawing.Point(125, 46);
            this.button_getSqlServerName.Name = "button_getSqlServerName";
            this.button_getSqlServerName.Size = new System.Drawing.Size(266, 23);
            this.button_getSqlServerName.TabIndex = 6;
            this.button_getSqlServerName.Text = "获得邻近的 SQL 服务器名(&G)...";
            this.button_getSqlServerName.UseVisualStyleBackColor = true;
            this.button_getSqlServerName.Click += new System.EventHandler(this.button_getSqlServerName_Click);
            // 
            // textBox_sqlServerName
            // 
            this.textBox_sqlServerName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_sqlServerName.Location = new System.Drawing.Point(125, 19);
            this.textBox_sqlServerName.Name = "textBox_sqlServerName";
            this.textBox_sqlServerName.Size = new System.Drawing.Size(266, 21);
            this.textBox_sqlServerName.TabIndex = 5;
            this.textBox_sqlServerName.TextChanged += new System.EventHandler(this.textBox_sqlServerName_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 12);
            this.label1.TabIndex = 4;
            this.label1.Text = "SQL服务器名(&S):";
            // 
            // tabPage_createLogin
            // 
            this.tabPage_createLogin.Controls.Add(this.button_copySqlServerInfo);
            this.tabPage_createLogin.Controls.Add(this.groupBox_login);
            this.tabPage_createLogin.Location = new System.Drawing.Point(4, 22);
            this.tabPage_createLogin.Name = "tabPage_createLogin";
            this.tabPage_createLogin.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_createLogin.Size = new System.Drawing.Size(399, 189);
            this.tabPage_createLogin.TabIndex = 1;
            this.tabPage_createLogin.Text = "创建登录名";
            this.tabPage_createLogin.UseVisualStyleBackColor = true;
            // 
            // button_copySqlServerInfo
            // 
            this.button_copySqlServerInfo.Enabled = false;
            this.button_copySqlServerInfo.Location = new System.Drawing.Point(6, 154);
            this.button_copySqlServerInfo.Name = "button_copySqlServerInfo";
            this.button_copySqlServerInfo.Size = new System.Drawing.Size(291, 23);
            this.button_copySqlServerInfo.TabIndex = 6;
            this.button_copySqlServerInfo.Text = "复制 SQL 服务器信息到 Windows 剪贴板";
            this.button_copySqlServerInfo.UseVisualStyleBackColor = true;
            this.button_copySqlServerInfo.Click += new System.EventHandler(this.button_copySqlServerInfo_Click);
            // 
            // groupBox_login
            // 
            this.groupBox_login.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_login.Controls.Add(this.textBox_confirmLoginPassword);
            this.groupBox_login.Controls.Add(this.label3);
            this.groupBox_login.Controls.Add(this.textBox_loginPassword);
            this.groupBox_login.Controls.Add(this.label2);
            this.groupBox_login.Controls.Add(this.textBox_loginName);
            this.groupBox_login.Controls.Add(this.label5);
            this.groupBox_login.Location = new System.Drawing.Point(6, 19);
            this.groupBox_login.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox_login.Name = "groupBox_login";
            this.groupBox_login.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox_login.Size = new System.Drawing.Size(386, 129);
            this.groupBox_login.TabIndex = 5;
            this.groupBox_login.TabStop = false;
            this.groupBox_login.Text = " 创建一个用于 dp2Kernel 的 SQL Server 登录名 ";
            // 
            // textBox_confirmLoginPassword
            // 
            this.textBox_confirmLoginPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_confirmLoginPassword.Location = new System.Drawing.Point(111, 88);
            this.textBox_confirmLoginPassword.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_confirmLoginPassword.Name = "textBox_confirmLoginPassword";
            this.textBox_confirmLoginPassword.PasswordChar = '*';
            this.textBox_confirmLoginPassword.Size = new System.Drawing.Size(230, 21);
            this.textBox_confirmLoginPassword.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 90);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "再次输入密码:";
            // 
            // textBox_loginPassword
            // 
            this.textBox_loginPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_loginPassword.Location = new System.Drawing.Point(111, 63);
            this.textBox_loginPassword.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_loginPassword.Name = "textBox_loginPassword";
            this.textBox_loginPassword.PasswordChar = '*';
            this.textBox_loginPassword.Size = new System.Drawing.Size(230, 21);
            this.textBox_loginPassword.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 66);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "密码(&P):";
            // 
            // textBox_loginName
            // 
            this.textBox_loginName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_loginName.Location = new System.Drawing.Point(111, 26);
            this.textBox_loginName.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_loginName.Name = "textBox_loginName";
            this.textBox_loginName.Size = new System.Drawing.Size(230, 21);
            this.textBox_loginName.TabIndex = 1;
            this.textBox_loginName.Text = "dp2kernel";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 29);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "登录名(&N):";
            // 
            // MsSqlServerDataSourceWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(408, 269);
            this.Controls.Add(this.button_finish);
            this.Controls.Add(this.button_next);
            this.Controls.Add(this.button_prev);
            this.Controls.Add(this.tabControl_main);
            this.Name = "MsSqlServerDataSourceWizard";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "MS SQL Server相关参数设置";
            this.Load += new System.EventHandler(this.MsSqlServerDataSourceWizard_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_welcome.ResumeLayout(false);
            this.tabPage_welcome.PerformLayout();
            this.tabPage_sqlServerName.ResumeLayout(false);
            this.tabPage_sqlServerName.PerformLayout();
            this.tabPage_createLogin.ResumeLayout(false);
            this.groupBox_login.ResumeLayout(false);
            this.groupBox_login.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private DigitalPlatform.WizardPages tabControl_main;
        private System.Windows.Forms.TabPage tabPage_sqlServerName;
        private System.Windows.Forms.TabPage tabPage_createLogin;
        private System.Windows.Forms.Button button_finish;
        private System.Windows.Forms.Button button_next;
        private System.Windows.Forms.Button button_prev;
        private System.Windows.Forms.Button button_getSqlServerName;
        private System.Windows.Forms.TextBox textBox_sqlServerName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_instanceName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox_login;
        private System.Windows.Forms.TextBox textBox_confirmLoginPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_loginPassword;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_loginName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TabPage tabPage_welcome;
        private System.Windows.Forms.TextBox textBox_message;
        private System.Windows.Forms.Button button_copySqlServerInfo;
    }
}