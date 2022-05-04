using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.Text;

namespace sipApiTester
{
    public partial class SettingDialog : Form
    {
        public SettingDialog()
        {
            InitializeComponent();
        }

        private void SettingDialog_Load(object sender, EventArgs e)
        {
            this.textBox_sip_serverAddr.Text = DataModel.sipServerAddr;
            this.textBox_sip_serverPort.Text = DataModel.sipServerPort;
            this.textBox_sip_userName.Text = DataModel.sipUserName;
            this.textBox_sip_password.Text = DataModel.sipPassword;
        }

        private void SettingDialog_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void SettingDialog_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            bool control = (Control.ModifierKeys & Keys.Control) == Keys.Control;

            List<string> errors = new List<string>();

            if (errors.Count > 0)
            {
                strError = StringUtil.MakePathList(errors, "\r\n");
                goto ERROR1;
            }

            DataModel.sipServerAddr = this.textBox_sip_serverAddr.Text;
            DataModel.sipServerPort = this.textBox_sip_serverPort.Text;
            DataModel.sipUserName = this.textBox_sip_userName.Text;

            DataModel.sipPassword = this.textBox_sip_password.Text;

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
