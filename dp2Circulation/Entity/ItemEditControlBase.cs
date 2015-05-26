using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Drawing;
using System.Diagnostics;

using DigitalPlatform;
using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    /// <summary>
    /// 册/订购/期/评注 编辑控件的基础类
    /// </summary>
    public class ItemEditControlBase : UserControl
    {
        internal TableLayoutPanel tableLayoutPanel_main = null;

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

        internal XmlDocument RecordDom = null;

        internal bool m_bChanged = false;

        internal bool m_bInInitial = true;   // 是否正在初始化过程之中

        internal Color ColorChanged = Color.Yellow; // 表示内容改变过的颜色
        internal Color ColorDifference = Color.Blue; // 表示内容有差异的颜色

        internal string m_strParentId = "";

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

        internal 
            // virtual 
            void ResetColor()
        {
            // throw new Exception("尚未实现 ResetColor()");

            for (int i = 0; i < this.tableLayoutPanel_main.RowStyles.Count; i++)
            {
                Label color = this.tableLayoutPanel_main.GetControlFromPosition(2, i) as Label;
                if (color == null)
                    continue;
                EditLineState state = color.Tag as EditLineState;
                if (state != null)
                {
                    if (state.Changed == true)
                        state.Changed = false;
                }
                color.BackColor = this.tableLayoutPanel_main.BackColor;
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
                    this.ResetColor();

                // 触发事件
                if (bOldValue != value && this.ContentChanged != null)
                {
                    ContentChangedEventArgs e = new ContentChangedEventArgs();
                    e.OldChanged = bOldValue;
                    e.CurrentChanged = value;
                    ContentChanged(this, e);
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

            this.RecordDom = new XmlDocument();

            try
            {
                if (String.IsNullOrEmpty(strXml) == true)
                    this.RecordDom.LoadXml("<root />");
                else
                    this.RecordDom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML数据装载到DOM时出错" + ex.Message;
                return -1;
            }

            this.Initializing = true;
            try
            {
                this.DomToMember(strRecPath);
            }
            finally
            {
                this.Initializing = false;
            }

            this.Changed = false;

            return 0;
        }

        internal virtual void DomToMember(string strRecPath)
        {
            throw new Exception("尚未实现 DomToMember()");
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
                if (this.RecordDom == null)
                {
                    this.RecordDom = new XmlDocument();
                    this.RecordDom.LoadXml("<root />");
                }
                this.RefreshDom();
                return this.RecordDom;
            }
        }

        // member --> dom
        internal virtual void RefreshDom()
        {
            throw new Exception("尚未实现 RefreshDom()");
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

            if (this.RecordDom == null)
            {
                this.RecordDom = new XmlDocument();
                this.RecordDom.LoadXml("<root />");
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
                strError = ex.Message;
                return -1;
            }

            strXml = this.RecordDom.OuterXml;

            return 0;
        }

        // 添加、删除各种事件
        internal void AddEvents(bool bAdd)
        {
            Debug.Assert(this.tableLayoutPanel_main != null, "");

            for (int i = 0; i < this.tableLayoutPanel_main.RowStyles.Count; i++)
            {
                Control control = this.tableLayoutPanel_main.GetControlFromPosition(2, i);
                if (control == null)
                    continue;
                if (bAdd)
                {
                    control.Enter += control_Enter;
                    control.Leave += control_Leave;
                    if (control is DateControl)
                        (control as DateControl).DateTextChanged += control_TextChanged;
                    else if (control is DateTimePicker)
                        (control as DateTimePicker).ValueChanged += control_TextChanged;
                    else
                        control.TextChanged += control_TextChanged;

                    if (control is ComboBox)
                        control.SizeChanged += control_SizeChanged;
                }
                else
                {
                    control.Enter -= control_Enter;
                    control.Leave -= control_Leave;
                    if (control is DateControl)
                        (control as DateControl).DateTextChanged -= control_TextChanged;
                    else if (control is DateTimePicker)
                        (control as DateTimePicker).ValueChanged -= control_TextChanged;
                    else
                        control.TextChanged -= control_TextChanged;

                    if (control is ComboBox)
                        control.SizeChanged += control_SizeChanged;
                }
            }

        }

        // 解决 Flat 风格 ComboBox 在改变大小的时候残留显示的问题
        void control_SizeChanged(object sender, EventArgs e)
        {
            (sender as Control).Invalidate();
        }

        void control_Leave(object sender, EventArgs e)
        {
            Control control = sender as Control;
            EditLineState state = GetLineState(control);

            if (state == null)
                state = new EditLineState();

            if (state.Active == true)
            {
                state.Active = false;
                SetLineState(control, state);
            }
        }

        void control_Enter(object sender, EventArgs e)
        {
            Control control = sender as Control;
            EditLineState state = GetLineState(control);

            if (state == null)
                state = new EditLineState();

            if (state.Active == false)
            {
                state.Active = true;
                SetLineState(control, state);
            }
        }

        void control_TextChanged(object sender, EventArgs e)
        {
            if (m_bInInitial == true)
                return;

            // this.label_barcode_color.BackColor = this.ColorChanged;
            Control control = sender as Control;
            EditLineState state = GetLineState(control);

            if (state == null)
                state = new EditLineState();

            if (state.Changed == false)
            {
                state.Changed = true;
                SetLineState(control, state);
            }
            this.Changed = true;

        }


        /// <summary>
        /// 编辑器行的状态
        /// </summary>
        public class EditLineState
        {
            /// <summary>
            /// 是否发生过修改
            /// </summary>
            public bool Changed = false;
            /// <summary>
            /// 是否处在输入焦点状态
            /// </summary>
            public bool Active = false;
        }

        void SetLineState(Control control, EditLineState newState)
        {
            SetLineDisplayState(this.tableLayoutPanel_main.GetCellPosition(control).Row, newState);
        }

        // 设置一行的显示状态
        void SetLineDisplayState(int nRowNumber, EditLineState newState)
        {
            Label color = this.tableLayoutPanel_main.GetControlFromPosition(1, nRowNumber) as Label;
            if (color == null)
                throw new ArgumentException("行 " + nRowNumber.ToString() + " 的 Color Label 对象不存在", "nRowNumber");

            color.Tag = newState;
            if (newState.Active == true)
                color.BackColor = SystemColors.Highlight;
            else if (newState.Changed == true)
                color.BackColor = this.ColorChanged;
            else
                color.BackColor = this.tableLayoutPanel_main.BackColor;
        }

        EditLineState GetLineState(Control control)
        {
            return GetLineState(this.tableLayoutPanel_main.GetCellPosition(control).Row);
        }

        EditLineState GetLineState(int nRowNumber)
        {
            Label color = this.tableLayoutPanel_main.GetControlFromPosition(1, nRowNumber) as Label;
            if (color == null)
                throw new ArgumentException("行 " + nRowNumber.ToString() + " 的 Color Label 对象不存在", "nRowNumber");
            return color.Tag as EditLineState;
        }


        internal void OnPaintContent(object sender, PaintEventArgs e)
        {
            if (this.PaintContent != null)
                this.PaintContent(sender, e);

        }

        internal void OnControlKeyDown(object sender, ControlKeyEventArgs e)
        {
            if (this.ControlKeyDown != null)
                this.ControlKeyDown(sender, e);
        }

        internal void OnControlKeyPress(object sender, ControlKeyPressEventArgs e)
        {
            if (this.ControlKeyPress != null)
                this.ControlKeyPress(sender, e);
        }

        internal void OnGetValueTable(object sender, GetValueTableEventArgs e)
        {
            if (this.GetValueTable != null)
                this.GetValueTable(sender, e);
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
