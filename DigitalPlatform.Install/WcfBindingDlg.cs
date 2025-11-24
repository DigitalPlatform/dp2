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
                // 补上空缺部分为默认值
                FillUrls(this.DefaultUrls, true);
            }
            catch (Exception ex)
            {
                this.MessageBoxShow(ex.Message);
            }
        }

        static string GetSchemeName(string scheme)
        {
            scheme = scheme.Replace(".", "");

            if (scheme.EndsWith("https"))
                scheme = scheme.Substring(0, scheme.Length - 1);
            return scheme;
        }

        // 根据 scheme 名字找到对应的 TextBox
        TextBox FindTextBoxControl(string scheme)
        {
            scheme = GetSchemeName(scheme);

            var name = "textBox_" + scheme + "Url";
            TextBox result = null;
            TraverseControls(this, (control) =>
            {
                if (control is TextBox textbox)
                {
                    if (textbox.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        result = textbox;
                        return;
                    }
                }
            });

            if (result == null)
                return this.textBox_othersUrl;
            return result;
        }

        CheckBox FindCheckBoxControl(string scheme)
        {
            scheme = GetSchemeName(scheme);

            var name = "checkBox_" + scheme;
            CheckBox result = null;
            TraverseControls(this, (control) =>
            {
                if (control is CheckBox textbox)
                {
                    if (textbox.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        result = textbox;
                        return;
                    }
                }
            });

            if (result == null)
                return this.checkBox_others;
            return result;
        }

        // 递归遍历所有控件
        private void TraverseControls(Control control, Action<Control> action)
        {
            // 执行当前控件的操作
            action(control);

            // 遍历所有子控件
            foreach (Control child in control.Controls)
            {
                TraverseControls(child, action);
            }
        }

        // parameters:
        //      fill_default    本次调用目的是否为填充缺省值。
        //                      填充缺省值时，只填充那些没有内容的 TextBox。并且不修改 checkbox 状态
        //                      否则，表示用户明确指定了一组 URL，要清除以前的内容，然后重新填充。并且要修改 checkbox 状态
        void FillUrls(string[] urls,
            bool fill_default)
        {
#if REMOVED
            this.checkBox_nettcp.Checked = false;
            this.checkBox_netpipe.Checked = false;
            this.checkBox_http.Checked = false;
            this.checkBox_rest.Checked = false;
            this.checkBox_basic.Checked = false;
            this.checkBox_https.Checked = false;
#endif
            if (fill_default == false)
            {
                // 清除原有 .Text 内容
                TraverseControls(this, (control) =>
                {
                    if (control is CheckBox checkbox)
                        checkbox.Checked = false;
                });
            }

            if (fill_default)
            {
                // 在 Tag 中标记哪些 TextBox 是空的
                TraverseControls(this, (control) =>
                {
                    if (control is TextBox textbox
                        && textbox.Name.EndsWith("Url"))
                    {
                        if (string.IsNullOrEmpty(textbox.Text))
                            textbox.Tag = "empty";
                        else
                            textbox.Tag = "";
                    }
                });

                /*
                this.textBox_nettcpUrl.Text = "";
                this.textBox_netpipeUrl.Text = "";
                this.textBox_httpUrl.Text = "";
                this.textBox_restUrl.Text = "";
                this.textBox_basicUrl.Text = "";
                */
            }
            else
            {
                // 清除原有 .Text 内容
                TraverseControls(this, (control) =>
                {
                    if (control is TextBox textbox
        && textbox.Name.EndsWith("Url"))
                        textbox.Text = "";
                });
            }

            // 填充 checked == false 位置的 textbox 缺省值
            foreach (string strUrl in urls)
            {
                // string strUrl = this.DefaultUrls[i].Trim();
                if (String.IsNullOrEmpty(strUrl) == true)
                    continue;

                var scheme = GetUriScheme(strUrl);
                var textbox = FindTextBoxControl(scheme);
                if (textbox == null)
                    throw new Exception($"无法根据 URL '{strUrl}' 中的协议部分 '{scheme}' 找到对应的 TextBox 控件");
                var checkBox = FindCheckBoxControl(scheme);
                if (checkBox == null)
                    throw new Exception($"无法根据 URL '{strUrl}' 中的协议部分 '{scheme}' 找到对应的 CheckBox 控件");

                var textbox_tag = textbox.Tag as string;
                if (fill_default == false
                    || (fill_default && textbox_tag == "empty"))
                {
                    AppendTextBox(textbox, strUrl);
                    if (fill_default == false)
                        checkBox.Checked = true;
                }

#if REMOVED
                if (scheme == "net.tcp")
                {
                    AppendTextBox(textBox_nettcpUrl, strUrl);
                    checkBox_nettcp.Checked = true;
                    /*
                    if (this.checkBox_nettcp.Checked == false
                        && String.IsNullOrEmpty(this.textBox_nettcpUrl.Text) == true)
                        this.textBox_nettcpUrl.Text = strUrl;
                    */
                }
                if (scheme == "net.pipe")
                {
                    AppendTextBox(textBox_netpipeUrl, strUrl);
                    checkBox_netpipe.Checked = true;
                    /*
                    if (this.checkBox_netpipe.Checked == false
                        && String.IsNullOrEmpty(this.textBox_netpipeUrl.Text) == true)
                        this.textBox_netpipeUrl.Text = strUrl;
                    */
                }
                if (scheme == "http" || scheme == "https")
                {
                    AppendTextBox(textBox_httpUrl, strUrl);
                    checkBox_http.Checked = true;
                    /*
                    if (this.checkBox_http.Checked == false
                        && String.IsNullOrEmpty(this.textBox_httpUrl.Text) == true)
                        this.textBox_httpUrl.Text = strUrl;
                    */
                }
                /*
                // 2021/6/18
                if (scheme == "https")
                {
                    if (this.checkBox_https.Checked == false
                        && String.IsNullOrEmpty(this.textBox_httpsUrl.Text) == true)
                        this.textBox_httpsUrl.Text = strUrl;
                }
                */
                if (scheme == "rest.http" || scheme == "rest.https")
                {
                    AppendTextBox(textBox_restUrl, strUrl);
                    checkBox_rest.Checked = true;
                    /*
                    if (this.checkBox_rest.Checked == false
                        && String.IsNullOrEmpty(this.textBox_restUrl.Text) == true)
                        this.textBox_restUrl.Text = strUrl;
                    */
                }
                if (scheme == "basic.http" || scheme == "basic.https")
                {
                    AppendTextBox(textBox_basicUrl, strUrl);
                    checkBox_basic.Checked = true;
                    /*
                    if (this.checkBox_basic.Checked == false
                        && String.IsNullOrEmpty(this.textBox_basicUrl.Text) == true)
                        this.textBox_basicUrl.Text = strUrl;
                    */
                }

#endif
            }
        }

        List<string> GetUrls()
        {
            List<string> results = new List<string>();

            if (this.checkBox_nettcp.Checked == true)
            {
                results.AddRange(GetUrls(textBox_nettcpUrl.Text));
            }

            if (this.checkBox_netpipe.Checked == true)
            {
                results.AddRange(GetUrls(textBox_netpipeUrl.Text));
            }

            if (this.checkBox_http.Checked == true)
            {
                results.AddRange(GetUrls(textBox_httpUrl.Text));
            }

            if (this.checkBox_resthttp.Checked == true)
            {
                results.AddRange(GetUrls(textBox_resthttpUrl.Text));
            }

            if (this.checkBox_basichttp.Checked == true)
            {
                results.AddRange(GetUrls(textBox_basichttpUrl.Text));
            }

            if (this.checkBox_others.Checked == true)
            {
                results.AddRange(GetUrls(textBox_othersUrl.Text));
            }
            return (results);
        }

        // parameters:
        //      value   形如 "net.tcp://localhost:8001/;net.tcp://localhost:8002/"
        static List<string> GetUrls(string value)
        {
            var results = new List<string>();
            foreach (var s in value.Split(new char[] { ';', ',' }, StringSplitOptions.RemoveEmptyEntries))
            {
                results.Add(s.Trim());
            }
            return results;
        }

        // 在 TextBox 中添加 内容。如果已经有内容，则以分号间隔再添加
        static void AppendTextBox(TextBoxBase textbox, string protocol)
        {
            if (string.IsNullOrEmpty(protocol))
                return;
            if (string.IsNullOrEmpty(textbox.Text) == false)
                textbox.Text += ";";
            textbox.Text += protocol;
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
            catch (UriFormatException)
            {
                throw new ArgumentException($"URL '{url}' 格式不合法");
            }
        }

        List<string> VerifyUrls()
        {
            var errors = new List<string>();
            TraverseControls(this, (control) =>
            {
                if (control is CheckBox checkbox)
                {
                    if (checkbox.Checked == false)
                        return;
                    var name = checkbox.Name.Substring("checkBox_".Length);
                    // other 类型暂不校验
                    if (name == "others")
                        return;
                    var textbox = FindTextBoxControl(name);
                    if (textbox == null)
                        throw new Exception($"VerifyUrls() 中无法根据 '{name}' 找到对应的 TextBox 控件");
                    var urls = GetUrls(textbox.Text);
                    if (urls.Count == 0)
                    {
                        errors.Add($"协议 '{name}' 输入框内容中尚未指定 URL");
                        return;
                    }
                    else
                    {
                        var prefix1 = name + "://";
                        var prefix2 = name + "s://";

                        var wrongs = urls.Where(o => o.Replace(".", "").StartsWith(prefix1) == false && o.Replace(".", "").StartsWith(prefix2) == false).ToList();
                        if (wrongs.Count > 0)
                        {
                            errors.Add($"协议 '{name}' 输入框内容包含不正确的 URL: {StringUtil.MakePathList(wrongs)}");
                        }
                    }
                }
            });

            return errors;
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";

            try
            {
                var errors = VerifyUrls();
                if (errors.Count > 0)
                {
                    strError = StringUtil.MakePathList(errors, "\r\n");
                    goto ERROR1;
                }
#if REMOVED
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

                /*
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
                */

                if (this.checkBox_resthttp.Checked == true)
                {
                    if (String.IsNullOrEmpty(this.textBox_resthttpUrl.Text) == true)
                    {
                        strError = "尚未指定 REST.HTTP 协议的 URL 地址";
                        goto ERROR1;
                    }
                    var scheme = GetUriScheme(this.textBox_resthttpUrl.Text);
                    if (scheme != "rest.http")
                    {
                        strError = "REST.BASIC 协议 URL '" + this.textBox_resthttpUrl.Text + "' 格式错误: 协议名部分不正确";
                        goto ERROR1;
                    }
                }

                if (this.checkBox_basichttp.Checked == true)
                {
                    if (String.IsNullOrEmpty(this.textBox_basichttpUrl.Text) == true)
                    {
                        strError = "尚未指定 BASIC.HTTP 协议的 URL 地址";
                        goto ERROR1;
                    }
                    var scheme = GetUriScheme(this.textBox_basichttpUrl.Text);
                    if (scheme != "basic.http")
                    {
                        strError = "BASIC.HTTP 协议 URL '" + this.textBox_basichttpUrl.Text + "' 格式错误: 协议名部分不正确";
                        goto ERROR1;
                    }
                }

#endif
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                goto ERROR1;
            }

            if (/*this.checkBox_nettcp.Checked == false
                && this.checkBox_netpipe.Checked == false
                && this.checkBox_http.Checked == false
                && this.checkBox_resthttp.Checked == false
                && this.checkBox_basichttp.Checked == false
                && this.checkBox_others.Checked == false*/
                CountSelectedCheckBox() == 0)
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

        // 统计已经 Checked 的 CheckBox 总数
        int CountSelectedCheckBox()
        {
            int count = 0;
            TraverseControls(this, (control) =>
            {
                if (control is CheckBox textbox)
                {
                    count++;
                }
            });

            return count;
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
                return this.checkBox_resthttp.Checked;
            }
            set
            {
                this.checkBox_resthttp.Checked = value;
            }
        }

        public bool RestEnabled
        {
            get
            {
                return this.checkBox_resthttp.Enabled;
            }
            set
            {
                this.checkBox_resthttp.Enabled = value;
                this.textBox_resthttpComment.Enabled = value;
            }
        }

        public string RestUrl
        {
            get
            {
                return this.textBox_resthttpUrl.Text;
            }
            set
            {
                this.textBox_resthttpUrl.Text = value;
            }
        }

        public string RestComment
        {
            get
            {
                return this.textBox_resthttpComment.Text;
            }
            set
            {
                this.textBox_resthttpComment.Text = value;
            }
        }

        // *** basic.http

        public bool BasicSelected
        {
            get
            {
                return this.checkBox_basichttp.Checked;
            }
            set
            {
                this.checkBox_basichttp.Checked = value;
            }
        }

        public bool BasicEnabled
        {
            get
            {
                return this.checkBox_basichttp.Enabled;
            }
            set
            {
                this.checkBox_basichttp.Enabled = value;
                this.textBox_basichttpComment.Enabled = value;
            }
        }

        public string BasicUrl
        {
            get
            {
                return this.textBox_basichttpUrl.Text;
            }
            set
            {
                this.textBox_basichttpUrl.Text = value;
            }
        }

        public string BasicComment
        {
            get
            {
                return this.textBox_basichttpComment.Text;
            }
            set
            {
                this.textBox_basichttpComment.Text = value;
            }
        }

        // *** https

        public bool OthersSelected
        {
            get
            {
                return this.checkBox_others.Checked;
            }
            set
            {
                this.checkBox_others.Checked = value;
            }
        }

        public bool OthersEnabled
        {
            get
            {
                return this.checkBox_others.Enabled;
            }
            set
            {
                this.checkBox_others.Enabled = value;
                this.textBox_othersComment.Enabled = value;
            }
        }

        public string OthersUrl
        {
            get
            {
                return this.textBox_othersUrl.Text;
            }
            set
            {
                this.textBox_othersUrl.Text = value;
            }
        }

        public string OthersComment
        {
            get
            {
                return this.textBox_othersComment.Text;
            }
            set
            {
                this.textBox_othersComment.Text = value;
            }
        }


        public string[] Urls
        {
            get
            {
                return GetUrls().ToArray();
#if REMOVED
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
#endif
            }
            set
            {
                FillUrls(value, false);
#if REMOVED
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
#endif
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
            this.textBox_resthttpUrl.Enabled = this.checkBox_resthttp.Checked;
            this.textBox_resthttpComment.Enabled = this.checkBox_resthttp.Checked;
        }

        private void checkBox_basic_CheckedChanged(object sender, EventArgs e)
        {
            this.textBox_basichttpUrl.Enabled = this.checkBox_basichttp.Checked;
            this.textBox_basichttpComment.Enabled = this.checkBox_basichttp.Checked;
        }

        private void checkBox_others_CheckedChanged(object sender, EventArgs e)
        {
            this.textBox_othersUrl.Enabled = this.checkBox_others.Checked;
            this.textBox_othersComment.Enabled = this.checkBox_others.Checked;
        }
    }
}
