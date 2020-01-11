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
using DigitalPlatform.dp2.Statis;
using System.IO;

namespace TestReporting
{
    /// <summary>
    /// 通用属性查看窗口
    /// </summary>
    public partial class ReportViewerForm : Form
    {
        public string DataDir { get; set; }

        /// <summary>
        /// 是否已经停靠
        /// </summary>
        public bool Docked = false;

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
                    this.webBrowser_html.SetHtmlString(
                        value,
                        this.DataDir,
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

                /*
                Global.SetXmlString(this.webBrowser_xml,  // ()
                    value,
                    Program.MainForm.DataDir,
                    "comment_viewer_xml");
                 * */
                this.webBrowser_xml.SetXmlToWebbrowser(
    this.DataDir,
    "comment_viewer_xml",
    value
    );

            }
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public ReportViewerForm()
        {
            InitializeComponent();
        }

        string _xmlFileName = "";

        public void SetXmlFile(string filename)
        {
            _xmlFileName = filename;
            WebBrowserExtension.NavigateTo(webBrowser_xml, filename);
        }

        public void SetHtmlFile(string filename)
        {
            WebBrowserExtension.NavigateTo(webBrowser_html, filename);
        }
#if NO
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


#endif
        List<Control> _freeControls = new List<Control>();

        void DisposeFreeControls()
        {
            ControlExtention.DisposeFreeControls(_freeControls);
        }

        private void toolStripButton_exportExcel_Click(object sender, EventArgs e)
        {
            string strExcelFileName = Path.Combine(this.DataDir, "test.xlsx");
            int nRet = Report.RmlToExcel(_xmlFileName,
                strExcelFileName,
                out string strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

            try
            {
                Process.Start(strExcelFileName);
            }
            catch
            {
                MessageBox.Show(this, $"Excel 文件 {strExcelFileName} 已经创建好了");
            }
        }
    }
}
