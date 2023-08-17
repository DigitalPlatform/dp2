namespace dp2ManageCenter
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
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.MenuItem_file = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_serversSetting = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_outputFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_configOutputFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_resetOutputFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openOutputFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_config = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_exit = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_management = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_newBackupTasks = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_continueBackupTasks = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.MenuItem_changePassword = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_refreshServerName = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_message = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_searchShelf = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_getFile = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_sendCommand = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItem_messageAccounts = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_chat = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_compactShelf = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_help = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openUserFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openDataFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.MenuItem_openProgramFolder = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel_message = new System.Windows.Forms.ToolStripStatusLabel();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_backupTasks = new System.Windows.Forms.TabPage();
            this.splitContainer_backupTasks = new System.Windows.Forms.SplitContainer();
            this.listView_backupTasks = new System.Windows.Forms.ListView();
            this.columnHeader_libraryName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_state = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_startTime = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_progress = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_serverFiles = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.webBrowser_backupTask = new System.Windows.Forms.WebBrowser();
            this.tabPage_operLogTasks = new System.Windows.Forms.TabPage();
            this.listView_operLogTasks = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage_errorLogTasks = new System.Windows.Forms.TabPage();
            this.listView_errorLogTasks = new System.Windows.Forms.ListView();
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader7 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader8 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage_kernelErrorLogTasks = new System.Windows.Forms.TabPage();
            this.listView_kernelErrorLogTasks = new System.Windows.Forms.ListView();
            this.columnHeader9 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader10 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader11 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader12 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage_history = new System.Windows.Forms.TabPage();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.tabControl_main.SuspendLayout();
            this.tabPage_backupTasks.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_backupTasks)).BeginInit();
            this.splitContainer_backupTasks.Panel1.SuspendLayout();
            this.splitContainer_backupTasks.Panel2.SuspendLayout();
            this.splitContainer_backupTasks.SuspendLayout();
            this.tabPage_operLogTasks.SuspendLayout();
            this.tabPage_errorLogTasks.SuspendLayout();
            this.tabPage_kernelErrorLogTasks.SuspendLayout();
            this.tabPage_history.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.GripMargin = new System.Windows.Forms.Padding(2, 2, 0, 2);
            this.menuStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_file,
            this.MenuItem_management,
            this.toolStripMenuItem_message,
            this.MenuItem_help});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Padding = new System.Windows.Forms.Padding(7, 3, 0, 3);
            this.menuStrip1.Size = new System.Drawing.Size(978, 39);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // MenuItem_file
            // 
            this.MenuItem_file.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_serversSetting,
            this.MenuItem_outputFolder,
            this.MenuItem_config,
            this.toolStripSeparator1,
            this.MenuItem_exit});
            this.MenuItem_file.Name = "MenuItem_file";
            this.MenuItem_file.Size = new System.Drawing.Size(97, 33);
            this.MenuItem_file.Text = "文件(&F)";
            // 
            // MenuItem_serversSetting
            // 
            this.MenuItem_serversSetting.Name = "MenuItem_serversSetting";
            this.MenuItem_serversSetting.Size = new System.Drawing.Size(369, 40);
            this.MenuItem_serversSetting.Text = "设置 dp2library 服务器 ...";
            this.MenuItem_serversSetting.Click += new System.EventHandler(this.MenuItem_serversSetting_Click);
            // 
            // MenuItem_outputFolder
            // 
            this.MenuItem_outputFolder.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_configOutputFolder,
            this.MenuItem_resetOutputFolder,
            this.MenuItem_openOutputFolder});
            this.MenuItem_outputFolder.Name = "MenuItem_outputFolder";
            this.MenuItem_outputFolder.Size = new System.Drawing.Size(369, 40);
            this.MenuItem_outputFolder.Text = "输出目录";
            // 
            // MenuItem_configOutputFolder
            // 
            this.MenuItem_configOutputFolder.Name = "MenuItem_configOutputFolder";
            this.MenuItem_configOutputFolder.Size = new System.Drawing.Size(297, 40);
            this.MenuItem_configOutputFolder.Text = "设置输出目录 ...";
            this.MenuItem_configOutputFolder.Click += new System.EventHandler(this.MenuItem_configOutputFolder_Click);
            // 
            // MenuItem_resetOutputFolder
            // 
            this.MenuItem_resetOutputFolder.Name = "MenuItem_resetOutputFolder";
            this.MenuItem_resetOutputFolder.Size = new System.Drawing.Size(297, 40);
            this.MenuItem_resetOutputFolder.Text = "恢复默认输出目录";
            this.MenuItem_resetOutputFolder.Click += new System.EventHandler(this.MenuItem_resetOutputFolder_Click);
            // 
            // MenuItem_openOutputFolder
            // 
            this.MenuItem_openOutputFolder.Name = "MenuItem_openOutputFolder";
            this.MenuItem_openOutputFolder.Size = new System.Drawing.Size(297, 40);
            this.MenuItem_openOutputFolder.Text = "打开文件夹";
            this.MenuItem_openOutputFolder.Click += new System.EventHandler(this.MenuItem_openOutputFolder_Click);
            // 
            // MenuItem_config
            // 
            this.MenuItem_config.Name = "MenuItem_config";
            this.MenuItem_config.Size = new System.Drawing.Size(369, 40);
            this.MenuItem_config.Text = "参数设置 ...";
            this.MenuItem_config.Click += new System.EventHandler(this.MenuItem_config_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(366, 6);
            // 
            // MenuItem_exit
            // 
            this.MenuItem_exit.Name = "MenuItem_exit";
            this.MenuItem_exit.Size = new System.Drawing.Size(369, 40);
            this.MenuItem_exit.Text = "退出(&X)";
            this.MenuItem_exit.Click += new System.EventHandler(this.MenuItem_exit_Click);
            // 
            // MenuItem_management
            // 
            this.MenuItem_management.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_newBackupTasks,
            this.MenuItem_continueBackupTasks,
            this.toolStripSeparator2,
            this.MenuItem_changePassword,
            this.MenuItem_refreshServerName});
            this.MenuItem_management.Name = "MenuItem_management";
            this.MenuItem_management.Size = new System.Drawing.Size(107, 33);
            this.MenuItem_management.Text = "管理(&M)";
            // 
            // MenuItem_newBackupTasks
            // 
            this.MenuItem_newBackupTasks.Name = "MenuItem_newBackupTasks";
            this.MenuItem_newBackupTasks.Size = new System.Drawing.Size(318, 40);
            this.MenuItem_newBackupTasks.Text = "新建大备份任务(&B)...";
            this.MenuItem_newBackupTasks.Click += new System.EventHandler(this.MenuItem_newBackupTasks_Click);
            // 
            // MenuItem_continueBackupTasks
            // 
            this.MenuItem_continueBackupTasks.Name = "MenuItem_continueBackupTasks";
            this.MenuItem_continueBackupTasks.Size = new System.Drawing.Size(318, 40);
            this.MenuItem_continueBackupTasks.Text = "重启大备份下载(&C)";
            this.MenuItem_continueBackupTasks.Click += new System.EventHandler(this.MenuItem_continueBackupTasks_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(315, 6);
            // 
            // MenuItem_changePassword
            // 
            this.MenuItem_changePassword.Name = "MenuItem_changePassword";
            this.MenuItem_changePassword.Size = new System.Drawing.Size(318, 40);
            this.MenuItem_changePassword.Text = "修改密码(&P)";
            this.MenuItem_changePassword.Click += new System.EventHandler(this.MenuItem_changePassword_Click);
            // 
            // MenuItem_refreshServerName
            // 
            this.MenuItem_refreshServerName.Name = "MenuItem_refreshServerName";
            this.MenuItem_refreshServerName.Size = new System.Drawing.Size(318, 40);
            this.MenuItem_refreshServerName.Text = "刷新服务器名(&R)";
            this.MenuItem_refreshServerName.ToolTipText = "从 dp2library 服务器获取图书馆名，作为服务器名";
            this.MenuItem_refreshServerName.Click += new System.EventHandler(this.MenuItem_refreshServerName_Click);
            // 
            // toolStripMenuItem_message
            // 
            this.toolStripMenuItem_message.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_searchShelf,
            this.toolStripMenuItem_getFile,
            this.toolStripMenuItem_sendCommand,
            this.toolStripSeparator3,
            this.ToolStripMenuItem_messageAccounts,
            this.ToolStripMenuItem_chat,
            this.ToolStripMenuItem_compactShelf});
            this.toolStripMenuItem_message.Name = "toolStripMenuItem_message";
            this.toolStripMenuItem_message.Size = new System.Drawing.Size(72, 33);
            this.toolStripMenuItem_message.Text = "消息";
            // 
            // ToolStripMenuItem_searchShelf
            // 
            this.ToolStripMenuItem_searchShelf.Name = "ToolStripMenuItem_searchShelf";
            this.ToolStripMenuItem_searchShelf.Size = new System.Drawing.Size(315, 40);
            this.ToolStripMenuItem_searchShelf.Text = "书柜查询(&S)";
            this.ToolStripMenuItem_searchShelf.Click += new System.EventHandler(this.ToolStripMenuItem_searchShelf_Click);
            // 
            // toolStripMenuItem_getFile
            // 
            this.toolStripMenuItem_getFile.Name = "toolStripMenuItem_getFile";
            this.toolStripMenuItem_getFile.Size = new System.Drawing.Size(315, 40);
            this.toolStripMenuItem_getFile.Text = "获取文件(&G)";
            this.toolStripMenuItem_getFile.Click += new System.EventHandler(this.toolStripMenuItem_getFile_Click);
            // 
            // toolStripMenuItem_sendCommand
            // 
            this.toolStripMenuItem_sendCommand.Name = "toolStripMenuItem_sendCommand";
            this.toolStripMenuItem_sendCommand.Size = new System.Drawing.Size(315, 40);
            this.toolStripMenuItem_sendCommand.Text = "发送命令(&M)";
            this.toolStripMenuItem_sendCommand.Click += new System.EventHandler(this.toolStripMenuItem_sendCommand_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(312, 6);
            // 
            // ToolStripMenuItem_messageAccounts
            // 
            this.ToolStripMenuItem_messageAccounts.Name = "ToolStripMenuItem_messageAccounts";
            this.ToolStripMenuItem_messageAccounts.Size = new System.Drawing.Size(315, 40);
            this.ToolStripMenuItem_messageAccounts.Text = "设置消息账户";
            this.ToolStripMenuItem_messageAccounts.Click += new System.EventHandler(this.ToolStripMenuItem_messageAccounts_Click);
            // 
            // ToolStripMenuItem_chat
            // 
            this.ToolStripMenuItem_chat.Name = "ToolStripMenuItem_chat";
            this.ToolStripMenuItem_chat.Size = new System.Drawing.Size(315, 40);
            this.ToolStripMenuItem_chat.Text = "聊天";
            this.ToolStripMenuItem_chat.Click += new System.EventHandler(this.ToolStripMenuItem_chat_Click);
            // 
            // ToolStripMenuItem_compactShelf
            // 
            this.ToolStripMenuItem_compactShelf.Name = "ToolStripMenuItem_compactShelf";
            this.ToolStripMenuItem_compactShelf.Size = new System.Drawing.Size(315, 40);
            this.ToolStripMenuItem_compactShelf.Text = "密集书架服务(&C)";
            this.ToolStripMenuItem_compactShelf.Click += new System.EventHandler(this.ToolStripMenuItem_compactShelf_Click);
            // 
            // MenuItem_help
            // 
            this.MenuItem_help.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_openUserFolder,
            this.MenuItem_openDataFolder,
            this.MenuItem_openProgramFolder});
            this.MenuItem_help.Name = "MenuItem_help";
            this.MenuItem_help.Size = new System.Drawing.Size(102, 33);
            this.MenuItem_help.Text = "帮助(&H)";
            // 
            // MenuItem_openUserFolder
            // 
            this.MenuItem_openUserFolder.Name = "MenuItem_openUserFolder";
            this.MenuItem_openUserFolder.Size = new System.Drawing.Size(306, 40);
            this.MenuItem_openUserFolder.Text = "打开用户文件夹(&U)";
            this.MenuItem_openUserFolder.Click += new System.EventHandler(this.MenuItem_openUserFolder_Click);
            // 
            // MenuItem_openDataFolder
            // 
            this.MenuItem_openDataFolder.Name = "MenuItem_openDataFolder";
            this.MenuItem_openDataFolder.Size = new System.Drawing.Size(306, 40);
            this.MenuItem_openDataFolder.Text = "打开数据文件夹(&D)";
            this.MenuItem_openDataFolder.Click += new System.EventHandler(this.MenuItem_openDataFolder_Click);
            // 
            // MenuItem_openProgramFolder
            // 
            this.MenuItem_openProgramFolder.Name = "MenuItem_openProgramFolder";
            this.MenuItem_openProgramFolder.Size = new System.Drawing.Size(306, 40);
            this.MenuItem_openProgramFolder.Text = "打开程序文件夹(&P)";
            this.MenuItem_openProgramFolder.Click += new System.EventHandler(this.MenuItem_openProgramFolder_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Location = new System.Drawing.Point(0, 39);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(978, 25);
            this.toolStrip1.TabIndex = 1;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel_message});
            this.statusStrip1.Location = new System.Drawing.Point(0, 563);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 17, 0);
            this.statusStrip1.Size = new System.Drawing.Size(978, 37);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel_message
            // 
            this.toolStripStatusLabel_message.Name = "toolStripStatusLabel_message";
            this.toolStripStatusLabel_message.Size = new System.Drawing.Size(27, 28);
            this.toolStripStatusLabel_message.Text = "...";
            // 
            // tabControl_main
            // 
            this.tabControl_main.Controls.Add(this.tabPage_backupTasks);
            this.tabControl_main.Controls.Add(this.tabPage_operLogTasks);
            this.tabControl_main.Controls.Add(this.tabPage_errorLogTasks);
            this.tabControl_main.Controls.Add(this.tabPage_kernelErrorLogTasks);
            this.tabControl_main.Controls.Add(this.tabPage_history);
            this.tabControl_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_main.Location = new System.Drawing.Point(0, 64);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(978, 499);
            this.tabControl_main.TabIndex = 2;
            // 
            // tabPage_backupTasks
            // 
            this.tabPage_backupTasks.Controls.Add(this.splitContainer_backupTasks);
            this.tabPage_backupTasks.Location = new System.Drawing.Point(4, 37);
            this.tabPage_backupTasks.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_backupTasks.Name = "tabPage_backupTasks";
            this.tabPage_backupTasks.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage_backupTasks.Size = new System.Drawing.Size(970, 458);
            this.tabPage_backupTasks.TabIndex = 0;
            this.tabPage_backupTasks.Text = "大备份任务";
            this.tabPage_backupTasks.UseVisualStyleBackColor = true;
            // 
            // splitContainer_backupTasks
            // 
            this.splitContainer_backupTasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_backupTasks.Location = new System.Drawing.Point(4, 4);
            this.splitContainer_backupTasks.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer_backupTasks.Name = "splitContainer_backupTasks";
            // 
            // splitContainer_backupTasks.Panel1
            // 
            this.splitContainer_backupTasks.Panel1.Controls.Add(this.listView_backupTasks);
            // 
            // splitContainer_backupTasks.Panel2
            // 
            this.splitContainer_backupTasks.Panel2.Controls.Add(this.webBrowser_backupTask);
            this.splitContainer_backupTasks.Size = new System.Drawing.Size(962, 450);
            this.splitContainer_backupTasks.SplitterDistance = 514;
            this.splitContainer_backupTasks.SplitterWidth = 10;
            this.splitContainer_backupTasks.TabIndex = 1;
            // 
            // listView_backupTasks
            // 
            this.listView_backupTasks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_libraryName,
            this.columnHeader_state,
            this.columnHeader_startTime,
            this.columnHeader_progress,
            this.columnHeader_serverFiles});
            this.listView_backupTasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_backupTasks.FullRowSelect = true;
            this.listView_backupTasks.HideSelection = false;
            this.listView_backupTasks.Location = new System.Drawing.Point(0, 0);
            this.listView_backupTasks.Margin = new System.Windows.Forms.Padding(4);
            this.listView_backupTasks.Name = "listView_backupTasks";
            this.listView_backupTasks.ShowItemToolTips = true;
            this.listView_backupTasks.Size = new System.Drawing.Size(514, 450);
            this.listView_backupTasks.TabIndex = 0;
            this.listView_backupTasks.UseCompatibleStateImageBehavior = false;
            this.listView_backupTasks.View = System.Windows.Forms.View.Details;
            this.listView_backupTasks.SelectedIndexChanged += new System.EventHandler(this.listView_backupTasks_SelectedIndexChanged);
            this.listView_backupTasks.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_backupTasks_MouseUp);
            // 
            // columnHeader_libraryName
            // 
            this.columnHeader_libraryName.Text = "服务器名";
            this.columnHeader_libraryName.Width = 205;
            // 
            // columnHeader_state
            // 
            this.columnHeader_state.Text = "任务状态";
            this.columnHeader_state.Width = 171;
            // 
            // columnHeader_startTime
            // 
            this.columnHeader_startTime.Text = "启动时间";
            this.columnHeader_startTime.Width = 184;
            // 
            // columnHeader_progress
            // 
            this.columnHeader_progress.Text = "进度";
            this.columnHeader_progress.Width = 196;
            // 
            // columnHeader_serverFiles
            // 
            this.columnHeader_serverFiles.Text = "备份文件名";
            this.columnHeader_serverFiles.Width = 300;
            // 
            // webBrowser_backupTask
            // 
            this.webBrowser_backupTask.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_backupTask.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_backupTask.Margin = new System.Windows.Forms.Padding(5);
            this.webBrowser_backupTask.MinimumSize = new System.Drawing.Size(29, 36);
            this.webBrowser_backupTask.Name = "webBrowser_backupTask";
            this.webBrowser_backupTask.Size = new System.Drawing.Size(438, 450);
            this.webBrowser_backupTask.TabIndex = 4;
            // 
            // tabPage_operLogTasks
            // 
            this.tabPage_operLogTasks.Controls.Add(this.listView_operLogTasks);
            this.tabPage_operLogTasks.Location = new System.Drawing.Point(4, 37);
            this.tabPage_operLogTasks.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_operLogTasks.Name = "tabPage_operLogTasks";
            this.tabPage_operLogTasks.Size = new System.Drawing.Size(970, 436);
            this.tabPage_operLogTasks.TabIndex = 2;
            this.tabPage_operLogTasks.Text = "日备份任务";
            this.tabPage_operLogTasks.UseVisualStyleBackColor = true;
            // 
            // listView_operLogTasks
            // 
            this.listView_operLogTasks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.listView_operLogTasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_operLogTasks.FullRowSelect = true;
            this.listView_operLogTasks.HideSelection = false;
            this.listView_operLogTasks.Location = new System.Drawing.Point(0, 0);
            this.listView_operLogTasks.Margin = new System.Windows.Forms.Padding(4);
            this.listView_operLogTasks.Name = "listView_operLogTasks";
            this.listView_operLogTasks.ShowItemToolTips = true;
            this.listView_operLogTasks.Size = new System.Drawing.Size(970, 436);
            this.listView_operLogTasks.TabIndex = 1;
            this.listView_operLogTasks.UseCompatibleStateImageBehavior = false;
            this.listView_operLogTasks.View = System.Windows.Forms.View.Details;
            this.listView_operLogTasks.SelectedIndexChanged += new System.EventHandler(this.listView_operLogTasks_SelectedIndexChanged);
            this.listView_operLogTasks.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_operLogTasks_MouseUp);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "服务器名";
            this.columnHeader1.Width = 205;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "任务状态";
            this.columnHeader2.Width = 171;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "启动时间";
            this.columnHeader3.Width = 184;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "进度";
            this.columnHeader4.Width = 196;
            // 
            // tabPage_errorLogTasks
            // 
            this.tabPage_errorLogTasks.Controls.Add(this.listView_errorLogTasks);
            this.tabPage_errorLogTasks.Location = new System.Drawing.Point(4, 37);
            this.tabPage_errorLogTasks.Name = "tabPage_errorLogTasks";
            this.tabPage_errorLogTasks.Size = new System.Drawing.Size(970, 436);
            this.tabPage_errorLogTasks.TabIndex = 3;
            this.tabPage_errorLogTasks.Text = "错误日志";
            this.tabPage_errorLogTasks.UseVisualStyleBackColor = true;
            // 
            // listView_errorLogTasks
            // 
            this.listView_errorLogTasks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeader7,
            this.columnHeader8});
            this.listView_errorLogTasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_errorLogTasks.FullRowSelect = true;
            this.listView_errorLogTasks.HideSelection = false;
            this.listView_errorLogTasks.Location = new System.Drawing.Point(0, 0);
            this.listView_errorLogTasks.Margin = new System.Windows.Forms.Padding(4);
            this.listView_errorLogTasks.Name = "listView_errorLogTasks";
            this.listView_errorLogTasks.ShowItemToolTips = true;
            this.listView_errorLogTasks.Size = new System.Drawing.Size(970, 436);
            this.listView_errorLogTasks.TabIndex = 2;
            this.listView_errorLogTasks.UseCompatibleStateImageBehavior = false;
            this.listView_errorLogTasks.View = System.Windows.Forms.View.Details;
            this.listView_errorLogTasks.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_errorLogTasks_MouseUp);
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "服务器名";
            this.columnHeader5.Width = 205;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "任务状态";
            this.columnHeader6.Width = 171;
            // 
            // columnHeader7
            // 
            this.columnHeader7.Text = "启动时间";
            this.columnHeader7.Width = 184;
            // 
            // columnHeader8
            // 
            this.columnHeader8.Text = "进度";
            this.columnHeader8.Width = 196;
            // 
            // tabPage_kernelErrorLogTasks
            // 
            this.tabPage_kernelErrorLogTasks.Controls.Add(this.listView_kernelErrorLogTasks);
            this.tabPage_kernelErrorLogTasks.Location = new System.Drawing.Point(4, 37);
            this.tabPage_kernelErrorLogTasks.Name = "tabPage_kernelErrorLogTasks";
            this.tabPage_kernelErrorLogTasks.Size = new System.Drawing.Size(970, 436);
            this.tabPage_kernelErrorLogTasks.TabIndex = 4;
            this.tabPage_kernelErrorLogTasks.Text = "dp2kernel 错误日志";
            this.tabPage_kernelErrorLogTasks.UseVisualStyleBackColor = true;
            // 
            // listView_kernelErrorLogTasks
            // 
            this.listView_kernelErrorLogTasks.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader9,
            this.columnHeader10,
            this.columnHeader11,
            this.columnHeader12});
            this.listView_kernelErrorLogTasks.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_kernelErrorLogTasks.FullRowSelect = true;
            this.listView_kernelErrorLogTasks.HideSelection = false;
            this.listView_kernelErrorLogTasks.Location = new System.Drawing.Point(0, 0);
            this.listView_kernelErrorLogTasks.Margin = new System.Windows.Forms.Padding(4);
            this.listView_kernelErrorLogTasks.Name = "listView_kernelErrorLogTasks";
            this.listView_kernelErrorLogTasks.ShowItemToolTips = true;
            this.listView_kernelErrorLogTasks.Size = new System.Drawing.Size(970, 436);
            this.listView_kernelErrorLogTasks.TabIndex = 3;
            this.listView_kernelErrorLogTasks.UseCompatibleStateImageBehavior = false;
            this.listView_kernelErrorLogTasks.View = System.Windows.Forms.View.Details;
            this.listView_kernelErrorLogTasks.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_kernelErrorLogTasks_MouseUp);
            // 
            // columnHeader9
            // 
            this.columnHeader9.Text = "服务器名";
            this.columnHeader9.Width = 205;
            // 
            // columnHeader10
            // 
            this.columnHeader10.Text = "任务状态";
            this.columnHeader10.Width = 171;
            // 
            // columnHeader11
            // 
            this.columnHeader11.Text = "启动时间";
            this.columnHeader11.Width = 184;
            // 
            // columnHeader12
            // 
            this.columnHeader12.Text = "进度";
            this.columnHeader12.Width = 196;
            // 
            // tabPage_history
            // 
            this.tabPage_history.Controls.Add(this.webBrowser1);
            this.tabPage_history.Location = new System.Drawing.Point(4, 37);
            this.tabPage_history.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_history.Name = "tabPage_history";
            this.tabPage_history.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage_history.Size = new System.Drawing.Size(970, 436);
            this.tabPage_history.TabIndex = 1;
            this.tabPage_history.Text = "操作历史";
            this.tabPage_history.UseVisualStyleBackColor = true;
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(4, 4);
            this.webBrowser1.Margin = new System.Windows.Forms.Padding(5);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(29, 36);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(962, 428);
            this.webBrowser1.TabIndex = 3;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(13F, 28F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(978, 600);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.MainMenuStrip = this.menuStrip1;
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "MainForm";
            this.ShowIcon = false;
            this.Text = "dp2 管理中心";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_backupTasks.ResumeLayout(false);
            this.splitContainer_backupTasks.Panel1.ResumeLayout(false);
            this.splitContainer_backupTasks.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_backupTasks)).EndInit();
            this.splitContainer_backupTasks.ResumeLayout(false);
            this.tabPage_operLogTasks.ResumeLayout(false);
            this.tabPage_errorLogTasks.ResumeLayout(false);
            this.tabPage_kernelErrorLogTasks.ResumeLayout(false);
            this.tabPage_history.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_backupTasks;
        private System.Windows.Forms.TabPage tabPage_history;
        private System.Windows.Forms.ListView listView_backupTasks;
        private System.Windows.Forms.ColumnHeader columnHeader_libraryName;
        private System.Windows.Forms.ColumnHeader columnHeader_state;
        private System.Windows.Forms.ColumnHeader columnHeader_startTime;
        private System.Windows.Forms.ColumnHeader columnHeader_progress;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_file;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_exit;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_serversSetting;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_management;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_newBackupTasks;
        private System.Windows.Forms.ColumnHeader columnHeader_serverFiles;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_continueBackupTasks;
        private System.Windows.Forms.SplitContainer splitContainer_backupTasks;
        private System.Windows.Forms.WebBrowser webBrowser_backupTask;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_message;
        private System.Windows.Forms.TabPage tabPage_operLogTasks;
        private System.Windows.Forms.ListView listView_operLogTasks;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_outputFolder;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_configOutputFolder;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_resetOutputFolder;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openOutputFolder;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_changePassword;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_refreshServerName;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_config;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_help;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openUserFolder;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openDataFolder;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_openProgramFolder;
        private System.Windows.Forms.TabPage tabPage_errorLogTasks;
        private System.Windows.Forms.ListView listView_errorLogTasks;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.ColumnHeader columnHeader7;
        private System.Windows.Forms.ColumnHeader columnHeader8;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_message;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_searchShelf;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_messageAccounts;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_chat;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_getFile;
        private System.Windows.Forms.TabPage tabPage_kernelErrorLogTasks;
        private System.Windows.Forms.ListView listView_kernelErrorLogTasks;
        private System.Windows.Forms.ColumnHeader columnHeader9;
        private System.Windows.Forms.ColumnHeader columnHeader10;
        private System.Windows.Forms.ColumnHeader columnHeader11;
        private System.Windows.Forms.ColumnHeader columnHeader12;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_compactShelf;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_sendCommand;
    }
}

