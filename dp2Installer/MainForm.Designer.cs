namespace dp2Installer
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
            this.MenuItem_dp2kernel_update = new System.Windows.Forms.ToolStripMenuItem();
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
            this.MenuItem_dp2library_update = new System.Windows.Forms.ToolStripMenuItem();
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
            this.MenuItem_dp2Commander = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2Commander_install = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2Commander_upgrade = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator27 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2Commander_openAppDir = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator28 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2Commander_settings = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator29 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripSeparator30 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem10 = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2Commander_startService = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2Commander_stopService = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator31 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2Commander_installService = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2Commander_uninstallService = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator32 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_dp2Commander_uninstall = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2zserver = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2ZServer_install = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_dp2ZServer_update = new System.Windows.Forms.ToolStripMenuItem();
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
            this.MenuItem_palmCenter = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_palmCenter_install = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_palmCenter_update = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator33 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_palmCenter_openProgramFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator34 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_palmCenter_config = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator35 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_palmCenter_tool = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_palmCenter_startService = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_palmCenter_stopService = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator36 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_palmCenter_installService = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_palmCenter_uninstallService = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator37 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_palmCenter_uninstall = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_help = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openUserFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openDataFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openProgramFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator25 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItem_getMD5ofFile = new System.Windows.Forms.ToolStripMenuItem();
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
            this.MenuItem_palmCenter_openDataDir = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1.SuspendLayout();
            this.statusStrip_main.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_file,
            this.MenuItem_dp2Kernel,
            this.dp2LibraryToolStripMenuItem,
            this.MenuItem_dp2OPAC,
            this.MenuItem_dp2Commander,
            this.MenuItem_dp2zserver,
            this.MenuItem_palmCenter,
            this.MenuItem_help});
            this.menuStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(11, 3, 0, 3);
            this.menuStrip1.Size = new System.Drawing.Size(794, 71);
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
            this.MenuItem_file.Size = new System.Drawing.Size(97, 32);
            this.MenuItem_file.Text = "文件(&F)";
            // 
            // MenuItem_autoUpgrade
            // 
            this.MenuItem_autoUpgrade.Name = "MenuItem_autoUpgrade";
            this.MenuItem_autoUpgrade.Size = new System.Drawing.Size(421, 40);
            this.MenuItem_autoUpgrade.Text = "自动升级全部产品(&A)";
            this.MenuItem_autoUpgrade.Click += new System.EventHandler(this.MenuItem_autoUpdate_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(418, 6);
            // 
            // MenuItem_displayDigitalPlatformEventLog
            // 
            this.MenuItem_displayDigitalPlatformEventLog.Name = "MenuItem_displayDigitalPlatformEventLog";
            this.MenuItem_displayDigitalPlatformEventLog.Size = new System.Drawing.Size(421, 40);
            this.MenuItem_displayDigitalPlatformEventLog.Text = "显示 DigitalPlatform 事件日志";
            this.MenuItem_displayDigitalPlatformEventLog.Click += new System.EventHandler(this.MenuItem_displayDigitalPlatformEventLog_Click);
            // 
            // MenuItem_sendDebugInfos
            // 
            this.MenuItem_sendDebugInfos.Name = "MenuItem_sendDebugInfos";
            this.MenuItem_sendDebugInfos.Size = new System.Drawing.Size(421, 40);
            this.MenuItem_sendDebugInfos.Text = "打包事件日志信息(&S)";
            this.MenuItem_sendDebugInfos.Click += new System.EventHandler(this.MenuItem_zipDebugInfos_Click);
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(418, 6);
            // 
            // MenuItem_exit
            // 
            this.MenuItem_exit.Name = "MenuItem_exit";
            this.MenuItem_exit.Size = new System.Drawing.Size(421, 40);
            this.MenuItem_exit.Text = "退出(&X)";
            this.MenuItem_exit.Click += new System.EventHandler(this.MenuItem_exit_Click);
            // 
            // MenuItem_dp2Kernel
            // 
            this.MenuItem_dp2Kernel.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_dp2kernel_install,
            this.MenuItem_dp2kernel_update,
            this.toolStripSeparator6,
            this.MenuItem_dp2kernel_openDataDir,
            this.MenuItem_dp2kernel_openAppDir,
            this.toolStripSeparator9,
            this.MenuItem_dp2kernel_instanceManagement,
            this.toolStripSeparator10,
            this.MenuItem_dp2kernel_tools});
            this.MenuItem_dp2Kernel.Name = "MenuItem_dp2Kernel";
            this.MenuItem_dp2Kernel.Size = new System.Drawing.Size(132, 32);
            this.MenuItem_dp2Kernel.Text = "dp2Kernel";
            // 
            // MenuItem_dp2kernel_install
            // 
            this.MenuItem_dp2kernel_install.Name = "MenuItem_dp2kernel_install";
            this.MenuItem_dp2kernel_install.Size = new System.Drawing.Size(279, 40);
            this.MenuItem_dp2kernel_install.Text = "安装 dp2Kernel";
            this.MenuItem_dp2kernel_install.Click += new System.EventHandler(this.MenuItem_dp2kernel_install_Click);
            // 
            // MenuItem_dp2kernel_upgrade
            // 
            this.MenuItem_dp2kernel_update.Name = "MenuItem_dp2kernel_upgrade";
            this.MenuItem_dp2kernel_update.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_dp2kernel_update.Text = "更新 dp2Kernel";
            this.MenuItem_dp2kernel_update.Click += new System.EventHandler(this.MenuItem_dp2kernel_update_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(276, 6);
            // 
            // MenuItem_dp2kernel_openDataDir
            // 
            this.MenuItem_dp2kernel_openDataDir.Name = "MenuItem_dp2kernel_openDataDir";
            this.MenuItem_dp2kernel_openDataDir.Size = new System.Drawing.Size(279, 40);
            this.MenuItem_dp2kernel_openDataDir.Text = "打开数据文件夹";
            // 
            // MenuItem_dp2kernel_openAppDir
            // 
            this.MenuItem_dp2kernel_openAppDir.Name = "MenuItem_dp2kernel_openAppDir";
            this.MenuItem_dp2kernel_openAppDir.Size = new System.Drawing.Size(279, 40);
            this.MenuItem_dp2kernel_openAppDir.Text = "打开程序文件夹";
            this.MenuItem_dp2kernel_openAppDir.Click += new System.EventHandler(this.MenuItem_dp2kernel_openAppDir_Click);
            // 
            // toolStripSeparator9
            // 
            this.toolStripSeparator9.Name = "toolStripSeparator9";
            this.toolStripSeparator9.Size = new System.Drawing.Size(276, 6);
            // 
            // MenuItem_dp2kernel_instanceManagement
            // 
            this.MenuItem_dp2kernel_instanceManagement.Name = "MenuItem_dp2kernel_instanceManagement";
            this.MenuItem_dp2kernel_instanceManagement.Size = new System.Drawing.Size(279, 40);
            this.MenuItem_dp2kernel_instanceManagement.Text = "配置实例";
            this.MenuItem_dp2kernel_instanceManagement.Click += new System.EventHandler(this.MenuItem_dp2kernel_instanceManagement_Click);
            // 
            // toolStripSeparator10
            // 
            this.toolStripSeparator10.Name = "toolStripSeparator10";
            this.toolStripSeparator10.Size = new System.Drawing.Size(276, 6);
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
            this.MenuItem_dp2kernel_tools.Size = new System.Drawing.Size(279, 40);
            this.MenuItem_dp2kernel_tools.Text = "工具";
            // 
            // MenuItem_dp2kernel_startService
            // 
            this.MenuItem_dp2kernel_startService.Name = "MenuItem_dp2kernel_startService";
            this.MenuItem_dp2kernel_startService.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_dp2kernel_startService.Text = "启动 Windows Service";
            this.MenuItem_dp2kernel_startService.Click += new System.EventHandler(this.MenuItem_dp2kernel_startService_Click);
            // 
            // MenuItem_dp2kernel_stopService
            // 
            this.MenuItem_dp2kernel_stopService.Name = "MenuItem_dp2kernel_stopService";
            this.MenuItem_dp2kernel_stopService.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_dp2kernel_stopService.Text = "停止 Windows Service";
            this.MenuItem_dp2kernel_stopService.Click += new System.EventHandler(this.MenuItem_dp2kernel_stopService_Click);
            // 
            // toolStripSeparator11
            // 
            this.toolStripSeparator11.Name = "toolStripSeparator11";
            this.toolStripSeparator11.Size = new System.Drawing.Size(345, 6);
            // 
            // MenuItem_dp2kernel_tools_installService
            // 
            this.MenuItem_dp2kernel_tools_installService.Name = "MenuItem_dp2kernel_tools_installService";
            this.MenuItem_dp2kernel_tools_installService.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_dp2kernel_tools_installService.Text = "注册 Windows Service";
            this.MenuItem_dp2kernel_tools_installService.Click += new System.EventHandler(this.MenuItem_dp2kernel_installService_Click);
            // 
            // MenuItem_dp2kernel_tools_uninstallService
            // 
            this.MenuItem_dp2kernel_tools_uninstallService.Name = "MenuItem_dp2kernel_tools_uninstallService";
            this.MenuItem_dp2kernel_tools_uninstallService.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_dp2kernel_tools_uninstallService.Text = "注销 Windows Service";
            this.MenuItem_dp2kernel_tools_uninstallService.Click += new System.EventHandler(this.MenuItem_dp2kernel_uninstallService_Click);
            // 
            // toolStripSeparator14
            // 
            this.toolStripSeparator14.Name = "toolStripSeparator14";
            this.toolStripSeparator14.Size = new System.Drawing.Size(345, 6);
            // 
            // MenuItem_dp2kernel_uninstall
            // 
            this.MenuItem_dp2kernel_uninstall.Name = "MenuItem_dp2kernel_uninstall";
            this.MenuItem_dp2kernel_uninstall.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_dp2kernel_uninstall.Text = "卸载 dp2Kernel";
            this.MenuItem_dp2kernel_uninstall.Click += new System.EventHandler(this.MenuItem_dp2kernel_uninstall_Click);
            // 
            // dp2LibraryToolStripMenuItem
            // 
            this.dp2LibraryToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_dp2library_install,
            this.MenuItem_dp2library_update,
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
            this.dp2LibraryToolStripMenuItem.Size = new System.Drawing.Size(137, 32);
            this.dp2LibraryToolStripMenuItem.Text = "dp2Library";
            // 
            // MenuItem_dp2library_install
            // 
            this.MenuItem_dp2library_install.Name = "MenuItem_dp2library_install";
            this.MenuItem_dp2library_install.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_dp2library_install.Text = "安装 dp2Library";
            this.MenuItem_dp2library_install.Click += new System.EventHandler(this.MenuItem_dp2library_install_Click);
            // 
            // MenuItem_dp2library_upgrade
            // 
            this.MenuItem_dp2library_update.Name = "MenuItem_dp2library_upgrade";
            this.MenuItem_dp2library_update.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_dp2library_update.Text = "更新 dp2Library";
            this.MenuItem_dp2library_update.Click += new System.EventHandler(this.MenuItem_dp2library_update_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(357, 6);
            // 
            // MenuItem_dp2library_openDataDir
            // 
            this.MenuItem_dp2library_openDataDir.Name = "MenuItem_dp2library_openDataDir";
            this.MenuItem_dp2library_openDataDir.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_dp2library_openDataDir.Text = "打开数据文件夹";
            // 
            // MenuItem_dp2library_openAppDir
            // 
            this.MenuItem_dp2library_openAppDir.Name = "MenuItem_dp2library_openAppDir";
            this.MenuItem_dp2library_openAppDir.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_dp2library_openAppDir.Text = "打开程序文件夹";
            this.MenuItem_dp2library_openAppDir.Click += new System.EventHandler(this.MenuItem_dp2library_openAppDir_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(357, 6);
            // 
            // MenuItem_dp2library_instanceManagement
            // 
            this.MenuItem_dp2library_instanceManagement.Name = "MenuItem_dp2library_instanceManagement";
            this.MenuItem_dp2library_instanceManagement.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_dp2library_instanceManagement.Text = "配置实例";
            this.MenuItem_dp2library_instanceManagement.Click += new System.EventHandler(this.MenuItem_dp2library_instanceManagement_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(357, 6);
            // 
            // MenuItem_dp2library_upgradeCfgs
            // 
            this.MenuItem_dp2library_upgradeCfgs.Name = "MenuItem_dp2library_upgradeCfgs";
            this.MenuItem_dp2library_upgradeCfgs.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_dp2library_upgradeCfgs.Text = "更新数据目录的配置文件";
            this.MenuItem_dp2library_upgradeCfgs.Click += new System.EventHandler(this.MenuItem_dp2library_updateCfgs_Click);
            // 
            // MenuItem_dp2library_verifySerialNumbers
            // 
            this.MenuItem_dp2library_verifySerialNumbers.Name = "MenuItem_dp2library_verifySerialNumbers";
            this.MenuItem_dp2library_verifySerialNumbers.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_dp2library_verifySerialNumbers.Text = "检查序列号(&S)";
            this.MenuItem_dp2library_verifySerialNumbers.Click += new System.EventHandler(this.MenuItem_dp2library_verifySerialNumbers_Click);
            // 
            // toolStripSeparator13
            // 
            this.toolStripSeparator13.Name = "toolStripSeparator13";
            this.toolStripSeparator13.Size = new System.Drawing.Size(357, 6);
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
            this.MenuItem_dp2library_tools.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_dp2library_tools.Text = "工具";
            // 
            // MenuItem_dp2library_startService
            // 
            this.MenuItem_dp2library_startService.Name = "MenuItem_dp2library_startService";
            this.MenuItem_dp2library_startService.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_dp2library_startService.Text = "启动 Windows Service";
            this.MenuItem_dp2library_startService.Click += new System.EventHandler(this.MenuItem_dp2library_startService_Click);
            // 
            // MenuItem_dp2library_stopService
            // 
            this.MenuItem_dp2library_stopService.Name = "MenuItem_dp2library_stopService";
            this.MenuItem_dp2library_stopService.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_dp2library_stopService.Text = "停止 Windows Service";
            this.MenuItem_dp2library_stopService.Click += new System.EventHandler(this.MenuItem_dp2library_stopService_Click);
            // 
            // toolStripSeparator12
            // 
            this.toolStripSeparator12.Name = "toolStripSeparator12";
            this.toolStripSeparator12.Size = new System.Drawing.Size(345, 6);
            // 
            // MenuItem_dp2library_installService
            // 
            this.MenuItem_dp2library_installService.Name = "MenuItem_dp2library_installService";
            this.MenuItem_dp2library_installService.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_dp2library_installService.Text = "注册 Windows Service";
            this.MenuItem_dp2library_installService.Click += new System.EventHandler(this.MenuItem_dp2library_installService_Click);
            // 
            // MenuItem_dp2library_uninstallService
            // 
            this.MenuItem_dp2library_uninstallService.Name = "MenuItem_dp2library_uninstallService";
            this.MenuItem_dp2library_uninstallService.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_dp2library_uninstallService.Text = "注销 Windows Service";
            this.MenuItem_dp2library_uninstallService.Click += new System.EventHandler(this.MenuItem_dp2library_uninstallService_Click);
            // 
            // toolStripSeparator15
            // 
            this.toolStripSeparator15.Name = "toolStripSeparator15";
            this.toolStripSeparator15.Size = new System.Drawing.Size(345, 6);
            // 
            // MenuItem_dp2library_setupMongoDB
            // 
            this.MenuItem_dp2library_setupMongoDB.Name = "MenuItem_dp2library_setupMongoDB";
            this.MenuItem_dp2library_setupMongoDB.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_dp2library_setupMongoDB.Text = "安装 MongoDB ...";
            this.MenuItem_dp2library_setupMongoDB.Click += new System.EventHandler(this.MenuItem_dp2library_setupMongoDB_Click);
            // 
            // toolStripSeparator18
            // 
            this.toolStripSeparator18.Name = "toolStripSeparator18";
            this.toolStripSeparator18.Size = new System.Drawing.Size(345, 6);
            // 
            // MenuItem_dp2library_enableMsmq
            // 
            this.MenuItem_dp2library_enableMsmq.Name = "MenuItem_dp2library_enableMsmq";
            this.MenuItem_dp2library_enableMsmq.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_dp2library_enableMsmq.Text = "启用 MSMQ";
            this.MenuItem_dp2library_enableMsmq.Click += new System.EventHandler(this.MenuItem_dp2library_enableMsmq_Click);
            // 
            // toolStripSeparator16
            // 
            this.toolStripSeparator16.Name = "toolStripSeparator16";
            this.toolStripSeparator16.Size = new System.Drawing.Size(345, 6);
            // 
            // MenuItem_dp2library_uninstall
            // 
            this.MenuItem_dp2library_uninstall.Name = "MenuItem_dp2library_uninstall";
            this.MenuItem_dp2library_uninstall.Size = new System.Drawing.Size(348, 40);
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
            this.MenuItem_dp2OPAC.Size = new System.Drawing.Size(127, 32);
            this.MenuItem_dp2OPAC.Text = "dp2OPAC";
            // 
            // MenuItem_dp2opac_install
            // 
            this.MenuItem_dp2opac_install.Name = "MenuItem_dp2opac_install";
            this.MenuItem_dp2opac_install.Size = new System.Drawing.Size(276, 40);
            this.MenuItem_dp2opac_install.Text = "安装 dp2OPAC";
            this.MenuItem_dp2opac_install.Click += new System.EventHandler(this.MenuItem_dp2opac_install_Click);
            // 
            // MenuItem_dp2opac_upgrade
            // 
            this.MenuItem_dp2opac_upgrade.Name = "MenuItem_dp2opac_upgrade";
            this.MenuItem_dp2opac_upgrade.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_dp2opac_upgrade.Text = "更新 dp2OPAC";
            this.MenuItem_dp2opac_upgrade.Click += new System.EventHandler(this.MenuItem_dp2opac_update_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(273, 6);
            // 
            // MenuItem_dp2opac_openDataDir
            // 
            this.MenuItem_dp2opac_openDataDir.Name = "MenuItem_dp2opac_openDataDir";
            this.MenuItem_dp2opac_openDataDir.Size = new System.Drawing.Size(276, 40);
            this.MenuItem_dp2opac_openDataDir.Text = "打开数据文件夹";
            // 
            // MenuItem_dp2opac_openVirtualDir
            // 
            this.MenuItem_dp2opac_openVirtualDir.Name = "MenuItem_dp2opac_openVirtualDir";
            this.MenuItem_dp2opac_openVirtualDir.Size = new System.Drawing.Size(276, 40);
            this.MenuItem_dp2opac_openVirtualDir.Text = "打开程序文件夹";
            // 
            // toolStripSeparator17
            // 
            this.toolStripSeparator17.Name = "toolStripSeparator17";
            this.toolStripSeparator17.Size = new System.Drawing.Size(273, 6);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_tools_enableIIS});
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(276, 40);
            this.toolStripMenuItem1.Text = "工具";
            // 
            // MenuItem_tools_enableIIS
            // 
            this.MenuItem_tools_enableIIS.Name = "MenuItem_tools_enableIIS";
            this.MenuItem_tools_enableIIS.Size = new System.Drawing.Size(201, 40);
            this.MenuItem_tools_enableIIS.Text = "启用 IIS";
            this.MenuItem_tools_enableIIS.Click += new System.EventHandler(this.MenuItem_tools_enableIIS_Click);
            // 
            // MenuItem_dp2Commander
            // 
            this.MenuItem_dp2Commander.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_dp2Commander_install,
            this.MenuItem_dp2Commander_upgrade,
            this.toolStripSeparator27,
            this.MenuItem_dp2Commander_openAppDir,
            this.toolStripSeparator28,
            this.MenuItem_dp2Commander_settings,
            this.toolStripSeparator29,
            this.toolStripSeparator30,
            this.toolStripMenuItem10});
            this.MenuItem_dp2Commander.Name = "MenuItem_dp2Commander";
            this.MenuItem_dp2Commander.Size = new System.Drawing.Size(193, 32);
            this.MenuItem_dp2Commander.Text = "dp2Commander";
            this.MenuItem_dp2Commander.Visible = false;
            // 
            // MenuItem_dp2Commander_install
            // 
            this.MenuItem_dp2Commander_install.Name = "MenuItem_dp2Commander_install";
            this.MenuItem_dp2Commander_install.Size = new System.Drawing.Size(340, 40);
            this.MenuItem_dp2Commander_install.Text = "安装 dp2Commander";
            this.MenuItem_dp2Commander_install.Click += new System.EventHandler(this.MenuItem_dp2Commander_install_Click);
            // 
            // MenuItem_dp2Commander_upgrade
            // 
            this.MenuItem_dp2Commander_upgrade.Name = "MenuItem_dp2Commander_upgrade";
            this.MenuItem_dp2Commander_upgrade.Size = new System.Drawing.Size(340, 40);
            this.MenuItem_dp2Commander_upgrade.Text = "更新 dp2Commander";
            // 
            // toolStripSeparator27
            // 
            this.toolStripSeparator27.Name = "toolStripSeparator27";
            this.toolStripSeparator27.Size = new System.Drawing.Size(337, 6);
            // 
            // MenuItem_dp2Commander_openAppDir
            // 
            this.MenuItem_dp2Commander_openAppDir.Name = "MenuItem_dp2Commander_openAppDir";
            this.MenuItem_dp2Commander_openAppDir.Size = new System.Drawing.Size(340, 40);
            this.MenuItem_dp2Commander_openAppDir.Text = "打开程序文件夹";
            // 
            // toolStripSeparator28
            // 
            this.toolStripSeparator28.Name = "toolStripSeparator28";
            this.toolStripSeparator28.Size = new System.Drawing.Size(337, 6);
            // 
            // MenuItem_dp2Commander_settings
            // 
            this.MenuItem_dp2Commander_settings.Name = "MenuItem_dp2Commander_settings";
            this.MenuItem_dp2Commander_settings.Size = new System.Drawing.Size(340, 40);
            this.MenuItem_dp2Commander_settings.Text = "配置实例";
            // 
            // toolStripSeparator29
            // 
            this.toolStripSeparator29.Name = "toolStripSeparator29";
            this.toolStripSeparator29.Size = new System.Drawing.Size(337, 6);
            this.toolStripSeparator29.Visible = false;
            // 
            // toolStripSeparator30
            // 
            this.toolStripSeparator30.Name = "toolStripSeparator30";
            this.toolStripSeparator30.Size = new System.Drawing.Size(337, 6);
            this.toolStripSeparator30.Visible = false;
            // 
            // toolStripMenuItem10
            // 
            this.toolStripMenuItem10.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_dp2Commander_startService,
            this.MenuItem_dp2Commander_stopService,
            this.toolStripSeparator31,
            this.MenuItem_dp2Commander_installService,
            this.MenuItem_dp2Commander_uninstallService,
            this.toolStripSeparator32,
            this.MenuItem_dp2Commander_uninstall});
            this.toolStripMenuItem10.Name = "toolStripMenuItem10";
            this.toolStripMenuItem10.Size = new System.Drawing.Size(340, 40);
            this.toolStripMenuItem10.Text = "工具";
            // 
            // MenuItem_dp2Commander_startService
            // 
            this.MenuItem_dp2Commander_startService.Name = "MenuItem_dp2Commander_startService";
            this.MenuItem_dp2Commander_startService.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_dp2Commander_startService.Text = "启动 Windows Service";
            // 
            // MenuItem_dp2Commander_stopService
            // 
            this.MenuItem_dp2Commander_stopService.Name = "MenuItem_dp2Commander_stopService";
            this.MenuItem_dp2Commander_stopService.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_dp2Commander_stopService.Text = "停止 Windows Service";
            // 
            // toolStripSeparator31
            // 
            this.toolStripSeparator31.Name = "toolStripSeparator31";
            this.toolStripSeparator31.Size = new System.Drawing.Size(345, 6);
            // 
            // MenuItem_dp2Commander_installService
            // 
            this.MenuItem_dp2Commander_installService.Name = "MenuItem_dp2Commander_installService";
            this.MenuItem_dp2Commander_installService.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_dp2Commander_installService.Text = "注册 Windows Service";
            // 
            // MenuItem_dp2Commander_uninstallService
            // 
            this.MenuItem_dp2Commander_uninstallService.Name = "MenuItem_dp2Commander_uninstallService";
            this.MenuItem_dp2Commander_uninstallService.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_dp2Commander_uninstallService.Text = "注销 Windows Service";
            // 
            // toolStripSeparator32
            // 
            this.toolStripSeparator32.Name = "toolStripSeparator32";
            this.toolStripSeparator32.Size = new System.Drawing.Size(345, 6);
            // 
            // MenuItem_dp2Commander_uninstall
            // 
            this.MenuItem_dp2Commander_uninstall.Name = "MenuItem_dp2Commander_uninstall";
            this.MenuItem_dp2Commander_uninstall.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_dp2Commander_uninstall.Text = "卸载 dp2Commander";
            // 
            // MenuItem_dp2zserver
            // 
            this.MenuItem_dp2zserver.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_dp2ZServer_install,
            this.MenuItem_dp2ZServer_update,
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
            this.MenuItem_dp2zserver.Size = new System.Drawing.Size(144, 32);
            this.MenuItem_dp2zserver.Text = "dp2ZServer";
            this.MenuItem_dp2zserver.Visible = false;
            // 
            // MenuItem_dp2ZServer_install
            // 
            this.MenuItem_dp2ZServer_install.Name = "MenuItem_dp2ZServer_install";
            this.MenuItem_dp2ZServer_install.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_dp2ZServer_install.Text = "安装 dp2ZServer";
            this.MenuItem_dp2ZServer_install.Click += new System.EventHandler(this.MenuItem_dp2ZServer_install_Click);
            // 
            // MenuItem_dp2ZServer_upgrade
            // 
            this.MenuItem_dp2ZServer_update.Name = "MenuItem_dp2ZServer_upgrade";
            this.MenuItem_dp2ZServer_update.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_dp2ZServer_update.Text = "更新 dp2ZServer";
            this.MenuItem_dp2ZServer_update.Click += new System.EventHandler(this.MenuItem_dp2ZServer_update_Click);
            // 
            // toolStripSeparator19
            // 
            this.toolStripSeparator19.Name = "toolStripSeparator19";
            this.toolStripSeparator19.Size = new System.Drawing.Size(357, 6);
            // 
            // MenuItem_dp2ZServer_openDataDir
            // 
            this.MenuItem_dp2ZServer_openDataDir.Name = "MenuItem_dp2ZServer_openDataDir";
            this.MenuItem_dp2ZServer_openDataDir.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_dp2ZServer_openDataDir.Text = "打开数据文件夹";
            // 
            // MenuItem_dp2ZServer_openAppDir
            // 
            this.MenuItem_dp2ZServer_openAppDir.Name = "MenuItem_dp2ZServer_openAppDir";
            this.MenuItem_dp2ZServer_openAppDir.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_dp2ZServer_openAppDir.Text = "打开程序文件夹";
            this.MenuItem_dp2ZServer_openAppDir.Click += new System.EventHandler(this.MenuItem_dp2ZServer_openAppDir_Click);
            // 
            // toolStripSeparator20
            // 
            this.toolStripSeparator20.Name = "toolStripSeparator20";
            this.toolStripSeparator20.Size = new System.Drawing.Size(357, 6);
            // 
            // MenuItem_dp2ZServer_instanceManagement
            // 
            this.MenuItem_dp2ZServer_instanceManagement.Name = "MenuItem_dp2ZServer_instanceManagement";
            this.MenuItem_dp2ZServer_instanceManagement.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_dp2ZServer_instanceManagement.Text = "配置实例";
            this.MenuItem_dp2ZServer_instanceManagement.Click += new System.EventHandler(this.MenuItem_dp2ZServer_instanceManagement_Click);
            // 
            // toolStripSeparator21
            // 
            this.toolStripSeparator21.Name = "toolStripSeparator21";
            this.toolStripSeparator21.Size = new System.Drawing.Size(357, 6);
            this.toolStripSeparator21.Visible = false;
            // 
            // MenuItem_dp2ZServer_upgradeCfgs
            // 
            this.MenuItem_dp2ZServer_upgradeCfgs.Name = "MenuItem_dp2ZServer_upgradeCfgs";
            this.MenuItem_dp2ZServer_upgradeCfgs.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_dp2ZServer_upgradeCfgs.Text = "更新数据目录的配置文件";
            this.MenuItem_dp2ZServer_upgradeCfgs.Visible = false;
            // 
            // MenuItem_dp2lZServer_verifySerialNumbers
            // 
            this.MenuItem_dp2lZServer_verifySerialNumbers.Name = "MenuItem_dp2lZServer_verifySerialNumbers";
            this.MenuItem_dp2lZServer_verifySerialNumbers.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_dp2lZServer_verifySerialNumbers.Text = "检查序列号(&S)";
            this.MenuItem_dp2lZServer_verifySerialNumbers.Visible = false;
            // 
            // toolStripSeparator22
            // 
            this.toolStripSeparator22.Name = "toolStripSeparator22";
            this.toolStripSeparator22.Size = new System.Drawing.Size(357, 6);
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
            this.MenuItem_dp2ZServer_tools.Size = new System.Drawing.Size(360, 40);
            this.MenuItem_dp2ZServer_tools.Text = "工具";
            // 
            // MenuItem_dp2ZServer_startService
            // 
            this.MenuItem_dp2ZServer_startService.Name = "MenuItem_dp2ZServer_startService";
            this.MenuItem_dp2ZServer_startService.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_dp2ZServer_startService.Text = "启动 Windows Service";
            this.MenuItem_dp2ZServer_startService.Click += new System.EventHandler(this.MenuItem_dp2ZServer_startService_Click);
            // 
            // MenuItem_dp2ZServer_stopService
            // 
            this.MenuItem_dp2ZServer_stopService.Name = "MenuItem_dp2ZServer_stopService";
            this.MenuItem_dp2ZServer_stopService.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_dp2ZServer_stopService.Text = "停止 Windows Service";
            this.MenuItem_dp2ZServer_stopService.Click += new System.EventHandler(this.MenuItem_dp2ZServer_stopService_Click);
            // 
            // toolStripSeparator23
            // 
            this.toolStripSeparator23.Name = "toolStripSeparator23";
            this.toolStripSeparator23.Size = new System.Drawing.Size(345, 6);
            // 
            // MenuItem_dp2ZServer_installService
            // 
            this.MenuItem_dp2ZServer_installService.Name = "MenuItem_dp2ZServer_installService";
            this.MenuItem_dp2ZServer_installService.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_dp2ZServer_installService.Text = "注册 Windows Service";
            this.MenuItem_dp2ZServer_installService.Click += new System.EventHandler(this.MenuItem_dp2ZServer_installService_Click);
            // 
            // MenuItem_dp2ZServer_uninstallService
            // 
            this.MenuItem_dp2ZServer_uninstallService.Name = "MenuItem_dp2ZServer_uninstallService";
            this.MenuItem_dp2ZServer_uninstallService.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_dp2ZServer_uninstallService.Text = "注销 Windows Service";
            this.MenuItem_dp2ZServer_uninstallService.Click += new System.EventHandler(this.MenuItem_dp2ZServer_uninstallService_Click);
            // 
            // toolStripSeparator26
            // 
            this.toolStripSeparator26.Name = "toolStripSeparator26";
            this.toolStripSeparator26.Size = new System.Drawing.Size(345, 6);
            // 
            // MenuItem_dp2ZServer_uninstall
            // 
            this.MenuItem_dp2ZServer_uninstall.Name = "MenuItem_dp2ZServer_uninstall";
            this.MenuItem_dp2ZServer_uninstall.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_dp2ZServer_uninstall.Text = "卸载 dp2ZServer";
            this.MenuItem_dp2ZServer_uninstall.Click += new System.EventHandler(this.MenuItem_dp2ZServer_uninstall_Click);
            // 
            // MenuItem_palmCenter
            // 
            this.MenuItem_palmCenter.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_palmCenter_install,
            this.MenuItem_palmCenter_update,
            this.toolStripSeparator33,
            this.MenuItem_palmCenter_openDataDir,
            this.MenuItem_palmCenter_openProgramFolder,
            this.toolStripSeparator34,
            this.MenuItem_palmCenter_config,
            this.toolStripSeparator35,
            this.MenuItem_palmCenter_tool});
            this.MenuItem_palmCenter.Name = "MenuItem_palmCenter";
            this.MenuItem_palmCenter.Size = new System.Drawing.Size(148, 32);
            this.MenuItem_palmCenter.Text = "palmCenter";
            // 
            // MenuItem_palmCenter_install
            // 
            this.MenuItem_palmCenter_install.Name = "MenuItem_palmCenter_install";
            this.MenuItem_palmCenter_install.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_palmCenter_install.Text = "安装 palmCenter";
            this.MenuItem_palmCenter_install.Click += new System.EventHandler(this.MenuItem_palmCenter_install_Click);
            // 
            // MenuItem_palmCenter_update
            // 
            this.MenuItem_palmCenter_update.Name = "MenuItem_palmCenter_update";
            this.MenuItem_palmCenter_update.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_palmCenter_update.Text = "更新 palmCenter";
            this.MenuItem_palmCenter_update.Click += new System.EventHandler(this.MenuItem_palmCenter_update_Click);
            // 
            // toolStripSeparator33
            // 
            this.toolStripSeparator33.Name = "toolStripSeparator33";
            this.toolStripSeparator33.Size = new System.Drawing.Size(312, 6);
            // 
            // MenuItem_palmCenter_openProgramFolder
            // 
            this.MenuItem_palmCenter_openProgramFolder.Name = "MenuItem_palmCenter_openProgramFolder";
            this.MenuItem_palmCenter_openProgramFolder.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_palmCenter_openProgramFolder.Text = "打开程序文件夹";
            this.MenuItem_palmCenter_openProgramFolder.Click += new System.EventHandler(this.MenuItem_palmCenter_openProgramFolder_Click);
            // 
            // toolStripSeparator34
            // 
            this.toolStripSeparator34.Name = "toolStripSeparator34";
            this.toolStripSeparator34.Size = new System.Drawing.Size(312, 6);
            // 
            // MenuItem_palmCenter_config
            // 
            this.MenuItem_palmCenter_config.Name = "MenuItem_palmCenter_config";
            this.MenuItem_palmCenter_config.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_palmCenter_config.Text = "配置";
            this.MenuItem_palmCenter_config.Click += new System.EventHandler(this.MenuItem_palmCenter_config_Click);
            // 
            // toolStripSeparator35
            // 
            this.toolStripSeparator35.Name = "toolStripSeparator35";
            this.toolStripSeparator35.Size = new System.Drawing.Size(312, 6);
            this.toolStripSeparator35.Visible = false;
            // 
            // MenuItem_palmCenter_tool
            // 
            this.MenuItem_palmCenter_tool.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_palmCenter_startService,
            this.MenuItem_palmCenter_stopService,
            this.toolStripSeparator36,
            this.MenuItem_palmCenter_installService,
            this.MenuItem_palmCenter_uninstallService,
            this.toolStripSeparator37,
            this.MenuItem_palmCenter_uninstall});
            this.MenuItem_palmCenter_tool.Name = "MenuItem_palmCenter_tool";
            this.MenuItem_palmCenter_tool.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_palmCenter_tool.Text = "工具";
            // 
            // MenuItem_palmCenter_startService
            // 
            this.MenuItem_palmCenter_startService.Name = "MenuItem_palmCenter_startService";
            this.MenuItem_palmCenter_startService.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_palmCenter_startService.Text = "启动 Windows Service";
            this.MenuItem_palmCenter_startService.Click += new System.EventHandler(this.MenuItem_palmCenter_startService_Click);
            // 
            // MenuItem_palmCenter_stopService
            // 
            this.MenuItem_palmCenter_stopService.Name = "MenuItem_palmCenter_stopService";
            this.MenuItem_palmCenter_stopService.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_palmCenter_stopService.Text = "停止 Windows Service";
            this.MenuItem_palmCenter_stopService.Click += new System.EventHandler(this.MenuItem_palmCenter_stopService_Click);
            // 
            // toolStripSeparator36
            // 
            this.toolStripSeparator36.Name = "toolStripSeparator36";
            this.toolStripSeparator36.Size = new System.Drawing.Size(345, 6);
            // 
            // MenuItem_palmCenter_installService
            // 
            this.MenuItem_palmCenter_installService.Name = "MenuItem_palmCenter_installService";
            this.MenuItem_palmCenter_installService.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_palmCenter_installService.Text = "注册 Windows Service";
            this.MenuItem_palmCenter_installService.Click += new System.EventHandler(this.MenuItem_palmCenter_installService_Click);
            // 
            // MenuItem_palmCenter_uninstallService
            // 
            this.MenuItem_palmCenter_uninstallService.Name = "MenuItem_palmCenter_uninstallService";
            this.MenuItem_palmCenter_uninstallService.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_palmCenter_uninstallService.Text = "注销 Windows Service";
            this.MenuItem_palmCenter_uninstallService.Click += new System.EventHandler(this.MenuItem_palmCenter_uninstallService_Click);
            // 
            // toolStripSeparator37
            // 
            this.toolStripSeparator37.Name = "toolStripSeparator37";
            this.toolStripSeparator37.Size = new System.Drawing.Size(345, 6);
            // 
            // MenuItem_palmCenter_uninstall
            // 
            this.MenuItem_palmCenter_uninstall.Name = "MenuItem_palmCenter_uninstall";
            this.MenuItem_palmCenter_uninstall.Size = new System.Drawing.Size(348, 40);
            this.MenuItem_palmCenter_uninstall.Text = "卸载 palmCenter";
            this.MenuItem_palmCenter_uninstall.Click += new System.EventHandler(this.MenuItem_palmCenter_uninstall_Click);
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
            this.MenuItem_help.Size = new System.Drawing.Size(102, 32);
            this.MenuItem_help.Text = "帮助(&H)";
            // 
            // MenuItem_openUserFolder
            // 
            this.MenuItem_openUserFolder.Name = "MenuItem_openUserFolder";
            this.MenuItem_openUserFolder.Size = new System.Drawing.Size(437, 40);
            this.MenuItem_openUserFolder.Text = "打开 dp2Installer 用户文件夹(&U)";
            this.MenuItem_openUserFolder.Click += new System.EventHandler(this.MenuItem_openUserFolder_Click);
            // 
            // MenuItem_openDataFolder
            // 
            this.MenuItem_openDataFolder.Name = "MenuItem_openDataFolder";
            this.MenuItem_openDataFolder.Size = new System.Drawing.Size(437, 40);
            this.MenuItem_openDataFolder.Text = "打开 dp2Installer 数据文件夹(&D)";
            this.MenuItem_openDataFolder.Click += new System.EventHandler(this.MenuItem_openDataFolder_Click);
            // 
            // MenuItem_openProgramFolder
            // 
            this.MenuItem_openProgramFolder.Name = "MenuItem_openProgramFolder";
            this.MenuItem_openProgramFolder.Size = new System.Drawing.Size(437, 40);
            this.MenuItem_openProgramFolder.Text = "打开 dp2Installer 程序文件夹(&P)";
            this.MenuItem_openProgramFolder.Click += new System.EventHandler(this.MenuItem_openProgramFolder_Click);
            // 
            // toolStripSeparator25
            // 
            this.toolStripSeparator25.Name = "toolStripSeparator25";
            this.toolStripSeparator25.Size = new System.Drawing.Size(434, 6);
            // 
            // ToolStripMenuItem_getMD5ofFile
            // 
            this.ToolStripMenuItem_getMD5ofFile.Name = "ToolStripMenuItem_getMD5ofFile";
            this.ToolStripMenuItem_getMD5ofFile.Size = new System.Drawing.Size(437, 40);
            this.ToolStripMenuItem_getMD5ofFile.Text = "获得 MD5 校验码 ...";
            this.ToolStripMenuItem_getMD5ofFile.Click += new System.EventHandler(this.ToolStripMenuItem_getMD5ofFile_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(434, 6);
            // 
            // ToolStripMenuItem_uninstallDp2zserver
            // 
            this.ToolStripMenuItem_uninstallDp2zserver.Name = "ToolStripMenuItem_uninstallDp2zserver";
            this.ToolStripMenuItem_uninstallDp2zserver.Size = new System.Drawing.Size(437, 40);
            this.ToolStripMenuItem_uninstallDp2zserver.Text = "卸载 dp2ZServer";
            this.ToolStripMenuItem_uninstallDp2zserver.Click += new System.EventHandler(this.ToolStripMenuItem_uninstallDp2zserver_Click);
            // 
            // toolStripSeparator24
            // 
            this.toolStripSeparator24.Name = "toolStripSeparator24";
            this.toolStripSeparator24.Size = new System.Drawing.Size(434, 6);
            // 
            // MenuItem_copyright
            // 
            this.MenuItem_copyright.Name = "MenuItem_copyright";
            this.MenuItem_copyright.Size = new System.Drawing.Size(437, 40);
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
            this.statusStrip_main.Location = new System.Drawing.Point(0, 602);
            this.statusStrip_main.Name = "statusStrip_main";
            this.statusStrip_main.Padding = new System.Windows.Forms.Padding(2, 0, 26, 0);
            this.statusStrip_main.RenderMode = System.Windows.Forms.ToolStripRenderMode.ManagerRenderMode;
            this.statusStrip_main.Size = new System.Drawing.Size(794, 33);
            this.statusStrip_main.TabIndex = 3;
            this.statusStrip_main.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_main
            // 
            this.toolStripStatusLabel_main.Name = "toolStripStatusLabel_main";
            this.toolStripStatusLabel_main.Size = new System.Drawing.Size(579, 24);
            this.toolStripStatusLabel_main.Spring = true;
            // 
            // toolStripProgressBar_main
            // 
            this.toolStripProgressBar_main.AutoSize = false;
            this.toolStripProgressBar_main.BackColor = System.Drawing.SystemColors.Control;
            this.toolStripProgressBar_main.Name = "toolStripProgressBar_main";
            this.toolStripProgressBar_main.Size = new System.Drawing.Size(183, 23);
            this.toolStripProgressBar_main.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolButton_stop,
            this.toolStripDropDownButton_stopAll});
            this.toolStrip1.Location = new System.Drawing.Point(0, 71);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(794, 34);
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
            this.toolButton_stop.Size = new System.Drawing.Size(40, 28);
            this.toolButton_stop.Text = "停止";
            // 
            // toolStripDropDownButton_stopAll
            // 
            this.toolStripDropDownButton_stopAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButton_stopAll.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_stopAll});
            this.toolStripDropDownButton_stopAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_stopAll.Name = "toolStripDropDownButton_stopAll";
            this.toolStripDropDownButton_stopAll.Size = new System.Drawing.Size(21, 28);
            this.toolStripDropDownButton_stopAll.Text = "停止全部";
            // 
            // ToolStripMenuItem_stopAll
            // 
            this.ToolStripMenuItem_stopAll.Name = "ToolStripMenuItem_stopAll";
            this.ToolStripMenuItem_stopAll.Size = new System.Drawing.Size(242, 40);
            this.ToolStripMenuItem_stopAll.Text = "停止全部(&A)";
            this.ToolStripMenuItem_stopAll.ToolTipText = "停止全部正在处理的操作";
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 105);
            this.webBrowser1.Margin = new System.Windows.Forms.Padding(5);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(37, 35);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(794, 497);
            this.webBrowser1.TabIndex = 4;
            // 
            // MenuItem_palmCenter_openDataDir
            // 
            this.MenuItem_palmCenter_openDataDir.Name = "MenuItem_palmCenter_openDataDir";
            this.MenuItem_palmCenter_openDataDir.Size = new System.Drawing.Size(315, 40);
            this.MenuItem_palmCenter_openDataDir.Text = "打开数据文件夹";
            this.MenuItem_palmCenter_openDataDir.Click += new System.EventHandler(this.MenuItem_palmCenter_openDataDir_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.ClientSize = new System.Drawing.Size(794, 635);
            this.Controls.Add(this.webBrowser1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.statusStrip_main);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(5);
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
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2library_update;
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
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2kernel_update;
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
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2ZServer_update;
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
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2Commander;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2Commander_install;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2Commander_upgrade;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator27;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2Commander_openAppDir;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator28;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2Commander_settings;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator29;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator30;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem10;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2Commander_startService;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2Commander_stopService;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator31;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2Commander_installService;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2Commander_uninstallService;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator32;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_dp2Commander_uninstall;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_palmCenter;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_palmCenter_install;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_palmCenter_update;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator33;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_palmCenter_openProgramFolder;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator34;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_palmCenter_config;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator35;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_palmCenter_tool;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_palmCenter_startService;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_palmCenter_stopService;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator36;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_palmCenter_installService;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_palmCenter_uninstallService;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator37;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_palmCenter_uninstall;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_palmCenter_openDataDir;
    }
}

