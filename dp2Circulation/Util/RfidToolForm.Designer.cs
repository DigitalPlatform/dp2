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

                _cancel?.Dispose();
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
            this.toolStripButton_loadRfid = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_autoRefresh = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_saveRfid = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_autoFixEas = new System.Windows.Forms.ToolStripButton();
            this.listView_tags = new System.Windows.Forms.ListView();
            this.columnHeader_pii = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_uid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_readerName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_protocol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_antenna = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_rssi = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_tag = new System.Windows.Forms.TabPage();
            this.chipEditor1 = new DigitalPlatform.RFID.UI.ChipEditor();
            this.tabPage_record = new System.Windows.Forms.TabPage();
            this.propertyGrid_record = new System.Windows.Forms.PropertyGrid();
            this.panel_okCancel = new System.Windows.Forms.Panel();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.label_message = new System.Windows.Forms.Label();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage_tag.SuspendLayout();
            this.tabPage_record.SuspendLayout();
            this.panel_okCancel.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(24, 24);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_loadRfid,
            this.toolStripSeparator1,
            this.toolStripButton_autoRefresh,
            this.toolStripSeparator2,
            this.toolStripButton_saveRfid,
            this.toolStripSeparator3,
            this.toolStripButton_autoFixEas});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(978, 38);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_loadRfid
            // 
            this.toolStripButton_loadRfid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_loadRfid.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_loadRfid.Image")));
            this.toolStripButton_loadRfid.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_loadRfid.Name = "toolStripButton_loadRfid";
            this.toolStripButton_loadRfid.Size = new System.Drawing.Size(40, 32);
            this.toolStripButton_loadRfid.Text = "装载标签";
            this.toolStripButton_loadRfid.Click += new System.EventHandler(this.toolStripButton_loadRfid_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton_autoRefresh
            // 
            this.toolStripButton_autoRefresh.CheckOnClick = true;
            this.toolStripButton_autoRefresh.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_autoRefresh.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_autoRefresh.Image")));
            this.toolStripButton_autoRefresh.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_autoRefresh.Name = "toolStripButton_autoRefresh";
            this.toolStripButton_autoRefresh.Size = new System.Drawing.Size(100, 32);
            this.toolStripButton_autoRefresh.Text = "自动刷新";
            this.toolStripButton_autoRefresh.CheckStateChanged += new System.EventHandler(this.toolStripButton_autoRefresh_CheckStateChanged);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton_saveRfid
            // 
            this.toolStripButton_saveRfid.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_saveRfid.Enabled = false;
            this.toolStripButton_saveRfid.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_saveRfid.Image")));
            this.toolStripButton_saveRfid.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_saveRfid.Name = "toolStripButton_saveRfid";
            this.toolStripButton_saveRfid.Size = new System.Drawing.Size(40, 32);
            this.toolStripButton_saveRfid.Text = "写入标签";
            this.toolStripButton_saveRfid.ToolTipText = "保存全部修改";
            this.toolStripButton_saveRfid.Click += new System.EventHandler(this.toolStripButton_saveRfid_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton_autoFixEas
            // 
            this.toolStripButton_autoFixEas.CheckOnClick = true;
            this.toolStripButton_autoFixEas.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_autoFixEas.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_autoFixEas.Image")));
            this.toolStripButton_autoFixEas.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_autoFixEas.Name = "toolStripButton_autoFixEas";
            this.toolStripButton_autoFixEas.Size = new System.Drawing.Size(193, 32);
            this.toolStripButton_autoFixEas.Text = "自动纠正 EAS 错误";
            this.toolStripButton_autoFixEas.CheckedChanged += new System.EventHandler(this.toolStripButton_autoFixEas_CheckedChanged);
            // 
            // listView_tags
            // 
            this.listView_tags.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_pii,
            this.columnHeader_uid,
            this.columnHeader_readerName,
            this.columnHeader_protocol,
            this.columnHeader_antenna,
            this.columnHeader_rssi});
            this.listView_tags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_tags.FullRowSelect = true;
            this.listView_tags.HideSelection = false;
            this.listView_tags.Location = new System.Drawing.Point(0, 0);
            this.listView_tags.Margin = new System.Windows.Forms.Padding(4);
            this.listView_tags.Name = "listView_tags";
            this.listView_tags.Size = new System.Drawing.Size(441, 385);
            this.listView_tags.TabIndex = 1;
            this.listView_tags.UseCompatibleStateImageBehavior = false;
            this.listView_tags.View = System.Windows.Forms.View.Details;
            this.listView_tags.SelectedIndexChanged += new System.EventHandler(this.listView_tags_SelectedIndexChanged);
            this.listView_tags.DoubleClick += new System.EventHandler(this.listView_tags_DoubleClick);
            this.listView_tags.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_tags_MouseUp);
            // 
            // columnHeader_pii
            // 
            this.columnHeader_pii.Text = "PII";
            this.columnHeader_pii.Width = 200;
            // 
            // columnHeader_uid
            // 
            this.columnHeader_uid.Text = "UID";
            this.columnHeader_uid.Width = 200;
            // 
            // columnHeader_readerName
            // 
            this.columnHeader_readerName.Text = "读卡器";
            this.columnHeader_readerName.Width = 200;
            // 
            // columnHeader_protocol
            // 
            this.columnHeader_protocol.Text = "协议";
            this.columnHeader_protocol.Width = 280;
            // 
            // columnHeader_antenna
            // 
            this.columnHeader_antenna.Text = "天线";
            // 
            // columnHeader_rssi
            // 
            this.columnHeader_rssi.Text = "RSSI";
            this.columnHeader_rssi.Width = 80;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(4, 4);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(4);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.listView_tags);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tabControl1);
            this.splitContainer1.Size = new System.Drawing.Size(970, 385);
            this.splitContainer1.SplitterDistance = 441;
            this.splitContainer1.SplitterWidth = 10;
            this.splitContainer1.TabIndex = 2;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage_tag);
            this.tabControl1.Controls.Add(this.tabPage_record);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(519, 385);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage_tag
            // 
            this.tabPage_tag.Controls.Add(this.chipEditor1);
            this.tabPage_tag.Location = new System.Drawing.Point(4, 31);
            this.tabPage_tag.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_tag.Name = "tabPage_tag";
            this.tabPage_tag.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage_tag.Size = new System.Drawing.Size(511, 350);
            this.tabPage_tag.TabIndex = 0;
            this.tabPage_tag.Text = "RFID 标签";
            this.tabPage_tag.UseVisualStyleBackColor = true;
            // 
            // chipEditor1
            // 
            this.chipEditor1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.chipEditor1.Location = new System.Drawing.Point(4, 4);
            this.chipEditor1.LogicChipItem = null;
            this.chipEditor1.Margin = new System.Windows.Forms.Padding(5);
            this.chipEditor1.Name = "chipEditor1";
            this.chipEditor1.Size = new System.Drawing.Size(503, 342);
            this.chipEditor1.TabIndex = 0;
            this.chipEditor1.TitleVisible = true;
            // 
            // tabPage_record
            // 
            this.tabPage_record.Controls.Add(this.propertyGrid_record);
            this.tabPage_record.Location = new System.Drawing.Point(4, 31);
            this.tabPage_record.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_record.Name = "tabPage_record";
            this.tabPage_record.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage_record.Size = new System.Drawing.Size(511, 350);
            this.tabPage_record.TabIndex = 1;
            this.tabPage_record.Text = "数据记录";
            this.tabPage_record.UseVisualStyleBackColor = true;
            // 
            // propertyGrid_record
            // 
            this.propertyGrid_record.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid_record.Location = new System.Drawing.Point(4, 4);
            this.propertyGrid_record.Margin = new System.Windows.Forms.Padding(4);
            this.propertyGrid_record.Name = "propertyGrid_record";
            this.propertyGrid_record.Size = new System.Drawing.Size(503, 342);
            this.propertyGrid_record.TabIndex = 1;
            // 
            // panel_okCancel
            // 
            this.panel_okCancel.Controls.Add(this.button_Cancel);
            this.panel_okCancel.Controls.Add(this.button_OK);
            this.panel_okCancel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel_okCancel.Location = new System.Drawing.Point(4, 397);
            this.panel_okCancel.Margin = new System.Windows.Forms.Padding(4);
            this.panel_okCancel.Name = "panel_okCancel";
            this.panel_okCancel.Size = new System.Drawing.Size(970, 44);
            this.panel_okCancel.TabIndex = 3;
            this.panel_okCancel.Visible = false;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(864, 0);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(4);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(103, 38);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Enabled = false;
            this.button_OK.Location = new System.Drawing.Point(754, 0);
            this.button_OK.Margin = new System.Windows.Forms.Padding(4);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(103, 38);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.panel_okCancel, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.splitContainer1, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label_message, 0, 2);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 38);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(978, 487);
            this.tableLayoutPanel1.TabIndex = 4;
            // 
            // label_message
            // 
            this.label_message.AutoSize = true;
            this.label_message.BackColor = System.Drawing.Color.DarkRed;
            this.label_message.Dock = System.Windows.Forms.DockStyle.Fill;
            this.label_message.Font = new System.Drawing.Font("宋体", 18F);
            this.label_message.ForeColor = System.Drawing.Color.White;
            this.label_message.Location = new System.Drawing.Point(4, 445);
            this.label_message.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label_message.Name = "label_message";
            this.label_message.Size = new System.Drawing.Size(970, 42);
            this.label_message.TabIndex = 4;
            this.label_message.Visible = false;
            // 
            // RfidToolForm
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(978, 525);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.toolStrip1);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "RfidToolForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "RFID 工具";
            this.Activated += new System.EventHandler(this.RfidToolForm_Activated);
            this.Deactivate += new System.EventHandler(this.RfidToolForm_Deactivate);
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
            this.panel_okCancel.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
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
        private System.Windows.Forms.Panel panel_okCancel;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ColumnHeader columnHeader_protocol;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton toolStripButton_autoFixEas;
        private System.Windows.Forms.Label label_message;
        private System.Windows.Forms.ColumnHeader columnHeader_antenna;
        private System.Windows.Forms.ColumnHeader columnHeader_rssi;
    }
}