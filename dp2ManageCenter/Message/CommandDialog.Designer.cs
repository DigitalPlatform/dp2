
namespace dp2ManageCenter.Message
{
    partial class CommandDialog
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
            this.textBox_command = new System.Windows.Forms.TextBox();
            this.button_send = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripProgressBar1 = new System.Windows.Forms.ToolStripProgressBar();
            this.toolStripStatusLabel_message = new System.Windows.Forms.ToolStripStatusLabel();
            this.button_stop = new System.Windows.Forms.Button();
            this.comboBox_targetAccount = new System.Windows.Forms.ComboBox();
            this.label3 = new System.Windows.Forms.Label();
            this.comboBox_myAccount = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 155);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(63, 21);
            this.label1.TabIndex = 0;
            this.label1.Text = "命令:";
            // 
            // textBox_command
            // 
            this.textBox_command.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_command.Location = new System.Drawing.Point(168, 152);
            this.textBox_command.Name = "textBox_command";
            this.textBox_command.Size = new System.Drawing.Size(728, 31);
            this.textBox_command.TabIndex = 1;
            // 
            // button_send
            // 
            this.button_send.Location = new System.Drawing.Point(168, 262);
            this.button_send.Name = "button_send";
            this.button_send.Size = new System.Drawing.Size(110, 40);
            this.button_send.TabIndex = 5;
            this.button_send.Text = "发送";
            this.button_send.UseVisualStyleBackColor = true;
            this.button_send.Click += new System.EventHandler(this.button_send_Click);
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
            this.button_stop.Visible = false;
            this.button_stop.Click += new System.EventHandler(this.button_stop_Click);
            // 
            // comboBox_targetAccount
            // 
            this.comboBox_targetAccount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_targetAccount.FormattingEnabled = true;
            this.comboBox_targetAccount.Location = new System.Drawing.Point(168, 50);
            this.comboBox_targetAccount.Name = "comboBox_targetAccount";
            this.comboBox_targetAccount.Size = new System.Drawing.Size(728, 29);
            this.comboBox_targetAccount.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 53);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(63, 21);
            this.label3.TabIndex = 10;
            this.label3.Text = "目标:";
            // 
            // comboBox_myAccount
            // 
            this.comboBox_myAccount.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_myAccount.FormattingEnabled = true;
            this.comboBox_myAccount.Location = new System.Drawing.Point(168, 12);
            this.comboBox_myAccount.Name = "comboBox_myAccount";
            this.comboBox_myAccount.Size = new System.Drawing.Size(728, 29);
            this.comboBox_myAccount.TabIndex = 9;
            this.comboBox_myAccount.DropDown += new System.EventHandler(this.comboBox_query_myAccount_DropDown);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(13, 15);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(126, 21);
            this.label4.TabIndex = 8;
            this.label4.Text = "我的账户名:";
            // 
            // CommandDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(908, 429);
            this.Controls.Add(this.comboBox_targetAccount);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.comboBox_myAccount);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.button_stop);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.button_send);
            this.Controls.Add(this.textBox_command);
            this.Controls.Add(this.label1);
            this.Name = "CommandDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "发送命令";
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
        private System.Windows.Forms.TextBox textBox_command;
        private System.Windows.Forms.Button button_send;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripProgressBar toolStripProgressBar1;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel_message;
        private System.Windows.Forms.Button button_stop;
        private System.Windows.Forms.ComboBox comboBox_targetAccount;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox comboBox_myAccount;
        private System.Windows.Forms.Label label4;
    }
}