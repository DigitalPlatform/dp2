using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace DigitalPlatform
{
    /// <summary>
    /// Summary description for LoginDlg.
    /// </summary>
    public class LoginDlg : System.Windows.Forms.Form
    {
        public System.Windows.Forms.CheckBox checkBox_savePassword;
        public System.Windows.Forms.TextBox textBox_password;
        public System.Windows.Forms.TextBox textBox_userName;
        private System.Windows.Forms.Button button_cancel;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_OK;
        public System.Windows.Forms.TextBox textBox_serverAddr;
        public System.Windows.Forms.TextBox textBox_comment;
        private StatusStrip statusStrip1;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public LoginDlg()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LoginDlg));
            this.checkBox_savePassword = new System.Windows.Forms.CheckBox();
            this.textBox_password = new System.Windows.Forms.TextBox();
            this.textBox_userName = new System.Windows.Forms.TextBox();
            this.textBox_serverAddr = new System.Windows.Forms.TextBox();
            this.button_cancel = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.button_OK = new System.Windows.Forms.Button();
            this.textBox_comment = new System.Windows.Forms.TextBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.SuspendLayout();
            // 
            // checkBox_savePassword
            // 
            this.checkBox_savePassword.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.checkBox_savePassword.Location = new System.Drawing.Point(120, 209);
            this.checkBox_savePassword.Name = "checkBox_savePassword";
            this.checkBox_savePassword.Size = new System.Drawing.Size(156, 19);
            this.checkBox_savePassword.TabIndex = 7;
            this.checkBox_savePassword.Text = "保存密码(&S)";
            // 
            // textBox_password
            // 
            this.textBox_password.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_password.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_password.Location = new System.Drawing.Point(120, 182);
            this.textBox_password.Name = "textBox_password";
            this.textBox_password.PasswordChar = '*';
            this.textBox_password.Size = new System.Drawing.Size(156, 21);
            this.textBox_password.TabIndex = 6;
            this.textBox_password.Enter += new System.EventHandler(this.textBox_password_Enter);
            // 
            // textBox_userName
            // 
            this.textBox_userName.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.textBox_userName.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_userName.Location = new System.Drawing.Point(120, 155);
            this.textBox_userName.Name = "textBox_userName";
            this.textBox_userName.Size = new System.Drawing.Size(156, 21);
            this.textBox_userName.TabIndex = 4;
            this.textBox_userName.Enter += new System.EventHandler(this.textBox_userName_Enter);
            // 
            // textBox_serverAddr
            // 
            this.textBox_serverAddr.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_serverAddr.ImeMode = System.Windows.Forms.ImeMode.Off;
            this.textBox_serverAddr.Location = new System.Drawing.Point(12, 119);
            this.textBox_serverAddr.Name = "textBox_serverAddr";
            this.textBox_serverAddr.Size = new System.Drawing.Size(427, 21);
            this.textBox_serverAddr.TabIndex = 2;
            // 
            // button_cancel
            // 
            this.button_cancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button_cancel.Location = new System.Drawing.Point(361, 205);
            this.button_cancel.Name = "button_cancel";
            this.button_cancel.Size = new System.Drawing.Size(78, 23);
            this.button_cancel.TabIndex = 9;
            this.button_cancel.Text = "取消";
            this.button_cancel.Click += new System.EventHandler(this.button_cancel_Click);
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(10, 184);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(59, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "密码(&P)：";
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(10, 158);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(71, 12);
            this.label2.TabIndex = 3;
            this.label2.Text = "用户名(&U)：";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 104);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(89, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "服务器地址(&H):";
            // 
            // button_OK
            // 
            this.button_OK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button_OK.Location = new System.Drawing.Point(361, 176);
            this.button_OK.Name = "button_OK";
            this.button_OK.Size = new System.Drawing.Size(78, 22);
            this.button_OK.TabIndex = 8;
            this.button_OK.Text = "确定";
            this.button_OK.Click += new System.EventHandler(this.button_OK_Click);
            // 
            // textBox_comment
            // 
            this.textBox_comment.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_comment.Location = new System.Drawing.Point(12, 12);
            this.textBox_comment.Multiline = true;
            this.textBox_comment.Name = "textBox_comment";
            this.textBox_comment.ReadOnly = true;
            this.textBox_comment.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_comment.Size = new System.Drawing.Size(427, 74);
            this.textBox_comment.TabIndex = 0;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Location = new System.Drawing.Point(0, 234);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.Professional;
            this.statusStrip1.Size = new System.Drawing.Size(451, 22);
            this.statusStrip1.TabIndex = 10;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // LoginDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(451, 256);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.textBox_comment);
            this.Controls.Add(this.checkBox_savePassword);
            this.Controls.Add(this.textBox_password);
            this.Controls.Add(this.textBox_userName);
            this.Controls.Add(this.textBox_serverAddr);
            this.Controls.Add(this.button_cancel);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button_OK);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "LoginDlg";
            this.ShowInTaskbar = false;
            this.Text = "登录";
            this.Load += new System.EventHandler(this.LoginDlg_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion

        private void button_OK_Click(object sender, System.EventArgs e)
        {
            if (textBox_serverAddr.Text == ""
                && textBox_serverAddr.Enabled == true)
            {
                MessageBox.Show(this, "尚未输入服务器地址");
                return;
            }
            if (textBox_userName.Text == "")
            {
                MessageBox.Show(this, "尚未输入用户名");
                return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_cancel_Click(object sender, System.EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void LoginDlg_Load(object sender, System.EventArgs e)
        {
            if (textBox_comment.Text == "")
                textBox_comment.Visible = false;

            this.BeginInvoke(new Delegate_SetFocus(SetFocus));
        }

        public delegate void Delegate_SetFocus();

        void SetFocus()
        {
            if (this.textBox_userName.Text == "")
                this.textBox_userName.Focus();
            else
                this.textBox_password.Focus();
        }

        public string UserName
        {
            get
            {
                return this.textBox_userName.Text;
            }
            set
            {
                this.textBox_userName.Text = value;
            }
        }

        public string Password
        {
            get
            {
                return this.textBox_password.Text;
            }
            set
            {
                this.textBox_password.Text = value;
            }
        }

        public string ServerUrl
        {
            get
            {
                return this.textBox_serverAddr.Text;
            }
            set
            {
                this.textBox_serverAddr.Text = value;
            }
        }

        // 2021/7/22
        public bool ServerAddrEnabled
        {
            get
            {
                return this.textBox_serverAddr.Enabled;
            }
            set
            {
                this.textBox_serverAddr.Enabled = value;
            }
        }

        public bool SavePassword
        {
            get
            {
                return this.checkBox_savePassword.Checked;
            }
            set
            {
                this.checkBox_savePassword.Checked = value;
            }
        }

        // 2022/7/4
        public bool SavePasswordVisible
        {
            get
            {
                return this.checkBox_savePassword.Visible;
            }
            set
            {
                this.checkBox_savePassword.Visible = value;
            }
        }

        public string Comment
        {
            get
            {
                return this.textBox_comment.Text;
            }
            set
            {
                this.textBox_comment.Text = value;
            }
        }

        private void textBox_userName_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_OK;
        }

        private void textBox_password_Enter(object sender, EventArgs e)
        {
            this.AcceptButton = this.button_OK;
        }
    }
}
