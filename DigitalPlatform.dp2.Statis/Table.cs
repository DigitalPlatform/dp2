using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Diagnostics;

using DigitalPlatform.Text;

namespace DigitalPlatform.dp2.Statis
{
    // 行
    public class Line : IComparable
    {
        internal object[] cells = null;

        internal string strKey = null;

        public Line(int nColumnsHint)
        {
            if (nColumnsHint > 0)
                cells = new object[nColumnsHint];
        }

        public bool IsNull(int nIndex)
        {
            if (cells == null)
                return true;
            if (nIndex >= cells.Length)
                return true;

            // 2007/5/18
            if (cells[nIndex] == null)
                return true;

            return false;
        }

        // 得到一个单元值，自动转换为字符串
        // 如果一个单元不曾设置过数据，则返回strDefaultValue
        // parameters:
        //      nIndex  列索引。如果为-1，表示希望获取Entry列
        public string GetString(int nIndex,
            string strDefaultValue)
        {
            // 2007/10/26
            if (nIndex == -1)
                return this.Entry;

            if (cells == null)
                return strDefaultValue;
            if (nIndex >= cells.Length)
                return strDefaultValue;

            Object obj = cells[nIndex];
            if (obj != null)
            {
                if (obj is Int32)
                    return Convert.ToString((Int32)obj);
                if (obj is Int64)
                    return Convert.ToString((Int64)obj);
                if (obj is double)
                    return Convert.ToString((double)obj);
                if (obj is decimal)
                    return Convert.ToString((decimal)obj);

                if (obj is string)
                {
                    string strText = (string)obj;

                    // 2008/4/3
                    if (String.IsNullOrEmpty(strText) == true)
                        return strDefaultValue;
                    return strText;
                }
                throw (new Exception("不支持的数据类型 " + obj.GetType().ToString()));
            }

            return strDefaultValue;
        }

        // 得到一个单元值，自动转换为字符串
        // 如果一个单元不曾设置过数据，则返回""
        // parameters:
        //      nIndex  列索引。如果为-1，表示希望获取Entry列
        public string GetString(int nIndex)
        {
           /*
            if (cells == null)
                return "";
            if (nIndex >= cells.Length)
                return "";

            Object obj = cells[nIndex];
            if (obj != null) 
            {
                if ((obj is Int32) || (obj is Int64))
                    return Convert.ToString((Int64)obj);
                if (obj is string)
                    return (string)obj;
                throw(new Exception("不支持的数据类型"));
            }

            return "";
            */
            return GetString(nIndex, "");
        }

        // 得到100倍整数金额值的字符串
        // Exception:
        //		Exception
        //		???
        public string GetPriceString(int nIndex)
        {
            Int64 v = GetInt64(nIndex);

            return StatisUtil.Int64ToPrice(v);
        }

        // 得到一个单元值，自动转换为Int64类型
        // 如果一个单元不曾设置过数据，则返回0
        public Int64 GetInt64(int nIndex)
        {
            if (cells == null)
                return (Int64)0;
            if (nIndex >= cells.Length)
                return (Int64)0;

            Object obj = cells[nIndex];
            if (obj != null)
            {
                if ((obj is Int32))
                    return (Int32)obj;
                if ((obj is Int64))
                    return (Int64)obj;
                if (obj is decimal)
                    return Convert.ToInt64((decimal)obj);

                if (obj is string)
                    return Convert.ToInt64((string)obj);

                throw (new Exception("不支持的数据类型 " + obj.GetType().ToString()));
            }

            return (Int64)0;
        }

        // 得到一个单元值，自动转换为double类型
        // 如果一个单元不曾设置过数据，则返回0
        public double GetDouble(int nIndex)
        {
            if (cells == null)
                return (double)0;
            if (nIndex >= cells.Length)
                return (double)0;

            Object obj = cells[nIndex];
            if (obj != null)
            {
                if ((obj is Int32) || (obj is Int64))
                    return Convert.ToDouble(obj);   // 注意，用(double)直接转换是不行的
                else if (obj is double)
                    return (double)obj;
                else if (obj is decimal)
                    return Convert.ToDouble((decimal)obj);
                else if (obj is string)
                {
                    string strText = (string)obj;
                    try
                    {
                        return Convert.ToDouble(strText);   // BUG!!! 忘了return
                    }
                    catch (Exception ex)
                    {
                        // 2008/4/24
                        throw new Exception("字符串值 '" + strText + "' 在转换为double类型时发生错误: " + ex.Message);
                    }
                }
                else
                    throw (new Exception("不支持的数据类型 " + obj.GetType().ToString()));
            }

            return (double)0;
        }

        public object GetObject(int nIndex)
        {
            if (cells == null)
                return null;
            if (nIndex >= cells.Length)
                return null;

            return cells[nIndex];
        }

        // 2014/6/8
        public object[] GetAllCells()
        {
            object [] result = new object [cells.Length + 1];
            result[0] = this.strKey;
            Array.Copy(cells, 0, result, 1, cells.Length);
            return result;
        }

        // 2008/11/29
        // 得到一个单元值，自动转换为decimal类型
        // 如果一个单元不曾设置过数据，则返回0
        public decimal GetDecimal(int nIndex)
        {
            if (cells == null)
                return (decimal)0;
            if (nIndex >= cells.Length)
                return (decimal)0;

            Object obj = cells[nIndex];
            if (obj != null)
            {
                if ((obj is Int32) || (obj is Int64))
                    return Convert.ToDecimal(obj);   // 注意，用(decimal)直接转换是不行的
                else if (obj is double)
                    return Convert.ToDecimal((double)obj);
                else if (obj is decimal)
                    return (decimal)obj;
                else if (obj is string)
                {
                    string strText = (string)obj;
                    try
                    {
                       return Convert.ToDecimal(strText);
                    }
                    catch (Exception ex)
                    {
                        // 2008/4/24
                        throw new Exception("字符串值 '" + strText + "' 在转换为decimal类型时发生错误: " + ex.Message);
                    }
                }
                else 
                    throw (new Exception("不支持的数据类型 " + obj.GetType().ToString()));
            }

            return (decimal)0;
        }

        // 得到一个单元的Object类型值。
        // 如果一个单元不曾设置过数据，则返回null
        // parameters:
        //      nIndex  列索引。如果为-1，表示希望获取Entry列
        public object this[int nIndex]
        {
            get
            {
                // 2007/10/26
                if (nIndex == -1)
                    return this.Entry;

                if (cells == null)
                    return null;
                if (nIndex >= cells.Length)
                    return null;
                return cells[nIndex];
            }
        }

        public int Count
        {
            get
            {
                if (cells == null)
                    return 0;
                return cells.Length;
            }
        }

        public int CompareTo(object obj)
        {
            // 相当于this - obj的效果

            if (obj is Line)
            {
                Line line = (Line)obj;

                return String.Compare(this.strKey, line.strKey, true);
            }

            throw new ArgumentException("object is not a Line");
        }

        // 行标题字符串
        public string Entry
        {
            get
            {
                return strKey;
            }
            set
            {
                // TODO: 更新hashtable的item key?
                strKey = value;
            }
        }


        // 确保列空间足够
        void EnsureCells(int nColumn)
        {
            if (cells == null)
            {
                cells = new object[nColumn + 1];
            }
            else if (cells.Length <= nColumn)
            {
                object[] temp = new object[nColumn + 1];
                // 复制
                Array.Copy(cells, 0, temp, 0, cells.Length);
                cells = temp;
            }
        }

        // 为一列设置一个值
        public void SetValue(int nColumn,
            object value)
        {
            EnsureCells(nColumn);

            cells[nColumn] = value;
        }

        // 为一列的整数值增量
        // 本方法只能应用在Int32或Int64值类型的列上，否则会抛出异常
        // parameters:
        //		createValue	如果列单元不存在，则采用此值初始设置
        //		incValue	如果列单元已经存在，则采用此值加上原来的值，修改回
        public void IncValue(
            int nColumn,
            Int64 createValue,
            Int64 incValue)
        {
            EnsureCells(nColumn);

            if (cells[nColumn] == null)
            {
                cells[nColumn] = createValue;
            }
            else
            {
                object oldvalue = cells[nColumn];
                if (oldvalue is Int32)
                {
                    Int64 v = (Int32)oldvalue;
                    v += incValue;
                    cells[nColumn] = v;
                } 
                else if (oldvalue is Int64)
                {
                    Int64 v = (Int64)oldvalue;
                    v += incValue;
                    cells[nColumn] = v;
                }
                else
                {
                    throw (new Exception("列" + Convert.ToString(nColumn) + "类型必须为Int32或Int64"));
                }
            }
        }

        // 为一列的整数值增量
        // 本方法只能应用在Int32或Int64值类型的列上，否则会抛出异常
        // parameters:
        //		createValue	如果列单元不存在，则采用此值初始设置
        //		incValue	如果列单元已经存在，则采用此值加上原来的值，修改回
        public void IncValue(
            int nColumn,
            double createValue,
            double incValue)
        {
            EnsureCells(nColumn);

            if (cells[nColumn] == null)
            {
                cells[nColumn] = createValue;
            }
            else
            {
                object oldvalue = cells[nColumn];
                if ((oldvalue is Int64)
                    || (oldvalue is Int32))
                {
                    double v = Convert.ToDouble(oldvalue);
                    v += incValue;
                    cells[nColumn] = v;
                }
                else if (oldvalue is double)
                {
                    double v = (double)oldvalue;
                    v += incValue;
                    cells[nColumn] = v;
                }
                else if (oldvalue is decimal)
                {
                    double v = Convert.ToDouble((decimal)oldvalue);
                    v += incValue;
                    cells[nColumn] = v;
                }
                else
                {
                    throw (new Exception("列" + Convert.ToString(nColumn) + "类型必须为Int32或Int64或double"));
                }
            }
        }

        // 2008/11/29
        // 为一列的整数值增量
        // 本方法只能应用在Int32或Int64值类型的列上，否则会抛出异常
        // parameters:
        //		createValue	如果列单元不存在，则采用此值初始设置
        //		incValue	如果列单元已经存在，则采用此值加上原来的值，修改回
        public void IncValue(
            int nColumn,
            decimal createValue,
            decimal incValue)
        {
            EnsureCells(nColumn);

            if (cells[nColumn] == null)
            {
                cells[nColumn] = createValue;
            }
            else
            {
                object oldvalue = cells[nColumn];
                if ((oldvalue is Int64)
                    || (oldvalue is Int32))
                {
                    decimal v = Convert.ToDecimal(oldvalue);
                    v += incValue;
                    cells[nColumn] = v;
                }
                else if (oldvalue is double)
                {
                    decimal v = Convert.ToDecimal((double)oldvalue);
                    v += incValue;
                    cells[nColumn] = v;
                }
                else if (oldvalue is decimal)
                {
                    decimal v = (decimal)oldvalue;
                    v += incValue;
                    cells[nColumn] = v;
                }
                else
                {
                    throw (new Exception("列" + Convert.ToString(nColumn) + "类型必须为Int32或Int64或decimal"));
                }
            }
        }

        // 为一列的字符串值增量
        // 本方法只能应用在string值类型的列上，否则会抛出异常
        // parameters:
        //		createValue	如果列单元不存在，则采用此值初始设置
        //		incValue	如果列单元已经存在，则采用原来的值后面追加此值，修改回
        public void IncValue(
            int nColumn,
            string createValue,
            string incValue)
        {
            EnsureCells(nColumn);

            if (cells[nColumn] == null)
            {
                cells[nColumn] = createValue;
            }
            else
            {
                object oldvalue = cells[nColumn];
                if (oldvalue is string)
                {
                    string v = (string)oldvalue;
                    v += incValue;
                    cells[nColumn] = v;
                }
                else
                {
                    throw (new Exception("列" + Convert.ToString(nColumn) + "类型必须为string"));
                }
            }
        }

        // 为一列的字符串值增量
        // 本方法只能应用在string值类型的列上，否则会抛出异常
        // parameters:
        //		createValue	如果列单元不存在，则采用此值初始设置
        //		incValue	如果列单元已经存在，则采用原来的值后面追加此值，修改回
        public void IncCurrency(
            int nColumn,
            string createValue,
            string incValue)
        {
            EnsureCells(nColumn);

            if (cells[nColumn] == null)
            {
                cells[nColumn] = createValue;
            }
            else
            {
                object oldvalue = cells[nColumn];
                if (oldvalue is string)
                {
                    string v = (string)oldvalue;

                   // 连接两个价格字符串
                    v = PriceUtil.JoinPriceString(v,
                        incValue);

                    string strSumPrices = "";
                    string strError = "";
                            // 将形如"-123.4+10.55-20.3"的价格字符串归并汇总
                    int nRet = PriceUtil.SumPrices(v,
            out strSumPrices,
            out strError);
                    if (nRet == 0)
                        v = strSumPrices;
                    if (nRet == -1)
                        throw new Exception("汇总金额字符串 '"+v+"' 时出错：" + strError);

                    // v += incValue;
                    cells[nColumn] = v;
                }
                else
                {
                    throw (new Exception("列" + Convert.ToString(nColumn) + "类型必须为string"));
                }
            }
        }

    }

    /// <summary>
    /// 帮助归类统计的2维内存表格
    /// </summary>
    public class Table
    {
        Hashtable lines = new Hashtable();

        // ArrayList sorted = null;
        List<Line> sorted = null;

        int m_nColumnsHint = 0;	// 暗示表的列数

        public int HintColumns
        {
            get
            {
                return this.m_nColumnsHint;
            }
        }

        public int GetMaxColumnCount()
        {
            int nResult = 0;
            foreach(string key in this.lines.Keys)
            {
                Line line = (Line)this.lines[key];
                if (line.Count > nResult)
                    nResult = line.Count;
            }

            return nResult;
        }

        public Table(int nColumnsHint)
        {
            m_nColumnsHint = nColumnsHint;
        }

        // 2013/6/14
        public ICollection Keys
        {
            get
            {
                return this.lines.Keys;
            }
        }

        // 写入一个单元的值
        public void SetValue(string strEntry,
            int nColumn,
            object value)
        {
            // 检查line事项是否存在
            Line line = EnsureLine(strEntry, m_nColumnsHint);

            Debug.Assert(line != null, "line在这里应该!=null");

            line.SetValue(nColumn, value);
        }

        public Line SearchLine(string strEntry)
        {
            return (Line)lines[strEntry];
        }

        public int Count
        {
            get
            {
                return lines.Count;
            }
        }

        public object SearchValue(string strEntry,
            int nColumn)
        {
            Line line = (Line)lines[strEntry];
            if (line == null)
                return null;
            if (line.cells == null)
                return null;
            if (line.cells.Length <= nColumn)
                return null;
            return line.cells[nColumn];
        }

        public void RemoveLine(string strEntry)
        {
            lines.Remove(strEntry);
        }

        // 删除从nStart开始到末尾的行
        public void RemoveLines(int nStart)
        {
            RemoveLines(nStart, lines.Count);   // ?? lines.Count - nStart
        }

        // 删除指定范围的行
        public void RemoveLines(int nStart,
            int nCount)
        {
            int i = 0;
            List<string> keys = new List<string>();
            foreach (string strKey in lines.Keys)
            {
                if (i >= nStart && i < nStart + nCount)
                    keys.Add(strKey);

                if (i >= nStart + nCount)
                    break;

                i++;
            }

            foreach (string strKey in keys)
            {
                lines.Remove(strKey);
            }
        }

        // 得到行对象。如果不存在，则临时创建一个
        public Line EnsureLine(string strEntry,
            int nColumnsHint = -1)
        {
            // 检查line事项是否存在
            Line line = (Line)lines[strEntry];

            if (line == null)
            {
                if (nColumnsHint == -1)
                    nColumnsHint = this.m_nColumnsHint;

                line = new Line(nColumnsHint);
                line.strKey = strEntry;

                lines.Add(strEntry, line);
            }

            Debug.Assert(line != null, "line在这里应该!=null");


            return line;
        }

        public Line this[string strEntry]
        {
            get
            {
                return (Line)lines[strEntry];
            }
        }

        // 必须排序后才能用
        public Line this[int nIndex]
        {
            get
            {
                // TODO: 当表内加入了新的行或者删除了行以后，sorted应该恢复null
                if (sorted == null)
                {
                    throw (new Exception("使用[int nIndex]索引器之前，必须先用Sort()方法排序..."));
                }
                return (Line)sorted[nIndex];
            }
        }

        // 包装后的版本 2015/4/2
        public void IncValue(string strEntry,
            int nColumn,
            Int64 value)
        {
            IncValue(strEntry,
                nColumn,
                value,
                value);
        }

        // 累加一个单元的值
        // createValue	如果指定的单元不存在，则以此值创建新单元
        // incValue	如果指定的单元已经存在，则在原值上递增此值。
        public void IncValue(string strEntry,
            int nColumn,
            Int64 createValue,
            Int64 incValue)
        {
            // 检查line事项是否存在
            Line line = EnsureLine(strEntry, m_nColumnsHint);

            Debug.Assert(line != null, "line在这里应该!=null");

            line.IncValue(nColumn, createValue, incValue);
        }

        // 累加一个单元的值
        // createValue	如果指定的单元不存在，则以此值创建新单元
        // incValue	如果指定的单元已经存在，则在原值上递增此值。
        public void IncValue(string strEntry,
            int nColumn,
            double createValue,
            double incValue)
        {
            // 检查line事项是否存在
            Line line = EnsureLine(strEntry, m_nColumnsHint);

            Debug.Assert(line != null, "line在这里应该!=null");

            line.IncValue(nColumn, createValue, incValue);
        }

        // 2008/12/1
        // 累加一个单元的值
        // createValue	如果指定的单元不存在，则以此值创建新单元
        // incValue	如果指定的单元已经存在，则在原值上递增此值。
        public void IncValue(string strEntry,
            int nColumn,
            decimal createValue,
            decimal incValue)
        {
            // 检查line事项是否存在
            Line line = EnsureLine(strEntry, m_nColumnsHint);

            Debug.Assert(line != null, "line在这里应该!=null");

            line.IncValue(nColumn, createValue, incValue);
        }


        // 累加一个单元的值
        // createValue	如果指定的单元不存在，则以此值创建新单元
        // incValue	如果指定的单元已经存在，则在原值后追加此值。
        public void IncValue(string strEntry,
            int nColumn,
            string createValue,
            string incValue)
        {
            // 检查line事项是否存在
            Line line = EnsureLine(strEntry, m_nColumnsHint);

            Debug.Assert(line != null, "line在这里应该!=null");

            line.IncValue(nColumn, createValue, incValue);
        }

        public void IncCurrency(string strEntry,
    int nColumn,
    string createValue,
    string incValue)
        {
            // 检查line事项是否存在
            Line line = EnsureLine(strEntry, m_nColumnsHint);

            Debug.Assert(line != null, "line在这里应该!=null");

            line.IncCurrency(nColumn, createValue, incValue);
        }


        // 按照行标题，升序排序
        public void Sort()
        {
            // 把lines中所有对象指针复制到ArrayList中
            if (sorted == null)
            {
                // sorted = new ArrayList();
                sorted = new List<Line>();
            }
            else
                sorted.Clear();

            //sorted.AddRange(lines);
            foreach (DictionaryEntry item in lines)
            {
                sorted.Add((Line)item.Value);
            }

            sorted.Sort();
        }

#if NO
        // 2009/9/30
        // 自定义规则排序
        public void Sort(IComparer comparer)
        {
            // 把lines中所有对象指针复制到ArrayList中
            if (sorted == null)
            {
                // sorted = new ArrayList();
                sorted = new List<Line>();
            }
            else
                sorted.Clear();

            foreach (DictionaryEntry item in lines)
            {
                sorted.Add((Line)item.Value);
            }

            sorted.Sort(comparer);
        }
#endif

        public void Sort(IComparer<Line> comparer)
        {
            // 把lines中所有对象指针复制到ArrayList中
            if (sorted == null)
            {
                // sorted = new ArrayList();
                sorted = new List<Line>();
            }
            else
                sorted.Clear();

            foreach (DictionaryEntry item in lines)
            {
                sorted.Add((Line)item.Value);
            }

            sorted.Sort(comparer);
        }

        // 按照复杂要求排序
        // strColumnList	逗号分割的列号字符串，排序将按照这个优先级进行
        public void Sort(string strColumnList)
        {
            // 把lines中所有对象指针复制到ArrayList中
            if (sorted == null)
            {
                // sorted = new ArrayList();
                sorted = new List<Line>();
            }
            else
                sorted.Clear();

            //sorted.AddRange(lines);
            foreach (DictionaryEntry item in lines)
            {
                sorted.Add((Line)item.Value);
            }

            sorted.Sort(new ComparerClass(strColumnList));
        }

        // 得到随机的第一行
        public Line FirstHashLine()
        {
            foreach (DictionaryEntry item in lines)
            {
                return (Line)item.Value;
            }

            return null;
        }

        #region IComparer类ComparerClass，用于排序

        public class ComparerClass : IComparer<Line>
        {
            SortColumnCollection sortstyle = null;

            public ComparerClass(string strColumnList)
            {
                sortstyle = new SortColumnCollection();
                sortstyle.Build(strColumnList);
            }

            // Calls CaseInsensitiveComparer.Compare with the parameters reversed.
            int IComparer<Line>.Compare(Line line1, Line line2)
            {
                /*
                if (!(x is Line))
                    throw new ArgumentException("object x is not a Line");
                if (!(y is Line))
                    throw new ArgumentException("object y is not a Line");


                Line line1 = (Line)x;
                Line line2 = (Line)y;
                */
                if (sortstyle == null)
                    throw (new Exception("sortstyle尚未创建"));

                for (int i = 0; i < this.sortstyle.Count; i++)
                {
                    SortColumn column = (SortColumn)this.sortstyle[i];

                    int nRet = 0;

                    // 取行标题进行比较
                    if (column.nColumnNumber == -1)
                    {
                        nRet = column.CompareString(line1.strKey, line2.strKey);
                        if (nRet != 0)
                            return nRet;

#if NO
                        if (column.dataType == DataType.Auto
                            || column.dataType == DataType.Number)
                        {
                            if (line1.strKey.Length == line2.strKey.Length)
                            {
                                nRet = String.Compare(line1.strKey, line2.strKey, column.bIgnorCase);
                            }
                            else
                            {
                                // 右对齐?
                                string s1 = line1.strKey;
                                string s2 = line2.strKey;

                                if (s1.Length < s2.Length)
                                {
                                    s1 = s1.PadLeft(s2.Length, ' ');
                                }
                                else if (s1.Length > s2.Length)
                                {
                                    s2 = s2.PadLeft(s1.Length, ' ');
                                }
                                nRet = String.Compare(s1, s2, column.bIgnorCase);

                            }

                            if (column.bAsc == false)
                                nRet = nRet * (-1);
                            if (nRet != 0)
                                return nRet;

                        }
                        else
                        {
                            nRet = String.Compare(line1.strKey, line2.strKey, column.bIgnorCase);
                            if (column.bAsc == false)
                                nRet = nRet * (-1);
                            if (nRet != 0)
                                return nRet;
                        }

#endif
                    }
                    else
                    {
                        object o1 = null;

                        if (column.nColumnNumber < line1.cells.Length)
                            o1 = line1.cells[column.nColumnNumber];

                        object o2 = null;

                        if (column.nColumnNumber < line2.cells.Length)
                            o2 = line2.cells[column.nColumnNumber];

                        nRet = column.CompareObject(o1, o2);
                        if (nRet != 0)
                            return nRet;
#if NO
                        if (column.dataType == DataType.Auto
                            || column.dataType == DataType.Number)
                        {
                            Int64 n1 = 0;
                            Int64 n2 = 0;
                            string s1 = null;
                            string s2 = null;
                            bool bException = false;

                            if ((o1 is Int32)
                                || (o1 is Int64))
                                n1 = (Int64)o1;
                            else if (o1 is string)
                            {
                                try
                                {
                                    n1 = Convert.ToInt64((string)o1);	// 可能抛出异常
                                }
                                catch
                                {
                                    s1 = (string)o1;
                                    bException = true;
                                }
                            }


                            if ((o2 is Int32)
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
                                }
                            }

                            if (bException == true)
                            {
                                // 对齐
                                int nMaxLength = Math.Max(s1.Length, s2.Length);
                                s2 = s2.PadLeft(nMaxLength, '0');
                                s1 = s1.PadLeft(nMaxLength, '0');

                                nRet = String.Compare(s1, s2, column.bIgnorCase);
                                if (column.bAsc == false)
                                    nRet = nRet * (-1);
                                if (nRet != 0)
                                    return nRet;
                            }
                            else
                            {

                                Int64 n64Ret = n1 - n2;
                                if (column.bAsc == false)
                                    n64Ret = n64Ret * (-1);
                                if (n64Ret != 0)
                                    return (int)n64Ret;
                            }
                        }
                        else if (column.dataType == DataType.String)
                        {
                            string s1 = "";
                            string s2 = "";


                            if ((o1 is Int32)
                                || (o1 is Int64))
                                s1 = Convert.ToString((Int64)o1);
                            else if (o1 is string)
                                s1 = (string)o1;


                            if ((o2 is Int32)
                                || (o2 is Int64))
                                s2 = Convert.ToString((Int64)o2);
                            else if (o2 is string)
                                s2 = (string)o2;


                            nRet = String.Compare(s1, s2, column.bIgnorCase);
                            if (column.bAsc == false)
                                nRet = nRet * (-1);
                            if (nRet != 0)
                                return nRet;
                        }

#endif

                    } // end of else

                } // end of loop

                return 0;
            }
        }

        #endregion


    }
}
