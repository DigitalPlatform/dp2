// 读者XML记录转换为HTML显示格式
// 编写者：谢涛

// 修改历史：
// 2008/6/20 	对<overdue>元素内容的显示增加了comment列
// 2008/11/15 	超期信息 文字修改
// 2008/11/27 	对误用的<tr>进行了修改
// 2009/8/4 	将文字中的“条码”改为“条码号”
// 2010/5/14	将借阅历史表格中的借阅操作者栏的strOperator修改为strBorrowOperator
// 2011/2/14	将<head>中<link>和<script>改到从前端本地文件获得
//		将App.LibraryServerUrl改为App.OpacServerUrl
// 2011/2/20    将调用readerinfoex.aspx修改为readerinfo.aspx
// 2011/9/3     加入读者照片<img>元素
// 2011/9/5     改用StringBuilder处理字符串
// 2011/9/24    增加支持为保存的记录HTML预览功能
// 2012/1/6     将读者证照片的Ajax获取字符串从证条码号修改为"object-path:"方式。注：原来的证条码号方式依然可选用。
// 2013/1/16    显示指纹图标
// 2013/12/4    识别 this.Formats 中的 noborrowhistory
// 2013/12/25   this.App.MaxPatronHistoryItems
// 2014/11/8    读者照片加入 img 元素加入 pending class 
// 2014/12/27	BC:xxxxx 借阅信息列表中摘要包含图书封面

using System;
using System.Xml;
using System.Web;
using System.Text;

using DigitalPlatform.LibraryServer;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

public class MyConverter : ReaderConverter
{
    public override string Convert(string strXml)
    {
        string strError = "";
        int nRet = 0;

        XmlDocument dom = new XmlDocument();

        try
        {
            dom.LoadXml(strXml);
        }
        catch (Exception ex)
        {
            return ex.Message;
        }
        string strLink = "<link href='%mappeddir%\\styles\\readerhtml.css' type='text/css' rel='stylesheet' />"
            + "<link href=\"%mappeddir%/jquery-ui-1.8.7/css/jquery-ui-1.8.7.css\" rel=\"stylesheet\" type=\"text/css\" />"
            + "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-1.4.4.min.js\"></script>"
            + "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-ui-1.8.7.min.js\"></script>"
            + "<meta http-equiv=\"X-UA-Compatible\" content=\"IE=9\"></meta>"
            + "<meta http-equiv='Content-type' content='text/html; charset=utf-8' ></meta>"
            + "<script type='text/javascript' charset='UTF-8' src='%mappeddir%\\scripts\\readerxml2html.js" + "'></script>"
            + "<script type='text/javascript' charset='UTF-8' src='%mappeddir%\\scripts\\getsummary.js" + "'></script>";

        // 证状态
        string strReaderState = DomUtil.GetElementText(dom.DocumentElement, "state");
        string strExpireDate = DomUtil.GetElementText(dom.DocumentElement, "expireDate");

        string strBodyClass = "";

        // return:
        //      -1  检测过程发生了错误。应当作不能借阅来处理
        //      0   可以借阅
        //      1   证已经过了失效期，不能借阅
        //      2   证有不让借阅的状态
        nRet = this.App.CheckReaderExpireAndState(dom, out strError);
        if (nRet != 0)
            strBodyClass = "warning";

        bool bExpired = false;
        if (nRet == 1)
            bExpired = true;

        StringBuilder strResult = new StringBuilder(4096);

        strResult.Append("<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\">");

        strResult.Append( "<html xmlns=\"http://www.w3.org/1999/xhtml\">\r\n\t<head>" + strLink + "</head>\t\n\t<body"
            + (string.IsNullOrEmpty(strBodyClass) == false ? " class='" + strBodyClass + "'" : "")
            + ">");

        // 左右分布的大表格
        strResult.Append( "\r\n\t\t<table class='layout'>");
        strResult.Append( "\r\n\t\t\t<tr class='content'>");

        string strReaderBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
        string strPersonName = DomUtil.GetElementText(dom.DocumentElement, "name");

        string strFingerprint = DomUtil.GetElementText(dom.DocumentElement, "fingerprint");

        string strPhotoPath = "";
        if (this.RecPath != "?")
            strPhotoPath = GetPhotoPath(dom, this.RecPath);

        strResult.Append("<td class='photo'>");
        strResult.Append("<img id='cardphoto' class='pending' name='"
            + (this.RecPath == "?" ? "?" : "object-path:" + strPhotoPath) // 这里直接用读者证条码号也可以,只不过前端处理速度稍慢
            + "' src='%mappeddir%\\images\\ajax-loader.gif' alt='" + HttpUtility.HtmlEncode(strPersonName) + " 的照片'></img>");
        if (string.IsNullOrEmpty(strFingerprint) == false)
        {
            strResult.Append("<img src='%mappeddir%\\images\\fingerprint.png' alt='有指纹信息'>");
        }
        strResult.Append("</td>");
        strResult.Append("<td class='warning' id='insertpoint'></td>");

        // 识别信息表格
        strResult.Append("\r\n\t\t\t\t<td class='left'><table class='readerinfo'>");


        // 证条码号
        string strReaderBarcodeLink = "<a href='javascript:void(0);' onclick=\"window.external.OpenForm('ReaderInfoForm', this.innerText, true);\">" + strReaderBarcode + "</a>";
        strResult.Append("<tr class='content barcode'><td class='name'>读者证条码号</td><td class='value'>" + strReaderBarcodeLink + "</td></tr>");

        // 读者类别		
        string strReaderType = DomUtil.GetElementText(dom.DocumentElement, "readerType");

        strResult.Append( "<tr class='content readertype'><td class='name'>读者类别</td><td class='value'>" + strReaderType + "</td></tr>");


        // 姓名
        strResult.Append( "<tr class='content name'><td class='name'>姓名</td><td class='value' >" + strPersonName
    + "</td></tr>");

        // 补齐高度
        strResult.Append("<tr class='content blank'><td class='name'></td><td class='value' ></td></tr>");

        strResult.Append( "</table></td>");

        strResult.Append( "<td class='middle'>&nbsp;</td>");

        strResult.Append( "<td class='right'>");

        strResult.Append( "<table class='readerinfo'>");

        // 证状态
        strResult.Append( "<tr class='content state'><td class='name'>证状态</td><td class='value'>"
 + strReaderState + "</td></tr>");

        /*
                // 发证日期
                strResult.Append( "<tr class='content createdate'><td class='name'>发证日期</td><td class='value'>" + LocalDate(DomUtil.GetElementText(dom.DocumentElement, "createDate")) + "</td></tr>");
        */


        // 失效日期
        string strExpireDateValueClass = "expiredate";
        if (bExpired == true)
            strExpireDateValueClass = "expireddate";

        strResult.Append( "<tr class='content " + strExpireDateValueClass + "'><td class='name'>失效日期</td><td class='value'>" + LocalDate(strExpireDate) + "</td></tr>");

        strResult.Append( "<tr class='content department'><td class='name'>单位</td><td class='value'>"
 + DomUtil.GetElementText(dom.DocumentElement, "department") + "</td></tr>");

        strResult.Append("<tr class='content comment'><td class='name'>注释</td><td class='value'><div class='wide'><div>"
 + DomUtil.GetElementText(dom.DocumentElement, "comment") + "</td></tr>");

        // 补齐高度
        strResult.Append("<tr class='content blank'><td class='name'></td><td class='value' ></td></tr>");

        strResult.Append( "</table>");

        strResult.Append( "</td></tr>");

        // 大表格收尾
        strResult.Append( "</table>");
        
        // 获得日历
        Calendar calendar = null;
        nRet = this.App.GetReaderCalendar(strReaderType,
            this.LibraryCode,
            out calendar,
            out strError);
        if (nRet == -1)
        {
            strResult.Append(strError);
            calendar = null;
        }

        string strWarningText = "";

        // ***
        // 违约/交费信息
        XmlNodeList nodes = dom.DocumentElement.SelectNodes("overdues/overdue");

        if (nodes.Count > 0)
        {
            strResult.Append( "<div class='tabletitle'>违约/交费信息</div>");
            strResult.Append( "<table class='overdue'>");
            strResult.Append( "<tr class='columntitle'><td>册条码号</td><td>说明</td><td>金额</td><td nowrap>以停代金情况</td><td>起点日期</td><td>期限</td><td>终点日期</td><td>ID</td><td>注释</td></tr>");

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strBarcode = DomUtil.GetAttr(node, "barcode");
                string strOver = DomUtil.GetAttr(node, "reason");

                string strBorrowPeriod = DomUtil.GetAttr(node, "borrowPeriod");

                string strBorrowDate = LocalDateOrTime(DomUtil.GetAttr(node, "borrowDate"), strBorrowPeriod);

                string strReturnDate = LocalDateOrTime(DomUtil.GetAttr(node, "returnDate"), strBorrowPeriod);
                string strID = DomUtil.GetAttr(node, "id");
                string strPrice = DomUtil.GetAttr(node, "price");
                string strOverduePeriod = DomUtil.GetAttr(node, "overduePeriod");

                // 把一行文字变为两行显示
                strBorrowDate = strBorrowDate.Replace(" ", "<br/>");
                strReturnDate = strReturnDate.Replace(" ", "<br/>");

                strID = SplitTwoLine(strID);

                string strComment = DomUtil.GetAttr(node, "comment");
                if (String.IsNullOrEmpty(strComment) == true)
                    strComment = "&nbsp;";

                // string strBarcodeLink = "<a href='" + App.OpacServerUrl + "/book.aspx?barcode=" + strBarcode + "&forcelogin=userid' target='_blank'>" + strBarcode + "</a>";
                string strBarcodeLink = "<a href='javascript:void(0);' onclick=\"window.external.OpenForm('ItemInfoForm', this.innerText, true);\"  onmouseover=\"window.external.HoverItemProperty(this.innerText);\">" + strBarcode + "</a>";

                string strPauseInfo = "";

                if (StringUtil.IsInList("pauseBorrowing", this.App.OverdueStyle) == true
                    && String.IsNullOrEmpty(strOverduePeriod) == false)
                {
                    string strPauseStart = DomUtil.GetAttr(node, "pauseStart");

                    if (String.IsNullOrEmpty(strPauseStart) == false)
                    {
                        strPauseInfo = "从 " + DateTimeUtil.LocalDate(strPauseStart) + " 开始，";
                    }

                    string strUnit = "";
                    long lOverduePeriod = 0;

                    // 分析期限参数
                    nRet = LibraryApplication.ParsePeriodUnit(strOverduePeriod,
                        out lOverduePeriod,
                        out strUnit,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "在分析期限参数的过程中发生错误: " + strError;
                        strResult.Append( strError);
                    }

                    long lResultValue = 0;
                    string strPauseCfgString = "";
                    nRet = this.App.ComputePausePeriodValue(strReaderType,
                        this.LibraryCode,
                            lOverduePeriod,
                            out lResultValue,
                        out strPauseCfgString,
                            out strError);
                    if (nRet == -1)
                    {
                        strError = "在计算以停代金周期的过程中发生错误: " + strError;
                        strResult.Append( strError);
                    }

                    strPauseInfo += "停借期 " + lResultValue.ToString() + LibraryApplication.GetDisplayTimeUnit(strUnit) + " (计算过程如下: 超期 " + lOverduePeriod.ToString() + LibraryApplication.GetDisplayTimeUnit(strUnit) + "，读者类型 " + strReaderType + " 的 以停代金因子 为 " + strPauseCfgString + ")";
                }

                strResult.Append( "<tr class='content'>");
                strResult.Append( "<td class='barcode' >" + strBarcodeLink + "</td>");
                strResult.Append( "<td class='reason'><div class='wide'></div>" + strOver + "</td>");
                strResult.Append( "<td class='price' >" + strPrice + "</td>");
                strResult.Append( "<td class='pauseinfo'>" + strPauseInfo + "</td>");
                strResult.Append( "<td class='borrowdate' >" + strBorrowDate + "</td>");
                strResult.Append( "<td class='borrowperiod' >" + LibraryApplication.GetDisplayTimePeriodString(strBorrowPeriod) + "</td>");
                strResult.Append( "<td class='returndate' >" + strReturnDate + "</td>");
                strResult.Append( "<td class='id' >" + strID + "</td>");
                strResult.Append( "<td class='comment' width='30%'>" + strComment + "</td>");
                strResult.Append( "</tr>");
            }

            if (StringUtil.IsInList("pauseBorrowing", this.App.OverdueStyle) == true)
            {

                // 汇报以停代金情况
                string strPauseMessage = "";
                nRet = App.HasPauseBorrowing(
                    calendar,
                    this.LibraryCode,
                    dom,
                    out strPauseMessage,
                    out strError);
                if (nRet == -1)
                {
                    strError = "在计算以停代金的过程中发生错误: " + strError;
                    strResult.Append( strError);
                }
                if (nRet == 1)
                {
                    strResult.Append( "<td colspan='8'>" + strPauseMessage + "</td>");	// ???
                }
            }

            strResult.Append( "</table>");


            strWarningText += "<div class='warning amerce'><div class='number'>"+nodes.Count.ToString()+"</div><div class='text'>待交费</div></div>";
        }

        // ***
        // 借阅的册
        strResult.Append( "<div class='tabletitle'>借阅信息</div>");

        nodes = dom.DocumentElement.SelectNodes("borrows/borrow");
        int nBorrowCount = nodes.Count;

        strResult.Append( "<table class='borrowinfo'>");

        strResult.Append( "<tr class='borrow_count'><td colspan='9' class='borrow_count'>");

        string strMaxItemCount = GetParam(strReaderType, "", "可借总册数");
        strResult.Append( "最多可借:" + strMaxItemCount + " ");

        int nMax = 0;
        try
        {
            nMax = System.Convert.ToInt32(strMaxItemCount);
        }
        catch
        {
            strResult.Append( "当前读者 可借总册数 参数 '" + strMaxItemCount + "' 格式错误");
            goto CONTINUE1;
        }

        strResult.Append( "当前可借:" + System.Convert.ToString(Math.Max(0, nMax - nodes.Count)) + "");

    CONTINUE1:

        int nOverdueCount = 0;
        strResult.Append( "</td></tr>");

        if (nodes.Count > 0)
        {
            strResult.Append( "<tr class='columntitle'><td>册条码号</td><td>摘要</td><td>价格</td><td>续借次</td><td>借阅日期</td><td>期限</td><td>操作者</td><td>应还日期</td><td>续借注</td></tr>");

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strBarcode = DomUtil.GetAttr(node, "barcode");
                string strNo = DomUtil.GetAttr(node, "no");
                string strBorrowDate = DomUtil.GetAttr(node, "borrowDate");
                string strPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                string strOperator = DomUtil.GetAttr(node, "operator");
                string strRenewComment = DomUtil.GetAttr(node, "renewComment");

                string strConfirmItemRecPath = DomUtil.GetAttr(node, "recPath");
                string strPrice = DomUtil.GetAttr(node, "price");
                string strTimeReturning = DomUtil.GetAttr(node, "timeReturning");


#if NO
                string strOverDue = "";
                bool bOverdue = false;
                // 检查超期情况。
                // return:
                //      -1  数据格式错误
                //      0   没有发现超期
                //      1   发现超期   strError中有提示信息
                nRet = App.CheckPeriod(
                calendar,
                strBorrowDate,
                strPeriod,
                out strError);
                if (nRet == -1)
                    strOverDue = strError;
                else if (nRet == 1)
                {
                    strOverDue = strError;	// "已超期";
                    bOverdue = true;
                }
                else
                {
                    strOverDue = "<a title='" + strError + "'>" + LocalDate(strTimeReturning) + "</a>";
                    // strOverDue = strError;	// 可能也有一些必要的信息，例如非工作日
                }
#endif
                    string strOverDue = "";
                    bool bOverdue = false;  // 是否超期
                {

                    DateTime timeReturning = DateTime.MinValue;
                    string strTips = "";

                    DateTime timeNextWorkingDay;
                    long lOver = 0;
                    string strPeriodUnit = "";

                    // 获得还书日期
                    // return:
                    //      -1  数据格式错误
                    //      0   没有发现超期
                    //      1   发现超期   strError中有提示信息
                    //      2   已经在宽限期内，很容易超期 
                    nRet = this.App.GetReturningTime(
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
                        strOverDue = timeReturning.ToString("d");
                        if (nRet == 1)
                        {
                            nOverdueCount++;
                            bOverdue = true;
                            strOverDue += " ("
                                + string.Format(this.App.GetString("已超期s"),  // 已超期 {0}
                                                this.App.GetDisplayTimePeriodStringEx(lOver.ToString() + " " + strPeriodUnit))
                                + ")";
                        }

                        strOverDue = "<a title='" + strError.Replace("'","\"") + "'>" + strOverDue + "</a>";
                    }
                }


                // string strBarcodeLink = "<a href='" + App.OpacServerUrl + "/book.aspx?barcode=" + strBarcode + "&borrower=" + strReaderBarcode + "&forcelogin=userid' target='_blank'>" + strBarcode + "</a>";
                string strBarcodeLink = "<a href='javascript:void(0);' onclick=\"window.external.OpenForm('ItemInfoForm', this.innerText, true);\"  onmouseover=\"window.external.HoverItemProperty(this.innerText);\">" + strBarcode + "</a>";

                /* strResult.Append( "<tr class='content' "+strColor+" nowrap>"); */
                if (bOverdue == true)
                    strResult.Append( "<tr class='content overdue'>");
                else
                    strResult.Append( "<tr class='content'>");
                strResult.Append( "<td class='barcode' nowrap>" + strBarcodeLink + "</td>");
                strResult.Append( "<td class='summary pending'><br/>BC:" + strBarcode + "|" + strConfirmItemRecPath + "</td>");

                strResult.Append( "<td class='price' nowrap align='right'>" + strPrice + "</td>");
                strResult.Append( "<td class='no' nowrap align='right'>" + strNo + "</td>");
                strResult.Append( "<td class='borrowdate' >" + LocalDateOrTime(strBorrowDate, strPeriod) + "</td>");
                strResult.Append( "<td class='period' nowrap>" + LibraryApplication.GetDisplayTimePeriodString(strPeriod) + "</td>");
                strResult.Append( "<td class='operator' nowrap>" + strOperator + "</td>");
                strResult.Append( "<td class='returndate' width='30%'>" + strOverDue + "</td>");
                strResult.Append( "<td class='renewcomment' width='30%'>" + strRenewComment.Replace(";", "<br/>") + "</td>");
                strResult.Append( "</tr>");
            }

        }


        strResult.Append( "</table>");

        if (nOverdueCount > 0)
            strWarningText += "<div class='warning overdue'><div class='number'>" + nOverdueCount.ToString() + "</div><div class='text'>已超期</div></div>";


        // ***
        // 预约请求
        strResult.Append( "<div class='tabletitle'>预约请求</div>");
        nodes = dom.DocumentElement.SelectNodes("reservations/request");

        if (nodes.Count > 0)
        {
            int nArriveCount = 0;

            strResult.Append( "<table class='reservation'>");
            strResult.Append( "<tr class='columntitle'><td nowrap>册条码号</td><td nowrap>到达情况</td><td nowrap>摘要</td><td nowrap>请求日期</td><td nowrap>操作者</td></tr>");

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strBarcodes = DomUtil.GetAttr(node, "items");
                string strRequestDate = LocalTime(DomUtil.GetAttr(node, "requestDate"));

                string strOperator = DomUtil.GetAttr(node, "operator");
                string strArrivedItemBarcode = DomUtil.GetAttr(node, "arrivedItemBarcode");

                string strSummary = this.App.GetBarcodesSummary(
                    this.SessionInfo,
                    strBarcodes,
                    strArrivedItemBarcode,
            "html", // "html,forcelogin",
            ""/*"target='_blank'"*/);

                string strClass = "content";

                int nBarcodesCount = GetBarcodesCount(strBarcodes);
                // 状态
                string strArrivedDate = DomUtil.GetAttr(node, "arrivedDate");
                string strState = DomUtil.GetAttr(node, "state");
                if (strState == "arrived")
                {
                    strArrivedDate = ItemConverter.LocalTime(strArrivedDate);
                    strState = "册 " + strArrivedItemBarcode + " 已于 " + strArrivedDate + " 到书";

                    if (nBarcodesCount > 1)
                    {
                        strState += string.Format("; 同一预约请求中的其余 {0} 册旋即失效",  // "；同一预约请求中的其余 {0} 册旋即失效"
                            (nBarcodesCount - 1).ToString());
                    }
                    strClass = "content active";

                    nArriveCount++;
                }


                strResult.Append( "<tr class='" + strClass + "'>");
                strResult.Append( "<td class='barcode'>"
                    + MakeBarcodeListHyperLink(strBarcodes, strArrivedItemBarcode, ", ")
                    + (nBarcodesCount > 1 ? " 之一" : "")
                    + "</td>");
                strResult.Append( "<td class='state'>" + strState + "</td>");
                strResult.Append( "<td class='summary'>" + strSummary + "</td>");
                strResult.Append( "<td class='requestdate'>" + strRequestDate + "</td>");
                strResult.Append( "<td class='operator'>" + strOperator + "</td>");
                strResult.Append( "</tr>");

            }
            strResult.Append( "</table>");

            if (nArriveCount > 0)
                strWarningText += "<div class='warning arrive'><div class='number'>" + nArriveCount.ToString() + "</div><div class='text'>预约到书</div></div>";
        }

        if (string.IsNullOrEmpty(strWarningText) == false)
            strResult.Append("<div id='warningframe'>" + strWarningText + "</div>");

        // ***
        // 借阅历史

        if (StringUtil.IsInList("noborrowhistory", this.Formats) == false)
        {
            strResult.Append("<div class='tabletitle'>借阅历史</div>");

            nodes = dom.DocumentElement.SelectNodes("borrowHistory/borrow");

            if (nodes.Count > 0)
            {
                strResult.Append("<table class='borrowhistory'>");

                strResult.Append("<tr class='borrow_count'><td colspan='10' class='borrow_count'>");

                XmlNode nodeHistory = dom.DocumentElement.SelectSingleNode("borrowHistory");
                string strHistoryCount = "";
                if (nodeHistory != null)
                    strHistoryCount = DomUtil.GetAttr(nodeHistory, "count");
                strResult.Append("共借阅: " + strHistoryCount + " 册 (下面表格中仅显示了最近借阅过的最多 " + this.App.MaxPatronHistoryItems.ToString() + " 册)");

                strResult.Append("</td></tr>");

                strResult.Append("<tr class='columntitle'><td nowrap>序</td><td nowrap>册条码号</td><td nowrap>摘要</td><td nowrap>续借次</td><td nowrap>借阅日期</td><td nowrap>期限</td><td nowrap>借阅操作者</td><td nowrap>续借注</td><td nowrap>还书日期</td><td nowrap>还书操作者</td></tr>");

                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlNode node = nodes[i];
                    string strBarcode = DomUtil.GetAttr(node, "barcode");
                    string strNo = DomUtil.GetAttr(node, "no");
                    string strBorrowDate = DomUtil.GetAttr(node, "borrowDate");
                    string strPeriod = DomUtil.GetAttr(node, "borrowPeriod");
                    string strBorrowOperator = DomUtil.GetAttr(node, "borrowOperator");	// 借书操作者
                    string strOperator = DomUtil.GetAttr(node, "operator");	// 还书操作者
                    string strRenewComment = DomUtil.GetAttr(node, "renewComment");
                    // string strSummary = "";
                    string strConfirmItemRecPath = DomUtil.GetAttr(node, "recPath");
                    string strReturnDate = DomUtil.GetAttr(node, "returnDate");

                    // string strBarcodeLink = "<a href='" + App.OpacServerUrl + "/book.aspx?barcode=" + strBarcode + "&borrower=" + strReaderBarcode + "&forcelogin=userid' target='_blank'>" + strBarcode + "</a>";
                    string strBarcodeLink = "<a href='javascript:void(0);'  onclick=\"window.external.OpenForm('ItemInfoForm', this.innerText, true);\" onmouseover=\"OnHover(this.innerText);\">" + strBarcode + "</a>";

                    strResult.Append("<tr class='content'>");
                    strResult.Append("<td class='index' nowrap>" + (i + 1).ToString() + "</td>");
                    strResult.Append("<td class='barcode' nowrap>" + strBarcodeLink + "</td>");
                    strResult.Append("<td class='summary pending'>BC:" + strBarcode + "|" + strConfirmItemRecPath + "</td>");

                    strResult.Append("<td class='no' nowrap align='right'>" + strNo + "</td>");
                    strResult.Append("<td class='borrowdate' >" + LocalDateOrTime(strBorrowDate, strPeriod) + "</td>");
                    strResult.Append("<td class='period' nowrap>" + LibraryApplication.GetDisplayTimePeriodString(strPeriod) + "</td>");
                    strResult.Append("<td class='borrowoperator' nowrap>" + strBorrowOperator + "</td>");
                    strResult.Append("<td class='renewcomment' width='30%'>" + strRenewComment.Replace(";", "<br/>") + "</td>");
                    strResult.Append("<td class='returndate' nowrap>" + LocalDateOrTime(strReturnDate, strPeriod) + "</td>");
                    strResult.Append("<td class='operator' nowrap>" + strOperator + "</td>");
                    strResult.Append("</tr>");
                }

                strResult.Append("</table>");
            }
        }

        strResult.Append( "</body></html>");

        return strResult.ToString();
    }

    static int GetBarcodesCount(string strBarcodes)
    {
        string[] barcodes = strBarcodes.Split(new char[] { ',' });

        return barcodes.Length;
    }

    string MakeBarcodeListHyperLink(string strBarcodes,
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
                + " href='javascript:void(0);' onclick=\"window.external.OpenForm('ItemInfoForm', this.innerText, true);\"  onmouseover=\"window.external.HoverItemProperty(this.innerText);\">" + strBarcode + "</a>";
        }

        return strResult;
    }

    // 把一行文字按照平分原则变为两行显示
    static string SplitTwoLine(string strText)
    {
        if (strText.Length < 6)
            return strText;

        int nLeftLen = 0;

        nLeftLen = strText.Length / 2;
        if ((strText.Length % 2) == 1)
            nLeftLen++;
        return strText.Substring(0, nLeftLen) + "<br/>" + strText.Substring(nLeftLen);
    }

    // 根据strPeriod中的时间单位(day/hour)，返回本地日期或者时间字符串
    static string LocalDateOrTime(string strTimeString, string strPeriod)
    {
        string strError = "";
        long lValue = 0;
        string strUnit = "";
        int nRet = LibraryApplication.ParsePeriodUnit(strPeriod,
                    out lValue,
                    out strUnit,
                    out strError);
        if (nRet == -1)
            strUnit = "day";
        if (strUnit == "day")
            return LocalDate(strTimeString);

        return LocalTime(strTimeString);
    }

    // 2012/1/6
    static string GetPhotoPath(XmlDocument readerdom,
        string strRecPath)
    {
        XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
        nsmgr.AddNamespace("dprms", DpNs.dprms);

        XmlNodeList nodes = readerdom.DocumentElement.SelectNodes("//dprms:file[@usage='cardphoto']", nsmgr);

        if (nodes.Count == 0)
            return null;

        string strID = DomUtil.GetAttr(nodes[0], "id");
        if (string.IsNullOrEmpty(strID) == true)
            return null;

        string strResPath = strRecPath + "/object/" + strID;
        return strResPath.Replace(":", "/");
    }

}