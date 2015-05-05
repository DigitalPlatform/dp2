namespace dp2Circulation
{
    partial class StartDkywReplicationDlg
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartPatronReplicationDlg));
            this.checkBox_startAtServerBreakPoint = new System.Windows.Forms.CheckBox();
            this.checkBox_loop = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textBox_startIndex = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // checkBox_startAtServerBreakPoint
            // 
            this.checkBox_startAtServerBreakPoint.AutoSize = true;
            this.checkBox_startAtServerBreakPoint.Checked = true;
            this.checkBox_startAtServerBreakPoint.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_startAtServerBreakPoint.Location = new System.Drawing.Point(8, 10);
            this.checkBox_startAtServerBreakPoint.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBox_startAtServerBreakPoint.Name = "checkBox_startAtServerBreakPoint";
            this.checkBox_startAtServerBreakPoint.Size = new System.Drawing.Size(198, 16);
            this.checkBox_startAtServerBreakPoint.TabIndex = 16;
            this.checkBox_startAtServerBreakPoint.Text = "从服务器保留的断点开始处理(&S)";
            this.checkBox_startAtServerBreakPoint.UseVisualStyleBackColor = true;
            this.checkBox_startAtServerBreakPoint.CheckedChanged += new System.EventHandler(this.checkBox_startAtServerBreakPoint_CheckedChanged);
            // 
            // checkBox_loop
            // 
            this.checkBox_loop.AutoSize = true;
            this.checkBox_loop.Location = new System.Drawing.Point(8, 148);
            this.checkBox_loop.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.checkBox_loop.Name = "checkBox_loop";
            this.checkBox_loop.Size = new System.Drawing.Size(114, 16);
            this.checkBox_loop.TabIndex = 21;
            this.checkBox_loop.Text = "任务循环执行(&L)";
            this.checkBox_loop.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.textBox_startIndex);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Location = new System.Drawing.Point(8, 37);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox1.Size = new System.Drawing.Size(308, 97);
            this.groupBox1.TabIndex = 17;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "处理起点 ";
            // 
            // textBox_startIndex
            // 
            this.textBox_startIndex.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_startIndex.Location = new System.Drawing.Point(155, 53);
            this.textBox_startIndex.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_startIndex.Name = "textBox_startIndex";
            this.textBox_startIndex.Size = new System.Drawing.Size(114, 21);
            this.textBox_startIndex.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 55);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(131, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "记录号 [IDNumber](&I):";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(261, 157);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 19;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(200, 157);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 18;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // StartDkywReplicationDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(326, 189);
            this.Controls.Add(this.checkBox_startAtServerBreakPoint);
            this.Controls.Add(this.checkBox_loop);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "StartDkywReplicationDlg";
            this.ShowInTaskbar = false;
            this.Text = "启动 迪科远望一卡通读者信息同步 任务";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.StartDkywReplicationDlg_FormClosed);
            this.Load += new System.EventHandler(this.StartDkywReplicationDlg_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox_startAtServerBreakPoint;
        private System.Windows.Forms.CheckBox checkBox_loop;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox textBox_startIndex;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
    }
}