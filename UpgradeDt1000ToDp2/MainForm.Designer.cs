namespace UpgradeDt1000ToDp2
{
    partial class MainForm
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.menuStrip_main = new System.Windows.Forms.MenuStrip();
            this.statusStrip_main = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip_main = new System.Windows.Forms.ToolStrip();
            this.toolButton_stop = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolButton_openDataFolder = new System.Windows.Forms.ToolStripButton();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_inputDt1000ServerInfo = new System.Windows.Forms.TabPage();
            this.label4 = new System.Windows.Forms.Label();
            this.checkBox_dtlpSavePassword = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_dtlpPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_dtlpUserName = new System.Windows.Forms.TextBox();
            this.textBox_dtlpAsAddress = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage_locateGisIniFile = new System.Windows.Forms.TabPage();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_gisIniFileContent = new System.Windows.Forms.TextBox();
            this.label_gisIniFileContent = new System.Windows.Forms.Label();
            this.button_autoSearchGisIniFilePath = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.button_inputGisIniFlePath = new System.Windows.Forms.Button();
            this.textBox_gisIniFilePath = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tabPage_selectDt1000Database = new System.Windows.Forms.TabPage();
            this.label_selectedDatabasesCount = new System.Windows.Forms.Label();
            this.button_unSelectAllDtlpDatabase = new System.Windows.Forms.Button();
            this.button_selectAllDtlpDatabase = new System.Windows.Forms.Button();
            this.listView_dtlpDatabases = new System.Windows.Forms.ListView();
            this.columnHeader_dtlpDatabaseName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_type = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_inCirculation = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button_setDtlpDatabaseProperty = new System.Windows.Forms.Button();
            this.tabPage_inputDp2ServerInfo = new System.Windows.Forms.TabPage();
            this.label10 = new System.Windows.Forms.Label();
            this.checkBox_dp2SavePassword = new System.Windows.Forms.CheckBox();
            this.label11 = new System.Windows.Forms.Label();
            this.textBox_dp2Password = new System.Windows.Forms.TextBox();
            this.label12 = new System.Windows.Forms.Label();
            this.textBox_dp2UserName = new System.Windows.Forms.TextBox();
            this.textBox_dp2AsUrl = new System.Windows.Forms.TextBox();
            this.label13 = new System.Windows.Forms.Label();
            this.tabPage_createTargetDatabase = new System.Windows.Forms.TabPage();
            this.splitContainer_createDp2Database = new System.Windows.Forms.SplitContainer();
            this.label9 = new System.Windows.Forms.Label();
            this.listView_creatingDp2DatabaseList = new System.Windows.Forms.ListView();
            this.columnHeader_createDp2Database_databaseName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_createDp2Database_type = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_createDp2Database_existing = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label8 = new System.Windows.Forms.Label();
            this.textBox_createDp2DatabaseSummary = new System.Windows.Forms.TextBox();
            this.tabPage_copyDatabase = new System.Windows.Forms.TabPage();
            this.checkBox_copyDatabase_checkEntityDup = new System.Windows.Forms.CheckBox();
            this.tabPage_verifyLoan = new System.Windows.Forms.TabPage();
            this.tabPage_upgradeReaderRights = new System.Windows.Forms.TabPage();
            this.button_rights_getCfgFromDtlpServer = new System.Windows.Forms.Button();
            this.label14 = new System.Windows.Forms.Label();
            this.textBox_rights_ltqxCfgContent = new System.Windows.Forms.TextBox();
            this.label_rights_ltqxCfgFileContent = new System.Windows.Forms.Label();
            this.button_rights_findLtqxCfgFilename = new System.Windows.Forms.Button();
            this.textBox_rights_ltxqCfgFilePath = new System.Windows.Forms.TextBox();
            this.label16 = new System.Windows.Forms.Label();
            this.webBrowser_info = new System.Windows.Forms.WebBrowser();
            this.button_next = new System.Windows.Forms.Button();
            this.imageList_resIcon16 = new System.Windows.Forms.ImageList(this.components);
            this.panel_main = new System.Windows.Forms.Panel();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.statusStrip_main.SuspendLayout();
            this.toolStrip_main.SuspendLayout();
            this.tabControl_main.SuspendLayout();
            this.tabPage_inputDt1000ServerInfo.SuspendLayout();
            this.tabPage_locateGisIniFile.SuspendLayout();
            this.tabPage_selectDt1000Database.SuspendLayout();
            this.tabPage_inputDp2ServerInfo.SuspendLayout();
            this.tabPage_createTargetDatabase.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_createDp2Database)).BeginInit();
            this.splitContainer_createDp2Database.Panel1.SuspendLayout();
            this.splitContainer_createDp2Database.Panel2.SuspendLayout();
            this.splitContainer_createDp2Database.SuspendLayout();
            this.tabPage_copyDatabase.SuspendLayout();
            this.tabPage_upgradeReaderRights.SuspendLayout();
            this.panel_main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip_main
            // 
            this.menuStrip_main.Location = new System.Drawing.Point(0, 0);
            this.menuStrip_main.Name = "menuStrip_main";
            this.menuStrip_main.Padding = new System.Windows.Forms.Padding(4, 2, 0, 2);
            this.menuStrip_main.Size = new System.Drawing.Size(454, 24);
            this.menuStrip_main.TabIndex = 0;
            this.menuStrip_main.Text = "menuStrip1";
            // 
            // statusStrip_main
            // 
            this.statusStrip_main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabel1});
            this.statusStrip_main.Location = new System.Drawing.Point(0, 407);
            this.statusStrip_main.Name = "statusStrip_main";
            this.statusStrip_main.Padding = new System.Windows.Forms.Padding(1, 0, 10, 0);
            this.statusStrip_main.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusStrip_main.Size = new System.Drawing.Size(454, 26);
            this.statusStrip_main.SizingGrip = false;
            this.statusStrip_main.TabIndex = 1;
            this.statusStrip_main.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.AutoSize = false;
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 20);
            this.toolStripProgressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(341, 21);
            this.toolStripStatusLabel1.Spring = true;
            this.toolStripStatusLabel1.Text = "test ";
            this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStrip_main
            // 
            this.toolStrip_main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolButton_stop,
            this.toolStripSeparator1,
            this.toolButton_openDataFolder});
            this.toolStrip_main.Location = new System.Drawing.Point(0, 24);
            this.toolStrip_main.Name = "toolStrip_main";
            this.toolStrip_main.Size = new System.Drawing.Size(454, 29);
            this.toolStrip_main.TabIndex = 2;
            this.toolStrip_main.Text = "toolStrip1";
            // 
            // toolButton_stop
            // 
            this.toolButton_stop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolButton_stop.Enabled = false;
            this.toolButton_stop.Image = ((System.Drawing.Image)(resources.GetObject("toolButton_stop.Image")));
            this.toolButton_stop.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolButton_stop.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolButton_stop.Name = "toolButton_stop";
            this.toolButton_stop.Size = new System.Drawing.Size(26, 26);
            this.toolButton_stop.Text = "停止";
            this.toolButton_stop.Click += new System.EventHandler(this.toolButton_stop_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 29);
            // 
            // toolButton_openDataFolder
            // 
            this.toolButton_openDataFolder.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolButton_openDataFolder.Image = ((System.Drawing.Image)(resources.GetObject("toolButton_openDataFolder.Image")));
            this.toolButton_openDataFolder.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolButton_openDataFolder.Name = "toolButton_openDataFolder";
            this.toolButton_openDataFolder.Size = new System.Drawing.Size(23, 26);
            this.toolButton_openDataFolder.Text = "打开数据目录文件夹";
            this.toolButton_openDataFolder.Click += new System.EventHandler(this.toolButton_openDataFolder_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_inputDt1000ServerInfo);
            this.tabControl_main.Controls.Add(this.tabPage_locateGisIniFile);
            this.tabControl_main.Controls.Add(this.tabPage_selectDt1000Database);
            this.tabControl_main.Controls.Add(this.tabPage_inputDp2ServerInfo);
            this.tabControl_main.Controls.Add(this.tabPage_createTargetDatabase);
            this.tabControl_main.Controls.Add(this.tabPage_copyDatabase);
            this.tabControl_main.Controls.Add(this.tabPage_verifyLoan);
            this.tabControl_main.Controls.Add(this.tabPage_upgradeReaderRights);
            this.tabControl_main.Location = new System.Drawing.Point(0, 2);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(449, 195);
            this.tabControl_main.TabIndex = 3;
            this.tabControl_main.SelectedIndexChanged += new System.EventHandler(this.tabControl_main_SelectedIndexChanged);
            // 
            // tabPage_inputDt1000ServerInfo
            // 
            this.tabPage_inputDt1000ServerInfo.Controls.Add(this.label4);
            this.tabPage_inputDt1000ServerInfo.Controls.Add(this.checkBox_dtlpSavePassword);
            this.tabPage_inputDt1000ServerInfo.Controls.Add(this.label2);
            this.tabPage_inputDt1000ServerInfo.Controls.Add(this.textBox_dtlpPassword);
            this.tabPage_inputDt1000ServerInfo.Controls.Add(this.label3);
            this.tabPage_inputDt1000ServerInfo.Controls.Add(this.textBox_dtlpUserName);
            this.tabPage_inputDt1000ServerInfo.Controls.Add(this.textBox_dtlpAsAddress);
            this.tabPage_inputDt1000ServerInfo.Controls.Add(this.label1);
            this.tabPage_inputDt1000ServerInfo.Location = new System.Drawing.Point(4, 22);
            this.tabPage_inputDt1000ServerInfo.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_inputDt1000ServerInfo.Name = "tabPage_inputDt1000ServerInfo";
            this.tabPage_inputDt1000ServerInfo.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_inputDt1000ServerInfo.Size = new System.Drawing.Size(441, 169);
            this.tabPage_inputDt1000ServerInfo.TabIndex = 0;
            this.tabPage_inputDt1000ServerInfo.Text = "输入dt1000服务器信息";
            this.tabPage_inputDt1000ServerInfo.UseVisualStyleBackColor = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.Red;
            this.label4.Location = new System.Drawing.Point(210, 79);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(233, 12);
            this.label4.TabIndex = 12;
            this.label4.Text = "(请使用具有最高权限的用户名，例如root)";
            // 
            // checkBox_dtlpSavePassword
            // 
            this.checkBox_dtlpSavePassword.Location = new System.Drawing.Point(72, 130);
            this.checkBox_dtlpSavePassword.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBox_dtlpSavePassword.Name = "checkBox_dtlpSavePassword";
            this.checkBox_dtlpSavePassword.Size = new System.Drawing.Size(126, 19);
            this.checkBox_dtlpSavePassword.TabIndex = 11;
            this.checkBox_dtlpSavePassword.Text = "记住密码(&R)";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(6, 79);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(54, 18);
            this.label2.TabIndex = 7;
            this.label2.Text = "用户名:";
            // 
            // textBox_dtlpPassword
            // 
            this.textBox_dtlpPassword.Location = new System.Drawing.Point(72, 105);
            this.textBox_dtlpPassword.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_dtlpPassword.Name = "textBox_dtlpPassword";
            this.textBox_dtlpPassword.PasswordChar = '*';
            this.textBox_dtlpPassword.Size = new System.Drawing.Size(127, 21);
            this.textBox_dtlpPassword.TabIndex = 10;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(6, 111);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(54, 18);
            this.label3.TabIndex = 9;
            this.label3.Text = "密码:";
            // 
            // textBox_dtlpUserName
            // 
            this.textBox_dtlpUserName.Location = new System.Drawing.Point(72, 73);
            this.textBox_dtlpUserName.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_dtlpUserName.Name = "textBox_dtlpUserName";
            this.textBox_dtlpUserName.Size = new System.Drawing.Size(127, 21);
            this.textBox_dtlpUserName.TabIndex = 8;
            // 
            // textBox_dtlpAsAddress
            // 
            this.textBox_dtlpAsAddress.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dtlpAsAddress.Location = new System.Drawing.Point(5, 30);
            this.textBox_dtlpAsAddress.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_dtlpAsAddress.Name = "textBox_dtlpAsAddress";
            this.textBox_dtlpAsAddress.Size = new System.Drawing.Size(433, 21);
            this.textBox_dtlpAsAddress.TabIndex = 1;
            this.textBox_dtlpAsAddress.Text = "localhost";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 16);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "应用服务器地址(&A):";
            // 
            // tabPage_locateGisIniFile
            // 
            this.tabPage_locateGisIniFile.Controls.Add(this.label7);
            this.tabPage_locateGisIniFile.Controls.Add(this.textBox_gisIniFileContent);
            this.tabPage_locateGisIniFile.Controls.Add(this.label_gisIniFileContent);
            this.tabPage_locateGisIniFile.Controls.Add(this.button_autoSearchGisIniFilePath);
            this.tabPage_locateGisIniFile.Controls.Add(this.label6);
            this.tabPage_locateGisIniFile.Controls.Add(this.button_inputGisIniFlePath);
            this.tabPage_locateGisIniFile.Controls.Add(this.textBox_gisIniFilePath);
            this.tabPage_locateGisIniFile.Controls.Add(this.label5);
            this.tabPage_locateGisIniFile.Location = new System.Drawing.Point(4, 22);
            this.tabPage_locateGisIniFile.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_locateGisIniFile.Name = "tabPage_locateGisIniFile";
            this.tabPage_locateGisIniFile.Size = new System.Drawing.Size(441, 179);
            this.tabPage_locateGisIniFile.TabIndex = 2;
            this.tabPage_locateGisIniFile.Text = "定位gis.ini配置文件";
            this.tabPage_locateGisIniFile.UseVisualStyleBackColor = true;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label7.Location = new System.Drawing.Point(4, 108);
            this.label7.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(233, 12);
            this.label7.TabIndex = 7;
            this.label7.Text = "或直接将文件内容粘贴到下面文本框中:";
            // 
            // textBox_gisIniFileContent
            // 
            this.textBox_gisIniFileContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_gisIniFileContent.Location = new System.Drawing.Point(5, 139);
            this.textBox_gisIniFileContent.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_gisIniFileContent.Multiline = true;
            this.textBox_gisIniFileContent.Name = "textBox_gisIniFileContent";
            this.textBox_gisIniFileContent.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_gisIniFileContent.Size = new System.Drawing.Size(438, 44);
            this.textBox_gisIniFileContent.TabIndex = 6;
            // 
            // label_gisIniFileContent
            // 
            this.label_gisIniFileContent.AutoSize = true;
            this.label_gisIniFileContent.Location = new System.Drawing.Point(3, 125);
            this.label_gisIniFileContent.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_gisIniFileContent.Name = "label_gisIniFileContent";
            this.label_gisIniFileContent.Size = new System.Drawing.Size(191, 12);
            this.label_gisIniFileContent.TabIndex = 5;
            this.label_gisIniFileContent.Text = "gis.ini(或gis2000.ini)文件内容:";
            // 
            // button_autoSearchGisIniFilePath
            // 
            this.button_autoSearchGisIniFilePath.Location = new System.Drawing.Point(6, 70);
            this.button_autoSearchGisIniFilePath.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_autoSearchGisIniFilePath.Name = "button_autoSearchGisIniFilePath";
            this.button_autoSearchGisIniFilePath.Size = new System.Drawing.Size(296, 22);
            this.button_autoSearchGisIniFilePath.TabIndex = 4;
            this.button_autoSearchGisIniFilePath.Text = "查找gis.ini(或gis2000.ini)文件的全路径(&S)";
            this.button_autoSearchGisIniFilePath.UseVisualStyleBackColor = true;
            this.button_autoSearchGisIniFilePath.Click += new System.EventHandler(this.button_autoSearchGisIniFilePath_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label6.Location = new System.Drawing.Point(4, 56);
            this.label6.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(137, 12);
            this.label6.TabIndex = 3;
            this.label6.Text = "或由软件自动查找(&A):";
            // 
            // button_inputGisIniFlePath
            // 
            this.button_inputGisIniFlePath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_inputGisIniFlePath.Location = new System.Drawing.Point(404, 25);
            this.button_inputGisIniFlePath.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_inputGisIniFlePath.Name = "button_inputGisIniFlePath";
            this.button_inputGisIniFlePath.Size = new System.Drawing.Size(38, 22);
            this.button_inputGisIniFlePath.TabIndex = 2;
            this.button_inputGisIniFlePath.Text = "...";
            this.button_inputGisIniFlePath.UseVisualStyleBackColor = true;
            this.button_inputGisIniFlePath.Click += new System.EventHandler(this.button_inputGisIniFlePath_Click);
            // 
            // textBox_gisIniFilePath
            // 
            this.textBox_gisIniFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_gisIniFilePath.Location = new System.Drawing.Point(6, 25);
            this.textBox_gisIniFilePath.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_gisIniFilePath.Name = "textBox_gisIniFilePath";
            this.textBox_gisIniFilePath.Size = new System.Drawing.Size(396, 21);
            this.textBox_gisIniFilePath.TabIndex = 1;
            this.textBox_gisIniFilePath.TextChanged += new System.EventHandler(this.textBox_gisIniFilePath_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label5.Location = new System.Drawing.Point(3, 10);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(329, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "请指定gis.ini(或gis2000.ini)配置文件的全路径(&P):";
            // 
            // tabPage_selectDt1000Database
            // 
            this.tabPage_selectDt1000Database.Controls.Add(this.label_selectedDatabasesCount);
            this.tabPage_selectDt1000Database.Controls.Add(this.button_unSelectAllDtlpDatabase);
            this.tabPage_selectDt1000Database.Controls.Add(this.button_selectAllDtlpDatabase);
            this.tabPage_selectDt1000Database.Controls.Add(this.listView_dtlpDatabases);
            this.tabPage_selectDt1000Database.Controls.Add(this.button_setDtlpDatabaseProperty);
            this.tabPage_selectDt1000Database.Location = new System.Drawing.Point(4, 22);
            this.tabPage_selectDt1000Database.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_selectDt1000Database.Name = "tabPage_selectDt1000Database";
            this.tabPage_selectDt1000Database.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_selectDt1000Database.Size = new System.Drawing.Size(441, 169);
            this.tabPage_selectDt1000Database.TabIndex = 1;
            this.tabPage_selectDt1000Database.Text = "选定要升级的dt1000数据库";
            this.tabPage_selectDt1000Database.UseVisualStyleBackColor = true;
            // 
            // label_selectedDatabasesCount
            // 
            this.label_selectedDatabasesCount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_selectedDatabasesCount.BackColor = System.Drawing.SystemColors.Info;
            this.label_selectedDatabasesCount.Location = new System.Drawing.Point(4, 129);
            this.label_selectedDatabasesCount.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_selectedDatabasesCount.Name = "label_selectedDatabasesCount";
            this.label_selectedDatabasesCount.Size = new System.Drawing.Size(438, 18);
            this.label_selectedDatabasesCount.TabIndex = 3;
            this.label_selectedDatabasesCount.Text = "您尚未选择要升级的数据库";
            this.label_selectedDatabasesCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // button_unSelectAllDtlpDatabase
            // 
            this.button_unSelectAllDtlpDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_unSelectAllDtlpDatabase.Location = new System.Drawing.Point(86, 148);
            this.button_unSelectAllDtlpDatabase.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_unSelectAllDtlpDatabase.Name = "button_unSelectAllDtlpDatabase";
            this.button_unSelectAllDtlpDatabase.Size = new System.Drawing.Size(75, 22);
            this.button_unSelectAllDtlpDatabase.TabIndex = 2;
            this.button_unSelectAllDtlpDatabase.Text = "全不选(&U)";
            this.button_unSelectAllDtlpDatabase.UseVisualStyleBackColor = true;
            this.button_unSelectAllDtlpDatabase.Click += new System.EventHandler(this.button_unSelectAllDtlpDatabase_Click);
            // 
            // button_selectAllDtlpDatabase
            // 
            this.button_selectAllDtlpDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_selectAllDtlpDatabase.Location = new System.Drawing.Point(5, 148);
            this.button_selectAllDtlpDatabase.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_selectAllDtlpDatabase.Name = "button_selectAllDtlpDatabase";
            this.button_selectAllDtlpDatabase.Size = new System.Drawing.Size(75, 22);
            this.button_selectAllDtlpDatabase.TabIndex = 1;
            this.button_selectAllDtlpDatabase.Text = "全选(&S)";
            this.button_selectAllDtlpDatabase.UseVisualStyleBackColor = true;
            this.button_selectAllDtlpDatabase.Click += new System.EventHandler(this.button_selectAllDtlpDatabase_Click);
            // 
            // listView_dtlpDatabases
            // 
            this.listView_dtlpDatabases.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_dtlpDatabases.CheckBoxes = true;
            this.listView_dtlpDatabases.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_dtlpDatabaseName,
            this.columnHeader_type,
            this.columnHeader_inCirculation});
            this.listView_dtlpDatabases.FullRowSelect = true;
            this.listView_dtlpDatabases.HideSelection = false;
            this.listView_dtlpDatabases.Location = new System.Drawing.Point(6, 13);
            this.listView_dtlpDatabases.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listView_dtlpDatabases.Name = "listView_dtlpDatabases";
            this.listView_dtlpDatabases.Size = new System.Drawing.Size(438, 116);
            this.listView_dtlpDatabases.TabIndex = 0;
            this.listView_dtlpDatabases.UseCompatibleStateImageBehavior = false;
            this.listView_dtlpDatabases.View = System.Windows.Forms.View.Details;
            this.listView_dtlpDatabases.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listView_dtlpDatabases_ItemChecked);
            this.listView_dtlpDatabases.SelectedIndexChanged += new System.EventHandler(this.listView_dtlpDatabases_SelectedIndexChanged);
            this.listView_dtlpDatabases.DoubleClick += new System.EventHandler(this.listView_dtlpDatabases_DoubleClick);
            this.listView_dtlpDatabases.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_dtlpDatabases_MouseUp);
            // 
            // columnHeader_dtlpDatabaseName
            // 
            this.columnHeader_dtlpDatabaseName.Text = "数据库名";
            this.columnHeader_dtlpDatabaseName.Width = 182;
            // 
            // columnHeader_type
            // 
            this.columnHeader_type.Text = "类型";
            this.columnHeader_type.Width = 259;
            // 
            // columnHeader_inCirculation
            // 
            this.columnHeader_inCirculation.Text = "是否参与流通";
            this.columnHeader_inCirculation.Width = 142;
            // 
            // button_setDtlpDatabaseProperty
            // 
            this.button_setDtlpDatabaseProperty.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_setDtlpDatabaseProperty.Enabled = false;
            this.button_setDtlpDatabaseProperty.Location = new System.Drawing.Point(188, 148);
            this.button_setDtlpDatabaseProperty.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_setDtlpDatabaseProperty.Name = "button_setDtlpDatabaseProperty";
            this.button_setDtlpDatabaseProperty.Size = new System.Drawing.Size(254, 22);
            this.button_setDtlpDatabaseProperty.TabIndex = 4;
            this.button_setDtlpDatabaseProperty.Text = "设置数据库的类型(&T)...";
            this.button_setDtlpDatabaseProperty.UseVisualStyleBackColor = true;
            this.button_setDtlpDatabaseProperty.Click += new System.EventHandler(this.button_setDtlpDatabaseProperty_Click);
            // 
            // tabPage_inputDp2ServerInfo
            // 
            this.tabPage_inputDp2ServerInfo.Controls.Add(this.label10);
            this.tabPage_inputDp2ServerInfo.Controls.Add(this.checkBox_dp2SavePassword);
            this.tabPage_inputDp2ServerInfo.Controls.Add(this.label11);
            this.tabPage_inputDp2ServerInfo.Controls.Add(this.textBox_dp2Password);
            this.tabPage_inputDp2ServerInfo.Controls.Add(this.label12);
            this.tabPage_inputDp2ServerInfo.Controls.Add(this.textBox_dp2UserName);
            this.tabPage_inputDp2ServerInfo.Controls.Add(this.textBox_dp2AsUrl);
            this.tabPage_inputDp2ServerInfo.Controls.Add(this.label13);
            this.tabPage_inputDp2ServerInfo.Location = new System.Drawing.Point(4, 22);
            this.tabPage_inputDp2ServerInfo.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_inputDp2ServerInfo.Name = "tabPage_inputDp2ServerInfo";
            this.tabPage_inputDp2ServerInfo.Size = new System.Drawing.Size(441, 179);
            this.tabPage_inputDp2ServerInfo.TabIndex = 4;
            this.tabPage_inputDp2ServerInfo.Text = "输入 dp2library 服务器信息";
            this.tabPage_inputDp2ServerInfo.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.ForeColor = System.Drawing.Color.Red;
            this.label10.Location = new System.Drawing.Point(209, 78);
            this.label10.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(269, 12);
            this.label10.TabIndex = 20;
            this.label10.Text = "(请使用具有最高权限的用户名，例如supervisor)";
            // 
            // checkBox_dp2SavePassword
            // 
            this.checkBox_dp2SavePassword.Location = new System.Drawing.Point(71, 129);
            this.checkBox_dp2SavePassword.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBox_dp2SavePassword.Name = "checkBox_dp2SavePassword";
            this.checkBox_dp2SavePassword.Size = new System.Drawing.Size(126, 19);
            this.checkBox_dp2SavePassword.TabIndex = 19;
            this.checkBox_dp2SavePassword.Text = "记住密码(&R)";
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(5, 78);
            this.label11.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(54, 18);
            this.label11.TabIndex = 15;
            this.label11.Text = "用户名:";
            // 
            // textBox_dp2Password
            // 
            this.textBox_dp2Password.Location = new System.Drawing.Point(71, 103);
            this.textBox_dp2Password.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_dp2Password.Name = "textBox_dp2Password";
            this.textBox_dp2Password.PasswordChar = '*';
            this.textBox_dp2Password.Size = new System.Drawing.Size(127, 21);
            this.textBox_dp2Password.TabIndex = 18;
            // 
            // label12
            // 
            this.label12.Location = new System.Drawing.Point(5, 110);
            this.label12.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(54, 18);
            this.label12.TabIndex = 17;
            this.label12.Text = "密码:";
            // 
            // textBox_dp2UserName
            // 
            this.textBox_dp2UserName.Location = new System.Drawing.Point(71, 71);
            this.textBox_dp2UserName.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_dp2UserName.Name = "textBox_dp2UserName";
            this.textBox_dp2UserName.Size = new System.Drawing.Size(127, 21);
            this.textBox_dp2UserName.TabIndex = 16;
            // 
            // textBox_dp2AsUrl
            // 
            this.textBox_dp2AsUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dp2AsUrl.Location = new System.Drawing.Point(4, 29);
            this.textBox_dp2AsUrl.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_dp2AsUrl.Name = "textBox_dp2AsUrl";
            this.textBox_dp2AsUrl.Size = new System.Drawing.Size(438, 21);
            this.textBox_dp2AsUrl.TabIndex = 14;
            this.textBox_dp2AsUrl.Text = "http://localhost:8001/dp2library";
            // 
            // label13
            // 
            this.label13.AutoSize = true;
            this.label13.Location = new System.Drawing.Point(2, 14);
            this.label13.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label13.Name = "label13";
            this.label13.Size = new System.Drawing.Size(209, 12);
            this.label13.TabIndex = 13;
            this.label13.Text = "图书馆应用服务器WebService URL(&A):";
            // 
            // tabPage_createTargetDatabase
            // 
            this.tabPage_createTargetDatabase.Controls.Add(this.splitContainer_createDp2Database);
            this.tabPage_createTargetDatabase.Location = new System.Drawing.Point(4, 22);
            this.tabPage_createTargetDatabase.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_createTargetDatabase.Name = "tabPage_createTargetDatabase";
            this.tabPage_createTargetDatabase.Size = new System.Drawing.Size(441, 169);
            this.tabPage_createTargetDatabase.TabIndex = 3;
            this.tabPage_createTargetDatabase.Text = "在dp2中创建数据库";
            this.tabPage_createTargetDatabase.UseVisualStyleBackColor = true;
            // 
            // splitContainer_createDp2Database
            // 
            this.splitContainer_createDp2Database.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_createDp2Database.Location = new System.Drawing.Point(0, 11);
            this.splitContainer_createDp2Database.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.splitContainer_createDp2Database.Name = "splitContainer_createDp2Database";
            this.splitContainer_createDp2Database.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_createDp2Database.Panel1
            // 
            this.splitContainer_createDp2Database.Panel1.Controls.Add(this.label9);
            this.splitContainer_createDp2Database.Panel1.Controls.Add(this.listView_creatingDp2DatabaseList);
            // 
            // splitContainer_createDp2Database.Panel2
            // 
            this.splitContainer_createDp2Database.Panel2.Controls.Add(this.label8);
            this.splitContainer_createDp2Database.Panel2.Controls.Add(this.textBox_createDp2DatabaseSummary);
            this.splitContainer_createDp2Database.Size = new System.Drawing.Size(443, 152);
            this.splitContainer_createDp2Database.SplitterDistance = 75;
            this.splitContainer_createDp2Database.SplitterWidth = 6;
            this.splitContainer_createDp2Database.TabIndex = 2;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(-2, 0);
            this.label9.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(131, 12);
            this.label9.TabIndex = 1;
            this.label9.Text = "拟创建的dp2数据库(&D):";
            // 
            // listView_creatingDp2DatabaseList
            // 
            this.listView_creatingDp2DatabaseList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_creatingDp2DatabaseList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_createDp2Database_databaseName,
            this.columnHeader_createDp2Database_type,
            this.columnHeader_createDp2Database_existing});
            this.listView_creatingDp2DatabaseList.FullRowSelect = true;
            this.listView_creatingDp2DatabaseList.HideSelection = false;
            this.listView_creatingDp2DatabaseList.Location = new System.Drawing.Point(0, 14);
            this.listView_creatingDp2DatabaseList.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.listView_creatingDp2DatabaseList.Name = "listView_creatingDp2DatabaseList";
            this.listView_creatingDp2DatabaseList.Size = new System.Drawing.Size(446, 59);
            this.listView_creatingDp2DatabaseList.TabIndex = 0;
            this.listView_creatingDp2DatabaseList.UseCompatibleStateImageBehavior = false;
            this.listView_creatingDp2DatabaseList.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_createDp2Database_databaseName
            // 
            this.columnHeader_createDp2Database_databaseName.Text = "数据库名";
            this.columnHeader_createDp2Database_databaseName.Width = 204;
            // 
            // columnHeader_createDp2Database_type
            // 
            this.columnHeader_createDp2Database_type.Text = "类型";
            this.columnHeader_createDp2Database_type.Width = 280;
            // 
            // columnHeader_createDp2Database_existing
            // 
            this.columnHeader_createDp2Database_existing.Text = "是否已经存在?";
            this.columnHeader_createDp2Database_existing.Width = 128;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(-2, 0);
            this.label8.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(77, 12);
            this.label8.TabIndex = 0;
            this.label8.Text = "创建情况(&I):";
            // 
            // textBox_createDp2DatabaseSummary
            // 
            this.textBox_createDp2DatabaseSummary.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_createDp2DatabaseSummary.Location = new System.Drawing.Point(0, 14);
            this.textBox_createDp2DatabaseSummary.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_createDp2DatabaseSummary.Multiline = true;
            this.textBox_createDp2DatabaseSummary.Name = "textBox_createDp2DatabaseSummary";
            this.textBox_createDp2DatabaseSummary.ReadOnly = true;
            this.textBox_createDp2DatabaseSummary.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_createDp2DatabaseSummary.Size = new System.Drawing.Size(446, 55);
            this.textBox_createDp2DatabaseSummary.TabIndex = 1;
            // 
            // tabPage_copyDatabase
            // 
            this.tabPage_copyDatabase.Controls.Add(this.checkBox_copyDatabase_checkEntityDup);
            this.tabPage_copyDatabase.Location = new System.Drawing.Point(4, 22);
            this.tabPage_copyDatabase.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_copyDatabase.Name = "tabPage_copyDatabase";
            this.tabPage_copyDatabase.Size = new System.Drawing.Size(441, 179);
            this.tabPage_copyDatabase.TabIndex = 5;
            this.tabPage_copyDatabase.Text = "复制dt1000数据库内的数据到dp2";
            this.tabPage_copyDatabase.UseVisualStyleBackColor = true;
            // 
            // checkBox_copyDatabase_checkEntityDup
            // 
            this.checkBox_copyDatabase_checkEntityDup.AutoSize = true;
            this.checkBox_copyDatabase_checkEntityDup.Location = new System.Drawing.Point(4, 17);
            this.checkBox_copyDatabase_checkEntityDup.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBox_copyDatabase_checkEntityDup.Name = "checkBox_copyDatabase_checkEntityDup";
            this.checkBox_copyDatabase_checkEntityDup.Size = new System.Drawing.Size(138, 16);
            this.checkBox_copyDatabase_checkEntityDup.TabIndex = 0;
            this.checkBox_copyDatabase_checkEntityDup.Text = "对册条码进行查重(&C)";
            this.checkBox_copyDatabase_checkEntityDup.UseVisualStyleBackColor = true;
            // 
            // tabPage_verifyLoan
            // 
            this.tabPage_verifyLoan.Location = new System.Drawing.Point(4, 22);
            this.tabPage_verifyLoan.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_verifyLoan.Name = "tabPage_verifyLoan";
            this.tabPage_verifyLoan.Size = new System.Drawing.Size(441, 179);
            this.tabPage_verifyLoan.TabIndex = 6;
            this.tabPage_verifyLoan.Text = "整理流通信息";
            this.tabPage_verifyLoan.UseVisualStyleBackColor = true;
            // 
            // tabPage_upgradeReaderRights
            // 
            this.tabPage_upgradeReaderRights.Controls.Add(this.button_rights_getCfgFromDtlpServer);
            this.tabPage_upgradeReaderRights.Controls.Add(this.label14);
            this.tabPage_upgradeReaderRights.Controls.Add(this.textBox_rights_ltqxCfgContent);
            this.tabPage_upgradeReaderRights.Controls.Add(this.label_rights_ltqxCfgFileContent);
            this.tabPage_upgradeReaderRights.Controls.Add(this.button_rights_findLtqxCfgFilename);
            this.tabPage_upgradeReaderRights.Controls.Add(this.textBox_rights_ltxqCfgFilePath);
            this.tabPage_upgradeReaderRights.Controls.Add(this.label16);
            this.tabPage_upgradeReaderRights.Location = new System.Drawing.Point(4, 22);
            this.tabPage_upgradeReaderRights.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabPage_upgradeReaderRights.Name = "tabPage_upgradeReaderRights";
            this.tabPage_upgradeReaderRights.Size = new System.Drawing.Size(441, 179);
            this.tabPage_upgradeReaderRights.TabIndex = 7;
            this.tabPage_upgradeReaderRights.Text = "升级流通权限参数";
            this.tabPage_upgradeReaderRights.UseVisualStyleBackColor = true;
            // 
            // button_rights_getCfgFromDtlpServer
            // 
            this.button_rights_getCfgFromDtlpServer.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_rights_getCfgFromDtlpServer.Location = new System.Drawing.Point(280, 46);
            this.button_rights_getCfgFromDtlpServer.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_rights_getCfgFromDtlpServer.Name = "button_rights_getCfgFromDtlpServer";
            this.button_rights_getCfgFromDtlpServer.Size = new System.Drawing.Size(160, 22);
            this.button_rights_getCfgFromDtlpServer.TabIndex = 14;
            this.button_rights_getCfgFromDtlpServer.Text = "从dt1000服务器获得(&S)";
            this.button_rights_getCfgFromDtlpServer.UseVisualStyleBackColor = true;
            this.button_rights_getCfgFromDtlpServer.Click += new System.EventHandler(this.button_rights_getCfgFromDtlpServer_Click);
            // 
            // label14
            // 
            this.label14.AutoSize = true;
            this.label14.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label14.Location = new System.Drawing.Point(4, 50);
            this.label14.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label14.Name = "label14";
            this.label14.Size = new System.Drawing.Size(233, 12);
            this.label14.TabIndex = 13;
            this.label14.Text = "或直接将文件内容粘贴到下面文本框中:";
            // 
            // textBox_rights_ltqxCfgContent
            // 
            this.textBox_rights_ltqxCfgContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_rights_ltqxCfgContent.Location = new System.Drawing.Point(4, 76);
            this.textBox_rights_ltqxCfgContent.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_rights_ltqxCfgContent.Multiline = true;
            this.textBox_rights_ltqxCfgContent.Name = "textBox_rights_ltqxCfgContent";
            this.textBox_rights_ltqxCfgContent.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_rights_ltqxCfgContent.Size = new System.Drawing.Size(438, 103);
            this.textBox_rights_ltqxCfgContent.TabIndex = 12;
            // 
            // label_rights_ltqxCfgFileContent
            // 
            this.label_rights_ltqxCfgFileContent.AutoSize = true;
            this.label_rights_ltqxCfgFileContent.Location = new System.Drawing.Point(4, 62);
            this.label_rights_ltqxCfgFileContent.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_rights_ltqxCfgFileContent.Name = "label_rights_ltqxCfgFileContent";
            this.label_rights_ltqxCfgFileContent.Size = new System.Drawing.Size(113, 12);
            this.label_rights_ltqxCfgFileContent.TabIndex = 11;
            this.label_rights_ltqxCfgFileContent.Text = "ltqx*.cfg文件内容:";
            // 
            // button_rights_findLtqxCfgFilename
            // 
            this.button_rights_findLtqxCfgFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_rights_findLtqxCfgFilename.Location = new System.Drawing.Point(403, 21);
            this.button_rights_findLtqxCfgFilename.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_rights_findLtqxCfgFilename.Name = "button_rights_findLtqxCfgFilename";
            this.button_rights_findLtqxCfgFilename.Size = new System.Drawing.Size(38, 22);
            this.button_rights_findLtqxCfgFilename.TabIndex = 10;
            this.button_rights_findLtqxCfgFilename.Text = "...";
            this.button_rights_findLtqxCfgFilename.UseVisualStyleBackColor = true;
            this.button_rights_findLtqxCfgFilename.Click += new System.EventHandler(this.button_rights_findLtqxCfgFilename_Click);
            // 
            // textBox_rights_ltxqCfgFilePath
            // 
            this.textBox_rights_ltxqCfgFilePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_rights_ltxqCfgFilePath.Location = new System.Drawing.Point(5, 21);
            this.textBox_rights_ltxqCfgFilePath.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_rights_ltxqCfgFilePath.Name = "textBox_rights_ltxqCfgFilePath";
            this.textBox_rights_ltxqCfgFilePath.Size = new System.Drawing.Size(396, 21);
            this.textBox_rights_ltxqCfgFilePath.TabIndex = 9;
            this.textBox_rights_ltxqCfgFilePath.TextChanged += new System.EventHandler(this.textBox_rights_ltxqCfgFilename_TextChanged);
            // 
            // label16
            // 
            this.label16.AutoSize = true;
            this.label16.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.label16.Location = new System.Drawing.Point(2, 6);
            this.label16.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label16.Name = "label16";
            this.label16.Size = new System.Drawing.Size(200, 12);
            this.label16.TabIndex = 8;
            this.label16.Text = "ltqx*.cfg配置文件的全路径(&P):";
            // 
            // webBrowser_info
            // 
            this.webBrowser_info.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser_info.Location = new System.Drawing.Point(0, 0);
            this.webBrowser_info.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.webBrowser_info.MinimumSize = new System.Drawing.Size(15, 16);
            this.webBrowser_info.Name = "webBrowser_info";
            this.webBrowser_info.Size = new System.Drawing.Size(449, 119);
            this.webBrowser_info.TabIndex = 0;
            // 
            // button_next
            // 
            this.button_next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_next.AutoSize = true;
            this.button_next.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_next.Location = new System.Drawing.Point(363, 200);
            this.button_next.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_next.Name = "button_next";
            this.button_next.Size = new System.Drawing.Size(86, 25);
            this.button_next.TabIndex = 4;
            this.button_next.Text = "继续(&C)";
            this.button_next.UseVisualStyleBackColor = true;
            this.button_next.Click += new System.EventHandler(this.button_next_Click);
            // 
            // imageList_resIcon16
            // 
            this.imageList_resIcon16.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_resIcon16.ImageStream")));
            this.imageList_resIcon16.TransparentColor = System.Drawing.Color.Fuchsia;
            this.imageList_resIcon16.Images.SetKeyName(0, "");
            this.imageList_resIcon16.Images.SetKeyName(1, "");
            this.imageList_resIcon16.Images.SetKeyName(2, "");
            this.imageList_resIcon16.Images.SetKeyName(3, "");
            this.imageList_resIcon16.Images.SetKeyName(4, "");
            this.imageList_resIcon16.Images.SetKeyName(5, "");
            this.imageList_resIcon16.Images.SetKeyName(6, "");
            this.imageList_resIcon16.Images.SetKeyName(7, "");
            this.imageList_resIcon16.Images.SetKeyName(8, "");
            this.imageList_resIcon16.Images.SetKeyName(9, "");
            this.imageList_resIcon16.Images.SetKeyName(10, "");
            this.imageList_resIcon16.Images.SetKeyName(11, "");
            // 
            // panel_main
            // 
            this.panel_main.Controls.Add(this.splitContainer_main);
            this.panel_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_main.Location = new System.Drawing.Point(0, 53);
            this.panel_main.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.panel_main.Name = "panel_main";
            this.panel_main.Size = new System.Drawing.Size(454, 354);
            this.panel_main.TabIndex = 5;
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(2, 2);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.splitContainer_main.Name = "splitContainer_main";
            this.splitContainer_main.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.tabControl_main);
            this.splitContainer_main.Panel1.Controls.Add(this.button_next);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.webBrowser_info);
            this.splitContainer_main.Size = new System.Drawing.Size(449, 349);
            this.splitContainer_main.SplitterDistance = 224;
            this.splitContainer_main.SplitterWidth = 6;
            this.splitContainer_main.TabIndex = 5;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(454, 433);
            this.Controls.Add(this.panel_main);
            this.Controls.Add(this.toolStrip_main);
            this.Controls.Add(this.statusStrip_main);
            this.Controls.Add(this.menuStrip_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip_main;
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "MainForm";
            this.Text = "升级dt1000到dp2";
            this.Activated += new System.EventHandler(this.MainForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.MainForm_FormClosed);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.statusStrip_main.ResumeLayout(false);
            this.statusStrip_main.PerformLayout();
            this.toolStrip_main.ResumeLayout(false);
            this.toolStrip_main.PerformLayout();
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_inputDt1000ServerInfo.ResumeLayout(false);
            this.tabPage_inputDt1000ServerInfo.PerformLayout();
            this.tabPage_locateGisIniFile.ResumeLayout(false);
            this.tabPage_locateGisIniFile.PerformLayout();
            this.tabPage_selectDt1000Database.ResumeLayout(false);
            this.tabPage_inputDp2ServerInfo.ResumeLayout(false);
            this.tabPage_inputDp2ServerInfo.PerformLayout();
            this.tabPage_createTargetDatabase.ResumeLayout(false);
            this.splitContainer_createDp2Database.Panel1.ResumeLayout(false);
            this.splitContainer_createDp2Database.Panel1.PerformLayout();
            this.splitContainer_createDp2Database.Panel2.ResumeLayout(false);
            this.splitContainer_createDp2Database.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_createDp2Database)).EndInit();
            this.splitContainer_createDp2Database.ResumeLayout(false);
            this.tabPage_copyDatabase.ResumeLayout(false);
            this.tabPage_copyDatabase.PerformLayout();
            this.tabPage_upgradeReaderRights.ResumeLayout(false);
            this.tabPage_upgradeReaderRights.PerformLayout();
            this.panel_main.ResumeLayout(false);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel1.PerformLayout();
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip_main;
        private System.Windows.Forms.StatusStrip statusStrip_main;
        private System.Windows.Forms.ToolStrip toolStrip_main;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_inputDt1000ServerInfo;
        private System.Windows.Forms.TabPage tabPage_selectDt1000Database;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.Button button_next;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_dtlpAsAddress;
        private System.Windows.Forms.CheckBox checkBox_dtlpSavePassword;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_dtlpPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_dtlpUserName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ListView listView_dtlpDatabases;
        private System.Windows.Forms.ColumnHeader columnHeader_dtlpDatabaseName;
        private System.Windows.Forms.ColumnHeader columnHeader_type;
        private System.Windows.Forms.Button button_unSelectAllDtlpDatabase;
        private System.Windows.Forms.Button button_selectAllDtlpDatabase;
        private System.Windows.Forms.TabPage tabPage_locateGisIniFile;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_gisIniFilePath;
        private System.Windows.Forms.Button button_inputGisIniFlePath;
        private System.Windows.Forms.Button button_autoSearchGisIniFilePath;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label_gisIniFileContent;
        private System.Windows.Forms.TextBox textBox_gisIniFileContent;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.ImageList imageList_resIcon16;
        private System.Windows.Forms.ColumnHeader columnHeader_inCirculation;
        private System.Windows.Forms.Label label_selectedDatabasesCount;
        private System.Windows.Forms.Button button_setDtlpDatabaseProperty;
        private System.Windows.Forms.TabPage tabPage_createTargetDatabase;
        private System.Windows.Forms.TextBox textBox_createDp2DatabaseSummary;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.SplitContainer splitContainer_createDp2Database;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.ListView listView_creatingDp2DatabaseList;
        private System.Windows.Forms.ColumnHeader columnHeader_createDp2Database_databaseName;
        private System.Windows.Forms.ColumnHeader columnHeader_createDp2Database_type;
        private System.Windows.Forms.ColumnHeader columnHeader_createDp2Database_existing;
        private System.Windows.Forms.ToolStripButton toolButton_stop;
        private System.Windows.Forms.TabPage tabPage_inputDp2ServerInfo;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox checkBox_dp2SavePassword;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.TextBox textBox_dp2Password;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.TextBox textBox_dp2UserName;
        private System.Windows.Forms.TextBox textBox_dp2AsUrl;
        private System.Windows.Forms.Label label13;
        private System.Windows.Forms.TabPage tabPage_copyDatabase;
        private System.Windows.Forms.WebBrowser webBrowser_info;
        private System.Windows.Forms.TabPage tabPage_verifyLoan;
        private System.Windows.Forms.TabPage tabPage_upgradeReaderRights;
        private System.Windows.Forms.Panel panel_main;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.Label label14;
        private System.Windows.Forms.TextBox textBox_rights_ltqxCfgContent;
        private System.Windows.Forms.Label label_rights_ltqxCfgFileContent;
        private System.Windows.Forms.Button button_rights_findLtqxCfgFilename;
        private System.Windows.Forms.TextBox textBox_rights_ltxqCfgFilePath;
        private System.Windows.Forms.Label label16;
        private System.Windows.Forms.CheckBox checkBox_copyDatabase_checkEntityDup;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolButton_openDataFolder;
        private System.Windows.Forms.Button button_rights_getCfgFromDtlpServer;
    }
}

