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
            this.checkBox_readerBarcodeLabel = new System.Windows.Forms.CheckBox();
            this.tabControl_main.SuspendLayout();
            this.tabPage_normal.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tabPage_chargingHistory.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(288, 277);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(209, 277);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
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
            this.tabControl_main.Location = new System.Drawing.Point(13, 13);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(349, 259);
            this.tabControl_main.TabIndex = 0;
            // 
            // tabPage_normal
            // 
            this.tabPage_normal.Controls.Add(this.groupBox1);
            this.tabPage_normal.Controls.Add(this.label2);
            this.tabPage_normal.Controls.Add(this.button_getOutputExcelFileName);
            this.tabPage_normal.Controls.Add(this.textBox_outputExcelFileName);
            this.tabPage_normal.Location = new System.Drawing.Point(4, 22);
            this.tabPage_normal.Name = "tabPage_normal";
            this.tabPage_normal.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_normal.Size = new System.Drawing.Size(341, 233);
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
            this.groupBox1.Location = new System.Drawing.Point(8, 79);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(228, 117);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " 输出 ";
            // 
            // checkBox_overdueInfo
            // 
            this.checkBox_overdueInfo.AutoSize = true;
            this.checkBox_overdueInfo.Location = new System.Drawing.Point(27, 77);
            this.checkBox_overdueInfo.Name = "checkBox_overdueInfo";
            this.checkBox_overdueInfo.Size = new System.Drawing.Size(72, 16);
            this.checkBox_overdueInfo.TabIndex = 3;
            this.checkBox_overdueInfo.Text = "违约信息";
            this.checkBox_overdueInfo.UseVisualStyleBackColor = true;
            this.checkBox_overdueInfo.CheckedChanged += new System.EventHandler(this.checkBox_readerInfo_CheckedChanged);
            // 
            // checkBox_readerInfo
            // 
            this.checkBox_readerInfo.AutoSize = true;
            this.checkBox_readerInfo.Location = new System.Drawing.Point(27, 33);
            this.checkBox_readerInfo.Name = "checkBox_readerInfo";
            this.checkBox_readerInfo.Size = new System.Drawing.Size(72, 16);
            this.checkBox_readerInfo.TabIndex = 0;
            this.checkBox_readerInfo.Text = "基本信息";
            this.checkBox_readerInfo.UseVisualStyleBackColor = true;
            this.checkBox_readerInfo.CheckedChanged += new System.EventHandler(this.checkBox_readerInfo_CheckedChanged);
            // 
            // checkBox_borrowInfo
            // 
            this.checkBox_borrowInfo.AutoSize = true;
            this.checkBox_borrowInfo.Location = new System.Drawing.Point(27, 55);
            this.checkBox_borrowInfo.Name = "checkBox_borrowInfo";
            this.checkBox_borrowInfo.Size = new System.Drawing.Size(72, 16);
            this.checkBox_borrowInfo.TabIndex = 2;
            this.checkBox_borrowInfo.Text = "在借信息";
            this.checkBox_borrowInfo.UseVisualStyleBackColor = true;
            this.checkBox_borrowInfo.CheckedChanged += new System.EventHandler(this.checkBox_readerInfo_CheckedChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(101, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "Excel 文件名(&F):";
            // 
            // button_getOutputExcelFileName
            // 
            this.button_getOutputExcelFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getOutputExcelFileName.Location = new System.Drawing.Point(291, 32);
            this.button_getOutputExcelFileName.Name = "button_getOutputExcelFileName";
            this.button_getOutputExcelFileName.Size = new System.Drawing.Size(44, 23);
            this.button_getOutputExcelFileName.TabIndex = 2;
            this.button_getOutputExcelFileName.Text = "...";
            this.button_getOutputExcelFileName.UseVisualStyleBackColor = true;
            this.button_getOutputExcelFileName.Click += new System.EventHandler(this.button_getOutputExcelFileName_Click);
            // 
            // textBox_outputExcelFileName
            // 
            this.textBox_outputExcelFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_outputExcelFileName.Location = new System.Drawing.Point(8, 34);
            this.textBox_outputExcelFileName.Name = "textBox_outputExcelFileName";
            this.textBox_outputExcelFileName.Size = new System.Drawing.Size(277, 21);
            this.textBox_outputExcelFileName.TabIndex = 1;
            // 
            // tabPage_chargingHistory
            // 
            this.tabPage_chargingHistory.Controls.Add(this.button_chargingHistory_getDateRange);
            this.tabPage_chargingHistory.Controls.Add(this.textBox_chargingHistory_dateRange);
            this.tabPage_chargingHistory.Controls.Add(this.label1);
            this.tabPage_chargingHistory.Controls.Add(this.checkBox_chargingHistory);
            this.tabPage_chargingHistory.Location = new System.Drawing.Point(4, 22);
            this.tabPage_chargingHistory.Name = "tabPage_chargingHistory";
            this.tabPage_chargingHistory.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_chargingHistory.Size = new System.Drawing.Size(341, 233);
            this.tabPage_chargingHistory.TabIndex = 1;
            this.tabPage_chargingHistory.Text = "借阅历史";
            this.tabPage_chargingHistory.UseVisualStyleBackColor = true;
            // 
            // button_chargingHistory_getDateRange
            // 
            this.button_chargingHistory_getDateRange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_chargingHistory_getDateRange.Enabled = false;
            this.button_chargingHistory_getDateRange.Location = new System.Drawing.Point(292, 69);
            this.button_chargingHistory_getDateRange.Name = "button_chargingHistory_getDateRange";
            this.button_chargingHistory_getDateRange.Size = new System.Drawing.Size(44, 23);
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
            this.textBox_chargingHistory_dateRange.Location = new System.Drawing.Point(86, 71);
            this.textBox_chargingHistory_dateRange.Name = "textBox_chargingHistory_dateRange";
            this.textBox_chargingHistory_dateRange.Size = new System.Drawing.Size(200, 21);
            this.textBox_chargingHistory_dateRange.TabIndex = 3;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 74);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "日期范围:";
            // 
            // checkBox_chargingHistory
            // 
            this.checkBox_chargingHistory.AutoSize = true;
            this.checkBox_chargingHistory.Location = new System.Drawing.Point(6, 24);
            this.checkBox_chargingHistory.Name = "checkBox_chargingHistory";
            this.checkBox_chargingHistory.Size = new System.Drawing.Size(96, 16);
            this.checkBox_chargingHistory.TabIndex = 1;
            this.checkBox_chargingHistory.Text = "输出借阅历史";
            this.checkBox_chargingHistory.UseVisualStyleBackColor = true;
            this.checkBox_chargingHistory.CheckedChanged += new System.EventHandler(this.checkBox_borrowHistory_CheckedChanged);
            // 
            // checkBox_readerBarcodeLabel
            // 
            this.checkBox_readerBarcodeLabel.AutoSize = true;
            this.checkBox_readerBarcodeLabel.Location = new System.Drawing.Point(124, 33);
            this.checkBox_readerBarcodeLabel.Name = "checkBox_readerBarcodeLabel";
            this.checkBox_readerBarcodeLabel.Size = new System.Drawing.Size(72, 16);
            this.checkBox_readerBarcodeLabel.TabIndex = 1;
            this.checkBox_readerBarcodeLabel.Text = "条码标签";
            this.checkBox_readerBarcodeLabel.UseVisualStyleBackColor = true;
            // 
            // ExportPatronExcelDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(374, 311);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
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
    }
}