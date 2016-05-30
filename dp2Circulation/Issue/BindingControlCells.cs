using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Diagnostics;
using System.Xml;

using System.Runtime.InteropServices;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.CommonControl;

namespace dp2Circulation
{
    internal class CellBase
    {
        internal bool m_bHover = false;
        internal bool m_bFocus = false;

        // 是否被选定?
        internal bool m_bSelected = false;

        public bool Selected
        {
            get
            {
                return this.m_bSelected;
            }
            set
            {
                this.m_bSelected = value;
                // TODO: 改变显示
            }
        }

        public virtual int Width
        {
            get
            {
                throw new Exception("Width not implement");
            }
        }

        public virtual int Height
        {
            get
            {
                throw new Exception("Height not implement");
            }
        }

        // 选择。
        // 注意：并不负责失效相关区域
        // return:
        //      true    状态发生变化
        //      false   状态没有变化
        public virtual bool Select(SelectAction action)
        {
            bool bOldSelected = this.m_bSelected;

            if (action == SelectAction.Off)
                this.m_bSelected = false;
            else if (action == SelectAction.On)
                this.m_bSelected = true;
            else
            {
                Debug.Assert(action == SelectAction.Toggle, "");
                if (this.m_bSelected == true)
                    this.m_bSelected = false;
                else
                    this.m_bSelected = true;
            }

            /*
            // 递归
            if (bRecursive == true)
            {
                for (int i = 0; i < this.ChildrenCollection.Count; i++)
                {
                    AreaBase obj = this.ChildrenCollection[i];
                    obj.Select(action, true);
                }
            }*/

            return (bOldSelected == this.m_bSelected ? false : true);
        }
    }

    // 用于HitTest()函数的特殊类，表示要获得潜在格子信息
    internal class NullCell : CellBase
    {
        public int X = -1;
        public int Y = -1;

        public NullCell(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        // 绘制NullCell每册的格子
        internal void Paint(
            BindingControl control,
            long start_x,
            long start_y,
            PaintEventArgs e)
        {
            // Debug.Assert(control != null, "");

            if (BindingControl.TooLarge(start_x) == true
                || BindingControl.TooLarge(start_y) == true)
                return;


            int x0 = (int)start_x;
            int y0 = (int)start_y;

            RectangleF rect;

            // 焦点虚线
            if (this.m_bFocus == true)
            {
                rect = new RectangleF(
    (int)start_x,
    (int)start_y,
    control.m_nCellWidth,
    control.m_nCellHeight);

                rect.Inflate(-4, -4);
                ControlPaint.DrawFocusRectangle(e.Graphics,
                    Rectangle.Round(rect));
            }
        }
    }

    // 每一个显示单元
    internal class Cell : CellBase
    {
        public IssueBindingItem Container = null;

        public ItemBindingItem item = null;

        public bool OutofIssue = false; // 卷期号是否与所在行不符? == true 不符；== false 符合

        internal bool m_bDisplayCheckBox = false;   // 是否要在hover时显示checkbox图像

        // public bool Binded = false; // 当item为null时，这里的Binded表示参与装订的占据位置的单元

        public ItemBindingItem ParentItem = null;  // 如果本格子为装订成员，这里是所从属的合订本对象

        // 是否为合订成员格子?
        // 注意，即便为合订成员格子，也可能是空白格子(即this.item == null)
        public bool IsMember
        {
            get
            {
                if (this.ParentItem != null)
                    return true;
                return false;
            }
        }

        public override int Width
        {
            get
            {
                return this.Container.Container.m_nCellWidth;
            }
        }

        public override int Height
        {
            get
            {
                return this.Container.Container.m_nCellHeight;
            }
        }

        public int LineHeight
        {
            get
            {
                return this.Container.Container.m_nLineHeight;
            }
        }



        // 选择。
        // 注意：并不负责失效相关区域
        // return:
        //      true    状态发生变化
        //      false   状态没有变化
        public override bool Select(SelectAction action)
        {
            bool bOldSelected = this.m_bSelected;

            if (action == SelectAction.Off)
                this.m_bSelected = false;
            else if (action == SelectAction.On)
                this.m_bSelected = true;
            else
            {
                Debug.Assert(action == SelectAction.Toggle, "");
                if (this.m_bSelected == true)
                    this.m_bSelected = false;
                else
                    this.m_bSelected = true;
            }

            /*
            // 递归
            if (bRecursive == true)
            {
                for (int i = 0; i < this.ChildrenCollection.Count; i++)
                {
                    AreaBase obj = this.ChildrenCollection[i];
                    obj.Select(action, true);
                }
            }*/

            return (bOldSelected == this.m_bSelected ? false : true);
        }

        // 将本对象坐标体系 的矩形 转换为 顶层对象的坐标体系
        public RectangleF ToRootCoordinate(RectangleF rect)
        {
            float x_offs = 0;
            float y_offs = 0;

            IssueBindingItem issue = this.Container;
            Debug.Assert(issue != null, "");

            x_offs += issue.Container.m_nCoverImageWidth + issue.Container.m_nLeftTextWidth;
            int index = issue.Cells.IndexOf(this);
            Debug.Assert(index != -1, "");

            x_offs += issue.Container.m_nCellWidth * index;

            BindingControl control = issue.Container;
            index = control.Issues.IndexOf(issue);
            Debug.Assert(index != -1, "");

            y_offs += index * control.m_nCellHeight;

            rect.X += x_offs;
            rect.Y += y_offs;
            return rect;
        }


        internal void RefreshOutofIssue()
        {
            if (this.Container == null)
                return;

            IssueBindingItem issue = this.Container;

            int nIndex = issue.IndexOfCell(this);
            if (nIndex == -1)
            {
                Debug.Assert(nIndex != -1, "");
                return;
            }

            /*
                        if ((nIndex % 2) == 0)
                        {
                            this.OutofIssue = false;
                            return; // 双格的左边没有必要设置?
                        }
             * */

            issue.RefreshOutofIssueValue(nIndex);
        }

        // Cell 点击测试
        // parameters:
        //      p_x   已经是文档坐标。即文档左上角为(0,0)
        //      type    要测试的最下级（叶级）对象的类型。如果为null，表示一直到末端
        public void HitTest(long p_x,
            long p_y,
            Type dest_type,
            out HitTestResult result)
        {
            result = new HitTestResult();

            if (GuiUtil.PtInRect((int)p_x,
                (int)p_y,
                this.Container.Container.RectGrab) == true)
            {
                result.AreaPortion = AreaPortion.Grab;
                result.X = p_x;
                result.Y = p_y;
                result.Object = this;
                return;
            }

            int nCenterX = this.Container.Container.m_nCellWidth / 2;
            int nCenterY = this.Container.Container.m_nCellHeight / 2;
            int nWidth = this.Container.Container.RectGrab.Width;
            Rectangle rectCheckBox = new Rectangle(
                nCenterX - nWidth / 2,
                nCenterY - nWidth / 2,
                nWidth,
                nWidth);
            if (GuiUtil.PtInRect((int)p_x,
    (int)p_y,
    rectCheckBox) == true)
            {
                result.AreaPortion = AreaPortion.CheckBox;
                result.X = p_x;
                result.Y = p_y;
                result.Object = this;
                return;
            }

            result.AreaPortion = AreaPortion.Content;
            result.X = p_x;
            result.Y = p_y;
            result.Object = this;
            return;
        }


        public PaintInfo GetPaintInfo()
        {
            PaintInfo info = new PaintInfo();
            // 普通单册
            info.ForeColor = this.Container.Container.SingleForeColor;

            if (this.IsMember == true)
            {
                // 成员册
                info.ForeColor = this.Container.Container.MemberForeColor;
                info.BackColor = this.Container.Container.MemberBackColor;
            }
            else if (this.item != null
                && this.item.Calculated == true)
            {
                // 预测的单册
                info.ForeColor = this.Container.Container.CalculatedForeColor;
                info.BackColor = this.Container.Container.CalculatedBackColor;
            }
            else if (this.item != null
           && this.item.IsParent == true)
            {
                // 合订本
                info.ForeColor = this.Container.Container.ParentForeColor;
                info.BackColor = this.Container.Container.ParentBackColor;
            }
            else
            {
                info.BackColor = this.Container.Container.SingleBackColor;
            }

            return info;
        }

        // TODO: 释放 Brush
        public void PaintBorder(long start_x,
            long start_y,
            int nWidth,
            int nHeight,
            PaintEventArgs e)
        {
            Debug.Assert(this.Container != null, "");
            bool bSelected = this.Selected;
            RectangleF rect;

            // 是否进行了整体旋转
            bool bRotate = false;

            // 普通单册
            Color colorText = this.Container.Container.SingleForeColor;
            Color colorGray = this.Container.Container.SingleGrayColor;
            Brush brushBack = null;
            float fBorderWidth = 2;
            Color colorBorder = Color.FromArgb(255, Color.Gray);

            try
            {

                // 背景
                if (this.IsMember == true)
                {
                    // 成员册
#if DEBUG
                    if (this.item != null)
                    {
                        Debug.Assert(this.item.IsMember == true, "");
                    }
#endif
                    fBorderWidth = 1;
                    Color colorBack = this.Container.Container.MemberBackColor;
                    {
                        brushBack = new SolidBrush(colorBack);
                    }

                    colorText = this.Container.Container.MemberForeColor;
                    colorGray = this.Container.Container.MemberGrayColor;
                }
                else if (this.item != null
                    && this.item.Calculated == true)
                {
                    // 预测的单册
                    fBorderWidth = (float)1;    //  0.2;
                    Color colorBack = this.Container.Container.CalculatedBackColor;
                    {
                        brushBack = new SolidBrush(colorBack);
                    }
                    colorText = this.Container.Container.CalculatedForeColor;
                    colorGray = this.Container.Container.CalculatedGrayColor;
                    colorBorder = Color.FromArgb(255, Color.White);
                }
                else if (this.item != null
               && this.item.IsParent == true)
                {
                    // 合订本
                    fBorderWidth = 3;
                    Color colorBack = this.Container.Container.ParentBackColor;
                    {
                        brushBack = new SolidBrush(colorBack);
                    }

                    colorText = this.Container.Container.ParentForeColor;
                    colorGray = this.Container.Container.ParentGrayColor;
                }
                else
                {
                    Debug.Assert(brushBack == null, "");
                    brushBack = null;
                    Color colorBack = this.Container.Container.SingleBackColor;
                    {
                        brushBack = new SolidBrush(colorBack);
                    }
                }

                // 边框和边条
                {

                    rect = new RectangleF(start_x,
                        start_y,
                        nWidth,
                        nHeight);

                    /*
                    // 没有焦点时要小一些
                    rect = GuiUtil.PaddingRect(this.Container.Container.CellMargin,
                        rect);
                     * */

                    RectangleF rectTest = rect;
                    rectTest.Inflate(1, 1);
                    rectTest.Width += 2;
                    rectTest.Height += 2;

                    // 优化
                    if (rectTest.IntersectsWith(e.ClipRectangle) == true
                        || bRotate == true)
                    {
                        float round_radius = 10;
                        // 阴影
                        RectangleF rectShadow = rect;
                        rectShadow.Offset((float)1.5, (float)1.5);  // 0.5
                        // rectShadow.Inflate((float)-0.9, (float)-0.9);
                        // Pen penShadow = new Pen(Color.FromArgb(160, 190,190,180),5);
                        using (Brush brushShadow = new SolidBrush(Color.FromArgb(160, 190, 190, 180)))
                        {
                            BindingControl.RoundRectangle(e.Graphics,
                                null,
                                brushShadow,
                                rectShadow,
                                round_radius);
                        }

                        float que_radius = 0;
                        if (this.item != null)
                        {
                            string strIntact = this.item.GetText("intact");
                            float r = GetIntactRatio(strIntact);
                            if (r < (float)1.0)
                            {
                                float h = Math.Min(rect.Width, rect.Height) - (round_radius * 2);
                                que_radius = h - (h * r);
                                if (que_radius < round_radius)
                                    que_radius = round_radius;
                            }
                        }


                        rect.Inflate(-(fBorderWidth / 2), -(fBorderWidth / 2));
                        using (Pen penBorder = new Pen(colorBorder, fBorderWidth))
                        {
                            penBorder.LineJoin = LineJoin.Bevel;
                            if (que_radius == 0)
                                BindingControl.RoundRectangle(e.Graphics,
                                    penBorder,
                                    brushBack,
                                    rect,
                                    round_radius);
                            else
                                BindingControl.QueRoundRectangle(e.Graphics,
                                    penBorder,
                                    brushBack,
                                    rect,
                                    round_radius,
                                    que_radius);
                        }

                    }
                }

            }
            finally
            {
                if (brushBack != null)
                    brushBack.Dispose();
            }

        }

        // 绘制每册的格子
        internal virtual void Paint(
            long start_x,
            long start_y,
            PaintEventArgs e)
        {
            Debug.Assert(this.Container != null, "");

            if (BindingControl.TooLarge(start_x) == true
                || BindingControl.TooLarge(start_y) == true)
                return;

#if DEBUG
            if (this.item != null)
            {
                Debug.Assert(this.item.IsMember == this.IsMember, "");
            }
            else
            {
            }
#endif


            //int x0 = (int)start_x;
            //int y0 = (int)start_y;

            bool bSelected = this.Selected;

            RectangleF rect;

            GraphicsState gstate = null;

            // 是否进行了整体旋转
            bool bRotate = this.OutofIssue == true
                | (this.m_bFocus == true && this.m_bHover == false);

            if (bRotate == true)
            {
                rect = new RectangleF(start_x,
                    start_y,
                    this.Width,
                    this.Height);

                gstate = e.Graphics.Save();
                e.Graphics.Clip = new Region(rect); // TODO: Region 需要明显 Dispose() 么?
                // Setup the transformation matrix
                Matrix x = new Matrix();
                if (this.OutofIssue == true
                    && (this.m_bFocus == true && this.m_bHover == false))
                    x.RotateAt(-35, new PointF(start_x + (this.Width / 2), start_y + (this.Height / 2)));
                else if (this.OutofIssue == true)
                    x.RotateAt(-45, new PointF(start_x + (this.Width / 2), start_y + (this.Height / 2)));
                else
                    x.RotateAt(10, new PointF(start_x + (this.Width / 2), start_y + (this.Height / 2)));
                e.Graphics.Transform = x;
            }

            // 普通单册
            Color colorText = this.Container.Container.SingleForeColor;
            Color colorGray = this.Container.Container.SingleGrayColor;
            float fBorderWidth = 1; // 2
            Color colorBorder = Color.FromArgb(255, Color.Gray);

            {
                Brush brushBack = null;
                try
                {
                    // 背景
                    if (bSelected == true)
                    {
                        // 选定了的格子
                        Color colorBack = this.Container.Container.SelectedBackColor;
                        if (this.m_bFocus == true)
                        {
                            // 左 -- 右
                            brushBack = new LinearGradientBrush(
                new PointF(start_x, start_y + this.Height),
                new PointF(start_x + this.Width, start_y),
                Color.FromArgb(120, colorBack),
                Color.FromArgb(255, ControlPaint.Dark(colorBack))   // 0-150
                );
                        }
                        else
                        {
                            brushBack = new SolidBrush(colorBack);
                        }
                        colorText = this.Container.Container.SelectedForeColor;
                        colorGray = this.Container.Container.SelectedGrayColor;
                    }
                    else if (this.IsMember == true)
                    {
                        // 成员册
#if DEBUG
                        if (this.item != null)
                        {
                            Debug.Assert(this.item.IsMember == true, "");
                        }
#endif
                        fBorderWidth = 1;
                        Color colorBack = this.Container.Container.MemberBackColor;
                        if (this.m_bFocus == true)
                        {
                            // 左 -- 右
                            brushBack = new LinearGradientBrush(
                new PointF(start_x, start_y + this.Height),
                new PointF(start_x + this.Width, start_y),
                Color.FromArgb(0, colorBack),
                Color.FromArgb(150, ControlPaint.Dark(colorBack))
                );
                        }
                        else
                        {
                            brushBack = new SolidBrush(colorBack);
                        }

                        colorText = this.Container.Container.MemberForeColor;
                        colorGray = this.Container.Container.MemberGrayColor;
                    }
                    else if (this.item != null
                        && this.item.Calculated == true)
                    {
                        // 预测的单册
                        fBorderWidth = (float)1;
                        Color colorBack = this.Container.Container.CalculatedBackColor;
                        if (this.m_bFocus == true)
                        {
                            // 左 -- 右
                            brushBack = new LinearGradientBrush(
                new PointF(start_x, start_y + this.Height),
                new PointF(start_x + this.Width, start_y),
                Color.FromArgb(0, colorBack),
                Color.FromArgb(150, ControlPaint.Dark(colorBack))
                );
                        }
                        else
                        {
                            brushBack = new SolidBrush(colorBack);
                        }
                        colorText = this.Container.Container.CalculatedForeColor;
                        colorGray = this.Container.Container.CalculatedGrayColor;
                        colorBorder = Color.FromArgb(255, Color.White);
                    }
                    else if (this.item != null
                   && this.item.IsParent == true)
                    {
                        // 合订本
                        fBorderWidth = 1;   // 3
                        Color colorBack = this.Container.Container.ParentBackColor;
                        if (this.m_bFocus == true)
                        {
                            // 左 -- 右
                            brushBack = new LinearGradientBrush(
                new PointF(start_x, start_y + this.Height),
                new PointF(start_x + this.Width, start_y),
                Color.FromArgb(0, colorBack),
                Color.FromArgb(150, ControlPaint.Dark(colorBack))
                );
                        }
                        else
                        {
                            brushBack = new SolidBrush(colorBack);
                        }

                        colorText = this.Container.Container.ParentForeColor;
                        colorGray = this.Container.Container.ParentGrayColor;
                    }
                    else
                    {
                        brushBack = null;
                        Color colorBack = this.Container.Container.SingleBackColor;
                        if (this.m_bFocus == true)
                        {
                            // 左 -- 右
                            brushBack = new LinearGradientBrush(
                new PointF(start_x, start_y + this.Height),
                new PointF(start_x + this.Width, start_y),
                Color.FromArgb(0, colorBack),   // 0
                Color.FromArgb(100, ControlPaint.Dark(colorBack))   // 255
                );
                        }
                        else
                        {
                            brushBack = new SolidBrush(colorBack);
                        }
                    }

                    Color colorSideBar = Color.FromArgb(0, 255, 255, 255);

                    // 新建的和发生过修改的，侧边条颜色需要设定
                    if (this.item != null
                        && this.item.NewCreated == true)
                    {
                        // 新创建的单册
                        colorSideBar = this.Container.Container.NewBarColor;
                    }
                    else if (this.item != null
                   && this.item.Changed == true)
                    {
                        // 修改过的的单册
                        colorSideBar = this.Container.Container.ChangedBarColor;
                    }

                    // 边框和边条
                    {

                        rect = new RectangleF(start_x,
                            start_y,
                            this.Container.Container.m_nCellWidth,
                            this.Container.Container.m_nCellHeight);

                        {
                            // 没有焦点时要小一些
                            rect = GuiUtil.PaddingRect(this.Container.Container.CellMargin,
                                rect);
                        }
                        // rect = RectangleF.Inflate(rect, -4, -4);

                        RectangleF rectTest = rect;
                        rectTest.Inflate(1, 1);
                        rectTest.Width += 2;
                        rectTest.Height += 2;

                        // 优化
                        if (rectTest.IntersectsWith(e.ClipRectangle) == true
                            || bRotate == true)
                        {
                            float round_radius = 10;

                            // 阴影
                            RectangleF rectShadow = rect;
                            rectShadow.Offset((float)1.5, (float)1.5);  // 0.5
                            // rectShadow.Inflate((float)-0.9, (float)-0.9);
                            // Pen penShadow = new Pen(Color.FromArgb(160, 190,190,180),5);
                            Brush brushShadow = new SolidBrush(Color.FromArgb(160, 190, 190, 180));
                            BindingControl.RoundRectangle(e.Graphics,
                                null,
                                brushShadow,
                                rectShadow,
                                round_radius);

                            float que_radius = 0;
                            if (this.item != null)
                            {
                                string strIntact = this.item.GetText("intact");
                                float r = GetIntactRatio(strIntact);
                                if (r < (float)1.0)
                                {
                                    float h = Math.Min(rect.Width, rect.Height) - (round_radius + fBorderWidth);
                                    que_radius = h - (h * r);
                                    if (que_radius < round_radius)
                                        que_radius = round_radius;
                                }
                            }

                            rect.Inflate(-(fBorderWidth / 2), -(fBorderWidth / 2));
                            using (Pen penBorder = new Pen(colorBorder, fBorderWidth))
                            {
                                penBorder.LineJoin = LineJoin.Bevel;
                                if (que_radius == 0)
                                    BindingControl.RoundRectangle(e.Graphics,
                                        penBorder,
                                        brushBack,
                                        rect,
                                        round_radius);
                                else
                                    BindingControl.QueRoundRectangle(e.Graphics,
                                        penBorder,
                                        brushBack,
                                        rect,
                                        round_radius,
                                        que_radius);
                            }

                            // 边条。左侧
                            Brush brushSideBar = new SolidBrush(colorSideBar);
                            RectangleF rectSideBar = new RectangleF(
                                rect.X + fBorderWidth,  // + penBorder.Width,
                                rect.Y + 10,
                                10 / 2,
                                rect.Height - 2 * 10);
                            e.Graphics.FillRectangle(brushSideBar, rectSideBar);
                        }
                    }
                }
                finally
                {
                    if (brushBack != null)
                    {
                        brushBack.Dispose();
                        brushBack = null;
                    }
                }
            }

            // 绘制文字
            if (this.item != null)
            {
                Debug.Assert(this.item.Missing == false, "Missing为true的Item对象应该在初始化刚结束时就丢弃");

                // 锁定状态的单元
                if (this.item.Locked == true && this.IsMember == false)
                {
                    Padding margin = this.Container.Container.CellMargin;
                    Padding padding = this.Container.Container.CellPadding;

                    float nLittleWidth = Math.Min(this.Width - margin.Horizontal - padding.Horizontal,
                        this.Height - margin.Vertical - padding.Vertical);

                    RectangleF rectMask = new RectangleF(
                        //start_x + this.Width / 2 - nLittleWidth / 2,
                        //start_y + this.Height / 2 - nLittleWidth / 2,
                        start_x + this.Width / 2 - nLittleWidth / 2,
                        start_y + this.Height- nLittleWidth,
                        nLittleWidth,
                        nLittleWidth);
                    PaintLockedMask(rectMask,
                        this.item.Calculated == true ? ControlPaint.Dark(colorGray) : colorGray,
                        e,
                        bRotate);
                }

                if (this.item.Calculated == true)
                {
                    // 绘制淡色的“?”字样
                    this.PaintQue(start_x,
                        start_y,
                        "?",
                        colorGray,
                        e,
                        bRotate);
                }
                // “数据库记录”已经删除的单元
                if (this.item.Deleted == true)
                {
                    /*
                    float nCenterX = start_x + (this.Container.Container.m_nCellWidth / 2);
                    float nCenterY = start_y + (this.Container.Container.m_nCellHeight / 2);
                    float nWidth = Math.Min(this.Container.Container.m_nCellWidth,
                        this.Container.Container.m_nCellHeight);
                    nWidth = nWidth * (float)0.6;
                    rect = new RectangleF(nCenterX - nWidth/2,
    nCenterY - nWidth/2,
    nWidth,
    nWidth);
                    rect = PaddingRect(this.Container.Container.CellMargin,
                        rect);
                    rect = RectangleF.Inflate(rect, -4, -4);

                    Pen pen = new Pen(Color.LightGray,
                        nWidth/10);
                    e.Graphics.DrawArc(pen, rect, 0, 360);
                    e.Graphics.DrawLine(pen, new PointF(rect.X, rect.Y + rect.Height / 2),
                    new PointF(rect.X + rect.Width, rect.Y + rect.Height / 2));
                     * */
                    Padding margin = this.Container.Container.CellMargin;
                    Padding padding = this.Container.Container.CellPadding;

                    float nLittleWidth = Math.Min(this.Width - margin.Horizontal - padding.Horizontal,
                        this.Height - margin.Vertical - padding.Vertical);

                    RectangleF rectMask = new RectangleF(
                        start_x + this.Width/2 - nLittleWidth/2,
                        start_y + this.Height/2 - nLittleWidth/2,
                        nLittleWidth,
                        nLittleWidth);
                    PaintDeletedMask(rectMask,
                        colorGray,
                        e,
                        bRotate);
                }

                // 被人借阅的格子
                if (String.IsNullOrEmpty(this.item.Borrower) == false)
                {
                    Padding margin = this.Container.Container.CellMargin;
                    Padding padding = this.Container.Container.CellPadding;

                    float nLittleWidth = Math.Min(this.Width - margin.Horizontal - padding.Horizontal,
                        this.Height - margin.Vertical - padding.Vertical);

                    RectangleF rectMask = new RectangleF(
                        start_x + this.Width / 2 - nLittleWidth / 2,
                        start_y + this.Height / 2 - nLittleWidth / 2,
                        nLittleWidth,
                        nLittleWidth);
                    PaintBorrowedMask(rectMask,
                        colorGray,
                        e,
                        bRotate);
                }

                if (StringUtil.IsInList("注销", this.item.State) == true)
                {
                    this.PaintTextLines(start_x, start_y, true,
                        colorText,
                        e, bRotate);
                }
                else
                {
                    this.PaintTextLines(start_x, start_y, false,
                        colorText,
                        e, bRotate);
                }

                if (this.m_bHover == true
                    && this.m_bDisplayCheckBox == false)    // 要显示checkbox，就不要显示行标题
                    this.PaintLineLabels(start_x, start_y, e, bRotate);

                // 绑定了采购信息的格子，显示xy值
                if (this.Container.Container.DisplayOrderInfoXY == true
                    && this.item != null && this.item.OrderInfoPosition.X != -1)
                {
                    string strText = this.item.OrderInfoPosition.X.ToString() + "," + this.item.OrderInfoPosition.Y.ToString();

                    Font font = this.Container.Container.m_fontLine;
                    SizeF size = e.Graphics.MeasureString(strText, font);

                    rect = new RectangleF(start_x,
                        start_y,
                        this.Width,
                        this.Height);
                    rect = GuiUtil.PaddingRect(this.Container.Container.CellMargin,
                        rect);
                    rect = GuiUtil.PaddingRect(this.Container.Container.CellPadding,
                        rect);

                    // 右上角
                    RectangleF rectText = new RectangleF(
                        rect.X + rect.Width - size.Width,
                        rect.Y,
                        size.Width,
                        size.Height);

                    StringFormat stringFormat = new StringFormat();
                    stringFormat.Alignment = StringAlignment.Near;
                    stringFormat.LineAlignment = StringAlignment.Near;
                    stringFormat.FormatFlags = StringFormatFlags.NoWrap;

                    using (Brush brushText = new SolidBrush(this.Container.Container.ForeColor))
                    {
                        e.Graphics.DrawString(strText,
                            font,
                            brushText,
                            rectText,
                            stringFormat);
                    }
                }
            }
            else
            {
                /*
                // 空白格子。绘制淡色的“缺”字样
                this.PaintQue(start_x,
                    start_y,
                    "缺",
                    colorGray,
                    e,
                    bRotate);
                 * */
                {
                    Padding margin = this.Container.Container.CellMargin;
                    Padding padding = this.Container.Container.CellPadding;

                    float nLittleWidth = Math.Min(this.Width - margin.Horizontal - padding.Horizontal,
                        this.Height - margin.Vertical - padding.Vertical);

                    RectangleF rectMask = new RectangleF(
                        start_x + this.Width / 2 - nLittleWidth / 2,
                        start_y + this.Height / 2 - nLittleWidth / 2,
                        nLittleWidth,
                        nLittleWidth);
                    PaintMissingMask(rectMask,
                        colorGray,
                        e,
                        bRotate);
                }

#if NOOOOOOOOOOOOOOO
                rect = new RectangleF(start_x,
                    start_y,
                    this.Container.Container.m_nCellWidth,
                    this.Container.Container.m_nCellHeight);
                rect = PaddingRect(this.Container.Container.CellMargin,
                    rect);
                float nWidthDelta = (rect.Width / 4);
                float nHeightDelta = (rect.Height / 4);
                rect = RectangleF.Inflate(rect, -nWidthDelta, -nHeightDelta);

                // 优化
                if (rect.IntersectsWith(e.ClipRectangle) == true)
                {
                    /*
                    GraphicsState gstate = e.Graphics.Save();
                    e.Graphics.TranslateTransform(rect.X+(rect.Width/2), rect.Y+(rect.Height/2));
                    e.Graphics.RotateTransform(-10, MatrixOrder.Append);
                     * */

                    StringFormat stringFormat = new StringFormat();
                    stringFormat.Alignment = StringAlignment.Center;
                    stringFormat.LineAlignment = StringAlignment.Center;
                    stringFormat.FormatFlags = StringFormatFlags.NoWrap;

                    /*
                    rect.X = rect.Width / 2;
                    rect.Y = rect.Height / 2;
                     * */
                    e.Graphics.DrawString("缺",
    new Font("微软雅黑",    // "Arial",
        rect.Height,
        FontStyle.Regular,
        GraphicsUnit.Pixel),
    new SolidBrush(Color.LightGray),
    rect,
    stringFormat);


                    /*
                    e.Graphics.DrawString("缺",
                        new Font("微软雅黑",    // "Arial",
                            rect.Height,
                            FontStyle.Regular,
                            GraphicsUnit.Pixel),
                        new SolidBrush(Color.LightGray),
                        rect,
                        stringFormat);
                     * */
                    /*
                    e.Graphics.DrawString("缺",
    new Font("微软雅黑",    // "Arial",
        rect.Height,
        FontStyle.Regular,
        GraphicsUnit.Pixel),
    new SolidBrush(Color.LightGray),
    0,
    0);
                     * */
                    /// e.Graphics.Restore(gstate);

                }
#endif
            }


            // 焦点虚线
            if (this.m_bFocus == true)
            {
                rect = new RectangleF(
    start_x,
    start_y,
    this.Width,
    this.Height);
                rect.Inflate(-1, -1);
                /*
                 rect = PaddingRect(this.Container.Container.CellMargin,
     rect);

                 rect.Inflate(-4, -4);
                  * */
                ControlPaint.DrawFocusRectangle(e.Graphics,
                    Rectangle.Round(rect));
            }

            if (gstate != null)
            {
                Debug.Assert(bRotate == true);
                e.Graphics.Restore(gstate);
            }

            // 移动把手，不要旋转。因为旋转会带来点击的不一致
            if (this.m_bHover == true)
            {
                // 把手
                Rectangle rect1 = this.Container.Container.RectGrab;
                rect1.Offset((int)start_x, (int)start_y);
                ControlPaint.DrawContainerGrabHandle(
        e.Graphics,
        rect1);

                // checkbox
                if (this.item != null
                    && this.m_bDisplayCheckBox == true)
                {
                    long nCenterX = start_x + this.Width / 2;
                    long nCenterY = start_y + this.Height / 2;
                    int nWidth = this.Container.Container.RectGrab.Width;
                    Rectangle rectCheckBox = new Rectangle(
                        (int)nCenterX - nWidth/2,
                        (int)nCenterY - nWidth/2,
                        nWidth,
                        nWidth);

                    // 显示半透明的遮罩
                    RectangleF rectShadow = rectCheckBox;
                    int nDelta = this.Width / 8;
                    rectShadow.Inflate(nDelta, nDelta);
                    using (Pen pen = new Pen(Color.FromArgb(100, 200, 200, 200), 2))
                    using(Brush brush = new SolidBrush(Color.FromArgb(150, 200, 200, 200)))
                    {
                        BindingControl.Circle(e.Graphics,
                            pen,
                            brush,
                            rectShadow);
                    }

                    if (this.item.Calculated == true)
                    {
                        ControlPaint.DrawCheckBox(e.Graphics,
                            rectCheckBox,
                            ButtonState.Normal);
                    }
                    else if (this.item.OrderInfoPosition.X != -1
                        && this.item.NewCreated == true)
                    {
                        ControlPaint.DrawCheckBox(e.Graphics,
                            rectCheckBox,
                            ButtonState.Checked);
                    }
                }
            }
        }

        // 绘制“缺期”标志
        internal static void PaintMissingMask(RectangleF rect,
            Color colorGray,
            PaintEventArgs e,
            bool bDoNotSpeedUp)
        {
            float delta = (rect.Width / 2) * (float)0.35;
            rect.Inflate(-delta, -delta);
            // 优化
            if (rect.IntersectsWith(e.ClipRectangle) == true
                    || bDoNotSpeedUp == true)
            {
                using (Pen pen = new Pen(colorGray,
                    rect.Width / 10))
                {
                    rect.Inflate(-pen.Width / 2, -pen.Width / 2);

                    float start = 0;
                    for (int i = 0; i < 10; i++)
                    {
                        e.Graphics.DrawArc(pen, rect, start, 18);
                        start += 36;
                    }
                }
            }
        }

        // 绘制“被人借阅”标志
        internal static void PaintBorrowedMask(RectangleF rect,
            Color colorGray,
            PaintEventArgs e,
            bool bDoNotSpeedUp)
        {
            float delta = (rect.Width / 2) * (float)0.35;
            rect.Inflate(-delta, -delta);
            // 优化
            if (rect.IntersectsWith(e.ClipRectangle) == true
                    || bDoNotSpeedUp == true)
            {
                using (Pen pen = new Pen(colorGray,
                    rect.Width / 10))
                {
                    pen.LineJoin = LineJoin.Bevel;

                    rect.Inflate(-pen.Width / 2, -pen.Width / 2);

                    float up_height = rect.Width / 2;
                    float down_height = up_height;

                    using (GraphicsPath path = new GraphicsPath())
                    {
                        // 头部
                        RectangleF rectUp = new RectangleF(rect.X + rect.Width / 2 - up_height / 2,
                            rect.Y, up_height, up_height);
                        path.AddArc(rectUp, 0, 360);

                        // 身体
                        RectangleF rectDown = new RectangleF(rect.X,
                            rect.Y + up_height, rect.Width, down_height * 2);
                        path.AddArc(rectDown, 180, 180);

                        e.Graphics.DrawPath(pen, path);
                    }
                }
            }
        }

        // 绘制“记录已删除”标志
        internal static void PaintLockedMask(RectangleF rect,
            Color colorGray,
            PaintEventArgs e,
            bool bDoNotSpeedUp)
        {
            float delta = (rect.Width / 2) * (float)0.35;
            rect.Inflate(-delta, -delta);
            // 优化
            if (rect.IntersectsWith(e.ClipRectangle) == true
                    || bDoNotSpeedUp == true)
            {
                using (Pen pen = new Pen(colorGray,
                    rect.Width / 10))
                {
                    pen.LineJoin = LineJoin.Bevel;

                    rect.Inflate(-pen.Width / 2, -pen.Width / 2);
                    float little = rect.Width / 7;

                    RectangleF rectBox = new RectangleF(rect.X + little, rect.Y + rect.Height / 2, rect.Width - little * 2, rect.Height / 2);

                    using (GraphicsPath path = new GraphicsPath())
                    {
                        RectangleF rectArc = new RectangleF(rect.X + rect.Width / 4, rect.Y, rect.Width / 2, rect.Height / 2);
                        // 左边竖线
                        path.AddLine(rect.X + rect.Width / 4, rect.Y + rect.Height / 2,
                            rect.X + rect.Width / 4, rect.Y + +rect.Height / 4);
                        // 半圆弧
                        path.AddArc(rectArc, 180, 180);
                        // 右边竖线
                        path.AddLine(rect.X + rect.Width - rect.Width / 4, rect.Y + +rect.Height / 4,
                            rect.X + rect.Width - rect.Width / 4, rect.Y + rect.Height / 2);
                        // 
                        path.AddRectangle(rectBox);

                        e.Graphics.DrawPath(pen, path);
                    }
                }
            }
        }

        // 绘制“记录已删除”标志
        internal static void PaintDeletedMask(RectangleF rect,
            Color colorGray,
            PaintEventArgs e,
            bool bDoNotSpeedUp)
        {
            float delta = (rect.Width / 2) * (float)0.35;
            rect.Inflate(-delta, -delta);
            // 优化
            if (rect.IntersectsWith(e.ClipRectangle) == true
                    || bDoNotSpeedUp == true)
            {
                using (Pen pen = new Pen(colorGray,
                    rect.Width / 10))
                {
                    pen.LineJoin = LineJoin.Bevel;

                    rect.Inflate(-pen.Width / 2, -pen.Width / 2);
                    float little = rect.Width / 7;

                    using (GraphicsPath path = new GraphicsPath())
                    {

                        path.AddArc(rect, 20, 360 - 20);

                        path.AddLine(
                            new PointF(rect.X + rect.Width - little - little / 2, rect.Y + rect.Height / 2 - little),
            new PointF(rect.X + rect.Width, rect.Y + rect.Height / 2));
                        path.AddLine(
                            new PointF(rect.X + rect.Width, rect.Y + rect.Height / 2),
        new PointF(rect.X + rect.Width + little, rect.Y + rect.Height / 2 - little - little / 2));

                        e.Graphics.DrawPath(pen, path);
                    }
                }
            }
        }

        internal virtual void PaintQue(float start_x,
            float start_y,
            string strText,
            Color colorGray,
            PaintEventArgs e,
            bool bDoNotSpeedUp)
        {
            Debug.Assert(String.IsNullOrEmpty(strText) == false, "");

            RectangleF rect = new RectangleF(start_x,
    start_y,
    this.Container.Container.m_nCellWidth,
    this.Container.Container.m_nCellHeight);
            rect = GuiUtil.PaddingRect(this.Container.Container.CellMargin,
                rect);
            float nWidthDelta = (rect.Width / 4);
            float nHeightDelta = (rect.Height / 4);
            rect = RectangleF.Inflate(rect, -nWidthDelta, -nHeightDelta);

            // 优化
            if (rect.IntersectsWith(e.ClipRectangle) == true
                    || bDoNotSpeedUp == true)
            {
                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Center;
                stringFormat.FormatFlags = StringFormatFlags.NoWrap;

                using(Font font = new Font("微软雅黑",    // "Arial",
    rect.Height,
    FontStyle.Regular,
    GraphicsUnit.Pixel))
                using (Brush brush = new SolidBrush(colorGray))
                {
                    e.Graphics.DrawString(strText,
    font,
    brush,
    rect,
    stringFormat);
                }
            }
        }


        internal virtual void PaintTextLines(float x0,
            float y0,
            bool bGrayText,
            Color colorText,
            PaintEventArgs e,
            bool bDoNotSpeedUp)
        {
            int nLineHeight = this.LineHeight;
            Padding margin = this.Container.Container.CellMargin;
            Padding padding = this.Container.Container.CellPadding;
            x0 += margin.Left + padding.Left;
            y0 += margin.Top + padding.Top;
            int nWidth = this.Width - margin.Horizontal - padding.Horizontal;
            int nHeight = this.Height - margin.Vertical - padding.Vertical;

            int nUsedHeight = 0;    // 使用过的累积高度
            // Color colorText = this.Container.Container.MemberForeColor;

            if (bGrayText == true)
                colorText = ControlPaint.Light(colorText, 1.5F);

            // 左 -- 右
            using (LinearGradientBrush brushGradient = new LinearGradientBrush(
new PointF(x0, 0),
new PointF(x0 + 6, 0),
Color.FromArgb(255, Color.Gray),
Color.FromArgb(0, Color.Gray)
))
            {
                Font font = this.Container.Container.m_fontLine;

                using (Pen penLine = new Pen(brushGradient, (float)1))
                {
                    for (int i = 0; i < this.Container.Container.TextLineNames.Length / 2; i++)
                    {
                        int nRestHeight = nHeight - nUsedHeight;

                        // 绘制行
                        RectangleF rect = new RectangleF(
                            x0,
                            y0,
                            nWidth,
                            Math.Min(this.LineHeight, nRestHeight));

                        // 优化
                        if (rect.IntersectsWith(e.ClipRectangle) == true
                            || bDoNotSpeedUp == true)
                        {
                            string strName = this.Container.Container.TextLineNames[i * 2];

                            string strText = this.item.GetText(strName);
                            if (strName == "intact")
                            {
                                // 预测格子，没有必要显示完好率
                                if (this.item.Calculated == false)
                                {
                                    PaintIntactBar(
                                    x0,
                                    y0,
                                    nWidth,
                                    Math.Min(nLineHeight, nRestHeight),
                                    strText,
                                    colorText,
                                    e);
                                }
                            }
                            else
                            {
                                PaintText(
                                    x0,
                                    y0,
                                    nWidth,
                                    Math.Min(nLineHeight, nRestHeight),
                                    strText,
                                    colorText,
                                    font,
                                    e);
                            }

                            // 下方线条
                            if (nLineHeight < nRestHeight)
                            {
                                e.Graphics.DrawLine(penLine,
                                    new PointF(rect.X, rect.Y + nLineHeight - 1),
                                    new PointF(rect.X + 5, rect.Y + nLineHeight - 1));
                            }
                        }


                        y0 += nLineHeight;
                        nUsedHeight += nLineHeight;

                        if (nUsedHeight > nHeight)
                            break;
                    }
                }
            }
        }

        static float GetIntactRatio(string strIntact)
        {
            if (String.IsNullOrEmpty(strIntact) == true)
                return (float)1.0;

            strIntact = strIntact.Replace("%", "");

            float r = (float)1.0;

            try
            {
                r = (float)Convert.ToDecimal(strIntact) / (float)100;
            }
            catch
            {
                return 0;
            }

            if (r > 1.0)
                r = (float)1.0;
            if (r < 0)
                r = 0;

            return r;
        }

        internal virtual void PaintIntactBar(float x0,
            float y0,
            int nWidth,
            int nHeight,
            string strIntact,
            Color colorText,
            PaintEventArgs e)
        {
            if (String.IsNullOrEmpty(strIntact) == true)
                strIntact = "100";
            else
                strIntact = strIntact.Replace("%", "");

            float r = (float)1.0;

            try
            {
                r = (float)Convert.ToDecimal(strIntact) / (float)100;
            }
            catch
            {
                r = 0;
                strIntact = "error '" + strIntact + "'";
            }

            if (r > 1.0)
                r = (float)1.0;
            if (r < 0)
                r = 0;

            int nLeftWidth = (int)((float)nWidth * r);
            if (nLeftWidth > 0)
            {
                // 左 -- 右
                using (LinearGradientBrush brushGradient = new LinearGradientBrush(
    new PointF(x0, y0),
    new PointF(x0 + nLeftWidth, y0 + nHeight),
    Color.FromArgb(100, Color.Gray),
    Color.FromArgb(255, Color.Gray)
    ))
                {
                    //  Brush brushLeft = new SolidBrush(Color.Gray);
                    RectangleF rectLeft = new RectangleF(x0, y0, nLeftWidth, nHeight);
                    e.Graphics.FillRectangle(brushGradient,
                        rectLeft);
                }
            }

            int nRightWidth = nWidth - nLeftWidth;
            if (nRightWidth > 0)
            {
                using (Brush brushRight = new SolidBrush(Color.FromArgb(100, Color.LightGray)))
                {
                    RectangleF rectRight = new RectangleF(x0 + nLeftWidth, y0, nRightWidth, nHeight);
                    e.Graphics.FillRectangle(brushRight,
                        rectRight);
                }
            }

            // 白色，粗体
            PaintText(
x0,
y0,
nWidth,
nHeight,
strIntact,
Color.White,
           new Font(this.Container.Container.m_fontLine, FontStyle.Bold),

e);
            /*
            PaintText(
    x0,
    y0,
    nWidth,
    nHeight,
    strIntact,
    colorText,
               this.Container.Container.m_fontLine,
    e);
             * */
        }

        internal virtual void PaintLineLabels(float x0,
            float y0,
            PaintEventArgs e,
            bool bDoNotSpeedUp)
        {
            int nLineHeight = this.LineHeight;

            Padding margin = this.Container.Container.CellMargin;
            Padding padding = this.Container.Container.CellPadding;
            x0 += margin.Left + padding.Left;
            y0 += margin.Top + padding.Top;
            int nWidth = this.Width - margin.Horizontal - padding.Horizontal;
            int nHeight = this.Height - margin.Vertical - padding.Vertical;

            int nUsedHeight = 0;    // 使用过的累积高度
            Color colorText = Color.FromArgb(200, 0, 100, 0);

            // 获得文字的最大宽度
            float fMaxTextWidth = 0;
            Font ref_font = this.Container.Container.m_fontLine;
            using (Font font = new Font(ref_font, FontStyle.Bold))
            {
                for (int i = 0; i < this.Container.Container.TextLineNames.Length / 2; i++)
                {
                    string strLabel = this.Container.Container.TextLineNames[i * 2 + 1];
                    SizeF size = e.Graphics.MeasureString(strLabel, font);
                    if (size.Width > fMaxTextWidth)
                        fMaxTextWidth = size.Width;
                }

                // 绘制半透明背景
                {
                    RectangleF rect1 = new RectangleF(
                        x0 + nWidth - (fMaxTextWidth + 4),
                        y0,
                        fMaxTextWidth + 4,
                        nHeight);

                    if (rect1.IntersectsWith(e.ClipRectangle) == true
                        || bDoNotSpeedUp == true)
                    {

                        // 左 -- 右
                        using (LinearGradientBrush brushGradient = new LinearGradientBrush(
        new PointF(rect1.X, rect1.Y),
        new PointF(rect1.X + rect1.Width, rect1.Y),
        Color.FromArgb(150, Color.White),
        Color.FromArgb(200, Color.White)
        ))
                        {
                            e.Graphics.FillRectangle(brushGradient,
                                rect1);
                        }
                    }
                }

                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Far;
                stringFormat.LineAlignment = StringAlignment.Near;
                stringFormat.FormatFlags = StringFormatFlags.NoWrap;

                using (Brush brushText = new SolidBrush(colorText))
                using (Pen penLine = new Pen(colorText, (float)1))
                {
                    for (int i = 0; i < this.Container.Container.TextLineNames.Length / 2; i++)
                    {
                        int nRestHeight = nHeight - nUsedHeight;

                        // 绘制行
                        RectangleF rect = new RectangleF(
                            x0 + nWidth - fMaxTextWidth,
                            y0,
                            fMaxTextWidth,
                            Math.Min(nLineHeight, nRestHeight));

                        // 优化
                        if (rect.IntersectsWith(e.ClipRectangle) == true
                            || bDoNotSpeedUp == true)
                        {
                            /*
                            PaintText(
                                x0,
                                y0,
                                nWidth,
                                Math.Min(this.LineHeight, nRestHeight),
                                strLabel,
                                colorText,
                                stringFormat,
                                e);
                             * */
                            string strLabel = this.Container.Container.TextLineNames[i * 2 + 1];


                            e.Graphics.DrawString(strLabel,
                                font,
                                brushText,
                                rect,
                                stringFormat);

                            // 下方线条
                            if (nLineHeight < nRestHeight)
                            {
                                e.Graphics.DrawLine(penLine,
                                    new PointF(rect.X, rect.Y + nLineHeight - 1),
                                    new PointF(rect.X + rect.Width, rect.Y + nLineHeight - 1));
                            }
                        }


                        y0 += nLineHeight;
                        nUsedHeight += nLineHeight;

                        if (nUsedHeight > nHeight)
                            break;
                    }
                }
            }
        }

        /*
        //
        void PaintText(
    int x0,
    int y0,
    int nMaxWidth,
    int nHeight,
    string strText,
            Font font,
            Color colorText,
            StringFormat stringFormat,
            PaintEventArgs e)
        {
            Brush brushText = null;

            brushText = new SolidBrush(colorText);
            SizeF size = e.Graphics.MeasureString(strText, font);
            RectangleF rect = new RectangleF(
                x0,
                y0,
                Math.Min(size.Width, nMaxWidth),
                Math.Min(nHeight, size.Height));

            e.Graphics.DrawString(strText,
                font,
                brushText,
                rect,
                stringFormat);
        }*/

        void PaintText(
            RectangleF rect,
            string strText,
            Color colorText,
            Font font,
            StringFormat stringFormat,
            PaintEventArgs e)
        {
            using (Brush brushText = new SolidBrush(colorText))
            {
                SizeF size = e.Graphics.MeasureString(strText, font);

                e.Graphics.DrawString(strText,
                    font,
                    brushText,
                    rect,
                    stringFormat);
            }
        }

        internal void PaintText(
            float x0,
            float y0,
            int nMaxWidth,
            int nHeight,
            string strText,
            Color colorText,
            Font font,
            PaintEventArgs e)
        {
            using (Brush brushText = new SolidBrush(colorText))
            {
                SizeF size = e.Graphics.MeasureString(strText, font);
                RectangleF rect = new RectangleF(
                    x0,
                    y0,
                    Math.Min(size.Width, nMaxWidth),
                    Math.Min(nHeight, size.Height));

                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Near;
                stringFormat.LineAlignment = StringAlignment.Near;
                stringFormat.FormatFlags = StringFormatFlags.NoWrap;

                e.Graphics.DrawString(strText,
                    font,
                    brushText,
                    rect,
                    stringFormat);
            }
        }

    }

    // 比较出版日期。小的在前
    internal class CellPublishTimeComparer : IComparer<Cell>
    {
        int IComparer<Cell>.Compare(Cell x, Cell y)
        {
            string s1 = x.Container.PublishTime;
            string s2 = y.Container.PublishTime;

            int nRet = String.Compare(s1, s2);
            if (nRet == 0)
            {
                // 如果出版日期相同，则把空白格子排在前面
                // 这样做的好处是，让空白格子先处理，便于它们被后面的其他同期格子覆盖
                if (x.item == null && y.item == null)
                    return 0;
                if (x.item == null)
                    return -1;
                return 1;
            }

            return nRet;
        }
    }


    // 一个订购组显示单元
    // 显示订购信息：书商、资金来源、价格、时间范围
    internal class GroupCell : Cell
    {
        public OrderBindingItem order = null;

        public bool EndBracket = false; // == false 引导的对象，左括号；==true，右边的括号

        // 本对象同组的组头部对象
        public GroupCell HeadGroupCell
        {
            get
            {
                Debug.Assert(this.EndBracket == true, "只能对尾部对象使用HeadGroupCell");
                IssueBindingItem issue = this.Container;
                Debug.Assert(issue != null, "");
                int index = issue.IndexOfCell(this);
                Debug.Assert(index != -1, "");
                for (int i = index-1; i >= 0; i--)
                {
                    Cell cell = issue.Cells[i];
                    if (cell == null)
                    {
                        Debug.Assert(false, "");
                        continue;
                    }

                    if (cell is GroupCell)
                    {
                        Debug.Assert(((GroupCell)cell).EndBracket == false, "");
                        return (GroupCell)cell;
                    }
                }

                return null;
            }
        }

        public List<Cell> MemberCells
        {
            get
            {
                Debug.Assert(this.EndBracket == false, "只能对头部对象使用MemberCells");
                return GetMemberCells(0x03);
            }
        }

        // parameters:
        //      0x01    预测的
        //      0x02    已经到的
        //      0x03    全部
        List<Cell> GetMemberCells(int nStyle)
        {
            Debug.Assert(this.EndBracket == false, "只能对头部对象使用GetMemberCells()");
            List<Cell> results = new List<Cell>();
            IssueBindingItem issue = this.Container;
            if (issue == null)
            {
                Debug.Assert(issue != null, "");
                return results;
            }
            int index = issue.IndexOfCell(this);
            if (index == -1)
            {
                Debug.Assert(false, "");
                return results;
            }
            for (int i = index + 1; i < issue.Cells.Count; i++)
            {
                Cell cell = issue.GetCell(i);
                if (cell is GroupCell)
                    break;
                if ((nStyle & 0x01) != 0)
                {
                    if (cell != null && cell.item != null
        && cell.item.Calculated == true)
                    {
                        results.Add(cell);
                        continue;
                    }
                }
                if ((nStyle & 0x02) != 0)
                {
                    if (cell != null && cell.item != null
        && cell.item.Calculated == false)
                    {
                        results.Add(cell);
                        continue;
                    }
                }
            }

            return results;
        }

        // 刷新组内每个格子的OrderInfoXY信息
        internal void RefreshGroupMembersOrderInfo(int nOrderCountDelta,
            int nArrivedCountDelta)
        {
            string strError = "";
            Debug.Assert(this.EndBracket == false, "只能对头部对象使用RefreshOrderInfoXY()");
            IssueBindingItem issue = this.Container;
            if (issue == null)
            {
                Debug.Assert(issue != null, "");
                strError = "this.Container == null";
                throw new Exception(strError);
            }

            int head_index = issue.IndexOfCell(this);
            if (head_index == -1)
            {
                Debug.Assert(false, "");
                strError = "在容器的.Cells集合中没有找到自己";
                throw new Exception(strError);
            }

            int nOrderIndex = issue.OrderItems.IndexOf(this.order);
            Debug.Assert(nOrderIndex != -1, "");

            // 将同一组中全部格子的订购信息定位参数刷新
            int y = 0;
            for (int i = head_index + 1; i < issue.Cells.Count; i++)
            {
                Cell cell = issue.GetCell(i);
                if (cell is GroupCell)
                    break;
                Debug.Assert(cell != null, "");
                Debug.Assert(cell.item != null, "");
                if (cell.item != null)
                {
                    cell.item.OrderInfoPosition = new Point(nOrderIndex, y);
                }
                y++;
            }
            bool bChanged = false;

            if (nOrderCountDelta != 0 || nArrivedCountDelta != 0)
            {
                string strNewValue = "";
                string strOldValue = "";
                OrderDesignControl.ParseOldNewValue(this.order.Copy,
                    out strOldValue,
                    out strNewValue);
                int nOldCopy = IssueBindingItem.GetNumberValue(strOldValue);
                int nNewCopy = IssueBindingItem.GetNumberValue(strNewValue);
                nOldCopy += nOrderCountDelta;
                // 2010/4/13
                if (nOldCopy < 0)
                    nOldCopy = 0;
                Debug.Assert(nOldCopy >= 0, "");
                nNewCopy += nArrivedCountDelta;
                // 2010/4/13
                if (nNewCopy < 0)
                    nNewCopy = 0;
                Debug.Assert(nNewCopy >= 0, "");
                this.order.Copy = OrderDesignControl.LinkOldNewValue(nOldCopy.ToString(),
                     nNewCopy.ToString());
                bChanged = true;
            }

            if (this.order.UpdateDistributeString(this) == true)
                bChanged = true;
            if (bChanged == true)
                issue.AfterMembersChanged();
        }

                // 在组内插入新的格子(预测格子)
        // parameters:
        //      nInsertPos  插入位置。如果为-1，表示插入在尾部
        // return:
        //      返回插入的index(整个issue.Cells下标)
        internal int InsertNewMemberCell(
            int nInsertPos,
            out string strError)
        {
            Debug.Assert(this.EndBracket == false, "只能对头部对象使用InsertGroupMemberCell()");
            IssueBindingItem issue = this.Container;
            if (issue == null)
            {
                Debug.Assert(issue != null, "");
                strError = "this.Container == null";
                return -1;
            }
            int head_index = issue.IndexOfCell(this);
            if (head_index == -1)
            {
                Debug.Assert(false, "");
                strError = "在容器的.Cells集合中没有找到自己";
                return -1;
            }

            int nStartIndex = -1;
            for (int i = head_index + 1; i < issue.Cells.Count; i++)
            {
                Cell cell = issue.GetCell(i);
                if (cell is GroupCell)
                {
                    if (nInsertPos == -1)
                    {
                        nStartIndex = i;
                        break;
                    }
                    break;
                }
                if (nInsertPos == i - head_index - 1)
                {
                    nStartIndex = i;
                    break;
                }
            }

            if (nStartIndex == -1)
            {
                strError = "nInsertPos值 " + nInsertPos.ToString() + " 超过了组的范围";
                return -1;
            }

            int nOrderIndex = issue.OrderItems.IndexOf(this.order);
            Debug.Assert(nOrderIndex != -1, "");

            {
                Cell cell = new Cell();
                cell.item = new ItemBindingItem();
                cell.item.Container = issue;
                cell.item.Initial("<root />", out strError);
                cell.item.RefID = "";
                cell.item.LocationString = "";
                cell.item.Calculated = true;


                IssueBindingItem.SetFieldValueFromOrderInfo(
                    false,
                    cell.item,
                    this.order);
                Debug.Assert(nStartIndex - head_index - 1 >= 0, "");
                cell.item.OrderInfoPosition = new Point(nOrderIndex, nStartIndex - head_index - 1);

                issue.Cells.Insert(nStartIndex, cell);
                cell.Container = issue;

                // 2010/4/1
                cell.item.PublishTime = issue.PublishTime;
                cell.item.Volume = VolumeInfo.BuildItemVolumeString(
                    IssueUtil.GetYearPart(issue.PublishTime),
                    issue.Issue,
                    issue.Zong,
                    issue.Volume);
            }

            // 将同一组中位于右边的格子的订购信息定位参数改变
            for (int i = nStartIndex + 1; i < issue.Cells.Count; i++)
            {
                Cell cell = issue.GetCell(i);
                if (cell is GroupCell)
                    break;
                Debug.Assert(cell != null, "");
                Debug.Assert(cell.item != null, "");
                if (cell.item != null)
                {
                    // 因为插入会带来增量
                    cell.item.OrderInfoPosition.Y++;
                }
            }


            // 除了修改<distribute>内容，还要修改<copy>内容
            {
                string strNewValue = "";
                string strOldValue = "";
                OrderDesignControl.ParseOldNewValue(this.order.Copy,
                    out strOldValue,
                    out strNewValue);
                int nOldCopy = IssueBindingItem.GetNumberValue(strOldValue);
                int nNewCopy = IssueBindingItem.GetNumberValue(strNewValue);
                nOldCopy ++;
                this.order.Copy = OrderDesignControl.LinkOldNewValue(nOldCopy.ToString(),
                     nNewCopy.ToString());
            }

            this.order.UpdateDistributeString(this);
            issue.AfterMembersChanged();
            return nStartIndex;
        }

#if NOOOOOOOOOOOOO
        // 在组内插入新的格子(预测格子)
        // parameters:
        //      nInsertPos  插入位置。如果为-1，表示插入在尾部
        // return:
        //      返回插入的index(整个issue.Cells下标)
        internal int InsertNewMemberCell(
            int nInsertPos,
            out string strError)
        {
            Debug.Assert(this.EndBracket == false, "只能对头部对象使用InsertGroupMemberCell()");
            IssueBindingItem issue = this.Container;
            if (issue == null)
            {
                Debug.Assert(issue != null, "");
                strError = "this.Container == null";
                return -1;
            }
            int head_index = issue.IndexOfCell(this);
            if (head_index == -1)
            {
                Debug.Assert(false, "");
                strError = "在容器的.Cells集合中没有找到自己";
                return -1;
            }

            int nStartndex = -1;
            for (int i = head_index + 1; i < issue.Cells.Count; i++)
            {
                Cell cell = issue.GetCell(i);
                if (cell is GroupCell)
                {
                    if (nInsertPos == -1)
                    {
                        nStartndex = i;
                        break;
                    }
                    break;
                }
                if (nInsertPos == i - head_index - 1)
                {
                    nStartndex = i;
                    break;
                }
            }

            if (nStartndex == -1)
            {
                strError = "nInsertPos值 " + nInsertPos.ToString() + " 超过了组的范围";
                return -1;
            }

            int nOrderIndex = issue.OrderItems.IndexOf(this.order);
            Debug.Assert(nOrderIndex != -1, "");

            {
                Cell cell = new Cell();
                cell.item = new ItemBindingItem();
                cell.item.Container = issue;
                cell.item.Initial("<root />", out strError);
                cell.item.RefID = "";
                cell.item.LocationString = "";
                cell.item.Calculated = true;
                IssueBindingItem.SetFieldValueFromOrderInfo(
                    false,
                    cell.item,
                    this.order);
                Debug.Assert(nStartndex - head_index - 1 >= 0, "");
                cell.item.OrderInfoPosition = new Point(nOrderIndex, nStartndex - head_index - 1);

                issue.Cells.Insert(nStartndex, cell);
                cell.Container = issue;
            }

            // 将同一组中位于右边的格子的订购信息定位参数改变
            for (int i = nStartndex+1; i < issue.Cells.Count; i++)
            {
                Cell cell = issue.GetCell(i);
                if (cell is GroupCell)
                    break;
                Debug.Assert(cell != null, "");
                Debug.Assert(cell.item != null, "");
                if (cell.item != null)
                {
                    // 因为插入会带来增量
                    cell.item.OrderInfoPosition.Y++;
                }
            }

            // 注意：仅仅修改了<distribute>内容，而没有修改<copy>内容
            // 因此保存后，下次启动，新增加的格子又会不见了

            bool bChanged = this.order.UpdateDistributeString(this);
            if (bChanged == true)
                issue.AfterMembersChanged();

            return nStartndex;
        }
#endif

        public List<Cell> CalculatedMemberCells
        {
            get
            {
                Debug.Assert(this.EndBracket == false, "只能对头部对象使用CalculatedMemberCells");
                return GetMemberCells(0x01);
            }
        }

        public List<Cell> AcceptedMemberCells
        {
            get
            {
                Debug.Assert(this.EndBracket == false, "只能对头部对象使用AcceptedMemberCells");
                return GetMemberCells(0x02);
            }
        }

        // 绘制每个订购组引导的格子
        internal override void Paint(
        long start_x,
        long start_y,
        PaintEventArgs e)
        {
            Debug.Assert(this.Container != null, "");

            if (BindingControl.TooLarge(start_x) == true
                || BindingControl.TooLarge(start_y) == true)
                return;

            if (this.EndBracket == false)
            {
                Debug.Assert(this.order != null, "");
            }
            else
            {
                Debug.Assert(this.order == null, "");
            }

            Debug.Assert(this.IsMember == false, "");

            bool bSelected = this.Selected;

            RectangleF rect;

            GraphicsState gstate = null;

            // 是否进行了整体旋转
            bool bRotate = this.OutofIssue == true
                | (this.m_bFocus == true && this.m_bHover == false);

            if (bRotate == true)
            {
                rect = new RectangleF(start_x,
                    start_y,
                    this.Width,
                    this.Height);

                gstate = e.Graphics.Save();
                e.Graphics.Clip = new Region(rect);
                // Setup the transformation matrix
                Matrix x = new Matrix();
                if (this.OutofIssue == true
                    && (this.m_bFocus == true && this.m_bHover == false))
                    x.RotateAt(-35, new PointF(start_x + (this.Width / 2), start_y + (this.Height / 2)));
                else if (this.OutofIssue == true)
                    x.RotateAt(-45, new PointF(start_x + (this.Width / 2), start_y + (this.Height / 2)));
                else
                    x.RotateAt(10, new PointF(start_x + (this.Width / 2), start_y + (this.Height / 2)));
                e.Graphics.Transform = x;
            }

            // 普通单册
            Color colorText = this.Container.Container.ForeColor;
            Color colorGray = this.Container.Container.GrayColor;

            {
                Brush brushBack = null;

                try
                {
                    // 背景
                    if (bSelected == true)
                    {
                        // 选定了的格子
                        Color colorBack = this.Container.Container.SelectedBackColor;
                        if (this.m_bFocus == true)
                        {
                            // 左 -- 右
                            brushBack = new LinearGradientBrush(
                new PointF(start_x, start_y + this.Height),
                new PointF(start_x + this.Width, start_y),
                Color.FromArgb(0, colorBack),
                Color.FromArgb(255, ControlPaint.Dark(colorBack))
                );
                        }
                        else
                        {
                            brushBack = new SolidBrush(colorBack);
                        }
                        colorText = this.Container.Container.SelectedForeColor;
                        colorGray = this.Container.Container.SelectedGrayColor;
                    }
                    else if (this.EndBracket == false)
                    {
                        // 左花括号

                        // brushBack = null;
                        Color colorBack = this.Container.Container.BackColor;
                        if (this.m_bFocus == true)
                        {
                            // 左 -- 右
                            brushBack = new LinearGradientBrush(
                new PointF(start_x, start_y + this.Height),
                new PointF(start_x + this.Width, start_y),
                Color.FromArgb(0, colorBack),
                Color.FromArgb(255, ControlPaint.Dark(colorBack))
                );
                        }
                        else
                        {
                            // brushBack = new SolidBrush(colorBack);
                            // 左 -- 右
                            brushBack = new LinearGradientBrush(
                new PointF(start_x, start_y),
                new PointF(start_x + this.Width, start_y),
                Color.FromArgb(50, colorBack),
                Color.FromArgb(255, colorBack)
                );
                        }
                    }
                    else
                    {
                        // 右花括号

                        // brushBack = null;
                        Color colorBack = this.Container.Container.BackColor;
                        if (this.m_bFocus == true)
                        {
                            // 左 -- 右
                            brushBack = new LinearGradientBrush(
                new PointF(start_x, start_y + this.Height),
                new PointF(start_x + this.Width, start_y),
                Color.FromArgb(255, colorBack),
                Color.FromArgb(0, ControlPaint.Dark(colorBack))
                );
                        }
                        else
                        {
                            // brushBack = new SolidBrush(colorBack);
                            brushBack = new LinearGradientBrush(
                new PointF(start_x, start_y),
                new PointF(start_x + this.Width, start_y),
                Color.FromArgb(255, colorBack),
                Color.FromArgb(50, colorBack)
                );
                        }
                    }

                    Color colorSideBar = Color.FromArgb(0, 255, 255, 255);

                    // 新建的和发生过修改的，侧边条颜色需要设定
                    if (this.item != null
                        && this.item.NewCreated == true)
                    {
                        // 新创建的单册
                        colorSideBar = this.Container.Container.NewBarColor;
                    }
                    else if (this.item != null
                   && this.item.Changed == true)
                    {
                        // 修改过的的单册
                        colorSideBar = this.Container.Container.ChangedBarColor;
                    }

                    // 边框和边条
                    {

                        rect = new RectangleF(start_x,
                            start_y,
                            this.Container.Container.m_nCellWidth,
                            this.Container.Container.m_nCellHeight);

                        {
                            // 没有焦点时要小一些
                            rect = GuiUtil.PaddingRect(this.Container.Container.CellMargin,
                                rect);
                        }
                        // rect = RectangleF.Inflate(rect, -4, -4);

                        // 优化
                        if (rect.IntersectsWith(e.ClipRectangle) == true
                            || bRotate == true)
                        {
                            float fPenWidth = 6;

                            e.Graphics.FillRectangle(brushBack, rect);

                            // rect.Inflate(-(fPenWidth / 2), -(fPenWidth / 2));
                            using (Pen penBorder = new Pen(Color.FromArgb(100, Color.Gray), fPenWidth))
                            {
                                penBorder.LineJoin = LineJoin.Bevel;
                                BindingControl.Bracket(e.Graphics,
                                    penBorder,
                                    this.EndBracket == false ? true : false,   //left
                                    rect,
                                    10);
                            }

                            // brushBack?
                            if (this.EndBracket == false)
                            {
                                int height = 20;
                                int width = 20;
                                // 右上角
                                RectangleF rectCircle = new RectangleF(
                                    rect.X + rect.Width - 20 - 20,  // radius * 2
                                    rect.Y, // +rect.Height/2-height/2,
                                    width,
                                    height);
                                Color colorDark = this.Container.Container.ForeColor;
                                using (Brush brush = new SolidBrush(colorDark))
                                {
                                    BindingControl.Circle(e.Graphics,
                                        null,
                                        brush,
                                        rectCircle);
                                }

                                IssueBindingItem issue = this.Container;
                                Debug.Assert(issue != null, "");
                                int nOrderIndex = issue.OrderItems.IndexOf(this.order);
                                Debug.Assert(nOrderIndex != -1, "");
                                string strText = new String((char)((int)'a' + nOrderIndex), 1);

                                StringFormat stringFormat = new StringFormat();
                                stringFormat.Alignment = StringAlignment.Center;
                                stringFormat.LineAlignment = StringAlignment.Center;
                                stringFormat.FormatFlags = StringFormatFlags.NoWrap;

                                using (Brush brushText = new SolidBrush(this.Container.Container.BackColor))
                                using (Font font = new Font("微软雅黑",
                                    rectCircle.Height,
                                    FontStyle.Bold,
                                    GraphicsUnit.Pixel))
                                {
                                    e.Graphics.DrawString(strText,
                                        font,
                                        brushText,
                                        rectCircle,
                                        stringFormat);
                                }

                            }

                            // 边条。左侧
                            using (Brush brushSideBar = new SolidBrush(colorSideBar))
                            {
                                RectangleF rectSideBar = new RectangleF(
                                    rect.X + fPenWidth, // + penBorder.Width,
                                    rect.Y + 10,
                                    10 / 2,
                                    rect.Height - 2 * 10);
                                e.Graphics.FillRectangle(brushSideBar, rectSideBar);
                            }

                        }
                    }
                }
                finally
                {
                    if (brushBack != null)
                    {
                        brushBack.Dispose();
                        brushBack = null;
                    }
                }
            }

            // 绘制文字
            if (this.order != null)
            {
                    this.PaintTextLines(start_x, start_y, false,
                        colorText,
                        e, bRotate);

                if (this.m_bHover == true)
                    this.PaintLineLabels(start_x, start_y, e, bRotate);
            }
            else
            {
                /*
                // 空白格子。绘制淡色的“缺”字样
                this.PaintQue(start_x,
                    start_y,
                    "缺",
                    colorGray,
                    e,
                    bRotate);
                */
            }


            // 焦点虚线
            if (this.m_bFocus == true)
            {
                rect = new RectangleF(
    start_x,
    start_y,
    this.Width,
    this.Height);
                rect.Inflate(-1, -1);
                ControlPaint.DrawFocusRectangle(e.Graphics,
                    Rectangle.Round(rect));
            }

            if (gstate != null)
            {
                Debug.Assert(bRotate == true);
                e.Graphics.Restore(gstate);
            }

            // 移动把手，不要旋转。因为旋转会带来点击的不一致
            Rectangle rect1 = this.Container.Container.RectGrab;
            rect1.Offset((int)start_x, (int)start_y);
            if (this.m_bHover == true)
            {
                ControlPaint.DrawContainerGrabHandle(
        e.Graphics,
        rect1);
            }
        }

        // 绘制订购组格子的文字行
        internal override void PaintTextLines(float x0,
    float y0,
    bool bGrayText,
    Color colorText,
    PaintEventArgs e,
    bool bDoNotSpeedUp)
        {
            int nLineHeight = this.LineHeight;
            Padding margin = this.Container.Container.CellMargin;
            Padding padding = this.Container.Container.CellPadding;
            x0 += margin.Left + padding.Left;
            y0 += margin.Top + padding.Top;
            int nWidth = this.Width - margin.Horizontal - padding.Horizontal;
            int nHeight = this.Height - margin.Vertical - padding.Vertical;

            int nUsedHeight = 0;    // 使用过的累积高度
            // Color colorText = this.Container.Container.MemberForeColor;

            if (bGrayText == true)
                colorText = ControlPaint.Light(colorText, 1.5F);

            // 左 -- 右
            using (LinearGradientBrush brushGradient = new LinearGradientBrush(
new PointF(x0, 0),
new PointF(x0 + 6, 0),
Color.FromArgb(255, Color.Gray),
Color.FromArgb(0, Color.Gray)
))
            {

                Font font = this.Container.Container.m_fontLine;

                using (Pen penLine = new Pen(brushGradient, (float)1))
                {
                    for (int i = 0; i < this.Container.Container.GroupTextLineNames.Length / 2; i++)
                    {
                        int nRestHeight = nHeight - nUsedHeight;

                        // 绘制行
                        RectangleF rect = new RectangleF(
                            x0,
                            y0,
                            nWidth,
                            Math.Min(this.LineHeight, nRestHeight));

                        // 优化
                        if (rect.IntersectsWith(e.ClipRectangle) == true
                            || bDoNotSpeedUp == true)
                        {
                            string strName = this.Container.Container.GroupTextLineNames[i * 2];

                            string strText = this.order.GetText(strName);

                            PaintText(
                                x0,
                                y0,
                                nWidth,
                                Math.Min(nLineHeight, nRestHeight),
                                strText,
                                colorText,
                                font,
                                e);

                            // 下方线条
                            if (nLineHeight < nRestHeight)
                            {
                                e.Graphics.DrawLine(penLine,
                                    new PointF(rect.X, rect.Y + nLineHeight - 1),
                                    new PointF(rect.X + 5, rect.Y + nLineHeight - 1));
                            }
                        }

                        y0 += nLineHeight;
                        nUsedHeight += nLineHeight;

                        if (nUsedHeight > nHeight)
                            break;
                    }
                }
            }
        }

        // 绘制订购组格子的文字标签(字段名)
        internal override void PaintLineLabels(float x0,
    float y0,
    PaintEventArgs e,
    bool bDoNotSpeedUp)
        {
            int nLineHeight = this.LineHeight;

            Padding margin = this.Container.Container.CellMargin;
            Padding padding = this.Container.Container.CellPadding;
            x0 += margin.Left + padding.Left;
            y0 += margin.Top + padding.Top;
            int nWidth = this.Width - margin.Horizontal - padding.Horizontal;
            int nHeight = this.Height - margin.Vertical - padding.Vertical;

            int nUsedHeight = 0;    // 使用过的累积高度
            Color colorText = Color.FromArgb(200, 0, 100, 0);

            Font ref_font = this.Container.Container.m_fontLine;
            using (Font font = new Font(ref_font, FontStyle.Bold))
            {

                // 获得文字的最大宽度
                float fMaxTextWidth = 0;
                for (int i = 0; i < this.Container.Container.GroupTextLineNames.Length / 2; i++)
                {
                    string strLabel = this.Container.Container.GroupTextLineNames[i * 2 + 1];
                    SizeF size = e.Graphics.MeasureString(strLabel, font);
                    if (size.Width > fMaxTextWidth)
                        fMaxTextWidth = size.Width;
                }

                // 绘制半透明背景
                {
                    RectangleF rect1 = new RectangleF(
                        x0 + nWidth - (fMaxTextWidth + 4),
                        y0,
                        fMaxTextWidth + 4,
                        nHeight);

                    if (rect1.IntersectsWith(e.ClipRectangle) == true
                        || bDoNotSpeedUp == true)
                    {

                        // 左 -- 右
                        using (LinearGradientBrush brushGradient = new LinearGradientBrush(
        new PointF(rect1.X, rect1.Y),
        new PointF(rect1.X + rect1.Width, rect1.Y),
        Color.FromArgb(150, Color.White),
        Color.FromArgb(200, Color.White)
        ))
                        {
                            e.Graphics.FillRectangle(brushGradient,
                                rect1);
                        }
                    }

                }

                StringFormat stringFormat = new StringFormat();
                stringFormat.Alignment = StringAlignment.Far;
                stringFormat.LineAlignment = StringAlignment.Near;
                stringFormat.FormatFlags = StringFormatFlags.NoWrap;

                using (Brush brushText = new SolidBrush(colorText))
                using (Pen penLine = new Pen(colorText, (float)1))
                {
                    for (int i = 0; i < this.Container.Container.GroupTextLineNames.Length / 2; i++)
                    {
                        int nRestHeight = nHeight - nUsedHeight;

                        // 绘制行
                        RectangleF rect = new RectangleF(
                            x0 + nWidth - fMaxTextWidth,
                            y0,
                            fMaxTextWidth,
                            Math.Min(nLineHeight, nRestHeight));

                        // 优化
                        if (rect.IntersectsWith(e.ClipRectangle) == true
                            || bDoNotSpeedUp == true)
                        {
                            string strLabel = this.Container.Container.GroupTextLineNames[i * 2 + 1];

                            e.Graphics.DrawString(strLabel,
                                font,
                                brushText,
                                rect,
                                stringFormat);

                            // 下方线条
                            if (nLineHeight < nRestHeight)
                            {
                                e.Graphics.DrawLine(penLine,
                                    new PointF(rect.X, rect.Y + nLineHeight - 1),
                                    new PointF(rect.X + rect.Width, rect.Y + nLineHeight - 1));
                            }
                        }

                        y0 += nLineHeight;
                        nUsedHeight += nLineHeight;

                        if (nUsedHeight > nHeight)
                            break;
                    }
                }
            }
        }
    }

    /// <summary>
    /// 前景背景颜色
    /// </summary>
    public class PaintInfo
    {
        /// <summary>
        /// 背景颜色
        /// </summary>
        public Color BackColor;

        /// <summary>
        /// 前景颜色
        /// </summary>
        public Color ForeColor;
    }
}
