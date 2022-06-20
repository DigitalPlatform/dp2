using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.IO;

using System.Xml;

using System.Threading;
using System.Resources;
using System.Globalization;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.IO;

//using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Text;

namespace DigitalPlatform.OPAC.Web
{
    /// <summary>
    /// 表示单个实体记录的控件
    /// </summary>
    [ToolboxData("<{0}:ItemControl runat=server></{0}:ItemControl>")]
    public class ItemControl : WebControl, INamingContainer
    {
        public string ItemRecPath = "";

        public bool Wrapper = false;

        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Web.res.ItemControl.cs",
                typeof(ItemControl).Module.Assembly);

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

        protected override void CreateChildControls()
        {
            LiteralControl literal = null;

            if (this.Wrapper == true)
            {
                literal = new LiteralControl();
                literal.Text = this.GetPrefixString(
                    this.GetString("册信息"),
                    "item_wrapper");
                this.Controls.Add(literal);
            }

            this.Controls.Add(new LiteralControl(
                "<table class='iteminfo'>"
                ));

            // 册条码号
            this.Controls.Add(new LiteralControl("<tr class='itembarcode'><td class='name'>"
                + this.GetString("册条码号")
                + "</td><td class='value'>"));
            literal = new LiteralControl();
            literal.ID = "itembarcode";
            this.Controls.Add(literal);

            LiteralControl recpath = new LiteralControl();
            recpath.ID = "recpath";
            this.Controls.Add(recpath);

            this.Controls.Add(new LiteralControl("</td></tr>"));

            // 状态
            this.Controls.Add(new LiteralControl("<tr class='state'><td class='name'>"
                + this.GetString("状态")
                + "</td><td class='value'>"));
            literal = new LiteralControl();
            literal.ID = "state";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));

            // 馆藏地点
            this.Controls.Add(new LiteralControl("<tr class='location'><td class='name'>"
                + this.GetString("馆藏地点")
                + "</td><td class='value'>"));
            literal = new LiteralControl();
            literal.ID = "location";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));

            // 册价格
            this.Controls.Add(new LiteralControl("<tr class='price'><td class='name'>"
                + this.GetString("册价格")
                + "</td><td class='value'>"));
            literal = new LiteralControl();
            literal.ID = "price";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));


            // 出版时间
            this.Controls.Add(new LiteralControl("<tr class='publishtime'><td class='name'>"
                + this.GetString("出版时间")
                + "</td><td class='value'>"));
            literal = new LiteralControl();
            literal.ID = "publishtime";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));

            // 渠道
            this.Controls.Add(new LiteralControl("<tr class='seller'><td class='name'>"
                + this.GetString("渠道")
                + "</td><td class='value'>"));
            literal = new LiteralControl();
            literal.ID = "seller";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));

            // 经费来源
            this.Controls.Add(new LiteralControl("<tr class='source'><td class='name'>"
                + this.GetString("经费来源")
                + "</td><td class='value'>"));
            literal = new LiteralControl();
            literal.ID = "source";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));

            // 索取号
            this.Controls.Add(new LiteralControl("<tr class='callnumber'><td class='name'>"
                + this.GetString("索取号")
                + "</td><td class='value'>"));
            literal = new LiteralControl();
            literal.ID = "callnumber";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));

            // 卷
            this.Controls.Add(new LiteralControl("<tr class='volume'><td class='name'>"
                + this.GetString("卷")
                + "</td><td class='value'>"));
            literal = new LiteralControl();
            literal.ID = "volume";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));

            // 册类型
            this.Controls.Add(new LiteralControl("<tr class='booktype'><td class='name'>"
                + this.GetString("册类型")
                + "</td><td class='value'>"));
            literal = new LiteralControl();
            literal.ID = "booktype";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));


            // 登录号
            this.Controls.Add(new LiteralControl("<tr class='registerno'><td class='name'>"
                + this.GetString("登录号")
                + "</td><td class='value'>"));
            literal = new LiteralControl();
            literal.ID = "registerno";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));

            // 注释
            this.Controls.Add(new LiteralControl("<tr class='comment'><td class='name'>"
                + this.GetString("注释")
                + "</td><td class='value'>"));
            literal = new LiteralControl();
            literal.ID = "comment";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));


            // 批次号
            this.Controls.Add(new LiteralControl("<tr class='batchno'><td class='name'>"
                + this.GetString("批次号")
                + "</td><td class='value'>"));
            literal = new LiteralControl();
            literal.ID = "batchno";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));

            // 借阅情况
            // 借者 + 借阅日期 + 借阅期限
            this.Controls.Add(new LiteralControl("<tr class='borrower'><td class='name'>"
                + this.GetString("借阅情况")
                + "</td><td class='value'>"));
            literal = new LiteralControl();
            literal.ID = "borrower";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));


            // 参考ID
            this.Controls.Add(new LiteralControl("<tr class='refid'><td class='name'>"
                + this.GetString("参考ID")
                + "</td><td class='value'>"));
            literal = new LiteralControl();
            literal.ID = "refid";
            this.Controls.Add(literal);

            this.Controls.Add(new LiteralControl("</td></tr>"));

            // 借阅历史
            PlaceHolder history = new PlaceHolder();
            history.ID = "history";
            this.Controls.Add(history);

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

            this.Controls.Add(new LiteralControl("</table>"));

            if (this.Wrapper == true)
                this.Controls.Add(new LiteralControl(this.GetPostfixString()));
        }

        void SetValue(XmlDocument dom,
            string strElementName,
            string strControlName = null)
        {
            if (String.IsNullOrEmpty(strControlName) == true)
                strControlName = strElementName;

            string strValue = DomUtil.GetElementText(dom.DocumentElement,
                    strElementName);
            LiteralControl text = (LiteralControl)this.FindControl(strControlName);
            text.Text = strValue;
        }

        bool m_bLoaded = false;

        // 提前获得记录体，然后可以获得parentid
        // return:
        //      -1  出错
        //      0   本册已经隐藏显示
        //      1   成功
        public int LoadRecord(string strItemRecPath,
            out string strParentID,
            out string strError)
        {
            int nRet = 0;
            strError = "";
            strParentID = "";

            this.EnsureChildControls();

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            string strXml = "";
            // LibraryChannel channel = sessioninfo.Channel;
            LibraryChannel channel = sessioninfo.GetChannel(true);
            try
            {
                string strBiblio = "";
                string strBiblioRecPath = "";

                byte[] timestamp = null;
                string strOutputPath = "";
                long lRet = // sessioninfo.Channel.
                    channel.GetItemInfo(
                null,
                "@path:" + strItemRecPath,
                "xml", // strResultType
                out strXml,
                out strOutputPath,
                out timestamp,
                "", // "recpath",  // strBiblioType
                out strBiblio,
                out strBiblioRecPath,
                out strError);
                if (lRet == -1)
                    goto ERROR1;
            }
            finally
            {
                sessioninfo.ReturnChannel(channel);
            }

            XmlDocument itemdom = null;
            nRet = OpacApplication.LoadToDom(strXml,
                out itemdom,
                out strError);
            if (nRet == -1)
            {
                // text-level: 内部错误
                strError = "装载册记录进入XML DOM时发生错误: " + strError;
                goto ERROR1;
            }

            strParentID = DomUtil.GetElementText(itemdom.DocumentElement,
                "parent");

            // 册条码号
            SetValue(itemdom, "barcode", "itembarcode");

            // 右上角记录路径
            string strUrl = "./book.aspx?ItemRecPath=" + HttpUtility.UrlEncode(strItemRecPath) + "#active";
            LiteralControl recpath = (LiteralControl)this.FindControl("recpath");
            recpath.Text = "<div class='recpath'><a href='" + strUrl + "' target='_blank' title='" + this.GetString("记录路径") + "'>" + strItemRecPath + "</a></div>";

            // 状态
            SetValue(itemdom, "state");

            // 馆藏地点
            SetValue(itemdom, "location");

            // 册价格
            SetValue(itemdom, "price");

            // 出版时间
            SetValue(itemdom, "publishTime", "publishtime");

            // 渠道
            SetValue(itemdom, "seller");

            // 经费来源
            SetValue(itemdom, "source");

            // 索取号
            SetValue(itemdom, "accessNo", "callnumber");

            // 卷
            SetValue(itemdom, "volume");

            // 册类型
            SetValue(itemdom, "bookType", "booktype");

            // 登录号
            SetValue(itemdom, "registerNo", "registerno");

            // 注释
            SetValue(itemdom, "comment");

            // 批次号
            SetValue(itemdom, "batchNo", "batchno");

            LoginState loginstate = GlobalUtil.GetLoginState(this.Page);

            // 2022/6/17
            bool hidden = false;
            string strState = DomUtil.GetElementText(itemdom.DocumentElement, "state");
            if (StringUtil.IsInList("内部", strState)
&& loginstate != LoginState.Librarian)
            {
                // this.Visible = false;
                hidden = true;
            }

            // 借者
            string strBorrower = DomUtil.GetElementText(itemdom.DocumentElement,
                            "borrower");
            if (String.IsNullOrEmpty(strBorrower) == false)
            {
                string strBorrowDate = DomUtil.GetElementText(itemdom.DocumentElement,
                    "borrowDate");
                strBorrowDate = DateTimeUtil.LocalDate(strBorrowDate);
                string strBorrowPeriod = DomUtil.GetElementText(itemdom.DocumentElement,
                    "borrowPeriod");
                strBorrowPeriod = app.GetDisplayTimePeriodStringEx(strBorrowPeriod);

                string strBorrowerText = "";
                if (loginstate == LoginState.Librarian)
                    strBorrowerText = "<a href='./readerinfo.aspx?barcode=" + strBorrower + "' target='_blank'>" + strBorrower + "</a>";
                else if (loginstate == LoginState.Reader && sessioninfo.ReaderInfo.Barcode == strBorrower)
                    strBorrowerText = strBorrower + "(" + this.GetString("我自己") + ")";
                else
                    strBorrowerText = new string('*', strBorrower.Length);

                LiteralControl text = (LiteralControl)this.FindControl("borrower");
                text.Text = this.GetString("借阅者") + ": " + strBorrowerText + "  " + this.GetString("借阅日期") + ":" + strBorrowDate + "  " + this.GetString("借阅期限") + ":" + strBorrowPeriod;
            }

            // 参考ID
            SetValue(itemdom, "refID", "refid");

            this.ItemRecPath = strItemRecPath;
            this.m_bLoaded = true;
            if (hidden)
                return 0;
            return 1;
        ERROR1:
            return -1;
        }

        protected override void Render(HtmlTextWriter output)
        {
            int nRet = 0;
            string strError = "";

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            if (m_bLoaded == false)
            {
                // return:
                //      -1  出错
                //      0   本册已经隐藏显示
                //      1   成功
                nRet = LoadRecord(this.ItemRecPath,
                    out string strParentID,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 0)
                    this.Visible = false;
            }

            base.Render(output);
            return;
        ERROR1:
            this.SetDebugInfo("errorinfo", strError);
        }

        void SetDebugInfo(string strText)
        {
            PlaceHolder line = (PlaceHolder)FindControl("debugline");
            line.Visible = true;

            LiteralControl text = (LiteralControl)this.FindControl("debugtext");

            text.Text = strText;
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

        public string GetPrefixString(string strTitle,
string strWrapperClass)
        {
            if (String.IsNullOrEmpty(strWrapperClass) == true)
                strWrapperClass = "content_wrapper";

            return "<div class='" + strWrapperClass + "'>"
                + "<table class='roundbar' cellpadding='0' cellspacing='0'>"
                + "<tr class='titlebar'>"
                + "<td class='left'></td>"
                + "<td class='middle'>" + strTitle + "</td>"
                + "<td class='right'></td>"
                + "</tr>"
                + "</table>";
        }

        public string GetPostfixString()
        {
            return "</div>";
        }
    }
}

