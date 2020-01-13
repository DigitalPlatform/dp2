using DigitalPlatform.CommonControl;

namespace TestReporting
{
    partial class BuildReportDialog
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
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_reportType = new DigitalPlatform.CommonControl.TabComboBox();
            this.textBox_dateRange = new System.Windows.Forms.TextBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.textBox_parameters = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_libraryCode = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "报表类型:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 100);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 18);
            this.label2.TabIndex = 1;
            this.label2.Text = "时间范围:";
            // 
            // comboBox_reportType
            // 
            this.comboBox_reportType.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.comboBox_reportType.FormattingEnabled = true;
            this.comboBox_reportType.Location = new System.Drawing.Point(138, 12);
            this.comboBox_reportType.Name = "comboBox_reportType";
            this.comboBox_reportType.Size = new System.Drawing.Size(424, 29);
            this.comboBox_reportType.TabIndex = 2;
            // 
            // textBox_dateRange
            // 
            this.textBox_dateRange.Location = new System.Drawing.Point(138, 97);
            this.textBox_dateRange.Name = "textBox_dateRange";
            this.textBox_dateRange.Size = new System.Drawing.Size(353, 28);
            this.textBox_dateRange.TabIndex = 3;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(392, 332);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(87, 29);
            this.button_OK.TabIndex = 4;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(485, 332);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(91, 29);
            this.button_Cancel.TabIndex = 5;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // textBox_parameters
            // 
            this.textBox_parameters.AcceptsReturn = true;
            this.textBox_parameters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_parameters.Location = new System.Drawing.Point(138, 131);
            this.textBox_parameters.Multiline = true;
            this.textBox_parameters.Name = "textBox_parameters";
            this.textBox_parameters.Size = new System.Drawing.Size(424, 179);
            this.textBox_parameters.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 134);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(89, 18);
            this.label3.TabIndex = 7;
            this.label3.Text = "附加参数:";
            // 
            // textBox_libraryCode
            // 
            this.textBox_libraryCode.Location = new System.Drawing.Point(138, 47);
            this.textBox_libraryCode.Name = "textBox_libraryCode";
            this.textBox_libraryCode.Size = new System.Drawing.Size(353, 28);
            this.textBox_libraryCode.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 50);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(71, 18);
            this.label4.TabIndex = 8;
            this.label4.Text = "馆代码:";
            // 
            // BuildReportDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 18F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(588, 373);
            this.Controls.Add(this.textBox_libraryCode);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_parameters);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_dateRange);
            this.Controls.Add(this.comboBox_reportType);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Name = "BuildReportDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "创建报表";
            this.Load += new System.EventHandler(this.BuildReportDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private TabComboBox comboBox_reportType;
        private System.Windows.Forms.TextBox textBox_dateRange;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.TextBox textBox_parameters;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_libraryCode;
        private System.Windows.Forms.Label label4;
    }
}