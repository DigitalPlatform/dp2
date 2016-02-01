namespace dp2Catalog
{
    partial class dp2SearchForm
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

            if (this.Channels != null)
                this.Channels.Dispose();

            this.EventLoadFinish.Dispose();

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(dp2SearchForm));
            this.columnHeader_1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.textBox_resPath = new System.Windows.Forms.TextBox();
            this.columnHeader_path = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.textBox_resultInfo = new System.Windows.Forms.TextBox();
            this.listView_browse = new DigitalPlatform.GUI.ListViewQU();
            this.columnHeader_2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label2 = new System.Windows.Forms.Label();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.splitContainer_up = new System.Windows.Forms.SplitContainer();
            this.panel_target = new System.Windows.Forms.Panel();
            this.panel_resTree = new System.Windows.Forms.Panel();
            this.dp2ResTree1 = new DigitalPlatform.CirculationClient.dp2ResTree();
            this.splitContainer_queryAndResultInfo = new System.Windows.Forms.SplitContainer();
            this.tabControl_query = new System.Windows.Forms.TabControl();
            this.tabPage_simple = new System.Windows.Forms.TabPage();
            this.comboBox_simple_matchStyle = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button_searchSimple = new System.Windows.Forms.Button();
            this.textBox_simple_queryWord = new System.Windows.Forms.TextBox();
            this.label_simple_queryWord = new System.Windows.Forms.Label();
            this.tabPage_multiline = new System.Windows.Forms.TabPage();
            this.panel_multiline = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.comboBox_multiline_matchStyle = new System.Windows.Forms.ComboBox();
            this.textBox_mutiline_queryContent = new System.Windows.Forms.TextBox();
            this.tabPage_logic = new System.Windows.Forms.TabPage();
            this.dp2QueryControl1 = new DigitalPlatform.CommonControl.dp2QueryControl();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_up)).BeginInit();
            this.splitContainer_up.Panel1.SuspendLayout();
            this.splitContainer_up.Panel2.SuspendLayout();
            this.splitContainer_up.SuspendLayout();
            this.panel_target.SuspendLayout();
            this.panel_resTree.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_queryAndResultInfo)).BeginInit();
            this.splitContainer_queryAndResultInfo.Panel1.SuspendLayout();
            this.splitContainer_queryAndResultInfo.Panel2.SuspendLayout();
            this.splitContainer_queryAndResultInfo.SuspendLayout();
            this.tabControl_query.SuspendLayout();
            this.tabPage_simple.SuspendLayout();
            this.tabPage_multiline.SuspendLayout();
            this.panel_multiline.SuspendLayout();
            this.tabPage_logic.SuspendLayout();
            this.SuspendLayout();
            // 
            // columnHeader_1
            // 
            this.columnHeader_1.Text = "1";
            this.columnHeader_1.Width = 200;
            // 
            // textBox_resPath
            // 
            this.textBox_resPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_resPath.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_resPath.Location = new System.Drawing.Point(83, 142);
            this.textBox_resPath.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_resPath.Name = "textBox_resPath";
            this.textBox_resPath.ReadOnly = true;
            this.textBox_resPath.Size = new System.Drawing.Size(208, 14);
            this.textBox_resPath.TabIndex = 1;
            // 
            // columnHeader_path
            // 
            this.columnHeader_path.Text = "记录路径";
            this.columnHeader_path.Width = 200;
            // 
            // textBox_resultInfo
            // 
            this.textBox_resultInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_resultInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_resultInfo.Location = new System.Drawing.Point(0, 0);
            this.textBox_resultInfo.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_resultInfo.MaxLength = 1000000;
            this.textBox_resultInfo.Multiline = true;
            this.textBox_resultInfo.Name = "textBox_resultInfo";
            this.textBox_resultInfo.ReadOnly = true;
            this.textBox_resultInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_resultInfo.Size = new System.Drawing.Size(274, 69);
            this.textBox_resultInfo.TabIndex = 1;
            // 
            // listView_browse
            // 
            this.listView_browse.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView_browse.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_path,
            this.columnHeader_1,
            this.columnHeader_2,
            this.columnHeader_3,
            this.columnHeader_4,
            this.columnHeader_5});
            this.listView_browse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_browse.FullRowSelect = true;
            this.listView_browse.HideSelection = false;
            this.listView_browse.Location = new System.Drawing.Point(0, 0);
            this.listView_browse.Margin = new System.Windows.Forms.Padding(2);
            this.listView_browse.Name = "listView_browse";
            this.listView_browse.Size = new System.Drawing.Size(569, 175);
            this.listView_browse.TabIndex = 1;
            this.listView_browse.UseCompatibleStateImageBehavior = false;
            this.listView_browse.View = System.Windows.Forms.View.Details;
            this.listView_browse.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_browse_ColumnClick);
            this.listView_browse.SelectedIndexChanged += new System.EventHandler(this.listView_browse_SelectedIndexChanged);
            this.listView_browse.DoubleClick += new System.EventHandler(this.listView_browse_DoubleClick);
            this.listView_browse.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_browse_MouseUp);
            // 
            // columnHeader_2
            // 
            this.columnHeader_2.Text = "2";
            this.columnHeader_2.Width = 100;
            // 
            // columnHeader_3
            // 
            this.columnHeader_3.Text = "3";
            this.columnHeader_3.Width = 50;
            // 
            // columnHeader_4
            // 
            this.columnHeader_4.Text = "4";
            this.columnHeader_4.Width = 150;
            // 
            // columnHeader_5
            // 
            this.columnHeader_5.Text = "5";
            this.columnHeader_5.Width = 100;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(-2, 142);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "目标路径(&P):";
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.BackColor = System.Drawing.SystemColors.Control;
            this.splitContainer_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_main.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_main.Name = "splitContainer_main";
            this.splitContainer_main.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.splitContainer_up);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.listView_browse);
            this.splitContainer_main.Size = new System.Drawing.Size(569, 340);
            this.splitContainer_main.SplitterDistance = 159;
            this.splitContainer_main.SplitterWidth = 6;
            this.splitContainer_main.TabIndex = 2;
            // 
            // splitContainer_up
            // 
            this.splitContainer_up.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_up.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_up.Margin = new System.Windows.Forms.Padding(2);
            this.splitContainer_up.Name = "splitContainer_up";
            // 
            // splitContainer_up.Panel1
            // 
            this.splitContainer_up.Panel1.Controls.Add(this.panel_target);
            // 
            // splitContainer_up.Panel2
            // 
            this.splitContainer_up.Panel2.Controls.Add(this.splitContainer_queryAndResultInfo);
            this.splitContainer_up.Size = new System.Drawing.Size(569, 159);
            this.splitContainer_up.SplitterDistance = 292;
            this.splitContainer_up.SplitterWidth = 3;
            this.splitContainer_up.TabIndex = 0;
            // 
            // panel_target
            // 
            this.panel_target.Controls.Add(this.panel_resTree);
            this.panel_target.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_target.Location = new System.Drawing.Point(0, 0);
            this.panel_target.Margin = new System.Windows.Forms.Padding(2);
            this.panel_target.Name = "panel_target";
            this.panel_target.Size = new System.Drawing.Size(292, 159);
            this.panel_target.TabIndex = 0;
            // 
            // panel_resTree
            // 
            this.panel_resTree.Controls.Add(this.dp2ResTree1);
            this.panel_resTree.Controls.Add(this.textBox_resPath);
            this.panel_resTree.Controls.Add(this.label2);
            this.panel_resTree.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_resTree.Location = new System.Drawing.Point(0, 0);
            this.panel_resTree.Margin = new System.Windows.Forms.Padding(2);
            this.panel_resTree.Name = "panel_resTree";
            this.panel_resTree.Size = new System.Drawing.Size(292, 159);
            this.panel_resTree.TabIndex = 3;
            // 
            // dp2ResTree1
            // 
            this.dp2ResTree1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dp2ResTree1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dp2ResTree1.HideSelection = false;
            this.dp2ResTree1.ImageIndex = 0;
            this.dp2ResTree1.Location = new System.Drawing.Point(0, 0);
            this.dp2ResTree1.Margin = new System.Windows.Forms.Padding(2);
            this.dp2ResTree1.Name = "dp2ResTree1";
            this.dp2ResTree1.SelectedImageIndex = 0;
            this.dp2ResTree1.Size = new System.Drawing.Size(292, 138);
            this.dp2ResTree1.TabIndex = 0;
            this.dp2ResTree1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.dp2ResTree1_AfterSelect);
            // 
            // splitContainer_queryAndResultInfo
            // 
            this.splitContainer_queryAndResultInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_queryAndResultInfo.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_queryAndResultInfo.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_queryAndResultInfo.Name = "splitContainer_queryAndResultInfo";
            this.splitContainer_queryAndResultInfo.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_queryAndResultInfo.Panel1
            // 
            this.splitContainer_queryAndResultInfo.Panel1.Controls.Add(this.tabControl_query);
            // 
            // splitContainer_queryAndResultInfo.Panel2
            // 
            this.splitContainer_queryAndResultInfo.Panel2.Controls.Add(this.textBox_resultInfo);
            this.splitContainer_queryAndResultInfo.Size = new System.Drawing.Size(274, 159);
            this.splitContainer_queryAndResultInfo.SplitterDistance = 87;
            this.splitContainer_queryAndResultInfo.SplitterWidth = 3;
            this.splitContainer_queryAndResultInfo.TabIndex = 0;
            // 
            // tabControl_query
            // 
            this.tabControl_query.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl_query.Controls.Add(this.tabPage_simple);
            this.tabControl_query.Controls.Add(this.tabPage_multiline);
            this.tabControl_query.Controls.Add(this.tabPage_logic);
            this.tabControl_query.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_query.Location = new System.Drawing.Point(0, 0);
            this.tabControl_query.Margin = new System.Windows.Forms.Padding(0);
            this.tabControl_query.Name = "tabControl_query";
            this.tabControl_query.Padding = new System.Drawing.Point(0, 0);
            this.tabControl_query.SelectedIndex = 0;
            this.tabControl_query.Size = new System.Drawing.Size(274, 87);
            this.tabControl_query.TabIndex = 0;
            this.tabControl_query.Selected += new System.Windows.Forms.TabControlEventHandler(this.tabControl_query_Selected);
            // 
            // tabPage_simple
            // 
            this.tabPage_simple.Controls.Add(this.comboBox_simple_matchStyle);
            this.tabPage_simple.Controls.Add(this.label3);
            this.tabPage_simple.Controls.Add(this.button_searchSimple);
            this.tabPage_simple.Controls.Add(this.textBox_simple_queryWord);
            this.tabPage_simple.Controls.Add(this.label_simple_queryWord);
            this.tabPage_simple.Location = new System.Drawing.Point(4, 25);
            this.tabPage_simple.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_simple.Name = "tabPage_simple";
            this.tabPage_simple.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_simple.Size = new System.Drawing.Size(266, 58);
            this.tabPage_simple.TabIndex = 0;
            this.tabPage_simple.Text = "简单";
            this.tabPage_simple.UseVisualStyleBackColor = true;
            // 
            // comboBox_simple_matchStyle
            // 
            this.comboBox_simple_matchStyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_simple_matchStyle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_simple_matchStyle.FormattingEnabled = true;
            this.comboBox_simple_matchStyle.Items.AddRange(new object[] {
            "前方一致",
            "中间一致",
            "后方一致",
            "精确一致",
            "空值"});
            this.comboBox_simple_matchStyle.Location = new System.Drawing.Point(79, 28);
            this.comboBox_simple_matchStyle.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_simple_matchStyle.Name = "comboBox_simple_matchStyle";
            this.comboBox_simple_matchStyle.Size = new System.Drawing.Size(139, 20);
            this.comboBox_simple_matchStyle.TabIndex = 4;
            this.comboBox_simple_matchStyle.SizeChanged += new System.EventHandler(this.comboBox_matchStyle_SizeChanged);
            this.comboBox_simple_matchStyle.TextChanged += new System.EventHandler(this.comboBox_matchStyle_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(-2, 31);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 3;
            this.label3.Text = "匹配方式(&M):";
            // 
            // button_searchSimple
            // 
            this.button_searchSimple.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_searchSimple.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_searchSimple.Location = new System.Drawing.Point(222, 5);
            this.button_searchSimple.Margin = new System.Windows.Forms.Padding(2);
            this.button_searchSimple.Name = "button_searchSimple";
            this.button_searchSimple.Size = new System.Drawing.Size(44, 22);
            this.button_searchSimple.TabIndex = 2;
            this.button_searchSimple.Text = "检索";
            this.button_searchSimple.UseVisualStyleBackColor = true;
            this.button_searchSimple.Click += new System.EventHandler(this.button_searchSimple_Click);
            // 
            // textBox_simple_queryWord
            // 
            this.textBox_simple_queryWord.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_simple_queryWord.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_simple_queryWord.Location = new System.Drawing.Point(79, 5);
            this.textBox_simple_queryWord.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_simple_queryWord.Name = "textBox_simple_queryWord";
            this.textBox_simple_queryWord.Size = new System.Drawing.Size(139, 21);
            this.textBox_simple_queryWord.TabIndex = 1;
            // 
            // label_simple_queryWord
            // 
            this.label_simple_queryWord.AutoSize = true;
            this.label_simple_queryWord.Location = new System.Drawing.Point(-3, 7);
            this.label_simple_queryWord.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_simple_queryWord.Name = "label_simple_queryWord";
            this.label_simple_queryWord.Size = new System.Drawing.Size(65, 12);
            this.label_simple_queryWord.TabIndex = 0;
            this.label_simple_queryWord.Text = "检索词(&Q):";
            this.label_simple_queryWord.MouseUp += new System.Windows.Forms.MouseEventHandler(this.label_simple_queryWord_MouseUp);
            // 
            // tabPage_multiline
            // 
            this.tabPage_multiline.Controls.Add(this.panel_multiline);
            this.tabPage_multiline.Location = new System.Drawing.Point(4, 25);
            this.tabPage_multiline.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_multiline.Name = "tabPage_multiline";
            this.tabPage_multiline.Padding = new System.Windows.Forms.Padding(2);
            this.tabPage_multiline.Size = new System.Drawing.Size(266, 80);
            this.tabPage_multiline.TabIndex = 1;
            this.tabPage_multiline.Text = "多行";
            this.tabPage_multiline.UseVisualStyleBackColor = true;
            // 
            // panel_multiline
            // 
            this.panel_multiline.Controls.Add(this.label4);
            this.panel_multiline.Controls.Add(this.comboBox_multiline_matchStyle);
            this.panel_multiline.Controls.Add(this.textBox_mutiline_queryContent);
            this.panel_multiline.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel_multiline.Location = new System.Drawing.Point(2, 2);
            this.panel_multiline.Margin = new System.Windows.Forms.Padding(2);
            this.panel_multiline.Name = "panel_multiline";
            this.panel_multiline.Size = new System.Drawing.Size(262, 76);
            this.panel_multiline.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(-3, 62);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 5;
            this.label4.Text = "匹配方式(&M):";
            // 
            // comboBox_multiline_matchStyle
            // 
            this.comboBox_multiline_matchStyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_multiline_matchStyle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_multiline_matchStyle.FormattingEnabled = true;
            this.comboBox_multiline_matchStyle.Items.AddRange(new object[] {
            "前方一致",
            "中间一致",
            "后方一致",
            "精确一致"});
            this.comboBox_multiline_matchStyle.Location = new System.Drawing.Point(76, 60);
            this.comboBox_multiline_matchStyle.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_multiline_matchStyle.Name = "comboBox_multiline_matchStyle";
            this.comboBox_multiline_matchStyle.Size = new System.Drawing.Size(187, 20);
            this.comboBox_multiline_matchStyle.TabIndex = 6;
            // 
            // textBox_mutiline_queryContent
            // 
            this.textBox_mutiline_queryContent.AcceptsReturn = true;
            this.textBox_mutiline_queryContent.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_mutiline_queryContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_mutiline_queryContent.Location = new System.Drawing.Point(0, 0);
            this.textBox_mutiline_queryContent.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_mutiline_queryContent.MaxLength = 1000000;
            this.textBox_mutiline_queryContent.Multiline = true;
            this.textBox_mutiline_queryContent.Name = "textBox_mutiline_queryContent";
            this.textBox_mutiline_queryContent.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_mutiline_queryContent.Size = new System.Drawing.Size(262, 55);
            this.textBox_mutiline_queryContent.TabIndex = 0;
            // 
            // tabPage_logic
            // 
            this.tabPage_logic.Controls.Add(this.dp2QueryControl1);
            this.tabPage_logic.Location = new System.Drawing.Point(4, 25);
            this.tabPage_logic.Name = "tabPage_logic";
            this.tabPage_logic.Size = new System.Drawing.Size(266, 80);
            this.tabPage_logic.TabIndex = 2;
            this.tabPage_logic.Text = "逻辑";
            this.tabPage_logic.UseVisualStyleBackColor = true;
            // 
            // dp2QueryControl1
            // 
            this.dp2QueryControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dp2QueryControl1.Location = new System.Drawing.Point(0, 0);
            this.dp2QueryControl1.Name = "dp2QueryControl1";
            this.dp2QueryControl1.Size = new System.Drawing.Size(266, 80);
            this.dp2QueryControl1.TabIndex = 0;
            this.dp2QueryControl1.GetList += new DigitalPlatform.CommonControl.GetListEventHandler(this.dp2QueryControl1_GetList);
            this.dp2QueryControl1.ViewXml += new System.EventHandler(this.dp2QueryControl1_ViewXml);
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // dp2SearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(569, 340);
            this.Controls.Add(this.splitContainer_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "dp2SearchForm";
            this.ShowInTaskbar = false;
            this.Text = "dp2检索窗";
            this.Activated += new System.EventHandler(this.dp2SearchForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.dp2SearchForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.dp2SearchForm_FormClosed);
            this.Load += new System.EventHandler(this.dp2SearchForm_Load);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.splitContainer_up.Panel1.ResumeLayout(false);
            this.splitContainer_up.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_up)).EndInit();
            this.splitContainer_up.ResumeLayout(false);
            this.panel_target.ResumeLayout(false);
            this.panel_resTree.ResumeLayout(false);
            this.panel_resTree.PerformLayout();
            this.splitContainer_queryAndResultInfo.Panel1.ResumeLayout(false);
            this.splitContainer_queryAndResultInfo.Panel2.ResumeLayout(false);
            this.splitContainer_queryAndResultInfo.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_queryAndResultInfo)).EndInit();
            this.splitContainer_queryAndResultInfo.ResumeLayout(false);
            this.tabControl_query.ResumeLayout(false);
            this.tabPage_simple.ResumeLayout(false);
            this.tabPage_simple.PerformLayout();
            this.tabPage_multiline.ResumeLayout(false);
            this.panel_multiline.ResumeLayout(false);
            this.panel_multiline.PerformLayout();
            this.tabPage_logic.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ColumnHeader columnHeader_1;
        private System.Windows.Forms.TextBox textBox_resPath;
        private System.Windows.Forms.ColumnHeader columnHeader_path;
        private System.Windows.Forms.TextBox textBox_resultInfo;
        private DigitalPlatform.GUI.ListViewQU listView_browse;
        private System.Windows.Forms.ColumnHeader columnHeader_2;
        private System.Windows.Forms.ColumnHeader columnHeader_3;
        private System.Windows.Forms.ColumnHeader columnHeader_4;
        private System.Windows.Forms.ColumnHeader columnHeader_5;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.SplitContainer splitContainer_up;
        private System.Windows.Forms.Panel panel_target;
        internal DigitalPlatform.CirculationClient.dp2ResTree dp2ResTree1;
        private System.Windows.Forms.SplitContainer splitContainer_queryAndResultInfo;
        private System.Windows.Forms.TextBox textBox_simple_queryWord;
        private System.Windows.Forms.Label label_simple_queryWord;
        private System.Windows.Forms.TabControl tabControl_query;
        private System.Windows.Forms.TabPage tabPage_simple;
        private System.Windows.Forms.TabPage tabPage_multiline;
        private System.Windows.Forms.TextBox textBox_mutiline_queryContent;
        private System.Windows.Forms.Button button_searchSimple;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_simple_matchStyle;
        private System.Windows.Forms.Panel panel_resTree;
        private System.Windows.Forms.Panel panel_multiline;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox comboBox_multiline_matchStyle;
        private System.Windows.Forms.TabPage tabPage_logic;
        private DigitalPlatform.CommonControl.dp2QueryControl dp2QueryControl1;
        private System.Windows.Forms.Timer timer1;
    }
}