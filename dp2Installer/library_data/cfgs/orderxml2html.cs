// 订购XML记录转换为HTML显示格式
// 编写者：谢涛
// 创建日期: 2011/9/6

// 修改历史：
// 2011/9/7

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
        strResult += "<link href='%mappeddir%\\styles\\orderhtml.css' type='text/css' rel='stylesheet' />";
        strResult += "<link href=\"%mappeddir%/jquery-ui-1.8.7/css/jquery-ui-1.8.7.css\" rel=\"stylesheet\" type=\"text/css\" />"
        + "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-1.4.4.min.js\"></script>"
        + "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-ui-1.8.7.min.js\"></script>"
        + "<script type='text/javascript' charset='UTF-8' src='%mappeddir%\\scripts\\getsummary.js" + "'></script>";
        strResult += "</head>";
        strResult += "<body>";

        strResult += "<table class='orderinfo'>";

        {

            // 编号
            string strIndex = DomUtil.GetElementText(dom.DocumentElement, "index");
            strResult += GetOneTR("index", "编号", strIndex);

            // 书目摘要
            strResult += "<tr class='content summary'>";
            strResult += "<td class='name summary' nowrap>";
            strResult += "书目摘要";
            strResult += "</td>";
            strResult += "<td class='value summary pending'>";
            strResult += "B:|" + e.RecPath;
            strResult += "</td></tr>";

            // 状态
            strResult += GetOneTR(dom.DocumentElement, "state", "状态");

            // 书目号
            strResult += GetOneTR(dom.DocumentElement, "catalogNo", "书目号");

            // 渠道
            strResult += GetOneTR(dom.DocumentElement, "seller", "渠道");

            // 经费来源
            strResult += GetOneTR(dom.DocumentElement, "source", "经费来源");

            // 时间范围
            strResult += GetOneTR(dom.DocumentElement, "range", "时间范围");

            // 包含期数
            strResult += GetOneTR(dom.DocumentElement, "issueCount", "包含期数");

            // 复本数
            strResult += GetOneTR(dom.DocumentElement, "copy", "复本数");

            // 单价
            strResult += GetOneTR(dom.DocumentElement, "price", "单价");

            // 总价格
            strResult += GetOneTR(dom.DocumentElement, "totalPrice", "总价");

            // 订购时间
            string strOrderTime = DomUtil.GetElementText(dom.DocumentElement, "orderTime");
            strResult += GetOneTR("orderTime", "订购时间", LocalTime(strOrderTime));

            // 订单号
            strResult += GetOneTR(dom.DocumentElement, "orderID", "订单号");

            // 馆藏分配
            strResult += GetOneTR(dom.DocumentElement, "distribute", "馆藏分配");

            // 类别
            strResult += GetOneTR(dom.DocumentElement, "class", "类别");

            // 附注
            strResult += GetOneTR(dom.DocumentElement, "comment", "附注");


            // 批次号
            strResult += GetOneTR(dom.DocumentElement, "batchNo", "批次号");

            // 渠道地址
            strResult += GetOneTR(dom.DocumentElement, "sellerAddress", "渠道地址");

            // 参考ID
            strResult += GetOneTR(dom.DocumentElement, "refID", "参考ID");

            // 订购记录路径
            strResult += GetOneTR("recpath", "订购记录路径", e.RecPath);

            strResult += "</table>";
        }

        // 操作历史
        XmlNodeList nodes = dom.DocumentElement.SelectNodes("operations/operation");

        if (nodes.Count > 0)
        {
            strResult += "<br/><b>操作历史</b><br/>";
            strResult += "<table class='operationhistory'>\r\n";

            strResult += "<tr class='columntitle'><td class='index' nowrap>序</td><td class='name' nowrap>操作名</td><td class='time' nowrap>操作时间</td><td class='operator' nowrap>操作者</td></tr>\r\n";

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                string strName = DomUtil.GetAttr(node, "name");
                string strTime = DomUtil.GetAttr(node, "time");
                string strOperator = DomUtil.GetAttr(node, "operator");	// 还书操作者

                // 表格内容奇数行的类名
                string strOdd = "";
                if (((i + 1) % 2) != 0)
                    strOdd = " odd";

                strResult += "<tr class='content" + strOdd + "'>";
                strResult += "<td class='index' nowrap>" + (i + 1).ToString() + "</td>";
                strResult += "<td class='name' nowrap>" + strName + "</td>";
                strResult += "<td class='time' nowrap>" + LocalTime(strTime) + "</td>";
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