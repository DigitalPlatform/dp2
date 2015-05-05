namespace dp2Circulation
{
    partial class ReaderSearchForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ReaderSearchForm));
            this.textBox_queryWord = new System.Windows.Forms.TextBox();
            this.comboBox_from = new System.Windows.Forms.ComboBox();
            this.listView_records = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_path = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.comboBox_readerDbName = new System.Windows.Forms.ComboBox();
            this.label_message = new System.Windows.Forms.Label();
            this.tableLayoutPanel_main = new System.Windows.Forms.TableLayoutPanel();
            this.tableLayoutPanel_query = new System.Windows.Forms.TableLayoutPanel();
            this.label_from = new System.Windows.Forms.Label();
            this.label_readerDbName = new System.Windows.Forms.Label();
            this.label_queryWord = new System.Windows.Forms.Label();
            this.label_matchStyle = new System.Windows.Forms.Label();
            this.comboBox_matchStyle = new System.Windows.Forms.ComboBox();
            this.toolStrip_search = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_search = new System.Windows.Forms.ToolStripButton();
            this.toolStripDropDownButton_inputTimeString = new System.Windows.Forms.ToolStripDropDownButton();
            this.ToolStripMenuItem_rfc1123Single = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_uSingle = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItem_rfc1123Range = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_uRange = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.ToolStripMenuItem_initFingerprintCache = new System.Windows.Forms.ToolStripMenuItem();
            this.tableLayoutPanel_main.SuspendLayout();
            this.tableLayoutPanel_query.SuspendLayout();
            this.toolStrip_search.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox_queryWord
            // 
            this.textBox_queryWord.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_queryWord.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_queryWord.Location = new System.Drawing.Point(86, 3);
            this.textBox_queryWord.Name = "textBox_queryWord";
            this.textBox_queryWord.Size = new System.Drawing.Size(192, 21);
            this.textBox_queryWord.TabIndex = 1;
            this.textBox_queryWord.TextChanged += new System.EventHandler(this.textBox_queryWord_TextChanged);
            this.textBox_queryWord.Enter += new System.EventHandler(this.textBox_queryWord_Enter);
            this.textBox_queryWord.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBox_queryWord_KeyPress);
            // 
            // comboBox_from
            // 
            this.comboBox_from.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_from.DropDownHeight = 300;
            this.comboBox_from.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_from.FormattingEnabled = true;
            this.comboBox_from.IntegralHeight = false;
            this.comboBox_from.Items.AddRange(new object[] {
            "证条码",
            "姓名",
            "单位",
            "身份证号",
            "姓名生日",
            "Email",
            "电话",
            "所借册条码",
            "失效日期",
            "状态",
            "__id"});
            this.comboBox_from.Location = new System.Drawing.Point(86, 54);
            this.comboBox_from.Name = "comboBox_from";
            this.comboBox_from.Size = new System.Drawing.Size(192, 20);
            this.comboBox_from.TabIndex = 6;
            this.comboBox_from.Text = "证条码";
            this.comboBox_from.SizeChanged += new System.EventHandler(this.comboBox_from_SizeChanged);
            // 
            // listView_records
            // 
            this.listView_records.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_path,
            this.columnHeader_1});
            this.listView_records.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_records.FullRowSelect = true;
            this.listView_records.HideSelection = false;
            this.listView_records.Location = new System.Drawing.Point(0, 113);
            this.listView_records.Margin = new System.Windows.Forms.Padding(0);
            this.listView_records.Name = "listView_records";
            this.listView_records.Size = new System.Drawing.Size(367, 137);
            this.listView_records.TabIndex = 0;
            this.listView_records.UseCompatibleStateImageBehavior = false;
            this.listView_records.View = System.Windows.Forms.View.Details;
            this.listView_records.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_records_ColumnClick);
            this.listView_records.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.listView_records_ItemDrag);
            this.listView_records.SelectedIndexChanged += new System.EventHandler(this.listView_records_SelectedIndexChanged);
            this.listView_records.DoubleClick += new System.EventHandler(this.listView_records_DoubleClick);
            this.listView_records.Enter += new System.EventHandler(this.listView_records_Enter);
            this.listView_records.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_records_MouseUp);
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
            // comboBox_readerDbName
            // 
            this.comboBox_readerDbName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_readerDbName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_readerDbName.FormattingEnabled = true;
            this.comboBox_readerDbName.Location = new System.Drawing.Point(85, 29);
            this.comboBox_readerDbName.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_readerDbName.Name = "comboBox_readerDbName";
            this.comboBox_readerDbName.Size = new System.Drawing.Size(194, 20);
            this.comboBox_readerDbName.TabIndex = 4;
            this.comboBox_readerDbName.DropDown += new System.EventHandler(this.comboBox_readerDbName_DropDown);
            this.comboBox_readerDbName.SizeChanged += new System.EventHandler(this.comboBox_readerDbName_SizeChanged);
            // 
            // label_message
            // 
            this.label_message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_message.Location = new System.Drawing.Point(0, 252);
            this.label_message.Margin = new System.Windows.Forms.Padding(0, 2, 0, 0);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(367, 14);
            this.label_message.TabIndex = 1;
            // 
            // tableLayoutPanel_main
            // 
            this.tableLayoutPanel_main.ColumnCount = 1;
            this.tableLayoutPanel_main.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.Controls.Add(this.tableLayoutPanel_query, 0, 0);
            this.tableLayoutPanel_main.Controls.Add(this.listView_records, 0, 1);
            this.tableLayoutPanel_main.Controls.Add(this.label_message, 0, 2);
            this.tableLayoutPanel_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_main.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_main.Margin = new System.Windows.Forms.Padding(2);
            this.tableLayoutPanel_main.Name = "tableLayoutPanel_main";
            this.tableLayoutPanel_main.Padding = new System.Windows.Forms.Padding(0, 10, 0, 10);
            this.tableLayoutPanel_main.RowCount = 3;
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_main.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16F));
            this.tableLayoutPanel_main.Size = new System.Drawing.Size(367, 276);
            this.tableLayoutPanel_main.TabIndex = 0;
            // 
            // tableLayoutPanel_query
            // 
            this.tableLayoutPanel_query.AutoSize = true;
            this.tableLayoutPanel_query.ColumnCount = 3;
            this.tableLayoutPanel_query.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_query.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_query.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_query.Controls.Add(this.label_from, 0, 2);
            this.tableLayoutPanel_query.Controls.Add(this.comboBox_readerDbName, 1, 1);
            this.tableLayoutPanel_query.Controls.Add(this.label_readerDbName, 0, 1);
            this.tableLayoutPanel_query.Controls.Add(this.label_queryWord, 0, 0);
            this.tableLayoutPanel_query.Controls.Add(this.comboBox_from, 1, 2);
            this.tableLayoutPanel_query.Controls.Add(this.label_matchStyle, 0, 3);
            this.tableLayoutPanel_query.Controls.Add(this.comboBox_matchStyle, 1, 3);
            this.tableLayoutPanel_query.Controls.Add(this.textBox_queryWord, 1, 0);
            this.tableLayoutPanel_query.Controls.Add(this.toolStrip_search, 2, 0);
            this.tableLayoutPanel_query.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_query.Location = new System.Drawing.Point(0, 10);
            this.tableLayoutPanel_query.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_query.MaximumSize = new System.Drawing.Size(375, 0);
            this.tableLayoutPanel_query.Name = "tableLayoutPanel_query";
            this.tableLayoutPanel_query.RowCount = 4;
            this.tableLayoutPanel_query.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_query.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_query.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_query.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_query.Size = new System.Drawing.Size(367, 103);
            this.tableLayoutPanel_query.TabIndex = 8;
            // 
            // label_from
            // 
            this.label_from.AutoSize = true;
            this.label_from.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_from.Location = new System.Drawing.Point(3, 51);
            this.label_from.Name = "label_from";
            this.label_from.Size = new System.Drawing.Size(77, 26);
            this.label_from.TabIndex = 5;
            this.label_from.Text = "检索途径(&F):";
            this.label_from.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label_readerDbName
            // 
            this.label_readerDbName.AutoSize = true;
            this.label_readerDbName.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_readerDbName.Location = new System.Drawing.Point(2, 27);
            this.label_readerDbName.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_readerDbName.Name = "label_readerDbName";
            this.label_readerDbName.Size = new System.Drawing.Size(79, 24);
            this.label_readerDbName.TabIndex = 3;
            this.label_readerDbName.Text = "读者库(&D):";
            this.label_readerDbName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label_queryWord
            // 
            this.label_queryWord.AutoSize = true;
            this.label_queryWord.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_queryWord.ImageAlign = System.Drawing.ContentAlignment.TopLeft;
            this.label_queryWord.Location = new System.Drawing.Point(3, 0);
            this.label_queryWord.Name = "label_queryWord";
            this.label_queryWord.Size = new System.Drawing.Size(77, 27);
            this.label_queryWord.TabIndex = 0;
            this.label_queryWord.Text = "检索词(&W):";
            this.label_queryWord.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label_matchStyle
            // 
            this.label_matchStyle.AutoSize = true;
            this.label_matchStyle.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_matchStyle.Location = new System.Drawing.Point(3, 77);
            this.label_matchStyle.Name = "label_matchStyle";
            this.label_matchStyle.Size = new System.Drawing.Size(77, 26);
            this.label_matchStyle.TabIndex = 7;
            this.label_matchStyle.Text = "匹配方式(&M):";
            this.label_matchStyle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboBox_matchStyle
            // 
            this.comboBox_matchStyle.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_matchStyle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_matchStyle.FormattingEnabled = true;
            this.comboBox_matchStyle.Items.AddRange(new object[] {
            "前方一致",
            "中间一致",
            "后方一致",
            "精确一致",
            "空值"});
            this.comboBox_matchStyle.Location = new System.Drawing.Point(86, 80);
            this.comboBox_matchStyle.Name = "comboBox_matchStyle";
            this.comboBox_matchStyle.Size = new System.Drawing.Size(192, 20);
            this.comboBox_matchStyle.TabIndex = 8;
            this.comboBox_matchStyle.Text = "前方一致";
            this.comboBox_matchStyle.SizeChanged += new System.EventHandler(this.comboBox_matchStyle_SizeChanged);
            this.comboBox_matchStyle.TextChanged += new System.EventHandler(this.comboBox_matchStyle_TextChanged);
            // 
            // toolStrip_search
            // 
            this.toolStrip_search.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip_search.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_search,
            this.toolStripDropDownButton_inputTimeString});
            this.toolStrip_search.Location = new System.Drawing.Point(281, 0);
            this.toolStrip_search.Name = "toolStrip_search";
            this.toolStrip_search.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.toolStrip_search.Size = new System.Drawing.Size(86, 25);
            this.toolStrip_search.TabIndex = 9;
            this.toolStrip_search.Text = "检索";
            // 
            // toolStripButton_search
            // 
            this.toolStripButton_search.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_search.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_search.Image")));
            this.toolStripButton_search.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_search.Name = "toolStripButton_search";
            this.toolStripButton_search.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_search.Text = "检索";
            this.toolStripButton_search.Click += new System.EventHandler(this.toolStripButton_search_Click);
            // 
            // toolStripDropDownButton_inputTimeString
            // 
            this.toolStripDropDownButton_inputTimeString.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripDropDownButton_inputTimeString.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ToolStripMenuItem_rfc1123Single,
            this.ToolStripMenuItem_uSingle,
            this.toolStripSeparator1,
            this.ToolStripMenuItem_rfc1123Range,
            this.ToolStripMenuItem_uRange,
            this.toolStripSeparator2,
            this.ToolStripMenuItem_initFingerprintCache});
            this.toolStripDropDownButton_inputTimeString.Image = ((System.Drawing.Image)(resources.GetObject("toolStripDropDownButton_inputTimeString.Image")));
            this.toolStripDropDownButton_inputTimeString.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripDropDownButton_inputTimeString.Name = "toolStripDropDownButton_inputTimeString";
            this.toolStripDropDownButton_inputTimeString.Size = new System.Drawing.Size(29, 22);
            this.toolStripDropDownButton_inputTimeString.Text = "和时间有关的功能";
            // 
            // ToolStripMenuItem_rfc1123Single
            // 
            this.ToolStripMenuItem_rfc1123Single.Name = "ToolStripMenuItem_rfc1123Single";
            this.ToolStripMenuItem_rfc1123Single.Size = new System.Drawing.Size(217, 22);
            this.ToolStripMenuItem_rfc1123Single.Text = "RFC1123时间值...";
            this.ToolStripMenuItem_rfc1123Single.Click += new System.EventHandler(this.ToolStripMenuItem_rfc1123Single_Click);
            // 
            // ToolStripMenuItem_uSingle
            // 
            this.ToolStripMenuItem_uSingle.Name = "ToolStripMenuItem_uSingle";
            this.ToolStripMenuItem_uSingle.Size = new System.Drawing.Size(217, 22);
            this.ToolStripMenuItem_uSingle.Text = "u时间值...";
            this.ToolStripMenuItem_uSingle.Click += new System.EventHandler(this.ToolStripMenuItem_uSingle_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(214, 6);
            // 
            // ToolStripMenuItem_rfc1123Range
            // 
            this.ToolStripMenuItem_rfc1123Range.Name = "ToolStripMenuItem_rfc1123Range";
            this.ToolStripMenuItem_rfc1123Range.Size = new System.Drawing.Size(217, 22);
            this.ToolStripMenuItem_rfc1123Range.Text = "RFC1123时间值范围...";
            this.ToolStripMenuItem_rfc1123Range.Click += new System.EventHandler(this.ToolStripMenuItem_rfc1123Range_Click);
            // 
            // ToolStripMenuItem_uRange
            // 
            this.ToolStripMenuItem_uRange.Name = "ToolStripMenuItem_uRange";
            this.ToolStripMenuItem_uRange.Size = new System.Drawing.Size(217, 22);
            this.ToolStripMenuItem_uRange.Text = "u时间值范围...";
            this.ToolStripMenuItem_uRange.Click += new System.EventHandler(this.ToolStripMenuItem_uRange_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(214, 6);
            this.toolStripSeparator2.Visible = false;
            // 
            // ToolStripMenuItem_initFingerprintCache
            // 
            this.ToolStripMenuItem_initFingerprintCache.Name = "ToolStripMenuItem_initFingerprintCache";
            this.ToolStripMenuItem_initFingerprintCache.Size = new System.Drawing.Size(217, 22);
            this.ToolStripMenuItem_initFingerprintCache.Text = "初始化指纹数据高速缓存...";
            this.ToolStripMenuItem_initFingerprintCache.Visible = false;
            this.ToolStripMenuItem_initFingerprintCache.Click += new System.EventHandler(this.ToolStripMenuItem_initFingerprintCache_Click);
            // 
            // ReaderSearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(367, 276);
            this.Controls.Add(this.tableLayoutPanel_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ReaderSearchForm";
            this.ShowInTaskbar = false;
            this.Text = "读者查询";
            this.Activated += new System.EventHandler(this.ReaderSearchForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ReaderSearchForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ReaderSearchForm_FormClosed);
            this.Load += new System.EventHandler(this.ReaderSearchForm_Load);
            this.tableLayoutPanel_main.ResumeLayout(false);
            this.tableLayoutPanel_main.PerformLayout();
            this.tableLayoutPanel_query.ResumeLayout(false);
            this.tableLayoutPanel_query.PerformLayout();
            this.toolStrip_search.ResumeLayout(false);
            this.toolStrip_search.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_queryWord;
        private System.Windows.Forms.ComboBox comboBox_from;
        private DigitalPlatform.GUI.ListViewNF listView_records;
        private System.Windows.Forms.ColumnHeader columnHeader_path;
        private System.Windows.Forms.ColumnHeader columnHeader_1;
        private System.Windows.Forms.ComboBox comboBox_readerDbName;
        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_main;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_query;
        private System.Windows.Forms.Label label_from;
        private System.Windows.Forms.Label label_readerDbName;
        private System.Windows.Forms.Label label_queryWord;
        private System.Windows.Forms.Label label_matchStyle;
        private System.Windows.Forms.ComboBox comboBox_matchStyle;
        private System.Windows.Forms.ToolStrip toolStrip_search;
        private System.Windows.Forms.ToolStripButton toolStripButton_search;
        private System.Windows.Forms.ToolStripDropDownButton toolStripDropDownButton_inputTimeString;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_rfc1123Single;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_uSingle;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_rfc1123Range;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_uRange;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_initFingerprintCache;
    }
}