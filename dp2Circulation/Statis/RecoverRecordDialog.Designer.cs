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
            this.checkBox_lastFirst = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 32);
            this.label1.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(105, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "日期范围:";
            // 
            // textBox_dateRange
            // 
            this.textBox_dateRange.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dateRange.Location = new System.Drawing.Point(141, 23);
            this.textBox_dateRange.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_dateRange.Name = "textBox_dateRange";
            this.textBox_dateRange.Size = new System.Drawing.Size(517, 31);
            this.textBox_dateRange.TabIndex = 1;
            // 
            // button_findDateRange
            // 
            this.button_findDateRange.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_findDateRange.Location = new System.Drawing.Point(673, 23);
            this.button_findDateRange.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_findDateRange.Name = "button_findDateRange";
            this.button_findDateRange.Size = new System.Drawing.Size(84, 40);
            this.button_findDateRange.TabIndex = 2;
            this.button_findDateRange.Text = "...";
            this.button_findDateRange.UseVisualStyleBackColor = true;
            this.button_findDateRange.Click += new System.EventHandler(this.button_findDateRange_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 94);
            this.label2.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(295, 21);
            this.label2.TabIndex = 3;
            this.label2.Text = "拟恢复的记录路径(每行一个):";
            // 
            // textBox_recPathList
            // 
            this.textBox_recPathList.AcceptsReturn = true;
            this.textBox_recPathList.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_recPathList.Location = new System.Drawing.Point(22, 121);
            this.textBox_recPathList.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_recPathList.MaxLength = 0;
            this.textBox_recPathList.Multiline = true;
            this.textBox_recPathList.Name = "textBox_recPathList";
            this.textBox_recPathList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_recPathList.Size = new System.Drawing.Size(732, 291);
            this.textBox_recPathList.TabIndex = 4;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(620, 436);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(138, 40);
            this.button_Cancel.TabIndex = 11;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(471, 436);
            this.button_OK.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(138, 40);
            this.button_OK.TabIndex = 10;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // checkBox_lastFirst
            // 
            this.checkBox_lastFirst.AutoSize = true;
            this.checkBox_lastFirst.Location = new System.Drawing.Point(141, 62);
            this.checkBox_lastFirst.Name = "checkBox_lastFirst";
            this.checkBox_lastFirst.Size = new System.Drawing.Size(162, 25);
            this.checkBox_lastFirst.TabIndex = 12;
            this.checkBox_lastFirst.Text = "最新日期在前";
            this.checkBox_lastFirst.UseVisualStyleBackColor = true;
            // 
            // RecoverRecordDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(779, 497);
            this.Controls.Add(this.checkBox_lastFirst);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_recPathList);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.button_findDateRange);
            this.Controls.Add(this.textBox_dateRange);
            this.Controls.Add(this.label1);
            this.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
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
        private System.Windows.Forms.CheckBox checkBox_lastFirst;
    }
}