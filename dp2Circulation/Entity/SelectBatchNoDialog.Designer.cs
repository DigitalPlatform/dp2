namespace dp2Circulation
{
    partial class SelectBatchNoDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectBatchNoDialog));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.listView_records = new DigitalPlatform.GUI.ListViewNF();
            this.columnHeader_batchNo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_count = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_selectAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_unselectAll = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(274, 226);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 4;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(195, 226);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // listView_records
            // 
            this.listView_records.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_records.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_batchNo,
            this.columnHeader_count});
            this.listView_records.FullRowSelect = true;
            this.listView_records.HideSelection = false;
            this.listView_records.Location = new System.Drawing.Point(8, 8);
            this.listView_records.Margin = new System.Windows.Forms.Padding(0);
            this.listView_records.Name = "listView_records";
            this.listView_records.Size = new System.Drawing.Size(341, 189);
            this.listView_records.TabIndex = 12;
            this.listView_records.UseCompatibleStateImageBehavior = false;
            this.listView_records.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader_batchNo
            // 
            this.columnHeader_batchNo.Text = "批次号";
            this.columnHeader_batchNo.Width = 150;
            // 
            // columnHeader_count
            // 
            this.columnHeader_count.Text = "记录数";
            this.columnHeader_count.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_count.Width = 100;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_selectAll,
            this.toolStripButton_unselectAll});
            this.toolStrip1.Location = new System.Drawing.Point(8, 197);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(87, 25);
            this.toolStrip1.TabIndex = 13;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_selectAll
            // 
            this.toolStripButton_selectAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_selectAll.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_selectAll.Image")));
            this.toolStripButton_selectAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_selectAll.Name = "toolStripButton_selectAll";
            this.toolStripButton_selectAll.Size = new System.Drawing.Size(36, 22);
            this.toolStripButton_selectAll.Text = "全选";
            this.toolStripButton_selectAll.Click += new System.EventHandler(this.toolStripButton_selectAll_Click);
            // 
            // toolStripButton_unselectAll
            // 
            this.toolStripButton_unselectAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_unselectAll.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_unselectAll.Image")));
            this.toolStripButton_unselectAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_unselectAll.Name = "toolStripButton_unselectAll";
            this.toolStripButton_unselectAll.Size = new System.Drawing.Size(48, 22);
            this.toolStripButton_unselectAll.Text = "全清除";
            this.toolStripButton_unselectAll.Click += new System.EventHandler(this.toolStripButton_unselectAll_Click);
            // 
            // SelectBatchNoDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(360, 261);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.listView_records);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "SelectBatchNoDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "选择批次号";
            this.Load += new System.EventHandler(this.SelectBatchNoDialog_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private DigitalPlatform.GUI.ListViewNF listView_records;
        private System.Windows.Forms.ColumnHeader columnHeader_batchNo;
        private System.Windows.Forms.ColumnHeader columnHeader_count;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_selectAll;
        private System.Windows.Forms.ToolStripButton toolStripButton_unselectAll;
    }
}