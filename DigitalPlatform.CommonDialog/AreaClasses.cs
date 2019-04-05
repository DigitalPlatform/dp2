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

namespace DigitalPlatform.CommonDialog
{
    // 所有区域类的基类
    public class AreaBase
    {
        internal bool m_bSelected = false;
        internal bool m_bFocus = false;

        internal long m_lWidthCache = -1;
        internal long m_lHeightCache = -1;

        public AreaBase _Container = null;

        public List<AreaBase> ChildrenCollection = new List<AreaBase>();

        public int NameValue = 0;   // 0 表示尚未初始化

        public AreaBase FirstChild
        {
            get
            {
                if (this.ChildrenCollection.Count == 0)
                    return null;
                return this.ChildrenCollection[0];
            }
        }

        public AreaBase LastChild
        {
            get
            {
                if (this.ChildrenCollection.Count == 0)
                    return null;
                return this.ChildrenCollection[this.ChildrenCollection.Count-1];
            }
        }

        public AreaBase EdgeChild(bool bHead)
        {
            if (this.ChildrenCollection.Count == 0)
                return null;
            if (bHead == true)
                return this.ChildrenCollection[0];
            else
                return this.ChildrenCollection[this.ChildrenCollection.Count - 1];
        }

        // 清除缓存的变量
        // 沿着祖先的路径, 全部清除
        public virtual void ClearCache()
        {
            AreaBase obj = this;

            while (obj != null)
            {
                obj.m_lWidthCache = -1;
                obj.m_lHeightCache = -1;

                obj = obj._Container;
            }
        }

        // 为下级以及再下级有selected的设置状态 (不包括自己)
        // parameters:
        public void SetChildrenDayState(int nState,
            ref List<AreaBase> update_objects,
            int nMaxCount)
        {
            for (int i = 0; i < this.ChildrenCollection.Count; i++)
            {
                AreaBase obj = this.ChildrenCollection[i];

                // 如果是DayArea类型
                if (obj is DayArea)
                {
                    DayArea day = (DayArea)obj;

                    if (obj.m_bSelected == true
                        && day.State != nState
                        && day.Blank == false)
                    {

                        day.State = nState;
                        if (update_objects.Count < nMaxCount)
                            update_objects.Add(obj);
                    }
                }
                else
                {
                    // 递归
                    obj.SetChildrenDayState(nState,
                        ref update_objects,
                        nMaxCount);
                }
            }

        }


        /*
        // 为下级以及再下级有selected的设置状态 (不包括自己)
        // parameters:
        //      bForce  如果为true，则表示不管是否有选择标记，都修改状态
        //              如果为false，则有选择标记的才修改状态
        public void SetChildrenDayState(int nState,
            bool bForce,
            ref List<AreaBase> update_objects,
            int nMaxCount)
        {
            for (int i = 0; i < this.ChildrenCollection.Count; i++)
            {
                AreaBase obj = this.ChildrenCollection[i];

                // 如果是DayArea类型
                if (obj is DayArea)
                {
                    DayArea day = (DayArea)obj;

                    if (( obj.m_bSelected == true || bForce == true)
                        && day.State != nState
                        && day.Blank == false)
                    {

                        day.State = nState;
                        if (update_objects.Count < nMaxCount)
                            update_objects.Add(obj);
                    }
                }
                else
                {
                    bool bNewForce = false;

                    // 如果一个对象虽然不是DayArea对象，但如果它已经被选择，那就意味着其下级全部DayArea对象都要强制被设置状态
                    if (bForce == true
                        || obj.m_bSelected == true)
                        bNewForce = true;

                    // 递归
                    obj.SetChildrenDayState(nState,
                        bNewForce,
                        ref update_objects,
                        nMaxCount);
                }
            }

            
        }
         * */

        // 下级以及再下级是否有selected? (不包括自己)
        public bool HasChildrenSelected()
        {
            for (int i = 0; i < this.ChildrenCollection.Count; i++)
            {
                AreaBase obj = this.ChildrenCollection[i];
                if (obj.m_bSelected == true)
                    return true;

                // 递归
                if (obj.HasChildrenSelected() == true)
                    return true;
            }

            return false;
        }

        // 清除所有下级对象
        public virtual void Clear()
        {
            this.ChildrenCollection.Clear();

            m_bSelected = false;

            m_lWidthCache = -1;
            m_lHeightCache = -1;

            NameValue = 0;   // 0 表示尚未初始化
        }

        // 清除当前对象本身以及全部下级的选择标志
        public void ClearAllSubSelected()
        {
            this.m_bSelected = false;

            for (int i = 0; i < this.ChildrenCollection.Count; i++)
            {
                this.ChildrenCollection[i].ClearAllSubSelected();
            }
        }

        // 清除当前对象本身以及全部下级的选择标志, 并返回需要刷新的对象
        public void ClearAllSubSelected(ref List<AreaBase> objects,
            int nMaxCount)
        {

            // 修改过的才加入数组
            if (this.m_bSelected == true && objects.Count < nMaxCount)
                objects.Add(this);

            this.m_bSelected = false;

            for (int i = 0; i < this.ChildrenCollection.Count; i++)
            {
                this.ChildrenCollection[i].ClearAllSubSelected(ref objects,
                    nMaxCount);
            }
        }

        // 根据给出的NameValue值, 从当前对象开始(包括当前对象) 定位后代对象
        public AreaBase FindByNameValue(List<int> values)
        {
            if (values == null)
            {
                Debug.Assert(false, "values不能为空");
                return null;
            }

            if (values.Count == 0)
            {
                Debug.Assert(false, "values.Count不能为0");
                return null;
            }

            if (values[0] == -1 // -1表示通配符
                 || this.NameValue == values[0])
            {
                if (values.Count == 1)
                    return this;

                Debug.Assert(values.Count > 1, "");
                // 继续向下找

                // 缩短参数一级
                List<int> temp = new List<int>();
                temp.AddRange(values);
                temp.RemoveAt(0);

                // 继续向下找
                // 递归即可
                for (int i = 0; i < this.ChildrenCollection.Count; i++)
                {
                    AreaBase obj = this.ChildrenCollection[i];

                    AreaBase result_obj = obj.FindByNameValue(temp);
                    if (result_obj != null)
                        return result_obj;

                    // 优化
                    // 如果是obj自己这一级匹配成功，但是下面没有匹配成功
                    if (temp[0] != -1 // -1表示通配符
                        && obj.NameValue == temp[0])
                    {
                        return null;
                    }
            
                    /*
                    if (temp[0] != -1)
                        return null;    // 如果不是通配符模式, 到此就结束了
                     * */
                }

                return null;

            }
            else
                return null;


        }

        // 找到下一个同级对象（可以是跨越父亲、祖辈的）
        public AreaBase GetNextSibling()
        {
            if (this._Container == null)
                return null;

            List<AreaBase> children = this._Container.ChildrenCollection;

            for (int i = 0; i < children.Count - 1; i++)
            {
                if (children[i] == this)
                {
                    return children[i + 1];
                }
            }

            // List<AreaBase> stack = new List<AreaBase>();

            AreaBase parent = this._Container;
            //    stack.Add(this._Container);

            // 没有找到
            for (; ; )
            {
                // 找到父亲的兄弟
                AreaBase parent_sibling = parent.GetNextSibling();
                if (parent_sibling == null)
                    return null;

                List<AreaBase> temp_children = parent_sibling.ChildrenCollection;

                // 父亲兄弟的第一个儿子
                if (temp_children.Count != 0)
                    return temp_children[0];

                // 否则继续找父亲的兄弟

                parent = parent_sibling;
            }


            // return null;
        }

        // 找到前一个同级对象（可以是跨越父亲、祖辈的）
        public AreaBase GetPrevSibling()
        {
            if (this._Container == null)
                return null;

            List<AreaBase> children = this._Container.ChildrenCollection;

            for (int i = children.Count - 1; i > 0; i--)
            {
                if (children[i] == this)
                {
                    return children[i - 1];
                }
            }

            AreaBase parent = this._Container;

            // 没有找到
            for (; ; )
            {
                // 找到父亲的兄弟
                AreaBase parent_sibling = parent.GetPrevSibling();
                if (parent_sibling == null)
                    return null;

                List<AreaBase> temp_children = parent_sibling.ChildrenCollection;

                // 父亲兄弟的最末一个儿子
                if (temp_children.Count != 0)
                    return temp_children[temp_children.Count-1];

                // 否则继续找父亲的兄弟
                parent = parent_sibling;
            }


            // return null;
        }

        public virtual string FullName
        {
            get
            {
                return "尚未实现";
            }
        }


        // return:
        //      true    状态发生变化
        //      false   状态没有变化
        public bool Select(SelectAction action,
            bool bRecursive)
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

            // 递归
            if (bRecursive == true)
            {
                for (int i = 0; i < this.ChildrenCollection.Count; i++)
                {
                    AreaBase obj = this.ChildrenCollection[i];
                    obj.Select(action, true);
                }
            }

            return (bOldSelected == this.m_bSelected ? false : true);
        }

        public virtual long Width
        {
            get
            {
                throw new Exception("Width not implement");
            }
        }

        public virtual long Height
        {
            get
            {
                throw new Exception("Height not implement");
            }
        }

        // 获得子对象在 本对象坐标体系中的 左上角位置
        public virtual PointF GetChildLeftTopPoint(AreaBase child)
        {
            throw new Exception("尚未实现");
        }

        // 将本对象坐标体系 的矩形 转换为 顶层对象的坐标体系
        public virtual RectangleF ToRootCoordinate(RectangleF rect)
        {
            AreaBase obj = this;

            for (; ; )
            {
                AreaBase parent = obj._Container;
                if (parent == null)
                    break;

                PointF childStart = parent.GetChildLeftTopPoint(obj);

                // 变换为父的坐标
                rect.Offset(childStart.X, childStart.Y);

                obj = parent;
            }

            return rect;
        }

        // 画背景
        // parameters:
        //      colorBack   正常的背景颜色
        public virtual void PaintBack(
            long x0,
            long y0,
            long width,
            long height,
            PaintEventArgs e,
            Color colorBack)
        {
            RectangleF rect = new RectangleF(
                x0,
                y0,
                width,
                height);

            Rectangle rectClip = e.ClipRectangle;
            rectClip.Inflate(1, 1); // 交叉后的矩形，由于是float格式，容易丢失1像素
            RectangleF result = RectangleF.Intersect(rect, rectClip);

            if (result.IsEmpty)
                return;

            using (Brush brush = new SolidBrush(colorBack))
            {
                e.Graphics.FillRectangle(brush, result);
            }
        }

        // 画选择效果
        // parameters:
        public virtual void PaintSelectEffect(
            long x0,
            long y0,
            long width,
            long height,
            PaintEventArgs e)
        {
            if (this.m_bSelected == false)
                return;

            RectangleF rect = new RectangleF(
                x0,
                y0,
                width,
                height);

            Rectangle rectClip = e.ClipRectangle;
            rectClip.Inflate(1, 1); // 交叉后的矩形，由于是float格式，容易丢失1像素
            RectangleF result = RectangleF.Intersect(rect, rectClip);

            if (result.IsEmpty)
                return;

            using (Brush brush = new SolidBrush(Color.FromArgb(20, SystemColors.Highlight)))
            {
                e.Graphics.FillRectangle(brush, result);
            }
        }
    }


    // 专门为解决数组内元素类型问题而设计的数组包裹器
    // 强类型 T 的数组界面 (T为AreaBase的派生类)
    public class TypedList<T> where T : AreaBase
    {
        List<AreaBase> m_base_array = null;

        public TypedList(List<AreaBase> base_array)
        {
            this.m_base_array = base_array;
        }

        // 和一个List<AreaBase>型的数组连接起来
        public void Link(List<AreaBase> base_array)
        {
            this.m_base_array = base_array;
        }

        public T this[int index]
        {
            get
            {
                return (T)this.m_base_array[index];
            }
            set
            {
                this.m_base_array[index] = value;
            }
        }

        public int Count
        {
            get
            {
                return this.m_base_array.Count;
            }
        }

        /*
        // 两种名字都提供
        public int Length
        {
            get
            {
                return this.m_base_array.Count;
            }
        }*/

        public void Add(T obj)
        {
            this.m_base_array.Add(obj);
        }

        // 2010/3/21
        public void Remove(T obj)
        {
            this.m_base_array.Remove(obj);
        }

        public void Insert(int index, T obj)
        {
            this.m_base_array.Insert(index, obj);
        }

    }

    // 在AreaBase和实际派生类之间的一个缓冲类，用来隐藏一些繁琐的初始化细节。
    public class NamedArea<ChildType> : AreaBase
        where ChildType : AreaBase
    {
        public TypedList<ChildType> ChildTypedCollection = null;

        public NamedArea()
        {
            this.ChildTypedCollection = new TypedList<ChildType>(this.ChildrenCollection);
        }
    }

    // 包含若干年的顶层容器
    public class DataRoot : NamedArea<YearArea>
    {
        internal SizeF DpiXY = new SizeF(96, 96);

        internal int m_nYearNameWidth = 100; // 50 // 左边显示年名的竖道的宽度
        internal int m_nMonthNameWidth = 80;     // 左边显示月名的竖道的宽度

        internal int m_nDayCellWidth = 100; // 日格子的宽度
        internal int m_nDayCellHeight = 100;    // 日格子的高度

        internal Rectangle m_rectCheckBox = new Rectangle(4, 4, 16, 16); // checkbox矩形(在DayArea坐标内)

        internal int m_nDayOfWeekTitleHeight = 30;   // 星期标题的高度

        internal string m_strDayOfWeekTitleLang = "zh";

        public Font DayTextFont = new Font("Arial Black", 12, FontStyle.Regular);

        public Font DaysOfWeekTitleFont = new Font("楷体_GB2312", 11, FontStyle.Regular);

        public Color YearBackColor = Color.White;

        public Color MonthBackColor = Color.White;

        public bool HoverCheckBox = false;

        // "Tahoma" "Verdana" "Jokerman" "Rockwell Extra Bold"
        // "Century Gothic" "Croobie"

        bool m_bBackColorTransparent = false;


        public DayStateDefCollection DayStateDefs = new DayStateDefCollection();

        // 构造函数
        public DataRoot(SizeF dpi_xy)
        {
            if (dpi_xy.Width != 0)
                SetDpiXY(dpi_xy);

            this.DayTextFont = new Font("Arial Black", 12, FontStyle.Regular);
            if (m_strDayOfWeekTitleLang == "zh")
                this.DaysOfWeekTitleFont = new Font("楷体_GB2312", 11, FontStyle.Regular);
            else
                this.DaysOfWeekTitleFont = new Font("Arial", 11, FontStyle.Regular);
        }

        public void SetDpiXY(SizeF dpi_xy)
        {
            this.DpiXY = dpi_xy;

            m_nYearNameWidth = DpiUtil.GetScalingX(dpi_xy, 100);
            m_nMonthNameWidth = DpiUtil.GetScalingX(dpi_xy, 80);

            m_nDayCellWidth = DpiUtil.GetScalingX(dpi_xy, 100);
            m_nDayCellHeight = DpiUtil.GetScalingY(dpi_xy, 100);

            m_rectCheckBox = DpiUtil.GetScaingRectangle(dpi_xy, new Rectangle(4, 4, 16, 16));

            m_nDayOfWeekTitleHeight = DpiUtil.GetScalingY(dpi_xy, 30);
        }

        public bool BackColorTransparent
        {
            get
            {
                return m_bBackColorTransparent;
            }
            set
            {
                if (this.m_bBackColorTransparent == value)
                    return;

                this.m_bBackColorTransparent = value;

                if (this.m_bBackColorTransparent == true)
                {
                    // 变为透明
                    if (this.YearBackColor.A >= 255)
                        this.YearBackColor = Color.FromArgb(100, this.YearBackColor);
                    if (this.MonthBackColor.A >= 255)
                        this.MonthBackColor = Color.FromArgb(100, this.MonthBackColor);
                }
                else
                {
                    // 变为不透明
                    this.YearBackColor = Color.FromArgb(255, this.YearBackColor);
                    this.MonthBackColor = Color.FromArgb(255, this.MonthBackColor);
                }
            }
        }

        // 将时间范围删除一年(头部或者尾部)
        public bool ShrinkYear(bool bHead)
        {
            YearArea first_year = null;

            if (this.YearCollection.Count > 0)
            {
                // 得到现存第一年

                if (bHead == true)
                {
                    first_year = (YearArea)this.FirstChild;
                }
                else
                {
                    first_year = (YearArea)this.LastChild;
                }
            }
            else
            {
                return false;
            }

            this.YearCollection.Remove(first_year);
            this.ClearCache();

            return true;
        }

        // 将时间范围删除一月(头部或者尾部)
        public bool ShrinkMonth(bool bHead)
        {
            YearArea first_year = null;

            if (this.YearCollection.Count == 0)
            {
                return false;
            }
            else
                first_year = (YearArea)this.EdgeChild(bHead);

            Debug.Assert(first_year != null, "");

            bool bRet = first_year.ShrinkMonth(bHead);

            // 注意善后
            if (first_year.MonthCollection.Count == 0)
                this.YearCollection.Remove(first_year);

            return bRet;
        }

        // 将时间范围删除一星期
        public bool ShrinkWeek(bool bHead)
        {
            YearArea first_year = null;

            if (this.YearCollection.Count == 0)
            {
                return false;
            }
            else
                first_year = (YearArea)this.EdgeChild(bHead);

            Debug.Assert(first_year != null, "");

            MonthArea first_month = null;

            if (first_year.MonthCollection.Count == 0)
            {
                return false;
            }
            else
            {
                first_month = (MonthArea)first_year.EdgeChild(bHead);
            }

            Debug.Assert(first_month != null, "");

            bool bRet = first_month.ShrinkWeek(bHead);

            // 注意善后
            if (bRet == true 
                && first_month.WeekCollection.Count == 0)
            {
                first_year.MonthCollection.Remove(first_month);
                if (first_year.MonthCollection.Count == 0)
                    this.YearCollection.Remove(first_year);
            }

            return bRet;
        }

        // 将时间范围扩展一年
        // 注意需要检查原有的第一年或者最后一年是否完整
        public YearArea ExpandYear(bool bHead, 
            bool bEmpty)
        {
            int nNewYear = 0;

            if (this.YearCollection.Count > 0)
            {
            // 得到现存第一年
                YearArea first_year = null;

                if (bHead == true)
                {
                    first_year = (YearArea)this.FirstChild;
                    // 确保年完整
                    first_year.CompleteMonth(bHead);

                    nNewYear = first_year.Year - 1;
                    if (nNewYear < 0)
                        throw new Exception("年到达最小极限值");
                }
                else
                {
                    first_year = (YearArea)this.LastChild;

                    // 确保年完整
                    first_year.CompleteMonth(bHead);

                    nNewYear = first_year.Year + 1;
                    if (nNewYear > 9999)
                        throw new Exception("年到达最大极限值");

                }
            }
            else
            {
                // 根据当前时间产生新对象的年份值
                DateTime now = DateTime.Now;
                nNewYear = now.Year;
            }

            YearArea new_year = new YearArea(this, nNewYear, bEmpty);
            if (bHead == true)
                this.YearCollection.Insert(0, new_year);
            else
                this.YearCollection.Add(new_year);

            this.ClearCache();

            return new_year;
        }

        // 将时间范围扩展一月
        public MonthArea ExpandMonth(bool bHead,
            bool bEmpty)
        {
            YearArea first_year = null;

            if (this.YearCollection.Count == 0)
            {
                // 先扩充一个空的年
                first_year = this.ExpandYear(bHead, true);
            }
            else
                first_year = (YearArea)this.EdgeChild(bHead);

            Debug.Assert(first_year != null, "");

            return first_year.ExpandMonth(bHead,
                bEmpty);
        }

        // 将时间范围扩展一星期
        public WeekArea ExpandWeek(bool bHead,
            bool bEmpty)
        {
            YearArea first_year = null;

            if (this.YearCollection.Count == 0)
            {
                // 先扩充一个空的年
                first_year = this.ExpandYear(bHead, true);
            }
            else
                first_year = (YearArea)this.EdgeChild(bHead);

            Debug.Assert(first_year != null, "");

            MonthArea first_month = null;

            if (first_year.MonthCollection.Count == 0)
            {
                // 先扩充一个空的月
                first_month = first_year.ExpandMonth(bHead, true);
            }
            else
            {
                first_month = (MonthArea)first_year.EdgeChild(bHead);
            }

            Debug.Assert(first_month != null, "");

            return first_month.ExpandWeek(bHead,
                bEmpty);
        }


        // 再换一个名字也无妨
        // 这是最后一点“盲肠”。如果generic支持根据类型名字符串构造动态的函数名就好了
        public TypedList<YearArea> YearCollection
        {
            get
            {
                return this.ChildTypedCollection;    // 其实NamedCollection也很好用，就是名字没有特色
            }
        }

        // 创建
        // parameters:
        //      nStartYear  开始年
        //      nEndYear    结束年(包含此年)
        public int Build(int nStartYear,
            int nEndYear,
            out string strError)
        {
            strError = "";

            if (nStartYear > nEndYear)
            {
                strError = "起始年不应大于结束年";
                return -1;
            }

            for (int i = nStartYear; i <= nEndYear; i++)
            {
                YearArea year = new YearArea(this, i);

                this.ChildrenCollection.Add(year);
            }

            return 0;
        }

        #region DataRoot重载AreaBase的virtual函数

        public override string FullName
        {
            get
            {
                return "DataRoot";
            }
        }


        public override long Height
        {
            get
            {
                if (m_lHeightCache == -1)
                {
                    long lHeight = 0;
                    for (int i = 0; i < this.ChildrenCollection.Count; i++)
                    {
                        lHeight += ((YearArea)this.ChildrenCollection[i]).Height;
                    }

                    m_lHeightCache = lHeight;
                }

                return m_lHeightCache;
            }
        }

        public override long Width
        {
            get
            {
                if (m_lWidthCache == -1)
                    m_lWidthCache = (7 * this.m_nDayCellWidth) + this.m_nMonthNameWidth + this.m_nYearNameWidth;

                return m_lWidthCache;
            }
        }

        // 获得子对象在 本对象坐标体系中的 左上角位置
        public override PointF GetChildLeftTopPoint(AreaBase child)
        {
            if (!(child is YearArea))
                throw new Exception("child只能为YearArea类型");

            YearArea year = (YearArea)child;

            bool bFound = false;
            long lHeight = 0;
            for (int i = 0; i < this.ChildrenCollection.Count; i++)
            {
                YearArea cur_year = (YearArea)this.ChildrenCollection[i];
                if (cur_year == year)
                {
                    bFound = true;
                    break;
                }
                lHeight += cur_year.Height;
            }

            if (bFound == false)
                throw new Exception("child在子对象中没有找到");

            return new PointF(0,
                lHeight);
        }


        #endregion

        // 检查一个long是否越过int16能表达的值范围
        public static bool TooLarge(long lValue)
        {
            if (lValue >= Int16.MaxValue || lValue <= Int16.MinValue)
                return true;
            return false;
        }

        // 数据范围之最小年
        public int MinYear
        {
            get
            {
                if (this.ChildrenCollection.Count == 0)
                    return -1;  // 表示尚未初始化
                return ((YearArea)this.ChildrenCollection[0]).Year;
            }
        }

        // 数据范围之最大年
        public int MaxYear
        {
            get
            {
                if (this.ChildrenCollection.Count == 0)
                    return -1;  // 表示尚未初始化
                return ((YearArea)this.ChildrenCollection[this.ChildrenCollection.Count - 1]).Year;
            }
        }

        // 点击检测
        // parameters:
        //      p_x   已经是文档坐标。即文档左上角为(0,0)
        public void HitTest(
            long p_x,
            long p_y,
            Type dest_type,
            out HitTestResult result)
        {
            result = null;

            if (dest_type == typeof(DataRoot))
            {
                result = new HitTestResult();
                result.Object = this;
                result.AreaPortion = AreaPortion.Content;
                result.X = p_x;
                result.Y = p_y;
                return;
            }

            // 如果有特殊要求，上部也算作第一个年的
            if (p_y < 0 && dest_type != null)
            {
                if (this.YearCollection.Count > 1)
                {
                    // 确定在一个YearArea对象中
                    this.YearCollection[0].HitTest(p_x,
                        p_y,
                        dest_type,
                        out result);
                    return;
                }
            }

            long y = 0;
            for (int i = 0; i < this.YearCollection.Count; i++)
            {
                // 优化
                if (dest_type == null
                    && y > p_y)
                    break;

                YearArea year = this.YearCollection[i];

                long lYearHeight = year.Height;

                if (p_y >= y && p_y < y + lYearHeight)
                {
                    // 确定在一个YearArea对象中
                    year.HitTest(p_x,
                        p_y - y,
                        dest_type,
                        out result);
                    return;
                }
                y += lYearHeight;
            }

            // 如果有特殊要求，下部也算作最后一个年的
            if (dest_type != null)
            {
                if (this.YearCollection.Count > 1)
                {
                    // 确定在一个YearArea对象中
                    this.YearCollection[this.YearCollection.Count - 1].HitTest(p_x,
                        p_y - y,
                        dest_type,
                        out result);
                    return;
                }
            }

            // 如果没有匹配上任何YearArea对象
            result = new HitTestResult();
            result.Object = this;
            result.AreaPortion = AreaPortion.BottomBlank;
            result.X = p_x;
            result.Y = p_y;
        }

        // 绘图 根容器
        public void Paint(
            long x0,
            long y0,
            PaintEventArgs e)
        {
            /*
            if (TooLarge(start_x) == true
                || TooLarge(start_y) == true )
                return;
             */

            /*
            PaintBack(
                x0,
                y0,
                0,
                this.Height,
                e,
                Color.White);
             * */


            long x = x0;
            long y = y0;

            bool bDrawBottomLine = true;    // 是否要画下方线条

            long lYearWidth = this.Width;

            long lHeight = 0;
            for (int i = 0; i < this.YearCollection.Count; i++)
            {
                YearArea year = this.YearCollection[i];
                long lYearHeight = year.Height;

                if (TooLarge(x) == true
                    || TooLarge(y) == true)
                    goto CONTINUE;

                // 优化
                RectangleF rect = new RectangleF((int)x,
                    (int)y,
                    lYearWidth,
                    lYearHeight);

                if (y > e.ClipRectangle.Y + e.ClipRectangle.Height)
                {
                    bDrawBottomLine = false;
                    break;
                }


                if (rect.IntersectsWith(e.ClipRectangle) == false)
                    goto CONTINUE;

                year.Paint((int)x, (int)y, e);

            CONTINUE:
                y += lYearHeight;
                lHeight += lYearHeight;
            }

            // 右、下线条

            using (Pen penBold = new Pen(Color.FromArgb(50, Color.Gray), (float)2))
            {

                // 右方竖线
                if (TooLarge(x0 + this.Width) == false)
                {
                    e.Graphics.DrawLine(penBold,
                        new PointF((int)x0 + this.Width, (int)y0),
                        new PointF((int)x0 + this.Width, (int)(y0 + lHeight))
                        );
                }

                // 下方横线
                if (bDrawBottomLine == true
                    && TooLarge(y0 + lHeight) == false)
                {

                    e.Graphics.DrawLine(penBold,
                        new PointF((int)x0, (int)(y0 + lHeight)),
                        new PointF((int)x0 + this.Width, (int)(y0 + lHeight))
                        );
                }
            }

        }

        /*
        // 找到指定日期的DayArea对象
        public DayArea FindDayArea(int year, int month, int day)
        {
            for (int i = 0; i < this.ChildrenCollection.Count; i++)
            {
                YearArea cur_year = (YearArea)this.ChildrenCollection[i];
                if (cur_year.Year == year)
                    return cur_year.FindDayArea(month, day);

                // 优化
                if (year < cur_year.Year)
                    return null;
            }

            return null;
        }
         * */

        // 找到指定日期的DayArea对象
        public DayArea FindDayArea(int year, int month, int day)
        {
            List<int> values = new List<int>();
            values.Add(-1); // -1 表示本对象这一级, 通配
            values.Add(year);
            values.Add(month);
            values.Add(-1); // -1 表示星期哪一级, 通配
            values.Add(day);

            return (DayArea)this.FindByNameValue(values);
        }

        // 选择位于矩形内的对象
        public void Select(RectangleF rect,
            SelectAction action,
            List<Type> types,
            ref List<AreaBase> update_objects,
            int nMaxCount)
        {
            RectangleF rectThis = new RectangleF(0, 0, this.Width, this.Height);

            if (rectThis.IntersectsWith(rect) == true)
            {
                /*
                if (types.IndexOf(this.GetType()) != -1)
                {
                    bool bRet = this.Select(action, false);
                    if (bRet == true && update_objects.Count < nMaxCount)
                    {
                        update_objects.Add(this);
                    }
                }*/

                long y = 0;
                for (int i = 0; i < this.YearCollection.Count; i++)
                {
                    YearArea year = this.YearCollection[i];

                    // 优化
                    if (y > rect.Bottom)
                        break;

                    // 变换为year内坐标
                    RectangleF rectYear = rect;
                    rectYear.Offset(0, -y);

                    year.Select(rectYear,
                        action,
                        types,
                        ref update_objects,
                        nMaxCount);

                    y += year.Height;
                }
            }

        }
    }

    // 年
    public class YearArea : NamedArea<MonthArea>
    {
        // 确保一年中的前方或者后方月份完整。
        // return:
        //      是否发生了增补
        public bool CompleteMonth(bool bHead)
        {
            if (this.MonthCollection.Count == 12)
                return false;

            bool bChanged = false;

            while(true)
            {
                MonthArea first_month = null;
                if (this.MonthCollection.Count > 0)
                {
                    if (bHead == true)
                    {
                        first_month = (MonthArea)this.FirstChild;
                        if (first_month.Month <= 1)
                            break;
                    }
                    else
                    {
                        first_month = (MonthArea)this.LastChild;
                        if (first_month.Month >= 12)
                            break;
                    }
                }

                ExpandMonth(bHead, false);
                bChanged = true;
            }

            return bChanged;
        }

        // 将时间范围删除一月
        // return:
        public bool ShrinkMonth(bool bHead)
        {
            int nNewMonth = 0;

                MonthArea first_month = null;
            if (this.MonthCollection.Count > 0)
            {
                // 得到现存第一月

                if (bHead == true)
                {
                    first_month = (MonthArea)this.FirstChild;
                }
                else
                {
                    first_month = (MonthArea)this.LastChild;
                }
            }
            else
            {
                return false;
            }

            this.MonthCollection.Remove(first_month);
            this.ClearCache();
            return true;
        }

        // 将时间范围扩展一月
        // 注意需要检查原有的第一月或者最后一月是否完整
        // return:
        //      其他    所创建的新月份对象
        public MonthArea ExpandMonth(bool bHead, 
            bool bEmpty)
        {
            int nNewMonth = 0;

            if (this.MonthCollection.Count > 0)
            {
                // 得到现存第一月
                MonthArea first_month = null;

                if (bHead == true)
                {
                    first_month = (MonthArea)this.FirstChild;
                    // 确保月完整
                    first_month.CompleteWeek(bHead);
                    nNewMonth = first_month.Month - 1;
                    if (nNewMonth == 0)
                    {
                        // 需要先扩年
                        return this.Container.ExpandYear(bHead, true).ExpandMonth(bHead, bEmpty);
                    }
                }
                else
                {
                    first_month = (MonthArea)this.LastChild;
                    // 确保月完整
                    first_month.CompleteWeek(bHead);
                    nNewMonth = first_month.Month + 1;
                    if (nNewMonth >= 13)
                    {
                        // 需要先扩年
                        return this.Container.ExpandYear(bHead, true).ExpandMonth(bHead, bEmpty);
                    }
                }
            }
            else
            {
                if (bHead == true)
                    nNewMonth = 12;
                else
                    nNewMonth = 1;
            }

            MonthArea new_month = new MonthArea(this, nNewMonth, bEmpty);
            if (bHead == true)
                this.MonthCollection.Insert(0, new_month);
            else
                this.MonthCollection.Add(new_month);

            this.ClearCache();

            return new_month;
        }

        // 扩展一个星期
        public WeekArea ExpandWeek(bool bHead,
            bool bEmpty)
        {
            MonthArea first_month = null;

            if (this.MonthCollection.Count == 0)
            {
                // 先扩充一个空的月
                first_month = this.ExpandMonth(bHead, true);
            }
            else
            {
                first_month = (MonthArea)this.EdgeChild(bHead);
            }

            Debug.Assert(first_month != null, "");

            return first_month.ExpandWeek(bHead,
                bEmpty);
        }

        public DataRoot Container
        {
            get
            {
                return (DataRoot)this._Container;
            }
        }

        // 构造函数
        // (参数包装版本)
        public YearArea(DataRoot container,
            int nYear)
        {
            InitialYearArea(container, nYear, false);
        }

        // 构造函数
        public YearArea(DataRoot container,
            int nYear,
            bool bEmpty)
        {
            InitialYearArea(container, nYear, bEmpty);
        }

        // 构造函数
        void InitialYearArea(DataRoot container,
            int nYear,
            bool bEmpty)
        {
            this._Container = container;

            this.Year = nYear;

            if (bEmpty == false)
            {
                // 创建月数组
                for (int i = 0; i < 12; i++)
                {
                    MonthArea month = new MonthArea(this, i + 1);
                    this.ChildrenCollection.Add(month);
                }
            }
        }

        // 别名
        public TypedList<MonthArea> MonthCollection
        {
            get
            {
                return this.ChildTypedCollection;    // 其实NamedCollection也很好用，就是名字没有特色
            }
        }

        // 选择位于矩形内的对象
        public void Select(RectangleF rect,
            SelectAction action,
            List<Type> types,
            ref List<AreaBase> update_objects,
            int nMaxCount)
        {
            RectangleF rectThis = new RectangleF(0, 0, this.Width, this.Height);

            if (rectThis.IntersectsWith(rect) == true)
            {

                RectangleF rectName = new RectangleF(0, 0,
                    this.DataRoot.m_nYearNameWidth,
                    this.Height);

                if (rectName.IntersectsWith(rect) == true)
                {
                    if (types.IndexOf(this.GetType()) != -1)
                    {
                        bool bRet = this.Select(action, true);
                        if (bRet == true && update_objects.Count < nMaxCount)
                        {
                            update_objects.Add(this);
                        }
                    }
                }



                long y = 0;
                for (int i = 0; i < this.MonthCollection.Count; i++)
                {
                    MonthArea month = this.MonthCollection[i];

                    // 优化
                    if (y > rect.Bottom)
                        break;

                    

                    // 变换为month内坐标
                    RectangleF rectMonth = rect;
                    rectMonth.Offset(-this.DataRoot.m_nYearNameWidth, -y);

                    month.Select(rectMonth,
                        action,
                        types,
                        ref update_objects,
                        nMaxCount);

                    y += month.Height;
                }
            }

        }

        #region YearArea重载AreaBase的virtual函数

        public override string FullName
        {
            get
            {
                return this.YearName;
            }
        }


        public override long Height
        {
            get
            {
                if (m_lHeightCache == -1)
                {
                    long lHeight = 0;
                    for (int i = 0; i < this.MonthCollection.Count; i++)
                    {
                        lHeight += this.MonthCollection[i].Height;
                    }
                    m_lHeightCache = lHeight;
                }

                return m_lHeightCache;
            }
        }

        public override long Width
        {
            get
            {
                if (m_lWidthCache == -1)
                    m_lWidthCache = (7 * this.DataRoot.m_nDayCellWidth) + this.DataRoot.m_nMonthNameWidth + this.DataRoot.m_nYearNameWidth;

                return m_lWidthCache;
            }
        }

        /*
        public override bool Select(SelectAction action,
bool bRecursive)
        {
            bool bRet = base.Select(action, bRecursive);

            // 递归
            if (bRecursive == true)
            {
                for (int i = 0; i < this.MonthCollection.Count; i++)
                {
                    MonthArea month = this.MonthCollection[i];
                    month.Select(action, true);
                }
            }

            return bRet;
        }*/

        // 获得子对象在 本对象坐标体系中的 左上角位置
        public override PointF GetChildLeftTopPoint(AreaBase child)
        {
            if (!(child is MonthArea))
                throw new Exception("child只能为MonthArea类型");

            MonthArea month = (MonthArea)child;

            bool bFound = false;
            long lHeight = 0;
            for (int i = 0; i < this.MonthCollection.Count; i++)
            {
                MonthArea cur_month = this.MonthCollection[i];
                if (cur_month == month)
                {
                    bFound = true;
                    break;
                }
                lHeight += cur_month.Height;
            }

            if (bFound == false)
                throw new Exception("child在子对象中没有找到");

            return new PointF(this.DataRoot.m_nYearNameWidth,
                lHeight);
        }


        #endregion

        public DataRoot DataRoot
        {
            get
            {
                return (DataRoot)this._Container;
            }
        }


        // 点击检测
        public void HitTest(
            long p_x,
            long p_y,
            Type dest_type,
            out HitTestResult result)
        {
            result = null;

            if (dest_type == typeof(YearArea))
            {
                result = new HitTestResult();
                result.Object = this;
                result.AreaPortion = AreaPortion.Content;
                result.X = p_x;
                result.Y = p_y;
                return;
            }

            // 看看是不是在左边的竖道上
            // dest_type如果有要求，则把左边竖道也让给下级判断
            if (dest_type == null
                && p_x < this.DataRoot.m_nYearNameWidth)
            {
                result = new HitTestResult();
                result.AreaPortion = AreaPortion.LeftBar;
                result.Object = this;
                result.X = p_x;
                result.Y = p_y;
                return;
            }

            // 如果有特殊要求，上部也算作第一个月份的
            if (p_y < 0 && dest_type != null)
            {
                if (this.MonthCollection.Count > 1)
                {
                    this.MonthCollection[0].HitTest(p_x - this.DataRoot.m_nYearNameWidth,
                        p_y - 0,
                        dest_type,
                        out result);
                    return;
                }
            }

            long y = 0;
            for (int i = 0; i < this.MonthCollection.Count; i++)
            {
                // 优化
                if (dest_type == null
                    && y > p_y)
                    break;

                MonthArea month = this.MonthCollection[i];

                long lMonthHeight = month.Height;

                if (p_y >= y && p_y < y + lMonthHeight)
                {
                    // 确定在一个MonthArea对象中
                    month.HitTest(p_x - this.DataRoot.m_nYearNameWidth,
                        p_y - y,
                        dest_type,
                        out result);
                    return;
                }
                y += lMonthHeight;
            }

            // 如果有特殊要求，下部也算作最后一个月份的
            if (dest_type != null)
            {
                if (this.MonthCollection.Count > 1)
                {
                    this.MonthCollection[this.MonthCollection.Count - 1].HitTest(p_x - this.DataRoot.m_nYearNameWidth,
                        p_y - y,
                        dest_type,
                        out result);
                    return;
                }
            }

            // 如果没有匹配上任何MonthArea对象
            result = new HitTestResult();
            result.Object = this;
            result.AreaPortion = AreaPortion.BottomBlank;
            result.X = p_x;
            result.Y = p_y;
        }

        // 绘图　年
        public void Paint(
            long start_x,
            long start_y,
            PaintEventArgs e)
        {
            if (DataRoot.TooLarge(start_x) == true
                || DataRoot.TooLarge(start_y) == true)
                return;

            /*
            PaintBack(
    start_x,
    start_y,
    // this.Width, // 这里不优化，选中的时候刷新反而不闪动 
    this.DataRoot.m_nYearNameWidth,
    this.Height,
    e,
                Color.White);
             * */

            PaintBack(
                start_x,
                start_y,
                this.Width, // 这里不优化，加上提前在内容域应用选中效果，那么选中的时候刷新反而不闪动 
                this.Height,
                e,
                this.DataRoot.YearBackColor);

            // 为防止闪动而增加的代码
            if (this.m_bSelected == true)
            {
                // 提前把透明叠加后的背景颜色模拟出来
                Color colorMask = SystemColors.Highlight;
                Color colorBase = Color.White;
                int r = (byte)((float)colorBase.R * ((255F - 20F) / 255F)
                     + (float)colorMask.R * (20F / 255F));
                int g = (byte)((float)colorBase.G * ((255F - 20F) / 255F)
                     + (float)colorMask.G * (20F / 255F));
                int b = (byte)((float)colorBase.B * ((255F - 20F) / 255F)
     + (float)colorMask.B * (20F / 255F));

                this.PaintBack(
                    start_x + this.DataRoot.m_nYearNameWidth,
                    start_y,
                    this.Width - this.DataRoot.m_nYearNameWidth,
                    this.Height,
                    e,
                    Color.FromArgb(r,g,b));
            }


            int x0 = (int)start_x;
            int y0 = (int)start_y;

            RectangleF rect;

            // 绘制年名字
            rect = new RectangleF(
x0,
y0,
this.DataRoot.m_nYearNameWidth,
this.Height);

            // 优化
            if (rect.IntersectsWith(e.ClipRectangle) == true)
            {
                PaintYearName(
                    x0,
                    y0,
                    this.DataRoot.m_nYearNameWidth,
                    (int)this.Height,
                    e);

                // 选择后的效果
                if (this.m_bSelected == true)
                {
                    this.PaintSelectEffect(
                    x0,
                    y0,
                    this.DataRoot.m_nYearNameWidth,
                    (int)this.Height,
        e);
                }
            }

            int x = x0;
            int y = y0;

            // 绘制月份
            x = x0 + this.DataRoot.m_nYearNameWidth;
            y = y0;
            for (int i = 0; i < this.MonthCollection.Count; i++)
            {
                MonthArea month = this.MonthCollection[i];

                rect = new RectangleF(
    x,
    y,
    month.Width,
    month.Height);

                // 提前结束循环
                if (rect.Y > e.ClipRectangle.Bottom)
                    break;

                // 优化
                if (rect.IntersectsWith(e.ClipRectangle) == true)
                    month.Paint(x, y, e);

                long lMonthHeight = month.Height;
                y += (int)lMonthHeight;
                // nHeight += (int)lMonthHeight;
            }
        }

        // 绘年名字
        void PaintYearName(
            int x0,
            int y0,
            int nWidth,
            int nHeight,
            PaintEventArgs e)
        {
            using (Pen penBold = new Pen(Color.FromArgb(50, Color.Gray), (float)2))
            {
                // 左方竖线
                e.Graphics.DrawLine(penBold,
                    new PointF(x0, y0),
                    new PointF(x0, y0 + nHeight)
                    );

            // 上方横线
            e.Graphics.DrawLine(penBold,
                new PointF(x0, y0),
                new PointF(x0 + nWidth, y0)
                );
            }

            int nFontHeight = Math.Min(nWidth, nHeight / 5);

            using(Font font = new Font("Arial", nFontHeight, FontStyle.Bold, GraphicsUnit.Pixel))
            using (Brush brushText = new SolidBrush(Color.Blue))
            {
                RectangleF rect = new RectangleF(
        x0 + nWidth / 4,
        y0,
        nWidth / 2,
        nHeight);

                StringFormat stringFormat = new StringFormat();

                stringFormat.Alignment = StringAlignment.Center;
                stringFormat.LineAlignment = StringAlignment.Center;

                e.Graphics.DrawString(this.YearName,
                    font,
                    brushText,
                    rect,
                    stringFormat);
            }
        }


        // 年份值
        public int Year
        {
            get
            {
                return this.NameValue;
                // return this.m_nYear;
            }
            set
            {
                this.NameValue = value;
                // this.m_nYear = value;
            }
        }

        public string YearName
        {
            get
            {
                return this.Year.ToString().PadLeft(4, '0');
            }

        }


        // 本年之后一个年对象
        public YearArea NextYearArea
        {
            // 改写
            get
            {
                return (YearArea)this.GetNextSibling();
            }

            /*
            get
            {
                int nIndex = this.Container.YearCollection.IndexOf(this);

                if (nIndex == -1)
                    throw new Exception("当前YearArea对象不在容器中");

                if (nIndex + 1 < this.Container.YearCollection.Count)
                    return this.Container.YearCollection[nIndex + 1];

                return null;
            }
             * */
        }

        public MonthArea FirstMonthArea
        {
            get
            {
                if (this.ChildrenCollection.Count == 0)
                    return null;

                return (MonthArea)this.ChildrenCollection[0];
            }
        }

        /*
        // 本年第一个“天”对象
        public DayArea FirstDayArea
        {
            get
            {
                if (this.MonthCollection.Length == 0)
                    return null;

                return this.MonthCollection[0].FirstDayArea;
            }
        }*/

        /*
        // 找到指定日期的DayArea对象
        public DayArea FindDayArea(int month, int day)
        {
            if (this.ChildrenCollection.Count == 0)
                return null;

            MonthArea cur_month = null;

            // 优化
            if (((MonthArea)this.ChildrenCollection[0]).Month == 1)
            {
                // 说明第一个月份对象是1月，month就可以用作下标
                if (month - 1 < this.ChildrenCollection.Count)
                    cur_month = (MonthArea)this.ChildrenCollection[month - 1];
                else
                    return null;
            }
            else
            {
                // 查找
                for (int i = 0; i < this.ChildrenCollection.Count; i++)
                {
                    cur_month = (MonthArea)this.ChildrenCollection[i];
                    if (cur_month.Month == month)
                        goto FOUND;


                    if (month > cur_month.Month)    // 优化
                        return null;
                }

                return null;
            }

            FOUND:
            if (cur_month != null)
                return cur_month.FindDayArea(day);

            return null;
        }
        */

    }

    // 月
    public class MonthArea : NamedArea<WeekArea>
    {
        // public List<WeekArea> WeekCollection = new List<WeekArea>();

        // int m_nMonth = 0;   // 尚未初始化

        // 本月最大星期数
        public int MaxWeekCount
        {
            get
            {
                int nWeekCount = 0;

                // 起始日 1号
                DateTime date = new DateTime(this.Year,
                    this.Month,
                    1);

                int nStartIndex = Convert.ToInt32(date.DayOfWeek);   // 起始日的序号。0为星期天
                int nMaxDays = this.Days;
                int nDay = 1;
                bool bBlank = true;
                for (int nCurWeek = 1; nCurWeek <= 6 && nDay <= nMaxDays; nCurWeek++)
                {
                    for (int nDayOfWeek = 0; nDayOfWeek < 7; nDayOfWeek++)
                    {
                        // 变为不是空白
                        if (nCurWeek == 1 && nDayOfWeek >= nStartIndex)
                            bBlank = false;

                        // 变回空白
                        if (nDay > nMaxDays)
                            bBlank = true;

                        if (bBlank == false)
                            nDay++;
                    }
                    nWeekCount++;
                }

                return nWeekCount;
            }
        }

        // 确保一月中的前方或者后方星期完整。
        // return:
        //      是否发生了增补
        public bool CompleteWeek(bool bHead)
        {
            if (this.WeekCollection.Count == 6)
                return false;

            bool bChanged = false;

            while (true)
            {
                WeekArea first_week = null;
                if (this.WeekCollection.Count > 0)
                {
                    if (bHead == true)
                    {
                        first_week = (WeekArea)this.FirstChild;
                        if (first_week.MinDay <= 1)
                            break;
                    }
                    else
                    {
                        first_week = (WeekArea)this.LastChild;
                        if (first_week.MaxDay >= first_week.Container.Days)
                            break;
                    }
                }

                ExpandWeek(bHead, false);
                bChanged = true;
            }

            return bChanged;
        }

        // 将时间范围删除一星期
        // return:
        public bool ShrinkWeek(bool bHead)
        {
                WeekArea first_week = null;
            if (this.WeekCollection.Count > 0)
            {
                // 得到现存第一星期
                if (bHead == true)
                {
                    first_week = (WeekArea)this.FirstChild;
                }
                else
                {
                    first_week = (WeekArea)this.LastChild;
                }
            }
            else
            {
                return false;
            }

            this.WeekCollection.Remove(first_week);
            this.ClearCache();

            return true;
        }

        // 将时间范围扩展一星期
        // 注意需要检查原有的第一星期或者最后一星期是否完整
        // return:
        //      其他    所创建的新的星期对象
        public WeekArea ExpandWeek(bool bHead,
            bool bEmpty)
        {
            int nNewWeek = 0;
            if (this.WeekCollection.Count > 0)
            {
                // 得到现存第一星期
                WeekArea first_week = null;

                if (bHead == true)
                {
                    first_week = (WeekArea)this.FirstChild;
                    nNewWeek = first_week.Week - 1;
                    if (nNewWeek == 0)
                    {
                        // 需要先扩月
                        return this.Container.ExpandMonth(bHead, true).ExpandWeek(bHead, bEmpty);
                    }
                }
                else
                {
                    first_week = (WeekArea)this.LastChild;
                    nNewWeek = first_week.Week + 1;
                    if (first_week.MaxDay >= first_week.Container.Days)
                    {
                        // 需要先扩月
                        return this.Container.ExpandMonth(bHead, true).ExpandWeek(bHead, bEmpty);
                    }
                }
            }
            else
            {
                if (bHead == true)
                    nNewWeek = this.MaxWeekCount;  // 末星期
                else
                    nNewWeek = 1;
            }

            WeekArea new_week = new WeekArea(this, nNewWeek);
            if (bHead == true)
                this.WeekCollection.Insert(0, new_week);
            else
                this.WeekCollection.Add(new_week);

            this.ClearCache();

            return new_week;
        }

        public YearArea Container
        {
            get
            {
                return (YearArea)this._Container;
            }
        }

                // 构造函数
        // parameters:
        //      nMonth  月份数。从1开始计数
        public MonthArea(YearArea container,
            int nMonth,
            bool bEmpty)
        {
            InitialMonthArea(container, nMonth, bEmpty);
        }

        // 构造函数
        // parameters:
        //      nMonth  月份数。从1开始计数
        public MonthArea(YearArea container,
            int nMonth)
        {
            InitialMonthArea(container, nMonth, false);
        }

        // 构造函数实际功能
        // parameters:
        //      nMonth  月份数。从1开始计数
        void InitialMonthArea(YearArea container,
            int nMonth,
            bool bEmpty)
        {
            this._Container = container;

            this.Month = nMonth;

            if (bEmpty == true)
                return;

            // 创建周数组和下属的日数组
            int nDays = this.Days;

            WeekArea week = null;
            int nWeek = 0;
            for (int i = 0; i < nDays; i++)
            {
                // this.Days可以得知本月有多少天
                // DataTime.DayOfWeek可以用来探测本月一号为星期几，这样一直推算下去
                // 就可以得知有多少个星期。
                // 注意，DayOfWeek的0表示星期天。A DayOfWeek enumerated constant that indicates the day of the week. This property value ranges from zero, indicating Sunday, to six, indicating Saturday. 
                // 另外建议推算的过程，也是同时创建WeekArea和DayArea对象的过程，不必再分两级去创建了
                DateTime date = new DateTime(this.Container.Year,
                    this.Month,
                    i + 1);

                // 逢上本月第一天，或者每周第一天（星期天），才代为创建星期对象
                if (i == 0
                    || date.DayOfWeek == 0)
                {
                    week = new WeekArea(this, i + 1, ++nWeek);
                    this.ChildrenCollection.Add(week);
                }
            }

        }

        // 别名
        public TypedList<WeekArea> WeekCollection
        {
            get
            {
                return this.ChildTypedCollection;    // 其实NamedCollection也很好用，就是名字没有特色
            }
        }

        // 选择位于矩形内的对象
        public void Select(RectangleF rect,
            SelectAction action,
            List<Type> types,
            ref List<AreaBase> update_objects,
            int nMaxCount)
        {
            RectangleF rectThis = new RectangleF(0, 0, this.Width, this.Height);

            if (rectThis.IntersectsWith(rect) == true)
            {
                RectangleF rectName = new RectangleF(0, 0,
    this.DataRoot.m_nMonthNameWidth,
    this.Height);

                if (rectName.IntersectsWith(rect) == true)
                {
                    if (types.IndexOf(this.GetType()) != -1)
                    {
                        bool bRet = this.Select(action, true);
                        if (bRet == true && update_objects.Count < nMaxCount)
                        {
                            update_objects.Add(this);
                        }
                    }
                }

                long y = this.DataRoot.m_nDayOfWeekTitleHeight;
                for (int i = 0; i < this.WeekCollection.Count; i++)
                {
                    WeekArea week = this.WeekCollection[i];

                    // 优化
                    if (y > rect.Bottom)
                        break;

                    // 变换为week内坐标
                    RectangleF rectWeek = rect;
                    rectWeek.Offset(-this.DataRoot.m_nMonthNameWidth, -y);

                    week.Select(rectWeek,
                        action,
                        types,
                        ref update_objects,
                        nMaxCount);

                    y += week.Height;
                }
            }

        }

        #region MonthArea重载AreaBase的virtual函数

        public override string FullName
        {
            get
            {
                return this.Year.ToString().PadLeft(4, '0') + "/" + this.MonthName;
            }
        }

        /*
        public override AreaBase[] Children
        {
            get
            {
                AreaBase[] children = new AreaBase[this.WeekCollection.Count];
                for (int i = 0; i < children.Length; i++)
                {
                    children[i] = (AreaBase)this.WeekCollection[i];
                }
                return children;
            }
        }*/

        public override long Height
        {
            get
            {
                if (m_lHeightCache == -1)
                {
                    long lHeight = this.DataRoot.m_nDayOfWeekTitleHeight; // 标题高度

                    for (int i = 0; i < this.WeekCollection.Count; i++)
                    {
                        lHeight += this.WeekCollection[i].Height;
                    }
                    m_lHeightCache = lHeight;
                }

                return m_lHeightCache;
            }
        }

        public override long Width
        {
            get
            {
                if (m_lWidthCache == -1)
                {
                    m_lWidthCache = this.DataRoot.m_nMonthNameWidth + 7 * this.DataRoot.m_nDayCellWidth;
                }

                return m_lWidthCache;
            }
        }

        /*
        public override bool Select(SelectAction action,
bool bRecursive)
        {
            bool bRet = base.Select(action, bRecursive);

            // 递归
            if (bRecursive == true)
            {
                for (int i = 0; i < this.WeekCollection.Count; i++)
                {
                    WeekArea week = this.WeekCollection[i];
                    week.Select(action, true);
                }
            }

            return bRet;
        }
         * */

        // 获得子对象在 本对象坐标体系中的 左上角位置
        public override PointF GetChildLeftTopPoint(AreaBase child)
        {
            if (!(child is WeekArea))
                throw new Exception("child只能为WeekArea类型");

            WeekArea week = (WeekArea)child;
            int index = this.ChildrenCollection.IndexOf(week);

            if (index == -1)
                throw new Exception("child在子对象中没有找到");

            return new PointF(this.DataRoot.m_nMonthNameWidth,
                this.DataRoot.m_nDayOfWeekTitleHeight + index * week.Height);
        }

        #endregion

        public DataRoot DataRoot
        {
            get
            {
                return this.Container.Container;
            }
        }

        // 点击检测
        public void HitTest(
            long p_x,
            long p_y,
            Type dest_type,
            out HitTestResult result)
        {
            result = null;

            if (dest_type == typeof(MonthArea))
            {
                result = new HitTestResult();
                result.Object = this;
                result.AreaPortion = AreaPortion.Content;
                result.X = p_x;
                result.Y = p_y;
                return;
            }

            // 看看是不是在左边的竖道(月名)上
            // dest_type如果有要求，则把左边竖道也让给下级判断
            if (dest_type == null
                && p_x < this.DataRoot.m_nMonthNameWidth)
            {
                result = new HitTestResult();
                result.AreaPortion = AreaPortion.LeftBar;
                result.Object = this;
                result.X = p_x;
                result.Y = p_y;
                return;
            }

            // 看看是不是在星期标题上
            // dest_type如果有要求，则把标题也让给下级判断
            if (dest_type == null
                && p_y < this.DataRoot.m_nDayOfWeekTitleHeight)
            {
                result = new HitTestResult();
                result.AreaPortion = AreaPortion.ColumnTitle;
                result.Object = this;
                result.X = p_x; // 注意x坐标还包含了m_nMonthNameWidth部分
                result.Y = p_y;
                return;
            }

            // 如果有特殊要求，上部和周标题也算作第一个星期的
            if (p_y < this.DataRoot.m_nDayOfWeekTitleHeight
                && dest_type != null)
            {
                if (this.WeekCollection.Count > 1)
                {
                    this.WeekCollection[0].HitTest(p_x - this.DataRoot.m_nMonthNameWidth,
                        p_y - this.DataRoot.m_nDayOfWeekTitleHeight,
                        dest_type,
                        out result);
                    return;
                }
            }

            // 看看是不是在星期行上
            long y = this.DataRoot.m_nDayOfWeekTitleHeight;
            for (int i = 0; i < this.WeekCollection.Count; i++)
            {
                // 优化
                if (dest_type == null
                    && y > p_y)
                    break;

                WeekArea week = this.WeekCollection[i];

                long lWeekHeight = week.Height;

                if (p_y >= y && p_y < y + lWeekHeight)
                {
                    // 确定在一个WeekArea对象中
                    week.HitTest(p_x - this.DataRoot.m_nMonthNameWidth,
                        p_y - y,
                        dest_type,
                        out result);
                    return;
                }
                y += lWeekHeight;
            }

            // 如果有特殊要求，下部也算作最后一个星期的
            if (dest_type != null)
            {
                if (this.WeekCollection.Count > 1)
                {
                    this.WeekCollection[this.WeekCollection.Count - 1].HitTest(p_x - this.DataRoot.m_nMonthNameWidth,
                        p_y - y,
                        dest_type,
                        out result);
                    return;
                }
            }

            // 如果没有匹配上任何WeekArea对象
            result = new HitTestResult();
            result.Object = this;
            result.AreaPortion = AreaPortion.BottomBlank;
            result.X = p_x;
            result.Y = p_y;
        }

        // 绘制周标题
        void PaintDayOfWeekTitle(long x0,
            long y0,
            PaintEventArgs e)
        {
            int nTitleHeight = this.DataRoot.m_nDayOfWeekTitleHeight;
            int nTitleCellWidth = this.DataRoot.m_nDayCellWidth;

            // 绘制星期标题
            Font font = this.DataRoot.DaysOfWeekTitleFont;

            using(Pen pen = new Pen(Color.FromArgb(50, Color.Gray)))
            using(Pen penBold = new Pen(Color.FromArgb(50, Color.Gray), (float)2))

            using (Brush brushText = new SolidBrush(Color.Gray))
            {
                long x = x0;
                long y = y0;

                // 背景
                {
                    float upper_height = ((float)nTitleHeight / 2) + 1;
                    float lower_height = ((float)nTitleHeight / 2);

                    using (LinearGradientBrush linGrBrush = new LinearGradientBrush(
    new PointF(0, y),
    new PointF(0, y + upper_height),
    Color.FromArgb(200, 251, 252, 253),
    Color.FromArgb(255, 235, 238, 242)
    ))
                    {

                        linGrBrush.GammaCorrection = true;

                        RectangleF rectBack = new RectangleF(
            x,
            y,
            nTitleCellWidth * 7,
            upper_height);
                        e.Graphics.FillRectangle(linGrBrush, rectBack);
                    }
                    //

                    using (LinearGradientBrush linGrBrush = new LinearGradientBrush(
    new PointF(0, y + upper_height),
    new PointF(0, y + upper_height + lower_height),
    Color.FromArgb(255, 220, 226, 231),
    Color.FromArgb(255, 215, 222, 228)
    ))
                    {
                        RectangleF rectBack = new RectangleF(
            x,
            y + upper_height,
            nTitleCellWidth * 7,
            lower_height - 1);
                        e.Graphics.FillRectangle(linGrBrush, rectBack);
                    }
                }

                using (Pen penVert = new Pen(Color.FromArgb(200, Color.White), (float)1.5))
                {
                    for (int i = 0; i < 7; i++)
                    {
                        RectangleF rectUpdate = new RectangleF(
                            x,
                            y,
                            nTitleCellWidth,
                            nTitleHeight);

                        // 优化
                        if (rectUpdate.IntersectsWith(e.ClipRectangle) == false)
                            goto CONTINUE;

                        // 左方竖线
                        e.Graphics.DrawLine(penVert,
                            new PointF(x, y + 1),
                            new PointF(x, y + 1 + nTitleHeight)
                            );

                        // 上方横线
                        e.Graphics.DrawLine(penBold,
                            new PointF(x, y),
                            new PointF(x + nTitleCellWidth, y)
                            );

                        // 文字
                        RectangleF rect = new RectangleF(
                            x,
                            y,
                            nTitleCellWidth,
                            nTitleHeight);

                        StringFormat stringFormat = new StringFormat();

                        stringFormat.Alignment = StringAlignment.Center;
                        stringFormat.LineAlignment = StringAlignment.Center;

                        string strText = "";
                        if (this.DataRoot.m_strDayOfWeekTitleLang == "zh")
                            strText = WeekArea.WeekDayNames_ZH[i];
                        if (this.DataRoot.m_strDayOfWeekTitleLang == "en")
                            strText = WeekArea.WeekDayNames_EN[i];

                        e.Graphics.DrawString(strText,
                            font,
                            brushText,
                            rect,
                            stringFormat);

                    CONTINUE:
                        x += nTitleCellWidth;
                    }
                }
            }
        }

        // 绘图 月
        public void Paint(
            long start_x,
            long start_y,
            PaintEventArgs e)
        {
            if (DataRoot.TooLarge(start_x) == true
    || DataRoot.TooLarge(start_y) == true)
                return;

            // 绘制背景
            PaintBack(
    start_x,
    start_y,
    this.Width, // this.DataRoot.m_nMonthNameWidth,
    this.Height,
    e,
                this.DataRoot.MonthBackColor);

            int x0 = (int)start_x;
            int y0 = (int)start_y;

            RectangleF rectUpdate;

            // 绘制月名字
            rectUpdate = new RectangleF(
x0,
y0,
this.DataRoot.m_nMonthNameWidth,
this.Height);

            // 优化
            if (rectUpdate.IntersectsWith(e.ClipRectangle) == true)
            {
                PaintLeftMonthName(
       x0,
       y0,
       this.DataRoot.m_nMonthNameWidth,
       (int)this.Height,
       e);

                // 选择后的效果
                if (this.m_bSelected == true)
                {
                    // 月名
                    this.PaintSelectEffect(
                        x0,
                        y0,
                        this.DataRoot.m_nMonthNameWidth,
                        (int)this.Height,
                        e);
                }
            }

            int x = x0 + this.DataRoot.m_nMonthNameWidth;
            int y = y0;

            // 绘制星期标题 
            rectUpdate = new RectangleF(
                x,
                y,
                this.DataRoot.m_nDayCellWidth * 7,  // this.Width - this.DataRoot.m_nMonthNameWidth,
                this.DataRoot.m_nDayOfWeekTitleHeight);

            // 优化
            if (rectUpdate.IntersectsWith(e.ClipRectangle) == true)
            {
                PaintDayOfWeekTitle(x,
                y,
                e);

                // 选择后的效果
                if (this.m_bSelected == true)
                {
                    // 星期标题
                    this.PaintSelectEffect(
                    x0 + this.DataRoot.m_nMonthNameWidth,
                    y0,
                    this.DataRoot.m_nDayCellWidth * 7, // (int)(this.Width - this.DataRoot.m_nMonthNameWidth),
                    this.DataRoot.m_nDayOfWeekTitleHeight,
                    e);

                }
            }

            // 绘制背景中月名字
            this.PaintBackMonthName(
               x0 + this.DataRoot.m_nMonthNameWidth,
               y0 + this.DataRoot.m_nDayOfWeekTitleHeight,
               this.Width - this.DataRoot.m_nMonthNameWidth,
               this.Height - this.DataRoot.m_nDayOfWeekTitleHeight,
               e);

            // 绘制下级内容

            x = x0 + this.DataRoot.m_nMonthNameWidth;
            y += this.DataRoot.m_nDayOfWeekTitleHeight;


            // 绘制每个星期
            for (int i = 0; i < this.WeekCollection.Count; i++)
            {
                WeekArea week = this.WeekCollection[i];

                rectUpdate = new RectangleF(
    x,
    y,
    week.Width,
    week.Height);

                // 提前结束循环
                if (rectUpdate.Y > e.ClipRectangle.Bottom)
                    break;

                // 优化
                if (rectUpdate.IntersectsWith(e.ClipRectangle) == true) 
                    week.Paint(x, y, e);

                long lWeekHeight = week.Height;
                y += (int)lWeekHeight;
            }
        }

        // 绘左边竖道上的月名字
        void PaintLeftMonthName(
            int x0,
            int y0,
            int nWidth,
            int nHeight,
            PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.FromArgb(50, Color.Gray)))
            {

                // 左方竖线
                e.Graphics.DrawLine(pen,
                    new PointF(x0, y0),
                    new PointF(x0, y0 + nHeight)
                    );
            }

            using (Pen penBold = new Pen(Color.FromArgb(50, Color.Gray), (float)2))
            {
                // 上方横线
                e.Graphics.DrawLine(penBold,
                    new PointF(x0, y0),
                    new PointF(x0 + nWidth, y0)
                    );
            }

            // 绘制小的年名字

            int x = x0;
            int y = y0;

            RectangleF rect;

            {
                using(Font font = new Font("Arial Black",
                    this.DataRoot.m_nMonthNameWidth/4,
                    FontStyle.Regular, 
                    GraphicsUnit.Pixel))
                using (Brush brushText = new SolidBrush(Color.Blue))
                {

                    rect = new RectangleF(
        x,
        y,
        nWidth,
        this.DataRoot.m_nDayOfWeekTitleHeight);

                    StringFormat stringFormat = new StringFormat();

                    stringFormat.Alignment = StringAlignment.Center;
                    stringFormat.LineAlignment = StringAlignment.Far;

                    e.Graphics.DrawString(this.Container.YearName,
                        font,
                        brushText,
                        rect,
                        stringFormat);
                }
            }


            // 绘制月名字
            {
                using(Font font = new Font("Arial", 20, FontStyle.Bold))
                using (Brush brushText = new SolidBrush(Color.Green))
                {

                    rect = new RectangleF(
                        x0,
                        y0 + this.DataRoot.m_nDayOfWeekTitleHeight,
                        nWidth,
                        nHeight - this.DataRoot.m_nDayOfWeekTitleHeight);

                    StringFormat stringFormat = new StringFormat();

                    stringFormat.Alignment = StringAlignment.Center;
                    stringFormat.LineAlignment = StringAlignment.Center;

                    e.Graphics.DrawString(this.MonthName,
                        font,
                        brushText,
                        rect,
                        stringFormat);
                }
            }

        }

        // 绘背景上的淡色月名字(和年名字)
        void PaintBackMonthName(
            long x0,
            long y0,
            long lWidth,
            long lHeight,
            PaintEventArgs e)
        {
            RectangleF rectUpdate = new RectangleF(
                x0,
                y0,
                lWidth,
                lHeight);

            // 优化
            if (rectUpdate.IntersectsWith(e.ClipRectangle) == false)
                return;

            // 绘制年名字
            RectangleF rect;
            long lHalfHeight = lHeight / 2;
            long lRegionHeight = Math.Min(lHalfHeight, this.DataRoot.m_nDayCellHeight * 2);
            //Font font = null;
            //Brush brushText = null;


                long lYearNameHeight = lRegionHeight/2;
            long lYDelta = lHalfHeight - lYearNameHeight;// 年名字上方预留的空白
            {
                using(Font font = new Font("Arial Black", lYearNameHeight, FontStyle.Regular, GraphicsUnit.Pixel))
                using (Brush brushText = new SolidBrush(Color.FromArgb(80, Color.LightGray)))
                {
                    rect = new RectangleF(
        x0,
        y0 + lYDelta - (lYDelta / 2),
        lWidth,
        lYearNameHeight);

                    StringFormat stringFormat = new StringFormat();

                    stringFormat.Alignment = StringAlignment.Center;
                    stringFormat.LineAlignment = StringAlignment.Center;

                    e.Graphics.DrawString(this.Container.YearName,
                        font,
                        brushText,
                        rect,
                        stringFormat);
                }
            }

            // 绘制月名字
            {
                using(Font font = new Font("Arial Black", lRegionHeight, FontStyle.Regular, GraphicsUnit.Pixel))
                using (Brush brushText = new SolidBrush(Color.FromArgb(100, Color.LightGray)))
                {
                    rect = new RectangleF(
        x0,
        y0 + lHalfHeight - (lYDelta / 2),
        lWidth,
        Math.Min(lRegionHeight, lHalfHeight));

                    StringFormat stringFormat = new StringFormat();

                    stringFormat.Alignment = StringAlignment.Center;
                    stringFormat.LineAlignment = StringAlignment.Center;

                    e.Graphics.DrawString(this.MonthName,
                        font,
                        brushText,
                        rect,
                        stringFormat);
                }
            }
        }

        // 从1开始计数
        public int Month
        {
            get
            {
                return this.NameValue;
            }
            set
            {
                this.NameValue = value;
            }
        }

        public string MonthName
        {
            get
            {
                return this.Month.ToString();
            }
        }

        public int Year
        {
            get
            {
                return this.Container.Year;
            }
        }



        // 本月共有多少天
        public int Days
        {
            get
            {
                if (this.Month == 0)
                    throw new Exception("要使用Days属性, Month属性必须先被初始化");

                return DaysInOneMonth(Container.Year, this.Month);
            }
        }


        static int[] month_array = new int[] {
31, // 1
28, // 2
31, // 3
30, // 4
31, // 5
30, // 6
31, // 7
31, // 8
30, // 9
31, // 10
30, // 11
31, // 12

};

        // parameters:
        //      month   从1开始计数
        public static int DaysInOneMonth(int year, int month)
        {
            if (month < 1 || month > 12)
                return -1;  // 出错

            if (month != 2)
                return month_array[month - 1];

            // 闰年计算
            if ((year % 100) == 0)
            {
                if ((year % 400) == 0)
                    return 29;
            }
            else
            {
                if ((year % 4) == 0)
                    return 29;
            }
            return 28;
        }

        /*
        public WeekArea FirstWeekArea
        {
            get
            {
                return(WeekArea) this.FirstChild;
            }
        }*/

    }

    // 星期
    public class WeekArea : NamedArea<DayArea>
    {
        // public List<DayArea> DayCollection = new List<DayArea>();

        // int m_nWeek = 0;    // 当前对象是本月第几个星期，从1开始计数 0表示尚未初始化

        int m_nMinDay = -1; // 本星期内的最小日值
        int m_nMaxDay = -1; // 本星期内的最大日值

        public static string[] WeekDayNames_ZH = new string[]
        {
            "星期日",
            "星期一",
            "星期二",
            "星期三",
            "星期四",
            "星期五",
            "星期六",
        };

        public static string[] WeekDayNames_EN = new string[]
        {
            "SUN",
            "MON",
            "TUE",
            "WED",
            "THU",
            "FRI",
            "SAT",
        };

        public MonthArea Container
        {
            get
            {
                return (MonthArea)this._Container;
            }
        }



        // 构造函数
        // 只提供周编号，函数会自行推算日期
        // parameters:
        public WeekArea(MonthArea container,
            int nWeek)
        {
            this._Container = container;

            this.NameValue = nWeek;

            // 起始日 1号
            DateTime date = new DateTime(this.Container.Container.Year,
                this.Container.Month,
                1);

            int nStartIndex = Convert.ToInt32(date.DayOfWeek);   // 起始日的序号。0为星期天
            int nMaxDays = this.Container.Days;
            int nDay = 1;
            bool bBlank = true;
            for (int nCurWeek = 1; nCurWeek <= nWeek && nDay <= nMaxDays; nCurWeek++)
            {
                for (int nDayOfWeek = 0; nDayOfWeek < 7; nDayOfWeek++)
                {
                    // 变为不是空白
                    if (nCurWeek == 1 && nDayOfWeek >= nStartIndex)
                        bBlank = false;

                    // 变回空白
                    if (nDay > nMaxDays)
                        bBlank = true;

                    // 只有到指定编号的星期，才开始创建
                    if (nCurWeek == nWeek)
                    {
                        DayArea day = null;
                        // 创建日格子
                        if (bBlank == true)
                            day = new DayArea(this, 0, nCurWeek);
                        else
                        {
                            day = new DayArea(this, nDay, nCurWeek);
                            if (this.m_nMinDay == -1)
                                this.m_nMinDay = nDay;

                            this.m_nMaxDay = nDay; // 不断被刷新
                        }

                        this.ChildrenCollection.Add(day);
                    }

                    if (bBlank == false)
                        nDay++;
                }
            }

        }

        // 构造函数
        // 需要提供起始日期和星期编号
        // parameters:
        //      nStartDay   起始日期
        public WeekArea(MonthArea container,
            int nStartDay,
            int nWeek)
        {
            this._Container = container;

            this.NameValue = nWeek;
            // this.m_nWeek = nWeek;

            // 观察起始日
            DateTime date = new DateTime(this.Container.Container.Year,
                this.Container.Month,
                nStartDay);

            int nStartIndex = Convert.ToInt32(date.DayOfWeek);   // 起始日的序号。0为星期天
            int nMaxDays = this.Container.Days;
            for (int i = 0; i < 7; i++)
            {
                DayArea day = null;
                if (i < nStartIndex || nStartDay > nMaxDays)
                {
                    day = new DayArea(this, 0, i);
                }
                else
                {
                    if (i == nStartIndex)
                        this.m_nMinDay = nStartDay;

                    this.m_nMaxDay = nStartDay; // 不断被刷新

                    day = new DayArea(this, nStartDay++, i);
                }

                this.ChildrenCollection.Add(day);
            }
        }

        // 别名
        public TypedList<DayArea> DayCollection
        {
            get
            {
                return this.ChildTypedCollection;    // 其实NamedCollection也很好用，就是名字没有特色
            }
        }

        // 选择位于矩形内的对象
        public void Select(RectangleF rect,
            SelectAction action,
            List<Type> types,
            ref List<AreaBase> update_objects,
            int nMaxCount)
        {
            RectangleF rectThis = new RectangleF(0, 0, this.Width, this.Height);

            if (rectThis.IntersectsWith(rect) == true)
            {
                /*
                if (types.IndexOf(this.GetType()) != -1)
                {
                    bool bRet = this.Select(action, false);
                    if (bRet == true && update_objects.Count < nMaxCount)
                    {
                        update_objects.Add(this);
                    }
                }*/

                long x = 0;
                for (int i = 0; i < this.DayCollection.Count; i++)
                {
                    DayArea day = this.DayCollection[i];

                    // 优化
                    if (x > rect.Right)
                        break;

                    // 变换为day内坐标
                    RectangleF rectDay = rect;
                    rectDay.Offset(-x, 0);

                    day.Select(rectDay,
                        action,
                        types,
                        ref update_objects,
                        nMaxCount);

                    x += day.Width;
                }
            }

        }

        #region WeekArea重载AreaBase的virtual函数

        public override string FullName
        {
            get
            {
                return this.Year.ToString().PadLeft(4, '0') + "/" + this.Month.ToString() + " 第 " + this.Week.ToString() + " 周";
            }
        }

        /*
        public override AreaBase[] Children
        {
            get
            {
                AreaBase[] children = new AreaBase[this.DayCollection.Count];
                for (int i = 0; i < children.Length; i++)
                {
                    children[i] = (AreaBase)this.DayCollection[i];
                }
                return children;
            }
        }*/

        public override long Height
        {
            get
            {
                // 不需要缓存
                return this.DataRoot.m_nDayCellHeight;
                /*
                if (m_lHeightCache == -1)
                    m_lHeightCache = this.DataRoot.m_nDayCellHeight;

                return m_lHeightCache;
                 */
            }
        }

        public override long Width
        {
            get
            {
                // 不需要缓存
                return this.DataRoot.m_nDayCellWidth * 7;
                /*
                if (m_lWidthCache == -1)
                    m_lWidthCache = this.DataRoot.m_nDayCellWidth * 7;

                return m_lWidthCache;
                 * */
            }
        }

        /*
        public override bool Select(SelectAction action,
bool bRecursive)
        {
            bool bRet = base.Select(action, bRecursive);

            // 递归
            if (bRecursive == true)
            {
                for (int i = 0; i < this.DayCollection.Count; i++)
                {
                    DayArea day = this.DayCollection[i];
                    day.Select(action, true);
                }
            }

            return bRet;
        }*/

        // 获得子对象在 本对象坐标体系中的 左上角位置
        public override PointF GetChildLeftTopPoint(AreaBase child)
        {
            if (!(child is DayArea))
                throw new Exception("child只能为DayArea类型");

            DayArea day = (DayArea)child;
            int index = this.ChildrenCollection.IndexOf(day);

            if (index == -1)
                throw new Exception("child在子对象中没有找到");

            return new PointF(index * day.Width, 0);
        }

        #endregion

        DataRoot m_cacheDataRoot = null;

        public DataRoot DataRoot
        {
            get
            {
                // 缓存
                if (m_cacheDataRoot == null)
                    m_cacheDataRoot = this.Container.Container.Container;

                return m_cacheDataRoot;
            }
        }

        // 点击检测
        public void HitTest(
            long p_x,
            long p_y,
            Type dest_type,
            out HitTestResult result)
        {
            result = null;

            if (dest_type == typeof(WeekArea))
            {
                result = new HitTestResult();
                result.Object = this;
                result.AreaPortion = AreaPortion.Content;
                result.X = p_x;
                result.Y = p_y;
                return;
            }

            // 如果有特殊要求，左部也算作第一日的
            if (p_x < 0 && dest_type != null)
            {
                if (this.DayCollection.Count > 1)
                {
                    this.DayCollection[0].HitTest(p_x,
                        p_y,
                        dest_type,
                        out result);
                    return;
                }
            }

            long x = 0;
            int nDayWidth = -1; // -1 表示尚未初始化
            for (int i = 0; i < this.DayCollection.Count; i++)
            {
                // 优化
                if (dest_type == null
                    && x > p_x)
                    break;

                DayArea day = this.DayCollection[i];

                // 提高速度
                if (nDayWidth == -1)
                    nDayWidth = (int)day.Width;

                if (p_x >= x && p_x < x + nDayWidth)
                {
                    // 确定在一个DayArea对象中
                    day.HitTest(p_x - x,
                        p_y,
                        dest_type,
                        out result);
                    return;
                }
                x += nDayWidth;
            }

            // 如果有特殊要求，右部也算作最后一日的
            if (dest_type != null)
            {
                if (this.DayCollection.Count > 1)
                {
                    this.DayCollection[this.DayCollection.Count - 1].HitTest(p_x - x,
                        p_y,
                        dest_type,
                        out result);
                    return;
                }
            }

            // 没有匹配上任何DayArea对象
            result = new HitTestResult();
            result.Object = this;
            result.AreaPortion = AreaPortion.RightBlank;
            result.X = p_x;
            result.Y = p_y;
        }


        // 绘图 星期
        public void Paint(
            long start_x,
            long start_y,
            PaintEventArgs e)
        {
            if (DataRoot.TooLarge(start_x) == true
                || DataRoot.TooLarge(start_y) == true)
                return;

            /*
            PaintBack(
start_x,
start_y,
0,
0,
e,
                Color.White);
             * */

            int x0 = (int)start_x;
            int y0 = (int)start_y;


            int x = x0;
            int y = y0;
            for (int i = 0; i < this.DayCollection.Count; i++)
            {
                DayArea day = this.DayCollection[i];

                RectangleF rectUpdate = new RectangleF(
    x,
    y,
    day.Width,
    day.Height);

                // 提前退出循环
                if (x > rectUpdate.Right)
                    break;

                // 优化
                if (rectUpdate.IntersectsWith(e.ClipRectangle) == true)
                    day.Paint(x, y, e);


                x += (int)day.Width;
            }
        }


        // 本星期内的最小日值。从1开始计数
        public int MinDay
        {
            get
            {
                return m_nMinDay;
            }
        }

        // 本星期内的最大日值。从1开始计数
        public int MaxDay
        {
            get
            {
                return m_nMaxDay;
            }
        }

        public int Week
        {
            get
            {
                return this.NameValue;
                // return m_nWeek;
            }
        }

        public int Month
        {
            get
            {
                return this.Container.Month;
            }
        }

        public int Year
        {
            get
            {
                return this.Container.Container.Year;
            }
        }


        /*

        // 下一个星期。注意，可以跨越本月
        public WeekArea NextWeekArea
        {
            get
            {
                if (this.Week == 0)
                    throw new Exception("WeekArea对象的Week属性尚未初始化");

                if (this.Week < this.Container.WeekCollection.Length)
                    return this.Container.WeekCollection[this.Week];

                MonthArea next_month = this.Container.NextMonthArea;
                if (next_month == null)
                    return null;

                return next_month.FirstWeekArea;
            }
        }
         * */

        /*
        // 返回下一个星期的第一个非空白日
        public DayArea NextWeekFirstDay()
        {
            WeekArea next_week = null;

            // 如果本月中有下一星期
            if (this.Week < this.Container.WeekCollection.Count)
            {
                next_week = this.Container.WeekCollection[this.Week];
                return next_week.FirstNonBlankDay;
            }

            return this.Container.NextMonthFirstDay();
        }*/

        public DayArea FistNonBlankDayArea
        {
            get
            {
                for (int i = 0; i < this.ChildrenCollection.Count; i++)
                {
                    DayArea day = (DayArea)this.ChildrenCollection[i];
                    if (day.Blank == false)
                        return day;
                }

                return null;    // 没有找到
            }
        }
    }


    // 日
    public class DayArea : AreaBase
    {
        // bool m_bChecked = false;
        public bool m_bHover = false;

        // int m_nDay = 0; // 日，从1开始计数。如果为0，表示该格子未使用

        int m_nDayOfWeek = -1;  // -1表示尚未初始化

        // DayState m_daystate = DayState.WorkDay;
        int m_nDayState = -1;   // -1表示尚未初始化

        /*
        int m_nCacheHeight = -1;
        int m_nCacheWidth = -1;
         */
        DataRoot m_cacheDataRoot = null;    // 提高访问DataRoot的速度

        public WeekArea Container
        {
            get
            {
                return (WeekArea)this._Container;
            }
        }

        // 选择位于矩形内的对象
        public void Select(RectangleF rect,
            SelectAction action,
            List<Type> types,
            ref List<AreaBase> update_objects,
            int nMaxCount)
        {
            RectangleF rectThis = new RectangleF(0, 0, this.Width, this.Height);

            if (rectThis.IntersectsWith(rect) == true)
            {
                if (types.IndexOf(this.GetType()) != -1)
                {
                    bool bRet = this.Select(action, false);
                    if (bRet == true && update_objects.Count < nMaxCount)
                    {
                        update_objects.Add(this);
                    }
                }
            }
        }

        #region DayArea重载AreaBase的virtual函数

        public override string FullName
        {
            get
            {
                return this.Year.ToString().PadLeft(4, '0') + "/" + this.Month.ToString() + "/" + this.DayName;
            }
        }

        public override long Height
        {
            get
            {
                // 没有缓存
                return this.DataRoot.m_nDayCellHeight;
            }
        }

        public override long Width
        {
            get
            {
                // 没有缓存
                return this.DataRoot.m_nDayCellWidth;
            }
        }

        // Select()不需要重载

        #endregion

        public DataRoot DataRoot
        {
            get
            {
                if (m_cacheDataRoot == null)
                    m_cacheDataRoot = this.Container.Container.Container.Container;

                return m_cacheDataRoot;
            }
        }



        // 点击检测
        public void HitTest(
            long p_x,
            long p_y,
            Type dest_type,
            out HitTestResult result)
        {
            /*
            if (dest_type == typeof(DayArea))
            {
                result = new HitTestResult();
                result.Object = this;
                result.AreaPortion = AreaPortion.Content;
                result.X = p_x;
                result.Y = p_y;
                return;
            }*/

            result = new HitTestResult();
            result.Object = this;

            // 观察点击到了哪个部位
            Rectangle rectCheckBox = this.DataRoot.m_rectCheckBox;
            if (p_x >= rectCheckBox.X
                && p_x <= rectCheckBox.X + rectCheckBox.Width
                && p_y >= rectCheckBox.Y
                && p_y <= rectCheckBox.Y + rectCheckBox.Height)
                result.AreaPortion = AreaPortion.CheckBox;
            else
                result.AreaPortion = AreaPortion.Content;
            result.X = p_x;
            result.Y = p_y;
        }

        // 绘图 日格子
        public void Paint(
            long start_x,
            long start_y,
            PaintEventArgs e)
        {
            if (DataRoot.TooLarge(start_x) == true
                || DataRoot.TooLarge(start_y) == true)
                return;

            DayStateDef def = this.DataRoot.DayStateDefs.GetDef(this.State);
            Color colorText = Color.Black;
            Color colorBack = Color.White;
            if (def != null)
            {
                colorText = def.TextColor;
                colorBack = def.BackColor;
            }

            if (colorBack != Color.White)
            {
                colorBack = Color.FromArgb(100, colorBack);

                // 绘制背景
                PaintBack(
                    start_x,
                    start_y,
                    this.Width,
                    this.Height,
                    e,
                    colorBack);
            }

            int x0 = (int)start_x;
            int y0 = (int)start_y;


            using (Pen pen = new Pen(Color.FromArgb(50, Color.Gray)))
            {

                // 左方竖线
                e.Graphics.DrawLine(pen,
                    new PointF(x0, y0),
                    new PointF(x0, y0 + this.Height)
                    );

                // 上方横线
                e.Graphics.DrawLine(pen,
                    new PointF(x0, y0),
                    new PointF(x0 + this.Width, y0)
                    );
            }

            RectangleF rect = new RectangleF(
x0,
y0,
this.Width,
this.Height);

            if (this.Blank == false )
            {

                // 绘制状态图标
                if (def != null 
                    && (this.DataRoot.HoverCheckBox == false || this.m_bHover == true))
                {
                    Image image = def.Icon;
                    if (image != null)
                    {
                        e.Graphics.DrawImage(image,
                            (float)x0 + this.DataRoot.m_rectCheckBox.X,
                            (float)y0 + this.DataRoot.m_rectCheckBox.Y);
                    }
                }

                Font new_font = null;
                try
                {
                    // 绘制文字
                    Font font = this.DataRoot.DayTextFont;
                    if (this.m_bFocus)
                    {
                        font = new Font(font.FontFamily.GetName(0),
                            font.Size + 3,
                            font.Style,
                            font.Unit);
                        new_font = font;
                    }

                    using (Brush brushText = new SolidBrush(colorText))
                    {
                        StringFormat stringFormat = new StringFormat();

                        stringFormat.Alignment = StringAlignment.Center;
                        stringFormat.LineAlignment = StringAlignment.Center;

                        e.Graphics.DrawString(this.DayName,
                            font,
                            brushText,
                            rect,
                            stringFormat);
                    }
                }
                finally
                {
                    if (new_font != null)
                        new_font.Dispose();
                }
            }

            // 选择后的效果
            if (this.m_bSelected == true)
            {
                this.PaintSelectEffect(
                    start_x,
                    start_y,
                    this.Width,
                    this.Height,
                    e);
            }

            // 焦点虚线
            if (this.m_bFocus == true)
            {
                rect.Inflate(-4, -4);
                ControlPaint.DrawFocusRectangle(e.Graphics,
                    Rectangle.Round(rect));
            }
        }

        public int State
        {
            get
            {
                return this.m_nDayState;
            }
            set
            {
                // 空白格子不能修改状态
                if (this.Blank == false)
                    this.m_nDayState = value;
            }
        }

        /*
        public bool Checked
        {
            get
            {
                return m_bChecked;
            }
            set
            {
                m_bChecked = value;
            }
        }*/

        public int Day
        {
            get
            {
                return this.NameValue;
                // return this.m_nDay;
            }
        }

        public string DayName
        {
            get
            {
                if (this.Day == 0)
                    return null;    // 表示当前格子没有使用

                return this.Day.ToString();
            }
        }

        public int Month
        {
            get
            {
                return this.Container.Container.Month;
            }
        }

        // 第几个星期
        public int Week
        {
            get
            {
                return this.Container.Week;
            }
        }

        public int Year
        {
            get
            {
                return this.Container.Container.Container.Year;
            }
        }

        // 在一个星期中的哪一天 0表示星期天
        public int DayOfWeek
        {
            get
            {
                return this.m_nDayOfWeek;
            }
        }

 
        public string DayOfWeekName(string strLang)
        {
            if (strLang == "zh")
                return WeekArea.WeekDayNames_ZH[this.m_nDayOfWeek];
            if (strLang == "en")
                return WeekArea.WeekDayNames_EN[this.m_nDayOfWeek];
            throw new Exception("不支持的语言代码 '" + strLang + "'");
        }

        // 是否为空白格子
        public bool Blank
        {
            get
            {
                if (this.NameValue == 0)
                    return true;

                return false;
            }
        }

        // 构造函数
        // parameters:
        //      nDay    日，从1开始计数。如果为0，表示该格子未使用
        public DayArea(WeekArea container, int nDay, int nDayOfWeek)
        {
            this._Container = container;

            this.NameValue = nDay;
            // this.m_nDay = nDay;

            if (nDay != 0)
                this.m_nDayState = 0;   // 初始化为缺省状态


            this.m_nDayOfWeek = nDayOfWeek;
        }

        // 返回非空的下一日
        public DayArea NextNoneBlankDayArea
        {
            // 改写
            get
            {
                DayArea day = this;
                for (; ; )
                {
                    day = (DayArea)day.GetNextSibling();
                    if (day == null)
                        return null;
                    if (day.Blank == false)
                        return day;
                }
            }
        }

        // 返回非空的前一日
        public DayArea PrevNoneBlankDayArea
        {
            get
            {
                DayArea day = this;
                for (; ; )
                {
                    day = (DayArea)day.GetPrevSibling();
                    if (day == null)
                        return null;
                    if (day.Blank == false)
                        return day;
                }
            }
        }


        // 切换日状态
        // return:
        //      状态是否发生了改变
        public bool ToggleState()
        {
            if (this.Blank == true)
                return false;

            DayStateDefCollection defs = this.DataRoot.DayStateDefs;
            if (defs != null)
            {
                if (defs.Count == 1)
                    return false;   // 只有一个状态，无法切换

                if (this.m_nDayState >= defs.Count - 1)
                {
                    this.m_nDayState = 0;
                }
                else
                    this.m_nDayState++;
                return true;
            }

            return false;
        }

        public string Name8
        {
            get
            {
                return this.Year.ToString().PadLeft(4, '0')
                + this.Month.ToString().PadLeft(2, '0')
                + this.Day.ToString().PadLeft(2, '0');
            }
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
    }

    // 点击检测结果
    public class HitTestResult
    {
        public AreaBase Object = null;    // 点击到的末级对象
        public AreaPortion AreaPortion = AreaPortion.None;

        // 对象坐标下的点击位置
        public long X = -1;
        public long Y = -1;

        public int Param = 0;   // 其他参数
    }

    // 选择一个对象的动作
    public enum SelectAction
    {
        Toggle = 0,
        On = 1,
        Off = 2,
    }

    /*
        // 日状态
    public enum DayState
    {
        NoneWorkDay = 0,
        WorkDay = 1,
    }*/

    // 一个状态定义
    public class DayStateDef
    {
        // 名称
        public string Caption = "";

        /*
        // 状态值
        public int State = -1;  // -1表示尚未初始化
         * */

        // 图标
        public Image Icon = null;

        // 文字颜色
        public Color TextColor = Color.Black;
        // 背景颜色
        public Color BackColor = Color.White;
    }

    // 一系列状态定义
    public class DayStateDefCollection : List<DayStateDef>
    {
        public int IconWidth 
        {
            get
            {
                for (int i = 0; i < this.Count; i++)
                {
                    DayStateDef item = this[i];
                    if (item.Icon != null)
                        return item.Icon.Width;
                }

                return 16;
            }
        }
        public int IconHeight
        {
            get
            {
                for (int i = 0; i < this.Count; i++)
                {
                    DayStateDef item = this[i];
                    if (item.Icon != null)
                        return item.Icon.Height;
                }

                return 16;
            }
        }

        public DayStateDef GetDef(int nState)
        {
            if (nState < 0 || nState >= this.Count)
                return null;
            return this[nState];
        }

    }
}
