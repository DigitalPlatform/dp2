﻿namespace dp2Installer
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

            if (this.Channel != null)
                this.Channel.Dispose();

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
            this.MenuItem_autoUpgrade = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_displayDigitalPlatformEventLog = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_sendDebugInfos = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_exit = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2Kernel = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2kernel_install = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2kernel_upgrade = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2kernel_openDataDir = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2kernel_openAppDir = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator9 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2kernel_instanceManagement = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator10 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2kernel_tools = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2kernel_startService = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2kernel_stopService = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator11 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2kernel_tools_installService = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2kernel_tools_uninstallService = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator14 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2kernel_uninstall = new System.Windows.Forms.ToolStripMenuItem();
            this.dp2LibraryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2library_install = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2library_upgrade = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2library_openDataDir = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2library_openAppDir = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2library_instanceManagement = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2library_upgradeCfgs = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2library_verifySerialNumbers = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator13 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2library_tools = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2library_startService = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2library_stopService = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator12 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2library_installService = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2library_uninstallService = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator15 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2library_setupMongoDB = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator18 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2library_enableMsmq = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator16 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2library_uninstall = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2OPAC = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2opac_install = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2opac_upgrade = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2opac_openDataDir = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2opac_openVirtualDir = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator17 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_tools_enableIIS = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2zserver = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2ZServer_install = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2ZServer_upgrade = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator19 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2ZServer_openDataDir = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2ZServer_openAppDir = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator20 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2ZServer_instanceManagement = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator21 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2ZServer_upgradeCfgs = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2lZServer_verifySerialNumbers = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator22 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2ZServer_tools = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2ZServer_startService = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2ZServer_stopService = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator23 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2ZServer_installService = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2ZServer_uninstallService = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator26 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2ZServer_uninstall = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_help = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openUserFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openDataFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openProgramFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItem_uninstallDp2zserver = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator24 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_copyright = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip_main = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_main = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripProgressBar_main = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolButton_stop = new System.Windows.Forms.ToolStripButton();
            this.toolStripDropDownButton_stopAll = new System.Windows.Forms.ToolStripDropDownButton();
            this.ToolStripMenuItem_stopAll = new System.Windows.Forms.ToolStripMenuItem();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.ToolStripMenuItem_getMD5ofFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator25 = new System.Windows.Forms.ToolStripSeparator();
            this.menuStrip1.SuspendLayout();
            this.statusStrip_main.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_file,
            this.MenuItem_dp2Kernel,
            this.dp2LibraryToolStripMenuItem,
            this.MenuItem_dp2OPAC,
            this.MenuItem_dp2zserver,
            this.MenuItem_help});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(9, 3, 0, 3);
            this.menuStrip1.Size = new System.Drawing.Size(867, 34);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // MenuItem_file
            // 
            this.MenuItem_file.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_autoUpgrade,
            this.toolStripSeparator4,
            this.MenuItem_displayDigitalPlatformEventLog,
            this.MenuItem_sendDebugInfos,
            this.toolStripSeparator8,
            this.MenuItem_exit});
            this.MenuItem_file.Name = "MenuItem_file";
            this.MenuItem_file.Size = new System.Drawing.Size(80, 28);
            this.MenuItem_file.Text = "文件(&F)";
            // 
            // MenuItem_autoUpgrade
            // 
            this.MenuItem_autoUpgrade.Name = "MenuItem_autoUpgrade";
            this.MenuItem_autoUpgrade.Size = new System.Drawing.Size(342, 30);
            this.MenuItem_autoUpgrade.Text = "自动升级全部产品(&A)";
            this.MenuItem_autoUpgrade.Click += new System.EventHandler(this.MenuItem_autoUpgrade_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(339, 6);
            // 
            // MenuItem_displayDigitalPlatformEventLog
            // 
            this.MenuItem_displayDigitalPlatformEventLog.Name = "MenuItem_displayDigitalPlatformEventLog";
            this.MenuItem_displayDigitalPlatformEventLog.Size = new System.Drawing.Size(342, 30);
            this.MenuItem_displayDigitalPlatformEventLog.Text = "显示 DigitalPlatform 事件日志";
            this.MenuItem_displayDigitalPlatformEventLog.Click += new System.EventHandler(this.MenuItem_displayDigitalPlatformEventLog_Click);
            // 
            // MenuItem_sendDebugInfos
            // 
            this.MenuItem_sendDebugInfos.Name = "MenuItem_sendDebugInfos";
            this.MenuItem_sendDebugInfos.Size = new System.Drawing.Size(342, 30);
            this.MenuItem_sendDebugInfos.Text = "打包事件日志信息(&S)";
            this.MenuItem_sendDebugInfos.Click += new System.EventHandler(this.MenuItem_zipDebugInfos_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(339, 6);
            // 
            // MenuItem_exit
            // 
            this.MenuItem_exit.Name = "MenuItem_exit";
            this.MenuItem_exit.Size = new System.Drawing.Size(342, 30);
            this.MenuItem_exit.Text = "退出(&X)";
            this.MenuItem_exit.Click += new System.EventHandler(this.MenuItem_exit_Click);
            // 
            // MenuItem_dp2Kernel
            // 
            this.MenuItem_dp2Kernel.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_dp2kernel_install,
            this.MenuItem_dp2kernel_upgrade,
            this.toolStripSeparator6,
            this.MenuItem_dp2kernel_openDataDir,
            this.MenuItem_dp2kernel_openAppDir,
            this.toolStripSeparator9,
            this.MenuItem_dp2kernel_instanceManagement,
            this.toolStripSeparator10,
            this.MenuItem_dp2kernel_tools});
            this.MenuItem_dp2Kernel.Name = "MenuItem_dp2Kernel";
            this.MenuItem_dp2Kernel.Size = new System.Drawing.Size(111, 28);
            this.MenuItem_dp2Kernel.Text = "dp2Kernel";
            // 
            // MenuItem_dp2kernel_install
            // 
            this.MenuItem_dp2kernel_install.Name = "MenuItem_dp2kernel_install";
            this.MenuItem_dp2kernel_install.Size = new System.Drawing.Size(252, 30);
            this.MenuItem_dp2kernel_install.Text = "安装 dp2Kernel";
            this.MenuItem_dp2kernel_install.Click += new System.EventHandler(this.MenuItem_dp2kernel_install_Click);
            // 
            // MenuItem_dp2kernel_upgrade
            // 
            this.MenuItem_dp2kernel_upgrade.Name = "MenuItem_dp2kernel_upgrade";
            this.MenuItem_dp2kernel_upgrade.Size = new System.Drawing.Size(252, 30);
            this.MenuItem_dp2kernel_upgrade.Text = "升级 dp2Kernel";
            this.MenuItem_dp2kernel_upgrade.Click += new System.EventHandler(this.MenuItem_dp2kernel_upgrade_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(249, 6);
            // 
            // MenuItem_dp2kernel_openDataDir
            // 
            this.MenuItem_dp2kernel_openDataDir.Name = "MenuItem_dp2kernel_openDataDir";
            this.MenuItem_dp2kernel_openDataDir.Size = new System.Drawing.Size(252, 30);
            this.MenuItem_dp2kernel_openDataDir.Text = "打开数据文件夹";
            // 
            // MenuItem_dp2kernel_openAppDir
            // 
            this.MenuItem_dp2kernel_openAppDir.Name = "MenuItem_dp2kernel_openAppDir";
            this.MenuItem_dp2kernel_openAppDir.Size = new System.Drawing.Size(252, 30);
            this.MenuItem_dp2kernel_openAppDir.Text = "打开程序文件夹";
            this.MenuItem_dp2kernel_openAppDir.Click += new System.EventHandler(this.MenuItem_dp2kernel_openAppDir_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(249, 6);
            // 
            // MenuItem_dp2kernel_instanceManagement
            // 
            this.MenuItem_dp2kernel_instanceManagement.Name = "MenuItem_dp2kernel_instanceManagement";
            this.MenuItem_dp2kernel_instanceManagement.Size = new System.Drawing.Size(252, 30);
            this.MenuItem_dp2kernel_instanceManagement.Text = "配置实例";
            this.MenuItem_dp2kernel_instanceManagement.Click += new System.EventHandler(this.MenuItem_dp2kernel_instanceManagement_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(249, 6);
            // 
            // MenuItem_dp2kernel_tools
            // 
            this.MenuItem_dp2kernel_tools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_dp2kernel_startService,
            this.MenuItem_dp2kernel_stopService,
            this.toolStripSeparator11,
            this.MenuItem_dp2kernel_tools_installService,
            this.MenuItem_dp2kernel_tools_uninstallService,
            this.toolStripSeparator14,
            this.MenuItem_dp2kernel_uninstall});
            this.MenuItem_dp2kernel_tools.Name = "MenuItem_dp2kernel_tools";
            this.MenuItem_dp2kernel_tools.Size = new System.Drawing.Size(252, 30);
            this.MenuItem_dp2kernel_tools.Text = "工具";
            // 
            // MenuItem_dp2kernel_startService
            // 
            this.MenuItem_dp2kernel_startService.Name = "MenuItem_dp2kernel_startService";
            this.MenuItem_dp2kernel_startService.Size = new System.Drawing.Size(277, 30);
            this.MenuItem_dp2kernel_startService.Text = "启动 Windows Service";
            this.MenuItem_dp2kernel_startService.Click += new System.EventHandler(this.MenuItem_dp2kernel_startService_Click);
            // 
            // MenuItem_dp2kernel_stopService
            // 
            this.MenuItem_dp2kernel_stopService.Name = "MenuItem_dp2kernel_stopService";
            this.MenuItem_dp2kernel_stopService.Size = new System.Drawing.Size(277, 30);
            this.MenuItem_dp2kernel_stopService.Text = "停止 Windows Service";
            this.MenuItem_dp2kernel_stopService.Click += new System.EventHandler(this.MenuItem_dp2kernel_stopService_Click);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(274, 6);
            // 
            // MenuItem_dp2kernel_tools_installService
            // 
            this.MenuItem_dp2kernel_tools_installService.Name = "MenuItem_dp2kernel_tools_installService";
            this.MenuItem_dp2kernel_tools_installService.Size = new System.Drawing.Size(277, 30);
            this.MenuItem_dp2kernel_tools_installService.Text = "注册 Windows Service";
            this.MenuItem_dp2kernel_tools_installService.Click += new System.EventHandler(this.MenuItem_dp2kernel_installService_Click);
            // 
            // MenuItem_dp2kernel_tools_uninstallService
            // 
            this.MenuItem_dp2kernel_tools_uninstallService.Name = "MenuItem_dp2kernel_tools_uninstallService";
            this.MenuItem_dp2kernel_tools_uninstallService.Size = new System.Drawing.Size(277, 30);
            this.MenuItem_dp2kernel_tools_uninstallService.Text = "注销 Windows Service";
            this.MenuItem_dp2kernel_tools_uninstallService.Click += new System.EventHandler(this.MenuItem_dp2kernel_uninstallService_Click);
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(274, 6);
            // 
            // MenuItem_dp2kernel_uninstall
            // 
            this.MenuItem_dp2kernel_uninstall.Name = "MenuItem_dp2kernel_uninstall";
            this.MenuItem_dp2kernel_uninstall.Size = new System.Drawing.Size(277, 30);
            this.MenuItem_dp2kernel_uninstall.Text = "卸载 dp2Kernel";
            this.MenuItem_dp2kernel_uninstall.Click += new System.EventHandler(this.MenuItem_dp2kernel_uninstall_Click);
            // 
            // dp2LibraryToolStripMenuItem
            // 
            this.dp2LibraryToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_dp2library_install,
            this.MenuItem_dp2library_upgrade,
            this.toolStripSeparator5,
            this.MenuItem_dp2library_openDataDir,
            this.MenuItem_dp2library_openAppDir,
            this.toolStripSeparator2,
            this.MenuItem_dp2library_instanceManagement,
            this.toolStripSeparator1,
            this.MenuItem_dp2library_upgradeCfgs,
            this.MenuItem_dp2library_verifySerialNumbers,
            this.toolStripSeparator13,
            this.MenuItem_dp2library_tools});
            this.dp2LibraryToolStripMenuItem.Name = "dp2LibraryToolStripMenuItem";
            this.dp2LibraryToolStripMenuItem.Size = new System.Drawing.Size(117, 28);
            this.dp2LibraryToolStripMenuItem.Text = "dp2Library";
            // 
            // MenuItem_dp2library_install
            // 
            this.MenuItem_dp2library_install.Name = "MenuItem_dp2library_install";
            this.MenuItem_dp2library_install.Size = new System.Drawing.Size(290, 30);
            this.MenuItem_dp2library_install.Text = "安装 dp2Library";
            this.MenuItem_dp2library_install.Click += new System.EventHandler(this.MenuItem_dp2library_install_Click);
            // 
            // MenuItem_dp2library_upgrade
            // 
            this.MenuItem_dp2library_upgrade.Name = "MenuItem_dp2library_upgrade";
            this.MenuItem_dp2library_upgrade.Size = new System.Drawing.Size(290, 30);
            this.MenuItem_dp2library_upgrade.Text = "升级 dp2Library";
            this.MenuItem_dp2library_upgrade.Click += new System.EventHandler(this.MenuItem_dp2library_upgrade_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(287, 6);
            // 
            // MenuItem_dp2library_openDataDir
            // 
            this.MenuItem_dp2library_openDataDir.Name = "MenuItem_dp2library_openDataDir";
            this.MenuItem_dp2library_openDataDir.Size = new System.Drawing.Size(290, 30);
            this.MenuItem_dp2library_openDataDir.Text = "打开数据文件夹";
            // 
            // MenuItem_dp2library_openAppDir
            // 
            this.MenuItem_dp2library_openAppDir.Name = "MenuItem_dp2library_openAppDir";
            this.MenuItem_dp2library_openAppDir.Size = new System.Drawing.Size(290, 30);
            this.MenuItem_dp2library_openAppDir.Text = "打开程序文件夹";
            this.MenuItem_dp2library_openAppDir.Click += new System.EventHandler(this.MenuItem_dp2library_openAppDir_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(287, 6);
            // 
            // MenuItem_dp2library_instanceManagement
            // 
            this.MenuItem_dp2library_instanceManagement.Name = "MenuItem_dp2library_instanceManagement";
            this.MenuItem_dp2library_instanceManagement.Size = new System.Drawing.Size(290, 30);
            this.MenuItem_dp2library_instanceManagement.Text = "配置实例";
            this.MenuItem_dp2library_instanceManagement.Click += new System.EventHandler(this.MenuItem_dp2library_instanceManagement_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(287, 6);
            // 
            // MenuItem_dp2library_upgradeCfgs
            // 
            this.MenuItem_dp2library_upgradeCfgs.Name = "MenuItem_dp2library_upgradeCfgs";
            this.MenuItem_dp2library_upgradeCfgs.Size = new System.Drawing.Size(290, 30);
            this.MenuItem_dp2library_upgradeCfgs.Text = "更新数据目录的配置文件";
            this.MenuItem_dp2library_upgradeCfgs.Click += new System.EventHandler(this.MenuItem_dp2library_upgradeCfgs_Click);
            // 
            // MenuItem_dp2library_verifySerialNumbers
            // 
            this.MenuItem_dp2library_verifySerialNumbers.Name = "MenuItem_dp2library_verifySerialNumbers";
            this.MenuItem_dp2library_verifySerialNumbers.Size = new System.Drawing.Size(290, 30);
            this.MenuItem_dp2library_verifySerialNumbers.Text = "检查序列号(&S)";
            this.MenuItem_dp2library_verifySerialNumbers.Click += new System.EventHandler(this.MenuItem_dp2library_verifySerialNumbers_Click);
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(287, 6);
            // 
            // MenuItem_dp2library_tools
            // 
            this.MenuItem_dp2library_tools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_dp2library_startService,
            this.MenuItem_dp2library_stopService,
            this.toolStripSeparator12,
            this.MenuItem_dp2library_installService,
            this.MenuItem_dp2library_uninstallService,
            this.toolStripSeparator15,
            this.MenuItem_dp2library_setupMongoDB,
            this.toolStripSeparator18,
            this.MenuItem_dp2library_enableMsmq,
            this.toolStripSeparator16,
            this.MenuItem_dp2library_uninstall});
            this.MenuItem_dp2library_tools.Name = "MenuItem_dp2library_tools";
            this.MenuItem_dp2library_tools.Size = new System.Drawing.Size(290, 30);
            this.MenuItem_dp2library_tools.Text = "工具";
            // 
            // MenuItem_dp2library_startService
            // 
            this.MenuItem_dp2library_startService.Name = "MenuItem_dp2library_startService";
            this.MenuItem_dp2library_startService.Size = new System.Drawing.Size(277, 30);
            this.MenuItem_dp2library_startService.Text = "启动 Windows Service";
            this.MenuItem_dp2library_startService.Click += new System.EventHandler(this.MenuItem_dp2library_startService_Click);
            // 
            // MenuItem_dp2library_stopService
            // 
            this.MenuItem_dp2library_stopService.Name = "MenuItem_dp2library_stopService";
            this.MenuItem_dp2library_stopService.Size = new System.Drawing.Size(277, 30);
            this.MenuItem_dp2library_stopService.Text = "停止 Windows Service";
            this.MenuItem_dp2library_stopService.Click += new System.EventHandler(this.MenuItem_dp2library_stopService_Click);
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(274, 6);
            // 
            // MenuItem_dp2library_installService
            // 
            this.MenuItem_dp2library_installService.Name = "MenuItem_dp2library_installService";
            this.MenuItem_dp2library_installService.Size = new System.Drawing.Size(277, 30);
            this.MenuItem_dp2library_installService.Text = "注册 Windows Service";
            this.MenuItem_dp2library_installService.Click += new System.EventHandler(this.MenuItem_dp2library_installService_Click);
            // 
            // MenuItem_dp2library_uninstallService
            // 
            this.MenuItem_dp2library_uninstallService.Name = "MenuItem_dp2library_uninstallService";
            this.MenuItem_dp2library_uninstallService.Size = new System.Drawing.Size(277, 30);
            this.MenuItem_dp2library_uninstallService.Text = "注销 Windows Service";
            this.MenuItem_dp2library_uninstallService.Click += new System.EventHandler(this.MenuItem_dp2library_uninstallService_Click);
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            this.toolStripSeparator15.Size = new System.Drawing.Size(274, 6);
            // 
            // MenuItem_dp2library_setupMongoDB
            // 
            this.MenuItem_dp2library_setupMongoDB.Name = "MenuItem_dp2library_setupMongoDB";
            this.MenuItem_dp2library_setupMongoDB.Size = new System.Drawing.Size(277, 30);
            this.MenuItem_dp2library_setupMongoDB.Text = "安装 MongoDB ...";
            this.MenuItem_dp2library_setupMongoDB.Click += new System.EventHandler(this.MenuItem_dp2library_setupMongoDB_Click);
            // 
            // toolStripSeparator18
            // 
            this.toolStripSeparator18.Name = "toolStripSeparator18";
            this.toolStripSeparator18.Size = new System.Drawing.Size(274, 6);
            // 
            // MenuItem_dp2library_enableMsmq
            // 
            this.MenuItem_dp2library_enableMsmq.Name = "MenuItem_dp2library_enableMsmq";
            this.MenuItem_dp2library_enableMsmq.Size = new System.Drawing.Size(277, 30);
            this.MenuItem_dp2library_enableMsmq.Text = "启用 MSMQ";
            this.MenuItem_dp2library_enableMsmq.Click += new System.EventHandler(this.MenuItem_dp2library_enableMsmq_Click);
            // 
            // toolStripSeparator16
            // 
            this.toolStripSeparator16.Name = "toolStripSeparator16";
            this.toolStripSeparator16.Size = new System.Drawing.Size(274, 6);
            // 
            // MenuItem_dp2library_uninstall
            // 
            this.MenuItem_dp2library_uninstall.Name = "MenuItem_dp2library_uninstall";
            this.MenuItem_dp2library_uninstall.Size = new System.Drawing.Size(277, 30);
            this.MenuItem_dp2library_uninstall.Text = "卸载 dp2Library";
            this.MenuItem_dp2library_uninstall.Click += new System.EventHandler(this.MenuItem_dp2library_uninstall_Click);
            // 
            // MenuItem_dp2OPAC
            // 
            this.MenuItem_dp2OPAC.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_dp2opac_install,
            this.MenuItem_dp2opac_upgrade,
            this.toolStripSeparator7,
            this.MenuItem_dp2opac_openDataDir,
            this.MenuItem_dp2opac_openVirtualDir,
            this.toolStripSeparator17,
            this.toolStripMenuItem1});
            this.MenuItem_dp2OPAC.Name = "MenuItem_dp2OPAC";
            this.MenuItem_dp2OPAC.Size = new System.Drawing.Size(108, 28);
            this.MenuItem_dp2OPAC.Text = "dp2OPAC";
            // 
            // MenuItem_dp2opac_install
            // 
            this.MenuItem_dp2opac_install.Name = "MenuItem_dp2opac_install";
            this.MenuItem_dp2opac_install.Size = new System.Drawing.Size(252, 30);
            this.MenuItem_dp2opac_install.Text = "安装 dp2OPAC";
            this.MenuItem_dp2opac_install.Click += new System.EventHandler(this.MenuItem_dp2opac_install_Click);
            // 
            // MenuItem_dp2opac_upgrade
            // 
            this.MenuItem_dp2opac_upgrade.Name = "MenuItem_dp2opac_upgrade";
            this.MenuItem_dp2opac_upgrade.Size = new System.Drawing.Size(252, 30);
            this.MenuItem_dp2opac_upgrade.Text = "升级 dp2OPAC";
            this.MenuItem_dp2opac_upgrade.Click += new System.EventHandler(this.MenuItem_dp2opac_upgrade_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(249, 6);
            // 
            // MenuItem_dp2opac_openDataDir
            // 
            this.MenuItem_dp2opac_openDataDir.Name = "MenuItem_dp2opac_openDataDir";
            this.MenuItem_dp2opac_openDataDir.Size = new System.Drawing.Size(252, 30);
            this.MenuItem_dp2opac_openDataDir.Text = "打开数据文件夹";
            // 
            // MenuItem_dp2opac_openVirtualDir
            // 
            this.MenuItem_dp2opac_openVirtualDir.Name = "MenuItem_dp2opac_openVirtualDir";
            this.MenuItem_dp2opac_openVirtualDir.Size = new System.Drawing.Size(252, 30);
            this.MenuItem_dp2opac_openVirtualDir.Text = "打开程序文件夹";
            // 
            // toolStripSeparator17
            // 
            this.toolStripSeparator17.Name = "toolStripSeparator17";
            this.toolStripSeparator17.Size = new System.Drawing.Size(249, 6);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_tools_enableIIS});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(252, 30);
            this.toolStripMenuItem1.Text = "工具";
            // 
            // MenuItem_tools_enableIIS
            // 
            this.MenuItem_tools_enableIIS.Name = "MenuItem_tools_enableIIS";
            this.MenuItem_tools_enableIIS.Size = new System.Drawing.Size(252, 30);
            this.MenuItem_tools_enableIIS.Text = "启用 IIS";
            this.MenuItem_tools_enableIIS.Click += new System.EventHandler(this.MenuItem_tools_enableIIS_Click);
            // 
            // MenuItem_dp2zserver
            // 
            this.MenuItem_dp2zserver.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_dp2ZServer_install,
            this.MenuItem_dp2ZServer_upgrade,
            this.toolStripSeparator19,
            this.MenuItem_dp2ZServer_openDataDir,
            this.MenuItem_dp2ZServer_openAppDir,
            this.toolStripSeparator20,
            this.MenuItem_dp2ZServer_instanceManagement,
            this.toolStripSeparator21,
            this.MenuItem_dp2ZServer_upgradeCfgs,
            this.MenuItem_dp2lZServer_verifySerialNumbers,
            this.toolStripSeparator22,
            this.MenuItem_dp2ZServer_tools});
            this.MenuItem_dp2zserver.Name = "MenuItem_dp2zserver";
            this.MenuItem_dp2zserver.Size = new System.Drawing.Size(121, 28);
            this.MenuItem_dp2zserver.Text = "dp2ZServer";
            this.MenuItem_dp2zserver.Visible = false;
            // 
            // MenuItem_dp2ZServer_install
            // 
            this.MenuItem_dp2ZServer_install.Name = "MenuItem_dp2ZServer_install";
            this.MenuItem_dp2ZServer_install.Size = new System.Drawing.Size(290, 30);
            this.MenuItem_dp2ZServer_install.Text = "安装 dp2ZServer";
            this.MenuItem_dp2ZServer_install.Click += new System.EventHandler(this.MenuItem_dp2ZServer_install_Click);
            // 
            // MenuItem_dp2ZServer_upgrade
            // 
            this.MenuItem_dp2ZServer_upgrade.Name = "MenuItem_dp2ZServer_upgrade";
            this.MenuItem_dp2ZServer_upgrade.Size = new System.Drawing.Size(290, 30);
            this.MenuItem_dp2ZServer_upgrade.Text = "升级 dp2ZServer";
            this.MenuItem_dp2ZServer_upgrade.Click += new System.EventHandler(this.MenuItem_dp2ZServer_upgrade_Click);
            // 
            // toolStripSeparator19
            // 
            this.toolStripSeparator19.Name = "toolStripSeparator19";
            this.toolStripSeparator19.Size = new System.Drawing.Size(287, 6);
            // 
            // MenuItem_dp2ZServer_openDataDir
            // 
            this.MenuItem_dp2ZServer_openDataDir.Name = "MenuItem_dp2ZServer_openDataDir";
            this.MenuItem_dp2ZServer_openDataDir.Size = new System.Drawing.Size(290, 30);
            this.MenuItem_dp2ZServer_openDataDir.Text = "打开数据文件夹";
            // 
            // MenuItem_dp2ZServer_openAppDir
            // 
            this.MenuItem_dp2ZServer_openAppDir.Name = "MenuItem_dp2ZServer_openAppDir";
            this.MenuItem_dp2ZServer_openAppDir.Size = new System.Drawing.Size(290, 30);
            this.MenuItem_dp2ZServer_openAppDir.Text = "打开程序文件夹";
            this.MenuItem_dp2ZServer_openAppDir.Click += new System.EventHandler(this.MenuItem_dp2ZServer_openAppDir_Click);
            // 
            // toolStripSeparator20
            // 
            this.toolStripSeparator20.Name = "toolStripSeparator20";
            this.toolStripSeparator20.Size = new System.Drawing.Size(287, 6);
            // 
            // MenuItem_dp2ZServer_instanceManagement
            // 
            this.MenuItem_dp2ZServer_instanceManagement.Name = "MenuItem_dp2ZServer_instanceManagement";
            this.MenuItem_dp2ZServer_instanceManagement.Size = new System.Drawing.Size(290, 30);
            this.MenuItem_dp2ZServer_instanceManagement.Text = "配置实例";
            this.MenuItem_dp2ZServer_instanceManagement.Click += new System.EventHandler(this.MenuItem_dp2ZServer_instanceManagement_Click);
            // 
            // toolStripSeparator21
            // 
            this.toolStripSeparator21.Name = "toolStripSeparator21";
            this.toolStripSeparator21.Size = new System.Drawing.Size(287, 6);
            this.toolStripSeparator21.Visible = false;
            // 
            // MenuItem_dp2ZServer_upgradeCfgs
            // 
            this.MenuItem_dp2ZServer_upgradeCfgs.Name = "MenuItem_dp2ZServer_upgradeCfgs";
            this.MenuItem_dp2ZServer_upgradeCfgs.Size = new System.Drawing.Size(290, 30);
            this.MenuItem_dp2ZServer_upgradeCfgs.Text = "更新数据目录的配置文件";
            this.MenuItem_dp2ZServer_upgradeCfgs.Visible = false;
            // 
            // MenuItem_dp2lZServer_verifySerialNumbers
            // 
            this.MenuItem_dp2lZServer_verifySerialNumbers.Name = "MenuItem_dp2lZServer_verifySerialNumbers";
            this.MenuItem_dp2lZServer_verifySerialNumbers.Size = new System.Drawing.Size(290, 30);
            this.MenuItem_dp2lZServer_verifySerialNumbers.Text = "检查序列号(&S)";
            this.MenuItem_dp2lZServer_verifySerialNumbers.Visible = false;
            // 
            // toolStripSeparator22
            // 
            this.toolStripSeparator22.Name = "toolStripSeparator22";
            this.toolStripSeparator22.Size = new System.Drawing.Size(287, 6);
            this.toolStripSeparator22.Visible = false;
            // 
            // MenuItem_dp2ZServer_tools
            // 
            this.MenuItem_dp2ZServer_tools.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_dp2ZServer_startService,
            this.MenuItem_dp2ZServer_stopService,
            this.toolStripSeparator23,
            this.MenuItem_dp2ZServer_installService,
            this.MenuItem_dp2ZServer_uninstallService,
            this.toolStripSeparator26,
            this.MenuItem_dp2ZServer_uninstall});
            this.MenuItem_dp2ZServer_tools.Name = "MenuItem_dp2ZServer_tools";
            this.MenuItem_dp2ZServer_tools.Size = new System.Drawing.Size(290, 30);
            this.MenuItem_dp2ZServer_tools.Text = "工具";
            // 
            // MenuItem_dp2ZServer_startService
            // 
            this.MenuItem_dp2ZServer_startService.Name = "MenuItem_dp2ZServer_startService";
            this.MenuItem_dp2ZServer_startService.Size = new System.Drawing.Size(277, 30);
            this.MenuItem_dp2ZServer_startService.Text = "启动 Windows Service";
            this.MenuItem_dp2ZServer_startService.Click += new System.EventHandler(this.MenuItem_dp2ZServer_startService_Click);
            // 
            // MenuItem_dp2ZServer_stopService
            // 
            this.MenuItem_dp2ZServer_stopService.Name = "MenuItem_dp2ZServer_stopService";
            this.MenuItem_dp2ZServer_stopService.Size = new System.Drawing.Size(277, 30);
            this.MenuItem_dp2ZServer_stopService.Text = "停止 Windows Service";
            this.MenuItem_dp2ZServer_stopService.Click += new System.EventHandler(this.MenuItem_dp2ZServer_stopService_Click);
            // 
            // toolStripSeparator23
            // 
            this.toolStripSeparator23.Name = "toolStripSeparator23";
            this.toolStripSeparator23.Size = new System.Drawing.Size(274, 6);
            // 
            // MenuItem_dp2ZServer_installService
            // 
            this.MenuItem_dp2ZServer_installService.Name = "MenuItem_dp2ZServer_installService";
            this.MenuItem_dp2ZServer_installService.Size = new System.Drawing.Size(277, 30);
            this.MenuItem_dp2ZServer_installService.Text = "注册 Windows Service";
            this.MenuItem_dp2ZServer_installService.Click += new System.EventHandler(this.MenuItem_dp2ZServer_installService_Click);
            // 
            // MenuItem_dp2ZServer_uninstallService
            // 
            this.MenuItem_dp2ZServer_uninstallService.Name = "MenuItem_dp2ZServer_uninstallService";
            this.MenuItem_dp2ZServer_uninstallService.Size = new System.Drawing.Size(277, 30);
            this.MenuItem_dp2ZServer_uninstallService.Text = "注销 Windows Service";
            this.MenuItem_dp2ZServer_uninstallService.Click += new System.EventHandler(this.MenuItem_dp2ZServer_uninstallService_Click);
            // 
            // toolStripSeparator26
            // 
            this.toolStripSeparator26.Name = "toolStripSeparator26";
            this.toolStripSeparator26.Size = new System.Drawing.Size(274, 6);
            // 
            // MenuItem_dp2ZServer_uninstall
            // 
            this.MenuItem_dp2ZServer_uninstall.Name = "MenuItem_dp2ZServer_uninstall";
            this.MenuItem_dp2ZServer_uninstall.Size = new System.Drawing.Size(277, 30);
            this.MenuItem_dp2ZServer_uninstall.Text = "卸载 dp2ZServer";
            this.MenuItem_dp2ZServer_uninstall.Click += new System.EventHandler(this.MenuItem_dp2ZServer_uninstall_Click);
            // 
            // MenuItem_help
            // 
            this.MenuItem_help.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_openUserFolder,
            this.MenuItem_openDataFolder,
            this.MenuItem_openProgramFolder,
            this.toolStripSeparator25,
            this.ToolStripMenuItem_getMD5ofFile,
            this.toolStripSeparator3,
            this.ToolStripMenuItem_uninstallDp2zserver,
            this.toolStripSeparator24,
            this.MenuItem_copyright});
            this.MenuItem_help.Name = "MenuItem_help";
            this.MenuItem_help.Size = new System.Drawing.Size(84, 28);
            this.MenuItem_help.Text = "帮助(&H)";
            // 
            // MenuItem_openUserFolder
            // 
            this.MenuItem_openUserFolder.Name = "MenuItem_openUserFolder";
            this.MenuItem_openUserFolder.Size = new System.Drawing.Size(357, 30);
            this.MenuItem_openUserFolder.Text = "打开 dp2Installer 用户文件夹(&U)";
            this.MenuItem_openUserFolder.Click += new System.EventHandler(this.MenuItem_openUserFolder_Click);
            // 
            // MenuItem_openDataFolder
            // 
            this.MenuItem_openDataFolder.Name = "MenuItem_openDataFolder";
            this.MenuItem_openDataFolder.Size = new System.Drawing.Size(357, 30);
            this.MenuItem_openDataFolder.Text = "打开 dp2Installer 数据文件夹(&D)";
            this.MenuItem_openDataFolder.Click += new System.EventHandler(this.MenuItem_openDataFolder_Click);
            // 
            // MenuItem_openProgramFolder
            // 
            this.MenuItem_openProgramFolder.Name = "MenuItem_openProgramFolder";
            this.MenuItem_openProgramFolder.Size = new System.Drawing.Size(357, 30);
            this.MenuItem_openProgramFolder.Text = "打开 dp2Installer 程序文件夹(&P)";
            this.MenuItem_openProgramFolder.Click += new System.EventHandler(this.MenuItem_openProgramFolder_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(354, 6);
            // 
            // ToolStripMenuItem_uninstallDp2zserver
            // 
            this.ToolStripMenuItem_uninstallDp2zserver.Name = "ToolStripMenuItem_uninstallDp2zserver";
            this.ToolStripMenuItem_uninstallDp2zserver.Size = new System.Drawing.Size(357, 30);
            this.ToolStripMenuItem_uninstallDp2zserver.Text = "卸载 dp2ZServer";
            this.ToolStripMenuItem_uninstallDp2zserver.Click += new System.EventHandler(this.ToolStripMenuItem_uninstallDp2zserver_Click);
            // 
            // toolStripSeparator24
            // 
            this.toolStripSeparator24.Name = "toolStripSeparator24";
            this.toolStripSeparator24.Size = new System.Drawing.Size(354, 6);
            // 
            // MenuItem_copyright
            // 
            this.MenuItem_copyright.Name = "MenuItem_copyright";
            this.MenuItem_copyright.Size = new System.Drawing.Size(357, 30);
            this.MenuItem_copyright.Text = "版权(&C)...";
            this.MenuItem_copyright.Visible = false;
            this.MenuItem_copyright.Click += new System.EventHandler(this.MenuItem_copyright_Click);
            // 
            // statusStrip_main
            // 
            this.statusStrip_main.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.statusStrip_main.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip_main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_main,
            this.toolStripProgressBar_main});
            this.statusStrip_main.Location = new System.Drawing.Point(0, 514);
            this.statusStrip_main.Name = "statusStrip_main";
            this.statusStrip_main.Padding = new System.Windows.Forms.Padding(2, 0, 21, 0);
            this.statusStrip_main.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
            this.statusStrip_main.Size = new System.Drawing.Size(867, 30);
            this.statusStrip_main.TabIndex = 3;
            this.statusStrip_main.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_main
            // 
            this.toolStripStatusLabel_main.Name = "toolStripStatusLabel_main";
            this.toolStripStatusLabel_main.Size = new System.Drawing.Size(692, 25);
            this.toolStripStatusLabel_main.Spring = true;
            // 
            // toolStripProgressBar_main
            // 
            this.toolStripProgressBar_main.AutoSize = false;
            this.toolStripProgressBar_main.BackColor = System.Drawing.SystemColors.Control;
            this.toolStripProgressBar_main.Name = "toolStripProgressBar_main";
            this.toolStripProgressBar_main.Size = new System.Drawing.Size(150, 24);
            this.toolStripProgressBar_main.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolButton_stop,
            this.toolStripDropDownButton_stopAll});
            this.toolStrip1.Location = new System.Drawing.Point(0, 34);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Padding = new System.Windows.Forms.Padding(0, 0, 2, 0);
            this.toolStrip1.Size = new System.Drawing.Size(867, 31);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolButton_stop
            // 
            this.toolButton_stop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolButton_stop.Enabled = false;
            this.toolButton_stop.Image = ((System.Drawing.Image)(resources.GetObject("toolButton_stop.Image")));
            this.toolButton_stop.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolButton_stop.Name = "toolButton_stop";
            this.toolButton_stop.Size = new System.Drawing.Size(28, 28);
            this.toolButton_stop.Text = "停止";
            // 
            // toolStripDropDownButton_stopAll
            // 
            this.toolStripDropDownButton_stopAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButton_stopAll.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_stopAll});
            this.toolStripDropDownButton_stopAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_stopAll.Name = "toolStripDropDownButton_stopAll";
            this.toolStripDropDownButton_stopAll.Size = new System.Drawing.Size(18, 28);
            this.toolStripDropDownButton_stopAll.Text = "停止全部";
            // 
            // ToolStripMenuItem_stopAll
            // 
            this.ToolStripMenuItem_stopAll.Name = "ToolStripMenuItem_stopAll";
            this.ToolStripMenuItem_stopAll.Size = new System.Drawing.Size(189, 30);
            this.ToolStripMenuItem_stopAll.Text = "停止全部(&A)";
            this.ToolStripMenuItem_stopAll.ToolTipText = "停止全部正在处理的操作";
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 65);
            this.webBrowser1.Margin = new System.Windows.Forms.Padding(4);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(30, 30);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(867, 449);
            this.webBrowser1.TabIndex = 4;
            // 
            // ToolStripMenuItem_getMD5ofFile
            // 
            this.ToolStripMenuItem_getMD5ofFile.Name = "ToolStripMenuItem_getMD5ofFile";
            this.ToolStripMenuItem_getMD5ofFile.Size = new System.Drawing.Size(357, 30);
            this.ToolStripMenuItem_getMD5ofFile.Text = "获得 MD5 校验码 ...";
            this.ToolStripMenuItem_getMD5ofFile.Click += new System.EventHandler(this.ToolStripMenuItem_getMD5ofFile_Click);
            // 
            // toolStripSeparator25
            // 
            this.toolStripSeparator25.Name = "toolStripSeparator25";
            this.toolStripSeparator25.Size = new System.Drawing.Size(354, 6);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(867, 544);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip_main);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.Text = "dp2Installer V3 -- dp2 安装实用工具";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip_main.ResumeLayout(false);
            this.statusStrip_main.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_file;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_exit;
        private System.Windows.Forms.ToolStripMenuItem dp2LibraryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2library_instanceManagement;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2library_upgrade;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2library_upgradeCfgs;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_help;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openUserFolder;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openDataFolder;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openProgramFolder;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        internal System.Windows.Forms.ToolStripMenuItem MenuItem_copyright;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2library_verifySerialNumbers;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2Kernel;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2kernel_upgrade;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2OPAC;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2opac_upgrade;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_autoUpgrade;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2library_openDataDir;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2kernel_openDataDir;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2opac_openDataDir;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2opac_openVirtualDir;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2library_openAppDir;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2kernel_openAppDir;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_sendDebugInfos;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_displayDigitalPlatformEventLog;
        private System.Windows.Forms.StatusStrip statusStrip_main;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_main;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar_main;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolButton_stop;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_stopAll;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_stopAll;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator9;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2kernel_instanceManagement;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2kernel_install;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator10;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2kernel_tools;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2kernel_tools_uninstallService;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2kernel_startService;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2kernel_stopService;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator11;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2kernel_tools_installService;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2library_install;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator13;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2library_tools;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2library_startService;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2library_stopService;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator12;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2library_installService;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2library_uninstallService;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator14;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2kernel_uninstall;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2opac_install;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator17;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_tools_enableIIS;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator15;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2library_uninstall;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2library_enableMsmq;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator16;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2library_setupMongoDB;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator18;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2zserver;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2ZServer_install;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2ZServer_upgrade;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator19;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2ZServer_openDataDir;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2ZServer_openAppDir;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator20;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2ZServer_instanceManagement;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator21;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2ZServer_upgradeCfgs;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2lZServer_verifySerialNumbers;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator22;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2ZServer_tools;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2ZServer_startService;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2ZServer_stopService;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator23;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2ZServer_installService;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2ZServer_uninstallService;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator26;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2ZServer_uninstall;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_uninstallDp2zserver;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator24;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_getMD5ofFile;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator25;
    }
}

