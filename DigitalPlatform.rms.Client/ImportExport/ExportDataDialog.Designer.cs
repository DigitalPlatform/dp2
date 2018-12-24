namespace DigitalPlatform.rms.Client
{
    partial class ExportDataDialog
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
            this.textBox_dbPath = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_setEndIdMax = new System.Windows.Forms.Button();
            this.button_setStartIdMin = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_endNo = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_startNo = new System.Windows.Forms.TextBox();
            this.radioButton_startEnd = new System.Windows.Forms.RadioButton();
            this.radioButton_all = new System.Windows.Forms.RadioButton();
            this.label4 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox_outputEncoding = new System.Windows.Forms.ComboBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox_dbPath
            // 
            this.textBox_dbPath.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dbPath.Location = new System.Drawing.Point(12, 30);
            this.textBox_dbPath.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.textBox_dbPath.Multiline = true;
            this.textBox_dbPath.Name = "textBox_dbPath";
            this.textBox_dbPath.ReadOnly = true;
            this.textBox_dbPath.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_dbPath.Size = new System.Drawing.Size(456, 124);
            this.textBox_dbPath.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.button_setEndIdMax);
            this.groupBox1.Controls.Add(this.button_setStartIdMin);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.textBox_endNo);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.textBox_startNo);
            this.groupBox1.Controls.Add(this.radioButton_startEnd);
            this.groupBox1.Controls.Add(this.radioButton_all);
            this.groupBox1.Location = new System.Drawing.Point(12, 162);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.groupBox1.Size = new System.Drawing.Size(456, 169);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " 输出记录范围 ";
            // 
            // button_setEndIdMax
            // 
            this.button_setEndIdMax.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_setEndIdMax.Location = new System.Drawing.Point(356, 123);
            this.button_setEndIdMax.Name = "button_setEndIdMax";
            this.button_setEndIdMax.Size = new System.Drawing.Size(75, 30);
            this.button_setEndIdMax.TabIndex = 7;
            this.button_setEndIdMax.Text = "最大值";
            this.button_setEndIdMax.UseVisualStyleBackColor = true;
            this.button_setEndIdMax.Click += new System.EventHandler(this.button_setEndIdMax_Click);
            // 
            // button_setStartIdMin
            // 
            this.button_setStartIdMin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_setStartIdMin.Location = new System.Drawing.Point(356, 92);
            this.button_setStartIdMin.Name = "button_setStartIdMin";
            this.button_setStartIdMin.Size = new System.Drawing.Size(75, 30);
            this.button_setStartIdMin.TabIndex = 4;
            this.button_setStartIdMin.Text = "最小值";
            this.button_setStartIdMin.UseVisualStyleBackColor = true;
            this.button_setStartIdMin.Click += new System.EventHandler(this.button_setStartIdMin_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(64, 126);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(105, 24);
            this.label5.TabIndex = 5;
            this.label5.Text = "结束记录ID:";
            // 
            // textBox_endNo
            // 
            this.textBox_endNo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_endNo.Location = new System.Drawing.Point(168, 122);
            this.textBox_endNo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.textBox_endNo.MaxLength = 10;
            this.textBox_endNo.Name = "textBox_endNo";
            this.textBox_endNo.Size = new System.Drawing.Size(182, 31);
            this.textBox_endNo.TabIndex = 6;
            this.textBox_endNo.TextChanged += new System.EventHandler(this.textBox_endNo_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(64, 95);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(105, 24);
            this.label1.TabIndex = 2;
            this.label1.Text = "起始记录ID:";
            // 
            // textBox_startNo
            // 
            this.textBox_startNo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_startNo.Location = new System.Drawing.Point(168, 91);
            this.textBox_startNo.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.textBox_startNo.MaxLength = 10;
            this.textBox_startNo.Name = "textBox_startNo";
            this.textBox_startNo.Size = new System.Drawing.Size(182, 31);
            this.textBox_startNo.TabIndex = 3;
            this.textBox_startNo.TextChanged += new System.EventHandler(this.textBox_startNo_TextChanged);
            // 
            // radioButton_startEnd
            // 
            this.radioButton_startEnd.AutoSize = true;
            this.radioButton_startEnd.Checked = true;
            this.radioButton_startEnd.Location = new System.Drawing.Point(22, 57);
            this.radioButton_startEnd.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.radioButton_startEnd.Name = "radioButton_startEnd";
            this.radioButton_startEnd.Size = new System.Drawing.Size(117, 28);
            this.radioButton_startEnd.TabIndex = 1;
            this.radioButton_startEnd.TabStop = true;
            this.radioButton_startEnd.Text = "起止ID(&S) ";
            // 
            // radioButton_all
            // 
            this.radioButton_all.AutoSize = true;
            this.radioButton_all.Location = new System.Drawing.Point(22, 28);
            this.radioButton_all.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.radioButton_all.Name = "radioButton_all";
            this.radioButton_all.Size = new System.Drawing.Size(96, 28);
            this.radioButton_all.TabIndex = 0;
            this.radioButton_all.Text = "全部(&A)";
            this.radioButton_all.CheckedChanged += new System.EventHandler(this.radioButton_all_CheckedChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 9);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(94, 24);
            this.label4.TabIndex = 0;
            this.label4.Text = "数据库(&D):";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(286, 387);
            this.button_OK.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(87, 33);
            this.button_OK.TabIndex = 3;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(381, 387);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(87, 33);
            this.button_Cancel.TabIndex = 4;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 339);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(108, 24);
            this.label2.TabIndex = 5;
            this.label2.Text = "编码方式(&E):";
            // 
            // comboBox_outputEncoding
            // 
            this.comboBox_outputEncoding.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_outputEncoding.FormattingEnabled = true;
            this.comboBox_outputEncoding.Items.AddRange(new object[] {
            "utf-8",
            "gb2312"});
            this.comboBox_outputEncoding.Location = new System.Drawing.Point(180, 336);
            this.comboBox_outputEncoding.Name = "comboBox_outputEncoding";
            this.comboBox_outputEncoding.Size = new System.Drawing.Size(288, 32);
            this.comboBox_outputEncoding.TabIndex = 6;
            this.comboBox_outputEncoding.Text = "utf-8";
            // 
            // ExportDataDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 24F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(482, 437);
            this.Controls.Add(this.comboBox_outputEncoding);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_dbPath);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.Name = "ExportDataDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "导出数据";
            this.Load += new System.EventHandler(this.ExportDataDialog_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton radioButton_startEnd;
        private System.Windows.Forms.RadioButton radioButton_all;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_setEndIdMax;
        private System.Windows.Forms.Button button_setStartIdMin;
        private System.Windows.Forms.TextBox textBox_endNo;
        private System.Windows.Forms.TextBox textBox_startNo;
        private System.Windows.Forms.TextBox textBox_dbPath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox_outputEncoding;
    }
}