using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Diagnostics;

using DigitalPlatform.Xml;
using DigitalPlatform.Text;

namespace DigitalPlatform.Script
{
    /// <summary>
    /// ISBN号分析器，帮助插入'-'
    /// </summary>
    public class IsbnSplitter
    {
        XmlDocument dom = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="strIsbnFileName">ISBN 定义文件。XML 格式</param>
        public IsbnSplitter(string strIsbnFileName)
        {
            dom = new XmlDocument();
            dom.Load(strIsbnFileName);
        }

        static bool InRange(string strValue,
            string strStart,
            string strEnd)
        {
            if (String.Compare(strValue, strStart) < 0)
                return false;
            if (String.Compare(strValue, strEnd) > 0)
                return false;

            return true;
        }

        static bool IsNumber(string strText)
        {
            for (int i = 0; i < strText.Length; i++)
            {
                if (strText[0] < '0' || strText[0] > '9')
                    return false;
            }

            return true;
        }


        /// <summary>
        ///  校验 ISBN 第一部分是否正确
        /// </summary>
        /// <param name="strFirstPart">ISBN 的第一部分</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 错误; 0: 正确</returns>
        public static int VerifyIsbnFirstPart(string strFirstPart,
                        out string strError)
        {
            strError = "";

            if (IsNumber(strFirstPart) == false)
            {
                strError = "ISBN第一部分应当为纯数字";
                goto WRONG;
            }


            if (String.IsNullOrEmpty(strFirstPart) == true)
            {
                strError = "ISBN第一部分字符数不能为0";
                goto WRONG;
            }
            if (strFirstPart.Length == 1)
            {
                if (InRange(strFirstPart, "0", "7") == true)
                    goto CORRECT;
                else
                {
                    strError = "如果ISBN第一部分('" + strFirstPart + "')为1字符，其取值范围应当为 0-7";
                    goto WRONG;
                }
            }
            else if (strFirstPart.Length == 2)
            {
                if (InRange(strFirstPart, "80", "94") == true)
                    goto CORRECT;
                else
                {
                    strError = "如果ISBN第一部分('"
                        + strFirstPart + "')为2字符，其取值范围应当为 80-94";
                    goto WRONG;
                }
            }

            else if (strFirstPart.Length == 3)
            {
                if (InRange(strFirstPart, "950", "994") == true)
                    goto CORRECT;
                else
                {
                    strError = "如果ISBN第一部分('" + strFirstPart + "')为3字符，其取值范围应当为 950-994";
                    goto WRONG;
                }
            }

            else if (strFirstPart.Length == 4)
            {
                if (InRange(strFirstPart, "9950", "9989") == true)
                    goto CORRECT;
                else
                {
                    strError = "如果ISBN第一部分('" + strFirstPart + "')为4字符，其取值范围应当为 9950-9989";
                    goto WRONG;
                }
            }

            else if (strFirstPart.Length == 5)
            {
                if (InRange(strFirstPart, "99900", "99999") == true)
                    goto CORRECT;
                else
                {
                    strError = "如果ISBN第一部分('" + strFirstPart + "')为5字符，其取值范围应当为 99900-99999";
                    goto WRONG;
                }
            }

            strError = "ISBN第一部分字符数不能超过5";
            WRONG:
            return -1;
            CORRECT:
            return 0;
        }

        // 2016/12/15 尚未测试
        // 是否为 10 位 ISBN?
        public static bool IsISBN10(string strSource)
        {
            if (string.IsNullOrEmpty(strSource) == true)
                return false;
            strSource = strSource.Replace("-", "").Trim();
            if (string.IsNullOrEmpty(strSource) == true)
                return false;

            // 10位、无-
            if (strSource.Length == 10
                && strSource.IndexOf("-") == -1)
            {
                try
                {
                    char c = GetIsbn10VerifyChar(strSource);
                    if (c != strSource[9])
                        return false;
                }
                catch (ArgumentException ex)
                {
                    return false;
                }

                return true;
            }

            return false;
        }

        // 校验 ISBN 字符串
        // 注：返回 -1 和 返回 1 的区别：-1 表示调用过程出错，暗示对这样的 ISBN 字符串应当预先检查，若不符合基本形式要求则避免调用本函数
        // return:
        //      -1  出错
        //      0   校验正确
        //      1   校验不正确。提示信息在strError中
        public static int VerifyISBN(string strISBNParam,
            out string strError)
        {
            strError = "";

            if (string.IsNullOrEmpty(strISBNParam) == true)
            {
                strError = "ISBN字符串内容为空";
                return -1;
            }

            // 2015/9/7
            string strISBN = strISBNParam.Trim();
            if (string.IsNullOrEmpty(strISBN) == true)
            {
                strError = "ISBN字符串内容为空(1)";
                return -1;
            }

            strISBN = strISBNParam.Replace("-", "").Replace(" ", "");
            if (string.IsNullOrEmpty(strISBN) == true)
            {
                strError = "ISBN字符串内容为空";
                return 1;
            }

            if (strISBN.Length != 10 && strISBN.Length != 13)
            {
                strError = "(除字符'-'和空格外)ISBN字符串的长度既不是10位也不是13位";
                return 1;
            }

            // 检查前面 length - 1 位必须为数字。最后一位必须是数字或者 X
            if (VerifyChars(strISBN) == false)
            {
                strError = $"ISBN '{strISBN}' 校验不正确。出现了非法字符";
                return 1;
            }

            if (strISBN.Length == 10)
            {
                try
                {
                    char c = GetIsbn10VerifyChar(strISBN);
                    if (c != strISBN[9])
                    {
                        strError = "ISBN '" + strISBN + "' 校验不正确";
                        return 1;
                    }
                }
                catch (ArgumentException ex)
                {
                    strError = "ISBN '" + strISBN + "' 校验不正确: " + ex.Message;
                    return 1;
                }
            }

            if (strISBN.Length == 13)
            {
                //
                char c = GetIsbn13VerifyChar(strISBN);
                if (c != strISBN[12])
                {
                    strError = "ISBN '" + strISBN + "' 校验不正确";
                    return 1;
                }
            }

            return 0;
        }

        static bool VerifyChars(string text)
        {
            if (string.IsNullOrEmpty(text))
                return true;
            for (int i = 0; i < text.Length; i++)
            {
                char ch = text[i];

                if (ch >= '0' && ch <= '9')
                {
                }
                else
                {
                    // 最后一个 char
                    if (i == text.Length - 1)
                    {
                        if (char.ToLower(ch) != 'x')
                            return false;
                        continue;
                    }
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 计算出 ISSN-8 校验位
        /// </summary>
        /// <param name="strISSN">ISSN 字符串.8 字符</param>
        /// <returns>校验位字符</returns>
        public static char GetIssn8VerifyChar(string strISSN)
        {
            strISSN = strISSN.Trim();
            strISSN = strISSN.Replace("-", "");
            strISSN = strISSN.Replace(" ", "");

            if (strISSN.Length < 7)
                throw new ArgumentException("用于计算校验位的ISSN-8长度至少要在7位数字以上(不包括横杠在内)");

            int sum = 0;
            for (int i = 0; i < 7; i++)
            {
                sum += (strISSN[i] - '0') * (8 - i);
            }
            int v = 11 - (sum % 11);

            if (v == 10)
                return 'X';

            return (char)('0' + v);
        }


        /// <summary>
        /// 计算出 ISBN-10 校验位
        /// </summary>
        /// <param name="strISBN">ISBN 字符串</param>
        /// <returns>校验位字符</returns>
        public static char GetIsbn10VerifyChar(string strISBN)
        {
            strISBN = strISBN.Trim();
            strISBN = strISBN.Replace("-", "");
            strISBN = strISBN.Replace(" ", "");

            if (strISBN.Length < 9)
                throw new ArgumentException("用于计算校验位的ISBN-10长度至少要在9位数字以上(不包括横杠在内)");

            int sum = 0;
            for (int i = 0; i < 9; i++)
            {
                sum += (strISBN[i] - '0') * (i + 1);
            }
            int v = sum % 11;

            if (v == 10)
                return 'X';

            return (char)('0' + v);
        }

        /// <summary>
        /// 计算出 ISBN-13 校验位
        /// </summary>
        /// <param name="strISBN">ISBN 字符串</param>
        /// <returns>校验位字符</returns>
        public static char GetIsbn13VerifyChar(string strISBN)
        {
            strISBN = strISBN.Trim();
            strISBN = strISBN.Replace("-", "");
            strISBN = strISBN.Replace(" ", "");


            if (strISBN.Length < 12)
                throw new Exception("用于计算校验位的ISBN-13长度至少要在12位数字以上(不包括横杠在内)");

            int m = 0;
            int sum = 0;
            for (int i = 0; i < 12; i++)
            {
                if ((i % 2) == 0)
                    m = 1;
                else
                    m = 3;

                sum += (strISBN[i] - '0') * m;
            }

            // 注：如果步骤5所得余数为0，则校验码为0。
            if ((sum % 10) == 0)
                return '0';

            int v = 10 - (sum % 10);

            return (char)('0' + v);
        }


        /// <summary>
        /// 在 ISBN 字符串中适当的位置插入'-'符号
        /// 如果提供的ISBN字符串本来就有978前缀，那么结果仍将保留前缀。如果本来就没有，结果里面也没有。
        /// </summary>
        /// <param name="strISBN">ISBN 字符串</param>
        /// <param name="strStyle">处理风格。force10/force13/auto/remainverifychar/strict</param>
        /// <param name="strTarget">返回处理结果</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1:出错; 0:未修改校验位; 1:修改了校验位</returns>
        public int IsbnInsertHyphen(
            string strISBN,
            string strStyle,
            out string strTarget,
            out string strError)
        {
            strTarget = "";
            strError = "";

            string strSource;
            // int nFirstLen = 0;
            int nSecondLen;

            // Debug.Assert(false, "");

            strSource = strISBN;
            strSource = strSource.Trim();

            bool bHasRemovePrefix978 = false; // 是否有978前缀

            bool bForce10 = StringUtil.IsInList("force10", strStyle);
            bool bForce13 = StringUtil.IsInList("force13", strStyle);
            bool bAuto = StringUtil.IsInList("auto", strStyle);
            bool bRemainVerifyChar = StringUtil.IsInList("remainverifychar", strStyle); // 是否不要重新计算校验位
            bool bStrict = StringUtil.IsInList("strict", strStyle); // 是否严格要求strISBN输入参数为10或13位

            int nCount = 0;
            if (bForce10 == true)
                nCount++;
            if (bForce13 == true)
                nCount++;
            if (bAuto == true)
                nCount++;

            if (nCount > 1)
            {
                strError = "strStyle值 '" + strStyle + "' 中的force10/force13/auto 3种风格是互相排斥，不能同时具备。";
                return -1;
            }

            strSource = strSource.Replace("-", "");
            strSource = strSource.Replace(" ", "");

            bool bAdjustLength = false; // 是否调整过输入的strISBN的长度

            if (bStrict == false)
            {
                if (strSource.Length == 9)
                {
                    strSource += '0';
                    bRemainVerifyChar = false;  // 必须要重新计算校验位了
                    bAdjustLength = true;
                }
                else if (strSource.Length == 12)
                {
                    strSource += '0';
                    bRemainVerifyChar = false;  // 必须要重新计算校验位了
                    bAdjustLength = true;
                }
            }

            string strPrefix = "978";

            // 13位、无-、前缀为978
            if (strSource.Length == 13
                && strSource.IndexOf("-") == -1
                && (strSource.Substring(0, 3) == "978" || strSource.Substring(0, 3) == "979")
                )
            {
                if (strSource.Length >= 3)
                    strPrefix = strSource.Substring(0, 3);

                strSource = strSource.Substring(3, 10); // 丢弃前3位，但不丢弃校验位

                bHasRemovePrefix978 = true;
            }

            if (strSource.Length != 10
                && strSource.Length != 13)
            {
                strError = "ISBN中(除'-'以外)应为10位或13位有效字符(" + strSource + " " + Convert.ToString(strSource.Length) + ")";
                return -1;
            }

            if (bForce10 && strPrefix == "979")
            {
                strError = "979 前缀的 ISBN 不能变为 ISBN-10 形态";
                return -1;
            }

            string strFirstPart = "";   // 第一部分

            XmlElement hit_prefix = null;

            XmlNodeList prefix_nodes = dom.DocumentElement.SelectNodes("RegistrationGroups/Group/Prefix");
            if (prefix_nodes.Count == 0)
            {
                strError = "ISBN 规则文件格式有误，无法选择到任何 RegistrationGroups/Group/Prefix 元素";
                return -1;
            }
            string strTemp = strPrefix + strSource;
            foreach (XmlElement prefix in prefix_nodes)
            {
                string strCurrent = prefix.InnerText.Trim().Replace("-", "");
                if (strTemp.StartsWith(strCurrent) == true)
                {
                    hit_prefix = prefix;
                    strFirstPart = strCurrent.Substring(3);
                    // nFirstLen = strFirstPart.Length;
                    break;
                }
            }

            if (hit_prefix == null)
            {
                strError = "prefix 部分格式错误";    // 是否需要解释一下?
                return -1;
            }

            XmlNodeList nodes = hit_prefix.ParentNode.SelectNodes("Rules/Rule");
            if (nodes.Count == 0)
            {
                strError = "ISBN 数据中 没有找到 prefix='" + strFirstPart + "'的 Rules/Rule 元素 ...";
                return -1;
            }

            string strSecondPart = "";

            foreach (XmlElement node in nodes)
            {
                Range range = GetRangeValue(node);
                if (range == null)
                    continue;   // TODO: 需要报错

#if NO
                if (strLeft.Length != strRight.Length)
                {
                    strError = "数据节点 " + node.OuterXml + "格式错误, value值'" + strValue + "'中两个数字宽度不等。";
                    return -1;
                }
#endif

                int nWidth = range.Left.Length;

                if (nWidth == 0)
                    continue;   // 可能数据有错误? 

                if (nWidth != strSecondPart.Length)
                    strSecondPart = strSource.Substring(strFirstPart.Length, nWidth);

                if (InRange(strSecondPart, range.Left, range.Right) == true)
                {
                    nSecondLen = nWidth;
                    goto FINISH;
                }

            }

            strError = "第二部分格式错误 nFirstLen=[" + Convert.ToString(strFirstPart.Length) + "]";
            return -1;

            FINISH:
            strTarget = strSource;

            strTarget = strTarget.Insert(strFirstPart.Length, "-");
            strTarget = strTarget.Insert(strFirstPart.Length + nSecondLen + 1, "-");
            strTarget = strTarget.Insert(9 + 1 + 1, "-");

            if (bForce13 == true)
            {
                if (strTarget.Length == 13)
                    strTarget = strPrefix + "-" + strTarget;
            }
            else if (bAuto == true && bHasRemovePrefix978 == true)
            {
                strTarget = strPrefix + "-" + strTarget;
            }

            bool bVerifyChanged = false;

            // 重新计算校验码
            // 重新添加ISBN-10的校验位。因为条码号中ISBN-13校验位算法不同。
            if (bRemainVerifyChar == false)
            {
                if (strTarget.Length == 13)
                {
                    char old_ver = strTarget[12];
                    strTarget = strTarget.Substring(0, strTarget.Length - 1);
                    char v = GetIsbn10VerifyChar(strTarget);
                    strTarget += new string(v, 1);

                    if (old_ver != v)
                        bVerifyChanged = true;
                }
                else if (strTarget.Length == 17)
                {
                    char old_ver = strTarget[16];

                    strTarget = strTarget.Substring(0, strTarget.Length - 1);
                    char v = GetIsbn13VerifyChar(strTarget);
                    strTarget += new string(v, 1);

                    if (old_ver != v)
                        bVerifyChanged = true;
                }
            }

            if (bHasRemovePrefix978 == true
                && bForce10 == true)
                return 0;   // 移走978后，校验位肯定要发生变化。因此不通知这种变化

            if (bAdjustLength == false
                && bForce13 == true
                && strISBN.Trim().Replace("-", "").Length == 10)
                return 0;   // 加入了前缀后，校验位肯定要发生变化，因此不通知这种变化

            if (bVerifyChanged == true)
                return 1;

            return 0;
        }

        /// <summary>
        /// 在 ISSN 字符串中适当的位置插入'-'符号
        /// 如果提供的ISBN字符串本来就有978前缀，那么结果仍将保留前缀。如果本来就没有，结果里面也没有。
        /// </summary>
        /// <param name="strISSN">ISSN 字符串</param>
        /// <param name="strStyle">处理风格。force8/force13/auto/remainverifychar/strict</param>
        /// <param name="strTarget">返回处理结果</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1:出错; 0:未修改校验位; 1:修改了校验位</returns>
        public static int IssnInsertHyphen(
            string strISSN,
            string strStyle,
            out string strTarget,
            out string strError)
        {
            strTarget = "";
            strError = "";

            string strSource;
            int nSecondLen;

            // Debug.Assert(false, "");

            strSource = strISSN;
            strSource = strSource.Trim();

            bool bHasRemovePrefix977 = false; // 是否有977前缀

            bool bForce8 = StringUtil.IsInList("force8", strStyle);
            bool bForce13 = StringUtil.IsInList("force13", strStyle);
            bool bAuto = StringUtil.IsInList("auto", strStyle);
            bool bRemainVerifyChar = StringUtil.IsInList("remainverifychar", strStyle); // 是否不要重新计算校验位
            bool bStrict = StringUtil.IsInList("strict", strStyle); // 是否严格要求strISBN输入参数为10或13位

            int nCount = 0;
            if (bForce8 == true)
                nCount++;
            if (bForce13 == true)
                nCount++;
            if (bAuto == true)
                nCount++;

            if (nCount > 1)
            {
                strError = "strStyle值 '" + strStyle + "' 中的force8/force13/auto 3种风格是互相排斥，不能同时具备。";
                return -1;
            }

            strSource = strSource.Replace("-", "");
            strSource = strSource.Replace(" ", "");

            bool bAdjustLength = false; // 是否调整过输入的strISSN的长度

            if (bStrict == false)
            {
                if (strSource.Length == 7)
                {
                    strSource += '0';
                    bRemainVerifyChar = false;  // 必须要重新计算校验位了
                    bAdjustLength = true;
                }
                else if (strSource.Length == 12)
                {
                    strSource += '0';
                    bRemainVerifyChar = false;  // 必须要重新计算校验位了
                    bAdjustLength = true;
                }
            }

            string strPrefix = "977";

            // 13位、无-、前缀为977
            if (strSource.Length == 13
                && strSource.IndexOf("-") == -1
                && strSource.Substring(0, 3) == "977"
                )
            {
                if (strSource.Length >= 3)
                    strPrefix = strSource.Substring(0, 3);

                strSource = strSource.Substring(3, 10); // 丢弃前3位，但不丢弃校验位

                bHasRemovePrefix977 = true;
            }

            if (strSource.Length != 8
                && strSource.Length != 10
                && strSource.Length != 13)
            {
                strError = "ISSN中(除'-'以外)应为8位、10位或13位有效字符(" + strSource + " " + Convert.ToString(strSource.Length) + ")";
                return -1;
            }

            // 2017/9/19
            if (strSource.Length == 8)
                strSource = strSource.Substring(0, 7);  // 丢掉原有 8 位的校验位
            if (strSource.Length < 10)
                strSource = strSource.PadRight(10, '0');

            strTarget = strSource;

            strTarget = strTarget.Insert(4, "-");
            strTarget = strTarget.Insert(4 + 3 + 1, "-");
            strTarget = strTarget.Insert(7 + 2 + 1 + 1, "-");

            if (bForce13 == true)
            {
                if (strTarget.Length == 13)
                    strTarget = strPrefix + "-" + strTarget;
            }
            else if (bAuto == true && bHasRemovePrefix977 == true)
            {
                strTarget = strPrefix + "-" + strTarget;
            }

            bool bVerifyChanged = false;

            // 重新计算校验码
            // 重新添加ISBN-10的校验位。因为条码号中ISBN-13校验位算法不同。
            if (bRemainVerifyChar == false)
            {
                if (strTarget.Length == 13)
                {
                    char old_ver = strTarget[12];
                    strTarget = strTarget.Substring(0, strTarget.Length - 1 - 4);
                    char v = GetIssn8VerifyChar(strTarget);
                    strTarget += new string(v, 1);

                    if (old_ver != v)
                        bVerifyChanged = true;
                }
                else if (strTarget.Length == 17)
                {
                    char old_ver = strTarget[16];

                    strTarget = strTarget.Substring(0, strTarget.Length - 1);
                    char v = GetIsbn13VerifyChar(strTarget);
                    strTarget += new string(v, 1);

                    if (old_ver != v)
                        bVerifyChanged = true;
                }
            }

            if (bHasRemovePrefix977 == true
                && bForce8 == true)
                return 0;   // 移走977后，校验位肯定要发生变化。因此不通知这种变化

            if (bAdjustLength == false
                && bForce13 == true
                && strISSN.Trim().Replace("-", "").Length == 8)
                return 0;   // 加入了前缀后，校验位肯定要发生变化，因此不通知这种变化

            if (bVerifyChanged == true)
                return 1;

            return 0;
        }


        class Range
        {
            public string Left = "";
            public string Right = "";
        }

        static Range GetRangeValue(XmlElement element)
        {
            string strRange = "";
            XmlElement range = element.SelectSingleNode("Range") as XmlElement;
            if (range == null)
                return null;

            strRange = range.InnerText.Trim();

            string strLength = "";
            XmlElement length = element.SelectSingleNode("Length") as XmlElement;
            if (length == null)
                return null;

            strLength = length.InnerText.Trim();

            int nLength = 0;
            if (int.TryParse(strLength, out nLength) == false)
                return null;

            string strLeft = "";
            string strRight = "";

            StringUtil.ParseTwoPart(strRange, "-", out strLeft,
                out strRight);
            Range result = new Range();
            result.Left = strLeft.Substring(0, nLength);
            result.Right = strRight.Substring(0, nLength);

            return result;
        }

        // 将ISBN号字符串变换为图书条码号形态的ISBN字符串
        // 步骤：
        // 1)去掉所有的'-'
        // 2)看是不是有前缀'978'，如果没有，就加上
        // 3)重新计算校验位
        public static string GetISBnBarcode(string strPureISBN)
        {
            string strText = strPureISBN.Replace("-", "");
            if (strText.Length < 3)
                return strText; // error

            string strHead = strPureISBN.Substring(0, 3);

            if (strHead == "978" || strHead == "979")
            {
            }
            else
            {
                strText = "978" + strText;
            }

            try
            {
                char v = GetIsbn13VerifyChar(strText);
                strText = strText.Substring(0, 12);
                strText += v;

                return strText;
            }
            catch
            {
                return strText; // error
            }

        }

        public static bool IsIsbn13(string strSource)
        {
            if (string.IsNullOrEmpty(strSource) == true)
                return false;
            strSource = strSource.Replace("-", "").Trim();
            if (string.IsNullOrEmpty(strSource) == true)
                return false;

            // 13位、无-、前缀为978
            if (strSource.Length == 13
                && strSource.IndexOf("-") == -1
                && (strSource.Substring(0, 3) == "978" || strSource.Substring(0, 3) == "979")
                )
                return true;

            return false;
        }

        public static string GetPublisherCode(string strSource)
        {
            if (strSource.IndexOf("-") == -1)
            {
                throw new Exception("ISBN '" + strSource + "' 中没有符号'-'，无法取出版社号码部分。请先为ISBN加上'-'");
            }

            string[] parts = strSource.Split(new char[] { '-' });
            if (IsIsbn13(strSource) == true)
            {
                if (parts.Length >= 3)
                    return parts[2].Trim();
            }
            else
            {
                if (parts.Length >= 2)
                    return parts[1].Trim();
            }

            throw new Exception("ISBN '" + strSource + "' 格式不正确，符号'-'数目不足");
        }
    }
}
