namespace DigitalPlatform.LibraryServer
{
    partial class CreateSupervisorDlg
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateSupervisorDlg));
            this.textBox_rights = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button_cancel = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.button_createSupervisor = new System.Windows.Forms.Button();
            this.textBox_confirmSupervisorPassword = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.textBox_supervisorPassword = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_supervisorUserName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBox_rights
            // 
            this.textBox_rights.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_rights.Location = new System.Drawing.Point(104, 166);
            this.textBox_rights.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_rights.Multiline = true;
            this.textBox_rights.Name = "textBox_rights";
            this.textBox_rights.ReadOnly = true;
            this.textBox_rights.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBox_rights.Size = new System.Drawing.Size(212, 77);
            this.textBox_rights.TabIndex = 32;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(11, 169);
            this.label5.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(53, 12);
            this.label5.TabIndex = 31;
            this.label5.Text = "权限(&R):";
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(259, 302);
            this.button_cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(56, 23);
            this.button_cancel.TabIndex = 30;
            this.button_cancel.Text = "取消";
            this.button_cancel.UseVisualStyleBackColor = true;
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.Location = new System.Drawing.Point(11, 10);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(304, 58);
            this.label1.TabIndex = 29;
            this.label1.Text = "需要创建一个dp2Library层的超级用户，您将用它对dp2Library进行各种管理操作。";
            // 
            // button_createSupervisor
            // 
            this.button_createSupervisor.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_createSupervisor.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.button_createSupervisor.Location = new System.Drawing.Point(177, 302);
            this.button_createSupervisor.Margin = new System.Windows.Forms.Padding(2);
            this.button_createSupervisor.Name = "button_createSupervisor";
            this.button_createSupervisor.Size = new System.Drawing.Size(77, 23);
            this.button_createSupervisor.TabIndex = 28;
            this.button_createSupervisor.Text = "确定";
            this.button_createSupervisor.UseVisualStyleBackColor = true;
            this.button_createSupervisor.Click += new System.EventHandler(this.button_createSupervisor_Click);
            // 
            // textBox_confirmSupervisorPassword
            // 
            this.textBox_confirmSupervisorPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_confirmSupervisorPassword.Location = new System.Drawing.Point(104, 132);
            this.textBox_confirmSupervisorPassword.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_confirmSupervisorPassword.Name = "textBox_confirmSupervisorPassword";
            this.textBox_confirmSupervisorPassword.PasswordChar = '*';
            this.textBox_confirmSupervisorPassword.Size = new System.Drawing.Size(163, 21);
            this.textBox_confirmSupervisorPassword.TabIndex = 26;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 134);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(83, 12);
            this.label3.TabIndex = 25;
            this.label3.Text = "再次输入密码:";
            // 
            // textBox_supervisorPassword
            // 
            this.textBox_supervisorPassword.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_supervisorPassword.Location = new System.Drawing.Point(104, 107);
            this.textBox_supervisorPassword.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_supervisorPassword.Name = "textBox_supervisorPassword";
            this.textBox_supervisorPassword.PasswordChar = '*';
            this.textBox_supervisorPassword.Size = new System.Drawing.Size(163, 21);
            this.textBox_supervisorPassword.TabIndex = 24;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(9, 110);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(53, 12);
            this.label2.TabIndex = 23;
            this.label2.Text = "密码(&P):";
            // 
            // textBox_supervisorUserName
            // 
            this.textBox_supervisorUserName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_supervisorUserName.Location = new System.Drawing.Point(104, 70);
            this.textBox_supervisorUserName.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_supervisorUserName.Name = "textBox_supervisorUserName";
            this.textBox_supervisorUserName.Size = new System.Drawing.Size(163, 21);
            this.textBox_supervisorUserName.TabIndex = 22;
            // 
            // label4
            // 
            this.label4.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 73);
            this.label4.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(65, 12);
            this.label4.TabIndex = 21;
            this.label4.Text = "用户名(&U):";
            // 
            // CreateSupervisorDlg
            // 
            this.AcceptButton = this.button_createSupervisor;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_cancel;
            this.ClientSize = new System.Drawing.Size(324, 335);
            this.Controls.Add(this.textBox_rights);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_createSupervisor);
            this.Controls.Add(this.textBox_confirmSupervisorPassword);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.textBox_supervisorPassword);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.textBox_supervisorUserName);
            this.Controls.Add(this.label4);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "CreateSupervisorDlg";
            this.Text = "创建超级用户";
            this.Load += new System.EventHandler(this.CreateSupervisorDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox_rights;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_createSupervisor;
        private System.Windows.Forms.TextBox textBox_confirmSupervisorPassword;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_supervisorPassword;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_supervisorUserName;
        private System.Windows.Forms.Label label4;
    }
}