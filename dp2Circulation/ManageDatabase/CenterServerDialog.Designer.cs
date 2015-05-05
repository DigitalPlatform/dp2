namespace dp2Circulation
{
    partial class CenterServerDialog
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
            this.button_OK = new System.Windows.Forms.Button();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_name = new System.Windows.Forms.TextBox();
            this.textBox_url = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_userName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_password = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_refid = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.checkBox_changePassword = new System.Windows.Forms.CheckBox();
            this.button_verify = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(263, 236);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 0;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(344, 236);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 1;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(77, 12);
            this.label1.TabIndex = 2;
            this.label1.Text = "服务器名(&N):";
            // 
            // textBox_name
            // 
            this.textBox_name.Location = new System.Drawing.Point(107, 12);
            this.textBox_name.Name = "textBox_name";
            this.textBox_name.Size = new System.Drawing.Size(214, 21);
            this.textBox_name.TabIndex = 3;
            // 
            // textBox_url
            // 
            this.textBox_url.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_url.Location = new System.Drawing.Point(107, 39);
            this.textBox_url.Name = "textBox_url";
            this.textBox_url.Size = new System.Drawing.Size(308, 21);
            this.textBox_url.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(89, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = "服务器 URL(&U):";
            // 
            // textBox_userName
            // 
            this.textBox_userName.Location = new System.Drawing.Point(107, 76);
            this.textBox_userName.Name = "textBox_userName";
            this.textBox_userName.Size = new System.Drawing.Size(134, 21);
            this.textBox_userName.TabIndex = 7;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 79);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 6;
            this.label3.Text = "用户名(&A):";
            // 
            // textBox_password
            // 
            this.textBox_password.Location = new System.Drawing.Point(107, 103);
            this.textBox_password.Name = "textBox_password";
            this.textBox_password.PasswordChar = '*';
            this.textBox_password.ReadOnly = true;
            this.textBox_password.Size = new System.Drawing.Size(134, 21);
            this.textBox_password.TabIndex = 9;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 106);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 8;
            this.label4.Text = "密码(&P):";
            // 
            // textBox_refid
            // 
            this.textBox_refid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_refid.Location = new System.Drawing.Point(107, 171);
            this.textBox_refid.Name = "textBox_refid";
            this.textBox_refid.ReadOnly = true;
            this.textBox_refid.Size = new System.Drawing.Size(312, 21);
            this.textBox_refid.TabIndex = 11;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 174);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(71, 12);
            this.label5.TabIndex = 10;
            this.label5.Text = "参考 ID(&R):";
            // 
            // checkBox_changePassword
            // 
            this.checkBox_changePassword.AutoSize = true;
            this.checkBox_changePassword.Location = new System.Drawing.Point(107, 131);
            this.checkBox_changePassword.Name = "checkBox_changePassword";
            this.checkBox_changePassword.Size = new System.Drawing.Size(90, 16);
            this.checkBox_changePassword.TabIndex = 12;
            this.checkBox_changePassword.Text = "修改密码(&C)";
            this.checkBox_changePassword.UseVisualStyleBackColor = true;
            this.checkBox_changePassword.CheckedChanged += new System.EventHandler(this.checkBox_changePassword_CheckedChanged);
            // 
            // button_verify
            // 
            this.button_verify.Location = new System.Drawing.Point(247, 76);
            this.button_verify.Name = "button_verify";
            this.button_verify.Size = new System.Drawing.Size(75, 23);
            this.button_verify.TabIndex = 13;
            this.button_verify.Text = "验证";
            this.button_verify.UseVisualStyleBackColor = true;
            this.button_verify.Click += new System.EventHandler(this.button_verify_Click);
            // 
            // CenterServerDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(427, 271);
            this.Controls.Add(this.button_verify);
            this.Controls.Add(this.checkBox_changePassword);
            this.Controls.Add(this.textBox_refid);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox_password);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_userName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_url);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_name);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Name = "CenterServerDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "中心服务器";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_name;
        private System.Windows.Forms.TextBox textBox_url;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_userName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_password;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_refid;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.CheckBox checkBox_changePassword;
        private System.Windows.Forms.Button button_verify;
    }
}