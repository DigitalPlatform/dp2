
namespace sipApiTester
{
    partial class SettingDialog
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
            this.button_Cancel = new System.Windows.Forms.Button();
            this.button_OK = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage_sip = new System.Windows.Forms.TabPage();
            this.textBox_sip_password = new System.Windows.Forms.TextBox();
            this.textBox_sip_userName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.textBox_sip_serverAddr = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.textBox_sip_serverPort = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tabControl1.SuspendLayout();
            this.tabPage_sip.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(698, 646);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(111, 40);
            this.button_Cancel.TabIndex = 5;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(581, 646);
            this.button_OK.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(111, 40);
            this.button_OK.TabIndex = 4;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage_sip);
            this.tabControl1.Location = new System.Drawing.Point(12, 11);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(3, 2, 3, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(797, 631);
            this.tabControl1.TabIndex = 3;
            // 
            // tabPage_sip
            // 
            this.tabPage_sip.AutoScroll = true;
            this.tabPage_sip.Controls.Add(this.textBox_sip_serverPort);
            this.tabPage_sip.Controls.Add(this.label4);
            this.tabPage_sip.Controls.Add(this.textBox_sip_password);
            this.tabPage_sip.Controls.Add(this.textBox_sip_userName);
            this.tabPage_sip.Controls.Add(this.label3);
            this.tabPage_sip.Controls.Add(this.label2);
            this.tabPage_sip.Controls.Add(this.textBox_sip_serverAddr);
            this.tabPage_sip.Controls.Add(this.label1);
            this.tabPage_sip.Location = new System.Drawing.Point(4, 31);
            this.tabPage_sip.Name = "tabPage_sip";
            this.tabPage_sip.Size = new System.Drawing.Size(789, 596);
            this.tabPage_sip.TabIndex = 1;
            this.tabPage_sip.Text = "SIP";
            this.tabPage_sip.UseVisualStyleBackColor = true;
            // 
            // textBox_sip_password
            // 
            this.textBox_sip_password.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_sip_password.Location = new System.Drawing.Point(212, 220);
            this.textBox_sip_password.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_sip_password.Name = "textBox_sip_password";
            this.textBox_sip_password.PasswordChar = '*';
            this.textBox_sip_password.Size = new System.Drawing.Size(266, 31);
            this.textBox_sip_password.TabIndex = 16;
            // 
            // textBox_sip_userName
            // 
            this.textBox_sip_userName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_sip_userName.Location = new System.Drawing.Point(212, 172);
            this.textBox_sip_userName.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_sip_userName.Name = "textBox_sip_userName";
            this.textBox_sip_userName.Size = new System.Drawing.Size(266, 31);
            this.textBox_sip_userName.TabIndex = 14;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 223);
            this.label3.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(106, 21);
            this.label3.TabIndex = 15;
            this.label3.Text = "密码(&P)：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 175);
            this.label2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(127, 21);
            this.label2.TabIndex = 13;
            this.label2.Text = "用户名(&U)：";
            // 
            // textBox_sip_serverAddr
            // 
            this.textBox_sip_serverAddr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_sip_serverAddr.Location = new System.Drawing.Point(212, 17);
            this.textBox_sip_serverAddr.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_sip_serverAddr.Name = "textBox_sip_serverAddr";
            this.textBox_sip_serverAddr.Size = new System.Drawing.Size(585, 31);
            this.textBox_sip_serverAddr.TabIndex = 11;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(11, 20);
            this.label1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(170, 21);
            this.label1.TabIndex = 10;
            this.label1.Text = "SIP 服务器地址:";
            // 
            // textBox_sip_serverPort
            // 
            this.textBox_sip_serverPort.Location = new System.Drawing.Point(212, 58);
            this.textBox_sip_serverPort.Margin = new System.Windows.Forms.Padding(5);
            this.textBox_sip_serverPort.Name = "textBox_sip_serverPort";
            this.textBox_sip_serverPort.Size = new System.Drawing.Size(171, 31);
            this.textBox_sip_serverPort.TabIndex = 18;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(11, 61);
            this.label4.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(191, 21);
            this.label4.TabIndex = 17;
            this.label4.Text = "SIP 服务器端口号:";
            // 
            // SettingDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 21F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(821, 697);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Name = "SettingDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "设置";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SettingDialog_FormClosed);
            this.Load += new System.EventHandler(this.SettingDialog_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_sip.ResumeLayout(false);
            this.tabPage_sip.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_sip;
        public System.Windows.Forms.TextBox textBox_sip_password;
        public System.Windows.Forms.TextBox textBox_sip_userName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox textBox_sip_serverAddr;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox textBox_sip_serverPort;
        private System.Windows.Forms.Label label4;
    }
}