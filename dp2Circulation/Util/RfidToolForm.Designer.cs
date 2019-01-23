namespace dp2Circulation
{
    partial class RfidToolForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RfidToolForm));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_saveRfid = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_loadRfid = new System.Windows.Forms.ToolStripButton();
            this.listView_tags = new System.Windows.Forms.ListView();
            this.columnHeader_readerName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_uid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_pii = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_tag = new System.Windows.Forms.TabPage();
            this.chipEditor1 = new DigitalPlatform.RFID.UI.ChipEditor();
            this.tabPage_record = new System.Windows.Forms.TabPage();
            this.propertyGrid_record = new System.Windows.Forms.PropertyGrid();
            this.toolStripButton_autoRefresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage_tag.SuspendLayout();
            this.tabPage_record.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_saveRfid,
            this.toolStripButton_loadRfid,
            this.toolStripSeparator1,
            this.toolStripButton_autoRefresh});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(800, 31);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_saveRfid
            // 
            this.toolStripButton_saveRfid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_saveRfid.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_saveRfid.Image")));
            this.toolStripButton_saveRfid.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_saveRfid.Name = "toolStripButton_saveRfid";
            this.toolStripButton_saveRfid.Size = new System.Drawing.Size(28, 28);
            this.toolStripButton_saveRfid.Text = "写入标签";
            // 
            // toolStripButton_loadRfid
            // 
            this.toolStripButton_loadRfid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_loadRfid.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_loadRfid.Image")));
            this.toolStripButton_loadRfid.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_loadRfid.Name = "toolStripButton_loadRfid";
            this.toolStripButton_loadRfid.Size = new System.Drawing.Size(28, 28);
            this.toolStripButton_loadRfid.Text = "装载标签";
            this.toolStripButton_loadRfid.Click += new System.EventHandler(this.toolStripButton_loadRfid_Click);
            // 
            // listView_tags
            // 
            this.listView_tags.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_readerName,
            this.columnHeader_uid,
            this.columnHeader_pii});
            this.listView_tags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_tags.FullRowSelect = true;
            this.listView_tags.HideSelection = false;
            this.listView_tags.Location = new System.Drawing.Point(0, 0);
            this.listView_tags.MultiSelect = false;
            this.listView_tags.Name = "listView_tags";
            this.listView_tags.Size = new System.Drawing.Size(365, 419);
            this.listView_tags.TabIndex = 1;
            this.listView_tags.UseCompatibleStateImageBehavior = false;
            this.listView_tags.View = System.Windows.Forms.View.Details;
            this.listView_tags.SelectedIndexChanged += new System.EventHandler(this.listView_tags_SelectedIndexChanged);
            // 
            // columnHeader_readerName
            // 
            this.columnHeader_readerName.Text = "读卡器";
            this.columnHeader_readerName.Width = 200;
            // 
            // columnHeader_uid
            // 
            this.columnHeader_uid.Text = "UID";
            this.columnHeader_uid.Width = 200;
            // 
            // columnHeader_pii
            // 
            this.columnHeader_pii.Text = "PII";
            this.columnHeader_pii.Width = 200;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 31);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.listView_tags);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(800, 419);
            this.splitContainer1.SplitterDistance = 365;
            this.splitContainer1.SplitterWidth = 8;
            this.splitContainer1.TabIndex = 2;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage_tag);
            this.tabControl1.Controls.Add(this.tabPage_record);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(427, 419);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage_tag
            // 
            this.tabPage_tag.Controls.Add(this.chipEditor1);
            this.tabPage_tag.Location = new System.Drawing.Point(4, 28);
            this.tabPage_tag.Name = "tabPage_tag";
            this.tabPage_tag.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_tag.Size = new System.Drawing.Size(419, 387);
            this.tabPage_tag.TabIndex = 0;
            this.tabPage_tag.Text = "RFID 标签";
            this.tabPage_tag.UseVisualStyleBackColor = true;
            // 
            // chipEditor1
            // 
            this.chipEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chipEditor1.Location = new System.Drawing.Point(3, 3);
            this.chipEditor1.LogicChipItem = null;
            this.chipEditor1.Name = "chipEditor1";
            this.chipEditor1.Size = new System.Drawing.Size(413, 381);
            this.chipEditor1.TabIndex = 0;
            // 
            // tabPage_record
            // 
            this.tabPage_record.Controls.Add(this.propertyGrid_record);
            this.tabPage_record.Location = new System.Drawing.Point(4, 28);
            this.tabPage_record.Name = "tabPage_record";
            this.tabPage_record.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_record.Size = new System.Drawing.Size(419, 387);
            this.tabPage_record.TabIndex = 1;
            this.tabPage_record.Text = "数据记录";
            this.tabPage_record.UseVisualStyleBackColor = true;
            // 
            // propertyGrid_record
            // 
            this.propertyGrid_record.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid_record.Location = new System.Drawing.Point(3, 3);
            this.propertyGrid_record.Name = "propertyGrid_record";
            this.propertyGrid_record.Size = new System.Drawing.Size(413, 381);
            this.propertyGrid_record.TabIndex = 1;
            // 
            // toolStripButton_autoRefresh
            // 
            this.toolStripButton_autoRefresh.CheckOnClick = true;
            this.toolStripButton_autoRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_autoRefresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_autoRefresh.Image")));
            this.toolStripButton_autoRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_autoRefresh.Name = "toolStripButton_autoRefresh";
            this.toolStripButton_autoRefresh.Size = new System.Drawing.Size(86, 28);
            this.toolStripButton_autoRefresh.Text = "自动刷新";
            this.toolStripButton_autoRefresh.CheckStateChanged += new System.EventHandler(this.toolStripButton_autoRefresh_CheckStateChanged);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 31);
            // 
            // RfidToolForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "RfidToolForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "RFID 工具";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RfidToolForm_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.RfidToolForm_FormClosed);
            this.Load += new System.EventHandler(this.RfidToolForm_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_tag.ResumeLayout(false);
            this.tabPage_record.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ListView listView_tags;
        private System.Windows.Forms.ColumnHeader columnHeader_readerName;
        private System.Windows.Forms.ColumnHeader columnHeader_uid;
        private System.Windows.Forms.ColumnHeader columnHeader_pii;
        private System.Windows.Forms.ToolStripButton toolStripButton_saveRfid;
        private System.Windows.Forms.ToolStripButton toolStripButton_loadRfid;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_tag;
        private System.Windows.Forms.TabPage tabPage_record;
        private System.Windows.Forms.PropertyGrid propertyGrid_record;
        private DigitalPlatform.RFID.UI.ChipEditor chipEditor1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_autoRefresh;
    }
}