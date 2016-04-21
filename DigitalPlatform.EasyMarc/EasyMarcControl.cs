using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;

using DigitalPlatform.CommonControl;
using DigitalPlatform.Marc;
using DigitalPlatform.Xml;
using DigitalPlatform.Text;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using DigitalPlatform.GUI;
using System.Threading.Tasks;

namespace DigitalPlatform.EasyMarc
{
    /// <summary>
    /// MARC 模板输入界面控件
    /// </summary>
    public partial class EasyMarcControl : UserControl
    {
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler SelectionChanged = null;

        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public new event EventHandler TextChanged
        {
            // http://stackoverflow.com/questions/9370448/add-attribute-to-base-event
            add
            {
                base.TextChanged += value;
            }
            remove
            {
                base.TextChanged -= value;
            }
        }

        Font _fixedFont = null;

        internal Font FixedFont
        {
            get
            {
                if (this._fixedFont == null)
                    this._fixedFont = new Font("Courier New", this.Font.Size);
                return this._fixedFont;
            }
        }

        public override Font Font
        {
            get
            {
                return base.Font;
            }
            set
            {
                base.Font = value;

                this._fixedFont = null;
            }
        }

        public EasyMarcControl()
        {
            this.DoubleBuffered = true;

            InitializeComponent();
        }

        /// <summary>
        /// 解析宏
        /// </summary>
        public event ParseMacroEventHandler ParseMacro = null;

        // 原始的行数组
        public List<EasyLine> Items = new List<EasyLine>();

        bool m_bChanged = false;

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        [Category("Content")]
        [DescriptionAttribute("Changed")]
        [DefaultValue(false)]
        public bool Changed
        {
            get
            {
                return this.m_bChanged;
            }
            set
            {
                if (this.m_bChanged != value)
                {
                    this.m_bChanged = value;

                    if (value == false)
                        ResetLineState();
                }
            }
        }

        bool m_bHideSelection = true;

        [Category("Appearance")]
        [DescriptionAttribute("HideSelection")]
        [DefaultValue(true)]
        public bool HideSelection
        {
            get
            {
                return this.m_bHideSelection;
            }
            set
            {
                if (this.m_bHideSelection != value)
                {
                    this.m_bHideSelection = value;
                    this.RefreshLineColor(); // 迫使颜色改变
                }
            }
        }

        void RefreshLineColor(bool bSetAll = false)
        {
            for (int i = 0; i < this.Items.Count; i++)
            {
                EasyLine item = this.Items[i];
                item.SetLineColor(bSetAll);
            }
        }

        // 将全部行的状态恢复为普通状态
        void ResetLineState()
        {
            foreach (EasyLine item in this.Items)
            {
                if ((item.State & ItemState.ReadOnly) != 0)
                    item.State = ItemState.Normal | ItemState.ReadOnly;
                else
                    item.State = ItemState.Normal;
            }

            this.Invalidate();
        }

        bool _hideIndicator = true;
        /// <summary>
        /// 是否要隐藏字段指示符?
        /// </summary>
        public bool HideIndicator
        {
            get
            {
                return this._hideIndicator;
            }
            set
            {
                if (this._hideIndicator != value)
                {
                    this._hideIndicator = value;
                    foreach (EasyLine line in this.Items)
                    {
                        if (line is FieldLine)
                        {
                            FieldLine field = line as FieldLine;
                            if (field.IsControlField == false
                                && field.Visible == true)
                                field.textBox_content.Visible = !value;
                        }
                    }
                }
            }
        }

#if NO
        bool _hideFields = false;
        public bool HideFields
        {
            get
            {
                return this._hideFields;
            }
            set
            {
                this._hideFields = value;
            }
        }
#endif
        /// <summary>
        /// 获得当前已经选定的事项列表
        /// </summary>
        public List<EasyLine> SelectedItems
        {
            get
            {
                List<EasyLine> results = new List<EasyLine>();

                foreach (EasyLine cur_element in this.Items)
                {
                    if ((cur_element.State & ItemState.Selected) != 0)
                        results.Add(cur_element);
                }

                return results;
            }
        }

        /// <summary>
        /// 获得当前已经选定的事项的下标的列表
        /// </summary>
        public List<int> SelectedIndices
        {
            get
            {
                List<int> results = new List<int>();

                int i = 0;
                foreach (EasyLine cur_element in this.Items)
                {
                    if ((cur_element.State & ItemState.Selected) != 0)
                        results.Add(i);
                    i++;
                }

                return results;
            }
        }

        /// <summary>
        /// 全选所有事项
        /// </summary>
        public void SelectAll()
        {
            bool bSelectionChanged = false;
            foreach (EasyLine cur_element in this.Items)
            {
                if ((cur_element.State & ItemState.Selected) == 0)
                {
                    cur_element.State |= ItemState.Selected;
                    bSelectionChanged = true;
                }
            }

            this.Invalidate();

            if (bSelectionChanged)
                OnSelectionChanged(new EventArgs());
        }

        internal void Verify()
        {
            for (int i = 0; i < this.Items.Count; i++)
            {
                EasyLine start = this.Items[i];

                for (int j = i + 1; j < this.Items.Count; j++)
                {
                    EasyLine current = this.Items[j];

                    if (start.textBox_content == current.textBox_content)
                        throw new Exception(i.ToString() + "==" + j.ToString());
                }
            }

            for (int i = 0; i < this.Items.Count; i++)
            {
                EasyLine line = this.Items[i];

                for (int j = 0; j < this.tableLayoutPanel_content.ColumnStyles.Count; j++)
                {
                    Control control = tableLayoutPanel_content.GetAnyControlAt(j, i);

                    Debug.Assert(control != null, "");

                    if (j == 0)
                        Debug.Assert(control == line.label_color, "");
                    if (j == 1)
                        Debug.Assert(control == line.label_caption, "");
                    if (j == 2)
                        Debug.Assert(control == line.splitter, "");
                    if (j == 3)
                        Debug.Assert(control == line.textBox_content, "");

                    int row_span = this.tableLayoutPanel_content.GetRowSpan(control);
                    Debug.Assert(row_span == 1, "");

                    int col_span = this.tableLayoutPanel_content.GetColumnSpan(control);
                    Debug.Assert(col_span == 1, "");

                }
            }

        }

        // 得到上一个可以输入内容的字段或子字段对象
        // parameters:
        //      ref_line    参考的对象。从它前面一个开始获取。如果为 null，表示获取最后一个可编辑的对象
        public EasyLine GetPrevEditableLine(EasyLine ref_line)
        {
            EasyLine line = null;
            foreach (EasyLine item in this.Items)
            {
                if (ref_line != null && ref_line == item)
                    break;

                {
                    if (item.Visible == false)
                        continue;
                    if (item.ExpandState == EasyMarc.ExpandState.Collapsed)
                        continue;
                    if (item.textBox_content.Visible == false)
                        continue;
                    // results.Add(item);
                    line = item;
                }
            }

            return line;
        }

        // 得到下一个可以输入内容的字段或子字段对象
        // parameters:
        //      ref_line    参考的对象。从它后面一个开始获取。如果为 null，表示获取第一个可编辑的对象
        public EasyLine GetNextEditableLine(EasyLine ref_line)
        {
            bool bOn = false;
            if (ref_line == null)
                bOn = true;
            foreach (EasyLine item in this.Items)
            {
                if (bOn == false && ref_line == item)
                {
                    bOn = true;
                    continue;
                }

                if (bOn)
                {
                    if (item.Visible == false)
                        continue;
                    if (item.ExpandState == EasyMarc.ExpandState.Collapsed)
                        continue;
                    if (item.textBox_content.Visible == false)
                        continue;
                    return item;
                }
            }

            return null;
        }

        // 获得一个不是 transparent color 的背景色
        internal Color GetRealBackColor(Color default_color)
        {
            Control control = this;
            while (control != null)
            {
                if (control.BackColor != Color.Transparent)
                    return control.BackColor;
                control = control.Parent;
            }

            return default_color;
        }
#if NO
        internal void AsyncSetAutoScrollPosition(Point p)
        {
            this.BeginInvoke(new Action<Point>(_setAutoScrollPosition), p);
        }

        void _setAutoScrollPosition(Point p)
        {
            this.SetAutoScrollPosition(p);
        }
#endif

        /// <summary>
        /// 获得本控件内部使用的 TableLayoutPanel 对象
        /// </summary>
        public TableLayoutPanel TableLayoutPanel
        {
            get
            {
                return this.tableLayoutPanel_content;
            }
        }

        /// <summary>
        /// 确保一个事项处在可见范围
        /// </summary>
        /// <param name="item">EasyItem 对象</param>
        public void EnsureVisible(EasyLine item)
        {
            if (item.Visible == true)
                this.ScrollControlIntoView(item.label_caption);
#if NO
            int[] row_heights = this.tableLayoutPanel_content.GetRowHeights();
            int nYOffs = 0; // row_heights[0];
            int i = 0;  // 1
            foreach (EasyLine cur_item in this.Items)
            {
                if (cur_item == item)
                    break;
                nYOffs += row_heights[i++];
            }

            // this.AutoScrollPosition = new Point(this.AutoScrollOffset.X, 1000);
            if (nYOffs < - this.AutoScrollPosition.Y)
            {
                this.AutoScrollPosition = new Point(this.AutoScrollOffset.X, nYOffs);
            }
            else if (nYOffs + row_heights[i] > - (this.AutoScrollPosition.Y - this.ClientSize.Height))
            {
                // 刚好进入下部
                this.AutoScrollPosition = new Point(this.AutoScrollOffset.X, nYOffs - this.ClientSize.Height + row_heights[i]);
            }
#endif
        }

        public ImageList ImageListIcons
        {
            get
            {
                return this.imageList_expandIcons;
            }
        }

        // 文档发生改变
        internal void FireTextChanged()
        {
            this.Changed = true;

            EventArgs e = new EventArgs();
            this.OnTextChanged(e);
            //if (this.TextChanged != null)
            //    this.TextChanged(this, e);
        }

        // 找到一个子字段所从属的字段行
        public FieldLine GetFieldLine(SubfieldLine subfield)
        {
            int nStart = this.Items.IndexOf(subfield);
            if (nStart == -1)
                return null;
            for (int i = nStart - 1; i >= 0; i--)
            {
                EasyLine line = this.Items[i];
                if (line is FieldLine)
                    return line as FieldLine;
            }

            return null;
        }

        // 获得一个字段行下属的全部子字段行
        List<EasyLine> GetSubfieldLines(FieldLine field)
        {
            List<EasyLine> results = new List<EasyLine>();
            int nStart = this.Items.IndexOf(field);
            if (nStart == -1)
                return results;
            for (int i = nStart + 1; i < this.Items.Count; i++)
            {
                EasyLine line = this.Items[i];
                if (line is FieldLine)
                    break;
                results.Add(line);
            }

            return results;
        }

        // parameters:
        //      strStyle    要删除哪些空元素? visible/hidden
        public void DeleteBlankElements(string strStyle)
        {
            bool bVisible = StringUtil.IsInList("visible", strStyle);
            bool bHidden = StringUtil.IsInList("hidden", strStyle);

            if (StringUtil.IsInList("all", strStyle) == true)
                bVisible = bHidden = true;

            List<EasyLine> selected_lines = new List<EasyLine>();
            FieldLine current_field = null;
            foreach (EasyLine line in this.Items)
            {
                if (line is FieldLine)
                    current_field = line as FieldLine;
                else if (line is SubfieldLine
                    && current_field != null)
                {
                    if ((current_field.Visible == true && bVisible == true)
                        || (current_field.Visible == false && bHidden == true))
                    {
                        SubfieldLine subfield = line as SubfieldLine;
                        if (string.IsNullOrEmpty(subfield.Content) == true)
                            selected_lines.Add(line);
                    }
                }
            }
            DeleteElements(selected_lines);
            // 再观察哪些字段，一个子字段也没有了?
            selected_lines = new List<EasyLine>();
            foreach (EasyLine line in this.Items)
            {
                if (line is FieldLine)
                {
                    if ((line.Visible == true && bVisible == true)
    || (line.Visible == false && bHidden == true))
                    {
                        FieldLine field = line as FieldLine;
                        if (field.IsControlField == true)
                        {
                            if (string.IsNullOrEmpty(field.Content) == true)
                                selected_lines.Add(line);
                        }
                        else
                        {
                            List<EasyLine> subfields = GetSubfieldLines(field);
                            if (subfields.Count == 0)
                                selected_lines.Add(line);
                        }
                    }
                }
            }
            DeleteElements(selected_lines);
        }

        // 删除若干行
        // 如果其中包含字段行，则要把字段下属的子字段行一并删除
        public void DeleteElements(List<EasyLine> selected_lines)
        {
            this.DisableUpdate();
            try
            {
                // TODO: 先将要删除的行对象记忆下来，然后挪动 Focus，然后再删除
                bool bChanged = false;
                List<EasyLine> deleted_subfields = new List<EasyLine>();
                // 先删除里面的 Field 行
                foreach (EasyLine line in selected_lines)
                {
                    if (line is FieldLine)
                    {
                        List<EasyLine> subfields = GetSubfieldLines(line as FieldLine);

                        this.RemoveItem(line, false);
                        bChanged = true;
                        foreach (EasyLine subfield in subfields)
                        {
                            this.RemoveItem(subfield, false);
                            bChanged = true;
                        }
                        deleted_subfields.AddRange(subfields);
                    }
                }

                // 然后删除零星的子字段行
                foreach (EasyLine line in selected_lines)
                {
                    if (line is SubfieldLine)
                    {
                        if (deleted_subfields.IndexOf(line) == -1)
                        {
                            this.RemoveItem(line, false);
                            bChanged = true;
                        }
                    }
                }

                if (bChanged == true)
                    this.FireTextChanged();
            }
            finally
            {
                this.EnableUpdate();
            }
        }

        EasyLine LastClickItem = null;   // 最近一次click选择过的Item对象

        /// <summary>
        /// 选定一个事项
        /// </summary>
        /// <param name="element">事项</param>
        /// <param name="bClearOld">是否清除以前的选择</param>
        public void SelectItem(EasyLine element,
            bool bClearOld,
            bool bSetFocus = true)
        {
            bool bSelectionChanged = false;

            if (bClearOld == true)
            {
                for (int i = 0; i < this.Items.Count; i++)
                {
                    EasyLine cur_element = this.Items[i];

                    if (cur_element == element)
                        continue;   // 暂时不处理当前行

                    if ((cur_element.State & ItemState.Selected) != 0)
                    {
                        cur_element.State -= ItemState.Selected;
                        bSelectionChanged = true;
                        this.InvalidateLine(cur_element);
                    }
                }
            }

            if (element == null)
                return;

            // 选中当前行
            if ((element.State & ItemState.Selected) == 0)
            {
                element.State |= ItemState.Selected;
                bSelectionChanged = true;
                this.InvalidateLine(element);
            }

            if (bSetFocus == true)
            {
                if (element.textBox_content.Visible == true)
                {

                    SetEditFocus(element.textBox_content);
                    element.textBox_content.Select(0, 0);
                }
                else
                {
                    // 2015/5/30
                    // 如果当前 item 是字段名行，需要将 Focus 设置到其后的第一个子字段行的 TextBox 上
                    if (element is FieldLine && element.Visible == true)
                    {
                        List<EasyLine> subfields = GetSubfieldLines(element as FieldLine);
                        if (subfields.Count > 0)
                            SetEditFocus(subfields[0].textBox_content);
                    }
                }
            }

            this.LastClickItem = element;

            if (bSelectionChanged)
                OnSelectionChanged(new EventArgs());
        }

        public void OnSelectionChanged(EventArgs e)
        {
            if (this.SelectionChanged != null)
                this.SelectionChanged(this, e);
        }

        /// <summary>
        /// 将一个事项的选定状态来回切换
        /// </summary>
        /// <param name="element"></param>
        public void ToggleSelectItem(EasyLine element)
        {
            // 选中当前行
            if ((element.State & ItemState.Selected) == 0)
                element.State |= ItemState.Selected;
            else
                element.State -= ItemState.Selected;

            this.InvalidateLine(element);

            this.LastClickItem = element;

            OnSelectionChanged(new EventArgs());
        }

        /// <summary>
        /// 选择一个范围的事项。本方法是从 上次选定过的事项 一直选定到 element 所指示的事项
        /// </summary>
        /// <param name="element">起点事项</param>
        public void RangeSelectItem(EasyLine element)
        {
            EasyLine start = this.LastClickItem;

            int nStart = this.Items.IndexOf(start);
            if (nStart == -1)
                return;

            int nEnd = this.Items.IndexOf(element);

            if (nStart > nEnd)
            {
                // 交换
                int nTemp = nStart;
                nStart = nEnd;
                nEnd = nTemp;
            }

            bool bSelectionChanged = false;
            for (int i = nStart; i <= nEnd; i++)
            {
                EasyLine cur_element = this.Items[i];

                if ((cur_element.State & ItemState.Selected) == 0)
                {
                    cur_element.State |= ItemState.Selected;
                    bSelectionChanged = true;
                    this.InvalidateLine(cur_element);
                }
            }

            // 清除其余位置
            for (int i = 0; i < nStart; i++)
            {
                EasyLine cur_element = this.Items[i];

                if ((cur_element.State & ItemState.Selected) != 0)
                {
                    cur_element.State -= ItemState.Selected;
                    bSelectionChanged = true;
                    this.InvalidateLine(cur_element);
                }
            }

            for (int i = nEnd + 1; i < this.Items.Count; i++)
            {
                EasyLine cur_element = this.Items[i];

                if ((cur_element.State & ItemState.Selected) != 0)
                {
                    cur_element.State -= ItemState.Selected;
                    bSelectionChanged = true;
                    this.InvalidateLine(cur_element);
                }
            }

            if (bSelectionChanged)
                OnSelectionChanged(new EventArgs());
        }

        // TODO: SelectItem(string strFieldName, string strSubfieldName)

        // focus 到第一个可见的字段
        /// <summary>
        /// 将当前第一个可见的字段设定为 Focued 状态
        /// </summary>
        public void SelectFirstItem()
        {
            foreach (EasyLine element in this.Items)
            {
                if (element.textBox_content.Visible == true)
                {
                    SetEditFocus(element.textBox_content);
                    element.textBox_content.Select(0, 0);
                    return;
                }
            }
        }

        static bool SetEditFocus(TextBox textbox)
        {
            if (textbox.Visible == true && textbox.ContainsFocus == false)
            {
                textbox.Focus();
                return true;
            }
            return false;
        }

        public List<string> HideFieldNames = new List<string>();

        // 获得当前已经隐藏的字段个数
        internal int GetHideFieldCount()
        {
            int nCount = 0;
            foreach (EasyLine line in this.Items)
            {
                if (line is FieldLine)
                {
                    FieldLine field = line as FieldLine;
                    if (field.Visible == false)
                        nCount++;
                }
            }

            return nCount;
        }

        // 将指定字段名的字段改变隐藏状态
        // parameters:
        //      field_names 要施加影响的字段名。如果为 null 表示全部
        public void HideFields(List<string> field_names, bool bHide)
        {
            this.DisableUpdate();
            try
            {
                // 2015/5/26
                bool bReverse = false;  // 是否反转逻辑。反转后，field_names 里面的表示不在集合里面的才做要求的操作
                if (field_names != null && field_names.Count > 0 && field_names[0] == "rvs")
                    bReverse = true;

                foreach (EasyLine line in this.Items)
                {
                    if (line is FieldLine)
                    {
                        FieldLine field = line as FieldLine;
                        if (field_names != null)
                        {
                            //if (field_names.IndexOf(field.Name) == -1)
                            //    continue;
                            if (MatchFieldName(field.Name, field_names) == bReverse)
                                continue;
                        }
                        if (field.Visible == bHide)
                            ToggleHide(field);
                    }
                }
            }
            finally
            {
                this.EnableUpdate();
            }
        }

        // 匹配字段名列表
        // parameters:
        //      field_names 字段名列表。每个字段名可以包含通配符 *。还可以用 @ 引导正则表达式
        public static bool MatchFieldName(string strFieldName, List<string> field_names)
        {
            foreach (string name in field_names)
            {
                if (name == "rvs")
                    continue;
                if (string.IsNullOrEmpty(name) == true)
                    continue;
                // 匹配字段名/子字段名
                // pamameters:
                //		strName	名字
                //		strMatchCase	要匹配的要求
                // return:
                //		-1	error
                //		0	not match
                //		1	match
                int nRet = MatchName(strFieldName, name);
                if (nRet == 1)
                    return true;
            }

            return false;
        }

        // 匹配字段名/子字段名
        // pamameters:
        //		strName	名字
        //		strMatchCase	要匹配的要求
        // return:
        //		-1	error
        //		0	not match
        //		1	match
        public static int MatchName(string strName,
            string strMatchCase)
        {
            if (strMatchCase == "")	// 如果strMatchCase为空，表示无论什么名字都匹配
                return 1;

            // Regular expression
            if (strMatchCase.Length >= 1
                && strMatchCase[0] == '@')
            {
                if (StringUtil.RegexCompare(strMatchCase.Substring(1),
                    RegexOptions.None,
                    strName) == true)
                    return 1;
                return 0;
            }
            else // 原来的*模式
            {
                if (CmpName(strName, strMatchCase) == 0)
                    return 1;
                return 0;
            }
        }

        // t的长度可以是s的整倍数
        public static int CmpName(string s, string t)
        {
            if (s.Length == t.Length)
                return CmpOneName(s, t);

            if ((t.Length % s.Length) != 0)
            {
                throw new Exception("t '" + t + "'的长度 " + t.Length.ToString() + " 应当为s '" + s + "' 的长度 " + s.Length.ToString() + "  的整倍数");
            }
            int nCount = t.Length / s.Length;
            for (int i = 0; i < nCount; i++)
            {
                int nRet = CmpOneName(s, t.Substring(i * s.Length, s.Length));
                if (nRet == 0)
                    return 0;
            }

            return 1;
        }

        // 含通配符的比较
        public static int CmpOneName(string s,
            string t)
        {
            int len = Math.Min(s.Length, t.Length);
            for (int i = 0; i < len; i++)
            {
                if (s[i] == '*' || t[i] == '*')
                    continue;
                if (s[i] != t[i])
                    return (s[i] - t[i]);
            }
            if (s.Length > t.Length)
                return 1;
            if (s.Length < t.Length)
                return -1;
            return 0;
        }


#if NO
        public void VisibleAllFields()
        {
            this.DisableUpdate();
            try
            {
                foreach (EasyLine line in this.Items)
                {
                    if (line is FieldLine)
                    {
                        FieldLine field = line as FieldLine;
                        if (field.Visible == false)
                            ToggleHide(field);
                    }
                }
            }
            finally
            {
                this.EnableUpdate();
            }
        }
#endif

        internal void ToggleHide(FieldLine field)
        {
            this.DisableUpdate();
            try
            {
                bool bNewValue = !field.Visible;

                field.Visible = bNewValue;

                int nStart = this.Items.IndexOf(field);
                if (nStart == -1)
                    return;

                if (field.ExpandState != ExpandState.Collapsed)
                {
                    // 将下属子字段显示或者隐藏
                    for (int i = nStart + 1; i < this.Items.Count; i++)
                    {
                        EasyLine current = this.Items[i];
                        if (current is FieldLine)
                            break;
                        if (current.Visible != bNewValue)
                            current.Visible = bNewValue;
                    }
                }
            }
            finally
            {
                this.EnableUpdate();
            }
        }

        internal void ToggleExpand(EasyLine line)
        {
            if (line.ExpandState == EasyMarc.ExpandState.None)
                return;

            this.DisableUpdate();
            try
            {
                if (line.ExpandState == EasyMarc.ExpandState.Expanded)
                    line.ExpandState = EasyMarc.ExpandState.Collapsed;
                else
                    line.ExpandState = EasyMarc.ExpandState.Expanded;

                int nStart = this.Items.IndexOf(line);
                if (nStart == -1)
                    return;

                // 将下属子字段显示或者隐藏
                for (int i = nStart + 1; i < this.Items.Count; i++)
                {
                    EasyLine current = this.Items[i];
                    if (current is FieldLine)
                        break;
                    if (line.ExpandState == ExpandState.Expanded)
                        current.Visible = true;
                    else
                        current.Visible = false;
                }

            }
            finally
            {
                this.EnableUpdate();
            }
        }

        public void ExpandAll(bool bExpand)
        {
            this.DisableUpdate();
            try
            {
                foreach (EasyLine line in this.Items)
                {
                    if (line.ExpandState != ExpandState.None)
                    {
                        if ((bExpand == true && line.ExpandState == ExpandState.Collapsed)
                            || (bExpand == false && line.ExpandState == ExpandState.Expanded))
                            ToggleExpand(line);
                    }
                }
            }
            finally
            {
                this.EnableUpdate();
            }
        }

        /// <summary>
        /// 获取或设置左边标题区的像素宽度
        /// </summary>
        public int CaptionWidth
        {
            get
            {
                return (int)this.tableLayoutPanel_content.ColumnStyles[1].Width;
            }
            set
            {
                int nNewWidth = value;
                // TODO: 当空间宽度很小的时候，可能就设置不了希望的更大宽度了。似乎应该放开限制
                nNewWidth = Math.Min(nNewWidth, this.tableLayoutPanel_content.Width / 2);
                nNewWidth = Math.Max(nNewWidth, 70);
                this.tableLayoutPanel_content.ColumnStyles[1].Width = nNewWidth;
            }
        }

        /// <summary>
        /// 增减左边标题区的像素宽度
        /// </summary>
        /// <param name="nDelta">要增减的宽度</param>
        public void ChangeCaptionWidth(int nDelta)
        {
            int nOldWidth = (int)this.tableLayoutPanel_content.ColumnStyles[1].Width;
            int nNewWidth = nOldWidth + nDelta;
            nNewWidth = Math.Min(nNewWidth, this.tableLayoutPanel_content.Width / 2);
            nNewWidth = Math.Max(nNewWidth, 70);
            this.tableLayoutPanel_content.ColumnStyles[1].Width = nNewWidth;
        }

        internal void InvalidateLine(EasyLine item)
        {
            return; // 将来有导致行背景颜色改变的操作再使用这个函数

            Point p = this.tableLayoutPanel_content.PointToScreen(new Point(0, 0));

            Rectangle rect = item.label_color.RectangleToScreen(item.label_color.ClientRectangle);
            rect.Width = this.tableLayoutPanel_content.DisplayRectangle.Width;
            rect.Offset(-p.X, -p.Y);
            rect.Height = (int)this.Font.GetHeight() + 8;   // 缩小刷新高度

            this.tableLayoutPanel_content.Invalidate(rect, false);

            // this.tableLayoutPanel_content.Invalidate();
        }

        public void RemoveItem(EasyLine line, bool bFireEvent)
        {
            int index = this.Items.IndexOf(line);

            if (index == -1)
                return;

            line.RemoveFromTable(this.tableLayoutPanel_content, index);

            this.Items.Remove(line);

            // this.Changed = true;
            if (bFireEvent == true)
                this.FireTextChanged();
        }

        // parameters:
        //      bDispose    是否立即 Dispose 若干 Control 对象。如果为 false，则不 Dispose，而是返回给调用者
        public List<Control> ClearItems(bool bDispose = true)
        {
            List<Control> results = new List<Control>();
            if (this.Items != null)
            {
                foreach (EasyLine item in this.Items)
                {
                    if (item != null)
                    {
                        if (bDispose)
                            item.Dispose();
                        else
                            results.AddRange(item.RemoveControls());
                    }
                }

                this.Items.Clear();
            }

            return results;
        }
        /// <summary>
        /// 清除全部行
        /// </summary>
        public List<Control> Clear(bool bDispose = true)
        {
            List<Control> results = new List<Control>();
            this.DisableUpdate();
            try
            {
                for (int i = 0; i < this.tableLayoutPanel_content.RowStyles.Count; i++)
                {
                    for (int j = 0; j < this.tableLayoutPanel_content.ColumnStyles.Count; j++)
                    {
                        Control control = this.tableLayoutPanel_content.GetAnyControlAt(j, i);
                        if (control != null)
                            this.tableLayoutPanel_content.Controls.Remove(control);
                    }
                }

#if NO
                for (int i = 0; i < this.Items.Count; i++)
                {
                    EasyLine element = this.Items[i];
                    ClearOneItemControls(this.tableLayoutPanel_content,
                        element);
                }
#endif

                Debug.Assert(this.tableLayoutPanel_content.Controls.Count == 0, "");

                // this.Items.Clear();
                results = this.ClearItems(false);

                this.tableLayoutPanel_content.RowCount = 2;    // 为什么是2？

                for (; ; )
                {
                    if (this.tableLayoutPanel_content.RowStyles.Count <= 2)
                        break;
                    this.tableLayoutPanel_content.RowStyles.RemoveAt(2);
                }
            }
            finally
            {
                this.EnableUpdate();
            }

            // 留在最后统一 Dispose()，速度较快
            if (bDispose)
            {
                foreach (Control control in results)
                {
                    if (control != null)
                        control.Dispose();
                }
                results.Clear();
            }
            return results;
        }

#if NO
        // 清除一个Item对象对应的Control
        internal void ClearOneItemControls(
            TableLayoutPanel table,
            EasyLine line)
        {
            table.Controls.Remove(line.label_color);

            table.Controls.Remove(line.label_caption);

            if (line.splitter != null)
                table.Controls.Remove(line.splitter);

            table.Controls.Remove(line.textBox_content);
        }
#endif

        string _header = "";

        // 设置 MARC 记录内容
        public void SetMarc(string strMarc)
        {
            this.Clear();
            this.MarcDefDom = null;  // 迫使重新获得字段名提示信息

            this.DisableUpdate();
            try
            {
                int nLineIndex = 0;
                MarcRecord record = new MarcRecord(strMarc);
                this._header = record.Header.ToString();    // 头标区
                foreach (MarcField field in record.ChildNodes)
                {
                    FieldLine field_line = new FieldLine(this);
                    field_line.Name = field.Name;
                    field_line.Caption = GetCaption(field.Name, "", this._includeNumber);
                    // field_line.Indicator = field.Indicator;
                    InsertNewLine(nLineIndex++,
                        field_line,
                        false);
                    field_line.IsControlField = field.IsControlField;
                    if (field.IsControlField == true)
                    {
                        field_line.Content = field.Content;
                        field_line.ExpandState = ExpandState.None;
                    }
                    else
                    {
#if NO
                        // 指示符行
                        IndicatorLine indicator_line = new IndicatorLine(this);
                        indicator_line.Name = "";
                        indicator_line.Caption = "指示符";
                        indicator_line.Content = field.Indicator;
                        InsertNewLine(nLineIndex++,
                            indicator_line);
#endif
                        field_line.Indicator = field.Indicator;

                        foreach (MarcSubfield subfield in field.ChildNodes)
                        {
                            SubfieldLine subfield_line = new SubfieldLine(this);
                            subfield_line.Name = subfield.Name;
                            subfield_line.Caption = GetCaption(field.Name, subfield.Name, this._includeNumber);
                            subfield_line.Content = subfield.Content;
                            InsertNewLine(nLineIndex++,
                                subfield_line,
                                false);
                        }

                        field_line.ExpandState = ExpandState.Expanded;
                    }
                }

                this.Changed = false;
                ResetLineState();
            }
            finally
            {
                this.EnableUpdate();
            }
        }

        #region 插入子字段功能

        public string DefaultSubfieldName = "a";

        // 插入一个新的子字段
        public SubfieldLine NewSubfield(int index)
        {
            EasyLine ref_line = this.Items[index];
            string strFieldName = "";
            if (ref_line is FieldLine)
            {
                // 只能后插 TODO: 让对话框不能选择前插
                strFieldName = (ref_line as FieldLine).Name;
            }
            else
            {
                FieldLine ref_field_line = GetFieldLine(ref_line as SubfieldLine);
                if (ref_field_line == null)
                    return null;    // 应该抛出异常?
                strFieldName = ref_field_line.Name;
            }

            // 询问子字段名
            NewSubfieldDialog dlg = new NewSubfieldDialog();
            dlg.Font = this.Font;
            dlg.Text = "新子字段";
            dlg.AutoComplete = true;

            dlg.ParentNameString = strFieldName;
            dlg.NameString = this.DefaultSubfieldName;
            dlg.MarcDefDom = this.MarcDefDom;
            dlg.Lang = this.Lang;

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return null;

            if (ref_line is FieldLine)
            {
                // 只能后插
                dlg.InsertBefore = false;
            }

            string strDefaultValue = "";

            List<string> results = null;
            string strError = "";
            // 获得宏值
            // parameters:
            //      strSubFieldName 子字段名。特殊地，如果为"#indicator"，表示想获取该字段的指示符缺省值
            // return:
            //      -1  error
            //      0   not found 
            //      1   found
            int nRet = GetDefaultValue(
                0,  // index,
                dlg.ParentNameString,
                dlg.NameString,
                out results,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

            if (results != null
                && results.Count > 0)
            {
                strDefaultValue = results[0];
            }

            bool bChanged = false;

            SubfieldLine subfield_line = new SubfieldLine(this);
            subfield_line.Name = dlg.NameString;
            subfield_line.Caption = GetCaption(dlg.ParentNameString, dlg.NameString, this._includeNumber);
            subfield_line.Content = strDefaultValue;
            if (dlg.InsertBefore == true)
            {
                InsertNewLine(index++,
                    subfield_line,
                    false);
                bChanged = true;
            }
            else
            {
                index++;
                InsertNewLine(index++,
                    subfield_line,
                    false);
                bChanged = true;
            }

            if (bChanged == true)
                this.FireTextChanged();

#if DEBUG
            this.Verify();
#endif

            return subfield_line;
        }

        #endregion

        #region 插入字段功能

        public string DefaultFieldName = "???";

        // 插入一个新的字段
        public FieldLine NewField(int index)
        {
            EasyLine ref_line = null;

            if (index < this.Items.Count)
                ref_line = this.Items[index];

            if (ref_line is SubfieldLine)
            {
                ref_line = GetFieldLine(ref_line as SubfieldLine);
                if (ref_line == null)
                {
                    throw new Exception("index 为 " + index.ToString() + " 的子字段行没有找到字段行");
                }
                index = this.Items.IndexOf(ref_line);
                Debug.Assert(index != -1, "");
            }

            // 询问字段名
            NewSubfieldDialog dlg = new NewSubfieldDialog();
            GuiUtil.SetControlFont(dlg, this.Font);
            // dlg.Font = this.Font;

            dlg.Text = "新字段";
            dlg.AutoComplete = true;

            if (ref_line != null)
                dlg.NameString = ref_line.Name;
            else
                dlg.NameString = this.DefaultFieldName;
            dlg.MarcDefDom = this.MarcDefDom;
            dlg.Lang = this.Lang;

            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return null;

            bool bControlField = Record.IsControlFieldName(dlg.NameString);
            string strDefaultValue = "";
            string strIndicator = "  ";
            if (bControlField == false)
                strDefaultValue = new string((char)31, 1) + "a";

            List<string> results = null;
            string strError = "";
            // 获得宏值
            // parameters:
            //      strSubFieldName 子字段名。特殊地，如果为"#indicator"，表示想获取该字段的指示符缺省值
            // return:
            //      -1  error
            //      0   not found 
            //      1   found
            int nRet = GetDefaultValue(
                0,  // index,
                dlg.NameString,
                "",
                out results,
                out strError);
            if (nRet == -1)
                MessageBox.Show(this, strError);

            if (results != null
                && results.Count > 0)
            {
                strDefaultValue = results[0];
                if (bControlField == false)
                    SplitDefaultValue(strDefaultValue,
                        out strIndicator,
                        out strDefaultValue);
            }

            bool bChanged = false;

            // 辅助剖析子字段的对象
            MarcField field_node = new MarcField(dlg.NameString, strIndicator, strDefaultValue);

            FieldLine field_line = new FieldLine(this);
            field_line.IsControlField = bControlField;
            field_line.Name = dlg.NameString;
            field_line.Caption = GetCaption(dlg.NameString, "", this._includeNumber);
            if (bControlField == false)
            {
                field_line.Indicator = strIndicator;  // 从 marcdef 中获得缺省的指示符值
                field_line.ExpandState = ExpandState.Expanded;
            }

            EasyLine after_line = null;
#if FIX
            bool bExpanded = false;
            bool bHideChanged = false;
#endif
            if (dlg.InsertBefore == true)
            {
                after_line = ref_line;
            }
            else
            {
                // 如果插入点是字段行，需要跳过下属的子字段行
                // EasyLine ref_line = this.Items[index];
                if (ref_line is FieldLine)
                {
                    List<EasyLine> lines = GetSubfieldLines(ref_line as FieldLine);
                    index += lines.Count + 1;
                }
                else
                    index++;

                if (index < this.Items.Count)
                    after_line = this.Items[index];
            }

#if FIX
            if (after_line != null)
            {
                // 如果插入位置后面一个字段是隐藏状态，则会出现故障，需要先修改为显示状态，插入后再隐藏
                if (after_line is FieldLine && after_line.Visible == false)
                {
                    this.ToggleHide(after_line as FieldLine);
                    bHideChanged = true;
                }
                // 如果本字段是收缩状态，则会出现故障，需要先修改为展开状态，插入后再收缩
                if (after_line.ExpandState == ExpandState.Collapsed)
                {
                    this.ToggleExpand(after_line);
                    Debug.Assert(after_line.ExpandState == ExpandState.Expanded, "");
                    bExpanded = true;
                }
            }
#endif

            InsertNewLine(index++,
    field_line,
    false);
            bChanged = true;

            // 如果必要，创建子字段对象
            foreach (MarcSubfield subfield_node in field_node.ChildNodes)
            {
                SubfieldLine subfield_line = new SubfieldLine(this);
                subfield_line.Name = subfield_node.Name;
                subfield_line.Caption = GetCaption(field_node.Name, subfield_node.Name, this._includeNumber);
                subfield_line.Content = subfield_node.Content;
                InsertNewLine(index++,
                    subfield_line,
                    false);
                bChanged = true;
            }

#if FIX
            // 把参考行恢复到以前的状态
            if (after_line != null && bExpanded == true)
            {
                this.ToggleExpand(after_line);
                Debug.Assert(after_line.ExpandState == ExpandState.Collapsed, "");
            }
            if (after_line != null && bHideChanged == true)
            {
                this.ToggleHide(after_line as FieldLine);
            }
#endif
            if (bChanged == true)
                this.FireTextChanged();

            return field_line;
        }

        const char SUBFLD = (char)31;

        // 从缺省值字符串中分离出字段指示符和纯粹字段内容部分
        // 函数调用前，strText中可能含有指示符，也可能没有
        static void SplitDefaultValue(string strText,
            out string strIndicator,
            out string strContent)
        {
            strIndicator = "  ";
            strContent = "";

            if (string.IsNullOrEmpty(strText) == true)
                return;

            int nRet = strText.IndexOf(SUBFLD);
            if (nRet == -1)
            {
                if (strText.Length < 2)
                {
                    strContent = strText;
                    return;
                }

                strIndicator = strText.Substring(0, 2);
                strContent = strText.Substring(2);
                return;
            }

            if (nRet >= 2)
            {
                strIndicator = strText.Substring(0, 2);
                strContent = strText.Substring(2);
                return;
            }

            strContent = strText;
        }

        // 获得宏值
        // parameters:
        //      nPushIndex  需要实做的字符串事项的下标。如果为-1，表示没有要实做的事项(即全部都是模拟)
        //      strSubFieldName 子字段名。特殊地，如果为"#indicator"，表示想获取该字段的指示符缺省值
        // return:
        //      -1  error
        //      0   not found 
        //      >0  found 结果的个数
        private int GetDefaultValue(
            // bool bSimulate,
            int nPushIndex,
            string strFieldName,
            string strSubFieldName,
            out List<string> results,
            out string strError)
        {
            Debug.Assert(strFieldName != null, "strFieldName参数不能为null");
            Debug.Assert(strSubFieldName != null, "strSubFieldName参数不能为null");
            // Debug.Assert(strValue != null, "strValue参数不能为null");

            strError = "";
            results = new List<string>();

            // 检查MarcDefDom是否存在
            if (this.MarcDefDom == null)
            {
                strError = m_strMarcDomError;
                return -1;
            }

            // 根据字段名找到配置文件中的该字段的定义
            XmlNode node = null;

            if (strSubFieldName == "" || strSubFieldName == "#indicator")
            {
                // 只找到字段
                node = this.MarcDefDom.DocumentElement.SelectSingleNode("Field[@name='" + strFieldName + "']");
            }
            else
            {
                // 找到子字段
                node = this.MarcDefDom.DocumentElement.SelectSingleNode("Field[@name='" + strFieldName + "']/Subfield[@name='" + strSubFieldName + "']");
            }

            if (node == null)
            {
                return 0;   // not found def
            }

            XmlNodeList value_nodes = null;

            if (strSubFieldName == "#indicator")
            {

                value_nodes = node.SelectNodes("Property/Indicator/Property/DefaultValue");
            }
            else
            {
                value_nodes = node.SelectNodes("Property/DefaultValue");
            }

            if (value_nodes.Count == 0)
                return 0;

            for (int i = 0; i < value_nodes.Count; i++)
            {
                string strOutputValue = value_nodes[i].InnerText;

                // 去掉定义值中的\r\n或者单独的\r和\n。这种具有\r\n的效果可能由notepad中折行状态时paste到编辑配置文件对话框并保存来造成.
                strOutputValue = strOutputValue.Replace("\r", "");
                strOutputValue = strOutputValue.Replace("\n", "");

                // 子字段符号
                strOutputValue = strOutputValue.Replace("\\", new string((char)31, 1));

                ParseMacroEventArgs e = new ParseMacroEventArgs();
                e.Macro = strOutputValue;
                // e.Simulate = bSimulate;
                if (i == nPushIndex)
                    e.Simulate = false; // 实做
                else
                    e.Simulate = true;  // 模拟

                TemplateControl_ParseMacro((object)this, e);
                if (String.IsNullOrEmpty(e.ErrorInfo) == false)
                {
                    strError = e.ErrorInfo;
                    return -1;
                }

                strOutputValue = e.Value;

                /*
                strOutputValue = MacroTimeValue(strOutputValue);

                // 替换下划线
                strOutputValue = strOutputValue.Replace("_", " ");


                if (strSubFieldName == "")
                {
                    // 替换子字段符号
                    strOutputValue = strOutputValue.Replace('$', Record.SUBFLD);
                }
                 * */

                results.Add(strOutputValue);
            }

            return results.Count;
        }

        // 兑现宏
        void TemplateControl_ParseMacro(object sender, ParseMacroEventArgs e)
        {
            // 将一些基本的宏兑现
            // %year%%m2%%d2%%h2%%min2%%sec2%.%hsec%

            string strOutputValue = MacroTimeValue(e.Macro);

            // 替换下划线
            // 只替换前面连续的'_'
            // strOutputValue = strOutputValue.Replace("_", " ");

            // 替换字符串最前面一段连续的字符
            strOutputValue = StringUtil.ReplaceContinue(strOutputValue, '_', ' ');

            // 替换子字段符号
            strOutputValue = strOutputValue.Replace(Record.KERNEL_SUBFLD, Record.SUBFLD);   // $?

            e.Value = strOutputValue;
            e.ErrorInfo = "";

            // 如果是一般的宏, MARC编辑器控件就可以解决
            // 如果控件外围没有支持事件, 也只能这里解决部分
            if (e.Macro.IndexOf("%") == -1 || this.ParseMacro == null)
            {
                return;
            }
            else
            {
                // 否则还需要求助于宿主
                ParseMacroEventArgs e1 = new ParseMacroEventArgs();
                e1.Macro = e.Value; // 第一次处理过的, 再级联处理
                e1.Simulate = e.Simulate;
                this.ParseMacro(this, e1);

                e.Value = e1.Value;
                e.ErrorInfo = e1.ErrorInfo;
                return;
            }
        }

        /// <summary>
        /// 兑现时间宏值
        /// </summary>
        /// <param name="strMacro">要处理的宏字符串</param>
        /// <returns>兑现宏以后的字符串</returns>
        public static string MacroTimeValue(string strMacro)
        {
            DateTime time = DateTime.Now;

            // utime
            strMacro = strMacro.Replace("%utime%", time.ToString("u"));

            // 年 year
            strMacro = strMacro.Replace("%year%", Convert.ToString(time.Year).PadLeft(4, '0'));

            // 年 y2
            strMacro = strMacro.Replace("%y2%", time.Year.ToString().PadLeft(4, '0').Substring(2, 2));

            // 月 month
            strMacro = strMacro.Replace("%month%", Convert.ToString(time.Month));

            // 月 m2
            strMacro = strMacro.Replace("%m2%", Convert.ToString(time.Month).PadLeft(2, '0'));

            // 日 day
            strMacro = strMacro.Replace("%day%", Convert.ToString(time.Day));

            // 日 d2
            strMacro = strMacro.Replace("%d2%", Convert.ToString(time.Day).PadLeft(2, '0'));

            // 时 hour
            strMacro = strMacro.Replace("%hour%", Convert.ToString(time.Hour));

            // 时 h2
            strMacro = strMacro.Replace("%h2%", Convert.ToString(time.Hour).PadLeft(2, '0'));

            // 分 minute
            strMacro = strMacro.Replace("%minute%", Convert.ToString(time.Minute));

            // 分 min2
            strMacro = strMacro.Replace("%min2%", Convert.ToString(time.Minute).PadLeft(2, '0'));

            // 秒 second
            strMacro = strMacro.Replace("%second%", Convert.ToString(time.Second));

            // 秒 sec2
            strMacro = strMacro.Replace("%sec2%", Convert.ToString(time.Second).PadLeft(2, '0'));

            // 百分秒 hsec
            strMacro = strMacro.Replace("%hsec%", Convert.ToString(time.Millisecond / 100));

            // 毫秒 msec
            strMacro = strMacro.Replace("%msec%", Convert.ToString(time.Millisecond));


            return strMacro;
        }

        #endregion

        public string GetMarc()
        {
            MarcRecord record = new MarcRecord(this._header);

            MarcField current_field_node = null;
            foreach (EasyLine line in this.Items)
            {
                if (line is FieldLine)
                {
                    FieldLine field = line as FieldLine;

                    MarcField field_node = null;

                    if (field.IsControlField == true)
                        field_node = new MarcField(field.Name, "", field.Content);
                    else
                        field_node = new MarcField(field.Name, field.Indicator, "");

                    record.ChildNodes.add(field_node);
                    current_field_node = field_node;
                }
                else if (line is SubfieldLine)
                {
                    SubfieldLine subfield = line as SubfieldLine;

                    MarcSubfield subfield_node = new MarcSubfield(subfield.Name, subfield.Content);
                    current_field_node.ChildNodes.add(subfield_node);
                }
            }

            return record.Text;
        }

        void RefreshCaption()
        {
            string strCurrentFieldName = "";
            foreach (EasyLine line in this.Items)
            {
                if (line is FieldLine)
                {
                    FieldLine field = line as FieldLine;
                    line.Caption = GetCaption(line.Name, "", this._includeNumber);
                    strCurrentFieldName = line.Name;
                }
                else if (line is SubfieldLine)
                {
                    SubfieldLine subfield = line as SubfieldLine;
                    line.Caption = GetCaption(strCurrentFieldName, subfield.Name, this._includeNumber);
                }
            }
        }

        #region marcdef

        /// <summary>
        /// 获得配置文件的 XmlDocument 对象
        /// </summary>
        public event GetConfigDomEventHandle GetConfigDom = null;

        XmlDocument m_domMarcDef = null;
        string m_strMarcDomError = "";

        /// <summary>
        /// 存储了 MARC 结构定义的 XmlDocument 对象
        /// </summary>
        public XmlDocument MarcDefDom
        {
            get
            {
                if (this.m_domMarcDef != null)
                    return this.m_domMarcDef;

                if (m_strMarcDomError != "")
                    return null;    // 避免反复报错

                string strError = "";

                GetConfigDomEventArgs e = new GetConfigDomEventArgs();
                e.Path = "marcdef";
                e.XmlDocument = null;
                if (this.GetConfigDom != null)
                {
                    this.GetConfigDom(this, e);
                }
                else
                {
                    //throw new Exception("GetConfigFile事件尚未初始化");
                    return null;
                }

                if (string.IsNullOrEmpty(e.ErrorInfo) == false)
                {
                    strError = "获取marcdef dom出错，原因:" + e.ErrorInfo;
                    goto ERROR1;
                }

                this.m_domMarcDef = e.XmlDocument;
                return this.m_domMarcDef;
            ERROR1:
                m_strMarcDomError = strError;
                return null;
            }
            set
            {
                this.m_strMarcDomError = "";
                this.m_domMarcDef = value;
            }
        }

        /// <summary>
        /// 当前界面语言代码
        /// </summary>
        public string Lang = "zh";

        bool _includeNumber = false;

        public bool IncludeNumber
        {
            get
            {
                return this._includeNumber;
            }
            set
            {
                if (this._includeNumber != value)
                {
                    this._includeNumber = value;
                    // 刷新 Caption
                    this.RefreshCaption();
                }
            }
        }


        // 从配置信息中得到一个字段的指定语言版本的标签名称
        // parameters:
        //		strFieldName	字段名
        // return:
        //		如果找不到则返回原始字段名或者子字段名；找到返回具体的标签信息
        public string GetCaption(string strFieldName,
            string strSubfieldName,
            bool bIncludeNumber)
        {
            string strDefault = "";
            if (string.IsNullOrEmpty(strSubfieldName) == false)
                strDefault = strSubfieldName;
            else
                strDefault = strFieldName;

            if (this.MarcDefDom == null)
                return strDefault;

            XmlNode nodeProperty = null;

            if (string.IsNullOrEmpty(strSubfieldName) == true)
                nodeProperty = this.MarcDefDom.DocumentElement.SelectSingleNode("Field[@name='" + strFieldName + "']/Property");
            else
                nodeProperty = this.MarcDefDom.DocumentElement.SelectSingleNode("Field[@name='" + strFieldName + "']/Subfield[@name='" + strSubfieldName + "']/Property");
            if (nodeProperty == null)
                return strDefault;

            // 从一个元素的下级的多个<strElementName>元素中, 提取语言符合的XmlNode的InnerText
            // parameters:
            //      bReturnFirstNode    如果找不到相关语言的，是否返回第一个<strElementName>
            string strValue = DomUtil.GetXmlLangedNodeText(
        this.Lang,
        nodeProperty,
        "Label",
        true);
            if (String.IsNullOrEmpty(strValue) == true)
                strValue = strDefault;

            if (bIncludeNumber == false)
                return strValue;

            if (string.IsNullOrEmpty(strSubfieldName) == false)
                return strSubfieldName + " " + strValue;
            return strFieldName + " " + strValue;
        }

        #endregion

        int m_nInSuspend = 0;

        public void DisableUpdate()
        {
            if (this.m_nInSuspend == 0)
            {
                this.tableLayoutPanel_content.SuspendLayout();
            }

            this.m_nInSuspend++;
        }

        // parameters:
        //      bOldVisible 如果为true, 表示真的要结束
        public void EnableUpdate()
        {
            this.m_nInSuspend--;

            if (this.m_nInSuspend == 0)
            {
                this.tableLayoutPanel_content.ResumeLayout(false);
                this.tableLayoutPanel_content.PerformLayout();
            }
        }

        internal static int RESERVE_LINES = 0;

        public void InsertNewLine(int index,
            EasyLine line,
            bool bFireEnvent)
        {
            this.DisableUpdate();   // 防止闪动

            try
            {
                RowStyle style = new RowStyle();
                //style.Height = 26;
                //style.SizeType = SizeType.Absolute;

                this.tableLayoutPanel_content.RowStyles.Insert(index + RESERVE_LINES, style);
                this.tableLayoutPanel_content.RowCount += 1;

                line.InsertToTable(this.tableLayoutPanel_content, index);

                this.Items.Insert(index, line);

                line.State = ItemState.New;

                if (bFireEnvent == true)
                    this.FireTextChanged();
            }
            finally
            {
                this.EnableUpdate();
            }
        }

        private void EasyMarcControl_SizeChanged(object sender, EventArgs e)
        {
            this.tableLayoutPanel_content.Width = this.Width - SystemInformation.VerticalScrollBarWidth;
            ResetAllTextBoxHeight();
        }

        // 重新调整所有 TextBox 的高度
        void ResetAllTextBoxHeight()
        {
            // 如果没有这一句，在不断宽/窄变换 tablelayoutpanel 宽度的时候，内容区下面的空白区域会逐渐变大
            this.tableLayoutPanel_content.Height = 0;

            this.DisableUpdate();   // 防止闪动
            try
            {
                foreach (EasyLine item in this.Items)
                {
                    if (item != null && item.textBox_content != null
                        && item.textBox_content.Visible == true)
                    {
                        item.textBox_content.SetHeight();
                    }
                }
            }
            finally
            {
                this.EnableUpdate();
            }

            // TODO: 重新设置 tablelayout 的高度
        }

        // 字段背景颜色
        public Color FieldBackColor = Color.FromArgb(230, 230, 230);
        // 子字段背景颜色
        public Color SubfieldBackColor = Color.FromArgb(240, 240, 240);
        // 扩展区背景颜色1
        public Color ExpandBackColor1 = Color.FromArgb(230, 230, 230);
        // 扩展区背景颜色2
        public Color ExpandBackColor2 = Color.FromArgb(255, 255, 255);
        // 左边颜色块的背景色，横线颜色
        public Color LeftBackColor = Color.FromArgb(225, 225, 225);
        // 子字段名的前景颜色
        public Color SubfieldCaptionForeColor = SystemColors.GrayText;
        // 字段名的前景颜色
        public Color FieldCaptionForeColor = Color.DarkGreen;
        // 正在编辑的 edit 的背景颜色
        public Color FocusedEditBackColor = Color.FromArgb(200, 200, 255);

        public void SetColorStyle(string strStyle)
        {
            if (strStyle == "dark")
            {
                // 字段背景颜色
                this.FieldBackColor = Color.FromArgb(0, 0, 0);
                // 子字段背景颜色
                this.SubfieldBackColor = Color.FromArgb(50, 50, 50);
                // 扩展区背景颜色1
                this.ExpandBackColor1 = Color.FromArgb(60, 60, 60);
                // 扩展区背景颜色2
                this.ExpandBackColor2 = Color.FromArgb(100, 100, 100);

                this.LeftBackColor = Color.FromArgb(80, 80, 80);

                this.SubfieldCaptionForeColor = Color.FromArgb(200, 200, 180);

                this.FieldCaptionForeColor = Color.FromArgb(200, 200, 200);

                this.FocusedEditBackColor = ControlPaint.Dark(this.BackColor);

                this.RefreshLineColor(true);
                this.Invalidate();
            }
            if (strStyle == "light")
            {
                // 字段背景颜色
                FieldBackColor = Color.FromArgb(230, 230, 230);
                // 子字段背景颜色
                SubfieldBackColor = Color.FromArgb(240, 240, 240);
                // 扩展区背景颜色1
                ExpandBackColor1 = Color.FromArgb(230, 230, 230);
                // 扩展区背景颜色2
                ExpandBackColor2 = Color.FromArgb(255, 255, 255);
                // 左边颜色块的背景色，横线颜色
                LeftBackColor = Color.FromArgb(225, 225, 225);
                // 子字段名的前景颜色
                SubfieldCaptionForeColor = SystemColors.GrayText;
                // 字段名的前景颜色
                FieldCaptionForeColor = Color.DarkGreen;
                // 正在编辑的 edit 的背景颜色
                FocusedEditBackColor = Color.FromArgb(200, 200, 255);

                this.RefreshLineColor(true);
                this.Invalidate();
            }
        }

        private void tableLayoutPanel_content_Paint(object sender, PaintEventArgs e)
        {
            LinearGradientBrush brushGradient = null;
            try
            {
#if NO
            int nLineLength = (int)(this.tableLayoutPanel_content.ColumnStyles[0].Width
                + this.tableLayoutPanel_content.ColumnStyles[1].Width);
#endif
                int nLineLength = 0;

                // 字段背景颜色
                using (Brush brush = new SolidBrush(this.FieldBackColor)) // Color.FromArgb(230, 230, 230)
                // 子字段背景颜色
                using (Brush brushSubfield = new SolidBrush(this.SubfieldBackColor)) // Color.FromArgb(240, 240, 240)
                using (Pen pen = new Pen(this.LeftBackColor)) // Color.FromArgb(225, 225, 225)
                {

                    Point p = this.tableLayoutPanel_content.PointToScreen(new Point(0, 0));

                    // float y = row_heights[0];   // +this.AutoScrollPosition.Y + this.tableLayoutPanel_content.Location.Y;
                    for (int i = 0; i < this.Items.Count; i++)
                    {
                        EasyLine item = this.Items[i];

                        if (item.Visible == true)
                        {
#if NO
                        // textbox 有时候是隐藏状态 X 为 0
                        if (nLineLength == 0)
                            nLineLength = item.textBox_content.Location.X - item.textBox_content.Margin.Left;
#endif
                            if (nLineLength == 0)
                                nLineLength = item.splitter.Location.X;

                            Rectangle rect = item.label_color.RectangleToScreen(item.label_color.ClientRectangle);
                            rect.Width = nLineLength;   //  this.tableLayoutPanel_content.DisplayRectangle.Width;
                            rect.Offset(-p.X, -p.Y);
                            // rect.Height = (int)this.Font.GetHeight() + 8;

                            if (e.ClipRectangle.IntersectsWith(rect) == false)
                                continue;

                            if (item is FieldLine)
                            {
                                if (item.ExpandState != ExpandState.None)
                                {
                                    if (brushGradient == null)
                                        brushGradient = new LinearGradientBrush(
                    new PointF(0, 0),
                    new PointF(nLineLength, 0),
                    this.ExpandBackColor1,  // Color.FromArgb(230, 230, 230)
                    this.ExpandBackColor2  // Color.FromArgb(255, 255, 255)
                    );
                                    e.Graphics.FillRectangle(brushGradient, rect);
                                }
                                else
                                    e.Graphics.FillRectangle(brush, rect);

                                {
                                    Point pt1 = new Point(rect.X, rect.Y);
                                    Point pt2 = new Point(rect.X + rect.Width, rect.Y);

                                    e.Graphics.DrawLine(pen, pt1, pt2);
                                }
                            }
                            else
                            {
                                // 子字段
                                e.Graphics.FillRectangle(brushSubfield, rect);

                                // 编辑区的横线
                                {
                                    Point pt1 = new Point(rect.Right + 10, rect.Y);
                                    Point pt2 = new Point(this.ClientSize.Width, rect.Y);

                                    e.Graphics.DrawLine(pen, pt1, pt2);
                                }
                            }
                        }
                        // y += height;
                    }
                }
            }
            finally
            {
                if (brushGradient != null)
                    brushGradient.Dispose();
            }
        }

        internal bool m_bFocused = false;

        private void EasyMarcControl_Enter(object sender, EventArgs e)
        {
            this.tableLayoutPanel_content.Focus();
            this.m_bFocused = true;
            this.RefreshLineColor();
        }

        private void EasyMarcControl_Leave(object sender, EventArgs e)
        {
            this.m_bFocused = false;
            this.RefreshLineColor();
        }

        #region 定长模板

        /// <summary>
        /// 获取一个特定模板的XML定义
        /// </summary>
        public event GetTemplateDefEventHandler GetTemplateDef = null;  // 外部接口，获取一个特定模板的XML定义

        // 通过模板取固定字段的值
        // parameter:
        //		strFieldName	字段名称
        //		strSubFieldName	子字段名称 如果为空表示获得字段的定长模板
        // return:
        //		-1	出错
        //		0	没找到 可能是模板文件不存在，或者对应的配置事项不存在
        //		1	找到
        private int GetValueFromTemplate(string strFieldName,
            string strSubFieldName,
            string strValue,
            out string strOutputValue,
            out string strError)
        {
            Debug.Assert(strFieldName != null, "GetValueFromTemplate()，strFieldName 参数不能为 null");
            Debug.Assert(strSubFieldName != null, "GetValueFromTemplate()，strSubFieldName 参数不能为 null");
            Debug.Assert(strValue != null, "GetValueFromTemplate()，strValue 参数不能为 null");

            strError = "";
            strOutputValue = strValue;

            // 检查MarcDefDom是否存在
            if (this.MarcDefDom == null)
            {
                strError = this.m_strMarcDomError;
                return 0;   // 2008/3/19恢复。 原来为什么要注释掉?
            }

            XmlNode nodeDef = null;
            string strTitle = "";

            // 首先尝试从外部接口获得模板定义
            if (this.GetTemplateDef != null)
            {
                GetTemplateDefEventArgs e = new GetTemplateDefEventArgs();
                e.FieldName = strFieldName;
                e.SubfieldName = strSubFieldName;
                e.Value = strValue;

                this.GetTemplateDef(this, e);
                if (string.IsNullOrEmpty(e.ErrorInfo) == false)
                {
                    strError = "在通过外部接口获取字段名为 '" + strFieldName + "' 子字段名为 '" + strSubFieldName + "' 的模板定义XML时出错: \r\n" + e.ErrorInfo;
                    return -1;
                }
                if (e.Canceled == false)
                {
                    nodeDef = e.DefNode;
                    strTitle = e.Title;
                    goto FOUND;
                }

                // e.Canceled == true 表示希望MarcEditor来自己获得定义
            }

            // *** MarcEditor来自己获得定义
            // 根据字段名找到配置文件中的该字段的定义
            XmlNodeList nodes = null;
            if (strSubFieldName == "")
                nodes = this.MarcDefDom.DocumentElement.SelectNodes("Field[@name='" + strFieldName + "']");
            else
                nodes = this.MarcDefDom.DocumentElement.SelectNodes("Field[@name='" + strFieldName + "']/Subfield[@name='" + strSubFieldName + "']");

            if (nodes.Count == 0)
            {
                strError = "MARC定义文件中没有找到字段名为 '" + strFieldName + "' 子字段名为 '" + strSubFieldName + "' 的字段/子字段 定义";
                return 0;
            }

            if (nodes.Count > 1)
            {
                List<string> lines = new List<string>();
                int i = 0;
                foreach (XmlNode node in nodes)
                {
                    string strType = DomUtil.GetAttr(node, "type");
                    lines.Add((i + 1).ToString() + ") " + strType);
                    i++;
                }

                SelectListStringDialog select_def_dlg = new SelectListStringDialog();
                GuiUtil.AutoSetDefaultFont(select_def_dlg);

                select_def_dlg.Text = "请选择模板定义";
                select_def_dlg.Values = lines;
                select_def_dlg.Comment = "MARC定义文件中发现 字段名为 '" + strFieldName + "' 子字段名为 '" + strSubFieldName + "' 的模板定义有 " + nodes.Count.ToString() + " 处。\r\n\r\n请选择其中一个";
                select_def_dlg.StartPosition = FormStartPosition.CenterScreen;
                select_def_dlg.ShowDialog(this);
                if (select_def_dlg.DialogResult == DialogResult.Cancel)
                {
                    strError = "放弃选择模板定义";
                    return 0;
                }
                Debug.Assert(select_def_dlg.SelectedIndex != -1, "");
                nodeDef = nodes[select_def_dlg.SelectedIndex];
#if NO
                strTitle = "定长模板 : " + strFieldName
                    + (string.IsNullOrEmpty(strSubFieldName) == false ? new String(Record.KERNEL_SUBFLD, 1) + strSubFieldName : "")
                    + " -- " + select_def_dlg.SelectedValue;
#endif
                strTitle = strFieldName
                    + (string.IsNullOrEmpty(strSubFieldName) == false ? "$" + strSubFieldName : "")
                    + " " + GetCaption(strFieldName, strSubFieldName, false) + " -- " + select_def_dlg.SelectedValue;
            }
            else
            {
                nodeDef = nodes[0];
            }
        FOUND:
            FixedTemplateDlg dlg = new FixedTemplateDlg();
            // GuiUtil.AutoSetDefaultFont(dlg);
            if (string.IsNullOrEmpty(strTitle) == false)
                dlg.Text = "定长模板 : " + strTitle;
            else
            {
#if NO
            dlg.Text = "定长模板 : " + strFieldName
                + (string.IsNullOrEmpty(strSubFieldName) == false ? new String(Record.KERNEL_SUBFLD, 1) + strSubFieldName : "");
        
#endif
                dlg.Text = "定长模板 : "
                    + strFieldName
                    + (string.IsNullOrEmpty(strSubFieldName) == false ? "$" + strSubFieldName : "")
                    + " " + GetCaption(strFieldName, strSubFieldName, false);
            }

            dlg.TemplateControl.GetConfigDom += this.GetConfigDom;
            dlg.GetTemplateDef += new GetTemplateDefEventHandler(dlg_GetTemplateDef);
            dlg.TemplateControl.ParseMacro += new ParseMacroEventHandler(TemplateControl_ParseMacro);

            dlg.TemplateControl.MarcDefDom = this.MarcDefDom;
            dlg.TemplateControl.Lang = this.Lang;
            dlg.StartPosition = FormStartPosition.CenterScreen;

            int nRet = dlg.Initial(nodeDef,
                this.Lang,
                strValue,
                out strError);
            if (nRet == 0)
                return 0;
            if (nRet == -1)
                return -1;

            dlg.ShowDialog(this);
#if NO
            dlg.TemplateControl.ParseMacro -= new ParseMacroEventHandler(TemplateControl_ParseMacro);
            dlg.TemplateControl.GetConfigDom -= this.GetConfigDom;
#endif

            if (dlg.DialogResult == DialogResult.OK)
            {
                strOutputValue = dlg.TemplateControl.Value;
            }

            return 1;
        }

        void dlg_GetTemplateDef(object sender, GetTemplateDefEventArgs e)
        {
            if (this.GetTemplateDef != null)
                this.GetTemplateDef(sender, e);
        }

        public void HeaderGetValueFromTemplate()
        {
            string strError;
            int nRet = 0;

            string strOutputValue;
            // 获取字段的定长模板
            nRet = this.GetValueFromTemplate("###",
                "",
                this._header,
                out strOutputValue,
                out strError);
            if (nRet == -1)
            {
                MessageBox.Show(this, strError);
                return;
            }

            if (nRet == 0)
                return;

            if (this._header != strOutputValue)
            {
                this._header = strOutputValue;
                this.Changed = true;
            }
        }

        /// <summary>
        /// 打开当前按位置的定长模板对话框
        /// </summary>
        public void GetValueFromTemplate(EasyLine line)
        {
            string strError;
            int nRet = 0;

            string strFieldName = "";
            string strFieldValue = "";

            string strSubfieldName = "";
            string strSubfieldValue = "";

            if (line is FieldLine)
            {
                strFieldName = line.Name;
                strFieldValue = line.Content;   // TODO: 如果遇到不是控制字段的，是否要合成字段内容?
            }
            else
            {
                FieldLine field = this.GetFieldLine(line as SubfieldLine);
                strFieldName = field.Name;
                strSubfieldName = line.Name;
                strSubfieldValue = line.Content;
            }

            // int nSubfieldDupIndex = 0;

            string strOutputValue;
            if (strSubfieldName == "")
            {
                // 获取字段的定长模板
                nRet = this.GetValueFromTemplate(strFieldName,
                    "",
                    strFieldValue,
                    out strOutputValue,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }

                if (nRet == 0)
                    return;

                // 此处应使用Value
                if (strFieldValue != strOutputValue)
                    line.Content = strOutputValue;

                // 不让小edit全选上
                line.textBox_content.SelectionLength = 0;
            }
            else
            {
                int nOldSelectionStart = line.textBox_content.SelectionStart;

                nRet = this.GetValueFromTemplate(strFieldName,
                    strSubfieldName,
                    strSubfieldValue,
                    out strOutputValue,
                    out strError);
                if (nRet == -1)
                {
                    MessageBox.Show(this, strError);
                    return;
                }

                // 不是定长子字段的情况
                if (nRet == 0)
                    return;

                if (strSubfieldValue != strOutputValue)
                {
                    line.Content = strOutputValue;
                }

                // 不让小edit全选上
                if (nOldSelectionStart < line.textBox_content.Text.Length)
                    line.textBox_content.SelectionStart = nOldSelectionStart;
                else
                    line.textBox_content.SelectionLength = 0;
            }
        }

        // 探测当前位置是否存在定长模板定义
        // parameters:
        //      strCurName  返回当前所在位置的字段、子字段名
        internal bool HasTemplateOrValueListDef(
            EasyLine line,
            string strDefName,
            out string strCurName)
        {
            strCurName = "";

            string strFieldName = "";
            string strSubfieldName = "";

            if (line is FieldLine)
            {
                strFieldName = line.Name;
                strCurName = strFieldName;
            }
            else
            {
                FieldLine field = this.GetFieldLine(line as SubfieldLine);
                strFieldName = field.Name;
                strSubfieldName = line.Name;

                strCurName = strFieldName + new String(Record.KERNEL_SUBFLD, 1) + strSubfieldName;
            }

            string strError;
            return HasTemplateOrValueListDef(
                strDefName,
                strFieldName,
                strSubfieldName,
                out strError);
        }

        private bool HasTemplateOrValueListDef(
    string strDefName,
    string strFieldName,
    string strSubFieldName,
    out string strError)
        {
            Debug.Assert(strFieldName != null, "strFieldName参数不能为null");
            Debug.Assert(strSubFieldName != null, "strSubFieldName参数不能为null");

            strError = "";

            // 检查MarcDefDom是否存在
            if (this.MarcDefDom == null)
            {
                strError = m_strMarcDomError;
                return false;
            }

            // 根据字段名找到配置文件中的该字段的定义
            XmlNode node = null;

            if (string.IsNullOrEmpty(strSubFieldName) == true || strSubFieldName == "#indicator")
            {
                // 只找到字段
                node = this.MarcDefDom.DocumentElement.SelectSingleNode("Field[@name='" + strFieldName + "']");
            }
            else
            {
                // 找到子字段
                node = this.MarcDefDom.DocumentElement.SelectSingleNode("Field[@name='" + strFieldName + "']/Subfield[@name='" + strSubFieldName + "']");
            }

            if (node == null)
                return false;   // not found def

            XmlNodeList nodes = null;

            if (strDefName == "template")
            {
                // 下级有至少一个<Char>定义
                nodes = node.SelectNodes("Char");
            }
            else if (strDefName == "valuelist")
            {
                // 下级有<ValueList>定义
                nodes = node.SelectNodes("Property/ValueList");
            }
            else
            {
                throw new Exception("strDefName 值应当为 'template' 和 'valuelist' 之一。");
            }

            if (nodes.Count >= 1)
                return true;

            return false;
        }

        #endregion

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Debug.WriteLine("keyData=" + keyData.ToString());
            if (keyData == (Keys.M | Keys.Control))
            {
                if (this.SelectedItems.Count > 0)
                    this.GetValueFromTemplate(this.SelectedItems[0]);
                return true;
            }
            return false;
        }

        internal void DoMouseWheel(MouseEventArgs e)
        {
            int nValue = this.tableLayoutPanel_content.VerticalScroll.Value;
            nValue -= e.Delta;
            if (nValue > this.tableLayoutPanel_content.VerticalScroll.Maximum)
                nValue = this.tableLayoutPanel_content.VerticalScroll.Maximum;
            if (nValue < this.tableLayoutPanel_content.VerticalScroll.Minimum)
                nValue = this.tableLayoutPanel_content.VerticalScroll.Minimum;

            if (this.tableLayoutPanel_content.VerticalScroll.Value != nValue)
            {
                this.tableLayoutPanel_content.VerticalScroll.Value = nValue;
                this.tableLayoutPanel_content.PerformLayout();
            }
        }
    }

    // 字段行
    public class FieldLine : EasyLine
    {
        public FieldLine(EasyMarcControl container)
            : base(container)
        {
            // this.textBox_content.ReadOnly = true;

            this.label_caption.Font = new Font(this.Container.Font, FontStyle.Bold);
            this.label_caption.ForeColor = this.Container.FieldCaptionForeColor;    // Color.DarkGreen;

            this.textBox_content.KeyPress -= new KeyPressEventHandler(textBox_content_KeyPress);
            this.textBox_content.KeyPress += new KeyPressEventHandler(textBox_content_KeyPress);

            this.textBox_content.KeyDown -= new KeyEventHandler(textBox_content_KeyDown);
            this.textBox_content.KeyDown += new KeyEventHandler(textBox_content_KeyDown);
        }

        public override void SetLineColor(bool bSetAll = false)
        {
            base.SetLineColor(bSetAll);

            if (bSetAll == true)
            {
                this.label_caption.ForeColor = Container.FieldCaptionForeColor;
            }
        }

        bool _isControlField = false;
        public bool IsControlField
        {
            get
            {
                return _isControlField;
            }
            set
            {
                _isControlField = value;
                // this.textBox_content.ReadOnly = !value;
                if (value == true)
                {
                    this.textBox_content.BorderStyle = BorderStyle.None;
                    this.textBox_content.Dock = DockStyle.Fill;
                    this.textBox_content.MaxLength = 0;
                    this._bOverwrite = false;
                    this.textBox_content.Font = this.Container.Font;

                    textBox_content.MinimumSize = new Size(80, 21); // 23
                    textBox_content.Size = new Size(80, 21); // 23

                    this.textBox_content.Visible = true;
                }
                else
                {
                    this.textBox_content.BorderStyle = BorderStyle.FixedSingle;
                    this.textBox_content.Dock = DockStyle.Left;
                    this.textBox_content.MaxLength = 2;
                    this._bOverwrite = true;
                    this.textBox_content.Font = this.Container.FixedFont;

                    textBox_content.MinimumSize = new Size(20, 21); // 23
                    textBox_content.Size = new Size(20, 21); // 23

                    this.textBox_content.Visible = !this.Container.HideIndicator;
                }
            }
        }

        public string Indicator
        {
            get
            {
                if (_isControlField == true)
                    throw new Exception("控制字段不能获取 Indicator");
                return this.textBox_content.Text;
            }
            set
            {
                if (_isControlField == true)
                    throw new Exception("控制字段不能设置 Indicator");

                this.textBox_content.Text = value;
            }
        }

        bool _bOverwrite = false;

        void textBox_content_KeyPress(object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar)
            {
                case (char)Keys.Back:
                    {
                        if (this._bOverwrite == true)
                        {
                            e.Handled = true;
                            Console.Beep();
                            return;
                        }
                    }
                    break;
                default:
                    {
                        if (this._bOverwrite == true)
                        {
                            if ((Control.ModifierKeys == Keys.Control)
                                // || Control.ModifierKeys == Keys.Shift
                                || Control.ModifierKeys == Keys.Alt)
                            {
                                break;
                            }
                            int nOldSelectionStart = this.textBox_content.SelectionStart;
                            if (nOldSelectionStart < this.textBox_content.Text.Length)
                            {
                                if (this.textBox_content.Text.Length >= this.textBox_content.MaxLength) // 2009/3/6 changed
                                {
                                    this.textBox_content.Text = this.textBox_content.Text.Remove(this.textBox_content.SelectionStart, 1 + (this.textBox_content.Text.Length - this.textBox_content.MaxLength));
                                    this.textBox_content.SelectionStart = nOldSelectionStart;
                                }
                            }
                            else
                            {
                                Console.Beep(); // 表示拒绝了输入的字符
                            }
                        }
                    }
                    break;
            }

        }

        void textBox_content_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Delete:
                    {
                        if (this._bOverwrite == true)
                        {
                            // 在 字段名 或 指示符 位置
                            int nStart = this.textBox_content.SelectionStart;
                            if (nStart < this.textBox_content.MaxLength)
                            {
                                this.textBox_content.Text = this.textBox_content.Text.Substring(0, nStart) + " " + this.textBox_content.Text.Substring(nStart + 1);
                                this.textBox_content.SelectionStart = nStart;
                                e.Handled = true;
                            }
                            return;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        public override bool Visible
        {
            get
            {
                return base.Visible;
            }
            set
            {
                this.label_color.Visible = value;
                this.label_caption.Visible = value;
                if (this.splitter != null)
                    this.splitter.Visible = value;
                if (this.Container.HideIndicator == false
                    || this._isControlField == true
                    )
                    this.textBox_content.Visible = value;
            }
        }
    }

    // 子字段行
    public class SubfieldLine : EasyLine
    {
        public SubfieldLine(EasyMarcControl container)
            : base(container)
        {
            this.label_caption.TextAlign = ContentAlignment.TopRight;
            // this.label_caption.BackColor = SystemColors.Window;
            this.label_caption.ForeColor = container.SubfieldCaptionForeColor;    // SystemColors.GrayText;
        }

        public override void SetLineColor(bool bSetAll = false)
        {
            base.SetLineColor(bSetAll);

            if (bSetAll == true)
            {
                this.label_caption.ForeColor = Container.SubfieldCaptionForeColor;
            }
        }
    }

#if NO
    // 指示符行
    public class IndicatorLine : EasyLine
    {
        public IndicatorLine(EasyMarcControl container)
            : base(container)
        {
            this.label_caption.TextAlign = ContentAlignment.MiddleRight;
        }
    }
#endif

    public enum ExpandState
    {
        None = 0,
        Expanded = 1,
        Collapsed = 2,
    }

    /// <summary>
    /// 视觉行基类
    /// </summary>
    public class EasyLine : IDisposable
    {
        public EasyMarcControl Container = null;

        public object Tag = null;   // 用于存放需要连接的任意类型对象

        // 颜色、popupmenu
        public Label label_color = null;

        public Label label_caption = null;

        public Splitter splitter = null;

        public AutoHeightTextBox textBox_content = null;

        ItemState m_state = ItemState.Normal;

        static void DisposeControl(ref Control control)
        {
            if (control != null)
            {
                control.Dispose();
                control = null;
            }
        }

        void DisposeChildControls()
        {
            List<Control> controls = RemoveControls();
            foreach (Control control in controls)
            {
                if (control != null)
                    control.Dispose();
            }
            Container = null;
        }

        internal List<Control> RemoveControls()
        {
            AddEvents(false);

            List<Control> controls = new List<Control>();

            controls.Add(this.label_color);
            this.label_color = null;

            controls.Add(label_caption);
            label_caption = null;

            controls.Add(splitter);
            splitter = null;

            controls.Add(textBox_content);
            textBox_content = null;

            return controls;
        }

        #region 释放资源

#if NO
        // TODO: 容易造成 mem leak。建议用 Dispose() 改写
        ~EasyLine()
        {
            Dispose(false);
        }
#endif

        private bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            // Take yourself off the Finalization queue 
            // to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    // release managed resources if any
                    // AddEvents(false);
                    DisposeChildControls();
                }

                // release unmanaged resource

                // Note that this is not thread safe.
                // Another thread could start disposing the object
                // after the managed resources are disposed,
                // but before the disposed flag is set to true.
                // If thread safety is necessary, it must be
                // implemented by the client.
            }
            disposed = true;
        }

        #endregion

        // 字段名或者子字段名
        string _name = "";
        public string Name
        {
            get
            {
                return this._name;
            }
            set
            {
                this._name = value;
            }
        }

        public ExpandState ExpandState
        {
            get
            {
                if (this.label_color.ImageIndex == -1)
                    return EasyMarc.ExpandState.None;
                if (this.label_color.ImageIndex == 0)
                    return EasyMarc.ExpandState.Collapsed;
                return EasyMarc.ExpandState.Expanded;
            }
            set
            {
                if (value == EasyMarc.ExpandState.None)
                    this.label_color.ImageIndex = -1;
                else if (value == EasyMarc.ExpandState.Collapsed)
                    this.label_color.ImageIndex = 0;
                else
                    this.label_color.ImageIndex = 1;
            }
        }

        public int Height
        {
            get
            {
                int nHeight = this.label_caption.Height;
                if (nHeight < this.textBox_content.Height)
                    nHeight = this.textBox_content.Height;
                return nHeight;
            }
        }

        // TODO: 首次设置颜色也尽量调用 SetLineColor() 实现，这样可以共享代码，减少多处修改带来的麻烦
        public EasyLine(EasyMarcControl container)
        {
            this.Container = container;
            // int nTopBlank = (int)this.Container.Font.GetHeight() + 2;

            label_color = new Label();
            label_color.Dock = DockStyle.Fill;
            label_color.Size = new Size(6, 23);
            label_color.Margin = new Padding(0, 0, 0, 0);

            label_color.ImageList = this.Container.ImageListIcons;
            label_color.ImageIndex = -1;

            label_color.BackColor = this.Container.LeftBackColor;

            label_caption = new Label();
            label_caption.Dock = DockStyle.Fill;
            label_caption.Size = new Size(6, 23);
            label_caption.AutoSize = true;
            label_caption.Margin = new Padding(4, 6, 4, 4); // new Padding(4, 2, 4, 0)
            label_caption.TextAlign = ContentAlignment.TopLeft;

            // label_caption.BackColor = SystemColors.Control;

            label_caption.BackColor = Color.Transparent;
            // label_caption.ForeColor = this.Container.ForeColor;

            splitter = new TransparentSplitter();
            // splitter.Dock = DockStyle.Fill;
            splitter.Size = new Size(8, 23);
            splitter.Width = 8;
            splitter.Margin = new Padding(0, 0, 0, 0);
            splitter.BackColor = Color.Transparent;

            // 字段/子字段内容
            this.textBox_content = new AutoHeightTextBox();
            textBox_content.Multiline = true;
            textBox_content.WordWrap = true;
            textBox_content.BorderStyle = BorderStyle.None;
            textBox_content.Dock = DockStyle.Fill;
            textBox_content.MinimumSize = new Size(20, 21); // 23
            textBox_content.Size = new Size(20, 21); // 23
            textBox_content.Margin = new Padding(8, 6, 0, 2);   // new Padding(8, 4, 0, 0)
            // textBox_content.BackColor = Color.Red;

            if (this.Container.BackColor != Color.Transparent)
                textBox_content.BackColor = this.Container.BackColor;
            else
                textBox_content.Tag = textBox_content.BackColor;

            textBox_content.ForeColor = this.Container.ForeColor;
        }

        // 从tablelayoutpanel中移除本Item涉及的控件
        // parameters:
        //      nRow    从0开始计数
        internal void RemoveFromTable(TableLayoutPanel table,
            int nRow)
        {
            this.Container.DisableUpdate();

            try
            {
                List<Control> hidden_controls = new List<Control>();

                // 移除本行相关的控件
                table.Controls.Remove(this.label_color);
                table.Controls.Remove(this.label_caption);
                if (this.splitter != null)
                    table.Controls.Remove(this.splitter);
                table.Controls.Remove(this.textBox_content);

                Debug.Assert(this.Container.Items.Count == table.RowCount - 2, "");

                // 然后压缩后方的
                int nEnd = Math.Min(table.RowCount - 1 - 1, this.Container.Items.Count - 1);
                for (int i = nRow; i < nEnd; i++)
                {
                    for (int j = 0; j < table.ColumnStyles.Count; j++)
                    {
                        Debug.Assert(i + EasyMarcControl.RESERVE_LINES + 1 < table.RowStyles.Count, "");

                        // Control control = table.GetControlFromPosition(j, i + EasyMarcControl.RESERVE_LINES + 1);
                        Control control = table.GetAnyControlAt(j, i + EasyMarcControl.RESERVE_LINES + 1);
                        if (control != null)
                        {
                            if (control.Visible == false)
                            {
                                control.Visible = true;
                                hidden_controls.Add(control);
                            }
                            table.Controls.Remove(control);
                            // Add 对于插入 Visible = false 的 Control 有问题。
                            // 为了避免问题，对这样的 Control 先显示，插入，最后再恢复为隐藏状态
                            table.Controls.Add(control, j, i + EasyMarcControl.RESERVE_LINES);
                        }
                        else
                        {
                            Debug.Assert(false, "");
                        }
                    }

                }

                table.RowCount--;
                table.RowStyles.RemoveAt(nRow);

                foreach (Control control in hidden_controls)
                {
                    control.Visible = false;
                }
            }
            finally
            {
                this.Container.EnableUpdate();
            }

        }

        // 插入本Line到某行。调用前，table.RowCount已经增量
        // parameters:
        //      nRow    从0开始计数
        internal void InsertToTable(TableLayoutPanel table,
            int nRow)
        {
            this.Container.DisableUpdate();

            try
            {
                Debug.Assert(table.RowCount == this.Container.Items.Count + 3, "");
                List<Control> hidden_controls = new List<Control>();

                // 先移动后方的
                int nEnd = Math.Min(table.RowCount - 1 - 1, this.Container.Items.Count - 1);
                for (int i = nEnd; i >= nRow; i--)
                {
                    // EasyLine line = this.Container.Items[i];

                    for (int j = 0; j < table.ColumnStyles.Count; j++)
                    {
                        Debug.Assert(i + EasyMarcControl.RESERVE_LINES + 1 < table.RowStyles.Count, "");

                        Control control = table.GetAnyControlAt(j, i + EasyMarcControl.RESERVE_LINES);
                        if (control != null)
                        {
                            if (control.Visible == false)
                            {
                                control.Visible = true;
                                hidden_controls.Add(control);
                            }
                            table.Controls.Remove(control);
                            // Add 对于插入 Visible = false 的 Control 有问题。
                            // 为了避免问题，对这样的 Control 先显示，插入，最后再恢复为隐藏状态
                            table.Controls.Add(control, j, i + EasyMarcControl.RESERVE_LINES + 1);
                        }
                        else
                        {
                            Debug.Assert(false, "");
                        }
                    }
                }

                table.Controls.Add(this.label_color, 0, nRow + EasyMarcControl.RESERVE_LINES);
                table.Controls.Add(this.label_caption, 1, nRow + EasyMarcControl.RESERVE_LINES);
                if (this.splitter != null)
                    table.Controls.Add(this.splitter, 2, nRow + EasyMarcControl.RESERVE_LINES);
                if (this.textBox_content.Visible == false)
                {
                    this.textBox_content.Visible = true;
                    hidden_controls.Add(this.textBox_content);
                }
                // 插入前，这里应该没有 Control
                {
                    // 这一句话必须有，不然会出现 BUG
                    Control temp = table.GetAnyControlAt(3, nRow + EasyMarcControl.RESERVE_LINES);
                    Debug.Assert(temp == null, "");
                }

                table.Controls.Add(this.textBox_content, 3, nRow + EasyMarcControl.RESERVE_LINES);

#if NO
#if DEBUG
                // 插入后，这里应该有 Control
                {
                    Control temp = table.GetAnyControlAt(3, nRow + EasyMarcControl.RESERVE_LINES);
                    Debug.Assert(temp != null, "");
                }
#endif
#endif

                foreach (Control control in hidden_controls)
                {
                    control.Visible = false;
                }

#if NO
#if DEBUG
                // 恢复隐藏后，这里应该有 Control
                {
                    Control temp = table.GetAnyControlAt(3, nRow + EasyMarcControl.RESERVE_LINES);
                    Debug.Assert(temp != null, "");
                }
#endif
#endif
            }
            finally
            {
                this.Container.EnableUpdate();
            }

            // events
            AddEvents(true);
        }

        void AddEvents(bool bAdd)
        {
            // 防止在控件成员变量为空的时候调用
            if (this.label_caption == null)
                return;

            if (bAdd)
            {
                this.label_caption.MouseUp += new MouseEventHandler(label_color_MouseUp);
                this.label_color.MouseUp += new MouseEventHandler(label_color_MouseUp);

                this.label_caption.MouseClick += new MouseEventHandler(label_caption_MouseClick);
                this.label_color.MouseClick += new MouseEventHandler(label_color_MouseClick);

                this.textBox_content.TextChanged += new EventHandler(textBox_content_TextChanged);
                this.textBox_content.Enter += new EventHandler(control_Enter);
                this.textBox_content.KeyDown += textBox_content_KeyDown;
                this.textBox_content.KeyPress += textBox_content_KeyPress;
                this.textBox_content.MouseWheel += textBox_content_MouseWheel;

                // this.splitter.Paint += new PaintEventHandler(splitter_Paint);

                this.splitter.MouseDown += new MouseEventHandler(splitter_MouseDown);
                this.splitter.MouseUp += new MouseEventHandler(splitter_MouseUp);

#if NO
            this.label_color.MouseWheel -= new MouseEventHandler(textBox_comment_MouseWheel);
            this.label_color.MouseWheel += new MouseEventHandler(textBox_comment_MouseWheel);

            this.label_caption.MouseWheel -= new MouseEventHandler(textBox_comment_MouseWheel);
            this.label_caption.MouseWheel += new MouseEventHandler(textBox_comment_MouseWheel);
#endif
            }
            else
            {
                this.label_caption.MouseUp -= new MouseEventHandler(label_color_MouseUp);
                this.label_color.MouseUp -= new MouseEventHandler(label_color_MouseUp);
                this.label_caption.MouseClick -= new MouseEventHandler(label_caption_MouseClick);
                this.label_color.MouseClick -= new MouseEventHandler(label_color_MouseClick);
                this.textBox_content.TextChanged -= new EventHandler(textBox_content_TextChanged);
                this.textBox_content.Enter -= new EventHandler(control_Enter);
                this.textBox_content.KeyDown -= textBox_content_KeyDown;
                this.textBox_content.KeyPress -= textBox_content_KeyPress;
                this.textBox_content.MouseWheel -= textBox_content_MouseWheel;
                this.splitter.MouseDown -= new MouseEventHandler(splitter_MouseDown);
                this.splitter.MouseUp -= new MouseEventHandler(splitter_MouseUp);
            }
        }

        void textBox_content_MouseWheel(object sender, MouseEventArgs e)
        {
            this.Container.DoMouseWheel(e);
        }

        void textBox_content_KeyPress(object sender, KeyPressEventArgs e)
        {
#if NO
            switch (e.KeyChar)
            {
                case 'M':
                case 'm':
                    {
                        if (Control.ModifierKeys == Keys.Control)
                        {
                            this.Container.GetValueFromTemplate(this);
                            e.Handled = true;
                        }
                    }
                    break;
            }
#endif
        }

        void textBox_content_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Down:
                case Keys.Enter:
                    if (this.Container.SelectNextControl((sender as Control), true, true, true, false) == false)
                    {
                        // SendKeys.Send("{TAB}");
                    }
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;
                case Keys.Up:
                    //
                    // 摘要: 
                    //     激活下一个控件。
                    //
                    // 参数: 
                    //   ctl:
                    //     从其上开始搜索的 System.Windows.Forms.Control。
                    //
                    //   forward:
                    //     如果为 true 则在 Tab 键顺序中前移；如果为 false 则在 Tab 键顺序中后移。
                    //
                    //   tabStopOnly:
                    //     true 表示忽略 System.Windows.Forms.Control.TabStop 属性设置为 false 的控件；false 表示不忽略。
                    //
                    //   nested:
                    //     true 表示包括嵌套子控件（子控件的子级）；false 表示不包括。
                    //
                    //   wrap:
                    //     true 表示在到达最后一个控件之后从 Tab 键顺序中第一个控件开始继续搜索；false 表示不继续搜索。
                    //
                    // 返回结果: 
                    //     如果控件已激活，则为 true；否则为 false。
                    if (this.Container.SelectNextControl((sender as Control), false, true, true, false) == false)
                    {
                        // SendKeys.Send("+{TAB}");
                    }
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;
            }
        }

        void splitter_MouseUp(object sender, MouseEventArgs e)
        {
            int nDelta = e.X - _nSplitterStart;
            this.Container.ChangeCaptionWidth(nDelta);
        }

        int _nSplitterStart = 0;
        void splitter_MouseDown(object sender, MouseEventArgs e)
        {
            _nSplitterStart = e.X;
        }

#if NO
        void textBox_comment_MouseWheel(object sender, MouseEventArgs e)
        {
            TableLayoutPanel table = this.Container.TableLayoutPanel;

            int nValue = table.VerticalScroll.Value;
            nValue -= e.Delta;
            if (nValue > table.VerticalScroll.Maximum)
                nValue = table.VerticalScroll.Maximum;
            if (nValue < table.VerticalScroll.Minimum)
                nValue = table.VerticalScroll.Minimum;

            if (table.VerticalScroll.Value != nValue)
            {
                table.VerticalScroll.Value = nValue;
                table.PerformLayout();
            }
        }
#endif

        void control_Enter(object sender, EventArgs e)
        {
            this.Container.SelectItem(this, true);
        }

        void textBox_content_TextChanged(object sender, EventArgs e)
        {
            if ((this.State & ItemState.New) == 0)
                this.State |= ItemState.Changed;

            // this.Container.Changed = true;
            this.Container.FireTextChanged();
        }

        void label_color_MouseClick(object sender, MouseEventArgs e)
        {
            this.Container.m_bFocused = true;
            if (this.Container.TableLayoutPanel.ContainsFocus == false)
                this.Container.TableLayoutPanel.Focus();

            if (e.Button == MouseButtons.Left)
            {
                if (Control.ModifierKeys == Keys.Control)
                {
                    this.Container.ToggleSelectItem(this);
                }
                else if (Control.ModifierKeys == Keys.Shift)
                    this.Container.RangeSelectItem(this);
                else
                {
                    this.Container.SelectItem(this, true);
                }

                if (this.ExpandState != EasyMarc.ExpandState.None)
                {
                    this.Container.ToggleExpand(this);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                // 如果当前有多重选择，则不必作什么l
                // 如果当前为单独一个选择或者0个选择，则选择当前对象
                // 这样做的目的是方便操作
                if (this.Container.SelectedIndices.Count < 2)
                {
                    this.Container.SelectItem(this, true);
                }
            }

            // 2014/9/30
            // this.EnsureVisible();
        }

        void label_caption_MouseClick(object sender, MouseEventArgs e)
        {
            this.Container.m_bFocused = true;
            if (this.Container.TableLayoutPanel.ContainsFocus == false)
                this.Container.TableLayoutPanel.Focus();

            if (e.Button == MouseButtons.Left)
            {
                if (Control.ModifierKeys == Keys.Control)
                {
                    this.Container.ToggleSelectItem(this);
                }
                else if (Control.ModifierKeys == Keys.Shift)
                    this.Container.RangeSelectItem(this);
                else
                {
                    this.Container.SelectItem(this, true);
                }
            }
            else if (e.Button == MouseButtons.Right)
            {
                // 如果当前有多重选择，则不必作什么l
                // 如果当前为单独一个选择或者0个选择，则选择当前对象
                // 这样做的目的是方便操作
                if (this.Container.SelectedIndices.Count < 2)
                {
                    this.Container.SelectItem(this, true);
                }
            }

            // 2014/9/30
            this.EnsureVisible();
        }

        void label_color_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            int nSelectedCount = this.Container.SelectedIndices.Count;

            EasyLine first_line = null;
            if (nSelectedCount > 0)
                first_line = this.Container.SelectedItems[0];

            //
            menuItem = new MenuItem("新增字段(&F)");
            menuItem.Click += new System.EventHandler(this.menu_newField_Click);
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("新增子字段(&S)");
            menuItem.Click += new System.EventHandler(this.menu_newSubfield_Click);
            if (first_line != null
                && first_line is FieldLine
                && (first_line as FieldLine).IsControlField == true)    // 控制字段下，无法插入子字段
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            // 定长模板
            string strCurName = "";
            bool bEnable = this.Container.HasTemplateOrValueListDef(
                this,
                "template",
                out strCurName);

            menuItem = new MenuItem("模板编辑 [" + strCurName + "] (&T)");
            menuItem.Click += new System.EventHandler(this.menu_templateDialog_Click);
            menuItem.Enabled = bEnable;
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("模板编辑 [头标区] (&H)");
            menuItem.Click += new System.EventHandler(this.menu_headerTemplateDialog_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("删除(&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteElements_Click);
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("删除全部空事项(&B)");
            menuItem.Click += new System.EventHandler(this.menu_deleteAllBlankElements_Click);
            contextMenu.MenuItems.Add(menuItem);


            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("全部收缩(&C)");
            menuItem.Click += new System.EventHandler(this.menu_collapseAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("全部展开(&E)");
            menuItem.Click += new System.EventHandler(this.menu_expandAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("隐藏字段(&H)");
            menuItem.Click += new System.EventHandler(this.menu_hideField_Click);
            if (!(this is FieldLine))
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            int nHideCount = this.Container.GetHideFieldCount();
            //
            menuItem = new MenuItem("恢复显示隐藏的字段 [" + nHideCount.ToString() + "] (&E)");
            menuItem.Click += new System.EventHandler(this.menu_unHideAllFields_Click);
            if (nHideCount == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);
            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("显示字段指示符(&I)");
            menuItem.Click += new System.EventHandler(this.menu_toggleDisplayIndicator_Click);
            menuItem.Checked = !this.Container.HideIndicator;
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("显示原始字段名、子字段名(&N)");
            menuItem.Click += new System.EventHandler(this.menu_toggleDisplayNumberName_Click);
            menuItem.Checked = this.Container.IncludeNumber;
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("Verify");
            menuItem.Click += new System.EventHandler(this.menu_verify_Click);
            contextMenu.MenuItems.Add(menuItem);


            /*
            menuItem = new MenuItem("test");
            menuItem.Click += new System.EventHandler(this.menu_test_Click);
            contextMenu.MenuItems.Add(menuItem);
             * */
            contextMenu.Show(this.label_color, new Point(e.X, e.Y));
        }

        void menu_verify_Click(object sender, EventArgs e)
        {
            this.Container.Verify();
        }

        // 打开定长模板对话框
        void menu_templateDialog_Click(object sender, EventArgs e)
        {
            this.Container.GetValueFromTemplate(this);
        }

        void menu_headerTemplateDialog_Click(object sender, EventArgs e)
        {
            this.Container.HeaderGetValueFromTemplate();
        }

        void menu_hideField_Click(object sender, EventArgs e)
        {
            this.Container.ToggleHide(this as FieldLine);
        }

        void menu_unHideAllFields_Click(object sender, EventArgs e)
        {
            this.Container.HideFields(null, false);
        }

        void menu_collapseAll_Click(object sender, EventArgs e)
        {
            this.Container.ExpandAll(false);
        }

        void menu_expandAll_Click(object sender, EventArgs e)
        {
            this.Container.ExpandAll(true);
        }

        // 显示字段指示符
        void menu_toggleDisplayIndicator_Click(object sender, EventArgs e)
        {
            this.Container.HideIndicator = !this.Container.HideIndicator;
        }

        // 显示原始字段名、子字段名
        void menu_toggleDisplayNumberName_Click(object sender, EventArgs e)
        {
            this.Container.IncludeNumber = !this.Container.IncludeNumber;
        }

        void menu_newField_Click(object sender, EventArgs e)
        {
            int nPos = this.Container.Items.IndexOf(this);

            if (nPos == -1)
                throw new Exception("not found myself");

            FieldLine field = this.Container.NewField(nPos);
            if (field != null)
            {
                // 置于可见范围
                field.EnsureVisible();
            }
        }

        void menu_newSubfield_Click(object sender, EventArgs e)
        {
            int nPos = this.Container.Items.IndexOf(this);
            if (nPos == -1)
            {
                throw new Exception("not found myself");
            }
            SubfieldLine subfield = this.Container.NewSubfield(nPos);
            if (subfield != null)
            {
                // 置于可见范围
                subfield.EnsureVisible();
            }
        }

        // 删除全部空元素
        void menu_deleteAllBlankElements_Click(object sender, EventArgs e)
        {
            Point save = this.Container.AutoScrollPosition;

            this.Container.DeleteBlankElements("visible");

            this.Container.Update();
            this.Container.SetAutoScrollPosition(new Point(-save.X, -save.Y));
        }

        // 删除当前元素
        void menu_deleteElements_Click(object sender, EventArgs e)
        {
            List<EasyLine> selected_lines = this.Container.SelectedItems;

            if (selected_lines.Count == 0)
            {
                MessageBox.Show(this.Container, "尚未选定要删除的事项");
                return;
            }
            string strText = "";

            if (selected_lines.Count == 1)
                strText = "确实要删除事项 '" + selected_lines[0].Caption + "'? ";
            else
                strText = "确实要删除所选定的 " + selected_lines.Count.ToString() + " 个事项?";

            DialogResult result = MessageBox.Show(this.Container,
                strText,
                "EasyMarcControl",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

#if NO
            int nNotDeleteCount = 0;
            this.Container.DisableUpdate();
            try
            {
                for (int i = 0; i < selected_lines.Count; i++)
                {
                    EasyLine item = selected_lines[i];
                    if ((item.State & ItemState.ReadOnly) != 0)
                    {
                        nNotDeleteCount++;
                        continue;
                    }
                    this.Container.RemoveItem(item);
                }
            }
            finally
            {
                this.Container.EnableUpdate();
            }

            if (nNotDeleteCount > 0)
                MessageBox.Show(this.Container, "有 " + nNotDeleteCount.ToString() + " 项已订购状态的事项未能删除");
#endif
            Point save = this.Container.AutoScrollPosition;

#if NO
            this.Container.DisableUpdate();
            try
            {
#endif
            this.Container.DeleteElements(selected_lines);
#if NO
            }
            finally
            {
                this.Container.EnableUpdate();
            }
#endif

            // TODO: 在删除前要找一个参考对象，删除完成后，要 EnsureVisible 这个参考对象
            // 也可以简单在删除前保存 y offset, 删除完成后恢复
            this.Container.Update();
            this.Container.SetAutoScrollPosition(new Point(-save.X, -save.Y));
        }


        // 事项状态
        public ItemState State
        {
            get
            {
                return this.m_state;
            }
            set
            {
                if (this.m_state != value)
                {
                    this.m_state = value;

                    SetLineColor();

                    bool bOldReadOnly = this.ReadOnly;
                    if ((this.m_state & ItemState.ReadOnly) != 0)
                    {
                        this.ReadOnly = true;
                    }
                    else
                    {
                        this.ReadOnly = false;
                    }
                }
            }
        }

        /*
发生未捕获的界面线程异常: 
Type: System.NullReferenceException
Message: 未将对象引用设置到对象的实例。
Stack:
在 DigitalPlatform.EasyMarc.EasyLine.SetLineColor(Boolean bSetAll)
在 DigitalPlatform.EasyMarc.SubfieldLine.SetLineColor(Boolean bSetAll)
在 DigitalPlatform.EasyMarc.EasyLine.set_State(ItemState value)
在 DigitalPlatform.EasyMarc.EasyMarcControl.SelectItem(EasyLine element, Boolean bClearOld, Boolean bSetFocus)
在 DigitalPlatform.EasyMarc.EasyLine.control_Enter(Object sender, EventArgs e)
在 System.Windows.Forms.ContainerControl.UpdateFocusedControl()


         * */
        // 设置事项左端label的颜色
        // parameters:
        //      bSetAll 是否要重设全部颜色？= false 表示仅重设和焦点变化有关的颜色; = true 表示要重设全部颜色，包括和焦点变化无关的那些颜色
        public virtual void SetLineColor(bool bSetAll = false)
        {
            if (this.Container == null
                || this.textBox_content == null
                || this.label_color == null)
                return;

            if (bSetAll == true)
            {
                if (this.Container.BackColor != Color.Transparent)
                    textBox_content.BackColor = this.Container.BackColor;

                textBox_content.ForeColor = this.Container.ForeColor;
            }

            if ((this.m_state & ItemState.Selected) != 0)
            {
                // 没有焦点，又需要隐藏selection情形
                if (this.Container.HideSelection == true
                    && this.Container.m_bFocused == false)
                {
                    // 继续向后走，显示其他颜色
                }
                else
                {
                    if (this.Container.m_bFocused == false)
                    {
                        this.label_color.BackColor = ControlPaint.Dark(SystemColors.Highlight);
                        this.textBox_content.BackColor = ControlPaint.Light(this.Container.FocusedEditBackColor);
                        return;
                    }
                    else
                    {
                        this.label_color.BackColor = SystemColors.Highlight;
                        this.textBox_content.BackColor = this.Container.FocusedEditBackColor;
                        return;
                    }
                }
            }

            if (this.textBox_content.Tag != null)
                this.textBox_content.BackColor = (Color)this.textBox_content.Tag;
            else
                this.textBox_content.BackColor = this.Container.GetRealBackColor(Color.Black);

            if ((this.m_state & ItemState.New) != 0)
            {
                this.label_color.BackColor = Color.Yellow;
                return;
            }
            if ((this.m_state & ItemState.Changed) != 0)
            {
                this.label_color.BackColor = Color.LightGreen;
                return;
            }
            if ((this.m_state & ItemState.ReadOnly) != 0)
            {
                this.label_color.BackColor = Color.LightGray;
                return;
            }

            this.label_color.BackColor = this.Container.LeftBackColor;  //  SystemColors.Window;
        }

        bool m_bReadOnly = false;

        public bool ReadOnly
        {
            get
            {
                return this.m_bReadOnly;
            }
            set
            {
                bool bOldValue = this.m_bReadOnly;
                if (bOldValue != value)
                {
                    this.m_bReadOnly = value;

                    // 
                    this.textBox_content.ReadOnly = value;
                }
            }
        }

        public string Content
        {
            get
            {
                return this.textBox_content.Text;
            }
            set
            {
                this.textBox_content.Text = value;
            }
        }

        public string Caption
        {
            get
            {
                return this.label_caption.Text;
            }
            set
            {
                this.label_caption.Text = value;
            }
        }

        public virtual bool Visible
        {
            get
            {
                return this.label_color.Visible;
            }
            set
            {
                this.label_color.Visible = value;
                this.label_caption.Visible = value;
                if (this.splitter != null)
                    this.splitter.Visible = value;
                this.textBox_content.Visible = value;
            }
        }

        public void EnsureVisible()
        {
            this.Container.EnsureVisible(this);
        }
    }

    // 支持透明背景色的 Splitter
    public class TransparentSplitter : Splitter
    {
        public TransparentSplitter()
            : base()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor, true);
        }
    }

}
