using DigitalPlatform;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Web;
using System.Windows.Forms;

namespace dp2Circulation
{
    /// <summary>
    /// MDI Client 上面用于显示文字的窗口
    /// 通过一个 IE 浏览器控件实现显示
    /// </summary>
    internal partial class BackgroundForm : Form
    {
        public BackgroundForm()
        {
            InitializeComponent();
        }

        public WebBrowser WebBrowser
        {
            get
            {
                return this.webBrowser1;
            }
        }

        // 线程安全
        public void AppendHtml(string strText)
        {
            // 当窗口 Close() 掉以后, Created 会变成 false
            if (this.Created == false)
                return;

            this.TryInvoke(() =>
            {
                Global.WriteHtml(this.WebBrowser, strText);

                // 因为HTML元素总是没有收尾，其他有些方法可能不奏效
                this.WebBrowser.Document.Window.ScrollTo(0,
        this.WebBrowser.Document.Body.ScrollRectangle.Height);
            });
        }

        // 线程安全
        public void ShowProgressMessage(string strID,
            string strText)
        {
            try
            {
                if (this.webBrowser1 == null || this.webBrowser1.IsDisposed == true)
                    return;

                if (this.webBrowser1.InvokeRequired)
                {
                    this.webBrowser1.Invoke(new Action<string, string>(ShowProgressMessage), strID, strText);
                    return;
                }

                if (webBrowser1.Document == null)
                    return;


                HtmlElement obj = this.webBrowser1.Document.GetElementById(strID);
                if (obj != null)
                {
                    obj.InnerText = strText;
                    return;
                }

                AppendHtml("<div id='" + strID + "'>" + HttpUtility.HtmlEncode(strText) + "</div>");
            }
            catch (ObjectDisposedException)  // 2024/6/24
            {

            }
        }

        private void BackgroundForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private void BackgroundForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            /*
            if (e.CloseReason == CloseReason.UserClosing)   // 2014/8/13
                e.Cancel = true;
            */
        }

        private void BackgroundForm_Activated(object sender, EventArgs e)
        {
            this.SendToBack();
        }
    }
}
