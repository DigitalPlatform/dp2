namespace dp2Circulation
{
    partial class StartBackupDialog
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
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_dbNameList = new System.Windows.Forms.TextBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.checkBox_startAtServerBreakPoint = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_backupFileName = new System.Windows.Forms.ComboBox();
            this.checkBox_downloadFile = new System.Windows.Forms.CheckBox();
            this.button_delete = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 70);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(263, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "数据库名(&D) [每行一个。全空表示全部数据库]:";
            // 
            // textBox_dbNameList
            // 
            this.textBox_dbNameList.AcceptsReturn = true;
            this.textBox_dbNameList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_dbNameList.Location = new System.Drawing.Point(8, 84);
            this.textBox_dbNameList.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_dbNameList.Multiline = true;
            this.textBox_dbNameList.Name = "textBox_dbNameList";
            this.textBox_dbNameList.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_dbNameList.Size = new System.Drawing.Size(312, 100);
            this.textBox_dbNameList.TabIndex = 4;
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(266, 261);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 22);
            this.button_Cancel.TabIndex = 7;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(205, 261);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 22);
            this.button_OK.TabIndex = 6;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // checkBox_startAtServerBreakPoint
            // 
            this.checkBox_startAtServerBreakPoint.AutoSize = true;
            this.checkBox_startAtServerBreakPoint.Location = new System.Drawing.Point(8, 11);
            this.checkBox_startAtServerBreakPoint.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_startAtServerBreakPoint.Name = "checkBox_startAtServerBreakPoint";
            this.checkBox_startAtServerBreakPoint.Size = new System.Drawing.Size(198, 16);
            this.checkBox_startAtServerBreakPoint.TabIndex = 0;
            this.checkBox_startAtServerBreakPoint.Text = "从服务器保留的断点开始处理(&S)";
            this.checkBox_startAtServerBreakPoint.UseVisualStyleBackColor = true;
            this.checkBox_startAtServerBreakPoint.CheckedChanged += new System.EventHandler(this.checkBox_startAtServerBreakPoint_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 42);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "备份文件名:";
            // 
            // comboBox_backupFileName
            // 
            this.comboBox_backupFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_backupFileName.FormattingEnabled = true;
            this.comboBox_backupFileName.Location = new System.Drawing.Point(85, 39);
            this.comboBox_backupFileName.Name = "comboBox_backupFileName";
            this.comboBox_backupFileName.Size = new System.Drawing.Size(237, 20);
            this.comboBox_backupFileName.TabIndex = 2;
            // 
            // checkBox_downloadFile
            // 
            this.checkBox_downloadFile.AutoSize = true;
            this.checkBox_downloadFile.Location = new System.Drawing.Point(8, 212);
            this.checkBox_downloadFile.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_downloadFile.Name = "checkBox_downloadFile";
            this.checkBox_downloadFile.Size = new System.Drawing.Size(138, 16);
            this.checkBox_downloadFile.TabIndex = 5;
            this.checkBox_downloadFile.Text = "同时启动下载文件(&D)";
            this.checkBox_downloadFile.UseVisualStyleBackColor = true;
            // 
            // button_delete
            // 
            this.button_delete.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button_delete.Location = new System.Drawing.Point(11, 260);
            this.button_delete.Margin = new System.Windows.Forms.Padding(2);
            this.button_delete.Name = "button_delete";
            this.button_delete.Size = new System.Drawing.Size(96, 22);
            this.button_delete.TabIndex = 8;
            this.button_delete.Text = "停止并撤销";
            this.button_delete.UseVisualStyleBackColor = true;
            this.button_delete.Click += new System.EventHandler(this.button_abort_Click);
            // 
            // StartBackupDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(331, 293);
            this.Controls.Add(this.button_delete);
            this.Controls.Add(this.checkBox_downloadFile);
            this.Controls.Add(this.comboBox_backupFileName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkBox_startAtServerBreakPoint);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_dbNameList);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "StartBackupDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "启动大备份任务";
            this.Load += new System.EventHandler(this.StartBackupDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_dbNameList;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.CheckBox checkBox_startAtServerBreakPoint;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_backupFileName;
        private System.Windows.Forms.CheckBox checkBox_downloadFile;
        private System.Windows.Forms.Button button_delete;
    }
}