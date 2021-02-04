using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;

using DigitalPlatform;
using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    /// <summary>
    /// 通用属性查看窗口
    /// </summary>
    public partial class CommentViewerForm : Form
    {
        WebExternalHost m_webExternalHost = null;

        /// <summary>
        /// 是否已经停靠
        /// </summary>
        public bool Docked = false;

        /// <summary>
        /// 框架窗口
        /// </summary>
        // public MainForm MainForm = null;

        // 为了让脚本代码能够兼容
        public virtual MainForm MainForm
        {
            get
            {
                return Program.MainForm;
            }
            set
            {
                // 为了让脚本代码能兼容
            }
        }

        string m_strHtmlString = "";

        /// <summary>
        /// HTML 字符串
        /// </summary>
        public string HtmlString
        {
            get
            {
                return m_strHtmlString;
            }
            set
            {
                lock (this)
                {
                    this.webBrowser_html.Stop();
                    m_strHtmlString = value;
                    Debug.Assert(Program.MainForm != null, "");

#if NO
                    Global.SetHtmlString(this.webBrowser_html,
                        value,
                        Program.MainForm.DataDir,
                        "comment_viewer_html");
#endif
                    this.m_webExternalHost.SetHtmlString(value,
                        "comment_viewer_html");
                }
            }
        }

        string m_strXmlString = "";

        /// <summary>
        /// XML 字符串
        /// </summary>
        public string XmlString
        {
            get
            {
                return m_strXmlString;
            }
            set
            {
                m_strXmlString = value;

                Debug.Assert(Program.MainForm != null, "");
                /*
                Global.SetXmlString(this.webBrowser_xml,  // ()
                    value,
                    Program.MainForm.DataDir,
                    "comment_viewer_xml");
                 * */
                Global.SetXmlToWebbrowser(this.webBrowser_xml,
    Program.MainForm.DataDir,
    "comment_viewer_xml",
    value
    );

            }
        }


        string m_strSubrecordsString = "";

        /// <summary>
        /// 子记录字符串
        /// </summary>
        public string SubrecordsString
        {
            get
            {
                return m_strSubrecordsString;
            }
            set
            {
                lock (this)
                {
                    m_strSubrecordsString = value;
                    Debug.Assert(Program.MainForm != null, "");

                    Global.SetHtmlString(this.webBrowser_subrecords,
    value,
    Program.MainForm.DataDir,
    "comment_viewer_subrecords");
                }
            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public CommentViewerForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 初始化浏览器控件
        /// </summary>
        public void InitialWebBrowser()
        {
            this.m_webExternalHost = new WebExternalHost();
            this.m_webExternalHost.IsBelongToHoverWindow = true;    // 表示自己就是hover窗口

            // webbrowser
            this.m_webExternalHost.Initial(// Program.MainForm, 
                this.webBrowser_html);
            this.webBrowser_html.ObjectForScripting = this.m_webExternalHost;
        }

        /// <summary>
        /// 退出浏览器控件
        /// </summary>
        public void ExitWebBrowser()
        {
            if (this.m_webExternalHost != null)
            {
                this.m_webExternalHost.Destroy();
                this.m_webExternalHost.Dispose();
                this.m_webExternalHost = null;
            }
        }

        /// <summary>
        /// 停止先前命令
        /// </summary>
        public void StopPrevious()
        {
            if (this.m_webExternalHost != null)
                this.m_webExternalHost.StopPrevious();

            this.webBrowser_html.Stop();
        }

        /// <summary>
        /// 输出 HTML 字符串
        /// </summary>
        /// <param name="strHtml">HTML 字符串</param>
        public void WriteHtml(string strHtml)
        {
            Global.WriteHtml(this.webBrowser_html,
                strHtml);
        }

        /// <summary>
        /// 输出 XML 字符串
        /// </summary>
        /// <param name="strXml">XML 字符串</param>
        public void WriteXml(string strXml)
        {
            Global.WriteHtml(this.webBrowser_xml,
                strXml);
        }

        private void toolStripButton_dock_Click(object sender, EventArgs e)
        {
            // this.FormBorderStyle = FormBorderStyle.None;

            DoDock(true);

            Program.MainForm.ActivatePropertyPage();
        }

        /// <summary>
        /// 负责显示 HTML 的浏览器控件
        /// </summary>
        public WebBrowser HtmlBrowserControl
        {
            get
            {
                return this.webBrowser_html;
            }
        }

        /// <summary>
        /// 负责显示 XML 的浏览器控件
        /// </summary>
        public WebBrowser XmlBrowserControl
        {
            get
            {
                return this.webBrowser_xml;
            }
        }

        public WebBrowser SubrecordsBrowserControl
        {
            get
            {
                return this.webBrowser_subrecords;
            }
        }

        List<Control> _freeControls = new List<Control>();

        /// <summary>
        /// 进行停靠
        /// </summary>
        /// <param name="bShowFixedPanel">是否同时促成显示固定面板</param>
        public void DoDock(bool bShowFixedPanel)
        {
            // return; // 测试内存泄漏
            if (Program.MainForm.CurrentPropertyControl != this.tabControl_main)
            {
                Program.MainForm.CurrentPropertyControl = this.tabControl_main;
                // 防止内存泄漏
                ControlExtention.AddFreeControl(_freeControls, this.tabControl_main);
            }

            if (bShowFixedPanel == true
                && Program.MainForm.PanelFixedVisible == false)
                Program.MainForm.PanelFixedVisible = true;

            this.Docked = true;
            this.Visible = false;
        }

        /// <summary>
        /// TabControl
        /// </summary>
        public Control MainControl
        {
            get
            {
                return this.tabControl_main;
            }
        }

        /// <summary>
        /// 清除当前全部内容
        /// </summary>
        public void Clear()
        {
            Global.ClearHtmlPage(this.webBrowser_html,
                Program.MainForm != null ? Program.MainForm.DataDir : null);
            Global.ClearHtmlPage(this.webBrowser_xml,
                Program.MainForm != null ? Program.MainForm.DataDir : null);
            Global.ClearHtmlPage(this.webBrowser_subrecords,
                Program.MainForm != null ? Program.MainForm.DataDir : null);
        }

        /// <summary>
        /// 清除 HTML 内容
        /// </summary>
        public void ClearHtml()
        {
            Global.ClearHtmlPage(this.webBrowser_html,
Program.MainForm != null ? Program.MainForm.DataDir : null);
        }

        /// <summary>
        /// 清除 XML 内容
        /// </summary>
        public void ClearXml()
        {
            Global.ClearHtmlPage(this.webBrowser_xml,
Program.MainForm != null ? Program.MainForm.DataDir : null);
        }

        public void ClearSubrecords()
        {
            Global.ClearHtmlPage(this.webBrowser_subrecords,
Program.MainForm != null ? Program.MainForm.DataDir : null);
        }

        bool m_bSuppressScriptErrors = false;

        /// <summary>
        /// 是否不显示浏览器脚本错误
        /// </summary>
        public bool SuppressScriptErrors
        {
            get
            {
                return this.m_bSuppressScriptErrors;
            }
            set
            {
                this.m_bSuppressScriptErrors = value;
            }
        }

        private void webBrowser_html_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            ((WebBrowser)sender).Document.Window.Error += new HtmlElementErrorEventHandler(Window_Error);
        }

        void Window_Error(object sender, HtmlElementErrorEventArgs e)
        {
            if (this.m_bSuppressScriptErrors == true)
                e.Handled = true;
        }

        void DisposeFreeControls()
        {
            // 2015/11/7
            if (this.tabControl_main != null && Program.MainForm != null)
            {
                // 如果当前固定面板拥有 tabcontrol，则要先解除它的拥有关系，否则怕本 Form 摧毁的时候无法 Dispose() 它
                if (Program.MainForm.CurrentPropertyControl == this.tabControl_main)
                    Program.MainForm.CurrentPropertyControl = null;
            }

            ControlExtention.DisposeFreeControls(_freeControls);
        }

        private void webBrowser_subrecords_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            ((WebBrowser)sender).Document.Window.Error += new HtmlElementErrorEventHandler(Window_Error);
        }
    }
}
