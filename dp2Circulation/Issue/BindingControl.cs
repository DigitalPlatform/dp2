using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Xml;

using System.Runtime.InteropServices;
using System.Net;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Drawing;
using DigitalPlatform.DataMining;
using DigitalPlatform.Marc;

namespace dp2Circulation
{
    /// <summary>
    /// 期刊图形界面控件
    /// </summary>
    internal partial class BindingControl : Control
    {
        // 2017/12/15
        /// <summary>
        /// 获得宏的值
        /// </summary>
        public event GetMacroValueHandler GetMacroValue = null;

        // 2016/10/8
        public event GetBiblioEventHandler GetBiblio = null;

        // 封面图像 图像管理器
        // public ImageManager ImageManager { get; set; }

        public string Operator = "";    // 当前操作者帐户名
        public string LibraryCodeList = "";     // 当前用户管辖的馆代码列表

        public bool HideLockedOrderGroup
        {
            // 是否隐藏完全在管辖范围以外的订购组。新增期的时候自动不会包括不相干的订购组，这个行为不受此变量的控制
            get
            {
                return this.m_bHideLockedOrderGroup;
            }
            set
            {
                this.m_bHideLockedOrderGroup = value;
                this.m_bHideLockedBindingCell = value;
            }
        }

        internal bool m_bHideLockedOrderGroup = false;    // 是否隐藏完全在管辖范围以外的订购组。新增期的时候自动不会包括不相干的订购组，这个行为不受此变量的控制
        internal bool m_bHideLockedBindingCell = false;    // 是否隐藏在管辖范围以外的合订册

        public string WholeLayout = "auto"; // auto/acception/binding

        internal bool m_bChanged = false;    // 主要记载是否删除过对象

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        public bool Changed
        {
            get
            {
                if (this.m_bChanged == true)
                    return true;

                for (int i = 0; i < this.Issues.Count; i++)
                {
                    IssueBindingItem issue = this.Issues[i];
                    if (issue.Virtual == true)
                        continue;
                    if (issue.Changed == true)
                        return true;
                    for (int j = 0; j < issue.Cells.Count; j++)
                    {
                        Cell cell = issue.Cells[j];
                        if (cell != null && cell.item != null)
                        {
                            if (cell.item.Deleted == true)
                                continue;
                            if (cell.item.Changed == true
                                || cell.item.NewCreated == true)
                                return true;
                        }
                    }
                }

                return false;
            }
            set
            {
                this.m_bChanged = value;

                if (value == false)
                {
                    for (int i = 0; i < this.Issues.Count; i++)
                    {
                        IssueBindingItem issue = this.Issues[i];
                        if (issue.Virtual == true)
                            continue;
                        issue.Changed = value;
                        for (int j = 0; j < issue.Cells.Count; j++)
                        {
                            Cell cell = issue.Cells[j];
                            if (cell != null && cell.item != null)
                            {
                                cell.item.Changed = value;
                            }
                        }
                    }
                }
            }
        }

        public string[] DefaultTextLineNames = new string[] {
            "location", "馆藏地点",
            "intact", "完好率",
            "state", "册状态",
            "refID", "参考ID",
            "barcode", "册条码号",
        };

        /*
        public string[] TextLineNames = new string[] {
            "location", "馆藏地点",
            "intact", "完好率",
            "state", "册状态",
            "refID", "参考ID",
            "barcode", "册条码号",
        };
         * */
        public string[] TextLineNames = null;

        public string[] DefaultGroupTextLineNames = new string[] {
            "seller", "订购渠道",
            "source", "经费来源",
            "price", "单册价格",
            "range", "时间范围",
            "batchNo", "批次号",
        };

        public string[] GroupTextLineNames = null;

        public IApplicationInfo AppInfo = null;

        /// <summary>
        /// 是否为新创建的册记录设置“加工中”状态
        /// </summary>
        public bool SetProcessingState = true;

        bool m_bBindingBatchNoInputed = false; // 是否输入过了装订批次号
        bool m_bAcceptingBatchNoInputed = false;    // 是否输入过了验收批次号

        /// <summary>
        /// 编辑区交互
        /// </summary>
        public event EditAreaEventHandler EditArea = null;

        /// <summary>
        /// 焦点发生改变
        /// </summary>
        public event FocusChangedEventHandler CellFocusChanged = null;

        Cell m_lastHoverObj = null;

        CellBase m_lastFocusObj = null;


        CellBase DragStartObject
        {
            get
            {
                return this.m_DragStartObject;
            }
            set
            {
                this.m_DragStartObject = value;

                /*
                if (value != null)
                    this.FocusObject = value;   // new changed
                 * */

                if (value != null)
                    SetObjectFocus(m_DragStartObject);
            }
        }

        // 拖动中途最近经过的对象
        CellBase DragLastEndObject
        {
            get
            {
                return this.m_DragLastEndObject;
            }
            set
            {
                this.m_DragLastEndObject = value;


                /*
                 * // 如果要围选的过程中也有focus跟随移动的话
                if (m_bRectSelecting == true)
                DrawSelectRect(true);


                    this.FocusObject = value;   // new changed
                    this.Update();

                if (m_bRectSelecting == true)
                    DrawSelectRect(true);
                 * */

                // 围选过程中没有focus跟随移动
                if (value != null)
                    SetObjectFocus(m_DragLastEndObject);
            }
        }

        public bool CheckProcessingState(ItemBindingItem item)
        {
            // 不必检查
            if (this.SetProcessingState == false)
                return true;

            if (item.IsProcessingState() == true)
                return true;

            return false;
        }

        // 拖动开始时的对象
        CellBase m_DragStartObject = null;

        // 拖动中途最近经过的对象
        CellBase m_DragLastEndObject = null;
        // 拖动开始时的鼠标位置，view坐标
        Point DragStartMousePosition = new Point(0, 0);
        //
        // 鼠标在拖动开始时的位置 整体文档坐标
        PointF m_DragStartPointOnDoc = new PointF(0, 0);

        // 鼠标在拖动中途时的位置 整体文档坐标
        PointF m_DragCurrentPointOnDoc = new PointF(0, 0);

        bool m_bRectSelectMode = true;
        bool m_bRectSelecting = false;  // 正在矩形选择中途

        bool m_bDraging = false;    // 正在拖拽中途 2010/2/12

        ToolTip trackTip;

        // 帮助快速清除显示的选择对象相关数组(不是太精确)
        List<CellBase> m_aSelectedArea = new List<CellBase>();
        bool m_bSelectedAreaOverflowed = true;  // true可以用来测试没有这个机制时的情况

        // 焦点单元
        public Cell FocusedCell = null;

        // 获得册信息
        public event GetItemInfoEventHandler GetItemInfo = null;

        // 获得订购信息
        public event GetOrderInfoEventHandler GetOrderInfo = null;


        // 期 数组
        public List<IssueBindingItem> Issues = new List<IssueBindingItem>();

        public IssueBindingItem FreeIssue = null;   // 无主的，自由的期

        // 合订的册 数组
        public List<ItemBindingItem> ParentItems = new List<ItemBindingItem>();

        /*
        // 初始化时无期刊对象管辖的单册 数组
        // 初始化结束后，就清空
        internal List<ItemBindingItem> NoneIssueItems = new List<ItemBindingItem>();
         * */

        // 初始化时所有册对象 数组
        // 初始化结束后，就清空
        internal List<ItemBindingItem> InitialItems = new List<ItemBindingItem>();

        BorderStyle borderStyle = BorderStyle.Fixed3D;

        #region 图形相关成员

        public bool DisplayOrderInfoXY = false;

        // 普通单册的单元颜色
        //public Color BackColor = Color.White;   // 背景色
        //public Color ForeColor = Color.Black;   // 前景色，也就是文字颜色
        public Color GrayColor = Color.Gray;   // 浅色背景

        // 选定状态的单元颜色
        public Color SelectedBackColor = Color.DarkRed; // Color.FromArgb(200, 255, 100, 100);    // 背景色
        public Color SelectedForeColor = Color.Black;   // 前景色，也就是文字颜色
        public Color SelectedGrayColor = Color.FromArgb(170, 170, 255);  // 浅色背景

        // 单册的单元颜色
        public Color SingleBackColor = Color.White;    // 背景色
        public Color SingleForeColor = Color.Black;   // 前景色，也就是文字颜色
        public Color SingleGrayColor = Color.DarkGray;   // 浅色背景

        // 合订成员的单元颜色
        public Color MemberBackColor = Color.FromArgb(200, 200, 200);    // 背景色
        public Color MemberForeColor = Color.White;   // 前景色，也就是文字颜色
        public Color MemberGrayColor = Color.FromArgb(180, 180, 180);   // 浅色背景

        // 合订本的单元颜色
        public Color ParentBackColor = Color.FromArgb(150, 150, 150);    // 背景色
        public Color ParentForeColor = Color.White;   // 前景色，也就是文字颜色
        public Color ParentGrayColor = Color.FromArgb(130, 130, 130);   // 浅色背景

        // 表示新创建的侧边条颜色
        public Color NewBarColor = Color.FromArgb(255, 255, 0);

        // 表示发生过修改的侧边条颜色
        public Color ChangedBarColor = Color.FromArgb(0, 255, 0);

        // 期格子的
        public Color IssueBoxBackColor = Color.FromArgb(255, Color.Black);  // 背景颜色 // Color.FromArgb(200, Color.White);
        public Color IssueBoxForeColor = Color.FromArgb(255, Color.White);  // 前景颜色
        public Color IssueBoxGrayColor = Color.DarkGray;   // 浅色背景

        // 预测的单元颜色
        public Color CalculatedBackColor = Color.FromArgb(0, Color.White);   // 背景色
        public Color CalculatedForeColor = Color.Gray;   // 前景色，也就是文字颜色
        public Color CalculatedGrayColor = Color.Yellow;   // 浅色背景。“？”的颜色

        // 合订册外框
        public Color FixedBorderColor = Color.DarkBlue; // Color.FromArgb(100, 110, 100) 固化的合订范围外框
        public Color NewlyBorderColor = Color.DarkBlue;  // DarkGreen 可修改的合订范围外框

        public enum BoundLineStyle
        {
            Curve = 0,
            Line = 1,
        }

        // 合订册连接线的风格
        public BoundLineStyle LineStyle = BoundLineStyle.Curve;

        // 各种颜色的名字：
        // http://msdn.microsoft.com/en-us/library/system.windows.media.color(VS.95).aspx

        // 期内的最大册数
        internal int m_nMaxItemCountOfOneIssue = -1; // -1 表示尚未初始化

        int nNestedSetScrollBars = 0;

        // 卷滚条比率 小于等于1.0F
        double m_v_ratio = 1.0F;
        double m_h_ratio = 1.0F;

        int m_nLeftBlank = 20;	// 边空
        int m_nRightBlank = 20;
        int m_nTopBlank = 20;
        int m_nBottomBlank = 20;

        long m_lWindowOrgX = 0;    // 窗口原点
        long m_lWindowOrgY = 0;

        long m_lContentWidth = 0;    // 内容部分的宽度。包括左边标题，若干格子。不包括左右空白
        long m_lContentHeight = 0;   // 内容部分的高度

        internal Font m_fontLine = null;    // 格子中每行文字的字体
        internal Font m_fontTitleSmall = null;   // 左侧标题文字的字体，小的
        internal Font m_fontTitleLarge = null;   // 左侧标题文字的字体，大的

        internal int m_nLineHeight = 16;  // 文字，每行的高度 18

        internal int m_nCellHeight = 110;   // 70
        internal int m_nCellWidth = 130;
        internal int m_nLeftTextWidth = 130;

        internal int m_nCoverImageWidth = 100;  // 期格子左边的封面图像宽度

        internal Padding CellMargin = new Padding(6);
        internal Padding CellPadding = new Padding(8);

        // internal Padding LeftTextMargin = new Padding(6);
        internal Padding LeftTextMargin { get; set; }

        internal Padding LeftTextPadding = new Padding(0);

        internal Rectangle RectGrab
        {
            get
            {
                return m_rectGrab;
            }
            set
            {
                m_rectGrab = value;
            }
        }

        Rectangle m_rectGrab = new Rectangle(4, 4, 16, 16); // dr g h ndle矩形(在Cell坐标内)

        #endregion

        public BindingControl()
        {
            this.LeftTextMargin = new Padding(6);

            this.DoubleBuffered = true;

            this.TextLineNames = this.DefaultTextLineNames;
            this.GroupTextLineNames = this.DefaultGroupTextLineNames;

            InitializeComponent();

            trackTip = new ToolTip();

            int nFontHeight = this.m_nLineHeight - 4;
            this.m_fontLine = new Font("微软雅黑",    // "Arial",
                nFontHeight,
                FontStyle.Regular,
                GraphicsUnit.Pixel);

            nFontHeight = this.m_nLineHeight - 4;
            this.m_fontTitleSmall = new Font("微软雅黑",    // "Arial",
                nFontHeight,
                FontStyle.Bold,
                GraphicsUnit.Pixel);

            nFontHeight = this.m_nLineHeight + 4;
            this.m_fontTitleLarge = new Font("微软雅黑",    // "Arial",
                nFontHeight,
                FontStyle.Bold,
                GraphicsUnit.Pixel);

            Program.MainForm._imageManager.GetObjectComplete += _imageManager_GetObjectComplete;
        }

        void DisposeFonts()
        {
            if (this.m_fontLine != null)
            {
                this.m_fontLine.Dispose();
                this.m_fontLine = null;
            }

            if (this.m_fontTitleSmall != null)
            {
                this.m_fontTitleSmall.Dispose();
                this.m_fontTitleSmall = null;
            }

            if (this.m_fontTitleLarge != null)
            {
                this.m_fontTitleLarge.Dispose();
                this.m_fontTitleLarge = null;
            }

            if (this.trackTip != null)
            {
                this.trackTip.Dispose();
                this.trackTip = null;
            }

        }

        void _imageManager_GetObjectComplete(object sender, GetObjectCompleteEventArgs e)
        {
            if (e.TraceObject == null)
                return;

            IssueBindingItem issue = e.TraceObject.Tag as IssueBindingItem;
            if (issue == null)
                return;
            if (string.IsNullOrEmpty(e.ErrorInfo))
            {
                issue.CoverImageFileName = e.TraceObject.FileName;
            }
        }

        string m_strBiblioDbName = "";

        // 获取值列表时作为线索的数据库名
        public string BiblioDbName
        {
            get
            {
                return this.m_strBiblioDbName;
            }
            set
            {
                this.m_strBiblioDbName = value;
            }
        }

        public void Clear()
        {
            this.Issues.Clear();
        }

        public const int SPLITTER_WIDTH = 4;

        // 2016/10/24
        void ChangeCoverImageColumnWidth(int delta)
        {
            this.m_nCoverImageWidth += delta;
            if (this.m_nCoverImageWidth < SPLITTER_WIDTH)
                this.m_nCoverImageWidth = SPLITTER_WIDTH;
            AfterWidthChanged(true);
        }

        public void StartGetCoverImage(IssueBindingItem issue,
            string strObjectPath)
        {
            Program.MainForm._imageManager.AsyncGetObjectFile(Program.MainForm.LibraryServerUrl,
                Program.MainForm.GetCurrentUserName(),
                strObjectPath,
                null,   // e.FileName,
                issue);
        }

        // 所有隐藏的册事项
        internal List<ItemBindingItem> m_hideitems = new List<ItemBindingItem>();
        public List<ItemBindingItem> AllHideItems
        {
            get
            {
                return this.m_hideitems;
            }
        }

        // 外部接口
        // 所有显示出来的和隐藏的册事项
        // 不包括已经删除的事项
        public List<ItemBindingItem> AllItems
        {
            get
            {
                List<ItemBindingItem> results = new List<ItemBindingItem>();
                results.AddRange(this.AllVisibleItems);
                results.AddRange(this.AllHideItems);

                return results;
            }
        }

        // 外部接口
        // 所有显示出来的册事项
        // 不包括已经删除的事项
        public List<ItemBindingItem> AllVisibleItems
        {
            get
            {
                List<ItemBindingItem> results = new List<ItemBindingItem>();
                for (int i = 0; i < this.Issues.Count; i++)
                {
                    IssueBindingItem issue = this.Issues[i];
                    for (int j = 0; j < issue.Cells.Count; j++)
                    {
                        Cell cell = issue.Cells[j];
                        if (cell != null
                            && !(cell is GroupCell)
                            && cell.item != null
                            && cell.item.Deleted == false
                            && cell.item.Calculated == false)
                            results.Add(cell.item);
                    }
                }

                return results;
            }
        }

        // 2012/9/25
        public static ItemBindingItem FindItemByRefID(string strRefID,
            List<ItemBindingItem> items)
        {
            foreach (ItemBindingItem item in items)
            {
                if (item == null)
                    continue;
                if (item.RefID == strRefID)
                    return item;
            }

            return null;
        }

        // 从期、册两个层次，查找一个特定refid的册事项
        // 只能用在初始化阶段的前部
        internal ItemBindingItem InitialFindItemByRefID(string strRefID)
        {
            for (int i = 0; i < this.Issues.Count; i++)
            {
                IssueBindingItem issue = this.Issues[i];

                for (int j = 0; j < issue.Items.Count; j++)
                {
                    ItemBindingItem item = issue.Items[j];
                    if (item.RefID == strRefID)
                        return item;
                }
            }

            return null;
        }

        // 从期、册两个层次，查找一个特定refid的册事项
        // 只能用在初始化阶段的前部
        internal Cell FindCellByRefID(string strRefID,
            IssueBindingItem exclude_issue)
        {
            for (int i = 0; i < this.Issues.Count; i++)
            {
                IssueBindingItem issue = this.Issues[i];

                if (issue == exclude_issue)
                    continue;

                for (int j = 0; j < issue.Cells.Count; j++)
                {
                    Cell cell = issue.Cells[j];
                    if (cell == null || cell.item == null)
                        continue;
                    if (cell.item.RefID == strRefID)
                        return cell;
                }
            }

            return null;
        }

        // 安放单独的册(即非合订成员册)在行最后一个空位
        // return:
        //      实际安放的单格index位置
        static int PlaceSingleToTail(ItemBindingItem item)
        {
            Debug.Assert(item.Container != null, "");

            IssueBindingItem issue = item.Container;

            // 从右边找，找到第一个可用的空位
            int nPos = issue.Cells.Count;
            for (int i = issue.Cells.Count - 1; i >= 0; i--)
            {
                if (issue.IsBlankSingleIndex(i) == false)
                    break;
            }

            Cell cell = new Cell();
            cell.item = item;
            issue.SetCell(nPos, cell);
            return nPos;
        }

        // 新版本
        // 将所有成员Cell向右移动若干个单格
        public void MoveMemberCellsToRight(ItemBindingItem parent_item,
            int nDistance)
        {
            Debug.Assert(nDistance > 0, "");

            for (int i = 0; i < nDistance; i++)
            {
                MoveMemberCellsToRight(parent_item);
            }
        }

        // 新版本
        // 将所有成员Cell向右移动一个单格
        public void MoveMemberCellsToRight(ItemBindingItem parent_item)
        {
#if DEBUG
            int nCol = -1;
#endif
            for (int i = 0; i < parent_item.MemberCells.Count; i++)
            {
                Cell cell = parent_item.MemberCells[i];

                // ItemBindingItem item = cell.item;

                IssueBindingItem issue = cell.Container;
                Debug.Assert(issue != null, "");

                if (issue.IssueLayoutState == IssueLayoutState.Accepting)
                    continue;

                int index = issue.Cells.IndexOf(cell);
                Debug.Assert(index != -1, "");

#if DEBUG
                if (nCol != -1)
                {
                    if (nCol != index)
                    {
                        Debug.Assert(false, "属于同一个合订册的各个成员格子index居然不同");
                    }
                }

                nCol = index;
#endif

                // 注意，index实际上为双格的右侧格子

                issue.GetBlankSingleIndex(index + 1);   // +1为移动后的右侧格子。确保这里有个单格即可，而不能去确保其左方的双格，因为那样涉及到已被本合订范围占据的位置

                // 复制一个双格到别处
                // 本功能比较原始，不负责挤压空位
                // parameters:
                //      nSourceIndex    源index位置。注意，必须是双格的左侧
                //      nTargetIndex    目标index位置。注意，必须是双格的左侧
                issue.CopyDoubleIndexTo(
                    index - 1,
                    index,
                    true);
                /*
                {
                    // 搬动右侧格子内容
                    Cell right_cell = issue.GetCell(index);
                    issue.SetCell(index + 2, right_cell);
                    issue.SetCell(index, null);
                }

                {
                    // 看看左侧位置? 把合订对象也一并移动了
                    Cell temp_cell = issue.GetCell(index - 1);
                    if (cell != null)
                    {
                        issue.SetCell(index  - 1 + 2, temp_cell);
                    }
                    issue.SetCell(index - 1, null);
                }
                 * */
            }
        }

#if OLD_VERSION
        // 将所有成员Cell向右移动一个双格
        public void MoveMemberCellsToRight(ItemBindingItem parent_item)
        {
            for(int i=0;i<parent_item.MemberCells.Count;i++)
            {
                Cell cell = parent_item.MemberCells[i];

                // ItemBindingItem item = cell.item;

                IssueBindingItem issue = cell.Container;
                Debug.Assert(issue != null, "");
                int index = issue.Cells.IndexOf(cell);
                Debug.Assert(index != -1, "");

                issue.GetBlankPosition((index/2)+1, parent_item);

                {
                    Cell temp_cell = issue.GetCell(index);
                    issue.SetCell(index + 2, temp_cell);
                    issue.SetCell(index, null);
                }

                {
                    // 看看合订位置? 把合订对象也一并移动了
                    Cell temp_cell = issue.GetCell(index - 1);
                    if (cell != null)
                    {
                        issue.SetCell(index + 2 - 1, temp_cell);
                        issue.SetCell(index - 1, null);
                    }
                }
            }
        }
#endif

        //新版本
        // 观察一个合订册的所有期，看是否可能向左移一个单格。
        // 也就是看看左边是否都是空白位置
        public bool CanMoveToLeft(Cell parent_cell)
        {
            Debug.Assert(parent_cell != null, "");
            Debug.Assert(parent_cell.Container != null, "");
            Debug.Assert(parent_cell.item != null, "");

            int nCol = parent_cell.Container.IndexOfCell(parent_cell);
            Debug.Assert(nCol != -1, "");

            if (nCol <= 0)
                return false;

            IssueBindingItem parent_issue = parent_cell.Container;
            Debug.Assert(parent_issue != null, "");

            if (parent_cell.item.MemberCells.Count == 0)
            {
                if (parent_issue.IssueLayoutState == IssueLayoutState.Binding)
                {
                    if (parent_issue.IsBlankDoubleIndex(nCol - 1, parent_cell.item) == false)
                        return false;
                    return true;
                }
                else
                {
                    Debug.Assert(parent_issue.IssueLayoutState == IssueLayoutState.Accepting, "");
                    return false;
                }
            }
            else
            {
                IssueBindingItem first_issue = null;
                IssueBindingItem last_issue = null;

                first_issue = parent_cell.item.MemberCells[0].Container;
                Debug.Assert(first_issue != null, "");
                last_issue = parent_cell.item.MemberCells[parent_cell.item.MemberCells.Count - 1].Container;
                Debug.Assert(last_issue != null, "");

                int nFirstLineNo = this.Issues.IndexOf(first_issue);
                Debug.Assert(nFirstLineNo != -1, "");

                // 如果偶然出现第一个成员不在合订册同期的情况，矫正最小行号
                int nParentLineNo = this.Issues.IndexOf(parent_issue);
                if (nParentLineNo < nFirstLineNo)
                    nFirstLineNo = nParentLineNo;

                int nLastLineNo = this.Issues.IndexOf(last_issue);
                Debug.Assert(nLastLineNo != -1, "");

                bool bAllInAcceptingLayout = true;
                for (int i = nFirstLineNo; i <= nLastLineNo; i++)
                {
                    IssueBindingItem issue = this.Issues[i];
                    Debug.Assert(issue != null, "");

                    if (issue.IssueLayoutState == IssueLayoutState.Binding)
                    {
                        if (issue.IsBlankDoubleIndex(nCol - 1, parent_cell.item) == false)
                            return false;
                        bAllInAcceptingLayout = false;
                    }
                }
                if (bAllInAcceptingLayout == false)
                    return true;

                return false;
            }
        }


#if NOOOOOOOOOOOOOOOOO

        // 观察一个合订册的所有期，看是否可能向左移一个双格。
        // 也就是看看左边是否都是空白位置
        public bool CanMoveToLeft(Cell parent_cell)
        {
            Debug.Assert(parent_cell != null, "");
            Debug.Assert(parent_cell.Container != null, "");
            Debug.Assert(parent_cell.item != null, "");

            int nCol = parent_cell.Container.IndexOfCell(parent_cell);
            Debug.Assert(nCol != -1, "");

            if (nCol <= 0)
                return false;

            IssueBindingItem parent_issue = parent_cell.Container;
            Debug.Assert(parent_issue != null, "");

            if (parent_cell.item.MemberCells.Count == 0)
            {
                if (parent_issue.IsBlankPosition((nCol / 2) - 1, null) == false)
                    return false;
            }
            else
            {
                IssueBindingItem first_issue = null;
                IssueBindingItem last_issue = null;

                first_issue = parent_cell.item.MemberCells[0].Container;
                Debug.Assert(first_issue != null, "");
                last_issue = parent_cell.item.MemberCells[parent_cell.item.MemberCells.Count-1].Container;
                Debug.Assert(last_issue != null, "");

                int nFirstLineNo = this.Issues.IndexOf(first_issue);
                Debug.Assert(nFirstLineNo != -1, "");

                // 如果偶然出现第一个成员不在合订册同期的情况，矫正最小行号
                int nParentLineNo = this.Issues.IndexOf(parent_issue);
                if (nParentLineNo < nFirstLineNo)
                    nFirstLineNo = nParentLineNo;

                int nLastLineNo = this.Issues.IndexOf(last_issue);
                Debug.Assert(nLastLineNo != -1, "");

                for (int i = nFirstLineNo; i <= nLastLineNo; i++)
                {
                    IssueBindingItem issue = this.Issues[i];
                    Debug.Assert(issue != null, "");

                    if (issue.IsBlankPosition((nCol / 2)-1, null) == false)
                        return false;
                }
            }

            return true;
        }

#endif

        // 新版本
        // 将合订本和所有成员Cell向左移动一个单格
        // 注意：调用本函数前，要用CanMoveToLeft()检查是否允许左移。否则会引起冲突
        public bool MoveCellsToLeft(Cell parent_cell)
        {
            Debug.Assert(parent_cell != null, "");
            Debug.Assert(parent_cell.Container != null, "");
            Debug.Assert(parent_cell.item != null, "");

            int nCol = parent_cell.Container.IndexOfCell(parent_cell);
            Debug.Assert(nCol != -1, "");

            if (nCol <= 0)
                return false;

            IssueBindingItem parent_issue = parent_cell.Container;
            Debug.Assert(parent_issue != null, "");

            // int nRet = 0;
            // string strError = "";
            bool bChanged = false;

            if (parent_cell.item.MemberCells.Count == 0)
            {
                if (parent_issue.IssueLayoutState == IssueLayoutState.Binding)
                {

                    parent_issue.CopyDoubleIndexTo(
                        nCol,
                        nCol - 1,
                        true);

                    // 将腾出来的一个空位继续删除
                    // 可能会引起递归
                    parent_issue.RemoveSingleIndex(nCol);

                    bChanged = true;
                }

                return bChanged;
            }
            else
            {
                IssueBindingItem first_issue = null;
                IssueBindingItem last_issue = null;

                first_issue = parent_cell.item.MemberCells[0].Container;
                Debug.Assert(first_issue != null, "");
                last_issue = parent_cell.item.MemberCells[parent_cell.item.MemberCells.Count - 1].Container;
                Debug.Assert(last_issue != null, "");

                int nFirstLineNo = this.Issues.IndexOf(first_issue);
                Debug.Assert(nFirstLineNo != -1, "");

                // 如果偶然出现第一个成员不在合订册同期的情况，矫正最小行号
                int nParentLineNo = this.Issues.IndexOf(parent_issue);
                if (nParentLineNo < nFirstLineNo)
                    nFirstLineNo = nParentLineNo;

                int nLastLineNo = this.Issues.IndexOf(last_issue);
                Debug.Assert(nLastLineNo != -1, "");

                for (int i = nFirstLineNo; i <= nLastLineNo; i++)
                {
                    IssueBindingItem issue = this.Issues[i];
                    Debug.Assert(issue != null, "");

                    if (issue.IssueLayoutState == IssueLayoutState.Accepting)
                        continue;
                    issue.CopyDoubleIndexTo(
                         nCol,
                         nCol - 1,
                         true);
#if DEBUG
                    {
                        Cell cellTemp = issue.GetCell(nCol + 1);
                        Debug.Assert(cellTemp == null, "");
                    }
#endif

                    bChanged = true;
                }

                // 将腾出来的一个纵列的空位继续删除
                // 可能会引起递归
                for (int i = nFirstLineNo; i <= nLastLineNo; i++)
                {
                    IssueBindingItem issue = this.Issues[i];
                    Debug.Assert(issue != null, "");

                    if (issue.IssueLayoutState == IssueLayoutState.Accepting)
                        continue;

                    // 由于右边可能是一竖列的合订册范围，可能因为前面行的删除而已经连带压缩了后面空位
                    Cell cellTemp = issue.GetCell(nCol + 1);
                    if (cellTemp == null)
                    {
                        // 探测是否为合订成员占据的位置
                        // return:
                        //      -1  是。并且是双格的左侧位置
                        //      0   不是
                        //      1   是。并且是双格的右侧位置
                        int nRet = issue.IsBoundIndex(nCol + 1);
                        if (nRet == -1 || nRet == 1)
                        {
                            Debug.Assert(nRet != 1, "");
                        }
                        else
                            issue.RemoveSingleIndex(nCol + 1);
                    }

                    bChanged = true;
                }
            }

            return bChanged;
        }

#if NOOOOOOOOOOOOOOOOOOOO
        // 将合订本和所有成员Cell向左移动一个双格
        public bool MoveCellsToLeft(Cell parent_cell)
        {
            Debug.Assert(parent_cell != null, "");
            Debug.Assert(parent_cell.Container != null, "");
            Debug.Assert(parent_cell.item != null, "");

            int nCol = parent_cell.Container.IndexOfCell(parent_cell);
            Debug.Assert(nCol != -1, "");

            if (nCol <= 0)
                return false;

            IssueBindingItem parent_issue = parent_cell.Container;
            Debug.Assert(parent_issue != null, "");

            // int nRet = 0;
            // string strError = "";
            bool bChanged = false;

            if (parent_cell.item.MemberCells.Count == 0)
            {
                parent_issue.CopyPositionTo(
                    nCol / 2,
                    (nCol / 2) - 1,
                    true);

                // 将腾出来的一个空位继续删除
                // 可能会引起递归
                parent_issue.RemovePosition(nCol / 2);

                bChanged = true;
            }
            else
            {
                IssueBindingItem first_issue = null;
                IssueBindingItem last_issue = null;

                first_issue = parent_cell.item.MemberCells[0].Container;
                Debug.Assert(first_issue != null, "");
                last_issue = parent_cell.item.MemberCells[parent_cell.item.MemberCells.Count - 1].Container;
                Debug.Assert(last_issue != null, "");

                int nFirstLineNo = this.Issues.IndexOf(first_issue);
                Debug.Assert(nFirstLineNo != -1, "");

                // 如果偶然出现第一个成员不在合订册同期的情况，矫正最小行号
                int nParentLineNo = this.Issues.IndexOf(parent_issue);
                if (nParentLineNo < nFirstLineNo)
                    nFirstLineNo = nParentLineNo;

                int nLastLineNo = this.Issues.IndexOf(last_issue);
                Debug.Assert(nLastLineNo != -1, "");

                for (int i = nFirstLineNo; i <= nLastLineNo; i++)
                {
                    IssueBindingItem issue = this.Issues[i];
                    Debug.Assert(issue != null, "");

                    issue.CopyPositionTo(
                        nCol / 2,
                        (nCol / 2) - 1,
                        true);

                    bChanged = true;
                }

                // 将腾出来的一个纵列的空位继续删除
                // 可能会引起递归
                for (int i = nFirstLineNo; i <= nLastLineNo; i++)
                {
                    IssueBindingItem issue = this.Issues[i];
                    Debug.Assert(issue != null, "");

                    issue.RemovePosition(nCol / 2);

                    bChanged = true;
                }
            }

            return bChanged;
        }
#endif

        /*
        // 如果要安放的位置已经存在内容，则向右移动它们(两格)
        int MoveToRight(IssueBindingItem issue,
            int nCol)
        {
            Debug.Assert((nCol % 2) == 1, "");

            if (issue.Cells.Count <= nCol)
                return 0;

            Cell cell = issue.Cells[nCol];
            if (cell != null && cell.item != null)
            {
                if (cell.item.ParentItem != null)
                {
                    MoveMemberCellsToRight(cell.item.ParentItem);
                }
            }
            else
            {
                ItemBindingItem item = issue.Cells[nCol];

                if (issue.Cells[nCol] != null)
                {
                    issue.Cells.Add(null);
                    issue.Cells.Add(null);
                    for (int j = issue.Cells.Count - 1; j >= nCol + 2; j--)
                    {
                        Cell temp = issue.Cells[j - 2];
                        issue.Cells[j] = temp;
                    }

                    issue.Cells[nCol] = null;
                }
            }

            return 1;
        }*/

        static int IndexOf(List<PublishTimeAndVolume> lists,
            string strPublishTime)
        {
            for (int i = 0; i < lists.Count; i++)
            {
                PublishTimeAndVolume item = lists[i];
                if (strPublishTime == item.PublishTime)
                    return i;
            }

            return -1;
        }

        public class PublishTimeAndVolume
        {
            public string PublishTime = "";
            public string Volume = "";
        }

        // 统计(bindingxml中)全部合订成员册所使用过的publishtime字符串
        int GetAllBindingXmlPublishTimes(
            out List<PublishTimeAndVolume> publishtimes,
            out string strError)
        {
            publishtimes = new List<PublishTimeAndVolume>();
            strError = "";

            // 遍历合订册对象数组
            for (int i = 0; i < this.ParentItems.Count; i++)
            {
                ItemBindingItem parent_item = this.ParentItems[i];

                string strBindingXml = parent_item.Binding;
                if (String.IsNullOrEmpty(strBindingXml) == true)
                    continue;

                // 根据refid, 找到它下属的那些ItemBindingItem对象
                XmlDocument dom = new XmlDocument();
                dom.LoadXml("<root />");
                try
                {
                    dom.DocumentElement.InnerXml = strBindingXml;
                }
                catch (Exception ex)
                {
                    strError = "参考ID为 '" + parent_item.RefID + "' 的册信息中，<binding>元素内嵌XML装入DOM时出错: " + ex.Message;
                    return -1;
                }

                /*
                 * bindingxml中，<item>元素未必有refID属性。
                 * 没有refID属性，表明这是一个被删除了册记录的单纯信息单元，或者是缺期情况。
                 * 缺期可能发生在装订范围的第一册或者最后一册，要引起注意
                 * */
                XmlNodeList nodes = dom.DocumentElement.SelectNodes("item");
                if (nodes.Count == 0)
                    continue;
                for (int j = 0; j < nodes.Count; j++)
                {
                    XmlNode node = nodes[j];
                    string strPublishTime = DomUtil.GetAttr(node, "publishTime");
                    if (String.IsNullOrEmpty(strPublishTime) == true)
                        continue;
                    if (strPublishTime.IndexOf("-") != -1)
                        continue;   // 是否报错?

                    if (IndexOf(publishtimes, strPublishTime) != -1)
                        continue;   // 优化，不加入那些重复的事项。TODO: 是否要尽量用第一个非空的valume string?

                    string strVolume = DomUtil.GetAttr(node, "volume");
                    PublishTimeAndVolume item = new PublishTimeAndVolume();
                    item.PublishTime = strPublishTime;
                    item.Volume = strVolume;
                    publishtimes.Add(item);
                }
            }

            return 0;
        }

        // 新版本
        // 安放合订成员册
        // parameters:
        //      items   Item数组。注意其中有的Item可能其Container为null，属于missing性质
        //      strPublishTimeString    输出出版时间范围字符串
        void PlaceMemberItems(
            Cell parent_cell,
            List<ItemBindingItem> items,
            int nCol,
            bool bSetBidingRange = true)
        {
            List<Cell> member_cells = new List<Cell>();

            for (int i = 0; i < items.Count; i++)
            {
                ItemBindingItem item = items[i];
                IssueBindingItem issue = item.Container;

                if (issue == null)
                {
                    // Debug.Assert(item.Missing == true, "");
                    Debug.Assert(String.IsNullOrEmpty(item.PublishTime) == false, "");
                    issue = this.FindIssue(item.PublishTime);
                    if (issue == null)
                    {
                        issue = this.NewIssue(item.PublishTime,
                            item.Volume);
                        // 本操作可能会引起一些合订范围的断裂。
                        // 需要修补断裂处
                        Debug.Assert(false, "不应该走到这里。因为前面已经预先创建了所有Virtual的期对象");
                    }
                    item.Container = issue;
                }


                Cell cell = new Cell();
                cell.item = item;
                cell.Container = issue;

                member_cells.Add(cell);
            }

            // 安放下属的单独册
            PlaceMemberCells(parent_cell,
                member_cells,
                nCol);

            if (bSetBidingRange == true)
            {
                // 可能会抛出异常
                SetBindingRange(parent_cell, false);
            }
        }

#if OLD_VERSION
        // 安放合订成员册
        // TODO: SetCell已经安全，代码可以简化
        // parameters:
        //      items   Item数组。注意其中有的Item可能其Container为null，属于missing性质
        //      strPublishTimeString    输出出版时间范围字符串
        void PlaceMemberItems(
            ItemBindingItem parent_item,
            List<ItemBindingItem> items,
            int nCol,
            out string strPublishTimeString)
        {
            strPublishTimeString = "";

            Debug.Assert(nCol >= 0, "");
            Debug.Assert(items.Count != 0, "");

            List<IssueBindingItem> done_issues = new List<IssueBindingItem>();
            int nFirstLineNo = 99999;
            int nLastLineNo = -1;
            for (int i = 0; i < items.Count; i++)
            {
                ItemBindingItem item = items[i];
                IssueBindingItem issue = item.Container;

                if (issue == null)
                {
                    // Debug.Assert(item.Missing == true, "");

                    issue = this.FindIssue(item.PublishTime);
                    if (issue == null)
                    {
                        issue = this.NewIssue(item.PublishTime,
                            item.Volume);
                    }
                }

                Debug.Assert(issue != null, "");

                done_issues.Add(issue);

                int nIssueLineNo = this.Issues.IndexOf(issue);
                if (nFirstLineNo > nIssueLineNo)
                    nFirstLineNo = nIssueLineNo;

                if (nLastLineNo < nIssueLineNo)
                    nLastLineNo = nIssueLineNo;

                // 检查item对象是否已经存在
                int nExistIndex = issue.IndexOfItem(item);

                // 如果本来就在那个位置了
                if (nExistIndex == nCol)
                {
                    item.ParentItem = parent_item;

                    Cell temp_cell = issue.Cells[nExistIndex];
                    Debug.Assert(temp_cell != null, "");
                    temp_cell.ParentItem = parent_item;
                    Debug.Assert(temp_cell.IsMember == true, "");

                    parent_item.MemberCells.Remove(temp_cell);  // 保险

                    parent_item.InsertMemberCell(temp_cell);
                    continue;
                }

                // 删除已经存在的Cell
                Cell exist_cell = null;
                if (nExistIndex != -1)
                {
                    Debug.Assert(false, "");
                    if ((nExistIndex % 2) == 0)
                    {
                        // 如果在奇数位置，就很奇怪了。因为这表明这是一个合订的册
                        throw new Exception("发现将要安放的下属册对象居然在奇数Cell位置已经存在");
                    }
                    exist_cell = issue.GetCell(nExistIndex);
                    issue.Cells.RemoveAt(nExistIndex);

                    issue.Cells.RemoveAt(nExistIndex-1);    // 左边一个，也删除
                }

                issue.GetBlankDoubleIndex(nCol, parent_item);

                Cell cell = null;
                if (exist_cell == null)
                {
                    cell = new Cell();
                    cell.ParentItem = parent_item;
                    if (item.Missing == true)
                    {
                        // 只是占据位置
                        cell.item = null;
                        Debug.Assert(item.Container == null, "");
                    }
                    else
                        cell.item = item;
                }
                else
                    cell = exist_cell;


                item.ParentItem = parent_item;
                issue.SetCell(nCol, cell);

                parent_item.MemberCells.Remove(cell);  // 保险
                parent_item.InsertMemberCell(cell);
            }

            Debug.Assert(nFirstLineNo != 99999, "");
            Debug.Assert(nLastLineNo != -1,"");

            // 2009/12/16 
            strPublishTimeString = this.Issues[nFirstLineNo].PublishTime
            + "-"
            + this.Issues[nLastLineNo].PublishTime;


            // 补充窟窿
            for (int i = nFirstLineNo; i <= nLastLineNo; i++)
            {
                IssueBindingItem issue = this.Issues[i];
                if (done_issues.IndexOf(issue) != -1)
                    continue;

                {
                    Cell cell = issue.GetCell(nCol);
                    // 如果是空白格子，而且无主，则直接使用
                    if (cell != null
                        && cell.item == null
                        && cell.IsMember == false)
                    {
                        cell.ParentItem = parent_item;
                        parent_item.InsertMemberCell(cell);
                        continue;
                    }
                }

                issue.GetBlankDoubleIndex(nCol, parent_item);

                {
                    Cell cell = new Cell();
                    cell.item = null;   // 只是占据位置
                    cell.ParentItem = parent_item;
                    issue.SetCell(nCol, cell);

                    // 放在合适位置
                    // parent_item.MemberCells.Add(cell);

                    parent_item.InsertMemberCell(cell);
                }
            }

            // 
        }
#endif

        // 查找特定的Issue对象
        IssueBindingItem FindIssue(string strPublishTime)
        {
            for (int i = 0; i < this.Issues.Count; i++)
            {
                IssueBindingItem issue = this.Issues[i];
                if (issue.PublishTime == strPublishTime)
                    return issue;
            }
            return null;
        }

        // 创建一个新的期对象
        // parameters:
        //      strVolume   合成的字符串，表示卷期册
        IssueBindingItem NewIssue(string strPublishTime,
            string strVolumeString)
        {
            Debug.Assert(String.IsNullOrEmpty(strPublishTime) == false, "");

            string strIssue = "";
            string strZong = "";
            string strOneVolume = "";

            // 解析当年期号、总期号、卷号的字符串
            VolumeInfo.ParseItemVolumeString(strVolumeString,
                out strIssue,
                out strZong,
                out strOneVolume);

            IssueBindingItem new_issue = new IssueBindingItem();
            new_issue.Container = this;
            {
                string strError = "";
                int nRet = new_issue.Initial("<root />",
                    "",
                    false,
                    out strError);
                Debug.Assert(nRet != -1, "");
            }
            new_issue.PublishTime = strPublishTime;
            new_issue.Issue = strIssue;
            new_issue.Volume = strOneVolume;
            new_issue.Zong = strZong;

            int nFreeIndex = -1;
            int nInsertIndex = -1;
            string strLastPublishTime = "";
            for (int i = 0; i < this.Issues.Count; i++)
            {
                IssueBindingItem issue = this.Issues[i];

                // 跳过自由期
                if (String.IsNullOrEmpty(issue.PublishTime) == true)
                {
                    nFreeIndex = i;
                    continue;
                }

                if (String.Compare(strPublishTime, strLastPublishTime) >= 0
                    && String.Compare(strPublishTime, issue.PublishTime) < 0)
                    nInsertIndex = i;

                strLastPublishTime = issue.PublishTime;
            }

            if (nInsertIndex == -1)
            {
                if (nFreeIndex == -1)
                    this.Issues.Add(new_issue);
                else
                    this.Issues.Insert(nFreeIndex, new_issue);    // 2010/3/30
            }
            else
                this.Issues.Insert(nInsertIndex, new_issue);

            return new_issue;
        }

#if OLD_INITIAL
        // *** 为初始化服务
        // 获得所有Issue的Items中的refid字符串。注意，不是Issue的MemberCells中的
        // 注意，Issue的Items是为了初始化用途的，在AppendIssue()调用后具备。初始化完成后，即被清除
        public List<string> AllIssueMembersRefIds
        {
            get
            {
                List<string> results = new List<string>();
                for (int i = 0; i < this.Issues.Count; i++)
                {
                    IssueBindingItem issue = this.Issues[i];
                    if (issue == null)
                    {
                        Debug.Assert(false, "");
                        continue;
                    }
                    for (int j = 0; j < issue.Items.Count; j++)
                    {
                        ItemBindingItem item = issue.Items[j];
                        if (item == null)
                        {
                            Debug.Assert(item != null, "");
                            continue;
                        }
                        string strRefID = item.RefID;
                        if (String.IsNullOrEmpty(strRefID) == true)
                            continue;

#if DEBUG
                        if (results.IndexOf(strRefID) != -1)
                        {
                            Debug.Assert(false, "发现有重复的refid值 '"+strRefID+"'");
                        }
#endif
                        results.Add(strRefID);
                    }
                }
                return results;
            }
        }
#endif

        int CreateParentItems(out string strError)
        {
            strError = "";
            for (int i = 0; i < this.InitialItems.Count; i++)
            {
                ItemBindingItem item = this.InitialItems[i];
                string strPublishTime = item.PublishTime;
                if (strPublishTime.IndexOf("-") != -1)
                {
                    item.Container = null;  // 暂时不属于某个期
                    this.ParentItems.Add(item);
                    item.IsParent = true;

                    this.InitialItems.RemoveAt(i);
                    i--;
                }

                // 2010/3/30
                // 特殊情况下，没有出版时间范围，但是volumstring表明为多册的
                if (String.IsNullOrEmpty(strPublishTime) == true
                    && string.IsNullOrEmpty(item.Volume) == false)
                {
                    List<VolumeInfo> infos = null;
                    int nRet = VolumeInfo.BuildVolumeInfos(item.Volume,
                        out infos,
                        out strError);
                    if (nRet != -1)
                    {
                        if (infos.Count > 1)
                        {
                            item.Container = null;  // 暂时不属于某个期
                            this.ParentItems.Add(item);
                            item.IsParent = true;

                            this.InitialItems.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }
            return 0;
        }

        int CreateInitialItems(List<string> ItemXmls,
            out string strError)
        {
            strError = "";

            for (int i = 0; i < ItemXmls.Count; i++)
            {
                string strXml = ItemXmls[i];
                ItemBindingItem item = new ItemBindingItem();
                int nRet = item.Initial(strXml, out strError);
                if (nRet == -1)
                {
                    strError = "CreateInitialItems() error, xmlrecord index = " + i.ToString() + " : " + strError;
                    return -1;
                }

                item.Container = null;  // 暂时不属于任何期
                this.InitialItems.Add(item);
            }

            return 0;
        }

        int CreateIssues(List<string> IssueXmls,
            List<string> IssueObjectXmls,
            out string strError)
        {
            strError = "";

            for (int i = 0; i < IssueXmls.Count; i++)
            {
                string strXml = IssueXmls[i];
                string strObjectXml = "";
                if (IssueObjectXmls != null && i < IssueObjectXmls.Count)
                    strObjectXml = IssueObjectXmls[i];

                IssueBindingItem issue = new IssueBindingItem();

                issue.Container = this;
                int nRet = issue.Initial(strXml,
                    strObjectXml,
                    false,
                    out strError);
                if (nRet == -1)
                {
                    strError = "CreateIssues() error, xmlrecord index = " + i.ToString() + " : " + strError;
                    return -1;
                }

                this.Issues.Add(issue);

                if (string.IsNullOrEmpty(issue.PublishTime) == false)
                {
                    nRet = issue.InitialLoadItems(issue.PublishTime,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "CreateIssues() InitialLoadItems() error: " + strError;
                        return -1;
                    }
                }
                else
                {
                    strError = "数据错误。期记录 (参考ID 为 '" + issue.RefID + "') 中出版日期字段不应为空";
                    return -1;
                }
            }

            // 按照publishtime排序
            this.Issues.Sort(new IssuePublishTimeComparer());

            // 检查是否有重复的出版日期
            // 2010/3/21 
            string strPrevPublishTime = "";
            for (int i = 0; i < this.Issues.Count; i++)
            {
                IssueBindingItem issue = this.Issues[i];
                if (String.IsNullOrEmpty(issue.PublishTime) == true)
                    continue;
                if (issue.PublishTime == strPrevPublishTime)
                {
                    strError = "出现了重复出版日期 '" + issue.PublishTime + "' 的多个期记录";
                    return -1;
                }

                strPrevPublishTime = issue.PublishTime;
            }

            if (this.Issues.Count > 0)
            {
                // 把自由期放在最后
                if (String.IsNullOrEmpty(this.Issues[0].PublishTime) == true)
                {
                    IssueBindingItem free_issue = this.Issues[0];
                    this.Issues.RemoveAt(0);
                    this.Issues.Add(free_issue);
                }
            }

            this.m_lContentHeight = this.m_nCellHeight * this.Issues.Count;
            return 0;
        }

#if ODERDESIGN_CONTROL
        // 将订购记录装载到OrderDesignControl中
        // return:
        //      -1  error
        //      >=0 订购的总份数
        static int LoadOrderDesignItems(List<string> XmlRecords,
            OrderDesignControl control,
            out string strError)
        {
            strError = "";

            control.DisableUpdate();

            try
            {

                control.Clear();

                int nOrderedCount = 0;  // 顺便计算出订购的总份数
                for (int i = 0; i < XmlRecords.Count; i++)
                {
                    DigitalPlatform.CommonControl.Item item =
                        control.AppendNewItem(XmlRecords[i],
                        out strError);
                    if (item == null)
                        return -1;

                    nOrderedCount += item.OldCopyValue;
                }

                control.Changed = false;
                return nOrderedCount;

            }
            finally
            {
                control.EnableUpdate();
            }
        }

        // 根据右边的OrderDesignControl内容构造XML记录
        static int BuildOrderXmlRecords(
            OrderDesignControl control,
            out List<string> XmlRecords,
            out string strError)
        {
            strError = "";
            XmlRecords = new List<string>();

            for (int i = 0; i < control.Items.Count; i++)
            {
                DigitalPlatform.CommonControl.Item design_item = control.Items[i];

                string strXml = "";
                int nRet = design_item.BuildXml(out strXml, out strError);
                if (nRet == -1)
                    return -1;

                XmlDocument dom = new XmlDocument();
                dom.LoadXml(strXml);
                XmlRecords.Add(dom.DocumentElement.OuterXml);   // 不要包含prolog
            }

            return 0;
        }

        // 将order控件中的信息修改兑现到IssueBindingItem对象中
        // return:
        //      -1  error
        //      0   不必要兑现
        //      1   兑现
        //      2   不但已经兑现，而且期信息发生了进一步修改(例如创建了册，需要重新反映到采购控件中)
        public int GetFromOrderControl(
            OrderDesignControl order_control,
            IssueBindingItem issue,
            out string strError)
        {
            strError = "";

            if (order_control.Changed == false)
            {
                return 0;
            }

            if (issue == null)
            {
                Debug.Assert(false, "");
                return 0;
            }


            // 将即将离开焦点的修改过的右边事项保存

            // 删除orderInfo元素下的全部元素
            XmlNodeList nodes = issue.dom.DocumentElement.SelectNodes("orderInfo/*");
            for (int i = 0; i < nodes.Count; i++)
            {
                XmlNode node = nodes[i];
                node.ParentNode.RemoveChild(node);
            }

            List<string> XmlRecords = null;
            // 根据右边的OrderDesignControl内容构造XML记录
            int nRet = BuildOrderXmlRecords(
                order_control,
                out XmlRecords,
                out strError);
            if (nRet == -1)
                return -1;

            XmlNode root = issue.dom.DocumentElement.SelectSingleNode("orderInfo");
            if (root == null)
            {
                root = issue.dom.CreateElement("orderInfo");
                issue.dom.DocumentElement.AppendChild(root);
            }
            for (int i = 0; i < XmlRecords.Count; i++)
            {
                XmlDocumentFragment fragment = issue.dom.CreateDocumentFragment();
                try
                {
                    fragment.InnerXml = XmlRecords[i];
                }
                catch (Exception ex)
                {
                    strError = "fragment XML装入XmlDocumentFragment时出错: " + ex.Message;
                    return -1;
                }

                root.AppendChild(fragment);
                // this.Changed = true;
            }

            issue.Changed = true;

            bool bItemCreated = false;
            List<IssueBindingItem> issues = new List<IssueBindingItem>();
            issues.Add(issue);
            // 根据验收数据，自动创建新的册
            // return:
            //      -1  error
            //      0   没有创建册
            //      1   创建了册
            nRet = CreateNewItems(issues,
                GetAcceptingBatchNo(),
                this.SetProcessingState,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 1)
                bItemCreated = true;

            // item.SetNodeCaption(tree_node); // 刷新节点显示

            order_control.Changed = false;

            if (bItemCreated == true)
                return 2;

            return 1;
        }

        // 根据验收数据，自动创建新的册
        // return:
        //      -1  error
        //      0   没有创建册
        //      1   创建了册
        int CreateNewItems(List<IssueBindingItem> issueitems,
            string strAcceptBatchNo,    // 验收批次号
            bool bSetProcessingState,   // 是否为状态加入“加工中”
            out string strError)
        {
            strError = "";
            int nRet = 0;

            List<CellBase> new_cells = new List<CellBase>();
            for (int i = 0; i < issueitems.Count; i++)
            {
                IssueBindingItem issue_item = issueitems[i];

                if (String.IsNullOrEmpty(issue_item.OrderInfo) == true)
                    continue;

                bool bOrderChanged = false;

                // 针对一个期内每个订购记录的循环
                XmlNodeList order_nodes = issue_item.dom.DocumentElement.SelectNodes("orderInfo/*");
                for (int j = 0; j < order_nodes.Count; j++)
                {
                    XmlNode order_node = order_nodes[j];

                    string strDistribute = DomUtil.GetElementText(order_node, "distribute");

                    LocationColletion locations = new LocationColletion();
                    nRet = locations.Build(strDistribute,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    bool bLocationChanged = false;

                    // 为每个馆藏地点创建一个实体记录
                    for (int k = 0; k < locations.Count; k++)
                    {
                        Location location = locations[k];

                        // TODO: 要注意两点：1) 已经验收过的行，里面出现*的refid，是否要再次创建册？这样效果结识，反复用的时候有好处
                        // 2) 没有验收足的时候，是不是要按照验收足来循环了？检查一下

                        // 已经创建过的事项，跳过
                        if (location.RefID != "*")
                            continue;

                        GenerateEntityData e = new GenerateEntityData();

                        location.RefID = Guid.NewGuid().ToString();   // 修改到馆藏地点字符串中

                        bLocationChanged = true;

                        XmlDocument dom = new XmlDocument();
                        dom.LoadXml("<root />");

                        // 2009/10/19 
                        // 状态
                        if (bSetProcessingState == true)
                        {
                            // 增补“加工中”值
                            string strOldState = DomUtil.GetElementText(dom.DocumentElement,
                                "state");
                            DomUtil.SetElementText(dom.DocumentElement,
                                "state", Global.AddStateProcessing(strOldState));
                        }

                        // seller
                        string strSeller = DomUtil.GetElementText(order_node,
                            "seller");

                        // seller内是单纯值
                        DomUtil.SetElementText(dom.DocumentElement,
                            "seller", strSeller);

                        string strOldValue = "";
                        string strNewValue = "";

                        // source
                        string strSource = DomUtil.GetElementText(order_node,
                            "source");

                        // source内采用新值
                        // 分离 "old[new]" 内的两个值
                        OrderDesignControl.ParseOldNewValue(strSource,
                            out strOldValue,
                            out strNewValue);
                        DomUtil.SetElementText(dom.DocumentElement,
                            "source", strNewValue);

                        // price
                        string strPrice = DomUtil.GetElementText(order_node,
                            "price");

                        // price内采用新值
                        OrderDesignControl.ParseOldNewValue(strPrice,
                            out strOldValue,
                            out strNewValue);
                        DomUtil.SetElementText(dom.DocumentElement,
                            "price", strNewValue);

                        // location
                        string strLocation = location.Name;
                        DomUtil.SetElementText(dom.DocumentElement,
                            "location", strLocation);

                        // publishTime
                        DomUtil.SetElementText(dom.DocumentElement,
                            "publishTime", issue_item.PublishTime);

                        // volume 其实是当年期号、总期号、卷号在一起的一个字符串
                        string strVolume = IssueManageControl.BuildItemVolumeString(issue_item.Issue,
                            issue_item.Zong,
                            issue_item.Volume);
                        DomUtil.SetElementText(dom.DocumentElement,
                            "volume", strVolume);

                        // 批次号
                        DomUtil.SetElementText(dom.DocumentElement,
                            "batchNo", strAcceptBatchNo);

                        ItemBindingItem item = new ItemBindingItem();
                        nRet = item.Initial(dom.OuterXml, out strError);
                        if (nRet == -1)
                            return -1;
                        item.Container = issue_item;
                        PlaceSingleToTail(item);
                        item.Changed = true;
                        item.RefID = Guid.NewGuid().ToString();
                        item.NewCreated = true;
                        item.ContainerCell.Select(SelectAction.On);
                        new_cells.Add(item.ContainerCell);
                    }

                    // 馆藏地点字符串有变化，需要反映给调主
                    if (bLocationChanged == true)
                    {
                        strDistribute = locations.ToString();
                        DomUtil.SetElementText(order_node,
                            "distribute", strDistribute);
                        bOrderChanged = true;
                        // order_item.RefreshListView();
                    }

                } // end of for j

                if (bOrderChanged == true)
                {
                    issue_item.OrderInfo = DomUtil.GetElementInnerXml(issue_item.dom.DocumentElement,
                        "orderInfo");
                    issue_item.Changed = true;

                    // 刷新Issue?
                }

            } // end of for i


            if (new_cells.Count > 0)
            {
                // this.SelectObjects(new_cells, SelectAction.On);
                this.AfterWidthChanged(true);
                return 1;
            }


            return 0;
        }

                // 根据期信息初始化采购控件
        // return:
        //      -1  出错
        //      0   没有找到对应的采购信息
        //      1   找到采购信息
        public int InitialOrderControl(
            IssueBindingItem issue,
            OrderDesignControl order_control,
            out string strOrderInfoMessage,
            out string strError)
        {
            strError = "";
            strOrderInfoMessage = "";

            List<string> XmlRecords = new List<string>();
            XmlNodeList nodes = issue.dom.DocumentElement.SelectNodes("orderInfo/*");

            if (nodes.Count > 0)
            {
                // XML数据已经具备
                for (int i = 0; i < nodes.Count; i++)
                {
                    XmlRecords.Add(nodes[i].OuterXml);
                }
            }
            else if (this.GetOrderInfo != null)
            {
                // 需要从外部获得采购信息
                GetOrderInfoEventArgs e1 = new GetOrderInfoEventArgs();
                e1.BiblioRecPath = "";
                e1.PublishTime = issue.PublishTime;
                this.GetOrderInfo(this, e1);
                if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
                {
                    strError = "在获取本种内出版日期为 '" + issue.PublishTime + "' 的订购信息的过程中发生错误: " + e1.ErrorInfo;
                    return -1;
                }

                XmlRecords = e1.OrderXmls;

                if (XmlRecords.Count == 0)
                {
                    strOrderInfoMessage = "出版日期 '" + issue.PublishTime + "' 没有对应的的订购信息";
                    // EanbleOrderDesignControl(false);

                    // issue.OrderedCount = -1;
                    return 0;
                }
            }

            strOrderInfoMessage = "";
            // EanbleOrderDesignControl(true);

            // return:
            //      -1  error
            //      >=0 订购的总份数
            int nRet = LoadOrderDesignItems(XmlRecords,
                order_control,
                out strError);
            if (nRet == -1)
                return -1;

            // issue.OrderedCount = nRet;

            return 1;
        }

#endif

        // 是否已经挂接的GetOrderInfo事件
        public bool HasGetOrderInfo()
        {
            if (this.GetOrderInfo == null)
                return false;

            return true;
        }

        // 强制让获取订购信息限定在当前用户管辖范围内
        bool m_bForceNarrowRange = false;

        public void DoGetOrderInfo(object sender, GetOrderInfoEventArgs e)
        {
            if (this.GetOrderInfo != null)
            {
                if (m_bForceNarrowRange == true)
                    e.LibraryCodeList = this.LibraryCodeList;

                this.GetOrderInfo(this, e);
            }
            else
            {
                Debug.Assert(false, "");
            }
        }



        // 对收尾状态进行全面检查
        public int Check(out string strError)
        {
            strError = "";

            // 1) 所有期对象的refID都应非空，并且不重复
            // publishtime不重复
            List<string> issue_refids = new List<string>();
            List<string> issue_publishtimes = new List<string>();
            for (int i = 0; i < this.Issues.Count; i++)
            {
                IssueBindingItem issue = this.Issues[i];
                Debug.Assert(issue != null, "");

                // 跳过自由期
                if (String.IsNullOrEmpty(issue.PublishTime) == true)
                    continue;

                if (String.IsNullOrEmpty(issue.RefID) == true
                    && issue.Virtual == false)
                {
                    if (String.IsNullOrEmpty(strError) == false)
                        strError += ";";
                    strError += "期 '" + issue.Caption + "' 的参考ID值为空";
                    continue;
                }

                // 对refid查重
                if (string.IsNullOrEmpty(issue.RefID) == false  // 需要非空才查重
                    && issue_refids.IndexOf(issue.RefID) != -1)
                {
                    if (String.IsNullOrEmpty(strError) == false)
                        strError += ";";
                    strError += "期 '" + issue.Caption + "' 的refID值 '" + issue.RefID + "' 和其他期发生了重复";
                }

                issue_refids.Add(issue.RefID);

                // 对publishtime查重
                if (issue_publishtimes.IndexOf(issue.PublishTime) != -1)
                {
                    if (String.IsNullOrEmpty(strError) == false)
                        strError += ";";
                    strError += "期 '" + issue.Caption + "' 的出版时间值 '" + issue.PublishTime + "' 和其他期发生了重复";
                }

                issue_publishtimes.Add(issue.PublishTime);
            }

            if (String.IsNullOrEmpty(strError) == false)
                return -1;

            return 0;
        }



        // 确保最接近当前日期的期格子显示在视线内
        public IssueBindingItem EnsureCurrentIssueVisible()
        {
            // 寻找和当前日期最接近的期格子
            DateTime now = DateTime.Now;
            TimeSpan min_delta = new TimeSpan(0);
            IssueBindingItem nearest_issue = null;
            for (int i = 0; i < this.Issues.Count; i++)
            {
                IssueBindingItem issue = this.Issues[i];

                if (String.IsNullOrEmpty(issue.PublishTime) == true)
                    continue;

                string strTime = "";
                try
                {
                    strTime = DateTimeUtil.ForcePublishTime8(issue.PublishTime);
                }
                catch
                {
                    continue;
                }

                DateTime time = DateTimeUtil.Long8ToDateTime(strTime);
                TimeSpan delta;
                if (time > now)
                    delta = time - now;
                else
                    delta = now - time;

                if (nearest_issue == null)
                {
                    nearest_issue = issue;
                    min_delta = delta;
                    continue;
                }

                if (min_delta > delta)
                {
                    min_delta = delta;
                    nearest_issue = issue;
                }
            }

            if (nearest_issue != null)
            {
                this.EnsureVisible(nearest_issue);
                if (m_lastFocusObj == null)
                {
                    // 自动把第一个格子设为焦点

                    m_lastFocusObj = nearest_issue.GetFirstCell();
                    SetObjectFocus(m_lastFocusObj);
                }
                return nearest_issue;
            }

            return null;
        }

        // 用卷期信息搜寻期对象
        int SearchIssue(
            VolumeInfo info,
            out List<IssueBindingItem> issues,
            out string strError)
        {
            strError = "";
            issues = new List<IssueBindingItem>();

            if (String.IsNullOrEmpty(info.Year) == true)
            {
                strError = "info.Year不能为空";
                return -1;
            }

            for (int i = 0; i < this.Issues.Count; i++)
            {
                IssueBindingItem issue = this.Issues[i];

                if (String.IsNullOrEmpty(issue.PublishTime) == true)
                    continue;

                string strYearPart = dp2StringUtil.GetYearPart(issue.PublishTime);

                // 检查出版时间
                if (strYearPart != info.Year)
                    continue;

                if (info.IssueNo == issue.Issue)
                    issues.Add(issue);
            }

            // 如果多于一个，才用总期号来过滤
            if (issues.Count > 1 && String.IsNullOrEmpty(info.Zong) == false)
            {
                List<IssueBindingItem> temp = new List<IssueBindingItem>();
                for (int i = 0; i < issues.Count; i++)
                {
                    IssueBindingItem issue = issues[i];
                    if (issue.Zong == info.Zong)
                        temp.Add(issue);
                }

                if (temp.Count == 0)
                {
                }
                else if (temp.Count < issues.Count)
                    issues = temp;
            }

            // 如果多于一个，才用卷号来过滤
            if (issues.Count > 1 && String.IsNullOrEmpty(info.Volume) == false)
            {
                List<IssueBindingItem> temp = new List<IssueBindingItem>();
                for (int i = 0; i < issues.Count; i++)
                {
                    IssueBindingItem issue = issues[i];
                    if (issue.Volume == info.Volume)
                        temp.Add(issue);
                }

                if (temp.Count == 0)
                {
                }
                else if (temp.Count < issues.Count)
                    issues = temp;
            }

            return 0;
        }

        // TODO: 增加在事项已经在内存中的那种初始化
        // 初始化。一次性初始化，不再需要其他函数
        // parameters:
        //      strLayoutMode   "auto" "accepting" "binding"。auto为自动模式，accepting为全部行为记到，binding为全部行为装订
        // return:
        //      -1  出错
        //      0   成功
        //      1   成功，但有警告。警告信息在strError中
        public int NewInitial(
            string strLayoutMode,
            List<string> ItemXmls,
            List<string> IssueXmls,
            List<string> IssueObjectXmls,
            out string strError)
        {
            strError = "";
            string strWarning = "";
            int nRet = 0;

            if (strLayoutMode == "auto"
                || strLayoutMode == "accepting"
                || strLayoutMode == "binding")
            {
                this.WholeLayout = strLayoutMode;
            }
            else
            {
                strError = "未知的布局模式 '" + strLayoutMode + "'";
                return -1;
            }

            // 首次设置状态。一般情况为隐藏编辑控件
            if (this.CellFocusChanged != null)
            {
                FocusChangedEventArgs e = new FocusChangedEventArgs();
                this.CellFocusChanged(this, e);
            }

            Hashtable placed_table = new Hashtable();   // 已经被作为合订本下属安放过位置的册对象

            // 把所有期对象的Cells数组清空
            this.FreeIssue = null;
            for (int i = 0; i < this.Issues.Count; i++)
            {
                IssueBindingItem issue = this.Issues[i];
                issue.Cells.Clear();

                if (String.IsNullOrEmpty(issue.PublishTime) == true)
                    this.FreeIssue = issue;
            }

            // 如果没有，则创建自由期
            if (this.FreeIssue == null)
            {
                this.FreeIssue = new IssueBindingItem();
                this.FreeIssue.Container = this;
                this.Issues.Add(this.FreeIssue);
            }

            // 创建this.InitalItems
            nRet = CreateInitialItems(ItemXmls,
                out strError);
            if (nRet == -1)
                return -1;


            // 创建this.Issues
            nRet = CreateIssues(IssueXmls,
                IssueObjectXmls,
                out strError);
            if (nRet == -1)
                return -1;

            // 创建this.ParentItems
            nRet = CreateParentItems(out strError);
            if (nRet == -1)
                return -1;

            // 剩下的就是无归属的单册了

            // 处理没有期归属的单册对象，将它们归属到适当的Issue对象的Items成员中
            // 由于virtual issues这时已经创建了，后面才创建合订册，因此初始化合订纵向范围不会(?)出现断裂
            // 注：这里显然没有考虑bindingxml中可能会出现的virtual出版日期。那时有可能会出现断裂
            for (int i = 0; i < this.InitialItems.Count; i++)
            {
                ItemBindingItem item = this.InitialItems[i];
                Debug.Assert(item != null, "");
                Debug.Assert(item.Container == null, "");


                IssueBindingItem issue = this.FindIssue(item.PublishTime);
                if (issue != null)
                {
                    // 注：这里如果publishtime为空的，正好加入到自由期
                    issue.Items.Add(item);
                    item.Container = issue;
                }
                else
                {
                    Debug.Assert(String.IsNullOrEmpty(item.PublishTime) == false, "");

                    issue = this.NewIssue(item.PublishTime,
                        item.Volume);
                    Debug.Assert(issue != null, "");
                    issue.Virtual = true;
                    issue.Items.Add(item);
                    item.Container = issue;
                }
            }
            this.InitialItems.Clear();

            List<PublishTimeAndVolume> publishtimes = new List<PublishTimeAndVolume>();
            // 统计(bindingxml中)全部合订成员册所使用过的publishtime字符串
            nRet = GetAllBindingXmlPublishTimes(
                out publishtimes,
                out strError);
            if (nRet == -1)
                return -1;
            for (int i = 0; i < publishtimes.Count; i++)
            {
                PublishTimeAndVolume item = publishtimes[i];
                string strPublishTime = item.PublishTime;
                Debug.Assert(String.IsNullOrEmpty(strPublishTime) == false);

                IssueBindingItem issue = this.FindIssue(strPublishTime);
                if (issue == null)
                {
                    issue = this.NewIssue(strPublishTime,
                        item.Volume);
                    Debug.Assert(issue != null, "");
                    issue.Virtual = true;
                }
            }

            /*
            // 调试
            while (this.ParentItems.Count > 3)
                this.ParentItems.RemoveAt(3);
             * */

            //// ////
            // parent_item --> member_items
            Hashtable memberitems_table = null;

            // 遍历合订册对象数组，建立成员对象数组
            nRet = CreateMemberItemTable(
                ref this.ParentItems,
                out memberitems_table,
                ref strWarning,
                out strError);
            if (nRet == -1)
                return -1;

            // 把超过管辖范围的合订册单元去掉
            if (this.m_bHideLockedBindingCell == true
                && Global.IsGlobalUser(this.LibraryCodeList) == false)
            {

#if NO
                for (int i = 0; i < this.ParentItems.Count; i++)
                {
                    ItemBindingItem parent_item = this.ParentItems[i];
                    List<ItemBindingItem> member_items = (List<ItemBindingItem>)memberitems_table[parent_item];

                    Debug.Assert(member_items != null, "");

                    if (member_items.Count == 0)
                        continue;

                    // 检查一个合订册的所有成员,看看是不是(至少一个)和当前可见订购组有从属关系?
                    // return:
                    //      -1  出错
                    //      0   没有交叉
                    //      1   有交叉
                    nRet = IsMemberCrossOrderGroup(parent_item,
                        member_items,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    string strLibraryCode = Global.GetLibraryCode(parent_item.LocationString);

                    bool bLocked = (StringUtil.IsInList(strLibraryCode, this.LibraryCodeList) == false);
                    parent_item.Locked = bLocked;

                    // 合订本本身馆代码在外，而且其成员也不和可见订购组交叉的，删除合订本对象
                    if (bLocked == true
                        && nRet == 0)
                    {
                        // this.RemoveItem(parent_item, false);

                        // 此时尚未加入Issue对象下面
                        this.m_hideitems.Add(parent_item);

                        this.ParentItems.RemoveAt(i);
                        i--;
                    }
                }

#endif
                // 把当前超过管辖范围的合订册单元去掉
                nRet = RemoveOutofBindingItems(
                    ref this.ParentItems,
                    memberitems_table,
                    false,
                    false,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 把自由期中的超过范围的单元去掉
                if (this.FreeIssue != null)
                {
                    foreach (Cell cell in this.FreeIssue.Cells)
                    {
                        if (cell == null || cell.item == null)
                            continue;
                        string strLibraryCode = Global.GetLibraryCode(cell.item.LocationString);
                        if (StringUtil.IsInList(strLibraryCode, this.LibraryCodeList) == false)
                        {
                            this.FreeIssue.Cells.Remove(cell);
                            if (cell.item != null)
                                this.m_hideitems.Add(cell.item);
                        }
                    }
                }

            }

            // 安放合订成员册对象
            nRet = PlaceMemberCell(
                ref this.ParentItems,
                memberitems_table,
                ref placed_table,
                out strError);
            if (nRet == -1)
                return -1;

            // 安放其余册对象。即非合订成员册
            for (int i = 0; i < this.Issues.Count; i++)
            {
                // 先存储再排序
                List<ItemBindingItem> items = new List<ItemBindingItem>();

                IssueBindingItem issue = this.Issues[i];
                for (int j = 0; j < issue.Items.Count; j++)
                {
                    ItemBindingItem item = issue.Items[j];

                    if (placed_table[item] != null)
                        continue;

                    items.Add(item);
                }
                issue.Items.Clear();    // 初始化使命完成以后立即清除

                if (items.Count > 0)
                {
                    if (String.IsNullOrEmpty(issue.PublishTime) == false)
                    {
                        // 按照Intact排序
                        items.Sort(new ItemIntactComparer());
                    }

                    for (int j = 0; j < items.Count; j++)
                    {
                        ItemBindingItem item = items[j];
                        // 安放在两个一组的右边位置
                        PlaceSingleToTail(item);
                    }
                }

                if (String.IsNullOrEmpty(issue.PublishTime) == false)
                {
                    // 首次设置每个格子的OutputIssue值
                    issue.RefreshAllOutofIssueValue();
                }
            }

            // 把Cell中Missing状态的Item全部设置为null
            for (int i = 0; i < this.Issues.Count; i++)
            {
                IssueBindingItem issue = this.Issues[i];
                for (int j = 0; j < issue.Cells.Count; j++)
                {
                    Cell cell = issue.Cells[j];
                    if (cell != null && cell.item != null)
                    {
                        if (cell.item.Missing == true)
                            cell.item = null;
                    }
                }
            }

#if NO
            // 把超过管辖范围的合订册单元去掉
            if (this.HideLockedBindingCell == true
                && Global.IsGlobalUser(this.LibraryCodeList) == false)
            {
                for (int i = 0; i < this.ParentItems.Count; i++)
                {
                    ItemBindingItem parent_item = this.ParentItems[i];

                    // 检查一个合订册的所有成员,看看是不是(至少一个)和当前可见订购组有从属关系?
                    // return:
                    //      -1  出错
                    //      0   没有交叉
                    //      1   有交叉
                    nRet = IsMemberCrossOrderGroup(parent_item,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    string strLibraryCode = Global.GetLibraryCode(parent_item.LocationString);
                    // 合订本本身馆代码在外，而且其成员也不和可见订购组交叉的，删除合订本对象
                    if (StringUtil.IsInList(strLibraryCode, this.LibraryCodeList) == false
                        && nRet == 0)
                    {
                        this.RemoveItem(parent_item, true);
                        this.ParentItems.RemoveAt(i);
                        i--;
                    }
                }

                // 把自由期中的超过范围的单元去掉
                if (this.FreeIssue != null)
                {
                    foreach (Cell cell in this.FreeIssue.Cells)
                    {
                        if (cell == null || cell.item == null)
                            continue;
                        string strLibraryCode = Global.GetLibraryCode(cell.item.LocationString);
                        if (StringUtil.IsInList(strLibraryCode, this.LibraryCodeList) == false)
                        {
                            this.FreeIssue.Cells.Remove(cell);
                        }
                    }
                }
            // TODO: 需要重新placement
            }
#endif

            // 设置记到布局模式

            if (strLayoutMode == "auto")
            {
                for (int i = 0; i < this.Issues.Count; i++)
                {
                    IssueBindingItem issue = this.Issues[i];
                    // 跳过自由期
                    if (String.IsNullOrEmpty(issue.PublishTime) == true)
                        continue;
                    if (issue.HasMemberOrParentCell() == true)
                    {
                        issue.IssueLayoutState = IssueLayoutState.Binding;
                        nRet = issue.InitialLayoutBinding(out strError);
                        if (nRet == -1)
                            return -1;
                    }
                    else
                    {
                        issue.IssueLayoutState = IssueLayoutState.Accepting;
                        nRet = issue.LayoutAccepting(out strError);
                        if (nRet == -1)
                            return -1;
                    }
                }
            }
            else if (strLayoutMode == "accepting")
            {
                for (int i = 0; i < this.Issues.Count; i++)
                {
                    IssueBindingItem issue = this.Issues[i];
                    // 跳过自由期
                    if (String.IsNullOrEmpty(issue.PublishTime) == true)
                        continue;
                    {
                        issue.IssueLayoutState = IssueLayoutState.Accepting;
                        nRet = issue.LayoutAccepting(out strError);
                        if (nRet == -1)
                            return -1;
                    }
                }
            }
            else
            {
                Debug.Assert(strLayoutMode == "binding", "");
                // 本来就是全部行已经为装订模式
                for (int i = 0; i < this.Issues.Count; i++)
                {
                    IssueBindingItem issue = this.Issues[i];
                    // 跳过自由期
                    if (String.IsNullOrEmpty(issue.PublishTime) == true)
                        continue;
                    {
                        issue.IssueLayoutState = IssueLayoutState.Binding;
                        nRet = issue.InitialLayoutBinding(out strError);
                        if (nRet == -1)
                            return -1;
                    }
                }
            }

            // 准备封面图像
            foreach (IssueBindingItem issue in this.Issues)
            {
                // 跳过自由期
                if (String.IsNullOrEmpty(issue.PublishTime) == true)
                    continue;

                issue.PrepareCoverImage();
            }
#if DEBUG
            this.VerifyAll();
#endif


            AfterWidthChanged(true);
            // Debug.WriteLine("NewInitial() AfterWidthChanged() done");

            if (String.IsNullOrEmpty(strWarning) == false)
            {
                strError = strWarning;
                return 1;
            }

            return 0;
        }

        // 2012/9/29
        // 遍历合订册对象数组，建立成员对象数组
        // parameters:
        //      parent_items    合订册对象数组。处理后的对象会从这个数组中移走
        int CreateMemberItemTable(
            ref List<ItemBindingItem> parent_items,
            out Hashtable memberitems_table,
            ref string strWarning,
            out string strError)
        {
            strError = "";
            int nRet = 0;
            // parent_item --> member_items
            memberitems_table = new Hashtable();

            // dup_table
            // 用于检查一个册事项被重复用于多个合订册的 hashtable
            // memberitem --> parent_item
            Hashtable dup_table = new Hashtable();

            // 遍历合订册对象数组，建立成员对象数组
            for (int i = 0; i < parent_items.Count; i++)
            {
                ItemBindingItem parent_item = parent_items[i];
                List<ItemBindingItem> member_items = new List<ItemBindingItem>();

                string strBindingXml = parent_item.Binding;
                if (String.IsNullOrEmpty(strBindingXml) == true)
                {
                    string strVolume = parent_item.Volume;

                    if (String.IsNullOrEmpty(strVolume) == true)
                    {
                        // 如果没有BindingXml，并且没有卷期范围，则被移动到自由区域
                        parent_items.Remove(parent_item);
                        Cell temp = new Cell();
                        temp.item = parent_item;
                        AddToFreeIssue(temp);
                        i--;
                        continue;
                    }

                    List<VolumeInfo> infos = null;
                    nRet = VolumeInfo.BuildVolumeInfos(strVolume,
                        out infos,
                        out strError);
                    if (nRet == -1 || infos.Count == 0) // 2015/5/8
                    {
                        parent_item.Comment += "\r\n解析卷期字符串的时候发生错误: " + strError;
                        parent_items.Remove(parent_item);
                        Cell temp = new Cell();
                        temp.item = parent_item;
                        AddToFreeIssue(temp);
                        i--;
                        continue;
                    }

                    bool bFailed = false;
                    string strFailMessage = "";
                    for (int j = 0; j < infos.Count; j++)
                    {
                        VolumeInfo info = infos[j];

                        ItemBindingItem sub_item = null;

                        // TODO: 可以征用refid为*的订购信息内的对象

                        // 通过卷期信息寻找合适的可以依附的期对象
                        List<IssueBindingItem> issues = null;
                        // 用卷期信息搜寻期对象
                        nRet = SearchIssue(
                            info,
                            out issues,
                            out strError);
                        if (nRet == -1)
                            return -1;

                        if (issues == null || issues.Count == 0)
                        {
                            // 搜寻失败，
                            // TODO: 也许可以指出出错原因?
                            strFailMessage = "期号(年:" + info.Year + ") '" + info.IssueNo + "' 没有找到期对象";
                            bFailed = true;
                            break;
                        }

                        string strPublishTime = issues[0].PublishTime;
                        Debug.Assert(String.IsNullOrEmpty(strPublishTime) == false, "");

                        if (sub_item == null)
                        {
                            // 如果没有现成的item对象，则通过<item>元素的相关属性来创建
                            sub_item = new ItemBindingItem();
                            nRet = sub_item.Initial("<root />", out strError);
                            Debug.Assert(nRet != -1, "");

                            sub_item.Volume = info.GetString();
                            sub_item.PublishTime = strPublishTime;
                            sub_item.RefID = "*";   // 象征性的，或可吸纳升级上来的订购绑定数据
                        }

                        sub_item.ParentItem = parent_item;
                        sub_item.Deleted = true;
                        sub_item.State = "已删除";
                        member_items.Add(sub_item);
                    }

                    if (bFailed == true)
                    {
                        parent_item.Comment += "\r\n卷期字符串中的部分期不存在，无法复原合订状态: " + strFailMessage;
                        parent_items.Remove(parent_item);
                        Cell temp = new Cell();
                        temp.item = parent_item;
                        AddToFreeIssue(temp);
                        i--;
                        continue;
                    }

                    goto PLACEMEMT;
                }

                // 根据refid, 找到它下属的那些ItemBindingItem对象
                XmlDocument dom = new XmlDocument();
                dom.LoadXml("<root />");
                try
                {
                    dom.DocumentElement.InnerXml = strBindingXml;
                }
                catch (Exception ex)
                {
                    strError = "参考ID为 '" + parent_item.RefID + "' 的册信息中，<binding>元素内嵌XML装入DOM时出错: " + ex.Message;
                    return -1;
                }

                /*
                 * bindingxml中，<item>元素未必有refID属性。
                 * 没有refID属性，表明这是一个被删除了册记录的单纯信息单元，或者是缺期情况。
                 * 缺期可能发生在装订范围的第一册或者最后一册，要引起注意
                 * */

                parent_item.MemberCells.Clear();
                XmlNodeList nodes = dom.DocumentElement.SelectNodes("item");
                if (nodes.Count == 0)
                {
                    // 虽然有BindingXml，但没有任何下级<item>元素，则被移动到自由区域
                    parent_items.Remove(parent_item);
                    Cell temp = new Cell();
                    temp.item = parent_item;
                    AddToFreeIssue(temp);
                    i--;
                    continue;
                }

                for (int j = 0; j < nodes.Count; j++)
                {
                    XmlNode node = nodes[j];
                    string strRefID = DomUtil.GetAttr(node, "refID");

                    bool bItemRecordDeleted = false;    // 册记录是否被删除?

                    if (String.IsNullOrEmpty(strRefID) == true)
                    {
                        bItemRecordDeleted = true;
                    }

                    ItemBindingItem sub_item = null;

                    if (bItemRecordDeleted == false)
                    {
                        sub_item = InitialFindItemByRefID(strRefID);
                        if (sub_item == null)
                        {
                            // 2012/9/29
                            sub_item = FindItemByRefID(strRefID, this.m_hideitems);
                            if (sub_item != null)
                                this.m_hideitems.Remove(sub_item);
                        }

                        if (sub_item == null)
                        {
                            if (String.IsNullOrEmpty(strWarning) == false)
                                strWarning += "; ";
                            // strWarning += "参考ID为 '" + parent_item.RefID + "' 的册信息中，<binding>元素内包含的参考ID '" + strRefID + "' 没有找到对应的册信息";
                            bItemRecordDeleted = true;
                        }
                    }

                    if (sub_item == null)
                    {
                        // 如果没有现成的item对象，则通过<item>元素的相关属性来创建
                        sub_item = new ItemBindingItem();
                        nRet = sub_item.Initial("<root />", out strError);
                        Debug.Assert(nRet != -1, "");

                        // TODO: 可以把node中所有的属性都翻译为item中的同名元素，这样就允许保存的地方随意扩充字段了
                        sub_item.Volume = DomUtil.GetAttr(node, "volume");
                        sub_item.PublishTime = DomUtil.GetAttr(node, "publishTime");
                        sub_item.RefID = DomUtil.GetAttr(node, "refID");
                        sub_item.Barcode = DomUtil.GetAttr(node, "barcode");
                        sub_item.RegisterNo = DomUtil.GetAttr(node, "registerNo");

                        // 2011/9/8
                        sub_item.Price = DomUtil.GetAttr(node, "price");

                        bool bMissing = false;
                        // 获得布尔型的属性参数值
                        // return:
                        //      -1  出错。但是bValue中已经有了bDefaultValue值，可以不加警告而直接使用
                        //      0   正常获得明确定义的参数值
                        //      1   参数没有定义，因此代替以缺省参数值返回
                        DomUtil.GetBooleanParam(node,
                            "missing",
                            false,
                            out bMissing,
                            out strError);
                        sub_item.Missing = bMissing;

                        if (String.IsNullOrEmpty(sub_item.PublishTime) == true)
                        {
                            // 没有publishtime，也无法安放
                        }
                    }
                    else
                    {
                        // 2018/8/22
                        // 检查 refid 是否被不同的合订册重复使用
                        if (dup_table.ContainsKey(sub_item))
                        {
                            ItemBindingItem parent = dup_table[sub_item] as ItemBindingItem;
                            // TODO: 需要增加一个获得事项描述名称的函数
                            strError = "数据错误：成员册事项 '" + sub_item.RefID + "' 被重复用于合订册 '" + parent.RefID + "' 和 '" + parent_item.RefID + "'";
                            return -1;
                        }
                        dup_table[sub_item] = parent_item;
                    }

                    // sub_item.Binded = true; // 注：place阶段会设置的
                    sub_item.ParentItem = parent_item;
                    if (sub_item.Missing == false
                        && bItemRecordDeleted == true)
                    {
                        sub_item.Deleted = bItemRecordDeleted;
                        sub_item.State = "已删除";
                    }

                    member_items.Add(sub_item);

                    // 使用后自然会被丢弃
                }

            PLACEMEMT:
                memberitems_table[parent_item] = member_items;
            }

            return 0;
        }

        // 2012/9/29
        // 只安放合订册对象
        // parameters:
        //      parent_items    合订册对象数组。无法安放的合订册对象会从这个数组中移走
        int PlaceParentItems(
            ref List<ItemBindingItem> parent_items,
            Hashtable memberitems_table,
            out string strError)
        {
            strError = "";

            ////
            // 安放册对象
            for (int i = 0; i < parent_items.Count; i++)
            {
                ItemBindingItem parent_item = parent_items[i];

                // 2012/9/28
                if (Global.IsGlobalUser(this.LibraryCodeList) == false)
                {
                    string strLibraryCode = Global.GetLibraryCode(parent_item.LocationString);
                    parent_item.Locked = (StringUtil.IsInList(strLibraryCode, this.LibraryCodeList) == false);
                }

                List<ItemBindingItem> member_items = (List<ItemBindingItem>)memberitems_table[parent_item];

                Debug.Assert(member_items != null, "");

                if (member_items.Count == 0)
                    continue;

                // 布局

                // 把合订册的ItemBindingItem对象安放在其下属的第一个册对象所在的期行内
                ItemBindingItem first_sub = member_items[0];
                IssueBindingItem first_issue = first_sub.Container;

                if (first_issue == null)
                {
                    // Debug.Assert(first_sub.Missing == true, "");

                    if (String.IsNullOrEmpty(first_sub.PublishTime) == true)
                    {
                        // 成员册没有publishtime无法安放
                        // 这就导致parent被移动到自由区
                        parent_items.Remove(parent_item);
                        Cell temp = new Cell();
                        temp.item = parent_item;
                        temp.item.Comment += "\r\n合订册的第一个成员册没有出版时间信息，因此合订册也无法正常安放，只好放在自由期中";
                        AddToFreeIssue(temp);
                        i--;
                        continue;
                    }

                    Debug.Assert(String.IsNullOrEmpty(first_sub.PublishTime) == false, "");

                    first_issue = this.FindIssue(first_sub.PublishTime);
                    if (first_issue == null)
                    {
                        first_issue = this.NewIssue(first_sub.PublishTime,
                            first_sub.Volume);
                    }
                }

                Debug.Assert(first_issue != null, "");

                // 安放在两个一组的靠左位置。偶数
                int col = -1;
                col = first_issue.GetFirstAvailableBoundColumn();

                // 安放合订本对象
                Cell parent_cell = new Cell();
                parent_cell.item = parent_item;
                parent_item.Container = first_issue; // 假装属于这个期
                first_issue.SetCell(col, parent_cell);

                // 安放下属的单独册
                try
                {
                    PlaceMemberItems(parent_cell,
                        member_items,
                        col + 1,
                        true);
                }
                catch (Exception ex)
                {
                    strError = "BindingControl PlaceMemberItems() exception: " + ExceptionUtil.GetAutoText(ex);
                    return -1;
                }
            }

            return 0;
        }

        // 2012/9/29
        // 安放合订成员册对象
        // parameters:
        //      parent_items    合订册对象数组。无法安放的合订册对象会从这个数组中移走
        int PlaceMemberCell(
            ref List<ItemBindingItem> parent_items,
            Hashtable memberitems_table,
            ref Hashtable placed_table,
            out string strError)
        {
            strError = "";

            ////
            // 安放册对象
            for (int i = 0; i < parent_items.Count; i++)
            {
                ItemBindingItem parent_item = parent_items[i];

                // 2012/9/28
                if (Global.IsGlobalUser(this.LibraryCodeList) == false)
                {
                    string strLibraryCode = Global.GetLibraryCode(parent_item.LocationString);
                    parent_item.Locked = (StringUtil.IsInList(strLibraryCode, this.LibraryCodeList) == false);
                }

                List<ItemBindingItem> member_items = (List<ItemBindingItem>)memberitems_table[parent_item];

                Debug.Assert(member_items != null, "");

                if (member_items.Count == 0)
                    continue;

                // 布局

                // 把合订册的ItemBindingItem对象安放在其下属的第一个册对象所在的期行内
                ItemBindingItem first_sub = member_items[0];
                IssueBindingItem first_issue = first_sub.Container;

                if (first_issue == null)
                {
                    // Debug.Assert(first_sub.Missing == true, "");

                    if (String.IsNullOrEmpty(first_sub.PublishTime) == true)
                    {
                        // 成员册没有publishtime无法安放
                        // 这就导致parent被移动到自由区
                        parent_items.Remove(parent_item);
                        Cell temp = new Cell();
                        temp.item = parent_item;
                        temp.item.Comment += "\r\n合订册的第一个成员册没有出版时间信息，因此合订册也无法正常安放，只好放在自由期中";
                        AddToFreeIssue(temp);
                        i--;
                        continue;
                    }

                    Debug.Assert(String.IsNullOrEmpty(first_sub.PublishTime) == false, "");

                    first_issue = this.FindIssue(first_sub.PublishTime);
                    if (first_issue == null)
                    {
                        first_issue = this.NewIssue(first_sub.PublishTime,
                            first_sub.Volume);
                    }
                }

                Debug.Assert(first_issue != null, "");

                // 安放在两个一组的靠左位置。偶数
                int col = -1;
                col = first_issue.GetFirstAvailableBoundColumn();

                // 安放合订本对象
                Cell parent_cell = new Cell();
                parent_cell.item = parent_item;
                parent_item.Container = first_issue; // 假装属于这个期
                first_issue.SetCell(col, parent_cell);

                // 安放下属的单独册
                try
                {
                    PlaceMemberItems(parent_cell,
                        member_items,
                        col + 1);
                }
                catch (Exception ex)
                {
                    strError = "BindingControl PlaceMemberItems() {2115A4A3-14E3-4A03-B0EA-C6687C16EAB7} exception: " + ex.Message;
                    return -1;
                }

                /* // this.Changed最后会被改变，就没有了修改记号，不好。还是用菜单命令实现，操作人员需要去调用
                if (String.IsNullOrEmpty(parent_item.PublishTime) == true)
                {
                    if (parent_item.RefreshPublishTime() == true)
                        parent_item.Changed = true;
                }
                 * */

                // 记忆
                foreach (ItemBindingItem temp in member_items)
                {
                    placed_table[temp] = temp;
                }

                /*
                // 稍后才安放合订本对象
                Cell cell = new Cell();
                cell.item = parent_item;
                parent_item.Container = first_issue; // 假装属于这个期
                first_issue.SetCell(col, cell);
                 * */

#if DEBUG
                {
                    string strError1 = "";
                    int nRet1 = parent_item.VerifyMemberCells(out strError1);
                    if (nRet1 == -1)
                    {
                        Debug.Assert(false, strError1);
                    }
                }
#endif
            }

            return 0;
        }

        // 检查一个合订册的所有成员,看看是不是(至少一个)和当前可见订购组有从属关系?
        // parameters:
        //      bRefreshOrderItem   是否强制刷新订购信息?
        // return:
        //      -1  出错
        //      0   没有交叉
        //      1   有交叉
        int IsMemberCrossOrderGroup(ItemBindingItem parent_item,
            List<ItemBindingItem> member_items,
            bool bRefreshOrderItem,
            out string strError)
        {
            strError = "";

            List<string> visible_refids = new List<string>();

            // 获得所有可见订购组中的refid
            foreach (IssueBindingItem issue in this.Issues)
            {
                if (issue == null)
                    continue;

                // 跳过自由期
                if (string.IsNullOrEmpty(issue.PublishTime) == true)
                    continue;

                if (bRefreshOrderItem == true)
                {
                    issue.ClearOrderItems();
                }

                int nRet = issue.InitialOrderItems(out strError);
                if (nRet == -1)
                    return -1;

                List<string> refids = null;
                // 获得可见的订购组中的refid
                nRet = issue.GetVisibleRefIDs(
                    this.LibraryCodeList,
                    out refids,
                    out strError);
                if (nRet == -1)
                    return -1;

                visible_refids.AddRange(refids);
            }

            foreach (ItemBindingItem item in member_items)
            {
                if (visible_refids.IndexOf(item.RefID) != -1)
                    return 1;
            }

            return 0;
        }

        // 找到并删除一个item。如果这个item是合订本对象，则也要删除其下属的册item
        void RemoveItem(ItemBindingItem item,
            bool bRemoveMemberCell)
        {
            if (bRemoveMemberCell == true)
            {
                foreach (Cell cell in item.MemberCells)
                {
                    if (cell.Container != null)
                    {
                        cell.Container.Cells.Remove(cell);
                        if (cell.item != null)
                            this.m_hideitems.Add(cell.item);
                    }
                }
            }
            item.MemberCells.Clear();

            foreach (IssueBindingItem issue in this.Issues)
            {
                foreach (Cell cell in issue.Cells)
                {
                    if (cell != null && cell.item == item)
                    {
                        issue.Cells.Remove(cell);
                        break;
                    }
                }
            }

            this.m_hideitems.Add(item);
        }

        // 获得索取号相关的部分信息
        // parameters:
        //      exclude_item    要排除的对象
        public List<CallNumberItem> GetCallNumberItems(ItemBindingItem exclude_item)
        {
            List<ItemBindingItem> all_items = this.AllItems;

            List<CallNumberItem> results = new List<CallNumberItem>();
            foreach (ItemBindingItem cur_item in all_items)
            {
                if (cur_item == exclude_item)
                    continue;

                CallNumberItem item = new CallNumberItem();

                item.RecPath = cur_item.RecPath;

#if REF
                // 2017/4/6
                if (string.IsNullOrEmpty(item.RecPath))
                {
                    if (string.IsNullOrEmpty(cur_item.RefID) == true)
                    {
                        // throw new Exception("cur_item 的 RefID 成员不应为空"); // TODO: 可以考虑增加健壮性，当时发生 RefID 字符串
                        cur_item.RefID = Guid.NewGuid().ToString();
                        cur_item.Changed = true;
                    }

                    item.RecPath = "@refID:" + cur_item.RefID;
                }
#endif

                item.CallNumber = cur_item.AccessNo;
                item.Location = cur_item.LocationString;
                item.Barcode = cur_item.Barcode;

                results.Add(item);
            }

            return results;
        }

#if OLD_INITIAL
        // *** 为初始化服务
        // 初始化。应在AppendIssue() AppendNoneIssueSingleItems() 和AppendBindItem()以后调用
        // return:
        //      -1  出错
        //      0   成功
        //      1   成功，但有警告。警告信息在strError中
        public int Initial(out string strError)
        {
            strError = "";
            string strWarning = "";
            int nRet = 0;

            // 首次设置状态。一般情况为隐藏编辑控件
            if (this.CellFocusChanged != null)
            {
                FocusChangedEventArgs e = new FocusChangedEventArgs();
                this.CellFocusChanged(this, e);
            }

            Hashtable placed_table = new Hashtable();   // 已经被作为合订本下属安放过位置的册对象

            // 把所有期对象的Cells数组清空
            this.FreeIssue = null;
            for (int i = 0; i < this.Issues.Count; i++)
            {
                IssueBindingItem issue = this.Issues[i];
                issue.Cells.Clear();

                if (String.IsNullOrEmpty(issue.PublishTime) == true)
                    this.FreeIssue = issue;
            }

            // 如果没有，则创建自由期
            if (this.FreeIssue == null)
            {
                this.FreeIssue = new IssueBindingItem();
                this.FreeIssue.Container = this;
                this.Issues.Add(this.FreeIssue);
            }

            // 处理没有期归属的单册对象，将它们归属到适当的Issue对象的Items成员中
            for (int i = 0; i < this.NoneIssueItems.Count; i++)
            {
                ItemBindingItem item = this.NoneIssueItems[i];
                Debug.Assert(item != null, "");
                Debug.Assert(item.Container == null, "");

                IssueBindingItem issue = this.FindIssue(item.PublishTime);
                if (issue != null)
                {
                    issue.Items.Add(item);
                    item.Container = issue;
                }
                else
                {
                    issue = this.NewIssue(item.PublishTime,
                        item.Volume);
                    Debug.Assert(issue != null, "");
                    issue.Virtual = true;
                    issue.Items.Add(item);
                    item.Container = issue;
                }
            }
            this.NoneIssueItems.Clear();


            // 遍历合订册对象数组
            for (int i = 0; i < this.ParentItems.Count; i++)
            {
                ItemBindingItem parent_item = this.ParentItems[i];

                string strBindingXml = parent_item.Binding;
                if (String.IsNullOrEmpty(strBindingXml) == true)
                {
                    // 如果没有BindingXml，则被移动到自由区域
                    this.ParentItems.Remove(parent_item);
                    Cell temp = new Cell();
                    temp.item = parent_item;
                    AddToFreeIssue(temp);
                    i--;
                    continue;
                }

                // 根据refid, 找到它下属的那些ItemBindingItem对象
                XmlDocument dom = new XmlDocument();
                dom.LoadXml("<root />");
                try
                {
                    dom.DocumentElement.InnerXml = strBindingXml;
                }
                catch (Exception ex)
                {
                    strError = "参考ID为 '"+parent_item.RefID+"' 的册信息中，<binding>元素内嵌XML装入DOM时出错: " + ex.Message;
                    return -1;
                }

                /*
                 * bindingxml中，<item>元素未必有refID属性。
                 * 没有refID属性，表明这是一个被删除了册记录的单纯信息单元，或者是缺期情况。
                 * 缺期可能发生在装订范围的第一册或者最后一册，要引起注意
                 * */

                parent_item.MemberCells.Clear();
                XmlNodeList nodes = dom.DocumentElement.SelectNodes("item");
                if (nodes.Count == 0)
                {
                    // 虽然BindingXml，但没有任何下级<item>元素，则被移动到自由区域
                    this.ParentItems.Remove(parent_item);
                    Cell temp = new Cell();
                    temp.item = parent_item;
                    AddToFreeIssue(temp);
                    i--;
                    continue;
                }

                List<ItemBindingItem> member_items = new List<ItemBindingItem>();

                for (int j = 0; j < nodes.Count; j++)
                {
                    XmlNode node = nodes[j];
                    string strRefID = DomUtil.GetAttr(node, "refID");

                    bool bItemRecordDeleted = false;    // 册记录是否被删除?

                    if (String.IsNullOrEmpty(strRefID) == true)
                    {
                        bItemRecordDeleted = true;
                    }

                    ItemBindingItem sub_item = null;

                    if (bItemRecordDeleted == false)
                    {
                        sub_item = FindItem(strRefID);
                        if (sub_item == null)
                        {
                            if (String.IsNullOrEmpty(strWarning) == false)
                                strWarning += "; ";
                            // strWarning += "参考ID为 '" + parent_item.RefID + "' 的册信息中，<binding>元素内包含的参考ID '" + strRefID + "' 没有找到对应的册信息";
                            bItemRecordDeleted = true;
                        }
                    }

                    if (sub_item == null)
                    {
                        // 如果没有现成的item对象，则通过<item>元素的相关属性来创建
                        sub_item = new ItemBindingItem();
                        nRet = sub_item.Initial("<root />", out strError);
                        Debug.Assert(nRet != -1, "");

                        sub_item.State = "已删除";

                        // TODO: 可以把node中所有的属性都翻译为item中的同名元素，这样就允许保存的地方随意扩充字段了
                        sub_item.Volume = DomUtil.GetAttr(node, "volume");
                        sub_item.PublishTime = DomUtil.GetAttr(node, "publishTime");
                        sub_item.RefID = DomUtil.GetAttr(node, "refID");
                        sub_item.Barcode = DomUtil.GetAttr(node, "barcode");
                        sub_item.RegisterNo = DomUtil.GetAttr(node, "registerNo");

                        bool bMissing = false;
                        // 获得布尔型的属性参数值
                        // return:
                        //      -1  出错。但是nValue中已经有了nDefaultValue值，可以不加警告而直接使用
                        //      0   正常获得明确定义的参数值
                        //      1   参数没有定义，因此代替以缺省参数值返回
                        DomUtil.GetBooleanParam(node,
                            "missing",
                            false,
                            out bMissing,
                            out strError);
                        sub_item.Missing = bMissing;
                    }

                    // sub_item.Binded = true; // 注：place阶段会设置的
                    sub_item.ParentItem = parent_item;
                    sub_item.Deleted = bItemRecordDeleted;

                    member_items.Add(sub_item);

                    // 使用后自然会被丢弃
                }

                // 布局
                if (member_items.Count > 0)
                {
                    // 把合订册的ItemBindingItem对象安放在其下属的第一个册对象所在的期行内
                    ItemBindingItem first_sub = member_items[0];
                    IssueBindingItem first_issue = first_sub.Container;

                    if (first_issue == null)
                    {
                        Debug.Assert(first_sub.Missing == true, "");

                        first_issue = this.FindIssue(first_sub.PublishTime);
                        if (first_issue == null)
                        {
                            first_issue = this.NewIssue(first_sub.PublishTime,
                                first_sub.Volume);
                        }
                    }

                    Debug.Assert(first_issue != null, "");

                    // 安放在两个一组的靠左位置。偶数
                    int col = -1;
                    if ((first_issue.Cells.Count % 2) == 0)
                    {
                        col = first_issue.Cells.Count;    // 记忆安放的列号
                        first_issue.Cells.Add(null);
                    }
                    else
                    {
                        col = first_issue.Cells.Count + 1;
                        first_issue.Cells.Add(null);
                        first_issue.Cells.Add(null);
                    }

                    // 安放下属的单独册
                    string strTemp = "";
                    PlaceMemberItems(parent_item,
                        member_items,
                        col + 1,
                        out strTemp);

                    // 记忆
                    foreach (ItemBindingItem temp in member_items)
                    {
                        placed_table[temp] = temp;
                    }

                    // 稍后才安放合订本对象
                    Cell cell = new Cell();
                    cell.item = parent_item;
                    parent_item.Container = first_issue; // 假装属于这个期
                    first_issue.SetCell(col, cell);

#if DEBUG
                    {
                        string strError1 = "";
                        int nRet1 = parent_item.VerifyMemberCells(out strError1);
                        if (nRet1 == -1)
                        {
                            Debug.Assert(false, strError1);
                        }
                    }
#endif
                }
            }

            // 安放其余册对象。即非合订成员册
            for (int i = 0; i < this.Issues.Count; i++)
            {
                List<ItemBindingItem> items = new List<ItemBindingItem>();
                IssueBindingItem issue = this.Issues[i];
                for (int j = 0; j < issue.Items.Count; j++)
                {
                    ItemBindingItem item = issue.Items[j];

                    if (placed_table[item] != null)
                        continue;

                    items.Add(item);
                }
                issue.Items.Clear();    // 初始化使命完成以后立即清除

                if (items.Count > 0)
                {
                    // 按照Intact排序
                    items.Sort(new ItemIntactComparer());

                    for (int j = 0; j < items.Count; j++)
                    {
                        ItemBindingItem item = items[j];
                        // 安放在两个一组的右边位置
                        PlaceSingleToTail(item);
                    }
                }

                // 首次设置每个格子的OutputIssue值
                issue.RefreshAllOutofIssueValue();


            }


            AfterWidthChanged(true);

            if (String.IsNullOrEmpty(strWarning) == false)
            {
                strError = strWarning;
                return 1;
            }

            return 0;
        }
#endif

        // 兑现所有单册和成员册item的<binding>元素。合订册的<binding>元素已经随时兑现了
        public int Finish(out string strError)
        {
            strError = "";

            for (int i = 0; i < this.Issues.Count; i++)
            {
                IssueBindingItem issue = this.Issues[i];
                for (int j = 0; j < issue.Cells.Count; j++)
                {
                    Cell cell = issue.Cells[j];
                    if (cell == null)
                        continue;
                    if (cell.item == null)
                        continue;
                    if (cell.item.Deleted == true)
                        continue;

                    if (this.ParentItems.IndexOf(cell.item) != -1)
                        continue;   // 跳过合订本

                    if (cell.item.ParentItem != null)
                    {
                        // 成员册
                        string strXmlFragment = "";
                        // 创建作为成员册的<binding>元素内片断
                        // 仅仅创建一个<bindingParent>元素
                        int nRet = cell.item.BuildMyselfBindingXmlString(
                            out strXmlFragment,
                            out strError);
                        if (nRet == -1)
                            return -1;
                        if (cell.item.Binding != strXmlFragment)
                        {
                            cell.item.Binding = strXmlFragment;
                            cell.item.Changed = true;
                        }
                    }
                    else
                    {
                        // 单册
                        // 清除bindingxml
                        if (String.IsNullOrEmpty(cell.item.Binding) == false)
                        {
                            cell.item.Binding = "";
                            cell.item.Changed = true;
                        }
                    }
                }
            }

            return 0;
        }

        // 内容宽度要变化
        void AfterWidthChanged(bool bAlwaysInvalidate)
        {
            int nOldMaxCells = this.m_nMaxItemCountOfOneIssue;
            bool bChanged = false;

            // 整个内容区域的高度
            long lNewHeight = this.m_nCellHeight * this.Issues.Count;
            if (lNewHeight != this.m_lContentHeight)
            {
                this.m_lContentHeight = lNewHeight;
                bChanged = true;
            }

            int nMaxCells = 0;
            for (int i = 0; i < this.Issues.Count; i++)
            {
                IssueBindingItem issue = this.Issues[i];
                issue.RemoveTailNullCell(); // 2010/2/20 

                if (nMaxCells < issue.Cells.Count)
                    nMaxCells = issue.Cells.Count;
            }

            if (nMaxCells != nOldMaxCells)
                bChanged = true;

            if (bChanged == true)
            {
                this.m_nMaxItemCountOfOneIssue = nMaxCells;

                SetContentWidth();

                try
                {
                    SetScrollBars(ScrollBarMember.Both);
                }
                catch
                {
                }
                // 刷新显示
                this.Invalidate();
            }
            else
            {
                if (bAlwaysInvalidate == true)
                    this.Invalidate();
            }
        }

#if OLD_INITIAL
        // *** 为初始化服务
        // 初始化期间，追加一个合订册对象
        public ItemBindingItem AppendBindItem(string strXml,
            out string strError)
        {
            strError = "";

            ItemBindingItem item = new ItemBindingItem();
            item.Container = null;
            item.RecPath = "";

            int nRet = item.Initial(strXml, out strError);
            if (nRet == -1)
                return null;

            this.ParentItems.Add(item);
            return item;
        }

        // *** 为初始化服务
        // 初始化期间，追加一个期对象
        public IssueBindingItem AppendIssue(string strXml,
            out string strError)
        {
            strError = "";

            IssueBindingItem issue = new IssueBindingItem();

            issue.Container = this;
            int nRet = issue.Initial(strXml, 
                true,
                out strError);
            if (nRet == -1)
                return null;

            this.Issues.Add(issue);

            this.m_lContentHeight = this.m_nCellHeight * this.Issues.Count;

            return issue;
        }

         // *** 为初始化服务
       // 初始化期间，追加一系列无所属期对象的单册对象
        public int AppendNoneIssueSingleItems(List<string> XmlRecords,
            out string strError)
        {
            strError = "";

            this.NoneIssueItems.Clear();
            for (int i = 0; i < XmlRecords.Count; i++)
            {
                string strXml = XmlRecords[i];
                if (String.IsNullOrEmpty(strXml) == true)
                {
                    Debug.Assert(false, "");
                    continue;
                }

                ItemBindingItem item = new ItemBindingItem();
                int nRet = item.Initial(strXml, out strError);
                if (nRet == -1)
                    return -1;

                item.Container = null;
                this.NoneIssueItems.Add(item);
            }

            return 0;
        }
#endif

        void InitialMaxItemCount()
        {
            if (this.m_nMaxItemCountOfOneIssue != -1)
                return; // 已经初始化

            int nMaxCount = 0;
            for (int i = 0; i < this.Issues.Count; i++)
            {
                IssueBindingItem issue = this.Issues[i];

                int nCount = issue.Items.Count;

                if (nCount > nMaxCount)
                    nMaxCount = nCount;
            }

            this.m_nMaxItemCountOfOneIssue = nMaxCount;

            SetContentWidth();
        }

        void SetContentWidth()
        {
            Debug.Assert(this.m_nMaxItemCountOfOneIssue != -1, "");
            // 整个内容区域的宽度
            this.m_lContentWidth = m_nCoverImageWidth + m_nLeftTextWidth + (this.m_nMaxItemCountOfOneIssue * m_nCellWidth);
        }

#if OLD_INITIAL
        // 是否已经挂接的GetItemInfo事件
        public bool HasGetItemInfo()
        {
            if (this.GetItemInfo == null)
                return false;

            return true;
        }
#endif

        #region 图形相关的函数

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // InitialMaxItemCount();  // 初始化期内最大事项数

            // 首次显示前, OnSizeChanged()一次也没有被调用前, 显示好卷滚条
            SetScrollBars(ScrollBarMember.Both);
        }

        protected override void OnSizeChanged(System.EventArgs e)
        {
            try
            {
                SetScrollBars(ScrollBarMember.Both);
            }
            catch
            {
            }

            // 如果client区域足够大，调整org，避免看不见某部分
            DocumentOrgY = DocumentOrgY;
            DocumentOrgX = DocumentOrgX;

            base.OnSizeChanged(e);
        }


        // 点击测试
        // parameters:
        //      p_x 点击位置x。为屏幕坐标
        //      type    要测试的最下级（叶级）对象的类型。如果为null，表示一直到末端
        void HitTest(long p_x,
            long p_y,
            Type dest_type,
            out HitTestResult result)
        {
            result = new HitTestResult();

            bool bIsRequiredType = false;

            if (dest_type == typeof(BindingControl))
                bIsRequiredType = true;

            // 换算为整体文档(包含上下左右的空白区域)坐标
            long x = p_x - m_lWindowOrgX;
            long y = p_y - m_lWindowOrgY;

            if (y < this.m_nTopBlank)
                result.AreaPortion = AreaPortion.TopBlank;  // 上方空白
            else if (y > this.m_nTopBlank + this.m_lContentHeight)
                result.AreaPortion = AreaPortion.BottomBlank;  // 下方空白
            else if ((dest_type == null || bIsRequiredType == true) // 如果末级类型有特殊要求，则左右空白都算作下级对象的范围
                && x < this.m_nLeftBlank)
                result.AreaPortion = AreaPortion.LeftBlank;  // 左方空白
            else if ((dest_type == null || bIsRequiredType == true)
                && x > this.m_nLeftBlank + this.m_lContentWidth)
                result.AreaPortion = AreaPortion.RightBlank;  // 右方空白
            else
            {
                if (dest_type == typeof(BindingControl))
                {
                    result.AreaPortion = AreaPortion.Content;
                    goto END1;
                }

                /*
                long xOffset = m_lWindowOrgX + m_nLeftBlank;
                long yOffset = m_lWindowOrgY + m_nTopBlank;

                long x = p.X - xOffset;
                long y = p.Y - yOffset;
                 * */

                x -= this.m_nLeftBlank;
                y -= this.m_nTopBlank;

                long y0 = 0;
                for (int i = 0; i < this.Issues.Count; i++)
                {
                    IssueBindingItem issue = this.Issues[i];

                    if (y >= y0 && y < y0 + this.m_nCellHeight)
                    {
                        issue.HitTest(x,
                            y - y0,
                            dest_type,
                            out result);
                        return;
                    }

                    y0 += this.m_nCellHeight;
                }

                // 实在无法匹配
                result.AreaPortion = AreaPortion.Content;
                goto END1;
            }

        END1:
            result.X = x;
            result.Y = y;
            result.Object = null;
        }

        /*
        static bool PtInRect(int x,
    int y,
    Rectangle rect)
        {
            if (x < rect.X)
                return false;
            if (x >= rect.Right)
                return false;
            if (y < rect.Y)
                return false;
            if (y >= rect.Bottom)
                return false;
            return true;
        }*/

        void BeginDraging()
        {
            this.m_bDraging = true;
            this.Cursor = Cursors.NoMove2D;
        }

        void EndDraging()
        {
            this.m_bDraging = false;
            this.Cursor = Cursors.Arrow;
        }

        bool _changingCoverImageWidth = false;
        int _endX = 0;
        int _startX = 0;

        void BeginChangeCoverImageWidth()
        {
            _changingCoverImageWidth = true;
            this.Cursor = Cursors.SizeWE;
        }

        void EndChangeCoverImageWidth()
        {
            _changingCoverImageWidth = false;
            this.Cursor = Cursors.Arrow;
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (m_bRectSelecting == true)
            {
                // Debug.Assert(false, "");
                DoEndRectSelecting();
            }

            if (GuiUtil.PtInRect(e.X, e.Y, this.ClientRectangle) == false)
            {
                // 防止在卷滚条上单击后拖动造成副作用
                goto END1;
            }

            this.Capture = true;

            this.Focus();

            if (e.Button == MouseButtons.Left)
            {
                bool bControl = (Control.ModifierKeys == Keys.Control);
                bool bShift = (Control.ModifierKeys == Keys.Shift);

                HitTestResult result = null;
                // 屏幕坐标
                this.HitTest(
                    e.X,
                    e.Y,
                    null,
                    out result);
                if (result == null)
                    goto END1;

                this.DragStartMousePosition = e.Location;

                if (result.AreaPortion == AreaPortion.Grab)
                {
                    // 拖拽对象，而不是rect围选
                    this.BeginDraging();
                    this.DragStartObject = (Cell)result.Object;
                    goto END1;
                }

                if (result.AreaPortion == AreaPortion.CoverImageEdge)
                {
                    this.BeginChangeCoverImageWidth();
                    this._startX = e.X;
                    this._endX = e.X;
                    goto END1;
                }

                if (result.Object is CellBase)  // new changed 2010/2/26
                    this.FocusObject = (CellBase)result.Object;

                /*
                if (result.Object is Cell)
                {
                    bool bCheckBox = ShouldDisplayCheckBox((Cell)result.Object);
                    if (bCheckBox == true
                        && result.AreaPortion == AreaPortion.CheckBox)
                    {
                        CellChecked((Cell)result.Object);
                        goto END1;
                    }
                }
                 * */

                // 清除以前的选择
                if (bControl == false && bShift == false)   // 按下了SHIFT，也不清除以前的
                {
                    if (m_bSelectedAreaOverflowed == false)
                    {
                        SelectObjects(this.m_aSelectedArea, SelectAction.Off);
                        ClearSelectedArea();
                    }
                    else
                    {
                        // 只好采用遍历的方法来全部清除
                        /*
                         * // 这个方法屏幕要抖动
                        this.DataRoot.ClearAllSubSelected();
                        this.Invalidate();
                         * */
                        List<CellBase> objects = new List<CellBase>();
                        this.ClearAllSubSelected(ref objects, 100);
                        if (objects.Count >= 100)
                            this.Invalidate();
                        else
                        {
                            // 这个方法屏幕不抖动
                            UpdateObjects(objects);
                        }
                    }
                }

                // 确保点到的对象全部进入视野
                if (result.Object != null
                    && result.Object is CellBase)
                {
                    if (EnsureVisible((CellBase)result.Object) == true)
                        this.Update();
                }

                // 矩形选择开始
                if (m_bRectSelectMode == true
                    && e.Button == MouseButtons.Left)
                {
                    this.m_DragStartPointOnDoc = new PointF(e.X - m_lWindowOrgX,
                        e.Y - m_lWindowOrgY);
                    this.m_DragCurrentPointOnDoc = m_DragStartPointOnDoc;
                    m_bRectSelecting = true;
                    goto END1;
                }

                if (result.Object != null
                    && result.Object is CellBase)   // 
                {
                    // 
                    // 单独刷新一个
                    List<CellBase> temp = new List<CellBase>();
                    temp.Add((CellBase)result.Object);
                    if (bControl == true)
                    {
                        SelectObjects(temp, SelectAction.Toggle);
                    }
                    else
                    {
                        SelectObjects(temp, SelectAction.On);
                    }

                    if (EnsureVisible((CellBase)result.Object) == true)
                        this.Update();

                    /*
                    ShowTip(result.Object, e.Location, false);
                     * */
                }
                // this.Update();
            }

        END1:
            base.OnMouseDown(e);
        }

        void CellChecked(Cell cell)
        {
            string strError = "";
            int nRet = 0;

            Debug.Assert(this.WholeLayout != "binding", "");
            Debug.Assert(cell.item != null, "");

            // 2010/9/21 add
            if (String.IsNullOrEmpty(cell.Container.PublishTime) == true)
            {
                strError = "自由期的格子不能进行记到";
                goto ERROR1;
            }

            if (cell.item.Locked == true)
            {
                strError = "格子状态为锁定时 不允许进行记到或者撤销记到的操作";
                goto ERROR1;
            }

            if (cell.item.Calculated == true)
            {
                nRet = cell.item.DoAccept(out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            else if (cell.item.OrderInfoPosition.X != -1
                && cell.item.NewCreated == true)
            {
                nRet = cell.item.DoUnaccept(out strError);
                if (nRet == -1)
                    goto ERROR1;
            }
            else
            {
                strError = "格子状态 不适合进行记到或者撤销记到的操作";
                goto ERROR1;
            }

            // 从单元格子变化为所从属的期格子
            IssueBindingItem issue = cell.Container;

            this.UpdateObject(issue);

            // 刷新编辑区域
            if (cell == this.FocusObject)
            {
                // Cell focus_obejct = (Cell)this.FocusObject;
                if (this.CellFocusChanged != null)
                {
                    FocusChangedEventArgs e1 = new FocusChangedEventArgs();
                    e1.OldFocusObject = this.FocusObject;
                    e1.NewFocusObject = this.FocusObject;
                    this.CellFocusChanged(this, e1);
                }
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        protected override void OnMouseHover(EventArgs e)
        {
            if (m_bRectSelecting == true)
                return;

            if (this._changingCoverImageWidth == true)
                return;

            HitTestResult result = null;

            Point p = this.PointToClient(Control.MousePosition);

            // Debug.WriteLine("hover=" + p.ToString());

            bool bCheckBox = false;

            // 屏幕坐标
            this.HitTest(
                p.X,
                p.Y,
                typeof(NullCell),   // null
                out result);
            if (result == null)
                goto END1;

            if (result.AreaPortion == AreaPortion.CoverImageEdge)
                this.Cursor = Cursors.SizeWE;
            else
                this.Cursor = Cursors.Arrow;

            // 只关注Cell类型
            if (result.Object != null)
            {
                if (result.Object.GetType() != typeof(Cell)
                    && result.Object.GetType() != typeof(GroupCell)
                    && result.Object.GetType() != typeof(NullCell))
                {
                    result.Object = null;
                }

                if (this.m_bDraging == true)
                {
                    this.FocusObject = (CellBase)result.Object;
                }

                if (this.WholeLayout != "binding"
                    && result.AreaPortion == AreaPortion.CheckBox)
                {
                    if (result.Object is Cell)
                    {
                        Cell cell = (Cell)result.Object;
                        if (cell != null)
                        {
                            bCheckBox = ShouldDisplayCheckBox(cell);
                            bool bOld = cell.m_bDisplayCheckBox;
                            cell.m_bDisplayCheckBox = bCheckBox;
                            if (cell.Selected == true && bOld != bCheckBox)
                            {
                                this.UpdateObjectHover(cell);    // 促使改变
                            }
                        }
                    }
                }

                if (result.AreaPortion != AreaPortion.Grab
                    && result.Object != null)
                {
                    if (result.Object is Cell
                        || result.Object is GroupCell)
                    {
                        Cell cell = (Cell)result.Object;

                        if (cell != null)
                        {
                            if (this.WholeLayout != "binding")
                            {
                                bCheckBox = ShouldDisplayCheckBox(cell);
                                if (bCheckBox == true)
                                {
                                    if (result.AreaPortion != AreaPortion.CheckBox)
                                        bCheckBox = false;
                                }

                                cell.m_bDisplayCheckBox = bCheckBox;
                            }

                            // unselected cell的grab以外部分被认为等同于off
                            if (cell.Selected == false && bCheckBox == false)
                                result.Object = null;
                        }
                    }
                    if (result.Object is NullCell)
                    {
                        result.Object = null;
                    }
                }
            }

            if (this.m_bDraging == true)
                return;

            this.HoverObject = (Cell)result.Object;
        END1:
            base.OnMouseHover(e);
        }

        /*
        // 测试是否点击到了Cell中央的checkbox部位
        // parameters:
        //      x   格子内部坐标
        bool HitCheckBox(int x,
            int y)
        {
            int nCenterX = this.m_nCellWidth / 2;
            int nCenterY = this.m_nCellHeight / 2;
            int nWidth = this.m_rectGrab.Width;
            Rectangle rectCheckBox = new Rectangle(
                nCenterX - nWidth / 2,
                nCenterY - nWidth / 2,
                nWidth,
                nWidth);
            if (GuiUtil.PtInRect(x, y,
                rectCheckBox) == true)
                return true;

            return false;
        }
         * */

        // 一个格子是否要(浮动)显示checkbox?
        static bool ShouldDisplayCheckBox(Cell cell)
        {
            if (cell is GroupCell)
                return false;

            bool bCheckBox = false;
            if (cell.item != null)
            {
                if (cell.item.Locked == true)
                    return false;   // 锁定状态的格子不显示checkbox

                if (cell.item.OrderInfoPosition.X != -1
                    && cell.item.NewCreated == true)
                    bCheckBox = true;
                else if (cell.item.Calculated == true)
                    bCheckBox = true;
            }

            return bCheckBox;
        }

        public Cell HoverObject
        {
            get
            {
                return this.m_lastHoverObj;
            }
            set
            {
                if (this.m_lastHoverObj == value)
                    return;

                if (this.m_lastHoverObj != null
                    && this.m_lastHoverObj.m_bHover == true)
                {
                    this.m_lastHoverObj.m_bHover = false;
                    if (this.m_lastFocusObj is Cell)
                        ((Cell)this.m_lastFocusObj).m_bDisplayCheckBox = false;
                    this.UpdateObjectHover(this.m_lastHoverObj);
                }

                this.m_lastHoverObj = value;

                if (this.m_lastHoverObj != null
                    && this.m_lastHoverObj.m_bHover == false)
                {
                    this.m_lastHoverObj.m_bHover = true;
                    this.UpdateObjectHover(this.m_lastHoverObj);
                }
            }
        }

        public CellBase FocusObject
        {
            get
            {
                return this.m_lastFocusObj;
            }
            set
            {
                if (value == null
                    || value is Cell
                    || value is NullCell
                    || value is IssueBindingItem)
                {
                }
                else
                {
                    throw new Exception("FocusObject必须为类型Cell/NullCell/IssueBindingItem之一");
                }

                if (this.m_lastFocusObj == value)
                    return;

                if (value is NullCell
                    && this.m_lastFocusObj is NullCell)
                {
                    // 虽然已有和即将设置的对象不同，但是指向的位置和状态完全相同
                    NullCell new_cell = (NullCell)value;
                    NullCell exist_cell = (NullCell)this.m_lastFocusObj;
                    if (IsEqual(new_cell, exist_cell) == true)
                        return;
                }


                if (this.m_lastFocusObj != null
                    && this.m_lastFocusObj.m_bFocus == true)
                {
                    this.m_lastFocusObj.m_bFocus = false;
                    this.UpdateObject(this.m_lastFocusObj);
                }

                object oldObject = this.m_lastFocusObj;

                this.m_lastFocusObj = value;

                if (this.m_lastFocusObj != null
                    && this.m_lastFocusObj.m_bFocus == false)
                {
                    this.m_lastFocusObj.m_bFocus = true;
                    this.UpdateObject(this.m_lastFocusObj);
                }

                if (this.CellFocusChanged != null)
                {
                    FocusChangedEventArgs e = new FocusChangedEventArgs();
                    e.OldFocusObject = oldObject;
                    e.NewFocusObject = value;
                    this.CellFocusChanged(this, e);
                }
            }
        }

        static bool IsEqual(NullCell cell1, NullCell cell2)
        {
            if (cell1 == cell2)
                return true;

            if (cell1.X == cell2.X
                && cell1.Y == cell2.Y)
                return true;

            return false;
        }

        static bool IsEqual(CellBase cell1, CellBase cell2)
        {
            if (cell1 == cell2)
                return true;

            if (cell1 is NullCell && cell2 is NullCell)
            {
                return IsEqual((NullCell)cell1, (NullCell)cell2);
            }

            return false;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            OnMouseHover(null);

            if (this._changingCoverImageWidth == true
                && e.Button == System.Windows.Forms.MouseButtons.Left)
            {
                this._endX = e.X;
                goto END1;
            }

            // 拖动时可以自动卷滚
            if (this.m_bDraging == true
                && this.Capture == true
                && e.Button == MouseButtons.Left)
            {
                // 拖动鼠标
                if (IsNearestPoint(this.DragStartMousePosition, e.Location) == true)
                {
                    // 防止在原地就被当作鼠标拖动
                    goto END1;
                }

                // 为了能卷滚
                HitTestResult result = null;

                // 屏幕坐标
                this.HitTest(
                    e.X,
                    e.Y,
                    typeof(NullCell),   // null
                    out result);
                if (result == null)
                    goto END1;

                if (result.Object == null)
                    goto END1;

                {
                    // 确保可见
                    if (EnsureVisibleWhenScrolling(result) == true)
                        this.Update();
                }

                if (result.Object.GetType() != typeof(Cell)
                    && result.Object.GetType() != typeof(NullCell))
                    goto END1;

                /*
                if (EnsureVisibleWhenScrolling((CellBase)result.Object) == true)
                    this.Update();
                 * */

                if (GuiUtil.PtInRect(e.X, e.Y, this.ClientRectangle) == false)
                    this.timer_dragScroll.Start();
                else
                    this.timer_dragScroll.Stop();
                goto END1;
            }

            // 围选
            if (this.Capture == true
                && e.Button == MouseButtons.Left)
            {

                // 拖动鼠标
                if (IsNearestPoint(this.DragStartMousePosition, e.Location) == true)
                {
                    // 防止在原地就被当作鼠标拖动
                    goto END1;
                }

                HitTestResult result = null;

                if (m_bRectSelecting == true
                    && e.Button == MouseButtons.Left
                    )
                {
                    // 清除上次虚框图像
                    DrawSelectRect(true);

                    // 画出这次的虚框图像
                    this.m_DragCurrentPointOnDoc = new PointF(e.X - m_lWindowOrgX,
                        e.Y - m_lWindowOrgY);
                    DrawSelectRect(true);

                    // 为了能卷滚
                    {
                        /*
                        Type objType = typeof(Cell);

                        if (this.DragStartObject != null)
                            objType = this.DragStartObject.GetType();
                         * */

                        result = null;

                        // 屏幕坐标
                        this.HitTest(
                            e.X,
                            e.Y,
                            typeof(NullCell),   // null
                            out result);
                        if (result == null)
                            goto END1;

                        if (result.Object == null)
                            goto END1;

                        {
                            // 清除
                            DrawSelectRect(true);
                            if (EnsureVisibleWhenScrolling(result) == true)
                                this.Update();

                            // 重画
                            DrawSelectRect(true);
                        }

                        if (result.Object.GetType() != typeof(Cell)
                            && result.Object.GetType() != typeof(NullCell))
                            goto END1;

                        // 补上
                        if (this.DragStartObject == null)
                            this.DragStartObject = (CellBase)result.Object;

                        /*
                        if (IsEqual((CellBase)this.DragLastEndObject,
                            (CellBase)result.Object) == false)
                        {
                            // 清除
                            DrawSelectRect(true);
                            if (EnsureVisibleWhenScrolling((CellBase)result.Object) == true)
                                this.Update();

                            // 重画
                            DrawSelectRect(true);

                            // if (result.Object is Cell)
                                this.DragLastEndObject = (CellBase)result.Object;
                        }
                         * */
                        if (IsEqual((CellBase)this.DragLastEndObject,
    (CellBase)result.Object) == false)
                            this.DragLastEndObject = (CellBase)result.Object;

                        if (GuiUtil.PtInRect(e.X, e.Y, this.ClientRectangle) == false)
                            this.timer_dragScroll.Start();
                        else
                            this.timer_dragScroll.Stop();
                    }

                    goto END1;
                }


                if (m_bRectSelecting == true)
                    goto END1;

#if NOOOOOOOOOOOOOOOOOO
                // 找到当前鼠标下的对象。必须是和DragStartObject同一级的对象
                if (this.DragStartObject == null)
                    goto END1;


                bool bControl = (Control.ModifierKeys == Keys.Control);
                bool bShift = (Control.ModifierKeys == Keys.Shift);

                // 屏幕坐标
                this.HitTest(
                    e.X,
                    e.Y,
                    this.DragStartObject.GetType(),
                    out result);
                if (result == null)
                    goto END1;

                if (result.Object == null)
                    goto END1;

                /*
                String tipText = String.Format("   {0}", result.Object.FullName);
                trackTip.Show(tipText, this, e.Location);
                 * */
                // ShowTip(result.Object, e.Location, false);

                if (result.Object.GetType() != this.DragStartObject.GetType())
                    goto END1;

                /*
                if (this.DragLastEndObject == null
                    && result.Object == this.DragStartObject)
                {
                    this.DragLastEndObject = result.Object;
                    goto END1;
                }*/

                if (result.Object == this.DragLastEndObject)
                {
                    if (EnsureVisibleWhenScrolling((Cell)result.Object) == true)
                        this.Update();
                    goto END1;
                }

                // 方法1
                // 从this.DragStartObject 到 DragCurrentObject 之间清除
                // 然后 DragCurrentObject 到 result.Object之间，选上
                // 这个方法速度慢

                // 方法2
                // Current到 result.Object之间，toggle；然后，在留意把Start选上

                List<Cell> objects = null;
                if (this.DragLastEndObject == null) // 第一次的特殊情况
                {
                    // this.SetObjectFocus(this.DragStartObject, false);

                    objects = GetRangeObjects(
                        true,
                        true,
                        this.DragStartObject, result.Object);

                }
                else
                {
                    // B C之间的方向
                    this.m_nDirectionBC = GetDirection(this.DragLastEndObject, result.Object);

                    Debug.Assert(this.m_nDirectionBC != 0, "B C两个对象，不能相同");

                    // 如果 A-B B-C同向， 则不包含头部，包含尾部
                    if (this.m_nDirectionAB == 0 // 首次特殊情况
                        || this.m_nDirectionAB == this.m_nDirectionBC)
                    {
                        objects = GetRangeObjects(
                            false,
                            true,
                            this.DragLastEndObject,
                            result.Object);
                    }
                    else
                    {
                        // 如果 A-B B-C不同向， 则包含头部，不包含尾部
                        objects = GetRangeObjects(
                            true,
                            false,
                            this.DragLastEndObject,
                            result.Object);
                    }
                }

                SelectObjects(objects, SelectAction.Toggle);

                {
                    // 追加选上原始头部
                    List<AreaBase> temp = new List<AreaBase>();
                    temp.Add(this.DragStartObject);
                    temp.Add(result.Object);    // C也保险
                    SelectObjects(temp, SelectAction.On);
                }

                // this.SetObjectFocus(this.DragLastEndObject, false);


                this.DragLastEndObject = result.Object;

                // this.SetObjectFocus(this.DragLastEndObject, true);


                // A B 之间的方向
                this.m_nDirectionAB = GetDirection(this.DragStartObject, this.DragLastEndObject);

                if (EnsureVisibleWhenScrolling(result.Object) == true)
                    this.Update();


                /*
                String tipText = String.Format("({0}, {1}) rect={2}", e.X, e.Y, this.ClientRectangle.ToString());
                trackTip.Show(tipText, this, e.Location);
                 */

                if (PtInRect(e.X, e.Y, this.ClientRectangle) == false)
                {
                    this.timer_dragScroll.Start();
                    // this.mouseMoveArgs = e;
                }
                else
                {
                    this.timer_dragScroll.Stop();
                }

#endif
                goto END1;
            }

            if (this.Capture == false)
            {
                if (IsNearestPoint(this.DragStartMousePosition, e.Location) == false)
                {
                    // 防止在原地的多余一次MouseMove消息隐藏tip窗口
                    trackTip.Hide(this);
                }
            }

        END1:
            // Call MyBase.OnMouseHover to activate the delegate.
            base.OnMouseMove(e);
        }

        // >0 则暂时禁止checkbox功能
        int m_nDisableCheckBox = 0;

        protected override void OnMouseUp(MouseEventArgs e)
        {
            this.Capture = false;

            this.timer_dragScroll.Stop();

            // 修改封面图像栏宽度的结束处理
            if (this._changingCoverImageWidth == true
                && e.Button == MouseButtons.Left)
            {
                this.EndChangeCoverImageWidth();
                this._endX = e.X;
                int delta = this._endX - this._startX;
                ChangeCoverImageColumnWidth(delta);
                goto END1;
            }

            // 拖拽的结束处理
            if (this.m_bDraging == true
                && e.Button == MouseButtons.Left)
            {

                this.EndDraging();
                this.DragLastEndObject = this.FocusObject;

                // 拖动鼠标
                if (IsNearestPoint(this.DragStartMousePosition, e.Location) == true)
                {
                    // 仅仅在原地click down up

                }
                else
                {
                    if (this.DragStartObject != this.DragLastEndObject)
                    {
                        // MessageBox.Show(this, "拖动进入");
                        this.DoDragEndFunction();
                        goto END1;
                    }
                }
            }

            // checkbox
            if (this.WholeLayout != "binding"
                    && m_bRectSelecting == true
                    && m_nDisableCheckBox == 0
                && e.Button == MouseButtons.Left
                && IsNearestPoint(this.DragStartMousePosition, e.Location) == true)
            {
                HitTestResult result = null;
                // 屏幕坐标
                this.HitTest(
                    e.X,
                    e.Y,
                    null,
                    out result);
                if (result != null && result.Object is Cell)
                {
                    bool bCheckBox = ShouldDisplayCheckBox((Cell)result.Object);
                    if (bCheckBox == true
                        && result.AreaPortion == AreaPortion.CheckBox)
                    {
                        CellChecked((Cell)result.Object);
                    }
                }
            }

            // 拖动矩形框围选的结束处理
            if (m_bRectSelecting == true
                && e.Button == MouseButtons.Left)
            {
                DoEndRectSelecting();
            }

            if (e.Button == MouseButtons.Right)
            {
                PopupMenu(e.Location);
                goto END1;
            }

        END1:
            base.OnMouseUp(e);
        }


        // 鼠标滚轮
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            int numberOfTextLinesToMove = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
            int numberOfPixelsToMove = numberOfTextLinesToMove * 18;

            DocumentOrgY += numberOfPixelsToMove;

            // base.OnMouseWheel(e);
        }

        // 鼠标双击
        protected override void OnMouseDoubleClick(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (this.FocusObject is IssueBindingItem)
                {
                    menuItem_modifyIssue_Click(this, e);
                    goto END1;
                }

                if (this.FocusObject is Cell)
                {
                    // 显示编辑区域
                    {
                        EditAreaEventArgs e1 = new EditAreaEventArgs();
                        e1.Action = "get_state";
                        this.EditArea(this, e1);
                        if (e1.Result == "visible")
                            goto END1;
                    }

                    {
                        EditAreaEventArgs e1 = new EditAreaEventArgs();
                        e1.Action = "open";
                        this.EditArea(this, e1);
                    }

                    goto END1;
                }
            }

        END1:
            base.OnMouseDoubleClick(e);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                // 菜单键
                case Keys.Apps:
                    {
                        Point p;
                        if (m_lastFocusObj != null)
                        {
                            RectangleF rect = GetViewRect(this.m_lastFocusObj);
                            p = new Point((int)rect.Right, (int)rect.Bottom);
                        }
                        else
                        {
                            p = this.PointToClient(Control.MousePosition);
                        }

                        PopupMenu(p);
                        break;
                    }
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    {
                        DoArrowLeftRight(e.KeyCode);
                    }
                    break;
                case Keys.PageDown:
                case Keys.PageUp:
                    {
                        if (this.m_lastFocusObj == null)
                        {
                            AutoSetFocusObject();
                            if (this.m_lastFocusObj == null)
                                break;
                        }

                        if (this.m_lastFocusObj != null
                            && this.EnsureVisible(this.m_lastFocusObj) == true)
                            this.Update();

                        // 获得当前焦点格子的中心所在的窗口坐标
                        RectangleF rect = this.GetViewRect(this.m_lastFocusObj);

                        if (e.KeyCode == Keys.PageDown)
                            this.DocumentOrgY -= this.ClientSize.Height;
                        else
                            this.DocumentOrgY += this.ClientSize.Height;

                        // 翻页前同样的窗口坐标位置，模拟点一下鼠标
                        MouseEventArgs e1 = new MouseEventArgs(MouseButtons.Left,
                            1,
                            (int)(rect.X + rect.Width / 2),
                            (int)(rect.Y + rect.Height / 2),
                            0);

                        // TODO: 点到中心位置，可能会导致记到。要防止 2011/8/4

                        // 点到星期标题上怎么办？似乎还需要更好的解决办法。
                        // 可以估算当前客户区高度相当于多少行，然后直接把焦点
                        // 竖向移动这么多行，卷入视线范围，即可。
                        this.m_nDisableCheckBox++;

                        this.OnMouseDown(e1);
                        this.OnMouseUp(e1);

                        this.m_nDisableCheckBox--;

                        if (this.m_lastFocusObj != null
                            && this.EnsureVisible(this.m_lastFocusObj) == true)
                            this.Update();

                    }
                    break;
            }

            base.OnKeyDown(e);
        }

        void GetCellXY(CellBase cell,
            out int x,
            out int y)
        {
            x = -2;
            y = -1;
            if (cell is NullCell)
            {
                NullCell null_cell = (NullCell)cell;
                x = null_cell.X;
                y = null_cell.Y;
            }
            else if (cell is Cell)
            {
                Cell normal_cell = (Cell)cell;
                x = normal_cell.Container.Cells.IndexOf(normal_cell);
                y = this.Issues.IndexOf(normal_cell.Container);
            }
            else if (cell is IssueBindingItem)
            {
                IssueBindingItem issue = (IssueBindingItem)cell;
                x = -1;
                y = this.Issues.IndexOf(issue);
            }
            else
            {
                Debug.Assert(false, "");
            }
        }

        // parameters:
        //      bCross  是否允许跨越期、册边界
        CellBase GetLeftCell(CellBase cell,
            bool bCross)
        {
            int x = -2;
            int y = -1;
            GetCellXY(cell,
                out x,
                out y);

            if (x < 0)
                return null;

            if (bCross == false)
            {
                if (x == 0)
                    return null;
            }
            else
            {
                if (x == 0)
                    return this.Issues[y];
            }

            x--;
            Debug.Assert(x >= 0, "");

            IssueBindingItem issue = this.Issues[y];
            if (issue.Cells.Count <= x
                || issue.Cells[x] == null)
                return new NullCell(x, y);

            return issue.Cells[x];
        }


        // parameters:
        //      bCross  是否允许跨越期、册边界
        CellBase GetRightCell(CellBase cell,
            bool bCross)
        {
            int x = -2;
            int y = -1;
            GetCellXY(cell,
                out x,
                out y);

            if (bCross == false)
            {
                if (x < 0)
                    return null;
            }

            if (x >= this.m_nMaxItemCountOfOneIssue - 1)
                return null;

            x++;
            Debug.Assert(x >= 0, "");
            IssueBindingItem issue = this.Issues[y];
            if (issue.Cells.Count <= x
                || issue.Cells[x] == null)
                return new NullCell(x, y);

            return issue.Cells[x];
        }

        CellBase GetUpCell(CellBase cell)
        {
            int x = -2;
            int y = -1;
            GetCellXY(cell,
                out x,
                out y);

            if (y < 0)
                return null;

            if (y == 0)
                return null;

            y--;

            IssueBindingItem issue = this.Issues[y];

            if (x == -1)
                return issue;

            if (issue.Cells.Count <= x
                || issue.Cells[x] == null)
                return new NullCell(x, y);

            return issue.Cells[x];
        }
        CellBase GetDownCell(CellBase cell)
        {
            int x = -2;
            int y = -1;
            GetCellXY(cell,
                out x,
                out y);

            if (y < 0)
                return null;

            if (y >= this.Issues.Count - 1)
                return null;

            y++;
            IssueBindingItem issue = this.Issues[y];

            if (x == -1)
                return issue;

            if (issue.Cells.Count <= x
                || issue.Cells[x] == null)
                return new NullCell(x, y);

            return issue.Cells[x];
        }

        // 2011/8/4
        // 当没有焦点对象时，设置一个焦点对象
        bool AutoSetFocusObject()
        {
            if (m_lastFocusObj == null)
            {
                // 自动把第一个格子设为焦点
                if (this.Issues.Count > 0)
                {
                    m_lastFocusObj = this.Issues[0].GetFirstCell();
                    SetObjectFocus(m_lastFocusObj);
                }
                return true;
            }

            return false;
        }

        // 上下左右方向键
        void DoArrowLeftRight(Keys key)
        {
            if (m_lastFocusObj == null)
            {
                // 自动把第一个格子设为焦点
                if (this.Issues.Count > 0)
                {
                    m_lastFocusObj = this.Issues[0].GetFirstCell();
                    SetObjectFocus(m_lastFocusObj);
                }
                return;
            }

            CellBase obj = null;

            bool bControl = (Control.ModifierKeys == Keys.Control);
            bool bShift = (Control.ModifierKeys == Keys.Shift);

            if (bControl == true || bShift == true)
            {
                if (key == Keys.Left)
                    obj = GetLeftCell(m_lastFocusObj, false);
                else if (key == Keys.Right)
                    obj = GetRightCell(m_lastFocusObj, false);
                else if (key == Keys.Up)
                    obj = GetUpCell(m_lastFocusObj);
                else if (key == Keys.Down)
                    obj = GetDownCell(m_lastFocusObj);
            }
            else
            {
                if (key == Keys.Left)
                    obj = GetLeftCell(m_lastFocusObj, true);
                else if (key == Keys.Right)
                    obj = GetRightCell(m_lastFocusObj, true);
                else if (key == Keys.Up)
                    obj = GetUpCell(m_lastFocusObj);
                else if (key == Keys.Down)
                    obj = GetDownCell(m_lastFocusObj);
            }

            if (obj != null)
            {
                // 清除以前的选择
                if (bControl == false && bShift == false)   // 按下了SHIFT，也不清除以前的
                {
                    if (m_bSelectedAreaOverflowed == false)
                    {
                        SelectObjects(this.m_aSelectedArea, SelectAction.Off);
                        ClearSelectedArea();
                    }
                    else
                    {
                        // 只好采用遍历的方法来全部清除
                        List<CellBase> objects = new List<CellBase>();
                        this.ClearAllSubSelected(ref objects, 100);
                        if (objects.Count >= 100)
                            this.Invalidate();
                        else
                        {
                            // 这个方法屏幕不抖动
                            UpdateObjects(objects);
                        }
                    }
                }
                else
                {
                    // 按下了Ctrl或者Shift的情况
                    /*
                    if (obj.GetType() != this.DragStartObject.GetType())
                        return;
                     * */

                    if (this.DragStartObject == null)
                    {
                        this.DragStartObject = this.FocusObject;
                        this.DragLastEndObject = this.FocusObject;
                    }

                    if (obj == this.DragLastEndObject)
                    {
                        if (EnsureVisible(obj) == true)
                            this.Update();
                        return;
                    }

                    List<CellBase> current = GetRangeObjects(
            this.DragStartObject,
            obj);
                    List<CellBase> last = new List<CellBase>();
                    if (this.DragLastEndObject != null)
                    {
                        last = GetRangeObjects(
                             this.DragStartObject,
                             this.DragLastEndObject);
                    }

                    /*
                    List<CellBase> old = new List<CellBase>();
                    old.AddRange(this.m_aSelectedArea);
                     * */

                    List<CellBase> cross = null;
                    // a和b中交叉的部分放入union，并从a和b中去掉
                    Compare(ref current,
                        ref last,
                        out cross);

                    SelectObjects(last, SelectAction.Toggle);
                    // cross部分不用操心
                    SelectObjects(current, SelectAction.On);

                    this.DragLastEndObject = obj;

                    // 方法2
                    // Current到 result.Object之间，toggle；然后，在留意把Start选上

                    if (EnsureVisibleWhenScrolling(obj) == true)
                        this.Update();

                    return;
                }


                // END1:
                this.DragStartObject = obj;
                this.DragLastEndObject = null;  // 清除

                // 选择新一个
                List<CellBase> temp = new List<CellBase>();
                temp.Add(obj);
                if (bControl == true)
                {
                    SelectObjects(temp, SelectAction.Toggle);
                }
                else
                {
                    SelectObjects(temp, SelectAction.On);
                }

                if (EnsureVisibleWhenScrolling(obj) == true)
                    this.Update();


                // ShowTip(obj, e.Location, false);

            }
            else
            {
                // 发出警告性的响声
                // Console.Beep();
            }

        }

        void BuildBindingMeneItems(ContextMenuStrip contextMenu,
            bool bHasCellSelected)
        {
            ToolStripMenuItem menuItem = null;
            ToolStripLabel label = null;

            label = new ToolStripLabel("装订");
            label.Font = new Font(label.Font, FontStyle.Bold);
            label.ForeColor = Color.DarkGreen;
            contextMenu.Items.Add(label);

            // 合订选择的事项
            menuItem = new ToolStripMenuItem(" 合订(&B)");
            menuItem.Click += new EventHandler(menuItem_bindingSelectedItem_Click);
            if (bHasCellSelected == false)  // TODO: 可以条件更严格一些，只有当具有选定的未装订的单册，菜单项才可用
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);


            bool bHasParentCell = false;
            bool bHasMemberCell = false;
            List<Cell> selected_cells = this.SelectedCells;
            for (int i = 0; i < selected_cells.Count; i++)
            {
                Cell cell = selected_cells[i];

                if (cell.IsMember == true)
                {
                    bHasMemberCell = true;
                }

                if (cell.item != null)
                {
                    if (cell.item.MemberCells.Count > 0)
                    {
                        Debug.Assert(cell.item.MemberCells.Count > 0, "");
                        bHasParentCell = true;
                    }

                }
            }

            // 解除合订
            menuItem = new ToolStripMenuItem(" 解除合订(&R)");
            menuItem.Click += new EventHandler(menuItem_releaseBinding_Click);
            if (bHasParentCell == false)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // 仅删除成员册记录
            menuItem = new ToolStripMenuItem(" 仅删除成员册记录(&D)");
            menuItem.Click += new EventHandler(menuItem_onlyDeleteMemberRecords_Click);
            if (bHasParentCell == false
                && bHasMemberCell == false)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);


            // 移出[不收缩]
            menuItem = new ToolStripMenuItem(" 移出[不收缩](&M)");
            menuItem.Click += new EventHandler(menuItem_removeFromBinding_Click);
            if (bHasMemberCell == false)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // 移出[收缩]
            menuItem = new ToolStripMenuItem(" 移出[收缩](&S)");
            menuItem.Click += new EventHandler(menuItem_removeFromBindingAndShrink_Click);
            if (bHasMemberCell == false)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);
        }

        void BuildAcceptingMeneItems(ContextMenuStrip contextMenu)
        {
            ToolStripMenuItem menuItem = null;
            ToolStripLabel label = null;

            label = new ToolStripLabel("记到");
            label.Font = new Font(label.Font, FontStyle.Bold);
            label.ForeColor = Color.DarkGreen;
            contextMenu.Items.Add(label);

            // 到
            menuItem = new ToolStripMenuItem(" 到(&A)");
            menuItem.Click += new EventHandler(menuItem_AcceptCells_Click);
            contextMenu.Items.Add(menuItem);

            // 撤销记到
            menuItem = new ToolStripMenuItem(" 撤销记到(&U)");
            menuItem.Click += new EventHandler(menuItem_unacceptCells_Click);
            contextMenu.Items.Add(menuItem);


            // 新增预测格
            menuItem = new ToolStripMenuItem(" 新增预测格[前插](&C)");
            menuItem.Click += new EventHandler(menuItem_newCalulatedCells_Click);
            contextMenu.Items.Add(menuItem);
        }

        // 上下文菜单
        void PopupMenu(Point point)
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem menuItem = null;
            ToolStripSeparator menuSepItem = null;
            ToolStripLabel label = null;

            // 是否有册格子被选择
            bool bHasCellSelected = this.HasCellSelected();
            // 是否有期格子被选择
            bool bHasIssueSelected = this.HasIssueSelected();

            if (this.WholeLayout != "binding")
            {
                // *** 记到
                BuildAcceptingMeneItems(contextMenu);
            }
            else
            {
                // *** 装订
                BuildBindingMeneItems(contextMenu,
                    bHasCellSelected);
            }

            // ---
            menuSepItem = new ToolStripSeparator();
            contextMenu.Items.Add(menuSepItem);


            label = new ToolStripLabel("册");
            label.Font = new Font(label.Font, FontStyle.Bold);
            label.ForeColor = Color.DarkGreen;
            contextMenu.Items.Add(label);

            // 编辑册格子
            menuItem = new ToolStripMenuItem(" 编辑(&M)");
            menuItem.Click += new EventHandler(menuItem_modifyCell_Click);
            if (bHasCellSelected == false)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // 刷新出版时间
            menuItem = new ToolStripMenuItem(" 刷新出版时间(&P)");
            menuItem.Click += new EventHandler(menuItem_refreshPublishTime_Click);
            if (bHasCellSelected == false)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // 刷新卷期范围
            menuItem = new ToolStripMenuItem(" 刷新卷期范围(&P)");
            menuItem.Click += new EventHandler(menuItem_refreshVolumeString_Click);
            if (bHasCellSelected == false)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // 设为空白
            menuItem = new ToolStripMenuItem(" 设为空白(&B)");
            menuItem.Tag = point;
            menuItem.Click += new EventHandler(menuItem_setBlank_Click);
            if (bHasCellSelected == true || bHasIssueSelected == true)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            /*
            // 向左移动
            menuItem = new ToolStripMenuItem("向左移动(&L)");
            menuItem.Click += new EventHandler(menuItem_moveToLeft_Click);
            if (bHasSelected == false)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);
             * */


            // 删除册格子
            menuItem = new ToolStripMenuItem(" 删除(&D)");
            menuItem.Click += new EventHandler(menuItem_deleteCells_Click);
            if (bHasCellSelected == false)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);


            // ---
            menuSepItem = new ToolStripSeparator();
            contextMenu.Items.Add(menuSepItem);

            label = new ToolStripLabel("期");
            label.Font = new Font(label.Font, FontStyle.Bold);
            label.ForeColor = Color.DarkGreen;
            contextMenu.Items.Add(label);

            // 新增期
            menuItem = new ToolStripMenuItem(" 新增[后插](&N)");
            menuItem.Click += new EventHandler(menuItem_newIssue_Click);
            contextMenu.Items.Add(menuItem);

            // 增全各期
            menuItem = new ToolStripMenuItem(" 增全[后插](&A)");
            menuItem.Click += new EventHandler(menuItem_newAllIssue_Click);
            contextMenu.Items.Add(menuItem);

            // 修改期
            menuItem = new ToolStripMenuItem(" 修改(&M)");
            menuItem.Click += new EventHandler(menuItem_modifyIssue_Click);
            if (bHasIssueSelected == false)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // 刷新订购信息
            menuItem = new ToolStripMenuItem(" 刷新订购信息(&R)");
            menuItem.Click += new EventHandler(menuItem_refreshOrderInfo_Click);
            if (bHasIssueSelected == false)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);


            // 删除期
            menuItem = new ToolStripMenuItem(" 删除(&D)");
            menuItem.Tag = point;
            menuItem.Click += new EventHandler(menuItem_deleteIssues_Click);
            if (bHasIssueSelected == false)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // 恢复期记录
            menuItem = new ToolStripMenuItem(" 恢复期记录(&V)");
            menuItem.Tag = point;
            menuItem.Click += new EventHandler(menuItem_recoverIssues_Click);
            if (bHasIssueSelected == false)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // 封面图像
            menuItem = new ToolStripMenuItem(" 封面图像");
            contextMenu.Items.Add(menuItem);

            {
                // 从剪贴板插入封面图像
                ToolStripMenuItem subMenuItem = new ToolStripMenuItem();
                subMenuItem = new ToolStripMenuItem(" 从剪贴板(&C) ...");
                subMenuItem.Tag = point;
                subMenuItem.Click += new EventHandler(menuItem_insertCoverImageFromClipboard_Click);
                if (bHasIssueSelected == false)
                    subMenuItem.Enabled = false;
                menuItem.DropDown.Items.Add(subMenuItem);

                // 从摄像头插入封面图像
                subMenuItem = new ToolStripMenuItem(" 从摄像头(&A) ...");
                subMenuItem.Tag = point;
                subMenuItem.Click += new EventHandler(menuItem_insertCoverImageFromCamera_Click);
                if (bHasIssueSelected == false)
                    subMenuItem.Enabled = false;
                menuItem.DropDown.Items.Add(subMenuItem);

                // 从龙源期刊插入封面图像
                subMenuItem = new ToolStripMenuItem(" 从龙源期刊(&A) ...");
                subMenuItem.Tag = point;
                subMenuItem.Click += new EventHandler(menuItem_insertCoverImageFromLongyuanQikan_Click);
                if (bHasIssueSelected == false)
                    subMenuItem.Enabled = false;
                menuItem.DropDown.Items.Add(subMenuItem);


                // 删除封面图像
                subMenuItem = new ToolStripMenuItem(" 删除(&D)");
                subMenuItem.Tag = point;
                subMenuItem.Click += new EventHandler(menuItem_deleteCoverImage_Click);
                if (bHasIssueSelected == false)
                    subMenuItem.Enabled = false;
                menuItem.DropDown.Items.Add(subMenuItem);
            }

            // 切换期布局
            menuItem = new ToolStripMenuItem(" 切换布局(&S)");
            /*
            if (bHasIssueSelected == false)
                menuItem.Enabled = false;
             * */
            contextMenu.Items.Add(menuItem);

            if (menuItem.Enabled == true)
            {
                // TODO:把统计数量占多数的一种模式给打勾到子菜单上
                IssueLayoutState layout = GetMostSelectedLayoutState();

                // 子菜单
                {
                    ToolStripMenuItem subMenuItem = new ToolStripMenuItem();
                    subMenuItem.Text = "装订";
                    subMenuItem.Tag = IssueLayoutState.Binding;
                    subMenuItem.Image = this.imageList_layout.Images[0];
                    subMenuItem.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                    subMenuItem.ImageTransparentColor = this.imageList_layout.TransparentColor;
                    if (bHasIssueSelected == false)
                        subMenuItem.Enabled = false;
                    else
                    {
                        if (layout == IssueLayoutState.Binding)
                            subMenuItem.Checked = true;
                    }
                    subMenuItem.Click += new EventHandler(MenuItem_switchIssueLayout_Click);
                    menuItem.DropDown.Items.Add(subMenuItem);
                }

                {
                    ToolStripMenuItem subMenuItem = new ToolStripMenuItem();
                    subMenuItem.Text = "记到";
                    subMenuItem.Tag = IssueLayoutState.Accepting;
                    subMenuItem.Image = this.imageList_layout.Images[1];
                    subMenuItem.DisplayStyle = ToolStripItemDisplayStyle.ImageAndText;
                    subMenuItem.ImageTransparentColor = this.imageList_layout.TransparentColor;
                    if (bHasIssueSelected == false)
                        subMenuItem.Enabled = false;
                    else
                    {
                        if (layout == IssueLayoutState.Accepting)
                            subMenuItem.Checked = true;
                    }
                    subMenuItem.Click += new EventHandler(MenuItem_switchIssueLayout_Click);
                    menuItem.DropDown.Items.Add(subMenuItem);
                }

                {
                    // ---
                    menuSepItem = new ToolStripSeparator();
                    menuItem.DropDown.Items.Add(menuSepItem);

                    ToolStripMenuItem subMenuItem = new ToolStripMenuItem();
                    subMenuItem.Text = "重新布局";
                    subMenuItem.Click += new EventHandler(MenuItem_refreshIssueLayout_Click);
                    menuItem.DropDown.Items.Add(subMenuItem);
                }
            }

            // ---
            menuSepItem = new ToolStripSeparator();
            contextMenu.Items.Add(menuSepItem);

            if (this.WholeLayout != "binding")
            {
                // *** 装订
                BuildBindingMeneItems(contextMenu,
                    bHasCellSelected);
            }
            else
            {
                // *** 记到
                BuildAcceptingMeneItems(contextMenu);
            }

            // ---
            menuSepItem = new ToolStripSeparator();
            contextMenu.Items.Add(menuSepItem);


            // 编辑区域
            menuItem = new ToolStripMenuItem("编辑区域(&E)");
            menuItem.Click += new EventHandler(menuItem_toggleEditArea_Click);
            if (this.EditArea == null)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);
            if (this.EditArea != null)
            {
                EditAreaEventArgs e1 = new EditAreaEventArgs();
                e1.Action = "get_state";
                this.EditArea(this, e1);
                if (e1.Result == "visible")
                    menuItem.Checked = true;
                else
                    menuItem.Checked = false;
            }

            this.Update();
            contextMenu.Show(this, point);
        }

        // 刷新订购信息
        void menuItem_refreshOrderInfo_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            List<IssueBindingItem> selected_issues = this.SelectedIssues;
            // 整理一下selected_issues数组
            for (int i = 0; i < selected_issues.Count; i++)
            {
                IssueBindingItem issue = selected_issues[i];
                if (issue == null
                    || String.IsNullOrEmpty(issue.PublishTime) == true)
                {
                    selected_issues.RemoveAt(i);
                    i--;
                    continue;
                }
            }

            if (selected_issues.Count == 0)
            {
                strError = "尚未选定要刷新的期格子";
                goto ERROR1;
            }

            for (int i = 0; i < selected_issues.Count; i++)
            {
                IssueBindingItem issue = selected_issues[i];

                nRet = issue.RefreshOrderInfo(out strError);
                if (nRet == -1)
                    goto ERROR1;
            }


            this.AfterWidthChanged(true);   // content宽度可能改变
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 刷新出版时间
        void menuItem_refreshPublishTime_Click(object sender, EventArgs e)
        {
            string strError = "";
            // int nRet = 0;

            List<Cell> selected_cells = this.SelectedCells;
            if (selected_cells.Count == 0)
            {
                strError = "尚未选定要刷新出版时间的格子";
                goto ERROR1;
            }

            List<CellBase> changed_cells = new List<CellBase>();
            for (int i = 0; i < selected_cells.Count; i++)
            {
                Cell cell = selected_cells[i];

                if (cell != null
                    && String.IsNullOrEmpty(cell.Container.PublishTime) == true)
                    continue;   // 跳过自由期内的

                if (cell.item == null)
                    continue;

                if (this.IsBindingParent(cell) == true)
                {
                    // 合订册
                    Debug.Assert(cell.item != null, "");
                    Debug.Assert(cell.item.IsMember == false, "");


                    if (cell.item.RefreshPublishTime() == true)
                    {
                        cell.item.Changed = true;
                        changed_cells.Add(cell);
                    }
                }
                else if (cell.IsMember == true)
                {
                    // 注：cell.item可能为空
                    if (cell.item != null)
                    {
                        if (cell.item.RefreshPublishTime() == true)
                        {
                            cell.item.Changed = true;
                            changed_cells.Add(cell);
                        }
                    }
                }
                else
                {
                    // 单册
                    if (cell.item != null
                        && cell.item.RefreshPublishTime() == true)
                    {
                        cell.item.Changed = true;
                        changed_cells.Add(cell);
                    }
                }
            }

            if (changed_cells.Count == 0)
            {
                strError = "没有发生刷新";
                goto ERROR1;
            }

            if (changed_cells.IndexOf(this.FocusObject) != -1)
            {
                if (this.CellFocusChanged != null)
                {
                    FocusChangedEventArgs e1 = new FocusChangedEventArgs();
                    e1.OldFocusObject = this.FocusObject;
                    e1.NewFocusObject = this.FocusObject;
                    this.CellFocusChanged(this, e1);
                }
            }

            this.UpdateObjects(changed_cells);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 刷新卷期范围
        void menuItem_refreshVolumeString_Click(object sender, EventArgs e)
        {
            string strError = "";
            // int nRet = 0;

            List<Cell> selected_cells = this.SelectedCells;
            if (selected_cells.Count == 0)
            {
                strError = "尚未选定要刷新卷期范围的格子";
                goto ERROR1;
            }

            List<CellBase> changed_cells = new List<CellBase>();
            for (int i = 0; i < selected_cells.Count; i++)
            {
                Cell cell = selected_cells[i];

                if (cell != null
    && String.IsNullOrEmpty(cell.Container.PublishTime) == true)
                    continue;   // 跳过自由期内的

                if (cell.item == null)
                    continue;

                if (this.IsBindingParent(cell) == true)
                {
                    // 合订册
                    Debug.Assert(cell.item != null, "");
                    Debug.Assert(cell.item.IsMember == false, "");

                    if (cell.item.RefreshVolumeString() == true)
                    {
                        cell.item.Changed = true;
                        changed_cells.Add(cell);
                    }
                }
                else if (cell.IsMember == true)
                {
                    // 注：cell.item可能为空
                    if (cell.item != null)
                    {
                        if (cell.item.RefreshVolumeString() == true)
                        {
                            cell.item.Changed = true;
                            changed_cells.Add(cell);
                        }
                    }
                }
                else
                {
                    // 单册
                    if (cell.item != null
                        && cell.item.RefreshVolumeString() == true)
                    {
                        cell.item.Changed = true;
                        changed_cells.Add(cell);
                    }
                }
            }

            if (changed_cells.Count == 0)
            {
                strError = "没有发生刷新";
                goto ERROR1;
            }

            if (changed_cells.IndexOf(this.FocusObject) != -1)
            {
                if (this.CellFocusChanged != null)
                {
                    FocusChangedEventArgs e1 = new FocusChangedEventArgs();
                    e1.OldFocusObject = this.FocusObject;
                    e1.NewFocusObject = this.FocusObject;
                    this.CellFocusChanged(this, e1);
                }
            }

            this.UpdateObjects(changed_cells);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 仅删除成员册记录。并不改变合订范围。
        void menuItem_onlyDeleteMemberRecords_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strWarning = "";
            // int nRet = 0;

            List<Cell> selected_cells = this.SelectedCells;
            if (selected_cells.Count == 0)
            {
                strError = "尚未选定要(仅)删除记录的格子";
                goto ERROR1;
            }

            // 已经参与装订的册
            List<Cell> member_cells = new List<Cell>();

            // 合订册
            List<Cell> parent_cells = new List<Cell>();


            // 分选
            for (int i = 0; i < selected_cells.Count; i++)
            {
                Cell cell = selected_cells[i];

                if (this.IsBindingParent(cell) == true)
                {
                    Debug.Assert(cell.item != null, "");
                    Debug.Assert(cell.item.IsMember == false, "");
                    parent_cells.Add(cell);
                }
                else if (cell.IsMember == true)
                {
                    // 注：cell.item可能为空
                    member_cells.Add(cell);
                }
                else
                {
                    strError = "本功能不能用于普通单册格子";
                    goto ERROR1;
                }
            }

            // 检查成员册
            strWarning = "";
            int nErrorCount = 0;
            int nOldCount = member_cells.Count;
            for (int i = 0; i < member_cells.Count; i++)
            {
                Cell cell = member_cells[i];
                IssueBindingItem issue = cell.Container;
                Debug.Assert(issue != null, "");

                if (cell.item == null)
                    continue;

                // 从属于所选合订册的成员册，要避免重复删除
                bool bFound = false;
                for (int j = 0; j < parent_cells.Count; j++)
                {
                    Cell parent_cell = parent_cells[j];

                    Debug.Assert(parent_cell.item != null, "");
                    if (cell.ParentItem == parent_cell.item)
                    {
                        bFound = true;
                        break;
                    }
                }
                if (bFound == true)
                {
                    member_cells.RemoveAt(i);
                    i--;
                    continue;
                }

                if (String.IsNullOrEmpty(cell.item.Borrower) == false)
                {
                    // 已借出状态
                    if (String.IsNullOrEmpty(strWarning) == false)
                        strWarning += ";\r\n";
                    strWarning += "册 '" + cell.item.RefID + "' 尚处于“已借出”状态";
                    nErrorCount++;
                    member_cells.RemoveAt(i);
                    i--;
                }
            }

            // 警告不能删除的成员册
            if (String.IsNullOrEmpty(strWarning) == false)
            {
                strError =
                    "所选定的 " + nOldCount.ToString() + " 个成员册中，有下列册记录不能删除:\r\n\r\n" + strWarning;
                goto ERROR1;
            }

            strWarning = "";
            if (member_cells.Count > 0)
                strWarning += " " + member_cells.Count.ToString() + " 个成员册的册记录";

            if (parent_cells.Count > 0)
            {
                if (String.IsNullOrEmpty(strWarning) == false)
                    strWarning += " 和";
                strWarning += " " + parent_cells.Count.ToString() + " 个合订册下属的所有成员册记录";
            }

            strWarning = "确实要删除所选定的" + strWarning + "?\r\n\r\n(注：本功能并不改变合订范围和合订状态)";
            DialogResult dialog_result = MessageBox.Show(this,
                strWarning,
                "BindingControls",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (dialog_result == DialogResult.No)
                return;

            // 开始删除

            // 检查所选合订册下属的全部成员册
            strWarning = "";
            nErrorCount = 0;
            nOldCount = parent_cells.Count;
            for (int i = 0; i < parent_cells.Count; i++)
            {
                Cell parent_cell = parent_cells[i];
                if (parent_cell.item == null)
                    continue;

                // 检查合订册的锁定状态
                if (parent_cell.item.Locked == true)
                {
                    strError = "锁定状态的合订册，其成员册不允许删除";
                    goto ERROR1;
                }

                // 从属于所选合订册的成员册，要避免重复删除
                for (int j = 0; j < parent_cell.item.MemberCells.Count; j++)
                {
                    Cell member_cell = parent_cell.item.MemberCells[j];
                    if (member_cell.item == null)
                        continue;

                    Debug.Assert(member_cell.item != null, "");

                    if (String.IsNullOrEmpty(member_cell.item.Borrower) == false)
                    {
                        // 已借出状态
                        if (String.IsNullOrEmpty(strWarning) == false)
                            strWarning += ";\r\n";
                        strWarning += "册 '" + member_cell.item.RefID + "' 尚处于“已借出”状态";
                        nErrorCount++;
                        j--;
                    }
                }
            }

            // 警告不能删除的(所选合订册的)成员册
            if (String.IsNullOrEmpty(strWarning) == false)
            {
                strError =
                    "所选定的 " + nOldCount.ToString() + " 个合订册中，有下列成员册记录不能删除:\r\n\r\n" + strWarning;
                goto ERROR1;
            }

            // 删除合订册下属的成员册记录
            strWarning = "";
            for (int i = 0; i < parent_cells.Count; i++)
            {
                Cell parent_cell = parent_cells[i];

                Debug.Assert(parent_cell.item != null, "");
                this.m_bChanged = true;

                for (int j = 0; j < parent_cell.item.MemberCells.Count; j++)
                {
                    Cell cell = parent_cell.item.MemberCells[j];

                    if (cell.item != null)
                    {
                        Debug.Assert(cell.item.Calculated == false, "");
                        cell.item.Deleted = true;
                        cell.item.Changed = true;
                    }
                }

                parent_cell.item.AfterMembersChanged();
            }

            // 删除单独选择成员格子
            List<ItemBindingItem> temp_parent_items = new List<ItemBindingItem>();    // 去重作用
            for (int i = 0; i < member_cells.Count; i++)
            {
                Cell cell = member_cells[i];
                if (cell == null && cell.item == null)
                    continue;
                Debug.Assert(cell.IsMember == true, "");
                Debug.Assert(cell.item.Calculated == false, "");
                cell.item.Deleted = true;
                cell.item.Changed = true;

                ItemBindingItem parent_item = cell.item.ParentItem;
                Debug.Assert(parent_item != null, "");

                if (temp_parent_items.IndexOf(parent_item) == -1)
                    temp_parent_items.Add(parent_item);
            }
            for (int i = 0; i < temp_parent_items.Count; i++)
            {
                temp_parent_items[i].AfterMembersChanged();
            }

            this.Invalidate();
            return;
        ERROR1:
            MessageBox.Show(this, strError);

        }

        // 编辑一个册格子
        void menuItem_modifyCell_Click(object sender, EventArgs e)
        {
            string strError = "";

            List<Cell> selected_cells = this.SelectedCells;

            Cell cell = null;
            if (this.FocusObject != null && this.FocusObject is Cell)
                cell = (Cell)this.FocusObject;
            else if (selected_cells.Count > 0)
                cell = selected_cells[0];
            else
            {
                strError = "尚未选择要编辑的格子";
                goto ERROR1;
            }

            Debug.Assert(cell != null, "");

            // 显示编辑区域
            EditAreaEventArgs e1 = new EditAreaEventArgs();
            e1.Action = "get_state";
            this.EditArea(this, e1);
            if (e1.Result != "visible")
            {
                e1 = new EditAreaEventArgs();
                e1.Action = "open";
                this.EditArea(this, e1);
            }

            e1 = new EditAreaEventArgs();
            e1.Action = "focus";
            this.EditArea(this, e1);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 编辑期
        void menuItem_modifyIssue_Click(object sender, EventArgs e)
        {
            string strError = "";
            // int nRet = 0;

            List<IssueBindingItem> selected_issues = this.SelectedIssues;
            if (selected_issues.Count == 0)
            {
                if (this.FocusObject is IssueBindingItem)
                {
                    selected_issues.Add((IssueBindingItem)this.FocusObject);
                }
                else
                {
                    strError = "尚未选定要修改的期";
                    goto ERROR1;
                }
            }

            IssueBindingItem issue = selected_issues[0];

            if (String.IsNullOrEmpty(issue.PublishTime) == true)
            {
                strError = "自由期不能被修改";
                goto ERROR1;
            }

            IssueDialog dlg = new IssueDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.Tag = issue;
            dlg.CheckDup -= new CheckDupEventHandler(dlg_CheckDup);
            dlg.CheckDup += new CheckDupEventHandler(dlg_CheckDup);

            dlg.PublishTime = issue.PublishTime;
            dlg.Issue = issue.Issue;
            dlg.Zong = issue.Zong;
            dlg.Volume = issue.Volume;
            dlg.Comment = issue.Comment;

            dlg.StartPosition = FormStartPosition.CenterScreen;

            // REDO_INPUT:
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            /*
            List<IssueBindingItem> dup_issues = null;
            List<IssueBindingItem> warning_issues = null;
            string strWarning = "";

            // 对出版时间进行查重
            // parameters:
            //      exclude 检查中要排除的TreeNode对象
            // return:
            //      -1  error
            //      0   没有重
            //      1   重
            nRet = CheckPublishTimeDup(dlg.PublishTime,
                dlg.Issue,
                dlg.Zong,
                dlg.Volume,
                issue,
                out warning_issues,
                out strWarning,
                out dup_issues,
                out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 1)
            {
                // 选中所重复的期格子，便于操作者观察重复的情况
                Debug.Assert(dup_issue != null, "");
                if (dup_issue != null)
                {
                    this.ClearAllSelection();
                    dup_issue.Select(SelectAction.On);
                    this.EnsureVisible(dup_issue);  // 确保滚入视野
                    this.UpdateObject(dup_issue);
                    this.Update();
                }

                MessageBox.Show(this, "修改后的期 " + strError + "\r\n请修改。");
                goto REDO_INPUT;
            }
             * */


            issue.PublishTime = dlg.PublishTime;
            issue.Issue = dlg.Issue;
            issue.Zong = dlg.Zong;
            issue.Volume = dlg.Volume;
            issue.Comment = dlg.Comment;
            issue.Changed = true;

            // 设置或者刷新一个操作记载
            // 可能会抛出异常
            issue.SetOperation(
                "lastModified",
                this.Operator,
                "");


            // 修改全部下属格子的volume string和publish time
            string strNewVolumeString =
    VolumeInfo.BuildItemVolumeString(
    dp2StringUtil.GetYearPart(issue.PublishTime),
    issue.Issue,
issue.Zong,
issue.Volume);
            for (int i = 0; i < issue.Cells.Count; i++)
            {
                Cell cell = issue.Cells[i];
                if (cell == null)
                    continue;

                // 成员册
                if (cell.item == null && cell.ParentItem != null)
                {
                    cell.RefreshOutofIssue();
                    cell.ParentItem.AfterMembersChanged();
                    continue;
                }

                if (cell.item == null)
                    continue;

                if (cell.item.IsParent == true)
                    continue;   // 不直接修改合订册。但是，合订册内的任何格子变化，都会自动汇总到合订册

                bool bChanged = false;
                if (cell.item.PublishTime != issue.PublishTime)
                {
                    cell.item.PublishTime = issue.PublishTime;
                    bChanged = true;
                }

                if (cell.item.Volume != strNewVolumeString)
                {
                    cell.item.Volume = strNewVolumeString;
                    bChanged = true;
                }

                if (bChanged == true)
                {
                    cell.RefreshOutofIssue();
                    cell.item.Changed = true;

                    if (cell.item.IsMember == true)
                        cell.item.ParentItem.AfterMembersChanged();
                }
            }

            // 选中修改过的期格子
            this.ClearAllSelection();
            issue.Select(SelectAction.On);
            this.EnsureVisible(issue);  // 确保滚入视野

            this.AfterWidthChanged(true);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 根据当前选择的期的情况，统计出数量最多的布局模式
        IssueLayoutState GetMostSelectedLayoutState()
        {
            int nBindingCount = 0;
            int nAcceptionCount = 0;
            List<IssueBindingItem> selected_issues = this.SelectedIssues;
            for (int i = 0; i < selected_issues.Count; i++)
            {
                IssueBindingItem issue = selected_issues[i];
                if (issue.IssueLayoutState == IssueLayoutState.Accepting)
                    nAcceptionCount++;
                else if (issue.IssueLayoutState == IssueLayoutState.Binding)
                    nBindingCount++;
            }

            if (nAcceptionCount > nBindingCount)
                return IssueLayoutState.Accepting;
            return IssueLayoutState.Binding;
        }

        // 新增预测格
        // 目前只能处理 记到布局的行
        // 如果选定了订购组格子，则在此组的末尾追加一个新的预测格；
        // 如果选定了订购组内的格子，则在此位置插入一个新的预测格。
        // 如果同一个组中选择了两者，则以组内格子选择为有效
        // 如果选择了其他格子，也就是组外的格子，则本功能无效
        void menuItem_newCalulatedCells_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 从选择范围中挑出组中的格子或者组格子。
            List<Cell> selected_cells = this.SelectedCells;
            if (selected_cells.Count == 0)
            {
                strError = "尚未选定要新增预测册格的参考格";
                goto ERROR1;
            }

            List<GroupCell> group_cells = new List<GroupCell>();
            List<Cell> ingroup_cells = new List<Cell>();
            // 分选
            for (int i = 0; i < selected_cells.Count; i++)
            {
                Cell cell = selected_cells[i];
                if (cell == null)
                    continue;
                if (cell is GroupCell)
                {
                    group_cells.Add((GroupCell)cell);
                    continue;
                }

                if (cell.item != null && cell.item.InGroup == true)
                {
                    if (cell.Container.IssueLayoutState != IssueLayoutState.Accepting)
                    {
                        strError = "只能对位于记到布局的期内的参考格子进行新增预测格的操作";
                        goto ERROR1;
                    }
                    ingroup_cells.Add(cell);
                }
            }

            // 去掉重复、把尾部对象替换为头部对象
            for (int i = 0; i < group_cells.Count; i++)
            {
                GroupCell group = group_cells[i];
                if (group.EndBracket == true)
                {
                    group_cells.RemoveAt(i);
                    GroupCell head = group.HeadGroupCell;
                    Debug.Assert(head != null, "");
                    if (head == null)
                        continue;
                    int idx = group_cells.IndexOf(head);
                    if (idx != i && idx != -1)
                    {
                        group_cells.Insert(i, head);
                        i--;
                        continue;
                    }
                }

                if (group.EndBracket == false)
                {
                    int idx = group_cells.IndexOf(group);
                    if (idx != i && idx != -1)
                    {
                        group_cells.RemoveAt(i);
                        i--;
                    }
                }
            }

            // 如果一个组的组内对象已经被选择了，就不要再有头部对象
            for (int i = 0; i < group_cells.Count; i++)
            {
                GroupCell group = group_cells[i];
                Debug.Assert(group.EndBracket == false, "");
                List<Cell> members = group.MemberCells;
                for (int j = 0; j < ingroup_cells.Count; j++)
                {
                    Cell cell = ingroup_cells[j];
                    if (members.IndexOf(cell) != -1)
                    {
                        group_cells.RemoveAt(i);
                        i--;
                        break;
                    }
                }
            }

            if (ingroup_cells.Count == 0 && group_cells.Count == 0)
            {
                strError = "所选定的格子中没有包含组格子或者组内格子";
                goto ERROR1;
            }

            List<Cell> new_cells = new List<Cell>();

            // 先遍历组格子
            for (int i = 0; i < group_cells.Count; i++)
            {
                GroupCell group = group_cells[i];
                Debug.Assert(group.EndBracket == false, "");
                // 在组内插入新的格子(预测格子)
                // parameters:
                //      nInsertPos  插入位置。如果为-1，表示插入在尾部
                // return:
                //      返回插入的index(整个issue.Cells下标)
                nRet = group.InsertNewMemberCell(
                    -1,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                new_cells.Add(group.Container.GetCell(nRet));
            }

            // 然后遍历组内格子
            for (int i = 0; i < ingroup_cells.Count; i++)
            {
                Cell cell = ingroup_cells[i];
                Debug.Assert(cell != null, "");
                Debug.Assert(cell.item != null, "");

                GroupCell group = cell.item.GroupCell;
                Debug.Assert(group != null, "");
                int nInsertPos = group.MemberCells.IndexOf(cell);
                Debug.Assert(nInsertPos != -1, "");

                nRet = group.InsertNewMemberCell(
                    nInsertPos,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                new_cells.Add(group.Container.GetCell(nRet));
            }

            // 选定新创建的那些对象
            this.ClearAllSelection();
            for (int i = 0; i < new_cells.Count; i++)
            {
                Cell cell = new_cells[i];
                cell.Select(SelectAction.On);
            }

            this.AfterWidthChanged(true);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 切换期行的布局
        void MenuItem_switchIssueLayout_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            ToolStripMenuItem menu = (ToolStripMenuItem)sender;

            IssueLayoutState layout = (IssueLayoutState)menu.Tag;

            List<IssueBindingItem> selected_issues = new List<IssueBindingItem>();
            selected_issues.AddRange(this.SelectedIssues);

            if (selected_issues.Count == 0)
            {
                strError = "尚未选择要切换布局的期对象";
                goto ERROR1;
            }

            List<IssueBindingItem> changed_issues = null;

            // 成批切换期行的布局模式
            nRet = SwitchIssueLayout(selected_issues,
                layout,
                out changed_issues,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            // UpdateIssues(changed_issues);
            this.AfterWidthChanged(true);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 把当前超过管辖范围的合订册单元去掉
        // parameters:
        //      bRemoveCell 是否要从 this.Issues 里面去掉相关Cell
        //      bRemoveMemberCell 是否要从 this.Issues 里面去掉合订成员Cell
        int RemoveOutofBindingItems(
            ref List<ItemBindingItem> binding_items,
            Hashtable memberitems_table,
            bool bRemoveCell,
            bool bRemoveMemberCell,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this.m_bHideLockedBindingCell == true
                && Global.IsGlobalUser(this.LibraryCodeList) == false)
            {
                for (int i = 0; i < binding_items.Count; i++)
                {
                    ItemBindingItem parent_item = binding_items[i];
                    List<ItemBindingItem> member_items = (List<ItemBindingItem>)memberitems_table[parent_item];

                    Debug.Assert(member_items != null, "");

                    if (member_items.Count == 0)
                        continue;

                    // 检查一个合订册的所有成员,看看是不是(至少一个)和当前可见订购组有从属关系?
                    // return:
                    //      -1  出错
                    //      0   没有交叉
                    //      1   有交叉
                    nRet = IsMemberCrossOrderGroup(parent_item,
                        member_items,
                        true,
                        out strError);
                    if (nRet == -1)
                        return -1;

                    string strLibraryCode = Global.GetLibraryCode(parent_item.LocationString);

                    bool bLocked = (StringUtil.IsInList(strLibraryCode, this.LibraryCodeList) == false);
                    parent_item.Locked = bLocked;

                    // 合订本本身馆代码在外，而且其成员也不和可见订购组交叉的，删除合订本对象
                    if (bLocked == true
                        && nRet == 0)
                    {
                        if (bRemoveCell == true)
                            this.RemoveItem(parent_item, bRemoveMemberCell);
                        else
                        {
                            // 此时尚未加入Issue对象下面
                            this.m_hideitems.Add(parent_item);
                        }

                        binding_items.RemoveAt(i);
                        i--;
                    }
                }
            }

            return 0;
        }

        // 把那些当前隐藏的合订册和成员册试图重新安放一次
        public int RelayoutHiddenBindingCell(out string strError)
        {
            strError = "";
            int nRet = 0;

            if (this.m_bHideLockedBindingCell == true
    && Global.IsGlobalUser(this.LibraryCodeList) == false)
            {
                Hashtable memberitems_table = new Hashtable();

                foreach (ItemBindingItem item in this.ParentItems)
                {
                    memberitems_table[item] = item.MemberItems;
                }

                // 把当前超过管辖范围的合订册单元去掉
                nRet = RemoveOutofBindingItems(
                    ref this.ParentItems,
                    memberitems_table,
                    true,
                    true,
                    out strError);
                if (nRet == -1)
                    return -1;

                return 0;
            }

            if (this.m_hideitems.Count == 0)
                return 0;

            // 挑出隐藏的合订册
            List<ItemBindingItem> binding_items = new List<ItemBindingItem>();
            foreach (ItemBindingItem item in this.m_hideitems)
            {
                if (item.IsParent == true)
                {
                    if (this.ParentItems.IndexOf(item) != -1)
                        continue;   // 如果已经显示了，就不要处理了
                    binding_items.Add(item);
                }
            }

            if (binding_items.Count == 0)
                return 0;

            {
                Hashtable memberitems_table = new Hashtable();
                string strWarning = "";
                // 遍历合订册对象数组，建立成员对象数组
                // parameters:
                //      parent_items    合订册对象数组。处理后的对象会从这个数组中移走
                nRet = CreateMemberItemTable(
                    ref binding_items,
                    out memberitems_table,
                    ref strWarning,
                    out strError);
                if (nRet == -1)
                    return -1;

                // 把当前超过管辖范围的合订册单元去掉
                nRet = RemoveOutofBindingItems(
                    ref binding_items,
                    memberitems_table,
                    true,
                    true,
                    out strError);
                if (nRet == -1)
                    return -1;

                /*
                Hashtable placed_table = new Hashtable();   // 已经被作为合订本下属安放过位置的册对象

                // 安放合订成员册对象
                nRet = PlaceMemberCell(
                    ref binding_items,
                    memberitems_table,
                    ref placed_table,
                    out strError);
                if (nRet == -1)
                    return -1;
                 * */
                // 只安放合订册对象
                nRet = PlaceParentItems(
                    ref binding_items,
                    memberitems_table,
                    out strError);
                if (nRet == -1)
                    return -1;

            }

            this.ParentItems.AddRange(binding_items);
            foreach (ItemBindingItem item in binding_items)
            {
                this.m_hideitems.Remove(item);
            }
            return 0;
        }

        public void RefreshLayout()
        {
            MenuItem_refreshIssueLayout_Click(null, null);
        }

        // 刷新全部期行的布局
        void MenuItem_refreshIssueLayout_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            for (int i = 0; i < this.Issues.Count; i++)
            {
                IssueBindingItem issue = this.Issues[i];
                if (String.IsNullOrEmpty(issue.PublishTime) == true)
                    continue;

                if (issue.IssueLayoutState == IssueLayoutState.Binding)
                    nRet = issue.ReLayoutBinding(out strError);
                else
                    nRet = issue.LayoutAccepting(out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            // UpdateIssues(changed_issues);
            this.AfterWidthChanged(true);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }


        void UpdateIssues(List<IssueBindingItem> issues)
        {
            List<CellBase> list = new List<CellBase>();
            for (int i = 0; i < issues.Count; i++)
            {
                IssueBindingItem issue = issues[i];
                list.Add((CellBase)issue);
            }

            this.UpdateObjects(list);
        }

        // 增全各期
        // 在当前位置后面新增若干期，直到可用订购范围的末尾
        // TODO: 应当用当年期号来查重，避免错位大量创建
        void menuItem_newAllIssue_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            //  int nCreateCount = 0;
            List<IssueBindingItem> new_issues = new List<IssueBindingItem>();

            IssueBindingItem ref_issue = null;

            List<IssueBindingItem> ref_issues = this.SelectedIssues;
            for (int i = 0; i < ref_issues.Count; i++)
            {
                IssueBindingItem issue = ref_issues[i];
                if (String.IsNullOrEmpty(issue.PublishTime) == true)
                    continue;
                ref_issue = issue;
                break;
            }

            if (ref_issue == null)
                ref_issue = GetTailIssue();
            REDO:
            // 找到最后一期。如果找不到，则先出现对话框询问第一期
            if (ref_issue == null)
            {
                string strStartDate = "";
                string strEndDate = "";
                // 获得可用的最大订购时间范围
                // return:
                //      -1  error
                //      0   not found
                //      1   found
                nRet = GetMaxOrderRange(out strStartDate,
                    out strEndDate,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 0)
                {
                    strError = "当前没有订购信息，无法进行增全操作";
                    goto ERROR1;
                }

                // 出现对话框，让输入第一期的参数。出版时间由软件自动探测和推荐
                // 这里要求日常管理订购信息把已经到全的订购记录“封闭”。否则会出现把原来早就验收过的第一期出版时间推荐出来的情况
                // 所谓封闭(订购信息的)操作，可以由过刊装订操作来负责
                // 封闭，是把相关订购记录的<state>中增加“已验收”字符串
                IssueDialog dlg = new IssueDialog();
                MainForm.SetControlFont(dlg, this.Font, false);
                dlg.Tag = null;
                dlg.CheckDup -= new CheckDupEventHandler(dlg_CheckDup);
                dlg.CheckDup += new CheckDupEventHandler(dlg_CheckDup);

                dlg.Text = "请指定首期的特征";
                dlg.PublishTime = strStartDate + "?";   // 获得订购范围的起点日期
                dlg.EditComment = "当前订购时间范围为 " + strStartDate + "-" + strEndDate;   // 显示可用的订购时间范围
                dlg.StartPosition = FormStartPosition.CenterScreen;

            REDO_INPUT:
                dlg.ShowDialog(this);

                if (dlg.DialogResult != DialogResult.OK)
                    return; // 放弃整个功能

                // 检查一下这个出版时间是否超过订购时间范围?
                if (InOrderRange(dlg.PublishTime) == false)
                {
                    // TODO: 最好提示当前可用的时间范围?
                    MessageBox.Show(this, "您指定的首期出版时间 '" + dlg.PublishTime + "' 不在当前可用的订购时间范围内，请重新输入。");
                    goto REDO_INPUT;
                }

                /*
                // 查重?
                // 对publishTime要查重，对号码体系要进行检查和提出警告
                IssueBindingItem dup_issue = null;
                // 对出版时间进行查重
                // parameters:
                //      exclude 检查中要排除的TreeNode对象
                // return:
                //      -1  error
                //      0   没有重
                //      1   重
                nRet = CheckPublishTimeDup(dlg.PublishTime,
                    null,
                    out dup_issue,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                {
                    // 
                    DialogResult dialog_result = MessageBox.Show(this,
            "您所设定的首期出版日期 '"+dlg.PublishTime+"' 已经存在，是否要使用这个已经存在的期作为参考对象继续创建后面的期?\r\n\r\n(Yes: 继续创建; No: 返回重新输入首期参数; Cancel: 放弃整个创建过程)",
            "BindingControls",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question,
            MessageBoxDefaultButton.Button2);
                    if (dialog_result == DialogResult.Cancel)
                        return;
                    if (dialog_result == DialogResult.No)
                        goto REDO_INPUT;
                }
                 * */

                IssueBindingItem new_issue = new IssueBindingItem();
                new_issue.Container = this;
                nRet = new_issue.Initial("<root />",
                    "",
                    false,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                new_issue.PublishTime = dlg.PublishTime;
                new_issue.Issue = dlg.Issue;
                new_issue.Zong = dlg.Zong;
                new_issue.Volume = dlg.Volume;
                new_issue.Comment = dlg.Comment;
                new_issue.RefID = Guid.NewGuid().ToString();

                new_issue.Changed = true;
                new_issue.NewCreated = true;

                /*
                // 设置或者刷新一个操作记载
                // 可能会抛出异常
                new_issue.SetOperation(
                    "create",
                    this.Operator,
                    "");
                 * */

                // 插入到合适的位置?
                InsertIssueToIssues(new_issue);

                // 为新增的期设置好Layout模式
                // 弥补因为插入带来的合订本裂断
                nRet = SetNewIssueLayout(new_issue,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // nCreateCount++;
                new_issues.Add(new_issue);
                ref_issue = new_issue;
            }
            else
            {
                /*
                // 选定最后一个TreeNode
                Debug.Assert(this.TreeView.Nodes.Count != 0, "");
                TreeNode last_tree_node = this.TreeView.Nodes[this.TreeView.Nodes.Count - 1];

                if (this.TreeView.SelectedNode != last_tree_node)
                    this.TreeView.SelectedNode = last_tree_node;
                 * */
            }

            // int nWarningCount = 0;

            int nPreferredDelta = -1;

            // 进行循环，增补全部节点
            for (int i = 0; ; i++)
            {
                string strNextPublishTime = "";
                string strNextIssue = "";
                string strNextZong = "";
                string strNextVolume = "";

                {
                    int nIssueCount = 0;
                    // 获得一年内的期总数
                    // return:
                    //      -1  出错
                    //      0   无法获得
                    //      1   获得
                    nRet = GetOneYearIssueCount(ref_issue.PublishTime,
                        out nIssueCount,
                        out strError);

                    if (nRet == 0 && i == 0)
                    {
                        ref_issue = null;
                        goto REDO;
                    }

                    // 参考的期号
                    int nRefIssue = 0;
                    try
                    {
                        string strNumber = GetPureNumber(ref_issue.Issue);
                        nRefIssue = Convert.ToInt32(strNumber);
                    }
                    catch
                    {
                        nRefIssue = 0;
                    }


                    try
                    {
                        int nDelta = nPreferredDelta;
                        // 预测下一期的出版时间
                        // parameters:
                        //      strPublishTime  当前这一期出版时间
                        //      nIssueCount 一年内出多少期
                        strNextPublishTime = NextPublishTime(ref_issue.PublishTime,
                             nIssueCount,
                             ref nDelta);
                        // 第一次调用的时候记忆下来
                        if (nPreferredDelta == -1)
                            nPreferredDelta = nDelta;
                    }
                    catch (Exception ex)
                    {
                        // 2009/2/8 
                        strError = "在获得日期 '" + ref_issue.PublishTime + "' 的后一期出版日期时发生错误: " + ex.Message;
                        goto ERROR1;
                    }

                    if (strNextPublishTime == "????????")
                        break;

                    // 检查一下这个出版时间是否超过订购时间范围?
                    if (InOrderRange(strNextPublishTime) == false)
                        break;  // 避免最后多插入一个


                    // 号码自动增量需要知道一个期是否跨年，可以通过查询采购信息得到一年所订阅的期数
                    if (nRefIssue >= nIssueCount
                        && nIssueCount > 0) // 2010/3/3 
                    {
                        // 跨年了
                        strNextIssue = "1";
                        // 2010/3/16
                        // 如果预测的下一期出版时间不是参考期的后一年的时间，则需要强制修改
                        string strNextYear = dp2StringUtil.GetYearPart(strNextPublishTime);
                        string strRefYear = dp2StringUtil.GetYearPart(ref_issue.PublishTime);

                        // 2012/5/14
                        // 如果参考期所在年份的各期之间已经跨年，则不必作修正
                        // 试图找到参考期之前的第一期
                        string strRefFirstYear = "";
                        IssueBindingItem year_first_issue = GetYearFirstIssue(ref_issue);
                        if (year_first_issue != null)
                        {
                            strRefFirstYear = dp2StringUtil.GetYearPart(year_first_issue.PublishTime);
                        }

                        if (string.Compare(strNextYear, strRefYear) <= 0
                            && strRefYear == strRefFirstYear/*参考期所在的全年各期不跨年*/)
                        {
                            strNextYear = DateTimeUtil.NextYear(strRefYear);
                            strNextPublishTime = strNextYear + "0101";

                            // 2015/1/30
                            // 检查一下这个出版时间是否超过订购时间范围?
                            if (InOrderRange(strNextPublishTime) == false)
                                break;  // 避免最后多插入一个
                        }
                    }
                    else
                    {
                        strNextIssue = (nRefIssue + 1).ToString();
                    }

                    strNextZong = IncreaseNumber(ref_issue.Zong);
                    if (nRefIssue >= nIssueCount && nIssueCount > 0)
                        strNextVolume = IncreaseNumber(ref_issue.Volume);
                    else
                        strNextVolume = ref_issue.Volume;
                }

                // 对publishTime要查重，对号码体系要进行检查和提出警告
                List<IssueBindingItem> dup_issues = null;
                List<IssueBindingItem> warning_issues = null;
                string strWarning = "";

                // 对出版时间进行查重
                // parameters:
                //      exclude 检查中要排除的TreeNode对象
                // return:
                //      -1  error
                //      0   没有重
                //      1   重
                nRet = CheckPublishTimeDup(strNextPublishTime,
                    strNextIssue,
                    strNextZong,
                    "", // strNextVolume, 故意不检查卷号
                    ref_issue,
                    out warning_issues,
                    out strWarning,
                    out dup_issues,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet == 1)
                {
                    // this.TreeView.SelectedNode = dup_tree_node; // 若没有这一句会引起死循环
                    Debug.Assert(dup_issues.Count > 0, "");
                    ref_issue = dup_issues[0];  // 放弃创建，而改将发现的重复对象作为新的参考位置，继续创建
                    continue;
                }
                if (warning_issues.Count > 0)
                {
                    /// 2023/2/8
                    // 警告一次。可以防止死循环
                    {
                        DialogResult dialog_result =
                            MessageBox.Show(this,
    $"检查号码时发现问题: {strWarning}\r\n\r\n请问是否继续处理?",
    "BindingControls",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                        if (dialog_result == DialogResult.No)
                        {
                            strError = strWarning;
                            goto ERROR1;
                        }
                    }
                    Debug.Assert(warning_issues.Count > 0, "");
                    ref_issue = warning_issues[0];  // 放弃创建，而改将发现的重复对象作为新的参考位置，继续创建
                    continue;
                }

                IssueBindingItem new_issue = new IssueBindingItem();
                new_issue.Container = this;
                nRet = new_issue.Initial("<root />",
                    "",
                    false,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                new_issue.PublishTime = strNextPublishTime;
                new_issue.Issue = strNextIssue;
                new_issue.Zong = strNextZong;
                new_issue.Volume = strNextVolume;
                new_issue.RefID = Guid.NewGuid().ToString();

                new_issue.Changed = true;
                new_issue.NewCreated = true;

                /*
                // 设置或者刷新一个操作记载
                // 可能会抛出异常
                new_issue.SetOperation(
                    "create",
                    this.Operator,
                    "");
                 * */

                // 插入到合适的位置?
                InsertIssueToIssues(new_issue);

                // 为新增的期设置好Layout模式
                // 弥补因为插入带来的合订本裂断
                nRet = SetNewIssueLayout(new_issue,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // nCreateCount++;
                new_issues.Add(new_issue);
                /*
                // 选上新插入的节点
                this.TreeView.SelectedNode = tree_node;
                 * */
                ref_issue = new_issue;
            }

            if (new_issues.Count > 0)
            {
                this.ClearAllSelection();
                for (int i = 0; i < new_issues.Count; i++)
                {
                    new_issues[i].Select(SelectAction.On);
                }
                // new_issue.Select(SelectAction.On);
                // 本来需要UpdateObject()，但是因为后面有Invalidate()，就免了
                this.AfterWidthChanged(true);   // content高度改变
                this.Update();
                // Application.DoEvents();
            }

            string strMessage = "";
            if (new_issues.Count == 0)
                strMessage = "没有增加新的期行";
            else
                strMessage = "共新增了 " + new_issues.Count.ToString() + " 个期行";

            MessageBox.Show(this, strMessage);

            if (new_issues.Count > 0)
            {
                // TODO: 似乎SetScrollBars()没有必要了？
                try
                {
                    SetScrollBars(ScrollBarMember.Both);
                }
                catch
                {
                }
                this.EnsureVisible(new_issues[new_issues.Count - 1]);  // 最后一项可见

                string strLockedCellLibraryCodes = GetLockedCellLibraryCodes(new_issues);
                if (string.IsNullOrEmpty(strLockedCellLibraryCodes) == false)
                {
                    MessageBox.Show(this, "警告：下列馆代码不在当前用户管辖范围内: \r\n\r\n" + strLockedCellLibraryCodes + "\r\n\r\n使用了这些馆代码的格子已经处于锁定状态。当期记录提交保存的时候可能会遇到报错。建议使用全局用户登录后重新操作");
                }
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 警告超过管辖范围的格子
        string GetLockedCellLibraryCodes(List<IssueBindingItem> issues)
        {
            List<string> locked_librarycodes = new List<string>();
            for (int i = 0; i < issues.Count; i++)
            {
                IssueBindingItem issue = issues[i];

                if (issue.Virtual == true)
                    continue;

                for (int j = 0; j < issue.Cells.Count; j++)
                {
                    Cell cell = issue.Cells[j];
                    if (cell != null && cell.item != null)
                    {
                        if (cell.item.Locked == true)
                        {
                            locked_librarycodes.Add(Global.GetLibraryCode(cell.item.LocationString));
                        }
                    }
                }
            }

            if (locked_librarycodes.Count == 0)
                return "";

            StringUtil.RemoveDupNoSort(ref locked_librarycodes);
            return StringUtil.MakePathList(locked_librarycodes);
        }

        // 从右端开始，获得一段纯粹数字
        static string GetPureNumber(string strText)
        {
            string strValue = "";
            bool bStart = false;
            for (int i = strText.Length - 1; i >= 0; i--)
            {
                char ch = strText[i];
                if (bStart == false)
                {
                    if (ch >= '0' && ch <= '9')
                        bStart = true;
                }
                else
                {
                    if (!(ch >= '0' && ch <= '9'))
                        break;
                }

                if (bStart == true)
                    strValue = new string(ch, 1) + strValue;
            }

            return strValue;
        }

        IssueBindingItem GetTailIssue()
        {
            for (int i = this.Issues.Count - 1; i >= 0; i--)
            {
                IssueBindingItem issue = this.Issues[i];
                if (issue == null)
                    continue;
                if (String.IsNullOrEmpty(issue.PublishTime) == true)
                    continue;
                return issue;
            }

            return null;
        }

        // 新增一期
        // 在当前位置后面新增一期
        void menuItem_newIssue_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 从选择范围中挑出期对象。

            // TODO: 警告在this.SelectedCells中的选择?

            List<IssueBindingItem> ref_issues = this.SelectedIssues;
            /*
            if (ref_issues.Count == 0)
            {
                strError = "尚未选定要新增期的(参考)期对象";
                goto ERROR1;
            }
             * */
            // 整理一下ref_issues数组
            for (int i = 0; i < ref_issues.Count; i++)
            {
                IssueBindingItem ref_issue = ref_issues[i];
                if (ref_issue == null
                    || String.IsNullOrEmpty(ref_issue.PublishTime) == true)
                {
                    ref_issues.RemoveAt(i);
                    i--;
                    continue;
                }
            }

            List<IssueBindingItem> new_issues = new List<IssueBindingItem>();   // 新创建的期
            if (ref_issues.Count > 0)
            {
                // 有参考对象
                for (int i = 0; i < ref_issues.Count; i++)
                {
                    IssueBindingItem ref_issue = ref_issues[i];
                    if (ref_issue == null)
                    {
                        continue;
                    }

                    if (String.IsNullOrEmpty(ref_issue.PublishTime) == true)
                        continue;

                    IssueBindingItem new_issue = null;
                    // 新增一个期(后插)
                    nRet = NewOneIssue(
                        ref_issue,
                        false,
                        out new_issue,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    if (nRet == 0)
                        break;
                    new_issues.Add(new_issue);
                }
            }
            else
            {
                // 如果没有参考对象
                Debug.Assert(ref_issues.Count == 0, "");

                IssueBindingItem ref_issue = null;

                // 找到最后一个期对象作为参考对象
                /*
                if (this.Issues.Count > 0)
                {
                    for (int i = this.Issues.Count - 1; i >= 0; i--)
                    {
                        IssueBindingItem issue = this.Issues[i];
                        if (issue == null)
                            continue;
                        if (String.IsNullOrEmpty(issue.PublishTime) == true)
                            continue;
                        ref_issue = issue;
                        break;
                    }
                }
                 * */
                ref_issue = GetTailIssue();

                IssueBindingItem new_issue = null;
                // 新增一个期(后插)
                nRet = NewOneIssue(
                    ref_issue,
                    false,
                    out new_issue,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                if (nRet != 0)
                {
                    Debug.Assert(new_issue != null, "");
                    new_issues.Add(new_issue);
                }
            }

            if (new_issues.Count > 0)
            {
                // 选定新创建的期
                this.ClearAllSelection();
                for (int i = 0; i < new_issues.Count; i++)
                {
                    IssueBindingItem issue = new_issues[i];
                    issue.Select(SelectAction.On);
                }
                this.AfterWidthChanged(true);   // content高度改变

                this.EnsureVisible(new_issues[0]);  // 可见

                string strLockedCellLibraryCodes = GetLockedCellLibraryCodes(new_issues);
                if (string.IsNullOrEmpty(strLockedCellLibraryCodes) == false)
                {
                    MessageBox.Show(this, "警告：下列馆代码不在当前用户管辖范围内: \r\n\r\n" + strLockedCellLibraryCodes + "\r\n\r\n使用了这些馆代码的格子已经处于锁定状态。当期记录提交保存的时候可能会遇到报错。建议使用全局用户登录后重新操作");
                }
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 选中若干期格子
        public void SelectIssues(List<IssueBindingItem> issues,
            bool bEnsureVisible)
        {
            Debug.Assert(issues != null, "");
            this.ClearAllSelection();
            List<CellBase> cells = new List<CellBase>();
            for (int i = 0; i < issues.Count; i++)
            {
                cells.Add((CellBase)issues[i]);
                issues[i].Select(SelectAction.On);
            }

            this.UpdateObjects(cells);

            if (bEnsureVisible == true)
                this.EnsureVisible(issues[0]);  // 确保滚入视野

            // this.Update();
        }

        static int CompareDateString(string strPublishTime1,
            string strPublishTime2)
        {
            return string.Compare(strPublishTime1, strPublishTime2);
        }

        // 新增一个期(后插)
        // return:
        //      -1  出错
        //      0   放弃
        //      1   成功
        int NewOneIssue(
            IssueBindingItem ref_issue,
            bool bUpdateDisplay,
            out IssueBindingItem new_issue,
            out string strError)
        {
            strError = "";
            new_issue = null;
            int nRet = 0;

            IssueDialog dlg = new IssueDialog();
            MainForm.SetControlFont(dlg, this.Font, false);
            dlg.Tag = null;
            dlg.CheckDup -= new CheckDupEventHandler(dlg_CheckDup);
            dlg.CheckDup += new CheckDupEventHandler(dlg_CheckDup);

            if (ref_issue == null)
            {
                // 没有参考对象?
                // 找到第一个还没有记到的订购时间范围的开端
                // 获得第一个未记到的订购范围的起始时间
                string strFirstPublishTime = "";
                // return:
                //      -1  出错
                //      0   无法获得
                //      1   获得
                nRet = GetFirstUseablePublishTime(
                    out strFirstPublishTime,
                    out strError);
                if (nRet == -1)
                    return -1;
                if (nRet == 1)
                    dlg.PublishTime = strFirstPublishTime;

                if (String.IsNullOrEmpty(strFirstPublishTime) == false)
                {
                    int nIssueCount = 0;
                    // 获得一年内的期总数
                    // return:
                    //      -1  出错
                    //      0   无法获得
                    //      1   获得
                    nRet = GetOneYearIssueCount(strFirstPublishTime,
                        out nIssueCount,
                        out strError);
                    if (nIssueCount > 0)
                        dlg.EditComment = "一年出版 " + nIssueCount.ToString() + " 期";
                    else
                        dlg.EditComment = "无法得知此年出版的期数。没有对应的订购信息";
                }

                // TODO: 预测出第一期的当年期号
                // 算法是，根据一年的期数，观察起始时间，分布在当年的比例位置，估算期号
            }

            if (ref_issue != null)
            {
                // TODO: 最好能自动增量

                int nIssueCount = 0;
                // 获得一年内的期总数
                // return:
                //      -1  出错
                //      0   无法获得
                //      1   获得
                nRet = GetOneYearIssueCount(ref_issue.PublishTime,
                    out nIssueCount,
                    out strError);


                int nRefIssue = 0;
                try
                {
                    string strNumber = GetPureNumber(ref_issue.Issue);
                    nRefIssue = Convert.ToInt32(strNumber);
                }
                catch
                {
                    nRefIssue = 0;
                }

                bool bGuestNumbers = false;
                string strNextPublishTime = "";

                if (nRet == 0)
                {
                    string strFirstPublishTime = "";
                    // return:
                    //      -1  出错
                    //      0   无法获得
                    //      1   获得
                    nRet = GetFirstUseablePublishTime(
                        out strFirstPublishTime,
                        out strError);
                    if (nRet == -1)
                        return -1;
                    if (nRet == 1)
                    {
                        // TODO: 要比较 ref_issue.PublishTime 和 strFirstPublishTime
                        // 如果正好后者更大，即可采纳。否则不宜采纳
                        if (CompareDateString(ref_issue.PublishTime, strFirstPublishTime) < 0)
                        {
                            nRet = GetOneYearIssueCount(strFirstPublishTime,
                                out nIssueCount,
                                out strError);
                            strNextPublishTime = strFirstPublishTime;
                            bGuestNumbers = true;
                        }
                    }
                }
                else
                {

                    try
                    {
                        // 预测下一期的出版时间
                        // parameters:
                        //      strPublishTime  当前这一期出版时间
                        //      nIssueCount 一年内出多少期
                        strNextPublishTime = NextPublishTime(ref_issue.PublishTime,
                             nIssueCount);
                    }
                    catch (Exception ex)
                    {
                        // 2009/2/8 
                        strError = "在获得日期 '" + ref_issue.PublishTime + "' 的后一期出版日期时发生错误: " + ex.Message;
                        return -1;
                    }
                }

                dlg.PublishTime = strNextPublishTime;

                // 号码自动增量需要知道一个期是否跨年，可以通过查询采购信息得到一年所订阅的期数
                if (nRefIssue >= nIssueCount
                    && nIssueCount > 0) // 2010/3/3 
                {
                    // 跨年了
                    dlg.Issue = "1";



                    // 2010/3/16
                    // 如果预测的下一期出版时间不是参考期的后一年的时间，则需要强制修改
                    string strNextYear = dp2StringUtil.GetYearPart(strNextPublishTime);
                    string strRefYear = dp2StringUtil.GetYearPart(ref_issue.PublishTime);

                    // 2012/5/14
                    // 如果参考期所在年份的各期之间已经跨年，则不必作修正
                    // 试图找到参考期之前的第一期
                    string strRefFirstYear = "";
                    IssueBindingItem year_first_issue = GetYearFirstIssue(ref_issue);
                    if (year_first_issue != null)
                    {
                        strRefFirstYear = dp2StringUtil.GetYearPart(year_first_issue.PublishTime);
                    }

                    if (string.Compare(strNextYear, strRefYear) <= 0
                        && strRefYear == strRefFirstYear/*参考期所在的全年各期不跨年*/)
                    {
                        strNextYear = DateTimeUtil.NextYear(strRefYear);
                        strNextPublishTime = strNextYear + "0101";
                        dlg.PublishTime = strNextPublishTime;
                    }
                }
                else
                {
                    dlg.Issue = (nRefIssue + 1).ToString();
                }

                dlg.Zong = IncreaseNumber(ref_issue.Zong);
                if (nRefIssue >= nIssueCount && nIssueCount > 0)
                    dlg.Volume = IncreaseNumber(ref_issue.Volume);
                else
                    dlg.Volume = ref_issue.Volume;

                if (nIssueCount > 0)
                    dlg.EditComment = "一年出版 " + nIssueCount.ToString() + " 期";
                else
                    dlg.EditComment = "无法得知一年出版的期数。没有对应的订购信息";

                if (bGuestNumbers == true)
                {
                    if (String.IsNullOrEmpty(dlg.Issue) == false)
                        dlg.Issue += "?";
                    if (String.IsNullOrEmpty(dlg.Volume) == false)
                        dlg.Volume += "?";
                    if (String.IsNullOrEmpty(dlg.Zong) == false)
                        dlg.Zong += "?";
                }
            }

            dlg.StartPosition = FormStartPosition.CenterScreen;

            // REDO_INPUT:
            dlg.ShowDialog(this);

            if (dlg.DialogResult != DialogResult.OK)
                return 0;
            /*
            // 对publishTime要查重，对号码体系要进行检查和提出警告
            IssueBindingItem dup_issue = null;
            // 对出版时间进行查重
            // parameters:
            //      exclude 检查中要排除的TreeNode对象
            // return:
            //      -1  error
            //      0   没有重
            //      1   重
            nRet = CheckPublishTimeDup(dlg.PublishTime,
                null,
                out dup_issue,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 1)
            {
                // 选中所重复的期格子，便于操作者观察重复的情况
                Debug.Assert(dup_issue != null, "");
                if (dup_issue != null)
                {
                    this.ClearAllSelection();
                    dup_issue.Select(SelectAction.On);
                    this.EnsureVisible(dup_issue);  // 确保滚入视野
                    this.UpdateObject(dup_issue);
                    this.Update();
                }

                MessageBox.Show(this, "拟新增的期行 " + strError + "\r\n请修改。");
                goto REDO_INPUT;
            }
             * */

            new_issue = new IssueBindingItem();
            new_issue.Container = this;
            nRet = new_issue.Initial("<root />",
                "",
                false, //?
                out strError);
            if (nRet == -1)
                return -1;

            new_issue.PublishTime = dlg.PublishTime;
            new_issue.Issue = dlg.Issue;
            new_issue.Zong = dlg.Zong;
            new_issue.Volume = dlg.Volume;
            new_issue.Comment = dlg.Comment;
            new_issue.RefID = Guid.NewGuid().ToString();
            // TODO: 获得验收新增期的批次号。或者沿用验收册的批次号?

            new_issue.Changed = true;
            new_issue.NewCreated = true;

            /*
            // 设置或者刷新一个操作记载
            // 可能会抛出异常
            new_issue.SetOperation(
                "create",
                this.Operator,
                "");
             * */

            // 插入到合适的位置?
            InsertIssueToIssues(new_issue);

            // 为新增的期设置好Layout模式
            // 弥补因为插入带来的合订本裂断
            nRet = SetNewIssueLayout(new_issue,
                out strError);
            if (nRet == -1)
                return -1;

            // 选上新插入的节点
            if (bUpdateDisplay == true)
            {
                this.ClearAllSelection();
                new_issue.Select(SelectAction.On);
                // 本来需要UpdateObject()，但是因为后面有Invalidate()，就免了
                this.AfterWidthChanged(true);   // content高度改变
            }
            return 1;
        }

        // 找到参考期前方当年第一期的节点
        IssueBindingItem GetYearFirstIssue(IssueBindingItem ref_issue)
        {
            IssueBindingItem first = null;
            IssueBindingItem tail = null;
            for (int i = 0; i < this.Issues.Count; i++)
            {
                IssueBindingItem issue = this.Issues[i];
                if (String.IsNullOrEmpty(issue.PublishTime) == true)
                    continue;
                // TODO: 注意期号不连续的情况
                if (IsFirstNumber(issue.Issue))
                    first = issue;
                if (issue == ref_issue)
                {
                    tail = issue;
                    break;
                }
            }

            if (tail == null)
                return null;

            return first;
        }

        static bool IsFirstNumber(string strNumber)
        {
            strNumber = strNumber.TrimStart(new char[] { ' ', '0' });
            if (strNumber == "1")
                return true;
            return false;
        }

        // 查重
        void dlg_CheckDup(object sender, CheckDupEventArgs e)
        {
            IssueDialog dialog = (IssueDialog)sender;
            List<IssueBindingItem> warning_issues = null;
            List<IssueBindingItem> dup_issues = null;
            string strDup = "";
            string strWarning = "";
            int nRet = this.CheckPublishTimeDup(
                e.PublishTime,
                e.Issue,
                e.Zong,
                e.Volume,
                (IssueBindingItem)dialog.Tag,
                out warning_issues,
                out strWarning,
                out dup_issues,
                out strDup);
            e.WarningInfo = strWarning;
            e.WarningIssues = warning_issues;
            e.DupInfo = strDup;
            e.DupIssues = dup_issues;

            if (e.EnsureVisible == true)
            {
                this.ClearAllSelection();
                if (dup_issues.Count > 0)
                {
                    this.SelectIssues(dup_issues, true);
                    return;
                }

                if (warning_issues.Count > 0)
                {
                    this.SelectIssues(warning_issues, true);
                }
            }
        }

        // 清除当前所有选择
        // TODO: 查找到类似的多行代码，简化为函数调用
        public void ClearAllSelection()
        {
            List<CellBase> objects = new List<CellBase>();
            this.ClearAllSubSelected(ref objects, 100);
            if (objects.Count >= 100)
                this.Invalidate();
            else
            {
                // 这个方法屏幕不抖动
                UpdateObjects(objects);
            }
        }

        #region --- 和新增期有关的函数 ---

        // 获得可用的最大订购时间范围
        // return:
        //      -1  error
        //      0   not found
        //      1   found
        int GetMaxOrderRange(out string strStartDate,
            out string strEndDate,
            out string strError)
        {
            strStartDate = "";
            strEndDate = "";
            strError = "";

            if (this.GetOrderInfo == null)
                return 0;

            GetOrderInfoEventArgs e1 = new GetOrderInfoEventArgs();
            e1.BiblioRecPath = "";
            e1.PublishTime = "*";
            e1.LibraryCodeList = this.LibraryCodeList;
            this.GetOrderInfo(this, e1);
            if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
            {
                strError = e1.ErrorInfo;
                return -1;
            }

            if (e1.OrderXmls.Count == 0)
                return 0;

            for (int i = 0; i < e1.OrderXmls.Count; i++)
            {
                string strXml = e1.OrderXmls[i];

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "订购XML装入DOM时发生错误: " + ex.Message;
                    return -1;
                }

                string strRange = DomUtil.GetElementText(dom.DocumentElement,
                    "range");

                string strIssueCount = DomUtil.GetElementText(dom.DocumentElement,
                    "issueCount");

                int nIssueCount = 0;
                try
                {
                    nIssueCount = Convert.ToInt32(strIssueCount);
                }
                catch
                {
                    continue;
                }

                int nRet = strRange.IndexOf("-");
                if (nRet == -1)
                {
                    strError = "时间范围 '" + strRange + "' 格式错误，缺乏-";
                    return -1;
                }

                string strStart = strRange.Substring(0, nRet).Trim();
                string strEnd = strRange.Substring(nRet + 1).Trim();

                if (strStart.Length != 8)
                {
                    strError = "时间范围 '" + strRange + "' 格式错误，左边部分字符数不为8";
                    return -1;
                }
                if (strEnd.Length != 8)
                {
                    strError = "时间范围 '" + strRange + "' 格式错误，右边部分字符数不为8";
                    return -1;
                }

                if (strStartDate == "")
                    strStartDate = strStart;
                else
                {
                    if (String.Compare(strStartDate, strStart) > 0)
                        strStartDate = strStart;
                }

                if (strEndDate == "")
                    strEndDate = strEnd;
                else
                {
                    if (String.Compare(strEndDate, strEnd) < 0)
                        strEndDate = strEnd;
                }
            }

            if (strStartDate == "")
            {
                Debug.Assert(strEndDate == "", "");
                return 0;
            }

            return 1;
        }

        // 检测一个出版时间是否在已经订购的范围内
        bool InOrderRange(string strPublishTime)
        {
            if (this.GetOrderInfo == null)
                return false;

            GetOrderInfoEventArgs e1 = new GetOrderInfoEventArgs();
            e1.BiblioRecPath = "";
            e1.PublishTime = strPublishTime;
            e1.LibraryCodeList = this.LibraryCodeList;
            this.GetOrderInfo(this, e1);
            if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
                return false;

            if (e1.OrderXmls.Count == 0)
                return false;

            return true;
        }

        // 将一个新的Issue对象插入this.Issues适当的位置
        // 注意，调用前，并未插入this.Issues
        void InsertIssueToIssues(IssueBindingItem issueInsert)
        {
            int nFreeIndex = -1;    // 自由期所在位置
            int nInsertIndex = -1;
            string strPublishTime = issueInsert.PublishTime;
            string strPrevPublishTime = "";
            for (int i = 0; i < this.Issues.Count; i++)
            {
                IssueBindingItem issue = this.Issues[i];
                if (issue == null)
                {
                    Debug.Assert(false, "");
                    continue;
                }
                if (String.IsNullOrEmpty(issue.PublishTime) == true)
                {
                    nFreeIndex = i;
                    continue;
                }

                if (issue == issueInsert)
                {
                    throw new Exception("要插入的期在调用函数前已经在数组里面了");
                }

                if (String.Compare(strPublishTime, strPrevPublishTime) >= 0
    && String.Compare(strPublishTime, issue.PublishTime) < 0)
                    nInsertIndex = i;

                strPrevPublishTime = issue.PublishTime;
            }

            if (nInsertIndex == -1)
            {
                // 插入在自由期之前
                if (nFreeIndex != -1)
                {
                    Debug.Assert(this.Issues.Count > 0, "");
                    if (nFreeIndex == this.Issues.Count - 1)
                    {
                        this.Issues.Insert(nFreeIndex, issueInsert);
                        return;
                    }
                }

                this.Issues.Add(issueInsert);
            }
            else
                this.Issues.Insert(nInsertIndex, issueInsert);
        }

        // 为新增的期设置好Layout模式
        int SetNewIssueLayout(IssueBindingItem issue,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            IssueBindingItem prev_issue = null;
            IssueBindingItem next_issue = null;
            int nLineNo = this.Issues.IndexOf(issue);
            if (nLineNo == -1)
            {
                Debug.Assert(false, "");
                strError = "issue not found in this.Issues";
                return -1;
            }

            if (nLineNo > 0)
            {
                prev_issue = this.Issues[nLineNo - 1];
                // 排除自由期
                if (String.IsNullOrEmpty(prev_issue.PublishTime) == true)
                    prev_issue = null;
            }

            if (nLineNo < this.Issues.Count - 1)
            {
                next_issue = this.Issues[nLineNo + 1];
                // 排除自由期
                if (String.IsNullOrEmpty(next_issue.PublishTime) == true)
                    next_issue = null;
            }

            // 在此范围内，如果发生获取订购信息的请求，则扭转为从管辖范围获取
            bool bOld = m_bForceNarrowRange;
            m_bForceNarrowRange = true;
            try
            {

                bool bCross = IsIssueInExistingBoundRange(issue);
                if (bCross == true
                    || (prev_issue != null && prev_issue.IssueLayoutState == IssueLayoutState.Binding)
                    || (prev_issue != null && prev_issue.IssueLayoutState == IssueLayoutState.Binding)
                    )
                {
                    // 如果当前期为断裂了合订册的情况，并且前一个期或者后一个期至少有一个是Binding Layout，
                    // 那么就设置为BindingLayout
                    nRet = issue.ReLayoutBinding(out strError);
                    if (nRet == -1)
                        return -1;
                    issue.IssueLayoutState = IssueLayoutState.Binding;
                    return 0;
                }

                // 否则设置为Accepting Layout
                nRet = issue.LayoutAccepting(out strError);
                if (nRet == -1)
                    return -1;
                issue.IssueLayoutState = IssueLayoutState.Accepting;
                return 0;

            }
            finally
            {
                m_bForceNarrowRange = bOld;
            }
        }

        // 找到一个合订册的在Binding布局下的已经采纳的列号。注意，是双格的左侧格子
        internal int FindExistBoundCol(ItemBindingItem parent_item,
            IssueBindingItem exclude_issue)
        {
            Debug.Assert(parent_item != null, "");
            for (int i = 0; i < this.Issues.Count; i++)
            {
                IssueBindingItem issue = this.Issues[i];
                if (issue == null)
                {
                    Debug.Assert(false, "");
                    continue;
                }
                if (String.IsNullOrEmpty(issue.PublishTime) == true)
                    continue;
                if (issue == exclude_issue)
                    continue;
                if (issue.IssueLayoutState == IssueLayoutState.Accepting)
                    continue;
                int nCol = issue.IndexOfItem(parent_item);
                if (nCol != -1)
                    return nCol;
                for (int j = 0; j < parent_item.MemberCells.Count; j++)
                {
                    Cell cell = parent_item.MemberCells[j];
                    if (cell == null)
                        continue;
                    nCol = issue.IndexOfCell(cell);
                    if (nCol != -1)
                    {
                        Debug.Assert(nCol != 0, "成员格子不应在0列出现");
                        return nCol - 1;
                    }
                }
            }

            return -1;  // not found
        }


        // 检查一个期对象的纵向所在位置是否穿越了现有的合订时间范围
        // 得到那些被穿越的合订册对象
        // parameters:
        //      bOnlyDetect     是否仅仅检测，不返回具体的parent item?
        //      parent_items    返回被穿越的合订册对象
        internal int GetCrossBoundRange(IssueBindingItem issueTest,
            bool bOnlyDetect,
            out List<ItemAndCol> cross_infos)
        {
            cross_infos = new List<ItemAndCol>();

            // 遍历合订册对象数组
            for (int i = 0; i < this.ParentItems.Count; i++)
            {
                ItemBindingItem parent_item = this.ParentItems[i];

                IssueBindingItem issue = parent_item.Container; // 假装属于这个期
                Debug.Assert(issue != null, "合订册的 Container 不应该为空。正确的处理方式是把这样的册放入自由期管辖范围");

                // 找到行号
                int nStartLineNo = this.Issues.IndexOf(issue);
                Debug.Assert(nStartLineNo != -1, "");
                if (nStartLineNo == -1)
                    continue;

                /*
                int nCol = issue.IndexOfItem(parent_item);
                Debug.Assert(nCol != -1, "");
                 * */

                // 看看垂直方向包含多少个期
                int nIssueCount = 0;
                if (parent_item.MemberCells.Count == 0)
                    nIssueCount = 1;
                else
                {
                    // TODO: 要保证item.MemberCells数组中对象是有序的
                    IssueBindingItem tail_issue = parent_item.MemberCells[parent_item.MemberCells.Count - 1].Container;// item.MemberItems[item.MemberItems.Count - 1].Container;
                    Debug.Assert(tail_issue != null, "");
                    // 找到行号
                    int nTailLineNo = this.Issues.IndexOf(tail_issue);
                    Debug.Assert(nTailLineNo != -1, "");
                    if (nTailLineNo == -1)
                        continue;

                    nIssueCount = nTailLineNo - nStartLineNo + 1;
                }

                int nTestLineNo = this.Issues.IndexOf(issueTest);
                if (nTestLineNo == -1)
                {
                    Debug.Assert(false, "");
                    continue;
                }

                if (nTestLineNo >= nStartLineNo && nTestLineNo < nStartLineNo + nIssueCount)
                {
                    if (bOnlyDetect == true)
                        return 1;
                    ItemAndCol info = new ItemAndCol();
                    info.item = parent_item;
                    cross_infos.Add(info);

                    // 找到当前正在用的列号。双格的左侧
                    info.Index = FindExistBoundCol(parent_item,
                        issueTest);
                }
            }

            return cross_infos.Count;
        }

        // 检查一个期对象的纵向所在位置是否穿越了现有的合订时间范围
        internal bool IsIssueInExistingBoundRange(IssueBindingItem issueTest)
        {
            List<ItemAndCol> infos = null;
            if (this.GetCrossBoundRange(issueTest,
                true,
                out infos) > 0)
                return true;
            return false;
        }

#if NOOOOOOOOOOOOOOO
        // 检查一个期对象的纵向所在位置是否穿越了现有的合订时间范围
        internal bool IsIssueInExistingBoundRange(IssueBindingItem issueTest)
        {
            // 遍历合订册对象数组
            for (int i = 0; i < this.ParentItems.Count; i++)
            {
                ItemBindingItem parent_item = this.ParentItems[i];

                IssueBindingItem issue = parent_item.Container; // 假装属于这个期
                Debug.Assert(issue != null, "");

                // 找到行号
                int nStartLineNo = this.Issues.IndexOf(issue);
                Debug.Assert(nStartLineNo != -1, "");
                if (nStartLineNo == -1)
                    continue;

                /*
                int nCol = issue.IndexOfItem(parent_item);
                Debug.Assert(nCol != -1, "");
                 * */

                // 看看垂直方向包含多少个期
                int nIssueCount = 0;
                if (parent_item.MemberCells.Count == 0)
                    nIssueCount = 1;
                else
                {
                    // TODO: 要保证item.MemberCells数组中对象是有序的
                    IssueBindingItem tail_issue = parent_item.MemberCells[parent_item.MemberCells.Count - 1].Container;// item.MemberItems[item.MemberItems.Count - 1].Container;
                    Debug.Assert(tail_issue != null, "");
                    // 找到行号
                    int nTailLineNo = this.Issues.IndexOf(tail_issue);
                    Debug.Assert(nTailLineNo != -1, "");
                    if (nTailLineNo == -1)
                        continue;

                    nIssueCount = nTailLineNo - nStartLineNo + 1;
                }

                int nTestLineNo = this.Issues.IndexOf(issueTest);
                if (nTestLineNo == -1)
                {
                    Debug.Assert(false, "");
                    continue;
                }

                if (nTestLineNo >= nStartLineNo && nTestLineNo <= nStartLineNo + nIssueCount)
                    return true;
            }

            return false;
        }
#endif

        // 对出版时间、当年期号、卷号进行查重
        // parameters:
        //      exclude 检查中要排除的TreeNode对象
        // return:
        //      -1  error
        //      0   没有重
        //      1   重
        int CheckPublishTimeDup(string strPublishTime,
            string strIssue,
            string strZong,
            string strVolume,
            IssueBindingItem exclude,
            out List<IssueBindingItem> warning_issues,
            out string strWarning,
            out List<IssueBindingItem> dup_issues,
            out string strError)
        {
            strWarning = "";
            strError = "";
            warning_issues = new List<IssueBindingItem>();
            dup_issues = new List<IssueBindingItem>();

            if (String.IsNullOrEmpty(strPublishTime) == true)
            {
                strError = "出版时间不能为空";
                return -1;
            }

            string strCurYear = dp2StringUtil.GetYearPart(strPublishTime);

            for (int i = 0; i < this.Issues.Count; i++)
            {
                IssueBindingItem issue = this.Issues[i];

                if (String.IsNullOrEmpty(issue.PublishTime) == true)
                    continue;

                if (issue == exclude)
                    continue;

                // 检查出版时间
                if (issue.PublishTime == strPublishTime)
                {
                    if (String.IsNullOrEmpty(strError) == false)
                        strError += ";\r\n";
                    strError += "出版时间 '" + strPublishTime + "' 和位置 " + (i + 1).ToString() + " (从1计数)的期重复了";
                    dup_issues.Add(issue);
                }

                // 在当年范围内检查当年期号、在其他年范围内检查卷号
                {
                    string strYear = dp2StringUtil.GetYearPart(issue.PublishTime);
                    if (strYear == strCurYear)
                    {
                        if (strIssue == issue.Issue
                        && String.IsNullOrEmpty(strIssue) == false)
                        {
                            if (String.IsNullOrEmpty(strWarning) == false)
                                strWarning += ";\r\n";
                            strWarning = "期号 '" + strIssue + "' 和位置 " + (i + 1).ToString() + " (从1计数)的期重复了";
                            warning_issues.Add(issue);
                        }
                    }
                    else if (strYear != strCurYear)
                    {
                        if (strVolume == issue.Volume
                            && String.IsNullOrEmpty(strVolume) == false)
                        {
                            if (String.IsNullOrEmpty(strWarning) == false)
                                strWarning += ";\r\n";
                            strWarning = "卷号 '" + strVolume + "' 和位置 " + (i + 1).ToString() + " (从1计数)的期重复了";
                            warning_issues.Add(issue);
                        }
                    }
                }


                // 检查总期号
                if (String.IsNullOrEmpty(strZong) == false)
                {
                    if (strZong == issue.Zong)
                    {
                        if (String.IsNullOrEmpty(strWarning) == false)
                            strWarning += ";\r\n";
                        strWarning = "总期号 '" + strZong + "' 和位置 " + (i + 1).ToString() + " (从1计数)的期重复了";
                        warning_issues.Add(issue);
                    }
                }
            }

            if (dup_issues.Count > 0)
                return 1;

            return 0;
        }

        static string IncreaseNumber(string strText)
        {
            string strNumber = GetPureNumber(strText);

            int v = 0;
            try
            {
                v = Convert.ToInt32(strNumber);
            }
            catch
            {
                return "";  // 避免查重查到自己   // 增量失败    strNumber
            }
            return (v + 1).ToString();
        }

        static string CanonicalizeLong8TimeString(string strPublishTime)
        {
            if (strPublishTime.Length == 4)
                return strPublishTime + "0101";
            if (strPublishTime.Length == 6)
                return strPublishTime + "01";
            return strPublishTime;
        }

        class PartOfMonth
        {
            public string StartDate = "";
            public string EndDate = "";

            public PartOfMonth(string strStartDate, string strEndDate)
            {
                Debug.Assert(strStartDate.Length == 8, "");
                Debug.Assert(strEndDate.Length == 8, "");
                this.StartDate = strStartDate;
                this.EndDate = strEndDate;

                // TODO: 验证两个日期都要在同一个月以内
            }

            public PartOfMonth(DateTime start, DateTime end)
            {
                this.StartDate = DateTimeUtil.DateTimeToString8(start);
                this.EndDate = DateTimeUtil.DateTimeToString8(end);
                // TODO: 验证两个日期都要在同一个月以内
            }
        }

        // 在若干部位中定位一个日期，并得到在部分以内的偏移量 delta
        static void LocationPart(List<PartOfMonth> parts,
            string strPublishTime,
            out int index,
            out int delta)
        {
            index = 0;
            delta = 0;
            foreach (PartOfMonth part in parts)
            {
                if (string.Compare(strPublishTime, part.StartDate) >= 0
                    && string.Compare(strPublishTime, part.EndDate) <= 0)
                {
                    DateTime range_start = DateTimeUtil.Long8ToDateTime(part.StartDate);
                    DateTime publish_time = DateTimeUtil.Long8ToDateTime(strPublishTime);
                    delta = (int)(publish_time - range_start).TotalDays;
                    return;
                }

                index++;
            }

            index = -1; // not found
            return;
        }

        // parameters:
        //      strEndTime  限定，不超过这个日期
        static string AddDays(string strPublishTime, int days, string strEndTime)
        {
            string strResult = DateTimeUtil.DateTimeToString8(
                DateTimeUtil.Long8ToDateTime(strPublishTime).AddDays(days)
                );
            if (string.Compare(strResult, strEndTime) > 0)
                return strEndTime;
            return strResult;
        }

        static string AddDays(string strPublishTime, int days)
        {
            return DateTimeUtil.DateTimeToString8(
                DateTimeUtil.Long8ToDateTime(strPublishTime).AddDays(days)
                );
        }

        static List<PartOfMonth> GetMonthParts(string strPublishTime, int nCount)
        {
            return GetMonthParts(DateTimeUtil.Long8ToDateTime(strPublishTime), nCount);
        }

        // 把一个月均匀划分为几个部分
        static List<PartOfMonth> GetMonthParts(DateTime time, int nCount)
        {
            List<PartOfMonth> results = new List<PartOfMonth>();
            // DateTime time = DateTimeUtil.Long8ToDateTime(strPublishTime);
            if (nCount == 1)
            {
                results.Add(
                    new PartOfMonth(
                    new DateTime(time.Year, time.Month, 1),
                    new DateTime(time.Year, time.Month, 1).AddMonths(1).AddDays(-1)
                    )
                    );
                return results;
            }
            if (nCount == 2)
            {
                // 1-15
                results.Add(
                    new PartOfMonth(
                    new DateTime(time.Year, time.Month, 1),
                    new DateTime(time.Year, time.Month, 15)
                    )
                    );
                // 16-end
                results.Add(
                    new PartOfMonth(
                    new DateTime(time.Year, time.Month, 16),
                    new DateTime(time.Year, time.Month, 1).AddMonths(1).AddDays(-1)
                    )
                    );
                return results;
            }
            if (nCount == 3)
            {
                // 1-10
                results.Add(
                    new PartOfMonth(
                    new DateTime(time.Year, time.Month, 1),
                    new DateTime(time.Year, time.Month, 10)
                    )
                    );
                // 11-20
                results.Add(
                    new PartOfMonth(
                    new DateTime(time.Year, time.Month, 11),
                    new DateTime(time.Year, time.Month, 20)
                    )
                    );
                // 21-end
                results.Add(
                    new PartOfMonth(
                    new DateTime(time.Year, time.Month, 21),
                    new DateTime(time.Year, time.Month, 1).AddMonths(1).AddDays(-1)
                    )
                    );
                return results;
            }
            if (nCount == 4)
            {
                // 1-7
                results.Add(
                    new PartOfMonth(
                    new DateTime(time.Year, time.Month, 1),
                    new DateTime(time.Year, time.Month, 7)
                    )
                    );
                // 8-15
                results.Add(
                    new PartOfMonth(
                    new DateTime(time.Year, time.Month, 8),
                    new DateTime(time.Year, time.Month, 15)
                    )
                    );
                // 16-22
                results.Add(
                    new PartOfMonth(
                    new DateTime(time.Year, time.Month, 16),
                    new DateTime(time.Year, time.Month, 22)
                    )
                    );
                // 23-end
                results.Add(
                    new PartOfMonth(
                    new DateTime(time.Year, time.Month, 23),
                    new DateTime(time.Year, time.Month, 1).AddMonths(1).AddDays(-1)
                    )
                    );
                return results;
            }

            throw new Exception("暂不支持一个月内 " + nCount.ToString() + " 个分割");
        }

        // 包装后的版本
        public static string NextPublishTime(string strPublishTime,
            int nIssueCount)
        {
            int nPreferredDelta = -1;
            return NextPublishTime(strPublishTime,
                nIssueCount,
                ref nPreferredDelta);
        }

        // 2016/1/6
        // 获得一年的天数。2016 年为 366 天
        static int GetDaysOfYear(string strYear)
        {
            DateTime start = DateTimeUtil.Long8ToDateTime(strYear + "0101");
            string strNextYear = (Int32.Parse(strYear) + 1).ToString().PadLeft(4, '0');
            DateTime end = DateTimeUtil.Long8ToDateTime(strNextYear + "0101");
            return (int)((end - start).TotalDays);
        }

        // 预测下一期的出版时间
        // exception:
        //      可能因strPublishTime为不可能的日期而抛出异常
        // parameters:
        //      strPublishTime  当前这一期出版时间
        //      nIssueCount 一年内出多少期
        //      nDelta  推荐的组内偏移天数。调用前如果为 -1，表示不使用这个参数。调用后返回本次得到的偏移
        public static string NextPublishTime(string strPublishTime,
            int nIssueCount,
            ref int nPreferredDelta)
        {
            strPublishTime = CanonicalizeLong8TimeString(strPublishTime);

            // 计算出一年有多少天。比如 2016 年就是 366 天而不是 365 天
            int nDaysOfYear = GetDaysOfYear(strPublishTime.Substring(0, 4));

            DateTime start = DateTimeUtil.Long8ToDateTime(strPublishTime);

            int nCount = 0;

            // 一年一期
            if (nIssueCount == 1)
            {
                return DateTimeUtil.DateTimeToString8(DateTimeUtil.NextYear(start));
            }

            // 一年两期
            else if (nIssueCount == 2)
            {
                // 6个月以后的同日
                for (int i = 0; i < 6; i++)
                {
                    start = DateTimeUtil.NextMonth(start);
                }

                return DateTimeUtil.DateTimeToString8(start);
            }

            // 一年三期
            else if (nIssueCount == 3)
            {
                // 4个月以后的同日
                for (int i = 0; i < 4; i++)
                {
                    start = DateTimeUtil.NextMonth(start);
                }

                return DateTimeUtil.DateTimeToString8(start);
            }

            // 一年4期
            else if (nIssueCount == 4)
            {
                // 3个月以后的同日
                for (int i = 0; i < 3; i++)
                {
                    start = DateTimeUtil.NextMonth(start);
                }

                return DateTimeUtil.DateTimeToString8(start);
            }

            // 一年5期 和一年6期处理办法一样
            // 一年6期
            else if (nIssueCount == 5 || nIssueCount == 6)
            {
                // 
                // 2个月以后的同日
                for (int i = 0; i < 2; i++)
                {
                    start = DateTimeUtil.NextMonth(start);
                }

                return DateTimeUtil.DateTimeToString8(start);
            }

            // 一年7/8/9/10/11期 和一年12期处理办法一样
            // 一年12期
            else if (nIssueCount >= 7 && nIssueCount <= 12)
            {
                // 1个月以后的同日
                start = DateTimeUtil.NextMonth(start);

                return DateTimeUtil.DateTimeToString8(start);
            }

            // 一年13期 可能是错误的时间范围造成的
            // 和一年12期处理办法一样
            else if (nIssueCount == 13)
            {

                // 12月放两期
                if (start.Month == 12)
                {
                    // 15天以后
                    start += new TimeSpan(15, 0, 0, 0);
                    return DateTimeUtil.DateTimeToString8(start);
                }

                // 1个月以后的同日
                start = DateTimeUtil.NextMonth(start);

                return DateTimeUtil.DateTimeToString8(start);
            }


            // 一年24期
            else if (nIssueCount > 13 && nIssueCount <= 24)
            {
#if NO
                // 15天以后
                start += new TimeSpan(15, 0, 0, 0);
                return DateTimeUtil.DateTimeToString8(start);
#endif
                nCount = 2;
            }

            // 一年36期
            else if (nIssueCount > 24 && nIssueCount <= 36)
            {
                // TODO: 如果是 36 期，则尽量分布在每月以内 3 期
                // 把一个月粗略划分为 30/3，根据落入的部分来测算下一个的时间
                nCount = 3;
#if NO
                // 10天以后
                now += new TimeSpan(10, 0, 0, 0);
                return DateTimeUtil.DateTimeToString8(now);
#endif
            }

            // 一年48期
            else if (nIssueCount > 36 && nIssueCount <= 48)
            {
#if NO
                // 7天以后
                start += new TimeSpan(7, 0, 0, 0);
                return DateTimeUtil.DateTimeToString8(start);
#endif
                nCount = 4;
            }

            // 一年52期
            else if (nIssueCount > 48 && nIssueCount <= 52)
            {
                // 7天以后
                start += new TimeSpan(7, 0, 0, 0);
                return DateTimeUtil.DateTimeToString8(start);
            }

            // 2012/5/9
            // 一年61期
            else if (nIssueCount > 52 && nIssueCount <= 61)
            {
                // 6天以后
                start += new TimeSpan(6, 0, 0, 0);
                return DateTimeUtil.DateTimeToString8(start);
            }

            // 2012/5/9
            // 一年73期
            else if (nIssueCount > 61 && nIssueCount <= 73)
            {
                // 5天以后
                start += new TimeSpan(5, 0, 0, 0);
                return DateTimeUtil.DateTimeToString8(start);
            }

            // 2012/5/9
            // 一年92期
            else if (nIssueCount > 73 && nIssueCount <= 92)
            {
                // 4天以后
                start += new TimeSpan(4, 0, 0, 0);
                return DateTimeUtil.DateTimeToString8(start);
            }

            // 2012/5/9
            // 一年122期
            else if (nIssueCount > 92 && nIssueCount <= 122)
            {
                // 3天以后
                start += new TimeSpan(3, 0, 0, 0);
                return DateTimeUtil.DateTimeToString8(start);
            }

            // 2012/5/9
            // 一年183期
            else if (nIssueCount > 122 && nIssueCount <= 183)
            {
                // 2天以后
                start += new TimeSpan(2, 0, 0, 0);
                return DateTimeUtil.DateTimeToString8(start);
            }

            // 一年365期
            else if (nIssueCount > 183 && nIssueCount <= nDaysOfYear)
            {
                // 1天以后
                start += new TimeSpan(1, 0, 0, 0);
                return DateTimeUtil.DateTimeToString8(start);
            }

#if NO
            // 一年730期
            else if (nIssueCount > 365 && nIssueCount <= 730)
            {
                // 12小时天以后
                now += new TimeSpan(0, 12, 0, 0);
                return DateTimeUtil.DateTimeToString8(now);
            }
#endif

            if (nCount == 0)
                return "????????";  // 无法处理的情形

            List<PartOfMonth> parts = GetMonthParts(strPublishTime, nCount);
            int current_delta = 0;
            int index = 0;
            LocationPart(parts,
                strPublishTime,
                out index,
                out current_delta);
            Debug.Assert(index != -1, "");

            int nDelta = current_delta;
            if (nPreferredDelta != -1)
                nDelta = nPreferredDelta;

            nPreferredDelta = current_delta;    // 返回本次的

            if (index >= nCount - 1)
            {
                // 处在当月最后一个部分了。需要返回下一个月的第一个部分
                List<PartOfMonth> next_parts = GetMonthParts(DateTimeUtil.NextMonth(start), nCount);
                return AddDays(next_parts[0].StartDate, nDelta, next_parts[0].EndDate);
            }

            return AddDays(parts[index + 1].StartDate, nDelta, parts[index + 1].EndDate);


        }

        // 获得一年内的期总数
        // return:
        //      -1  出错
        //      0   无法获得
        //      1   获得
        int GetOneYearIssueCount(string strPublishYear,
            out int nValue,
            out string strError)
        {
            strError = "";
            nValue = 0;

            if (this.GetOrderInfo == null)
                return 0;   // 无法获得

            GetOrderInfoEventArgs e1 = new GetOrderInfoEventArgs();
            e1.BiblioRecPath = "";
            e1.PublishTime = strPublishYear;
            e1.LibraryCodeList = this.LibraryCodeList;
            this.GetOrderInfo(this, e1);
            if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
            {
                strError = "在获取本种内出版日期为 '" + strPublishYear + "' 的订购信息的过程中发生错误: " + e1.ErrorInfo;
                return -1;
            }

            if (e1.OrderXmls.Count == 0)
                return 0;

            for (int i = 0; i < e1.OrderXmls.Count; i++)
            {
                string strXml = e1.OrderXmls[i];

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "XML装入DOM时发生错误: " + ex.Message;
                    return -1;
                }

                string strRange = DomUtil.GetElementText(dom.DocumentElement,
                    "range");

                string strIssueCount = DomUtil.GetElementText(dom.DocumentElement,
                    "issueCount");

                int nIssueCount = 0;
                try
                {
                    nIssueCount = Convert.ToInt32(strIssueCount);
                }
                catch
                {
                    continue;
                }

                float years = Global.Years(strRange);
                if (years != 0)
                {
                    nValue = Convert.ToInt32((float)nIssueCount * (1 / years));
                }
            }

            return 1;
        }

        // 获得第一个未记到的订购范围的起始时间
        // return:
        //      -1  出错
        //      0   无法获得
        //      1   获得
        int GetFirstUseablePublishTime(
            out string strPublishTime,
            out string strError)
        {
            strPublishTime = "";
            strError = "";

            if (this.GetOrderInfo == null)
                return 0;   // 无法获得

            GetOrderInfoEventArgs e1 = new GetOrderInfoEventArgs();
            e1.BiblioRecPath = "";
            e1.PublishTime = "*";
            e1.LibraryCodeList = this.LibraryCodeList;
            this.GetOrderInfo(this, e1);
            if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
            {
                strError = "在获取本种内出版日期为 '" + "*" + "' 的订购信息的过程中发生错误: " + e1.ErrorInfo;
                return -1;
            }

            if (e1.OrderXmls.Count == 0)
                return 0;

            List<string> timestrings = new List<string>();
            for (int i = 0; i < e1.OrderXmls.Count; i++)
            {
                string strXml = e1.OrderXmls[i];

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(strXml);
                }
                catch (Exception ex)
                {
                    strError = "XML装入DOM时发生错误: " + ex.Message;
                    return -1;
                }

                string strState = DomUtil.GetElementText(dom.DocumentElement,
                    "state");

                // 表示已全部验收
                if (StringUtil.IsInList("已验收", strState) == true)
                    continue;

                string strRange = DomUtil.GetElementText(dom.DocumentElement,
                    "range");

                string strIssueCount = DomUtil.GetElementText(dom.DocumentElement,
                    "issueCount");

                int nIssueCount = 0;
                try
                {
                    nIssueCount = Convert.ToInt32(strIssueCount);
                }
                catch
                {
                    continue;
                }

                if (nIssueCount == 0)
                    continue;

                int nRet = strRange.IndexOf("-");
                if (nRet == -1)
                {
                    strError = "时间范围字符串 '" + strRange + "' 格式不正确";
                    return -1;
                }

                timestrings.Add(strRange.Substring(0, nRet));
            }

            if (timestrings.Count == 0)
                return 0;

            // 排序，取得最小的时间值
            timestrings.Sort();
            strPublishTime = timestrings[0];

            return 1;
        }

        #endregion

        // 撤销记到
        void menuItem_unacceptCells_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 从选择范围中挑出已经记到的格子。
            // 如果选择范围中包含了GroupCell left或者GroupCellRight，则表明选择了其下属的全部记到格子。注意不要重复准备格子对象

            List<Cell> selected_cells = this.SelectedCells;
            if (selected_cells.Count == 0)
            {
                strError = "尚未选定要撤销记到的册";
                goto ERROR1;
            }

            int nSkipCount = 0;
            // 预测册
            List<Cell> accepted_cells = new List<Cell>();
            // 分选
            for (int i = 0; i < selected_cells.Count; i++)
            {
                Cell cell = selected_cells[i];
                if (cell == null)
                {
                    nSkipCount++;
                    continue;
                }

                if (cell is GroupCell)
                {
                    // 2010/4/15
                    GroupCell group = (GroupCell)cell;
                    if (group.EndBracket == true)
                        group = group.HeadGroupCell;

                    List<Cell> temp = group.AcceptedMemberCells;
                    for (int j = 0; j < temp.Count; j++)
                    {
                        Cell cell_temp = temp[j];
                        // 避免重复加入
                        if (accepted_cells.IndexOf(cell_temp) == -1)
                            accepted_cells.Add(cell_temp);
                    }
                    continue;
                }

                if (cell.item != null && cell.item.OrderInfoPosition.X != -1)
                {
                    // 避免重复加入
                    if (accepted_cells.IndexOf(cell) == -1)
                        accepted_cells.Add(cell);
                    continue;
                }

                nSkipCount++;
            }

            if (accepted_cells.Count == 0)
            {
                strError = "所选定的 " + this.SelectedCells.Count.ToString() + " 个格子中，没有处于已记到状态的格子";
                goto ERROR1;
            }

            // 统计出要删除的(非本次新创建的)册记录
            int nOldRecordCount = 0;
            for (int i = 0; i < accepted_cells.Count; i++)
            {
                Cell cell = accepted_cells[i];
                Debug.Assert(cell != null, "");

                // 统计将导致删除的非本次创建册
                if (cell.item.NewCreated == false
                    && cell.item.Deleted == false)
                {
                    // 要检查这些册里面是否有借阅信息
                    if (String.IsNullOrEmpty(cell.item.Borrower) == false)
                    {
                        strError = "册 " + cell.item.RefID + "(册条码为'" + cell.item.Barcode + "') 中包含有借阅信息，不能删除。操作被取消";
                        goto ERROR1;
                    }
                    nOldRecordCount++;
                }

                // 检查是否为成员册
                if (cell.IsMember == true)
                {
                    strError = "册 " + cell.item.RefID + " 已经被合订，不能被撤销记到。操作被取消";
                    goto ERROR1;
                }

                if (cell.item != null && cell.item.Locked == true)
                {
                    strError = "对处于锁定状态的格子不能进行撤销记到操作";
                    goto ERROR1;
                }
            }

            string strMessage = "";
            if (nOldRecordCount > 0)
                strMessage = "撤销记到的操作将导致以前创建的 " + nOldRecordCount.ToString() + " 个册记录被删除。\r\n\r\n";

            // 警告
            DialogResult dialog_result = MessageBox.Show(this,
                strMessage
            + "确实要对所选定的 " + accepted_cells.Count.ToString() + " 个格子进行撤销记到的操作?",
"BindingControls",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
            if (dialog_result == DialogResult.No)
                return;


            for (int i = 0; i < accepted_cells.Count; i++)
            {
                Cell cell = accepted_cells[i];
                Debug.Assert(cell != null, "");
                if (String.IsNullOrEmpty(cell.Container.PublishTime) == true)
                {
                    strError = "对处于自由期中的格子不能进行撤销记到操作";
                    goto ERROR1;
                }
            }

            for (int i = 0; i < accepted_cells.Count; i++)
            {
                Cell cell = accepted_cells[i];
                Debug.Assert(cell != null, "");
                nRet = cell.item.DoUnaccept(out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            // 从单元格子变化为所从属的期格子
            List<IssueBindingItem> update_issues = GetIssueList(accepted_cells);
            this.UpdateIssues(update_issues);
            /*

            // 刷新期行
            List<CellBase> update_cells = new List<CellBase>();
            for (int i = 0; i < update_issues.Count; i++)
            {
                update_cells.Add((CellBase)update_issues[i]);
            }
            this.UpdateObjects(update_cells);
             * */

            // 刷新编辑区域
            if (this.FocusObject != null && this.FocusObject is Cell)
            {
                Cell focus_obejct = (Cell)this.FocusObject;
                if (accepted_cells.IndexOf(focus_obejct) != -1)
                {
                    if (this.CellFocusChanged != null)
                    {
                        FocusChangedEventArgs e1 = new FocusChangedEventArgs();
                        e1.OldFocusObject = this.FocusObject;
                        e1.NewFocusObject = this.FocusObject;
                        this.CellFocusChanged(this, e1);
                    }
                }
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 记到 -- 到。即为预测格子创建真正的册记录
        // 为<orderInfo>下的特定<root>内<location>设定refid，并修改<copy>中的已到值部分
        // 用户如果要修改字段内容，可到册信息编辑器中进行。这里就不再出现对话框了。
        // 批次号等会自动设置
        void menuItem_AcceptCells_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            // 从选择范围中挑出预测格子。
            // 如果选择范围中包含了GroupCell left或者GroupCellRight，则表明选择了其下属的全部预测格子。注意不要重复准备格子对象

            List<Cell> selected_cells = this.SelectedCells;
            if (selected_cells.Count == 0)
            {
                strError = "尚未选定要记到的册";
                goto ERROR1;
            }

            int nSkipCount = 0;
            // 预测册
            List<Cell> calculated_cells = new List<Cell>();
            // 分选
            for (int i = 0; i < selected_cells.Count; i++)
            {
                Cell cell = selected_cells[i];
                if (cell == null)
                {
                    nSkipCount++;
                    continue;
                }

                if (cell is GroupCell)
                {
                    GroupCell group = (GroupCell)cell;

                    if (group.EndBracket == true)
                        group = group.HeadGroupCell;

                    List<Cell> member_cells = group.CalculatedMemberCells;
                    for (int j = 0; j < member_cells.Count; j++)
                    {
                        Cell cell_temp = member_cells[j];
                        // 避免重复加入
                        if (calculated_cells.IndexOf(cell_temp) == -1)
                            calculated_cells.Add(cell_temp);
                    }
                    continue;
                }

                if (cell.item != null && cell.item.Calculated == true)
                {
                    // 避免重复加入
                    if (calculated_cells.IndexOf(cell) == -1)
                        calculated_cells.Add(cell);
                    continue;
                }

                nSkipCount++;
            }

            if (calculated_cells.Count == 0)
            {
                strError = "所选定的 " + this.SelectedCells.Count.ToString() + " 个格子中，没有处于预测状态的格子";
                goto ERROR1;
            }

            for (int i = 0; i < calculated_cells.Count; i++)
            {
                Cell cell = calculated_cells[i];
                Debug.Assert(cell != null, "");

                if (String.IsNullOrEmpty(cell.Container.PublishTime) == true)
                {
                    strError = "对处于自由期中的格子不能进行记到操作";
                    goto ERROR1;
                }

                if (cell.item != null && cell.item.Locked == true)
                {
                    strError = "对处于锁定状态的格子不能进行记到操作";
                    goto ERROR1;
                }
            }

            for (int i = 0; i < calculated_cells.Count; i++)
            {
                Cell cell = calculated_cells[i];
                Debug.Assert(cell != null, "");
                nRet = cell.item.DoAccept(out strError);
                if (nRet == -1)
                    goto ERROR1;
            }

            // 从单元格子变化为所从属的期格子
            List<IssueBindingItem> update_issues = GetIssueList(calculated_cells);
            this.UpdateIssues(update_issues);
            /*

            // 刷新期行
            List<CellBase> update_cells = new List<CellBase>();
            for (int i = 0; i < update_issues.Count; i++)
            {
                update_cells.Add((CellBase)update_issues[i]);
            }
            this.UpdateObjects(update_cells);
             * */

            // 刷新编辑区域
            if (this.FocusObject != null && this.FocusObject is Cell)
            {
                Cell focus_obejct = (Cell)this.FocusObject;
                if (calculated_cells.IndexOf(focus_obejct) != -1)
                {
                    if (this.CellFocusChanged != null)
                    {
                        FocusChangedEventArgs e1 = new FocusChangedEventArgs();
                        e1.OldFocusObject = this.FocusObject;
                        e1.NewFocusObject = this.FocusObject;
                        this.CellFocusChanged(this, e1);
                    }
                }
            }
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 获得期刊名称的列表。用于显示在对话框上
        // parameters:
        //      strDelimiter    分隔符号
        //      nMaxCount   最多列出多少个
        static string GetIssuesCaption(List<IssueBindingItem> issues,
            string strDelimiter,
            int nMaxCount)
        {
            string strCaptions = "";
            for (int i = 0; i < Math.Min(issues.Count, nMaxCount); i++)
            {
                if (string.IsNullOrEmpty(strCaptions) == false)
                    strCaptions += strDelimiter;
                strCaptions += issues[i].Caption;
            }
            if (issues.Count >= nMaxCount)
                strCaptions += strDelimiter + "...";

            return strCaptions;
        }

        void menuItem_deleteCoverImage_Click(object sender, EventArgs e)
        {
            List<IssueBindingItem> selected_issues = new List<IssueBindingItem>();
            foreach (IssueBindingItem issue in this.SelectedIssues)
            {
                // 跳过自由期
                if (String.IsNullOrEmpty(issue.PublishTime) == true)
                    continue;

                selected_issues.Add(issue);
            }

            foreach (IssueBindingItem issue in selected_issues)
            {
                issue.DeleteCoverImage();
            }
        }

        void menuItem_insertCoverImageFromCamera_Click(object sender, EventArgs e)
        {
            string strError = "";

            List<IssueBindingItem> selected_issues = new List<IssueBindingItem>();
            foreach (IssueBindingItem issue in this.SelectedIssues)
            {
                // 跳过自由期
                if (String.IsNullOrEmpty(issue.PublishTime) == true)
                    continue;

                selected_issues.Add(issue);
            }

            Program.MainForm.DisableCamera();
            try
            {
                int i = 0;
                foreach (IssueBindingItem issue in selected_issues)
                {
                    ImageInfo info = new ImageInfo();

                    try
                    {
                        // TODO: 为对话框增加关于即将扫描的期，期号的说明提示

                        bool control = (Control.ModifierKeys & Keys.Control) == Keys.Control;

                        // 注： new CameraClipDialog() 可能会抛出异常
                        ICameraClip dlg = null;
                        if (string.IsNullOrEmpty(Program.MainForm.FaceReaderUrl)
                            || control)
                            dlg = new CameraClipDialog();
                        else
                            dlg = new PhotoClipDialog();
                        using (dlg)
                        {
                            dlg.Font = this.Font;

                            dlg.CurrentCamera = Program.MainForm.AppInfo.GetString(
                                "bindingControl",
                                "current_camera",
                                "");

                            Program.MainForm.AppInfo.LinkFormState((Form)dlg, "CameraClipDialog_state");
                            dlg.ShowDialog(this);
                            Program.MainForm.AppInfo.UnlinkFormState((Form)dlg);

                            Program.MainForm.AppInfo.SetString(
                                "bindingControl",
                                "current_camera",
                                dlg.CurrentCamera);

                            if (dlg.DialogResult == System.Windows.Forms.DialogResult.Cancel)
                                return;

                            info = dlg.ImageInfo;
                            if (Program.MainForm.SaveOriginCoverImage == false)
                                info.ClearBackupImage();
                        }

                        GC.Collect();

                        if (issue.SetCoverImage(info, out strError) == -1)
                            goto ERROR1;
                    }
                    finally
                    {
                        if (info != null)
                            info.Dispose();
                    }

                    i++;
                }
            }
            finally
            {
                Application.DoEvents();

                Program.MainForm.EnableCamera();
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menuItem_insertCoverImageFromClipboard_Click(object sender, EventArgs e)
        {
            string strError = "";

            List<IssueBindingItem> selected_issues = new List<IssueBindingItem>();
            foreach (IssueBindingItem issue in this.SelectedIssues)
            {
                // 跳过自由期
                if (String.IsNullOrEmpty(issue.PublishTime) == true)
                    continue;

                selected_issues.Add(issue);
            }

            // 从剪贴板中取得图像对象
            List<Image> images = ImageUtil.GetImagesFromClipboard(out strError);
            if (images == null)
            {
                strError += "。无法创建封面图像";
                goto ERROR1;
            }

#if NO
            // 从剪贴板中取得图像对象
            List<Image> images = new List<Image>();
            IDataObject obj1 = Clipboard.GetDataObject();
            if (obj1.GetDataPresent(typeof(Bitmap)))
            {
                images.Add((Image)obj1.GetData(typeof(Bitmap)));
            }
            else if (obj1.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])obj1.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    try
                    {
                        images.Add(Image.FromFile(file));
                    }
                    catch (OutOfMemoryException)
                    {
                        strError = "当前 Windows 剪贴板中的某个文件不是图像文件。无法创建封面图像";
                        goto ERROR1;
                    }
                }
            }
            else
            {
                strError = "当前 Windows 剪贴板中没有图形对象。无法创建封面图像";
                goto ERROR1;
            }
#endif

            {
                int i = 0;
                foreach (IssueBindingItem issue in selected_issues)
                {
                    if (i >= images.Count)
                        break;
                    Image image = images[i];
                    ImageInfo info = new ImageInfo();
                    info.Image = image;
                    if (issue.SetCoverImage(info, out strError) == -1)
                        goto ERROR1;

                    i++;
                }
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        void menuItem_insertCoverImageFromLongyuanQikan_Click(object sender, EventArgs e)
        {
            string strError = "";

            List<IssueBindingItem> selected_issues = new List<IssueBindingItem>();
            foreach (IssueBindingItem issue in this.SelectedIssues)
            {
                // 跳过自由期
                if (String.IsNullOrEmpty(issue.PublishTime) == true)
                    continue;

                selected_issues.Add(issue);
            }

#if NO
            // 从剪贴板中取得图像对象
            List<Image> images = null;
            if (images == null)
            {
                strError = "。无法创建封面图像";
                goto ERROR1;
            }
#endif
            string strISSN = GetTitle();
            if (string.IsNullOrEmpty(strISSN))
            {
                strError = "本刊书目记录中缺乏 ISSN 号，因此无法获取封面图像";
                goto ERROR1;
            }

            {
                CookieContainer cookie = new CookieContainer();
                int i = 0;
                foreach (IssueBindingItem issue in selected_issues)
                {
#if NO
                    string strImageUrl = "";
                    int nRet = LongyuanQikan.GetCoverImageUrl(
                        this,
                        strISSN,
                        dp2StringUtil.GetYearPart(issue.PublishTime),
                        issue.Issue,
                        ref cookie,
                        out strImageUrl,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
#endif
                    int nRet = LongyuanQikan.GetCoverImageToClipboard(
    this,
    strISSN,
    dp2StringUtil.GetYearPart(issue.PublishTime),
    issue.Issue,
    out strError);
                    if (nRet == -1)
                        goto ERROR1;

#if NO
                   string strLocalFileName = Path.GetTempFileName();
                    try
                    {
                        // return:
                        //      -1  出错
                        //      0   没有找到
                        //      1   找到
                        nRet = LongyuanQikan.DownloadImageFile(strImageUrl,
                            strLocalFileName,
                            ref cookie,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        if (nRet == 0)
                        {
                            strError = "图像 " + strImageUrl + " 没有找到";
                            goto ERROR1;
                        }

                        using (Image image = Image.FromFile(strLocalFileName))
                        {
                            if (issue.SetCoverImage(image, out strError) == -1)
                                goto ERROR1;
                        }
                    }
                    finally
                    {
                        File.Delete(strLocalFileName);
                    }
#endif
                    // 从剪贴板中取得图像对象
                    List<Image> images = ImageUtil.GetImagesFromClipboard(out strError);
                    if (images == null)
                    {
                        strError += "。无法创建封面图像";
                        goto ERROR1;
                    }
                    if (images.Count > 0)
                    {
                        try
                        {
                            ImageInfo info = new ImageInfo();
                            info.Image = images[0];
                            if (issue.SetCoverImage(info, out strError) == -1)
                                goto ERROR1;
                        }
                        finally
                        {
                            foreach (Image image in images)
                            {
                                image.Dispose();
                            }
                        }
                    }
                    i++;
                }
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        string GetIssn()
        {
            var func = this.GetBiblio;
            if (func != null)
            {
                GetBiblioEventArgs e = new GetBiblioEventArgs();
                func(this, e);
                if (string.IsNullOrEmpty(e.Data))
                    return "";
                MarcRecord record = new MarcRecord(e.Data);
                if (e.Syntax == "unimarc")
                    return record.select("field[@name='011']/subfield[@name='a']").FirstContent;
                if (e.Syntax == "usmarc")
                    return record.select("field[@name='020']/subfield[@name='a']").FirstContent;
            }
            return "";
        }

        string GetTitle()
        {
            var func = this.GetBiblio;
            if (func != null)
            {
                GetBiblioEventArgs e = new GetBiblioEventArgs();
                func(this, e);
                if (string.IsNullOrEmpty(e.Data))
                    return "";
                MarcRecord record = new MarcRecord(e.Data);
                if (e.Syntax == "unimarc")
                    return record.select("field[@name='200']/subfield[@name='a']").FirstContent;
                if (e.Syntax == "usmarc")
                    return record.select("field[@name='245']/subfield[@name='a']").FirstContent;
            }
            return "";
        }

        // 恢复若干个期记录
        void menuItem_recoverIssues_Click(object sender, EventArgs e)
        {
            string strError = "";

            // 已经是实在状态的期
            List<IssueBindingItem> normal_issues = new List<IssueBindingItem>();
            // 符合恢复条件的期
            List<IssueBindingItem> selected_issues = new List<IssueBindingItem>();
            for (int i = 0; i < this.SelectedIssues.Count; i++)
            {
                IssueBindingItem issue = this.SelectedIssues[i];

                // 跳过自由期
                if (String.IsNullOrEmpty(issue.PublishTime) == true)
                    continue;

                if (issue.Virtual == false)
                {
                    normal_issues.Add(issue);
                    continue;
                }

                selected_issues.Add(issue);
            }

            if (normal_issues.Count == 0
                && selected_issues.Count == 0)
            {
                strError = "尚未选择要恢复的期对象";
                goto ERROR1;
            }

            string strNormalIssueList = "";
            if (normal_issues.Count > 0)
            {
                strNormalIssueList = GetIssuesCaption(normal_issues,
                    "\r\n", 10);
                strError = "不能恢复以下本来就记录的期:\r\n" + strNormalIssueList + "\r\n\r\n";
                if (selected_issues.Count == 0)
                {
                    goto ERROR1;
                }
            }

            string strCaptions = GetIssuesCaption(selected_issues,
                    "\r\n", 10);

            string strMessage = strError;
            if (String.IsNullOrEmpty(strMessage) == false)
                strMessage += "---\r\n是否要继续恢复其余的下列期的数据库记录?\r\n " + strCaptions;
            else
                strMessage += "确实要恢复下列期的数据库记录?\r\n " + strCaptions;

            // 
            DialogResult dialog_result = MessageBox.Show(this,
    strMessage,
    "BindingControls",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
            if (dialog_result == DialogResult.No)
                return;

            for (int i = 0; i < selected_issues.Count; i++)
            {
                IssueBindingItem issue = selected_issues[i];
                issue.Virtual = false;
                if (string.IsNullOrEmpty(issue.RefID) == true)
                    issue.RefID = Guid.NewGuid().ToString();
                issue.Changed = true;
                issue.NewCreated = true;
                issue.AfterMembersChanged();    // 刷新Issue对象内的XML
                this.m_bChanged = true;
            }

            // this.m_lContentHeight = this.m_nCellHeight * this.Issues.Count;
            this.AfterWidthChanged(true);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 删除若干个期
        void menuItem_deleteIssues_Click(object sender, EventArgs e)
        {
            string strError = "";

            /*
            ToolStripMenuItem menu = (ToolStripMenuItem)sender;

            HitTestResult result = null;

            Point p = (Point)menu.Tag;

            // Debug.WriteLine("hover=" + p.ToString());

            // 屏幕坐标
            this.HitTest(
                p.X,
                p.Y,
                typeof(IssueBindingItem),
                out result);
            if (result == null || !(result.Object is IssueBindingItem))
            {
                strError = "鼠标未处于适当的位置";
                goto ERROR1;
            }

            IssueBindingItem issue = (IssueBindingItem)result.Object;

            Debug.Assert(issue != null, "");
             * */
            List<string> messages = new List<string>();
            // 非空的期
            List<IssueBindingItem> cantdelete_issues = new List<IssueBindingItem>();
            // 符合删除条件的期
            List<IssueBindingItem> selected_issues = new List<IssueBindingItem>();
            for (int i = 0; i < this.SelectedIssues.Count; i++)
            {
                IssueBindingItem issue = this.SelectedIssues[i];

                // 跳过自由期
                if (String.IsNullOrEmpty(issue.PublishTime) == true)
                    continue;

                string strTemp = "";
                if (issue.CanDelete(out strTemp) == false)
                {
                    cantdelete_issues.Add(issue);
                    messages.Add(strTemp);
                    continue;
                }

                selected_issues.Add(issue);
            }

            if (cantdelete_issues.Count == 0
                && selected_issues.Count == 0)
            {
                strError = "尚未选择要删除的期对象";
                goto ERROR1;
            }

#if NO
            string strNoneBlankList = "";
            if (noneblank_issues.Count > 0)
            {
                strNoneBlankList = GetIssuesCaption(noneblank_issues,
                    "\r\n", 10);
                strError = "不能删除以下还含有册的期:\r\n" + strNoneBlankList + "\r\n\r\n";
                if (selected_issues.Count == 0)
                {
                    goto ERROR1;
                }
            }
#endif
            string strCantDeleteList = "";
            if (cantdelete_issues.Count > 0)
            {
                for (int i = 0; i < cantdelete_issues.Count; i++)
                {
                    strCantDeleteList += cantdelete_issues[i].Caption + " : " + messages[i] + "\r\n";
                    if (i > 10)
                    {
                        strCantDeleteList += "...";
                        break;
                    }
                }

                strError = "不能删除以下期:\r\n" + strCantDeleteList + "\r\n\r\n";
                if (selected_issues.Count == 0)
                {
                    goto ERROR1;
                }
            }

            string strCaptions = GetIssuesCaption(selected_issues,
                    "\r\n", 10);

            string strMessage = strError;
            if (String.IsNullOrEmpty(strMessage) == false)
                strMessage += "---\r\n是否要继续删除其余的下列期?\r\n " + strCaptions;
            else
                strMessage += "确实要删除下列期?\r\n " + strCaptions;

            // 
            DialogResult dialog_result = MessageBox.Show(this,
    strMessage,
    "BindingControls",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
            if (dialog_result == DialogResult.No)
                return;

            for (int i = 0; i < selected_issues.Count; i++)
            {
                IssueBindingItem issue = selected_issues[i];

                if (this.FocusObject == issue)
                    this.FocusObject = null;

                this.m_aSelectedArea.Remove(issue); // 防止取消选择时抛出异常

                this.Issues.Remove(issue);
                this.m_bChanged = true;
            }

            // this.m_lContentHeight = this.m_nCellHeight * this.Issues.Count;
            this.AfterWidthChanged(true);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 打开/关闭 编辑区域
        void menuItem_toggleEditArea_Click(object sender, EventArgs e)
        {
            if (this.EditArea == null)
                return;

            bool bClose = false;
            {
                EditAreaEventArgs e1 = new EditAreaEventArgs();
                e1.Action = "get_state";
                this.EditArea(this, e1);
                if (e1.Result == "visible")
                    bClose = true;
                else
                    bClose = false;
            }

            {
                EditAreaEventArgs e1 = new EditAreaEventArgs();
                if (bClose == true)
                    e1.Action = "close";
                else
                    e1.Action = "open";
                this.EditArea(this, e1);
            }
        }

        // 将选择的合订册向左移动一个双格
        void menuItem_moveToLeft_Click(object sender, EventArgs e)
        {
            string strError = "";
            // string strWarning = "";

            // 合订册数组
            List<Cell> parent_cells = new List<Cell>();

            List<Cell> selected_cells = this.SelectedCells;
            for (int i = 0; i < selected_cells.Count; i++)
            {
                Cell cell = selected_cells[i];
                if (IsBindingParent(cell) == true)
                {
                    // 注：cell.item.MemberCells.Count 有可能等于0
                    parent_cells.Add(cell);
                }
            }

            if (parent_cells.Count == 0)
            {
                strError = "所选定的单元中，没有合订册";
                goto ERROR1;
            }

            for (int i = 0; i < parent_cells.Count; i++)
            {
                Cell cell = parent_cells[i];
                if (CanMoveToLeft(cell) == false)
                    continue;

                MoveCellsToLeft(cell);
#if DEBUG
                {
                    string strError1 = "";
                    int nRet1 = cell.item.VerifyMemberCells(out strError1);
                    if (nRet1 == -1)
                    {
                        Debug.Assert(false, strError1);
                    }
                }

#endif
            }

            this.AfterWidthChanged(true);

#if DEBUG
            VerifyAll();
#endif
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 从合订本中移出成员册，并(如果必要)缩小合订范围
        void menuItem_removeFromBindingAndShrink_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.SelectedCells.Count == 0)
            {
                strError = "尚未选定要移出的合订成员册";
                goto ERROR1;
            }

            List<Cell> source_cells = new List<Cell>();

            source_cells.AddRange(this.SelectedCells);
            // 将合订成员册从合订册中移出，成为单册
            nRet = RemoveFromBinding(
                true,   // shrink
                false,
                source_cells,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 从合订本中移出成员册，不缩小合订范围
        void menuItem_removeFromBinding_Click(object sender, EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            if (this.SelectedCells.Count == 0)
            {
                strError = "尚未选定要移出的合订成员册";
                goto ERROR1;
            }

            List<Cell> source_cells = new List<Cell>();

            source_cells.AddRange(this.SelectedCells);
            // 将合订成员册从合订册中移出，成为单册
            nRet = RemoveFromBinding(
                false,  // shrink
                false,
                source_cells,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 设为空白
        void menuItem_setBlank_Click(object sender, EventArgs e)
        {
            string strError = "";

            ToolStripMenuItem menu = (ToolStripMenuItem)sender;

            HitTestResult result = null;

            // Point p = this.PointToClient((Point)menu.Tag); // Control.MousePosition
            Point p = (Point)menu.Tag;

            // Debug.WriteLine("hover=" + p.ToString());

            // 屏幕坐标
            this.HitTest(
                p.X,
                p.Y,
                typeof(NullCell),
                out result);
            if (result == null || !(result.Object is NullCell))
            {
                strError = "鼠标未处于适当的位置";
                goto ERROR1;
            }

            NullCell null_cell = (NullCell)result.Object;

            IssueBindingItem issue = this.Issues[null_cell.Y];
            Debug.Assert(issue != null, "");

            // TODO: 如果在已经装订的范围内？是否要把binded设置为true

            Cell cell = new Cell();
            cell.item = null;
            cell.ParentItem = null;
            issue.SetCell(null_cell.X, cell);
            this.UpdateObject(cell);

            this.ClearAllSelection();
            cell.Select(SelectAction.On);
            this.FocusObject = cell;

            // 有可能新创建的空白格子超过了右侧边界
            if (null_cell.X >= this.m_nMaxItemCountOfOneIssue)
                this.AfterWidthChanged(true);

            this.EnsureVisible(cell);  // 确保滚入视野

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 删除所选择的 格子
        void menuItem_deleteCells_Click(object sender, EventArgs e)
        {
            // TODO: 记得最后从选择范围中移走 this.m_aSelectedArea.Remove(deleted_cell); // 防止取消选择时抛出异常

            string strError = "";
            string strWarning = "";
            int nRet = 0;

            List<Cell> selected_cells = this.SelectedCells;
            if (selected_cells.Count == 0)
            {
                strError = "尚未选定要删除的格子";
                goto ERROR1;
            }

            // 单册
            List<Cell> mono_cells = new List<Cell>();

            // 已经参与装订的册
            List<Cell> member_cells = new List<Cell>();

            // 合订册
            List<Cell> parent_cells = new List<Cell>();

            // 空白Cell
            List<Cell> blank_cells = new List<Cell>();

            // 分选
            for (int i = 0; i < selected_cells.Count; i++)
            {
                Cell cell = selected_cells[i];

                if (this.IsBindingParent(cell) == true)
                {
                    Debug.Assert(cell.item != null, "");
                    Debug.Assert(cell.item.IsMember == false, "");
                    parent_cells.Add(cell);
                }
                else if (cell.IsMember == true)
                {
                    // 注：cell.item可能为空
                    member_cells.Add(cell);
                }
                else
                    mono_cells.Add(cell);

                /*
                blank_cells.Add(cell);
                    // TODO: 空白格子如何处理?
                 * */
            }

            /*
            // 警告，合订成员册也会被删除
            // TODO: 要检查合订册和所有下属册，都没有借还信息，符合删除条件
            if (binded_items.Count > 0)
            {
                strError = "有 " + binded_items.Count.ToString() + " 个事项是已经被合订的册，因此无法进行合订";
                goto ERROR1;
            }

            if (mono_items.Count == 0)
            {
                strError = "所选定的格子中没有包含任何册";
                goto ERROR1;
            }
             * */

            // 检查固化册
            strWarning = "";
            int nFixedCount = 0;
            int nOldCount = parent_cells.Count;
            for (int i = 0; i < parent_cells.Count; i++)
            {
                Cell parent_cell = parent_cells[i];

                // 2015/12/2
                if (parent_cell.item == null)
                    continue;

                if (CheckProcessingState(parent_cell.item) == false)
                {
                    if (String.IsNullOrEmpty(strWarning) == false)
                        strWarning += "\r\n";
                    strWarning += parent_cell.item.PublishTime;
                    nFixedCount++;
                    parent_cells.RemoveAt(i);
                    i--;
                }

                if (parent_cell.item.Locked == true)
                {
                    strError = "处于锁定状态的合订册不能删除";
                    goto ERROR1;
                }
            }

            bool bAsked = false;

            // 警告固化册
            if (String.IsNullOrEmpty(strWarning) == false)
            {
                strError =
                    "所选定的 " + nOldCount.ToString() + " 个合订册中，有下列册处于固化状态，不能删除:\r\n\r\n" + strWarning;

                if (parent_cells.Count > 0)
                {
                    strError += "\r\n\r\n是否要继续删除所选范围内的其余 "
                        + (parent_cells.Count).ToString()
                        + " 个合订册?";
                    DialogResult result = MessageBox.Show(this,
                        strError,
                        "BindingControls",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                        return;
                    bAsked = true;
                }
                else
                    goto ERROR1;
            }

            // 检查合订册的成员册
            strWarning = "";
            int nErrorCount = 0;
            for (int i = 0; i < parent_cells.Count; i++)
            {
                Cell parent_cell = parent_cells[i];

                for (int j = 0; j < parent_cell.item.MemberCells.Count; j++)
                {
                    Cell cell = parent_cell.item.MemberCells[j];

                    if (cell == null && cell.item == null)
                        continue;

                    if (CheckProcessingState(cell.item) == false
    && cell.item.Calculated == false   // 预测格子除外
    && cell.item.Deleted == false)  // 已经删除的格子除外
                    {
                        // 不是"加工中"状态
                        if (String.IsNullOrEmpty(strWarning) == false)
                            strWarning += ";\r\n";
                        strWarning += "册 '" + cell.item.RefID + "' 不是“加工中”状态";
                        nErrorCount++;
                    }
                    else if (String.IsNullOrEmpty(cell.item.Borrower) == false)
                    {
                        // 已借出状态
                        if (String.IsNullOrEmpty(strWarning) == false)
                            strWarning += ";\r\n";
                        strWarning += "册 '" + cell.item.RefID + "' 尚处于“已借出”状态";
                        nErrorCount++;
                    }
                }
            }
            // 警告有不宜删除的成员册 的 合订册
            if (String.IsNullOrEmpty(strWarning) == false)
            {
                strError =
                    "所选定的 " + parent_cells.Count.ToString() + " 个合订册中，因有下列成员册不能删除:\r\n\r\n" + strWarning;
                strError += "\r\n\r\n因此合订册本身也连带着不能被删除。\r\n\r\n(如果确实想要删除合订册本身，可先解除合订以后，再行删除)";

                goto ERROR1;
            }

            // 检查单册
            strWarning = "";
            nErrorCount = 0;
            nOldCount = mono_cells.Count;
            for (int i = 0; i < mono_cells.Count; i++)
            {
                Cell cell = mono_cells[i];
                IssueBindingItem issue = cell.Container;
                Debug.Assert(issue != null, "");

                if (cell is GroupCell)
                {
                    if (String.IsNullOrEmpty(strWarning) == false)
                        strWarning += ";\r\n";
                    strWarning += "订购组格子";
                    nErrorCount++;
                    mono_cells.RemoveAt(i);
                    i--;
                }

                if (cell.item == null)
                    continue;

                if (CheckProcessingState(cell.item) == false
                    && cell.item.Calculated == false   // 预测格子除外
                    && cell.item.Deleted == false)  // 已经删除的格子除外
                {
                    // 不是"加工中"状态
                    if (String.IsNullOrEmpty(strWarning) == false)
                        strWarning += ";\r\n";
                    strWarning += "册 '" + cell.item.RefID + "' 不是“加工中”状态";
                    nErrorCount++;
                    mono_cells.RemoveAt(i);
                    i--;
                }
                else if (String.IsNullOrEmpty(cell.item.Borrower) == false)
                {
                    // 已借出状态
                    if (String.IsNullOrEmpty(strWarning) == false)
                        strWarning += ";\r\n";
                    strWarning += "册 '" + cell.item.RefID + "' 尚处于“已借出”状态";
                    nErrorCount++;
                    mono_cells.RemoveAt(i);
                    i--;
                }
                else if (cell.item.Locked == true)
                {
                    // 已借出状态
                    if (String.IsNullOrEmpty(strWarning) == false)
                        strWarning += ";\r\n";
                    strWarning += "册 '" + cell.item.RefID + "' 处于“锁定”状态";
                    nErrorCount++;
                    mono_cells.RemoveAt(i);
                    i--;
                }
            }
            // 警告不能删除的单册
            if (String.IsNullOrEmpty(strWarning) == false)
            {
                strError =
                    "所选定的 " + nOldCount.ToString() + " 个单册中，有下列册不能删除:\r\n\r\n" + strWarning;

                if (mono_cells.Count > 0)
                {
                    strError += "\r\n\r\n是否要继续删除所选范围内的其余 "
                        + (mono_cells.Count).ToString()
                        + " 个单册?";
                    DialogResult result = MessageBox.Show(this,
                        strError,
                        "BindingControls",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                        return;
                    bAsked = true;
                }
                else
                    goto ERROR1;
            }

            // 检查成员册
            strWarning = "";
            nErrorCount = 0;
            nOldCount = member_cells.Count;
            for (int i = 0; i < member_cells.Count; i++)
            {
                Cell cell = member_cells[i];
                IssueBindingItem issue = cell.Container;
                Debug.Assert(issue != null, "");

                if (cell.item == null)
                    continue;

                // 从属于所选合订册的成员册，要避免重复删除
                bool bFound = false;
                for (int j = 0; j < parent_cells.Count; j++)
                {
                    Cell parent_cell = parent_cells[j];

                    Debug.Assert(parent_cell.item != null, "");
                    if (cell.ParentItem == parent_cell.item)
                    {
                        bFound = true;
                        break;
                    }
                }
                if (bFound == true)
                {
                    member_cells.RemoveAt(i);
                    i--;
                    continue;
                }

                if (CheckProcessingState(cell.item) == false)
                {
                    // 不是"加工中"状态
                    if (String.IsNullOrEmpty(strWarning) == false)
                        strWarning += ";\r\n";
                    strWarning += "册 '" + cell.item.RefID + "' 不是“加工中”状态";
                    nErrorCount++;
                    member_cells.RemoveAt(i);
                    i--;
                }
                else if (String.IsNullOrEmpty(cell.item.Borrower) == false)
                {
                    // 已借出状态
                    if (String.IsNullOrEmpty(strWarning) == false)
                        strWarning += ";\r\n";
                    strWarning += "册 '" + cell.item.RefID + "' 尚处于“已借出”状态";
                    nErrorCount++;
                    member_cells.RemoveAt(i);
                    i--;
                }
                else
                {
                    // 从属的合订册 不是“加工中”状态
                    Cell parent_cell = cell.ParentItem.ContainerCell;
                    if (parent_cell.item != null)
                    {
                        if (CheckProcessingState(parent_cell.item) == false)
                        {
                            // 不是"加工中"状态
                            if (String.IsNullOrEmpty(strWarning) == false)
                                strWarning += ";\r\n";
                            strWarning += "册 '" + cell.item.RefID + "' 所从属的合订册处于固化状态";
                            nErrorCount++;
                            member_cells.RemoveAt(i);
                            i--;
                        }

                        if (parent_cell.item.Locked == true)
                        {
                            // 不是"加工中"状态
                            if (String.IsNullOrEmpty(strWarning) == false)
                                strWarning += ";\r\n";
                            strWarning += "册 '" + cell.item.RefID + "' 所从属的合订册处于锁定状态";
                            nErrorCount++;
                            member_cells.RemoveAt(i);
                            i--;
                        }
                    }
                }
            }

            // 警告不能删除的成员册
            if (String.IsNullOrEmpty(strWarning) == false)
            {
                strError =
                    "所选定的 " + nOldCount.ToString() + " 个成员册中，有下列册不能删除:\r\n\r\n" + strWarning;

                if (member_cells.Count > 0)
                {
                    strError += "\r\n\r\n是否要继续删除所选范围内的其余 "
                        + (member_cells.Count).ToString()
                        + " 个成员册?";
                    DialogResult result = MessageBox.Show(this,
                        strError,
                        "BindingControls",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                        return;
                    bAsked = true;
                }
                else
                    goto ERROR1;
            }

            // 警告即将删除合订册
            if (parent_cells.Count > 0)
            {
                DialogResult dialog_result = MessageBox.Show(this,
    "警告：删除合订册的同时，也将删除其下属的成员册。\r\n\r\n(如果仅要删除合订册而保留其成员册，可改为先用“解除合订”功能，再进行删除)\r\n\r\n确实要删除所选定的 "
    + parent_cells.Count.ToString()
    + " 个合订册及其下属的成员册?",
    "BindingControls",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (dialog_result == DialogResult.No)
                    return;
                bAsked = true;
                // 还要警告删除其余的单册/成员册?
                if (mono_cells.Count > 0 || member_cells.Count > 0)
                    bAsked = false;
            }


            if (bAsked == false)
            {
                DialogResult dialog_result = MessageBox.Show(this,
    "确实要删除所选定的 "
    + (mono_cells.Count + member_cells.Count + parent_cells.Count).ToString()
    + " 个册?",
    "BindingControls",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                if (dialog_result == DialogResult.No)
                    return;
                bAsked = true;
            }



            // *** 开始进行删除

            // 删除单册
            for (int i = 0; i < mono_cells.Count; i++)
            {
                Cell cell = mono_cells[i];
                IssueBindingItem issue = cell.Container;
                Debug.Assert(issue != null, "");

                int nCol = issue.IndexOfCell(cell);
                Debug.Assert(nCol != -1, "");

                // 记到布局模式下的删除
                if (issue.IssueLayoutState == IssueLayoutState.Accepting)
                {
                    DeleteOneCellInAcceptingLayout(cell);
                    continue;
                }

                // 装订布局模式下的删除

                // 具有采购信息关联的格子
                if (cell.item != null
                    && cell.item.OrderInfoPosition.X != -1)
                {
                    Debug.Assert(cell.item.OrderInfoPosition.Y != -1, "");

                    /*
                    // 两步删除法
                    if (cell.item.Calculated == false)
                    {
                        nRet = cell.item.DoUnaccept(out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }
                    else
                    {
                        // 删除预测格，要更新订购信息
                        nRet = cell.item.DoDelete(out strError);
                        if (nRet == -1)
                            goto ERROR1;
                        goto DODELETE;
                    }
                     * */

                    // 一步删除法
                    // 删除前部操作，要更新订购信息
                    nRet = cell.item.DoDelete(out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    goto DODELETE;

                    /*
                    this.m_bChanged = true;
                    continue;
                     * */
                }

                {
                    // 合订双格的左侧位置，不能用RemoveSingleIndex删除
                    // 探测是否为合订成员占据的位置
                    // return:
                    //      -1  是。并且是双格的左侧位置
                    //      0   不是
                    //      1   是。并且是双格的右侧位置
                    nRet = issue.IsBoundIndex(nCol);
                    if (nRet == -1)
                    {
                        if (cell.item == null)
                        {
                            issue.SetCell(nCol, null);
                            goto CONTINUE;
                        }

                    }
                }

            DODELETE:
                // 其他单册(订购信息管辖外的单册)，删除
                issue.RemoveSingleIndex(nCol);
            CONTINUE:
                this.m_bChanged = true;
            }

            // 删除合订册
            strWarning = "";
            for (int i = 0; i < parent_cells.Count; i++)
            {
                Cell parent_cell = parent_cells[i];

                IssueBindingItem parent_issue = parent_cell.Container;
                Debug.Assert(parent_issue != null, "");

                int nParentCol = parent_issue.IndexOfCell(parent_cell);
                Debug.Assert(nParentCol != -1, "");

                Debug.Assert(parent_cell.item != null, "");
                this.ParentItems.Remove(parent_cell.item);

                parent_issue.SetCell(nParentCol, null); // TODO: 可以修改为带有压缩能力
                this.m_bChanged = true;

                for (int j = 0; j < parent_cell.item.MemberCells.Count; j++)
                {
                    Cell cell = parent_cell.item.MemberCells[j];

                    // TODO: 何处检查成员册是否可以被删除? 例如是否具备加工中状态

                    IssueBindingItem issue = cell.Container;
                    Debug.Assert(issue != null, "");

                    int nCurCol = issue.IndexOfCell(cell);
#if DEBUG
                    if (issue.IssueLayoutState == IssueLayoutState.Binding
                        && parent_issue.IssueLayoutState == IssueLayoutState.Binding)
                    {
                        Debug.Assert(nCurCol == nParentCol + 1, "");
                    }
#endif

                    // 具有采购信息关联的格子
                    if (cell.item != null
                        && cell.item.OrderInfoPosition.X != -1)
                    {
                        Debug.Assert(cell.item.OrderInfoPosition.Y != -1, "");
                        nRet = cell.item.DoDelete(out strError);
                        if (nRet == -1)
                            goto ERROR1;
                    }

                    // 清除为NullCell
                    issue.SetCell(nCurCol, null);
                    if (issue.IssueLayoutState == IssueLayoutState.Accepting)
                    {
                        if (nCurCol < issue.Cells.Count)
                        {
                            issue.Cells.RemoveAt(nCurCol);
                        }
                    }
                    this.m_bChanged = true;

                    // 所选定的合订册对象，可能其下属对象，也被选定了
                    // 这里保证不再重复删除
                    member_cells.Remove(cell);
                }
            }

            // 删除成员册
            // 将合订成员册从合订册中移出，消失
            nRet = RemoveFromBinding(
                true,
                true,
                member_cells,
                out strError);
            if (nRet == -1)
                goto ERROR1;

            this.AfterWidthChanged(true);
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 在记到布局模式下删除一个格子
        void DeleteOneCellInAcceptingLayout(Cell cell)
        {
            IssueBindingItem issue = cell.Container;
            Debug.Assert(issue != null, "");

            int nCol = issue.IndexOfCell(cell);
            Debug.Assert(nCol != -1, "");

            Debug.Assert(issue.IssueLayoutState == IssueLayoutState.Accepting, "");

            if (cell.item != null)
            {
                GroupCell group = null;
                if (cell.item.Calculated == true
                    || cell.item.OrderInfoPosition.X != -1)
                {
                    group = cell.item.GroupCell;
                }

                if (nCol < issue.Cells.Count)
                {
                    issue.SetCell(nCol, null);  // 防止HoverObject Assertion
                    issue.Cells.RemoveAt(nCol);
                }

                if (group != null)
                {
                    int nSourceOrderCountDelta = 0;
                    int nSourceArrivedCountDelta = 0;

                    nSourceOrderCountDelta--;
                    if (cell.item != null
                        && cell.item.Calculated == false)
                        nSourceArrivedCountDelta--;

                    group.RefreshGroupMembersOrderInfo(nSourceOrderCountDelta,
                        nSourceArrivedCountDelta);
                }
                return;
            }

            Debug.Assert(cell.item == null, "");

            if (nCol < issue.Cells.Count)
            {
                issue.SetCell(nCol, null);  // 防止HoverObject Assertion
                issue.Cells.RemoveAt(nCol);
            }
        }

        // 解除合订
        // TODO: 成员空白格子如何处理？
        void menuItem_releaseBinding_Click(object sender, EventArgs e)
        {
            string strError = "";
            string strWarning = "";

            // 合订册数组
            List<ItemBindingItem> parent_items = new List<ItemBindingItem>();

            List<Cell> selected_cells = this.SelectedCells;
            for (int i = 0; i < selected_cells.Count; i++)
            {
                Cell cell = selected_cells[i];
                if (IsBindingParent(cell) == true)
                {
                    // 检查所定状态
                    if (cell.item.Locked == true)
                    {
                        strError = "对锁定状态的合订册不能解除合订";
                        goto ERROR1;
                    }

                    // 注：cell.item.MemberCells.Count 有可能等于0
                    parent_items.Add(cell.item);
                }
            }

            if (parent_items.Count == 0)
            {
                strError = "所选定的单元中，没有合订册";
                goto ERROR1;
            }

            // 检查固化册
            int nFixedCount = 0;
            for (int i = 0; i < parent_items.Count; i++)
            {
                ItemBindingItem parent_item = parent_items[i];
                // 2015/12/2
                if (parent_item == null)
                {
                    Debug.Assert(false, "");
                    continue;
                }
                if (CheckProcessingState(parent_item) == false)
                {
                    if (String.IsNullOrEmpty(strWarning) == false)
                        strWarning += "\r\n";
                    strWarning += parent_item.PublishTime;
                    nFixedCount++;
                }
            }

            // 警告固化册
            if (String.IsNullOrEmpty(strWarning) == false)
            {
                strError =
                    "所选定的 " + parent_items.Count.ToString() + " 合订册中，有下列册处于固化状态，不能拆分:\r\n\r\n" + strWarning;

                if (parent_items.Count > nFixedCount)
                {
                    strError += "\r\n\r\n是否要继续拆散所选范围内的其余 "
                        + (parent_items.Count - nFixedCount).ToString()
                        + " 个合订册?";
                    DialogResult result = MessageBox.Show(this,
                        strError,
                        "BindingControls",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result == DialogResult.No)
                        return;
                }
                else
                    goto ERROR1;
            }

            strWarning = "";
            List<Point> nullpos_list = new List<Point>();
            for (int i = 0; i < parent_items.Count; i++)
            {
                ItemBindingItem parent_item = parent_items[i];
                // 2015/12/2
                if (parent_item == null)
                {
                    Debug.Assert(false, "");
                    continue;
                }
                string strPublishTime = parent_item.PublishTime;

                if (CheckProcessingState(parent_item) == false)
                {
                    if (String.IsNullOrEmpty(strWarning) == false)
                        strWarning += "; ";
                    strWarning += parent_item.PublishTime;
                    continue;
                }

                for (int j = 0; j < parent_item.MemberCells.Count; j++)
                {
                    Cell cell = parent_item.MemberCells[j];

                    // 记忆左侧空白位置
                    Point p = GetCellPosition(cell);
                    p.X = p.X - 1;
                    nullpos_list.Add(p);

                    if (cell.item == null)
                    {
                        cell.ParentItem = null;
                    }
                    else
                    {
                        cell.item.ParentItem = null;
                        Debug.Assert(cell.item.IsMember == false, "");

                        // 修改容器Cell binded
                        Cell container_cell = cell.item.ContainerCell;
                        Debug.Assert(container_cell != null, "");
                        Debug.Assert(container_cell == cell, "");
                        container_cell.ParentItem = null;
                    }
                }

                Cell parent_cell = parent_item.ContainerCell;
                if (parent_cell != null)
                {
                    Debug.Assert(parent_cell.IsMember == false, "");
                }

                parent_item.MemberCells.Clear();

                /*
                parent_item.RefreshBindingXml();
                parent_item.RefreshIntact();
                parent_item.Changed = true;
                 * */
                parent_item.AfterMembersChanged();

                // 为了MessageBox()而强制刷新
                this.Invalidate();
                this.Update();

                // 询问是否要删除 合订册 对象?
                DialogResult result = MessageBox.Show(this,
    "合订册 '" + strPublishTime + "' 被拆散后，是否顺便删除原先代表合订册的对象?\r\n\r\n(Yes: 删除；No: 移入自由区；Cancel: 保留在原地)",
    "BindingControls",
    MessageBoxButtons.YesNoCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button3);
                if (result == DialogResult.Yes)
                {
                    RemoveParentCell(parent_cell, false);

                }
                else if (result == DialogResult.No)
                {
                    RemoveParentCell(parent_cell, true);
                }
                else
                {
                    Debug.Assert(result == DialogResult.Cancel, "");

                    // 如果合订册要保留在原地，需要把原有合订范围的第一个成员册向右或者向左移开，避免落入原有合订范围内
                    IssueBindingItem issue = parent_item.Container;
                    int nCol = issue.IndexOfCell(parent_cell);
                    Debug.Assert(nCol != -1, "");

                    Cell first_member_cell = issue.GetCell(nCol + 1);
                    Debug.Assert(first_member_cell.IsMember == false, "刚刚解除合订，现在不可能是合订成员册了");
                    if (first_member_cell.item != null)
                    {
                        // 在右边找到一个空位, 将first_member_cell转移过去
                        issue.GetBlankSingleIndex(nCol + 2/*, parent_item*/);
                        // issue.SetCell(nCol + 1, null);
                        issue.SetCell(nCol + 2, first_member_cell);

                        // 原来位置改为加入一个空白格子
                        {
                            Cell cell = new Cell();
                            cell.ParentItem = parent_item;
                            issue.SetCell(nCol + 1, cell);
                            Debug.Assert(cell.Container != null, "");
                            parent_item.InsertMemberCell(cell);
                        }
                    }

                    // 去掉先前记忆的第一项，即，合订本位置不再需要后面删除
                    nullpos_list.RemoveAt(0);
                }
            }

            // 删除空白位置
            for (int i = 0; i < nullpos_list.Count; i++)
            {
                Point p = nullpos_list[i];
                IssueBindingItem issue = this.Issues[p.Y];
                Debug.Assert(issue != null, "");
                Cell cell = issue.Cells[p.X];
                if (cell == null)
                {
                    // 探测是否为合订成员占据的位置
                    // return:
                    //      -1  是。并且是双格的左侧位置
                    //      0   不是
                    //      1   是。并且是双格的右侧位置
                    int nRet = issue.IsBoundIndex(p.X);
                    if (nRet == -1 || nRet == 1)
                    {
                        Debug.Assert(nRet != -1, "按理说这里不太可能出现双格左侧");
                        Debug.Assert(nRet != 1, "");
                    }
                    else
                        issue.RemoveSingleIndex(p.X);
                }
            }

            this.Invalidate();
            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 获得一个格子的坐标x/y。注意，是行列号，而不是象素位置
        Point GetCellPosition(Cell cell)
        {
            Debug.Assert(cell != null, "");

            IssueBindingItem issue = cell.Container;
            Debug.Assert(issue != null, "");
            int nLineNo = this.Issues.IndexOf(issue);
            Debug.Assert(nLineNo != -1, "");
            int nCol = issue.IndexOfCell(cell);
            Debug.Assert(nCol != -1, "");
            return new Point(nCol, nLineNo);
        }


        Cell GetCell(Point p)
        {
            Debug.Assert(p.X >= 0, "");
            Debug.Assert(p.Y >= 0, "");

            IssueBindingItem issue = this.Issues[p.Y];
            return issue.Cells[p.X];
        }

        // 判断一个格子是否为合订册
        internal bool IsBindingParent(Cell cellTest)
        {
            Debug.Assert(cellTest != null, "");

            if (cellTest.item == null)
                return false;
            if (this.ParentItems.IndexOf(cellTest.item) != -1)
                return true;
            return false;
        }

        // 判断一个Item是否为合订册
        bool IsBindingParent(ItemBindingItem itemTest)
        {
            Debug.Assert(itemTest != null, "");

            if (this.ParentItems.IndexOf(itemTest) != -1)
                return true;
            return false;
        }

        /*
        // 判断一个格子是否属于已有的装订范围?
        // return:
        //      1   属于装订范围，即为某个合订册的成员册。cellParent中返回了合订册格子对象
        //      0   不属于装订范围。即，不是合订成员册，而是单册或合订册。
        int IsBelongToBinding(Cell cellTest,
            out Cell cellParent,
            out string strError)
        {
            cellParent = null;
            strError = "";

            for (int i = 0; i < this.ParentItems.Count; i++)
            {
                ItemBindingItem parent_item = this.ParentItems[i];
                int index = parent_item.MemberCells.IndexOf(cellTest);
                if (index != -1)
                {
                    cellParent = parent_item.ContainerCell;
                    return 1;
                }
            }

            return 0;
        }
         * */

        // 获得成员册所从属的合订册
        // 如果用合订册调用本函数，则返回null
        Cell GetBindingParent(Cell cellTest)
        {
            if (cellTest == null)
            {
                Debug.Assert(false, "");
                return null;
            }

            if (cellTest.ParentItem != null)
                return cellTest.ParentItem.ContainerCell;

            return null;
        }

        // 找出属于特定的期的成员册对象
        Cell GetMemberCellByIssue(Cell parent_cell,
            IssueBindingItem issue)
        {
            for (int i = 0; i < parent_cell.item.MemberCells.Count; i++)
            {
                Cell member_cell = parent_cell.item.MemberCells[i];

                if (member_cell.Container == issue)
                    return member_cell;
            }

            return null;
        }

        // 做拖拽结束时的功能
        void DoDragEndFunction()
        {
            string strError = "";
            int nRet = 0;

            // 如果源是单册，目标为合订册或成员册，就做“将若干单册加入一个合订册”
            Cell source = (Cell)this.DragStartObject;
            Cell target = null;
            NullCell target_null = null;

            if (this.DragLastEndObject is Cell)
                target = (Cell)this.DragLastEndObject;
            else if (this.DragLastEndObject is NullCell)
            {
                target_null = (NullCell)this.DragLastEndObject;
            }

            if (source == null)
                return;

            List<Cell> source_cells = new List<Cell>();

            if (this.SelectedCells.IndexOf(source) != -1)
            {
                // 源单元属于已经选择的范围
                source_cells.AddRange(this.SelectedCells);

                // TODO: 检查数组中单元的一致性：要么都属于未装订的单册，要么都属于已经装订的成员册
            }
            else
            {
                // 源单元不属于已经选择的范围
                source_cells.Add(source);
            }

#if DEBUG
            VerifyListCell(source_cells);
#endif

            if (source != null
                && !(target == null && target_null == null))
            {
                Cell source_parent = null;
                Cell target_parent = null;

                // 准备源合订册对象
                if (IsBindingParent(source_cells[0]) == true)
                    source_parent = source_cells[0];
                else
                {
                    /*
                    nRet = IsBelongToBinding(source_cells[0],
                        out source_parent,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                     * */
                    source_parent = GetBindingParent(source_cells[0]);
                }

                // 准备目标合订册对象
                if (target != null)
                {
                    if (IsBindingParent(target) == true)
                        target_parent = target;
                    else
                    {
                        // 判断target格子是否属于已有的装订范围?
                        /*
                        nRet = IsBelongToBinding(target,
                            out target_parent,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                         * */
                        target_parent = GetBindingParent(target);
                    }
                }
                else if (target_null != null)
                {
                    target_parent = BelongToBinding(target_null);
                }

                // 1)
                // 单册的格子拖入合订范围
                if (source_parent == null && target_parent != null)
                {
                    // 将若干单册加入一个合订册
                    // return:
                    //      -1  出错。注意函数返回后，parent 格子需要及时清除
                    //      0   成功
                    nRet = AddToBinding(source_cells,
                        target_parent,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    return;
                }

                // 2)
                // 合订的格子拖入单册范围
                if (source_parent != null
                    && target_parent == null)
                {
                    Debug.Assert(this.ParentItems.IndexOf(source_parent.item) != -1, "源合订册应该是this.BindItems内元素");

                    // 进行检查，要求source_cells中所有成员都是成员册
                    for (int i = 0; i < source_cells.Count; i++)
                    {
                        Cell temp = source_cells[i];
                        Cell temp_parent = null;
                        /*
                        nRet = IsBelongToBinding(temp,
                            out temp_parent,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                         * */
                        temp_parent = GetBindingParent(temp);

                        if (temp_parent == null)
                        {
                            strError = "要求所拖拽的格子都是合订成员";
                            goto ERROR1;
                        }
                    }

                    // 将合订成员册从合订册中移出，成为单册
                    // TODO: 有可能出现前段member为NullCell的非常情况，请测试IsBinded
                    nRet = RemoveFromBinding(
                        false,
                        false,
                        source_cells,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
                    return;
                }

                // 3)
                // 合订的格子拖入另一个合订范围
                if (source_parent != null
                    && target_parent != null)
                {
                    Debug.Assert(this.ParentItems.IndexOf(source_parent.item) != -1, "源合订册应该是this.BindItems内元素");
                    Debug.Assert(this.ParentItems.IndexOf(target_parent.item) != -1, "目标合订册应该是this.BindItems内元素");

                    // 进行检查，要求...
                    for (int i = 0; i < source_cells.Count; i++)
                    {
                        Cell temp = source_cells[i];

                        if (temp.item != null)
                        {
                            if (StringUtil.IsInList("注销", temp.item.State) == true)
                            {
                                strError = "出版日期为 '" + temp.item.PublishTime + "' 的册记录状态为“注销”，不能被拖入另一合订册";
                                goto ERROR1;
                            }
                        }

                        // 进行检查，要求source_cells中所有成员都是成员册
                        Cell temp_parent = null;
                        /*
                        nRet = IsBelongToBinding(temp,
                            out temp_parent,
                            out strError);
                        if (nRet == -1)
                            goto ERROR1;
                         * */
                        temp_parent = GetBindingParent(temp);

                        if (temp_parent == null)
                        {
                            strError = "要求所拖拽的格子都是合订成员";
                            goto ERROR1;
                        }

                        // 检查源合订册是否为固化状态
                        if (CheckProcessingState(temp_parent.item) == false)
                        {
                            strError = "源合订册 '" + temp_parent.item.PublishTime + "' 为固化状态，不能从中拖出单册";
                            goto ERROR1;
                        }

                        // 检查源合订册是否为锁定状态
                        if (temp_parent.item.Locked == true)
                        {
                            strError = "源合订册 '" + temp_parent.item.PublishTime + "' 为锁定状态，不能从中拖出单册";
                            goto ERROR1;
                        }
                    }

                    // 需要预先检查，即将拖入target_parent范围的对象，是否和已有的期重复了
                    // 如果不检查，就可能在拖出时候不报错而在拖入的时候报错
                    // TODO: 这种情况可以当作交换对象来实现？出现MessageBox()询问
                    for (int i = 0; i < source_cells.Count; i++)
                    {
                        Cell cell = source_cells[i];
                        Debug.Assert(cell.Container != null, "");

                        Cell dup_cell = GetMemberCellByIssue(target_parent,
                            cell.Container);
                        if (dup_cell != null && dup_cell.item != null)
                        {
                            strError = "目标合订册中已有出版日期为 " + dup_cell.Container.PublishTime + " 的成员册，不能拖入出版日期相同的成员册";
                            goto ERROR1;
                        }


                    }

                    if (target_parent.item != null)
                    {
                        // 检查目标是否为固化状态
                        if (CheckProcessingState(target_parent.item) == false)
                        {
                            strError = "目标合订册 '" + target_parent.item.PublishTime + "' 为固化状态，不能再拖入单册";
                            goto ERROR1;
                        }

                        // 检查目标是否为锁定状态
                        if (target_parent.item.Locked == true)
                        {
                            strError = "目标合订册 '" + target_parent.item.PublishTime + "' 为锁定状态，不能再拖入单册";
                            goto ERROR1;
                        }
                    }

                    // 将合订成员册从合订册中移出，成为临时对象
                    nRet = RemoveFromBinding(
                        true,
                        false,  // TODO: 可以试验true，看看AddToBinding()是否可以适应
                        source_cells,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;

#if DEBUG
                    VerifyAll();
#endif

                    // 将若干单册加入一个合订册
                    // return:
                    //      -1  出错。注意函数返回后，parent 格子需要及时清除
                    //      0   成功
                    nRet = AddToBinding(source_cells,
                        target_parent,
                        out strError);
                    if (nRet == -1)
                        goto ERROR1;
#if DEBUG
                    {
                        string strError1 = "";
                        int nRet1 = target_parent.item.VerifyMemberCells(out strError1);
                        if (nRet1 == -1)
                        {
                            Debug.Assert(false, strError1);
                        }
                    }

#endif

#if DEBUG
                    VerifyAll();
#endif

                    return;
                }

                // 4)
                // 单册的格子拖入单册范围。实际上是移动单册格子的位置
                if (source_parent == null && target_parent == null)
                {
                    if (source_cells.Count > 1)
                    {
                        strError = "目前只支持将一个单册格子拖动位置";
                        goto ERROR1;
                    }

                    // 目标为普通格子(非NullCell)
                    if (target != null)
                    {
                        // 跨期移动
                        if (target.Container != source_cells[0].Container)
                        {
                            Cell cell = source_cells[0];
                            if (cell.item != null && cell.item.IsParent == true
                                && cell.Container == this.FreeIssue)
                            {
                                DialogResult result = MessageBox.Show(this,
"来自于自由期的合订册格子 " + cell.item.PublishTime + "(参考ID:" + cell.item.RefID + ") 若被拖入其他期，将被改变为单册性质。\r\n\r\n是否继续?",
"BindingControls",
MessageBoxButtons.OKCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                                if (result == DialogResult.Cancel)
                                {
                                    strError = "拖动操作被放弃";
                                    goto ERROR1;
                                }
                                cell.item.IsParent = false;
                            }

                            /*
                            strError = "拖动时源和目标位置应属于同一期";
                            goto ERROR1;
                             * */
                            // 移动一个格子到不同的期
                            nRet = MoveToAnotherIssue(source_cells[0],
                                target.Container,
                                target.Container.IndexOfCell(target),
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                            return;
                        }

                        // 同一期内移动
                        IssueBindingItem issue = target.Container;
                        Debug.Assert(issue != null, "");
                        int nTargetCol = issue.IndexOfCell(target);
                        Debug.Assert(nTargetCol != -1, "");
                        int nSourceCol = source_cells[0].Container.IndexOfCell(source_cells[0]);
                        Debug.Assert(nSourceCol != -1, "");

                        if (nSourceCol == nTargetCol)
                            return;

                        if (issue.IssueLayoutState == IssueLayoutState.Binding)
                        {

                            // 探测是否为合订成员占据的位置
                            // return:
                            //      -1  是。并且是双格的左侧位置
                            //      0   不是
                            //      1   是。并且是双格的右侧位置
                            if (issue.IsBoundIndex(nSourceCol) != 0)
                            {
                                strError = "源不能是成员册";
                                goto ERROR1;
                            }
                            if (issue.IsBoundIndex(nTargetCol) != 0)
                            {
                                strError = "目标不能是成员册";
                                goto ERROR1;
                            }

                            nRet = issue.MoveSingleIndexTo(
                                nSourceCol,
                                nTargetCol,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            this.FocusObject = source_cells[0];

                            if (nRet == 0)
                                this.Invalidate();
                            else
                                this.AfterWidthChanged(true);
                        }
                        else
                        {
                            nRet = issue.MoveCellTo(
                               nSourceCol,
                               nTargetCol,
                               out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            if (nRet == 0)
                                this.Invalidate();
                            else
                                this.AfterWidthChanged(true);
                        }
                    }

                    // 目标为NullCell
                    if (target_null != null)
                    {
                        IssueBindingItem target_issue = this.Issues[target_null.Y];
                        Debug.Assert(target_issue != null, "");

                        // 跨期移动
                        if (target_issue != source_cells[0].Container)
                        {
                            // 检查源对象，是否为来自于自由期的合订性质的格子，如果是，需要修改item.IsParent为false
                            Cell cell = source_cells[0];
                            if (cell.item != null && cell.item.IsParent == true
                                && cell.Container == this.FreeIssue)
                            {
                                DialogResult result = MessageBox.Show(this,
"来自于自由期的合订册格子 " + cell.item.PublishTime + "(参考ID:" + cell.item.RefID + ") 若被拖入其他期，将被改变为单册性质。\r\n\r\n是否继续?",
"BindingControls",
MessageBoxButtons.OKCancel,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);
                                if (result == DialogResult.Cancel)
                                {
                                    strError = "拖动操作被放弃";
                                    goto ERROR1;
                                }
                                cell.item.IsParent = false;
                            }
                            /*
                            strError = "拖动时源和目标位置应属于同一期";
                            goto ERROR1;
                             * */
                            // 移动一个格子到不同的期
                            nRet = MoveToAnotherIssue(source_cells[0],
                                target_issue,
                                target_null.X,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;
                            return;
                        }

                        // 同一期内移动
                        int nTargetCol = target_null.X;
                        Debug.Assert(nTargetCol != -1, "");
                        int nSourceCol = source_cells[0].Container.IndexOfCell(source_cells[0]);
                        Debug.Assert(nSourceCol != -1, "");

                        if (nSourceCol == nTargetCol)
                            return;

                        if (target_issue.IssueLayoutState == IssueLayoutState.Binding)
                        {
                            if (target_issue.IsBoundIndex(nSourceCol) != 0)
                            {
                                strError = "源不能是成员册";
                                goto ERROR1;
                            }
                            if (target_issue.IsBoundIndex(nTargetCol) != 0)
                            {
                                strError = "目标不能是成员册";
                                goto ERROR1;
                            }

                            nRet = target_issue.MoveSingleIndexTo(
                                nSourceCol,
                                nTargetCol,
                                out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            this.FocusObject = source_cells[0];

                            if (nRet == 0)
                                this.Invalidate();
                            else
                                this.AfterWidthChanged(true);
                        }
                        else
                        {
                            nRet = target_issue.MoveCellTo(
    nSourceCol,
    nTargetCol,
    out strError);
                            if (nRet == -1)
                                goto ERROR1;

                            if (nRet == 0)
                                this.Invalidate();
                            else
                                this.AfterWidthChanged(true);
                        }

                    }
                    return;
                }
            }

            return;
        ERROR1:
            MessageBox.Show(this, strError);
        }

        // 移动一个格子到不同的期
        // parameters:
        //      nInsertIndex   要插入的单格index
        int MoveToAnotherIssue(Cell source_cell,
            IssueBindingItem target_issue,
            int nInsertIndex,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (source_cell.Container == target_issue)
            {
                strError = "本来就在同一期，不是跨期移动";
                return -1;
            }

            if (source_cell is GroupCell)
            {
                strError = "不能移动组首尾格子";
                return -1;
            }

#if DEBUG
            if (source_cell.item != null)
            {
                Debug.Assert(source_cell.item.IsParent == false, "本函数只能处理非合订册");
            }
#endif

            Debug.Assert(source_cell.IsMember == false, "本函数只能处理非成员的单册");

            IssueBindingItem source_issue = source_cell.Container;
            Debug.Assert(source_issue != null, "");

            string strOldVolumeString =
                VolumeInfo.BuildItemVolumeString(
                dp2StringUtil.GetYearPart(source_issue.PublishTime),
                source_issue.Issue,
                        source_issue.Zong,
                        source_issue.Volume);
            string strNewVolumeString =
                VolumeInfo.BuildItemVolumeString(
                dp2StringUtil.GetYearPart(target_issue.PublishTime),
                target_issue.Issue,
            target_issue.Zong,
            target_issue.Volume);

            string strMessage = "是否要将期 "
                + source_issue.PublishTime
                + " 中的格子移动到 期 "
                + target_issue.PublishTime
                + " 中？\r\n\r\n这样格子除了出版日期会改变为 "
                + target_issue.PublishTime
                + " 以外，卷期号也将从 '"
                + strOldVolumeString
                + "' 改为 '" + strNewVolumeString + "'。";

            DialogResult result = MessageBox.Show(this,
    strMessage,
    "BindingControls",
    MessageBoxButtons.YesNo,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
            if (result == DialogResult.No)
                return 0;   // 放弃了

            // GroupCell source_group = null;

            // 从原来的期移走
            int nOldCol = source_issue.IndexOfCell(source_cell);
            Debug.Assert(nOldCol != -1, "");
            if (nOldCol != -1)
            {
                /*
                Cell temp = source_issue.GetCell(nOldCol - 1);
                Debug.Assert(temp == null || temp.item == null, "双格的左侧位置应该没有内容");
                 * */
                if (source_issue.IssueLayoutState == IssueLayoutState.Binding)
                {
                    // 清除旧位置

                    if (source_cell.item != null
                        && source_cell.item.OrderInfoPosition.X != -1)
                    {
                        // 删除前部操作，要更新订购信息
                        nRet = source_cell.item.DoDelete(out strError);
                        if (nRet == -1)
                            return -1;
                    }

                    source_issue.RemoveSingleIndex(nOldCol);

                    // 失去和订购组的关系，变为计划外格子
                    if (source_cell.item != null)
                    {
                        source_cell.item.OrderInfoPosition.X = -1;
                        source_cell.item.OrderInfoPosition.Y = -1;
                    }
                }
                else
                {
                    Debug.Assert(source_issue.IssueLayoutState == IssueLayoutState.Accepting, "");

                    /*
                    if (source_cell.item != null)
                    {
                        if (source_cell.item.Calculated == true
                            || source_cell.item.OrderInfoPosition.X != -1)
                        {
                            source_group = source_cell.item.GroupCell;
                        }
                    }
                    if (source_issue.Cells.Count > nOldCol)
                        source_issue.Cells.RemoveAt(nOldCol);

                    if (source_group != null)
                    {
                        int nSourceOrderCountDelta = 0;
                        int nSourceArrivedCountDelta = 0;
                        nSourceOrderCountDelta--;
                        if (source_cell.item != null
                            && source_cell.item.Calculated == false)
                            nSourceArrivedCountDelta--;

                        source_group.RefreshGroupMembersOrderInfo(nSourceOrderCountDelta,
            nSourceArrivedCountDelta);
                    }
                    */
                    DeleteOneCellInAcceptingLayout(source_cell);
                }
            }

            // 需要检查目标位置，不能是合订本所占据的列
            // 可能会改变格局，nSourceNo会变得无效
            if (nInsertIndex != -1)
            {
            }
            else
            {
                // TODO: 改为获得末尾的第一个空位
                if (target_issue.IssueLayoutState == IssueLayoutState.Binding)
                    nInsertIndex = target_issue.GetFirstAvailableSingleInsertIndex();
                else
                    nInsertIndex = target_issue.GetFirstFreeIndex();
            }

            if (target_issue.IssueLayoutState == IssueLayoutState.Binding)
                target_issue.GetNewSingleIndex(nInsertIndex);
            else
            {
                if (nInsertIndex < target_issue.Cells.Count)
                    target_issue.Cells.Insert(nInsertIndex, null);
            }

            target_issue.SetCell(nInsertIndex, source_cell);
            if (source_cell.item != null)
                source_cell.item.Container = target_issue;

            GroupCell target_group = null;  // 只有在Acception模式下才可能有值

            if (target_issue.IssueLayoutState == IssueLayoutState.Accepting)
                target_group = target_issue.BelongToGroup(nInsertIndex);

            bool bSourceHasOrderInfo = false;

            if (source_cell.item != null)
                bSourceHasOrderInfo = source_cell.item.OrderInfoPosition.X != -1;

            // 如果源被从组区域拖动到计划外区域
            if (bSourceHasOrderInfo && target_group == null)
            {
                if (source_cell.item != null)
                {
                    source_cell.item.OrderInfoPosition.X = -1;
                    source_cell.item.OrderInfoPosition.Y = -1;
                    if (source_cell.item.Calculated == true)
                    {
                        // 预测格子变为普通空白格子
                        source_cell.item = null;
                    }
                }
            }

            // 如果源被从计划外区域拖动到组区域
            if (bSourceHasOrderInfo == false && target_group != null)
            {
                if (source_cell.item == null)
                {
                    // 空白格子要变为预测格式
                    source_cell.item = new ItemBindingItem();
                    source_cell.item.Container = target_issue;
                    source_cell.item.Initial("<root />", out strError);
                    source_cell.item.RefID = "";
                    source_cell.item.LocationString = "";
                    source_cell.item.Calculated = true;
                    IssueBindingItem.SetFieldValueFromOrderInfo(
                        false,
                        source_cell.item,
                        target_group.order);
                }
            }

            // 2010/9/21
            // 把移动到自由期的预测状态的格子变为空白格子
            if (source_cell.item != null)
            {
                if (String.IsNullOrEmpty(source_cell.Container.PublishTime) == true
        && source_cell.item.Calculated == true)
                {
                    // 预测格子变为普通空白格子
                    source_cell.item = null;
                }
            }

            Cell target_cell = source_cell;
            if (target_cell.item != null)
            {
                // 修改册记录内的字段
                target_cell.item.Volume = strNewVolumeString;
                target_cell.item.PublishTime = target_issue.PublishTime;
                target_cell.item.Changed = true;
            }
            target_cell.RefreshOutofIssue();

            if (target_group != null)
            {
                int nTargetOrderCountDelta = 0;
                int nTargetArrivedCountDelta = 0;
                nTargetOrderCountDelta++;
                if (source_cell.item != null
                    && source_cell.item.Calculated == false)
                    nTargetArrivedCountDelta++;
                target_group.RefreshGroupMembersOrderInfo(nTargetOrderCountDelta,
    nTargetArrivedCountDelta);
            }

            //this.Invalidate();   // TODO: 修改为最少失效。源格子和其右边的区域，目标格子和右边的区域
            this.AfterWidthChanged(true);

            return 1;   // 作了
        }

        // 从所选择的格子中选择期号最小的一个，获得它的列号
        int DetectFirstMemberCol(List<Cell> members)
        {
            Debug.Assert(members.Count > 0, "");
            members.Sort(new CellPublishTimeComparer());
            Cell cell = members[0];
            return cell.Container.Cells.IndexOf(cell);
        }

        // 将若干单册加入一个合订册
        // return:
        //      -1  出错。注意函数返回后，parent 格子需要及时清除
        //      0   成功
        int AddToBinding(List<Cell> singles,
            Cell parent,
            out string strError)
        {
            strError = "";
            Debug.Assert(parent != null && parent.item != null, "");

            if (CheckProcessingState(parent.item) == false
                && parent.item.Calculated == false   // 预测格子除外
                    && parent.item.Deleted == false)  // 已经删除的格子除外
            {
                strError = "合订册 '" + parent.item.PublishTime + "' 为固化状态，不能再加入单册";
                return -1;
            }

            if (parent.item.Locked == true
    && parent.item.Calculated == false   // 预测格子除外
        && parent.item.Deleted == false)  // 已经删除的格子除外
            {
                strError = "合订册 '" + parent.item.PublishTime + "' 为锁定状态，不能再加入单册";
                return -1;
            }

            // 2010/4/6
            parent.item.Calculated = false;
            parent.item.Deleted = false;
            if (String.IsNullOrEmpty(parent.item.RefID) == true)
                parent.item.RefID = Guid.NewGuid().ToString();

            // 检查：singles中的成员，应该和parent下属的格子的期不重叠
            for (int i = 0; i < singles.Count; i++)
            {
                Cell single = singles[i];
                IssueBindingItem issue = single.Container;

                Debug.Assert(issue.Cells.IndexOf(single) != -1, "");

                for (int j = 0; j < parent.item.MemberCells.Count; j++)
                {
                    Cell exist_cell = parent.item.MemberCells[j];
                    if (exist_cell.Container == single.Container
                        && exist_cell.item != null)
                    {
                        strError = "合订册中已经包含了出版日期为 '" + issue.PublishTime + "' 的(非空白)册(格子)，不能重复加入";
                        return -1;
                    }
                }
            }

#if NO
            // 不允许进行合订的册状态
            string[] states = new string[] { "注销", "丢失" };

            // 检查单册，“注销”状态的不能加入合订范围
            // 预测状态的不能加入合订范围
            for (int i = 0; i < singles.Count; i++)
            {
                Cell single = singles[i];
                Debug.Assert(single != null, "");

                if (single is GroupCell)
                {
                    strError = "订购组首尾格子不能参与合订";
                    return -1;
                }

                if (String.IsNullOrEmpty(single.Container.PublishTime) == true)
                {
                    strError = "来自于自由期的格子不能加入合订范围";
                    return -1;
                }

                if (single.item == null)
                    continue;

                foreach (string state in states)
                {
                    if (StringUtil.IsInList(state, single.item.State) == true)   // "注销"
                    {
                        strError = "出版日期为 '" + single.item.PublishTime + "' 的单册记录状态为“" + state + "”，不能加入合订范围";
                        return -1;
                    }
                }

                if (String.IsNullOrEmpty(single.item.Borrower) == false)
                {
                    strError = "出版日期为 '" + single.item.PublishTime + "' 的单册记录目前处于被借阅状态，不能加入合订范围";
                    return -1;
                }

                if (single.item.Calculated == true)
                {
                    strError = "出版日期为 '" + single.item.PublishTime + "' 的格子为预测状态，不能加入合订范围";
                    return -1;
                }
            }

#endif

            int nCol = -1;

            if (parent.Container != null)
            {
                nCol = parent.Container.Cells.IndexOf(parent);
                Debug.Assert(nCol != -1, "");
                nCol++;
            }
            else
            {
                nCol = DetectFirstMemberCol(singles);
                Debug.Assert(nCol != -1, "");

                /*
                // 看看左边有没有空一格位置。如果有，就直接占用，如果没有，则偏右占用
                IssueBindingItem issue = singles[0].Container;
                Debug.Assert(issue != null, "");
                 * */
                nCol++;
            }

            // 安放下属的单独册
            PlaceMemberCells(parent,
                singles,
                nCol);
            try
            {
                SetBindingRange(parent, true);
            }
            catch (Exception ex)
            {
                strError = "BindingControl SetBindingRange() exception: " + ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            /*
            parent.item.RefreshBindingXml();
            parent.item.RefreshIntact();
            parent.item.Changed = true;
             * */

            /*
            PlaceBinding(
                parent,
                singles,
                out strPublishTimeString);
            parent.item.PublishTime = strPublishTimeString;

            // 创建<binding>元素内片断
            parent.item.RefreshBindingXml();

            parent.item.Changed = true;
             * */

#if DEBUG
            {
                string strError1 = "";
                int nRet1 = parent.item.VerifyMemberCells(out strError1);
                if (nRet1 == -1)
                {
                    Debug.Assert(false, strError1);
                }
            }

            VerifyAll();
#endif

            this.AfterWidthChanged(true);
            return 0;
        }

        // 将合订成员册从合订册中移出，成为单册
        // 注：这些成员册可能并不都属于同一个合订册
        // TODO: 需要新增加一个功能，移出的对象进入一个数组，而不必进入实际显示的格子。这些对象紧接着会被用来移入另外一个合订册
        // parameters:
        //      bShrink 是否在移走首位位置的格子时缩小装订范围
        //      bDelete 是否删除移出的格子。==false，则移出到外面，还存在；==true，则不存在
        int RemoveFromBinding(
            bool bShrink,
            bool bDelete,
            List<Cell> members,
            out string strError)
        {
            strError = "";
            int nRet = 0;

            if (bDelete == true)
            {
                // 检查
                for (int i = 0; i < members.Count; i++)
                {
                    Cell member_cell = members[i];
                    if (member_cell.item != null)
                    {
                        nRet = member_cell.item.CanDelete(out strError);
                        if (nRet == -1)
                            return -1;
                        if (nRet == 0)
                        {
                            strError = "册 " + member_cell.item.RefID + " 不能被删除: " + strError;
                            return -1;
                        }
                    }
                }
            }

            bool bClearAsBlank = true;  // 是否要在删除位置填充空白格子。==true表示要填充；否则就是null格子

            if (bShrink == true)
                bClearAsBlank = false;

            List<Cell> parent_cells = new List<Cell>(); // 最后统一处理SetBindingRange()
            for (int i = 0; i < members.Count; i++)
            {
                Cell member_cell = members[i];

                ItemBindingItem parent_item = member_cell.ParentItem;
                if (parent_item == null)
                    continue;   // TODO: 是否要警告?

                if (CheckProcessingState(parent_item) == false)
                {
                    strError = "合订册 '" + parent_item.PublishTime + "' 为固化状态，不能从中移出单册";
                    return -1;
                }

                // 检查合订册的锁定状态
                if (parent_item.Locked == true)
                {
                    strError = "合订册 '" + parent_item.PublishTime + "' 为锁定状态，不能从中移出单册";
                    return -1;
                }

                Cell parent_cell = parent_item.ContainerCell;
                Debug.Assert(parent_cell != null, "");


                IssueBindingItem issue = member_cell.Container;
                Debug.Assert(issue != null, "");


                int nOldCol = issue.Cells.IndexOf(member_cell);
                Debug.Assert(nOldCol != -1, "");

                // 2010/3/3 
                bool bLastPos = false;
                // 如果是所有成员的最后一个，并且和Parent同在一期
                if (parent_item.MemberCells.Count <= 1
                    && parent_item.Container == member_cell.Container)
                {
                    bLastPos = true;
                }

                // 获得同一期的末尾/第一个可用位置
                int nNewCol = -1;
                if (bDelete == false || bLastPos == true)
                {
                    if (issue.IssueLayoutState == IssueLayoutState.Binding)
                    {
                        nNewCol = issue.GetFirstAvailableSingleInsertIndex();
                        Debug.Assert(nNewCol != -1, "");
                        Debug.Assert(nNewCol > nOldCol, "");
                    }
                    else
                    {
                        Debug.Assert(issue.IssueLayoutState == IssueLayoutState.Accepting, "");
                        nNewCol = issue.GetFirstFreeBlankIndex();
                        Debug.Assert(nNewCol != -1, "");
                    }
                }



                // 在原有位置添加空白格子
                // 两种语义：1)不负责缩小合订范围; 2)必要时要缩小合订范围。特别是最后一个合订册移走的时候要警告? 合订册中没有成员，算合订册么？要不然放入自由区

                bool bOldSeted = false; // 旧位置是否已经被处理安放?

                if (bClearAsBlank == false
                    && bDelete == true
                    ) // && bLastPos == false
                {
                    // 看看是否有订购信息绑定
                    if (member_cell.item != null
                        && member_cell.item.OrderInfoPosition.X != -1)
                    {
                        nRet = member_cell.item.DoDelete(out strError);
                        if (nRet == -1)
                            return -1;
                    }
                    // 清除为NullCell
                    issue.SetCell(nOldCol, null);
                    if (issue.IssueLayoutState == IssueLayoutState.Accepting)
                    {
                        if (nOldCol < issue.Cells.Count)
                        {
                            issue.Cells.RemoveAt(nOldCol);
                            if (nNewCol != -1)
                                nNewCol = issue.GetFirstFreeBlankIndex();   // 重新生成
                        }
                    }
                    bOldSeted = true;
                }

                parent_item.MemberCells.Remove(member_cell);
                this.m_bChanged = true;

                if (bClearAsBlank == true || bLastPos == true)
                {
                    // 清除为空白格子
                    Cell blank_cell = new Cell();
                    blank_cell.Container = issue;
                    blank_cell.ParentItem = parent_item;
                    parent_item.InsertMemberCell(blank_cell);
                    if (issue.IssueLayoutState == IssueLayoutState.Binding)
                    {
                        issue.SetCell(nOldCol, blank_cell);
                        bOldSeted = true;
                    }
                    else
                    {
                        Debug.Assert(nNewCol != -1, "");
                        issue.SetCell(nNewCol, blank_cell); // 新位置创建空白的格子，作为合订成员
                    }
                }

                if (bDelete == false
                    && issue.IssueLayoutState == IssueLayoutState.Binding)
                {
                    // 在装订布局下，不改变订购绑定的移动，可以随意进行，
                    // 不必调用DoDelete()函数
                    if (bOldSeted == false)
                    {
                        // 清除为NullCell
                        Debug.Assert(nOldCol != -1, "");
                        issue.SetCell(nOldCol, null);
                        bOldSeted = true;
                    }

                    // 找一个新位置安放
                    Debug.Assert(nNewCol != -1, "");
                    issue.GetNewSingleIndex(nNewCol);
                    issue.SetCell(nNewCol, member_cell);
                }

                if (member_cell.item != null)
                    member_cell.item.ParentItem = null;  // 变为非成员
                member_cell.ParentItem = null; // 变为非成员

                // 如果装订范围发生变化
                // parent_item.PublishTime = strPublishTimeString;

                /*
                parent_item.RefreshBindingXml();
                parent_item.RefreshIntact();
                parent_item.Changed = true;
                 * */

                if (bShrink == true)
                {
                    /*
#if DEBUG
                    if (parent_cell.item.MemberCells.Count == 0)
                    {
                        VerifyAll();
                    }
#endif
                     * */
                    /*
                    try
                    {
                        SetBindingRange(parent_cell, true);
                    }
                    catch (Exception ex)
                    {
                        strError = ex.Message;
                        return -1;
                    }
                     * */

                    // 带有去重的能力
                    if (parent_cells.IndexOf(parent_cell) == -1)
                        parent_cells.Add(parent_cell);
                }
                else
                {
                    parent_cell.item.AfterMembersChanged(); // 虽然不压缩范围，但是成员可能发生变化

#if DEBUG
                    VerifyListCell(parent_item.MemberCells);
#endif

#if DEBUG
                    {
                        string strError1 = "";
                        int nRet1 = parent_item.VerifyMemberCells(out strError1);
                        if (nRet1 == -1)
                        {
                            Debug.Assert(false, strError1);
                        }
                    }
#endif
                }

            }

            // 统一集中处理
            for (int i = 0; i < parent_cells.Count; i++)
            {
                Cell parent_cell = parent_cells[i];
                try
                {
                    SetBindingRange(parent_cell, true);
                }
                catch (Exception ex)
                {
                    strError = "BindingControl SetBindingRange() {B59FB04A-3F0A-45DE-B527-9A73167BB03C} exception: " + ex.Message;
                    return -1;
                }
#if DEBUG
                VerifyListCell(parent_cell.item.MemberCells);
#endif

#if DEBUG
                {
                    string strError1 = "";
                    int nRet1 = parent_cell.item.VerifyMemberCells(out strError1);
                    if (nRet1 == -1)
                    {
                        Debug.Assert(false, strError1);
                    }
                }
#endif
            }


#if DEBUG
            VerifyAll();
#endif

            this.AfterWidthChanged(true);
            return 0;
        }

        // 可能会抛出异常
        // 收缩合订册包含的范围，填充中部的NullCell。
        // 尾部不需要检查，只需要检查头部
        // parameters:
        //      bBackSetParent  是否要把成员信息的修改兑现到parent对象?
        void SetBindingRange(Cell parent_cell,
            bool bBackSetParent)
        {
            int nCol = -1;

            if (parent_cell.item.MemberCells.Count == 0)
            {
                ItemBindingItem item = parent_cell.item;
                IssueBindingItem issue = parent_cell.Container;

                if (issue == null)
                {
                    Debug.Assert(false, "");
                    goto END1;
                }

                if (issue.IssueLayoutState == IssueLayoutState.Binding)
                {
                    nCol = issue.Cells.IndexOf(parent_cell);
                    if (nCol == -1)
                    {
                        Debug.Assert(false, "");
                        goto END1;
                    }
                    nCol++;
                }
                else
                {
                    // 随便找到一个空白格子(不需要两个)
                    nCol = issue.GetFirstFreeBlankIndex();
                }

                // 补充窟窿
                {
                    int nSetCol = -1;
                    if (issue.IssueLayoutState == IssueLayoutState.Binding)
                    {
                        if (issue.IsBlankDoubleIndex(nCol - 1) == true)
                        {
                            Cell cell = issue.GetCell(nCol);
                            if (cell == null)
                            {
                                cell = new Cell();
                                issue.SetCell(nCol, cell);
                            }
                            Debug.Assert(cell != null
                                && cell.item == null
                                && cell.IsMember == false, "");
                            cell.ParentItem = parent_cell.item;
                            parent_cell.item.InsertMemberCell(cell);
                            goto END1;
                        }
                        // issue.GetBlankPosition(nCol / 2, parent_cell.item);
                        issue.GetBlankDoubleIndex(nCol - 1,
                            parent_cell.item,
                            null);
                        nSetCol = nCol;
                    }
                    else
                    {
                        // 随便找到一个空白格子(不需要两个)。接近nCol位置更好
                        nSetCol = issue.GetFirstFreeBlankIndex();
                        Debug.Assert(nSetCol != -1, "");
                    }

                    {
                        Cell cell = new Cell();
                        cell.item = null;   // 只是占据位置
                        cell.ParentItem = parent_cell.item;
                        issue.SetCell(nSetCol, cell);

                        // 放在合适位置
                        parent_cell.item.InsertMemberCell(cell);
                    }
                }

            END1:
                if (bBackSetParent == true)
                    parent_cell.item.AfterMembersChanged();
                return;
            }

            // string strPublishTimeString = "";

            List<IssueBindingItem> done_issues = new List<IssueBindingItem>();
            int nFirstLineNo = 99999;
            int nLastLineNo = -1;
            for (int i = 0; i < parent_cell.item.MemberCells.Count; i++)
            {
                Cell cell = parent_cell.item.MemberCells[i];

                ItemBindingItem item = cell.item;
                IssueBindingItem issue = cell.Container;

                if (issue == null)
                {
                    Debug.Assert(false, "");
                }

                Debug.Assert(issue != null, "");

                Debug.Assert(issue.Cells.IndexOf(cell) != -1, "");

                done_issues.Add(issue);

                int nIssueLineNo = this.Issues.IndexOf(issue);
                if (nFirstLineNo > nIssueLineNo)
                    nFirstLineNo = nIssueLineNo;

                if (nLastLineNo < nIssueLineNo)
                    nLastLineNo = nIssueLineNo;
            }

            Debug.Assert(nFirstLineNo != 99999, "");
            Debug.Assert(nLastLineNo != -1, "");

            // bool bChanged = false;

            // 合订册格子发生变动
            IssueBindingItem first_issue = this.Issues[nFirstLineNo];
            Debug.Assert(first_issue != null, "");

            int nOldCol = -1;
            IssueBindingItem old_first_issue = parent_cell.Container;
            if (old_first_issue != null)
            {
                nOldCol = old_first_issue.Cells.IndexOf(parent_cell);
                Debug.Assert(nOldCol != -1, "");
            }

            // 关于first_issue
            {
                if (first_issue.IssueLayoutState == IssueLayoutState.Binding)
                {
                    Debug.Assert(first_issue != null, "");
                    for (int i = 0; i < parent_cell.item.MemberCells.Count; i++)
                    {
                        nCol = first_issue.Cells.IndexOf(parent_cell.item.MemberCells[i]);
                        if (nCol != -1)
                            break;
                    }
                    Debug.Assert(nCol != -1, "");
                    nCol--;
                }
                else
                {
                    // 随便找到一个空白格子(不需要两个)
                    nCol = first_issue.GetFirstFreeBlankIndex();
                }
            }

            // 如果合订册行不等于其一个成员册的行
            if (parent_cell.Container != first_issue)
            {
                if (old_first_issue != null)
                    old_first_issue.SetCell(nOldCol, null);

                // TODO: 尽量找到binding布局的一个成员已经用过的列，这样就不用搬动成员了

                first_issue.SetCell(nCol, parent_cell);
                // 2010/3/29
                // TODO: 如果新的parent所在行为binding布局，那么要将所有binding布局的成员行中的格子移位，满足和这个parent的竖位置关系
                PlaceMemberCells(parent_cell,
                    parent_cell.item.MemberCells,
                    nCol + 1);
                // goto REDO;

                Debug.Assert(parent_cell.Container == first_issue, "");
                parent_cell.item.Container = first_issue;

                // bChanged = true;
            }

            /*
            strPublishTimeString = this.Issues[nFirstLineNo].PublishTime
            + "-"
            + this.Issues[nLastLineNo].PublishTime;

            if (parent_cell.item.PublishTime != strPublishTimeString)
                bChanged = true;
             * */


            nCol++; // nCol对Accepting行不起作用
            // 补充窟窿
            for (int i = nFirstLineNo; i <= nLastLineNo; i++)
            {
                IssueBindingItem issue = this.Issues[i];
                if (done_issues.IndexOf(issue) != -1)
                    continue;

                int nSetCol = -1;
                if (issue.IssueLayoutState == IssueLayoutState.Binding)
                {
                    if (issue.IsBlankDoubleIndex(nCol - 1) == true)
                    {
                        Cell cell = issue.GetCell(nCol);
                        if (cell == null)
                        {
                            cell = new Cell();
                            issue.SetCell(nCol, cell);
                        }
                        Debug.Assert(cell != null
                            && cell.item == null
                            && cell.IsMember == false, "");
                        cell.ParentItem = parent_cell.item;
                        parent_cell.item.InsertMemberCell(cell);
                        continue;
                    }

                    /*
                    {
                        Cell cell = issue.GetCell(nCol);
                        // 如果是空白格子，而且无主，则直接使用
                        // TODO: 有点问题？这个空白格子的左方的格子呢？是否为被占据的?
                        if (cell != null
                            && cell.item == null
                            && cell.IsMember == false)
                        {
                            cell.ParentItem = parent_cell.item;
                            parent_cell.item.InsertMemberCell(cell);
                            // bChanged = true;
                            continue;
                        }
                    }
                     * */

                    // issue.GetBlankPosition(nCol / 2, parent_cell.item);
                    issue.GetBlankDoubleIndex(nCol - 1,
                        parent_cell.item,
                        null);
                    nSetCol = nCol;
                }
                else
                {
                    // 随便找到一个空白格子(不需要两个)。接近nCol位置更好
                    nSetCol = issue.GetFirstFreeBlankIndex();
                    Debug.Assert(nSetCol != -1, "");
                }

                {
                    Cell cell = new Cell();
                    cell.item = null;   // 只是占据位置
                    cell.ParentItem = parent_cell.item;
                    issue.SetCell(nSetCol, cell);

                    // 放在合适位置
                    parent_cell.item.InsertMemberCell(cell);
                    // bChanged = true;
                }
            }

            if (bBackSetParent == true)
                parent_cell.item.AfterMembersChanged();
        }

        void RemoveParentCell(Cell cell,
            bool bAddToFree)
        {
            Debug.Assert(cell.item != null, "");

            if (this.ParentItems.IndexOf(cell.item) == -1)
            {
                Debug.Assert(false, "");
                return;
            }

            // 从显示格子中去掉
            // 从原来从属的期行中移走
            // TODO: 导致压缩?
            int index = cell.Container.Cells.IndexOf(cell);
            Debug.Assert(index != -1, "");
            cell.Container.SetCell(index, null);

            // 从已装订册集合中移走
            this.ParentItems.Remove(cell.item);

            /*
            IssueBindingItem issue = item.Container;
            Debug.Assert(issue != null, "");
            if (issue != null)
            {
                int nIndex = issue.IndexOfItem(item);
                Debug.Assert(nIndex != -1, "");
                if (nIndex != -1)
                {
                    Cell cell = issue.GetCell(nIndex);
                    cell.item = null;
                    cell.ParentItem = null;
                }
            }
             * */
            // 加入自由期
            if (bAddToFree == true)
                AddToFreeIssue(cell);
        }

        /*
        void RemoveBindItem(ItemBindingItem item,
    bool bAddToFree)
        {
            if (this.BindItems.IndexOf(item) == -1)
                return;

            // 从已装订册集合中移走
            this.BindItems.Remove(item);

            // 从原来从属的期行中移走
            IssueBindingItem issue = item.Container;
            Debug.Assert(issue != null, "");
            if (issue != null)
            {
                int nIndex = issue.IndexOfItem(item);
                Debug.Assert(nIndex != -1, "");
                if (nIndex != -1)
                {
                    Cell cell = issue.GetCell(nIndex);
                    cell.item = null;
                    cell.ParentItem = null;
                }
            }

            // 加入自由期
            if (bAddToFree == true)
                AddToFreeIssue(item);
        }
         * */

        /*
        // 加入自由期
        void AddToFreeIssue(ItemBindingItem item)
        {
            Debug.Assert(this.FreeIssue != null, "");

            Cell cell = new Cell();
            cell.item = item;
            this.FreeIssue.AddCell(cell);
            item.Container = this.FreeIssue;
        }
         * */

        // 加入自由期
        void AddToFreeIssue(Cell cell)
        {
            Debug.Assert(cell.item != null, "");
            Debug.Assert(this.FreeIssue != null, "");

            this.FreeIssue.AddCell(cell);
            Debug.Assert(cell.Container == this.FreeIssue, "");
            cell.item.Container = this.FreeIssue;
        }

        public List<Cell> SelectedCells
        {
            get
            {
                List<Cell> results = new List<Cell>();
                for (int i = 0; i < this.Issues.Count; i++)
                {
                    IssueBindingItem issue = this.Issues[i];
                    List<Cell> temp = issue.SelectedCells;
                    if (temp.Count > 0)
                        results.AddRange(temp);
                }

                return results;
            }
        }

        public List<IssueBindingItem> SelectedIssues
        {
            get
            {
                List<IssueBindingItem> results = new List<IssueBindingItem>();
                for (int i = 0; i < this.Issues.Count; i++)
                {
                    IssueBindingItem issue = this.Issues[i];
                    if (issue.Selected == true)
                        results.Add(issue);
                }

                return results;
            }
        }

        // 是否有单元被选择?
        public bool HasCellSelected()
        {
            for (int i = 0; i < this.Issues.Count; i++)
            {
                IssueBindingItem issue = this.Issues[i];
                if (issue.HasCellSelected() == true)
                    return true;
            }

            return false;
        }

        // 是否有单元被选择?
        public bool HasIssueSelected()
        {
            for (int i = 0; i < this.Issues.Count; i++)
            {
                IssueBindingItem issue = this.Issues[i];
                if (issue.Selected == true)
                    return true;
            }

            return false;
        }

        /*
        // 检查是否一期只有一个册。期可以不连续
        // 检查：一期只能有一个册参与。也就是说，每个册的Container不能相同
        // return:
        //      -1  error
        //      0   合格
        //      1   不合格。strError中有提示
        static int CheckBindingItems(List<ItemBindingItem> items,
            out string strError)
        {
            strError = "";

            for (int i = 0; i < items.Count; i++)
            {
                ItemBindingItem item = items[i];
                if (item == null)
                    continue;

                for (int j = i + 1; j < items.Count; j++)
                {
                    ItemBindingItem item1 = items[j];
                    if (item1 == null)
                        continue;

                    if (item.Container == item1.Container)
                    {
                        strError = "有同属于一期 (" + item.PublishTime + ") 的多册";
                        return 1;
                    }
                }
            }

            return 0;
        }
         * */

        // 检查参与装订的册。检查应该尽量在这里进行，以避免创建合订册格子以后，再检查成员格子发现不合格去删除合订册格子
        // 检查是否一期只有一个册。期可以不连续
        // 检查：一期只能有一个册参与。也就是说，每个册的Container不能相同
        // return:
        //      -1  error
        //      0   合格
        //      1   不合格。strError中有提示
        static int CheckBindingCells(List<Cell> cells,
            out string strError)
        {
            strError = "";

            for (int i = 0; i < cells.Count; i++)
            {
                Cell cell = cells[i];
                if (cell == null)
                {
                    Debug.Assert(false, "");
                    continue;
                }

                if (cell.item != null
                    && StringUtil.IsInList("注销", cell.item.State) == true)
                {
                    strError = "出版日期为 '" + cell.item.PublishTime + "' 的单册记录状态为“注销”，不允许加入合订范围";
                    return 1;
                }

                if (cell.item != null
    && String.IsNullOrEmpty(cell.item.Borrower) == false)
                {
                    strError = "出版日期为 '" + cell.item.PublishTime + "' 的单册记录目前处于被借阅状态，不允许加入合订范围";
                    return 1;
                }

                /*
                if (cell.item == null)
                    continue;
                 * */
                Debug.Assert(cell.Container != null, "");

                ItemBindingItem item = cell.item;
                for (int j = i + 1; j < cells.Count; j++)
                {
                    Cell cell1 = cells[j];

                    Debug.Assert(cell1.Container != null, "");

                    if (cell.Container == cell1.Container)
                    {
                        strError = "有同属于一期 (" + cell.Container.PublishTime + ") 的多册";
                        return 1;
                    }
                }
            }

            // 检查参与合订的册的状态
            // return:
            //      -1  出错
            //      0   检查没有发现问题
            //      1   检查发现有问题
            int nRet = CheckBindingMemberStates(cells,
                out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 1)
                return 1;
            return 0;
        }


        // 检查参与合订的册的状态
        // return:
        //      -1  出错
        //      0   检查没有发现问题
        //      1   检查发现有问题
        static int CheckBindingMemberStates(List<Cell> singles,
            out string strError)
        {
            strError = "";

            // 不允许进行合订的册状态
            string[] states = new string[] { "注销", "丢失" };

            // 检查单册，“注销”状态的不允许加入合订范围
            // 预测状态的不能加入合订范围
            for (int i = 0; i < singles.Count; i++)
            {
                Cell single = singles[i];
                Debug.Assert(single != null, "");

                if (single is GroupCell)
                {
                    strError = "订购组首尾格子不能参与合订";
                    return 1;
                }

                if (String.IsNullOrEmpty(single.Container.PublishTime) == true)
                {
                    strError = "来自于自由期的格子不允许加入合订范围";
                    return 1;
                }

                if (single.item == null)
                    continue;

                foreach (string state in states)
                {
                    if (StringUtil.IsInList(state, single.item.State) == true)   // "注销"
                    {
                        strError = "出版日期为 '" + single.item.PublishTime + "' 的单册记录状态为“" + state + "”，不允许加入合订范围";
                        return 1;
                    }
                }

                if (String.IsNullOrEmpty(single.item.Borrower) == false)
                {
                    strError = "出版日期为 '" + single.item.PublishTime + "' 的单册记录目前处于被借阅状态，不允许加入合订范围";
                    return 1;
                }

                if (single.item.Calculated == true)
                {
                    strError = "出版日期为 '" + single.item.PublishTime + "' 的格子为预测状态，不允许加入合订范围";
                    return 1;
                }
            }

            return 0;
        }

#if NOOOOOOOOOOOOOOOOOOO
        // 安放合并本册和下属册的Cell位置
        // 本函数对parent_cell.item.MemberCells并不进行清除(解除关系)。如果要彻底重新创建，需要在调用本函数前自行清除
        void PlaceBinding(
            Cell parent_cell,
            List<Cell> member_cells,
            out string strPublishTimeString)
        {
            Debug.Assert(member_cells.Count != 0, "");

            strPublishTimeString = "";

            // parent_item.MemberCells.Clear();

            Debug.Assert(member_cells.Count > 0, "");

#if DEBUG
            VerifyListCell(parent_cell.item.MemberCells);
            VerifyListCell(member_cells);
#endif

            List<Cell> members = new List<Cell>();
            members.AddRange(parent_cell.item.MemberCells);
            members.AddRange(member_cells);

            members.Sort(new CellComparer());

            Cell first_cell = members[0];
            IssueBindingItem first_issue = first_cell.Container;
            Debug.Assert(first_issue != null, "");

            int nCol = -1;
            int nOldCol = parent_cell.Container.Cells.IndexOf(parent_cell);

            // 当前合订册所在的期位置不对，需要调整
            if (first_issue != parent_cell.Container)
            {
                // 把原来的位置设置为空。这样就不会引起GetFirstAvailableBindingColumn()误判
                if (nOldCol != -1)
                    parent_cell.Container.SetCell(nOldCol, null);

                nCol = first_issue.GetFirstAvailableBindingColumn();

                //设置到新的位置
                first_issue.SetCell(nCol, parent_cell);
                parent_cell.item.Container = first_issue;   // 假装属于这个期
            }
            else
            {
                // parent_cell position not changed
                nCol = nOldCol;
            }

            Debug.Assert(nCol != -1, "");

            // 重新安排元素顺序，保证新增的元素在后面
            members.Clear();
            members.AddRange(parent_cell.item.MemberCells);
            members.AddRange(member_cells);


            // 安放下属的单独册
            PlaceMemberCells(parent_cell,
                members,
                nCol + 1,
                out strPublishTimeString);

#if DEBUG
            {
                string strError = "";
                int nRet = parent_cell.item.VerifyMemberCells(out strError);
                if (nRet == -1)
                {
                    Debug.Assert(false, strError);
                }
            }
#endif
        }
#endif

        void VerifyListCell(List<Cell> cells)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                Cell cell = cells[i];
                IssueBindingItem issue = cell.Container;
                Debug.Assert(issue != null, "");
                Debug.Assert(issue.Cells.IndexOf(cell) != -1, "");
            }
        }

        internal void VerifyAll()
        {
            for (int i = 0; i < this.Issues.Count; i++)
            {
                IssueBindingItem issue = this.Issues[i];
                /*
                if (String.IsNullOrEmpty(issue.PublishTime) == true)
                    continue;
                 * */

                for (int j = 0; j < issue.Cells.Count; j++)
                {
                    Cell cell = issue.Cells[j];
                    if (cell != null)
                    {

                        if (issue.Cells.IndexOf(cell) != j)
                        {
                            Debug.Assert(false, "issue.Cells中有重复的元素");
                        }

                        if (cell.Container != issue)
                        {
                            Debug.Assert(false, "cell.Container不正确");
                        }
                    }
                }
            }

            // 遍历合订册对象数组
            for (int i = 0; i < this.ParentItems.Count; i++)
            {
                ItemBindingItem parent_item = this.ParentItems[i];

                IssueBindingItem issue = parent_item.Container; // 假装属于这个期

                // Debug.Assert(issue != null, "issue == null");

                if (issue != null)
                {
                    // 找到行号
                    int nLineNo = this.Issues.IndexOf(issue);
                    Debug.Assert(nLineNo != -1, "");

                    int nCol = issue.IndexOfItem(parent_item);
                    if (nCol == -1)
                    {
                        Debug.Assert(nCol != -1, "nCol == -1");
                    }
                }
            }
        }

#if NOOOOOOOOOOOOOOOOO
        // 安放合订成员格子
        // parameters:
        //      members   Cell数组。注意其中有的Cell可能其item为null，为空白格子
        //      strPublishTimeString    输出出版时间范围字符串
        void PlaceMemberCells(
            Cell parent_cell,
            List<Cell> members,
            int nCol,
            out string strPublishTimeString)
        {
            strPublishTimeString = "";

            Debug.Assert(nCol >= 0, "");
            Debug.Assert(members.Count != 0, "");

            List<IssueBindingItem> done_issues = new List<IssueBindingItem>();
            int nFirstLineNo = 99999;
            int nLastLineNo = -1;
            for (int i = 0; i < members.Count; i++)
            {
                Cell cell = members[i];

                ItemBindingItem item = cell.item;
                IssueBindingItem issue = cell.Container;

                if (issue == null)
                {
                    Debug.Assert(false, "");
                }

                Debug.Assert(issue != null, "");

                Debug.Assert(issue.Cells.IndexOf(cell) != -1, "");

                done_issues.Add(issue);

                int nIssueLineNo = this.Issues.IndexOf(issue);
                if (nFirstLineNo > nIssueLineNo)
                    nFirstLineNo = nIssueLineNo;

                if (nLastLineNo < nIssueLineNo)
                    nLastLineNo = nIssueLineNo;

                // 检查cell对象是否已经存在
                int nExistIndex = issue.Cells.IndexOf(cell);

                // 如果Cell本来就在同期的拟插入位置了
                // 直接使用
                if (nExistIndex == nCol)
                {
                    cell.ParentItem = parent_cell.item;
                    if (item != null)
                    {
                        item.ParentItem = parent_cell.item;
                    }

                    // parent_cell.item.MemberCells.Remove(cell);  // 保险
                    parent_cell.item.InsertMemberCell(cell);
                    continue;
                }

                // Cell在同一期，但不在合适的位置。
                // 移出已经存在的Cell到exist_cell中，待用
                Cell exist_cell = null;
                if (nExistIndex != -1)
                {
                    if ((nExistIndex % 2) == 0)
                    {
                        // 如果在奇数位置，就很奇怪了。因为这表明这是一个合订的册
                        throw new Exception("发现将要安放的下属册对象居然在奇数Cell位置已经存在");
                    }
                    exist_cell = issue.GetCell(nExistIndex);
                    issue.Cells.RemoveAt(nExistIndex);

                    issue.Cells.RemoveAt(nExistIndex - 1);    // 左边一个，也删除
                }
                else
                {
                    // Cell不在同一期
                    Debug.Assert(false, "");
                }

                issue.GetBlankPosition(nCol / 2, parent_cell.item);

                /*
                if (exist_cell == null)
                {
                    // Debug.Assert(false, "好像不可能走到这里");

                    exist_cell = new Cell();
                    exist_cell.ParentItem = parent_cell.item;

                    // 只是占据位置
                    exist_cell.item = item;
                }*/

                if (exist_cell != null)
                {
                    // 加入到现在位置
                    // parent_cell.item.MemberCells.Remove(exist_cell);  // 保险

                    parent_cell.item.InsertMemberCell(exist_cell);


                    Debug.Assert(exist_cell == cell, "");
                }
                else
                {
                    /*
                    // Cell不在同一期，或者是独立的单册
                    Debug.Assert(cell != null, "");

                    // 从原先的issue位置移走

                    // 从原先的member位置移走
                    ItemBindingItem temp_parent = cell.ParentItem;
                    if (temp_parent != null)
                        temp_parent.MemberCells.Remove(cell);
                    cell.ParentItem = null;
                     * */

                    Debug.Assert(false, "好像不可能走到这里");

                    // 加入到现在member位置
                    // parent_cell.item.MemberCells.Remove(cell); 
                    parent_cell.item.InsertMemberCell(cell);
                }

                cell.ParentItem = parent_cell.item;
                if (item != null)
                {
                    item.ParentItem = parent_cell.item;
                    Debug.Assert(cell.item == item, "");
                }

                // 在即将被覆盖的位置进行彻底清除
                Cell old_cell = issue.GetCell(nCol);
                if (old_cell != null && old_cell != cell)
                {
                    ItemBindingItem temp_parent = old_cell.ParentItem;
                    if (temp_parent != null)
                        temp_parent.MemberCells.Remove(old_cell);
                    old_cell.ParentItem = null;

                    old_cell.Container = null;  // 

                    int temp = members.IndexOf(old_cell);
                    if (temp >= i)
                    {
                        Debug.Assert(false, "");
                    }
                }

                issue.SetCell(nCol, cell);
            }

            Debug.Assert(nFirstLineNo != 99999, "");
            Debug.Assert(nLastLineNo != -1, "");

            // 2009/12/16 
            strPublishTimeString = this.Issues[nFirstLineNo].PublishTime
            + "-"
            + this.Issues[nLastLineNo].PublishTime;


            // 补充窟窿
            for (int i = nFirstLineNo; i <= nLastLineNo; i++)
            {
                IssueBindingItem issue = this.Issues[i];
                if (done_issues.IndexOf(issue) != -1)
                    continue;

                {
                    Cell cell = issue.GetCell(nCol);
                    // 如果是空白格子，而且无主，则直接使用
                    if (cell != null
                        && cell.item == null
                        && cell.Binded == false)
                    {
                        cell.ParentItem = parent_cell.item;
                        parent_cell.item.InsertMemberCell(cell);
                        continue;
                    }
                }

                issue.GetBlankPosition(nCol / 2, parent_cell.item);
                /*
                // 如果要安放的位置已经存在内容，则向右移动它们(两格)
                if (issue.Cells.Count > nCol)
                {
                    if (issue.Cells[nCol] != null)
                    {
                        issue.Cells.Add(null);
                        issue.Cells.Add(null);
                        for (int j = issue.Cells.Count - 1; j >= nCol + 2; j--)
                        {
                            Cell temp = issue.Cells[j - 2];
                            issue.Cells[j] = temp;
                        }
                    }
                }
                else
                {
                    while (issue.Cells.Count <= nCol)
                    {
                        issue.Cells.Add(null);
                    }
                }
                 * */

                {
                    Cell cell = new Cell();
                    cell.item = null;   // 只是占据位置
                    cell.ParentItem = parent_cell.item;
                    issue.SetCell(nCol, cell);

                    // 放在合适位置
                    parent_cell.item.InsertMemberCell(cell);
                }
            }
        }
#endif

        // 追加安放合订成员格子
        // 注意本函数不负责调整合订册Cell的格子位置
        // parameters:
        //      members   Cell数组。注意其中有的Cell可能其item为null，为空白格子
        //      nCol    列号。单格index位置。TODO: 如果==-1，可以选第一个格子所在期的第一个合订可用位置。记得返回这个列号，给调用者
        void PlaceMemberCells(
            Cell parent_cell,
            List<Cell> members,
            int nCol)
        {
            Debug.Assert(nCol >= 0, "");
            Debug.Assert(members.Count != 0, "");

            for (int i = 0; i < members.Count; i++)
            {
                Cell cell = members[i];

                ItemBindingItem item = cell.item;
                IssueBindingItem issue = cell.Container;

                if (issue == null)
                {
                    Debug.Assert(false, "");
                }

                Debug.Assert(issue != null, "");

                if (issue.IssueLayoutState == IssueLayoutState.Accepting)
                {
                    // 将原来的成员中属于这个期的删除
                    parent_cell.item.RemoveMemberCell(issue);
                    // 利用旧的位置
                    parent_cell.item.InsertMemberCell(cell);
                    cell.ParentItem = parent_cell.item;
                    if (cell.item != null)
                        cell.item.ParentItem = parent_cell.item;

                    if (issue.IndexOfCell(cell) == -1)
                        issue.AddCell(cell);    // 2012/9/29 !!!TEST!!!

                    continue;
                }

                // Debug.Assert(issue.Cells.IndexOf(cell) != -1, "");
                // 有完全没有加入issue.Cells的Cell的可能

                // int nIssueLineNo = this.Issues.IndexOf(issue);

                issue.GetBlankDoubleIndex(nCol - 1,
                    parent_cell.item,
                    item);

                // 检查cell对象是否已经存在
                int nExistIndex = issue.Cells.IndexOf(cell);

                // 如果Cell本来就在同期的拟插入位置左侧
                if (nExistIndex != -1
                    && nExistIndex == nCol - 1)
                {
                    // 检测nCol位置是否为空白
                    if (issue.IsBlankSingleIndex(nCol) == true)
                    {
                        // 可以优化
                        cell.ParentItem = parent_cell.item;
                        if (item != null)
                            item.ParentItem = parent_cell.item;

                        parent_cell.item.InsertMemberCell(cell);

                        issue.SetCell(nCol - 1, null);
                        issue.SetCell(nCol, cell);
                        continue;
                    }
                }

                // 如果Cell本来就在同期的拟插入位置了
                // 直接使用
                if (nExistIndex == nCol)
                {
                    // 还要看看nCol左边的位置是否合适
                    Cell cellLeft = issue.GetCell(nCol - 1);

                    if (IssueBindingItem.IsBlankOrNullCell(cellLeft) == true
                        || (cellLeft != null && cell.item != null && cell.item == parent_cell.item))
                    {
                        // 清空左侧
                        if (cellLeft != null && cell.item != null && cell.item == parent_cell.item)
                        {
                        }
                        else
                            issue.SetCell(nCol - 1, null);  // 

                        cell.ParentItem = parent_cell.item;
                        if (item != null)
                        {
                            item.ParentItem = parent_cell.item;
                        }

                        // parent_cell.item.MemberCells.Remove(cell);  // 保险
                        parent_cell.item.InsertMemberCell(cell);
                        continue;
                    }
                }

                // Cell在同一期，但不在合适的位置。
                // 移出已经存在的Cell到exist_cell中，待用
                Cell exist_cell = null;
                if (nExistIndex != -1)
                {
                    exist_cell = issue.GetCell(nExistIndex);

                    if (nExistIndex > nCol)
                    {
                        // 探测是否为合订成员占据的位置
                        // return:
                        //      -1  是。并且是双格的左侧位置
                        //      0   不是
                        //      1   是。并且是双格的右侧位置
                        int nRet = issue.IsBoundIndex(nExistIndex);
                        if (nRet == -1 || nRet == 1)
                            issue.SetCell(nExistIndex, null);   // 2010/3/29
                        else
                            issue.RemoveSingleIndex(nExistIndex);
                    }
                    else
                        issue.SetCell(nExistIndex, null);   // 2010/3/17

                    // 如果合订本对象也在同一期，可能会因为GetBlankDoubleIndex()而而被移动。
                    // 需要重新移动到合适的位置
                    if (parent_cell.Container == issue)
                    {
                        int nParentIndex = issue.IndexOfCell(parent_cell);
                        if (nParentIndex != nCol - 1)
                        {
                            issue.RemoveSingleIndex(nParentIndex);
                            issue.SetCell(nCol - 1, parent_cell);
                        }
                    }
                }
                else
                {
                    /*
                    // Cell不在同一期
                    Debug.Assert(false, "");
                     * */

                    // 没有加入issue.Cells的情况
                }

                // issue.GetBlankDoubleIndex(nCol - 1, parent_cell.item);

                /*
                if (exist_cell == null)
                {
                    // Debug.Assert(false, "好像不可能走到这里");

                    exist_cell = new Cell();
                    exist_cell.ParentItem = parent_cell.item;

                    // 只是占据位置
                    exist_cell.item = item;
                }*/

                if (exist_cell != null)
                {
                    // 加入到现在位置
                    // parent_cell.item.MemberCells.Remove(exist_cell);  // 保险

                    parent_cell.item.InsertMemberCell(exist_cell);


                    Debug.Assert(exist_cell == cell, "");
                }
                else
                {
                    /*
                    // Cell不在同一期，或者是独立的单册
                    Debug.Assert(cell != null, "");

                    // 从原先的issue位置移走

                    // 从原先的member位置移走
                    ItemBindingItem temp_parent = cell.ParentItem;
                    if (temp_parent != null)
                        temp_parent.MemberCells.Remove(cell);
                    cell.ParentItem = null;
                     * */

                    // Debug.Assert(false, "好像不可能走到这里");
                    // 没有预先加入issue.Cells的情况可能走到这里

                    // 加入到现在member位置
                    // parent_cell.item.MemberCells.Remove(cell); 
                    parent_cell.item.InsertMemberCell(cell);
                }

                cell.ParentItem = parent_cell.item;
                if (item != null)
                {
                    item.ParentItem = parent_cell.item;
                    Debug.Assert(cell.item == item, "");
                }

                // 在即将被覆盖的位置进行彻底清除
                Cell old_cell = issue.GetCell(nCol);
                if (old_cell != null && old_cell != cell)
                {
                    ItemBindingItem temp_parent = old_cell.ParentItem;
                    if (temp_parent != null)
                        temp_parent.MemberCells.Remove(old_cell);
                    old_cell.ParentItem = null;

                    old_cell.Container = null;  // 

                    int temp = members.IndexOf(old_cell);
                    if (temp >= i)
                    {
                        Debug.Assert(false, "");
                    }
                }

                issue.SetCell(nCol, cell);
            }
        }

#if NNNNNNNNNNNNNNNNNNNNN
        // 安放合订本册和下属册的Cell位置
        void PlaceBinding(
            ItemBindingItem parent_item,
            List<ItemBindingItem> members,
            out string strPublishTimeString)
        {
            strPublishTimeString = "";
            Debug.Assert(members.Count != 0, "");

            /*
            parent_item.MemberItems.Clear();
            parent_item.MemberItems.AddRange(members);  // 注意，此时每个member的ParentItem尚未设置
            */
            parent_item.MemberCells.Clear();


            ItemBindingItem first_sub = members[0];
            IssueBindingItem first_issue = first_sub.Container;
            Debug.Assert(first_issue != null, "");

            int nCol = first_issue.GetFirstAvailableBoundColumn();

            Cell cell = new Cell();
            cell.item = parent_item;
            first_issue.SetCell(nCol, cell);
            parent_item.Container = first_issue; // 假装属于这个期

            // 安放下属的单独册
            PlaceMemberItems(parent_item,
                members,
                nCol + 1,
                out strPublishTimeString);

#if DEBUG
            {
                string strError = "";
                int nRet = parent_item.VerifyMemberCells(out strError);
                if (nRet == -1)
                {
                    Debug.Assert(false, strError);
                }
            }
#endif
        }
#endif

        // 获得格子对象所从属的期对象
        static List<IssueBindingItem> GetIssueList(List<Cell> cells)
        {
            List<IssueBindingItem> results = new List<IssueBindingItem>();
            for (int i = 0; i < cells.Count; i++)
            {
                Cell cell = cells[i];
                if (cell == null)
                    continue;
                if (cell.Container != null)
                {
                    if (results.IndexOf(cell.Container) == -1)
                        results.Add(cell.Container);
                }
            }

            return results;
        }

        // 成批切换期行的布局模式
        static int SwitchIssueLayout(List<IssueBindingItem> issues,
            IssueLayoutState state,
            out List<IssueBindingItem> changed_issues,
            out string strError)
        {
            strError = "";
            changed_issues = new List<IssueBindingItem>();

            int nRet = 0;
            for (int i = 0; i < issues.Count; i++)
            {
                IssueBindingItem issue = issues[i];
                if (String.IsNullOrEmpty(issue.PublishTime) == true)
                    continue;

                if (issue.IssueLayoutState != state)
                {
                    if (issue.IssueLayoutState == IssueLayoutState.Accepting)
                        nRet = issue.ReLayoutBinding(out strError);
                    else
                        nRet = issue.LayoutAccepting(out strError);
                    if (nRet == -1)
                        return -1;
                    issue.IssueLayoutState = state;

                    changed_issues.Add(issue);
                }
            }

            return 0;
        }

        // 合订选择的事项
        void menuItem_bindingSelectedItem_Click(object sender,
            EventArgs e)
        {
            string strError = "";
            int nRet = 0;

            bool bRemoveFromFreeIssue = false;
            bool bAddToParentItems = false;

            Cell parent_cell = null;

            // 选定的用于装订的单册
            List<Cell> member_cells = new List<Cell>();

            // 已经参与装订的册
            List<Cell> binded_cells = new List<Cell>();

            // 目标册。
            // 以前就存在的属于自由期的册，用于直接作为合订册对象
            List<Cell> target_cells = new List<Cell>();

            List<Cell> selected_cells = this.SelectedCells;
            if (selected_cells.Count == 0)
            {
                strError = "尚未选定要合订的册";
                goto ERROR1;
            }

            // 检查所选定的事项，必须满足
            // 1) 不是已经被装订的册
            // 2) 一期只能有一个册参与。也就是说，每个册的Container不能相同
            for (int i = 0; i < selected_cells.Count; i++)
            {
                Cell cell = selected_cells[i];

                if (cell is GroupCell)
                {
                    strError = "订购组首尾格子不能参与合订";
                    goto ERROR1;
                }

                if (this.FreeIssue != null
                    && cell.Container == this.FreeIssue)
                {
                    if (cell.item == null)
                    {
                        strError = "选择的对象不能包含来自于自由期的空白格子";
                        goto ERROR1;
                    }

                    if (cell.item != null
                        && cell.item.IsParent == false)
                    {
                        DialogResult dialog_result = MessageBox.Show(this,
    "您从自由期中选定的对象并不是合订本，是否要把它当作本次合订操作的目标?\r\n\r\n(Yes: 当作合订目标; No: 不当作合订目标，并被忽略; Cancel: 放弃整个合订操作)",
    "BindingControls",
    MessageBoxButtons.YesNoCancel,
    MessageBoxIcon.Question,
    MessageBoxDefaultButton.Button2);
                        if (dialog_result == DialogResult.Yes)
                            cell.item.IsParent = true;
                        else if (dialog_result == DialogResult.Cancel)
                        {
                            strError = "合订操作被放弃";
                            goto ERROR1;
                        }
                        else if (dialog_result == DialogResult.No)
                            continue;
                        else
                        {
                            Debug.Assert(false, "");
                            continue;
                        }
                    }

                    Debug.Assert(cell.item.IsParent == true, "");

                    target_cells.Add(cell);
                    continue;
                }

                if (this.IsBindingParent(cell) == true)
                {
                    Debug.Assert(cell.item != null, "");
                    if (CheckProcessingState(cell.item) == false)
                    {
                        strError = "合订册 '" + cell.item.PublishTime + "' 为固化状态，不能作为装订目标";
                        goto ERROR1;
                    }
                    if (cell.item.Locked == true)
                    {
                        strError = "合订册 '" + cell.item.PublishTime + "' 为锁定状态，不能作为装订目标";
                        goto ERROR1;
                    }
                    target_cells.Add(cell);
                    continue;
                }

                if (cell.item != null && cell.item.Calculated == true)
                {
                    strError = "预测格子不能参与合订";
                    goto ERROR1;
                }

                if (cell.IsMember == true)
                    binded_cells.Add(cell);
                else
                {
                    // 排除合订本对象，仅加入普通单册对象
                    if (this.IsBindingParent(cell) == false)
                    {
                        member_cells.Add(cell);
                    }

                    Debug.Assert(this.ParentItems.IndexOf(cell.item) == -1, "");
                }
            }

            if (binded_cells.Count > 0)
            {
                strError = "有 " + binded_cells.Count.ToString() + " 个事项是已经被合订的成员册，因此无法再进行合订";
                goto ERROR1;
            }

            if (member_cells.Count == 0)
            {
                strError = "所选定的格子中没有包含任何未装订的单册";
                goto ERROR1;
            }

            if (target_cells.Count > 1)
            {
                strError = "所选的册中有 " + target_cells.Count.ToString() + " 个自由册或者合订册，因此无法进行合订。请确保只包含一个(被用作合订目标的)自由册或合订册";
                goto ERROR1;
            }

            // return:
            //      -1  error
            //      0   合格
            //      1   不合格。strError中有提示
            nRet = CheckBindingCells(member_cells,
                out strError);
            if (nRet != 0)
            {
                strError = "无法进行合订: " + strError;
                goto ERROR1;
            }

#if NOOOOOOOOOOOOO

            List<IssueBindingItem> issues = GetIssueList(member_cells);
            if (issues.Count > 0)
            {
                List<IssueBindingItem> changed_issues = null;
                nRet = SwitchIssueLayout(issues,
                    IssueLayoutState.Binding,
                    out changed_issues,
                    out strError);
                if (nRet == -1)
                {
                    strError = "SwitchLayout() to binding mode error: " + strError;
                    goto ERROR1;
                }

#if DEBUG
                // 检查
                for (int i = 0; i < member_cells.Count; i++)
                {
                    Cell cell = member_cells[i];
                    IssueBindingItem issue = cell.Container;
                    Debug.Assert(issue != null, "");
                    Debug.Assert(issue.Cells.IndexOf(cell) != -1, "");
                }
#endif

                // 如果有先前选定的对象被丢弃，需要提示重新选择
                for (int i = 0; i < member_cells.Count; i++)
                {
                    Cell cell = member_cells[i];
                    if (cell.Container == null
                        || (cell.item != null && cell.item.Container == null))
                    {
                        strError = "因为切换布局，先前选择的某些对象发生了变化，无法继续进行合订操作。请重新选择后再试";
                        this.Invalidate();
                        goto ERROR1;
                    }
                    IssueBindingItem issue = cell.Container;
                    int nCol = issue.Cells.IndexOf(cell);
                    if (nCol == -1)
                    {
                        strError = "因为切换布局，先前选择的某些对象发生了变化，无法继续进行合订操作。请重新选择后再试";
                        this.Invalidate();
                        goto ERROR1;
                    }
                }

                // TODO: 失效那些被改变行的显示区域
            }

#endif

            this.Invalidate();
            this.Update();

            string strBatchNo = GetBindingBatchNo();   // 只能放在这里。放在后面会引起Paint()出错

            // 进行合订
            parent_cell = null;
            if (target_cells.Count == 0)
            {
                ItemBindingItem parent_item = new ItemBindingItem();
                this.ParentItems.Add(parent_item);
                bAddToParentItems = true;

                // 2018/6/5
                // 设置册记录默认值
                string strXml = "<root />";
                nRet = this.SetItemDefaultValues("quickRegister_default",
                    true,
                    ref strXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;

                // 初始化册信息
                nRet = parent_item.Initial(strXml,
                    out strError);
                if (nRet == -1)
                    goto ERROR1;
                parent_item.NewCreated = true;

                /*
                // 设置或者刷新一个操作记载
                // 可能会抛出异常
                parent_item.SetOperation(
                    "create",
                    this.Operator,
                    "");
                 * */

                parent_item.RefID = Guid.NewGuid().ToString();
                if (this.SetProcessingState == true)
                    parent_item.State = Global.AddStateProcessing(parent_item.State);   // 加工中
                parent_item.BatchNo = strBatchNo;
                parent_item.IsParent = true;

                parent_cell = new Cell();
                parent_cell.item = parent_item;
            }
            else
            {
                parent_cell = target_cells[0];

                // 从自由期中移走
                if (parent_cell.Container == this.FreeIssue)
                {
                    this.FreeIssue.RemoveCell(parent_cell.item);
                    parent_cell.Container = null;
                    // TODO: 如果失败，是否要还原?
                    bRemoveFromFreeIssue = true;
                }

                // 加入BindItems
                if (this.ParentItems.IndexOf(parent_cell.item) == -1)
                {
                    this.ParentItems.Add(parent_cell.item);
                    parent_cell.Container = null;
                    // TODO: 如果失败，是否要还原?
                    bAddToParentItems = true;
                }

                if (this.SetProcessingState == true)
                    parent_cell.item.State = Global.AddStateProcessing(parent_cell.item.State);   // 加工中

                parent_cell.item.BatchNo = strBatchNo;
            }

            Debug.Assert(parent_cell.item.IsParent == true, "");

            // 将若干单册加入一个合订册
            nRet = AddToBinding(member_cells,
                parent_cell,
                out strError);
            if (nRet == -1)
            {
                goto ERROR1;
            }

            // 检查馆藏地点字符串是否在当前用户管辖范围内
            // return:
            //      0   在当前用户管辖范围内，不需要修改
            //      1   不在当前用户管辖范围内，需要修改。strPreferredLocationString中已经设置了一个值，但只到了分馆代码一级，库房名称为空
            nRet = CheckLocationString(parent_cell.item.LocationString,
            out string strPreferredLocationString,
            out strError);
            if (nRet == -1)
                goto ERROR1;
            if (nRet == 1)
            {
                parent_cell.item.LocationString = strPreferredLocationString;

                // TODO: 可以问Yes No，如果Yes才主动打开Edit区域
                MessageBox.Show(this, "合订成功。请注意及时设置合订册 馆藏地点 值");

                // 选定新创建的册
                this.ClearAllSelection();
                menuItem_modifyCell_Click(null, null);
                parent_cell.Select(SelectAction.On);
                this.FocusObject = parent_cell;
                EnsureVisible(parent_cell);
            }

            /*
            string strPublishTimeString = "";
            PlaceBinding(
                parent_item,
                selected_items,
                out strPublishTimeString);
            parent_item.PublishTime = strPublishTimeString;

            // 创建<binding>元素内片断
            parent_item.RefreshBindingXml();
            parent_item.State = Global.AddStateProcessing(parent_item.State);   // 加工中
            parent_item.Changed = true;
            */


            /*
            // 选定所有成员格子
            parent_item.SelectAllMemberCells();
            this.Invalidate(); 
             * */
#if DEBUG
            {
                string strError1 = "";
                int nRet1 = parent_cell.item.VerifyMemberCells(out strError1);
                if (nRet1 == -1)
                {
                    Debug.Assert(false, strError1);
                }
            }

            VerifyAll();
#endif
            return;
        ERROR1:
            // 复原
            if (bAddToParentItems == true)
            {
                this.ParentItems.Remove(parent_cell.item);
            }

            if (bRemoveFromFreeIssue == true)
            {
                this.AddToFreeIssue(parent_cell);
            }
#if DEBUG
            if (parent_cell != null && parent_cell.item != null)
            {
                string strError1 = "";
                int nRet1 = parent_cell.item.VerifyMemberCells(out strError1);
                if (nRet1 == -1)
                {
                    Debug.Assert(false, strError1);
                }
            }

            VerifyAll();
#endif

            MessageBox.Show(this, strError);
        }

        // 检查官仓地点字符串是否在当前用户管辖范围内
        // return:
        //      0   在当前用户管辖范围内，不需要修改
        //      1   不在当前用户管辖范围内，需要修改。strPreferredLocationString中已经设置了一个值，但只到了分馆代码一级，库房名称为空
        int CheckLocationString(string strLocationString,
            out string strPreferredLocationString,
            out string strError)
        {
            strError = "";
            strPreferredLocationString = strLocationString;

            if (Global.IsGlobalUser(this.LibraryCodeList) == true)
                return 0;

            string strLibraryCode = Global.GetLibraryCode(strLocationString);

            if (StringUtil.IsInList(strLibraryCode, this.LibraryCodeList) == false)
            {
                strPreferredLocationString = StringUtil.FromListString(this.LibraryCodeList)[0] + "/";
            }

            if (strPreferredLocationString == strLocationString)
                return 0;

            return 1;
        }

        // 获得装订批次号
        string GetBindingBatchNo()
        {
            if (this.AppInfo == null)
                return "";

#if NO
            string strDefault = this.AppInfo.GetString(
                "binding_form",
                "binding_batchno",
                "");
#endif
            // 2017/12/20 改为从默认值模板中获得
            string strDefault = EntityFormOptionDlg.GetFieldValue("quickRegister_default",
    "batchNo");

            if (this.m_bBindingBatchNoInputed == true)
                return strDefault;

            string strResult = InputDlg.GetInput(this, "请指定装订批次号",
                "装订批次号:",
                strDefault,
                this.Font);
            if (strResult == null)
                return "";

            if (strResult != strDefault)
            {
#if NO
                this.AppInfo.SetString(
                "binding_form",
                "binding_batchno",
                strResult);
#endif
                EntityFormOptionDlg.SetFieldValue("quickRegister_default",
    "batchNo",
    strResult);
            }

            this.m_bBindingBatchNoInputed = true;
            return strResult;
        }

#if NO
        /// <summary>
        /// 验收批次号
        /// </summary>
        public string AcceptBatchNo
        {
            get
            {
                if (this.AppInfo == null)
                    return "";
                return this.AppInfo.GetString(
                    "binding_form",
                    "accept_batchno",
                    "");
            }
            set
            {
                if (this.AppInfo != null)
                {
                    this.AppInfo.SetString(
                        "binding_form",
                        "accept_batchno",
                        value);
                }
            }
        }
#endif

        /// <summary>
        /// 验收批次号是否已经在界面被输入了
        /// </summary>
        public bool AcceptBatchNoInputed
        {
            get
            {
                return this.m_bAcceptingBatchNoInputed;
            }
            set
            {
                this.m_bAcceptingBatchNoInputed = value;
            }
        }

        // 获得验收批次号
        internal string GetAcceptingBatchNo()
        {
            if (this.AppInfo == null)
                return "";

#if NO
            string strDefault = this.AcceptBatchNo;
#endif
            // 2017/12/20 改为从默认值模板中获得
            string strDefault = EntityFormOptionDlg.GetFieldValue("quickRegister_default",
    "batchNo");

            if (this.AcceptBatchNoInputed == true)
                return strDefault;

            string strResult = InputDlg.GetInput(this, "请指定验收批次号",
                "验收批次号:",
                strDefault,
                this.Font);
            if (strResult == null)
                return "";

            if (strResult != strDefault)
            {
#if NO
                // 记忆
                this.AcceptBatchNo = strResult;
#endif
                EntityFormOptionDlg.SetFieldValue("quickRegister_default",
"batchNo",
strResult);
            }

            this.AcceptBatchNoInputed = true;
            return strResult;
        }

        public static void PaintButton(Graphics graphics,
            Color color,
            RectangleF rect)
        {
            float upper_height = rect.Height / 2 + 1;
            float lower_height = rect.Height / 2;
            float x = rect.X;
            float y = rect.Y;

            using (LinearGradientBrush linGrBrush = new LinearGradientBrush(
new PointF(0, y),
new PointF(0, y + upper_height),
Color.FromArgb(70, color),
Color.FromArgb(120, color)
))
            {
                linGrBrush.GammaCorrection = true;

                RectangleF rectBack = new RectangleF(
    x,
    y,
    rect.Width,
    upper_height);
                graphics.FillRectangle(linGrBrush, rectBack);
            }
            //

            using (LinearGradientBrush linGrBrush = new LinearGradientBrush(
new PointF(0, y + upper_height),
new PointF(0, y + upper_height + lower_height),
Color.FromArgb(200, color),
Color.FromArgb(100, color)
))
            {
                RectangleF rectBack = new RectangleF(
    x,
    y + upper_height,
    rect.Width,
    lower_height - 1);
                graphics.FillRectangle(linGrBrush, rectBack);
            }
        }

        // paramters:
        //      pen 绘制边框。可以为null，那样就整体一个填充色，没有边框
        //      brush   绘制填充色。可以为null，那样就只有边框
        public static void RoundRectangle(Graphics graphics,
            Pen pen,
            Brush brush,
            RectangleF rect,
            float radius)
        {
            RoundRectangle(graphics,
                pen,
                brush,
                rect.X,
                rect.Y,
                rect.Width,
                rect.Height,
                radius);
        }

        // paramters:
        //      pen 绘制边框。可以为null，那样就整体一个填充色，没有边框
        //      brush   绘制填充色。可以为null，那样就只有边框
        public static void RoundRectangle(Graphics graphics,
            Pen pen,
            Brush brush,
            float x,
            float y,
            float width,
            float height,
            float radius)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddLine(x + radius, y, x + width - (radius * 2), y);
                path.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90);
                path.AddLine(x + width, y + radius, x + width, y + height - (radius * 2));
                path.AddArc(x + width - (radius * 2), y + height - (radius * 2), radius * 2, radius * 2, 0, 90); // Corner
                path.AddLine(x + width - (radius * 2), y + height, x + radius, y + height);
                path.AddArc(x, y + height - (radius * 2), radius * 2, radius * 2, 90, 90);
                path.AddLine(x, y + height - (radius * 2), x, y + radius);
                path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
                path.CloseFigure();
                if (brush != null)
                    graphics.FillPath(brush, path);
                if (pen != null)
                    graphics.DrawPath(pen, path);
            }
        }

        // 包装版本
        public static void PartRoundRectangle(
    Graphics graphics,
    Pen pen,
    Brush brush,
    RectangleF rect,
    float radius,
    string strMask) // 左上 右上 右下 左下
        {
            PartRoundRectangle(graphics,
                pen,
                brush,
                rect.X,
                rect.Y,
                rect.Width,
                rect.Height,
                radius,
                strMask);
        }

        // 部分圆角的矩形
        // paramters:
        //      pen 绘制边框。可以为null，那样就整体一个填充色，没有边框
        //      brush   绘制填充色。可以为null，那样就只有边框
        public static void PartRoundRectangle(
            Graphics graphics,
            Pen pen,
            Brush brush,
            float x,
            float y,
            float width,
            float height,
            float radius,
            string strMask) // 左上 右上 右下 左下
        {
            float x0 = 0;
            float y0 = 0;
            float width0 = 0;
            float height0 = 0;

            using (GraphicsPath path = new GraphicsPath())
            {

                // 左上 --> 右上
                if (strMask[0] == 'r')
                {
                    x0 = x + radius;
                    y0 = y;
                    if (strMask[1] == 'r')
                        width0 = width - radius * 2;
                    else
                        width0 = width - radius * 1;
                }
                else
                {
                    // != 'r'
                    x0 = x;
                    y0 = y;
                    if (strMask[1] == 'r')
                        width0 = width - radius * 1;
                    else
                        width0 = width;
                }

                path.AddLine(x0, y0,
                    x0 + width0, y0);

                // 右上
                if (strMask[1] == 'r')
                    path.AddArc(x + width - (radius * 2), y,
                        radius * 2, radius * 2,
                        270, 90);

                // 右上 --> 右下
                if (strMask[1] == 'r')
                {
                    x0 = x + width;
                    y0 = y + radius;
                    if (strMask[2] == 'r')
                        height0 = height - radius * 2;
                    else
                        height0 = height - radius * 1;
                }
                else
                {
                    // != 'r'
                    x0 = x + width;
                    y0 = y;
                    if (strMask[2] == 'r')
                        height0 = height - radius * 1;
                    else
                        height0 = height;
                }


                path.AddLine(x0, y0,
                    x0, y0 + height0);

                // 右下
                if (strMask[2] == 'r')
                    path.AddArc(x + width - (radius * 2), y + height - (radius * 2),
                        radius * 2, radius * 2,
                        0, 90); // Corner

                // 右下 --> 左下
                if (strMask[2] == 'r')
                {
                    x0 = x + width - radius;
                    y0 = y + height;
                    if (strMask[3] == 'r')
                        width0 = width - radius * 2;
                    else
                        width0 = width - radius * 1;
                }
                else
                {
                    // != 'r'
                    x0 = x + width;
                    y0 = y + height;
                    if (strMask[3] == 'r')
                        width0 = width - radius * 1;
                    else
                        width0 = width;
                }

                path.AddLine(x0, y0,
                    x0 - width0, y0);

                // 左下
                if (strMask[3] == 'r')
                    path.AddArc(x, y + height - (radius * 2),
                        radius * 2, radius * 2,
                        90, 90);

                // 左下 --> 左上
                if (strMask[3] == 'r')
                {
                    x0 = x;
                    y0 = y + height - radius;
                    if (strMask[0] == 'r')
                        height0 = height - radius * 2;
                    else
                        height0 = height - radius * 1;
                }
                else
                {
                    // != 'r'
                    x0 = x;
                    y0 = y + height;
                    if (strMask[0] == 'r')
                        height0 = height - radius * 1;
                    else
                        height0 = height;
                }

                path.AddLine(x0, y0,
                    x0, y0 - height0);

                // 左上
                if (strMask[0] == 'r')
                    path.AddArc(x, y,
                        radius * 2, radius * 2,
                        180, 90);

                path.CloseFigure();

                if (brush != null)
                    graphics.FillPath(brush, path);
                if (pen != null)
                    graphics.DrawPath(pen, path);
            }
        }

        // paramters:
        //      pen 绘制边框。可以为null，那样就整体一个填充色，没有边框
        //      brush   绘制填充色。可以为null，那样就只有边框
        public static void QueRoundRectangle(Graphics graphics,
            Pen pen,
            Brush brush,
            RectangleF rect,
            float radius,
            float que_radius)
        {
            QueRoundRectangle(graphics,
                pen,
                brush,
                rect.X,
                rect.Y,
                rect.Width,
                rect.Height,
                radius,
                que_radius);
        }

        // 缺角的矩形
        // paramters:
        //      pen 绘制边框。可以为null，那样就整体一个填充色，没有边框
        //      brush   绘制填充色。可以为null，那样就只有边框
        public static void QueRoundRectangle(Graphics graphics,
            Pen pen,
            Brush brush,
            float x,
            float y,
            float width,
            float height,
            float radius,
            float que_radius)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                // 左上 --> 右上
                path.AddLine(x + radius, y,
                    x + width - (radius + que_radius), y);
                // 右上
                path.AddArc(x + width - (que_radius), y - que_radius,
                    que_radius * 2, que_radius * 2, 180, -90);
                /*
                // 右上 --> 右下
                path.AddLine(x + width, y + que_radius, 
                    x + width, y + height - (radius + que_radius));
                 * */

                // 右下
                path.AddArc(x + width - (radius * 2), y + height - (radius * 2),
                    radius * 2, radius * 2, 0, 90); // Corner
                // 右下 --> 左下
                path.AddLine(x + width - (radius * 2), y + height,
                    x + radius, y + height);
                // 左下
                path.AddArc(x, y + height - (radius * 2),
                    radius * 2, radius * 2, 90, 90);
                // 左下 --> 左上
                path.AddLine(x, y + height - (radius * 2), x, y + radius);
                // 左上
                path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
                path.CloseFigure();
                if (brush != null)
                    graphics.FillPath(brush, path);
                if (pen != null)
                    graphics.DrawPath(pen, path);
            }
        }

        // paramters:
        //      pen 绘制边框。可以为null，那样就整体一个填充色，没有边框
        //      brush   绘制填充色。可以为null，那样就只有边框
        public static void Circle(Graphics graphics,
            Pen pen,
            Brush brush,
            RectangleF rect)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                if (pen != null)
                {
                    rect.Inflate(-pen.Width / 2, -pen.Width / 2);
                }

                float x = rect.X;
                float y = rect.Y;
                float width = rect.Width;
                float height = rect.Height;

                path.AddArc(x, y,
                    width, height, 0, 360);
                path.CloseFigure();
                if (brush != null)
                    graphics.FillPath(brush, path);
                if (pen != null)
                    graphics.DrawPath(pen, path);
            }
        }

        // paramters:
        //      pen 绘制边框。可以为null，那样就整体一个填充色，没有边框
        //      brush   绘制填充色。可以为null，那样就只有边框
        public static void Bracket(Graphics graphics,
            Pen pen,
            bool bLeft,
            RectangleF rect,
            float radius)
        {
            using (GraphicsPath path = new GraphicsPath())
            {

                rect.Inflate(0, -pen.Width / 2);

                float x = rect.X;
                float y = rect.Y;
                float width = rect.Width;
                float height = rect.Height;

                if (bLeft == true)
                {
                    path.AddArc(x + width - radius, y,
                        radius * 2, radius * 2, 270, -90);
                    /*
                    path.AddLine(x + width - (radius), y+(radius),
                        x + width - (radius), y + (height/2)-(radius));
                     * */
                    path.AddArc(x + width - (radius * 2) - radius, y + (height / 2) - (radius) - radius,
                        radius * 2, radius * 2, 0, 90);
                    path.AddArc(x + width - (radius * 2) - radius, y + (height / 2),
            radius * 2, radius * 2, 270, 90);
                    /*
                    path.AddLine(x + width - (radius), y + (height/2)+(radius),
            x + width - (radius), y + height - (radius));
                     * */
                    path.AddArc(x + width - (radius), y + height - (radius) - radius,
            radius * 2, radius * 2, 180, -90);
                }
                else
                {
                    path.AddArc(x - radius, y,
                        radius * 2, radius * 2, 270, 90);
                    path.AddArc(x + radius, y + height / 2 - radius * 2,
                        radius * 2, radius * 2, 180, -90);
                    path.AddArc(x + radius, y + (height / 2),
            radius * 2, radius * 2, 270, -90);
                    path.AddArc(x - radius, y + height - radius * 2,
            radius * 2, radius * 2, 0, 90);
                }

                graphics.DrawPath(pen, path);
            }
        }

        public static void RoundRectangle(Graphics graphics,
            Pen pen,
            RectangleF rect,
            float radius)
        {
            RoundRectangle(graphics,
                pen,
                rect.X,
                rect.Y,
                rect.Width,
                rect.Height,
                radius);
        }

        public static void RoundRectangle(Graphics graphics,
            Pen pen,
            float x,
            float y,
            float width,
            float height,
            float radius)
        {
            using (GraphicsPath path = new GraphicsPath())
            {
                path.AddLine(x + radius, y, x + width - (radius * 2), y);
                path.AddArc(x + width - (radius * 2), y, radius * 2, radius * 2, 270, 90);
                path.AddLine(x + width, y + radius, x + width, y + height - (radius * 2));
                path.AddArc(x + width - (radius * 2), y + height - (radius * 2), radius * 2, radius * 2, 0, 90); // Corner
                path.AddLine(x + width - (radius * 2), y + height, x + radius, y + height);
                path.AddArc(x, y + height - (radius * 2), radius * 2, radius * 2, 90, 90);
                path.AddLine(x, y + height - (radius * 2), x, y + radius);
                path.AddArc(x, y, radius * 2, radius * 2, 180, 90);
                path.CloseFigure();
                graphics.DrawPath(pen, path);
            }
        }

        void DoEndRectSelecting()
        {
            bool bControl = (Control.ModifierKeys == Keys.Control);

            // 清除选择框
            DrawSelectRect(true);

            // 整体文档坐标
            RectangleF rect = MakeRect(m_DragStartPointOnDoc,
                m_DragCurrentPointOnDoc);

            // DataRoot坐标
            rect.Offset(-this.m_nLeftBlank, -this.m_nTopBlank);

            // 选择位于矩形内的对象
            List<Type> types = new List<Type>();
            types.Add(typeof(Cell));

            List<CellBase> update_objects = new List<CellBase>();
            this.Select(rect,
                bControl == true ? SelectAction.Toggle : SelectAction.On,
                types,
                ref update_objects,
                100);
            if (update_objects.Count < 100)
                this.UpdateObjects(update_objects);
            else
                this.Invalidate();

            m_bRectSelecting = false;   // 结束

            this.DragStartObject = this.FocusObject;
            this.DragLastEndObject = this.FocusObject;
        }

        // 交换两个值
        static void Exchange<T>(ref T v1, ref T v2)
        {
            T temp = v1;
            v1 = v2;
            v2 = temp;
        }

        // 构造一个矩形，通过两个端点
        // 本函数可以自动比较端点大小，创建出正规的矩形
        static RectangleF MakeRect(PointF p1,
            PointF p2)
        {
            float x1 = p1.X;
            float y1 = p1.Y;

            float x2 = p2.X;
            float y2 = p2.Y;

            if (x1 > x2)
                Exchange<float>(ref x1, ref x2);

            if (y1 > y2)
                Exchange<float>(ref y1, ref y2);

            return new RectangleF(x1,
                y1,
                x2 - x1,
                y2 - y1);
        }


        // 画、或者清除选择矩形
        // 因为是异或运算，第一次是画，第二次在同样位置就是清除
        void DrawSelectRect(bool bUpdateBefore)
        {
            if (bUpdateBefore == true)
                this.Update();

            RectangleF rect = MakeRect(m_DragStartPointOnDoc,
            m_DragCurrentPointOnDoc);

            rect.Offset(m_lWindowOrgX, m_lWindowOrgY);
            ControlPaint.DrawReversibleFrame( // Graphics.FromHwnd(this.Handle),
                this.RectangleToScreen(Rectangle.Round(rect)),
                this.SelectedBackColor, // Color.Yellow,
                FrameStyle.Dashed);
        }

        // 点b是否处在点a周围一个不大的矩形范围内
        // 矩形采用系统DoubleClickSize
        static bool IsNearestPoint(Point a, Point b)
        {
            Rectangle rect = new Rectangle(a.X, a.Y, 0, 0);
            rect.Inflate(
                SystemInformation.DoubleClickSize.Width / 2,
                SystemInformation.DoubleClickSize.Height / 2);

            return rect.Contains(b.X, b.Y);
        }



        // 选择位于矩形内的对象
        public void Select(RectangleF rect,
            SelectAction action,
            List<Type> types,
            ref List<CellBase> update_objects,
            int nMaxCount)
        {
            RectangleF rectThis = new RectangleF(0, 0, this.m_lContentWidth, this.m_lContentHeight);

            if (rectThis.IntersectsWith(rect) == true)
            {
                long y = 0;
                for (int i = 0; i < this.Issues.Count; i++)
                {
                    IssueBindingItem issue = this.Issues[i];

                    // 优化
                    if (y > rect.Bottom)
                        break;

                    // 变换为issue内坐标
                    RectangleF rectIssue = rect;
                    rectIssue.Offset(0, -y);

                    issue.Select(rectIssue,
                        action,
                        types,
                        ref update_objects,
                        nMaxCount);

                    y += this.m_nCellHeight;
                }
            }

        }

        void SetObjectFocus(CellBase obj)
        {
            if (m_bRectSelecting == true)
                return;

            if (obj == null)    // 表示关闭最后focus对象的focus状态
            {
                goto OFF_OLD;
            }

            if (obj.m_bFocus == true)
                return;


            obj.m_bFocus = true;
            this.UpdateObject(obj);

            if (obj == this.m_lastFocusObj)
                return;

            // off先前的focus对象
            OFF_OLD:
            if (this.m_lastFocusObj != null
                && this.m_lastFocusObj.m_bFocus == true)
            {
                this.m_lastFocusObj.m_bFocus = false;
                this.UpdateObject(this.m_lastFocusObj);
            }

            this.m_lastFocusObj = obj;  // 记忆
        }


        // 刷新一群对象的区域
        void UpdateObjects(List<CellBase> objects)
        {
            for (int i = 0; i < objects.Count; i++)
            {
                CellBase obj = objects[i];
                if (obj == null)
                    continue;

                //  rectUpdate = new RectangleF(0, 0, obj.Width, obj.Height);

                RectangleF rectUpdate = GetViewRect(obj);
                this.Invalidate(Rectangle.Round(rectUpdate));
            }
        }

        internal void UpdateObject(CellBase obj)
        {
            Debug.Assert(obj != null, "");

            if (obj is Cell
    || obj is NullCell
                || obj is IssueBindingItem)
            {
            }
            else
            {
                throw new Exception("obj必须为类型Cell/NullCell/IssueBindingItem之一");
            }

            if (obj is NullCell)
            {
                RectangleF rectUpdate = GetViewRect(obj);
                this.Invalidate(Rectangle.Round(rectUpdate));
                return;
            }

            if (obj is Cell)
            {
                if (((Cell)obj).Container == null)
                    return;

                RectangleF rectUpdate = GetViewRect(obj);
                this.Invalidate(Rectangle.Round(rectUpdate));
                return;
            }

            if (obj is IssueBindingItem)
            {
                if (((IssueBindingItem)obj).Container == null)
                    return;

                RectangleF rectUpdate = GetViewRect(obj);
                this.Invalidate(Rectangle.Round(rectUpdate));
                return;
            }
        }

        // 刷新一个对象的drag handle区域
        void UpdateObjectHover(Cell obj)
        {
            UpdateObject(obj);
            /*
            RectangleF rectUpdate = this.m_rectCheckBox; 

            RectangleF rectObj = GetViewRect(obj);
            rectUpdate = new RectangleF(rectObj.X + rectUpdate.X,
                rectObj.Y + rectUpdate.Y,
                rectUpdate.Width,
                rectUpdate.Height);

            this.Invalidate(Rectangle.Round(rectUpdate));
             * */
        }

        // 确保一个区域在窗口客户区可见
        // parameters:
        //      rectCell    要关注的区域
        //      rectCaret   要关注的区域中，用于插入符（热点）的矩形。一般可以小于rectCell
        // return:
        //      是否发生卷滚了
        public bool EnsureVisible(RectangleF rectCell,
            RectangleF rectCaret)
        {
            /*
            if (rectCaret == null)
                rectCaret = rectCell;
             * */

            long lDelta = (long)rectCell.Y;

            bool bScrolled = false;

            if (lDelta + rectCaret.Height >= this.ClientSize.Height)
            {
                if (rectCaret.Height >= this.ClientSize.Height)
                    DocumentOrgY = DocumentOrgY - (lDelta + (long)rectCaret.Height) + ClientSize.Height + /*调整系数*/ ((long)rectCaret.Height / 2) - (this.ClientSize.Height / 2);
                else
                    DocumentOrgY = DocumentOrgY - (lDelta + (long)rectCaret.Height) + ClientSize.Height;
                bScrolled = true;
            }
            else if (lDelta < 0)
            {
                if (rectCaret.Height >= this.ClientSize.Height)
                    DocumentOrgY = DocumentOrgY - (lDelta) - /*调整系数*/ (((long)rectCaret.Height / 2) - (this.ClientSize.Height / 2));
                else
                    DocumentOrgY = DocumentOrgY - (lDelta);
                bScrolled = true;
            }
            else
            {
                // y不需要卷滚
            }

            ////
            // 水平方向
            lDelta = 0;

            lDelta = (long)rectCell.X;


            if (lDelta + rectCaret.Width >= this.ClientSize.Width)
            {
                if (rectCaret.Width >= this.ClientSize.Width)
                    DocumentOrgX = DocumentOrgX - (lDelta + (long)rectCaret.Width) + ClientSize.Width + /*调整系数*/ ((long)rectCaret.Width / 2) - (this.ClientSize.Width / 2);
                else
                    DocumentOrgX = DocumentOrgX - (lDelta + (long)rectCaret.Width) + ClientSize.Width;
                bScrolled = true;
            }
            else if (lDelta < 0)
            {
                if (rectCaret.Width >= this.ClientSize.Width)
                    DocumentOrgX = DocumentOrgX - (lDelta) - /*调整系数*/ (((long)rectCaret.Width / 2) - (this.ClientSize.Width / 2));
                else
                    DocumentOrgX = DocumentOrgX - (lDelta);
                bScrolled = true;
            }
            else
            {
                // x不需要卷滚
            }


            return bScrolled;
        }

        // 确保一个单元在窗口客户区可见
        // return:
        //      是否发生卷滚了
        public bool EnsureVisible(CellBase obj)
        {
            if (obj == null)
            {
                Debug.Assert(false, "");
                return false;
            }
            RectangleF rectUpdate = GetViewRect(obj);

            RectangleF rectCell = rectUpdate;

            RectangleF rectCaret = rectUpdate;

            if (obj is IssueBindingItem)
            {
                // 修正
                IssueBindingItem issue = (IssueBindingItem)obj;
                rectCaret.Width = this.m_nCoverImageWidth + this.m_nLeftTextWidth;
            }

            return EnsureVisible(rectCell, rectCaret);
        }

        // 确保一个对象单元在窗口客户区可见
        // 对DayArea有特殊处理
        // return:
        //      是否发生卷滚了
        public bool EnsureVisibleWhenScrolling(CellBase obj)
        {
            if (obj == null)
            {
                Debug.Assert(false, "");
                return false;
            }

            RectangleF rectUpdate = GetViewRect(obj);

            /*
            if (obj is Cell)
            {
                DayArea day = (DayArea)obj;
                // 如果是每月第一个星期的日子
                if (day.Container.Week == 1)
                {
                    // 调整矩形，以包括星期名标题
                    rectUpdate.Y -= this.DataRoot.m_nDayOfWeekTitleHeight;
                    rectUpdate.Height += this.DataRoot.m_nDayOfWeekTitleHeight;
                }
            }*/

            // TODO:
            // 如果是月、年等较大尺寸的物体，只要这个物体当前部分可见，就不必卷滚了
            // 也可以通过把caret设置为已经可见的部分，来实现类似效果

            RectangleF rectCell = rectUpdate;

            RectangleF rectCaret = rectUpdate;

            if (obj is IssueBindingItem)
            {
                // 修正 2010/3/26
                IssueBindingItem issue = (IssueBindingItem)obj;
                rectCaret.Width = this.m_nCoverImageWidth + this.m_nLeftTextWidth;
            }

            return EnsureVisible(rectCell, rectCaret);
        }

        // return:
        //      是否发生卷滚了
        public bool EnsureVisibleWhenScrolling(HitTestResult result)
        {
            if (result == null)
                return false;

            if (result.Object is IssueBindingItem
                && result.AreaPortion == AreaPortion.LeftText)
            {
                IssueBindingItem issue = (IssueBindingItem)result.Object;

                RectangleF rectUpdate = GetViewRect(issue);
                rectUpdate.Width = this.m_nCoverImageWidth + this.m_nLeftTextWidth;   // 左边标题部分

                RectangleF rectCell = rectUpdate;
                RectangleF rectCaret = rectUpdate;

                return EnsureVisible(rectCell, rectCaret);
            }
            else if (result.Object is Cell
                || result.Object is IssueBindingItem)   // 2016/8/31
            {
                RectangleF rectUpdate = GetViewRect(result.Object);

                RectangleF rectCell = rectUpdate;
                RectangleF rectCaret = rectUpdate;

                return EnsureVisible(rectCell, rectCaret);
            }
            else if (result.Object is NullCell)
            {
                RectangleF rectUpdate = GetViewRect(result.Object);

                RectangleF rectCell = rectUpdate;
                RectangleF rectCaret = rectUpdate;

                return EnsureVisible(rectCell, rectCaret);
            }
            else
            {
                throw new Exception("尚不支持IssueBindingItem/Cell/NullCell以外的其他类型 " + result.Object.GetType().ToString());
            }

        }


        // 清除当前对象本身以及全部下级的选择标志, 并返回需要刷新的对象
        public void ClearAllSubSelected(ref List<CellBase> objects,
            int nMaxCount)
        {
            /*
            // 修改过的才加入数组
            if (this.m_bSelected == true && objects.Count < nMaxCount)
                objects.Add(this);

            this.m_bSelected = false;
             * */

            for (int i = 0; i < this.Issues.Count; i++)
            {
                this.Issues[i].ClearAllSubSelected(ref objects,
                    nMaxCount);
            }
        }

        // 探测两个同级对象的先后关系
        // return:
        //      -1  start在end之前
        //      0   start和end是同一个对象
        //      1   start在end之后
        int GetDirection(Cell start, Cell end)
        {
            Debug.Assert(start != null, "");
            Debug.Assert(end != null);

            if (start == end)
                return 0;

            IssueBindingItem start_issue = start.Container;
            IssueBindingItem end_issue = end.Container;

            int start_issue_index = this.Issues.IndexOf(start_issue);
            int end_issue_index = this.Issues.IndexOf(end_issue);

            Debug.Assert(start_issue_index != -1, "");
            Debug.Assert(end_issue_index != -1, "");

            if (start_issue_index > end_issue_index)
            {
                // start在end后面
                return 1;
            }

            return -1;  // start在end前面
        }

        // a和b中交叉的部分放入union，并从a和b中去掉
        void Compare(ref List<CellBase> a,
            ref List<CellBase> b,
            out List<CellBase> union)
        {
            union = new List<CellBase>();
            for (int i = 0; i < a.Count; i++)
            {
                CellBase x = a[i];

                bool bFound = false;
                for (int j = 0; j < b.Count; j++)
                {
                    CellBase y = b[j];
                    if (IsSamePos(x, y) == true)
                    {
                        union.Add(x);
                        b.RemoveAt(j);
                        bFound = true;
                        break;
                    }
                }

                if (bFound == true)
                {
                    a.RemoveAt(i);
                    i--;
                }
            }
        }

        bool IsSamePos(CellBase start, CellBase end)
        {
            int start_x = -1;
            int start_y = -1;
            GetCellXY(start,
    out start_x,
    out start_y);
            int end_x = -1;
            int end_y = -1;
            GetCellXY(end,
    out end_x,
    out end_y);
            if (start_x == end_x
                && start_y == end_y)
                return true;
            return false;
        }

        // 从起点到终点，构造包含所有兄弟对象的数组
        List<CellBase> GetRangeObjects(
            CellBase start,
            CellBase end)
        {
            Debug.Assert(start != null, "");
            Debug.Assert(end != null);

            List<CellBase> results = new List<CellBase>();

            if (start == end)
            {
                results.Add(end);
                return results;
            }

            if (start is NullCell
                && end is NullCell)
            {
                NullCell start_n = (NullCell)start;
                NullCell end_n = (NullCell)end;

                if (start_n.X == end_n.X
                    && start_n.Y == end_n.Y)
                {
                    results.Add(end);
                    return results;
                }
            }

            int start_x = -1;
            int start_y = -1;
            GetCellXY(start,
    out start_x,
    out start_y);
            int end_x = -1;
            int end_y = -1;
            GetCellXY(end,
    out end_x,
    out end_y);

            int x1 = 0;
            int y1 = 0;
            int x2 = 0;
            int y2 = 0;

            if (start_x < end_x)
            {
                x1 = start_x;
                x2 = end_x;
            }
            else
            {
                x1 = end_x;
                x2 = start_x;
            }

            if (start_y < end_y)
            {
                y1 = start_y;
                y2 = end_y;
            }
            else
            {
                y1 = end_y;
                y2 = start_y;
            }

            for (int i = y1; i <= y2; i++)
            {
                IssueBindingItem issue = this.Issues[i];
                if (x1 == -1)
                {
                    Debug.Assert(x2 == -1, "");
                    results.Add(issue);
                    continue;
                }

                for (int j = x1; j <= x2; j++)
                {
                    Cell cell = issue.GetCell(j);
                    if (cell == null)
                        results.Add(new NullCell(j, i));
                    else
                        results.Add(cell);
                }
            }

            return results;
        }

#if NOOOOOOOOOOOOOOO
        // 从起点到终点，构造包含所有兄弟对象的数组
        List<Cell> GetRangeObjects(
            bool bIncludeStart,
            bool bIncludeEnd,
            Cell start,
            Cell end)
        {
            Debug.Assert(start != null, "");
            Debug.Assert(end != null);

            List<Cell> result = new List<Cell>();

            if (start == end)
            {
                if (bIncludeStart == true)
                {
                    result.Add(start);
                    return result;
                }

                if (bIncludeEnd == true)
                    result.Add(end);
                return result;
            }

            // 先观察哪个在前面
            int nDirection = GetDirection(start, end);

            if (nDirection > 0)
            {
                // 交换start和end
                Cell temp = start;
                start = end;
                end = temp;

                // 交换bool
                bool bTemp = bIncludeStart;
                bIncludeStart = bIncludeEnd;
                bIncludeEnd = bTemp;
            }

            if (bIncludeStart == false)
            {
                start = start.GetNextSibling();
                if (start == null)
                {
                    return result;  // 返回空集合
                }
                Debug.Assert(start != null, "");
            }

            // 从start到end，建立数组
            for (; ; )
            {
                if (bIncludeEnd == false
                    && start == end)
                    break;  // 不包含尾部

                Debug.Assert(start != null, "");
                result.Add(start);

                if (start == end)
                    break;  // 包含尾部

                start = start.GetNextSibling();
                if (start == null)
                {
                    Debug.Assert(false, "竟然没有遇上end");
                    break;
                }
            }

            return result;
        }
#endif

        void ClearSelectedArea()
        {
            this.m_aSelectedArea.Clear();
            this.m_bSelectedAreaOverflowed = false;
        }

        void AddSelectedArea(CellBase obj)
        {
            if (this.m_bSelectedAreaOverflowed == true)
                return;

            int index = this.m_aSelectedArea.IndexOf(obj);
            if (index == -1)
                this.m_aSelectedArea.Add(obj);

            if (this.m_aSelectedArea.Count > 1000)
                this.m_bSelectedAreaOverflowed = true;
        }

        void RemoveSelectedArea(CellBase obj)
        {
            if (this.m_bSelectedAreaOverflowed == true)
                return;

            this.m_aSelectedArea.Remove(obj);
        }


        // 选择一系列对象
        void SelectObjects(List<CellBase> aObject,
            SelectAction action)
        {
            if (aObject == null)
                return;

            for (int i = 0; i < aObject.Count; i++)
            {
                CellBase obj = aObject[i];
                if (obj == null)
                    continue;

                // 数组不是m_aSelectedArea才能做
                if (aObject != m_aSelectedArea)
                {
                    if (action == SelectAction.On
                        || action == SelectAction.Toggle)
                    {
                        AddSelectedArea(obj);
                    }
                    else if (action == SelectAction.Off)
                    {
                        RemoveSelectedArea(obj);
                    }
                }

                // RectangleF rectUpdate = new RectangleF(0, 0, obj.Width, obj.Height);

                bool bChanged = obj.Select(action/*, true*/);
                if (bChanged == false)
                    continue;

                RectangleF rectUpdate = GetViewRect(obj);
                /*
                rectUpdate = Object.ToRootCoordinate(rectUpdate);

                // 由DataRoot坐标，变换为整体文档坐标，然后变换为屏幕坐标
                rectUpdate.Offset(this.m_lWindowOrgX + m_nLeftBlank,
                    this.m_lWindowOrgY + m_nTopBlank);
                 */
                this.Invalidate(Rectangle.Round(rectUpdate));
            }
        }

        // 得到一个对象的矩形(view坐标)
        RectangleF GetViewRect(object objParam)
        {
            Debug.Assert(objParam != null, "");
            // 2011/8/4
            if (objParam == null)
            {
                return new RectangleF(0, 0, 0, 0);
            }

            if (objParam is Cell
    || objParam is NullCell
                || objParam is IssueBindingItem)
            {
            }
            else
            {
                throw new Exception("objParam必须为类型Cell和NullCell之一");
            }

            if (objParam is Cell)
            {
                Cell obj = (Cell)objParam;

                RectangleF rect = new RectangleF(0, 0, obj.Width, obj.Height);

                rect = obj.ToRootCoordinate(rect);

                // 由DataRoot坐标，变换为整体文档坐标，然后变换为view坐标
                rect.Offset(this.m_lWindowOrgX + m_nLeftBlank,
                    this.m_lWindowOrgY + m_nTopBlank);

                return rect;
            }
            else if (objParam is NullCell)
            {
                NullCell obj = (NullCell)objParam;

                RectangleF rect = new RectangleF(0, 0, this.m_nCellWidth, this.m_nCellHeight);

                // 变换为内容文档坐标
                rect.Offset(this.m_nCoverImageWidth + this.m_nLeftTextWidth + obj.X * this.m_nCellWidth,
                    obj.Y * this.m_nCellHeight);

                // 由内容文档坐标，变换为整体文档坐标，然后变换为view坐标
                rect.Offset(this.m_lWindowOrgX + this.m_nLeftBlank,
                    this.m_lWindowOrgY + this.m_nTopBlank);

                return rect;
            }
            else
            {
                Debug.Assert(objParam is IssueBindingItem, "");

                IssueBindingItem obj = (IssueBindingItem)objParam;

                int nLineNo = this.Issues.IndexOf(obj);
                if (nLineNo == -1)
                {
                    Debug.Assert(nLineNo != -1, "");
                    return new RectangleF(-1, -1, 0, 0);
                }

                RectangleF rect = new RectangleF(0,
                    0,
                    this.m_nCoverImageWidth + this.m_nLeftTextWidth + this.m_nCellWidth * this.m_nMaxItemCountOfOneIssue, // 只包括左侧标题部分
                    this.m_nCellHeight);

                // 变换为内容文档坐标
                rect.Offset(0,
                    nLineNo * this.m_nCellHeight);

                // 由内容文档坐标，变换为整体文档坐标，然后变换为view坐标
                rect.Offset(this.m_lWindowOrgX + this.m_nLeftBlank,
                    this.m_lWindowOrgY + this.m_nTopBlank);

                return rect;
            }
        }


        protected override void OnPaintBackground(PaintEventArgs e)
        {
            /*
            return;
             * */
            base.OnPaintBackground(e);
            /*
            Brush brush0 = null;

            if (this.Enabled == false)
                brush0 = new SolidBrush(Color.LightGray);
            else
                brush0 = new SolidBrush(this.BackColor);

            e.Graphics.FillRectangle(brush0, e.ClipRectangle);

            brush0.Dispose();
            return;
             * */
        }

        // 判断一个NullCell属于哪个合订册
        Cell BelongToBinding(NullCell cell)
        {
            // 遍历合订册对象数组
            for (int i = 0; i < this.ParentItems.Count; i++)
            {
                ItemBindingItem parent_item = this.ParentItems[i];

                IssueBindingItem issue = parent_item.Container; // 假装属于这个期
                Debug.Assert(issue != null, "");

                // 2010/4/1
                if (issue.IssueLayoutState == IssueLayoutState.Accepting)
                    continue;

                int nCol = issue.IndexOfItem(parent_item);
                Debug.Assert(nCol != -1, "");


                if (cell.X != nCol
                    && cell.X != nCol + 1)
                    continue;

                // 找到行号
                int nLineNo = this.Issues.IndexOf(issue);
                Debug.Assert(nLineNo != -1, "");
                if (cell.Y == nLineNo)
                    return parent_item.ContainerCell;

                for (int j = 0; j < parent_item.MemberCells.Count; j++)
                {
                    Cell member_cell = parent_item.MemberCells[j];

                    issue = member_cell.Container;
                    Debug.Assert(issue != null, "");

                    // 2010/4/1
                    if (issue.IssueLayoutState == IssueLayoutState.Accepting)
                        continue;

                    // 找到行号
                    nLineNo = this.Issues.IndexOf(issue);
                    Debug.Assert(nLineNo != -1, "");

                    if (cell.Y == nLineNo)
                        return parent_item.ContainerCell;
                }
            }

            return null;
        }

        // 将出版日期字符串转换为适合显示的格式
        public static string GetDisplayPublishTime(string strPublishTime)
        {
            int nLength = strPublishTime.Length;
            if (nLength > 8)
                strPublishTime = strPublishTime.Insert(8, ":");

            if (nLength > 6)
                strPublishTime = strPublishTime.Insert(6, "-");
            if (nLength > 4)
                strPublishTime = strPublishTime.Insert(4, "-");

            return strPublishTime;
        }

        // 是否本年的第一期?
        internal bool IsYearFirstIssue(IssueBindingItem issue)
        {
            int index = this.Issues.IndexOf(issue);
            Debug.Assert(index != -1, "");

            if (index == -1)
                return false;

            if (index == 0)
                return true;

            IssueBindingItem prev_issue = this.Issues[index - 1];
            string strThisYear = dp2StringUtil.GetYearPart(issue.PublishTime);
            string strPrevYear = dp2StringUtil.GetYearPart(prev_issue.PublishTime);

            if (String.Compare(strThisYear, strPrevYear) > 0)
                return true;

            return false;
        }

        void PaintIssues(
            long x0,
            long y0,
            PaintEventArgs e)
        {
            long x = x0;
            long y = y0;

            // 整个内容区域的高度
            this.m_lContentHeight = this.m_nCellHeight * this.Issues.Count;

            bool bDrawBottomLine = true;    // 是否要画下方线条

            long lIssueWidth = this.m_lContentWidth;
            long lIssueHeight = this.m_nCellHeight; // issue.Height;

            // 优化
            int nStartLine = (int)((e.ClipRectangle.Top - y) / lIssueHeight);
            nStartLine = Math.Max(0, nStartLine);
            y += nStartLine * lIssueHeight;

            for (int i = nStartLine; i < this.Issues.Count; i++)
            {
                // 优化
                if (y > e.ClipRectangle.Bottom)
                {
                    bDrawBottomLine = false;
                    break;
                }

                IssueBindingItem issue = this.Issues[i];

                if (TooLarge(x) == true
                    || TooLarge(y) == true)
                    goto CONTINUE;

                // 优化
                RectangleF rect = new RectangleF((int)x,
                    (int)y,
                    lIssueWidth,
                    lIssueHeight);

                if (rect.IntersectsWith(e.ClipRectangle) == false)
                    goto CONTINUE;

                issue.Paint(i, (int)x, (int)y, e);
            CONTINUE:
                y += lIssueHeight;
                //  lHeight += lIssueHeight;
            }

            long lHeight = lIssueHeight * this.Issues.Count;

            // 右、下线条

            using (Pen penFrame = new Pen(Color.FromArgb(50, Color.Gray), (float)1))
            {

                // 右方竖线
                if (TooLarge(x0 + this.m_lContentWidth) == false)
                {
                    e.Graphics.DrawLine(penFrame,
                        new PointF((int)x0 + this.m_lContentWidth, (int)y0),
                        new PointF((int)x0 + this.m_lContentWidth, (int)(y0 + lHeight))
                        );
                }

                // 下方横线
                if (bDrawBottomLine == true
                    && TooLarge(y0 + lHeight) == false)
                {

                    e.Graphics.DrawLine(penFrame,
                        new PointF((int)x0 + this.m_nCoverImageWidth + this.m_nLeftTextWidth, (int)(y0 + lHeight)),
                        new PointF((int)x0 + this.m_lContentWidth, (int)(y0 + lHeight))
                        );
                }
            }

            // 绘制合订本范围的连接线
            // 遍历合订册对象数组
            for (int i = 0; i < this.ParentItems.Count; i++)
            {
                ItemBindingItem parent_item = this.ParentItems[i];

                Debug.Assert(parent_item != null, "");

                if (parent_item.Container == null)
                    continue;

                PointF[] points = null;
                RectangleF rectBound;

                Debug.Assert(parent_item.Container != null, "");

                bool bAllBindingLayout = GetBoundPoints(
                    parent_item,
                    this.m_lWindowOrgX + this.m_nLeftBlank + this.m_nCoverImageWidth + this.m_nLeftTextWidth,
                    this.m_lWindowOrgY + this.m_nTopBlank,
                    out points,
                    out rectBound);

                // 优化
                if (rectBound.IntersectsWith(e.ClipRectangle) == false)
                    continue;

                if (points.Length == 0)
                {
                    Debug.Assert(false, "");
                    continue;
                }

                Color colorBorder;
                {
                    Pen penBorder = null;
                    Brush brushInner = null;
                    Brush brush = null;
                    try
                    {
                        if (CheckProcessingState(parent_item) == false)
                        {
                            colorBorder = this.FixedBorderColor;
                            penBorder = new Pen(Color.FromArgb(150, colorBorder),
                                (float)4);  // 固化

                            Debug.Assert(brushInner == null, "");   // 2017/11/10

                            brushInner = new SolidBrush(Color.FromArgb(30, Color.Green));
                        }
                        else
                        {
                            colorBorder = this.NewlyBorderColor;
                            Debug.Assert(brush == null, "");  // 2017/11/10
                            brush = new HatchBrush(HatchStyle.WideDownwardDiagonal,
                                Color.FromArgb(0, 255, 255, 255),
                                Color.FromArgb(255, colorBorder)
                                );    // back
                            // Dispose() penBorder 时是否也会 Dispose() brush?
                            Debug.Assert(penBorder == null, "");  // 2017/11/10
                            penBorder = new Pen(brush,
                                (float)4);  // 可修改
                            // penBorder.Alignment = PenAlignment.
                        }

                        e.Graphics.RenderingOrigin = Point.Round(points[0]);

                        // 绘制方框
                        if (bAllBindingLayout == true)
                        {
                            float delta = (penBorder.Width / 2) + 1;
                            rectBound.Inflate(-delta, -delta);

                            BindingControl.RoundRectangle(e.Graphics,
            penBorder,
            brushInner,
            rectBound,
            10);
                            continue;
                        }

                        // 获得纵向偏移量
                        int nOffset = GetVerticalOffset(parent_item.Container,
                            parent_item);
                        if (nOffset == 0)
                        {
                            if (this.LineStyle == BoundLineStyle.Line)
                                e.Graphics.DrawLines(penBorder, points);
                            else
                                e.Graphics.DrawCurve(penBorder, points);
                        }
                        else
                        {
                            int nStep = (this.m_nCellHeight - this.CellMargin.Vertical - this.CellPadding.Vertical) / 4;
                            nOffset = (nOffset % 5);
                            if ((nOffset % 2) == 1)
                            {
                                nOffset = -1 * ((nOffset + 1) / 2);
                            }
                            else
                            {
                                nOffset = nOffset / 2;
                            }

                            if (points.Length >= 2)
                            {
                                points[0].Y += nOffset * nStep;
                                points[1].Y += nOffset * nStep;
                            }

                            if (this.LineStyle == BoundLineStyle.Line)
                                e.Graphics.DrawLines(penBorder, points);
                            else
                                e.Graphics.DrawCurve(penBorder, points);
                        }

                    }
                    finally
                    {
                        if (penBorder != null)
                            penBorder.Dispose();
                        if (brushInner != null)
                            brushInner.Dispose();
                        if (brush != null)  // 2017/12/7 修改
                            brush.Dispose();
                    }
                }

                PaintCenterDots(points,
                    colorBorder,
                    e);
            }
        }

        // 获得纵向偏移量
        static int GetVerticalOffset(IssueBindingItem issue,
            ItemBindingItem parent_item)
        {
            int nOffset = 0;
            for (int i = 0; i < issue.Cells.Count; i++)
            {
                Cell cell = issue.Cells[i];
                if (cell == null || cell.item == null)
                    continue;
                if (cell.item == parent_item)
                    break;
                if (cell.item.IsParent == true)
                    nOffset++;
            }

            return nOffset;
        }

        void PaintCenterDots(PointF[] points,
            Color colorBorder,
            PaintEventArgs e)
        {
            // 最好是偶数

            int nLargeCircleWidth = 16; // 第一个大圈圈的直径
            int nCircleWidth = 10;  // 其他小圈圈的直径

            using (Pen pen = new Pen(colorBorder))
            using (Brush brush = new SolidBrush(Color.White))
            {
                for (int i = 0; i < points.Length; i++)
                {
                    PointF point = points[i];
                    RectangleF rect;
                    if (i == 0)
                    {
                        rect = new RectangleF(point.X - nLargeCircleWidth / 2,
                          point.Y - nLargeCircleWidth / 2,
                          nLargeCircleWidth,
                          nLargeCircleWidth);
                    }
                    else
                    {
                        rect = new RectangleF(point.X - nCircleWidth / 2,
                           point.Y - nCircleWidth / 2,
                           nCircleWidth,
                           nCircleWidth);
                    }

                    // 优化
                    if (rect.IntersectsWith(e.ClipRectangle) == false)
                        continue;

                    Circle(e.Graphics, pen, brush, rect);
                }
            }
        }

        bool GetBoundPoints(ItemBindingItem parent_item,
            long lStartX,
            long lStartY,
            out PointF[] results,
            out RectangleF rectBound)
        {
            results = new PointF[0];
            rectBound = new RectangleF();
            List<PointF> points = new List<PointF>();
            IssueBindingItem parent_issue = parent_item.Container; // 假装属于这个期
            Debug.Assert(parent_issue != null, "");

            // 找到开始行号
            int nStartLineNo = this.Issues.IndexOf(parent_issue);
            Debug.Assert(nStartLineNo != -1, "");
            if (nStartLineNo == -1)
                return true;

            int nParentCol = parent_issue.IndexOfItem(parent_item);
            Debug.Assert(nParentCol != -1, "");


            {
                long y = lStartY
        + (long)m_nCellHeight * (long)nStartLineNo
        + m_nCellHeight / 2;
                long x = lStartX
                    + (long)m_nCellWidth * nParentCol
                    + m_nCellWidth / 2;
                points.Add(new PointF(x, y));

                rectBound = new RectangleF(
                    x - m_nCellWidth / 2,
                    y - m_nCellHeight / 2,
                    m_nCellWidth,
                    m_nCellHeight);
            }


            bool bAllBindingLayout = true;

            for (int i = 0; i < parent_item.MemberCells.Count; i++)
            {
                Cell member_cell = parent_item.MemberCells[i];
                if (member_cell == null)
                {
                    Debug.Assert(false, "");
                    continue;
                }

                IssueBindingItem member_issue = member_cell.Container;
                Debug.Assert(member_issue != null, "");

                if (member_issue.IssueLayoutState != IssueLayoutState.Binding)
                    bAllBindingLayout = false;

                int nMemberLineNo = this.Issues.IndexOf(member_issue);
                Debug.Assert(nMemberLineNo != -1, "");

                int nMemberCol = member_issue.Cells.IndexOf(member_cell);
                Debug.Assert(nMemberCol != -1, "");

                long y = lStartY
                    + (long)m_nCellHeight * (long)nMemberLineNo
                    + m_nCellHeight / 2;
                long x = lStartX
                    + (long)m_nCellWidth * nMemberCol
                    + m_nCellWidth / 2;
                points.Add(new PointF(x, y));

                RectangleF rectCell;
                rectCell = new RectangleF(
    x - m_nCellWidth / 2,
    y - m_nCellHeight / 2,
    m_nCellWidth,
    m_nCellHeight);
                rectBound = RectangleF.Union(rectCell, rectBound);
            }

            results = new PointF[points.Count];
            points.CopyTo(results);

            /*
            if (bAllBindingLayout == true)
            {
                rectBound = new RectangleF(points[0].X - this.m_nCellWidth / 2,
                    points[0].Y - this.m_nCellHeight / 2,
                    this.m_nCellWidth * 2,
                    (points.Length - 1) * m_nCellHeight);
            }
             * */

            return bAllBindingLayout;
        }


        public enum ScrollBarMember
        {
            Vert = 0,
            Horz = 1,
            Both = 2,
        }

        // 检查一个long是否越过int16能表达的值范围
        public static bool TooLarge(long lValue)
        {
            if (lValue >= Int16.MaxValue || lValue <= Int16.MinValue)
                return true;
            return false;
        }

        static string GetString(API.ScrollInfoStruct si)
        {
            string strResult = "";
            strResult += "si.nMin:" + si.nMin.ToString() + "\r\n";
            strResult += "si.nMax:" + si.nMax.ToString() + "\r\n";
            strResult += "si.nPage:" + si.nPage.ToString() + "\r\n";
            strResult += "si.nPos:" + si.nPos.ToString() + "\r\n";
            return strResult;
        }

        void SetScrollBars(ScrollBarMember member)
        {

            nNestedSetScrollBars++;


            try
            {
                int nClientWidth = this.ClientSize.Width;
                int nClientHeight = this.ClientSize.Height;

                // 文档尺寸
                long lDocumentWidth = DocumentWidth;
                long lDocumentHeight = DocumentHeight;

                long lWindowOrgX = this.m_lWindowOrgX;
                long lWindowOrgY = this.m_lWindowOrgY;

                if (member == ScrollBarMember.Horz
                    || member == ScrollBarMember.Both)
                {

                    if (TooLarge(lDocumentWidth) == true)
                    {
                        this.m_h_ratio = (double)(Int16.MaxValue - 1) / (double)lDocumentWidth;

                        lDocumentWidth = (long)((double)lDocumentWidth * m_h_ratio);
                        nClientWidth = (int)((double)nClientWidth * m_h_ratio);
                        lWindowOrgX = (long)((double)lWindowOrgX * m_h_ratio);
                    }
                    else
                        this.m_h_ratio = 1.0F;

                    // 水平方向
                    API.ScrollInfoStruct si = new API.ScrollInfoStruct();

                    si.cbSize = Marshal.SizeOf(si);
                    si.fMask = API.SIF_RANGE | API.SIF_POS | API.SIF_PAGE;
                    si.nMin = 0;
                    si.nMax = (int)lDocumentWidth;
                    si.nPage = nClientWidth;
                    si.nPos = -(int)lWindowOrgX;
                    API.SetScrollInfo(this.Handle, API.SB_HORZ, ref si, true);

                    // Debug.WriteLine("SetScrollInfo() HORZ\r\n" + GetString(si));
                }


                if (member == ScrollBarMember.Vert
                    || member == ScrollBarMember.Both)
                {
                    if (TooLarge(lDocumentHeight) == true)
                    {
                        this.m_v_ratio = (double)(Int16.MaxValue - 1) / (double)lDocumentHeight;

                        lDocumentHeight = (long)((double)lDocumentHeight * m_v_ratio);
                        nClientHeight = (int)((double)nClientHeight * m_v_ratio);
                        lWindowOrgY = (long)((double)lWindowOrgY * m_v_ratio);

                    }
                    else
                        this.m_v_ratio = 1.0F;

                    // 垂直方向
                    API.ScrollInfoStruct si = new API.ScrollInfoStruct();

                    si.cbSize = Marshal.SizeOf(si);
                    si.fMask = API.SIF_RANGE | API.SIF_POS | API.SIF_PAGE;
                    si.nMin = 0;
                    si.nMax = (int)lDocumentHeight;
                    si.nPage = nClientHeight;
                    si.nPos = -(int)lWindowOrgY;
                    // Debug.Assert(si.nPos != 0, "");
                    API.SetScrollInfo(this.Handle, API.SB_VERT, ref si, true);

                    // Debug.WriteLine("SetScrollInfo() VERT\r\n" + GetString(si));
                }

            }
            finally
            {
                nNestedSetScrollBars--;
            }
        }

        public long DocumentWidth
        {
            get
            {
                return m_lContentWidth + (long)m_nLeftBlank + (long)m_nRightBlank;
            }

        }
        public long DocumentHeight
        {
            get
            {
                return m_lContentHeight + (long)m_nTopBlank + (long)m_nBottomBlank;
            }
        }

        public long DocumentOrgX
        {
            get
            {
                return m_lWindowOrgX;
            }
            set
            {
                long lWidth = DocumentWidth;
                int nViewportWidth = this.ClientSize.Width;

                long lWindowOrgX_old = m_lWindowOrgX;


                if (nViewportWidth >= lWidth)
                    m_lWindowOrgX = 0;
                else
                {
                    if (value <= -lWidth + nViewportWidth)
                        m_lWindowOrgX = -lWidth + nViewportWidth;
                    else
                        m_lWindowOrgX = value;

                    if (m_lWindowOrgX > 0)
                        m_lWindowOrgX = 0;
                }

                // AfterDocumentChanged(ScrollBarMember.Horz);
                SetScrollBars(ScrollBarMember.Horz);



                if (this.BackgroundImage != null)
                {
                    this.Invalidate();
                    return;
                }

                long lDelta = m_lWindowOrgX - lWindowOrgX_old;

                if (lDelta != 0)
                {
                    // 如果卷滚的距离超过32位整数范围
                    if (lDelta >= Int32.MaxValue || lDelta <= Int32.MinValue)
                        this.Invalidate();
                    else
                    {
                        RECT rect1 = new RECT();
                        rect1.left = 0;
                        rect1.top = 0;
                        rect1.right = this.ClientSize.Width;
                        rect1.bottom = this.ClientSize.Height;


                        API.ScrollWindowEx(this.Handle,
                            (int)lDelta,
                            0,
                            ref rect1,
                            IntPtr.Zero,	//	ref RECT lprcClip,
                            0,	// int hrgnUpdate,
                            IntPtr.Zero,	// ref RECT lprcUpdate,
                            API.SW_INVALIDATE /*| API.SW_SCROLLCHILDREN*/ /*int fuScroll*/);
                    }
                }

                // this.Invalidate();
            }
        }

        public long DocumentOrgY
        {
            get
            {
                return m_lWindowOrgY;
            }
            set
            {
                // Debug.Assert(value != 0, "");
                long lHeight = DocumentHeight;
                int nViewportHeight = this.ClientSize.Height;

                long lWindowOrgY_old = m_lWindowOrgY;

                if (nViewportHeight >= lHeight)
                    m_lWindowOrgY = 0;
                else
                {
                    if (value <= -lHeight + nViewportHeight)
                        m_lWindowOrgY = -lHeight + nViewportHeight;
                    else
                        m_lWindowOrgY = value;

                    if (m_lWindowOrgY > 0)
                        m_lWindowOrgY = 0;
                }


                // AfterDocumentChanged(ScrollBarMember.Vert);
                SetScrollBars(ScrollBarMember.Vert);

                if (this.BackgroundImage != null)
                {
                    this.Invalidate();
                    return;
                }

                long lDelta = m_lWindowOrgY - lWindowOrgY_old;
                if (lDelta != 0)
                {
                    // 如果卷滚的距离超过32位整数范围
                    if (lDelta >= Int32.MaxValue || lDelta <= Int32.MinValue)
                        this.Invalidate();
                    else
                    {

                        RECT rect1 = new RECT();
                        rect1.left = 0;
                        rect1.top = 0;
                        rect1.right = this.ClientSize.Width;
                        rect1.bottom = this.ClientSize.Height;


                        API.ScrollWindowEx(this.Handle,
                            0,
                            (int)lDelta,
                            ref rect1,
                            IntPtr.Zero,	//	ref RECT lprcClip,
                            0,	// int hrgnUpdate,
                            IntPtr.Zero,	// ref RECT lprcUpdate,
                            API.SW_INVALIDATE /*| API.SW_SCROLLCHILDREN*/ /*int fuScroll*/);

                    }

                }

                // this.Invalidate();
            }
        }

        #endregion

        protected override void OnPaint(PaintEventArgs pe)
        {
            // TODO: Add custom paint code here

            // Calling the base class OnPaint
            base.OnPaint(pe);

            pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            pe.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            // e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            // e.Graphics.SetClip(e.ClipRectangle); // 废话

            long xOffset = m_lWindowOrgX + m_nLeftBlank;
            long yOffset = m_lWindowOrgY + m_nTopBlank;

            this.PaintIssues(xOffset, yOffset, pe);
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case API.WM_GETDLGCODE:
                    m.Result = new IntPtr(API.DLGC_WANTALLKEYS | API.DLGC_WANTARROWS | API.DLGC_WANTCHARS);
                    return;
                case API.WM_VSCROLL:
                    {
                        int CellWidth = this.m_nCellHeight / 2;
                        switch (API.LoWord(m.WParam.ToInt32()))
                        {
                            case API.SB_BOTTOM:
                                break;
                            case API.SB_TOP:
                                break;
                            case API.SB_THUMBPOSITION:
                            case API.SB_THUMBTRACK:
                                this.Update();
                                int v = API.HiWord(m.WParam.ToInt32());
                                if (this.m_v_ratio != 1.0F)
                                    DocumentOrgY = -(long)((double)v / this.m_v_ratio);
                                else
                                    DocumentOrgY = -v;
                                break;
                            case API.SB_LINEDOWN:
                                DocumentOrgY -= (int)CellWidth;
                                break;
                            case API.SB_LINEUP:
                                DocumentOrgY += (int)CellWidth;
                                break;
                            case API.SB_PAGEDOWN:
                                DocumentOrgY -= this.ClientSize.Height;
                                break;
                            case API.SB_PAGEUP:
                                DocumentOrgY += this.ClientSize.Height;
                                break;
                        }
                        // MessageBox.Show("this");
                    }
                    break;

                case API.WM_HSCROLL:
                    {

                        int CellWidth = this.m_nCellWidth / 2;
                        switch (API.LoWord(m.WParam.ToInt32()))
                        {
                            case API.SB_THUMBPOSITION:
                            case API.SB_THUMBTRACK:
                                int v = API.HiWord(m.WParam.ToInt32());
                                if (this.m_h_ratio != 1.0F)
                                    DocumentOrgX = -(long)((double)v / this.m_h_ratio);
                                else
                                    DocumentOrgX = -v;
                                break;
                            case API.SB_LINEDOWN:
                                DocumentOrgX -= CellWidth;
                                break;
                            case API.SB_LINEUP:
                                DocumentOrgX += CellWidth;
                                break;
                            case API.SB_PAGEDOWN:
                                DocumentOrgX -= this.ClientSize.Width;
                                break;
                            case API.SB_PAGEUP:
                                DocumentOrgX += this.ClientSize.Width;
                                break;
                        }
                    }
                    break;

                default:
                    break;

            }

            base.DefWndProc(ref m);
        }


        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams param = base.CreateParams;

                if (borderStyle == BorderStyle.FixedSingle)
                {
                    param.Style |= API.WS_BORDER;
                }
                else if (borderStyle == BorderStyle.Fixed3D)
                {
                    param.ExStyle |= API.WS_EX_CLIENTEDGE;
                }

                return param;
            }
        }



        private void BorderStyleToWindowStyle(ref int style, ref int exStyle)
        {
            style &= ~API.WS_BORDER;
            exStyle &= ~API.WS_EX_CLIENTEDGE;
            switch (borderStyle)
            {
                case BorderStyle.Fixed3D:
                    exStyle |= API.WS_EX_CLIENTEDGE;
                    break;

                case BorderStyle.FixedSingle:
                    style |= API.WS_BORDER;
                    break;

                case BorderStyle.None:
                    // No border style values
                    break;
            }
        }

        // 利用事件接口this.GetItemInfo，获得所需的册信息
        // return:
        //      -1  error
        //      >-0 所获得记录个数。(XmlRecords.Count)
        internal int DoGetItemInfo(string strPublishTime,
            out List<string> XmlRecords,
            out string strError)
        {
            strError = "";

            XmlRecords = new List<string>();

            if (this.GetItemInfo == null)
            {
                strError = "尚未挂接GetItemInfo事件";
                return -1;
            }


            GetItemInfoEventArgs e1 = new GetItemInfoEventArgs();
            e1.BiblioRecPath = "";
            e1.PublishTime = strPublishTime;
            this.GetItemInfo(this, e1);
            if (String.IsNullOrEmpty(e1.ErrorInfo) == false)
            {
                strError = "在获取本种内出版日期为 '" + strPublishTime + "' 的册信息的过程中发生错误: " + e1.ErrorInfo;
                return -1;
            }

            XmlRecords = e1.ItemXmls;

            return XmlRecords.Count;
        }

        // 模仿MouseMove事件导致卷滚
        private void timer_dragScroll_Tick(object sender, EventArgs e)
        {
            if (this.Capture == false)
                return;

            Point p = this.PointToClient(Control.MousePosition);
            MouseEventArgs e1 = new MouseEventArgs(MouseButtons.Left,
                0, // clicks,
                p.X,
                p.Y,
                0 //delta
                );
            // this.mouseMoveArgs
            this.OnMouseMove(e1);
        }

        internal string DoGetMacroValue(string strMacroName)
        {
            if (this.GetMacroValue != null)
            {
                GetMacroValueEventArgs e = new GetMacroValueEventArgs();
                e.MacroName = strMacroName;
                this.GetMacroValue(this, e);

                return e.MacroValue;
            }

            return null;
        }

        // 为 strXml 中设置默认的字段值
        public int SetItemDefaultValues(
    string strCfgEntry,
    bool bGetMacroValue,
    ref string strXml,
    // BookItem item,
    out string strError)
        {
            strError = "";

            XmlDocument old_dom = new XmlDocument();
            try
            {
                old_dom.LoadXml(string.IsNullOrEmpty(strXml) ? "<root />" : strXml);
            }
            catch (Exception ex)
            {
                strError = "原有 XML 记录装入 DOM 时出错: " + ex.Message;
                return -1;
            }

            string strNewDefault = Program.MainForm.AppInfo.GetString(
    "entityform_optiondlg",
    strCfgEntry,
    "<root />");

            // 字符串strNewDefault包含了一个XML记录，里面相当于一个记录的原貌。
            // 但是部分字段的值可能为"@"引导，表示这是一个宏命令。
            // 需要把这些宏兑现后，再正式给控件
            XmlDocument new_dom = new XmlDocument();
            try
            {
                new_dom.LoadXml(strNewDefault);
            }
            catch (Exception ex)
            {
                strError = "默认值定义 XML 记录装入 DOM 时出错: " + ex.Message;
                return -1;
            }

            // 合并新旧记录的第一级元素
            // 合并新旧记录
            // domExist 中的非空元素内容保留，添加 domNew 中的其余元素
            int nRet = MergeTwoEntityXml(old_dom,
                new_dom,
                out string strMergedXml,
                out strError);
            if (nRet == -1)
                return -1;

            new_dom.LoadXml(strMergedXml);

            if (bGetMacroValue == true)
            {
                // 遍历所有一级元素的内容
                XmlNodeList nodes = new_dom.DocumentElement.SelectNodes("*");
                for (int i = 0; i < nodes.Count; i++)
                {
                    string strText = nodes[i].InnerText;
                    if (strText.Length > 0
                        && (strText[0] == '@' || strText.IndexOf("%") != -1))
                    {
                        // 兑现宏
                        nodes[i].InnerText = DoGetMacroValue(strText);
                    }
                }
            }

            DomUtil.RemoveEmptyElements(new_dom.DocumentElement);

            strXml = new_dom.OuterXml;
#if NO
            strNewDefault = dom.OuterXml;

            int nRet = item.SetData("",
                strNewDefault,
                null,
                out strError);
            if (nRet == -1)
                return -1;

            item.Parent = "";
            item.RecPath = "";
#endif
            return 0;
        }

        // 合并新旧记录
        // domExist 中的非空元素内容保留，添加 domNew 中的其余元素
        static int MergeTwoEntityXml(XmlDocument domExist,
            XmlDocument domNew,
            out string strMergedXml,
            out string strError)
        {
            strMergedXml = "";
            strError = "";

            XmlNodeList nodes = domExist.DocumentElement.SelectNodes("*");

            // 算法的要点是, 把"新记录"中的要害字段, 覆盖到"已存在记录"中
            foreach (XmlElement node in nodes)
            {
                string name = node.Name;

                if (string.IsNullOrEmpty(node.InnerXml.Trim()) == true)
                    continue;

                string strTextNew = DomUtil.GetElementOuterXml(domExist.DocumentElement,
                    name);
                if (string.IsNullOrEmpty(strTextNew) == false)
                    DomUtil.SetElementOuterXml(domNew.DocumentElement,
                        name, strTextNew);
            }

            strMergedXml = domNew.OuterXml;
            return 0;
        }
    }

    // 点击检测结果
    internal class HitTestResult
    {
        public Object Object = null;    // 点击到的末级对象
        public AreaPortion AreaPortion = AreaPortion.None;

        // 对象坐标下的点击位置
        public long X = -1;
        public long Y = -1;

        public int Param = 0;   // 其他参数
    }

    // 区域名称
    internal enum AreaPortion
    {
        None = 0,
        Blank = 1,    // 空白部分。指Cell不足延伸到的部分，或者空的Cell对象所在位置

        Content = 2,    // 内容本体
        CoverImage = 3, // 左边的封面图像
        LeftText = 4,   // 左边的文字，指IssueBindingItem

        LeftBlank = 5,  // 左边空白
        TopBlank = 6,   // 上方空白
        RightBlank = 7, // 右方空白
        BottomBlank = 8,    // 下方空白

        Grab = 9,   // moving grab handle
        CheckBox = 10,  // 格子中央的checkbox

        CoverImageEdge = 11,   // 封面图像列右侧的可修改大小的竖线位置
    }

    // 选择一个对象的动作
    internal enum SelectAction
    {
        Toggle = 0,
        On = 1,
        Off = 2,
    }


    /// <summary>
    /// 焦点发生改变事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void FocusChangedEventHandler(object sender,
    FocusChangedEventArgs e);

    /// <summary>
    /// 焦点发生改变事件的参数
    /// </summary>
    public class FocusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 先前焦点所在对象
        /// </summary>
        public object OldFocusObject = null;
        /// <summary>
        /// 现在焦点所在对象
        /// </summary>
        public object NewFocusObject = null;
    }

    /// <summary>
    /// 编辑区交互事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void EditAreaEventHandler(object sender,
        EditAreaEventArgs e);

    /// <summary>
    /// 编辑区交互事件的参数
    /// </summary>
    public class EditAreaEventArgs : EventArgs
    {
        /// <summary>
        /// [in] 动作
        /// </summary>
        public string Action = "";  // [in] 动作

        /// <summary>
        /// [out] 结果
        /// </summary>
        public string Result = "";  // [out] 结果
    }

    internal class ItemAndCol
    {
        public ItemBindingItem item = null;
        public int Index = -1;
    }
}
