using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;

namespace DigitalPlatform.Text
{
    public class StringUtil
    {
        public static string SpecialChars = "！·＃￥％……—＊（）——＋－＝［］《》＜＞，。？／＼｜｛｝“”‘’•";

        public static string GetPercentText(long uploaded, long length)
        {
            return String.Format("{0,3:N}", ((double)uploaded / (double)length) * (double)100) + "%";
        }

        public static string[] units = new string[] { "K", "M", "G", "T" };
        public static string GetLengthText(long length)
        {
            decimal v = length;
            int i = 0;
            foreach (string strUnit in units)
            {
                v = decimal.Round(v / 1024, 2);
                if (v < 1024 || i >= units.Length - 1)
                    return v.ToString() + strUnit;

                i++;
            }

            return length.ToString();
        }

        // 获得一个字符串的 UTF-8 字节数
        public static int GetUtf8Bytes(string text)
        {
            return Encoding.UTF8.GetByteCount(text);
        }

        // 规范为半角字符串
        public static void CanonializeWideChars(List<string> values)
        {
            for (int i = 0; i < values.Count; i++)
            {
                string value = values[i];
                string new_value = ToDBC(value);
                if (value != new_value)
                {
                    values[i] = new_value;
                }
            }
        }

        // /
        // / 转半角的函数(DBC case)
        // /
        // /任意字符串
        // /半角字符串
        // /
        // /全角空格为12288，半角空格为32
        // /其他字符半角(33-126)与全角(65281-65374)的对应关系是：均相差65248
        // /
        public static string ToDBC(String input)
        {
            char[] c = input.ToCharArray();
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] == 12288)
                {
                    c[i] = (char)32;
                    continue;
                }
                if (c[i] > 65280 && c[i] < 65375)
                    c[i] = (char)(c[i] - 65248);
            }
            return new String(c);
        }

        public static bool IsHttpUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
                return false;
            url = url.ToLower();
            if (url.StartsWith("http:") || url.StartsWith("https"))
                return true;
            return false;
        }

        #region IP 地址匹配

        // 匹配 ip 地址列表
        // parameters:
        //      strList IP 地址列表。例如 localhost|192.168.1.1|192.168.*.*
        //      strIP   要检测的一个 IP 地址
        public static bool MatchIpAddressList(string strList, string strIP)
        {
            if (strList == null)
                return false;

            string[] list = strList.Split(new char[] { '|' });
            foreach (string pattern in list)
            {
                if (MatchIpAddress(pattern, strIP) == true)
                    return true;
            }

            return false;
        }

        public static bool MatchIpAddress(string pattern, string ip)
        {
            ip = CanonicalizeIP(ip);
            pattern = CanonicalizeIP(pattern);

            if (pattern == ip)
                return true;
            return false;
        }

        // 正规化 IP 地址
        public static string CanonicalizeIP(string ip)
        {
            if (ip == "::1" || ip == "127.0.0.1")
                return "localhost";
            return ip;
        }

        #endregion

        public static string GetMd5(string strText)
        {
            MD5 hasher = MD5.Create();
            byte[] buffer = Encoding.UTF8.GetBytes(strText);
            byte[] target = hasher.ComputeHash(buffer);
            StringBuilder sb = new StringBuilder();
            foreach (byte b in target)
            {
                sb.Append(b.ToString("x2").ToLower());
            }

            return sb.ToString();
        }

        // 去掉外围括住的符号
        // parameters:
        //      pairs   若干对打算去除的符号。例如 "%%" "()" "[](){}"。如果包含多对符号，则从左到右匹配，用上前面的就用它处理然后返回了，后面的若干对就不发生作用了
        public static string Unquote(string strValue, string pairs)
        {
            if (string.IsNullOrEmpty(pairs))
                throw new ArgumentException("pairs 参数值不应为空", "pairs");

            if ((pairs.Length % 2) != 0)
                throw new ArgumentException("pairs 参数值的字符个数应为偶数", "pairs");

            if (string.IsNullOrEmpty(strValue) == true)
                return "";

            for (int i = 0; i < pairs.Length / 2; i++)
            {
                char left = pairs[i * 2];
                if (strValue[0] == left)
                {
                    strValue = strValue.Substring(1);
                    if (strValue.Length == 0)
                        return "";

                    char right = pairs[(i * 2) + 1];
                    if (strValue[strValue.Length - 1] == right)
                        return strValue.Substring(0, strValue.Length - 1);
                }
            }

            return strValue;
        }
#if NO
        // 去掉外围括住的符号
        public static string Unquote(string strValue, char quote)
        {
            if (string.IsNullOrEmpty(strValue) == true)
                return "";

            if (strValue[0] == quote)
                strValue = strValue.Substring(1);
            if (strValue.Length == 0)
                return "";
            if (strValue[strValue.Length - 1] == quote)
                return strValue.Substring(0, strValue.Length - 1);

            return strValue;
        }
#endif

        // 注: 和 GetStyleParam() 函数相似
        // parameters:
        //      strPrefix 前缀。例如 "getreaderinfo"
        //      strDelimiter    前缀和后面参数的分隔符号。例如 ":"
        // return:
        //      null    没有找到前缀
        //      ""      找到了前缀，并且值部分为空
        //      其他     返回值部分
        public static string GetParameterByPrefix(string strList,
            string strPrefix,
            string strDelimiter = ":")
        {
            if (string.IsNullOrEmpty(strList) == true)
            {
                if (strPrefix == "")
                    return "";
                return null;
            }

            string[] list = strList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in list)
            {
                if (s.StartsWith(strPrefix + strDelimiter) == true)
                    return s.Substring(strPrefix.Length + strDelimiter.Length);
                if (s == strPrefix)
                    return "";
            }

            return null;
        }

        // 2017/11/22
        public static string SetParameterByPrefix(string strList,
            string strPrefix,
            string strDelimiter = ":",
            string strValue = null)
        {
            if (string.IsNullOrEmpty(strList) == true)
                strList = "";

            List<string> results = new List<string>();
            string[] list = strList.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string s in list)
            {
                if (s.StartsWith(strPrefix + strDelimiter) == true
                    || s == strPrefix)
                {
                    // 注: strValue 为 ""，会产生 'prefix:'；strValue 为 null，则这个 prefix 在内容中会被完整删除
                    if (strValue == null)
                        continue;
                    results.Add(strPrefix + strDelimiter + strValue);
                }
                else
                    results.Add(s);
            }

            return StringUtil.MakePathList(results, ",");
        }

        public static bool IsValidCMIS(string strText)
        {
            if (string.IsNullOrEmpty(strText))
                return false;
            if (strText.Length == 16)
            {
                if (IsPureNumber(strText))
                    return true;
                return false;
            }

            if (strText.Length == 19)
            {
                char ch = strText[0];
                if (
                    // ch == 'G' || ch == 'J' || ch == 'L'
                    char.IsLetter(ch) && char.IsUpper(ch)
                    )
                {
                    string strMiddle = strText.Substring(1, 16);    // 中间 16 位
                    if (IsPureNumber(strMiddle) == false)
                        return false;
                    // 最后两位可能是数字或者大写字母
                    if (char.IsLetterOrDigit(strText[18]) == false
                        || char.IsLower(strText[18]) == true
                        || char.IsLetterOrDigit(strText[17]) == false
                        || char.IsLower(strText[17]) == true)
                        return false;
                    return true;
                }
                else
                    return false;
            }

            return false;
        }

        // 检测一个号码字符串是否在指定的范围内
        public static bool Between(string strNumber,
            string strStart,
            string strEnd)
        {
            if (strStart.Length != strEnd.Length)
                throw new ArgumentException("strStart 和 strEnd 应当字符数相同");
            if (strNumber == null)
                throw new ArgumentException("strNumber 参数值不能为 null");

            if (strNumber.Length != strStart.Length)
                return false;
            if (string.Compare(strNumber, strStart) < 0)
                return false;
            if (string.Compare(strNumber, strEnd) > 0)
                return false;
            return true;
        }

        public static int CompareVersion(string strVersion1, string strVersion2)
        {
            if (string.IsNullOrEmpty(strVersion1) == true)
                strVersion1 = "0.0";
            if (string.IsNullOrEmpty(strVersion2) == true)
                strVersion2 = "0.0";

            try
            {
                Version version1 = new Version(strVersion1);
                Version version2 = new Version(strVersion2);

                return version1.CompareTo(version2);
            }
            catch (Exception ex)
            {
                throw new Exception("比较版本号字符串 '" + strVersion1 + "' 和 '" + strVersion2 + "' 过程出现异常: " + ex.Message, ex);
            }
        }

        // 从一个纯路径(不含url部分)中截取库名部分
        // parameters:
        //      strPath 路径字符串。例如 “中文图书/1”
        public static string GetDbName(string strPath)
        {
            int nRet = strPath.IndexOf("/");
            if (nRet == -1)
                return strPath;
            else
                return strPath.Substring(0, nRet);
        }

        // 从一个纯路径(不含url部分)中截取记录id部分
        // parameters:
        //      strPath 路径字符串。例如 “中文图书/1”
        public static string GetRecordId(string strPath)
        {
            int nRet = strPath.IndexOf("/");
            if (nRet == -1)
                return strPath;
            else
                return strPath.Substring(nRet + 1).Trim();
        }

        public static bool IsEqualOrSubPath(string strShort, string strLong)
        {
            string strDbName1 = GetFirstPartPath(ref strShort);
            string strDbName2 = GetFirstPartPath(ref strLong);

            if (strDbName1 != strDbName2)
                return false;
            string strRecordID1 = GetFirstPartPath(ref strShort);
            string strRecordID2 = GetFirstPartPath(ref strLong);

            if (strRecordID1 != strRecordID2)
                return false;

            return true;
        }

        // 将 strMime 的左边部分和 strLeftParam 进行比较
        // return:
        //      false   不匹配
        //      true    匹配
        public static bool MatchMIME(string strMime, string strLeftParam)
        {
            string strLeft = "";
            string strRight = "";
            ParseTwoPart(strMime, "/", out strLeft, out strRight);
            if (string.Compare(strLeft, strLeftParam, true) == 0)
                return true;
            return false;
        }

        // 
        /// <summary>
        /// 过滤掉最外面的 {} 字符
        /// </summary>
        /// <param name="strText">待过滤的字符串</param>
        /// <returns>过滤后的字符串</returns>
        public static string GetPureSelectedValue(string strText)
        {
            for (; ; )
            {
                int nRet = strText.IndexOf("{");
                if (nRet == -1)
                    return strText;
                int nStart = nRet;
                nRet = strText.IndexOf("}", nStart + 1);
                if (nRet == -1)
                    return strText;
                int nEnd = nRet;
                strText = strText.Remove(nStart, nEnd - nStart + 1).Trim();
            }
        }

        #region 和 Application 有关的功能

        public static bool IsDevelopMode()
        {
#if NO
            string[] args = Environment.GetCommandLineArgs();
            int i = 0;
            foreach(string arg in args)
            {
                if (i > 0 && arg == "develop")
                    return true;
                i++;
            }

            return false;
#endif
            List<string> args = GetCommandLineArgs();
            return args.IndexOf("develop") != -1;
        }

        public static bool IsNewInstance()
        {
            List<string> args = GetCommandLineArgs();
            return args.IndexOf("newinstance") != -1;
        }

        // 取得命令行参数
        // 丢掉第一个元素
        public static List<string> GetCommandLineArgs()
        {
            string[] args = Environment.GetCommandLineArgs();

            List<string> list = new List<string>(args);
            if (list.Count == 0)
                return new List<string>();
            list.RemoveAt(0);
            return list;
        }

        public static List<string> GetClickOnceCommandLineArgs(string query)
        {
            List<string> args = new List<string>();
            if (!string.IsNullOrEmpty(query) && query.StartsWith("?"))
            {
                args = StringUtil.SplitList(query.Substring(1), '&');
                for (int i = 0; i < args.Count; i++)
                {
                    args[i] = HttpUtility.UrlDecode(args[i]);
                }
            }

            return args;
        }
        #endregion

        // 在列表中寻找指定前缀的元素
        public static List<string> FindPrefixInList(List<string> list,
            string strPrefix)
        {
            List<string> results = new List<string>();
            foreach (string s in list)
            {
                if (s.StartsWith(strPrefix) == true)
                    results.Add(s);
            }

            return results;
        }

        public static Hashtable ParseMetaDataXml(string strXml,
    out string strError)
        {
            strError = "";
            Hashtable result = new Hashtable();

            if (string.IsNullOrEmpty(strXml) == true)
                return result;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return null;
            }

            if (dom.DocumentElement == null)
                return result;

            XmlAttributeCollection attrs = dom.DocumentElement.Attributes;
            foreach (XmlAttribute attr in dom.DocumentElement.Attributes)
            {
                result.Add(attr.Name, attr.Value);
            }
#if NO
            for (int i = 0; i < attrs.Count; i++)
            {
                string strName = attrs[i].Name;
                string strValue = attrs[i].Value;

                result.Add(strName, strValue);
            }
#endif

            return result;
        }

        public static void ChangeMetaData(ref string strMetaData,
string strID,
string strLocalPath,
string strMimeType,
string strLastModified,
string strPath,
string strTimestamp)
        {
            XmlDocument dom = new XmlDocument();

            if (strMetaData == "")
                strMetaData = "<file/>";

            dom.LoadXml(strMetaData);

            if (strID != null)
                dom.DocumentElement.SetAttribute("id", strID);

            if (strLocalPath != null)
                dom.DocumentElement.SetAttribute("localpath", strLocalPath);

            if (strMimeType != null)
                dom.DocumentElement.SetAttribute("mimetype", strMimeType);

            if (strLastModified != null)
                dom.DocumentElement.SetAttribute("lastmodified", strLastModified);

            if (strPath != null)
                dom.DocumentElement.SetAttribute("path", strPath);

            if (strTimestamp != null)
                dom.DocumentElement.SetAttribute("timestamp", strTimestamp);

            strMetaData = dom.OuterXml;
        }

        // 解析对象路径
        // parameters:
        //      strPathParam    等待解析的路径
        //      strXmlRecPath   返回元数据记录路径
        //      strObjectID     返回对象 ID
        // return:
        //      false   不是记录路径
        //      true    是记录路径
        public static bool ParseObjectPath(string strPathParam,
            out string strXmlRecPath,
            out string strObjectID)
        {
            strXmlRecPath = "";
            strObjectID = "";

            if (string.IsNullOrEmpty(strPathParam) == true)
                return false;

            string strPath = strPathParam;

            string strDbName = StringUtil.GetFirstPartPath(ref strPath);

            string strRecordID = StringUtil.GetFirstPartPath(ref strPath);

            if (string.IsNullOrEmpty(strRecordID) == true)
                return false;

            // 记录ID
            if (StringUtil.IsPureNumber(strRecordID) == false)
                return false;

            strXmlRecPath = strDbName + "/" + strRecordID;

            // 只到记录ID这一层
            if (string.IsNullOrEmpty(strPath) == true)
                return true;

            string strObject = StringUtil.GetFirstPartPath(ref strPath);

            // 书目记录名下的外部 URL
            if (strObject == "url")
            {
                strObjectID = strPath;
                return true;
            }

            // 对象资源
            if (strObject != "object")
                return false;

            strObjectID = StringUtil.GetFirstPartPath(ref strPath);
            return true;
        }

        // 
        /// <summary>
        /// 兑现字符串中的宏值
        /// </summary>
        /// <param name="macro_table">宏对照表。宏名字 --> 值</param>
        /// <param name="strInputString">输入字符串</param>
        /// <returns>兑现后的字符串</returns>
        public static string MacroString(Hashtable macro_table,
            string strInputString)
        {
            foreach (string strMacroName in macro_table.Keys)
            {
                strInputString = strInputString.Replace(strMacroName, (string)macro_table[strMacroName]);
            }

            return strInputString;
        }


        /// <summary>
        /// 解析一个字符串中的数字和单位
        /// </summary>
        /// <param name="strText">要解释的字符串</param>
        /// <param name="strValue">返回数字部分</param>
        /// <param name="strUnit">返回单位部分</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 正确</returns>
        public static int ParseUnit(string strText,
    out string strValue,
    out string strUnit,
    out string strError)
        {
            strValue = "";
            strUnit = "";
            strError = "";

            if (String.IsNullOrEmpty(strText) == true)
            {
                strError = "strText 值不应为空";
                return -1;
            }

            strText = strText.Trim();

            if (String.IsNullOrEmpty(strText) == true)
            {
                strError = "strText 值除去两端空格后不应为空";
                return -1;
            }

            StringBuilder text = new StringBuilder();
            int i = 0;
            foreach (char ch in strText)
            {
                if (ch >= '0' && ch <= '9')
                {
                    text.Append(ch);
                }
                else
                {
                    strUnit = strText.Substring(i).Trim();
                    break;
                }
                i++;
            }

            strValue = text.ToString();
            return 0;
        }

        // 分析期限参数
        public static int ParsePeriodUnit(string strPeriod,
            out long lValue,
            out string strUnit,
            out string strError)
        {
            lValue = 0;
            strUnit = "";
            strError = "";

            strPeriod = strPeriod.Trim();

            if (String.IsNullOrEmpty(strPeriod) == true)
            {
                strError = "期限字符串为空";
                return -1;
            }

            string strValue = "";


            for (int i = 0; i < strPeriod.Length; i++)
            {
                if (strPeriod[i] >= '0' && strPeriod[i] <= '9')
                {
                    strValue += strPeriod[i];
                }
                else
                {
                    strUnit = strPeriod.Substring(i).Trim();
                    break;
                }
            }

            // 将strValue转换为数字
            try
            {
                lValue = Convert.ToInt64(strValue);
            }
            catch (Exception)
            {
                strError = "期限参数数字部分'" + strValue + "'格式不合法";
                return -1;
            }

            if (String.IsNullOrEmpty(strUnit) == true)
                strUnit = "day";   // 缺省单位为"天"

            strUnit = strUnit.ToLower();    // 统一转换为小写

            return 0;
        }


        public static string EscapeString(string strText, string speical_chars)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return "";

            StringBuilder text = new StringBuilder();
            foreach (char ch in strText)
            {
                if (ch != '%' && speical_chars.IndexOf(ch) == -1)
                    text.Append(ch);
                else
                    text.Append(Uri.HexEscape(ch));
            }

            return text.ToString();
        }

        public static string UnescapeString(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return "";

            StringBuilder result = new StringBuilder();
            int len = strText.Length;
            int i = 0;
            while (i < len)
            {
                if (Uri.IsHexEncoding(strText, i))
                    result.Append(Uri.HexUnescape(strText, ref i));
                else
                    result.Append(strText[i++]);
            }

            return result.ToString();
        }

        // 2014/3/7
        // 从style字符串中得到 format:XXXX子串
        public static string GetStyleParam(string strStyle, string strParamName)
        {
            string strHead = strParamName + ":";
            string[] parts = strStyle.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string strPart in parts)
            {
                string strText = strPart.Trim();
                if (strText.StartsWith(strHead) == true)
                    return strText.Substring(strHead.Length).Trim();
#if NO
                if (StringUtil.HasHead(strText, strParamName + ":") == true)
                    return strText.Substring((strParamName + ":").Length).Trim();
#endif
            }

            return null;
        }

        // 接续追加两个 string []
        public static string[] Append(string[] a1, string[] a2)
        {
            if (a1 == null && a2 == null)
                return null;
            if (a1 == null)
                return a2;
            if (a2 == null)
                return a1;

            Debug.Assert(a1 != null && a2 != null, "");

            string[] result = new string[a1.Length + a2.Length];
            Array.Copy(a1, 0, result, 0, a1.Length);
            Array.Copy(a2, 0, result, a1.Length, a2.Length);
            return result;
        }

        public static List<string> ParseTwoPart(string strText, string strSep)
        {
            string strLeft = "";
            string strRight = "";
            ParseTwoPart(strText, strSep, out strLeft, out strRight);
            List<string> results = new List<string>();
            results.Add(strLeft);
            results.Add(strRight);
            return results;
        }

        public static List<string> ParseTwoPart(string strText, string[] seps)
        {
            string strLeft = "";
            string strRight = "";

            if (string.IsNullOrEmpty(strText) == true)
                goto END1;

            int nRet = 0;
            string strSep = "";
            foreach (string sep in seps)
            {
                nRet = strText.IndexOf(sep);
                if (nRet != -1)
                {
                    strSep = sep;
                    goto FOUND;
                }
            }

            strLeft = strText;
            goto END1;

        FOUND:
            Debug.Assert(nRet != -1, "");
            strLeft = strText.Substring(0, nRet).Trim();
            strRight = strText.Substring(nRet + strSep.Length).Trim();

        END1:
            List<string> results = new List<string>();
            results.Add(strLeft);
            results.Add(strRight);
            return results;
        }

        /// <summary>
        /// 通用的，切割两个部分的函数
        /// </summary>
        /// <param name="strText">要处理的字符串</param>
        /// <param name="strSep">分隔符号</param>
        /// <param name="strPart1">返回第一部分</param>
        /// <param name="strPart2">返回第二部分</param>
        public static void ParseTwoPart(string strText,
            string strSep,
            out string strPart1,
            out string strPart2)
        {
            strPart1 = "";
            strPart2 = "";

            if (string.IsNullOrEmpty(strText) == true)
                return;

            int nRet = strText.IndexOf(strSep);
            if (nRet == -1)
            {
                strPart1 = strText;
                return;
            }

            strPart1 = strText.Substring(0, nRet).Trim();
            strPart2 = strText.Substring(nRet + strSep.Length).Trim();
        }

        /// <summary>
        /// 正规化 WCF 主机 URL。确保最后有一个 '/'
        /// </summary>
        /// <param name="strUrl">待处理的 URL 字符串</param>
        /// <returns>返回正规化以后的字符串</returns>
        public static string CanonicalizeHostUrl(string strUrl)
        {
            if (string.IsNullOrEmpty(strUrl) == true)
                return "";

            if (strUrl[strUrl.Length - 1] != '/')
                strUrl += "/";

            return strUrl;
        }

        // http://stackoverflow.com/questions/1341847/special-character-in-xpath-query
        /// <summary>
        /// Produce an XPath literal equal to the value if possible; if not, produce
        /// an XPath expression that will match the value.
        /// 
        /// Note that this function will produce very long XPath expressions if a value
        /// contains a long run of double quotes.
        /// </summary>
        /// <param name="value">The value to match.</param>
        /// <returns>If the value contains only single or double quotes, an XPath
        /// literal equal to the value.  If it contains both, an XPath expression,
        /// using concat(), that evaluates to the value.</returns>
        public static string XPathLiteral(string value)
        {
            // if the value contains only single or double quotes, construct
            // an XPath literal
            if (!value.Contains("\""))
            {
                return "\"" + value + "\"";
            }
            if (!value.Contains("'"))
            {
                return "'" + value + "'";
            }

            // if the value contains both single and double quotes, construct an
            // expression that concatenates all non-double-quote substrings with
            // the quotes, e.g.:
            //
            //    concat("foo", '"', "bar")
            StringBuilder sb = new StringBuilder();
            sb.Append("concat(");
            string[] substrings = value.Split('\"');
            for (int i = 0; i < substrings.Length; i++)
            {
                bool needComma = (i > 0);
                if (substrings[i] != "")
                {
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }
                    sb.Append("\"");
                    sb.Append(substrings[i]);
                    sb.Append("\"");
                    needComma = true;
                }
                if (i < substrings.Length - 1)
                {
                    if (needComma)
                    {
                        sb.Append(", ");
                    }
                    sb.Append("'\"'");
                }

            }
            sb.Append(")");
            return sb.ToString();
        }
        // 对短路径进行比较
        // 数据库名/id
        public static int CompareRecPath(string s1, string s2)
        {
            string strLeft1;
            string strRight1;
            string strLeft2;
            string strRight2;
            SplitRecPath(s1, out strLeft1, out strRight1);
            SplitRecPath(s2, out strLeft2, out strRight2);

            int nRet = String.Compare(strLeft1, strLeft2);
            if (nRet != 0)
                return nRet;

            // 对记录号部分进行右对齐的比较
            int nMaxLength = strRight1.Length;
            if (strRight2.Length > nMaxLength)
                nMaxLength = strRight2.Length;

            strRight1 = strRight1.PadLeft(nMaxLength, ' ');
            strRight2 = strRight2.PadLeft(nMaxLength, ' ');

            return String.Compare(strRight1, strRight2);
        }

        // 将记录路径切割为两个部分：左边部分和右边部分。
        // 中文图书/1
        // 右边部分是从右开始找到第一个'/'右边的部分，所以不论路径长短，一定是最右边的数字部分
        static void SplitRecPath(string strRecPath,
            out string strLeft,
            out string strRight)
        {
            int nRet = strRecPath.LastIndexOf("/");
            if (nRet == -1)
            {
                strLeft = "";
                strRight = strRecPath;  // 如果没有斜杠，则当作右边部分
                return;
            }

            strLeft = strRecPath.Substring(0, nRet);
            strRight = strRecPath.Substring(nRet + 1);
        }

        static string FindQuotePair(char ch, string[] quote_pairs)
        {
            foreach (string two_char in quote_pairs)
            {
                if (ch == two_char[0])
                    return two_char;
            }

            return null;
        }

        static bool FindStack(char ch, List<string> stack)
        {
            if (stack.Count == 0)
                return false;

            if (stack[stack.Count - 1][1] == ch)
                return true;

            return false;
        }

        // 根据分割符号切割字符串
        // parameters:
        //      quote_pairs 括号数组。每个字符串要求包含两个字符，一个是左括号，一个是右括号
        public static List<string> SplitString(string strText,
            string sep_chars,
            string[] quote_pairs,
            StringSplitOptions options = StringSplitOptions.None)
        {
            List<string> results = new List<string>();

            // 2013/4/9
            if (string.IsNullOrEmpty(strText) == true)
                return results;

            List<string> stack = new List<string>();
            StringBuilder s = new StringBuilder(4096);
            foreach (char ch in strText)
            {
                string pair = FindQuotePair(ch, quote_pairs);
                if (pair != null)
                {
                    stack.Add(pair);
                    s.Append(ch);
                    continue;
                }
                else
                {
                    if (FindStack(ch, stack) == true)
                    {
                        stack.RemoveAt(stack.Count - 1);
                        s.Append(ch);
                        continue;
                    }
                }

                if (stack.Count == 0 && sep_chars.IndexOf(ch) != -1)
                {
                    if ((options == StringSplitOptions.RemoveEmptyEntries && s.Length > 0)
                        || options == StringSplitOptions.None)
                        results.Add(s.ToString());
                    s.Clear();
                }
                else
                    s.Append(ch);
            }

            if (s.Length > 0)
                results.Add(s.ToString());

            return results;
        }

        // 合并两个字符串数组
        // 注：也可直接使用 LogicOper()
        // parameter:
        //		sourceLeft: 源左边数组
        //		sourceRight: 源右边数组
        //		targetLeft: 目标左边数组
        //		targetMiddle: 目标中间数组
        //		targetRight: 目标右边数组
        // 出错抛出异常
        public static void MergeStringList(List<string> sourceLeft,
            List<string> sourceRight,
            ref List<string> targetLeft,
            ref List<string> targetMiddle,
            ref List<string> targetRight)
        {
            int i = 0;
            int j = 0;
            string strLeft;
            string strRight;
            int ret;

            while (true)
            {
                strLeft = null;
                strRight = null;
                if (i >= sourceLeft.Count)
                {
                    i = -1;
                }
                else if (i != -1)
                {
                    try
                    {
                        strLeft = sourceLeft[i];
                    }
                    catch
                    {
                        Exception ex = new Exception("i=" + Convert.ToString(i) + "----Count=" + Convert.ToString(sourceLeft.Count) + "<br/>");
                        throw (ex);
                    }
                }
                if (j >= sourceRight.Count)
                {
                    j = -1;
                }
                else if (j != -1)
                {
                    try
                    {
                        strRight = sourceRight[j];
                    }
                    catch
                    {
                        Exception ex = new Exception("j=" + Convert.ToString(j) + "----Count=" + Convert.ToString(sourceLeft.Count) + sourceRight.GetHashCode() + "<br/>");
                        throw (ex);
                    }
                }
                if (i == -1 && j == -1)
                {
                    break;
                }

                if (strLeft == null)
                {
                    ret = 1;
                }
                else if (strRight == null)
                {
                    ret = -1;
                }
                else
                {
                    ret = strLeft.CompareTo(strRight);  //MyCompareTo(oldOneKey); //改CompareTO
                }

                if (ret == 0)
                {
                    if (targetMiddle != null)
                        targetMiddle.Add(strLeft);
                    i++;
                    j++;
                }

                if (ret < 0)
                {
                    if (targetLeft != null && strLeft != null)
                        targetLeft.Add(strLeft);
                    i++;
                }

                if (ret > 0)
                {
                    if (targetRight != null && strRight != null)
                        targetRight.Add(strRight);
                    j++;
                }
            }
        }

        // 获得纯净的馆藏地点字符串
        // dp2册记录中的<location>元素内容，有可能是类似"流通库,#reservation"这样的复杂字符串(表示在预约保留架上)。本函数专门提取非#号引导的第一部分
        public static string GetPureLocation(string strLocation)
        {
            if (string.IsNullOrEmpty(strLocation) == true)
                return "";

            strLocation = strLocation.Trim();

            string[] parts = strLocation.Split(new char[] { ',' });
            if (parts.Length <= 1)
                return strLocation;

            for (int i = 0; i < parts.Length; i++)
            {
                string strPart = parts[i].Trim();
                if (String.IsNullOrEmpty(strPart) == true)
                    continue;
                if (strPart[0] != '#')
                    return strPart;
            }

            return "";
        }

        // 只返回馆藏地点字段内容中的纯粹馆藏地点字符串
        // 返回非 #xxx 的第一个子串
        public static string GetPureLocationString(string strLocation)
        {
            if (string.IsNullOrEmpty(strLocation) == true)
                return "";

            // 去掉 #xxx, 部分
            if (strLocation.IndexOf("#") != -1)
            {
                string[] parts = strLocation.Split(new char[] { ',' });
                foreach (string s in parts)
                {
                    string strText = s.Trim();
                    if (string.IsNullOrEmpty(strText) == true)
                        continue;
                    if (strText[0] == '#')
                        continue;
                    return strText;
                }

                return "";
            }

            return strLocation;
        }

        // 替换 #reservation,xxxx 中的 xxxx 部分
        public static string SetLocationString(string strLocation, string strPureLocation)
        {
            if (string.IsNullOrEmpty(strLocation) == true)
                return strPureLocation;

            {
                List<string> results = new List<string>();
                string[] parts = strLocation.Split(new char[] { ',' });
                foreach (string s in parts)
                {
                    string strText = s.Trim();
                    if (string.IsNullOrEmpty(strText) == true)
                        continue;
                    if (strText[0] == '#')
                        results.Add(strText);
                }

                results.Add(strPureLocation);
                return StringUtil.MakePathList(results);
            }
        }


        public static string GetBooleanString(bool bValue)
        {
            if (bValue == true)
                return "true";
            return "false";
        }

        public static string GetBooleanString(bool bValue,
            bool bDefaultValue)
        {
            if (bValue == bDefaultValue)
                return null;

            if (bValue == true)
                return "true";
            return "false";
        }

        public static bool GetBooleanValue(string strValue,
            bool bDefaultValue)
        {
            if (String.IsNullOrEmpty(strValue) == true)
                return bDefaultValue;
            strValue = strValue.ToLower();
            if (strValue == "yes" || strValue == "on"
    || strValue == "1" || strValue == "true")
                return true;
            return false;
        }

        // 获得 "llll -- rrrrr"的左边部分
        public static string GetLeft(string strText)
        {
            // 去掉"-- ?????"部分
            string strResult = strText;
            int nRet = strResult.IndexOf("--", 0);
            if (nRet != -1)
                strResult = strResult.Substring(0, nRet).Trim();

            return strResult;
        }

        #region 和索取号有关的功能

        // 获取引导的{...}内容。注意返回值不包括花括号
        public static string GetLeadingCommand(string strLine)
        {
            if (string.IsNullOrEmpty(strLine) == true)
                return null;

            // 关注"{...}"
            if (strLine[0] == '{')
            {
                int nRet = strLine.IndexOf("}");
                if (nRet != -1)
                    return strLine.Substring(1, nRet - 1).Trim();
            }

            return null;
        }

        // 从册记录中<accessNo>元素中的原始字符串获得表示馆藏代码的第一行
        public static string GetCallNumberHeadLine(string strCallNumber)
        {
            string[] lines = strCallNumber.Split(new char[] { '/' });
            foreach (string line in lines)
            {
                string strLine = line.Trim();

                // 关注"{ns}"开头的行
                if (strLine.Length > 0 && strLine[0] == '{')
                {
                    int nRet = strLine.IndexOf("}");
                    if (nRet != -1)
                    {
                        string strCmd = strLine.Substring(0, nRet + 1).Trim().ToLower();
                        if (strCmd == "{ns}")
                        {
                            // 去掉命令部分
                            return strLine.Substring(nRet + 1).Trim();
                        }
                    }
                }
            }

            return null;    // 没有找到
        }

        // 根据册记录中<accessNo>元素中的原始字符串创建 LocationClass 字符串
        public static string BuildLocationClassEntry(string strCallNumber)
        {
            StringBuilder result = new StringBuilder(4096);
            string[] lines = strCallNumber.Split(new char[] { '/' });
            foreach (string line in lines)
            {
                string strLine = line.Trim();

                // 去掉"{ns}"开头的行
                if (strLine.Length > 0 && strLine[0] == '{')
                {
                    int nRet = strLine.IndexOf("}");
                    if (nRet != -1)
                    {
                        string strCmd = strLine.Substring(0, nRet + 1).Trim().ToLower();
                        if (strCmd == "{ns}")
                            continue;
                        // 否则也要去掉命令部分
                        strLine = strLine.Substring(nRet + 1).Trim();
                    }
                }

                if (result.Length > 0)
                    result.Append("/");
                result.Append(strLine);
            }

            return result.ToString();
        }

        // 获得纯净的索取号字符串
        public static string GetPlainTextCallNumber(string strCallNumber)
        {
            if (strCallNumber.IndexOf("{") == -1)
                return strCallNumber;

            StringBuilder result = new StringBuilder(4096);
            string[] lines = strCallNumber.Split(new char[] { '/' });
            foreach (string line in lines)
            {
                string strLine = line.Trim();

                // 去掉"{XXX}"开头的部分
                if (strLine.Length > 0 && strLine[0] == '{')
                {
                    int nRet = strLine.IndexOf("}");
                    if (nRet != -1)
                        strLine = strLine.Substring(nRet + 1).Trim();
                }

                if (result.Length > 0)
                    result.Append("/");
                result.Append(strLine);
            }

            return result.ToString();
        }

        // 右对齐比较字符串
        // parameters:
        //      chFill  填充用的字符
        public static int RightAlignCompare(string s1, string s2, char chFill = '0')
        {
            if (s1 == null)
                s1 = "";
            if (s2 == null)
                s2 = "";
            int nMaxLength = Math.Max(s1.Length, s2.Length);
            return string.CompareOrdinal(s1.PadLeft(nMaxLength, chFill),
                s2.PadLeft(nMaxLength, chFill));
        }

        // 右对齐的方式比较索取号的第一行以外的某行
        public static int CompareAccessNoRestLine(string s1, string s2)
        {
            List<OneSegment> r1 = BuildLineSegments(s1);
            List<OneSegment> r2 = BuildLineSegments(s2);
            int nMaxCount = Math.Max(r1.Count, r2.Count);

            int nRet = 0;
            for (int i = 0; i < nMaxCount; i++)
            {
                if (i >= r1.Count)
                {
                    if (i >= r2.Count)
                        return 0;   // 不分胜负
                    return 1;   // s2 更大
                }

                if (i >= r2.Count)
                {
                    if (i >= r1.Count)
                        return 0;   // 不分胜负
                    return -1;   // s1 更大
                }

                OneSegment p1 = r1[i];
                OneSegment p2 = r2[i];

                // 先比较leading
                nRet = string.CompareOrdinal(p1.Leading, p2.Leading);
                if (nRet != 0)
                    return nRet;

                // 然后右对齐比较数字部分
                nRet = RightAlignCompare(p1.Number, p2.Number);
                if (nRet != 0)
                    return nRet;
            }

            return 0;
        }

        // 对索取号的第一行以外的某行进行规整，以便进行比较
        // 按照非数字字符串切割为多个段落，每个段落右对齐比较
        public static List<OneSegment> BuildLineSegments(string strLine)
        {
            string strLeading = "";
            string strNumber = "";
            int nState = 0; // 0: 开始一个段落   1: 正在填入 leading   2: 正在填入 number

            List<OneSegment> results = new List<OneSegment>();

            foreach (char ch in strLine)
            {
                if (ch >= '0' && ch <= '9')
                {
                    if (nState == 0)
                    {
                        strNumber += ch;
                        nState = 2;
                        continue;
                    }
                    if (nState == 1)
                    {
                        strNumber += ch;
                        nState = 2;
                        continue;
                    }
                    if (nState == 2)
                    {
                        strNumber += ch;
                        nState = 2;
                        continue;
                    }
                }
                else
                {
                    // 非数字字符
                    if (nState == 0)
                    {
                        strLeading += ch;
                        nState = 1;
                        continue;
                    }
                    if (nState == 1)
                    {
                        strLeading += ch;
                        nState = 1;
                        continue;
                    }
                    if (nState == 2)
                    {
                        // 结束一个段落
                        OneSegment segment = new OneSegment();
                        segment.Leading = strLeading;
                        segment.Number = strNumber;
                        results.Add(segment);

                        // 重新开始一个段落
                        strNumber = "";
                        strLeading = "";
                        strLeading += ch;
                        nState = 1;
                        continue;
                    }
                }
            }

            // 最后一个段落
            if (string.IsNullOrEmpty(strLeading) == false
                || string.IsNullOrEmpty(strNumber) == false)
            {
                // 结束一个段落
                OneSegment segment = new OneSegment();
                segment.Leading = strLeading;
                segment.Number = strNumber;
                results.Add(segment);
            }

            return results;
        }

        public class OneSegment
        {
            public string Leading = ""; // 引导符号。一般是一个非数字的字符串
            public string Number = "";  // 纯数字的字符串
        }

        // 比较两个索取号的大小
        // return:
        //      <0  s1 < s2
        //      ==0 s1 == s2
        //      >0  s1 > s2
        public static int CompareAccessNo(string s1,
            string s2,
            bool bRemoveNoSortLine = false)
        {
            if (bRemoveNoSortLine == true)
            {
                // 2013/3/31 去掉表示馆藏地的第一行
                if (s1 != null && s1.IndexOf("{") != -1)
                    s1 = StringUtil.BuildLocationClassEntry(s1);
                if (s2 != null && s2.IndexOf("{") != -1)
                    s2 = StringUtil.BuildLocationClassEntry(s2);
            }

            string[] parts1 = s1.Split(new char[] { '/' });
            string[] parts2 = s2.Split(new char[] { '/' });

            int nRet = 0;
            int nMaxCount = Math.Max(parts1.Length, parts2.Length);
            for (int i = 0; i < nMaxCount; i++)
            {
                if (i >= parts1.Length) // 2013/3/27 s1.Length BUG!!!
                {
                    if (i >= parts2.Length)
                        return 0;   // 不分胜负
                    return 1;   // s2 更大
                }

                if (i >= parts2.Length)
                {
                    if (i >= parts1.Length)
                        return 0;   // 不分胜负
                    return -1;   // s1 更大
                }

                string p1 = parts1[i];
                string p2 = parts2[i];

                // 第一行采用左对齐进行比较
                if (i == 0)
                {
                    nRet = string.CompareOrdinal(p1, p2);
                    if (nRet != 0)
                        return nRet;
                    continue;
                }

                // 其他行用右对齐进行比较
                nRet = CompareAccessNoRestLine(p1, p2);
                if (nRet != 0)
                    return nRet;
            }

            return 0;
        }



        #endregion

        // 获得有限的行数
        public static string GetSomeLines(string strText,
            int nMaxLines)
        {
            StringBuilder result = new StringBuilder(4096);
            string[] lines = strText.Replace("\r\n", "\n").Split(new char[] { '\n' });
            for (int i = 0; i < Math.Min(lines.Length, nMaxLines); i++)
            {
                result.Append(lines[i] + "\r\n");
            }

            return result.ToString();
        }

        // 是否身份证号?
        public static bool IsIdcardNumber(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return false;

            if (strText.Length != 18
                && strText.Length != 15)
                return false;

            if (strText.Length == 18)
            {
                string strPrev = strText.Substring(0, 17);
                if (StringUtil.IsPureNumber(strPrev) == false)
                    return false;

                // 最后一位可能是 'X'
                char tail = strText[17];
                if (tail == 'X' || tail == 'x')
                    return true;
                if (tail >= '0' && tail <= '9')
                    return true;

                return false;
            }

            Debug.Assert(strText.Length == 15, "");

            if (StringUtil.IsPureNumber(strText) == false)
                return false;

            return true;
        }

        // 替换字符串最前面一段连续的字符
        public static string ReplaceContinue(string strText,
    char ch1,
    char ch2)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return strText;
            bool bOn = true;
            StringBuilder result = new StringBuilder(4096);
            foreach (char ch in strText)
            {
                if (ch == ch1 && bOn == true)
                    result.Append(ch2);
                else
                {
                    bOn = false;
                    result.Append(ch);
                }
            }

            return result.ToString();
        }

        public static string Trim(string strText)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return strText;
            return strText.Trim();
        }
        static int[] iSign =   {65306,
                            8220,
                            65307,
                            8216,
                            65292,
                            65281,
                            12289,

65311,
8212,
12290,
12298,
12297,
8230,
65509,
65288,
65289,8217,8221};

        public static bool IsHanzi(char ch)
        {
            int n = (int)ch;
            if (n <= 0X1ef3) // < 1024
                return false;
            foreach (int v in iSign)
            {
                if (ch == v)
                    return false;
            }

            return true;
        }

        // 是否包含一个以上的汉字
        public static bool ContainHanzi(string strText)
        {
            foreach (char ch in strText)
            {
                if (IsHanzi(ch) == true)
                    return true;
            }

            return false;
        }

        public static bool IsEqualList(string strList1, string strList2)
        {
            List<string> list1 = SplitList(strList1);
            List<string> list2 = SplitList(strList2);

            return IsEqualList(list1, list2);
        }

        // 两个列表内的元素是否相同？(注：不考虑排列顺序)
        public static bool IsEqualList(List<string> list1, List<string> list2)
        {
            if (list1.Count != list2.Count)
                return false;
            foreach (string s1 in list1)
            {
                if (list2.IndexOf(s1) == -1)
                    return false;
            }

            foreach (string s2 in list2)
            {
                if (list1.IndexOf(s2) == -1)
                    return false;
            }

            return true;
        }

        // 2011/12/12
        // 去掉列表中的空字符串，并且去掉每个元素的首尾空白
        public static void RemoveBlank(ref List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string strText = list[i].Trim();
                if (string.IsNullOrEmpty(strText) == true)
                {
                    list.RemoveAt(i);
                    i--;
                    continue;
                }
                if (strText != list[i])
                    list[i] = strText;
            }
        }

        public static List<string> SplitList(string strText)
        {
            // 2011/12/26
            if (string.IsNullOrEmpty(strText) == true)
                return new List<string>();

            string[] parts = strText.Split(new char[] { ',' });
            List<string> results = new List<string>();
            results.AddRange(parts);
            return results;
        }

        public static List<string> SplitList(string strText,
            char delimeter)
        {
            // 2011/12/26
            if (string.IsNullOrEmpty(strText) == true)
                return new List<string>();

            string[] parts = strText.Split(new char[] { delimeter });
            List<string> results = new List<string>();
            results.AddRange(parts);
            return results;
        }

        // 2015/7/16
        public static List<string> SplitList(string strText,
            string strSep)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return new List<string>();

            char delimeter = (char)1;
            strText = strText.Replace(strSep, new string(delimeter, 1));

            string[] parts = strText.Split(new char[] { delimeter });
            List<string> results = new List<string>();
            results.AddRange(parts);
            return results;
        }

        public static string CircleNumberString = "①②③④⑤⑥⑦⑧⑨⑩";

        public static string GetCircleNumber(int v)
        {
            if (v <= 0 || v - 1 > CircleNumberString.Length - 1)
            {
                return "(" + v.ToString() + ")";
            }

            return new string(CircleNumberString[v - 1], 1);
        }

        public static string CutString(string strText, int nMaxLength)
        {
            if (string.IsNullOrEmpty(strText) == true)
                return "";

            if (strText.Length <= nMaxLength)
                return strText;

            return strText.Substring(0, nMaxLength) + "...";
        }

        public static int GetCommonPartLength(string s1, string s2)
        {
            int nCount = 0;
            for (int i = 0; i < s1.Length && i < s2.Length; i++)
            {
                if (s1[i] != s2[i])
                    return i;
                nCount = i;
            }

            return nCount;
        }

        public static List<string> CompactNumbers(List<string> source)
        {
            List<string> results = new List<string>();

            string strPrev = "";
            string strStart = "";
            string strTail = "";
            for (int i = 0; i < source.Count; i++)
            {
                string strCurrent = source[i];
                if (string.IsNullOrEmpty(strPrev) == false)
                {
                    string strResult = "";

                    string strError = "";
                    // 给一个被字符引导的数字增加一个数量。
                    // 例如 B019X + 1 变成 B020X
                    int nRet = IncreaseNumber(strPrev,
            1,
            out strResult,
            out strError);
                    if (nRet == -1)
                        continue;

                    if (strCurrent == strResult)
                    {
                        if (strStart == "")
                            strStart = strPrev;
                        strTail = strCurrent;
                    }
                    else
                    {
                        if (strStart != "")
                        {
                            int nLengh = GetCommonPartLength(strStart, strTail);
                            results.Add(strStart + "-" + strTail.Substring(nLengh));
                            strStart = "";
                            strTail = "";
                        }
                        else
                        {
                            results.Add(strCurrent);
                        }
                    }
                }

                strPrev = strCurrent;
            }

            if (strStart != "")
            {
                int nLengh = GetCommonPartLength(strStart, strTail);
                results.Add(strStart + "-" + strTail.Substring(nLengh));
                strStart = "";
                strTail = "";
            }

            // 2012/4/1
            if (string.IsNullOrEmpty(strPrev) == false)
            {
                results.Add(strPrev);
            }

            return results;
        }

        public static string[] FromListString(List<string> values)
        {
            string[] results = new string[values.Count];
            for (int i = 0; i < values.Count; i++)
            {
                results[i] = values[i];
            }

            return results;
        }

        // 切割一个字符串，返回List数组
        // 注，不返回其中的空字符串成员
        public static List<string> FromListString(string strList,
            char chSep = ',',
            bool bRemoveBlank = true)
        {
            List<string> results = new List<string>();
            string[] parts = strList.Split(new char[] { chSep });
            foreach (string s in parts)
            {
                string strText = s.Trim();
                if (bRemoveBlank == true)
                {
                    if (string.IsNullOrEmpty(strText) == true)
                        continue;
                }
                results.Add(strText);
            }

            return results;
        }

        public static List<string> FromStringArray(string[] values)
        {
            List<string> results = new List<string>(values);
            return results;
        }

        // 2012/5/13
        public static string BuildParameterString(Hashtable table,
            char chSegChar = ',',
            char chEqualChar = '=',
            string strEncodeStyle = "")
        {
            StringBuilder result = new StringBuilder(4096);
            foreach (string key in table.Keys)
            {
                if (result.Length > 0)
                    result.Append(chSegChar);
                string strValue = (string)table[key];

                if (strEncodeStyle == "url")
                    result.Append(key + new string(chEqualChar, 1) + HttpUtility.UrlEncode(strValue));
                else
                    result.Append(key + new string(chEqualChar, 1) + strValue);
            }

            return result.ToString();
        }

        // 2014/8/23
        // 按照指定的 key 名字集合顺序和个数输出
        public static string BuildParameterString(Hashtable table,
            List<string> keys,
            char chSegChar = ',',
            char chEqualChar = '=',
            string strEncodeStyle = "")
        {
            StringBuilder result = new StringBuilder(4096);
            foreach (string key in keys)
            {
                if (result.Length > 0)
                    result.Append(chSegChar);
                string strValue = (string)table[key];

                if (strEncodeStyle == "url")
                    result.Append(key + new string(chEqualChar, 1) + HttpUtility.UrlEncode(strValue));
                else
                    result.Append(key + new string(chEqualChar, 1) + strValue);
            }

            return result.ToString();
        }

        // 2014/8/23
        // 将参数字符串内的参数排序
        public static string SortParams(string strParams,
            char chSegChar = ',',
            char chEqualChar = '=',
            string strEncodeStyle = "")
        {
            Hashtable table = StringUtil.ParseParameters(strParams, chSegChar, chEqualChar, strEncodeStyle);
            List<string> keys = new List<string>();
            foreach (string key in table.Keys)
            {
                keys.Add(key);
            }

            keys.Sort();
            return StringUtil.BuildParameterString(table, keys, chSegChar, chEqualChar, strEncodeStyle);
        }

        // 合并两个参数表
        // 如果有同名的参数，table2的会覆盖table1
        public static Hashtable MergeParametersTable(Hashtable table1, Hashtable table2)
        {
            Hashtable new_table = new Hashtable();
            foreach (string key in table1.Keys)
            {
                new_table[key] = table1[key];
            }
            foreach (string key in table2.Keys)
            {
                new_table[key] = table2[key];
            }
            return new_table;
        }

        // 将逗号间隔的参数表解析到Hashtable中
        // parameters:
        //      strText 字符串。形态如 "名1=值1,名2=值2"
        public static Hashtable ParseParameters(string strText)
        {
            return ParseParameters(strText, ',', '=');
        }

        // 将逗号间隔的参数表解析到Hashtable中
        // parameters:
        //      strText 字符串。形态如 "名1=值1,名2=值2"
        public static Hashtable ParseParameters(string strText,
            char chSegChar,
            char chEqualChar,
            string strDecodeStyle = "")
        {
            Hashtable results = new Hashtable();

            if (string.IsNullOrEmpty(strText) == true)
                return results;

            string[] parts = strText.Split(new char[] { chSegChar });   // ','
            for (int i = 0; i < parts.Length; i++)
            {
                string strPart = parts[i].Trim();
                if (String.IsNullOrEmpty(strPart) == true)
                    continue;
                string strName = "";
                string strValue = "";
                int nRet = strPart.IndexOf(chEqualChar);    // '='
                if (nRet == -1)
                {
                    strName = strPart;
                    strValue = "";
                }
                else
                {
                    strName = strPart.Substring(0, nRet).Trim();
                    strValue = strPart.Substring(nRet + 1).Trim();
                }

                if (String.IsNullOrEmpty(strName) == true
                    && String.IsNullOrEmpty(strValue) == true)
                    continue;

                if (strDecodeStyle == "url")
                    strValue = HttpUtility.UrlDecode(strValue);

                results[strName] = strValue;
            }

            return results;
        }

        // 检测一个字符串的头部
        public static bool HasHead(string strText,
            string strHead,
            bool bIgnoreCase = false)
        {
            // 2013/9/11
            if (strText == null)
                strText = "";
            if (strHead == null)
                strHead = "";

            if (strText.Length < strHead.Length)
                return false;

            string strPart = strText.Substring(0, strHead.Length);  // BUG!!! strText.Substring(strHead.Length);

            // 2015/4/3
            if (bIgnoreCase == true)
            {
                if (string.Compare(strPart, strHead, true) == 0)
                    return true;
                return false;
            }

            if (strPart == strHead)
                return true;

            return false;
        }

        // 检测一个字符串的尾部 2015/1/3
        public static bool HasTail(string strText,
            string strTail)
        {
            if (strText == null)
                strText = "";
            if (strTail == null)
                strTail = "";

            if (strText.Length < strTail.Length)
                return false;

            string strPart = strText.Substring(strText.Length - strTail.Length, strTail.Length);

            if (strPart == strTail)
                return true;

            return false;
        }

        // 把一个字符串数组去重。调用前，不要求已经排序
        public static void RemoveDupNoSort(ref List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string strItem = list[i];
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (strItem == list[j])
                    {
                        list.RemoveAt(j);
                        j--;
                    }
                }
            }
        }

        // 把一个字符串数组去重。调用前，应当已经排序
        public static void RemoveDup(ref List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string strItem = list[i];
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (strItem == list[j])
                    {
                        list.RemoveAt(j);
                        j--;
                    }
                    else
                    {
                        i = j - 1;
                        break;
                    }
                }
            }
        }

        public static bool HasDup(List<string> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                string strItem = list[i];
                for (int j = i + 1; j < list.Count; j++)
                {
                    if (strItem == list[j])
                        return true;
                }
            }

            return false;
        }

        /*
         * ByteArray.ToString()可以起同样作用
        // 将UTF8 byte[] 内容转换为string类型。
        // 特别处理了Preamble问题。
        static public string GetUtf8String(byte[] buffer)
        {
            if (buffer.Length >= 3)
            {
                if (buffer[0] == 0xef && buffer[1] == 0xbb && buffer[2] == 0xbf)
                {
                    return Encoding.UTF8.GetString(buffuer, 3, buffer.Length - 3);
                }
            }

            return Encoding.UTF8.GetString(buffer);
        }
         * */

        // 构造路径列表字符串，逗号分隔
        public static string MakePathList(List<string> aPath)
        {
            // 2016/11/9
            if (aPath == null)
                return "";

            // 2012/9/7
            if (aPath.Count == 0)
                return "";

            string[] pathlist = new string[aPath.Count];
            aPath.CopyTo(pathlist);

            return String.Join(",", pathlist);
        }

        // 2008/11/17
        public static string MakePathList(List<string> aPath,
            string strSep)
        {
            // 2012/9/7
            if (aPath.Count == 0)
                return "";

            string[] pathlist = new string[aPath.Count];
            aPath.CopyTo(pathlist);

            return String.Join(strSep, pathlist);
        }

        // 构造路径列表字符串，逗号分隔
        public static string MakePathList(string[] pathlist)
        {
            return String.Join(",", pathlist);
        }

        #region 处理被字符引导的数字的几个函数

        // 把一个被字符引导的字符串分成三部分
        public static void SplitLedNumber(string strLedNumber,
            out string strHead,
            out string strNumber,
            out string strEnd)
        {
            strHead = strLedNumber;
            strNumber = "";
            strEnd = "";

            string strTemp = strLedNumber;
            // 定位第一个数字
            for (int i = 0; i < strTemp.Length; i++)
            {
                if (Char.IsDigit(strTemp, i) == true)
                {
                    strHead = strTemp.Substring(0, i);
                    strTemp = strTemp.Substring(i);
                    break;
                }
            }

            if (strTemp.Length > 0)
            {
                strNumber = strTemp;

                // 定位第一个数字
                for (int i = 0; i < strTemp.Length; i++)
                {
                    if (Char.IsDigit(strTemp, i) == false)
                    {
                        strNumber = strTemp.Substring(0, i);
                        strEnd = strTemp.Substring(i);
                        break;
                    }
                }
            }
        }

        public static string IncreaseNumber(string strLedNubmer,
            int nNumber)
        {
            string strResult = "";
            string strError = "";
            int nRet = IncreaseNumber(strLedNubmer,
                nNumber,
                out strResult,
                out strError);
            if (nRet == -1)
                throw (new Exception(strError));

            return strResult;
        }

        // 给一个被字符引导的数字增加一个数量。
        // 例如 B019X + 1 变成 B020X
        public static int IncreaseNumber(string strLeadNubmer,
            int nNumber,
            out string strResult,
            out string strError)
        {
            strError = "";
            strResult = strLeadNubmer;

            string strHead = "";
            string strNumber = "";
            string strEnd = "";

            // 把一个被字符引导或结尾的数字分成三部分
            StringUtil.SplitLedNumber(strLeadNubmer,
                out strHead,
                out strNumber,
                out strEnd);

            if (strNumber == "")
                strNumber = "0";
            int nWidth = strNumber.Length;
            long nValue = 0;
            try
            {
                nValue = Convert.ToInt64(strNumber);
            }
            catch (Exception ex)
            {
                strError = "数字 '" + strNumber + "' 格式不正常: " + ex.Message;
                return -1;
            }

            strNumber = Convert.ToString(nValue + nNumber).PadLeft(nWidth, '0');
            strResult = strHead + strNumber + strEnd;

            return 0;
        }


        // 该函数可能抛出异常
        // return:
        //		== 0	相等
        //		> 0	strLeadNumber1 > strLeadNumber2
        //		< 0	strLeadNumber1 < strLeadNumber2
        public static int CompareLedNumber(string strLedNumber1,
            string strLedNumber2)
        {
            // --------------------------------------
            // 处理第一个数字
            // ---------------------------------------
            string strHead1 = "";
            string strNumber1 = "";
            string strEnd1 = "";
            // 把一个被字符引导或结尾的数字分成三部分
            StringUtil.SplitLedNumber(strLedNumber1,
                out strHead1,
                out strNumber1,
                out strEnd1);

            if (strNumber1 == "")
                strNumber1 = "0";

            int nWidth1 = strNumber1.Length;

            // 可能抛出异常
            long nValue1 = 0;
            try
            {
                nValue1 = Convert.ToInt64(strNumber1);
            }
            catch (Exception ex)
            {
                throw new Exception("数字 '" + strNumber1 + "' 格式不正常: " + ex.Message);
            }


            // --------------------------------------
            // 处理第二个数字
            // ---------------------------------------
            string strHead2 = "";
            string strNumber2 = "";
            string strEnd2 = "";
            // 把一个被字符引导或结尾的数字分成三部分
            StringUtil.SplitLedNumber(strLedNumber2,
                out strHead2,
                out strNumber2,
                out strEnd2);

            if (strNumber2 == "")
                strNumber2 = "0";
            int nWidth2 = strNumber2.Length;

            // 可能抛出异常
            long nValue2 = 0;
            try
            {
                nValue2 = Convert.ToInt64(strNumber2);
            }
            catch (Exception ex)
            {
                throw new Exception("数字 '" + strNumber2 + "' 格式不正常: " + ex.Message);
            }

            return (int)(nValue1 - nValue2);
        }

        // 得到两个被字符引导的数字的较大者
        public static int GetBiggerLedNumber(string strLedNumber1,
            string strLedNumber2,
            out string strResult,
            out string strError)
        {
            strError = "";
            strResult = "";

            int nRet = 0;
            try
            {
                nRet = StringUtil.CompareLedNumber(strLedNumber1,
                    strLedNumber2);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }

            // 第2个比第1个大
            if (nRet < 0)
                strResult = strLedNumber2;
            else
                strResult = strLedNumber1;

            return 0;
        }


        #endregion

        // 给一个被字符引导的数字增加一个数量。
        // 例如 B019 + 1 变成 B020
        public static int IncreaseLeadNumber(string strText,
            int nNumber,
            out string strResult,
            out string strError)
        {
            strError = "";
            strResult = strText;

            string strHead = strText;
            string strNumber = "";

            // 定位第一个数字

            for (int i = 0; i < strText.Length; i++)
            {
                if (Char.IsDigit(strText, i) == true)
                {
                    strHead = strText.Substring(0, i);
                    strNumber = strText.Substring(i);
                    break;
                }

            }

            if (strNumber == "")
                strNumber = "0";

            int nWidth = strNumber.Length;

            long nValue = 0;
            try
            {
                nValue = Convert.ToInt64(strNumber);
            }
            catch (Exception ex)
            {
                strError = "数字 '" + strNumber + "' 格式不正常: " + ex.Message;
                return -1;
            }

            strNumber = Convert.ToString(nValue + nNumber).PadLeft(nWidth, '0');
            strResult = strHead + strNumber;
            return 0;
        }


        // 将字符串转换为byte数组
        public static byte[] GetUtf8Bytes(string strText,
            bool bIncludePreamble)
        {
            if (bIncludePreamble == true)
            {
                byte[] baPreamble = Encoding.UTF8.GetPreamble();

                byte[] baTotal = Encoding.UTF8.GetBytes(strText);

                if (baPreamble.Length > 0)
                {
                    byte[] temp = new byte[baPreamble.Length + baTotal.Length];
                    Array.Copy(baPreamble, 0, temp, 0, baPreamble.Length);
                    Array.Copy(baTotal, 0, temp, baPreamble.Length, baTotal.Length);

                    return temp;
                }

                return baTotal;
            }
            return Encoding.UTF8.GetBytes(strText);
        }


        // 把一个字符串变成一个短一些的字符串,如果长于最大长度后面加"..."
        // parameter:
        //		strText	字符串
        //		nLength	最大长度
        // 编写者: 任延华
        public static string GetShortString(string strText,
            int nMaxLength)
        {
            // <= 最大长度时，直接返回该字符串
            if (strText.Length <= nMaxLength)
                return strText;

            return strText.Substring(0, nMaxLength) + "...";
        }

        // 得到strPath的第一部分,以'/'作为间隔符,同时 strPath 缩短
        public static string GetFirstPartPath(ref string strPath)
        {
            if (string.IsNullOrEmpty(strPath) == true)
                return "";

            string strResult = "";

            int nIndex = strPath.IndexOf('/');
            if (nIndex == -1)
            {
                strResult = strPath;
                strPath = "";
                return strResult;
            }

            strResult = strPath.Substring(0, nIndex);
            strPath = strPath.Substring(nIndex + 1);

            return strResult;
        }

        // 修改字符串某一个位字符
        public static string SetAt(string strText, int index, char c)
        {
            strText = strText.Remove(index, 1);
            strText = strText.Insert(index, new string(c, 1));

            return strText;
        }

        public static string Format(string strText, params string[] list)
        {
            if (list == null)
                return strText;

            for (int i = list.Length - 1; i >= 0; i--)
            {
                string strName = "%" + Convert.ToString(i + 1);
                strText = strText.Replace(strName, list[i]);
            }

            strText = strText.Replace("%%", "%");

            return strText;
        }

        /*
        public static string Format(params string[] list)
        {
            if (list == null) 
            {
                throw(new Exception("param list不能为空"));
            }
            if (list.Length < 1) 
            {
                throw(new Exception("param list至少要有一个参数"));
            }

            string strText = list[0];

            for(int i=list.Length-1;i>=1;i--)
            {
                string strName = "%" + Convert.ToString(i);
                strText = strText.Replace(strName, list[i]);
            }

            strText = strText.Replace("%%", "%");

            return strText;
        }
        */

        public static bool IsDouble(string s)
        {
            double v = 0;
            return Double.TryParse(s, out v);
        }

        // 检测字符串是否为纯数字(前面可以包含一个'-'号)
        public static bool IsNumber(string s)
        {
            if (string.IsNullOrEmpty(s) == true)
                return false;

            bool bFoundNumber = false;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] == '-' && bFoundNumber == false)
                {
                    continue;
                }
                if (s[i] > '9' || s[i] < '0')
                    return false;
                bFoundNumber = true;
            }
            return true;
        }

        // 检测字符串是否为纯数字(不包含'-','.'号)
        public static bool IsPureNumber(string s)
        {
            if (s == null)
                return false;
            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] > '9' || s[i] < '0')
                    return false;
            }
            return true;
        }

        // 检测字符串是否为16进制数字
        public static bool IsHexNumber(string s)
        {

            if (s.Length >= 3)
            {
                if (s.Substring(0, 2) == "0x"
                    || s.Substring(0, 2) == "0X")
                {
                    s = s.Substring(2);
                }
            }

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] >= '0' && s[i] <= '9')
                    continue;
                if (s[i] >= 'a' && s[i] <= 'f')
                    continue;
                if (s[i] >= 'A' && s[i] <= 'F')
                    continue;
                return false;
            }
            return true;
        }

        //比较字符串是否符合正则表达式
        public static bool RegexCompare(string strPattern,
            RegexOptions regOptions,
            string strInstance)
        {
            Regex r = new Regex(strPattern, regOptions);
            System.Text.RegularExpressions.Match m = r.Match(strInstance);

            if (m.Success)
                return true;
            else
                return false;
        }

        public static bool RegexCompare(string strPattern,
            string strInstance)
        {
            Regex r = new Regex(strPattern, RegexOptions.IgnoreCase);
            System.Text.RegularExpressions.Match m = r.Match(strInstance);

            if (m.Success)
                return true;
            else
                return false;
        }




#if NO
        // 大小写严格匹配
        // 逗号前后没有空格
        public static bool QuickIsInList(string strSub,
            string strList)
        {
            if (strList.IndexOf(strSub) == -1)
                return false;

            string[] parts = strList.Split(new char[] {','});
            foreach (string s in parts)
            {
                if (strSub == s)
                    return true;
                string strText = s.Trim();
                if (strSub == strText)
                    return true;
            }

            return false;
        }
#endif

        /// <summary>
        /// 忽略大小写
        /// 查找一个小字符串是否包含在大字符串，
        /// 内部调isInAList函数
        /// </summary>
        /// <param name="strSub">小字符串</param>
        /// <param name="strList">大字符串</param>
        /// <returns>
        /// 1:包含
        /// 0:不包含
        /// </returns>
        public static bool IsInList(string strSub,
            string strList)
        {
            /*
            string[] aTemp;
            aTemp = strList.Split(new char[]{','});

            int nRet = strList.IndexOfAny(new char[]{' ','\t'});
            if (nRet != -1) 
            {
                for(int i=0;i<aTemp.Length;i++) {
                    aTemp[i] = aTemp[i].Trim();	// 去除左右空白
                }
            }
 
            return IsInAlist(strSub,aTemp);
            */
            return IsInList(strSub,
                strList,
                true);
        }

        // parameters:
        //		bIgnoreCase	是否忽略大小写
        public static bool IsInList(string strSub,
            string strList,
            bool bIgnoreCase)
        {
            if (String.IsNullOrEmpty(strList) == true)
                return false;	// 优化

            string[] aTemp;
            aTemp = strList.Split(new char[] { ',' });

            int nRet = strList.IndexOfAny(new char[] { ' ', '\t' });
            if (nRet != -1)
            {
                for (int i = 0; i < aTemp.Length; i++)
                {
                    aTemp[i] = aTemp[i].Trim();	// 去除左右空白
                }
            }

            return IsInAlist(strSub,
                aTemp,
                bIgnoreCase);
        }

        // TODO: 似乎可以用 IndexOf() 代替
        public static bool IsInList(int v, int[] a)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (v == a[i])
                    return true;
            }

            return false;
        }

        // 合并两list，去重
        // parameters:
        //		bIgnoreCase	是否忽略大小写
        public static string MergeList(string strList1,
            string strList2,
            bool bIgnoreCase)
        {
            string[] items1 = strList1.Split(new char[] { ',' });

            // 去掉左右空白
            int nRet = strList1.IndexOfAny(new char[] { ' ', '\t' });
            if (nRet != -1)
            {
                for (int i = 0; i < items1.Length; i++)
                {
                    items1[i] = items1[i].Trim();	// 去除左右空白
                }
            }

            string[] items2 = strList2.Split(new char[] { ',' });

            // 去掉左右空白
            nRet = strList2.IndexOfAny(new char[] { ' ', '\t' });
            if (nRet != -1)
            {
                for (int i = 0; i < items2.Length; i++)
                {
                    items2[i] = items2[i].Trim();	// 去除左右空白
                }
            }

            // TODO: 改造为使用StringBuilder
            string strResult = "";
            for (int i = 0; i < items1.Length; i++)
            {
                for (int j = 0; j < items2.Length; j++)
                {
                    if (String.Compare(items1[i], items2[j], bIgnoreCase) == 0)
                        goto FOUND;
                }

                if (strResult != "")
                    strResult += ",";
                strResult += items1[i];
                continue;
            FOUND:
                continue;
            }

            for (int i = 0; i < items2.Length; i++)
            {
                if (strResult != "")
                    strResult += ",";
                strResult += items2[i];
            }

            return strResult;
        }

        // 对两个已经排序的List进行逻辑运算
        // 注：sourceLeft和sourceRight在调用前应当已经排序，从小到大的方向
        // parameters:
        //		strLogicOper	运算风格 OR , AND , SUB
        //		sourceLeft	源左边结果集
        //		sourceRight	源右边结果集
        //		targetLeft	目标左边结果集
        //		targetMiddle	目标中间结果集
        //		targetRight	目标右边结果集
        //		bOutputDebugInfo	是否输出处理信息
        //		strDebugInfo	处理信息
        // return
        //		-1	出错
        //		0	成功
        public static int LogicOper(string strLogicOper,
            List<string> sourceLeft,
            List<string> sourceRight,
            ref List<string> targetLeft,
            ref List<string> targetMiddle,
            ref List<string> targetRight,
            bool bOutputDebugInfo,
            out string strDebugInfo,
            out string strError)
        {
            strDebugInfo = "";
            strError = "";

            DateTime start_time = DateTime.Now;

            strLogicOper = strLogicOper.ToUpper();

            /*
            if (bOutputDebugInfo == true)
            {
                strDebugInfo += "strLogicOper值:" + strLogicOper + "<br/>";
                strDebugInfo += "sourceLeft结果集:" + sourceLeft.Dump() + "<br/>";
                strDebugInfo += "sourceRight结果集:" + sourceRight.Dump() + "<br/>";
            }
             * */

            if (strLogicOper == "OR")
            {
                // OR操作不应使用targetLeft和targetRight参数
                if (targetLeft != null || targetRight != null)
                {
                    Exception ex = new Exception("StringUtil::LogicOper()中是不是参数用错了?当strLogicOper参数值为\"OR\"时，targetLeft参数和targetRight无效，值应为null");
                    throw (ex);
                }
            }

            if (strLogicOper == "SUB")
            {
                // SUB操作不应使用targetMiddle和targetRight参数
                if (targetMiddle != null || targetRight != null)
                {
                    Exception ex = new Exception("StringUtil::LogicOper()中是不是参数用错了?当strLogicOper参数值为\"SUB\"时，targetMiddle参数和targetRight无效，值应为null");
                    throw (ex);
                }
            }

            string left = null;
            string right = null;

            string old_left = null;
            string old_right = null;
            int old_ret = 0;

            int i = 0;
            int j = 0;
            int ret = 0;
            while (true)
            {
                old_left = left;
                old_right = right;
                old_ret = ret;

                // 准备left right
                left = null;
                right = null;
                if (i >= sourceLeft.Count)
                {
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "i大于等于sourceLeft的个数，将i改为-1<br/>";
                    }
                    i = -1;
                }
                else if (i != -1)
                {
                    try
                    {
                        left = sourceLeft[i];
                        if (bOutputDebugInfo == true)
                        {
                            strDebugInfo += "取出sourceLeft集合中第" + Convert.ToString(i) + "个元素，ID为" + left + "<br/>";
                        }
                    }
                    catch (Exception e)
                    {
                        Exception ex = new Exception("取sourceLeft集合出错：i=" + Convert.ToString(i) + "----Count=" + Convert.ToString(sourceLeft.Count) + ", internel error :" + e.Message + "<br/>");
                        throw (ex);
                    }
                }
                if (j >= sourceRight.Count)
                {
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "j大于等于sourceRight的个数，将j改为-1<br/>";
                    }
                    j = -1;
                }
                else if (j != -1)
                {
                    try
                    {
                        right = sourceRight[j];
                        if (bOutputDebugInfo == true)
                        {
                            strDebugInfo += "取出sourceRight集合中第" + Convert.ToString(j) + "个元素，ID为" + right + "<br/>";
                        }
                    }
                    catch
                    {
                        Exception ex = new Exception("j=" + Convert.ToString(j) + "----Count=" + Convert.ToString(sourceLeft.Count) + sourceRight.GetHashCode() + "<br/>");
                        throw (ex);
                    }
                }


                if (i == -1 && j == -1)
                {
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "i,j都等于-1跳出<br/>";
                    }
                    break;
                }

                if (left == null)
                {
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "left为null，设ret等于1<br/>";
                    }
                    ret = 1;
                }
                else if (right == null)
                {
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "right为null，设ret等于-1<br/>";
                    }
                    ret = -1;
                }
                else
                {
                    ret = left.CompareTo(right);  //MyCompareTo(oldOneKey); //改CompareTO
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "left与right均不为null，比较两条记录得到ret等于" + Convert.ToString(ret) + "<br/>";
                    }
                }



                if (strLogicOper == "OR" && targetMiddle != null)
                {
                    if (ret == 0)
                    {
                        // id值完全相同的时候，输出左边
                        targetMiddle.Add(left);

                        i++;
                        j++;
                    }
                    else if (ret < 0)
                    {
                        targetMiddle.Add(left);
                        i++;
                    }
                    else if (ret > 0)
                    {
                        targetMiddle.Add(right);
                        j++;
                    }
                    continue;
                }

                if (ret == 0)
                {
                    if (targetMiddle != null)
                    {
                        if (bOutputDebugInfo == true)
                        {
                            strDebugInfo += "ret等于0,加到targetMiddle里面<br/>";
                        }

                        if (strLogicOper != "AND")
                            targetMiddle.Add(left);
                        else
                        {
                            Debug.Assert(strLogicOper == "AND", "");

                            // id值完全相同的时候，输出左边
                            targetMiddle.Add(left);
                        }

                    }   // endof if (targetMiddle != null)

                    i++;
                    j++;
                }

                if (ret < 0)
                {
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "ret小于0,加到targetLeft里面<br/>";
                    }

                    if (targetLeft != null && left != null)
                        targetLeft.Add(left);
                    i++;
                }

                if (ret > 0)
                {
                    if (bOutputDebugInfo == true)
                    {
                        strDebugInfo += "ret大于0,加到targetRight里面<br/>";
                    }

                    if (targetRight != null && right != null)
                        targetRight.Add(right);

                    j++;
                }
            }

            TimeSpan delta = DateTime.Now - start_time;
            Debug.WriteLine("Merge() " + strLogicOper + " 耗时 " + delta.ToString());

            return 0;
        }

        // 在列举值中增加或清除一个值
        // parameters:
        //      strSub  里面可以包含多个值
        public static void SetInList(ref string strList,
            string strSub,
            bool bOn)
        {
            if (bOn == false)
            {
                RemoveFromInList(strSub,
                    true,
                    ref strList);
            }
            else
            {
                // 单个值的情况
                if (strSub.IndexOf(',') == -1)
                {
                    if (IsInList(strSub, strList) == true)
                        return;	// 已经有了

                    // 在尾部新增加
                    if (string.IsNullOrEmpty(strList) == false)
                        strList += ",";

                    strList += strSub;
                    return;
                }

                // 2012/2/2
                // 多个值的情况
                string[] sub_parts = strSub.Split(new char[] { ',' });
                foreach (string sub in sub_parts)
                {
                    if (sub == null)
                        continue;

                    string strOne = sub.Trim();
                    if (string.IsNullOrEmpty(strOne) == true)
                        continue;

                    if (IsInList(strOne, strList) == true)
                        continue;	// 已经有了

                    // 在尾部新增加
                    if (string.IsNullOrEmpty(strList) == false)
                        strList += ",";

                    strList += strOne;
                }
            }
        }


        // 从逗号间隔的list中去除一个特定的style值。大小写不敏感
        // parameters:
        //      strSub  要去除的值列表。字符串中可以包含多个值。
        //      bRemoveMultiple	是否具有去除多个相同strSub值的能力。==false，只去除发现的第一个
        public static bool RemoveFromInList(string strSub,
            bool bRemoveMultiple,
            ref string strList)
        {
            string[] sub_parts = strSub.Split(new char[] { ',' });

            string[] list_parts = strList.Split(new char[] { ',' });

            bool bChanged = false;
            foreach (string temp in sub_parts)
            {
                string sub = temp.Trim();
                if (string.IsNullOrEmpty(sub) == true)
                    continue;

                for (int j = 0; j < list_parts.Length; j++)
                {
                    string list = list_parts[j];
                    if (list == null)
                        continue;

                    list = list.Trim();
                    if (string.IsNullOrEmpty(list) == true)
                        continue;

                    if (String.Compare(sub, list, true) == 0)
                    {
                        bChanged = true;
                        list_parts[j] = null;
                        if (bRemoveMultiple == false)
                            break;
                    }
                }
            }

            StringBuilder result = new StringBuilder(4096);
            foreach (string list in list_parts)
            {
                if (string.IsNullOrEmpty(list) == false)
                {
                    if (result.Length > 0)
                        result.Append(",");
                    result.Append(list);
                }
            }

            strList = result.ToString();

            return bChanged;
        }

        /// <summary>
        /// 查找一个小字符串是否包含在一个字符串数组中
        /// </summary>
        /// <param name="strSub">小字符串</param>
        /// <param name="aList">字符串数组</param>
        /// <returns>
        /// 1:包含
        /// 0:不包含
        /// </returns>
        public static bool IsInAlist(string strSub,
            string[] aList)
        {
            /*
            for(int i=0;i<aList.Length;i++)
            {
                if (String.Compare(strSub,aList[i],true) == 0) 
                {
                    return true;
                }
            }
            return false;
            */
            return IsInAlist(strSub,
                aList,
                true);
        }

        // parameters:
        //      strSub      要比较的单个值。可以包含多个单独的值，用逗号连接。注：如果是多个值，则只要有一个匹配上，就返回true
        //		bIgnoreCase	是否忽略大小写
        public static bool IsInAlist(string strSub,
            string[] aList,
            bool bIgnoreCase)
        {
            // 2015/5/27
            if (string.IsNullOrEmpty(strSub) == true)
                return false;

            string[] sub_parts = strSub.Split(new char[] { ',' });

            // 2012/2/2 增加了处理strSub中包含多个值的能力
            foreach (string sub in sub_parts)
            {
                if (sub == null)
                    continue;

                string strOne = sub.Trim();
                if (string.IsNullOrEmpty(strOne) == true)
                    continue;

                for (int i = 0; i < aList.Length; i++)
                {
                    if (String.Compare(strOne, aList[i], bIgnoreCase) == 0)
                        return true;
                }
            }
            return false;
        }


        // 检索一个字符串是否是数字格式
        // '.'也算合法
        // 编写者：任延华
        public static bool IsNum(string strText)
        {
            string strPattern = @"^[.]*\d*[.]*\d*$";
            return RegexCompare(strPattern, strText);
        }

        // 检索一个字符串是否是数字格式,不包含'.'
        // 编写者：任延华
        public static bool IsDigital(string strText)
        {
            foreach (char oneChar in strText)
            {
                if (StringUtil.IsDigital(oneChar) == false)
                    return false;
            }
            return true;
        }
        // 检索一个字符串是否是数字格式
        // 编写者：任延华
        public static bool IsLetterOrDigit(string strText)
        {
            foreach (char oneChar in strText)
            {
                if (Char.IsLetterOrDigit(oneChar) == false)
                    return false;
            }
            return true;
        }

        // 得到xml中适用的字符串，这是替换了xml敏感符号为实体的字符串
        public static string GetXmlString(string strText)
        {
            using (TextWriter textWrite = new StringWriter())
            using (XmlTextWriter xmlTextWriter = new XmlTextWriter(textWrite))
            {
                xmlTextWriter.WriteString(strText);
                return textWrite.ToString();
            }
        }

        public static string GetVisualableStringSimple(string strText)
        {
            /*
			strText = strText.Replace("&amp;","&");
			strText = strText.Replace("&apos;","'");
			strText = strText.Replace("&quot;","\"");
			strText = strText.Replace("&gt;",">");
			strText = strText.Replace("&lt;","<");

			return strText;
             * */

            // 2006/8/30 改造
            string strResult = "";
            bool bIn = false;
            string strPart = "";
            for (int i = 0; i < strText.Length; i++)
            {
                char ch = strText[i];
                if (ch == '&')
                {
                    bIn = true;
                    continue;
                }
                else if (ch == ';')
                {
                    bIn = false;

                    if (strPart == "") // 单独出现的一个';'
                    {
                        strResult += ch;
                        continue;
                    }


                    // 处理strPart
                    if (strPart == "amp")
                        ch = '&';
                    else if (strPart == "apos")
                        ch = '\'';
                    else if (strPart == "quot")
                        ch = '\"';
                    else if (strPart == "gt")
                        ch = '>';
                    else if (strPart == "lt")
                        ch = '<';
                    else if (strPart.Length > 1 && strPart[0] == '#')
                    {
                        string strNumber = strPart.Substring(1);
                        // 数字
                        int nRet = strNumber.IndexOf("x");
                        if (nRet == -1)
                        {
                            // 十进制
                            int nNumber = 0;
                            try
                            {
                                nNumber = Convert.ToInt32(strNumber);
                            }
                            catch // (Exception ex)
                            {
                                strResult += "&" + strPart + ";";
                                strPart = "";
                                continue;
                            }
                        }

                        strNumber = strNumber.Substring(nRet + 1);
                        ch = (char)Convert.ToInt16(strNumber, 16);
                    }
                    else
                    {
                        strResult += "&" + strPart + ";";
                        strPart = "";
                        continue;
                    }

                    strResult += ch;
                    strPart = "";
                    continue;
                }

                if (bIn == true)
                {
                    strPart += ch;
                    continue;
                }
                // CONTINUE:
                strResult += ch;
            }

            if (bIn == true)
                strResult += new string('&', 1) + strPart;   // 如果有残余的

            return strResult;
        }

        public static string GetXmlStringSimple(string strText)
        {

            // 2010/10/2 add
            if (String.IsNullOrEmpty(strText) == true)
                return "";

            /*
             * 
             * XmlTextWriter.WriteString()注释
             * WriteString 执行以下操作 
分别用 &amp;、&lt; 和 &gt; 替换字符 &、< 和 >。
用数字字符实体（&#0; 到 &#0x1F）替换范围 0x-0x1F（不包括空白字符 0x9、0x10 和 0x13）中的字符值。
如果在属性值的上下文中调用 WriteString，则分别用 &quot; 和 &apos; 替换双引号和单引号。
例如，此输入字符串 test<item>test 被写为下面的形式 
 test&lt;item&gt;test
 
             * 因此，本函数目前的做法，对于不是属性的上下文的时候，有点“过激”，多处理了两个字符
             * 
             */


            // 2006/8/30 改造
            // 处理 &#x1F; 情况
            string strResult = "";

            for (int i = 0; i < strText.Length; i++)
            {
                char ch = strText[i];

                if (ch >= 0 && ch <= 0x1f && ch != 0x9 && ch != 0x10 && ch != 0x13)
                {
                    strResult += "&#x" + Convert.ToString(ch, 16).PadLeft(2, '0') + ";";
                }
                else if (ch == '&')
                {
                    strResult += "&amp;";
                }
                else if (ch == '\'')
                {
                    strResult += "&apos;";
                }
                else if (ch == '\"')
                {
                    strResult += "&quot;";
                }
                else if (ch == '>')
                {
                    strResult += "&gt;";
                }
                else if (ch == '<')
                {
                    strResult += "&lt;";
                }
                else
                {
                    strResult += ch;
                }

            }

            return strResult;

            /*

			strText = strText.Replace("&","&amp;");
			strText = strText.Replace("'","&apos;");
			strText = strText.Replace("\"","&quot;");
			strText = strText.Replace(">","&gt;");
			strText = strText.Replace("<","&lt;");
			return strText;
             * */

        }
        /*
                public static string GetAttrValueString(string strText)
                {
                    strText = strText.Replace("'","&apos;");
                    strText = strText.Replace("\"","&quot;");
                    return strText;
                }

                public static string GetTextValueString(string strText)
                {
                    strText = strText.Replace(">","&gt;");
                    strText = strText.Replace("<","&lt;");
                    return strText;
                }
        */

        // 将范围字符串,如"00000001~000000100"字符串拆分为两部分。如果没有字符~,则找字符-
        // parameter:
        //		strRange: 输入的范围字符串
        //		strStart: 返回开始
        //		strEnd:   返回结束
        // return:
        //      false   不是范围表达式
        //      true    是范围表达式
        public static bool SplitRangeEx(string strRange,
            out string strStart,
            out string strEnd)
        {
            strStart = "";
            strEnd = "";

            int nPosition = strRange.IndexOf("~");
            if (nPosition == -1)
                nPosition = strRange.IndexOf("-");

            if (nPosition == -1)
                return false;

            if (nPosition > 0)
            {
                strStart = strRange.Substring(0, nPosition).Trim();
                strEnd = strRange.Substring(nPosition + 1).Trim();
                if (string.IsNullOrEmpty(strEnd) == true)
                    strEnd = "9999999999";
            }
            if (nPosition == 0)
            {
                strStart = "0";
                strEnd = strRange.Substring(1).Trim();
                if (string.IsNullOrEmpty(strEnd) == true)
                    strEnd = "9999999999";
            }
            if (nPosition < 0)
            {
                strStart = strRange.Trim();
                strEnd = strStart;
            }

            return true;
        }

#if NO
		// 将范围字符串,如"00000001-000000100"字符串拆分为两部分
		// parameter:
		//		strRange: 输入的范围字符串
		//		strStart: 返回开始
		//		strEnd:   返回结束
		public static void SplitRange(string strRange,
			out string strStart,
			out string strEnd)
		{
			strStart = "";
			strEnd = "";

			int nPosition = strRange.IndexOf("-");
			if (nPosition > 0)
			{
				strStart = strRange.Substring(0,nPosition).Trim();
				strEnd = strRange.Substring(nPosition+1).Trim();
				if (strEnd == "")
					strEnd = "9999999999";
			}
			if (nPosition == 0)
			{
				strStart = "0";
				strEnd = strRange.Substring(1).Trim();
			}
			if (nPosition < 0)
			{
				strStart = strRange.Trim();
				strEnd = strStart;
			}
		}
#endif


        // 将带运算符的式子分成两部分
        // parameter:
        //		strText: 传来的字符串
        //		strOperator: 操作符
        //		strRealText: 真正的字符串
        // 编写者: 任延华
        public static int GetPartCondition(string strText,
            out string strOperator,
            out string strRealText)
        {
            strText = strText.Trim();

            strOperator = "=";
            strRealText = strText;

            int nPosition;

            nPosition = strText.IndexOf(">=");
            if (nPosition >= 0)
            {
                strRealText = strText.Substring(nPosition + 2);
                strOperator = ">=";
                return 0;
            }
            nPosition = strText.IndexOf("<=");
            if (nPosition >= 0)
            {
                strRealText = strText.Substring(nPosition + 2);
                strOperator = "<=";
                return 0;
            }
            nPosition = strText.IndexOf("<>");
            if (nPosition >= 0)
            {
                strRealText = strText.Substring(nPosition + 2);
                strOperator = "<>";
                return 0;
            }
            nPosition = strText.IndexOf("><");
            if (nPosition >= 0)
            {
                strRealText = strText.Substring(nPosition + 2);
                strOperator = "<>";
                return 0;
            }
            nPosition = strText.IndexOf("!=");
            if (nPosition >= 0)
            {
                strRealText = strText.Substring(nPosition + 2);
                strOperator = "<>";
                return 0;
            }
            nPosition = strText.IndexOf(">");
            int nPosition2 = strText.IndexOf(">=");
            if (nPosition2 < 0 && nPosition >= 0)
            {
                strRealText = strText.Substring(nPosition + 1);
                strOperator = ">";
                return 0;
            }
            nPosition = strText.IndexOf("<");
            nPosition2 = strText.IndexOf("<=");
            if (nPosition2 < 0 && nPosition >= 0)
            {
                strRealText = strText.Substring(nPosition + 1);
                strOperator = "<";
                return 0;
            }
            return 0;
        }


        // 从指定起始位置查找子字符串在大字符串出现的位置
        // parameter:
        //		strBigString: 大字符串
        //		strSmallString: 小字符串
        //		nStart: 起始位置
        // return:
        //		-1: 没找到
        //		>=0: 出现的位置
        // 编写者: 任延华
        public static int FindSubstring(string strBigString,
            string strSmallString,
            int nStart)
        {
            for (int i = nStart; i < strBigString.Length; i++)
            {
                if (String.Compare(strBigString,
                    i,
                    strSmallString,
                    0,
                    strSmallString.Length,
                    true) == 0)
                {
                    return i;
                }
            }
            return -1;
        }


        // 检查字符串是否是汉字
        // strWord: 传入的字符串
        // return:
        //		true:字符串每个字符均是汉字
        //		false: 出现非汉字
        // 编写者: 任延华
        public static bool IsChineseChar(string strWord)
        {
            foreach (Char oneChar in strWord)
            {
                if (Char.GetUnicodeCategory(oneChar) != UnicodeCategory.OtherLetter)
                    return false;
            }

            return true;
        }

        public static bool IsDigital(char ch)
        {
            char[] chs = new char[10];
            chs[0] = '0';
            chs[1] = '1';
            chs[2] = '2';
            chs[3] = '3';
            chs[4] = '4';
            chs[5] = '5';
            chs[6] = '6';
            chs[7] = '7';
            chs[8] = '8';
            chs[9] = '9';

            for (int i = 0; i < chs.Length; i++)
            {
                if (ch == chs[i])
                    return true;
            }

            return false;

        }

        // 过滤字符串的非数字字符,变成数字型字符串,包含"."
        // parameter:
        //		strText: 传进的字符串
        // return:
        //		数字型字符串
        // 编写者: 任延华
        public static string GetStringNumber(string strInputText)
        {
            ArrayList aDigitalList = new ArrayList();
            if (StringUtil.IsNum(strInputText) == true)
                return strInputText;
            for (int i = 0; i < strInputText.Length; i++)
            {
                char oneChar = strInputText[i];

                if ((StringUtil.IsDigital(oneChar) == true))
                    aDigitalList.Add(oneChar);
                else if (oneChar == '.')
                    aDigitalList.Add(oneChar);
                else
                    continue;
            }
            char[] achar = new char[aDigitalList.Count];
            for (int i = 0; i < aDigitalList.Count; i++)
            {
                achar[i] = (char)aDigitalList[i];
            }
            string strResult = new string(achar);
            if (strResult == "")
                strResult = "-1";
            return strResult;
        }

        // 把一数字型字符串变成整数型字符串，并根据精度进行扩展
        // strKey: 传入的字符串
        // strPrecision: 精度
        // 返回值: 根据精度加工的字符串，出现返回“-1”
        // 编写者: 任延华
        public static string ExtendByPrecision(string strKey,
            string strPrecision)
        {
            strKey = GetStringNumber(strKey);
            if (strKey == "-1")
                return "-1";


            int nPrecision;
            if (strPrecision == "0")
                nPrecision = 0;
            else
                nPrecision = Convert.ToInt32(strPrecision);


            string strResult = "";
            string str1;
            string str2;

            int nPosition;
            nPosition = strKey.IndexOf(".");
            if (nPosition >= 0)
            {
                str1 = strKey.Substring(0, nPosition);
                str2 = strKey.Substring(nPosition + 1);
                str2 = str2.Replace(".", "");

                if (str2.Length < nPrecision)
                    str2 += new String('0', nPrecision - str2.Length);

                if (str2.Length > nPrecision && nPrecision > 0)
                    str2 = str2.Substring(0, nPrecision - 1);

                strResult = str1 + str2;
                if (nPrecision == 0)
                    strResult = str1;
            }
            else
            {
                strResult = strKey + new String('0', nPrecision);
            }
            //加去0的函数
            strResult = Delete0FromString(strResult);
            if (strResult == "")
                strResult = "0";
            return strResult;
        }


        // 删除字符串前方的“0”
        // parameter:
        //		strInputText: 输入的字符串
        // return:
        //		去前方0后的字符串
        // 编写者: 任延华
        public static string Delete0FromString(string strText)
        {
            if (strText == "")
                return strText;

            while (strText.Length > 0 && strText.Substring(0, 1) == "0")
            {
                strText = strText.Substring(1);
            }
            return strText;
        }

        // 根据操作符比较两个字符串是否符合 strText strOperator strCanKaoText
        // parameter:
        //		strText: 当前字符串
        //		strOperator: 操作符
        //		strCanKaoText: 被比较的字符串
        // return:
        //		true: 符合条件
        //		false:不满足条件
        public static bool CompareByOperator(string strText,
            string strOperator,
            string strCanKaoText)
        {

            if (strOperator == "=")
            {
                if (String.Compare(strText, strCanKaoText) == 0)
                    return true;
            }
            if (strOperator == ">=")
            {
                if (String.Compare(strText, strCanKaoText) >= 0)
                    return true;
            }
            if (strOperator == "<=")
            {
                if (String.Compare(strText, strCanKaoText) <= 0)
                    return true;
            }
            if (strOperator == "<>" || strOperator == "><" || strOperator == "!=")
            {
                if (String.Compare(strText, strCanKaoText) != 0)
                    return true;
            }
            if (strOperator == ">")
            {
                if (String.Compare(strText, strCanKaoText) > 0)
                    return true;
            }
            if (strOperator == "<")
            {
                if (String.Compare(strText, strCanKaoText) < 0)
                    return true;
            }
            return false;
        }

    }

}
