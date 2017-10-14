namespace dp2Circulation
{
    partial class StartServerReplicationDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartServerReplicationDlg));
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_startIndex = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_startDate = new System.Windows.Forms.TextBox();
            this.label_recoverLevel = new System.Windows.Forms.Label();
            this.comboBox_recoverLevel = new System.Windows.Forms.ComboBox();
            this.checkBox_clearBefore = new System.Windows.Forms.CheckBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.checkBox_continueWhenError = new System.Windows.Forms.CheckBox();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(4, 25);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "日期(&D):";
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.textBox_startIndex);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBox_startDate);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(9, 10);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(310, 141);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " 起点 ";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.BackColor = System.Drawing.SystemColors.Info;
            this.label4.ForeColor = System.Drawing.SystemColors.InfoText;
            this.label4.Location = new System.Drawing.Point(100, 45);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(179, 24);
            this.label4.TabIndex = 2;
            this.label4.Text = "格式为 yyyymmdd ，共 8 字符。\r\n空表示从上次断点位置继续";
            // 
            // textBox_startIndex
            // 
            this.textBox_startIndex.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_startIndex.Location = new System.Drawing.Point(102, 81);
            this.textBox_startIndex.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_startIndex.Name = "textBox_startIndex";
            this.textBox_startIndex.Size = new System.Drawing.Size(116, 21);
            this.textBox_startIndex.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(4, 84);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "记录序号(&I):";
            // 
            // textBox_startFileName
            // 
            this.textBox_startDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_startDate.Location = new System.Drawing.Point(102, 22);
            this.textBox_startDate.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_startDate.Name = "textBox_startFileName";
            this.textBox_startDate.Size = new System.Drawing.Size(148, 21);
            this.textBox_startDate.TabIndex = 1;
            // 
            // label_recoverLevel
            // 
            this.label_recoverLevel.AutoSize = true;
            this.label_recoverLevel.Location = new System.Drawing.Point(9, 178);
            this.label_recoverLevel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label_recoverLevel.Name = "label_recoverLevel";
            this.label_recoverLevel.Size = new System.Drawing.Size(77, 12);
            this.label_recoverLevel.TabIndex = 1;
            this.label_recoverLevel.Text = "恢复级别(&L):";
            // 
            // comboBox_recoverLevel
            // 
            this.comboBox_recoverLevel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_recoverLevel.FormattingEnabled = true;
            this.comboBox_recoverLevel.Items.AddRange(new object[] {
            "Logic(逻辑)",
            "LogicAndSnapshot(逻辑+快照)",
            "Snapshot(快照)",
            "Robust(容错)"});
            this.comboBox_recoverLevel.Location = new System.Drawing.Point(113, 176);
            this.comboBox_recoverLevel.Margin = new System.Windows.Forms.Padding(2);
            this.comboBox_recoverLevel.Name = "comboBox_recoverLevel";
            this.comboBox_recoverLevel.Size = new System.Drawing.Size(211, 20);
            this.comboBox_recoverLevel.TabIndex = 2;
            this.comboBox_recoverLevel.Text = "Snapshot(快照)";
            // 
            // checkBox_clearBefore
            // 
            this.checkBox_clearBefore.AutoSize = true;
            this.checkBox_clearBefore.Location = new System.Drawing.Point(11, 210);
            this.checkBox_clearBefore.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_clearBefore.Name = "checkBox_clearBefore";
            this.checkBox_clearBefore.Size = new System.Drawing.Size(192, 16);
            this.checkBox_clearBefore.TabIndex = 3;
            this.checkBox_clearBefore.Text = "恢复前 清除全部数据库记录(&C)";
            this.checkBox_clearBefore.UseVisualStyleBackColor = true;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(204, 281);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 5;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(265, 281);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 6;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // checkBox_continueWhenError
            // 
            this.checkBox_continueWhenError.AutoSize = true;
            this.checkBox_continueWhenError.Location = new System.Drawing.Point(11, 230);
            this.checkBox_continueWhenError.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_continueWhenError.Name = "checkBox_continueWhenError";
            this.checkBox_continueWhenError.Size = new System.Drawing.Size(138, 16);
            this.checkBox_continueWhenError.TabIndex = 4;
            this.checkBox_continueWhenError.Text = "出错后继续批处理(&T)";
            this.checkBox_continueWhenError.UseVisualStyleBackColor = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.BackColor = System.Drawing.SystemColors.Info;
            this.label3.ForeColor = System.Drawing.SystemColors.InfoText;
            this.label3.Location = new System.Drawing.Point(100, 104);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "从 0 开始计数";
            // 
            // StartServerReplicationDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(330, 313);
            this.Controls.Add(this.checkBox_continueWhenError);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.checkBox_clearBefore);
            this.Controls.Add(this.comboBox_recoverLevel);
            this.Controls.Add(this.label_recoverLevel);
            this.Controls.Add(this.groupBox1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "StartServerReplicationDlg";
            this.ShowInTaskbar = false;
            this.Text = "启动 日志恢复 任务";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.StartLogRecoverDlg_FormClosed);
            this.Load += new System.EventHandler(this.StartLogRecoverDlg_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox_startDate;
        private System.Windows.Forms.TextBox textBox_startIndex;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label_recoverLevel;
        private System.Windows.Forms.ComboBox comboBox_recoverLevel;
        private System.Windows.Forms.CheckBox checkBox_clearBefore;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox checkBox_continueWhenError;
        private System.Windows.Forms.Label label3;
    }
}