using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using DigitalPlatform.Text;

namespace DigitalPlatform.Install
{
    public partial class WcfBindingDlg : Form
    {
        public WcfBindingDlg()
        {
            InitializeComponent();
        }

        private void WcfBindingDlg_Load(object sender, EventArgs e)
        {
            try
            {
                // 填充 checked == false 位置的 textbox 缺省值
                for (int i = 0; i < this.DefaultUrls.Length; i++)
                {
                    string strUrl = this.DefaultUrls[i].Trim();
                    if (String.IsNullOrEmpty(strUrl) == true)
                        continue;

                    var scheme = GetUriScheme(strUrl);
                    if (scheme == "net.tcp")
                    {
                        if (this.checkBox_nettcp.Checked == false
                            && String.IsNullOrEmpty(this.textBox_nettcpUrl.Text) == true)
                            this.textBox_nettcpUrl.Text = strUrl;
                    }
                    if (scheme == "net.pipe")
                    {
                        if (this.checkBox_netpipe.Checked == false
                            && String.IsNullOrEmpty(this.textBox_netpipeUrl.Text) == true)
                            this.textBox_netpipeUrl.Text = strUrl;
                    }
                    if (scheme == "http")
                    {
                        if (this.checkBox_http.Checked == false
                            && String.IsNullOrEmpty(this.textBox_httpUrl.Text) == true)
                            this.textBox_httpUrl.Text = strUrl;
                    }
                    // 2021/6/18
                    if (scheme == "https")
                    {
                        if (this.checkBox_https.Checked == false
                            && String.IsNullOrEmpty(this.textBox_httpsUrl.Text) == true)
                            this.textBox_httpsUrl.Text = strUrl;
                    }
                    if (scheme == "rest.http")
                    {
                        if (this.checkBox_rest.Checked == false
                            && String.IsNullOrEmpty(this.textBox_restUrl.Text) == true)
                            this.textBox_restUrl.Text = strUrl;
                    }
                    if (scheme == "basic.http")
                    {
                        if (this.checkBox_basic.Checked == false
                            && String.IsNullOrEmpty(this.textBox_basicUrl.Text) == true)
                            this.textBox_basicUrl.Text = strUrl;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message);
            }
        }

        public static string TryGetUriScheme(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                return uri.Scheme.ToLower();
            }
            catch
            {
                return "";
            }
        }

        public static string GetUriScheme(string url)
        {
            try
            {
                Uri uri = new Uri(url);
                return uri.Scheme.ToLower();
            }
            catch(UriFormatException)
            {
                throw new ArgumentException($"URL '{url}' 格式不合法");
            }
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            try
            {
                if (this.checkBox_nettcp.Checked == true)
                {
                    if (String.IsNullOrEmpty(this.textBox_nettcpUrl.Text) == true)
                    {
                        strError = "尚未指定 NET.TCP 协议的 URL 地址";
                        goto ERROR1;
                    }
                    var scheme = GetUriScheme(this.textBox_nettcpUrl.Text);
                    if (scheme != "net.tcp")
                    {
                        strError = "NET.TCP 协议 URL '" + this.textBox_nettcpUrl.Text + "' 格式错误: 协议名部分不正确";
                        goto ERROR1;
                    }
                }

                if (this.checkBox_netpipe.Checked == true)
                {
                    if (String.IsNullOrEmpty(this.textBox_netpipeUrl.Text) == true)
                    {
                        strError = "尚未指定 NET.PIPE 协议的 URL 地址";
                        goto ERROR1;
                    }
                    var scheme = GetUriScheme(this.textBox_netpipeUrl.Text);
                    if (scheme != "net.pipe")
                    {
                        strError = "NET.PIPE 协议 URL '" + this.textBox_netpipeUrl.Text + "' 格式错误: 协议名部分不正确";
                        goto ERROR1;
                    }

                    // TODO: 检查端口号
                }

                if (this.checkBox_http.Checked == true)
                {
                    if (String.IsNullOrEmpty(this.textBox_httpUrl.Text) == true)
                    {
                        strError = "尚未指定 HTTP 协议的 URL 地址";
                        goto ERROR1;
                    }
                    var scheme = GetUriScheme(this.textBox_httpUrl.Text);
                    if (scheme != "http")
                    {
                        strError = "HTTP 协议 URL '" + this.textBox_httpUrl.Text + "' 格式错误: 协议名部分不正确";
                        goto ERROR1;
                    }
                }

                // 2021/6/18
                if (this.checkBox_https.Checked == true)
                {
                    if (String.IsNullOrEmpty(this.textBox_httpsUrl.Text) == true)
                    {
                        strError = "尚未指定 HTTPS 协议的 URL 地址";
                        goto ERROR1;
                    }
                    var scheme = GetUriScheme(this.textBox_httpsUrl.Text);
                    if (scheme != "https")
                    {
                        strError = "HTTPS 协议 URL '" + this.textBox_httpsUrl.Text + "' 格式错误: 协议名部分不正确";
                        goto ERROR1;
                    }
                }

                if (this.checkBox_rest.Checked == true)
                {
                    if (String.IsNullOrEmpty(this.textBox_restUrl.Text) == true)
                    {
                        strError = "尚未指定 REST.HTTP 协议的 URL 地址";
                        goto ERROR1;
                    }
                    var scheme = GetUriScheme(this.textBox_restUrl.Text);
                    if (scheme != "rest.http")
                    {
                        strError = "REST.BASIC 协议 URL '" + this.textBox_restUrl.Text + "' 格式错误: 协议名部分不正确";
                        goto ERROR1;
                    }
                }

                if (this.checkBox_basic.Checked == true)
                {
                    if (String.IsNullOrEmpty(this.textBox_basicUrl.Text) == true)
                    {
                        strError = "尚未指定 BASIC.HTTP 协议的 URL 地址";
                        goto ERROR1;
                    }
                    var scheme = GetUriScheme(this.textBox_basicUrl.Text);
                    if (scheme != "basic.http")
                    {
                        strError = "BASIC.HTTP 协议 URL '" + this.textBox_basicUrl.Text + "' 格式错误: 协议名部分不正确";
                        goto ERROR1;
                    }
                }
            }
            catch(Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }

            if (this.checkBox_nettcp.Checked == false
                && this.checkBox_netpipe.Checked == false
                && this.checkBox_http.Checked == false
                && this.checkBox_rest.Checked == false
                && this.checkBox_basic.Checked == false
                && this.checkBox_https.Checked == false)
            {
                strError = "必须指定启用至少一个协议";
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

        // *** net.tcp

        public bool NetTcpSelected
        {
            get
            {
                return this.checkBox_nettcp.Checked;
            }
            set
            {
                this.checkBox_nettcp.Checked = value;
            }
        }

        public bool NetTcpEnabled
        {
            get
            {
                return this.checkBox_nettcp.Enabled;
            }
            set
            {
                this.checkBox_nettcp.Enabled = value;
                this.textBox_nettcpComment.Enabled = value;
            }
        }

        public string NetTcpUrl
        {
            get
            {
                return this.textBox_nettcpUrl.Text;
            }
            set
            {
                this.textBox_nettcpUrl.Text = value;
            }
        }

        public string NetTcpComment
        {
            get
            {
                return this.textBox_nettcpComment.Text;
            }
            set
            {
                this.textBox_nettcpComment.Text = value;
            }
        }

        // *** net.pipe

        public bool NetPipeSelected
        {
            get
            {
                return this.checkBox_netpipe.Checked;
            }
            set
            {
                this.checkBox_netpipe.Checked = value;
            }
        }

        public bool NetPipeEnabled
        {
            get
            {
                return this.checkBox_netpipe.Enabled;
            }
            set
            {
                this.checkBox_netpipe.Enabled = value;
                this.textBox_netpipeComment.Enabled = value;
            }
        }

        public string NetPipeUrl
        {
            get
            {
                return this.textBox_netpipeUrl.Text;
            }
            set
            {
                this.textBox_netpipeUrl.Text = value;
            }
        }

        public string NetPipeComment
        {
            get
            {
                return this.textBox_netpipeComment.Text;
            }
            set
            {
                this.textBox_netpipeComment.Text = value;
            }
        }

        // *** http

        public bool HttpSelected
        {
            get
            {
                return this.checkBox_http.Checked;
            }
            set
            {
                this.checkBox_http.Checked = value;
            }
        }

        public bool HttpEnabled
        {
            get
            {
                return this.checkBox_http.Enabled;
            }
            set
            {
                this.checkBox_http.Enabled = value;
                this.textBox_httpComment.Enabled = value;
            }
        }

        public string HttpUrl
        {
            get
            {
                return this.textBox_httpUrl.Text;
            }
            set
            {
                this.textBox_httpUrl.Text = value;
            }
        }

        public string HttpComment
        {
            get
            {
                return this.textBox_httpComment.Text;
            }
            set
            {
                this.textBox_httpComment.Text = value;
            }
        }

        // *** rest.http

        public bool RestSelected
        {
            get
            {
                return this.checkBox_rest.Checked;
            }
            set
            {
                this.checkBox_rest.Checked = value;
            }
        }

        public bool RestEnabled
        {
            get
            {
                return this.checkBox_rest.Enabled;
            }
            set
            {
                this.checkBox_rest.Enabled = value;
                this.textBox_restComment.Enabled = value;
            }
        }

        public string RestUrl
        {
            get
            {
                return this.textBox_restUrl.Text;
            }
            set
            {
                this.textBox_restUrl.Text = value;
            }
        }

        public string RestComment
        {
            get
            {
                return this.textBox_restComment.Text;
            }
            set
            {
                this.textBox_restComment.Text = value;
            }
        }

        // *** basic.http

        public bool BasicSelected
        {
            get
            {
                return this.checkBox_basic.Checked;
            }
            set
            {
                this.checkBox_basic.Checked = value;
            }
        }

        public bool BasicEnabled
        {
            get
            {
                return this.checkBox_basic.Enabled;
            }
            set
            {
                this.checkBox_basic.Enabled = value;
                this.textBox_basicComment.Enabled = value;
            }
        }

        public string BasicUrl
        {
            get
            {
                return this.textBox_basicUrl.Text;
            }
            set
            {
                this.textBox_basicUrl.Text = value;
            }
        }

        public string BasicComment
        {
            get
            {
                return this.textBox_basicComment.Text;
            }
            set
            {
                this.textBox_basicComment.Text = value;
            }
        }

        // *** https

        public bool HttpsSelected
        {
            get
            {
                return this.checkBox_https.Checked;
            }
            set
            {
                this.checkBox_https.Checked = value;
            }
        }

        public bool HttpsEnabled
        {
            get
            {
                return this.checkBox_https.Enabled;
            }
            set
            {
                this.checkBox_https.Enabled = value;
                this.textBox_httpsComment.Enabled = value;
            }
        }

        public string HttpsUrl
        {
            get
            {
                return this.textBox_httpsUrl.Text;
            }
            set
            {
                this.textBox_httpsUrl.Text = value;
            }
        }

        public string HttpsComment
        {
            get
            {
                return this.textBox_httpsComment.Text;
            }
            set
            {
                this.textBox_httpsComment.Text = value;
            }
        }


        public string[] Urls
        {
            get
            {
                List<string> results = new List<string>();

                if (this.checkBox_nettcp.Checked == true)
                {
                    results.Add(this.textBox_nettcpUrl.Text);
                }

                if (this.checkBox_netpipe.Checked == true)
                {
                    results.Add(this.textBox_netpipeUrl.Text);
                }

                if (this.checkBox_http.Checked == true)
                {
                    results.Add(this.textBox_httpUrl.Text);
                }

                if (this.checkBox_rest.Checked == true)
                {
                    results.Add(this.textBox_restUrl.Text);
                }

                if (this.checkBox_basic.Checked == true)
                {
                    results.Add(this.textBox_basicUrl.Text);
                }

                if (this.checkBox_https.Checked == true)
                {
                    results.Add(this.textBox_httpsUrl.Text);
                }

                return StringUtil.FromListString(results);
            }
            set
            {
                this.checkBox_nettcp.Checked = false;
                this.checkBox_netpipe.Checked = false;
                this.checkBox_http.Checked = false;
                this.checkBox_rest.Checked = false;
                this.checkBox_basic.Checked = false;
                this.checkBox_https.Checked = false;

                for (int i = 0; i < value.Length; i++)
                {
                    string strUrl = value[i].Trim();
                    if (String.IsNullOrEmpty(strUrl) == true)
                        continue;

                    Uri uri = new Uri(strUrl);
                    if (uri.Scheme.ToLower() == "net.tcp")
                    {
                        this.textBox_nettcpUrl.Text = strUrl;
                        this.checkBox_nettcp.Checked = true;
                    }
                    if (uri.Scheme.ToLower() == "net.pipe")
                    {
                        this.textBox_netpipeUrl.Text = strUrl;
                        this.checkBox_netpipe.Checked = true;
                    }
                    if (uri.Scheme.ToLower() == "http")
                    {
                        this.textBox_httpUrl.Text = strUrl;
                        this.checkBox_http.Checked = true;
                    }
                    if (uri.Scheme.ToLower() == "rest.http")
                    {
                        this.textBox_restUrl.Text = strUrl;
                        this.checkBox_rest.Checked = true;
                    }
                    if (uri.Scheme.ToLower() == "basic.http")
                    {
                        this.textBox_basicUrl.Text = strUrl;
                        this.checkBox_basic.Checked = true;
                    }
                    if (uri.Scheme.ToLower() == "https")
                    {
                        this.textBox_httpsUrl.Text = strUrl;
                        this.checkBox_https.Checked = true;
                    }
                }
            }
        }

        public string[] DefaultUrls = new string[0];

        private void checkBox_nettcp_CheckedChanged(object sender, EventArgs e)
        {
            this.textBox_nettcpUrl.Enabled = this.checkBox_nettcp.Checked;
            this.textBox_nettcpComment.Enabled = this.checkBox_nettcp.Checked;
        }

        private void checkBox_netpipe_CheckedChanged(object sender, EventArgs e)
        {
            this.textBox_netpipeUrl.Enabled = this.checkBox_netpipe.Checked;
            this.textBox_netpipeComment.Enabled = this.checkBox_netpipe.Checked;
        }

        private void checkBox_http_CheckedChanged(object sender, EventArgs e)
        {
            this.textBox_httpUrl.Enabled = this.checkBox_http.Checked;
            this.textBox_httpComment.Enabled = this.checkBox_http.Checked;
        }

        private void checkBox_rest_CheckedChanged(object sender, EventArgs e)
        {
            this.textBox_restUrl.Enabled = this.checkBox_rest.Checked;
            this.textBox_restComment.Enabled = this.checkBox_rest.Checked;
        }

        private void checkBox_basic_CheckedChanged(object sender, EventArgs e)
        {
            this.textBox_basicUrl.Enabled = this.checkBox_basic.Checked;
            this.textBox_basicComment.Enabled = this.checkBox_basic.Checked;
        }

        private void checkBox_https_CheckedChanged(object sender, EventArgs e)
        {
            this.textBox_httpsUrl.Enabled = this.checkBox_https.Checked;
            this.textBox_httpsComment.Enabled = this.checkBox_https.Checked;
        }
    }
}
