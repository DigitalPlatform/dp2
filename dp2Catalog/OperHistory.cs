using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DigitalPlatform.IO;

namespace dp2Catalog
{
    /// <summary>
    /// 操作历史
    /// </summary>
    public class OperHistory
    {
        /// <summary>
        /// IE 浏览器控件，用于显示操作历史信息
        /// </summary>
        public WebBrowser WebBrowser = null;

        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;


        /// <summary>
        /// 初始化 OperHistory 对象
        /// 初始化过程中，要编译出纳打印方案脚本代码，使它处于就绪状态
        /// </summary>
        /// <param name="main_form">框架窗口</param>
        /// <param name="webbrowser">用于显示操作历史信息的 IE 浏览器控件</param>
        /// <param name="strError">出错信息</param>
        /// <returns>-1: 出错，错误信息在 strError中；0: 成功</returns>
        public int Initial(MainForm main_form,
            WebBrowser webbrowser,
            out string strError)
        {
            //int nRet = 0;
            strError = "";

            this.MainForm = main_form;

            this.WebBrowser = webbrowser;

            // string strCssUrl = this.MainForm.LibraryServerDir + "/history.css";
            string strCssUrl = PathUtil.MergePath(this.MainForm.DataDir, "/history.css");

            string strLink = "<link href='" + strCssUrl + "' type='text/css' rel='stylesheet' />";

            string strJs = "";

            Global.WriteHtml(this.WebBrowser,
                "<html><head>" + strLink + strJs + "</head><body>");
            return 0;
        }

        /// <summary>
        /// 向 IE 控件中追加一段 HTML 内容
        /// </summary>
        /// <param name="strText">HTML 内容</param>
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
