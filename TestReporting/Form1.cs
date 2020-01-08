
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Xml;
using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.CommonControl;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.LibraryServer.Reporting;

namespace TestReporting
{
    public partial class Form1 : Form
    {
        // 主要的通道池，用于当前服务器
        public LibraryChannelPool _channelPool = new LibraryChannelPool();

        FloatingMessageForm _floatingMessage = null;

        CancellationTokenSource _cancel = new CancellationTokenSource();

        public Form1()
        {
            ClientInfo.ProgramName = "fingerprintcenter";
            ClientInfo.MainForm = this;

            InitializeComponent();

            {
                _floatingMessage = new FloatingMessageForm(this);
                _floatingMessage.Font = new System.Drawing.Font(this.Font.FontFamily, this.Font.Size * 2, FontStyle.Bold);
                _floatingMessage.Opacity = 0.7;
                _floatingMessage.RectColor = Color.Green;
                _floatingMessage.Show(this);
            }
        }

        public string UiState
        {
            get
            {
                List<object> controls = new List<object>
                {
                    this.tabControl_main,
                    this.textBox_cfg_dp2LibraryServerUrl,
                    this.textBox_cfg_userName,
                    new SavePassword(this.textBox_cfg_password, this.checkBox_cfg_savePasswordLong),
                    this.textBox_cfg_location,
                    new ControlWrapper(this.checkBox_cfg_savePasswordLong, true),
                };
                return GuiState.GetUiState(controls);
            }
            set
            {
                List<object> controls = new List<object>
                {
                    this.tabControl_main,
                    this.textBox_cfg_dp2LibraryServerUrl,
                    this.textBox_cfg_userName,
                    new SavePassword(this.textBox_cfg_password, this.checkBox_cfg_savePasswordLong),
                    this.textBox_cfg_location,
                    new ControlWrapper(this.checkBox_cfg_savePasswordLong, true),
                };
                GuiState.SetUiState(controls, value);
            }
        }

        public void ShowMessage(string strMessage,
string strColor = "",
bool bClickClose = false)
        {
            if (this._floatingMessage == null)
                return;

            Color color = Color.FromArgb(80, 80, 80);

            if (strColor == "red")          // 出错
                color = Color.DarkRed;
            else if (strColor == "yellow")  // 成功，提醒
                color = Color.DarkGoldenrod;
            else if (strColor == "green")   // 成功
                color = Color.Green;
            else if (strColor == "progress")    // 处理过程
                color = Color.FromArgb(80, 80, 80);

            this._floatingMessage.SetMessage(strMessage, color, bClickClose);
        }

        public void ClearMessage()
        {
            this.ShowMessage("");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            ClientInfo.Initial("testreporting");

            this.UiState = ClientInfo.Config.Get("global", "ui_state", ""); // Properties.Settings.Default.ui_state;

            this.textBox_replicationStart.Text = ClientInfo.Config.Get("global", "replication_start", "");   //  Properties.Settings.Default.repPlan;

            ClearHtml();

            // 显示版本号
            this.OutputHistory($"版本号: {ClientInfo.ClientVersion}");

            this._channelPool.BeforeLogin += new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            this._channelPool.AfterLogin += new AfterLoginEventHandle(Channel_AfterLogin);

        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            SaveSettings();

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            _cancel?.Cancel();
            _cancel?.Dispose();

            this._channelPool.BeforeLogin -= new DigitalPlatform.LibraryClient.BeforeLoginEventHandle(Channel_BeforeLogin);
            this._channelPool.AfterLogin -= new AfterLoginEventHandle(Channel_AfterLogin);

        }

        void SaveSettings()
        {
            if (this.checkBox_cfg_savePasswordLong.Checked == false)
                this.textBox_cfg_password.Text = "";
            ClientInfo.Config?.Set("global", "ui_state", this.UiState);
            ClientInfo.Config?.Set("global", "replication_start", this.textBox_replicationStart.Text);
            ClientInfo.Finish();
        }

        internal void Channel_BeforeLogin(object sender,
DigitalPlatform.LibraryClient.BeforeLoginEventArgs e)
        {
            if (e.FirstTry == true)
            {
                // string strPhoneNumber = "";

                {
                    e.UserName = this.textBox_cfg_userName.Text;

                    // e.Password = this.DecryptPasssword(e.Password);
                    e.Password = this.textBox_cfg_password.Text;

                    bool bIsReader = false;

                    string strLocation = this.textBox_cfg_location.Text;

                    e.Parameters = "location=" + strLocation;
                    if (bIsReader == true)
                        e.Parameters += ",type=reader";
                }

                // 2014/9/13
                // e.Parameters += ",mac=" + StringUtil.MakePathList(SerialCodeForm.GetMacAddress(), "|");

                e.Parameters += ",client=testreporting|" + ClientInfo.ClientVersion;

                if (String.IsNullOrEmpty(e.UserName) == false)
                    return; // 立即返回, 以便作第一次 不出现 对话框的自动登录
                else
                {
                    e.ErrorInfo = "尚未配置 dp2library 服务器用户名";
                    e.Cancel = true;
                }
            }

            // e.ErrorInfo = "尚未配置 dp2library 服务器用户名";
            e.Cancel = true;
        }

        string _currentUserName = "";

        public string ServerUID = "";
        public string ServerVersion = "";

        internal void Channel_AfterLogin(object sender, AfterLoginEventArgs e)
        {
            LibraryChannel channel = sender as LibraryChannel;
            _currentUserName = channel.UserName;
        }

        List<LibraryChannel> _channelList = new List<LibraryChannel>();

        public void AbortAllChannel()
        {
            foreach (LibraryChannel channel in _channelList)
            {
                if (channel != null)
                    channel.Abort();
            }
        }

        // parameters:
        //      style    风格。如果为 GUI，表示会自动添加 Idle 事件，并在其中执行 Application.DoEvents
        public LibraryChannel GetChannel()
        {
            string strServerUrl = this.textBox_cfg_dp2LibraryServerUrl.Text;

            string strUserName = this.textBox_cfg_userName.Text;

            LibraryChannel channel = this._channelPool.GetChannel(strServerUrl, strUserName);
            _channelList.Add(channel);
            // TODO: 检查数组是否溢出
            return channel;
        }

        public void ReturnChannel(LibraryChannel channel)
        {
            this._channelPool.ReturnChannel(channel);
            _channelList.Remove(channel);
        }

        #region 浏览器控件

        public void ClearHtml()
        {
            string strCssUrl = Path.Combine(ClientInfo.DataDir, "history.css");
            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";
            string strJs = "";
            {
                HtmlDocument doc = this.webBrowser1.Document;

                if (doc == null)
                {
                    this.webBrowser1.Navigate("about:blank");
                    doc = this.webBrowser1.Document;
                }
                doc = doc.OpenNew(true);
            }

            WriteHtml(this.webBrowser1,
                "<html><head>" + strLink + strJs + "</head><body>");
        }


        delegate void Delegate_AppendHtml(string strText);

        public void AppendHtml(string strText)
        {
            if (this.webBrowser1.InvokeRequired)
            {
                Delegate_AppendHtml d = new Delegate_AppendHtml(AppendHtml);
                this.webBrowser1.BeginInvoke(d, new object[] { strText });
                return;
            }

            WriteHtml(this.webBrowser1,
                strText);
            // Global.ScrollToEnd(this.WebBrowser);

            // 因为HTML元素总是没有收尾，其他有些方法可能不奏效
            this.webBrowser1.Document.Window.ScrollTo(0,
    this.webBrowser1.Document.Body.ScrollRectangle.Height);
        }

        public static void WriteHtml(WebBrowser webBrowser,
string strHtml)
        {

            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                // webBrowser.Navigate("about:blank");
                Navigate(webBrowser, "about:blank");

                doc = webBrowser.Document;
            }

            // doc = doc.OpenNew(true);
            doc.Write(strHtml);

            // 保持末行可见
            // ScrollToEnd(webBrowser);
        }

        // 2015/7/28 
        // 能处理异常的 Navigate
        internal static void Navigate(WebBrowser webBrowser, string urlString)
        {
            int nRedoCount = 0;
        REDO:
            try
            {
                webBrowser.Navigate(urlString);
            }
            catch (System.Runtime.InteropServices.COMException ex)
            {
                /*
System.Runtime.InteropServices.COMException (0x800700AA): 请求的资源在使用中。 (异常来自 HRESULT:0x800700AA)
   在 System.Windows.Forms.UnsafeNativeMethods.IWebBrowser2.Navigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.PerformNavigate2(Object& URL, Object& flags, Object& targetFrameName, Object& postData, Object& headers)
   在 System.Windows.Forms.WebBrowser.Navigate(String urlString)
   在 dp2Circulation.QuickChargingForm._setReaderRenderString(String strText) 位置 F:\cs4.0\dp2Circulation\Charging\QuickChargingForm.cs:行号 394
                 * */
                if ((uint)ex.ErrorCode == 0x800700AA)
                {
                    nRedoCount++;
                    if (nRedoCount < 5)
                    {
                        Application.DoEvents(); // 2015/8/13
                        Thread.Sleep(200);
                        goto REDO;
                    }
                }

                throw ex;
            }
        }

        public static void SetHtmlString(WebBrowser webBrowser,
    string strHtml,
    string strDataDir,
    string strTempFileType)
        {
            // StopWebBrowser(webBrowser);

            strHtml = strHtml.Replace("%datadir%", strDataDir);
            strHtml = strHtml.Replace("%mappeddir%", Path.Combine(strDataDir, "servermapped"));

            string strTempFilename = Path.Combine(strDataDir, "~temp_" + strTempFileType + ".html");
            using (StreamWriter sw = new StreamWriter(strTempFilename, false, Encoding.UTF8))
            {
                sw.Write(strHtml);
            }
            // webBrowser.Navigate(strTempFilename);
            Navigate(webBrowser, strTempFilename);  // 2015/7/28
        }

        public static void SetHtmlString(WebBrowser webBrowser,
string strHtml)
        {
            webBrowser.DocumentText = strHtml;
        }

        /// <summary>
        /// 向控制台输出 HTML
        /// </summary>
        /// <param name="strHtml">要输出的 HTML 字符串</param>
        public void OutputHtml(string strHtml)
        {
            AppendHtml(strHtml);
        }

        public void OutputHistory(string strText, int nWarningLevel = 0)
        {
            OutputText(DateTime.Now.ToLongTimeString() + " " + strText, nWarningLevel);
        }

        // parameters:
        //      nWarningLevel   0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)
        /// <summary>
        /// 向控制台输出纯文本
        /// </summary>
        /// <param name="strText">要输出的纯文本字符串</param>
        /// <param name="nWarningLevel">警告级别。0 正常文本(白色背景) 1 警告文本(黄色背景) >=2 错误文本(红色背景)</param>
        public void OutputText(string strText, int nWarningLevel = 0)
        {
            string strClass = "normal";
            if (nWarningLevel == 1)
                strClass = "warning";
            else if (nWarningLevel >= 2)
                strClass = "error";
            AppendHtml("<div class='debug " + strClass + "'>" + HttpUtility.HtmlEncode(strText).Replace("\r\n", "<br/>") + "</div>");
        }

        #endregion

        private void toolStripButton_cfg_setHongnibaServer_Click(object sender, EventArgs e)
        {
            if (this.textBox_cfg_dp2LibraryServerUrl.Text != ServerDlg.HnbUrl)
            {
                this.textBox_cfg_dp2LibraryServerUrl.Text = ServerDlg.HnbUrl;

                this.textBox_cfg_userName.Text = "";
                this.textBox_cfg_password.Text = "";
            }
        }

        private void toolStripButton_cfg_setXeServer_Click(object sender, EventArgs e)
        {
            if (this.textBox_cfg_dp2LibraryServerUrl.Text != "net.pipe://localhost/dp2library/xe")
            {

                this.textBox_cfg_dp2LibraryServerUrl.Text = "net.pipe://localhost/dp2library/xe";

                this.textBox_cfg_userName.Text = "supervisor";
                this.textBox_cfg_password.Text = "";
            }
        }

        private async void MenuItem_buildPlan_Click(object sender, EventArgs e)
        {
            var result = await Replication();
            if (result.Value == -1)
                MessageBox.Show(this, result.ErrorInfo);
            else
                MessageBox.Show(this, "OK");
        }

        Task<NormalResult> Replication()
        {
            return Task<NormalResult>.Run(() =>
            {
                Replication replication = new Replication();
                LibraryChannel channel = this.GetChannel();
                try
                {
                    int nRet = replication.Initialize(channel,
                        out string strError);
                    if (nRet == -1)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = strError
                        };

                    nRet = replication.BuildPlan("*",
                        channel,
                        (message) =>
                        {
                            OutputHistory(message);
                        },
                        out XmlDocument task_dom,
                        out strError);
                    if (nRet == -1)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = strError
                        };

                    DatabaseConfig config = new DatabaseConfig {
                        ServerName = "localhost",
                        DatabaseName = "testrep",
                        UserName = "root",
                        Password = "test",
                    };
                    nRet = replication.DoPlan(
                        config,
channel,
ref task_dom,
out strError);
                    if (nRet == -1)
                        return new NormalResult
                        {
                            Value = -1,
                            ErrorInfo = strError
                        };

                    return new NormalResult();
                }
                finally
                {
                    this.ReturnChannel(channel);
                }
            });
        }

        private void MenuItem_doPlan_Click(object sender, EventArgs e)
        {

        }
    }
}
