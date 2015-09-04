using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;

using DigitalPlatform;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.Text;
// using Microsoft.AspNet.SignalR.Client.Hubs;


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

        }

        private void IMForm_FormClosing(object sender, FormClosingEventArgs e)
        {

        }

        private void IMForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            //CloseConnection();

            //this._channelPool.BeforeLogin -= new BeforeLoginEventHandle(_channelPool_BeforeLogin);
        }




        // 登录到 IM 服务器
        void SignIn()
        {

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
            if (strName == null)
                strName = "";
            if (strContent == null)
                strContent = "";

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


        // delegate void Delegate_AppendHtml(string strText);
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
#if NO
            string strUserName = this.MainForm.GetCurrentUserName();
            HubProxy.Invoke("Send", strUserName, this.textBox_input.Text);
            textBox_input.Text = string.Empty;
            textBox_input.Focus();
#endif
        }

        // 书目检索
        private void toolStripButton_searchBiblio_Click(object sender, EventArgs e)
        {
#if NO
            Task<MessageResult> task = HubProxy.Invoke<MessageResult>("RequestSearchBiblio",
                "<全部>",
                "中国",
                "<全部>",
                "left",
                "",
(Int64)100);

            while (task.IsCompleted == false)
            {
                Application.DoEvents();
                Thread.Sleep(200);
            }

            if (task.IsFaulted == true)
            {
                AddErrorLine(GetExceptionText(task.Exception));
                return;
            }

            MessageResult result = task.Result;
            if (result.Value == -1)
            {
                AddErrorLine(result.ErrorInfo);
                return;
            }
            if (result.Value == 0)
            {
                AddErrorLine(result.ErrorInfo);
                return;
            }
            AddMessageLine("search ID:", result.String);

            // 出现对话框

            // DoSearchBiblio();
#endif
        }

#if NO
        public async void DoSearchBiblio()
        {
            MessageResult result = await HubProxy.Invoke<MessageResult>(
                "RequestSearchBiblio",
                "<全部>",
                "中国",
                "<全部>",
                "left",
                100);
            if (result.Value == -1)
            {
                AddErrorLine(result.ErrorInfo);
                return;
            }

            string strSearchID = result.String;
            AddMessageLine("search ID:", result.String);
        }  
#endif
    }


}
