namespace dp2Circulation
{
    partial class SelectItemDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectItemDialog));
            this.comboBox_from = new System.Windows.Forms.ComboBox();
            this.textBox_queryWord = new System.Windows.Forms.TextBox();
            this.button_search = new System.Windows.Forms.Button();
            this.dpTable_items = new DigitalPlatform.CommonControl.DpTable();
            this.dpColumn_state = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_itemBarcode = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_borrowInfo = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_summary = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_volume = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_location = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_price = new DigitalPlatform.CommonControl.DpColumn();
            this.dpColumn_itemRecPath = new DigitalPlatform.CommonControl.DpColumn();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_stop = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // comboBox_from
            // 
            this.comboBox_from.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.comboBox_from.FormattingEnabled = true;
            this.comboBox_from.Location = new System.Drawing.Point(12, 12);
            this.comboBox_from.Name = "comboBox_from";
            this.comboBox_from.Size = new System.Drawing.Size(121, 20);
            this.comboBox_from.TabIndex = 0;
            // 
            // textBox_queryWord
            // 
            this.textBox_queryWord.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_queryWord.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_queryWord.Location = new System.Drawing.Point(139, 13);
            this.textBox_queryWord.Name = "textBox_queryWord";
            this.textBox_queryWord.Size = new System.Drawing.Size(132, 21);
            this.textBox_queryWord.TabIndex = 1;
            // 
            // button_search
            // 
            this.button_search.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_search.Image = ((System.Drawing.Image)(resources.GetObject("button_search.Image")));
            this.button_search.Location = new System.Drawing.Point(277, 10);
            this.button_search.Name = "button_search";
            this.button_search.Size = new System.Drawing.Size(75, 23);
            this.button_search.TabIndex = 2;
            this.button_search.Text = "检索(&S)";
            this.button_search.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.button_search.UseVisualStyleBackColor = true;
            this.button_search.Click += new System.EventHandler(this.button_search_Click);
            // 
            // dpTable_items
            // 
            this.dpTable_items.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dpTable_items.AutoDocCenter = true;
            this.dpTable_items.BackColor = System.Drawing.SystemColors.Window;
            this.dpTable_items.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dpTable_items.Columns.Add(this.dpColumn_state);
            this.dpTable_items.Columns.Add(this.dpColumn_itemBarcode);
            this.dpTable_items.Columns.Add(this.dpColumn_borrowInfo);
            this.dpTable_items.Columns.Add(this.dpColumn_summary);
            this.dpTable_items.Columns.Add(this.dpColumn_volume);
            this.dpTable_items.Columns.Add(this.dpColumn_location);
            this.dpTable_items.Columns.Add(this.dpColumn_price);
            this.dpTable_items.Columns.Add(this.dpColumn_itemRecPath);
            this.dpTable_items.ColumnsBackColor = System.Drawing.SystemColors.Control;
            this.dpTable_items.ColumnsForeColor = System.Drawing.SystemColors.ControlText;
            this.dpTable_items.DocumentBorderColor = System.Drawing.SystemColors.Window;
            this.dpTable_items.DocumentOrgX = ((long)(0));
            this.dpTable_items.DocumentOrgY = ((long)(0));
            this.dpTable_items.DocumentShadowColor = System.Drawing.SystemColors.Window;
            this.dpTable_items.FocusedItem = null;
            this.dpTable_items.ForeColor = System.Drawing.SystemColors.WindowText;
            this.dpTable_items.FullRowSelect = true;
            this.dpTable_items.HighlightBackColor = System.Drawing.SystemColors.Highlight;
            this.dpTable_items.HightlightForeColor = System.Drawing.SystemColors.HighlightText;
            this.dpTable_items.HoverBackColor = System.Drawing.SystemColors.HotTrack;
            this.dpTable_items.InactiveHighlightBackColor = System.Drawing.SystemColors.InactiveCaption;
            this.dpTable_items.InactiveHightlightForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.dpTable_items.Location = new System.Drawing.Point(13, 39);
            this.dpTable_items.Name = "dpTable_items";
            this.dpTable_items.Size = new System.Drawing.Size(390, 184);
            this.dpTable_items.TabIndex = 4;
            this.dpTable_items.SelectionChanged += new System.EventHandler(this.dpTable_items_SelectionChanged);
            this.dpTable_items.PaintBack += new DigitalPlatform.CommonControl.PaintBackEventHandler(this.dpTable_items_PaintBack);
            this.dpTable_items.DoubleClick += new System.EventHandler(this.dpTable_items_DoubleClick);
            this.dpTable_items.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.dpTable_items_KeyPress);
            this.dpTable_items.MouseUp += new System.Windows.Forms.MouseEventHandler(this.dpTable_items_MouseUp);
            // 
            // dpColumn_state
            // 
            this.dpColumn_state.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_state.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_state.Font = null;
            this.dpColumn_state.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_state.Text = "状态";
            this.dpColumn_state.Width = 70;
            // 
            // dpColumn_itemBarcode
            // 
            this.dpColumn_itemBarcode.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_itemBarcode.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_itemBarcode.Font = null;
            this.dpColumn_itemBarcode.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_itemBarcode.Text = "册条码号";
            // 
            // dpColumn_borrowInfo
            // 
            this.dpColumn_borrowInfo.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_borrowInfo.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_borrowInfo.Font = null;
            this.dpColumn_borrowInfo.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_borrowInfo.Text = "在借情况";
            // 
            // dpColumn_summary
            // 
            this.dpColumn_summary.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_summary.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_summary.Font = null;
            this.dpColumn_summary.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_summary.Text = "书目摘要";
            this.dpColumn_summary.Width = 200;
            // 
            // dpColumn_volume
            // 
            this.dpColumn_volume.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_volume.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_volume.Font = null;
            this.dpColumn_volume.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_volume.Text = "卷册";
            this.dpColumn_volume.Width = 50;
            // 
            // dpColumn_location
            // 
            this.dpColumn_location.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_location.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_location.Font = null;
            this.dpColumn_location.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_location.Text = "馆藏地";
            // 
            // dpColumn_price
            // 
            this.dpColumn_price.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_price.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_price.Font = null;
            this.dpColumn_price.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_price.Text = "价格";
            // 
            // dpColumn_itemRecPath
            // 
            this.dpColumn_itemRecPath.Alignment = System.Drawing.StringAlignment.Near;
            this.dpColumn_itemRecPath.BackColor = System.Drawing.Color.Transparent;
            this.dpColumn_itemRecPath.Font = null;
            this.dpColumn_itemRecPath.ForeColor = System.Drawing.Color.Transparent;
            this.dpColumn_itemRecPath.Text = "册记录路径";
            this.dpColumn_itemRecPath.Width = 150;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Enabled = false;
            this.button_OK.Location = new System.Drawing.Point(247, 229);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(328, 229);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_stop
            // 
            this.button_stop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_stop.Enabled = false;
            this.button_stop.Location = new System.Drawing.Point(351, 10);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(52, 23);
            this.button_stop.TabIndex = 3;
            this.button_stop.Text = "停止";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.statusStrip1.AutoSize = false;
            this.statusStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(13, 229);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(231, 23);
            this.statusStrip1.SizingGrip = false;
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.AutoSize = false;
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(100, 17);
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(0, 18);
            // 
            // SelectItemDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlDark;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(415, 264);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.dpTable_items);
            this.Controls.Add(this.comboBox_from);
            this.Controls.Add(this.textBox_queryWord);
            this.Controls.Add(this.button_search);
            this.Name = "SelectItemDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "选择册记录";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SelectItemDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SelectItemDialog_FormClosed);
            this.Load += new System.EventHandler(this.SelectItemDialog_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ComboBox comboBox_from;
        private System.Windows.Forms.TextBox textBox_queryWord;
        private System.Windows.Forms.Button button_search;
        private DigitalPlatform.CommonControl.DpTable dpTable_items;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_itemBarcode;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_borrowInfo;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_summary;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_volume;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_location;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_price;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_itemRecPath;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private DigitalPlatform.CommonControl.DpColumn dpColumn_state;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
    }
}