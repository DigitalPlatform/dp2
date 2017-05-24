using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;

namespace dp2Circulation
{
    /// <summary>
    /// 用于查看 XML 内容的窗口
    /// </summary>
    public partial class XmlViewerForm : Form
    {
        /// <summary>
        /// 框架窗口
        /// </summary>
        // public MainForm MainForm = null;

        string m_strXmlString = "";

        // string m_strTempFileName = "";

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
                this.SetXmlToWebbrowser(this.webBrowser1, 
                    m_strXmlString);
            }

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        public XmlViewerForm()
        {
            InitializeComponent();
        }

        void SetXmlToWebbrowser(WebBrowser webbrowser,
             string strXml)
        {
            string strTargetFileName = Path.Combine(Program.MainForm.DataDir, "xml.xml");

            using (StreamWriter sw = new StreamWriter(strTargetFileName,
                false,	// append
                System.Text.Encoding.UTF8))
            {
                sw.Write(strXml);
            }

            webbrowser.Navigate(strTargetFileName);
        }

        private void XmlViewerForm_Activated(object sender, EventArgs e)
        {
            // Program.MainForm.stopManager.Active(this.stop);
        }

        private void XmlViewerForm_Load(object sender, EventArgs e)
        {
            if (Program.MainForm != null)
            {
                MainForm.SetControlFont(this, Program.MainForm.DefaultFont);
            }
        }
    }
}