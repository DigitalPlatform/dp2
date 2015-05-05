using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using DigitalPlatform.Text;

namespace DigitalPlatform.dp2.Statis
{
    // 排序有关的类: SortColumn SortColumnCollection

    // 列数据类型
    public enum DataType
    {
        Auto = 0,
        String = 1,
        Number = 2,
        Price = 3,// 100倍金额整数
        PriceDouble = 4,    // double，用来表示金额。也就是最多只有两位小数部分 -- 注意，有累计误差问题，以后建议废止
        PriceDecimal = 5,   // decimal，用来表示金额。
        Currency = 6,   // 货币字符串。带有货币单位的字符串，可能是若干个子串连接起来的
        RecPath = 7,    // 记录路径，短的形式。例如“中文图书/1”
    }

    // 一个列的排序属性
    public class SortColumn
    {
        internal bool bAsc = true;	// 是否为升序
        internal DataType dataType = DataType.Auto;
        internal bool bIgnorCase = true;
        internal int nColumnNumber = -1;
        internal bool bPatronBarcode = false;   // 是否读者证条码号。如果是，排序的时候要先按照位数排

        public string ToStyleString()
        {
            StringBuilder text = new StringBuilder();
            if (this.bAsc == true)
                text.Append("a");
            else
                text.Append("d");

            if (this.dataType == DataType.String)
                text.Append("s");
            if (this.dataType == DataType.Number)
                text.Append("n");
            if (this.dataType == DataType.RecPath)
                text.Append("P");

            if (this.bPatronBarcode == true)
                text.Append("p");

            if (this.bIgnorCase == false)
                text.Append("c");

            return text.ToString();
        }

        public static void GetStyle(string strStyle,
            out bool bAsc,
            out bool bIgnoreCase,
            out bool bPatronBarcode,
            out DataType dataType)
        {
            bAsc = true;
            dataType = DataType.Auto;
            bIgnoreCase = true;
            bPatronBarcode = false;

            for (int i = 0; i < strStyle.Length; i++)
            {
                if (strStyle[i] == 'a')
                {
                    bAsc = true;
                }
                else if (strStyle[i] == 'd')
                {
                    bAsc = false;
                }
                else if (strStyle[i] == 's')
                {
                    dataType = DataType.String;
                }
                else if (strStyle[i] == 'n')
                {
                    dataType = DataType.Number;
                }
                else if (strStyle[i] == 'P')
                {
                    dataType = DataType.RecPath;
                }
                else if (strStyle[i] == 'p')
                {
                    bPatronBarcode = true;
                }
                else if (strStyle[i] == 'c') // 大小写敏感
                {
                    bIgnoreCase = false;	// 但是注意缺省情况为大小写不敏感
                }

            }
        }

        public int CompareObject(object o1, object o2)
        {
            int nRet = 0;
            Int64 n1 = 0;
            Int64 n2 = 0;
            string s1 = null;
            string s2 = null;
            bool bException = false;

            if ((o1 is Int32))  // 2013/12/19
                n1 = (Int32)o1;
            else if ((o1 is Int32)
                || (o1 is Int64))
                n1 = (Int64)o1;
            else if (o1 is string)
            {
                // 2014/1/12
                bool bRet = Int64.TryParse((string)o1, out n1);
                if (bRet == false)
                {
                    s1 = (string)o1;
                    bException = true;
                }
#if NO
                try
                {
                    n1 = Convert.ToInt64((string)o1);	// 可能抛出异常
                }
                catch
                {
                    s1 = (string)o1;
                    bException = true;
                }
#endif
            }


            if ((o2 is Int32))  // 2013/12/19
                n2 = (Int32)o2;
            else if ((o2 is Int32)
                || (o2 is Int64))
            {
                n2 = (Int64)o2;
                if (bException == true)
                    s2 = Convert.ToString(n2);
            }
            else if (o2 is string)
            {
                if (bException == true)
                    s2 = (string)o2;
                else
                {
                    // 2014/1/12
                    bool bRet = Int64.TryParse((string)o2, out n2);
                    if (bRet == false)
                    {
                        s2 = (string)o2;
                        bException = true;
                        s1 = Convert.ToString(n1);
                    }

#if NO
                    try
                    {
                        n2 = Convert.ToInt64((string)o2);
                    }
                    catch
                    {
                        s2 = (string)o2;
                        bException = true;
                        s1 = Convert.ToString(n1);
                    }
#endif
                }
            }

            if (bException == true)
            {
                return CompareString(s1, s2);
            }
            else
            {
                Int64 n64Ret = n1 - n2;
                if (this.bAsc == false)
                    n64Ret = n64Ret * (-1);
                if (n64Ret != 0)
                    return (int)n64Ret;
            }

            return 0;
        }

        public int CompareString(string s1, string s2)
        {
            int nRet = 0;

            // 证条码号先比较位数
            if (this.bPatronBarcode == true
                && s1 != null & s2 != null
                && s1.Length != s2.Length)
            {
                nRet = s1.Length - s2.Length;
                goto END1;
            }

            if (this.dataType == DataType.Auto
                || this.dataType == DataType.Number)
            {
                if (s1.Length == s1.Length)
                {
                    nRet = String.Compare(s1, s2, this.bIgnorCase);
                }
                else
                {
                    // 右对齐?
                    if (s1.Length < s2.Length)
                    {
                        s1 = s1.PadLeft(s2.Length, ' ');
                    }
                    else if (s1.Length > s2.Length)
                    {
                        s2 = s2.PadLeft(s1.Length, ' ');
                    }
                    nRet = String.Compare(s1, s2, this.bIgnorCase);
                }
            }
            else if (this.dataType == DataType.String)
            {
                nRet = String.Compare(s1, s2, this.bIgnorCase);
            }
            else if (this.dataType == DataType.RecPath)
            {
                nRet = StringUtil.CompareRecPath(s1, s2);
            }
            else
            {
                nRet = String.Compare(s1, s2, this.bIgnorCase);
            }

            END1:
            if (this.bAsc == false)
                nRet = nRet * (-1);
            if (nRet != 0)
                return nRet;

            return 0;
        }
    }

    // 排序属性数组
    public class SortColumnCollection : List<SortColumn>
    {
        // 将通用的列表示法，转化为 table 专用的列表示法
        // 前者是从 0 开始计算列号；后者是从 -1 开始计算列号
        public static string NormalToTable(string strSource)
        {
            SortColumnCollection temp = new SortColumnCollection();
            temp.Build(strSource);

            StringBuilder text = new StringBuilder(4096);
            foreach (SortColumn column in temp)
            {
                if (text.Length > 0)
                    text.Append(",");
                text.Append((column.nColumnNumber -1).ToString() + ":");
                text.Append(column.ToStyleString());
            }

            return text.ToString();
        }

        // 将table 专用的列表示法， 转化为通用的列表示法
        // 前者是从 -1 开始计算列号；后者是从 0 开始计算列号
        public static string TableToNormal(string strSource)
        {
            SortColumnCollection temp = new SortColumnCollection();
            temp.Build(strSource);

            StringBuilder text = new StringBuilder(4096);
            foreach (SortColumn column in temp)
            {
                if (text.Length > 0)
                    text.Append(",");
                text.Append((column.nColumnNumber + 1).ToString() + ":");
                text.Append(column.ToStyleString());
            }

            return text.ToString();
        }

        // 0:a,1:p
        // -1 表示 Entry 列
        public void Build(string strColumnList)
        {
            string[] aName = strColumnList.Split(new Char[] { ',' });

            this.Clear();

            for (int i = 0; i < aName.Length; i++)
            {
                string strPart = aName[i];
                if (strPart == "")
                    continue;
                SortColumn column = new SortColumn();
                this.Add(column);

                int nRet = strPart.IndexOf(":");
                if (nRet == -1)
                {
                    column.nColumnNumber = Convert.ToInt32(strPart);
                }
                else
                {
                    column.nColumnNumber = Convert.ToInt32(strPart.Substring(0, nRet).Trim());

                    strPart = strPart.Substring(nRet + 1).Trim();

                    bool bAsc;
                    DataType dataType;
                    bool bIgnoreCase;
                    bool bPatronBarcode = false;
                    SortColumn.GetStyle(strPart,
                        out bAsc,
                        out bIgnoreCase,
                        out bPatronBarcode,
                        out dataType);
                    column.bAsc = bAsc;
                    column.bIgnorCase = bIgnoreCase;
                    column.dataType = dataType;
                    column.bPatronBarcode = bPatronBarcode;
                }
            }

        }

    }

}
