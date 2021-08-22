
namespace dp2ManageCenter.Message
{
    partial class GetFileDialog
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
            this.textBox_remotePath = new System.Windows.Forms.TextBox();
            this.textBox_localFileName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.button_getFileName = new System.Windows.Forms.Button();
            this.button_get = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel_message = new System.Windows.Forms.ToolStripStatusLabel();
            this.button_stop = new System.Windows.Forms.Button();
            this.comboBox_query_shelfAccount = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_query_myAccount = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 155);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(126, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "书柜端路径:";
            // 
            // textBox_remotePath
            // 
            this.textBox_remotePath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_remotePath.Location = new System.Drawing.Point(168, 152);
            this.textBox_remotePath.Name = "textBox_remotePath";
            this.textBox_remotePath.Size = new System.Drawing.Size(728, 31);
            this.textBox_remotePath.TabIndex = 1;
            // 
            // textBox_localFileName
            // 
            this.textBox_localFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_localFileName.Location = new System.Drawing.Point(168, 192);
            this.textBox_localFileName.Name = "textBox_localFileName";
            this.textBox_localFileName.Size = new System.Drawing.Size(653, 31);
            this.textBox_localFileName.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 195);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(105, 21);
            this.label2.TabIndex = 2;
            this.label2.Text = "本地文件:";
            // 
            // button_getFileName
            // 
            this.button_getFileName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_getFileName.Location = new System.Drawing.Point(827, 189);
            this.button_getFileName.Name = "button_getFileName";
            this.button_getFileName.Size = new System.Drawing.Size(69, 36);
            this.button_getFileName.TabIndex = 4;
            this.button_getFileName.Text = "...";
            this.button_getFileName.UseVisualStyleBackColor = true;
            this.button_getFileName.Click += new System.EventHandler(this.button_getFileName_Click);
            // 
            // button_get
            // 
            this.button_get.Location = new System.Drawing.Point(168, 262);
            this.button_get.Name = "button_get";
            this.button_get.Size = new System.Drawing.Size(110, 40);
            this.button_get.TabIndex = 5;
            this.button_get.Text = "获取";
            this.button_get.UseVisualStyleBackColor = true;
            this.button_get.Click += new System.EventHandler(this.button_get_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.ImageScalingSize = new System.Drawing.Size(28, 28);
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripProgressBar1,
            this.toolStripStatusLabel_message});
            this.statusStrip1.Location = new System.Drawing.Point(0, 392);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(908, 37);
            this.statusStrip1.TabIndex = 6;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // toolStripProgressBar1
            // 
            this.toolStripProgressBar1.Name = "toolStripProgressBar1";
            this.toolStripProgressBar1.Size = new System.Drawing.Size(300, 27);
            // 
            // toolStripStatusLabel_message
            // 
            this.toolStripStatusLabel_message.Name = "toolStripStatusLabel_message";
            this.toolStripStatusLabel_message.Size = new System.Drawing.Size(0, 28);
            // 
            // button_stop
            // 
            this.button_stop.Enabled = false;
            this.button_stop.Location = new System.Drawing.Point(284, 262);
            this.button_stop.Name = "button_stop";
            this.button_stop.Size = new System.Drawing.Size(110, 40);
            this.button_stop.TabIndex = 7;
            this.button_stop.Text = "中断";
            this.button_stop.UseVisualStyleBackColor = true;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // comboBox_query_shelfAccount
            // 
            this.comboBox_query_shelfAccount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_query_shelfAccount.FormattingEnabled = true;
            this.comboBox_query_shelfAccount.Location = new System.Drawing.Point(168, 50);
            this.comboBox_query_shelfAccount.Name = "comboBox_query_shelfAccount";
            this.comboBox_query_shelfAccount.Size = new System.Drawing.Size(728, 29);
            this.comboBox_query_shelfAccount.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 21);
            this.label3.TabIndex = 10;
            this.label3.Text = "书柜:";
            // 
            // comboBox_query_myAccount
            // 
            this.comboBox_query_myAccount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_query_myAccount.FormattingEnabled = true;
            this.comboBox_query_myAccount.Location = new System.Drawing.Point(168, 12);
            this.comboBox_query_myAccount.Name = "comboBox_query_myAccount";
            this.comboBox_query_myAccount.Size = new System.Drawing.Size(728, 29);
            this.comboBox_query_myAccount.TabIndex = 9;
            this.comboBox_query_myAccount.DropDown += new System.EventHandler(this.comboBox_query_myAccount_DropDown);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 15);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(84, 21);
            this.label4.TabIndex = 8;
            this.label4.Text = "账户名:";
            // 
            // GetFileDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(908, 429);
            this.Controls.Add(this.comboBox_query_shelfAccount);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBox_query_myAccount);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.button_get);
            this.Controls.Add(this.button_getFileName);
            this.Controls.Add(this.textBox_localFileName);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_remotePath);
            this.Controls.Add(this.label1);
            this.Name = "GetFileDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "获取文件";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.GetFileDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.GetFileDialog_FormClosed);
            this.Load += new System.EventHandler(this.GetFileDialog_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_remotePath;
        private System.Windows.Forms.TextBox textBox_localFileName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button button_getFileName;
        private System.Windows.Forms.Button button_get;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_message;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.ComboBox comboBox_query_shelfAccount;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_query_myAccount;
        private System.Windows.Forms.Label label4;
    }
}