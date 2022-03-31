
namespace sipApiTester
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
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.MenuItem_file = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_settings = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_file_clearHtmlText = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_test = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_test_connect = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_test_connectAndLogin = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_test_loginSequence = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_test_loginConcurrent = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_test_loginConcurrentErrorPassword = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_test_scStatusConcurrent = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 62);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(800, 366);
            this.webBrowser1.TabIndex = 11;
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip1.Location = new System.Drawing.Point(0, 37);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(800, 25);
            this.toolStrip1.TabIndex = 10;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.statusStrip1.Location = new System.Drawing.Point(0, 428);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(800, 22);
            this.statusStrip1.TabIndex = 9;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // menuStrip1
            // 
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_file,
            this.MenuItem_test});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(800, 37);
            this.menuStrip1.TabIndex = 8;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // MenuItem_file
            // 
            this.MenuItem_file.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_settings,
            this.MenuItem_file_clearHtmlText});
            this.MenuItem_file.Name = "MenuItem_file";
            this.MenuItem_file.Size = new System.Drawing.Size(72, 33);
            this.MenuItem_file.Text = "文件";
            // 
            // MenuItem_settings
            // 
            this.MenuItem_settings.Name = "MenuItem_settings";
            this.MenuItem_settings.Size = new System.Drawing.Size(255, 40);
            this.MenuItem_settings.Text = "设置 ...";
            this.MenuItem_settings.Click += new System.EventHandler(this.MenuItem_settings_Click);
            // 
            // MenuItem_file_clearHtmlText
            // 
            this.MenuItem_file_clearHtmlText.Name = "MenuItem_file_clearHtmlText";
            this.MenuItem_file_clearHtmlText.Size = new System.Drawing.Size(255, 40);
            this.MenuItem_file_clearHtmlText.Text = "清除面板文字";
            this.MenuItem_file_clearHtmlText.Click += new System.EventHandler(this.MenuItem_file_clearHtmlText_Click);
            // 
            // MenuItem_test
            // 
            this.MenuItem_test.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_test_connect,
            this.MenuItem_test_connectAndLogin});
            this.MenuItem_test.Name = "MenuItem_test";
            this.MenuItem_test.Size = new System.Drawing.Size(72, 33);
            this.MenuItem_test.Text = "测试";
            // 
            // MenuItem_test_connect
            // 
            this.MenuItem_test_connect.Name = "MenuItem_test_connect";
            this.MenuItem_test_connect.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_test_connect.Text = "连接";
            this.MenuItem_test_connect.Click += new System.EventHandler(this.MenuItem_test_connect_Click);
            // 
            // MenuItem_test_connectAndLogin
            // 
            this.MenuItem_test_connectAndLogin.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_test_loginSequence,
            this.MenuItem_test_loginConcurrent,
            this.MenuItem_test_loginConcurrentErrorPassword,
            this.MenuItem_test_scStatusConcurrent});
            this.MenuItem_test_connectAndLogin.Name = "MenuItem_test_connectAndLogin";
            this.MenuItem_test_connectAndLogin.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_test_connectAndLogin.Text = "连接和登录";
            // 
            // MenuItem_test_loginSequence
            // 
            this.MenuItem_test_loginSequence.Name = "MenuItem_test_loginSequence";
            this.MenuItem_test_loginSequence.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_test_loginSequence.Text = "顺次";
            this.MenuItem_test_loginSequence.Click += new System.EventHandler(this.MenuItem_test_loginSequence_Click);
            // 
            // MenuItem_test_loginConcurrent
            // 
            this.MenuItem_test_loginConcurrent.Name = "MenuItem_test_loginConcurrent";
            this.MenuItem_test_loginConcurrent.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_test_loginConcurrent.Text = "并发";
            this.MenuItem_test_loginConcurrent.Click += new System.EventHandler(this.MenuItem_test_loginConcurrent_Click);
            // 
            // MenuItem_test_loginConcurrentErrorPassword
            // 
            this.MenuItem_test_loginConcurrentErrorPassword.Name = "MenuItem_test_loginConcurrentErrorPassword";
            this.MenuItem_test_loginConcurrentErrorPassword.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_test_loginConcurrentErrorPassword.Text = "并发+错误的密码";
            this.MenuItem_test_loginConcurrentErrorPassword.Click += new System.EventHandler(this.MenuItem_test_loginConcurrentErrorPassword_Click);
            // 
            // MenuItem_test_scStatusConcurrent
            // 
            this.MenuItem_test_scStatusConcurrent.Name = "MenuItem_test_scStatusConcurrent";
            this.MenuItem_test_scStatusConcurrent.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_test_scStatusConcurrent.Text = "并发 ScStatus";
            this.MenuItem_test_scStatusConcurrent.Click += new System.EventHandler(this.MenuItem_test_scStatusConcurrent_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Name = "MainForm";
            this.Text = "sipApiTester";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_file;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_settings;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test_connect;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test_connectAndLogin;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_file_clearHtmlText;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test_loginSequence;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test_loginConcurrent;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test_loginConcurrentErrorPassword;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test_scStatusConcurrent;
    }
}

