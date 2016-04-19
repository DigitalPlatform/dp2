using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

using DigitalPlatform.Xml;

namespace DigitalPlatform.Library
{
    /// <summary>
    /// ISBN号分析器，帮助插入'-'
    /// </summary>
    public class IsbnSplitter1
    {
        XmlDocument dom = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="strIsbnFileName"></param>
        public IsbnSplitter1(string strIsbnFileName)
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
        ///  校验ISBN第一部分是否正确
        /// </summary>
        /// <param name="strFirstPart"></param>
        /// <param name="strError"></param>
        /// <returns>return:-1	error,0	correct</returns>
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

        /// <summary>
        /// 计算出校验位
        /// </summary>
        /// <param name="strISBN"></param>
        /// <returns></returns>
        public static char GetIsbnVerifyChar(string strISBN)
        {
            strISBN = strISBN.Trim();
            strISBN = strISBN.Replace("-", "");
            strISBN = strISBN.Replace(" ", "");


            if (strISBN.Length < 9)
                throw new Exception("用于计算校验位的ISBN长度至少要在9位数字以上(不包括横杠在内)");

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
        /// 在ISBN子符串中适当的位置插入'-'符号
        /// </summary>
        /// <param name="strISBN"></param>
        /// <param name="strTarget"></param>
        /// <param name="strError"></param>
        /// <returns></returns>
        public int IsbnInsertHyphen(
            string strISBN,
            out string strTarget,
            out string strError)
        {
            strTarget = "";
            strError = "";

            string strSource;
            int nFirstLen;
            int nSecondLen;

            strSource = strISBN;
            strSource = strSource.Trim();

            // 是否为条码号
            if (strSource.Length == 13
                && strSource.IndexOf("-") == -1
                && strSource.Substring(0, 3) == "978")
            {
                strSource = strSource.Substring(3, 9);  // 丢弃前三位，和最后一位

                // 添加校验位
                char v = GetIsbnVerifyChar(strSource);
                strSource += new string(v, 1);
            }

            strSource = strSource.Replace("-", "");
            strSource = strSource.Replace(" ", "");

            if (strSource.Length != 10)
            {
                strError = "ISBN中(除'-'以外)应为10位有效字符(" + strSource + " " + Convert.ToString(strSource.Length) + ")";
                return -1;
            }

            // 观察第一部分
            string strFirstPart = strSource.Substring(0, 1);
            if (InRange(strFirstPart, "0", "7") == true)
            {
                nFirstLen = 1;
                goto DOSECOND;
            }

            strFirstPart = strSource.Substring(0, 2);
            if (InRange(strFirstPart, "80", "94") == true)
            {
                nFirstLen = 2;
                goto DOSECOND;
            }

            strFirstPart = strSource.Substring(0, 3);
            if (InRange(strFirstPart, "950", "994") == true)
            {
                nFirstLen = 3;
                goto DOSECOND;
            }

            strFirstPart = strSource.Substring(0, 4);
            if (InRange(strFirstPart, "9950", "9989") == true)
            {
                nFirstLen = 4;
                goto DOSECOND;
            }

            strFirstPart = strSource.Substring(0, 5);
            if (InRange(strFirstPart, "99900", "99999") == true)
            {
                nFirstLen = 5;
                goto DOSECOND;
            }

            strError = "第一部分格式错误";    // 是否需要解释一下?
            return -1;

        DOSECOND:

            XmlNodeList nodes = this.dom.DocumentElement.SelectNodes("agency/group[@name='" + strFirstPart + "']/range");
            if (nodes.Count == 0)
            {
                strError = "ISBN数据中没有找到name='" + strFirstPart + "'的<group>元素 ...";
                return -1;
            }



            string strSecondPart = "";

            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];

                string strValue = DomUtil.GetAttr(node, "value").Trim();

                int nRet = strValue.IndexOf('-');
                if (nRet == -1)
                {
                    strError = "数据节点 " + node.OuterXml + "格式错误, value值中无'-'";
                    return -1;
                }

                string strLeft = strValue.Substring(0, nRet).Trim();
                string strRight = strValue.Substring(nRet + 1).Trim();

                if (strLeft.Length != strRight.Length)
                {
                    strError = "数据节点 " + node.OuterXml + "格式错误, value值'" + strValue + "'中两个数字宽度不等。";
                    return -1;
                }

                int nWidth = strLeft.Length;

                if (nWidth != strSecondPart.Length)
                    strSecondPart = strSource.Substring(nFirstLen, nWidth);


                if (InRange(strSecondPart, strLeft, strRight) == true)
                {
                    nSecondLen = nWidth;
                    goto FINISH;
                }

            }

            strError = "第二部分格式错误 nFirstLen=[" + Convert.ToString(nFirstLen);
            return -1;

        FINISH:
            strTarget = strSource;

            strTarget = strTarget.Insert(nFirstLen, "-");
            strTarget = strTarget.Insert(nFirstLen + nSecondLen + 1, "-");
            strTarget = strTarget.Insert(9 + 1 + 1, "-");

            return 0;
        }

    }
}
