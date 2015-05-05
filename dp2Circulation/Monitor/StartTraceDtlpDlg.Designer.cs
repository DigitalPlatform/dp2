namespace dp2Circulation
{
    partial class StartTraceDtlpDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartTraceDtlpDlg));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.checkBox_clearBefore = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox_startIndex = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_startFileName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBox_startAtServerBreakPoint = new System.Windows.Forms.CheckBox();
            this.checkBox_dump = new System.Windows.Forms.CheckBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.checkBox_loop = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(349, 366);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 28);
            this.button_Cancel.TabIndex = 12;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(268, 366);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 28);
            this.button_OK.TabIndex = 11;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // checkBox_clearBefore
            // 
            this.checkBox_clearBefore.AutoSize = true;
            this.checkBox_clearBefore.Location = new System.Drawing.Point(19, 34);
            this.checkBox_clearBefore.Name = "checkBox_clearBefore";
            this.checkBox_clearBefore.Size = new System.Drawing.Size(254, 19);
            this.checkBox_clearBefore.TabIndex = 10;
            this.checkBox_clearBefore.Text = "1) 清除全部目标数据库内记录(&C)";
            this.checkBox_clearBefore.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.textBox_startIndex);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBox_startFileName);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Location = new System.Drawing.Point(12, 189);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(411, 121);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = " 日志跟踪起点 ";
            // 
            // textBox_startIndex
            // 
            this.textBox_startIndex.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_startIndex.Location = new System.Drawing.Point(155, 66);
            this.textBox_startIndex.Name = "textBox_startIndex";
            this.textBox_startIndex.Size = new System.Drawing.Size(151, 25);
            this.textBox_startIndex.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(114, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "记录索引号(&I):";
            // 
            // textBox_startFileName
            // 
            this.textBox_startFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_startFileName.Location = new System.Drawing.Point(155, 29);
            this.textBox_startFileName.Name = "textBox_startFileName";
            this.textBox_startFileName.Size = new System.Drawing.Size(193, 25);
            this.textBox_startFileName.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 32);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 15);
            this.label1.TabIndex = 0;
            this.label1.Text = "日志文件名(&F):";
            // 
            // checkBox_startAtServerBreakPoint
            // 
            this.checkBox_startAtServerBreakPoint.AutoSize = true;
            this.checkBox_startAtServerBreakPoint.Location = new System.Drawing.Point(12, 12);
            this.checkBox_startAtServerBreakPoint.Name = "checkBox_startAtServerBreakPoint";
            this.checkBox_startAtServerBreakPoint.Size = new System.Drawing.Size(245, 19);
            this.checkBox_startAtServerBreakPoint.TabIndex = 4;
            this.checkBox_startAtServerBreakPoint.Text = "从服务器保留的断点开始跟踪(&S)";
            this.checkBox_startAtServerBreakPoint.UseVisualStyleBackColor = true;
            this.checkBox_startAtServerBreakPoint.CheckedChanged += new System.EventHandler(this.checkBox_startAtServerBreakPoint_CheckedChanged);
            // 
            // checkBox_dump
            // 
            this.checkBox_dump.AutoSize = true;
            this.checkBox_dump.Location = new System.Drawing.Point(19, 59);
            this.checkBox_dump.Name = "checkBox_dump";
            this.checkBox_dump.Size = new System.Drawing.Size(239, 19);
            this.checkBox_dump.TabIndex = 13;
            this.checkBox_dump.Text = "2) 复制全部源数据库内记录(&D)";
            this.checkBox_dump.UseVisualStyleBackColor = true;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.checkBox_dump);
            this.groupBox2.Controls.Add(this.checkBox_clearBefore);
            this.groupBox2.Location = new System.Drawing.Point(12, 53);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(410, 100);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = " 日志跟踪进行前先要 ";
            // 
            // checkBox_loop
            // 
            this.checkBox_loop.AutoSize = true;
            this.checkBox_loop.Location = new System.Drawing.Point(12, 331);
            this.checkBox_loop.Name = "checkBox_loop";
            this.checkBox_loop.Size = new System.Drawing.Size(140, 19);
            this.checkBox_loop.TabIndex = 15;
            this.checkBox_loop.Text = "任务循环执行(&L)";
            this.checkBox_loop.UseVisualStyleBackColor = true;
            // 
            // StartTraceDtlpDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(435, 406);
            this.Controls.Add(this.checkBox_startAtServerBreakPoint);
            this.Controls.Add(this.checkBox_loop);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.groupBox2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "StartTraceDtlpDlg";
            this.ShowInTaskbar = false;
            this.Text = "启动 跟踪DTLP 任务";
            this.Load += new System.EventHandler(this.StartTraceDtlpDlg_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.StartTraceDtlpDlg_FormClosed);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.CheckBox checkBox_clearBefore;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox_startIndex;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_startFileName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox checkBox_dump;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.CheckBox checkBox_startAtServerBreakPoint;
        private System.Windows.Forms.CheckBox checkBox_loop;
    }
}