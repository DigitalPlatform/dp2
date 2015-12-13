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

using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Xml;
using DigitalPlatform.OPAC.Server;
using DigitalPlatform.CirculationClient;
using DigitalPlatform.LibraryClient;

namespace DigitalPlatform.OPAC.Web
{
    [ToolboxData("<{0}:ReservationInfoControl runat=server></{0}:ReservationInfoControl>")]
    public class ReservationInfoControl : WebControl, INamingContainer
    {
        ResourceManager m_rm = null;

        ResourceManager GetRm()
        {
            if (this.m_rm != null)
                return this.m_rm;

            this.m_rm = new ResourceManager("DigitalPlatform.OPAC.Web.res.ReservationInfoControl.cs",
                typeof(ReservationInfoControl).Module.Assembly);

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


        // 作为管理员身份此时要查看的读者证条码号。注意，不是指管理员自己的读者证
        // 存储在Session中
        public string ReaderBarcode
        {
            get
            {
                object o = this.Page.Session[this.ID + "ReservationInfoControl_readerbarcode"];
                if (o == null)
                    return "";
                return (string)o;
            }

            set
            {
                this.Page.Session[this.ID + "ReservationInfoControl_readerbarcode"] = value;
            }
        }


        // 预约信息内容行数
        public int ReservationLineCount
        {
            get
            {
                object o = this.Page.Session[this.ID + "ReservationInfoControl_ReservationLineCount"];
                return (o == null) ? 0 : (int)o;
            }
            set
            {
                this.Page.Session[this.ID + "ReservationInfoControl_ReservationLineCount"] = (object)value;
            }
        }

        // 已预约条码号列表
        public List<string> ReservationBarcodes
        {
            get
            {
                object o = this.Page.Session[this.ID + "ReservationInfoControl_ReservationBarcodes"];
                return (o == null) ? new List<string>() : (List<string>)o;
            }
            set
            {
                this.Page.Session[this.ID + "ReservationInfoControl_ReservationBarcodes"] = (object)value;
            }
        }

        // 布局控件
        protected override void CreateChildControls()
        {
            // 预约请求
            PlaceHolder reservation = new PlaceHolder();
            reservation.ID = "reservation";
            this.Controls.Add(reservation);

            CreateReservationControls(reservation);
        }


        // 创建初始的预约信息内部行布局
        void CreateReservationControls(PlaceHolder parent)
        {
            parent.Controls.Clear();    // ???

            PlaceHolder reservation = new PlaceHolder();
            reservation.ID = "reservation_content";
            parent.Controls.Add(reservation);

            //

            LiteralControl titleline = new LiteralControl();
            titleline.ID = "reservation_titleline";
            titleline.Text = "<div class='content_wrapper'>";    // cellpadding='0' cellspacing='0' border='0' width='100%'
            titleline.Text += "<table class='roundbar' cellpadding='0' cellspacing='0'>";    // cellpadding='0' cellspacing='0' border='0' width='100%'
            titleline.Text += "<tr class='titlebar'>"
                + "<td class='left'></td>"
                + "<td class='middle'>"
                + this.GetString("预约请求")
                + "</td>"
                + "<td class='right'></td>"
                + "</tr>";

            titleline.Text += "</table>";
            titleline.Text += "<table class='reservationinfo'>";

            titleline.Text += "<tr class='columntitle'><td nowrap>"
                + this.GetString("册条码号")
                + "</td><td nowrap>"
                + this.GetString("到达情况")
                + "</td><td nowrap width='50%'>"
                + this.GetString("摘要")
                + "</td><td nowrap>"
                + this.GetString("请求日期")
                + "</td><td nowrap>"
                + this.GetString("操作者")
                + "</td></tr>";
            reservation.Controls.Add(titleline);

            // 每一行一个占位控件
            for (int i = 0; i < this.ReservationLineCount; i++)
            {
                PlaceHolder line = NewReservationLine(reservation, i, null);
                line.Visible = true;
            }

            // 表格行插入点
            PlaceHolder insertpos = new PlaceHolder();
            insertpos.ID = "reservation_insertpos";
            reservation.Controls.Add(insertpos);

            // 命令行
            PlaceHolder cmdline = new PlaceHolder();
            cmdline.ID = "reservation_cmdline";
            reservation.Controls.Add(cmdline);

            LiteralControl literal = new LiteralControl();
            literal.Text = "<tr class='cmdline'><td colspan='5'>";
            cmdline.Controls.Add(literal);

            literal = new LiteralControl();
            literal.Text = "<div class='comment'>"
                + this.GetString("面板底部注释") // "(注：删除状态为“已到书”的行表示读者放弃取书。如果要去图书馆正常取书，请一定不要删除这样的行，待取书完成后软件会自动删除)"
                + "</div>";
            cmdline.Controls.Add(literal);

            Button reservationDeleteButton = new Button();
            reservationDeleteButton.ID = "reservation_deletebutton";
            reservationDeleteButton.Text = this.GetString("删除");
            reservationDeleteButton.CssClass = "deletebutton";
            reservationDeleteButton.Click -= new EventHandler(reservationDeleteButton_Click);
            reservationDeleteButton.Click += new EventHandler(reservationDeleteButton_Click);
            cmdline.Controls.Add(reservationDeleteButton);
            reservationDeleteButton = null;


            literal = new LiteralControl();
            literal.Text = "&nbsp;";
            cmdline.Controls.Add(literal);


            Button reservationMergeButton = new Button();
            reservationMergeButton.ID = "reservation_mergebutton";
            reservationMergeButton.Text = this.GetString("合并");
            reservationMergeButton.CssClass = "mergebutton";
            reservationMergeButton.Click -= new EventHandler(reservationMergeButton_Click);
            reservationMergeButton.Click += new EventHandler(reservationMergeButton_Click);
            cmdline.Controls.Add(reservationMergeButton);
            reservationMergeButton = null;

            literal = new LiteralControl();
            literal.Text = "&nbsp;";
            cmdline.Controls.Add(literal);


            Button reservationSplitButton = new Button();
            reservationSplitButton.ID = "reservation_splitbutton";
            reservationSplitButton.Text = this.GetString("拆散");
            reservationSplitButton.CssClass = "splitbutton";
            reservationSplitButton.Click -= new EventHandler(reservationSplitButton_Click);
            reservationSplitButton.Click += new EventHandler(reservationSplitButton_Click);
            cmdline.Controls.Add(reservationSplitButton);
            reservationSplitButton = null;


            literal = new LiteralControl();
            literal.Text = "</td></tr>";
            cmdline.Controls.Add(literal);

            cmdline = null;

            // 调试信息行
            PlaceHolder debugline = new PlaceHolder();
            debugline.ID = "debugline";
            reservation.Controls.Add(debugline);

            literal = new LiteralControl();
            literal.Text = "<tr class='debugline'><td colspan='5'>";
            debugline.Controls.Add(literal);

            literal = new LiteralControl();
            literal.ID = "debugtext";
            literal.Text = "";
            debugline.Controls.Add(literal);


            literal = new LiteralControl();
            literal.Text = "</td></tr>";
            debugline.Controls.Add(literal);

            debugline = null;


            // 表格结尾
            literal = new LiteralControl();
            literal.ID = "reservation_tableend";
            literal.Text = "</table>";
            reservation.Controls.Add(literal);

            literal = new LiteralControl();
            literal.Text = "</div>";
            reservation.Controls.Add(literal);

        }

        PlaceHolder NewReservationLine(Control parent,
int index,
Control insertbefore)
        {
            PlaceHolder line = new PlaceHolder();
            line.ID = "reservation_line" + Convert.ToString(index);

            if (FindControl(line.ID) != null)
                throw new Exception("id='" + line.ID + "' already existed");

            if (insertbefore == null)
                parent.Controls.Add(line);
            else
            {
                int pos = parent.Controls.IndexOf(insertbefore);
                parent.Controls.AddAt(pos, line);
            }

            // 左侧文字
            LiteralControl literal = new LiteralControl();
            literal.ID = "reservation_line" + Convert.ToString(index) + "left";
            literal.Text = "<tr><td>";
            line.Controls.Add(literal);

            // checkbox
            CheckBox checkbox = new CheckBox();
            checkbox.ID = "reservation_line" + Convert.ToString(index) + "checkbox";
            line.Controls.Add(checkbox);

            // 右侧文字
            literal = new LiteralControl();
            literal.ID = "reservation_line" + Convert.ToString(index) + "right";
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

        static string BuildBarcodeList(string strBarcodes,
            string strArrivedBarcode)
        {
            if (string.IsNullOrEmpty(strArrivedBarcode) == true)
                return strBarcodes;

            string[] barcodes = strBarcodes.Split(new char[] {','});
            string strResult = "";
            foreach (string barcode in barcodes)
            {
                if (string.IsNullOrEmpty(barcode) == true)
                    continue;
                if (string.IsNullOrEmpty(strResult) == false)
                    strResult += ",";

                if (barcode == strArrivedBarcode)
                {
                    strResult += "!" + barcode;
                }
                else
                    strResult += barcode;
            }

            return strResult;
        }

        void RenderReservation(OpacApplication app,
    SessionInfo sessioninfo,
    XmlDocument dom)
        {
            // 预约请求
            PlaceHolder reservation = (PlaceHolder)this.FindControl("reservation");
            this.ReservationBarcodes = new List<string>();

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("reservations/request");
            this.ReservationLineCount = nodes.Count;
            for (int i = 0; i < nodes.Count; i++)
            {
                PlaceHolder line = (PlaceHolder)reservation.FindControl("reservation_line" + Convert.ToString(i));
                if (line == null)
                {
                    Control insertpos = reservation.FindControl("reservation_insertpos");
                    line = NewReservationLine(insertpos.Parent, i, insertpos);
                    //this.ReservationLineCount++;
                }
                line.Visible = true;

                LiteralControl left = (LiteralControl)line.FindControl("reservation_line" + Convert.ToString(i) + "left");
                CheckBox checkbox = (CheckBox)line.FindControl("reservation_line" + Convert.ToString(i) + "checkbox");
                LiteralControl right = (LiteralControl)line.FindControl("reservation_line" + Convert.ToString(i) + "right");


                XmlNode node = nodes[i];
                string strBarcodes = DomUtil.GetAttr(node, "items");

                this.ReservationBarcodes.Add(strBarcodes);

                string strRequestDate = DateTimeUtil.LocalTime(DomUtil.GetAttr(node, "requestDate"));

                string strResult = "";

                string strTrClass = " class='dark' ";

                if ((i % 2) == 1)
                    strTrClass = " class='light' ";


                strResult += "<tr " + strTrClass + "><td nowrap>";

                // 左
                left.Text = strResult;

                // 右开始
                strResult = "&nbsp;";

                // 2007/1/18
                string strArrivedItemBarcode = DomUtil.GetAttr(node, "arrivedItemBarcode");

                //strResult += "" + strBarcodes + "</td>";
                int nBarcodesCount = GetBarcodesCount(strBarcodes);

                strResult += "" + MakeBarcodeListHyperLink(strBarcodes, strArrivedItemBarcode, ",")
                    + (nBarcodesCount > 1 ? " 之一" : "")  // 2007/7/5
                    + "</td>";

                // 操作者
                string strOperator = DomUtil.GetAttr(node, "operator");
                // 状态
                string strArrivedDate = DomUtil.GetAttr(node, "arrivedDate");
                string strState = DomUtil.GetAttr(node, "state");
                if (strState == "arrived")
                {
                    strArrivedDate = DateTimeUtil.LocalTime(strArrivedDate);
                    // text-level: 用户提示
                    strState = string.Format(this.GetString("册s已于s到书"),    // "册 {0} 已于 {1} 到书"
                        strArrivedItemBarcode,
                        strArrivedDate);

                    // "册 " + strArrivedItemBarcode + " 已于 " + strArrivedDate + " 到书";
                    if (nBarcodesCount > 1)
                    {
                        strState += string.Format(this.GetString("同一预约请求中的其余s册旋即失效"),  // "；同一预约请求中的其余 {0} 册旋即失效"
                            (nBarcodesCount - 1).ToString());

                        // "；同一预约请求中的其余 " + (nBarcodesCount - 1).ToString() + " 册旋即失效";  // 2007/7/5
                    }
                }
                strResult += "<td>" + strState + "</td>";

                /*
                string strSummary = GetBarcodesSummary(
                    app,
                    sessioninfo,
                    strBarcodes,
                    "html",
                    "");
                 * */

                strResult += "<td width='50%' class='pending'>formated:" + BuildBarcodeList(strBarcodes,
            strArrivedItemBarcode) + "</td>";
                strResult += "<td nowrap>" + strRequestDate + "</td>";
                strResult += "<td nowrap>" + strOperator + "</td>";
                strResult += "</tr>";

                right.Text = strResult;
            }

            // 把多余的行隐藏起来
            for (int i = nodes.Count; ; i++)
            {
                PlaceHolder line = (PlaceHolder)reservation.FindControl("reservation_line" + Convert.ToString(i));
                if (line == null)
                    break;

                line.Visible = false;
            }

            if (nodes.Count == 0)
            {

                Control insertpos = reservation.FindControl("reservation_insertpos");
                int pos = insertpos.Parent.Controls.IndexOf(insertpos);
                if (pos == -1)
                {
                    // text-level: 内部错误
                    throw new Exception("插入参照对象没有找到");
                }

                LiteralControl literal = new LiteralControl();
                literal.Text = "<tr class='dark'><td colspan='5'>"
                    + this.GetString("无预约信息") // "(无预约信息)"
                    + "<td></tr>";

                insertpos.Parent.Controls.AddAt(pos, literal);

            }
        }

        // 获得一系列册的摘要字符串
        // 
        // paramters:
        //      strStyle    风格。逗号间隔的列表。如果包含html text表示格式。forcelogin
        //      strOtherParams  <a>命令中其余的参数。例如" target='_blank' "可以用来打开新窗口
        public static string GetBarcodesSummary(
            OpacApplication app,
            // SessionInfo sessioninfo,
            LibraryChannel channel,
            string strBarcodes,
            string strArrivedItemBarcode,
            string strStyle,
            string strOtherParams)
        {
            string strSummary = "";

            if (strOtherParams == null)
                strOtherParams = "";

            string strDisableClass = "";
            if (string.IsNullOrEmpty(strArrivedItemBarcode) == false)
                strDisableClass = "deleted";


            bool bForceLogin = false;
            if (StringUtil.IsInList("forcelogin", strStyle) == true)
                bForceLogin = true;

            string strPrevBiblioRecPath = "";
            string[] barcodes = strBarcodes.Split(new char[] { ',' });
            for (int j = 0; j < barcodes.Length; j++)
            {
                string strBarcode = barcodes[j];
                if (String.IsNullOrEmpty(strBarcode) == true)
                    continue;

                // 获得摘要
                string strOneSummary = "";
                string strBiblioRecPath = "";

                string strError = "";
                long lRet = channel.GetBiblioSummary(
                    null,
                    strBarcode,
    null,
    strPrevBiblioRecPath,   // 前一个path
    out strBiblioRecPath,
    out strOneSummary,
    out strError);
                if (lRet == -1 || lRet == 0)
                    strOneSummary = strError;
                /*
                LibraryServerResult result = this.GetBiblioSummary(sessioninfo,
    strBarcode,
    null,
    strPrevBiblioRecPath,   // 前一个path
    out strBiblioRecPath,
    out strOneSummary);
                if (result.Value == -1 || result.Value == 0)
                    strOneSummary = result.ErrorInfo;
                 * */

                if (strOneSummary == ""
                    && strPrevBiblioRecPath == strBiblioRecPath)
                    strOneSummary = "(同上)";

                if (StringUtil.IsInList("html", strStyle) == true)
                {

                    string strBarcodeLink = "<a "
                        + (string.IsNullOrEmpty(strDisableClass) == false && strBarcode != strArrivedItemBarcode ? "class='" + strDisableClass + "'" : "")
                        + " href='book.aspx?barcode=" + strBarcode +
                        (bForceLogin == true ? "&forcelogin=userid" : "")
                        + "' " + strOtherParams + " >" + strBarcode + "</a>";

                    strSummary += strBarcodeLink + " : " + strOneSummary + "<br/>";
                }
                else
                {
                    strSummary += strBarcode + " : " + strOneSummary + "<br/>";
                }

                strPrevBiblioRecPath = strBiblioRecPath;
            }

            return strSummary;
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
                if (String.IsNullOrEmpty(this.ReaderBarcode) == true)
                {
                    // text-level: 内部错误
                    strError = "当前登录的用户不是reader类型，并且BorrowInfoControl.ReaderBarcode也为空";
                    goto ERROR1;
                }

                // TODO: 是否进一步判断Type
                // if (sessioninfo.Account.Type != "worreader")

                // 管理员获得特定证条码号的读者记录DOM
                // return:
                //      -2  当前登录的用户不是librarian类型
                //      -1  出错
                //      0   尚未登录
                //      1   成功
                nRet = sessioninfo.GetOtherReaderDom(
                    this.ReaderBarcode,
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

            // 兑现 预约信息
            RenderReservation(app,
                sessioninfo,
                readerdom);

            base.Render(output);
            return;

        ERROR1:
            output.Write(strError);
        }

        // 预约：拆散请求
        void reservationSplitButton_Click(object sender, EventArgs e)
        {
            string strBarcodeList = GetChekcedReservationBarcodes();

            if (String.IsNullOrEmpty(strBarcodeList) == true)
            {
                // text-level: 用户提示
                this.SetDebugInfo("errorinfo", this.GetString("尚未选择要拆散的预约事项"));  // "尚未选择要拆散的预约事项。"
                return;
            }

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            string strReaderBarcode = "";
            if (String.IsNullOrEmpty(this.ReaderBarcode) == false)
                strReaderBarcode = this.ReaderBarcode;
            else
                strReaderBarcode = sessioninfo.ReaderInfo.Barcode;

            if (String.IsNullOrEmpty(strReaderBarcode) == true)
            {
                // text-level: 用户提示
                this.SetDebugInfo("errorinfo", this.GetString("尚未指定读者证条码号。操作失败"));  // "尚未指定读者证条码号。操作失败。"
                return;
            }
            string strError = "";
            long lRet = sessioninfo.Channel.Reservation(
                null,
                "split",
                strReaderBarcode,
                strBarcodeList,
                out strError);
            if (lRet == -1)
                this.SetDebugInfo("errorinfo", strError);
            else
            {
                // text-level: 用户提示
                this.SetDebugInfo(this.GetString("拆散预约信息成功。请看预约列表"));    // "拆散预约信息成功。请看预约列表。"
            }
            /*
            LibraryServerResult result = app.Reservation(sessioninfo,
                "split",
                strReaderBarcode,
                strBarcodeList);
            if (result.Value == -1)
                this.SetDebugInfo("errorinfo", result.ErrorInfo);
            else
            {
                // text-level: 用户提示
                this.SetDebugInfo(this.GetString("拆散预约信息成功。请看预约列表"));    // "拆散预约信息成功。请看预约列表。"
            }
             * */

            // 清除读者记录缓存
            sessioninfo.ClearLoginReaderDomCache();
        }

        // 预约：合并请求
        void reservationMergeButton_Click(object sender, EventArgs e)
        {
            string strBarcodeList = GetChekcedReservationBarcodes();

            if (String.IsNullOrEmpty(strBarcodeList) == true)
            {
                // text-level: 用户提示
                this.SetDebugInfo("errorinfo", this.GetString("尚未选择要合并的预约事项"));  // "尚未选择要合并的预约事项。"
                return;
            }

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            string strReaderBarcode = "";
            if (String.IsNullOrEmpty(this.ReaderBarcode) == false)
                strReaderBarcode = this.ReaderBarcode;
            else
                strReaderBarcode = sessioninfo.ReaderInfo.Barcode;

            if (String.IsNullOrEmpty(strReaderBarcode) == true)
            {
                // text-level: 用户提示
                this.SetDebugInfo("errorinfo", this.GetString("尚未指定读者证条码号。操作失败"));  // "尚未指定读者证条码号。操作失败。"
                return;
            }

            string strError = "";
            long lRet = sessioninfo.Channel.Reservation(
                null,
                "merge",
                strReaderBarcode,
                strBarcodeList,
                out strError);
            if (lRet == -1)
                this.SetDebugInfo("errorinfo", strError);
            else
            {
                // text-level: 用户提示
                this.SetDebugInfo(this.GetString("合并预约信息成功。请看预约列表"));    // "合并预约信息成功。请看预约列表。"
            }
            /*
            LibraryServerResult result = app.Reservation(sessioninfo,
                "merge",
                strReaderBarcode,
                strBarcodeList);
            if (result.Value == -1)
                this.SetDebugInfo("errorinfo", result.ErrorInfo);
            else
            {
                // text-level: 用户提示
                this.SetDebugInfo(this.GetString("合并预约信息成功。请看预约列表"));    // "合并预约信息成功。请看预约列表。"
            }
             * */

            // 清除读者记录缓存
            sessioninfo.ClearLoginReaderDomCache();

        }

        // 预约：删除请求
        void reservationDeleteButton_Click(object sender, EventArgs e)
        {
            string strBarcodeList = GetChekcedReservationBarcodes();

            if (String.IsNullOrEmpty(strBarcodeList) == true)
            {
                // text-level: 用户提示
                this.SetDebugInfo("errorinfo", this.GetString("尚未选择要删除的预约事项"));  // "尚未选择要删除的预约事项。"
                return;
            }

            OpacApplication app = (OpacApplication)this.Page.Application["app"];
            SessionInfo sessioninfo = (SessionInfo)this.Page.Session["sessioninfo"];

            string strReaderBarcode = "";
            if (String.IsNullOrEmpty(this.ReaderBarcode) == false)
                strReaderBarcode = this.ReaderBarcode;
            else
                strReaderBarcode = sessioninfo.ReaderInfo.Barcode;

            if (String.IsNullOrEmpty(strReaderBarcode) == true)
            {
                // text-level: 用户提示
                this.SetDebugInfo("errorinfo", this.GetString("尚未指定读者证条码号。操作失败"));  // "尚未指定读者证条码号。操作失败。"
                return;
            }

            string strError = "";
            long lRet = sessioninfo.Channel.Reservation(
                null,
                "delete",
                strReaderBarcode,
                strBarcodeList,
                out strError);
            if (lRet == -1)
                this.SetDebugInfo("errorinfo", strError);
            else
            {
                // text-level: 用户提示
                string strMessage = this.GetString("删除预约信息成功。请看预约列表"); // "删除预约信息成功。请看预约列表。"

                // 成功时也可能有提示信息
                if (String.IsNullOrEmpty(strError) == false)
                    strMessage += "<br/><br/>" + strError;

                this.SetDebugInfo(strMessage);
            }
            /*
            LibraryServerResult result = app.Reservation(sessioninfo,
                "delete",
                strReaderBarcode,
                strBarcodeList);
            if (result.Value == -1)
                this.SetDebugInfo("errorinfo", result.ErrorInfo);
            else
            {
                // text-level: 用户提示
                string strMessage = this.GetString("删除预约信息成功。请看预约列表"); // "删除预约信息成功。请看预约列表。"

                // 成功时也可能有提示信息
                if (String.IsNullOrEmpty(result.ErrorInfo) == false)
                    strMessage += "<br/><br/>" + result.ErrorInfo;

                this.SetDebugInfo(strMessage);
            }
             * */

            // Button button = (Button)FindControl("reservation_deletebutton");

            // 清除读者记录缓存
            sessioninfo.ClearLoginReaderDomCache();
        }

        static int GetBarcodesCount(string strBarcodes)
        {
            string[] barcodes = strBarcodes.Split(new char[] { ',' });

            return barcodes.Length;
        }

        static string MakeBarcodeListHyperLink(string strBarcodes,
            string strArrivedItemBarcode,
    string strSep)
        {
            string strResult = "";
            string strDisableClass = "";
            if (string.IsNullOrEmpty(strArrivedItemBarcode) == false)
                strDisableClass = "deleted";
            string[] barcodes = strBarcodes.Split(new char[] { ',' });
            for (int i = 0; i < barcodes.Length; i++)
            {
                string strBarcode = barcodes[i];
                if (String.IsNullOrEmpty(strBarcode) == true)
                    continue;

                if (strResult != "")
                    strResult += strSep;
                strResult += "<a "
                    + (string.IsNullOrEmpty(strDisableClass) == false && strBarcode != strArrivedItemBarcode ? "class='" + strDisableClass + "'" : "")
                    + " href='book.aspx?barcode=" + strBarcode + "&forcelogin=on'>"
                    + strBarcode + "</a>";
            }

            return strResult;
        }

        string GetChekcedReservationBarcodes()
        {
            string strBarcodeList = "";

            PlaceHolder reservation = (PlaceHolder)this.FindControl("reservation");

            for (int i = 0; i < this.ReservationLineCount; i++)
            {
                CheckBox checkbox = (CheckBox)reservation.FindControl("reservation_line" + Convert.ToString(i) + "checkbox");
                if (checkbox.Checked == true)
                {
                    if (this.ReservationBarcodes.Count <= i)
                    {
                        //this.SetReservationDebugInfo("ReservationBarcodes失效...");
                        //return null;

                        // text-level: 内部错误
                        throw new Exception("ReservationBarcodes失效...");
                    }
                    string strBarcode = this.ReservationBarcodes[i];

                    if (strBarcodeList != "")
                        strBarcodeList += ",";
                    strBarcodeList += strBarcode;
                    checkbox.Checked = false;
                }
            }

            return strBarcodeList;
        }
    }
}

