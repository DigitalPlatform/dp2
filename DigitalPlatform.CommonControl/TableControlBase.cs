using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace DigitalPlatform.CommonControl
{
    /// <summary>
    /// 表格控件基础类
    /// </summary>
    /// <typeparam name="T">表格行的类型</typeparam>
    public partial class TableControlBase : UserControl
    {
        public int RESERVE_LINES = 0;

        // 原始的行数组
        public List<TableItemBase> Items = new List<TableItemBase>();

        internal TableLayoutPanel tableLayoutPanel_content = null;

        public TableLayoutPanel TableLayoutPanel
        {
            get
            {
                return this.tableLayoutPanel_content;
            }
            set
            {
                this.tableLayoutPanel_content = value;
            }
        }

        public TableControlBase()
        {
            InitializeComponent();
        }

        void ClearItems()
        {
            if (this.Items != null)
            {
                foreach (TableItemBase item in this.Items)
                {
                    if (item != null)
                        item.Dispose();
                }
                this.Items.Clear();
            }
        }

        public void SetTitleLine(List<string> titles)
        {
            int nColumnIndex = 0;
            foreach (string title in titles)
            {
                Label label = new Label();
                label.Dock = DockStyle.Fill;
                label.Size = new Size(6, 23);
                label.AutoSize = true;
                label.Margin = new Padding(0, 0, 0, 0);
                label.Text = title;
                this.tableLayoutPanel_content.Controls.Add(label, nColumnIndex++, 0);
            }
        }

        bool m_bChanged = false;

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        [Category("Content")]
        [DescriptionAttribute("Changed")]
        [DefaultValue(false)]
        public virtual bool Changed
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

        void RefreshLineColor()
        {
            for (int i = 0; i < this.Items.Count; i++)
            {
                TableItemBase item = this.Items[i];
                item.SetLineColor();
            }
        }

        // 将全部行的状态恢复为普通状态
        void ResetLineState()
        {
            for (int i = 0; i < this.Items.Count; i++)
            {
                TableItemBase item = this.Items[i];

                if ((item.State & ItemState.ReadOnly) != 0)
                    item.State = ItemState.Normal | ItemState.ReadOnly;
                else
                    item.State = ItemState.Normal;
            }

            this.Invalidate();
        }

        public List<TableItemBase> SelectedItems
        {
            get
            {
                List<TableItemBase> results = new List<TableItemBase>();

                for (int i = 0; i < this.Items.Count; i++)
                {
                    TableItemBase cur_element = this.Items[i];
                    if ((cur_element.State & ItemState.Selected) != 0)
                        results.Add(cur_element);
                }

                return results;
            }
        }

        public List<int> SelectedIndices
        {
            get
            {
                List<int> results = new List<int>();

                for (int i = 0; i < this.Items.Count; i++)
                {
                    TableItemBase cur_element = this.Items[i];
                    if ((cur_element.State & ItemState.Selected) != 0)
                        results.Add(i);
                }

                return results;
            }
        }


        public void SelectAll()
        {
            for (int i = 0; i < this.Items.Count; i++)
            {
                TableItemBase cur_element = this.Items[i];
                if ((cur_element.State & ItemState.Selected) == 0)
                    cur_element.State |= ItemState.Selected;
            }

            this.Invalidate();
        }

        public TableItemBase LastClickItem = null;   // 最近一次click选择过的Item对象

        public void SelectItem(TableItemBase element,
            bool bClearOld)
        {

            if (bClearOld == true)
            {
                for (int i = 0; i < this.Items.Count; i++)
                {
                    TableItemBase cur_element = this.Items[i];

                    if (cur_element == element)
                        continue;   // 暂时不处理当前行

                    if ((cur_element.State & ItemState.Selected) != 0)
                    {
                        cur_element.State -= ItemState.Selected;

                        this.InvalidateLine(cur_element);
                    }
                }
            }

            // 选中当前行
            if ((element.State & ItemState.Selected) == 0)
            {
                element.State |= ItemState.Selected;

                this.InvalidateLine(element);
            }

            this.LastClickItem = element;
        }

        public void ToggleSelectItem(TableItemBase element)
        {
            // 选中当前行
            if ((element.State & ItemState.Selected) == 0)
                element.State |= ItemState.Selected;
            else
                element.State -= ItemState.Selected;

            this.InvalidateLine(element);

            this.LastClickItem = element;
        }

        public void RangeSelectItem(TableItemBase element)
        {
            TableItemBase start = this.LastClickItem;

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

            for (int i = nStart; i <= nEnd; i++)
            {
                TableItemBase cur_element = this.Items[i];

                if ((cur_element.State & ItemState.Selected) == 0)
                {
                    cur_element.State |= ItemState.Selected;

                    this.InvalidateLine(cur_element);
                }
            }

            // 清除其余位置
            for (int i = 0; i < nStart; i++)
            {
                TableItemBase cur_element = this.Items[i];

                if ((cur_element.State & ItemState.Selected) != 0)
                {
                    cur_element.State -= ItemState.Selected;

                    this.InvalidateLine(cur_element);
                }
            }

            for (int i = nEnd + 1; i < this.Items.Count; i++)
            {
                TableItemBase cur_element = this.Items[i];

                if ((cur_element.State & ItemState.Selected) != 0)
                {
                    cur_element.State -= ItemState.Selected;

                    this.InvalidateLine(cur_element);
                }
            }
        }

        internal void InvalidateLine(TableItemBase item)
        {

            Point p = this.tableLayoutPanel_content.PointToScreen(new Point(0, 0));

            Rectangle rect = item.label_color.RectangleToScreen(item.label_color.ClientRectangle);
            rect.Width = this.tableLayoutPanel_content.DisplayRectangle.Width;
            rect.Offset(-p.X, -p.Y);
            rect.Height = (int)this.Font.GetHeight() + 8;   // 缩小刷新高度

            this.tableLayoutPanel_content.Invalidate(rect, false);

            // this.tableLayoutPanel_content.Invalidate();
        }


        int m_nInSuspend = 0;
        // List<Point> _points = new List<Point>();

        public void DisableUpdate()
        {
#if NO
            Point point = tableLayoutPanel_content.AutoScrollPosition;
            this._points.Add(point);
#endif

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

#if NO
            Point point = this._points[this._points.Count - 1];
            this._points.RemoveAt(this._points.Count - 1);
            this.tableLayoutPanel_content.AutoScrollPosition = new Point(-point.X, -point.Y);
#endif

        }

        public void InsertNewLine(int index,
    TableItemBase line,
    bool bFireEnvent)
        {
            this.DisableUpdate();   // 防止闪动

            try
            {
#if NO
                RowStyle style = new RowStyle();
                //style.Height = 26;
                //style.SizeType = SizeType.Absolute;

                this.tableLayoutPanel_content.RowStyles.Insert(index + RESERVE_LINES, style);
#endif
                Debug.Assert(this.tableLayoutPanel_content.RowStyles.Count == 1, "");
                this.tableLayoutPanel_content.RowCount += 1;
                Debug.Assert(this.tableLayoutPanel_content.RowStyles.Count == 1, "");

                line.InsertToTable(this.tableLayoutPanel_content, index);

                this.Items.Insert(index, line);

                line.State = ItemState.New;

                if (bFireEnvent == true)
                    this.FireTextChanged();

                Debug.Assert(this.tableLayoutPanel_content.RowStyles.Count == 1, "");
            }
            finally
            {
                this.EnableUpdate();
            }
        }

        // 文档发生改变
        internal void FireTextChanged()
        {
            this.Changed = true;

            EventArgs e = new EventArgs();
            this.OnTextChanged(e);
        }

        internal bool m_bFocused = false;

        public bool IsFocued
        {
            get
            {
                return this.m_bFocused;
            }
            set
            {
                this.m_bFocused = value;
            }
        }

        private void TableControlBase_Enter(object sender, EventArgs e)
        {
            this.tableLayoutPanel_content.Focus();
            this.m_bFocused = true;
            this.RefreshLineColor();
        }

        private void TableControlBase_Leave(object sender, EventArgs e)
        {
            this.m_bFocused = false;
            this.RefreshLineColor();
        }

    }

    /// <summary>
    /// 行基础类型
    /// </summary>
    public class TableItemBase : IDisposable
    {
        public TableControlBase Container = null;

        public object Tag = null;   // 用于存放需要连接的任意类型对象
        ItemState m_state = ItemState.Normal;

        // 颜色、popupmenu
        public Label label_color = null;

        public virtual void DisposeChildControls()
        {
            label_color.Dispose();
            Container = null;
        }

        #region 释放资源

        ~TableItemBase()
        {
            Dispose(false);
        }

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
                    AddEvents(false);
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


        // 从tablelayoutpanel中移除本Item涉及的控件
        // parameters:
        //      nRow    从0开始计数
        internal void RemoveFromTable(TableLayoutPanel table,
            int nRow)
        {
            this.Container.DisableUpdate();

            try
            {
                // 移除本行相关的控件
                RemoveControls(table);

                Debug.Assert(this.Container.Items.Count == table.RowCount - 2, "");

                // 然后压缩后方的
                int nEnd = Math.Min(table.RowCount - 1 - 1, this.Container.Items.Count - 1);
                for (int i = nRow; i < nEnd; i++)
                {
                    for (int j = 0; j < table.ColumnStyles.Count; j++)
                    {
                        // Debug.Assert(i + this.Container.RESERVE_LINES + 1 < table.RowStyles.Count, "");

                        Control control = table.GetControlFromPosition(j, i + this.Container.RESERVE_LINES + 1);
                        if (control != null)
                        {
                            table.Controls.Remove(control);
                            table.Controls.Add(control, j, i + this.Container.RESERVE_LINES);
                        }
                    }

                }

                table.RowCount--;
                // table.RowStyles.RemoveAt(nRow);

                this.AddEvents(false);  // 2015/7/21
            }
            finally
            {
                this.Container.EnableUpdate();
            }
        }

        public virtual void RemoveControls(TableLayoutPanel table)
        {

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

                Debug.Assert(table.RowCount == this.Container.Items.Count + 2, "");

                // 先移动后方的
                int nEnd = Math.Min(table.RowCount - 1, this.Container.Items.Count - 1);
                for (int i = nEnd; i >= nRow; i--)
                {
                    // EasyLine line = this.Container.Items[i];

                    for (int j = 0; j < table.ColumnStyles.Count; j++)
                    {
                        // Debug.Assert(i + this.Container.RESERVE_LINES + 1 < table.RowStyles.Count, "");

                        Control control = table.GetControlFromPosition(j, i + this.Container.RESERVE_LINES);
                        if (control != null)
                        {
                            table.Controls.Remove(control);
                            table.Controls.Add(control, j, i + this.Container.RESERVE_LINES + 1);
                        }
                    }
                }

                AddControls(table, nRow);
            }
            finally
            {
                this.Container.EnableUpdate();
            }

            // events
            AddEvents(true);
        }

        public virtual void AddControls(TableLayoutPanel table, int nRow)
        {

        }


        public virtual void AddEvents(bool bAdd)
        {

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

        // 设置事项左端label的颜色
        internal void SetLineColor()
        {
            if ((this.m_state & ItemState.Selected) != 0)
            {
                // 没有焦点，又需要隐藏selection情形
                if (this.Container.HideSelection == true
                    && this.Container.IsFocued == false)
                {
                    // 继续向后走，显示其他颜色
                }
                else
                {
                    this.label_color.BackColor = SystemColors.Highlight;
                    return;
                }
            }
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

            this.label_color.BackColor = SystemColors.Window;
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

                    SetReaderOnly(value);
                }
            }
        }

        public virtual void SetReaderOnly(bool bReadOnly)
        {

        }

    }
}
