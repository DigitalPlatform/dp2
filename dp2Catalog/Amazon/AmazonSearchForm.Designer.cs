namespace dp2Catalog
{
    partial class AmazonSearchForm
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

            this.eventComplete.Dispose();

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AmazonSearchForm));
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.tabControl_query = new System.Windows.Forms.TabControl();
            this.tabPage_simple = new System.Windows.Forms.TabPage();
            this.amazonSimpleQueryControl_simple = new dp2Catalog.AmazonSimpleQueryControl();
            this.button_searchSimple = new System.Windows.Forms.Button();
            this.tabPage_multiline = new System.Windows.Forms.TabPage();
            this.splitContainer_multiline = new System.Windows.Forms.SplitContainer();
            this.textBox_mutiline_queryContent = new System.Windows.Forms.TextBox();
            this.amazonSimpleQueryControl_multiLine = new dp2Catalog.AmazonSimpleQueryControl();
            this.listView_browse = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_path = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_isbnIssn = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList_browseItemType = new System.Windows.Forms.ImageList(this.components);
            this.columnHeader_5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.tabControl_query.SuspendLayout();
            this.tabPage_simple.SuspendLayout();
            this.tabPage_multiline.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_multiline)).BeginInit();
            this.splitContainer_multiline.Panel1.SuspendLayout();
            this.splitContainer_multiline.Panel2.SuspendLayout();
            this.splitContainer_multiline.SuspendLayout();
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
            this.splitContainer_main.Panel1.Controls.Add(this.tabControl_query);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.listView_browse);
            this.splitContainer_main.Size = new System.Drawing.Size(433, 264);
            this.splitContainer_main.SplitterDistance = 132;
            this.splitContainer_main.TabIndex = 1;
            // 
            // tabControl_query
            // 
            this.tabControl_query.Controls.Add(this.tabPage_simple);
            this.tabControl_query.Controls.Add(this.tabPage_multiline);
            this.tabControl_query.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl_query.Location = new System.Drawing.Point(0, 0);
            this.tabControl_query.Name = "tabControl_query";
            this.tabControl_query.SelectedIndex = 0;
            this.tabControl_query.Size = new System.Drawing.Size(433, 132);
            this.tabControl_query.TabIndex = 0;
            // 
            // tabPage_simple
            // 
            this.tabPage_simple.Controls.Add(this.amazonSimpleQueryControl_simple);
            this.tabPage_simple.Controls.Add(this.button_searchSimple);
            this.tabPage_simple.Location = new System.Drawing.Point(4, 22);
            this.tabPage_simple.Name = "tabPage_simple";
            this.tabPage_simple.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_simple.Size = new System.Drawing.Size(425, 106);
            this.tabPage_simple.TabIndex = 0;
            this.tabPage_simple.Text = "简单";
            this.tabPage_simple.UseVisualStyleBackColor = true;
            // 
            // amazonSimpleQueryControl_simple
            // 
            this.amazonSimpleQueryControl_simple.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.amazonSimpleQueryControl_simple.AutoScroll = true;
            this.amazonSimpleQueryControl_simple.Comment = "";
            this.amazonSimpleQueryControl_simple.From = "";
            this.amazonSimpleQueryControl_simple.Location = new System.Drawing.Point(0, 3);
            this.amazonSimpleQueryControl_simple.MatchStyle = "";
            this.amazonSimpleQueryControl_simple.Name = "amazonSimpleQueryControl_simple";
            this.amazonSimpleQueryControl_simple.Size = new System.Drawing.Size(335, 100);
            this.amazonSimpleQueryControl_simple.TabIndex = 9;
            this.amazonSimpleQueryControl_simple.Word = "";
            this.amazonSimpleQueryControl_simple.WordVisible = true;
            // 
            // button_searchSimple
            // 
            this.button_searchSimple.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_searchSimple.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_searchSimple.Location = new System.Drawing.Point(342, 5);
            this.button_searchSimple.Margin = new System.Windows.Forms.Padding(2);
            this.button_searchSimple.Name = "button_searchSimple";
            this.button_searchSimple.Size = new System.Drawing.Size(78, 22);
            this.button_searchSimple.TabIndex = 7;
            this.button_searchSimple.Text = "检索";
            this.button_searchSimple.UseVisualStyleBackColor = true;
            this.button_searchSimple.Click += new System.EventHandler(this.button_searchSimple_Click);
            this.button_searchSimple.MouseUp += new System.Windows.Forms.MouseEventHandler(this.button_searchSimple_MouseUp);
            // 
            // tabPage_multiline
            // 
            this.tabPage_multiline.Controls.Add(this.splitContainer_multiline);
            this.tabPage_multiline.Location = new System.Drawing.Point(4, 22);
            this.tabPage_multiline.Name = "tabPage_multiline";
            this.tabPage_multiline.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_multiline.Size = new System.Drawing.Size(425, 106);
            this.tabPage_multiline.TabIndex = 1;
            this.tabPage_multiline.Text = "多行";
            this.tabPage_multiline.UseVisualStyleBackColor = true;
            // 
            // splitContainer_multiline
            // 
            this.splitContainer_multiline.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_multiline.Location = new System.Drawing.Point(3, 3);
            this.splitContainer_multiline.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_multiline.Name = "splitContainer_multiline";
            // 
            // splitContainer_multiline.Panel1
            // 
            this.splitContainer_multiline.Panel1.Controls.Add(this.textBox_mutiline_queryContent);
            // 
            // splitContainer_multiline.Panel2
            // 
            this.splitContainer_multiline.Panel2.Controls.Add(this.amazonSimpleQueryControl_multiLine);
            this.splitContainer_multiline.Size = new System.Drawing.Size(419, 100);
            this.splitContainer_multiline.SplitterDistance = 218;
            this.splitContainer_multiline.SplitterWidth = 8;
            this.splitContainer_multiline.TabIndex = 2;
            // 
            // textBox_mutiline_queryContent
            // 
            this.textBox_mutiline_queryContent.AcceptsReturn = true;
            this.textBox_mutiline_queryContent.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_mutiline_queryContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBox_mutiline_queryContent.Location = new System.Drawing.Point(0, 0);
            this.textBox_mutiline_queryContent.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_mutiline_queryContent.MaxLength = 1000000;
            this.textBox_mutiline_queryContent.Multiline = true;
            this.textBox_mutiline_queryContent.Name = "textBox_mutiline_queryContent";
            this.textBox_mutiline_queryContent.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_mutiline_queryContent.Size = new System.Drawing.Size(218, 100);
            this.textBox_mutiline_queryContent.TabIndex = 1;
            // 
            // amazonSimpleQueryControl_multiLine
            // 
            this.amazonSimpleQueryControl_multiLine.AutoScroll = true;
            this.amazonSimpleQueryControl_multiLine.Comment = "";
            this.amazonSimpleQueryControl_multiLine.Dock = System.Windows.Forms.DockStyle.Fill;
            this.amazonSimpleQueryControl_multiLine.From = "";
            this.amazonSimpleQueryControl_multiLine.Location = new System.Drawing.Point(0, 0);
            this.amazonSimpleQueryControl_multiLine.MatchStyle = "";
            this.amazonSimpleQueryControl_multiLine.Name = "amazonSimpleQueryControl_multiLine";
            this.amazonSimpleQueryControl_multiLine.Size = new System.Drawing.Size(193, 100);
            this.amazonSimpleQueryControl_multiLine.TabIndex = 0;
            this.amazonSimpleQueryControl_multiLine.Word = "";
            this.amazonSimpleQueryControl_multiLine.WordVisible = true;
            // 
            // listView_browse
            // 
            this.listView_browse.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.listView_browse.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_path,
            this.columnHeader_isbnIssn,
            this.columnHeader_1,
            this.columnHeader_2,
            this.columnHeader_3,
            this.columnHeader_4,
            this.columnHeader_5});
            this.listView_browse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_browse.FullRowSelect = true;
            this.listView_browse.HideSelection = false;
            this.listView_browse.LargeImageList = this.imageList_browseItemType;
            this.listView_browse.Location = new System.Drawing.Point(0, 0);
            this.listView_browse.Margin = new System.Windows.Forms.Padding(2);
            this.listView_browse.Name = "listView_browse";
            this.listView_browse.Size = new System.Drawing.Size(433, 128);
            this.listView_browse.SmallImageList = this.imageList_browseItemType;
            this.listView_browse.TabIndex = 2;
            this.listView_browse.UseCompatibleStateImageBehavior = false;
            this.listView_browse.View = System.Windows.Forms.View.Details;
            this.listView_browse.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView_browse_ColumnClick);
            this.listView_browse.SelectedIndexChanged += new System.EventHandler(this.listView_browse_SelectedIndexChanged);
            this.listView_browse.DoubleClick += new System.EventHandler(this.listView_browse_DoubleClick);
            this.listView_browse.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_browse_MouseUp);
            // 
            // columnHeader_path
            // 
            this.columnHeader_path.Text = "ASIN";
            this.columnHeader_path.Width = 130;
            // 
            // columnHeader_isbnIssn
            // 
            this.columnHeader_isbnIssn.Text = "ISBN/ISSN";
            this.columnHeader_isbnIssn.Width = 100;
            // 
            // columnHeader_1
            // 
            this.columnHeader_1.Text = "题名";
            this.columnHeader_1.Width = 200;
            // 
            // columnHeader_2
            // 
            this.columnHeader_2.Text = "责任者";
            this.columnHeader_2.Width = 100;
            // 
            // columnHeader_3
            // 
            this.columnHeader_3.Text = "出版者";
            this.columnHeader_3.Width = 50;
            // 
            // columnHeader_4
            // 
            this.columnHeader_4.Text = "出版日期";
            this.columnHeader_4.Width = 150;
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
            // columnHeader_5
            // 
            this.columnHeader_5.Text = "EAN";
            this.columnHeader_5.Width = 120;
            // 
            // AmazonSearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(433, 264);
            this.Controls.Add(this.splitContainer_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AmazonSearchForm";
            this.Text = "亚马逊检索窗";
            this.Activated += new System.EventHandler(this.AmazonSearchForm_Activated);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AmazonSearchForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.AmazonSearchForm_FormClosed);
            this.Load += new System.EventHandler(this.AmazonSearchForm_Load);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.tabControl_query.ResumeLayout(false);
            this.tabPage_simple.ResumeLayout(false);
            this.tabPage_multiline.ResumeLayout(false);
            this.splitContainer_multiline.Panel1.ResumeLayout(false);
            this.splitContainer_multiline.Panel1.PerformLayout();
            this.splitContainer_multiline.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_multiline)).EndInit();
            this.splitContainer_multiline.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.TabControl tabControl_query;
        private System.Windows.Forms.TabPage tabPage_simple;
        private System.Windows.Forms.TabPage tabPage_multiline;
        private DigitalPlatform.GUI.ListViewNF listView_browse;
        private System.Windows.Forms.ColumnHeader columnHeader_path;
        private System.Windows.Forms.ColumnHeader columnHeader_1;
        private System.Windows.Forms.ColumnHeader columnHeader_2;
        private System.Windows.Forms.ColumnHeader columnHeader_3;
        private System.Windows.Forms.ColumnHeader columnHeader_4;
        private System.Windows.Forms.Button button_searchSimple;
        private AmazonSimpleQueryControl amazonSimpleQueryControl_simple;
        private System.Windows.Forms.ImageList imageList_browseItemType;
        private System.Windows.Forms.SplitContainer splitContainer_multiline;
        private System.Windows.Forms.TextBox textBox_mutiline_queryContent;
        private AmazonSimpleQueryControl amazonSimpleQueryControl_multiLine;
        private System.Windows.Forms.ColumnHeader columnHeader_isbnIssn;
        private System.Windows.Forms.ColumnHeader columnHeader_5;
    }
}