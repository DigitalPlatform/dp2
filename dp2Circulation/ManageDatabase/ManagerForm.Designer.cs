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
            this.toolStripButton_refreshDatabaseDef = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_refresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_initialAllDatabases = new System.Windows.Forms.ToolStripButton();
            this.listView_databases = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_type = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_marcSyntax = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_bookOrSeries = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_comment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage_opacDatabases = new System.Windows.Forms.TabPage();
            this.splitContainer_opac = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel_opac_up = new System.Windows.Forms.TableLayoutPanel();
            this.listView_opacDatabases = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_opac_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_opac_type = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_opac_alias = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_opac_visible = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_opac_comment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip_opacDatabases = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton_insertOpacDatabase = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem_insertOpacDatabase_normal = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_insertOpacDatabase_virtual = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton_modifyOpacDatabase = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_removeOpacDatabase = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_refreshOpacDatabaseList = new System.Windows.Forms.ToolStripButton();
            this.label1 = new System.Windows.Forms.Label();
            this.tableLayoutPanel_opac_down = new System.Windows.Forms.TableLayoutPanel();
            this.label2 = new System.Windows.Forms.Label();
            this.toolStrip_opacBrowseFormats = new System.Windows.Forms.ToolStrip();
            this.toolStripDropDownButton1 = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem_opacBrowseFormats_insertDatabaseNameNode = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_opacBrowseFormats_insertBrowseFormatNode = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton_opacBrowseFormats_modify = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_opacBrowseFormats_remove = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_opacBrowseFormats_refresh = new System.Windows.Forms.ToolStripButton();
            this.treeView_opacBrowseFormats = new System.Windows.Forms.TreeView();
            this.tabPage_dup = new System.Windows.Forms.TabPage();
            this.toolStrip_dup_main = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_dup_refresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_dup_save = new System.Windows.Forms.ToolStripButton();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.panel_projects = new System.Windows.Forms.Panel();
            this.tableLayoutPanel_dup_up = new System.Windows.Forms.TableLayoutPanel();
            this.listView_dup_projects = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip_dup_project = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_dup_project_new = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_dup_project_modify = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_dup_project_delete = new System.Windows.Forms.ToolStripButton();
            this.tableLayoutPanel_dup_down = new System.Windows.Forms.TableLayoutPanel();
            this.label5 = new System.Windows.Forms.Label();
            this.toolStrip_dup_default = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_dup_default_new = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_dup_default_modify = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_dup_default_delete = new System.Windows.Forms.ToolStripButton();
            this.listView_dup_defaults = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_databaseName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_defaultProjectName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage_locations = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel_locations = new System.Windows.Forms.TableLayoutPanel();
            this.listView_location_list = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_location_libraryCode = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_location_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_location_canBorrow = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_location_canReturn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_location_itemBarcodeNullable = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
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
            this.tabPage_zhongcihaoDatabases = new System.Windows.Forms.TabPage();
            this.toolStrip_zhongcihao = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_zhongcihao_save = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_zhongcihao_refresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripDropDownButton_insert = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem_zhongcihao_insert_group = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_zhongcihao_insert_database = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_zhongcihao_insert_nstable = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton_zhongcihao_modify = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_zhongcihao_remove = new System.Windows.Forms.ToolStripButton();
            this.treeView_zhongcihao = new System.Windows.Forms.TreeView();
            this.tabPage_bookshelf = new System.Windows.Forms.TabPage();
            this.toolStrip_arrangement = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_arrangement_save = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_arrangement_refresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripDropDownButton2 = new System.Windows.Forms.ToolStripDropDownButton();
            this.toolStripMenuItem_arrangement_insert_group = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_arrangement_insert_location = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripButton_arrangement_modify = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_arrangement_remove = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_arrangement_viewXml = new System.Windows.Forms.ToolStripButton();
            this.treeView_arrangement = new System.Windows.Forms.TreeView();
            this.tabPage_script = new System.Windows.Forms.TabPage();
            this.splitContainer_script = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel_script = new System.Windows.Forms.TableLayoutPanel();
            this.textBox_script = new DigitalPlatform.GUI.NoHasSelTextBox();
            this.toolStrip_script = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_script_save = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_script_refresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel_script_caretPos = new System.Windows.Forms.ToolStripLabel();
            this.textBox_script_comment = new DigitalPlatform.GUI.NoHasSelTextBox();
            this.tabPage_barcodeValidation = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel_barcodeValidation = new System.Windows.Forms.TableLayoutPanel();
            this.textBox_barcodeValidation = new DigitalPlatform.GUI.NoHasSelTextBox();
            this.toolStrip_barcodeValidation = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_barcodeValidation_save = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_barcodeValidation_refresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel2 = new System.Windows.Forms.ToolStripLabel();
            this.tabPage_valueTable = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.textBox_valueTables = new DigitalPlatform.GUI.NoHasSelTextBox();
            this.toolStrip_valueTables = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_valueTable_save = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_valueTable_refresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel1 = new System.Windows.Forms.ToolStripLabel();
            this.tabPage_center = new System.Windows.Forms.TabPage();
            this.listView_center = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_center_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_center_url = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_center_userName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_center_refid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip_center = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_center_modify = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_center_add = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_center_delete = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_center_refresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_center_save = new System.Windows.Forms.ToolStripButton();
            this.tabPage_newLoanPolicy = new System.Windows.Forms.TabPage();
            this.tabControl_newLoanPolicy = new System.Windows.Forms.TabControl();
            this.tabPage_newLoanPolicy_rightsTable = new System.Windows.Forms.TabPage();
            this.elementHost1 = new System.Windows.Forms.Integration.ElementHost();
            this.loanPolicyControlWrapper1 = new DigitalPlatform.CirculationClient.LoanPolicyControlWrapper();
            this.tabPage_newLoanPolicy_xml = new System.Windows.Forms.TabPage();
            this.textBox_newLoanPolicy_xml = new System.Windows.Forms.TextBox();
            this.toolStrip_newLoanPolicy = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_newLoanPolicy_save = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_newLoanPolicy_refresh = new System.Windows.Forms.ToolStripButton();
            this.tabPage_calendar = new System.Windows.Forms.TabPage();
            this.listView_calendar = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_calendar_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_calendar_range = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_calendar_comment = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_calendar_content = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList_itemType = new System.Windows.Forms.ImageList(this.components);
            this.toolStrip_calendar = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_calendar_save = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_calendar_refresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton3 = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton4 = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator8 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_calendar_new = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_calendar_modify = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_calendar_delete = new System.Windows.Forms.ToolStripButton();
            this.tabPage_kernel = new System.Windows.Forms.TabPage();
            this.kernelResTree1 = new DigitalPlatform.CirculationClient.KernelResTree();
            this.imageList_opacBrowseFormatType = new System.Windows.Forms.ImageList(this.components);
            this.imageList_opacDatabaseType = new System.Windows.Forms.ImageList(this.components);
            this.imageList_zhongcihao = new System.Windows.Forms.ImageList(this.components);
            this.imageList_arrangement = new System.Windows.Forms.ImageList(this.components);
            this.tabControl_main.SuspendLayout();
            this.tabPage_databases.SuspendLayout();
            this.toolStrip_databases.SuspendLayout();
            this.tabPage_opacDatabases.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_opac)).BeginInit();
            this.splitContainer_opac.Panel1.SuspendLayout();
            this.splitContainer_opac.Panel2.SuspendLayout();
            this.splitContainer_opac.SuspendLayout();
            this.tableLayoutPanel_opac_up.SuspendLayout();
            this.toolStrip_opacDatabases.SuspendLayout();
            this.tableLayoutPanel_opac_down.SuspendLayout();
            this.toolStrip_opacBrowseFormats.SuspendLayout();
            this.tabPage_dup.SuspendLayout();
            this.toolStrip_dup_main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.panel_projects.SuspendLayout();
            this.tableLayoutPanel_dup_up.SuspendLayout();
            this.toolStrip_dup_project.SuspendLayout();
            this.tableLayoutPanel_dup_down.SuspendLayout();
            this.toolStrip_dup_default.SuspendLayout();
            this.tabPage_locations.SuspendLayout();
            this.tableLayoutPanel_locations.SuspendLayout();
            this.toolStrip_location.SuspendLayout();
            this.tabPage_zhongcihaoDatabases.SuspendLayout();
            this.toolStrip_zhongcihao.SuspendLayout();
            this.tabPage_bookshelf.SuspendLayout();
            this.toolStrip_arrangement.SuspendLayout();
            this.tabPage_script.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_script)).BeginInit();
            this.splitContainer_script.Panel1.SuspendLayout();
            this.splitContainer_script.Panel2.SuspendLayout();
            this.splitContainer_script.SuspendLayout();
            this.tableLayoutPanel_script.SuspendLayout();
            this.toolStrip_script.SuspendLayout();
            this.tabPage_barcodeValidation.SuspendLayout();
            this.tableLayoutPanel_barcodeValidation.SuspendLayout();
            this.toolStrip_barcodeValidation.SuspendLayout();
            this.tabPage_valueTable.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.toolStrip_valueTables.SuspendLayout();
            this.tabPage_center.SuspendLayout();
            this.toolStrip_center.SuspendLayout();
            this.tabPage_newLoanPolicy.SuspendLayout();
            this.tabControl_newLoanPolicy.SuspendLayout();
            this.tabPage_newLoanPolicy_rightsTable.SuspendLayout();
            this.tabPage_newLoanPolicy_xml.SuspendLayout();
            this.toolStrip_newLoanPolicy.SuspendLayout();
            this.tabPage_calendar.SuspendLayout();
            this.toolStrip_calendar.SuspendLayout();
            this.tabPage_kernel.SuspendLayout();
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
            this.tabControl_main.Controls.Add(this.tabPage_locations);
            this.tabControl_main.Controls.Add(this.tabPage_zhongcihaoDatabases);
            this.tabControl_main.Controls.Add(this.tabPage_bookshelf);
            this.tabControl_main.Controls.Add(this.tabPage_script);
            this.tabControl_main.Controls.Add(this.tabPage_barcodeValidation);
            this.tabControl_main.Controls.Add(this.tabPage_valueTable);
            this.tabControl_main.Controls.Add(this.tabPage_center);
            this.tabControl_main.Controls.Add(this.tabPage_newLoanPolicy);
            this.tabControl_main.Controls.Add(this.tabPage_calendar);
            this.tabControl_main.Controls.Add(this.tabPage_kernel);
            this.tabControl_main.Location = new System.Drawing.Point(0, 15);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(810, 429);
            this.tabControl_main.TabIndex = 0;
            this.tabControl_main.SelectedIndexChanged += new System.EventHandler(this.tabControl_main_SelectedIndexChanged);
            // 
            // tabPage_databases
            // 
            this.tabPage_databases.Controls.Add(this.toolStrip_databases);
            this.tabPage_databases.Controls.Add(this.listView_databases);
            this.tabPage_databases.Location = new System.Drawing.Point(4, 28);
            this.tabPage_databases.Name = "tabPage_databases";
            this.tabPage_databases.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_databases.Size = new System.Drawing.Size(802, 397);
            this.tabPage_databases.TabIndex = 0;
            this.tabPage_databases.Text = "数据库";
            this.tabPage_databases.UseVisualStyleBackColor = true;
            // 
            // toolStrip_databases
            // 
            this.toolStrip_databases.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip_databases.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_databases.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip_databases.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton_create,
            this.toolStripButton_modifyDatabase,
            this.toolStripButton_deleteDatabase,
            this.toolStripButton_initializeDatabase,
            this.toolStripButton_refreshDatabaseDef,
            this.toolStripSeparator4,
            this.toolStripButton_refresh,
            this.toolStripButton_initialAllDatabases});
            this.toolStrip_databases.Location = new System.Drawing.Point(3, 356);
            this.toolStrip_databases.Name = "toolStrip_databases";
            this.toolStrip_databases.Size = new System.Drawing.Size(796, 38);
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
            this.toolStripDropDownButton_create.Size = new System.Drawing.Size(75, 32);
            this.toolStripDropDownButton_create.Text = "创建";
            // 
            // ToolStripMenuItem_createBiblioDatabase
            // 
            this.ToolStripMenuItem_createBiblioDatabase.Name = "ToolStripMenuItem_createBiblioDatabase";
            this.ToolStripMenuItem_createBiblioDatabase.Size = new System.Drawing.Size(277, 40);
            this.ToolStripMenuItem_createBiblioDatabase.Text = "书目库(&B)...";
            this.ToolStripMenuItem_createBiblioDatabase.Click += new System.EventHandler(this.ToolStripMenuItem_createBiblioDatabase_Click);
            // 
            // ToolStripMenuItem_createReaderDatabase
            // 
            this.ToolStripMenuItem_createReaderDatabase.Name = "ToolStripMenuItem_createReaderDatabase";
            this.ToolStripMenuItem_createReaderDatabase.Size = new System.Drawing.Size(277, 40);
            this.ToolStripMenuItem_createReaderDatabase.Text = "读者库(&R)...";
            this.ToolStripMenuItem_createReaderDatabase.Click += new System.EventHandler(this.ToolStripMenuItem_createReaderDatabase_Click);
            // 
            // ToolStripMenuItem_createAmerceDatabase
            // 
            this.ToolStripMenuItem_createAmerceDatabase.Name = "ToolStripMenuItem_createAmerceDatabase";
            this.ToolStripMenuItem_createAmerceDatabase.Size = new System.Drawing.Size(277, 40);
            this.ToolStripMenuItem_createAmerceDatabase.Text = "违约金库(&A)...";
            this.ToolStripMenuItem_createAmerceDatabase.Click += new System.EventHandler(this.ToolStripMenuItem_createAmerceDatabase_Click);
            // 
            // ToolStripMenuItem_createArrivedDatabase
            // 
            this.ToolStripMenuItem_createArrivedDatabase.Name = "ToolStripMenuItem_createArrivedDatabase";
            this.ToolStripMenuItem_createArrivedDatabase.Size = new System.Drawing.Size(277, 40);
            this.ToolStripMenuItem_createArrivedDatabase.Text = "预约到书库(&V)...";
            this.ToolStripMenuItem_createArrivedDatabase.Click += new System.EventHandler(this.ToolStripMenuItem_createArrivedDatabase_Click);
            // 
            // ToolStripMenuItem_createPublisherDatabase
            // 
            this.ToolStripMenuItem_createPublisherDatabase.Name = "ToolStripMenuItem_createPublisherDatabase";
            this.ToolStripMenuItem_createPublisherDatabase.Size = new System.Drawing.Size(277, 40);
            this.ToolStripMenuItem_createPublisherDatabase.Text = "出版者库(&P)...";
            this.ToolStripMenuItem_createPublisherDatabase.Click += new System.EventHandler(this.ToolStripMenuItem_createPublisherDatabase_Click);
            // 
            // ToolStripMenuItem_createMessageDatabase
            // 
            this.ToolStripMenuItem_createMessageDatabase.Name = "ToolStripMenuItem_createMessageDatabase";
            this.ToolStripMenuItem_createMessageDatabase.Size = new System.Drawing.Size(277, 40);
            this.ToolStripMenuItem_createMessageDatabase.Text = "消息库(&M)..";
            this.ToolStripMenuItem_createMessageDatabase.Click += new System.EventHandler(this.ToolStripMenuItem_createMessageDatabase_Click);
            // 
            // ToolStripMenuItem_createZhongcihaoDatabase
            // 
            this.ToolStripMenuItem_createZhongcihaoDatabase.Name = "ToolStripMenuItem_createZhongcihaoDatabase";
            this.ToolStripMenuItem_createZhongcihaoDatabase.Size = new System.Drawing.Size(277, 40);
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
            this.toolStripButton_modifyDatabase.Size = new System.Drawing.Size(58, 32);
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
            this.toolStripButton_deleteDatabase.Size = new System.Drawing.Size(58, 32);
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
            this.toolStripButton_initializeDatabase.Size = new System.Drawing.Size(79, 32);
            this.toolStripButton_initializeDatabase.Text = "初始化";
            this.toolStripButton_initializeDatabase.Click += new System.EventHandler(this.toolStripButton_initializeDatabase_Click);
            // 
            // toolStripButton_refreshDatabaseDef
            // 
            this.toolStripButton_refreshDatabaseDef.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_refreshDatabaseDef.Enabled = false;
            this.toolStripButton_refreshDatabaseDef.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_refreshDatabaseDef.Image")));
            this.toolStripButton_refreshDatabaseDef.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_refreshDatabaseDef.Name = "toolStripButton_refreshDatabaseDef";
            this.toolStripButton_refreshDatabaseDef.Size = new System.Drawing.Size(100, 32);
            this.toolStripButton_refreshDatabaseDef.Text = "刷新定义";
            this.toolStripButton_refreshDatabaseDef.Click += new System.EventHandler(this.toolStripButton_refreshDatabaseDef_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton_refresh
            // 
            this.toolStripButton_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_refresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_refresh.Image")));
            this.toolStripButton_refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_refresh.Name = "toolStripButton_refresh";
            this.toolStripButton_refresh.Size = new System.Drawing.Size(58, 32);
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
            this.toolStripButton_initialAllDatabases.Size = new System.Drawing.Size(184, 32);
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
            this.columnHeader_marcSyntax,
            this.columnHeader_bookOrSeries,
            this.columnHeader_comment});
            this.listView_databases.FullRowSelect = true;
            this.listView_databases.HideSelection = false;
            this.listView_databases.Location = new System.Drawing.Point(8, 9);
            this.listView_databases.Name = "listView_databases";
            this.listView_databases.Size = new System.Drawing.Size(784, 336);
            this.listView_databases.TabIndex = 1;
            this.listView_databases.UseCompatibleStateImageBehavior = false;
            this.listView_databases.View = System.Windows.Forms.View.Details;
            this.listView_databases.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_databases_ColumnClick);
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
            // columnHeader_marcSyntax
            // 
            this.columnHeader_marcSyntax.Text = "MARC 格式";
            this.columnHeader_marcSyntax.Width = 160;
            // 
            // columnHeader_bookOrSeries
            // 
            this.columnHeader_bookOrSeries.Text = "图书/期刊";
            this.columnHeader_bookOrSeries.Width = 80;
            // 
            // columnHeader_comment
            // 
            this.columnHeader_comment.Text = "说明";
            this.columnHeader_comment.Width = 300;
            // 
            // tabPage_opacDatabases
            // 
            this.tabPage_opacDatabases.Controls.Add(this.splitContainer_opac);
            this.tabPage_opacDatabases.Location = new System.Drawing.Point(4, 28);
            this.tabPage_opacDatabases.Name = "tabPage_opacDatabases";
            this.tabPage_opacDatabases.Size = new System.Drawing.Size(802, 397);
            this.tabPage_opacDatabases.TabIndex = 1;
            this.tabPage_opacDatabases.Text = "OPAC";
            this.tabPage_opacDatabases.UseVisualStyleBackColor = true;
            // 
            // splitContainer_opac
            // 
            this.splitContainer_opac.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_opac.Location = new System.Drawing.Point(8, 21);
            this.splitContainer_opac.Name = "splitContainer_opac";
            this.splitContainer_opac.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_opac.Panel1
            // 
            this.splitContainer_opac.Panel1.Controls.Add(this.tableLayoutPanel_opac_up);
            // 
            // splitContainer_opac.Panel2
            // 
            this.splitContainer_opac.Panel2.Controls.Add(this.tableLayoutPanel_opac_down);
            this.splitContainer_opac.Size = new System.Drawing.Size(786, 370);
            this.splitContainer_opac.SplitterDistance = 222;
            this.splitContainer_opac.SplitterWidth = 20;
            this.splitContainer_opac.TabIndex = 8;
            // 
            // tableLayoutPanel_opac_up
            // 
            this.tableLayoutPanel_opac_up.ColumnCount = 1;
            this.tableLayoutPanel_opac_up.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_opac_up.Controls.Add(this.listView_opacDatabases, 0, 1);
            this.tableLayoutPanel_opac_up.Controls.Add(this.toolStrip_opacDatabases, 0, 2);
            this.tableLayoutPanel_opac_up.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel_opac_up.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_opac_up.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_opac_up.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel_opac_up.Name = "tableLayoutPanel_opac_up";
            this.tableLayoutPanel_opac_up.RowCount = 3;
            this.tableLayoutPanel_opac_up.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_opac_up.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_opac_up.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_opac_up.Size = new System.Drawing.Size(786, 222);
            this.tableLayoutPanel_opac_up.TabIndex = 8;
            // 
            // listView_opacDatabases
            // 
            this.listView_opacDatabases.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView_opacDatabases.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_opac_name,
            this.columnHeader_opac_type,
            this.columnHeader_opac_alias,
            this.columnHeader_opac_visible,
            this.columnHeader_opac_comment});
            this.listView_opacDatabases.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_opacDatabases.FullRowSelect = true;
            this.listView_opacDatabases.HideSelection = false;
            this.listView_opacDatabases.Location = new System.Drawing.Point(3, 21);
            this.listView_opacDatabases.Name = "listView_opacDatabases";
            this.listView_opacDatabases.Size = new System.Drawing.Size(780, 160);
            this.listView_opacDatabases.TabIndex = 2;
            this.listView_opacDatabases.UseCompatibleStateImageBehavior = false;
            this.listView_opacDatabases.View = System.Windows.Forms.View.Details;
            this.listView_opacDatabases.SelectedIndexChanged += new System.EventHandler(this.listView_opacDatabases_SelectedIndexChanged);
            this.listView_opacDatabases.DoubleClick += new System.EventHandler(this.listView_opacDatabases_DoubleClick);
            this.listView_opacDatabases.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_opacDatabases_MouseUp);
            // 
            // columnHeader_opac_name
            // 
            this.columnHeader_opac_name.Text = "数据库名";
            this.columnHeader_opac_name.Width = 150;
            // 
            // columnHeader_opac_type
            // 
            this.columnHeader_opac_type.Text = "类型";
            this.columnHeader_opac_type.Width = 80;
            // 
            // columnHeader_opac_alias
            // 
            this.columnHeader_opac_alias.Text = "别名";
            this.columnHeader_opac_alias.Width = 100;
            // 
            // columnHeader_opac_visible
            // 
            this.columnHeader_opac_visible.Text = "显示";
            this.columnHeader_opac_visible.Width = 85;
            // 
            // columnHeader_opac_comment
            // 
            this.columnHeader_opac_comment.Text = "说明";
            this.columnHeader_opac_comment.Width = 300;
            // 
            // toolStrip_opacDatabases
            // 
            this.toolStrip_opacDatabases.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip_opacDatabases.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_opacDatabases.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip_opacDatabases.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton_insertOpacDatabase,
            this.toolStripButton_modifyOpacDatabase,
            this.toolStripButton_removeOpacDatabase,
            this.toolStripButton_refreshOpacDatabaseList});
            this.toolStrip_opacDatabases.Location = new System.Drawing.Point(0, 184);
            this.toolStrip_opacDatabases.Name = "toolStrip_opacDatabases";
            this.toolStrip_opacDatabases.Size = new System.Drawing.Size(786, 38);
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
            this.toolStripDropDownButton_insertOpacDatabase.Size = new System.Drawing.Size(75, 32);
            this.toolStripDropDownButton_insertOpacDatabase.Text = "插入";
            // 
            // toolStripMenuItem_insertOpacDatabase_normal
            // 
            this.toolStripMenuItem_insertOpacDatabase_normal.Name = "toolStripMenuItem_insertOpacDatabase_normal";
            this.toolStripMenuItem_insertOpacDatabase_normal.Size = new System.Drawing.Size(238, 40);
            this.toolStripMenuItem_insertOpacDatabase_normal.Text = "普通库(&N)...";
            this.toolStripMenuItem_insertOpacDatabase_normal.Click += new System.EventHandler(this.toolStripMenuItem_insertOpacDatabase_normal_Click);
            // 
            // toolStripMenuItem_insertOpacDatabase_virtual
            // 
            this.toolStripMenuItem_insertOpacDatabase_virtual.Name = "toolStripMenuItem_insertOpacDatabase_virtual";
            this.toolStripMenuItem_insertOpacDatabase_virtual.Size = new System.Drawing.Size(238, 40);
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
            this.toolStripButton_modifyOpacDatabase.Size = new System.Drawing.Size(58, 32);
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
            this.toolStripButton_removeOpacDatabase.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_removeOpacDatabase.Text = "移除";
            this.toolStripButton_removeOpacDatabase.Click += new System.EventHandler(this.toolStripButton_removeOpacDatabase_Click);
            // 
            // toolStripButton_refreshOpacDatabaseList
            // 
            this.toolStripButton_refreshOpacDatabaseList.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_refreshOpacDatabaseList.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_refreshOpacDatabaseList.Image")));
            this.toolStripButton_refreshOpacDatabaseList.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_refreshOpacDatabaseList.Name = "toolStripButton_refreshOpacDatabaseList";
            this.toolStripButton_refreshOpacDatabaseList.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_refreshOpacDatabaseList.Text = "刷新";
            this.toolStripButton_refreshOpacDatabaseList.Click += new System.EventHandler(this.toolStripButton_refreshOpacDatabaseList_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(224, 18);
            this.label1.TabIndex = 3;
            this.label1.Text = "参与OPAC检索的数据库(&D):";
            // 
            // tableLayoutPanel_opac_down
            // 
            this.tableLayoutPanel_opac_down.ColumnCount = 1;
            this.tableLayoutPanel_opac_down.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_opac_down.Controls.Add(this.label2, 0, 0);
            this.tableLayoutPanel_opac_down.Controls.Add(this.toolStrip_opacBrowseFormats, 0, 2);
            this.tableLayoutPanel_opac_down.Controls.Add(this.treeView_opacBrowseFormats, 0, 1);
            this.tableLayoutPanel_opac_down.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_opac_down.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_opac_down.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel_opac_down.Name = "tableLayoutPanel_opac_down";
            this.tableLayoutPanel_opac_down.RowCount = 3;
            this.tableLayoutPanel_opac_down.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_opac_down.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_opac_down.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_opac_down.Size = new System.Drawing.Size(786, 128);
            this.tableLayoutPanel_opac_down.TabIndex = 14;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(116, 18);
            this.label2.TabIndex = 12;
            this.label2.Text = "显示格式(&F):";
            // 
            // toolStrip_opacBrowseFormats
            // 
            this.toolStrip_opacBrowseFormats.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip_opacBrowseFormats.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_opacBrowseFormats.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip_opacBrowseFormats.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripDropDownButton1,
            this.toolStripButton_opacBrowseFormats_modify,
            this.toolStripButton_opacBrowseFormats_remove,
            this.toolStripButton_opacBrowseFormats_refresh});
            this.toolStrip_opacBrowseFormats.Location = new System.Drawing.Point(0, 90);
            this.toolStrip_opacBrowseFormats.Name = "toolStrip_opacBrowseFormats";
            this.toolStrip_opacBrowseFormats.Size = new System.Drawing.Size(786, 38);
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
            this.toolStripDropDownButton1.Size = new System.Drawing.Size(75, 32);
            this.toolStripDropDownButton1.Text = "插入";
            // 
            // toolStripMenuItem_opacBrowseFormats_insertDatabaseNameNode
            // 
            this.toolStripMenuItem_opacBrowseFormats_insertDatabaseNameNode.Name = "toolStripMenuItem_opacBrowseFormats_insertDatabaseNameNode";
            this.toolStripMenuItem_opacBrowseFormats_insertDatabaseNameNode.Size = new System.Drawing.Size(337, 40);
            this.toolStripMenuItem_opacBrowseFormats_insertDatabaseNameNode.Text = "插入库名节点(&N)...";
            this.toolStripMenuItem_opacBrowseFormats_insertDatabaseNameNode.Click += new System.EventHandler(this.toolStripMenuItem_opacBrowseFormats_insertDatabaseNameNode_Click);
            // 
            // toolStripMenuItem_opacBrowseFormats_insertBrowseFormatNode
            // 
            this.toolStripMenuItem_opacBrowseFormats_insertBrowseFormatNode.Name = "toolStripMenuItem_opacBrowseFormats_insertBrowseFormatNode";
            this.toolStripMenuItem_opacBrowseFormats_insertBrowseFormatNode.Size = new System.Drawing.Size(337, 40);
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
            this.toolStripButton_opacBrowseFormats_modify.Size = new System.Drawing.Size(58, 32);
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
            this.toolStripButton_opacBrowseFormats_remove.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_opacBrowseFormats_remove.Text = "移除";
            this.toolStripButton_opacBrowseFormats_remove.Click += new System.EventHandler(this.toolStripButton_opacBrowseFormats_remove_Click);
            // 
            // toolStripButton_opacBrowseFormats_refresh
            // 
            this.toolStripButton_opacBrowseFormats_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_opacBrowseFormats_refresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_opacBrowseFormats_refresh.Image")));
            this.toolStripButton_opacBrowseFormats_refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_opacBrowseFormats_refresh.Name = "toolStripButton_opacBrowseFormats_refresh";
            this.toolStripButton_opacBrowseFormats_refresh.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_opacBrowseFormats_refresh.Text = "刷新";
            this.toolStripButton_opacBrowseFormats_refresh.Click += new System.EventHandler(this.toolStripButton_opacBrowseFormats_refresh_Click);
            // 
            // treeView_opacBrowseFormats
            // 
            this.treeView_opacBrowseFormats.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeView_opacBrowseFormats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView_opacBrowseFormats.HideSelection = false;
            this.treeView_opacBrowseFormats.Location = new System.Drawing.Point(3, 21);
            this.treeView_opacBrowseFormats.Name = "treeView_opacBrowseFormats";
            this.treeView_opacBrowseFormats.Size = new System.Drawing.Size(780, 66);
            this.treeView_opacBrowseFormats.TabIndex = 11;
            this.treeView_opacBrowseFormats.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_opacBrowseFormats_AfterSelect);
            this.treeView_opacBrowseFormats.DoubleClick += new System.EventHandler(this.treeView_opacBrowseFormats_DoubleClick);
            this.treeView_opacBrowseFormats.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeView_opacBrowseFormats_MouseDown);
            this.treeView_opacBrowseFormats.MouseUp += new System.Windows.Forms.MouseEventHandler(this.treeView_opacBrowseFormats_MouseUp);
            // 
            // tabPage_dup
            // 
            this.tabPage_dup.Controls.Add(this.toolStrip_dup_main);
            this.tabPage_dup.Controls.Add(this.splitContainer_main);
            this.tabPage_dup.Location = new System.Drawing.Point(4, 28);
            this.tabPage_dup.Name = "tabPage_dup";
            this.tabPage_dup.Size = new System.Drawing.Size(802, 397);
            this.tabPage_dup.TabIndex = 2;
            this.tabPage_dup.Text = "查重";
            this.tabPage_dup.UseVisualStyleBackColor = true;
            // 
            // toolStrip_dup_main
            // 
            this.toolStrip_dup_main.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip_dup_main.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_dup_main.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip_dup_main.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_dup_refresh,
            this.toolStripButton_dup_save});
            this.toolStrip_dup_main.Location = new System.Drawing.Point(0, 359);
            this.toolStrip_dup_main.Name = "toolStrip_dup_main";
            this.toolStrip_dup_main.Size = new System.Drawing.Size(802, 38);
            this.toolStrip_dup_main.TabIndex = 9;
            this.toolStrip_dup_main.Text = "toolStrip1";
            // 
            // toolStripButton_dup_refresh
            // 
            this.toolStripButton_dup_refresh.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_dup_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_dup_refresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_dup_refresh.Image")));
            this.toolStripButton_dup_refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_dup_refresh.Name = "toolStripButton_dup_refresh";
            this.toolStripButton_dup_refresh.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_dup_refresh.Text = "刷新";
            this.toolStripButton_dup_refresh.Click += new System.EventHandler(this.toolStripButton_dup_refresh_Click);
            // 
            // toolStripButton_dup_save
            // 
            this.toolStripButton_dup_save.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_dup_save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_dup_save.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.toolStripButton_dup_save.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_dup_save.Image")));
            this.toolStripButton_dup_save.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_dup_save.Name = "toolStripButton_dup_save";
            this.toolStripButton_dup_save.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_dup_save.Text = "保存";
            this.toolStripButton_dup_save.Click += new System.EventHandler(this.toolStripButton_dup_save_Click);
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_main.Name = "splitContainer_main";
            this.splitContainer_main.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.panel_projects);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.tableLayoutPanel_dup_down);
            this.splitContainer_main.Size = new System.Drawing.Size(801, 352);
            this.splitContainer_main.SplitterDistance = 192;
            this.splitContainer_main.SplitterWidth = 20;
            this.splitContainer_main.TabIndex = 1;
            // 
            // panel_projects
            // 
            this.panel_projects.Controls.Add(this.tableLayoutPanel_dup_up);
            this.panel_projects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_projects.Location = new System.Drawing.Point(0, 0);
            this.panel_projects.Name = "panel_projects";
            this.panel_projects.Size = new System.Drawing.Size(801, 192);
            this.panel_projects.TabIndex = 0;
            // 
            // tableLayoutPanel_dup_up
            // 
            this.tableLayoutPanel_dup_up.ColumnCount = 1;
            this.tableLayoutPanel_dup_up.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_dup_up.Controls.Add(this.listView_dup_projects, 0, 0);
            this.tableLayoutPanel_dup_up.Controls.Add(this.toolStrip_dup_project, 0, 1);
            this.tableLayoutPanel_dup_up.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_dup_up.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_dup_up.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel_dup_up.Name = "tableLayoutPanel_dup_up";
            this.tableLayoutPanel_dup_up.RowCount = 2;
            this.tableLayoutPanel_dup_up.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_dup_up.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_dup_up.Size = new System.Drawing.Size(801, 192);
            this.tableLayoutPanel_dup_up.TabIndex = 9;
            // 
            // listView_dup_projects
            // 
            this.listView_dup_projects.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView_dup_projects.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader4,
            this.columnHeader5});
            this.listView_dup_projects.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_dup_projects.Font = new System.Drawing.Font("宋体", 9F);
            this.listView_dup_projects.FullRowSelect = true;
            this.listView_dup_projects.HideSelection = false;
            this.listView_dup_projects.Location = new System.Drawing.Point(3, 3);
            this.listView_dup_projects.Name = "listView_dup_projects";
            this.listView_dup_projects.Size = new System.Drawing.Size(795, 148);
            this.listView_dup_projects.TabIndex = 0;
            this.listView_dup_projects.UseCompatibleStateImageBehavior = false;
            this.listView_dup_projects.View = System.Windows.Forms.View.Details;
            this.listView_dup_projects.SelectedIndexChanged += new System.EventHandler(this.listView_dup_projects_SelectedIndexChanged);
            this.listView_dup_projects.DoubleClick += new System.EventHandler(this.listView_dup_projects_DoubleClick);
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "查重方案";
            this.columnHeader4.Width = 170;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "说明";
            this.columnHeader5.Width = 300;
            // 
            // toolStrip_dup_project
            // 
            this.toolStrip_dup_project.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip_dup_project.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_dup_project.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip_dup_project.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_dup_project_new,
            this.toolStripButton_dup_project_modify,
            this.toolStripButton_dup_project_delete});
            this.toolStrip_dup_project.Location = new System.Drawing.Point(0, 154);
            this.toolStrip_dup_project.Name = "toolStrip_dup_project";
            this.toolStrip_dup_project.Size = new System.Drawing.Size(801, 38);
            this.toolStrip_dup_project.TabIndex = 8;
            this.toolStrip_dup_project.Text = "toolStrip1";
            // 
            // toolStripButton_dup_project_new
            // 
            this.toolStripButton_dup_project_new.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_dup_project_new.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_dup_project_new.Image")));
            this.toolStripButton_dup_project_new.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_dup_project_new.Name = "toolStripButton_dup_project_new";
            this.toolStripButton_dup_project_new.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_dup_project_new.Text = "新增";
            this.toolStripButton_dup_project_new.Click += new System.EventHandler(this.toolStripButton_dup_project_new_Click);
            // 
            // toolStripButton_dup_project_modify
            // 
            this.toolStripButton_dup_project_modify.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_dup_project_modify.Enabled = false;
            this.toolStripButton_dup_project_modify.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_dup_project_modify.Image")));
            this.toolStripButton_dup_project_modify.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_dup_project_modify.Name = "toolStripButton_dup_project_modify";
            this.toolStripButton_dup_project_modify.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_dup_project_modify.Text = "修改";
            this.toolStripButton_dup_project_modify.Click += new System.EventHandler(this.toolStripButton_dup_project_modify_Click);
            // 
            // toolStripButton_dup_project_delete
            // 
            this.toolStripButton_dup_project_delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_dup_project_delete.Enabled = false;
            this.toolStripButton_dup_project_delete.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_dup_project_delete.Image")));
            this.toolStripButton_dup_project_delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_dup_project_delete.Name = "toolStripButton_dup_project_delete";
            this.toolStripButton_dup_project_delete.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_dup_project_delete.Text = "删除";
            this.toolStripButton_dup_project_delete.Click += new System.EventHandler(this.toolStripButton_dup_project_delete_Click);
            // 
            // tableLayoutPanel_dup_down
            // 
            this.tableLayoutPanel_dup_down.ColumnCount = 1;
            this.tableLayoutPanel_dup_down.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_dup_down.Controls.Add(this.label5, 0, 0);
            this.tableLayoutPanel_dup_down.Controls.Add(this.toolStrip_dup_default, 0, 2);
            this.tableLayoutPanel_dup_down.Controls.Add(this.listView_dup_defaults, 0, 1);
            this.tableLayoutPanel_dup_down.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_dup_down.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_dup_down.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel_dup_down.Name = "tableLayoutPanel_dup_down";
            this.tableLayoutPanel_dup_down.RowCount = 3;
            this.tableLayoutPanel_dup_down.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_dup_down.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_dup_down.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_dup_down.Size = new System.Drawing.Size(801, 140);
            this.tableLayoutPanel_dup_down.TabIndex = 9;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(3, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(116, 18);
            this.label5.TabIndex = 0;
            this.label5.Text = "缺省关系(&F):";
            // 
            // toolStrip_dup_default
            // 
            this.toolStrip_dup_default.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip_dup_default.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_dup_default.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip_dup_default.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_dup_default_new,
            this.toolStripButton_dup_default_modify,
            this.toolStripButton_dup_default_delete});
            this.toolStrip_dup_default.Location = new System.Drawing.Point(0, 102);
            this.toolStrip_dup_default.Name = "toolStrip_dup_default";
            this.toolStrip_dup_default.Size = new System.Drawing.Size(801, 38);
            this.toolStrip_dup_default.TabIndex = 8;
            this.toolStrip_dup_default.Text = "toolStrip1";
            // 
            // toolStripButton_dup_default_new
            // 
            this.toolStripButton_dup_default_new.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_dup_default_new.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_dup_default_new.Image")));
            this.toolStripButton_dup_default_new.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_dup_default_new.Name = "toolStripButton_dup_default_new";
            this.toolStripButton_dup_default_new.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_dup_default_new.Text = "新增";
            this.toolStripButton_dup_default_new.Click += new System.EventHandler(this.toolStripButton_dup_default_new_Click);
            // 
            // toolStripButton_dup_default_modify
            // 
            this.toolStripButton_dup_default_modify.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_dup_default_modify.Enabled = false;
            this.toolStripButton_dup_default_modify.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_dup_default_modify.Image")));
            this.toolStripButton_dup_default_modify.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_dup_default_modify.Name = "toolStripButton_dup_default_modify";
            this.toolStripButton_dup_default_modify.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_dup_default_modify.Text = "修改";
            this.toolStripButton_dup_default_modify.Click += new System.EventHandler(this.toolStripButton_dup_default_modify_Click);
            // 
            // toolStripButton_dup_default_delete
            // 
            this.toolStripButton_dup_default_delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_dup_default_delete.Enabled = false;
            this.toolStripButton_dup_default_delete.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_dup_default_delete.Image")));
            this.toolStripButton_dup_default_delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_dup_default_delete.Name = "toolStripButton_dup_default_delete";
            this.toolStripButton_dup_default_delete.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_dup_default_delete.Text = "删除";
            this.toolStripButton_dup_default_delete.Click += new System.EventHandler(this.toolStripButton_dup_default_delete_Click);
            // 
            // listView_dup_defaults
            // 
            this.listView_dup_defaults.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView_dup_defaults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_databaseName,
            this.columnHeader_defaultProjectName});
            this.listView_dup_defaults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_dup_defaults.FullRowSelect = true;
            this.listView_dup_defaults.HideSelection = false;
            this.listView_dup_defaults.Location = new System.Drawing.Point(3, 21);
            this.listView_dup_defaults.MultiSelect = false;
            this.listView_dup_defaults.Name = "listView_dup_defaults";
            this.listView_dup_defaults.Size = new System.Drawing.Size(795, 78);
            this.listView_dup_defaults.TabIndex = 1;
            this.listView_dup_defaults.UseCompatibleStateImageBehavior = false;
            this.listView_dup_defaults.View = System.Windows.Forms.View.Details;
            this.listView_dup_defaults.SelectedIndexChanged += new System.EventHandler(this.listView_dup_defaults_SelectedIndexChanged);
            this.listView_dup_defaults.DoubleClick += new System.EventHandler(this.listView_dup_defaults_DoubleClick);
            // 
            // columnHeader_databaseName
            // 
            this.columnHeader_databaseName.Text = "数据库";
            this.columnHeader_databaseName.Width = 230;
            // 
            // columnHeader_defaultProjectName
            // 
            this.columnHeader_defaultProjectName.Text = "缺省查重方案名";
            this.columnHeader_defaultProjectName.Width = 166;
            // 
            // tabPage_locations
            // 
            this.tabPage_locations.Controls.Add(this.tableLayoutPanel_locations);
            this.tabPage_locations.Location = new System.Drawing.Point(4, 28);
            this.tabPage_locations.Name = "tabPage_locations";
            this.tabPage_locations.Size = new System.Drawing.Size(802, 397);
            this.tabPage_locations.TabIndex = 4;
            this.tabPage_locations.Text = "馆藏地";
            this.tabPage_locations.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel_locations
            // 
            this.tableLayoutPanel_locations.ColumnCount = 1;
            this.tableLayoutPanel_locations.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_locations.Controls.Add(this.listView_location_list, 0, 0);
            this.tableLayoutPanel_locations.Controls.Add(this.textBox_location_comment, 0, 2);
            this.tableLayoutPanel_locations.Controls.Add(this.toolStrip_location, 0, 1);
            this.tableLayoutPanel_locations.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_locations.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_locations.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel_locations.Name = "tableLayoutPanel_locations";
            this.tableLayoutPanel_locations.RowCount = 3;
            this.tableLayoutPanel_locations.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_locations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_locations.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_locations.Size = new System.Drawing.Size(802, 397);
            this.tableLayoutPanel_locations.TabIndex = 10;
            // 
            // listView_location_list
            // 
            this.listView_location_list.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_location_list.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_location_libraryCode,
            this.columnHeader_location_name,
            this.columnHeader_location_canBorrow,
            this.columnHeader_location_canReturn,
            this.columnHeader_location_itemBarcodeNullable});
            this.listView_location_list.FullRowSelect = true;
            this.listView_location_list.HideSelection = false;
            this.listView_location_list.Location = new System.Drawing.Point(3, 3);
            this.listView_location_list.Name = "listView_location_list";
            this.listView_location_list.Size = new System.Drawing.Size(796, 279);
            this.listView_location_list.TabIndex = 0;
            this.listView_location_list.UseCompatibleStateImageBehavior = false;
            this.listView_location_list.View = System.Windows.Forms.View.Details;
            this.listView_location_list.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_location_list_ColumnClick);
            this.listView_location_list.SelectedIndexChanged += new System.EventHandler(this.listView_location_list_SelectedIndexChanged);
            this.listView_location_list.DoubleClick += new System.EventHandler(this.listView_location_list_DoubleClick);
            this.listView_location_list.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_location_list_MouseUp);
            // 
            // columnHeader_location_libraryCode
            // 
            this.columnHeader_location_libraryCode.Text = "馆代码";
            this.columnHeader_location_libraryCode.Width = 186;
            // 
            // columnHeader_location_name
            // 
            this.columnHeader_location_name.Text = "馆藏地";
            this.columnHeader_location_name.Width = 168;
            // 
            // columnHeader_location_canBorrow
            // 
            this.columnHeader_location_canBorrow.Text = "允许外借";
            this.columnHeader_location_canBorrow.Width = 100;
            // 
            // columnHeader_location_canReturn
            // 
            this.columnHeader_location_canReturn.Text = "允许还回";
            this.columnHeader_location_canReturn.Width = 100;
            // 
            // columnHeader_location_itemBarcodeNullable
            // 
            this.columnHeader_location_itemBarcodeNullable.Text = "册条码号可为空";
            this.columnHeader_location_itemBarcodeNullable.Width = 100;
            // 
            // textBox_location_comment
            // 
            this.textBox_location_comment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_location_comment.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_location_comment.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_location_comment.Location = new System.Drawing.Point(3, 326);
            this.textBox_location_comment.Multiline = true;
            this.textBox_location_comment.Name = "textBox_location_comment";
            this.textBox_location_comment.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_location_comment.Size = new System.Drawing.Size(796, 68);
            this.textBox_location_comment.TabIndex = 9;
            this.textBox_location_comment.Text = "注: 当library.xml中有ItemCanBorrow()函数时，在这里配置的关于馆藏地点是否允许外借的参数会自动失效。";
            // 
            // toolStrip_location
            // 
            this.toolStrip_location.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip_location.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip_location.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_location.ImageScalingSize = new System.Drawing.Size(24, 24);
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
            this.toolStrip_location.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStrip_location.Location = new System.Drawing.Point(0, 285);
            this.toolStrip_location.Name = "toolStrip_location";
            this.toolStrip_location.Size = new System.Drawing.Size(802, 38);
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
            this.toolStripButton_location_save.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_location_save.Text = "保存";
            this.toolStripButton_location_save.Click += new System.EventHandler(this.toolStripButton_location_save_Click);
            // 
            // toolStripButton_location_refresh
            // 
            this.toolStripButton_location_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_location_refresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_location_refresh.Image")));
            this.toolStripButton_location_refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_location_refresh.Name = "toolStripButton_location_refresh";
            this.toolStripButton_location_refresh.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_location_refresh.Text = "刷新";
            this.toolStripButton_location_refresh.Click += new System.EventHandler(this.toolStripButton_location_refresh_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton_location_up
            // 
            this.toolStripButton_location_up.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_location_up.Enabled = false;
            this.toolStripButton_location_up.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_location_up.Image")));
            this.toolStripButton_location_up.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_location_up.Name = "toolStripButton_location_up";
            this.toolStripButton_location_up.Size = new System.Drawing.Size(58, 32);
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
            this.toolStripButton_location_down.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_location_down.Text = "下移";
            this.toolStripButton_location_down.Click += new System.EventHandler(this.toolStripButton_location_down_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton_location_new
            // 
            this.toolStripButton_location_new.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_location_new.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_location_new.Image")));
            this.toolStripButton_location_new.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_location_new.Name = "toolStripButton_location_new";
            this.toolStripButton_location_new.Size = new System.Drawing.Size(58, 32);
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
            this.toolStripButton_location_modify.Size = new System.Drawing.Size(58, 32);
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
            this.toolStripButton_location_delete.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_location_delete.Text = "删除";
            this.toolStripButton_location_delete.Click += new System.EventHandler(this.toolStripButton_location_delete_Click);
            // 
            // tabPage_zhongcihaoDatabases
            // 
            this.tabPage_zhongcihaoDatabases.Controls.Add(this.toolStrip_zhongcihao);
            this.tabPage_zhongcihaoDatabases.Controls.Add(this.treeView_zhongcihao);
            this.tabPage_zhongcihaoDatabases.Location = new System.Drawing.Point(4, 28);
            this.tabPage_zhongcihaoDatabases.Name = "tabPage_zhongcihaoDatabases";
            this.tabPage_zhongcihaoDatabases.Size = new System.Drawing.Size(802, 397);
            this.tabPage_zhongcihaoDatabases.TabIndex = 5;
            this.tabPage_zhongcihaoDatabases.Text = "种次号";
            this.tabPage_zhongcihaoDatabases.UseVisualStyleBackColor = true;
            // 
            // toolStrip_zhongcihao
            // 
            this.toolStrip_zhongcihao.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip_zhongcihao.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_zhongcihao.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip_zhongcihao.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_zhongcihao_save,
            this.toolStripButton_zhongcihao_refresh,
            this.toolStripSeparator3,
            this.toolStripDropDownButton_insert,
            this.toolStripButton_zhongcihao_modify,
            this.toolStripButton_zhongcihao_remove});
            this.toolStrip_zhongcihao.Location = new System.Drawing.Point(0, 359);
            this.toolStrip_zhongcihao.Name = "toolStrip_zhongcihao";
            this.toolStrip_zhongcihao.Size = new System.Drawing.Size(802, 38);
            this.toolStrip_zhongcihao.TabIndex = 14;
            this.toolStrip_zhongcihao.Text = "toolStrip1";
            // 
            // toolStripButton_zhongcihao_save
            // 
            this.toolStripButton_zhongcihao_save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_zhongcihao_save.Enabled = false;
            this.toolStripButton_zhongcihao_save.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_zhongcihao_save.Image")));
            this.toolStripButton_zhongcihao_save.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_zhongcihao_save.Name = "toolStripButton_zhongcihao_save";
            this.toolStripButton_zhongcihao_save.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_zhongcihao_save.Text = "保存";
            this.toolStripButton_zhongcihao_save.Click += new System.EventHandler(this.toolStripButton_zhongcihao_save_Click);
            // 
            // toolStripButton_zhongcihao_refresh
            // 
            this.toolStripButton_zhongcihao_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_zhongcihao_refresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_zhongcihao_refresh.Image")));
            this.toolStripButton_zhongcihao_refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_zhongcihao_refresh.Name = "toolStripButton_zhongcihao_refresh";
            this.toolStripButton_zhongcihao_refresh.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_zhongcihao_refresh.Text = "刷新";
            this.toolStripButton_zhongcihao_refresh.Click += new System.EventHandler(this.toolStripButton_zhongcihao_refresh_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 38);
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
            this.toolStripDropDownButton_insert.Size = new System.Drawing.Size(75, 32);
            this.toolStripDropDownButton_insert.Text = "插入";
            // 
            // toolStripMenuItem_zhongcihao_insert_group
            // 
            this.toolStripMenuItem_zhongcihao_insert_group.Name = "toolStripMenuItem_zhongcihao_insert_group";
            this.toolStripMenuItem_zhongcihao_insert_group.Size = new System.Drawing.Size(339, 40);
            this.toolStripMenuItem_zhongcihao_insert_group.Text = "插入组节点(&G)...";
            this.toolStripMenuItem_zhongcihao_insert_group.Click += new System.EventHandler(this.toolStripMenuItem_zhongcihao_insert_group_Click);
            // 
            // toolStripMenuItem_zhongcihao_insert_database
            // 
            this.toolStripMenuItem_zhongcihao_insert_database.Name = "toolStripMenuItem_zhongcihao_insert_database";
            this.toolStripMenuItem_zhongcihao_insert_database.Size = new System.Drawing.Size(339, 40);
            this.toolStripMenuItem_zhongcihao_insert_database.Text = "插入书目库名节点(&B)...";
            this.toolStripMenuItem_zhongcihao_insert_database.Click += new System.EventHandler(this.toolStripMenuItem_zhongcihao_insert_database_Click);
            // 
            // ToolStripMenuItem_zhongcihao_insert_nstable
            // 
            this.ToolStripMenuItem_zhongcihao_insert_nstable.Name = "ToolStripMenuItem_zhongcihao_insert_nstable";
            this.ToolStripMenuItem_zhongcihao_insert_nstable.Size = new System.Drawing.Size(339, 40);
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
            this.toolStripButton_zhongcihao_modify.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_zhongcihao_modify.Text = "修改";
            this.toolStripButton_zhongcihao_modify.Click += new System.EventHandler(this.toolStripButton_zhongcihao_modify_Click);
            // 
            // toolStripButton_zhongcihao_remove
            // 
            this.toolStripButton_zhongcihao_remove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_zhongcihao_remove.Enabled = false;
            this.toolStripButton_zhongcihao_remove.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_zhongcihao_remove.Image")));
            this.toolStripButton_zhongcihao_remove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_zhongcihao_remove.Name = "toolStripButton_zhongcihao_remove";
            this.toolStripButton_zhongcihao_remove.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_zhongcihao_remove.Text = "移除";
            this.toolStripButton_zhongcihao_remove.Click += new System.EventHandler(this.toolStripButton_zhongcihao_remove_Click);
            // 
            // treeView_zhongcihao
            // 
            this.treeView_zhongcihao.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView_zhongcihao.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeView_zhongcihao.HideSelection = false;
            this.treeView_zhongcihao.Location = new System.Drawing.Point(6, 38);
            this.treeView_zhongcihao.Name = "treeView_zhongcihao";
            this.treeView_zhongcihao.Size = new System.Drawing.Size(786, 311);
            this.treeView_zhongcihao.TabIndex = 12;
            this.treeView_zhongcihao.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_zhongcihao_AfterSelect);
            this.treeView_zhongcihao.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeView_zhongcihao_MouseDown);
            this.treeView_zhongcihao.MouseUp += new System.Windows.Forms.MouseEventHandler(this.treeView_zhongcihao_MouseUp);
            // 
            // tabPage_bookshelf
            // 
            this.tabPage_bookshelf.Controls.Add(this.toolStrip_arrangement);
            this.tabPage_bookshelf.Controls.Add(this.treeView_arrangement);
            this.tabPage_bookshelf.Location = new System.Drawing.Point(4, 28);
            this.tabPage_bookshelf.Name = "tabPage_bookshelf";
            this.tabPage_bookshelf.Size = new System.Drawing.Size(802, 397);
            this.tabPage_bookshelf.TabIndex = 7;
            this.tabPage_bookshelf.Text = "排架体系";
            this.tabPage_bookshelf.UseVisualStyleBackColor = true;
            // 
            // toolStrip_arrangement
            // 
            this.toolStrip_arrangement.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip_arrangement.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_arrangement.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip_arrangement.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_arrangement_save,
            this.toolStripButton_arrangement_refresh,
            this.toolStripSeparator5,
            this.toolStripDropDownButton2,
            this.toolStripButton_arrangement_modify,
            this.toolStripButton_arrangement_remove,
            this.toolStripButton_arrangement_viewXml});
            this.toolStrip_arrangement.Location = new System.Drawing.Point(0, 359);
            this.toolStrip_arrangement.Name = "toolStrip_arrangement";
            this.toolStrip_arrangement.Size = new System.Drawing.Size(802, 38);
            this.toolStrip_arrangement.TabIndex = 16;
            this.toolStrip_arrangement.Text = "toolStrip1";
            // 
            // toolStripButton_arrangement_save
            // 
            this.toolStripButton_arrangement_save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_arrangement_save.Enabled = false;
            this.toolStripButton_arrangement_save.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_arrangement_save.Image")));
            this.toolStripButton_arrangement_save.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_arrangement_save.Name = "toolStripButton_arrangement_save";
            this.toolStripButton_arrangement_save.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_arrangement_save.Text = "保存";
            this.toolStripButton_arrangement_save.Click += new System.EventHandler(this.toolStripButton_arrangement_save_Click);
            // 
            // toolStripButton_arrangement_refresh
            // 
            this.toolStripButton_arrangement_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_arrangement_refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_arrangement_refresh.Name = "toolStripButton_arrangement_refresh";
            this.toolStripButton_arrangement_refresh.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_arrangement_refresh.Text = "刷新";
            this.toolStripButton_arrangement_refresh.Click += new System.EventHandler(this.toolStripButton_arrangement_refresh_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripDropDownButton2
            // 
            this.toolStripDropDownButton2.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripDropDownButton2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_arrangement_insert_group,
            this.toolStripMenuItem_arrangement_insert_location});
            this.toolStripDropDownButton2.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton2.Image")));
            this.toolStripDropDownButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton2.Name = "toolStripDropDownButton2";
            this.toolStripDropDownButton2.Size = new System.Drawing.Size(75, 32);
            this.toolStripDropDownButton2.Text = "插入";
            // 
            // toolStripMenuItem_arrangement_insert_group
            // 
            this.toolStripMenuItem_arrangement_insert_group.Name = "toolStripMenuItem_arrangement_insert_group";
            this.toolStripMenuItem_arrangement_insert_group.Size = new System.Drawing.Size(360, 40);
            this.toolStripMenuItem_arrangement_insert_group.Text = "插入排架体系节点(&G)...";
            this.toolStripMenuItem_arrangement_insert_group.Click += new System.EventHandler(this.toolStripMenuItem_arrangement_insert_group_Click);
            // 
            // toolStripMenuItem_arrangement_insert_location
            // 
            this.toolStripMenuItem_arrangement_insert_location.Name = "toolStripMenuItem_arrangement_insert_location";
            this.toolStripMenuItem_arrangement_insert_location.Size = new System.Drawing.Size(360, 40);
            this.toolStripMenuItem_arrangement_insert_location.Text = "插入馆藏地点名节点(&B)...";
            this.toolStripMenuItem_arrangement_insert_location.Click += new System.EventHandler(this.toolStripMenuItem_arrangement_insert_location_Click);
            // 
            // toolStripButton_arrangement_modify
            // 
            this.toolStripButton_arrangement_modify.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_arrangement_modify.Enabled = false;
            this.toolStripButton_arrangement_modify.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_arrangement_modify.Image")));
            this.toolStripButton_arrangement_modify.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_arrangement_modify.Name = "toolStripButton_arrangement_modify";
            this.toolStripButton_arrangement_modify.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_arrangement_modify.Text = "修改";
            this.toolStripButton_arrangement_modify.Click += new System.EventHandler(this.toolStripButton_arrangement_modify_Click);
            // 
            // toolStripButton_arrangement_remove
            // 
            this.toolStripButton_arrangement_remove.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_arrangement_remove.Enabled = false;
            this.toolStripButton_arrangement_remove.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_arrangement_remove.Name = "toolStripButton_arrangement_remove";
            this.toolStripButton_arrangement_remove.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_arrangement_remove.Text = "移除";
            this.toolStripButton_arrangement_remove.Click += new System.EventHandler(this.toolStripButton_arrangement_remove_Click);
            // 
            // toolStripButton_arrangement_viewXml
            // 
            this.toolStripButton_arrangement_viewXml.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_arrangement_viewXml.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_arrangement_viewXml.Image")));
            this.toolStripButton_arrangement_viewXml.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_arrangement_viewXml.Name = "toolStripButton_arrangement_viewXml";
            this.toolStripButton_arrangement_viewXml.Size = new System.Drawing.Size(188, 32);
            this.toolStripButton_arrangement_viewXml.Text = "观察XML定义代码";
            this.toolStripButton_arrangement_viewXml.Click += new System.EventHandler(this.toolStripButton_arrangement_viewXml_Click);
            // 
            // treeView_arrangement
            // 
            this.treeView_arrangement.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView_arrangement.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeView_arrangement.HideSelection = false;
            this.treeView_arrangement.Location = new System.Drawing.Point(6, 36);
            this.treeView_arrangement.Name = "treeView_arrangement";
            this.treeView_arrangement.Size = new System.Drawing.Size(786, 312);
            this.treeView_arrangement.TabIndex = 15;
            this.treeView_arrangement.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView_arrangement_AfterSelect);
            this.treeView_arrangement.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeView_arrangement_MouseDown);
            this.treeView_arrangement.MouseUp += new System.Windows.Forms.MouseEventHandler(this.treeView_arrangement_MouseUp);
            // 
            // tabPage_script
            // 
            this.tabPage_script.Controls.Add(this.splitContainer_script);
            this.tabPage_script.Location = new System.Drawing.Point(4, 28);
            this.tabPage_script.Name = "tabPage_script";
            this.tabPage_script.Size = new System.Drawing.Size(802, 397);
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
            this.splitContainer_script.Panel1.Controls.Add(this.tableLayoutPanel_script);
            this.splitContainer_script.Panel1.Padding = new System.Windows.Forms.Padding(0, 9, 0, 0);
            // 
            // splitContainer_script.Panel2
            // 
            this.splitContainer_script.Panel2.Controls.Add(this.textBox_script_comment);
            this.splitContainer_script.Panel2.Padding = new System.Windows.Forms.Padding(0, 0, 0, 9);
            this.splitContainer_script.Size = new System.Drawing.Size(802, 397);
            this.splitContainer_script.SplitterDistance = 342;
            this.splitContainer_script.SplitterWidth = 12;
            this.splitContainer_script.TabIndex = 12;
            // 
            // tableLayoutPanel_script
            // 
            this.tableLayoutPanel_script.ColumnCount = 1;
            this.tableLayoutPanel_script.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_script.Controls.Add(this.textBox_script, 0, 0);
            this.tableLayoutPanel_script.Controls.Add(this.toolStrip_script, 0, 1);
            this.tableLayoutPanel_script.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_script.Location = new System.Drawing.Point(0, 9);
            this.tableLayoutPanel_script.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel_script.Name = "tableLayoutPanel_script";
            this.tableLayoutPanel_script.RowCount = 2;
            this.tableLayoutPanel_script.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_script.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_script.Size = new System.Drawing.Size(802, 333);
            this.tableLayoutPanel_script.TabIndex = 12;
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
            this.textBox_script.Location = new System.Drawing.Point(3, 3);
            this.textBox_script.MaxLength = 0;
            this.textBox_script.Multiline = true;
            this.textBox_script.Name = "textBox_script";
            this.textBox_script.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_script.Size = new System.Drawing.Size(796, 289);
            this.textBox_script.TabIndex = 2;
            this.textBox_script.TextChanged += new System.EventHandler(this.textBox_script_TextChanged);
            this.textBox_script.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox_script_KeyDown);
            this.textBox_script.MouseUp += new System.Windows.Forms.MouseEventHandler(this.textBox_script_MouseUp);
            // 
            // toolStrip_script
            // 
            this.toolStrip_script.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStrip_script.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_script.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip_script.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_script_save,
            this.toolStripButton_script_refresh,
            this.toolStripLabel_script_caretPos});
            this.toolStrip_script.Location = new System.Drawing.Point(0, 295);
            this.toolStrip_script.Name = "toolStrip_script";
            this.toolStrip_script.Size = new System.Drawing.Size(802, 38);
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
            this.toolStripButton_script_save.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_script_save.Text = "保存";
            this.toolStripButton_script_save.Click += new System.EventHandler(this.toolStripButton_script_save_Click);
            // 
            // toolStripButton_script_refresh
            // 
            this.toolStripButton_script_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_script_refresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_script_refresh.Image")));
            this.toolStripButton_script_refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_script_refresh.Name = "toolStripButton_script_refresh";
            this.toolStripButton_script_refresh.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_script_refresh.Text = "刷新";
            this.toolStripButton_script_refresh.Click += new System.EventHandler(this.toolStripButton_script_refresh_Click);
            // 
            // toolStripLabel_script_caretPos
            // 
            this.toolStripLabel_script_caretPos.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel_script_caretPos.Name = "toolStripLabel_script_caretPos";
            this.toolStripLabel_script_caretPos.Size = new System.Drawing.Size(0, 32);
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
            this.textBox_script_comment.Size = new System.Drawing.Size(802, 34);
            this.textBox_script_comment.TabIndex = 10;
            this.textBox_script_comment.DoubleClick += new System.EventHandler(this.textBox_script_comment_DoubleClick);
            // 
            // tabPage_barcodeValidation
            // 
            this.tabPage_barcodeValidation.Controls.Add(this.tableLayoutPanel_barcodeValidation);
            this.tabPage_barcodeValidation.Location = new System.Drawing.Point(4, 28);
            this.tabPage_barcodeValidation.Name = "tabPage_barcodeValidation";
            this.tabPage_barcodeValidation.Size = new System.Drawing.Size(802, 397);
            this.tabPage_barcodeValidation.TabIndex = 13;
            this.tabPage_barcodeValidation.Text = "条码校验";
            this.tabPage_barcodeValidation.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel_barcodeValidation
            // 
            this.tableLayoutPanel_barcodeValidation.ColumnCount = 1;
            this.tableLayoutPanel_barcodeValidation.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_barcodeValidation.Controls.Add(this.textBox_barcodeValidation, 0, 0);
            this.tableLayoutPanel_barcodeValidation.Controls.Add(this.toolStrip_barcodeValidation, 0, 1);
            this.tableLayoutPanel_barcodeValidation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_barcodeValidation.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_barcodeValidation.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel_barcodeValidation.Name = "tableLayoutPanel_barcodeValidation";
            this.tableLayoutPanel_barcodeValidation.RowCount = 2;
            this.tableLayoutPanel_barcodeValidation.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_barcodeValidation.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_barcodeValidation.Size = new System.Drawing.Size(802, 397);
            this.tableLayoutPanel_barcodeValidation.TabIndex = 12;
            // 
            // textBox_barcodeValidation
            // 
            this.textBox_barcodeValidation.AcceptsReturn = true;
            this.textBox_barcodeValidation.AcceptsTab = true;
            this.textBox_barcodeValidation.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_barcodeValidation.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_barcodeValidation.HideSelection = false;
            this.textBox_barcodeValidation.Location = new System.Drawing.Point(3, 3);
            this.textBox_barcodeValidation.MaxLength = 0;
            this.textBox_barcodeValidation.Multiline = true;
            this.textBox_barcodeValidation.Name = "textBox_barcodeValidation";
            this.textBox_barcodeValidation.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_barcodeValidation.Size = new System.Drawing.Size(796, 353);
            this.textBox_barcodeValidation.TabIndex = 2;
            this.textBox_barcodeValidation.TextChanged += new System.EventHandler(this.textBox_barcodeValidation_TextChanged);
            // 
            // toolStrip_barcodeValidation
            // 
            this.toolStrip_barcodeValidation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStrip_barcodeValidation.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_barcodeValidation.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip_barcodeValidation.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_barcodeValidation_save,
            this.toolStripButton_barcodeValidation_refresh,
            this.toolStripLabel2});
            this.toolStrip_barcodeValidation.Location = new System.Drawing.Point(0, 359);
            this.toolStrip_barcodeValidation.Name = "toolStrip_barcodeValidation";
            this.toolStrip_barcodeValidation.Size = new System.Drawing.Size(802, 38);
            this.toolStrip_barcodeValidation.TabIndex = 11;
            this.toolStrip_barcodeValidation.Text = "toolStrip1";
            // 
            // toolStripButton_barcodeValidation_save
            // 
            this.toolStripButton_barcodeValidation_save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_barcodeValidation_save.Enabled = false;
            this.toolStripButton_barcodeValidation_save.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_barcodeValidation_save.Image")));
            this.toolStripButton_barcodeValidation_save.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_barcodeValidation_save.Name = "toolStripButton_barcodeValidation_save";
            this.toolStripButton_barcodeValidation_save.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_barcodeValidation_save.Text = "保存";
            this.toolStripButton_barcodeValidation_save.Click += new System.EventHandler(this.toolStripButton_barcodeValidation_save_Click);
            // 
            // toolStripButton_barcodeValidation_refresh
            // 
            this.toolStripButton_barcodeValidation_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_barcodeValidation_refresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_barcodeValidation_refresh.Image")));
            this.toolStripButton_barcodeValidation_refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_barcodeValidation_refresh.Name = "toolStripButton_barcodeValidation_refresh";
            this.toolStripButton_barcodeValidation_refresh.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_barcodeValidation_refresh.Text = "刷新";
            this.toolStripButton_barcodeValidation_refresh.Click += new System.EventHandler(this.toolStripButton_barcodeValidation_refresh_Click);
            // 
            // toolStripLabel2
            // 
            this.toolStripLabel2.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel2.Name = "toolStripLabel2";
            this.toolStripLabel2.Size = new System.Drawing.Size(0, 32);
            // 
            // tabPage_valueTable
            // 
            this.tabPage_valueTable.Controls.Add(this.tableLayoutPanel1);
            this.tabPage_valueTable.Location = new System.Drawing.Point(4, 28);
            this.tabPage_valueTable.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_valueTable.Name = "tabPage_valueTable";
            this.tabPage_valueTable.Size = new System.Drawing.Size(802, 397);
            this.tabPage_valueTable.TabIndex = 8;
            this.tabPage_valueTable.Text = "值列表";
            this.tabPage_valueTable.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.textBox_valueTables, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.toolStrip_valueTables, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(802, 397);
            this.tableLayoutPanel1.TabIndex = 13;
            // 
            // textBox_valueTables
            // 
            this.textBox_valueTables.AcceptsReturn = true;
            this.textBox_valueTables.AcceptsTab = true;
            this.textBox_valueTables.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_valueTables.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_valueTables.HideSelection = false;
            this.textBox_valueTables.Location = new System.Drawing.Point(3, 3);
            this.textBox_valueTables.MaxLength = 0;
            this.textBox_valueTables.Multiline = true;
            this.textBox_valueTables.Name = "textBox_valueTables";
            this.textBox_valueTables.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_valueTables.Size = new System.Drawing.Size(796, 353);
            this.textBox_valueTables.TabIndex = 2;
            this.textBox_valueTables.TextChanged += new System.EventHandler(this.textBox_valueTables_TextChanged);
            // 
            // toolStrip_valueTables
            // 
            this.toolStrip_valueTables.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStrip_valueTables.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_valueTables.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip_valueTables.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_valueTable_save,
            this.toolStripButton_valueTable_refresh,
            this.toolStripLabel1});
            this.toolStrip_valueTables.Location = new System.Drawing.Point(0, 359);
            this.toolStrip_valueTables.Name = "toolStrip_valueTables";
            this.toolStrip_valueTables.Size = new System.Drawing.Size(802, 38);
            this.toolStrip_valueTables.TabIndex = 11;
            this.toolStrip_valueTables.Text = "toolStrip1";
            // 
            // toolStripButton_valueTable_save
            // 
            this.toolStripButton_valueTable_save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_valueTable_save.Enabled = false;
            this.toolStripButton_valueTable_save.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_valueTable_save.Image")));
            this.toolStripButton_valueTable_save.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_valueTable_save.Name = "toolStripButton_valueTable_save";
            this.toolStripButton_valueTable_save.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_valueTable_save.Text = "保存";
            this.toolStripButton_valueTable_save.Click += new System.EventHandler(this.toolStripButton_valueTable_save_Click);
            // 
            // toolStripButton_valueTable_refresh
            // 
            this.toolStripButton_valueTable_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_valueTable_refresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_valueTable_refresh.Image")));
            this.toolStripButton_valueTable_refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_valueTable_refresh.Name = "toolStripButton_valueTable_refresh";
            this.toolStripButton_valueTable_refresh.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_valueTable_refresh.Text = "刷新";
            this.toolStripButton_valueTable_refresh.Click += new System.EventHandler(this.toolStripButton_valueTable_refresh_Click);
            // 
            // toolStripLabel1
            // 
            this.toolStripLabel1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel1.Name = "toolStripLabel1";
            this.toolStripLabel1.Size = new System.Drawing.Size(0, 32);
            // 
            // tabPage_center
            // 
            this.tabPage_center.Controls.Add(this.listView_center);
            this.tabPage_center.Controls.Add(this.toolStrip_center);
            this.tabPage_center.Location = new System.Drawing.Point(4, 28);
            this.tabPage_center.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_center.Name = "tabPage_center";
            this.tabPage_center.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_center.Size = new System.Drawing.Size(802, 397);
            this.tabPage_center.TabIndex = 9;
            this.tabPage_center.Text = "中心服务器";
            this.tabPage_center.UseVisualStyleBackColor = true;
            // 
            // listView_center
            // 
            this.listView_center.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_center.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView_center.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_center_name,
            this.columnHeader_center_url,
            this.columnHeader_center_userName,
            this.columnHeader_center_refid});
            this.listView_center.FullRowSelect = true;
            this.listView_center.HideSelection = false;
            this.listView_center.Location = new System.Drawing.Point(6, 6);
            this.listView_center.Name = "listView_center";
            this.listView_center.Size = new System.Drawing.Size(785, 340);
            this.listView_center.TabIndex = 0;
            this.listView_center.UseCompatibleStateImageBehavior = false;
            this.listView_center.View = System.Windows.Forms.View.Details;
            this.listView_center.SelectedIndexChanged += new System.EventHandler(this.listView_center_SelectedIndexChanged);
            this.listView_center.DoubleClick += new System.EventHandler(this.listView_center_DoubleClick);
            this.listView_center.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_center_MouseUp);
            // 
            // columnHeader_center_name
            // 
            this.columnHeader_center_name.Text = "中心名称";
            this.columnHeader_center_name.Width = 139;
            // 
            // columnHeader_center_url
            // 
            this.columnHeader_center_url.Text = "URL";
            this.columnHeader_center_url.Width = 258;
            // 
            // columnHeader_center_userName
            // 
            this.columnHeader_center_userName.Text = "用户名";
            this.columnHeader_center_userName.Width = 100;
            // 
            // columnHeader_center_refid
            // 
            this.columnHeader_center_refid.Text = "参考 ID";
            this.columnHeader_center_refid.Width = 280;
            // 
            // toolStrip_center
            // 
            this.toolStrip_center.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip_center.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_center.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip_center.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_center_modify,
            this.toolStripButton_center_add,
            this.toolStripButton_center_delete,
            this.toolStripButton_center_refresh,
            this.toolStripSeparator6,
            this.toolStripButton_center_save});
            this.toolStrip_center.Location = new System.Drawing.Point(3, 356);
            this.toolStrip_center.Name = "toolStrip_center";
            this.toolStrip_center.Size = new System.Drawing.Size(796, 38);
            this.toolStrip_center.TabIndex = 1;
            this.toolStrip_center.Text = "toolStrip1";
            // 
            // toolStripButton_center_modify
            // 
            this.toolStripButton_center_modify.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_center_modify.Enabled = false;
            this.toolStripButton_center_modify.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_center_modify.Image")));
            this.toolStripButton_center_modify.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_center_modify.Name = "toolStripButton_center_modify";
            this.toolStripButton_center_modify.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_center_modify.Text = "修改";
            this.toolStripButton_center_modify.Click += new System.EventHandler(this.toolStripButton_center_modify_Click);
            // 
            // toolStripButton_center_add
            // 
            this.toolStripButton_center_add.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_center_add.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_center_add.Image")));
            this.toolStripButton_center_add.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_center_add.Name = "toolStripButton_center_add";
            this.toolStripButton_center_add.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_center_add.Text = "添加";
            this.toolStripButton_center_add.Click += new System.EventHandler(this.toolStripButton_center_add_Click);
            // 
            // toolStripButton_center_delete
            // 
            this.toolStripButton_center_delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_center_delete.Enabled = false;
            this.toolStripButton_center_delete.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_center_delete.Image")));
            this.toolStripButton_center_delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_center_delete.Name = "toolStripButton_center_delete";
            this.toolStripButton_center_delete.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_center_delete.Text = "删除";
            this.toolStripButton_center_delete.Click += new System.EventHandler(this.toolStripButton_center_delete_Click);
            // 
            // toolStripButton_center_refresh
            // 
            this.toolStripButton_center_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_center_refresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_center_refresh.Image")));
            this.toolStripButton_center_refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_center_refresh.Name = "toolStripButton_center_refresh";
            this.toolStripButton_center_refresh.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_center_refresh.Text = "刷新";
            this.toolStripButton_center_refresh.Click += new System.EventHandler(this.toolStripButton_center_refresh_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton_center_save
            // 
            this.toolStripButton_center_save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_center_save.Enabled = false;
            this.toolStripButton_center_save.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_center_save.Image")));
            this.toolStripButton_center_save.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_center_save.Name = "toolStripButton_center_save";
            this.toolStripButton_center_save.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_center_save.Text = "保存";
            // 
            // tabPage_newLoanPolicy
            // 
            this.tabPage_newLoanPolicy.Controls.Add(this.tabControl_newLoanPolicy);
            this.tabPage_newLoanPolicy.Controls.Add(this.toolStrip_newLoanPolicy);
            this.tabPage_newLoanPolicy.Location = new System.Drawing.Point(4, 28);
            this.tabPage_newLoanPolicy.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_newLoanPolicy.Name = "tabPage_newLoanPolicy";
            this.tabPage_newLoanPolicy.Size = new System.Drawing.Size(802, 397);
            this.tabPage_newLoanPolicy.TabIndex = 10;
            this.tabPage_newLoanPolicy.Text = "流通权限";
            this.tabPage_newLoanPolicy.UseVisualStyleBackColor = true;
            // 
            // tabControl_newLoanPolicy
            // 
            this.tabControl_newLoanPolicy.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_newLoanPolicy.Controls.Add(this.tabPage_newLoanPolicy_rightsTable);
            this.tabControl_newLoanPolicy.Controls.Add(this.tabPage_newLoanPolicy_xml);
            this.tabControl_newLoanPolicy.Location = new System.Drawing.Point(4, 4);
            this.tabControl_newLoanPolicy.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl_newLoanPolicy.Name = "tabControl_newLoanPolicy";
            this.tabControl_newLoanPolicy.SelectedIndex = 0;
            this.tabControl_newLoanPolicy.Size = new System.Drawing.Size(789, 344);
            this.tabControl_newLoanPolicy.TabIndex = 9;
            this.tabControl_newLoanPolicy.SelectedIndexChanged += new System.EventHandler(this.tabControl_newLoanPolicy_SelectedIndexChanged);
            // 
            // tabPage_newLoanPolicy_rightsTable
            // 
            this.tabPage_newLoanPolicy_rightsTable.Controls.Add(this.elementHost1);
            this.tabPage_newLoanPolicy_rightsTable.Location = new System.Drawing.Point(4, 28);
            this.tabPage_newLoanPolicy_rightsTable.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_newLoanPolicy_rightsTable.Name = "tabPage_newLoanPolicy_rightsTable";
            this.tabPage_newLoanPolicy_rightsTable.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage_newLoanPolicy_rightsTable.Size = new System.Drawing.Size(781, 312);
            this.tabPage_newLoanPolicy_rightsTable.TabIndex = 0;
            this.tabPage_newLoanPolicy_rightsTable.Text = "权限表";
            this.tabPage_newLoanPolicy_rightsTable.UseVisualStyleBackColor = true;
            // 
            // elementHost1
            // 
            this.elementHost1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.elementHost1.Location = new System.Drawing.Point(4, 4);
            this.elementHost1.Margin = new System.Windows.Forms.Padding(4);
            this.elementHost1.Name = "elementHost1";
            this.elementHost1.Size = new System.Drawing.Size(773, 304);
            this.elementHost1.TabIndex = 1;
            this.elementHost1.Text = "elementHost1";
            this.elementHost1.Child = this.loanPolicyControlWrapper1;
            // 
            // tabPage_newLoanPolicy_xml
            // 
            this.tabPage_newLoanPolicy_xml.Controls.Add(this.textBox_newLoanPolicy_xml);
            this.tabPage_newLoanPolicy_xml.Location = new System.Drawing.Point(4, 28);
            this.tabPage_newLoanPolicy_xml.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_newLoanPolicy_xml.Name = "tabPage_newLoanPolicy_xml";
            this.tabPage_newLoanPolicy_xml.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage_newLoanPolicy_xml.Size = new System.Drawing.Size(781, 312);
            this.tabPage_newLoanPolicy_xml.TabIndex = 1;
            this.tabPage_newLoanPolicy_xml.Text = "XML";
            this.tabPage_newLoanPolicy_xml.UseVisualStyleBackColor = true;
            // 
            // textBox_newLoanPolicy_xml
            // 
            this.textBox_newLoanPolicy_xml.AcceptsReturn = true;
            this.textBox_newLoanPolicy_xml.AcceptsTab = true;
            this.textBox_newLoanPolicy_xml.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_newLoanPolicy_xml.Font = new System.Drawing.Font("Courier New", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBox_newLoanPolicy_xml.HideSelection = false;
            this.textBox_newLoanPolicy_xml.Location = new System.Drawing.Point(4, 4);
            this.textBox_newLoanPolicy_xml.MaxLength = 0;
            this.textBox_newLoanPolicy_xml.Multiline = true;
            this.textBox_newLoanPolicy_xml.Name = "textBox_newLoanPolicy_xml";
            this.textBox_newLoanPolicy_xml.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_newLoanPolicy_xml.Size = new System.Drawing.Size(773, 304);
            this.textBox_newLoanPolicy_xml.TabIndex = 2;
            this.textBox_newLoanPolicy_xml.TextChanged += new System.EventHandler(this.textBox_newLoanPolicy_xml_TextChanged);
            // 
            // toolStrip_newLoanPolicy
            // 
            this.toolStrip_newLoanPolicy.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip_newLoanPolicy.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_newLoanPolicy.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip_newLoanPolicy.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_newLoanPolicy_save,
            this.toolStripButton_newLoanPolicy_refresh});
            this.toolStrip_newLoanPolicy.Location = new System.Drawing.Point(0, 359);
            this.toolStrip_newLoanPolicy.Name = "toolStrip_newLoanPolicy";
            this.toolStrip_newLoanPolicy.Size = new System.Drawing.Size(802, 38);
            this.toolStrip_newLoanPolicy.TabIndex = 8;
            this.toolStrip_newLoanPolicy.Text = "toolStrip1";
            // 
            // toolStripButton_newLoanPolicy_save
            // 
            this.toolStripButton_newLoanPolicy_save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_newLoanPolicy_save.Enabled = false;
            this.toolStripButton_newLoanPolicy_save.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_newLoanPolicy_save.Image")));
            this.toolStripButton_newLoanPolicy_save.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_newLoanPolicy_save.Name = "toolStripButton_newLoanPolicy_save";
            this.toolStripButton_newLoanPolicy_save.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_newLoanPolicy_save.Text = "保存";
            this.toolStripButton_newLoanPolicy_save.Click += new System.EventHandler(this.toolStripButton_newLoanPolicy_save_Click);
            // 
            // toolStripButton_newLoanPolicy_refresh
            // 
            this.toolStripButton_newLoanPolicy_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_newLoanPolicy_refresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_newLoanPolicy_refresh.Image")));
            this.toolStripButton_newLoanPolicy_refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_newLoanPolicy_refresh.Name = "toolStripButton_newLoanPolicy_refresh";
            this.toolStripButton_newLoanPolicy_refresh.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_newLoanPolicy_refresh.Text = "刷新";
            this.toolStripButton_newLoanPolicy_refresh.Click += new System.EventHandler(this.toolStripButton_newLoanPolicy_refresh_Click);
            // 
            // tabPage_calendar
            // 
            this.tabPage_calendar.Controls.Add(this.listView_calendar);
            this.tabPage_calendar.Controls.Add(this.toolStrip_calendar);
            this.tabPage_calendar.Location = new System.Drawing.Point(4, 28);
            this.tabPage_calendar.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_calendar.Name = "tabPage_calendar";
            this.tabPage_calendar.Size = new System.Drawing.Size(802, 397);
            this.tabPage_calendar.TabIndex = 11;
            this.tabPage_calendar.Text = "开馆日历";
            this.tabPage_calendar.UseVisualStyleBackColor = true;
            // 
            // listView_calendar
            // 
            this.listView_calendar.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_calendar.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_calendar_name,
            this.columnHeader_calendar_range,
            this.columnHeader_calendar_comment,
            this.columnHeader_calendar_content});
            this.listView_calendar.FullRowSelect = true;
            this.listView_calendar.HideSelection = false;
            this.listView_calendar.LargeImageList = this.imageList_itemType;
            this.listView_calendar.Location = new System.Drawing.Point(4, 3);
            this.listView_calendar.Name = "listView_calendar";
            this.listView_calendar.Size = new System.Drawing.Size(790, 344);
            this.listView_calendar.SmallImageList = this.imageList_itemType;
            this.listView_calendar.TabIndex = 9;
            this.listView_calendar.UseCompatibleStateImageBehavior = false;
            this.listView_calendar.View = System.Windows.Forms.View.Details;
            this.listView_calendar.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_calendar_ColumnClick);
            this.listView_calendar.SelectedIndexChanged += new System.EventHandler(this.listView_calendar_SelectedIndexChanged);
            this.listView_calendar.DoubleClick += new System.EventHandler(this.listView_calendar_DoubleClick);
            this.listView_calendar.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_calendar_MouseUp);
            // 
            // columnHeader_calendar_name
            // 
            this.columnHeader_calendar_name.Text = "日历名";
            this.columnHeader_calendar_name.Width = 186;
            // 
            // columnHeader_calendar_range
            // 
            this.columnHeader_calendar_range.Text = "时间范围";
            this.columnHeader_calendar_range.Width = 168;
            // 
            // columnHeader_calendar_comment
            // 
            this.columnHeader_calendar_comment.Text = "说明";
            this.columnHeader_calendar_comment.Width = 200;
            // 
            // columnHeader_calendar_content
            // 
            this.columnHeader_calendar_content.Text = "内容";
            this.columnHeader_calendar_content.Width = 200;
            // 
            // imageList_itemType
            // 
            this.imageList_itemType.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_itemType.ImageStream")));
            this.imageList_itemType.TransparentColor = System.Drawing.Color.Magenta;
            this.imageList_itemType.Images.SetKeyName(0, "normal_entity.bmp");
            this.imageList_itemType.Images.SetKeyName(1, "new_entity.bmp");
            this.imageList_itemType.Images.SetKeyName(2, "changed_entity.bmp");
            this.imageList_itemType.Images.SetKeyName(3, "deleted_entity.bmp");
            this.imageList_itemType.Images.SetKeyName(4, "error_entity.bmp");
            // 
            // toolStrip_calendar
            // 
            this.toolStrip_calendar.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.toolStrip_calendar.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_calendar.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip_calendar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_calendar_save,
            this.toolStripButton_calendar_refresh,
            this.toolStripSeparator7,
            this.toolStripButton3,
            this.toolStripButton4,
            this.toolStripSeparator8,
            this.toolStripButton_calendar_new,
            this.toolStripButton_calendar_modify,
            this.toolStripButton_calendar_delete});
            this.toolStrip_calendar.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
            this.toolStrip_calendar.Location = new System.Drawing.Point(0, 359);
            this.toolStrip_calendar.Name = "toolStrip_calendar";
            this.toolStrip_calendar.Size = new System.Drawing.Size(802, 38);
            this.toolStrip_calendar.TabIndex = 10;
            this.toolStrip_calendar.Text = "toolStrip1";
            // 
            // toolStripButton_calendar_save
            // 
            this.toolStripButton_calendar_save.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_calendar_save.Enabled = false;
            this.toolStripButton_calendar_save.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_calendar_save.Image")));
            this.toolStripButton_calendar_save.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_calendar_save.Name = "toolStripButton_calendar_save";
            this.toolStripButton_calendar_save.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_calendar_save.Text = "保存";
            this.toolStripButton_calendar_save.Click += new System.EventHandler(this.toolStripButton_calendar_save_Click);
            // 
            // toolStripButton_calendar_refresh
            // 
            this.toolStripButton_calendar_refresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_calendar_refresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_calendar_refresh.Image")));
            this.toolStripButton_calendar_refresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_calendar_refresh.Name = "toolStripButton_calendar_refresh";
            this.toolStripButton_calendar_refresh.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_calendar_refresh.Text = "刷新";
            this.toolStripButton_calendar_refresh.Click += new System.EventHandler(this.toolStripButton_calendar_refresh_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton3
            // 
            this.toolStripButton3.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton3.Enabled = false;
            this.toolStripButton3.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton3.Image")));
            this.toolStripButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton3.Name = "toolStripButton3";
            this.toolStripButton3.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton3.Text = "上移";
            // 
            // toolStripButton4
            // 
            this.toolStripButton4.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton4.Enabled = false;
            this.toolStripButton4.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton4.Image")));
            this.toolStripButton4.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton4.Name = "toolStripButton4";
            this.toolStripButton4.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton4.Text = "下移";
            // 
            // toolStripSeparator8
            // 
            this.toolStripSeparator8.Name = "toolStripSeparator8";
            this.toolStripSeparator8.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton_calendar_new
            // 
            this.toolStripButton_calendar_new.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_calendar_new.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_calendar_new.Image")));
            this.toolStripButton_calendar_new.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_calendar_new.Name = "toolStripButton_calendar_new";
            this.toolStripButton_calendar_new.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_calendar_new.Text = "新增";
            this.toolStripButton_calendar_new.Click += new System.EventHandler(this.toolStripButton_calendar_new_Click);
            // 
            // toolStripButton_calendar_modify
            // 
            this.toolStripButton_calendar_modify.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_calendar_modify.Enabled = false;
            this.toolStripButton_calendar_modify.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_calendar_modify.Image")));
            this.toolStripButton_calendar_modify.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_calendar_modify.Name = "toolStripButton_calendar_modify";
            this.toolStripButton_calendar_modify.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_calendar_modify.Text = "修改";
            this.toolStripButton_calendar_modify.Click += new System.EventHandler(this.toolStripButton_calendar_modify_Click);
            // 
            // toolStripButton_calendar_delete
            // 
            this.toolStripButton_calendar_delete.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_calendar_delete.Enabled = false;
            this.toolStripButton_calendar_delete.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_calendar_delete.Image")));
            this.toolStripButton_calendar_delete.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_calendar_delete.Name = "toolStripButton_calendar_delete";
            this.toolStripButton_calendar_delete.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_calendar_delete.Text = "删除";
            this.toolStripButton_calendar_delete.Click += new System.EventHandler(this.toolStripButton_calendar_delete_Click);
            // 
            // tabPage_kernel
            // 
            this.tabPage_kernel.Controls.Add(this.kernelResTree1);
            this.tabPage_kernel.Location = new System.Drawing.Point(4, 28);
            this.tabPage_kernel.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_kernel.Name = "tabPage_kernel";
            this.tabPage_kernel.Size = new System.Drawing.Size(802, 397);
            this.tabPage_kernel.TabIndex = 12;
            this.tabPage_kernel.Text = "内核";
            this.tabPage_kernel.UseVisualStyleBackColor = true;
            // 
            // kernelResTree1
            // 
            this.kernelResTree1.CssFileName = null;
            this.kernelResTree1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.kernelResTree1.HideSelection = false;
            this.kernelResTree1.ImageIndex = 0;
            this.kernelResTree1.Lang = null;
            this.kernelResTree1.Location = new System.Drawing.Point(0, 0);
            this.kernelResTree1.Margin = new System.Windows.Forms.Padding(4);
            this.kernelResTree1.Name = "kernelResTree1";
            this.kernelResTree1.SelectedImageIndex = 0;
            this.kernelResTree1.Size = new System.Drawing.Size(802, 397);
            this.kernelResTree1.TabIndex = 0;
            this.kernelResTree1.GetChannel += new DigitalPlatform.LibraryClient.GetChannelEventHandler(this.kernelResTree1_GetChannel);
            this.kernelResTree1.ReturnChannel += new DigitalPlatform.LibraryClient.ReturnChannelEventHandler(this.kernelResTree1_ReturnChannel);
            // 
            // imageList_opacBrowseFormatType
            // 
            this.imageList_opacBrowseFormatType.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_opacBrowseFormatType.ImageStream")));
            this.imageList_opacBrowseFormatType.TransparentColor = System.Drawing.Color.Fuchsia;
            this.imageList_opacBrowseFormatType.Images.SetKeyName(0, "database.bmp");
            this.imageList_opacBrowseFormatType.Images.SetKeyName(1, "document.ico");
            this.imageList_opacBrowseFormatType.Images.SetKeyName(2, "error_entity.bmp");
            // 
            // imageList_opacDatabaseType
            // 
            this.imageList_opacDatabaseType.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_opacDatabaseType.ImageStream")));
            this.imageList_opacDatabaseType.TransparentColor = System.Drawing.Color.Fuchsia;
            this.imageList_opacDatabaseType.Images.SetKeyName(0, "database.bmp");
            this.imageList_opacDatabaseType.Images.SetKeyName(1, "v_database.bmp");
            this.imageList_opacDatabaseType.Images.SetKeyName(2, "error_entity.bmp");
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
            // imageList_arrangement
            // 
            this.imageList_arrangement.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_arrangement.ImageStream")));
            this.imageList_arrangement.TransparentColor = System.Drawing.Color.Fuchsia;
            this.imageList_arrangement.Images.SetKeyName(0, "group.ico");
            this.imageList_arrangement.Images.SetKeyName(1, "database.bmp");
            this.imageList_arrangement.Images.SetKeyName(2, "error_entity.bmp");
            // 
            // ManagerForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(810, 459);
            this.Controls.Add(this.tabControl_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ManagerForm";
            this.ShowInTaskbar = false;
            this.Text = "系统管理";
            this.Activated += new System.EventHandler(this.ManagerForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ManagerForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ManagerForm_FormClosed);
            this.Load += new System.EventHandler(this.ManagerForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_databases.ResumeLayout(false);
            this.tabPage_databases.PerformLayout();
            this.toolStrip_databases.ResumeLayout(false);
            this.toolStrip_databases.PerformLayout();
            this.tabPage_opacDatabases.ResumeLayout(false);
            this.splitContainer_opac.Panel1.ResumeLayout(false);
            this.splitContainer_opac.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_opac)).EndInit();
            this.splitContainer_opac.ResumeLayout(false);
            this.tableLayoutPanel_opac_up.ResumeLayout(false);
            this.tableLayoutPanel_opac_up.PerformLayout();
            this.toolStrip_opacDatabases.ResumeLayout(false);
            this.toolStrip_opacDatabases.PerformLayout();
            this.tableLayoutPanel_opac_down.ResumeLayout(false);
            this.tableLayoutPanel_opac_down.PerformLayout();
            this.toolStrip_opacBrowseFormats.ResumeLayout(false);
            this.toolStrip_opacBrowseFormats.PerformLayout();
            this.tabPage_dup.ResumeLayout(false);
            this.tabPage_dup.PerformLayout();
            this.toolStrip_dup_main.ResumeLayout(false);
            this.toolStrip_dup_main.PerformLayout();
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.panel_projects.ResumeLayout(false);
            this.tableLayoutPanel_dup_up.ResumeLayout(false);
            this.tableLayoutPanel_dup_up.PerformLayout();
            this.toolStrip_dup_project.ResumeLayout(false);
            this.toolStrip_dup_project.PerformLayout();
            this.tableLayoutPanel_dup_down.ResumeLayout(false);
            this.tableLayoutPanel_dup_down.PerformLayout();
            this.toolStrip_dup_default.ResumeLayout(false);
            this.toolStrip_dup_default.PerformLayout();
            this.tabPage_locations.ResumeLayout(false);
            this.tableLayoutPanel_locations.ResumeLayout(false);
            this.tableLayoutPanel_locations.PerformLayout();
            this.toolStrip_location.ResumeLayout(false);
            this.toolStrip_location.PerformLayout();
            this.tabPage_zhongcihaoDatabases.ResumeLayout(false);
            this.tabPage_zhongcihaoDatabases.PerformLayout();
            this.toolStrip_zhongcihao.ResumeLayout(false);
            this.toolStrip_zhongcihao.PerformLayout();
            this.tabPage_bookshelf.ResumeLayout(false);
            this.tabPage_bookshelf.PerformLayout();
            this.toolStrip_arrangement.ResumeLayout(false);
            this.toolStrip_arrangement.PerformLayout();
            this.tabPage_script.ResumeLayout(false);
            this.splitContainer_script.Panel1.ResumeLayout(false);
            this.splitContainer_script.Panel2.ResumeLayout(false);
            this.splitContainer_script.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_script)).EndInit();
            this.splitContainer_script.ResumeLayout(false);
            this.tableLayoutPanel_script.ResumeLayout(false);
            this.tableLayoutPanel_script.PerformLayout();
            this.toolStrip_script.ResumeLayout(false);
            this.toolStrip_script.PerformLayout();
            this.tabPage_barcodeValidation.ResumeLayout(false);
            this.tableLayoutPanel_barcodeValidation.ResumeLayout(false);
            this.tableLayoutPanel_barcodeValidation.PerformLayout();
            this.toolStrip_barcodeValidation.ResumeLayout(false);
            this.toolStrip_barcodeValidation.PerformLayout();
            this.tabPage_valueTable.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.toolStrip_valueTables.ResumeLayout(false);
            this.toolStrip_valueTables.PerformLayout();
            this.tabPage_center.ResumeLayout(false);
            this.tabPage_center.PerformLayout();
            this.toolStrip_center.ResumeLayout(false);
            this.toolStrip_center.PerformLayout();
            this.tabPage_newLoanPolicy.ResumeLayout(false);
            this.tabPage_newLoanPolicy.PerformLayout();
            this.tabControl_newLoanPolicy.ResumeLayout(false);
            this.tabPage_newLoanPolicy_rightsTable.ResumeLayout(false);
            this.tabPage_newLoanPolicy_xml.ResumeLayout(false);
            this.tabPage_newLoanPolicy_xml.PerformLayout();
            this.toolStrip_newLoanPolicy.ResumeLayout(false);
            this.toolStrip_newLoanPolicy.PerformLayout();
            this.tabPage_calendar.ResumeLayout(false);
            this.tabPage_calendar.PerformLayout();
            this.toolStrip_calendar.ResumeLayout(false);
            this.toolStrip_calendar.PerformLayout();
            this.tabPage_kernel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_databases;
        private DigitalPlatform.GUI.ListViewNF listView_databases;
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
        private DigitalPlatform.GUI.ListViewNF listView_opacDatabases;
        private System.Windows.Forms.ColumnHeader columnHeader_opac_name;
        private System.Windows.Forms.ColumnHeader columnHeader_opac_type;
        private System.Windows.Forms.ColumnHeader columnHeader_opac_comment;
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
        private System.Windows.Forms.TabPage tabPage_locations;
        private System.Windows.Forms.TabPage tabPage_zhongcihaoDatabases;
        private DigitalPlatform.GUI.ListViewNF listView_location_list;
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
        private System.Windows.Forms.ToolStripButton toolStripButton_zhongcihao_save;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButton_refreshDatabaseDef;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.TabPage tabPage_bookshelf;
        private System.Windows.Forms.ToolStrip toolStrip_arrangement;
        private System.Windows.Forms.ToolStripButton toolStripButton_arrangement_save;
        private System.Windows.Forms.ToolStripButton toolStripButton_arrangement_refresh;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_arrangement_insert_group;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_arrangement_insert_location;
        private System.Windows.Forms.ToolStripButton toolStripButton_arrangement_modify;
        private System.Windows.Forms.ToolStripButton toolStripButton_arrangement_remove;
        private System.Windows.Forms.TreeView treeView_arrangement;
        private System.Windows.Forms.ImageList imageList_arrangement;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.Panel panel_projects;
        private DigitalPlatform.GUI.ListViewNF listView_dup_defaults;
        private System.Windows.Forms.ColumnHeader columnHeader_databaseName;
        private System.Windows.Forms.ColumnHeader columnHeader_defaultProjectName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolStrip toolStrip_dup_project;
        private System.Windows.Forms.ToolStripButton toolStripButton_dup_project_modify;
        private System.Windows.Forms.ToolStripButton toolStripButton_dup_project_delete;
        private System.Windows.Forms.ToolStrip toolStrip_dup_default;
        private System.Windows.Forms.ToolStripButton toolStripButton_dup_default_modify;
        private System.Windows.Forms.ToolStripButton toolStripButton_dup_default_delete;
        private System.Windows.Forms.ToolStripButton toolStripButton_dup_project_new;
        private DigitalPlatform.GUI.ListViewNF listView_dup_projects;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ToolStrip toolStrip_dup_main;
        private System.Windows.Forms.ToolStripButton toolStripButton_dup_refresh;
        private System.Windows.Forms.ToolStripButton toolStripButton_dup_save;
        private System.Windows.Forms.ToolStripButton toolStripButton_dup_default_new;
        private System.Windows.Forms.ToolStripButton toolStripButton_arrangement_viewXml;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_locations;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_opac_up;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_opac_down;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_dup_up;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_dup_down;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_script;
        private System.Windows.Forms.ColumnHeader columnHeader_location_libraryCode;
        private System.Windows.Forms.TabPage tabPage_valueTable;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private NoHasSelTextBox textBox_valueTables;
        private System.Windows.Forms.ToolStrip toolStrip_valueTables;
        private System.Windows.Forms.ToolStripButton toolStripButton_valueTable_save;
        private System.Windows.Forms.ToolStripButton toolStripButton_valueTable_refresh;
        private System.Windows.Forms.ToolStripLabel toolStripLabel1;
        private System.Windows.Forms.TabPage tabPage_center;
        private ListViewNF listView_center;
        private System.Windows.Forms.ColumnHeader columnHeader_center_name;
        private System.Windows.Forms.ColumnHeader columnHeader_center_url;
        private System.Windows.Forms.ColumnHeader columnHeader_center_userName;
        private System.Windows.Forms.ToolStrip toolStrip_center;
        private System.Windows.Forms.ToolStripButton toolStripButton_center_delete;
        private System.Windows.Forms.ToolStripButton toolStripButton_center_refresh;
        private System.Windows.Forms.ToolStripButton toolStripButton_center_add;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripButton toolStripButton_center_modify;
        private System.Windows.Forms.ToolStripButton toolStripButton_center_save;
        private System.Windows.Forms.ColumnHeader columnHeader_center_refid;
        private System.Windows.Forms.TabPage tabPage_newLoanPolicy;
        private System.Windows.Forms.ToolStrip toolStrip_newLoanPolicy;
        private System.Windows.Forms.ToolStripButton toolStripButton_newLoanPolicy_save;
        private System.Windows.Forms.ToolStripButton toolStripButton_newLoanPolicy_refresh;
        private System.Windows.Forms.TabControl tabControl_newLoanPolicy;
        private System.Windows.Forms.TabPage tabPage_newLoanPolicy_rightsTable;
        private System.Windows.Forms.TabPage tabPage_newLoanPolicy_xml;
        private System.Windows.Forms.TextBox textBox_newLoanPolicy_xml;
        private System.Windows.Forms.Integration.ElementHost elementHost1;
        private DigitalPlatform.CirculationClient.LoanPolicyControlWrapper loanPolicyControlWrapper1;
        private System.Windows.Forms.TabPage tabPage_calendar;
        private ListViewNF listView_calendar;
        private System.Windows.Forms.ColumnHeader columnHeader_calendar_name;
        private System.Windows.Forms.ColumnHeader columnHeader_calendar_range;
        private System.Windows.Forms.ColumnHeader columnHeader_calendar_comment;
        private System.Windows.Forms.ToolStrip toolStrip_calendar;
        private System.Windows.Forms.ToolStripButton toolStripButton_calendar_save;
        private System.Windows.Forms.ToolStripButton toolStripButton_calendar_refresh;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
        private System.Windows.Forms.ToolStripButton toolStripButton3;
        private System.Windows.Forms.ToolStripButton toolStripButton4;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator8;
        private System.Windows.Forms.ToolStripButton toolStripButton_calendar_new;
        private System.Windows.Forms.ToolStripButton toolStripButton_calendar_modify;
        private System.Windows.Forms.ToolStripButton toolStripButton_calendar_delete;
        private System.Windows.Forms.ColumnHeader columnHeader_calendar_content;
        private System.Windows.Forms.ImageList imageList_itemType;
        private System.Windows.Forms.ColumnHeader columnHeader_location_itemBarcodeNullable;
        private System.Windows.Forms.TabPage tabPage_kernel;
        private DigitalPlatform.CirculationClient.KernelResTree kernelResTree1;
        private System.Windows.Forms.ColumnHeader columnHeader_opac_alias;
        private System.Windows.Forms.ColumnHeader columnHeader_location_canReturn;
        private System.Windows.Forms.TabPage tabPage_barcodeValidation;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_barcodeValidation;
        private NoHasSelTextBox textBox_barcodeValidation;
        private System.Windows.Forms.ToolStrip toolStrip_barcodeValidation;
        private System.Windows.Forms.ToolStripButton toolStripButton_barcodeValidation_save;
        private System.Windows.Forms.ToolStripButton toolStripButton_barcodeValidation_refresh;
        private System.Windows.Forms.ToolStripLabel toolStripLabel2;
        private System.Windows.Forms.ColumnHeader columnHeader_opac_visible;
        private System.Windows.Forms.ColumnHeader columnHeader_marcSyntax;
        private System.Windows.Forms.ColumnHeader columnHeader_bookOrSeries;
    }
}