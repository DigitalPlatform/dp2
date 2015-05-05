namespace dp2Circulation
{
    partial class LibraryReportConfigForm
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_normalInfo = new System.Windows.Forms.TabPage();
            this.comboBox_general_libraryCode = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tabPage_reports = new System.Windows.Forms.TabPage();
            this.listView_reports = new System.Windows.Forms.ListView();
            this.columnHeader_report_name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_freq = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_report_type = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_report_cfgFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader_report_nameTable = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.tabPage_style = new System.Windows.Forms.TabPage();
            this.comboBox_style_htmlTemplate = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage_normalInfo.SuspendLayout();
            this.tabPage_reports.SuspendLayout();
            this.tabPage_style.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage_normalInfo);
            this.tabControl1.Controls.Add(this.tabPage_reports);
            this.tabControl1.Controls.Add(this.tabPage_style);
            this.tabControl1.Location = new System.Drawing.Point(13, 13);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(407, 227);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage_normalInfo
            // 
            this.tabPage_normalInfo.Controls.Add(this.comboBox_general_libraryCode);
            this.tabPage_normalInfo.Controls.Add(this.label2);
            this.tabPage_normalInfo.Location = new System.Drawing.Point(4, 22);
            this.tabPage_normalInfo.Name = "tabPage_normalInfo";
            this.tabPage_normalInfo.Size = new System.Drawing.Size(399, 201);
            this.tabPage_normalInfo.TabIndex = 2;
            this.tabPage_normalInfo.Text = "一般信息";
            this.tabPage_normalInfo.UseVisualStyleBackColor = true;
            // 
            // comboBox_general_libraryCode
            // 
            this.comboBox_general_libraryCode.FormattingEnabled = true;
            this.comboBox_general_libraryCode.Location = new System.Drawing.Point(100, 15);
            this.comboBox_general_libraryCode.Name = "comboBox_general_libraryCode";
            this.comboBox_general_libraryCode.Size = new System.Drawing.Size(167, 20);
            this.comboBox_general_libraryCode.TabIndex = 1;
            this.comboBox_general_libraryCode.TextChanged += new System.EventHandler(this.comboBox_general_libraryCode_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 18);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "馆代码(&C):";
            // 
            // tabPage_reports
            // 
            this.tabPage_reports.Controls.Add(this.listView_reports);
            this.tabPage_reports.Location = new System.Drawing.Point(4, 22);
            this.tabPage_reports.Name = "tabPage_reports";
            this.tabPage_reports.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_reports.Size = new System.Drawing.Size(399, 201);
            this.tabPage_reports.TabIndex = 1;
            this.tabPage_reports.Text = "报表";
            this.tabPage_reports.UseVisualStyleBackColor = true;
            // 
            // listView_reports
            // 
            this.listView_reports.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_report_name,
            this.columnHeader_freq,
            this.columnHeader_report_type,
            this.columnHeader_report_cfgFile,
            this.columnHeader_report_nameTable});
            this.listView_reports.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listView_reports.FullRowSelect = true;
            this.listView_reports.HideSelection = false;
            this.listView_reports.Location = new System.Drawing.Point(3, 3);
            this.listView_reports.Name = "listView_reports";
            this.listView_reports.Size = new System.Drawing.Size(393, 195);
            this.listView_reports.TabIndex = 0;
            this.listView_reports.UseCompatibleStateImageBehavior = false;
            this.listView_reports.View = System.Windows.Forms.View.Details;
            this.listView_reports.DoubleClick += new System.EventHandler(this.listView_reports_DoubleClick);
            this.listView_reports.MouseUp += new System.Windows.Forms.MouseEventHandler(this.listView_reports_MouseUp);
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
            // columnHeader_report_cfgFile
            // 
            this.columnHeader_report_cfgFile.Text = "配置文件";
            this.columnHeader_report_cfgFile.Width = 200;
            // 
            // columnHeader_report_nameTable
            // 
            this.columnHeader_report_nameTable.Text = "名字表";
            this.columnHeader_report_nameTable.Width = 200;
            // 
            // tabPage_style
            // 
            this.tabPage_style.Controls.Add(this.comboBox_style_htmlTemplate);
            this.tabPage_style.Controls.Add(this.label1);
            this.tabPage_style.Location = new System.Drawing.Point(4, 22);
            this.tabPage_style.Name = "tabPage_style";
            this.tabPage_style.Size = new System.Drawing.Size(399, 201);
            this.tabPage_style.TabIndex = 3;
            this.tabPage_style.Text = "样式";
            this.tabPage_style.UseVisualStyleBackColor = true;
            // 
            // comboBox_style_htmlTemplate
            // 
            this.comboBox_style_htmlTemplate.FormattingEnabled = true;
            this.comboBox_style_htmlTemplate.Location = new System.Drawing.Point(117, 18);
            this.comboBox_style_htmlTemplate.Name = "comboBox_style_htmlTemplate";
            this.comboBox_style_htmlTemplate.Size = new System.Drawing.Size(167, 20);
            this.comboBox_style_htmlTemplate.TabIndex = 1;
            this.comboBox_style_htmlTemplate.TextChanged += new System.EventHandler(this.comboBox_style_htmlTemplate_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 21);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(107, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "HTML 样式模板(&T):";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(264, 246);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(345, 246);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // LibraryReportConfigForm
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(432, 281);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.tabControl1);
            this.Name = "LibraryReportConfigForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "一个分馆的报表配置";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.LibraryReportConfigForm_FormClosing);
            this.Load += new System.EventHandler(this.LibraryReportConfigForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_normalInfo.ResumeLayout(false);
            this.tabPage_normalInfo.PerformLayout();
            this.tabPage_reports.ResumeLayout(false);
            this.tabPage_style.ResumeLayout(false);
            this.tabPage_style.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_reports;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.TabPage tabPage_normalInfo;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_general_libraryCode;
        private System.Windows.Forms.ListView listView_reports;
        private System.Windows.Forms.ColumnHeader columnHeader_report_name;
        private System.Windows.Forms.ColumnHeader columnHeader_report_type;
        private System.Windows.Forms.ColumnHeader columnHeader_report_cfgFile;
        private System.Windows.Forms.ColumnHeader columnHeader_freq;
        private System.Windows.Forms.ColumnHeader columnHeader_report_nameTable;
        private System.Windows.Forms.TabPage tabPage_style;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_style_htmlTemplate;
    }
}