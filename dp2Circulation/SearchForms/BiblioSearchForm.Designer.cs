namespace dp2Circulation
{
    partial class BiblioSearchForm
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
            this.DisposeFreeControls();

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BiblioSearchForm));
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_queryWord = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label_biblioDbName = new System.Windows.Forms.Label();
            this.contextMenuStrip_biblioDb = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.MenuItem_viewBiblioDbProperty = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel_query = new System.Windows.Forms.TableLayoutPanel();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBox_from = new DigitalPlatform.CommonControl.TabComboBox();
            this.label_matchStyle = new System.Windows.Forms.Label();
            this.comboBox_matchStyle = new System.Windows.Forms.ComboBox();
            this.checkedComboBox_biblioDbNames = new DigitalPlatform.CommonControl.CheckedComboBox();
            this.toolStrip_search = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_search = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripDropDownButton_searchKeys = new System.Windows.Forms.ToolStripDropDownButton();
            this.ToolStripMenuItem_continueLoad = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_searchKeyID = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_searchKeys = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItem_searchShareBiblio = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_searchZ3950 = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_filterRecords = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_subrecords = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItem_z3950ServerList = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripDropDownButton_inputTimeString = new System.Windows.Forms.ToolStripDropDownButton();
            this.ToolStripMenuItem_rfc1123Single = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_uSingle = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItem_rfc1123Range = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_uRange = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_prevQuery = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_nextQuery = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_multiLine = new System.Windows.Forms.ToolStripButton();
            this.button_search = new System.Windows.Forms.Button();
            this.comboBox_location = new System.Windows.Forms.ComboBox();
            this.tableLayoutPanel_main = new System.Windows.Forms.TableLayoutPanel();
            this.label_message = new System.Windows.Forms.Label();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.tabControl_query = new System.Windows.Forms.TabControl();
            this.tabPage_simple = new System.Windows.Forms.TabPage();
            this.tabPage_logic = new System.Windows.Forms.TabPage();
            this.dp2QueryControl1 = new DigitalPlatform.CommonControl.dp2QueryControl();
            this.listView_records = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_path = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStripMenuItem_findInList = new System.Windows.Forms.ToolStripMenuItem();
            this.contextMenuStrip_biblioDb.SuspendLayout();
            this.tableLayoutPanel_query.SuspendLayout();
            this.toolStrip_search.SuspendLayout();
            this.tableLayoutPanel_main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.tabControl_query.SuspendLayout();
            this.tabPage_simple.SuspendLayout();
            this.tabPage_logic.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label2.Location = new System.Drawing.Point(7, 85);
            this.label2.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(138, 46);
            this.label2.TabIndex = 5;
            this.label2.Text = "检索途径(&F):";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // textBox_queryWord
            // 
            this.textBox_queryWord.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_queryWord.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_queryWord.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.textBox_queryWord.Location = new System.Drawing.Point(159, 7);
            this.textBox_queryWord.Margin = new System.Windows.Forms.Padding(7);
            this.textBox_queryWord.MaxLength = 0;
            this.textBox_queryWord.Name = "textBox_queryWord";
            this.textBox_queryWord.Size = new System.Drawing.Size(253, 31);
            this.textBox_queryWord.TabIndex = 1;
            this.textBox_queryWord.TextChanged += new System.EventHandler(this.textBox_queryWord_TextChanged);
            this.textBox_queryWord.Enter += new System.EventHandler(this.textBox_queryWord_Enter);
            this.textBox_queryWord.Leave += new System.EventHandler(this.textBox_queryWord_Leave);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label1.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label1.Location = new System.Drawing.Point(7, 0);
            this.label1.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(138, 45);
            this.label1.TabIndex = 0;
            this.label1.Text = "检索词(&W):";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label_biblioDbName
            // 
            this.label_biblioDbName.AutoSize = true;
            this.label_biblioDbName.ContextMenuStrip = this.contextMenuStrip_biblioDb;
            this.label_biblioDbName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_biblioDbName.Location = new System.Drawing.Point(5, 45);
            this.label_biblioDbName.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label_biblioDbName.Name = "label_biblioDbName";
            this.label_biblioDbName.Size = new System.Drawing.Size(142, 40);
            this.label_biblioDbName.TabIndex = 3;
            this.label_biblioDbName.Text = "书目库(&D):";
            this.label_biblioDbName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // contextMenuStrip_biblioDb
            // 
            this.contextMenuStrip_biblioDb.ImageScalingSize = new System.Drawing.Size(32, 32);
            this.contextMenuStrip_biblioDb.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuItem_viewBiblioDbProperty});
            this.contextMenuStrip_biblioDb.Name = "contextMenuStrip_biblioDb";
            this.contextMenuStrip_biblioDb.Size = new System.Drawing.Size(274, 38);
            // 
            // MenuItem_viewBiblioDbProperty
            // 
            this.MenuItem_viewBiblioDbProperty.Name = "MenuItem_viewBiblioDbProperty";
            this.MenuItem_viewBiblioDbProperty.Size = new System.Drawing.Size(273, 34);
            this.MenuItem_viewBiblioDbProperty.Text = "观察书目库属性(&P)...";
            this.MenuItem_viewBiblioDbProperty.Click += new System.EventHandler(this.MenuItem_viewBiblioDbProperty_Click);
            // 
            // tableLayoutPanel_query
            // 
            this.tableLayoutPanel_query.AutoSize = true;
            this.tableLayoutPanel_query.ColumnCount = 3;
            this.tableLayoutPanel_query.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_query.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_query.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_query.Controls.Add(this.label4, 0, 4);
            this.tableLayoutPanel_query.Controls.Add(this.label2, 0, 2);
            this.tableLayoutPanel_query.Controls.Add(this.textBox_queryWord, 1, 0);
            this.tableLayoutPanel_query.Controls.Add(this.label_biblioDbName, 0, 1);
            this.tableLayoutPanel_query.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel_query.Controls.Add(this.comboBox_from, 1, 2);
            this.tableLayoutPanel_query.Controls.Add(this.label_matchStyle, 0, 3);
            this.tableLayoutPanel_query.Controls.Add(this.comboBox_matchStyle, 1, 3);
            this.tableLayoutPanel_query.Controls.Add(this.checkedComboBox_biblioDbNames, 1, 1);
            this.tableLayoutPanel_query.Controls.Add(this.toolStrip_search, 2, 0);
            this.tableLayoutPanel_query.Controls.Add(this.button_search, 2, 1);
            this.tableLayoutPanel_query.Controls.Add(this.comboBox_location, 1, 4);
            this.tableLayoutPanel_query.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_query.Location = new System.Drawing.Point(5, 5);
            this.tableLayoutPanel_query.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_query.MaximumSize = new System.Drawing.Size(733, 0);
            this.tableLayoutPanel_query.Name = "tableLayoutPanel_query";
            this.tableLayoutPanel_query.RowCount = 6;
            this.tableLayoutPanel_query.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_query.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_query.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_query.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_query.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_query.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_query.Size = new System.Drawing.Size(733, 245);
            this.tableLayoutPanel_query.TabIndex = 8;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label4.Location = new System.Drawing.Point(7, 174);
            this.label4.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(138, 43);
            this.label4.TabIndex = 15;
            this.label4.Text = "馆藏地(&L):";
            this.label4.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboBox_from
            // 
            this.comboBox_from.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBox_from.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBox_from.DropDownHeight = 350;
            this.comboBox_from.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_from.FormattingEnabled = true;
            this.comboBox_from.IntegralHeight = false;
            this.comboBox_from.Location = new System.Drawing.Point(159, 92);
            this.comboBox_from.Margin = new System.Windows.Forms.Padding(7);
            this.comboBox_from.Name = "comboBox_from";
            this.comboBox_from.Size = new System.Drawing.Size(253, 32);
            this.comboBox_from.TabIndex = 6;
            this.comboBox_from.SizeChanged += new System.EventHandler(this.comboBox_from_SizeChanged);
            // 
            // label_matchStyle
            // 
            this.label_matchStyle.AutoSize = true;
            this.label_matchStyle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_matchStyle.Location = new System.Drawing.Point(7, 131);
            this.label_matchStyle.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
            this.label_matchStyle.Name = "label_matchStyle";
            this.label_matchStyle.Size = new System.Drawing.Size(138, 43);
            this.label_matchStyle.TabIndex = 7;
            this.label_matchStyle.Text = "匹配方式(&M):";
            this.label_matchStyle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboBox_matchStyle
            // 
            this.comboBox_matchStyle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBox_matchStyle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_matchStyle.FormattingEnabled = true;
            this.comboBox_matchStyle.Items.AddRange(new object[] {
            "前方一致",
            "中间一致",
            "后方一致",
            "精确一致",
            "空值"});
            this.comboBox_matchStyle.Location = new System.Drawing.Point(159, 138);
            this.comboBox_matchStyle.Margin = new System.Windows.Forms.Padding(7);
            this.comboBox_matchStyle.Name = "comboBox_matchStyle";
            this.comboBox_matchStyle.Size = new System.Drawing.Size(253, 29);
            this.comboBox_matchStyle.TabIndex = 8;
            this.comboBox_matchStyle.Text = "前方一致";
            this.comboBox_matchStyle.SizeChanged += new System.EventHandler(this.comboBox_matchStyle_SizeChanged);
            this.comboBox_matchStyle.TextChanged += new System.EventHandler(this.comboBox_matchStyle_TextChanged);
            // 
            // checkedComboBox_biblioDbNames
            // 
            this.checkedComboBox_biblioDbNames.BackColor = System.Drawing.SystemColors.Window;
            this.checkedComboBox_biblioDbNames.Dock = System.Windows.Forms.DockStyle.Fill;
            this.checkedComboBox_biblioDbNames.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkedComboBox_biblioDbNames.Location = new System.Drawing.Point(156, 49);
            this.checkedComboBox_biblioDbNames.Margin = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_biblioDbNames.Name = "checkedComboBox_biblioDbNames";
            this.checkedComboBox_biblioDbNames.Padding = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_biblioDbNames.ReadOnly = false;
            this.checkedComboBox_biblioDbNames.Size = new System.Drawing.Size(259, 32);
            this.checkedComboBox_biblioDbNames.TabIndex = 9;
            this.checkedComboBox_biblioDbNames.DropDown += new System.EventHandler(this.checkedComboBox_biblioDbNames_DropDown);
            this.checkedComboBox_biblioDbNames.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.checkedComboBox_biblioDbNames_ItemChecked);
            // 
            // toolStrip_search
            // 
            this.toolStrip_search.BackColor = System.Drawing.Color.Transparent;
            this.toolStrip_search.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStrip_search.Font = new System.Drawing.Font("Microsoft YaHei UI", 9F);
            this.toolStrip_search.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_search.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip_search.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_search,
            this.toolStripSeparator3,
            this.toolStripDropDownButton_searchKeys,
            this.toolStripDropDownButton_inputTimeString,
            this.toolStripSeparator1,
            this.toolStripButton_prevQuery,
            this.toolStripButton_nextQuery,
            this.toolStripButton_multiLine});
            this.toolStrip_search.Location = new System.Drawing.Point(419, 0);
            this.toolStrip_search.Name = "toolStrip_search";
            this.toolStrip_search.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip_search.Size = new System.Drawing.Size(314, 45);
            this.toolStrip_search.TabIndex = 13;
            this.toolStrip_search.Text = "检索";
            // 
            // toolStripButton_search
            // 
            this.toolStripButton_search.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_search.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_search.Image")));
            this.toolStripButton_search.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_search.Name = "toolStripButton_search";
            this.toolStripButton_search.Size = new System.Drawing.Size(40, 39);
            this.toolStripButton_search.Text = "检索";
            this.toolStripButton_search.Click += new System.EventHandler(this.toolStripButton_search_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 45);
            // 
            // toolStripDropDownButton_searchKeys
            // 
            this.toolStripDropDownButton_searchKeys.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.None;
            this.toolStripDropDownButton_searchKeys.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_continueLoad,
            this.toolStripMenuItem_searchKeyID,
            this.toolStripMenuItem_searchKeys,
            this.toolStripSeparator4,
            this.ToolStripMenuItem_searchShareBiblio,
            this.toolStripMenuItem_searchZ3950,
            this.ToolStripMenuItem_filterRecords,
            this.toolStripMenuItem_findInList,
            this.toolStripMenuItem_subrecords,
            this.toolStripSeparator5,
            this.ToolStripMenuItem_z3950ServerList});
            this.toolStripDropDownButton_searchKeys.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton_searchKeys.Image")));
            this.toolStripDropDownButton_searchKeys.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.toolStripDropDownButton_searchKeys.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_searchKeys.Name = "toolStripDropDownButton_searchKeys";
            this.toolStripDropDownButton_searchKeys.Size = new System.Drawing.Size(21, 39);
            this.toolStripDropDownButton_searchKeys.Text = "更多命令...";
            // 
            // ToolStripMenuItem_continueLoad
            // 
            this.ToolStripMenuItem_continueLoad.Enabled = false;
            this.ToolStripMenuItem_continueLoad.Name = "ToolStripMenuItem_continueLoad";
            this.ToolStripMenuItem_continueLoad.Size = new System.Drawing.Size(327, 40);
            this.ToolStripMenuItem_continueLoad.Text = "继续装入";
            this.ToolStripMenuItem_continueLoad.ToolTipText = "继续装入中断时尚未装入的那些浏览行";
            this.ToolStripMenuItem_continueLoad.Click += new System.EventHandler(this.ToolStripMenuItem_continueLoad_Click);
            // 
            // toolStripMenuItem_searchKeyID
            // 
            this.toolStripMenuItem_searchKeyID.Name = "toolStripMenuItem_searchKeyID";
            this.toolStripMenuItem_searchKeyID.Size = new System.Drawing.Size(327, 40);
            this.toolStripMenuItem_searchKeyID.Text = "带检索点的检索";
            this.toolStripMenuItem_searchKeyID.Click += new System.EventHandler(this.toolStripMenuItem_searchKeyID_Click);
            // 
            // toolStripMenuItem_searchKeys
            // 
            this.toolStripMenuItem_searchKeys.Name = "toolStripMenuItem_searchKeys";
            this.toolStripMenuItem_searchKeys.Size = new System.Drawing.Size(327, 40);
            this.toolStripMenuItem_searchKeys.Text = "仅获得检索点";
            this.toolStripMenuItem_searchKeys.Click += new System.EventHandler(this.toolStripMenuItem_searchKeys_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(324, 6);
            // 
            // ToolStripMenuItem_searchShareBiblio
            // 
            this.ToolStripMenuItem_searchShareBiblio.Name = "ToolStripMenuItem_searchShareBiblio";
            this.ToolStripMenuItem_searchShareBiblio.Size = new System.Drawing.Size(327, 40);
            this.ToolStripMenuItem_searchShareBiblio.Text = "使用共享网络";
            this.ToolStripMenuItem_searchShareBiblio.Click += new System.EventHandler(this.ToolStripMenuItem_searchShareBiblio_Click);
            // 
            // toolStripMenuItem_searchZ3950
            // 
            this.toolStripMenuItem_searchZ3950.Name = "toolStripMenuItem_searchZ3950";
            this.toolStripMenuItem_searchZ3950.Size = new System.Drawing.Size(327, 40);
            this.toolStripMenuItem_searchZ3950.Text = "使用 Z39.50";
            this.toolStripMenuItem_searchZ3950.Click += new System.EventHandler(this.toolStripMenuItem_searchZ3950_Click);
            // 
            // ToolStripMenuItem_filterRecords
            // 
            this.ToolStripMenuItem_filterRecords.Name = "ToolStripMenuItem_filterRecords";
            this.ToolStripMenuItem_filterRecords.Size = new System.Drawing.Size(327, 40);
            this.ToolStripMenuItem_filterRecords.Text = "筛选 ...";
            this.ToolStripMenuItem_filterRecords.Click += new System.EventHandler(this.ToolStripMenuItem_filterRecords_Click);
            // 
            // toolStripMenuItem_subrecords
            // 
            this.toolStripMenuItem_subrecords.CheckOnClick = true;
            this.toolStripMenuItem_subrecords.Name = "toolStripMenuItem_subrecords";
            this.toolStripMenuItem_subrecords.Size = new System.Drawing.Size(327, 40);
            this.toolStripMenuItem_subrecords.Text = "显示下级记录";
            this.toolStripMenuItem_subrecords.CheckedChanged += new System.EventHandler(this.toolStripMenuItem_subrecords_CheckedChanged);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(324, 6);
            // 
            // ToolStripMenuItem_z3950ServerList
            // 
            this.ToolStripMenuItem_z3950ServerList.Name = "ToolStripMenuItem_z3950ServerList";
            this.ToolStripMenuItem_z3950ServerList.Size = new System.Drawing.Size(327, 40);
            this.ToolStripMenuItem_z3950ServerList.Text = "Z39.50 服务器列表 ...";
            this.ToolStripMenuItem_z3950ServerList.Click += new System.EventHandler(this.ToolStripMenuItem_z3950ServerList_Click);
            // 
            // toolStripDropDownButton_inputTimeString
            // 
            this.toolStripDropDownButton_inputTimeString.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButton_inputTimeString.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_rfc1123Single,
            this.ToolStripMenuItem_uSingle,
            this.toolStripSeparator2,
            this.ToolStripMenuItem_rfc1123Range,
            this.ToolStripMenuItem_uRange});
            this.toolStripDropDownButton_inputTimeString.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton_inputTimeString.Image")));
            this.toolStripDropDownButton_inputTimeString.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_inputTimeString.Name = "toolStripDropDownButton_inputTimeString";
            this.toolStripDropDownButton_inputTimeString.Size = new System.Drawing.Size(45, 39);
            this.toolStripDropDownButton_inputTimeString.Text = "和时间有关的功能";
            // 
            // ToolStripMenuItem_rfc1123Single
            // 
            this.ToolStripMenuItem_rfc1123Single.Name = "ToolStripMenuItem_rfc1123Single";
            this.ToolStripMenuItem_rfc1123Single.Size = new System.Drawing.Size(336, 40);
            this.ToolStripMenuItem_rfc1123Single.Text = "RFC1123时间值...";
            this.ToolStripMenuItem_rfc1123Single.Visible = false;
            this.ToolStripMenuItem_rfc1123Single.Click += new System.EventHandler(this.ToolStripMenuItem_rfc1123Single_Click);
            // 
            // ToolStripMenuItem_uSingle
            // 
            this.ToolStripMenuItem_uSingle.Name = "ToolStripMenuItem_uSingle";
            this.ToolStripMenuItem_uSingle.Size = new System.Drawing.Size(336, 40);
            this.ToolStripMenuItem_uSingle.Text = "u时间值...";
            this.ToolStripMenuItem_uSingle.Click += new System.EventHandler(this.ToolStripMenuItem_uSingle_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(333, 6);
            // 
            // ToolStripMenuItem_rfc1123Range
            // 
            this.ToolStripMenuItem_rfc1123Range.Name = "ToolStripMenuItem_rfc1123Range";
            this.ToolStripMenuItem_rfc1123Range.Size = new System.Drawing.Size(336, 40);
            this.ToolStripMenuItem_rfc1123Range.Text = "RFC1123时间值范围...";
            this.ToolStripMenuItem_rfc1123Range.Visible = false;
            this.ToolStripMenuItem_rfc1123Range.Click += new System.EventHandler(this.ToolStripMenuItem_rfc1123Range_Click);
            // 
            // ToolStripMenuItem_uRange
            // 
            this.ToolStripMenuItem_uRange.Name = "ToolStripMenuItem_uRange";
            this.ToolStripMenuItem_uRange.Size = new System.Drawing.Size(336, 40);
            this.ToolStripMenuItem_uRange.Text = "u时间值范围...";
            this.ToolStripMenuItem_uRange.Click += new System.EventHandler(this.ToolStripMenuItem_uRange_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 45);
            this.toolStripSeparator1.Visible = false;
            // 
            // toolStripButton_prevQuery
            // 
            this.toolStripButton_prevQuery.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_prevQuery.Enabled = false;
            this.toolStripButton_prevQuery.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_prevQuery.Image")));
            this.toolStripButton_prevQuery.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_prevQuery.Name = "toolStripButton_prevQuery";
            this.toolStripButton_prevQuery.Size = new System.Drawing.Size(40, 39);
            this.toolStripButton_prevQuery.Text = "后退";
            this.toolStripButton_prevQuery.Click += new System.EventHandler(this.toolStripButton_prevQuery_Click);
            // 
            // toolStripButton_nextQuery
            // 
            this.toolStripButton_nextQuery.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_nextQuery.Enabled = false;
            this.toolStripButton_nextQuery.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_nextQuery.Image")));
            this.toolStripButton_nextQuery.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_nextQuery.Name = "toolStripButton_nextQuery";
            this.toolStripButton_nextQuery.Size = new System.Drawing.Size(40, 39);
            this.toolStripButton_nextQuery.Text = "前进";
            this.toolStripButton_nextQuery.Click += new System.EventHandler(this.toolStripButton_nextQuery_Click);
            // 
            // toolStripButton_multiLine
            // 
            this.toolStripButton_multiLine.CheckOnClick = true;
            this.toolStripButton_multiLine.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_multiLine.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_multiLine.Image")));
            this.toolStripButton_multiLine.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_multiLine.Name = "toolStripButton_multiLine";
            this.toolStripButton_multiLine.Size = new System.Drawing.Size(58, 39);
            this.toolStripButton_multiLine.Text = "多行";
            this.toolStripButton_multiLine.CheckedChanged += new System.EventHandler(this.toolStripButton_multiLine_CheckedChanged);
            // 
            // button_search
            // 
            this.button_search.Location = new System.Drawing.Point(424, 48);
            this.button_search.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.button_search.Name = "button_search";
            this.button_search.Size = new System.Drawing.Size(0, 0);
            this.button_search.TabIndex = 14;
            this.button_search.Text = "检索(&S)";
            this.button_search.UseVisualStyleBackColor = true;
            this.button_search.Click += new System.EventHandler(this.button_search_Click);
            // 
            // comboBox_location
            // 
            this.comboBox_location.Dock = System.Windows.Forms.DockStyle.Fill;
            this.comboBox_location.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_location.FormattingEnabled = true;
            this.comboBox_location.Location = new System.Drawing.Point(159, 181);
            this.comboBox_location.Margin = new System.Windows.Forms.Padding(7);
            this.comboBox_location.Name = "comboBox_location";
            this.comboBox_location.Size = new System.Drawing.Size(253, 29);
            this.comboBox_location.TabIndex = 16;
            // 
            // tableLayoutPanel_main
            // 
            this.tableLayoutPanel_main.ColumnCount = 1;
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.Controls.Add(this.label_message, 0, 1);
            this.tableLayoutPanel_main.Controls.Add(this.splitContainer_main, 0, 0);
            this.tableLayoutPanel_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_main.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_main.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.tableLayoutPanel_main.Name = "tableLayoutPanel_main";
            this.tableLayoutPanel_main.Padding = new System.Windows.Forms.Padding(0, 21, 0, 21);
            this.tableLayoutPanel_main.RowCount = 2;
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.Size = new System.Drawing.Size(897, 647);
            this.tableLayoutPanel_main.TabIndex = 0;
            // 
            // label_message
            // 
            this.label_message.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label_message.Location = new System.Drawing.Point(0, 588);
            this.label_message.Margin = new System.Windows.Forms.Padding(0, 3, 0, 0);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(897, 38);
            this.label_message.TabIndex = 1;
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_main.Location = new System.Drawing.Point(5, 26);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(5);
            this.splitContainer_main.Name = "splitContainer_main";
            this.splitContainer_main.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.tabControl_query);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.listView_records);
            this.splitContainer_main.Size = new System.Drawing.Size(887, 554);
            this.splitContainer_main.SplitterDistance = 290;
            this.splitContainer_main.SplitterWidth = 14;
            this.splitContainer_main.TabIndex = 2;
            // 
            // tabControl_query
            // 
            this.tabControl_query.Controls.Add(this.tabPage_simple);
            this.tabControl_query.Controls.Add(this.tabPage_logic);
            this.tabControl_query.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_query.Location = new System.Drawing.Point(0, 0);
            this.tabControl_query.Margin = new System.Windows.Forms.Padding(5);
            this.tabControl_query.Name = "tabControl_query";
            this.tabControl_query.SelectedIndex = 0;
            this.tabControl_query.Size = new System.Drawing.Size(887, 290);
            this.tabControl_query.TabIndex = 1;
            // 
            // tabPage_simple
            // 
            this.tabPage_simple.AutoScroll = true;
            this.tabPage_simple.Controls.Add(this.tableLayoutPanel_query);
            this.tabPage_simple.Location = new System.Drawing.Point(4, 31);
            this.tabPage_simple.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage_simple.Name = "tabPage_simple";
            this.tabPage_simple.Padding = new System.Windows.Forms.Padding(5);
            this.tabPage_simple.Size = new System.Drawing.Size(879, 255);
            this.tabPage_simple.TabIndex = 0;
            this.tabPage_simple.Text = "简单";
            this.tabPage_simple.UseVisualStyleBackColor = true;
            // 
            // tabPage_logic
            // 
            this.tabPage_logic.AutoScroll = true;
            this.tabPage_logic.Controls.Add(this.dp2QueryControl1);
            this.tabPage_logic.Location = new System.Drawing.Point(4, 31);
            this.tabPage_logic.Margin = new System.Windows.Forms.Padding(5);
            this.tabPage_logic.Name = "tabPage_logic";
            this.tabPage_logic.Padding = new System.Windows.Forms.Padding(5);
            this.tabPage_logic.Size = new System.Drawing.Size(879, 255);
            this.tabPage_logic.TabIndex = 1;
            this.tabPage_logic.Text = "逻辑";
            this.tabPage_logic.UseVisualStyleBackColor = true;
            // 
            // dp2QueryControl1
            // 
            this.dp2QueryControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dp2QueryControl1.Location = new System.Drawing.Point(5, 5);
            this.dp2QueryControl1.Margin = new System.Windows.Forms.Padding(11, 10, 11, 10);
            this.dp2QueryControl1.Name = "dp2QueryControl1";
            this.dp2QueryControl1.PanelMode = DigitalPlatform.CommonControl.PanelMode.None;
            this.dp2QueryControl1.Size = new System.Drawing.Size(869, 245);
            this.dp2QueryControl1.TabIndex = 0;
            this.dp2QueryControl1.GetList += new DigitalPlatform.CommonControl.GetListEventHandler(this.dp2QueryControl1_GetList);
            this.dp2QueryControl1.ViewXml += new System.EventHandler(this.dp2QueryControl1_ViewXml);
            this.dp2QueryControl1.AppendMenu += new DigitalPlatform.ApendMenuEventHandler(this.dp2QueryControl1_AppendMenu);
            this.dp2QueryControl1.GetFromStyle += new DigitalPlatform.CommonControl.GetFromStyleHandler(this.dp2QueryControl1_GetFromStyle);
            // 
            // listView_records
            // 
            this.listView_records.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView_records.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_path,
            this.columnHeader_1});
            this.listView_records.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_records.FullRowSelect = true;
            this.listView_records.HideSelection = false;
            this.listView_records.Location = new System.Drawing.Point(0, 0);
            this.listView_records.Margin = new System.Windows.Forms.Padding(0);
            this.listView_records.Name = "listView_records";
            this.listView_records.Size = new System.Drawing.Size(887, 250);
            this.listView_records.TabIndex = 0;
            this.listView_records.UseCompatibleStateImageBehavior = false;
            this.listView_records.View = System.Windows.Forms.View.Details;
            this.listView_records.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_records_ColumnClick);
            this.listView_records.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.listView_records_ItemDrag);
            this.listView_records.SelectedIndexChanged += new System.EventHandler(this.listView_records_SelectedIndexChanged);
            this.listView_records.DoubleClick += new System.EventHandler(this.listView_records_DoubleClick);
            this.listView_records.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_records_MouseUp);
            // 
            // columnHeader_path
            // 
            this.columnHeader_path.Text = "路径";
            this.columnHeader_path.Width = 100;
            // 
            // columnHeader_1
            // 
            this.columnHeader_1.Text = "1";
            this.columnHeader_1.Width = 300;
            // 
            // toolStripMenuItem_findInList
            // 
            this.toolStripMenuItem_findInList.Name = "toolStripMenuItem_findInList";
            this.toolStripMenuItem_findInList.Size = new System.Drawing.Size(327, 40);
            this.toolStripMenuItem_findInList.Text = "在列表中查找...";
            this.toolStripMenuItem_findInList.Click += new System.EventHandler(this.toolStripMenuItem_findInList_Click);
            // 
            // BiblioSearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(897, 647);
            this.Controls.Add(this.tableLayoutPanel_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.Name = "BiblioSearchForm";
            this.ShowInTaskbar = false;
            this.Text = "书目查询";
            this.Activated += new System.EventHandler(this.BiblioSearchForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.BiblioSearchForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.BiblioSearchForm_FormClosed);
            this.Load += new System.EventHandler(this.BiblioSearchForm_Load);
            this.VisibleChanged += new System.EventHandler(this.BiblioSearchForm_VisibleChanged);
            this.contextMenuStrip_biblioDb.ResumeLayout(false);
            this.tableLayoutPanel_query.ResumeLayout(false);
            this.tableLayoutPanel_query.PerformLayout();
            this.toolStrip_search.ResumeLayout(false);
            this.toolStrip_search.PerformLayout();
            this.tableLayoutPanel_main.ResumeLayout(false);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.tabControl_query.ResumeLayout(false);
            this.tabPage_simple.ResumeLayout(false);
            this.tabPage_simple.PerformLayout();
            this.tabPage_logic.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private DigitalPlatform.GUI.ListViewNF listView_records;
        private System.Windows.Forms.ColumnHeader columnHeader_path;
        private System.Windows.Forms.ColumnHeader columnHeader_1;
        private DigitalPlatform.CommonControl.TabComboBox comboBox_from;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_queryWord;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label_biblioDbName;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_biblioDb;
        private System.Windows.Forms.ToolStripMenuItem MenuItem_viewBiblioDbProperty;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_query;
        private System.Windows.Forms.Label label_matchStyle;
        private System.Windows.Forms.ComboBox comboBox_matchStyle;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_main;
        private System.Windows.Forms.Label label_message;
        private DigitalPlatform.CommonControl.CheckedComboBox checkedComboBox_biblioDbNames;
        private System.Windows.Forms.ToolStrip toolStrip_search;
        private System.Windows.Forms.ToolStripButton toolStripButton_search;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_searchKeys;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_continueLoad;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_searchKeyID;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_prevQuery;
        private System.Windows.Forms.ToolStripButton toolStripButton_nextQuery;
        private System.Windows.Forms.Button button_search;
        private System.Windows.Forms.TabControl tabControl_query;
        private System.Windows.Forms.TabPage tabPage_simple;
        private System.Windows.Forms.TabPage tabPage_logic;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private DigitalPlatform.CommonControl.dp2QueryControl dp2QueryControl1;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_inputTimeString;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_rfc1123Single;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_uSingle;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_rfc1123Range;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_uRange;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_searchKeys;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_searchShareBiblio;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox_location;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_filterRecords;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_searchZ3950;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_z3950ServerList;
        private System.Windows.Forms.ToolStripButton toolStripButton_multiLine;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_subrecords;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_findInList;
    }
}