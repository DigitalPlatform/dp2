namespace dp2Circulation
{
    partial class ExportPatronExcelDialog
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
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_normal = new System.Windows.Forms.TabPage();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox_readerBarcodeLabel = new System.Windows.Forms.CheckBox();
            this.checkBox_overdueInfo = new System.Windows.Forms.CheckBox();
            this.checkBox_readerInfo = new System.Windows.Forms.CheckBox();
            this.checkBox_borrowInfo = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button_getOutputExcelFileName = new System.Windows.Forms.Button();
            this.textBox_outputExcelFileName = new System.Windows.Forms.TextBox();
            this.tabPage_chargingHistory = new System.Windows.Forms.TabPage();
            this.button_chargingHistory_getDateRange = new System.Windows.Forms.Button();
            this.textBox_chargingHistory_dateRange = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBox_chargingHistory = new System.Windows.Forms.CheckBox();
            this.tabPage_filtering = new System.Windows.Forms.TabPage();
            this.checkBox_filter_amerce = new System.Windows.Forms.CheckBox();
            this.checkBox_filter_borrowing = new System.Windows.Forms.CheckBox();
            this.checkBox_filter_overdue = new System.Windows.Forms.CheckBox();
            this.checkBox_filter_notFilter = new System.Windows.Forms.CheckBox();
            this.tabControl_main.SuspendLayout();
            this.tabPage_normal.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage_chargingHistory.SuspendLayout();
            this.tabPage_filtering.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(432, 416);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(112, 34);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(314, 416);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(112, 34);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_normal);
            this.tabControl_main.Controls.Add(this.tabPage_chargingHistory);
            this.tabControl_main.Controls.Add(this.tabPage_filtering);
            this.tabControl_main.Location = new System.Drawing.Point(20, 20);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(4);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(524, 388);
            this.tabControl_main.TabIndex = 0;
            // 
            // tabPage_normal
            // 
            this.tabPage_normal.Controls.Add(this.groupBox1);
            this.tabPage_normal.Controls.Add(this.label2);
            this.tabPage_normal.Controls.Add(this.button_getOutputExcelFileName);
            this.tabPage_normal.Controls.Add(this.textBox_outputExcelFileName);
            this.tabPage_normal.Location = new System.Drawing.Point(4, 28);
            this.tabPage_normal.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_normal.Name = "tabPage_normal";
            this.tabPage_normal.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage_normal.Size = new System.Drawing.Size(516, 356);
            this.tabPage_normal.TabIndex = 0;
            this.tabPage_normal.Text = "通用";
            this.tabPage_normal.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBox_readerBarcodeLabel);
            this.groupBox1.Controls.Add(this.checkBox_overdueInfo);
            this.groupBox1.Controls.Add(this.checkBox_readerInfo);
            this.groupBox1.Controls.Add(this.checkBox_borrowInfo);
            this.groupBox1.Location = new System.Drawing.Point(12, 118);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(342, 176);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " 输出 ";
            // 
            // checkBox_readerBarcodeLabel
            // 
            this.checkBox_readerBarcodeLabel.AutoSize = true;
            this.checkBox_readerBarcodeLabel.Location = new System.Drawing.Point(186, 50);
            this.checkBox_readerBarcodeLabel.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_readerBarcodeLabel.Name = "checkBox_readerBarcodeLabel";
            this.checkBox_readerBarcodeLabel.Size = new System.Drawing.Size(106, 22);
            this.checkBox_readerBarcodeLabel.TabIndex = 1;
            this.checkBox_readerBarcodeLabel.Text = "条码标签";
            this.checkBox_readerBarcodeLabel.UseVisualStyleBackColor = true;
            // 
            // checkBox_overdueInfo
            // 
            this.checkBox_overdueInfo.AutoSize = true;
            this.checkBox_overdueInfo.Location = new System.Drawing.Point(40, 116);
            this.checkBox_overdueInfo.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_overdueInfo.Name = "checkBox_overdueInfo";
            this.checkBox_overdueInfo.Size = new System.Drawing.Size(106, 22);
            this.checkBox_overdueInfo.TabIndex = 3;
            this.checkBox_overdueInfo.Text = "违约信息";
            this.checkBox_overdueInfo.UseVisualStyleBackColor = true;
            this.checkBox_overdueInfo.CheckedChanged += new System.EventHandler(this.checkBox_readerInfo_CheckedChanged);
            // 
            // checkBox_readerInfo
            // 
            this.checkBox_readerInfo.AutoSize = true;
            this.checkBox_readerInfo.Location = new System.Drawing.Point(40, 50);
            this.checkBox_readerInfo.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_readerInfo.Name = "checkBox_readerInfo";
            this.checkBox_readerInfo.Size = new System.Drawing.Size(106, 22);
            this.checkBox_readerInfo.TabIndex = 0;
            this.checkBox_readerInfo.Text = "基本信息";
            this.checkBox_readerInfo.UseVisualStyleBackColor = true;
            this.checkBox_readerInfo.CheckedChanged += new System.EventHandler(this.checkBox_readerInfo_CheckedChanged);
            // 
            // checkBox_borrowInfo
            // 
            this.checkBox_borrowInfo.AutoSize = true;
            this.checkBox_borrowInfo.Location = new System.Drawing.Point(40, 82);
            this.checkBox_borrowInfo.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_borrowInfo.Name = "checkBox_borrowInfo";
            this.checkBox_borrowInfo.Size = new System.Drawing.Size(106, 22);
            this.checkBox_borrowInfo.TabIndex = 2;
            this.checkBox_borrowInfo.Text = "在借信息";
            this.checkBox_borrowInfo.UseVisualStyleBackColor = true;
            this.checkBox_borrowInfo.CheckedChanged += new System.EventHandler(this.checkBox_readerInfo_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 28);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(152, 18);
            this.label2.TabIndex = 0;
            this.label2.Text = "Excel 文件名(&F):";
            // 
            // button_getOutputExcelFileName
            // 
            this.button_getOutputExcelFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getOutputExcelFileName.Location = new System.Drawing.Point(436, 48);
            this.button_getOutputExcelFileName.Margin = new System.Windows.Forms.Padding(4);
            this.button_getOutputExcelFileName.Name = "button_getOutputExcelFileName";
            this.button_getOutputExcelFileName.Size = new System.Drawing.Size(66, 34);
            this.button_getOutputExcelFileName.TabIndex = 2;
            this.button_getOutputExcelFileName.Text = "...";
            this.button_getOutputExcelFileName.UseVisualStyleBackColor = true;
            this.button_getOutputExcelFileName.Click += new System.EventHandler(this.button_getOutputExcelFileName_Click);
            // 
            // textBox_outputExcelFileName
            // 
            this.textBox_outputExcelFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_outputExcelFileName.Location = new System.Drawing.Point(12, 51);
            this.textBox_outputExcelFileName.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_outputExcelFileName.Name = "textBox_outputExcelFileName";
            this.textBox_outputExcelFileName.Size = new System.Drawing.Size(414, 28);
            this.textBox_outputExcelFileName.TabIndex = 1;
            // 
            // tabPage_chargingHistory
            // 
            this.tabPage_chargingHistory.Controls.Add(this.button_chargingHistory_getDateRange);
            this.tabPage_chargingHistory.Controls.Add(this.textBox_chargingHistory_dateRange);
            this.tabPage_chargingHistory.Controls.Add(this.label1);
            this.tabPage_chargingHistory.Controls.Add(this.checkBox_chargingHistory);
            this.tabPage_chargingHistory.Location = new System.Drawing.Point(4, 28);
            this.tabPage_chargingHistory.Margin = new System.Windows.Forms.Padding(4);
            this.tabPage_chargingHistory.Name = "tabPage_chargingHistory";
            this.tabPage_chargingHistory.Padding = new System.Windows.Forms.Padding(4);
            this.tabPage_chargingHistory.Size = new System.Drawing.Size(516, 356);
            this.tabPage_chargingHistory.TabIndex = 1;
            this.tabPage_chargingHistory.Text = "借阅历史";
            this.tabPage_chargingHistory.UseVisualStyleBackColor = true;
            // 
            // button_chargingHistory_getDateRange
            // 
            this.button_chargingHistory_getDateRange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_chargingHistory_getDateRange.Enabled = false;
            this.button_chargingHistory_getDateRange.Location = new System.Drawing.Point(438, 104);
            this.button_chargingHistory_getDateRange.Margin = new System.Windows.Forms.Padding(4);
            this.button_chargingHistory_getDateRange.Name = "button_chargingHistory_getDateRange";
            this.button_chargingHistory_getDateRange.Size = new System.Drawing.Size(66, 34);
            this.button_chargingHistory_getDateRange.TabIndex = 4;
            this.button_chargingHistory_getDateRange.Text = "...";
            this.button_chargingHistory_getDateRange.UseVisualStyleBackColor = true;
            this.button_chargingHistory_getDateRange.Click += new System.EventHandler(this.button_chargingHistory_getDateRange_Click);
            // 
            // textBox_chargingHistory_dateRange
            // 
            this.textBox_chargingHistory_dateRange.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_chargingHistory_dateRange.Enabled = false;
            this.textBox_chargingHistory_dateRange.Location = new System.Drawing.Point(129, 106);
            this.textBox_chargingHistory_dateRange.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_chargingHistory_dateRange.Name = "textBox_chargingHistory_dateRange";
            this.textBox_chargingHistory_dateRange.Size = new System.Drawing.Size(298, 28);
            this.textBox_chargingHistory_dateRange.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 111);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 18);
            this.label1.TabIndex = 2;
            this.label1.Text = "日期范围:";
            // 
            // checkBox_chargingHistory
            // 
            this.checkBox_chargingHistory.AutoSize = true;
            this.checkBox_chargingHistory.Location = new System.Drawing.Point(9, 36);
            this.checkBox_chargingHistory.Margin = new System.Windows.Forms.Padding(4);
            this.checkBox_chargingHistory.Name = "checkBox_chargingHistory";
            this.checkBox_chargingHistory.Size = new System.Drawing.Size(142, 22);
            this.checkBox_chargingHistory.TabIndex = 1;
            this.checkBox_chargingHistory.Text = "输出借阅历史";
            this.checkBox_chargingHistory.UseVisualStyleBackColor = true;
            this.checkBox_chargingHistory.CheckedChanged += new System.EventHandler(this.checkBox_borrowHistory_CheckedChanged);
            // 
            // tabPage_filtering
            // 
            this.tabPage_filtering.Controls.Add(this.checkBox_filter_amerce);
            this.tabPage_filtering.Controls.Add(this.checkBox_filter_borrowing);
            this.tabPage_filtering.Controls.Add(this.checkBox_filter_overdue);
            this.tabPage_filtering.Controls.Add(this.checkBox_filter_notFilter);
            this.tabPage_filtering.Location = new System.Drawing.Point(4, 28);
            this.tabPage_filtering.Name = "tabPage_filtering";
            this.tabPage_filtering.Size = new System.Drawing.Size(516, 356);
            this.tabPage_filtering.TabIndex = 2;
            this.tabPage_filtering.Text = "筛选";
            this.tabPage_filtering.UseVisualStyleBackColor = true;
            // 
            // checkBox_filter_amerce
            // 
            this.checkBox_filter_amerce.AutoSize = true;
            this.checkBox_filter_amerce.Enabled = false;
            this.checkBox_filter_amerce.Location = new System.Drawing.Point(29, 139);
            this.checkBox_filter_amerce.Name = "checkBox_filter_amerce";
            this.checkBox_filter_amerce.Size = new System.Drawing.Size(151, 22);
            this.checkBox_filter_amerce.TabIndex = 3;
            this.checkBox_filter_amerce.Text = "有未交费的(&O)";
            this.checkBox_filter_amerce.UseVisualStyleBackColor = true;
            // 
            // checkBox_filter_borrowing
            // 
            this.checkBox_filter_borrowing.AutoSize = true;
            this.checkBox_filter_borrowing.Enabled = false;
            this.checkBox_filter_borrowing.Location = new System.Drawing.Point(29, 111);
            this.checkBox_filter_borrowing.Name = "checkBox_filter_borrowing";
            this.checkBox_filter_borrowing.Size = new System.Drawing.Size(133, 22);
            this.checkBox_filter_borrowing.TabIndex = 2;
            this.checkBox_filter_borrowing.Text = "有在借册(&O)";
            this.checkBox_filter_borrowing.UseVisualStyleBackColor = true;
            // 
            // checkBox_filter_overdue
            // 
            this.checkBox_filter_overdue.AutoSize = true;
            this.checkBox_filter_overdue.Enabled = false;
            this.checkBox_filter_overdue.Location = new System.Drawing.Point(29, 83);
            this.checkBox_filter_overdue.Name = "checkBox_filter_overdue";
            this.checkBox_filter_overdue.Size = new System.Drawing.Size(151, 22);
            this.checkBox_filter_overdue.TabIndex = 1;
            this.checkBox_filter_overdue.Text = "有超期未还(&O)";
            this.checkBox_filter_overdue.UseVisualStyleBackColor = true;
            // 
            // checkBox_filter_notFilter
            // 
            this.checkBox_filter_notFilter.AutoSize = true;
            this.checkBox_filter_notFilter.Checked = true;
            this.checkBox_filter_notFilter.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_filter_notFilter.Location = new System.Drawing.Point(29, 34);
            this.checkBox_filter_notFilter.Name = "checkBox_filter_notFilter";
            this.checkBox_filter_notFilter.Size = new System.Drawing.Size(169, 22);
            this.checkBox_filter_notFilter.TabIndex = 0;
            this.checkBox_filter_notFilter.Text = "不做任何筛选(&N)";
            this.checkBox_filter_notFilter.UseVisualStyleBackColor = true;
            this.checkBox_filter_notFilter.CheckedChanged += new System.EventHandler(this.checkBox_filter_notFilter_CheckedChanged);
            // 
            // ExportPatronExcelDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(561, 466);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "ExportPatronExcelDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "导出读者详情";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ExportPatronExcelDialog_FormClosed);
            this.Load += new System.EventHandler(this.ExportPatronExcelDialog_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_normal.ResumeLayout(false);
            this.tabPage_normal.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tabPage_chargingHistory.ResumeLayout(false);
            this.tabPage_chargingHistory.PerformLayout();
            this.tabPage_filtering.ResumeLayout(false);
            this.tabPage_filtering.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_normal;
        private System.Windows.Forms.TabPage tabPage_chargingHistory;
        private System.Windows.Forms.CheckBox checkBox_readerInfo;
        private System.Windows.Forms.CheckBox checkBox_borrowInfo;
        private System.Windows.Forms.CheckBox checkBox_overdueInfo;
        private System.Windows.Forms.CheckBox checkBox_chargingHistory;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_chargingHistory_getDateRange;
        private System.Windows.Forms.TextBox textBox_chargingHistory_dateRange;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_getOutputExcelFileName;
        private System.Windows.Forms.TextBox textBox_outputExcelFileName;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBox_readerBarcodeLabel;
        private System.Windows.Forms.TabPage tabPage_filtering;
        private System.Windows.Forms.CheckBox checkBox_filter_borrowing;
        private System.Windows.Forms.CheckBox checkBox_filter_overdue;
        private System.Windows.Forms.CheckBox checkBox_filter_notFilter;
        private System.Windows.Forms.CheckBox checkBox_filter_amerce;
    }
}