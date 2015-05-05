namespace dp2Circulation
{
    partial class ConvertReportFormatDialog
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_reportDirectory = new System.Windows.Forms.TextBox();
            this.button_findReportDirectory = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.checkBox_html = new System.Windows.Forms.CheckBox();
            this.checkBox_excel = new System.Windows.Forms.CheckBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_localReportDir = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "报表文件目录(&D):";
            // 
            // textBox_reportDirectory
            // 
            this.textBox_reportDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_reportDirectory.Location = new System.Drawing.Point(12, 28);
            this.textBox_reportDirectory.Name = "textBox_reportDirectory";
            this.textBox_reportDirectory.Size = new System.Drawing.Size(313, 21);
            this.textBox_reportDirectory.TabIndex = 1;
            // 
            // button_findReportDirectory
            // 
            this.button_findReportDirectory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findReportDirectory.Location = new System.Drawing.Point(331, 26);
            this.button_findReportDirectory.Name = "button_findReportDirectory";
            this.button_findReportDirectory.Size = new System.Drawing.Size(46, 23);
            this.button_findReportDirectory.TabIndex = 2;
            this.button_findReportDirectory.Text = "...";
            this.button_findReportDirectory.UseVisualStyleBackColor = true;
            this.button_findReportDirectory.Click += new System.EventHandler(this.button_findReportDirectory_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.checkBox_excel);
            this.groupBox1.Controls.Add(this.checkBox_html);
            this.groupBox1.Location = new System.Drawing.Point(12, 85);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(122, 93);
            this.groupBox1.TabIndex = 3;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "转换为";
            // 
            // checkBox_html
            // 
            this.checkBox_html.AutoSize = true;
            this.checkBox_html.Location = new System.Drawing.Point(24, 31);
            this.checkBox_html.Name = "checkBox_html";
            this.checkBox_html.Size = new System.Drawing.Size(48, 16);
            this.checkBox_html.TabIndex = 0;
            this.checkBox_html.Text = "HTML";
            this.checkBox_html.UseVisualStyleBackColor = true;
            // 
            // checkBox_excel
            // 
            this.checkBox_excel.AutoSize = true;
            this.checkBox_excel.Location = new System.Drawing.Point(24, 53);
            this.checkBox_excel.Name = "checkBox_excel";
            this.checkBox_excel.Size = new System.Drawing.Size(54, 16);
            this.checkBox_excel.TabIndex = 1;
            this.checkBox_excel.Text = "Excel";
            this.checkBox_excel.UseVisualStyleBackColor = true;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(302, 192);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 9;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(221, 192);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 8;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_localReportDir
            // 
            this.button_localReportDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_localReportDir.Location = new System.Drawing.Point(273, 50);
            this.button_localReportDir.Name = "button_localReportDir";
            this.button_localReportDir.Size = new System.Drawing.Size(104, 23);
            this.button_localReportDir.TabIndex = 10;
            this.button_localReportDir.Text = "当前报表目录";
            this.button_localReportDir.UseVisualStyleBackColor = true;
            this.button_localReportDir.Click += new System.EventHandler(this.button_localReportDir_Click);
            // 
            // ConvertReportFormatDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(389, 227);
            this.Controls.Add(this.button_localReportDir);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button_findReportDirectory);
            this.Controls.Add(this.textBox_reportDirectory);
            this.Controls.Add(this.label1);
            this.Name = "ConvertReportFormatDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "报表格式转换";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_reportDirectory;
        private System.Windows.Forms.Button button_findReportDirectory;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkBox_excel;
        private System.Windows.Forms.CheckBox checkBox_html;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_localReportDir;
    }
}