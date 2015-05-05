namespace dp2Circulation
{
    partial class FtpUploadDialog
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
            this.textBox_ftpServerUrl = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_targetDir = new System.Windows.Forms.TextBox();
            this.textBox_userName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_password = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button_begin = new System.Windows.Forms.Button();
            this.checkBox_savePassword = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(113, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "FTP 服务器地址(&S):";
            // 
            // textBox_ftpServerUrl
            // 
            this.textBox_ftpServerUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_ftpServerUrl.Location = new System.Drawing.Point(13, 29);
            this.textBox_ftpServerUrl.Name = "textBox_ftpServerUrl";
            this.textBox_ftpServerUrl.Size = new System.Drawing.Size(342, 21);
            this.textBox_ftpServerUrl.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 60);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(77, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "目标目录(&T):";
            // 
            // textBox_targetDir
            // 
            this.textBox_targetDir.Location = new System.Drawing.Point(99, 57);
            this.textBox_targetDir.Name = "textBox_targetDir";
            this.textBox_targetDir.Size = new System.Drawing.Size(174, 21);
            this.textBox_targetDir.TabIndex = 3;
            // 
            // textBox_userName
            // 
            this.textBox_userName.Location = new System.Drawing.Point(99, 95);
            this.textBox_userName.Name = "textBox_userName";
            this.textBox_userName.Size = new System.Drawing.Size(118, 21);
            this.textBox_userName.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 98);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "用户名(&U):";
            // 
            // textBox_password
            // 
            this.textBox_password.Location = new System.Drawing.Point(99, 122);
            this.textBox_password.Name = "textBox_password";
            this.textBox_password.PasswordChar = '*';
            this.textBox_password.Size = new System.Drawing.Size(118, 21);
            this.textBox_password.TabIndex = 7;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 125);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "密码(&P):";
            // 
            // button_begin
            // 
            this.button_begin.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_begin.Location = new System.Drawing.Point(237, 229);
            this.button_begin.Name = "button_begin";
            this.button_begin.Size = new System.Drawing.Size(118, 23);
            this.button_begin.TabIndex = 9;
            this.button_begin.Text = "开始上传";
            this.button_begin.UseVisualStyleBackColor = true;
            this.button_begin.Click += new System.EventHandler(this.button_begin_Click);
            // 
            // checkBox_savePassword
            // 
            this.checkBox_savePassword.AutoSize = true;
            this.checkBox_savePassword.Location = new System.Drawing.Point(99, 150);
            this.checkBox_savePassword.Name = "checkBox_savePassword";
            this.checkBox_savePassword.Size = new System.Drawing.Size(72, 16);
            this.checkBox_savePassword.TabIndex = 10;
            this.checkBox_savePassword.Text = "保存密码";
            this.checkBox_savePassword.UseVisualStyleBackColor = true;
            // 
            // FtpUploadDialog
            // 
            this.AcceptButton = this.button_begin;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(367, 264);
            this.Controls.Add(this.checkBox_savePassword);
            this.Controls.Add(this.button_begin);
            this.Controls.Add(this.textBox_password);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_userName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_targetDir);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_ftpServerUrl);
            this.Controls.Add(this.label1);
            this.Name = "FtpUploadDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "FTP 上传";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.FtpUploadDialog_FormClosed);
            this.Load += new System.EventHandler(this.FtpUploadDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_ftpServerUrl;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_targetDir;
        private System.Windows.Forms.TextBox textBox_userName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_password;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_begin;
        private System.Windows.Forms.CheckBox checkBox_savePassword;
    }
}