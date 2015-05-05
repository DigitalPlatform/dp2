using DigitalPlatform.GUI;


namespace dp2Circulation
{
    partial class ManagerForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ManagerForm));
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_databases = new System.Windows.Forms.TabPage();
            this.toolStrip_databases = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton_create = new System.Windows.Forms.ToolStripDropDownButton();
            this.ToolStripMenuItem_createBiblioDatabase = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_createReaderDatabase = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_createAmerceDatabase = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_createArrivedDatabase = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_createPublisherDatabase = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_createMessageDatabase = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_createZhongcihaoDatabase = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton_modifyDatabase = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_deleteDatabase = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_initializeDatabase = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_refresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_initialAllDatabases = new System.Windows.Forms.ToolStripButton();
            this.listView_databases = new System.Windows.Forms.ListView();
            this.columnHeader_name = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_type = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_comment = new System.Windows.Forms.ColumnHeader();
            this.tabPage_opacDatabases = new System.Windows.Forms.TabPage();
            this.splitContainer_opac = new System.Windows.Forms.SplitContainer();
            this.label1 = new System.Windows.Forms.Label();
            this.toolStrip_opacDatabases = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton_insertOpacDatabase = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem_insertOpacDatabase_normal = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_insertOpacDatabase_virtual = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton_modifyOpacDatabase = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_removeOpacDatabase = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_refreshOpacDatabaseList = new System.Windows.Forms.ToolStripButton();
            this.listView_opacDatabases = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.toolStrip_opacBrowseFormats = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem_opacBrowseFormats_insertDatabaseNameNode = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_opacBrowseFormats_insertBrowseFormatNode = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton_opacBrowseFormats_modify = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_opacBrowseFormats_remove = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_opacBrowseFormats_refresh = new System.Windows.Forms.ToolStripButton();
            this.label2 = new System.Windows.Forms.Label();
            this.treeView_opacBrowseFormats = new System.Windows.Forms.TreeView();
            this.tabPage_dup = new System.Windows.Forms.TabPage();
            this.tabPage_loanPolicy = new System.Windows.Forms.TabPage();
            this.toolStrip_loanPolicy = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_loanPolicy_save = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_loanPolicy_refresh = new System.Windows.Forms.ToolStripButton();
            this.splitContainer_loanPolicy = new System.Windows.Forms.SplitContainer();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_loanPolicy_rightsTableDef = new System.Windows.Forms.TextBox();
            this.webBrowser_rightsTableHtml = new System.Windows.Forms.WebBrowser();
            this.label4 = new System.Windows.Forms.Label();
            this.tabPage_locations = new System.Windows.Forms.TabPage();
            this.textBox_location_comment = new System.Windows.Forms.TextBox();
            this.toolStrip_location = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_location_save = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_location_refresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_location_up = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_location_down = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_location_new = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_location_modify = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_location_delete = new System.Windows.Forms.ToolStripButton();
            this.listView_location_list = new System.Windows.Forms.ListView();
            this.columnHeader_location_name = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_location_canBorrow = new System.Windows.Forms.ColumnHeader();
            this.tabPage_zhongcihaoDatabases = new System.Windows.Forms.TabPage();
            this.toolStrip_zhongcihao = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton_insert = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem_zhongcihao_insert_group = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_zhongcihao_insert_database = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_zhongcihao_insert_nstable = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton_zhongcihao_modify = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_zhongcihao_remove = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_zhongcihao_refresh = new System.Windows.Forms.ToolStripButton();
            this.treeView_zhongcihao = new System.Windows.Forms.TreeView();
            this.tabPage_script = new System.Windows.Forms.TabPage();
            this.splitContainer_script = new System.Windows.Forms.SplitContainer();
            this.textBox_script = new DigitalPlatform.GUI.NoHasSelTextBox();
            this.toolStrip_script = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_script_save = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_script_refresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel_script_caretPos = new System.Windows.Forms.ToolStripLabel();
            this.textBox_script_comment = new DigitalPlatform.GUI.NoHasSelTextBox();
            this.imageList_opacDatabaseType = new System.Windows.Forms.ImageList(this.components);
            this.imageList_opacBrowseFormatType = new System.Windows.Forms.ImageList(this.components);
            this.imageList_zhongcihao = new System.Windows.Forms.ImageList(this.components);
            this.tabControl_main.SuspendLayout();
            this.tabPage_databases.SuspendLayout();
            this.toolStrip_databases.SuspendLayout();
            this.tabPage_opacDatabases.SuspendLayout();
            this.splitContainer_opac.Panel1.SuspendLayout();
            this.splitContainer_opac.Panel2.SuspendLayout();
            this.splitContainer_opac.SuspendLayout();
            this.toolStrip_opacDatabases.SuspendLayout();
            this.toolStrip_opacBrowseFormats.SuspendLayout();
            this.tabPage_loanPolicy.SuspendLayout();
            this.toolStrip_loanPolicy.SuspendLayout();
            this.splitContainer_loanPolicy.Panel1.SuspendLayout();
            this.splitContainer_loanPolicy.Panel2.SuspendLayout();
            this.splitContainer_loanPolicy.SuspendLayout();
            this.tabPage_locations.SuspendLayout();
            this.toolStrip_location.SuspendLayout();
            this.tabPage_zhongcihaoDatabases.SuspendLayout();
            this.toolStrip_zhongcihao.SuspendLayout();
            this.tabPage_script.SuspendLayout();
            this.splitContainer_script.Panel1.SuspendLayout();
            this.splitContainer_script.Panel2.SuspendLayout();
            this.splitContainer_script.SuspendLayout();
            this.toolStrip_script.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_databases);
            this.tabControl_main.Controls.Add(this.tabPage_opacDatabases);
            this.tabControl_main.Controls.Add(this.tabPage_dup);
            this.tabControl_main.Controls.Add(this.tabPage_loanPolicy);
            this.tabControl_main.Controls.Add(this.tabPage_locations);
            this.tabControl_main.Controls.Add(this.tabPage_zhongcihaoDatabases);
            this.tabControl_main.Controls.Add(this.tabPage_script);
            this.tabControl_main.Location = new System.Drawing.Point(0, 13);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(595, 232);
            this.tabControl_main.TabIndex = 1;
            // 
            // tabPage_databases
            // 
            this.tabPage_databases.Controls.Add(this.toolStrip_databases);
            this.tabPage_databases.Controls.Add(this.listView_databases);
            this.tabPage_databases.Location = new System.Drawing.Point(4, 24);
            this.tabPage_databases.Name = "tabPage_databases";
            this.tabPage_databases.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_databases.Size = new System.Drawing.Size(587, 148);
            this.tabPage_databases.TabIndex = 0;
            this.tabPage_databases.Text = "数据库";
            this.tabPage_databases.UseVisualStyleBackColor = true;
            // 
            // toolStrip_databases
            // 
            this.toolStrip_databases.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip_databases.AutoSize = false;
            this.toolStrip_databases.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip_databases.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton_create,
            this.toolStripButton_modifyDatabase,
            this.toolStripButton_deleteDatabase,
            this.toolStripButton_initializeDatabase,
            this.toolStripButton_refresh,
            this.toolStripButton_initialAllDatabases});
            this.toolStrip_databases.Location = new System.Drawing.Point(7, 120);
            this.toolStrip_databases.Name = "toolStrip_databases";
            this.toolStrip_databases.Size = new System.Drawing.Size(574, 25);
            this.toolStrip_databases.TabIndex = 6;
            this.toolStrip_databases.Text = "toolStrip1";
            // 
            // toolStripDropDownButton_create
            // 
            this.toolStripDropDownButton_create.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton_create.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_createBiblioDatabase,
            this.ToolStripMenuItem_createReaderDatabase,
            this.ToolStripMenuItem_createAmerceDatabase,
            this.ToolStripMenuItem_createArrivedDatabase,
            this.ToolStripMenuItem_createPublisherDatabase,
            this.ToolStripMenuItem_createMessageDatabase,
            this.ToolStripMenuItem_createZhongcihaoDatabase});
            this.toolStripDropDownButton_create.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton_create.Image")));
            this.toolStripDropDownButton_create.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_create.Name = "toolStripDropDownButton_create";
            this.toolStripDropDownButton_create.Size = new System.Drawing.Size(51, 22);
            this.toolStripDropDownButton_create.Text = "创建";
            // 
            // ToolStripMenuItem_createBiblioDatabase
            // 
            this.ToolStripMenuItem_createBiblioDatabase.Name = "ToolStripMenuItem_createBiblioDatabase";
            this.ToolStripMenuItem_createBiblioDatabase.Size = new System.Drawing.Size(187, 22);
            this.ToolStripMenuItem_createBiblioDatabase.Text = "书目库(&B)...";
            this.ToolStripMenuItem_createBiblioDatabase.Click += new System.EventHandler(this.ToolStripMenuItem_createBiblioDatabase_Click);
            // 
            // ToolStripMenuItem_createReaderDatabase
            // 
            this.ToolStripMenuItem_createReaderDatabase.Name = "ToolStripMenuItem_createReaderDatabase";
            this.ToolStripMenuItem_createReaderDatabase.Size = new System.Drawing.Size(187, 22);
            this.ToolStripMenuItem_createReaderDatabase.Text = "读者库(&R)...";
            this.ToolStripMenuItem_createReaderDatabase.Click += new System.EventHandler(this.ToolStripMenuItem_createReaderDatabase_Click);
            // 
            // ToolStripMenuItem_createAmerceDatabase
            // 
            this.ToolStripMenuItem_createAmerceDatabase.Name = "ToolStripMenuItem_createAmerceDatabase";
            this.ToolStripMenuItem_createAmerceDatabase.Size = new System.Drawing.Size(187, 22);
            this.ToolStripMenuItem_createAmerceDatabase.Text = "违约金库(&A)...";
            this.ToolStripMenuItem_createAmerceDatabase.Click += new System.EventHandler(this.ToolStripMenuItem_createAmerceDatabase_Click);
            // 
            // ToolStripMenuItem_createArrivedDatabase
            // 
            this.ToolStripMenuItem_createArrivedDatabase.Name = "ToolStripMenuItem_createArrivedDatabase";
            this.ToolStripMenuItem_createArrivedDatabase.Size = new System.Drawing.Size(187, 22);
            this.ToolStripMenuItem_createArrivedDatabase.Text = "预约到书库(&V)...";
            this.ToolStripMenuItem_createArrivedDatabase.Click += new System.EventHandler(this.ToolStripMenuItem_createArrivedDatabase_Click);
            // 
            // ToolStripMenuItem_createPublisherDatabase
            // 
            this.ToolStripMenuItem_createPublisherDatabase.Name = "ToolStripMenuItem_createPublisherDatabase";
            this.ToolStripMenuItem_createPublisherDatabase.Size = new System.Drawing.Size(187, 22);
            this.ToolStripMenuItem_createPublisherDatabase.Text = "出版者库(&P)...";
            this.ToolStripMenuItem_createPublisherDatabase.Click += new System.EventHandler(this.ToolStripMenuItem_createPublisherDatabase_Click);
            // 
            // ToolStripMenuItem_createMessageDatabase
            // 
            this.ToolStripMenuItem_createMessageDatabase.Name = "ToolStripMenuItem_createMessageDatabase";
            this.ToolStripMenuItem_createMessageDatabase.Size = new System.Drawing.Size(187, 22);
            this.ToolStripMenuItem_createMessageDatabase.Text = "消息库(&M)..";
            this.ToolStripMenuItem_createMessageDatabase.Click += new System.EventHandler(this.ToolStripMenuItem_createMessageDatabase_Click);
            // 
            // ToolStripMenuItem_createZhongcihaoDatabase
            // 
            this.ToolStripMenuItem_createZhongcihaoDatabase.Name = "ToolStripMenuItem_createZhongcihaoDatabase";
            this.ToolStripMenuItem_createZhongcihaoDatabase.Size = new System.Drawing.Size(187, 22);
            this.ToolStripMenuItem_createZhongcihaoDatabase.Text = "种次号库(&Z)...";
            this.ToolStripMenuItem_createZhongcihaoDatabase.Click += new System.EventHandler(this.ToolStripMenuItem_createZhongcihaoDatabase_Click);
            // 
            // toolStripButton_modifyDatabase
            // 
            this.toolStripButton_modifyDatabase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_modifyDatabase.Enabled = false;
            this.toolStripButton_modifyDatabase.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_modifyDatabase.Image")));
            this.toolStripButton_modifyDatabase.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_modifyDatabase.Name = "toolStripButton_modifyDatabase";
            this.toolStripButton_modifyDatabase.Size = new System.Drawing.Size(42, 22);
            this.toolStripButton_modifyDatabase.Text = "修改";
            this.toolStripButton_modifyDatabase.Click += new System.EventHandler(this.toolStripButton_modifyDatabase_Click);
            // 
            // toolStripButton_deleteDatabase
            // 
            this.toolStripButton_deleteDatabase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_deleteDatabase.Enabled = false;
            this.toolStripButton_deleteDatabase.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_deleteDatabase.Image")));
            this.toolStripButton_deleteDatabase.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_deleteDatabase.Name = "toolStripButton_deleteDatabase";
            this.toolStripButton_deleteDatabase.Size = new System.Drawing.Size(42, 22);
            this.toolStripButton_deleteDatabase.Text = "删除";
            this.toolStripButton_deleteDatabase.Click += new System.EventHandler(this.toolStripButton_deleteDatabase_Click);
            // 
            // toolStripButton_initializeDatabase
            // 
            this.toolStripButton_initializeDatabase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_initializeDatabase.Enabled = false;
            this.toolStripButton_initializeDatabase.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_initializeDatabase.Image")));
            this.toolStripButton_initializeDatabase.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_initializeDatabase.Name = "toolStripButton_initializeDatabase";
            this.toolStripButton_initializeDatabase.Size = new System.Drawing.Size(57, 22);
            this.toolStripButton_initializeDatabase.Text = "初始化";
            this.toolStripButton_initializeDatabase.Click += new System.EventHandler(this.toolStripButton_initializeDatabase_Click);
            // 
            // toolStripButton_refresh
            // 
            this.toolStripButton_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_refresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_refresh.Image")));
            this.toolStripButton_refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_refresh.Name = "toolStripButton_refresh";
            this.toolStripButton_refresh.Size = new System.Drawing.Size(42, 22);
            this.toolStripButton_refresh.Text = "刷新";
            this.toolStripButton_refresh.Click += new System.EventHandler(this.toolStripButton_refresh_Click);
            // 
            // toolStripButton_initialAllDatabases
            // 
            this.toolStripButton_initialAllDatabases.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_initialAllDatabases.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_initialAllDatabases.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_initialAllDatabases.Image")));
            this.toolStripButton_initialAllDatabases.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_initialAllDatabases.Name = "toolStripButton_initialAllDatabases";
            this.toolStripButton_initialAllDatabases.Size = new System.Drawing.Size(132, 22);
            this.toolStripButton_initialAllDatabases.Text = "初始化所有数据库";
            this.toolStripButton_initialAllDatabases.Click += new System.EventHandler(this.toolStripButton_initialAllDatabases_Click);
            // 
            // listView_databases
            // 
            this.listView_databases.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_databases.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView_databases.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_name,
            this.columnHeader_type,
            this.columnHeader_comment});
            this.listView_databases.FullRowSelect = true;
            this.listView_databases.HideSelection = false;
            this.listView_databases.Location = new System.Drawing.Point(7, 7);
            this.listView_databases.Name = "listView_databases";
            this.listView_databases.Size = new System.Drawing.Size(574, 110);
            this.listView_databases.TabIndex = 1;
            this.listView_databases.UseCompatibleStateImageBehavior = false;
            this.listView_databases.View = System.Windows.Forms.View.Details;
            this.listView_databases.SelectedIndexChanged += new System.EventHandler(this.listView_databases_SelectedIndexChanged);
            this.listView_databases.DoubleClick += new System.EventHandler(this.listView_databases_DoubleClick);
            this.listView_databases.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_databases_MouseUp);
            // 
            // columnHeader_name
            // 
            this.columnHeader_name.Text = "数据库名";
            this.columnHeader_name.Width = 150;
            // 
            // columnHeader_type
            // 
            this.columnHeader_type.Text = "类型";
            this.columnHeader_type.Width = 80;
            // 
            // columnHeader_comment
            // 
            this.columnHeader_comment.Text = "说明";
            this.columnHeader_comment.Width = 300;
            // 
            // tabPage_opacDatabases
            // 
            this.tabPage_opacDatabases.Controls.Add(this.splitContainer_opac);
            this.tabPage_opacDatabases.Location = new System.Drawing.Point(4, 24);
            this.tabPage_opacDatabases.Name = "tabPage_opacDatabases";
            this.tabPage_opacDatabases.Size = new System.Drawing.Size(587, 148);
            this.tabPage_opacDatabases.TabIndex = 1;
            this.tabPage_opacDatabases.Text = "OPAC";
            this.tabPage_opacDatabases.UseVisualStyleBackColor = true;
            // 
            // splitContainer_opac
            // 
            this.splitContainer_opac.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_opac.Location = new System.Drawing.Point(7, 17);
            this.splitContainer_opac.Name = "splitContainer_opac";
            this.splitContainer_opac.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_opac.Panel1
            // 
            this.splitContainer_opac.Panel1.Controls.Add(this.label1);
            this.splitContainer_opac.Panel1.Controls.Add(this.toolStrip_opacDatabases);
            this.splitContainer_opac.Panel1.Controls.Add(this.listView_opacDatabases);
            // 
            // splitContainer_opac.Panel2
            // 
            this.splitContainer_opac.Panel2.Controls.Add(this.toolStrip_opacBrowseFormats);
            this.splitContainer_opac.Panel2.Controls.Add(this.label2);
            this.splitContainer_opac.Panel2.Controls.Add(this.treeView_opacBrowseFormats);
            this.splitContainer_opac.Size = new System.Drawing.Size(574, 295);
            this.splitContainer_opac.SplitterDistance = 144;
            this.splitContainer_opac.SplitterWidth = 16;
            this.splitContainer_opac.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(-3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(191, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "参与OPAC检索的数据库(&D):";
            // 
            // toolStrip_opacDatabases
            // 
            this.toolStrip_opacDatabases.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip_opacDatabases.AutoSize = false;
            this.toolStrip_opacDatabases.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip_opacDatabases.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton_insertOpacDatabase,
            this.toolStripButton_modifyOpacDatabase,
            this.toolStripButton_removeOpacDatabase,
            this.toolStripButton_refreshOpacDatabaseList});
            this.toolStrip_opacDatabases.Location = new System.Drawing.Point(0, 119);
            this.toolStrip_opacDatabases.Name = "toolStrip_opacDatabases";
            this.toolStrip_opacDatabases.Size = new System.Drawing.Size(573, 25);
            this.toolStrip_opacDatabases.TabIndex = 7;
            this.toolStrip_opacDatabases.Text = "toolStrip1";
            // 
            // toolStripDropDownButton_insertOpacDatabase
            // 
            this.toolStripDropDownButton_insertOpacDatabase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton_insertOpacDatabase.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_insertOpacDatabase_normal,
            this.toolStripMenuItem_insertOpacDatabase_virtual});
            this.toolStripDropDownButton_insertOpacDatabase.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton_insertOpacDatabase.Image")));
            this.toolStripDropDownButton_insertOpacDatabase.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_insertOpacDatabase.Name = "toolStripDropDownButton_insertOpacDatabase";
            this.toolStripDropDownButton_insertOpacDatabase.Size = new System.Drawing.Size(51, 22);
            this.toolStripDropDownButton_insertOpacDatabase.Text = "插入";
            // 
            // toolStripMenuItem_insertOpacDatabase_normal
            // 
            this.toolStripMenuItem_insertOpacDatabase_normal.Name = "toolStripMenuItem_insertOpacDatabase_normal";
            this.toolStripMenuItem_insertOpacDatabase_normal.Size = new System.Drawing.Size(158, 22);
            this.toolStripMenuItem_insertOpacDatabase_normal.Text = "普通库(&N)...";
            this.toolStripMenuItem_insertOpacDatabase_normal.Click += new System.EventHandler(this.toolStripMenuItem_insertOpacDatabase_normal_Click);
            // 
            // toolStripMenuItem_insertOpacDatabase_virtual
            // 
            this.toolStripMenuItem_insertOpacDatabase_virtual.Name = "toolStripMenuItem_insertOpacDatabase_virtual";
            this.toolStripMenuItem_insertOpacDatabase_virtual.Size = new System.Drawing.Size(158, 22);
            this.toolStripMenuItem_insertOpacDatabase_virtual.Text = "虚拟库(&V)...";
            this.toolStripMenuItem_insertOpacDatabase_virtual.Click += new System.EventHandler(this.toolStripMenuItem_insertOpacDatabase_virtual_Click);
            // 
            // toolStripButton_modifyOpacDatabase
            // 
            this.toolStripButton_modifyOpacDatabase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_modifyOpacDatabase.Enabled = false;
            this.toolStripButton_modifyOpacDatabase.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_modifyOpacDatabase.Image")));
            this.toolStripButton_modifyOpacDatabase.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_modifyOpacDatabase.Name = "toolStripButton_modifyOpacDatabase";
            this.toolStripButton_modifyOpacDatabase.Size = new System.Drawing.Size(42, 22);
            this.toolStripButton_modifyOpacDatabase.Text = "修改";
            this.toolStripButton_modifyOpacDatabase.Click += new System.EventHandler(this.toolStripButton_modifyOpacDatabase_Click);
            // 
            // toolStripButton_removeOpacDatabase
            // 
            this.toolStripButton_removeOpacDatabase.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_removeOpacDatabase.Enabled = false;
            this.toolStripButton_removeOpacDatabase.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_removeOpacDatabase.Image")));
            this.toolStripButton_removeOpacDatabase.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_removeOpacDatabase.Name = "toolStripButton_removeOpacDatabase";
            this.toolStripButton_removeOpacDatabase.Size = new System.Drawing.Size(42, 22);
            this.toolStripButton_removeOpacDatabase.Text = "移除";
            this.toolStripButton_removeOpacDatabase.Click += new System.EventHandler(this.toolStripButton_removeOpacDatabase_Click);
            // 
            // toolStripButton_refreshOpacDatabaseList
            // 
            this.toolStripButton_refreshOpacDatabaseList.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_refreshOpacDatabaseList.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_refreshOpacDatabaseList.Image")));
            this.toolStripButton_refreshOpacDatabaseList.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_refreshOpacDatabaseList.Name = "toolStripButton_refreshOpacDatabaseList";
            this.toolStripButton_refreshOpacDatabaseList.Size = new System.Drawing.Size(42, 22);
            this.toolStripButton_refreshOpacDatabaseList.Text = "刷新";
            this.toolStripButton_refreshOpacDatabaseList.Click += new System.EventHandler(this.toolStripButton_refreshOpacDatabaseList_Click);
            // 
            // listView_opacDatabases
            // 
            this.listView_opacDatabases.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_opacDatabases.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView_opacDatabases.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.listView_opacDatabases.FullRowSelect = true;
            this.listView_opacDatabases.HideSelection = false;
            this.listView_opacDatabases.Location = new System.Drawing.Point(0, 18);
            this.listView_opacDatabases.Name = "listView_opacDatabases";
            this.listView_opacDatabases.Size = new System.Drawing.Size(574, 98);
            this.listView_opacDatabases.TabIndex = 2;
            this.listView_opacDatabases.UseCompatibleStateImageBehavior = false;
            this.listView_opacDatabases.View = System.Windows.Forms.View.Details;
            this.listView_opacDatabases.SelectedIndexChanged += new System.EventHandler(this.listView_opacDatabases_SelectedIndexChanged);
            this.listView_opacDatabases.DoubleClick += new System.EventHandler(this.listView_opacDatabases_DoubleClick);
            this.listView_opacDatabases.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_opacDatabases_MouseUp);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "数据库名";
            this.columnHeader1.Width = 150;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "类型";
            this.columnHeader2.Width = 80;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "说明";
            this.columnHeader3.Width = 300;
            // 
            // toolStrip_opacBrowseFormats
            // 
            this.toolStrip_opacBrowseFormats.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip_opacBrowseFormats.AutoSize = false;
            this.toolStrip_opacBrowseFormats.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip_opacBrowseFormats.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1,
            this.toolStripButton_opacBrowseFormats_modify,
            this.toolStripButton_opacBrowseFormats_remove,
            this.toolStripButton_opacBrowseFormats_refresh});
            this.toolStrip_opacBrowseFormats.Location = new System.Drawing.Point(0, 109);
            this.toolStrip_opacBrowseFormats.Name = "toolStrip_opacBrowseFormats";
            this.toolStrip_opacBrowseFormats.Size = new System.Drawing.Size(573, 25);
            this.toolStrip_opacBrowseFormats.TabIndex = 13;
            this.toolStrip_opacBrowseFormats.Text = "toolStrip1";
            // 
            // toolStripDropDownButton1
            // 
            this.toolStripDropDownButton1.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_opacBrowseFormats_insertDatabaseNameNode,
            this.toolStripMenuItem_opacBrowseFormats_insertBrowseFormatNode});
            this.toolStripDropDownButton1.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton1.Image")));
            this.toolStripDropDownButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton1.Name = "toolStripDropDownButton1";
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(51, 22);
            this.toolStripDropDownButton1.Text = "插入";
            // 
            // toolStripMenuItem_opacBrowseFormats_insertDatabaseNameNode
            // 
            this.toolStripMenuItem_opacBrowseFormats_insertDatabaseNameNode.Name = "toolStripMenuItem_opacBrowseFormats_insertDatabaseNameNode";
            this.toolStripMenuItem_opacBrowseFormats_insertDatabaseNameNode.Size = new System.Drawing.Size(231, 22);
            this.toolStripMenuItem_opacBrowseFormats_insertDatabaseNameNode.Text = "插入库名节点(&N)...";
            this.toolStripMenuItem_opacBrowseFormats_insertDatabaseNameNode.Click += new System.EventHandler(this.toolStripMenuItem_opacBrowseFormats_insertDatabaseNameNode_Click);
            // 
            // toolStripMenuItem_opacBrowseFormats_insertBrowseFormatNode
            // 
            this.toolStripMenuItem_opacBrowseFormats_insertBrowseFormatNode.Name = "toolStripMenuItem_opacBrowseFormats_insertBrowseFormatNode";
            this.toolStripMenuItem_opacBrowseFormats_insertBrowseFormatNode.Size = new System.Drawing.Size(231, 22);
            this.toolStripMenuItem_opacBrowseFormats_insertBrowseFormatNode.Text = "插入显示格式节点(&F)...";
            this.toolStripMenuItem_opacBrowseFormats_insertBrowseFormatNode.Click += new System.EventHandler(this.toolStripMenuItem_opacBrowseFormats_insertBrowseFormatNode_Click);
            // 
            // toolStripButton_opacBrowseFormats_modify
            // 
            this.toolStripButton_opacBrowseFormats_modify.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_opacBrowseFormats_modify.Enabled = false;
            this.toolStripButton_opacBrowseFormats_modify.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_opacBrowseFormats_modify.Image")));
            this.toolStripButton_opacBrowseFormats_modify.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_opacBrowseFormats_modify.Name = "toolStripButton_opacBrowseFormats_modify";
            this.toolStripButton_opacBrowseFormats_modify.Size = new System.Drawing.Size(42, 22);
            this.toolStripButton_opacBrowseFormats_modify.Text = "修改";
            this.toolStripButton_opacBrowseFormats_modify.Click += new System.EventHandler(this.toolStripButton_opacBrowseFormats_modify_Click);
            // 
            // toolStripButton_opacBrowseFormats_remove
            // 
            this.toolStripButton_opacBrowseFormats_remove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_opacBrowseFormats_remove.Enabled = false;
            this.toolStripButton_opacBrowseFormats_remove.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_opacBrowseFormats_remove.Image")));
            this.toolStripButton_opacBrowseFormats_remove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_opacBrowseFormats_remove.Name = "toolStripButton_opacBrowseFormats_remove";
            this.toolStripButton_opacBrowseFormats_remove.Size = new System.Drawing.Size(42, 22);
            this.toolStripButton_opacBrowseFormats_remove.Text = "移除";
            this.toolStripButton_opacBrowseFormats_remove.Click += new System.EventHandler(this.toolStripButton_opacBrowseFormats_remove_Click);
            // 
            // toolStripButton_opacBrowseFormats_refresh
            // 
            this.toolStripButton_opacBrowseFormats_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_opacBrowseFormats_refresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_opacBrowseFormats_refresh.Image")));
            this.toolStripButton_opacBrowseFormats_refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_opacBrowseFormats_refresh.Name = "toolStripButton_opacBrowseFormats_refresh";
            this.toolStripButton_opacBrowseFormats_refresh.Size = new System.Drawing.Size(42, 22);
            this.toolStripButton_opacBrowseFormats_refresh.Text = "刷新";
            this.toolStripButton_opacBrowseFormats_refresh.Click += new System.EventHandler(this.toolStripButton_opacBrowseFormats_refresh_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(-3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(99, 15);
            this.label2.TabIndex = 12;
            this.label2.Text = "显示格式(&F):";
            // 
            // treeView_opacBrowseFormats
            // 
            this.treeView_opacBrowseFormats.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView_opacBrowseFormats.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeView_opacBrowseFormats.HideSelection = false;
            this.treeView_opacBrowseFormats.Location = new System.Drawing.Point(0, 18);
            this.treeView_opacBrowseFormats.Name = "treeView_opacBrowseFormats";
            this.treeView_opacBrowseFormats.Size = new System.Drawing.Size(574, 88);
            this.treeView_opacBrowseFormats.TabIndex = 11;
            this.treeView_opacBrowseFormats.DoubleClick += new System.EventHandler(this.treeView_opacBrowseFormats_DoubleClick);
            this.treeView_opacBrowseFormats.MouseUp += new System.Windows.Forms.MouseEventHandler(this.treeView_opacBrowseFormats_MouseUp);
            this.treeView_opacBrowseFormats.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_opacBrowseFormats_AfterSelect);
            this.treeView_opacBrowseFormats.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeView_opacBrowseFormats_MouseDown);
            // 
            // tabPage_dup
            // 
            this.tabPage_dup.Location = new System.Drawing.Point(4, 24);
            this.tabPage_dup.Name = "tabPage_dup";
            this.tabPage_dup.Size = new System.Drawing.Size(587, 148);
            this.tabPage_dup.TabIndex = 2;
            this.tabPage_dup.Text = "查重";
            this.tabPage_dup.UseVisualStyleBackColor = true;
            // 
            // tabPage_loanPolicy
            // 
            this.tabPage_loanPolicy.Controls.Add(this.toolStrip_loanPolicy);
            this.tabPage_loanPolicy.Controls.Add(this.splitContainer_loanPolicy);
            this.tabPage_loanPolicy.Location = new System.Drawing.Point(4, 24);
            this.tabPage_loanPolicy.Name = "tabPage_loanPolicy";
            this.tabPage_loanPolicy.Size = new System.Drawing.Size(587, 148);
            this.tabPage_loanPolicy.TabIndex = 3;
            this.tabPage_loanPolicy.Text = "读者流通权限";
            this.tabPage_loanPolicy.UseVisualStyleBackColor = true;
            // 
            // toolStrip_loanPolicy
            // 
            this.toolStrip_loanPolicy.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip_loanPolicy.AutoSize = false;
            this.toolStrip_loanPolicy.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip_loanPolicy.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_loanPolicy_save,
            this.toolStripButton_loanPolicy_refresh});
            this.toolStrip_loanPolicy.Location = new System.Drawing.Point(7, 301);
            this.toolStrip_loanPolicy.Name = "toolStrip_loanPolicy";
            this.toolStrip_loanPolicy.Size = new System.Drawing.Size(574, 25);
            this.toolStrip_loanPolicy.TabIndex = 7;
            this.toolStrip_loanPolicy.Text = "toolStrip1";
            // 
            // toolStripButton_loanPolicy_save
            // 
            this.toolStripButton_loanPolicy_save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_loanPolicy_save.Enabled = false;
            this.toolStripButton_loanPolicy_save.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_loanPolicy_save.Image")));
            this.toolStripButton_loanPolicy_save.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_loanPolicy_save.Name = "toolStripButton_loanPolicy_save";
            this.toolStripButton_loanPolicy_save.Size = new System.Drawing.Size(42, 22);
            this.toolStripButton_loanPolicy_save.Text = "保存";
            this.toolStripButton_loanPolicy_save.Click += new System.EventHandler(this.toolStripButton_loanPolicy_save_Click);
            // 
            // toolStripButton_loanPolicy_refresh
            // 
            this.toolStripButton_loanPolicy_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_loanPolicy_refresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_loanPolicy_refresh.Image")));
            this.toolStripButton_loanPolicy_refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_loanPolicy_refresh.Name = "toolStripButton_loanPolicy_refresh";
            this.toolStripButton_loanPolicy_refresh.Size = new System.Drawing.Size(42, 22);
            this.toolStripButton_loanPolicy_refresh.Text = "刷新";
            this.toolStripButton_loanPolicy_refresh.Click += new System.EventHandler(this.toolStripButton_loanPolicy_refresh_Click);
            // 
            // splitContainer_loanPolicy
            // 
            this.splitContainer_loanPolicy.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_loanPolicy.Location = new System.Drawing.Point(4, 4);
            this.splitContainer_loanPolicy.Name = "splitContainer_loanPolicy";
            this.splitContainer_loanPolicy.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_loanPolicy.Panel1
            // 
            this.splitContainer_loanPolicy.Panel1.Controls.Add(this.label3);
            this.splitContainer_loanPolicy.Panel1.Controls.Add(this.textBox_loanPolicy_rightsTableDef);
            // 
            // splitContainer_loanPolicy.Panel2
            // 
            this.splitContainer_loanPolicy.Panel2.Controls.Add(this.webBrowser_rightsTableHtml);
            this.splitContainer_loanPolicy.Panel2.Controls.Add(this.label4);
            this.splitContainer_loanPolicy.Size = new System.Drawing.Size(580, 285);
            this.splitContainer_loanPolicy.SplitterDistance = 141;
            this.splitContainer_loanPolicy.SplitterWidth = 8;
            this.splitContainer_loanPolicy.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(-3, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(183, 15);
            this.label3.TabIndex = 0;
            this.label3.Text = "读者流通权限XML定义(&D):";
            // 
            // textBox_loanPolicy_rightsTableDef
            // 
            this.textBox_loanPolicy_rightsTableDef.AcceptsReturn = true;
            this.textBox_loanPolicy_rightsTableDef.AcceptsTab = true;
            this.textBox_loanPolicy_rightsTableDef.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_loanPolicy_rightsTableDef.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_loanPolicy_rightsTableDef.HideSelection = false;
            this.textBox_loanPolicy_rightsTableDef.Location = new System.Drawing.Point(0, 18);
            this.textBox_loanPolicy_rightsTableDef.Multiline = true;
            this.textBox_loanPolicy_rightsTableDef.Name = "textBox_loanPolicy_rightsTableDef";
            this.textBox_loanPolicy_rightsTableDef.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_loanPolicy_rightsTableDef.Size = new System.Drawing.Size(580, 120);
            this.textBox_loanPolicy_rightsTableDef.TabIndex = 1;
            this.textBox_loanPolicy_rightsTableDef.TextChanged += new System.EventHandler(this.textBox_loanPolicy_rightsTableDef_TextChanged);
            this.textBox_loanPolicy_rightsTableDef.Leave += new System.EventHandler(this.textBox_loanPolicy_rightsTableDef_Leave);
            this.textBox_loanPolicy_rightsTableDef.Enter += new System.EventHandler(this.textBox_loanPolicy_rightsTableDef_Enter);
            // 
            // webBrowser_rightsTableHtml
            // 
            this.webBrowser_rightsTableHtml.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.webBrowser_rightsTableHtml.Location = new System.Drawing.Point(0, 22);
            this.webBrowser_rightsTableHtml.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser_rightsTableHtml.Name = "webBrowser_rightsTableHtml";
            this.webBrowser_rightsTableHtml.Size = new System.Drawing.Size(580, 111);
            this.webBrowser_rightsTableHtml.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(0, 4);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(114, 15);
            this.label4.TabIndex = 0;
            this.label4.Text = "权限对照表(&T):";
            // 
            // tabPage_locations
            // 
            this.tabPage_locations.Controls.Add(this.textBox_location_comment);
            this.tabPage_locations.Controls.Add(this.toolStrip_location);
            this.tabPage_locations.Controls.Add(this.listView_location_list);
            this.tabPage_locations.Location = new System.Drawing.Point(4, 24);
            this.tabPage_locations.Name = "tabPage_locations";
            this.tabPage_locations.Size = new System.Drawing.Size(587, 148);
            this.tabPage_locations.TabIndex = 4;
            this.tabPage_locations.Text = "馆藏地";
            this.tabPage_locations.UseVisualStyleBackColor = true;
            // 
            // textBox_location_comment
            // 
            this.textBox_location_comment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_location_comment.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_location_comment.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_location_comment.Location = new System.Drawing.Point(5, 210);
            this.textBox_location_comment.Multiline = true;
            this.textBox_location_comment.Name = "textBox_location_comment";
            this.textBox_location_comment.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_location_comment.Size = new System.Drawing.Size(574, 106);
            this.textBox_location_comment.TabIndex = 9;
            this.textBox_location_comment.Text = "注: 当library.xml中有ItemCanBorrow()函数时，在这里配置的关于馆藏地点是否允许外借的参数会自动失效。";
            // 
            // toolStrip_location
            // 
            this.toolStrip_location.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip_location.AutoSize = false;
            this.toolStrip_location.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip_location.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_location_save,
            this.toolStripButton_location_refresh,
            this.toolStripSeparator1,
            this.toolStripButton_location_up,
            this.toolStripButton_location_down,
            this.toolStripSeparator2,
            this.toolStripButton_location_new,
            this.toolStripButton_location_modify,
            this.toolStripButton_location_delete});
            this.toolStrip_location.Location = new System.Drawing.Point(5, 181);
            this.toolStrip_location.Name = "toolStrip_location";
            this.toolStrip_location.Size = new System.Drawing.Size(574, 25);
            this.toolStrip_location.TabIndex = 8;
            this.toolStrip_location.Text = "toolStrip1";
            // 
            // toolStripButton_location_save
            // 
            this.toolStripButton_location_save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_location_save.Enabled = false;
            this.toolStripButton_location_save.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_location_save.Image")));
            this.toolStripButton_location_save.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_location_save.Name = "toolStripButton_location_save";
            this.toolStripButton_location_save.Size = new System.Drawing.Size(42, 22);
            this.toolStripButton_location_save.Text = "保存";
            this.toolStripButton_location_save.Click += new System.EventHandler(this.toolStripButton_location_save_Click);
            // 
            // toolStripButton_location_refresh
            // 
            this.toolStripButton_location_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_location_refresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_location_refresh.Image")));
            this.toolStripButton_location_refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_location_refresh.Name = "toolStripButton_location_refresh";
            this.toolStripButton_location_refresh.Size = new System.Drawing.Size(42, 22);
            this.toolStripButton_location_refresh.Text = "刷新";
            this.toolStripButton_location_refresh.Click += new System.EventHandler(this.toolStripButton_location_refresh_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_location_up
            // 
            this.toolStripButton_location_up.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_location_up.Enabled = false;
            this.toolStripButton_location_up.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_location_up.Image")));
            this.toolStripButton_location_up.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_location_up.Name = "toolStripButton_location_up";
            this.toolStripButton_location_up.Size = new System.Drawing.Size(42, 22);
            this.toolStripButton_location_up.Text = "上移";
            this.toolStripButton_location_up.Click += new System.EventHandler(this.toolStripButton_location_up_Click);
            // 
            // toolStripButton_location_down
            // 
            this.toolStripButton_location_down.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_location_down.Enabled = false;
            this.toolStripButton_location_down.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_location_down.Image")));
            this.toolStripButton_location_down.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_location_down.Name = "toolStripButton_location_down";
            this.toolStripButton_location_down.Size = new System.Drawing.Size(42, 22);
            this.toolStripButton_location_down.Text = "下移";
            this.toolStripButton_location_down.Click += new System.EventHandler(this.toolStripButton_location_down_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // toolStripButton_location_new
            // 
            this.toolStripButton_location_new.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_location_new.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_location_new.Image")));
            this.toolStripButton_location_new.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_location_new.Name = "toolStripButton_location_new";
            this.toolStripButton_location_new.Size = new System.Drawing.Size(42, 22);
            this.toolStripButton_location_new.Text = "新增";
            this.toolStripButton_location_new.Click += new System.EventHandler(this.toolStripButton_location_new_Click);
            // 
            // toolStripButton_location_modify
            // 
            this.toolStripButton_location_modify.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_location_modify.Enabled = false;
            this.toolStripButton_location_modify.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_location_modify.Image")));
            this.toolStripButton_location_modify.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_location_modify.Name = "toolStripButton_location_modify";
            this.toolStripButton_location_modify.Size = new System.Drawing.Size(42, 22);
            this.toolStripButton_location_modify.Text = "修改";
            this.toolStripButton_location_modify.Click += new System.EventHandler(this.toolStripButton_location_modify_Click);
            // 
            // toolStripButton_location_delete
            // 
            this.toolStripButton_location_delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_location_delete.Enabled = false;
            this.toolStripButton_location_delete.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_location_delete.Image")));
            this.toolStripButton_location_delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_location_delete.Name = "toolStripButton_location_delete";
            this.toolStripButton_location_delete.Size = new System.Drawing.Size(42, 22);
            this.toolStripButton_location_delete.Text = "删除";
            this.toolStripButton_location_delete.Click += new System.EventHandler(this.toolStripButton_location_delete_Click);
            // 
            // listView_location_list
            // 
            this.listView_location_list.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_location_list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_location_name,
            this.columnHeader_location_canBorrow});
            this.listView_location_list.FullRowSelect = true;
            this.listView_location_list.HideSelection = false;
            this.listView_location_list.Location = new System.Drawing.Point(5, 13);
            this.listView_location_list.Name = "listView_location_list";
            this.listView_location_list.Size = new System.Drawing.Size(574, 165);
            this.listView_location_list.TabIndex = 0;
            this.listView_location_list.UseCompatibleStateImageBehavior = false;
            this.listView_location_list.View = System.Windows.Forms.View.Details;
            this.listView_location_list.SelectedIndexChanged += new System.EventHandler(this.listView_location_list_SelectedIndexChanged);
            this.listView_location_list.DoubleClick += new System.EventHandler(this.listView_location_list_DoubleClick);
            this.listView_location_list.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_location_list_MouseUp);
            // 
            // columnHeader_location_name
            // 
            this.columnHeader_location_name.Text = "馆藏地";
            this.columnHeader_location_name.Width = 168;
            // 
            // columnHeader_location_canBorrow
            // 
            this.columnHeader_location_canBorrow.Text = "可否外借";
            this.columnHeader_location_canBorrow.Width = 94;
            // 
            // tabPage_zhongcihaoDatabases
            // 
            this.tabPage_zhongcihaoDatabases.Controls.Add(this.toolStrip_zhongcihao);
            this.tabPage_zhongcihaoDatabases.Controls.Add(this.treeView_zhongcihao);
            this.tabPage_zhongcihaoDatabases.Location = new System.Drawing.Point(4, 24);
            this.tabPage_zhongcihaoDatabases.Name = "tabPage_zhongcihaoDatabases";
            this.tabPage_zhongcihaoDatabases.Size = new System.Drawing.Size(587, 204);
            this.tabPage_zhongcihaoDatabases.TabIndex = 5;
            this.tabPage_zhongcihaoDatabases.Text = "种次号库";
            this.tabPage_zhongcihaoDatabases.UseVisualStyleBackColor = true;
            // 
            // toolStrip_zhongcihao
            // 
            this.toolStrip_zhongcihao.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip_zhongcihao.AutoSize = false;
            this.toolStrip_zhongcihao.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip_zhongcihao.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton_insert,
            this.toolStripButton_zhongcihao_modify,
            this.toolStripButton_zhongcihao_remove,
            this.toolStripButton_zhongcihao_refresh});
            this.toolStrip_zhongcihao.Location = new System.Drawing.Point(6, 267);
            this.toolStrip_zhongcihao.Name = "toolStrip_zhongcihao";
            this.toolStrip_zhongcihao.Size = new System.Drawing.Size(574, 25);
            this.toolStrip_zhongcihao.TabIndex = 14;
            this.toolStrip_zhongcihao.Text = "toolStrip1";
            // 
            // toolStripDropDownButton_insert
            // 
            this.toolStripDropDownButton_insert.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton_insert.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_zhongcihao_insert_group,
            this.toolStripMenuItem_zhongcihao_insert_database,
            this.ToolStripMenuItem_zhongcihao_insert_nstable});
            this.toolStripDropDownButton_insert.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton_insert.Image")));
            this.toolStripDropDownButton_insert.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_insert.Name = "toolStripDropDownButton_insert";
            this.toolStripDropDownButton_insert.Size = new System.Drawing.Size(51, 22);
            this.toolStripDropDownButton_insert.Text = "插入";
            // 
            // toolStripMenuItem_zhongcihao_insert_group
            // 
            this.toolStripMenuItem_zhongcihao_insert_group.Name = "toolStripMenuItem_zhongcihao_insert_group";
            this.toolStripMenuItem_zhongcihao_insert_group.Size = new System.Drawing.Size(232, 22);
            this.toolStripMenuItem_zhongcihao_insert_group.Text = "插入组节点(&G)...";
            this.toolStripMenuItem_zhongcihao_insert_group.Click += new System.EventHandler(this.toolStripMenuItem_zhongcihao_insert_group_Click);
            // 
            // toolStripMenuItem_zhongcihao_insert_database
            // 
            this.toolStripMenuItem_zhongcihao_insert_database.Name = "toolStripMenuItem_zhongcihao_insert_database";
            this.toolStripMenuItem_zhongcihao_insert_database.Size = new System.Drawing.Size(232, 22);
            this.toolStripMenuItem_zhongcihao_insert_database.Text = "插入书目库名节点(&B)...";
            this.toolStripMenuItem_zhongcihao_insert_database.Click += new System.EventHandler(this.toolStripMenuItem_zhongcihao_insert_database_Click);
            // 
            // ToolStripMenuItem_zhongcihao_insert_nstable
            // 
            this.ToolStripMenuItem_zhongcihao_insert_nstable.Name = "ToolStripMenuItem_zhongcihao_insert_nstable";
            this.ToolStripMenuItem_zhongcihao_insert_nstable.Size = new System.Drawing.Size(232, 22);
            this.ToolStripMenuItem_zhongcihao_insert_nstable.Text = "插入名字表节点(&N)...";
            this.ToolStripMenuItem_zhongcihao_insert_nstable.Click += new System.EventHandler(this.ToolStripMenuItem_zhongcihao_insert_nstable_Click);
            // 
            // toolStripButton_zhongcihao_modify
            // 
            this.toolStripButton_zhongcihao_modify.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_zhongcihao_modify.Enabled = false;
            this.toolStripButton_zhongcihao_modify.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_zhongcihao_modify.Image")));
            this.toolStripButton_zhongcihao_modify.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_zhongcihao_modify.Name = "toolStripButton_zhongcihao_modify";
            this.toolStripButton_zhongcihao_modify.Size = new System.Drawing.Size(42, 22);
            this.toolStripButton_zhongcihao_modify.Text = "修改";
            // 
            // toolStripButton_zhongcihao_remove
            // 
            this.toolStripButton_zhongcihao_remove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_zhongcihao_remove.Enabled = false;
            this.toolStripButton_zhongcihao_remove.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_zhongcihao_remove.Image")));
            this.toolStripButton_zhongcihao_remove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_zhongcihao_remove.Name = "toolStripButton_zhongcihao_remove";
            this.toolStripButton_zhongcihao_remove.Size = new System.Drawing.Size(42, 22);
            this.toolStripButton_zhongcihao_remove.Text = "移除";
            // 
            // toolStripButton_zhongcihao_refresh
            // 
            this.toolStripButton_zhongcihao_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_zhongcihao_refresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_zhongcihao_refresh.Image")));
            this.toolStripButton_zhongcihao_refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_zhongcihao_refresh.Name = "toolStripButton_zhongcihao_refresh";
            this.toolStripButton_zhongcihao_refresh.Size = new System.Drawing.Size(42, 22);
            this.toolStripButton_zhongcihao_refresh.Text = "刷新";
            // 
            // treeView_zhongcihao
            // 
            this.treeView_zhongcihao.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView_zhongcihao.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeView_zhongcihao.HideSelection = false;
            this.treeView_zhongcihao.Location = new System.Drawing.Point(6, 31);
            this.treeView_zhongcihao.Name = "treeView_zhongcihao";
            this.treeView_zhongcihao.Size = new System.Drawing.Size(574, 102);
            this.treeView_zhongcihao.TabIndex = 12;
            this.treeView_zhongcihao.MouseUp += new System.Windows.Forms.MouseEventHandler(this.treeView_zhongcihao_MouseUp);
            this.treeView_zhongcihao.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_zhongcihao_AfterSelect);
            // 
            // tabPage_script
            // 
            this.tabPage_script.Controls.Add(this.splitContainer_script);
            this.tabPage_script.Location = new System.Drawing.Point(4, 24);
            this.tabPage_script.Name = "tabPage_script";
            this.tabPage_script.Size = new System.Drawing.Size(587, 204);
            this.tabPage_script.TabIndex = 6;
            this.tabPage_script.Text = "脚本程序";
            this.tabPage_script.UseVisualStyleBackColor = true;
            // 
            // splitContainer_script
            // 
            this.splitContainer_script.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_script.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_script.Name = "splitContainer_script";
            this.splitContainer_script.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_script.Panel1
            // 
            this.splitContainer_script.Panel1.Controls.Add(this.textBox_script);
            this.splitContainer_script.Panel1.Controls.Add(this.toolStrip_script);
            this.splitContainer_script.Panel1.Padding = new System.Windows.Forms.Padding(0, 8, 0, 0);
            // 
            // splitContainer_script.Panel2
            // 
            this.splitContainer_script.Panel2.Controls.Add(this.textBox_script_comment);
            this.splitContainer_script.Panel2.Padding = new System.Windows.Forms.Padding(0, 0, 0, 8);
            this.splitContainer_script.Size = new System.Drawing.Size(587, 204);
            this.splitContainer_script.SplitterDistance = 153;
            this.splitContainer_script.SplitterWidth = 10;
            this.splitContainer_script.TabIndex = 12;
            // 
            // textBox_script
            // 
            this.textBox_script.AcceptsReturn = true;
            this.textBox_script.AcceptsTab = true;
            this.textBox_script.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_script.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_script.HideSelection = false;
            this.textBox_script.Location = new System.Drawing.Point(0, 8);
            this.textBox_script.Multiline = true;
            this.textBox_script.Name = "textBox_script";
            this.textBox_script.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_script.Size = new System.Drawing.Size(587, 117);
            this.textBox_script.TabIndex = 2;
            this.textBox_script.TextChanged += new System.EventHandler(this.textBox_script_TextChanged);
            this.textBox_script.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_script_KeyDown);
            this.textBox_script.MouseUp += new System.Windows.Forms.MouseEventHandler(this.textBox_script_MouseUp);
            // 
            // toolStrip_script
            // 
            this.toolStrip_script.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip_script.AutoSize = false;
            this.toolStrip_script.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip_script.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_script_save,
            this.toolStripButton_script_refresh,
            this.toolStripLabel_script_caretPos});
            this.toolStrip_script.Location = new System.Drawing.Point(0, 128);
            this.toolStrip_script.Name = "toolStrip_script";
            this.toolStrip_script.Size = new System.Drawing.Size(587, 25);
            this.toolStrip_script.TabIndex = 11;
            this.toolStrip_script.Text = "toolStrip1";
            // 
            // toolStripButton_script_save
            // 
            this.toolStripButton_script_save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_script_save.Enabled = false;
            this.toolStripButton_script_save.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_script_save.Image")));
            this.toolStripButton_script_save.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_script_save.Name = "toolStripButton_script_save";
            this.toolStripButton_script_save.Size = new System.Drawing.Size(42, 22);
            this.toolStripButton_script_save.Text = "保存";
            this.toolStripButton_script_save.Click += new System.EventHandler(this.toolStripButton_script_save_Click);
            // 
            // toolStripButton_script_refresh
            // 
            this.toolStripButton_script_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_script_refresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_script_refresh.Image")));
            this.toolStripButton_script_refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_script_refresh.Name = "toolStripButton_script_refresh";
            this.toolStripButton_script_refresh.Size = new System.Drawing.Size(42, 22);
            this.toolStripButton_script_refresh.Text = "刷新";
            this.toolStripButton_script_refresh.Click += new System.EventHandler(this.toolStripButton_script_refresh_Click);
            // 
            // toolStripLabel_script_caretPos
            // 
            this.toolStripLabel_script_caretPos.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel_script_caretPos.Name = "toolStripLabel_script_caretPos";
            this.toolStripLabel_script_caretPos.Size = new System.Drawing.Size(0, 22);
            // 
            // textBox_script_comment
            // 
            this.textBox_script_comment.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_script_comment.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_script_comment.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_script_comment.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_script_comment.HideSelection = false;
            this.textBox_script_comment.Location = new System.Drawing.Point(0, 0);
            this.textBox_script_comment.Multiline = true;
            this.textBox_script_comment.Name = "textBox_script_comment";
            this.textBox_script_comment.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_script_comment.Size = new System.Drawing.Size(587, 33);
            this.textBox_script_comment.TabIndex = 10;
            this.textBox_script_comment.DoubleClick += new System.EventHandler(this.textBox_script_comment_DoubleClick);
            // 
            // imageList_opacDatabaseType
            // 
            this.imageList_opacDatabaseType.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_opacDatabaseType.ImageStream")));
            this.imageList_opacDatabaseType.TransparentColor = System.Drawing.Color.Fuchsia;
            this.imageList_opacDatabaseType.Images.SetKeyName(0, "database.bmp");
            this.imageList_opacDatabaseType.Images.SetKeyName(1, "v_database.bmp");
            this.imageList_opacDatabaseType.Images.SetKeyName(2, "error_entity.bmp");
            // 
            // imageList_opacBrowseFormatType
            // 
            this.imageList_opacBrowseFormatType.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_opacBrowseFormatType.ImageStream")));
            this.imageList_opacBrowseFormatType.TransparentColor = System.Drawing.Color.Fuchsia;
            this.imageList_opacBrowseFormatType.Images.SetKeyName(0, "database.bmp");
            this.imageList_opacBrowseFormatType.Images.SetKeyName(1, "document.ico");
            this.imageList_opacBrowseFormatType.Images.SetKeyName(2, "error_entity.bmp");
            // 
            // imageList_zhongcihao
            // 
            this.imageList_zhongcihao.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_zhongcihao.ImageStream")));
            this.imageList_zhongcihao.TransparentColor = System.Drawing.Color.Fuchsia;
            this.imageList_zhongcihao.Images.SetKeyName(0, "textdoc.ico");
            this.imageList_zhongcihao.Images.SetKeyName(1, "group.ico");
            this.imageList_zhongcihao.Images.SetKeyName(2, "database.bmp");
            this.imageList_zhongcihao.Images.SetKeyName(3, "error_entity.bmp");
            // 
            // ManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(595, 257);
            this.Controls.Add(this.tabControl_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ManagerForm";
            this.ShowInTaskbar = false;
            this.Text = "系统管理";
            this.Load += new System.EventHandler(this.ManagerForm_Load);
            this.Activated += new System.EventHandler(this.ManagerForm_Activated);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ManagerForm_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ManagerForm_FormClosing);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_databases.ResumeLayout(false);
            this.toolStrip_databases.ResumeLayout(false);
            this.toolStrip_databases.PerformLayout();
            this.tabPage_opacDatabases.ResumeLayout(false);
            this.splitContainer_opac.Panel1.ResumeLayout(false);
            this.splitContainer_opac.Panel1.PerformLayout();
            this.splitContainer_opac.Panel2.ResumeLayout(false);
            this.splitContainer_opac.Panel2.PerformLayout();
            this.splitContainer_opac.ResumeLayout(false);
            this.toolStrip_opacDatabases.ResumeLayout(false);
            this.toolStrip_opacDatabases.PerformLayout();
            this.toolStrip_opacBrowseFormats.ResumeLayout(false);
            this.toolStrip_opacBrowseFormats.PerformLayout();
            this.tabPage_loanPolicy.ResumeLayout(false);
            this.toolStrip_loanPolicy.ResumeLayout(false);
            this.toolStrip_loanPolicy.PerformLayout();
            this.splitContainer_loanPolicy.Panel1.ResumeLayout(false);
            this.splitContainer_loanPolicy.Panel1.PerformLayout();
            this.splitContainer_loanPolicy.Panel2.ResumeLayout(false);
            this.splitContainer_loanPolicy.Panel2.PerformLayout();
            this.splitContainer_loanPolicy.ResumeLayout(false);
            this.tabPage_locations.ResumeLayout(false);
            this.tabPage_locations.PerformLayout();
            this.toolStrip_location.ResumeLayout(false);
            this.toolStrip_location.PerformLayout();
            this.tabPage_zhongcihaoDatabases.ResumeLayout(false);
            this.toolStrip_zhongcihao.ResumeLayout(false);
            this.toolStrip_zhongcihao.PerformLayout();
            this.tabPage_script.ResumeLayout(false);
            this.splitContainer_script.Panel1.ResumeLayout(false);
            this.splitContainer_script.Panel1.PerformLayout();
            this.splitContainer_script.Panel2.ResumeLayout(false);
            this.splitContainer_script.Panel2.PerformLayout();
            this.splitContainer_script.ResumeLayout(false);
            this.toolStrip_script.ResumeLayout(false);
            this.toolStrip_script.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_databases;
        private System.Windows.Forms.ListView listView_databases;
        private System.Windows.Forms.ColumnHeader columnHeader_name;
        private System.Windows.Forms.ColumnHeader columnHeader_type;
        private System.Windows.Forms.ColumnHeader columnHeader_comment;
        private System.Windows.Forms.ToolStrip toolStrip_databases;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_create;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_createBiblioDatabase;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_createAmerceDatabase;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_createReaderDatabase;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_createArrivedDatabase;
        private System.Windows.Forms.ToolStripButton toolStripButton_modifyDatabase;
        private System.Windows.Forms.ToolStripButton toolStripButton_deleteDatabase;
        private System.Windows.Forms.ToolStripButton toolStripButton_initializeDatabase;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_createPublisherDatabase;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_createMessageDatabase;
        private System.Windows.Forms.ToolStripButton toolStripButton_refresh;
        private System.Windows.Forms.TabPage tabPage_opacDatabases;
        private System.Windows.Forms.ListView listView_opacDatabases;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ToolStrip toolStrip_opacDatabases;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_insertOpacDatabase;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_insertOpacDatabase_normal;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_insertOpacDatabase_virtual;
        private System.Windows.Forms.ToolStripButton toolStripButton_modifyOpacDatabase;
        private System.Windows.Forms.ToolStripButton toolStripButton_removeOpacDatabase;
        private System.Windows.Forms.ToolStripButton toolStripButton_refreshOpacDatabaseList;
        private System.Windows.Forms.ToolStripButton toolStripButton_initialAllDatabases;
        private System.Windows.Forms.ImageList imageList_opacDatabaseType;
        private System.Windows.Forms.TabPage tabPage_dup;
        private System.Windows.Forms.ImageList imageList_opacBrowseFormatType;
        private System.Windows.Forms.SplitContainer splitContainer_opac;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStrip toolStrip_opacBrowseFormats;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_opacBrowseFormats_insertDatabaseNameNode;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_opacBrowseFormats_insertBrowseFormatNode;
        private System.Windows.Forms.ToolStripButton toolStripButton_opacBrowseFormats_modify;
        private System.Windows.Forms.ToolStripButton toolStripButton_opacBrowseFormats_remove;
        private System.Windows.Forms.ToolStripButton toolStripButton_opacBrowseFormats_refresh;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TreeView treeView_opacBrowseFormats;
        private System.Windows.Forms.TabPage tabPage_loanPolicy;
        private System.Windows.Forms.TextBox textBox_loanPolicy_rightsTableDef;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.SplitContainer splitContainer_loanPolicy;
        private System.Windows.Forms.WebBrowser webBrowser_rightsTableHtml;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ToolStrip toolStrip_loanPolicy;
        private System.Windows.Forms.ToolStripButton toolStripButton_loanPolicy_save;
        private System.Windows.Forms.ToolStripButton toolStripButton_loanPolicy_refresh;
        private System.Windows.Forms.TabPage tabPage_locations;
        private System.Windows.Forms.TabPage tabPage_zhongcihaoDatabases;
        private System.Windows.Forms.ListView listView_location_list;
        private System.Windows.Forms.ColumnHeader columnHeader_location_name;
        private System.Windows.Forms.ColumnHeader columnHeader_location_canBorrow;
        private System.Windows.Forms.ToolStrip toolStrip_location;
        private System.Windows.Forms.ToolStripButton toolStripButton_location_save;
        private System.Windows.Forms.ToolStripButton toolStripButton_location_refresh;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_location_new;
        private System.Windows.Forms.ToolStripButton toolStripButton_location_modify;
        private System.Windows.Forms.ToolStripButton toolStripButton_location_delete;
        private System.Windows.Forms.ToolStripButton toolStripButton_location_up;
        private System.Windows.Forms.ToolStripButton toolStripButton_location_down;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.TextBox textBox_location_comment;
        private System.Windows.Forms.TabPage tabPage_script;
        private NoHasSelTextBox textBox_script_comment;
        private NoHasSelTextBox textBox_script;
        private System.Windows.Forms.ToolStrip toolStrip_script;
        private System.Windows.Forms.ToolStripButton toolStripButton_script_save;
        private System.Windows.Forms.ToolStripButton toolStripButton_script_refresh;
        private System.Windows.Forms.ToolStripLabel toolStripLabel_script_caretPos;
        private System.Windows.Forms.SplitContainer splitContainer_script;
        private System.Windows.Forms.TreeView treeView_zhongcihao;
        private System.Windows.Forms.ImageList imageList_zhongcihao;
        private System.Windows.Forms.ToolStrip toolStrip_zhongcihao;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_insert;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_zhongcihao_insert_group;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_zhongcihao_insert_database;
        private System.Windows.Forms.ToolStripButton toolStripButton_zhongcihao_modify;
        private System.Windows.Forms.ToolStripButton toolStripButton_zhongcihao_remove;
        private System.Windows.Forms.ToolStripButton toolStripButton_zhongcihao_refresh;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_zhongcihao_insert_nstable;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_createZhongcihaoDatabase;
    }
}