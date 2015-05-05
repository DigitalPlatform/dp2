// 评注XML记录转换为HTML显示格式
// 编写者：谢涛
// 最后修改日期: 2011/3/1

// 修改历史：
// 2011/2/28	开始编写
// 2011/9/7
// 2012/12/27   “创建者”修改为“作者”

using System;
using System.Xml;
using System.Web;

using DigitalPlatform.LibraryServer;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;

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

        string strResult = "<!DOCTYPE html PUBLIC \"-//W3C//DTD XHTML 1.0 Transitional//EN\" \"http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd\"><html xmlns=\"http://www.w3.org/1999/xhtml\">";

        strResult += "<head>";
        strResult += "<link href='%mappeddir%\\styles\\commenthtml.css' type='text/css' rel='stylesheet' />";
        strResult += "<link href=\"%mappeddir%/jquery-ui-1.8.7/css/jquery-ui-1.8.7.css\" rel=\"stylesheet\" type=\"text/css\" />"
        + "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-1.4.4.min.js\"></script>"
        + "<script type=\"text/javascript\" src=\"%mappeddir%/jquery-ui-1.8.7/js/jquery-ui-1.8.7.min.js\"></script>"
        + "<script type='text/javascript' charset='UTF-8' src='%mappeddir%\\scripts\\getsummary.js" + "'></script>";

        strResult += "</head>";
        strResult += "<body>";

        strResult += "<table class='commentinfo'>";

        {

            // 帖子标题
            string strTitle = DomUtil.GetElementText(dom.DocumentElement, "title");
            strResult += GetOneTR("title", "标题", strTitle);

            // 作者
            string strOriginCreator = DomUtil.GetElementText(dom.DocumentElement, "creator");
            strResult += GetOneTR("creator", "作者", strOriginCreator);

            // 馆代码
            strResult += GetOneTR(dom.DocumentElement, "libraryCode", "馆代码");


            // 帖子状态
            string strState = DomUtil.GetElementText(dom.DocumentElement, "state");
            strResult += GetOneTR(dom.DocumentElement, "state", "状态");

            // 订购建议
            string strType = DomUtil.GetElementText(dom.DocumentElement, "type");
            strResult += GetOneTR("type", "类型", strType);

            string strOrderSuggestion = DomUtil.GetElementText(dom.DocumentElement, "orderSuggestion");

            if (strType == "订购征询")
            {
                string strOrderSuggestionText = this.GetString("建议不要订购本书");
                string strYesOrNo = "no";
                string strImg = "<img class='icon' src='%datadir%\\no_order_24.png'>";
                if (strOrderSuggestion == "yes")
                {
                    // 建议订购
                    strOrderSuggestionText = this.GetString("建议订购本书");
                    strYesOrNo = "yes";
                    strImg = "<img class='icon' src='%datadir%\\yes_order_24.png'>";
                }
                else if (string.IsNullOrEmpty(strOrderSuggestion) == true)
                {
                    // 无所谓
                    strOrderSuggestionText = this.GetString("无所谓");
                    strYesOrNo = "";
                    strImg = "<img class='icon' src='%datadir%\\null_order_32.png'>";
                }

                strResult += GetOneTR("order_suggestion", "订购建议", strImg + strOrderSuggestionText);

            }

            // 正文
            string strOriginContent = DomUtil.GetElementText(dom.DocumentElement, "content");
            string strContent = strOriginContent.Replace("\\r", "\r\n");
            strContent = ParseHttpString(
                strContent);

            strContent = GetHtmlContentFromPureText(
                strContent,
                Text2HtmlStyle.P);
            strResult += GetOneTR("content", "正文", strContent);

            // 
            string strOperInfo = "";
            {
                string strFirstOperator = "";
                string strTime = "";

                XmlNode node = dom.DocumentElement.SelectSingleNode("operations/operation[@name='create']");
                if (node != null)
                {
                    strFirstOperator = DomUtil.GetAttr(node, "operator");
                    strTime = DomUtil.GetAttr(node, "time");
                    strOperInfo += " " + this.GetString("创建") + ": "
                        + GetUTimeString(strTime);
                }

                node = dom.DocumentElement.SelectSingleNode("operations/operation[@name='lastContentModified']");
                if (node != null)
                {
                    string strLastOperator = DomUtil.GetAttr(node, "operator");
                    strTime = DomUtil.GetAttr(node, "time");
                    strOperInfo += "<br/>" + this.GetString("最后修改") + ": "
                        + GetUTimeString(strTime);
                    if (strLastOperator != strFirstOperator)
                        strOperInfo += " (" + strLastOperator + ")";
                }

                XmlNodeList nodes = dom.DocumentElement.SelectNodes("operations/operation[@name='stateModified']");
                if (nodes.Count > 0)
                {
                    XmlNode tail = nodes[nodes.Count - 1];
                    string strLastOperator = DomUtil.GetAttr(tail, "operator");
                    strTime = DomUtil.GetAttr(tail, "time");
                    strOperInfo += "<br/>" + this.GetString("状态最后修改") + ": "
                        + GetUTimeString(strTime);
                    if (strLastOperator != strFirstOperator)
                        strOperInfo += " (" + strLastOperator + ")";
                }
            }
            strResult += GetOneTR("operinfo", "创建时间", strOperInfo);

            {
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
                nsmgr.AddNamespace("dprms", DpNs.dprms);

                // 全部<dprms:file>元素
                XmlNodeList nodes = dom
                    .DocumentElement.SelectNodes("//dprms:file", nsmgr);
                foreach (XmlNode node in nodes)
                {
                    string strMime = DomUtil.GetAttr(node, "__mime");
                    if (StringUtil.HasHead(strMime, "image/") == false)
                        continue;   // 只关注图像文件

                    string strResPath = e.RecPath + "/object/" + DomUtil.GetAttr(node, "id");

                    string strImage = "<img class='upload pending' name='"
            + "object-path:" + strResPath + "' src='%mappeddir%\\images\\ajax-loader.gif' alt=''></img>";
                    strResult += GetOneTR("operinfo", "上传图像", strImage);
                }
            }

            // 参考ID
            strResult += GetOneTR(dom.DocumentElement, "refID", "参考ID");

            // 册记录路径
            strResult += GetOneTR("recpath", "评注记录路径", e.RecPath);

            strResult += "</table>";
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

    // 将纯文本中的"http://"替换为<a>命令
    public static string ParseHttpString(
        string strText)
    {
        string strResult = "";
        int nCur = 0;
        for (; ; )
        {
            int nStart = strText.IndexOf("http://", nCur);
            if (nStart == -1)
            {
                strResult += ReplaceLeadingBlank(HttpUtility.HtmlEncode(strText.Substring(nCur)));
                break;
            }

            // 复制nCur到nStart一段
            strResult += ReplaceLeadingBlank(HttpUtility.HtmlEncode(strText.Substring(nCur, nStart - nCur)));

            int nEnd = strText.IndexOfAny(new char[] { ' ', ',', ')', '(', '\r', '\n', '\"', '\'' },
                nStart + 1);
            if (nEnd == -1)
                nEnd = strText.Length;

            string strUrl = strText.Substring(nStart, nEnd - nStart);

            string strLeft = "<a href='" + strUrl + "' target='_blank'>";
            string strRight = "</a>";

            strResult += strLeft + HttpUtility.HtmlEncode(strUrl) + strRight;

            nCur = nEnd;
        }

        return strResult;
    }

    // 把一个字符串开头的连续空白替换为&nbsp;
    public static string ReplaceLeadingBlank(string strText)
    {
        if (strText == "")
            return "";
        strText = strText.Replace("\t", "&nbsp;&nbsp;&nbsp;&nbsp;");
        return strText;
    }

    // 把纯文本变为适合html显示的格式
    public static string GetHtmlContentFromPureText(
        string strText,
        Text2HtmlStyle style)
    {
        string[] aLine = strText.Replace("\r", "").Split(new char[] { '\n' });
        string strResult = "";
        for (int i = 0; i < aLine.Length; i++)
        {
            string strLine = aLine[i];

            if (style == Text2HtmlStyle.BR)
            {
                strResult += strLine + "<br/>";
            }
            else if (style == Text2HtmlStyle.P)
            {
                if (String.IsNullOrEmpty(strLine) == true)
                    strResult += "<p>&nbsp;</p>";
                else
                {
                    strResult += "<p>" + strLine + "</p>";
                }
            }
            else
            {
                if (String.IsNullOrEmpty(strLine) == true)
                    strResult += "<p>&nbsp;</p>";
                else
                {
                    strResult += "<p>" + strLine + "</p>";
                }
            }
        }

        return strResult;
    }

    public static string GetUTimeString(string strRfc1123TimeString)
    {
        if (String.IsNullOrEmpty(strRfc1123TimeString) == true)
            return "";

        DateTime time = new DateTime(0);
        try
        {
            time = DateTimeUtil.FromRfc1123DateTimeString(strRfc1123TimeString);
        }
        catch
        {
        }

        return time.ToLocalTime().ToString("u");
    }

    string GetString(string strText)
    {
        return strText;
    }
}

public enum Text2HtmlStyle
{
    BR = 0,
    P = 1,
}