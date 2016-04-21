using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace dp2SIPServer
{
    public partial class SettingsForm : Form
    {
        public SettingsForm()
        {
            InitializeComponent();
        }

        public string Port
        {
            get
            {
                return this.textBox_port.Text;
            }
            set
            {
                this.textBox_port.Text = value;
            }
        }

        public string ServerUrl
        {
            get
            {
                return this.textBox_serverUrl.Text;
            }

            set
            {
                this.textBox_serverUrl.Text = value;
            }
        }

        public string Username
        {
            get
            {
                return this.textBox_username.Text;
            }

            set
            {
                this.textBox_username.Text = value;
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

        private void SettingsForm_Load(object sender, EventArgs e)
        {
            this.Port = Properties.Settings.Default.Port.ToString();

            this.ServerUrl = Properties.Settings.Default.LibraryServerUrl;

            this.Username = Properties.Settings.Default.Username;

            this.Password = Properties.Settings.Default.Password;

        }

        private void button_ok_Click(object sender, EventArgs e)
        {
            int v = 0;
            bool bRet = int.TryParse(this.Port, out v);
            if (!bRet)
            {
                MessageBox.Show(this, "'端口号'只能是纯数字");
                return;
            }

            Properties.Settings.Default.Port = v;

            Properties.Settings.Default.LibraryServerUrl = this.ServerUrl;
            Properties.Settings.Default.Username = this.Username;
            Properties.Settings.Default.Password = this.Password;
            Properties.Settings.Default.Save();

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}
