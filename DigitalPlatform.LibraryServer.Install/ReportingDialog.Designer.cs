
namespace DigitalPlatform.LibraryServer
{
    partial class ReportingDialog
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_sqlServer = new System.Windows.Forms.TabPage();
            this.tabPage_dp2libraryServer = new System.Windows.Forms.TabPage();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_editMasterServer = new System.Windows.Forms.Button();
            this.textBox_masterServer = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBox_sslMode = new System.Windows.Forms.ComboBox();
            this.groupBox_login = new System.Windows.Forms.GroupBox();
            this.textBox_confirmLoginPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_loginPassword = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_loginName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_instanceName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_sqlServerName = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage_sqlServer.SuspendLayout();
            this.tabPage_dp2libraryServer.SuspendLayout();
            this.groupBox_login.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage_sqlServer);
            this.tabControl1.Controls.Add(this.tabPage_dp2libraryServer);
            this.tabControl1.Location = new System.Drawing.Point(13, 13);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(811, 631);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage_sqlServer
            // 
            this.tabPage_sqlServer.Controls.Add(this.label6);
            this.tabPage_sqlServer.Controls.Add(this.comboBox_sslMode);
            this.tabPage_sqlServer.Controls.Add(this.groupBox_login);
            this.tabPage_sqlServer.Controls.Add(this.textBox_instanceName);
            this.tabPage_sqlServer.Controls.Add(this.label1);
            this.tabPage_sqlServer.Controls.Add(this.textBox_sqlServerName);
            this.tabPage_sqlServer.Controls.Add(this.label7);
            this.tabPage_sqlServer.Location = new System.Drawing.Point(4, 31);
            this.tabPage_sqlServer.Name = "tabPage_sqlServer";
            this.tabPage_sqlServer.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_sqlServer.Size = new System.Drawing.Size(803, 596);
            this.tabPage_sqlServer.TabIndex = 0;
            this.tabPage_sqlServer.Text = "SQL 服务器";
            this.tabPage_sqlServer.UseVisualStyleBackColor = true;
            // 
            // tabPage_dp2libraryServer
            // 
            this.tabPage_dp2libraryServer.Controls.Add(this.button_editMasterServer);
            this.tabPage_dp2libraryServer.Controls.Add(this.textBox_masterServer);
            this.tabPage_dp2libraryServer.Controls.Add(this.label4);
            this.tabPage_dp2libraryServer.Location = new System.Drawing.Point(4, 31);
            this.tabPage_dp2libraryServer.Name = "tabPage_dp2libraryServer";
            this.tabPage_dp2libraryServer.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_dp2libraryServer.Size = new System.Drawing.Size(803, 596);
            this.tabPage_dp2libraryServer.TabIndex = 1;
            this.tabPage_dp2libraryServer.Text = "dp2library 服务器";
            this.tabPage_dp2libraryServer.UseVisualStyleBackColor = true;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(684, 655);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(138, 40);
            this.button_Cancel.TabIndex = 21;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(535, 655);
            this.button_OK.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(138, 40);
            this.button_OK.TabIndex = 20;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_editMasterServer
            // 
            this.button_editMasterServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_editMasterServer.Location = new System.Drawing.Point(697, 13);
            this.button_editMasterServer.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_editMasterServer.Name = "button_editMasterServer";
            this.button_editMasterServer.Size = new System.Drawing.Size(83, 40);
            this.button_editMasterServer.TabIndex = 12;
            this.button_editMasterServer.Text = "...";
            this.button_editMasterServer.UseVisualStyleBackColor = true;
            this.button_editMasterServer.Click += new System.EventHandler(this.button_editMasterServer_Click);
            // 
            // textBox_masterServer
            // 
            this.textBox_masterServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_masterServer.Location = new System.Drawing.Point(159, 20);
            this.textBox_masterServer.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_masterServer.Name = "textBox_masterServer";
            this.textBox_masterServer.ReadOnly = true;
            this.textBox_masterServer.Size = new System.Drawing.Size(526, 31);
            this.textBox_masterServer.TabIndex = 11;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 23);
            this.label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(138, 21);
            this.label4.TabIndex = 10;
            this.label4.Text = "主服务器(&M):";
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(24, 178);
            this.label6.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(140, 21);
            this.label6.TabIndex = 26;
            this.label6.Text = "SSL 模式(&M):";
            this.label6.Visible = false;
            // 
            // comboBox_sslMode
            // 
            this.comboBox_sslMode.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_sslMode.FormattingEnabled = true;
            this.comboBox_sslMode.Items.AddRange(new object[] {
            "Preferred",
            "None",
            "Required",
            "VerifyCA",
            "VerifyFull"});
            this.comboBox_sslMode.Location = new System.Drawing.Point(231, 175);
            this.comboBox_sslMode.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.comboBox_sslMode.Name = "comboBox_sslMode";
            this.comboBox_sslMode.Size = new System.Drawing.Size(545, 29);
            this.comboBox_sslMode.TabIndex = 25;
            this.comboBox_sslMode.Text = "Preferred";
            this.comboBox_sslMode.Visible = false;
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
            this.groupBox_login.Location = new System.Drawing.Point(28, 240);
            this.groupBox_login.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox_login.Name = "groupBox_login";
            this.groupBox_login.Padding = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.groupBox_login.Size = new System.Drawing.Size(753, 226);
            this.groupBox_login.TabIndex = 22;
            this.groupBox_login.TabStop = false;
            this.groupBox_login.Text = " 指定一个已有的 MySQL 帐户";
            // 
            // textBox_confirmLoginPassword
            // 
            this.textBox_confirmLoginPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_confirmLoginPassword.Location = new System.Drawing.Point(203, 154);
            this.textBox_confirmLoginPassword.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBox_confirmLoginPassword.Name = "textBox_confirmLoginPassword";
            this.textBox_confirmLoginPassword.PasswordChar = '*';
            this.textBox_confirmLoginPassword.Size = new System.Drawing.Size(295, 31);
            this.textBox_confirmLoginPassword.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(29, 157);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(147, 21);
            this.label3.TabIndex = 4;
            this.label3.Text = "再次输入密码:";
            // 
            // textBox_loginPassword
            // 
            this.textBox_loginPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_loginPassword.Location = new System.Drawing.Point(203, 110);
            this.textBox_loginPassword.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBox_loginPassword.Name = "textBox_loginPassword";
            this.textBox_loginPassword.PasswordChar = '*';
            this.textBox_loginPassword.Size = new System.Drawing.Size(295, 31);
            this.textBox_loginPassword.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(29, 115);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 21);
            this.label2.TabIndex = 2;
            this.label2.Text = "密码(&P):";
            // 
            // textBox_loginName
            // 
            this.textBox_loginName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_loginName.Location = new System.Drawing.Point(203, 45);
            this.textBox_loginName.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.textBox_loginName.Name = "textBox_loginName";
            this.textBox_loginName.Size = new System.Drawing.Size(295, 31);
            this.textBox_loginName.TabIndex = 1;
            this.textBox_loginName.Text = "root";
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(29, 51);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(117, 21);
            this.label5.TabIndex = 0;
            this.label5.Text = "用户名(&N):";
            // 
            // textBox_instanceName
            // 
            this.textBox_instanceName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_instanceName.Location = new System.Drawing.Point(231, 500);
            this.textBox_instanceName.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_instanceName.Name = "textBox_instanceName";
            this.textBox_instanceName.Size = new System.Drawing.Size(295, 31);
            this.textBox_instanceName.TabIndex = 24;
            this.textBox_instanceName.Text = "instance";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(24, 506);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(159, 21);
            this.label1.TabIndex = 23;
            this.label1.Text = "内核实例名(&I):";
            // 
            // textBox_sqlServerName
            // 
            this.textBox_sqlServerName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_sqlServerName.Location = new System.Drawing.Point(231, 66);
            this.textBox_sqlServerName.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_sqlServerName.Name = "textBox_sqlServerName";
            this.textBox_sqlServerName.Size = new System.Drawing.Size(547, 31);
            this.textBox_sqlServerName.TabIndex = 21;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(21, 70);
            this.label7.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(171, 21);
            this.label7.TabIndex = 20;
            this.label7.Text = "SQL服务器名(&S):";
            // 
            // ReportingDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(836, 709);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.tabControl1);
            this.Name = "ReportingDialog";
            this.Text = "ReportingDialog";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ReportingDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ReportingDialog_FormClosed);
            this.Load += new System.EventHandler(this.ReportingDialog_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_sqlServer.ResumeLayout(false);
            this.tabPage_sqlServer.PerformLayout();
            this.tabPage_dp2libraryServer.ResumeLayout(false);
            this.tabPage_dp2libraryServer.PerformLayout();
            this.groupBox_login.ResumeLayout(false);
            this.groupBox_login.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_sqlServer;
        private System.Windows.Forms.TabPage tabPage_dp2libraryServer;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_editMasterServer;
        private System.Windows.Forms.TextBox textBox_masterServer;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboBox_sslMode;
        private System.Windows.Forms.GroupBox groupBox_login;
        private System.Windows.Forms.TextBox textBox_confirmLoginPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_loginPassword;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_loginName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_instanceName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_sqlServerName;
        private System.Windows.Forms.Label label7;
    }
}