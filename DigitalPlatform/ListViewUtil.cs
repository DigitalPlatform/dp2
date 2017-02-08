using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using System.Windows.Forms;
using System.Drawing;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace DigitalPlatform.GUI
{
    public class ListViewUtil
    {
        public static void BeginSelectItem(Control control, ListViewItem item)
        {
            control.BeginInvoke(new Action<ListViewItem>(
                (o) =>
                {
                    o.Selected = true;
                    o.EnsureVisible();
                }), item);
        }

        public static int GetColumnHeaderHeight(ListView list)
        {
            RECT rc = new RECT();
            IntPtr hwnd = API.SendMessage(list.Handle, API.LVM_GETHEADER, 0, 0);
            if (hwnd == null)
                return -1;

            if (API.GetWindowRect(new HandleRef(null, hwnd), out rc))
            {
                return rc.bottom - rc.top;
            }

            return -1;
        }

        // 2012/5/9
        // 创建事项名列表
        public static string GetItemNameList(ListView.SelectedListViewItemCollection items,
            string strSep = ",")
        {
            StringBuilder strItemNameList = new StringBuilder(4096);
            foreach (ListViewItem item in items)
            {
                if (strItemNameList.Length > 0)
                    strItemNameList.Append(strSep);
                strItemNameList.Append(item.Text);
            }

            return strItemNameList.ToString();
        }

        // 2012/5/9
        // 创建事项名列表
        public static string GetItemNameList(ListView list,
            string strSep = ",")
        {
            StringBuilder strItemNameList = new StringBuilder(4096);
            foreach (ListViewItem item in list.SelectedItems)
            {
                if (strItemNameList.Length > 0)
                    strItemNameList.Append(strSep);
                strItemNameList.Append(item.Text);
            }

            return strItemNameList.ToString();
        }

        // 上下移动事项的菜单是否应被使能
        public static bool MoveItemEnabled(
            ListView list,
            bool bUp)
        {
            if (list.SelectedItems.Count == 0)
                return false;
            int index = list.SelectedIndices[0];
            if (bUp == true)
            {
                if (index == 0)
                    return false;
                return true;
            }
            else
            {
                if (index >= list.Items.Count - 1)
                    return false;
                return true;
            }
        }

        public static bool MoveSelectedUpDown(
            ListView list,
            bool bUp)
        {
            if (list.SelectedItems.Count == 0)
                return false;

            int index = list.SelectedIndices[0];

            if (bUp)
            {
                index--;
                if (index < 0)
                    return false;
            }
            else
            {
                index++;
                if (index >= list.Items.Count)
                    return false;
            }

            list.SelectedItems.Clear();
            list.Items[index].Selected = true;
            return true;
        }

        // parameters:
        //      indices 返回移动涉及到的下标位置。第一个元素是移动前的位置，第二个元素是移动后的位置
        public static int MoveItemUpDown(
            ListView list,
            bool bUp,
            out List<int> indices,
            out string strError)
        {
            strError = "";
            indices = new List<int>();
            // int nRet = 0;

            if (list.SelectedItems.Count == 0)
            {
                strError = "尚未选定要进行上下移动的事项";
                return -1;
            }

            // ListViewItem item = list.SelectedItems[0];
            // int index = list.Items.IndexOf(item);
            // Debug.Assert(index >= 0 && index <= list.Items.Count - 1, "");
            int index = list.SelectedIndices[0];
            ListViewItem item = list.Items[index];

            indices.Add(index);

            bool bChanged = false;

            if (bUp == true)
            {
                if (index == 0)
                {
                    strError = "到头";
                    return -1;
                }

                list.Items.RemoveAt(index);
                index--;
                list.Items.Insert(index, item);
                indices.Add(index);
                list.FocusedItem = item;

                bChanged = true;
            }

            if (bUp == false)
            {
                if (index >= list.Items.Count - 1)
                {
                    strError = "到尾";
                    return -1;
                }
                list.Items.RemoveAt(index);
                index++;
                list.Items.Insert(index, item);
                indices.Add(index);
                list.FocusedItem = item;

                bChanged = true;
            }

            if (bChanged == true)
                return 1;
            return 0;
        }

        public static void DeleteSelectedItems(ListView list)
        {
            int[] indices = new int[list.SelectedItems.Count];
            list.SelectedIndices.CopyTo(indices, 0);

            list.BeginUpdate();

            for (int i = indices.Length - 1; i >= 0; i--)
            {
                list.Items.RemoveAt(indices[i]);
            }

            list.EndUpdate();

#if NO
            for (int i = list.SelectedIndices.Count - 1;
    i >= 0;
    i--)
            {
                int index = list.SelectedIndices[i];
                list.Items.RemoveAt(index);
            }
#endif
#if NO
            foreach (ListViewItem item in list.SelectedItems)
            {
                list.Items.Remove(item);
            }
#endif
        }

        public static void SelectAllLines(ListView list)
        {
#if NO
            list.BeginUpdate();
            foreach (ListViewItem item in list.Items)
            {
                item.Selected = true;
            }
            list.EndUpdate();
#endif
            SelectAllItems(list);
        }

        #region

        private const int LVM_FIRST = 0x1000;
        private const int LVM_SETITEMSTATE = LVM_FIRST + 43;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        public struct LVITEM
        {
            public int mask;
            public int iItem;
            public int iSubItem;
            public int state;
            public int stateMask;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string pszText;
            public int cchTextMax;
            public int iImage;
            public IntPtr lParam;
            public int iIndent;
            public int iGroupId;
            public int cColumns;
            public IntPtr puColumns;
        };

        [DllImport("user32.dll", EntryPoint = "SendMessage", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessageLVItem(IntPtr hWnd, int msg, int wParam, ref LVITEM lvi);

        /// <summary>
        /// Select all rows on the given listview
        /// </summary>
        /// <param name="list">The listview whose items are to be selected</param>
        public static void SelectAllItems(ListView list)
        {
            SetItemState(list, -1, 2, 2);
        }

        /// <summary>
        /// Deselect all rows on the given listview
        /// </summary>
        /// <param name="list">The listview whose items are to be deselected</param>
        public static void DeselectAllItems(ListView list)
        {
            SetItemState(list, -1, 2, 0);
        }

        /// <summary>
        /// Set the item state on the given item
        /// </summary>
        /// <param name="list">The listview whose item's state is to be changed</param>
        /// <param name="itemIndex">The index of the item to be changed</param>
        /// <param name="mask">Which bits of the value are to be set?</param>
        /// <param name="value">The value to be set</param>
        public static void SetItemState(ListView list, int itemIndex, int mask, int value)
        {
            LVITEM lvItem = new LVITEM();
            lvItem.stateMask = mask;
            lvItem.state = value;
            SendMessageLVItem(list.Handle, LVM_SETITEMSTATE, itemIndex, ref lvItem);
        }

        #endregion

        // 获得列标题宽度字符串
        public static string GetColumnWidthListString(ListView list)
        {
            string strResult = "";
            for (int i = 0; i < list.Columns.Count; i++)
            {
                ColumnHeader header = list.Columns[i];
                if (i != 0)
                    strResult += ",";
                strResult += header.Width.ToString();
            }

            return strResult;
        }

        // 获得列标题宽度字符串
        // 扩展功能版本。不包含右边连续的没有标题文字的栏
        public static string GetColumnWidthListStringExt(ListView list)
        {
            string strResult = "";
            int nEndIndex = list.Columns.Count;
            for (int i = list.Columns.Count - 1; i >= 0; i--)
            {
                ColumnHeader header = list.Columns[i];
                if (String.IsNullOrEmpty(header.Text) == false)
                    break;
                nEndIndex = i;
            }
            for (int i = 0; i < nEndIndex; i++)
            {
                ColumnHeader header = list.Columns[i];
                if (i != 0)
                    strResult += ",";
                strResult += header.Width.ToString();
            }

            return strResult;
        }

        // 设置列标题的宽度
        // parameters:
        //      bExpandColumnCount  是否要扩展列标题到足够数目？
        public static void SetColumnHeaderWidth(ListView list,
            string strWidthList,
            bool bExpandColumnCount)
        {
            string[] parts = strWidthList.Split(new char[] { ',' });

            if (bExpandColumnCount == true)
                EnsureColumns(list, parts.Length, 100);

            for (int i = 0; i < parts.Length; i++)
            {
                if (i >= list.Columns.Count)
                    break;

                string strValue = parts[i].Trim();
                int nWidth = -1;
                try
                {
                    nWidth = Convert.ToInt32(strValue);
                }
                catch
                {
                    break;
                }

                if (nWidth != -1)
                    list.Columns[i].Width = nWidth;
            }
        }



        // 响应选择标记发生变化的动作，修改栏目标题文字
        // parameters:
        //      protect_column_numbers  需要保护的列的列号数组。列号从0开始计算。所谓保护就是不破坏这样的列的标题，设置标题从它们以外的列开始算起。nRecPathColumn表示的列号不必纳入本数组，也会自动受到保护。如果不需要本参数，可以用null
        public static void OnSelectedIndexChanged(ListView list,
            int nRecPathColumn,
            List<int> protect_column_numbers)
        {
            ListViewProperty prop = GetListViewProperty(list);

            if (prop == null)
            {
                throw new Exception("ListView 的 Tag 没有包含 ListViewProperty 对象");
            }

            if (list.SelectedItems.Count == 0)
            {
                // 清除所有栏目标题为1,2,3...，或者保留以前的残余值?
                return;
            }

            ListViewItem item = list.SelectedItems[0];
            // 获得路径。假定都在第一列？
            string strRecPath = GetItemText(item, nRecPathColumn);

            ColumnPropertyCollection props = null;
            string strDbName = "";

            if (String.IsNullOrEmpty(strRecPath) == true)
            {
                strDbName = "<blank>";  // 特殊的数据库名，表示第一列空的情况
                props = prop.GetColumnName(strDbName);
                goto DO_REFRESH;
            }

            // 取出数据库名
            strDbName = prop.ParseDbName(strRecPath);   //  GetDbName(strRecPath);

            if (String.IsNullOrEmpty(strDbName) == true)
            {
                return;
            }

            if (strDbName == prop.CurrentDbName)
                return; // 没有必要刷新

            props = prop.GetColumnName(strDbName);

        DO_REFRESH:

            if (props == null)
            {
                // not found

                // 清除所有栏目标题为1,2,3...，或者保留以前的残余值?
                props = new ColumnPropertyCollection();
            }

            // 修改文字
            int index = 0;
            for (int i = 0; i < list.Columns.Count; i++)
            {
                ColumnHeader header = list.Columns[i];

                if (i == nRecPathColumn)
                    continue;

                // 越过需要保护的列
                if (protect_column_numbers != null)
                {
                    if (protect_column_numbers.IndexOf(i) != -1)
                        continue;
                }

#if NO
                if (index < props.Count)
                {
                    if (header.Tag != null)
                        header.Tag = props[index];
                    else
                        header.Text = props[index].Title;
                }
                else 
                {
                    ColumnProperty temp = (ColumnProperty)header.Tag;

                    if (temp == null)
                        header.Text = i.ToString();
                    else
                        header.Text = temp.Title;
                }
#endif

                ColumnProperty temp = (ColumnProperty)header.Tag;

                if (index < props.Count)
                {
                    if (temp != props[index])
                    {
                        header.Tag = props[index];
                        temp = props[index];
                    }
                }
                else
                    temp = null;    // 2013/10/5 多出来找不到定义的列，需要显示为数字

                if (temp == null)
                {
                    // 如果 header 以前有文字就沿用，没有时才使用编号填充 2014/9/6 消除 BUG
                    if (string.IsNullOrEmpty(header.Text) == true)
                        header.Text = i.ToString();
                }
                else
                    header.Text = temp.Title;

                index++;
            }

            // 刷新排序列的显示。也就是说刷新那些参与了排序的个别列的显示
            prop.SortColumns.RefreshColumnDisplay(list.Columns);

            prop.CurrentDbName = strDbName; // 记忆
        }

        // 响应点击栏目标题的动作，进行排序
        // parameters:
        //      bClearSorter    是否在排序后清除 sorter 函数
        public static void OnColumnClick(ListView list,
            ColumnClickEventArgs e,
            bool bClearSorter = true)
        {
            int nClickColumn = e.Column;

            ListViewProperty prop = GetListViewProperty(list);

            if (prop == null)
            {
                throw new Exception("ListView的Tag没有包含ListViewProperty对象");
            }

            // 2013/3/31
            // 如果标题栏没有初始化，则需要先初始化
            if (list.SelectedItems.Count == 0 && list.Items.Count > 0)
            {
                list.Items[0].Selected = true;
                OnSelectedIndexChanged(list,
                    0,
                    null);
                list.Items[0].Selected = false;
            }

            ColumnSortStyle sortStyle = prop.GetSortStyle(list, nClickColumn);

            prop.SortColumns.SetFirstColumn(nClickColumn,
                sortStyle,
                list.Columns,
                true);

            // 排序
            SortColumnsComparer sorter = new SortColumnsComparer(prop.SortColumns);
            if (prop.HasCompareColumnEvent() == true)
            {
                sorter.EventCompare += (sender1, e1) =>
                {
                    prop.OnCompareColumn(sender1, e1);
                };
            }
            list.ListViewItemSorter = sorter;

            if (bClearSorter == true)
                list.ListViewItemSorter = null;
        }

        class SetSortStyleParam
        {
            public ColumnSortStyle Style;
            public ListViewProperty prop = null;
            public int ColumnIndex = -1;
        }

        // 响应鼠标右键点击栏目标题的动作，出现上下文菜单
        public static void OnColumnContextMenuClick(ListView list,
            ColumnClickEventArgs e)
        {
            int nClickColumn = e.Column;

            ListViewProperty prop = GetListViewProperty(list);

            if (prop == null)
            {
                throw new Exception("ListView的Tag没有包含ListViewProperty对象");
            }

#if NO
            ColumnSortStyle sortStyle = prop.GetSortStyle(nClickColumn);
            prop.SortColumns.SetFirstColumn(nClickColumn,
                sortStyle,
                list.Columns,
                true);
#endif
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem menuItem = null;
            ToolStripMenuItem subMenuItem = null;

            // list.Columns[nClickColumn].Text
            menuItem = new ToolStripMenuItem("设置排序方式");
            contextMenu.Items.Add(menuItem);

            ColumnSortStyle sortStyle = prop.GetSortStyle(list, nClickColumn);
            if (sortStyle == null)
                sortStyle = ColumnSortStyle.None;

            List<ColumnSortStyle> all_styles = prop.GetAllSortStyle(list, nClickColumn);

            foreach (ColumnSortStyle style in all_styles)
            {
                subMenuItem = new ToolStripMenuItem();
                subMenuItem.Text = GetSortStyleCaption(style);
                SetSortStyleParam param = new SetSortStyleParam();
                param.ColumnIndex = nClickColumn;
                param.prop = prop;
                param.Style = style;
                subMenuItem.Tag = param;
                subMenuItem.Click += new EventHandler(menu_setSortStyle_Click);
                if (style == sortStyle)
                    subMenuItem.Checked = true;
                menuItem.DropDown.Items.Add(subMenuItem);
            }

            Point p = list.PointToClient(Control.MousePosition);
            contextMenu.Show(list, p);
        }

        static string GetSortStyleCaption(ColumnSortStyle style)
        {
            string strName = style.Name;
            if (string.IsNullOrEmpty(strName) == true)
                return "[None]";

            // 将 call_number 形态转换为 CallNumber 形态
            string[] parts = strName.Split(new char[] { '_' }, StringSplitOptions.RemoveEmptyEntries);
            StringBuilder text = new StringBuilder(4096);
            foreach (string s in parts)
            {
                if (string.IsNullOrEmpty(s) == true)
                    continue;

                text.Append(char.ToUpper(s[0]));
                if (s.Length > 1)
                    text.Append(s.Substring(1));
            }

            return text.ToString();
        }

        static void menu_setSortStyle_Click(object sender, EventArgs e)
        {
            var menu = sender as ToolStripMenuItem;
            var param = menu.Tag as SetSortStyleParam;
            param.prop.SetSortStyle(param.ColumnIndex, param.Style);
        }

        // 清除所有留存的排序信息，刷新list的标题栏上的陈旧的排序标志
        public static void ClearSortColumns(ListView list)
        {
            ListViewProperty prop = GetListViewProperty(list);

            if (prop == null)
                return;

            prop.SortColumns.Clear();
            SortColumns.ClearColumnSortDisplay(list.Columns);

            prop.CurrentDbName = "";    // 清除记忆
        }

        // 获得ListViewProperty对象
        public static ListViewProperty GetListViewProperty(ListView list)
        {
            if (list.Tag == null)
                return null;
            if (!(list.Tag is ListViewProperty))
                return null;

            return (ListViewProperty)list.Tag;
        }

        // 查找一个事项
        public static ListViewItem FindItem(ListView listview,
            string strText,
            int nColumn)
        {
            for (int i = 0; i < listview.Items.Count; i++)
            {
                ListViewItem item = listview.Items[i];
                string strThisText = GetItemText(item, nColumn);
                if (strThisText == strText)
                    return item;
            }

            return null;
        }

        // 检测一个x位置在何列上。
        // return:
        //		-1	没有命中
        //		其他 列号
        public static int ColumnHitTest(ListView listview,
            int x)
        {
            int nStart = 0;
            for (int i = 0; i < listview.Columns.Count; i++)
            {
                ColumnHeader header = listview.Columns[i];
                if (x >= nStart && x < nStart + header.Width)
                    return i;
                nStart += header.Width;
            }

            return -1;
        }

        // 确保列标题数量足够
        public static void EnsureColumns(ListView listview,
            int nCount,
            int nInitialWidth = 200)
        {
            if (listview.Columns.Count >= nCount)
                return;

            for (int i = listview.Columns.Count; i < nCount; i++)
            {
                string strText = "";
                // strText = Convert.ToString(i);

                ColumnHeader col = new ColumnHeader();
                col.Text = strText;
                col.Width = nInitialWidth;
                listview.Columns.Add(col);
            }
        }

        // 获得一个单元的值
        public static string GetItemText(ListViewItem item,
            int col)
        {
            if (col == 0)
                return item.Text;

            // 2008/5/14。否则会抛出异常
            if (col >= item.SubItems.Count)
                return "";

            return item.SubItems[col].Text;
        }

        // 修改一个单元的值
        public static void ChangeItemText(ListViewItem item,
            int col,
            string strText)
        {
            // 确保线程安全 2014/9/3
            if (item.ListView != null && item.ListView.InvokeRequired)
            {
                item.ListView.BeginInvoke(new Action<ListViewItem, int, string>(ChangeItemText), item, col, strText);
                return;
            }

            if (col == 0)
            {
                item.Text = strText;
                return;
            }

            // 保险
            while (item.SubItems.Count < col + 1)   // 原来为<=, 会造成多加一列的后果 2006/10/9 changed
            {
                item.SubItems.Add("");
            }

#if NO
            item.SubItems.RemoveAt(col);
            item.SubItems.Insert(col, new ListViewItem.ListViewSubItem(item, strText));
#endif
            item.SubItems[col].Text = strText;
        }

        // 2009/10/21
        // 获得一个行的值。即把各个单元的值用\t字符连接起来
        public static string GetLineText(ListViewItem item)
        {
            string strResult = "";
            for (int i = 0; i < item.SubItems.Count; i++)
            {
                if (i > 0)
                    strResult += "\t";

                strResult += item.SubItems[i].Text;
            }

            return strResult;
        }

        // 清除全部选择状态
        public static void ClearSelection(ListView list)
        {
            list.SelectedItems.Clear();
        }

        // 清除全部 Checked 状态
        public static void ClearChecked(ListView list)
        {
            List<ListViewItem> items = new List<ListViewItem>();
            foreach (ListViewItem item in list.CheckedItems)
            {
                items.Add(item);
            }

            foreach (ListViewItem item in items)
            {
                item.Checked = false;
            }
        }

        // 选择一行
        // parameters:
        //		nIndex	要设置选择标记的行。如果==-1，表示清除全部选择标记但不选择。
        //		bMoveFocus	是否同时移动focus标志到所选择行
        public static void SelectLine(ListView list,
            int nIndex,
            bool bMoveFocus)
        {
            list.SelectedItems.Clear();

            if (nIndex != -1)
            {
                list.Items[nIndex].Selected = true;

                if (bMoveFocus == true)
                {
                    list.Items[nIndex].Focused = true;
                }
            }
        }

        // 选择一行
        // 2008/9/9
        // parameters:
        //		bMoveFocus	是否同时移动focus标志到所选择行
        public static void SelectLine(ListViewItem item,
            bool bMoveFocus)
        {
            Debug.Assert(item != null, "");

            item.ListView.SelectedItems.Clear();

            item.Selected = true;

            if (bMoveFocus == true)
            {
                item.Focused = true;
            }
        }

    }

    public static class ListBoxUtil
    {
        public static void EnsureVisible(ListBox listBox, int nItemIndex)
        {
            int visibleItems = listBox.ClientSize.Height / listBox.ItemHeight;
            listBox.TopIndex = Math.Max(nItemIndex - visibleItems + 1, 0);
        }
    }

    public class ListViewProperty
    {
        public string CurrentDbName = ""; // 当前已经显示的标题所对应的数据库名。为了加快速度

        public event GetColumnTitlesEventHandler GetColumnTitles = null;
        public event ParsePathEventHandler ParsePath = null;
        public event CompareEventHandler CompareColumn = null;

        // 参与排序的列号数组
        public SortColumns SortColumns = new SortColumns();

        public List<ColumnSortStyle> SortStyles = new List<ColumnSortStyle>();

        public Hashtable UsedColumnTitles = new Hashtable();   // key为数据库名，value为List<string>

        public void ClearCache()
        {
            this.UsedColumnTitles.Clear();
            this.CurrentDbName = "";
        }

        public void OnCompareColumn(object sender, CompareEventArgs e)
        {
            if (this.CompareColumn != null)
                this.CompareColumn(sender, e);
        }

        public bool HasCompareColumnEvent()
        {
            if (this.CompareColumn != null)
                return true;
            return false;
        }

        // 获得一个列可用的全部 sort style
        public List<ColumnSortStyle> GetAllSortStyle(ListView list, int nColumn)
        {
            List<ColumnSortStyle> styles = new List<ColumnSortStyle>();
            styles.Add(ColumnSortStyle.None); // 没有
            styles.Add(ColumnSortStyle.LeftAlign); // 左对齐字符串
            styles.Add(ColumnSortStyle.RightAlign);// 右对齐字符串
            styles.Add(ColumnSortStyle.RecPath);    // 记录路径。例如“中文图书/1”，以'/'为界，右边部分当作数字值排序。或者“localhost/中文图书/ctlno/1”
            styles.Add(ColumnSortStyle.LongRecPath);  // 记录路径。例如“中文图书/1 @本地服务器”

            // 寻找标题 .Tag 中的定义
            if (nColumn < list.Columns.Count)
            {
                ColumnHeader header = list.Columns[nColumn];
                ColumnProperty prop = (ColumnProperty)header.Tag;
                if (prop != null)
                {
                    if (string.IsNullOrEmpty(prop.Type) == false)
                    {
                        ColumnSortStyle default_style = new ColumnSortStyle(prop.Type);
                        if (styles.IndexOf(default_style) == -1)
                            styles.Add(default_style);
                    }
                }
            }
            return styles;
        }


        public ColumnSortStyle GetSortStyle(ListView list, int nColumn)
        {
            ColumnSortStyle result = null;
            if (this.SortStyles.Count <= nColumn)
            {
            }
            else
                result = SortStyles[nColumn];

            if (result == null || result == ColumnSortStyle.None)
            {
                // 寻找标题 .Tag 中的定义
                if (nColumn < list.Columns.Count)
                {
                    ColumnHeader header = list.Columns[nColumn];
                    ColumnProperty prop = (ColumnProperty)header.Tag;
                    if (prop != null)
                    {
                        if (string.IsNullOrEmpty(prop.Type) == false)
                            return new ColumnSortStyle(prop.Type);
                    }
                }
            }
            return result;
        }

        public void SetSortStyle(int nColumn, ColumnSortStyle style)
        {
            // 确保元素足够
            while (this.SortStyles.Count < nColumn + 1)
            {
                this.SortStyles.Add(null); // 或者 .None // 缺省的 ColumnSortStyle.LeftAlign
            }

            this.SortStyles[nColumn] = style;

            // 2013/3/27
            // 刷新 SortColumns
            foreach (Column column in this.SortColumns)
            {
                if (column.No == nColumn)
                    column.SortStyle = style;
            }
        }

        public ColumnPropertyCollection GetColumnName(string strDbName)
        {
            // 先从Hashtable中寻找
            if (this.UsedColumnTitles.Contains(strDbName) == true)
                return (ColumnPropertyCollection)this.UsedColumnTitles[strDbName];

            if (this.GetColumnTitles != null)
            {
                GetColumnTitlesEventArgs e = new GetColumnTitlesEventArgs();
                e.DbName = strDbName;
                e.ListViewProperty = this;
                this.GetColumnTitles(this, e);
                if (e.ColumnTitles != null)
                {
                    this.UsedColumnTitles[strDbName] = e.ColumnTitles;
                }
                return e.ColumnTitles;
            }

            return null;    // not found
        }

        public string ParseDbName(string strPath)
        {
            if (this.ParsePath != null)
            {
                ParsePathEventArgs e = new ParsePathEventArgs();
                e.Path = strPath;
                this.ParsePath(this, e);
                return e.DbName;
            }

            // 如果是 "中文图书/3" 则返回数据库名，如果是"中文图书/1@本地服务器"则返回全路径
            return GetDbName(strPath);
        }

        // 从路径中取出库名部分
        // parammeters:
        //      strPath 路径。例如"中文图书/3"
        public static string GetDbName(string strPath)
        {
            // 看看是否有服务器名部分 2015/8/12
            int nRet = strPath.IndexOf("@");
            if (nRet != -1)
            {
                return strPath; // 返回全路径
            }

            nRet = strPath.LastIndexOf("/");
            if (nRet == -1)
                return strPath;

            return strPath.Substring(0, nRet).Trim();
        }
#if NO
        // 从路径中取出库名部分
        // parammeters:
        //      strPath 路径。例如"中文图书/3"
        public static string GetDbName(string strPath)
        {
            // 看看是否有服务器名部分 2015/8/11
            string strServerName = "";
            int nRet = strPath.IndexOf("@");
            if (nRet != -1)
            {
                strServerName = strPath.Substring(nRet).Trim(); // 包含字符 '@'
                strPath = strPath.Substring(0, nRet).Trim();
            }

            nRet = strPath.LastIndexOf("/");
            if (nRet == -1)
                return strPath + strServerName;

            return strPath.Substring(0, nRet).Trim() + strServerName;
        }
#endif
    }

    /// <summary>
    /// 获得栏目标题
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void GetColumnTitlesEventHandler(object sender,
    GetColumnTitlesEventArgs e);

    // 2013/3/31
    /// <summary>
    /// 一个栏目的属性
    /// </summary>
    public class ColumnProperty
    {
        /// <summary>
        /// 栏目标题
        /// </summary>
        public string Title = "";   // 栏目标题

        /// <summary>
        /// 数值类型
        /// </summary>
        public string Type = "";    // 数值类型。排序时有用

        /// <summary>
        /// XPath
        /// </summary>
        public string XPath = "";   // XPath 字符串 2015/8/27

        /// <summary>
        /// 字符串转换方法
        /// </summary>
        public string Convert = ""; // 字符串转换方法 2015/8/27

        public ColumnProperty(string strTitle,
            string strType = "",
            string strXPath = "",
            string strConvert = "")
        {
            this.Title = strTitle;
            this.Type = strType;
            this.XPath = strXPath;
            this.Convert = strConvert;
        }
    }

    /// <summary>
    /// 栏目属性集合
    /// </summary>
    public class ColumnPropertyCollection : List<ColumnProperty>
    {
        /// <summary>
        /// 追加一个栏目属性对象
        /// </summary>
        /// <param name="strTitle">标题</param>
        /// <param name="strType">类型</param>
        public void Add(string strTitle,
            string strType = "",
            string strXPath = "",
            string strConvert = "")
        {
            ColumnProperty prop = new ColumnProperty(strTitle, strType, strXPath, strConvert);
            base.Add(prop);
        }

        /// <summary>
        /// 插入一个栏目属性对象
        /// </summary>
        /// <param name="nIndex">插入位置下标</param>
        /// <param name="strTitle">标题</param>
        /// <param name="strType">类型</param>
        public void Insert(int nIndex, string strTitle, string strType = "")
        {
            ColumnProperty prop = new ColumnProperty(strTitle, strType);
            base.Insert(nIndex, prop);
        }

        /// <summary>
        /// 根据 type 值查找列号
        /// </summary>
        /// <returns>-1: 没有找到; 其他: 列号</returns>
        public int FindColumnByType(string strType)
        {
            int index = 0;
            foreach (ColumnProperty col in this)
            {
                if (col.Type == strType)
                    return index;
                index++;
            }
            return -1;
        }
    }

    /// <summary>
    /// 获得栏目标题的参数
    /// </summary>
    public class GetColumnTitlesEventArgs : EventArgs
    {
        public string DbName = "";  // [in] 如果值为"<blank>"，表示第一列为空的情况，例如keys的情形
        public ListViewProperty ListViewProperty = null;    // [in][out]

        // public List<string> ColumnTitles = null;  // [out] null表示not found；而.Count == 0表示栏目标题为空，并且不是not found
        public ColumnPropertyCollection ColumnTitles = null;  // [out] null表示not found；而.Count == 0表示栏目标题为空，并且不是not found

        // public string ErrorInfo = "";    // [out]
    }

    /// <summary>
    /// 解释路径
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void ParsePathEventHandler(object sender,
    ParsePathEventArgs e);

    /// <summary>
    /// 解析路径的参数
    /// </summary>
    public class ParsePathEventArgs : EventArgs
    {
        public string Path = "";    // [in]
        public string DbName = "";    // [out]  // 数据库名部分。可能包含服务器名称部分
    }
}
