namespace dp2Circulation
{
    partial class StartReplicationDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartReplicationDlg));
            this.label1 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_startFileName = new System.Windows.Forms.TextBox();
            this.checkBox_clearBefore = new System.Windows.Forms.CheckBox();
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_copyAndRep = new System.Windows.Forms.TabPage();
            this.label5 = new System.Windows.Forms.Label();
            this.tabPage_continue = new System.Windows.Forms.TabPage();
            this.label2 = new System.Windows.Forms.Label();
            this.tabPage_specDay = new System.Windows.Forms.TabPage();
            this.tabControl1.SuspendLayout();
            this.tabPage_copyAndRep.SuspendLayout();
            this.tabPage_continue.SuspendLayout();
            this.tabPage_specDay.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 18);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "日志文件名(&F):";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(107, 38);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(155, 12);
            this.label4.TabIndex = 4;
            this.label4.Text = "(注: 格式为 yyyymmdd.log)";
            // 
            // textBox_startFileName
            // 
            this.textBox_startFileName.Location = new System.Drawing.Point(109, 15);
            this.textBox_startFileName.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_startFileName.Name = "textBox_startFileName";
            this.textBox_startFileName.Size = new System.Drawing.Size(152, 21);
            this.textBox_startFileName.TabIndex = 3;
            // 
            // checkBox_clearBefore
            // 
            this.checkBox_clearBefore.AutoSize = true;
            this.checkBox_clearBefore.Location = new System.Drawing.Point(9, 188);
            this.checkBox_clearBefore.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_clearBefore.Name = "checkBox_clearBefore";
            this.checkBox_clearBefore.Size = new System.Drawing.Size(192, 16);
            this.checkBox_clearBefore.TabIndex = 3;
            this.checkBox_clearBefore.Text = "同步前 清除全部数据库记录(&C)";
            this.checkBox_clearBefore.UseVisualStyleBackColor = true;
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(177, 259);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 4;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(238, 259);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 5;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage_copyAndRep);
            this.tabControl1.Controls.Add(this.tabPage_continue);
            this.tabControl1.Controls.Add(this.tabPage_specDay);
            this.tabControl1.Location = new System.Drawing.Point(9, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(285, 171);
            this.tabControl1.TabIndex = 6;
            // 
            // tabPage_copyAndRep
            // 
            this.tabPage_copyAndRep.Controls.Add(this.label5);
            this.tabPage_copyAndRep.Location = new System.Drawing.Point(4, 22);
            this.tabPage_copyAndRep.Name = "tabPage_copyAndRep";
            this.tabPage_copyAndRep.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_copyAndRep.Size = new System.Drawing.Size(277, 145);
            this.tabPage_copyAndRep.TabIndex = 0;
            this.tabPage_copyAndRep.Text = "复制并同步";
            this.tabPage_copyAndRep.UseVisualStyleBackColor = true;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 22);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(281, 12);
            this.label5.TabIndex = 1;
            this.label5.Text = "从中心服务器复制全部书目记录记录，然后开始同步";
            // 
            // tabPage_continue
            // 
            this.tabPage_continue.Controls.Add(this.label2);
            this.tabPage_continue.Location = new System.Drawing.Point(4, 22);
            this.tabPage_continue.Name = "tabPage_continue";
            this.tabPage_continue.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_continue.Size = new System.Drawing.Size(277, 145);
            this.tabPage_continue.TabIndex = 1;
            this.tabPage_continue.Text = "继续";
            this.tabPage_continue.UseVisualStyleBackColor = true;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 19);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(173, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "从上次的断点位置继续向后同步";
            // 
            // tabPage_specDay
            // 
            this.tabPage_specDay.AutoScroll = true;
            this.tabPage_specDay.Controls.Add(this.textBox_startFileName);
            this.tabPage_specDay.Controls.Add(this.label1);
            this.tabPage_specDay.Controls.Add(this.label4);
            this.tabPage_specDay.Location = new System.Drawing.Point(4, 22);
            this.tabPage_specDay.Name = "tabPage_specDay";
            this.tabPage_specDay.Size = new System.Drawing.Size(277, 145);
            this.tabPage_specDay.TabIndex = 2;
            this.tabPage_specDay.Text = "指定日期";
            this.tabPage_specDay.UseVisualStyleBackColor = true;
            // 
            // StartReplicationDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(303, 291);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.checkBox_clearBefore);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "StartReplicationDlg";
            this.ShowInTaskbar = false;
            this.Text = "启动 dp2Library 同步 任务";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.StartReplicationDlg_FormClosed);
            this.Load += new System.EventHandler(this.StartReplicationDlg_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_copyAndRep.ResumeLayout(false);
            this.tabPage_copyAndRep.PerformLayout();
            this.tabPage_continue.ResumeLayout(false);
            this.tabPage_continue.PerformLayout();
            this.tabPage_specDay.ResumeLayout(false);
            this.tabPage_specDay.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_startFileName;
        private System.Windows.Forms.CheckBox checkBox_clearBefore;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_copyAndRep;
        private System.Windows.Forms.TabPage tabPage_continue;
        private System.Windows.Forms.TabPage tabPage_specDay;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label2;
    }
}