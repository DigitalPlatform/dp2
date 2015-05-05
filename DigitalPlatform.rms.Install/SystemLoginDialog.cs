using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.rms
{
    /// <summary>
    /// Oracle 数据库服务器管理账户登录对话框
    /// </summary>
    public partial class SystemLoginDialog : Form
    {
        public SystemLoginDialog()
        {
            InitializeComponent();
        }

        public string SqlServerName
        {
            get
            {
                return this.textBox_sqlServerName.Text;
            }
            set
            {
                this.textBox_sqlServerName.Text = value;
            }
        }

        public string SqlUserName
        {
            get
            {
                return this.textBox_sqlUserName.Text;
            }
            set
            {
                this.textBox_sqlUserName.Text = value;
            }
        }

        public string SqlPassword
        {
            get
            {
                return this.textBox_sqlPassword.Text;
            }
            set
            {
                this.textBox_sqlPassword.Text = value;
            }
        }

        public string Comment
        {
            get
            {
                return this.textBox_message.Text;
            }
            set
            {
                this.textBox_message.Text = value;
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.textBox_sqlServerName.Text == "")
            {
                MessageBox.Show(this, "尚未指定SQL服务器。");
                return;
            }

            if (this.textBox_sqlUserName.Text == "")
            {
                MessageBox.Show(this, "尚未指定SQL帐号。");
                return;
            }

#if NO
            // 检测SQL帐户是否正确
            EnableControls(false);
            string strError = "";
            int nRet = this.detect(this.textBox_sqlServerName.Text,
                this.textBox_sqlUserName.Text,
                this.textBox_sqlPassword.Text,
                radioButton_SSPI.Checked,
                out strError);
            EnableControls(true);
            if (nRet == -1)
            {
                strError = strError + "\r\n" + "请重新指定登录信息。";
                MessageBox.Show(this, strError);
                return;
            }
#endif

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        // 检查服务器名，用户名和密码是否正确
        private void button_detect_Click(object sender, EventArgs e)
        {
            string strError = "";
            // return:
            //      -1  出错
            //      0   密码不正确
            //      1   正确
            int nRet = OracleDataSourceWizard.VerifyUserNameAndPassword(
                this.textBox_sqlServerName.Text,
                this.textBox_sqlUserName.Text,
                this.textBox_sqlPassword.Text,
                out strError);
            if (nRet == 1)
                MessageBox.Show(this, "用户名和密码正确");
            else
                MessageBox.Show(this, strError);
        }

    }
}
