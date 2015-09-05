using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace dp2Circulation
{
    /// <summary>
    /// 察看 HTML 显示效果的窗口
    /// </summary>
    public partial class HtmlViewerForm : Form
    {
        string m_strHtmlString = "";

        /// <summary>
        /// 当前 HTML 字符串
        /// </summary>
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

        /// <summary>
        /// 构造函数
        /// </summary>
        public HtmlViewerForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 写入 HTML 字符串
        /// </summary>
        /// <param name="strHtml">HTML 字符串</param>
        public void WriteHtml(string strHtml)
        {
            Global.WriteHtml(this.webBrowser1,
                strHtml);
        }

     }
}