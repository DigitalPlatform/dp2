using Microsoft.AspNet.SignalR.Client.Hubs;
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

namespace dp2Circulation
{
    public partial class ChatForm : MyForm
    {
        public ChatForm()
        {
            InitializeComponent();

            this.webBrowser1.Width = 300;
            this.panel_input.Width = 300;
        }

        private void IMForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }

            ClearHtml();

            if (string.IsNullOrEmpty(this.dp2MServerUrl) == false)
            {
                this.BeginInvoke(new Action<string>(ConnectAsync), this.dp2MServerUrl);
            }
        }

        private void IMForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void IMForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseConnection();
        }

        string dp2MServerUrl
        {
            get
            {
                // dp2MServer URL
                return this.MainForm.AppInfo.GetString("config",
                    "im_server_url",
                    "http://dp2003.com/dp2MServer");
            }
        }

        // 登录到 IM 服务器
        void SignIn()
        {

        }

        private IHubProxy HubProxy
        {
            get;
            set;
        }

        private HubConnection Connection
        {
            get;
            set;
        }

        public override void EnableControls(bool bEnable)
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action<bool>(EnableControls), bEnable);
                return;
            }

            this.textBox_input.Enabled = bEnable;
            this.button_send.Enabled = bEnable;

            // base.EnableControls(bEnable);
        }

        void AddInfoLine(string strContent)
        {
            string strText = "<div class='item'>"
+ "<div class='item_line'>"
+ " <div class='item_summary'>" + HttpUtility.HtmlEncode(strContent).Replace("\r\n", "<br/>") + "</div>"
+ "</div>"
+ " <div class='clear'></div>"
+ "</div>";
            AppendHtml(strText);
        }

        void AddMessageLine(string strName, string strContent)
        {
            string strText = "<div class='item'>"
+ "<div class='item_line'>"
+ " <div class='item_summary'>" + HttpUtility.HtmlEncode(strName).Replace("\r\n", "<br/>") + "</div>"
+ " <div class='item_summary'>" + HttpUtility.HtmlEncode(strContent).Replace("\r\n", "<br/>") + "</div>"
+ "</div>"
+ " <div class='clear'></div>"
+ "</div>";
            AppendHtml(strText);
        }

        void AddErrorLine(string strContent)
        {
            string strText = "<div class='item error'>"
+ "<div class='item_line'>"
+ " <div class='item_summary'>" + HttpUtility.HtmlEncode(strContent).Replace("\r\n", "<br/>") + "</div>"
+ "</div>"
+ " <div class='clear'></div>"
+ "</div>";
            AppendHtml(strText);
        }

        private void ConnectAsync(string strServerUrl)
        {
            AddInfoLine("正在连接服务器 " + strServerUrl + " ...");

            Connection = new HubConnection(strServerUrl);
            Connection.Closed += new Action(Connection_Closed);
            HubProxy = Connection.CreateHubProxy("MyHub");

#if NO
            HubProxy.On<string, string>("AddMessage", (name, message) =>
                this.Invoke((Action)(() =>
                    richTextBoxConsole.AppendText(string.Format("{0}: {1}" + Environment.NewLine, name, message))
                    ))
            );
#endif
            HubProxy.On<string, string>("AddMessage", (name, message) =>
    this.Invoke(new Action<string, string>(AddMessageLine), name, message 
        )
);

            Task task = Connection.Start();
#if NO
            CancellationTokenSource token = new CancellationTokenSource();
            if (!task.Wait(60 * 1000, token.Token))
            {
                token.Cancel();
                // labelStatusText.Text = "time out";
                AddMessageLine("error", "time out");
                return;
            }
#endif
            while (task.IsCompleted == false)
            {
                Application.DoEvents();
                Thread.Sleep(200);
            }

            if (task.IsFaulted == true)
            {
#if NO
                if (task.Exception is HttpRequestException)
                    labelStatusText.Text = "Unable to connect to server: start server bofore connection client.";
#endif
                AddErrorLine(GetExceptionText(task.Exception));
                return;
            }

            EnableControls(true);
            textBox_input.Focus();
            AddInfoLine("成功连接到 " + strServerUrl);
        }

        static string GetExceptionText(AggregateException exception)
        {
            StringBuilder text = new StringBuilder();
            foreach (Exception ex in exception.InnerExceptions)
            {
                text.Append(ex.Message + "\r\n");
                // text.Append(ex.ToString() + "\r\n");
            }

            return text.ToString();
        }

        void Connection_Closed()
        {
            this.EnableControls(false);
#if NO
            this.Invoke((Action)(() => panelChat.Visible = false));
            this.Invoke((Action)(() => buttonSend.Enabled = false));
            this.Invoke((Action)(() => this.labelStatusText.Text = "You have been disconnected."));
            this.Invoke((Action)(() => this.panelSignIn.Visible = true));
#endif
        }

        void CloseConnection()
        {
            if (this.Connection != null)
            {
                this.Connection.Stop(new TimeSpan(0,0,5));
                this.Connection = null;
            }
        }

        /// <summary>
        /// 清除已有的 HTML 显示
        /// </summary>
        public void ClearHtml()
        {
            string strCssUrl = Path.Combine(this.MainForm.DataDir, "history.css");
            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";
            string strJs = "";

            {
                HtmlDocument doc = webBrowser1.Document;

                if (doc == null)
                {
                    webBrowser1.Navigate("about:blank");
                    doc = webBrowser1.Document;
                }
                doc = doc.OpenNew(true);
            }

            Global.WriteHtml(this.webBrowser1,
                "<html><head>" + strLink + strJs + "</head><body>");
        }


        delegate void Delegate_AppendHtml(string strText);
        /// <summary>
        /// 向 IE 控件中追加一段 HTML 内容
        /// </summary>
        /// <param name="strText">HTML 内容</param>
        public void AppendHtml(string strText)
        {
            if (this.InvokeRequired)
            {
                this.BeginInvoke(new Action<string>(AppendHtml), strText);
                return;
            }

            Global.WriteHtml(this.webBrowser1,
                strText);

            // 因为HTML元素总是没有收尾，其他有些方法可能不奏效
            this.webBrowser1.Document.Window.ScrollTo(0,
                this.webBrowser1.Document.Body.ScrollRectangle.Height);
        }

        private void button_send_Click(object sender, EventArgs e)
        {
            string strUserName = this.MainForm.GetCurrentUserName();
            HubProxy.Invoke("Send", strUserName, this.textBox_input.Text);
            textBox_input.Text = string.Empty;
            textBox_input.Focus();
        }

    }
}
