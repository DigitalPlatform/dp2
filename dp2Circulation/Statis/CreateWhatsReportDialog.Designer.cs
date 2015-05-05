namespace dp2Circulation
{
    partial class CreateWhatsReportDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateWhatsReportDialog));
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_dateRange = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.listView_reports = new System.Windows.Forms.ListView();
            this.columnHeader_report_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_freq = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_report_type = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.checkedComboBox_createFreq = new DigitalPlatform.CommonControl.CheckedComboBox();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.toolStripButton_selectAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripButton_clearAll = new System.Windows.Forms.ToolStripButton();
            this.toolStripLabel_info = new System.Windows.Forms.ToolStripLabel();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "日期范围(&D):";
            // 
            // textBox_dateRange
            // 
            this.textBox_dateRange.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_dateRange.Location = new System.Drawing.Point(101, 12);
            this.textBox_dateRange.Name = "textBox_dateRange";
            this.textBox_dateRange.Size = new System.Drawing.Size(179, 21);
            this.textBox_dateRange.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 40);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 2;
            this.label4.Text = "创建频率(&F):";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(307, 229);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(226, 229);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // listView_reports
            // 
            this.listView_reports.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_reports.CheckBoxes = true;
            this.listView_reports.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_report_name,
            this.columnHeader_freq,
            this.columnHeader_report_type});
            this.listView_reports.FullRowSelect = true;
            this.listView_reports.HideSelection = false;
            this.listView_reports.Location = new System.Drawing.Point(12, 65);
            this.listView_reports.Name = "listView_reports";
            this.listView_reports.Size = new System.Drawing.Size(370, 139);
            this.listView_reports.TabIndex = 4;
            this.listView_reports.UseCompatibleStateImageBehavior = false;
            this.listView_reports.View = System.Windows.Forms.View.Details;
            this.listView_reports.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listView_reports_ItemChecked);
            // 
            // columnHeader_report_name
            // 
            this.columnHeader_report_name.Text = "报表名";
            this.columnHeader_report_name.Width = 183;
            // 
            // columnHeader_freq
            // 
            this.columnHeader_freq.Text = "创建频率";
            this.columnHeader_freq.Width = 151;
            // 
            // columnHeader_report_type
            // 
            this.columnHeader_report_type.Text = "类型";
            // 
            // checkedComboBox_createFreq
            // 
            this.checkedComboBox_createFreq.BackColor = System.Drawing.SystemColors.Window;
            this.checkedComboBox_createFreq.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.checkedComboBox_createFreq.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkedComboBox_createFreq.Location = new System.Drawing.Point(101, 38);
            this.checkedComboBox_createFreq.Margin = new System.Windows.Forms.Padding(0);
            this.checkedComboBox_createFreq.Name = "checkedComboBox_createFreq";
            this.checkedComboBox_createFreq.Padding = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_createFreq.Size = new System.Drawing.Size(179, 24);
            this.checkedComboBox_createFreq.TabIndex = 3;
            this.checkedComboBox_createFreq.DropDown += new System.EventHandler(this.checkedComboBox_createFreq_DropDown);
            // 
            // toolStrip1
            // 
            this.toolStrip1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.toolStrip1.AutoSize = false;
            this.toolStrip1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripButton_selectAll,
            this.toolStripButton_clearAll,
            this.toolStripLabel_info});
            this.toolStrip1.Location = new System.Drawing.Point(12, 203);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(370, 25);
            this.toolStrip1.TabIndex = 5;
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
            // toolStripButton_clearAll
            // 
            this.toolStripButton_clearAll.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.toolStripButton_clearAll.Image = ((System.Drawing.Image)(resources.GetObject("toolStripButton_clearAll.Image")));
            this.toolStripButton_clearAll.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripButton_clearAll.Name = "toolStripButton_clearAll";
            this.toolStripButton_clearAll.Size = new System.Drawing.Size(48, 22);
            this.toolStripButton_clearAll.Text = "全清除";
            this.toolStripButton_clearAll.Click += new System.EventHandler(this.toolStripButton_clearAll_Click);
            // 
            // toolStripLabel_info
            // 
            this.toolStripLabel_info.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripLabel_info.Name = "toolStripLabel_info";
            this.toolStripLabel_info.Size = new System.Drawing.Size(0, 22);
            // 
            // CreateWhatsReportDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(394, 264);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.listView_reports);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.checkedComboBox_createFreq);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_dateRange);
            this.Controls.Add(this.label1);
            this.Name = "CreateWhatsReportDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "创建哪些报表?";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.CreateWhatsReportDialog_FormClosed);
            this.Load += new System.EventHandler(this.CreateWhatsReportDialog_Load);
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_dateRange;
        private DigitalPlatform.CommonControl.CheckedComboBox checkedComboBox_createFreq;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.ListView listView_reports;
        private System.Windows.Forms.ColumnHeader columnHeader_report_name;
        private System.Windows.Forms.ColumnHeader columnHeader_freq;
        private System.Windows.Forms.ColumnHeader columnHeader_report_type;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton toolStripButton_selectAll;
        private System.Windows.Forms.ToolStripButton toolStripButton_clearAll;
        private System.Windows.Forms.ToolStripLabel toolStripLabel_info;
    }
}