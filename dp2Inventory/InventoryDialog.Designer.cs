
namespace dp2Inventory
{
    partial class InventoryDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InventoryDialog));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_begin = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_exportUidPiiMap = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_stop = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_nextScan = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_clearList = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_test_nextTags = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_cancelSpeaking = new System.Windows.Forms.ToolStripButton();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStripStatusLabel_currentShelfNo = new System.Windows.Forms.ToolStripStatusLabel();
            this.listView_tags = new System.Windows.Forms.ListView();
            this.columnHeader_uid = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_errorInfo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_pii = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_title = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_currentLocation = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_location = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_state = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_tu = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_oi = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_aoi = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_eas = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_afi = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_readerName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_antenna = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_protocol = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStripStatusLabel2 = new System.Windows.Forms.ToolStripStatusLabel();
            this.toolStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // toolStrip1
            // 
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_begin,
            this.toolStripSeparator1,
            this.toolStripButton_exportUidPiiMap,
            this.toolStripButton_stop,
            this.toolStripSeparator2,
            this.toolStripButton_nextScan,
            this.toolStripButton_clearList,
            this.toolStripButton_test_nextTags,
            this.toolStripButton_cancelSpeaking});
            this.toolStrip1.Location = new System.Drawing.Point(0, 0);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(951, 44);
            this.toolStrip1.TabIndex = 0;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_begin
            // 
            this.toolStripButton_begin.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_begin.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_begin.Image")));
            this.toolStripButton_begin.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_begin.Name = "toolStripButton_begin";
            this.toolStripButton_begin.Size = new System.Drawing.Size(58, 38);
            this.toolStripButton_begin.Text = "开始";
            this.toolStripButton_begin.ToolTipText = "开始修改";
            this.toolStripButton_begin.Click += new System.EventHandler(this.toolStripButton_begin_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 44);
            // 
            // toolStripButton_exportUidPiiMap
            // 
            this.toolStripButton_exportUidPiiMap.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripButton_exportUidPiiMap.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_exportUidPiiMap.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_exportUidPiiMap.Image")));
            this.toolStripButton_exportUidPiiMap.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_exportUidPiiMap.Name = "toolStripButton_exportUidPiiMap";
            this.toolStripButton_exportUidPiiMap.Size = new System.Drawing.Size(142, 32);
            this.toolStripButton_exportUidPiiMap.Text = "导出对照关系";
            this.toolStripButton_exportUidPiiMap.Visible = false;
            this.toolStripButton_exportUidPiiMap.Click += new System.EventHandler(this.toolStripButton_exportUidPiiMap_Click);
            // 
            // toolStripButton_stop
            // 
            this.toolStripButton_stop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_stop.Enabled = false;
            this.toolStripButton_stop.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_stop.Image")));
            this.toolStripButton_stop.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_stop.Name = "toolStripButton_stop";
            this.toolStripButton_stop.Size = new System.Drawing.Size(58, 38);
            this.toolStripButton_stop.Text = "停止";
            this.toolStripButton_stop.ToolTipText = "停止修改";
            this.toolStripButton_stop.Click += new System.EventHandler(this.toolStripButton_stop_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 44);
            // 
            // toolStripButton_nextScan
            // 
            this.toolStripButton_nextScan.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_nextScan.Enabled = false;
            this.toolStripButton_nextScan.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_nextScan.Image")));
            this.toolStripButton_nextScan.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_nextScan.Name = "toolStripButton_nextScan";
            this.toolStripButton_nextScan.Size = new System.Drawing.Size(58, 38);
            this.toolStripButton_nextScan.Text = "跳过";
            this.toolStripButton_nextScan.ToolTipText = "跳过本次扫描";
            this.toolStripButton_nextScan.Click += new System.EventHandler(this.toolStripButton_nextScan_Click);
            // 
            // toolStripButton_clearList
            // 
            this.toolStripButton_clearList.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_clearList.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_clearList.Image")));
            this.toolStripButton_clearList.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_clearList.Name = "toolStripButton_clearList";
            this.toolStripButton_clearList.Size = new System.Drawing.Size(100, 38);
            this.toolStripButton_clearList.Text = "清空列表";
            this.toolStripButton_clearList.Click += new System.EventHandler(this.toolStripButton_clearList_Click);
            // 
            // toolStripButton_test_nextTags
            // 
            this.toolStripButton_test_nextTags.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_test_nextTags.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_test_nextTags.Image")));
            this.toolStripButton_test_nextTags.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_test_nextTags.Name = "toolStripButton_test_nextTags";
            this.toolStripButton_test_nextTags.Size = new System.Drawing.Size(177, 32);
            this.toolStripButton_test_nextTags.Text = "(测试)下一组标签";
            this.toolStripButton_test_nextTags.Visible = false;
            this.toolStripButton_test_nextTags.Click += new System.EventHandler(this.toolStripButton_test_nextTags_Click);
            // 
            // toolStripButton_cancelSpeaking
            // 
            this.toolStripButton_cancelSpeaking.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_cancelSpeaking.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_cancelSpeaking.Image")));
            this.toolStripButton_cancelSpeaking.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_cancelSpeaking.Name = "toolStripButton_cancelSpeaking";
            this.toolStripButton_cancelSpeaking.Size = new System.Drawing.Size(100, 38);
            this.toolStripButton_cancelSpeaking.Text = "中断语音";
            this.toolStripButton_cancelSpeaking.Click += new System.EventHandler(this.toolStripButton_cancelSpeaking_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1,
            this.toolStripStatusLabel_currentShelfNo,
            this.toolStripStatusLabel2});
            this.statusStrip1.Location = new System.Drawing.Point(0, 539);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(951, 41);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(706, 28);
            this.toolStripStatusLabel1.Spring = true;
            this.toolStripStatusLabel1.Text = "...";
            this.toolStripStatusLabel1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // toolStripStatusLabel_currentShelfNo
            // 
            this.toolStripStatusLabel_currentShelfNo.BorderSides = ((System.Windows.Forms.ToolStripStatusLabelBorderSides)((((System.Windows.Forms.ToolStripStatusLabelBorderSides.Left | System.Windows.Forms.ToolStripStatusLabelBorderSides.Top) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Right) 
            | System.Windows.Forms.ToolStripStatusLabelBorderSides.Bottom)));
            this.toolStripStatusLabel_currentShelfNo.Name = "toolStripStatusLabel_currentShelfNo";
            this.toolStripStatusLabel_currentShelfNo.Overflow = System.Windows.Forms.ToolStripItemOverflow.Never;
            this.toolStripStatusLabel_currentShelfNo.Size = new System.Drawing.Size(21, 32);
            this.toolStripStatusLabel_currentShelfNo.Text = ".";
            // 
            // listView_tags
            // 
            this.listView_tags.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_uid,
            this.columnHeader_errorInfo,
            this.columnHeader_pii,
            this.columnHeader_title,
            this.columnHeader_currentLocation,
            this.columnHeader_location,
            this.columnHeader_state,
            this.columnHeader_tu,
            this.columnHeader_oi,
            this.columnHeader_aoi,
            this.columnHeader_eas,
            this.columnHeader_afi,
            this.columnHeader_readerName,
            this.columnHeader_antenna,
            this.columnHeader_protocol});
            this.listView_tags.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_tags.FullRowSelect = true;
            this.listView_tags.HideSelection = false;
            this.listView_tags.Location = new System.Drawing.Point(0, 44);
            this.listView_tags.Name = "listView_tags";
            this.listView_tags.Size = new System.Drawing.Size(951, 495);
            this.listView_tags.TabIndex = 2;
            this.listView_tags.UseCompatibleStateImageBehavior = false;
            this.listView_tags.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_uid
            // 
            this.columnHeader_uid.Text = "UID";
            this.columnHeader_uid.Width = 120;
            // 
            // columnHeader_errorInfo
            // 
            this.columnHeader_errorInfo.Text = "错误信息";
            this.columnHeader_errorInfo.Width = 205;
            // 
            // columnHeader_pii
            // 
            this.columnHeader_pii.Text = "PII";
            this.columnHeader_pii.Width = 166;
            // 
            // columnHeader_title
            // 
            this.columnHeader_title.Text = "题名";
            this.columnHeader_title.Width = 180;
            // 
            // columnHeader_currentLocation
            // 
            this.columnHeader_currentLocation.Text = "当前位置";
            this.columnHeader_currentLocation.Width = 200;
            // 
            // columnHeader_location
            // 
            this.columnHeader_location.Text = "永久位置";
            this.columnHeader_location.Width = 200;
            // 
            // columnHeader_state
            // 
            this.columnHeader_state.Text = "状态";
            this.columnHeader_state.Width = 142;
            // 
            // columnHeader_tu
            // 
            this.columnHeader_tu.Text = "TU(应用类别)";
            this.columnHeader_tu.Width = 151;
            // 
            // columnHeader_oi
            // 
            this.columnHeader_oi.Text = "OI(机构代码)";
            this.columnHeader_oi.Width = 163;
            // 
            // columnHeader_aoi
            // 
            this.columnHeader_aoi.Text = "AOI(非标机构代码)";
            this.columnHeader_aoi.Width = 160;
            // 
            // columnHeader_eas
            // 
            this.columnHeader_eas.Text = "EAS";
            this.columnHeader_eas.Width = 89;
            // 
            // columnHeader_afi
            // 
            this.columnHeader_afi.Text = "AFI";
            this.columnHeader_afi.Width = 100;
            // 
            // columnHeader_readerName
            // 
            this.columnHeader_readerName.Text = "读写器";
            this.columnHeader_readerName.Width = 100;
            // 
            // columnHeader_antenna
            // 
            this.columnHeader_antenna.Text = "天线编号";
            this.columnHeader_antenna.Width = 100;
            // 
            // columnHeader_protocol
            // 
            this.columnHeader_protocol.Text = "协议";
            this.columnHeader_protocol.Width = 120;
            // 
            // toolStripStatusLabel2
            // 
            this.toolStripStatusLabel2.Name = "toolStripStatusLabel2";
            this.toolStripStatusLabel2.Size = new System.Drawing.Size(17, 32);
            this.toolStripStatusLabel2.Text = ".";
            // 
            // InventoryDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(951, 580);
            this.Controls.Add(this.listView_tags);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.toolStrip1);
            this.Name = "InventoryDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "修改标签";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.InventoryDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.InventoryDialog_FormClosed);
            this.Load += new System.EventHandler(this.InventoryDialog_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ListView listView_tags;
        private System.Windows.Forms.ColumnHeader columnHeader_uid;
        private System.Windows.Forms.ColumnHeader columnHeader_errorInfo;
        private System.Windows.Forms.ColumnHeader columnHeader_pii;
        private System.Windows.Forms.ColumnHeader columnHeader_tu;
        private System.Windows.Forms.ColumnHeader columnHeader_oi;
        private System.Windows.Forms.ColumnHeader columnHeader_eas;
        private System.Windows.Forms.ToolStripButton toolStripButton_begin;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_exportUidPiiMap;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        private System.Windows.Forms.ToolStripButton toolStripButton_stop;
        private System.Windows.Forms.ColumnHeader columnHeader_aoi;
        private System.Windows.Forms.ColumnHeader columnHeader_readerName;
        private System.Windows.Forms.ColumnHeader columnHeader_antenna;
        private System.Windows.Forms.ColumnHeader columnHeader_protocol;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton toolStripButton_clearList;
        private System.Windows.Forms.ToolStripButton toolStripButton_nextScan;
        private System.Windows.Forms.ColumnHeader columnHeader_afi;
        private System.Windows.Forms.ToolStripButton toolStripButton_test_nextTags;
        private System.Windows.Forms.ColumnHeader columnHeader_title;
        private System.Windows.Forms.ToolStripButton toolStripButton_cancelSpeaking;
        private System.Windows.Forms.ColumnHeader columnHeader_state;
        private System.Windows.Forms.ColumnHeader columnHeader_currentLocation;
        private System.Windows.Forms.ColumnHeader columnHeader_location;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_currentShelfNo;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel2;
    }
}