using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Xml;
using System.IO;

using System.Threading;
using System.Resources;
using System.Globalization;

using DigitalPlatform.Xml;
using DigitalPlatform.OPAC.Server;

namespace DigitalPlatform.OPAC.Web
{
    /// <summary>
    /// 个性化设置 控件
    /// </summary>
    [DefaultProperty("Text")]
    [ToolboxData("<{0}:PreferenceControl runat=server></{0}:PreferenceControl>")]
    public class PreferenceControl : ReaderInfoBase
    {
        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Web.res.PreferenceControl.cs",
                typeof(PreferenceControl).Module.Assembly);

            return this.m_rm;
        }

        public string GetString(string strID)
        {
            CultureInfo ci = new CultureInfo(Thread.CurrentThread.CurrentUICulture.Name);

            // TODO: 如果抛出异常，则要试着取zh-cn的字符串，或者返回一个报错的字符串
            try
            {

                string s = GetRm().GetString(strID, ci);
                if (String.IsNullOrEmpty(s) == true)
                    return strID;
                return s;
            }
            catch (Exception /*ex*/)
            {
                return strID + " 在 " + Thread.CurrentThread.CurrentUICulture.Name + " 中没有找到对应的资源。";
            }
        }


        /*
        List<string> GetStyleDirs()
        {
            List<string> result = new List<string>();
            string strStyleRoot = this.Page.Server.MapPath("./style");

            DirectoryInfo root = new DirectoryInfo(strStyleRoot);
            DirectoryInfo[] dis = root.GetDirectories();
            for (int i = 0; i < dis.Length; i++)
            {
                result.Add(dis[i].Name);
            }

            return result;
        }

        // 风格个数
        public int StyleLineCount
        {
            get
            {
                List<string> dirs = GetStyleDirs();
                return dirs.Count;
            }
        }*/

        // 布局控件
        protected override void CreateChildControls()
        {
            LiteralControl titleline = new LiteralControl();
            titleline.ID = "preference_titleline";
            titleline.Text = "<div class='content_wrapper'>";    // cellpadding='0' cellspacing='0' border='0' width='100%'
            titleline.Text += "<table class='roundbar' cellpadding='0' cellspacing='0'>";    // cellpadding='0' cellspacing='0' border='0' width='100%'
            titleline.Text += "<tr class='titlebar'>"
                + "<td class='left'></td>"
                + "<td class='middle'>"
                + this.GetString("个性化设置")
                + "</td>"
                + "<td class='right'></td>"
                + "</tr>";

            titleline.Text += "</table>";
            titleline.Text += "<table class='preference'>";

            titleline.Text += "<tr class='skin'><td class='name' nowrap>"
                + this.GetString("外观风格")
                + "</td>";
            this.Controls.Add(titleline);

            this.Controls.Add(new LiteralControl("<td class='value'>"));

            // 内容
            CreateStyleLines();

            this.Controls.Add(new LiteralControl("</td></tr>"));

            // 个性化标题文字
            LiteralControl literal = new LiteralControl();
            literal.Text = "<tr class='title'><td class='name' nowrap>"
                + this.GetString("个性化标题文字")
                + "</td>";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("<td class='value'>"));
            CreateTitleText();
            this.Controls.Add(new LiteralControl("</td></tr>"));


            // 调试信息行
            PlaceHolder debugline = new PlaceHolder();
            debugline.ID = "debugline";
            this.Controls.Add(debugline);

            literal = new LiteralControl();
            literal.Text = "<tr class='debugline'><td colspan='2'>";
            debugline.Controls.Add(literal);

            literal = new LiteralControl();
            literal.ID = "debugtext";
            literal.Text = "";
            debugline.Controls.Add(literal);


            literal = new LiteralControl();
            literal.Text = "</td></tr>";
            debugline.Controls.Add(literal);

            debugline = null;

            // 命令行
            this.Controls.Add(new LiteralControl("<tr class='cmdline'><td colspan='2'>"));

            // 
            // 提交按钮
            Button submitTitleText = new Button();
            submitTitleText.ID = "submit_title_text";
            submitTitleText.Text = this.GetString("提交");
            submitTitleText.CssClass = "submit_title_text";
            submitTitleText.Click += new EventHandler(submitTitleText_Click);
            this.Controls.Add(submitTitleText);
            submitTitleText = null;

            this.Controls.Add(new LiteralControl("</td></tr>"));

            literal = new LiteralControl();
            literal.ID = "preference_tableend";
            literal.Text = "</table>";
            this.Controls.Add(literal);


            literal = new LiteralControl();
            literal.Text = "</div>";
            this.Controls.Add(literal);
        }

        void CreateTitleText()
        {
            string strError = "";

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            if (app == null)
            {
                strError = "app == null";
                goto ERROR1;
            }
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];
            if (sessioninfo == null)
            {
                strError = "sessioninfo == null";
                goto ERROR1;
            }

            XmlDocument readerdom = null;
            // 获得当前session中已经登录的读者记录DOM
            // return:
            //      -2  当前登录的用户不是reader类型
            //      -1  出错
            //      0   尚未登录
            //      1   成功
            int nRet = sessioninfo.GetLoginReaderDom(
                out readerdom,
                out strError);
            if (nRet == -1 || nRet == -2)
                goto ERROR1;

            if (nRet == 0)
            {
                sessioninfo.LoginCallStack.Push(this.Page.Request.RawUrl);
                this.Page.Response.Redirect("login.aspx", true);
                return;
            }


            string strTitleText = GetReaderTitleText(readerdom);

            /*
            // 左侧文字
            LiteralControl literal = new LiteralControl();
            literal.Text = "<tr><td>";
            this.Controls.Add(literal);
            */

            // 编辑控件
            TextBox textbox = new TextBox();
            textbox.ID = "titletext";
            textbox.Text = strTitleText;
            this.Controls.Add(textbox);


            /*
            // 右侧文字
            literal = new LiteralControl();
            literal.Text = "</td></tr>";
            this.Controls.Add(literal);
             * */


            return;
        ERROR1:
            return;
        }

        void submitTitleText_Click(object sender, EventArgs e)
        {
            string strError = "";

            TextBox textbox = (TextBox)this.FindControl("titletext");

            string strTitleText = textbox.Text;

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            if (app == null)
            {
                strError = "app == null";
                goto ERROR1;
            }
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];
            if (sessioninfo == null)
            {
                strError = "sessioninfo == null";
                goto ERROR1;
            }

            int nRedoCount = 0;

        REDO:
            XmlDocument readerdom = null;
            // 获得当前session中已经登录的读者记录DOM
            // return:
            //      -2  当前登录的用户不是reader类型
            //      -1  出错
            //      0   尚未登录
            //      1   成功
            int nRet = sessioninfo.GetLoginReaderDom(
                out readerdom,
                out strError);
            if (nRet == -1 || nRet == -2)
                goto ERROR1;

            if (nRet == 0)
                goto ERROR1;

            this.SetReaderTitleText(ref readerdom,
    strTitleText);
            sessioninfo.SetLoginReaderDomChanged();


            // return:
            //      -2  时间戳冲突
            //      -1  error
            //      0   没有必要保存(changed标志为false)
            //      1   成功保存
            nRet = sessioninfo.SaveLoginReaderDom(
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == -2)
            {
                if (nRedoCount < 10)
                {
                    nRedoCount++;
                    sessioninfo.ReaderInfo.ReaderDom = null;   // 强迫重新读入
                    goto REDO;
                }
                goto ERROR1;
            }

            // this.SetDebugInfo("保存成功。");


            // 刷新显示

            return;
        ERROR1:
            this.SetDebugInfo("errorinfo", strError);
        }

        void SetDebugInfo(string strSpanClass,
    string strText)
        {
            PlaceHolder line = (PlaceHolder)FindControl("debugline");
            line.Visible = true;

            LiteralControl text = (LiteralControl)line.FindControl("debugtext");
            if (strSpanClass == "errorinfo")
                text.Text = "<div class='errorinfo-frame'><div class='" + strSpanClass + "'>" + strText + "</div></div>";
            else
                text.Text = "<div class='" + strSpanClass + "'>" + strText + "</div>";
        }

        public static string GetReaderTitleText(XmlDocument readerdom)
        {
            XmlNode preference = readerdom.DocumentElement.SelectSingleNode("preference");
            if (preference == null)
                return null;

            XmlNode webui = preference.SelectSingleNode("webui");
            if (webui == null)
                return null;

            return DomUtil.GetAttr(webui, "titletext");
        }

        void SetReaderTitleText(ref XmlDocument readerdom,
    string strTitleText)
        {
            XmlNode preference = readerdom.DocumentElement.SelectSingleNode("preference");
            if (preference == null)
            {
                preference = readerdom.CreateElement("preference");
                readerdom.DocumentElement.AppendChild(preference);
            }

            XmlNode webui = preference.SelectSingleNode("webui");
            if (webui == null)
            {
                webui = readerdom.CreateElement("webui");
                preference.AppendChild(webui);
            }

            DomUtil.SetAttr(webui, "titletext", strTitleText);
        }


        public static string GetReaderSelectedStyleDir(XmlDocument readerdom)
        {
            XmlNode preference = readerdom.DocumentElement.SelectSingleNode("preference");
            if (preference == null)
                return "0"; // 缺省值

            XmlNode webui = preference.SelectSingleNode("webui");
            if (webui == null)
                return "0"; // 缺省值

            string strResult = DomUtil.GetAttr(webui, "style");

            if (String.IsNullOrEmpty(strResult) == true)
                return "0";// 缺省值

            return strResult;
        }

        void SetReaderSelectedStyleDir(ref XmlDocument readerdom,
            string strDirName)
        {
            XmlNode preference = readerdom.DocumentElement.SelectSingleNode("preference");
            if (preference == null)
            {
                preference = readerdom.CreateElement("preference");
                readerdom.DocumentElement.AppendChild(preference);
            }

            XmlNode webui = preference.SelectSingleNode("webui");
            if (webui == null)
            {
                webui = readerdom.CreateElement("webui");
                preference.AppendChild(webui);
            }

            DomUtil.SetAttr(webui, "style", strDirName);
        }

        // 选定风格
        // return:
        //      -1  error
        //      0   succeed
        int SelectStyle(string strDirName,
            out string strError)
        {
            strError = "";

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            if (app == null)
            {
                strError = "app == null";
                goto ERROR1;
            }
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];
            if (sessioninfo == null)
            {
                strError = "sessioninfo == null";
                goto ERROR1;
            }

            XmlDocument readerdom = null;
            // 获得当前session中已经登录的读者记录DOM
            // return:
            //      -2  当前登录的用户不是reader类型
            //      -1  出错
            //      0   尚未登录
            //      1   成功
            int nRet = sessioninfo.GetLoginReaderDom(
                out readerdom,
                out strError);
            if (nRet == -1 || nRet == -2)
                goto ERROR1;

            if (nRet == 0)
            {
                return -1;
            }

            SetReaderSelectedStyleDir(ref readerdom,
                strDirName);
            sessioninfo.SetLoginReaderDomChanged();

            return 0;
        ERROR1:
            return -1;
        }

        void CreateStyleLines()
        {
            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            if (app == null)
                return;

            string strStyleRoot = this.Page.Server.MapPath("./style");

            // 左侧文字
            // LiteralControl literal = null;

            DirectoryInfo root = new DirectoryInfo(strStyleRoot);
            if (root.Exists == false)
            {
                strStyleRoot = Path.Combine(app.DataDir, "style");
                root = new DirectoryInfo(strStyleRoot);

                if (root.Exists == false)
                    return;
            }


            DirectoryInfo[] dis = root.GetDirectories();
            Array.Sort(dis, new DirectoryInfoCompare());
            for (int i = 0; i < dis.Length; i++)
            {
                // 2015/1/26
                string strTotalFileName = Path.Combine(dis[i].FullName, "total.jpg");
                if (File.Exists(strTotalFileName) == false)
                    continue;

                string strDirName = dis[i].Name;

                // imagebutton
                ImageButton imagebutton = new ImageButton();

                /*
                if (strReaderStyleDir == strDirName)
                    imagebutton.CssClass = "selected";
                else
                    imagebutton.CssClass = "unselected";
                 * */
                // 这里暂时设置成这样，待Render阶段再把选定的一个设置为"selected"
                imagebutton.CssClass = "unselected";

                imagebutton.ID = strDirName;
                imagebutton.ImageUrl = MyWebPage.GetStylePath(app, strDirName + "/total.jpg");
                imagebutton.Click += new ImageClickEventHandler(imagebutton_Click);
                this.Controls.Add(imagebutton);

            }

            return;
            /*
        ERROR1:
            return;
             * */
        }

        protected override void Render(HtmlTextWriter output)
        {
            string strError = "";
            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            if (app == null)
            {
                strError = "app == null";
                goto ERROR1;
            }
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];
            if (sessioninfo == null)
            {
                strError = "sessioninfo == null";
                goto ERROR1;
            }

            XmlDocument readerdom = null;
            // 获得当前session中已经登录的读者记录DOM
            // return:
            //      -2  当前登录的用户不是reader类型
            //      -1  出错
            //      0   尚未登录
            //      1   成功
            int nRet = sessioninfo.GetLoginReaderDom(
                 out readerdom,
                out strError);
            if (nRet == -1 || nRet == -2)
                goto ERROR1;

            if (nRet == 0)
            {
                sessioninfo.LoginCallStack.Push(this.Page.Request.RawUrl);
                this.Page.Response.Redirect("login.aspx", true);
                return;
            }

            string strReaderStyleDir = GetReaderSelectedStyleDir(readerdom);

            if (String.IsNullOrEmpty(strReaderStyleDir) == true)
                strReaderStyleDir = "0";

            ImageButton imagebutton = (ImageButton)this.FindControl(strReaderStyleDir);

            if (imagebutton != null)
                imagebutton.CssClass = "selected";

            base.Render(output);
            return;

        ERROR1:
            output.Write(strError);
        }

        // 选定一个风格
        void imagebutton_Click(object sender, ImageClickEventArgs e)
        {
            string strError = "";
            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            if (app == null)
            {
                strError = "app == null";
                goto ERROR1;
            }
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];
            if (sessioninfo == null)
            {
                strError = "sessioninfo == null";
                goto ERROR1;
            }

            ImageButton imagebutton = (ImageButton)sender;
            string strDirName = imagebutton.ID;

            int nRedoCount = 0;

        REDO:
            int nRet = SelectStyle(strDirName, out strError);
            if (nRet == -1)
                goto ERROR1;

            // return:
            //      -2  时间戳冲突
            //      -1  error
            //      0   没有必要保存(changed标志为false)
            //      1   成功保存
            nRet = sessioninfo.SaveLoginReaderDom(
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == -2)
            {
                if (nRedoCount < 5)
                {
                    nRedoCount++;
                    sessioninfo.ReaderInfo.ReaderDom = null;   // 强迫重新读入
                    goto REDO;
                }
                goto ERROR1;
            }
            return;
        ERROR1:
            this.SetDebugInfo("errorinfo", strError);
        }

        /*
        protected override void RenderContents(HtmlTextWriter output)
        {
            output.Write(Text);
        }
         * */
    }

    public class DirectoryInfoCompare : IComparer
    {

        // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
        int IComparer.Compare(Object x, Object y)
        {
            return ((new CaseInsensitiveComparer()).Compare(((DirectoryInfo)x).Name, ((DirectoryInfo)y).Name));
        }

    }
}
