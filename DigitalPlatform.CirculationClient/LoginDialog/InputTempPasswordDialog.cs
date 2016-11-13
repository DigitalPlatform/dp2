using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.CirculationClient
{
    /// <summary>
    /// 输入临时密码的对话框。手机短信验证码
    /// </summary>
    public partial class InputTempPasswordDialog : Form
    {
        public InputTempPasswordDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            if (string.IsNullOrEmpty(this.textBox_password.Text))
            {
                strError = "请输入验证码";
                goto ERROR1;
            }

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }

        public string TempPassword
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

        private void checkBox_maskChar_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox_maskChar.Checked)
                this.textBox_password.PasswordChar = '*';
            else
                this.textBox_password.PasswordChar = (char)0;
        }
    }
}
