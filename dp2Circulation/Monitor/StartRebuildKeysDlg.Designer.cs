namespace dp2Circulation
{
    partial class StartRebuildKeysDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartRebuildKeysDlg));
            this.checkBox_startAtServerBreakPoint = new System.Windows.Forms.CheckBox();
            this.textBox_dbNameList = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // checkBox_startAtServerBreakPoint
            // 
            this.checkBox_startAtServerBreakPoint.AutoSize = true;
            this.checkBox_startAtServerBreakPoint.Location = new System.Drawing.Point(8, 10);
            this.checkBox_startAtServerBreakPoint.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_startAtServerBreakPoint.Name = "checkBox_startAtServerBreakPoint";
            this.checkBox_startAtServerBreakPoint.Size = new System.Drawing.Size(198, 16);
            this.checkBox_startAtServerBreakPoint.TabIndex = 16;
            this.checkBox_startAtServerBreakPoint.Text = "从服务器保留的断点开始处理(&S)";
            this.checkBox_startAtServerBreakPoint.UseVisualStyleBackColor = true;
            // 
            // textBox_dbNameList
            // 
            this.textBox_dbNameList.AcceptsReturn = true;
            this.textBox_dbNameList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dbNameList.Location = new System.Drawing.Point(8, 53);
            this.textBox_dbNameList.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_dbNameList.Multiline = true;
            this.textBox_dbNameList.Name = "textBox_dbNameList";
            this.textBox_dbNameList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_dbNameList.Size = new System.Drawing.Size(307, 100);
            this.textBox_dbNameList.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 39);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(143, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "数据库名(&D) [每行一个]:";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(261, 157);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
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
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 18;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // StartRebuildKeysDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(326, 189);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_dbNameList);
            this.Controls.Add(this.checkBox_startAtServerBreakPoint);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "StartRebuildKeysDlg";
            this.ShowInTaskbar = false;
            this.Text = "启动 重建检索点 任务";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.StartArriveMonitorDlg_FormClosed);
            this.Load += new System.EventHandler(this.StartArriveMonitorDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox_startAtServerBreakPoint;
        private System.Windows.Forms.TextBox textBox_dbNameList;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
    }
}