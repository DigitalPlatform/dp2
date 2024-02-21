using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

using System.Xml;

using System.Threading;
using System.Resources;
using System.Globalization;

using DigitalPlatform.Xml;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.IO;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Text;

namespace DigitalPlatform.OPAC.Web
{
    [ToolboxData("<{0}:BorrowHistoryControl runat=server></{0}:BorrowHistoryControl>")]
    public class BorrowHistoryControl : ReaderInfoBase
    {
        /// <summary>
        /// 是否为借还历史库模式。false 表示不是历史库模式，即要从读者记录中获得借阅历史信息；true 表示为历史库模式，要用 dp2library 的 SearchCharging() API 获得历史信息
        /// </summary>
        public bool DatabaseMode
        {
            get;
            set;
        }

        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Web.res.BorrowHistoryControl.cs",
                typeof(BorrowHistoryControl).Module.Assembly);

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
        public int ResultCount
        {
            get
            {
                object o = ViewState[this.ID + "_ResultCount"];
                if (o == null)
                    return 0;
                else
                    return (int)o;
            }

            set
            {
                ViewState[this.ID + "_ResultCount"] = value;
            }
        }
         * */

        public int ResultCount
        {
            get
            {
                string strError = "";
                int nRet = 0;
                if (this.DatabaseMode == false)
                {
                    // return:
                    //      -1  出错
                    //      0   成功
                    //      1   尚未登录
                    nRet = this.LoadReaderXml(out strError);
                    if (nRet == -1)
                        return 0;

                    if (nRet == 1)
                        return 0;

                    XmlNodeList nodes = ReaderDom.DocumentElement.SelectNodes("borrowHistory/borrow");
                    return nodes.Count;
                }

                List<ChargingItemWrapper> results = null;
                // TODO: 这里可能会因为事项太多而超时
                // return:
                //      -2  尚未登录
                //      -1  出错
                //      其它  符合条件的事项总数
                nRet = GetChargingHistory(0,
            -1,
            out results,
            out strError);
                if (nRet < 0)
                    return 0;
                return nRet;
            }
        }

        // 获得历史信息
        // parameters:
        //      nItemsPerPage  每页事项数。如果为 -1，表示仅获得事项总数
        // return:
        //      -2  尚未登录
        //      -1  出错
        //      其它  符合条件的事项总数
        int GetChargingHistory(int nPageNo,
            int nItemsPerPage,
            out List<ChargingItemWrapper> results,
            out string strError)
        {
            strError = "";
            results = null;

            // 获得读者证条码号
            string strReaderBarcode = "";
            {
                // return:
                //      -1  出错
                //      0   成功
                //      1   尚未登录
                int nRet = this.LoadReaderXml(out strError);
                if (nRet == -1)
                    return 0;
                if (nRet == 1)
                    return -2;
                var barcode = DomUtil.GetElementText(ReaderDom.DocumentElement, "barcode");
                var refid = DomUtil.GetElementText(ReaderDom.DocumentElement, "refID");
                strReaderBarcode = dp2StringUtil.BuildReaderKey(barcode, refid);
            }
            /*
            // 2024/2/20
            strReaderBarcode = this.ReaderKey;
            if (string.IsNullOrEmpty(strReaderBarcode))
                return 0;
            */
            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            LibraryChannel channel = sessioninfo.GetChannel(true);
            try
            {
                string style = "return,lost,read";

                // 2021/6/8
                if (nItemsPerPage == -1)
                    style += ",noResult";

                // 获得借阅历史
                // parameters:
                //      nPageNo 页号
                //      nItemsPerPage    每页的事项个数。如果为 -1，表示希望从头获取全部内容
                // return:
                //      -1  出错
                //      其它  符合条件的事项总数
                return (int)//sessioninfo.Channel.
                    channel.LoadChargingHistory(
                    null,
                    strReaderBarcode,
                    style,
                    nPageNo,
                    nItemsPerPage,
                    out results,
                    out strError);
            }
            finally
            {
                sessioninfo.ReturnChannel(channel);
            }
        }

        // 计算出页码总数
        public int PageCount
        {
            get
            {
                int nDelta = this.ResultCount % this.PageMaxLines;
                if (nDelta > 0)
                    return (this.ResultCount / this.PageMaxLines) + 1;
                return (this.ResultCount / this.PageMaxLines);
            }
        }

        // 设置结果集有关数量参数
        public void SetResultInfo(int nResultCount)
        {
            int nPageNo = this.StartIndex / this.PageMaxLines;

            LiteralControl resultinfo = (LiteralControl)this.FindControl("resultinfo");
            if (nResultCount/*this.ResultCount*/ != 0)  // 2007/8/24 new changed
            {
                // text-level: 界面信息
                resultinfo.Text = string.Format(
                    this.GetString("借阅历史信息共s条, 分s页显示, 当前为第s页"),
                    // "借阅历史信息共 {0} 条, 分 {1} 页显示, 当前为第 {2} 页。"
                    Convert.ToString(nResultCount),
                    Convert.ToString(this.PageCount),
                    Convert.ToString(nPageNo + 1));
                // "借阅历史信息共 " + Convert.ToString(nResultCount) + " 条, 分 " + Convert.ToString(this.PageCount) + " 页显示, 当前为第 " + Convert.ToString(nPageNo + 1) + "页。";
            }
            else
            {
                // text-level: 界面提示
                resultinfo.Text = this.GetString("借阅历史信息为空"); // "(借阅历史信息为空)"
            }

            LiteralControl maxpagecount = (LiteralControl)this.FindControl("maxpagecount");

            maxpagecount.Text = " " + string.Format(this.GetString("共s页"),  // "(共 {0} 页)"
                Convert.ToString(this.PageCount));
            // " (共 " + Convert.ToString(this.PageCount) + " 页)";

            LiteralControl currentpageno = (LiteralControl)this.FindControl("currentpageno");
            currentpageno.Text = Convert.ToString(nPageNo + 1);

            PlaceHolder pageswitcher = (PlaceHolder)this.FindControl("pageswitcher");
            if (this.PageCount <= 1)
                pageswitcher.Visible = false;
            else
                pageswitcher.Visible = true;
        }


        protected override void CreateChildControls()
        {
            this.Controls.Add(new LiteralControl(
                this.GetPrefixString(
                this.GetString("借阅历史"), // "借阅历史"
                "content_wrapper")
                + "<table class='borrowhistory'>"
                ));

            this.Controls.Add(new LiteralControl(
                "<tr class='info'><td colspan='4'>"
            ));

            // 信息文字
            LiteralControl resultinfo = new LiteralControl();
            resultinfo.ID = "resultinfo";
            this.Controls.Add(resultinfo);

            this.Controls.Add(new LiteralControl(
                "</td></tr>"
            ));

            // 标题行
            this.Controls.Add(new LiteralControl(
                "<tr class='columntitle'><td class='no'>"
                + this.GetString("序号")
                + "</td><td class='action'>"
                + this.GetString("类型")
                + "</td><td class='barcode'>"
                + this.GetString("册条码号")
                + "</td><td class='summary'>"
                + this.GetString("书目摘要")
                + "</td><td class='borrowinfo'>"
                + this.GetString("借阅情况")
                + "</td><td class='renewcomment'>"
                + this.GetString("续借注")
                + "</td><td class='operator'>"
                + this.GetString("操作者")
                + "</td></tr>"
            ));

            // 内容代表
            PlaceHolder content = new PlaceHolder();
            content.ID = "content";
            this.Controls.Add(content);

            // 内容行
            for (int i = 0; i < this.LineCount; i++)
            {
                PlaceHolder line = NewContentLine(content, i, null);
            }

            // 插入点
            PlaceHolder insertpoint = new PlaceHolder();
            insertpoint.ID = "insertpoint";
            content.Controls.Add(insertpoint);

            // 命令行
            CreateCmdLine();

            this.Controls.Add(new LiteralControl(
               "</table>" + this.GetPostfixString()
               ));
        }

        // 新创建内容行
        PlaceHolder NewContentLine(Control content,
            int nLineNo,
            Control insertpos)
        {
            PlaceHolder line = new PlaceHolder();
            line.ID = "line" + Convert.ToString(nLineNo);

            if (insertpos != null)
            {
                int index = content.Controls.IndexOf(insertpos);
                content.Controls.AddAt(index, line);
            }
            else
            {
                content.Controls.Add(line);
            }

            // 内容
            LiteralControl literal = new LiteralControl();
            literal.ID = "line" + Convert.ToString(nLineNo) + "_content";
            line.Controls.Add(literal);

            /*
            literal = new LiteralControl();
            literal.Text = "</td></tr>";
            line.Controls.Add(literal);
             * */

            return line;
        }

        void CreateCmdLine()
        {
            this.Controls.Add(new LiteralControl(
                "<tr class='cmdline'><td colspan='4'>"
            ));

            this.Controls.Add(new LiteralControl(
                "<table border='0' width='100%'><tr><td>"
            ));

            this.Controls.Add(new LiteralControl(
                "</td><td align='right'> "
            ));

            PlaceHolder pageswitcher = new PlaceHolder();
            pageswitcher.ID = "pageswitcher";
            this.Controls.Add(pageswitcher);

            LinkButton firstpage = new LinkButton();
            firstpage.ID = "first";
            firstpage.Text = this.GetString("首页");
            firstpage.CssClass = "firstpage";
            firstpage.Click += new EventHandler(firstpage_Click);
            pageswitcher.Controls.Add(firstpage);

            pageswitcher.Controls.Add(new LiteralControl(
                " "
            ));

            LinkButton prevpage = new LinkButton();
            prevpage.ID = "prev";
            prevpage.Text = this.GetString("前页");
            prevpage.CssClass = "prevpage";
            prevpage.Click += new EventHandler(prevpage_Click);
            pageswitcher.Controls.Add(prevpage);

            pageswitcher.Controls.Add(new LiteralControl(
                " "
            ));

            LiteralControl literal = new LiteralControl();
            literal.ID = "currentpageno";
            literal.Text = "";
            pageswitcher.Controls.Add(literal);

            pageswitcher.Controls.Add(new LiteralControl(
                " "
            ));

            LinkButton nextpage = new LinkButton();
            nextpage.ID = "next";
            nextpage.Text = this.GetString("后页");
            nextpage.CssClass = "nextpage";
            nextpage.Click += new EventHandler(nextpage_Click);
            pageswitcher.Controls.Add(nextpage);

            pageswitcher.Controls.Add(new LiteralControl(
                " "
            ));

            LinkButton lastpage = new LinkButton();
            lastpage.ID = "last";
            lastpage.Text = this.GetString("末页");
            lastpage.CssClass = "lastpage";
            lastpage.Click += new EventHandler(lastpage_Click);
            pageswitcher.Controls.Add(lastpage);

            literal = new LiteralControl();
            literal.Text = "  ";
            pageswitcher.Controls.Add(literal);

            Button gotobutton = new Button();
            gotobutton.ID = "gotobutton";
            gotobutton.Text = this.GetString("跳到");
            gotobutton.CssClass = "goto";
            gotobutton.Click += new EventHandler(gotobutton_Click);
            pageswitcher.Controls.Add(gotobutton);

            literal = new LiteralControl();
            literal.Text = " " + this.GetString("第") + " ";    // " 第 "
            pageswitcher.Controls.Add(literal);

            TextBox textbox = new TextBox();
            textbox.ID = "gotopageno";
            textbox.Width = new Unit("40");
            textbox.CssClass = "gotopageno";
            pageswitcher.Controls.Add(textbox);

            /*
            literal = new LiteralControl();
            literal.Text = " 页";
            pageswitcher.Controls.Add(literal);
             * */

            literal = new LiteralControl();
            literal.ID = "maxpagecount";
            literal.Text = " " + string.Format(this.GetString("maxpagecount"), this.PageCount.ToString());    // (共 {0} 页)
            pageswitcher.Controls.Add(literal);

            this.Controls.Add(new LiteralControl(
                "</td></tr></table>"
            ));

            this.Controls.Add(new LiteralControl(
                "</td></tr>"
            ));
        }

        // 跳到指定的页号
        void gotobutton_Click(object sender, EventArgs e)
        {
            TextBox textbox = (TextBox)this.FindControl("gotopageno");

            int nPageNo = 0;

            try
            {
                nPageNo = Convert.ToInt32(textbox.Text);
            }
            catch
            {
                return;
            }

            if (nPageNo < 1)
                nPageNo = 1;
            this.StartIndex = this.PageMaxLines * (nPageNo - 1);
            if (this.StartIndex >= this.ResultCount)
            {
                lastpage_Click(sender, e);
            }
        }

        void lastpage_Click(object sender, EventArgs e)
        {
            int delta = this.ResultCount % this.PageMaxLines;
            if (delta > 0)
                this.StartIndex = (this.ResultCount / this.PageMaxLines) * this.PageMaxLines;
            else
                this.StartIndex = Math.Max(0, (this.ResultCount / this.PageMaxLines) * this.PageMaxLines - 1);
        }

        void nextpage_Click(object sender, EventArgs e)
        {
            this.StartIndex += this.PageMaxLines;
            if (this.StartIndex >= this.ResultCount)
            {
                lastpage_Click(sender, e);
            }
        }

        void prevpage_Click(object sender, EventArgs e)
        {
            this.StartIndex -= this.PageMaxLines;
            if (this.StartIndex < 0)
                this.StartIndex = 0;
        }

        void firstpage_Click(object sender, EventArgs e)
        {
            this.StartIndex = 0;
        }

        class LineInfo
        {
            public string strAction { get; set; }
            public string strBarcode { get; set; }
            public string strRecPath { get; set; }
            public string strBorrowDate { get; set; }
            public string strBorrowPeriod { get; set; }
            public string strBorrowOperator { get; set; }   // 2016/1/1
            public string strReturnDate { get; set; }
            public string strNo { get; set; }
            public string strRenewComment { get; set; }
            public string strOperator { get; set; }

            public string strBiblioRecPath { get; set; }

            public LineInfo()
            {

            }

            public LineInfo(XmlElement node)
            {
                this.strAction = DomUtil.GetAttr(node, "action");
                this.strBarcode = DomUtil.GetAttr(node, "barcode");
                this.strRecPath = DomUtil.GetAttr(node, "recPath");
                this.strBorrowDate = DateTimeUtil.LocalTime(DomUtil.GetAttr(node, "borrowDate"));
                this.strBorrowPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                this.strReturnDate = DateTimeUtil.LocalTime(DomUtil.GetAttr(node, "returnDate"));
                this.strNo = DomUtil.GetAttr(node, "no");
                this.strRenewComment = DomUtil.GetAttr(node, "renewComment");
                this.strOperator = DomUtil.GetAttr(node, "operator");
            }

            public LineInfo(ChargingItemWrapper wrapper)
            {
                this.strAction = wrapper.Item.Action;
                this.strBarcode = wrapper.Item.ItemBarcode;
                this.strRecPath = "";
                this.strBorrowDate = wrapper.RelatedItem == null ? "" : wrapper.RelatedItem.OperTime;
                this.strBorrowPeriod = wrapper.RelatedItem == null ? "" : wrapper.RelatedItem.Period;
                this.strBorrowOperator = wrapper.RelatedItem == null ? "" : wrapper.RelatedItem.Operator;
                this.strReturnDate = wrapper.Item.OperTime;
                this.strNo = wrapper.RelatedItem == null ? "" : wrapper.RelatedItem.No;
                if (this.strNo == "0")
                    this.strNo = "";
                this.strBiblioRecPath = wrapper.Item.BiblioRecPath;

                this.strRenewComment = "";
                this.strOperator = wrapper.Item.Operator;
            }

            public bool IsEmpty()
            {
                return (string.IsNullOrEmpty(this.strBarcode) == true);
            }
        }

        protected override void Render(HtmlTextWriter writer)
        {
            int nRet = 0;

            string strError = "";
            List<LineInfo> infos = new List<LineInfo>();

            int nPageNo = this.StartIndex / this.PageMaxLines;

            if (this.DatabaseMode == false)
            {
                // return:
                //      -1  出错
                //      0   成功
                //      1   尚未登录
                nRet = this.LoadReaderXml(out strError);
                if (nRet == -1)
                {
                    writer.Write(strError);
                    return;
                }

                if (nRet == 1)
                {
                    sessioninfo.LoginCallStack.Push(this.Page.Request.RawUrl);
                    this.Page.Response.Redirect("login.aspx", true);
                    return;
                }

                XmlNodeList nodes = ReaderDom.DocumentElement.SelectNodes("borrowHistory/borrow");

                SetResultInfo(nodes.Count);

                for (int i = this.StartIndex; i < this.StartIndex + this.PageMaxLines; i++)
                {
                    if (i >= nodes.Count)
                    {
                        infos.Add(new LineInfo());
                    }
                    else
                    {
                        XmlNode node = nodes[i];
                        infos.Add(new LineInfo(node as XmlElement));
                    }
                }
            }
            else
            {
                List<ChargingItemWrapper> results = null;

                // return:
                //      -2  尚未登录
                //      -1  出错
                //      其它  符合条件的事项总数
                nRet = GetChargingHistory(nPageNo,
            this.PageMaxLines,
            out results,
            out strError);
                if (nRet == -1)
                {
                    writer.Write(strError);
                    return;
                }
                if (nRet == -2)
                {
                    sessioninfo.LoginCallStack.Push(this.Page.Request.RawUrl);
                    this.Page.Response.Redirect("login.aspx", true);
                    return;
                }

                SetResultInfo(nRet);

                if (results != null)    // 2016/10/30
                {
                    for (int i = 0; i < this.PageMaxLines; i++)
                    {
                        if (i >= results.Count)
                        {
                            infos.Add(new LineInfo());
                        }
                        else
                        {
                            ChargingItemWrapper wrapper = results[i];
                            LineInfo info = new LineInfo(wrapper);
                            infos.Add(info);
                        }
                    }
                }
            }

            if (infos.Count != 0)
            {
                //OpacApplication app = (OpacApplication)this.Page.Application["app"];
                //SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

                // 显示本页中的浏览行
                {
                    int i = 0;
                    foreach (LineInfo info in infos)
                    {
                        StringBuilder text = new StringBuilder();

                        string strBarcodeLink = "<a href='book.aspx?barcode=" + info.strBarcode + "&forcelogin=userid' target='_blank'>" + info.strBarcode + "</a>";
                        string strItemBarcode = info.strBarcode;
                        if (info.strAction == "read")
                        {
                            if (string.IsNullOrEmpty(strItemBarcode) == true)
                                strItemBarcode = "@biblioRecPath:" + info.strBiblioRecPath;
                            strBarcodeLink = "<a href='book.aspx?barcode=" + strItemBarcode + "&forcelogin=userid' target='_blank'>" + strItemBarcode + "</a>";
                        }

                        string strTrClass = " class='dark content " + info.strAction + "' ";

                        if ((i % 2) == 1)
                            strTrClass = " class='light content " + info.strAction + "' ";

                        text.Append("<tr " + strTrClass + ">");

                        text.Append("<td class='no'>" + (i + this.StartIndex + 1).ToString() + "</td>");
                        text.Append("<td class='action'>" + HttpUtility.HtmlEncode(GetActionName(info.strAction)) + "</td>");
                        text.Append("<td class='barcode'>" + strBarcodeLink + "</td>");
                        text.Append("<td class='summary pending' >" + HttpUtility.HtmlEncode(strItemBarcode) + "</td>");

                        info.strBorrowPeriod = app.GetDisplayTimePeriodStringEx(info.strBorrowPeriod);

                        if (info.IsEmpty() == true)
                            text.Append("<td class='borrowinfo'>" + "</td>");
                        else
                        {
                            if (info.strAction == "read")
                                text.Append("<td class='borrowinfo'>"
    + "<div class='returndate'>"
    + this.GetString("操作日期")
    + ":" + info.strReturnDate + "</div>"
    + "</td>");
                            else
                                text.Append("<td class='borrowinfo'>"
                                    + "<div class='borrowno'>"
                                    + this.GetString("续借次")
                                    + "  :" + info.strNo + "</div>"
                                    + "<div class='borrowdate'>"
                                    + this.GetString("借阅日期")
                                    + ":" + info.strBorrowDate + "</div>"
                                    + "<div class='borrowperiod'>"
                                    + this.GetString("期限")
                                    + ":    " + info.strBorrowPeriod + "</div>"
                                    + "<div class='returndate'>"
                                    + this.GetString("还书日期")
                                    + ":" + info.strReturnDate + "</div>"
                                    + "</td>");
                        }

                        text.Append("<td class='renewcomment'>" + HttpUtility.HtmlEncode(info.strRenewComment) + "</td>");

                        if (this.DatabaseMode == false || info.IsEmpty())
                            text.Append("<td class='operator'>" + info.strOperator + "</td>");
                        else
                        {
                            if (info.strAction == "read")
                                text.Append("<td class='operator'>"
                                    + HttpUtility.HtmlEncode(info.strOperator)
                                    + "</td>");
                            else
                                text.Append("<td class='operator'>"
        + "<div class='borrowoperator'>"
        + this.GetString("借")
        + ": " + HttpUtility.HtmlEncode(info.strBorrowOperator) + "</div>"
        + "<div class='returnoperator'>"
        + this.GetString("还")
        + ": " + HttpUtility.HtmlEncode(info.strOperator) + "</div>"
        + "</td>");
                        }

                        text.Append("</tr>");

                        PlaceHolder line = (PlaceHolder)this.FindControl("line" + Convert.ToString(i));
                        if (line == null)
                        {
                            PlaceHolder insertpoint = (PlaceHolder)this.FindControl("insertpoint");
                            PlaceHolder content = (PlaceHolder)this.FindControl("content");

                            line = this.NewContentLine(content, i, insertpoint);
                        }

                        LiteralControl contentcontrol = (LiteralControl)this.FindControl("line" + Convert.ToString(i) + "_content");
                        contentcontrol.Text = text.ToString();
                        i++;
                    } // end of for
                }

                this.LineCount = Math.Max(this.LineCount, this.PageMaxLines);
            }
            else
            {
                // 显示空行
                for (int i = 0; i < this.PageMaxLines; i++)
                {
                    PlaceHolder line = (PlaceHolder)this.FindControl("line" + Convert.ToString(i));
                    if (line == null)
                        continue;
                }
            }

            base.Render(writer);
        }

        static string GetActionName(string strText)
        {
            if (strText == "return")
                return "借阅";
            if (strText == "read")
                return "读过";
            return strText;
        }
    }
}
