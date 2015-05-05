// 期XML记录转换为HTML显示格式
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
        strResult += "<link href='%mappeddir%\\styles\\issuehtml.css' type='text/css' rel='stylesheet' />";
        strResult += "<link href=\"%mappeddir%/jquery-ui-1.8.7/css/jquery-ui-1.8.7.css\" rel=\"stylesheet\" type=\"text/css\" />"
        + "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-1.4.4.min.js\"></script>"
        + "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-ui-1.8.7.min.js\"></script>"
        + "<script type='text/javascript' charset='UTF-8' src='%mappeddir%\\scripts\\getsummary.js" + "'></script>";
        strResult += "</head>";
        strResult += "<body>";

        strResult += "<table class='issueinfo'>";

        {

            // 出版时间
            strResult += GetOneTR(dom.DocumentElement, "publishTime", "出版时间");

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

            // 期号
            strResult += GetOneTR(dom.DocumentElement, "issue", "期号");

            // 总期号
            strResult += GetOneTR(dom.DocumentElement, "zong", "总期号");

            // 卷号
            strResult += GetOneTR(dom.DocumentElement, "volumn", "卷号");

            // 附注
            strResult += GetOneTR(dom.DocumentElement, "comment", "附注");

            // 批次号
            strResult += GetOneTR(dom.DocumentElement, "batchNo", "批次号");

            // 参考ID
            strResult += GetOneTR(dom.DocumentElement, "refID", "参考ID");

            // 期记录路径
            strResult += GetOneTR("recpath", "期记录路径", e.RecPath);

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