namespace DigitalPlatform
{
    partial class ConfirmSupervisorDialog
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfirmSupervisorDialog));
            this.textBox_comment = new System.Windows.Forms.TextBox();
            this.textBox_password = new System.Windows.Forms.TextBox();
            this.textBox_userName = new System.Windows.Forms.TextBox();
            this.textBox_serverAddr = new System.Windows.Forms.TextBox();
            this.button_cancel = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_tempCode = new System.Windows.Forms.TextBox();
            this.label_tempCode = new System.Windows.Forms.Label();
            this.textBox_phoneNumber = new System.Windows.Forms.TextBox();
            this.label_phoneNumber = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // textBox_comment
            // 
            this.textBox_comment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_comment.BackColor = System.Drawing.SystemColors.Info;
            this.textBox_comment.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox_comment.ForeColor = System.Drawing.SystemColors.InfoText;
            this.textBox_comment.Location = new System.Drawing.Point(10, 9);
            this.textBox_comment.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_comment.Multiline = true;
            this.textBox_comment.Name = "textBox_comment";
            this.textBox_comment.ReadOnly = true;
            this.textBox_comment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_comment.Size = new System.Drawing.Size(374, 105);
            this.textBox_comment.TabIndex = 0;
            // 
            // textBox_password
            // 
            this.textBox_password.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_password.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_password.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_password.Location = new System.Drawing.Point(121, 208);
            this.textBox_password.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_password.Name = "textBox_password";
            this.textBox_password.PasswordChar = '*';
            this.textBox_password.Size = new System.Drawing.Size(156, 21);
            this.textBox_password.TabIndex = 6;
            // 
            // textBox_userName
            // 
            this.textBox_userName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_userName.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_userName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_userName.Location = new System.Drawing.Point(121, 183);
            this.textBox_userName.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_userName.Name = "textBox_userName";
            this.textBox_userName.Size = new System.Drawing.Size(156, 21);
            this.textBox_userName.TabIndex = 4;
            // 
            // textBox_serverAddr
            // 
            this.textBox_serverAddr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_serverAddr.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_serverAddr.Location = new System.Drawing.Point(10, 142);
            this.textBox_serverAddr.Margin = new System.Windows.Forms.Padding(2);
            this.textBox_serverAddr.Name = "textBox_serverAddr";
            this.textBox_serverAddr.ReadOnly = true;
            this.textBox_serverAddr.Size = new System.Drawing.Size(375, 21);
            this.textBox_serverAddr.TabIndex = 2;
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_cancel.Location = new System.Drawing.Point(306, 279);
            this.button_cancel.Margin = new System.Windows.Forms.Padding(2);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(78, 22);
            this.button_cancel.TabIndex = 8;
            this.button_cancel.Text = "取消";
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(8, 209);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "密码(&P)：";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 184);
            this.label2.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "用户名(&U)：";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(8, 128);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "服务器地址(&H):";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button_OK.Location = new System.Drawing.Point(306, 252);
            this.button_OK.Margin = new System.Windows.Forms.Padding(2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(78, 22);
            this.button_OK.TabIndex = 7;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_tempCode
            // 
            this.textBox_tempCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_tempCode.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBox_tempCode.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_tempCode.ForeColor = System.Drawing.SystemColors.ControlText;
            this.textBox_tempCode.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_tempCode.Location = new System.Drawing.Point(121, 273);
            this.textBox_tempCode.Name = "textBox_tempCode";
            this.textBox_tempCode.Size = new System.Drawing.Size(156, 21);
            this.textBox_tempCode.TabIndex = 20;
            this.textBox_tempCode.Visible = false;
            // 
            // label_tempCode
            // 
            this.label_tempCode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_tempCode.AutoSize = true;
            this.label_tempCode.Location = new System.Drawing.Point(8, 275);
            this.label_tempCode.Name = "label_tempCode";
            this.label_tempCode.Size = new System.Drawing.Size(65, 12);
            this.label_tempCode.TabIndex = 19;
            this.label_tempCode.Text = "验证码(&S):";
            this.label_tempCode.Visible = false;
            // 
            // textBox_phoneNumber
            // 
            this.textBox_phoneNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_phoneNumber.BackColor = System.Drawing.SystemColors.ControlLight;
            this.textBox_phoneNumber.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.textBox_phoneNumber.ForeColor = System.Drawing.SystemColors.ControlText;
            this.textBox_phoneNumber.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_phoneNumber.Location = new System.Drawing.Point(121, 250);
            this.textBox_phoneNumber.Name = "textBox_phoneNumber";
            this.textBox_phoneNumber.Size = new System.Drawing.Size(156, 21);
            this.textBox_phoneNumber.TabIndex = 18;
            // 
            // label_phoneNumber
            // 
            this.label_phoneNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label_phoneNumber.AutoSize = true;
            this.label_phoneNumber.Location = new System.Drawing.Point(8, 252);
            this.label_phoneNumber.Name = "label_phoneNumber";
            this.label_phoneNumber.Size = new System.Drawing.Size(65, 12);
            this.label_phoneNumber.TabIndex = 17;
            this.label_phoneNumber.Text = "手机号(&P):";
            // 
            // ConfirmSupervisorDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_cancel;
            this.ClientSize = new System.Drawing.Size(392, 311);
            this.Controls.Add(this.textBox_tempCode);
            this.Controls.Add(this.label_tempCode);
            this.Controls.Add(this.textBox_phoneNumber);
            this.Controls.Add(this.label_phoneNumber);
            this.Controls.Add(this.textBox_comment);
            this.Controls.Add(this.textBox_password);
            this.Controls.Add(this.textBox_userName);
            this.Controls.Add(this.textBox_serverAddr);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "ConfirmSupervisorDialog";
            this.ShowInTaskbar = false;
            this.Text = "确认身份";
            this.Load += new System.EventHandler(this.ConfirmSupervisorDialog_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        public System.Windows.Forms.TextBox textBox_comment;
        public System.Windows.Forms.TextBox textBox_password;
        public System.Windows.Forms.TextBox textBox_userName;
        private System.Windows.Forms.TextBox textBox_serverAddr;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_OK;
        public System.Windows.Forms.TextBox textBox_tempCode;
        private System.Windows.Forms.Label label_tempCode;
        public System.Windows.Forms.TextBox textBox_phoneNumber;
        private System.Windows.Forms.Label label_phoneNumber;
    }
}