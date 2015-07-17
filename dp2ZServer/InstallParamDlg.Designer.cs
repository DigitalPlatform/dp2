namespace dp2ZServer
{
    partial class InstallParamDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(InstallParamDlg));
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_detectManageUser = new System.Windows.Forms.Button();
            this.textBox_managePassword = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_manageUserName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_librarywsUrl = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.button_detectAnonymousUser = new System.Windows.Forms.Button();
            this.textBox_anonymousPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_anonymousUserName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(375, 328);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(56, 23);
            this.button_Cancel.TabIndex = 24;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(314, 328);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(56, 23);
            this.button_OK.TabIndex = 23;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.button_detectManageUser);
            this.groupBox1.Controls.Add(this.textBox_managePassword);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBox_manageUserName);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(10, 61);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox1.Size = new System.Drawing.Size(421, 118);
            this.groupBox1.TabIndex = 22;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Z39.50 服务器管理帐户";
            // 
            // button_detectManageUser
            // 
            this.button_detectManageUser.Location = new System.Drawing.Point(112, 80);
            this.button_detectManageUser.Margin = new System.Windows.Forms.Padding(2);
            this.button_detectManageUser.Name = "button_detectManageUser";
            this.button_detectManageUser.Size = new System.Drawing.Size(81, 23);
            this.button_detectManageUser.TabIndex = 6;
            this.button_detectManageUser.Text = "检测(&D)";
            this.button_detectManageUser.UseVisualStyleBackColor = true;
            this.button_detectManageUser.Click += new System.EventHandler(this.button_detectManageUser_Click);
            // 
            // textBox_managePassword
            // 
            this.textBox_managePassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_managePassword.Location = new System.Drawing.Point(112, 55);
            this.textBox_managePassword.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_managePassword.Name = "textBox_managePassword";
            this.textBox_managePassword.PasswordChar = '*';
            this.textBox_managePassword.Size = new System.Drawing.Size(242, 21);
            this.textBox_managePassword.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(14, 58);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "密码(&P):";
            // 
            // textBox_manageUserName
            // 
            this.textBox_manageUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_manageUserName.Location = new System.Drawing.Point(112, 30);
            this.textBox_manageUserName.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_manageUserName.Name = "textBox_manageUserName";
            this.textBox_manageUserName.Size = new System.Drawing.Size(242, 21);
            this.textBox_manageUserName.TabIndex = 1;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(14, 33);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 0;
            this.label4.Text = "用户名(&U):";
            // 
            // textBox_librarywsUrl
            // 
            this.textBox_librarywsUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_librarywsUrl.Location = new System.Drawing.Point(9, 25);
            this.textBox_librarywsUrl.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_librarywsUrl.Name = "textBox_librarywsUrl";
            this.textBox_librarywsUrl.Size = new System.Drawing.Size(422, 21);
            this.textBox_librarywsUrl.TabIndex = 21;
            this.textBox_librarywsUrl.Text = "http://localhost:8001/dp2library";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(155, 12);
            this.label1.TabIndex = 20;
            this.label1.Text = "dp2Library 服务器 URL(&U):";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.button_detectAnonymousUser);
            this.groupBox2.Controls.Add(this.textBox_anonymousPassword);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.textBox_anonymousUserName);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Location = new System.Drawing.Point(10, 192);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(2);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(2);
            this.groupBox2.Size = new System.Drawing.Size(421, 118);
            this.groupBox2.TabIndex = 25;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "代表匿名登录的 dp2Library 帐户 (如果设置用户名为空，表示不允许Z39.50前端匿名登录)";
            // 
            // button_detectAnonymousUser
            // 
            this.button_detectAnonymousUser.Location = new System.Drawing.Point(112, 80);
            this.button_detectAnonymousUser.Margin = new System.Windows.Forms.Padding(2);
            this.button_detectAnonymousUser.Name = "button_detectAnonymousUser";
            this.button_detectAnonymousUser.Size = new System.Drawing.Size(81, 23);
            this.button_detectAnonymousUser.TabIndex = 6;
            this.button_detectAnonymousUser.Text = "检测(&T)";
            this.button_detectAnonymousUser.UseVisualStyleBackColor = true;
            this.button_detectAnonymousUser.Click += new System.EventHandler(this.button_detectAnonymousUser_Click);
            // 
            // textBox_anonymousPassword
            // 
            this.textBox_anonymousPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_anonymousPassword.Location = new System.Drawing.Point(112, 55);
            this.textBox_anonymousPassword.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_anonymousPassword.Name = "textBox_anonymousPassword";
            this.textBox_anonymousPassword.PasswordChar = '*';
            this.textBox_anonymousPassword.Size = new System.Drawing.Size(242, 21);
            this.textBox_anonymousPassword.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(14, 58);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(53, 12);
            this.label3.TabIndex = 2;
            this.label3.Text = "密码(&P):";
            // 
            // textBox_anonymousUserName
            // 
            this.textBox_anonymousUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_anonymousUserName.Location = new System.Drawing.Point(112, 30);
            this.textBox_anonymousUserName.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_anonymousUserName.Name = "textBox_anonymousUserName";
            this.textBox_anonymousUserName.Size = new System.Drawing.Size(242, 21);
            this.textBox_anonymousUserName.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(14, 33);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "用户名(&U):";
            // 
            // InstallParamDlg
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(439, 362);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.textBox_librarywsUrl);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "InstallParamDlg";
            this.Text = "请指定 dp2ZServer 安装参数";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button_detectManageUser;
        private System.Windows.Forms.TextBox textBox_managePassword;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_manageUserName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_librarywsUrl;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button button_detectAnonymousUser;
        private System.Windows.Forms.TextBox textBox_anonymousPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_anonymousUserName;
        private System.Windows.Forms.Label label5;
    }
}