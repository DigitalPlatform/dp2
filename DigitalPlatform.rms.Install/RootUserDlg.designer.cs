namespace DigitalPlatform.rms
{
    partial class RootUserDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RootUserDlg));
            this.textBox_rights = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_confirmRootPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_rootPassword = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_rootUserName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button_cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox_rights
            // 
            this.textBox_rights.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_rights.Location = new System.Drawing.Point(102, 188);
            this.textBox_rights.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_rights.Multiline = true;
            this.textBox_rights.Name = "textBox_rights";
            this.textBox_rights.ReadOnly = true;
            this.textBox_rights.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_rights.Size = new System.Drawing.Size(216, 61);
            this.textBox_rights.TabIndex = 28;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 188);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(77, 12);
            this.label5.TabIndex = 27;
            this.label5.Text = "权限定义(&R):";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.label1.Location = new System.Drawing.Point(9, 7);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(309, 71);
            this.label1.TabIndex = 26;
            this.label1.Text = "安装过程需要创建一个root账户，这是一个具有最高权限的账户，您将用它在以后进行各种管理操作。";
            // 
            // textBox_confirmRootPassword
            // 
            this.textBox_confirmRootPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_confirmRootPassword.Location = new System.Drawing.Point(102, 154);
            this.textBox_confirmRootPassword.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_confirmRootPassword.Name = "textBox_confirmRootPassword";
            this.textBox_confirmRootPassword.PasswordChar = '*';
            this.textBox_confirmRootPassword.Size = new System.Drawing.Size(126, 21);
            this.textBox_confirmRootPassword.TabIndex = 25;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 156);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 12);
            this.label3.TabIndex = 24;
            this.label3.Text = "再次输入密码:";
            // 
            // textBox_rootPassword
            // 
            this.textBox_rootPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_rootPassword.Location = new System.Drawing.Point(102, 129);
            this.textBox_rootPassword.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_rootPassword.Name = "textBox_rootPassword";
            this.textBox_rootPassword.PasswordChar = '*';
            this.textBox_rootPassword.Size = new System.Drawing.Size(126, 21);
            this.textBox_rootPassword.TabIndex = 23;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 131);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 22;
            this.label2.Text = "密码(&P):";
            // 
            // textBox_rootUserName
            // 
            this.textBox_rootUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_rootUserName.Location = new System.Drawing.Point(102, 92);
            this.textBox_rootUserName.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_rootUserName.Name = "textBox_rootUserName";
            this.textBox_rootUserName.Size = new System.Drawing.Size(126, 21);
            this.textBox_rootUserName.TabIndex = 21;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 94);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 20;
            this.label4.Text = "用户名(&U):";
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.Location = new System.Drawing.Point(261, 271);
            this.button_cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(56, 23);
            this.button_cancel.TabIndex = 30;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_OK.Location = new System.Drawing.Point(200, 271);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 23);
            this.button_OK.TabIndex = 29;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // RootUserDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(326, 304);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.textBox_rights);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_confirmRootPassword);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_rootPassword);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_rootUserName);
            this.Controls.Add(this.label4);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "RootUserDlg";
            this.ShowInTaskbar = false;
            this.Text = "root账户";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_rights;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_confirmRootPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_rootPassword;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_rootUserName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.Button button_OK;
    }
}