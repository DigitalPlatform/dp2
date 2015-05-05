using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace DigitalPlatform.CirculationClient
{
    public partial class ServerDlg : Form
    {
        public ServerDlg()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
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

        private void button_cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();

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

        public string　ServerName
        {
            get
            {
                return this.textBox_serverName.Text;
            }
            set
            {
                this.textBox_serverName.Text = value;
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

        public static string HnbUrl = "http://123.103.13.236/dp2library";   // "http://hnbclub.cn/dp2library";

        private void toolStripButton_server_setHongnibaServer_Click(object sender, EventArgs e)
        {
            if (this.textBox_serverAddr.Text != HnbUrl)
            {
                this.textBox_serverName.Text = "红泥巴.数字平台服务器";
                this.textBox_serverAddr.Text = HnbUrl;

                this.textBox_userName.Text = "";
                this.textBox_password.Text = "";
            }
        }

        private void toolStripButton_server_setXeServer_Click(object sender, EventArgs e)
        {
            if (this.textBox_serverAddr.Text != "net.pipe://localhost/dp2library/xe")
            {
                this.textBox_serverName.Text = "单机版服务器";
                this.textBox_serverAddr.Text = "net.pipe://localhost/dp2library/xe";

                this.textBox_userName.Text = "supervisor";
                this.textBox_password.Text = "";
            }
        }
    }
}