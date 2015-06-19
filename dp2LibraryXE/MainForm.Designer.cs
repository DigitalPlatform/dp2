namespace dp2LibraryXE
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
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.MenuItem_file = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_exit = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_management1 = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_installDp2Opac = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_updateDp2Opac = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_setupKernelDataDir = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_setupLibraryDataDir = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_setupOpacDataAppDir = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_openLibraryWsdl = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openKernelWsdl = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_updateDateDir = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_startIISExpress = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_iisExpressVersion = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_registerWebApp = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_test = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openDp2OPACHomePage = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_getSqllocaldbexePath = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_help = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openUserFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openDataFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openProgramFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_autoStartDp2Circulation = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_resetSerialCode = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_setListeningUrl = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator20 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_copyright = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolButton_stop = new System.Windows.Forms.ToolStripButton();
            this.toolStripDropDownButton_stopAll = new System.Windows.Forms.ToolStripDropDownButton();
            this.ToolStripMenuItem_stopAll = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip_main = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_main = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar_main = new System.Windows.Forms.ToolStripProgressBar();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.statusStrip_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_file,
            this.MenuItem_management1,
            this.MenuItem_help});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(372, 25);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // MenuItem_file
            // 
            this.MenuItem_file.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_exit});
            this.MenuItem_file.Name = "MenuItem_file";
            this.MenuItem_file.Size = new System.Drawing.Size(58, 21);
            this.MenuItem_file.Text = "文件(&F)";
            // 
            // MenuItem_exit
            // 
            this.MenuItem_exit.Name = "MenuItem_exit";
            this.MenuItem_exit.Size = new System.Drawing.Size(116, 22);
            this.MenuItem_exit.Text = "退出(&X)";
            this.MenuItem_exit.Click += new System.EventHandler(this.MenuItem_exit_Click);
            // 
            // MenuItem_management1
            // 
            this.MenuItem_management1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_installDp2Opac,
            this.MenuItem_updateDp2Opac,
            this.toolStripSeparator7,
            this.MenuItem_setupKernelDataDir,
            this.MenuItem_setupLibraryDataDir,
            this.MenuItem_setupOpacDataAppDir,
            this.toolStripSeparator2,
            this.MenuItem_openLibraryWsdl,
            this.MenuItem_openKernelWsdl,
            this.toolStripSeparator1,
            this.MenuItem_updateDateDir,
            this.toolStripSeparator5,
            this.MenuItem_startIISExpress,
            this.MenuItem_iisExpressVersion,
            this.MenuItem_registerWebApp,
            this.toolStripSeparator6,
            this.MenuItem_test,
            this.MenuItem_openDp2OPACHomePage,
            this.MenuItem_getSqllocaldbexePath});
            this.MenuItem_management1.Name = "MenuItem_management1";
            this.MenuItem_management1.Size = new System.Drawing.Size(64, 21);
            this.MenuItem_management1.Text = "维护(&M)";
            // 
            // MenuItem_installDp2Opac
            // 
            this.MenuItem_installDp2Opac.Name = "MenuItem_installDp2Opac";
            this.MenuItem_installDp2Opac.Size = new System.Drawing.Size(320, 22);
            this.MenuItem_installDp2Opac.Text = "安装 dp2OPAC";
            this.MenuItem_installDp2Opac.Click += new System.EventHandler(this.MenuItem_installDp2Opac_Click);
            // 
            // MenuItem_updateDp2Opac
            // 
            this.MenuItem_updateDp2Opac.Name = "MenuItem_updateDp2Opac";
            this.MenuItem_updateDp2Opac.Size = new System.Drawing.Size(320, 22);
            this.MenuItem_updateDp2Opac.Text = "升级 dp2OPAC";
            this.MenuItem_updateDp2Opac.Click += new System.EventHandler(this.MenuItem_updateDp2Opac_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(317, 6);
            // 
            // MenuItem_setupKernelDataDir
            // 
            this.MenuItem_setupKernelDataDir.Name = "MenuItem_setupKernelDataDir";
            this.MenuItem_setupKernelDataDir.Size = new System.Drawing.Size(320, 22);
            this.MenuItem_setupKernelDataDir.Text = "重新安装 dp2Kernel 数据目录";
            this.MenuItem_setupKernelDataDir.Click += new System.EventHandler(this.MenuItem_setupKernelDataDir_Click);
            // 
            // MenuItem_setupLibraryDataDir
            // 
            this.MenuItem_setupLibraryDataDir.Name = "MenuItem_setupLibraryDataDir";
            this.MenuItem_setupLibraryDataDir.Size = new System.Drawing.Size(320, 22);
            this.MenuItem_setupLibraryDataDir.Text = "重新安装 dp2Library 数据目录";
            this.MenuItem_setupLibraryDataDir.Click += new System.EventHandler(this.MenuItem_setupLibraryDataDir_Click);
            // 
            // MenuItem_setupOpacDataAppDir
            // 
            this.MenuItem_setupOpacDataAppDir.Name = "MenuItem_setupOpacDataAppDir";
            this.MenuItem_setupOpacDataAppDir.Size = new System.Drawing.Size(320, 22);
            this.MenuItem_setupOpacDataAppDir.Text = "重新安装 dp2OPAC 数据目录和应用程序目录";
            this.MenuItem_setupOpacDataAppDir.Click += new System.EventHandler(this.MenuItem_setupOpacDataAppDir_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(317, 6);
            // 
            // MenuItem_openLibraryWsdl
            // 
            this.MenuItem_openLibraryWsdl.Name = "MenuItem_openLibraryWsdl";
            this.MenuItem_openLibraryWsdl.Size = new System.Drawing.Size(320, 22);
            this.MenuItem_openLibraryWsdl.Text = "查看 dp2Library WSDL ...";
            this.MenuItem_openLibraryWsdl.Click += new System.EventHandler(this.MenuItem_openLibraryWsdl_Click);
            // 
            // MenuItem_openKernelWsdl
            // 
            this.MenuItem_openKernelWsdl.Name = "MenuItem_openKernelWsdl";
            this.MenuItem_openKernelWsdl.Size = new System.Drawing.Size(320, 22);
            this.MenuItem_openKernelWsdl.Text = "查看 dp2Kernel WSDL ...";
            this.MenuItem_openKernelWsdl.Click += new System.EventHandler(this.MenuItem_openKernelWsdl_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(317, 6);
            // 
            // MenuItem_updateDateDir
            // 
            this.MenuItem_updateDateDir.Name = "MenuItem_updateDateDir";
            this.MenuItem_updateDateDir.Size = new System.Drawing.Size(320, 22);
            this.MenuItem_updateDateDir.Text = "从安装包更新数据目录中的配置文件";
            this.MenuItem_updateDateDir.Click += new System.EventHandler(this.MenuItem_updateDataDir_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(317, 6);
            // 
            // MenuItem_startIISExpress
            // 
            this.MenuItem_startIISExpress.Name = "MenuItem_startIISExpress";
            this.MenuItem_startIISExpress.Size = new System.Drawing.Size(320, 22);
            this.MenuItem_startIISExpress.Text = "启动 IIS Express";
            this.MenuItem_startIISExpress.Click += new System.EventHandler(this.MenuItem_startIISExpress_Click);
            // 
            // MenuItem_iisExpressVersion
            // 
            this.MenuItem_iisExpressVersion.Name = "MenuItem_iisExpressVersion";
            this.MenuItem_iisExpressVersion.Size = new System.Drawing.Size(320, 22);
            this.MenuItem_iisExpressVersion.Text = "察看 IIS Express 版本";
            this.MenuItem_iisExpressVersion.Click += new System.EventHandler(this.MenuItem_iisExpressVersion_Click);
            // 
            // MenuItem_registerWebApp
            // 
            this.MenuItem_registerWebApp.Name = "MenuItem_registerWebApp";
            this.MenuItem_registerWebApp.Size = new System.Drawing.Size(320, 22);
            this.MenuItem_registerWebApp.Text = "注册 dp2OPAC 为 WebApp";
            this.MenuItem_registerWebApp.Click += new System.EventHandler(this.MenuItem_registerWebApp_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(317, 6);
            // 
            // MenuItem_test
            // 
            this.MenuItem_test.Name = "MenuItem_test";
            this.MenuItem_test.Size = new System.Drawing.Size(320, 22);
            this.MenuItem_test.Text = "test";
            this.MenuItem_test.Visible = false;
            this.MenuItem_test.Click += new System.EventHandler(this.MenuItem_test_Click);
            // 
            // MenuItem_openDp2OPACHomePage
            // 
            this.MenuItem_openDp2OPACHomePage.Name = "MenuItem_openDp2OPACHomePage";
            this.MenuItem_openDp2OPACHomePage.Size = new System.Drawing.Size(320, 22);
            this.MenuItem_openDp2OPACHomePage.Text = "打开浏览器访问 dp2OPAC";
            this.MenuItem_openDp2OPACHomePage.Click += new System.EventHandler(this.MenuItem_openDp2OPACHomePage_Click);
            // 
            // MenuItem_getSqllocaldbexePath
            // 
            this.MenuItem_getSqllocaldbexePath.Name = "MenuItem_getSqllocaldbexePath";
            this.MenuItem_getSqllocaldbexePath.Size = new System.Drawing.Size(320, 22);
            this.MenuItem_getSqllocaldbexePath.Text = "get sqllocaldb.exe path";
            this.MenuItem_getSqllocaldbexePath.Visible = false;
            this.MenuItem_getSqllocaldbexePath.Click += new System.EventHandler(this.MenuItem_getSqllocaldbexePath_Click);
            // 
            // MenuItem_help
            // 
            this.MenuItem_help.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_openUserFolder,
            this.MenuItem_openDataFolder,
            this.MenuItem_openProgramFolder,
            this.toolStripSeparator3,
            this.MenuItem_autoStartDp2Circulation,
            this.toolStripSeparator4,
            this.MenuItem_resetSerialCode,
            this.MenuItem_setListeningUrl,
            this.toolStripSeparator20,
            this.MenuItem_copyright});
            this.MenuItem_help.Name = "MenuItem_help";
            this.MenuItem_help.Size = new System.Drawing.Size(61, 21);
            this.MenuItem_help.Text = "帮助(&H)";
            // 
            // MenuItem_openUserFolder
            // 
            this.MenuItem_openUserFolder.Name = "MenuItem_openUserFolder";
            this.MenuItem_openUserFolder.Size = new System.Drawing.Size(212, 22);
            this.MenuItem_openUserFolder.Text = "打开用户文件夹(&U)";
            this.MenuItem_openUserFolder.Click += new System.EventHandler(this.MenuItem_openUserFolder_Click);
            // 
            // MenuItem_openDataFolder
            // 
            this.MenuItem_openDataFolder.Name = "MenuItem_openDataFolder";
            this.MenuItem_openDataFolder.Size = new System.Drawing.Size(212, 22);
            this.MenuItem_openDataFolder.Text = "打开数据文件夹(&D)";
            this.MenuItem_openDataFolder.Click += new System.EventHandler(this.MenuItem_openDataFolder_Click);
            // 
            // MenuItem_openProgramFolder
            // 
            this.MenuItem_openProgramFolder.Name = "MenuItem_openProgramFolder";
            this.MenuItem_openProgramFolder.Size = new System.Drawing.Size(212, 22);
            this.MenuItem_openProgramFolder.Text = "打开程序文件夹(&P)";
            this.MenuItem_openProgramFolder.Click += new System.EventHandler(this.MenuItem_openProgramFolder_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(209, 6);
            // 
            // MenuItem_autoStartDp2Circulation
            // 
            this.MenuItem_autoStartDp2Circulation.Checked = true;
            this.MenuItem_autoStartDp2Circulation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.MenuItem_autoStartDp2Circulation.Name = "MenuItem_autoStartDp2Circulation";
            this.MenuItem_autoStartDp2Circulation.Size = new System.Drawing.Size(212, 22);
            this.MenuItem_autoStartDp2Circulation.Text = "自动启动 dp2Circulation";
            this.MenuItem_autoStartDp2Circulation.Visible = false;
            this.MenuItem_autoStartDp2Circulation.Click += new System.EventHandler(this.MenuItem_autoStartDp2Circulation_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(209, 6);
            this.toolStripSeparator4.Visible = false;
            // 
            // MenuItem_resetSerialCode
            // 
            this.MenuItem_resetSerialCode.Name = "MenuItem_resetSerialCode";
            this.MenuItem_resetSerialCode.Size = new System.Drawing.Size(212, 22);
            this.MenuItem_resetSerialCode.Text = "设置序列号(&R) ...";
            this.MenuItem_resetSerialCode.Click += new System.EventHandler(this.MenuItem_resetSerialCode_Click);
            // 
            // MenuItem_setListeningUrl
            // 
            this.MenuItem_setListeningUrl.Name = "MenuItem_setListeningUrl";
            this.MenuItem_setListeningUrl.Size = new System.Drawing.Size(212, 22);
            this.MenuItem_setListeningUrl.Text = "设置监听 URL(&L) ...";
            this.MenuItem_setListeningUrl.Click += new System.EventHandler(this.MenuItem_setListeningUrl_Click);
            // 
            // toolStripSeparator20
            // 
            this.toolStripSeparator20.Name = "toolStripSeparator20";
            this.toolStripSeparator20.Size = new System.Drawing.Size(209, 6);
            this.toolStripSeparator20.Visible = false;
            // 
            // MenuItem_copyright
            // 
            this.MenuItem_copyright.Name = "MenuItem_copyright";
            this.MenuItem_copyright.Size = new System.Drawing.Size(212, 22);
            this.MenuItem_copyright.Text = "版权(&C)...";
            this.MenuItem_copyright.Visible = false;
            this.MenuItem_copyright.Click += new System.EventHandler(this.MenuItem_copyright_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolButton_stop,
            this.toolStripDropDownButton_stopAll});
            this.toolStrip1.Location = new System.Drawing.Point(0, 25);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(372, 25);
            this.toolStrip1.TabIndex = 4;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolButton_stop
            // 
            this.toolButton_stop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolButton_stop.Enabled = false;
            this.toolButton_stop.Image = ((System.Drawing.Image)(resources.GetObject("toolButton_stop.Image")));
            this.toolButton_stop.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolButton_stop.Name = "toolButton_stop";
            this.toolButton_stop.Size = new System.Drawing.Size(23, 22);
            this.toolButton_stop.Text = "停止";
            this.toolButton_stop.Click += new System.EventHandler(this.toolButton_stop_Click);
            // 
            // toolStripDropDownButton_stopAll
            // 
            this.toolStripDropDownButton_stopAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButton_stopAll.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_stopAll});
            this.toolStripDropDownButton_stopAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_stopAll.Name = "toolStripDropDownButton_stopAll";
            this.toolStripDropDownButton_stopAll.Size = new System.Drawing.Size(13, 22);
            this.toolStripDropDownButton_stopAll.Text = "停止全部";
            // 
            // ToolStripMenuItem_stopAll
            // 
            this.ToolStripMenuItem_stopAll.Name = "ToolStripMenuItem_stopAll";
            this.ToolStripMenuItem_stopAll.Size = new System.Drawing.Size(140, 22);
            this.ToolStripMenuItem_stopAll.Text = "停止全部(&A)";
            this.ToolStripMenuItem_stopAll.ToolTipText = "停止全部正在处理的操作";
            this.ToolStripMenuItem_stopAll.Click += new System.EventHandler(this.ToolStripMenuItem_stopAll_Click);
            // 
            // statusStrip_main
            // 
            this.statusStrip_main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_main,
            this.toolStripProgressBar_main});
            this.statusStrip_main.Location = new System.Drawing.Point(0, 279);
            this.statusStrip_main.Name = "statusStrip_main";
            this.statusStrip_main.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
            this.statusStrip_main.Size = new System.Drawing.Size(372, 22);
            this.statusStrip_main.TabIndex = 6;
            this.statusStrip_main.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_main
            // 
            this.toolStripStatusLabel_main.AutoSize = false;
            this.toolStripStatusLabel_main.Name = "toolStripStatusLabel_main";
            this.toolStripStatusLabel_main.Size = new System.Drawing.Size(255, 17);
            this.toolStripStatusLabel_main.Spring = true;
            // 
            // toolStripProgressBar_main
            // 
            this.toolStripProgressBar_main.AutoSize = false;
            this.toolStripProgressBar_main.BackColor = System.Drawing.SystemColors.Control;
            this.toolStripProgressBar_main.Name = "toolStripProgressBar_main";
            this.toolStripProgressBar_main.Size = new System.Drawing.Size(100, 16);
            this.toolStripProgressBar_main.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 50);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(372, 229);
            this.webBrowser1.TabIndex = 7;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(372, 301);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.statusStrip_main);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.ForeColor = System.Drawing.SystemColors.WindowText;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "dp2Library XE";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip_main.ResumeLayout(false);
            this.statusStrip_main.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_management1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_setupKernelDataDir;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_setupLibraryDataDir;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openKernelWsdl;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openLibraryWsdl;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolButton_stop;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_stopAll;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_stopAll;
        private System.Windows.Forms.StatusStrip statusStrip_main;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_main;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar_main;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_help;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openUserFolder;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openDataFolder;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openProgramFolder;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator20;
        internal System.Windows.Forms.ToolStripMenuItem MenuItem_copyright;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_resetSerialCode;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_autoStartDp2Circulation;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_setListeningUrl;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_updateDateDir;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_setupOpacDataAppDir;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_startIISExpress;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_registerWebApp;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_iisExpressVersion;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_installDp2Opac;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_file;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_exit;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_test;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openDp2OPACHomePage;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_updateDp2Opac;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_getSqllocaldbexePath;
    }
}

