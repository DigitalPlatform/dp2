// 册XML记录转换为HTML显示格式
// 编写者：谢涛
// 最后修改日期: 2011/2/20

// 修改历史：
// 2010/5/14	将借阅操作者栏的strOperator修改为strBorrowOperator
// 2011/2/15	将<head>中<link>和<script>改到从前端本地文件获得
//		将App.LibraryServerUrl改为App.OpacServerUrl
// 2011/2/20    将调用readerinfoex.aspx修改为readerinfo.aspx
// 2011/9/5
// 2011/9/7
// 2013/12/25   this.App.MaxItemHistoryItems

using System;
using System.Xml;

using DigitalPlatform.LibraryServer;
using DigitalPlatform.Xml;

public class MyConverter : ItemConverter
{
    public override void Item(object sender, ItemConverterEventArgs e)
    {
        XmlDocument dom = new XmlDocument();

        try
        {
            dom.LoadXml(e.Xml);
        }
        catch (Exception ex)
        {
            e.ResultString = ex.Message;
            return;
        }

        string strResult = "<html>";

        strResult += "<head>";
        strResult += "<link href='%mappeddir%\\styles\\itemhtml.css' type='text/css' rel='stylesheet' />";
        strResult += "<link href=\"%mappeddir%/jquery-ui-1.8.7/css/jquery-ui-1.8.7.css\" rel=\"stylesheet\" type=\"text/css\" />"
        + "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-1.4.4.min.js\"></script>"
        + "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-ui-1.8.7.min.js\"></script>"
        + "<script type='text/javascript' charset='UTF-8' src='%mappeddir%\\scripts\\getsummary.js" + "'></script>";



        strResult += "</head>";
        strResult += "<body>";

        strResult += "<table class='iteminfo'>";

        {

            // 册条码
            string strItemBarcode = DomUtil.GetElementText(dom.DocumentElement, "barcode");
            /*
            string strBarcodeLink = "<a href='" + App.OpacServerUrl + "/book.aspx?barcode=" + strItemBarcode + "&forcelogin=userid' target='_blank'>"
                + strItemBarcode
                + "</a>";
             * */
            string strBarcodeLink = "<a href='javascript:void(0);' onclick=\"window.external.OpenForm('ItemInfoForm', this.innerText, true);\">" + strItemBarcode + "</a>";
            strResult += GetOneTR("barcode", "册条码号", strBarcodeLink);

            // 馆藏地点
            strResult += GetOneTR(dom.DocumentElement, "location", "馆藏地点");

            // 书目摘要
            strResult += "<tr class='content summary'>";
            strResult += "<td class='name summary' nowrap>";
            strResult += "书目摘要";
            strResult += "</td>";
            strResult += "<td class='value summary pending'>";
            strResult += "B:" + strItemBarcode + "|" + e.RecPath;
            strResult += "</td></tr>";

            // 状态
            strResult += GetOneTR(dom.DocumentElement, "state", "状态");


            // 册价格
            strResult += GetOneTR(dom.DocumentElement, "price", "册价格");

            // 出版时间
            strResult += GetOneTR(dom.DocumentElement, "publishTime", "出版时间");

            // 渠道
            strResult += GetOneTR(dom.DocumentElement, "seller", "渠道");

            // 经费来源
            strResult += GetOneTR(dom.DocumentElement, "source", "经费来源");

            // 索取号
            strResult += GetOneTR(dom.DocumentElement, "accessNo", "索取号");

            // 卷
            strResult += GetOneTR(dom.DocumentElement, "volume", "卷");

            // 册类型
            strResult += GetOneTR(dom.DocumentElement, "bookType", "册类型");

            // 登录号
            strResult += GetOneTR(dom.DocumentElement, "registerNo", "登录号");

            // 注释
            strResult += GetOneTR(dom.DocumentElement, "comment", "注释");

            // 合并注释
            strResult += GetOneTR(dom.DocumentElement, "mergeComment", "合并注释");

            // 批次号
            strResult += GetOneTR(dom.DocumentElement, "batchNo", "批次号");

            string strBorrower = DomUtil.GetElementText(dom.DocumentElement, "borrower");	// 借者条码

            // 借者姓名
            strResult += "<tr class='content patronname'>";
            strResult += "<td class='name patronname' nowrap>";
            strResult += "借者姓名";
            strResult += "</td>";
            if (string.IsNullOrEmpty(strBorrower) == false)
            {
                strResult += "<td class='value patronname pending'>";
                strResult += "P:" + strBorrower;
            }
            else
            {
                strResult += "<td class='value patronname'>";
                strResult += "&nbsp;";
            }
            strResult += "</td></tr>";

            // 借者条码

            string strBorrowerLink = "";
            if (String.IsNullOrEmpty(strBorrower) == false)
            {
                /*
                strBorrowerLink = "<a href='" + App.OpacServerUrl + "/readerinfo.aspx?barcode=" + strBorrower + "&forcelogin=userid' target='_blank'>"
                    + strBorrower
                    + "</a>";
                 * */
                strBorrowerLink = "<a href='javascript:void(0);' onclick=\"window.external.OpenForm('ReaderInfoForm', this.innerText, true);\">" + strBorrower + "</a>";
            }
            else
                strBorrowerLink = "&nbsp";

            strResult += GetOneTR("borrower", "借者证条码号", strBorrowerLink);

            // 借阅日期
            string strBorrowDate = DomUtil.GetElementText(dom.DocumentElement, "borrowDate");
            strBorrowDate = LocalTime(strBorrowDate);
            strResult += GetOneTR("borrowDate", "借阅日期", strBorrowDate);

            // 借阅期限
            string strBorrowPeriod = DomUtil.GetElementText(dom.DocumentElement, "borrowPeriod");
            strBorrowPeriod = LibraryApplication.GetDisplayTimePeriodString(strBorrowPeriod);
            strResult += GetOneTR("borrowPeriod", "借阅期限", strBorrowPeriod);

            // 参考ID
            strResult += GetOneTR(dom.DocumentElement, "refID", "参考ID");

            // 册记录路径
            strResult += GetOneTR("recpath", "册记录路径", e.RecPath);

            strResult += "</table>";
        }


        // 借阅历史
        XmlNodeList nodes = dom.DocumentElement.SelectNodes("borrowHistory/borrower");

        if (nodes.Count > 0)
        {
            strResult += "<br/><b>历史</b><br/>";
            strResult += "<table class='borrowhistory'>\r\n";

            strResult += "<tr class='borrowcount'><td colspan='10' class='borrowcount'>";

            XmlNode nodeHistory = dom.DocumentElement.SelectSingleNode("borrowHistory");
            string strHistoryCount = "";
            if (nodeHistory != null)
                strHistoryCount = DomUtil.GetAttr(nodeHistory, "count");
            strResult += "本册共被 " + strHistoryCount + " 位读者借阅过 (下表中最多仅能显示最近 " + this.App.MaxItemHistoryItems.ToString() + " 位)";

            strResult += "</td></tr>\r\n";

            strResult += "<tr class='columntitle'><td class='index' nowrap>序</td><td class='barcode' nowrap>证条码号</td><td class='summary' nowrap>姓名</td><td class='no' nowrap>续借次</td><td class='borrowdate' nowrap>借阅日期</td><td class='period' nowrap>期限</td><td class='borrowoperator' nowrap>借阅操作者</td><td class='renewcomment' nowrap>续借注</td><td class='returndate' nowrap>还书日期</td><td class='operator' nowrap>还书操作者</td></tr>\r\n";

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
                // string strConfirmItemRecPath = DomUtil.GetAttr(node, "recPath");
                string strReturnDate = DomUtil.GetAttr(node, "returnDate");

                // string strBarcodeLink = "<a href='" + App.OpacServerUrl + "/readerinfo.aspx?barcode=" + strBarcode + "&forcelogin=userid' target='_blank'>" + strBarcode + "</a>";
                string strBarcodeLink = "<a href='javascript:void(0);' onclick=\"window.external.OpenForm('ReaderInfoForm', this.innerText, true);\">" + strBarcode + "</a>";

                // 表格内容奇数行的类名
                string strOdd = "";
                if (((i + 1) % 2) != 0)
                    strOdd = " odd";

                strResult += "<tr class='content" + strOdd + "'>";
                strResult += "<td class='index' nowrap>" + (i + 1).ToString() + "</td>";
                strResult += "<td class='barcode' nowrap>" + strBarcodeLink + "</td>";
                strResult += "<td class='summary pending' nowrap>P:" + strBarcode + "</td>";

                strResult += "<td class='no' nowrap align='right'>" + strNo + "</td>";
                strResult += "<td class='borrowdate' nowrap>" + LocalDate(strBorrowDate) + "</td>";
                strResult += "<td class='period' nowrap>" + LibraryApplication.GetDisplayTimePeriodString(strPeriod) + "</td>";
                strResult += "<td class='borrowoperator' nowrap>" + strBorrowOperator + "</td>";
                strResult += "<td class='renewcomment' width='30%'>" + strRenewComment.Replace(";", "<br/>") + "</td>";
                strResult += "<td class='returndate' nowrap>" + LocalDate(strReturnDate) + "</td>";
                strResult += "<td class='operator' nowrap>" + strOperator + "</td>";
                strResult += "</tr>\r\n";
            }
            strResult += "</table>\r\n";
        }


        strResult += "</body></html>";

        e.ResultString = strResult;
    }

    static string GetOneTR(XmlNode root,
        string strElementName,
        string strTitle)
    {
        string strValue = DomUtil.GetElementText(root, strElementName);

        return GetOneTR(strElementName, strTitle, strValue);
    }

    static string GetOneTR(
        string strElementName,
        string strTitle,
        string strValue)
    {
        string strResult = "";

        strResult += "<tr class='content " + strElementName + "'>";
        strResult += "<td class='name " + strElementName + "' nowrap>";
        strResult += strTitle;
        strResult += "</td>";
        strResult += "<td class='value " + strElementName + "'>";
        strResult += strValue;
        strResult += "</td>";
        strResult += "</tr>\r\n";

        return strResult;
    }
}