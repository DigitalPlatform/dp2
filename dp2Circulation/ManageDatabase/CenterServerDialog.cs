using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    /// <summary>
    /// 中心服务器对话框
    /// </summary>
    internal partial class CenterServerDialog : Form
    {
        public bool CreateMode = false;

        public ManagerForm ManagerForm = null;

        public CenterServerDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {

            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        public string ServerName
        {
            get
            {
                return this.textBox_name.Text;
            }
            set
            {
                this.textBox_name.Text = value;
            }
        }

        public string ServerUrl
        {
            get
            {
                return this.textBox_url.Text;
            }
            set
            {
                this.textBox_url.Text = value;
            }
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

        public string RefID
        {
            get
            {
                return this.textBox_refid.Text;
            }
            set
            {
                this.textBox_refid.Text = value;
            }
        }

        public bool ChangePassword
        {
            get
            {
                return this.checkBox_changePassword.Checked;
            }
            set
            {
                this.checkBox_changePassword.Checked = value;
            }
        }

        private void checkBox_changePassword_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_changePassword.Checked == true)
                this.textBox_password.ReadOnly = false;
            else
                this.textBox_password.ReadOnly = true;
        }

        private void button_verify_Click(object sender, EventArgs e)
        {
            string strError = "";

            if (string.IsNullOrEmpty(this.textBox_url.Text) == true)
            {
                strError = "尚未输入 URL";
                goto ERROR1;
            }

            if (string.IsNullOrEmpty(this.textBox_userName.Text) == true)
            {
                strError = "尚未输入 用户名";
                goto ERROR1;
            }

            List<string> dbnames = null;
            int nRet = ManagerForm.GetRemoteBiblioDbNames(
                this.textBox_url.Text,
                this.textBox_userName.Text,
                this.textBox_password.Text,
                out dbnames,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            MessageBox.Show(this, "URL 用户名 和密码 经验证正确");
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
    }
}
