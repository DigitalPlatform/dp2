namespace dp2Circulation
{
    partial class SelectOutputRangeDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SelectOutputRangeDialog));
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader_batchNo = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_targetLocation = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_itemCount = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_selectAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_clearAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripButton_outputOneSheet = new System.Windows.Forms.ToolStripButton();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(607, 444);
            this.button_OK.Margin = new System.Windows.Forms.Padding(4);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(103, 38);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(717, 444);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(4);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(103, 38);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.CheckBoxes = true;
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_batchNo,
            this.columnHeader_targetLocation,
            this.columnHeader_itemCount});
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(13, 13);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(808, 424);
            this.listView1.TabIndex = 8;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.listView1_ColumnClick);
            this.listView1.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listView1_ItemChecked);
            // 
            // columnHeader_batchNo
            // 
            this.columnHeader_batchNo.Text = "批次号";
            this.columnHeader_batchNo.Width = 300;
            // 
            // columnHeader_targetLocation
            // 
            this.columnHeader_targetLocation.Text = "目标馆藏地";
            this.columnHeader_targetLocation.Width = 200;
            // 
            // columnHeader_itemCount
            // 
            this.columnHeader_itemCount.Text = "册数";
            this.columnHeader_itemCount.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader_itemCount.Width = 100;
            // 
            // toolStrip1
            // 
            this.toolStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_selectAll,
            this.toolStripButton_clearAll,
            this.toolStripSeparator1,
            this.toolStripButton_outputOneSheet});
            this.toolStrip1.Location = new System.Drawing.Point(13, 446);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(390, 38);
            this.toolStrip1.TabIndex = 9;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // toolStripButton_selectAll
            // 
            this.toolStripButton_selectAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_selectAll.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_selectAll.Image")));
            this.toolStripButton_selectAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_selectAll.Name = "toolStripButton_selectAll";
            this.toolStripButton_selectAll.Size = new System.Drawing.Size(58, 32);
            this.toolStripButton_selectAll.Text = "全选";
            this.toolStripButton_selectAll.Click += new System.EventHandler(this.toolStripButton_selectAll_Click);
            // 
            // toolStripButton_clearAll
            // 
            this.toolStripButton_clearAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_clearAll.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_clearAll.Image")));
            this.toolStripButton_clearAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_clearAll.Name = "toolStripButton_clearAll";
            this.toolStripButton_clearAll.Size = new System.Drawing.Size(142, 32);
            this.toolStripButton_clearAll.Text = "清除全部选择";
            this.toolStripButton_clearAll.Click += new System.EventHandler(this.toolStripButton_clearAll_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 38);
            // 
            // toolStripButton_outputOneSheet
            // 
            this.toolStripButton_outputOneSheet.CheckOnClick = true;
            this.toolStripButton_outputOneSheet.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_outputOneSheet.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_outputOneSheet.Image")));
            this.toolStripButton_outputOneSheet.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_outputOneSheet.Name = "toolStripButton_outputOneSheet";
            this.toolStripButton_outputOneSheet.Size = new System.Drawing.Size(163, 32);
            this.toolStripButton_outputOneSheet.Text = "输出为一个表单";
            // 
            // SelectOutputRangeDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(833, 495);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.button_Cancel);
            this.Name = "SelectOutputRangeDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "选择输出范围";
            this.Load += new System.EventHandler(this.SelectOutputRangeDialog_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader_batchNo;
        private System.Windows.Forms.ColumnHeader columnHeader_targetLocation;
        private System.Windows.Forms.ColumnHeader columnHeader_itemCount;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_selectAll;
        private System.Windows.Forms.ToolStripButton toolStripButton_clearAll;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripButton toolStripButton_outputOneSheet;
    }
}