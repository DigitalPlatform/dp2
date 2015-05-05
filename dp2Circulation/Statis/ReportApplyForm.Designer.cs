namespace dp2Circulation
{
    partial class ReportApplyForm
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_reportName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_reportType = new DigitalPlatform.CommonControl.TabComboBox();
            this.textBox_cfgFileName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.button_download_templateCfgFile = new System.Windows.Forms.Button();
            this.button_editCfgFile = new System.Windows.Forms.Button();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_normal = new System.Windows.Forms.TabPage();
            this.checkedComboBox_createFreq = new DigitalPlatform.CommonControl.CheckedComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tabPage_nameTable = new System.Windows.Forms.TabPage();
            this.button_nameTable_importStrings = new System.Windows.Forms.Button();
            this.textBox_nameTable_strings = new System.Windows.Forms.TextBox();
            this.label_nameTable_strings = new System.Windows.Forms.Label();
            this.tabControl_main.SuspendLayout();
            this.tabPage_normal.SuspendLayout();
            this.tabPage_nameTable.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(314, 275);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 6;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(233, 275);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 5;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "报表名称(&N):";
            // 
            // textBox_reportName
            // 
            this.textBox_reportName.Location = new System.Drawing.Point(105, 10);
            this.textBox_reportName.Name = "textBox_reportName";
            this.textBox_reportName.Size = new System.Drawing.Size(179, 21);
            this.textBox_reportName.TabIndex = 8;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 9;
            this.label2.Text = "报表类型(&T):";
            // 
            // comboBox_reportType
            // 
            this.comboBox_reportType.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_reportType.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBox_reportType.FormattingEnabled = true;
            this.comboBox_reportType.Location = new System.Drawing.Point(105, 66);
            this.comboBox_reportType.Name = "comboBox_reportType";
            this.comboBox_reportType.Size = new System.Drawing.Size(257, 22);
            this.comboBox_reportType.TabIndex = 10;
            this.comboBox_reportType.SelectedIndexChanged += new System.EventHandler(this.comboBox_reportType_SelectedIndexChanged);
            this.comboBox_reportType.TextChanged += new System.EventHandler(this.comboBox_reportType_TextChanged);
            // 
            // textBox_cfgFileName
            // 
            this.textBox_cfgFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_cfgFileName.Location = new System.Drawing.Point(105, 122);
            this.textBox_cfgFileName.Name = "textBox_cfgFileName";
            this.textBox_cfgFileName.Size = new System.Drawing.Size(257, 21);
            this.textBox_cfgFileName.TabIndex = 12;
            this.textBox_cfgFileName.TextChanged += new System.EventHandler(this.textBox_cfgFileName_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 125);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 12);
            this.label3.TabIndex = 11;
            this.label3.Text = "配置文件名(&C):";
            // 
            // button_download_templateCfgFile
            // 
            this.button_download_templateCfgFile.Location = new System.Drawing.Point(105, 92);
            this.button_download_templateCfgFile.Name = "button_download_templateCfgFile";
            this.button_download_templateCfgFile.Size = new System.Drawing.Size(179, 23);
            this.button_download_templateCfgFile.TabIndex = 13;
            this.button_download_templateCfgFile.Text = "从 dp2003.com 下载配置文件";
            this.button_download_templateCfgFile.UseVisualStyleBackColor = true;
            this.button_download_templateCfgFile.Visible = false;
            this.button_download_templateCfgFile.Click += new System.EventHandler(this.button_download_templateCfgFile_Click);
            // 
            // button_editCfgFile
            // 
            this.button_editCfgFile.Location = new System.Drawing.Point(105, 149);
            this.button_editCfgFile.Name = "button_editCfgFile";
            this.button_editCfgFile.Size = new System.Drawing.Size(111, 23);
            this.button_editCfgFile.TabIndex = 14;
            this.button_editCfgFile.Text = "修改配置文件";
            this.button_editCfgFile.UseVisualStyleBackColor = true;
            this.button_editCfgFile.Click += new System.EventHandler(this.button_editCfgFile_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_normal);
            this.tabControl_main.Controls.Add(this.tabPage_nameTable);
            this.tabControl_main.Location = new System.Drawing.Point(12, 12);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(377, 257);
            this.tabControl_main.TabIndex = 15;
            // 
            // tabPage_normal
            // 
            this.tabPage_normal.Controls.Add(this.checkedComboBox_createFreq);
            this.tabPage_normal.Controls.Add(this.label4);
            this.tabPage_normal.Controls.Add(this.button_download_templateCfgFile);
            this.tabPage_normal.Controls.Add(this.button_editCfgFile);
            this.tabPage_normal.Controls.Add(this.label1);
            this.tabPage_normal.Controls.Add(this.textBox_reportName);
            this.tabPage_normal.Controls.Add(this.textBox_cfgFileName);
            this.tabPage_normal.Controls.Add(this.label2);
            this.tabPage_normal.Controls.Add(this.label3);
            this.tabPage_normal.Controls.Add(this.comboBox_reportType);
            this.tabPage_normal.Location = new System.Drawing.Point(4, 22);
            this.tabPage_normal.Name = "tabPage_normal";
            this.tabPage_normal.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_normal.Size = new System.Drawing.Size(369, 231);
            this.tabPage_normal.TabIndex = 0;
            this.tabPage_normal.Text = "一般信息";
            this.tabPage_normal.UseVisualStyleBackColor = true;
            // 
            // checkedComboBox_createFreq
            // 
            this.checkedComboBox_createFreq.BackColor = System.Drawing.SystemColors.Window;
            this.checkedComboBox_createFreq.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.checkedComboBox_createFreq.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.checkedComboBox_createFreq.Location = new System.Drawing.Point(105, 37);
            this.checkedComboBox_createFreq.Margin = new System.Windows.Forms.Padding(0);
            this.checkedComboBox_createFreq.Name = "checkedComboBox_createFreq";
            this.checkedComboBox_createFreq.Padding = new System.Windows.Forms.Padding(4);
            this.checkedComboBox_createFreq.Size = new System.Drawing.Size(179, 24);
            this.checkedComboBox_createFreq.TabIndex = 16;
            this.checkedComboBox_createFreq.DropDown += new System.EventHandler(this.checkedComboBox_createFreq_DropDown);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 42);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(77, 12);
            this.label4.TabIndex = 15;
            this.label4.Text = "创建频率(&F):";
            // 
            // tabPage_nameTable
            // 
            this.tabPage_nameTable.Controls.Add(this.button_nameTable_importStrings);
            this.tabPage_nameTable.Controls.Add(this.textBox_nameTable_strings);
            this.tabPage_nameTable.Controls.Add(this.label_nameTable_strings);
            this.tabPage_nameTable.Location = new System.Drawing.Point(4, 22);
            this.tabPage_nameTable.Name = "tabPage_nameTable";
            this.tabPage_nameTable.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_nameTable.Size = new System.Drawing.Size(369, 231);
            this.tabPage_nameTable.TabIndex = 1;
            this.tabPage_nameTable.Text = "名字列表";
            this.tabPage_nameTable.UseVisualStyleBackColor = true;
            // 
            // button_nameTable_importStrings
            // 
            this.button_nameTable_importStrings.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.button_nameTable_importStrings.Location = new System.Drawing.Point(8, 202);
            this.button_nameTable_importStrings.Name = "button_nameTable_importStrings";
            this.button_nameTable_importStrings.Size = new System.Drawing.Size(134, 23);
            this.button_nameTable_importStrings.TabIndex = 5;
            this.button_nameTable_importStrings.Text = "导入全部名字";
            this.button_nameTable_importStrings.UseVisualStyleBackColor = true;
            this.button_nameTable_importStrings.Click += new System.EventHandler(this.button_nameTable_importStrings_Click);
            // 
            // textBox_nameTable_strings
            // 
            this.textBox_nameTable_strings.AcceptsReturn = true;
            this.textBox_nameTable_strings.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_nameTable_strings.HideSelection = false;
            this.textBox_nameTable_strings.Location = new System.Drawing.Point(8, 36);
            this.textBox_nameTable_strings.Multiline = true;
            this.textBox_nameTable_strings.Name = "textBox_nameTable_strings";
            this.textBox_nameTable_strings.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_nameTable_strings.Size = new System.Drawing.Size(232, 163);
            this.textBox_nameTable_strings.TabIndex = 4;
            // 
            // label_nameTable_strings
            // 
            this.label_nameTable_strings.AutoSize = true;
            this.label_nameTable_strings.Location = new System.Drawing.Point(6, 21);
            this.label_nameTable_strings.Name = "label_nameTable_strings";
            this.label_nameTable_strings.Size = new System.Drawing.Size(191, 12);
            this.label_nameTable_strings.TabIndex = 3;
            this.label_nameTable_strings.Text = "部门名称列表(&D) [每行一个名称]:";
            // 
            // ReportApplyForm
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(401, 310);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "ReportApplyForm";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "一个报表";
            this.Load += new System.EventHandler(this.ReportApplyForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_normal.ResumeLayout(false);
            this.tabPage_normal.PerformLayout();
            this.tabPage_nameTable.ResumeLayout(false);
            this.tabPage_nameTable.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_reportName;
        private System.Windows.Forms.Label label2;
        private DigitalPlatform.CommonControl.TabComboBox comboBox_reportType;
        private System.Windows.Forms.TextBox textBox_cfgFileName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button_download_templateCfgFile;
        private System.Windows.Forms.Button button_editCfgFile;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_normal;
        private System.Windows.Forms.TabPage tabPage_nameTable;
        private System.Windows.Forms.Label label4;
        private DigitalPlatform.CommonControl.CheckedComboBox checkedComboBox_createFreq;
        private System.Windows.Forms.Button button_nameTable_importStrings;
        private System.Windows.Forms.TextBox textBox_nameTable_strings;
        private System.Windows.Forms.Label label_nameTable_strings;
    }
}