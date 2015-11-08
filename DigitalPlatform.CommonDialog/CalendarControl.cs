using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Diagnostics;

using System.Runtime.InteropServices;

using DigitalPlatform;
using DigitalPlatform.GUI;
using DigitalPlatform.Range;

namespace DigitalPlatform.CommonDialog
{
    public partial class CalendarControl : Control
    {
        bool _readOnly = false;

        public event EventHandler BoxStateChanged = null;

        // bool m_bStartOutClient = false;
        bool m_bChanged = false;

        DayArea m_lastHoverObj = null;

        AreaBase m_lastFocusObj = null;

        bool m_bRectSelectMode = true;

        bool m_bRectSelecting = false;  // 正在矩形选择中途

        // 鼠标在拖动开始时的位置 整体文档坐标
        PointF m_DragStartPointOnDoc = new PointF(0, 0);

        // 鼠标在拖动中途时的位置 整体文档坐标
        PointF m_DragCurrentPointOnDoc = new PointF(0, 0);

        // AreaBase m_focusObject = null;

        ToolTip trackTip;

        // MouseEventArgs mouseMoveArgs = null;

        int m_nDirectionAB = 0;
        int m_nDirectionBC = 0;

        // 拖动开始时的对象
        AreaBase m_DragStartObject = null;
        // 拖动开始时的鼠标位置，view坐标
        Point DragStartMousePosition = new Point(0, 0);

        // 拖动中途最近经过的对象
        AreaBase m_DragLastEndObject = null;

        #region 设计态接口

        [Category("Appearance")]
        [DescriptionAttribute("Day Cell Height")]
        [DefaultValue(100)]
        public int DayCellHeight
        {
            get
            {
                return this.DataRoot.m_nDayCellHeight;
            }
            set
            {
                this.DataRoot.m_nDayCellHeight = value;
            }
        }

        [Category("Appearance")]
        [DescriptionAttribute("Day Cell Width")]
        [DefaultValue(100)]
        public int DayCellWidth
        {
            get
            {
                return this.DataRoot.m_nDayCellWidth;
            }
            set
            {
                this.DataRoot.m_nDayCellWidth = value;
            }
        }

        [Category("Appearance")]
        [DescriptionAttribute("ReadOnly")]
        [DefaultValue(false)]
        public bool ReadOnly
        {
            get
            {
                return this._readOnly;
            }
            set
            {
                this._readOnly = value;
                this.Invalidate();
            }
        }

        [Category("Appearance")]
        [DescriptionAttribute("Back Color Transparence")]
        [DefaultValue(false)]
        public bool BackColorTransparent
        {
            get
            {
                return this.DataRoot.BackColorTransparent;
            }
            set
            {
                this.DataRoot.BackColorTransparent = value;
                this.Invalidate();
            }
        }

        // 是否鼠标在格子上浮动的时候才出现checkbox
        // true 浮动的时候才出现
        // false 一直出现
        [Category("Appearance")]
        [DescriptionAttribute("Hover CheckBox")]
        [DefaultValue(false)]
        public bool HoverCheckBox
        {
            get
            {
                return this.DataRoot.HoverCheckBox;
            }
            set
            {
                this.DataRoot.HoverCheckBox = value;
                this.Invalidate();
            }
        }

        [Category("Appearance")]
        [DescriptionAttribute("Back Color of Year Box")]
        [DefaultValue(typeof(Color), "White")]
        public Color YearBackColor
        {
            get
            {
                return this.DataRoot.YearBackColor;
            }
            set
            {
                this.DataRoot.YearBackColor = value;
                this.Invalidate();
            }
        }

        [Category("Appearance")]
        [DescriptionAttribute("Back Color of Month Box")]
        [DefaultValue(typeof(Color), "White")]
        public Color MonthBackColor
        {
            get
            {
                return this.DataRoot.MonthBackColor;
            }
            set
            {
                this.DataRoot.MonthBackColor = value;
                this.Invalidate();
            }
        }

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

        [Category("Time range")]
        [DescriptionAttribute("time range of calendar")]
        [DefaultValue(typeof(string), "20060101-20061231")]
        public string TimeRange
        {
            get
            {
                return GetRangeString();
            }
            set
            {
                string strError = "";
                int nRet = InitialDataTree(value,
                    out strError);
                if (nRet == -1)
                    throw new Exception(strError);

                AfterDocumentChanged(ScrollBarMember.Both);

                this.Invalidate();

            }
        }

        [Category("State Definition")]
        [DescriptionAttribute("Box state definitions")]
        [DefaultValue(typeof(DayStateDefCollection), "")]
        public DayStateDefCollection DayStateDefCollection
        {
            get
            {
                return this.DataRoot.DayStateDefs;
            }
            set
            {
                this.DataRoot.DayStateDefs = value;

                if (this.DataRoot.DayStateDefs != null)
                {
                    this.DataRoot.m_rectCheckBox.Width = this.DataRoot.DayStateDefs.IconWidth;
                    this.DataRoot.m_rectCheckBox.Height = this.DataRoot.DayStateDefs.IconHeight;
                }

                AfterDocumentChanged(ScrollBarMember.Both);

                this.Invalidate();

            }
        }


        #endregion

        #region 窗口事件

        protected override void OnMouseDown(MouseEventArgs e)
        {
            if (m_bRectSelecting == true)
            {
                Debug.Assert(false, "");
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

                // 看看是否点击在checkbox部分
                if (this._readOnly == false
                    && result.AreaPortion == AreaPortion.CheckBox
                    && result.Object is DayArea)
                {
                    DayArea day = (DayArea)result.Object;

                    if (day.Blank == false) // 空白格子没有这个功能
                    {
                        if (day.ToggleState() == true)
                        {
                            // 刷新
                            this.UpdateObject(day);
                            this.AfterBoxStateChanged();
                            
                        }
                        else
                        {
                            // 发出警告性的响声
                            Console.Beep();
                        }

                        this.Capture = false;   // 不让继续拖拽
                        goto END1;
                    }
                }

                this.DragLastEndObject = null;  // 清除
                this.DragStartObject = result.Object;

                this.DragStartMousePosition = e.Location;

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
                        List<AreaBase> objects = new List<AreaBase>();
                        this.DataRoot.ClearAllSubSelected(ref objects, 100);
                        if (objects.Count >= 100)
                            this.Invalidate();
                        else
                        {
                            // 这个方法屏幕不抖动

                            UpdateObjects(objects);
                        }
                    }
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

                if (result.Object != null)
                {
                    // 
                    // 单独刷新一个
                    List<AreaBase> temp = new List<AreaBase>();
                    temp.Add(result.Object);
                    if (bControl == true)
                    {
                        SelectObjects(temp, SelectAction.Toggle);
                    }
                    else
                    {
                        SelectObjects(temp, SelectAction.On);
                    }

                    if (EnsureVisible(result.Object) == true)
                        this.Update();

                    ShowTip(result.Object, e.Location, false);
                }

                // this.Update();
            }

        END1:
            base.OnMouseDown(e);
        }

        protected override void OnMouseHover(EventArgs e)
        {
            if (m_bRectSelecting == true)
                return;

            HitTestResult result = null;

            Point p = this.PointToClient(Control.MousePosition);

            // 屏幕坐标
            this.HitTest(
                p.X,
                p.Y,
                null, // typeof(DayArea),
                out result);
            if (result == null)
                goto END1;

            if (result.Object != null)
            {
                if (result.Object.GetType() != typeof(DayArea))
                {
                    result.Object = null;
                }
            }

            if (this.m_lastHoverObj == result.Object)
                goto END1;

            /*
            if (m_bRectSelecting == true)
                DrawSelectRect();
             * */

            if (this.m_lastHoverObj != null)
            {
                if (this.m_lastHoverObj.m_bHover != false)
                {
                    this.m_lastHoverObj.m_bHover = false;
                    UpdateObjectHover(this.m_lastHoverObj);
                }
            }

            this.m_lastHoverObj = (DayArea)result.Object;

            if (this.m_lastHoverObj == null)
                goto END1;

            if (this.m_lastHoverObj.m_bHover != true
                && this.m_lastHoverObj.Blank == false)
            {
                this.m_lastHoverObj.m_bHover = true;
                UpdateObjectHover(this.m_lastHoverObj);
            }

            END1:
            base.OnMouseHover(e);
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            if (this.HoverCheckBox == true)
                OnMouseHover(null);

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
                    // 清除上次图像?
                    DrawSelectRect();

                    // 画出这次的图像
                    this.m_DragCurrentPointOnDoc = new PointF(e.X - m_lWindowOrgX,
                        e.Y - m_lWindowOrgY);

                    DrawSelectRect();

                    // 为了能卷滚
                    {
                        Type objType = typeof(DayArea);

                        if (this.DragStartObject != null)
                            objType = this.DragStartObject.GetType();

                        result = null;

                        // 屏幕坐标
                        this.HitTest(
                            e.X,
                            e.Y,
                            objType,
                            out result);
                        if (result == null)
                            goto END1;

                        if (result.Object == null)
                            goto END1;

                        if (result.Object.GetType() != objType)
                            goto END1;

                        // 补上
                        if (this.DragStartObject == null)
                            this.DragStartObject = result.Object;

                        if (this.DragLastEndObject != result.Object)
                        {
                            // 清除
                            DrawSelectRect();
                            if (EnsureVisibleWhenScrolling(result.Object) == true)
                                this.Update();

                            // 重画
                            DrawSelectRect();

                            this.DragLastEndObject = result.Object;
                        }

                        if (GuiUtil.PtInRect(e.X, e.Y, this.ClientRectangle) == false)
                        {
                            this.timer_dragScroll.Start();
                        }
                        else
                        {
                            this.timer_dragScroll.Stop();
                        }
                    }

                    goto END1;
                }


                if (m_bRectSelecting == true)
                    goto END1;


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
                ShowTip(result.Object, e.Location, false);

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
                    if (EnsureVisibleWhenScrolling(result.Object) == true)
                        this.Update();
                    goto END1;
                }

                // 方法1
                // 从this.DragStartObject 到 DragCurrentObject 之间清除
                // 然后 DragCurrentObject 到 result.Object之间，选上
                // 这个方法速度慢

                // 方法2
                // Current到 result.Object之间，toggle；然后，在留意把Start选上

                List<AreaBase> objects = null;
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

                if (GuiUtil.PtInRect(e.X, e.Y, this.ClientRectangle) == false)
                {
                    this.timer_dragScroll.Start();
                    // this.mouseMoveArgs = e;
                }
                else
                {
                    this.timer_dragScroll.Stop();
                }


                goto END1;
            }

            if (this.Capture == false)
            {
                if (IsNearestPoint(this.DragStartMousePosition, e.Location) == false)
                {
                    // 防止在原地的多余一次MouseMove消息隐藏tip窗口
                    trackTip.Hide(this);

                    /*
                    // 

                    HitTestResult result = null;

                    // 屏幕坐标
                    this.HitTest(
                        e.X,
                        e.Y,
                        null,
                        out result);
                    if (result != null 
                        && result.Object != null)
                    {
                        if (this.m_hoverObject != result.Object)
                        {
                            ShowTip(result.Object, e.Location, true);
                            this.m_hoverObject = result.Object;
                        }
                        else
                            this.m_hoverObject = null;
                    }
                     * */
                }
            }

        END1:
            // Call MyBase.OnMouseHover to activate the delegate.
            base.OnMouseMove(e);
        }

        protected override void OnMouseUp(MouseEventArgs e)
        {
            this.Capture = false;

            this.timer_dragScroll.Stop();

            base.OnMouseUp(e);

            // trackTip.Hide(this);


            // 拖动矩形框围选的结束处理
            if (m_bRectSelecting == true
                && e.Button == MouseButtons.Left)
            {
                DoEndRectSelecting();
            }

            // 拖动Portion
            if (e.Button == MouseButtons.Left)
            {
            }

            if (e.Button == MouseButtons.Right)
            {
                PopupMenu(e.Location);
                return;
            }
        }

        void DoEndRectSelecting()
        {
            bool bControl = (Control.ModifierKeys == Keys.Control);
            // bool bShift = (Control.ModifierKeys == Keys.Shift);

            // 清除选择框
            DrawSelectRect();

            // 整体文档坐标
            RectangleF rect = MakeRect(m_DragStartPointOnDoc,
m_DragCurrentPointOnDoc);

            // DataRoot坐标
            rect.Offset(-this.m_nLeftBlank, -this.m_nTopBlank);

            // 选择位于矩形内的对象
            List<Type> types = new List<Type>();
            /*
            types.Add(typeof(YearArea));
            types.Add(typeof(MonthArea));
             * */

            types.Add(typeof(DayArea));

            List<AreaBase> update_objects = new List<AreaBase>();
            this.DataRoot.Select(rect,
                bControl == true ? SelectAction.Toggle : SelectAction.On,
                types,
                ref update_objects,
                100);
            if (update_objects.Count < 100)
                this.UpdateObjects(update_objects);
            else
                this.Invalidate();

            m_bRectSelecting = false;   // 结束
        }

        // 鼠标滚轮
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            int numberOfTextLinesToMove = e.Delta * SystemInformation.MouseWheelScrollLines / 120;
            int numberOfPixelsToMove = numberOfTextLinesToMove * this.DataRoot.m_nDayCellHeight;

            DocumentOrgY += numberOfPixelsToMove;

            // base.OnMouseWheel(e);
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
                        if (this.EnsureVisible(this.m_lastFocusObj) == true)
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

                        // 点到星期标题上怎么办？似乎还需要更好的解决办法。
                        // 可以估算当前客户区高度相当于多少行，然后直接把焦点
                        // 竖向移动这么多行，卷入视线范围，即可。

                        this.OnMouseDown(e1);
                        this.OnMouseUp(e1);

                        if (this.EnsureVisible(this.m_lastFocusObj) == true)
                            this.Update();

                    }
                    break;
                case Keys.Space:
                    {
                        if (this.m_lastFocusObj == null)
                            break;
                        if (!(this.m_lastFocusObj is DayArea))
                            break;

                        DayArea day = (DayArea)this.m_lastFocusObj;

                        if (day.Blank == true) // 空白格子没有这个功能
                            break;
                        if (day.ToggleState() == true)
                        {
                            // 刷新
                            this.UpdateObject(day);
                        }
                        else
                        {
                            // 发出警告性的响声
                            Console.Beep();
                        }

                    }
                    break;
            }

            base.OnKeyDown(e);
        }

        // 上下左右方向键
        // TODO: 可以为GetRangeObjects()设计一个坐标倒转的状态，即上下移动认为才是线性的。这样方便定义同竖向位置的天
        void DoArrowLeftRight(Keys key)
        {
            if (m_lastFocusObj == null)
            {
                // 自动把第一个格子设为焦点
                m_lastFocusObj = this.DataRoot.FindDayArea(-1, -1, -1); // 获得第一天
                SetObjectFocus(m_lastFocusObj);

                return;
            }

            AreaBase obj = null;

            bool bControl = (Control.ModifierKeys == Keys.Control);
            bool bShift = (Control.ModifierKeys == Keys.Shift);


            if (key == Keys.Left)
                obj = m_lastFocusObj.GetPrevSibling();
            else if (key == Keys.Right)
                obj = m_lastFocusObj.GetNextSibling();
            else if (key == Keys.Up)
            {
                if (m_lastFocusObj is DayArea)
                {
                    DayArea day = (DayArea)m_lastFocusObj;
                    WeekArea week = (WeekArea)day.Container.GetPrevSibling();
                    if (week == null)
                        return;
                    obj = week.DayCollection[day.DayOfWeek];
                }
                else
                    obj = m_lastFocusObj.GetPrevSibling();
            }
            else if (key == Keys.Down)
            {
                if (m_lastFocusObj is DayArea)
                {
                    DayArea day = (DayArea)m_lastFocusObj;
                    WeekArea week = (WeekArea)day.Container.GetNextSibling();
                    if (week == null)
                        return;
                    obj = week.DayCollection[day.DayOfWeek];
                }
                else
                    obj = m_lastFocusObj.GetNextSibling();
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
                        List<AreaBase> objects = new List<AreaBase>();
                        this.DataRoot.ClearAllSubSelected(ref objects, 100);
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
                    if (obj.GetType() != this.DragStartObject.GetType())
                        return;

                    if (obj == this.DragLastEndObject)
                    {
                        if (EnsureVisible(obj) == true)
                            this.Update();
                        return;
                    }

                    // 方法2
                    // Current到 result.Object之间，toggle；然后，在留意把Start选上

                    List<AreaBase> objects = null;
                    if (this.DragLastEndObject == null) // 第一次的特殊情况
                    {
                        objects = GetRangeObjects(
                            true,
                            true,
                            this.DragStartObject, obj);

                    }
                    else
                    {
                        // B C之间的方向
                        this.m_nDirectionBC = GetDirection(this.DragLastEndObject, obj);

                        Debug.Assert(this.m_nDirectionBC != 0, "B C两个对象，不能相同");

                        // 如果 A-B B-C同向， 则不包含头部，包含尾部
                        if (this.m_nDirectionAB == 0 // 首次特殊情况
                            || this.m_nDirectionAB == this.m_nDirectionBC)
                        {
                            objects = GetRangeObjects(
                                false,
                                true,
                                this.DragLastEndObject,
                                obj);
                        }
                        else
                        {
                            // 如果 A-B B-C不同向， 则包含头部，不包含尾部
                            objects = GetRangeObjects(
                                true,
                                false,
                                this.DragLastEndObject,
                                obj);
                        }
                    }

                    SelectObjects(objects, SelectAction.Toggle);

                    {
                        // 追加选上原始头部
                        List<AreaBase> temp0 = new List<AreaBase>();
                        temp0.Add(this.DragStartObject);
                        temp0.Add(obj);    // C也保险
                        SelectObjects(temp0, SelectAction.On);
                    }

                    this.DragLastEndObject = obj;

                    // A B 之间的方向
                    this.m_nDirectionAB = GetDirection(this.DragStartObject, this.DragLastEndObject);

                    if (EnsureVisibleWhenScrolling(obj) == true)
                        this.Update();

                    return;
                }

                DragStartObject = obj;
                this.DragLastEndObject = null;  // 清除

                // 选择新一个
                List<AreaBase> temp = new List<AreaBase>();
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

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

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


        #endregion


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
                this.m_bChanged = value;
            }
        }

        void AfterBoxStateChanged()
        {
            this.m_bChanged = true;
            if (this.BoxStateChanged != null)
            {
                EventArgs e = new EventArgs();
                this.BoxStateChanged(this, e);
            }
        }

        // 清除所有格子
        public void Clear()
        {
            this.DataRoot.ChildrenCollection.Clear();
            this.DataRoot.ClearCache();

            this.AfterDocumentChanged(ScrollBarMember.Both);
            this.Invalidate();
        }

        public void ClearMember()
        {
            this.m_lastHoverObj = null;
            this.m_lastFocusObj = null;

            m_bRectSelecting = false;  // 正在矩形选择中途

            m_DragStartPointOnDoc = new PointF(0, 0);

            m_DragCurrentPointOnDoc = new PointF(0, 0);

            trackTip.Hide(this);

            m_nDirectionAB = 0;
            m_nDirectionBC = 0;

            m_DragStartObject = null;
            DragStartMousePosition = new Point(0, 0);

            m_DragLastEndObject = null;

        }


        AreaBase DragStartObject
        {
            get
            {
                return m_DragStartObject;
            }
            set
            {
                // SetObjectFocus(this.m_lastFocusObj, false);
                m_DragStartObject = value;
                if (value != null)
                    SetObjectFocus(m_DragStartObject);
            }
        }

        // 拖动中途最近经过的对象
        AreaBase DragLastEndObject
        {
            get
            {
                return m_DragLastEndObject;
            }
            set
            {
                // SetObjectFocus(this.m_lastFocusObj, false);
                m_DragLastEndObject = value;
                if (value != null)
                    SetObjectFocus(m_DragLastEndObject);
            }
        }


        // 帮助快速清除显示的选择对象相关数组(不是太精确)
        List<AreaBase> m_aSelectedArea = new List<AreaBase>();
        bool m_bSelectedAreaOverflowed = true;  // true可以用来测试没有这个机制时的情况

        BorderStyle borderStyle = BorderStyle.Fixed3D;

        int nNestedSetScrollBars = 0;

        // 卷滚条比率 小于等于1.0F
        double m_v_ratio = 1.0F;
        double m_h_ratio = 1.0F;

        public DateTime StartDate;  // 显示范围开始日
        public DateTime EndDate;    // 显示范围结束日

        public DataRoot DataRoot = new DataRoot();

        int m_nLeftBlank = 10;	// 边空
        int m_nRightBlank = 10;
        int m_nTopBlank = 10;
        int m_nBottomBlank = 10;

        long m_lWindowOrgX = 0;    // 窗口原点
        long m_lWindowOrgY = 0;

        long m_lContentWidth = 0;    // 内容部分的宽度
        long m_lContentHeight = 0;   // 内容部分的高度

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

        // 当文档尺寸和文档原点改变后，更新卷滚条等等设施状态，以便文档可见
        void AfterDocumentChanged(ScrollBarMember member)
        {
            if (member == ScrollBarMember.Both
                || member == ScrollBarMember.Horz)
                this.m_lContentWidth = this.DataRoot.Width;

            if (member == ScrollBarMember.Both
               || member == ScrollBarMember.Vert)
                this.m_lContentHeight = this.DataRoot.Height;   // 用整数，使为了提高速度。注意要及时修改

            SetScrollBars(member);
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            return;
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
                    }
                    break;

                default:
                    break;

            }

            base.DefWndProc(ref m);
        }


        // 构造函数
        public CalendarControl()
        {
            trackTip = new ToolTip();
            // trackTip.ShowAlways = true;

            InitializeComponent();

            // 一些缺省值
            this.DayStateDefCollection = DefaultDayStateDefCollection();
            string strYear = DateTime.Now.Year.ToString().PadLeft(4, '0');
            this.TimeRange = strYear + "0101" + "-" + strYear + "1231";
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;

            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;

            // e.Graphics.SetClip(e.ClipRectangle); // 废话

            long xOffset = m_lWindowOrgX + m_nLeftBlank;
            long yOffset = m_lWindowOrgY + m_nTopBlank;

            this.DataRoot.Paint(xOffset, yOffset, e);

            /*
            for (line = 0; line < m_nCellCount; line++)
            {
                // 每次一行
                RectangleF rect = new RectangleF(0 + xOffset,
                    line * cell_width + yOffset,
                    m_nCellCount * cell_width + 1,
                    cell_width);

                if (rect.IntersectsWith(e.ClipRectangle) == false)
                    continue;

                for (col = 0; col < m_nCellCount; col++)
                {
                    Qizi qizi = (Qizi)aQizi[line * m_nCellCount + col];

                    DrawCellBack(e.Graphics,
                        col,
                        line,
                        qizi);

                    bool bBlack = false;
                    if ((qizi.State & QiziState.Black) == QiziState.Black)
                        bBlack = true;

                    if ((qizi.State & QiziState.On) == QiziState.On)
                    {


                        DrawQizi(e.Graphics, col, line, qizi, bBlack);
                    }

                    if (PrintMode == true)
                        DrawQiziText(e.Graphics, col, line, qizi, bBlack);

                }
            }
             * */

            /*
            Rectangle rectPortion = GetPortionRectangle();
            rectPortion.Offset(xOffset, yOffset);
            rectPortion = this.RectangleToScreen(rectPortion);

            ControlPaint.DrawReversibleFrame(rectPortion,
                Color.Black,
                FrameStyle.Thick);
            */

        }

        void SetObjectFocus(AreaBase obj)
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
        void UpdateObjects(List<AreaBase> objects)
        {
            for (int i = 0; i < objects.Count; i++)
            {
                AreaBase obj = objects[i];
                if (obj == null)
                    continue;

                //  rectUpdate = new RectangleF(0, 0, obj.Width, obj.Height);

                RectangleF rectUpdate = GetViewRect(obj);
                this.Invalidate(Rectangle.Round(rectUpdate));
            }
        }

        // 刷新一个对象的区域
        void UpdateObject(AreaBase obj)
        {
            /*
            RectangleF rectUpdate = new RectangleF(0, 0, obj.Width, obj.Height);

            rectUpdate = GetViewRect(obj);
            this.Invalidate(Rectangle.Round(rectUpdate));
             * */

            RectangleF rectUpdate = GetViewRect(obj);
            this.Invalidate(Rectangle.Round(rectUpdate));

        }

        // 刷新一个对象的checkbox区域
        void UpdateObjectHover(DayArea obj)
        {
            RectangleF rectUpdate = obj.DataRoot.m_rectCheckBox;

            RectangleF rectObj = GetViewRect(obj);
            rectUpdate = new RectangleF(rectObj.X + rectUpdate.X,
                rectObj.Y + rectUpdate.Y,
                rectUpdate.Width,
                rectUpdate.Height);

            this.Invalidate(Rectangle.Round(rectUpdate));
        }

        void ShowTip(AreaBase obj,
            Point p,
            bool bDelay)
        {

            string strTipText = "";

            if (obj is DayArea)
            {
                DayArea day = (DayArea)obj;
                
                if (day.Blank == true)
                    strTipText = "空白";
                else
                    strTipText = String.Format(" {0} {1} ", day.FullName, day.DayOfWeekName(this.DataRoot.m_strDayOfWeekTitleLang));
            }
            else
                strTipText = String.Format(" {0} ", obj.FullName);


            p.Offset(SystemInformation.CursorSize.Width, 0);

            if (bDelay == true)
            {
                trackTip.InitialDelay = 500;
                trackTip.Show(strTipText, this, p, 1000);
            }
            else 
            {
                trackTip.InitialDelay = 0;
                trackTip.Show(strTipText, this, p);
            }

        }


        // 上下文菜单
        void PopupMenu(Point point)
        {
            ContextMenuStrip contextMenu = new ContextMenuStrip();

            ToolStripMenuItem menuItem = null;
            ToolStripMenuItem subMenuItem = null;
            ToolStripSeparator menuSepItem = null;

            bool bHasSelected = this.DataRoot.HasChildrenSelected();

            /*

            ToolStripStatusLabel status_label = new ToolStripStatusLabel("status_label");
            contextMenu.Items.Add(status_label);

            ToolStripButton button = new ToolStripButton("button");
            contextMenu.Items.Add(button);
             * */

            ToolStripLabel label = new ToolStripLabel("日期范围");
            label.Font = new Font(label.Font, FontStyle.Bold);
            contextMenu.Items.Add(label);

            // 前扩
            menuItem = new ToolStripMenuItem("前扩");
            if (this._readOnly == true)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // 子菜单
            subMenuItem = new ToolStripMenuItem();
            subMenuItem.Text = "星期";
            subMenuItem.Tag = "week";
            subMenuItem.Click += new EventHandler(MenuItem_headExpand_Click);
            menuItem.DropDown.Items.Add(subMenuItem);

            subMenuItem = new ToolStripMenuItem();
            subMenuItem.Text = "月";
            subMenuItem.Tag = "month";
            subMenuItem.Click += new EventHandler(MenuItem_headExpand_Click);
            menuItem.DropDown.Items.Add(subMenuItem);

            subMenuItem = new ToolStripMenuItem();
            subMenuItem.Text = "年";
            subMenuItem.Tag = "year";
            subMenuItem.Click += new EventHandler(MenuItem_headExpand_Click);
            menuItem.DropDown.Items.Add(subMenuItem);

            // 后扩
            menuItem = new ToolStripMenuItem("后扩");
            if (this._readOnly == true)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // 子菜单
            subMenuItem = new ToolStripMenuItem();
            subMenuItem.Text = "星期";
            subMenuItem.Tag = "week";
            subMenuItem.Click += new EventHandler(MenuItem_tailExpand_Click);
            menuItem.DropDown.Items.Add(subMenuItem);

            subMenuItem = new ToolStripMenuItem();
            subMenuItem.Text = "月";
            subMenuItem.Tag = "month";
            subMenuItem.Click += new EventHandler(MenuItem_tailExpand_Click);
            menuItem.DropDown.Items.Add(subMenuItem);

            subMenuItem = new ToolStripMenuItem();
            subMenuItem.Text = "年";
            subMenuItem.Tag = "year";
            subMenuItem.Click += new EventHandler(MenuItem_tailExpand_Click);
            menuItem.DropDown.Items.Add(subMenuItem);

            // 
            // 确保头部完整
            menuItem = new ToolStripMenuItem("补齐头部");
            if (this._readOnly == true)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // 子菜单
            subMenuItem = new ToolStripMenuItem();
            subMenuItem.Text = "月";
            subMenuItem.Tag = "month";
            subMenuItem.Click += new EventHandler(MenuItem_headComplete_Click);
            menuItem.DropDown.Items.Add(subMenuItem);

            subMenuItem = new ToolStripMenuItem();
            subMenuItem.Text = "年";
            subMenuItem.Tag = "year";
            subMenuItem.Click += new EventHandler(MenuItem_headComplete_Click);
            menuItem.DropDown.Items.Add(subMenuItem);

            // 
            // 确保尾部完整
            menuItem = new ToolStripMenuItem("补齐尾部");
            if (this._readOnly == true)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // 子菜单
            subMenuItem = new ToolStripMenuItem();
            subMenuItem.Text = "月";
            subMenuItem.Tag = "month";
            subMenuItem.Click += new EventHandler(MenuItem_tailComplete_Click);
            menuItem.DropDown.Items.Add(subMenuItem);

            subMenuItem = new ToolStripMenuItem();
            subMenuItem.Text = "年";
            subMenuItem.Tag = "year";
            subMenuItem.Click += new EventHandler(MenuItem_tailComplete_Click);
            menuItem.DropDown.Items.Add(subMenuItem);


            //
            // 前删
            menuItem = new ToolStripMenuItem("前删");
            if (this._readOnly == true)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // 子菜单
            subMenuItem = new ToolStripMenuItem();
            subMenuItem.Text = "星期";
            subMenuItem.Tag = "week";
            subMenuItem.Click += new EventHandler(MenuItem_headShrink_Click);
            menuItem.DropDown.Items.Add(subMenuItem);

            subMenuItem = new ToolStripMenuItem();
            subMenuItem.Text = "月";
            subMenuItem.Tag = "month";
            subMenuItem.Click += new EventHandler(MenuItem_headShrink_Click);
            menuItem.DropDown.Items.Add(subMenuItem);

            subMenuItem = new ToolStripMenuItem();
            subMenuItem.Text = "年";
            subMenuItem.Tag = "year";
            subMenuItem.Click += new EventHandler(MenuItem_headShrink_Click);
            menuItem.DropDown.Items.Add(subMenuItem);

            // 后删
            menuItem = new ToolStripMenuItem("后删");
            if (this._readOnly == true)
                menuItem.Enabled = false;
            contextMenu.Items.Add(menuItem);

            // 子菜单
            subMenuItem = new ToolStripMenuItem();
            subMenuItem.Text = "星期";
            subMenuItem.Tag = "week";
            subMenuItem.Click += new EventHandler(MenuItem_tailShrink_Click);
            menuItem.DropDown.Items.Add(subMenuItem);

            subMenuItem = new ToolStripMenuItem();
            subMenuItem.Text = "月";
            subMenuItem.Tag = "month";
            subMenuItem.Click += new EventHandler(MenuItem_tailShrink_Click);
            menuItem.DropDown.Items.Add(subMenuItem);

            subMenuItem = new ToolStripMenuItem();
            subMenuItem.Text = "年";
            subMenuItem.Tag = "year";
            subMenuItem.Click += new EventHandler(MenuItem_tailShrink_Click);
            menuItem.DropDown.Items.Add(subMenuItem);


            // ---
            menuSepItem = new ToolStripSeparator();
            contextMenu.Items.Add(menuSepItem);

            // 标题
            label = new ToolStripLabel("针对选择标记");
            label.Font = new Font(label.Font, FontStyle.Bold);
            contextMenu.Items.Add(label);


            // 设置状态
            menuItem = new ToolStripMenuItem("设置状态(&S)");
            if (bHasSelected == false || this._readOnly == true)
                menuItem.Enabled = false;
            else
                menuItem.Enabled = true;

            // menuItem.Click += new System.EventHandler(null);
            contextMenu.Items.Add(menuItem);

            DayStateDefCollection defs = this.DataRoot.DayStateDefs;
            if (defs != null)
            {
                for (int i = 0; i < defs.Count; i++)
                {
                    DayStateDef state = defs[i];

                    subMenuItem = new ToolStripMenuItem();
                    subMenuItem.Image = state.Icon;
                    subMenuItem.Text = state.Caption;
                    subMenuItem.Tag = i;
                    subMenuItem.Click += new EventHandler(DayStateMenuItem_Click);

                    menuItem.DropDown.Items.Add(subMenuItem);
                }
            }

            // ---
            menuSepItem = new ToolStripSeparator();
            contextMenu.Items.Add(menuSepItem);

            // 标题
            label = new ToolStripLabel("状态");
            label.Font = new Font(label.Font, FontStyle.Bold);
            contextMenu.Items.Add(label);

            // 全选
            menuItem = new ToolStripMenuItem("浮动显示状态图标(&H)");
            if (this.HoverCheckBox == true)
                menuItem.Checked = true;
            else
                menuItem.Checked = false;
            menuItem.Click += new EventHandler(menuItem_toggleHoverCheckBox_Click);
            contextMenu.Items.Add(menuItem);


            // 选择模式
            menuItem = new ToolStripMenuItem("选择模式(&M)");
            contextMenu.Items.Add(menuItem);

            // 子菜单
            subMenuItem = new ToolStripMenuItem();
            subMenuItem.Text = "线性(&L)";
            subMenuItem.Tag = 0;
            if (this.m_bRectSelectMode == false)
                subMenuItem.Checked = true;
            subMenuItem.Click += new EventHandler(MenuItem_selecteMode_Click);
            menuItem.DropDown.Items.Add(subMenuItem);

            subMenuItem = new ToolStripMenuItem();
            subMenuItem.Text = "矩形(&R)";
            subMenuItem.Tag = 1;
            if (this.m_bRectSelectMode == true)
                subMenuItem.Checked = true;
            subMenuItem.Click += new EventHandler(MenuItem_selecteMode_Click);
            menuItem.DropDown.Items.Add(subMenuItem);


            // ---
            menuSepItem = new ToolStripSeparator();
            contextMenu.Items.Add(menuSepItem);

            // 标题
            label = new ToolStripLabel("选择标记");
            label.Font = new Font(label.Font, FontStyle.Bold);
            contextMenu.Items.Add(label);

            // 全选
            menuItem = new ToolStripMenuItem("全选(&A)");
            menuItem.Click += new EventHandler(menuItem_selectAll_Click);
            contextMenu.Items.Add(menuItem);

            this.Update();
            contextMenu.Show(this, point);
        }

        void menuItem_toggleHoverCheckBox_Click(object sender, EventArgs e)
        {
            if (this.HoverCheckBox == true)
                this.HoverCheckBox = false;
            else
                this.HoverCheckBox = true;
        }

        // 前扩日期范围
        void MenuItem_headExpand_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem subMenuItem = (ToolStripMenuItem)sender;
            string strUnit = (string)subMenuItem.Tag;

            ExpandTimeRange(strUnit, true);
        }

        // 后扩日期范围
        void MenuItem_tailExpand_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem subMenuItem = (ToolStripMenuItem)sender;
            string strUnit = (string)subMenuItem.Tag;

            ExpandTimeRange(strUnit, false);
        }

        // 确保头部完整日期范围
        void MenuItem_headComplete_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem subMenuItem = (ToolStripMenuItem)sender;
            string strUnit = (string)subMenuItem.Tag;

            CompleteTimeRange(strUnit, true);
        }

        // 确保尾部完整日期范围
        void MenuItem_tailComplete_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem subMenuItem = (ToolStripMenuItem)sender;
            string strUnit = (string)subMenuItem.Tag;

            CompleteTimeRange(strUnit, false);
        }

        //
        // 前删日期范围
        void MenuItem_headShrink_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem subMenuItem = (ToolStripMenuItem)sender;
            string strUnit = (string)subMenuItem.Tag;

            ShrinkTimeRange(strUnit, true);
        }

        // 后删日期范围
        void MenuItem_tailShrink_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem subMenuItem = (ToolStripMenuItem)sender;
            string strUnit = (string)subMenuItem.Tag;

            ShrinkTimeRange(strUnit, false);
        }

        // 删除时间范围
        void ShrinkTimeRange(string strUnit,
            bool bHead)
        {
            bool bRet = false;

            string strName = "";
                        if (strUnit == "year")
            {
                strName = "年";
            }
            else if (strUnit == "month")
            {
                strName = "月";
            }
            else if (strUnit == "week")
            {
                strName = "星期";
            }
            else
                throw new Exception("无法识别的unit '" + strUnit + "'");

            string strWarning = "确实要删除"
                + (bHead == true ? "最前部" : "最后面")
                + "的一个 "
                + strName + " ?";

            DialogResult result = MessageBox.Show(this,
strWarning,
"CalendarControl",
MessageBoxButtons.YesNo,
MessageBoxIcon.Question,
MessageBoxDefaultButton.Button2);

            this.SetObjectFocus(null);
            this.m_lastHoverObj = null;

            if (strUnit == "year")
            {
                bRet = this.DataRoot.ShrinkYear(bHead);
            }
            else if (strUnit == "month")
            {
                bRet =  this.DataRoot.ShrinkMonth(bHead);
            }
            else if (strUnit == "week")
            {
                bRet = this.DataRoot.ShrinkWeek(bHead);
            }
            else
                throw new Exception("无法识别的unit '" + strUnit + "'");

            if (bRet == true)
            {
                AfterDocumentChanged(ScrollBarMember.Both);
                this.Invalidate();
            }

            this.AfterBoxStateChanged();

            if (bHead == true)
                this.DocumentOrgY = 0; // 显示最前面
            else
                this.DocumentOrgY = -this.DocumentHeight;
        }

        // 扩展时间范围
        void ExpandTimeRange(string strUnit,
            bool bHead)
        {

            if (strUnit == "year")
            {
                YearArea new_year = this.DataRoot.ExpandYear(bHead, false);

                // 并且选中
                new_year.Select(SelectAction.On, true);

                AfterDocumentChanged(ScrollBarMember.Both); // 如果扩充前内容内容，这就是首次，要承担水平卷滚条的设置任务
                this.Invalidate();
            }
            else if (strUnit == "month")
            {
                MonthArea new_month = this.DataRoot.ExpandMonth(bHead, false);

                // 并且选中
                new_month.Select(SelectAction.On, true);

                AfterDocumentChanged(ScrollBarMember.Both); // 如果扩充前内容内容，这就是首次，要承担水平卷滚条的设置任务
                this.Invalidate();
            }
            else if (strUnit == "week")
            {
                WeekArea new_week = this.DataRoot.ExpandWeek(bHead, false);

                // 并且选中
                new_week.Select(SelectAction.On, true);

                AfterDocumentChanged(ScrollBarMember.Both); // 如果扩充前内容内容，这就是首次，要承担水平卷滚条的设置任务
                this.Invalidate();
            }
            else
                throw new Exception("无法识别的unit '" + strUnit + "'");

            this.AfterBoxStateChanged();

            if (bHead == true)
                this.DocumentOrgY = 0; // 显示最前面
            else
                this.DocumentOrgY = -this.DocumentHeight;

        }

        // 确保头部或者尾部时间范围完整
        void CompleteTimeRange(string strUnit,
            bool bHead)
        {
            bool bRet = false;
            if (strUnit == "year")
            {
                YearArea first_year = (YearArea)this.DataRoot.EdgeChild(bHead);
                if (first_year == null)
                    return;

                bRet = first_year.CompleteMonth(bHead);
                if (bRet == false)
                    return;

                this.AfterBoxStateChanged();

                AfterDocumentChanged(ScrollBarMember.Both); // 如果扩充前内容内容，这就是首次，要承担水平卷滚条的设置任务
                this.Invalidate();
            }
            else if (strUnit == "month")
            {
                YearArea first_year = (YearArea)this.DataRoot.EdgeChild(bHead);
                if (first_year == null)
                    return;

                MonthArea first_month = (MonthArea)first_year.EdgeChild(bHead);
                if (first_month == null)
                    return;

                bRet = first_month.CompleteWeek(bHead);
                if (bRet == false)
                    return;

                this.AfterBoxStateChanged();

                AfterDocumentChanged(ScrollBarMember.Both); // 如果扩充前内容内容，这就是首次，要承担水平卷滚条的设置任务
                this.Invalidate();
            }
            else
                throw new Exception("无法识别的unit '" + strUnit + "'");

            if (bHead == true)
                this.DocumentOrgY = 0; // 显示最前面
            else
                this.DocumentOrgY = -this.DocumentHeight;

        }

        void MenuItem_selecteMode_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem subMenuItem = (ToolStripMenuItem)sender;
            int nState = (int)subMenuItem.Tag;

            if (nState == 0)
                this.m_bRectSelectMode = false;
            else
                this.m_bRectSelectMode = true;

        }

        // 全选
        void menuItem_selectAll_Click(object sender, EventArgs e)
        {
            this.DataRoot.Select(SelectAction.On, true);
            this.Invalidate();
        }

        // 修改所选择块的状态
        void DayStateMenuItem_Click(object sender, EventArgs e)
        {
            ToolStripMenuItem subMenuItem = (ToolStripMenuItem)sender;
            int nState = (int)subMenuItem.Tag;

            // MessageBox.Show(this, nState.ToString());

            List<AreaBase> update_objects = new List<AreaBase>();
            // 为下级以及再下级有selected的设置状态 (不包括自己)
            // parameters:
            //      bForce  如果为true，则表示不管是否有选择标记，都修改状态
            //              如果为false，则有选择标记的才修改状态
            this.DataRoot.SetChildrenDayState(nState,
                ref update_objects,
                100);
            if (update_objects.Count < 100)
            {
                if (update_objects.Count > 0)
                {
                    this.AfterBoxStateChanged();
                    this.UpdateObjects(update_objects);
                }
            }
            else
            {
                this.Invalidate();
            }
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

        static bool IsNearestPointF(PointF a, PointF b)
        {
            RectangleF rect = new RectangleF(a.X, a.Y, 0, 0);
            rect.Inflate(
                SystemInformation.DoubleClickSize.Width / 2,
                SystemInformation.DoubleClickSize.Height / 2);

            return rect.Contains(b.X, b.Y);
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
        void DrawSelectRect()
        {
            this.Update();

            RectangleF rect = MakeRect(m_DragStartPointOnDoc,
            m_DragCurrentPointOnDoc);

            rect.Offset(m_lWindowOrgX, m_lWindowOrgY);
            ControlPaint.DrawReversibleFrame( // Graphics.FromHwnd(this.Handle),
                this.RectangleToScreen(Rectangle.Round(rect)),
                Color.Yellow,
                FrameStyle.Dashed);
        }

        // 探测两个同级对象的先后关系
        // return:
        //      -1  start在end之前
        //      0   start和end是同一个对象
        //      1   start在end之后
        int GetDirection(AreaBase start, AreaBase end)
        {
            Debug.Assert(start != null, "");
            Debug.Assert(end != null);

            if (start == end)
                return 0;

            // 找到共同的祖先
            AreaBase a = start;
            AreaBase b = end;

            bool bFound = false;
            for (; ; )
            {
                AreaBase p_a = a._Container;
                AreaBase p_b = b._Container;

                if (p_a == p_b)
                {
                    bFound = true;
                    break;
                }

                if (p_a == null)
                    break;
                if (p_b == null)
                    break;

                a = p_a;
                b = p_b;
            }


            if (bFound == false)
            {
                Debug.Assert(false, "所给出的两个对象没有共同祖先");
                return 0;
            }


            // 这时可以通过判断a b之间的顺序来断定后代的顺序
            List<AreaBase> children = a._Container.ChildrenCollection;

            int index_a = -1;
            int index_b = -1;

            for (int i = 0; i < children.Count; i++)
            {
                if (children[i] == a)
                    index_a = i;
                if (children[i] == b)
                    index_b = i;
            }

            if (index_a > index_b)
            {
                // start在end后面
                return 1;
            }

            return -1;  // start在end前面
        }

        // 从起点到终点，构造包含所有兄弟对象的数组
        List<AreaBase> GetRangeObjects(
            bool bIncludeStart,
            bool bIncludeEnd,
            AreaBase start,
            AreaBase end)
        {
            Debug.Assert(start != null, "");
            Debug.Assert(end != null);

            List<AreaBase> result = new List<AreaBase>();

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
                AreaBase temp = start;
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

        void ClearSelectedArea()
        {
            this.m_aSelectedArea.Clear();
            this.m_bSelectedAreaOverflowed = false;
        }

        void AddSelectedArea(AreaBase obj)
        {
            if (this.m_bSelectedAreaOverflowed == true)
                return;

            int index = this.m_aSelectedArea.IndexOf(obj);
            if (index == -1)
                this.m_aSelectedArea.Add(obj);

            if (this.m_aSelectedArea.Count > 1000)
                this.m_bSelectedAreaOverflowed = true;
        }

        void RemoveSelectedArea(AreaBase obj)
        {
            if (this.m_bSelectedAreaOverflowed == true)
                return;

            this.m_aSelectedArea.Remove(obj);
        }

        // 选择一系列对象
        void SelectObjects(List<AreaBase> aObject,
            SelectAction action)
        {
            if (aObject == null)
                return;

            for (int i = 0; i < aObject.Count; i++)
            {
                AreaBase obj = aObject[i];
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

                RectangleF rectUpdate = new RectangleF(0, 0, obj.Width, obj.Height);

                bool bChanged = obj.Select(action, true);
                if (bChanged == false)
                    continue;

                rectUpdate = GetViewRect(obj);
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
        RectangleF GetViewRect(AreaBase obj)
        {
            RectangleF rect = new RectangleF(0, 0, obj.Width, obj.Height);

            rect = obj.ToRootCoordinate(rect);

            // 由DataRoot坐标，变换为整体文档坐标，然后变换为view坐标
            rect.Offset(this.m_lWindowOrgX + m_nLeftBlank,
                this.m_lWindowOrgY + m_nTopBlank);

            return rect;
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
        public bool EnsureVisible(AreaBase obj)
        {
            RectangleF rectUpdate = GetViewRect(obj);

            RectangleF rectCell = rectUpdate;

            RectangleF rectCaret = rectUpdate;

            return EnsureVisible(rectCell, rectCaret);
        }

        // 确保一个对象单元在窗口客户区可见
        // 对DayArea有特殊处理
        // return:
        //      是否发生卷滚了
        public bool EnsureVisibleWhenScrolling(AreaBase obj)
        {

            RectangleF rectUpdate = GetViewRect(obj);

            if (obj is DayArea)
            {
                DayArea day = (DayArea)obj;
                // 如果是每月第一个星期的日子
                if (day.Container.Week == 1)
                {
                // 调整矩形，以包括星期名标题
                    rectUpdate.Y -= this.DataRoot.m_nDayOfWeekTitleHeight;
                    rectUpdate.Height += this.DataRoot.m_nDayOfWeekTitleHeight;
                }
            }

            // TODO:
            // 如果是月、年等较大尺寸的物体，只要这个物体当前部分可见，就不必卷滚了
            // 也可以通过把caret设置为已经可见的部分，来实现类似效果

            RectangleF rectCell = rectUpdate;

            RectangleF rectCaret = rectUpdate;

            return EnsureVisible(rectCell, rectCaret);
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

            if (dest_type == typeof(CalendarControl))
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
                if (dest_type == typeof(CalendarControl))
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
                // 交给文档对象处理
                this.DataRoot.HitTest(x - m_nLeftBlank, 
                    y - m_nTopBlank,
                    dest_type,
                    out result);
                return;
            }

            END1:
            result.X = x;
            result.Y = y;
            result.Object = null;
        }


        public enum ScrollBarMember
        {
            Vert = 0,
            Horz = 1,
            Both = 2,
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

                    if (DataRoot.TooLarge(lDocumentWidth) == true)
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
                    if (DataRoot.TooLarge(lDocumentHeight) == true)
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
                nNestedSetScrollBars--;
            }
        }

        // 设置数据
        // parameters:
        //      strDataString   格式如 '19980101-20010505,20030701-20030801'
        public int SetData(
            string strRangeString,
            int nState,
            string strDataString,
            out string strError)
        {
            strError = "";

            int nRet = InitialDataTree(
                strRangeString,
                out strError);
            if (nRet == -1)
                return -1;

            nRet = SetDayState(
                nState,
                strDataString,
                out strError);
            if (nRet == -1)
                return -1;

            /*
            this.m_lContentWidth = this.DataRoot.Width;
            this.m_lContentHeight = this.DataRoot.Height;   // 用整数，使为了提高速度。注意要及时修改
             * */

            AfterDocumentChanged(ScrollBarMember.Both);

            this.Invalidate();

            return 0;
        }

        // 系统内置的格子状态配置
        public DayStateDefCollection DefaultDayStateDefCollection()
        {
            DayStateDefCollection array = new DayStateDefCollection();

            // 工作日
            DayStateDef state = new DayStateDef();
            state.Caption = "工作日";
            state.BackColor = Color.White;
            state.TextColor = Color.Black;
            state.Icon = this.imageList_stateIcons.Images[0];
            array.Add(state);

            // 休息日
            state = new DayStateDef();
            state.Caption = "休息日";
            state.BackColor = Color.LightYellow;
            state.TextColor = Color.Red;
            state.Icon = this.imageList_stateIcons.Images[1];

            array.Add(state);

            return array;
        }

        public string GetRangeString()
        {
            YearArea first_year = (YearArea)this.DataRoot.FirstChild;
            if (first_year == null)
                return "";

            DayArea first_day = this.DataRoot.FindDayArea(-1, -1, -1); // 获得第一天
            if (first_day == null)
                return "";

            DayArea last_day = null;

            try
            {
                last_day = (DayArea)this.DataRoot.LastChild.LastChild.LastChild.LastChild;
            }
            catch (Exception /*ex*/)
            {
                return "";
            }

            last_day = last_day.PrevNoneBlankDayArea;
            if (last_day == null)
                return "";

            if (first_day != last_day)
                return first_day.Name8 + "-" + last_day.Name8;

            return first_day.Name8;
        }

        // 按照给定的时间范围初始化数据树。
        public int InitialDataTree(string strRangeString,
            out string strError)
        {
            strError = "";

            RangeList rl = null;

            try
            {
                rl = new RangeList(strRangeString);
            }
            catch (Exception ex)
            {
                strError = "解析日期字符串 '" + strRangeString + "' 时发生错误: " + ex.Message;
                return -1;
            }

            this.ClearMember();

            // 构造第一级对象 年
            if (this.DataRoot == null)
                this.DataRoot = new DataRoot();
            else
                this.DataRoot.Clear();

            if (rl.Count == 0)
                return 0;

            long lMax = rl.max();
            long lMin = rl.min();

            // 提取起始和结束日期
            string strStartDate = lMin.ToString().PadLeft(8, '0');
            string strEndDate = lMax.ToString().PadLeft(8, '0');

            // 提取起始和结束年
            string strStartYear = strStartDate.Substring(0, 4);
            string strEndYear = strEndDate.Substring(0, 4);

            int nStartYear = Convert.ToInt32(strStartYear);
            int nEndYear = Convert.ToInt32(strEndYear);



            int nRet = this.DataRoot.Build(nStartYear,
                nEndYear,
                out strError);
            if (nRet == -1)
                return -1;

            return 0;
        }

        // 为指定的格子设置指定状态
        public int SetDayState(
            int nState,
            string strDataString,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strDataString) == true)
                return 0;

            if (this.DataRoot.YearCollection.Count == 0)
            {
                strError = "日期范围尚未初始化";
                return -1;
            }

            RangeList rl = null;

            try
            {
                rl = new RangeList(strDataString);
            }
            catch (Exception ex)
            {
                strError = "解析日期字符串 '" + strDataString + "' 时发生错误: " + ex.Message;
                return -1;
            }

            if (rl.Count == 0)
                return 0;

            long lMax = rl.max();
            long lMin = rl.min();

            // 提取起始和结束日期
            string strStartDate = lMin.ToString().PadLeft(8, '0');
            string strEndDate = lMax.ToString().PadLeft(8, '0');

            // 提取起始和结束年
            string strStartYear = strStartDate.Substring(0, 4);
            string strEndYear = strEndDate.Substring(0, 4);

            int nStartYear = Convert.ToInt32(strStartYear);
            int nEndYear = Convert.ToInt32(strEndYear);

            // 检查范围
            if (nStartYear < this.DataRoot.MinYear)
            {
                strError = "要设置的日期范围头部 '"+nStartYear.ToString().PadLeft(4, '0')+"' 年越过了 当前范围的头部 '"+this.DataRoot.MinYear.ToString().PadLeft(4,'0')+"'";
                return -1;
            }

            if (nEndYear > this.DataRoot.MaxYear)
            {
                strError = "要设置的日期范围尾部 '" + nEndYear.ToString().PadLeft(4, '0') + "' 年越过了 当前范围的头部 '" + this.DataRoot.MaxYear.ToString().PadLeft(4, '0') + "'";
                return -1;
            }

            // 设置被选日期的状态
            for (int i = 0; i < rl.Count; i++)
            {
                RangeItem range = (RangeItem)rl[i];
                long lStartDate = range.lStart;
                long lEndDate = range.lStart + range.lLength - 1;

                int year = 0;
                int month = 0;
                int day = 0;
                ParseDate(range.lStart,
                    out year,
                    out month,
                    out day);

                int end_year = 0;
                int end_month = 0;
                int end_day = 0;
                ParseDate(lEndDate,
                    out end_year,
                    out end_month,
                    out end_day);

                DayArea objDay = this.DataRoot.FindDayArea(year, month, day);
                if (objDay == null)
                {
                    strError = "日对象在树结构中没有找到: " + year.ToString() + "." + month.ToString() + "." + day.ToString();
                    return -1;
                }

                // 为一个连续的日段落设置状态
                for (; ; )
                {

                    objDay.State = nState;

                    objDay = objDay.NextNoneBlankDayArea;
                    if (objDay == null)
                        break;

                    int cur_year = objDay.Year;

                    if (cur_year > end_year)
                        break;

                    int cur_month = objDay.Month;

                    if (cur_year == end_year
                        && cur_month > end_month)
                        break;

                    if (cur_year == end_year
                           && cur_month == end_month
                        && objDay.Day > end_day)
                        break;

                }
            }

            return 0;
        }

        // 获得表示全部所要的状态的日子 的日期字符串
        public int GetDates(int nState,
            out string strDateString,
            out string strError)
        {
            strDateString = "";
            strError = "";

            /*
            List<int> levels = new List<int>();
            levels.Add(-1); // DataRoot
            levels.Add(-1); // 年
            levels.Add(-1); // 月
            levels.Add(-1); // 周
            levels.Add(-1); // 日

                    // 根据给出的NameValue值, 从当前对象开始(包括当前对象) 定位后代对象
            AreaBase obj = FindByNameValue(levels);
            if (obj == null)
                return 0;
             * */


            DayArea objDay = this.DataRoot.FindDayArea(-1, -1, -1); // 获得第一天
            if (objDay == null)
                return 0;

            string strStart = "";
            string strEnd = "";
            // 为一个连续的日段落设置状态
            for (; ; )
            {
                // 如果正好是所要的状态
                if (objDay != null
                    && objDay.State == nState)
                {
                    if (strStart == "")
                        strStart = objDay.Name8;
                    else
                        strEnd = objDay.Name8;
                }
                else if (objDay == null // 最后一次
                    || objDay.State != nState // 如果状态不是所要的
                    ) 
                {
                    if (strStart != "" && strEnd != "")
                    {
                        if (strDateString != "")
                        {
                            strDateString += ",";
                        }

                        strDateString += strStart + "-" + strEnd;

                        strStart = "";
                        strEnd = "";

                    }
                    else if (strStart != "" && strEnd == "")
                    {
                        if (strDateString != "")
                        {
                            strDateString += ",";
                        }

                        strDateString += strStart;

                        strStart = "";
                        strEnd = "";

                    }

                }

                if (objDay == null)
                    break;

                objDay = objDay.NextNoneBlankDayArea;

            }


            return 0;
        }

        static void ParseDate(long lDate,
            out int year,
            out int month,
            out int day)
        {
            string strDate = lDate.ToString().PadLeft(8, '0');
            year = Convert.ToInt32(strDate.Substring(0, 4));
            month = Convert.ToInt32(strDate.Substring(4, 2));
            day = Convert.ToInt32(strDate.Substring(6, 2));
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
    }



}
