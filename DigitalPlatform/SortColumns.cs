using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using DigitalPlatform.GUI;
using System.Diagnostics;

namespace DigitalPlatform
{
#if NO
    public enum ColumnSortStyle
    {
        LeftAlign = 0, // 左对齐字符串
        RightAlign = 1, // 右对齐字符串
        RecPath = 2,    // 记录路径。例如“中文图书/1”，以'/'为界，右边部分当作数字值排序。或者“localhost/中文图书/ctlno/1”
        LongRecPath = 3,    // 记录路径。例如“中文图书/1 @本地服务器”
        Extend = 4,    // 扩展的排序方式
    }
#endif

    // 栏目排序方式
    public class ColumnSortStyle
    {
        public string Name = "";
        public CompareEventHandler CompareFunc = null;

        public ColumnSortStyle(string strStyle)
        {
            this.Name = strStyle;
        }

        public static ColumnSortStyle None
        {
            get
            {
                return new ColumnSortStyle("");
            }
        }

        public static ColumnSortStyle LeftAlign
        {
            get
            {
                return new ColumnSortStyle("LeftAlign");
            }
        }

        public static ColumnSortStyle RightAlign
        {
            get
            {
                return new ColumnSortStyle("RightAlign");
            }
        }

        public static ColumnSortStyle RecPath
        {
            get
            {
                return new ColumnSortStyle("RecPath");
            }
        }

        public static ColumnSortStyle LongRecPath
        {
            get
            {
                return new ColumnSortStyle("LongRecPath");
            }
        }

        public static ColumnSortStyle IpAddress
        {
            get
            {
                return new ColumnSortStyle("IpAddress");
            }
        }

        public override bool Equals(System.Object obj)
        {
            ColumnSortStyle o = obj as ColumnSortStyle;
            if ((object)o == null)
                return false;

            if (this.Name == o.Name)
                return true;
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(ColumnSortStyle a, ColumnSortStyle b)
        {
            if (System.Object.ReferenceEquals(a, b))
                return true;
            if ((object)a == null || (object)b == null)
                return false;

            return a.Name == b.Name;
        }

        public static bool operator !=(ColumnSortStyle a, ColumnSortStyle b)
        {
            return !(a == b);
        }
    }

    public class Column
    {
        public int No = -1;
        public bool Asc = true;
        public ColumnSortStyle SortStyle = ColumnSortStyle.None;   // ColumnSortStyle.LeftAlign;
    }

    public class SortColumns : List<Column>
    {
        // 包装版本，兼容以前的格式
        // 如果针对同一列反复调用此函数，则排序方向会toggle
        // 所以，不能用本函数来设定固定的排序方向
        public void SetFirstColumn(int nFirstColumn,
            ListView.ColumnHeaderCollection columns)
        {
            SetFirstColumn(nFirstColumn,
                columns,
                true);
        }


        // parameters:
        //      bToggleDirection    ==true 若nFirstColumn本来已经是当前第一列，则更换其排序方向
        public void SetFirstColumn(int nFirstColumn,
            ListView.ColumnHeaderCollection columns,
            bool bToggleDirection)
        {
            int nIndex = -1;
            Column column = null;
            // 找到这个列号
            for (int i = 0; i < this.Count; i++)
            {
                column = this[i];
                if (column.No == nFirstColumn)
                {
                    nIndex = i;
                    break;
                }
            }

            ColumnSortStyle firstColumnStyle = ColumnSortStyle.None;   //  ColumnSortStyle.LeftAlign;

            // 自动设置右对齐风格
            // 2008/8/30 changed
            if (columns[nFirstColumn].TextAlign == HorizontalAlignment.Right)
                firstColumnStyle = ColumnSortStyle.RightAlign;

            // 本来已经是第一列，则更换排序方向
            if (nIndex == 0 && bToggleDirection == true)
            {
                if (column.Asc == true)
                    column.Asc = false;
                else
                    column.Asc = true;

                // 修改这一列的视觉
                ColumnHeader header = columns[column.No];

                SetHeaderText(header,
                    nIndex,
                    column);
                return;
            }

            if (nIndex != -1)
            {
                // 从数组中移走已经存在的值
                this.RemoveAt(nIndex);
            }
            else
            {
                column = new Column();
                column.No = nFirstColumn;
                column.Asc = true;  // 初始时为正向排序
                column.SortStyle = firstColumnStyle;    // 2007/12/20 new add
            }

            // 放到首部
            this.Insert(0, column);

            // 修改全部列的视觉
            RefreshColumnDisplay(columns);
        }

        // 修改排序数组，设置第一列，把原来的列号推后
        // parameters:
        //      bToggleDirection    ==true 若nFirstColumn本来已经是当前第一列，则更换其排序方向
        public void SetFirstColumn(int nFirstColumn,
            ColumnSortStyle firstColumnStyle,
            ListView.ColumnHeaderCollection columns,
            bool bToggleDirection)
        {
            int nIndex = -1;
            Column column = null;
            // 找到这个列号
            for (int i = 0; i < this.Count; i++)
            {
                column = this[i];
                if (column.No == nFirstColumn)
                {
                    nIndex = i;
                    break;
                }
            }

            // 本来已经是第一列，则更换排序方向
            if (nIndex == 0 && bToggleDirection == true)
            {
                if (column.Asc == true)
                    column.Asc = false;
                else
                    column.Asc = true;

                column.SortStyle = firstColumnStyle;    // 2008/11/30 new add

                // 修改这一列的视觉
                ColumnHeader header = columns[column.No];

                SetHeaderText(header,
                    nIndex,
                    column);
                return;
            }

            if (nIndex != -1)
            {
                // 从数组中移走已经存在的值
                this.RemoveAt(nIndex);
            }
            else
            {
                column = new Column();
                column.No = nFirstColumn;
                column.Asc = true;  // 初始时为正向排序
                column.SortStyle = firstColumnStyle;    // 2007/12/20 new add
            }

            // 放到首部
            this.Insert(0, column);

            // 修改全部列的视觉
            RefreshColumnDisplay(columns);
        }

        void DisplayColumnsText(ListView.ColumnHeaderCollection columns)
        {
            Debug.WriteLine("***");
            foreach (ColumnHeader column0 in columns)
            {
                Debug.WriteLine(column0.Text);
            }
            Debug.WriteLine("***");
        }

        // 修改全部列的视觉
        public void RefreshColumnDisplay(ListView.ColumnHeaderCollection columns)
        {
#if DEBUG
            DisplayColumnsText(columns);
#endif
            Column column = null;
            for (int i = 0; i < this.Count; i++)
            {
                column = this[i];

                ColumnHeader header = columns[column.No];

                SetHeaderText(header,
                    i,
                    column);
            }
#if DEBUG
            DisplayColumnsText(columns);
#endif
        }

        // 恢复没有任何排序标志的列标题文字内容
        public static void ClearColumnSortDisplay(ListView.ColumnHeaderCollection columns)
        {
            for (int i = 0; i < columns.Count; i++)
            {
                ColumnHeader header = columns[i];

                ColumnProperty prop = (ColumnProperty)header.Tag;
                if (prop != null)
                {
                    header.Text = prop.Title;
                }
            }
        }

        // 设置ColumnHeader文字
        public static void SetHeaderText(ColumnHeader header,
            int nSortNo,
            Column column)
        {
            ColumnProperty prop = (ColumnProperty)header.Tag;

            //string strOldText = "";
            if (prop != null)
            {
                // strOldText = (string)header.Tag;
            }
            else
            {
                // strOldText = header.Text;
                // 记忆下来
                prop = new ColumnProperty(header.Text);
                header.Tag = prop; 
            }

            string strNewText = 
                (column.Asc == true ? "▲" : "▼")
                + (nSortNo + 1).ToString()
                + " "
                + prop.Title;   //  strOldText;
            header.Text = strNewText;

            // 2008/11/30
            if (column.SortStyle == ColumnSortStyle.RightAlign)
            {
                if (header.TextAlign != HorizontalAlignment.Right)
                    header.TextAlign = HorizontalAlignment.Right;
            }
            else
            {
                if (header.TextAlign != HorizontalAlignment.Left)
                    header.TextAlign = HorizontalAlignment.Left;
            }
        }
    }

    // Implements the manual sorting of items by columns.
    public class SortColumnsComparer : IComparer
    {
        SortColumns SortColumns = new SortColumns();

        // 当一个 SortStyle 不是预知的类型的时候，使用这个 handler 排序
        public event CompareEventHandler EventCompare = null;

        public SortColumnsComparer()
        {
            Column column = new Column();
            column.No = 0;
            this.SortColumns.Add(column);
        }
        public SortColumnsComparer(SortColumns sortcolumns)
        {
            this.SortColumns = sortcolumns;
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
                strLeft = strRecPath; // 如果没有斜杠，则当作左边部分。这一点有何意义还需要仔细考察
                strRight = "";
                return;
            }

            strLeft = strRecPath.Substring(0, nRet);
            strRight = strRecPath.Substring(nRet + 1);
        }

        static void SplitLongRecPath(string strRecPath,
            out string strLeft,
            out string strRight,
            out string strServerName)
        {
            int nRet = 0;

            nRet = strRecPath.IndexOf("@");
            if (nRet != -1)
            {
                strServerName = strRecPath.Substring(nRet + 1).Trim();
                strRecPath = strRecPath.Substring(0, nRet).Trim();
            }
            else
                strServerName = "";
            
            nRet = strRecPath.LastIndexOf("/");
            if (nRet == -1)
            {
                strLeft = strRecPath;
                strRight = "";
                return;
            }

            strLeft = strRecPath.Substring(0, nRet);
            strRight = strRecPath.Substring(nRet + 1);
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

        // 比较两个 IP 地址
        public static int CompareIpAddress(string s1, string s2)
        {
            if (s1 == null)
                s1 = "";
            if (s2 == null)
                s2 = "";

            string[] parts1 = s1.Split(new char[] { '.', ':' });
            string[] parts2 = s2.Split(new char[] { '.', ':' });

            for (int i = 0; i < Math.Min(parts1.Length, parts2.Length); i++)
            {
                if (i >= parts1.Length)
                    break;
                if (i >= parts2.Length)
                    break;
                string n1 = parts1[i];
                string n2 = parts2[i];
                int nRet = RightAlignCompare(n1, n2);
                if (nRet != 0)
                    return nRet;
            }

            return (parts1.Length - parts2.Length);
        }

        public int Compare(object x, object y)
        {
            for (int i = 0; i < this.SortColumns.Count; i++)
            {
                Column column = this.SortColumns[i];

                string s1 = "";
                try
                {
                    s1 = ((ListViewItem)x).SubItems[column.No].Text;
                }
                catch
                {
                }
                string s2 = "";
                try
                {
                    s2 = ((ListViewItem)y).SubItems[column.No].Text;
                }
                catch
                {
                }

                int nRet = 0;

                if (column.SortStyle == null)
                {
                    nRet = String.Compare(s1, s2);
                }
                else if (column.SortStyle.CompareFunc != null)
                {
                    // 如果有排序函数，直接用排序函数
                    CompareEventArgs e = new CompareEventArgs();
                    e.Column = column;
                    e.SortColumnIndex = i;
                    // e.ColumnIndex = column.No;
                    e.String1 = s1;
                    e.String2 = s2;
                    column.SortStyle.CompareFunc(this, e);
                    nRet = e.Result;
                }
                else if (column.SortStyle == ColumnSortStyle.None 
                    || column.SortStyle == ColumnSortStyle.LeftAlign)
                {
                    nRet = String.Compare(s1, s2);
                }
                else if (column.SortStyle == ColumnSortStyle.RightAlign)
                {
#if NO
                    int nMaxLength = s1.Length;
                    if (s2.Length > nMaxLength)
                        nMaxLength = s2.Length;

                    s1 = s1.PadLeft(nMaxLength, ' ');
                    s2 = s2.PadLeft(nMaxLength, ' ');

                    nRet = String.Compare(s1, s2);
#endif
                    nRet = RightAlignCompare(s1, s2, ' ');
                }
                else if (column.SortStyle == ColumnSortStyle.RecPath)
                {
                    string strLeft1;
                    string strRight1;
                    string strLeft2;
                    string strRight2;
                    SplitRecPath(s1, out strLeft1, out strRight1);
                    SplitRecPath(s2, out strLeft2, out strRight2);

                    nRet = String.Compare(strLeft1, strLeft2);
                    if (nRet != 0)
                        goto END1;

#if NO
                    // 对记录号部分进行右对齐的比较
                    int nMaxLength = strRight1.Length;
                    if (strRight2.Length > nMaxLength)
                        nMaxLength = strRight2.Length;

                    strRight1 = strRight1.PadLeft(nMaxLength, ' ');
                    strRight2 = strRight2.PadLeft(nMaxLength, ' ');

                    nRet = String.Compare(strRight1, strRight2);
#endif
                    nRet = RightAlignCompare(strRight1, strRight2, ' ');
                }
                else if (column.SortStyle == ColumnSortStyle.LongRecPath)
                {
                    string strLeft1;
                    string strRight1;
                    string strServerName1;
                    string strLeft2;
                    string strRight2;
                    string strServerName2;

                    SplitLongRecPath(s1, out strLeft1, out strRight1, out strServerName1);
                    SplitLongRecPath(s2, out strLeft2, out strRight2, out strServerName2);

                    nRet = String.Compare(strServerName1, strServerName2);
                    if (nRet != 0)
                        goto END1;

                    nRet = String.Compare(strLeft1, strLeft2);
                    if (nRet != 0)
                        goto END1;

                    // 对记录号部分进行右对齐的比较
                    int nMaxLength = strRight1.Length;
                    if (strRight2.Length > nMaxLength)
                        nMaxLength = strRight2.Length;

                    strRight1 = strRight1.PadLeft(nMaxLength, ' ');
                    strRight2 = strRight2.PadLeft(nMaxLength, ' ');

                    nRet = String.Compare(strRight1, strRight2);

                }
                else if (column.SortStyle == ColumnSortStyle.IpAddress)
                {
                    nRet = CompareIpAddress(s1, s2);
                }
                else if (this.EventCompare != null)
                {
                    CompareEventArgs e = new CompareEventArgs();
                    e.Column = column;
                    e.SortColumnIndex = i;
                    e.String1 = s1;
                    e.String2 = s2;
                    this.EventCompare(this, e);
                    nRet = e.Result;
                }
                else
                {
                    // 不能识别的方式，按照左对齐处理
                    nRet = String.Compare(s1, s2);
                }

                END1:
                if (nRet != 0)
                {
                    if (column.Asc == true)
                        return nRet;
                    else
                        return -nRet;
                }
            }

            return 0;
        }
    }

    public delegate void CompareEventHandler(object sender,
        CompareEventArgs e);

    public class CompareEventArgs : EventArgs
    {
        public Column Column = null;    // 排序列
        public int SortColumnIndex = -1;    // 排序列 index。即 Column 在 SortColumns 数组中的下标
        public string String1 = "";
        public string String2 = "";
        public int Result = 0;  // [out]
    }
}
