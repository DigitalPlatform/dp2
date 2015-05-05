namespace dp2Circulation
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
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_reader_barcode = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_reader_oldPassword = new System.Windows.Forms.TextBox();
            this.textBox_reader_newPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_reader_confirmNewPassword = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button_reader_changePassword = new System.Windows.Forms.Button();
            this.tabControl_main = new System.Windows.Forms.TabControl();
            this.tabPage_reader = new System.Windows.Forms.TabPage();
            this.textBox_reader_comment = new System.Windows.Forms.TextBox();
            this.tabPage_worker = new System.Windows.Forms.TabPage();
            this.textBox_worker_comment = new System.Windows.Forms.TextBox();
            this.checkBox_worker_force = new System.Windows.Forms.CheckBox();
            this.button_worker_changePassword = new System.Windows.Forms.Button();
            this.textBox_worker_confirmNewPassword = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.textBox_worker_oldPassword = new System.Windows.Forms.TextBox();
            this.textBox_worker_newPassword = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.textBox_worker_userName = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.tabControl_main.SuspendLayout();
            this.tabPage_reader.SuspendLayout();
            this.tabPage_worker.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(15, 84);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(101, 12);
            this.label1.TabIndex = 0;
            this.label1.Text = "读者证条码号(&B):";
            // 
            // textBox_reader_barcode
            // 
            this.textBox_reader_barcode.Location = new System.Drawing.Point(113, 81);
            this.textBox_reader_barcode.Name = "textBox_reader_barcode";
            this.textBox_reader_barcode.Size = new System.Drawing.Size(160, 21);
            this.textBox_reader_barcode.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(15, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 2;
            this.label2.Text = "旧密码(&O):";
            // 
            // textBox_reader_oldPassword
            // 
            this.textBox_reader_oldPassword.Location = new System.Drawing.Point(113, 113);
            this.textBox_reader_oldPassword.Name = "textBox_reader_oldPassword";
            this.textBox_reader_oldPassword.Size = new System.Drawing.Size(160, 21);
            this.textBox_reader_oldPassword.TabIndex = 3;
            this.textBox_reader_oldPassword.UseSystemPasswordChar = true;
            // 
            // textBox_reader_newPassword
            // 
            this.textBox_reader_newPassword.Location = new System.Drawing.Point(113, 145);
            this.textBox_reader_newPassword.Name = "textBox_reader_newPassword";
            this.textBox_reader_newPassword.Size = new System.Drawing.Size(160, 21);
            this.textBox_reader_newPassword.TabIndex = 5;
            this.textBox_reader_newPassword.UseSystemPasswordChar = true;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(15, 148);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(65, 12);
            this.label3.TabIndex = 4;
            this.label3.Text = "新密码(&N):";
            // 
            // textBox_reader_confirmNewPassword
            // 
            this.textBox_reader_confirmNewPassword.Location = new System.Drawing.Point(113, 172);
            this.textBox_reader_confirmNewPassword.Name = "textBox_reader_confirmNewPassword";
            this.textBox_reader_confirmNewPassword.Size = new System.Drawing.Size(160, 21);
            this.textBox_reader_confirmNewPassword.TabIndex = 7;
            this.textBox_reader_confirmNewPassword.UseSystemPasswordChar = true;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(15, 175);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(89, 12);
            this.label4.TabIndex = 6;
            this.label4.Text = "确认新密码(&C):";
            // 
            // button_reader_changePassword
            // 
            this.button_reader_changePassword.Location = new System.Drawing.Point(113, 199);
            this.button_reader_changePassword.Name = "button_reader_changePassword";
            this.button_reader_changePassword.Size = new System.Drawing.Size(96, 23);
            this.button_reader_changePassword.TabIndex = 8;
            this.button_reader_changePassword.Text = "修改密码(&C)";
            this.button_reader_changePassword.UseVisualStyleBackColor = true;
            this.button_reader_changePassword.Click += new System.EventHandler(this.button_reader_changePassword_Click);
            // 
            // tabControl_main
            // 
            this.tabControl_main.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl_main.Controls.Add(this.tabPage_reader);
            this.tabControl_main.Controls.Add(this.tabPage_worker);
            this.tabControl_main.Location = new System.Drawing.Point(0, 10);
            this.tabControl_main.Margin = new System.Windows.Forms.Padding(2);
            this.tabControl_main.Name = "tabControl_main";
            this.tabControl_main.SelectedIndex = 0;
            this.tabControl_main.Size = new System.Drawing.Size(373, 283);
            this.tabControl_main.TabIndex = 0;
            this.tabControl_main.SelectedIndexChanged += new System.EventHandler(this.tabControl_main_SelectedIndexChanged);
            // 
            // tabPage_reader
            // 
            this.tabPage_reader.AutoScroll = true;
            this.tabPage_reader.Controls.Add(this.textBox_reader_comment);
            this.tabPage_reader.Controls.Add(this.label1);
            this.tabPage_reader.Controls.Add(this.button_reader_changePassword);
            this.tabPage_reader.Controls.Add(this.textBox_reader_barcode);
            this.tabPage_reader.Controls.Add(this.textBox_reader_confirmNewPassword);
            this.tabPage_reader.Controls.Add(this.label2);
            this.tabPage_reader.Controls.Add(this.label4);
            this.tabPage_reader.Controls.Add(this.textBox_reader_oldPassword);
            this.tabPage_reader.Controls.Add(this.textBox_reader_newPassword);
            this.tabPage_reader.Controls.Add(this.label3);
            this.tabPage_reader.Location = new System.Drawing.Point(4, 22);
            this.tabPage_reader.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_reader.Name = "tabPage_reader";
            this.tabPage_reader.Padding = new System.Windows.Forms.Padding(12);
            this.tabPage_reader.Size = new System.Drawing.Size(365, 257);
            this.tabPage_reader.TabIndex = 0;
            this.tabPage_reader.Text = "读者";
            this.tabPage_reader.UseVisualStyleBackColor = true;
            // 
            // textBox_reader_comment
            // 
            this.textBox_reader_comment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_reader_comment.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_reader_comment.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_reader_comment.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_reader_comment.Location = new System.Drawing.Point(18, 15);
            this.textBox_reader_comment.Multiline = true;
            this.textBox_reader_comment.Name = "textBox_reader_comment";
            this.textBox_reader_comment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_reader_comment.Size = new System.Drawing.Size(332, 57);
            this.textBox_reader_comment.TabIndex = 9;
            this.textBox_reader_comment.Text = "这是工作人员为读者强制修改密码。\r\n\r\n使用本功能前，请务必仔细核实读者身份。";
            // 
            // tabPage_worker
            // 
            this.tabPage_worker.AutoScroll = true;
            this.tabPage_worker.Controls.Add(this.textBox_worker_comment);
            this.tabPage_worker.Controls.Add(this.checkBox_worker_force);
            this.tabPage_worker.Controls.Add(this.button_worker_changePassword);
            this.tabPage_worker.Controls.Add(this.textBox_worker_confirmNewPassword);
            this.tabPage_worker.Controls.Add(this.label6);
            this.tabPage_worker.Controls.Add(this.label7);
            this.tabPage_worker.Controls.Add(this.textBox_worker_oldPassword);
            this.tabPage_worker.Controls.Add(this.textBox_worker_newPassword);
            this.tabPage_worker.Controls.Add(this.label8);
            this.tabPage_worker.Controls.Add(this.textBox_worker_userName);
            this.tabPage_worker.Controls.Add(this.label5);
            this.tabPage_worker.Location = new System.Drawing.Point(4, 22);
            this.tabPage_worker.Margin = new System.Windows.Forms.Padding(2);
            this.tabPage_worker.Name = "tabPage_worker";
            this.tabPage_worker.Padding = new System.Windows.Forms.Padding(12);
            this.tabPage_worker.Size = new System.Drawing.Size(365, 257);
            this.tabPage_worker.TabIndex = 1;
            this.tabPage_worker.Text = "工作人员";
            this.tabPage_worker.UseVisualStyleBackColor = true;
            // 
            // textBox_worker_comment
            // 
            this.textBox_worker_comment.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_worker_comment.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_worker_comment.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_worker_comment.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_worker_comment.Location = new System.Drawing.Point(18, 14);
            this.textBox_worker_comment.Multiline = true;
            this.textBox_worker_comment.Name = "textBox_worker_comment";
            this.textBox_worker_comment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_worker_comment.Size = new System.Drawing.Size(332, 57);
            this.textBox_worker_comment.TabIndex = 10;
            this.textBox_worker_comment.Text = "这是工作人员为自己或者其他工作人员修改密码。\r\n";
            // 
            // checkBox_worker_force
            // 
            this.checkBox_worker_force.AutoSize = true;
            this.checkBox_worker_force.Location = new System.Drawing.Point(117, 194);
            this.checkBox_worker_force.Margin = new System.Windows.Forms.Padding(2);
            this.checkBox_worker_force.Name = "checkBox_worker_force";
            this.checkBox_worker_force.Size = new System.Drawing.Size(90, 16);
            this.checkBox_worker_force.TabIndex = 8;
            this.checkBox_worker_force.Text = "强制修改(&F)";
            this.checkBox_worker_force.UseVisualStyleBackColor = true;
            this.checkBox_worker_force.CheckedChanged += new System.EventHandler(this.checkBox_worker_force_CheckedChanged);
            // 
            // button_worker_changePassword
            // 
            this.button_worker_changePassword.Location = new System.Drawing.Point(117, 214);
            this.button_worker_changePassword.Margin = new System.Windows.Forms.Padding(2);
            this.button_worker_changePassword.Name = "button_worker_changePassword";
            this.button_worker_changePassword.Size = new System.Drawing.Size(96, 22);
            this.button_worker_changePassword.TabIndex = 9;
            this.button_worker_changePassword.Text = "修改密码(&C)";
            this.button_worker_changePassword.UseVisualStyleBackColor = true;
            this.button_worker_changePassword.Click += new System.EventHandler(this.button_worker_changePassword_Click);
            // 
            // textBox_worker_confirmNewPassword
            // 
            this.textBox_worker_confirmNewPassword.Location = new System.Drawing.Point(117, 168);
            this.textBox_worker_confirmNewPassword.Name = "textBox_worker_confirmNewPassword";
            this.textBox_worker_confirmNewPassword.Size = new System.Drawing.Size(160, 21);
            this.textBox_worker_confirmNewPassword.TabIndex = 7;
            this.textBox_worker_confirmNewPassword.UseSystemPasswordChar = true;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(19, 112);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(65, 12);
            this.label6.TabIndex = 2;
            this.label6.Text = "旧密码(&O):";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(19, 171);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(89, 12);
            this.label7.TabIndex = 6;
            this.label7.Text = "确认新密码(&C):";
            // 
            // textBox_worker_oldPassword
            // 
            this.textBox_worker_oldPassword.Location = new System.Drawing.Point(117, 109);
            this.textBox_worker_oldPassword.Name = "textBox_worker_oldPassword";
            this.textBox_worker_oldPassword.Size = new System.Drawing.Size(160, 21);
            this.textBox_worker_oldPassword.TabIndex = 3;
            this.textBox_worker_oldPassword.UseSystemPasswordChar = true;
            // 
            // textBox_worker_newPassword
            // 
            this.textBox_worker_newPassword.Location = new System.Drawing.Point(117, 141);
            this.textBox_worker_newPassword.Name = "textBox_worker_newPassword";
            this.textBox_worker_newPassword.Size = new System.Drawing.Size(160, 21);
            this.textBox_worker_newPassword.TabIndex = 5;
            this.textBox_worker_newPassword.UseSystemPasswordChar = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(19, 144);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(65, 12);
            this.label8.TabIndex = 4;
            this.label8.Text = "新密码(&N):";
            // 
            // textBox_worker_userName
            // 
            this.textBox_worker_userName.Location = new System.Drawing.Point(117, 76);
            this.textBox_worker_userName.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_worker_userName.Name = "textBox_worker_userName";
            this.textBox_worker_userName.Size = new System.Drawing.Size(160, 21);
            this.textBox_worker_userName.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(19, 79);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(65, 12);
            this.label5.TabIndex = 0;
            this.label5.Text = "用户名(&U):";
            // 
            // ChangePasswordForm
            // 
            this.AcceptButton = this.button_reader_changePassword;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(373, 302);
            this.Controls.Add(this.tabControl_main);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ChangePasswordForm";
            this.ShowInTaskbar = false;
            this.Text = "修改密码";
            this.Activated += new System.EventHandler(this.ChangePasswordForm_Activated);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.ChangePasswordForm_FormClosed);
            this.Load += new System.EventHandler(this.ChangePasswordForm_Load);
            this.tabControl_main.ResumeLayout(false);
            this.tabPage_reader.ResumeLayout(false);
            this.tabPage_reader.PerformLayout();
            this.tabPage_worker.ResumeLayout(false);
            this.tabPage_worker.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_reader_barcode;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_reader_oldPassword;
        private System.Windows.Forms.TextBox textBox_reader_newPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_reader_confirmNewPassword;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button button_reader_changePassword;
        private System.Windows.Forms.TabControl tabControl_main;
        private System.Windows.Forms.TabPage tabPage_reader;
        private System.Windows.Forms.TabPage tabPage_worker;
        private System.Windows.Forms.TextBox textBox_worker_userName;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox textBox_worker_confirmNewPassword;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox textBox_worker_oldPassword;
        private System.Windows.Forms.TextBox textBox_worker_newPassword;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.Button button_worker_changePassword;
        private System.Windows.Forms.CheckBox checkBox_worker_force;
        private System.Windows.Forms.TextBox textBox_reader_comment;
        private System.Windows.Forms.TextBox textBox_worker_comment;
    }
}