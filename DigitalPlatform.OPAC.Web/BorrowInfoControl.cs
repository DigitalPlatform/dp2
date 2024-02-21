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

//using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient.localhost;
using DigitalPlatform.LibraryClient;

namespace DigitalPlatform.OPAC.Web
{
    /// <summary>
    /// 借阅信息Web控件
    /// </summary>
    [ToolboxData("<{0}:BorrowInfoControl runat=server></{0}:BorrowInfoControl>")]
    public class BorrowInfoControl : WebControl, INamingContainer
    {
        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Web.res.BorrowInfoControl.cs",
                typeof(BorrowInfoControl).Module.Assembly);

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

        // 2024/2/20 将 ReaderBarcode 改造为 ReaderRefID
        // 作为管理员身份此时要查看的读者键。注意，不是指管理员自己的读者键
        // 存储在Session中
        public string ReaderKey
        {
            get
            {
                /*
                object o = this.Page.Session[this.ID + "BorrowInfoControl_readerkey"];
                if (o == null)
                    return "";
                return (string)o;
                */
                return TitleBarControl.GetReaderKey(this);
            }
            set
            {
                /*
                // 清除 ReaderDom 缓存
                {
                    SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];
                    if (sessioninfo != null)
                        sessioninfo.RefreshLoginReaderDomCache(value);
                }
                this.Page.Session[this.ID + "BorrowInfoControl_readerkey"] = value;
                */
                TitleBarControl.SetReaderKey(this, value);
            }
        }

#if REMOVED
        // 作为管理员身份此时要查看的读者证条码号。注意，不是指管理员自己的读者证
        // 存储在Session中
        public string ReaderBarcode
        {
            get
            {
                object o = this.Page.Session[this.ID + "BorrowInfoControl_readerbarcode"];
                if (o == null)
                    return "";
                return (string)o;
            }

            set
            {
                // 2023/11/11
                // 清除 ReaderDom 缓存
                {
                    SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];
                    if (sessioninfo != null)
                        sessioninfo.RefreshLoginReaderDomCache(value);
                }
                this.Page.Session[this.ID + "BorrowInfoControl_readerbarcode"] = value;
            }
        }
#endif

        // 借阅信息内容行数
        public int BorrowLineCount
        {
            get
            {
                object o = this.Page.Session[this.ID + "BorrowInfoControl_BorrowLineCount"];
                return (o == null) ? 0 : (int)o;
            }
            set
            {
                this.Page.Session[this.ID + "BorrowInfoControl_BorrowLineCount"] = (object)value;
            }
        }

        // 已借阅条码号列表
        public List<string> BorrowBarcodes
        {
            get
            {
                object o = this.Page.Session[this.ID + "BorrowInfoControl_BorrowBarcodes"];
                return (o == null) ? new List<string>() : (List<string>)o;
            }
            set
            {
                this.Page.Session[this.ID + "BorrowInfoControl_BorrowBarcodes"] = (object)value;
            }
        }

        // 布局控件
        protected override void CreateChildControls()
        {
            // 借阅信息
            PlaceHolder borrowinfo = new PlaceHolder();
            borrowinfo.ID = "borrowinfo";
            this.Controls.Add(borrowinfo);

            CreateBorrowInfoControls(borrowinfo);
        }

        // return:
        //      null    xml文件不存在，或者<borrowInfoControl>元素不存在
        static string GetColumnStyle(OpacApplication app)
        {
            if (app == null || app.WebUiDom == null)
                return null;

            XmlNode node = app.WebUiDom.DocumentElement.SelectSingleNode("borrowInfoControl");
            if (node == null)
                return null;

            return DomUtil.GetAttr(node, "columnStyle");
        }

        // 创建初始的借阅信息内部行布局
        void CreateBorrowInfoControls(PlaceHolder parent)
        {
            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            if (app == null)
            {
                /*
                strError = "app == null";
                goto ERROR1;
                 * */
                return;
            }

            PlaceHolder borrowinfo = new PlaceHolder();
            borrowinfo.ID = "borrowinfo_content";
            parent.Controls.Add(borrowinfo);


            LiteralControl titleline = new LiteralControl();
            titleline.ID = "borrowinfo_titleline";
            titleline.Text = "<div class='content_wrapper'>";    // cellpadding='0' cellspacing='0' border='0' width='100%'
            titleline.Text += "<table class='roundbar' cellpadding='0' cellspacing='0'>";    // cellpadding='0' cellspacing='0' border='0' width='100%'
            titleline.Text += "<tr class='titlebar'>"
                + "<td class='left'></td>"
                + "<td class='middle'>"
                + this.GetString("借阅信息")
                + "</td>"
                + "<td class='right'></td>"
                + "</tr>";

            titleline.Text += "</table>";
            titleline.Text += "<table class='borrowinfo'>";

            // return:
            //      null    xml文件不存在，或者<borrowInfoControl>元素不存在
            string strColumnStyle = GetColumnStyle(app);
            if (strColumnStyle == null)
                strColumnStyle = "";    // 2009/11/23 防止ToLower()抛出异常
            if (strColumnStyle.ToLower() == "style1")
            {
                // 新风格，将是否超期栏目修改为“应还日期”栏目
                titleline.Text += "<tr class='columntitle'><td class='barcode' nowrap>"
                    + this.GetString("册条码号")
                    + "</td><td class='summary' nowrap width='50%'>"
                    + this.GetString("摘要")
                    + "</td><td class='no' nowrap>"
                    + this.GetString("续借次")
                    + "</td><td class='borrowdate' nowrap>"
                    + this.GetString("借阅日期")
                    + "</td><td class='borrowperiod' nowrap>"
                    + this.GetString("期限")
                    + "</td><td class='returningdate' nowrap>"
                    + this.GetString("应还日期")
                    + "</td><td class='renewcomment' nowrap>"
                    + this.GetString("续借注释")
                    + "</td><td class='operator' nowrap>"
                    + this.GetString("操作者")
                    + "</td></tr>";
            }
            else
            {
                // 缺省的
                titleline.Text += "<tr class='columntitle'><td class='barcode' nowrap>"
    + this.GetString("册条码号")
    + "</td><td class='summary' nowrap width='50%'>"
    + this.GetString("摘要")
    + "</td><td class='no' nowrap>"
    + this.GetString("续借次")
    + "</td><td class='borrowdate' nowrap>"
    + this.GetString("借阅日期")
    + "</td><td class='borrowperiod' nowrap>"
    + this.GetString("期限")
    + "</td><td class='overdue' nowrap>"
    + this.GetString("是否超期")
    + "</td><td class='renewcomment' nowrap>"
    + this.GetString("续借注释")
    + "</td><td class='operator' nowrap>"
    + this.GetString("操作者")
    + "</td></tr>";

            }
            borrowinfo.Controls.Add(titleline);

            // 每一行一个占位控件
            for (int i = 0; i < this.BorrowLineCount; i++)
            {
                PlaceHolder line = NewBorrowLine(borrowinfo, i, null);
                line.Visible = true;
            }

            // 表格行插入点
            PlaceHolder insertpos = new PlaceHolder();
            insertpos.ID = "borrowinfo_insertpos";
            borrowinfo.Controls.Add(insertpos);


            //
            // 命令行
            PlaceHolder cmdline = new PlaceHolder();
            cmdline.ID = "borrowinfo_cmdline";
            borrowinfo.Controls.Add(cmdline);

            LiteralControl literal = new LiteralControl();
            literal.Text = "<tr class='cmdline'><td colspan='8'>";
            cmdline.Controls.Add(literal);

            Button renewButton = new Button();
            renewButton.ID = "borrowinfo_renewbutton";
            renewButton.Text = this.GetString("续借");
            renewButton.CssClass = "renewbutton";
            renewButton.Click += new EventHandler(renewButton_Click);
            cmdline.Controls.Add(renewButton);
            renewButton = null;

            literal = new LiteralControl();
            literal.Text = "</td></tr>";
            cmdline.Controls.Add(literal);

            cmdline = null;

            // 调试信息行
            PlaceHolder debugline = new PlaceHolder();
            debugline.ID = "debugline";
            borrowinfo.Controls.Add(debugline);

            literal = new LiteralControl();
            literal.Text = "<tr class='debugline'><td colspan='8'>";
            debugline.Controls.Add(literal);

            literal = new LiteralControl();
            literal.ID = "debugtext";
            literal.Text = "";
            debugline.Controls.Add(literal);


            literal = new LiteralControl();
            literal.Text = "</td></tr>";
            debugline.Controls.Add(literal);

            debugline = null;

            //
            /*

            // 最后一个空白行
            LiteralControl literal = new LiteralControl();
            literal.ID = "borrowinfo_blank";
            literal.Text = "<tr class='roundcontent'><td colspan='9'>&nbsp;</td></tr>";
            borrowinfo.Controls.Add(literal);
             */


            literal = new LiteralControl();
            literal.ID = "borrowinfo_tableend";
            literal.Text = "</table>";
            borrowinfo.Controls.Add(literal);


            literal = new LiteralControl();
            literal.Text = "</div>";
            borrowinfo.Controls.Add(literal);

        }

        PlaceHolder NewBorrowLine(Control parent,
    int index,
    Control insertbefore)
        {
            if (parent != null && insertbefore != null)
            {
                if (insertbefore.Parent != parent)
                {
                    // text-level: 内部错误
                    throw new Exception("插入参考位置和父Control之间, 父子关系不正确");
                }
            }


            PlaceHolder line = new PlaceHolder();
            line.ID = "borrowinfo_line" + Convert.ToString(index);

            if (FindControl(line.ID) != null)
                throw new Exception("id='" + line.ID + "' already existed");

            if (insertbefore == null)
                parent.Controls.Add(line);
            else
            {
                int pos = parent.Controls.IndexOf(insertbefore);
                if (pos == -1)
                {
                    // text-level: 内部错误
                    throw new Exception("插入参照对象没有找到");
                }
                parent.Controls.AddAt(pos, line);
            }


            // 左侧文字
            LiteralControl literal = new LiteralControl();
            literal.ID = "borrowinfo_line" + Convert.ToString(index) + "left";
            literal.Text = "<tr><td>";
            line.Controls.Add(literal);

            // checkbox
            CheckBox checkbox = new CheckBox();
            checkbox.ID = "borrowinfo_line" + Convert.ToString(index) + "checkbox";
            line.Controls.Add(checkbox);

            // 右侧文字
            literal = new LiteralControl();
            literal.ID = "borrowinfo_line" + Convert.ToString(index) + "right";
            literal.Text = "</td></tr>";
            line.Controls.Add(literal);


            return line;
        }

        void SetDebugInfo(string strText)
        {
            PlaceHolder line = (PlaceHolder)FindControl("debugline");
            line.Visible = true;

            LiteralControl text = (LiteralControl)line.FindControl("debugtext");
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

        void RenderBorrow(
    OpacApplication app,
    SessionInfo sessioninfo,
    XmlDocument dom)
        {
            string strReaderType = DomUtil.GetElementText(dom.DocumentElement,
                "readerType");

            // 获得日历
            string strError = "";
            /*
            Calendar calendar = null;
            int nRet = app.GetReaderCalendar(strReaderType, out calendar, out strError);
            if (nRet == -1)
            {
                this.SetDebugInfo("warninginfo", strError);
                calendar = null;
            }
             * */

            // return:
            //      null    xml文件不存在，或者<borrowInfoControl>元素不存在
            string strColumnStyle = GetColumnStyle(app);
            if (strColumnStyle == null)
                strColumnStyle = "";    // 2009/11/23 防止ToLower()抛出异常

            // 借阅的册
            PlaceHolder borrowinfo = (PlaceHolder)this.FindControl("borrowinfo");

            // 清空集合
            this.BorrowBarcodes = new List<string>();

            string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement,
                "barcode");

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("borrows/borrow");
            this.BorrowLineCount = nodes.Count;
            for (int i = 0; i < nodes.Count; i++)
            {

                PlaceHolder line = (PlaceHolder)borrowinfo.FindControl("borrowinfo_line" + Convert.ToString(i));
                if (line == null)
                {
                    Control insertpos = borrowinfo.FindControl("borrowinfo_insertpos");
                    line = NewBorrowLine(insertpos.Parent, i, insertpos);
                    // this.BorrowLineCount++;
                }
                line.Visible = true;

                LiteralControl left = (LiteralControl)line.FindControl("borrowinfo_line" + Convert.ToString(i) + "left");
                CheckBox checkbox = (CheckBox)line.FindControl("borrowinfo_line" + Convert.ToString(i) + "checkbox");
                LiteralControl right = (LiteralControl)line.FindControl("borrowinfo_line" + Convert.ToString(i) + "right");


                XmlNode node = nodes[i];

                string strBarcode = DomUtil.GetAttr(node, "barcode");

                // 添加到集合
                this.BorrowBarcodes.Add(strBarcode);

                string strNo = DomUtil.GetAttr(node, "no");
                string strBorrowDate = DomUtil.GetAttr(node, "borrowDate");
                string strPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                string strOperator = DomUtil.GetAttr(node, "operator");
                string strRenewComment = DomUtil.GetAttr(node, "renewComment");

                string strOverDue = "";
                bool bOverdue = false;  // 是否超期

                DateTime timeReturning = DateTime.MinValue;
                string strTips = "";
#if NO

                if (strColumnStyle.ToLower() == "style1")
                {
                    DateTime timeNextWorkingDay;
                    long lOver = 0;
                    string strPeriodUnit = "";

                    // 获得还书日期
                    // return:
                    //      -1  数据格式错误
                    //      0   没有发现超期
                    //      1   发现超期   strError中有提示信息
                    //      2   已经在宽限期内，很容易超期 
                    nRet = app.GetReturningTime(
                        calendar,
                        strBorrowDate,
                        strPeriod,
                        out timeReturning,
                        out timeNextWorkingDay,
                        out lOver,
                        out strPeriodUnit,
                        out strError);
                    if (nRet == -1)
                        strOverDue = strError;
                    else
                    {
                        strTips = strError;
                        if (nRet == 1)
                        {
                            bOverdue = true;
                            strOverDue = " ("
                                + string.Format(this.GetString("已超期s"),  // 已超期 {0}
                                                app.GetDisplayTimePeriodStringEx(lOver.ToString() + " " + strPeriodUnit))
                                + ")";
                            /*
                            strOverDue = " (已超期 " 
                                + lOver.ToString()
                                + " "
                                + app.GetDisplayTimeUnitLang(strPeriodUnit)
                                + ")";
                             * */
                        }
                    }
                }
                else
                {
                    // string strError = "";
                    // 检查超期情况。
                    // return:
                    //      -1  数据格式错误
                    //      0   没有发现超期
                    //      1   发现超期   strError中有提示信息
                    nRet = app.CheckPeriod(
                        calendar,
                        strBorrowDate,
                        strPeriod,
                        out strError);
                    if (nRet == -1)
                        strOverDue = strError;
                    else
                    {
                        if (nRet == 1)
                            bOverdue = true;
                        strOverDue = strError;	// 其他无论什么情况都显示出来
                    }
                }

#endif

                string strResult = "";

                string strTrClass = " class='dark' ";

                if ((i % 2) == 1)
                    strTrClass = " class='light' ";

                strResult += "<tr " + strTrClass + " nowrap><td class='barcode' nowrap>";
                // 左
                left.Text = strResult;

                // checkbox
                // checkbox.Text = Convert.ToString(i + 1);

                // 右开始
                strResult = "&nbsp;";

                strResult += "<a href='book.aspx?barcode=" + strBarcode + "&borrower=" + strReaderBarcode + "'>"
                    + strBarcode + "</a></td>";

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
                 * */
#endif

                strResult += "<td class='summary pending' width='50%'>" + strBarcode + "</td>";
                strResult += "<td class='no' nowrap align='right'>" + strNo + "</td>";  // 续借次

                strResult += "<td class='borrowdate' nowrap>" + OpacApplication.LocalDateOrTime(strBorrowDate, strPeriod) + "</td>";
                strResult += "<td class='borrowperiod' nowrap>" + app.GetDisplayTimePeriodStringEx(strPeriod) + "</td>";

                strOverDue = DomUtil.GetAttr(node, "overdueInfo");
                string strOverdue1 = DomUtil.GetAttr(node, "overdueInfo1");
                string strIsOverdue = DomUtil.GetAttr(node, "isOverdue");
                if (strIsOverdue == "yes")
                    bOverdue = true;

                string strTimeReturning = DomUtil.GetAttr(node, "timeReturning");
                if (String.IsNullOrEmpty(strTimeReturning) == false)
                    timeReturning = DateTimeUtil.FromRfc1123DateTimeString(strTimeReturning).ToLocalTime();

                if (strColumnStyle.ToLower() == "style1")
                {
                    strTips = strOverDue;
                    strOverDue = strOverdue1;

                    if (bOverdue == true)
                        strResult += "<td class='returningdate overdue'>"
                            + "<a title=\"" + strTips.Replace("\"", "'") + "\">"
                            + OpacApplication.LocalDateOrTime(timeReturning, strPeriod)
                            // + timeReturning.ToString("d")
                            + strOverDue
                            + "</a>"
                            + "</td>";
                    else
                        strResult += "<td class='returningdate'>"
                            + "<a title=\"" + strTips.Replace("\"", "'") + "\">"
                            + OpacApplication.LocalDateOrTime(timeReturning, strPeriod)
                            // + timeReturning.ToString("d")
                            + strOverDue
                            + "</a>"
                            + "</td>";
                }
                else
                {
                    if (bOverdue == true)
                        strResult += "<td class='overdue'>" + strOverDue + "</td>";
                    else
                        strResult += "<td class='notoverdue'>" + strOverDue + "</td>";
                }
                strResult += "<td class='renewcomment'>" + strRenewComment.Replace(";", "<br/>") +

    "</td>";
                strResult += "<td class='operator' nowrap>" + strOperator + "</td>";
                strResult += "</tr>";

                right.Text = strResult;
            }

            // 把多余的行隐藏起来
            for (int i = nodes.Count; ; i++)
            {
                PlaceHolder line = (PlaceHolder)borrowinfo.FindControl("borrowinfo_line" + Convert.ToString(i));
                if (line == null)
                    break;

                line.Visible = false;
            }

            if (nodes.Count == 0)
            {

                Control insertpos = borrowinfo.FindControl("borrowinfo_insertpos");
                int pos = insertpos.Parent.Controls.IndexOf(insertpos);
                if (pos == -1)
                {
                    // text-level: 内部错误
                    throw new Exception("插入参照对象没有找到");
                }

                LiteralControl literal = new LiteralControl();
                literal.Text = "<tr class='dark'><td colspan='8'>" + this.GetString("(无借阅信息)") + "<td></tr>";

                insertpos.Parent.Controls.AddAt(pos, literal);
            }


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
            if (nRet == -1)
                goto ERROR1;

            if (nRet == 0)
            {
                sessioninfo.LoginCallStack.Push(this.Page.Request.RawUrl);
                this.Page.Response.Redirect("login.aspx", true);
                return;
            }

            if (nRet == -2)
            {
                if (String.IsNullOrEmpty(this.ReaderKey) == true)
                {
                    // text-level: 内部错误
                    strError = "当前登录的用户不是reader类型，并且BorrowInfoControl.ReaderBarcode也为空";
                    goto ERROR1;
                }

                // TODO: 是否进一步判断Type
                // if (sessioninfo.Account.Type != "worreader")

                // 管理员获得特定证条码号的读者记录DOM
                // parameters:
                //      strReaderKey    读者键
                // return:
                //      -2  当前登录的用户不是librarian类型
                //      -1  出错
                //      0   尚未登录
                //      1   成功
                nRet = sessioninfo.GetOtherReaderDom(
                    this.ReaderKey,
                    out readerdom,
                    out strError);
                if (nRet == -1 || nRet == -2)
                    goto ERROR1;

                if (nRet == 0)
                {
                    sessioninfo.LoginCallStack.Push(this.Page.Request.RawUrl);
                    this.Page.Response.Redirect("login.aspx?loginstyle=librarian", true);
                    return;
                }
            }

            // 兑现 借阅信息
            RenderBorrow(app,
                sessioninfo,
                readerdom);

            base.Render(output);
            return;

        ERROR1:
            output.Write(strError);
        }

        List<string> GetCheckedBorrowBarcodes()
        {
            List<string> barcodes = new List<string>();

            for (int i = 0; i < this.BorrowLineCount; i++)
            {
                CheckBox checkbox = (CheckBox)this.FindControl("borrowinfo_line" + Convert.ToString(i) + "checkbox");
                if (checkbox.Checked == true)
                {
                    if (this.BorrowBarcodes.Count <= i)
                    {
                        // text-level: 内部错误
                        throw new Exception("BorrowBarcodes失效...");
                    }
                    string strBarcode = this.BorrowBarcodes[i];

                    barcodes.Add(strBarcode);

                    checkbox.Checked = false;
                }
            }

            return barcodes;
        }

        void renewButton_Click(object sender, EventArgs e)
        {
            List<string> barcodes = this.GetCheckedBorrowBarcodes();

            if (barcodes.Count == 0)
            {
                // text-level: 用户提示
                this.SetDebugInfo("errorinfo", this.GetString("尚未选择要续借的事项")); // "操作失败。尚未选择要续借的事项。"
                return;
            }

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            LibraryChannel channel = sessioninfo.GetChannel(true);
            try
            {
                for (int i = 0; i < barcodes.Count; i++)
                {
                    string strItemBarcode = barcodes[i];

                    string strReaderKey = "";
                    if (String.IsNullOrEmpty(this.ReaderKey) == false)
                        strReaderKey = this.ReaderKey;
                    else
                        strReaderKey = sessioninfo.ReaderInfo.ReaderKey;

                    if (String.IsNullOrEmpty(strReaderKey) == true)
                    {
                        // text-level: 用户提示
                        this.SetDebugInfo("errorinfo", this.GetString("尚未指定读者证条码号"));   // "尚未指定读者证条码号。操作失败。"
                        return;
                    }

                    string[] aDupPath = null;
                    string[] item_records = null;
                    string[] reader_records = null;
                    string[] biblio_records = null;
                    BorrowInfo borrow_info = null;
                    string strError = "";
                    string strOutputReaderBarcode = "";

                    long lRet = // sessioninfo.Channel.
                        channel.Borrow(
                        null,
                        true,
                        strReaderKey,
                        strItemBarcode,
                        null,
                        false,
                        null,
                        "", // style
                        "",
                        out item_records,
                        "",
                        out reader_records,
                        "",
                        out biblio_records,
                        out aDupPath,
                        out strOutputReaderBarcode,
                        out borrow_info,
                        out strError);
                    if (lRet == -1)
                    {
                        this.SetDebugInfo("errorinfo", strError);
                        return;
                    }

                    // 清除读者记录缓存，以便借阅信息得到刷新
                    sessioninfo.ClearLoginReaderDomCache();
                }
            }
            finally
            {
                sessioninfo.ReturnChannel(channel);
            }

            // text-level: 用户提示
            this.SetDebugInfo(this.GetString("续借成功"));   // "续借成功。"
        }

        public override void RenderBeginTag(HtmlTextWriter writer)
        {

        }
        public override void RenderEndTag(HtmlTextWriter writer)
        {

        }
    }
}
