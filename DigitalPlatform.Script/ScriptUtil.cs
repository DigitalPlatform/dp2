using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Web;
using System.Diagnostics;
using System.Xml;

using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using DigitalPlatform.LibraryClient;
using DigitalPlatform.Xml;
using DigitalPlatform.LibraryClient.localhost;

namespace DigitalPlatform.Script
{
    public class IssueString
    {
        // 用于显示
        public string Volume { get; set; }
        // 用于查询
        public string Query { get; set; }

        public IssueString(string volume, string query)
        {
            this.Volume = volume;
            this.Query = query;
        }
    }

    /// <summary>
    /// 工具性函数
    /// </summary>
    public class ScriptUtil
    {

        // 2016/10/6
        // 从实体记录中获得 定位期 的检索式字符串
        // 如果是合订册，则可能返回多个字符串。普通册返回(最多)一个字符串
        // return:
        //      空集合
        //      其他
        public static List<IssueString> GetIssueQueryStringFromItemXml(XmlDocument dom)
        {
            if (dom == null || dom.DocumentElement == null)
                return new List<IssueString>();

            // 看看记录中是否有 banding/item 元素
            /*
- <binding>
  <item publishTime="20160101" volume="2016,no.1, 总.100, v.10" refID="1e651c7d-bcce-442a-b574-ceee7b4b81e0" price="CNY12" /> 
  <item publishTime="20160201" volume="2016,no.2, 总.101, v.10" refID="64e4cc27-36df-42ce-a90b-9d4cfa6631dd" price="CNY12" /> 
  <item publishTime="20160301" volume="2016,no.3, 总.102, v.10" refID="574b3639-a033-4bbe-8ac3-804251cb13de" price="CNY12" /> 
  <item publishTime="20160401" volume="2016,no.4, 总.103, v.10" refID="" missing="true" /> 
  <item publishTime="20160501" volume="2016,no.5, 总.104, v.10" refID="9d0a2975-2c88-4f29-9b62-2f864a850259" price="CNY12" /> 
  </binding>
             * 
             * */
            List<XmlElement> items = new List<XmlElement>();
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("binding/item");
            if (nodes.Count == 0)
            {
                items.Add(dom.DocumentElement);
            }
            else
            {
                foreach(XmlElement item in nodes)
                {
                    items.Add(item);
                }
            }

            List<IssueString> results = new List<IssueString>();
            foreach (XmlElement item in items)
            {
                XmlNode node = item.SelectSingleNode("volume/text()");
                string strVolumeString = "";
                if (node != null)
                    strVolumeString = node.Value;

                if (string.IsNullOrEmpty(strVolumeString) == true)
                    continue;

                string strYear = "";
                string strIssue = "";
                string strZong = "";
                string strVolume = "";

                // 解析当年期号、总期号、卷号的字符串
                VolumeInfo.ParseItemVolumeString(strVolumeString,
                    out strYear,
                    out strIssue,
                    out strZong,
                    out strVolume);

                // 若 volume 元素中不包含年份，则从 publishTime 元素中取
                if (string.IsNullOrEmpty(strYear))
                {
                    node = dom.DocumentElement.SelectSingleNode("publishTime/text()");

                    string strPublishTime = "";
                    if (node != null)
                        strPublishTime = node.Value;
                    strYear = dp2StringUtil.GetYearPart(strPublishTime);
                }

                results.Add( new IssueString(strVolumeString, strYear + "|" + strIssue + "|" + strZong + "|" + strVolume));
            }

            return results;
        }

        /// <summary>
        /// 从路径中取出库名部分
        /// </summary>
        /// <param name="strPath">路径。例如"中文图书/3"</param>
        /// <returns>返回库名部分</returns>
        public static string GetDbName(string strPath)
        {
            int nRet = strPath.LastIndexOf("/");
            if (nRet == -1)
                return strPath;

            return strPath.Substring(0, nRet).Trim();
        }

        // 
        // parammeters:
        //      strPath 路径。例如"中文图书/3"
        /// <summary>
        /// 从路径中取出记录号部分
        /// </summary>
        /// <param name="strPath">路径。例如"中文图书/3"</param>
        /// <returns>返回记录号部分</returns>
        public static string GetRecordID(string strPath)
        {
            int nRet = strPath.LastIndexOf("/");
            if (nRet == -1)
                return "";

            return strPath.Substring(nRet + 1).Trim();
        }

        // 为了二次开发脚本使用
        public static string MakeObjectUrl(string strRecPath,
            string strUri)
        {
            if (string.IsNullOrEmpty(strUri) == true)
                return strUri;

            if (StringUtil.HasHead(strUri, "http:") == true)
                return strUri;

            if (StringUtil.HasHead(strUri, "uri:") == true)
                strUri = strUri.Substring(4).Trim();

            string strDbName = GetDbName(strRecPath);
            string strRecID = GetRecordID(strRecPath);

            string strOutputUri = "";
            ReplaceUri(strUri,
                strDbName,
                strRecID,
                out strOutputUri);

            return strOutputUri;
        }

        // "object/1"
        // "1/object/1"
        // "库名/1/object/1"
        // return:
        //		false	没有发生替换
        //		true	替换了
        static bool ReplaceUri(string strUri,
            string strCurDbName,
            string strCurRecID,
            out string strOutputUri)
        {
            strOutputUri = strUri;
            string strTemp = strUri;
            // 看看第一部分是不是object
            string strPart = StringUtil.GetFirstPartPath(ref strTemp);
            if (strPart == "")
                return false;

            if (strTemp == "")
            {
                strOutputUri = strCurDbName + "/" + strCurRecID + "/object/" + strPart;
                return true;
            }

            if (strPart == "object")
            {
                strOutputUri = strCurDbName + "/" + strCurRecID + "/object/" + strTemp;
                return true;
            }

            string strPart2 = StringUtil.GetFirstPartPath(ref strTemp);
            if (strPart2 == "")
                return false;

            if (strPart2 == "object")
            {
                strOutputUri = strCurDbName + "/" + strPart + "/object/" + strTemp;
                return false;
            }

            string strPart3 = StringUtil.GetFirstPartPath(ref strTemp);
            if (strPart3 == "")
                return false;

            if (strPart3 == "object")
            {
                strOutputUri = strPart + "/" + strPart2 + "/object/" + strTemp;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 获得封面图像 URL
        /// 优先选择中等大小的图片
        /// </summary>
        /// <param name="strMARC">MARC机内格式字符串</param>
        /// <param name="strPreferredType">优先使用何种大小类型</param>
        /// <returns>返回封面图像 URL。空表示没有找到</returns>
        public static string GetCoverImageUrl(string strMARC,
            string strPreferredType = "MediumImage")
        {
            string strLargeUrl = "";
            string strMediumUrl = "";   // type:FrontCover.MediumImage
            string strUrl = ""; // type:FronCover
            string strSmallUrl = "";

            MarcRecord record = new MarcRecord(strMARC);
            MarcNodeList fields = record.select("field[@name='856']");
            foreach (MarcField field in fields)
            {
                string x = field.select("subfield[@name='x']").FirstContent;
                if (string.IsNullOrEmpty(x) == true)
                    continue;
                Hashtable table = StringUtil.ParseParameters(x, ';', ':');
                string strType = (string)table["type"];
                if (string.IsNullOrEmpty(strType) == true)
                    continue;

                string u = field.select("subfield[@name='u']").FirstContent;
                // if (string.IsNullOrEmpty(u) == true)
                //     u = field.select("subfield[@name='8']").FirstContent;

                // . 分隔 FrontCover.MediumImage
                if (StringUtil.HasHead(strType, "FrontCover." + strPreferredType) == true)
                    return u;

                if (StringUtil.HasHead(strType, "FrontCover.SmallImage") == true)
                    strSmallUrl = u;
                else if (StringUtil.HasHead(strType, "FrontCover.MediumImage") == true)
                    strMediumUrl = u;
                else if (StringUtil.HasHead(strType, "FrontCover.LargeImage") == true)
                    strLargeUrl = u;
                else if (StringUtil.HasHead(strType, "FrontCover") == true)
                    strUrl = u;

            }

            if (string.IsNullOrEmpty(strLargeUrl) == false)
                return strLargeUrl;
            if (string.IsNullOrEmpty(strMediumUrl) == false)
                return strMediumUrl;
            if (string.IsNullOrEmpty(strUrl) == false)
                return strUrl;
            return strSmallUrl;
        }

        [Flags]
        public enum BuildObjectHtmlTableStyle
        {
            None = 0,
            HttpUrlHitCount = 0x01,     // 是否对 http:// 地址进行访问计数
            FrontCover = 0x02,   // 是否包含封面图像事项
        }

        // 创建 OPAC 详细页面中的对象资源显示局部 HTML。这是一个 <table> 片段
        // 前导语 $3
        // 链接文字 $y $f
        // URL $u
        // 格式类型 $q
        // 对象ID $8
        // 对象尺寸 $s
        // 公开注释 $z
        public static string BuildObjectHtmlTable(string strMARC,
            string strRecPath,
            BuildObjectHtmlTableStyle style = BuildObjectHtmlTableStyle.HttpUrlHitCount)
        {
            // Debug.Assert(false, "");

            MarcRecord record = new MarcRecord(strMARC);
            MarcNodeList fields = record.select("field[@name='856']");

            if (fields.count == 0)
                return "";

            StringBuilder text = new StringBuilder();

            text.Append("<table class='object_table'>");
            text.Append("<tr class='column_title'>");
            text.Append("<td class='type' style='word-break:keep-all;'>名称</td>");
            text.Append("<td class='hitcount'></td>");
            text.Append("<td class='link' style='word-break:keep-all;'>链接</td>");
            text.Append("<td class='mime' style='word-break:keep-all;'>媒体类型</td>");
            text.Append("<td class='size' style='word-break:keep-all;'>尺寸</td>");
            text.Append("<td class='bytes' style='word-break:keep-all;'>字节数</td>");
            text.Append("</tr>");

            int nCount = 0;
            foreach (MarcField field in fields)
            {
                string x = field.select("subfield[@name='x']").FirstContent;

                Hashtable table = StringUtil.ParseParameters(x, ';', ':');
                string strType = (string)table["type"];

                if (string.IsNullOrEmpty(strType) == false
                    && (style & BuildObjectHtmlTableStyle.FrontCover) == 0
                    && (strType == "FrontCover" || strType.StartsWith("FrontCover.") == true))
                    continue;

                string strSize = (string)table["size"];
                string s_q = field.select("subfield[@name='q']").FirstContent;  // 注意， FirstContent 可能会返回 null

                string u = field.select("subfield[@name='u']").FirstContent;
                string strUri = MakeObjectUrl(strRecPath, u);

                string strSaveAs = "";
                if (string.IsNullOrEmpty(s_q) == true
                    || StringUtil.MatchMIME(s_q, "text") == true
                    || StringUtil.MatchMIME(s_q, "image") == true)
                {

                }
                else
                {
                    strSaveAs = "&saveas=true";
                }
                string strHitCountImage = "";
                string strObjectUrl = strUri;
                if (StringUtil.HasHead(strUri, "http:") == false
                    && StringUtil.HasHead(strUri, "https:") == false)
                {
                    // 内部对象
                    strObjectUrl = "./getobject.aspx?uri=" + HttpUtility.UrlEncode(strUri) + strSaveAs;
                    strHitCountImage = "<img src='" + strObjectUrl + "&style=hitcount' alt='hitcount'></img>";
                }
                else
                {
                    // http: 或 https: 的情形，即外部 URL
                    if ((style & BuildObjectHtmlTableStyle.HttpUrlHitCount) != 0)
                    {
                        strObjectUrl = "./getobject.aspx?uri=" + HttpUtility.UrlEncode(strUri) + strSaveAs + "&biblioRecPath=" + HttpUtility.UrlEncode(strRecPath);
                        strHitCountImage = "<img src='" + strObjectUrl + "&style=hitcount&biblioRecPath=" + HttpUtility.UrlEncode(strRecPath) + "' alt='hitcount'></img>";
                    }
                }

                string y = field.select("subfield[@name='y']").FirstContent;
                string f = field.select("subfield[@name='f']").FirstContent;

                string urlLabel = "";
                if (string.IsNullOrEmpty(y) == false)
                    urlLabel = y;
                else
                    urlLabel = f;
                if (string.IsNullOrEmpty(urlLabel) == true)
                    urlLabel = strType;

                // 2015/11/26
                string s_z = field.select("subfield[@name='z']").FirstContent;
                if (string.IsNullOrEmpty(urlLabel) == true
                    && string.IsNullOrEmpty(s_z) == false)
                {
                    urlLabel = s_z;
                    s_z = "";
                }

                if (string.IsNullOrEmpty(urlLabel) == true)
                    urlLabel = strObjectUrl;

                string urlTemp = "";
                if (String.IsNullOrEmpty(strObjectUrl) == false)
                {
                    urlTemp += "<a href='" + strObjectUrl + "'>";
                    urlTemp += urlLabel;
                    urlTemp += "</a>";
                }
                else
                    urlTemp = urlLabel;

                string s_3 = field.select("subfield[@name='3']").FirstContent;
                string s_s = field.select("subfield[@name='s']").FirstContent;

                text.Append("<tr class='content'>");
                text.Append("<td class='type'>" + HttpUtility.HtmlEncode(s_3 + " " + strType) + "</td>");
                text.Append("<td class='hitcount' style='text-align: right;'>" + strHitCountImage + "</td>");
                text.Append("<td class='link' style='word-break:break-all;'>" + urlTemp + "</td>");
                text.Append("<td class='mime'>" + HttpUtility.HtmlEncode(s_q) + "</td>");
                text.Append("<td class='size'>" + HttpUtility.HtmlEncode(strSize) + "</td>");
                text.Append("<td class='bytes'>" + HttpUtility.HtmlEncode(s_s) + "</td>");
                text.Append("</tr>");

                if (string.IsNullOrEmpty(s_z) == false)
                {
                    text.Append("<tr class='comment'>");
                    text.Append("<td colspan='6'>" + HttpUtility.HtmlEncode(s_z) + "</td>");
                    text.Append("</tr>");
                }
                nCount++;
            }

            if (nCount == 0)
                return "";

            text.Append("</table>");

            return text.ToString();
        }

        // 创建 table 中的对象资源局部 XML。这是一个 <table> 片段
        // 前导语 $3
        // 链接文字 $y $f
        // URL $u
        // 格式类型 $q
        // 对象ID $8
        // 对象尺寸 $s
        // 公开注释 $z
        public static string BuildObjectXmlTable(string strMARC,
            // string strRecPath,
            BuildObjectHtmlTableStyle style = BuildObjectHtmlTableStyle.None)
        {
            // Debug.Assert(false, "");

            MarcRecord record = new MarcRecord(strMARC);
            MarcNodeList fields = record.select("field[@name='856']");

            if (fields.count == 0)
                return "";

            XmlDocument dom = new XmlDocument();
            dom.LoadXml("<table/>");

            int nCount = 0;
            foreach (MarcField field in fields)
            {
                string x = field.select("subfield[@name='x']").FirstContent;

                Hashtable table = StringUtil.ParseParameters(x, ';', ':');
                string strType = (string)table["type"];

                if (string.IsNullOrEmpty(strType) == false
                    && (style & BuildObjectHtmlTableStyle.FrontCover) == 0
                    && (strType == "FrontCover" || strType.StartsWith("FrontCover.") == true))
                    continue;

                string strSize = (string)table["size"];
                string s_q = field.select("subfield[@name='q']").FirstContent;  // 注意， FirstContent 可能会返回 null

                string u = field.select("subfield[@name='u']").FirstContent;
                // string strUri = MakeObjectUrl(strRecPath, u);

                string strSaveAs = "";
                if (string.IsNullOrEmpty(s_q) == true   // 2016/9/4
                    || StringUtil.MatchMIME(s_q, "text") == true
                    || StringUtil.MatchMIME(s_q, "image") == true)
                {

                }
                else
                {
                    strSaveAs = "true";
                }

#if NO
                string strObjectUrl = strUri;
                if (StringUtil.HasHead(strUri, "http:") == false
                    && StringUtil.HasHead(strUri, "https:") == false)
                {
                    // 内部对象
                    strObjectUrl = "./getobject.aspx?uri=" + HttpUtility.UrlEncode(strUri) + strSaveAs;
                }
                else
                {
                    // http: 或 https: 的情形，即外部 URL
                    if ((style & BuildObjectHtmlTableStyle.HttpUrlHitCount) != 0)
                    {
                        strObjectUrl = "./getobject.aspx?uri=" + HttpUtility.UrlEncode(strUri) + strSaveAs + "&biblioRecPath=" + HttpUtility.UrlEncode(strRecPath);
                    }
                }
#endif

                string y = field.select("subfield[@name='y']").FirstContent;
                string f = field.select("subfield[@name='f']").FirstContent;

                string urlLabel = "";
                if (string.IsNullOrEmpty(y) == false)
                    urlLabel = y;
                else
                    urlLabel = f;
                if (string.IsNullOrEmpty(urlLabel) == true)
                    urlLabel = strType;

                // 2015/11/26
                string s_z = field.select("subfield[@name='z']").FirstContent;
                if (string.IsNullOrEmpty(urlLabel) == true
                    && string.IsNullOrEmpty(s_z) == false)
                {
                    urlLabel = s_z;
                    s_z = "";
                }

                if (string.IsNullOrEmpty(urlLabel) == true)
                    urlLabel = u;

#if NO
                string urlTemp = "";
                if (String.IsNullOrEmpty(strObjectUrl) == false)
                {
                    urlTemp += "<a href='" + strObjectUrl + "'>";
                    urlTemp += urlLabel;
                    urlTemp += "</a>";
                }
                else
                    urlTemp = urlLabel;
#endif

                string s_3 = field.select("subfield[@name='3']").FirstContent;
                string s_s = field.select("subfield[@name='s']").FirstContent;

                XmlElement line = dom.CreateElement("line");
                dom.DocumentElement.AppendChild(line);

                string strTypeString = (s_3 + " " + strType).Trim();
                if (string.IsNullOrEmpty(strTypeString) == false)
                    line.SetAttribute("type", strTypeString);

                if (string.IsNullOrEmpty(urlLabel) == false)
                    line.SetAttribute("urlLabel", urlLabel);

                if (string.IsNullOrEmpty(u) == false)
                    line.SetAttribute("uri", u);

                if (string.IsNullOrEmpty(s_q) == false)
                    line.SetAttribute("mime", s_q);

                if (string.IsNullOrEmpty(strSize) == false)
                    line.SetAttribute("size", strSize);

                if (string.IsNullOrEmpty(s_s) == false)
                    line.SetAttribute("bytes", s_s);

                if (string.IsNullOrEmpty(strSaveAs) == false)
                    line.SetAttribute("saveAs", strSaveAs);

                if (string.IsNullOrEmpty(s_z) == false)
                    line.SetAttribute("comment", s_z);
                nCount++;
            }

            if (nCount == 0)
                return "";

            return dom.DocumentElement.OuterXml;
        }
    }

    /// <summary>
    /// 对 LibraryChannel 的扩展
    /// </summary>
    public static class LibraryChannelExtension2
    {
        // 获得指定一期的封面图片 URI
        // parameters:
        //      strBiblioPath   书目记录路径
        //      strQueryString  检索词。例如 “2005|1|1000|50”。格式为 年|期号|总期号|卷号。一般为 年|期号| 即可。
        public static int GetIssueCoverImageUri(this LibraryChannel channel,
            DigitalPlatform.Stop stop,
            string strBiblioRecPath,
            string strQueryString,
            string strPreferredType,
            out string strUri,
            out string strError)
        {
            strUri = "";
            strError = "";

            string strBiblioRecordID = StringUtil.GetRecordId(strBiblioRecPath);
            string strStyle = "query:父记录+期号|" + strBiblioRecordID + "|" + strQueryString;
            DigitalPlatform.LibraryClient.localhost.EntityInfo[] issueinfos = null;
            long lRet = channel.GetIssues(stop,
                strBiblioRecPath,
                0,
                1,
                strStyle,
                channel.Lang,
                out issueinfos,
                out strError);
            if (lRet == -1)
                return -1;
            if (lRet == 0)
                return 0;   // not found

            EntityInfo info = issueinfos[0]; 
            string strXml = info.OldRecord;
            string strIssueRecordPath = info.OldRecPath;

            if (string.IsNullOrEmpty(strXml))
            {
                strError = "期记录 '"+strIssueRecordPath+"' 的 strXml 为空";
                return -1;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch(Exception ex)
            {
                strError = "期记录 '" + strIssueRecordPath + "' XML 装入 DOM 时出错: " + ex.Message;
                return -1;
            }

            string strObjectID = GetCoverImageID(dom, strPreferredType);
            if (string.IsNullOrEmpty(strObjectID))
                return 0;

            strUri = strIssueRecordPath + "/object/" + strObjectID;
            return 1;
        }

        public static string GetCoverImageID(XmlDocument dom,
            string strPreferredType = "MediumImage")
        {
            string strLargeUrl = "";
            string strMediumUrl = "";   // type:FrontCover.MediumImage
            string strUrl = ""; // type:FronCover
            string strSmallUrl = "";

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(new NameTable());
            nsmgr.AddNamespace("dprms", DpNs.dprms);
            XmlNodeList nodes = dom.DocumentElement.SelectNodes("//dprms:file", nsmgr);
            foreach (XmlElement node in nodes)
            {
                string strUsage = node.GetAttribute("usage");
                string strID = node.GetAttribute("id");

                if (string.IsNullOrEmpty(strUsage) == true)
                    continue;

                // . 分隔 FrontCover.MediumImage
                if (StringUtil.HasHead(strUsage, "FrontCover." + strPreferredType) == true)
                    return strID;

                if (StringUtil.HasHead(strUsage, "FrontCover.SmallImage") == true)
                    strSmallUrl = strID;
                else if (StringUtil.HasHead(strUsage, "FrontCover.MediumImage") == true)
                    strMediumUrl = strID;
                else if (StringUtil.HasHead(strUsage, "FrontCover.LargeImage") == true)
                    strLargeUrl = strID;
                else if (StringUtil.HasHead(strUsage, "FrontCover") == true)
                    strUrl = strID;

            }

            if (string.IsNullOrEmpty(strLargeUrl) == false)
                return strLargeUrl;
            if (string.IsNullOrEmpty(strMediumUrl) == false)
                return strMediumUrl;
            if (string.IsNullOrEmpty(strUrl) == false)
                return strUrl;
            return strSmallUrl;
        }

    }
}
