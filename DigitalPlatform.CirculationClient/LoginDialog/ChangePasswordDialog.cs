using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.LibraryClient;

namespace DigitalPlatform.CirculationClient
{
    public partial class ChangePasswordDialog : Form
    {
        public string ServerUrl { get; set; }

        public ChangePasswordDialog()
        {
            InitializeComponent();
        }

        private void button_worker_changePassword_Click(object sender, EventArgs e)
        {
            bool succeed = false;
            this.button_worker_changePassword.Enabled = false;
            try
            {
                using (LibraryChannel channel = new LibraryChannel())
                {
                    channel.Timeout = TimeSpan.FromSeconds(10);
                    channel.Url = ServerUrl;
                    var ret = channel.ChangeUserPassword(null,
                        this.textBox_worker_userName.Text,
                        this.textBox_worker_oldPassword.Text,
                        this.textBox_worker_newPassword.Text,
                        out string strError);
                    if (ret == -1)
                        MessageBox.Show(this, strError);
                    else
                    {
                        MessageBox.Show(this, "密码修改成功");
                        succeed = true;
                    }
                    channel.Close();
                }
            }
            finally
            {
                this.button_worker_changePassword.Enabled = true;
            }

            if (succeed)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        public string UserName
        {
            get
            {
                return this.textBox_worker_userName.Text;
            }
            set
            {
                this.textBox_worker_userName.Text = value;
            }
        }

        public string OldPassword
        {
            get
            {
                return this.textBox_worker_oldPassword.Text;
            }
            set
            {
                this.textBox_worker_oldPassword.Text = value;
            }
        }

        public string NewPassword
        {
            get
            {
                return this.textBox_worker_newPassword.Text;
            }
            set
            {
                this.textBox_worker_newPassword.Text = value;
            }
        }

        public string ConfirmNewPassword
        {
            get
            {
                return this.textBox_worker_confirmNewPassword.Text;
            }
            set
            {
                this.textBox_worker_confirmNewPassword.Text = value;
            }
        }
    }
}
