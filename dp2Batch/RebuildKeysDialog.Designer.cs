namespace dp2Batch
{
    partial class RebuildKeysDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RebuildKeysDialog));
            this.radioButton_record = new System.Windows.Forms.RadioButton();
            this.radioButton_whole = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_comment_record = new System.Windows.Forms.TextBox();
            this.textBox_comment_whole = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // radioButton_record
            // 
            this.radioButton_record.AutoSize = true;
            this.radioButton_record.Location = new System.Drawing.Point(17, 36);
            this.radioButton_record.Name = "radioButton_record";
            this.radioButton_record.Size = new System.Drawing.Size(94, 19);
            this.radioButton_record.TabIndex = 0;
            this.radioButton_record.Text = "单记录(&R)";
            this.radioButton_record.UseVisualStyleBackColor = true;
            // 
            // radioButton_whole
            // 
            this.radioButton_whole.AutoSize = true;
            this.radioButton_whole.Checked = true;
            this.radioButton_whole.Location = new System.Drawing.Point(17, 172);
            this.radioButton_whole.Name = "radioButton_whole";
            this.radioButton_whole.Size = new System.Drawing.Size(79, 19);
            this.radioButton_whole.TabIndex = 1;
            this.radioButton_whole.TabStop = true;
            this.radioButton_whole.Text = "整体(&W)";
            this.radioButton_whole.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.textBox_comment_whole);
            this.groupBox1.Controls.Add(this.textBox_comment_record);
            this.groupBox1.Controls.Add(this.radioButton_record);
            this.groupBox1.Controls.Add(this.radioButton_whole);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(344, 317);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "重建检索点的方式";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(268, 336);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(89, 28);
            this.button_Cancel.TabIndex = 13;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(173, 336);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(89, 28);
            this.button_OK.TabIndex = 12;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_comment_record
            // 
            this.textBox_comment_record.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_comment_record.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_comment_record.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_comment_record.Location = new System.Drawing.Point(36, 61);
            this.textBox_comment_record.Multiline = true;
            this.textBox_comment_record.Name = "textBox_comment_record";
            this.textBox_comment_record.ReadOnly = true;
            this.textBox_comment_record.Size = new System.Drawing.Size(275, 105);
            this.textBox_comment_record.TabIndex = 2;
            this.textBox_comment_record.Text = "重建检索点时不清除数据库的全部检索点， 对数据库的日常使用基本没有影响。\r\n\r\n重建操作过程随时可以中断、继续。";
            // 
            // textBox_comment_whole
            // 
            this.textBox_comment_whole.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_comment_whole.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_comment_whole.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_comment_whole.Location = new System.Drawing.Point(36, 197);
            this.textBox_comment_whole.Multiline = true;
            this.textBox_comment_whole.Name = "textBox_comment_whole";
            this.textBox_comment_whole.ReadOnly = true;
            this.textBox_comment_whole.Size = new System.Drawing.Size(275, 97);
            this.textBox_comment_whole.TabIndex = 3;
            this.textBox_comment_whole.Text = "先清除了数据库的全部检索点，然后逐记录重建。\r\n\r\n在整批操作没有完成前，数据库处于不完整状态。\r\n";
            // 
            // RebuildKeysDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(369, 376);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "RebuildKeysDialog";
            this.ShowInTaskbar = false;
            this.Text = "重建检索点的方式";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton radioButton_record;
        private System.Windows.Forms.RadioButton radioButton_whole;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TextBox textBox_comment_record;
        private System.Windows.Forms.TextBox textBox_comment_whole;
    }
}