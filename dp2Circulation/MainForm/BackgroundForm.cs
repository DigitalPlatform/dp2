using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
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

        public void AppendHtml(string strText)
        {
            Global.WriteHtml(this.WebBrowser,
                strText);

            // 因为HTML元素总是没有收尾，其他有些方法可能不奏效
            this.WebBrowser.Document.Window.ScrollTo(0,
    this.WebBrowser.Document.Body.ScrollRectangle.Height);
        }
    }
}
