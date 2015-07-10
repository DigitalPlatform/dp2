using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Diagnostics;
// using System.Windows.Media;

using System.Runtime.InteropServices;

using DigitalPlatform;
using DigitalPlatform.GUI;

namespace DigitalPlatform.CommonControl
{
    public partial class DpTable : Control
    {
        public ImageList ImageList = null;
        
        public event EventHandler SelectionChanged = null;

        /// <summary>
        /// 卷滚条被触动事件
        /// </summary>
        public event ScrollBarTouchedEventHandler ScrollBarTouched = null;

        /// <summary>
        /// 绘制扩展区域的事件
        /// </summary>
        public event PaintRegionEventHandler PaintRegion = null;

        /// <summary>
        /// 绘制背景的事件
        /// </summary>
        public event PaintBackEventHandler PaintBack = null;

        internal bool HasPaintBack
        {
            get
            {
                return this.PaintBack != null;
            }
        }

        internal void TriggerPaintBack(object sender, PaintBackArgs e)
        {
            if (this.PaintBack != null)
                this.PaintBack(sender, e);
        }

        internal bool m_bAutoDocCenter = true;
        [Category("Appearance")]
        [DescriptionAttribute("Auto Align Whole Document Center")]
        [DefaultValue(typeof(int), "true")]
        public bool AutoDocCenter
        {
            get
            {
                return this.m_bAutoDocCenter;
            }
            set
            {
                this.m_bAutoDocCenter = value;

                int nOldDelta = this.m_nHorzDelta;
                if (value == true)
                {
                    SetAutoCenterValue();
                }
                else
                    this.m_nHorzDelta = 0;

                if (this.m_bDelayUpdate == false)
                {
                    if (nOldDelta != this.m_nHorzDelta)
                        this.Invalidate();
                }
            }
        }

        internal int m_nHorzDelta = 0;  // 因为居中要考虑的左边空白

        internal int m_nMaxTextHeight = 100;
        [DescriptionAttribute("Max Text Height of Rows")]
        [DefaultValue(typeof(int), "100")]
        public int MaxTextHeight
        {
            get
            {
                return this.m_nMaxTextHeight;
            }
            set
            {
                this.m_nMaxTextHeight = value;

                {
                    long lOldContentHeight = this.m_lContentHeight;
                    this.RefreshAllTextHeight();
                    if (lOldContentHeight != this.m_lContentHeight)
                    {
                        this.SetScrollBars(ScrollBarMember.Vert);
                    }
                }

            }
        }

        // 行间距
        internal int m_nLineDistance = 0;
        [DescriptionAttribute("Distance of Rows")]
        [DefaultValue(typeof(int), "0")]
        public int LineDistance
        {
            get
            {
                return this.m_nLineDistance;
            }
            set
            {
                this.m_nLineDistance = value;


                // 重新布局
                {
                    long lOldContentHeight = this.m_lContentHeight;
                    this.RefreshAllTextHeight();
                    if (lOldContentHeight != this.m_lContentHeight)
                    {
                        this.SetScrollBars(ScrollBarMember.Vert);
                    }
                }
            }
        }

        internal DpColumnRow m_columns = new DpColumnRow();
        // 栏标题
        [
        DescriptionAttribute("Columns"),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        Editor(typeof(System.ComponentModel.Design.CollectionEditor), typeof(System.Drawing.Design.UITypeEditor))
        ]
        public DpColumnRow Columns
        {
            get
            {
                return this.m_columns;
            }
        }

        // 内容行
        public DpRowCollection Rows = new DpRowCollection();

        internal bool m_bFullRowSelect = false;   // 整行选定
        internal bool m_bSingleSelection = false;    // 只允许选一个单元(或者一行)

        internal long m_lWindowOrgX = 0;    // 窗口原点
        internal long m_lWindowOrgY = 0;

        internal long m_lContentWidth = 0;    // 内容部分的宽度
        internal long m_lContentHeight = 0;   // 内容部分的高度(包含了Columns的高度)

        internal List<DpRow> m_seletecLines = null;     // 选定的行对象
        internal List<int> m_selectedLineIndices = null; // 选定的行的行号

        internal List<CellXY> m_seletedCellXY = null;    // 选定的格子坐标

        internal object m_focusObj = null;
        internal object m_shiftStartObj = null;

        public DpTable()
        {
            this.DoubleBuffered = true;

            this.Rows.Control = this;
            this.m_columns.Control = this;

            InitializeComponent();

            this.m_lContentHeight = this.GetContentHeight();
        }

        public object FocusedItem
        {
            get
            {
                return this.m_focusObj;
            }
            set
            {
                if (value != null)
                {
                    if (this.m_bFullRowSelect == true)
                    {
                        if (!(value is DpRow))
                            throw new Exception("当FullRowSelect == true的时候，只能使用DpRow类型的对象");
                    }
                    else
                    {
                        if (!(value is DpCell))
                            throw new Exception("当FullRowSelect == false的时候，只能使用DpCell类型的对象");
                    }
                }

                bool bChanged = false;
                if (this.m_focusObj != value)
                    bChanged = true;

                if (this.m_focusObj != null
                    && bChanged == true/*this.m_focusObj != value*/)
                {
                    this.InvalidateObject(this.m_focusObj);
                }
                this.m_focusObj = value;
                if (this.m_focusObj != null
                    && bChanged == true/*this.m_focusObj != value*/)
                {
                    this.InvalidateObject(this.m_focusObj);
                }

                // 2015/7/10
                this.m_shiftStartObj = value;
            }
        }

        [Category("Appearance")]
        [DescriptionAttribute("FullRowSelect")]
        [DefaultValue(typeof(bool), "false")]
        public bool FullRowSelect
        {
            get
            {
                return this.m_bFullRowSelect;
            }
            set
            {
                this.m_bFullRowSelect = value;
                ExpireSelectedLines();
                ExpireSelectedXYs();
            }
        }

        internal Color m_hoverBackColor = SystemColors.HotTrack;

        [Category("Appearance")]
        [DescriptionAttribute("BackColor of Hover Object")]
        [DefaultValue(typeof(Color), "SystemColors.HotTrack")]
        public Color HoverBackColor
        {
            get
            {
                return m_hoverBackColor;
            }
            set
            {
                this.m_hoverBackColor = value;
            }
        }

        internal Color m_columnsBackColor = SystemColors.Control;

        [Category("Appearance")]
        [DescriptionAttribute("BackColor of Columns")]
        [DefaultValue(typeof(Color), "SystemColors.Control")]
        public Color ColumnsBackColor
        {
            get
            {
                return m_columnsBackColor;
            }
            set
            {
                this.m_columnsBackColor = value;
                if (this.m_bDelayUpdate == false)
                {
                    this.Invalidate();
                }
            }
        }

        internal Color m_columnsForeColor = SystemColors.ControlText;

        [Category("Appearance")]
        [DescriptionAttribute("ForeColor of Columns")]
        [DefaultValue(typeof(Color), "SystemColors.ControlText")]
        public Color ColumnsForeColor
        {
            get
            {
                return m_columnsForeColor;
            }
            set
            {
                this.m_columnsForeColor = value;
                if (this.m_bDelayUpdate == false)
                {
                    this.Invalidate();
                }
            }
        }

        // 栏标题每个Cell的Padding
        public Padding m_columnCellPadding = new Padding(4);

        [Category("Appearance")]
        [DescriptionAttribute("Padding of Column Cell")]
        [DefaultValue(typeof(Padding), "4,4,4,4")]
        public Padding ColumnCellPadding
        {
            get
            {
                return this.m_columnCellPadding;
            }
            set
            {
                this.m_columnCellPadding = value;
                this.Columns.OnChanged(false);
            }
        }

        // 每个内容Cell的Padding
        public Padding m_cellPadding = new Padding(4);

        [Category("Appearance")]
        [DescriptionAttribute("Padding of Content Cell")]
        [DefaultValue(typeof(Padding), "4,4,4,4")]
        public Padding CellPadding
        {
            get
            {
                return this.m_cellPadding;
            }
            set
            {
                this.m_cellPadding = value;

                {
                    long lOldContentHeight = this.m_lContentHeight;
                    this.RefreshAllTextHeight();
                    if (lOldContentHeight != this.m_lContentHeight)
                    {
                        this.SetScrollBars(ScrollBarMember.Vert);
                    }
                }
            }
        }
        // 绘制文档边框时，和文档区域的边距
        // 注意，应当小于this.Padding
        public Padding m_documentMargin = new Padding(4);

        [Category("Appearance")]
        [DescriptionAttribute("Margin of Document border")]
        [DefaultValue(typeof(Padding), "4,4,4,4")]
        public Padding DocumentMargin
        {
            get
            {
                return this.m_documentMargin;
            }
            set
            {
                this.m_documentMargin = value;
                if (this.m_bDelayUpdate == false)
                {
                    this.Invalidate();
                }
            }
        }

        //
        internal Color m_documentBorderColor = SystemColors.ControlDark;

        [Category("Appearance")]
        [DescriptionAttribute("BorderColor of Document")]
        [DefaultValue(typeof(Color), "SystemColors.ControlDark")]
        public Color DocumentBorderColor
        {
            get
            {
                return m_documentBorderColor;
            }
            set
            {
                this.m_documentBorderColor = value;
                if (this.m_bDelayUpdate == false)
                {
                    this.Invalidate();
                }
            }
        }

        internal Color m_documentShadowColor = SystemColors.ControlDarkDark;

        [Category("Appearance")]
        [DescriptionAttribute("Border Shadow Color of Document")]
        [DefaultValue(typeof(Color), "SystemColors.ControlDarkDark")]
        public Color DocumentShadowColor
        {
            get
            {
                return m_documentShadowColor;
            }
            set
            {
                this.m_documentShadowColor = value;
                if (this.m_bDelayUpdate == false)
                {
                    this.Invalidate();
                }
            }
        }

        ////

        //
        internal Color m_highlightBackColor = SystemColors.Highlight;

        [Category("Appearance")]
        [DescriptionAttribute("BackColor of Highlight")]
        [DefaultValue(typeof(Color), "SystemColors.Highlight")]
        public Color HighlightBackColor
        {
            get
            {
                return m_highlightBackColor;
            }
            set
            {
                this.m_highlightBackColor = value;
                if (this.m_bDelayUpdate == false)
                {
                    this.Invalidate();
                }
            }
        }

        internal Color m_highlightForeColor = SystemColors.HighlightText;

        [Category("Appearance")]
        [DescriptionAttribute("ForeColor of Highlight")]
        [DefaultValue(typeof(Color), "SystemColors.HighlightText")]
        public Color HightlightForeColor
        {
            get
            {
                return m_highlightForeColor;
            }
            set
            {
                this.m_highlightForeColor = value;
                if (this.m_bDelayUpdate == false)
                {
                    this.Invalidate();
                }
            }
        }

        //
        internal Color m_inactiveHighlightBackColor = SystemColors.InactiveCaption;

        [Category("Appearance")]
        [DescriptionAttribute("BackColor of Inactive Highlight")]
        [DefaultValue(typeof(Color), "SystemColors.InactiveCaption")]
        public Color InactiveHighlightBackColor
        {
            get
            {
                return m_inactiveHighlightBackColor;
            }
            set
            {
                this.m_inactiveHighlightBackColor = value;
                if (this.m_bDelayUpdate == false)
                {
                    this.Invalidate();
                }
            }
        }

        internal Color m_inactiveHighlightForeColor = SystemColors.InactiveCaptionText;

        [Category("Appearance")]
        [DescriptionAttribute("ForeColor of Inactive Highlight")]
        [DefaultValue(typeof(Color), "SystemColors.InactiveCaptionText")]
        public Color InactiveHightlightForeColor
        {
            get
            {
                return m_inactiveHighlightForeColor;
            }
            set
            {
                this.m_inactiveHighlightForeColor = value;
                if (this.m_bDelayUpdate == false)
                {
                    this.Invalidate();
                }
            }
        }

        BorderStyle borderStyle = BorderStyle.Fixed3D;

        [Category("Appearance")]
        [DescriptionAttribute("Border style of the control")]
        [DefaultValue(typeof(System.Windows.Forms.BorderStyle), "Fixed3D")]
        public BorderStyle BorderStyle
        {
            get
            {
                return borderStyle;
            }
            set
            {
                borderStyle = value;

                // Get Styles using Win32 calls
                int style = API.GetWindowLong(Handle, API.GWL_STYLE);
                int exStyle = API.GetWindowLong(Handle, API.GWL_EXSTYLE);

                // Modify Styles to match the selected border style
                BorderStyleToWindowStyle(ref style, ref exStyle);

                // Set Styles using Win32 calls
                API.SetWindowLong(Handle, API.GWL_STYLE, style);
                API.SetWindowLong(Handle, API.GWL_EXSTYLE, exStyle);

                // Tell Windows that the frame changed
                API.SetWindowPos(this.Handle, IntPtr.Zero, 0, 0, 0, 0,
                    API.SWP_NOACTIVATE | API.SWP_NOMOVE | API.SWP_NOSIZE |
                    API.SWP_NOZORDER | API.SWP_NOOWNERZORDER |
                    API.SWP_FRAMECHANGED);
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

        internal int ColumnHeight
        {
            get
            {
                if (this.m_columns.Visible == true)
                    return this.m_columns.TextHeight + this.m_columnCellPadding.Vertical;
                return 0;
            }
        }

        internal bool m_bDelayUpdate = false;

        public void BeginUpdate()
        {
            this.m_bDelayUpdate = true;
        }

        public void EndUpdate()
        {
            this.m_bDelayUpdate = false;
            this.SetScrollBars(ScrollBarMember.Both);
            this.Invalidate();
        }

        // 征询扩展区域的高度
        // parameters:
        //      可以绘制的区域的宽度
        // return:
        //      返回高度
        internal int QueryExtHeight(object item,
            int nIconWidth,
            int nTextWidth)
        {
            if (this.PaintRegion == null)
                return 0;
            PaintRegionArgs e = new PaintRegionArgs();
            e.Action = "query";
            e.Item = item;
            e.X = nIconWidth;
            e.Width = nTextWidth;
            this.PaintRegion(this, e);
            return e.Height;
        }

        internal void PaintExtRegion(
            PaintEventArgs pe,
            object item,
            long x,
            long y,
            int width,
            int height)
        {
            if (this.PaintRegion == null)
                return;
            PaintRegionArgs e = new PaintRegionArgs();
            e.Action = "paint";
            e.pe = pe;
            e.Item = item;
            e.X = x;
            e.Y = y;
            e.Width = width;
            e.Height = height;
            this.PaintRegion(this, e);
        }

        // this.Font字体发生了改变
        protected override void OnFontChanged(EventArgs e)
        {
            base.OnFontChanged(e);

            this.Columns.OnChanged(false);

            {
                long lOldContentHeight = this.m_lContentHeight;
                this.RefreshAllTextHeight();
                if (lOldContentHeight != this.m_lContentHeight)
                {
                    this.SetScrollBars(ScrollBarMember.Vert);
                }
            }
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            base.OnPaint(pe);

            pe.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            pe.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            pe.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality; 
#if NO
            pe.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            pe.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
#endif

            long xOffset = m_lWindowOrgX + this.Padding.Left + this.m_nHorzDelta;
            long yOffset = m_lWindowOrgY + this.Padding.Top
                + (this.ColumnHeight);

            // Rectangle old_clip = pe.ClipRectangle;
            // 标题行
            {
                RectangleF rect = new RectangleF(m_lWindowOrgX,
        0,
        this.m_lContentWidth + this.Padding.Horizontal + this.m_nHorzDelta * 2,
        this.ColumnHeight);
                if (rect.IntersectsWith(pe.ClipRectangle) == true)
                {
                    this.m_columns.Paint(m_lWindowOrgX,
                        0,
                        pe);
                }
            }

            // 修改cliprect，排除标题行
            RectangleF clip = this.ClientRectangle;
            clip.Y += this.ColumnHeight + 1;
            clip.Height -= this.ColumnHeight + 1;
            pe.Graphics.IntersectClip(clip);

            // 绘制背景边框
            // 阴影部分
            if (this.m_documentShadowColor != Color.Transparent)
            {
                using (Brush brush = new SolidBrush(this.m_documentShadowColor))
                {
                    pe.Graphics.FillRectangle(brush,
                        xOffset + this.m_lContentWidth + this.m_documentMargin.Right,
                        yOffset - this.m_documentMargin.Top + 4,
                        4,
                        this.m_lContentHeight - this.ColumnHeight + this.m_documentMargin.Vertical - 1);
                    pe.Graphics.FillRectangle(brush,
                        xOffset - this.m_documentMargin.Left + 4,
                        yOffset + this.m_lContentHeight - this.ColumnHeight + this.m_documentMargin.Bottom,
                        this.m_lContentWidth + this.m_documentMargin.Horizontal -1,
                        4);
                }
            }
            // 边框
            if (this.m_documentBorderColor != Color.Transparent)
            {
                using (Pen pen = new Pen(this.m_documentBorderColor, (float)1))
                {
                    pe.Graphics.DrawRectangle(pen,
                        xOffset - this.m_documentMargin.Left,
                        yOffset - this.m_documentMargin.Top,
                        this.m_lContentWidth + this.m_documentMargin.Horizontal,
                        this.m_lContentHeight - this.ColumnHeight + this.m_documentMargin.Vertical);
                }
            }

            int i = 0;
            foreach (DpRow line in this.Rows)
            {
                if (yOffset > pe.ClipRectangle.Bottom)
                    break;

                // DpRow line = this.Rows[i];

                // 每次一行
                RectangleF rect = new RectangleF(xOffset,
                    yOffset,
                    this.m_lContentWidth,
                    line.TextHeight + this.m_cellPadding.Vertical);
                if (rect.IntersectsWith(pe.ClipRectangle) == true)
                {
                    line.Paint(xOffset,
                        yOffset,
                        this.m_columns,
                        pe);
                }
                else
                {
                    // line.HideInnerControls();
                }
                yOffset += line.TextHeight + this.m_cellPadding.Vertical;

                // 行间距
                yOffset += this.m_nLineDistance;

                i++;
            }

#if NO
            // 将Client以外的行中的控件隐藏
            // this.ClientRectangle;

            // TODO: 需要一个标志来表明当前是否有InnerControl，以便提高速度
            for (; i < this.Rows.Count; i++)
            {
                DpRow line = this.Rows[i];
                line.HideInnerControls();
            }
#endif
        }



        // 点击测试
        // parameters:
        //      p_x 点击位置x。为屏幕坐标
        void HitTest(long p_x,
            long p_y,
            out HitTestResult result)
        {
            result = new HitTestResult();

            // 换算为整体文档(包含上下左右的空白区域)坐标
            long x = p_x - m_lWindowOrgX - this.m_nHorzDelta;
            long y = p_y - m_lWindowOrgY;

            if (p_y < this.ColumnHeight)
            {
                this.m_columns.HitTest(x, p_y, out result);
                return;
            }

            int nColumnHeight = this.ColumnHeight;

            if (y < nColumnHeight + this.Padding.Top)
                result.AreaPortion = AreaPortion.TopBlank;  // 上方空白
            else if (y > nColumnHeight + this.Padding.Top + this.m_lContentHeight)
                result.AreaPortion = AreaPortion.BottomBlank;  // 下方空白
            else if (x < this.Padding.Left)
                result.AreaPortion = AreaPortion.LeftBlank;  // 左方空白
            else if (x > this.Padding.Left + this.m_lContentWidth)
                result.AreaPortion = AreaPortion.RightBlank;  // 右方空白
            else
            {
                    result.AreaPortion = AreaPortion.Content;

                    long xOffset = this.Padding.Left;
                    long yOffset = this.Padding.Top
        + (this.ColumnHeight);

                    int i = 0;
                    foreach (DpRow line in this.Rows)
                    {
                        // DpRow line = this.Rows[i];

                        // 每次一行
                        RectangleF rect = new RectangleF(xOffset,
                            yOffset,
                            this.m_lContentWidth,
                            line.TextHeight + this.m_cellPadding.Vertical);

                        if (GuiUtil.PtInRect(x, y, rect) == true)
                        {
                            line.HitTest(x - xOffset,
                                y - yOffset,
                                out result);
                            return;
                        }

                        yOffset += line.TextHeight + this.m_cellPadding.Vertical;

                        // 行间距
                        yOffset += this.m_nLineDistance;

                        i++;
                    }

                return;
            }

        END1:
            result.X = x;
            result.Y = y;
            result.Object = null;
        }

        DpColumn DragCol = null;	// null表示当前没有按下鼠标左键。其余值，表示鼠标左键按下，
        int nLastTrackerX = -1;

        void DrawTraker()
        {
            Point p1 = new Point(nLastTrackerX, 0);
            p1 = this.PointToScreen(p1);

            Point p2 = new Point(nLastTrackerX, this.ClientSize.Height);
            p2 = this.PointToScreen(p2);

            ControlPaint.DrawReversibleLine(p1,
                p2,
                SystemColors.Control);
        }


        internal object m_lastHoverObj = null;

#if NO
        protected override void OnMouseHover(EventArgs e)
        {
            /*
            if (m_bRectSelecting == true)
                return;
             * */

            HitTestResult result = null;

            Point p = this.PointToClient(Control.MousePosition);

            // 屏幕坐标
            this.HitTest(
                p.X,
                p.Y,
                out result);
            if (result == null)
                goto END1;

            if (this.m_lastHoverObj != result.Object)
            {
                if (this.m_lastHoverObj != null)
                {
                    if (GetHoverValue(this.m_lastHoverObj) != false)
                    {
                        SetHoverValue(this.m_lastHoverObj, false);
                        InvalidateObject(this.m_lastHoverObj);
                    }
                }

                this.m_lastHoverObj = result.Object;

                if (this.m_lastHoverObj == null)
                    goto END1;

                if (GetHoverValue(this.m_lastHoverObj) != true)
                {
                    SetHoverValue(this.m_lastHoverObj, true);
                    InvalidateObject(this.m_lastHoverObj);
                }
            }
    END1:
            base.OnMouseHover(e);
        }
#endif

        static bool GetHoverValue(object o)
        {
            if (o is DpCell)
                return ((DpCell)o).m_bHover;
            else if (o is DpRow)
                return ((DpRow)o).m_bHover;
            else if (o is DpColumn)
                return ((DpColumn)o).m_bHover;
            else if (o is DpColumnRow)
                return ((DpColumnRow)o).m_bHover;
            return false;
        }

        static void SetHoverValue(object o, bool value)
        {
            if (o is DpCell)
                ((DpCell)o).m_bHover = value;
            else if (o is DpRow)
                ((DpRow)o).m_bHover = value;
            else if (o is DpColumn)
                ((DpColumn)o).m_bHover = value;
            else if (o is DpColumnRow)
                ((DpColumnRow)o).m_bHover = value;
        }

        // 鼠标滚轮
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            int numberOfTextLinesToMove = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
            int numberOfPixelsToMove = numberOfTextLinesToMove * 18;

            DocumentOrgY += numberOfPixelsToMove;

            // base.OnMouseWheel(e);
        }

        // 重载鼠标移动事件
        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (DragCol != null)
            {
                Cursor = Cursors.SizeWE;

                // 消上次残余的一根
                DrawTraker();

                nLastTrackerX = e.X;

                // 绘制本次的一根
                DrawTraker();
            }
            else if (this.Capture == false)
            {
                HitTestResult result = null;
                    // 屏幕坐标
                    this.HitTest(
                        e.X,
                        e.Y,
                        out result);

                // 栏目标题行
                if (result != null
                    && result.Object != null
                    && result.Object is DpColumn
                    && result.AreaPortion == AreaPortion.Splitter)
                    Cursor = Cursors.SizeWE;
                else
                    Cursor = Cursors.Arrow;

                object resultObject = null;
                if (result != null)
                    resultObject = result.Object;

                if (this.FullRowSelect == true)
                {
                    if (resultObject is DpCell)
                        resultObject = GetLineObj(resultObject);
                }

                if (this.m_lastHoverObj != resultObject)
                {
                    if (this.m_lastHoverObj != null)
                    {
                        if (GetHoverValue(this.m_lastHoverObj) != false)
                        {
                            SetHoverValue(this.m_lastHoverObj, false);
                            InvalidateObject(this.m_lastHoverObj);
                        }
                    }

                    this.m_lastHoverObj = resultObject;

                    if (this.m_lastHoverObj == null)
                        goto END1;

                    if (GetHoverValue(this.m_lastHoverObj) != true)
                    {
                        SetHoverValue(this.m_lastHoverObj, true);
                        InvalidateObject(this.m_lastHoverObj);
                    }
                }
            }
            END1:
            base.OnMouseMove(e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            if (this.m_lastHoverObj != null)
            {
                if (GetHoverValue(this.m_lastHoverObj) != false)
                {
                    SetHoverValue(this.m_lastHoverObj, false);
                    InvalidateObject(this.m_lastHoverObj);
                }
            }

            this.m_lastHoverObj = null;
            base.OnMouseLeave(e);
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
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
                    out result);
                if (result == null)
                    goto END1;

                // 栏目标题行
                if (result.Object != null
    && result.Object is DpColumn)
                {
                    DpColumn column = (DpColumn)result.Object;

                    if (result.AreaPortion == AreaPortion.Splitter)
                    {
                        DragCol = column;
                        // 第一次
                        nLastTrackerX = e.X;
                        DrawTraker();
                        goto END1;
                    }

                    goto END1;
                }

                if (result.Object != null
                    && result.Object is DpRow
                    && ((DpRow)result.Object).IsSeperator == true)
                    goto END1;

                bool bFocusCleared = false;
                bool bNeedTriggerEvent = false; // 是否需要稍后触发Changed事件

                // 清除以前的选择
                if (bControl == false)   // 按下了CONTROL，不清除以前的
                {
                    if (bShift == false)
                    {
                        if (this.m_bFullRowSelect == true)
                            bFocusCleared = this.ClearAllSelections(GetLineObj(result.Object));
                        else
                            bFocusCleared = this.ClearAllSelections(GetCellObj(result.Object));
                    }
                    else
                    {
                        if (result.Object == null)
                            bFocusCleared = this.ClearAllSelections();
                    }

                    bNeedTriggerEvent = true;
                }

                if (result.Object != null)
                {
                    if (this.m_bFullRowSelect == true)
                    {
                        DpRow result_line = GetLineObj(result.Object);

                        if (bFocusCleared == false
                            && this.m_focusObj != null
                            && this.m_focusObj != result_line)
                        {
                            this.InvalidateObject(this.m_focusObj);
                        }

                        this.m_focusObj = result_line;
                        OnFocusObjChanged();

                        if (bShift == true)
                        {
                            // //
                            if (this.m_shiftStartObj == null)
                            {
                                // 将第一个已经选定的对象作为 shiftStartObject
                                if (this.SelectedRows.Count > 0)
                                    this.m_shiftStartObj = this.SelectedRows[0];
                            }

                            bool bChanged = SelectRange(this.m_shiftStartObj, result_line, true);
                            if (bChanged == true
                                || bNeedTriggerEvent == true)
                            {
                                TriggerSelectionChanged();
                                bNeedTriggerEvent = false;
                            }
                            goto END1;
                        }

                        this.m_shiftStartObj = result_line;


                        {
                            bool bRet = false;
                            if (bControl == true)
                                bRet = result_line.InternalSelect(SelectAction.Toggle);
                            else
                                bRet = result_line.InternalSelect(SelectAction.On);
                            if (bRet == true)
                            {
                                InvalidateLine(result_line);
                                ExpireSelectedLines();
                                TriggerSelectionChanged();
                                bNeedTriggerEvent = false;
                            }

                            if (EnsureVisible(result_line) == true)
                                this.Update();
                        }
                    }
                    else    // cell select
                    {
                        if (result.Object is DpRow)
                            goto END1;

                        DpCell result_cell = (DpCell)result.Object;

                        if (bFocusCleared == false
    && this.m_focusObj != null
    && this.m_focusObj != result_cell)
                        {
                            this.InvalidateObject(this.m_focusObj);
                        }

                        this.m_focusObj = result_cell;
                        OnFocusObjChanged();
                        this.m_shiftStartObj = result_cell;

                        bool bRet = false;
                        if (bControl == true)
                            bRet = result_cell.InternalSelect(SelectAction.Toggle);
                        else
                            bRet = result_cell.InternalSelect(SelectAction.On);
                        if (bRet == true)
                        {
                            this.InvalidateCell(result_cell);
                            ExpireSelectedXYs();
                            TriggerSelectionChanged();
                            bNeedTriggerEvent = false;
                        }

                        if (EnsureVisible(result_cell) == true)
                            this.Update();

                    }


                    // 防止最后遗漏触发Changed事件
                    if (bNeedTriggerEvent == true)
                    {
                        TriggerSelectionChanged();
                        bNeedTriggerEvent = false;
                    }
                }



                // this.Update();
            }

        END1:
            base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            this.Capture = false;

            if (DragCol != null)
            {
                // 消最后残余的一根
                DrawTraker();

                // 做改变列宽度
                int nNewWidth = (int)(nLastTrackerX - this.m_lWindowOrgX - this.m_nHorzDelta - this.Padding.Left - this.m_columns.GetLeftStart(DragCol));
                DragCol.Width = Math.Max(0, nNewWidth);
                // this.InvalidateColumns();

                nLastTrackerX = -1;
                DragCol = null;

                // 重新初始化全部行的文字高度
                //RefreshAllTextHeight();
                //AfterDocumentChanged(ScrollBarMember.Both);

                //this.Invalidate();
                // this.DocumentOrgX = this.DocumentOrgX;
                this.AutoDocCenter = this.AutoDocCenter;
                this.DocumentOrgY = this.DocumentOrgY;
            }

            base.OnMouseUp(e);

            /*
            if (e.Button == MouseButtons.Right)
            {
                PopupMenu(e.Location);
                return;
            }
            */
        }

        // 重新初始化所有行的文字高度，并更新m_lContentHeight
        internal void RefreshAllTextHeight()
        {
            Graphics g = Graphics.FromHwnd(this.Handle);
            long height = this.ColumnHeight;

            int i = 0;
            foreach (DpRow row in this.Rows)
            {
                row.TextHeight = row.GetTextHeight(g);
                height += (long)row.TextHeight + (long)this.m_cellPadding.Vertical;

                // 行间距
                if (i > 0)  // 少加一次
                    height += this.m_nLineDistance;

                i++;
            }

            this.m_lContentHeight = height;

            LayoutAllRows(false);
        }

        internal void LayoutAllRows(bool bMove)
        {
            long xOffset = m_lWindowOrgX + this.Padding.Left + this.m_nHorzDelta;
            long yOffset = m_lWindowOrgY + this.Padding.Top
                + (this.ColumnHeight);
            bool bOldDelay = this.m_bDelayUpdate;
            this.m_bDelayUpdate = true;

            // this.SuspendLayout();

            //this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            //this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;

            foreach (DpRow line in this.Rows)
            {
                // DpRow line = this.Rows[i];

                // 每次一行
                line.LayoutInnerControl(
                    xOffset,
                    yOffset,
                    this.Columns,
                    bMove);

                yOffset += line.TextHeight + this.m_cellPadding.Vertical;

                // 行间距
                yOffset += this.m_nLineDistance;
            }

            // this.ResumeLayout(false);
            this.m_bDelayUpdate = bOldDelay;
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            InvalidateCurrentSelections();
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            InvalidateCurrentSelections();
        }

        // 刷新选定了的对象
        void InvalidateCurrentSelections()
        {
            if (this.m_bDelayUpdate == true)
                return;

            Rectangle rect_client = this.ClientRectangle;

            long xOffset = m_lWindowOrgX + this.Padding.Left + this.m_nHorzDelta;   // bug 缺乏 + this.m_nHorzDelta 2013/11/26
            long yOffset = m_lWindowOrgY + this.Padding.Top
                + (this.ColumnHeight);

            foreach (DpRow line in this.Rows)
            {
                if (yOffset > rect_client.Bottom)
                    break;

                // DpRow line = this.Rows[i];

                // 每次一行
                RectangleF rect = new RectangleF(xOffset,
                    yOffset,
                    this.m_lContentWidth,
                    line.TextHeight + this.m_cellPadding.Vertical);
                if (rect.IntersectsWith(rect_client) == true)
                {
                    if (this.FullRowSelect == true)
                    {
                        if (line.Selected == true)
                        {
                            rect.Inflate(1, 1);
                            this.Invalidate(Rectangle.Ceiling(rect));
                        }
                    }
                    else
                    {
                        foreach (DpCell cell in line)
                        {
                            // DpCell cell = line[j];
                            if (cell.Selected == true)
                                this.InvalidateCell(cell);  // TODO: 可以优化
                        }
                    }
                }
                yOffset += line.TextHeight + this.m_cellPadding.Vertical;

                // 行间距
                yOffset += this.m_nLineDistance;
            }

        }

        public LinearGradientBrush GetHoverBrush(Rectangle rectBack)
        {
            LinearGradientBrush linGrBrush = new LinearGradientBrush(
new PointF(0, rectBack.Y - 1),
new PointF(0, rectBack.Bottom),
Color.FromArgb(0, this.m_hoverBackColor),
Color.FromArgb(100, this.m_hoverBackColor)
);
            linGrBrush.GammaCorrection = true;
            return linGrBrush;
        }

        static DpCell GetCellObj(object cell_or_line)
        {
            if (cell_or_line == null)
                return null;

            if (cell_or_line is DpRow)
                return null;    // 不需要DpLine对象
            else if (cell_or_line is DpCell)
                return (DpCell)cell_or_line;
            else
            {
                Debug.Assert(false, "");
                return null;
            }
        }

        static DpRow GetLineObj(object cell_or_line)
        {
            if (cell_or_line == null)
                return null;

            if (cell_or_line is DpRow)
                return (DpRow)cell_or_line;
            else if (cell_or_line is DpCell)
                return ((DpCell)cell_or_line).Container;
            else
            {
                Debug.Assert(false, "");
                return null;
            }
        }

        public void InvalidateObject(object cell_or_line)
        {
            if (this.m_bDelayUpdate == true)
                return;
            if (cell_or_line == null)
            {
                Debug.Assert(false, "");
                return;
            }
            if (cell_or_line is DpCell)
                this.InvalidateCell((DpCell)cell_or_line);
            else if (cell_or_line is DpRow)
                this.InvalidateLine((DpRow)cell_or_line);
            else if (cell_or_line is DpColumn)
                this.InvalidateColumnHeader((DpColumn)cell_or_line);
            else if (cell_or_line is DpColumnRow)
                this.InvalidateColumns();
            else
            {
                Debug.Assert(false, "");
            }
        }

        public void InvalidateLine(DpRow line)
        {
            if (this.m_bDelayUpdate == true)
                return;

            Rectangle rect = Rectangle.Ceiling(line.GetViewRect());
            rect.Inflate(1, 1);
            this.Invalidate(rect);
        }

        // parameters:
        //      bInvalidateUpperDistance    是否要失效上方的行间距部分
        public void InvalidateLineAndBlow(DpRow line,
            bool bInvalidateLeftRightBlank,
            bool bInvalidateUpperDistance = false)
        {
            if (this.m_bDelayUpdate == true)
                return;

            Rectangle rect = Rectangle.Ceiling(line.GetViewRect());
            if (bInvalidateLeftRightBlank == true)
            {
                rect.X -= this.Padding.Left + this.m_documentMargin.Left;
                rect.Width += this.Padding.Horizontal + this.m_documentMargin.Horizontal + 4*2;
            }

#if NO
            long yOffset = m_lWindowOrgY + this.Padding.Top
    + (this.ColumnHeight);
#endif

            rect.Height = (int)this.m_lContentHeight + this.m_documentMargin.Vertical + this.Padding.Vertical + 2000;  // + 4*2
            
            if (bInvalidateUpperDistance == true)
            {
                rect.Y -= this.m_nLineDistance;
                rect.Height += this.m_nLineDistance;
            } 
            
            rect.Inflate(1, 1);
            this.Invalidate(rect);
        }

        public void InvalidateColumns()
        {
            if (this.m_bDelayUpdate == true)
                return;
            Rectangle rect = new Rectangle(0, 0, (int)this.DocumentWidth, this.ColumnHeight);
            this.Invalidate(rect);
        }

        public void InvalidateColumnHeader(DpColumn column)
        {
            if (this.m_bDelayUpdate == true)
                return;
            Rectangle rect = Rectangle.Ceiling(column.GetViewRect());
            rect.Inflate(1, 1);
            this.Invalidate(rect);
        }

        public void InvalidateCell(DpCell cell)
        {
            if (this.m_bDelayUpdate == true)
                return;
            Rectangle rect = Rectangle.Ceiling(cell.GetViewRect());
            rect.Inflate(1, 1);
            this.Invalidate(rect);
        }

        public void InvalidateCellIcon(DpCell cell)
        {
            if (this.m_bDelayUpdate == true)
                return;
            Rectangle rect = Rectangle.Ceiling(cell.GetIconViewRect());

            rect.Inflate(1, 1);
            this.Invalidate(rect);
        }

        internal void TriggerSelectionChanged()
        {
            if (this.SelectionChanged != null)
                this.SelectionChanged(this, new EventArgs());
        }

        // 选定一个范围的对象
        // parameters:
        //      bClearRest  是否清除其余的选择
        // return:
        //      操作中是否修改过Selected状态
        public bool SelectRange(object start, 
            object end,
            bool bClearRest = false)
        {
            Debug.Assert(start != null, "");
            Debug.Assert(end != null, "");

            bool bChanged = false;

            if (this.m_bFullRowSelect == true)
            {
                if (!(start is DpRow))
                    start = GetLineObj(start); 
                
                if (!(end is DpRow))
                    end = GetLineObj(end);

                int nStart = this.Rows.IndexOf((DpRow)start);
                int nEnd = this.Rows.IndexOf((DpRow)end);
                if (nStart > nEnd)
                {
                    int nTemp = nEnd;
                    nEnd = nStart;
                    nStart = nTemp;
                }

                for (int i = nStart; i <= nEnd; i++)
                {
                    DpRow line = this.Rows[i];
                    if (line.InternalSelect(SelectAction.On) == true)
                    {
                        this.InvalidateLine(line);
                        bChanged = true;
                    }
                }

                if (bClearRest == true)
                {
                    for (int i = 0; i < nStart; i++)
                    {
                        DpRow line = this.Rows[i];
                        if (line.InternalSelect(SelectAction.Off) == true)
                        {
                            this.InvalidateLine(line);
                            bChanged = true;
                        }
                    }
                    for (int i = nEnd + 1; i < this.Rows.Count; i++)
                    {
                        DpRow line = this.Rows[i];
                        if (line.InternalSelect(SelectAction.Off) == true)
                        {
                            this.InvalidateLine(line);
                            bChanged = true;
                        }
                    }
                }

                if (bChanged == true)
                {
                    ExpireSelectedLines();
                    TriggerSelectionChanged();  // 2014/11/11

                }
            }

            return bChanged;
        }

        // 清除全部选择
        // 不触发Changed事件
        // return:
        //      是否顺带清除了Focus位置的对象Selected状态
        public bool ClearAllSelections(object exclude_obj = null)
        {
            bool bFoundExclude = false;
            bool bFocusCleared = false;
            if (this.m_bFullRowSelect == true)
            {
                bool bSkip = false;
                Rectangle rect = this.ClientRectangle;

                for (int i = 0; i < this.SelectedRowIndices.Count; i++)
                {
                    DpRow line = this.Rows[this.SelectedRowIndices[i]];

                    if (line == exclude_obj)
                    {
                        if (line.m_bSelected == true)
                            bFoundExclude = true;
                        continue;
                    }

                    if (line.m_bSelected == true)
                    {
                        line.m_bSelected = false;

                        if (bSkip == false)
                        {
                            RectangleF rectUpdate = line.GetViewRect(i);
                            if (rectUpdate.Top > rect.Bottom)
                                bSkip = true;
                            else
                            {
                                if (rectUpdate.Bottom >= rect.Top)
                                {
                                    if (this.m_bDelayUpdate == false)
                                    {
                                        Rectangle temp = Rectangle.Ceiling(rectUpdate);
                                        temp.Inflate(1, 1);
                                        this.Invalidate(temp);
                                    }

                                    if (line == this.m_focusObj)
                                        bFocusCleared = true;
                                }
                            }
                        }
                    }
                }   // end of for

                if (bFoundExclude == true)
                {
                    // 可能还剩下一个有选择状态的对象
                    this.m_seletecLines = null;
                    this.m_selectedLineIndices = null;
                }
                else
                {
                    this.m_seletecLines = new List<DpRow>();
                    this.m_selectedLineIndices = new List<int>();
                }
            }
            else // 格子方式
            {
                bool bSkip = false;
                Rectangle rect = this.ClientRectangle;

                for (int i = 0; i < this.SelectedXYs.Count; i++)
                {
                    CellXY cell_xy = this.SelectedXYs[i];

                    DpCell cell = this.Rows[cell_xy.Y][cell_xy.X];

                    if (cell == exclude_obj)
                    {
                        if (cell.Selected == true)
                            bFoundExclude = true;
                        continue;
                    }

                    if (cell.Selected == true)
                    {
                        cell.m_bSelected = false;   // 不触发事件

                        if (bSkip == false)
                        {
                            RectangleF rectUpdate = cell.GetViewRect();
                            if (rectUpdate.Top > rect.Bottom)
                                bSkip = true;
                            else
                            {
                                if (rectUpdate.Bottom >= rect.Top)
                                {
                                    if (this.m_bDelayUpdate == false)
                                    {
                                        Rectangle temp = Rectangle.Ceiling(rectUpdate);
                                        temp.Inflate(1, 1);
                                        this.Invalidate(temp);
                                    }

                                    if (cell == this.m_focusObj)
                                        bFocusCleared = true;
                                }
                            }
                        }
                    }
                }   // end of for

                if (bFoundExclude == true)
                {
                    // 可能还剩下一个有选择状态的对象
                    this.m_seletedCellXY = null;
                }
                else
                {
                    this.m_seletedCellXY = new List<CellXY>();
                }
            }


            return bFocusCleared;
        }



        public bool EnsureColumnWidths(DpRow line)
        {
            bool bChanged = false;
            while (this.m_columns.Count < line.Count)
            {
                this.m_columns.Add(new DpColumn());
                bChanged = true;
            }

            return bChanged;
        }

        internal long GetContentWidth()
        {
            long result = 0;
            foreach (DpColumn column in this.m_columns)
            {
                result += (long)column.m_nWidth;
            }

            return result;
        }

        // TODO: 栏标题高度改变，也会导致这个变量改变
        long GetContentHeight()
        {
            long result = this.ColumnHeight;
            int i = 0;
            foreach (DpRow line in this.Rows)
            {
                result += (long)line.TextHeight + (long)this.m_cellPadding.Vertical;

                // 行间距
                if (i > 0)  // 少加一次
                    result += this.m_nLineDistance;
                i++;
            }

            return result;
        }

        int m_nNestedSetScrollBars = 0;

        // 卷滚条比率 小于等于1.0F
        double m_v_ratio = 1.0F;
        double m_h_ratio = 1.0F;

        internal void SetScrollBars(ScrollBarMember member)
        {
            if (this.m_bDelayUpdate == true)
                return;

            m_nNestedSetScrollBars++;

            try
            {
                LayoutAllRows(true);

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
                }
            }
            finally
            {
                m_nNestedSetScrollBars--;
            }
        }

        // 检查一个long是否越过int16能表达的值范围
        public static bool TooLarge(long lValue)
        {
            if (lValue >= Int16.MaxValue || lValue <= Int16.MinValue)
                return true;
            return false;
        }

        public long DocumentWidth
        {
            get
            {
                return m_lContentWidth + (long)this.Padding.Horizontal;
            }

        }
        public long DocumentHeight
        {
            get
            {
                return m_lContentHeight + (long)this.Padding.Vertical;
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

                long lDelta = m_lWindowOrgX - lWindowOrgX_old;

                // AfterDocumentChanged(ScrollBarMember.Horz);
                if (lDelta != 0)    // 2015/6/10
                    SetScrollBars(ScrollBarMember.Horz);

                if (this.BackgroundImage != null)
                {
                    if (this.m_bDelayUpdate == false)
                    {
                        this.Invalidate();
                    }
                    return;
                }

                if (lDelta != 0)
                {
                    // 如果卷滚的距离超过32位整数范围
                    if (lDelta >= Int32.MaxValue || lDelta <= Int32.MinValue)
                    {
                        if (this.m_bDelayUpdate == false)
                        {
                            this.Invalidate();
                        }
                    }
                    else
                    {
                        RECT rect1 = new RECT();
                        rect1.left = 0;
                        rect1.top = 0;
                        rect1.right = this.ClientSize.Width;
                        rect1.bottom = this.ClientSize.Height;

                        if (rect1.left >= rect1.right)
                        {
                            if (this.m_bDelayUpdate == false)
                            {
                                this.Invalidate();
                            }
                            return;
                        }

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

                long lDelta = m_lWindowOrgY - lWindowOrgY_old;

                // AfterDocumentChanged(ScrollBarMember.Vert);
                if (lDelta != 0)    // 2015/6/10
                    SetScrollBars(ScrollBarMember.Vert);

                if (this.BackgroundImage != null)
                {
                    if (this.m_bDelayUpdate == false)
                    {
                        this.Invalidate();
                    }
                    return;
                }

                if (lDelta != 0)
                {
                    // 如果卷滚的距离超过32位整数范围
                    if (lDelta >= Int32.MaxValue || lDelta <= Int32.MinValue)
                    {
                        if (this.m_bDelayUpdate == false)
                        {
                            this.Invalidate();
                        }
                    }
                    else
                    {

                        RECT rect1 = new RECT();
                        rect1.left = 0;
                        if (lDelta < 0)
                            rect1.top = this.ColumnHeight + 1 - (int)lDelta;   // 0;
                        else
                            rect1.top = this.ColumnHeight + 1;

                        rect1.right = this.ClientSize.Width;
                        rect1.bottom = this.ClientSize.Height;

                        if (rect1.top >= rect1.bottom)
                        {
                            if (this.m_bDelayUpdate == false)
                            {
                                this.Invalidate();
                            }
                            return;
                        }


                        API.ScrollWindowEx(this.Handle,
                            0,
                            (int)lDelta,
                            ref rect1,
                            IntPtr.Zero,	//	ref RECT lprcClip,
                            0,	// int hrgnUpdate,
                            IntPtr.Zero,	// ref RECT lprcUpdate,
                            API.SW_INVALIDATE /*| API.SW_SCROLLCHILDREN*/ /*int fuScroll*/);

                        if (lDelta < 0)
                        {
                            if (this.m_bDelayUpdate == false)
                            {
                                int nNewBottomY = rect1.bottom + (int)lDelta;
                                Rectangle rectUpdate = new Rectangle(0,
                                    nNewBottomY,
                                    this.ClientSize.Width,
                                    this.ClientSize.Height - nNewBottomY);
                                this.Invalidate(rectUpdate);
                            }
                        }
                    }

                }

                // this.Invalidate();
            }
        }

        // 当文档尺寸和文档原点改变后，更新卷滚条等等设施状态，以便文档可见
        internal void AfterDocumentChanged(ScrollBarMember member)
        {
            if (member == ScrollBarMember.Both
                || member == ScrollBarMember.Horz)
                this.m_lContentWidth = this.GetContentWidth();

            if (member == ScrollBarMember.Both
               || member == ScrollBarMember.Vert)
                this.m_lContentHeight = this.GetContentHeight();   // 用整数，使为了提高速度。注意要及时修改

            SetScrollBars(member);
        }

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            // 首次显示前, OnSizeChanged()一次也没有被调用前, 显示好卷滚条
            SetScrollBars(ScrollBarMember.Both);
        }

        void SetAutoCenterValue()
        {
            this.m_nHorzDelta = (int)(((long)this.ClientSize.Width - this.DocumentWidth) / 2);
            if (this.m_nHorzDelta < 0)
                this.m_nHorzDelta = 0;
        }

        protected override void OnSizeChanged(System.EventArgs e)
        {
            int nOldDelta = this.m_nHorzDelta;
            if (this.m_bAutoDocCenter == true)
            {
                SetAutoCenterValue();
            }

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

            if (nOldDelta != this.m_nHorzDelta)
            {
                if (this.m_bDelayUpdate == false)
                {
                    this.Invalidate();
                }
            }

            base.OnSizeChanged(e);
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
                        int CellWidth = 100;
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
                                DocumentOrgY -= Math.Max(0, this.ClientSize.Height - this.ColumnHeight);
                                break;
                            case API.SB_PAGEUP:
                                DocumentOrgY += Math.Max(0, this.ClientSize.Height - this.ColumnHeight);
                                break;
                        }
                        // MessageBox.Show("this");

                        if (this.ScrollBarTouched != null)
                        {
                            ScrollBarTouchedArgs e1 = new ScrollBarTouchedArgs();
                            this.ScrollBarTouched(this, e1);
                        }
                    }
                    break;

                case API.WM_HSCROLL:
                    {

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
                                DocumentOrgX -= 20;
                                break;
                            case API.SB_LINEUP:
                                DocumentOrgX += 20;
                                break;
                            case API.SB_PAGEDOWN:
                                DocumentOrgX -= this.ClientSize.Width;
                                break;
                            case API.SB_PAGEUP:
                                DocumentOrgX += this.ClientSize.Width;
                                break;
                        }

                        if (this.ScrollBarTouched != null)
                        {
                            ScrollBarTouchedArgs e1 = new ScrollBarTouchedArgs();
                            this.ScrollBarTouched(this, e1);
                        }
                    }
                    break;

                default:
                    break;

            }

            base.DefWndProc(ref m);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                // 菜单键
                case Keys.Apps:
                    break;
                case Keys.Left:
                case Keys.Right:
                case Keys.Up:
                case Keys.Down:
                    {
                        DoArrowKey((Keys)e.KeyValue);
                    }
                    break;
                case Keys.PageDown:
                case Keys.PageUp:
                    break;
                case Keys.Space:
                    {
                        // 选定当前Focus对象
                        this.DoSpaceKey();
                    }
                    break;
            }

            base.OnKeyDown(e);
        }

        internal void ExpireSelectedXYs()
        {
            this.m_seletedCellXY = null;
        }

        void PrepareSelectedXYs()
        {
            if (this.m_seletedCellXY != null)
                return; // 优化

            if (this.m_seletedCellXY != null)
                this.m_seletedCellXY.Clear();
            else
                this.m_seletedCellXY = new List<CellXY>();

            int i = 0;
            foreach (DpRow line in this.Rows)
            {
                // DpRow line = this.Rows[i];
                int j = 0;
                foreach (DpCell cell in line)
                {
                    // DpCell cell = line[j];
                    if (cell.Selected == true)
                    {
                        this.m_seletedCellXY.Add(new CellXY(j, i));
                        if (this.m_bSingleSelection == true)
                            break;
                    }
                    j++;
                }

                i++;
            }
        }

        public List<CellXY> SelectedXYs
        {
            get
            {
                PrepareSelectedXYs();

                return this.m_seletedCellXY;
            }
        }

        internal void ExpireSelectedLines()
        {
            this.m_seletecLines = null;
            this.m_selectedLineIndices = null;
        }

        void PrepareSelectedLines()
        {
            if (this.m_seletecLines != null
                && this.m_selectedLineIndices != null)
                return; // 优化

            if (this.m_seletecLines != null)
                this.m_seletecLines.Clear();
            else
                this.m_seletecLines = new List<DpRow>();

            if (this.m_selectedLineIndices != null)
                this.m_selectedLineIndices.Clear();
            else
                this.m_selectedLineIndices = new List<int>();

            int i = 0;
            foreach (DpRow line in this.Rows)
            {
                // DpRow line = this.Rows[i];
                if (line.m_bSelected == true)
                {
                    this.m_seletecLines.Add(line);
                    this.m_selectedLineIndices.Add(i);

                    if (this.m_bSingleSelection == true)
                        break;
                }

                i++;
            }
        }

        public List<DpRow> SelectedRows
        {
            get
            {
                PrepareSelectedLines();

                return this.m_seletecLines;
            }
        }

        public List<int> SelectedRowIndices
        {
            get
            {
                PrepareSelectedLines();

                return this.m_selectedLineIndices;
            }
        }

        void DoSpaceKey()
        {
            if (this.m_bFullRowSelect == true)
            {
                DpRow focus_line = GetLineObj(this.m_focusObj);
                if (focus_line.InternalSelect(SelectAction.Toggle) == true)
                {
                    InvalidateLine(focus_line);
                    TriggerSelectionChanged();  // 2014/11/11
                }
            }
        }

        void DoArrowKey(Keys key)
        {
            bool bControl = (Control.ModifierKeys == Keys.Control);
            bool bShift = (Control.ModifierKeys == Keys.Shift);

            bool bNeedTriggerEvent = false;

            if (this.m_bFullRowSelect == true)
            {
                if (key == Keys.Left)
                {
                    DocumentOrgX += 20;
                    return;
                }
                else if (key == Keys.Right)
                {
                    DocumentOrgX -= 20;
                    return;
                }

                int nOldIndex = -1;
                int index = -1;

                DpRow focus_line = GetLineObj(this.m_focusObj);

                if (focus_line == null)
                {
                    if (this.Rows.Count == 0)
                        return;
                    index = 0;
                    goto SELECT;
                }
                else
                {
                    index = this.Rows.IndexOf(focus_line);
                    if (index == -1)
                        index = 0;
                    else
                        nOldIndex = index;
                }

                if (key == Keys.Up
    && index == 0)
                {
                    // 已经在第一行，但是还想向上看
                    this.DocumentOrgY = 0;
                    return;
                }
                else if (key == Keys.Down
    && index == this.Rows.Count - 1)
                {
                    // 已经在最后一行，但是还想向下看
                    this.DocumentOrgY = - this.DocumentHeight;
                    return;
                }

            REDO:
                if (key == Keys.Up
                    && index > 0)
                {
                    index--;
                }
                else if (key == Keys.Down
                    && index < this.Rows.Count - 1)
                {
                    index++;
                }
                else
                {
                    return;
                }

            SELECT:
                if (nOldIndex != index)
                {
                    DpRow line = this.Rows[index];

                    // 跳过分隔行
                    if (line.IsSeperator == true)
                        goto REDO;

                    bool bFocusCleared = false;

                    if (bControl == false && bShift == false)
                    {
                        bFocusCleared = this.ClearAllSelections(line);
                        bNeedTriggerEvent = true;
                    }

                    if (bControl == true || bShift == true)
                    {
                        // 按住Control的情况下，仅仅移动Focus。不变动以前的选择
                        if (this.m_focusObj != null
&& this.m_focusObj != line)
                        {
                            this.InvalidateObject(this.m_focusObj);
                        }
                        this.m_focusObj = line;
                        OnFocusObjChanged();
                        this.InvalidateObject(this.m_focusObj);

                        if (EnsureVisible(line) == true)
                            this.Update();

                        if (bControl == true)
                            return;
                    }

                    if (bShift == true)
                    {
                        if (this.m_shiftStartObj == null)
                        {
                            
                            // 将第一个已经选定的对象作为 shiftStartObject
                            if (this.m_bFullRowSelect == true)
                            {
                                if (this.SelectedRows.Count > 0)
                                    this.m_shiftStartObj = this.SelectedRows[0];
                            }
                            else
                            {
                                // TODO:
                            }
                        }

                        bool bSelectionChanged = SelectRange(this.m_shiftStartObj, line, true);
                        if (bSelectionChanged == true
                            || bNeedTriggerEvent == true)
                            TriggerSelectionChanged();
                        return;
                    }

                    // 没有Control和Shift按下的情况
                    bool bChanged = false;

                    bChanged = line.InternalSelect(SelectAction.On);
                    if (bChanged == true)
                    {
                        InvalidateLine(line);
                        ExpireSelectedLines();
                        bNeedTriggerEvent = true;
                    }

                    if (bFocusCleared == false
    && this.m_focusObj != null
    && this.m_focusObj != line)
                    {
                        this.InvalidateObject(this.m_focusObj);
                    }

                    this.m_focusObj = line;
                    OnFocusObjChanged();
                    this.m_shiftStartObj = line;

                    if (bNeedTriggerEvent == true)
                        TriggerSelectionChanged();

                    if (EnsureVisible(line) == true)
                        this.Update();
                }
                return;
            }

            // 格子
            {
                int x = -1;
                int y = -1;
                DpCell old_focus_cell = GetCellObj(this.m_focusObj);

                if (old_focus_cell == null)
                {
                    x = 0;
                    y = 0;
                    goto SKIP1;
                }
                else
                {
                    GetCellXY(old_focus_cell,
                    out x,
                    out y);
                    if (x == -1)
                        x = 0;
                    if (y == -1)
                        y = 0;
                }

                if (key == Keys.Up
&& y == 0)
                {
                    // 已经在第一行，但是还想向上看
                    this.DocumentOrgY = 0;
                    return;
                }
                else if (key == Keys.Down
    && y == this.Rows.Count - 1)
                {
                    // 已经在最后一行，但是还想向下看
                    this.DocumentOrgY = -this.DocumentHeight;
                    return;
                }

            REDO:
                bool bMoved = false;
                if (key == Keys.Left && x > 0)
                {
                    x--;
                    bMoved = true;
                }
                else if (key == Keys.Right && x < this.m_columns.Count - 1)
                {
                    x++;
                    bMoved = true;
                }

                if (key == Keys.Up && y > 0)
                {
                    y--;
                    bMoved = true;
                }
                else if (key == Keys.Down && y < this.Rows.Count - 1)
                {
                    y++;
                    bMoved = true;
                }

                if (bMoved == false)
                    return;

            SKIP1:
                DpRow line = this.Rows[y];
            if (line.IsSeperator == true)
                goto REDO;

                DpCell new_focus_cell = line[x];

                bool bFocusCleared = false;

                // 清除旧位置对象的选择标记
                if (bControl == false && bShift == false)
                {
                    bFocusCleared = this.ClearAllSelections(new_focus_cell);
                    bNeedTriggerEvent = true;
                }

                if (old_focus_cell != new_focus_cell
                    && old_focus_cell != null)
                {
                    InvalidateCell(old_focus_cell);
                }

                this.m_focusObj = new_focus_cell;
                OnFocusObjChanged();
                this.m_shiftStartObj = new_focus_cell;
                InvalidateCell(new_focus_cell);

                // 选定新位置对象
                if (bShift == false && bControl == false)
                {
                    bool bChanged = false;
                    bChanged = new_focus_cell.InternalSelect(SelectAction.On);
                    if (bChanged == true)
                    {
                        // InvalidateLine(new_focus_cell);
                        ExpireSelectedXYs();
                        bNeedTriggerEvent = true;
                    }
                }

                if (bNeedTriggerEvent == true)
                    TriggerSelectionChanged();

                if (EnsureVisible(new_focus_cell) == true)
                    this.Update();
            }
        }

        void OnFocusObjChanged()
        {
            return;

            if (this.m_focusObj == null)
            {
                if (m_textbox != null
                    && m_textbox.Visible == true)
                    m_textbox.Visible = false;
            }
            else if (this.m_focusObj is DpCell)
            {
                DpCell cell = (DpCell)this.m_focusObj;
                SetEditControl(cell);
            }
        }

        internal TextBox m_textbox = null;
        void SetEditControl(DpCell cell)
        {
            if (m_textbox == null)
            {
                m_textbox = new TextBox();
                m_textbox.ImeMode = ImeMode.NoControl;    // off
                m_textbox.BorderStyle = BorderStyle.None;  // BorderStyle.FixedSingle;
                m_textbox.MaxLength = 0;
                m_textbox.Multiline = true;
                m_textbox.AutoSize = false;
                this.Controls.Add(m_textbox);
            }

            Rectangle rect = Rectangle.Ceiling(cell.GetViewRect());

            Size oldsize = m_textbox.Size;
            Size newsize = new System.Drawing.Size(
                rect.Width,
                rect.Height);

            Point loc = new System.Drawing.Point(rect.X, rect.Y);

            // 从小变大，先move然后改变size
            if (oldsize.Height < newsize.Height)
            {
                m_textbox.Location = loc;
                m_textbox.Size = newsize;
            }
            else
            {
                // 从大变小，先size然后改变move
                m_textbox.Size = newsize;
                m_textbox.Location = loc;
            }
        }

        void GetCellXY(DpCell cell,
            out int x,
            out int y)
        {
            x = -1;
            y = -1;

            y = this.Rows.IndexOf(cell.Container);
            if (y == -1)
                return;
            x = cell.Container.IndexOf(cell);
        }

        // 确保一个单元在窗口客户区可见
        // return:
        //      是否发生卷滚了
        public bool EnsureVisible(DpRow line)
        {
            RectangleF rectUpdate = line.GetViewRect();

            RectangleF rectCell = rectUpdate;

            RectangleF rectCaret = rectUpdate;

            return EnsureVisible(rectCell, rectCaret, ScrollBarMember.Vert);
        }

        // 确保一个单元在窗口客户区可见
        // return:
        //      是否发生卷滚了
        public bool EnsureVisible(DpCell cell)
        {
            RectangleF rectUpdate = cell.GetViewRect();

            RectangleF rectCell = rectUpdate;

            RectangleF rectCaret = rectUpdate;

            return EnsureVisible(rectCell, rectCaret);
        }

        // 确保一个区域在窗口客户区可见
        // parameters:
        //      rectCell    要关注的区域
        //      rectCaret   要关注的区域中，用于插入符（热点）的矩形。一般可以小于rectCell
        // return:
        //      是否发生卷滚了
        public bool EnsureVisible(RectangleF rectCell,
            RectangleF rectCaret,
            ScrollBarMember direction = ScrollBarMember.Both)
        {
            /*
            if (rectCaret == null)
                rectCaret = rectCell;
             * */
            bool bScrolled = false;

            if ((direction & ScrollBarMember.Vert) != 0)
            {
                long lDelta = (long)rectCell.Y;


                if (lDelta + rectCaret.Height >= this.ClientSize.Height)
                {
                    if (rectCaret.Height >= this.ClientSize.Height)
                        DocumentOrgY = DocumentOrgY - (lDelta + (long)rectCaret.Height) + ClientSize.Height + /*调整系数*/ ((long)rectCaret.Height / 2) - (this.ClientSize.Height / 2);
                    else
                        DocumentOrgY = DocumentOrgY - (lDelta + (long)rectCaret.Height) + ClientSize.Height;
                    bScrolled = true;
                }
                else if (lDelta < this.ColumnHeight)
                {
                    if (rectCaret.Height >= this.ClientSize.Height)
                        DocumentOrgY = DocumentOrgY - (lDelta - this.ColumnHeight) - /*调整系数*/ (((long)rectCaret.Height / 2) - (this.ClientSize.Height / 2));
                    else
                        DocumentOrgY = DocumentOrgY - (lDelta - this.ColumnHeight);
                    bScrolled = true;
                }
                else
                {
                    // y不需要卷滚
                }
            }

            if ((direction & ScrollBarMember.Horz) != 0)
            {
                ////
                // 水平方向
                long lDelta = 0;

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
            }

            if (bScrolled == true)
            {
                Point p = this.PointToClient(Control.MousePosition);
                MouseEventArgs e = new MouseEventArgs(System.Windows.Forms.MouseButtons.None,
                    0,
                    p.X,
                    p.Y,
                    0);

                OnMouseMove(e);
            }

            return bScrolled;
        }

        #region 实用函数

        // 获得列标题宽度字符串
        public static string GetColumnWidthListString(DpTable table)
        {
            string strResult = "";
            for (int i = 0; i < table.Columns.Count; i++)
            {
                DpColumn column = table.Columns[i];
                if (i != 0)
                    strResult += ",";
                strResult += column.Width.ToString();
            }

            return strResult;
        }

        // 获得列标题宽度字符串
        // 扩展功能版本。不包含右边连续的没有标题文字的栏
        public static string GetColumnWidthListStringExt(DpTable list)
        {
            string strResult = "";
            int nEndIndex = list.Columns.Count;
            for (int i = list.Columns.Count - 1; i >= 0; i--)
            {
                DpColumn header = list.Columns[i];
                if (String.IsNullOrEmpty(header.Text) == false)
                    break;
                nEndIndex = i;
            }
            for (int i = 0; i < nEndIndex; i++)
            {
                DpColumn header = list.Columns[i];
                if (i != 0)
                    strResult += ",";
                strResult += header.Width.ToString();
            }

            return strResult;
        }

        // 设置列标题的宽度
        // parameters:
        //      bExpandColumnCount  是否要扩展列标题到足够数目？
        public static void SetColumnHeaderWidth(DpTable list,
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

        // 确保列标题数量足够
        public static void EnsureColumns(DpTable listview,
            int nCount,
            int nInitialWidth)
        {
            if (listview.Columns.Count >= nCount)
                return;

            for (int i = listview.Columns.Count; i < nCount; i++)
            {
                string strText = "";
                // strText = Convert.ToString(i);

                DpColumn col = new DpColumn();
                col.Text = strText;
                col.Width = nInitialWidth;
                listview.Columns.Add(col);
            }
        }

        #endregion
    }

    // 一个标题栏单元
    public class DpColumn : Component
    {
        public DpColumnRow m_collection = null;

        internal bool m_bHover = false;

        internal StringAlignment m_alignment = StringAlignment.Near;
        [DescriptionAttribute("Alignment")]
        [DefaultValue(typeof(StringAlignment), "StringAlignment.Near")]
        public StringAlignment Alignment
        {
            get
            {
                return this.m_alignment;
            }
            set
            {
                this.m_alignment = value;

                if (this.m_collection != null
                    && this.m_collection.Control != null)
                {
                    // TODO: 精确失效一个列(包括内容行中的该列)
                    this.m_collection.Control.Invalidate();
                }
            }
        }

        internal StringAlignment m_lineAlignment = StringAlignment.Near;
        [DescriptionAttribute("LineAlignment")]
        [DefaultValue(typeof(StringAlignment), "StringAlignment.Near")]
        public StringAlignment LineAlignment
        {
            get
            {
                return this.m_lineAlignment;
            }
            set
            {
                this.m_lineAlignment = value;

                if (this.m_collection != null
                    && this.m_collection.Control != null)
                {
                    // TODO: 精确失效一个列(包括内容行中的该列)
                    this.m_collection.Control.Invalidate();
                }
            }
        }

        internal string m_strText = "";
        [DescriptionAttribute("Text")]
        [DefaultValue(typeof(string), "")]
        public string Text
        {
            get
            {
                return this.m_strText;
            }
            set
            {
                this.m_strText = value;

                if (this.m_collection != null)
                {
                    // 重新初始化文字高度
                    this.m_collection.InitialTextHeight();
                }
            }
        }

        internal int m_nWidth = 100;
        [DescriptionAttribute("Width")]
        [DefaultValue(typeof(int), "100")]
        public int Width
        {
            get
            {
                return this.m_nWidth;
            }
            set
            {
                this.m_nWidth = value;

                if (this.m_collection != null)
                {
                    // 刷新宽度，重新初始化文字高度
                    this.m_collection.OnChanged();
                }
            }
        }

        internal bool m_bVisible = true;
        [DescriptionAttribute("Visible")]
        [DefaultValue(typeof(bool), "true")]
        public bool Visible
        {
            get
            {
                return this.m_bVisible;
            }
            set
            {
                this.m_bVisible = value;

                if (this.m_collection != null)
                {
                    // 刷新宽度，重新初始化文字高度
                    this.m_collection.OnChanged();
                }
            }
        }


        Font m_font = null;

        [DescriptionAttribute("Font of Column")]
        [DefaultValue(typeof(Font), "null")]
        public Font Font
        {
            get
            {
                return this.m_font;
            }
            set
            {
                this.m_font = value;

                if (this.m_collection != null)
                {
                    // 重新初始化文字高度
                    this.m_collection.InitialTextHeight();
                }
            }
        }

        // 真正用于显示操作的字体
        internal Font DisplayFont
        {
            get
            {
                if (this.m_font != null)
                    return this.m_font;
                if (this.m_collection != null)
                    return this.m_collection.DisplayFont;
                return null;
            }
        }

        Color m_backColor = Color.Transparent;
        [DescriptionAttribute("BackColor of Column")]
        [DefaultValue(typeof(Color), "SystemColors.Transparent")]
        public Color BackColor
        {
            get
            {
                return this.m_backColor;
            }
            set
            {
                this.m_backColor = value;

                if (this.m_collection != null
                    && this.m_collection.Control != null)
                    this.m_collection.Control.InvalidateColumns();
            }
        }

        internal Color DisplayBackColor
        {
            get
            {
                if (this.m_backColor == Color.Transparent
                    && this.m_collection != null)
                    return this.m_collection.DisplayBackColor;
                return this.m_backColor;
            }
        }

        Color m_foreColor = Color.Transparent;
        [DescriptionAttribute("ForeColor of Column")]
        [DefaultValue(typeof(Color), "SystemColors.Transparent")]
        public Color ForeColor
        {
            get
            {
                return this.m_foreColor;
            }
            set
            {
                this.m_foreColor = value;

                if (this.m_collection != null
    && this.m_collection.Control != null)
                    this.m_collection.Control.InvalidateColumns();

            }
        }

        internal Color DisplayForeColor
        {
            get
            {
                if (this.m_foreColor == Color.Transparent
                    && this.m_collection != null)
                    return this.m_collection.DisplayForeColor;
                return this.m_foreColor;
            }
        }

        // internal const TextFormatFlags editflags = TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding | TextFormatFlags.ExpandTabs | TextFormatFlags.NoPrefix;  // | TextFormatFlags.RightToLeft;


        public void Paint(long x_offs,
            long y_offs,
            int nWidth,
            int nHeight,
            // Color textColor,
            PaintEventArgs pe)
        {
            if (nWidth == 0)
                return;

            if (this.m_collection == null)
                return;

            DpTable control = this.m_collection.Control;
            if (control == null)
                return;

            Padding cell_padding = control.m_columnCellPadding;

            // 如果需要绘制背景色
            if (this.m_backColor != Color.Transparent)
            {
                Rectangle rectBack = new Rectangle(
   (int)x_offs,
   (int)y_offs,
   nWidth,
   nHeight);
                pe.Graphics.FillRectangle(new SolidBrush(this.m_backColor), rectBack);
            }

            // 叠加
            if (this.m_bHover == true)
            {
                Rectangle rectBack = new Rectangle(
(int)x_offs,
(int)y_offs,
nWidth,
nHeight);
                pe.Graphics.FillRectangle(control.GetHoverBrush(rectBack), rectBack);
            }


            Rectangle textRect = new Rectangle(
    (int)x_offs + cell_padding.Left,
    (int)y_offs + cell_padding.Top,
    nWidth - cell_padding.Horizontal,
    nHeight - cell_padding.Vertical);

            StringFormat format = new StringFormat();   //  (StringFormat)StringFormat.GenericTypographic.Clone();
            format.FormatFlags |= StringFormatFlags.FitBlackBox;
            format.Alignment = this.Alignment;
            format.LineAlignment = this.LineAlignment;

            pe.Graphics.DrawString(
                this.m_strText,
                this.DisplayFont,
                new SolidBrush(this.DisplayForeColor),
                textRect,
                format);

            // 竖线
            // TODO: 颜色做成可定义的
            pe.Graphics.DrawLine(new Pen(SystemColors.ControlDark, (float)1),
    x_offs + nWidth - 1, y_offs,
    x_offs + nWidth - 1, y_offs + nHeight);

        }

        public int GetTextHeight(Graphics g,
            int nWidth)
        {
            if (nWidth == 0)
                return 0;

            Padding cell_padding = this.m_collection.Control.m_columnCellPadding;
            StringFormat format = new StringFormat();   //  (StringFormat)StringFormat.GenericTypographic.Clone();
            format.FormatFlags |= StringFormatFlags.FitBlackBox;
            SizeF size = g.MeasureString(this.m_strText,
                this.DisplayFont,
                nWidth - cell_padding.Horizontal,
                format);

            return (int)size.Height;
        }

        // 得到一个对象的矩形(view坐标)
        public RectangleF GetViewRect(int nWidth = -1,
            int nHeight = -1,
            int index = -1)
        {
            DpTable control = this.m_collection.Control;
            Padding padding = this.m_collection.Control.Padding;
            Padding cell_padding = this.m_collection.Control.m_columnCellPadding;

            if (index == -1)
            {
                index = this.m_collection.IndexOf(this);

                if (index == -1)
                    return new RectangleF(0, 0, 0, 0); // 说明对象已经不在挂接状态

                Debug.Assert(index != -1, "");
            }

            if (nWidth == -1)
                nWidth = control.m_columns[index].m_nWidth;

            if (nHeight == -1)
                nHeight = control.ColumnHeight;

            RectangleF rect = new RectangleF(0,
                0,
                nWidth,
                nHeight);

            // 变换为整体文档坐标，然后变换为view坐标
            rect.Offset(control.m_lWindowOrgX + padding.Left + control.m_nHorzDelta + this.m_collection.GetLeftStart(this),
                0);

            return rect;
        }
    }

    // 标题行
    public class DpColumnRow : List<DpColumn>
    {
        public DpTable Control = null;

        bool _visible = true;
        public bool Visible
        {
            get
            {
                return this._visible;
            }
            set
            {
                if (this._visible != value)
                {
                    this._visible = value;

                    OnChanged(false);
                }

            }
        }

        public int TextHeight = 0;
        internal bool m_bHover = false;

        internal int m_nMaxTextHeight = 100;
        [DescriptionAttribute("Max Text Height of columns")]
        [DefaultValue(typeof(int), "100")]
        public int MaxTextHeight
        {
            get
            {
                return this.m_nMaxTextHeight;
            }
            set
            {
                this.m_nMaxTextHeight = value;

                OnChanged(false);
            }
        }

        Font m_font = null;

        [DescriptionAttribute("Font of Columns")]
        [DefaultValue(typeof(Font), "null")]
        public Font Font
        {
            get
            {
                return this.m_font;
            }
            set
            {
                this.m_font = value;

                OnChanged(false);
            }
        }

        internal Font DisplayFont
        {
            get
            {
                if (this.m_font != null)
                    return this.m_font;

                return this.Control.Font;
            }
        }

        Color m_backColor = Color.Transparent;
        [DescriptionAttribute("BackColor of Columns")]
        [DefaultValue(typeof(Color), "SystemColors.Transparent")]
        public Color BackColor
        {
            get
            {
                return this.m_backColor;
            }
            set
            {
                this.m_backColor = value;

                if (this.Control != null)
                    this.Control.Invalidate();
            }
        }

        internal Color DisplayBackColor
        {
            get
            {
                if (this.m_backColor == Color.Transparent)
                    return this.Control.m_columnsBackColor;
                return this.m_backColor;
            }
        }

        Color m_foreColor = Color.Transparent;
        [DescriptionAttribute("ForeColor of Columns")]
        [DefaultValue(typeof(Color), "SystemColors.Transparent")]
        public Color ForeColor
        {
            get
            {
                return this.m_foreColor;
            }
            set
            {
                this.m_foreColor = value;

                if (this.Control != null)
                    this.Control.Invalidate();

            }
        }

        internal Color DisplayForeColor
        {
            get
            {
                if (this.m_foreColor == Color.Transparent)
                    return this.Control.m_columnsForeColor;
                return this.m_foreColor;
            }
        }

        public void InitialTextHeight(Graphics g = null)
        {
            this.TextHeight = GetTextHeight(g);
        }

        public int GetTextHeight(Graphics g = null)
        {
            if (g == null)
            {
                if (this.Control == null)
                    return 0;
                g = Graphics.FromHwnd(this.Control.Handle);
            }

            int nHeight = 0;
            for (int i = 0; i < this.Count; i++)
            {
                DpColumn cell = this[i];
                int nWidth = cell.m_nWidth;
                if (nWidth <= 0 || cell.Visible == false)
                    continue;
                int nTempHeight = cell.GetTextHeight(g, nWidth);
                if (nTempHeight > nHeight)
                    nHeight = nTempHeight;
            }

            if (this.m_nMaxTextHeight == -1)
                return nHeight;

            return Math.Min(this.m_nMaxTextHeight, nHeight);
        }

        // 获得左边的开始位置
        // 不包含Padding.Left
        public int GetLeftStart(DpColumn column)
        {
            int result = 0; //  this.Control.Padding.Left;
            foreach (DpColumn cur_column in this)
            {
                if (cur_column == column)
                    break;
                result += cur_column.m_nWidth;
            }

            return result;
        }

        // parameters:
        //      x_offs  从0开始。注意绘制的时候要考虑Control.Padding/this.m_nHorzDelta在左右方向上起作用
        public void Paint(long x_offs,
            long y_offs,
            PaintEventArgs pe)
        {
            if (this.Control == null)
                return;

            Padding cell_padding = this.Control.m_columnCellPadding;
            Padding padding = this.Control.Padding;

            int nColumnHeight = this.Control.ColumnHeight;
            // 背景色
            if (this.DisplayBackColor != Color.Transparent)
            {
                RectangleF rectBack = new RectangleF(x_offs,
    y_offs,
    this.Control.DocumentWidth + padding.Horizontal + this.Control.m_nHorzDelta * 2,
    nColumnHeight);
                Color textColor = this.Control.ForeColor;

                pe.Graphics.FillRectangle(new SolidBrush(this.DisplayBackColor), rectBack);
            }

            x_offs += padding.Left + this.Control.m_nHorzDelta;
            for (int i = 0; i < this.Count; i++)
            {
                DpColumn cell = this[i];

                RectangleF rect = new RectangleF(0 + x_offs,
    y_offs,
    cell.m_nWidth,
    this.TextHeight + cell_padding.Vertical);
                if (rect.IntersectsWith(pe.ClipRectangle) == true)
                {
                    cell.Paint(x_offs,
                        y_offs,
                        cell.m_nWidth,
                        nColumnHeight,
                        // this.Control.m_columnsForeColor,
                        pe);
                }

                x_offs += cell.m_nWidth;
            }
        }

        #region 改写 List 的成员

        public new DpColumn this[int index]
        {
            get
            {
                return base[index];
            }
            set
            {
                base[index] = value;
                value.m_collection = this;

                OnChanged();
            }
        }

        public new void Add(DpColumn cell)
        {
            cell.m_collection = this;
            base.Add(cell);

            OnChanged();
        }

        public new void AddRange(IEnumerable<DpColumn> collection)
        {
            int nStart = this.Count;
            base.AddRange(collection);
            for (int i = nStart; i < this.Count; i++)
            {
                DpColumn column = this[i];
                column.m_collection = this;
            }

            OnChanged();
        }

        public new void Insert(int index, DpColumn column)
        {
            column.m_collection = this;
            base.Insert(index, column);
            OnChanged();
        }

        public new void InsertRange(int index, IEnumerable<DpColumn> collection)
        {
            base.InsertRange(index, collection);
            for (int i = index; i < index + collection.Count<DpColumn>(); i++)
            {
                DpColumn column = this[i];
                column.m_collection = this;
            }

            OnChanged();
        }

        public new bool Remove(DpColumn item)
        {
            bool bRet = base.Remove(item);
            if (bRet == true)
                OnChanged();

            return bRet;
        }

        public new int RemoveAll(Predicate<DpColumn> match)
        {
            int nRet = base.RemoveAll(match);
            if (nRet > 0)
                OnChanged();

            return nRet;
        }

        public new void RemoveAt(int index)
        {
            base.RemoveAt(index);
                OnChanged();
        }

        public new void RemoveRange(int index, int count)
        {
            base.RemoveRange(index, count);
            OnChanged();
        }

        public new void Reverse()
        {
            // TODO: 内容列是否要调转?
            base.Reverse();
            OnChanged();
        }

        public new void Reverse(int index, int count)
        {
            base.Reverse(index, count);
            OnChanged();
        }

        public new void Sort()
        {
            base.Sort();
            if (this.Control != null)
                this.Control.Invalidate();
        }

        public new void Sort(Comparison<DpColumn> comparison)
        {
            base.Sort(comparison);
            if (this.Control != null)
                this.Control.Invalidate();
        }

        public new void Sort(IComparer<DpColumn> comparer)
        {
            base.Sort(comparer);
            if (this.Control != null)
                this.Control.Invalidate();
        }

        public new void Sort(int index, int count, IComparer<DpColumn> comparer)
        {
            base.Sort(index, count, comparer);
            if (this.Control != null)
                this.Control.Invalidate();
        }

        public new void Clear()
        {
            base.Clear();
            OnChanged();
        }

        #endregion

        internal void OnChanged(bool bRefreshAllLineHeight = true)
        {
            ScrollBarMember scrollbar = ScrollBarMember.Horz;

            Graphics g = Graphics.FromHwnd(this.Control.Handle);

            // 初始化高度
            int nOldTextHeight = this.TextHeight;
            this.TextHeight = this.GetTextHeight(g);
            if (nOldTextHeight != this.TextHeight)
            {
                this.Control.m_lContentHeight += this.TextHeight - nOldTextHeight;   // 优化
                scrollbar |= ScrollBarMember.Vert;
            }

            // 初始化宽度
            long lOldContentWidth = this.Control.m_lContentWidth;
            this.Control.m_lContentWidth = this.Control.GetContentWidth();
            if (lOldContentWidth != this.Control.m_lContentWidth)
            {
                scrollbar |= ScrollBarMember.Horz;
            }

            if (bRefreshAllLineHeight == true)
            {
                long lOldContentHeight = this.Control.m_lContentHeight;
                this.Control.RefreshAllTextHeight();
                if (lOldContentHeight != this.Control.m_lContentHeight)
                {
                    scrollbar |= ScrollBarMember.Vert;
                }
            }

            this.Control.SetScrollBars(scrollbar);

            // 布局子控件位置

            // TODO: 精确失效新增的列
            this.Control.Invalidate();
        }

        // 找到连续的0宽度列标题的最后一个
        int FindLastZeroWidthColumn(int nStart)
        {
            for (int i = nStart + 1; i < this.Count; i++)
            {
                DpColumn column = this[i];
                if (column.Width != 0)
                    return i - 1;
            }

            return nStart;
        }

        // 栏目标题 点击测试
        // parameters:
        //      p_x 点击位置x。为行内坐标
        //      注意要考虑Control.Padding在左右方向上起作用
        public void HitTest(long p_x,
            long p_y,
            out HitTestResult result)
        {
            result = new HitTestResult();

            Padding cell_padding = this.Control.m_columnCellPadding;
            Padding padding = this.Control.Padding;

            if (p_x < padding.Left)
            {
                result.AreaPortion = AreaPortion.LeftBlank;
                if (this.Count > 0)
                {
                    result.Object = this[0];
                    result.X = p_x - padding.Left;
                }
                else
                {
                    result.Object = this;
                    result.X = p_x;
                }

                result.Y = p_y;
                return;
            }

            long x = padding.Left;
            for (int i = 0; i < this.Count; i++)
            {
                DpColumn cell = this[i];
                int nWidth = cell.m_nWidth;

                if (nWidth == 0)
                {
                    i = FindLastZeroWidthColumn(i);
                    cell = this[i];
                    nWidth = cell.m_nWidth;
                    // x 中间不用再累加
                }

                if (nWidth == 0
                    && p_x > x
                    && p_x <= x + 4)
                {
                    result.AreaPortion = AreaPortion.Splitter;
                    result.Object = cell;
                    result.X = p_x - x;
                    result.Y = p_y;
                    return;
                }

                RectangleF rect = new RectangleF(x,
    0,
    nWidth,
    this.TextHeight + cell_padding.Vertical);

                if (GuiUtil.PtInRect(p_x, p_y, rect) == true)
                {
                    result.AreaPortion = AreaPortion.Content;
                    result.Object = cell;
                    result.X = p_x - x;
                    result.Y = p_y;
                    if (result.X >= nWidth - 4)
                        result.AreaPortion = AreaPortion.Splitter;
                    else if (result.X <= 4
                        && i > 0)
                    {
                        if (nWidth == 0)
                        {
                            result.AreaPortion = AreaPortion.Splitter;
                            return;
                        }

                        // 算作左方一个Column的Splitter
                        result.AreaPortion = AreaPortion.Splitter;
                        result.Object = this[i - 1];
                        result.X += nWidth;
                    }
                    return;
                }

                x += nWidth;
            }

            result.AreaPortion = AreaPortion.RightBlank;
            if (this.Count > 0)
            {
                result.Object = this[this.Count - 1];
                result.X = p_x - x - this[this.Count - 1].m_nWidth;
            }
            else
            {
                result.Object = this;
                result.X = p_x;
            }
            result.Y = p_y;
        }
    }

    public enum DpTextAlignment
    {
        InheritLine = 0,
        InheritColumn = 1,
        Near = 2,
        Center = 3,
        Far = 4,
    }

    public enum DpCellStyle
    {
        None = 0,
        Editable = 1,  // 可编辑
    }

    // 一个显示单元
    public class DpCell
    {
        public object Tag = null;   // 存储附加的数据

        public bool OwnerDraw = false;  // 是否要定制绘制


        public DpRow Container = null;

        internal int TextHeight = 0;
        internal int ExtHeight = 0; //  扩展部分的高度。TextHeight 包含了 ExtHeight 在内
        internal bool m_bHover = false;

        public DpCellStyle Style = DpCellStyle.None;

        int m_nImageIndex = -1;
        public int ImageIndex
        {
            get
            {
                return this.m_nImageIndex;
            }
            set
            {
                this.m_nImageIndex = value;
                // invalidate Icon 区域
                if (this.Container != null
                    && this.Container.Control != null
                    && this.Container.Control.ImageList != null)
                {
                    // 重新初始化行的高度
                    if (this.Container.UpdateLineHeight() == false) // == false表示没有Invalid当前行
                        this.Container.Control.InvalidateCell(this);
                    else
                        this.Container.Control.InvalidateCellIcon(this);
                }
            }
        }

        Image _image = null;
        public Image Image
        {
            get
            {
                return this._image;
            }
            set
            {
                this._image = value;

                // invalidate Icon 区域
                if (this.Container != null
                    && this.Container.Control != null)
                {
                    // 重新初始化行的高度
                    if (this.Container.UpdateLineHeight() == false) // == false表示没有Invalid当前行
                        this.Container.Control.InvalidateCell(this);
                    else
                        this.Container.Control.InvalidateCellIcon(this);
                }
            }
        }

        internal string m_strText = "";

        public string Text
        {
            get
            {
                return this.m_strText;
            }
            set
            {
                // 提供给外部调用
                // if (value != this.m_strText)
                {
                    this.m_strText = value;

                    if (this.Container != null
                        && this.Container.Control != null)
                    {
                        // 重新初始化行的高度
                        if (this.Container.UpdateLineHeight() == false) // == false表示没有Invalid当前行
                            this.Container.Control.InvalidateCell(this);
                    }
                }
            }
        }

        internal Control m_innerControl = null;

        public Control InnerControl
        {
            get
            {
                return this.m_innerControl;
            }
            set
            {
                // 提供给外部调用
                if (value == null)
                {
                    if (this.m_innerControl != null
                        && this.Container != null
                        && this.Container.Control != null)
                        this.Container.Control.Controls.Remove(m_innerControl);
                }

                if (value != this.m_innerControl)
                {
                    this.m_innerControl = value;

                    if (value != null
                        && this.Container != null
                        && this.Container.Control != null)
                        this.Container.Control.Controls.Add(value);

                    if (this.Container != null
                        && this.Container.Control != null)
                    {
                        // 重新初始化行的高度
                        if (this.Container.UpdateLineHeight() == false) // == false表示没有Invalid当前行
                            this.Container.Control.InvalidateCell(this);

                        // 首次设置宽度
                        if (this.m_innerControl != null)
                            this.m_innerControl.Width = this.GetWidth();
                    }
                }


            }
        }

        Font m_font = null;

        internal bool m_bSelected = false;
        public bool Selected
        {
            get
            {
                return this.m_bSelected;
            }
            set
            {
                // 提供给外部调用
                if (value != this.m_bSelected)
                {
                    this.m_bSelected = value;

                    if (this.Container != null
                        && this.Container.Control != null)
                    {
                        // 迫使将来重新获取
                        this.Container.Control.m_seletedCellXY = null;

                        this.Container.Control.InvalidateCell(this);

                        this.Container.Control.TriggerSelectionChanged();
                    }
               }
            }
        }

        internal DpTextAlignment m_alignment = DpTextAlignment.InheritColumn;
        public DpTextAlignment Alignment
        {
            get
            {
                return this.m_alignment;
            }
            set
            {
                this.m_alignment = value;

                if (this.Container != null
                    && this.Container.Control != null)
                {
                    this.Container.Control.InvalidateCell(this);
                }
            }
        }

        static StringAlignment GetAlignment(DpTextAlignment alignment)
        {
            if (alignment == DpTextAlignment.Near)
                return StringAlignment.Near;
            else if (alignment == DpTextAlignment.Far)
                return StringAlignment.Far;
            else if (alignment == DpTextAlignment.Center)
                return StringAlignment.Center;

            throw new Exception("GetAlignment() 无法处理两个Inherit值");
        }

        internal StringAlignment DisplayAlignment
        {
            get
            {
                if (this.m_alignment == DpTextAlignment.InheritColumn)
                {
                    if (this.Container == null
                        || this.Container.Control == null)
                        return StringAlignment.Near;

                    int index = this.Container.IndexOf(this);
                    if (index == -1)
                        return StringAlignment.Near;
                    DpColumn column = this.Container.Control.Columns[index];
                    return column.Alignment;
                }
                if (this.m_alignment == DpTextAlignment.InheritLine)
                {
                    if (this.Container == null)
                        return StringAlignment.Near;

                    return this.Container.Alignment;
                }
                return GetAlignment(this.m_alignment);
            }
        }

        internal DpTextAlignment m_lineAlignment = DpTextAlignment.InheritLine; // 默认继承行的 LineAlignment
        public DpTextAlignment LineAlignment
        {
            get
            {
                return this.m_lineAlignment;
            }
            set
            {
                this.m_lineAlignment = value;

                if (this.Container != null
                    && this.Container.Control != null)
                {
                    this.Container.Control.InvalidateCell(this);
                }
            }
        }

        internal StringAlignment DisplayLineAlignment
        {
            get
            {
                if (this.m_lineAlignment == DpTextAlignment.InheritColumn)
                {
                    if (this.Container == null
                        || this.Container.Control == null)
                        return StringAlignment.Near;

                    int index = this.Container.IndexOf(this);
                    if (index == -1)
                        return StringAlignment.Near;
                    DpColumn column = this.Container.Control.Columns[index];
                    return column.LineAlignment;
                }
                if (this.m_lineAlignment == DpTextAlignment.InheritLine)
                {
                    if (this.Container == null)
                        return StringAlignment.Near;

                    return this.Container.LineAlignment;
                }
                return GetAlignment(this.m_lineAlignment);
            }
        }

        public Font Font
        {
            get
            {
                return this.Container.Font;
            }
            set
            {
#if NO
                if (this.m_font != null
                    && value != null
                    && value != this.m_font)
                {
                    FontStyle oldStyle = this.m_font.Style;
                    m_font = new Font(this.m_font.FontFamily, value.SizeInPoints,
                        oldStyle, GraphicsUnit.Point);
                }
#endif

                this.m_font = value;
                if (this.Container != null
                    && this.Container.Control != null)
                {
                    // 重新初始化行的高度
                    if (this.Container.UpdateLineHeight() == false) // == false表示没有Invalid当前行
                        this.Container.Control.InvalidateCell(this);
                }
            }
        }

        public Font DisplayFont
        {
            get
            {
                if (this.m_font != null)
                    return this.m_font;

                return this.Container.DisplayFont;
            }
        }

        Color m_backColor = Color.Transparent;
        public Color BackColor
        {
            get
            {
                return this.m_backColor;
            }
            set
            {
                this.m_backColor = value;
                if (this.Container != null 
                    && this.Container.Control != null)
                    this.Container.Control.InvalidateCell(this);
            }
        }

        public Color DisplayBackColor
        {
            get
            {
                if (this.m_backColor == Color.Transparent)
                    return this.Container.Control.BackColor;
                return this.m_backColor;
            }
        }

        Color m_foreColor = Color.Transparent;
        public Color ForeColor
        {
            get
            {
                return this.m_foreColor;
            }
            set
            {
                this.m_foreColor = value;
                if (this.Container != null
                    && this.Container.Control != null)
                    this.Container.Control.InvalidateCell(this);
            }
        }

        public Color DisplayForeColor
        {
            get
            {
                if (this.m_foreColor == Color.Transparent)
                {
                    return this.Container.ForeColor;
                }
                return this.m_foreColor;
            }
        }

        public DpCell(string strText)
        {
            this.m_strText = strText;
        }

        public DpCell()
        {
        }

        public void Relayout()
        {
            if (this.Container != null
                && this.Container.Control != null)
            {
                // 重新初始化行的高度
                if (this.Container.UpdateLineHeight() == false) // == false表示没有Invalid当前行
                    this.Container.Control.InvalidateCell(this);
            }
        }

        public void EnsureVisible()
        {
            if (this.Container != null
                && this.Container.Control != null)
                this.Container.Control.EnsureVisible(this);
        }

        // internal const TextFormatFlags editflags = TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.NoPadding | TextFormatFlags.ExpandTabs | TextFormatFlags.NoPrefix;  // | TextFormatFlags.RightToLeft;

        static void DrawClippedString(
            Graphics g,
            string strText,
            Font font,
            Color textColor,
            Rectangle textRect,
            int nNormalHeight,
            StringFormat format)
        {
            Region old_clip = g.Clip;

            Rectangle clip = textRect;
            clip.Height = nNormalHeight;

            g.IntersectClip(clip);

            // 普通色
            g.DrawString(
                strText,
                font,
                new SolidBrush(textColor),
                textRect,
                format);

            g.Clip = old_clip;

            nNormalHeight--;    // 防止白线出现
            LinearGradientBrush linGrBrush = new LinearGradientBrush(
new PointF(0, textRect.Y + nNormalHeight),
new PointF(0, textRect.Bottom),
Color.FromArgb(255, textColor),
Color.FromArgb(0, textColor)
);
            linGrBrush.GammaCorrection = true;

            clip = textRect;
            clip.Y += nNormalHeight;
            clip.Height = clip.Height - nNormalHeight;

            g.IntersectClip(clip);
            g.DrawString(
    strText,
    font,
    linGrBrush,
    textRect,
    format);
            g.Clip = old_clip;
        }

        public void Paint(long x_offs,
            long y_offs,
            int nWidthParam,
            int nHeight,
            // Color textColor,
            bool bLineSeleted,
    PaintEventArgs pe)
        {
            bool bClip = (this.Container.TextHeight < this.TextHeight);

            Padding cell_padding = this.Container.Control.m_cellPadding;
            DpTable control = this.Container.Control;
            Color textColor;

            if (bLineSeleted == true || this.Selected == true)
            {
                // 如果行没有绘制背景，则这里绘制
                if (control.Focused == true)
                {
                    textColor = control.m_highlightForeColor;
                }
                else
                {
                    textColor = control.InactiveHightlightForeColor;
                }
            }
            else
            {
                textColor = this.DisplayForeColor;
            }

            Rectangle rectBack = new Rectangle(
(int)x_offs,
(int)y_offs,
nWidthParam,
nHeight);
            if (control.HasPaintBack == true)
            {
                PaintBackArgs e1 = new PaintBackArgs();
                e1.Rect = rectBack;
                e1.Item = this;
                e1.pe = pe;
                control.TriggerPaintBack(this, e1);
            }
            else
            {

                if (bLineSeleted == true || this.Selected == true)
                {
                    // 如果行没有绘制背景，则这里绘制
                    if (control.Focused == true)
                    {
                        //textColor = control.m_highlightForeColor;

                        if (this.Selected == true)
                            pe.Graphics.FillRectangle(new SolidBrush(control.m_highlightBackColor), rectBack);
                    }
                    else
                    {
                        //textColor = control.InactiveHightlightForeColor;
                        if (this.Selected == true)
                            pe.Graphics.FillRectangle(new SolidBrush(control.m_inactiveHighlightBackColor), rectBack);
                    }
                }
                else
                {
                    //textColor = this.DisplayForeColor;

                    if (this.m_backColor != Color.Transparent)
                    {
                        pe.Graphics.FillRectangle(new SolidBrush(this.m_backColor), rectBack);
                    }
                }
            }

            // 叠加
            if (this.m_bHover == true)
            {
                pe.Graphics.FillRectangle(control.GetHoverBrush(rectBack), rectBack);
            }

            int nWidth = nWidthParam;

            int nIconWidth = 0;
            int nIconHeight = 0;

            // 除去Icon宽度
            if (((this.ImageIndex != -1 && this.Container.Control.ImageList != null) || this._image != null)
                && this.Container != null
                && this.Container.Control != null
                )
            {
                Image image = null;
                if (this.ImageIndex != -1)
                {
                    ImageList imagelist = this.Container.Control.ImageList;
                    if (this.ImageIndex >= 0
    && this.ImageIndex < imagelist.Images.Count)
                        image = imagelist.Images[this.ImageIndex];
#if NO
                    nIconWidth = imagelist.ImageSize.Width;
                    nIconHeight = imagelist.ImageSize.Height;
#endif

                }
                else
                {
                    Debug.Assert(this._image != null, "");
                    image = this._image;
                }

                if (image != null)
                {
                    nIconWidth = image.Width;
                    nIconHeight = image.Height;

                    nWidth -= nIconWidth;

                    if (nWidth < 0)
                        nWidth = 0;

                    Rectangle imageRect = new Rectangle(
(int)x_offs + cell_padding.Left,
(int)y_offs + cell_padding.Top,
nWidthParam - cell_padding.Horizontal,
nHeight - cell_padding.Vertical);

                    Region old_clip = pe.Graphics.Clip;
                    Rectangle clip = imageRect;

                    pe.Graphics.IntersectClip(clip);

                    // 绘制Icon
                    pe.Graphics.DrawImage(image,
                        (float)x_offs + cell_padding.Left,
        (float)y_offs + cell_padding.Top);

                    pe.Graphics.Clip = old_clip;
                }
            }

            Rectangle textRect = new Rectangle(
    (int)x_offs + cell_padding.Left + nIconWidth,
    (int)y_offs + cell_padding.Top,
    nWidth - cell_padding.Horizontal,
    nHeight - cell_padding.Vertical);

            if (textRect.Width > 0)
            {

                StringFormat format = new StringFormat();   //  (StringFormat)StringFormat.GenericTypographic.Clone();
                format.FormatFlags |= StringFormatFlags.FitBlackBox;
                format.Alignment = this.DisplayAlignment;
                format.LineAlignment = this.DisplayLineAlignment;

                if (bClip == false)
                {
                    pe.Graphics.DrawString(
                        this.m_strText,
                        this.DisplayFont,
                        new SolidBrush(textColor),
                        textRect,
                        format);
                }
                else
                {
                    int nNormalHeight = textRect.Height - (textRect.Height / 4);
                    DrawClippedString(
                        pe.Graphics,
                        this.m_strText,
                        this.DisplayFont,
                        textColor,
                        textRect,
                        nNormalHeight,
                        format);
                }

            }

            // Ext
            if (this.OwnerDraw == true
                && this.Container != null
                && this.Container.Control != null)
            {
                Rectangle imageRect = new Rectangle(
(int)x_offs + cell_padding.Left,
(int)y_offs + cell_padding.Top,
nWidthParam - cell_padding.Horizontal,
nHeight - cell_padding.Vertical);

                Region old_clip = pe.Graphics.Clip;
                Rectangle clip = imageRect;

                pe.Graphics.IntersectClip(clip);

                this.Container.Control.PaintExtRegion(
                    pe,
                    this,
                    textRect.X,
                    textRect.Y + this.TextHeight - this.ExtHeight,
                    textRect.Width,
                    this.ExtHeight);

                pe.Graphics.Clip = old_clip;
            }

            // 定位控件
            if (this.m_innerControl != null)
            {
                if (this.m_innerControl.Location.X != textRect.Left
                    || this.m_innerControl.Location.Y != textRect.Top)
                    this.m_innerControl.Location = new Point(textRect.Left, textRect.Top);
                if (this.m_innerControl.Size.Width != textRect.Width
                    || this.m_innerControl.Size.Height != textRect.Height)
                    this.m_innerControl.Size = new Size(textRect.Width, textRect.Height);
            }

            if (control.m_focusObj == this)
            {

                rectBack.Inflate(-2, -2);
                ControlPaint.DrawFocusRectangle(pe.Graphics,
                    rectBack);
            }
            /*
            TextRenderer.DrawText(
    pe.Graphics,
    this.Text,
    this.Font,
    textRect,
    Color.Black,
    editflags);  // TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
            */
        }

        public int GetTextHeight(Graphics g, int nWidthParam)
        {
            if (this.m_innerControl != null)
                return this.m_innerControl.Height;

            /*
            SizeF size = TextRenderer.MeasureText(g,
    this.Text == "" ? "lg" : this.Text,
    this.Font,
    new Size(nWidth, -1),
    editflags);
             * */
            int nWidth = nWidthParam;

            int nIconWidth = 0;
            int nIconHeight = 0;

            // 除去Icon宽度
            if (((this.ImageIndex != -1 && this.Container.Control.ImageList != null ) || this._image != null)
                && this.Container != null
                && this.Container.Control != null
                )
            {
                if (this.ImageIndex != -1)
                {
                    nIconWidth = this.Container.Control.ImageList.ImageSize.Width;
                    nIconHeight = this.Container.Control.ImageList.ImageSize.Height;
                }
                else
                {
                    Debug.Assert(this._image != null, "");
                    nIconWidth = this._image.Width;
                    nIconHeight = this._image.Height;
                }

                nWidth -= nIconWidth;

                if (nWidth < 0)
                    nWidth = 0;
            }

            Padding cell_padding = this.Container.Control.m_cellPadding;
            StringFormat format = new StringFormat();   //  (StringFormat)StringFormat.GenericTypographic.Clone();
            format.FormatFlags |= StringFormatFlags.FitBlackBox;
            SizeF size = g.MeasureString(this.m_strText,
                this.DisplayFont,
                nWidth - cell_padding.Horizontal,
                format);

            if (this.OwnerDraw == true)
            {
                this.ExtHeight = this.Container.Control.QueryExtHeight(this,
                    nIconWidth,
                    nWidth);
            }
            else
                this.ExtHeight = 0;

            this.TextHeight = (int)size.Height + this.ExtHeight; // 保存下来

            // return Math.Max((int)size.Height, nIconHeight);
            return Math.Max((int)size.Height + this.ExtHeight, nIconHeight);
        }

        // 修改一个单元的选定状态
        // 并不刷新显示，也不触发事件
        // return:
        //      false   没有发生变化
        //      true    发生了变化
        internal bool InternalSelect(SelectAction action)
        {
            if (action == SelectAction.On)
            {
                if (this.m_bSelected == false)
                {
                    this.m_bSelected = true;
                    return true;
                }
                return false;
            }
            else if (action == SelectAction.Off)
            {
                if (this.m_bSelected == true)
                {
                    this.m_bSelected = false;
                    return true;
                }
                return false;
            }
            else if (action == SelectAction.Toggle)
            {
                if (this.m_bSelected == false)
                    this.m_bSelected = true;
                else
                    this.m_bSelected = false;
                return true;
            }

            Debug.Assert(false, "");
            return false;
        }

        // 2012/5/25
        public int GetWidth(int index = -1)
        {
            // Padding cell_padding = this.Container.Control.m_cellPadding;
            DpTable control = this.Container.Control;

            if (index == -1)
            {
                index = this.Container.IndexOf(this);


                Debug.Assert(index != -1, "");
            }

            return control.m_columns[index].m_nWidth;
        }

        // 得到一个对象的矩形(view坐标)
        public RectangleF GetViewRect(int nWidth = -1,
            int nHeight = -1,
            int index = -1)
        {
            Padding padding = this.Container.Control.Padding;
            Padding cell_padding = this.Container.Control.m_cellPadding;
            DpTable control = this.Container.Control;

            if (index == -1)
            {
                index = this.Container.IndexOf(this);

                if (index == -1)
                    return new RectangleF(0, 0, 0, 0); // 说明对象已经不在挂接状态

                Debug.Assert(index != -1, "");
            }

            if (nWidth == -1)
                nWidth = control.m_columns[index].m_nWidth;

            if (nHeight == -1)
                nHeight = this.Container.TextHeight + cell_padding.Vertical;

            RectangleF rect = new RectangleF(0,
                0,
                nWidth,
                nHeight);

            // 变换为整体文档坐标，然后变换为view坐标
            rect.Offset(control.m_lWindowOrgX + padding.Left + control.m_nHorzDelta + this.Container.GetCellXOffset(this),
                control.m_lWindowOrgY
                + padding.Top 
                + control.ColumnHeight
                + control.Rows.GetLineYOffset(this.Container));

            return rect;
        }

        // 得到对象的Icon的矩形(view坐标)
        public RectangleF GetIconViewRect()
        {
            if (this.Container.Control.ImageList == null && this._image == null)
                return new RectangleF(0, 0, 0, 0);

            Padding padding = this.Container.Control.Padding;
            Padding cell_padding = this.Container.Control.m_cellPadding;
            DpTable control = this.Container.Control;

            int nIconWidth = 0;
            int nIconHeight = 0;
            if (this._image != null)
            {
                Debug.Assert(this._image != null, "");
                nIconWidth = this._image.Width;
                nIconHeight = this._image.Height;
            }
            else if (this.Container.Control.ImageList != null)
            {
                nIconWidth = this.Container.Control.ImageList.ImageSize.Width;
                nIconHeight = this.Container.Control.ImageList.ImageSize.Height;
            }
#if NO
            Size imagesize = this.Container.Control.ImageList.ImageSize;

            RectangleF rect = new RectangleF(0,
                0,
                imagesize.Width,
                imagesize.Height);
#endif
            RectangleF rect = new RectangleF(0,
    0,
    nIconWidth,
    nIconHeight);

            // 变换为整体文档坐标，然后变换为view坐标
            rect.Offset(control.m_lWindowOrgX + padding.Left + control.m_nHorzDelta + this.Container.GetCellXOffset(this),
                control.m_lWindowOrgY
                + padding.Top
                + control.ColumnHeight
                + control.Rows.GetLineYOffset(this.Container));

            return rect;
        }
    }

    public enum DpRowStyle
    {
        None = 0,
        Seperator = 0x01,   // 分割线 行
        HorzGrid = 0x02,    // 水平横线 
    }

    // 一个显示行
    public class DpRow : List<DpCell>
    {
        public DpTable Control = null;

        public object Tag = null;

        public int TextHeight = 0;
        internal bool m_bHover = false;

        public DpRowStyle Style = DpRowStyle.None;

        internal StringAlignment m_alignment = StringAlignment.Near;
        [DescriptionAttribute("Alignment")]
        [DefaultValue(typeof(StringAlignment), "StringAlignment.Near")]
        public StringAlignment Alignment
        {
            get
            {
                return this.m_alignment;
            }
            set
            {
                this.m_alignment = value;

                if (this.Control != null)
                {
                    this.Control.InvalidateLine(this);
                }
            }
        }

        internal StringAlignment m_lineAlignment = StringAlignment.Near;
        [DescriptionAttribute("LineAlignment")]
        [DefaultValue(typeof(StringAlignment), "StringAlignment.Near")]
        public StringAlignment LineAlignment
        {
            get
            {
                return this.m_lineAlignment;
            }
            set
            {
                this.m_lineAlignment = value;

                if (this.Control != null)
                {
                    this.Control.InvalidateLine(this);
                }
            }
        }

        internal bool m_bSelected = false;
        public bool Selected
        {
            get
            {
                return this.m_bSelected;
            }
            set
            {
                // 提供给外部调用
                if (value != this.m_bSelected)
                {
                    this.m_bSelected = value;

                    if (this.Control != null)
                    {
                        // 迫使将来重新获取
                        this.Control.m_selectedLineIndices = null;
                        this.Control.m_seletecLines = null;


                        this.Control.InvalidateLine(this);

                        this.Control.TriggerSelectionChanged();
                    }

#if DEBUG
                    if ((this.Style & DpRowStyle.Seperator) != 0
        && value == true)
                    {
                        Debug.Assert(false, "分割条不应被选择");
                    }
#endif
                }
            }
        }

        public bool IsSeperator
        {
            get
            {
                return (this.Style & DpRowStyle.Seperator) != 0;
            }
        }

        public bool IsHorzGrid
        {
            get
            {
                return (this.Style & DpRowStyle.HorzGrid) != 0;
            }
        }

        Font m_font = null;
        public Font Font
        {
            get
            {
                if (this.m_font != null)
                    return this.m_font;

                return this.Control.Font;
            }
            set
            {
                this.m_font = value;

                if (this.Control != null)
                {
                    // 重新初始化行的高度
                    this.UpdateLineHeight();
                }
            }
        }

        public Font DisplayFont
        {
            get
            {
                if (this.m_font != null)
                    return this.m_font;

                return this.Control.Font;
            }
        }

        Color m_backColor = Color.Transparent;
        public Color BackColor
        {
            get
            {
                if (this.m_backColor == Color.Transparent)
                    return this.Control.BackColor;
                return this.m_backColor;
            }
            set
            {
                this.m_backColor = value;

                if (this.Control != null)
                    this.Control.InvalidateLine(this);
            }
        }

        Color m_foreColor = Color.Transparent;
        public Color ForeColor
        {
            get
            {
                if (this.m_foreColor == Color.Transparent)
                    return this.Control.ForeColor;
                return this.m_foreColor;
            }
            set
            {
                this.m_foreColor = value;
                if (this.Control != null)
                    this.Control.InvalidateLine(this);
            }
        }

        public void EnsureVisible()
        {
            if (this.Control != null)
                this.Control.EnsureVisible(this);
        }

        // 因为字体、Padding等改变，改变行的显示高度
        // TODO: 行的高度改变，需要 Invalidate 整个行
        // return:
        //      行高度是否发生改变
        public bool UpdateLineHeight()
        {
            // 重新初始化行的高度
            int nOldTextHeight = this.TextHeight;

            // 2015/6/10
            // m_lContentHeight 在 this.GetTextHeight() 调用过程中可能被改变
            long lOldContentHeight = this.Control.m_lContentHeight;
            this.TextHeight = this.GetTextHeight();
            bool bContentHeightChanged = lOldContentHeight != this.Control.m_lContentHeight;

            if (nOldTextHeight != this.TextHeight)
            {
                // 如果 this.GetTextHeight() 调用中总高度没有发生过改变，这里才主动增加总高度
                if (bContentHeightChanged == false)
                {
                    this.Control.m_lContentHeight += this.TextHeight - nOldTextHeight;   // 优化
                    this.Control.SetScrollBars(ScrollBarMember.Vert);
                }

                // 2013/12/12
                this.Control.InvalidateLineAndBlow(this, true);
#if NO
                // 将当前行以下的全部区域失效。TODO: 可以优化为ScrollWindowEx()
                Rectangle rect = Rectangle.Ceiling(this.GetViewRect());
                rect.Inflate(1, 1);
                //if (rect.Top < this.Control.ClientSize.Height)
                //{

                rect.Height = (int)this.Control.m_lContentHeight;

                // 2013/12/12
                {
                    rect.X -= this.Control.Padding.Left + this.Control.m_documentMargin.Left;
                    rect.Width += this.Control.Padding.Horizontal + this.Control.m_documentMargin.Horizontal;
                }

                this.Control.Invalidate(rect);
                //}
#endif

                // this.Control.InvalidateLine(this);
                return true;
            }

            return false;
        }

        public int GetTextHeight(Graphics g = null,
             DpColumnRow cols = null)
        {
            if ((this.Style & DpRowStyle.Seperator) != 0)
                return 2;

            if (g == null)
			    g = Graphics.FromHwnd(this.Control.Handle);

            if (cols == null)
                cols = this.Control.m_columns;

            int nMaxImageHeight = 0;

            int nCount = Math.Min(this.Count, cols.Count);
            int nHeight = 0;
            for (int i = 0; i < nCount; i++)
            {
                DpCell cell = this[i];
                int nWidth = cols[i].m_nWidth;
                if (nWidth <= 0)
                    continue;
                int nTempHeight = cell.GetTextHeight(g, nWidth);
                if (nTempHeight > nHeight)
                    nHeight = nTempHeight;

                // TODO: 也要考虑 imagelist 情况
                if (cell.Image != null)
                    nMaxImageHeight = Math.Max(nMaxImageHeight, cell.Image.Height);
            }

            if (this.Control.m_nMaxTextHeight == -1)
                return nHeight;

            // 如果有图像，则最大高度不低于图像高度。这可以保证图像不被竖向截断
            if (nMaxImageHeight > this.Control.m_nMaxTextHeight)
                return Math.Min(nMaxImageHeight, nHeight);

            return Math.Min(this.Control.m_nMaxTextHeight, nHeight);
        }

        // 隐藏行内的内嵌控件
        public void HideInnerControls()
        {
            int nCount = Math.Min(this.Count, this.Control.Columns.Count);
            for (int i = 0; i < nCount; i++)
            {
                DpCell cell = this[i];

                if (cell.InnerControl != null)
                {
                    // cell.InnerControl.Dock = DockStyle.None;
                    cell.InnerControl.Location = new Point(-1000,
                        -1000);
                    // cell.InnerControl.Size = new Size(1, 1);
                    // cell.InnerControl.BackColor = Color.Red;
                    //cell.InnerControl.Size = new Size(nWidth, cell.InnerControl.Height);
                    // cell.InnerControl.Visible = false;
                }
            }
        }

        // 布局行内的内嵌控件
        public void LayoutInnerControl(long x_offs,
            long y_offs,
            DpColumnRow cols,
            bool bMove)
        {
            Padding cell_padding = this.Control.m_cellPadding;

            /*
            RectangleF rectBack = new RectangleF(x_offs,
y_offs,
this.Control.m_lContentWidth,
this.TextHeight + cell_padding.Vertical);
             * */


            int nCount = Math.Min(this.Count, this.Control.Columns.Count);
            for (int i = 0; i < nCount; i++)
            {
                DpCell cell = this[i];
                int nWidth = cols[i].m_nWidth;

                RectangleF rect = new RectangleF(0 + x_offs,
    y_offs,
    nWidth,
    this.TextHeight + cell_padding.Vertical);

                if (cell.InnerControl != null)
                {
                    // cell.InnerControl.Dock = DockStyle.None;
                    cell.InnerControl.Location = new Point((int)x_offs,
                        (int)y_offs);
                    if (bMove == false)
                        cell.InnerControl.Size = new Size(nWidth, cell.InnerControl.Height);

                    // cell.InnerControl.Size = new Size(1, 1);
                    // cell.InnerControl.BackColor = Color.Red;
                    // cell.InnerControl.Visible = false;
                }

                x_offs += nWidth;
            }
        }

        public void Paint(long x_offs,
            long y_offs,
            DpColumnRow cols,
            PaintEventArgs pe)
        {
            Padding cell_padding = this.Control.m_cellPadding;

            RectangleF rectBack = new RectangleF(x_offs,
y_offs,
this.Control.m_lContentWidth,
this.TextHeight + cell_padding.Vertical);


            if ((this.Style & DpRowStyle.Seperator) != 0)
            {
                if (this.m_backColor != Color.Transparent)
                {
                    pe.Graphics.FillRectangle(new SolidBrush(this.m_backColor), rectBack);
                }

                Color textColor = this.ForeColor;
                pe.Graphics.DrawLine(new Pen(textColor, (float)1),
                    x_offs + cell_padding.Left, y_offs + cell_padding.Top,
                    x_offs + this.Control.m_lContentWidth - cell_padding.Right, y_offs + cell_padding.Top);

                Debug.Assert(this.Control.m_focusObj != this, "");


                return;
            }

            if (this.Control.HasPaintBack == true)
            {
                PaintBackArgs e1 = new PaintBackArgs();
                e1.Rect = rectBack;
                e1.Item = this;
                e1.pe = pe;
                this.Control.TriggerPaintBack(this, e1);
            }
            else
            {
                if (this.m_bSelected == true)
                {

                    if (this.Control.Focused == true)
                    {
                        // textColor = SystemColors.HighlightText;
                        pe.Graphics.FillRectangle(new SolidBrush(this.Control.m_highlightBackColor), rectBack);
                    }
                    else
                    {
                        // textColor = SystemColors.InactiveCaptionText;

                        pe.Graphics.FillRectangle(new SolidBrush(this.Control.m_inactiveHighlightBackColor), rectBack);
                    }
                }
                else
                {
                    /*
                    textColor = this.Control.ForeColor;
                    pe.Graphics.FillRectangle(new SolidBrush(this.Control.BackColor), rectBack);
                     * */

                    if (this.m_backColor != Color.Transparent)
                    {
                        pe.Graphics.FillRectangle(new SolidBrush(this.m_backColor), rectBack);
                    }

                }
            }

            if (this.IsHorzGrid == true)
            {
                pe.Graphics.DrawLine(new Pen(SystemColors.ControlDark, (float)1),
    rectBack.X, rectBack.Y + rectBack.Height - 1,
    rectBack.X + rectBack.Width, rectBack.Y + rectBack.Height - 1);
            }

            // 叠加到原来背景之上
            if (this.m_bHover == true)
                pe.Graphics.FillRectangle(this.Control.GetHoverBrush(Rectangle.Ceiling(rectBack)), rectBack);


            int nCount = Math.Min(this.Count, this.Control.Columns.Count);
            for (int i = 0; i < nCount; i++)
            {
                DpCell cell = this[i];
                int nWidth = cols[i].m_nWidth;

                RectangleF rect = new RectangleF(0 + x_offs,
    y_offs,
    nWidth,
    this.TextHeight + cell_padding.Vertical);
                if (rect.IntersectsWith(pe.ClipRectangle) == true)
                {
                    cell.Paint(x_offs,
                        y_offs,
                        nWidth,
                        this.TextHeight + cell_padding.Vertical,
                        // textColor,
                        this.m_bSelected,
                        pe);
                }

                x_offs += nWidth;
            }

            if (this.Control.m_focusObj == this)
            {
                rectBack.Inflate(-2, -2);
                ControlPaint.DrawFocusRectangle(pe.Graphics,
                    Rectangle.Round(rectBack));
            }
        }

        // 点击测试
        // parameters:
        //      p_x 点击位置x。为行内坐标
        public void HitTest(long p_x,
            long p_y,
            out HitTestResult result)
        {
            result = new HitTestResult();

            if (this.IsSeperator == true)
            {
                result.AreaPortion = AreaPortion.Content;
                result.Object = this;
                result.X = p_x;
                result.Y = p_y;
                return;
            }

            Padding cell_padding = this.Control.m_cellPadding;

            long x = 0;
            for (int i = 0; i < this.Count; i++)
            {
                DpCell cell = this[i];
                int nWidth = this.Control.m_columns[i].m_nWidth;

                RectangleF rect = new RectangleF(x,
    0,
    nWidth,
    this.TextHeight + cell_padding.Vertical);

                if (GuiUtil.PtInRect(p_x, p_y, rect) == true)
                {
                    result.AreaPortion = AreaPortion.Content;
                    result.Object = cell;
                    result.X = p_x - x;
                    result.Y = p_y;
                    return;
                }

                x += nWidth;
            }

            result.AreaPortion = AreaPortion.Content;
            result.Object = this;
            result.X = p_x;
            result.Y = p_y;
        }

        public new void Add(DpCell cell)
        {
            cell.Container = this;
            base.Add(cell);
        }

        // 得到一个对象的矩形(view坐标)
        public RectangleF GetViewRect(int index = -1)
        {
            if (index == -1)
            {
                index = this.Control.Rows.IndexOf(this);
                if (index == -1)
                    return new RectangleF(0,0,0,0); // 说明对象已经不在挂接状态

                Debug.Assert(index != -1, "");
            }
            Padding padding = this.Control.Padding;
            Padding cell_padding = this.Control.m_cellPadding;

            RectangleF rect = new RectangleF(0,
                0,
                this.Control.m_lContentWidth,
                this.TextHeight + cell_padding.Vertical);

            // 变换为整体文档坐标，然后变换为view坐标
            rect.Offset(this.Control.m_lWindowOrgX + padding.Left + this.Control.m_nHorzDelta,
                this.Control.m_lWindowOrgY
                + padding.Top 
                + this.Control.ColumnHeight
                + this.Control.Rows.GetLineYOffset(this));
            
            return rect;
        }

        public void Select(SelectAction action)
        {
            if (this.InternalSelect(action) == true
                && this.Control != null)
            {
                this.Control.InvalidateLine(this);
                this.Control.ExpireSelectedLines();
                this.Control.TriggerSelectionChanged();
            }
        }

        // 修改一行的选定状态
        // 并不刷新显示，也不触发事件
        // return:
        //      false   没有发生变化
        //      true    发生了变化
        internal bool InternalSelect(SelectAction action)
        {
            if (action == SelectAction.On)
            {
                if (this.m_bSelected == false)
                {
                    this.m_bSelected = true;
                    return true;
                }
                return false;
            }
            else if (action == SelectAction.Off)
            {
                if (this.m_bSelected == true)
                {
                    this.m_bSelected = false;
                    return true;
                }
                return false;
            }
            else if (action == SelectAction.Toggle)
            {
                if (this.m_bSelected == false)
                    this.m_bSelected = true;
                else
                    this.m_bSelected = false;
                return true;
            }

            Debug.Assert(false, "");
            return false;
        }

        public long GetCellXOffset(DpCell cell)
        {
            long result = 0;
            for(int i=0;i<this.Count;i++)
            {
                DpCell curcell = this[i];
                if (curcell == cell)
                    break;
                int nWidth = this.Control.m_columns[i].m_nWidth;
                result += nWidth;
            }

            return result;
        }
    }

    public class DpRowCollection : List<DpRow>
    {
        public DpTable Control = null;
 
        public new void Add(DpRow row)
        {
            Debug.Assert(this.Control != null, "");
            row.Control = Control;
            base.Add(row);

            // bool bColumnChanged = this.Control.EnsureColumnWidths(row);

			Graphics g = Graphics.FromHwnd(this.Control.Handle);

            // 初始化高度
            row.TextHeight = row.GetTextHeight(g, this.Control.m_columns);

            ScrollBarMember scrollbar = ScrollBarMember.Vert;
            /*
            if (this.Count == 1 || bColumnChanged == true)
            {
                this.Control.m_lContentWidth = this.Control.GetContentWidth();
                scrollbar |= ScrollBarMember.Horz;
            }
             * */

            Padding cell_padding = this.Control.m_cellPadding;
            this.Control.m_lContentHeight += row.TextHeight + cell_padding.Vertical;   // 优化
            
            // 行间距
            if (this.Count > 1)
                this.Control.m_lContentHeight += this.Control.m_nLineDistance;

            // 加入控件
            foreach (DpCell cell in row)
            {
                if (cell.m_innerControl != null)
                    this.Control.Controls.Add(cell.m_innerControl);
            }

            if (this.Control.m_bDelayUpdate == false)
            {
                this.Control.SetScrollBars(scrollbar);

                // 精确失效新增的行
                if (this.Count > 1) 
                    this.Control.InvalidateLineAndBlow(row, true, true);
                else
                    this.Control.InvalidateLineAndBlow(row, true);
            }
        }

        public new bool Remove(DpRow row)
        {
            // 2013/12/11
            if (this.Control.m_bDelayUpdate == false)
            {
                Graphics g = Graphics.FromHwnd(this.Control.Handle);

                // 初始化高度
                row.TextHeight = row.GetTextHeight(g, this.Control.m_columns);

                ScrollBarMember scrollbar = ScrollBarMember.Vert;

                Padding cell_padding = this.Control.m_cellPadding;
                this.Control.m_lContentHeight -= row.TextHeight + cell_padding.Vertical;   // 优化

                // 行间距
                if (this.Count > 1)
                    this.Control.m_lContentHeight -= this.Control.m_nLineDistance;
                
                this.Control.SetScrollBars(scrollbar);

                // 精确失效新增的行
                this.Control.InvalidateLineAndBlow(row, true);

                // TODO: 如果 focusObj 正好为 row，需要消除 focusObj，以免下次刷新 FocuedItem 的时候遇到找不到对象的问题

                // selected indices
                this.Control.ExpireSelectedLines();
                this.Control.ExpireSelectedXYs();
            }

            bool bRet = base.Remove(row);
            if (bRet == true)
            {
#if NO
                // 移走控件
                foreach (DpCell cell in row)
                {
                    if (cell.m_innerControl != null)
                        this.Control.Controls.Remove(cell.m_innerControl);
                }
#endif

            }

            return bRet;
        }

        public new void RemoveAt(int index)
        {
            DpRow row = this[index];
            this.Remove(row);
        }

        public new void Insert(int nIndex, DpRow row)
        {
            Debug.Assert(this.Control != null, "");
            row.Control = Control;
            base.Insert(nIndex, row);

            // bool bColumnChanged = this.Control.EnsureColumnWidths(row);

            Graphics g = Graphics.FromHwnd(this.Control.Handle);

            // 初始化高度
            row.TextHeight = row.GetTextHeight(g, this.Control.m_columns);

            ScrollBarMember scrollbar = ScrollBarMember.Vert;

            Padding cell_padding = this.Control.m_cellPadding;
            this.Control.m_lContentHeight += row.TextHeight + cell_padding.Vertical;   // 优化
            // 行间距
            if (this.Count > 1)
                this.Control.m_lContentHeight += this.Control.m_nLineDistance;

#if NO
            // 加入控件
            foreach (DpCell cell in row)
            {
                if (cell.m_innerControl != null)
                    this.Control.Controls.Add(cell.m_innerControl);
            }
#endif

            this.Control.SetScrollBars(scrollbar);

            // 精确失效新增的行
            if (this.Count > 1)
                this.Control.InvalidateLineAndBlow(row, true, true);
            else
                this.Control.InvalidateLineAndBlow(row, true);
        }


        public long GetLineYOffset(DpRow line)
        {
            Padding cell_padding = this.Control.m_cellPadding;
            long result = 0;
            int i = 0;
            foreach (DpRow curline in this)
            {
                if (curline == line)
                    break;
                result += curline.TextHeight + cell_padding.Vertical;

                // 行间距
                result += this.Control.m_nLineDistance;
                i++;
            }

            return result;
        }

        public new void Clear()
        {
            int nOldCount = base.Count;

            base.Clear();

            // 清除全部控件
            this.Control.Controls.Clear();

            this.Control.m_lWindowOrgX = 0;    // 窗口原点
            this.Control.m_lWindowOrgY = 0;

            this.Control.AfterDocumentChanged(ScrollBarMember.Both);

            this.Control.m_lastHoverObj = null;
            this.Control.m_focusObj = null;
            if (this.Control.m_textbox != null)
                this.Control.m_textbox.Visible = false;
            this.Control.m_shiftStartObj = null;

            /*
            this.Control.m_seletecLines = null;     // 选定的行对象
            this.Control.m_selectedLineIndices = null; // 选定的行的行号
            this.Control.m_seletedCellXY = null;    // 选定的格子坐标
             * */
            this.Control.ExpireSelectedLines();
            this.Control.ExpireSelectedXYs();

            this.Control.Invalidate();

            // TODO: 还可以优化一下，函数调用前明显没有选定任何对象的，这里就不触发事件了
            if (nOldCount > 0)
                this.Control.TriggerSelectionChanged();  // 2014/11/11
        }



    }

    public class CellXY
    {
        public int X = -1;
        public int Y = -1;

        public CellXY(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }
    }
    // 区域名称
    public enum AreaPortion
    {
        None = 0,
        LeftBar = 1,    // 左边的竖条
        ColumnTitle = 2,    // 栏目标题
        Content = 3,    // 内容本体
        CheckBox = 4,   // checkbox

        LeftBlank = 5,  // 左边空白
        TopBlank = 6,   // 上方空白
        RightBlank = 7, // 右方空白
        BottomBlank = 8,    // 下方空白

        Splitter = 9,   // 分割带
    }

    // 点击检测结果
    public class HitTestResult
    {
        public object Object = null;    // 点击到的末级对象
        public AreaPortion AreaPortion = AreaPortion.None;

        // 对象坐标下的点击位置
        public long X = -1;
        public long Y = -1;
    }

    // 选择一个对象的动作
    public enum SelectAction
    {
        Toggle = 0,
        On = 1,
        Off = 2,
    }

    public enum ScrollBarMember
    {
        Vert = 1,
        Horz = 2,
        Both = 3,
    }

    /// <summary>
    /// 卷滚条被触动事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void ScrollBarTouchedEventHandler(object sender,
        ScrollBarTouchedArgs e);

    /// <summary>
    /// 卷滚条被触动事件的参数
    /// </summary>
    public class ScrollBarTouchedArgs : EventArgs
    {
        public string Position = "";    // 到达什么位置
    }

    /// <summary>
    /// 区域绘制事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void PaintRegionEventHandler(object sender,
        PaintRegionArgs e);

    /// <summary>
    /// 区域绘制事件的参数
    /// </summary>
    public class PaintRegionArgs : EventArgs
    {
        public string Action = "";  // query / paint 之一
        public PaintEventArgs pe = null;   // [in]
        public object Item = null; // [in] 要绘制的对象 DpRow 或者 DpCell

        public long X = 0; // 绘制区域的左上角坐标
        public long Y = 0;

        public int Width = 0;  // 区域的宽度
        public int Height = 0;
    }

    /// <summary>
    /// 背景绘制事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void PaintBackEventHandler(object sender,
        PaintBackArgs e);

    /// <summary>
    /// 背景绘制事件的参数
    /// </summary>
    public class PaintBackArgs : EventArgs
    {
        public PaintEventArgs pe = null;   // [in]
        public object Item = null; // [in] 要绘制的对象 DpRow 或者 DpCell

        public RectangleF Rect = new RectangleF();
    }

}
