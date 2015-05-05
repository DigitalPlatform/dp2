namespace dp2Circulation
{
    partial class StartZhengyuanReplicationDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartZhengyuanReplicationDlg));
            this.checkBox_loop = new System.Windows.Forms.CheckBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.checkBox_forceDumpComplete = new System.Windows.Forms.CheckBox();
            this.checkBox_clearFirst = new System.Windows.Forms.CheckBox();
            this.checkBox_autoDumpDayChange = new System.Windows.Forms.CheckBox();
            this.checkBox_forceDumpDay = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // checkBox_loop
            // 
            this.checkBox_loop.AutoSize = true;
            this.checkBox_loop.Location = new System.Drawing.Point(11, 143);
            this.checkBox_loop.Name = "checkBox_loop";
            this.checkBox_loop.Size = new System.Drawing.Size(140, 19);
            this.checkBox_loop.TabIndex = 21;
            this.checkBox_loop.Text = "任务循环执行(&L)";
            this.checkBox_loop.UseVisualStyleBackColor = true;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(348, 207);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 28);
            this.button_Cancel.TabIndex = 19;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(267, 207);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 28);
            this.button_OK.TabIndex = 18;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // checkBox_forceDumpComplete
            // 
            this.checkBox_forceDumpComplete.AutoSize = true;
            this.checkBox_forceDumpComplete.Location = new System.Drawing.Point(11, 37);
            this.checkBox_forceDumpComplete.Name = "checkBox_forceDumpComplete";
            this.checkBox_forceDumpComplete.Size = new System.Drawing.Size(170, 19);
            this.checkBox_forceDumpComplete.TabIndex = 13;
            this.checkBox_forceDumpComplete.Text = "一次性处理完整表(&C)";
            this.checkBox_forceDumpComplete.UseVisualStyleBackColor = true;
            // 
            // checkBox_clearFirst
            // 
            this.checkBox_clearFirst.AutoSize = true;
            this.checkBox_clearFirst.Enabled = false;
            this.checkBox_clearFirst.Location = new System.Drawing.Point(11, 12);
            this.checkBox_clearFirst.Name = "checkBox_clearFirst";
            this.checkBox_clearFirst.Size = new System.Drawing.Size(170, 19);
            this.checkBox_clearFirst.TabIndex = 10;
            this.checkBox_clearFirst.Text = "清除原有读者记录(&C)";
            this.checkBox_clearFirst.UseVisualStyleBackColor = true;
            // 
            // checkBox_autoDumpDayChange
            // 
            this.checkBox_autoDumpDayChange.AutoSize = true;
            this.checkBox_autoDumpDayChange.Location = new System.Drawing.Point(11, 103);
            this.checkBox_autoDumpDayChange.Name = "checkBox_autoDumpDayChange";
            this.checkBox_autoDumpDayChange.Size = new System.Drawing.Size(215, 19);
            this.checkBox_autoDumpDayChange.TabIndex = 22;
            this.checkBox_autoDumpDayChange.Text = "定时自动处理当日增量表(&D)";
            this.checkBox_autoDumpDayChange.UseVisualStyleBackColor = true;
            // 
            // checkBox_forceDumpDay
            // 
            this.checkBox_forceDumpDay.AutoSize = true;
            this.checkBox_forceDumpDay.Location = new System.Drawing.Point(11, 62);
            this.checkBox_forceDumpDay.Name = "checkBox_forceDumpDay";
            this.checkBox_forceDumpDay.Size = new System.Drawing.Size(200, 19);
            this.checkBox_forceDumpDay.TabIndex = 23;
            this.checkBox_forceDumpDay.Text = "一次性处理当日增量表(&F)";
            this.checkBox_forceDumpDay.UseVisualStyleBackColor = true;
            // 
            // StartZhengyuanReplicationDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(435, 247);
            this.Controls.Add(this.checkBox_forceDumpDay);
            this.Controls.Add(this.checkBox_autoDumpDayChange);
            this.Controls.Add(this.checkBox_clearFirst);
            this.Controls.Add(this.checkBox_forceDumpComplete);
            this.Controls.Add(this.checkBox_loop);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "StartZhengyuanReplicationDlg";
            this.ShowInTaskbar = false;
            this.Text = "StartZhengyuanReplicationDlg";
            this.Load += new System.EventHandler(this.StartZhengyuanReplicationDlg_Load);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.StartZhengyuanReplicationDlg_FormClosed);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox checkBox_loop;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.CheckBox checkBox_forceDumpComplete;
        private System.Windows.Forms.CheckBox checkBox_clearFirst;
        private System.Windows.Forms.CheckBox checkBox_autoDumpDayChange;
        private System.Windows.Forms.CheckBox checkBox_forceDumpDay;
    }
}