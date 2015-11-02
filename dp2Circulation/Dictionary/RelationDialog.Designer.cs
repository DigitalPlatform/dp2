namespace dp2Circulation
{
    partial class RelationDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RelationDialog));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.splitContainer_relation = new System.Windows.Forms.SplitContainer();
            this.flowLayoutPanel_relationList = new System.Windows.Forms.FlowLayoutPanel();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_stop = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_upLevel = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_downLevel = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel_currentKey = new System.Windows.Forms.ToolStripLabel();
            this.toolStripButton_wild = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel_message = new System.Windows.Forms.ToolStripLabel();
            this.dpTable1 = new DigitalPlatform.CommonControl.DpTable();
            this.dpColumn_key = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_related = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_weight = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_level = new DigitalPlatform.CommonControl.DpColumn();
            this.splitContainer_main = new System.Windows.Forms.SplitContainer();
            this.webBrowser1 = new System.Windows.Forms.WebBrowser();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_relation)).BeginInit();
            this.splitContainer_relation.Panel1.SuspendLayout();
            this.splitContainer_relation.Panel2.SuspendLayout();
            this.splitContainer_relation.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).BeginInit();
            this.splitContainer_main.Panel1.SuspendLayout();
            this.splitContainer_main.Panel2.SuspendLayout();
            this.splitContainer_main.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(401, 335);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 3;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(320, 335);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 2;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // splitContainer_relation
            // 
            this.splitContainer_relation.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer_relation.Location = new System.Drawing.Point(0, 0);
            this.splitContainer_relation.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_relation.Name = "splitContainer_relation";
            // 
            // splitContainer_relation.Panel1
            // 
            this.splitContainer_relation.Panel1.Controls.Add(this.flowLayoutPanel_relationList);
            // 
            // splitContainer_relation.Panel2
            // 
            this.splitContainer_relation.Panel2.Controls.Add(this.dpTable1);
            this.splitContainer_relation.Size = new System.Drawing.Size(470, 172);
            this.splitContainer_relation.SplitterDistance = 155;
            this.splitContainer_relation.SplitterWidth = 8;
            this.splitContainer_relation.TabIndex = 4;
            // 
            // flowLayoutPanel_relationList
            // 
            this.flowLayoutPanel_relationList.AutoScroll = true;
            this.flowLayoutPanel_relationList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel_relationList.Location = new System.Drawing.Point(0, 0);
            this.flowLayoutPanel_relationList.Margin = new System.Windows.Forms.Padding(0);
            this.flowLayoutPanel_relationList.Name = "flowLayoutPanel_relationList";
            this.flowLayoutPanel_relationList.Size = new System.Drawing.Size(155, 172);
            this.flowLayoutPanel_relationList.TabIndex = 0;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip1.AutoSize = false;
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_stop,
            this.toolStripButton_upLevel,
            this.toolStripButton_downLevel,
            this.toolStripLabel_currentKey,
            this.toolStripButton_wild,
            this.toolStripLabel_message});
            this.toolStrip1.Location = new System.Drawing.Point(9, 336);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(305, 25);
            this.toolStrip1.TabIndex = 5;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_stop
            // 
            this.toolStripButton_stop.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_stop.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_stop.Image")));
            this.toolStripButton_stop.ImageTransparentColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(193)))));
            this.toolStripButton_stop.Name = "toolStripButton_stop";
            this.toolStripButton_stop.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_stop.Text = "停止";
            // 
            // toolStripButton_upLevel
            // 
            this.toolStripButton_upLevel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_upLevel.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_upLevel.Image")));
            this.toolStripButton_upLevel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_upLevel.Name = "toolStripButton_upLevel";
            this.toolStripButton_upLevel.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_upLevel.Text = "上级";
            // 
            // toolStripButton_downLevel
            // 
            this.toolStripButton_downLevel.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.toolStripButton_downLevel.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_downLevel.Image")));
            this.toolStripButton_downLevel.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_downLevel.Name = "toolStripButton_downLevel";
            this.toolStripButton_downLevel.Size = new System.Drawing.Size(23, 22);
            this.toolStripButton_downLevel.Text = "下级";
            // 
            // toolStripLabel_currentKey
            // 
            this.toolStripLabel_currentKey.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel_currentKey.Name = "toolStripLabel_currentKey";
            this.toolStripLabel_currentKey.Size = new System.Drawing.Size(0, 22);
            // 
            // toolStripButton_wild
            // 
            this.toolStripButton_wild.CheckOnClick = true;
            this.toolStripButton_wild.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_wild.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_wild.Image")));
            this.toolStripButton_wild.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_wild.Name = "toolStripButton_wild";
            this.toolStripButton_wild.Size = new System.Drawing.Size(60, 22);
            this.toolStripButton_wild.Text = "前方一致";
            // 
            // toolStripLabel_message
            // 
            this.toolStripLabel_message.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel_message.Name = "toolStripLabel_message";
            this.toolStripLabel_message.Size = new System.Drawing.Size(17, 22);
            this.toolStripLabel_message.Text = "...";
            // 
            // dpTable1
            // 
            this.dpTable1.AutoDocCenter = true;
            this.dpTable1.BackColor = System.Drawing.SystemColors.Window;
            this.dpTable1.Columns.Add(this.dpColumn_key);
            this.dpTable1.Columns.Add(this.dpColumn_related);
            this.dpTable1.Columns.Add(this.dpColumn_weight);
            this.dpTable1.Columns.Add(this.dpColumn_level);
            this.dpTable1.ColumnsBackColor = System.Drawing.SystemColors.Control;
            this.dpTable1.ColumnsForeColor = System.Drawing.SystemColors.ControlText;
            this.dpTable1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dpTable1.DocumentBorderColor = System.Drawing.SystemColors.ControlDark;
            this.dpTable1.DocumentOrgX = ((long)(0));
            this.dpTable1.DocumentOrgY = ((long)(0));
            this.dpTable1.DocumentShadowColor = System.Drawing.SystemColors.ControlDarkDark;
            this.dpTable1.FocusedItem = null;
            this.dpTable1.ForeColor = System.Drawing.SystemColors.WindowText;
            this.dpTable1.FullRowSelect = true;
            this.dpTable1.HighlightBackColor = System.Drawing.SystemColors.Highlight;
            this.dpTable1.HightlightForeColor = System.Drawing.SystemColors.HighlightText;
            this.dpTable1.HoverBackColor = System.Drawing.SystemColors.HotTrack;
            this.dpTable1.InactiveHighlightBackColor = System.Drawing.SystemColors.InactiveCaption;
            this.dpTable1.InactiveHightlightForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.dpTable1.Location = new System.Drawing.Point(0, 0);
            this.dpTable1.Margin = new System.Windows.Forms.Padding(0);
            this.dpTable1.Name = "dpTable1";
            this.dpTable1.Padding = new System.Windows.Forms.Padding(8);
            this.dpTable1.Size = new System.Drawing.Size(307, 172);
            this.dpTable1.TabIndex = 0;
            this.dpTable1.Text = "dpTable1";
            this.dpTable1.SelectionChanged += new System.EventHandler(this.dpTable1_SelectionChanged);
            this.dpTable1.PaintRegion += new DigitalPlatform.CommonControl.PaintRegionEventHandler(this.dpTable1_PaintRegion);
            this.dpTable1.PaintBack += new DigitalPlatform.CommonControl.PaintBackEventHandler(this.dpTable1_PaintBack);
            // 
            // dpColumn_key
            // 
            this.dpColumn_key.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_key.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_key.Font = null;
            this.dpColumn_key.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_key.LineAlignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_key.Text = "键";
            // 
            // dpColumn_related
            // 
            this.dpColumn_related.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_related.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_related.Font = null;
            this.dpColumn_related.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_related.LineAlignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_related.Text = "关联项";
            // 
            // dpColumn_weight
            // 
            this.dpColumn_weight.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_weight.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_weight.Font = null;
            this.dpColumn_weight.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_weight.LineAlignment = System.Drawing.StringAlignment.Far;
            this.dpColumn_weight.Text = "权值";
            this.dpColumn_weight.Width = 50;
            // 
            // dpColumn_level
            // 
            this.dpColumn_level.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_level.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_level.Font = null;
            this.dpColumn_level.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_level.LineAlignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_level.Text = "级次";
            this.dpColumn_level.Width = 50;
            // 
            // splitContainer_main
            // 
            this.splitContainer_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer_main.Location = new System.Drawing.Point(9, 9);
            this.splitContainer_main.Margin = new System.Windows.Forms.Padding(0);
            this.splitContainer_main.Name = "splitContainer_main";
            this.splitContainer_main.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer_main.Panel1
            // 
            this.splitContainer_main.Panel1.Controls.Add(this.splitContainer_relation);
            // 
            // splitContainer_main.Panel2
            // 
            this.splitContainer_main.Panel2.Controls.Add(this.webBrowser1);
            this.splitContainer_main.Size = new System.Drawing.Size(470, 323);
            this.splitContainer_main.SplitterDistance = 172;
            this.splitContainer_main.SplitterWidth = 8;
            this.splitContainer_main.TabIndex = 6;
            // 
            // webBrowser1
            // 
            this.webBrowser1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.webBrowser1.Location = new System.Drawing.Point(0, 0);
            this.webBrowser1.MinimumSize = new System.Drawing.Size(20, 20);
            this.webBrowser1.Name = "webBrowser1";
            this.webBrowser1.Size = new System.Drawing.Size(470, 143);
            this.webBrowser1.TabIndex = 0;
            // 
            // RelationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(488, 370);
            this.Controls.Add(this.splitContainer_main);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "RelationDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "RelationDialog";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.RelationDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.RelationDialog_FormClosed);
            this.Load += new System.EventHandler(this.RelationDialog_Load);
            this.splitContainer_relation.Panel1.ResumeLayout(false);
            this.splitContainer_relation.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_relation)).EndInit();
            this.splitContainer_relation.ResumeLayout(false);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.splitContainer_main.Panel1.ResumeLayout(false);
            this.splitContainer_main.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer_main)).EndInit();
            this.splitContainer_main.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.SplitContainer splitContainer_relation;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel_relationList;
        private DigitalPlatform.CommonControl.DpTable dpTable1;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_key;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_related;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_weight;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_stop;
        private System.Windows.Forms.ToolStripButton toolStripButton_upLevel;
        private System.Windows.Forms.ToolStripButton toolStripButton_downLevel;
        private System.Windows.Forms.ToolStripLabel toolStripLabel_currentKey;
        private System.Windows.Forms.ToolStripButton toolStripButton_wild;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_level;
        private System.Windows.Forms.ToolStripLabel toolStripLabel_message;
        private System.Windows.Forms.SplitContainer splitContainer_main;
        private System.Windows.Forms.WebBrowser webBrowser1;
    }
}