namespace DigitalPlatform.CirculationClient
{
    partial class ServerDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ServerDlg));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.checkBox_savePassword = new System.Windows.Forms.CheckBox();
            this.textBox_password = new System.Windows.Forms.TextBox();
            this.textBox_userName = new System.Windows.Forms.TextBox();
            this.textBox_serverAddr = new System.Windows.Forms.TextBox();
            this.button_cancel = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_serverName = new System.Windows.Forms.TextBox();
            this.textBox_comment = new System.Windows.Forms.TextBox();
            this.toolStrip_server = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_server_setXeServer = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_server_setHongnibaServer = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_enableMultiLine = new System.Windows.Forms.ToolStripButton();
            this.toolStrip_server.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Location = new System.Drawing.Point(0, 422);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(2, 0, 15, 0);
            this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusStrip1.Size = new System.Drawing.Size(600, 22);
            this.statusStrip1.TabIndex = 21;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // checkBox_savePassword
            // 
            this.checkBox_savePassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_savePassword.Location = new System.Drawing.Point(180, 363);
            this.checkBox_savePassword.Name = "checkBox_savePassword";
            this.checkBox_savePassword.Size = new System.Drawing.Size(234, 30);
            this.checkBox_savePassword.TabIndex = 18;
            this.checkBox_savePassword.Text = "保存密码(&S)";
            // 
            // textBox_password
            // 
            this.textBox_password.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_password.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_password.Location = new System.Drawing.Point(180, 321);
            this.textBox_password.Name = "textBox_password";
            this.textBox_password.PasswordChar = '*';
            this.textBox_password.Size = new System.Drawing.Size(234, 28);
            this.textBox_password.TabIndex = 17;
            // 
            // textBox_userName
            // 
            this.textBox_userName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_userName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_userName.Location = new System.Drawing.Point(180, 279);
            this.textBox_userName.Name = "textBox_userName";
            this.textBox_userName.Size = new System.Drawing.Size(234, 28);
            this.textBox_userName.TabIndex = 15;
            // 
            // textBox_serverAddr
            // 
            this.textBox_serverAddr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_serverAddr.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_serverAddr.Location = new System.Drawing.Point(14, 198);
            this.textBox_serverAddr.Name = "textBox_serverAddr";
            this.textBox_serverAddr.Size = new System.Drawing.Size(574, 28);
            this.textBox_serverAddr.TabIndex = 13;
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(471, 356);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(117, 36);
            this.button_cancel.TabIndex = 20;
            this.button_cancel.Text = "取消";
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 326);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 18);
            this.label3.TabIndex = 16;
            this.label3.Text = "密码(&P)：";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 284);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 18);
            this.label2.TabIndex = 14;
            this.label2.Text = "用户名(&U)：";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 176);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(134, 18);
            this.label1.TabIndex = 12;
            this.label1.Text = "服务器地址(&H):";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(471, 314);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(117, 34);
            this.button_OK.TabIndex = 19;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 108);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(116, 18);
            this.label4.TabIndex = 22;
            this.label4.Text = "服务器名(&N):";
            // 
            // textBox_serverName
            // 
            this.textBox_serverName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_serverName.Location = new System.Drawing.Point(14, 129);
            this.textBox_serverName.Name = "textBox_serverName";
            this.textBox_serverName.Size = new System.Drawing.Size(574, 28);
            this.textBox_serverName.TabIndex = 23;
            // 
            // textBox_comment
            // 
            this.textBox_comment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_comment.Location = new System.Drawing.Point(14, 15);
            this.textBox_comment.Multiline = true;
            this.textBox_comment.Name = "textBox_comment";
            this.textBox_comment.ReadOnly = true;
            this.textBox_comment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_comment.Size = new System.Drawing.Size(574, 79);
            this.textBox_comment.TabIndex = 24;
            // 
            // toolStrip_server
            // 
            this.toolStrip_server.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip_server.AutoSize = false;
            this.toolStrip_server.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip_server.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_server.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip_server.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_server_setXeServer,
            this.toolStripSeparator1,
            this.toolStripButton_server_setHongnibaServer,
            this.toolStripButton_enableMultiLine});
            this.toolStrip_server.Location = new System.Drawing.Point(14, 228);
            this.toolStrip_server.Name = "toolStrip_server";
            this.toolStrip_server.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStrip_server.Size = new System.Drawing.Size(573, 38);
            this.toolStrip_server.TabIndex = 25;
            this.toolStrip_server.Text = "toolStrip1";
            // 
            // toolStripButton_server_setXeServer
            // 
            this.toolStripButton_server_setXeServer.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_server_setXeServer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_server_setXeServer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_server_setXeServer.Name = "toolStripButton_server_setXeServer";
            this.toolStripButton_server_setXeServer.Size = new System.Drawing.Size(122, 35);
            this.toolStripButton_server_setXeServer.Text = "单机版服务器";
            this.toolStripButton_server_setXeServer.ToolTipText = "设为单机版服务器";
            this.toolStripButton_server_setXeServer.Click += new System.EventHandler(this.toolStripButton_server_setXeServer_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton_server_setHongnibaServer
            // 
            this.toolStripButton_server_setHongnibaServer.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_server_setHongnibaServer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_server_setHongnibaServer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_server_setHongnibaServer.Name = "toolStripButton_server_setHongnibaServer";
            this.toolStripButton_server_setHongnibaServer.Size = new System.Drawing.Size(198, 35);
            this.toolStripButton_server_setHongnibaServer.Text = "红泥巴.数字平台服务器";
            this.toolStripButton_server_setHongnibaServer.ToolTipText = "设为红泥巴.数字平台服务器";
            this.toolStripButton_server_setHongnibaServer.Click += new System.EventHandler(this.toolStripButton_server_setHongnibaServer_Click);
            // 
            // toolStripButton_enableMultiLine
            // 
            this.toolStripButton_enableMultiLine.CheckOnClick = true;
            this.toolStripButton_enableMultiLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_enableMultiLine.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_enableMultiLine.Image")));
            this.toolStripButton_enableMultiLine.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_enableMultiLine.Name = "toolStripButton_enableMultiLine";
            this.toolStripButton_enableMultiLine.Size = new System.Drawing.Size(50, 35);
            this.toolStripButton_enableMultiLine.Text = "多行";
            this.toolStripButton_enableMultiLine.ToolTipText = "允许在“服务器地址”中输入多行文本";
            this.toolStripButton_enableMultiLine.CheckedChanged += new System.EventHandler(this.toolStripButton_enableMultiLine_CheckedChanged);
            // 
            // ServerDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(600, 444);
            this.Controls.Add(this.textBox_comment);
            this.Controls.Add(this.textBox_serverName);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.checkBox_savePassword);
            this.Controls.Add(this.textBox_password);
            this.Controls.Add(this.textBox_userName);
            this.Controls.Add(this.textBox_serverAddr);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.toolStrip_server);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ServerDlg";
            this.ShowInTaskbar = false;
            this.Text = "服务器";
            this.toolStrip_server.ResumeLayout(false);
            this.toolStrip_server.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        public System.Windows.Forms.CheckBox checkBox_savePassword;
        public System.Windows.Forms.TextBox textBox_password;
        public System.Windows.Forms.TextBox textBox_userName;
        public System.Windows.Forms.TextBox textBox_serverAddr;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_serverName;
        public System.Windows.Forms.TextBox textBox_comment;
        private System.Windows.Forms.ToolStrip toolStrip_server;
        private System.Windows.Forms.ToolStripButton toolStripButton_server_setXeServer;
        private System.Windows.Forms.ToolStripButton toolStripButton_server_setHongnibaServer;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_enableMultiLine;
    }
}