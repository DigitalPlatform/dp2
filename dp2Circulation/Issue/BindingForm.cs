using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

using DigitalPlatform;
using DigitalPlatform.Xml;
using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    // C# 脚本中要用到这个类型 is BindingForm
    /// <summary>
    /// 显示期刊装订图形界面的对话框
    /// </summary>
    public partial class BindingForm : Form
    {
        // Ctrl+A自动创建数据
        /// <summary>
        /// 自动创建数据
        /// </summary>
        public event GenerateDataEventHandler GenerateData = null;

        public event GetBiblioEventHandler GetBiblio = null;

        const int WM_ENSURE_VISIBLE = API.WM_USER + 200;

        /// <summary>
        /// 框架窗口
        /// </summary>
        public MainForm MainForm = null;

        /// <summary>
        /// 期刊控件所关联的 ApplicationInfo 对象
        /// </summary>
        public ApplicationInfo AppInfo
        {
            get
            {
                return this.bindingControl1.AppInfo;
            }
            set
            {
                this.bindingControl1.AppInfo = value;
            }
        }

        /// <summary>
        /// 获得订购信息
        /// </summary>
        public event GetOrderInfoEventHandler GetOrderInfo = null;

        // public event GetItemInfoEventHandler GetItemInfo = null;

        /// <summary>
        /// 获得值列表
        /// </summary>
        public event GetValueTableEventHandler GetValueTable = null;

        /// <summary>
        /// 构造函数
        /// </summary>
        public BindingForm()
        {
            InitializeComponent();

            this.bindingControl1.GetBiblio += bindingControl1_GetBiblio;
        }

        void bindingControl1_GetBiblio(object sender, GetBiblioEventArgs e)
        {
            var func = this.GetBiblio;
            if (func != null)
                func(sender, e);
        }

        private void BindingForm_Load(object sender, EventArgs e)
        {
            if (this.MainForm != null)
            {
                MainForm.SetControlFont(this, this.MainForm.DefaultFont);
            }

            this.bindingControl1.GetOrderInfo -= new GetOrderInfoEventHandler(bindingControl1_GetOrderInfo);
            this.bindingControl1.GetOrderInfo += new GetOrderInfoEventHandler(bindingControl1_GetOrderInfo);

#if OLD_INITIAL
            this.bindingControl1.GetItemInfo -= new GetItemInfoEventHandler(bindingControl1_GetItemInfo);
            this.bindingControl1.GetItemInfo += new GetItemInfoEventHandler(bindingControl1_GetItemInfo);
#endif

            this.entityEditControl1.GetValueTable -= new GetValueTableEventHandler(orderDesignControl1_GetValueTable);
            this.entityEditControl1.GetValueTable += new GetValueTableEventHandler(orderDesignControl1_GetValueTable);

            this.entityEditControl1.SetReadOnly("binding");

            this.entityEditControl1.GetAccessNoButton.Click -= new EventHandler(button_getAccessNo_Click);
            this.entityEditControl1.GetAccessNoButton.Click += new EventHandler(button_getAccessNo_Click);

            this.orderDesignControl1.ArriveMode = true;
            this.orderDesignControl1.SeriesMode = true;
            this.orderDesignControl1.Changed = false;

            this.orderDesignControl1.GetValueTable -= new GetValueTableEventHandler(orderDesignControl1_GetValueTable);
            this.orderDesignControl1.GetValueTable += new GetValueTableEventHandler(orderDesignControl1_GetValueTable);

            LoadState();

            this.MainForm.LoadSplitterPos(
this.splitContainer_main,
"bindingform",
"main_splitter_pos");

            API.PostMessage(this.Handle, WM_ENSURE_VISIBLE, 0, 0);
        }

        // 获得索取号
        void button_getAccessNo_Click(object sender, EventArgs e)
        {
            if (this.GenerateData != null)
            {
                GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                if (Control.ModifierKeys == Keys.Control)
                    e1.ScriptEntry = "ManageCallNumber";
                else
                    e1.ScriptEntry = "CreateCallNumber";
                e1.FocusedControl = sender; // sender为最原始的子控件
                this.GenerateData(this, e1);
            }
        }

#if NO
        public List<CallNumberItem> GetCallNumberItems()
        {
            List<CallNumberItem> callnumber_items = this.BookItems.GetCallNumberItems();

            CallNumberItem item = null;

            int index = this.BookItems.IndexOf(this.BookItem);
            if (index == -1)
            {
                // 增补一个对象
                item = new CallNumberItem();
                callnumber_items.Add(item);

                item.CallNumber = "";   // 不要给出当前的，以免影响到取号结果
            }
            else
            {
                // 刷新自己的位置
                item = callnumber_items[index];
                item.CallNumber = entityEditControl_editing.AccessNo;
            }

            item.RecPath = this.entityEditControl_editing.RecPath;
            item.Location = entityEditControl_editing.LocationString;
            item.Barcode = entityEditControl_editing.Barcode;

            return callnumber_items;
        }
#endif

        /// <summary>
        /// 获得索取号事项数组
        /// </summary>
        /// <returns>CallNumberItem 数组</returns>
        public List<CallNumberItem> GetCallNumberItems()
        {
            ItemBindingItem cur_item = null;
            if (this.m_item is ItemBindingItem)
            {
                cur_item = (ItemBindingItem)this.m_item;
            }

            // 返回同一种期刊内的全部册事项信息
            List<CallNumberItem> callnumber_items = this.bindingControl1.GetCallNumberItems(cur_item);

            {
                CallNumberItem item = null;
                // 增补一个对象
                item = new CallNumberItem();
                callnumber_items.Add(item);

                item.CallNumber = "";   // 不要给出当前的，以免影响到取号结果

                item.RecPath = this.entityEditControl1.RecPath;
                item.Location = entityEditControl1.LocationString;
                item.Barcode = entityEditControl1.Barcode;
            }

            // FOUND:
            return callnumber_items;
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_ENSURE_VISIBLE:
                    this.bindingControl1.EnsureCurrentIssueVisible();
                    return;
            }
            base.DefWndProc(ref m);
        }

        /// <summary>
        /// 装载以前存储的状态
        /// </summary>
        public void LoadState()
        {
            bool bEditAreaVisible = false;
            bool bNeedInvalidate = false;
            bool bNeedRelayout = false;

            if (this.AppInfo != null)
            {
                bEditAreaVisible = this.AppInfo.GetBoolean("bindingform",
                    "edit_area_visible", false);
            }
            // 一开始编辑区域是隐藏的？或者保持上次的状态
            VisibleEditArea(bEditAreaVisible);

            string strSplitterDirection = this.AppInfo.GetString(
                "binding_form",
                "splitter_direction",
                "水平");

            if (strSplitterDirection == "垂直")
                this.splitContainer_main.Orientation = Orientation.Horizontal;
            else
                this.splitContainer_main.Orientation = Orientation.Vertical;

            // 显示订购信息坐标值
            bool bValue = this.AppInfo.GetBoolean(
                "binding_form",
                "display_orderinfoxy",
                false);
            if (this.bindingControl1.DisplayOrderInfoXY != bValue)
            {
                this.bindingControl1.DisplayOrderInfoXY = bValue;
                bNeedInvalidate = true;
            }

            // 显示分馆外订购组
            bValue = this.AppInfo.GetBoolean(
               "binding_form",
               "display_lockedOrderGroup",
               true);
            if (this.bindingControl1.HideLockedOrderGroup != !bValue)
            {
                this.bindingControl1.HideLockedOrderGroup = !bValue;
                bNeedRelayout = true;
            }

            // 验收批次号
            this.AcceptBatchNo = this.AppInfo.GetString(
                "binding_form",
                "accept_batchno",
                "");

            // 册格子内容行
            {
                string strLinesCfg = this.AppInfo.GetString(
        "binding_form",
        "cell_lines_cfg",
        "");
                if (String.IsNullOrEmpty(strLinesCfg) == false)
                {
                    string[] parts = strLinesCfg.Split(new char[] { ',' });
                    this.bindingControl1.TextLineNames = parts;
                    bNeedInvalidate = true;
                }
                else
                {
                    if (this.bindingControl1.TextLineNames != this.bindingControl1.DefaultTextLineNames)
                    {
                        this.bindingControl1.TextLineNames = this.bindingControl1.DefaultTextLineNames;
                        bNeedInvalidate = true;
                    }
                }
            }

            // 组格子内容行
            {
                string strLinesCfg = this.AppInfo.GetString(
        "binding_form",
        "group_lines_cfg",
        "");
                if (String.IsNullOrEmpty(strLinesCfg) == false)
                {
                    string[] parts = strLinesCfg.Split(new char[] { ',' });
                    this.bindingControl1.GroupTextLineNames = parts;
                    bNeedInvalidate = true;
                }
                else
                {
                    if (this.bindingControl1.GroupTextLineNames != this.bindingControl1.DefaultGroupTextLineNames)
                    {
                        this.bindingControl1.GroupTextLineNames = this.bindingControl1.DefaultGroupTextLineNames;
                        bNeedInvalidate = true;
                    }
                }
            }

            if (bNeedInvalidate == true)
            {
                this.bindingControl1.Invalidate();
            }

            if (bNeedRelayout == true)
            {
                // if (this.bindingControl1.HideLockedOrderGroup == false)
                {
                    string strError = "";
                    // 把那些当前隐藏的合订册和成员册试图重新安放一次
                    int nRet = this.bindingControl1.RelayoutHiddenBindingCell(out strError);
                    if (nRet == -1)
                        MessageBox.Show(this, strError);
                }
                this.bindingControl1.RefreshLayout();
            }
        }

        private void BindingForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.bindingControl1.Changed == true
                && this.DialogResult == DialogResult.Cancel)
            {
                DialogResult dialog_result = MessageBox.Show(this,
"窗口内容发生过修改，若此时关闭窗口将导致这些修改丢失。\r\n\r\n确实要关闭窗口？",
"BindingControls",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                if (dialog_result == DialogResult.No)
                {
                    e.Cancel = true;
                    return;
                }
            }

            if (this.MainForm != null && this.MainForm.AppInfo != null)
            {
                // 分割条位置
                this.MainForm.SaveSplitterPos(
                    this.splitContainer_main,
                    "bindingform",
                    "main_splitter_pos");

                // 显示订购信息坐标值
                this.MainForm.AppInfo.SetBoolean(
                    "binding_form",
                    "display_orderinfoxy",
                    this.bindingControl1.DisplayOrderInfoXY);

                // 显示分馆外订购组
                this.MainForm.AppInfo.SetBoolean(
                    "binding_form",
                    "display_lockedOrderGroup",
                    !this.bindingControl1.HideLockedOrderGroup);
            }

        }

        private void BindingForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            if (this.AppInfo != null)
            {
                this.AppInfo.SetBoolean("bindingform",
                    "edit_area_visible",
                    this.m_bEditAreaVisible);

                this.AppInfo.SetString(
                    "binding_form",
                    "accept_batchno",
                    this.AcceptBatchNo);
            }
        }

        void orderDesignControl1_GetValueTable(object sender, GetValueTableEventArgs e)
        {
            if (this.GetValueTable != null)
                this.GetValueTable(sender, e);
        }

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed
        {
            get
            {
                return this.bindingControl1.Changed;
            }
            set
            {
                this.bindingControl1.Changed = value;
            }
        }

#if NO
        internal List<IssueBindingItem> Items   // 是否笔误?
        {
            get
            {
                return this.bindingControl1.Issues;
            }
        }
#endif

        internal List<ItemBindingItem> BindItems
        {
            get
            {
                return this.bindingControl1.ParentItems;
            }
        }

        internal List<ItemBindingItem> AllItems
        {
            get
            {
                return this.bindingControl1.AllItems;
            }
        }

        internal List<IssueBindingItem> Issues
        {
            get
            {
                return this.bindingControl1.Issues;
            }
        }

        /// <summary>
        /// 册记录编辑控件。当前选定的册记录
        /// </summary>
        public EntityEditControl EntityEditControl
        {
            get
            {
                return this.entityEditControl1;
            }
        }

#if OLD_INITIAL
        // 初始化期间，追加一个期对象
        public IssueBindingItem AppendIssue(string strXml,
            out string strError)
        {
            if (this.bindingControl1.HasGetItemInfo() == false)
            {
                this.bindingControl1.GetItemInfo += new GetItemInfoEventHandler(bindingControl1_GetItemInfo);
            }

            return this.bindingControl1.AppendIssue(strXml, out strError);
        }

        // 初始化期间，追加一个合订册对象
        public ItemBindingItem AppendBindItem(string strXml,
            out string strError)
        {
            if (this.bindingControl1.HasGetItemInfo() == false)
            {
                this.bindingControl1.GetItemInfo += new GetItemInfoEventHandler(bindingControl1_GetItemInfo);
            }

            return this.bindingControl1.AppendBindItem(strXml, 
                out strError);
        }

        public List<string> AllIssueMembersRefIds
        {
            get
            {
                return this.bindingControl1.AllIssueMembersRefIds;
            }
        }

        public int AppendNoneIssueSingleItems(List<string> XmlRecords,
    out string strError)
        {
            return this.bindingControl1.AppendNoneIssueSingleItems(XmlRecords,
                out strError);
        }


        // 初始化
        public int Initial(out string strError)
        {
            if (this.bindingControl1.HasGetItemInfo() == false)
            {
                this.bindingControl1.GetItemInfo += new GetItemInfoEventHandler(bindingControl1_GetItemInfo);
            }

            return this.bindingControl1.Initial(out strError);
        }
#endif

        // 初始化
        // parameters:
        //      strLayoutMode   "auto" "accepting" "binding"。auto为自动模式，accepting为全部行为记到，binding为全部行为装订
        // return:
        //      -1  出错
        //      0   成功
        //      1   成功，但有警告。警告信息在strError中
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="strLayoutMode">布局模式。"auto" "accepting" "binding" 之一。auto 为自动模式，accepting 为全部行为记到，binding 为全部行为装订</param>
        /// <param name="ItemXmls">册记录 XML 数组</param>
        /// <param name="IssueXmls">期记录 XML 数组</param>
        /// <param name="IssueObjectXmls">期记录对象资源的 XML 数组</param>
        /// <param name="strError">返回出错信息</param>
        /// <returns>
        /// <para>-1: 出错</para>>
        /// <para>0: 成功</para>>
        /// <para>1: 成功，但有警告。警告信息在 strError 中</para>>
        /// </returns>
        public int NewInitial(
            string strLayoutMode,
            List<string> ItemXmls,
            List<string> IssueXmls,
            List<string> IssueObjectXmls,
            out string strError)
        {
            if (this.bindingControl1.HasGetOrderInfo() == false)
            {
                this.bindingControl1.GetOrderInfo += new GetOrderInfoEventHandler(bindingControl1_GetOrderInfo);
            }

            return this.bindingControl1.NewInitial(
                strLayoutMode,
                ItemXmls,
                IssueXmls,
                IssueObjectXmls,
                out strError);
        }

        /// <summary>
        /// 是否为新创建的册记录设置“加工中”状态
        /// </summary>
        public bool SetProcessingState
        {
            get
            {
                return this.bindingControl1.SetProcessingState;
            }
            set
            {
                this.bindingControl1.SetProcessingState = value;
            }
        }

        /// <summary>
        /// 验收批次号
        /// </summary>
        public string AcceptBatchNo
        {
            get
            {
                return this.bindingControl1.AcceptBatchNo;
            }
            set
            {
                this.bindingControl1.AcceptBatchNo = value;
            }
        }

        /// <summary>
        /// 验收批次号是否已经在界面被输入了
        /// </summary>
        public bool AcceptBatchNoInputed
        {
            get
            {
                return this.bindingControl1.AcceptBatchNoInputed;
            }
            set
            {
                this.bindingControl1.AcceptBatchNoInputed = value;
            }
        }

        // 获取值列表时作为线索的数据库名
        /// <summary>
        /// 书目库名。获取值列表时作为线索的数据库名
        /// </summary>
        public string BiblioDbName
        {
            get
            {
                return this.bindingControl1.BiblioDbName;
            }
            set
            {
                this.bindingControl1.BiblioDbName = value;
            }
        }

        /// <summary>
        /// 当前操作者帐户名
        /// </summary>
        public string Operator
        {
            get
            {
                return this.bindingControl1.Operator;
            }
            set
            {
                this.bindingControl1.Operator = value;
            }
        }

        /// <summary>
        /// 当前用户管辖的馆代码列表
        /// </summary>
        public string LibraryCodeList
        {
            get
            {
                return this.bindingControl1.LibraryCodeList;
            }
            set
            {
                this.bindingControl1.LibraryCodeList = value;
            }
        }

        /*
        void bindingControl1_GetItemInfo(object sender, GetItemInfoEventArgs e)
        {
            if (this.GetItemInfo != null)
                this.GetItemInfo(sender, e);
        }
         * */

        void bindingControl1_GetOrderInfo(object sender, GetOrderInfoEventArgs e)
        {
            if (this.GetOrderInfo != null)
                this.GetOrderInfo(sender, e);
        }

        private void button_OK_Click(object sender, EventArgs e)
        {
            string strError = "";
            // 收尾最后一次在编辑区域的修改
            BackItem();

            // 兑现最后一些状态
            int nRet = this.bindingControl1.Finish(out strError);
            if (nRet == -1)
                goto ERROR1;

            // 检查
            nRet = this.bindingControl1.Check(out strError);
            if (nRet == -1)
                goto ERROR1;

            this.DialogResult = DialogResult.OK;
            this.Close();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void button_Cancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

#if ORDERDESIGN_CONTROL
        void BackIssue()
        {
            if (!(this.m_item is IssueBindingItem))
            {
                return;
            }

            IssueBindingItem issue = (IssueBindingItem)this.m_item;

            string strError = "";
            int nRet = 0;

            // 收尾上次的对象，从编辑器到对象
            if (this.orderDesignControl1.Changed == true
    && issue != null)
            {
                // 将order控件中的信息修改兑现到IssueBindingItem对象中
                nRet = this.bindingControl1.GetFromOrderControl(
                    this.orderDesignControl1,
                    issue,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                if (nRet == 2)  // 期信息有进一步变化，需要设置到编辑器
                {
                    string strOrderInfoMessage = "";
                    // 根据期信息初始化采购控件
                    // return:
                    //      -1  出错
                    //      0   没有找到对应的采购信息
                    //      1   找到采购信息
                    nRet = this.bindingControl1.InitialOrderControl(
                        issue,
                        this.orderDesignControl1,
                        out strOrderInfoMessage,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                    {
                        this.orderDesignControl1.Visible = false;
                        this.orderDesignControl1.Clear();
                        issue = null;
                        return;
                    }

                }

                // this.m_issue.Changed = true;
                this.orderDesignControl1.Changed = false;
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }
#endif

        object m_item = null;

        void BackItem()
        {
            if (!(this.m_item is ItemBindingItem))
            {
                return;
            }

            ItemBindingItem item = (ItemBindingItem)this.m_item;
            string strError = "";
            int nRet = 0;

            // 收尾上次的对象，从编辑器到对象
            if (this.entityEditControl1.Changed == true
    && item != null)
            {
                string strXml = "";
                nRet = this.entityEditControl1.GetData(
                    false,  // 不检查this.Parent
                    out strXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                nRet = item.ChangeItemXml(strXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                item.Changed = true;
                // this.m_item = null;

                this.entityEditControl1.Changed = false;
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        private void bindingControl1_CellFocusChanged(object sender, FocusChangedEventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 收尾上次的对象，从编辑器到对象
            BackItem();

#if ORDERDESIGN_CONTROL
            BackIssue();
#endif


            // 从册对象到编辑器
            if (e.NewFocusObject is Cell)
            {
                Cell cell = null;

                cell = (Cell)e.NewFocusObject;
                if (/*cell.item != this.m_item
                    &&*/ cell.item != null
                    )
                {
                    nRet = this.entityEditControl1.SetData(cell.item.Xml,
                        cell.item.RecPath,
                        null,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (cell.item.Calculated == true
                        || cell.item.Deleted == true
                        || cell.item.Locked == true
                        || (cell.item.ParentItem != null && cell.item.ParentItem.Locked == true))
                        this.entityEditControl1.SetReadOnly("all");
                    else
                        this.entityEditControl1.SetReadOnly("binding");
                    this.entityEditControl1.Changed = false;
                    this.m_item = cell.item;
                    this.entityEditControl1.ContentControl.Invalidate(); // 背景有可能改变
                    this.entityEditControl1.Visible = true;
                    this.orderDesignControl1.Visible = false;
                    return;
                }

            }

#if ORDERDESIGN_CONTROL

            // 从期对象到编辑器
            if (e.NewFocusObject is IssueBindingItem)
            {
                IssueBindingItem issue = null;

                issue = (IssueBindingItem)e.NewFocusObject;
                if (issue != this.m_item
                    && String.IsNullOrEmpty(issue.PublishTime) == false
                    && issue.Virtual == false)
                {
                    string strOrderInfoMessage = "";

                    // 根据期信息初始化采购控件
                    // return:
                    //      -1  出错
                    //      0   没有找到对应的采购信息
                    //      1   找到采购信息
                    nRet = this.bindingControl1.InitialOrderControl(
                        issue,
                        this.orderDesignControl1,
                        out strOrderInfoMessage,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

                    if (nRet == 0)
                        goto END1;
                    
                    this.m_item = issue;
                    this.orderDesignControl1.Visible = true;
                    this.entityEditControl1.Visible = false;
                    return;
                }

            }
#endif

            // END1:
            this.orderDesignControl1.Visible = false;
            this.orderDesignControl1.Clear();
            this.m_item = null;

            this.entityEditControl1.Visible = false;
            this.entityEditControl1.Clear();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        bool m_bEditAreaVisible = true;

        /// <summary>
        /// 编辑区域是否可见
        /// </summary>
        public bool EditAreaVisible
        {
            get
            {
                return this.checkBox_displayEditArea.Checked;
            }
            set
            {
                this.checkBox_displayEditArea.Checked = value;
            }
        }

        List<Control> _freeControls = new List<Control>();

        void DisposeFreeControls()
        {
            ControlExtention.DisposeFreeControls(_freeControls);
        }

        void VisibleEditArea(bool bVisible)
        {
            this.checkBox_displayEditArea.Checked = bVisible;

            if (m_bEditAreaVisible == bVisible)
                return;
            if (bVisible == false)
            {
                // 隐藏编辑区域。相当于把装订控件直接放到顶层

                // 从集合中移出装订控件
                this.splitContainer_main.Panel2.Controls.Remove(this.bindingControl1);

                // 修改装订控件的位置和尺寸
                this.bindingControl1.Dock = DockStyle.None;
                this.bindingControl1.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
                this.bindingControl1.Location = this.splitContainer_main.Location;
                this.bindingControl1.Size = this.splitContainer_main.Size;
                this.Controls.Add(this.bindingControl1);

                this.Controls.Remove(this.splitContainer_main);
                ControlExtention.AddFreeControl(_freeControls, this.splitContainer_main);
            }
            else
            {
                // 显示编辑区域。相当于把分割控件直接放到顶层
                this.splitContainer_main.Dock = DockStyle.None;
                this.splitContainer_main.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Top | AnchorStyles.Bottom;
                this.splitContainer_main.Location = this.bindingControl1.Location;
                this.splitContainer_main.Size = this.bindingControl1.Size;

                this.Controls.Remove(this.bindingControl1);
                this.bindingControl1.Dock = DockStyle.Fill;
                this.splitContainer_main.Panel1.Controls.Add(this.bindingControl1);

                this.Controls.Add(this.splitContainer_main);
                ControlExtention.RemoveFreeControl(_freeControls, this.splitContainer_main);
            }

            this.m_bEditAreaVisible = bVisible;
        }

        private void bindingControl1_EditArea(object sender, EditAreaEventArgs e)
        {
            if (e.Action == "get_state")
            {
                if (this.m_bEditAreaVisible == true)
                    e.Result = "visible";
                else
                    e.Result = "hide";
                return;
            }

            if (e.Action == "open")
                this.VisibleEditArea(true);
            else if (e.Action == "close")
                this.VisibleEditArea(false);
            else if (e.Action == "focus")
                this.entityEditControl1.Focus();
        }

        private void entityEditControl1_Leave(object sender, EventArgs e)
        {
            BackItem();
        }

        private void orderDesignControl1_Leave(object sender, EventArgs e)
        {
#if ORDERDESIGN_CONTROL
            BackIssue();
#endif
        }

        private void checkBox_displayEditArea_CheckedChanged(object sender, EventArgs e)
        {
            this.VisibleEditArea(this.checkBox_displayEditArea.Checked);

            // 令LoadState()工作正常
            if (this.AppInfo != null)
            {
                this.AppInfo.SetBoolean("bindingform",
        "edit_area_visible",
        this.m_bEditAreaVisible);
            }

        }

        // 选项
        private void button_option1_Click(object sender, EventArgs e)
        {
            // 同步存储值
            this.MainForm.AppInfo.SetBoolean(
    "binding_form",
    "display_orderinfoxy",
    this.bindingControl1.DisplayOrderInfoXY);

            this.MainForm.AppInfo.SetBoolean(
"binding_form",
"display_lockedOrderGroup",
!this.bindingControl1.HideLockedOrderGroup);

            BindingOptionDialog dlg = new BindingOptionDialog();
            MainForm.SetControlFont(dlg, this.Font, false);

            dlg.DefaultTextLineNames = this.bindingControl1.DefaultTextLineNames;
            dlg.DefaultGroupTextLineNames = this.bindingControl1.DefaultGroupTextLineNames;
            dlg.AppInfo = this.AppInfo;
            dlg.StartPosition = FormStartPosition.CenterScreen;

            dlg.ShowDialog(this);
            if (dlg.DialogResult == DialogResult.OK)
            {
                this.LoadState();

                // TODO: 显示和隐藏状态的改变，需要引起一次重新初始化才行
                // 主要是找出来那些需要显示的合订册对象。需要综合判断其成员所属的订购组
            }
        }

        private void entityEditControl1_VisibleChanged(object sender, EventArgs e)
        {
            if (this.entityEditControl1.Visible == true)
            {
                this.tableLayoutPanel_editArea.RowStyles[1].SizeType = SizeType.Percent;
                this.tableLayoutPanel_editArea.RowStyles[1].Height = 100;

                this.tableLayoutPanel_editArea.RowStyles[2].SizeType = SizeType.Percent;
                this.tableLayoutPanel_editArea.RowStyles[2].Height = 0;
            }
        }

        private void orderDesignControl1_VisibleChanged(object sender, EventArgs e)
        {
            if (this.orderDesignControl1.Visible == true)
            {
                this.tableLayoutPanel_editArea.RowStyles[2].SizeType = SizeType.Percent;
                this.tableLayoutPanel_editArea.RowStyles[2].Height = 100;

                this.tableLayoutPanel_editArea.RowStyles[1].SizeType = SizeType.Percent;
                this.tableLayoutPanel_editArea.RowStyles[1].Height = 0;
            }
        }

        private void toolStripButton_closeTextArea_Click(object sender, EventArgs e)
        {
            this.VisibleEditArea(false);
        }

        private void entityEditControl1_PaintContent(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            if (!(this.m_item is ItemBindingItem))
                return;

            ItemBindingItem item = (ItemBindingItem)this.m_item;
            Cell cell = item.ContainerCell;
            if (cell == null)
                return;

            PaintInfo info = cell.GetPaintInfo();
            this.entityEditControl1.MemberBackColor = info.BackColor;
            this.entityEditControl1.MemberForeColor = info.ForeColor;

            int nDelta = 8;
            RectangleF rect = this.entityEditControl1.ContentControl.DisplayRectangle;
            rect.Inflate(nDelta, nDelta);

            cell.PaintBorder((long)rect.X,
            (long)rect.Y,
            (int)rect.Width,
            (int)rect.Height,
            e);

        }

        private void entityEditControl1_ControlKeyDown(object sender, ControlKeyEventArgs e)
        {
            if (e.e.KeyCode == Keys.A && e.e.Control == true)
            {
                if (this.GenerateData != null)
                {
                    GenerateDataEventArgs e1 = new GenerateDataEventArgs();
                    e1.FocusedControl = sender; // sender为 EntityEditControl
                    this.GenerateData(this, e1);
                }
                e.e.SuppressKeyPress = true;    // 2015/5/28
                return;
            }
        }

    }
}