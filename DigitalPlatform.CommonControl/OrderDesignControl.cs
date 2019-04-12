﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Diagnostics;
using System.Xml;

using DigitalPlatform.GUI;
using DigitalPlatform.Xml;
using DigitalPlatform.IO;
using DigitalPlatform.Text;
using DigitalPlatform.Core;

namespace DigitalPlatform.CommonControl
{
    /// <summary>
    /// 订购信息交叉关系 控件
    /// </summary>
    public partial class OrderDesignControl : UserControl
    {
        // 是否处在初始化过程中。如果是，则 item 不响应 textchanged 事件。避免引起连锁更新其他文字栏
        public bool InInitial { get; set; }

        public ToolTip ToolTip { get; set; }

        public bool CheckDupItem = true;    // 是否在结束的时候检查三元组、四元组

        internal bool m_bFocused = false;

        bool m_bHideSelection = true;

        internal int DisableNewlyOrderTextChanged = 0;

        internal int DisableNewlyArriveTextChanged = 0;

        public Item LastClickItem = null;   // 最近一次click选择过的Item对象

        string _sellerFilter = "";
        // 书商名称过滤。"<不过滤>"或者空，表示不过滤
        public string SellerFilter
        {
            get
            {
                return _sellerFilter;
            }
            set
            {
                _sellerFilter = value;
            }
        }

        // 获取值列表时作为线索的数据库名
        string m_strBiblioDbName = "";
        public string BiblioDbName
        {
            get
            {
                return this.m_strBiblioDbName;
            }
            set
            {
                this.m_strBiblioDbName = value;
                foreach (Item item in this.Items)
                {
                    item.location.DbName = value;
                }
            }
        }

        // 获得缺省记录
        /// <summary>
        /// 获得缺省记录
        /// </summary>
        public event GetDefaultRecordEventHandler GetDefaultRecord = null;

        /// <summary>
        /// 获得值列表
        /// </summary>
        public event GetValueTableEventHandler GetValueTable = null;

        // 2012/10/4
        /// <summary>
        /// 检查馆代码是否在管辖范围内
        /// </summary>
        public event VerifyLibraryCodeEventHandler VerifyLibraryCode = null;

        // const int WM_NUMBER_CHANGED = API.WM_USER + 201;

        int m_nInSuspend = 0;

        // public int m_nTotalCopy = 0;

        public List<Item> Items = new List<Item>();

        bool m_bChanged = false;

        public OrderDesignControl()
        {
            InitializeComponent();
        }

        bool m_bSeriesMode = false;

        // 是否为期刊模式? true表示为期刊模式，false表示为图书模式
        [Category("Appearance")]
        [DescriptionAttribute("SeriesMode")]
        [DefaultValue(false)]
        public bool SeriesMode
        {
            get
            {
                return this.m_bSeriesMode;
            }
            set
            {
                if (this.m_bSeriesMode != value)
                {
                    this.m_bSeriesMode = value;

                    SetSeriesMode(value);
                }
            }
        }

        void SetSeriesMode(bool bSeriesMode)
        {
            this.DisableUpdate();

            for (int i = 0; i < this.Items.Count; i++)
            {
                Item item = this.Items[i];

                item.SeriesMode = bSeriesMode;
            }

            if (bSeriesMode == true)
            {
                this.label_range.Visible = true;
                this.label_range.Text = "时间范围";
                this.label_issueCount.Visible = true;
            }
            else
            {
                // this.label_range.Visible = false;
                this.label_range.Visible = true;    // ????
                this.label_range.Text = "预计出版时间";
                this.label_issueCount.Visible = false;
            }

            this.EnableUpdate();
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

                SetArriveMode(value);
            }
        }

        void SetArriveMode(bool bArriveMode)
        {
            if (bArriveMode == true)
            {
                // 验收态

                /*
                this.label_orderedTotalCopy.Text = "已验收总复本数(&O):";
                this.label_newlyOrderTotalCopy.Text = "新验收总复本数(&N):";
                 * */
                this.label_copy.ForeColor = Color.Red;
                this.label_price.ForeColor = Color.Red;
                this.label_location.ForeColor = Color.Red;

                this.label_newlyOrderTotalCopy.Visible = false;
                this.textBox_newlyOrderTotalCopy.Visible = false;

                this.button_newItem.Visible = false;

                this.label_arrivedTotalCopy.Visible = true;
                this.textBox_arrivedTotalCopy.Visible = true;

                // 2008/11/3
                this.panel_targetRecPath.Visible = true;

                this.label_newlyArriveTotalCopy.Visible = true;
                this.textBox_newlyArriveTotalCopy.Visible = true;

                // 2008/11/3
                this.button_fullyAccept.Visible = true;
            }
            else
            {
                // false 表示订购态

                /*
                this.label_orderedTotalCopy.Text = "已订购总复本数(&O):";
                this.label_newlyOrderTotalCopy.Text = "新订购总复本数(&N):";
                 * */

                this.label_copy.ForeColor = this.ForeColor;
                this.label_price.ForeColor = this.ForeColor;
                this.label_location.ForeColor = this.ForeColor;


                this.label_newlyOrderTotalCopy.Visible = true;
                this.textBox_newlyOrderTotalCopy.Visible = true;

                this.button_newItem.Visible = true;

                this.label_arrivedTotalCopy.Visible = false;
                this.textBox_arrivedTotalCopy.Visible = false;

                // 2008/11/3
                this.panel_targetRecPath.Visible = false;

                this.label_newlyArriveTotalCopy.Visible = false;
                this.textBox_newlyArriveTotalCopy.Visible = false;

                // 2008/11/3
                this.button_fullyAccept.Visible = false;
            }
        }

        public void EnsureVisible(Item item)
        {
            int[] row_heights = this.tableLayoutPanel_content.GetRowHeights();
            int nYOffs = row_heights[0];
            int i = 1;
            foreach (Item cur_item in this.Items)
            {
                if (cur_item == item)
                    break;
                nYOffs += row_heights[i++];
            }

            // this.AutoScrollPosition = new Point(this.AutoScrollOffset.X, 1000);

            this.AutoScrollPosition = new Point(this.AutoScrollOffset.X, nYOffs);
        }

        bool m_bOrderedTotalCopyVisible = false;

        // 已订购份数 是否可见?
        internal bool OrderedTotalCopyVisible
        {
            get
            {
                return this.m_bOrderedTotalCopyVisible;
            }
            set
            {
                if (this.m_bOrderedTotalCopyVisible != value)
                {
                    this.m_bOrderedTotalCopyVisible = value;

                    this.textBox_orderedTotalCopy.Visible = value;
                    this.label_orderedTotalCopy.Visible = value;
                }
            }
        }

        bool m_bArrivedTotalCopyVisible = false;

        // 已验收份数 是否可见?
        internal bool ArrivedTotalCopyVisible
        {
            get
            {
                return this.m_bArrivedTotalCopyVisible;
            }
            set
            {
                if (this.m_bArrivedTotalCopyVisible != value)
                {
                    this.m_bArrivedTotalCopyVisible = value;

                    this.textBox_arrivedTotalCopy.Visible = value;
                    this.label_arrivedTotalCopy.Visible = value;
                }
            }
        }

        // 验收目标记录路径
        public string TargetRecPath
        {
            get
            {
                return this.textBox_targetRecPath.Text;
            }
            set
            {
                this.textBox_targetRecPath.Text = value;
            }
        }

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

        /// <summary>
        /// 内容是否发生过修改
        /// </summary>
        [Category("Content")]
        [DescriptionAttribute("Changed")]
        [DefaultValue(false)]
        public bool Changed
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

                    bool bOldChanged = this.m_bChanged;
                    if (value == false)
                        ResetLineState();
                    // 因为 ResetLineState 过程会导致 Changed 被修改为 true
                    this.m_bChanged = bOldChanged;
                    Debug.Assert(bOldChanged == this.m_bChanged, "");
                }
            }
        }


        // return:
        //      -1  error
        //      0   succeed
        public static int VerifyDateRange(string strValue,
            out string strError)
        {
            strError = "";

            string strStart = "";
            string strEnd = "";

            int nRet = strValue.IndexOf("-");
            if (nRet == -1)
            {
                strStart = strValue;
                if (strStart.Length != 8)
                {
                    strError = "时间范围 '" + strValue + "' 格式不正确";
                    return -1;
                }

                strEnd = "";
            }
            else
            {
                strStart = strValue.Substring(0, nRet).Trim();

                if (strStart.Length != 8)
                {
                    strError = "时间范围 '" + strValue + "' 内 '" + strStart + "' 格式不正确";
                    return -1;
                }

                strEnd = strValue.Substring(nRet + 1).Trim();

                if (strStart.Length != 8)
                {
                    strError = "时间范围 '" + strValue + "' 内 '" + strEnd + "' 格式不正确";
                    return -1;
                }
            }

            if (String.Compare(strStart, strEnd) > 0)
            {
                strError = "时间范围内的起始时间不应大于结束时间";
                return -1;
            }

            return 0;
        }

        // 是否为虚拟价格。就是说有 {} 包围的价格
        public static bool IsVirtual(string strPrice)
        {
            if (string.IsNullOrEmpty(strPrice))
                return false;
            if (strPrice.StartsWith("{"))
                return true;
            return false;
        }

        // 进行检查
        // return:
        //      -1  函数运行出错
        //      0   检查没有发现错误
        //      1   检查发现了错误
        public int Check(out string strError)
        {
            strError = "";
            int nRet = 0;

            bool bStrict = true;    // 是否严格检查

            // 检查是否每行都输入了价格、份数
            for (int i = 0; i < this.Items.Count; i++)
            {
                Item item = this.Items[i];

                // 只检查新规划的事项
                if ((item.State & ItemState.ReadOnly) != 0)
                    continue;
                // 跳过未曾修改过的事项
                if ((item.State & ItemState.New) == 0
                    && (item.State & ItemState.Changed) == 0)
                    continue;

                // 进行检查
                // return:
                //      -1  函数运行出错
                //      0   检查没有发现错误
                //      1   检查发现了错误
                nRet = item.location.Check(out strError);
                if (nRet != 0)
                {
                    strError = "第 " + (i + 1).ToString() + " 行: 馆藏分配去向 格式有问题: " + strError;
                    return 1;
                }

                // 2009/11/9
                string strTotalPrice = "";
                try
                {
                    strTotalPrice = item.TotalPrice;
                }
                catch (Exception ex)
                {
                    strError = "获取item.TotalPrice时出错: " + ex.Message;
                    return -1;
                }

                if (String.IsNullOrEmpty(strTotalPrice) == true)
                {
                    // 总价为空的时候，必须输入订购价
                    if (String.IsNullOrEmpty(item.Price) == true
                        && string.IsNullOrEmpty(item.FixedPrice) == true)
                    {
                        strError = "第 " + (i + 1).ToString() + " 行: 尚未输入价格";
                        return 1;
                    }
                }
                else
                {
                    if (String.IsNullOrEmpty(item.StateString) == true
                        && String.IsNullOrEmpty(item.Price) == false)
                    {
                        strError = "第 " + (i + 1).ToString() + " 行: 当输入了价格 ('" + item.Price + "') 时，必须把总价格设置为空 (但现在为 '" + strTotalPrice + "')";
                        return 1;
                    }
                }

                if (this.ArriveMode == false)   // 2009/2/4
                {
                    // 订购模式
                    if (String.IsNullOrEmpty(item.CopyString) == true)
                    {
                        strError = "第 " + (i + 1).ToString() + " 行: 尚未输入复本数";
                        return 1;
                    }

                    // 自动计算出订购价
                    if (string.IsNullOrEmpty(item.OldFixedPrice) == false
                        && IsVirtual(item.OldFixedPrice) == false
                        && string.IsNullOrEmpty(item.OldPrice) == true
                        && string.IsNullOrEmpty(item.TotalPrice) == true)
                    {
                        // return:
                        //      -1  计算过程出现错误
                        //      0   strFixedPrice 为空，无法计算
                        //      1   计算成功
                        nRet = ComputeOrderPriceByFixedPrice(item.OldFixedPrice,
                    item.OldDiscount,
                    out string strWishOrderPrice,
                    out strError);
                        if (nRet == -1)
                        {
                            strError = "第 " + (i + 1).ToString() + " 行: 根据订购码洋 '" + item.OldFixedPrice + "' 和订购折扣 '" + item.OldDiscount + "' 自动计算订购价时出错: " + strError;
                            return -1;
                        }
                        if (nRet == 1)
                        {
                            item.OldPrice = strWishOrderPrice;
                            item.Price = strWishOrderPrice;
                        }
                    }

#if NO
                    // 自动计算出码洋
                    if (string.IsNullOrEmpty(item.OldFixedPrice) == true
                        && string.IsNullOrEmpty(item.OldDiscount) == false
                        && string.IsNullOrEmpty(item.OldPrice) == false)
                    {
                        // TODO: 注意订购价可能为 "CNY100/3" 形态
                        // return:
                        //      -1  计算过程出现错误
                        //      0   strPrice 为空，无法计算
                        //      1   计算成功
                        nRet = ComputeFixedPriceByOrderPrice(item.OldPrice,
                    item.OldDiscount,
                    out string strResultPrice,
                    out strError);
                        if (nRet == -1)
                        {
                            strError = "第 " + (i + 1).ToString() + " 行: 根据订购价 '" + item.OldPrice + "' 和订购折扣 '" + item.OldDiscount + "' 自动计算订码洋时出错: " + strError;
                            return -1;
                        }
                        if (nRet == 1)
                        {
                            if (string.IsNullOrEmpty(strResultPrice) == false)
                                strResultPrice = "{" + strResultPrice + "}";    // {} 表示这是计算出来的，不是原始值
                            item.OldDiscount = strResultPrice;
                            item.Discount = strResultPrice;
                        }
                    }
#endif

                    if (string.IsNullOrEmpty(item.OldPrice) == false)
                    {
                        // 2018/8/1
                        // 检查码洋、折扣、订购价之间的关系
                        // return:
                        //      -2  码洋和订购价货币单位不同，无法进行校验。TODO: 今后可增加汇率表，让这种情况变得可以校验
                        //      -1  校验过程出错
                        //      0   校验发现三者关系不正确
                        //      1   校验三者关系正确
                        nRet = VerifyOrderPriceByFixedPrice(StringUtil.Unquote(item.OldFixedPrice, "{}"),
                            item.OldDiscount,
                            item.OldPrice,
                            out string strWishOldPrice,
                            out strError);
                        if (nRet != 1)
                        {
                            if (nRet == -1)
                            {
                                strError = "第 " + (i + 1).ToString() + " 行: 验证订购码洋 '" + item.OldFixedPrice + "' 和订购折扣 '" + item.OldDiscount + "' 和订购价 '" + item.OldPrice + "' 关系时出错: " + strError;
                                return 1;
                            }
                            strError = "第 " + (i + 1).ToString() + " 行: 订购码洋 '" + item.OldFixedPrice + "' 和订购折扣 '" + item.OldDiscount + "' 计算出的结果 '" + strWishOldPrice + "' 和订购价 '" + item.OldPrice + "' 不符";
                            return 1;
                        }
                    }
                }
                else
                {
                    // 验收模式

                    // 不一定每一行都要验收

                    // TODO: 是否检查一下至少有一行验收了？不太好检查。

                    // 自动计算出验收价
                    if (string.IsNullOrEmpty(item.FixedPrice) == false
                        && IsVirtual(item.FixedPrice) == false
                        && string.IsNullOrEmpty(item.Price) == true)
                    {
                        // return:
                        //      -1  计算过程出现错误
                        //      0   strFixedPrice 为空，无法计算
                        //      1   计算成功
                        nRet = ComputeOrderPriceByFixedPrice(item.FixedPrice,
                    item.Discount,
                    out string strWishOrderPrice,
                    out strError);
                        if (nRet == -1)
                        {
                            strError = "第 " + (i + 1).ToString() + " 行: 根据验收码洋 '" + item.FixedPrice + "' 和验收折扣 '" + item.Discount + "' 自动计算验收价时出错: " + strError;
                            return -1;
                        }
                        if (nRet == 1)
                            item.Price = strWishOrderPrice;
                    }

                    if (string.IsNullOrEmpty(item.Price) == false)
                    {
                        // 2018/8/1
                        // 检查码洋、折扣、订购价之间的关系
                        // return:
                        //      -2  码洋和订购价货币单位不同，无法进行校验。TODO: 今后可增加汇率表，让这种情况变得可以校验
                        //      -1  校验过程出错
                        //      0   校验发现三者关系不正确
                        //      1   校验三者关系正确
                        nRet = VerifyOrderPriceByFixedPrice(StringUtil.Unquote(item.FixedPrice, "{}"),
                            item.Discount,
                            item.Price,
                            out string strWishPrice,
                            out strError);
                        if (nRet != 1)
                        {
                            if (nRet == -1)
                            {
                                strError = "第 " + (i + 1).ToString() + " 行: 验证验收码洋 '" + item.FixedPrice + "' 和验收折扣 '" + item.Discount + "' 和验收价 '" + item.Price + "' 关系时出错: " + strError;
                                return 1;
                            }
                            strError = "第 " + (i + 1).ToString() + " 行: 验收码洋 '" + item.FixedPrice + "' 和验收折扣 '" + item.Discount + "' 计算出的结果 '" + strWishPrice + "' 和验收价 '" + item.Price + "' 不符";
                            return 1;
                        }
                    }
                }

                if (this.SeriesMode == true)
                {
                    // 期刊

                    if (String.IsNullOrEmpty(item.RangeString) == true)
                    {
                        strError = "第 " + (i + 1).ToString() + " 行: 尚未输入时间范围";
                        return 1;
                    }

                    if (item.RangeString.Length != (2 * 8 + 1))
                    {
                        strError = "第 " + (i + 1).ToString() + " 行: 尚未输入完整的时间范围";
                        return 1;
                    }

                    // return:
                    //      -1  error
                    //      0   succeed
                    nRet = VerifyDateRange(item.RangeString,
                        out strError);
                    if (nRet == -1)
                    {
                        strError = "第 " + (i + 1).ToString() + " 行: " + strError;
                        return 1;
                    }

                    if (String.IsNullOrEmpty(item.IssueCountString) == true)
                    {
                        strError = "第 " + (i + 1).ToString() + " 行: 尚未输入期数";
                        return 1;
                    }
                }
                else
                {
                    // 图书

                    if (item.IssueCountValue != 1)
                    {
                        item.IssueCountValue = 1;
                        //strError = "第 " + (i + 1).ToString() + " 行: 图书订购只允许期数为 1";
                        //return 1;
                    }
                }

                if (bStrict == true)
                {
                    if (String.IsNullOrEmpty(item.Source) == true
                        && item.Seller != "交换" && item.Seller != "赠")
                    {
                        strError = "第 " + (i + 1).ToString() + " 行: 尚未输入经费来源";
                        return 1;
                    }

                    // 2009/2/15
                    if (item.Seller == "交换" || item.Seller == "赠")
                    {
                        if (String.IsNullOrEmpty(item.Source) == false)
                        {
                            strError = "第 " + (i + 1).ToString() + " 行: 如果渠道为 交换 或 赠，则经费来源必须为空";
                            return 1;
                        }
                    }

                    if (String.IsNullOrEmpty(item.Seller) == true)
                    {
                        strError = "第 " + (i + 1).ToString() + " 行: 尚未输入渠道";
                        return 1;
                    }
                    /*
                    if (String.IsNullOrEmpty(item.CatalogNo) == true)
                    {
                        strError = "第 " + (i + 1).ToString() + " 行: 尚未输入书目号";
                        return 1;
                    }
                     * */
                    if (String.IsNullOrEmpty(item.Class) == true)
                    {
                        strError = "第 " + (i + 1).ToString() + " 行: 尚未输入类别";
                        return 1;
                    }
                }
            }

            if (bStrict == true)
            {
                // 检查 渠道 + 经费来源 + 价格 3元组是否有重复
                for (int i = 0; i < this.Items.Count; i++)
                {
                    Item item = this.Items[i];

                    // 只检查新规划的事项
                    if ((item.State & ItemState.ReadOnly) != 0)
                        continue;

                    // 2009/2/4 只检查新输入的订购事项
                    if (String.IsNullOrEmpty(item.StateString) == false)
                        continue;

                    string strLocationString = item.location.Value;
                    LocationCollection locations = new LocationCollection();
                    nRet = locations.Build(strLocationString, out strError);
                    if (nRet == -1)
                    {
                        strError = "第 " + (i + 1).ToString() + " 行: 馆藏分配去向字符串 '" + strLocationString + "' 格式错误: " + strError;
                        return -1;
                    }
                    string strUsedLibraryCodes = StringUtil.MakePathList(locations.GetUsedLibraryCodes());

                    // 检查馆代码是否在管辖范围内
                    // 只检查修改过的事项
                    if (IsChangedItem(item) == true
                        && this.VerifyLibraryCode != null)
                    {
                        VerifyLibraryCodeEventArgs e = new VerifyLibraryCodeEventArgs();
                        e.LibraryCode = strUsedLibraryCodes;
                        this.VerifyLibraryCode(this, e);
                        if (string.IsNullOrEmpty(e.ErrorInfo) == false)
                        {
                            strError = "第 " + (i + 1).ToString() + " 行: 馆藏分配去向错误: " + e.ErrorInfo;
                            return -1;
                        }
                    }

                    for (int j = i + 1; j < this.Items.Count; j++)
                    {
                        Item temp_item = this.Items[j];

                        // 只检查新规划的事项
                        if ((temp_item.State & ItemState.ReadOnly) != 0)
                            continue;
                        // 跳过未曾修改过的事项
                        if (IsChangedItem(temp_item) == false)
                            continue;

                        // 2009/2/4 只检查新输入的订购事项
                        if (String.IsNullOrEmpty(temp_item.StateString) == false)
                            continue;

                        string strTempLocationString = temp_item.location.Value;
                        LocationCollection temp_locations = new LocationCollection();
                        nRet = temp_locations.Build(strTempLocationString, out strError);
                        if (nRet == -1)
                        {
                            strError = "第 " + (j + 1).ToString() + " 行: 馆藏分配去向字符串 '" + strTempLocationString + "' 格式错误: " + strError;
                            return -1;
                        }
                        string strTempUsedLibraryCodes = StringUtil.MakePathList(temp_locations.GetUsedLibraryCodes());

                        if (this.CheckDupItem == true)
                        {
                            if (this.SeriesMode == false)
                            {
                                // 对图书检查四元组
                                if (item.Seller == temp_item.Seller
                                    && item.Source == temp_item.Source
                                    && item.Price == temp_item.Price
                                    && strUsedLibraryCodes == strTempUsedLibraryCodes)
                                {
                                    strError = "第 " + (i + 1).ToString() + " 行 和 第 " + (j + 1) + " 行之间 渠道/经费来源/价格/馆藏分配去向(中所含的馆代码) 四元组重复，需要将它们合并为一行";
                                    return 1;
                                }
                            }
                            else
                            {
                                // 对期刊检查五元组
                                if (item.Seller == temp_item.Seller
                                    && item.Source == temp_item.Source
                                    && item.Price == temp_item.Price
                                    && item.RangeString == temp_item.RangeString
                                    && strUsedLibraryCodes == strTempUsedLibraryCodes)
                                {
                                    strError = "第 " + (i + 1).ToString() + " 行 和 第 " + (j + 1) + " 行之间 渠道/经费来源/时间范围/价格/馆藏分配去向(中所含的馆代码) 五元组重复，需要将它们合并为一行";
                                    return 1;
                                }
                            }
                        }

                    }
                }
            }

            return 0;
        }

        static bool IsChangedItem(Item item)
        {
            if ((item.State & ItemState.Changed) != 0
                || (item.State & ItemState.New) != 0)
                return true;
            return false;
        }

        // 获得总份数。包括了所有新规划的和已订购(已验收)的事项
        public int GetTotalCopy()
        {
            int value = 0;
            for (int i = 0; i < this.Items.Count; i++)
            {
                Item cur_element = this.Items[i];
                value += cur_element.CopyValue;
            }

            return value;
        }

        // 获得新规划的总份数。不包括已订购(已验收)的事项
        public int GetNewlyOrderTotalCopy()
        {
            Debug.Assert(this.ArriveMode == false, "本函数只能在订购状态下使用");
            int value = 0;
            for (int i = 0; i < this.Items.Count; i++)
            {
                Item cur_element = this.Items[i];

                if ((cur_element.State & ItemState.ReadOnly) != 0)
                    continue;

                value += cur_element.CopyValue;
            }

            return value;
        }

        // 获得新验收的总份数。不包括未订购的事项。不包括(显示为只读的)已验收事项
        public int GetNewlyArriveTotalCopy()
        {
            Debug.Assert(this.ArriveMode == true, "本函数只能在验收状态下使用");
            int value = 0;
            for (int i = 0; i < this.Items.Count; i++)
            {
                Item cur_element = this.Items[i];

                value += cur_element.location.ArrivedCount - cur_element.location.ReadOnlyArrivedCount;
            }

            return value;
        }

        // 检测是否有事项尚未订购(状态为空，表示刚刚输入了采购数据)
        // return:
        //      -1  error
        //      0   没有处于未订购状态的事项
        //      1   有部分处于未订购状态的事项
        //      2   全部事项都是未订购状态
        public int NotOrdering(out string strMessage)
        {
            strMessage = "";
            int nNotOrderItemCount = 0;
            for (int i = 0; i < this.Items.Count; i++)
            {
                Item cur_element = this.Items[i];

                if (String.IsNullOrEmpty(cur_element.StateString) == true)
                {
                    Debug.Assert(cur_element.location.ReadOnly == true, "");
                    nNotOrderItemCount++;
                }
            }

            if (nNotOrderItemCount == this.Items.Count)
            {
                strMessage = "全部 " + this.Items.Count.ToString() + " 个事项都处在未订购状态";
                return 2;
            }

            if (nNotOrderItemCount > 0)
            {
                strMessage = "全部 " + this.Items.Count + " 个事项中有 " + nNotOrderItemCount.ToString() + " 个事项处在未订购状态";
                return 1;
            }

            strMessage = "没有事项处在未订购状态";
            return 0;
        }

        // 获得可新验收的最大总份数。包含了本函数操作前已经新验收的份数。
        public int GetNewlyArrivingTotalCopy()
        {
            Debug.Assert(this.ArriveMode == true, "本函数只能在验收状态下使用");
            int value = 0;
            for (int i = 0; i < this.Items.Count; i++)
            {
                Item cur_element = this.Items[i];

                // 要看看item本身的状态是不是完全不允许验收 2008/11/12
                if (cur_element.location.ReadOnly == true)
                    continue;

                value += cur_element.location.Count - cur_element.location.ReadOnlyArrivedCount;
            }

            return value;
        }

        // 获得已订购(已验收)的总份数。不包括新规划的事项
        public int GetOrderedTotalCopy()
        {
            Debug.Assert(this.ArriveMode == false, "本函数只能在订购状态下使用");

            int value = 0;
            for (int i = 0; i < this.Items.Count; i++)
            {
                Item cur_element = this.Items[i];

                if ((cur_element.State & ItemState.ReadOnly) == 0)
                    continue;

                value += cur_element.CopyValue;
            }

            return value;
        }

        // 获得已验收的总份数。不包括新规划的事项
        public int GetArrivedTotalCopy()
        {
            Debug.Assert(this.ArriveMode == true, "本函数只能在验收状态下使用");
            int value = 0;
            for (int i = 0; i < this.Items.Count; i++)
            {
                Item cur_element = this.Items[i];

                value += cur_element.location.ReadOnlyArrivedCount;
                /*
                if ((cur_element.State & ItemState.ReadOnly) == 0)
                    continue;

                value += cur_element.CopyValue;
                 * */
            }

            return value;
        }

        public void SelectAll()
        {
            for (int i = 0; i < this.Items.Count; i++)
            {
                Item cur_element = this.Items[i];
                if ((cur_element.State & ItemState.Selected) == 0)
                    cur_element.State |= ItemState.Selected;
            }

            this.Invalidate();
        }

        public void SelectItem(Item element,
            bool bClearOld)
        {

            if (bClearOld == true)
            {
                for (int i = 0; i < this.Items.Count; i++)
                {
                    Item cur_element = this.Items[i];

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

        public void ToggleSelectItem(Item element)
        {
            // 选中当前行
            if ((element.State & ItemState.Selected) == 0)
                element.State |= ItemState.Selected;
            else
                element.State -= ItemState.Selected;

            this.InvalidateLine(element);

            this.LastClickItem = element;
        }

        public void RangeSelectItem(Item element)
        {
            Item start = this.LastClickItem;

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
                Item cur_element = this.Items[i];

                if ((cur_element.State & ItemState.Selected) == 0)
                {
                    cur_element.State |= ItemState.Selected;

                    this.InvalidateLine(cur_element);
                }
            }

            // 清除其余位置
            for (int i = 0; i < nStart; i++)
            {
                Item cur_element = this.Items[i];

                if ((cur_element.State & ItemState.Selected) != 0)
                {
                    cur_element.State -= ItemState.Selected;

                    this.InvalidateLine(cur_element);
                }
            }

            for (int i = nEnd + 1; i < this.Items.Count; i++)
            {
                Item cur_element = this.Items[i];

                if ((cur_element.State & ItemState.Selected) != 0)
                {
                    cur_element.State -= ItemState.Selected;

                    this.InvalidateLine(cur_element);
                }
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
            if (this.GetValueTable != null)
                this.GetValueTable(sender, e);
        }

        // 获得一个未曾使用的index字符串
        // paremeters:
        //      exclude 查阅过程中，排除此项。如果不需要本参数(即不排除任何事项)，用使用值null
        string GetNewIndex(Item exclude)
        {
            for (int j = 1; ; j++)
            {
                string strIndex = j.ToString();

                bool bFound = false;
                for (int i = 0; i < this.Items.Count; i++)
                {
                    Item item = this.Items[i];

                    if (item == exclude)
                        continue;

                    if (item.Index == strIndex)
                    {
                        bFound = true;
                        break;
                    }
                }

                if (bFound == false)
                    return strIndex;
            }
        }

        // 将全部行的状态恢复为普通状态
        // 不过，仍保留了Ordered状态
        void ResetLineState()
        {
            for (int i = 0; i < this.Items.Count; i++)
            {
                Item item = this.Items[i];

                item.location.Changed = false;  // 2014/8/29

                if ((item.State & ItemState.ReadOnly) != 0)
                    item.State = ItemState.Normal | ItemState.ReadOnly;
                else
                    item.State = ItemState.Normal;
            }

            this.Invalidate();
        }

        void RefreshLineColor()
        {
            for (int i = 0; i < this.Items.Count; i++)
            {
                Item item = this.Items[i];
                item.SetLineColor();
            }
        }

        // 新规划订购的总份数
        // 或者 新验收的总份数
        public int NewlyOrderTotalCopy
        {
            get
            {
                if (String.IsNullOrEmpty(this.textBox_newlyOrderTotalCopy.Text) == true)
                    return 0;

                return Convert.ToInt32(this.textBox_newlyOrderTotalCopy.Text);
            }
            set
            {
                this.textBox_newlyOrderTotalCopy.Text = value.ToString();
            }

        }

        void ClearItems()
        {
            if (this.Items != null)
            {
                foreach (Item item in this.Items)
                {
                    if (item != null)
                        item.Dispose();
                }
                this.Items.Clear();
            }
        }

        public void Clear()
        {
            this.DisableUpdate();

            try
            {
                for (int i = 0; i < this.Items.Count; i++)
                {
                    Item element = this.Items[i];
                    ClearOneItemControls(this.tableLayoutPanel_content,
                        element);
                }

                // this.Items.Clear();
                this.ClearItems();

                this.tableLayoutPanel_content.RowCount = 2;    // 为什么是2？
                for (; ; )
                {
                    if (this.tableLayoutPanel_content.RowStyles.Count <= 2)
                        break;
                    this.tableLayoutPanel_content.RowStyles.RemoveAt(2);
                }

                // 2008/12/30
                this.textBox_arrivedTotalCopy.Text = "";
                this.textBox_newlyArriveTotalCopy.Text = "";
                this.textBox_newlyOrderTotalCopy.Text = "";
                this.textBox_orderedTotalCopy.Text = "";
            }
            finally
            {
                this.EnableUpdate();
            }
        }


        // 清除一个Item对象对应的Control
        public void ClearOneItemControls(
            TableLayoutPanel table,
            Item line)
        {
            // color
            Label label = line.label_color;
            table.Controls.Remove(label);

            // catalog no
            table.Controls.Remove(line.textBox_catalogNo);

            // seller
            table.Controls.Remove(line.comboBox_seller);

            // source
            table.Controls.Remove(line.comboBox_source);

            // range
            table.Controls.Remove(line.dateRange_range);

            // issue count
            table.Controls.Remove(line.comboBox_issueCount);

            // copy
            table.Controls.Remove(line.comboBox_copy);

            // price
            table.Controls.Remove(line.textBox_price);

            // location
            table.Controls.Remove(line.location);

            // class
            table.Controls.Remove(line.comboBox_class);

            // seller address
            table.Controls.Remove(line.label_sellerAddress);

            // other
            table.Controls.Remove(line.label_other);
        }

        public List<Item> SelectedItems
        {
            get
            {
                List<Item> results = new List<Item>();

                for (int i = 0; i < this.Items.Count; i++)
                {
                    Item cur_element = this.Items[i];
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
                    Item cur_element = this.Items[i];
                    if ((cur_element.State & ItemState.Selected) != 0)
                        results.Add(i);
                }

                return results;
            }
        }

        public void DisableUpdate()
        {
            /*
            bool bOldVisible = this.Visible;

            this.Visible = false;

            return bOldVisible;
             * */

            if (this.m_nInSuspend == 0)
            {
                this.tableLayoutPanel_content.SuspendLayout();
                /*
                this.tableLayoutPanel_main.SuspendLayout();

                this.SuspendLayout();
                 * */
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

                /*
                this.tableLayoutPanel_main.ResumeLayout(false);
                this.tableLayoutPanel_main.PerformLayout();

                this.ResumeLayout(false);
                 * */
            }
        }

#if NOOOOOOOOOOOOOOOOOOOOO
        // 根据XML订购记录建立一个新的事项
        public Item AppendNewItem(string strDefaultRecord,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strDefaultRecord);
            }
            catch (Exception ex)
            {
                strError = "XML记录装入DOM时出错: " + ex.Message;
                return null;
            }

            Item item = AppendNewItem();

            item.Seller = DomUtil.GetElementText(dom.DocumentElement,
                "seller");
            item.Source = DomUtil.GetElementText(dom.DocumentElement,
                "source");
            try
            {
                item.Copy = Convert.ToInt32(DomUtil.GetElementText(dom.DocumentElement,
                    "copy"));
            }
            catch
            {
            }
            item.Price = DomUtil.GetElementText(dom.DocumentElement,
                "price");
            item.Distribute = DomUtil.GetElementText(dom.DocumentElement,
                "distribute");

            // 设置好 已订购 状态
            string strState = DomUtil.GetElementText(dom.DocumentElement,
                "state");
            if (strState == "已订购" || strState == "已验收")
                item.State |= ItemState.Ordered;

            try
            {
                item.OtherXml = strDefaultRecord;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return null;
            }


            return item;
        }
#endif

        // 根据XML订购记录建立一个新的事项
        public Item AppendNewItem(string strDefaultRecord,
            out string strError)
        {
            strError = "";

            Item item = AppendNewItem(false);

            int nRet = SetDefaultRecord(item,
                strDefaultRecord,
                false,  // 不修正index值，保持原来的值
                out strError);
            if (nRet == -1)
            {
                this.RemoveItem(item);
                return null;
            }

            // 2017/2/28
            if (string.IsNullOrEmpty(item.RefID))
                item.RefID = Guid.NewGuid().ToString();

            return item;
        }

        // 匹配书商名称
        static bool MatchSeller(string seller, string seller_filter)
        {
            return (StringUtil.IsInList(seller, seller_filter));
        }

        // 根据缺省XML订购记录填充必要的字段
        int SetDefaultRecord(Item item,
            string strDefaultRecord,
            bool bResetIndexValue,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strDefaultRecord) == true)
                return 0;

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strDefaultRecord);
            }
            catch (Exception ex)
            {
                strError = "XML记录装入DOM时出错: " + ex.Message;
                return -1;
            }

            // catalog no
            item.CatalogNo = DomUtil.GetElementText(dom.DocumentElement,
                "catalogNo");

            // seller
            item.Seller = DomUtil.GetElementText(dom.DocumentElement,
                "seller");

            // range
            try
            {
                item.RangeString = DomUtil.GetElementText(dom.DocumentElement,
                    "range");
            }
            catch (Exception ex)
            {
                // 2008/12/18
                strError = ExceptionUtil.GetAutoText(ex);
                return -1;
            }

            item.IssueCountString = DomUtil.GetElementText(dom.DocumentElement,
                "issueCount");

            // source
            string strSource = DomUtil.GetElementText(dom.DocumentElement,
                "source");

            string strNewSource = "";
            string strOldSource = "";
            dp2StringUtil.ParseOldNewValue(strSource,
                out strOldSource,
                out strNewSource);

            if (String.IsNullOrEmpty(strNewSource) == true) // 没有新值的时候用旧值作为初始值
                item.Source = strOldSource;
            else
                item.Source = strNewSource;

            item.OldSource = strOldSource;

            // distribute string
            // 注意：必须在copy前设置，因为copy string中可能包含勾选location item的信息，如果copy string先设置，勾选好的状态会被后来重设distribute string而冲掉
            string strDistribute = DomUtil.GetElementText(dom.DocumentElement,
                "distribute");

            item.Distribute = strDistribute;

            {
                // copy
                // 注：copy值是按照XML记录来设置的。一般设置为可能的最大值
                string strCopy = DomUtil.GetElementText(dom.DocumentElement,
                        "copy");

                string strNewCopy = "";
                string strOldCopy = "";
                dp2StringUtil.ParseOldNewValue(strCopy,
                    out strOldCopy,
                    out strNewCopy);

                /*
                if (String.IsNullOrEmpty(strNewCopy) == true) // 没有新值的时候用旧值作为初始值
                    item.CopyString = strOldCopy;
                else
                    item.CopyString = strNewCopy;

                item.OldCopyString = strOldCopy;
                 * */

                // 2008/11/3 changed
                if (this.ArriveMode == false)
                {
                    // 订购时，用旧价格
                    item.CopyString = strOldCopy;
                    item.OldCopyString = strOldCopy;
                }
                else
                {
                    // 2008/10/19 changed
                    // 验收时，新旧价格都分明
                    if (String.IsNullOrEmpty(strNewCopy) == false)
                        item.CopyString = strNewCopy;

                    if (String.IsNullOrEmpty(strOldCopy) == false)
                        item.OldCopyString = strOldCopy;
                }
            }

            // 限制馆藏地点事项的个数
            int nMaxCopyValue = Math.Max(item.CopyValue, item.OldCopyValue);

            if (nMaxCopyValue < item.DistributeCount)
                item.DistributeCount = nMaxCopyValue;

            /*
            // 限制馆藏地点事项的个数
            try
            {
                strDistribute = LocationEditControl.CanonicalizeDistributeString(
                    strDistribute,
                    item.CopyValue);
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }*/

            {
                // fixedPrice
                string strFixedPrice = DomUtil.GetElementText(dom.DocumentElement,
                    "fixedPrice");

                dp2StringUtil.ParseOldNewValue(strFixedPrice,
                    out string strOldPrice,
                    out string strNewPrice);

                if (String.IsNullOrEmpty(strNewPrice) == true) // 没有新值的时候用旧值作为初始值
                    item.FixedPrice = strOldPrice;
                else
                    item.FixedPrice = strNewPrice;

                item.OldFixedPrice = strOldPrice;
            }

            {
                // discount
                string strDiscount = DomUtil.GetElementText(dom.DocumentElement,
                        "discount");

                dp2StringUtil.ParseOldNewValue(strDiscount,
                    out string strOldValue,
                    out string strNewValue);

                if (this.ArriveMode == false)
                {
                    // 订购时，用旧折扣
                    item.Discount = strOldValue;
                    item.OldDiscount = strOldValue;
                }
                else
                {
                    // 没有新值的时候用旧值作为初始值。这样可以避免无谓的输入
                    if (String.IsNullOrEmpty(strNewValue) == true)
                        item.Discount = strOldValue;
                    else
                        item.Discount = strNewValue;

#if NO
                    // 验收时，新旧折扣都分明
                    if (String.IsNullOrEmpty(strNewValue) == false)
                        item.Discount = strNewValue;
#endif

                    if (String.IsNullOrEmpty(strOldValue) == false)
                        item.OldDiscount = strOldValue;
                }
            }


            {
                // price
                string strPrice = DomUtil.GetElementText(dom.DocumentElement,
                    "price");

                dp2StringUtil.ParseOldNewValue(strPrice,
                    out string strOldPrice,
                    out string strNewPrice);

                if (String.IsNullOrEmpty(strNewPrice) == true) // 没有新值的时候用旧值作为初始值
                    item.Price = strOldPrice;
                else
                    item.Price = strNewPrice;

                item.OldPrice = strOldPrice;
            }

            // class
            item.Class = DomUtil.GetElementText(dom.DocumentElement,
                "class");

            // 设置好 已订购 状态
            string strState = DomUtil.GetElementText(dom.DocumentElement,
                "state");

            item.StateComment = "";

            List<string> comments = new List<string>();

            string strControlled = IsDistributeControlled(item.Distribute);
            if (string.IsNullOrEmpty(strControlled) == false)
            {
                item.State |= ItemState.ReadOnly;
                comments.Add(strControlled);
            }

            if (this.ArriveMode == false)
            {
                // *** 订购态

                // 注：只有未订购的、草稿状态的事项才允许修改订购
                // 已订购(必然是全部发出，不可能局部发出)，或者已验收(有可能是局部验收，或者(虽然已验收完但是)潜在可以追加)，这样的事项都不能再进行任何订购操作，所以为readonly

                if (strState == "已订购" || strState == "已验收")
                {
                    item.State |= ItemState.ReadOnly;
                    comments.Add("状态 '" + strState + "' 不允许进行订购修改操作");
                }
            }
            else
            {
                // *** 验收态

                // 检查书商
                bool seller_matched = true;
                if (string.IsNullOrEmpty(this._sellerFilter) == false && this._sellerFilter != "<不过滤>")
                {
                    if (MatchSeller(item.Seller, this._sellerFilter) == false)
                        seller_matched = false;
                }

                if (seller_matched == false)
                {
                    item.State |= ItemState.ReadOnly;
                    // TODO: tips 提示只读状态的原因
                    comments.Add("渠道 '" + item.Seller + "' 不符合渠道过滤器 '" + this._sellerFilter + "'");
                }
                else
                {
                    // 验收态
                    if (strState == "已订购" || strState == "已验收")
                    {
                        // 注：状态为“已验收”时，不一定全部复本都已验收，所以这时应当允许再次验收。
                        // 即便所有复本都已验收，还可以追加、多验收复本，所以这样的事项不能readonly

                        // item.State -= ItemState.ReadOnly;

                        // 将location item中已经勾选的事项设置为readonly态，表示是已经验收的(馆藏地点、册)事项
                        item.location.SetAlreadyCheckedToReadOnly(false);
                    }
                    else
                    {
                        // 一般而言可能出现了空白的状态值，这表明尚未订出，还属于草稿记录，自然也就无从验收了

                        item.State |= ItemState.ReadOnly;
                        comments.Add("状态 '" + strState + "' 不允许进行验收操作");
                    }
                }
            }

            if (comments.Count > 0)
                item.StateComment = StringUtil.MakePathList(comments, "; ");

            // 2009/2/13
            try
            {
                item.SellerAddressXml = DomUtil.GetElementOuterXml(dom.DocumentElement, "sellerAddress");
            }
            catch (Exception ex)
            {
                strError = "设置SellerAddressXml时发生错误: " + ex.Message;
                return -1;
            }

            try
            {
                item.OtherXml = strDefaultRecord;
            }
            catch (Exception ex)
            {
                strError = "设置OtherXml时发生错误: " + ex.Message;
                return -1;
            }

            if (bResetIndexValue == true)
            {
                // 修正index
                item.Index = GetNewIndex(item);
            }

            return 0;
        }

        // 检查馆代码是否在管辖范围内
        string IsDistributeControlled(string strDistribute)
        {
            LocationCollection locations = new LocationCollection();
            int nRet = locations.Build(strDistribute, out string strError);
            if (nRet == -1)
            {
                return "馆藏分配字符串 '" + strDistribute + "' 格式不正确: " + strError;
            }

            List<string> codes = new List<string>();
            foreach (Location location in locations)
            {
                // 空的馆藏地点被视为不在分馆用户管辖范围内
                if (string.IsNullOrEmpty(location.Name) == true)
                {
                    codes.Add("");
                    continue;
                }

                // 解析
                dp2StringUtil.ParseCalendarName(location.Name,
            out string strLibraryCode,
            out string strPureName);

                codes.Add(strLibraryCode);
            }

            StringUtil.RemoveDupNoSort(ref codes);

            // 检查馆代码是否在管辖范围内
            if (this.VerifyLibraryCode != null)
            {
                VerifyLibraryCodeEventArgs e = new VerifyLibraryCodeEventArgs();
                e.LibraryCode = StringUtil.MakePathList(codes);
                this.VerifyLibraryCode(this, e);
                if (string.IsNullOrEmpty(e.ErrorInfo) == false)
                    return "馆藏分配去向不完全受当前用户管辖。详情: " + e.ErrorInfo;
            }

            return null;
        }

        // 整理一下，在已经有0份以外事项的前提下，清除多余的份数为0的事项
        public void RemoveMultipleZeroCopyItem()
        {
            int nTotalCopies = 0;
            for (int i = 0; i < this.Items.Count; i++)
            {
                Item item = this.Items[i];
                nTotalCopies += item.CopyValue;
            }

            // 2008/8/27
            // ruguo you duoyu yige de 0 shixiang
            if (nTotalCopies == 0 && this.Items.Count > 1)
            {
                for (int i = 1; i < this.Items.Count; i++)
                {
                    Item item = this.Items[i];
                    this.RemoveItem(i);
                    i--;
                }

            }

            if (nTotalCopies > 0)
            {
                for (int i = 0; i < this.Items.Count; i++)
                {
                    Item item = this.Items[i];
                    if (item.CopyValue == 0)
                    {
                        this.RemoveItem(i);
                        i--;
                    }
                }
            }
        }

        // 获得缺省记录
        internal string GetDefaultXml()
        {
            if (this.GetDefaultRecord != null)
            {
                GetDefaultRecordEventArgs e = new GetDefaultRecordEventArgs();
                this.GetDefaultRecord(this, e);

                return e.Xml;
            }
            return "";
        }

        public Item AppendNewItem(bool bSetDefaultRecord)
        {
            this.DisableUpdate();   // 防止闪动。彻底解决问题。2009/10/13 

            try
            {
                this.tableLayoutPanel_content.RowCount += 1;
                this.tableLayoutPanel_content.RowStyles.Add(new System.Windows.Forms.RowStyle());

                Item item = new Item(this);

                item.AddToTable(this.tableLayoutPanel_content, this.Items.Count + 1);

                this.Items.Add(item);

                if (bSetDefaultRecord == true)
                {
                    string strDefaultRecord = GetDefaultXml();

                    if (String.IsNullOrEmpty(strDefaultRecord) == false)
                    {
                        string strError = "";
                        // 根据缺省XML订购记录填充必要的字段
                        int nRet = SetDefaultRecord(item,
                            strDefaultRecord,
                            true,
                            out strError);
                        if (nRet == -1)
                            throw new Exception(strError);
                    }
                }

                // 2017/2/28
                if (string.IsNullOrEmpty(item.RefID))
                    item.RefID = Guid.NewGuid().ToString();

                item.State = ItemState.New;
                return item;
            }
            finally
            {
                this.EnableUpdate();
            }
        }

        public Item InsertNewItem(int index, string strDefaultRecord = "")
        {
            this.DisableUpdate();   // 防止闪动。彻底解决问题。2009/10/13 
            try
            {
                this.tableLayoutPanel_content.RowCount += 1;
                this.tableLayoutPanel_content.RowStyles.Insert(index + 1, new System.Windows.Forms.RowStyle());

                Item item = new Item(this);

                item.InsertToTable(this.tableLayoutPanel_content, index);

                this.Items.Insert(index, item);

                if (string.IsNullOrEmpty(strDefaultRecord) == true)
                    strDefaultRecord = this.GetDefaultXml();

                if (String.IsNullOrEmpty(strDefaultRecord) == false)
                {
                    string strError = "";
                    // 根据缺省XML订购记录填充必要的字段
                    int nRet = SetDefaultRecord(item,
                        strDefaultRecord,
                        true,
                        out strError);
                    if (nRet == -1)
                        throw new Exception("装载订购记录缺省值时出错: " + strError);
                }

                // 2017/2/28
                if (string.IsNullOrEmpty(item.RefID))
                    item.RefID = Guid.NewGuid().ToString();

                item.State = ItemState.New;
                return item;
            }
            finally
            {
                this.EnableUpdate();
            }
        }

        public void RemoveItem(int index)
        {
            Item line = this.Items[index];

            line.RemoveFromTable(this.tableLayoutPanel_content, index);

            this.Items.Remove(line);

            /*
            if (this.LastClickItem == line)
                this.LastClickItem = null;
             * */

            this.Changed = true;
        }

        public void RemoveItem(Item line)
        {
            int index = this.Items.IndexOf(line);

            if (index == -1)
                return;

            line.RemoveFromTable(this.tableLayoutPanel_content, index);

            this.Items.Remove(line);

            /*
            if (this.LastClickItem == line)
                this.LastClickItem = null;
             * */

            this.Changed = true;
        }

        /*
        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {

        }

        private void numericUpDown1_KeyPress(object sender, KeyPressEventArgs e)
        {
            API.PostMessage(this.Handle, WM_NUMBER_CHANGED, 0, 0);
        }

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            switch (m.Msg)
            {
                case WM_NUMBER_CHANGED:
                    {
                        numericUpDown1_ValueChanged(null, null);
                    }
                    return;
            }
            base.DefWndProc(ref m);
        }
         * */

#if NOOOOOOOOOOOOOOOOOOOOOOOO
        private void textBox_totalCopy_TextChanged(object sender, EventArgs e)
        {
            // 不要响应事件
            if (this.DisableTextChanged > 0)
                return;

            if (this.textBox_totalCopy.Text == "")
                return;

            Item item = null;


            int nValue = 0;

            try
            {
                nValue = Convert.ToInt32(this.textBox_totalCopy.Text);
            }
            catch
            {
                MessageBox.Show(this, "总册数 '" + this.textBox_totalCopy.Text + "' 应当为纯数字";
                return;
            }

            // 如果列表中一个事项也没有，则新增一个事项
            if (this.Items.Count == 0)
            {
                item = AppendNewItem();
                item.Copy = nValue;
                return;
            }

            if (this.Items.Count == 1)
            {
                item = this.Items[0];
                item.Copy = nValue;
                return;
            }


            int nCurrent = 0;   // 当前台阶
            for (int i = 0; i < this.Items.Count; i++)
            {
                item = this.Items[i];

                if (nValue >= nCurrent
                    && nValue < nCurrent + item.Copy)
                {
                    // 落入一个item的范围
                    item.Copy = nValue - nCurrent;

                    // this.Items.RemoveRange(i + 1, this.Items.Count - i - 1);
                    for (int j = i + 1; j < this.Items.Count; j++)
                    {
                        this.RemoveItem(i + 1);
                    }
                    return;
                }

                nCurrent += item.Copy;
            }

            // 修改最后一项
            item.Copy = nValue - nCurrent;
        }
#endif

        // 计算出首个被允许的值。在遇到(因“已订购”状态而起的)非法值的时候使用。
        int GetFirstValidCopyValue()
        {
            int nValue = 0;
            for (int i = 0; i < this.Items.Count; i++)
            {
                Item item = this.Items[i];
                if ((item.State & ItemState.ReadOnly) == 0)
                    continue;
                nValue += item.CopyValue;
            }

            return nValue;
        }

        // 新订购总册数 textbox 值改变
        private void textBox_newlyOrderTotalCopy_TextChanged(object sender, EventArgs e)
        {
            // 如果当前为验收模式，则不响应
            if (this.ArriveMode == true)
                return;

            // 不要响应事件
            if (this.DisableNewlyOrderTextChanged > 0)
                return;

            if (this.textBox_newlyOrderTotalCopy.Text == "")
                return;

            this.DisableUpdate();

            try
            {

                Item item = null;


                int nValue = 0;

                try
                {
                    nValue = Convert.ToInt32(this.textBox_newlyOrderTotalCopy.Text);
                }
                catch
                {
                    MessageBox.Show(this, "新规划总册数 '" + this.textBox_newlyOrderTotalCopy.Text + "' 应当为纯数字");

                    // 2008/9/16
                    this.textBox_newlyOrderTotalCopy.Text = this.GetNewlyOrderTotalCopy().ToString();  // 改变回可行的值
                    return;
                }

                // 如果列表中一个事项也没有，则新增一个事项
                if (this.Items.Count == 0)
                {
                    item = AppendNewItem(true);
                    item.CopyValue = nValue;
                    return;
                }


                Item lastChangeableItem = null; // 遍历中发现的最后一个非“已订购状态”的事项。

                int nCurrent = 0;   // 当前台阶
                for (int i = 0; i < this.Items.Count; i++)
                {
                    item = this.Items[i];

                    // 跳过已订购事项
                    if ((item.State & ItemState.ReadOnly) != 0)
                        continue;

                    lastChangeableItem = item;

                    if (nValue >= nCurrent
                        && nValue < nCurrent + item.CopyValue)
                    {
                        // 落入一个item的范围
                        item.CopyValue = nValue - nCurrent;

                        // 删除这个item后面的所有非已订购状态的事项
                        for (int j = i + 1; j < this.Items.Count; j++)
                        {
                            Item temp = this.Items[j];
                            // 跳过已订购事项
                            if ((temp.State & ItemState.ReadOnly) != 0)
                                continue;

                            this.RemoveItem(i + 1);
                        }
                        return;
                    }

                    nCurrent += item.CopyValue;
                }
                // 循环结束后，item中保留了遍历所遇到的最后一个事项。
                // lastChangeableItem中则为最后一个非“已订购”事项。

                if (nValue - nCurrent == 0)
                    return; // 没有必要修改什么

                // 如果存在最后一个可改变事项
                if (lastChangeableItem != null)
                {
                    lastChangeableItem.CopyValue += nValue - nCurrent;
                    return;
                }

                /*
                // 修改最后一项，如果最后一项不是已订购事项
                if ((item.State & ItemState.Ordered) == 0)
                {
                    item.Copy += nValue - nCurrent;
                    return;
                }*/

                Debug.Assert(nValue > nCurrent, "");

                // 否则要在最后增加一个新事项
                item = AppendNewItem(true);
                item.CopyValue = nValue - nCurrent;
                return;

            }
            finally
            {
                this.EnableUpdate();
            }

            /*
            ERROR1:
            MessageBox.Show(this, strError);
             * */
        }


        private void textBox_newlyOrderTotalCopy_Validating(object sender, CancelEventArgs e)
        {
            if (this.textBox_newlyOrderTotalCopy.Text == "")
                return;

            try
            {
                int nValue = Convert.ToInt32(this.textBox_newlyOrderTotalCopy.Text);
            }
            catch
            {
                MessageBox.Show(this, "所输入的数字 '" + this.textBox_newlyOrderTotalCopy.Text + "' 格式不正确");
                e.Cancel = true;
                return;
            }
        }

        private void label_topleft_MouseUp(object sender, MouseEventArgs e)
        {

        }

        private void OrderCrossControl_Enter(object sender, EventArgs e)
        {
            this.m_bFocused = true;
            this.RefreshLineColor();

        }

        private void OrderCrossControl_Leave(object sender, EventArgs e)
        {
            this.m_bFocused = false;
            this.RefreshLineColor();

        }

        // 在末尾追加一个新的事项
        private void button_newItem_Click(object sender, EventArgs e)
        {
            int nPos = this.Items.Count;

            this.InsertNewItem(nPos).EnsureVisible();
        }

        // 最后最靠后的一个未被完全封锁的、可改变copy值的事项
        Item GetLastChangeableItem()
        {
            for (int i = this.Items.Count - 1; i >= 0; i--)
            {
                Item item = this.Items[i];

                // 跳过(整个)已标记为只读的封锁的事项
                if ((item.State & ItemState.ReadOnly) != 0)
                    continue;

                return item;
            }

            return null;
        }

        // 新验收总册数 textbox 值改变
        private void textBox_newlyArriveTotalCopy_TextChanged(object sender, EventArgs e)
        {
            // 如果当前为订购模式，则不响应
            if (this.ArriveMode == false)
                return;

            // 不要响应事件
            if (this.DisableNewlyArriveTextChanged > 0)
                return;

            if (string.IsNullOrEmpty(this.textBox_newlyArriveTotalCopy.Text))
                return;

            /*
             * 算法为：遍历所有已验收事项，测算它们的馆藏事项中打勾的有多少。
             * 如果打勾的不足，则在适当位置增加打勾。如果打勾的太多，则off后方的多余打勾事项。
             * 如果整体馆藏事项不足，也就是即便全部事项打勾也不到要求的数目，则考虑增加订购事项，增补出够用的馆藏事项
             * */

            Item item = null;

            int nValue = 0;

            try
            {
                nValue = Convert.ToInt32(this.textBox_newlyArriveTotalCopy.Text);
            }
            catch
            {
                MessageBox.Show(this, "新验收总册数 '" + this.textBox_newlyArriveTotalCopy.Text + "' 应当为纯数字");

                // 2008/9/16
                this.textBox_newlyArriveTotalCopy.Text = GetNewlyArriveTotalCopy().ToString();  // 改变回可行的值
                return;
            }

            // 如果列表中一个事项也没有，则新增一个事项
            if (this.Items.Count == 0)
            {
                if (nValue == 0)
                    return;

                // 警告太大的值
                if (nValue > 10)
                {
                    DialogResult result = MessageBox.Show(this,
                        "确实要设置 " + nValue.ToString() + " 这么大的值?",
                        "OrderDesignControl",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result != DialogResult.Yes)
                    {
                        this.textBox_newlyArriveTotalCopy.Text = GetNewlyArriveTotalCopy().ToString();  // 改变回可行的值
                        return;
                    }
                }

                item = AppendNewItem(true);
                item.CopyValue = nValue;
                return;
            }

            // TODO: 应先计算出delta值，然后遍历每个item时，一个Item内添一点(在CopyValue和OldCopyValue之间)

            // 计算已有的arrived count(不包括readonly checked)
            int nNewlyArrivedCount = GetNewlyArriveTotalCopy();

            /*
            for (int i = 0; i < this.Items.Count; i++)
            {
                item = this.Items[i];

                nNewlyArrivedCount += item.location.ArrivedCount - item.location.ReadOnlyArrivedCount;
            }*/

            // 
            int nDelta = nValue - nNewlyArrivedCount;

            if (nDelta == 0)
                return; // 既没有必要增，也没有必要减

            // 增需要从前方开始进行。
            if (nDelta > 0)
            {
                // 警告太大的值
                if (nDelta > 10)
                {
                    DialogResult result = MessageBox.Show(ForegroundWindow.Instance,
                        "确实要增大到 " + nValue.ToString() + " 这么大的值?",
                        "OrderDesignControl",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Question,
                        MessageBoxDefaultButton.Button2);
                    if (result != DialogResult.Yes)
                    {
                        this.textBox_newlyArriveTotalCopy.Text = GetNewlyArriveTotalCopy().ToString();  // 改变回可行的值
                        return;
                    }
                }

                for (int i = 0; i < this.Items.Count; i++)
                {
                    item = this.Items[i];

                    int nCheckable = item.location.Count - item.location.ArrivedCount;
                    if (nCheckable > 0)
                    {
                        int nThisCount = Math.Min(nDelta, nCheckable);
                        item.location.ArrivedCount += nThisCount;
                        item.UpdateCopyCount(); // 2008/12/18
                    }

                    nDelta -= nCheckable;
                    if (nDelta <= 0)
                        break;
                }

                if (nDelta > 0)
                {
                    // 在已有事项的允许范围内，还没有增足够，需要继续增最后一项
                    item = GetLastChangeableItem();
                    if (item == null)
                    {
                        MessageBox.Show(this, "没有可改变的事项");
                        textBox_newlyArriveTotalCopy.Text = (nValue - nDelta).ToString();   // 修改到一个保守值
                        return;
                    }
                    item.location.ArrivedCount += nDelta;
                    item.UpdateCopyCount(); // 2008/12/18
                }

                return;
            }

            // 减需要从后方开始进行。
            if (nDelta < 0)
            {
                nDelta *= -1;   // 变为正数

                for (int i = this.Items.Count - 1; i >= 0; i--)
                {
                    item = this.Items[i];

                    int nUnCheckable = item.location.ArrivedCount - item.location.ReadOnlyArrivedCount;
                    if (nUnCheckable > 0)
                    {
                        int nThisCount = Math.Min(nDelta, nUnCheckable);
                        item.location.ArrivedCount -= nThisCount;   // 会自动删除一些空白馆藏地点的事项
                        item.UpdateCopyCount(); // 2008/12/18
                    }

                    nDelta -= nUnCheckable;
                    if (nDelta <= 0)
                        break;
                }

                if (nDelta > 0)
                {
                    MessageBox.Show(this, "无法减小到 " + nValue.ToString());
                    textBox_newlyArriveTotalCopy.Text = (nValue + nDelta).ToString();   // 修改到一个保守值
                    return;
                }

                return;
            }
        }

        // 把剩下的余额全部验收
        private void button_fullyAccept_Click(object sender, EventArgs e)
        {
            // 获得可新验收的最大总份数。包含了本函数操作前已经新验收的份数。
            int nValue = GetNewlyArrivingTotalCopy();
            if (nValue == 0)
            {
                // 检测是否有事项尚未订购(状态为空，表示刚刚输入了采购数据)
                // return:
                //      -1  error
                //      0   没有处于未订购状态的事项
                //      1   有部分处于未订购状态的事项
                //      2   全部事项都是未订购状态
                int nRet = NotOrdering(out string strMessage);
                if (nRet == 2)
                    MessageBox.Show(ForegroundWindow.Instance, "事项尚未经过打印订单环节，无法进行验收");
                else
                    MessageBox.Show(ForegroundWindow.Instance, "已经收满");
                return;
            }

            // string strRightCopy = OrderDesignControl.GetRightFromCopyString(this.comboBox_copy.Text);

            string strOldValue = this.textBox_newlyArriveTotalCopy.Text;

            try
            {
                this.textBox_newlyArriveTotalCopy.Text = nValue.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ExceptionUtil.GetAutoText(ex));
                this.textBox_newlyArriveTotalCopy.Text = strOldValue;
            }
        }

        internal void InvalidateLine(Item item)
        {

            Point p = this.tableLayoutPanel_content.PointToScreen(new Point(0, 0));

            Rectangle rect = item.label_color.RectangleToScreen(item.label_color.ClientRectangle);
            rect.Width = this.tableLayoutPanel_content.DisplayRectangle.Width;
            rect.Offset(-p.X, -p.Y);
            rect.Height = (int)this.Font.GetHeight() + 8;   // 缩小刷新高度

            this.tableLayoutPanel_content.Invalidate(rect, false);

            // this.tableLayoutPanel_content.Invalidate();
        }


        private void tableLayoutPanel_content_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            using (Brush brushText = new SolidBrush(Color.Black))
            using (Pen pen = new Pen(Color.Red))
            {
                Point p = this.tableLayoutPanel_content.PointToScreen(new Point(0, 0));
                // Debug.WriteLine("p x=" + p.X.ToString() + " y=" + p.Y.ToString());

                // int[] row_heights = this.tableLayoutPanel_content.GetRowHeights();
                int[] column_widths = this.tableLayoutPanel_content.GetColumnWidths();
                // Debug.WriteLine("height count=" + row_heights.Length.ToString() + " width count=" + column_widths.Length.ToString());

                Font font = null;
                List<string> column_titles = new List<string>();
                for (int j = 0; j < this.tableLayoutPanel_content.ColumnCount; j++)
                {
                    Control control = this.tableLayoutPanel_content.GetControlFromPosition(j, 0);
                    if (control != null)
                    {
                        column_titles.Add(control.Text);
                        if (font == null)
                            font = control.Font;
                    }
                    else
                        column_titles.Add("");
                }


                // float y = row_heights[0];   // +this.AutoScrollPosition.Y + this.tableLayoutPanel_content.Location.Y;
                for (int i = 0; i < this.Items.Count; i++)
                {
                    Item item = this.Items[i];

                    if ((item.State & ItemState.Selected) == 0
                        || i == 0)
                        continue;

                    // int height = row_heights[i + 1];

                    Rectangle rect = item.label_color.RectangleToScreen(item.label_color.ClientRectangle);
                    rect.Width = this.tableLayoutPanel_content.DisplayRectangle.Width;
                    rect.Offset(-p.X, -p.Y);
                    rect.Height = (int)this.Font.GetHeight() + 8;

                    using (LinearGradientBrush brushGradient = new LinearGradientBrush(
    new PointF(rect.X, rect.Y),
    new PointF(rect.X, rect.Y + rect.Height),
    Color.FromArgb(10, Color.Gray),
    Color.FromArgb(50, Color.Gray)
    ))
                    {
                        e.Graphics.FillRectangle(brushGradient, rect);
                    }

                    // 一行中每个格子
                    float x = rect.X;    //  this.AutoScrollPosition.X + this.tableLayoutPanel_content.Location.X;
                    for (int j = 0; j < column_widths.Length; j++)
                    {
                        float fWidth = column_widths[j];

                        string strTitle = column_titles[j];
                        // Debug.WriteLine("x=" + x.ToString() + " y=" + y.ToString());

                        if (fWidth > 0 && string.IsNullOrEmpty(strTitle) == false)
                        {
                            e.Graphics.DrawString(
                            strTitle,
                            font,
                            brushText,
                            x + 6,
                            rect.Y + 4);
                        }
                        x += fWidth;
                    }
                    // y += height;
                }
            }
        }

        private void tableLayoutPanel_content_CellPaint(object sender, TableLayoutCellPaintEventArgs e)
        {
            if (this.m_nInSuspend > 0)
                return; // 防止闪动

            // Rectangle rect = Rectangle.Inflate(e.CellBounds, -1, -1);
            Rectangle rect = e.CellBounds;
            using (Pen pen = new Pen(Color.FromArgb(200, 200, 200)))
            {
                e.Graphics.DrawRectangle(pen, rect);
            }
        }

        // 比较新旧值对
        // parameters:
        //      strVerifyStyle 比较风格。old/new/both 只比较旧/只比较新/新旧都比较
        // return:
        //      -2  码洋和订购价货币单位不同，无法进行校验。
        //      -1  校验过程出错
        //      0   校验发现三者关系不正确
        //      1   校验三者关系正确
        public static int VerifyOrderPriceByFixedPricePair(
            string strFixedPrice,
    string strDiscount,
    string strOrderPrice,
    string strVerifyStyle,
    out string strError)
        {
            strError = "";

            OldNewValue fixedPrice = OldNewValue.Parse(strFixedPrice);
            OldNewValue orderPrice = OldNewValue.Parse(strOrderPrice);

            if (orderPrice.IsVirtual)
            {
                strError = "单价字符串 '" + strOrderPrice + "' 中不应该包含花括号";
                return -1;
            }

            OldNewValue discount = OldNewValue.Parse(strDiscount);

            if (discount.IsVirtual)
            {
                strError = "折扣字符串 '" + strDiscount + "' 中不应该包含花括号";
                return -1;
            }
#if NO
            dp2StringUtil.ParseOldNewValue(strFixedPrice,
    out string strOldFixedPrice,
    out string strNewFixedPrice);

            dp2StringUtil.ParseOldNewValue(strDiscount,
out string strOldDiscount,
out string strNewDiscount);

            dp2StringUtil.ParseOldNewValue(strOrderPrice,
out string strOldOrderPrice,
out string strNewOrderPrice);
#endif

            if (string.IsNullOrEmpty(fixedPrice.OldValue) == false)
            {
                // return:
                //      -2  码洋和订购价货币单位不同，无法进行校验。TODO: 今后可增加汇率表，让这种情况变得可以校验
                //      -1  校验过程出错
                //      0   校验发现三者关系不正确
                //      1   校验三者关系正确
                int nRet = VerifyOrderPriceByFixedPrice(fixedPrice.OldValue,
        discount.OldValue,
        orderPrice.OldValue,
        out string strWishOldFixedPrice,
        out strError);
                if (nRet != 1)
                    return nRet;
            }

            if (string.IsNullOrEmpty(fixedPrice.NewValue) == false)
            {
                // return:
                //      -2  码洋和订购价货币单位不同，无法进行校验。TODO: 今后可增加汇率表，让这种情况变得可以校验
                //      -1  校验过程出错
                //      0   校验发现三者关系不正确
                //      1   校验三者关系正确
                int nRet = VerifyOrderPriceByFixedPrice(fixedPrice.NewValue,
        discount.NewValue,
        orderPrice.NewValue,
        out string strWishNewFixedPrice,
        out strError);
                if (nRet != 1)
                    return nRet;
            }

            return 1;
        }

#if NO
        public static int VerifyOrderPriceByFixedPricePair(
            string strFixedPrice,
    string strDiscount,
    string strOrderPrice,
    string strVerifyStyle,
    out string strError)
        {
            strError = "";

            dp2StringUtil.ParseOldNewValue(strFixedPrice,
    out string strOldFixedPrice,
    out string strNewFixedPrice);

            dp2StringUtil.ParseOldNewValue(strDiscount,
out string strOldDiscount,
out string strNewDiscount);

            dp2StringUtil.ParseOldNewValue(strOrderPrice,
out string strOldOrderPrice,
out string strNewOrderPrice);

            if (string.IsNullOrEmpty(strOldFixedPrice) == false)
            {
                // return:
                //      -2  码洋和订购价货币单位不同，无法进行校验。TODO: 今后可增加汇率表，让这种情况变得可以校验
                //      -1  校验过程出错
                //      0   校验发现三者关系不正确
                //      1   校验三者关系正确
                int nRet = VerifyOrderPriceByFixedPrice(strOldFixedPrice,
        strOldDiscount,
        strOldOrderPrice,
        out string strWishOldFixedPrice,
        out strError);
                if (nRet != 1)
                    return nRet;
            }

            if (string.IsNullOrEmpty(strNewFixedPrice) == false)
            {
                // return:
                //      -2  码洋和订购价货币单位不同，无法进行校验。TODO: 今后可增加汇率表，让这种情况变得可以校验
                //      -1  校验过程出错
                //      0   校验发现三者关系不正确
                //      1   校验三者关系正确
                int nRet = VerifyOrderPriceByFixedPrice(strNewFixedPrice,
        strNewDiscount,
        strNewOrderPrice,
        out string strWishNewFixedPrice,
        out strError);
                if (nRet != 1)
                    return nRet;
            }

            return 1;
        }
#endif


        // 根据码洋和折扣值校验订购价的正确性
        // 注意码洋、折扣、订购价字符串里面都必须是单个金额字符串，不支持带有方括号的(新旧)复合形式
        // parameters:
        //      strFixedPrice   码洋
        //      strDiscount     折扣
        //      strOrderPrice   订购价
        // return:
        //      -2  码洋和订购价货币单位不同，无法进行校验。TODO: 今后可增加汇率表，让这种情况变得可以校验
        //      -1  校验过程出错
        //      0   校验发现三者关系不正确
        //      1   校验三者关系正确
        public static int VerifyOrderPriceByFixedPrice(string strFixedPrice,
            string strDiscount,
            string strOrderPrice,
            out string strWishOrderPrice,
            out string strError)
        {
            strError = "";

            // return:
            //      -1  计算过程出现错误
            //      0   strFixedPrice 为空，无法计算
            //      1   计算成功
            int nRet = ComputeOrderPriceByFixedPrice(strFixedPrice,
        strDiscount,
        out strWishOrderPrice,
        out strError);
            if (nRet == -1)
                return -1;
            if (nRet == 0)
                return 1;

            try
            {
                CurrencyItem price = CurrencyItem.Parse(strOrderPrice);
                CurrencyItem wish = CurrencyItem.Parse(strWishOrderPrice);

                price.EnsurePrefix("CNY");
                wish.EnsurePrefix("CNY");

                if (string.IsNullOrEmpty(price.Postfix) == true
                    && string.IsNullOrEmpty(wish.Postfix) == true
                    && price.Prefix != wish.Prefix)
                {
                    strError = "码洋 '" + price.ToString() + "' 和订购价 '" + wish.ToString() + "' 货币单位不同，无法进行比较";
                    return -2;
                }

                if (price.IsEqual(wish, "CNY") == true)
                    return 1;

                strError = "码洋 '" + strFixedPrice + "' 和折扣 '" + strDiscount + "' 计算出的结果 '" + strWishOrderPrice + "' 和订购价 '" + strOrderPrice + "' 不符";
                return 0;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
        }

        // 根据订购价和折扣倒着计算出码洋
        // 注意最后 strFixedOrderPrice 中返回的金额字符串，和 strPrice 的货币类型一样。有可能缺省货币类型。
        // (缺乏货币类型的金额字符串在互相比较的时候需要添加上默认货币类型以后再比较)
        // return:
        //      -1  计算过程出现错误
        //      0   strPrice 为空，无法计算
        //      1   计算成功
        public static int ComputeFixedPriceByOrderPrice(string strPrice,
    string strDiscount,
    out string strFixedOrderPrice,
    out string strError)
        {
            strError = "";
            strFixedOrderPrice = "";

            if (string.IsNullOrEmpty(strPrice))
            {
                strError = "单价为空，无法计算码洋";
                return 0;
            }

            if (string.IsNullOrEmpty(strPrice) == false
    && strPrice.StartsWith("{"))
            {
                strError = "ComputeFixedPriceByOrderPrice() 不应用于虚拟字符串";
                return -1;
            }

            // 折扣为空，默认 '1.0'
            if (string.IsNullOrEmpty(strDiscount))
                strDiscount = "1.0";

            if (decimal.TryParse(strDiscount, out decimal discount) == false)
            {
                strError = "折扣值 '" + strDiscount + "' 格式错误。应为 '1.0' '0.90' 这样的形态";
                return -1;
            }

            if (discount == 0)
            {
                strError = "折扣值不应为 0";
                return -1;
            }

            try
            {
                CurrencyItem item = CurrencyItem.Parse(strPrice);
                item.Value = item.Value / discount;
                strFixedOrderPrice = item.ToString();    // 注意，变换为字符串时候，decimal 值没有做四舍五入
                return 1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
        }

        // 根据码洋和折扣计算订购价
        // 注意最后 strOrderPrice 中返回的金额字符串，和 strFixedPrice 的货币类型一样。有可能缺省货币类型。
        // (缺乏货币类型的金额字符串在互相比较的时候需要添加上默认货币类型以后再比较)
        // return:
        //      -1  计算过程出现错误
        //      0   strFixedPrice 为空，无法计算
        //      1   计算成功
        public static int ComputeOrderPriceByFixedPrice(string strFixedPrice,
    string strDiscount,
    out string strOrderPrice,
    out string strError)
        {
            strError = "";
            strOrderPrice = "";

            if (string.IsNullOrEmpty(strFixedPrice))
            {
                strError = "码洋为空，无法计算订购价";
                return 0;
            }

            if (string.IsNullOrEmpty(strFixedPrice) == false
    && strFixedPrice.StartsWith("{"))
            {
                strError = "ComputeOrderPriceByFixedPrice() 不应用于虚拟字符串";
                return -1;
            }

            // 折扣为空，默认 '1.0'
            if (string.IsNullOrEmpty(strDiscount))
                strDiscount = "1.0";

            if (decimal.TryParse(strDiscount, out decimal discount) == false)
            {
                strError = "折扣值 '" + strDiscount + "' 格式错误。应为 '1.0' '0.90' 这样的形态";
                return -1;
            }

            try
            {
                CurrencyItem item = CurrencyItem.Parse(strFixedPrice);
                item.Value = item.Value * discount;
                strOrderPrice = item.ToString();    // 注意，变换为字符串时候，decimal 值没有做四舍五入
                return 1;
            }
            catch (Exception ex)
            {
                strError = ex.Message;
                return -1;
            }
        }

        // 价格乘以一个倍率
        // 注：可以处理 "{CNY10.00}" 这样的字符串。结果字符串依然保留 {} 形态
        public static string MultiplePrice(string price,
            int count,
            string position)
        {
            string changed_price = StringUtil.Unquote(price, "{}");

            int nRet = PriceUtil.MultiPrice(changed_price, count, out string result, out string strError);
            if (nRet == -1)
                throw new PositionException(strError, position);
            if (string.IsNullOrEmpty(price) == false && price[0] == '{')
                return "{" + result + "}";
            return result;
        }

        // 累加价格字符串
        // 注：可以处理 "{CNY10.00}" 这样的字符串。但结果字符串不带有 {}
        public static string GetTotalPrice(List<string> prices,
            string strPosition)
        {
            List<string> changed_prices = new List<string>();
            foreach (string price in prices)
            {
                changed_prices.Add(StringUtil.Unquote(price, "{}"));
            }
            int nRet = PriceUtil.TotalPrice(changed_prices,
    out string strResult,
    out string strError);
            if (nRet == -1)
                throw new PositionException(strError, strPosition);
            return strResult;
        }
    }

    [Flags]
    public enum ItemState
    {
        Normal = 0x00,  // 普通状态
        Changed = 0x01, // 内容被修改过
        New = 0x02, // 新增的行
        Selected = 0x04,    // 被选择

        ReadOnly = 0x10, // 状态为只读的行。订购态下，因为“已订购”，订单已经发出，内容不能再更改了；验收态下，因为尚未订购，所以不能进行验收，内容不能更改
    }

    public class Item : IDisposable
    {
        int m_nInDropDown = 0;  // 2009/1/15

        public OrderDesignControl Container = null;

        public object Tag = null;   // 用于存放需要连接的任意类型对象

        // 颜色、popupmenu
        public Label label_color = null;

        // 书目号 2008/8/31
        public TextBox textBox_catalogNo = null;

        // 渠道
        public ComboBox comboBox_seller = null;

        // 经费来源
        public DoubleComboBox comboBox_source = null;

        // 时间范围
        public DateRangeControl dateRange_range = null;

        // 期数
        public ComboBox comboBox_issueCount = null;

        // 复本数
        public DoubleComboBox comboBox_copy = null;

        // 单价
        public DoubleTextBox textBox_price = null;

        // 2018/7/31
        // 码洋
        public DoubleTextBox textBox_fixedPrice = null;

        // 2018/7/31
        // 折扣
        public DoubleComboBox comboBox_discount = null;

        // 去向
        // public TextBox textBox_location = null;
        public LocationEditControl location = null;

        // 类别 2008/8/31
        public ComboBox comboBox_class = null;

        // 渠道地址
        public Label label_sellerAddress = null;

        internal string m_sellerAddressXml = "";    // 表示渠道地址的XML记录。根元素为<sellerAddress>

        // 其他信息
        public Label label_other = null;

        internal string m_otherXml = "";    // 表示其他信息的XML记录

        void DisposeChildControls()
        {
            label_color.Dispose();
            textBox_catalogNo.Dispose();
            comboBox_seller.Dispose();
            comboBox_source.Dispose();
            dateRange_range.Dispose();
            comboBox_issueCount.Dispose();
            comboBox_copy.Dispose();
            textBox_price.Dispose();
            textBox_fixedPrice.Dispose();
            comboBox_discount.Dispose();
            location.Dispose();
            comboBox_class.Dispose();
            label_sellerAddress.Dispose();
            label_other.Dispose();
            Container = null;
        }

        string _stateComment = "";

        ItemState m_state = ItemState.Normal;

        // 主动修改location控件的ArrivedCount，需要避免递归处理由此引起的事件
        int DisableLocationArrivedChanged = 0;

        #region 释放资源

#if NO
        // TODO: 容易造成 mem leak。建议用 Dispose() 改写
        ~Item()
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

        public Item(OrderDesignControl container)
        {
            this.Container = container;
            int nTopBlank = (int)this.Container.Font.GetHeight() + 2;

            // 色块
            label_color = new Label();
            label_color.Dock = DockStyle.Fill;
            label_color.Size = new Size(6, 28);
            label_color.Margin = new Padding(1, 0, 1, 0);

            // 书目号
            this.textBox_catalogNo = new TextBox();
            textBox_catalogNo.BorderStyle = BorderStyle.None;
            textBox_catalogNo.Dock = DockStyle.Fill;
            textBox_catalogNo.MinimumSize = new Size(80, 28);
            // textBox_price.Multiline = true;
            textBox_catalogNo.Margin = new Padding(6, nTopBlank + 6, 6, 0);
            textBox_catalogNo.ForeColor = this.Container.tableLayoutPanel_content.ForeColor;
            // this.textBox_catalogNo.Visible = false;

            // 渠道
            comboBox_seller = new ComboBox();
            comboBox_seller.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox_seller.FlatStyle = FlatStyle.Flat;
            comboBox_seller.Dock = DockStyle.Fill;
            comboBox_seller.MaximumSize = new Size(150, 28);
            comboBox_seller.Size = new Size(100, 28);
            comboBox_seller.MinimumSize = new Size(50, 28);
            comboBox_seller.DropDownHeight = 300;
            comboBox_seller.DropDownWidth = 300;
            comboBox_seller.ForeColor = this.Container.tableLayoutPanel_content.ForeColor;
            comboBox_seller.Text = "";
            comboBox_seller.Margin = new Padding(6, nTopBlank + 6, 6, 0);
            // this.comboBox_seller.Visible = false;

            // 经费来源
            comboBox_source = new DoubleComboBox();

            comboBox_source.ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox_source.ComboBox.FlatStyle = FlatStyle.Flat;
            comboBox_source.ComboBox.DropDownHeight = 300;
            comboBox_source.ComboBox.DropDownWidth = 300;
            comboBox_source.ComboBox.ForeColor = this.Container.tableLayoutPanel_content.ForeColor;
            comboBox_source.Margin = new Padding(6, nTopBlank,  // + 3,
                6, 0);

            comboBox_source.TextBox.ReadOnly = true;
            comboBox_source.TextBox.BorderStyle = BorderStyle.None;
            comboBox_source.TextBox.ForeColor = SystemColors.GrayText;

            comboBox_source.Dock = DockStyle.Fill;
            comboBox_source.MaximumSize = new Size(110, 28 * 2);
            comboBox_source.Size = new Size(80, 28 * 2);
            comboBox_source.MinimumSize = new Size(50, 28);

            // 范围
            dateRange_range = new DateRangeControl();

            if (container != null && container.SeriesMode == false)
            {
                // dateRange_range.Visible = false; // ????
            }

            // dateRange_range.ForeColor = this.Container.tableLayoutPanel_content.ForeColor;
            dateRange_range.BorderStyle = BorderStyle.None;

            dateRange_range.Dock = DockStyle.Fill;
            dateRange_range.Margin = new Padding(1, nTopBlank, // + 3,
                1, 0);
            // this.dateRange_range.Visible = false;

            // 期数
            comboBox_issueCount = new ComboBox();

            if (container != null && container.SeriesMode == false)
                this.comboBox_issueCount.Visible = false;

            comboBox_issueCount.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox_issueCount.FlatStyle = FlatStyle.Flat;
            comboBox_issueCount.DropDownHeight = 300;
            comboBox_issueCount.DropDownWidth = 100;
            comboBox_issueCount.Dock = DockStyle.Fill;
            comboBox_issueCount.MaximumSize = new Size(100, 28);
            comboBox_issueCount.Size = new Size(70, 28);
            comboBox_issueCount.MinimumSize = new Size(50, 28);
            comboBox_issueCount.Items.AddRange(new object[] {
            "6",
            "12",
            "24",
            "36"});

            comboBox_issueCount.ForeColor = this.Container.tableLayoutPanel_content.ForeColor;
            comboBox_issueCount.Margin = new Padding(6, nTopBlank + 6, // + 3,
                6, 0);
            // this.comboBox_issueCount.Visible = false;

            // 复本数
            comboBox_copy = new DoubleComboBox();
            comboBox_copy.ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox_copy.ComboBox.FlatStyle = FlatStyle.Flat;
            comboBox_copy.ComboBox.DropDownHeight = 300;
            comboBox_copy.ComboBox.DropDownWidth = 250;
            comboBox_copy.ComboBox.ForeColor = this.Container.tableLayoutPanel_content.ForeColor;
            comboBox_copy.Margin = new Padding(6, nTopBlank, // + 3,
                6, 0);

            comboBox_copy.TextBox.ReadOnly = true;
            comboBox_copy.TextBox.BorderStyle = BorderStyle.None;
            comboBox_copy.TextBox.ForeColor = SystemColors.GrayText;

            comboBox_copy.Dock = DockStyle.Fill;
            comboBox_copy.MaximumSize = new Size(60, 28 * 2);
            comboBox_copy.Size = new Size(40, 28 * 2);
            comboBox_copy.MinimumSize = new Size(30, 28 * 2);
            // this.comboBox_copy.Visible = false;

            // 码洋
            textBox_fixedPrice = new DoubleTextBox();
            textBox_fixedPrice.TextBox.BorderStyle = BorderStyle.None;
            textBox_fixedPrice.TextBox.ForeColor = this.Container.tableLayoutPanel_content.ForeColor;

            textBox_fixedPrice.SecondTextBox.ReadOnly = true;
            textBox_fixedPrice.SecondTextBox.BorderStyle = BorderStyle.None;
            textBox_fixedPrice.SecondTextBox.ForeColor = SystemColors.GrayText;

            textBox_fixedPrice.Dock = DockStyle.Fill;
            textBox_fixedPrice.MaximumSize = new Size(90, 28 * 2);
            textBox_fixedPrice.Size = new Size(70, 28 * 2);
            textBox_fixedPrice.MinimumSize = new Size(50, 28 * 2);
            textBox_fixedPrice.Margin = new Padding(6, nTopBlank + 1,
                6, 0);

            // 折扣
            comboBox_discount = new DoubleComboBox();
            comboBox_discount.ComboBox.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox_discount.ComboBox.FlatStyle = FlatStyle.Flat;
            comboBox_discount.ComboBox.DropDownHeight = 300;
            comboBox_discount.ComboBox.DropDownWidth = 250;
            comboBox_discount.ComboBox.ForeColor = this.Container.tableLayoutPanel_content.ForeColor;
            comboBox_discount.Margin = new Padding(6, nTopBlank, // + 3,
                6, 0);

            comboBox_discount.TextBox.ReadOnly = true;
            comboBox_discount.TextBox.BorderStyle = BorderStyle.None;
            comboBox_discount.TextBox.ForeColor = SystemColors.GrayText;

            comboBox_discount.Dock = DockStyle.Fill;
            comboBox_discount.MaximumSize = new Size(60, 28 * 2);
            comboBox_discount.Size = new Size(50, 28 * 2);
            comboBox_discount.MinimumSize = new Size(40, 28 * 2);

            // 单价
            textBox_price = new DoubleTextBox();
            textBox_price.TextBox.BorderStyle = BorderStyle.None;
            textBox_price.TextBox.ForeColor = this.Container.tableLayoutPanel_content.ForeColor;

            textBox_price.SecondTextBox.ReadOnly = true;
            textBox_price.SecondTextBox.BorderStyle = BorderStyle.None;
            textBox_price.SecondTextBox.ForeColor = SystemColors.GrayText;

            textBox_price.Dock = DockStyle.Fill;
            textBox_price.MaximumSize = new Size(90, 28 * 2);
            textBox_price.Size = new Size(70, 28 * 2);
            textBox_price.MinimumSize = new Size(50, 28 * 2);
            textBox_price.Margin = new Padding(6, nTopBlank + 1,
                6, 0);

            // 去向
            location = new LocationEditControl();
            location.ArriveMode = this.Container.ArriveMode;
            location.BorderStyle = BorderStyle.None;
            location.Dock = DockStyle.Fill;
            // location.MinimumSize = new Size(100, 28);
            location.Margin = new Padding(6, nTopBlank + 6,
                6, 0);

            location.ForeColor = this.Container.tableLayoutPanel_content.ForeColor;
            location.AutoScaleMode = AutoScaleMode.None;    // 防止它内部的控件放上去后被重新挪动位置
            // location.BorderStyle = BorderStyle.FixedSingle;
            location.DbName = container.BiblioDbName;

            // 类别
            comboBox_class = new ComboBox();
            comboBox_class.DropDownStyle = ComboBoxStyle.DropDown;
            comboBox_class.FlatStyle = FlatStyle.Flat;
            comboBox_class.Dock = DockStyle.Fill;
            comboBox_class.MaximumSize = new Size(150, 28);
            comboBox_class.Size = new Size(100, 28);
            comboBox_class.MinimumSize = new Size(50, 28);
            comboBox_class.DropDownHeight = 300;
            comboBox_class.DropDownWidth = 300;
            comboBox_class.ForeColor = this.Container.tableLayoutPanel_content.ForeColor;
            comboBox_class.Text = "";
            comboBox_class.Margin = new Padding(6, nTopBlank + 6,
                6, 0);
            // this.comboBox_class.Visible = false;

            // 渠道地址
            this.label_sellerAddress = new Label();
            this.label_sellerAddress.BorderStyle = BorderStyle.None;
            this.label_sellerAddress.Dock = DockStyle.Fill;
            this.label_sellerAddress.MinimumSize = new Size(40, 28 * 2);
            // this.label_sellerAddress.Multiline = true;
            this.label_sellerAddress.Margin = new Padding(6, nTopBlank + 6,
                6, 0);
            this.label_sellerAddress.AutoSize = true;

            this.label_sellerAddress.ForeColor = this.Container.tableLayoutPanel_content.ForeColor;
            // this.label_sellerAddress.Visible = false;

            // 其他
            this.label_other = new Label();
            this.label_other.BorderStyle = BorderStyle.None;
            this.label_other.Dock = DockStyle.Fill;
            this.label_other.MinimumSize = new Size(50, 28 * 2);
            // this.label_other.Multiline = true;
            this.label_other.Margin = new Padding(6, nTopBlank + 6,
                6, 0);
            this.label_other.AutoSize = true;
            // this.label_other.Visible = false;

            this.label_other.ForeColor = this.Container.tableLayoutPanel_content.ForeColor;
        }

        public void EnsureVisible()
        {
            this.Container.EnsureVisible(this);
        }

        bool m_bSeriesMode = false;
        public bool SeriesMode
        {
            get
            {
                return this.m_bSeriesMode;
            }
            set
            {
                this.m_bSeriesMode = value;
                if (value == true)
                {
                    this.dateRange_range.Visible = true;
                    this.comboBox_issueCount.Visible = true;
                }
                else
                {
                    this.dateRange_range.Visible = false;
                    this.comboBox_issueCount.Visible = false;
                }
            }
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

                    // 书目号
                    this.textBox_catalogNo.ReadOnly = value;

                    // 渠道
                    this.comboBox_seller.Enabled = !value;

                    // 经费来源
                    this.comboBox_source.Enabled = !value;

                    // 时间范围
                    this.dateRange_range.Enabled = !value;

                    // 期数
                    this.comboBox_issueCount.Enabled = !value;

                    // 复本数
                    this.comboBox_copy.Enabled = !value;

                    // 码洋
                    this.textBox_fixedPrice.ReadOnly = value;

                    // 折扣
                    this.comboBox_discount.Enabled = !value;

                    // 单价
                    this.textBox_price.ReadOnly = value;

                    // 去向
                    this.location.ReadOnly = value;

                    // 类别
                    this.comboBox_class.Enabled = !value;

                    // 渠道地址

                    // 其他
                    // this.label_other
                }
            }
        }

        // 状态注释，用于 tips 显示
        public string StateComment
        {
            get
            {
                return this._stateComment;
            }
            set
            {
                this._stateComment = value;
                this.Container?.ToolTip?.SetToolTip(this.label_color, value);
            }
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

                    // 状态变动后，还会引起份数统计值的变动
                    if (bOldReadOnly != this.ReadOnly)
                    {
                        // 如果当前是订购态
                        if (this.Container.ArriveMode == false)
                        {
                            this.Container.DisableNewlyOrderTextChanged++;    // 优化，避免多余的动作
                            this.Container.textBox_newlyOrderTotalCopy.Text = this.Container.GetNewlyOrderTotalCopy().ToString();
                            this.Container.DisableNewlyOrderTextChanged--;

                            int nOrderedTotalCopy = this.Container.GetOrderedTotalCopy();
                            this.Container.textBox_orderedTotalCopy.Text = nOrderedTotalCopy.ToString();

                            if (nOrderedTotalCopy > 0)
                                this.Container.OrderedTotalCopyVisible = true;
                            else
                                this.Container.OrderedTotalCopyVisible = false;
                        }
                        else
                        {
                            // 如果当前是验收态

                            this.Container.DisableNewlyArriveTextChanged++;    // 优化，避免多余的动作
                            this.Container.textBox_newlyArriveTotalCopy.Text = this.Container.GetNewlyArriveTotalCopy().ToString();
                            this.Container.DisableNewlyArriveTextChanged--;

                            int nArrivedTotalCopy = this.Container.GetArrivedTotalCopy();
                            this.Container.textBox_arrivedTotalCopy.Text = nArrivedTotalCopy.ToString();

                            if (nArrivedTotalCopy > 0)
                                this.Container.ArrivedTotalCopyVisible = true;
                            else
                                this.Container.ArrivedTotalCopyVisible = false;
                        }

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
                    && this.Container.m_bFocused == false)
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

        // 将控件加入到tablelayoutpanel中
        internal void AddToTable(TableLayoutPanel table,
            int nRow)
        {
            int i = 0;
            table.Controls.Add(this.label_color, i++, nRow);
            table.Controls.Add(this.textBox_catalogNo, i++, nRow);
            table.Controls.Add(this.comboBox_seller, i++, nRow);
            table.Controls.Add(this.comboBox_source, i++, nRow);

            table.Controls.Add(this.dateRange_range, i++, nRow);
            table.Controls.Add(this.comboBox_issueCount, i++, nRow);

            table.Controls.Add(this.comboBox_copy, i++, nRow);
            table.Controls.Add(this.textBox_fixedPrice, i++, nRow);
            table.Controls.Add(this.comboBox_discount, i++, nRow);
            table.Controls.Add(this.textBox_price, i++, nRow);
            table.Controls.Add(this.location, i++, nRow);
            table.Controls.Add(this.comboBox_class, i++, nRow);
            table.Controls.Add(this.label_sellerAddress, i++, nRow);
            table.Controls.Add(this.label_other, i++, nRow);

            AddEvents(true);
        }

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
                table.Controls.Remove(this.label_color);
                table.Controls.Remove(this.textBox_catalogNo);
                table.Controls.Remove(this.comboBox_seller);
                table.Controls.Remove(this.comboBox_source);

                table.Controls.Remove(this.dateRange_range);
                table.Controls.Remove(this.comboBox_issueCount);

                table.Controls.Remove(this.comboBox_copy);

                table.Controls.Remove(this.textBox_fixedPrice);
                table.Controls.Remove(this.comboBox_discount);

                table.Controls.Remove(this.textBox_price);
                table.Controls.Remove(this.location);
                table.Controls.Remove(this.comboBox_class);
                table.Controls.Remove(this.label_sellerAddress);
                table.Controls.Remove(this.label_other);

                Debug.Assert(this.Container.Items.Count == table.RowCount - 2, "");

                // 然后压缩后方的
                for (int i = (table.RowCount - 2) - 1; i >= nRow + 1; i--)
                {
                    Item line = this.Container.Items[i];

                    int index = 0;

                    // color
                    Label label = line.label_color;
                    table.Controls.Remove(label);
                    table.Controls.Add(label, index++, i - 1 + 1);

                    // catalog no
                    TextBox catalogNo = line.textBox_catalogNo;
                    table.Controls.Remove(catalogNo);
                    table.Controls.Add(catalogNo, index++, i - 1 + 1);

                    // seller
                    ComboBox seller = line.comboBox_seller;
                    table.Controls.Remove(seller);
                    table.Controls.Add(seller, index++, i - 1 + 1);

                    // source
                    DoubleComboBox source = line.comboBox_source;
                    table.Controls.Remove(source);
                    table.Controls.Add(source, index++, i - 1 + 1);

                    // time range
                    DateRangeControl range = line.dateRange_range;
                    table.Controls.Remove(range);
                    table.Controls.Add(range, index++, i - 1 + 1);

                    // issue count
                    ComboBox issueCount = line.comboBox_issueCount;
                    table.Controls.Remove(issueCount);
                    table.Controls.Add(issueCount, index++, i - 1 + 1);

                    // copy
                    DoubleComboBox copy = line.comboBox_copy;
                    table.Controls.Remove(copy);
                    table.Controls.Add(copy, index++, i - 1 + 1);

                    // fixedPrice
                    DoubleTextBox fixedPrice = line.textBox_fixedPrice;
                    table.Controls.Remove(fixedPrice);
                    table.Controls.Add(fixedPrice, index++, i - 1 + 1);

                    // discount
                    DoubleComboBox discount = line.comboBox_discount;
                    table.Controls.Remove(comboBox_discount);
                    table.Controls.Add(comboBox_discount, index++, i - 1 + 1);

                    // price
                    DoubleTextBox price = line.textBox_price;
                    table.Controls.Remove(price);
                    table.Controls.Add(price, index++, i - 1 + 1);

                    // location
                    LocationEditControl location = line.location;
                    table.Controls.Remove(location);
                    table.Controls.Add(location, index++, i - 1 + 1);

                    // class
                    ComboBox orderClass = line.comboBox_class;
                    table.Controls.Remove(orderClass);
                    table.Controls.Add(orderClass, index++, i - 1 + 1);

                    // seller address
                    Label sellerAddress = line.label_sellerAddress;
                    table.Controls.Remove(sellerAddress);
                    table.Controls.Add(sellerAddress, index++, i - 1 + 1);

                    // other
                    Label other = line.label_other;
                    table.Controls.Remove(other);
                    table.Controls.Add(other, index++, i - 1 + 1);
                }

                table.RowCount--;
                table.RowStyles.RemoveAt(nRow);

                this.AddEvents(false);  // 2015/7/21
            }
            finally
            {
                this.Container.EnableUpdate();
            }

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
                Debug.Assert(table.RowCount == this.Container.Items.Count + 3, "");

                // 先移动后方的
                for (int i = (table.RowCount - 1) - 3; i >= nRow; i--)
                {
                    Item line = this.Container.Items[i];

                    int index = 0;
                    // color
                    Label label = line.label_color;
                    table.Controls.Remove(label);
                    table.Controls.Add(label, index++, i + 1 + 1);

                    // catalog no
                    TextBox catalogNo = line.textBox_catalogNo;
                    table.Controls.Remove(catalogNo);
                    table.Controls.Add(catalogNo, index++, i + 1 + 1);


                    // seller
                    ComboBox seller = line.comboBox_seller;
                    table.Controls.Remove(seller);
                    table.Controls.Add(seller, index++, i + 1 + 1);


                    // source
                    DoubleComboBox source = line.comboBox_source;
                    table.Controls.Remove(source);
                    table.Controls.Add(source, index++, i + 1 + 1);

                    // time range
                    DateRangeControl range = line.dateRange_range;
                    table.Controls.Remove(range);
                    table.Controls.Add(range, index++, i + 1 + 1);

                    // issue count
                    ComboBox issueCount = line.comboBox_issueCount;
                    table.Controls.Remove(issueCount);
                    table.Controls.Add(issueCount, index++, i + 1 + 1);

                    // copy
                    DoubleComboBox copy = line.comboBox_copy;
                    table.Controls.Remove(copy);
                    table.Controls.Add(copy, index++, i + 1 + 1);

                    // fixedPrice
                    DoubleTextBox fixedPrice = line.textBox_fixedPrice;
                    table.Controls.Remove(fixedPrice);
                    table.Controls.Add(fixedPrice, index++, i + 1 + 1);

                    // discount
                    DoubleComboBox discount = line.comboBox_discount;
                    table.Controls.Remove(discount);
                    table.Controls.Add(discount, index++, i + 1 + 1);

                    // price
                    DoubleTextBox price = line.textBox_price;
                    table.Controls.Remove(price);
                    table.Controls.Add(price, index++, i + 1 + 1);

                    // location
                    table.Controls.Remove(line.location);
                    table.Controls.Add(line.location, index++, i + 1 + 1);

                    // class
                    ComboBox orderClass = line.comboBox_class;
                    table.Controls.Remove(orderClass);
                    table.Controls.Add(orderClass, index++, i + 1 + 1);

                    // seller address
                    table.Controls.Remove(line.label_sellerAddress);
                    table.Controls.Add(line.label_sellerAddress, index++, i + 1 + 1);

                    // other
                    table.Controls.Remove(line.label_other);
                    table.Controls.Add(line.label_other, index++, i + 1 + 1);
                }

                {
                    int index = 0;
                    table.Controls.Add(this.label_color, index++, nRow + 1);
                    table.Controls.Add(this.textBox_catalogNo, index++, nRow + 1);
                    table.Controls.Add(this.comboBox_seller, index++, nRow + 1);
                    table.Controls.Add(this.comboBox_source, index++, nRow + 1);

                    table.Controls.Add(this.dateRange_range, index++, nRow + 1);
                    table.Controls.Add(this.comboBox_issueCount, index++, nRow + 1);

                    table.Controls.Add(this.comboBox_copy, index++, nRow + 1);

                    table.Controls.Add(this.textBox_fixedPrice, index++, nRow + 1);
                    table.Controls.Add(this.comboBox_discount, index++, nRow + 1);

                    table.Controls.Add(this.textBox_price, index++, nRow + 1);
                    table.Controls.Add(this.location, index++, nRow + 1);
                    table.Controls.Add(this.comboBox_class, index++, nRow + 1);
                    table.Controls.Add(this.label_sellerAddress, index++, nRow + 1);
                    table.Controls.Add(this.label_other, index++, nRow + 1);
                }
            }
            finally
            {
                this.Container.EnableUpdate();
            }

            // events
            AddEvents(true);
        }

        void AddEvents(bool bAdd)
        {
            if (bAdd)
            {
                // label_color
                this.label_color.MouseUp += new MouseEventHandler(label_color_MouseUp);

                this.label_color.MouseClick += new MouseEventHandler(label_color_MouseClick);

                // catalog no 
                this.textBox_catalogNo.Enter += new EventHandler(control_Enter);

                this.textBox_catalogNo.TextChanged += new EventHandler(textBox_catalogNo_TextChanged);

                // seller
                this.comboBox_seller.DropDown += new EventHandler(comboBox_seller_DropDown);

                this.comboBox_seller.Enter += new EventHandler(control_Enter);

                this.comboBox_seller.TextChanged += new EventHandler(comboBox_seller_TextChanged);

                this.comboBox_seller.SelectedIndexChanged += new EventHandler(comboBox_seller_SelectedIndexChanged);

                // source
                this.comboBox_source.ComboBox.DropDown += new EventHandler(comboBox_seller_DropDown);

                this.comboBox_source.Enter += new EventHandler(control_Enter);

                this.comboBox_source.ComboBox.TextChanged += new EventHandler(comboBox_source_TextChanged);

                this.comboBox_source.SelectedIndexChanged += new EventHandler(comboBox_seller_SelectedIndexChanged);

                // 2012/5/26
                // range
                this.dateRange_range.DateTextChanged += new EventHandler(dateRange_range_DateTextChanged);

                this.dateRange_range.Enter += new EventHandler(control_Enter);

                // issuecount
                this.comboBox_issueCount.TextChanged += new EventHandler(comboBox_issueCount_TextChanged);

                this.comboBox_issueCount.Enter += new EventHandler(control_Enter);

                // copy
                this.comboBox_copy.ComboBox.DropDown += new EventHandler(comboBox_copy_DropDown);
                this.comboBox_copy.Enter += new EventHandler(control_Enter);
                this.comboBox_copy.ComboBox.TextChanged += new EventHandler(comboBox_copy_TextChanged);

                // fixedPrice
                this.textBox_fixedPrice.TextBox.TextChanged += new EventHandler(FixedPrice_TextChanged);
                this.textBox_fixedPrice.TextBox.Enter += new EventHandler(control_Enter);
                this.textBox_fixedPrice.TextBox.KeyDown += TextBox_fixedPrice_KeyDown;

                // discount
                this.comboBox_discount.ComboBox.DropDown += new EventHandler(comboBox_discount_DropDown);
                this.comboBox_discount.Enter += new EventHandler(control_Enter);
                this.comboBox_discount.ComboBox.TextChanged += new EventHandler(comboBox_discount_TextChanged);

                // price
                this.textBox_price.TextBox.TextChanged += new EventHandler(Price_TextChanged);
                this.textBox_price.TextBox.Enter += new EventHandler(control_Enter);
                this.textBox_price.TextBox.KeyDown += TextBox_price_KeyDown;

                // location
                this.location.GetValueTable += new GetValueTableEventHandler(textBox_location_GetValueTable);

                this.location.Enter += new EventHandler(control_Enter);

                this.location.ContentChanged += new ContentChangedEventHandler(location_ContentChanged);

                this.location.ArrivedChanged += new EventHandler(location_ArrivedChanged);

                this.location.ReadOnlyChanged += new EventHandler(location_ReadOnlyChanged);

                // class
                this.comboBox_class.DropDown += new EventHandler(comboBox_seller_DropDown);

                this.comboBox_class.Enter += new EventHandler(control_Enter);

                this.comboBox_class.TextChanged += new EventHandler(comboBox_class_TextChanged);

                this.comboBox_class.SelectedIndexChanged += new EventHandler(comboBox_seller_SelectedIndexChanged);

                // address
                this.label_sellerAddress.Click += new EventHandler(control_Enter);

                // other
                this.label_other.Click += new EventHandler(control_Enter);
            }
            else
            {
                this.label_color.MouseUp -= new MouseEventHandler(label_color_MouseUp);
                this.label_color.MouseClick -= new MouseEventHandler(label_color_MouseClick);
                this.textBox_catalogNo.Enter -= new EventHandler(control_Enter);
                this.textBox_catalogNo.TextChanged -= new EventHandler(textBox_catalogNo_TextChanged);
                this.comboBox_seller.DropDown -= new EventHandler(comboBox_seller_DropDown);
                this.comboBox_seller.Enter -= new EventHandler(control_Enter);
                this.comboBox_seller.TextChanged -= new EventHandler(comboBox_seller_TextChanged);
                this.comboBox_seller.SelectedIndexChanged -= new EventHandler(comboBox_seller_SelectedIndexChanged);
                this.comboBox_source.ComboBox.DropDown -= new EventHandler(comboBox_seller_DropDown);
                this.comboBox_source.Enter -= new EventHandler(control_Enter);
                this.comboBox_source.ComboBox.TextChanged -= new EventHandler(comboBox_source_TextChanged);
                this.comboBox_source.SelectedIndexChanged -= new EventHandler(comboBox_seller_SelectedIndexChanged);
                this.dateRange_range.DateTextChanged -= new EventHandler(dateRange_range_DateTextChanged);
                this.dateRange_range.Enter -= new EventHandler(control_Enter);
                this.comboBox_issueCount.TextChanged -= new EventHandler(comboBox_issueCount_TextChanged);
                this.comboBox_issueCount.Enter -= new EventHandler(control_Enter);
                this.comboBox_copy.ComboBox.DropDown -= new EventHandler(comboBox_copy_DropDown);
                this.comboBox_copy.Enter -= new EventHandler(control_Enter);
                this.comboBox_copy.ComboBox.TextChanged -= new EventHandler(comboBox_copy_TextChanged);


                // fixedPrice
                this.textBox_fixedPrice.TextBox.TextChanged -= new EventHandler(FixedPrice_TextChanged);
                this.textBox_fixedPrice.TextBox.Enter -= new EventHandler(control_Enter);

                // discount
                this.comboBox_discount.ComboBox.DropDown -= new EventHandler(comboBox_discount_DropDown);
                this.comboBox_discount.Enter -= new EventHandler(control_Enter);
                this.comboBox_discount.ComboBox.TextChanged -= new EventHandler(comboBox_copy_TextChanged);

                this.textBox_price.TextBox.TextChanged -= new EventHandler(Price_TextChanged);
                this.textBox_price.TextBox.Enter -= new EventHandler(control_Enter);
                this.textBox_price.TextBox.KeyDown -= TextBox_price_KeyDown;


                this.location.GetValueTable -= new GetValueTableEventHandler(textBox_location_GetValueTable);
                this.location.Enter -= new EventHandler(control_Enter);
                this.location.ContentChanged -= new ContentChangedEventHandler(location_ContentChanged);
                this.location.ArrivedChanged -= new EventHandler(location_ArrivedChanged);
                this.location.ReadOnlyChanged -= new EventHandler(location_ReadOnlyChanged);
                this.comboBox_class.DropDown -= new EventHandler(comboBox_seller_DropDown);
                this.comboBox_class.Enter -= new EventHandler(control_Enter);
                this.comboBox_class.TextChanged -= new EventHandler(comboBox_class_TextChanged);
                this.comboBox_class.SelectedIndexChanged -= new EventHandler(comboBox_seller_SelectedIndexChanged);
                this.label_sellerAddress.Click -= new EventHandler(control_Enter);
                this.label_other.Click -= new EventHandler(control_Enter);
            }
        }

        private void TextBox_fixedPrice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                // 自动计算出码洋
                // 注： 如果 FixedPrice 里面已经是实在价格，那么就不执行这个 Ctrl+A
                if (string.IsNullOrEmpty(this.Price) == false
                    && (OrderDesignControl.IsVirtual(this.FixedPrice) || string.IsNullOrEmpty(this.FixedPrice)))
                {
                    Debug.Assert(this.Price.StartsWith("{") == false, "");
                    // return:
                    //      -1  计算过程出现错误
                    //      0   strPrice 为空，无法计算
                    //      1   计算成功
                    int nRet = OrderDesignControl.ComputeFixedPriceByOrderPrice(this.Price,
                this.Discount,
                out string strResultPrice,
                out string strError);
                    if (nRet == -1)
                    {
                        strError = "根据订购价 '" + this.Price + "' 和折扣 '" + this.Discount + "' 自动计算码洋时出错: " + strError;
                        MessageBox.Show(this.Container, strError);
                        goto END;
                    }
                    if (nRet == 1)
                    {
                        if (string.IsNullOrEmpty(strResultPrice) == false)
                            strResultPrice = "{" + strResultPrice + "}";    // {} 表示这是计算出来的，不是原始值

                        if (this.FixedPrice != strResultPrice)
                            this.FixedPrice = strResultPrice;
                    }
                }
                else
                    Console.Beep();

                END:
                e.Handled = true;
            }
        }

        private void TextBox_price_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
            {
                // MessageBox.Show(this.Container, "Ctrl+A");

                // 自动计算出验收价
                if (string.IsNullOrEmpty(this.FixedPrice) == false
                    && OrderDesignControl.IsVirtual(this.FixedPrice) == false)
                {
                    // return:
                    //      -1  计算过程出现错误
                    //      0   strFixedPrice 为空，无法计算
                    //      1   计算成功
                    int nRet = OrderDesignControl.ComputeOrderPriceByFixedPrice(this.FixedPrice,
                this.Discount,
                out string strWishOrderPrice,
                out string strError);
                    if (nRet == -1)
                    {
                        strError = "根据码洋 '" + this.FixedPrice + "' 和折扣 '" + this.Discount + "' 自动计算单价时出错: " + strError;
                        MessageBox.Show(this.Container, strError);
                        goto END;
                    }
                    if (nRet == 1)
                    {
                        if (this.Price != strWishOrderPrice)
                            this.Price = strWishOrderPrice;
                    }
                }
                else
                    Console.Beep();

                END:
                e.Handled = true;
            }
        }

#if NO
        // 过滤掉 {} 包围的部分
        static string GetPureSelectedValue(string strText)
        {
            for (; ; )
            {
                int nRet = strText.IndexOf("{");
                if (nRet == -1)
                    return strText;
                int nStart = nRet;
                nRet = strText.IndexOf("}", nStart + 1);
                if (nRet == -1)
                    return strText;
                int nEnd = nRet;
                strText = strText.Remove(nStart, nEnd - nStart + 1).Trim();
            }
        }
#endif

        delegate void Delegate_filterValue(Control control);

        // 不安全版本
        // 过滤掉 {} 包围的部分
        void __FilterValue(Control control)
        {
            if (control is DoubleComboBox)
            {
                DoubleComboBox combox = (DoubleComboBox)control;
                string strText = StringUtil.GetPureSelectedValue(combox.Text);
                if (combox.Text != strText)
                    combox.Text = strText;
            }
            else
            {
                string strText = StringUtil.GetPureSelectedValue(control.Text);
                if (control.Text != strText)
                    control.Text = strText;
            }
        }

#if NO
        // 安全版本
        void FilterValue(Control control)
        {
            if (this.Container.InvokeRequired == true)
            {
                Delegate_filterValue d = new Delegate_filterValue(__FilterValue);
                this.Container.BeginInvoke(d, new object[] { control });
            }
            else
            {
                __FilterValue((Control)control);
            }
        }
#endif
        // 安全版本
        void FilterValue(Control control)
        {
            Delegate_filterValue d = new Delegate_filterValue(__FilterValue);

            if (this.Container.Created == false)
                __FilterValue((Control)control);
            else
                this.Container.BeginInvoke(d, new object[] { control });
        }

        void comboBox_seller_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterValue((Control)sender);
#if NO
            Delegate_filterValue d = new Delegate_filterValue(__FilterValue);
            this.Container.BeginInvoke(d, new object[] { sender });
#endif
        }

        void comboBox_issueCount_TextChanged(object sender, EventArgs e)
        {
            if ((this.State & ItemState.New) == 0)
                this.State |= ItemState.Changed;

            this.Container.Changed = true;
        }

        void dateRange_range_DateTextChanged(object sender, EventArgs e)
        {

            if ((this.State & ItemState.New) == 0)
                this.State |= ItemState.Changed;

            this.Container.Changed = true;
        }

        #region events

        // 2008/9/13
        void location_ReadOnlyChanged(object sender, EventArgs e)
        {
            if (this.Container.ArriveMode == false)
            {
                this.Container.DisableNewlyOrderTextChanged++;    // 优化，避免多余的动作
                this.Container.textBox_newlyOrderTotalCopy.Text = this.Container.GetNewlyOrderTotalCopy().ToString();
                this.Container.DisableNewlyOrderTextChanged--;

                this.Container.textBox_orderedTotalCopy.Text = this.Container.GetOrderedTotalCopy().ToString();
            }
            else
            {
                this.Container.DisableNewlyArriveTextChanged++;    // 优化，避免多余的动作
                this.Container.textBox_newlyArriveTotalCopy.Text = this.Container.GetNewlyArriveTotalCopy().ToString();
                this.Container.DisableNewlyArriveTextChanged--;

                this.Container.textBox_arrivedTotalCopy.Text = this.Container.GetArrivedTotalCopy().ToString();
            }
        }

        void location_ArrivedChanged(object sender, EventArgs e)
        {
            // 属于主动修改，避免不必要地处理这个连带事件
            if (this.DisableLocationArrivedChanged > 0)
                return;

            UpdateCopyCount();
        }

        // 主动从location checked状态中汇总已到的册数
        public void UpdateCopyCount()
        {
            string strCount = this.location.ArrivedCount.ToString();

            // 2010/12/1
            string strCopy = dp2StringUtil.GetCopyFromCopyString(this.comboBox_copy.Text);

            if (strCopy != strCount)
            {
                // 如果到书copy字符串为空，则需要从订购copy字符串中寻找可能的套内册数
                if (String.IsNullOrEmpty(this.comboBox_copy.Text) == true)
                {
                    string strRightCopy = dp2StringUtil.GetRightFromCopyString(this.comboBox_copy.OldText);
                    if (String.IsNullOrEmpty(strRightCopy) == false)
                    {
                        this.comboBox_copy.Text = dp2StringUtil.ModifyCopy(this.comboBox_copy.Text, strCount);
                        this.comboBox_copy.Text = dp2StringUtil.ModifyRightCopy(this.comboBox_copy.Text, strRightCopy);
                        return;
                    }
                }

                this.comboBox_copy.Text = dp2StringUtil.ModifyCopy(this.comboBox_copy.Text, strCount);
            }


            /*
            if (strCount != this.comboBox_copy.Text)
                this.comboBox_copy.Text = strCount;
             * */
        }

        void location_ContentChanged(object sender, ContentChangedEventArgs e)
        {
            if ((this.State & ItemState.New) == 0)
                this.State |= ItemState.Changed;

            this.Container.Changed = true;
        }

        void comboBox_source_TextChanged(object sender, EventArgs e)
        {
            if (this.Container.ArriveMode == false)
            {
                // 在订购状态下，新旧值保持统一，以便显示单行
                this.comboBox_source.OldText = this.comboBox_source.Text;
            }

            if ((this.State & ItemState.New) == 0)
                this.State |= ItemState.Changed;

            this.Container.Changed = true;

            // 2009/2/15
            // 如果seller和source矛盾，则将seller清为空
            if (this.comboBox_seller.Text == "交换"
                || this.comboBox_seller.Text == "赠")
            {
                if (String.IsNullOrEmpty(this.comboBox_source.Text) == false)
                    this.comboBox_seller.Text = "";
            }
        }

        // price
        void Price_TextChanged(object sender, EventArgs e)
        {
            if (this.Container.InInitial == true)
                return;

            if (this.Container.ArriveMode == false)
            {
                // 在订购状态下，新旧值保持统一，以便显示单行
                this.textBox_price.OldText = this.textBox_price.Text;
#if NO
                // 2018/7/31
                DoubleTextBox control = (sender as Control).Parent as DoubleTextBox;
                Debug.Assert(control == this.textBox_price || control == this.textBox_fixedPrice, "");
                control.OldText = control.Text;
#endif
            }

            // 重新计算出码洋
            // 注：码洋输入域为空的时候，不会自动计算。用户要先在里面 Ctrl+A，创建好一个值，从此以后，每当单价变化的时候才会自动计算码洋
            if (string.IsNullOrEmpty(this.textBox_fixedPrice.Text) == false)
                RecomputFixedPrice();

            if ((this.State & ItemState.New) == 0)
                this.State |= ItemState.Changed;

            this.Container.Changed = true;
        }

        // fixePrice
        void FixedPrice_TextChanged(object sender, EventArgs e)
        {
            if (this.Container.InInitial == true)
                return;

            if (this.Container.ArriveMode == false)
            {
                // 在订购状态下，新旧值保持统一，以便显示单行
                this.textBox_fixedPrice.OldText = this.textBox_fixedPrice.Text;
            }

            // 重新计算出实洋
            RecomputPrice();

            if ((this.State & ItemState.New) == 0)
                this.State |= ItemState.Changed;

            this.Container.Changed = true;
        }

        void comboBox_seller_TextChanged(object sender, EventArgs e)
        {
            if ((this.State & ItemState.New) == 0)
                this.State |= ItemState.Changed;

            this.Container.Changed = true;

            // 2009/2/15
            if (this.comboBox_seller.Text == "交换"
                || this.comboBox_seller.Text == "赠")
                this.comboBox_source.Text = "";
        }

        void comboBox_class_TextChanged(object sender, EventArgs e)
        {
            if ((this.State & ItemState.New) == 0)
                this.State |= ItemState.Changed;

            this.Container.Changed = true;
        }

        void textBox_catalogNo_TextChanged(object sender, EventArgs e)
        {
            if ((this.State & ItemState.New) == 0)
                this.State |= ItemState.Changed;

            this.Container.Changed = true;
        }

        void control_Enter(object sender, EventArgs e)
        {
            this.Container.SelectItem(this, true);
        }

        void label_color_MouseClick(object sender, MouseEventArgs e)
        {
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

        void textBox_location_GetValueTable(object sender, GetValueTableEventArgs e)
        {
            this.Container.OnGetValueTable(sender, e);
        }

        // 复本数 文字改变
        void comboBox_copy_TextChanged(object sender, EventArgs e)
        {
            if (this.Container.ArriveMode == false)
            {
                // 在订购状态下，新旧值保持统一，以便显示单行
                this.comboBox_copy.OldText = this.comboBox_copy.Text;
            }

            try
            {
                // location控件联动
                // 2010/12/1 changed
                int nCopy = Convert.ToInt32(dp2StringUtil.GetCopyFromCopyString(this.comboBox_copy.Text));

                // 如果当前为订购模式
                if (this.Container.ArriveMode == false)
                {
                    this.location.Count = nCopy;

                    // 汇总值发生变化
                    if ((this.State & ItemState.ReadOnly) == 0)
                    {
                        this.Container.DisableNewlyOrderTextChanged++;    // 优化，避免多余的动作
                        this.Container.textBox_newlyOrderTotalCopy.Text = this.Container.GetNewlyOrderTotalCopy().ToString();
                        this.Container.DisableNewlyOrderTextChanged--;
                    }
                    else
                    {
                        this.Container.textBox_orderedTotalCopy.Text = this.Container.GetOrderedTotalCopy().ToString();
                    }
                }
                else
                {
                    // 如果当前为验收模式


                    // 警告太大的值
                    // 2008/9/17
                    int nDelta = nCopy - this.location.ArrivedCount;
                    if (nDelta > 10)
                    {
                        DialogResult result = MessageBox.Show(ForegroundWindow.Instance,
                            "确实要增大到 " + nCopy.ToString() + " 这么大的值?",
                            "OrderDesignControl",
                            MessageBoxButtons.YesNo,
                            MessageBoxIcon.Question,
                            MessageBoxDefaultButton.Button2);
                        if (result != DialogResult.Yes)
                        {
                            // 2010/12/1 changed
                            // this.comboBox_copy.Text = this.location.ArrivedCount.ToString();    // 恢复原来的值或者最近可用的值
                            this.comboBox_copy.Text = dp2StringUtil.ModifyCopy(
                                this.comboBox_copy.Text, this.location.ArrivedCount.ToString());    // 恢复原来的值或者最近可用的值
                            return;
                        }
                    }


                    // 主动修改location控件的ArrivedCount，需要避免递归处理由此引起的事件
                    this.DisableLocationArrivedChanged++;
                    try
                    {
                        this.location.ArrivedCount = nCopy;
                    }
                    catch (NotEnoughException ex)
                    {
                        MessageBox.Show(this.Container, ex.Message);

                        // 2008/9/16
                        // this.comboBox_copy.Text = this.location.ArrivedCount.ToString(); 
                        // 恢复原来的值或者最近可用的值
                        // 2010/12/1 changed
                        this.comboBox_copy.Text = dp2StringUtil.ModifyCopy(
                            this.comboBox_copy.Text, this.location.ArrivedCount.ToString());    // 恢复原来的值或者最近可用的值
                        return;
                    }
                    finally
                    {
                        this.DisableLocationArrivedChanged--;
                    }

                    // 汇总值发生变化
                    if ((this.State & ItemState.ReadOnly) == 0)
                    {
                        this.Container.DisableNewlyArriveTextChanged++;    // 优化，避免多余的动作
                        this.Container.textBox_newlyArriveTotalCopy.Text = this.Container.GetNewlyArriveTotalCopy().ToString();
                        this.Container.DisableNewlyArriveTextChanged--;
                    }
                    else
                    {
                        this.Container.textBox_arrivedTotalCopy.Text = this.Container.GetArrivedTotalCopy().ToString();
                    }
                }
            }
            catch
            {
                return;
            }

            if ((this.State & ItemState.New) == 0)
                this.State |= ItemState.Changed;

            this.Container.Changed = true;
        }

        void comboBox_seller_DropDown(object sender, EventArgs e)
        {
            // 防止重入 2009/1/15
            if (this.m_nInDropDown > 0)
                return;

            Cursor oldCursor = this.Container.Cursor;
            this.Container.Cursor = Cursors.WaitCursor;
            this.m_nInDropDown++;
            try
            {

                ComboBox combobox = null;

                if (sender is DoubleComboBox)
                    combobox = ((DoubleComboBox)sender).ComboBox;
                else
                    combobox = (ComboBox)sender;

                if (combobox.Items.Count == 0
                    && this.Container.HasGetValueTable() != false)
                {
                    GetValueTableEventArgs e1 = new GetValueTableEventArgs();
                    e1.DbName = this.Container.BiblioDbName;

                    if (combobox == this.comboBox_seller)
                        e1.TableName = "orderSeller";
                    else if (combobox == this.comboBox_class)
                        e1.TableName = "orderClass";
                    else if (combobox == this.comboBox_source.ComboBox)
                        e1.TableName = "orderSource";
                    else if (combobox == this.comboBox_copy.ComboBox)
                        e1.TableName = "orderCopy";
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

        void comboBox_copy_DropDown(object sender, EventArgs e)
        {
            ComboBox combobox = (ComboBox)sender;

            if (combobox.Items.Count == 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    combobox.Items.Add((i + 1).ToString());
                }
            }
        }

        void comboBox_discount_DropDown(object sender, EventArgs e)
        {
            ComboBox combobox = (ComboBox)sender;

            if (combobox.Items.Count == 0)
            {
                combobox.Items.Add("1.00");
                for (int i = 0; i < 9; i++)
                {
                    // TODO: 从 .10 到 .90
                    combobox.Items.Add("." + (i + 1).ToString() + "0");
                }
                combobox.Items.Add("1.00");
            }
        }

        // 正向计算
        void RecomputPrice()
        {
            // 正向计算：码洋 --> 单价
            if (string.IsNullOrEmpty(this.textBox_fixedPrice.Text) == false
                && string.IsNullOrEmpty(this.textBox_fixedPrice.Text) == false
                && OrderDesignControl.IsVirtual(this.textBox_fixedPrice.Text) == false)
            {
                // 注意，无论是在订购模式还是在验收模式，实际上都是修改的 textbox.Text 而不是 OldText
                int nRet = OrderDesignControl.ComputeOrderPriceByFixedPrice(this.textBox_fixedPrice.Text,
this.comboBox_discount.Text,
out string strWishOrderPrice,
out string strError);
                if (nRet == 1)
                {
                    if (this.textBox_price.Text != strWishOrderPrice)
                        this.textBox_price.Text = strWishOrderPrice;
                }
                else
                    Console.Beep();
            }
        }


        // 反向计算：单价 --> 码洋
        void RecomputFixedPrice()
        {
            if (string.IsNullOrEmpty(this.textBox_price.Text) == false && OrderDesignControl.IsVirtual(this.textBox_price.Text) == false
                && string.IsNullOrEmpty(this.comboBox_discount.Text) == false // 折扣不为空才计算
                && (OrderDesignControl.IsVirtual(this.textBox_fixedPrice.Text) == true || string.IsNullOrEmpty(this.textBox_fixedPrice.Text))
                )
            {
                // 注意，无论是在订购模式还是在验收模式，实际上都是修改的 textbox.Text 而不是 OldText
                int nRet = OrderDesignControl.ComputeFixedPriceByOrderPrice(this.textBox_price.Text,
this.comboBox_discount.Text,
out string strResultPrice,
out string strError);
                if (nRet == 1)
                {
                    if (string.IsNullOrEmpty(strResultPrice) == false)
                        strResultPrice = "{" + strResultPrice + "}";
                    if (this.textBox_fixedPrice.Text != strResultPrice)
                        this.textBox_fixedPrice.Text = strResultPrice;
                }
                else
                    Console.Beep();
            }
        }

        // 折扣 文字改变
        void comboBox_discount_TextChanged(object sender, EventArgs e)
        {
            if (this.Container.InInitial == true)
                return;
            if (this.Container.ArriveMode == false)
            {
                // 在订购状态下，新旧值保持统一，以便显示单行
                this.comboBox_discount.OldText = this.comboBox_discount.Text;
            }

            if (OrderDesignControl.IsVirtual(this.textBox_fixedPrice.Text) && string.IsNullOrEmpty(this.textBox_fixedPrice.Text) == false)
                RecomputFixedPrice();
            else
                RecomputPrice();

            if ((this.State & ItemState.New) == 0)
                this.State |= ItemState.Changed;

            this.Container.Changed = true;
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

            //
            menuItem = new MenuItem("复制新增[后](&A)");
            menuItem.Click += new System.EventHandler(this.menu_appendCopyElement_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("新增[前](&I)");
            menuItem.Click += new System.EventHandler(this.menu_insertElement_Click);
            contextMenu.MenuItems.Add(menuItem);

            //
            menuItem = new MenuItem("新增[后](&A)");
            menuItem.Click += new System.EventHandler(this.menu_appendElement_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);

            if (this.Container.ArriveMode == false)
            {
                menuItem = new MenuItem("特殊渠道订购(&S)");
                menuItem.Click += new System.EventHandler(this.menu_specialOrder_Click);
                contextMenu.MenuItems.Add(menuItem);

                // ---
                menuItem = new MenuItem("-");
                contextMenu.MenuItems.Add(menuItem);
            }


            menuItem = new MenuItem("总价(&T)");
            menuItem.Click += new System.EventHandler(this.menu_totalPrice_Click);
            contextMenu.MenuItems.Add(menuItem);

            // ---
            menuItem = new MenuItem("-");
            contextMenu.MenuItems.Add(menuItem);


            //
            menuItem = new MenuItem("删除(&D)");
            menuItem.Click += new System.EventHandler(this.menu_deleteElements_Click);
            contextMenu.MenuItems.Add(menuItem);

            /*
            menuItem = new MenuItem("test");
            menuItem.Click += new System.EventHandler(this.menu_test_Click);
            contextMenu.MenuItems.Add(menuItem);
             * */

            contextMenu.Show(this.label_color, new Point(e.X, e.Y));
        }


        #endregion

        void menu_test_Click(object sender, EventArgs e)
        {
            this.EnsureVisible();
        }

        void menu_totalPrice_Click(object sender, EventArgs e)
        {
            if (String.IsNullOrEmpty(this.StateString) == false)
            {
                MessageBox.Show(this.Container, "对于状态为非空的订购事项，不允许修改其总价格");
                return;
            }

            try
            {
                string strTotalPrice = this.TotalPrice;
                string strNewTotalPrice = InputDlg.GetInput(
                    this.Container,
                    "请输入总价格",
                    "总价格: ",
                    strTotalPrice,
                    this.Container.Font);
                if (strNewTotalPrice == null)
                    return;

                this.TotalPrice = strNewTotalPrice;

                // 如果具有了总价格，则册价格为空
                if (String.IsNullOrEmpty(strNewTotalPrice) == false)
                    this.Price = "";

                // 刷新显示
                this.OtherXml = this.OtherXml;

                if ((this.State & ItemState.New) == 0)
                    this.State |= ItemState.Changed;

                this.Container.Changed = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(this.Container, ex.Message);
            }
        }

        // 特殊渠道订购
        void menu_specialOrder_Click(object sender, EventArgs e)
        {
            List<Item> selected_lines = this.Container.SelectedItems;

            if (selected_lines.Count == 0)
            {
                MessageBox.Show(this.Container, "尚未选定要进行特殊渠道订购的事项");
                return;
            }

            if (selected_lines.Count > 1)
            {
                // 如果要修改的多于一个事项，警告
                DialogResult result = MessageBox.Show(this.Container,
                     "确实要同时编辑 " + selected_lines.Count.ToString() + " 个行的特殊渠道订购特性?",
                     "OrderDesignControl",
                     MessageBoxButtons.OKCancel,
                     MessageBoxIcon.Question,
                     MessageBoxDefaultButton.Button2);
                if (result == DialogResult.Cancel)
                    return;
            }

            SpecialSourceSeriesDialog dlg = new SpecialSourceSeriesDialog();
            GuiUtil.SetControlFont(dlg, this.Container.Font, false);

            dlg.DbName = this.Container.BiblioDbName;
            dlg.Seller = selected_lines[0].Seller;
            dlg.Source = selected_lines[0].Source;
            dlg.AddressXml = selected_lines[0].SellerAddressXml;

            dlg.GetValueTable -= new GetValueTableEventHandler(dlg_GetValueTable);
            dlg.GetValueTable += new GetValueTableEventHandler(dlg_GetValueTable);

            // TODO: 如何保存对话框修改后的大小和位置?
            dlg.StartPosition = FormStartPosition.CenterScreen;
            dlg.ShowDialog(this.Container);

            if (dlg.DialogResult != DialogResult.OK)
                return;

            int nNotChangeCount = 0;
            for (int i = 0; i < selected_lines.Count; i++)
            {
                Item item = selected_lines[i];

                if ((item.State & ItemState.ReadOnly) != 0)
                {
                    nNotChangeCount++;
                    continue;
                }

                item.Source = dlg.Source;
                item.Seller = dlg.Seller;
                item.SellerAddressXml = dlg.AddressXml;
            }

            if (nNotChangeCount > 0)
                MessageBox.Show(this.Container, "有 " + nNotChangeCount.ToString() + " 项只读状态的行没有被修改");
        }

        void dlg_GetValueTable(object sender, GetValueTableEventArgs e)
        {
            this.Container.OnGetValueTable(sender, e);
        }

        void menu_insertElement_Click(object sender, EventArgs e)
        {
            int nPos = this.Container.Items.IndexOf(this);

            if (nPos == -1)
                throw new Exception("not found myself");

            this.Container.InsertNewItem(nPos).EnsureVisible();
        }

        void menu_appendElement_Click(object sender, EventArgs e)
        {
            int nPos = this.Container.Items.IndexOf(this);
            if (nPos == -1)
            {
                throw new Exception("not found myself");
            }

            this.Container.InsertNewItem(nPos + 1).EnsureVisible();
        }

        // 复制新增
        void menu_appendCopyElement_Click(object sender, EventArgs e)
        {
            int nPos = this.Container.Items.IndexOf(this);
            if (nPos == -1)
            {
                throw new Exception("not found myself");
            }

            string strError = "";
            string strXml = "";
            Item source = this.Container.Items[nPos];
            // 获得表示事项全部内容的XML记录
            int nRet = BuildXml(out strXml, out strError);
            if (nRet == -1)
                throw new Exception(strError);

            string strBatchNo = "";
            {
                string strDefaultXml = this.Container.GetDefaultXml();
                if (string.IsNullOrEmpty(strDefaultXml) == false)
                {
                    XmlDocument dom = new XmlDocument();
                    dom.LoadXml(strDefaultXml);
                    strBatchNo = DomUtil.GetElementText(dom.DocumentElement, "batchNo");
                }
            }

            // 修改一些字段
            {
                XmlDocument dom = new XmlDocument();
                dom.LoadXml(strXml);

                // 如果单价和总价都有，则要把总价清空
                string strPrice = DomUtil.GetElementText(dom.DocumentElement, "price");
                string strTotalPrice = DomUtil.GetElementText(dom.DocumentElement, "totalPrice");
                if (string.IsNullOrEmpty(strPrice) == false && string.IsNullOrEmpty(strTotalPrice) == false)
                    DomUtil.SetElementText(dom.DocumentElement, "totalPrice", "");

                // 把 location 中的已验收信息清除
                string strDistributeString = DomUtil.GetElementText(dom.DocumentElement, "distribute");
                if (string.IsNullOrEmpty(strDistributeString) == false)
                {
                    LocationCollection locations = new LocationCollection();
                    nRet = locations.Build(strDistributeString, out strError);
                    if (nRet == -1)
                        throw new Exception("馆藏分配去向字符串 '" + strDistributeString + "' 格式错误: " + strError);
                    strDistributeString = locations.ToString(false);
                    DomUtil.SetElementText(dom.DocumentElement, "distribute", strDistributeString);
                }

                DomUtil.SetElementText(dom.DocumentElement, "state", "");
                DomUtil.SetElementText(dom.DocumentElement, "refID", "");
                DomUtil.SetElementText(dom.DocumentElement, "range", "");
                DomUtil.SetElementText(dom.DocumentElement, "batchNo", strBatchNo);
                strXml = dom.DocumentElement.OuterXml;
            }

            Item target = this.Container.InsertNewItem(nPos + 1, strXml);
#if NO
            target.StateString = "";
            target.RefID = "";
            target.RangeString = "";    // TODO: 可以从上一个事项的时候后面自动计算延展
#endif
            target.EnsureVisible();
        }

        // 删除当前元素
        void menu_deleteElements_Click(object sender, EventArgs e)
        {
            List<Item> selected_lines = this.Container.SelectedItems;

            if (selected_lines.Count == 0)
            {
                MessageBox.Show(this.Container, "尚未选定要删除的事项");
                return;
            }
            string strText = "";

            if (selected_lines.Count == 1)
                strText = "确实要删除事项 '" + selected_lines[0].ItemCaption + "'? ";
            else
                strText = "确实要删除所选定的 " + selected_lines.Count.ToString() + " 个事项?";

            DialogResult result = MessageBox.Show(this.Container,
                strText,
                "OrderCrossControl",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (result != DialogResult.Yes)
            {
                return;
            }

            int nNotDeleteCount = 0;
            this.Container.DisableUpdate();
            try
            {
                for (int i = 0; i < selected_lines.Count; i++)
                {
                    Item item = selected_lines[i];
                    if ((item.State & ItemState.ReadOnly) != 0)
                    {
                        nNotDeleteCount++;
                        continue;
                    }
                    this.Container.RemoveItem(item);
                }
            }
            finally
            {
                this.Container.EnableUpdate();
            }

            if (nNotDeleteCount > 0)
                MessageBox.Show(this.Container, "有 " + nNotDeleteCount.ToString() + " 项已订购状态的事项未能删除");
        }

        void CopyTo(Item item)
        {
            item.CatalogNo = this.CatalogNo;
            item.Seller = this.Seller;
            item.Source = this.Source;
            item.RangeString = this.RangeString;
            item.IssueCountString = this.IssueCountString;
            item.CopyString = this.CopyString;

            item.OldFixedPrice = this.OldFixedPrice;
            item.FixedPrice = this.FixedPrice;

            item.Discount = this.Discount;

            item.OldPrice = this.OldPrice;
            item.Price = this.Price;

            item.Distribute = this.Distribute;
            item.SellerAddressXml = this.SellerAddressXml;
            item.Class = this.Class;
            // StateString 不要复制
            // item.TotalPrice = this.TotalPrice;
        }

        #region 外部需要使用的属性

        // 书目号
        public string CatalogNo
        {
            get
            {
                return this.textBox_catalogNo.Text;
            }
            set
            {
                this.textBox_catalogNo.Text = value;
            }
        }

        // 渠道
        public string Seller
        {
            get
            {
                return this.comboBox_seller.Text;
            }
            set
            {
                this.comboBox_seller.Text = value;
            }
        }

        // 经费来源
        public string Source
        {
            get
            {
                return this.comboBox_source.Text;
            }
            set
            {
                this.comboBox_source.Text = value;
            }
        }

        // 原有的经费来源
        public string OldSource
        {
            get
            {
                return this.comboBox_source.OldText;
            }
            set
            {
                this.comboBox_source.OldText = value;
            }
        }

        // 日期范围
        // Exception: set的时候可能会抛出异常
        public string RangeString
        {
            get
            {
                return this.dateRange_range.Text;
            }
            set
            {
                // 可能会抛出异常
                this.dateRange_range.Text = value;
            }
        }

        // 检测一个出版时间是否处在RangeString的时间范围内?
        // Exception: 有可能抛出异常
        // parameters:
        //      strPublishTime  4/6/8字符
        //      strRange    格式为"20080101-20081231"
        public bool InRange(string strPublishTime)
        {
            try
            {
                if (strPublishTime.Length == 4)
                    strPublishTime += "0101";
                else if (strPublishTime.Length == 6)
                    strPublishTime += "01";

                string strRange = this.RangeString;

                if (string.IsNullOrEmpty(strRange) == true)
                    return false;

                int nRet = strRange.IndexOf("-");

                string strStart = strRange.Substring(0, nRet).Trim();
                string strEnd = strRange.Substring(nRet + 1).Trim();

                if (strStart.Length != 8)
                    throw new Exception("时间范围字符串 '" + strRange + "' 的左边部分 '" + strStart + "' 格式错误，应为8字符");

                if (strEnd.Length != 8)
                    throw new Exception("时间范围字符串 '" + strRange + "' 的右边部分 '" + strEnd + "' 格式错误，应为8字符");

                if (String.Compare(strPublishTime, strStart) < 0)
                    return false;

                if (String.Compare(strPublishTime, strEnd) > 0)
                    return false;

                return true;
            }
            catch
            {
                return false;
            }
        }

        // 期数整数
        public int IssueCountValue
        {
            get
            {
                try
                {
                    return Convert.ToInt32(this.comboBox_issueCount.Text);
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                EnsureListValue(this.comboBox_issueCount, value.ToString());
                this.comboBox_issueCount.Text = value.ToString();
            }
        }

        // 期数字符串
        public string IssueCountString
        {
            get
            {
                return this.comboBox_issueCount.Text;
            }
            set
            {
                EnsureListValue(this.comboBox_issueCount, value);
                this.comboBox_issueCount.Text = value;
            }
        }

        // 确保 combobox 的列表里面有特定的值
        static void EnsureListValue(ComboBox combobox, string strValue)
        {
            List<string> values = new List<string>();
            foreach (string s in combobox.Items)
            {
                if (s == strValue)
                    return;
                values.Add(s);
            }

            values.Add(strValue);

            values.Sort((x, y) =>
            {
                return StringUtil.RightAlignCompare(x, y);
            });

            combobox.Items.Clear();
            foreach (string s in values)
            {
                combobox.Items.Add(s);
            }
        }

        // 复本数整数
        public int CopyValue
        {
            get
            {
                try
                {
                    // 2010/12/1 changed
                    return Convert.ToInt32(dp2StringUtil.GetCopyFromCopyString(this.comboBox_copy.Text));
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                // 2010/12/1 changed
                // this.comboBox_copy.Text = value.ToString();
                this.comboBox_copy.Text = dp2StringUtil.ModifyCopy(this.comboBox_copy.Text, value.ToString());
            }
        }

        // 复本数字符串
        public string CopyString
        {
            get
            {
                return this.comboBox_copy.Text;
            }
            set
            {
                this.comboBox_copy.Text = value;
            }
        }

        // 原有的 复本数整数
        // 2008/9/12
        public int OldCopyValue
        {
            get
            {
                try
                {
                    // 2010/12/1 changed
                    return Convert.ToInt32(dp2StringUtil.GetCopyFromCopyString(this.comboBox_copy.OldText));
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                // 2010/12/1 changed
                // this.comboBox_copy.OldText = value.ToString();
                this.comboBox_copy.OldText = dp2StringUtil.ModifyCopy(this.comboBox_copy.OldText, value.ToString());
            }
        }

        // 原有的 复本数字符串
        public string OldCopyString
        {
            get
            {
                return this.comboBox_copy.OldText;
            }
            set
            {
                this.comboBox_copy.OldText = value;
            }
        }

        // 码洋
        public string FixedPrice
        {
            get
            {
                return this.textBox_fixedPrice.Text;
            }
            set
            {
                this.textBox_fixedPrice.Text = value;
            }
        }


        // 原有的 码洋
        public string OldFixedPrice
        {
            get
            {
                return this.textBox_fixedPrice.OldText;
            }
            set
            {
                this.textBox_fixedPrice.OldText = value;
            }
        }

        // 折扣字符串
        public string Discount
        {
            get
            {
                return this.comboBox_discount.Text;
            }
            set
            {
                this.comboBox_discount.Text = value;
            }
        }

        // 原有的 折扣字符串
        public string OldDiscount
        {
            get
            {
                return this.comboBox_discount.OldText;
            }
            set
            {
                this.comboBox_discount.OldText = value;
            }
        }

        // 原有的 折扣小数
        public decimal OldDiscountValue
        {
            get
            {
                try
                {
                    return Convert.ToDecimal(dp2StringUtil.GetCopyFromCopyString(this.comboBox_discount.OldText));
                }
                catch
                {
                    return 0;
                }
            }
            set
            {
                this.comboBox_discount.OldText = dp2StringUtil.ModifyCopy(this.comboBox_discount.OldText, value.ToString());
            }
        }

        // 单价
        public string Price
        {
            get
            {
                return this.textBox_price.Text;
            }
            set
            {
                this.textBox_price.Text = value;
            }
        }

        // 原有的 单价
        public string OldPrice
        {
            get
            {
                return this.textBox_price.OldText;
            }
            set
            {
                this.textBox_price.OldText = value;
            }
        }

        // 馆藏地点事项的个数 2008/9/12
        public int DistributeCount
        {
            get
            {
                return this.location.Count;
            }
            set
            {
                this.location.Count = value;
            }
        }

        // 去向，馆藏分配策略
        public string Distribute
        {
            get
            {
                return this.location.Value;
            }
            set
            {
                // 馆藏分配字符串的修改会导致其count改变，进而会影响到同一事项的Copy值
                this.location.Value = value;
            }
        }

        public string OtherXml
        {
            get
            {
                return this.m_otherXml;
            }
            set
            {
                this.m_otherXml = value;
                string strError = "";
                int nRet = DisplayOtherXml(value,
                    out strError);
                if (nRet == -1)
                    throw new Exception(strError);
            }
        }

        public string SellerAddressXml
        {
            get
            {
                return this.m_sellerAddressXml;
            }
            set
            {
                this.m_sellerAddressXml = value;
                string strError = "";
                int nRet = DisplaySellerAddressXml(value,
                    out strError);
                if (nRet == -1)
                    throw new Exception(strError);
            }
        }

        // 类别
        public string Class
        {
            get
            {
                return this.comboBox_class.Text;
            }
            set
            {
                this.comboBox_class.Text = value;
            }
        }

        public string Index
        {
            get
            {
                if (String.IsNullOrEmpty(this.OtherXml) == true)
                    return "";

                string strError = "";

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(this.OtherXml);
                }
                catch (Exception ex)
                {
                    strError = "XML记录装入DOM时出错: " + ex.Message;
                    throw new Exception(strError);
                }

                // 提取所需的内容，构成文本显示
                return DomUtil.GetElementText(dom.DocumentElement,
                    "index");
            }
            set
            {
                if (String.IsNullOrEmpty(this.OtherXml) == true)
                    this.OtherXml = "<root />";

                string strError = "";

                XmlDocument dom = new XmlDocument();
                try
                {
                    dom.LoadXml(this.OtherXml);
                }
                catch (Exception ex)
                {
                    strError = "XML记录装入DOM时出错: " + ex.Message;
                    throw new Exception(strError);
                }

                // 提取所需的内容，构成文本显示
                DomUtil.SetElementText(dom.DocumentElement,
                    "index", value);

                this.OtherXml = dom.OuterXml;
                // 会自动刷新显示
            }
        }

#if NO
        // 2008/11/12
        // string m_strStateString = "";

        public string StateString
        {
            get
            {
                return m_strStateString;
            }
            set
            {
                m_strStateString = value;
            }
        }
#endif
        string GetFieldValue(string strElementName)
        {
            XmlDocument dom = new XmlDocument();
            if (String.IsNullOrEmpty(this.m_otherXml) == false)
            {
                try
                {
                    dom.LoadXml(this.m_otherXml);
                }
                catch (Exception ex)
                {
                    throw new Exception("load other xml error: " + ex.Message);
                }
                return DomUtil.GetElementText(dom.DocumentElement,
                    strElementName);
            }
            else
                return "";
        }

        void SetFieldValue(string strElementName, string strValue)
        {
            XmlDocument dom = new XmlDocument();
            if (String.IsNullOrEmpty(this.m_otherXml) == false)
            {
                try
                {
                    dom.LoadXml(this.m_otherXml);
                }
                catch (Exception ex)
                {
                    throw new Exception("load other xml error: " + ex.Message);
                }
            }
            else
                dom.LoadXml("<root />");

            DomUtil.SetElementText(dom.DocumentElement,
                strElementName, strValue);

            this.m_otherXml = dom.DocumentElement.OuterXml;
        }

        // 可能会抛出异常
        public string StateString
        {
            get
            {
                return this.GetFieldValue("state");
            }
            set
            {
                this.SetFieldValue("state", value);
            }
        }


        int DisplaySellerAddressXml(string strXml,
            out string strError)
        {
            strError = "";

            if (String.IsNullOrEmpty(strXml) == true)
            {
                this.label_sellerAddress.Text = "";
                return 0;
            }

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML记录装入DOM时出错: " + ex.Message;
                return -1;
            }

            if (dom.DocumentElement == null
                || dom.DocumentElement.ChildNodes.Count == 0)
            {
                this.label_sellerAddress.Text = "";
                return 0;
            }

            // 提取所需的内容，构成文本显示
            string strZipcode = DomUtil.GetElementText(dom.DocumentElement,
                "zipcode");
            string strAddress = DomUtil.GetElementText(dom.DocumentElement,
                "address");
            string strDepartment = DomUtil.GetElementText(dom.DocumentElement,
                "department");
            string strName = DomUtil.GetElementText(dom.DocumentElement,
                "name");
            string strTel = DomUtil.GetElementText(dom.DocumentElement,
                "tel");
            string strEmail = DomUtil.GetElementText(dom.DocumentElement,
                "email");
            string strBank = DomUtil.GetElementText(dom.DocumentElement,
                "bank");
            string strAccounts = DomUtil.GetElementText(dom.DocumentElement,
                "accounts");
            string strPayStyle = DomUtil.GetElementText(dom.DocumentElement,
                "payStyle");
            string strComment = DomUtil.GetElementText(dom.DocumentElement,
                "comment");

            this.label_sellerAddress.Text = "邮政编码: \t" + strZipcode + "\r\n"
            + "地址: \t" + strAddress + "\r\n"
            + "单位名: \t" + strDepartment + "\r\n"
            + "联系人: \t" + strName + "\r\n"
            + "电话: \t" + strTel + "\r\n"
            + "Email: \t" + strEmail + "\r\n"
            + "开户行: \t" + strBank + "\r\n"
            + "银行账号: \t" + strAccounts + "\r\n"
            + "汇款方式: \t" + strPayStyle + "\r\n"
            + "注释: \t" + strComment + "\r\n";

            return 0;
        }

        int DisplayOtherXml(string strXml,
            out string strError)
        {
            strError = "";

            XmlDocument dom = new XmlDocument();
            try
            {
                dom.LoadXml(strXml);
            }
            catch (Exception ex)
            {
                strError = "XML记录装入DOM时出错: " + ex.Message;
                return -1;
            }

            // 提取所需的内容，构成文本显示
            string strIndex = DomUtil.GetElementText(dom.DocumentElement,
                "index");
            string strState = DomUtil.GetElementText(dom.DocumentElement,
                "state");

            ////m_strStateString = strState;

            string strRange = DomUtil.GetElementText(dom.DocumentElement,
                "range");
            string strOrderTime = DateTimeUtil.LocalTime(DomUtil.GetElementText(dom.DocumentElement,
                "orderTime"));   // 2008/12/17 changed
            string strOrderID = DomUtil.GetElementText(dom.DocumentElement,
                "orderID");
            string strComment = DomUtil.GetElementText(dom.DocumentElement,
                "comment");
            string strBatchNo = DomUtil.GetElementText(dom.DocumentElement,
                "batchNo");
            /*
            string strIssueCount = DomUtil.GetElementText(dom.DocumentElement,
                "issueCount");
             * */
            string strTotalPrice = DomUtil.GetElementText(dom.DocumentElement,
                "totalPrice");

            this.label_other.Text = "编号: \t" + strIndex + "\r\n"
            + "状态: \t" + strState + "\r\n"
            // + "时间范围:\t" + strRange + "\r\n"
            + "订购时间: \t" + strOrderTime + "\r\n"
            + "订单号: \t" + strOrderID + "\r\n"
            + "总价格: \t" + strTotalPrice + "\r\n"
            + "注释: \t" + strComment + "\r\n"
            + "批次号: \t" + strBatchNo + "\r\n";

            return 0;
        }

        // 可能会抛出异常
        public string TotalPrice
        {
            get
            {
#if NO
                XmlDocument dom = new XmlDocument();
                if (String.IsNullOrEmpty(this.m_otherXml) == false)
                {
                    try
                    {
                        dom.LoadXml(this.m_otherXml);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("load other xml error: " + ex.Message);
                    }
                    return DomUtil.GetElementText(dom.DocumentElement,
                        "totalPrice");
                }
                else
                    return "";
#endif
                return this.GetFieldValue("totalPrice");
            }
            set
            {
#if NO
                XmlDocument dom = new XmlDocument();
                if (String.IsNullOrEmpty(this.m_otherXml) == false)
                {
                    try
                    {
                        dom.LoadXml(this.m_otherXml);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("load other xml error: " + ex.Message);
                    }
                }
                else
                    dom.LoadXml("<root />");

                DomUtil.SetElementText(dom.DocumentElement,
                    "totalPrice", value);

                this.m_otherXml = dom.DocumentElement.OuterXml;
#endif
                this.SetFieldValue("totalPrice", value);
            }
        }

        // 可能会抛出异常
        public string RefID
        {
            get
            {
#if NO
                XmlDocument dom = new XmlDocument();
                if (String.IsNullOrEmpty(this.m_otherXml) == false)
                {
                    try
                    {
                        dom.LoadXml(this.m_otherXml);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("load other xml error: " + ex.Message);
                    }
                    return DomUtil.GetElementText(dom.DocumentElement,
                        "refID");
                }
                else
                    return "";
#endif
                return this.GetFieldValue("refID");
            }
            set
            {
#if NO
                XmlDocument dom = new XmlDocument();
                if (String.IsNullOrEmpty(this.m_otherXml) == false)
                {
                    try
                    {
                        dom.LoadXml(this.m_otherXml);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("load other xml error: " + ex.Message);
                    }
                }
                else
                    dom.LoadXml("<root />");

                DomUtil.SetElementText(dom.DocumentElement,
                    "refID", value);

                this.m_otherXml = dom.DocumentElement.OuterXml;
#endif
                this.SetFieldValue("refID", value);
            }
        }

        // 获得表示事项全部内容的XML记录
        public int BuildXml(out string strXml,
            out string strError)
        {
            strError = "";
            strXml = "";

            XmlDocument dom = new XmlDocument();
            if (String.IsNullOrEmpty(this.m_otherXml) == false)
            {
                try
                {
                    dom.LoadXml(this.m_otherXml);
                }
                catch (Exception ex)
                {
                    strError = "load other xml error: " + ex.Message;
                    return -1;
                }
            }
            else
                dom.LoadXml("<root />");

            /*
             * other xml中已经包含下列内容:
                "index"
                "state"
                "range"
                "orderTime"
                "orderID"
                "comment"
                "batchNo"
                "totalPrice" 
             * */

            DomUtil.SetElementText(dom.DocumentElement,
                "catalogNo", this.CatalogNo);
            DomUtil.SetElementText(dom.DocumentElement,
                "seller", this.Seller);
            DomUtil.SetElementText(dom.DocumentElement,
                "source", dp2StringUtil.LinkOldNewValue(this.OldSource, this.Source));
            DomUtil.SetElementText(dom.DocumentElement,
                "range", this.RangeString);
            DomUtil.SetElementText(dom.DocumentElement,
                "issueCount", this.IssueCountString);
            DomUtil.SetElementText(dom.DocumentElement,
                "copy", dp2StringUtil.LinkOldNewValue(this.OldCopyString, this.CopyString));
            DomUtil.SetElementText(dom.DocumentElement,
    "fixedPrice", dp2StringUtil.LinkOldNewValue(this.OldFixedPrice, this.FixedPrice));
            DomUtil.SetElementText(dom.DocumentElement,
    "discount", dp2StringUtil.LinkOldNewValue(this.OldDiscount, this.Discount));


            DomUtil.SetElementText(dom.DocumentElement,
                "price", dp2StringUtil.LinkOldNewValue(this.OldPrice, this.Price));
            DomUtil.SetElementText(dom.DocumentElement,
                "distribute", this.Distribute);
            DomUtil.SetElementText(dom.DocumentElement,
                "class", this.Class);

            strXml = dom.OuterXml;
            return 0;
        }

#if NO
        // 将RFC1123时间字符串转换为本地一般时间字符串
        // exception: 不会抛出异常
        public static string LocalTime(string strRfc1123Time)
        {
            try
            {
                if (String.IsNullOrEmpty(strRfc1123Time) == true)
                    return "";
                return Rfc1123DateTimeStringToLocal(strRfc1123Time, "G");
            }
            catch // (Exception ex)    // 2008/10/28
            {
                return "时间字符串 '" + strRfc1123Time + "' 格式错误，不是合法的RFC1123格式";
            }
        }
#endif

        // 事项标题
        public string ItemCaption
        {
            get
            {
                // 归纳汇总一个行的特征
                return this.comboBox_seller.Text + ":" + this.comboBox_source.Text + ":" + this.comboBox_copy.Text;
            }
        }

        // 本次新验收的数量
        public int NewlyAcceptedCount
        {
            get
            {
                if (this.location == null)
                    return 0;

                return this.location.ArrivedCount - this.location.ReadOnlyArrivedCount;
            }
        }

        #endregion
    }


    /// <summary>
    /// 获得缺省记录
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void GetDefaultRecordEventHandler(object sender,
    GetDefaultRecordEventArgs e);

    /// <summary>
    /// 获得缺省记录的参数
    /// </summary>
    public class GetDefaultRecordEventArgs : EventArgs
    {
        public string Xml = ""; // 缺省记录
    }

    public class MyTableLayoutPanel : TableLayoutPanel
    {
        public bool DisableUpdate = false;

        /// <summary>
        /// 缺省窗口过程
        /// </summary>
        /// <param name="m">消息</param>
        protected override void DefWndProc(ref Message m)
        {
            if (this.DisableUpdate == true)
            {
                if ((int)API.WM_ERASEBKGND == m.Msg)
                    m.Msg = (int)API.WM_NULL;
                else if ((int)API.WM_PAINT == m.Msg)
                    m.Msg = (int)API.WM_NULL;
                return;
            }
            base.DefWndProc(ref m);
        }
    }

    // 检查馆代码是否在管辖范围内
    /// <summary>
    /// 检查馆代码是否在管辖范围内 事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">事件参数</param>
    public delegate void VerifyLibraryCodeEventHandler(object sender,
VerifyLibraryCodeEventArgs e);

    /// <summary>
    /// 检查馆代码是否在管辖范围内事件的参数
    /// </summary>
    public class VerifyLibraryCodeEventArgs : EventArgs
    {
        /// <summary>
        /// [in] 待检查的馆代码。可能是一个字符串列表形态
        /// </summary>
        public string LibraryCode = ""; // [in]待检查的馆代码。可能是一个字符串列表形态
        /// <summary>
        /// [out] 检查结果。非空表示检查发现了问题
        /// </summary>
        public string ErrorInfo = "";   // [out]检查结果。非空表示检查发现了问题
    }


}
