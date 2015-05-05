namespace dp2Catalog
{
	partial class ZBatchSearchForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZBatchSearchForm));
            this.tabControl_steps = new System.Windows.Forms.TabControl();
            this.tabPage_target = new System.Windows.Forms.TabPage();
            this.zTargetControl1 = new dp2Catalog.ZTargetControl();
            this.tabPage_queryLines = new System.Windows.Forms.TabPage();
            this.tableLayoutPanel_queryLines = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.button_queryLines_load = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_queryLines_filename = new System.Windows.Forms.TextBox();
            this.textBox_queryLines_content = new System.Windows.Forms.TextBox();
            this.tabPage_features = new System.Windows.Forms.TabPage();
            this.numericUpDown_features_oneWordMaxHitCount = new System.Windows.Forms.NumericUpDown();
            this.label8 = new System.Windows.Forms.Label();
            this.comboBox_features_elementSetName = new System.Windows.Forms.ComboBox();
            this.label7 = new System.Windows.Forms.Label();
            this.comboBox_features_syntax = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.comboBox_features_from = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tabPage_search = new System.Windows.Forms.TabPage();
            this.dpTable_queryWords = new DigitalPlatform.CommonControl.DpTable();
            this.dpColumn9 = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn6 = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn7 = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn8 = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn10 = new DigitalPlatform.CommonControl.DpColumn();
            this.tabPage_saveResults = new System.Windows.Forms.TabPage();
            this.textBox_saveResult_notHitFilename = new System.Windows.Forms.TextBox();
            this.button_saveResult_saveNotHitFile = new System.Windows.Forms.Button();
            this.button_saveResult_findNotHitFilename = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox_saveResult_multiHitFilename = new System.Windows.Forms.TextBox();
            this.button_saveResult_saveMultiHitFile = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.button_saveResult_findMultiHitFilename = new System.Windows.Forms.Button();
            this.textBox_saveResult_singleHitFilename = new System.Windows.Forms.TextBox();
            this.button_saveResult_findSingleHitFilename = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.button_saveResult_saveSingleHitFile = new System.Windows.Forms.Button();
            this.button_next = new System.Windows.Forms.Button();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.dpTable_records = new DigitalPlatform.CommonControl.DpTable();
            this.dpColumn1 = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn2 = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn3 = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn4 = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn5 = new DigitalPlatform.CommonControl.DpColumn();
            this.tableLayoutPanel_main = new System.Windows.Forms.TableLayoutPanel();
            this.imageList_browseItemType = new System.Windows.Forms.ImageList(this.components);
            this.imageList_queryWords = new System.Windows.Forms.ImageList(this.components);
            this.tabControl_steps.SuspendLayout();
            this.tabPage_target.SuspendLayout();
            this.tabPage_queryLines.SuspendLayout();
            this.tableLayoutPanel_queryLines.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabPage_features.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_features_oneWordMaxHitCount)).BeginInit();
            this.tabPage_search.SuspendLayout();
            this.tabPage_saveResults.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.tableLayoutPanel_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_steps
            // 
            this.tabControl_steps.Controls.Add(this.tabPage_target);
            this.tabControl_steps.Controls.Add(this.tabPage_queryLines);
            this.tabControl_steps.Controls.Add(this.tabPage_features);
            this.tabControl_steps.Controls.Add(this.tabPage_search);
            this.tabControl_steps.Controls.Add(this.tabPage_saveResults);
            this.tabControl_steps.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_steps.Location = new System.Drawing.Point(0, 0);
            this.tabControl_steps.Name = "tabControl_steps";
            this.tabControl_steps.SelectedIndex = 0;
            this.tabControl_steps.Size = new System.Drawing.Size(436, 172);
            this.tabControl_steps.TabIndex = 0;
            this.tabControl_steps.SelectedIndexChanged += new System.EventHandler(this.tabControl_steps_SelectedIndexChanged);
            // 
            // tabPage_target
            // 
            this.tabPage_target.Controls.Add(this.zTargetControl1);
            this.tabPage_target.Location = new System.Drawing.Point(4, 22);
            this.tabPage_target.Name = "tabPage_target";
            this.tabPage_target.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_target.Size = new System.Drawing.Size(428, 146);
            this.tabPage_target.TabIndex = 0;
            this.tabPage_target.Text = "检索目标";
            this.tabPage_target.UseVisualStyleBackColor = true;
            // 
            // zTargetControl1
            // 
            this.zTargetControl1.Changed = false;
            this.zTargetControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zTargetControl1.HideSelection = false;
            this.zTargetControl1.ImageIndex = 0;
            this.zTargetControl1.Location = new System.Drawing.Point(3, 3);
            this.zTargetControl1.Name = "zTargetControl1";
            this.zTargetControl1.SelectedImageIndex = 0;
            this.zTargetControl1.Size = new System.Drawing.Size(422, 140);
            this.zTargetControl1.TabIndex = 0;
            this.zTargetControl1.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.zTargetControl1_AfterCheck);
            this.zTargetControl1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.zTargetControl1_MouseUp);
            // 
            // tabPage_queryLines
            // 
            this.tabPage_queryLines.Controls.Add(this.tableLayoutPanel_queryLines);
            this.tabPage_queryLines.Location = new System.Drawing.Point(4, 22);
            this.tabPage_queryLines.Name = "tabPage_queryLines";
            this.tabPage_queryLines.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_queryLines.Size = new System.Drawing.Size(428, 146);
            this.tabPage_queryLines.TabIndex = 1;
            this.tabPage_queryLines.Text = "检索词";
            this.tabPage_queryLines.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel_queryLines
            // 
            this.tableLayoutPanel_queryLines.ColumnCount = 1;
            this.tableLayoutPanel_queryLines.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_queryLines.Controls.Add(this.panel1, 0, 0);
            this.tableLayoutPanel_queryLines.Controls.Add(this.textBox_queryLines_content, 0, 1);
            this.tableLayoutPanel_queryLines.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_queryLines.Location = new System.Drawing.Point(3, 3);
            this.tableLayoutPanel_queryLines.Name = "tableLayoutPanel_queryLines";
            this.tableLayoutPanel_queryLines.RowCount = 2;
            this.tableLayoutPanel_queryLines.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_queryLines.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_queryLines.Size = new System.Drawing.Size(422, 140);
            this.tableLayoutPanel_queryLines.TabIndex = 19;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.button_queryLines_load);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.textBox_queryLines_filename);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(3, 3);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(416, 25);
            this.panel1.TabIndex = 18;
            // 
            // button_queryLines_load
            // 
            this.button_queryLines_load.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_queryLines_load.Location = new System.Drawing.Point(341, 0);
            this.button_queryLines_load.Name = "button_queryLines_load";
            this.button_queryLines_load.Size = new System.Drawing.Size(75, 23);
            this.button_queryLines_load.TabIndex = 16;
            this.button_queryLines_load.Text = "装载";
            this.button_queryLines_load.UseVisualStyleBackColor = true;
            this.button_queryLines_load.Click += new System.EventHandler(this.button_queryLines_load_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 5);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 12);
            this.label4.TabIndex = 13;
            this.label4.Text = "检索词文件:";
            // 
            // textBox_queryLines_filename
            // 
            this.textBox_queryLines_filename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_queryLines_filename.Location = new System.Drawing.Point(84, 2);
            this.textBox_queryLines_filename.Name = "textBox_queryLines_filename";
            this.textBox_queryLines_filename.Size = new System.Drawing.Size(251, 21);
            this.textBox_queryLines_filename.TabIndex = 14;
            // 
            // textBox_queryLines_content
            // 
            this.textBox_queryLines_content.AcceptsReturn = true;
            this.textBox_queryLines_content.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_queryLines_content.Location = new System.Drawing.Point(3, 34);
            this.textBox_queryLines_content.MaxLength = 1048576;
            this.textBox_queryLines_content.Multiline = true;
            this.textBox_queryLines_content.Name = "textBox_queryLines_content";
            this.textBox_queryLines_content.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_queryLines_content.Size = new System.Drawing.Size(416, 103);
            this.textBox_queryLines_content.TabIndex = 17;
            // 
            // tabPage_features
            // 
            this.tabPage_features.AutoScroll = true;
            this.tabPage_features.Controls.Add(this.numericUpDown_features_oneWordMaxHitCount);
            this.tabPage_features.Controls.Add(this.label8);
            this.tabPage_features.Controls.Add(this.comboBox_features_elementSetName);
            this.tabPage_features.Controls.Add(this.label7);
            this.tabPage_features.Controls.Add(this.comboBox_features_syntax);
            this.tabPage_features.Controls.Add(this.label6);
            this.tabPage_features.Controls.Add(this.comboBox_features_from);
            this.tabPage_features.Controls.Add(this.label5);
            this.tabPage_features.Location = new System.Drawing.Point(4, 22);
            this.tabPage_features.Name = "tabPage_features";
            this.tabPage_features.Size = new System.Drawing.Size(428, 146);
            this.tabPage_features.TabIndex = 2;
            this.tabPage_features.Text = "特性";
            this.tabPage_features.UseVisualStyleBackColor = true;
            // 
            // numericUpDown_features_oneWordMaxHitCount
            // 
            this.numericUpDown_features_oneWordMaxHitCount.Location = new System.Drawing.Point(137, 98);
            this.numericUpDown_features_oneWordMaxHitCount.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericUpDown_features_oneWordMaxHitCount.Minimum = new decimal(new int[] {
            1,
            0,
            0,
            0});
            this.numericUpDown_features_oneWordMaxHitCount.Name = "numericUpDown_features_oneWordMaxHitCount";
            this.numericUpDown_features_oneWordMaxHitCount.Size = new System.Drawing.Size(120, 21);
            this.numericUpDown_features_oneWordMaxHitCount.TabIndex = 7;
            this.numericUpDown_features_oneWordMaxHitCount.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // label8
            // 
            this.label8.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(6, 100);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(125, 12);
            this.label8.TabIndex = 6;
            this.label8.Text = "一词命中最多条数(&M):";
            this.label8.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboBox_features_elementSetName
            // 
            this.comboBox_features_elementSetName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_features_elementSetName.DropDownHeight = 300;
            this.comboBox_features_elementSetName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_features_elementSetName.FormattingEnabled = true;
            this.comboBox_features_elementSetName.IntegralHeight = false;
            this.comboBox_features_elementSetName.Items.AddRange(new object[] {
            "B -- Brief(MARC records)",
            "F -- Full (MARC and OPAC records)",
            "dc --  Dublin Core (XML records)",
            "mods  -- MODS (XML records)",
            "marcxml -- MARCXML (XML records), default schema for XML",
            "opacxml -- MARCXML with holdings attached"});
            this.comboBox_features_elementSetName.Location = new System.Drawing.Point(89, 72);
            this.comboBox_features_elementSetName.Name = "comboBox_features_elementSetName";
            this.comboBox_features_elementSetName.Size = new System.Drawing.Size(334, 20);
            this.comboBox_features_elementSetName.TabIndex = 5;
            this.comboBox_features_elementSetName.SizeChanged += new System.EventHandler(this.comboBox_features_from_SizeChanged);
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(6, 75);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(77, 12);
            this.label7.TabIndex = 4;
            this.label7.Text = "元素集名(&E):";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboBox_features_syntax
            // 
            this.comboBox_features_syntax.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_features_syntax.DropDownHeight = 300;
            this.comboBox_features_syntax.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_features_syntax.FormattingEnabled = true;
            this.comboBox_features_syntax.IntegralHeight = false;
            this.comboBox_features_syntax.Items.AddRange(new object[] {
            "1.2.840.10003.5.1 -- UNIMARC",
            "1.2.840.10003.5.10 -- MARC21",
            "1.2.840.10003.5.101 -- SUTRS",
            "1.2.840.10003.5.109.10 -- XML"});
            this.comboBox_features_syntax.Location = new System.Drawing.Point(89, 46);
            this.comboBox_features_syntax.Name = "comboBox_features_syntax";
            this.comboBox_features_syntax.Size = new System.Drawing.Size(334, 20);
            this.comboBox_features_syntax.TabIndex = 3;
            this.comboBox_features_syntax.SizeChanged += new System.EventHandler(this.comboBox_features_from_SizeChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 49);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(77, 12);
            this.label6.TabIndex = 2;
            this.label6.Text = "数据格式(&S):";
            // 
            // comboBox_features_from
            // 
            this.comboBox_features_from.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_features_from.DropDownHeight = 300;
            this.comboBox_features_from.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_features_from.FormattingEnabled = true;
            this.comboBox_features_from.IntegralHeight = false;
            this.comboBox_features_from.Location = new System.Drawing.Point(89, 20);
            this.comboBox_features_from.Name = "comboBox_features_from";
            this.comboBox_features_from.Size = new System.Drawing.Size(334, 20);
            this.comboBox_features_from.TabIndex = 1;
            this.comboBox_features_from.SizeChanged += new System.EventHandler(this.comboBox_features_from_SizeChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 23);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "检索途径(&U):";
            // 
            // tabPage_search
            // 
            this.tabPage_search.Controls.Add(this.dpTable_queryWords);
            this.tabPage_search.Location = new System.Drawing.Point(4, 22);
            this.tabPage_search.Name = "tabPage_search";
            this.tabPage_search.Size = new System.Drawing.Size(428, 146);
            this.tabPage_search.TabIndex = 3;
            this.tabPage_search.Text = "执行检索";
            this.tabPage_search.UseVisualStyleBackColor = true;
            // 
            // dpTable_queryWords
            // 
            this.dpTable_queryWords.AutoDocCenter = true;
            this.dpTable_queryWords.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dpTable_queryWords.Columns.Add(this.dpColumn9);
            this.dpTable_queryWords.Columns.Add(this.dpColumn6);
            this.dpTable_queryWords.Columns.Add(this.dpColumn7);
            this.dpTable_queryWords.Columns.Add(this.dpColumn8);
            this.dpTable_queryWords.Columns.Add(this.dpColumn10);
            this.dpTable_queryWords.ColumnsBackColor = System.Drawing.SystemColors.Control;
            this.dpTable_queryWords.ColumnsForeColor = System.Drawing.SystemColors.ControlText;
            this.dpTable_queryWords.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dpTable_queryWords.DocumentBorderColor = System.Drawing.SystemColors.ControlDark;
            this.dpTable_queryWords.DocumentOrgX = ((long)(0));
            this.dpTable_queryWords.DocumentOrgY = ((long)(0));
            this.dpTable_queryWords.DocumentShadowColor = System.Drawing.SystemColors.ControlDarkDark;
            this.dpTable_queryWords.FocusedItem = null;
            this.dpTable_queryWords.FullRowSelect = true;
            this.dpTable_queryWords.HighlightBackColor = System.Drawing.SystemColors.Highlight;
            this.dpTable_queryWords.HightlightForeColor = System.Drawing.SystemColors.HighlightText;
            this.dpTable_queryWords.HoverBackColor = System.Drawing.SystemColors.HotTrack;
            this.dpTable_queryWords.InactiveHighlightBackColor = System.Drawing.SystemColors.InactiveCaption;
            this.dpTable_queryWords.InactiveHightlightForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.dpTable_queryWords.Location = new System.Drawing.Point(0, 0);
            this.dpTable_queryWords.Name = "dpTable_queryWords";
            this.dpTable_queryWords.Padding = new System.Windows.Forms.Padding(8);
            this.dpTable_queryWords.Size = new System.Drawing.Size(428, 146);
            this.dpTable_queryWords.TabIndex = 0;
            this.dpTable_queryWords.Text = "dpTable2";
            this.dpTable_queryWords.DoubleClick += new System.EventHandler(this.dpTable_queryWords_DoubleClick);
            this.dpTable_queryWords.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dpTable_queryWords_MouseUp);
            // 
            // dpColumn9
            // 
            this.dpColumn9.Alignment = System.Drawing.StringAlignment.Far;
            this.dpColumn9.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn9.Font = null;
            this.dpColumn9.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn9.Text = "序号";
            this.dpColumn9.Width = 50;
            // 
            // dpColumn6
            // 
            this.dpColumn6.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn6.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn6.Font = null;
            this.dpColumn6.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn6.Text = "检索词";
            this.dpColumn6.Width = 200;
            // 
            // dpColumn7
            // 
            this.dpColumn7.Alignment = System.Drawing.StringAlignment.Far;
            this.dpColumn7.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn7.Font = null;
            this.dpColumn7.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn7.Text = "命中数";
            // 
            // dpColumn8
            // 
            this.dpColumn8.Alignment = System.Drawing.StringAlignment.Far;
            this.dpColumn8.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn8.Font = null;
            this.dpColumn8.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn8.Text = "取出数";
            this.dpColumn8.Width = 70;
            // 
            // dpColumn10
            // 
            this.dpColumn10.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn10.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn10.Font = null;
            this.dpColumn10.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn10.Text = "出错信息";
            // 
            // tabPage_saveResults
            // 
            this.tabPage_saveResults.AutoScroll = true;
            this.tabPage_saveResults.Controls.Add(this.textBox_saveResult_notHitFilename);
            this.tabPage_saveResults.Controls.Add(this.button_saveResult_saveNotHitFile);
            this.tabPage_saveResults.Controls.Add(this.button_saveResult_findNotHitFilename);
            this.tabPage_saveResults.Controls.Add(this.label3);
            this.tabPage_saveResults.Controls.Add(this.groupBox1);
            this.tabPage_saveResults.Location = new System.Drawing.Point(4, 22);
            this.tabPage_saveResults.Name = "tabPage_saveResults";
            this.tabPage_saveResults.Size = new System.Drawing.Size(428, 146);
            this.tabPage_saveResults.TabIndex = 4;
            this.tabPage_saveResults.Text = "保存结果";
            this.tabPage_saveResults.UseVisualStyleBackColor = true;
            // 
            // textBox_saveResult_notHitFilename
            // 
            this.textBox_saveResult_notHitFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_saveResult_notHitFilename.Location = new System.Drawing.Point(106, 104);
            this.textBox_saveResult_notHitFilename.Name = "textBox_saveResult_notHitFilename";
            this.textBox_saveResult_notHitFilename.Size = new System.Drawing.Size(173, 21);
            this.textBox_saveResult_notHitFilename.TabIndex = 10;
            // 
            // button_saveResult_saveNotHitFile
            // 
            this.button_saveResult_saveNotHitFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_saveResult_saveNotHitFile.Location = new System.Drawing.Point(337, 102);
            this.button_saveResult_saveNotHitFile.Name = "button_saveResult_saveNotHitFile";
            this.button_saveResult_saveNotHitFile.Size = new System.Drawing.Size(75, 23);
            this.button_saveResult_saveNotHitFile.TabIndex = 12;
            this.button_saveResult_saveNotHitFile.Text = "保存";
            this.button_saveResult_saveNotHitFile.UseVisualStyleBackColor = true;
            this.button_saveResult_saveNotHitFile.Click += new System.EventHandler(this.button_saveResult_saveNotHitFile_Click);
            // 
            // button_saveResult_findNotHitFilename
            // 
            this.button_saveResult_findNotHitFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_saveResult_findNotHitFilename.Location = new System.Drawing.Point(285, 102);
            this.button_saveResult_findNotHitFilename.Name = "button_saveResult_findNotHitFilename";
            this.button_saveResult_findNotHitFilename.Size = new System.Drawing.Size(46, 23);
            this.button_saveResult_findNotHitFilename.TabIndex = 11;
            this.button_saveResult_findNotHitFilename.Text = "...";
            this.button_saveResult_findNotHitFilename.UseVisualStyleBackColor = true;
            this.button_saveResult_findNotHitFilename.Click += new System.EventHandler(this.button_saveResult_findNotHitFilename_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(5, 107);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(95, 12);
            this.label3.TabIndex = 9;
            this.label3.Text = "未命中的检索词:";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.textBox_saveResult_multiHitFilename);
            this.groupBox1.Controls.Add(this.button_saveResult_saveMultiHitFile);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.button_saveResult_findMultiHitFilename);
            this.groupBox1.Controls.Add(this.textBox_saveResult_singleHitFilename);
            this.groupBox1.Controls.Add(this.button_saveResult_findSingleHitFilename);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.button_saveResult_saveSingleHitFile);
            this.groupBox1.Location = new System.Drawing.Point(6, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(417, 84);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "MARC文件";
            // 
            // textBox_saveResult_multiHitFilename
            // 
            this.textBox_saveResult_multiHitFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_saveResult_multiHitFilename.Location = new System.Drawing.Point(91, 48);
            this.textBox_saveResult_multiHitFilename.Name = "textBox_saveResult_multiHitFilename";
            this.textBox_saveResult_multiHitFilename.Size = new System.Drawing.Size(182, 21);
            this.textBox_saveResult_multiHitFilename.TabIndex = 5;
            // 
            // button_saveResult_saveMultiHitFile
            // 
            this.button_saveResult_saveMultiHitFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_saveResult_saveMultiHitFile.Location = new System.Drawing.Point(331, 46);
            this.button_saveResult_saveMultiHitFile.Name = "button_saveResult_saveMultiHitFile";
            this.button_saveResult_saveMultiHitFile.Size = new System.Drawing.Size(75, 23);
            this.button_saveResult_saveMultiHitFile.TabIndex = 7;
            this.button_saveResult_saveMultiHitFile.Text = "保存";
            this.button_saveResult_saveMultiHitFile.UseVisualStyleBackColor = true;
            this.button_saveResult_saveMultiHitFile.Click += new System.EventHandler(this.button_saveResult_saveMultiHitFile_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(14, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "命中唯一的:";
            // 
            // button_saveResult_findMultiHitFilename
            // 
            this.button_saveResult_findMultiHitFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_saveResult_findMultiHitFilename.Location = new System.Drawing.Point(279, 46);
            this.button_saveResult_findMultiHitFilename.Name = "button_saveResult_findMultiHitFilename";
            this.button_saveResult_findMultiHitFilename.Size = new System.Drawing.Size(46, 23);
            this.button_saveResult_findMultiHitFilename.TabIndex = 6;
            this.button_saveResult_findMultiHitFilename.Text = "...";
            this.button_saveResult_findMultiHitFilename.UseVisualStyleBackColor = true;
            this.button_saveResult_findMultiHitFilename.Click += new System.EventHandler(this.button_saveResult_findMultiHitFilename_Click);
            // 
            // textBox_saveResult_singleHitFilename
            // 
            this.textBox_saveResult_singleHitFilename.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_saveResult_singleHitFilename.Location = new System.Drawing.Point(91, 21);
            this.textBox_saveResult_singleHitFilename.Name = "textBox_saveResult_singleHitFilename";
            this.textBox_saveResult_singleHitFilename.Size = new System.Drawing.Size(182, 21);
            this.textBox_saveResult_singleHitFilename.TabIndex = 1;
            // 
            // button_saveResult_findSingleHitFilename
            // 
            this.button_saveResult_findSingleHitFilename.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_saveResult_findSingleHitFilename.Location = new System.Drawing.Point(279, 19);
            this.button_saveResult_findSingleHitFilename.Name = "button_saveResult_findSingleHitFilename";
            this.button_saveResult_findSingleHitFilename.Size = new System.Drawing.Size(46, 23);
            this.button_saveResult_findSingleHitFilename.TabIndex = 2;
            this.button_saveResult_findSingleHitFilename.Text = "...";
            this.button_saveResult_findSingleHitFilename.UseVisualStyleBackColor = true;
            this.button_saveResult_findSingleHitFilename.Click += new System.EventHandler(this.button_saveResult_findSingleHitFilename_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "命中多条的:";
            // 
            // button_saveResult_saveSingleHitFile
            // 
            this.button_saveResult_saveSingleHitFile.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_saveResult_saveSingleHitFile.Location = new System.Drawing.Point(331, 19);
            this.button_saveResult_saveSingleHitFile.Name = "button_saveResult_saveSingleHitFile";
            this.button_saveResult_saveSingleHitFile.Size = new System.Drawing.Size(75, 23);
            this.button_saveResult_saveSingleHitFile.TabIndex = 3;
            this.button_saveResult_saveSingleHitFile.Text = "保存";
            this.button_saveResult_saveSingleHitFile.UseVisualStyleBackColor = true;
            this.button_saveResult_saveSingleHitFile.Click += new System.EventHandler(this.button_saveResult_saveSingleHitFile_Click);
            // 
            // button_next
            // 
            this.button_next.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_next.Location = new System.Drawing.Point(364, 283);
            this.button_next.Name = "button_next";
            this.button_next.Size = new System.Drawing.Size(75, 23);
            this.button_next.TabIndex = 1;
            this.button_next.Text = "下一步";
            this.button_next.UseVisualStyleBackColor = true;
            this.button_next.Click += new System.EventHandler(this.button_next_Click);
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_main.Location = new System.Drawing.Point(3, 13);
            this.splitContainer_main.Name = "splitContainer_main";
            this.splitContainer_main.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.tabControl_steps);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.dpTable_records);
            this.splitContainer_main.Size = new System.Drawing.Size(436, 264);
            this.splitContainer_main.SplitterDistance = 172;
            this.splitContainer_main.SplitterWidth = 8;
            this.splitContainer_main.TabIndex = 2;
            // 
            // dpTable_records
            // 
            this.dpTable_records.AutoDocCenter = true;
            this.dpTable_records.BackColor = System.Drawing.SystemColors.Window;
            this.dpTable_records.Columns.Add(this.dpColumn1);
            this.dpTable_records.Columns.Add(this.dpColumn2);
            this.dpTable_records.Columns.Add(this.dpColumn3);
            this.dpTable_records.Columns.Add(this.dpColumn4);
            this.dpTable_records.Columns.Add(this.dpColumn5);
            this.dpTable_records.ColumnsBackColor = System.Drawing.SystemColors.Control;
            this.dpTable_records.ColumnsForeColor = System.Drawing.SystemColors.ControlText;
            this.dpTable_records.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dpTable_records.DocumentBorderColor = System.Drawing.SystemColors.ControlDark;
            this.dpTable_records.DocumentMargin = new System.Windows.Forms.Padding(8);
            this.dpTable_records.DocumentOrgX = ((long)(0));
            this.dpTable_records.DocumentOrgY = ((long)(0));
            this.dpTable_records.DocumentShadowColor = System.Drawing.SystemColors.ControlDarkDark;
            this.dpTable_records.FocusedItem = null;
            this.dpTable_records.FullRowSelect = true;
            this.dpTable_records.HighlightBackColor = System.Drawing.SystemColors.Highlight;
            this.dpTable_records.HightlightForeColor = System.Drawing.SystemColors.HighlightText;
            this.dpTable_records.HoverBackColor = System.Drawing.SystemColors.HotTrack;
            this.dpTable_records.InactiveHighlightBackColor = System.Drawing.SystemColors.InactiveCaption;
            this.dpTable_records.InactiveHightlightForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.dpTable_records.Location = new System.Drawing.Point(0, 0);
            this.dpTable_records.Name = "dpTable_records";
            this.dpTable_records.Padding = new System.Windows.Forms.Padding(16);
            this.dpTable_records.Size = new System.Drawing.Size(436, 84);
            this.dpTable_records.TabIndex = 0;
            this.dpTable_records.Text = "dpTable1";
            this.dpTable_records.DoubleClick += new System.EventHandler(this.dpTable_records_DoubleClick);
            this.dpTable_records.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dpTable_records_MouseUp);
            // 
            // dpColumn1
            // 
            this.dpColumn1.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn1.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn1.Font = null;
            this.dpColumn1.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn1.Text = "ID";
            // 
            // dpColumn2
            // 
            this.dpColumn2.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn2.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn2.Font = null;
            this.dpColumn2.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn2.Text = "题名";
            // 
            // dpColumn3
            // 
            this.dpColumn3.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn3.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn3.Font = null;
            this.dpColumn3.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn3.Text = "责任者";
            // 
            // dpColumn4
            // 
            this.dpColumn4.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn4.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn4.Font = null;
            this.dpColumn4.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn4.Text = "出版者";
            // 
            // dpColumn5
            // 
            this.dpColumn5.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn5.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn5.Font = null;
            this.dpColumn5.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn5.Text = "出版日期";
            // 
            // tableLayoutPanel_main
            // 
            this.tableLayoutPanel_main.ColumnCount = 1;
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.Controls.Add(this.splitContainer_main, 0, 0);
            this.tableLayoutPanel_main.Controls.Add(this.button_next, 0, 1);
            this.tableLayoutPanel_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_main.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_main.Name = "tableLayoutPanel_main";
            this.tableLayoutPanel_main.Padding = new System.Windows.Forms.Padding(0, 10, 0, 10);
            this.tableLayoutPanel_main.RowCount = 2;
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.Size = new System.Drawing.Size(442, 319);
            this.tableLayoutPanel_main.TabIndex = 3;
            // 
            // imageList_browseItemType
            // 
            this.imageList_browseItemType.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_browseItemType.ImageStream")));
            this.imageList_browseItemType.TransparentColor = System.Drawing.Color.Fuchsia;
            this.imageList_browseItemType.Images.SetKeyName(0, "file.bmp");
            this.imageList_browseItemType.Images.SetKeyName(1, "error.bmp");
            this.imageList_browseItemType.Images.SetKeyName(2, "brieftype.bmp");
            this.imageList_browseItemType.Images.SetKeyName(3, "fulltype.bmp");
            this.imageList_browseItemType.Images.SetKeyName(4, "diagtype.bmp");
            // 
            // imageList_queryWords
            // 
            this.imageList_queryWords.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList_queryWords.ImageStream")));
            this.imageList_queryWords.TransparentColor = System.Drawing.Color.Fuchsia;
            this.imageList_queryWords.Images.SetKeyName(0, "notfound.bmp");
            this.imageList_queryWords.Images.SetKeyName(1, "found.bmp");
            this.imageList_queryWords.Images.SetKeyName(2, "Error.bmp");
            // 
            // ZBatchSearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(442, 319);
            this.Controls.Add(this.tableLayoutPanel_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ZBatchSearchForm";
            this.ShowInTaskbar = false;
            this.Text = "Z39.50批检索";
            this.Activated += new System.EventHandler(this.ZBatchSearchForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ZBatchSearchForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ZBatchSearchForm_FormClosed);
            this.Load += new System.EventHandler(this.ZBatchSearchForm_Load);
            this.tabControl_steps.ResumeLayout(false);
            this.tabPage_target.ResumeLayout(false);
            this.tabPage_queryLines.ResumeLayout(false);
            this.tableLayoutPanel_queryLines.ResumeLayout(false);
            this.tableLayoutPanel_queryLines.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabPage_features.ResumeLayout(false);
            this.tabPage_features.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown_features_oneWordMaxHitCount)).EndInit();
            this.tabPage_search.ResumeLayout(false);
            this.tabPage_saveResults.ResumeLayout(false);
            this.tabPage_saveResults.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.tableLayoutPanel_main.ResumeLayout(false);
            this.ResumeLayout(false);

		}

		#endregion

        private System.Windows.Forms.TabControl tabControl_steps;
        private System.Windows.Forms.TabPage tabPage_target;
        private System.Windows.Forms.TabPage tabPage_queryLines;
        private System.Windows.Forms.Button button_next;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_main;
        private DigitalPlatform.CommonControl.DpTable dpTable_records;
        private System.Windows.Forms.TabPage tabPage_features;
        private System.Windows.Forms.TabPage tabPage_search;
        private System.Windows.Forms.TabPage tabPage_saveResults;
        private System.Windows.Forms.Button button_saveResult_saveSingleHitFile;
        private System.Windows.Forms.Button button_saveResult_findSingleHitFilename;
        private System.Windows.Forms.TextBox textBox_saveResult_singleHitFilename;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_saveResult_saveMultiHitFile;
        private System.Windows.Forms.Button button_saveResult_findMultiHitFilename;
        private System.Windows.Forms.TextBox textBox_saveResult_multiHitFilename;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox_saveResult_notHitFilename;
        private System.Windows.Forms.Button button_saveResult_saveNotHitFile;
        private System.Windows.Forms.Button button_saveResult_findNotHitFilename;
        private System.Windows.Forms.Label label3;
        private ZTargetControl zTargetControl1;
        private System.Windows.Forms.TextBox textBox_queryLines_filename;
        private System.Windows.Forms.Button button_queryLines_load;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_queryLines_content;
        private System.Windows.Forms.ComboBox comboBox_features_syntax;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboBox_features_from;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBox_features_elementSetName;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.NumericUpDown numericUpDown_features_oneWordMaxHitCount;
        private System.Windows.Forms.Label label8;
        private DigitalPlatform.CommonControl.DpColumn dpColumn1;
        private DigitalPlatform.CommonControl.DpColumn dpColumn2;
        private DigitalPlatform.CommonControl.DpColumn dpColumn3;
        private DigitalPlatform.CommonControl.DpColumn dpColumn4;
        private DigitalPlatform.CommonControl.DpColumn dpColumn5;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_queryLines;
        private System.Windows.Forms.ImageList imageList_browseItemType;
        private DigitalPlatform.CommonControl.DpTable dpTable_queryWords;
        private DigitalPlatform.CommonControl.DpColumn dpColumn6;
        private DigitalPlatform.CommonControl.DpColumn dpColumn7;
        private DigitalPlatform.CommonControl.DpColumn dpColumn8;
        private System.Windows.Forms.ImageList imageList_queryWords;
        private DigitalPlatform.CommonControl.DpColumn dpColumn9;
        private DigitalPlatform.CommonControl.DpColumn dpColumn10;
	}
}