namespace dp2Catalog
{
    partial class OaiSearchForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OaiSearchForm));
            this.listView_browse = new System.Windows.Forms.ListView();
            this.columnHeader_id = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_title = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_author = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_publisher = new System.Windows.Forms.ColumnHeader();
            this.columnHeader_publishDate = new System.Windows.Forms.ColumnHeader();
            this.imageList_browseItemType = new System.Windows.Forms.ImageList(this.components);
            this.splitContainer_queryAndResultInfo = new System.Windows.Forms.SplitContainer();
            this.tableLayoutPanel_presentFormatAndSearchResult = new System.Windows.Forms.TableLayoutPanel();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.splitContainer_up = new System.Windows.Forms.SplitContainer();
            this.oaiTargeControl1 = new dp2Catalog.OaiTargeControl();
            this.splitContainer_queryAndResultInfo.Panel2.SuspendLayout();
            this.splitContainer_queryAndResultInfo.SuspendLayout();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.splitContainer_up.Panel1.SuspendLayout();
            this.splitContainer_up.Panel2.SuspendLayout();
            this.splitContainer_up.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView_browse
            // 
            this.listView_browse.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_id,
            this.columnHeader_title,
            this.columnHeader_author,
            this.columnHeader_publisher,
            this.columnHeader_publishDate});
            this.listView_browse.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_browse.Font = new System.Drawing.Font("Arial Unicode MS", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.listView_browse.FullRowSelect = true;
            this.listView_browse.HideSelection = false;
            this.listView_browse.LargeImageList = this.imageList_browseItemType;
            this.listView_browse.Location = new System.Drawing.Point(0, 0);
            this.listView_browse.Name = "listView_browse";
            this.listView_browse.Size = new System.Drawing.Size(687, 148);
            this.listView_browse.SmallImageList = this.imageList_browseItemType;
            this.listView_browse.TabIndex = 0;
            this.listView_browse.UseCompatibleStateImageBehavior = false;
            this.listView_browse.View = System.Windows.Forms.View.Details;
            this.listView_browse.VirtualMode = true;
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
            // 
            // splitContainer_queryAndResultInfo
            // 
            this.splitContainer_queryAndResultInfo.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_queryAndResultInfo.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_queryAndResultInfo.Name = "splitContainer_queryAndResultInfo";
            this.splitContainer_queryAndResultInfo.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_queryAndResultInfo.Panel2
            // 
            this.splitContainer_queryAndResultInfo.Panel2.Controls.Add(this.tableLayoutPanel_presentFormatAndSearchResult);
            this.splitContainer_queryAndResultInfo.Size = new System.Drawing.Size(375, 189);
            this.splitContainer_queryAndResultInfo.SplitterDistance = 75;
            this.splitContainer_queryAndResultInfo.SplitterWidth = 8;
            this.splitContainer_queryAndResultInfo.TabIndex = 1;
            // 
            // tableLayoutPanel_presentFormatAndSearchResult
            // 
            this.tableLayoutPanel_presentFormatAndSearchResult.ColumnCount = 1;
            this.tableLayoutPanel_presentFormatAndSearchResult.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel_presentFormatAndSearchResult.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel_presentFormatAndSearchResult.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel_presentFormatAndSearchResult.Name = "tableLayoutPanel_presentFormatAndSearchResult";
            this.tableLayoutPanel_presentFormatAndSearchResult.RowCount = 2;
            this.tableLayoutPanel_presentFormatAndSearchResult.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_presentFormatAndSearchResult.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel_presentFormatAndSearchResult.Size = new System.Drawing.Size(375, 106);
            this.tableLayoutPanel_presentFormatAndSearchResult.TabIndex = 0;
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
            this.splitContainer_main.Size = new System.Drawing.Size(687, 341);
            this.splitContainer_main.SplitterDistance = 189;
            this.splitContainer_main.TabIndex = 1;
            // 
            // splitContainer_up
            // 
            this.splitContainer_up.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_up.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_up.Name = "splitContainer_up";
            // 
            // splitContainer_up.Panel1
            // 
            this.splitContainer_up.Panel1.Controls.Add(this.oaiTargeControl1);
            // 
            // splitContainer_up.Panel2
            // 
            this.splitContainer_up.Panel2.Controls.Add(this.splitContainer_queryAndResultInfo);
            this.splitContainer_up.Size = new System.Drawing.Size(687, 189);
            this.splitContainer_up.SplitterDistance = 308;
            this.splitContainer_up.TabIndex = 0;
            // 
            // oaiTargeControl1
            // 
            this.oaiTargeControl1.Changed = false;
            this.oaiTargeControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.oaiTargeControl1.ImageIndex = 0;
            this.oaiTargeControl1.Location = new System.Drawing.Point(0, 0);
            this.oaiTargeControl1.Name = "oaiTargeControl1";
            this.oaiTargeControl1.SelectedImageIndex = 0;
            this.oaiTargeControl1.Size = new System.Drawing.Size(308, 189);
            this.oaiTargeControl1.TabIndex = 0;
            // 
            // OaiSearchForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(687, 341);
            this.Controls.Add(this.splitContainer_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "OaiSearchForm";
            this.ShowInTaskbar = false;
            this.Text = "OaiSearchForm";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.OaiSearchForm_FormClosed);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OaiSearchForm_FormClosing);
            this.Load += new System.EventHandler(this.OaiSearchForm_Load);
            this.splitContainer_queryAndResultInfo.Panel2.ResumeLayout(false);
            this.splitContainer_queryAndResultInfo.ResumeLayout(false);
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            this.splitContainer_main.ResumeLayout(false);
            this.splitContainer_up.Panel1.ResumeLayout(false);
            this.splitContainer_up.Panel2.ResumeLayout(false);
            this.splitContainer_up.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView_browse;
        private System.Windows.Forms.ColumnHeader columnHeader_id;
        private System.Windows.Forms.ColumnHeader columnHeader_title;
        private System.Windows.Forms.ColumnHeader columnHeader_author;
        private System.Windows.Forms.ColumnHeader columnHeader_publisher;
        private System.Windows.Forms.ColumnHeader columnHeader_publishDate;
        private System.Windows.Forms.ImageList imageList_browseItemType;
        private System.Windows.Forms.SplitContainer splitContainer_queryAndResultInfo;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel_presentFormatAndSearchResult;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.SplitContainer splitContainer_up;
        private OaiTargeControl oaiTargeControl1;
    }
}