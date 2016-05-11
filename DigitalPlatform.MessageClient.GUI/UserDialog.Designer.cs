namespace DigitalPlatform.MessageClient
{
    partial class UserDialog
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
            this.textBox_userName = new System.Windows.Forms.TextBox();
            this.textBox_password = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_confirmPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_department = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_rights = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.textBox_tel = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_comment = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_duty = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.checkBox_changePassword = new System.Windows.Forms.CheckBox();
            this.textBox_groups = new System.Windows.Forms.TextBox();
            this.label9 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(65, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "用户名(&N):";
            // 
            // textBox_userName
            // 
            this.textBox_userName.Location = new System.Drawing.Point(119, 12);
            this.textBox_userName.Name = "textBox_userName";
            this.textBox_userName.Size = new System.Drawing.Size(204, 21);
            this.textBox_userName.TabIndex = 1;
            this.textBox_userName.TextChanged += new System.EventHandler(this.textBox_comment_TextChanged);
            // 
            // textBox_password
            // 
            this.textBox_password.Location = new System.Drawing.Point(119, 39);
            this.textBox_password.Name = "textBox_password";
            this.textBox_password.PasswordChar = '*';
            this.textBox_password.Size = new System.Drawing.Size(204, 21);
            this.textBox_password.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "密码(&P):";
            // 
            // textBox_confirmPassword
            // 
            this.textBox_confirmPassword.Location = new System.Drawing.Point(119, 66);
            this.textBox_confirmPassword.Name = "textBox_confirmPassword";
            this.textBox_confirmPassword.PasswordChar = '*';
            this.textBox_confirmPassword.Size = new System.Drawing.Size(204, 21);
            this.textBox_confirmPassword.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 69);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "确认密码(&C):";
            // 
            // textBox_department
            // 
            this.textBox_department.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_department.Location = new System.Drawing.Point(119, 93);
            this.textBox_department.Name = "textBox_department";
            this.textBox_department.Size = new System.Drawing.Size(289, 21);
            this.textBox_department.TabIndex = 7;
            this.textBox_department.TextChanged += new System.EventHandler(this.textBox_comment_TextChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(12, 96);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(53, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "单位(&D):";
            // 
            // textBox_rights
            // 
            this.textBox_rights.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_rights.Location = new System.Drawing.Point(119, 120);
            this.textBox_rights.Name = "textBox_rights";
            this.textBox_rights.Size = new System.Drawing.Size(289, 21);
            this.textBox_rights.TabIndex = 9;
            this.textBox_rights.TextChanged += new System.EventHandler(this.textBox_comment_TextChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 123);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 8;
            this.label5.Text = "权限(&R):";
            // 
            // textBox_tel
            // 
            this.textBox_tel.Location = new System.Drawing.Point(119, 181);
            this.textBox_tel.Name = "textBox_tel";
            this.textBox_tel.Size = new System.Drawing.Size(204, 21);
            this.textBox_tel.TabIndex = 13;
            this.textBox_tel.TextChanged += new System.EventHandler(this.textBox_comment_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 184);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(53, 12);
            this.label6.TabIndex = 12;
            this.label6.Text = "电话(&T):";
            // 
            // textBox_comment
            // 
            this.textBox_comment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_comment.Location = new System.Drawing.Point(119, 208);
            this.textBox_comment.Multiline = true;
            this.textBox_comment.Name = "textBox_comment";
            this.textBox_comment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_comment.Size = new System.Drawing.Size(289, 61);
            this.textBox_comment.TabIndex = 15;
            this.textBox_comment.TextChanged += new System.EventHandler(this.textBox_comment_TextChanged);
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(12, 211);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(53, 12);
            this.label7.TabIndex = 14;
            this.label7.Text = "注释(&C):";
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.Location = new System.Drawing.Point(333, 377);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(75, 23);
            this.button_Cancel.TabIndex = 17;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(252, 377);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(75, 23);
            this.button_OK.TabIndex = 16;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_duty
            // 
            this.textBox_duty.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_duty.Location = new System.Drawing.Point(119, 147);
            this.textBox_duty.Name = "textBox_duty";
            this.textBox_duty.Size = new System.Drawing.Size(289, 21);
            this.textBox_duty.TabIndex = 11;
            this.textBox_duty.TextChanged += new System.EventHandler(this.textBox_comment_TextChanged);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(12, 150);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(53, 12);
            this.label8.TabIndex = 10;
            this.label8.Text = "义务(&D):";
            // 
            // checkBox_changePassword
            // 
            this.checkBox_changePassword.AutoSize = true;
            this.checkBox_changePassword.Location = new System.Drawing.Point(329, 41);
            this.checkBox_changePassword.Name = "checkBox_changePassword";
            this.checkBox_changePassword.Size = new System.Drawing.Size(72, 16);
            this.checkBox_changePassword.TabIndex = 18;
            this.checkBox_changePassword.Text = "修改密码";
            this.checkBox_changePassword.UseVisualStyleBackColor = true;
            this.checkBox_changePassword.CheckedChanged += new System.EventHandler(this.checkBox_changePassword_CheckedChanged);
            // 
            // textBox_groups
            // 
            this.textBox_groups.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_groups.Location = new System.Drawing.Point(119, 275);
            this.textBox_groups.Multiline = true;
            this.textBox_groups.Name = "textBox_groups";
            this.textBox_groups.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_groups.Size = new System.Drawing.Size(289, 75);
            this.textBox_groups.TabIndex = 20;
            this.textBox_groups.TextChanged += new System.EventHandler(this.textBox_comment_TextChanged);
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(12, 278);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(53, 12);
            this.label9.TabIndex = 19;
            this.label9.Text = "群组(&G):";
            // 
            // UserDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(420, 412);
            this.Controls.Add(this.textBox_groups);
            this.Controls.Add(this.label9);
            this.Controls.Add(this.checkBox_changePassword);
            this.Controls.Add(this.textBox_duty);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.textBox_comment);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBox_tel);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.textBox_rights);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.textBox_department);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.textBox_confirmPassword);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_password);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_userName);
            this.Controls.Add(this.label1);
            this.Name = "UserDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "用户";
            this.Load += new System.EventHandler(this.UserDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_userName;
        private System.Windows.Forms.TextBox textBox_password;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_confirmPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_department;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_rights;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_tel;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBox_comment;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TextBox textBox_duty;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.CheckBox checkBox_changePassword;
        private System.Windows.Forms.TextBox textBox_groups;
        private System.Windows.Forms.Label label9;
    }
}