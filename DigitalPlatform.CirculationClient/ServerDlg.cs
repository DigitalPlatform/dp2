using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using DigitalPlatform.LibraryClient;

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

        // 注：允许输入多行文本。即字符串中包含 \r\n
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

        public string ServerName
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

        public static bool IsSameUrl(string strUrl1, string strUrl2)
        {
            Uri uri1 = new Uri(strUrl1);
            Uri uri2 = new Uri(strUrl2);

            bool bRet = uri1.Equals(uri2);
            if (bRet == true)
                return true;

            // 进一步比较 host 是否实际一致
            if (IsSameHost(uri1.Host, uri2.Host) == true)
            {
                if (uri1.Scheme == uri2.Scheme
                    && uri1.Port == uri2.Port
                    && uri1.PathAndQuery == uri2.PathAndQuery)
                    return true;
            }

            return false;
        }

        public static bool IsSameHost(string strHost1, string strHost2)
        {
            if (strHost1 == strHost2)
                return true;

#if NO
            IPAddress[] address_list1 = Array.FindAll(
    Dns.GetHostEntry(strHost1).AddressList,
    a => a.AddressFamily == AddressFamily.InterNetwork);

            IPAddress[] address_list2 = Array.FindAll(
Dns.GetHostEntry(strHost2).AddressList,
a => a.AddressFamily == AddressFamily.InterNetwork);
#endif
            IPAddress[] address_list1 = null;
            IPAddress[] address_list2 = null;

            try
            {
                address_list1 = Dns.GetHostAddresses(strHost1);
            }
            catch (Exception ex)
            {
                throw new Exception("解析主机名 '" + strHost1 + "' 时出错: " + ex.Message + "\r\n请检查网络状态是否正常");
            }

            try
            {
                address_list2 = Dns.GetHostAddresses(strHost2);
            }
            catch (Exception ex)
            {
                throw new Exception("解析主机名 '" + strHost2 + "' 时出错: " + ex.Message + "\r\n检查网络状态是否正常");
            }

            foreach (IPAddress address1 in address_list1)
            {
                foreach (IPAddress address2 in address_list2)
                {
                    if (IPAddress.Equals(address1, address2) == true)
                        return true;
                }
            }

            return false;
        }

        public static string HnbUrl = "rest.http://58.87.101.80/hnb/rest";   // "http://hnbclub.cn/dp2library";

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

        private void toolStripButton_enableMultiLine_CheckedChanged(object sender, EventArgs e)
        {
            this.textBox_serverAddr.Multiline = this.toolStripButton_enableMultiLine.Checked;
        }

        private async void button_getLibraryName_Click(object sender, EventArgs e)
        {
            this.button_getLibraryName.Enabled = false;
            try
            {
                var result = await Task.Run<NormalResult>(() =>
                {
                    using (LibraryChannel channel = new LibraryChannel())
                    {
                        channel.Timeout = TimeSpan.FromSeconds(5);
                        channel.Url = this.textBox_serverAddr.Text;
                        long lRet = channel.GetSystemParameter(null, "library", "name",
                            out string libraryName,
                            out string strError);
                        if (lRet == -1)
                        {
                            if (channel.ErrorCode == LibraryClient.localhost.ErrorCode.NotLogin)
                                strError = "dp2library 服务器需要升级到 3.22 版以上，才支持不登录即可获得图书馆名";
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = strError
                            };
                        }
                        else
                            return new NormalResult { ErrorInfo = libraryName };
                    }
                });

                if (result.Value == -1)
                    MessageBox.Show(this, result.ErrorInfo);
                else
                    this.textBox_serverName.Text = result.ErrorInfo;
            }
            finally
            {
                this.button_getLibraryName.Enabled = true;
            }
        }

        public static async Task<NormalResult> GetServerUID(string url)
        {
            return await Task.Run<NormalResult>(() =>
            {
                using (LibraryChannel channel = new LibraryChannel())
                {
                    channel.Timeout = TimeSpan.FromSeconds(10);
                    channel.Url = url;
                    for (int i = 0;; i++)
                    {
                        long lRet = channel.GetVersion(null,
                            out string version,
                            out string uid,
                            out string strError);
                        if (lRet == -1)
                        {
                            if (channel.ErrorCode == LibraryClient.localhost.ErrorCode.RequestTimeOut)
                            {
                                if (i < 2)
                                    continue;
                            }
                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = strError
                            };
                        }
                        else
                            return new NormalResult { ErrorCode = uid };
                    }
                }
            });
        }

        public static async Task<NormalResult> GetServerName(string url)
        {
            return await Task.Run<NormalResult>(() =>
            {
                using (LibraryChannel channel = new LibraryChannel())
                {
                    channel.Timeout = TimeSpan.FromSeconds(10);
                    channel.Url = url;
                    for (int i = 0; ; i++)
                    {
                        long lRet = channel.GetSystemParameter(null,
    "library",
    "name",
    out string strLibraryName,
    out string strError);
                        if (lRet == -1)
                        {
                            if (channel.ErrorCode == LibraryClient.localhost.ErrorCode.RequestTimeOut)
                            {
                                if (i < 2)
                                    continue;
                            }
                            if (channel.ErrorCode == LibraryClient.localhost.ErrorCode.NotLogin)
                            {
                                strError = $"服务器 {channel.Url} 版本太旧，无法免登录获得图书馆名。请将它升级到最新版本";
                            }

                            return new NormalResult
                            {
                                Value = -1,
                                ErrorInfo = strError
                            };
                        }
                        else
                            return new NormalResult { ErrorCode = strLibraryName };
                    }
                }
            });
        }

    }
}