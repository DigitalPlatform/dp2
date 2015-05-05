using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace dp2Library
{
    public partial class HostNameDialog : Form
    {
        public List<string> HostNames = null;

        public HostNameDialog()
        {
            InitializeComponent();
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            if (this.comboBox_hostName.Text == "")
            {
                MessageBox.Show(this, "尚未指定主机名");
                return;
            }

            // 检查是否为localhost或127.0.0.1?
            string strPureHostName = "";
            int nRet = this.comboBox_hostName.Text.IndexOf(":");
            if (nRet != -1)
                strPureHostName = this.comboBox_hostName.Text.Substring(0, nRet).Trim().ToLower();
            else
                strPureHostName = this.comboBox_hostName.Text.Trim().ToLower();

            if (strPureHostName == "localhost"
                || strPureHostName == "127.0.0.1")
            {
                DialogResult result = MessageBox.Show(this,
                    "警告：因您选择了主机名 '"+strPureHostName+"'，这样，如果位于别的机器的前端程序访问这台图书馆应用服务器的时候，这个主机名配置会造成一些故障：前端某些HTML显示界面会无法找到正确的css文件，等等。\r\n建议在这里配置“从其他机器的角度看过来的这台服务器的主机名”，例如这台机器的域名或(非loopback)IP地址。\r\n\r\n确实要坚持使用主机名 '" + strPureHostName + "' ?",
                    "HostNameDialog",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button2);
                if (result == DialogResult.No)
                    return;
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {

            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void HostNameDialog_Load(object sender, EventArgs e)
        {
            if (this.HostNames != null)
            {
                for (int i = 0; i < this.HostNames.Count; i++)
                {
                    this.comboBox_hostName.Items.Add(this.HostNames[i]);
                }
            }
        }

        public string HostName
        {
            get
            {
                return this.comboBox_hostName.Text;
            }
            set
            {
                this.comboBox_hostName.Text = value;
            }
        }
    }
}