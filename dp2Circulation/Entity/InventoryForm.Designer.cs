namespace dp2Circulation
{
    partial class InventoryForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InventoryForm));
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_start = new System.Windows.Forms.TabPage();
            this.comboBox_inventoryDbName = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.button_start_restoreCfgs = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.button_start_setLocations = new System.Windows.Forms.Button();
            this.inventoryBatchNoControl_start_batchNo = new dp2Circulation.InventoryBatchNoControl();
            this.textBox_start_locations = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabPage_scan = new System.Windows.Forms.TabPage();
            this.tabPage_inventoryList = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel_inventoryList = new System.Windows.Forms.TableLayoutPanel();
            this.listView_inventoryList_records = new DigitalPlatform.GUI.ListViewQU();
            this.columnHeader_path = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel_list_searchPanel = new System.Windows.Forms.Panel();
            this.button_inventoryList_getBatchNos = new System.Windows.Forms.Button();
            this.button_inventoryList_search = new System.Windows.Forms.Button();
            this.textBox_inventoryList_batchNo = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabPage_baseList = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.listView_baseList_records = new DigitalPlatform.GUI.ListViewQU();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.panel1 = new System.Windows.Forms.Panel();
            this.button_baseList_getLocations = new System.Windows.Forms.Button();
            this.button_baseList_search = new System.Windows.Forms.Button();
            this.textBox_baseList_locations = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tabPage_operLog = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel_operLog = new System.Windows.Forms.TableLayoutPanel();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            this.panel2 = new System.Windows.Forms.Panel();
            this.button_operLog_load = new System.Windows.Forms.Button();
            this.button_operLog_setDateRange = new System.Windows.Forms.Button();
            this.textBox_operLog_dateRange = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tabPage_statis = new System.Windows.Forms.TabPage();
            this.button_statis_return = new System.Windows.Forms.Button();
            this.button_statis_maskItems = new System.Windows.Forms.Button();
            this.button_statis_defOutputColumns = new System.Windows.Forms.Button();
            this.button_statis_outputExcel = new System.Windows.Forms.Button();
            this.button_statis_crossCompute = new System.Windows.Forms.Button();
            this.timer_qu = new System.Windows.Forms.Timer(this.components);
            this.tabControl_main.SuspendLayout();
            this.tabPage_start.SuspendLayout();
            this.tabPage_inventoryList.SuspendLayout();
            this.tableLayoutPanel_inventoryList.SuspendLayout();
            this.panel_list_searchPanel.SuspendLayout();
            this.tabPage_baseList.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabPage_operLog.SuspendLayout();
            this.tableLayoutPanel_operLog.SuspendLayout();
            this.panel2.SuspendLayout();
            this.tabPage_statis.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_main
            // 
            this.tabControl_main.Controls.Add(this.tabPage_start);
            this.tabControl_main.Controls.Add(this.tabPage_scan);
            this.tabControl_main.Controls.Add(this.tabPage_inventoryList);
            this.tabControl_main.Controls.Add(this.tabPage_baseList);
            this.tabControl_main.Controls.Add(this.tabPage_operLog);
            this.tabControl_main.Controls.Add(this.tabPage_statis);
            this.tabControl_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_main.Location = new System.Drawing.Point(0, 0);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(806, 513);
            this.tabControl_main.TabIndex = 0;
            // 
            // tabPage_start
            // 
            this.tabPage_start.AutoScroll = true;
            this.tabPage_start.Controls.Add(this.comboBox_inventoryDbName);
            this.tabPage_start.Controls.Add(this.label7);
            this.tabPage_start.Controls.Add(this.button_start_restoreCfgs);
            this.tabPage_start.Controls.Add(this.label6);
            this.tabPage_start.Controls.Add(this.button_start_setLocations);
            this.tabPage_start.Controls.Add(this.inventoryBatchNoControl_start_batchNo);
            this.tabPage_start.Controls.Add(this.textBox_start_locations);
            this.tabPage_start.Controls.Add(this.label5);
            this.tabPage_start.Controls.Add(this.label1);
            this.tabPage_start.Location = new System.Drawing.Point(4, 31);
            this.tabPage_start.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabPage_start.Name = "tabPage_start";
            this.tabPage_start.Padding = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabPage_start.Size = new System.Drawing.Size(798, 478);
            this.tabPage_start.TabIndex = 0;
            this.tabPage_start.Text = "开始";
            this.tabPage_start.UseVisualStyleBackColor = true;
            this.tabPage_start.Leave += new System.EventHandler(this.tabPage_start_Leave);
            // 
            // comboBox_inventoryDbName
            // 
            this.comboBox_inventoryDbName.FormattingEnabled = true;
            this.comboBox_inventoryDbName.Location = new System.Drawing.Point(16, 43);
            this.comboBox_inventoryDbName.Name = "comboBox_inventoryDbName";
            this.comboBox_inventoryDbName.Size = new System.Drawing.Size(260, 29);
            this.comboBox_inventoryDbName.TabIndex = 1;
            this.comboBox_inventoryDbName.TextChanged += new System.EventHandler(this.comboBox_inventoryDbName_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 19);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(138, 21);
            this.label7.TabIndex = 0;
            this.label7.Text = "盘点库名(&N):";
            // 
            // button_start_restoreCfgs
            // 
            this.button_start_restoreCfgs.Location = new System.Drawing.Point(16, 424);
            this.button_start_restoreCfgs.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_start_restoreCfgs.Name = "button_start_restoreCfgs";
            this.button_start_restoreCfgs.Size = new System.Drawing.Size(359, 40);
            this.button_start_restoreCfgs.TabIndex = 8;
            this.button_start_restoreCfgs.Text = "恢复配置文件出厂设置";
            this.button_start_restoreCfgs.UseVisualStyleBackColor = true;
            this.button_start_restoreCfgs.Click += new System.EventHandler(this.button_start_restoreCfgs_Click);
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(16, 330);
            this.label6.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(544, 88);
            this.label6.TabIndex = 7;
            this.label6.Text = "注：在扫入册条码号盘点过程中，软件会根据上述馆藏地对册记录进行检查，如果发现册记录馆藏地超出上述范围，会用黄色状态进行提醒。";
            // 
            // button_start_setLocations
            // 
            this.button_start_setLocations.Location = new System.Drawing.Point(572, 223);
            this.button_start_setLocations.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_start_setLocations.Name = "button_start_setLocations";
            this.button_start_setLocations.Size = new System.Drawing.Size(82, 40);
            this.button_start_setLocations.TabIndex = 6;
            this.button_start_setLocations.Text = "...";
            this.button_start_setLocations.UseVisualStyleBackColor = true;
            this.button_start_setLocations.Click += new System.EventHandler(this.button_start_setLocations_Click);
            // 
            // inventoryBatchNoControl_start_batchNo
            // 
            this.inventoryBatchNoControl_start_batchNo.AutoSize = true;
            this.inventoryBatchNoControl_start_batchNo.LibaryCodeEanbled = true;
            this.inventoryBatchNoControl_start_batchNo.LibraryCodeList = ((System.Collections.Generic.List<string>)(resources.GetObject("inventoryBatchNoControl_start_batchNo.LibraryCodeList")));
            this.inventoryBatchNoControl_start_batchNo.LibraryCodeText = "";
            this.inventoryBatchNoControl_start_batchNo.Location = new System.Drawing.Point(16, 115);
            this.inventoryBatchNoControl_start_batchNo.Margin = new System.Windows.Forms.Padding(11, 10, 11, 10);
            this.inventoryBatchNoControl_start_batchNo.Name = "inventoryBatchNoControl_start_batchNo";
            this.inventoryBatchNoControl_start_batchNo.Size = new System.Drawing.Size(638, 66);
            this.inventoryBatchNoControl_start_batchNo.TabIndex = 3;
            this.inventoryBatchNoControl_start_batchNo.TextChanged += new System.EventHandler(this.inventoryBatchNoControl_start_batchNo_TextChanged);
            // 
            // textBox_start_locations
            // 
            this.textBox_start_locations.AcceptsReturn = true;
            this.textBox_start_locations.Location = new System.Drawing.Point(16, 223);
            this.textBox_start_locations.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_start_locations.Multiline = true;
            this.textBox_start_locations.Name = "textBox_start_locations";
            this.textBox_start_locations.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_start_locations.Size = new System.Drawing.Size(541, 92);
            this.textBox_start_locations.TabIndex = 5;
            this.textBox_start_locations.TextChanged += new System.EventHandler(this.textBox_start_locations_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 197);
            this.label5.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(243, 21);
            this.label5.TabIndex = 4;
            this.label5.Text = "扫入时所在的馆藏地(&L):";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 93);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(243, 21);
            this.label1.TabIndex = 2;
            this.label1.Text = "扫入时所用的批次号(&B):";
            // 
            // tabPage_scan
            // 
            this.tabPage_scan.Location = new System.Drawing.Point(4, 31);
            this.tabPage_scan.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabPage_scan.Name = "tabPage_scan";
            this.tabPage_scan.Padding = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabPage_scan.Size = new System.Drawing.Size(798, 478);
            this.tabPage_scan.TabIndex = 1;
            this.tabPage_scan.Text = "扫入";
            this.tabPage_scan.UseVisualStyleBackColor = true;
            // 
            // tabPage_inventoryList
            // 
            this.tabPage_inventoryList.Controls.Add(this.tableLayoutPanel_inventoryList);
            this.tabPage_inventoryList.Location = new System.Drawing.Point(4, 31);
            this.tabPage_inventoryList.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabPage_inventoryList.Name = "tabPage_inventoryList";
            this.tabPage_inventoryList.Size = new System.Drawing.Size(798, 478);
            this.tabPage_inventoryList.TabIndex = 2;
            this.tabPage_inventoryList.Text = "盘点集";
            this.tabPage_inventoryList.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel_inventoryList
            // 
            this.tableLayoutPanel_inventoryList.ColumnCount = 1;
            this.tableLayoutPanel_inventoryList.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_inventoryList.Controls.Add(this.listView_inventoryList_records, 0, 1);
            this.tableLayoutPanel_inventoryList.Controls.Add(this.panel_list_searchPanel, 0, 0);
            this.tableLayoutPanel_inventoryList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_inventoryList.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_inventoryList.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tableLayoutPanel_inventoryList.Name = "tableLayoutPanel_inventoryList";
            this.tableLayoutPanel_inventoryList.RowCount = 2;
            this.tableLayoutPanel_inventoryList.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_inventoryList.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_inventoryList.Size = new System.Drawing.Size(798, 478);
            this.tableLayoutPanel_inventoryList.TabIndex = 12;
            // 
            // listView_inventoryList_records
            // 
            this.listView_inventoryList_records.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_path,
            this.columnHeader_1});
            this.listView_inventoryList_records.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_inventoryList_records.FullRowSelect = true;
            this.listView_inventoryList_records.HideSelection = false;
            this.listView_inventoryList_records.Location = new System.Drawing.Point(0, 128);
            this.listView_inventoryList_records.Margin = new System.Windows.Forms.Padding(0);
            this.listView_inventoryList_records.Name = "listView_inventoryList_records";
            this.listView_inventoryList_records.Size = new System.Drawing.Size(798, 350);
            this.listView_inventoryList_records.TabIndex = 11;
            this.listView_inventoryList_records.UseCompatibleStateImageBehavior = false;
            this.listView_inventoryList_records.View = System.Windows.Forms.View.Details;
            this.listView_inventoryList_records.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_inventoryList_ColumnClick);
            this.listView_inventoryList_records.SelectedIndexChanged += new System.EventHandler(this.listView_inventoryList_SelectedIndexChanged);
            this.listView_inventoryList_records.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_inventoryList_records_MouseUp);
            // 
            // columnHeader_path
            // 
            this.columnHeader_path.Text = "路径";
            this.columnHeader_path.Width = 150;
            // 
            // columnHeader_1
            // 
            this.columnHeader_1.Text = "1";
            this.columnHeader_1.Width = 200;
            // 
            // panel_list_searchPanel
            // 
            this.panel_list_searchPanel.Controls.Add(this.button_inventoryList_getBatchNos);
            this.panel_list_searchPanel.Controls.Add(this.button_inventoryList_search);
            this.panel_list_searchPanel.Controls.Add(this.textBox_inventoryList_batchNo);
            this.panel_list_searchPanel.Controls.Add(this.label2);
            this.panel_list_searchPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_list_searchPanel.Location = new System.Drawing.Point(0, 0);
            this.panel_list_searchPanel.Margin = new System.Windows.Forms.Padding(0);
            this.panel_list_searchPanel.Name = "panel_list_searchPanel";
            this.panel_list_searchPanel.Size = new System.Drawing.Size(798, 128);
            this.panel_list_searchPanel.TabIndex = 12;
            // 
            // button_inventoryList_getBatchNos
            // 
            this.button_inventoryList_getBatchNos.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_inventoryList_getBatchNos.Location = new System.Drawing.Point(556, 33);
            this.button_inventoryList_getBatchNos.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_inventoryList_getBatchNos.Name = "button_inventoryList_getBatchNos";
            this.button_inventoryList_getBatchNos.Size = new System.Drawing.Size(81, 40);
            this.button_inventoryList_getBatchNos.TabIndex = 3;
            this.button_inventoryList_getBatchNos.Text = "...";
            this.button_inventoryList_getBatchNos.UseVisualStyleBackColor = true;
            this.button_inventoryList_getBatchNos.Click += new System.EventHandler(this.button_list_getBatchNos_Click);
            // 
            // button_inventoryList_search
            // 
            this.button_inventoryList_search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_inventoryList_search.Location = new System.Drawing.Point(660, 33);
            this.button_inventoryList_search.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_inventoryList_search.Name = "button_inventoryList_search";
            this.button_inventoryList_search.Size = new System.Drawing.Size(138, 89);
            this.button_inventoryList_search.TabIndex = 2;
            this.button_inventoryList_search.Text = "检索";
            this.button_inventoryList_search.UseVisualStyleBackColor = true;
            this.button_inventoryList_search.Click += new System.EventHandler(this.button_list_search_Click);
            // 
            // textBox_inventoryList_batchNo
            // 
            this.textBox_inventoryList_batchNo.AcceptsReturn = true;
            this.textBox_inventoryList_batchNo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_inventoryList_batchNo.Location = new System.Drawing.Point(0, 33);
            this.textBox_inventoryList_batchNo.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_inventoryList_batchNo.Multiline = true;
            this.textBox_inventoryList_batchNo.Name = "textBox_inventoryList_batchNo";
            this.textBox_inventoryList_batchNo.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_inventoryList_batchNo.Size = new System.Drawing.Size(550, 92);
            this.textBox_inventoryList_batchNo.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(-4, 7);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 21);
            this.label2.TabIndex = 0;
            this.label2.Text = "批次号:";
            // 
            // tabPage_baseList
            // 
            this.tabPage_baseList.Controls.Add(this.tableLayoutPanel1);
            this.tabPage_baseList.Location = new System.Drawing.Point(4, 31);
            this.tabPage_baseList.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabPage_baseList.Name = "tabPage_baseList";
            this.tabPage_baseList.Size = new System.Drawing.Size(798, 478);
            this.tabPage_baseList.TabIndex = 3;
            this.tabPage_baseList.Text = "基准集";
            this.tabPage_baseList.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.listView_baseList_records, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(798, 478);
            this.tableLayoutPanel1.TabIndex = 13;
            // 
            // listView_baseList_records
            // 
            this.listView_baseList_records.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.listView_baseList_records.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_baseList_records.FullRowSelect = true;
            this.listView_baseList_records.HideSelection = false;
            this.listView_baseList_records.Location = new System.Drawing.Point(0, 128);
            this.listView_baseList_records.Margin = new System.Windows.Forms.Padding(0);
            this.listView_baseList_records.Name = "listView_baseList_records";
            this.listView_baseList_records.Size = new System.Drawing.Size(798, 350);
            this.listView_baseList_records.TabIndex = 11;
            this.listView_baseList_records.UseCompatibleStateImageBehavior = false;
            this.listView_baseList_records.View = System.Windows.Forms.View.Details;
            this.listView_baseList_records.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_baseList_records_ColumnClick);
            this.listView_baseList_records.SelectedIndexChanged += new System.EventHandler(this.listView_baseList_records_SelectedIndexChanged);
            this.listView_baseList_records.DoubleClick += new System.EventHandler(this.listView_baseList_records_DoubleClick);
            this.listView_baseList_records.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_baseList_records_MouseUp);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "路径";
            this.columnHeader1.Width = 150;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "1";
            this.columnHeader2.Width = 200;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button_baseList_getLocations);
            this.panel1.Controls.Add(this.button_baseList_search);
            this.panel1.Controls.Add(this.textBox_baseList_locations);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(798, 128);
            this.panel1.TabIndex = 12;
            // 
            // button_baseList_getLocations
            // 
            this.button_baseList_getLocations.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_baseList_getLocations.Location = new System.Drawing.Point(558, 33);
            this.button_baseList_getLocations.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_baseList_getLocations.Name = "button_baseList_getLocations";
            this.button_baseList_getLocations.Size = new System.Drawing.Size(81, 40);
            this.button_baseList_getLocations.TabIndex = 3;
            this.button_baseList_getLocations.Text = "...";
            this.button_baseList_getLocations.UseVisualStyleBackColor = true;
            this.button_baseList_getLocations.Click += new System.EventHandler(this.button_baseList_getLocations_Click);
            // 
            // button_baseList_search
            // 
            this.button_baseList_search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_baseList_search.Location = new System.Drawing.Point(660, 33);
            this.button_baseList_search.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_baseList_search.Name = "button_baseList_search";
            this.button_baseList_search.Size = new System.Drawing.Size(138, 89);
            this.button_baseList_search.TabIndex = 2;
            this.button_baseList_search.Text = "检索";
            this.button_baseList_search.UseVisualStyleBackColor = true;
            this.button_baseList_search.Click += new System.EventHandler(this.button_baseList_search_Click);
            // 
            // textBox_baseList_locations
            // 
            this.textBox_baseList_locations.AcceptsReturn = true;
            this.textBox_baseList_locations.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_baseList_locations.Location = new System.Drawing.Point(0, 33);
            this.textBox_baseList_locations.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_baseList_locations.Multiline = true;
            this.textBox_baseList_locations.Name = "textBox_baseList_locations";
            this.textBox_baseList_locations.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_baseList_locations.Size = new System.Drawing.Size(550, 92);
            this.textBox_baseList_locations.TabIndex = 1;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(-4, 7);
            this.label3.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(84, 21);
            this.label3.TabIndex = 0;
            this.label3.Text = "馆藏地:";
            // 
            // tabPage_operLog
            // 
            this.tabPage_operLog.Controls.Add(this.tableLayoutPanel_operLog);
            this.tabPage_operLog.Location = new System.Drawing.Point(4, 31);
            this.tabPage_operLog.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabPage_operLog.Name = "tabPage_operLog";
            this.tabPage_operLog.Size = new System.Drawing.Size(798, 478);
            this.tabPage_operLog.TabIndex = 5;
            this.tabPage_operLog.Text = "借还日志";
            this.tabPage_operLog.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel_operLog
            // 
            this.tableLayoutPanel_operLog.ColumnCount = 1;
            this.tableLayoutPanel_operLog.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_operLog.Controls.Add(this.webBrowser1, 0, 1);
            this.tableLayoutPanel_operLog.Controls.Add(this.panel2, 0, 0);
            this.tableLayoutPanel_operLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_operLog.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_operLog.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tableLayoutPanel_operLog.Name = "tableLayoutPanel_operLog";
            this.tableLayoutPanel_operLog.RowCount = 2;
            this.tableLayoutPanel_operLog.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_operLog.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_operLog.Size = new System.Drawing.Size(798, 478);
            this.tableLayoutPanel_operLog.TabIndex = 0;
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(6, 53);
            this.webBrowser1.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(37, 35);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(786, 420);
            this.webBrowser1.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.button_operLog_load);
            this.panel2.Controls.Add(this.button_operLog_setDateRange);
            this.panel2.Controls.Add(this.textBox_operLog_dateRange);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(6, 5);
            this.panel2.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(786, 38);
            this.panel2.TabIndex = 1;
            // 
            // button_operLog_load
            // 
            this.button_operLog_load.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_operLog_load.Location = new System.Drawing.Point(654, 0);
            this.button_operLog_load.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_operLog_load.Name = "button_operLog_load";
            this.button_operLog_load.Size = new System.Drawing.Size(123, 37);
            this.button_operLog_load.TabIndex = 3;
            this.button_operLog_load.Text = "装载";
            this.button_operLog_load.UseVisualStyleBackColor = true;
            this.button_operLog_load.Click += new System.EventHandler(this.button_operLog_load_Click);
            // 
            // button_operLog_setDateRange
            // 
            this.button_operLog_setDateRange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_operLog_setDateRange.Location = new System.Drawing.Point(577, 0);
            this.button_operLog_setDateRange.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_operLog_setDateRange.Name = "button_operLog_setDateRange";
            this.button_operLog_setDateRange.Size = new System.Drawing.Size(66, 37);
            this.button_operLog_setDateRange.TabIndex = 2;
            this.button_operLog_setDateRange.Text = "...";
            this.button_operLog_setDateRange.UseVisualStyleBackColor = true;
            this.button_operLog_setDateRange.Click += new System.EventHandler(this.button_operLog_setDateRange_Click);
            // 
            // textBox_operLog_dateRange
            // 
            this.textBox_operLog_dateRange.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_operLog_dateRange.Location = new System.Drawing.Point(165, 0);
            this.textBox_operLog_dateRange.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_operLog_dateRange.Name = "textBox_operLog_dateRange";
            this.textBox_operLog_dateRange.Size = new System.Drawing.Size(407, 31);
            this.textBox_operLog_dateRange.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(0, 7);
            this.label4.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(147, 21);
            this.label4.TabIndex = 0;
            this.label4.Text = "日志起止范围:";
            // 
            // tabPage_statis
            // 
            this.tabPage_statis.Controls.Add(this.button_statis_return);
            this.tabPage_statis.Controls.Add(this.button_statis_maskItems);
            this.tabPage_statis.Controls.Add(this.button_statis_defOutputColumns);
            this.tabPage_statis.Controls.Add(this.button_statis_outputExcel);
            this.tabPage_statis.Controls.Add(this.button_statis_crossCompute);
            this.tabPage_statis.Location = new System.Drawing.Point(4, 31);
            this.tabPage_statis.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.tabPage_statis.Name = "tabPage_statis";
            this.tabPage_statis.Size = new System.Drawing.Size(798, 478);
            this.tabPage_statis.TabIndex = 4;
            this.tabPage_statis.Text = "统计";
            this.tabPage_statis.UseVisualStyleBackColor = true;
            // 
            // button_statis_return
            // 
            this.button_statis_return.Location = new System.Drawing.Point(16, 187);
            this.button_statis_return.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_statis_return.Name = "button_statis_return";
            this.button_statis_return.Size = new System.Drawing.Size(425, 40);
            this.button_statis_return.TabIndex = 4;
            this.button_statis_return.Text = "补做还书";
            this.button_statis_return.UseVisualStyleBackColor = true;
            this.button_statis_return.Click += new System.EventHandler(this.button_statis_return_Click);
            // 
            // button_statis_maskItems
            // 
            this.button_statis_maskItems.Location = new System.Drawing.Point(16, 110);
            this.button_statis_maskItems.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_statis_maskItems.Name = "button_statis_maskItems";
            this.button_statis_maskItems.Size = new System.Drawing.Size(425, 40);
            this.button_statis_maskItems.TabIndex = 3;
            this.button_statis_maskItems.Text = "修改册状态";
            this.button_statis_maskItems.UseVisualStyleBackColor = true;
            this.button_statis_maskItems.Click += new System.EventHandler(this.button_statis_maskItems_Click);
            // 
            // button_statis_defOutputColumns
            // 
            this.button_statis_defOutputColumns.Location = new System.Drawing.Point(16, 322);
            this.button_statis_defOutputColumns.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_statis_defOutputColumns.Name = "button_statis_defOutputColumns";
            this.button_statis_defOutputColumns.Size = new System.Drawing.Size(425, 40);
            this.button_statis_defOutputColumns.TabIndex = 2;
            this.button_statis_defOutputColumns.Text = "配置 Excel 报表栏目 ...";
            this.button_statis_defOutputColumns.UseVisualStyleBackColor = true;
            this.button_statis_defOutputColumns.Click += new System.EventHandler(this.button_statis_defOutputColumns_Click);
            // 
            // button_statis_outputExcel
            // 
            this.button_statis_outputExcel.Location = new System.Drawing.Point(16, 271);
            this.button_statis_outputExcel.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_statis_outputExcel.Name = "button_statis_outputExcel";
            this.button_statis_outputExcel.Size = new System.Drawing.Size(425, 40);
            this.button_statis_outputExcel.TabIndex = 1;
            this.button_statis_outputExcel.Text = "创建 Excel 报表 ...";
            this.button_statis_outputExcel.UseVisualStyleBackColor = true;
            this.button_statis_outputExcel.Click += new System.EventHandler(this.button_statis_outputExcel_Click);
            // 
            // button_statis_crossCompute
            // 
            this.button_statis_crossCompute.Location = new System.Drawing.Point(16, 32);
            this.button_statis_crossCompute.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_statis_crossCompute.Name = "button_statis_crossCompute";
            this.button_statis_crossCompute.Size = new System.Drawing.Size(425, 40);
            this.button_statis_crossCompute.TabIndex = 0;
            this.button_statis_crossCompute.Text = "交叉运算";
            this.button_statis_crossCompute.UseVisualStyleBackColor = true;
            this.button_statis_crossCompute.Click += new System.EventHandler(this.button_statis_crossCompute_Click);
            // 
            // timer_qu
            // 
            this.timer_qu.Interval = 1000;
            this.timer_qu.Tick += new System.EventHandler(this.timer_qu_Tick);
            // 
            // InventoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(806, 513);
            this.Controls.Add(this.tabControl_main);
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.Name = "InventoryForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "盘点";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.InventoryForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.InventoryForm_FormClosed);
            this.Load += new System.EventHandler(this.InventoryForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_start.ResumeLayout(false);
            this.tabPage_start.PerformLayout();
            this.tabPage_inventoryList.ResumeLayout(false);
            this.tableLayoutPanel_inventoryList.ResumeLayout(false);
            this.panel_list_searchPanel.ResumeLayout(false);
            this.panel_list_searchPanel.PerformLayout();
            this.tabPage_baseList.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabPage_operLog.ResumeLayout(false);
            this.tableLayoutPanel_operLog.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.tabPage_statis.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_start;
        private System.Windows.Forms.TabPage tabPage_scan;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TabPage tabPage_inventoryList;
        private DigitalPlatform.GUI.ListViewQU listView_inventoryList_records;
        private System.Windows.Forms.ColumnHeader columnHeader_path;
        private System.Windows.Forms.ColumnHeader columnHeader_1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_inventoryList;
        private System.Windows.Forms.Panel panel_list_searchPanel;
        private System.Windows.Forms.Button button_inventoryList_getBatchNos;
        private System.Windows.Forms.Button button_inventoryList_search;
        private System.Windows.Forms.TextBox textBox_inventoryList_batchNo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabPage tabPage_baseList;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private DigitalPlatform.GUI.ListViewQU listView_baseList_records;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button button_baseList_getLocations;
        private System.Windows.Forms.Button button_baseList_search;
        private System.Windows.Forms.TextBox textBox_baseList_locations;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabPage tabPage_statis;
        private System.Windows.Forms.Button button_statis_crossCompute;
        private System.Windows.Forms.Timer timer_qu;
        private System.Windows.Forms.Button button_statis_outputExcel;
        private System.Windows.Forms.Button button_statis_defOutputColumns;
        private System.Windows.Forms.TabPage tabPage_operLog;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_operLog;
        private System.Windows.Forms.WebBrowser webBrowser1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button button_operLog_setDateRange;
        private System.Windows.Forms.TextBox textBox_operLog_dateRange;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_operLog_load;
        private System.Windows.Forms.Button button_statis_maskItems;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_start_locations;
        private InventoryBatchNoControl inventoryBatchNoControl_start_batchNo;
        private System.Windows.Forms.Button button_start_setLocations;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button button_statis_return;
        private System.Windows.Forms.Button button_start_restoreCfgs;
        private System.Windows.Forms.ComboBox comboBox_inventoryDbName;
        private System.Windows.Forms.Label label7;
    }
}