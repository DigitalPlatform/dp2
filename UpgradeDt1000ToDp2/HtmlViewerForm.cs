using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace UpgradeDt1000ToDp2
{
    public partial class HtmlViewerForm : Form
    {
        string m_strHtmlString = "";

        public string HtmlString
        {
            get
            {
                return m_strHtmlString;
            }
            set
            {
                m_strHtmlString = value;
                Global.SetHtmlString(this.webBrowser1, value);
            }

        }

        public HtmlViewerForm()
        {
            InitializeComponent();
        }

        public void WriteHtml(string strHtml)
        {
            Global.WriteHtml(this.webBrowser1,
                strHtml);
        }

    }
}