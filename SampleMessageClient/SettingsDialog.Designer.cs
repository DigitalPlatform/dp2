
namespace SampleMessageClient
{
    partial class SettingsDialog
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
            this.tabPage_messageServer = new System.Windows.Forms.TabPage();
            this.textBox_messageServer_password = new System.Windows.Forms.TextBox();
            this.textBox_messageServer_userName = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_messageServer_url = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_messageServer.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabPage_messageServer
            // 
            this.tabPage_messageServer.AutoScroll = true;
            this.tabPage_messageServer.Controls.Add(this.textBox_messageServer_password);
            this.tabPage_messageServer.Controls.Add(this.textBox_messageServer_userName);
            this.tabPage_messageServer.Controls.Add(this.label6);
            this.tabPage_messageServer.Controls.Add(this.label7);
            this.tabPage_messageServer.Controls.Add(this.textBox_messageServer_url);
            this.tabPage_messageServer.Controls.Add(this.label8);
            this.tabPage_messageServer.Location = new System.Drawing.Point(4, 31);
            this.tabPage_messageServer.Name = "tabPage_messageServer";
            this.tabPage_messageServer.Size = new System.Drawing.Size(769, 537);
            this.tabPage_messageServer.TabIndex = 2;
            this.tabPage_messageServer.Text = "消息服务器";
            this.tabPage_messageServer.UseVisualStyleBackColor = true;
            // 
            // textBox_messageServer_password
            // 
            this.textBox_messageServer_password.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_messageServer_password.Location = new System.Drawing.Point(218, 181);
            this.textBox_messageServer_password.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_messageServer_password.Name = "textBox_messageServer_password";
            this.textBox_messageServer_password.PasswordChar = '*';
            this.textBox_messageServer_password.Size = new System.Drawing.Size(242, 31);
            this.textBox_messageServer_password.TabIndex = 24;
            // 
            // textBox_messageServer_userName
            // 
            this.textBox_messageServer_userName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_messageServer_userName.Location = new System.Drawing.Point(218, 140);
            this.textBox_messageServer_userName.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_messageServer_userName.Name = "textBox_messageServer_userName";
            this.textBox_messageServer_userName.Size = new System.Drawing.Size(242, 31);
            this.textBox_messageServer_userName.TabIndex = 22;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(15, 184);
            this.label6.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(106, 21);
            this.label6.TabIndex = 23;
            this.label6.Text = "密码(&P)：";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(15, 143);
            this.label7.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(127, 21);
            this.label7.TabIndex = 21;
            this.label7.Text = "用户名(&U)：";
            // 
            // textBox_messageServer_url
            // 
            this.textBox_messageServer_url.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_messageServer_url.Location = new System.Drawing.Point(218, 48);
            this.textBox_messageServer_url.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_messageServer_url.Name = "textBox_messageServer_url";
            this.textBox_messageServer_url.Size = new System.Drawing.Size(541, 31);
            this.textBox_messageServer_url.TabIndex = 20;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(15, 51);
            this.label8.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(170, 21);
            this.label8.TabIndex = 19;
            this.label8.Text = "消息服务器 URL:";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(678, 587);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(111, 40);
            this.button_Cancel.TabIndex = 8;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(561, 587);
            this.button_OK.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(111, 40);
            this.button_OK.TabIndex = 7;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage_messageServer);
            this.tabControl1.Location = new System.Drawing.Point(12, 11);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(777, 572);
            this.tabControl1.TabIndex = 6;
            // 
            // SettingsDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(801, 638);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.tabControl1);
            this.Name = "SettingsDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "设置";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SettingsDialog_FormClosed);
            this.Load += new System.EventHandler(this.SettingsDialog_Load);
            this.tabPage_messageServer.ResumeLayout(false);
            this.tabPage_messageServer.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabPage tabPage_messageServer;
        public System.Windows.Forms.TextBox textBox_messageServer_password;
        public System.Windows.Forms.TextBox textBox_messageServer_userName;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox_messageServer_url;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TabControl tabControl1;
    }
}