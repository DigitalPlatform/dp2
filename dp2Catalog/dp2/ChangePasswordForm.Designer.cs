namespace dp2Catalog
{
    partial class ChangePasswordForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ChangePasswordForm));
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_dp2library = new System.Windows.Forms.TabPage();
            this.button_dp2library_findServerName = new System.Windows.Forms.Button();
            this.textBox_dp2library_serverName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.checkBox_dp2library_force = new System.Windows.Forms.CheckBox();
            this.button_dp2library_changePassword = new System.Windows.Forms.Button();
            this.textBox_dp2library_confirmNewPassword = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_dp2library_oldPassword = new System.Windows.Forms.TextBox();
            this.textBox_dp2library_newPassword = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textBox_dp2library_userName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tabControl_main.SuspendLayout();
            this.tabPage_dp2library.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_dp2library);
            this.tabControl_main.Location = new System.Drawing.Point(12, 12);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(473, 354);
            this.tabControl_main.TabIndex = 10;
            // 
            // tabPage_dp2library
            // 
            this.tabPage_dp2library.Controls.Add(this.button_dp2library_findServerName);
            this.tabPage_dp2library.Controls.Add(this.textBox_dp2library_serverName);
            this.tabPage_dp2library.Controls.Add(this.label1);
            this.tabPage_dp2library.Controls.Add(this.checkBox_dp2library_force);
            this.tabPage_dp2library.Controls.Add(this.button_dp2library_changePassword);
            this.tabPage_dp2library.Controls.Add(this.textBox_dp2library_confirmNewPassword);
            this.tabPage_dp2library.Controls.Add(this.label6);
            this.tabPage_dp2library.Controls.Add(this.label7);
            this.tabPage_dp2library.Controls.Add(this.textBox_dp2library_oldPassword);
            this.tabPage_dp2library.Controls.Add(this.textBox_dp2library_newPassword);
            this.tabPage_dp2library.Controls.Add(this.label8);
            this.tabPage_dp2library.Controls.Add(this.textBox_dp2library_userName);
            this.tabPage_dp2library.Controls.Add(this.label5);
            this.tabPage_dp2library.Location = new System.Drawing.Point(4, 24);
            this.tabPage_dp2library.Name = "tabPage_dp2library";
            this.tabPage_dp2library.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage_dp2library.Size = new System.Drawing.Size(465, 326);
            this.tabPage_dp2library.TabIndex = 1;
            this.tabPage_dp2library.Text = "dp2library";
            this.tabPage_dp2library.UseVisualStyleBackColor = true;
            // 
            // button_dp2library_findServerName
            // 
            this.button_dp2library_findServerName.Location = new System.Drawing.Point(369, 14);
            this.button_dp2library_findServerName.Name = "button_dp2library_findServerName";
            this.button_dp2library_findServerName.Size = new System.Drawing.Size(46, 28);
            this.button_dp2library_findServerName.TabIndex = 17;
            this.button_dp2library_findServerName.Text = "...";
            this.button_dp2library_findServerName.UseVisualStyleBackColor = true;
            this.button_dp2library_findServerName.Click += new System.EventHandler(this.button_dp2library_findServerName_Click);
            // 
            // textBox_dp2library_serverName
            // 
            this.textBox_dp2library_serverName.Location = new System.Drawing.Point(151, 14);
            this.textBox_dp2library_serverName.Name = "textBox_dp2library_serverName";
            this.textBox_dp2library_serverName.Size = new System.Drawing.Size(212, 25);
            this.textBox_dp2library_serverName.TabIndex = 16;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 17);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(99, 15);
            this.label1.TabIndex = 15;
            this.label1.Text = "服务器名(&S):";
            // 
            // checkBox_dp2library_force
            // 
            this.checkBox_dp2library_force.AutoSize = true;
            this.checkBox_dp2library_force.Location = new System.Drawing.Point(10, 238);
            this.checkBox_dp2library_force.Name = "checkBox_dp2library_force";
            this.checkBox_dp2library_force.Size = new System.Drawing.Size(110, 19);
            this.checkBox_dp2library_force.TabIndex = 10;
            this.checkBox_dp2library_force.Text = "强制修改(&F)";
            this.checkBox_dp2library_force.UseVisualStyleBackColor = true;
            this.checkBox_dp2library_force.CheckedChanged += new System.EventHandler(this.checkBox_dp2library_force_CheckedChanged);
            // 
            // button_dp2library_changePassword
            // 
            this.button_dp2library_changePassword.Location = new System.Drawing.Point(331, 292);
            this.button_dp2library_changePassword.Name = "button_dp2library_changePassword";
            this.button_dp2library_changePassword.Size = new System.Drawing.Size(128, 28);
            this.button_dp2library_changePassword.TabIndex = 14;
            this.button_dp2library_changePassword.Text = "修改密码(&C)";
            this.button_dp2library_changePassword.UseVisualStyleBackColor = true;
            this.button_dp2library_changePassword.Click += new System.EventHandler(this.button_dp2library_changePassword_Click);
            // 
            // textBox_dp2library_confirmNewPassword
            // 
            this.textBox_dp2library_confirmNewPassword.Location = new System.Drawing.Point(151, 181);
            this.textBox_dp2library_confirmNewPassword.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_dp2library_confirmNewPassword.Name = "textBox_dp2library_confirmNewPassword";
            this.textBox_dp2library_confirmNewPassword.Size = new System.Drawing.Size(212, 25);
            this.textBox_dp2library_confirmNewPassword.TabIndex = 13;
            this.textBox_dp2library_confirmNewPassword.UseSystemPasswordChar = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 102);
            this.label6.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(84, 15);
            this.label6.TabIndex = 8;
            this.label6.Text = "旧密码(&O):";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 184);
            this.label7.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(114, 15);
            this.label7.TabIndex = 12;
            this.label7.Text = "确认新密码(&C):";
            // 
            // textBox_dp2library_oldPassword
            // 
            this.textBox_dp2library_oldPassword.Location = new System.Drawing.Point(151, 99);
            this.textBox_dp2library_oldPassword.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_dp2library_oldPassword.Name = "textBox_dp2library_oldPassword";
            this.textBox_dp2library_oldPassword.Size = new System.Drawing.Size(212, 25);
            this.textBox_dp2library_oldPassword.TabIndex = 9;
            this.textBox_dp2library_oldPassword.UseSystemPasswordChar = true;
            // 
            // textBox_dp2library_newPassword
            // 
            this.textBox_dp2library_newPassword.Location = new System.Drawing.Point(151, 148);
            this.textBox_dp2library_newPassword.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_dp2library_newPassword.Name = "textBox_dp2library_newPassword";
            this.textBox_dp2library_newPassword.Size = new System.Drawing.Size(212, 25);
            this.textBox_dp2library_newPassword.TabIndex = 11;
            this.textBox_dp2library_newPassword.UseSystemPasswordChar = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(7, 151);
            this.label8.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(84, 15);
            this.label8.TabIndex = 10;
            this.label8.Text = "新密码(&N):";
            // 
            // textBox_dp2library_userName
            // 
            this.textBox_dp2library_userName.Location = new System.Drawing.Point(151, 57);
            this.textBox_dp2library_userName.Name = "textBox_dp2library_userName";
            this.textBox_dp2library_userName.Size = new System.Drawing.Size(212, 25);
            this.textBox_dp2library_userName.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 60);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(84, 15);
            this.label5.TabIndex = 0;
            this.label5.Text = "用户名(&U):";
            // 
            // ChangePasswordForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(497, 378);
            this.Controls.Add(this.tabControl_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ChangePasswordForm";
            this.ShowInTaskbar = false;
            this.Text = "修改密码";
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ChangePasswordForm_FormClosed);
            this.Activated += new System.EventHandler(this.ChangePasswordForm_Activated);
            this.Load += new System.EventHandler(this.ChangePasswordForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_dp2library.ResumeLayout(false);
            this.tabPage_dp2library.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_dp2library;
        private System.Windows.Forms.CheckBox checkBox_dp2library_force;
        private System.Windows.Forms.Button button_dp2library_changePassword;
        private System.Windows.Forms.TextBox textBox_dp2library_confirmNewPassword;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox_dp2library_oldPassword;
        private System.Windows.Forms.TextBox textBox_dp2library_newPassword;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBox_dp2library_userName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_dp2library_serverName;
        private System.Windows.Forms.Button button_dp2library_findServerName;
    }
}