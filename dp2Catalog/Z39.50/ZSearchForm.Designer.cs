namespace dp2Catalog
{
    partial class ZSearchForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ZSearchForm));
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.splitContainer_up = new System.Windows.Forms.SplitContainer();
            this.zTargetControl1 = new dp2Catalog.ZTargetControl();
            this.splitContainer_queryAndResultInfo = new System.Windows.Forms.SplitContainer();
            this.queryControl1 = new dp2Catalog.ZQueryControl();
            this.tableLayoutPanel_presentFormatAndSearchResult = new System.Windows.Forms.TableLayoutPanel();
            this.textBox_resultInfo = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel_presentFormat = new System.Windows.Forms.TableLayoutPanel();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_recordSyntax = new System.Windows.Forms.ComboBox();
            this.comboBox_elementSetName = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.listView_browse = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_id = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_title = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_author = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_publisher = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_publishDate = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList_browseItemType = new System.Windows.Forms.ImageList(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_up)).BeginInit();
            this.splitContainer_up.Panel1.SuspendLayout();
            this.splitContainer_up.Panel2.SuspendLayout();
            this.splitContainer_up.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_queryAndResultInfo)).BeginInit();
            this.splitContainer_queryAndResultInfo.Panel1.SuspendLayout();
            this.splitContainer_queryAndResultInfo.Panel2.SuspendLayout();
            this.splitContainer_queryAndResultInfo.SuspendLayout();
            this.tableLayoutPanel_presentFormatAndSearchResult.SuspendLayout();
            this.tableLayoutPanel_presentFormat.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_main.Location = new System.Drawing.Point(0, 0);
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
            this.splitContainer_main.Size = new System.Drawing.Size(607, 400);
            this.splitContainer_main.SplitterDistance = 220;
            this.splitContainer_main.SplitterWidth = 8;
            this.splitContainer_main.TabIndex = 0;
            // 
            // splitContainer_up
            // 
            this.splitContainer_up.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_up.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_up.Name = "splitContainer_up";
            // 
            // splitContainer_up.Panel1
            // 
            this.splitContainer_up.Panel1.Controls.Add(this.zTargetControl1);
            // 
            // splitContainer_up.Panel2
            // 
            this.splitContainer_up.Panel2.Controls.Add(this.splitContainer_queryAndResultInfo);
            this.splitContainer_up.Size = new System.Drawing.Size(607, 220);
            this.splitContainer_up.SplitterDistance = 271;
            this.splitContainer_up.TabIndex = 0;
            // 
            // zTargetControl1
            // 
            this.zTargetControl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.zTargetControl1.Changed = false;
            this.zTargetControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.zTargetControl1.HideSelection = false;
            this.zTargetControl1.ImageIndex = 0;
            this.zTargetControl1.Location = new System.Drawing.Point(0, 0);
            this.zTargetControl1.Name = "zTargetControl1";
            this.zTargetControl1.SelectedImageIndex = 0;
            this.zTargetControl1.Size = new System.Drawing.Size(271, 220);
            this.zTargetControl1.TabIndex = 0;
            this.zTargetControl1.OnSetMenu += new DigitalPlatform.GUI.GuiAppendMenuEventHandle(this.zTargetControl1_OnSetMenu);
            this.zTargetControl1.OnServerChanged += new dp2Catalog.ServerChangedEventHandle(this.zTargetControl1_OnServerChanged);
            this.zTargetControl1.BeforeSelect += new System.Windows.Forms.TreeViewCancelEventHandler(this.zTargetControl1_BeforeSelect);
            this.zTargetControl1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.zTargetControl1_AfterSelect);
            // 
            // splitContainer_queryAndResultInfo
            // 
            this.splitContainer_queryAndResultInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_queryAndResultInfo.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_queryAndResultInfo.Name = "splitContainer_queryAndResultInfo";
            this.splitContainer_queryAndResultInfo.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_queryAndResultInfo.Panel1
            // 
            this.splitContainer_queryAndResultInfo.Panel1.Controls.Add(this.queryControl1);
            // 
            // splitContainer_queryAndResultInfo.Panel2
            // 
            this.splitContainer_queryAndResultInfo.Panel2.Controls.Add(this.tableLayoutPanel_presentFormatAndSearchResult);
            this.splitContainer_queryAndResultInfo.Size = new System.Drawing.Size(332, 220);
            this.splitContainer_queryAndResultInfo.SplitterDistance = 117;
            this.splitContainer_queryAndResultInfo.SplitterWidth = 8;
            this.splitContainer_queryAndResultInfo.TabIndex = 1;
            // 
            // queryControl1
            // 
            this.queryControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.queryControl1.Location = new System.Drawing.Point(0, 0);
            this.queryControl1.Margin = new System.Windows.Forms.Padding(2);
            this.queryControl1.Name = "queryControl1";
            this.queryControl1.Size = new System.Drawing.Size(332, 117);
            this.queryControl1.TabIndex = 0;
            // 
            // tableLayoutPanel_presentFormatAndSearchResult
            // 
            this.tableLayoutPanel_presentFormatAndSearchResult.ColumnCount = 1;
            this.tableLayoutPanel_presentFormatAndSearchResult.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_presentFormatAndSearchResult.Controls.Add(this.textBox_resultInfo, 0, 1);
            this.tableLayoutPanel_presentFormatAndSearchResult.Controls.Add(this.tableLayoutPanel_presentFormat, 0, 0);
            this.tableLayoutPanel_presentFormatAndSearchResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_presentFormatAndSearchResult.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_presentFormatAndSearchResult.Name = "tableLayoutPanel_presentFormatAndSearchResult";
            this.tableLayoutPanel_presentFormatAndSearchResult.RowCount = 2;
            this.tableLayoutPanel_presentFormatAndSearchResult.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_presentFormatAndSearchResult.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_presentFormatAndSearchResult.Size = new System.Drawing.Size(332, 95);
            this.tableLayoutPanel_presentFormatAndSearchResult.TabIndex = 0;
            // 
            // textBox_resultInfo
            // 
            this.textBox_resultInfo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_resultInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_resultInfo.Location = new System.Drawing.Point(0, 61);
            this.textBox_resultInfo.Margin = new System.Windows.Forms.Padding(0);
            this.textBox_resultInfo.Multiline = true;
            this.textBox_resultInfo.Name = "textBox_resultInfo";
            this.textBox_resultInfo.ReadOnly = true;
            this.textBox_resultInfo.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_resultInfo.Size = new System.Drawing.Size(332, 39);
            this.textBox_resultInfo.TabIndex = 0;
            // 
            // tableLayoutPanel_presentFormat
            // 
            this.tableLayoutPanel_presentFormat.AutoScroll = true;
            this.tableLayoutPanel_presentFormat.ColumnCount = 2;
            this.tableLayoutPanel_presentFormat.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel_presentFormat.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_presentFormat.Controls.Add(this.label1, 0, 0);
            this.tableLayoutPanel_presentFormat.Controls.Add(this.comboBox_recordSyntax, 1, 0);
            this.tableLayoutPanel_presentFormat.Controls.Add(this.comboBox_elementSetName, 1, 1);
            this.tableLayoutPanel_presentFormat.Controls.Add(this.label2, 0, 1);
            this.tableLayoutPanel_presentFormat.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_presentFormat.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_presentFormat.Margin = new System.Windows.Forms.Padding(0);
            this.tableLayoutPanel_presentFormat.Name = "tableLayoutPanel_presentFormat";
            this.tableLayoutPanel_presentFormat.RowCount = 3;
            this.tableLayoutPanel_presentFormat.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_presentFormat.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_presentFormat.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_presentFormat.Size = new System.Drawing.Size(332, 61);
            this.tableLayoutPanel_presentFormat.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 26);
            this.label1.TabIndex = 0;
            this.label1.Text = "数据格式(&S):";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // comboBox_recordSyntax
            // 
            this.comboBox_recordSyntax.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_recordSyntax.DropDownHeight = 300;
            this.comboBox_recordSyntax.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_recordSyntax.FormattingEnabled = true;
            this.comboBox_recordSyntax.IntegralHeight = false;
            this.comboBox_recordSyntax.Items.AddRange(new object[] {
            "1.2.840.10003.5.1 -- UNIMARC",
            "1.2.840.10003.5.10 -- MARC21",
            "1.2.840.10003.5.101 -- SUTRS",
            "1.2.840.10003.5.109.10 -- XML"});
            this.comboBox_recordSyntax.Location = new System.Drawing.Point(86, 3);
            this.comboBox_recordSyntax.Name = "comboBox_recordSyntax";
            this.comboBox_recordSyntax.Size = new System.Drawing.Size(243, 20);
            this.comboBox_recordSyntax.TabIndex = 2;
            this.comboBox_recordSyntax.SizeChanged += new System.EventHandler(this.comboBox_recordSyntax_SizeChanged);
            // 
            // comboBox_elementSetName
            // 
            this.comboBox_elementSetName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_elementSetName.DropDownHeight = 300;
            this.comboBox_elementSetName.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_elementSetName.FormattingEnabled = true;
            this.comboBox_elementSetName.IntegralHeight = false;
            this.comboBox_elementSetName.Items.AddRange(new object[] {
            "B -- Brief(MARC records)",
            "F -- Full (MARC and OPAC records)",
            "dc --  Dublin Core (XML records)",
            "mods  -- MODS (XML records)",
            "marcxml -- MARCXML (XML records), default schema for XML",
            "opacxml -- MARCXML with holdings attached"});
            this.comboBox_elementSetName.Location = new System.Drawing.Point(86, 29);
            this.comboBox_elementSetName.Name = "comboBox_elementSetName";
            this.comboBox_elementSetName.Size = new System.Drawing.Size(243, 20);
            this.comboBox_elementSetName.TabIndex = 3;
            this.comboBox_elementSetName.SizeChanged += new System.EventHandler(this.comboBox_elementSetName_SizeChanged);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(3, 26);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 26);
            this.label2.TabIndex = 1;
            this.label2.Text = "元素集名(&E):";
            this.label2.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // listView_browse
            // 
            this.listView_browse.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView_browse.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_id,
            this.columnHeader_title,
            this.columnHeader_author,
            this.columnHeader_publisher,
            this.columnHeader_publishDate});
            this.listView_browse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_browse.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listView_browse.FullRowSelect = true;
            this.listView_browse.HideSelection = false;
            this.listView_browse.LargeImageList = this.imageList_browseItemType;
            this.listView_browse.Location = new System.Drawing.Point(0, 0);
            this.listView_browse.Margin = new System.Windows.Forms.Padding(0);
            this.listView_browse.Name = "listView_browse";
            this.listView_browse.Size = new System.Drawing.Size(607, 172);
            this.listView_browse.SmallImageList = this.imageList_browseItemType;
            this.listView_browse.TabIndex = 0;
            this.listView_browse.UseCompatibleStateImageBehavior = false;
            this.listView_browse.View = System.Windows.Forms.View.Details;
            this.listView_browse.VirtualMode = true;
            this.listView_browse.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listView_browse_ItemSelectionChanged);
            this.listView_browse.RetrieveVirtualItem += new System.Windows.Forms.RetrieveVirtualItemEventHandler(this.listView_browse_RetrieveVirtualItem);
            this.listView_browse.SelectedIndexChanged += new System.EventHandler(this.listView_browse_SelectedIndexChanged);
            this.listView_browse.VirtualItemsSelectionRangeChanged += new System.Windows.Forms.ListViewVirtualItemsSelectionRangeChangedEventHandler(this.listView_browse_VirtualItemsSelectionRangeChanged);
            this.listView_browse.DoubleClick += new System.EventHandler(this.listView_browse_DoubleClick);
            this.listView_browse.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_browse_MouseUp);
            // 
            // columnHeader_id
            // 
            this.columnHeader_id.Text = "ID";
            // 
            // columnHeader_title
            // 
            this.columnHeader_title.Text = "题名";
            this.columnHeader_title.Width = 200;
            // 
            // columnHeader_author
            // 
            this.columnHeader_author.Text = "责任者";
            this.columnHeader_author.Width = 100;
            // 
            // columnHeader_publisher
            // 
            this.columnHeader_publisher.Text = "出版者";
            this.columnHeader_publisher.Width = 150;
            // 
            // columnHeader_publishDate
            // 
            this.columnHeader_publishDate.Text = "出版日期";
            this.columnHeader_publishDate.Width = 100;
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
            // ZSearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(607, 400);
            this.Controls.Add(this.splitContainer_main);
            this.Font = new System.Drawing.Font("宋体", 9F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ZSearchForm";
            this.ShowInTaskbar = false;
            this.Text = "Z39.50检索窗";
            this.Activated += new System.EventHandler(this.ZSearchForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ZSearchForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ZSearchForm_FormClosed);
            this.Load += new System.EventHandler(this.ZSearchForm_Load);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.splitContainer_up.Panel1.ResumeLayout(false);
            this.splitContainer_up.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_up)).EndInit();
            this.splitContainer_up.ResumeLayout(false);
            this.splitContainer_queryAndResultInfo.Panel1.ResumeLayout(false);
            this.splitContainer_queryAndResultInfo.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_queryAndResultInfo)).EndInit();
            this.splitContainer_queryAndResultInfo.ResumeLayout(false);
            this.tableLayoutPanel_presentFormatAndSearchResult.ResumeLayout(false);
            this.tableLayoutPanel_presentFormatAndSearchResult.PerformLayout();
            this.tableLayoutPanel_presentFormat.ResumeLayout(false);
            this.tableLayoutPanel_presentFormat.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.SplitContainer splitContainer_up;
        private DigitalPlatform.GUI.ListViewNF listView_browse;
        private System.Windows.Forms.ColumnHeader columnHeader_id;
        private System.Windows.Forms.ColumnHeader columnHeader_title;
        private System.Windows.Forms.ColumnHeader columnHeader_author;
        private System.Windows.Forms.ColumnHeader columnHeader_publisher;
        private System.Windows.Forms.ColumnHeader columnHeader_publishDate;
        private ZQueryControl queryControl1;
        private ZTargetControl zTargetControl1;
        private System.Windows.Forms.SplitContainer splitContainer_queryAndResultInfo;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_presentFormatAndSearchResult;
        private System.Windows.Forms.TextBox textBox_resultInfo;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_presentFormat;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_recordSyntax;
        private System.Windows.Forms.ComboBox comboBox_elementSetName;
        private System.Windows.Forms.ImageList imageList_browseItemType;
    }
}