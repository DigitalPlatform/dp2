using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Drawing;
using System.Diagnostics;
using System.ComponentModel;

using DigitalPlatform;
using DigitalPlatform.CommonControl;
using DigitalPlatform.Xml;
using DigitalPlatform.Core;
using Accord.IO;

namespace dp2Circulation
{
    /// <summary>
    /// 册/订购/期/评注/读者 编辑控件的基础类
    /// </summary>
    public class ItemEditControlBase : UserControl
    {
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        public event EventHandler SelectionChanged = null;

        internal TableLayoutPanel _tableLayoutPanel_main = null;

        ItemDisplayState _createState = ItemDisplayState.Normal;
        // 创建状态
        public virtual ItemDisplayState CreateState
        {
            get
            {
                return this._createState;
            }
            set
            {
                this._createState = value;
            }
        }

        /// <summary>
        /// 刷新 Layout 事件
        /// </summary>
        public event PaintEventHandler PaintContent = null;

        // 获取值列表时作为线索的数据库名
        /// <summary>
        /// 书目库名。获取值列表时作为线索的数据库名
        /// </summary>
        public string BiblioDbName = "";

        internal XmlDocument _dataDom = null;

        internal bool m_bChanged = false;

        internal bool m_bInInitial = true;   // 是否正在初始化过程之中

        internal Color ColorChanged = Color.Yellow; // 表示内容改变过的颜色
        internal Color ColorDifference = Color.Blue; // 表示内容有差异的颜色
        private ToolTip toolTip1;
        private IContainer components;
        internal string m_strParentId = "";

        // 2022/10/28
        public event VerifyEditEventHandler VerifyContent = null;

        /// <summary>
        /// 获得值列表
        /// </summary>
        public event GetValueTableEventHandler GetValueTable = null;

        /// <summary>
        /// 内容发生改变
        /// </summary>
        public event ContentChangedEventHandler ContentChanged = null;

        /// <summary>
        /// ControlKeyPress
        /// </summary>
        public event ControlKeyPressEventHandler ControlKeyPress = null;

        /// <summary>
        /// ControlKeyDown
        /// </summary>
        public event ControlKeyEventHandler ControlKeyDown = null;

        public ItemEditControlBase()
        {
#if NO
            this.DoubleBuffered = true;

            this.Enter += new System.EventHandler(this.ItemEditControlBase_Enter);
            this.Leave += new System.EventHandler(this.ItemEditControlBase_Leave);
#endif
        }

        // 2015/7/21
        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // release managed resources if any
                AddEvents(false);
            }

            base.Dispose(disposing);
        }

        /// <summary>
        /// 是否正在执行初始化
        /// </summary>
        public bool Initializing
        {
            get
            {
                return this.m_bInInitial;
            }
            set
            {
                this.m_bInInitial = value;
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

        #region 数据成员

        /// <summary>
        /// 旧记录
        /// </summary>
        public string OldRecord = "";

        /// <summary>
        /// 时间戳
        /// </summary>
        public byte[] Timestamp = null;

        /// <summary>
        /// 父记录 ID
        /// </summary>
        public string ParentId
        {
            get
            {
                return this.m_strParentId;
            }
            set
            {
                this.m_strParentId = value;
            }
        }

        #endregion

        // 获得行的宽度。不包括最后按钮一栏
        internal int GetLineContentPixelWidth()
        {
            for (int i = 0; i < this._tableLayoutPanel_main.RowStyles.Count; i++)
            {
                Control control = this._tableLayoutPanel_main.GetControlFromPosition(2, i);
                if (control == null)
                    continue;
                Label label = this._tableLayoutPanel_main.GetControlFromPosition(0, i) as Label;
                if (label == null)
                    continue;
                return control.Location.X + control.Width - label.Location.X;
            }

            return 0;
        }

        // 将所有 changed color 清除
        internal void ResetColor()
        {
            // throw new Exception("尚未实现 ResetColor()");

            for (int i = 0; i < this._tableLayoutPanel_main.RowStyles.Count; i++)
            {
                Label color = this._tableLayoutPanel_main.GetControlFromPosition(1, i) as Label;
                if (color == null)
                    continue;
                EditLineState state = color.Tag as EditLineState;
                if (state != null)
                {
                    if (state.Changed == true)
                        state.Changed = false;
                    SetLineState(color, state);
                }
                else
                    color.BackColor = this._tableLayoutPanel_main.BackColor;
            }
        }

#if NO
        internal void OnContentChanged(bool bOldValue, bool value)
        {
            // 触发事件
            if (bOldValue != value && this.ContentChanged != null)
            {
                ContentChangedEventArgs e = new ContentChangedEventArgs();
                e.OldChanged = bOldValue;
                e.CurrentChanged = value;
                ContentChanged(this, e);
            }
        }
#endif
        // 不优化触发 ContentChanged 的过程
        public bool OptimizeTriggerContentChanged = false;

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed
        {
            get
            {
                return this.m_bChanged;
            }
            set
            {
                bool bOldValue = this.m_bChanged;

                this.m_bChanged = value;
                if (this.m_bChanged == false)
                {
                    this.TryInvoke(() =>
                    {
                        this.ResetColor();
                    });
                }

                // 触发事件
                if ((bOldValue != value || OptimizeTriggerContentChanged == false)
                    && this.ContentChanged != null)
                {
                    this.TryInvoke(() =>
                    {
                        ContentChangedEventArgs e = new ContentChangedEventArgs();
                        e.OldChanged = bOldValue;
                        e.CurrentChanged = value;
                        ContentChanged(this, e);
                    });
                }
            }
        }

        /// <summary>
        /// 设置数据
        /// </summary>
        /// <param name="strXml">实体记录 XML</param>
        /// <param name="strRecPath">实体记录路径</param>
        /// <param name="timestamp">时间戳</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int SetData(string strXml,
            string strRecPath,
            byte[] timestamp,
            out string strError)
        {
            strError = "";

            this.OldRecord = strXml;
            this.Timestamp = timestamp;

            this._dataDom = new XmlDocument();

            try
            {
                if (String.IsNullOrEmpty(strXml) == true)
                    this._dataDom.LoadXml("<root />");
                else
                    this._dataDom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML数据装载到DOM时出错" + ex.Message;
                return -1;
            }

            this.TryInvoke(() =>
            {
                this.Initializing = true;
                try
                {
                    this.DomToMember(strRecPath);
                }
                finally
                {
                    this.Initializing = false;
                }
            });

            this.Changed = false;
            return 0;
        }

        internal virtual void DomToMember(string strRecPath)
        {
            // throw new Exception("尚未实现 DomToMember()");

            for (int i = this._tableLayoutPanel_main.RowStyles.Count - 1; i >= 0; i--)
            {
                Control caption = this._tableLayoutPanel_main.GetControlFromPosition(0, i);
                if (caption == null || caption.Tag == null)
                    continue;
                FieldDef def = caption.Tag as FieldDef;

                Control edit = this._tableLayoutPanel_main.GetControlFromPosition(2, i);
                if (edit == null)
                    continue;

                edit.Text = DomUtil.GetElementText(this.DataDom.DocumentElement, def.Element);
            }
        }

        /// <summary>
        /// 清除全部内容
        /// </summary>
        public virtual void Clear()
        {
            throw new Exception("尚未实现 Clear()");
        }

        // 可能会抛出异常
        /// <summary>
        /// 数据 XmlDocument 对象
        /// </summary>
        public XmlDocument DataDom
        {
            get
            {
                // 2012/12/28
                if (this._dataDom == null)
                {
                    this._dataDom = new XmlDocument();
                    this._dataDom.LoadXml("<root />");
                }
                if (this.m_bInInitial == false) // 2015/10/11
                    this.RefreshDom();
                return this._dataDom;
            }
        }

        int _inRefreshDom = 0;
        // member --> dom
        internal virtual void RefreshDom()
        {
            // throw new Exception("尚未实现 RefreshDom()");
            // 防止递归
            if (this._inRefreshDom > 0)
                return;
            this._inRefreshDom++;
            try
            {
                for (int i = this._tableLayoutPanel_main.RowStyles.Count - 1; i >= 0; i--)
                {
                    Control caption = this._tableLayoutPanel_main.GetControlFromPosition(0, i);
                    if (caption == null || caption.Tag == null)
                        continue;
                    FieldDef def = caption.Tag as FieldDef;

                    Control edit = this._tableLayoutPanel_main.GetControlFromPosition(2, i);
                    if (edit == null)
                        continue;

                    DomUtil.SetElementText(this.DataDom.DocumentElement, def.Element, edit.Text);
                }
            }
            finally
            {
                this._inRefreshDom--;
            }
        }

        /// <summary>
        /// 创建好适合于保存的记录信息
        /// </summary>
        /// <param name="bWarningParent">是否要警告this.Parent为空情况?</param>
        /// <param name="strXml">返回构造好的实体记录 XML</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>-1: 出错; 0: 成功</returns>
        public int GetData(
            bool bWarningParent,
            out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            if (this._dataDom == null)
            {
                this._dataDom = new XmlDocument();
                this._dataDom.LoadXml("<root />");
            }

            if (this.ParentId == ""
                && bWarningParent == true)
            {
                strError = "GetData()错误：Parent成员尚未定义。";
                return -1;
            }

            /*
            if (this.Barcode == "")
            {
                strError = "Barcode成员尚未定义";
                return -1;
            }*/

            try
            {
                this.RefreshDom();
            }
            catch (Exception ex)
            {
                strError = "RefreshDom() exception: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            // 2018/6/5
            if (this._dataDom.DocumentElement != null)
                DomUtil.RemoveEmptyElements(this._dataDom.DocumentElement);

            strXml = this._dataDom.OuterXml;
            return 0;
        }

        // 添加、删除各种事件
        internal void AddEvents(bool bAdd)
        {
            if (this._tableLayoutPanel_main == null)
                return;

            Debug.Assert(this._tableLayoutPanel_main != null, "");

            if (bAdd)
            {
                this._tableLayoutPanel_main.MouseClick += tableLayoutPanel_main_MouseClick;
                this._tableLayoutPanel_main.MouseDown += tableLayoutPanel_main_MouseDown;
            }
            else
            {
                this._tableLayoutPanel_main.MouseClick -= tableLayoutPanel_main_MouseClick;
                this._tableLayoutPanel_main.MouseDown -= tableLayoutPanel_main_MouseDown;
            }

            AddEvents(0, this._tableLayoutPanel_main.RowStyles.Count, bAdd);
#if NO
            for (int i = 0; i < this._tableLayoutPanel_main.RowStyles.Count; i++)
            {
                // 第一列
                Label label_control = this._tableLayoutPanel_main.GetControlFromPosition(0, i) as Label;
                if (label_control != null)
                {
                    if (bAdd)
                    {
                        label_control.Click += label_control_Click;
                    }
                    else
                    {
                        label_control.Click -= label_control_Click;
                    }
                }

                // 第二列
                Label color_control = this._tableLayoutPanel_main.GetControlFromPosition(1, i) as Label;
                if (color_control != null)
                {
                    if (bAdd)
                    {
                        color_control.Click += color_control_Click;
                    }
                    else
                    {
                        color_control.Click -= color_control_Click;
                    }
                }

                // 第三列
                Control edit_control = this._tableLayoutPanel_main.GetControlFromPosition(2, i);
                if (edit_control != null)
                {
                    if (bAdd)
                    {
                        edit_control.Enter += control_Enter;
                        edit_control.Leave += control_Leave;
                        if (edit_control is DateControl)
                            (edit_control as DateControl).DateTextChanged += control_TextChanged;
                        else if (edit_control is DateTimePicker)
                            (edit_control as DateTimePicker).ValueChanged += control_TextChanged;
                        else
                            edit_control.TextChanged += control_TextChanged;

                        if (edit_control is ComboBox)
                            edit_control.SizeChanged += control_SizeChanged;

                        edit_control.PreviewKeyDown += edit_control_PreviewKeyDown;
                        edit_control.KeyDown += edit_control_KeyDown;
                    }
                    else
                    {
                        edit_control.Enter -= control_Enter;
                        edit_control.Leave -= control_Leave;
                        if (edit_control is DateControl)
                            (edit_control as DateControl).DateTextChanged -= control_TextChanged;
                        else if (edit_control is DateTimePicker)
                            (edit_control as DateTimePicker).ValueChanged -= control_TextChanged;
                        else
                            edit_control.TextChanged -= control_TextChanged;

                        if (edit_control is ComboBox)
                            edit_control.SizeChanged += control_SizeChanged;

                        edit_control.PreviewKeyDown -= edit_control_PreviewKeyDown;
                        edit_control.KeyDown -= edit_control_KeyDown;

                    }
                }
            }
#endif
        }

        // parameters:
        //      nStart  包含 nStart
        //      nEnd    不包含 nEnd
        void AddEvents(int nStart,
            int nEnd,
            bool bAdd)
        {
            for (int i = nStart; i < nEnd; i++)
            {
                // 第一列
                Label label_control = this._tableLayoutPanel_main.GetControlFromPosition(0, i) as Label;
                if (label_control != null)
                {
                    if (bAdd)
                    {
                        label_control.Click += label_control_Click;
                    }
                    else
                    {
                        label_control.Click -= label_control_Click;
                    }
                }

                // 第二列
                Label color_control = this._tableLayoutPanel_main.GetControlFromPosition(1, i) as Label;
                if (color_control != null)
                {
                    if (bAdd)
                    {
                        color_control.Click += color_control_Click;
                    }
                    else
                    {
                        color_control.Click -= color_control_Click;
                    }
                }

                // 第三列
                Control edit_control = this._tableLayoutPanel_main.GetControlFromPosition(2, i);
                if (edit_control != null)
                {
                    if (bAdd)
                    {
                        edit_control.Enter += control_Enter;
                        edit_control.Leave += control_Leave;
                        if (edit_control is DateControl)
                            (edit_control as DateControl).DateTextChanged += control_TextChanged;
                        else if (edit_control is DateTimePicker)
                            (edit_control as DateTimePicker).ValueChanged += control_TextChanged;
                        else
                            edit_control.TextChanged += control_TextChanged;

                        if (edit_control is ComboBox)
                            edit_control.SizeChanged += control_SizeChanged;

                        edit_control.PreviewKeyDown += edit_control_PreviewKeyDown;
                        edit_control.KeyDown += edit_control_KeyDown;
                    }
                    else
                    {
                        edit_control.Enter -= control_Enter;
                        edit_control.Leave -= control_Leave;
                        if (edit_control is DateControl)
                            (edit_control as DateControl).DateTextChanged -= control_TextChanged;
                        else if (edit_control is DateTimePicker)
                            (edit_control as DateTimePicker).ValueChanged -= control_TextChanged;
                        else
                            edit_control.TextChanged -= control_TextChanged;

                        if (edit_control is ComboBox)
                            edit_control.SizeChanged += control_SizeChanged;

                        edit_control.PreviewKeyDown -= edit_control_PreviewKeyDown;
                        edit_control.KeyDown -= edit_control_KeyDown;

                    }
                }
            }

        }

        public void OnSelectionChanged(EventArgs e)
        {
            if (this.SelectionChanged != null)
                this.SelectionChanged(this, e);
        }

        // 获得当前具有输入焦点的一行
        public virtual EditLine GetFocuedLine()
        {
            for (int i = 0; i < this._tableLayoutPanel_main.RowStyles.Count; i++)
            {
                // 第三列
                Control edit_control = this._tableLayoutPanel_main.GetControlFromPosition(2, i);
                if (edit_control != null
                    && edit_control.Visible == true
                    && edit_control.Enabled == true
                    // && edit_control.Focused == true
                    )
                {
                    EditLineState state = GetLineState(i);
                    if (state != null && state.Active == true)
                    {
                        EditLine line = new EditLine(this, i);
                        return line;
                    }
                }
            }
            return null;
        }

        // 得到下一个可以输入内容的行对象
        // parameters:
        //      ref_line    参考的对象。从它后面一个开始获取。如果为 null，表示获取第一个可编辑的对象
        public virtual EditLine GetNextEditableLine(EditLine ref_line)
        {
            bool bOn = false;
            if (ref_line == null)
                bOn = true;
            for (int i = 0; i < this._tableLayoutPanel_main.RowStyles.Count; i++)
            {
                // 第一列
                Label label_control = this._tableLayoutPanel_main.GetControlFromPosition(0, i) as Label;
                if (label_control == null)
                    continue;

                if (ref_line != null && label_control == ref_line._labelCaption)
                {
                    bOn = true;
                    continue;
                }

                if (bOn == true)
                {
                    // 第三列
                    Control edit_control = this._tableLayoutPanel_main.GetControlFromPosition(2, i);
                    if (edit_control != null
                        && edit_control.Visible == true
                        && edit_control.Enabled == true)
                    {
                        if (edit_control is TextBox)
                        {
                            if ((edit_control as TextBox).ReadOnly == true)
                                continue;
                        }
                        EditLine line = new EditLine(this, i);
                        return line;
                    }
                }
            }
            return null;
        }

        // 清除全部行的 Active 状态
        public bool ClearActiveState(EditLine exclude = null)
        {
            bool bSelectionChanged = false;

            for (int i = 0; i < this._tableLayoutPanel_main.RowStyles.Count; i++)
            {
                // 第一列
                Label label_control = this._tableLayoutPanel_main.GetControlFromPosition(0, i) as Label;
                if (label_control == null)
                    continue;
                if (exclude != null)
                {
                    if (label_control == exclude._labelCaption)
                        continue;   // 跳过要排除的一行
                }
#if NO
                // 第二列
                label_control = this._tableLayoutPanel_main.GetControlFromPosition(1, i) as Label;
                if (label_control == null)
                    continue;
#endif

                EditLineState state = GetLineState(i);
                if (state != null && state.Active == true)
                {
                    state.Active = false;
                    SetLineState(i, state);

                    bSelectionChanged = true;
                }
            }
            if (bSelectionChanged)
                this.OnSelectionChanged(new EventArgs());

            return bSelectionChanged;
        }

        // 设置一行的状态
        public void SetLineState(EditLine line, EditLineState new_state)
        {
            bool bNotified = false;
            // 需要把其他事项的 Active 状态全部设置为 false
            if (new_state.Active == true)
            {
                bNotified = ClearActiveState(line);
            }
            SetLineState(line.EditControl, new_state);
            if (bNotified == false)
                OnSelectionChanged(new EventArgs());
        }

        void tableLayoutPanel_main_MouseDown(object sender, MouseEventArgs e)
        {
            this.OnMouseDown(e);
        }

        void tableLayoutPanel_main_MouseClick(object sender, MouseEventArgs e)
        {
            this.OnMouseClick(e);
        }

        void edit_control_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Enter:
                case Keys.LineFeed:
                case Keys.Down:
                    if (this.SelectNextControl((sender as Control), true, true, true, false) == false)
                        SendKeys.Send("{TAB}");
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
                    if (this.SelectNextControl((sender as Control), false, true, true, false) == false)
                        SendKeys.Send("+{TAB}");
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                    break;
            }
        }

        void edit_control_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {

        }

        void color_control_Click(object sender, EventArgs e)
        {
            FocusLine(sender as Control);
        }

        void label_control_Click(object sender, EventArgs e)
        {
            FocusLine(sender as Control);
        }

        // 将输入焦点切换到一个控件所在行的 edit 控件上
        void FocusLine(Control control)
        {
            // 找到同一行的 edit control
            int nRow = this._tableLayoutPanel_main.GetCellPosition(control).Row;
            Control edit_control = this._tableLayoutPanel_main.GetControlFromPosition(2, nRow);
            if (edit_control != null)
            {
                edit_control.Focus();
            }
        }

        // 解决 Flat 风格 ComboBox 在改变大小的时候残留显示的问题
        void control_SizeChanged(object sender, EventArgs e)
        {
            (sender as Control).Invalidate();
        }

        void control_Leave(object sender, EventArgs e)
        {
            //if (this.m_nInSuspend == 0)
            {
                Control control = sender as Control;
                EditLineState state = GetLineState(control);

                if (state == null)
                    state = new EditLineState();

                bool bSelectionChanged = state.Active == true;

                if (state.Active == true)
                {
                    state.Active = false;
                    SetLineState(control, state);
                }

                if (bSelectionChanged)
                    this.OnSelectionChanged(new EventArgs());
            }
        }

        void control_Enter(object sender, EventArgs e)
        {
            //if (this.m_nInSuspend == 0)
            {
                Control control = sender as Control;

                ClearActiveState();

                EditLineState state = GetLineState(control);

                if (state == null)
                    state = new EditLineState();

                bool bSelectionChanged = state.Active == false;

                // if (state.Active == false)
                {
                    state.Active = true;
                    SetLineState(control, state);
                }

                if (bSelectionChanged)
                    this.OnSelectionChanged(new EventArgs());
            }
        }

        void control_TextChanged(object sender, EventArgs e)
        {
            Control control = sender as Control;

            /*
            if (m_bInInitial == true)
                return;
            */

            EditLineState state = TryGetLineState(control, out Exception ex);

            if (state == null)
            {
                if (ex != null)
                    return;
                state = new EditLineState();
            }

            string old_state = state.ToString();

            // 触发内容校验
            if (this.VerifyContent != null)
            {
                var args = new VerifyEditEventArgs
                {
                    EditControl = control,
                    EditName = GetControlName(control),
                    Text = control.Text,
                };
                this.VerifyContent?.Invoke(control, args);
                state.VerifyInfo = args.ErrorInfo;
                state.VerifyFail = string.IsNullOrEmpty(args.ErrorInfo) == false;
            }

            if (m_bInInitial == false)
            {
                if (state.Changed == false)
                {
                    state.Changed = true;
                    // SetLineState(control, state);
                }
                this.Changed = true;
            }

            if (state.ToString() != old_state)
                SetLineState(control, state);
        }

        public static string GetControlName(Control control)
        {
            string name = control.Tag as string;
            return name;
        }

        /// <summary>
        /// 编辑器行的状态
        /// </summary>
        public class EditLineState
        {
            /// <summary>
            /// 是否发生过修改
            /// </summary>
            public bool Changed { get; set; }
            /// <summary>
            /// 是否处在输入焦点状态
            /// </summary>
            public bool Active { get; set; }

            // 2022/10/28
            // 内容是否错误
            public bool VerifyFail { get; set; }
            public string VerifyInfo { get; set; }

            public override string ToString()
            {
                return $"Changed={Changed},Active={Active},VerifyFail={VerifyFail},VerifyInfo={VerifyInfo}";
            }
        }

        void SetLineState(Control control, EditLineState newState)
        {
            SetLineState(this._tableLayoutPanel_main.GetCellPosition(control).Row, newState);
        }

        // 设置一行的显示状态
        void SetLineState(int nRowNumber, EditLineState newState)
        {
            Label color = this._tableLayoutPanel_main.GetControlFromPosition(1, nRowNumber) as Label;
            if (color == null)
                throw new ArgumentException("行 " + nRowNumber.ToString() + " 的 Color Label 对象不存在", "nRowNumber");

            Control edit = this._tableLayoutPanel_main.GetControlFromPosition(2, nRowNumber);

            color.Tag = newState;
            ResetColor(color, edit);
        }

        public Color VerifyFailBackColor = Color.Red;
        public Color VerifyFailBackColorDark = Color.DarkRed;

        // 正在编辑的 edit 的背景颜色
        public Color FocusedEditBackColor = Color.FromArgb(200, 200, 255);

        internal Color _editBackColor = SystemColors.Window;
        internal Color _editForeColor = SystemColors.WindowText;

        public void SetAllEditColor(Color backColor, Color foreColor)
        {
            _editBackColor = backColor;
            _editForeColor = foreColor;

            for (int i = 0; i < this._tableLayoutPanel_main.RowStyles.Count; i++)
            {
                Control control = this._tableLayoutPanel_main.GetControlFromPosition(2, i);
                if (control != null)
                {
                    if (control is TextBox)
                    {
                        TextBox textbox = control as TextBox;
                        // readonly 的 TextBox 要显示为不同的颜色
                        if (textbox.ReadOnly == true)
                        {
                            if (this.BackColor != Color.Transparent)
                                textbox.BackColor = this.BackColor;
                        }
                        else
                        {
                            if (backColor != Color.Transparent)
                                textbox.BackColor = backColor;
                        }
                        textbox.ForeColor = foreColor;
                    }
                    else
                    {
                        control.BackColor = backColor;
                        control.ForeColor = foreColor;
                    }
                }
            }

        }

        void ResetColor(Label color, Control edit)
        {
            EditLineState newState = color.Tag as EditLineState;
            if (newState == null)
            {
                color.BackColor = this._tableLayoutPanel_main.BackColor;
                return;
            }

            {
                SetToolTip(color, newState.VerifyInfo);
            }

            if (newState.Active == true)
            {
                if (this.ContainsFocus == true)
                {
                    if (newState.VerifyFail == true)
                        color.BackColor = this.VerifyFailBackColor;
                    else
                        color.BackColor = SystemColors.Highlight;
                    Color focus_color = this.FocusedEditBackColor;
                    if (edit != null && edit.BackColor != focus_color)
                    {
                        edit.BackColor = focus_color;
                    }
                    return;
                }
                else
                {
                    if (this.m_bHideSelection == false)
                    {
                        if (newState.VerifyFail == true)
                            color.BackColor = this.VerifyFailBackColorDark;
                        else
                            color.BackColor = ControlPaint.Dark(SystemColors.Highlight);
                        Color focus_color = ControlPaint.Light(this.FocusedEditBackColor);
                        if (edit != null && edit.BackColor != focus_color)
                        {
                            edit.BackColor = focus_color;
                        }
                        return;
                    }
                }
            }


#if NO
            // 恢复原来的颜色
            if (edit != null && edit.Tag != null)
            {
                Color back_color = (Color)edit.Tag;
                if (edit.BackColor != back_color)
                    edit.BackColor = back_color;
            }
#endif
            if (edit != null)
            {
                if (edit is TextBox)
                {
                    TextBox textbox = edit as TextBox;
                    if (textbox.ReadOnly == true && this.BackColor != Color.Transparent)
                        textbox.BackColor = this.BackColor;
                    else
                        edit.BackColor = _editBackColor;
                }
                else
                    edit.BackColor = _editBackColor;
            }

            if (newState.VerifyFail == true)
                color.BackColor = this.VerifyFailBackColorDark;
            else
            {
                if (newState.Changed == true)
                    color.BackColor = this.ColorChanged;
                else
                    color.BackColor = this._tableLayoutPanel_main.BackColor;
            }
        }

        // 刷新所有行的颜色
        void RefreshLineColor()
        {
            for (int i = 0; i < this._tableLayoutPanel_main.RowStyles.Count; i++)
            {
                Label label = this._tableLayoutPanel_main.GetControlFromPosition(1, i) as Label;
                if (label == null)
                    continue;
                Control edit = this._tableLayoutPanel_main.GetControlFromPosition(2, i);
                ResetColor(label, edit);
            }
        }

        EditLineState GetLineState(Control control)
        {
            return GetLineState(this._tableLayoutPanel_main.GetCellPosition(control).Row);
        }

        EditLineState TryGetLineState(Control control, out Exception ex)
        {
            ex = null;
            try
            {
                return GetLineState(this._tableLayoutPanel_main.GetCellPosition(control).Row);
            }
            catch (ArgumentException e)
            {
                ex = e;
                return null;
            }
        }

        EditLineState GetLineState(int nRowNumber)
        {
            Label color = this._tableLayoutPanel_main.GetControlFromPosition(1, nRowNumber) as Label;
            if (color == null)
                throw new ArgumentException("行 " + nRowNumber.ToString() + " 的 Color Label 对象不存在", "nRowNumber");
            return color.Tag as EditLineState;
        }

        internal void OnPaintContent(object sender, PaintEventArgs e)
        {
            if (this.PaintContent != null)
                this.PaintContent(this, e);  // sender
        }

        internal void OnControlKeyDown(object sender, ControlKeyEventArgs e)
        {
            if (this.ControlKeyDown != null)
                this.ControlKeyDown(this, e);   // sender
        }

        internal void OnControlKeyPress(object sender, ControlKeyPressEventArgs e)
        {
            if (this.ControlKeyPress != null)
                this.ControlKeyPress(this, e);  // sender
        }

        internal void OnGetValueTable(object sender, GetValueTableEventArgs e)
        {
            if (this.GetValueTable != null)
                this.GetValueTable(this, e);  // sender
        }

        // 比较自己和refControl的数据差异，用特殊颜色显示差异字段
        /// <summary>
        /// 比较自己和refControl的数据差异，用特殊颜色显示差异字段
        /// </summary>
        /// <param name="r">要和自己进行比较的控件对象</param>
        public virtual void HighlightDifferences(ItemEditControlBase r)
        {
            throw new Exception("尚未实现 HighlightDifferences()");
        }

        /// <summary>
        /// 设置为可修改状态
        /// </summary>
        public virtual void SetChangeable()
        {
            throw new Exception("尚未实现 SetChangeable()");
        }

        /// <summary>
        /// 设置只读状态
        /// </summary>
        /// <param name="strStyle">如何设置只读状态</param>
        public virtual void SetReadOnly(string strStyle)
        {
            throw new Exception("尚未实现 SetReadonly()");
        }

        int m_nInSuspend = 0;

        public void DisableUpdate()
        {
            if (this.m_nInSuspend == 0
                && this._tableLayoutPanel_main != null)
            {
                this._tableLayoutPanel_main.SuspendLayout();
            }

            this.m_nInSuspend++;
        }

        // parameters:
        //      bOldVisible 如果为true, 表示真的要结束
        public void EnableUpdate()
        {
            this.m_nInSuspend--;

            if (this.m_nInSuspend == 0
                && this._tableLayoutPanel_main != null)
            {
                this._tableLayoutPanel_main.ResumeLayout(false);
                this._tableLayoutPanel_main.PerformLayout();
            }
        }

#if NO
        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // ItemEditControlBase
            // 
            this.Name = "ItemEditControlBase";
            this.Enter += new System.EventHandler(this.ItemEditControlBase_Enter);
            this.Leave += new System.EventHandler(this.ItemEditControlBase_Leave);
            this.ResumeLayout(false);

        }
#endif

#if NO
        internal bool m_bFocused = false;
        // 会引起焦点和选择的问题！
        private void ItemEditControlBase_Enter(object sender, EventArgs e)
        {
            if (this._tableLayoutPanel_main != null)
                this._tableLayoutPanel_main.Focus();
            this.m_bFocused = true;
            this.RefreshLineColor();
        }

        // 会引起焦点和选择的问题！
        private void ItemEditControlBase_Leave(object sender, EventArgs e)
        {
            this.m_bFocused = false;
            this.RefreshLineColor();
        }
#endif
        XmlDocument _configDom = null;

        // 从配置文件装载字段配置，初始化这些字段
        public int LoadConfig(string strFileName,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.Load(strFileName);
            }
            catch (Exception ex)
            {
                strError = "装入配置文件 '" + strFileName + "' 到 XMLDOM 时出错: " + ex.Message;
                return -1;
            }

            this._configDom = dom;

            // 找到当前最后一行有内容的 index
            int nStart = FindInsertLinePos();
            if (nStart == -1)
            {
                strError = "FindInsertLinePos() error";
                return -1;
            }

            XmlNodeList nodes = dom.DocumentElement.SelectNodes("field");
            if (nodes.Count == 0)
                return 0;

            this.DisableUpdate();
            try
            {
                this._tableLayoutPanel_main.RowCount += nodes.Count;
                int nRow = nStart;
                foreach (XmlElement field in nodes)
                {
                    string strElement = field.GetAttribute("element");
                    string strCaption = DomUtil.GetCaption("zh", field);
                    if (string.IsNullOrEmpty(strCaption) == true)
                        strCaption = strElement;

                    this._tableLayoutPanel_main.RowStyles.Insert(nRow, new System.Windows.Forms.RowStyle());

                    Label caption = new Label();
                    caption.Text = strCaption;
                    caption.TextAlign = ContentAlignment.MiddleLeft;
                    caption.Dock = DockStyle.Fill;

                    Label color = new Label();
                    color.Dock = DockStyle.Fill;

                    TextBox edit = new TextBox();
                    edit.Dock = DockStyle.Fill;
                    edit.Tag = strElement;  // 2021/7/21

                    this._tableLayoutPanel_main.Controls.Add(caption, 0, nRow);
                    this._tableLayoutPanel_main.Controls.Add(color, 1, nRow);
                    this._tableLayoutPanel_main.Controls.Add(edit, 2, nRow);

                    FieldDef def = new FieldDef();
                    def.Element = strElement;
                    caption.Tag = def;

                    nRow++;
                }
                AddEvents(nStart, nRow, true);
            }
            finally
            {
                this.EnableUpdate();
            }
            return 1;
        }

        class FieldDef
        {
            public string Element = ""; // 字段对应的元素名
        }

        // 寻找可以插入新行的位置
        int FindInsertLinePos()
        {
            for (int i = this._tableLayoutPanel_main.RowStyles.Count - 1; i >= 0; i--)
            {
                Control control = this._tableLayoutPanel_main.GetControlFromPosition(2, i);
                if (control != null)
                    return i + 1;
            }

            return -1;
        }

        public void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // ItemEditControlBase
            // 
            this.Name = "ItemEditControlBase";
            this.ResumeLayout(false);
        }

        public void SetToolTip(Control control, string text)
        {
            this.toolTip1?.SetToolTip(control, text);
        }
    }

    public class EditLine
    {
        internal ItemEditControlBase _container = null;
        internal Control _editBox = null;
        internal Label _labelCaption = null;
        internal Label _labelColor = null;

#if NO
        public EditLine(ItemEditControlBase container, Control textbox, Label labelCaption, Label labelColor)
        {
            this._container = container;
            this._editBox = textbox;
            this._labelCaption = labelCaption;
            this._labelColor = labelColor;
        }
#endif
        public EditLine(ItemEditControlBase container, int nRow)
        {
            this._container = container;

            // 第一列
            this._labelCaption = container._tableLayoutPanel_main.GetControlFromPosition(0, nRow) as Label;

            // 第二列
            this._labelColor = container._tableLayoutPanel_main.GetControlFromPosition(1, nRow) as Label;

            // 第三列
            this._editBox = container._tableLayoutPanel_main.GetControlFromPosition(2, nRow);
        }

        public virtual string Content
        {
            get
            {
                if (this._editBox == null)
                    return null;
                return this._editBox.Text;
            }
            set
            {
                if (this._editBox.Text != value)
                    this._editBox.Text = value;
            }
        }

        public virtual string Caption
        {
            get
            {
                if (this._labelCaption == null)
                    return null;
                return this._labelCaption.Text;
            }
            set
            {
                this._labelCaption.Text = value;
            }
        }

        public virtual Control EditControl
        {
            get
            {
                return this._editBox;
            }
        }

        public ItemEditControlBase Container
        {
            get
            {
                return this._container;
            }
        }

        // 获取或设置行的焦点状态
        public bool ActiveState
        {
            get
            {
                if (this._labelColor == null || this._labelColor.Tag == null)
                    return false;
                dp2Circulation.ItemEditControlBase.EditLineState state = this._labelColor.Tag as dp2Circulation.ItemEditControlBase.EditLineState;
                return state.Active;
            }
            set
            {
                if (this._labelColor == null)
                    return;
                if (this._labelColor.Tag == null)
                    this._labelColor.Tag = new dp2Circulation.ItemEditControlBase.EditLineState();

                dp2Circulation.ItemEditControlBase.EditLineState state = this._labelColor.Tag as dp2Circulation.ItemEditControlBase.EditLineState;
                if (state.Active == value)
                    return;

                bool bSelectionChanged = state.Active != value;

                state.Active = value;   // true ??
                this.Container.SetLineState(this, state);

                if (bSelectionChanged)
                    this._container.OnSelectionChanged(new EventArgs());
            }
        }
    }

    /// <summary>
    /// 只读状态风格
    /// </summary>
    public enum ReadOnlyStyle
    {
        /// <summary>
        /// 清除全部只读状态，恢复可编辑状态
        /// </summary>
        Clear = 0,  // 清除全部ReadOnly状态，恢复可编辑状态
        /// <summary>
        /// 全部只读
        /// </summary>
        All = 1,    // 全部禁止修改
        /// <summary>
        /// 图书馆一般工作人员，不能修改路径
        /// </summary>
        Librarian = 2,  // 图书馆工作人员，不能修改路径
        /// <summary>
        /// 读者。不能修改条码等许多字段
        /// </summary>
        Reader = 3, // 读者。不能修改条码等许多字段
        /// <summary>
        /// 装订操作者。除了图书馆一般工作人员不能修改的字段外，还不能修改卷、装订信息、操作者等
        /// </summary>
        Binding = 4,    // 装订操作者。除了图书馆一般工作人员不能修改的字段外，还不能修改卷、binding、操作者等

    }
}
