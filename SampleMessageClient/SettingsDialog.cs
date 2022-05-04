using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.Text;

namespace SampleMessageClient
{
    public partial class SettingsDialog : Form
    {
        public SettingsDialog()
        {
            InitializeComponent();
        }

        private void SettingsDialog_Load(object sender, EventArgs e)
        {
            this.textBox_messageServer_url.Text = DataModel.messageServerUrl;
            this.textBox_messageServer_userName.Text = DataModel.messageServerUserName;
            this.textBox_messageServer_password.Text = DataModel.messageServerPassword;
        }

        private void SettingsDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            bool control = (Control.ModifierKeys & Keys.Control) == Keys.Control;

            List<string> errors = new List<string>();

            if (control == false
                && string.IsNullOrEmpty(this.textBox_messageServer_url.Text))
                errors.Add("尚未配置消息服务器 URL");

            if (control == false
    && string.IsNullOrEmpty(this.textBox_messageServer_userName.Text))
                errors.Add("尚未配置消息服务器用户名");

            if (errors.Count > 0)
            {
                strError = StringUtil.MakePathList(errors, "\r\n");
                goto ERROR1;
            }

            DataModel.messageServerUrl = this.textBox_messageServer_url.Text;
            DataModel.messageServerUserName = this.textBox_messageServer_userName.Text;
            DataModel.messageServerPassword = this.textBox_messageServer_password.Text;

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
