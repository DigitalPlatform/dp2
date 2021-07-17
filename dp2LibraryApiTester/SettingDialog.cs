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

using DigitalPlatform.CirculationClient;
using DigitalPlatform.Text;

namespace dp2LibraryApiTester
{
    public partial class SettingDialog : Form
    {
        public SettingDialog()
        {
            InitializeComponent();
        }

        private void SettingDialog_Load(object sender, EventArgs e)
        {
            this.textBox_dp2library_serverUrl.Text = DataModel.dp2libraryServerUrl;
            this.textBox_dp2library_userName.Text = DataModel.dp2libraryUserName;
            this.textBox_dp2library_password.Text = DataModel.dp2libraryPassword;
            this.textBox_dp2library_location.Text = DataModel.dp2libraryLocation;
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

            DataModel.dp2libraryServerUrl = this.textBox_dp2library_serverUrl.Text;
            DataModel.dp2libraryUserName = this.textBox_dp2library_userName.Text;

            DataModel.dp2libraryPassword = this.textBox_dp2library_password.Text;
            DataModel.dp2libraryLocation = this.textBox_dp2library_location.Text;

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

        private void toolStripButton_cfg_setHongnibaServer_Click(object sender, EventArgs e)
        {
            if (this.textBox_dp2library_serverUrl.Text != ServerDlg.HnbUrl)
            {
                this.textBox_dp2library_serverUrl.Text = ServerDlg.HnbUrl;

                this.textBox_dp2library_userName.Text = "";
                this.textBox_dp2library_password.Text = "";
            }
        }

        private void toolStripButton_cfg_setXeServer_Click(object sender, EventArgs e)
        {
            if (this.textBox_dp2library_serverUrl.Text != "net.pipe://localhost/dp2library/xe")
            {
                this.textBox_dp2library_serverUrl.Text = "net.pipe://localhost/dp2library/xe";

                this.textBox_dp2library_userName.Text = "supervisor";
                this.textBox_dp2library_password.Text = "";
            }
        }
    }
}
