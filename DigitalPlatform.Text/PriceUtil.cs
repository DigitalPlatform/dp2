using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Diagnostics;

namespace DigitalPlatform.Text
{
    public class PriceUtil
    {
        #region 级联操作函数

        public static string Add(string strText1, string strText2)
        {
            PriceUtil currency = new PriceUtil(strText1);
            return currency.Add(strText2).ToString();
        }

        string _current = "";   // 金额字符串

        public PriceUtil()
        {
        }

        public PriceUtil(string strText)
        {
            this._current = strText;
        }

        public PriceUtil Set(string strText)
        {
            this._current = strText;
            return this;
        }

        public PriceUtil Clear()
        {
            this._current = "";
            return this;
        }

        public PriceUtil Add(string strText)
        {
            return this.Operator(strText, "+");
        }

        public PriceUtil Substract(string strText)
        {
            return this.Operator(strText, "-");
        }

        public PriceUtil Mutiple(string strText)
        {
            return this.Operator(strText, "*");
        }

        public PriceUtil Divide(string strText)
        {
            return this.Operator(strText, "/");
        }

#if NO
        public PriceUtil Operator(string strText, string strOperator)
        {
            int nRet = 0;
            string strError = "";
            string strResult = "";

            if (strOperator == "*" || strOperator == "/")
            {
                if (string.IsNullOrEmpty(this._current)
                    || string.IsNullOrEmpty(strText))
                    throw new ArgumentException("乘法和除法运算要求两个操作数都不能为空");

                string strString = this._current + strOperator + strText;
                nRet = SumPrices(strString,
    out strResult,
    out strError);
                if (nRet == -1)
                {
                    strError = strError + " (SumPrices)'" + strString + "'";
                    throw new Exception(strError);
                }
                this._current = strResult;
                return this;
            }


            if (string.IsNullOrEmpty(strText))
                return this;

            List<string> prices = new List<string>();
            string s1 = "";
            nRet = SumPrices(this._current,
                out s1,
                out strError);
            if (nRet == -1)
            {
                strError = strError + " (SumPrices)'" + this._current + "'";
                throw new Exception(strError);
            }

            string s2 = "";
            if (strOperator == "-")
            {
                nRet = NegativePrices(strText,
                    true,
                    out s2,
                    out strError);
                if (nRet == -1)
                {
                    strError = strError + " (NegativePrices)'" + strText + "'";
                    throw new Exception(strError);
                }
            }
            else
            {
                nRet = SumPrices(strText,
                    out s2,
                    out strError);
                if (nRet == -1)
                {
                    strError = strError + " (SumPrices)'" + strText + "'";
                    throw new Exception(strError);
                }
            }

            prices.Add(s1);
            prices.Add(s2);

            nRet = TotalPrice(prices,
    out strResult,
    out strError);
            if (nRet == -1)
            {
                strError = strError + " (TotalPrices)'" + StringUtil.MakePathList(prices) + "'";
                throw new Exception(strError);
            }
            this._current = strResult;
            return this;
        }
#endif

        public PriceUtil Operator(string strText, string strOperator)
        {
            int nRet = 0;
            string strError = "";
            string strResult = "";

            if (strOperator == "*" || strOperator == "/")
            {
                if (string.IsNullOrEmpty(this._current)
                    || string.IsNullOrEmpty(strText))
                    throw new ArgumentException("乘法和除法运算要求两个操作数都不能为空");

                string strString = this._current + strOperator + strText;
                nRet = SumPrices(strString,
    out strResult,
    out strError);
                if (nRet == -1)
                {
                    strError = strError + " (SumPrices)'" + strString + "'";
                    throw new Exception(strError);
                }
                this._current = strResult;
                return this;
            }


            if (string.IsNullOrEmpty(strText))
                return this;

            string s2 = "";
            if (strOperator == "-")
            {
                nRet = NegativePrices(strText,
                    true,
                    out s2,
                    out strError);
                if (nRet == -1)
                {
                    strError = strError + " (NegativePrices)'" + strText + "'";
                    throw new Exception(strError);
                }
            }
            else
            {
#if NO
                nRet = SumPrices(strText,
                    out s2,
                    out strError);
                if (nRet == -1)
                {
                    strError = strError + " (SumPrices)'" + strText + "'";
                    throw new Exception(strError);
                }
#endif
                s2 = strText;
            }

            string s3 = JoinPriceString(this._current, s2);

            nRet = SumPrices(s3,
    out strResult,
    out strError);
            if (nRet == -1)
            {
                strError = strError + " (SumPrices)'" + s3 + "'";
                throw new Exception(strError);
            }

            this._current = strResult;
            return this;
        }

        public override string ToString()
        {
            return this._current;
        }

        #endregion

        // 计算价格乘积
        // 从PrintOrderForm中转移过来
        public static int MultiPrice(string strPrice,
            int nCopy,
            out string strResult,
            out string strError)
        {
            strError = "";
            strResult = "";

            string strPrefix = "";
            string strValue = "";
            string strPostfix = "";
            int nRet = PriceUtil.ParsePriceUnit(strPrice,
                out strPrefix,
                out strValue,
                out strPostfix,
                out strError);
            if (nRet == -1)
                return -1;

            decimal value = 0;
            try
            {
                value = Convert.ToDecimal(strValue);
            }
            catch
            {
                strError = "数字 '" + strValue + "' 格式不正确";
                return -1;
            }

            value *= (decimal)nCopy;

            strResult = strPrefix + value.ToString() + strPostfix;
            return 0;
        }

        // 能够处理乘号或者除号了
        public static string GetPurePrice(string strText)
        {
            string strError = "";

            string strLeft = "";
            string strRight = "";
            string strOperator = "";
            // 先处理乘除号
            // return:
            //      -1  出错
            //      0   没有发现乘号、除号
            //      1   发现乘号或者除号
            int nRet = ParseMultipcation(strText,
                out strLeft,
                out strRight,
                out strOperator,
                out strError);
            if (nRet == -1)
                throw new Exception(strError);

            if (nRet == 0)
                return OldGetPurePrice(strText);

            Debug.Assert(nRet == 1, "");
            string strMultiper = "";
            string strPrice = "";

            if (StringUtil.IsDouble(strLeft) == false
                && StringUtil.IsDouble(strRight) == false)
            {
                strError = "金额字符串格式错误 '" + strText + "'。乘号或除号的两边必须至少有一边是纯数字";
                throw new Exception(strError);
            }

            if (StringUtil.IsDouble(strLeft) == false)
            {
                strPrice = strLeft;
                strMultiper = strRight;
            }
            else if (StringUtil.IsDouble(strRight) == false)
            {
                strPrice = strRight;
                strMultiper = strLeft;
                if (strOperator == "/")
                {
                    strError = "金额字符串 '" + strText + "' 格式错误。除号的右边才能是纯数字";
                    throw new Exception(strError);
                }
            }
            else
            {
                // 默认左边是价格，右边是倍率
                strPrice = strLeft;
                strMultiper = strRight;
            }

            string strValue = OldGetPurePrice(strPrice);

            decimal value = 0;
            try
            {
                value = Convert.ToDecimal(strValue);
            }
            catch
            {
                strError = "单个金额字符串 '" + strPrice + "' 中, 数字部分 '" + strValue + "' 格式不正确";
                throw new Exception(strError);
            }

            if (String.IsNullOrEmpty(strOperator) == false)
            {
                double multiper = 0;
                try
                {
                    multiper = Convert.ToDouble(strMultiper);
                }
                catch
                {
                    strError = "数字 '" + strMultiper + "' 格式不正确";
                    throw new Exception(strError);
                }

                if (strOperator == "*")
                {
                    value = (decimal)((double)value * multiper);
                }
                else
                {
                    Debug.Assert(strOperator == "/", "");

                    if (multiper == 0)
                    {
                        strError = "金额字符串格式错误 '" + strText + "'。除法运算中，除数不能为0";
                        throw new Exception(strError);
                    }

                    value = (decimal)((double)value / multiper);
                }

                return value.ToString();
            }

            return strValue;
        }

        // 从复杂的字符串中，析出纯粹价格数字部分（包括小数点）。
        // 2006/11/15 能处理数字前的正负号
        public static string OldGetPurePrice(string strPrice)
        {
            if (String.IsNullOrEmpty(strPrice) == true)
                return strPrice;

            string strResult = "";
            int nSegment = 0;   // 0 非数字段 1数字段 2 非数字段
            int nPointCount = 0;

            bool bNegative = false; // 是否为负数

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
                    if (nSegment == 0)
                    {
                        if (ch == '-')
                            bNegative = true;
                    }

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

            // 2008/11/15
            if (bNegative == true)
                return "-" + strResult;

            return strResult;
        }


        // 汇总价格
        // 货币单位不同的，互相独立
        // 本函数主要用于显示，可以自动处理出错情况 -- 把错误字符串当作结果返回
        // return:
        //      汇总后的价格字符串
        public static string TotalPrice(List<string> prices)
        {
            string strResult = "";
            string strError = "";

            int nRet = TotalPrice(prices,
                out strResult,
                out strError);
            if (nRet == -1)
                return strError;

            return strResult;
        }

        // 汇总价格
        // 货币单位不同的，互相独立
        // 本函数还有另外一个版本，是返回List<string>的
        // return:
        //      -1  error
        //      0   succeed
        public static int TotalPrice(List<string> prices,
            out string strTotalPrice,
            out string strError)
        {
            strError = "";
            strTotalPrice = "";
            List<string> results = null;
            int nRet = TotalPrice(prices,
                out results,
                out strError);
            if (nRet == -1)
                return -1;

            Debug.Assert(results != null, "");
            if (results.Count == 0)
                return 0;

            strTotalPrice = JoinPriceString(results);
            return 0;
        }

        // 把若干价格字符串结合起来
        public static string JoinPriceString(List<string> prices)
        {
            string strResult = "";
            for (int i = 0; i < prices.Count; i++)
            {
                string strPrice = prices[i].Trim();
                if (String.IsNullOrEmpty(strPrice) == true)
                    continue;
                if (strPrice[0] == '+' || strPrice[0] == '-')
                    strResult += strPrice;
                else
                {
                    // 第一个价格前面不用加+号
                    if (String.IsNullOrEmpty(strResult) == false)
                        strResult += "+";

                    strResult += strPrice;
                }
            }

            return strResult;
        }

        // 连接两个价格字符串
        public static string JoinPriceString(string strPrice1,
            string strPrice2)
        {
            if (string.IsNullOrEmpty(strPrice1) == true
                && string.IsNullOrEmpty(strPrice2) == true)
                return "";

            if (string.IsNullOrEmpty(strPrice1) == true)
                return strPrice2;
            if (string.IsNullOrEmpty(strPrice2) == true)
                return strPrice1;

            strPrice1 = strPrice1.Trim();
            strPrice2 = strPrice2.Trim();

            if (String.IsNullOrEmpty(strPrice1) == true
                && String.IsNullOrEmpty(strPrice2) == true)
                return "";

            if (String.IsNullOrEmpty(strPrice1) == true)
                return strPrice2;

            if (String.IsNullOrEmpty(strPrice2) == true)
                return strPrice1;

            if (strPrice2[0] == '+'
                || strPrice2[0] == '-')
                return strPrice1 + strPrice2;

            return strPrice1 + "+" + strPrice2;
        }

        // 将形如"-123.4+10.55-20.3"的价格字符串反转正负号
        // parameters:
        //      bSum    是否要顺便汇总? true表示要汇总
        public static int NegativePrices(string strPrices,
            bool bSum,
            out string strResultPrice,
            out string strError)
        {
            strError = "";
            strResultPrice = "";

            strPrices = strPrices.Trim();

            if (String.IsNullOrEmpty(strPrices) == true)
                return 0;

            List<string> prices = null;
            // 将形如"-123.4+10.55-20.3"的价格字符串切割为单个的价格字符串，并各自带上正负号
            // return:
            //      -1  error
            //      0   succeed
            int nRet = SplitPrices(strPrices,
                out prices,
                out strError);
            if (nRet == -1)
                return -1;

            // 直接每个反转
            if (bSum == false)
            {
                for (int i = 0; i < prices.Count; i++)
                {
                    string strOnePrice = prices[i];
                    if (String.IsNullOrEmpty(strOnePrice) == true)
                        continue;
                    if (strOnePrice[0] == '+')
                        strResultPrice += "-" + strOnePrice.Substring(1);
                    else if (strOnePrice[0] == '-')
                        strResultPrice += "+" + strOnePrice.Substring(1);
                    else
                        strResultPrice += "-" + strOnePrice;    // 缺省为正数
                }

                return 0;
            }

            List<string> results = new List<string>();

            // 汇总价格
            // 货币单位不同的，互相独立
            // return:
            //      -1  error
            //      0   succeed
            nRet = TotalPrice(prices,
                out results,
                out strError);
            if (nRet == -1)
                return -1;

            for (int i = 0; i < results.Count; i++)
            {
                string strOnePrice = results[i];
                if (String.IsNullOrEmpty(strOnePrice) == true)
                    continue;
                if (strOnePrice[0] == '+')
                    strResultPrice += "-" + strOnePrice.Substring(1);
                else if (strOnePrice[0] == '-')
                    strResultPrice += "+" + strOnePrice.Substring(1);
                else
                    strResultPrice += "-" + strOnePrice;    // 缺省为正数
            }

            return 0;
        }

        // 比较两个价格字符串
        // return:
        //      -3  币种不同，无法直接比较 strError中有说明
        //      -2  error strError中有说明
        //      -1  strPrice1小于strPrice2
        //      0   等于
        //      1   strPrice1大于strPrice2
        public static int Compare(string strPrice1,
            string strPrice2,
            out string strError)
        {
            strError = "";

            string strPrefix1 = "";
            string strValue1 = "";
            string strPostfix1 = "";
            int nRet = ParsePriceUnit(strPrice1,
                out strPrefix1,
                out strValue1,
                out strPostfix1,
                out strError);
            if (nRet == -1)
            {
                strError = "金额字符串1 '" + strPrice1 + "' 解析出错: " + strError;
                return -2;
            }

            decimal value1 = 0;
            try
            {
                value1 = Convert.ToDecimal(strValue1);
            }
            catch
            {
                strError = "数字 '" + strValue1 + "' 格式不正确";
                return -2;
            }

            if (strPrefix1 == "" && strPostfix1 == "")
                strPrefix1 = "CNY";

            string strPrefix2 = "";
            string strValue2 = "";
            string strPostfix2 = "";
            nRet = ParsePriceUnit(strPrice2,
                out strPrefix2,
                out strValue2,
                out strPostfix2,
                out strError);
            if (nRet == -1)
            {
                strError = "金额字符串2 '" + strPrice2 + "' 解析出错: " + strError;
                return -2;
            }

            if (strPrefix2 == "" && strPostfix2 == "")
                strPrefix2 = "CNY";

            if (strPrefix1 != strPrefix2
                || strPostfix1 != strPostfix2)
            {
                strError = "币种不同(一个是'" + strPrice1 + "'，一个是'" + strPrice2 + "')，无法进行金额比较";
                return -3;
            }

            decimal value2 = 0;
            try
            {
                value2 = Convert.ToDecimal(strValue2);
            }
            catch
            {
                strError = "数字 '" + strValue2 + "' 格式不正确";
                return -2;
            }

            if (value1 < value2)
                return -1;

            if (value1 == value2)
                return 0;

            Debug.Assert(value1 > value2, "");

            return 1;
        }

        // 看看若干个价格字符串是否都表示了0?
        // return:
        //      -1  出错
        //      0   不为0
        //      1   为0
        public static int IsZero(List<string> prices,
            out string strError)
        {
            strError = "";

            List<CurrencyItem> items = new List<CurrencyItem>();

            // 变换为PriceItem
            for (int i = 0; i < prices.Count; i++)
            {
                string strPrefix = "";
                string strValue = "";
                string strPostfix = "";
                int nRet = ParsePriceUnit(prices[i],
                    out strPrefix,
                    out strValue,
                    out strPostfix,
                    out strError);
                if (nRet == -1)
                    return -1;
                decimal value = 0;
                try
                {
                    value = Convert.ToDecimal(strValue);
                }
                catch
                {
                    strError = "数字 '" + strValue + "' 格式不正确";
                    return -1;
                }

                CurrencyItem item = new CurrencyItem();
                item.Prefix = strPrefix;
                item.Postfix = strPostfix;
                item.Value = value;

                items.Add(item);
            }

            // 分析
            for (int i = 0; i < items.Count; i++)
            {
                CurrencyItem item = items[i];

                if (item.Value != 0)
                    return 0;   // 中间出现了不为0的
            }

            return 1;   // 全部为0
        }

        // 将形如"-123.4+10.55-20.3"的价格字符串归并汇总
        public static int SumPrices(string strPrices,
            out List<string> results,
            out string strError)
        {
            strError = "";
            results = new List<string>();

            List<string> prices = null;
            // 将形如"-123.4+10.55-20.3"的价格字符串切割为单个的价格字符串，并各自带上正负号
            // return:
            //      -1  error
            //      0   succeed
            int nRet = SplitPrices(strPrices,
                out prices,
                out strError);
            if (nRet == -1)
                return -1;

            // 汇总价格
            // 货币单位不同的，互相独立
            // return:
            //      -1  error
            //      0   succeed
            nRet = TotalPrice(prices,
                out results,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        // 2012/3/7
        // 将形如"-123.4+10.55-20.3"的价格字符串归并汇总
        public static int SumPrices(string strPrices,
            out string strSumPrices,
            out string strError)
        {
            strError = "";
            strSumPrices = "";

            List<string> prices = null;
            // 将形如"-123.4+10.55-20.3"的价格字符串切割为单个的价格字符串，并各自带上正负号
            // return:
            //      -1  error
            //      0   succeed
            int nRet = SplitPrices(strPrices,
                out prices,
                out strError);
            if (nRet == -1)
                return -1;

            List<string> results = new List<string>();

            // 汇总价格
            // 货币单位不同的，互相独立
            // return:
            //      -1  error
            //      0   succeed
            nRet = TotalPrice(prices,
                out results,
                out strError);
            if (nRet == -1)
                return -1;

            strSumPrices = JoinPriceString(results);
            return 0;
        }

        // 将形如"-123.4+10.55-20.3"的价格字符串切割为单个的价格字符串，并各自带上正负号
        // return:
        //      -1  error
        //      0   succeed
        public static int SplitPrices(string strPrices,
            out List<string> prices,
            out string strError)
        {
            strError = "";
            prices = new List<string>();

            strPrices = strPrices.Replace("+", ",+").Replace("-", ",-");
            string[] parts = strPrices.Split(new char[] { ',' });
            for (int i = 0; i < parts.Length; i++)
            {
                string strPart = parts[i].Trim();
                if (String.IsNullOrEmpty(strPart) == true)
                    continue;
                prices.Add(strPart);
            }

            return 0;
        }

        // 2012/3/7
        // 校验金额字符串格式正确性
        // return:
        //      -1  有错
        //      0   没有错
        public static int VerifyPriceFormat(
            List<string> valid_formats,
            string strString,
            out string strError)
        {
            strError = "";

            // 没有格式定义，就不作校验
            if (valid_formats.Count == 0)
                return 0;

            string strPrefix = "";
            string strValue = "";
            string strPostfix = "";

            int nRet = ParsePriceUnit(strString,
            out strPrefix,
            out strValue,
            out strPostfix,
            out strError);
            if (nRet == -1)
                return -1;

            foreach (string fmt in valid_formats)
            {
                string[] parts = fmt.Split(new char[] { '|' });
                string strPrefixFormat = "";
                string strValueFormat = "";
                string strPostfixFormat = "";
                if (parts.Length > 0)
                    strPrefixFormat = parts[0];
                if (parts.Length > 1)
                    strValueFormat = parts[1];
                if (parts.Length > 2)
                    strPostfixFormat = parts[2];

                if (string.IsNullOrEmpty(strPrefixFormat) == false
                    && strPrefix != strPrefixFormat)
                    continue;

                // 暂时不校验value部分

                if (string.IsNullOrEmpty(strPostfixFormat) == false
    && strPostfix != strPostfixFormat)
                    continue;

                return 0;
            }

            strError = "金额字符串 '" + strString + "' 的格式不符合定义 '" + StringUtil.MakePathList(valid_formats) + "' 的要求";
            return -1;
        }

        // 分析价格参数
        // 允许前面出现+ -号
        // return:
        //      -1  出错
        //      0   成功
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
            // 去掉逗号 2012/9/1
            strString = strString.Replace(",", "");
            strString = strString.Replace("，", "");

            if (String.IsNullOrEmpty(strString) == true)
            {
                strError = "金额字符串为空";
                return -1;
            }

            bool bNegative = false; // 是否为负数
            if (strString[0] == '+')
            {
                bNegative = false;
                strString = strString.Substring(1).Trim();
            }
            else if (strString[0] == '-')
            {
                bNegative = true;
                strString = strString.Substring(1).Trim();
            }

            if (String.IsNullOrEmpty(strString) == true)
            {
                strError = "金额字符串(除了正负号以外)为空";
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

            strPrefix = strPrefix.Trim();   // 2012/3/7

            if (string.IsNullOrEmpty(strValue) == true)
            {
                strError = "金额字符串 '" + strString + "' 缺乏数字部分";
                return -1;
            }

            // 2012/1/5
            if (strPrefix.IndexOfAny(new char[] { '+', '-' }) != -1
                || strPostfix.IndexOfAny(new char[] { '+', '-' }) != -1)
            {
                strError = "金额字符串 '" + strString + "' 格式错误：符号 + 或 - 只应出现在单个金额字符串的第一个字符位置 (strPrefix='" + strPrefix + "' strPostfix='" + strPostfix + "')";
                return -1;
            }

            // 2008/11/11
            if (bNegative == true)
                strValue = "-" + strValue;

            return 0;
        }

        // return:
        //      -1  出错
        //      0   没有发现乘号、除号。注意此时 strLeft 和 strRight 返回的都是空
        //      1   发现乘号或者除号
        static int ParseMultipcation(string strText,
            out string strLeft,
            out string strRight,
            out string strOperator,
            out string strError)
        {
            strLeft = "";
            strRight = "";
            strOperator = "";
            strError = "";

            int nRet = strText.IndexOfAny(new char[] { '/', '*' });
            if (nRet == -1)
                return 0;

            strLeft = strText.Substring(0, nRet).Trim();
            strRight = strText.Substring(nRet + 1).Trim();
            strOperator = strText.Substring(nRet, 1);
            return 1;
        }

        // 解析单个金额字符串。例如 CNY10.00 或 -CNY100.00/7
        public static int ParseSinglePrice(string strText,
            out CurrencyItem item,
            out string strError)
        {
            strError = "";
            item = new CurrencyItem();

            if (string.IsNullOrEmpty(strText) == true)
                return 0;

            strText = strText.Trim();

            if (String.IsNullOrEmpty(strText) == true)
                return 0;

            string strLeft = "";
            string strRight = "";
            string strOperator = "";
            // 先处理乘除号
            // return:
            //      -1  出错
            //      0   没有发现乘号、除号
            //      1   发现乘号或者除号
            int nRet = ParseMultipcation(strText,
                out strLeft,
                out strRight,
                out strOperator,
                out strError);
            if (nRet == -1)
                return -1;

            if (nRet == 1)
            {
                Debug.Assert(strOperator.Length == 1, "");

                if (String.IsNullOrEmpty(strLeft) == true
                    || String.IsNullOrEmpty(strRight) == true)
                {
                    strError = "金额字符串格式错误 '" + strText + "'。乘号或除号的两边必须都有内容";
                    return -1;
                }
            }

            string strPrice = "";
            string strMultiper = "";

            if (nRet == 0)
            {
                Debug.Assert(String.IsNullOrEmpty(strLeft) == true, "");
                Debug.Assert(String.IsNullOrEmpty(strRight) == true, "");
                Debug.Assert(String.IsNullOrEmpty(strOperator) == true, "");

                strPrice = strText.Trim();
            }
            else
            {
                Debug.Assert(nRet == 1, "");

                if (StringUtil.IsDouble(strLeft) == false
                    && StringUtil.IsDouble(strRight) == false)
                {
                    strError = "金额字符串格式错误 '" + strText + "'。乘号或除号的两边必须至少有一边是纯数字";
                    return -1;
                }


                if (StringUtil.IsDouble(strLeft) == false)
                {
                    strPrice = strLeft;
                    strMultiper = strRight;
                }
                else if (StringUtil.IsDouble(strRight) == false)
                {
                    strPrice = strRight;
                    strMultiper = strLeft;
                    if (strOperator == "/")
                    {
                        strError = "金额字符串格式错误 '" + strText + "'。除号的右边才能是纯数字";
                        return -1;
                    }
                }
                else
                {
                    // 默认左边是价格，右边是倍率
                    strPrice = strLeft;
                    strMultiper = strRight;
                }
            }

            string strPrefix = "";
            string strValue = "";
            string strPostfix = "";
            nRet = ParsePriceUnit(strPrice,
                out strPrefix,
                out strValue,
                out strPostfix,
                out strError);
            if (nRet == -1)
                return -1;

            if (string.IsNullOrEmpty(strValue) == true)
            {
                strError = "金额字符串 '" + strPrice + "' 中没有包含数字部分";
                return -1;
            }

            decimal value = 0;
            try
            {
                value = Convert.ToDecimal(strValue);
            }
            catch
            {
                strError = "金额字符串 '" + strPrice + "' 中, 数字部分 '" + strValue + "' 格式不正确";
                return -1;
            }

            if (String.IsNullOrEmpty(strOperator) == false)
            {
                double multiper = 0;
                try
                {
                    multiper = Convert.ToDouble(strMultiper);
                }
                catch
                {
                    strError = "数字 '" + strMultiper + "' 格式不正确";
                    return -1;
                }

                if (strOperator == "*")
                {
                    value = (decimal)((double)value * multiper);
                }
                else
                {
                    Debug.Assert(strOperator == "/", "");

                    if (multiper == 0)
                    {
                        strError = "金额字符串格式错误 '" + strText + "'。除法运算中，除数不能为0";
                        return -1;
                    }

                    // value = (decimal)((double)value / multiper);
                    value = Convert.ToDecimal((double)value / multiper);
                }
            }

            item.Prefix = strPrefix.ToUpper();
            item.Postfix = strPostfix.ToUpper();
            item.Value = value;

            // 缺省货币为人民币
            if (item.Prefix == "" && item.Postfix == "")
                item.Prefix = "CNY";

            return 0;
        }

        // 汇总价格
        // 货币单位不同的，互相独立
        // parameters:
        //      prices  若干单一价格字符串构成的数组。并未进行过排序
        // return:
        //      -1  error
        //      0   succeed
        public static int TotalPrice(List<string> prices,
            out List<string> results,
            out string strError)
        {
            strError = "";
            results = new List<string>();

            List<CurrencyItem> items = new List<CurrencyItem>();

            // 变换为PriceItem
            // for (int i = 0; i < prices.Count; i++)
            foreach (string price in prices)
            {
                // string strText = prices[i].Trim();
                if (price == null)
                    continue;
                string strText = price.Trim();

                if (String.IsNullOrEmpty(strText) == true)
                    continue;

#if NO
                string strLeft = "";
                string strRight = "";
                string strOperator = "";
                // 先处理乘除号
                // return:
                //      -1  出错
                //      0   没有发现乘号、除号
                //      1   发现乘号或者除号
                int nRet = ParseMultipcation(strText,
                    out strLeft,
                    out strRight,
                    out strOperator,
                    out strError);
                if (nRet == -1)
                    return -1;

                if (nRet == 1)
                {
                    Debug.Assert(strOperator.Length == 1, "");

                    if (String.IsNullOrEmpty(strLeft) == true
                        || String.IsNullOrEmpty(strRight) == true)
                    {
                        strError = "金额字符串格式错误 '" + strText + "'。乘号或除号的两边必须都有内容";
                        return -1;
                    }
                }


                string strPrice = "";
                string strMultiper = "";

                if (nRet == 0)
                {
                    Debug.Assert(String.IsNullOrEmpty(strLeft) == true, "");
                    Debug.Assert(String.IsNullOrEmpty(strRight) == true, "");
                    Debug.Assert(String.IsNullOrEmpty(strOperator) == true, "");

                    strPrice = strText.Trim();
                }
                else
                {
                    Debug.Assert(nRet == 1, "");

                    if (StringUtil.IsDouble(strLeft) == false
                        && StringUtil.IsDouble(strRight) == false)
                    {
                        strError = "金额字符串格式错误 '" + strText + "'。乘号或除号的两边必须至少有一边是纯数字";
                        return -1;
                    }


                    if (StringUtil.IsDouble(strLeft) == false)
                    {
                        strPrice = strLeft;
                        strMultiper = strRight;
                    }
                    else if (StringUtil.IsDouble(strRight) == false)
                    {
                        strPrice = strRight;
                        strMultiper = strLeft;
                        if (strOperator == "/")
                        {
                            strError = "金额字符串格式错误 '" + strText + "'。除号的右边才能是纯数字";
                            return -1;
                        }
                    }
                    else
                    {
                        // 默认左边是价格，右边是倍率
                        strPrice = strLeft;
                        strMultiper = strRight;
                    }
                }

                string strPrefix = "";
                string strValue = "";
                string strPostfix = "";
                nRet = ParsePriceUnit(strPrice,
                    out strPrefix,
                    out strValue,
                    out strPostfix,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 2012/1/5
                if (string.IsNullOrEmpty(strValue) == true)
                {
                    strError = "单个金额字符串 '" + strPrice + "' 中没有包含数字部分";
                    return -1;
                }

                decimal value = 0;
                try
                {
                    value = Convert.ToDecimal(strValue);
                }
                catch
                {
                    strError = "单个金额字符串 '" + strPrice + "' 中, 数字部分 '" + strValue + "' 格式不正确";
                    return -1;
                }

                if (String.IsNullOrEmpty(strOperator) == false)
                {
                    double multiper = 0;
                    try
                    {
                        multiper = Convert.ToDouble(strMultiper);
                    }
                    catch
                    {
                        strError = "数字 '" + strMultiper + "' 格式不正确";
                        return -1;
                    }

                    if (strOperator == "*")
                    {
                        value = (decimal)((double)value * multiper);
                    }
                    else
                    {
                        Debug.Assert(strOperator == "/", "");

                        if (multiper == 0)
                        {
                            strError = "金额字符串格式错误 '" + strText + "'。除法运算中，除数不能为0";
                            return -1;
                        }

                        value = (decimal)((double)value / multiper);
                    }
                }

                PriceItem item = new PriceItem();
                item.Prefix = strPrefix.ToUpper();
                item.Postfix = strPostfix.ToUpper();
                item.Value = value;

                // 缺省货币为人民币
                if (item.Prefix == "" && item.Postfix == "")
                    item.Prefix = "CNY";
#endif
                CurrencyItem item = null;
                int nRet = ParseSinglePrice(strText,
            out item,
            out strError);
                if (nRet == -1)
                    return -1;

                items.Add(item);
            }

            // 汇总
            for (int i = 0; i < items.Count; i++)
            {
                CurrencyItem item = items[i];

                for (int j = i + 1; j < items.Count; j++)
                {
                    CurrencyItem current_item = items[j];
                    if (current_item.Prefix == item.Prefix
                        && current_item.Postfix == item.Postfix)
                    {
                        item.Value += current_item.Value;
                        items.RemoveAt(j);
                        j--;
                    }

                    /*
                else
                    break;
                     * */
                    // 这里是一个BUG。没有排序，并不知道后面还有没有重复的事项呢，不能break。2009/10/10 changed
                }
            }

            // 输出
            for (int i = 0; i < items.Count; i++)
            {
                CurrencyItem item = items[i];
                decimal value = item.Value;

                // 负号要放在最前面
                if (value < 0)
                    results.Add("-" + item.Prefix + (-value).ToString("#.##") + item.Postfix);
                else
                    results.Add(item.Prefix + value.ToString("#.##") + item.Postfix);
            }

            // 注: value.ToString("#.##") 采用的是四舍五入的方法
            return 0;
        }

    }

    /// <summary>
    /// 金额事项
    /// </summary>
    public class CurrencyItem
    {
        /// <summary>
        /// 前缀字符串
        /// </summary>
        public string Prefix = "";
        /// <summary>
        /// 后缀字符串
        /// </summary>
        public string Postfix = "";
        /// <summary>
        /// 数值
        /// </summary>
        public decimal Value = 0;

        public static CurrencyItem Parse(string strText)
        {
            string strError = "";

            string strPrefix = "";
            string strValue = "";
            string strPostfix = "";
            int nRet = PriceUtil.ParsePriceUnit(strText,
                out strPrefix,
                out strValue,
                out strPostfix,
                out strError);
            if (nRet == -1)
                throw new Exception(strError);

            decimal value = 0;
            try
            {
                value = Convert.ToDecimal(strValue);
            }
            catch
            {
                strError = "数字 '" + strValue + "' 格式不正确";
                throw new Exception(strError);
            }

            CurrencyItem item = new CurrencyItem();
            item.Prefix = strPrefix;
            item.Postfix = strPostfix;
            item.Value = value;

            return item;
        }

        public override string ToString()
        {
            return this.Prefix + this.Value.ToString("#.##") + this.Postfix;
            // 注: value.ToString("#.##") 采用的是四舍五入的方法
        }
    }
}
