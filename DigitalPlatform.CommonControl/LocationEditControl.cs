using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

using DigitalPlatform;

/*
 * TODO:
 * 1) 应提供菜单可以增删行，进而触发CountChanged事件和ContentChanged事件
 * 2) 可改为平时绘制控件外貌，当点到某行的时候再真正创建这一行的控件
 * 
 * */

namespace DigitalPlatform.CommonControl
{
    /// <summary>
    /// 馆藏去向 编辑控件
    /// </summary>
    public partial class LocationEditControl : UserControl
    {
        // 以前曾用过的地点名。为了实现去掉后又重新要新增时，回到原来的文字
        List<string> UsedText = new List<string>();

        internal bool m_bFocused = false;

        bool m_bHideSelection = true;

        /// <summary>
        /// 内容发生改变
        /// </summary>
        public event ContentChangedEventHandler ContentChanged = null;

        // 验收已到checkbox状态改变
        [Category("New Event")]
        public event EventHandler ArrivedChanged = null;

        // ReadOnly状态发生改变
        [Category("New Event")]
        public event EventHandler ReadOnlyChanged = null;

        public LocationItem LastClickItem = null;   // 最近一次click选择过的LocationItem对象

        public string DbName = "";  // 数据库名。用于获得值列表

        /// <summary>
        /// 获得值列表
        /// </summary>
        public event GetValueTableEventHandler GetValueTable = null;

        int m_nLineHeight = 26;

        internal int m_nLabelWidth = 40;    // 26
        // internal int m_nLibraryWidth = 100;
        internal int m_nLocationWidth = 160;
        internal int m_nArrivedWidth = 40;

        internal int m_nLineLeftBlank = 6;    // 线条部分左边的空白宽度
        internal int m_nLineWidth = 6;    // 线条横线部分的宽度
        internal int m_nNumberTextWidth = 20;    // 线条横线右边的数字文字的宽度

        internal int m_nRightBlank = 4;    // 30

        public List<LocationItem> LocationItems = new List<LocationItem>();

        bool m_bChanged = false;

        public LocationEditControl()
        {
            InitializeComponent();
        }

        bool m_bArriveMode = false;

        // 是否为验收模式? true表示为验收模式，false表示为订购模式
        [Category("Appearance")]
        [DescriptionAttribute("ArriveMode")]
        [DefaultValue(false)]
        public bool ArriveMode
        {
            get
            {
                return this.m_bArriveMode;
            }
            set
            {
                this.m_bArriveMode = value;

                // SetArriveMode(value);
            }
        }

        // 加工id列表，只取得指定数目以内的id构成新列表
        static string LimitIDs(string strIDs,
            int nCount)
        {
            if (String.IsNullOrEmpty(strIDs) == true)
                return "";

            string[] ids = strIDs.Split(new char[] { ',' });

            if (ids.Length <= nCount)
                return strIDs;

            string strResult = "";
            for (int i = 0; i < nCount; i++)
            {
                if (i != 0)
                    strResult += ",";

                strResult += ids[i];
            }

            return strResult;
        }

        // 规范馆藏地点字符串，限定其包含的事项个数
        // 加工方法是如果有多余的事项就裁减掉，如果缺乏事项就增补上
        public static string CanonicalizeDistributeString(
            string strDistributeString,
            int nAmount)
        {
            string strResult = "";
            int nCurrent = 0;
            string[] sections = strDistributeString.Split(new char[] { ';' });
            for (int i = 0; i < sections.Length; i++)
            {
                string strSection = sections[i].Trim();
                if (String.IsNullOrEmpty(strSection) == true)
                    continue;

                string strIDs = ""; // 已验收id列表

                string strLocationString = "";
                int nCount = 0;
                int nRet = strSection.IndexOf(":");
                if (nRet == -1)
                {
                    strLocationString = strSection;
                    nCount = 1;
                }
                else
                {
                    strLocationString = strSection.Substring(0, nRet).Trim();
                    string strCount = strSection.Substring(nRet + 1);


                    nRet = strCount.IndexOf("{");
                    if (nRet != -1)
                    {
                        strIDs = strCount.Substring(nRet + 1).Trim();

                        if (strIDs.Length > 0 && strIDs[strIDs.Length - 1] == '}')
                            strIDs = strIDs.Substring(0, strIDs.Length - 1);

                        strCount = strCount.Substring(0, nRet).Trim();
                    }

                    try
                    {
                        nCount = Convert.ToInt32(strCount);
                    }
                    catch
                    {
                        throw new Exception("'" + strCount + "' 应为纯数字");
                    }

                    if (nCount > 1000)
                        throw new Exception("数字太大，超过1000");
                }

                if (nCurrent + nCount > nAmount)
                {
                    if (nAmount - nCurrent > 0)
                    {
                        if (strResult != "")
                            strResult += ";";
                        strResult += strLocationString + ":" + (nAmount - nCurrent).ToString();

                        string strPart = LimitIDs(strIDs, nAmount - nCurrent);
                        if (LocationCollection.IsEmptyIDs(strPart) == false)
                            strResult += "{" + strPart + "}";
                    }
                    nCurrent += nAmount - nCurrent;
                    break;
                }

                if (strResult != "")
                    strResult += ";";
                strResult += strLocationString + ":" + nCount.ToString();

                {
                    string strPart = LimitIDs(strIDs, nCount);
                    if (LocationCollection.IsEmptyIDs(strPart) == false)
                        strResult += "{" + strPart + "}";
                }

                nCurrent += nCount;
            }

            // 如果不足则增补
            if (nCurrent < nAmount)
            {
                if (strResult != "")
                    strResult += ";";
                strResult += "" + ":" + (nAmount - nCurrent).ToString();

                // ids不用了
            }

            return strResult;
        }

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

        bool m_bReadOnly = false;

        // 全体Item的ReadOnly状态
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
                    for (int i = 0; i < this.LocationItems.Count; i++)
                    {
                        LocationItem item = this.LocationItems[i];
                        /*
                        if (value == true)
                            item.comboBox_location.Enabled = false;
                        else
                            item.comboBox_location.Enabled = true;
                         * */
                        item.ReadOnly = value;
                    }

                    this.OnReadOnlyChanged();
                }
            }
        }

        public LocationItem InsertNewItem(int index)
        {
            LocationItem item = new LocationItem(this);
            this.LocationItems.Insert(index, item);
            item.State = ItemState.New;

            this.SetSize();
            this.LayoutItems();

            return item;
        }

        // 删除一个事项
        // 2008/9/16
        public void RemoveItem(int index,
            bool bUpdateDisplay)
        {
            LocationItem line = this.LocationItems[index];

            line.RemoveFromContainer();

            this.LocationItems.Remove(line);

            /*
            if (this.LastClickItem == line)
                this.LastClickItem = null;
             * */

            this.Changed = true;

            if (bUpdateDisplay == true)
            {
                this.SetSize();
                this.LayoutItems();
            }

        }

        // 删除一个事项
        // 2008/9/16
        public void RemoveItem(LocationItem line,
            bool bUpdateDisplay)
        {
            int index = this.LocationItems.IndexOf(line);

            if (index == -1)
                return;

            line.RemoveFromContainer();

            this.LocationItems.Remove(line);

            /*
            if (this.LastClickItem == line)
                this.LastClickItem = null;
             * */

            this.Changed = true;

            if (bUpdateDisplay == true)
            {
                this.SetSize();
                this.LayoutItems();
            }
        }

        // 将已经勾选的、具有ref id的事项设置为ReadOnly状态
        // 2008/9/13
        // parameters:
        //      bClearAllReadOnlyBeforeSet  是否在设置前清除已有的readonly状态
        public void SetAlreadyCheckedToReadOnly(bool bClearAllReadOnlyBeforeSet)
        {
            bool bChanged = false;
            for (int i = 0; i < this.LocationItems.Count; i++)
            {
                LocationItem item = this.LocationItems[i];

                if (item.checkBox_arrived.Checked == true
                    && String.IsNullOrEmpty(item.ArrivedID) == false)
                {
                    if (item.ReadOnly != true && item.ArrivedID != "*") // 2008/12/25 changed
                    {
                        item.ReadOnly = true;
                        bChanged = true;
                    }
                }
                else
                {
                    if (bClearAllReadOnlyBeforeSet == true)
                    {
                        if (item.ReadOnly != false)
                        {
                            item.ReadOnly = false;
                            bChanged = true;
                        }
                    }
                }
            }

            if (this.m_bReadOnly != false)
            {
                this.m_bReadOnly = false;   // 不是全部事项都为ReadOnly
                bChanged = true;
            }

            if (bChanged == true)
                this.OnReadOnlyChanged();

            // 会出现这样一种情况：全部item都是readonly了，但是container不是readonly状态。
            // 这种状态意味着还可以新增加item。而整个container为readonly的时候，是不允许向里面加入任何新item的了
        }

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



                if (this.m_bChanged != value)
                {
                    this.m_bChanged = value;

                    if (value == false)
                        ResetLineState();

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
        }

        internal void OnArrivedChanged()
        {
            if (this.ArrivedChanged != null)
            {
                this.ArrivedChanged(this, new EventArgs());
            }
        }

        internal void OnReadOnlyChanged()
        {
            if (this.ReadOnlyChanged != null)
            {
                this.ReadOnlyChanged(this, new EventArgs());
            }
        }

        // 进行检查
        // return:
        //      -1  函数运行出错
        //      0   检查没有发现错误
        //      1   检查发现了错误
        public int Check(out string strError)
        {
            strError = "";

            bool bStrict = true;    // 是否严格检查

            if (bStrict == true)
            {
                for (int i = 0; i < this.LocationItems.Count; i++)
                {
                    LocationItem element = this.LocationItems[i];

                    if (String.IsNullOrEmpty(element.LocationString) == true)
                    {
                        if (this.LocationItems.Count == 1)
                            strError = "尚未指定确切的馆藏地点";   // 只有一行的情况，避免提示行号。这样可以让本控件嵌入 OrderDesignControl 时的提示清爽一些 2014/8/29
                        else
                            strError = "馆藏事项第 " + (i + 1).ToString() + " 行: 尚未指定确切的馆藏地点";
                        return 1;
                    }
                }
            }

            return 0;
        }

        // 把全部事项的状态设置为Normal
        void ResetLineState()
        {
            for (int i = 0; i < this.LocationItems.Count; i++)
            {
                LocationItem element = this.LocationItems[i];
                element.State = ItemState.Normal;
            }
        }

        void RefreshLineColor()
        {
            for (int i = 0; i < this.LocationItems.Count; i++)
            {
                LocationItem element = this.LocationItems[i];
                element.SetLineColor();
            }
        }

        void ClearItems()
        {
            if (this.LocationItems != null)
            {
                List<LocationItem> items = new List<LocationItem>();
                foreach (LocationItem item in this.LocationItems)
                {
#if NO
                    if (item != null)
                        item.Dispose();
#endif
                    if (item != null)
                        items.Add(item);
                }
                this.LocationItems.Clear();

                foreach (LocationItem item in items)
                {
                    item.Dispose();
                }
            }
        }

        public void Clear()
        {
            // this.LocationItems.Clear();
            this.ClearItems();

            List<Control> controls = new List<Control>();
            while (this.panel_main.Controls.Count != 0)
            {
                Control control = this.panel_main.Controls[0];
                this.panel_main.Controls.RemoveAt(0);
                if (control != null)
                    controls.Add(control);
            }

            foreach (Control control in controls)
            {
                control.Dispose();
            }
        }

        public void SelectAll()
        {
            for (int i = 0; i < this.LocationItems.Count; i++)
            {
                LocationItem cur_element = this.LocationItems[i];
                if ((cur_element.State & ItemState.Selected) == 0)
                    cur_element.State |= ItemState.Selected;
            }
        }

        public void SelectItem(LocationItem element,
            bool bClearOld)
        {

            if (bClearOld == true)
            {
                for (int i = 0; i < this.LocationItems.Count; i++)
                {
                    LocationItem cur_element = this.LocationItems[i];

                    if (cur_element == element)
                        continue;   // 暂时不处理当前行

                    if ((cur_element.State & ItemState.Selected) != 0)
                        cur_element.State -= ItemState.Selected;
                }
            }

            // 选中当前行
            if ((element.State & ItemState.Selected) == 0)
                element.State |= ItemState.Selected;

            this.LastClickItem = element;
        }

        public void ToggleSelectItem(LocationItem element)
        {
            // 选中当前行
            if ((element.State & ItemState.Selected) == 0)
                element.State |= ItemState.Selected;
            else
                element.State -= ItemState.Selected;

            this.LastClickItem = element;
        }

        public void RangeSelectItem(LocationItem element)
        {
            LocationItem start = this.LastClickItem;

            int nStart = this.LocationItems.IndexOf(start);
            if (nStart == -1)
                return;

            int nEnd = this.LocationItems.IndexOf(element);

            if (nStart > nEnd)
            {
                // 交换
                int nTemp = nStart;
                nStart = nEnd;
                nEnd = nTemp;
            }

            for (int i = nStart; i <= nEnd; i++)
            {
                LocationItem cur_element = this.LocationItems[i];

                if ((cur_element.State & ItemState.Selected) == 0)
                    cur_element.State |= ItemState.Selected;
            }

            // 清除其余位置
            for (int i = 0; i < nStart; i++)
            {
                LocationItem cur_element = this.LocationItems[i];

                if ((cur_element.State & ItemState.Selected) != 0)
                    cur_element.State -= ItemState.Selected;
            }

            for (int i = nEnd + 1; i < this.LocationItems.Count; i++)
            {
                LocationItem cur_element = this.LocationItems[i];

                if ((cur_element.State & ItemState.Selected) != 0)
                    cur_element.State -= ItemState.Selected;
            }
        }




#if NOOOOOOOOOOOOOO

        // 将馆藏地点字符串转换为对象列表
        static int LocationStringToList(
            string value,
            LocationEditControl container,
            out List<LocationItem> items,
            out string strError)
        {
            strError = "";
            items = new List<LocationItem>();

            string[] sections = value.Split(new char[] { ';' });
            for (int i = 0; i < sections.Length; i++)
            {
                string strSection = sections[i].Trim();
                if (String.IsNullOrEmpty(strSection) == true)
                    continue;

                string strIDs = ""; // 已验收id列表

                string strLocationString = "";
                int nCount = 0;
                int nRet = strSection.IndexOf(":");
                if (nRet == -1)
                {
                    strLocationString = strSection;
                    nCount = 1;
                }
                else
                {
                    strLocationString = strSection.Substring(0, nRet).Trim();
                    string strCount = strSection.Substring(nRet + 1);


                    nRet = strCount.IndexOf("{");
                    if (nRet != -1)
                    {
                        strIDs = strCount.Substring(nRet + 1).Trim();

                        if (strIDs.Length > 0 && strIDs[strIDs.Length - 1] == '}')
                            strIDs = strIDs.Substring(0, strIDs.Length - 1);

                        strCount = strCount.Substring(0, nRet).Trim();
                    }

                    try
                    {
                        nCount = Convert.ToInt32(strCount);
                    }
                    catch
                    {
                        strError = "'" + strCount + "' 应为纯数字";
                        return -1;
                    }

                    if (nCount > 1000)
                    {
                        strError = "数字太大，超过1000";
                        return -1;
                    }

                }

                for (int j = 0; j < nCount; j++)
                {
                    LocationItem item = new LocationItem(container);
                    if (container != null)
                        item.LocationString = strLocationString;
                    items.Add(item);
                }

                if (string.IsNullOrEmpty(strIDs) == false)
                {
                    string[] ids = strIDs.Split(new char[] { ',' });

                    int nStartBase = items.Count - nCount;
                    for (int k = 0; k < nCount; k++)
                    {
                        LocationItem item = items[nStartBase + k];

                        if (k >= ids.Length)
                            break;

                        string strID = ids[k];

                        if (String.IsNullOrEmpty(strID) == true)
                        {
                            // item.Arrived = false;
                            continue;
                        }


                        item.Arrived = true;
                        item.ArrivedID = strID;
                    }
                }
            }

            return 0;
        }

        // 将对象列表转换为馆藏地点字符串
        static string ListToLocationString(List<LocationItem> items,
            bool bOutputID)
        {
            string strResult = "";
            string strPrevLocationString = null;
            int nPartCount = 0;
            string strIDs = "";
            for (int i = 0; i < items.Count; i++)
            {
                LocationItem item = items[i];

                if (item.LocationString == strPrevLocationString)
                {
                    nPartCount++;
                    strIDs += item.ArrivedID + ",";
                }
                else
                {
                    if (strPrevLocationString != null)
                    {
                        if (strResult != "")
                            strResult += ";";
                        strResult += strPrevLocationString + ":" + nPartCount.ToString();

                        if (bOutputID == true)
                        {
                            if (LocationEditControl.IsEmptyIDs(strIDs) == false)
                                strResult += "{" + RemoveTailComma(strIDs) + "}";
                        }

                        nPartCount = 0;
                        strIDs = "";
                    }

                    nPartCount++;
                    strIDs += item.ArrivedID + ",";
                }

                strPrevLocationString = item.LocationString;
            }

            if (nPartCount != 0)
            {
                if (strResult != "")
                    strResult += ";";
                strResult += strPrevLocationString + ":" + nPartCount.ToString();

                if (bOutputID == true)
                {
                    if (LocationEditControl.IsEmptyIDs(strIDs) == false)
                        strResult += "{" + RemoveTailComma(strIDs) + "}";
                }
            }

            return strResult;
        }

#endif

        // 2012/5/18
        internal int m_nDontMerge = 0;

        // 字符串形态表达的内容
        // 分号间隔每个segment。segment的内部结构是: 馆藏地点:份数{已到记录id罗列}
        // '已到记录罗列'的格式为：逗号分隔的字符串。如果某个值为空，其后部的逗号不能省略
        public string Value
        {
            get
            {
                // this.Merge();

                string strResult = "";
                string strPrevLocationString = null;
                int nPartCount = 0;
                string strIDs = "";
                // int nIDCount = 0;
                for (int i = 0; i < this.LocationItems.Count; i++)
                {
                    LocationItem item = this.LocationItems[i];

                    if (item.LocationString == strPrevLocationString)
                    {
                        nPartCount++;
                        strIDs += (item.Arrived == true ? item.ArrivedID : "")
                            + ",";
                        /*
                        if (item.Arrived == true)
                            nIDCount++;
                         * */
                    }
                    else
                    {
                        if (strPrevLocationString != null)
                        {
                            Debug.Assert(nPartCount >= 0, "");

                            if (strResult != "")
                                strResult += ";";
                            strResult += strPrevLocationString + ":" + nPartCount.ToString();

                            if (LocationCollection.IsEmptyIDs(strIDs) == false)
                                strResult += "{" + LocationCollection.RemoveTailComma(strIDs) + "}";

                            nPartCount = 0;
                            strIDs = "";
                            // nIDCount = 0;
                        }

                        nPartCount++;
                        strIDs += (item.Arrived == true ? item.ArrivedID : "")
                            + ",";
                        /*
                        if (item.Arrived == true)
                            nIDCount++;
                         * */
                    }

                    strPrevLocationString = item.LocationString;
                }

                if (nPartCount != 0)
                {
                    Debug.Assert(nPartCount > 0, "");

                    if (strResult != "")
                        strResult += ";";
                    strResult += strPrevLocationString + ":" + nPartCount.ToString();
                    if (LocationCollection.IsEmptyIDs(strIDs) == false)
                        strResult += "{" + LocationCollection.RemoveTailComma(strIDs) + "}";

                }

                return strResult;
            }
            set
            {
                this.Clear();

                string[] sections = value.Split(new char[] { ';' });
                for (int i = 0; i < sections.Length; i++)
                {
                    string strSection = sections[i].Trim();
                    if (String.IsNullOrEmpty(strSection) == true)
                        continue;

                    string strIDs = ""; // 已验收id列表

                    string strLocationString = "";
                    int nCount = 0;
                    int nRet = strSection.IndexOf(":");
                    if (nRet == -1)
                    {
                        strLocationString = strSection;
                        nCount = 1;
                    }
                    else
                    {
                        strLocationString = strSection.Substring(0, nRet).Trim();
                        string strCount = strSection.Substring(nRet + 1);


                        nRet = strCount.IndexOf("{");
                        if (nRet != -1)
                        {
                            strIDs = strCount.Substring(nRet + 1).Trim();

                            if (strIDs.Length > 0 && strIDs[strIDs.Length - 1] == '}')
                                strIDs = strIDs.Substring(0, strIDs.Length - 1);

                            strCount = strCount.Substring(0, nRet).Trim();
                        }

                        try
                        {
                            nCount = Convert.ToInt32(strCount);
                        }
                        catch
                        {
                            throw new Exception(
                                "馆藏地点字符串局部 '" + strSection + "' 中 "
                                + "'" + strCount + "' 应为纯数字");
                        }

                        if (nCount > 1000)
                            throw new Exception(
                                "馆藏地点字符串局部 '" + strSection + "' 中 "
                                + "数字 " + strCount + " 值太大，超过1000");

                        // 2008/12/5
                        if (nCount < 0)
                            throw new Exception(
                                "馆藏地点字符串局部 '" + strSection + "' 中 "
                                + "数字 " + strCount + " 为负数，格式错误");

                        Debug.Assert(nCount >= 0, "");
                    }

                    this.m_nDontMerge++;
                    try
                    {
                        for (int j = 0; j < nCount; j++)
                        {
                            LocationItem item = new LocationItem(this);
                            item.LocationString = strLocationString;
                            this.LocationItems.Add(item);
                        }
                    }
                    finally
                    {
                        this.m_nDontMerge--;
                    }


                    if (string.IsNullOrEmpty(strIDs) == false)
                    {
                        Debug.Assert(nCount >= 0, "");

                        string[] ids = strIDs.Split(new char[] { ',' });

                        int nStartBase = this.LocationItems.Count - nCount;
                        for (int k = 0; k < nCount; k++)
                        {
                            Debug.Assert((nStartBase + k) >= 0
                                && (nStartBase + k) < this.LocationItems.Count,
                                "");
                            LocationItem item = this.LocationItems[nStartBase + k];

                            if (k >= ids.Length)
                                break;

                            string strID = ids[k];

                            if (String.IsNullOrEmpty(strID) == true)
                            {
                                // item.Arrived = false;
                                continue;
                            }

                            int nCountSave = this.LocationItems.Count;

                            item.Arrived = true;
                            item.ArrivedID = strID;

                            Debug.Assert(nCountSave == this.LocationItems.Count, "调用前后的count不能变化");
                        }
                    }
                }

                this.ResetLineState();
                this.SetSize();
                this.LayoutItems();
            }
        }

        internal void SetSize()
        {
            // 调整容器高度
            this.Size = new Size(this.TotalWidth,
                this.m_nLineHeight * Math.Max(1, this.LocationItems.Count) + 4/*微调*/);
        }

        public override Size MaximumSize
        {
            get
            {
                Size size = base.MaximumSize;
                int nLimitHeight = this.m_nLineHeight * Math.Max(1, this.LocationItems.Count) + 4;
                if (size.Height > nLimitHeight
                    || size.Height == 0)
                    size.Height = nLimitHeight;

                int nLimitWidth = this.TotalWidth;
                if (size.Width > nLimitWidth)
                    size.Width = nLimitWidth;

                return size;
            }
            set
            {
                base.MaximumSize = value;
            }
        }

        public override Size MinimumSize
        {
            get
            {
                Size size = base.MinimumSize;
                int nLimitHeight = this.m_nLineHeight * Math.Max(1, this.LocationItems.Count) + 4;
                int nLimitWidth = this.TotalWidth;
                size.Height = nLimitHeight;
                size.Width = nLimitWidth;

                return size;
            }
            set
            {
                base.MinimumSize = value;
            }
        }

        protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
        {
            base.SetBoundsCore(x, y, this.TotalWidth, height, specified);
        }

        public List<LocationItem> SelectedItems
        {
            get
            {
                List<LocationItem> results = new List<LocationItem>();

                for (int i = 0; i < this.LocationItems.Count; i++)
                {
                    LocationItem cur_element = this.LocationItems[i];
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

                for (int i = 0; i < this.LocationItems.Count; i++)
                {
                    LocationItem cur_element = this.LocationItems[i];
                    if ((cur_element.State & ItemState.Selected) != 0)
                        results.Add(i);
                }

                return results;
            }
        }

        // readonly状态的已到事项的个数
        public int ReadOnlyArrivedCount
        {
            get
            {
                int nValue = 0;
                for (int i = 0; i < this.LocationItems.Count; i++)
                {
                    LocationItem item = this.LocationItems[i];
                    if (item.Arrived == true &&
                        (item.ReadOnly == true || this.ReadOnly == true))// 2008/11/12
                        nValue++;
                }
                return nValue;
            }
        }

        // 处于已到状态的事项的个数
        // 注：修改值的时候，虽然改变了checked状态，但是不会触发checkbox要引起的事件
        // Exception:   set operation may throw exception
        public int ArrivedCount
        {
            get
            {
                int nValue = 0;
                for (int i = 0; i < this.LocationItems.Count; i++)
                {
                    LocationItem item = this.LocationItems[i];
                    if (item.Arrived == true)
                        nValue++;
                }
                return nValue;
            }
            set
            {
                // 如果欲设置的事项个数比当前全部事项个数还多，
                // 就需要先增加事项了
                if (value > this.LocationItems.Count)
                {
                    this.Count = value;
                }

                Debug.Assert(value <= this.Count, "");

                // 统计出差距：当前已经为Arrived状态的事项和要求的事项个数之间的的差距
                int nDelta = value - this.ArrivedCount;
                if (nDelta == 0)
                    return;

                if (nDelta > 0)
                {
                    // 增
                    int nPassReadOnlyCount = 0; // 计算中间经过的本来符合要求的但是为readonly状态的事项个数

                    // 从前方开始，逐步勾选不是Arrived状态的事项，直到满nDelta个
                    int nCount = 0;
                    for (int i = 0; i < this.LocationItems.Count; i++)
                    {
                        LocationItem item = this.LocationItems[i];

                        if (item.Arrived == true)
                            continue;

                        // 2008/9/13
                        if (item.ReadOnly == true)
                        {
                            nPassReadOnlyCount++;
                            continue;
                        }

                        item.Arrived = true;
                        nCount++;
                        if (nCount >= nDelta)
                            break;
                    }

                    if (nCount < nDelta)
                    {
                        NotEnoughException ex = new NotEnoughException("无法新增勾选事项 " + nDelta.ToString() + " 个，仅新增了 " + nCount + " 个");
                        ex.WantValue = nDelta;
                        ex.DoneValue = nCount;
                        throw ex;
                    }
                }
                else if (nDelta < 0)
                {
                    // 减

                    Debug.Assert(nDelta < 0, "");
                    // 减少现有已经勾选的事项

                    bool bDeleted = false;  // 是否发生过事项删除

                    int nPassReadOnlyCount = 0; // 计算中间经过的本来符合要求的但是为readonly状态的事项个数

                    // 从后方开始，逐步off已经是Arrived状态的事项，直到满nDelta个
                    int nCount = 0;
                    for (int i = LocationItems.Count - 1; i >= 0; i--)
                    {
                        LocationItem item = this.LocationItems[i];


                        if (item.Arrived == false)
                            continue;

                        // 2008/9/13
                        if (item.ReadOnly == true)
                        {
                            nPassReadOnlyCount++;
                            continue;
                        }

                        item.Arrived = false;

                        // 2008/9/16
                        // 删除本次刚刚增加的，但还没有来得及设置地点字符串的事项
                        if (String.IsNullOrEmpty(item.ArrivedID) == true
                            || item.ArrivedID == "*")
                        {
                            if (String.IsNullOrEmpty(item.LocationString) == true)
                            {
                                this.RemoveItem(item, false);
                                bDeleted = true;
                            }
                        }

                        nCount++;
                        Debug.Assert(nDelta < 0, "");
                        if (nCount >= -1 * nDelta)
                            break;
                    }

                    if (bDeleted == true)
                    {
                        this.SetSize();
                        this.LayoutItems();
                    }

                    if (nCount < -1 * nDelta)
                    {
                        NotEnoughException ex = new NotEnoughException("无法新减勾选事项 " + (-1 * nDelta).ToString() + " 个，仅新减了 " + nCount + " 个");
                        ex.WantValue = nDelta;
                        ex.DoneValue = -1 * nCount;
                        throw ex;
                    }
                }
            }
        }

        void SetUsedText(int index,
            string strText)
        {
            while (this.UsedText.Count < index + 1)
                this.UsedText.Add("");
            this.UsedText[index] = strText;
        }

        string GetUsedText(int index)
        {
            if (index >= this.UsedText.Count)
                return "";
            return this.UsedText[index];
        }

        // 事项个数
        public int Count
        {
            get
            {
                return this.LocationItems.Count;
            }
            set
            {
                // 删除一些
                if (value < this.LocationItems.Count)
                {
                    for (int i = value; i < this.LocationItems.Count; i++)
                    {
                        SetUsedText(i, this.LocationItems[i].LocationString);
                        this.LocationItems[i].RemoveFromContainer();
                    }

                    this.LocationItems.RemoveRange(value, this.LocationItems.Count - value);

                    this.SetSize();
                    this.LayoutItems();

                    return;
                }

                // 增加一些
                if (value > this.LocationItems.Count)
                {

                    int nStart = this.LocationItems.Count;
                    for (int i = nStart; i < value; i++)
                    {
                        LocationItem item = new LocationItem(this);
                        item.LocationString = GetUsedText(i);   // 2009/10/13
                        item.Location = new Point(0, this.m_nLineHeight * i);
                        item.No = (i + 1).ToString();
                        item.State = ItemState.New;
                        this.LocationItems.Add(item);
                    }

                    // this.ResetLineColor();
                    this.SetSize();
                    this.LayoutItems();

                    return;
                }
            }
        }

        public void RefreshLineAndText()
        {
            this.panel_main.Invalidate();   // 促使背景上的文字和线条被刷新
            // this.panel_main.Update();
        }

        // 重新设置事项的显示位置。并重新设置行序号。
        // 一般在LocationItems排序后使用。
        public void LayoutItems()
        {
            // 多于一项的时候才显示序号
            bool bSetNo = false;

            if (this.LocationItems.Count > 1)
                bSetNo = true;

            for (int i = 0; i < this.LocationItems.Count; i++)
            {
                LocationItem item = this.LocationItems[i];
                item.Location = new Point(0, this.m_nLineHeight * i);

                // 如果LocationEditControl AutoScaleMode 不是 AutoScaleMode.None， 则需要重新定位?
                // item.comboBox_location.Size = new Size(this.m_nLocationWidth, 28);

                if (bSetNo == true)
                    item.No = (i + 1).ToString();
                else
                    item.No = "";
            }

            this.RefreshLineAndText();   // 促使背景上的文字和线条被刷新
        }

        public void Sort()
        {
            this.LocationItems.Sort(new LocationItemComparer());
            this.LayoutItems();
        }

        // 2008/8/29
        // 归并。让相同的事项靠近。和排序不同，它不改变已有的基本的序。
        // return:
        //      0   unchanged
        //      1   changed
        public static int Merge(ref List<LocationItem> items)
        {
            bool bChanged = false;
            for (int i = 0; i < items.Count; )
            {
                LocationItem item = items[i];

                string strLocationString = item.LocationString;
                int nTop = i + 1;
                for (int j = i + 1; j < items.Count; j++)
                {
                    LocationItem comp_item = items[j];
                    if (comp_item.LocationString == strLocationString)
                    {
                        // 拉到最近位置(其余被推后)
                        if (j != nTop)
                        {
                            LocationItem temp = items[j];
                            items.RemoveAt(j);
                            items.Insert(nTop, temp);
                            bChanged = true;
                        }

                        nTop++;
                    }

                }

                i = nTop;
            }

            if (bChanged == true)
                return 1;
            return 0;
        }

        public void Merge()
        {
            int nRet = Merge(ref this.LocationItems);
            if (nRet == 1)
                this.LayoutItems();
        }

#if NOOOOOOOOOOOOOOOO
        // 归并。让相同的事项靠近。和排序不同，它不改变已有的基本的序。
        public void Merge()
        {
            bool bChanged = false;
            for (int i = 0; i < this.LocationItems.Count;)
            {
                LocationItem item = this.LocationItems[i];

                string strLocationString = item.LocationString;
                int nTop = i + 1;
                for (int j = i+1; j < this.LocationItems.Count; j++)
                {
                    LocationItem comp_item = this.LocationItems[j];
                    if (comp_item.LocationString == strLocationString)
                    {
                        // 拉到最近位置(其余被推后)
                        if (j != nTop)
                        {
                            LocationItem temp = this.LocationItems[j];
                            this.LocationItems.RemoveAt(j);
                            this.LocationItems.Insert(nTop, temp);
                            bChanged = true;
                        }

                        nTop++;
                    }

                }

                i = nTop;
            }

            if (bChanged == true)
                this.LayoutItems();
        }
#endif

        /*
        // 交换两个对象
        void ExchangeTwoItems(int i, int j)
        {
            if (i == j)
                return;

            LocationItem item = this.LocationItems[i];

            this.LocationItems[i] = this.LocationItems[j];

            this.LocationItems[j] = item;
        }
         * */

        public int TotalWidth
        {
            get
            {
                return m_nLabelWidth    // 左边色块
                    + m_nLocationWidth  // 组合框
                    + m_nArrivedWidth   // checkbox部分
                    + m_nLineLeftBlank
                    + m_nLineWidth
                    + m_nNumberTextWidth
                    + m_nRightBlank;    // 右边空白
            }
        }

        public bool HasGetValueTable()
        {
            if (this.GetValueTable != null)
                return true;

            return false;
        }

        public void OnGetValueTable(object sender, GetValueTableEventArgs e)
        {
            this.GetValueTable(sender, e);
        }

        // 绘制汇总线条
        void PaintLine(Graphics g,
            int nStart,
            int nCount)
        {
            int x = m_nLabelWidth + m_nLocationWidth
                + m_nArrivedWidth
                + m_nLineLeftBlank; // 6
            int w = m_nLineWidth;   // 6

            using (Pen pen = new Pen(SystemColors.GrayText))
            {
                pen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dot;
                pen.DashCap = System.Drawing.Drawing2D.DashCap.Round;
                pen.LineJoin = System.Drawing.Drawing2D.LineJoin.Bevel;

                int start_y = this.m_nLineHeight * nStart + (this.m_nLineHeight / 2);

                // 开始位置横线
                g.DrawLine(pen,
                    new Point(x, start_y),
                    new Point(x + w, start_y));

                int end_y = this.m_nLineHeight * (nStart + nCount - 1) + (this.m_nLineHeight / 2);

                if (nCount > 1)
                {

                    // 竖线
                    g.DrawLine(pen, new Point(x + w, start_y), new Point(x + w, end_y));


                    // 结束位置横线
                    g.DrawLine(pen,
                        new Point(x + w, end_y),
                        new Point(x - 1, end_y)
                        );
                }

                // 文字
                int middle_y = ((start_y + end_y) / 2) - (this.m_nLineHeight / 4);

                using (Brush brush = new SolidBrush(SystemColors.GrayText))
                {
                    g.DrawString(nCount.ToString(),
                        this.panel_main.Font,
                        brush,
                        new Point(x + w + 2, middle_y));
                }
            }
        }

        // 绘制 汇总线条和文字
        private void panel_main_Paint(object sender, PaintEventArgs e)
        {
            // 只有一个事项的时候，不必显示线条和文字
            if (this.LocationItems.Count <= 1)
                return;

            string strPrevText = null;
            int nSegmentCount = 0;
            for (int i = 0; i < this.LocationItems.Count; i++)
            {
                LocationItem item = this.LocationItems[i];

                if (strPrevText != item.LocationString)
                {
                    if (strPrevText != null)
                    {
                        // 结束前面累积的count
                        PaintLine(e.Graphics, i - nSegmentCount, nSegmentCount);
                        nSegmentCount = 0;
                    }

                    nSegmentCount++;
                }
                else
                {
                    nSegmentCount++;
                }


                strPrevText = item.LocationString;
            }

            if (nSegmentCount != 0)
            {
                // 结束前面累积的count
                PaintLine(e.Graphics, this.LocationItems.Count - nSegmentCount, nSegmentCount);
            }
        }

        private void LocationEditControl_Enter(object sender, EventArgs e)
        {
            this.m_bFocused = true;
            this.RefreshLineColor();
        }

        private void LocationEditControl_Leave(object sender, EventArgs e)
        {
            this.m_bFocused = false;
            this.RefreshLineColor();
        }
    }


    // 排序
    class LocationItemComparer : IComparer<LocationItem>
    {
        /*
        public LocationItemComparer()
        {
        }*/

        int IComparer<LocationItem>.Compare(LocationItem x, LocationItem y)
        {
            string s1 = x.LocationString;
            string s2 = y.LocationString;

            return String.Compare(s1, s2);
        }
    }

    // 一个馆藏地点事项
    public class LocationItem : IDisposable
    {
        int DisableArrivedCheckedChanged = 0;   // 是否需要禁止由checkBox_arrived的Checked修改连带引起触发事件。程序主动的修改需要禁止；而用户一般鼠标操作需要触发

        public LocationEditControl Container = null;

        // 颜色、popupmenu
        public Label label_color = null;

        // 馆藏地点
        public ComboBox comboBox_location = null;

        public CheckBox checkBox_arrived = null;

        void DisposeChildControls()
        {
            label_color.Dispose();
            comboBox_location.Dispose();
            checkBox_arrived.Dispose();
            Container = null;
        }

        ItemState m_state = ItemState.Normal;

        int m_nTopX = 0;
        int m_nTopY = 0;

        internal ItemState State
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

#if DEBUG
                    // 2014/11/13
                    // 检查这个状态和 ReadOnly 之间的关系
                    if ((this.m_state & ItemState.ReadOnly) != 0)
                    {
                        Debug.Assert(this.ReadOnly == true, "");
                    }
                    else
                    {
                        Debug.Assert(this.ReadOnly == false, "");
                    }
#endif
                }
            }
        }

        #region 释放资源

#if NO
        // TODO: 容易造成 mem leak。建议用 Dispose() 改写
        ~LocationItem()
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

        internal void SetLineColor()
        {
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
                    this.label_color.BackColor = SystemColors.Highlight;
                    this.label_color.ForeColor = SystemColors.HighlightText;
                    return;
                }
            }

            if ((this.m_state & ItemState.New) != 0)
            {
                this.label_color.BackColor = Color.Yellow;
                this.label_color.ForeColor = SystemColors.GrayText;
                return;
            }
            if ((this.m_state & ItemState.Changed) != 0)
            {
                this.label_color.BackColor = Color.LightGreen;
                this.label_color.ForeColor = SystemColors.GrayText;
                return;
            }

            this.label_color.BackColor = SystemColors.Window;
            this.label_color.ForeColor = SystemColors.GrayText;
        }

        // 单个Item的ReadOnly状态
        // 注: 在set单个item的readonly状态的时候，没有触发 container.OnReadOnlyChanged(); 这样做是为了效率考虑
        public bool ReadOnly
        {
            get
            {
#if NO
                bool bRet = (this.comboBox_location.Enabled == true ? false : true);

#if DEBUG
                if ((this.State & ItemState.ReadOnly) != 0)
                {
                    Debug.Assert(bRet == true, "");
                }
                else
                {
                    Debug.Assert(bRet == false, "");
                }
#endif

                return bRet;
#endif
                return ((this.State & ItemState.ReadOnly) != 0);
            }
            set
            {
                if (value == true)
                {
                    this.comboBox_location.Enabled = false;
                    if (this.checkBox_arrived != null)
                        this.checkBox_arrived.Enabled = false;

                    this.State |= ItemState.ReadOnly;
                }
                else
                {
                    this.comboBox_location.Enabled = true;
                    if (this.checkBox_arrived != null)
                        this.checkBox_arrived.Enabled = true;

                    if ((this.State & ItemState.ReadOnly) != 0)
                        this.State -= ItemState.ReadOnly;
                }
            }
        }

        public LocationItem(LocationEditControl container)
        {
            this.Container = container;

            if (container == null)
                return; // 2008/8/29

            // 颜色
            label_color = new Label();
            label_color.Size = new Size(this.Container.m_nLabelWidth, 26);
            label_color.TextAlign = ContentAlignment.MiddleRight;
            label_color.ForeColor = SystemColors.GrayText;

            container.panel_main.Controls.Add(label_color);

            /*
            // 馆名
            comboBox_library = new ComboBox();
            comboBox_library.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox_library.FlatStyle = FlatStyle.Flat;
            comboBox_library.Size = new Size(this.Container.m_nLibraryWidth, 28);
            comboBox_library.DropDownHeight = 300;
            comboBox_library.DropDownWidth = 300;
            comboBox_library.ForeColor = this.Container.panel_main.ForeColor;
            comboBox_library.Text = "";

            container.panel_main.Controls.Add(comboBox_library);
             * */

            // 馆藏地点
            comboBox_location = new ComboBox();
            comboBox_location.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox_location.FlatStyle = FlatStyle.Flat;
            comboBox_location.DropDownHeight = 300;
            comboBox_location.DropDownWidth = 300;
            comboBox_location.Size = new Size(this.Container.m_nLocationWidth, 28);
            comboBox_location.ForeColor = this.Container.panel_main.ForeColor;

            container.panel_main.Controls.Add(comboBox_location);

            // 已验收标志
            this.checkBox_arrived = new CheckBox();
            this.checkBox_arrived.Size = new Size(this.Container.m_nArrivedWidth, 28);
            this.checkBox_arrived.ForeColor = this.Container.panel_main.ForeColor;
            container.panel_main.Controls.Add(checkBox_arrived);

            if (this.Container.ArriveMode == false)
                this.checkBox_arrived.Enabled = false;  // 在订购状态下，已验收标记也需要显示出来，但处在Disable状态，不可修改

            AddEvents(true);
        }

        public void RemoveFromContainer()
        {
            Container.panel_main.Controls.Remove(this.label_color);
            this.label_color.Dispose();
            this.label_color = null;

            Container.panel_main.Controls.Remove(this.comboBox_location);
            this.comboBox_location.Dispose();
            this.comboBox_location = null;

            Debug.Assert(this.checkBox_arrived != null, "");

            Container.panel_main.Controls.Remove(this.checkBox_arrived);
            this.checkBox_arrived.Dispose();
            this.checkBox_arrived = null;
        }

        public Point Location
        {
            get
            {
                return new Point(this.m_nTopX, this.m_nTopY);
            }
            set
            {
                this.m_nTopX = value.X;
                this.m_nTopY = value.Y;

                this.label_color.Location = new Point(this.m_nTopX, this.m_nTopY);

                /*
                this.comboBox_library.Location = new Point(this.m_nTopX + this.label_color.Width,
                    this.m_nTopY);
                 * */

                this.comboBox_location.Location = new Point(this.m_nTopX + this.label_color.Width/* + this.comboBox_library.Width*/,
                    this.m_nTopY);

                if (this.checkBox_arrived != null)
                {
                    this.checkBox_arrived.Location = new Point(this.m_nTopX + this.label_color.Width + this.comboBox_location.Width,
                       this.m_nTopY);
                }

                // TODO: 修改容器大小?
            }
        }

        public string LocationString
        {
            get
            {
                return this.comboBox_location.Text;
            }
            set
            {
                this.comboBox_location.Text = value;
            }
        }

        // 行序号
        public string No
        {
            get
            {
                return this.label_color.Text;
            }
            set
            {
                this.label_color.Text = value;
            }
        }

        public bool Arrived
        {
            get
            {
                Debug.Assert(this.checkBox_arrived != null, "");
                return this.checkBox_arrived.Checked;
            }
            set
            {
                Debug.Assert(this.checkBox_arrived != null, "");

                this.DisableArrivedCheckedChanged++;    // 2008/12/17
                try
                {
                    this.checkBox_arrived.Checked = value;
                }
                finally
                {
                    this.DisableArrivedCheckedChanged--;
                }

                if (value == false)
                {
                    // this.checkBox_arrived.Text = "";    // TODO: 这里有没有必要把id清掉？其实可以不清(这样的好处是重新勾选后id还在)，在保存记录的最后阶段再决定清(保存记录的时候必须清，因为id存在与否代表了check状态)
                }
                else
                {
                    if (this.checkBox_arrived.Text == "")
                    {
                        this.checkBox_arrived.Text = "*";   // 表示新到的项

                        // 为何不起作用?
                        // this.Container.toolTip1.SetToolTip(this.checkBox_arrived, this.checkBox_arrived.Text);
                    }
                }
            }
        }

        public string ArrivedID
        {
            get
            {
                Debug.Assert(this.checkBox_arrived != null, "");

                // 如果checked为true，但是text为空，则返回星号。以便后续环节知道这是true的状态
                if (this.checkBox_arrived.Checked == true
                    && string.IsNullOrEmpty(this.checkBox_arrived.Text) == true)
                {
                    return "*";
                }

                return this.checkBox_arrived.Text;
            }
            set
            {
                Debug.Assert(this.checkBox_arrived != null, "");
                this.checkBox_arrived.Text = value;

                // 为何不起作用?
                // this.Container.toolTip1.SetToolTip(this.checkBox_arrived, this.checkBox_arrived.Text);

                if (string.IsNullOrEmpty(value) == false)
                {
                    if (this.checkBox_arrived.Checked != true)
                    {
                        this.DisableArrivedCheckedChanged++;    // 2009/12/17
                        try
                        {
                            this.checkBox_arrived.Checked = true;   // 补充设置状态
                        }
                        finally
                        {
                            this.DisableArrivedCheckedChanged--;
                        }
                    }
                }

                // 不过，checked == true的时候，text仍可以为空。这用来表示新增的、尚未赋给id字符串的事项
                // 而当字符串不为空的时候，checked绝对不能为false
            }
        }

        // 2015/7/21
        void AddEvents(bool bAdd)
        {
            if (bAdd)
            {
                // label_color
                this.label_color.MouseUp += new MouseEventHandler(label_color_MouseUp);

                label_color.MouseClick += new MouseEventHandler(label_color_MouseClick);

                /*
                // library
                this.comboBox_library.DropDown -= new EventHandler(comboBox_location_DropDown);
                this.comboBox_library.DropDown += new EventHandler(comboBox_location_DropDown);
                 * */


                // location
                this.comboBox_location.DropDown += new EventHandler(comboBox_location_DropDown);

                this.comboBox_location.TextChanged += new EventHandler(comboBox_location_TextChanged);

                this.comboBox_location.Enter += new EventHandler(comboBox_location_Enter);

                // arrived
                this.checkBox_arrived.CheckedChanged += new EventHandler(checkBox_arrived_CheckedChanged);
            }
            else
            {
                this.label_color.MouseUp -= new MouseEventHandler(label_color_MouseUp);
                label_color.MouseClick -= new MouseEventHandler(label_color_MouseClick);
                this.comboBox_location.DropDown -= new EventHandler(comboBox_location_DropDown);
                this.comboBox_location.TextChanged -= new EventHandler(comboBox_location_TextChanged);
                this.comboBox_location.Enter -= new EventHandler(comboBox_location_Enter);
                this.checkBox_arrived.CheckedChanged -= new EventHandler(checkBox_arrived_CheckedChanged);
            }
        }

        // 已到达(验收)checkbox被checked
        // 2008/4/16
        void checkBox_arrived_CheckedChanged(object sender, EventArgs e)
        {
            this.Container.Changed = true;

            if (this.DisableArrivedCheckedChanged == 0)
                this.Container.OnArrivedChanged();
        }

        void label_color_MouseClick(object sender, MouseEventArgs e)
        {
            this.Container.Focus(); // 2008/9/16

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
        }

        void comboBox_location_Enter(object sender, EventArgs e)
        {
            this.Container.SelectItem(this, true);
        }

        // location文字改变
        void comboBox_location_TextChanged(object sender, EventArgs e)
        {
#if NO
            // 如果当前选定的事项多于一个，则选定的其他事项也要修改
            List<LocationItem> selected = this.Container.SelectedItems;
            for (int i = 0; i < selected.Count; i++)
            {
                LocationItem item = selected[i];
                if (item == this)
                    continue;

                if (item.LocationString != this.LocationString)
                {
                    item.LocationString = this.LocationString;

                    if ((item.State & ItemState.New) == 0)
                        item.State |= ItemState.Changed;
                }
            }
#endif


            // 立即归并、调整顺序
            if (Control.ModifierKeys == Keys.Control)
            {
            }
            else
            {
                if (this.Container.m_nDontMerge == 0)
                    this.Container.Merge();
            }

            this.Container.RefreshLineAndText();

            if ((this.State & ItemState.New) == 0)
            {
                this.State |= ItemState.Changed;
                // TODO: 补一次事件?
            }

            this.Container.Changed = true;
        }

        // 防止重入 2009/7/19
        int m_nInDropDown = 0;

        void comboBox_location_DropDown(object sender, EventArgs e)
        {
            // 防止重入 2009/7/19
            if (this.m_nInDropDown > 0)
                return;

            Cursor oldCursor = this.Container.Cursor;
            this.Container.Cursor = Cursors.WaitCursor;
            this.m_nInDropDown++;
            try
            {
                ComboBox combobox = (ComboBox)sender;

                if (combobox.Items.Count == 0
                    && this.Container.HasGetValueTable() != false)
                {
                    GetValueTableEventArgs e1 = new GetValueTableEventArgs();
                    e1.DbName = this.Container.DbName;

                    if (combobox == this.comboBox_location)
                        e1.TableName = "location";
                    else
                    {
                        Debug.Assert(false, "不支持的sender");
                        return;
                    }

                    this.Container.OnGetValueTable(this, e1);

                    if (e1.values != null)
                    {
                        for (int i = 0; i < e1.values.Length; i++)
                        {
                            combobox.Items.Add(e1.values[i]);
                        }
                    }
                    else
                    {
                        combobox.Items.Add("<not found>");
                    }
                }
            }
            finally
            {
                this.Container.Cursor = oldCursor;
                this.m_nInDropDown--;
            }
        }

        void label_color_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right)
                return;

            ContextMenu contextMenu = new ContextMenu();
            MenuItem menuItem = null;

            int nSelectedCount = this.Container.SelectedIndices.Count;
            /*
            bool bHasClipboardObject = false;
            IDataObject ido = Clipboard.GetDataObject();
            if (ido.GetDataPresent(DataFormats.Text) == true)
                bHasClipboardObject = true;
             * */

            // 2012/5/30
            menuItem = new MenuItem("设值");
            if (nSelectedCount == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            List<string> values = GetLocationListItem();
            if (values.Count > 0)
            {
                for (int i = 0; i < values.Count; i++)
                {
                    string strText = values[i];
                    MenuItem subMenuItem = new MenuItem(strText);
                    subMenuItem.Tag = strText;
                    subMenuItem.Click += new System.EventHandler(this.menu_setLocationString_Click);
                    menuItem.MenuItems.Add(subMenuItem);
                }
            }
            else
                menuItem.Enabled = false;

            menuItem = new MenuItem("全选(&A)");
            menuItem.Click += new System.EventHandler(this.menu_selectAll_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            //
            menuItem = new MenuItem("前插(&I)");
            menuItem.Click += new System.EventHandler(this.menu_insertElement_Click);
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("后插(&A)");
            menuItem.Click += new System.EventHandler(this.menu_appendElement_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            menuItem = new MenuItem("强制清除参考ID(&R)");
            menuItem.Click += new System.EventHandler(this.menu_clearRefIDs_Click);
            if (this.Container.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("合并同名事项(&M)");
            menuItem.Click += new System.EventHandler(this.menu_merge_Click);
            contextMenu.MenuItems.Add(menuItem);


            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("删除(&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteElements_Click);
            if (this.Container.SelectedItems.Count == 0)
                menuItem.Enabled = false;
            contextMenu.MenuItems.Add(menuItem);


            contextMenu.Show(this.label_color, new Point(e.X, e.Y));
        }

        List<string> m_locationListItems = new List<string>();

        List<string> GetLocationListItem()
        {
            if (m_locationListItems.Count > 0)
                return m_locationListItems;

            GetValueTableEventArgs e1 = new GetValueTableEventArgs();
            e1.DbName = this.Container.DbName;
            e1.TableName = "location";

            this.Container.OnGetValueTable(this, e1);

            if (e1.values != null)
            {
                for (int i = 0; i < e1.values.Length; i++)
                {
                    m_locationListItems.Add(e1.values[i]);
                }
            }

            return m_locationListItems;
        }

        void menu_selectAll_Click(object sender, EventArgs e)
        {
            this.Container.SelectAll();
        }

        // 一次性修改多个combobox的值
        void menu_setLocationString_Click(object sender, EventArgs e)
        {
            string strValue = (string)((MenuItem)sender).Tag;

            foreach (LocationItem item in this.Container.SelectedItems)
            {
                if (item.LocationString != strValue)
                    item.LocationString = strValue;
            }
        }

        void menu_insertElement_Click(object sender, EventArgs e)
        {
            int nPos = this.Container.LocationItems.IndexOf(this);

            if (nPos == -1)
                throw new Exception("not found myself");

            this.Container.InsertNewItem(nPos);
        }

        void menu_appendElement_Click(object sender, EventArgs e)
        {
            int nPos = this.Container.LocationItems.IndexOf(this);
            if (nPos == -1)
            {
                throw new Exception("not found myself");
            }

            this.Container.InsertNewItem(nPos + 1);
        }

        // 合并同名事项
        void menu_merge_Click(object sender, EventArgs e)
        {
            this.Container.Merge();
        }

        // 强制清除参考ID
        void menu_clearRefIDs_Click(object sender, EventArgs e)
        {
            List<LocationItem> selected_lines = this.Container.SelectedItems;

            if (selected_lines.Count == 0)
            {
                MessageBox.Show(this.Container, "尚未选定要清除参考ID的事项");
                return;
            }

            string strText = "";

            if (selected_lines.Count == 1)
                strText = "确实要清除事项 '" + selected_lines[0].No + "' 的参考ID? ";
            else
                strText = "确实要清除所选定的 " + selected_lines.Count.ToString() + " 个事项的参考ID?";

            DialogResult result = MessageBox.Show(this.Container,
                strText,
                "LocationEditControl",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            int nNotChangeCount = 0;
            int nChangedCount = 0;

            for (int i = 0; i < selected_lines.Count; i++)
            {
                LocationItem item = selected_lines[i];
                if ((item.State & ItemState.ReadOnly) != 0)
                {
                    nNotChangeCount++;
                    continue;
                }

                if (item.Arrived == true)
                    item.ArrivedID = "*";
                else
                    item.ArrivedID = "";

                nChangedCount++;
            }

            if (nNotChangeCount > 0)
            {
                MessageBox.Show(this.Container, "有 " + nNotChangeCount.ToString() + " 项只读事项未能清除其参考ID");
            }
        }

        // 删除当前元素
        void menu_deleteElements_Click(object sender, EventArgs e)
        {
            List<LocationItem> selected_lines = this.Container.SelectedItems;

            if (selected_lines.Count == 0)
            {
                MessageBox.Show(this.Container, "尚未选定要删除的事项");
                return;
            }
            string strText = "";

            if (selected_lines.Count == 1)
                strText = "确实要删除事项 '" + selected_lines[0].No + "'? ";
            else
                strText = "确实要删除所选定的 " + selected_lines.Count.ToString() + " 个事项?";

            DialogResult result = MessageBox.Show(this.Container,
                strText,
                "LocationEditControl",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            int nNotDeleteCount = 0;
            int nDeletedCount = 0;

            for (int i = 0; i < selected_lines.Count; i++)
            {
                LocationItem item = selected_lines[i];
                if ((item.State & ItemState.ReadOnly) != 0)
                {
                    nNotDeleteCount++;
                    continue;
                }
                this.Container.RemoveItem(item, false);
                nDeletedCount++;
            }

            if (nDeletedCount > 0)
            {
                this.Container.SetSize();
                this.Container.LayoutItems();
            }

            if (nNotDeleteCount > 0)
            {
                MessageBox.Show(this.Container, "有 " + nNotDeleteCount.ToString() + " 项只读事项未能删除");
            }
        }
    }

    /*
    /// <summary>
    /// ReadOnly状态发生改变
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void ReadOnlyChangedEventHandler(object sender,
    ReadOnlyChangedEventArgs e);

    /// <summary>
    /// ReadOnly状态发生改变的参数
    /// </summary>
    public class ReadOnlyChangedEventArgs : EventArgs
    {
        
    }
     * */

    // 增加、减少目的不能达到的异常
    public class NotEnoughException : Exception
    {
        public int WantValue = 0;   // 想要改变的值
        public int DoneValue = 0;   // 实际改变的值

        public NotEnoughException(string s)
            : base(s)
        {
        }
    }
}
