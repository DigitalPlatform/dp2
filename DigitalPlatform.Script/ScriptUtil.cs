using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DigitalPlatform.Marc;
using DigitalPlatform.Text;
using System.Collections;
using System.Web;
using System.Diagnostics;

namespace DigitalPlatform.Script
{
    /// <summary>
    /// 工具性函数
    /// </summary>
    public class ScriptUtil
    {
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
            HttpUrlHitCount = 0x01, 
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
            text.Append("<td>名称</td>");
            text.Append("<td></td>");
            text.Append("<td>链接</td>");
            text.Append("<td>媒体类型</td>");
            text.Append("<td>尺寸</td>");
            text.Append("<td>字节数</td>");
            text.Append("</tr>");

            foreach (MarcField field in fields)
            {
                string x = field.select("subfield[@name='x']").FirstContent;

                Hashtable table = StringUtil.ParseParameters(x, ';', ':');
                string strType = (string)table["type"];
                string strSize = (string)table["size"];
                string s_q = field.select("subfield[@name='q']").FirstContent;  // 注意， FirstContent 可能会返回 null

                string u = field.select("subfield[@name='u']").FirstContent;
                string strUri = MakeObjectUrl(strRecPath, u);

                string strSaveAs = "";
                if (StringUtil.MatchMIME(s_q, "text") == true
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
                string s_z = field.select("subfield[@name='z']").FirstContent;

                text.Append("<tr class='content'>");
                text.Append("<td>"+HttpUtility.HtmlEncode(s_3 + " " + strType)+"</td>");
                text.Append("<td style='text-align: right;'>" + strHitCountImage + "</td>");
                text.Append("<td>" + urlTemp + "</td>");
                text.Append("<td>"+HttpUtility.HtmlEncode(s_q)+"</td>");
                text.Append("<td>"+HttpUtility.HtmlEncode(strSize)+"</td>");
                text.Append("<td>"+HttpUtility.HtmlEncode(s_s)+"</td>");
                text.Append("</tr>");

                if (string.IsNullOrEmpty(s_z) == false)
                {
                    text.Append("<tr class='comment'>");
                    text.Append("<td colspan='5'>" + HttpUtility.HtmlEncode(s_z) + "</td>");
                    text.Append("</tr>");
                }

            }
            text.Append("</table>");

            return text.ToString();
        }

    
    }
}
