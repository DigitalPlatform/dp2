namespace DigitalPlatform.rms
{
    partial class MsSqlServerDataSourceDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MsSqlServerDataSourceDlg));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_sqlServerName = new System.Windows.Forms.TextBox();
            this.button_getSqlServerName = new System.Windows.Forms.Button();
            this.textBox_instanceName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.textBox_message = new System.Windows.Forms.TextBox();
            this.groupBox_login = new System.Windows.Forms.GroupBox();
            this.textBox_confirmLoginPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_loginPassword = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_loginName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox_login.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 110);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "SQL服务器名(&S):";
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // textBox_sqlServerName
            // 
            this.textBox_sqlServerName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_sqlServerName.Location = new System.Drawing.Point(123, 108);
            this.textBox_sqlServerName.Name = "textBox_sqlServerName";
            this.textBox_sqlServerName.Size = new System.Drawing.Size(300, 21);
            this.textBox_sqlServerName.TabIndex = 2;
            this.textBox_sqlServerName.TextChanged += new System.EventHandler(this.textBox_sqlServerName_TextChanged);
            // 
            // button_getSqlServerName
            // 
            this.button_getSqlServerName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getSqlServerName.Location = new System.Drawing.Point(219, 134);
            this.button_getSqlServerName.Name = "button_getSqlServerName";
            this.button_getSqlServerName.Size = new System.Drawing.Size(202, 23);
            this.button_getSqlServerName.TabIndex = 3;
            this.button_getSqlServerName.Text = "获得邻近的SQL服务器名(&G)...";
            this.button_getSqlServerName.UseVisualStyleBackColor = true;
            this.button_getSqlServerName.Click += new System.EventHandler(this.button_getSqlServerName_Click);
            // 
            // textBox_instanceName
            // 
            this.textBox_instanceName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_instanceName.Location = new System.Drawing.Point(123, 322);
            this.textBox_instanceName.Name = "textBox_instanceName";
            this.textBox_instanceName.Size = new System.Drawing.Size(163, 21);
            this.textBox_instanceName.TabIndex = 6;
            this.textBox_instanceName.Text = "dp2kernel";
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 325);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 12);
            this.label4.TabIndex = 5;
            this.label4.Text = "内核实例名(&I):";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(347, 322);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 7;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(347, 351);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 8;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // textBox_message
            // 
            this.textBox_message.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_message.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_message.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_message.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_message.Location = new System.Drawing.Point(10, 10);
            this.textBox_message.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_message.Multiline = true;
            this.textBox_message.Name = "textBox_message";
            this.textBox_message.ReadOnly = true;
            this.textBox_message.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_message.Size = new System.Drawing.Size(414, 80);
            this.textBox_message.TabIndex = 0;
            // 
            // groupBox_login
            // 
            this.groupBox_login.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox_login.Controls.Add(this.textBox_confirmLoginPassword);
            this.groupBox_login.Controls.Add(this.label3);
            this.groupBox_login.Controls.Add(this.textBox_loginPassword);
            this.groupBox_login.Controls.Add(this.label2);
            this.groupBox_login.Controls.Add(this.textBox_loginName);
            this.groupBox_login.Controls.Add(this.label5);
            this.groupBox_login.Location = new System.Drawing.Point(12, 173);
            this.groupBox_login.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox_login.Name = "groupBox_login";
            this.groupBox_login.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox_login.Size = new System.Drawing.Size(411, 129);
            this.groupBox_login.TabIndex = 4;
            this.groupBox_login.TabStop = false;
            this.groupBox_login.Text = " 创建一个用于 dp2Kernel 的 SQL Server 登录名 ";
            // 
            // textBox_confirmLoginPassword
            // 
            this.textBox_confirmLoginPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_confirmLoginPassword.Location = new System.Drawing.Point(111, 88);
            this.textBox_confirmLoginPassword.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_confirmLoginPassword.Name = "textBox_confirmLoginPassword";
            this.textBox_confirmLoginPassword.PasswordChar = '*';
            this.textBox_confirmLoginPassword.Size = new System.Drawing.Size(163, 21);
            this.textBox_confirmLoginPassword.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
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
            this.textBox_loginPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_loginPassword.Location = new System.Drawing.Point(111, 63);
            this.textBox_loginPassword.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_loginPassword.Name = "textBox_loginPassword";
            this.textBox_loginPassword.PasswordChar = '*';
            this.textBox_loginPassword.Size = new System.Drawing.Size(163, 21);
            this.textBox_loginPassword.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
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
            this.textBox_loginName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_loginName.Location = new System.Drawing.Point(111, 26);
            this.textBox_loginName.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_loginName.Name = "textBox_loginName";
            this.textBox_loginName.Size = new System.Drawing.Size(163, 21);
            this.textBox_loginName.TabIndex = 1;
            this.textBox_loginName.Text = "dp2kernel";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(16, 29);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "登录名(&N):";
            // 
            // MsSqlServerDataSourceDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(432, 385);
            this.Controls.Add(this.groupBox_login);
            this.Controls.Add(this.textBox_message);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_instanceName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button_getSqlServerName);
            this.Controls.Add(this.textBox_sqlServerName);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MsSqlServerDataSourceDlg";
            this.ShowInTaskbar = false;
            this.Text = "MS SQL Server相关参数设置";
            this.Load += new System.EventHandler(this.DataSourceDlg_Load);
            this.groupBox_login.ResumeLayout(false);
            this.groupBox_login.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_sqlServerName;
        private System.Windows.Forms.Button button_getSqlServerName;
        private System.Windows.Forms.TextBox textBox_instanceName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.TextBox textBox_message;
        private System.Windows.Forms.GroupBox groupBox_login;
        private System.Windows.Forms.TextBox textBox_confirmLoginPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_loginPassword;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_loginName;
        private System.Windows.Forms.Label label5;
    }
}