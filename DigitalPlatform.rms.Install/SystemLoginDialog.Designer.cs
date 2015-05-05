namespace DigitalPlatform.rms
{
    partial class SystemLoginDialog
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
            this.textBox_sqlServerName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_message = new System.Windows.Forms.TextBox();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.button_detect = new System.Windows.Forms.Button();
            this.textBox_sqlUserName = new System.Windows.Forms.TextBox();
            this.textBox_sqlPassword = new System.Windows.Forms.TextBox();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBox_sqlServerName
            // 
            this.textBox_sqlServerName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_sqlServerName.Location = new System.Drawing.Point(165, 84);
            this.textBox_sqlServerName.Name = "textBox_sqlServerName";
            this.textBox_sqlServerName.ReadOnly = true;
            this.textBox_sqlServerName.Size = new System.Drawing.Size(257, 21);
            this.textBox_sqlServerName.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 87);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(155, 12);
            this.label1.TabIndex = 7;
            this.label1.Text = "Oracle 数据库服务器名(&S):";
            // 
            // textBox_message
            // 
            this.textBox_message.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_message.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_message.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_message.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_message.Location = new System.Drawing.Point(11, 10);
            this.textBox_message.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_message.Multiline = true;
            this.textBox_message.Name = "textBox_message";
            this.textBox_message.ReadOnly = true;
            this.textBox_message.Size = new System.Drawing.Size(413, 69);
            this.textBox_message.TabIndex = 6;
            this.textBox_message.Text = "为完成安装，需要以管理员身份登录 Oracle 数据库服务器。";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(347, 307);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 11;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(266, 307);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 10;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.button_detect);
            this.groupBox2.Controls.Add(this.textBox_sqlUserName);
            this.groupBox2.Controls.Add(this.textBox_sqlPassword);
            this.groupBox2.Location = new System.Drawing.Point(11, 119);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(410, 177);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "管理员(超级用户)登录方式 ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(70, 69);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "用户名(&U):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(70, 104);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "密码(&P):";
            // 
            // button_detect
            // 
            this.button_detect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_detect.Location = new System.Drawing.Point(330, 11);
            this.button_detect.Name = "button_detect";
            this.button_detect.Size = new System.Drawing.Size(75, 23);
            this.button_detect.TabIndex = 3;
            this.button_detect.Text = "检测(&D)";
            this.button_detect.UseVisualStyleBackColor = true;
            this.button_detect.Click += new System.EventHandler(this.button_detect_Click);
            // 
            // textBox_sqlUserName
            // 
            this.textBox_sqlUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_sqlUserName.Location = new System.Drawing.Point(154, 69);
            this.textBox_sqlUserName.Name = "textBox_sqlUserName";
            this.textBox_sqlUserName.Size = new System.Drawing.Size(138, 21);
            this.textBox_sqlUserName.TabIndex = 1;
            this.textBox_sqlUserName.Text = "system";
            // 
            // textBox_sqlPassword
            // 
            this.textBox_sqlPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_sqlPassword.Location = new System.Drawing.Point(154, 101);
            this.textBox_sqlPassword.Name = "textBox_sqlPassword";
            this.textBox_sqlPassword.PasswordChar = '*';
            this.textBox_sqlPassword.Size = new System.Drawing.Size(138, 21);
            this.textBox_sqlPassword.TabIndex = 3;
            // 
            // SystemLoginDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(432, 341);
            this.Controls.Add(this.textBox_sqlServerName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_message);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.groupBox2);
            this.Name = "SystemLoginDialog";
            this.ShowInTaskbar = false;
            this.Text = "以 Oracle 数据库服务器管理员身份登录";
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_sqlServerName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_message;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button_detect;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_sqlUserName;
        private System.Windows.Forms.TextBox textBox_sqlPassword;
    }
}