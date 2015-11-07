using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using DigitalPlatform.CommonControl;

namespace dp2Catalog
{
    public partial class BiblioViewerForm : Form
    {
        public bool Docked = false;
        public MainForm MainForm = null;

        string m_strHtmlString = "";

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
                    // Global.SetHtmlString(this.webBrowser1, value);
                    Debug.Assert(this.MainForm != null, "");

                    if (string.IsNullOrEmpty(value) == true)
                        Global.ClearHtmlPage(this.webBrowser_html,
    this.MainForm != null ? this.MainForm.DataDir : null);
                    else
                        Global.SetHtmlString(this.webBrowser_html,
                            value,
                            this.MainForm.DataDir,
                            "comment_viewer_html");
                }
            }
        }

        string m_strMarcString = "";

        public string MarcString
        {
            get
            {
                return m_strMarcString;
            }
            set
            {
                lock (this)
                {
                    this.webBrowser_marc.Stop();
                    m_strMarcString = value;
                    // Global.SetHtmlString(this.webBrowser1, value);
                    Debug.Assert(this.MainForm != null, "");

                    if (string.IsNullOrEmpty(value) == true)
                        Global.ClearHtmlPage(this.webBrowser_marc,
    this.MainForm != null ? this.MainForm.DataDir : null);
                    else
                    Global.SetHtmlString(this.webBrowser_marc,
                        value,
                        this.MainForm.DataDir,
                        "comment_viewer_marc");
                }
            }
        }


        string m_strXmlString = "";

        public string XmlString
        {
            get
            {
                return m_strXmlString;
            }
            set
            {
                m_strXmlString = value;
                // Global.SetHtmlString(this.webBrowser1, value);
                Debug.Assert(this.MainForm != null, "");
                /*
                Global.SetXmlString(this.webBrowser_xml,  // ()
                    value,
                    this.MainForm.DataDir,
                    "comment_viewer_xml");
                 * */
                if (string.IsNullOrEmpty(value) == true)
                    Global.ClearHtmlPage(this.webBrowser_xml,
this.MainForm != null ? this.MainForm.DataDir : null);
                else 
                    Global.SetXmlToWebbrowser(this.webBrowser_xml,
    this.MainForm.DataDir,
    "comment_viewer_xml",
    value
    );

            }
        }

        public BiblioViewerForm()
        {
            InitializeComponent();
        }

        public void WriteHtml(string strHtml)
        {
            Global.WriteHtml(this.webBrowser_html,
                strHtml);
        }

        public void WriteXml(string strXml)
        {
            Global.WriteHtml(this.webBrowser_xml,
                strXml);
        }

        private void toolStripButton_dock_Click(object sender, EventArgs e)
        {
            DoDock(true);

            this.MainForm.ActivatePropertyPage();

        }

        public WebBrowser HtmlBrowserControl
        {
            get
            {
                return this.webBrowser_html;
            }
        }


        public WebBrowser MarcBrowserControl
        {
            get
            {
                return this.webBrowser_marc;
            }
        }

        public WebBrowser XmlBrowserControl
        {
            get
            {
                return this.webBrowser_xml;
            }
        }

        List<Control> _freeControls = new List<Control>();

        public void DoDock(bool bShowFixedPanel)
        {
            // return; // 测试内存泄漏

            if (this.MainForm.CurrentPropertyControl != this.tabControl_main)
                this.MainForm.CurrentPropertyControl = this.tabControl_main;

            // 防止内存泄漏
            ControlExtention.AddFreeControl(_freeControls, this.tabControl_main);

            if (bShowFixedPanel == true
                && this.MainForm.PanelFixedVisible == false)
                this.MainForm.PanelFixedVisible = true;

            this.Docked = true;
            this.Visible = false;
        }

        public Control MainControl
        {
            get
            {
                return this.tabControl_main;
            }
        }

        public void Clear()
        {
            Global.ClearHtmlPage(this.webBrowser_html,
                this.MainForm != null ? this.MainForm.DataDir : null);
            Global.ClearHtmlPage(this.webBrowser_marc,
                this.MainForm != null ? this.MainForm.DataDir : null);
            Global.ClearHtmlPage(this.webBrowser_xml,
                this.MainForm != null ? this.MainForm.DataDir : null);
        }

        public void ClearHtml()
        {
            Global.ClearHtmlPage(this.webBrowser_html,
this.MainForm != null ? this.MainForm.DataDir : null);
        }

        public void ClearMarc()
        {
            Global.ClearHtmlPage(this.webBrowser_marc,
this.MainForm != null ? this.MainForm.DataDir : null);
        }

        public void ClearXml()
        {
            Global.ClearHtmlPage(this.webBrowser_xml,
this.MainForm != null ? this.MainForm.DataDir : null);
        }

        bool m_bSuppressScriptErrors = false;
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
            if (this.tabControl_main != null && this.MainForm != null)
            {
                // 如果当前固定面板拥有 tabControl_main，则要先解除它的拥有关系，否则怕本 Form 摧毁的时候无法 Dispose() 它
                if (this.MainForm.CurrentPropertyControl == this.tabControl_main)
                    this.MainForm.CurrentPropertyControl = null;
            }

            ControlExtention.DisposeFreeControls(_freeControls);
        }

    }
}
