using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Threading;
using System.Diagnostics;
using System.IO;

using DigitalPlatform;
using DigitalPlatform.Marc;
using DigitalPlatform.Xml;
using DigitalPlatform.GUI;
using DigitalPlatform.Text;

namespace UpgradeDt1000ToDp2
{
    public class Global
    {
        // 检查数据库名
        // return:
        //      -1  有错
        //      0   无错
        public static int CheckDbName(string strDbName,
            out string strError)
        {
            strError = "";
            if (strDbName.IndexOf("#") != -1)
            {
                strError = "数据库名 '" + strDbName + "' 格式错误。不能有#号";
                return -1;
            }

            return 0;
        }

        /* 移动到DigitalPlatform.Text的PriceUtil中
        // 从复杂的字符串中，析出纯粹价格数字部分（包括小数点）
        public static string GetPurePrice(string strPrice)
        {
            if (String.IsNullOrEmpty(strPrice) == true)
                return strPrice;

            string strResult = "";
            int nSegment = 0;   // 0 非数字段 1数字段 2 非数字段
            int nPointCount = 0;

            for (int i = 0; i < strPrice.Length; i++)
            {
                char ch = strPrice[i];

                if ((ch <= '9' && ch >= '0')
                    || ch == '.')
                {

                    if (ch == '.')
                    {
                        if (nPointCount == 1)
                            break;  // 已经出现过一个小数点了

                        nPointCount++;
                    }

                    if (nSegment == 0)
                    {
                        nSegment = 1;
                    }
                }
                else
                {
                    if (nSegment == 1)
                    {
                        nSegment = 2;
                        break;
                    }
                }

                if (nSegment == 1)
                    strResult += ch;
            }

            // 如果第一个就是小数点
            if (strResult.Length > 0
                && strResult[0] == '.')
            {
                strResult = "0" + strResult;
            }

            return strResult;
        }
         * */

        static string source_chars = "０１２３４５６７８９．。ａｂｃｄｅｆｇｈｉｊｋｌｍｎｏｐｑｒｓｔｕｖｗｘｙｚＡＢＣＤＥＦＧＨＩＪＫＬＭＮＯＰＱＲＳＴＵＶＷＸＹＺ";
        static string target_chars = "0123456789..abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public static string ConvertQuanjiaoToBanjiao(string strText)
        {
            Debug.Assert(source_chars.Length == target_chars.Length, "");
            string strTarget = "";
            for (int i = 0; i < strText.Length; i++)
            {
                char ch = strText[i];

                int nRet = source_chars.IndexOf(ch);
                if (nRet != -1)
                    ch = target_chars[nRet];

                strTarget += ch;
            }

            return strTarget;
        }

        /*
~~~~~~~
乐山师院数据来源多，以前的种价格字段格式著录格式多样，有“CNY25.00元”、
“25.00”、“￥25.00元”、“￥25.00”、“CNY25.00”、“cny25.00”、“25.00
元”等等，现在他们确定以后全采用“CNY25.00”格式著录。
CALIS中，许可重复010$d来表达价格实录和获赠或其它币种价格。所以，可能乐山
师院也有少量的此类重复价格子字段的数据。
为省成本，批处理或册信息编辑窗中，建议只管一个价格字段，别的都不管（如果
没有价格字段，则转换为空而非零）。
转换时，是否可以兼顾到用中文全角输入的数字如“２５.００”或小数点是中文
全解但标点选择的是英文标点如“．”？

~~~~
处理步骤：
1) 全部字符转换为半角
2) 抽出纯数字部分
3) 观察前缀或者后缀，如果有CNY cny ￥ 元等字样，可以确定为人民币。
前缀和后缀完全为空，也可确定为人民币。
否则，保留原来的前缀。         * */
        // 正规化价格字符串
        public static string CanonicalizePrice(string strPrice,
            bool bForceCNY)
        {
            // 全角字符变换为半角
            strPrice = Global.ConvertQuanjiaoToBanjiao(strPrice);

            if (bForceCNY == true)
            {
                // 提取出纯数字
                string strPurePrice = PriceUtil.GetPurePrice(strPrice);

                return "CNY" + strPurePrice;
            }

            string strPrefix = "";
            string strValue = "";
            string strPostfix = "";
            string strError = "";

            int nRet = Global.ParsePriceUnit(strPrice,
                out strPrefix,
                out strValue,
                out strPostfix,
                out strError);
            if (nRet == -1)
                return strPrice;    // 无法parse

            bool bCNY = false;
            strPrefix = strPrefix.Trim();
            strPostfix = strPostfix.Trim();

            if (String.IsNullOrEmpty(strPrefix) == true
                && String.IsNullOrEmpty(strPostfix) == true)
            {
                bCNY = true;
                goto DONE;
            }


            if (strPrefix.IndexOf("CNY") != -1
                || strPrefix.IndexOf("cny") != -1
                || strPrefix.IndexOf("ＣＮＹ") != -1
                || strPrefix.IndexOf("ｃｎｙ") != -1
                || strPrefix.IndexOf('￥') != -1)
            {
                bCNY = true;
                goto DONE;
            }

            if (strPostfix.IndexOf("元") != -1)
            {
                bCNY = true;
                goto DONE;
            }

        DONE:
            // 人民币
            if (bCNY == true)
                return "CNY" + strValue;

            // 其他货币
            return strPrefix + strValue + strPostfix;

        }

        // 分析价格参数
        public static int ParsePriceUnit(string strString,
            out string strPrefix,
            out string strValue,
            out string strPostfix,
            out string strError)
        {
            strPrefix = "";
            strValue = "";
            strPostfix = "";
            strError = "";

            strString = strString.Trim();

            if (String.IsNullOrEmpty(strString) == true)
            {
                strError = "价格字符串为空";
                return -1;
            }

            bool bInPrefix = true;

            for (int i = 0; i < strString.Length; i++)
            {
                if ((strString[i] >= '0' && strString[i] <= '9')
                    || strString[i] == '.')
                {
                    bInPrefix = false;
                    strValue += strString[i];
                }
                else
                {
                    if (bInPrefix == true)
                        strPrefix += strString[i];
                    else
                    {
                        strPostfix = strString.Substring(i).Trim();
                        break;
                    }
                }
            }

            return 0;
        }

        // 从一个纯路径(不含url部分)中截取库名部分
        public static string GetDbName(string strLongPath)
        {
            int nRet = strLongPath.IndexOf("/");
            if (nRet == -1)
                return strLongPath;
            else
                return strLongPath.Substring(0, nRet);
        }

        // 从一个纯路径(不含url部分)中截取记录id部分
        public static string GetRecordId(string strLongPath)
        {
            int nRet = strLongPath.IndexOf("/");
            if (nRet == -1)
                return strLongPath;
            else
                return strLongPath.Substring(nRet + 1).Trim();
        }

        public static string MakeListString(List<string> names,
    string strSep)
        {
            string strResult = "";
            for (int i = 0; i < names.Count; i++)
            {
                if (i > 0)
                    strResult += strSep;
                strResult += names[i];
            }

            return strResult;
        }

        /*
        public static string MakeListString(List<string> list)
        {
            string strResult = "";
            for (int i = 0; i < list.Count; i++)
            {
                if (i > 0)
                    strResult += ",";
                strResult += list[i];
            }

            return strResult;
        }*/

        public static void SetHtmlString(WebBrowser webBrowser,
string strHtml)
        {

            /*
            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                webBrowser.Navigate("about:blank");
                doc = webBrowser.Document;
                Debug.Assert(doc != null, "doc不应该为null");
            }

            doc = doc.OpenNew(true);
            doc.Write(strHtml);
             * */
            webBrowser.DocumentText = strHtml;


        }

        /*

        // 将MARC格式转换为XML格式
        public static int ConvertMarcToXml(
            string strMarcSyntax,
            string strMARC,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            int nRet = 0;

            MemoryStream s = new MemoryStream();

            MarcXmlWriter writer = new MarcXmlWriter(s, Encoding.UTF8);

            // 在当前没有定义MARC语法的情况下，默认unimarc
            if (String.IsNullOrEmpty(strMarcSyntax) == true)
                strMarcSyntax = "unimarc";

            if (strMarcSyntax == "unimarc")
            {
                writer.MarcNameSpaceUri = DpNs.unimarcxml;
                writer.MarcPrefix = strMarcSyntax;
            }
            else if (strMarcSyntax == "usmarc")
            {
                writer.MarcNameSpaceUri = Ns.usmarcxml;
                writer.MarcPrefix = strMarcSyntax;
            }
            else
            {
                strError = "strMarcSyntax值应当为unimarc和usmarc之一";
                return -1;
            }

            // string strDebug = strMARC.Replace((char)Record.FLDEND, '#');
            nRet = writer.WriteRecord(strMARC,
                out strError);
            if (nRet == -1)
                return -1;

            writer.Flush();
            s.Flush();

            byte[] baContent = s.ToArray();
            strXml = ByteArray.ToString(baContent, Encoding.UTF8);
            return 0;
        }
         * */

        // 将浏览器中已有的内容刷新，并为后面输出的纯文本显示做好准备
        public static void Clear(WebBrowser webBrowser)
        {
            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                webBrowser.Navigate("about:blank");
                doc = webBrowser.Document;
            }

            doc = doc.OpenNew(true);
        }

        /*
        public static void AppendHtml(WebBrowser webBrowser,
            string strText)
        {
            WriteHtml(webBrowser,
                strText);
            ScrollToEnd(webBrowser);
        }*/

        // 不支持异步调用
        public static void WriteHtml(WebBrowser webBrowser,
    string strHtml)
        {

            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                webBrowser.Navigate("about:blank");
                doc = webBrowser.Document;
            }

            // doc = doc.OpenNew(true);
            doc.Write(strHtml);


        }

        public static string GetText(WebBrowser webBrowser)
        {
            HtmlDocument doc = webBrowser.Document;

            if (doc == null)
            {
                webBrowser.Navigate("about:blank");
                doc = webBrowser.Document;
            }

            return doc.Body.InnerText;
        }

        // 保持末行可见
        public static void ScrollToEnd(WebBrowser webBrowser)
        {
            HtmlDocument doc = webBrowser.Document;
            doc.Window.ScrollTo(0, 0x7fffffff);
        }
    }
}
