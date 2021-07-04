
namespace DigitalPlatform.CirculationClient
{
    partial class ChangePasswordDialog
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
            this.button_worker_changePassword = new System.Windows.Forms.Button();
            this.textBox_worker_confirmNewPassword = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_worker_oldPassword = new System.Windows.Forms.TextBox();
            this.textBox_worker_newPassword = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textBox_worker_userName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button_worker_changePassword
            // 
            this.button_worker_changePassword.Location = new System.Drawing.Point(193, 221);
            this.button_worker_changePassword.Margin = new System.Windows.Forms.Padding(4);
            this.button_worker_changePassword.Name = "button_worker_changePassword";
            this.button_worker_changePassword.Size = new System.Drawing.Size(176, 38);
            this.button_worker_changePassword.TabIndex = 30;
            this.button_worker_changePassword.Text = "修改密码(&C)";
            this.button_worker_changePassword.UseVisualStyleBackColor = true;
            this.button_worker_changePassword.Click += new System.EventHandler(this.button_worker_changePassword_Click);
            // 
            // textBox_worker_confirmNewPassword
            // 
            this.textBox_worker_confirmNewPassword.Location = new System.Drawing.Point(193, 181);
            this.textBox_worker_confirmNewPassword.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_worker_confirmNewPassword.Name = "textBox_worker_confirmNewPassword";
            this.textBox_worker_confirmNewPassword.Size = new System.Drawing.Size(290, 31);
            this.textBox_worker_confirmNewPassword.TabIndex = 28;
            this.textBox_worker_confirmNewPassword.UseSystemPasswordChar = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(13, 83);
            this.label6.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(117, 21);
            this.label6.TabIndex = 23;
            this.label6.Text = "旧密码(&O):";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(13, 186);
            this.label7.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(159, 21);
            this.label7.TabIndex = 27;
            this.label7.Text = "确认新密码(&C):";
            // 
            // textBox_worker_oldPassword
            // 
            this.textBox_worker_oldPassword.Location = new System.Drawing.Point(193, 78);
            this.textBox_worker_oldPassword.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_worker_oldPassword.Name = "textBox_worker_oldPassword";
            this.textBox_worker_oldPassword.Size = new System.Drawing.Size(290, 31);
            this.textBox_worker_oldPassword.TabIndex = 24;
            this.textBox_worker_oldPassword.UseSystemPasswordChar = true;
            // 
            // textBox_worker_newPassword
            // 
            this.textBox_worker_newPassword.Location = new System.Drawing.Point(193, 134);
            this.textBox_worker_newPassword.Margin = new System.Windows.Forms.Padding(6, 5, 6, 5);
            this.textBox_worker_newPassword.Name = "textBox_worker_newPassword";
            this.textBox_worker_newPassword.Size = new System.Drawing.Size(290, 31);
            this.textBox_worker_newPassword.TabIndex = 26;
            this.textBox_worker_newPassword.UseSystemPasswordChar = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(13, 139);
            this.label8.Margin = new System.Windows.Forms.Padding(6, 0, 6, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(117, 21);
            this.label8.TabIndex = 25;
            this.label8.Text = "新密码(&N):";
            // 
            // textBox_worker_userName
            // 
            this.textBox_worker_userName.Location = new System.Drawing.Point(193, 20);
            this.textBox_worker_userName.Margin = new System.Windows.Forms.Padding(4);
            this.textBox_worker_userName.Name = "textBox_worker_userName";
            this.textBox_worker_userName.Size = new System.Drawing.Size(290, 31);
            this.textBox_worker_userName.TabIndex = 22;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(13, 25);
            this.label5.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(117, 21);
            this.label5.TabIndex = 21;
            this.label5.Text = "用户名(&U):";
            // 
            // ChangePasswordDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(514, 292);
            this.Controls.Add(this.button_worker_changePassword);
            this.Controls.Add(this.textBox_worker_confirmNewPassword);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.textBox_worker_oldPassword);
            this.Controls.Add(this.textBox_worker_newPassword);
            this.Controls.Add(this.label8);
            this.Controls.Add(this.textBox_worker_userName);
            this.Controls.Add(this.label5);
            this.Name = "ChangePasswordDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "修改密码";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button button_worker_changePassword;
        private System.Windows.Forms.TextBox textBox_worker_confirmNewPassword;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox_worker_oldPassword;
        private System.Windows.Forms.TextBox textBox_worker_newPassword;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox textBox_worker_userName;
        private System.Windows.Forms.Label label5;
    }
}