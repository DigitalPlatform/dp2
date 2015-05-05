#if NO
namespace dp2Kernel
{
    partial class SaLoginDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SaLoginDialog));
            this.radioButton_sqlAccount = new System.Windows.Forms.RadioButton();
            this.radioButton_SSPI = new System.Windows.Forms.RadioButton();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_sqlUserName = new System.Windows.Forms.TextBox();
            this.textBox_sqlPassword = new System.Windows.Forms.TextBox();
            this.button_detect = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_message = new System.Windows.Forms.TextBox();
            this.textBox_sqlServerName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // radioButton_sqlAccount
            // 
            this.radioButton_sqlAccount.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButton_sqlAccount.AutoSize = true;
            this.radioButton_sqlAccount.Location = new System.Drawing.Point(22, 49);
            this.radioButton_sqlAccount.Name = "radioButton_sqlAccount";
            this.radioButton_sqlAccount.Size = new System.Drawing.Size(89, 16);
            this.radioButton_sqlAccount.TabIndex = 1;
            this.radioButton_sqlAccount.Text = "SQL账号(&S):";
            this.radioButton_sqlAccount.UseVisualStyleBackColor = true;
            this.radioButton_sqlAccount.CheckedChanged += new System.EventHandler(this.radioButton_sqlAccount_CheckedChanged);
            // 
            // radioButton_SSPI
            // 
            this.radioButton_SSPI.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.radioButton_SSPI.AutoSize = true;
            this.radioButton_SSPI.Checked = true;
            this.radioButton_SSPI.Location = new System.Drawing.Point(22, 27);
            this.radioButton_SSPI.Name = "radioButton_SSPI";
            this.radioButton_SSPI.Size = new System.Drawing.Size(155, 16);
            this.radioButton_SSPI.TabIndex = 0;
            this.radioButton_SSPI.TabStop = true;
            this.radioButton_SSPI.Text = "Windows集成权限认证(&W)";
            this.radioButton_SSPI.UseVisualStyleBackColor = true;
            this.radioButton_SSPI.CheckedChanged += new System.EventHandler(this.radioButton_SSPI_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.textBox_sqlUserName);
            this.groupBox1.Controls.Add(this.textBox_sqlPassword);
            this.groupBox1.Location = new System.Drawing.Point(59, 64);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(300, 100);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 0;
            this.label2.Text = "用户名(&U):";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(11, 63);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "密码(&P):";
            // 
            // textBox_sqlUserName
            // 
            this.textBox_sqlUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_sqlUserName.Location = new System.Drawing.Point(95, 28);
            this.textBox_sqlUserName.Name = "textBox_sqlUserName";
            this.textBox_sqlUserName.Size = new System.Drawing.Size(138, 21);
            this.textBox_sqlUserName.TabIndex = 1;
            this.textBox_sqlUserName.Text = "sa";
            // 
            // textBox_sqlPassword
            // 
            this.textBox_sqlPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_sqlPassword.Location = new System.Drawing.Point(95, 60);
            this.textBox_sqlPassword.Name = "textBox_sqlPassword";
            this.textBox_sqlPassword.PasswordChar = '*';
            this.textBox_sqlPassword.Size = new System.Drawing.Size(138, 21);
            this.textBox_sqlPassword.TabIndex = 3;
            // 
            // button_detect
            // 
            this.button_detect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button_detect.Location = new System.Drawing.Point(330, 11);
            this.button_detect.Name = "button_detect";
            this.button_detect.Size = new System.Drawing.Size(75, 22);
            this.button_detect.TabIndex = 3;
            this.button_detect.Text = "检测(&D)";
            this.button_detect.UseVisualStyleBackColor = true;
            this.button_detect.Click += new System.EventHandler(this.button_detect_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(346, 307);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 22);
            this.button_Cancel.TabIndex = 5;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(265, 307);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 22);
            this.button_OK.TabIndex = 4;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_message
            // 
            this.textBox_message.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_message.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_message.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_message.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_message.Location = new System.Drawing.Point(10, 10);
            this.textBox_message.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.textBox_message.Multiline = true;
            this.textBox_message.Name = "textBox_message";
            this.textBox_message.ReadOnly = true;
            this.textBox_message.Size = new System.Drawing.Size(413, 69);
            this.textBox_message.TabIndex = 0;
            this.textBox_message.Text = "为创建数据库内核专用的SQL登录名，安装程序需要知道SQL Server管理员的登录方式。";
            // 
            // textBox_sqlServerName
            // 
            this.textBox_sqlServerName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_sqlServerName.Location = new System.Drawing.Point(109, 84);
            this.textBox_sqlServerName.Name = "textBox_sqlServerName";
            this.textBox_sqlServerName.ReadOnly = true;
            this.textBox_sqlServerName.Size = new System.Drawing.Size(312, 21);
            this.textBox_sqlServerName.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 87);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(95, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "SQL服务器名(&S):";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.button_detect);
            this.groupBox2.Controls.Add(this.radioButton_SSPI);
            this.groupBox2.Controls.Add(this.radioButton_sqlAccount);
            this.groupBox2.Controls.Add(this.groupBox1);
            this.groupBox2.Location = new System.Drawing.Point(10, 119);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.groupBox2.Size = new System.Drawing.Size(410, 177);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = " SQL Server管理员(超级用户)登录方式 ";
            // 
            // SaLoginDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(432, 341);
            this.Controls.Add(this.textBox_sqlServerName);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_message);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.groupBox2);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.Name = "SaLoginDialog";
            this.ShowInTaskbar = false;
            this.Text = "请指定SQL Server管理员的登录方式";
            this.Load += new System.EventHandler(this.SaLoginDialog_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.RadioButton radioButton_sqlAccount;
        private System.Windows.Forms.RadioButton radioButton_SSPI;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button_detect;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_sqlUserName;
        private System.Windows.Forms.TextBox textBox_sqlPassword;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TextBox textBox_message;
        private System.Windows.Forms.TextBox textBox_sqlServerName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
    }
}

#endif