namespace TestReporting
{
    partial class Form1
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.MenuItem_file = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_test = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_buildPlan = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_exit = new System.Windows.Forms.ToolStripMenuItem();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_history = new System.Windows.Forms.TabPage();
            this.tabPage_config = new System.Windows.Forms.TabPage();
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
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.textBox_replicationStart = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.MenuItem_doPlan = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.tabControl_main.SuspendLayout();
            this.tabPage_history.SuspendLayout();
            this.tabPage_config.SuspendLayout();
            this.toolStrip_server.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_file,
            this.MenuItem_test});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 32);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Location = new System.Drawing.Point(0, 32);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(800, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Location = new System.Drawing.Point(0, 493);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(800, 22);
            this.statusStrip1.TabIndex = 2;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // MenuItem_file
            // 
            this.MenuItem_file.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_buildPlan,
            this.MenuItem_doPlan,
            this.MenuItem_exit});
            this.MenuItem_file.Name = "MenuItem_file";
            this.MenuItem_file.Size = new System.Drawing.Size(58, 28);
            this.MenuItem_file.Text = "文件";
            // 
            // MenuItem_test
            // 
            this.MenuItem_test.Name = "MenuItem_test";
            this.MenuItem_test.Size = new System.Drawing.Size(58, 28);
            this.MenuItem_test.Text = "测试";
            // 
            // MenuItem_buildPlan
            // 
            this.MenuItem_buildPlan.Name = "MenuItem_buildPlan";
            this.MenuItem_buildPlan.Size = new System.Drawing.Size(252, 30);
            this.MenuItem_buildPlan.Text = "创建同步计划";
            this.MenuItem_buildPlan.Click += new System.EventHandler(this.MenuItem_buildPlan_Click);
            // 
            // MenuItem_exit
            // 
            this.MenuItem_exit.Name = "MenuItem_exit";
            this.MenuItem_exit.Size = new System.Drawing.Size(252, 30);
            this.MenuItem_exit.Text = "退出";
            // 
            // tabControl_main
            // 
            this.tabControl_main.Controls.Add(this.tabPage_history);
            this.tabControl_main.Controls.Add(this.tabPage_config);
            this.tabControl_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_main.Location = new System.Drawing.Point(0, 57);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(800, 436);
            this.tabControl_main.TabIndex = 3;
            // 
            // tabPage_history
            // 
            this.tabPage_history.Controls.Add(this.webBrowser1);
            this.tabPage_history.Location = new System.Drawing.Point(4, 28);
            this.tabPage_history.Name = "tabPage_history";
            this.tabPage_history.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_history.Size = new System.Drawing.Size(792, 339);
            this.tabPage_history.TabIndex = 0;
            this.tabPage_history.Text = "操作历史";
            this.tabPage_history.UseVisualStyleBackColor = true;
            // 
            // tabPage_config
            // 
            this.tabPage_config.Controls.Add(this.textBox_replicationStart);
            this.tabPage_config.Controls.Add(this.label5);
            this.tabPage_config.Controls.Add(this.checkBox_cfg_savePasswordLong);
            this.tabPage_config.Controls.Add(this.textBox_cfg_location);
            this.tabPage_config.Controls.Add(this.label4);
            this.tabPage_config.Controls.Add(this.textBox_cfg_password);
            this.tabPage_config.Controls.Add(this.textBox_cfg_userName);
            this.tabPage_config.Controls.Add(this.label3);
            this.tabPage_config.Controls.Add(this.label2);
            this.tabPage_config.Controls.Add(this.textBox_cfg_dp2LibraryServerUrl);
            this.tabPage_config.Controls.Add(this.label1);
            this.tabPage_config.Controls.Add(this.toolStrip_server);
            this.tabPage_config.Location = new System.Drawing.Point(4, 28);
            this.tabPage_config.Name = "tabPage_config";
            this.tabPage_config.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_config.Size = new System.Drawing.Size(792, 404);
            this.tabPage_config.TabIndex = 1;
            this.tabPage_config.Text = "配置参数";
            this.tabPage_config.UseVisualStyleBackColor = true;
            // 
            // checkBox_cfg_savePasswordLong
            // 
            this.checkBox_cfg_savePasswordLong.AutoSize = true;
            this.checkBox_cfg_savePasswordLong.Location = new System.Drawing.Point(14, 305);
            this.checkBox_cfg_savePasswordLong.Margin = new System.Windows.Forms.Padding(5);
            this.checkBox_cfg_savePasswordLong.Name = "checkBox_cfg_savePasswordLong";
            this.checkBox_cfg_savePasswordLong.Size = new System.Drawing.Size(133, 22);
            this.checkBox_cfg_savePasswordLong.TabIndex = 19;
            this.checkBox_cfg_savePasswordLong.Text = "保存密码(&L)";
            // 
            // textBox_cfg_location
            // 
            this.textBox_cfg_location.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_cfg_location.Location = new System.Drawing.Point(190, 260);
            this.textBox_cfg_location.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_cfg_location.Name = "textBox_cfg_location";
            this.textBox_cfg_location.Size = new System.Drawing.Size(283, 28);
            this.textBox_cfg_location.TabIndex = 18;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 263);
            this.label4.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(125, 18);
            this.label4.TabIndex = 17;
            this.label4.Text = "工作台号(&W)：";
            // 
            // textBox_cfg_password
            // 
            this.textBox_cfg_password.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_cfg_password.Location = new System.Drawing.Point(190, 212);
            this.textBox_cfg_password.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_cfg_password.Name = "textBox_cfg_password";
            this.textBox_cfg_password.PasswordChar = '*';
            this.textBox_cfg_password.Size = new System.Drawing.Size(283, 28);
            this.textBox_cfg_password.TabIndex = 16;
            // 
            // textBox_cfg_userName
            // 
            this.textBox_cfg_userName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_cfg_userName.Location = new System.Drawing.Point(190, 164);
            this.textBox_cfg_userName.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_cfg_userName.Name = "textBox_cfg_userName";
            this.textBox_cfg_userName.Size = new System.Drawing.Size(283, 28);
            this.textBox_cfg_userName.TabIndex = 14;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 215);
            this.label3.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 18);
            this.label3.TabIndex = 15;
            this.label3.Text = "密码(&P)：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 167);
            this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 18);
            this.label2.TabIndex = 13;
            this.label2.Text = "用户名(&U)：";
            // 
            // textBox_cfg_dp2LibraryServerUrl
            // 
            this.textBox_cfg_dp2LibraryServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_cfg_dp2LibraryServerUrl.Location = new System.Drawing.Point(15, 44);
            this.textBox_cfg_dp2LibraryServerUrl.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_cfg_dp2LibraryServerUrl.Name = "textBox_cfg_dp2LibraryServerUrl";
            this.textBox_cfg_dp2LibraryServerUrl.Size = new System.Drawing.Size(767, 28);
            this.textBox_cfg_dp2LibraryServerUrl.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 12);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(206, 18);
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
            this.toolStrip_server.Location = new System.Drawing.Point(15, 84);
            this.toolStrip_server.Name = "toolStrip_server";
            this.toolStrip_server.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStrip_server.Size = new System.Drawing.Size(772, 51);
            this.toolStrip_server.TabIndex = 12;
            this.toolStrip_server.Text = "toolStrip1";
            // 
            // toolStripButton_cfg_setXeServer
            // 
            this.toolStripButton_cfg_setXeServer.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_cfg_setXeServer.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_cfg_setXeServer.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_cfg_setXeServer.Name = "toolStripButton_cfg_setXeServer";
            this.toolStripButton_cfg_setXeServer.Size = new System.Drawing.Size(122, 48);
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
            this.toolStripButton_cfg_setHongnibaServer.Size = new System.Drawing.Size(198, 48);
            this.toolStripButton_cfg_setHongnibaServer.Text = "红泥巴.数字平台服务器";
            this.toolStripButton_cfg_setHongnibaServer.ToolTipText = "设为红泥巴.数字平台服务器";
            this.toolStripButton_cfg_setHongnibaServer.Click += new System.EventHandler(this.toolStripButton_cfg_setHongnibaServer_Click);
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(3, 3);
            this.webBrowser1.Margin = new System.Windows.Forms.Padding(4);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(24, 27);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(786, 333);
            this.webBrowser1.TabIndex = 2;
            // 
            // textBox_replicationStart
            // 
            this.textBox_replicationStart.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_replicationStart.Location = new System.Drawing.Point(190, 353);
            this.textBox_replicationStart.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_replicationStart.Name = "textBox_replicationStart";
            this.textBox_replicationStart.Size = new System.Drawing.Size(283, 28);
            this.textBox_replicationStart.TabIndex = 21;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(10, 356);
            this.label5.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(152, 18);
            this.label5.TabIndex = 20;
            this.label5.Text = "日志同步起点(&R):";
            // 
            // MenuItem_doPlan
            // 
            this.MenuItem_doPlan.Name = "MenuItem_doPlan";
            this.MenuItem_doPlan.Size = new System.Drawing.Size(252, 30);
            this.MenuItem_doPlan.Text = "执行同步计划";
            this.MenuItem_doPlan.Click += new System.EventHandler(this.MenuItem_doPlan_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 515);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.Form1_FormClosed);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_history.ResumeLayout(false);
            this.tabPage_config.ResumeLayout(false);
            this.tabPage_config.PerformLayout();
            this.toolStrip_server.ResumeLayout(false);
            this.toolStrip_server.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_file;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_buildPlan;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_exit;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_history;
        private System.Windows.Forms.TabPage tabPage_config;
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
        private System.Windows.Forms.WebBrowser webBrowser1;
        public System.Windows.Forms.TextBox textBox_replicationStart;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_doPlan;
    }
}

