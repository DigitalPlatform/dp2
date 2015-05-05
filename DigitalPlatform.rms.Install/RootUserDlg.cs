using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.rms
{
    public partial class RootUserDlg : Form
    {
        public RootUserDlg()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.UserName == "")
            {
                MessageBox.Show(this, "尚未指定用户名。");
                return;
            }

            if (this.textBox_rootPassword.Text != this.textBox_confirmRootPassword.Text)
            {
                MessageBox.Show(this, "密码 和 再次输入密码不一致。请重新输入。");
                return;
            }

            if (this.textBox_rootPassword.Text == "")
            {
                DialogResult result = MessageBox.Show(this,
                    "root账户密码为空。这样会很不安全。您也可以在安装成功后，尽快利用dp2manager工具为root账户加上密码。\r\n\r\n确实要保持root账户的密码为空吗?",
                    "setup_dp2Kernel",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        public string UserName
        {
            get
            {
                return this.textBox_rootUserName.Text;
            }
            set
            {
                this.textBox_rootUserName.Text = value;
            }
        }

        public string Password
        {
            get
            {
                return this.textBox_rootPassword.Text;
            }
            set
            {
                this.textBox_rootPassword.Text = value;
                this.textBox_confirmRootPassword.Text = value;
            }
        }

        public string Rights
        {
            get
            {
                return this.textBox_rights.Text;
            }
            set
            {
                this.textBox_rights.Text = value;
            }
        }

    }
}