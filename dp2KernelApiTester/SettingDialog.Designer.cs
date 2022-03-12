
namespace dp2KernelApiTester
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
            this.tabPage_dp2kernel = new System.Windows.Forms.TabPage();
            this.textBox_dp2kernel_password = new System.Windows.Forms.TextBox();
            this.textBox_dp2kernel_userName = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBox_dp2kernel_serverUrl = new System.Windows.Forms.ComboBox();
            this.tabControl1.SuspendLayout();
            this.tabPage_dp2kernel.SuspendLayout();
            this.SuspendLayout();
            // 
            // button_Cancel
            // 
            this.button_Cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_Cancel.Location = new System.Drawing.Point(1042, 842);
            this.button_Cancel.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.button_Cancel.Name = "button_Cancel";
            this.button_Cancel.Size = new System.Drawing.Size(182, 69);
            this.button_Cancel.TabIndex = 2;
            this.button_Cancel.Text = "取消";
            this.button_Cancel.UseVisualStyleBackColor = true;
            this.button_Cancel.Click += new System.EventHandler(this.button_Cancel_Click);
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(851, 842);
            this.button_OK.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(182, 69);
            this.button_OK.TabIndex = 1;
            this.button_OK.Text = "确定";
            this.button_OK.UseVisualStyleBackColor = true;
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControl1.Controls.Add(this.tabPage_dp2kernel);
            this.tabControl1.Location = new System.Drawing.Point(20, 19);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(5, 3, 5, 3);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(1204, 802);
            this.tabControl1.TabIndex = 0;
            // 
            // tabPage_dp2kernel
            // 
            this.tabPage_dp2kernel.AutoScroll = true;
            this.tabPage_dp2kernel.Controls.Add(this.comboBox_dp2kernel_serverUrl);
            this.tabPage_dp2kernel.Controls.Add(this.textBox_dp2kernel_password);
            this.tabPage_dp2kernel.Controls.Add(this.textBox_dp2kernel_userName);
            this.tabPage_dp2kernel.Controls.Add(this.label3);
            this.tabPage_dp2kernel.Controls.Add(this.label2);
            this.tabPage_dp2kernel.Controls.Add(this.label1);
            this.tabPage_dp2kernel.Location = new System.Drawing.Point(12, 67);
            this.tabPage_dp2kernel.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.tabPage_dp2kernel.Name = "tabPage_dp2kernel";
            this.tabPage_dp2kernel.Size = new System.Drawing.Size(1180, 723);
            this.tabPage_dp2kernel.TabIndex = 1;
            this.tabPage_dp2kernel.Text = "dp2kernel";
            this.tabPage_dp2kernel.UseVisualStyleBackColor = true;
            // 
            // textBox_dp2kernel_password
            // 
            this.textBox_dp2kernel_password.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_dp2kernel_password.Location = new System.Drawing.Point(313, 377);
            this.textBox_dp2kernel_password.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_dp2kernel_password.Name = "textBox_dp2kernel_password";
            this.textBox_dp2kernel_password.PasswordChar = '*';
            this.textBox_dp2kernel_password.Size = new System.Drawing.Size(433, 55);
            this.textBox_dp2kernel_password.TabIndex = 5;
            // 
            // textBox_dp2kernel_userName
            // 
            this.textBox_dp2kernel_userName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_dp2kernel_userName.Location = new System.Drawing.Point(313, 295);
            this.textBox_dp2kernel_userName.Margin = new System.Windows.Forms.Padding(8, 9, 8, 9);
            this.textBox_dp2kernel_userName.Name = "textBox_dp2kernel_userName";
            this.textBox_dp2kernel_userName.Size = new System.Drawing.Size(433, 55);
            this.textBox_dp2kernel_userName.TabIndex = 3;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(20, 382);
            this.label3.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(174, 46);
            this.label3.TabIndex = 4;
            this.label3.Text = "密码(&P)：";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(20, 300);
            this.label2.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(215, 46);
            this.label2.TabIndex = 2;
            this.label2.Text = "用户名(&U)：";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(18, 34);
            this.label1.Margin = new System.Windows.Forms.Padding(8, 0, 8, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(401, 46);
            this.label1.TabIndex = 0;
            this.label1.Text = "dp2kernel 服务器 URL:";
            // 
            // comboBox_dp2kernel_serverUrl
            // 
            this.comboBox_dp2kernel_serverUrl.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBox_dp2kernel_serverUrl.FormattingEnabled = true;
            this.comboBox_dp2kernel_serverUrl.Location = new System.Drawing.Point(26, 94);
            this.comboBox_dp2kernel_serverUrl.Name = "comboBox_dp2kernel_serverUrl";
            this.comboBox_dp2kernel_serverUrl.Size = new System.Drawing.Size(1126, 54);
            this.comboBox_dp2kernel_serverUrl.TabIndex = 1;
            // 
            // SettingDialog
            // 
            this.AcceptButton = this.button_OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(22F, 46F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.button_Cancel;
            this.ClientSize = new System.Drawing.Size(1243, 930);
            this.Controls.Add(this.button_Cancel);
            this.Controls.Add(this.button_OK);
            this.Controls.Add(this.tabControl1);
            this.Font = new System.Drawing.Font("微软雅黑", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.Margin = new System.Windows.Forms.Padding(5, 5, 5, 5);
            this.Name = "SettingDialog";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "设置";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.SettingDialog_FormClosing);
            this.FormClosed += new System.Windows.Forms.FormClosedEventHandler(this.SettingDialog_FormClosed);
            this.Load += new System.EventHandler(this.SettingDialog_Load);
            this.tabControl1.ResumeLayout(false);
            this.tabPage_dp2kernel.ResumeLayout(false);
            this.tabPage_dp2kernel.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button button_Cancel;
        private System.Windows.Forms.Button button_OK;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage_dp2kernel;
        public System.Windows.Forms.TextBox textBox_dp2kernel_password;
        public System.Windows.Forms.TextBox textBox_dp2kernel_userName;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox comboBox_dp2kernel_serverUrl;
    }
}