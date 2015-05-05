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

namespace DigitalPlatform.OPAC.Web
{
    [ToolboxData("<{0}:BorrowHistoryControl runat=server></{0}:BorrowHistoryControl>")]
    public class BorrowHistoryControl : ReaderInfoBase
    {
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
                // return:
                //      -1  出错
                //      0   成功
                //      1   尚未登录
                int nRet = this.LoadReaderXml(out strError);
                if (nRet == -1)
                {
                    return 0;
                }

                if (nRet == 1)
                {
                    return 0;
                }

                XmlNodeList nodes = ReaderDom.DocumentElement.SelectNodes("borrowHistory/borrow");

                return nodes.Count;
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


        protected override void Render(HtmlTextWriter writer)
        {
            int nRet = 0;

            string strError = "";
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

            int nPageNo = this.StartIndex / this.PageMaxLines;

            XmlNodeList nodes = ReaderDom.DocumentElement.SelectNodes("borrowHistory/borrow");

            SetResultInfo(nodes.Count);

            if (nodes.Count != 0)
            {
                OpacApplication app = (OpacApplication)this.Page.Application["app"];
                SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];


                // 显示本页中的浏览行
                for (int i = this.StartIndex; i < this.StartIndex + this.PageMaxLines; i++)
                {
                    /*
                    string strBarcode = "";
                    string strRecPath = "";
                    string strBorrowDate = "";
                    string strBorrowPeriod = "";
                    string strReturnDate = "";
                    string strNo = "";
                    string strRenewComment = "";
                    string strOperator = "";
                    string strBarcodeLink = "";
                    string strSummary = "";
                    string strBiblioRecPath = "";
                     * */

                    string strResult = "";

                    if (i >= nodes.Count)
                    {
                        string strTrClass = " class='dark content blank' ";

                        if ((i % 2) == 1)
                            strTrClass = " class='light content blank' ";

                        strResult += "<tr " + strTrClass + ">";

                        strResult += "<td class='no'>" + (i + 1).ToString() + "</td>";
                        strResult += "<td class='barcode'></td>";
                        strResult += "<td class='summary' ></td>";
                        strResult += "<td class='borrowinfo'></td>";
                        strResult += "<td class='renewcomment'></td>";
                        strResult += "<td class='operator'></td>";
                        strResult += "</tr>";

                    }
                    else
                    {

                        XmlNode node = nodes[i];
                        string strBarcode = DomUtil.GetAttr(node, "barcode");
                        string strRecPath = DomUtil.GetAttr(node, "recPath");
                        string strBorrowDate = DateTimeUtil.LocalTime(DomUtil.GetAttr(node, "borrowDate"));
                        string strBorrowPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                        string strReturnDate = DateTimeUtil.LocalTime(DomUtil.GetAttr(node, "returnDate"));
                        string strNo = DomUtil.GetAttr(node, "no");
                        string strRenewComment = DomUtil.GetAttr(node, "renewComment");
                        string strOperator = DomUtil.GetAttr(node, "operator");

                        string strBarcodeLink = "<a href='book.aspx?barcode=" + strBarcode + "&forcelogin=userid' target='_blank'>" + strBarcode + "</a>";

#if NO
                        // 获得摘要
                        string strSummary = "";
                        string strBiblioRecPath = "";

                        long lRet = sessioninfo.Channel.GetBiblioSummary(
                            null,
                            strBarcode,
                            null,
                            null,
                            out strBiblioRecPath,
                            out strSummary,
                            out strError);
                        if (lRet == -1 || lRet == 0)
                            strSummary = strError;
                        /*
                        LibraryServerResult result = app.GetBiblioSummary(
                            sessioninfo,
                            strBarcode,
                            null,
                            null,
                            out strBiblioRecPath,
                            out strSummary);
                        if (result.Value == -1 || result.Value == 0)
                            strSummary = result.ErrorInfo;
                        */
#endif

                        string strTrClass = " class='dark content' ";

                        if ((i % 2) == 1)
                            strTrClass = " class='light content' ";


                        strResult += "<tr " + strTrClass + ">";

                        strResult += "<td class='no'>" + (i + 1).ToString() + "</td>";
                        strResult += "<td class='barcode'>" + strBarcodeLink + "</td>";
                        strResult += "<td class='summary pending' >" + strBarcode + "</td>";

                        strBorrowPeriod = app.GetDisplayTimePeriodStringEx(strBorrowPeriod);

                        strResult += "<td class='borrowinfo'>"
                            + "<div class='borrowno'>"
                            + this.GetString("续借次")
                            + "  :" + strNo + "</div>"
                            + "<div class='borrowdate'>"
                            + this.GetString("借阅日期")
                            + ":" + strBorrowDate + "</div>"
                            + "<div class='borrowperiod'>"
                            + this.GetString("期限")
                            + ":    " + strBorrowPeriod + "</div>"
                            + "<div class='returndate'>"
                            + this.GetString("还书日期")
                            + ":" + strReturnDate + "</div>"
                            + "</td>";
                        strResult += "<td class='renewcomment'>" + strRenewComment + "</td>";
                        strResult += "<td class='operator'>" + strOperator + "</td>";
                        strResult += "</tr>";
                    }

                    PlaceHolder line = (PlaceHolder)this.FindControl("line" + Convert.ToString(i));
                    if (line == null)
                    {
                        PlaceHolder insertpoint = (PlaceHolder)this.FindControl("insertpoint");
                        PlaceHolder content = (PlaceHolder)this.FindControl("content");

                        line = this.NewContentLine(content, i, insertpoint);
                    }

                    LiteralControl contentcontrol = (LiteralControl)this.FindControl("line" + Convert.ToString(i) + "_content");

                    contentcontrol.Text = strResult;

                } // end of for

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

        /*
        <borrow 
         barcode="0000001"
         recPath="中文图书实体/1" 
         borrowDate="Fri, 07 Apr 2006 01:38:43 GMT"
         no="0"
         borrowPeriod="30day" 
         renewComment="" 
         operator="test" 
         notifyHistory="yy" />
         */
        /*
        protected override void RenderContents(HtmlTextWriter output)
        {
            string strError = "";
            // return:
            //      -1  出错
            //      0   成功
            //      1   尚未登录
            int nRet = this.LoadReaderXml(out strError);
            if (nRet == -1)
            {
                output.Write(strError);
                return;
            }

            if (nRet == 1)
            {
                sessioninfo.LoginCallStack.Push(this.Page.Request.RawUrl);
                this.Page.Response.Redirect("login.aspx", true);
                return;
            }

            string strResult = "";

            XmlNodeList nodes = ReaderDom.DocumentElement.SelectNodes("borrowHistory/borrow");

            strResult += this.GetPrefixString("借阅历史", null);
            strResult += "<table class='borrowhistory'>";
            strResult += "<tr class='columntitle'><td>册条码号</td><td>书目摘要</td><td>借阅情况</td><td>续借注</td><td>操作者</td></tr>";

            if (nodes.Count == 0)
            {
                strResult += "<tr class='dark' >";
                strResult += "<td class='comment' colspan='6'>(无违约/交费信息)</td>";
                strResult += "</tr>";
            }

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strBarcode = DomUtil.GetAttr(node, "barcode");
                string strRecPath = DomUtil.GetAttr(node, "recPath");
                string strBorrowDate = DateTimeUtil.LocalTime(DomUtil.GetAttr(node, "borrowDate"));
                string strBorrowPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                string strReturnDate = DateTimeUtil.LocalTime(DomUtil.GetAttr(node, "returnDate"));
                string strNo = DomUtil.GetAttr(node, "no");
                string strRenewComment = DomUtil.GetAttr(node, "renewComment");
                string strOperator = DomUtil.GetAttr(node, "operator");

                string strBarcodeLink = "<a href='book.aspx?barcode=" + strBarcode + "&forcelogin=userid' target='_blank'>" + strBarcode + "</a>";

                // 获得摘要
                string strSummary = "";
                string strBiblioRecPath = "";
                Result result = app.GetBiblioSummary(
                    sessioninfo,
                    strBarcode,
                    null,
                    null,
                    out strBiblioRecPath,
                    out strSummary);
                if (result.Value == -1 || result.Value == 0)
                    strSummary = result.ErrorInfo;


                string strTrClass = " class='dark' ";

                if ((i % 2) == 1)
                    strTrClass = " class='light' ";

                strResult += "<tr " + strTrClass + ">";
                strResult += "<td class='barcode' nowrap>" + strBarcodeLink + "</td>";
                strResult += "<td class='summary' >" + strSummary + "</td>";
                strResult += "<td class='borrowinfo'>"
                    + "<div class='borrowno'>续借次  :" + strNo + "</div>"
                    + "<div class='borrowdate'>借阅日期:" + strBorrowDate + "</div>"
                    + "<div class='borrowperiod'>期限:    " + strBorrowPeriod + "</div>"
                    + "<div class='returndate'>还书日期:" + strReturnDate + "</div>"
                    + "</td>";
                strResult += "<td class='renewcomment'>" + strRenewComment + "</td>";
                strResult += "<td class='operator'>" + strOperator + "</td>";
                strResult += "</tr>";

            }

            strResult += "</table>";

            strResult += this.GetPostfixString();

            output.Write(strResult);
        }
         * */
    }
}
