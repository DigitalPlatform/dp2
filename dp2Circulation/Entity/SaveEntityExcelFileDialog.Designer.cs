namespace dp2Circulation
{
    partial class SaveEntityExcelFileDialog
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
            this.button_getOutputFileName = new System.Windows.Forms.Button();
            this.textBox_outputFileName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button_biblioColumns = new System.Windows.Forms.Button();
            this.button_entityColumns = new System.Windows.Forms.Button();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_output = new System.Windows.Forms.TabPage();
            this.comboBox_libraryCode = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.tabPage_cfg = new System.Windows.Forms.TabPage();
            this.tabControl_main.SuspendLayout();
            this.tabPage_output.SuspendLayout();
            this.tabPage_cfg.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(507, 391);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(4);
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
            this.button_OK.Location = new System.Drawing.Point(386, 391);
            this.button_OK.Margin = new System.Windows.Forms.Padding(4);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(112, 34);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_getOutputFileName
            // 
            this.button_getOutputFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getOutputFileName.Location = new System.Drawing.Point(508, 184);
            this.button_getOutputFileName.Margin = new System.Windows.Forms.Padding(4);
            this.button_getOutputFileName.Name = "button_getOutputFileName";
            this.button_getOutputFileName.Size = new System.Drawing.Size(72, 34);
            this.button_getOutputFileName.TabIndex = 6;
            this.button_getOutputFileName.Text = "...";
            this.button_getOutputFileName.UseVisualStyleBackColor = true;
            this.button_getOutputFileName.Click += new System.EventHandler(this.button_getOutputFileName_Click);
            // 
            // textBox_outputFileName
            // 
            this.textBox_outputFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_outputFileName.Location = new System.Drawing.Point(21, 184);
            this.textBox_outputFileName.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_outputFileName.Name = "textBox_outputFileName";
            this.textBox_outputFileName.Size = new System.Drawing.Size(479, 28);
            this.textBox_outputFileName.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 162);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(215, 18);
            this.label2.TabIndex = 4;
            this.label2.Text = "输出的 Excel 文件名(&F):";
            // 
            // button_biblioColumns
            // 
            this.button_biblioColumns.Location = new System.Drawing.Point(17, 17);
            this.button_biblioColumns.Name = "button_biblioColumns";
            this.button_biblioColumns.Size = new System.Drawing.Size(277, 30);
            this.button_biblioColumns.TabIndex = 31;
            this.button_biblioColumns.Text = "书目信息列 ...";
            this.button_biblioColumns.UseVisualStyleBackColor = true;
            this.button_biblioColumns.Click += new System.EventHandler(this.button_biblioColumns_Click);
            // 
            // button_entityColumns
            // 
            this.button_entityColumns.Location = new System.Drawing.Point(17, 53);
            this.button_entityColumns.Name = "button_entityColumns";
            this.button_entityColumns.Size = new System.Drawing.Size(277, 30);
            this.button_entityColumns.TabIndex = 32;
            this.button_entityColumns.Text = "册信息列 ...";
            this.button_entityColumns.UseVisualStyleBackColor = true;
            this.button_entityColumns.Click += new System.EventHandler(this.button_orderColumns_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_output);
            this.tabControl_main.Controls.Add(this.tabPage_cfg);
            this.tabControl_main.Location = new System.Drawing.Point(12, 12);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(608, 372);
            this.tabControl_main.TabIndex = 0;
            // 
            // tabPage_output
            // 
            this.tabPage_output.Controls.Add(this.comboBox_libraryCode);
            this.tabPage_output.Controls.Add(this.label3);
            this.tabPage_output.Controls.Add(this.label2);
            this.tabPage_output.Controls.Add(this.textBox_outputFileName);
            this.tabPage_output.Controls.Add(this.button_getOutputFileName);
            this.tabPage_output.Location = new System.Drawing.Point(4, 28);
            this.tabPage_output.Name = "tabPage_output";
            this.tabPage_output.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_output.Size = new System.Drawing.Size(600, 340);
            this.tabPage_output.TabIndex = 0;
            this.tabPage_output.Text = "如何输出";
            this.tabPage_output.UseVisualStyleBackColor = true;
            // 
            // comboBox_libraryCode
            // 
            this.comboBox_libraryCode.FormattingEnabled = true;
            this.comboBox_libraryCode.Location = new System.Drawing.Point(204, 14);
            this.comboBox_libraryCode.Name = "comboBox_libraryCode";
            this.comboBox_libraryCode.Size = new System.Drawing.Size(263, 26);
            this.comboBox_libraryCode.TabIndex = 1;
            this.comboBox_libraryCode.TextChanged += new System.EventHandler(this.comboBox_libraryCode_TextChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 17);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(98, 18);
            this.label3.TabIndex = 0;
            this.label3.Text = "馆代码(&C):";
            // 
            // tabPage_cfg
            // 
            this.tabPage_cfg.Controls.Add(this.button_biblioColumns);
            this.tabPage_cfg.Controls.Add(this.button_entityColumns);
            this.tabPage_cfg.Location = new System.Drawing.Point(4, 28);
            this.tabPage_cfg.Name = "tabPage_cfg";
            this.tabPage_cfg.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_cfg.Size = new System.Drawing.Size(600, 340);
            this.tabPage_cfg.TabIndex = 1;
            this.tabPage_cfg.Text = "配置";
            this.tabPage_cfg.UseVisualStyleBackColor = true;
            // 
            // SaveEntityExcelFileDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(632, 438);
            this.Controls.Add(this.tabControl_main);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "SaveEntityExcelFileDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "保存到册信息 Excel 文件";
            this.Load += new System.EventHandler(this.SaveDistributeExcelFileDialog_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_output.ResumeLayout(false);
            this.tabPage_output.PerformLayout();
            this.tabPage_cfg.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_getOutputFileName;
        private System.Windows.Forms.TextBox textBox_outputFileName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_biblioColumns;
        private System.Windows.Forms.Button button_entityColumns;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_output;
        private System.Windows.Forms.TabPage tabPage_cfg;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_libraryCode;
    }
}