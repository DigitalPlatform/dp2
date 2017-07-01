namespace dp2Circulation
{
    partial class RecoverRecordDialog
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
            this.textBox_dateRange = new System.Windows.Forms.TextBox();
            this.button_findDateRange = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_recPathList = new System.Windows.Forms.TextBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "日期范围:";
            // 
            // textBox_dateRange
            // 
            this.textBox_dateRange.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dateRange.Location = new System.Drawing.Point(77, 13);
            this.textBox_dateRange.Name = "textBox_dateRange";
            this.textBox_dateRange.Size = new System.Drawing.Size(284, 21);
            this.textBox_dateRange.TabIndex = 1;
            // 
            // button_findDateRange
            // 
            this.button_findDateRange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findDateRange.Location = new System.Drawing.Point(367, 13);
            this.button_findDateRange.Name = "button_findDateRange";
            this.button_findDateRange.Size = new System.Drawing.Size(46, 23);
            this.button_findDateRange.TabIndex = 2;
            this.button_findDateRange.Text = "...";
            this.button_findDateRange.UseVisualStyleBackColor = true;
            this.button_findDateRange.Click += new System.EventHandler(this.button_findDateRange_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 54);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(167, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "拟恢复的记录路径(每行一个):";
            // 
            // textBox_recPathList
            // 
            this.textBox_recPathList.AcceptsReturn = true;
            this.textBox_recPathList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_recPathList.Location = new System.Drawing.Point(12, 69);
            this.textBox_recPathList.MaxLength = 0;
            this.textBox_recPathList.Multiline = true;
            this.textBox_recPathList.Name = "textBox_recPathList";
            this.textBox_recPathList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_recPathList.Size = new System.Drawing.Size(401, 168);
            this.textBox_recPathList.TabIndex = 4;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(338, 249);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 11;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(257, 249);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 10;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // RecoverRecordDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(425, 284);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_recPathList);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_findDateRange);
            this.Controls.Add(this.textBox_dateRange);
            this.Controls.Add(this.label1);
            this.Name = "RecoverRecordDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "从日志恢复数据库记录";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_dateRange;
        private System.Windows.Forms.Button button_findDateRange;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_recPathList;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
    }
}